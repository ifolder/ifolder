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
	/// Invitation states.
	/// </summary>
	public enum InvitationState
	{
		/// <summary>
		/// The invitation has been created but not sent.
		/// </summary>
		Pending,

		/// <summary>
		/// The invitation has been sent.
		/// </summary>
		Invited,

		/// <summary>
		/// The invitation, a request to join, has been sent.
		/// </summary>
		Posted,

		/// <summary>
		/// The invitation, a request to join, has been rejected.
		/// </summary>
		Rejected,

		/// <summary>
		/// The invitation has been accepted.
		/// </summary>
		Accepted,

		/// <summary>
		/// The invitation has been declined.
		/// </summary>
		Declined
	};

	/// <summary>
	/// An Invitation object is a specialized message used for inviting someone to a team space.
	/// </summary>
	public class Invitation : Message
	{
		#region Class Members
		/// <summary>
		/// The name of the property storing the InvitationState.
		/// </summary>
		public const string InvitationStateProperty = "InvitationState";

		/// <summary>
		/// The name of the property storing the recipient's public key.
		/// </summary>
		public const string ToPublicKeyProperty = "ToPublicKey";

		/// <summary>
		/// The name of the property storing the sender's public key.
		/// </summary>
		public const string FromPublicKeyProperty = "FromPublicKey";

		/// <summary>
		/// The name of the property storing the collection name.
		/// </summary>
		public const string InvitationCollectionNameProperty = "InvitationCollectionName";

		/// <summary>
		/// The name of the property storing the collection ID.
		/// </summary>
		public const string InvitationCollectionIdProperty = "InvitationCollectionId";

		/// <summary>
		/// The name of the property storing the collection types.
		/// </summary>
		public const string InvitationCollectionTypesProperty = "InvitationCollectionTypes";

		/// <summary>
		/// The name of the property storing the collection master URL.
		/// </summary>
		public const string InvitationCollectionURLProperty = "InvitationCollectionURL";

		/// <summary>
		/// The name of the property storing the collection description.
		/// </summary>
		public const string CollectionDescriptionProperty = "CollectionDescription";

		/// <summary>
		/// The name of the property storing the DirNode ID.
		/// </summary>
		public const string DirNodeIdProperty = "DirNodeId";

		/// <summary>
		/// The name of the property storing the DirNode name.
		/// </summary>
		public const string DirNodeNameProperty = "DirNodeName";

		/// <summary>
		/// The name of the property storing the rights requested/granted.
		/// </summary>
		public const string InvitationRightsProperty = "InvitationRights";
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new Invitation object.
		/// </summary>
		/// <param name="collection">Collection that the ShallowNode belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the Message object from.</param>
		public Invitation(Collection collection, ShallowNode shallowNode) :
			base (collection, shallowNode)
		{
		}

		/// <summary>
		/// Constructor for creating a new Invitation object.
		/// </summary>
		/// <param name="invitationName">The friendly name of the invitation.</param>
		/// <param name="messageType">The type of the message.</param>
		public Invitation(string invitationName, string messageType) :
			base (invitationName, messageType)
		{
			InviteState = InvitationState.Pending;
		}

		/// <summary>
		/// Constructor for creating a new Invitation object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="toIdentity">The identity of the recipient.</param>
		public Invitation(string messageName, string messageType, string toIdentity) :
			base (messageName, messageType, toIdentity)
		{
			InviteState = InvitationState.Pending;
		}

		/// <summary>
		/// Constructor for creating a new Invitation object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="toIdentity">The recipient's identity.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		public Invitation(string messageName, string messageType, string toIdentity, string fromIdentity) :
			base (messageName, messageType, toIdentity, fromIdentity)
		{
			InviteState = InvitationState.Pending;
		}

		/// <summary>
		/// Constructor for creating a new Invitation object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="toIdentity">The recipient's identity.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="toAddress">The recipient's address.</param>
		public Invitation(string messageName, string messageType, string toIdentity, string fromIdentity, string toAddress) :
			base (messageName, messageType, toIdentity, fromIdentity, toAddress)
		{
			InviteState = InvitationState.Pending;
		}

		/// <summary>
		/// Constructor for creating a new Invitation object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="toIdentity">The recipient's identity.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="toAddress">The recipient's address.</param>
		/// <param name="fromAddress">The sender's address.</param>
		public Invitation(string messageName, string messageType, string toIdentity, string fromIdentity, string toAddress, string fromAddress) :
			base (messageName, messageType, toIdentity, fromIdentity, toAddress, fromAddress)
		{
			InviteState = InvitationState.Pending;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets/sets the state of the Invitation object.
		/// </summary>
		public InvitationState InviteState
		{
			get
			{
				return (InvitationState)Properties.GetSingleProperty(InvitationStateProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(InvitationStateProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the recipient's public key
		/// </summary>
		public string ToPublicKey
		{
			get
			{
				return (string)Properties.GetSingleProperty(ToPublicKeyProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(ToPublicKeyProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the sender's public key.
		/// </summary>
		public string FromPublicKey
		{
			get
			{
				return (string)Properties.GetSingleProperty(FromPublicKeyProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(FromPublicKeyProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the name of the collection to share.
		/// </summary>
		public string InvitationCollectionName
		{
			get
			{
				return (string)Properties.GetSingleProperty(InvitationCollectionNameProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(InvitationCollectionNameProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the ID of the collection to share.
		/// </summary>
		public string InvitationCollectionId
		{
			get
			{
				return (string)Properties.GetSingleProperty(InvitationCollectionIdProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(InvitationCollectionIdProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the types of the collection to share.
		/// </summary>
		public string InvitationCollectionTypes
		{
			get
			{
				return (string)Properties.GetSingleProperty(InvitationCollectionTypesProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(InvitationCollectionTypesProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the master URL of the collection to share.
		/// </summary>
		public string InvitationCollectionURL
		{
			get
			{
				return (string)Properties.GetSingleProperty(InvitationCollectionURLProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(InvitationCollectionURLProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the description of the collection to share.
		/// </summary>
		public string CollectionDescription
		{
			get
			{
				return (string)Properties.GetSingleProperty(CollectionDescriptionProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(CollectionDescriptionProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the ID of the collection's root DirNode.
		/// </summary>
		public string DirNodeId
		{
			get
			{
				return (string)Properties.GetSingleProperty(DirNodeIdProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(DirNodeIdProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the name of the collection's root DirNode.
		/// </summary>
		public string DirNodeName
		{
			get
			{
				return (string)Properties.GetSingleProperty(DirNodeNameProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(DirNodeNameProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the rights that will be granted on the shared collection.
		/// </summary>
		public Access.Rights InvitationRights
		{
			get
			{
				return (Access.Rights)Properties.GetSingleProperty(InvitationRightsProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(InvitationRightsProperty, value);
			}
		}
		#endregion
	}
}
