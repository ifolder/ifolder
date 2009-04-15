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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Utilclass;

using Simias;
using Simias.Event;
using Simias.POBox;
using Simias.Storage;

namespace Simias.LdapProvider
{
	[Serializable]
	public enum Status
	{
		Syncing,
		Sleeping,
		LdapConnectionFailure,
		LdapAuthenticationFailure,
		SyncThreadDown,
		ConfigurationError,
		InternalException
	}

	/// <summary>
	/// Service class used to get an execution context
	/// so we can register ourselves with the external
	/// sync container
	/// </summary>
	public class Sync : Simias.IIdentitySyncProvider
	{
		#region Class Members
		private readonly string name = "LDAP Synchronization";
		private readonly string description = "LDAP Synchronization provider to synchronize identities from an ldap store to a simias domain";
		private bool abort = false;
		
		private Status syncStatus;
		private static LdapSettings ldapSettings;
		private Exception syncException;

		private Store store = null;
		private Domain domain = null;
		
		private LdapConnection conn = null;
		private Simias.IdentitySynchronization.State state = null;
		
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Properties
		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		public string Name { get{ return name; } }

		/// <summary>
		/// Gets the description of the provider.
		/// </summary>
		public string Description { get{ return description; } }
		#endregion
		
		#region Private Methods
		
        /// <summary>
        /// Processes search objects for user, group and container one by one
        /// </summary>
        /// <param name="conn">ldap connection</param>
        /// <param name="settings">ldap setting</param>
		private void ProcessSearchObjects( LdapConnection conn, LdapSettings settings )
		{
			foreach ( string searchContext in settings.SearchContexts )
			{
				string[] searchAttributes = { "objectClass" };

				log.Debug( "SearchObject: " + searchContext );

				try
				{
					LdapEntry ldapEntry = conn.Read( searchContext, searchAttributes );
					LdapAttribute attrObjectClass = ldapEntry.getAttribute( "objectClass" );
					String[] values = attrObjectClass.StringValueArray;

				foreach( string s in values)
				{
					log.Debug( "objectClass: " + s );
				}

					if ( IsUser( values ) == true )
					{
						// Process SearchDN as 
						log.Debug( "Processing User Object..." );
						ProcessSearchUser( conn, searchContext, "", "" );
					}
					else if ( IsGroup( values ) == true )
					{
						// Process SearchDN as 
						log.Debug( "Processing Group Object..." );
						//ProcessSearchGroup( conn, searchContext );
						ProcessSearchContainer( conn, searchContext );
					}
					else if ( IsContainer( values ) == true )
					{
						// Process SearchDN as Container
						log.Debug( "Processing Container Object..." );
						ProcessSearchContainer( conn, searchContext );
					}
					else
					{
						log.Debug( "Invalid objectClass: " + values[0] );
						log.Debug( attrObjectClass.ToString() );
					}
				}
				catch( SimiasShutdownException s )
				{
					log.Error( s.Message );
					throw s;
				}
				catch ( LdapException e )
				{
					log.Error( e.LdapErrorMessage );
					log.Error( e.StackTrace );
				}
				catch ( Exception e )
				{
					log.Error( e.Message );
					log.Error( e.StackTrace );
				}
			}
		}
		
        /// <summary>
        /// Builds GUID filter for given guid using GUID=
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>new filter</returns>
		private string BuildGuidFilter( string guid )
		{
			Guid cGuid = new Guid( guid );
			byte[] bGuid = cGuid.ToByteArray();

			string guidFilter = "(GUID=";
			string tmp;

			// The CSharpLdap SDK expects each byte to
			// be zero padded else an exception is thrown
			for( int i = 0; i < 16; i++ )
			{
				guidFilter += "\\";
				tmp = Convert.ToString( bGuid[i], 16 );
				if ( tmp.Length == 1 )
				{
					guidFilter += "0";
				}
				guidFilter += tmp;
			}

			guidFilter += ")";
			return guidFilter;
		}

