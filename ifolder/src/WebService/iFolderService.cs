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
		/// WebMethod that creates and iFolder collection.
		/// </summary>
		/// <param name = "Path">
		/// The full path to the iFolder on the local system
		/// </param>
		/// <returns>
		/// iFolder object representing the iFolder created
		/// </returns>
		[WebMethod(Description="Create An iFolder. This will create an iFolder using the path specified.  The Path must exist or an exception will be thrown.")]
		[SoapRpcMethod]
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
		[SoapRpcMethod]
		public iFolder GetiFolder(string iFolderID)
		{
			Store store = Store.GetStore();
			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

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
		[SoapRpcMethod]
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
		[SoapRpcMethod]
		public void SetMemberRights(	string iFolderID, 
										string UserID,
										string Rights)
		{
			SharedCollection.SetMemberRights(iFolderID, UserID, Rights);
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
		[SoapRpcMethod]
		public Simias.Web.Member GetOwner( string iFolderID )
		{
			return SharedCollection.GetOwner(iFolderID);
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
		[SoapRpcMethod]
		public void ChangeOwner(	string iFolderID, 
									string NewOwnerUserID,
									string OldOwnerRights)
		{
			SharedCollection.ChangeOwner(iFolderID, NewOwnerUserID, 
														OldOwnerRights);
		}




		/// <summary>
		/// WebMethod that adds a member to an ifolder granting the Rights
		/// specified.  Note:  This is not inviting a member, rather it is
		/// adding them and placing a subscription in the "ready" state in
		/// their POBox.
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
		[WebMethod(Description="Add a single member to an iFolder")]
		[SoapRpcMethod]
		public void AddMember(	string iFolderID, 
								string UserID,
								string Rights)
		{
			SharedCollection.AddMember(iFolderID, UserID, Rights);
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
		[SoapRpcMethod]
		public void RemoveMember(	string iFolderID, 
									string UserID)
		{
			SharedCollection.RemoveMember(iFolderID, UserID);
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
		[SoapRpcMethod]
		public Simias.Web.Member[] GetMembers(string iFolderID)
		{
			return SharedCollection.GetMembers(iFolderID);
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
		public Simias.Web.Member[] GetAllMembers()
		{
			return SharedCollection.GetAllMembers();
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
		public Simias.Web.Member GetMember( string UserID )
		{
			return SharedCollection.GetMember(UserID);
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
		public Simias.Web.Member GetiFolderMember( string UserID, 
													string iFolderID )
		{
			return SharedCollection.GetCollectionMember(UserID, iFolderID);
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
		[SoapRpcMethod]
		public Simias.Web.DiskSpaceQuota GetMemberDiskSpaceQuota( string UserID )
		{
			return SharedCollection.GetMemberDiskQuota( UserID );
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
		[SoapRpcMethod]
		public Simias.Web.DiskSpaceQuota GetiFolderDiskSpaceQuota( string iFolderID )
		{
			return SharedCollection.GetCollectionDiskQuota( iFolderID );
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
		[SoapRpcMethod]
		public void SetMemberSpaceLimit( string UserID, long Limit )
		{
			SharedCollection.SetMemberSpaceLimit(UserID, Limit);
		}




		/// <summary>
		/// WebMethod that sets the disk space limit for an iFolder 
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the iFolder to set the disk space limit
		/// </param>
		/// <param name = "Limit">
		/// The size to set in MegaBytes
		/// </param>
		[WebMethod(Description="Sets the Disk Space Limit for an iFolder")]
		[SoapRpcMethod]
		public void SetiFolderSpaceLimit( string iFolderID, long Limit )
		{
			SharedCollection.SetCollectionSpaceLimit(iFolderID, Limit);
		}

	}
}
