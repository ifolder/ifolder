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

namespace Simias.Service
{
	/// <summary>
	/// Message object used to communicate to a ProcessService.
	/// </summary>
	public class Message
	{
		#region fields.

		char [] seperator = {';'};
		internal MessageCode	majorMessage = 0;
		internal int			customMessage;
		internal string			data;

		#endregion

		#region Constructor

		/// <summary>
		/// Construct a Message from a string.
		/// </summary>
		/// <param name="message">The string to parse.</param>
		public Message(string message)
		{
			string[] args = message.Split(seperator, 4);
			if (args.Length < 3 || !args[0].Equals(this.GetType().ToString()))
			{
				throw new InvalidMessageException();
			}

			if (args.Length == 4)
			{
				data = args[3];
			}
			
			customMessage = int.Parse(args[2]);
			majorMessage = (MessageCode)Enum.Parse(typeof(MessageCode), args[1]);
		}

		/// <summary>
		/// Construct a message from the values.
		/// </summary>
		/// <param name="majorMessage">The MessageCode for this message.</param>
		/// <param name="customMessage">The custom message code.</param>
		/// <param name="data">A string for extra data.</param>
		public Message(MessageCode majorMessage, int customMessage, string data)
		{
			this.majorMessage = majorMessage;
			this.customMessage = customMessage;
			this.data = data;
		}

		#endregion

		/// <summary>
		/// Gets the message as a string.
		/// </summary>
		/// <returns>A string representation of the message.</returns>
		public override string ToString()
		{
			return String.Format("{0};{1};{2};{3}", this.GetType().ToString(), majorMessage, customMessage, data);
		}

		#region Properties.

		/// <summary>
		/// Get the Message code.
		/// </summary>
		internal MessageCode MajorMessage
		{
			get {return majorMessage;}
		}

		/// <summary>
		/// Gets the custom code.
		/// </summary>
		internal int CustomMessage
		{
			get {return customMessage;}
		}

		/// <summary>
		/// Gets the data of the message.
		/// </summary>
		internal string Data
		{
			get {return data;}
		}

		#endregion
	}

	#region MessageCode enum

	/// <summary>
	/// Defines the valid messages for a Service.
	/// </summary>
	public enum MessageCode
	{	
		/// <summary>
		/// 
		/// </summary>
		Invalid = 0,
		/// <summary>
		/// The service should start.
		/// </summary>
		Start,
		/// <summary>
		/// The service should stop.
		/// </summary>
		Stop,
		/// <summary>
		/// The service should pause.
		/// </summary>
		Pause,
		/// <summary>
		/// The service should resume.
		/// </summary>
		Resume,
		/// <summary>
		/// Used for custom messages.
		/// </summary>
		Custom
	};

	#endregion

	#region InvalidMessageException class

	/// <summary>
	/// Exception for an invalid message.
	/// </summary>
	public class InvalidMessageException : System.Exception
	{
		/// <summary>
		/// Constructs an InvalidMessageException.
		/// </summary>
		public InvalidMessageException() :
			base("Invalid Service Message")
		{
		}
	}

	#endregion
}
