/***********************************************************************
 *  $RCSfile$
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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using Simias.Storage;
using Simias.Agent;
using Simias;
using Novell.AddressBook;

namespace Novell.iFolder
{
	/// <summary>
	/// Provides methods to manipulate iFolders.
	/// </summary>
	public class iFolderManager : IEnumerable, IEnumerator
	{
#region Class Members
		/// <summary>
		/// The user that opened the iFolderManager.
		/// </summary>
		//private string		storeUserName = null;

		/// <summary>
		/// Path to the store that this instance represents.
		/// </summary>
		//private string	storePath = null;

		internal Store						store = null;
		internal Novell.AddressBook.Manager	abMan = null;
		internal Configuration				config = null;

		//private	Collection	iFolderCollection = null;
		private	IEnumerator	storeEnum = null;
#endregion




#region Constructors
		internal iFolderManager(Configuration config, Store store, 
				Novell.AddressBook.Manager manager)
		{
			this.config = config;
			this.store = store;
			this.abMan = manager;
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
		public static iFolderManager Connect(Uri location)
		{
			//
			// TODO: Hook up with Copernicus here!
			//

			iFolderManager	manager = null;
			Store	store;

			Configuration config = new Configuration(location.AbsolutePath);
			store = Store.Connect(config);
			if(store != null)
			{
				Novell.AddressBook.Manager abMan = 
						Novell.AddressBook.Manager.Connect(location);

				manager = new iFolderManager(config, store, abMan);
				if(manager != null)
				{
					//	iFolderManager.storeUserName = userName;
				}
			}

			return(manager);
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

			iFolderManager	manager = null;
			Store	store;
			//			string	userName = "test";

			Configuration config = new Configuration();
			store = Store.Connect(config);
			if(store != null)
			{
				Novell.AddressBook.Manager abMan = 
					Novell.AddressBook.Manager.Connect(store.StorePath);

				manager = new iFolderManager(config, store, abMan);
				if(manager != null)
				{
					//	iFolderManager.storeUserName = userName;
				}
			}

			return(manager);
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
		public iFolder CreateiFolder(string path)
		{
			try
			{
				if(!CanBeiFolder(path))
					throw new ApplicationException("The path specified is either a parent or a child of an existing iFolder");

				iFolder newiFolder = new iFolder(store, abMan);
				newiFolder.Create(path);
				return (newiFolder);
			}
			catch(Exception e)
			{
				throw new ApplicationException("iFolder not created for " + 
						path + " - Reason: " + e.ToString());
			}
		}




		/// <summary>
		/// OBSOLETE: Create an iFolder with a specific friendly name.
		/// </summary>
		/// <param name="name">
		/// The friendly name of the iFolder.
		/// </param>
		/// <param name="path">
		/// The absolute path where the iFolder will be located.
		/// </param>
		/// <returns>
		/// An <see cref="iFolder"/> for <paramref name="path"/>.
		/// </returns>
		/// <remarks>
		/// This overload of CreateiFolder is obsolete. Please do not use it.
		/// </remarks>
		[ Obsolete( "This overloaded method is marked for removal. There is no replacement.", false ) ]
			public iFolder CreateiFolder(string name, string path)
			{
				try
				{
					iFolder newiFolder = new iFolder(store, abMan);
					newiFolder.Create(name, path);
					return(newiFolder);
				}
				catch(Exception e)
				{
					throw new ApplicationException("iFolder " + name +
							"not created - Reason: " + e.ToString());
				}
			}




		/// <summary>
		/// OBSOLETE: Deletes an iFolder by rooted path name or by ID.
		/// </summary>
		/// <param name="iFolderName">
		/// The root path of the iFolder or the iFolder ID.
		/// The iFolder ID can be queried from the object representing the
		/// iFolder.
		/// </param>
		/// <param name="byID">
		/// Specify <b>true</b> if <paramref name="name"/> is the iFolder ID,
		/// or <b>false</b> if it specifies the root path of the iFolder.
		/// </param>
		/// <remarks>
		/// This overload of DeleteiFolder is obsolete. Please do not use it.
		/// </remarks>
		[ Obsolete( "Please use DeleteiFolderByPath or DeleteiFolderById instead.", false ) ]
			public void DeleteiFolder(string iFolderName, bool byID)
			{
				//Property.Syntax propertyType;

				if (byID)
				{
					Collection tmpCollection = 
							store.GetCollectionById(iFolderName);

					if (tmpCollection.Type == iFolder.iFolderType)
					{
						tmpCollection.Delete(true);
						return;
					}
				}
				else
				{
					//string name= Path.GetFileName(iFolderName);
					Path.GetFileName(iFolderName);
					foreach (iFolder ifolder in this)
					{
						if (ifolder.LocalPath.CompareTo(iFolderName) == 0)
						{
							ifolder.Delete();
							return;
						}
					}
				}

				throw new ApplicationException( "iFolder " + iFolderName + 
						" not found" );
			}




		/// <summary>
		/// Deletes an iFolder by its ID.
		/// </summary>
		/// <param name="id">
		/// The ID of the iFolder.
		/// The iFolder ID can be queried from the <see cref="iFolder"/>.
		/// </param>
		public void DeleteiFolderById(string id)
		{
			Collection tmpCollection = store.GetCollectionById(id);

			// Make sure this is an iFolder.
			if (tmpCollection.Type == iFolder.iFolderType)
			{
				tmpCollection.Delete(true);
				return;
			}

			throw new ApplicationException( "iFolder with ID=" + id + 
					" not found" );
		}




		/// <summary>
		/// Deletes an iFolder by its local path.
		/// </summary>
		/// <param name="path">
		/// The rooted local path of the iFolder.
		/// </param>
		public void DeleteiFolderByPath(string path)
		{
			foreach (iFolder ifolder in this)
			{
				if (ifolder.LocalPath.Equals(
							path.Replace(Path.AltDirectorySeparatorChar, 
							Path.DirectorySeparatorChar)))
				{
					ifolder.Delete();
					return;
				}
			}

			throw new ApplicationException( "iFolder with LocalPath=" + 
					path + " not found" );
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
		public bool IsiFolder(string path)
		{
			string name = Path.GetFileName(path);

			ICSList list = store.GetCollectionsByName(name);
			foreach(Collection c in list)
			{
				if (c.Type == iFolder.iFolderType)
				{
					Uri documentRoot= (Uri)c.Properties.GetSingleProperty(Property.DocumentRoot).Value;
					if (Path.DirectorySeparatorChar == Convert.ToChar("\\"))
					{
						if (String.Compare(path, documentRoot.LocalPath, true) == 0)
						{
							return true;
						}
					}
					else
					{
						if (path.Equals(documentRoot.LocalPath))
						{
							return true;
						}
					}
				}
			}

			return false;
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
		public iFolder GetiFolderById(string id)
		{
			try
			{
				iFolder tmpiFolder = new iFolder(this.store, this.abMan);
				tmpiFolder.Load(this.store, id);
				return(tmpiFolder);
			}
			catch(Exception)
			{
				throw new ApplicationException("iFolder " + id + "not found");
			}
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
		public iFolder GetiFolderByPath(string path)
		{
			string name = Path.GetFileName(path);

			ICSList list = store.GetCollectionsByName(name);
			foreach(Collection c in list)
			{
				if (c.Type == iFolder.iFolderType)
				{
					Uri documentRoot= (Uri)c.Properties.GetSingleProperty(Property.DocumentRoot).Value;
					if (((Path.DirectorySeparatorChar == Convert.ToChar("\\")) &&
								(String.Compare(path, documentRoot.LocalPath, true) == 0)) ||
							path.Equals(documentRoot.LocalPath))
					{
						iFolder ifolder = new iFolder(store, abMan);
						ifolder.Load(store, (string)c.Properties.GetSingleProperty(Property.CollectionID).Value);
						return ifolder;
					}
				}
			}

			throw new ApplicationException("Path is not an iFolder: " + path);
		}




		/// <summary>
		/// Dredges the file system starting at the root of the iFolder and
		/// adds missing directories and files to the iFolder.
		/// </summary>
		/// <param name="id">
		/// The ID of the iFolder to update.
		/// </param>
		public void UpdateiFolder(string id)
		{
			iFolder ifolder = GetiFolderById(id);
			ifolder.Update();
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
		public bool CanBeiFolder(string path)
		{
			try
			{
				// Replace slash/backslash with backslash/slash as needed
				path = path.Replace(Path.AltDirectorySeparatorChar, 
						Path.DirectorySeparatorChar);

				foreach(iFolder ifolder in this)
				{
					// Adding a separator Char on the end of each so we
					// can really match if this is a path
					// /home/calvin should not match
					// /home/calvin's stuff
					// but
					// /home/calvin should match
					// /home/calvin/files
					// Adding a slash on the end of each will test this
					// correctly
					string rootPath = ifolder.LocalPath + 
							Path.DirectorySeparatorChar.ToString();
					string testPath = path + 
							Path.DirectorySeparatorChar.ToString();

					// Check if path is within the iFolder
					if (testPath.StartsWith(rootPath))
						return false;

					// Check if path contains the iFolder
					if(rootPath.StartsWith(testPath))
						return false;
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
				return false;
			}

			return true;
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
		public bool IsPathIniFolder(string path)
		{
			try
			{
				// Replace slash/backslash with backslash/slash as needed
				path = path.Replace(Path.AltDirectorySeparatorChar,
						Path.DirectorySeparatorChar);

				foreach(iFolder ifolder in this)
				{
					// Adding a separator Char on the end of each so we
					// can really match if this is a path
					// /home/calvin should not match
					// /home/calvin's stuff
					// but
					// /home/calvin should match
					// /home/calvin/files
					// Adding a slash on the end of each will test this
					// correctly.
					string rootPath = ifolder.LocalPath +
							Path.DirectorySeparatorChar.ToString();
					string testPath = path + 
							Path.DirectorySeparatorChar.ToString();
					if (testPath.StartsWith(rootPath))
					{
						return true;
					}
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}
			return false;
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
		public void AcceptInvitation(Invitation invitation, string path)
		{
			// Check the path to see if it is inside an existing iFolder
			if(IsPathIniFolder(path))
			{
				throw new ApplicationException("The path specified is " +
						"located within an existing iFolder" );
			}

			invitation.RootPath = path;

			AgentFactory.GetInviteAgent().Accept(store, invitation);
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
			//Console.WriteLine("Manager::GetEnumerator called");
			storeEnum = store.GetEnumerator();
			return(this);
		}




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
			//Console.WriteLine("Manager::MoveNext called");

			// Need to make sure the next object is an iFolder collection
			while(storeEnum.MoveNext() == true)
			{
				Collection tmpCollection = (Collection) storeEnum.Current;

				if (tmpCollection.Type == iFolder.iFolderType)
				{
					return (true);
				}
			}

			return(false);
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
				//Console.WriteLine("Manager::Current called");
				Collection currentCollection = (Collection) storeEnum.Current;

				iFolder ifolder =
					this.GetiFolderById( (string) currentCollection.
						Properties.GetSingleProperty( Property.CollectionID ).
						Value );
				//Console.WriteLine("   Name: {0}", addrBook.Name);
				return(ifolder);
			}
		}




		/// <summary>
		/// Sets the enumerator to its initial position, which is before the
		/// first iFolder in the store.
		/// </summary>
		public void Reset()
		{
			//Console.WriteLine("Manager::Reset called");
			storeEnum.Reset();
		}

#endregion

	}
}
