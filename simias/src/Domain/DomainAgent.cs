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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Net;
using System.Web;
using System.Xml;

using Simias;
using Simias.Storage;
using Simias.Sync;
using PostOffice = Simias.POBox;

namespace Simias.Domain
{
	/// <summary>
	/// Class used to assist in configuring the domain.
	/// </summary>
	public class DomainConfig
	{
		#region Class Members
		/// <summary>
		/// Configuration file XML tags.
		/// </summary>
		private static string SectionTag = "DomainClient";
		private static string ServersTag = "Servers";
		private static string ServerTag = "Server";
		private static string NameTag = "name";
		private static string IDTag = "id";
		private static string DescriptionTag = "description";
		private static string UriTag = "uri";
		private static string EnabledTag = "enabled";

		/// <summary>
		/// Default scheme.
		/// </summary>
		private static string defaultScheme = "http";

		/// <summary>
		/// Holds a reference to the simias configuration file.
		/// </summary>
		private Configuration config = Configuration.GetConfiguration();

		/// <summary>
		/// Name of the domain for which configuration information is associated with.
		/// </summary>
		private string domainName;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the domain ID.
		/// </summary>
		public string ID
		{
			get { return GetDomainAttribute(IDTag); }
			set { SetDomainAttribute(IDTag, value); }
		}

		/// <summary>
		/// Gets or sets the domain description.
		/// </summary>
		public string Description
		{
			get { return GetDomainAttribute(DescriptionTag); }
			set { SetDomainAttribute(DescriptionTag, value); }
		}

		/// <summary>
		/// Gets or sets the domain host.
		/// </summary>
		public string Host
		{
			get	{ return ServiceUrl.Host; }

			set	
			{ 
				UriBuilder ub = new UriBuilder(ServiceUrl);
				ub.Host = value;
				ServiceUrl = ub.Uri;
			}
		}

		/// <summary>
		/// Gets or sets the domain port.
		/// </summary>
		public int Port
		{
			get	{ return ServiceUrl.Port; }

			set 
			{ 
				UriBuilder ub = new UriBuilder(ServiceUrl);
				ub.Port = value;
				ServiceUrl = ub.Uri;
			}
		}

		/// <summary>
		/// Gets or sets whether the domain is enabled.
		/// </summary>
		public bool Enabled
		{
			get
			{
				string enableString = GetDomainAttribute(EnabledTag);
				return (enableString != null) ? Convert.ToBoolean(enableString) : false;
			}

			set { SetDomainAttribute(EnabledTag, value.ToString()); }
		}

		/// <summary>
		/// Gets or sets the url scheme.
		/// </summary>
		public string Scheme
		{
			get { return ServiceUrl.Scheme; }
			set 
			{ 
				UriBuilder ub = new UriBuilder(ServiceUrl);
				ub.Scheme = value;
				ServiceUrl = ub.Uri;
			}
		}

