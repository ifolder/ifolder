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
using System.Xml;
using System.Text.RegularExpressions;

using Simias;
using Simias.Storage;
using Simias.Sync;

namespace Novell.iFolder
{
	/// <summary>
	/// Provides methods to manipulate iFolders.
	/// </summary>
	public class iFolderManager : IEnumerable
	{
		#region Properties
		/// <summary>
		/// Gets/sets the default refresh interval for iFolders.
		/// </summary>
		public int DefaultRefreshInterval
		{
			get
			{
				return new SyncProperties(config).Interval;
			}

			set
			{
				new SyncProperties(config).Interval = value;
			}
		}
        #endregion

		#region Class Members
		
        internal Store						store;
        private Configuration config;

		/// <summary>
		/// The section in the configuration file dealing with iFolder specific information.
		/// </summary>
		public static readonly string CFG_Section = "iFolder";

		/// <summary>
		/// The section that lists paths that are not allowed to become an iFolder.
		/// </summary>
		public static readonly string CFG_ExcludedPaths = "Excluded Paths";

		/// <summary>
		/// The tag used for an individual excluded path entry.
		/// </summary>
		public static readonly string XmlPathTag = "Path";

		/// <summary>
		/// The attribute holding the name of the path or environment variable.
		/// </summary>
		public static readonly string XmlNameAttr = "name";

		/// <summary>
		/// The attribute holding a value indicating if subdirectories of the path are also not
		/// allowed to become iFolders.
		/// </summary>
		public static readonly string XmlDeepAttr = "deep";

		/// <summary>
		/// The attribute holding a value indicating if this element represents an environment 
		/// variable.
		/// </summary>
		public static readonly string XmlEnvironmentAttr = "environment";

		public static readonly string CFG_AllowedFSTypes = "Allowed fs types";
		public static readonly string XmlTypeTag = "Type";

		private static ExcludeDirectory[] linuxExcludeList = {
			new ExcludeDirectory("/bin", false, true),
			new ExcludeDirectory("/sbin", false, true),
			new ExcludeDirectory("/opt", false, true),
			new ExcludeDirectory("/etc", false, true),
			new ExcludeDirectory("/lib", false, true),
			new ExcludeDirectory("/sys", false, true)
		};

		private static ExcludeDirectory[] windowsExcludeList = {
			new ExcludeDirectory("SystemDrive", true, false),
			new ExcludeDirectory("windir", true, true)
		};

		private static string[] linuxIncludeTypes = new string[] {
			"reiserfs",
			"ext",
			"ext2",
			"ext3",
			"ntfs",
			"msdos"
		};

        #endregion

		#region Constructors
		internal iFolderManager( Configuration config )
		{
			this.config = config;
            this.store = Store.GetStore();
		}
		#endregion

		#region Static Methods
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
			return new iFolderManager(Configuration.GetConfiguration());
		}
		
		/// <summary>
		/// Creates a section in the configuration file that lists directories to exclude 
		/// from becoming iFolders.
		/// </summary>
		/// <param name="config">The configuration file to modify.</param>
		public static void CreateDefaultExclusions(Configuration config)
		{
			// Get the XML element from the config file.
			XmlElement pathsElement = config.GetElement(CFG_Section, CFG_ExcludedPaths);

			// Only set the defaults if the section is empty.
			if (pathsElement.IsEmpty)
			{
				ExcludeDirectory[] excludeList;

				// Use the exclude list for the correct OS.
				if (MyEnvironment.Unix)
				{
					excludeList = linuxExcludeList;
				}
				else
				{
					excludeList = windowsExcludeList;
				}

				// Walk the exclude list and add entries to the section.
				foreach (ExcludeDirectory exDir in excludeList)
				{
					XmlElement xmlElement = pathsElement.OwnerDocument.CreateElement(XmlPathTag);
					xmlElement.SetAttribute(XmlNameAttr, exDir.Name);
					xmlElement.SetAttribute(XmlEnvironmentAttr, exDir.Environment.ToString());
					xmlElement.SetAttribute(XmlDeepAttr, exDir.Deep.ToString());
					pathsElement.AppendChild(xmlElement);
					config.SetElement(CFG_Section, CFG_ExcludedPaths, pathsElement);
				}
			}

			if (MyEnvironment.Unix)
			{
				// Add the allowed filesystem types for Linux.
				XmlElement typesElement = config.GetElement(CFG_Section, CFG_AllowedFSTypes);

				if (typesElement.IsEmpty)
				{
					foreach (string s in linuxIncludeTypes)
					{
						XmlElement xmlElement = typesElement.OwnerDocument.CreateElement(XmlTypeTag);
						xmlElement.SetAttribute(XmlNameAttr, s);
						typesElement.AppendChild(xmlElement);
						config.SetElement(CFG_Section, CFG_AllowedFSTypes, typesElement);
					}
				}
			}
		}
		#endregion

