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
 *  Author: Russ Young <ryoung@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Xml;

using Simias.Client;

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents a Simias Host.
	/// </summary>
	[ Serializable ]
	public class HostNode : Node
	{
		#region Class Members
		/// <summary>
		/// The Private address not accessible outside the firewall
		/// used for server to server communication.
		/// </summary>
		static string PrivateAddressTag = "PrivateAddress";
		/// <summary>
		/// The public address used for normal access.
		/// </summary>
		static string PublicAddressTag = "PublicAddress";
		#endregion

		#region Properties
		/// <summary>
		/// Gets/Sets the public address for this host.
		/// </summary>
		public string PublicAddress
		{
			get
			{
				Property pa = Properties.GetSingleProperty(PublicAddressTag);
				if (pa != null)
				{
					return pa.Value.ToString();
				}
				throw new NotExistException(PublicAddressTag);
			}
			set
			{
				Properties.ModifyProperty(new Property(PublicAddressTag, value));
			}
		}

		/// <summary>
		/// Gets/Sets the private address for this host.
		/// </summary>
		public string PrivateAddress
		{
			get
			{
				Property pa = Properties.GetSingleProperty(PrivateAddressTag);
				if (pa != null)
				{
					return pa.Value.ToString();
				}
				throw new NotExistException(PrivateAddressTag);
			}
			set
			{
				Properties.ModifyProperty(new Property(PrivateAddressTag, value));
			}
		}

		/// <summary>
		/// Gets the public key for this host.
		/// </summary>
		public string PublicKey
		{
			get
			{
				return "Not Implemented";
			}
			set
			{
			}
		}

		/// <summary>
		/// Gets the private key for this host.
		/// </summary>
		internal string PrivateKey
		{
			get
			{
				return "Not Implemented";
			}
			set
			{
			}
		}
		#endregion

		#region Consturctors
		/// <summary>
		/// Construct a new host node.
		/// </summary>
		/// <param name="name">The name of the host.</param>
		/// <param name="publicAddress">The public address for the host.</param>
		/// <param name="privateAddress">The private address for the host.</param>
		public HostNode(string name, string publicAddress, string privateAddress) :
			base(name, Guid.NewGuid().ToString(), NodeTypes.HostNodeType)
		{
			// Set the Addresses.
			PublicAddress = publicAddress;
			PrivateAddress = privateAddress;

			// Generate Private/Public Keypair.
		}

		/// <summary>
		/// Consturct a new host node.
		/// </summary>
		/// <param name="name">The name of the host.</param>
		/// <param name="publicAddress">The public address for the host.</param>
		public HostNode(string name, string publicAddress) :
			this(name, publicAddress, publicAddress)
		{
		}

		/// <summary>
		/// Construct a host node from a node.
		/// </summary>
		/// <param name="node">The host node.</param>
		public HostNode(Node node) :
			base(node)
		{
			if (type != NodeTypes.HostNodeType)
			{
				throw new CollectionStoreException(String.Format("Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.HostNodeType, type));
			}
		}

		/// <summary>
		/// Construct a host node from a shallow node.
		/// </summary>
		/// <param name="collection">The collection the node belongs to.</param>
		/// <param name="shallowNode">The shallow node that represents the HostNode.</param>
		public HostNode(Collection collection, ShallowNode shallowNode)
			:
			base(collection, shallowNode)
		{
			if (type != NodeTypes.HostNodeType)
			{
				throw new CollectionStoreException(String.Format("Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.HostNodeType, type));
			}
		}

		/// <summary>
		/// Construct a HostNode from the serialized XML.
		/// </summary>
		/// <param name="document">The XML represention of the HostNode.</param>
		internal HostNode(XmlDocument document)
			:
			base(document)
		{
			if (type != NodeTypes.HostNodeType)
			{
				throw new CollectionStoreException(String.Format("Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.HostNodeType, type));
			}
		}
		#endregion
	}
}