        /// <summary>
        /// Gets ldap guid
        /// </summary>
        /// <param name="entry">ldap entry</param>
        /// <returns>string containing ldap guid</returns>
		private string GetLdapGuid( LdapEntry entry )
		{
			string ldapGuid = null;

			try
			{
				LdapAttribute guidAttr = entry.getAttribute( "GUID" );
				if ( guidAttr != null && guidAttr.StringValue.Length != 0 )
				{
					byte[] bGuid = new byte[8];
					for( int i = 0; i < 8; i++ )
					{
						bGuid[i] = (byte) guidAttr.ByteValue[i];
					}

					Guid cGuid = 
						new Guid(
							BitConverter.ToInt32( bGuid, 0 ),
							BitConverter.ToInt16( bGuid, 4 ),
							BitConverter.ToInt16( bGuid, 6 ),
							(byte) guidAttr.ByteValue[8], 
							(byte) guidAttr.ByteValue[9],
							(byte) guidAttr.ByteValue[10],
							(byte) guidAttr.ByteValue[11],
							(byte) guidAttr.ByteValue[12],
							(byte) guidAttr.ByteValue[13],
							(byte) guidAttr.ByteValue[14],
							(byte) guidAttr.ByteValue[15] );

					ldapGuid = cGuid.ToString();
				}
			}
			catch{}
			return ldapGuid;
		}

        /// <summary>
        /// Checks if the object classes has user
        /// </summary>
        /// <param name="objectClasses">string array</param>
        /// <returns>true if inetorgperson attribute is present</returns>
		private bool IsUser( String[] objectClasses )
		{
			try
		    {
				foreach( string s in objectClasses )
				{
					if ( s.ToLower() == "inetorgperson" )
					{
						return true;
					}
				}
			}
		    catch( Exception e )
		    {
				log.Error( "IsUser failed with exception" );
				log.Error( e.Message );
			}
		    return false;
		}

        /// <summary>
        /// Checks if it is a group
        /// </summary>
        /// <param name="objectClasses"></param>
        /// <returns>true if it has attributes groupofnames, or dynamic group or dynamicgroupaux</returns>
		private bool IsGroup( String[] objectClasses )
		{
			try
			{
				foreach( string s in objectClasses )
				{
					if ( s.ToLower() == "groupofnames" ||
						s.ToLower() == "dynamicgroup" ||
						s.ToLower() == "dynamicgroupaux" )
					{
						return true;
					}
				}
		    }
		    catch( Exception e )
			{
				log.Error( "IsGroup failed with exception" );
				log.Error( e.Message );
			}
		    return false;
		}

        /// <summary>
        /// Checks if it is a container
        /// </summary>
        /// <param name="objectClasses"></param>
        /// <returns>true if it is a conatiner e.g. organizational unit</returns>
		private bool IsContainer( String[] objectClasses )
		{
			bool isContainer = false;

			try
			{
				foreach( string s in objectClasses )
				{
					string lower = s.ToLower();
					if ( lower == "organization" )
					{
						log.Debug( "Processing Organization Object..." );
						isContainer = true;
						break;
					}
					else if ( lower == "organizationalunit" )
					{
						isContainer = true;
						log.Debug( "Processing OrganizationalUnit Object..." );
						break;
					}
					else if ( lower == "country" )
					{
						isContainer = true;
						log.Debug( "Processing Country Object..." );
						break;
					}
					else if ( lower == "locality" )
					{
						isContainer = true;
						log.Debug( "Processing Locality Object..." );
						break;
					}
				}
			}
			catch( Exception e )
			{
				log.Error( "IsContainer failed with exception" );
				log.Error( e.Message );
			}

			return isContainer;
		}

