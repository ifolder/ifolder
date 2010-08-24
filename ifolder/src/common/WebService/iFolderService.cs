/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Calvin Gaisford <cgaisford@novell.com> ,Rob Lyon <rlyon@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

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
using System.Net;
using Simias;
using Simias.Client;
using Simias.Authentication;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Simias.Discovery;
using Simias.Policy;
using Simias.Web;
using System.Xml;
using System.Xml.Serialization;
//using Novell.AddressBook;

//using Novell.Security.ClientPasswordManager;

namespace Novell.iFolder.Web
{
	/// <summary>
	/// Supported store search operators.
	/// </summary>
	public enum iFolderSearchType
	{
		/// <summary>
		/// Used to compare if two values are equal.
		/// </summary>
		Equal,

		/// <summary>
		/// Used to compare if two values are not equal.
		/// </summary>
		Not_Equal,

		/// <summary>
		/// Used to compare if a string value begins with a sub-string value.
		/// </summary>
		Begins,

		/// <summary>
		/// Used to compare if a string value ends with a sub-string value.
		/// </summary>
		Ends,

		/// <summary>
		/// Used to compare if a string value contains a sub-string value.
		/// </summary>
		Contains,

		/// <summary>
		/// Used to compare if a value is greater than another value.
		/// </summary>
		Greater,

		/// <summary>
		/// Used to compare if a value is less than another value.
		/// </summary>
		Less,

		/// <summary>
		/// Used to compare if a value is greater than or equal to another value.
		/// </summary>
		Greater_Equal,

		/// <summary>
		/// Used to compare if a value is less than or equal to another value.
		/// </summary>
		Less_Equal,

		/// <summary>
		/// Used to test for existence of a property.
		/// </summary>
		Exists,

		/// <summary>
		/// Used to do a case sensitive compare.
		/// </summary>
		CaseEqual
	};

	enum SecurityState
	{
		encrypt = 1,
		enforceEncrypt = 2,
		encryptionState = 3,
		SSL = 4,
		enforceSSL = 8,
		SSLState = 12,
		UserEncrypt = 16,
		UserSSL = 32
	}

	/// <summary>
	/// enum value to denote different combinations of Disabling options
	/// <summary>
	public enum Share
	{	
		Sharing = 1,
		EnforcedSharing = 4,
		DisableSharing = 8
	}

	/// <summary>
	/// This is the core of the iFolderServce.  All of the methods in the
	/// web service are implemented here.
	/// </summary>
	[WebService(
	Namespace="http://novell.com/ifolder/web/",
	Name="iFolderWebService",
	Description="Web Service providing access to iFolder")]
	public class iFolderService : WebService
	{
		/// <summary>
		/// Creates the iFolderService and sets up logging
		/// </summary>
		public iFolderService()
		{
		}


		internal class UserComparer : IComparer  
		{
			int IComparer.Compare( Object x, Object y )  
			{
				iFolderUser memberX = x as iFolderUser;
				iFolderUser memberY = y as iFolderUser;

				if ( memberX.FN != null )
				{
					if (memberY.FN != null)
					{
						return (new CaseInsensitiveComparer()).Compare( memberX.FN, memberY.FN );
					}

					return (new CaseInsensitiveComparer()).Compare( memberX.FN, memberY.Name );
				}
				else
				if ( memberY.FN != null )
				{
					return ( new CaseInsensitiveComparer()).Compare( memberX.Name, memberY.FN );
				}

				return ( new CaseInsensitiveComparer()).Compare( memberX.Name, memberY.Name );
			}
		}


		/// <summary>
		/// WebMethod that allows a client to ping the service to see
		/// if it is up and running
		/// </summary>
		[WebMethod(EnableSession=true, Description="Allows a client to ping to make sure the Web Service is up and running")]
		[SoapDocumentMethod]
		public void Ping()
		{
			// Nothing to do here, just return
		}



/*
		/// <summary>
		/// WebMethod that gets general iFolder Settings
		/// </summary>
		/// <returns>
		/// Settings
		/// </returns>
		[WebMethod(EnableSession=true, Description="Gets the current iFolder Settings")]
		[SoapDocumentMethod]
		public iFolderSettings GetSettings()
		{
			return new iFolderSettings();
		}




		/// <summary>
		/// WebMethod that sets the display iFolder creation confirmation 
		/// setting.
		/// </summary>
		/// <param name="DisplayConfirmation">
		/// Set to <b>true</b> to enable the iFolder creation confirmation 
		/// dialog.
		/// </param>
		[WebMethod(EnableSession=true, Description="Sets the display iFolder confirmation setting")]
		[SoapDocumentMethod]
		public void SetDisplayConfirmation(bool DisplayConfirmation)
		{
			iFolderSettings.SetDisplayConfirmation(DisplayConfirmation);
		}
*/



