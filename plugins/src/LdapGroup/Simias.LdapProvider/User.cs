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
using System.Text;
using System.Web;
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Syscert = System.Security.Cryptography.X509Certificates;
using Mono.Security.X509;
using Mono.Security.Cryptography;

using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Utilclass;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Identity;

using SCodes = Simias.Authentication.StatusCodes;

namespace Simias.LdapProvider
{
	/// <summary>
	/// Implementation of the IUserProvider Service for
	/// the ldap provider.
	///
	/// NOTE! This ldap provider implementation does NOT allow 
	/// user creation and deletion via the IUserProvider interface
	///	so those methods will return false.
	/// </summary>
	public class User : Simias.Identity.IUserProvider
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
		static private string providerName = "LDAP Provider";
		static private string providerDescription = "A provider to sync identities from a LDAP to a Simias domain";
		
		static private string missingDomainMessage = "Enterprise domain does not exist!";

		// Frequently used Simias types
		private Store store = null;
		private Simias.Storage.Domain domain = null;
		private LdapSettings ldapSettings = null;
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

		private bool CertFailure = false;
		
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of this object.
		/// </summary>
		public User()
		{
			store = Store.GetStore();
			if ( store.DefaultDomain == null )
			{
				throw new SimiasException( User.missingDomainMessage );
			}
			
			// Is the default domain always the correct domain?
			domain = store.GetDomain( store.DefaultDomain );
			if ( domain == null )
			{
				throw new SimiasException( User.missingDomainMessage );
			}
			
			if ( domain.IsType( "Enterprise" ) == false )
			{
				throw new SimiasException( User.missingDomainMessage );
			}
			
			ldapSettings = LdapSettings.Get( Store.StorePath );
			
			
			// Make sure the password is set on the admin
			SetAdminPassword();
		}
		#endregion
		
		#region Private Methods
		
		//
		// When the Enterprise Server Domain is instantiated in Simias.Identity
		// the domain owner is determined via the AdminName in Simias.config.
		// Simple Server determines the domain owner via an attribute in
		// the SimpleServer.xml file so this method will actually change
		// ownership of the domain to the one specified in the xml file
		// unless they happen to be the same.
		//
		private bool SetAdminPassword()
		{
			bool status = false;

			/*
			// Domain administrator is determined by "owner" attribute
			// in the SimpleServer.xml document
			
			string identityDocumentPath = "../../etc/SimpleServer.xml";
			string owner = null;
			string ownerPassword = null;

			try
			{
				// Load the SimpleServer domain and memberlist XML file.
				XmlDocument serverDoc = new XmlDocument();
				serverDoc.Load( identityDocumentPath );
				XmlElement domainElement = serverDoc.DocumentElement;
				XmlAttribute attr;
				
				for ( int i = 0; i < domainElement.ChildNodes.Count; i++ )
				{
					attr = domainElement.ChildNodes[i].Attributes[ "Owner" ];
					if (attr != null)
					{
						XmlNode cNode = domainElement.ChildNodes[i];
						owner = cNode.Attributes[ "Name" ].Value;
						ownerPassword = cNode.Attributes[ "Password" ].Value;
						break;
					}	
				}
			}
			catch(Exception e)
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}
			
			if ( owner != null && ownerPassword != null )
			{
				Member ownerMember = domain.Owner;
				if ( ownerMember.Name != owner )
				{
					// New owner must first be a member before ownership
					// can be transfered
					Member adminMember =
						new Member(
							owner,
							Guid.NewGuid().ToString(),
							Simias.Storage.Access.Rights.ReadOnly,
							null,
							null );
							
					domain.Commit( adminMember );		
					domain.Commit( domain.ChangeOwner( adminMember, Simias.Storage.Access.Rights.Admin ) );

					// Now remove the old member
					domain.Commit( domain.Delete( domain.Refresh( ownerMember ) ) );
					ownerMember = adminMember;
				}
				
				Property pwd = 
					ownerMember.Properties.GetSingleProperty( User.pwdProperty );
				if ( pwd == null || pwd.Value == null )
				{
					pwd = new Property( User.pwdProperty, HashPassword( ownerPassword ) );
					ownerMember.Properties.ModifyProperty( pwd );

					domain.Commit( ownerMember );
					status = true;
				}
			}
			*/
			
			return status;
		}		
		
