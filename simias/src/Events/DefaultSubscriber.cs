/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Threading;

namespace Simias.Event
{
	/// <summary>
	/// Summary description for DefaultSubscriber.
	/// </summary>
	public class DefaultSubscriber : MarshalByRefObject, ISubscriber, IDisposable
	{
		#region fields

		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(DefaultSubscriber));
		// This is a singleton per Store.
		static Hashtable	instanceTable = new Hashtable();
		int					count;
		Configuration		conf;
		EventBroker			broker = null;
		bool				alreadyDisposed;

		#endregion

		#region Events

		/// <summary>
		/// Delegate to handle Collection Events.
		/// </summary>
		public event CollectionEventHandler CollectionEvent;
		
		#endregion

		#region Factory Methods

		internal static DefaultSubscriber GetDefaultSubscriber(Configuration conf)
		{
			DefaultSubscriber instance;
			lock (typeof(DefaultSubscriber))
			{
				if (!instanceTable.Contains(conf.StorePath))
				{
					instance = new DefaultSubscriber(conf);
					instanceTable.Add(conf.StorePath, instance);
				}
				else
				{
					instance = (DefaultSubscriber)instanceTable[conf.StorePath];
				}
				++instance.count;
			}
			return instance;
		}

		
		#endregion

		#region Constructor / Finalizer

		DefaultSubscriber(Configuration conf)
		{
			alreadyDisposed = false;
			this.conf = conf;
			count = 0;
			if (!setupBroker())
			{
				// Start a thread to connect when the broker is availablehandle.
				System.Threading.Thread t = new Thread(new ThreadStart(EventThread));
				t.IsBackground = true;
				t.Start();
			}
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~DefaultSubscriber()
		{
			Dispose(true);
		}

		bool setupBroker()
		{
			bool status = false;
			broker = EventBroker.GetBroker(conf);
			if (broker != null)
			{
				try
				{
					broker.AddSubscriber(this);
					status = true;
				}
				catch
				{
					broker = null;
				}
			}
			return status;
		}

		#endregion

		#region Callbacks

		/// <summary>
		/// Callback used by the EventBroker for Collection events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		public void OnCollectionEvent(SimiasEventArgs args)
		{
			callDelegate(args);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		public void RecieveEvent(SimiasEventArgs args)
		{
			OnCollectionEvent(args);
		}

		
		#endregion

		#region Queue and Thread Methods.

		private void EventThread()
		{
			while (!alreadyDisposed && broker == null)
			{
				if (EventBroker.RegisterClientChannel(conf))
				{
					if (setupBroker())
					{
						break;
					}
				}
				Thread.Sleep(1000);
			}
		
			logger.Info("Connected to event broker");
		}
		
		#endregion

		#region Delegate Invokers

		void callDelegate(SimiasEventArgs args)
		{
			if (CollectionEvent != null)
			{
				Delegate[] cbList = CollectionEvent.GetInvocationList();
				foreach (CollectionEventHandler cb in cbList)
				{
					try 
					{ 
						cb(args);
					}
					catch (Exception ex)
					{
						logger.Debug(ex, "Delegate {0}.{1} failed", cb.Target, cb.Method);
					}
				}
			}
		}
		
		#endregion

		#region private Dispose

		private void Dispose(bool inFinalize)
		{
			try 
			{
				if (!alreadyDisposed)
				{
					lock (typeof(DefaultSubscriber))
					{
						--count;
						if (count == 0)
						{
							instanceTable.Remove(conf);
							alreadyDisposed = true;
							
							// Signal thread so it can exit.
							if (!inFinalize)
							{
								GC.SuppressFinalize(this);
							}
						}
					}
				}
			}
			catch {};
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Dispose method.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		#endregion

		#region MarshallByRef Overrides

		/// <summary>
		/// This client will listen until it deregisteres.
		/// </summary>
		/// <returns>null (Do not expire object).</returns>
		public override Object InitializeLifetimeService()
		{
			return null;
		}

		#endregion

	}
}
