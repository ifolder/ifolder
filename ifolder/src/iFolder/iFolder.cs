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
using System.Net;
using Simias.Storage;
using Simias.Agent;
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
		private	Collection						collection = null;
		private	Novell.AddressBook.AddressBook	ab = null;
		private Store							store = null;
		private Novell.AddressBook.Manager 		abMan = null;

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
			Deny,

			/// <summary>
			/// User can view information in an iFolder.
			/// </summary>
			ReadOnly,

			/// <summary>
			/// User can view and modify information in an iFolder.
			/// </summary>
			ReadWrite,

			/// <summary>
			/// User can view, modify and change rights in an iFolder.
			/// </summary>
			Admin
		};
#endregion

#region Constructors
		internal iFolder(Store store, Novell.AddressBook.Manager manager)
		{
			this.store = store;
			this.abMan = manager;
		}
#endregion

#region Internal methods
		/// <summary>
		/// Add a files/directories to an iFolder recursively.
		/// </summary>
		/// <param name="path">The path at which to begin the recursive 
		/// add.</param>
		/// <param name="count">Holds the number of nodes added.</param>
		internal void AddiFolderNodes(string path, ref int count)
		{
			// Get the files in this directory.
			string[] dirs = Directory.GetFiles(path);
			foreach (string file in dirs)
			{
				// Create the iFolderFile of type File.
				AddiFolderNode(file);
				count++;
			}

			// Get the sub-directories in this directory.
			dirs = Directory.GetDirectories(path);
			foreach (string dir in dirs)
			{
				// Create the iFolderFile of type File.
				AddiFolderNode(dir);
				count++;

				// Recurse and add files/directories contained in directory.
				AddiFolderNodes(dir, ref count);
			}
		}




		/// <summary>
		/// Method: Load
		/// Abstract: iFolders are created only through the manager class.
		/// The FinalConstructor method is called after construction so 
		/// exceptions can be generated back to the manager method 
		/// "CreateiFolder".
		///
		/// </summary>
		///
		internal void Load(Store callingStore, string iFolderID)
		{
			//Property.Syntax propertyType;

			store = callingStore;
			this.collection = store.GetCollectionById(iFolderID);


			// Make sure this collection has our store propery
			if(this.collection.Type != iFolderType)
			{
				// Raise an exception here
			}

			ConnectAddressBook();
		}




		/// <summary>
		/// Method: ConnectAddressBook
		/// Abstract: Internal method to load the correct default
		/// AddressBook for resolving contact information
		/// </summary>
		///
		internal void ConnectAddressBook()
		{
			// TODO: where should we discover the contact information?
			// CRG: this was connecting to the wrong store
//			Novell.AddressBook.Manager abManager =
//				Novell.AddressBook.Manager.Connect(
//						collection.LocalStore.StorePath);

			LocalAddressBook lab = store.GetLocalAddressBook();
			this.ab = abMan.GetAddressBookByName(lab.Name);
		}





		/// <summary>
		/// Create an iFolder collection.  This version of create creates 
		/// the iFolder with a name of the leaf node of the path.
		/// </summary>
		/// <param name="path">The path where the iFolder collection will
		/// be rooted.</param>
		internal void Create(string path)
		{
			// Make sure the path doesn't end with a separator character.
			if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				path= path.Substring(0, path.Length - 1);
			}

			// Get the leaf name.
			string name = Path.GetFileName(path);

			Uri documentRoot = new Uri(path);

			// Call to create the collection.
			this.collection = store.CreateCollection(name, iFolderType, 
					documentRoot);

			this.collection.Commit();

			ConnectAddressBook();
		}




		/// <summary>
		/// Create an iFolder collection.  iFolders are created only through
		/// the manager class.  The FinalConstructor method is called after
		/// construction so exceptions can be generated back to the manager
		/// method "CreateiFolder".
		/// </summary>
		/// <param name="name">The friendly name of the collection.</param>
		/// <param name="path">The path where the iFolder collection will be
		/// rooted.</param>
		internal void Create(string name, string path)
		{
			// Make sure the path doesn't end with a separator character.
			if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				path= path.Substring(0, path.Length - 1);
			}

			Uri documentRoot = new Uri(path);
			this.collection = store.CreateCollection(name, iFolderType,
					documentRoot);

			this.collection.Commit();

			ConnectAddressBook();
		}




		internal void Delete()
		{
			collection.Delete(true);
		}




		internal Node GetNodeForPath(string path)
		{
			Node node = null;

			// Get the leaf name.
			string relativeName = Path.GetFileName(path);

			// Search the collection for the leaf name.
			// TODO - ugh, fix this unreadable line of code
			ICSEnumerator e = (ICSEnumerator)collection.Search( Property.ObjectName, relativeName, Property.Operator.Equal).GetEnumerator();
			try
			{
				bool found = false;

				// Get the next node.
				while (!found && e.MoveNext())
				{
					// Initialize parentName to the directory name of the path.
					string parentName = Path.GetDirectoryName(path);

					// Initialize parentNode to the parent of the this node.
					Node parentNode = ((Node)e.Current).GetParent();

					if (parentNode == null)
					{
						// TODO - ugh, fix this unreadable line of code
						Uri documentRoot = (Uri)((Collection)e.Current).Properties.  GetSingleProperty(Property.DocumentRoot).  Value;
	
						// TODO - ugh, fix this unreadable line of code
						if(	((Path.DirectorySeparatorChar == Convert.ToChar("\\")) && (String.Compare(path, documentRoot.LocalPath, true) == 0)) || path.Equals(documentRoot.LocalPath))
						{
							node = (Node)e.Current;
						}

						// There isn't a parent node, so we are done.
						break;
					}

					// See if this node is the one we want.
					while (true)
					{
						// See if the directory name matches the parent 
						// node name.
						// TODO - ugh, fix this unreadable line of code
						string parentPath = Path.GetFileName(parentName);
						if( ((Path.DirectorySeparatorChar == Convert.ToChar("\\")) && (String.Compare(parentPath, parentNode.Name, true) != 0)) || !parentPath.Equals(parentNode.Name))
						{
							// This isn't the right node, move on to the 
							// next one.
							break;
						}

						// Check if we're at the top node of the collection.
						if (parentNode.Type == iFolderType)
						{
							// This must be the right node.
							found = true;
							node = (Node)e.Current;
							break;
						}

						// Move up a directory in the name.
						parentName = Path.GetDirectoryName(parentName);

						// Move up to the parent node.
						parentNode = parentNode.GetParent();						
					}
				}

				if (node == null)
				{
					// See if this path is the root of the collection.
					// TODO - ugh, fix this unreadable line of code
					if (((Path.DirectorySeparatorChar == Convert.ToChar("\\")) && (String.Compare(path, LocalPath, true) == 0)) || path.Equals(LocalPath))
					{
						node = collection;
					}
				}
			}
			finally
			{
				e.Dispose();
			}

			return node;
		}