		/// <summary>
		/// WebMethod that gets a LocalPath to see if it's an iFolder
		/// </summary>
		/// <param name = "LocalPath">
		/// The LocalPath to check for an iFolder
		/// </param>
		/// <returns>
		/// true if it is an iFolder, false if it isn't
		/// </returns>
		[WebMethod(EnableSession=true, Description="Checks a LocalPath to see if it's an iFolder")]
		[SoapDocumentMethod]
		public bool IsiFolder(string LocalPath)
		{
			Collection col = SharedCollection.GetCollectionByPath(LocalPath);
			if(col != null)
			{
				if(col.IsType(col, iFolderWeb.iFolderType))
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
		[WebMethod(EnableSession=true, Description="Checks LocalPath to see if can be an iFolder")]
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
		[WebMethod(EnableSession=true, Description="Checks LocalPath to see if is in an iFolder")]
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
		[WebMethod(EnableSession=true, Description="Create An iFolder. This will create an iFolder using the path specified.  The Path must exist or an exception will be thrown.")]
		[SoapDocumentMethod]
		public iFolderWeb CreateLocaliFolder(string Path)
		{
			// TODO: Figure out who we are running as so we
			// can create the ifolder as the correct user
			Collection col = SharedCollection.CreateLocalSharedCollection(
								Path, iFolderWeb.iFolderType);
			return new iFolderWeb(col);
		}




		/// <summary>
		/// WebMethod that creates an iFolder collection.
		/// </summary>
		/// <param name = "Path">
		/// The full path to the iFolder on the local system
		/// </param>
		/// <param name="DomainID">The ID of the domain to create the iFolder in.</param>
		/// <returns>
		/// iFolder object representing the iFolder created
		/// </returns>
		[WebMethod(EnableSession=true, Description="Create an iFolder. This will create an iFolder using the path specified with the security Level desired. The Path must exist or an exception will be thrown.")]
		[SoapDocumentMethod]
		public iFolderWeb CreateiFolderInDomainEncr(string Path, string DomainID, bool SSL, string EncryptionAlgorithm, string Passphrase)
		{
			try
			{
                //If Path ends with char "/" or "\", remvoing all such occurence from end of Path
                Path = Path.TrimEnd(new char[] { '\\' });
                Path = Path.TrimEnd(new char[] { '/' });

				Collection col = SharedCollection.CreateLocalSharedCollection( Path, DomainID, SSL, iFolderWeb.iFolderType, EncryptionAlgorithm, Passphrase);

				return new iFolderWeb(col);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				throw e;
			}
		}



//	added now..

		[WebMethod(EnableSession=true, Description="Create an iFolder. This will create an iFolder using the path specified.  The Path must exist or an exception will be thrown.")]
		[SoapDocumentMethod]
		public iFolderWeb CreateiFolderInDomain(string Path, string DomainID)
		{
			try
			{
			Collection col = SharedCollection.CreateLocalSharedCollection(
								Path, DomainID, iFolderWeb.iFolderType);
			return new iFolderWeb(col);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				throw e;
			}
		}


		/// <summary>
		/// WebMethod that checks the presence of an iFolder based on an iFolderID
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the collection representing this iFolder to check
		/// </param>
		/// <returns>
		/// the ifolder this ID represents
		/// </returns>
		[WebMethod(EnableSession=true, Description="Checks the presence of an iFolder")]
		[SoapDocumentMethod]
		public bool CheckiFolder(string iFolderID)
		{
			Store store = Store.GetStore();
			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
			{
				CollectionInfo ci = DiscoveryFramework.GetLocalCollectionInfo(iFolderID);
				//if there is an ID then there must be a collection
                if (ci == null)
                {
                    ci = DiscoveryFramework.GetCollectionInfo(iFolderID);
                    if (ci == null) //this must never be a case
                        return false;
                    else
                        return true;
                }
                else
                    return true;
			}
			else
			{
				if(col.IsType(col, iFolderWeb.iFolderType))
					return true;
				else
					return false;
			}
			
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
		[WebMethod(EnableSession=true, Description="Get An iFolder")]
		[SoapDocumentMethod]
		public iFolderWeb GetiFolder(string iFolderID)
		{
			iFolderWeb ifolder = null;
			
			Store store = Store.GetStore();
			Collection col = store.GetCollectionByID(iFolderID);
			if (col != null)
			{
				// CRG: Removed the code that checked for a Subscription
				// USE: GetiFolderInvitation to be safe in multi-domain
				if(col.IsType(col, iFolderWeb.iFolderType))
					ifolder = new iFolderWeb(col);
				else
					ifolder = null;
			}
			else
			{
				CollectionInfo ci = DiscoveryFramework.GetLocalCollectionInfo(iFolderID);
                if(ci == null)
                    ci = DiscoveryFramework.GetCollectionInfo(iFolderID);
				ifolder = new iFolderWeb(ci);			
			}

			return ifolder;
		}

		/// <summary>
		/// WebMethod that gets an iFolder based on an iFolderID
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the collection representing this iFolder to get
		/// </param>
		/// <param name = "infoToFetch">
		/// a bitmap that states which info must be included in the web object
		/// </param>
		/// <returns>
		/// the ifolder this ID represents
		/// </returns>
		[WebMethod(EnableSession=true, Description="Get An minimal iFolder")]
		[SoapDocumentMethod]
		public iFolderWeb GetMinimaliFolder(string iFolderID, int infoToFetch)
		{
			if(infoToFetch == 0)
				return GetiFolder(iFolderID);
			
			iFolderWeb ifolder = null;
			
			Store store = Store.GetStore();
			Collection col = store.GetCollectionByID(iFolderID);
			if (col != null)
			{
				// CRG: Removed the code that checked for a Subscription
				// USE: GetiFolderInvitation to be safe in multi-domain
				if(col.IsType(col, iFolderWeb.iFolderType))
					ifolder = new iFolderWeb(col, infoToFetch);
				else
					ifolder = null;
			}
			else
			{
				CollectionInfo ci = DiscoveryFramework.GetLocalCollectionInfo(iFolderID);
				if(ci == null)
					ci = DiscoveryFramework.GetCollectionInfo(iFolderID);
				
				ifolder = new iFolderWeb(ci);			
			}

			return ifolder;
		}


		/*
		/// Not used
		[WebMethod(EnableSession=true, Description="This method is for setting security policy from the thick client.")]
		[SoapDocumentMethod]
		public int SetSecurityPolicy(string DomainID, int status)
		{
			
                        Simias.Storage.Store store = Simias.Storage.Store.GetStore();
	                Simias.Storage.Domain domain = store.GetDomain(DomainID);
			Simias.Storage.Member member = domain.GetCurrentMember();
			// Can make the changes to read and modify the status
			Simias.Policy.SecurityState.Create(member, status);
			return 0;
		}
		*/

		/// This method is for finding the user security status depending upon the system and user policies
		/// This is not a webmethos and is not exposed.
		private int DeriveStatus(int system, int group, int user, int preference)
		{
			if( preference != 0)	// server wins
			{
				if(system != 0)
					return system;
				else if(group != 0)
					return group;
				return user;
			}
			else			// user wins
			{
				if(user != 0)
					return user;
				else if(group != 0)
					return group; 
				return system;
			}
		}

		[WebMethod(EnableSession=true, Description="This method is for getting security policy from the Collectionstore.")]
		[SoapDocumentMethod]
		public int GetSecurityPolicy(string DomainID)
		{
			Console.WriteLine("GetSecurityPolicy web service called");

			try
			{
                		Simias.Storage.Store sstore = Simias.Storage.Store.GetStore();
                		Simias.Storage.Domain sdomain = sstore.GetDomain(DomainID);
                		Simias.Storage.Member smember = sdomain.GetCurrentMember();
                		if (smember == null)
                		{
                    			return 0;
                		}
		 		return(Simias.Policy.SecurityState.GetStatus( smember ));
			}
			catch( Exception e )
			{
				Console.WriteLine(e);
				throw(e);
			}
		}

		/// To return a bool value saying whether sharing is enabled or disabled for this iFolder.
		[WebMethod(EnableSession = true, Description = "This method is for getting Disable sharing policy from the Collectionstore.")]
		[SoapDocumentMethod]
		public bool GetDisableSharingPolicy(string currentUserID, string iFolderID, string OwnerID, string DomainID)
		{
			//bool SharingIsDisabled = false;
            try
            {
                Simias.Storage.Store store = Simias.Storage.Store.GetStore();
                Simias.Storage.Domain domain = store.GetDomain(DomainID);

                Collection col = store.GetCollectionByID(iFolderID);
                Simias.Storage.Member member = domain.GetCurrentMember();
                if(member.UserID != OwnerID)
                {
                    //FIXME : if this user has full control , but not ownership , then allow temporarily , 
                    // but code should be written so that it is reverted back during sync.

                    return true;
                }

                int UserAndAboveAggregateSharingStatus = 0;
                int iFolderSharingStatus ;

                UserAndAboveAggregateSharingStatus = Simias.Policy.Sharing.Get(member);

                iFolderSharingStatus = Simias.Policy.Sharing.GetStatus(col);
                if (((UserAndAboveAggregateSharingStatus & (int)Share.EnforcedSharing) == (int)Share.EnforcedSharing))
                {
                    /// If on system level or user level, enforcement of policy is there, it means the iFolder must not be shared
                    if ((UserAndAboveAggregateSharingStatus & (int)Share.Sharing) == (int)Share.Sharing)
                        return true;
                    return false;
                }
                if (iFolderSharingStatus != 0)
                {
                    if ((iFolderSharingStatus & (int)Share.Sharing) == (int)Share.Sharing)
                    {
                        /// it means, on iFolder Details page, admin had unchecked the box so sharing is enabled now
                        return true;
                    }
                    if ((iFolderSharingStatus & (int)Share.DisableSharing) == (int)Share.DisableSharing)
                    {
                        /// it means, on iFolder Details page, admin had checked the box so sharing is disabled
                        return false;
                    }
                }
                else
                {
                    if ((UserAndAboveAggregateSharingStatus & (int)Share.Sharing) == (int)Share.Sharing)
                    {
                        return true;
                    }
                    if ((UserAndAboveAggregateSharingStatus & (int)Share.DisableSharing) == (int)Share.DisableSharing)
                    {
                        return false;
                    }
                }
                return true;        
            }
            catch (Exception e)
            {
                throw (e);

            }
		}
        
        /// <summary>
        /// WebMethod that gets an limit policy based on an userid while transferring Ownership of iFolder
        /// </summary>
        /// <returns>
        /// ifolder can be transferred (true)  or not (false)
        /// < returns>
        [WebMethod(EnableSession = true, Description = "Gets whetgher ifolder can be transferred or not")]
        [SoapDocumentMethod]
        public bool CanOwnerBeChanged(string newUserID, string domainID)
        {
            Simias.Storage.Store store = Simias.Storage.Store.GetStore();
            Domain domain = store.GetDomain(domainID);
            Simias.Storage.Member member = domain.GetCurrentMember();
            return member.IsTransferAllowed(domain.ID, member.UserID, newUserID);
        }
        

		/// <summary>
        /// WebMethod that gets an iFolder based on an iFolderID
        /// </summary>
        /// <returns>
        /// ifolder can be created (1)  or not (0)
        /// < returns>
        [WebMethod(EnableSession = true, Description = "Gets whetgher ifolder can be created or not")]
        [SoapDocumentMethod]
        public int GetLimitPolicyStatus(string DomainID)
        {
            
            int count = 0;
            long NoiFoldersLimit = -1;
            try
            {
                Simias.Storage.Store store = Simias.Storage.Store.GetStore();
                Simias.Storage.Domain domain = store.GetDomain(DomainID);
                //Simias.Storage.Domain domain = store.GetDomain(store.DefaultDomain);
                if (domain == null)
                    return 0;
                Simias.Storage.Member member = domain.GetCurrentMember();
                count = GetiFoldercount(DomainID, member.UserID);
               
                NoiFoldersLimit = Simias.Policy.iFolderLimit.Get(member).Limit;

                // if -1 , then no limit applied anywhere
                if (NoiFoldersLimit < 0)
                {
                    return 1;
                }
                else
                if (count >= NoiFoldersLimit)
                {
                    return 0;
                }

                return 1;
  
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw (e);
            }
        }


		/// <summary>
		/// WebMethod that gets an iFolder based on an iFolderID
		/// </summary>
		/// <param name = "POBoxID"></param>
		/// <param name = "iFolderID">
		/// The ID of the collection representing this iFolder to get
		/// </param>
		/// <returns>
		/// the ifolder this ID represents
		/// </returns>
		[WebMethod(EnableSession=true, Description="Get An iFolder")]
		[SoapDocumentMethod]
		public iFolderWeb GetiFolderInvitation(string POBoxID, string iFolderID)
		{
			iFolderWeb ifolder = null;
		/*
			Store store = Store.GetStore();
			POBox poBox = Simias.POBox.POBox.GetPOBoxByID(store, POBoxID);
			if(poBox != null)
			{
				Node node = poBox.GetNodeByID(iFolderID);
				if (node != null)
				{
					Subscription sub = new Subscription(node);

					// Only return subscriptions that are for us (the current user)
					if(sub.ToIdentity == poBox.Owner.UserID)
					{
						// BHT: Filter out subscriptions that are not iFolders
						if (sub.SubscriptionCollectionType == iFolderWeb.iFolderType)
							ifolder = new iFolderWeb(sub);
					}
				}
			}
		*/
			CollectionInfo ci = DiscoveryFramework.GetLocalCollectionInfo(iFolderID);
            if(ci == null)
                ci = DiscoveryFramework.GetCollectionInfo(iFolderID);
			ifolder = new iFolderWeb(ci);
			return ifolder;
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
		[WebMethod(EnableSession=true, Description="Get An iFolder using a LocalPath")]
		[SoapDocumentMethod]
		public iFolderWeb GetiFolderByLocalPath(string LocalPath)
		{
			iFolderWeb ifolder = null;
			Collection col = SharedCollection.GetCollectionByPath(LocalPath);
			if(col != null)
			{
				ifolder = new iFolderWeb(col);
			}

			return ifolder;
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
		[WebMethod(EnableSession=true, Description="Delete An iFolder")]
		[SoapDocumentMethod]
		public void DeleteiFolder(string DomainID, string iFolderID)
		{
                       Store store = Store.GetStore();
                        SharedCollection.DeleteSharedCollection(iFolderID);
			DiscoveryFramework.DeleteCollectionInCatalog(DomainID, iFolderID);

		}



                /// <summary>
                /// WebMethod that will revert an iFolder back to an unsubscribed
                /// state that can be then re-setup on the local box.  This API will
                /// not delete the iFolder
                /// </summary>
                /// <param name = "iFolderID">
                /// The ID of the collection representing this iFolder
                /// </param>
                /// <returns>
                /// An iFolder object representing the subscription for the reverted iFolder.
                /// </returns>
                [WebMethod(EnableSession=true, Description="Revert an iFolder on the local computer but remain a member")]
                [SoapDocumentMethod]
                public iFolderWeb RevertiFolder(string iFolderID)
                {
                        iFolderWeb ifolder = null;
                        CollectionInfo cinfo = SharedCollection.RevertSharedCollection(iFolderID);
                        if (cinfo != null)
                        {
                                ifolder = new iFolderWeb(cinfo);
                        }

                        return ifolder;
                }





		/// <summary>
		/// WebMethod that will revert an iFolder back to an unsubscribed
		/// state that can be then re-setup on the local box.  This API will
		/// not delete the iFolder
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the collection representing this iFolder
		/// </param>
		/// <returns>
		/// An iFolder object representing the subscription for the reverted iFolder.
		/// </returns>
		[WebMethod(EnableSession=true, Description="Revert an iFolder on the local computer but remain a member")]
		[SoapDocumentMethod]
		public iFolderWeb RevertiFolder1(string iFolderID)
		{
			iFolderWeb ifolder = null;
//			Subscription sub = SharedCollection.RevertSharedCollection(iFolderID);
//			if (sub != null)
//			{
//				ifolder = new iFolderWeb(sub);
//			}
//
			return ifolder;
		}




		/// <summary>
		/// WebMethod that returns all iFolders on the iFolder Server
		/// </summary>
		/// <returns>
		/// An array of iFolders
		/// </returns>
		[WebMethod(EnableSession=true, Description="Returns all iFolders on the Server")]
		[SoapDocumentMethod]
		public iFolderWeb[] GetAlliFolders()
		{
			Collection col = null;
			ArrayList list = null;
            ICSList iFolderList = null;
            ArrayList collectionList = null;

			list = new ArrayList();
			Store store = Store.GetStore();
			Simias.Discovery.DiscService.UpdateCollectionList();
			collectionList = Simias.Discovery.CollectionList.GetCollectionList();
            
            //Get all the iFolders that are downloaded and the info is present in the local store.
            iFolderList = store.GetCollectionsByType(iFolderWeb.iFolderType);
            foreach (ShallowNode sn in iFolderList)
            {
                col = null;
                col = store.GetCollectionByID(sn.ID);
                list.Add(new iFolderWeb(col));
            }

            if (collectionList != null && collectionList.Count != 0) //When domain/server is connected/online
            {
                foreach ( CollectionInfo ci in collectionList ) 
				{
					col = null;
                    // Add all the folders that are not local, leaving those that were added from local store.
					if ((col = store.GetCollectionByID(ci.CollectionID)) == null)
					{
						list.Add(new iFolderWeb(ci)); //adding ifolder on server
					}
				}
			}
			return (iFolderWeb[])list.ToArray(typeof(iFolderWeb));
		}


		/// <summary>
		/// WebMethod that returns all iFolders on the iFolder Server
		/// </summary>
		/// <returns>
		/// An array of iFolders
		/// </returns>
		[WebMethod(EnableSession=true, Description="Returns all iFolders on the Server")]
		[SoapDocumentMethod]
		public iFolderWeb[] GetAlliFolders1()
		{

			ArrayList list = new ArrayList();

			Store store = Store.GetStore();

			ICSList iFolderList = 
					store.GetCollectionsByType(iFolderWeb.iFolderType);

			foreach(ShallowNode sn in iFolderList)
			{
				Collection col = store.GetCollectionByID(sn.ID);
				list.Add(new iFolderWeb(col));
			}


			ICSList domainList = store.GetDomainList();

			foreach (ShallowNode sn in domainList)
			{
				// Now we need to get all of Subscriptions
				POBox poBox = Simias.POBox.POBox.FindPOBox(store, 
							sn.ID, 
							store.GetUserIDFromDomainID(sn.ID));
				if(poBox != null)
				{
	
					// Get all of the subscription obects in the POBox
					ICSList poList = poBox.Search(
							PropertyTags.Types,
							typeof(Subscription).Name,
							SearchOp.Equal);
	
					foreach(ShallowNode sNode in poList)
					{
						Subscription sub = new Subscription(poBox, sNode);
	
						// if the subscription is not for us, we don't
						// care
						if(sub.ToIdentity != poBox.Owner.UserID)
							continue;

						// BHT: Filter out subscriptions that are not iFolders
						if (sub.SubscriptionCollectionType != iFolderWeb.iFolderType)
							continue;
	
						// Filter out all subscriptions that match
						// iFolders that are already local on our machine
						if (store.GetCollectionByID(
									sub.SubscriptionCollectionID) != null)
						{
							continue;
						}
						// Add check for declined iFolders
						// We don't want those to show up either since they
						// are going to be deleted by the PO Service
						if(sub.SubscriptionDisposition == 
									SubscriptionDispositions.Declined)
						{
							continue;
						}
	
						list.Add(new iFolderWeb(sub));
					}
				}
			}
			return (iFolderWeb[])list.ToArray(typeof(iFolderWeb));
		}




		/// <summary>
		/// WebMethod that returns all iFolders on the iFolder Server
		/// </summary>
		/// <param name="DomainID">The ID of the domain.</param>
		/// <returns>
		/// An array of iFolders
		/// </returns>
		[WebMethod(EnableSession=true, Description="Returns all iFolders in the specified domain")]
		[SoapDocumentMethod]
		public iFolderWeb[] GetiFoldersForDomain( string DomainID )
		{
			ArrayList list = new ArrayList();
            Hashtable ht = new Hashtable();

			Store store = Store.GetStore();

			ICSList iFolderList = 
					store.GetCollectionsByDomain(DomainID);

            /// Add all the local iFolders belonging to the domain
			foreach(ShallowNode sn in iFolderList)
			{
				if (sn.Type.Equals(NodeTypes.CollectionType))
				{
					Collection col = store.GetCollectionByID(sn.ID);
					if (col.IsType(col, iFolderWeb.iFolderType))
					{
						list.Add(new iFolderWeb(col));
                        if( ht.ContainsKey( col.ID) == false)
                            ht.Add(col.ID, "");
					}
				}
			}

            /// Add the server iFolders only if the added folder is not a local iFolder..
			ArrayList collectionList = Simias.Discovery.CollectionList.GetCollectionList();
            foreach (CollectionInfo ci in collectionList)
            {
                if ( ci.DomainID != DomainID)
                    continue;
                if( ht.ContainsKey(ci.ID) == false)                    
                    list.Add (new iFolderWeb (ci));
            }            

			return (iFolderWeb[])list.ToArray(typeof(iFolderWeb));
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
		[WebMethod(EnableSession=true, Description="Returns iFolders for the specified UserID")]
		[SoapDocumentMethod]
		public iFolderWeb[] GetiFolders(string UserID)
		{
			ArrayList list = new ArrayList();

			Store store = Store.GetStore();


			ICSList iFolderList = 
					store.GetCollectionsByUser(UserID);

			foreach(ShallowNode sn in iFolderList)
			{
				Collection col = store.GetCollectionByID(sn.ID);
				if(col.IsType(col, iFolderWeb.iFolderType))
					list.Add(new iFolderWeb(col));
			}

			return (iFolderWeb[])list.ToArray(typeof(iFolderWeb));
		}

		public int GetiFoldercount(string DomainID, string userID)
                {
			ArrayList list = Simias.Discovery.CollectionList.GetCollectionList();
			int count = 0;
                        foreach (CollectionInfo ci in list)
                        {
                                if ( ci.DomainID != DomainID)
                                        continue;
				if( ci.OwnerID == userID )
					count++;
			}	
			return count;
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
		[WebMethod(EnableSession=true, Description="Set the Rights of a member of an iFolder.  The Rights can be \"Admin\", \"ReadOnly\", or \"ReadWrite\".")]
		[SoapDocumentMethod]
		public void SetUserRights(	string iFolderID, 
									string UserID,
									string Rights)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

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
					throw new Exception("Invalid Rights Specified");

				col.Commit(member);
			}
			else
			{

                try
                {

                    // If the user wasn't found, look for the UserID as
                    // a subscription in the default POBox
                    POBox poBox = Simias.POBox.POBox.FindPOBox(store,
                            col.Domain,
                            store.GetUserIDFromDomainID(col.Domain));
                    if (poBox == null)
                    {
                        throw new Exception("Unable to access POBox");

                    }

                    ICSList poList = poBox.Search(
                                Subscription.ToIdentityProperty,
                                UserID,
                                SearchOp.Equal);

                    Subscription sub = null;
                    foreach (ShallowNode sNode in poList)
                    {
                        sub = new Subscription(poBox, sNode);
                        break;
                    }

                    if (sub == null)
                    {
                        throw new Exception("Invalid UserID");
                    }

                    if (Rights == "Admin")
                        sub.SubscriptionRights = Access.Rights.Admin;
                    else if (Rights == "ReadOnly")
                        sub.SubscriptionRights = Access.Rights.ReadOnly;
                    else if (Rights == "ReadWrite")
                        sub.SubscriptionRights = Access.Rights.ReadWrite;
                    else
                        throw new Exception("Invalid Rights Specified");

                    poBox.Commit(sub);
                }
                catch (Exception ex)
                {
                    // do nothing , this is the case: user was moved to another server , POBox was not moved, so client
                    // does not have those POBox.
                }
			}
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
		[WebMethod(EnableSession=true, Description="Get the Owner of an iFolder")]
		[SoapDocumentMethod]
		public iFolderUser GetOwner( string iFolderID )
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			Domain domain = store.GetDomain( col.Domain);

			iFolderUser user = new iFolderUser( domain, col.Owner );
			return user;
		}




		/// <summary>
		/// WebMethod that sets the owner of an iFolder
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the collection representing the iFolder to which
		/// the member is to be added
		/// </param>
		/// <param name = "NewOwnerUserID">
		/// The ID of the member to be added
		/// </param>
		/// <param name = "OldOwnerRights">
		/// The Rights to be given to the newly added member
		/// </param>
		/// <returns>
		/// True if the member was successfully added
		/// </returns>
		[WebMethod(EnableSession=true, Description="Changes the owner of an iFolder and sets the rights of the previous owner to the rights specified.")]
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
		[WebMethod(EnableSession=true, Description="Remove a single member from an iFolder")]
		[SoapDocumentMethod]
		public void RemoveiFolderUser(	string iFolderID, 
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
		[WebMethod(EnableSession=true, Description="Get the list of iFolder Members")]
		[SoapDocumentMethod]
		public iFolderUser[] GetiFolderUsers(string iFolderID)
		{
			ArrayList members = new ArrayList();
			ICSList memberList;

			Store store = Store.GetStore();
			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new SimiasException("Invalid iFolderID");

			Simias.Storage.Domain domain = store.GetDomain( col.Domain );
			if ( domain == null )
			{
				throw new SimiasException( "iFolderID isn't linked to a valid domain " );
			}

			memberList = col.GetMemberList();
			foreach( ShallowNode sNode in memberList )
			{
				if ( sNode.Type.Equals( "Member" ) )
				{
					members.Add( new iFolderUser( domain, new Member( col, sNode ) ) );
				}
			}	
            
			// Use the POBox for the domain that this iFolder belongs to.
            POBox poBox;
            try
            {
                poBox = Simias.POBox.POBox.FindPOBox(store,
                    col.Domain,
                    store.GetUserIDFromDomainID(col.Domain));
            }
            catch (Exception ex)
            {
                poBox = null;
            }
            if (poBox != null)
            {
                ICSList poList = poBox.Search(
                    Subscription.SubscriptionCollectionIDProperty,
                    col.ID,
                    SearchOp.Equal);

                foreach (ShallowNode sNode in poList)
                {
                    Subscription sub = new Subscription(poBox, sNode);

                    // Filter out subscriptions that are on this box
                    // already
                    if (sub.SubscriptionState == SubscriptionStates.Ready)
                    {
                        if (poBox.StoreReference.GetCollectionByID(
                            sub.SubscriptionCollectionID) != null)
                        {
                            continue;
                        }
                    }

                    members.Add(new iFolderUser(sub));
                }
            }
            if (members.Count > 0)
            {
                UserComparer comparer = new UserComparer();
                members.Sort(0, members.Count, comparer);
            }
            
            return (iFolderUser[])(members.ToArray(typeof(iFolderUser)));
           
		}




		/// <summary>
		/// WebMethod that returns a limited list of iFolderUsers.  If there
		/// are more users than specified, the list will return none.
		/// </summary>
		/// <param name="DomainID"></param>
		/// <param name="numUsers">The number of iFolderUsers to return.
		/// -1 will return all of them.</param>
		/// <returns>
		/// An array of members
		/// </returns>
		[WebMethod(EnableSession=true, Description="Get a scoped list of iFolderUsers for the specified domain")]
		[SoapDocumentMethod]
		public iFolderUser[] GetDomainUsers(string DomainID, int numUsers)
		{
			int userCount = 0;
			ArrayList members = new ArrayList();

			Domain domain = Store.GetStore().GetDomain( DomainID );
			if( domain == null )
			{
				throw new SimiasException( "Invalid domain ID " );
			}

			ICSList memberList = domain.GetMemberList();
			foreach( ShallowNode sNode in memberList )
			{
				if ( sNode.Type.Equals( "Member" ) )
				{
					if( numUsers != -1 && ++userCount > numUsers )
					{
						// Empty the list and break;
						members.Clear();
						break;
					}

					members.Add( new iFolderUser( domain, new Member( domain, sNode ) ) );
				}
			}	

			if ( members.Count > 0 )
			{
				UserComparer comparer = new UserComparer();
				members.Sort( 0, members.Count, comparer );
			}

			return ( iFolderUser[] )( members.ToArray( typeof( iFolderUser ) ) );
		}





		/// <summary>
		/// Web method that returns a list of users whose name's contain
		/// the specified string.
		/// </summary>
		/// <param name="DomainID"></param>
		/// <param name="SearchString">The string to search for.</param>
		/// <returns>An array of users.</returns>
		[WebMethod(EnableSession=true, Description="Search for a Member of a specified name in the specified domain.")]
		[SoapDocumentMethod]
		public iFolderUser[] SearchForDomainUsers(string DomainID, string SearchString)
		{
			ArrayList members = new ArrayList();
			Hashtable matches = new Hashtable();

			Domain domain = Store.GetStore().GetDomain( DomainID );
			if( domain == null )
			{
				throw new SimiasException( "Invalid domain ID " );
			}

			ICSList	searchList = domain.Search( PropertyTags.FullName, SearchString, SearchOp.Begins );
			foreach( ShallowNode sNode in searchList )
			{
				if ( sNode.Type.Equals( "Member" ) )
				{
					Member member = new Member( domain, sNode );
					matches.Add( sNode.ID, member );
					members.Add( new iFolderUser( domain, member ) );
				}
			}	

			searchList = domain.Search( PropertyTags.Family, SearchString, SearchOp.Begins );
			foreach( ShallowNode sNode in searchList )
			{
				if ( sNode.Type.Equals( "Member" ) )
				{
					if ( matches.Contains( sNode.ID ) == false )
					{
						members.Add( new iFolderUser( domain, new Member( domain, sNode ) ) );
						matches.Add( sNode.ID, null );
					}
				}
			}	

			searchList = domain.Search( BaseSchema.ObjectName, SearchString, SearchOp.Begins );
			foreach( ShallowNode sNode in searchList )
			{
				if ( sNode.Type.Equals( "Member" ) )
				{
					if ( matches.Contains( sNode.ID ) == false )
					{
						members.Add( new iFolderUser( domain, new Member( domain, sNode ) ) );
						matches.Add( sNode.ID, null );
					}
				}
			}

			if ( members.Count > 0 )
			{
				UserComparer comparer = new UserComparer();
				members.Sort( 0, members.Count, comparer );
			}

			return ( iFolderUser[] )( members.ToArray( typeof( iFolderUser ) ) );
		}




		/// <summary>
		/// End the search for domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain.</param>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstMembers
		/// or FindFirstSpecificMembers methods.</param>
		[WebMethod(EnableSession=true, Description="End the search for domain members.")]
		[SoapDocumentMethod]
		public void FindCloseiFolderMembers( string domainID, string searchContext )
		{
			DomainProvider.FindCloseDomainMembers( domainID, searchContext );
		}



		/// <summary>
		/// Starts a search for all domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="totalMembers">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Starts a search for all domain members.")]
		[SoapDocumentMethod]
		public bool FindFirstiFolderMembers( 
			string domainID, 
			int count,
			out string searchContext, 
			out iFolderUser[] memberList, 
			out int totalMembers )
		{
			Member[] tempList;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers(
					domainID, 
					count,
					out searchContext, 
					out tempList, 
					out totalMembers );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				Domain domain = Store.GetStore().GetDomain( domainID );

				memberList = new iFolderUser[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new iFolderUser( domain, tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}



		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="attributeName">Attribute name to search.</param>
		/// <param name="searchString">String that contains a pattern to search for.</param>
		/// <param name="operation">Type of search operation to perform.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="totalMembers">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Starts a search for a specific set of domain members.")]
		[SoapDocumentMethod]
		public bool FindFirstSpecificiFolderMembers(
			string domainID, 
			string attributeName, 
			string searchString, 
			iFolderSearchType operation, 
			int count,
			out string searchContext, 
			out iFolderUser[] memberList, 
			out int totalMembers )
		{
			Member[] tempList;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers(
					domainID,
					attributeName,
					searchString,
					( Simias.Storage.SearchOp )Enum.ToObject( typeof( Simias.Storage.SearchOp ), operation ),
					count,
					out searchContext, 
					out tempList, 
					out totalMembers );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				Domain domain = Store.GetStore().GetDomain( domainID );

				memberList = new iFolderUser[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new iFolderUser( domain, tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}



		/// <summary>
		/// Continues the search for domain members from the current record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by 
		/// FindFirstiFolderMembers or FindFirstSpecificiFolderMembers methods.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Continues the search for domain members from the current record location.")]
		[SoapDocumentMethod]
		public bool FindNextiFolderMembers( 
			string domainID, 
			ref string searchContext, 
			int count,
			out iFolderUser[] memberList )
		{
			Member[] tempList;

			bool moreEntries = DomainProvider.FindNextDomainMembers( domainID, ref searchContext, count, out tempList );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				Domain domain = Store.GetStore().GetDomain( domainID );

				memberList = new iFolderUser[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new iFolderUser( domain, tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}



		/// <summary>
		/// Continues the search for domain members previous to the current record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by 
		/// FindFirstiFolderMembers or FindFirstSpecificiFolderMembers methods.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Continues the search for domain members previous to the current record location.")]
		[SoapDocumentMethod]
		public bool FindPreviousiFolderMembers( 
			string domainID, 
			ref string searchContext, 
			int count,
			out iFolderUser[] memberList )
		{
			Member[] tempList;

			bool moreEntries = DomainProvider.FindPreviousDomainMembers( domainID, ref searchContext, count, out tempList );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				Domain domain = Store.GetStore().GetDomain( domainID );

				memberList = new iFolderUser[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new iFolderUser( domain, tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}



		/// <summary>
		/// Continues the search for domain members from the specified record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by 
		/// FindFirstiFolderMembers or FindFirstSpecificiFolderMembers methods.</param>
		/// <param name="offset">Record offset to return members from.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Continues the search for domain members from the specified record location.")]
		[SoapDocumentMethod]
		public bool FindSeekiFolderMembers( 
			string domainID, 
			ref string searchContext, 
			int offset,
			int count,
			out iFolderUser[] memberList )
		{
			Member[] tempList;

			bool moreEntries = DomainProvider.FindSeekDomainMembers(
				domainID, 
				ref searchContext, 
				offset,
				count, 
				out tempList );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				Domain domain = Store.GetStore().GetDomain( domainID );

				memberList = new iFolderUser[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new iFolderUser( domain, tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}



		/// <summary>
		/// WebMethod that gets a member from the default domain
		/// </summary>
		/// <param name = "UserID">
		/// The ID of the member to be added
		/// </param>
		/// <returns>
		/// Member that matches the UserID
		/// </returns>
		[WebMethod(EnableSession=true, Description="Lookup a single member to a collection")]
		[SoapDocumentMethod]
		public iFolderUser GetiFolderUser( string UserID )
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomainForUser(UserID);
			if(domain == null)
				throw new Exception("Unable to access domain");

			Simias.Storage.Member simMem = domain.GetMemberByID(UserID);
			if(simMem == null)
				throw new Exception("Invalid UserID");

			return new iFolderUser( domain, simMem );
		}

		/// <summary>
		/// WebMethod that gets a member from the default domain
		/// </summary>
		/// <param name = "UserID">
		/// The ID of the member to be added
		/// </param>
		/// <returns>
		/// Member that matches the UserID
		/// </returns>
		[WebMethod(EnableSession=true, Description="Lookup a single member to a collection")]
		[SoapDocumentMethod]
		public string GetRAName( string DomainID )
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(DomainID);
			if(domain == null)
				throw new Exception("Unable to access domain");

			Simias.Storage.Member member = domain.GetCurrentMember();
			if(member == null)
				throw new Exception("Invalid UserID");
			return member.RAName;
		}


		/// <summary>
		/// WebMethod that gets a member from the specified collection
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection to search.
		/// </param>
		/// <param name = "NodeID">
		/// The ID of the node.
		/// </param>
		/// <returns>
		/// A user object.
		/// </returns>
		[WebMethod(EnableSession=true, Description="Lookup a user in a collection based on node ID.")]
		[SoapDocumentMethod]
		public iFolderUser GetiFolderUserFromNodeID(string CollectionID,
													string NodeID)
		{
			iFolderUser ifolderUser = null;
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col != null)
			{
				Node node = col.GetNodeByID(NodeID);
				if(node != null)
				{
					Domain domain = store.GetDomain( col.Domain );
					if (col.IsBaseType(node, NodeTypes.MemberType))
					{
						ifolderUser = new iFolderUser( domain, new Member( node ) );
					}
					else if (col.IsType(node, typeof( Subscription ).Name))
					{
						ifolderUser = new iFolderUser( new Subscription( node ) );
					}
				}
			}

			return ifolderUser;
		}




		/// <summary>
		/// WebMethod that adds a user to the domain and invites the user 
		/// to an iFolder.
		/// </summary>
		/// <param name = "iFolderID">The ID of the collection representing 
		/// the Collection to which the member is to be invited</param>
		/// <param name="MemberName">The name of the user.</param>
		/// <param name="GivenName">The first name of the user.</param>
		/// <param name="FamilyName">The last name of the user.</param>
		/// <param name = "MemberID">The ID of the member to be invited</param>
		/// <param name="PublicKey">The users public key.</param>
		/// <param name = "Rights">The Rights to be given to the newly invited member</param>
		/// <returns>iFolderUser that was invited</returns>
		[WebMethod(EnableSession=true, Description="Invite a user to an iFolder.  This call will only work with Enterprise iFolders")]
		[SoapDocumentMethod]
		public iFolderUser AddAndInviteUser(string iFolderID,
											string MemberName,
											string GivenName,
											string FamilyName,
											string MemberID,
											string PublicKey,
											string Rights)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			Domain domain = store.GetDomain(col.Domain);
			if(domain == null)
				throw new Exception("Unable to access domain");

			Simias.Storage.Member member = domain.GetMemberByID(MemberID);
			if(member == null)
			{
				bool given;
				member = new Simias.Storage.Member( MemberName, MemberID, Access.Rights.ReadOnly );

				if ( PublicKey != null )
				{
					member.Properties.AddProperty( "PublicKey", PublicKey );
				}

				if ( GivenName != null && GivenName != "" )
				{
					member.Given = GivenName;
					given = true;
				}
				else
				{
					given = false;
				}

				if ( FamilyName != null && FamilyName != "" )
				{
					member.Family = FamilyName;
					if ( given == true )
					{
						member.FN = GivenName + " " + FamilyName;
					}
				}

				domain.Commit( member );
			}

			Access.Rights newRights;

			if(Rights == "Admin")
				newRights = Access.Rights.Admin;
			else if(Rights == "ReadOnly")
				newRights = Access.Rights.ReadOnly;
			else if(Rights == "ReadWrite")
				newRights = Access.Rights.ReadWrite;
			else
				throw new Exception("Invalid Rights Specified");


			// member
			Member membercoll = new Member(MemberName, MemberID, newRights);

			// commic
			col.Commit(membercoll);

			iFolderUser user = new iFolderUser(domain, membercoll );
			return user;
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
		[WebMethod(EnableSession=true, Description="Invite a user to an iFolder.  This call will only work with Enterprise iFolders")]
		[SoapDocumentMethod]
		public iFolderUser InviteUser(	string iFolderID,
										string UserID,
										string Rights)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			Domain domain = store.GetDomain(col.Domain);
			if(domain == null)
				throw new Exception("Unable to access domain");

			Simias.Storage.Member member = domain.GetMemberByID(UserID);
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
			POBox poBox = Simias.POBox.POBox.FindPOBox(store, 
						domain.ID, 
						store.GetUserIDFromDomainID(domain.ID));

			Subscription sub = poBox.CreateSubscription(col,
										col.GetCurrentMember(),
										"iFolder");

			sub.SubscriptionRights = newRights;
			sub.ToName = member.Name;
			sub.ToIdentity = UserID;

			poBox.AddMessage(sub);

			iFolderUser user = new iFolderUser( sub );
			return user;
		}

		/// <summary>
		/// Accepts an Enterprise Subscription
		/// </summary>
		/// <param name="DomainID"></param>
		/// <param name = "iFolderID">
		/// The ID of the iFolder to accept the invitation for
		/// </param>
		/// <param name = "LocalPath">
		/// The LocalPath to to store the iFolder
		/// </param>
		[WebMethod(EnableSession=true, Description="Accept an invitation fo an iFolder.  The iFolder ID represents a Subscription object")]
		[SoapDocumentMethod]
		public iFolderWeb MergeiFolder( string DomainID,
							   string iFolderID, 
							   string path)
                {
                        Store store = Store.GetStore ();
//TODO : later. need a unique way to get the collection information
//                  CollectionInfo cinfo = GetCollectionInfo (DomainID, iFolderID);
//TODO : create this exception inf the framework
                        CollectionInfo cinfo = DiscoveryFramework.GetLocalCollectionInfo (iFolderID);
                        if(cinfo == null)
                            cinfo = DiscoveryFramework.GetCollectionInfo(iFolderID);

                        if(cinfo == null)
                                throw new Exception("Invalid iFolderID");

                        CollectionPathStatus pStatus;

                        pStatus = SharedCollection.CheckCollectionPath(path);
                        switch(pStatus)
                        {
                                case CollectionPathStatus.ValidPath:
                                        break;
                                case CollectionPathStatus.RootOfDrivePath:
                                        throw new Exception("RootOfDrivePath");
                                case CollectionPathStatus.InvalidCharactersPath:
                                        throw new Exception("InvalidCharactersPath");
                                case CollectionPathStatus.AtOrInsideStorePath:
                                        throw new Exception("AtOrInsideStorePath");
                                case CollectionPathStatus.ContainsStorePath:
                                        throw new Exception("ContainsStorePath");
                                case CollectionPathStatus.NotFixedDrivePath:
                                        throw new Exception("NotFixedDrivePath");
                                case CollectionPathStatus.SystemDirectoryPath:
                                        throw new Exception("SystemDirectoryPath");
                                case CollectionPathStatus.SystemDrivePath:
                                        throw new Exception("SystemDrivePath");
                                case CollectionPathStatus.IncludesWinDirPath:
                                        throw new Exception("IncludesWinDirPath");
                                case CollectionPathStatus.IncludesProgFilesPath:
                                        throw new Exception("IncludesProgFilesPath");
                                case CollectionPathStatus.DoesNotExistPath:
                                        throw new Exception("DoesNotExistPath");
                                case CollectionPathStatus.NoReadRightsPath:
                                        throw new Exception("NoReadRightsPath");
                                case CollectionPathStatus.NoWriteRightsPath:
                                        throw new Exception("NoWriteRightsPath");
                                case CollectionPathStatus.ContainsCollectionPath:
                                        throw new Exception("ContainsCollectionPath");
                                case CollectionPathStatus.AtOrInsideCollectionPath:
                                        throw new Exception("AtOrInsideCollectionPath");
                        }


			DiscoveryFramework.CreateProxy (store, cinfo, DomainID, iFolderID, Path.GetFullPath (path));
			Collection coll = store.GetCollectionByID(cinfo.ID);
			coll.Merge = true;
			coll.Commit();
			iFolderWeb ifolder = new iFolderWeb(cinfo);
 			ifolder.State = "Available";
			ifolder.Role = "Master";
 			ifolder.UnManagedPath = path;
 			ifolder.IsSubscription = false;
			return ifolder;
		}

		/// <summary>
		/// Accepts an Enterprise Subscription
		/// </summary>
		/// <param name="DomainID"></param>
		/// <param name = "iFolderID">
		/// The ID of the iFolder to accept the invitation for
		/// </param>
		/// <param name = "LocalPath">
		/// The LocalPath to to store the iFolder
		/// </param>
		[WebMethod(EnableSession=true, Description="Accept an invitation fo an iFolder.  The iFolder ID represents a Subscription object")]
		[SoapDocumentMethod]
		public iFolderWeb AcceptiFolderInvitation( string DomainID,
							   string iFolderID, 
							   string LocalPath)
		{
		        Store store = Store.GetStore ();
//TODO : later. need a unique way to get the collection information
//		    CollectionInfo cinfo = GetCollectionInfo (DomainID, iFolderID); 
//TODO : create this exception inf the framework
		        CollectionInfo cinfo = DiscoveryFramework.GetLocalCollectionInfo (iFolderID);
            if(cinfo == null)
                cinfo = DiscoveryFramework.GetCollectionInfo(iFolderID);

 			if(cinfo == null)
 				throw new Exception("Invalid iFolderID");

		    
			string path = Path.Combine(LocalPath, cinfo.DirNodeName);
			if (Directory.Exists(path))
				throw new Exception("PathExists");

			CollectionPathStatus pStatus;

			pStatus = SharedCollection.CheckCollectionPath(path);
			switch(pStatus)
			{
				case CollectionPathStatus.ValidPath:
					break;
				case CollectionPathStatus.RootOfDrivePath:
					throw new Exception("RootOfDrivePath");
				case CollectionPathStatus.InvalidCharactersPath:
					throw new Exception("InvalidCharactersPath");
				case CollectionPathStatus.AtOrInsideStorePath:
					throw new Exception("AtOrInsideStorePath");
				case CollectionPathStatus.ContainsStorePath:
					throw new Exception("ContainsStorePath");
				case CollectionPathStatus.NotFixedDrivePath:
					throw new Exception("NotFixedDrivePath");
				case CollectionPathStatus.SystemDirectoryPath:
					throw new Exception("SystemDirectoryPath");
				case CollectionPathStatus.SystemDrivePath:
					throw new Exception("SystemDrivePath");
				case CollectionPathStatus.IncludesWinDirPath:
					throw new Exception("IncludesWinDirPath");
				case CollectionPathStatus.IncludesProgFilesPath:
					throw new Exception("IncludesProgFilesPath");
				case CollectionPathStatus.DoesNotExistPath:
					throw new Exception("DoesNotExistPath");
				case CollectionPathStatus.NoReadRightsPath:
					throw new Exception("NoReadRightsPath");
				case CollectionPathStatus.NoWriteRightsPath:
					throw new Exception("NoWriteRightsPath");
				case CollectionPathStatus.ContainsCollectionPath:
					throw new Exception("ContainsCollectionPath");
				case CollectionPathStatus.AtOrInsideCollectionPath:
					throw new Exception("AtOrInsideCollectionPath");
			}

			DiscoveryFramework.CreateProxy (store, cinfo, DomainID, iFolderID, Path.GetFullPath (path));
			iFolderWeb ifolder = new iFolderWeb(cinfo);
 			ifolder.State = "Available";
			ifolder.Role = "Master";
 			ifolder.UnManagedPath = path;
 			ifolder.IsSubscription = false;
			return ifolder;
		}

		/// <summary>
                /// Accepts an Enterprise Subscription
                /// </summary>
                /// <param name="DomainID"></param>
                /// <param name = "iFolderID">
                /// The ID of the iFolder to accept the invitation for
                /// </param>
                /// <param name = "LocalPath">
                /// The LocalPath to to store the iFolder
                /// </param>
                [WebMethod(EnableSession=true, Description="Accept an invitation fo an iFolder.  The iFolder ID represents a Subscription object")]
                [SoapDocumentMethod]
                public iFolderWeb AcceptiFolderInvitation1( string DomainID, 
                                                                                                   string iFolderID,
                                                                                                string LocalPath)
                {
                        Store store = Store.GetStore();
//note : check for local collection. ui is handling the collection information wrongly
//here use the discovery framework to get the collection information.
                        POBox poBox = Simias.POBox.POBox.FindPOBox(store,
                                                DomainID,
                                                store.GetUserIDFromDomainID(DomainID));

                        // iFolders returned in the Web service are also
                        // Subscriptions and it ID will be the subscription ID
                        Node node = poBox.GetNodeByID(iFolderID);
                        if(node == null)
                                throw new Exception("Invalid iFolderID : id : "+ iFolderID);

                        Subscription sub = new Subscription(node);

                        string path = Path.Combine(LocalPath, sub.DirNodeName);
                        if (Directory.Exists(path))
                                throw new Exception("PathExists");

                        CollectionPathStatus pStatus;

			pStatus = SharedCollection.CheckCollectionPath(path);
                        switch(pStatus)
                        {
                                case CollectionPathStatus.ValidPath:
                                        break;
                                case CollectionPathStatus.RootOfDrivePath:
                                        throw new Exception("RootOfDrivePath");
                                case CollectionPathStatus.InvalidCharactersPath:
                                        throw new Exception("InvalidCharactersPath");
                                case CollectionPathStatus.AtOrInsideStorePath:
                                        throw new Exception("AtOrInsideStorePath");
                                case CollectionPathStatus.ContainsStorePath:
                                        throw new Exception("ContainsStorePath");
                                case CollectionPathStatus.NotFixedDrivePath:
                                        throw new Exception("NotFixedDrivePath");
                                case CollectionPathStatus.SystemDirectoryPath:
                                        throw new Exception("SystemDirectoryPath");
                                case CollectionPathStatus.SystemDrivePath:
                                        throw new Exception("SystemDrivePath");
                                case CollectionPathStatus.IncludesWinDirPath:
                                        throw new Exception("IncludesWinDirPath");
                                case CollectionPathStatus.IncludesProgFilesPath:
                                        throw new Exception("IncludesProgFilesPath");
                                case CollectionPathStatus.DoesNotExistPath:
                                        throw new Exception("DoesNotExistPath");
                                case CollectionPathStatus.NoReadRightsPath:
                                        throw new Exception("NoReadRightsPath");
                                case CollectionPathStatus.NoWriteRightsPath:
                                        throw new Exception("NoWriteRightsPath");
                                case CollectionPathStatus.ContainsCollectionPath:
                                        throw new Exception("ContainsCollectionPath");
                                case CollectionPathStatus.AtOrInsideCollectionPath:
                                        throw new Exception("AtOrInsideCollectionPath");
                        }

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

                        iFolderWeb ifolder = new iFolderWeb(sub);
                        return ifolder;
                }


		/// <summary>
		/// Check for new Mac client avaibility
		/// </summary>
		/// <param name="DomainID"></param>
		/// <param name = "CurrentVersion">
		/// Version of the Mac client running on machine. 
		/// </param>
		[WebMethod(EnableSession=true, Description="Check for newer Mac client availability.")]
		[SoapDocumentMethod]
		public int CheckForMacUpdate( string DomainID, string CurrentVersion, out string ServerVersion)
		{
			
			string serverVersion = null;
                        int status = Novell.iFolder.Install.ClientUpgrade.CheckForMacUpdate(DomainID, CurrentVersion, out serverVersion);
			
                        ServerVersion = serverVersion;
                        Store store = Store.GetStore();
                        Domain dom = store.GetDomain(DomainID);
			
                        if (dom != null)
                        {
                                Version ver = new Version(serverVersion);
                                dom.ServerVersion = ver; 
                                dom.Commit();
                        }
                        return status;
		}

        	
		/// <summary>
		/// Decline an Enterprise subscription
		/// </summary>
		/// <param name="DomainID">The ID of the domain that the iFolder belongs to.</param>
		/// <param name = "iFolderID">
		/// The ID of the iFolder to decline the invitation for
		/// </param>
		[WebMethod(EnableSession=true, Description="Decline an invitation to an iFolder.  The iFolder ID represents a Subscription object")]
		[SoapDocumentMethod]
		public void DeclineiFolderInvitation( string DomainID, string iFolderID )
		{
			DiscoveryFramework.RemoveMembership (DomainID, iFolderID);
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
		[WebMethod(EnableSession=true, Description="Gets the DiskSpaceQuota for a member")]
		[SoapDocumentMethod]
		public DiskSpace GetUserDiskSpace( string UserID )
		{
			try
			{
				return DiskSpace.GetMemberDiskSpace(UserID);
			}
			catch(Exception e)
			{
				throw e;
			}
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
		[WebMethod(EnableSession=true, Description="Gets the DiskSpaceQuota for an iFolder")]
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
		[WebMethod(EnableSession=true, Description="Sets the Disk Space Limit for a user")]
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
		[WebMethod(EnableSession=true, Description="Sets the Disk Space Limit for an iFolder")]
		[SoapDocumentMethod]
		public void SetiFolderDiskSpaceLimit( string iFolderID, long Limit )
		{
			DiskSpace.SetiFolderDiskSpaceLimit(iFolderID, Limit);
		}


		/// <summary>
                /// WebMethod that sets the Secure SYnc for an iFolder.
                /// </summary>
                /// True for ssl and false for non ssl 
                /// </param>
        [WebMethod(EnableSession = true, Description = "Sets the Disk Space Limit for an iFolder")]
        [SoapDocumentMethod]
        public void SetiFolderSecureSync(string iFolderID, bool ssl)
        {
            Store store = Store.GetStore();

            Collection col = store.GetCollectionByID(iFolderID);
            if (col == null)
                throw new Exception("Invalid iFolderID");

            col.SSL = ssl;

            col.Commit();
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
		[WebMethod(EnableSession=true, Description="Sets the Sync Interval for an iFolder")]
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
		[WebMethod(EnableSession=true, Description="Sets the Default Sync Interval")]
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
		[WebMethod(EnableSession=true, Description="Gets the Default Sync Interval")]
		[SoapDocumentMethod]
		public int GetDefaultSyncInterval()
		{
			return Simias.Policy.SyncInterval.GetInterval();
		}




		/// <summary>
		/// WebMethod that will authenticate a domain
		/// </summary>
		/// <param name = "DomainID"></param>
		/// <param name = "Password">
		/// The password to use to connect to the Domain
		/// </param>
		/// <returns>
		/// The Domain object associated with this Server
		/// </returns>
		[WebMethod(EnableSession=true, Description="Connects to an iFolder Domain")]
		[SoapDocumentMethod]
		public int AuthenticateToDomain(	string DomainID,
											string Password)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(DomainID);
			if(domain == null)
				throw new Exception("ERROR:Invalid Domain ID");

			Member member = domain.GetCurrentMember();
			if(member == null)
				throw new Exception("ERROR:Unable locate user");

			DomainService domainSvc = new DomainService();

			Uri uri = DomainProvider.ResolveLocation(DomainID);
			if (uri == null)
				throw new Exception("ERROR:No host address for domain");

			domainSvc.Url = uri.ToString() + "/DomainService.asmx";

			domainSvc.Credentials =
				new NetworkCredential(member.Name, Password);

			try
			{
				// Call the remote domain service and attempt to
				// get Domain Information.  This will force an
				// authentication to occurr
				domainSvc.GetDomainInfo(member.UserID);

				/*
				NetCredential cCreds =
					new NetCredential("iFolder", DomainID, true, member.Name,
								Password);
				if(cCreds == null)
					throw new Exception("ERROR: Updating NetCredentials");
				*/
			}
			catch(WebException webEx)
			{
				if (webEx.Status == System.Net.WebExceptionStatus.ProtocolError ||
					webEx.Status == System.Net.WebExceptionStatus.TrustFailure)
				{
					throw new Exception("ERROR: Invalid Credentials");
				}
				else if (webEx.Status == System.Net.WebExceptionStatus.ConnectFailure)
				{
					throw new Exception("ERROR: Domain Connection failed");
				}
				else
					throw webEx;
			}

			return 0;
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
		[WebMethod(EnableSession=true, Description="Connects to an iFolder Enterprise Server")]
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
				Node conflictNode = col.GetNodeByID(sn.ID);
				
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
		/// <param name = "localChangesWin">
		/// A bool that determines if the local or server copies win
		/// </param>
		[WebMethod(EnableSession=true, Description="Resolves a file conflict in an iFolder.")]
		[SoapDocumentMethod]
		public void ResolveFileConflict(string iFolderID, string conflictID, bool localChangesWin)
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
		/// WebMethod that resolves a conflict
		/// </summary>
		/// <param name = "iFolderID">
		/// The iFolder ID to return the conflicts
		/// </param>
		/// <param name = "conflictID">
		/// The node ID that represents a conflict to resolve
		/// </param>
		/// <param name = "localChangesWin">
		/// A bool that determines if the local or server copies win
		/// </param>
		[WebMethod(EnableSession=true, Description="Resolves a file conflict in an iFolder.")]
		[SoapDocumentMethod]
		public void ResolveEnhancedFileConflict(string iFolderID, string conflictID, bool localChangesWin,
										string conflictBinPath)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			Node conflictNode = col.GetNodeByID(conflictID);
			if(conflictNode == null)
				throw new Exception("Invalid conflictID");

			Conflict.Resolve(col, conflictNode, localChangesWin, conflictBinPath);
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
		/// <param name = "newLocalName"></param>
		[WebMethod(EnableSession=true, Description="Resolves a name conflict")]
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
		/// Webmethod that renames a conflicting (local) file and resolves the conflicted (server) file
		/// to the same name.
		/// </summary>
		/// <param name="iFolderID">The ID of the iFolder containing the conflict.</param>
		/// <param name="conflictID">The ID of the conflict.</param>
		/// <param name="newFileName">The new name of the conflicting file.</param>
		[WebMethod(EnableSession=true, Description="Renames a file and resolves a name conflict")]
		[SoapDocumentMethod]
		public void RenameAndResolveConflict(string iFolderID, string conflictID, string newFileName)
		{
			Collection col = Store.GetStore().GetCollectionByID(iFolderID);
			if (col == null)
				throw new Exception("Invalid iFolderID");

			Node conflictNode = col.GetNodeByID(conflictID);
			if (conflictNode == null)
				throw new Exception("Invalid conflictID");

			Conflict.RenameConflictingAndResolve(col, conflictNode, newFileName);
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
		[WebMethod(EnableSession=true, Description="Sets up a proxy for iFolder to use")]
		[SoapDocumentMethod]
		public void SetupProxy(string Host, int Port)
		{
			// I'm not sure what to do here
		}




		/// <summary>
		/// WebMethod that will setup the Proxy
		/// </summary>
		[WebMethod(EnableSession=true, Description="Removes proxy settings")]
		[SoapDocumentMethod]
		public void RemoveProxy()
		{
			// I'm not sure what to do here
		}




		/// <summary>
		/// WebMethod to calculate the number of nodes and bytes that need sync'd.
		/// </summary>
		/// <param name="iFolderID">The collection ID of the iFolder to calculate the sync size of.</param>
		/// <returns>The number of nodes that need to be sync'd.</returns>
		[WebMethod(EnableSession=true, Description="Calculates the number of nodes and bytes that need to be sync'd.")]
		[SoapDocumentMethod]
		public SyncSize CalculateSyncSize(string iFolderID)
		{
			Collection col = Store.GetStore().GetCollectionByID(iFolderID);
			if (col == null)
			{
				throw new Exception("Invalid iFolderID");
			}

			uint SyncNodeCount;
			ulong SyncByteCount;
			SharedCollection.CalculateSendSize(col, out SyncNodeCount, out SyncByteCount);
			
			return new SyncSize(SyncNodeCount, SyncByteCount);
		}




		/// <summary>
		/// WebMethod that sends a command to the sync engine to sync the iFolder of the specified ID.
		/// </summary>
		/// <param name="iFolderID">The collection ID of the iFolder to sync.</param>
		[WebMethod(EnableSession=true, Description="Sends a command to the sync engine to sync the iFolder of the specified ID.")]
		[SoapDocumentMethod]
		public void SynciFolderNow(string iFolderID)
		{
			SharedCollection.SyncCollectionNow(iFolderID);
		}
	
		
		
		
		/// <summary>
		/// WebMethod that deletes a File Size Limit policy from an iFolder.
		/// </summary>
		/// <param name="iFolderID">The ID of the iFolder to delete the policy from.</param>
		[WebMethod(EnableSession=true, Description="Delete a file size limit policy from an iFolder")]
		[SoapDocumentMethod]
		public void DeleteiFolderFileSizeLimit(string iFolderID)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			FileSizeFilter.Delete(col);
		}


		
	
		/// <summary>
		/// WebMethod that gets a users File Size Limit on an iFolder.
		/// </summary>
		/// <param name="UserID">The ID of the user.</param>
		/// <param name="iFolderID">The ID of the iFolder.</param>
		/// <returns>The file size limit in bytes.</returns>
		[WebMethod(EnableSession=true, Description="Get a users file size limit on an iFolder")]
		[SoapDocumentMethod]
		public long GetMemberiFolderFileSizeLimit(string UserID, string iFolderID)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomainForUser(UserID);
			if(domain == null)
				throw new Exception("Unable to access domain");

			Simias.Storage.Member member = domain.GetMemberByID(UserID);
			if(member == null)
				throw new Exception("Invalid UserID");

			Collection col = store.GetCollectionByID(iFolderID);
			if (col == null)
				throw new Exception("Invalid iFolderID");

			FileSizeFilter filter = FileSizeFilter.Get(col);
			if(filter == null)
				throw new Exception("Unable to get File Size Limit");

			return filter.Limit;
		}


		
	
		/// <summary>
		/// WebMethod that gets the File Size Limit of an iFolder.
		/// </summary>
		/// <param name="iFolderID">The ID of the iFolder.</param>
		/// <returns>The file size limit in bytes.</returns>
		[WebMethod(EnableSession=true, Description="Get the file size limit of an iFolder")]
		[SoapDocumentMethod]
		public long GetiFolderFileSizeLimit(string iFolderID)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if (col == null)
				throw new Exception("Invalid iFolderID");

			return FileSizeFilter.GetLimit(col);
		}


		/// <summary>
		/// WebMethod that sets the File Size Limit of an iFolder.
		/// </summary>
		/// <param name="iFolderID">The ID of the iFolder.</param>
		/// <param name="Limit">The file size limit (in bytes) to set.</param>
		[WebMethod(EnableSession=true, Description="Set the file size limit of an iFolder")]
		[SoapDocumentMethod]
		public void SetiFolderFileSizeLimit(string iFolderID, long Limit)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if (col == null)
				throw new Exception("Invalid iFolderID");

			FileSizeFilter.Set(col, Limit);
		}

		/// <summary>
		/// Checks to see if there is a newer client application available on the domain server and
		/// prompts the user to upgrade.
		/// </summary>
		/// <param name="domainID">The ID of the domain to check for updates against.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		[WebMethod(Description="Check for an available update")]
		[SoapDocumentMethod]
		public string CheckForUpdatedClientAvailable(string domainID)
		{
			return Novell.iFolder.Install.ClientUpgrade.CheckForUpdateAvailable(domainID);
		}


		/// <summary>
		/// Checks to see if there is a need for newer client application on the domain server and
		/// prompts the user to upgrade.
		/// </summary>
		/// <param name="domainID">The ID of the domain to check for updates against.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		[WebMethod(Description="Check for an available update")]
		[SoapDocumentMethod]
		public string CheckForUpdatedClient(string domainID)
		{
		//	return Novell.iFolder.Install.ClientUpgrade.CheckForUpdate(domainID);
			return null;
		}

		/// <summary>
		/// Checks to see if there is a need for newer client application on the domain server and
		/// prompts the user to upgrade.
		/// </summary>
		/// <param name="domainID">The ID of the domain to check for updates against.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		[WebMethod(Description="Check for an available update")]
		[SoapDocumentMethod]
		public int CheckForUpdate(string domainID, out string ServerVersion)
		{
			string serverVersion = null;
			int status = Novell.iFolder.Install.ClientUpgrade.CheckForUpdate(domainID, out serverVersion);
			ServerVersion = serverVersion;
            		Store store = Store.GetStore();
            		Domain dom = store.GetDomain(domainID);
            		if (dom != null)
            		{
                		if (dom.Owner != null)
                		{
                			Version ver = new Version(serverVersion);
                			dom.ServerVersion = ver;
                			dom.Commit();
				}
            		}
			return status;
		}

		/// <summary>
		/// Checks to see if the erver is running an
		/// older version of simias
		/// </summary>
		/// <param name="domainID">The ID of the domain to check for updates against.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		[WebMethod(Description="Check for an available update")]
		[SoapDocumentMethod]
		public bool CheckForServerUpdate(string domainID)
		{
			return Novell.iFolder.Install.ClientUpgrade.CheckForServerUpdate(domainID);
		}

		/// <summary>
		/// Gets the updated client application and runs the installation program.
		/// Note: This call will return before the application is updated.
		/// </summary>
		/// <param name="domainID">The ID of the domain to check for updates against.</param>
		/// <returns>True if the installation program is successfully started. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Run the client update")]
		[SoapDocumentMethod]
		public bool RunClientUpdate(string domainID, string path)
		{
			Console.WriteLine("RunClientUpdate web service called");
			return Novell.iFolder.Install.ClientUpgrade.RunUpdate(domainID, path);
		}

	        /// <summary>
        	/// Gets the updated client application and runs the installation program.
	        /// Note: This call will return before the application is updated.
        	/// </summary>
	        /// <param name="domainID">The ID of the domain to check for updates against.</param>
        	/// <returns>True if the installation program is successfully started. Otherwise false is returned.</returns>
	        [WebMethod(EnableSession = true, Description = "Run the client update")]
        	[SoapDocumentMethod]
	        public int ChangePassword(string domainid, string oldpassword, string newpassword)
        	{
                int retval = -1;
                Simias.Storage.Store store = Simias.Storage.Store.GetStore();
                Domain domain = store.GetDomain(domainid);
                Member member = domain.GetCurrentMember();
                retval = member.ChangePassword(oldpassword, newpassword);
                return retval;
	        }

		/// <summary>
		/// Gets the default public key for the domain - used for iFolder encryption.
		/// </summary>
		/// <param name="DomainID">The ID of the domain to get the Default public key .</param>
		/// <param name="UserID">The ID of the user in the specified domain</param>
		/// <returns>The default public key of the domain otherwise null.</returns>
		[WebMethod(EnableSession = true, Description = "Gets the Default public key for iFolder key encryption")]
		[SoapDocumentMethod]
		//public string GetDefaultPublicKey(string DomainID, string UserID)
		public string GetDefaultServerPublicKey(string DomainID, string UserID)
		{
			return SharedCollection.GetDefaultPublicKey(DomainID, UserID);
		}
	}
}
