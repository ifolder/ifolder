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

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Channels;
using PostOffice = Simias.POBox;

namespace Simias.Domain
{
	/// <summary>
	/// Simias Domain Agent
	/// </summary>
	public class DomainAgent
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(DomainAgent));

		private static readonly string SectionName = "Domain";

		/// <summary>
		/// The suggested service url for the current machine.
		/// </summary>
		private static readonly Uri DefaultServiceUrl = (new UriBuilder("http",
			MyDns.GetHostName(), 6346, EndPoint)).Uri;

		private static readonly string UrlKeyName = "Service URL";
		
		/// <summary>
		/// The enabled state of using the domain service.
		/// </summary>
		private static readonly bool DefaultEnabled = false;

		private static readonly string EnabledKeyName = "Enabled";
		
		
		private Configuration config;
		private SimiasChannel channel;

		public DomainAgent(Configuration config)
		{
			this.config = config;
		}

		public void Attach(string host, string user, string password)
		{
			// disable any current domain
			this.Enabled = false;

			// update service URL
			UriBuilder url = new UriBuilder(this.ServiceUrl);
			url.Host = host;
			this.ServiceUrl = url.Uri;
			log.Debug("Updated Domain Service URL: {0}", ServiceUrl);

			// connect
			IDomainService service = Connect();

			// get domain info
			DomainInfo domainInfo = service.GetDomainInfo();
			log.Debug(domainInfo.ToString());

			// provision user
			ProvisionInfo provisionInfo = service.ProvisionUser(user, password);
			log.Debug(provisionInfo.ToString());

			Store store = new Store(config);

			// create domain node
			store.AddDomainIdentity(provisionInfo.UserID, domainInfo.Name,
				domainInfo.ID, domainInfo.Description);

			// create roster stub
			CreateSlave(store, domainInfo.ID, domainInfo.RosterID,
				domainInfo.RosterName, domainInfo.SyncServiceUrl);
			
			// create PO Box stub
			Collection cStub = CreateSlave(store, domainInfo.ID, provisionInfo.POBoxID,
				provisionInfo.POBoxName, domainInfo.SyncServiceUrl);

			// Get a POBox object from its created stub.
			PostOffice.POBox poBox = new PostOffice.POBox(store, cStub);
			poBox.POServiceUrl = domainInfo.POServiceUrl;

			// set the default domain
			store.DefaultDomain = domainInfo.ID;

			// enable the new domain
			this.Enabled = true;

			// clean-up
			store.Dispose();
			channel.Dispose();
			service = null;
		}

		private Collection CreateSlave(Store store, string domain, string id, string name, string url)
		{
			Collection c = new Collection(store, name, id, domain);
			
			// sync information
			Property pr = new Property(SyncCollection.RolePropertyName,
				SyncCollectionRoles.Slave);
			pr.LocalProperty = true;
			c.Properties.AddProperty(pr);
			
			Property pu = new Property(SyncCollection.MasterUrlPropertyName, new Uri(url));
			pu.LocalProperty = true;
			c.Properties.AddProperty(pu);

			// commit
			c.Sealed = true;
			c.IsStub = true;
			c.Commit();
			return c;
		}

		public void CreateMaster(SyncCollection collection)
		{
			// connect
			IDomainService service = Connect();

			string rootID = null;
			string rootName = null;

			DirNode rootNode = collection.GetRootDirectory();

			if (rootNode != null)
			{
				rootID = rootNode.ID;
				rootName = rootNode.Name;
			}

			string uriString = service.CreateMaster(collection.ID, collection.Name,
				rootID, rootName);

			if (uriString == null)
				throw new ApplicationException("Unable to create remote master collection.");

			collection.MasterUrl = new Uri(uriString);
			collection.CreateMaster = false;

			// TODO: bump collection, magic number, etc.
			collection.Properties.ModifyNodeProperty(PropertyTags.MasterIncarnation, 1);
			collection.Properties.ModifyNodeProperty(PropertyTags.LocalIncarnation, 1);
			collection.Commit();
			
			if (rootNode != null)
			{
				rootNode.Properties.ModifyNodeProperty(PropertyTags.MasterIncarnation, 1);
				rootNode.Properties.ModifyNodeProperty(PropertyTags.LocalIncarnation, 1);
				collection.Commit(rootNode);
			}

			// clean-up
			channel.Dispose();
			service = null;
		}

		private IDomainService Connect()
		{
			log.Debug("Connecting to Domain Service: {0}", ServiceUrl);

			if (ServiceUrl == null) return null;

			// properties
			SyncProperties props = new SyncProperties(config);

			Store store = new Store(config);

			// create channel
			channel = SimiasChannelFactory.GetInstance().GetChannel(store,
				ServiceUrl.Scheme, props.ChannelSinks);

			// create URL
			string url = ServiceUrl.ToString();

			IDomainService service = (IDomainService)Activator.GetObject(
				typeof(IDomainService), url);
			
			// clean-up
			store.Dispose();

			return service;
		}

		#region Properties
		
		public static string EndPoint
		{
			get { return "DomainService.rem"; }
		}

		public Uri ServiceUrl
		{
			get { return new Uri(config.Get(SectionName, UrlKeyName, DefaultServiceUrl.ToString())); }

			set { config.Set(SectionName, UrlKeyName, value.ToString()); }
		}

		public bool Enabled
		{
			get { return bool.Parse(config.Get(SectionName, EnabledKeyName, DefaultEnabled.ToString())); }

			set { config.Set(SectionName, EnabledKeyName, value.ToString()); }
		}

		#endregion
	}
}
