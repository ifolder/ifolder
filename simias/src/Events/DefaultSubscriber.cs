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

using Simias.Client.Event;

namespace Simias.Event
{
	/// <summary>
	/// Summary description for DefaultSubscriber.
	/// </summary>
	public class DefaultSubscriber : IDisposable
	{
		#region fields

		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(DefaultSubscriber));
		// This is a singleton per Store.
		EventBroker			broker = null;
		bool				alreadyDisposed;

		#endregion

		#region Events

		/// <summary>
		/// Delegate to handle Collection Events.
		/// </summary>
		public event SimiasEventHandler SimiasEvent;
		
		#endregion

		#region Constructor / Finalizer

		/// <summary>
		/// Constructor
		/// </summary>
		public DefaultSubscriber()
		{
			alreadyDisposed = false;
			broker = EventBroker.GetBroker();
			broker.SimiasEvent += new SimiasEventHandler(OnSimiasEvent);
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~DefaultSubscriber()
		{
			Dispose(true);
		}

		#endregion

		#region Callbacks

		/// <summary>
		/// Called when an event has been triggered.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		public void OnSimiasEvent(SimiasEventArgs args)
		{
			callDelegate(args);
		}

		#endregion

		#region Delegate Invokers

		void callDelegate(SimiasEventArgs args)
		{
			if (SimiasEvent != null)
			{
				Delegate[] cbList = SimiasEvent.GetInvocationList();
				foreach (SimiasEventHandler cb in cbList)
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
				lock (this)
				{
					if (!alreadyDisposed)
					{
						alreadyDisposed = true;
						if (!inFinalize)
						{
							GC.SuppressFinalize(this);
						}
						broker.SimiasEvent -= new SimiasEventHandler(OnSimiasEvent);
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

	}
}
