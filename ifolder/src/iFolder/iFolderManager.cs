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

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Invite;
using Novell.AddressBook;

namespace Novell.iFolder
{
	/// <summary>
	/// Provides methods to manipulate iFolders.
	/// </summary>
	public class iFolderManager : IEnumerable
	{
		#region Class Members
		
        internal Store						store;
		internal Novell.AddressBook.Manager	abMan;
        private Configuration config;

        #endregion

		#region Constructors
		internal iFolderManager( Configuration config )
		{
			this.config = config;
            this.store = new Store( config );
			this.abMan = Novell.AddressBook.Manager.Connect( config );
		}
		#endregion

		#region Static Methods
		/// <summary>
		/// Authenticates the current user to a persistent store at a specified
		/// location and returns an <see cref="iFolderManager"/> that can be 
		/// used
		/// to manipulate iFolders in that store.
		/// </summary>
		///	<param name="location">
		///	<see cref="Uri"/> specifying the location of the persistent store 
		/// server.
		///	</param>
		///	<returns>
		///	An <see cref="iFolderManager"/> that can be used
		/// to manipulate iFolders in the specified store.
		///	</returns>
		public static iFolderManager Connect( Uri location )
		{
			//
			// TODO: Hook up with Copernicus here!
			//

			Configuration config = ( location == null ) ? new Configuration() : new Configuration( location.LocalPath );
			return new iFolderManager( config );
		}

