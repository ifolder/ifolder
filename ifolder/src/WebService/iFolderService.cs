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
using System.Web.SessionState;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.IO;
using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Simias.Web;
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
		/// WebMethod that allows a client to ping the service to see
		/// if it is up and running
		/// </summary>
		[WebMethod(Description="Allows a client to pint to make sure the Web Service is up and running")]
		[SoapDocumentMethod]
		public void Ping()
		{
			// Nothing to do here, just return
		}




		/// <summary>
		/// WebMethod that gets general iFolder Settings
		/// </summary>
		/// <returns>
		/// Settings
		/// </returns>
		[WebMethod(Description="Gets the current iFolder Settings")]
		[SoapDocumentMethod]
		public iFolderSettings GetSettings()
		{
			return new iFolderSettings();
		}




		/// <summary>
		/// WebMethod that sets the display iFolder creation confirmation setting.
		/// </summary>
		/// <param name="DisplayConfirmation">
		/// Set to <b>true</b> to enable the iFolder creation confirmation dialog.
		/// </param>
		[WebMethod(Description="Sets the display iFolder confirmation setting")]
		[SoapDocumentMethod]
		public void SetDisplayConfirmation(bool DisplayConfirmation)
		{
			iFolderSettings.SetDisplayConfirmation(DisplayConfirmation);
		}




		/// <summary>
		/// WebMethod that gets a LocalPath to see if it's an iFolder
		/// </summary>
		/// <param name = "LocalPath">
		/// The LocalPath to check for an iFolder
		/// </param>
		/// <returns>
		/// true if it is an iFolder, false if it isn't
		/// </returns>
		[WebMethod(Description="Checks a LocalPath to see if it's an iFolder")]
		[SoapDocumentMethod]
		public bool IsiFolder(string LocalPath)
		{
			Collection col = SharedCollection.GetCollectionByPath(LocalPath);
			if(col != null)
			{
				if(col.IsType(col, iFolder.iFolderType))
					return true;
			}
			return false;
		}




		/// <summary>
		/// WebMethod that checks a LocalPath to see if it can be an iFolder
		/// </summary>
		/// <param name = "LocalPath">
		/// The LocalPath to check
		/// </param>
		/// <returns>
		/// true if it can be an iFolder, otherwise false
		/// </returns>
		[WebMethod(Description="Checks LocalPath to see if can be an iFolder")]
		[SoapDocumentMethod]
		public bool CanBeiFolder(string LocalPath)
		{
			return SharedCollection.CanBeCollection(LocalPath);
		}




		/// <summary>
		/// WebMethod that checks a LocalPath to see if is in an Collection
		/// </summary>
		/// <param name = "LocalPath">
		/// The LocalPath to check
		/// </param>
		/// <returns>
		///  true if it is in an iFolder, otherwise false
		/// </returns>
		[WebMethod(Description="Checks LocalPath to see if is in an iFolder")]
		[SoapDocumentMethod]
		public bool IsPathIniFolder(string LocalPath)
		{
			return SharedCollection.IsPathInCollection(LocalPath);
		}




		/// <summary>
		/// WebMethod that creates and iFolder collection.
		/// </summary>
		/// <param name = "Path">
		/// The full path to the iFolder on the local system
		/// </param>
		/// <returns>
		/// iFolder object representing the iFolder created
		/// </returns>
		[WebMethod(Description="Create An iFolder. This will create an iFolder using the path specified.  The Path must exist or an exception will be thrown.")]
		[SoapDocumentMethod]
		public iFolder CreateLocaliFolder(string Path)
		{
			// TODO: Figure out who we are running as so we
			// can create the ifolder as the correct user
			Collection col = SharedCollection.CreateLocalSharedCollection(
								Path, iFolder.iFolderType);
			return new iFolder(col);
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
		[SoapDocumentMethod]
		public iFolder GetiFolder(string iFolderID)
		{
			Store store = Store.GetStore();
			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			return new iFolder(col);
		}




		/// <summary>
		/// WebMethod that gets an iFolder based on a LocalPath
		/// </summary>
		/// <param name = "LocalPath">
		/// The path of the iFolder to get
		/// </param>
		/// <returns>
		/// the ifolder this ID represents
		/// </returns>
		[WebMethod(Description="Get An iFolder using a LocalPath")]
		[SoapDocumentMethod]
		public iFolder GetiFolderByLocalPath(string LocalPath)
		{
			Collection col = SharedCollection.GetCollectionByPath(LocalPath);
			if(col == null)
				throw new Exception("Path not an iFolder");

			return new iFolder(col);
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
		[SoapDocumentMethod]
		public void DeleteiFolder(string iFolderID)
		{
			SharedCollection.DeleteSharedCollection(iFolderID);
		}




		/// <summary>
		/// WebMethod that returns all iFolders on the iFolder Server
		/// </summary>
		/// <returns>
		/// An array of iFolders
		/// </returns>
		[WebMethod(Description="Returns all iFolders on the Server")]
		[SoapDocumentMethod]
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


			// Now we need to get all of Subscriptions
			POBox pobox = Simias.POBox.POBox.GetPOBox(store, 
													store.DefaultDomain);
			if(pobox != null)
			{

				// Get all of the subscription obects in the POBox
				ICSList poList = pobox.Search(
						PropertyTags.Types,
						typeof(Subscription).Name,
						SearchOp.Equal);

				foreach(ShallowNode sNode in poList)
				{
					Subscription sub = new Subscription(pobox, sNode);

					// if the subscription is not for us, we don't
					// care
					if(sub.ToIdentity != pobox.Owner.UserID)
						continue;

					// Filter out all subscriptions that match
					// iFolders that are already local on our machine
					if (store.GetCollectionByID(
								sub.SubscriptionCollectionID) != null)
					{
						continue;
					}
					// CRG: this used to check for ready but the subscriptions
					// for other users were showing up
//					if( (sub.SubscriptionState == SubscriptionStates.Ready)||
//					{
//					}

					list.Add(new iFolder(sub));
				}
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
		[SoapDocumentMethod]
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
		/// <param name = "iFolderID">
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
		[WebMethod(Description="Set the Rights of a member of an iFolder.  The Rights can be \"Admin\", \"ReadOnly\", or \"ReadWrite\".")]
		[SoapDocumentMethod]
		public void SetUserRights(	string iFolderID, 
									string UserID,
									string Rights)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col != null)
				throw new Exception("Invalid iFolderID");

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
		/// WebMethod that gets the owner of an iFolder
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the collection representing the iFolder to which
		/// the member is to be added
		/// </param>
		/// <returns>
		/// Member that is the owner of the iFolder
		/// </returns>
		[WebMethod(Description="Get the Owner of an iFolder")]
		[SoapDocumentMethod]
		public iFolderUser GetOwner( string iFolderID )
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			iFolderUser user = new iFolderUser(col.Owner);
			return user;
		}




		/// <summary>
		/// WebMethod that sets the owner of an iFolder
		/// </summary>
		/// <param name = "iFolderID">
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
		[WebMethod(Description="Changes the owner of an iFolder and sets the rights of the previous owner to the rights specified.")]
		[SoapDocumentMethod]
		public void ChangeOwner(	string iFolderID, 
									string NewOwnerUserID,
									string OldOwnerRights)
		{
			SharedCollection.ChangeOwner(iFolderID, NewOwnerUserID, 
													OldOwnerRights);
		}




		/// <summary>
		/// WebMethod that removes a member from an ifolder.  The subscription
		/// is also removed from the member's POBox.
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the collection representing the iFolder from which
		/// the member is to be removed
		/// </param>
		/// <param name = "UserID">
		/// The ID of the member to be removed
		/// </param>
		/// <returns>
		/// True if the member was successfully removed
		/// </returns>
		[WebMethod(Description="Remove a single member from an iFolder")]
		[SoapDocumentMethod]
		public void RemoveiFolderUser(	string iFolderID, 
										string UserID)
		{
			SharedCollection.RemoveMember(iFolderID, UserID);
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
		[WebMethod(Description="Remove a subscription.")]
		[SoapDocumentMethod]
		public void RemoveSubscription(string DomainID,
									   string SubscriptionID)
		{
			SharedCollection.RemoveSubscription(DomainID, SubscriptionID);
		}
	



		/// <summary>
		/// WebMethod that Lists all members of a Collection
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the Collection representing the iFolder 
		/// </param>
		/// <returns>
		/// An array of Members
		/// </returns>
		[WebMethod(Description="Get the list of iFolder Members")]
		[SoapDocumentMethod]
		public iFolderUser[] GetiFolderUsers(string iFolderID)
		{
			ArrayList list = new ArrayList();

			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			ICSList memberlist = col.GetMemberList();
			foreach(ShallowNode sNode in memberlist)
			{
				Simias.Storage.Member simMem =
					new Simias.Storage.Member(col, sNode);

				iFolderUser user = new iFolderUser(simMem);
				list.Add(user);
			}

			// Use the POBox for the domain that this iFolder belongs to.
			Simias.POBox.POBox pobox = Simias.POBox.POBox.GetPOBox(
											store,
											col.Domain);

			ICSList poList = pobox.Search(
					Subscription.SubscriptionCollectionIDProperty,
					col.ID,
					SearchOp.Equal);

			foreach(ShallowNode sNode in poList)
			{
				Subscription sub = new Subscription(pobox, sNode);

				// Filter out subscriptions that are on this box
				// already
				if (sub.SubscriptionState == SubscriptionStates.Ready)
				{
					if (pobox.StoreReference.GetCollectionByID(
							sub.SubscriptionCollectionID) != null)
					{
						continue;
					}
				}

				iFolderUser user = new iFolderUser(sub);
				list.Add(user);
			}


			return (iFolderUser[]) (list.ToArray(typeof(iFolderUser)));
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
		[SoapDocumentMethod]
		public iFolderUser[] GetAlliFolderUsers()
		{
			ArrayList list = new ArrayList();

			Store store = Store.GetStore();

			Roster roster = 
				store.GetDomain(store.DefaultDomain).GetRoster(store);

			if(roster == null)
				throw new Exception("Unable to access user roster");


			ICSList memberlist = roster.GetMemberList();
			foreach(ShallowNode sNode in memberlist)
			{
				Simias.Storage.Member simMem =
					new Simias.Storage.Member(roster, sNode);

				iFolderUser user = new iFolderUser(simMem);
				list.Add(user);
			}

			return (iFolderUser[])(list.ToArray(typeof(iFolderUser)));
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
		[SoapDocumentMethod]
		public iFolderUser GetiFolderUser( string UserID )
		{
			Store store = Store.GetStore();

			Roster roster = 
					store.GetDomain(store.DefaultDomain).GetRoster(store);

			if(roster == null)
				throw new Exception("Unable to access user roster");

			Simias.Storage.Member simMem = roster.GetMemberByID(UserID);
			if(simMem == null)
				throw new Exception("Invalid UserID");

			return new iFolderUser(simMem);
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
		[SoapDocumentMethod]
		public iFolderUser GetiFolderUserFromiFolder(	string UserID, 
														string iFolderID )
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			Simias.Storage.Member simMem = col.GetMemberByID(UserID);
			if(simMem == null)
				throw new Exception("Invalid UserID");

			return new iFolderUser(simMem);
		}




		/// <summary>
		/// WebMethod that invites a user to an iFolder.
		/// This method only runs on an Enterprise iFolder
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the collection representing the Collection to which
		/// the member is to be invited
		/// </param>
		/// <param name = "UserID">
		/// The ID of the member to be invited
		/// </param>
		/// <param name = "Rights">
		/// The Rights to be given to the newly invited member
		/// </param>
		/// <returns>
		/// iFolderUser that was invited
		/// </returns>
		[WebMethod(Description="Invite a user to an iFolder.  This call will only work with Enterprise iFolders")]
		[SoapDocumentMethod]
		public iFolderUser InviteUser(	string iFolderID,
										string UserID,
										string Rights)
		{
			Store store = Store.GetStore();

			// Check to be sure we are not in Workgroup Mode
			if(store.DefaultDomain == Simias.Storage.Domain.WorkGroupDomainID)
				throw new Exception("The client default is set to Workgroup Mode.  Invitations only work in the enterprise version of ifolder.");

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			if(col.Domain == Simias.Storage.Domain.WorkGroupDomainID)
				throw new Exception("This iFolder is a Workgroup iFolder.  InviteUser will only work for an Enterprise iFolder.");

			Roster roster = 
				store.GetDomain(store.DefaultDomain).GetRoster(store);

			if(roster == null)
				throw new Exception("Unable to access ifolder users");

			Simias.Storage.Member member = roster.GetMemberByID(UserID);
			if(member == null)
				throw new Exception("Invalid UserID");

			Access.Rights newRights;

			if(Rights == "Admin")
				newRights = Access.Rights.Admin;
			else if(Rights == "ReadOnly")
				newRights = Access.Rights.ReadOnly;
			else if(Rights == "ReadWrite")
				newRights = Access.Rights.ReadWrite;
			else
				throw new Exception("Invalid Rights Specified");


			// Use the POBox for the domain that this iFolder belongs to.
			Simias.POBox.POBox poBox = Simias.POBox.POBox.GetPOBox(
											store, 
											col.Domain);

			Subscription sub = poBox.CreateSubscription(col,
										col.GetCurrentMember(),
										"iFolder");

			sub.SubscriptionRights = newRights;
			sub.ToName = member.Name;
			sub.ToIdentity = UserID;

			poBox.AddMessage(sub);

			iFolderUser user = new iFolderUser(sub);
			return user;
		}



		/// <summary>
		/// Accpets and Enterprise Subscription
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the iFolder to accept the invitation for
		/// </param>
		/// <param name = "LocalPath">
		/// The LocalPath to to store the iFolder
		/// </param>
		[WebMethod(Description="Accept an invitation fo an iFolder.  The iFolder ID represents a Subscription object")]
		[SoapDocumentMethod]
		public iFolder AcceptiFolderInvitation( string iFolderID, 
												string LocalPath)
		{
			Store store = Store.GetStore();

			// Check to be sure we are not in Workgroup Mode
			if(store.DefaultDomain == Simias.Storage.Domain.WorkGroupDomainID)
				throw new Exception("The client default is set to Workgroup Mode.  Invitations only work in the enterprise version of ifolder.");

			Simias.POBox.POBox poBox = Simias.POBox.POBox.GetPOBox(
											store, 
											store.DefaultDomain);

			// iFolders returned in the Web service are also
			// Subscriptions and it ID will be the subscription ID
			Node node = poBox.GetNodeByID(iFolderID);
			if(node == null)
				throw new Exception("Invalid iFolderID");

			if(CanBeiFolder(LocalPath) == false)
				throw new Exception("Path specified Cannot be an iFolder, it is either a parent or a child of an existing iFolder");

			Subscription sub = new Subscription(node);

			sub.CollectionRoot = Path.GetFullPath(LocalPath);
			if(sub.SubscriptionState == SubscriptionStates.Ready)
			{
				poBox.Commit(sub);
				sub.CreateSlave(store);
			}
			else
			{
				sub.Accept(store, SubscriptionDispositions.Accepted);
				poBox.Commit(sub);
			}

			iFolder ifolder = new iFolder(sub);
			return ifolder;
		}




		/// <summary>
		/// WebMethod that gets the DiskSpaceQuota for a given member
		/// </summary>
		/// <param name = "UserID">
		/// The ID of the member to get the DiskSpaceQuota
		/// </param>
		/// <returns>
		/// DiskSpaceQuota for the specified member
		/// </returns>
		[WebMethod(Description="Gets the DiskSpaceQuota for a member")]
		[SoapDocumentMethod]
		public DiskSpace GetUserDiskSpace( string UserID )
		{
			return DiskSpace.GetMemberDiskSpace(UserID);
		}




		/// <summary>
		/// WebMethod that gets the DiskSpaceQuota for a given iFolder
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the iFolder to get the DiskSpaceQuota
		/// </param>
		/// <returns>
		/// DiskSpaceQuota for the specified iFolder
		/// </returns>
		[WebMethod(Description="Gets the DiskSpaceQuota for an iFolder")]
		[SoapDocumentMethod]
		public DiskSpace GetiFolderDiskSpace( string iFolderID )
		{
			return DiskSpace.GetiFolderDiskSpace(iFolderID);
		}




		/// <summary>
		/// WebMethod that sets the disk space limit for a member
		/// </summary>
		/// <param name = "UserID">
		/// The ID of the member to set the disk space limit
		/// </param>
		/// <param name = "Limit">
		/// The size to set in MegaBytes
		/// </param>
		[WebMethod(Description="Sets the Disk Space Limit for a user")]
		[SoapDocumentMethod]
		public void SetUserDiskSpaceLimit( string UserID, long Limit )
		{
			DiskSpace.SetUserDiskSpaceLimit(UserID, Limit);
		}




		/// <summary>
		/// WebMethod that sets the disk space limit for an iFolder.
		/// </summary>
		/// <param name="iFolderID">
		/// The ID of the iFolder to set the disk space limit on.
		/// </param>
		/// <param name="Limit">
		/// The size to set in megabytes.
		/// </param>
		[WebMethod(Description="Sets the Disk Space Limit for an iFolder")]
		[SoapDocumentMethod]
		public void SetiFolderDiskSpaceLimit( string iFolderID, long Limit )
		{
			DiskSpace.SetiFolderDiskSpaceLimit(iFolderID, Limit);
		}




		/// <summary>
		/// WebMethod that sets an iFolders SyncInterval
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the iFolder to set the syncinterval
		/// </param>
		/// <param name = "Interval">
		/// The interval to set in seconds
		/// </param>
		[WebMethod(Description="Sets the Sync Interval for an iFolder")]
		[SoapDocumentMethod]
		public void SetiFolderSyncInterval( string iFolderID, int Interval )
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			Simias.Policy.SyncInterval.Set(col, Interval);
		}




		/// <summary>
		/// WebMethod that sets the default SyncInterval
		/// </summary>
		/// <param name = "Interval">
		/// The interval to set in seconds
		/// </param>
		[WebMethod(Description="Sets the Default Sync Interval")]
		[SoapDocumentMethod]
		public void SetDefaultSyncInterval( int Interval )
		{
			Simias.Policy.SyncInterval.Set( Interval );
		}




		/// <summary>
		/// WebMethod that gets the default SyncInterval
		/// </summary>
		/// <returns>
		/// The default sync interval
		/// </returns>
		[WebMethod(Description="Gets the Default Sync Interval")]
		[SoapDocumentMethod]
		public int GetDefaultSyncInterval()
		{
			return Simias.Policy.SyncInterval.GetInterval();
		}




		/// <summary>
		/// WebMethod that connects up an iFolder Enterprise Server
		/// </summary>
		/// <param name = "UserName">
		/// The username to use to connect to the Enterprise server
		/// </param>
		/// <param name = "Password">
		/// The password to use to connect to the Enterprise server
		/// </param>
		/// <param name = "Host">
		/// The host of the enterprise server
		/// </param>
		/// <returns>
		/// The current Settings
		/// </returns>
		[WebMethod(Description="Connects to an iFolder Enterprise Server")]
		[SoapDocumentMethod]
		public iFolderSettings ConnectToEnterpriseServer(	string UserName,
															string Password,
															string Host)
		{
			Configuration conf = Configuration.GetConfiguration();
			Simias.Domain.DomainAgent da = new Simias.Domain.DomainAgent(conf);
			da.Attach(Host, UserName, Password);
			return new iFolderSettings();
		}




		/// <summary>
		/// WebMethod that retuns all of an ifolder's Conflicts
		/// </summary>
		/// <param name = "iFolderID">
		/// The iFolder ID to return the conflicts
		/// </param>
		/// <returns>
		/// An Array of conflicts
		/// </returns>
		[WebMethod(Description="Connects to an iFolder Enterprise Server")]
		[SoapDocumentMethod]
		public Conflict[] GetiFolderConflicts(	string iFolderID )
		{
			ArrayList list = new ArrayList();

			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			ICSList collisionList = col.GetCollisions();
			foreach(ShallowNode sn in collisionList)
			{
				Node conflictNode = new Node(col, sn);

				Conflict conflict = new Conflict(col, conflictNode);

				list.Add(conflict);
			}
			return (Conflict[])list.ToArray(typeof(Conflict));
		}




		/// <summary>
		/// WebMethod that resolves a conflict
		/// </summary>
		/// <param name = "iFolderID">
		/// The iFolder ID to return the conflicts
		/// </param>
		/// <param name = "conflictID">
		/// The node ID that represents a conflict to resolve
		/// </param>
		/// <param name = "iFolderID">
		/// A bool that determines if the local or server copies win
		/// </param>
		[WebMethod(Description="Resolves a file conflict in an iFolder.")]
		[SoapDocumentMethod]
		public void ResolveFileConflict(string iFolderID, string conflictID,
										bool localChangesWin)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			Node conflictNode = col.GetNodeByID(conflictID);
			if(conflictNode == null)
				throw new Exception("Invalid conflictID");

			Conflict.Resolve(col, conflictNode, localChangesWin);
		}




		/// <summary>
		/// WebMethod that resolves a name conflict
		/// </summary>
		/// <param name = "iFolderID">
		/// The iFolder ID to return the conflicts
		/// </param>
		/// <param name = "conflictID">
		/// The node ID that represents a conflict to resolve
		/// </param>
		/// <param name = "iFolderID">
		/// A bool that determines if the local or server copies win
		/// </param>
		[WebMethod(Description="Resolves a name conflict")]
		[SoapDocumentMethod]
		public void ResolveNameConflict(string iFolderID, string conflictID,
										string newLocalName)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			Node conflictNode = col.GetNodeByID(conflictID);
			if(conflictNode == null)
				throw new Exception("Invalid conflictID");

			Conflict.Resolve(col, conflictNode, newLocalName);
		}




		/// <summary>
		/// WebMethod that will setup the Proxy
		/// </summary>
		/// <param name = "Host">
		/// The host of the proxy
		/// </param>
		/// <param name = "Port">
		/// The Port on the host to use
		/// </param>
		[WebMethod(Description="Sets up a proxy for iFolder to use")]
		[SoapDocumentMethod]
		public void SetupProxy(string Host, int Port)
		{
			// I'm not sure what to do here
		}




		/// <summary>
		/// WebMethod that will setup the Proxy
		/// </summary>
		[WebMethod(Description="Removes proxy settings")]
		[SoapDocumentMethod]
		public void RemoveProxy()
		{
			// I'm not sure what to do here
		}



	}
}
