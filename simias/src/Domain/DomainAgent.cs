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
		private static string defaultScheme = "http";

		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(DomainAgent));
		private Store store = Store.GetStore();
		private string domainName;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		public DomainAgent()
		{
			this.domainName = store.GetDomain(store.DefaultDomain).Name;
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
			this.domainName = domainName;
		}
		#endregion

		/// <summary>
		/// Attach to an enterprise system.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="user"></param>
		/// <param name="password"></param>
		public void Attach(string host, string user, string password)
		{
			// Defaults
			string ipAddress = host;
			int port = 80;

			// Determine if there is a port set on the host entry.
			int length = host.IndexOf(':');
			if (length != -1)
			{
				ipAddress = host.Substring(0, length);
				port = Convert.ToInt32(host.Substring(length + 1));
			}

			// Set the url to where the enterprise server is.
			UriBuilder ub = new UriBuilder( defaultScheme, ipAddress, port);

			// Create the domain service web client object.
			DomainService domainService = new DomainService();
			domainService.Url = new Uri(ub.Uri, "DomainService.asmx").ToString();

			// get domain info
			DomainInfo domainInfo = domainService.GetDomainInfo();
			domainName = domainInfo.Name;

			// provision user
			ProvisionInfo provisionInfo = domainService.ProvisionUser(user, password);
			if (provisionInfo == null)
				throw new ApplicationException("User does not exist on server.");

			Store store = Store.GetStore();

			// create domain node
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
					CreateRosterProxy(store, domain, domainInfo.RosterID);
					log.Debug("Creating Roster Proxy: {0}", domainInfo.RosterName);
				}

				if (store.GetCollectionByID(provisionInfo.POBoxID) == null)
				{
					// create PO Box proxy
					CreatePOBoxProxy(store, domainInfo.ID, provisionInfo.POBoxID, provisionInfo.POBoxName);
					log.Debug("Creating PO Box Proxy: {0}", provisionInfo.POBoxName);
				}

				// Set the host and port number in the configuration file.
				DomainConfig dConf = new DomainConfig(domainInfo.Name);
				dConf.SetAttributes(domain.ID, domain.Description, ub.Uri, true);
			}
			catch(Exception e)
			{
				// restore the previous domain
				store.DefaultDomain = previousDomain;
				throw e;
			}
		}

		private void CreateRosterProxy(Store store, Storage.Domain domain, string id)
		{
			// Create a new roster
			Roster roster = new Roster(store, id, domain);
			
			// sync information
			Property pr = new Property(SyncCollection.RolePropertyName, SyncCollectionRoles.Slave);
			pr.LocalProperty = true;
			roster.Properties.AddProperty(pr);
			
			// commit
			roster.Proxy = true;
			roster.Commit();
		}

		private void CreatePOBoxProxy(Store store, string domain, string id, string name)
		{
			// Create a new POBox
			PostOffice.POBox poBox = new PostOffice.POBox(store, name, id, domain);
			
			// sync information
			Property pr = new Property(SyncCollection.RolePropertyName, SyncCollectionRoles.Slave);
			pr.LocalProperty = true;
			poBox.Properties.AddProperty(pr);
			
			// commit
			poBox.Proxy = true;
			poBox.Commit();
		}

		/// <summary>
		/// Create the master on the server.
		/// </summary>
		/// <param name="collection"></param>
		public void CreateMaster(SyncCollection collection)
		{
			// Get the configuration information for this domain.
			DomainConfig dConf = new DomainConfig(domainName);

			// Construct the web client.
			DomainService domainService = new DomainService();
			domainService.Url = new Uri(dConf.ServiceUrl, "DomainService.asmx").ToString();

			string rootID = null;
			string rootName = null;

			DirNode rootNode = collection.GetRootDirectory();
			if (rootNode != null)
			{
				rootID = rootNode.ID;
				rootName = rootNode.Name;
			}

			string uriString = domainService.CreateMaster(collection.ID, collection.Name,
				rootID, rootName, collection.Owner.UserID);

			if (uriString == null)
				throw new ApplicationException("Unable to create remote master collection.");

			collection.MasterUrl = new Uri(uriString);
			collection.CreateMaster = false;
			collection.Commit();
		}

		#region Properties
		/// <summary>
		/// Domain service URL
		/// </summary>
		public Uri ServiceUrl
		{
			get { return new DomainConfig(domainName).ServiceUrl; }
			set 
			{ 
				DomainConfig dConf = new DomainConfig(domainName);
				dConf.Scheme = value.Scheme;
				dConf.Host = value.Host;
				dConf.Port = value.Port;
			}
		}

		/// <summary>
		/// Is the server enterprise domain enabled for this client?
		/// </summary>
		public bool Enabled
		{
			get { return new DomainConfig(domainName).Enabled; }
			set { new DomainConfig(domainName).Enabled = value; }
		}
		#endregion
	}
}