        /// <summary>
        /// For given entity, attach all ldap attributes
        /// </summary>
        /// <param name="connection">ldap connection</param>
        /// <param name="searchUser">user to be searched</param>
        /// <param name="groupDn">group DN</param>
        /// <param name="groupList">groupList</param>
		private void ProcessSearchUser( LdapConnection connection, string searchUser, string groupDn  , string groupList)
		{
			// Since the first version of the iFolder 3.0 only
		    // exposes a username, firstname, lastname and full
		    // name, we'll limit the scope of the search
			string[] searchAttributes = {	
					"modifytimestamp",
					ldapSettings.NamingAttribute,
					"cn",
					"sn",
					"GUID",
					"givenName",
					"ou",
					"objectclass",
                                        "member",
                                        "groupMembership",
                                        "iFolderHomeServer",
                                        "uniquemember" };

		    log.Debug( "ProcessSearchUser(" + searchUser + ")" );

			try
			{
				LdapEntry ldapEntry = connection.Read( searchUser, searchAttributes );
				ProcessUserEntry(connection, ldapEntry, groupDn , groupList);
			}
			catch( SimiasShutdownException s )
			{
				throw s;
			}
			catch( LdapException e )
			{
				log.Error( e.LdapErrorMessage );
				log.Error( e.StackTrace );
			}
			catch( Exception e ) 
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}
		}

        /// <summary>
        /// If the configured Simias Admin is different than the SimiasAdmin
        /// identified in the store, make all the changes necessary to
        /// make the configured admin the store admin.
        /// </summary>
        /// <param name="conn">ldap connection</param>
		private void ChangeSimiasAdmin( LdapConnection conn )
		{
			char[] dnDelimiters = {',', '='};
			LdapEntry entry = null;
			Property dn;
			string commonName;
			string ldapGuid;
			string[] searchAttributes = { "cn", "sn", "GUID" };

			try
			{
				// Nothing in the config for a SimiasAdmin - we're done here
				if ( ldapSettings.AdminDN == null || ldapSettings.AdminDN == "" )
				{
					return;
				}

				// If the SimiasAdmin has been changed in the Simias.config, which BTW
				// is not something that is exposed in the normal management UI, 
				// we need to verify the new SimiasAdmin exists in the directory,
				// check if the new admin exists in local domain memberlist (and
				// if he doesn't create him), transfer default domain ownership
				// to the new SimiasAdmin and lastly transfer ownership of all
				// orphaned iFolders to the new SimiasAdmin.

				try
				{
					entry = conn.Read( Sync.ldapSettings.AdminDN, searchAttributes );
				}
				catch( LdapException lEx )
				{
					log.Error( "Could not verify the newly configured Simias Administrator in the directory" );
					log.Error( lEx.Message );
				}
				catch( Exception e1 )
				{
					log.Error( "Could not verify the newly configured Simias Administrator in the directory" );
					log.Error( e1.Message );
				}

				if ( entry == null )
				{
					return;
				}

				ldapGuid = GetLdapGuid( entry );
				if ( ldapGuid == null || ldapGuid == "" )
				{
					return;
				}

				// Get the common name from the Simias.config.AdminDN entry
				string[] components = Sync.ldapSettings.AdminDN.Split( dnDelimiters );
				commonName = ( components[0].ToLower() == "cn" ) ? components[1] : components[0];
				if ( commonName == null || commonName == "" )
				{
					return;
				}

				store = Store.GetStore();
				if ( domain == null )
				{
					domain = store.GetDomain( store.DefaultDomain );
					if ( domain == null )
					{
						throw new SimiasException( "Enterprise domain does not exist!" );
					}
				}

				Member member = domain.GetMemberByName( commonName );
				if ( member == null )
				{
					// Create the member with the Ldap guid
					member = 
						new Member(	commonName,	ldapGuid, Simias.Storage.Access.Rights.ReadOnly );
					member.Properties.ModifyProperty( "DN", ldapSettings.AdminDN );
				}

				Property lguid = new Property( "LdapGuid", ldapGuid );
				lguid.LocalProperty = true;
				member.Properties.ModifyProperty( lguid );
				domain.Commit( member );

				// Transfer ownership of all collections owned by the 
				// previous admin that have the orphaned property
				Property orphaned;
				ICSList subList = store.GetCollectionsByOwner( domain.Owner.ID, domain.ID ); 
				foreach ( ShallowNode sn in subList )
				{
					// Get the collection object for this node.
					Collection c = store.GetCollectionByID( sn.CollectionID );
					if ( c != null )
					{
						orphaned = c.Properties.GetSingleProperty( "OrphanedOwner" );
						if ( orphaned != null )
						{
							dn = c.Owner.Properties.GetSingleProperty( "DN" );
							if ( dn != null )
							{
								c.PreviousOwner = dn.Value.ToString();
								c.Commit();
							}

							c.Commit( c.ChangeOwner( member, Simias.Storage.Access.Rights.ReadWrite ) );
						}
					}
				}

				// For now I'm just going to leave the LdapGuid property
				// on the old SimiasAdmin
				dn = domain.Owner.Properties.GetSingleProperty( "DN" );
				if ( dn != null )
				{
					domain.PreviousOwner = dn.Value.ToString();
					domain.Commit();
				}
				
				domain.Commit( domain.ChangeOwner( member, Simias.Storage.Access.Rights.ReadWrite ) );
			}
			catch( Exception vsa )
			{
				log.Error( vsa.Message );
				log.Error( vsa.StackTrace );
			}
		}