		/// <summary>
		/// Gets the login status for the specified user.
		/// </summary>
		/// <param name="connection">Ldap connection to use to get the status.</param>
		/// <param name="status">User information.</param>
		private void GetUserStatus( bool proxyUser, LdapConnection connection, Simias.Authentication.Status status )
		{
			if ( connection != null )
			{
				// Get the search attributes for login status.
				string[] searchAttributes = { 
												"loginDisabled", 
												"loginExpirationTime", 
												"loginGraceLimit", 
												"loginGraceRemaining",
												"passwordAllowChange",
												"passwordRequired",
												"passwordExpirationTime"
											};
				LdapEntry ldapEntry = connection.Read( status.DistinguishedUserName, searchAttributes );
				if ( ldapEntry != null )
				{
					// If the account has been disabled or the account has expired
					// the bind will fail and we'll come through on the proxy user
					// connection so there is no reason for the extra checking on
					// a successful bind in the context of the actual user.
					if ( proxyUser == true && LoginDisabled( ldapEntry ) == true )
					{
						status.statusCode = SCodes.AccountDisabled;
					}
					else
					if ( proxyUser == true && LoginAccountExpired( ldapEntry ) == true )
					{
						status.statusCode = SCodes.AccountDisabled;
					}
					else
					{
						if ( LoginIsPasswordRequired( ldapEntry ) == true )
						{
							if ( LoginCanUserChangePassword( ldapEntry ) == true )
							{
								if ( LoginPasswordExpired( ldapEntry ) == true )
								{
									status.TotalGraceLogins = LoginGraceLimit( ldapEntry );
									status.RemainingGraceLogins = LoginGraceRemaining( ldapEntry );

									if ( status.statusCode == SCodes.Success &&
										( status.TotalGraceLogins == -1 ||
										status.RemainingGraceLogins >= 0 ) )
									{
										status.statusCode = SCodes.SuccessInGrace;
									}
									else
									{
										status.statusCode = SCodes.AccountLockout;
									}
								}
							}
						}
					}
				}
				else
				{
					status.statusCode = SCodes.InternalException;
					status.ExceptionMessage = "Failed reading LDAP attributes";
				}
			}
		}
		
		/// <summary>
		/// Creates a proxy connection to retrieve user information for.
		/// 
		/// NOTE: This connection is used instead of the connection created by
		/// the Service.Start method because there cannot be multiple outstanding
		/// requests made on a single ldap connection.
		/// </summary>
		/// <returns>The LdapConnection object if successful. Otherwise a null is returned.</returns>
		private LdapConnection BindProxyUser()
		{
			LdapConnection proxy;

			try
			{
				proxy = new LdapConnection();
				proxy.SecureSocketLayer = ldapSettings.SSL;
				proxy.Connect( ldapSettings.Host, ldapSettings.Port );

				Simias.LdapProvider.ProxyUser proxyCredentials = 
					new Simias.LdapProvider.ProxyUser();

				proxy.Bind( proxyCredentials.UserDN, proxyCredentials.Password );
			}
			catch( LdapException e )
			{
				log.Error( "LdapError:" + e.LdapErrorMessage );
				log.Error( "Error:" + e.Message );
				proxy = null;
			}
			catch( Exception e )
			{
				log.Error( "Error:" + e.Message );
				proxy = null;
			}

			return proxy;
		}
		
