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
using System.Net;
using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Simias.Policy;
using Simias.Web;
using System.Xml;
using System.Xml.Serialization;
//using Novell.AddressBook;

using Novell.Security.ClientPasswordManager;

namespace Novell.iFolder.Web
{
	/// <summary>
	/// Supported store search operators.
	/// </summary>
	public enum SearchType
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

			return ifolder;
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

			Store store = Store.GetStore();
			POBox poBox = Simias.POBox.POBox.GetPOBoxByID(store, POBoxID);
			if(poBox != null)
			{
				Node node = poBox.GetNodeByID(iFolderID);
				if (node != null)
				{
					ifolder = new iFolderWeb(new Subscription(node));
				}
			}

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
		public void DeleteiFolder(string iFolderID)
		{
			SharedCollection.DeleteSharedCollection(iFolderID);
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
			Subscription sub = SharedCollection.RevertSharedCollection(iFolderID);
			if (sub != null)
			{
				ifolder = new iFolderWeb(sub);
			}

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

			Store store = Store.GetStore();

			ICSList iFolderList = 
					store.GetCollectionsByDomain(DomainID);

			foreach(ShallowNode sn in iFolderList)
			{
				if (sn.Type.Equals(NodeTypes.CollectionType))
				{
					Collection col = store.GetCollectionByID(sn.ID);
					if (col.IsType(col, iFolderWeb.iFolderType))
					{
						list.Add(new iFolderWeb(col));
					}
				}
			}


			// Now we need to get all of Subscriptions
			POBox poBox = Simias.POBox.POBox.FindPOBox(store, 
						DomainID, 
						store.GetUserIDFromDomainID(DomainID));
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
				// If the user wasn't found, look for the UserID as
				// a subscription in the default POBox
				POBox poBox = Simias.POBox.POBox.FindPOBox(store, 
						col.Domain, 
						store.GetUserIDFromDomainID(col.Domain));
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

				if (sub == null)
				{
					throw new Exception("Invalid UserID");
				}
				
				if(Rights == "Admin")
					sub.SubscriptionRights = Access.Rights.Admin;
				else if(Rights == "ReadOnly")
					sub.SubscriptionRights = Access.Rights.ReadOnly;
				else if(Rights == "ReadWrite")
					sub.SubscriptionRights =Access.Rights.ReadWrite;
				else
					throw new Exception("Invalid Rights Specified");

				poBox.Commit(sub);
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
			POBox poBox = Simias.POBox.POBox.FindPOBox(store, 
				col.Domain, 
				store.GetUserIDFromDomainID(col.Domain));

			ICSList poList = poBox.Search(
				Subscription.SubscriptionCollectionIDProperty,
				col.ID,
				SearchOp.Equal);

			foreach(ShallowNode sNode in poList)
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

				members.Add( new iFolderUser( sub ) );
			}

			if ( members.Count > 0 )
			{
				UserComparer comparer = new UserComparer();
				members.Sort( 0, members.Count, comparer );
			}

			return ( iFolderUser[] ) ( members.ToArray( typeof( iFolderUser ) ) );
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
			SearchType operation, 
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


			// Use the POBox for the domain that this iFolder belongs to.
			POBox poBox = Simias.POBox.POBox.FindPOBox(store, 
						domain.ID, 
						store.GetUserIDFromDomainID(domain.ID));

			Subscription sub = poBox.CreateSubscription(col,
										col.GetCurrentMember(),
										"iFolder");

			sub.SubscriptionRights = newRights;
			sub.ToName = member.Name;
			sub.ToIdentity = MemberID;

			poBox.AddMessage(sub);

			iFolderUser user = new iFolderUser( sub );
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
		public iFolderWeb AcceptiFolderInvitation( string DomainID,
												   string iFolderID, 
												string LocalPath)
		{
			Store store = Store.GetStore();

			POBox poBox = Simias.POBox.POBox.FindPOBox(store, 
						DomainID, 
						store.GetUserIDFromDomainID(DomainID));

			// iFolders returned in the Web service are also
			// Subscriptions and it ID will be the subscription ID
			Node node = poBox.GetNodeByID(iFolderID);
			if(node == null)
				throw new Exception("Invalid iFolderID");

			Subscription sub = new Subscription(node);

			string path = Path.Combine(LocalPath, sub.DirNodeName);
			if (Directory.Exists(path))
				throw new Exception("Path specified cannot be an iFolder, a directory by this name already exists.");

			if(CanBeiFolder(path) == false)
				throw new Exception("Path specified cannot be an iFolder, it is either a parent or a child of an existing iFolder");

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
			Store store = Store.GetStore();

			Simias.POBox.POBox poBox = 
				Simias.POBox.POBox.GetPOBox( store, DomainID );

			// iFolders returned in the Web service are also
			// Subscriptions and it ID will be the subscription ID
			Node node = poBox.GetNodeByID(iFolderID);
			if(node == null)
				throw new Exception("Invalid iFolderID");

			Subscription sub = new Subscription(node);

			// Change the local subscription
			sub.SubscriptionState = SubscriptionStates.Replied;
			sub.SubscriptionDisposition = SubscriptionDispositions.Declined;

			poBox.Commit(sub);
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

				NetCredential cCreds =
					new NetCredential("iFolder", DomainID, true, member.Name,
								Password);
				if(cCreds == null)
					throw new Exception("ERROR: Updating NetCredentials");
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
		/// <param name = "localChangesWin">
		/// A bool that determines if the local or server copies win
		/// </param>
		[WebMethod(EnableSession=true, Description="Resolves a file conflict in an iFolder.")]
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

			FileSizeFilter filter = FileSizeFilter.Get(member, col);
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
		/// Checks to see if there is a newer client application on the domain server and
		/// prompts the user to upgrade.
		/// </summary>
		/// <param name="domainID">The ID of the domain to check for updates against.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		[WebMethod(Description="Check for an available update")]
		[SoapDocumentMethod]
		public string CheckForUpdatedClient(string domainID)
		{
			return Novell.iFolder.Install.ClientUpgrade.CheckForUpdate(domainID);
		}

		/// <summary>
		/// Gets the updated client application and runs the installation program.
		/// Note: This call will return before the application is updated.
		/// </summary>
		/// <param name="domainID">The ID of the domain to check for updates against.</param>
		/// <returns>True if the installation program is successfully started. Otherwise false is returned.</returns>
		[WebMethod(Description="Run the client update")]
		[SoapDocumentMethod]
		public bool RunClientUpdate(string domainID)
		{
			return Novell.iFolder.Install.ClientUpgrade.RunUpdate(domainID);
		}
	}
}
