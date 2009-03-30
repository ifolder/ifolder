/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Mahabaleshwar Asundi <amahabaleshwar@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using System;
using System.Collections;
using System.Xml;

using Simias;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Identity
{
	/// <summary>
	/// Class to initialize/verify an Enterprise server domain
	/// </summary>
	public class EnterpriseDomain
	{
		#region Class Members
		/// <summary>
		/// GUID for this enterprise server domain
		/// </summary>
		private string id = "";

		/// <summary>
		/// Friendly name for the enterprise domain.
		/// The domainName, description, admin and adminPassword
		/// are usually overridden from properties in the
		/// Simias.config file.
		/// </summary>
		private string domainName = "Simias";
		private string hostAddress = String.Empty;
		private string description = "Enterprise server domain";
		private string admin = "admin";
		//private string adminPassword = "simias";

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Store store;
		private Simias.Storage.Domain domain = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the enterprise domain's unique ID
		/// </summary>
		public string ID
		{
			get { return( this.id ); }
		}

		/// <summary>
		/// Gets the Enterprise domain's friendly name
		/// </summary>
		public string Name
		{
			get { return( this.domainName ); }
		}

		/// <summary>
		/// Gets the Enterprise domain's description
		/// </summary>
		public string Description
		{
			get { return( this.description ); }
		}

		/// <summary>
		/// Gets the Enterprise domain's host address
		/// </summary>
		public string Host
		{
			get { return( this.hostAddress ); }
		}
		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for creating/initializing an 
		/// enterprise server domain
		/// </summary>
		internal EnterpriseDomain( bool Create )
		{
			store = Store.GetStore();
			domain = this.GetServerDomain( Create );
			if ( domain == null )
			{
				throw new SimiasException( "Enterprise domain could not created or initialized" );
			}
		}
		#endregion

		/// <summary>
		/// Method to get the Simias Enterprise server domain
		/// If the the domain does not exist and the create flag is true
		/// the domain will be created.  If create == false, ownerName is ignored
		/// </summary>
		internal Simias.Storage.Domain GetServerDomain( bool Create )
		{
			//  Check if the Server domain exists in the store
			Simias.Storage.Domain enterpriseDomain = null;
			bool master = true;
			
			try
			{
				Collection collection = store.GetSingleCollectionByType( "Enterprise" );
				if ( collection != null )
				{
					enterpriseDomain = store.GetDomain( collection.ID );
					if ( enterpriseDomain != null )
					{
						this.domainName = enterpriseDomain.Name;
						this.id = enterpriseDomain.ID;

						// For backwards compatibility, if the report collection does not
						// exist because the store was created with a previous version of
						// simias, check and create it here.
						// Don't create this directory on a slave server.

                                                //TODO : Check with migration !!
						//Report.CreateReportCollection( store, enterpriseDomain );
					}
				}

				if ( enterpriseDomain == null && Create == true )
				{
					// Bootstrap the domain from the Simias.config file
					Simias.Configuration config = Store.Config;
					string cfgValue = config.Get( "EnterpriseDomain", "SystemName" );
					if ( cfgValue != null && cfgValue != String.Empty )
					{
						this.domainName = cfgValue;
					}

					cfgValue = config.Get( "EnterpriseDomain", "Description" );
					if ( cfgValue != null && cfgValue != String.Empty )
					{
						this.description = cfgValue;
					}

					cfgValue = config.Get( "EnterpriseDomain", "AdminName" );
					if ( cfgValue != null && cfgValue != String.Empty )
					{
						this.admin = cfgValue;
					}

					cfgValue = config.Get( "Server", "MasterAddress" );
					if ( cfgValue != null && cfgValue != String.Empty )
					{
						master = false;
					}

					/*
					cfgValue = config.Get( "EnterpriseDomain", "AdminPassword" );
					if ( cfgValue != null && cfgValue != "" )
					{
						this.adminPassword = cfgValue;
					}
					*/

					if ( master == true )
					{
						this.id = Guid.NewGuid().ToString();

						// Create the enterprise server domain.
						enterpriseDomain = 
							new Simias.Storage.Domain(
							store, 
							this.domainName, 
							this.id,
							this.description, 
							Simias.Sync.SyncRoles.Master, 
							Simias.Storage.Domain.ConfigurationType.ClientServer );

						// This needs to be added to allow the enterprise location provider
						// to be able to resolve this domain.
						enterpriseDomain.SetType( enterpriseDomain, "Enterprise" );

						// Create the owner member for the domain.
						string provider = null;
						cfgValue = config.Get( "Identity", "Assembly" );
						if ( cfgValue != null && cfgValue != String.Empty )
						{
						    provider = cfgValue;
						}

						this.admin = ParseUserName (this.admin, provider);

						Member member = 
							new Member(this.admin, Guid.NewGuid().ToString(), Access.Rights.Admin );

						member.IsOwner = true;
						enterpriseDomain.SetType( member as Node, "User" );
					
						// Marker so we know this member was created internally
						// and not through an external identity sync.
						enterpriseDomain.SetType( member as Node, "Internal" );
					
						enterpriseDomain.Commit( new Node[] { enterpriseDomain, member } );
					
						// Set the domain default
						store.DefaultDomain = enterpriseDomain.ID;

						// Create the name mapping.
						store.AddDomainIdentity( enterpriseDomain.ID, member.UserID );
					}
					else
					{
						// Slave host so create the proxy domain and owner.
						enterpriseDomain = Simias.Host.SlaveSetup.GetDomain( Store.StorePath );
						store.DefaultDomain = enterpriseDomain.ID;
						enterpriseDomain.Role = Simias.Sync.SyncRoles.Slave;
						Member owner = Simias.Host.SlaveSetup.GetOwner( Store.StorePath );
						enterpriseDomain.SetType( enterpriseDomain, "Enterprise" );
						enterpriseDomain.Proxy = true;
						owner.Proxy = true;
						enterpriseDomain.Commit(new Node[] { enterpriseDomain, owner } );
					}

					//Report.CreateReportCollection( store, enterpriseDomain );
				}
			}
			catch( Exception gssd )
			{
				log.Error( gssd.Message );
				log.Error( gssd.StackTrace );
			}

			return enterpriseDomain;
		}


        /// <summary>
        /// Utility Func to set the BaseSchema.Name according the naming attribute
        /// </summary>
        /// <param name="configAdminName">admin name</param>
        /// <param name="provider">provider of ldap service</param>
        /// <returns></returns>
 	        private string ParseUserName (string configAdminName, string provider)
		{
		    string attribute = null;
		    //AD and eDir
		    if ( provider == "Simias.ADLdapProvider" || provider == "Simias.LdapProvider" )
			attribute = "cn";

		    if ( provider == "Simias.OpenLdapProvider")
			attribute = "uid";

		    if ( attribute != null && attribute != String.Empty )
		    {
			string[] attributes = configAdminName.Split(',');

			foreach (string attr in attributes)
			{
			    if (attr.StartsWith(attribute))
			    {
				string [] values = attr.Split ('=');
				return values[1]; //get the second part in the split
			    }
			}
		    }

		    return this.admin;
		}
		#region Public Methods
		#endregion
	}
}
