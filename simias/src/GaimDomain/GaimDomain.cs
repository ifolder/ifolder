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
		/// Well known ID for Gaim Workgroup Domain
		/// </summary>
		public static readonly string ID = "4a9ff9d6-8139-11d9-960e-000d936ac9c4";

		/// <summary>
		/// Friendly name for the workgroup domain.
		/// </summary>
		private string domainName = "Gaim Buddy List";
		private string hostAddress;
		private string description = "";
		private string hostName;
		private string userName;
		private string userID;
		private string poBoxID;

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Properties

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
			get { return(this.hostName); }
		}
		
		/// <summary>
		/// Gets the Gaim Domain's current user
		/// </summary>
		public string User
		{
			get { return( this.userName ); }
		}
		#endregion

		#region Constructors
		
		/// <summary>
		/// Constructor for creating a new Gaim Domain object.
		/// </summary>
		internal GaimDomain( bool init )
		{
			hostName = Environment.MachineName;
			userName = Environment.UserName + ".gaim";

			description = 
				Environment.UserName +
				"'s Gaim Buddy List Domain";

			if ( init == true )
			{
				this.Init();
			}
		}

		/// <summary>
		/// Constructor for creating a new Gaim Domain object.
		/// </summary>
		/// <param name="description">String that describes this domain.</param>
		internal GaimDomain( bool init, string description ) 
		{
			hostName = Environment.MachineName;
			userName = Environment.UserName + ".gaim";
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
			Store store = Store.GetStore();

			try
			{
				Uri localUri = Manager.LocalServiceUrl;

				//
				// Verify the local Rendezvous user exists in the local database
				//
				LocalDatabase ldb = store.GetDatabaseObject();
				Member ldbMember;
				Node memberNode = ldb.GetSingleNodeByName( userName );
				if (memberNode == null)
				{
					// Create a local member which is the owner of the Gaim Domain
					ldbMember = new Member( userName, Guid.NewGuid().ToString(), Access.Rights.Admin );
					ldbMember.IsOwner = true;

					// Save the local database changes.
					ldb.Commit( new Node[] { ldbMember } );
				}
				else
				{
					ldbMember = new Member( memberNode );
				}

				userID = ldbMember.ID;

				//
				// Verify the Gaim workgroup domain exists
				//

				Simias.Storage.Domain rDomain = store.GetDomain( ID );
				if (rDomain == null)
				{
					// Create the Gaim Workgroup Domain
					rDomain = 
						new Simias.Storage.Domain(
							store, 
							this.domainName,
							Simias.Gaim.GaimDomain.ID,
							this.description, 
							Simias.Sync.SyncRoles.Master, 
							localUri );

					rDomain.SetType( rDomain, "GaimDomain" );
					rDomain.SetType( rDomain, "AB:AddressBook" );
					rDomain.SetType( rDomain, "Workgroup" );

					// Create the owner member for the domain.
					Member member = 
						new Member(
							userName, 
							ldbMember.ID,
							Access.Rights.Admin );

					member.IsOwner = true;

					rDomain.Commit( new Node[] { rDomain, member } );

					// Create the name mapping.
					store.AddDomainIdentity( rDomain.ID, member.UserID );
				}

				//
				// Verify the POBox for the local Rendezvous user
				//
			
				Member pMember;
				Simias.POBox.POBox poBox = null;
				string poBoxName = "POBox:" + Simias.Gaim.GaimDomain.ID + ":" + ldbMember.ID;

				try
				{
					poBox = Simias.POBox.POBox.FindPOBox( store, Simias.Gaim.GaimDomain.ID, ldbMember.ID );
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

				poBoxID = poBox.ID;
			}
			catch( Exception e1 )
			{
				log.Error(e1.Message);
				log.Error(e1.StackTrace);

				throw e1;
				// FIXME:: rethrow the exception
			}			
		}

		#region Public Methods

		/// <summary>
		/// Checks if the local/master Gaim Domain exists.
		/// </summary>
		/// <returns>true if the domain exists otherwise false</returns>
		public bool Exists()
		{
			bool exists = false;
			Simias.Storage.Domain gaimDomain = null;

			try
			{
				Store store = Store.GetStore();
				gaimDomain = store.GetDomain( ID );
				if ( gaimDomain != null )
				{
					userID = gaimDomain.GetMemberByName( userName ).ID;
					Simias.POBox.POBox pobox = 
						Simias.POBox.POBox.FindPOBox( store, ID, userID );
					poBoxID = pobox.ID;
					exists = true;
				}
			}
			catch{}
			return exists;
		}

		/// <summary>
		/// Method to get the Gaim Domain
		/// </summary>
		public Simias.Storage.Domain GetDomain()
		{
			//
			//  Check if the Gaim Domain exists in the store
			//
			Simias.Storage.Domain gaimDomain = null;

			try
			{
				Store store = Store.GetStore();
				gaimDomain = store.GetDomain(ID);
				if (gaimDomain != null)
				{
					return gaimDomain;
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