        /// <summary>
        /// The SimiasAdmin is processed differently than normal simias users because
        /// the account is aleady created in the Simias store before LdapSync runs
        /// so the GUID has already been created.  The SimiasAdmin must always exist in the
        /// store and the DN entry in the store must be correct with the Distinguished
        /// Name in the directory.  LdapSync counts on the AdminDN entry in Simias.config
        /// to be updated if the admin is moved in the directory.
        /// </summary>
        /// <param name="conn">ldap connection</param>
		private void ProcessSimiasAdmin( LdapConnection conn )
		{
			// Since the first version of the iFolder 3.0 only
			// exposes a username, firstname, lastname and full
			// name, we'll limit the scope of the search
			string[] searchAttributes = {
						"modifytimestamp",
						ldapSettings.NamingAttribute,
						"cn",
						"sn",
						"GUID",
						"givenName" };

			char[] dnDelimiters = {',', '='};
			LdapEntry entry = null;
			LdapAttribute timeStampAttr = null;
			Member cMember = null;
			Property dn = null;
			string ldapGuid = null;

			log.Debug( "ProcessSimiasAdmin( " + ldapSettings.AdminDN + ")" );

			if ( domain == null )
			{
				store = Store.GetStore();
				domain = store.GetDomain( store.DefaultDomain );
				if ( domain == null )
				{
					throw new SimiasException( "Enterprise domain does not exist!" );
				}
			}

			// If the DN property has never been set on the SimiasAdmin,
			// set it now
			cMember = domain.Owner;
			dn = cMember.Properties.GetSingleProperty( "DN" );
			if ( dn == null || dn.Value.ToString() == "" )
			{
				if ( ldapSettings.AdminDN != null && ldapSettings.AdminDN != "" )
				{
					dn = new Property( "DN", ldapSettings.AdminDN );
					cMember.Properties.ModifyProperty( dn );
				}
			}

			// Check if the Simias Admin has changed in configuration
			if ( ldapSettings.AdminDN != null && ldapSettings.AdminDN != "" &&
				dn.Value.ToString() != ldapSettings.AdminDN )
			{
				ChangeSimiasAdmin( conn );
				cMember = domain.Owner;
			}

			// The Simias admin is tracked in the directory by the directory
			// guid.  Make sure the guid is stored in the node
			Property lguidProp = cMember.Properties.GetSingleProperty( "LdapGuid" );
			if ( lguidProp == null )
			{
				// This must be the first time thru so let's get the directory
				// entry based on the configured DN
				try
				{
					entry = conn.Read( ldapSettings.AdminDN, searchAttributes );
				}
				catch( LdapException lEx )
				{
					log.Error( "The Simias Administrator does not exist in the Ldap directory as configured in Simias.config!" );
					log.Error( lEx.Message );
				}
				catch( Exception e1 )
				{
					log.Error( "The Simias Administrator does not exist in the Ldap directory as configured in Simias.config!" );
					log.Error( e1.Message );
				}

				if ( entry != null )
				{
					ldapGuid = GetLdapGuid( entry );
					lguidProp = new Property( "LdapGuid", ldapGuid );
					lguidProp.LocalProperty = true;
					cMember.Properties.ModifyProperty( lguidProp );
				}
			}
			else
			{
				ldapGuid = lguidProp.Value.ToString();
			}

			if ( ldapGuid != null )
			{
				try
				{
					entry = null;

					// Now go find the SimiasAdmin in the Ldap directory
					string guidFilter = BuildGuidFilter( ldapGuid );
					LdapSearchResults results = 
						conn.Search(
							"",
							LdapConnection.SCOPE_SUB,
							"(&(objectclass=inetOrgPerson)" + guidFilter + ")",
							searchAttributes,
							false);
					if ( results.hasMore() == true )
					{
						entry = results.next();
					}
				}
				catch ( LdapException e )
				{
					log.Error( e.LdapErrorMessage );
					log.Error( e.StackTrace );
				}
				catch ( Exception e )
				{
					log.Error( e.Message );
					log.Error( e.StackTrace );
				}

				if ( entry != null )
				{
					//
					// check if the ldap object's time stamp has changed
					//
					try
					{
						timeStampAttr = entry.getAttribute( "modifytimestamp" );
						Property pStamp = 
							cMember.Properties.GetSingleProperty( "LdapTimeStamp" );

						if ( ( pStamp == null ) ||
							( pStamp != null && 
							(string) pStamp.Value != timeStampAttr.StringValue ) )
						{
							// The time stamp changed let's look at first and
							// last name
	
							try
							{
								bool changed = false;
	
								// If we're tracking by ldap see if the naming attribute
								// has changed
								LdapAttribute namingAttr = entry.getAttribute( ldapSettings.NamingAttribute );
								if ( namingAttr != null && namingAttr.StringValue.Length != 0 )
								{
									if ( namingAttr.StringValue != cMember.Name )
									{
										cMember.Name = namingAttr.StringValue;
									}
								}
	
								LdapAttribute givenAttr = entry.getAttribute( "givenName" );
								if ( givenAttr != null && givenAttr.StringValue.Length != 0 )
								{
									if ( givenAttr.StringValue != cMember.Given )
									{
										changed = true;
										cMember.Given = givenAttr.StringValue;
									}
								}

								LdapAttribute sirAttr = entry.getAttribute( "sn" );
								if ( sirAttr != null && sirAttr.StringValue.Length != 0 )
								{
									if ( sirAttr.StringValue != cMember.Family )
									{
										cMember.Family = sirAttr.StringValue;
										changed = true;
									}
								}


								// If the entry has changed and we have a valid
								// family and given
								if ( changed == true && 
									cMember.Given != null &&
									cMember.Given != "" && 
									cMember.Family != null &&
									cMember.Family != "" )
								{
									cMember.FN = cMember.Given + " " + cMember.Family;
								}

								// Did the distinguished name change?
								Property dnProp = cMember.Properties.GetSingleProperty( "DN" );
								if ( dnProp != null && ( dnProp.ToString() != entry.DN ) )
								{
									dnProp.Value = entry.DN;
									cMember.Properties.ModifyProperty( "DN", dnProp );
								}
							}
							catch {}

							pStamp = new Property( "LdapTimeStamp", timeStampAttr.StringValue );
							pStamp.LocalProperty = true;
							cMember.Properties.ModifyProperty( pStamp );
						}
					}
					catch{}
				}
				else
				{
					log.Error( "The Simias administrator could not be verified in the directory!" );
					log.Error( "Please update Simias.config with a valid Ldap user" );
				}
			}
			else
			{
				log.Error( "The Simias administrator could not be verified in the directory!" );
				log.Error( "Please update Simias.config with a valid Ldap user" );
			}

			// Now matter what always update the sync guid so
			// the SimiasAdmin won't be deleted from Simias

			cMember.Properties.ModifyProperty( state.SyncGuid );
			domain.Commit( cMember );
		}

