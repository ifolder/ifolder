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
 *          Rob Lyon <rlyon@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.IO;
using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using System.Xml;
using System.Xml.Serialization;

namespace Novell.iFolder.Web
{
	/// <summary>
	/// This is the core of the iFolderServce.  All of the methods in the
	/// web service are implemented here.
	/// </summary>
	[WebService(
	Namespace="http://novell.com/ifolder/web/",
	Name="iFolder Web Service",
	Description="Web Service providing access to iFolder")]
	public class iFolderService : WebService
	{

		/// <summary>
		/// Creates the iFolderService and sets up logging
		/// </summary>
		public iFolderService()
		{
		}




		/// <summary>
		/// WebMethod that creates and iFolder collection.
		/// </summary>
		/// <param name = "iFolderPath">
		/// The path to the ifolder
		/// </param>
		/// <returns>
		/// iFolder object representing the Collection created
		/// </returns>
		[WebMethod(Description="Create An iFolder.")]
		[SoapRpcMethod]
		public iFolder CreateiFolder(string iFolderPath)
		{
			ArrayList nodeList = new ArrayList();

			Store store = Store.GetStore();

			string name = Path.GetFileName(iFolderPath);

			// Create the Collection and set it as an iFolder
			Collection c = 
					new Collection(store, name, store.DefaultDomain);
			c.SetType(c, iFolder.iFolderType);
			nodeList.Add(c);

			// Create the member and add it as the owner
			Roster roster = 
					store.GetDomain(store.DefaultDomain).GetRoster(store);

			if(roster == null)
			{
				throw new Exception("Unable to obtain default Roster");
			}

			Simias.Storage.Member member = roster.Owner;
			if(member == null)
			{
				throw new Exception("UserID is invalid");
			}
				
			Simias.Storage.Member newMember = 
					new Simias.Storage.Member(	member.Name,
												member.UserID,
												Access.Rights.Admin);
			newMember.IsOwner = true;
			nodeList.Add(newMember);

			// create root directory node
			DirNode dn = new DirNode(c, iFolderPath);
			nodeList.Add(dn);

			if(!Directory.Exists(iFolderPath) )
				throw new Exception("Path did not exist");

			// Commit the new collection and the fileNode at the root
			c.Commit(nodeList.ToArray( typeof( Node) ) as Node[] );

			AddSubscription( store, c, member, 
					member, SubscriptionStates.Ready);

			return new iFolder(c);
		}




		/// <summary>
		/// WebMethod that gets an iFolder based on an iFolderID
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the collection representing this iFolder to get
		/// </param>
		/// <returns>
		/// the ifolder this ID represents
		/// </returns>
		[WebMethod(Description="Get An iFolder")]
		[SoapRpcMethod]
		public iFolder GetiFolder(string iFolderID)
		{
			Store store = Store.GetStore();
			Collection col = store.GetCollectionByID(iFolderID);
			if(col != null)
			{
				return new iFolder(col);
			}
			else
				throw new Exception("Invalid iFolderID");
		}




		/// <summary>
		/// WebMethod that deletes an iFolder and removes all subscriptions
		/// from all members.  The files will remain in place using this
		/// method.
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the collection representing this iFolder to delete
		/// </param>
		/// <returns>
		/// true if the iFolder was successfully removed
		/// </returns>
		[WebMethod(Description="Delete An iFolder")]
		[SoapRpcMethod]
		public bool DeleteiFolder(string iFolderID)
		{
			try
			{
				Store store = Store.GetStore();
				Collection collection = store.GetCollectionByID(iFolderID);
				if(collection != null)
				{
					RemoveAllSubscriptions(store, collection);
					collection.Delete();
					collection.Commit();
					return true;
				}
			}
			catch{}

			return false;
		}




		/// <summary>
		/// WebMethod that returns all iFolders on the iFolder Server
		/// </summary>
		/// <returns>
		/// An array of iFolders
		/// </returns>
		[WebMethod(Description="Returns all iFolders on the Server")]
		[SoapRpcMethod]
		public iFolder[] GetAlliFolders()
		{
			ArrayList list = new ArrayList();

			Store store = Store.GetStore();

			ICSList iFolderList = 
					store.GetCollectionsByType(iFolder.iFolderType);

			foreach(ShallowNode sn in iFolderList)
			{
				Collection col = store.GetCollectionByID(sn.ID);
				list.Add(new iFolder(col));
			}

			return (iFolder[])list.ToArray(typeof(iFolder));
		}