		/// <summary>
		/// Authenticates the current user to the default local store and
		/// returns an <see cref="iFolderManager"/> that can be used
		/// to manipulate iFolders in that store.
		/// </summary>
		///	<returns>
		///	An <see cref="iFolderManager"/> that can be used
		/// to manipulate iFolders in the default local store.
		///	</returns>
		public static iFolderManager Connect()
		{
			//
			// TODO: Hook up with Copernicus here!
			//
			
			return Connect( null );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the current iFolder in the store.
		/// </summary>
		/// <returns>
		/// An <see cref="iFolder"/> for the current iFolder.
		/// </returns>
		/// <remarks>
		/// This property returns the current element in the enumerator.
		/// </remarks>
		public Novell.AddressBook.Manager AddressBookManager
		{
			get 
			{ 
				return abMan; 
			}
		}

		/// <summary>
		/// Creates an iFolder located at a specified directory.
		/// </summary>
		/// <param name="path">
		/// The absolute path where the iFolder will be located.
		/// </param>
		/// <returns>
		/// An <see cref="iFolder"/> for <paramref name="path"/>.
		/// </returns>
		/// <exception cref="ApplicationException">
		/// The path is a parent or child of an existing iFolder.
		/// </exception>
		/// <exception cref="ApplicationException">
		/// Failed to create the iFolder.
		/// </exception>
		public iFolder CreateiFolder( string path )
		{
			try
			{
				if ( !CanBeiFolder( path ) )
				{
					throw new ApplicationException("The path specified is either a parent or a child of an existing iFolder");
				}

				string name = Path.GetFileName(path);

				iFolder newiFolder = new iFolder(store, name, path, abMan);
				return newiFolder;
			}
			catch( Exception e )
			{
				throw new ApplicationException("iFolder not created for " +  path + " - Reason: " + e.ToString());
			}
		}

		/// <summary>
		/// Deletes an iFolder by its ID.
		/// </summary>
		/// <param name="id">
		/// The ID of the iFolder.
		/// The iFolder ID can be queried from the <see cref="iFolder"/>.
		/// </param>
		public void DeleteiFolderById( string id )
		{
			iFolder ifolder = GetiFolderById( id );
			if ( ifolder != null )
			{
				ifolder.Delete();
			}
		}

		/// <summary>
		/// Deletes an iFolder by its local path.
		/// </summary>
		/// <param name="path">
		/// The rooted local path of the iFolder.
		/// </param>
		public void DeleteiFolderByPath( string path )
		{
			iFolder ifolder = GetiFolderByPath( path );
			if ( ifolder != null )
			{
				ifolder.Delete();
			}
		}

		/// <summary>
		/// Checks whether a given directory is an iFolder.
		/// </summary>
		/// <param name="path">
		/// An absolute path to check.
		/// </param>
		/// <returns>
		/// <b>true</b> if <paramref name="path"/> is an iFolder;
		/// otherwise, <b>false</b>.
		/// </returns>
		public bool IsiFolder( string path )
		{
			return ( GetiFolderByPath( path ) != null ) ? true : false;
		}

		/// <summary>
		/// Get an <see cref="iFolder"/> by ID.
		/// </summary>
		/// <param name="id">
		/// The ID of the iFolder.
		/// </param>
		/// <returns>
		/// An <see cref="iFolder"/> for <paramref name="id"/>.
		/// </returns>
		public iFolder GetiFolderById( string id )
		{
			iFolder ifolder;

			try
			{
				Collection collection = store.GetCollectionByID(id);
				ifolder = new iFolder(store, collection, abMan);
			}
			catch
			{
				ifolder = null;
			}

			return ifolder;
		}

		/// <summary>
		/// Get an <see cref="iFolder"/> by its local path.
		/// </summary>
		/// <param name="path">
		/// The rooted local path of the iFolder.
		/// </param>
		/// <returns>
		/// An <see cref="iFolder"/> for <paramref name="path"/>.
		/// </returns>
		public iFolder GetiFolderByPath( string path )
		{
			Uri normalizedPath = new Uri( path );
			iFolder ifolder = null;

			foreach ( iFolder tempif in this )
			{
				Uri ifolderPath = new Uri( tempif.LocalPath );

				bool ignoreCase = MyEnvironment.Unix ? false : true;
				if ( String.Compare( normalizedPath.LocalPath, ifolderPath.LocalPath, ignoreCase ) == 0 )
				{
					ifolder = tempif;
					break;
				}
			}

			return ifolder;
		}

		/// <summary>
		/// Dredges the file system starting at the root of the iFolder and
		/// adds missing directories and files to the iFolder.
		/// </summary>
		/// <param name="id">
		/// The ID of the iFolder to update.
		/// </param>
		public void UpdateiFolder( string id )
		{
			iFolder ifolder = GetiFolderById( id );
			if ( ifolder != null )
			{
				ifolder.Update();
			}
		}

		/// <summary>
		/// Checks whether it is valid to make a given directory an iFolder.
		/// </summary>
		/// <param name="path">
		/// An absolute path to check.
		/// </param>
		/// <returns>
		/// <b>true</b> if <paramref name="path"/> can be an iFolder;
		/// otherwise, <b>false</b>.
		/// </returns>
		/// <remarks>
		/// Nested iFolders are not permitted; <paramref name="path"/> is
		/// checked to see if it is within, or contains, an existing iFolder.
		/// </remarks>
		public bool CanBeiFolder( string path )
		{
			bool isiFolder = true;

			// Make sure the paths end with a separator.
			// Create a normalized path that can be compared on any platform.
			Uri nPath = new Uri( path.EndsWith(Path.DirectorySeparatorChar.ToString()) ?
								path :
								path + Path.DirectorySeparatorChar.ToString());

			foreach ( iFolder ifolder in this )
			{
				Uri iPath = new Uri( ifolder.LocalPath.EndsWith(Path.DirectorySeparatorChar.ToString()) ?
									ifolder.LocalPath :
									ifolder.LocalPath + Path.DirectorySeparatorChar.ToString());

				// Check if the specified path is subordinate to or a parent of the iFolder root path.
				if ( nPath.LocalPath.StartsWith( iPath.LocalPath ) || iPath.LocalPath.StartsWith( nPath.LocalPath ) )
				{
					isiFolder = false;
					break;
				}
			}

			return isiFolder;
		}

		/// <summary>
		/// Checks whether a given path is within an existing iFolder.
		/// </summary>
		/// <param name="path">
		/// An absolute path to check.
		/// </param>
		/// <returns>
		/// <b>true</b> if <paramref name="path"/> is in an existing iFolder;
		/// otherwise, <b>false</b>.
		/// </returns>
		public bool IsPathIniFolder( string path )
		{
			bool iniFolder = false;

			// Create a normalized path that can be compared on any platform.
			Uri nPath = new Uri( path );

			foreach ( iFolder ifolder in this )
			{
				Uri iPath = new Uri( ifolder.LocalPath );

				// Check if the specified path is subordinate to or a parent of the iFolder root path.
				if ( nPath.LocalPath.StartsWith( iPath.LocalPath ) )
				{
					iniFolder = true;
					break;
				}
			}

			return iniFolder;
		}

		/// <summary>
		/// Accepts an invitation to share an iFolder.
		/// </summary>
		/// <param name="invitation">
		/// The <see cref="Invitation"/> to be accepted.
		/// </param>
		/// <param name="path">
		/// The local path where the new iFolder is to be placed.
		/// </param>
		/// <exception cref="ApplicationException">
		/// <paramref name="path"/> is within an existing iFolder."
		/// </exception>
		public void AcceptInvitation( Invitation invitation, string path )
		{
			// Check the path to see if it is inside an existing iFolder
			if( IsPathIniFolder( path ) )
			{
				throw new ApplicationException("The path specified is " + "located within an existing iFolder" );
			}

			invitation.RootPath = path;
			InvitationService.Accept( store, invitation );
		}
		#endregion

		#region IEnumerable
		/// <summary>
		/// Returns an <see cref="IEnumerator"/> that iterates over all
		/// iFolders in the store.
		/// </summary>
		/// <returns>
		/// An <see cref="IEnumerator"/> that iterates over all iFolders
		/// in the store.
		/// </returns>
		public IEnumerator GetEnumerator()
		{
			return new iFolderEnumerator( store, this );
		}

		/// <summary>
		/// Enumerator class for the Store object that allows enumeration of the Collection objects
		/// within the Store.
		/// </summary>
		private class iFolderEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// Indicates whether the object has been disposed.
			/// </summary>
			private bool disposed = false;

			/// <summary>
			///  Collection Store object associated with this iFolder.
			/// </summary>
			private Store store;

			/// <summary>
			/// Object used to enumerate the iFolder collections.
			/// </summary>
			private ICSEnumerator enumerator;

			/// <summary>
			/// Reference to manager.
			/// </summary>
			private iFolderManager ifMan;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor for the StoreEnumerator class.
			/// </summary>
			/// <param name="storeObject">Store object where to enumerate the collections.</param>
			/// <param name="ifManager">Reference to an iFolderManager object.</param>
			public iFolderEnumerator( Store storeObject, iFolderManager ifManager )
			{
				// Get all of the collections that are of iFolder type and save the enumerator to the results.
				store = storeObject;
				ifMan = ifManager;
				enumerator = store.GetCollectionsByType( iFolder.iFolderType ).GetEnumerator() as ICSEnumerator;
			}
			#endregion

			#region IEnumerator Members
			/// <summary>
			/// Advances the enumerator to the next iFolder in the store.
			/// </summary>
			/// <returns>
			/// <b>true</b> if the enumerator was successfully advanced to the
			/// next iFolder; <b>false</b> if the enumerator has passed the end
			/// of the store.
			/// </returns>
			public bool MoveNext()
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return enumerator.MoveNext();
			}

