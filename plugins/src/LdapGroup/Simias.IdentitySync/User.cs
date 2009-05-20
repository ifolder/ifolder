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
using System.Reflection;
using System.Collections;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Sync;

using SCodes = Simias.Authentication.StatusCodes;

namespace Simias.Identity
{
	/// <summary>
	/// Class to manage users in the enterprise server domain. 
	/// </summary>
	public class User
	{
		#region Fields
		static private IUserProvider provider = null;
		static private string lockIt = "lock";
		static private string noProviderMessage = "No identity/user provider has registered";

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Store store;
		//private Simias.Storage.Domain domain = null;
		
		private string username;
		private string userguid;
		private string email;
		private string firstname;
		private string lastname;
		private string fullname;
		private string dn;
		#endregion

		#region Properties
		/// <summary>
		/// Username
		/// </summary>
		public string UserName
		{
			get { return( this.username ); }
			set { this.username = value; }
		}

		/// <summary>
		/// Guid
		/// </summary>
		public string UserGuid
		{
			get { return( this.userguid ); }
			set { this.userguid = value; }
		}

		/// <summary>
		/// First Name
		/// </summary>
		public string FirstName
		{
			get { return( this.firstname ); }
			set { this.firstname = value; }
		}

		/// <summary>
		/// Last Name
		/// </summary>
		public string LastName
		{
			get { return( this.lastname ); }
			set { this.lastname = value; }
		}

		/// <summary>
		/// Full Name
		/// </summary>
		public string FullName
		{
			get { return( this.fullname ); }
			set { this.fullname = value; }
		}

		/// <summary>
		/// Distinguished Name
		/// </summary>
		public string DN
		{
			get { return( this.dn ); }
			set { this.dn = value; }
		}