#endregion




#region Properties
		/// <summary>
		/// Gets the identity of the owner of the iFolder.
		/// </summary>
		public string OwnerIdentity 
		{
			get
			{
				return(this.collection.Owner);
			}
		}




		/// <summary>
		/// Gets/sets the name of the iFolder.
		/// </summary>
		public string Name
		{
			get
			{
				return(this.collection.Name);
			}

			set
			{
				this.collection.Name = value;
			}
		}




		/// <summary>
		/// Gets the iFolder ID.
		/// </summary>
		public string ID
		{
			get
			{
				//Property.Syntax propertyType;
				return((string) this.collection.Properties.
						GetSingleProperty( Property.CollectionID ).Value);
			}
		}




		/// <summary>
		/// Gets the local path of the iFolder.
		/// </summary>
		public string LocalPath
		{
			get
			{
				//Property.Syntax propertyType;
				Uri documentRoot= (Uri) this.collection.Properties.
					GetSingleProperty(Property.DocumentRoot).Value;
				return(documentRoot.LocalPath);
			}
		}




		/// <summary>
		/// Gets/sets the current node in the iFolder.
		/// </summary>
		public Node CurrentNode
		{
			get
			{
				return collection;
			}
		}




		/// <summary>
		/// Gets/sets the current node in the iFolder.
		/// </summary>
		internal Collection Collection
		{
			get
			{
				return collection;
			}
		}
#endregion




