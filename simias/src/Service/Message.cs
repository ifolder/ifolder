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
	internal class Message
	{
		#region fields.

		internal ServiceCtl		service;
		internal static char [] seperator = {';'};
		internal MessageCode	majorMessage = 0;
		internal int			customMessage;
		internal string			data;
		internal static string  messageSignature = typeof(Message).ToString();
		
		#endregion

		#region Constructor

		/// <summary>
		/// Construct a Message from a string.
		/// </summary>
		/// <param name="service">The servie the command is for.</param>
		/// <param name="message">The string to parse.</param>
		internal Message(ServiceCtl service, string message)
		{
			this.service = service;
			string[] args = message.Split(seperator, 4);
			if (args.Length < 3 || !args[0].Equals(messageSignature))
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
		/// <param name="service">The servie the command is for.</param>
		/// <param name="majorMessage">The MessageCode for this message.</param>
		/// <param name="customMessage">The custom message code.</param>
		/// <param name="data">A string for extra data.</param>
		internal Message(ServiceCtl service, MessageCode majorMessage, int customMessage, string data)
		{
			this.service = service;
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
			return String.Format("{0};{1};{2};{3}", messageSignature, majorMessage, customMessage, data);
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

	/// <summary>
	/// Used to start a service.
	/// </summary>
	internal class StartMessage : Message
	{
		internal StartMessage(ServiceCtl service) :
			base(service, MessageCode.Start, 0, null)
		{
		}
	}

	/// <summary>
	/// Used to stop a service.
	/// </summary>
	internal class StopMessage : Message
	{
		internal StopMessage(ServiceCtl service) :
			base(service, MessageCode.Stop, 0, null)
		{
		}
	}

	/// <summary>
	/// Used to pause a service.
	/// </summary>
	internal class PauseMessage : Message
	{
		internal PauseMessage(ServiceCtl service) :
			base(service, MessageCode.Pause, 0, null)
		{
		}
	}

	/// <summary>
	/// Used to resume a service.
	/// </summary>
	internal class ResumeMessage : Message
	{
		internal ResumeMessage(ServiceCtl service) :
			base(service, MessageCode.Resume, 0, null)
		{
		}
	}

	/// <summary>
	/// Used to send a custom message.
	/// </summary>
	internal class CustomMessage : Message
	{
		internal CustomMessage(ServiceCtl service, int message, string data) :
			base(service, MessageCode.Custom, message, data)
		{
		}
	}

	/// <summary>
	/// Used to signale that all services have been started.
	/// </summary>
	internal class StartComplete : Message
	{
		internal StartComplete() :
			base(null, MessageCode.StartComplete, 0, null)
		{
		}
	}

	/// <summary>
	/// Used to signal that all services have been stoped.
	/// </summary>
	internal class StopComplete : Message
	{
		internal StopComplete() :
			base(null, MessageCode.StopComplete, 0, null)
		{
		}
	}

	#region MessageCode enum

	/// <summary>
	/// Defines the valid messages for a Service.
	/// </summary>
	internal enum MessageCode
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
		Custom,
		/// <summary>
		/// Used to signal that all services have been started.
		/// </summary>
		StartComplete,
		/// <summary>
		/// Used to signal that all services have been stoped.
		/// </summary>
		StopComplete
	};

	#endregion

	#region InvalidMessageException class

	/// <summary>
	/// Exception for an invalid message.
	/// </summary>
	public class InvalidMessageException : SimiasException
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
