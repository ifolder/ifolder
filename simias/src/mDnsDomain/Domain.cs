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
//using Simias.Client;
using Simias.Storage;

namespace Simias.mDns
{
	/// <summary>
	/// Class that represents a mDns Domain object in the Collection Store.
	/// </summary>
	public class Domain
	{
		#region Class Members

		/// <summary>
		/// Well known identitifer for mDns workgroup.
		/// </summary>
		public static readonly string ID = "74d3a71f-daae-4a36-b9f3-6466081f6401";

		/// <summary>
		/// Friendly name for the workgroup domain.
		/// </summary>
		private string mDnsDomainName = "Rendezvous";

		private string hostAddress;
		private string description = "";
		private string mDnsHostName;
		private string mDnsUserName;
		private string mDnsUserID;
		private string mDnsPOBoxID;

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Properties

		/*
		/// <summary>
		/// Gets the mDnsDomain's ID
		/// </summary>
		public static string ID
		{
			get { return(this.id); }
		}
		*/

		/// <summary>
		/// Gets the mDnsDomain's friendly ID
		/// </summary>
		public string Name
		{
			get { return( this.mDnsDomainName ); }
		}

		/// <summary>
		/// Gets the mDnsDomain's description
		/// </summary>
		public string Description
		{
			get { return( this.description ); }
		}

		/// <summary>
		/// Gets the current mDns Host Name
		/// </summary>
		public string Host
		{
			get { return( this.mDnsHostName ); }
		}