#region Public Methods
		/// <summary>
		/// Updates and adds all files under a given iFolder
		/// </summary>
		/// <param name="path">The path at which to begin the recursive 
		/// add.</param>
		/// <param name="count">Holds the number of nodes added.</param>
		public void Update()
		{
			AddiFolderNodes(LocalPath);
			collection.Commit();
		}




		/// <summary>
		/// Add a files/directories to an iFolder recursively.
		/// </summary>
		/// <param name="path">The path at which to begin the recursive 
		/// add.</param>
		/// <returns>An int with the number of nodes added.</returns>
		public int AddiFolderNodes(string path)
		{
			int count = 0;

			// if the current node is not added, add it now
			AddiFolderNode(path);
			count++;

			AddiFolderNodes(path, ref count);

			return count;
		}



		/// <summary>
		/// Add a single file/directory to an iFolder.
		/// </summary>
		/// <param name="fileName">The path to the File/directory to 
		/// add.</param>
		public iFolderNode AddiFolderNode(string path)
		{
			// Get the leaf name from the path.
			string name = Path.GetFileName(path);
			string type;

			if(File.Exists(path))
			{
				type = iFolderFileType;
			}
			else if(Directory.Exists(path))
			{
				type = iFolderDirectoryType;
			}
			else
			{
				throw new ArgumentException("Path " + path + 
										" does not exist!");
			}

			// Make sure the node doesn't already exist.
			iFolderNode ifNode = GetiFolderNodeByPath(path);
			if(ifNode == null)
			{
				string parentPath = Path.GetDirectoryName(path);

				Node parentNode = GetNodeForPath(parentPath);
				if(parentNode != null)
				{
					// Create the node.
					Node node = parentNode.CreateChild(name, type);
					ifNode = new iFolderNode(this, node);
					node.Commit();
				}
				else
				{
					throw new ArgumentException("Path " + path +
						" could not be added because the parent " + 
						parentPath + " was not in the iFolder");
				}
			}

			return ifNode;
		}




		/// <summary>
		/// Gets an <see cref="iFolderNode"/> for a given iFolder file ID.
		/// </summary>
		/// <param name="fileID">
		/// The iFolder file ID of the file.
		/// </param>
		/// <returns>
		/// An <see cref="iFolderNode"/> for the file specified by 
		/// <paramref name="fileID"/>.
		/// </returns>
		public iFolderNode GetiFolderNodeByID(string fileID)
		{
			Node node = collection.GetNodeById(fileID);

			iFolderNode ifNode = new iFolderNode(this, node);

			return(ifNode);
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
		/// by <paramref name="path"/>, or <b>null</b> if it does not exist
		/// in the iFolder.
		/// </returns>
		public iFolderNode GetiFolderNodeByPath(string path)
		{
			// Replace slash/backslash with backslash/slash as needed
			path = path.Replace(Path.AltDirectorySeparatorChar,
					Path.DirectorySeparatorChar);

			Node node = GetNodeForPath(path);
			if (node != null)
			{
				// Make an iFolderFile object to return.
				iFolderNode ifNode = new iFolderNode(this, node);
				return ifNode;
			}

			return null;
		}




		/// <summary>
		/// Returns the relative path to the iFolder of the passed in path
		/// </summary>
		/// <param name="path">
		/// An absolute path to a file or directory.
		/// </param>
		/// <returns>
		/// The path relative to the root of the iFolder.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="path"/> is not subordinate to this iFolder.
		/// </exception>
		public string GetiFolderRelativePath(string path)
		{
			//int test= path.IndexOf(LocalPath);

			// Get the substring from the path that represents the iFolder path.
			string pathToTest= path.Substring(0, LocalPath.Length);

			// Check if the paths are equal.
			if (!Path.Equals(LocalPath, pathToTest))
			{
				throw new ArgumentException("File " + path + 
						" is not subordinate to this iFolder!");
			}

			string relativeName= path.Remove(0, LocalPath.Length);

			// Make sure the relative path doesn't start with separator char.
			if (relativeName.StartsWith(Path.DirectorySeparatorChar.ToString()))
			{
				relativeName= relativeName.Remove(0, 1);
			}

			return relativeName;
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
			return (new IFAccessControlList(collection, ab));
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
			public Access.Rights GetShareAccess(string userID)
			{
				return collection.GetUserAccess(userID);
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
			return (Rights)collection.GetUserAccess(contact.ID);
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
			public void Share(string userID, Access.Rights rights, bool invite)
			{
				// Set the specified rights on the collection only if the id 
				// is not
				// the current owner.
				if (collection.LocalStore.CurrentUser != userID)
				{
					collection.SetUserAccess(userID, rights);
					collection.Commit();
				}

				if (invite)
				{
					// inform the notification service that we have shared
					IInviteAgent agent = AgentFactory.GetInviteAgent();
					Invitation invitation = agent.CreateInvitation(collection,
							userID);

					// TODO: where should we discover the contact information?
//					Novell.AddressBook.Manager abManager =
//						Novell.AddressBook.Manager.Connect(
//								collection.LocalStore.StorePath);
//					Novell.AddressBook.AddressBook ab = 
//						abManager.OpenDefaultAddressBook();

					// from
					Contact from = ab.GetContact(collection.Owner);
					invitation.FromName = from.FN;
					invitation.FromEmail = from.EMail;

					// to
					Contact to = ab.GetContact(userID);
					invitation.ToName = to.FN;
					invitation.ToEmail = to.EMail;

					// send the invitation
					agent.Invite(invitation);
				}
			}




		/// <summary>
		/// Sends an invitation to the specified contact using their
		/// preferred method of being contacted
		/// </summary>
		/// <param name="contact">
		/// The <see cref="Contact"/> to send an invitation to.
		/// </param>
		public void Invite(Contact contact)
		{
			// inform the notification service that we have shared
			IInviteAgent agent = AgentFactory.GetInviteAgent();
			Invitation invitation = agent.CreateInvitation(collection,
					contact.ID);

			// TODO: where should we discover the contact information?
			// CRG: this was connecting to the wrong store
//			Novell.AddressBook.Manager abManager =
//				Novell.AddressBook.Manager.Connect(
//						collection.LocalStore.StorePath);

//			Novell.AddressBook.Manager abManager =
//				Novell.AddressBook.Manager.Connect();

//			Novell.AddressBook.AddressBook ab = 
//				abManager.OpenDefaultAddressBook();

			// from
			Contact from = ab.GetContact(collection.Owner);
			invitation.FromName = from.FN;
			invitation.FromEmail = from.EMail;

			// to
			invitation.ToName = contact.FN;
			invitation.ToEmail = contact.EMail;

			// send the invitation
			agent.Invite(invitation);
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
		public void CreateInvitationFile(Contact contact, string filePath)
		{
			// inform the notification service that we have shared
			IInviteAgent agent = AgentFactory.GetInviteAgent();
			Invitation invitation = agent.CreateInvitation(collection,
					contact.ID);

			// TODO: where should we discover the contact information?
//			Novell.AddressBook.Manager abManager =
//				Novell.AddressBook.Manager.Connect(
//						collection.LocalStore.StorePath);
//			Novell.AddressBook.AddressBook ab = 
//				abManager.OpenDefaultAddressBook();

			// from
			Contact from = ab.GetContact(collection.Owner);
			invitation.FromName = from.FN;
			invitation.FromEmail = from.EMail;

			// to
			invitation.ToName = contact.FN;
			invitation.ToEmail = contact.EMail;

			invitation.Save(filePath);
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
		public void SetRights(Contact contact, Rights rights)
		{
			if (collection.LocalStore.CurrentUser != contact.ID)
			{
				collection.SetUserAccess(contact.ID, (Access.Rights)rights);
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
			public void SetShareAccess(string userID, Access.Rights rights)
			{
				if (collection.LocalStore.CurrentUser != userID)
				{
					collection.SetUserAccess(userID, rights);
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
			if (collection.LocalStore.CurrentUser != contact.ID)
			{
				collection.RemoveUserAccess( contact.ID );
				collection.Commit();
			}
		}


		/// <summary>
		/// Removes all access rights on the iFolder for the specified contact.
		/// </summary>
		/// <param name="contact">
		/// The ID of the user for which to remove access rights.
		/// </param>
		[ Obsolete( "This method is marked for removal.  Use RemoveRights(Contact contact) instead.", false ) ]
			public void RemoveUserAccess( string userId )
			{
				collection.RemoveUserAccess( userId );
				collection.Commit();
			}
#endregion
	}
}