		#region Public Methods
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

				iFolder newiFolder = new iFolder(store, name, path);
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
				ifolder = new iFolder(store, collection);
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

			ICSList collectionList = store.GetCollectionsByName(Path.GetFileName(path));
			foreach (ShallowNode sn in collectionList)
			{
				iFolder tempif = GetiFolderById(sn.ID);
				if (tempif != null)
				{
					Uri ifolderPath = new Uri( tempif.LocalPath );

					if ( String.Compare( normalizedPath.LocalPath, ifolderPath.LocalPath, !MyEnvironment.Unix ) == 0 )
					{
						ifolder = tempif;
						break;
					}
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

			// TODO: Call to Policy engine to check for directory/drive exclusions ... 
			// the Policy overrides the config file settings.
			if (folderAllowedByType(nPath) && !folderExcluded(nPath))
			{
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
			}
			else
			{
				isiFolder = false;
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

/*		/// <summary>
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
		}*/
		#endregion
		
		#region Private Methods
		private bool folderExcluded( Uri nPath)
		{
			bool folderExcluded = false;

			XmlElement pathsElement = config.GetElement(CFG_Section, CFG_ExcludedPaths);
			XmlNodeList pathNodes = pathsElement.SelectNodes(XmlPathTag);
			foreach (XmlElement pathNode in pathNodes)
			{
				try
				{
					string path = pathNode.GetAttribute(XmlNameAttr);
					string s = pathNode.GetAttribute(XmlDeepAttr);
					bool deep = s != String.Empty ? bool.Parse(s) : false;

					s = pathNode.GetAttribute(XmlEnvironmentAttr);
					bool environment = s != String.Empty ? bool.Parse(s) : false;

					if (environment)
					{
						path = Environment.GetEnvironmentVariable(path);
					}

					Uri excludePath = new Uri(path.EndsWith(Path.DirectorySeparatorChar.ToString()) ?
										path :
										path + Path.DirectorySeparatorChar.ToString());

					string nPath2 = deep ? nPath.LocalPath.Substring(0, excludePath.LocalPath.Length) : nPath.LocalPath;
					if (String.Compare(nPath2, excludePath.LocalPath, !MyEnvironment.Unix) == 0)
					{
						folderExcluded = true;
						break;
					}
				}
				catch 
				{
				}
			}

			return folderExcluded;
		}

		private bool folderAllowedByType(Uri nPath)
		{
			bool folderAllowed = true;
/*
			// This is making calls which are very Linux
			// specific and is not checking for Linux
			// Checking for Unix is not good enough

			bool folderAllowed = false;
			
			if (MyEnvironment.Unix)
			{
				string type = getFSType(nPath);

				XmlElement typesElement = config.GetElement(CFG_Section, CFG_AllowedFSTypes);
				XmlNodeList typeNodes = typesElement.SelectNodes(XmlTypeTag);
				foreach (XmlElement typeNode in typeNodes)
				{
					try
					{
						string allowedType = typeNode.GetAttribute(XmlNameAttr);

						if (allowedType.Equals(type))
						{
							folderAllowed = true;
							break;
						}
					}
					catch 
					{
					}
				}
			}
			else
			{
				folderAllowed = true;
			}
*/

			return folderAllowed;
		}

		private string getFSType(Uri path)
		{
			string type = null;

			Hashtable ht = new Hashtable();
			StreamReader sr = null;
			try
			{
				sr = new StreamReader("/etc/mtab");
				string text;
				Regex ex = new Regex("[ ]+");
				while ((text = sr.ReadLine()) != null)
				{
					// Split the line into substrings.
					string[] s = ex.Split(text);

					// Put the values in a hashtable.
					ht.Add(s[1], s[2]);
				}				
			}
			catch (Exception e)
			{
			}

			if (sr != null)
			{
				sr.Close();
			}

			string parent = path.LocalPath;
			while (true)
			{
				if (ht.ContainsKey(parent))
				{
					break;
				}

				parent = Path.GetDirectoryName(parent);
			}

			type = (string)ht[parent];

			return type;
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