		/// <summary>
		/// Gets the mDnsDomain's current user
		/// </summary>
		public string User
		{
			get { return( this.mDnsUserName ); }
		}
		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for creating a new mDnsDomain object.
		/// </summary>
		internal Domain( bool init )
		{
			mDnsHostName = Environment.MachineName + ".local";
			mDnsUserName = Environment.UserName + "@" + mDnsHostName;

			description = 
				Environment.UserName +
				"@" +
				Environment.MachineName +
				"'s Rendezvous Workgroup Domain";

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
			mDnsHostName = Environment.MachineName + ".local";
			mDnsUserName = Environment.UserName + "@" + mDnsHostName;
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
			log.Debug( "  My Address: " + hostAddress );
			Store store = Store.GetStore();

			try
			{
				Uri localUri = new Uri("http://" + hostAddress);

				//
				// Verify the local Rendezvous user exists in the local database
				//
				LocalDatabase ldb = store.GetDatabaseObject();
				Member ldbMember;
				Node memberNode = ldb.GetSingleNodeByName( mDnsUserName );
				if (memberNode == null)
				{
					// Create a local member which is the owner of the mDnsDomain
					ldbMember = new Member( mDnsUserName, Guid.NewGuid().ToString(), Access.Rights.Admin );
					ldbMember.IsOwner = true;

					// Save the local database changes.
					ldb.Commit( new Node[] { ldbMember } );
				}
				else
				{
					ldbMember = new Member(memberNode);
				}

				mDnsUserID = ldbMember.ID;

				//
				// Verify the Rendezvous workgroup domain exists
				//

				Simias.Storage.Domain rDomain = store.GetDomain( ID );
				if (rDomain == null)
				{
					// Create the mDnsDomain and add an identity mapping.
					store.AddDomainIdentity(
						ldbMember.ID,
						this.mDnsDomainName, 
						Simias.mDns.Domain.ID, 
						this.description,
						localUri,
						Simias.Sync.SyncRoles.Master );
				}

				//
				// Verify the Rendezvous workgroup roster
				//

				Roster mdnsRoster = null;
				Member rMember;
				ArrayList changeList = new ArrayList();

				try
				{
					mdnsRoster = rDomain.Roster;
				}
				catch{}
				if (mdnsRoster == null)
				{
					mdnsRoster = new Roster( store, store.GetDomain( Simias.mDns.Domain.ID ) );
					rMember = new Member( ldbMember.Name, ldbMember.ID, Access.Rights.Admin );
					rMember.IsOwner = true;
					//rMember.Properties.ModifyProperty( "POBox", hostAddress );
					changeList.Add( mdnsRoster );
					//mdnsRoster.Commit( new Node[] { mdnsRoster, rMember } );
				}
				else
				{
					// Make sure the member exists
					rMember = mdnsRoster.GetMemberByID( ldbMember.ID );
					if (rMember == null)
					{
						rMember = new Member( ldbMember.Name, ldbMember.ID, Access.Rights.Admin );
						rMember.IsOwner = true;
						//mdnsRoster.Commit( new Node[] { rMember } );
					}
				}

				rMember.Properties.ModifyProperty( "POBox", hostAddress );
				changeList.Add(rMember);
				mdnsRoster.Commit( changeList.ToArray( typeof( Node ) ) as Node[] );

				//
				// Verify the POBox for the local Rendezvous user
				//
			
				Member pMember;
				Simias.POBox.POBox poBox = null;
				string poBoxName = "POBox:" + Simias.mDns.Domain.ID + ":" + ldbMember.ID;

				try
				{
					poBox = Simias.POBox.POBox.FindPOBox( store, Simias.mDns.Domain.ID, ldbMember.ID );
				}
				catch{}
				if (poBox == null)
				{
					poBox = new Simias.POBox.POBox( store, poBoxName, ID );
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

				mDnsPOBoxID = poBox.ID;
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
		/// Creates the local/master Rendezvous domain.
		/// The Member, Roster and PO Box also gets created as well
		/// </summary>
		/// <returns>Throws an exception if the domain can't be created</returns>
		public void Create()
		{
			string myAddress = MyDns.GetHostName();
			log.Debug("  My Address: " + myAddress);
			Store store = Store.GetStore();

			try
			{
				Uri localUri = new Uri("http://" + myAddress);

				//
				// Verify the local Rendezvous user exists in the local database
				LocalDatabase ldb = store.GetDatabaseObject();

				ldb.GetSingleNodeByName( mDnsUserName );

				// Create a local member which is the owner of the mDnsDomain
				Member member = new Member( mDnsUserName, Guid.NewGuid().ToString(), Access.Rights.Admin );
				member.IsOwner = true;
				mDnsUserID = member.ID;

				// Save the local database changes.
				ldb.Commit( new Node[] { member } );

				// Create the mDnsDomain and add an identity mapping.
				store.AddDomainIdentity(
					member.ID, 
					this.mDnsDomainName, 
					Simias.mDns.Domain.ID,
					this.description,
					localUri,
					Simias.Sync.SyncRoles.Master );

				// Create an empty roster for the mDns domain.
				Roster mdnsRoster = new Roster( store, store.GetDomain( ID ));
				Member rMember = new Member( member.Name, member.ID, Access.Rights.Admin );
				rMember.IsOwner = true;
				mdnsRoster.Commit( new Node[] { mdnsRoster, rMember } );

				// Create the POBox for the user
				string poBoxName = "POBox:" + ID + ":" + member.ID;

				Simias.POBox.POBox poBox = 
					new Simias.POBox.POBox( store, poBoxName, ID );
				Member pMember = new Member( member.Name, member.ID, Access.Rights.ReadWrite );
				pMember.IsOwner = true;
				poBox.Commit(new Node[] { poBox, pMember });
				mDnsPOBoxID = poBox.ID;
			}
			catch(Exception e1)
			{
				log.Error(e1.Message);
				log.Error(e1.StackTrace);

				throw e1;
				// FIXME:: rethrow the exception
			}			
		}

		/// <summary>
		/// Checks if the local/master Rendezvous domain exists.
		/// </summary>
		/// <returns>true if the domain exists otherwise false</returns>
		public bool Exists()
		{
			bool exists = false;
			Simias.Storage.Domain mdnsDomain = null;

			try
			{
				Store store = Store.GetStore();
				mdnsDomain = store.GetDomain( ID );
				if ( mdnsDomain != null )
				{
					Roster roster = mdnsDomain.Roster;
					Member member = roster.GetMemberByName( mDnsUserName );
					mDnsUserID = member.ID;
					Simias.POBox.POBox pobox = 
						Simias.POBox.POBox.FindPOBox( store, ID, member.ID );
					mDnsPOBoxID = pobox.ID;
					exists = true;
				}
			}
			catch{}
			return exists;
		}

		/// <summary>
		/// Obtains the string representation of this instance.
		/// </summary>
		/// <returns>The friendly name of the domain.</returns>
		public override string ToString()
		{
			return this.mDnsDomainName;
		}
		#endregion
	}
}