			/// <summary>
			/// Gets the current iFolder in the store.
			/// </summary>
			/// <returns>
			/// An <see cref="iFolder"/> for the current iFolder.
			/// </returns>
			/// <remarks>
			/// This property returns the current element in the enumerator.
			/// </remarks>
			public object Current
			{
				get	
				{ 
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					return ifMan.GetiFolderById( ( enumerator.Current as ShallowNode ).ID ); 
				}
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the
			/// first iFolder in the store.
			/// </summary>
			public void Reset()
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				enumerator.Reset();
			}
			#endregion

			#region IDisposable Members
			/// <summary>
			/// Allows for quick release of managed and unmanaged resources.
			/// Called by applications.
			/// </summary>
			public void Dispose()
			{
				Dispose( true );
				GC.SuppressFinalize( this );
			}

			/// <summary>
			/// Dispose( bool disposing ) executes in two distinct scenarios.
			/// If disposing equals true, the method has been called directly
			/// or indirectly by a user's code. Managed and unmanaged resources
			/// can be disposed.
			/// If disposing equals false, the method has been called by the 
			/// runtime from inside the finalizer and you should not reference 
			/// other objects. Only unmanaged resources can be disposed.
			/// </summary>
			/// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
			private void Dispose( bool disposing )
			{
				// Check to see if Dispose has already been called.
				if ( !disposed )
				{
					// Protect callers from accessing the freed members.
					disposed = true;

					// If disposing equals true, dispose all managed and unmanaged resources.
					if ( disposing )
					{
						// Dispose managed resources.
						enumerator.Dispose();
					}
				}
			}
		
			/// <summary>
			/// Use C# destructor syntax for finalization code.
			/// This destructor will run only if the Dispose method does not get called.
			/// It gives your base class the opportunity to finalize.
			/// Do not provide destructors in types derived from this class.
			/// </summary>
			~iFolderEnumerator()      
			{
				Dispose( false );
			}
			#endregion
		}
		#endregion
	}
}