		/// <summary>
		/// WebMethod that returns all iFolders on the iFolder Server
		/// for the specified UserID
		/// </summary>
		/// <param name = "UserID">
		/// This is the UserID who's iFolders are being gotten
		/// </param>
		/// <returns>
		/// An array of iFolders
		/// </returns>
		[WebMethod(Description="Returns iFolders for the specified UserID")]
		[SoapRpcMethod]
		public iFolder[] GetiFolders(string UserID)
		{
			ArrayList list = new ArrayList();

			Store store = Store.GetStore();


			ICSList iFolderList = 
					store.GetCollectionsByUser(UserID);

			foreach(ShallowNode sn in iFolderList)
			{
				Collection col = store.GetCollectionByID(sn.ID);
				if(col.IsType(col, iFolder.iFolderType))
					list.Add(new iFolder(col));
			}

			return (iFolder[])list.ToArray(typeof(iFolder));
		}




		/// <summary>
		/// WebMethod that to set the Rights of a user on an iFolder
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
		/// </param>
		/// <returns>
		/// True if the member was successfully added
		/// </returns>
		[WebMethod(Description="Set the Rights of a member of a collection")]
		[SoapRpcMethod]
		public bool SetMemberRights(	string CollectionID, 
										string UserID,
										string Rights)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col != null)
			{
				Simias.Storage.Member member = col.GetMemberByID(UserID);
				if(member != null)
				{
					if(Rights == "Admin")
						member.Rights = Access.Rights.Admin;
					else if(Rights == "ReadOnly")
						member.Rights = Access.Rights.ReadOnly;
					else if(Rights == "ReadWrite")
						member.Rights = Access.Rights.ReadWrite;
					else
						throw new ApplicationException("Invalid Rights");

					col.Commit(member);

					return true;
				}
			}
			return false;
		}




		/// <summary>
		/// WebMethod that gets the owner of a Collection
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the iFolder to which
		/// the member is to be added
		/// </param>
		/// <returns>
		/// Member that is the owner of the Collection
		/// </returns>
		[WebMethod(Description="Get the Owner of a Collection")]
		[SoapRpcMethod]
		public Member GetOwner( string CollectionID )
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col != null)
			{
				Member member = new Member(col.Owner);
				return member;
			}
			throw new Exception("Invalid Collection ID");
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
		/// </param>
		/// <returns>
		/// True if the member was successfully added
		/// </returns>
		[WebMethod(Description="Set the Rights of a member of a collection")]
		[SoapRpcMethod]
		public bool ChangeOwner(	string CollectionID, 
									string NewOwnerUserID,
									string OldOwnerRights)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col != null)
			{
				Simias.Storage.Member member = 
						col.GetMemberByID(NewOwnerUserID);

				if(member != null)
				{
					Access.Rights rights;

					if(OldOwnerRights == "Admin")
						rights = Access.Rights.Admin;
					else if(OldOwnerRights == "ReadOnly")
						rights = Access.Rights.ReadOnly;
					else if(OldOwnerRights == "ReadWrite")
						rights = Access.Rights.ReadWrite;
					else
						throw new Exception("Invalid Rights");

					Node[] nodes = col.ChangeOwner(member, rights);

					col.Commit(nodes);

					return true;
				}
				else
					throw new Exception("UserID is not a member of Collection");
			}
			else
				throw new Exception("Invalid Collection specified");
		}




		/// <summary>
		/// WebMethod that adds a member to an ifolder granting the Rights
		/// specified.  Note:  This is not inviting a member, rather it is
		/// adding them and placing a subscription in the "ready" state in
		/// their POBox.
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
		/// </param>
		/// <returns>
		/// True if the member was successfully added
		/// </returns>
		[WebMethod(Description="Add a single member to a collection")]
		[SoapRpcMethod]
		public bool AddMember(	string CollectionID, 
								string UserID,
								string Rights)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col != null)
			{
				Roster roster = 
					store.GetDomain(store.DefaultDomain).GetRoster(store);

				if(roster != null)
				{
					Simias.Storage.Member member = roster.GetMemberByID(UserID);
					if(member != null)
					{
						Access.Rights newRights;

						if(Rights == "Admin")
							newRights = Access.Rights.Admin;
						else if(Rights == "ReadOnly")
							newRights = Access.Rights.ReadOnly;
						else if(Rights == "ReadWrite")
							newRights = Access.Rights.ReadWrite;
						else
							throw new ApplicationException("Invalid Rights");

						Simias.Storage.Member newMember = 
							new Simias.Storage.Member(	member.Name,
														member.UserID,
														newRights);
						col.Commit(newMember);

						AddSubscription( store, col, 
								newMember, newMember, SubscriptionStates.Ready);
						return true;
					}
				}
			}
			return false;
		}




		/// <summary>
		/// WebMethod that removes a member from an ifolder.  The subscription
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
		[WebMethod(Description="Remove a single member from a collection")]
		[SoapRpcMethod]
		public bool RemoveMember(	string CollectionID, 
									string UserID)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col != null)
			{
				Simias.Storage.Member member = col.GetMemberByID(UserID);
				if(member != null)
				{
					if(member.IsOwner)
						return false;

					col.Delete(member);
					col.Commit(member);
				}

				// even if the member is null, try to clean up the subscription
				RemoveMemberSubscription(	store, 
											col,
											UserID);
				return true;
			}
			return false;
		}
	



		/// <summary>
		/// WebMethod that Lists all members of a Collection
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the Collection representing the iFolder 
		/// </param>
		/// <returns>
		/// An array of Members
		/// </returns>
		[WebMethod(Description="Get the list of Collection Members")]
		[SoapRpcMethod]
		public Member[] GetMembers(string CollectionID)
		{
			ArrayList list = new ArrayList();

			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col != null)
			{
				ICSList memberlist = col.GetMemberList();
				foreach(ShallowNode sNode in memberlist)
				{
					Simias.Storage.Member simMem =
						new Simias.Storage.Member(col, sNode);

					Member member = new Member(simMem);
					list.Add(member);
				}
			}
			return (Member[])(list.ToArray(typeof(Member)));
		}




		/// <summary>
		/// WebMethod that returns a list of all members from the default
		/// domain of the iFolder Enterprise Server.  This list represents
		/// the "Roster" of the default domain.
		/// </summary>
		/// <returns>
		/// An array of members
		/// </returns>
		[WebMethod(Description="Get the list of All Members")]
		[SoapRpcMethod]
		public Member[] GetAllMembers()
		{
			ArrayList list = new ArrayList();

			Store store = Store.GetStore();

			Roster roster = 
				store.GetDomain(store.DefaultDomain).GetRoster(store);

			if(roster != null)
			{
				ICSList memberlist = roster.GetMemberList();
				foreach(ShallowNode sNode in memberlist)
				{
					Simias.Storage.Member simMem =
						new Simias.Storage.Member(roster, sNode);

					Member member = new Member(simMem);
					list.Add(member);
				}
			}

			return (Member[])(list.ToArray(typeof(Member)));
		}




		/// <summary>
		/// WebMethod that gets a member from the default Roster
		/// </summary>
		/// <param name = "UserID">
		/// The ID of the member to be added
		/// </param>
		/// <returns>
		/// Member that matches the UserID
		/// </returns>
		[WebMethod(Description="Lookup a single member to a collection")]
		[SoapRpcMethod]
		public Member GetMember( string UserID )
		{
			Store store = Store.GetStore();

			Roster roster = 
					store.GetDomain(store.DefaultDomain).GetRoster(store);

			if(roster != null)
			{
				Simias.Storage.Member simMem = roster.GetMemberByID(UserID);
				if(simMem != null)
				{
					return new Member(simMem);
				}
				else
					throw new Exception("Invalid UserID");
			}
			else
				throw new Exception("Unable to access user roster");
		}




		/// <summary>
		/// WebMethod that gets a member from the specified collection
		/// </summary>
		/// <param name = "UserID">
		/// The ID of the member to get
		/// </param>
		/// <returns>
		/// Member that matches the UserID
		/// </returns>
		[WebMethod(Description="Lookup a single member to a collection")]
		[SoapRpcMethod]
		public Member GetiFolderMember( string UserID, string iFolderID )
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col != null)
			{
				Simias.Storage.Member simMem = col.GetMemberByID(UserID);
				if(simMem != null)
				{
					return new Member(simMem);
				}
				else
					throw new Exception("Invalid UserID");
			}
			else
				throw new Exception("Invalid iFolderID");
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
		private void AddSubscription(	Store store, 
										Collection collection, 
										Simias.Storage.Member inviteMember,
										Simias.Storage.Member newMember,
										SubscriptionStates state)
		{
			POBox poBox = 
				POBox.GetPOBox(store, store.DefaultDomain, newMember.UserID );

			Subscription sub = poBox.CreateSubscription(collection,
														inviteMember,
														iFolder.iFolderType);
			sub.ToName = newMember.Name;
			sub.ToIdentity = newMember.UserID;
			sub.ToPublicKey = newMember.PublicKey;
			sub.SubscriptionRights = newMember.Rights;
			sub.SubscriptionState = state;

			// copied from the iFolder code, this may need to change
			// in the future
			Roster roster = 
				store.GetDomain(store.DefaultDomain).GetRoster(store);
			SyncCollection sc = new SyncCollection(roster);
			sub.SubscriptionCollectionURL = sc.MasterUrl.ToString();

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
		private void RemoveAllSubscriptions(Store store, Collection col)
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
		private void RemoveMemberSubscription(	Store store, 
												Collection col,
												string UserID)
		{
			// Get the member's POBox
			POBox poBox = POBox.GetPOBox(store, store.DefaultDomain, 
								UserID );

			// Search for the matching subscription
			Subscription sub = poBox.GetSubscriptionByCollectionID(col.ID);
			if(sub != null)
			{
				poBox.Delete(sub);
				poBox.Commit(sub);
			}
		}

	}
}
