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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Net;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Invite;
using Novell.AddressBook;

namespace Novell.iFolder
{
	/// <summary>
	/// Provides methods for manipulating the properties and contents 
	/// of an iFolder.
	/// </summary>
	public class iFolder
	{
		#region Class Members
		private Store							store;
		private	Collection						collection = null;
		private	Novell.AddressBook.AddressBook	ab = null;
		private Novell.AddressBook.Manager 		abMan;

		/// <summary>
		/// Type of collection that represents an iFolder.
		/// </summary>
		internal const string iFolderType = "iFolder";

		/// <summary>
		/// A file in an iFolder collection.
		/// </summary>
		internal const string iFolderFileType = "File";

		/// <summary>
		/// A directory in an iFolder collection.
		/// </summary>
		internal const string iFolderDirectoryType = "Directory";


		/// <summary>
		/// Access rights used by the iFolder.
		/// These rights are mutually exclusive.
		/// </summary>
		public enum Rights
		{
			/// <summary>
			/// User has no rights to the iFolder.
			/// </summary>
			Deny = Access.Rights.Deny,

			/// <summary>
			/// User can view information in an iFolder.
			/// </summary>
			ReadOnly = Access.Rights.ReadOnly,

			/// <summary>
			/// User can view and modify information in an iFolder.
			/// </summary>
			ReadWrite = Access.Rights.ReadWrite,

			/// <summary>
			/// User can view, modify and change rights in an iFolder.
			/// </summary>
			Admin = Access.Rights.Admin
		};
		#endregion