        /// <summary>
        /// Process each member of given group
        /// </summary>
        /// <param name="conn">ldap connection</param>
        /// <param name="groupMembers">all members of the group</param>
        /// <param name="searchGroup">search group</param>
        /// <param name="groupList">all groups list</param>
		private void ProcessSearchGroup( LdapConnection conn, string groupMembers, string searchGroup , string groupList)
		{
			log.Debug( "ProcessSearchGroup(" + searchGroup + "   " + groupMembers + " )" );

			int count = 0;

			try
			{
				string[] memberArray = groupMembers.Split(new char[] { ';' });
				foreach( String member in memberArray )
				{
					// Check if the sync engine wants us to abort
					if(member != null && member != String.Empty && member != "" )
					{
						log.Debug( "   Processing member: " + member );
						count++;
						ProcessSearchUser( conn, member, searchGroup, groupList );
					}
				}
			}
			catch( SimiasShutdownException s )
			{
				throw s;
			}
			catch( LdapException e )
			{
				log.Error( e.LdapErrorMessage );
				log.Error( e.StackTrace );
			}
			catch( Exception e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}

			log.Debug( "Processed " + count.ToString() + " entries" );
		}

        /// <summary>
        /// Process each member of given container
        /// </summary>
        /// <param name="conn">ldap connection</param>
        /// <param name="searchContainer">container to be searched</param>
		private void ProcessSearchContainer(LdapConnection conn, String searchContainer)
		{
			String searchFilter = "(|(objectclass=user)(objectclass=groupOfNames)(objectclass=dynamicGroup)(objectclass=dynamicGroupAux))";
			string[] searchAttributes = {
						"modifytimestamp",
						ldapSettings.NamingAttribute,
						"cn",
						"sn",
						"GUID",
						"givenName",
						"ou",
						"objectclass",
						"member",
						"groupMembership",
						"iFolderHomeServer",
						"uniquemember" };

			log.Debug( "ProcessSearchContainer(" + searchContainer + ")" );

			int count = 0;
			LdapSearchConstraints searchConstraints = new LdapSearchConstraints();
			searchConstraints.MaxResults = 0;

		    LdapSearchQueue queue = 
				conn.Search(
					searchContainer, 
					LdapConnection.SCOPE_SUB, 
					searchFilter, 
					searchAttributes, 
					false,
					(LdapSearchQueue) null,
					searchConstraints);

		    LdapMessage ldapMessage;
		    while( ( ldapMessage = queue.getResponse() ) != null )
			{
				// Check if the sync engine wants us to abort
				if ( this.abort == true )
				{
					return;
				}

				if ( ldapMessage is LdapSearchResult )
				{
					LdapEntry cEntry = ((LdapSearchResult) ldapMessage).Entry;
					if (cEntry == null)
					{
						continue;
					}

					try
					{
						ProcessUserEntry( conn, cEntry, "", "" );
						count++;
					}
					catch( SimiasShutdownException s )
					{
						log.Error( s.Message );
						throw s;
					}
					catch( LdapException e )
					{
						log.Error( "   Failed processing: " + cEntry.DN );
						log.Error( e.LdapErrorMessage );
						log.Error( e.StackTrace );
					}
					catch( Exception e )
					{
						log.Error( "   Failed processing: " + cEntry.DN );
						log.Error( e.Message );
						log.Error( e.StackTrace );
					}
				}
			}

			log.Debug( "Processed " + count.ToString() + " entries" );
		}