		/// <summary>
		/// Gets the distinguished name from the member name.
		/// </summary>
		/// <param name="domainID">The identifier for the domain.</param>
		/// <param name="user">The user name.</param>
		/// <param name="distinguishedName">Receives the ldap distinguished name.</param>
		/// <param name="id">Receives the member's user ID.</param>
		/// <returns>True if the distinguished name was found.</returns>
		private bool GetUserDN( string user, out string distinguishedName, out string id )
		{
			bool status = false;
			Member member = null;

			// Initialize the outputs.
			distinguishedName = String.Empty;
			id = String.Empty;

			if ( domain != null )
			{
				member = domain.GetMemberByName( user );
				if ( member != null )
				{
					Property dn = member.Properties.GetSingleProperty( "DN" );
					if ( dn != null )
					{
						distinguishedName = dn.ToString();
						id = member.UserID;
						status = true;
					}
				}
				else
				{
					// The specified user did not exist in the roster under 
					// the short or common name.
					// Let's see if the user came in fully distinguished.
					// ex. cn=user.o=context

					string dn = user.ToLower();
					if ( dn.StartsWith( "cn=" ) == true )
					{
						// NDAP name to LDAP name
						dn = dn.Replace( '.', ',' );
						ICSList dnList = domain.Search( "DN", dn, SearchOp.Equal );
						if ( dnList != null && dnList.Count == 1 )
						{
							IEnumerator dnEnum = dnList.GetEnumerator();
							if ( dnEnum.MoveNext() == true )
							{
								member = new Member( domain, dnEnum.Current as ShallowNode );
								if ( member != null )
								{
									distinguishedName = dn;
									id = member.UserID;
									status = true;
								}
							}
						}
					}
				}
			}
		
			return status;
		}
		
		/// <summary>
		/// Checks if the user's account has expired
		/// </summary>
		/// <param name="entry">LdapEntry</param>
		/// <returns>true - account expired/false - account still valid or no policy</returns>
		private bool LoginAccountExpired( LdapEntry entry )
		{
			bool expired = false;

			LdapAttribute attrExpiration = entry.getAttribute( "loginExpirationTime" );
			if ( attrExpiration != null )
			{
				char[] exp = attrExpiration.StringValue.ToCharArray();

				DateTime expiredPolicy =
					new DateTime(
					Convert.ToInt32( new String( exp, 0, 4 ) ),
					Convert.ToInt32( new String( exp, 4, 2 ) ),
					Convert.ToInt32( new String( exp, 6, 2 ) ),
					Convert.ToInt32( new String( exp, 8, 2 ) ),
					Convert.ToInt32( new String( exp, 10, 2 ) ),
					Convert.ToInt32( new String( exp, 12, 2 ) ) );

				if ( DateTime.UtcNow > expiredPolicy )
				{
					expired = true;
				}
			}

			return expired;
		}

		/// <summary>
		/// Checks if the user is allowed to change their password
		/// </summary>
		/// <param name="entry">LdapEntry</param>
		/// <returns>true - user can change their password/false - can't change</returns>
		private bool LoginCanUserChangePassword( LdapEntry entry )
		{
			bool canChange = false;

			LdapAttribute attrAllowChange = entry.getAttribute( "passwordAllowChange" );
			if ( attrAllowChange != null )
			{
				if ( attrAllowChange.StringValue.ToLower() == "true" )
				{
					canChange = true;
				}
			}

			return canChange;
		}

		/// <summary>
		/// Checks if a password is required
		/// </summary>
		/// <param name="entry">LdapEntry</param>
		/// <returns>true/false</returns>
		private bool LoginIsPasswordRequired( LdapEntry entry )
		{
			bool required = false;

			LdapAttribute attrPasswordRequired = entry.getAttribute( "passwordRequired" );
			if ( attrPasswordRequired != null )
			{
				if ( attrPasswordRequired.StringValue.ToLower() == "true" )
				{
					required = true;
				}
			}

			return required;
		}

		/// <summary>
		/// Gets the login disabled status.
		/// </summary>
		/// <param name="entry">LdapEntry</param>
		/// <returns>True if login is disabled.</returns>
		private bool LoginDisabled( LdapEntry entry )
		{
			bool loginDisabled;
			LdapAttribute attrLoginDisabled = entry.getAttribute( "loginDisabled" );
			if ( attrLoginDisabled != null )
			{
				loginDisabled = Convert.ToBoolean( attrLoginDisabled.StringValue );
			}
			else
			{
				loginDisabled = false;
			}

			return loginDisabled;
		}

		/// <summary>
		/// Gets the login grace limit.
		/// </summary>
		/// <param name="entry">LdapEntry</param>
		/// <returns>The number of grace logins.</returns>
		private int LoginGraceLimit( LdapEntry entry )
		{
			int loginGraceLimit;

			LdapAttribute attrLoginGraceLimit = entry.getAttribute( "loginGraceLimit" );
			if ( attrLoginGraceLimit != null )
			{
				loginGraceLimit = Convert.ToInt32( attrLoginGraceLimit.StringValue );
			}
			else
			{
				// There is not a grace login limit set.
				loginGraceLimit = -1;
			}

			return loginGraceLimit;
		}