		/// <summary>
		/// Primary e-mail address
		/// </summary>
		public string Email
		{
			get { return( this.email ); }
			set { this.email = value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for managing users in the enterprise domain 
		/// </summary>
		public User( string Username )
		{
			username = Username;
			fullname = Username;

			store = Store.GetStore();
		}
		#endregion

		#region Public Methods
		
		/// <summary>
		/// Method to get an instance of the registered identity
		/// provider.  There can only be one identity provider in
		/// the system.
		/// </summary>
		/// <returns>an IUserProvider instance</returns>
		static public IUserProvider GetRegisteredProvider()
		{
			return User.provider;
		}
		
		/// <summary>
		/// Method to register an identity provider with the system.
		/// </summary>
		/// <returns>true success - false failure</returns>
		static public bool RegisterProvider( IUserProvider provider )
		{
			lock( User.lockIt )
			{
				if( User.provider == null )
				{
					User.provider = provider;
					return true;
				}
			}
			
			return false;
		}

		/// <summary>
		/// Method to deregister an identity provider with the system.
		/// </summary>
		/// <returns>true success - false failure</returns>
		static public bool UnregisterProvider( IUserProvider provider )
		{
			lock( User.lockIt )
			{
				if( User.provider == provider )
				{
					User.provider = null;
					return true;
				}
			}
			
			return false;
		}
		
		/// <summary>
		/// Method to create a user/identity in the external user database.
		/// Some external systems may not allow for creation of new users.
		/// </summary>
		/// <param name="Password" mandatory="true">Password associated to the user.</param>
		/// <returns>RegistrationStatus</returns>
		public RegistrationInfo Create( string Password )
		{
			RegistrationInfo info;
			if ( User.provider != null )
			{
				if ( Password != null )
				{
					UserProviderCaps caps = User.provider.GetCapabilities();
					if ( caps.CanCreate == true )
					{
						// Verify the user doesn't already exist
						Domain domain = store.GetDomain( store.DefaultDomain );
						if ( domain.GetMemberByName( this.username ) == null )
						{
							// Call the user provider to create the user
							log.Debug( "Creating member: {0}", this.username );

							// guarantee a full name exists
							if ( this.fullname == null )
							{
								this.fullname = this.username;
							}

							info = User.provider.Create(
										this.userguid,
										this.username,
										Password,
										this.firstname,
										this.lastname,
										this.fullname,
										this.dn );
						
							// Some providers may create the user in the server
							// domain - so verify a few things
							if ( info.Status == RegistrationStatus.UserCreated )
							{
								bool commit = false;
								Member member = domain.GetMemberByName( this.username );
								if ( member == null )
								{
									string guid; 
									if ( info.UserGuid != null && info.UserGuid != "" )
									{
										// Guid from the provider?
										guid = info.UserGuid;
									}
									else
									if ( this.userguid != null )
									{
										// Guid from the caller?
										guid = this.userguid;
									}
									else
									{
										guid = Guid.NewGuid().ToString();
									}
								
									member = 
										new Member(
											this.username,
											guid, 
											Access.Rights.ReadOnly,
											this.firstname,
											this.lastname );
						
									if ( this.fullname != null )
									{
										member.FN = this.fullname;
									}
									else
									if ( this.firstname != null && this.lastname != null )
									{
										member.FN = this.firstname + " " + this.lastname;
									}
								
									Property dnProp = new Property( "DN", info.DistinguishedName );
									member.Properties.ModifyProperty( dnProp );
									commit = true;
								}
								else
								{
									// FIXME
									// verify non-mandatory properties
								}
							
								if ( this.email != null && this.email != "" )
								{
									Property emailProp = new Property( "Email", this.email );
									member.Properties.ModifyProperty( emailProp );
									commit = true;
								}
							
								if ( commit == true )
								{
									domain.Commit( member );
								}
							}
						}	
						else
						{
							info = new RegistrationInfo( RegistrationStatus.UserAlreadyExists );
							info.Message = "Member already exists";
						}
					}
					else
					{
						info = new RegistrationInfo( RegistrationStatus.MethodNotSupported );
						info.Message = "Provider can't create users";
					}
				}
				else
				{
					info = new RegistrationInfo( RegistrationStatus.PasswordPolicyException );
					info.Message = "Password can't be null";
				}
			}
			else
			{
				info = new RegistrationInfo( RegistrationStatus.NoRegisteredUserProvider );
				info.Message = User.noProviderMessage;
			}
			
			return info;
		}

		/// <summary>
		/// Method to retrieve the capabilities of a user identity
		/// provider.
		/// </summary>
		/// <returns>providers capabilities</returns>
		public UserProviderCaps GetCapabilities()
		{
			UserProviderCaps caps = null;
			if ( User.provider != null )
			{
				caps = User.provider.GetCapabilities();
			}
			
			return caps;
		}

		/// <summary>
		/// Method to delete a member from the domain.
		/// </summary>
		/// <returns><b>True</b> if the user is successfully deleted; otherwise, <b>False</b> is returned.</returns>
		public bool Delete()
		{
			if ( User.provider != null )
			{
				// Get the domain for this user.
				Domain domain = this.store.GetDomain( this.store.DefaultDomain );
				if ( domain != null )
				{
					// Get the user's information by ID first.
					Member member = domain.GetMemberByID( this.username );
					if ( member == null )
					{
						// Try getting the member by name.
						member = domain.GetMemberByName( this.username );
					}

					// Make sure that we found the member.
					if ( member != null )
					{
						// Get the owner for the domain.
						Member domainOwner = domain.Owner;

						// Don't let the administrator be deleted.
						if ( member.UserID != domainOwner.UserID )
						{
							// Get all of the collections for this user.
							// BUGBUG!!! - This will need to be changed for multi-server.
							ICSList collections = this.store.GetCollectionsByUser( member.UserID );
							foreach( ShallowNode sn in collections )
							{
								Collection c = new Collection( this.store, sn );
								if ( c.Owner.UserID == member.UserID )
								{
									// Don't orphan POBoxes.
									if ( !c.IsBaseType( NodeTypes.POBoxType ) )
									{
										// Orphan this collection and assign it to the administrator.
										// BUGBUG!!! - Need to handle the case where the administrator is on
										// a different server. This collection would then need to be moved.

										// Check if administrator is a member.
										Member adminMember = c.GetMemberByID( domainOwner.UserID );
										if ( adminMember == null )
										{
											adminMember = new Member( domainOwner.Name, domainOwner.UserID, Access.Rights.Admin );
											c.Commit( adminMember );
										}

										// Change the owner to administrator and delete the old member.
										c.Commit( c.ChangeOwner( adminMember, Access.Rights.Deny ) );

										// Set the old member id to the previous owner.
										c.PreviousOwner = member.UserID;
										c.Commit();
									}
									else
									{
										// Delete the POBox.
										c.Commit( c.Delete() );
									}
								}
								else
								{
									// Don't remove the user from the domain collection.
									if ( c.ID != domain.ID )
									{
										// Find the member in this collection.
										Member cMember = c.GetMemberByID( member.UserID );
										if ( cMember != null )
										{
											// Remove this collection's user membership.
											c.Commit( c.Delete( cMember ) );
										}
									}
								}
							}

							// Delete the user from the system.
							return User.provider.Delete( this.username );
						}
					}
				}
			}
			
			return false;
		}

		/// <summary>
		/// Method to set/reset a user's password
		/// Note: This method will be replaced when the self-service
		/// framework is designed and implemented.
		/// </summary>
		/// <param name="Username" mandatory="true">Username to set the password on.</param>
		/// <param name="Password" mandatory="true">New password.</param>
		/// <returns>true - successful</returns>
		static public bool SetPassword( string Username, string Password )
		{
			bool status = false;
			if ( User.provider != null && Username != null && Username != String.Empty && Password != null )
			{
				if ( User.provider.GetCapabilities().CanModify == true )
				{
					status = User.provider.SetPassword( Username, Password );
				}
			}

			return status;
		}
		
		/// <summary>
		/// Method to verify a user's password
		/// </summary>
		/// <param name="Username">User to verify the password against</param>
		/// <param name="Password">Password to verify</param>
		/// <param name="status">Structure used to pass additional information back to the user.</param>
		/// <returns>true - Valid password, false Invalid password</returns>
		static public bool VerifyPassword( string Username, string Password, Simias.Authentication.Status status )
		{
			if ( User.provider != null )
			{
				return User.provider.VerifyPassword( Username, Password, status );
			}
		
			return false;
		}
		
		#endregion
	}
	
	/// <summary>
	/// Implementation of the IUserProvider Service for Internal Server.
	///
	/// The internal server does not use any external identity source 
	/// for creating or authenticating members in the Enterprise
	/// domain roster.
	/// </summary>
	public class InternalUser : Simias.Identity.IUserProvider
	{
		#region Fields
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

		/// <summary>
		/// String used to identify domain provider.
		/// </summary>
		static private string providerName = "Simias User Provider";
		static private string providerDescription = "User provider that uses Simias as the user database";
		
		static private string missingDomainMessage = "Enterprise domain does not exist!";
		static private string memberMarker = "Internal";
		static private string userType = "User";
		static private string pwdProperty = "PWD";

		// Frequently used Simias types
		private Store store = null;
		private Simias.Storage.Domain domain = null;

		// Types for creating an MD5 password hash
		private UTF8Encoding utf8Encoding;
		private MD5CryptoServiceProvider md5Service;
		#endregion
		
		#region Properties
		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		public string Name { get { return providerName; } }

		/// <summary>
		/// Gets the description of the provider.
		/// </summary>
		public string Description { get { return providerDescription; } }
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of this object.
		/// </summary>
		public InternalUser()
		{
			store = Store.GetStore();
			if ( store.DefaultDomain == null )
			{
				throw new SimiasException( InternalUser.missingDomainMessage );
			}
			
			domain = store.GetDomain( store.DefaultDomain );
			if ( domain == null )
			{
				throw new SimiasException( InternalUser.missingDomainMessage );
			}
			
			if ( domain.IsType( "Enterprise" ) == false )
			{
				throw new SimiasException( InternalUser.missingDomainMessage );
			}
			
			utf8Encoding = new UTF8Encoding();
			md5Service = new MD5CryptoServiceProvider();
			
			// Make sure the password is set on the admin
			SetAdminPassword();
		}
		#endregion
		
		#region Private Methods
        /// <summary>
        /// Encrypt the password
        /// </summary>
        /// <param name="password">password</param>
        /// <returns>encrypted password</returns>
		private string HashPassword( string password )
		{
			byte[] bytes = new Byte[ utf8Encoding.GetByteCount( password ) ];
			bytes = utf8Encoding.GetBytes( password );
			return Convert.ToBase64String( md5Service.ComputeHash( bytes ) );
		}
		
        /// <summary>
        /// commit the encrypted password as part of member object
        /// </summary>
        /// <returns>true if setting is successful</returns>
		private bool SetAdminPassword()
		{
			bool status = false;
			
			// Bootstrap the domain from the Simias.config file
			Simias.Configuration config = Store.Config;
			string adminName = config.Get( "EnterpriseDomain", "AdminName" );
			string adminPassword = config.Get( "EnterpriseDomain", "AdminPassword" );
			
			if ( adminName != null && adminName != "" && adminPassword != null )
			{
				try
				{
					Member member = domain.GetMemberByName( adminName );
					if ( member != null )
					{
						Property pwd = 
							member.Properties.GetSingleProperty( InternalUser.pwdProperty );
						if ( pwd == null || pwd.Value == null )
						{
							pwd = new Property( InternalUser.pwdProperty, HashPassword( adminPassword ) );
							member.Properties.ModifyProperty( pwd );

							// Marker so we know this member was created internally
							// and not through an external identity sync.
							domain.SetType( member as Node, InternalUser.memberMarker );
							domain.Commit( member );
							status = true;
						}	
					}
				}
				catch( Exception ap )
				{
					log.Error( ap.Message );
					log.Error( ap.StackTrace );
				}
			}
			
			return status;
		}		
		#endregion
		
		#region Public Methods
		/// <summary>
		/// Method to create a user/identity in the external user database.
		/// Some external systems may not allow for creation of new users.
		/// </summary>
		/// <param name="Userguid" mandatory="false">Guid associated to the user.</param>
		/// <param name="Username" mandatory="true">User or short name for the new user.</param>
		/// <param name="Password" mandatory="true">Password associated to the user.</param>
		/// <param name="Firstname" mandatory="false">First or given name associated to the user.</param>
		/// <param name="Lastname" mandatory="false">Last or family name associated to the user.</param>
		/// <param name="Fullname" mandatory="false">Full or complete name associated to the user.</param>
		/// <param name="Distinguished" mandatory="false">Distinguished name of the user.</param>
		/// <returns>RegistrationStatus</returns>
		/// Note: Method assumes the mandatory arguments: Username, Password have already 
		/// been validated.  Also assumes the username does NOT exist in the domain
		public
		RegistrationInfo
		Create(
			string Userguid,
			string Username,
			string Password,
			string Firstname,
			string Lastname,
			string Fullname,
			string Distinguished )
		{
			Member member = null;
			RegistrationInfo info = new RegistrationInfo();
			
			if ( domain == null )
			{
				log.Debug( "Domain instance == null" );
			}
			
			try
			{
				member = domain.GetMemberByName( Username );
				if ( member == null )
				{
					string guid = ( Userguid != null && Userguid != "" ) 
							? Userguid : Guid.NewGuid().ToString();
							
					member = 
						new Member(
								Username,
								guid, 
								Access.Rights.ReadOnly,
								Firstname,
								Lastname );

					// Simias server stores a simple MD5 hash of the password
					Property pwd = new Property( InternalUser.pwdProperty,	HashPassword ( Password ) );
					pwd.LocalProperty = true;
					member.Properties.ModifyProperty( pwd );
							
					member.FN = ( Fullname != null ) ? Fullname : Firstname + " " + Lastname;

					Property dnProp = 
						( Distinguished != null && Distinguished != "" )
								? new Property( "DN", Distinguished )
								: new Property( "DN", Username);
					member.Properties.ModifyProperty( dnProp );
					domain.SetType( member as Node, InternalUser.userType );
					domain.SetType( member as Node, InternalUser.memberMarker );
					domain.Commit( member );
							
					info.Status = RegistrationStatus.UserCreated;
					info.Message = "Successful";
					info.UserGuid = guid;
				}
				else
				{
					info.Status = RegistrationStatus.UserAlreadyExists;
					info.Message = "Specified user already exists!";
				}
			}
			catch( Exception e1 )
			{
				log.Error( e1.Message );
				log.Error( e1.StackTrace );
				info.Status = RegistrationStatus.InternalException;
				info.Message = e1.Message;
			}			
			
			return info;
		}
		
		/// <summary>
		/// Method to delete a user from the external identity/user database.
		/// Some external systems may not allow deletion of users.
		/// </summary>
		/// <param name="Username">Name of the user to delete from the external system.</param>
		/// <returns>true - successful  false - failed</returns>
		public bool Delete( string Username )
		{
			bool status = false;
			
			try
			{
				Member member = domain.GetMemberByName( Username );
				if ( member != null )
				{
					domain.Commit( domain.Delete( member ) );
					status = true;
				}
			}
			catch( Exception e1 )
			{
				log.Error( e1.Message );
				log.Error( e1.StackTrace );
			}
			
			return status;
		}
		
		/// <summary>
		/// Method to retrieve the capabilities of a user identity
		/// provider.
		/// </summary>
		/// <returns>providers capabilities</returns>
		public UserProviderCaps GetCapabilities()
		{
			UserProviderCaps caps = new UserProviderCaps();
			caps.CanCreate = true;
			caps.CanDelete = true;
			caps.CanModify = true;
			caps.ExternalSync = false;
			
			return caps;
		}

		/// <summary>
		/// Method to set/reset a user's password
		/// Note: This method will be replaced when the self-service
		/// framework is designed and implemented.
		/// </summary>
		/// <param name="Username" mandatory="true">Username to set the password on.</param>
		/// <param name="Password" mandatory="true">New password.</param>
		/// <returns>true - successful</returns>
		public bool SetPassword( string Username, string Password )
		{
			bool status = false;

			try
			{
				Member member = domain.GetMemberByName( Username );
				if ( member != null )
				{
					Property pwd = new Property( InternalUser.pwdProperty, HashPassword( Password ) );
					pwd.LocalProperty = true;

					member.Properties.ModifyProperty( pwd );
					domain.Commit( member );
					status = true;
				}
			}
			catch( Exception e1 )
			{
				log.Error( e1.Message );
				log.Error( e1.StackTrace );
			}			
			
			return status;
		}
		
		/// <summary>
		/// Method to verify a user's password
		/// </summary>
		/// <param name="Username">User to verify the password against</param>
		/// <param name="Password">Password to verify</param>
		/// <param name="status">Structure used to pass additional information back to the user.</param>
		/// <returns>true - Valid password, false Invalid password</returns>
		public bool VerifyPassword( string Username, string Password, Simias.Authentication.Status status )
		{
			bool result;
			
			try
			{
				Member member = domain.GetMemberByName( Username );
				if ( member != null )
				{
					Property pwd = member.Properties.GetSingleProperty( InternalUser.pwdProperty );
					if ( pwd != null && ( pwd.Value as string == HashPassword( Password ) ) )
					{
						status.UserID = member.UserID;
						status.statusCode = SCodes.Success;
						result = true;
					}
					else
					{
						status.statusCode = SCodes.InvalidCredentials;
						result = false;
					}
				}
				else
				{
					log.Debug( "failed to find user: {0}", Username );
					status.statusCode = SCodes.UnknownUser;
					result = false;
				}
			}
			catch( Exception e1 )
			{
				log.Error( e1.Message );
				log.Error( e1.StackTrace );
				status.statusCode = SCodes.InternalException;
				status.ExceptionMessage = e1.Message;
				result = false;
			}			
			
			return result;
		}
		#endregion
	}	
}
