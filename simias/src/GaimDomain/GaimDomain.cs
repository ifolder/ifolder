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
 *  Author(s):
 *		Boyd Timothy <btimothy@novell.com>
 *		Brady Anderson <banderso@novell.com>
 *		(this code is a mostly copy-n-paste from SimpleServer code, which
 *		 Brady wrote)
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Client;

namespace Simias.Gaim
{
	/// <summary>
	/// Class to initialize/verify a Gaim domain in the Collection Store
	/// </summary>
	public class GaimDomain
	{
		#region Class Members

		/// <summary>
		/// GUID for this Gaim domain
		/// </summary>
		private string id = "";

		/// <summary>
		/// Friendly name for the workgroup domain.
		/// </summary>
		private string domainName = "Gaim Buddy List";
		private string hostAddress;
		private string description = "This domain consists of buddies in your Gaim Buddy List.  Add/Remove users in Gaim to add them to this domain.";
		private string ownerMember;

/*
		private string firstName;
		private string lastName;
		private string emailAddress;
		private string im;
*/

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

/*		private string serverDocumentPath = "../../etc/SimpleServer.xml";*/
/*		private XmlDocument serverDoc;*/

		#endregion

		#region Properties

		/// <summary>
		/// Gets the Gaim domain's unique ID
		/// </summary>
		public string ID
		{
			get { return(this.id); }
		}

		/// <summary>
		/// Gets the Gaim domain's friendly ID
		/// </summary>
		public string Name
		{
			get { return(this.domainName); }
		}

		/// <summary>
		/// Gets the Gaim domain's description
		/// </summary>
		public string Description
		{
			get { return(this.description); }
		}

