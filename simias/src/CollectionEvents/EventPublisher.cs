/***********************************************************************
 *  EventPublisher.cs - An event publisher class.
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young <ryoung@novell.com>
 * 
 ***********************************************************************/
using System;
using System.Diagnostics;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Simias
{
	/// <summary>
	/// Class used to publish events.
	/// </summary>
	public class EventPublisher
	{
		EventBroker broker;
		public static int TargetAll = 0;
	
		#region Constructor

		/// <summary>
		/// Creates an Event Publisher.
		/// </summary>
		public EventPublisher()
		{
			EventBroker.RegisterClientChannel();

			broker = new EventBroker();
		}

		#endregion

		#region Publish Calls.

		/// <summary>
		/// Called to publish a Change event.
		/// </summary>
		/// <param name="args"></param>
		public void FireChanged(EventArgs args)
		{
			if (broker != null)
				broker.FireChanged(args);
		}

		/// <summary>
		/// Called to publish a Create event.
		/// </summary>
		/// <param name="args"></param>
		public void FireCreated(EventArgs args)
		{
			if (broker != null)
				broker.FireCreated(args);
		}

		/// <summary>
		/// Called to publish a Delete event.
		/// </summary>
		/// <param name="args"></param>
		public void FireDeleted(EventArgs args)
		{
			if (broker != null)
				broker.FireDeleted(args);
		}

		/// <summary>
		/// Called to publish a Rename event.
		/// </summary>
		/// <param name="args">The arguments of the event.</param>
		public void FireRenamed(EventArgs args)
		{
			if (broker != null)
				broker.FireRenamed(args);
		}

		/// <summary>
		/// Called to publish an event to listening services.
		/// </summary>
		/// <param name="targetProcess">The process Id of the process to notify.</param>
		/// <param name="t"></param>
		public void FireServiceControl(int targetProcess, ServiceEventType t)
		{
			if (broker != null)
				broker.FireServiceControl(targetProcess, t);
		}

		#endregion
	}
}
