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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Storage;

namespace Simias.SimpleServer
{
	/// <summary>
	/// Class to initialize/verify a SimpleServer domain in the Collection Store
	/// </summary>
	public class Domain
	{
		#region Class Members

		/// <summary>
		/// GUID for this SimpleServer domain
		/// FIXME::This ID shouldn't be static but we don't have
		/// a way to look a domain by id
		/// </summary>
		private string id = "29ebbf9c-d9e8-4471-be80-df3dd90db0ff";

		/// <summary>
		/// Friendly name for the workgroup domain.
		/// </summary>
		private string domainName = "SimpleServer";
		private string hostAddress;
		private string description = "Simple Server domain";

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


		private string		serverDocumentPath = "SimpleServer.xml";
		private XmlDocument serverDoc;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the SimpleServer domain's unique ID
		/// </summary>
		public string ID
		{
			get { return(this.id); }
		}

		/// <summary>
		/// Gets the SimpleServer domain's friendly ID
		/// </summary>
		public string Name
		{
			get { return(this.domainName); }
		}

		/// <summary>
		/// Gets the SimpleServer domain's description
		/// </summary>
		public string Description
		{
			get { return(this.description); }
		}

		/// <summary>
		/// Gets the SimpleServer domain's host address
		/// </summary>
		public string Host
		{
			get { return(this.hostAddress); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for creating a new mDnsDomain object.
		/// </summary>
		internal Domain( bool init )
		{
			if ( init == true )
			{
				this.Init();
			}
		}

		/// <summary>
		/// Constructor for creating a new mDnsDomain object.
		/// </summary>
		/// <param name="description">String that describes this domain.</param>
		internal Domain( bool init, string description ) 
		{
			this.description = description;

			if ( init == true )
			{
				this.Init();
			}
		}
		#endregion

		internal void Init()
		{
			hostAddress = MyDns.GetHostName();
			log.Debug("  My Address: " + hostAddress);
			Store store = Store.GetStore();


			// Find which member should be the domain owner
			try
			{
				// Load the configuration document from the file.
				serverDoc = new XmlDocument();
				serverDoc.Load(serverDocumentPath);

				XmlElement domainElement = serverDoc.DocumentElement;
				domainElement.GetAttributeNode("Owner");

				this.domainName = domainElement.GetAttribute("Name");
				string tmpDescription = "";
				try
				{
					tmpDescription = domainElement.GetAttribute("Description");
				}
				catch{}
				if ( tmpDescription != null )
				{
					this.description = tmpDescription;
				}


				//
				// The current member of the local database will be
				// the SimpleServer domain owner
				//

				// Find the Member designated as owner
				XmlNode root = serverDoc.DocumentElement;
				XmlNodeList nodeList = 
					root.SelectNodes("/Member/Owner");

				XmlNode xmlOwner = null;
				foreach(XmlNode xmlNode in nodeList)
				{
					xmlOwner = xmlNode;
					break;
				}

				if (xmlOwner == null)
				{
					throw new Exception("No member with owner status specified");
				}
				
				string owner = xmlOwner.Attributes["Name"].Name;


				LocalDatabase ldb = store.GetDatabaseObject();
				Member ldbMember = ldb.GetCurrentMember();

				//
				// Verify the SimpleServer domain exists
				//
			
				Uri localUri = new Uri("http://" + hostAddress + "/simias10");
				Simias.Storage.Domain rDomain = store.GetDomain( this.id );
				if (rDomain == null)
				{
					// Create the mDnsDomain and add an identity mapping.
					/* Reliase when Mike checks in Role changes
					store.AddDomainIdentity(
						ldbMember.ID,
						this.domainName,
						this.id, 
						this.description,
						localUri,
						Simias.Storage.Domain.DomainRole.Master);
					*/

					store.AddDomainIdentity(
						ldbMember.ID,
						this.domainName,
						this.id, 
						this.description,
						localUri);
				}

				//
				// Verify the SimpleServer roster
				//

				Roster ssRoster = null;
				Member rMember;
				ArrayList changeList = new ArrayList();

				try
				{
					ssRoster = rDomain.GetRoster( store );
				}
				catch{}
				if (ssRoster == null)
				{
					ssRoster = new Roster( store, store.GetDomain( this.id ));
					rMember = new Member( ldbMember.Name, ldbMember.ID, Access.Rights.Admin );
					rMember.IsOwner = true;
					//rMember.Properties.ModifyProperty( "POBox", hostAddress );
					changeList.Add(ssRoster);
					//mdnsRoster.Commit( new Node[] { mdnsRoster, rMember } );
				}
				else
				{
					// Make sure the member exists
					rMember = ssRoster.GetMemberByID( ldbMember.ID );
					if (rMember == null)
					{
						rMember = new Member( ldbMember.Name, ldbMember.ID, Access.Rights.Admin );
						rMember.IsOwner = true;
					}
				}

				//rMember.Properties.ModifyProperty( "POBox", hostAddress );
				changeList.Add(rMember);
				ssRoster.Commit( changeList.ToArray( typeof( Node ) ) as Node[] );

				//
				// Verify the POBox for the local SimpleServer owner
				//
			
				Member pMember;
				Simias.POBox.POBox poBox = null;
				string poBoxName = "POBox:" + this.id + ":" + ldbMember.ID;

				try
				{
					poBox = Simias.POBox.POBox.FindPOBox( store, this.ID, ldbMember.ID );
				}
				catch{}
				if (poBox == null)
				{
					poBox = new Simias.POBox.POBox( store, poBoxName, this.id );
					pMember = 
						new Member( ldbMember.Name, ldbMember.ID, Access.Rights.ReadWrite );
					pMember.IsOwner = true;
					poBox.Commit(new Node[] { poBox, pMember });
				}
				else
				{
					// verify member in POBox
					pMember = poBox.GetMemberByID( ldbMember.ID );
					if (pMember == null)
					{
						pMember = 
							new Member( ldbMember.Name, ldbMember.ID, Access.Rights.ReadWrite );
						pMember.IsOwner = true;
						poBox.Commit(new Node[] { pMember });

					}
				}
			}
			catch(Exception e1)
			{
				log.Error(e1.Message);
				log.Error(e1.StackTrace);

				throw e1;
				// FIXME:: rethrow the exception
			}			
		}

		#region Public Methods

		/// <summary>
		/// Synchronize the SimpleServer.xml members to the Simias member list
		/// </summary>
		public void SynchronizeMembers()
		{
			return;
		}

		/// <summary>
		/// Obtains the string representation of this instance.
		/// </summary>
		/// <returns>The friendly name of the domain.</returns>
		public override string ToString()
		{
			return this.domainName;
		}
		#endregion
	}
}