		/// <summary>
		/// Gets the domain url
		/// </summary>
		public Uri ServiceUrl
		{
			get 
			{ 
				string uriString = GetDomainAttribute(UriTag);
				return (uriString != null) ? new Uri(uriString) : new UriBuilder(defaultScheme, IPAddress.Any.ToString(), 80).Uri;
			}

			set { SetDomainAttribute(UriTag, value.ToString()); }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of this object.
		/// </summary>
		/// <param name="domainName"></param>
		public DomainConfig(string domainName)
		{
			this.domainName = domainName;
		}

		/// <summary>
		/// Initializes a new instance of this object.
		/// </summary>
		public DomainConfig()
		{
			Store store = Store.GetStore();
			this.domainName = store.GetDomain( store.DefaultDomain ).Name;
		}
		#endregion

		#region Private Methods
		private XmlElement GetServerElement(XmlElement rootElement)
		{
			XmlElement serverElement = null;
			foreach(XmlElement element in rootElement)
			{
				if (element.GetAttribute(NameTag) == domainName)
				{
					serverElement = element;
					break;
				}
			}

			return serverElement;
		}

		private string GetDomainAttribute(string tag)
		{
			XmlElement root = config.GetElement(SectionTag, ServersTag);
			XmlElement element = GetServerElement(root);
			return (element != null) ? element.GetAttribute(tag) : null;
		}

		private void SetDomainAttribute(string tag, string tagValue)
		{
			XmlElement root = config.GetElement(SectionTag, ServersTag);
			XmlElement element = GetServerElement(root);
			if (element == null)
			{
				element = root.OwnerDocument.CreateElement(ServerTag);
				element.SetAttribute(NameTag, domainName);
				root.AppendChild(element);
			}

			element.SetAttribute(tag, tagValue);
			config.SetElement(SectionTag, ServersTag, root);
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Sets all the parameters in one shot.
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="description"></param>
		/// <param name="serviceAddress"></param>
		/// <param name="enabled"></param>
		public void SetAttributes(string ID, string description, Uri serviceAddress, bool enabled)
		{
			XmlElement root = config.GetElement(SectionTag, ServersTag);
			XmlElement element = GetServerElement(root);
			if (element == null)
			{
				element = root.OwnerDocument.CreateElement(ServerTag);
				root.AppendChild(element);
			}

			element.SetAttribute(NameTag, domainName);
			element.SetAttribute(IDTag, ID);
			element.SetAttribute(DescriptionTag, description);
			element.SetAttribute(UriTag, serviceAddress.ToString());
			element.SetAttribute(EnabledTag, enabled.ToString());
			config.SetElement(SectionTag, ServersTag, root);
		}
		#endregion
	}

	/// <summary>
	/// Simias Domain Agent
	/// </summary>
	public class DomainAgent
	{
		#region Class Members
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(DomainAgent));
		private Store store = Store.GetStore();
		private DomainConfig domainConfiguration;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		public DomainAgent()
		{
			domainConfiguration = new DomainConfig(store.GetDomain(store.DefaultDomain).Name);
		}

		/// <summary>
		/// TODO: Remove this constructor once the iFolderService.cs file has been updated.
		/// </summary>
		/// <param name="config"></param>
		public DomainAgent(Configuration config) :
			this()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="domainName"></param>
		public DomainAgent(string domainName)
		{
			domainConfiguration = new DomainConfig(domainName);
		}
		#endregion

		/// <summary>
		/// Attach to an enterprise system.
		/// </summary>
		/// <param name="host">Url of the enterprise server.</param>
		/// <param name="user">User to provision on the server.</param>
		/// <param name="password">Password to validate user.</param>
		public void Attach(string host, string user, string password)
		{
			// Set the url to where the enterprise server is.
			Uri uhost = new Uri( host );

			// Create the domain service web client object.
			DomainService domainService = new DomainService();
			domainService.Url = host + "/DomainService.asmx";

			// provision user
			ProvisionInfo provisionInfo = domainService.ProvisionUser(user, password);
			if (provisionInfo == null)
				throw new ApplicationException("User does not exist on server.");

			// get domain info
			DomainInfo domainInfo = domainService.GetDomainInfo(provisionInfo.UserID);

			// create domain node
			Store store = Store.GetStore();
			Storage.Domain domain = store.AddDomainIdentity(provisionInfo.UserID,
				domainInfo.Name, domainInfo.ID, domainInfo.Description);

			// set the default domain
			string previousDomain = store.DefaultDomain;
			store.DefaultDomain = domainInfo.ID;

			try
			{
				// create roster if needed
				if (store.GetCollectionByID(domainInfo.RosterID) == null)
				{
					// create roster proxy
					CreateRosterProxy(store, domain, provisionInfo.UserID, domainInfo, uhost);
					log.Debug("Creating Roster Proxy: {0}", domainInfo.RosterName);
				}

				if (store.GetCollectionByID(provisionInfo.POBoxID) == null)
				{
					// create PO Box proxy
					CreatePOBoxProxy(store, domainInfo.ID, provisionInfo, uhost);
					log.Debug("Creating PO Box Proxy: {0}", provisionInfo.POBoxName);
				}

				// Set the host and port number in the configuration file.
				domainConfiguration = new DomainConfig(domainInfo.Name);
				domainConfiguration.SetAttributes(domain.ID, domain.Description, uhost, true);
			}
			catch(Exception e)
			{
				// restore the previous domain
				store.DefaultDomain = previousDomain;
				throw e;
			}
		}

		private void CreateRosterProxy(Store store, Storage.Domain domain, string userID, DomainInfo info, Uri host)
		{
			// Create a new roster
			Roster roster = new Roster(store, info.RosterID, domain);
			roster.Proxy = true;
			
			// sync information
			Property p = new Property(SyncCollection.RolePropertyName, SyncCollectionRoles.Slave);
			p.LocalProperty = true;
			roster.Properties.AddProperty(p);

			// url information
			p = new Property(SyncCollection.MasterUrlPropertyName, host);
			p.LocalProperty = true;
			roster.Properties.AddProperty(p);

			// Create roster member.
			Access.Rights rights = ( Access.Rights )Enum.Parse( typeof( Access.Rights ), info.MemberRights );
			Member member = new Member( info.MemberNodeName, info.MemberNodeID, userID, rights, null );
			member.Proxy = true;

			// commit
			roster.Commit( new Node[] { roster, member } );
		}

		private void CreatePOBoxProxy(Store store, string domainID, ProvisionInfo info, Uri host)
		{
			// Create a new POBox
			PostOffice.POBox poBox = new PostOffice.POBox(store, info.POBoxName, info.POBoxID, domainID);
			poBox.Proxy = true;
			
			// sync information
			Property p = new Property(SyncCollection.RolePropertyName, SyncCollectionRoles.Slave);
			p.LocalProperty = true;
			poBox.Properties.AddProperty(p);

			// url information
			p = new Property(SyncCollection.MasterUrlPropertyName, host);
			p.LocalProperty = true;
			poBox.Properties.AddProperty(p);

			// Create member.
			Access.Rights rights = ( Access.Rights )Enum.Parse( typeof( Access.Rights ), info.MemberRights );
			Member member = new Member( info.MemberNodeName, info.MemberNodeID, info.UserID, rights, null );
			member.Proxy = true;
			
			// commit
			poBox.Commit( new Node[] { poBox, member } );
		}

		/// <summary>
		/// Create the master on the server.
		/// </summary>
		/// <param name="collection">Collection to create on the enterprise server.</param>
		public void CreateMaster(SyncCollection collection)
		{
			// Construct the web client.
			DomainService domainService = new DomainService();
			domainService.Url = domainConfiguration.ServiceUrl.ToString() + "/DomainService.asmx";

			string rootID = null;
			string rootName = null;

			DirNode rootNode = collection.GetRootDirectory();
			if (rootNode != null)
			{
				rootID = rootNode.ID;
				rootName = rootNode.Name;
			}

			Member member = collection.Owner;

			string uriString = domainService.CreateMaster(collection.ID, collection.Name,
				rootID, rootName, member.UserID, member.Name, member.ID, member.Rights.ToString() );

			if (uriString == null)
				throw new ApplicationException("Unable to create remote master collection.");

			collection.MasterUrl = new Uri(uriString);
			collection.CreateMaster = false;
			collection.Commit();
		}

		/// <summary>
		/// Deletes the specified collection off of the enterprise server.
		/// </summary>
		/// <param name="collection">Collection to delete from the server.</param>
		public void DeleteMaster(Collection collection)
		{
			// Construct the web client.
			DomainService domainService = new DomainService();
			domainService.Url = domainConfiguration.ServiceUrl.ToString() + "/DomainService.asmx";
			domainService.DeleteMaster(collection.ID);
		}

		#region Properties
		/// <summary>
		/// Domain service URL
		/// </summary>
		public Uri ServiceUrl
		{
			get { return domainConfiguration.ServiceUrl; }
			set 
			{ 
				domainConfiguration.Scheme = value.Scheme;
				domainConfiguration.Host = value.Host;
				domainConfiguration.Port = value.Port;
			}
		}

		/// <summary>
		/// Is the server enterprise domain enabled for this client?
		/// </summary>
		public bool Enabled
		{
			get { return domainConfiguration.Enabled; }
			set { domainConfiguration.Enabled = value; }
		}
		#endregion
	}
}
