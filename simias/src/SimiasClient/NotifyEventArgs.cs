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
 *  Author: Mike Lasky
 *
 ***********************************************************************/
using System;

namespace Simias.Client.Event
{
	/// <summary>
	/// The event arguments for an error event.
	/// </summary>
	[Serializable]
	public class NotifyEventArgs : SimiasEventArgs
	{
		#region Class Members
		/// <summary>
		/// User defined notification message.
		/// </summary>
		private string message;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the notification message.
		/// </summary>
		public string Message
		{
			get { return message; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object.
		/// </summary>
		/// <param name="type">User defined notification type.</param>
		/// <param name="message">User defined notification message.</param>
		/// <param name="time">Time the error occurred.</param>
		public NotifyEventArgs( string type, string message, DateTime time ) :
			base ( type, time )
		{
			this.message = message;
		}
		#endregion
	}
}
