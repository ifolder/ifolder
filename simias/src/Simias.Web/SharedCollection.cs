/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.IO;
using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;

#if MONO
	using Mono.Posix;
#endif

namespace Simias.Web
{
	/// <summary>
	/// SharedCollection implements all of the APIs needed to use a Shared
	/// Collection in Simias.  The APIs are designed to be wrapped by a
	/// WebService
	/// </summary>
	public class SharedCollection
	{
		public static readonly string FilesDirName = "SimiasFiles";


		/// <summary>
		/// Creates a new collection of the type specified
		/// </summary>
		/// <param name = "Name">
		/// The name of the Collection to be created
		/// </param>
		/// <param name = "UserID">
		/// The UserID to be made the owner of this Collection.
		/// A subsciption will be placed in this UserID's POBox.
		/// </param>
		/// <param name = "Type">
		/// A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type
		/// will be added.
		/// </param>
		/// <returns>
		/// Collection that was created
		/// </returns>
		public static Collection CreateSharedCollection(
				string Name, string UserID, string Type)
		{
			return CreateSharedCollection(Name, UserID, Type, false, null);
		}




		/// <summary>
		/// Creates a new collection of the type specified.  It gets the
		/// current member and makes them the owner
		/// </summary>
		/// <param name = "Name">
		/// The name of the Collection to be created
		/// </param>
		/// <param name = "Type">
		/// A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type
		/// will be added.
		/// </param>
		/// <returns>
		/// Collection that was created
		/// </returns>
		public static Collection CreateSharedCollection(
				string Name, string Type)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);
			if(domain == null)
				throw new Exception("Unable to obtain domain");

			Simias.Storage.Member member = domain.GetCurrentMember();
			if(member == null)
				throw new Exception("Unable to obtain current member");

