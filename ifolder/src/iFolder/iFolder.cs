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
using Simias.POBox;
using Simias.Storage;
using Simias.Sync;

namespace Novell.iFolder
{
	/// <summary>
	/// Provides methods for manipulating the properties and contents 
	/// of an iFolder.
	/// </summary>
	public class iFolder : Collection
	{
		#region Class Members
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
		#endregion

		#region Constructors
		/// <summary>
		/// Creates an iFolder collection.
		/// </summary>
		/// <param name="store">The store object that this iFolder belongs to.</param>
		/// <param name="name">The friendly name that is used by applications to describe the iFolder.</param>
		/// <param name="path">The full path of the iFolder.</param>
		internal iFolder(Store store, string name, string path) :
			this(store, name, path, store.DefaultDomain)
		{
		}


		/// <summary>
		/// Creates an iFolder collection.
		/// </summary>
		/// <param name="store">The store object that this iFolder belongs to.</param>
		/// <param name="name">The friendly name that is used by applications to describe the iFolder.</param>
		/// <param name="path">The full path of the iFolder.</param>
		/// <param name="domainName">The domain for this iFolder.</param>
		internal iFolder(Store store, string name, string path, string domainName) :
			base(store, name, domainName)
		{
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

			// Create an invitation in the POBox of the current user for this iFolder.
			CreatePersonalSubscription( store, domainName, dirNode );
		}

		/// <summary>
		/// Constructor for creating an iFolder object from an existing node.
		/// </summary>
		/// <param name="store">The store object that this iFolder belongs to.</param>
		/// <param name="node">The node object to construct the iFolder object from.</param>
		internal iFolder(Store store, Node node)
			: base(store, node)
		{
			// Make sure this collection has our store propery
			if (!this.IsType( this, iFolderType ) )
			{
				// Raise an exception here
				throw new ApplicationException( "Invalid iFolder collection." );
			}
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

		/// <summary>
		/// Creates a personal subscription for an iFolder that can be used by this user on another
		/// machine to sync down this iFolder.
		/// </summary>
		/// <param name="store">Store where iFolder was created.</param>
		/// <param name="domain">Domain that the iFolder belongs to.</param>
		/// <param name="dirNode">The root DirNode object that belongs to the collection.</param>
		private void CreatePersonalSubscription( Store store, string domain, DirNode dirNode )
		{
			// Get the current member for this iFolder.
			Member member = GetCurrentMember();

			// Get or create a POBox for the user.
			POBox poBox = POBox.GetPOBox( store, domain, member.UserID );

			// Create a subscription for this iFolder in the POBox.
			Subscription subscription = poBox.CreateSubscription( this, member, typeof( iFolder ).Name );

			// Set the 'To:' field in the subscription the the current user, so that this subscription cannot
			// be used by any other person.
			subscription.ToName = member.Name;
			subscription.ToIdentity = member.UserID;
			subscription.ToPublicKey = member.PublicKey;
			subscription.SubscriptionRights = member.Rights;
			subscription.SubscriptionState = SubscriptionStates.Ready;

			// TODO: This may not be right in the future.
			// Get the master URL from the Roster for this domain.
			Roster roster = store.GetDomain( domain ).GetRoster( store );
			SyncCollection sc = new SyncCollection( roster );
			subscription.SubscriptionCollectionURL = sc.MasterUrl.ToString();

			// Add the DirNode information.
			subscription.DirNodeID = dirNode.ID;
			subscription.DirNodeName = dirNode.Name;
			
			// Commit the subscription to the POBox.
			poBox.Commit( subscription );
		}
		#endregion

		#region Internal methods
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
		#endregion

		#region Properties
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
		/// The path and name of a file or directory in the iFolder for which to return an 
		/// <see cref="iFolderNode"/>.</param>
		/// <returns>
		/// An <see cref="iFolderNode"/> for the file or directory specified
		/// by <paramref name="path"/>, or <b>null</b> if it does not exist in the iFolder.
		/// </returns>
		public iFolderNode GetiFolderNodeByPath( string path )
		{
			Node node = GetNodeForPath( path );
			return (node != null) ? new iFolderNode( this, node ) : null;
		}
		#endregion
	}
}
