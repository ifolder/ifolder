/***********************************************************************
 *  IIdentity.cs - Implements an interface allowing for creation,
 *  association and access of identity information.
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Mike Lasky <mlasky@novell.com>
 * 
 ***********************************************************************/

using System;
using System.Reflection;

namespace Simias.Identity
{
	/// <summary>
	/// This defines the interface that represents an alternate identity.
	/// </summary>
	public class KeyChainItem
	{
		#region Class Members
		/// <summary>
		/// Identifies the alternate user.
		/// </summary>
		private string userGuid;

		/// <summary>
		/// Specifies the domain that the user guid belongs to.
		/// </summary>
		private string domain;

		/// <summary>
		/// Credential to prove the identity.
		/// </summary>
		private string credential;
		#endregion

		#region Properties
		/// <summary>
		///  Gets the user guid for the alternate user.
		/// </summary>
		public string UserGuid
		{
			get { return userGuid; }
		}

		/// <summary>
		/// Gets the domain for the alternate user.
		/// </summary>
		public string DomainName
		{
			get { return domain; }
		}

		/// <summary>
		/// Gets the credential for the alternate user.
		/// </summary>
		public string Credential
		{
			get { return credential; }
		}
		#endregion

		#region Constructor
		/// <summary>
		///  Constructs a KeyChainItem object.
		/// </summary>
		/// <param name="userGuid">Unique identifier for an alternate user.</param>
		/// <param name="domain">Domain that the alternate user belongs to.</param>
		/// <param name="credential">Credential to prove the alternate user.</param>
		public KeyChainItem( string userGuid, string domain, string credential )
		{
			this.userGuid = userGuid.ToLower();
			this.domain = domain;
			this.credential = credential;
		}
		#endregion
	}

	/// <summary>
	/// This defines the interface that an identity provider must implement.
	/// </summary>
	public interface IIdentity
	{
		/// <summary>
		/// Gets the credential for the specified user guid.
		/// </summary>
		/// <param name="userGuid">User guid to retrieve credential for.</param>
		/// <returns>A string that represents the credential for the specified user guid.</returns>
		string GetCredentialFromUserGuid( string userGuid );

		/// <summary>
		/// Gets the identity key chain for this identity.  The identity key chain contains alternate
		/// identities that the user can be known by.
		/// </summary>
		/// <returns>An array of objects containing the alternate identities.</returns>
		KeyChainItem[] GetKeyChain();

		/// <summary>
		/// Gets the key chain item for the specified user guid.
		/// </summary>
		/// <param name="userGuid">User guid to get key chain item for.</param>
		/// <returns>A KeyChainItem object.</returns>
		KeyChainItem GetKeyChainItem( string userGuid );

		/// <summary>
		/// Gets a previously stored secret.  The secret is a key value pair.
		/// </summary>
		/// <param name="key">Key that is used to store the secret with.</param>
		/// <returns></returns>
		string GetSecret(string key);

		/// <summary>
		/// Gets a user guid that is associated with this identity and belongs to the specified domain.
		/// </summary>
		/// <param name="domain">Domain that alternate guid belongs to.</param>
		/// <returns>A string that represents an alternate guid.  Null will be returned if there is no alternate
		/// guid associated with the specified domain.</returns>
		string GetUserGuidFromDomain( string domain );

		/// <summary>
		/// Sets an alternate identity in the user's key chain.
		/// </summary>
		/// <param name="domain">Domain that the alternate identity belongs to.</param>
		/// <param name="userGuid">A unique identifier for an alternate user.</param>
		/// <param name="credential">Credential to prove the alternate user.</param>
		void SetKeyChainItem( string domain, string userGuid, string credential );

		/// <summary>
		/// Sets a secret by the specified key.
		/// </summary>
		/// <param name="key">Key that is used to reference the secret.</param>
		/// <param name="secret">The secret to be stored.</param>
		void SetSecret(string key, string secret);

		/// <summary>
		/// Returns a constant globally unique identifier that represents this identity.
		/// </summary>
		string UserGuid
		{
			get;
		}

		/// <summary>
		/// Returns the credential associated with this identity.
		/// </summary>
		string Credential
		{
			get;
		}

		/// <summary>
		/// Returns the user identifier ( e.g. user name ) for this identity.
		/// </summary>
		string UserId
		{
			get;
		}

		/// <summary>
		/// Returns the name of the domain that the identity exists in.
		/// </summary>
		string DomainName
		{
			get;
		}
	}

	/// <summary>
	/// Defines the factory interface that an identity provider must implement.
	/// </summary>
	public interface IIdentityFactory
	{
		/// <summary>
		/// Method to Create a new Identity.
		/// </summary>
		/// <param name="id">ID to create.</param>
		/// <param name="credential">An object that represents the credentials.</param>
		/// <returns></returns>
		IIdentity Create(string id, object credential);

		/// <summary>
		/// Method to Authenticate an Identity.
		/// </summary>
		/// <param name="id">ID to authenticate.</param>
		/// <param name="credential">An object that represents the credentials.</param>
		/// <returns></returns>
		IIdentity Authenticate(string id, object credential);

		/// <summary>
		/// Method that returns the identity that the user Id is associated with.
		/// </summary>
		/// <param name="userGuid">Unique user identity to look up main identity with.</param>
		/// <returns>An interface to an Identity if successful, otherwise a null.</returns>
		IIdentity GetIdentityFromUserGuid( string userGuid );

		/// <summary>
		/// Gets the identity of the currently executing security context.
		/// </summary>
		IIdentity CurrentId
		{
			get;
		}
	}

	/// <summary>
	/// Class to connect to a specific identity provider
	/// </summary>
	public class IdentityManager
	{
		/// <summary>
		/// Method to connect to a specific identity provider.
		/// </summary>
		/// <returns></returns>
		public static IIdentityFactory Connect()
		{
			string className = "Simias.Identity.WorkGroupIdentityFactory";
			string assemblyName = "WorkGroupIdentity";
			IIdentityFactory factory = null;

			// Load the assembly and find our provider.
			Assembly factoryAssembly = AppDomain.CurrentDomain.Load(assemblyName);

			factory = (IIdentityFactory)factoryAssembly.CreateInstance(className);
			return (factory);
		}
	}
}
