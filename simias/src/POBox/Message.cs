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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using Simias.Storage;

namespace Simias.POBox
{
	/// <summary>
	/// Message states.
	/// </summary>
	public enum MessageState
	{
		/// <summary>
		/// The message is un-opened.
		/// </summary>
		New,

		/// <summary>
		/// The message has been opened.
		/// </summary>
		Opened,

		/// <summary>
		/// The message has been deleted (but not purged yet).
		/// </summary>
		Deleted
	};

	/// <summary>
	/// Message types.
	/// </summary>
	public enum MessageType
	{
		/// <summary>
		/// The message is an inbound message.
		/// </summary>
		Inbound,

		/// <summary>
		/// The message is an outbound message.
		/// </summary>
		Outbound
	};

	/// <summary>
	/// A Message object is a specialized node used to hold ...
	/// </summary>
	public class Message : Node
	{
		#region Class Members
		/// <summary>
		/// The name of the property storing the message type.
		/// </summary>
		public const string MsgType = "MsgType";

		/// <summary>
		/// The name of the property storing the message state.
		/// </summary>
		public const string MsgState = "MsgState";

		/// <summary>
		/// The name of the property storing the "To:" identity.
		/// </summary>
		public const string MsgTo = "MsgTo";

		/// <summary>
		/// The name of the property storing the "From:" identity.
		/// </summary>
		public const string MsgFrom = "MsgFrom";
		#endregion

		#region Properties
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name that is used by applications to describe the object.</param>
		/// <param name="messageType">Specifies the type of the message.</param>
		public Message(string messageName, MessageType messageType) :
			base (messageName)
		{
			this.Properties.AddProperty(MsgType, messageType);
			this.Properties.AddProperty(MsgState, MessageState.New);
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="to">The identity of the recipient.</param>
		public Message(string messageName, MessageType messageType, string to) :
			this (messageName, messageType)
		{
			this.Properties.AddProperty(MsgTo, to);
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="to">The recipient's identity.</param>
		/// <param name="from">The sender's identity.</param>
		public Message(string messageName, MessageType messageType, string to, string from) :
			this (messageName, messageType, to)
		{
			this.Properties.AddProperty(MsgFrom, from);
		}
		#endregion
	}
}
