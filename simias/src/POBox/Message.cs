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
using System.Xml;
using Simias.Storage;

namespace Simias.POBox
{
	// TODO: do we need a way to make a message read-only ... after a message has
	// been sent it should not be editable.

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
		/// The name of the property storing the message state.
		/// </summary>
		private const string MsgState = "MsgState";

		/// <summary>
		/// The name of the property storing the "To:" identity.
		/// </summary>
		private const string MsgToIdentity = "ToIdentity";

		/// <summary>
		/// The name of the property storing the "To:" address.
		/// </summary>
		private const string MsgToAddress = "ToAddress";

		/// <summary>
		/// The name of the property storing the "From:" identity.
		/// </summary>
		private const string MsgFromIdentity = "FromIdentity";

		/// <summary>
		/// The name of the property storing the "From:" address.
		/// </summary>
		private const string MsgFromAddress = "FromAddress";

		/// <summary>
		/// The name of the property storing the message body.
		/// </summary>
		private const string MsgBody = "MsgBody";

		/// <summary>
		/// The name of the property storing the message subject.
		/// </summary>
		private const string MsgSubject = "MsgSubject";
		#endregion

		#region Properties
		/// <summary>
		/// Gets/sets the state of the Message object.
		/// </summary>
		public MessageState MessageState
		{
			get
			{
				return (MessageState)Properties.GetSingleProperty(MsgState).Value;
			}
			set
			{
				Properties.ModifyProperty(MsgState, value);
			}
		}

		/// <summary>
		/// Gets/sets the subject of the message.
		/// </summary>
		public string MessageSubject
		{
			get
			{
				return (string)Properties.GetSingleProperty(MsgSubject).Value;
			}
			set
			{
				Properties.ModifyProperty(MsgSubject, value);
			}
		}

		/// <summary>
		/// Gets/sets the body of the message.
		/// </summary>
		public XmlDocument MessageBody
		{
			get
			{
				return (XmlDocument)Properties.GetSingleProperty(MsgBody).Value;
			}
			set
			{
				Properties.ModifyProperty(MsgBody, value);
			}
		}

		/// <summary>
		/// Gets/sets the recipient's address.
		/// </summary>
		public string MessageToAddress
		{
			get
			{
				return (string)Properties.GetSingleProperty(MsgToAddress).Value;
			}
			set
			{
				Properties.ModifyProperty(MsgToAddress, value);
			}
		}

		/// <summary>
		/// Gets/sets the recipient's identity.
		/// </summary>
		public string MessageToIdentity
		{
			get
			{
				return (string)Properties.GetSingleProperty(MsgToIdentity).Value;
			}
			set
			{
				Properties.ModifyProperty(MsgToIdentity, value);
			}
		}

		/// <summary>
		/// Gets/sets the sender's address.
		/// </summary>
		public string MessageFromAddress
		{
			get
			{
				return (string)Properties.GetSingleProperty(MsgFromAddress).Value;
			}
			set
			{
				Properties.ModifyProperty(MsgFromAddress, value);
			}
		}

		/// <summary>
		/// Gets/sets the sender's identity.
		/// </summary>
		public string MessageFromIdentity
		{
			get
			{
				return (string)Properties.GetSingleProperty(MsgFromIdentity).Value;
			}
			set
			{
				Properties.ModifyProperty(MsgFromIdentity, value);
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="collection">Collection that the ShallowNode belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the Message object from.</param>
		public Message(Collection collection, ShallowNode shallowNode) :
			base (collection, shallowNode)
		{
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name that is used by applications to describe the object.</param>
		/// <param name="messageType">Specifies the type of the message.</param>
		public Message(string messageName, MessageType messageType) :
			base (messageName)
		{
			this.Properties.AddProperty(MsgState, MessageState.New);
			this.Properties.AddNodeProperty(PropertyTags.Types, messageType.ToString());
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="toIdentity">The identity of the recipient.</param>
		public Message(string messageName, MessageType messageType, string toIdentity) :
			this (messageName, messageType)
		{
			this.Properties.AddProperty(MsgToIdentity, toIdentity);
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="toIdentity">The recipient's identity.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		public Message(string messageName, MessageType messageType, string toIdentity, string fromIdentity) :
			this (messageName, messageType, toIdentity)
		{
			this.Properties.AddProperty(MsgFromIdentity, fromIdentity);
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="toIdentity">The recipient's identity.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="toAddress">The recipient's address.</param>
		public Message(string messageName, MessageType messageType, string toIdentity, string fromIdentity, string toAddress) :
			this (messageName, messageType, toIdentity, fromIdentity)
		{
			Properties.AddProperty(MsgToAddress, toAddress);
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="toIdentity">The recipient's identity.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="toAddress">The recipient's address.</param>
		/// <param name="fromAddress">The sender's address.</param>
		public Message(string messageName, MessageType messageType, string toIdentity, string fromIdentity, string toAddress, string fromAddress) :
			this (messageName, messageType, toIdentity, fromIdentity, toAddress)
		{
			Properties.AddProperty(MsgFromAddress, fromAddress);
		}
		#endregion

		#region Public Methods
		#endregion
	}
}
