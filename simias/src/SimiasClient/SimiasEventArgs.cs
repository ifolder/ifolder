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
using System.Text;

namespace Simias.Client.Event
{
	/// <summary>
	/// The event arguments for a Collection event.
	/// </summary>
	[Serializable]
	public abstract class SimiasEventArgs : EventArgs
	{
		string					eventData;
		internal char			seperatorChar = '\0';
		DateTime				timeStamp;


		/// <summary>
		/// Constructs a SimiasEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="time"></param>
		public SimiasEventArgs(DateTime time) :
			this(null, time)
		{
		}

		/// <summary>
		/// Constructs a SimiasEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="eventData">Data of the event.</param>
		/// <param name="time">The time of this event.</param>
		public SimiasEventArgs(string eventData, DateTime time)
		{
			this.eventData = eventData;
			timeStamp = time;
		}

		/// <summary>
		/// Constructs a SimiasEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		public SimiasEventArgs() :
			this(null, DateTime.Now)
		{
		}
	
		#region Properties
		
		/// <summary>
		/// Gets the ChangeType for the event.
		/// </summary>
		public string EventData
		{
			get {return eventData;}
			set {eventData = value;}
		}

		/// <summary>
		/// Gets the timestamp for the event.
		/// </summary>
		public DateTime TimeStamp
		{
			get {return timeStamp;}
		}

		#endregion
	}
}