		#region Constructors
		internal iFolder(Store store, Novell.AddressBook.Manager manager)
		{
			this.store = store;
			this.abMan = manager;
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Add a single directory to an iFolder.
		/// </summary>
		/// <param name="dirPath">The path to the directory to add.</param>
		/// <param name="dirNode">Parent DirNode.</param>
		private iFolderNode AddiFolderDirNode( string dirPath, DirNode dirNode )
		{
			// Make sure the node doesn't already exist.
			iFolderNode ifNode = GetiFolderNodeByPath( dirPath );
			if ( ifNode == null )
			{
				if ( dirNode == null )
				{
					dirNode = GetNodeForPath( Path.GetDirectoryName( dirPath ) ) as DirNode;
					if ( dirNode == null )
					{
						throw new FileNotFoundException();
					}
				}

				// Create the node.
				DirNode childDirNode = new DirNode( collection, dirNode, Path.GetFileName( dirPath ) );
				collection.Commit( childDirNode );
				ifNode = new iFolderNode( this, childDirNode );
			}

			return ifNode;
		}

		/// <summary>
		/// Add a single file to an iFolder.
		/// </summary>
		/// <param name="filePath">The path to the file to add.</param>
		/// <param name="dirNode">Parent DirNode.</param>
		private iFolderNode AddiFolderFileNode( string filePath, DirNode dirNode )
		{
			// Make sure the node doesn't already exist.
			iFolderNode ifNode = GetiFolderNodeByPath( filePath );
			if ( ifNode == null )
			{
				if ( dirNode == null )
				{
					dirNode = GetNodeForPath( Path.GetDirectoryName( filePath ) ) as DirNode;
					if ( dirNode == null )
					{
						throw new FileNotFoundException();
					}
				}

				// Create the node.
				FileNode fileNode = new FileNode( collection, dirNode, Path.GetFileName( filePath ) );
				collection.Commit( fileNode );
				ifNode = new iFolderNode( this, fileNode );
			}

			return ifNode;
		}

		/// <summary>
		/// Add a files/directories to an iFolder recursively.
		/// </summary>
		/// <param name="path">The path at which to begin the recursive add.</param>
		/// <param name="dirNode">Parent DirNode.</param>
		private void AddiFolderNodes( string path, DirNode dirNode )
		{
			// Always add the specified path.
			if ( File.Exists( path ) )
			{
				AddiFolderFileNode( path, dirNode );
			}
			else if ( Directory.Exists( path ) )
			{
				DirNode parentDirNode = AddiFolderDirNode( path, dirNode );

				// Get the files in this directory.
				string[] dirs = Directory.GetFiles( path );
				foreach ( string file in dirs )
				{
					// Create the iFolderFile of type File.
					AddiFolderFileNode( file, parentDirNode );
				}

				// Get the sub-directories in this directory.
				dirs = Directory.GetDirectories( path );
				foreach ( string dir in dirs )
				{
					// Recurse and add files/directories contained in directory.
					AddiFolderNodes( dir, parentDirNode );
				}
			}
			else
			{
				throw new FileNotFoundException();
			}
		}
		#endregion

		#region Internal methods
		/// <summary>
		/// Method: Load
		/// Abstract: iFolders are created only through the manager class.
		/// The FinalConstructor method is called after construction so 
		/// exceptions can be generated back to the manager method 
		/// "CreateiFolder".
		/// </summary>
		internal void Load( Store callingStore, string iFolderID )
		{
			store = callingStore;
			collection = store.GetCollectionByID( iFolderID );

			// Make sure this collection has our store propery
			if ( ( collection == null ) || !collection.IsType( iFolderType ) )
			{
				// Raise an exception here
				throw new ApplicationException( "Invalid iFolder collection." );
			}

			ConnectAddressBook();
		}

		/// <summary>
		/// Method: ConnectAddressBook
		/// Abstract: Internal method to load the correct default
		/// AddressBook for resolving contact information
		/// </summary>
		internal void ConnectAddressBook()
		{
			// TODO: where should we discover the contact information?
			ab = abMan.GetAddressBookByName( store.GetLocalAddressBook().Name );
		}

		/// <summary>
		/// Create an iFolder collection.  This version of create creates 
		/// the iFolder with a name of the leaf node of the path.
		/// </summary>
		/// <param name="path">The path where the iFolder collection will be rooted.</param>
		internal void Create( string path )
		{
			// Make sure the path doesn't end with a separator character.
			if ( path.EndsWith( Path.DirectorySeparatorChar.ToString() ) )
			{
				path = path.Substring( 0, path.Length - 1 );
			}

			Create( Path.GetFileName( path ), path );
		}

		/// <summary>
		/// Create an iFolder collection.  iFolders are created only through
		/// the manager class.  The FinalConstructor method is called after
		/// construction so exceptions can be generated back to the manager
		/// method "CreateiFolder".
		/// </summary>
		/// <param name="name">The friendly name of the collection.</param>
		/// <param name="path">The path where the iFolder collection will be rooted.</param>
		internal void Create( string name, string path )
		{
			// Make sure the path doesn't end with a separator character.
			if ( path.EndsWith( Path.DirectorySeparatorChar.ToString() ) )
			{
				path = path.Substring( 0, path.Length - 1 );
			}

			// Create the collection.
			collection = new Collection( store, name );
			collection.Properties.AddProperty( Property.Types, iFolderType );

			// Create the DirNode object that will represent the root of the iFolder.
			DirNode dirNode = new DirNode( collection, path );

			// Commit the changes.
			Node[] nodeList = { collection, dirNode };
			collection.Commit( nodeList );

			ConnectAddressBook();
		}

		/// <summary>
		/// Deletes an iFolder.
		/// </summary>
		internal void Delete()
		{
			collection.Commit( collection.Delete() );
		}

		/// <summary>
		/// Get a Node object that represents the specified path.
		/// </summary>
		/// <param name="path">Path string to find associated Node object for.</param>
		/// <returns>Node object that represents the specified path string.</returns>
		internal Node GetNodeForPath( string path )
		{
			Uri normalizedPath = new Uri( path );
			Node node = null;

			// Search for all Nodes that contain the specified leaf name.
			ICSList results = collection.Search( BaseSchema.ObjectName, Path.GetFileName( path ), SearchOp.Equal );
			foreach ( ShallowNode sn in results )
			{
				Node tempNode;
				Uri fullPath;

				// Instantiate the right type of Node object.
				switch ( sn.Type )
				{
					case "DirNode":
						tempNode = new DirNode( collection, sn );
						fullPath = new Uri( ( tempNode as DirNode ).GetFullPath( collection ) );
						break;

					case "FileNode":
						tempNode = new FileNode( collection, sn );
						fullPath = new Uri( ( tempNode as FileNode ).GetFullPath( collection ) );
						break;

					default:
						// This isn't the right Node type.
						continue;
				}

				// Normalize each path by using a Uri object to do the compare.
				bool ignoreCase = MyEnvironment.Unix ? false : true;
				if ( !String.Compare( normalizedPath.LocalPath, fullPath.LocalPath, ignoreCase ) )
				{
					// We found our Node object.
					node = tempNode;
					break;
				}
			}

			return node;
		}

		/// <summary>
		/// Maps simias access rights to iFolder access rights.
		/// </summary>
		/// <param name="simiasRights">The simias access right to map.</param>
		/// <returns>The corresponding iFolder access right value.</returns>
		internal static Rights MapAccessRights( Access.Rights simiasRights )
		{
			return ( Rights )Enum.Parse( typeof( Rights ), simiasRights.ToString() );
		}

		/// <summary>
		/// Maps iFolder access rights to simias access rights.
		/// </summary>
		/// <param name="iFolderRights">The iFolder access right to map.</param>
		/// <returns>The corresponding simias access right value.</returns>
		internal static Access.Rights MapAccessRights( Rights iFolderRights )
		{
			return ( Access.Rights )Enum.Parse( typeof( Access.Rights ), iFolderRights.ToString() );
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets the identity of the owner of the iFolder.
		/// </summary>
		public string OwnerIdentity 
		{
			get { return collection.Owner; }
		}

		/// <summary>
		/// Gets/sets the name of the iFolder.
		/// </summary>
		public string Name
		{
			get { return collection.Name; }
			set { collection.Name = value; }
		}

		/// <summary>
		/// Gets the iFolder ID.
		/// </summary>
		public string ID
		{
			get { return collection.ID; }
		}

		/// <summary>
		/// Gets the local path of the iFolder.
		/// </summary>
		public string LocalPath
		{
			get
			{
				DirNode dirNode = collection.GetRootDirectory();
				return ( dirNode != null ) ? dirNode.GetFullPath( collection ) : null;
			}
		}

		/// <summary>
		/// Gets/sets the current node in the iFolder.
		/// </summary>
		public Node CurrentNode
		{
			get { return collection; }
		}

		/// <summary>
		/// Gets/sets the current node in the iFolder.
		/// </summary>
//		internal Collection Collection
//		{
//			get { return collection; }
//		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Updates and adds all files under a given iFolder
		/// </summary>
		public void Update()
		{
			AddiFolderNodes( LocalPath, null );
		}

		/// <summary>
		/// Gets an <see cref="iFolderNode"/> for a file or directory in the
		/// iFolder.
		/// </summary>
		/// <param name="path">
		/// The path and name of a file or directory in the iFolder for which
		/// to return an <see cref="iFolderNode"/>.
		/// </param>
		/// <returns>
		/// An <see cref="iFolderNode"/> for the file or directory specified
		/// by <paramref name="path"/>, or <b>null</b> if it does not exist in the iFolder.
		/// </returns>
		public iFolderNode GetiFolderNodeByPath( string path )
		{
			Node node = GetNodeForPath( path );
			return (node != null) ? new iFolderNode( this, node ) : null;
		}

		/// <summary>
		/// Returns an <see cref="IEnumerator"/> that iterates over all users
		/// that have rights on the iFolder.
		/// </summary>
		/// <returns>
		/// An <see cref="IEnumerator"/> that iterates over all users that have
		/// rights to the iFolder and returns an
		/// <see cref="AccessControlEntry"/> for each user.
		/// </returns>
		[ Obsolete( "This method is marked for removal.  Use GetAccessControlList() instead.", false ) ]
		public ICSList GetShareAccess()
		{
			return collection.GetAccessControlList();
		}

		/// <summary>
		/// Returns an <see cref="IFAccessControlList"/> that iterates over 
		/// all contacts that have rights on the iFolder.
		/// </summary>
		/// <returns>
		/// An <see cref="iFAccessControlList"/> that iterates over all 
		/// contacts that have rights to the iFolder and returns an
		/// <see cref="IFAccessControlEntry"/> for each contact.
		/// </returns>
		public IFAccessControlList GetAccessControlList()
		{
			return ( new IFAccessControlList( collection, ab ) );
		}

		/// <summary>
		/// Returns the access rights that a specified user has on the iFolder.
		/// </summary>
		/// <param name="userID">
		/// The ID of the user for which to return access rights.
		/// </param>
		/// <returns>
		/// The <see cref="Access.Rights"/> for the user specified by 
		/// <paramref name="userID"/>. 
		/// </returns>
		[ Obsolete( "This method is marked for removal.  Use GetRights(Contact contact) instead.", false ) ]
		public Access.Rights GetShareAccess( string userID )
		{
			return collection.GetUserAccess( userID );
		}

		/// <summary>
		/// Returns the rights that a specified user has on the iFolder.
		/// </summary>
		/// <param name="contact">
		/// The <see cref="Contact"/> of the user for which to return rights.
		/// </param>
		/// <returns>
		/// The <see cref="Rights"/> for the user specified by 
		/// <paramref name="contact"/>. 
		/// </returns>
		public Rights GetRights(Contact contact)
		{
			return MapAccessRights( collection.GetUserAccess( contact.ID ) );
		}

		/// <summary>
		/// Determines whether the current user has rights to share the iFolder.
		/// </summary>
		/// <returns>
		/// <b>true</b> if the user can share the iFolder; <b>false</b> if the
		/// user does not have rights to share the iFolder.
		/// </returns>
		public bool IsShareable()
		{
			return collection.Shareable;
		}

		/// <summary>
		/// Shares the iFolder with a specified user.
		/// </summary>
		/// <param name="userID">
		/// The ID of the user with whom to share the iFolder.
		/// </param>
		/// <param name="rights">
		/// The <see cref="Access.Rights"/> to grant the user.
		/// </param>
		/// <param name="invite">
		/// <b>true</b> if an invitation to share the iFolder should be sent;
		/// otherwise, <b>false</b>.
		/// </param>
		[ Obsolete( "This method is marked for removal.  Use SetRights(Contact contact, Rights rights) and Invite(Contact contact) instead.", false ) ]
		public void Share( string userID, Access.Rights rights, bool invite )
		{
			// Set the specified rights on the collection only if the id is not the current owner.
			if ( collection.Owner != userID)
			{
				collection.SetUserAccess( userID, rights );
				collection.Commit();
			}

			if ( invite )
			{
				// inform the notification service that we have shared
				Invitation invitation = InvitationService.CreateInvitation( collection, userID );

				// from
				Contact from = ab.GetContact( collection.Owner );
				invitation.FromName = from.FN;
				invitation.FromEmail = from.EMail;

				// to
				Contact to = ab.GetContact( userID );
				invitation.ToName = to.FN;
				invitation.ToEmail = to.EMail;

				// send the invitation
				InvitationService.Invite( invitation );
			}
		}

		/// <summary>
		/// Sends an invitation to the specified contact using their
		/// preferred method of being contacted
		/// </summary>
		/// <param name="contact">
		/// The <see cref="Contact"/> to send an invitation to.
		/// </param>
		public void Invite( Contact contact )
		{
			// inform the notification service that we have shared
			Invitation invitation = InvitationService.CreateInvitation( collection, contact.ID );

			// from
			Contact from = ab.GetContact( collection.Owner );
			invitation.FromName = from.FN;
			invitation.FromEmail = from.EMail;

			// to
			invitation.ToName = contact.FN;
			invitation.ToEmail = contact.EMail;

			// send the invitation
			InvitationService.Invite( invitation );
		}

		/// <summary>
		/// Generates an iFolder Invitation for the specified contact
		/// and saves it to a file.
		/// </summary>
		/// <param name="contact">
		/// The <see cref="Contact"/> to send an invitation to.
		/// </param>
		/// <param name="filePath">
		/// The full path to the file where the Invitation will be saved
		/// </param>
		public void CreateInvitationFile( Contact contact, string filePath )
		{
			// inform the notification service that we have shared
			Invitation invitation = InvitationService.CreateInvitation( collection, contact.ID );

			// from
			Contact from = ab.GetContact( collection.Owner );
			invitation.FromName = from.FN;
			invitation.FromEmail = from.EMail;

			// to
			invitation.ToName = contact.FN;
			invitation.ToEmail = contact.EMail;

			invitation.Save( filePath );
		}

		/// <summary>
		/// Sets the rights on the iFolder for the specified user.
		/// </summary>
		/// <param name="contact">
		/// The <see cref="Contact"/> for the user for which to set rights.
		/// </param>
		/// <param name="rights">
		/// The <see cref="Rights"/> to set.
		/// </param>
		public void SetRights( Contact contact, Rights rights )
		{
			if ( collection.Owner != contact.ID )
			{
				collection.SetUserAccess( contact.ID, MapAccessRights( rights ) );
				collection.Commit();
			}
		}

		/// <summary>
		/// Sets the access rights on the iFolder for the specified user.
		/// </summary>
		/// <param name="userID">
		/// The ID of the user for which to set access rights.
		/// </param>
		/// <param name="rights">
		/// The <see cref="Access.Rights"/> to set.
		/// </param>
		[ Obsolete( "This method is marked for removal.  Use SetRights(Contact contact, Rights rights) instead.", false ) ]
		public void SetShareAccess( string userID, Access.Rights rights )
		{
			if ( collection.Owner != userID )
			{
				collection.SetUserAccess( userID, rights );
				collection.Commit();
			}
		}

		/// <summary>
		/// Removes all access rights on the iFolder for the specified user.
		/// </summary>
		/// <param name="userId">
		/// The <see cref="Contact"/> for which to remove all rights.
		/// </param>
		public void RemoveRights( Contact contact )
		{
			if ( collection.Owner != contact.ID )
			{
				collection.RemoveUserAccess( contact.ID );
				collection.Commit();
			}
		}

		/// <summary>
		/// Removes all access rights on the iFolder for the specified contact.
		/// </summary>
		/// <param name="userID">
		/// The ID of the user for which to remove access rights.
		/// </param>
		[ Obsolete( "This method is marked for removal.  Use RemoveRights(Contact contact) instead.", false ) ]
		public void RemoveUserAccess( string userID )
		{
			collection.RemoveUserAccess( userID );
			collection.Commit();
		}
		#endregion
	}
}
