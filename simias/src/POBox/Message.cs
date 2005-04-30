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

using Simias.Client;
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
		Deleted,

		/// <summary>
		/// The message state is unknown.
		/// </summary>
		Unknown
	};

	/// <summary>
	/// A Message object is a specialized node used to hold ...
	/// </summary>
	[Serializable]
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
		/// The name of the property storing the message id.
		/// </summary>
		public const string MessageIDProperty = "MsgID";

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
		/// Gets/sets the id of the message.
		/// </summary>
		public string MessageID
		{
			get
			{
				Property p = properties.FindSingleValue(MessageIDProperty);
				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(MessageIDProperty, value);
			}
		}
		
		/// <summary>
		/// Gets/sets the type of the message.
		/// </summary>
		public string MessageType
		{
			get
			{
				Property p = properties.FindSingleValue(MessageTypeProperty);
				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(MessageTypeProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the state of the Message object.
		/// </summary>
		public MessageState State
		{
			get
			{
				Property p = properties.FindSingleValue(MessageStateProperty);
				return (p != null) ? (MessageState)p.Value : MessageState.Unknown;
			}
			set
			{
				properties.ModifyNodeProperty(MessageStateProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the subject of the message.
		/// </summary>
		public string Subject
		{
			get
			{
				Property p = properties.FindSingleValue(SubjectProperty);
				return (p != null) ? p.ToString() : null;
			}
			set
			{
				Properties.ModifyNodeProperty(SubjectProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the body of the message.
		/// </summary>
		public string Body
		{
			get
			{
				Property p = properties.FindSingleValue(BodyProperty);
				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(BodyProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the recipient's friendly name.
		/// </summary>
		public string ToName
		{
			get
			{
				Property p = properties.FindSingleValue(ToNameProperty);
				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(ToNameProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the recipient's address.
		/// </summary>
		public string ToAddress
		{
			get
			{
				Property p = properties.FindSingleValue(ToAddressProperty);
				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(ToAddressProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the recipient's identity.
		/// </summary>
		public string ToIdentity
		{
			get
			{
				Property p = properties.FindSingleValue(ToIdentityProperty);
				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(ToIdentityProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the sender's friendly name.
		/// </summary>
		public string FromName
		{
			get
			{
				Property p = properties.FindSingleValue(FromNameProperty);
				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(FromNameProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the sender's address.
		/// </summary>
		public string FromAddress
		{
			get
			{
				Property p = properties.FindSingleValue(FromAddressProperty);
				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(FromAddressProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the sender's identity.
		/// </summary>
		public string FromIdentity
		{
			get
			{
				Property p = properties.FindSingleValue(FromIdentityProperty);
				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(FromIdentityProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the identity domain id.
		/// </summary>
		public string DomainID
		{
			get
			{
				Property p = properties.FindSingleValue(DomainIDProperty);
				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(DomainIDProperty, value);
			}
		}
		
		/// <summary>
		/// Gets/sets the identity domain name.
		/// </summary>
		public string DomainName
		{
			get
			{
				Property p = properties.FindSingleValue(DomainNameProperty);
				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(DomainNameProperty, value);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for creating a new Message object with a specific ID.
		/// </summary>
		/// <param name="messageName">The friendly name of the Message object.</param>
		/// <param name="baseType">Base type of node.</param>
		/// <param name="messageID">The ID of the Message object.</param>
		public Message(string messageName, string baseType, string messageID) :
			base (messageName, Guid.NewGuid().ToString(), baseType)
		{
			this.MessageID = messageID;

			if (baseType != NodeTypes.MessageType)
			{
				properties.AddNodeProperty(PropertyTags.Types, NodeTypes.MessageType);
			}

			// Add a property to never allow collisions to happen on a Message. The Message
			// on the server will always be the authority.
			CollisionPolicy = CollisionPolicy.ServerWins;
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="baseType">Base type of node.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The identity of the sender.</param>
		public Message(string messageName, string baseType, string messageType, string fromIdentity) :
			this (messageName, baseType, messageType, fromIdentity, null, null, null)
		{
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="baseType">Base type of node.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="fromAddress">The sender's address.</param>
		public Message(string messageName, string baseType, string messageType, string fromIdentity, string fromAddress) :
			this (messageName, baseType, messageType, fromIdentity, fromAddress, null, null)
		{
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="baseType">Base type of node.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="fromAddress">The sender's address.</param>
		/// <param name="toAddress">The recipient's address.</param>
		public Message(string messageName, string baseType, string messageType, string fromIdentity, string fromAddress, string toAddress) :
			this (messageName, baseType, messageType, fromIdentity, fromAddress, toAddress, null)
		{
		}

		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="baseType">Base type of node.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="fromAddress">The sender's address.</param>
		/// <param name="toAddress">The recipient's address.</param>
		/// <param name="toIdentity">The recipient's identity.</param>
		public Message(string messageName, string baseType, string messageType, string fromIdentity, string fromAddress, string toAddress, string toIdentity) :
			base(messageName, Guid.NewGuid().ToString(), baseType)
		{
			MessageID = Guid.NewGuid().ToString();
			State = MessageState.New;

			if (messageType != null)
			{
				MessageType = messageType;
			}

			if (fromIdentity != null)
			{
				FromIdentity = fromIdentity;
			}

			if (toAddress != null)
			{
				ToAddress = toAddress;
			}

			if (fromAddress != null)
			{
				FromAddress = fromAddress;
			}

			if (toIdentity != null)
			{
				ToIdentity = toIdentity;
			}

			if (baseType != NodeTypes.MessageType)
			{
				properties.AddNodeProperty(PropertyTags.Types, NodeTypes.MessageType);
			}

			// Add a property to never allow collisions to happen on a Message. The Message
			// on the server will always be the authority.
			CollisionPolicy = CollisionPolicy.ServerWins;
		}

		/// <summary>
		/// Clone the message with a new ID.
		/// </summary>
		/// <param name="ID">The new node ID.</param>
		/// <param name="message">The message to clone.</param>
		public Message(string ID, Message message) : 
			base(ID, message)
		{
		}

		/// <summary>
		/// Constructor for creating an existing Message object from a Node object.
		/// </summary>
		/// <param name="node">The Node object to create the Message object from.</param>
		public Message(Node node) :
			base (node)
		{
		}

		/// <summary>
		/// Constructor for creating a existing Message object.
		/// </summary>
		/// <param name="collection">Collection that the ShallowNode belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the Message object from.</param>
		public Message(Collection collection, ShallowNode shallowNode) :
			base (collection, shallowNode)
		{
		}

		/// <summary>
		/// Constructor for creating an existing Message object from an Xml document.
		/// </summary>
		/// <param name="document">Xml document object to create Message object from.</param>
		internal Message(XmlDocument document) :
			base (document)
		{
		}
#endregion

		#region Public Methods
		
		#endregion
	}
}