		/// <summary>
		/// Gets the number of grace logins remaining.
		/// </summary>
		/// <param name="entry">LdapEntry</param>
		/// <returns>The number grace logins remaining.</returns>
		private int LoginGraceRemaining(LdapEntry entry)
		{
			int loginGraceRemaining;

			LdapAttribute attrLoginGraceRemaining = entry.getAttribute( "loginGraceRemaining" );
			if ( attrLoginGraceRemaining != null )
			{
				loginGraceRemaining = Convert.ToInt32( attrLoginGraceRemaining.StringValue );
			}
			else
			{
				loginGraceRemaining = -1;
			}
			return loginGraceRemaining;
		}

		/// <summary>
		/// Checks if the user's password has expired
		/// </summary>
		/// <param name="entry">LdapEntry</param>
		/// <returns>true - password expired/false - password still valid or no policy</returns>
		private bool LoginPasswordExpired( LdapEntry entry )
		{
			bool expired = false;

			LdapAttribute attrExpiration = entry.getAttribute( "passwordExpirationTime" );
			if ( attrExpiration != null )
			{
				char[] exp = attrExpiration.StringValue.ToCharArray();

				DateTime expiredPolicy =
					new DateTime(
							Convert.ToInt32( new String( exp, 0, 4 ) ),
							Convert.ToInt32( new String( exp, 4, 2 ) ),
							Convert.ToInt32( new String( exp, 6, 2 ) ),
							Convert.ToInt32( new String( exp, 8, 2 ) ),
							Convert.ToInt32( new String( exp, 10, 2 ) ),
							Convert.ToInt32( new String( exp, 12, 2 ) ) );

				if ( DateTime.UtcNow > expiredPolicy )
				{
					expired = true;
				}
			}

			return expired;
		}
		#endregion
		
		#region Internal Methods
        /// <summary>
        /// Do a utf encoding of password and encrypt it
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
		static internal string HashPassword( string password )
		{
			UTF8Encoding utf8Encoding = new UTF8Encoding();
			MD5CryptoServiceProvider md5Service = new MD5CryptoServiceProvider();
		
			byte[] bytes = new Byte[ utf8Encoding.GetByteCount( password ) ];
			bytes = utf8Encoding.GetBytes( password );
			return Convert.ToBase64String( md5Service.ComputeHash( bytes ) );
		}
		#endregion
		
		#region Public Methods
		/// <summary>
		/// Method to create a user/identity in the external user database.
		/// Some external systems may not allow for creation of new users.
		/// </summary>
		/// <param name="Guid" mandatory="false">Guid associated to the user.</param>
		/// <param name="Username" mandatory="true">User or short name for the new user.</param>
		/// <param name="Password" mandatory="true">Password associated to the user.</param>
		/// <param name="Firstname" mandatory="false">First or given name associated to the user.</param>
		/// <param name="Lastname" mandatory="false">Last or family name associated to the user.</param>
		/// <param name="Fullname" mandatory="false">Full or complete name associated to the user.</param>
		/// <returns>RegistrationStatus</returns>
		/// Note: Method assumes the mandatory arguments: Username, Password have already 
		/// been validated.  Also assumes the username does NOT exist in the domain
		public
		Simias.Identity.RegistrationInfo
		Create(
			string Userguid,
			string Username,
			string Password,
			string Firstname,
			string Lastname,
			string Fullname,
			string Distinguished )
		{
			RegistrationInfo info = new RegistrationInfo( RegistrationStatus.MethodNotSupported );
			info.Message = "The LDAP Provider does not support creating users through the registration class";
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
			// Simple Server does not support delete through the registration APIs
			return false;
		}
		
		/// <summary>
		/// Method to retrieve the capabilities of a user identity
		/// provider.
		/// </summary>
		/// <returns>providers capabilities</returns>
		public UserProviderCaps GetCapabilities()
		{
			UserProviderCaps caps = new UserProviderCaps();
			caps.CanCreate = false;
			caps.CanDelete = false;
			caps.CanModify = false;
			caps.ExternalSync = true;
			
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
			// today we don't allow modification in the ldap provider
			return false;
		}

