/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Net;

using Simias;
using Simias.Storage;
using Simias.Agent;

namespace Simias.Sync
{
	/// <summary>
	/// A sync wrapper for collection objects.  The wrapper contains property names (scheme) used
	/// by syncing and implements serveral common tasks used by syncing on collections.
	/// </summary>
	public class SyncCollection : SyncNode, IDisposable
	{
		/// <summary>
		/// A collection property name for the sync role of the collection.
		/// </summary>
		public static readonly string RolePropertyName = "Sync Role";

		/// <summary>
		/// A collection property name for the sync host of the collection.
		/// The host is the machine with the master collection, which depending on
		/// the sync role could be the current collection and machine.
		/// </summary>
		public static readonly string HostPropertyName = "Sync Host";
		
		/// <summary>
		/// A collection property name for the sync port of the collection.
		/// The port is used with the host to connect to the master.
		/// </summary>
		public static readonly string PortPropertyName = "Sync Port";
		
		/// <summary>
		/// A collection property name for the sync interval to be used with the collection.
		/// The interval is specified in seconds.
		/// </summary>
		public static readonly string IntervalPropertyName = "Sync Interval";
		
		/// <summary>
		/// A collection property name of the sync logic class and assembly of the collection.
		/// Only collection using the same sync logic can communicate.
		/// </summary>
		public static readonly string LogicTypePropertyName = "Sync Logic";

		// the wrappered collection object
		private Collection baseCollection;

		/// <summary>
		/// The default constructor.
		/// </summary>
		/// <param name="collection">The collection object to be wrappered.</param>
		public SyncCollection(Collection collection) : base(collection)
		{
			this.baseCollection = collection;

			// guarentee an existing document root
			if (!Directory.Exists(collection.DocumentRoot.LocalPath))
			{
				Directory.CreateDirectory(collection.DocumentRoot.LocalPath);
			}
		}

		/// <summary>
		/// Commit the changes to the base collection.
		/// </summary>
		public override void Commit()
		{
			baseCollection.Commit(true);
		}

		/// <summary>
		/// Create an invitation object for the given identity.
		/// </summary>
		/// <param name="identity">The identity for the invitation.</param>
		/// <returns>A new invitation object.</returns>
		public Invitation CreateInvitation(string identity)
		{
			// validate the host and port
			if ((Host == null) || (Port <= 0))
			{
				throw new ArgumentException("An invitation requires " +
					"the sync host and port properties on " +
					"the master collection.");
			}

			// create the invitation
			Invitation invitation = new Invitation();

			invitation.CollectionId = ID;
			invitation.CollectionName = Name;
			invitation.CollectionType = baseCollection.Type;
			invitation.Domain = baseCollection.DomainName;
			invitation.MasterHost = Host;
			invitation.MasterPort = Port.ToString();
			invitation.Identity = identity;
			invitation.CollectionRights = baseCollection.GetUserAccess(identity).ToString();
			invitation.PublicKey = baseCollection.LocalStore.ServerPublicKey.ToXmlString( false );

			return invitation;
		}

		/// <summary>
		/// Refresh (or reload) the base collection to ensure that we are up-to-date.
		/// </summary>
		public void Refresh()
		{
			baseCollection = baseCollection.LocalStore.GetCollectionById(baseCollection.Id);
		}

		/// <summary>
		/// Generate a SyncNodeInfo object array from all the children of the base collection.
		/// </summary>
		/// <returns>The object array.</returns>
		public SyncNodeInfo[] GetNodeInfoArray()
		{
			ArrayList list = new ArrayList();

			foreach(Node node in baseCollection)
			{
				list.Add(new SyncNodeInfo(node));
			}

			return (SyncNodeInfo[])list.ToArray(typeof(SyncNodeInfo));
		}

		/// <summary>
		/// Get a node from the base collection with the given id.
		/// </summary>
		/// <param name="id">The id of the node.</param>
		/// <returns>The node object.</returns>
		public SyncNode GetNode(string id)
		{
			Node node = baseCollection.GetNodeById(id);

			return new SyncNode(node);
		}