        /// <summary>
        /// For given entity, assign all ldap attributes
        /// </summary>
        /// <param name="conn">ldap connection</param>
        /// <param name="entry">entry to be processed</param>
        /// <param name="group">group</param>
        /// <param name="groupList">list of all groups</param>
		private void ProcessUserEntry( LdapConnection conn, LdapEntry entry, string group , string groupList)
		{
			log.Debug( "ProcessUserEntry(" + entry.DN + ")" );

			string commonName = String.Empty;
			string firstName = null;
			string lastName = null;
			string fullName = null;
			string distinguishedName = String.Empty;
			string ldapGuid = null;
			string objectclass = String.Empty;
			bool Group = false;
			string groupmembers = String.Empty;
			string groupmembership = String.Empty;
			string iFolderHomeServer = String.Empty;

		    char[] dnDelimiters = {',', '='};
		    LdapAttribute timeStampAttr = null;

			bool attrError = false;
			string FullNameDisplay = "";

			store = Store.GetStore();
			Domain domain = store.GetDomain( store.DefaultDomain );
			if ( domain != null )
			{
				FullNameDisplay = domain.UsersFullNameDisplay;
			}


			try
			{
				// get the last update time
				timeStampAttr = entry.getAttribute( "modifytimestamp" );

				ldapGuid = GetLdapGuid( entry );
				distinguishedName = entry.DN;

				// retrieve from configuration the directory attribute configured
				// for naming in Simias.  
				LdapAttribute cAttr = 
					entry.getAttribute( ldapSettings.NamingAttribute );
				if ( cAttr != null && cAttr.StringValue.Length != 0 )
				{
					commonName = cAttr.StringValue;
				}
				else
				if ( ldapSettings.NamingAttribute.ToLower() == LdapSettings.DefaultNamingAttribute.ToLower() )
				{
					// If the naming attribute is default (cn) then we want to continue
					// to work the way we previously did so we don't break any existing installs.
					//
					// If the distinguishing attribute did not exist,
					// then make the Simias username the first component
					// of the ldap DN.
					string[] components = entry.DN.Split( dnDelimiters );
					commonName = components[1];
				}

				LdapAttribute givenAttr = entry.getAttribute( "givenName" );
				if ( givenAttr != null && givenAttr.StringValue.Length != 0 )
				{
					firstName = givenAttr.StringValue as string;
				}

				LdapAttribute sirAttr = entry.getAttribute( "sn" );
				if ( sirAttr != null && sirAttr.StringValue.Length != 0 )
				{
					lastName = sirAttr.StringValue as string;
				}

				LdapAttribute objectAttr = entry.getAttribute( "objectclass" );
                                String[] values = objectAttr.StringValueArray;
                                if ( IsGroup( values ) == true )
                                {
                                                Group = true;
                                }

				LdapAttribute iFolderHomeAttr = entry.getAttribute( "iFolderHomeServer" );
				if ( iFolderHomeAttr != null && iFolderHomeAttr.StringValue.Length != 0 )
				{
					iFolderHomeServer = iFolderHomeAttr.StringValue as string;
				}
				if(Group == true)
				{
					if(isGropuAlreadyprocessed(groupList, distinguishedName) == true)
						return;
           				LdapAttributeSet attributeSet = entry.getAttributeSet();
           				System.Collections.IEnumerator ienum = attributeSet.GetEnumerator();
           				while(ienum.MoveNext())
           				{
                  				LdapAttribute attribute=(LdapAttribute)ienum.Current;
                  				string attributeName =attribute.Name; 
						System.Collections.IEnumerator enumVals = attribute.StringValues;
						if(attributeName.ToLower() == "member" || 
							attributeName.ToLower() == "uniquemember")
						{
							while (enumVals.MoveNext())
							{
								if(isGropuAlreadyprocessed(groupList, (System.String) enumVals.Current) == false)
								{
									groupmembers += (System.String) enumVals.Current;
									groupmembers += ";"; 
								}
							}
						}
           				}
					groupList += distinguishedName;
					groupList += ";";
                                        ProcessSearchGroup( conn, groupmembers, distinguishedName, groupList);
				}
                                if(group != null && group != String.Empty)
                                {
                                        groupmembership = group;
                                        groupmembership += ";";
                                }
				if ( firstName != null && lastName != null )
				{
					if(FullNameDisplay == "FirstNameLastName")
						fullName = firstName + " " + lastName;
					else
						fullName = lastName + " " + firstName;
				}
				else
					fullName = commonName;
			}
			catch( Exception gEx )
			{
				log.Error( gEx.Message );
				log.Error( gEx.StackTrace );

				state.ReportError( gEx.Message );
				attrError = true;
			}

			log.Debug( "FullName: " + fullName + " commonName: " + commonName + " ObjectClass: " + objectclass + " groupmembers: " + groupmembers + " groupmembership: " + groupmembership + " " );
			
			// No exception were generated gathering member info
			// so call the sync engine to process this member
			if ( attrError == false )
			{
				if ( timeStampAttr != null && timeStampAttr.StringValue.Length != 0 )
				{
					Property ts = new Property( "LdapTimeStamp", timeStampAttr.StringValue );
					ts.LocalProperty = true;
					Property[] propertyList = { ts };

					state.ProcessMember(
						ldapGuid,
						commonName,
						firstName,
						lastName,
						fullName,
						distinguishedName,
						propertyList,
						Group,
						groupmembers,
						groupmembership,
						iFolderHomeServer );
				}
				else
				{
					state.ProcessMember(
						ldapGuid,
						commonName,
						firstName,
						lastName,
						fullName,
						distinguishedName,
						null,
						Group,
						groupmembers,
						groupmembership,
						iFolderHomeServer );
				}
			}
		}

