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
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

using System;

namespace Simias.Client
{
	/// <summary>
	/// Contains class names derived from Node that are used as Node object types.
	/// </summary>
	public class NodeTypes
	{
		#region Class Members
		/// <summary>
		/// Base types for a node object.
		/// </summary>
		public enum NodeTypeEnum
		{
			/// <summary>
			/// BaseFileNode
			/// </summary>
			BaseFileNode,

			/// <summary>
			/// Collection
			/// </summary>
			Collection,

			/// <summary>
			/// DirNode
			/// </summary>
			DirNode,

			/// <summary>
			/// Domain
			/// </summary>
			Domain,

			/// <summary>
			/// FileNode
			/// </summary>
			FileNode,

			/// <summary>
			/// Identity
			/// </summary>
			Identity,

			/// <summary>
			/// LinkNode
			/// </summary>
			LinkNode,

			/// <summary>
			/// LocalDatabase
			/// </summary>
			LocalDatabase,

			/// <summary>
			/// Member
			/// </summary>
			Member,

			/// <summary>
			/// Message
			/// </summary>
			Message,

			/// <summary>
			/// Node
			/// </summary>
			Node,

			/// <summary>
			/// POBox
			/// </summary>
			POBox,

			/// <summary>
			/// Policy
			/// </summary>
			Policy,

			/// <summary>
			/// StoreFileNode
			/// </summary>
			StoreFileNode,

			/// <summary>
			/// Subscription
			/// </summary>
			Subscription,

			/// <summary>
			/// Tombstone
			/// </summary>
			Tombstone
		};

		/// <summary>
		/// Arrray of strings used to hold the names of classes used as Node object types.
		/// </summary>
		static private string[] classNames;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the BaseFileNode class name.
		/// </summary>
		static public string BaseFileNodeType
		{
			get { return classNames[ 0 ]; }
		}

		/// <summary>
		/// Gets the Collection class name.
		/// </summary>
		static public string CollectionType
		{
			get { return classNames[ 1 ]; }
		}

		/// <summary>
		/// Gets the DirNode class name.
		/// </summary>
		static public string DirNodeType
		{
			get { return classNames[ 2 ]; }
		}


		/// <summary>
		/// Gets the Domain class name.
		/// </summary>
		static public string DomainType
		{
			get { return classNames[ 3 ]; }
		}

		/// <summary>
		/// Gets the FileNode class name.
		/// </summary>
		static public string FileNodeType
		{
			get { return classNames[ 4 ]; }
		}

		/// <summary>
		/// Gets the Identity class name.
		/// </summary>
		static public string IdentityType
		{
			get { return classNames[ 5 ]; }
		}

		/// <summary>
		/// Gets the LinkNode class name.
		/// </summary>
		static public string LinkNodeType
		{
			get { return classNames[ 6 ]; }
		}

		/// <summary>
		/// Gets the LocalDatabase class name.
		/// </summary>
		static public string LocalDatabaseType
		{
			get { return classNames[ 7 ]; }
		}

		/// <summary>
		/// Gets the Member class name.
		/// </summary>
		static public string MemberType
		{
			get { return classNames[ 8 ]; }
		}

		/// <summary>
		/// Gets the Message class name.
		/// </summary>
		static public string MessageType
		{
			get { return classNames[ 9 ]; }
		}

		/// <summary>
		/// Gets the Node class name.
		/// </summary>
		static public string NodeType
		{
			get { return classNames[ 10 ]; }
		}

		/// <summary>
		/// Gets the POBox class name.
		/// </summary>
		static public string POBoxType
		{
			get { return classNames[ 11 ]; }
		}

		/// <summary>
		/// Gets the Policy class name.
		/// </summary>
		static public string PolicyType
		{
			get { return classNames[ 12 ]; }
		}

		/// <summary>
		/// Gets the StoreFileNode class name.
		/// </summary>
		static public string StoreFileNodeType
		{
			get { return classNames[ 13 ]; }
		}

		/// <summary>
		/// Gets the Subscription class name.
		/// </summary>
		static public string SubscriptionType
		{
			get { return classNames[ 14 ]; }
		}

		/// <summary>
		/// Gets the Tombstone class name.
		/// </summary>
		static public string TombstoneType
		{
			get { return classNames[ 15 ]; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Static constructor to do one-time initialization.
		/// </summary>
		static NodeTypes()
		{
			classNames = new string[] {	  "BaseFileNode",
										  "Collection",
										  "DirNode",
										  "Domain",
										  "FileNode",
										  "Identity",
										  "LinkNode",
										  "LocalDatabase",
										  "Member",
										  "Message",
										  "Node",
										  "POBox",
										  "Policy",
										  "StoreFileNode",
										  "Subscription",
										  "Tombstone" };
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns whether specified class name is a NodeType.
		/// </summary>
		/// <param name="type">Class name string.</param>
		/// <returns>True if specified class name is a Node object type. Otherwise false is returned.</returns>
		static public bool IsNodeType( string type )
		{
			bool isType = false;

			foreach ( string s in classNames )
			{
				if ( s == type )
				{
					isType = true;
					break;
				}
			}

			return isType;
		}
		#endregion
	}
}
