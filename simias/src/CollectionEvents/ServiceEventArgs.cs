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
	/// Service Events.
	/// </summary>
	public enum ServiceControl
	{
		/// <summary>
		/// The service should shutdown.
		/// </summary>
		Shutdown = 1,
		/// <summary>
		/// The service should reconfigure.
		/// </summary>
		Reconfigure = 2
	};

	/// <summary>
	/// The event arguments for a Collection event.
	/// </summary>
	[Serializable]
	public class ServiceEventArgs : CollectionEventArgs
	{
		/// <summary>
		/// Used to target all services.
		/// </summary>
		public static int	TargetAll = 0;
		int					target;
		ServiceControl		controlEvent;
		string				userName;
		
		/// <summary>
		/// Constructs a ServiceEventArgs to describe the event.
		/// </summary>
		/// <param name="target">The Process ID of the service to signal.</param>
		/// <param name="controlEvent">The event to execute.</param>
		public ServiceEventArgs(int target, ServiceControl controlEvent) :
			base(EventType.ServiceControl)
		{
			this.target = target;
			this.controlEvent = controlEvent;
			this.userName = System.Environment.UserName;
		}

		/// <summary>
		/// Gets the target process ID.
		/// </summary>
		public int Target
		{
			get {return target;}
		}

		/// <summary>
		/// Gets the event type.
		/// </summary>
		public ServiceControl ControlEvent
		{
			get {return controlEvent;}
		}
	}
}