		/// <summary>
		/// Get the xml representation of a node from the base collection with the given id.
		/// </summary>
		/// <param name="id">The id of the node.</param>
		/// <returns>A string of xml representing the data of the node.</returns>
		public string GetNodeXml(string id)
		{
			XmlDocument doc = baseCollection.LocalStore.ExportSingleNodeToXml(baseCollection, id);

			return doc.OuterXml;
		}

		/// <summary>
		/// Create a node object from a xml data string.
		/// </summary>
		/// <param name="xml">The node data in xml format.</param>
		/// <returns>The node object.</returns>
		public SyncNode CreateNodeFromXml(string xml)
		{
			XmlDocument doc = new XmlDocument();
			
			doc.LoadXml(xml);

			Node node = baseCollection.LocalStore.ImportSingleNodeFromXml(baseCollection, doc);

			return new SyncNode(node);
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose of this object.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			baseCollection = null;
		}

		#endregion

		#region Properties
		
		/// <summary>
		/// The base collection object.
		/// </summary>
		public Collection BaseCollection
		{
			get { return baseCollection; }
		}

		/// <summary>
		/// The syncing role of the base collection.
		/// </summary>
		public SyncCollectionRoles Role
		{
			get
			{
				SyncCollectionRoles role;
				
				role = (SyncCollectionRoles)GetProperty(RolePropertyName, SyncCollectionRoles.None);
				
				if (role == SyncCollectionRoles.None)
				{
					// note: slave collections are always marked by the invitation
					role = baseCollection.Synchronizeable ? SyncCollectionRoles.Master : SyncCollectionRoles.Local;
				}

				return role;
			}

			set { SetProperty(RolePropertyName, value, true); }
		}

		/// <summary>
		/// The syncing host of the base collection.
		/// </summary>
		public string Host
		{
			get { return (string)GetProperty(HostPropertyName); }
			set { SetProperty(HostPropertyName, value, true); }
		}

		/// <summary>
		/// The syncing port of the base collection.
		/// </summary>
		public int Port
		{
			get { return (int)GetProperty(PortPropertyName, -1); }
			set { SetProperty(PortPropertyName, value, true); }
		}

		/// <summary>
		/// The syncing interval of the base collection.
		/// </summary>
		public int Interval
		{
			get { return (int)GetProperty(IntervalPropertyName, -1); }
			set { SetProperty(IntervalPropertyName, value, true); }
		}

		/// <summary>
		/// The syncing logic of the base collection.
		/// </summary>
		public string LogicType
		{
			get { return (string)GetProperty(LogicTypePropertyName); }
			set { SetProperty(LogicTypePropertyName, value, true); }
		}

		/// <summary>
		/// The remoting endpoint for the base collection.
		/// </summary>
		public string EndPoint
		{
			get { return ID + ".rem"; }
		}

		/// <summary>
		/// The syncing URL for the base collection.
		/// </summary>
		public string Url
		{
			get { return (new UriBuilder("http", Host, Port, EndPoint).ToString()); }
		}

		/// <summary>
		/// The store path of the base collection.
		/// </summary>
		public string StorePath
		{
			get { return Path.GetDirectoryName(baseCollection.LocalStore.StorePath.LocalPath); }
		}

		/// <summary>
		/// The root path of the base collection.
		/// </summary>
		public string RootPath
		{
			get { return Path.GetDirectoryName(baseCollection.DocumentRoot.LocalPath); }
		}

		/// <summary>
		/// The root path of the file entries of the collection.
		/// </summary>
		public string StreamRootPath
		{
			get { return baseCollection.DocumentRoot.LocalPath; }
		}

		/// <summary>
		/// The authroization domain of the base collection.
		/// </summary>
		public string Domain
		{
			get { return baseCollection.DomainName; }
		}

		/// <summary>
		/// The identity for accessing the base collection.
		/// </summary>
		public string AccessIdentity
		{
			get
			{
				return baseCollection.LocalStore.CurrentIdentity.GetDomainUserGuid(Domain);
			}
		}

		#endregion
	}
}