		/// <summary>
		/// SSLHandler: call back method to check certificate
		/// </summary>
		/// <param name="certificate">Actual certificate</param>
		/// <param name="certificateErrors">certificateErrors errors</param>
		/// <returns>certificate validity true or false.</returns>
		public bool SSLHandler( Syscert.X509Certificate certificate, int[] certificateErrors)
		{
			bool retFlag=true;

                        if (certificateErrors != null && certificateErrors.Length > 0)
				if( !(certificateErrors.Length==1 && certificateErrors[0] == -2146762481))
                                {
       		                        for (int i = 0; i < certificateErrors.Length; i++)
                                		log.Debug("Detected errors in the Server Certificate:{0}", certificateErrors[i]);
					retFlag = false;
                                	CertFailure = true;                                                                
				}
                        return retFlag;
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
			log.Debug( "VerifyPassword for: " + Username );

			LdapConnection conn = null;
			LdapConnection proxyConnection = null;

			// Get the distinguished name and member(user) id from the
			// simias store rather than the ldap server
			if ( GetUserDN( Username, out status.DistinguishedUserName, out status.UserID ) == false )
			{
				log.Debug( "failed to get the user's distinguished name" );
				status.statusCode = SCodes.UnknownUser;
				return false;
			}

			bool doNotCheckStatus = false;
			try
			{
				conn = new LdapConnection();
				conn.SecureSocketLayer = ldapSettings.SSL;
				conn.UserDefinedServerCertValidationDelegate += new CertificateValidationCallback( SSLHandler );
				conn.Connect( ldapSettings.Host, ldapSettings.Port );
				conn.Bind( status.DistinguishedUserName, Password );
				if ( conn.AuthenticationDN == null )
				{
					doNotCheckStatus = true;
					throw new LdapException( "Anonymous bind is not allowed", LdapException.INAPPROPRIATE_AUTHENTICATION, "Anonymous bind is not allowed" );
				}
				status.statusCode = SCodes.Success;
				GetUserStatus( false, conn, status );
				return ( true );
			}
			catch( LdapException e )
			{
				log.Error( "LdapError:" + e.LdapErrorMessage );
				log.Error( "Error:" + e.Message );
				log.Error( "DN:" + status.DistinguishedUserName );

				switch ( e.ResultCode )
				{
					case LdapException.INVALID_CREDENTIALS:
						status.statusCode = SCodes.InvalidCredentials;
						break;

					case LdapException.SSL_HANDSHAKE_FAILED:
					case LdapException.CONNECT_ERROR:
						if(CertFailure == true)
							status.statusCode = SCodes.InvalidCertificate;
						CertFailure = false;
						break;

					default:
						status.statusCode = SCodes.InternalException;
						break;
				}

				status.ExceptionMessage = e.Message;
				if ( !doNotCheckStatus )
				{
					proxyConnection = BindProxyUser();
					if ( proxyConnection != null )
					{
						// GetUserStatus may change the status code
						GetUserStatus( true, proxyConnection, status );
					}
				}
			}
			catch( Exception e )
			{
				log.Error( "Error:" + e.Message );
				status.statusCode = SCodes.InternalException;
				status.ExceptionMessage = e.Message;
				proxyConnection = BindProxyUser();
				if ( proxyConnection != null )
				{
					// GetUserStatus may change the status code
					GetUserStatus( true, proxyConnection, status );
				}
			}
			finally
			{
				if ( conn != null )
				{
					// In Mono 2.0 runtime environment, first connection.Disconnect()
					// always throws exception(bug 449092).
					// First disconnect always throws "The socket is not connected" Messages.
					// With this try, catch only ignoring that perticular Exception
					try
					{
						conn.Disconnect();
					}
					catch(Exception Ex)
					{
						if(String.Compare(Ex.Message,"The socket is not connected") != 0)
							throw Ex;
						else
							log.Info( "LdapConnection.Disconnect Exception {0} {1} ", Ex.Message, Ex.StackTrace );
					}
				}

				if ( proxyConnection != null )
				{
					try{
						proxyConnection.Disconnect();
					}catch{}
				}
			}

			return ( false );
		}
		#endregion
	}	
}
