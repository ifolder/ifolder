/***********************************************************************
 *  StoreIdentity.cs - Class that implements identity management which
 *  provides access control to collection objects in the store.
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
using System.Collections;
using System.Xml;
using Simias.Identity;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
	/// <summary>
	/// Used to hold impersonation information on the impersonation stack.
	/// </summary>
	internal struct ImpersonationInfo
	{
		#region Class Members
		/// <summary>
		/// Guid the current impersonating identity is known as.
		/// </summary>
		public string userGuid;

		/// <summary>
		/// The identity object of the impersonating user.  This member will be null if the userGuid is
		/// a well-known role.
		/// </summary>
		public IIdentity identity;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for the struct.
		/// </summary>
		/// <param name="userGuid">User guid of the impersonating user.</param>
		/// <param name="identity">Identity that userGuid represents.  This may be null if userGuid is a well
		/// known role.</param>
		public ImpersonationInfo( string userGuid, IIdentity identity )
		{
			this.userGuid = userGuid;
			this.identity = identity;
		}
		#endregion
	}

	/// <summary>
	/// Implements the store identity which is used to control access to Collection Store objects.
	/// There is only one identity that is allowed to authenticate to the CollectionStore database, 
	/// since the database only ever has one owner.  All other identities can access the database 
	/// only by impersonation.
	/// </summary>
	internal class StoreIdentity
	{
		#region Class Members
		/// <summary>
		/// Represents the identity of the user that is logged onto to this workstation.
		/// </summary>
		private IIdentity identity;

		/// <summary>
		/// Container used to keep track of the current identity for this store handle.
		/// </summary>
		private Stack impersonationId = new Stack();
		#endregion

		#region Properties
		/// <summary>
		/// Gets the currently impersonating user guid.
		/// </summary>
		public string CurrentUserGuid
		{
			get { return ( impersonationId.Count == 0 ) ? identity.UserGuid : ( ( ImpersonationInfo )impersonationId.Peek() ).userGuid; }
		}

		/// <summary>
		/// Gets the current impersonating identity.
		/// </summary>
		public IIdentity CurrentIdentity
		{
			get { return ( impersonationId.Count == 0 ) ? identity : ( ( ImpersonationInfo )impersonationId.Peek() ).identity; }
		}

		/// <summary>
		/// Gets the store owner identity.
		/// </summary>
		public string OwnerId
		{
			get { return identity.UserGuid; }
		}

		/// <summary>
		/// Gets the domain name where this identity exists.
		/// </summary>
		public string DomainName
		{
			get { return identity.DomainName; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor of the object.
		/// </summary>
		/// <param name="identity">Object that represents the current identity.</param>
		private StoreIdentity( IIdentity identity )
		{
			this.identity = identity;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the database collection.
		/// </summary>
		/// <param name="store">Handle to the Collection Store.</param>
		/// <param name="id">Identifier of owner opening this collection.</param>
		/// <returns>A Collection object that represents the database collection.</returns>
		private Collection GetDatabaseCollection( Store store, string id )
		{
			Persist.IResultSet iterator;
			char[] results = new char[ 256 ];

			// If the store owner is doing the search, return all collections in his store.
			Persist.Query query = new Persist.Query( Property.ObjectType, Persist.Query.Operator.Equal, Node.CollectionType + Store.DatabaseType, Property.Syntax.String.ToString() );
			iterator = store.StorageProvider.Search( query );
			if ( iterator != null )
			{
				// Get the first set of results from the query.
				int length = iterator.GetNext( ref results );
				iterator.Dispose();

				// Make sure that something was put in the buffer.
				if ( length > 0 )
				{
					XmlDocument list = new XmlDocument();
					list.LoadXml( new string( results, 0, length ) );
					XmlNode dbNode = list.DocumentElement.FirstChild;
					return new Collection( store, dbNode.Attributes[ Property.NameAttr ].Value, dbNode.Attributes[ Property.IDAttr ].Value, dbNode.Attributes[ Property.TypeAttr ].Value, id );
				}
				else
				{
					throw new ApplicationException( "No database object exists" );
				}
			}
			else
			{
				throw new ApplicationException( "No database object exists" );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Authenticates the identity that owns the database.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <returns>A store identity object that represents the database owner.</returns>
		static public StoreIdentity Authenticate( Store store )
		{
			// Get the user that we are currently running as.
			IIdentity identity = IdentityManager.Connect().CurrentId;
			StoreIdentity storeIdentity = new StoreIdentity( identity );

			// Get the database object to check if this ID is the same as the owner.
			Collection db = storeIdentity.GetDatabaseCollection( store, identity.UserGuid );
			if ( db == null )
			{
				throw new ApplicationException( "Store database object does not exist" );
			}

			// Get the owner property and make sure that it is the same as the current user.
			Property owner = db.Properties.GetSingleProperty( Property.Owner );
			if ( ( owner == null ) || ( owner.ToString() != storeIdentity.CurrentUserGuid ) )
			{
				throw new UnauthorizedAccessException( "Current user is not store owner." );
			}

			return storeIdentity;
		}

		/// <summary>
		/// Creates an identity representing the database owner.
		/// </summary>
		/// <returns>A store identity object that represents the database owner.</returns>
		static public StoreIdentity CreateIdentity()
		{
			// Store the impersonation id on the current user.
			IIdentity identity = IdentityManager.Connect().CurrentId;
			return new StoreIdentity( identity );
		}

		/// <summary>
		/// Gets the identity of the current user and all of its aliases.
		/// </summary>
		/// <returns>An array list of guids that represents the current user and all of its aliases.</returns>
		public ArrayList GetIdentityAndAliases()
		{
			ArrayList ids = new ArrayList();
			ids.Add( CurrentUserGuid );

			IIdentity currentId = CurrentIdentity;
			if ( currentId != null )
			{
				KeyChainItem[] keyChain = currentId.GetKeyChain();
				foreach ( KeyChainItem kcItem in keyChain )
				{
					ids.Add( kcItem.UserGuid );
				}
			}

			return ids;
		}

		/// <summary>
		/// Returns the user guid that the current user is known as in the specified domain.
		/// </summary>
		/// <param name="domain">The domain that the user is in.</param>
		/// <returns>A string representing the user's guid in the specified domain.  If the user does not exist
		/// in the specified domain, the current user guid is returned.</returns>
		public string GetDomainUserGuid( string domain )
		{
			string userGuid = null;

			// If no domain is speicified, use the current identity.
			if ( domain != null )
			{
				IIdentity currentId = CurrentIdentity;
				if ( currentId != null )
				{
					userGuid = currentId.GetUserGuidFromDomain( domain );
				}
			}

			return ( userGuid == null ) ? CurrentUserGuid : userGuid;
		}

		/// <summary>
		/// Impersonates the specified identity, if the userId is verified.
		/// TODO: May want to look at limiting who can impersonate.
		/// </summary>
		/// <param name="userId">User ID to impersonate.</param>
		/// <param name="credential">Credential to verify the user ID.</param>
		public void Impersonate( string userId, object credential )
		{
			switch ( userId )
			{
				case Access.StoreAdminRole:
					impersonationId.Push( new ImpersonationInfo( userId, null ) );
					break;

				case Access.BackupOperatorRole:
					impersonationId.Push( new ImpersonationInfo( userId, null ) );
					break;

				case Access.SyncOperatorRole:
					impersonationId.Push( new ImpersonationInfo( userId, null ) );
					break;

				default:
					IIdentity identity = IdentityManager.Connect().Authenticate( userId, credential );
					impersonationId.Push( new ImpersonationInfo( identity.UserGuid, identity ) );
					break;
			}
		}

		/// <summary>
		/// Reverts back to the previous impersonating identity.
		/// </summary>
		public void Revert()
		{
			// Don't ever pop off the authenticated identity.
			if ( impersonationId.Count > 0 )
			{
				impersonationId.Pop();
			}
		}
		#endregion
	}
}
