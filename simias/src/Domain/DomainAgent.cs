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
using System.Threading;
using System.Web;
using System.Xml;

using Simias;
using Simias.Authentication;
using Simias.Client;
using Simias.Storage;
using Simias.Sync;

using Novell.Security.ClientPasswordManager;

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
		/// Holds a reference to the simias configuration file.
		/// </summary>
		private Configuration config = Configuration.GetConfiguration();

		/// <summary>
		/// Identifier of the domain for which configuration information is associated with.
		/// </summary>
		private string domainID;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the domain ID.
		/// </summary>
		public string ID
		{
			get { return domainID; }
			set { domainID = value; SetDomainAttribute(IDTag, value); }
		}

		/// <summary>
		/// Gets or sets the domain name.
		/// </summary>
		public string Name
		{
			get { return GetDomainAttribute(NameTag); }
			set { SetDomainAttribute(NameTag, value ); }
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
			get	
			{ 
				Uri uri = ServiceUrl;
				return (uri != null) ? uri.Host : null; 
			}

			set	
			{ 
				Uri uri = ServiceUrl;
				UriBuilder ub = (uri != null) ? new UriBuilder(uri) : new UriBuilder(Uri.UriSchemeHttp, value);
				ub.Host = value;
				ServiceUrl = ub.Uri;
			}
		}

		/// <summary>
		/// Gets or sets the domain port.
		/// </summary>
		public int Port
		{
			get	
			{ 
				Uri uri = ServiceUrl;
				return (uri != null) ? uri.Port : -1; 
			}

			set 
			{ 
				Uri uri = ServiceUrl;
				UriBuilder ub = (uri != null) ? new UriBuilder(uri) : new UriBuilder(Uri.UriSchemeHttp, IPAddress.Any.ToString());
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
			get 
			{ 
				Uri uri = ServiceUrl;
				return (uri != null) ? uri.Scheme : Uri.UriSchemeHttp; 
			}

			set 
			{ 
				Uri uri = ServiceUrl;
				UriBuilder ub = (uri != null) ? new UriBuilder(uri) : new UriBuilder(Uri.UriSchemeHttp, IPAddress.Any.ToString());
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
				return (uriString != null) ? new Uri(uriString) : null;
			}

			set { SetDomainAttribute(UriTag, value.ToString()); }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of this object.
		/// </summary>
		/// <param name="domainID">Identifier of the domain.</param>
		public DomainConfig(string domainID)
		{
			this.domainID = domainID;
		}

		/// <summary>
		/// Initializes a new instance of this object.
		/// </summary>
		public DomainConfig()
		{
			Store store = Store.GetStore();
			domainID = store.DefaultDomain;
		}
		#endregion

		#region Private Methods
		private XmlElement GetServerElement(XmlElement rootElement)
		{
			XmlElement serverElement = null;
			foreach(XmlElement element in rootElement)
			{
				if (element.GetAttribute(IDTag) == domainID)
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
				element.SetAttribute(IDTag, domainID);
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
		/// <param name="name"></param>
		/// <param name="description"></param>
		/// <param name="serviceAddress"></param>
		/// <param name="enabled"></param>
		public void SetAttributes(string name, string description, Uri serviceAddress, bool enabled)
		{
			XmlElement root = config.GetElement(SectionTag, ServersTag);
			XmlElement element = GetServerElement(root);
			if (element == null)
			{
				element = root.OwnerDocument.CreateElement(ServerTag);
				root.AppendChild(element);
			}

			element.SetAttribute(IDTag, domainID);
			element.SetAttribute(NameTag, name);
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
	
		/// <summary>
		/// Service type for this service.
		/// </summary>
		private static string DomainServiceType = "Domain Service";
		private static string DomainServicePath = "/simias10/DomainService.asmx";

		private Store store = Store.GetStore();
		private DomainConfig domainConfiguration;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		public DomainAgent()
		{
			domainConfiguration = new DomainConfig(store.DefaultDomain);
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
		/// <param name="domainID">Identifier for the domain.</param>
		public DomainAgent(string domainID)
		{
			domainConfiguration = new DomainConfig(domainID);
		}
		#endregion

		/// <summary>
		/// Attach to an enterprise system.
		/// </summary>
		/// <param name="host">Url of the enterprise server.</param>
		/// <param name="user">User to provision on the server.</param>
		/// <param name="password">Password to validate user.</param>
		/// <returns>
		/// The Domain ID of the newly attached Domain
		/// </returns>
		public string Attach(string host, string user, string password)
		{
			// Get a URL to our service.
			Uri domainServiceUrl = WSInspection.GetServiceUrl( host, DomainServiceType );
			if ( domainServiceUrl == null )
			{
				// There was a failure in obtaining the service url. Try a hard coded one.
				domainServiceUrl = new Uri( Uri.UriSchemeHttp + Uri.SchemeDelimiter + host + DomainServicePath );
			}

			// Get just the path portion of the URL.
			string hostString = domainServiceUrl.ToString();
			int startIndex = hostString.LastIndexOf( "/" );
			Uri hostUri = new Uri( hostString.Remove( startIndex, hostString.Length - startIndex ) );

			// Create the domain service web client object.
			DomainService domainService = new DomainService();
			domainService.Url = domainServiceUrl.ToString();

			// Setup the credentials
			NetworkCredential myCred = new NetworkCredential(user, password); 
			CredentialCache myCache = new CredentialCache();
			myCache.Add(new Uri(domainService.Url), "Basic", myCred);
			domainService.Credentials = myCache;
			domainService.CookieContainer = new CookieContainer();
			domainService.PreAuthenticate = true;
			domainService.Timeout = 30000;
			
			int normalThreads = 9999;
			int	completionPortThreads = 8888;
			ThreadPool.GetAvailableThreads(out normalThreads, out completionPortThreads);
			
			log.Debug("Available threads: " + normalThreads.ToString());
			log.Debug("Calling " + domainService.Url + " to provision the user");

			// provision user
			ProvisionInfo provisionInfo = domainService.ProvisionUser(user, password);
			if (provisionInfo == null)
				throw new ApplicationException("User does not exist on server.");
				
			log.Debug("the user has been provisioned on the remote domain");

			// get domain info
			DomainInfo domainInfo = domainService.GetDomainInfo(provisionInfo.UserID);

			Store store = Store.GetStore();

			// create domain node
			Storage.Domain domain = 
				store.AddDomainIdentity(
					provisionInfo.UserID,
					domainInfo.Name, 
					domainInfo.ID, 
					domainInfo.Description,
					hostUri,
					Simias.Storage.Domain.DomainRole.Slave);

			// set the default domain
			string previousDomain = store.DefaultDomain;
			store.DefaultDomain = domainInfo.ID;

			// authentication was successful - save the credentials
			new NetCredential("iFolder", domainInfo.ID, true, user, password);

			try
			{
				if (store.GetCollectionByID(provisionInfo.POBoxID) == null)
				{
					// create PO Box proxy
					CreatePOBoxProxy(store, domainInfo.ID, provisionInfo, hostUri );
					log.Debug("Creating PO Box Proxy: {0}", provisionInfo.POBoxName);
				}

				// create roster if needed
				if (store.GetCollectionByID(domainInfo.RosterID) == null)
				{
					// create roster proxy
					CreateRosterProxy(store, domain, provisionInfo.UserID, domainInfo, hostUri );
					log.Debug("Creating Roster Proxy: {0}", domainInfo.RosterName);
				}

				// Set the host and port number in the configuration file.
				domainConfiguration = new DomainConfig(domainInfo.ID);
				domainConfiguration.SetAttributes(domain.Name, domain.Description, hostUri, true);

				store.LocalDb.Commit( domain );
			}
			catch(Exception e)
			{
				// restore the previous domain
				store.DefaultDomain = previousDomain;
				throw e;
			}
			return domainInfo.ID;
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

			// Create roster member.
			Access.Rights rights = ( Access.Rights )Enum.Parse( typeof( Access.Rights ), info.MemberRights );
			Member member = new Member( info.MemberNodeName, info.MemberNodeID, userID, rights, null );
			member.Proxy = true;
			member.IsOwner = true;

			// commit
			roster.Commit( new Node[] { roster, member } );
		}

		private void CreatePOBoxProxy(Store store, string domainID, ProvisionInfo info, Uri host)
		{
			// Create a new POBox
			PostOffice.POBox poBox = new PostOffice.POBox(store, info.POBoxName, info.POBoxID, domainID);
			poBox.Priority = 0;
			poBox.Proxy = true;
			
			// sync information
			Property p = new Property(SyncCollection.RolePropertyName, SyncCollectionRoles.Slave);
			p.LocalProperty = true;
			poBox.Properties.AddProperty(p);

			// Create member.
			Access.Rights rights = ( Access.Rights )Enum.Parse( typeof( Access.Rights ), info.MemberRights );
			Member member = new Member( info.MemberNodeName, info.MemberNodeID, info.UserID, rights, null );
			member.Proxy = true;
			member.IsOwner = true;
			
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
			Uri uri = domainConfiguration.ServiceUrl;
			if (uri == null)
			{
				throw new SimiasException("Domain URL is not set.");
			}

			domainService.Url = uri.ToString() + "/DomainService.asmx";
			Credentials cSimiasCreds = new Credentials(collection.ID);
			domainService.Credentials = cSimiasCreds.GetCredentials();

			if (domainService.Credentials == null)
				throw new ApplicationException("No credentials available for specified collection.");
			domainService.PreAuthenticate = true;

			string rootID = null;
			string rootName = null;

			DirNode rootNode = collection.GetRootDirectory();
			if (rootNode != null)
			{
				rootID = rootNode.ID;
				rootName = rootNode.Name;
			}

			Member member = collection.Owner;

			domainService.CreateMaster(
				collection.ID, 
				collection.Name, 
				rootID, rootName, 
				member.UserID, 
				member.Name, 
				member.ID, 
				member.Rights.ToString() );

			collection.CreateMaster = false;
			collection.Commit();
		}

		#region Properties
		/// <summary>
		/// Domain service URL
		/// </summary>
		public Uri ServiceUrl
		{
			get { return domainConfiguration.ServiceUrl; }
			set { domainConfiguration.ServiceUrl = value; }
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