		/// <summary>
		/// Gets the Gaim domain's host address
		/// </summary>
		public string Host
		{
			get { return(this.hostAddress); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for creating a new GaimDomain object.
		/// </summary>
		public GaimDomain( bool init )
		{
			if ( init == true )
			{
				this.Init();
			}
		}

		/// <summary>
		/// Constructor for creating a new GaimDomain object.
		/// </summary>
		/// <param name="description">String that describes this domain.</param>
		internal GaimDomain( bool init, string description ) 
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
			//log.Debug("  My Address: " + hostAddress);
			Store store = Store.GetStore();

			try
			{
				// FIXME: Possibly ask Gaim what the user's screenname is for
				// their default (AOL) account.
				if (ownerMember == null || ownerMember == "")
				{
					ownerMember = "GaimDomainOwner";
					// throw new Exception("Couldn't determine host username");
				}

				// The current owner of the local database will be the Gaim
				// Domain's owner.
				Member ldbMember = null;
				LocalDatabase ldb = store.GetDatabaseObject();
				
				ICSList memberList = ldb.GetNodesByName(ownerMember);
				foreach(ShallowNode shallowNode in memberList)
				{
					Node cNode = new Node(ldb, shallowNode);
					Simias.Storage.Property simpleProp =
						cNode.Properties.GetSingleProperty("GaimDomainOwner");
					if (simpleProp != null)
					{
						ldbMember = new Member(cNode);
						break;
					}
				}

				if (ldbMember == null)
				{
					// Create a local member which is the owner of the Gaim Domain
					ldbMember = new Member(ownerMember, Guid.NewGuid().ToString(),
										   Access.Rights.Admin);
					ldbMember.IsOwner = false;
					
					Simias.Storage.Property simpleProp =
						new Property("GaimDomainOwner", true);
					simpleProp.LocalProperty = true;
					ldbMember.Properties.AddProperty(simpleProp);
					
					ldb.Commit(new Node[] {ldbMember});
				}

				//
				// Verify the GaimDomain exists
				//
				
				Simias.Storage.Domain gaimDomain =
					this.GetGaimDomain(true, ldbMember.ID);
					
				if (gaimDomain == null)
				{
					log.Error("Coudln't create or verify the Gaim domain");
					return;
				}

				//
				// Verify/Create the Gaim Domain Roster
				//				
				Simias.Storage.Roster gaimDomainRoster = null;
				Member rMember;
				ArrayList changeList = new ArrayList();
				
				try
				{
					gaimDomainRoster = gaimDomain.Roster;
				}
				catch{}
				if (gaimDomainRoster == null)
				{
					//
					// Create an empty roster for the Gaim Domain
					//
					gaimDomainRoster = new Roster(store, gaimDomain);
					rMember = new Member(ldbMember.Name, ldbMember.ID,
										 Access.Rights.Admin);
					rMember.IsOwner = true;
					changeList.Add(rMember);
					
					//
					// Make sure this is an Address Book
					//
					gaimDomainRoster.SetType(gaimDomainRoster, "AB:AddressBook");

					changeList.Add(gaimDomainRoster);
				}
				else
				{
					// Make sure the member exists
					rMember = gaimDomainRoster.GetMemberByID(ldbMember.ID);
					if (rMember == null)
					{
						rMember = new Member(ldbMember.Name, ldbMember.ID,
											 Access.Rights.Admin);
						rMember.IsOwner = true;
						changeList.Add(rMember);
					}
				}
				
				gaimDomainRoster.Commit(changeList.ToArray(typeof(Node)) as Node[]);


				//
				// Verify the POBox for the local SimpleServer owner
				//

				Member pMember;
				Simias.POBox.POBox poBox = null;
				string poBoxName = "POBox:" + gaimDomain.ID + ":" + ldbMember.ID;

				try
				{
					poBox = Simias.POBox.POBox.FindPOBox( store, gaimDomain.ID, ldbMember.ID );
				}
				catch{}
				if (poBox == null)
				{
					poBox = new Simias.POBox.POBox( store, poBoxName, gaimDomain.ID );
					poBox.CreateMaster = false;

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

				//throw e1;
				// FIXME:: rethrow the exception
			}			
		}


		#region Public Methods



		/// <summary>
		/// Method to get the Gaim Domain
		/// If the the domain does not exist and the create flag is true
		/// the domain will be created.  If create == false, ownerID is ignored
		/// </summary>
		public Simias.Storage.Domain GetGaimDomain( bool create, string ownerID )
		{
			//
			//  Check if the Gaim Domain exists in the store
			//

			Simias.Storage.Domain gaimDomain = null;

			try
			{
				Store store = Store.GetStore();

				foreach( ShallowNode sNode in store.GetDomainList() )
				{
					Simias.Storage.Domain tmpDomain = store.GetDomain( sNode.ID );
					Property p = tmpDomain.Properties.GetSingleProperty( "GaimDomain" );
					if ( p != null && (bool) p.Value == true )
					{
						gaimDomain = tmpDomain;
						this.id = tmpDomain.ID;
						break;
					}
				}

				if ( gaimDomain == null && create == true )
				{
					string id = Guid.NewGuid().ToString();
					Uri localUri = Manager.LocalServiceUrl;

					gaimDomain =
						store.AddDomainIdentity(
							ownerID,
							this.domainName,
							id, 
							this.description,
							localUri,
							Simias.Sync.SyncRoles.Master );

					if ( gaimDomain != null )
					{
						Property p = new Property( "GaimDomain", true );
						p.LocalProperty = true;
						gaimDomain.Properties.ModifyProperty( p );
						store.GetDatabaseObject().Commit( gaimDomain );
						this.id = gaimDomain.ID;
					}
				}
			}
			catch(Exception ggd)
			{
				log.Error( ggd.Message );
				log.Error( ggd.StackTrace );
			}

			return gaimDomain;
		}



		/// <summary>
		/// Method to get the Gaim Domain Roster
		/// </summary>
		public Simias.Storage.Roster GetGaimRoster()
		{

			Simias.Storage.Domain gaimDomain = GetGaimDomain( false, "" );
			
			if (gaimDomain == null)
			{
				log.Error("GetGaimRoster(): The Gaim Domain does not exist");
				return null;
			}

			Simias.Storage.Roster gaimDomainRoster = null;
			try
			{
				gaimDomainRoster = gaimDomain.Roster;
			}
			catch{}
			
			return gaimDomainRoster;
		}

		/// <summary>
		/// Synchronize the Gaim Domain members to the Simias member list
		/// </summary>
		public void SynchronizeMembers()
		{
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