			return CreateSharedCollection(Name, member.UserID, 
						Type, false, null);
		}




		/// <summary>
		/// Creates a new collection of the type specified.  It gets the
		/// current member and makes them the owner
		/// </summary>
		/// <param name = "LocalPath">
		/// The name of the Collection to be created
		/// </param>
		/// <param name = "Type">
		/// A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type
		/// will be added.
		/// </param>
		/// <returns>
		/// Collection that was created
		/// </returns>
		public static Collection CreateLocalSharedCollection(
				string LocalPath, string Type)
		{
			Store store = Store.GetStore();

			Domain domain = 
					store.GetDomain(store.DefaultDomain);
			if(domain == null)
				throw new Exception("Unable to obtain default domain");

			Simias.Storage.Member member = domain.GetCurrentMember();
			if(member == null)
				throw new Exception("Unable to obtain current member");


			String name = Path.GetFileName(LocalPath);

			return CreateSharedCollection(name, member.UserID, 
						Type, true, LocalPath);
		}




		/// <summary>
		/// Creates a new collection of the type specified.  It gets the
		/// current member and makes them the owner
		/// </summary>
		/// <param name = "LocalPath">
		/// The name of the Collection to be created
		/// </param>
		/// <param name = "Type">
		/// A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type
		/// will be added.
		/// </param>
		/// <returns>
		/// Collection that was created
		/// </returns>
		public static Collection CreateLocalSharedCollection(
				string LocalPath, string DomainID, string Type)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(DomainID);
			if(domain == null)
				throw new Exception("Unable to obtain default domain");

			Simias.Storage.Member member = domain.GetCurrentMember();
			if(member == null)
				throw new Exception("Unable to obtain current member");


			String name = Path.GetFileName(LocalPath);

			return CreateSharedCollection(name, DomainID, member.UserID, 
						Type, true, LocalPath);
		}




		/// <summary>
		/// WebMethod that creates and SharedCollection
		/// </summary>
		/// <param name = "Name">
		/// The name of the SharedCollection to create.  If a Path is
		/// Specified, it must match the name of the last folder in the path
		/// </param>
		/// <param name = "UserID">
		/// The UserID to be made the owner of this SharedCollection. 
		/// A subsciption will be placed in this UserID's POBox.
		/// </param>
		/// <param name = "Type">
		/// A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type
		/// will be added.
		/// </param>
		/// <param name = "CollectionPath">
		/// The full path to this SharedCollection.  If Path is null or "",
		/// it will be ignored. The last folder name in the path should
		/// match the name of this SharedCollection
		/// </param>
		/// <returns>
		/// Collection object that was created
		/// </returns>
		public static Collection CreateSharedCollection(
				string Name, string UserID, string Type, 
				bool UnmanagedFiles, string CollectionPath)
		{
			return (CreateSharedCollection(Name, Store.GetStore().DefaultDomain,
										   UserID, Type, UnmanagedFiles, CollectionPath));
		}




		/// <summary>
		/// WebMethod that creates a SharedCollection
		/// </summary>
		/// <param name="Name">The name of the SharedCollection to create.  If a Path is
		/// specified, it must match the name of the last folder in the path.</param>
		/// <param name="DomainID">The ID of the domain to create the collection in.</param>
		/// <param name="UserID">The UserID to be made the owner of this SharedCollection. 
		/// A subscription will be placed in this UserID's POBox.</param>
		/// <param name="Type">A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type will be added.</param>
		/// <param name="UnmanagedFiles">A value indicating if this collection contains files
		/// that are not store-managed.</param>
		/// <param name="CollectionPath">The full path to this SharedCollection.  If Path is 
		/// null or "", it will be ignored. The last folder name in the path should match the 
		/// name of this SharedCollection</param>
		/// <returns>The Collection object that was created.</returns>
		public static Collection CreateSharedCollection(
			string Name, string DomainID, string UserID, string Type,
			bool UnmanagedFiles, string CollectionPath)
		{
			ArrayList nodeList = new ArrayList();

			// if they are attempting to create a Collection using
			// a path, then check to see if that path can be used
			if(	UnmanagedFiles && (CollectionPath != null) )
			{
				if( CanBeCollection( CollectionPath )  == false )
				{
					throw new Exception("Path is either a parent or child of an existing Collection.  Collections cannot be nested");
				}
			}

			Configuration config = Configuration.GetConfiguration();
			Store store = Store.GetStore();

			// Create the Collection and set it as an iFolder
			Collection c = 
					new Collection(store, Name, DomainID);

			if( (Type != null) && (Type.Length > 0) )
				c.SetType(c, Type);

			nodeList.Add(c);

			// Create the member and add it as the owner
			Domain domain = store.GetDomain(DomainID);
			if(domain == null)
				throw new Exception("Unable to obtain default domain");

			Simias.Storage.Member member = domain.GetMemberByID(UserID);
			if(member == null)
				throw new Exception("UserID is invalid");
				
			Simias.Storage.Member newMember = 
					new Simias.Storage.Member(	member.Name,
												member.UserID,
												Access.Rights.Admin);
			newMember.IsOwner = true;
			nodeList.Add(newMember);

			if(UnmanagedFiles)
			{
				string dirNodePath;

				if( (CollectionPath == null) || (CollectionPath.Length == 0) )
				{
					// create a root dir node for this iFolder in the
					// ~/.local/shared/simias/SimiasFiles/<guid>/name
					// directory
					dirNodePath = Path.Combine(config.StorePath, FilesDirName);
					dirNodePath = Path.Combine(dirNodePath, c.ID);
					dirNodePath = Path.Combine(dirNodePath, Name);

					if(!Directory.Exists(dirNodePath) )
						Directory.CreateDirectory(dirNodePath);
				}
				else
					dirNodePath = CollectionPath;

				if(!Directory.Exists(dirNodePath) )
					throw new Exception("Path did not exist");

				// create root directory node
				DirNode dn = new DirNode(c, dirNodePath);
				nodeList.Add(dn);
			}

			// Commit the new collection and the fileNode at the root
			c.Commit(nodeList.ToArray( typeof( Node) ) as Node[] );

			AddSubscription( store, c, member, 
					newMember, SubscriptionStates.Ready, Type);

			return c;
		}




		/// <summary>
		/// Checks whether it is valid to make a given directory a
		/// Collection
		/// </summary>
		/// <param name="path">
		/// An absolute path to check.
		/// </param>
		/// <returns>
		/// True if the path can be a Collection, otherwise false
		/// </returns>
		/// <remarks>
		/// Nested Collections (iFolder) are not permitted.  The path is
		/// checked to see if it is within, or contains, a Collection .
		/// </remarks>
		public static bool CanBeCollection( string path )
		{
			bool canBeCollection = true;

			// Don't allow the root of a drive to be a collection.
			string parentDir = Path.GetDirectoryName( path );
			if ( ( parentDir == null ) || ( parentDir == String.Empty ) )
			{
				return false;
			}

			// Make sure the name of the collection doesn't contain any invalid
			// characters.  Also make sure that the path doesn't end with a
			// slash character.
			string collectionName = path.Substring(parentDir.Length + 1);
			if (collectionName == null || collectionName == String.Empty
				|| !Simias.Sync.SyncFile.IsNameValid(collectionName))
			{
				return false;
			}

			// Make sure the paths end with a separator.
			// Create a normalized path that can be compared on any platform.
			Uri nPath = GetUriPath(path);

			// The store path cannot be used nor any path under the store path.
			string excludeDirectory = Configuration.GetConfiguration().StorePath;
			if (ExcludeDirectory(nPath, excludeDirectory, true))
			{
				return false;
			}

			// Any path containing the store path cannot be used
			while (true)
			{
				excludeDirectory = Path.GetDirectoryName(excludeDirectory);
				if ((excludeDirectory == null) || excludeDirectory.Equals(Path.DirectorySeparatorChar.ToString()))
				{
					break;
				}

				if (ExcludeDirectory(nPath, excludeDirectory, false))
				{
					return false;
				}
			}

#if WINDOWS
			// Only allow fixed drives to become iFolders.
			if (GetDriveType(Path.GetPathRoot(path)) != DRIVE_FIXED)
			{
				return false;
			}

			// Don't allow System directories to become iFolders.
			if ((new DirectoryInfo(path).Attributes & FileAttributes.System) == FileAttributes.System)
			{
				return false;
			}
#endif

			if (MyEnvironment.Windows)
			{
				// Don't allow the system drive to become an iFolder.
				excludeDirectory = Environment.GetEnvironmentVariable("SystemDrive");
				if (ExcludeDirectory(nPath, excludeDirectory, false))
				{
					return false;
				}

				// Don't allow the Windows directory or subdirectories become an iFolder.
				excludeDirectory = Environment.GetEnvironmentVariable("windir");
				if (ExcludeDirectory(nPath, excludeDirectory, true))
				{
					return false;
				}

				// Don't allow the Program Files directory or subdirectories become an iFolder.
				excludeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
				if (ExcludeDirectory(nPath, excludeDirectory, true))
				{
					return false;
				}
			}

#if MONO
			string ifPath = null; 
	
			// Sometimes we are asking about a folder that exists and
			// sometimes we are asking about one that doesn't.  If the
			// current folder doesn't exist, and the parent doesn't
			// exist, it ain't valid, return false;
			if(!System.IO.Directory.Exists(path))
			{
				DirectoryInfo di = System.IO.Directory.GetParent(path);
				if(di.Exists)
					ifPath = di.FullName;
				else
					return false;
			}
			else
				ifPath = path;

			// Check to see if the user has write rights to the
			// path used as a collection
			try
			{
				if(Mono.Posix.Syscall.access(ifPath, 
							Mono.Posix.AccessMode.W_OK) != 0)
				{
					return false;
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}


			// put an ugly try catch around this to see what is 
			// happening
			try
			{
				// This will check on Linux to see if a path is on a physical
				// drive and not mounted off the network
				if(File.Exists("/proc/mounts"))
				{
					bool retval = false;
	
					FileStream fs = File.OpenRead("/proc/mounts");
					if( (fs != null) && (ifPath != null) )
					{
						StreamReader sr = new StreamReader(fs);
						string mntLine = sr.ReadLine();
	
						// Get the stat structure on the path
						Stat pathStat;
						Mono.Posix.Syscall.stat(ifPath, out pathStat);
	
						while(mntLine != null)
						{
							// verify it's a device on this box
							if(mntLine.StartsWith("/dev"))
							{
								Stat stat;
								string[] entries;
		
								entries = mntLine.Split(' ');
								Mono.Posix.Syscall.stat(entries[1], out stat);
		
								if(stat.Device == pathStat.Device)
								{
									retval = true;
									break;
								}
							}
							mntLine = sr.ReadLine();
						}
						sr.Close();
					}
					else
					{
						Console.WriteLine("ERROR: Unable to open /proc/mounts");
					}
	
					if(!retval)
					{
						Console.WriteLine("Unable to find a physical device for: {0}", ifPath);
						return retval;
					}
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
#endif


			Store store = Store.GetStore();

			// TODO: Change this into a search
			foreach(ShallowNode sn in store)
			{
				Collection col = store.GetCollectionByID(sn.ID);
				DirNode dirNode = col.GetRootDirectory();
				if(dirNode != null)
				{
					Uri colPath = GetUriPath(dirNode.GetFullPath(col));

					if(nPath.LocalPath.StartsWith( colPath.LocalPath ) || 
						colPath.LocalPath.StartsWith( nPath.LocalPath ) )
					{
						canBeCollection = false;
						break;
					}
				}
			}

			return canBeCollection;
		}




		/// <summary>
		/// Generates a comparable URI path
		/// </summary>
		/// <param name="path">
		/// Path to build URI from
		/// </param>
		/// <returns>
		/// Uri that can be compared against another Uri
		/// </returns>
		public static Uri GetUriPath(string path)
		{
			Uri uriPath = new Uri( path.EndsWith(
								Path.DirectorySeparatorChar.ToString()) ?
								path :
								path + Path.DirectorySeparatorChar.ToString());
			return uriPath;
		}




		/// <summary>
		/// Checks whether a given path is within an existing Collection.
		/// </summary>
		/// <param name="path">
		/// An absolute path to check.
		/// </param>
		/// <returns>
		/// <b>true</b> if <paramref name="path"/> is in a Collection;
		/// otherwise, <b>false</b>.
		/// </returns>
		public static bool IsPathInCollection( string path )
		{
			bool inCollection = false;
			Store store = Store.GetStore();

			// Create a normalized path that can be compared on any platform.
			Uri nPath = GetUriPath( path );

			foreach(ShallowNode sn in store)
			{
				Collection col = store.GetCollectionByID(sn.ID);
				DirNode dirNode = col.GetRootDirectory();
				if(dirNode != null)
				{
					Uri colPath = GetUriPath( dirNode.GetFullPath(col) );
					if(nPath.LocalPath.StartsWith( colPath.LocalPath ) )
					{
						inCollection = true;
						break;
					}
				}
			}
			return inCollection;
		}




		/// <summary>
		/// Checks whether a given directory is a Collection
		/// </summary>
		/// <param name="path">
		/// An absolute path to check.
		/// </param>
		/// <returns>
		/// <b>true</b> if <paramref name="path"/> is a Collection;
		/// otherwise, <b>false</b>.
		/// </returns>
		public static bool IsCollection( string path )
		{
			return ( GetCollectionByPath( path ) != null ) ? true : false;
		}




		/// <summary>
		/// Get a Collection by it's local path
		/// </summary>
		/// <param>
		/// The rooted local path of the Collection.
		/// </param>
		/// <returns>
		/// A Collection object if found
		/// </returns>
		public static Collection GetCollectionByPath( string path )
		{
			Collection col = null;
			Uri nPath = GetUriPath( path );

			Store store = Store.GetStore();

			// In the old iFolder code we used to get the Collection by
			// name and use the filename on the path passed in.  That 
			// will not work because the name of the collection does
			// not have to be the name of the directory that contains it
			foreach(ShallowNode sn in store)
			{
				Collection tmpCol = store.GetCollectionByID(sn.ID);
				DirNode dirNode = tmpCol.GetRootDirectory();
				if(dirNode != null)
				{
					Uri colPath = GetUriPath( dirNode.GetFullPath(tmpCol) );
					// Compare the two paths and ignore the case if our
					// platform is not Unix
					if ( String.Compare( nPath.LocalPath, colPath.LocalPath, 
							!MyEnvironment.Unix ) == 0 )
					{
						col = tmpCol;
						break;
					}
				}
			}
			return col;
		}




		/// <summary>
		/// WebMethod that deletes a SharedCollection and removes all
		/// subscriptions from all members.  Any files that were in place
		/// if there was a DirNode will remain there
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing this iFolder to delete
		/// </param>
		/// <returns>
		/// true if the iFolder was successfully removed
		/// </returns>
		public static void DeleteSharedCollection(string CollectionID)
		{
			DeleteSharedCollection(CollectionID, null);
		}

		/// <summary>
		/// WebMethod that deletes a SharedCollection and removes all
		/// subscriptions from all members.  Any files that were in place
		/// if there was a DirNode will remain there
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing this iFolder to delete
		/// </param>
		/// <param name="accessID">Access User ID</param>
		/// <returns>
		/// true if the iFolder was successfully removed
		/// </returns>
		public static void DeleteSharedCollection(string CollectionID, string accessID)
		{
			Store store = Store.GetStore();
			Collection collection = store.GetCollectionByID(CollectionID);
			
			if(collection != null)
			{
				// impersonate
				if ((accessID != null) && (accessID.Length != 0))
				{
					Simias.Storage.Member member = collection.GetMemberByID(accessID);
				
					if(member == null)
						throw new Exception("Invalid UserID");
				
					collection.Impersonate(member);

					// admin rights
					// note: check for admin rights before cleaning subscriptions
					if (member.Rights != Access.Rights.Admin)
					{
						throw new AccessException(collection, member, Access.Rights.Admin);
					}
				}
			
				// first clean out all subscriptions for this iFolder
				// if we are the server in a workgroup, then this
				// will clean out everyone else's subscriptions too
				RemoveAllSubscriptions(store, collection);

				collection.Delete();
				collection.Commit();
				return;
			}
			else
			{
				// TODO: Need to rework for multi-domain ... check if this method is used anywhere.
				// try to find a subscription with this ID
				Simias.POBox.POBox poBox = Simias.POBox.POBox.GetPOBox(                                             store,
									store.DefaultDomain);

				// SharedCollections returned in the Web service are also
				// Subscriptions and it ID will be the subscription ID
				Node node = poBox.GetNodeByID(CollectionID);
				if(node == null)
					throw new Exception("Invalid ID");

				Subscription sub = new Subscription(node);
				if(sub != null)
				{
					poBox.Delete(sub);
					poBox.Commit(sub);
					return;
				}
			}

			throw new Exception("Invalid CollectionID");
		}





		/// <summary>
		/// WebMethod that removes a SharedCollection from the local store
		/// but will leave the subscription intact.  It will result in
		/// removing the SharedCollection from this computer but remain
		/// a member.
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing this iFolder to delete
		/// </param>
		/// <returns>
		/// The subscription for this iFolder
		/// </returns>
		public static Subscription RevertSharedCollection(string CollectionID)
		{
			Store store = Store.GetStore();
			Collection collection = store.GetCollectionByID(CollectionID);
			if(collection == null)
				throw new Exception("Invalid CollectionID");

			
			// Get the subscription for this iFolder to return.
			Subscription sub = null;

			// Get the member's POBox
			Simias.POBox.POBox poBox = Simias.POBox.POBox.GetPOBox(store,
																   collection.Domain);
			if (poBox != null)
			{
				Member member = collection.GetCurrentMember();

				// Search for the matching subscription
				sub = poBox.GetSubscriptionByCollectionID(collection.ID,
														  member.UserID);
			}

			collection.Delete();
			collection.Commit();

			return sub;
		}




		/// <summary>
		/// WebMethod that to set the Rights of a user on a Collection
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the Collection to which
		/// the member is to be added
		/// </param>
		/// <param name = "UserID">
		/// The ID of the member to be added
		/// </param>
		/// <param name = "Rights">
		/// The Rights to be given to the newly added member
		/// Rights can be "Admin", "ReadOnly", or "ReadWrite"
		/// </param>
		/// <returns>
		/// True if the member was successfully added
		/// </returns>
		public static void SetMemberRights(	string CollectionID, 
											string UserID,
											string Rights)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col == null)
				throw new Exception("Invalid CollectionID");

			Simias.Storage.Member member = col.GetMemberByID(UserID);
			if(member == null)
				throw new Exception("Invalid UserID");

			if(Rights == "Admin")
				member.Rights = Access.Rights.Admin;
			else if(Rights == "ReadOnly")
				member.Rights = Access.Rights.ReadOnly;
			else if(Rights == "ReadWrite")
				member.Rights = Access.Rights.ReadWrite;
			else
				throw new Exception("Invalid Rights Specified");

			col.Commit(member);
		}




		/// <summary>
		/// WebMethod that sets the owner of a Collection
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the iFolder to which
		/// the member is to be added
		/// </param>
		/// <param name = "UserID">
		/// The ID of the member to be added
		/// </param>
		/// <param name = "Rights">
		/// The Rights to be given to the newly added member
		/// Rights can be "Admin", "ReadOnly", or "ReadWrite"
		/// </param>
		/// <returns>
		/// True if the member was successfully added
		/// </returns>
		public static void ChangeOwner(	string CollectionID, 
										string NewOwnerUserID,
										string OldOwnerRights)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			Simias.Storage.Member member = 
					col.GetMemberByID(NewOwnerUserID);

			if(member == null)
				throw new Exception("UserID is not a Collection Member");

			Access.Rights rights;

			if(OldOwnerRights == "Admin")
				rights = Access.Rights.Admin;
			else if(OldOwnerRights == "ReadOnly")
				rights = Access.Rights.ReadOnly;
			else if(OldOwnerRights == "ReadWrite")
				rights = Access.Rights.ReadWrite;
			else
				throw new Exception("Invalid Rights Specified");

			Node[] nodes = col.ChangeOwner(member, rights);

			col.Commit(nodes);
		}





		/// <summary>
		/// WebMethod that adds a member to a Collection granting the Rights
		/// specified.  Note:  This is not inviting a member, rather it is
		/// adding them and placing a subscription in the "ready" state in
		/// their POBox.
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the Collection to which
		/// the member is to be added
		/// </param>
		/// <param name = "UserID">
		/// The ID of the member to be added
		/// </param>
		/// <param name = "Rights">
		/// The Rights to be given to the newly added member
		/// </param>
		public static void AddMember(	string CollectionID, 
										string UserID,
										string Rights,
										string collectionType)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col == null)
				throw new Simias.NotExistException(CollectionID);

			Domain domain = store.GetDomain(col.Domain);
			if(domain == null)
				throw new Simias.NotExistException(col.Domain);

			Simias.Storage.Member member = domain.GetMemberByID(UserID);
			if(member == null)
				throw new Simias.NotExistException(UserID);

			Access.Rights newRights;

			if(Rights == "Admin")
				newRights = Access.Rights.Admin;
			else if(Rights == "ReadOnly")
				newRights = Access.Rights.ReadOnly;
			else if(Rights == "ReadWrite")
				newRights = Access.Rights.ReadWrite;
			else
				throw new Exception("Invalid Rights Specified");

			// Check to see if the user is already a member of the collection.
			Simias.Storage.Member newMember = col.GetMemberByID(member.UserID);
			if(newMember != null)
			{
				throw new Simias.ExistsException(member.UserID);
			}

			newMember = 
				new Simias.Storage.Member(	member.Name,
				member.UserID,
				newRights);

			col.Commit(newMember);

			AddSubscription( store, col, 
				newMember, newMember, SubscriptionStates.Ready,
				collectionType);
		}




		/// <summary>
		/// WebMethod that removes a member from a Collection. The subscription
		/// is also removed from the member's POBox.
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the iFolder from which
		/// the member is to be removed
		/// </param>
		/// <param name = "UserID">
		/// The ID of the member to be removed
		/// </param>
		/// <returns>
		/// True if the member was successfully removed
		/// </returns>
		public static void RemoveMember(	string CollectionID, 
											string UserID)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col == null)
				throw new Exception("Invalid CollectionID");

			// try to find a subscription with this ID
			Simias.POBox.POBox poBox = Simias.POBox.POBox.GetPOBox(store,
																   col.Domain);

			if(poBox == null)
			{
				throw new Exception("Unable to access POBox");
			}


			ICSList poList = poBox.Search(
					Subscription.ToIdentityProperty,
					UserID,
					SearchOp.Equal);

			Subscription sub = null;
			foreach(ShallowNode sNode in poList)
			{
				sub = new Subscription(poBox, sNode);
				break;
			}

			if (sub != null)
			{
				poBox.Commit(poBox.Delete(sub));
			}
			else
			{
				Simias.Storage.Member member = col.GetMemberByID(UserID);
				if(member != null)
				{
					if(member.IsOwner)
						throw new Exception("UserID is the iFolder owner");

					col.Delete(member);
					col.Commit(member);

					// even if the member is null, try to clean up the subscription
					RemoveMemberSubscription(	store, 
												col,
												UserID);
				}
			}
		}




		/// <summary>
		/// WebMethod that removes a subscription for an iFolder.
		/// </summary>
		/// <param name="DomainID">
		/// The ID of the domain that the subscription belongs to.
		/// </param>
		/// <param name="SubscriptionID">
		/// The ID of the subscription to remove.
		/// </param>
		/// <param name="UserID">
		/// The ID of the user owning the POBox where the subscription is stored.
		/// </param>
		public static void RemoveSubscription(string DomainID,
											  string SubscriptionID,
											  string UserID)
		{
			Store store = Store.GetStore();

			// Get the current member's POBox
			Simias.POBox.POBox poBox = Simias.POBox.POBox.FindPOBox(store,
																   DomainID,
																   UserID);
			if (poBox != null)
			{
				Node node = poBox.GetNodeByID(SubscriptionID);
				if (node != null)
				{
					Subscription sub = new Subscription(node);

					if(sub != null)
					{
						poBox.Commit(poBox.Delete(sub));
					}
				}
			}
		}




		/// <summary>
		/// WebMethod that calculates the number of nodes and bytes that need to be sync'd.
		/// </summary>
		/// <param name="collection">The collection to calculate the sync size.</param>
		/// <param name="nodeCount">On return, contains the number of nodes that need to be sync'd.</param>
		/// <param name="maxBytesToSend">On return, contains the number of bytes that need to be sync'd.</param>
		public static void CalculateSendSize(Collection collection,	out uint nodeCount, out ulong maxBytesToSend)
		{
			SyncSize.CalculateSendSize(collection, out nodeCount, out maxBytesToSend);
		}




		/// <summary>
		/// WebMethod that causes the collection of the specified ID to be sync'd immediately.
		/// </summary>
		/// <param name="CollectionID">The ID of the collection to sync.</param>
		public static void SyncCollectionNow(string CollectionID)
		{
			SyncClient.ScheduleSync(CollectionID);
		}




		/// <summary>
		/// Utility method that should be moved into the POBox class.
		/// This will create a subscription and place it in the POBox
		/// of the invited user.
		/// </summary>
		/// <param name = "store">
		/// The store where the POBox and collection for this subscription
		/// is to be found.
		/// </param>
		/// <param name = "collection">
		/// The Collection for which the subscription is being created
		/// </param>
		/// <param name = "inviteMember">
		/// The Member from which the subscription is being created
		/// </param>
		/// <param name = "newMember">
		/// The Member being invited
		/// </param>
		/// <param name = "state">
		/// The initial state of the subscription when placed in the POBox
		/// of the invited Member
		/// </param>
		private static void AddSubscription(	Store store, 
											Collection collection, 
											Simias.Storage.Member inviteMember,
											Simias.Storage.Member newMember,
											SubscriptionStates state,
											string collectionType)
		{
			Simias.POBox.POBox poBox = 
				Simias.POBox.POBox.GetPOBox(store, collection.Domain, 
												newMember.UserID );

			Subscription sub = poBox.CreateSubscription(collection,
														inviteMember,
														collectionType);
			sub.ToName = newMember.Name;
			sub.ToIdentity = newMember.UserID;
			sub.ToMemberNodeID = newMember.ID;
			sub.ToPublicKey = newMember.PublicKey;
			sub.SubscriptionRights = newMember.Rights;
			sub.SubscriptionState = state;
			//sub.SubscriptionCollectionURL = collection.MasterUrl.ToString();

			DirNode dirNode = collection.GetRootDirectory();
			if(dirNode != null)
			{
				sub.DirNodeID = dirNode.ID;
				sub.DirNodeName = dirNode.Name;
			}

			poBox.Commit(sub);
		}




		/// <summary>
		/// Utility method that will find all members of a collection
		/// and remove the subscription to this collection from their
		/// POBox
		/// </summary>
		/// <param name = "store">
		/// The store where the POBox and collection for this subscription
		/// is to be found.
		/// </param>
		/// <param name = "collection">
		/// The Collection for which the subscription is being removed
		/// </param>
		private static void RemoveAllSubscriptions(Store store, Collection col)
		{
			ICSList memberlist = col.GetMemberList();
			
			foreach(ShallowNode sNode in memberlist)
			{
				// Get the member from the list
				Simias.Storage.Member member =
					new Simias.Storage.Member(col, sNode);

				RemoveMemberSubscription(store, col, member.UserID);
			}
		}




		/// <summary>
		/// Utility method that removes a subscription for the specified
		/// collection from the specified UserID
		/// </summary>
		/// <param name = "store">
		/// The store where the POBox and collection for this subscription
		/// is to be found.
		/// </param>
		/// <param name = "collection">
		/// The Collection for which the subscription is being removed
		/// </param>
		/// <param name = "UserID">
		/// The UserID from which to remove the subscription
		/// </param>
		private static void RemoveMemberSubscription(	Store store, 
														Collection col,
														string UserID)
		{
			// Get the member's POBox
			Simias.POBox.POBox poBox = Simias.POBox.POBox.FindPOBox(store, 
												col.Domain, 
								UserID );
			if (poBox != null)
			{
				// Search for the matching subscription
				Subscription sub = poBox.GetSubscriptionByCollectionID(col.ID);
				if(sub != null)
				{
					poBox.Delete(sub);
					poBox.Commit(sub);
				}
			}
		}

		private static bool ExcludeDirectory(Uri path, string excludeDirectory, bool deep)
		{
			Uri excludePath = new Uri(excludeDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()) ?
								excludeDirectory :
								excludeDirectory + Path.DirectorySeparatorChar.ToString());

			if (!(path.LocalPath.Length < excludePath.LocalPath.Length) &&
				(String.Compare(deep ? path.LocalPath.Substring(0, excludePath.LocalPath.Length) : path.LocalPath, excludePath.LocalPath, true) == 0))
			{
				return true;
			}

			return false;
		}

#if WINDOWS
		private const uint DRIVE_REMOVABLE = 2;
		private const uint DRIVE_FIXED = 3;

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		private static extern uint GetDriveType(string rootPathName);
#endif

	}
}
