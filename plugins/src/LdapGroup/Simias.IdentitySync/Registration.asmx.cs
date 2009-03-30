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
using System.Security.Cryptography;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

using Simias;
using Simias.Storage;

namespace Simias.Identity
{
	/// <summary>
	/// The registration status.
	/// </summary>
	public enum RegistrationStatus
	{
		/// <summary>
		/// The user was created.
		/// </summary>
		UserCreated = 0,

		/// <summary>
		/// The user already exists.
		/// </summary>
		UserAlreadyExists,

		/// <summary>
		/// Invalid parameters were specified.
		/// </summary>
		InvalidParameters,

		/// <summary>
		/// Invalid domain was specified.
		/// </summary>
		InvalidDomain,

		/// <summary>
		/// Username policy exception.
		/// </summary>
		UsernamePolicyException,

		/// <summary>
		/// Password policy exception.
		/// </summary>
		PasswordPolicyException,

		/// <summary>
		/// No user providers are registered.
		/// </summary>
		NoRegisteredUserProvider,

		/// <summary>
		/// The method is not supported.
		/// </summary>
		MethodNotSupported,

		/// <summary>
		/// An internal exception occurred.
		/// </summary>
		InternalException
	}
	
	/// <summary>
	/// Class that represents the current state and configuration
	/// of the synchronization service.
	/// </summary>
	[ Serializable ]
	public class RegistrationInfo
	{
		/// <summary>
		/// Constructs a RegistrationInfo object.
		/// </summary>
		public RegistrationInfo()
		{
		}

		/// <summary>
		/// Constructs a RegistrationInfo object.
		/// </summary>
		/// <param name="StatusCode">The status of the registration.</param>
		public RegistrationInfo( RegistrationStatus StatusCode )
		{
			Status = StatusCode;
		}
		
		/// <summary>
		/// Status result from a create or delete
		/// method
		/// </summary>
		public RegistrationStatus Status;
		
		/// <summary>
		/// Message returned from the CreateUser method.
		/// </summary>
		public string Message;
		
		/// <summary>
		/// Guid assigned to the user.
		/// Not valid if the registration method fails.
		/// </summary>
		public string UserGuid;
		
		/// <summary>
		/// Distinguished Name in the external identity database.
		/// Not valid if the registration method fails.
		/// </summary>
		public string DistinguishedName;
		
		/// <summary>
		/// If the Registration.CreateUser method fails with a
		/// UserExists status, the provider MAY return a list of
		/// suggested names the caller could try.
		/// </summary>
		public string[] SuggestedNames;
	}
	
	/// <summary>
	/// Registration
	/// Web service methods to manage the Identity Sync Service
	/// </summary>
	[WebService(
	 Namespace="http://novell.com/simias-server/registration",
	 Name="User Registration",
	 Description="Web Service providing self provisioning/registration for Simias users.")]
	public class Registration : System.Web.Services.WebService
	{
		private Store store = null;
		
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log =
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	
		/// <summary>
		/// Constructor
		/// </summary>
		public Registration()
		{
			store = Store.GetStore();
		}
		
		/// <summary>
		/// Method to get the domain's public key
		/// </summary>
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public
		string
		GetPublicKey()
		{
			Simias.Storage.Domain domain = store.GetDomain( store.LocalDomain );
			RSACryptoServiceProvider pubKey = domain.Owner.PublicKey;
			
			log.Debug( "Public Key: " + pubKey.ToString() );
			log.Debug( "Public Key (XML): " + pubKey.ToXmlString( false ) );
			
			return pubKey.ToXmlString( false );
		}
		
		/// <summary>
		/// Method to add/create a new user in the system.
		/// </summary>
		/// <param name="Username">Username (mandatory) short name of the user</param>
		/// <param name="Password">Password (mandatory)</param>
		/// <param name="UserGuid">UserGuid (optional) caller can specify the guid for the user</param>
		/// <param name="FirstName">FirstName (optional) first/given name of the user</param>
		/// <param name="LastName">LastName (optional) last/family name of the user</param>
		/// <param name="FullName">FullName (optional) Fullname of the user</param>
		/// <param name="DistinguishedName">DistinguishedName (optional) usually the distinguished name from an external identity store</param>
		/// <param name="Email">Email (optional) Primary email address</param>
		/// <remarks>
		/// If the FirstName and LastName are specified but the FullName is null, FullName is
		/// autocreated using: FirstName + " " + LastName
		/// </remarks>
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public
		RegistrationInfo
		CreateUser(
			string 	Username,
			string 	Password,
			string 	UserGuid,
			string 	FirstName,
			string 	LastName,
			string 	FullName,
			string	DistinguishedName,
			string	Email)
		{
			RegistrationInfo info;
			
			if ( Username == null || Username == "" || Password == null )
			{
				info = new RegistrationInfo( RegistrationStatus.InvalidParameters );
				info.Message = "Missing mandatory parameters";
				log.Info( "called with missing mandatory parameters" );
			}
			else
			{
				Simias.Identity.User user = new Simias.Identity.User( Username );
				user.FirstName = FirstName;
				user.LastName = LastName;
				user.UserGuid = UserGuid;
				user.FullName = FullName;
				user.DN = DistinguishedName;
				user.Email = Email;
			
				info = user.Create( Password );
			}
			
			return info;
		}
	}
}
