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
	/// A Message object is a specialized node used to hold ...
	/// </summary>
	public class Message : Node
	{
		#region Class Members
		/// <summary>
		/// The type for an inbound message.
		/// </summary>
		public static readonly string InboundMessage = "Inbound";

		/// <summary>
		/// The type for an outbound message.
		/// </summary>
		public static readonly string OutboundMessage = "Outbound";

		/// <summary>
		/// The name of the property storing the message type.
		/// </summary>
		public const string MessageTypeProperty = "MsgType";

		/// <summary>
		/// The name of the property storing the message state.
		/// </summary>
		public const string MessageStateProperty = "MsgState";

		/// <summary>
		/// The name of the property storing the "To:" friendly name.
		/// </summary>
		public const string ToNameProperty = "ToName";

		/// <summary>
		/// The name of the property storing the "To:" identity.
		/// </summary>
		public const string ToIdentityProperty = "ToID";

		/// <summary>
		/// The name of the property storing the "To:" address.
		/// </summary>
		public const string ToAddressProperty = "ToAddr";

		/// <summary>
		/// The name of the property storing the "From:" friendly name.
		/// </summary>
		public const string FromNameProperty = "FromName";

		/// <summary>
		/// The name of the property storing the "From:" identity.
		/// </summary>
		public const string FromIdentityProperty = "FromID";

		/// <summary>
		/// The name of the property storing the "From:" address.
		/// </summary>
		public const string FromAddressProperty = "FromAddr";

		/// <summary>
		/// The name of the property storing the message body.
		/// </summary>
		public const string BodyProperty = "Body";

		/// <summary>
		/// The name of the property storing the message subject.
		/// </summary>
		public const string SubjectProperty = "Subject";

		/// <summary>
		/// The name of the property storing the master URL.
		/// </summary>
		public const string MasterURLProperty = "MasterUrl";
		
		/// <summary>
		/// The name of the property storing the domain id.
		/// </summary>
		public const string DomainIDProperty = "DomainID";
		
		/// <summary>
		/// The name of the property storing the domain name.
		/// </summary>
		public const string DomainNameProperty = "DomainName";
		
		#endregion

		#region Properties
		
		/// <summary>
		/// Gets/sets the type of the message.
		/// </summary>
		public string MessageType
		{
			get
			{
				return (string)Properties.GetSingleProperty(MessageTypeProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(MessageTypeProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the state of the Message object.
		/// </summary>
		public MessageState State
		{
			get
			{
				return (MessageState)Properties.GetSingleProperty(MessageStateProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(MessageStateProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the subject of the message.
		/// </summary>
		public string Subject
		{
			get
			{
				return (string)Properties.GetSingleProperty(SubjectProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(SubjectProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the body of the message.
		/// </summary>
		public string Body
		{
			get
			{
				return (string)Properties.GetSingleProperty(BodyProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(BodyProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the recipient's friendly name.
		/// </summary>
		public string ToName
		{
			get
			{
				return (string)Properties.GetSingleProperty(ToNameProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(ToNameProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the recipient's address.
		/// </summary>
		public string ToAddress
		{
			get
			{
				return (string)Properties.GetSingleProperty(ToAddressProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(ToAddressProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the recipient's identity.
		/// </summary>
		public string ToIdentity
		{
			get
			{
				return (string)Properties.GetSingleProperty(ToIdentityProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(ToIdentityProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the sender's friendly name.
		/// </summary>
		public string FromName
		{
			get
			{
				return (string)Properties.GetSingleProperty(FromNameProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(FromNameProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the sender's address.
		/// </summary>
		public string FromAddress
		{
			get
			{
				return (string)Properties.GetSingleProperty(FromAddressProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(FromAddressProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the sender's identity.
		/// </summary>
		public string FromIdentity
		{
			get
			{
				return (string)Properties.GetSingleProperty(FromIdentityProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(FromIdentityProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the master URL for the collection.
		/// </summary>
		public Uri MasterURL
		{
			get
			{
				return (Uri)Properties.GetSingleProperty(MasterURLProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(MasterURLProperty, value);
			}
		}
		
		/// <summary>
		/// Gets/sets the identity domain id.
		/// </summary>
		public string DomainID
		{
			get
			{
				return (string)Properties.GetSingleProperty(DomainIDProperty).Value;
			}
			set
			{
				Properties.ModifyNodeProperty(DomainIDProperty, value);
			}
		}
		
		/// <summary>
		/// Gets/sets the identity domain name.
		/// </summary>
		public string DomainName
		{
			get
			{
				return (string)Properties.GetSingleProperty(DomainNameProperty).Value;
			}
			set
			{
				Properties.ModifyNodeProperty(DomainNameProperty, value);
			}
		}

		#endregion

		#region Constructors
		
		/// <summary>
		/// Constructor for creating a Message object from a Node object.
		/// </summary>
		/// <param name="node">The Node object to create the Message object from.</param>
		public Message(Node node) :
			base (node)
		{
		}

		/// <summary>
		/// Constructor for creating a new Message object with a specific ID.
		/// </summary>
		/// <param name="messageName">The friendly name of the Message object.</param>
		/// <param name="messageID">The ID of the Message object.</param>
		public Message(string messageName, string messageID) :
			base (messageName, messageID)
		{
			Properties.AddProperty(PropertyTags.Types, typeof(Message).Name);
		}

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
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The identity of the sender.</param>
		public Message(string messageName, string messageType, string fromIdentity) :
			base (messageName)
		{
			State = MessageState.New;
			MessageType = messageType;
			FromIdentity = fromIdentity;
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="fromAddress">The sender's address.</param>
		public Message(string messageName, string messageType, string fromIdentity, string fromAddress) :
			this (messageName, messageType, fromIdentity)
		{
			FromAddress = fromAddress;
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="fromAddress">The sender's address.</param>
		/// <param name="toAddress">The recipient's address.</param>
		public Message(string messageName, string messageType, string fromIdentity, string fromAddress, string toAddress) :
			this (messageName, messageType, fromIdentity, fromAddress)
		{
			ToAddress = toAddress;
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="fromAddress">The sender's address.</param>
		/// <param name="toAddress">The recipient's address.</param>
		/// <param name="toIdentity">The recipient's identity.</param>
		public Message(string messageName, string messageType, string fromIdentity, string fromAddress, string toAddress, string toIdentity) :
			this (messageName, messageType, fromIdentity, fromAddress, toAddress)
		{
			ToIdentity = toIdentity;
		}

		#endregion

		#region Public Methods
		
		#endregion
	}
}
