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
	public class DefaultSubscriber : MarshalByRefObject, IDisposable
	{
		#region fields

		// This is a singleton per Store.
		static Hashtable	instanceTable = new Hashtable();
		int					count;
		Configuration		conf;
		Queue				eventQueue;
		ManualResetEvent	queued;
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
			//if (EventBroker.RegisterClientChannel(conf))
			setupBroker();
			// Start a thread to handle events.
			eventQueue = new Queue();
			queued = new ManualResetEvent(false);
			System.Threading.Thread t = new Thread(new ThreadStart(EventThread));
			t.IsBackground = true;
			t.Start();
		}

		~DefaultSubscriber()
		{
			Dispose(true);
		}

		void setupBroker()
		{
			broker = EventBroker.GetBroker(conf);
			broker.CollectionEvent += new CollectionEventHandlerS(OnCollectionEventS);
		}

		#endregion

		#region Callbacks

		/// <summary>
		/// Callback used by the EventBroker for Collection events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		public void OnCollectionEvent(CollectionEventArgs args)
		{
			queueEvent(args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Collection events.
		/// </summary>
		/// <param name="changeType">The type of event.</param>
		/// <param name="args">Arguments for the event.</param>
		public void OnCollectionEventS(EventType changeType, string args)
		{
			CollectionEventArgs eArgs = null;
			switch (changeType)
			{
				case EventType.CollectionRootChanged:
					eArgs = new CollectionRootChangedEventArgs(args);
					break;
				case EventType.FileChanged:
				case EventType.FileCreated:
				case EventType.FileDeleted:
					eArgs = new FileEventArgs(args);
					break;
				case EventType.FileRenamed:
					eArgs = new FileRenameEventArgs(args);
					break;
				case EventType.NodeChanged:
				case EventType.NodeCreated:
				case EventType.NodeDeleted:
					eArgs = new NodeEventArgs(args);
					break;
			}
			if (eArgs != null)
				queueEvent(eArgs);
		}

		#endregion

		#region Queue and Thread Methods.

		private void queueEvent(CollectionEventArgs args)
		{
			lock (eventQueue)
			{
				eventQueue.Enqueue(args);
				queued.Set();
			}
		}

		private void EventThread()
		{
			while (!alreadyDisposed && broker == null)
			{
				if (EventBroker.RegisterClientChannel(conf))
				{
					setupBroker();
					break;
				}
				Thread.Sleep(100);
			}
			while (!alreadyDisposed)
			{
				try
				{
					queued.WaitOne();
					CollectionEventArgs args = null;
					lock (eventQueue)
					{
						if (eventQueue.Count > 0)
						{
							args = (CollectionEventArgs)eventQueue.Dequeue();
							callDelegate(args);
						}
						else
						{
							queued.Reset();
							continue;
						}
					}
				}
				catch {}
			}
		}

		#endregion

		#region Delegate Invokers

		void callDelegate(CollectionEventArgs args)
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
					catch 
					{
						// Remove the offending delegate.
						CollectionEvent -= cb;
						MyTrace.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
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
							broker.CollectionEvent -= new CollectionEventHandlerS(OnCollectionEventS);
							
							// Signal thread so it can exit.
							queued.Set();
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

		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}
}
