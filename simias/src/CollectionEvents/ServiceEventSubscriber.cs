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

namespace Simias.Event
{
	/// <summary>
	/// Class to listen to Service events.
	/// </summary>
	public class ServiceEventSubscriber : MarshalByRefObject, IDisposable
	{
		#region Events
		/// <summary>
		/// Delegate used to control services in the system.
		/// </summary>
		public event ServiceEventHandler ServiceControl;
		#endregion

		#region Private Fields
		EventBroker broker = null;
		string		userName;
		bool		enabled;
		bool		alreadyDisposed;
		
		#endregion

		#region Constructor/Finalizer

		/// <summary>
		/// Creates a Subscriber to listen for service events.
		/// </summary>
		/// <param name="conf">Configuration Object.</param>
		public ServiceEventSubscriber(Configuration conf)
		{
			userName = System.Environment.UserName;
			enabled = true;
			alreadyDisposed = false;
			if (EventBroker.RegisterClientChannel(conf))
			{
				broker = new EventBroker();
				broker.ServiceEvent += new ServiceEventHandler(OnServiceControl);
			}
		}

		
		/// <summary>
		/// Finalizer.
		/// </summary>
		~ServiceEventSubscriber()
		{
			Dispose(true);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets and set the enabled state.
		/// </summary>
		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				enabled = value;
			}
		}

		/// <summary>
		/// Gets the User That this service listener is running as.
		/// </summary>
		public string UserName
		{
			get {return userName;}
		}
		#endregion

		#region Callbacks

		/// <summary>
		/// Callback used to control services in the Simias System.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		//[OneWay]
		public void OnServiceControl(ServiceEventArgs args)
		{
			if (ServiceControl != null)
				ServiceControl(args);
		}

		#endregion

		#region Private Methods
		
		private void Dispose(bool inFinalize)
		{
			try 
			{
				if (!alreadyDisposed)
				{
					alreadyDisposed = true;
					broker.ServiceEvent -= new ServiceEventHandler(OnServiceControl);
					if (!inFinalize)
					{
						GC.SuppressFinalize(this);
					}
				}
			}
			catch {};
		}

		#endregion

		#region MarshalByRefObject overrides

		/// <summary>
		/// This object should not time out.
		/// </summary>
		/// <returns></returns>
		public override Object InitializeLifetimeService()
		{
			return null;
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Called to cleanup any resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}
}
