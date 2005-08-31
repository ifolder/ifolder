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
using System.Threading;
using System.Xml;

using Simias;
using Simias.Storage;
using Simias.Sync;

//using Novell.AddressBook;

namespace Simias.Server
{
	/// <summary>
	/// Class to initialize/verify a SimpleServer domain in the Collection Store
	/// </summary>
	public class Domain
	{
		#region Class Members

		/// <summary>
		/// GUID for this SimpleServer domain
		/// </summary>
		private string id = "";

		/// <summary>
		/// Friendly name for the workgroup domain.
		/// </summary>
		private string domainName = "Simias Server";
		private string hostAddress;
		private string description = "Stand-alone server for Simias";
		private string adminName = "ServerAdmin";
		private string adminPassword = "root";

		private readonly string DomainSection = "Domain";


		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Properties

		/// <summary>
		/// Gets the server's unique ID
		/// </summary>
		public string ID
		{
			get { return( this.id ); }
		}

		/// <summary>
		/// Gets the server's friendly ID
		/// </summary>
		public string Name
		{
			get { return( this.domainName ); }
		}

		/// <summary>
		/// Gets the server's description
		/// </summary>
		public string Description
		{
			get { return( this.description ); }
		}

		/// <summary>
		/// Gets the server's host address
		/// </summary>
		public string Host
		{
			get { return( this.hostAddress ); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for creating a new Simias Server Domain object.
		/// </summary>
		internal Domain( bool init )
		{
			if ( init == true )
			{
				this.Init();
			}
		}

		/// <summary>
		/// Constructor for creating a new Simias Server Domain object.
		/// </summary>
		/// <param name="init"></param>
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
			//hostAddress = MyDns.GetHostName();
			Store store = Store.GetStore();

			try
			{
				//
				// Verify the Simias Server domain exists if it
				// doesn't go ahead and create it.
				//
			
				Simias.Storage.Domain rDomain = this.GetSimiasServerDomain( true );
				if ( rDomain == null )
				{
					log.Error( "Couldn't create or verify Simias Server domain" );
					return;
				}

				//
				// Make this domain the default
				//

				store.DefaultDomain = rDomain.ID;

				/*
				Member pMember;
				Simias.POBox.POBox poBox = null;
				string poBoxName = "POBox:" + rDomain.ID + ":" + ldbMember.ID;

				try
				{
					poBox = Simias.POBox.POBox.FindPOBox( store, rDomain.ID, ldbMember.ID );
				}
				catch{}
				if (poBox == null)
				{
					poBox = new Simias.POBox.POBox( store, poBoxName, rDomain.ID );
					poBox.Role = SyncRoles.Master;
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
				*/
			}
			catch(Exception e1)
			{
				log.Error( e1.Message );
				log.Error( e1.StackTrace );

				//throw e1;
				// FIXME:: rethrow the exception
			}			
		}

		/// <summary>
		/// Method to get the Simias simple server domain
		/// If the domain does not exist and the create flag is true
		/// the domain will be created.  If create == false, ownerName is ignored
		/// </summary>
		internal Simias.Storage.Domain GetSimiasServerDomain( bool create )
		{
			//
			//  Check if the Simias Server domain exists in the store
			//

			Simias.Storage.Domain ssDomain = null;
			Store store = Store.GetStore();

			try
			{
				foreach( ShallowNode sNode in store.GetDomainList() )
				{
					Simias.Storage.Domain tmpDomain = store.GetDomain( sNode.ID );
					Property p = tmpDomain.Properties.GetSingleProperty( "SimiasServer" );
					if ( p != null && (bool) p.Value == true )
					{
						ssDomain = tmpDomain;
						this.id = tmpDomain.ID;
						break;
					}
				}

				if ( ssDomain == null && create == true )
				{
					// Get the domain name and description from the config file
					Configuration config =
						Simias.Configuration.GetConfiguration();
					if ( config != null )
					{
						string name = config.Get( DomainSection, "EnterpriseName", "" );
						if ( name != null && name != "" )
						{
							this.domainName = name;
						}

						string description = config.Get( DomainSection, "EnterpriseDescription", "" );
						if ( description != null && description != "" )
						{
							this.description = description;
						}

						string admin = config.Get( DomainSection, "SimiasServerAdmin", "" );
						if ( admin != null && admin != "" )
						{
							this.adminName = admin;
						}

						string adminPwd = config.Get( DomainSection, "SimiasServerAdminPassword", "" );
						if ( adminPwd != null && adminPwd != "" )
						{
							this.adminPassword = adminPwd;
						}
					}

					this.id = Guid.NewGuid().ToString();
					//Uri localUri = new Uri("http://" + MyDns.GetHostName() + "/simias10");

					// Create the Simias Server server domain.
					ssDomain = 
						new Simias.Storage.Domain(
							store, 
							this.domainName, 
							this.id,
							this.description, 
							Simias.Sync.SyncRoles.Master, 
                            Simias.Storage.Domain.ConfigurationType.ClientServer );

					// This needs to be added to allow the enterprise location provider
					// to be able to resolve this domain.
					ssDomain.SetType( ssDomain, "Enterprise" );

					// Set the known tag for a Simias Server domain.
					Property p = new Property( "SimiasServer", true );
					p.LocalProperty = true;
					ssDomain.Properties.ModifyProperty( p );

					// Create the owner member for the domain.
					Member member = 
						new Member(
							this.adminName,
							Guid.NewGuid().ToString(), 
							Access.Rights.Admin );

					member.IsOwner = true;

					// Note! need to first perform a known
					// hash on the password before storing it.
					//

					// Set the admin password
					string hashedPwd = SimiasCredentials.HashPassword( this.adminPassword );
					Property pwd = new Property( "SS:PWD", hashedPwd );
					pwd.LocalProperty = true;
					member.Properties.ModifyProperty( pwd );

					ssDomain.Commit( new Node[] { ssDomain, member } );

					// Create the name mapping.
					store.AddDomainIdentity( ssDomain.ID, member.UserID );
				}
			}
			catch( Exception gssd )
			{
				log.Error( gssd.Message );
				log.Error( gssd.StackTrace );
			}

			return ssDomain;
		}

		#region Public Methods

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
