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
	public class iFolder : Collection
	{
		#region Class Members
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
		/// <summary>
		/// Creates an iFolder collection.
		/// </summary>
		/// <param name="store">The store object that this iFolder belongs to.</param>
		/// <param name="name">The friendly name that is used by applications to describe the iFolder.</param>
		/// <param name="path">The full path of the iFolder.</param>
		/// <param name="manager">The address book manager.</param>
		internal iFolder(Store store, string name, string path, Novell.AddressBook.Manager manager) :
			base(store, name)
		{
			this.abMan = manager;

			// Make sure the path doesn't end with a separator character.
			if ( path.EndsWith( Path.DirectorySeparatorChar.ToString() ) )
			{
				path = path.Substring( 0, path.Length - 1 );
			}

			// Set the type of the collection.
			this.SetType(this, iFolderType);

			// Create the DirNode object that will represent the root of the iFolder.
			DirNode dirNode = new DirNode( this, path );

			// Commit the changes.
			Node[] nodeList = { this, dirNode };
			this.Commit( nodeList );

			ConnectAddressBook();
		}

		/// <summary>
		/// Constructor for creating an iFolder object from an existing node.
		/// </summary>
		/// <param name="store">The store object that this iFolder belongs to.</param>
		/// <param name="node">The node object to construct the iFolder object from.</param>
		/// <param name="manager">The address book manager.</param>
		internal iFolder(Store store, Node node, Novell.AddressBook.Manager manager)
			: base(store, node)
		{
			// Make sure this collection has our store propery
			if (!this.IsType( this, iFolderType ) )
			{
				// Raise an exception here
				throw new ApplicationException( "Invalid iFolder collection." );
			}

			this.abMan = manager;

			ConnectAddressBook();
		}
        #endregion

		#region Private methods
		/// <summary>
		/// Add a single directory to an iFolder.
		/// </summary>
		/// <param name="dirPath">The path to the directory to add.</param>
		/// <param name="dirNode">Parent DirNode.</param>
		private DirNode AddiFolderDirNode( string dirPath, DirNode dirNode )
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
				DirNode childDirNode = new DirNode( this, dirNode, Path.GetFileName( dirPath ) );
				this.Commit( childDirNode );
				ifNode = new iFolderNode( this, childDirNode );
			}

			return ifNode.Node as DirNode;
		}

		/// <summary>
		/// Add a single file to an iFolder.
		/// </summary>
		/// <param name="filePath">The path to the file to add.</param>
		/// <param name="dirNode">Parent DirNode.</param>
		private void AddiFolderFileNode( string filePath, DirNode dirNode )
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
				FileNode fileNode = new FileNode( this, dirNode, Path.GetFileName( filePath ) );
				this.Commit( fileNode );
				ifNode = new iFolderNode( this, fileNode );
			}
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
		/// Method: ConnectAddressBook
		/// Abstract: Internal method to load the correct default
		/// AddressBook for resolving contact information
		/// </summary>
		internal void ConnectAddressBook()
		{
			// TODO: where should we discover the contact information?
			ab = abMan.GetAddressBookByName( this.StoreReference.GetLocalAddressBook().Name );
		}

		/// <summary>
		/// Deletes an iFolder.
		/// </summary>
		new internal void Delete()
		{
			this.Commit( ((Collection)this).Delete() );
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
			ICSList results = this.Search( BaseSchema.ObjectName, Path.GetFileName( path ), SearchOp.Equal );
			foreach ( ShallowNode sn in results )
			{
				Node tempNode;
				Uri fullPath;

				// Instantiate the right type of Node object.
				switch ( sn.Type )
				{
					case "DirNode":
						tempNode = new DirNode( this, sn );
						fullPath = new Uri( ( tempNode as DirNode ).GetFullPath( this ) );
						break;

					case "FileNode":
						tempNode = new FileNode( this, sn );
						fullPath = new Uri( ( tempNode as FileNode ).GetFullPath( this ) );
						break;

					default:
						// This isn't the right Node type.
						continue;
				}

				// Normalize each path by using a Uri object to do the compare.
				bool ignoreCase = MyEnvironment.Unix ? false : true;
				if ( String.Compare( normalizedPath.LocalPath, fullPath.LocalPath, ignoreCase ) == 0 )
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
		[ Obsolete( "This property is marked for removal.  Use Owner instead.", false ) ]
		public string OwnerIdentity 
		{
			get { return this.Owner; }
		}

		/// <summary>
		/// Gets the local path of the iFolder.
		/// </summary>
		public string LocalPath
		{
			get
			{
				DirNode dirNode = this.GetRootDirectory();
				return ( dirNode != null ) ? dirNode.GetFullPath( this ) : null;
			}
		}

		/// <summary>
		/// Gets the collection node for this iFolder.
		/// </summary>
		[ Obsolete( "This property is marked for removal.  This object is the collection.", false ) ]
		public Node CurrentNode
		{
			get { return this; }
		}

		/// <summary>
		/// Gets/sets the refresh interval for the iFolder.
		/// </summary>
		public int RefreshInterval
		{
			get
			{
				return new SyncCollection(this).Interval;
			}

			set
			{
				new SyncCollection(this).Interval = value;
			}
		}
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
		/// Returns an <see cref="IFAccessControlList"/> that iterates over 
		/// all contacts that have rights on the iFolder.
		/// </summary>
		/// <returns>
		/// An <see cref="iFAccessControlList"/> that iterates over all 
		/// contacts that have rights to the iFolder and returns an
		/// <see cref="IFAccessControlEntry"/> for each contact.
		/// </returns>
		new public IFAccessControlList GetAccessControlList()
		{
			return ( new IFAccessControlList( this, ab ) );
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
			return MapAccessRights( this.GetUserAccess( contact.ID ) );
		}

		/// <summary>
		/// Determines whether the current user has rights to share the iFolder.
		/// </summary>
		/// <returns>
		/// <b>true</b> if the user can share the iFolder; <b>false</b> if the
		/// user does not have rights to share the iFolder.
		/// </returns>
		[ Obsolete( "This method is marked for removal.  Use the property Shareable instead.", false ) ]
		public bool IsShareable()
		{
			return this.Shareable;
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
			Invitation invitation = InvitationService.CreateInvitation( this, contact.ID );

			// from
			Contact from = ab.GetContact( this.Owner );
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
			Invitation invitation = InvitationService.CreateInvitation( this, contact.ID );

			// from
			Contact from = ab.GetContact( this.Owner );
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
			if ( this.Owner != contact.ID )
			{
				this.SetUserAccess( contact.ID, MapAccessRights( rights ) );
				this.Commit();
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
			if ( this.Owner != contact.ID )
			{
				this.RemoveUserAccess( contact.ID );
				this.Commit();
			}
		}
		#endregion
	}
}