        /// <summary>
        /// Checks if given group is already in processed list
        /// </summary>
        /// <param name="groupList">list og group names already processed</param>
        /// <param name="groupName">Name of the group to be searched</param>
        /// <returns>true if given groupName is present in the list</returns>
		private bool isGropuAlreadyprocessed( string groupList, string groupName)
		{
			if(groupList != null && groupList != String.Empty & groupList != "")
			{
                          		string[] groupArray = groupList.Split(new char[] { ';' });
                             		foreach( String grp in groupArray )
                               		{
                                      		if(grp != null && grp != String.Empty && grp != "" )
                                       		{
							if(String.Compare(grp, groupName) == 0)
								return true;
                                       		}
                               		}

			}
			return false;
		}
		
		#endregion

		#region Public Methods
		/// <summary>
		/// Call to abort an in process synchronization
		/// </summary>
		/// <returns>N/A</returns>
		public void Abort()
		{
			abort = true;
		}
		
		/// <summary>
		/// Call to inform a provider to start a synchronization cycle
		/// </summary>
		/// <returns> True - provider successfully finished a sync cycle, 
		/// False - provider failed the sync cycle
		/// </returns>
		public bool Start( Simias.IdentitySynchronization.State State )
		{
			log.Debug( "Start called" );
			int MaxConnectRetry = 5;

			bool status = false;
			abort = false;
			try
			{
				this.state = State;

				try
				{
					ldapSettings = LdapSettings.Get( Store.StorePath );
					log.Debug( "new LdapConnection" );
					conn = new LdapConnection();

					log.Debug( "Connecting to: " + ldapSettings.Host + " on port: " + ldapSettings.Port.ToString() );
					conn.SecureSocketLayer = ldapSettings.SSL;
                                        for(int i =1; i <= MaxConnectRetry ; i++)
                                        {
                                                try
                                                {
                                                        conn.Connect( ldapSettings.Host, ldapSettings.Port );
                                                }
                                                catch( Exception ex )
                                                {
                                                        log.Debug( "Failed to connect to : " + ldapSettings.Host + ", retry count: " + i.ToString() + ", Error Message : " + ex.Message );
                                                        continue;
                                                }
                                                break;
                                        }


					ProxyUser proxy = new ProxyUser();

					log.Debug( "Binding as: " + proxy.UserDN );
					conn.Bind( proxy.UserDN, proxy.Password );

					ProcessSimiasAdmin( conn );
					ProcessSearchObjects( conn, ldapSettings );
				}
				catch( SimiasShutdownException s )
				{
					log.Error( s.Message );
					syncException = s;
					syncStatus = Status.SyncThreadDown;
				}
				catch( LdapException e )
				{
					log.Error( e.LdapErrorMessage );
					log.Error( e.StackTrace );
					syncException = e;
					syncStatus = 
						( conn == null )
							? Status.LdapConnectionFailure
							: Status.LdapAuthenticationFailure;

					state.ReportError( e.LdapErrorMessage );
				}
				catch(Exception e)
				{
					log.Error( e.Message );
					log.Error( e.StackTrace );
					syncException = e;
					syncStatus = Status.InternalException;

					state.ReportError( e.Message );
				}
				finally
				{
					if ( conn != null )
					{
						log.Debug( "Disconnecting Ldap connection" );
						conn.Disconnect();
						conn = null;
					}
				}	
				status = true;
			}
			catch( Exception e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
				State.ReportError( e.Message );
			}
			
			return status;
		}
		#endregion
	}
}
