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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Simias.Web;

namespace Simias.POBoxService.Web
{
	/// <summary>
	/// Status codes returned from POBoxService methods
	/// </summary>
	[Serializable]
	public enum POBoxStatus
	{
		/// <summary>
		/// The method was successful.
		/// </summary>
		Success,

		/// <summary>
		/// The specified PO Box was not found.
		/// </summary>
		UnknownPOBox,

		/// <summary>
		/// The specified identity was not found in the domain.
		/// </summary>
		UnknownIdentity,

		/// <summary>
		/// The specified subscription was not found.
		/// </summary>
		UnknownSubscription,

		/// <summary>
		/// The specified collection was not found.
		/// </summary>
		UnknownCollection,

		/// <summary>
		/// The specified domain was not found.
		/// </summary>
		UnknownDomain,

		/// <summary>
		/// The suscription was in an invalid state for the method
		/// </summary>
		InvalidState,

		/// <summary>
		/// The access rights were invalid during an inviate
		/// </summary>
		InvalidAccessRights,

		/// <summary>
		/// The subscription was already accepted by another client.
		/// </summary>
		AlreadyAccepted,

		/// <summary>
		/// The subscription was already denied by another client.
		/// </summary>
		AlreadyDeclined,

		/// <summary>
		/// The invitation has not moved to the posted state yet.
		/// </summary>
		NotPosted,

		/// <summary>
		/// An unknown error was realized.
		/// </summary>
		UnknownError
	};

	/// <summary>
	/// Object used for inviting, accepting/declining subscriptions etc.
	/// </summary>
	[Serializable]
	public class SubscriptionMsg
	{
		/// <summary>
		/// Domain to invite and accept on
		/// </summary>
		public string DomainID;

		/// <summary>
		/// The ID of the user who sent the subscription
		/// </summary>
		public string FromID;

		/// <summary>
		/// The name of the user who sent the subscription
		/// </summary>
		public string FromName;

		/// <summary>
		/// The ID of the user who received the subscription
		/// </summary>
		public string ToID;

		/// <summary>
		/// The ID of the originating subscription
		/// Subscription ID are consistent in both the
		/// sender's and receiver's PO Boxes
		/// </summary>
		public string SubscriptionID;

		/// <summary>
		/// The ID of the collection the sender is wanting
		/// to share.
		/// </summary>
		public string SharedCollectionID;

		/// <summary>
		/// The friendly name of the collection the sender
		/// is wanting to share
		/// </summary>
		public string SharedCollectionName;

		/// <summary>
		/// The type of collection the sender is wanting
		/// to share
		/// </summary>
		public string SharedCollectionType;

		/// <summary>
		/// If the shared collection contains a directory
		/// node, the id will be set on the invite
		/// </summary>
		public string DirNodeID;

		/// <summary>
		/// If the shared collection contains a directory
		/// node, the node's name will be set on the invite
		/// </summary>
		public string DirNodeName;

		/// <summary>
		/// Access rights the sender is wishing to grant
		/// to the receiver for the shared collection
		/// This member is really only valid on the
		/// invite method
		/// </summary>
		public int	AccessRights;
	};

	/// <summary>
	/// Summary description for POBoxService
	/// </summary>
	/// 
	[WebService(Namespace="http://novell.com/simias/pobox/")]
	public class POBoxService : System.Web.Services.WebService
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(POBoxService));

		/// <summary>
		/// </summary>
		public POBoxService()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		/// <summary>
		/// Checks to see if the specified identity is already a member of the collection.
		/// </summary>
		/// <param name="store">Store handle.</param>
		/// <param name="collectionID">Identifier for the collection.</param>
		/// <param name="identity">Identity to check for membership.</param>
		/// <returns>AlreadyAccepted is returned if the identity is already a member of the collection.</returns>
		private POBoxStatus AlreadyAMember( Store store, string collectionID, string identity )
		{
			POBoxStatus status;

			Collection collection = store.GetCollectionByID( collectionID );
			if ( collection != null )
			{
				status = ( collection.GetMemberByID( identity ) != null ) ? 
					POBoxStatus.AlreadyAccepted :
					POBoxStatus.UnknownSubscription;
			}
			else
			{
				status = POBoxStatus.UnknownCollection;
			}

			return status;
		}

		/// <summary>
		/// Ping
		/// Method for clients to determine if POBoxService is
		/// up and running.
		/// </summary>
		/// <param name="sleepFor"></param>
		/// <returns>0</returns>
		[WebMethod(EnableSession = true)]
		public int Ping(int sleepFor)
		{
			Thread.Sleep(sleepFor * 1000);
			return 0;
		}

		/// <summary>
		/// Accept subscription
		/// </summary>
		/// <param name="subMsg"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public POBoxStatus AcceptedSubscription( SubscriptionMsg subMsg )
		{
			POBoxStatus status = POBoxStatus.UnknownError;
			Store store = Store.GetStore();

			log.Debug( "AcceptedSubscription - called" );
			log.Debug( "  subscription ID: " + subMsg.SubscriptionID );
			log.Debug( "  collection ID: " + subMsg.SharedCollectionID );
			log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name );

			try
			{
				if ( subMsg.ToID != Thread.CurrentPrincipal.Identity.Name )
				{
					log.Error( "Specified \"toIdentity\" is not the caller" );
					return( POBoxStatus.UnknownIdentity );
				}

				// open the post office box
				POBox.POBox poBox = Simias.POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.FromID );
				if ( poBox != null )
				{
					// check that the message has already not been posted
					ICSList list = poBox.Search( Message.MessageIDProperty, subMsg.SubscriptionID, SearchOp.Equal );
					if ( list.Count == 0 )
					{
						// See if the toIdentity already exists in the memberlist of the shared collection
						log.Debug( "AcceptedSubscription - Subscription does not exist" );
						status = AlreadyAMember( store, subMsg.SharedCollectionID, subMsg.ToID );
					}
					else
					{
						// Subscription exists in the inviters PO box
						ICSEnumerator e = list.GetEnumerator() as ICSEnumerator; e.MoveNext();
						Subscription cSub = new Subscription( poBox, e.Current as ShallowNode );

						// Identities need to match up
						if ( ( subMsg.FromID == cSub.FromIdentity ) && ( subMsg.ToID == cSub.ToIdentity ) )
						{
							switch ( cSub.SubscriptionState )
							{
								case SubscriptionStates.Invited:
								{
									// Wait for the sender's subscription to be posted.
									status = POBoxStatus.NotPosted;
									break;
								}

								case SubscriptionStates.Posted:
								{
									// Accepted. Next state = Responded and disposition is set.
									cSub.Accept( store, cSub.SubscriptionRights );
									poBox.Commit( cSub );
									status = POBoxStatus.Success;
									break;
								}

								case SubscriptionStates.Responded:
								{
									// The subscription has already been accepted or declined.
									status = 
										( cSub.SubscriptionDisposition == SubscriptionDispositions.Accepted ) ? 
										POBoxStatus.AlreadyAccepted :
										POBoxStatus.AlreadyDeclined;
									break;
								}

								default:
								{
									log.Debug( "  invalid accept state = {0}", cSub.SubscriptionState.ToString() );
									status = POBoxStatus.InvalidState;
									break;
								}
							}
						}
						else
						{
							log.Debug( "  to or from identity does not match" );
							status = POBoxStatus.UnknownIdentity;
						}

						e.Dispose();
					}
				}
				else
				{
					status = POBoxStatus.UnknownPOBox;
				}
			}
			catch(Exception e)
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}
			
			log.Debug( "AcceptedSubscription exit  status: " + status.ToString() );
			return status;
		}

		/// <summary>
		/// Decline subscription
		/// </summary>
		/// <param name="subMsg"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public POBoxStatus DeclinedSubscription( SubscriptionMsg subMsg )
		{
			POBoxStatus status;

			log.Debug( "DeclinedSubscription - called" );
			log.Debug( "  subscription ID: " + subMsg.SubscriptionID );
			log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name );

			if ( subMsg.ToID != Thread.CurrentPrincipal.Identity.Name )
			{
				log.Error( "  specified \"toIdentity\" is not the caller" );
				return POBoxStatus.UnknownIdentity;
			}

			// open the post office box of the To user
			Store store = Store.GetStore();
			POBox.POBox toPOBox = Simias.POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.ToID );
			if ( toPOBox != null )
			{
				// Get the subscription from the caller's PO box
				ICSList list = toPOBox.Search( Message.MessageIDProperty, subMsg.SubscriptionID, SearchOp.Equal);
				if ( list.Count == 0 )
				{
					// Assume that the subscription has already been declined and cleaned up by
					// a different client. Just return successfully from here.
					log.Debug( "  subscription: " + subMsg.SubscriptionID + " does not exist");
					status = POBoxStatus.Success;
				}
				else
				{
					// Get the subscription object
					ICSEnumerator e = list.GetEnumerator() as ICSEnumerator; e.MoveNext();
					Subscription cSub = new Subscription( toPOBox, e.Current as ShallowNode );

					// The subscription must be in the Replied, Received or Ready State to decline.
					if ( ( cSub.SubscriptionState == SubscriptionStates.Replied ) || 
						 ( cSub.SubscriptionState == SubscriptionStates.Received ) ||
						 ( cSub.SubscriptionState == SubscriptionStates.Ready ) )
					{
						// Identities need to match up
						if ( ( subMsg.FromID == cSub.FromIdentity ) && ( subMsg.ToID == cSub.ToIdentity ) )
						{
							// Validate the shared collection
							Collection cCol = store.GetCollectionByID( cSub.SubscriptionCollectionID );
							if ( cCol != null )
							{
								//
								// Actions taken when a subscription is declined
								//
								// If I'm the owner of the shared collection then the
								// decline is treated as a delete of the shared collection
								// so we must.
								// 1) Delete all the subscriptions in all members PO boxes
								// 2) Delete all outstanding subscriptions assigned to the
								//    shared collection ID.
								// 3) Delete the shared collection itself
								//
								// If I'm already a member of the shared collection but not
								// the owner.
								// 1) Remove myself from the member list of the shared collection
								// 2) Delete my subscription to the shared collection
								//
								// If I'm not yet a member of the shared collection but declined
								// an invitation from another user in the system.  In this case
								// the From and To identies will be different.
								// 1) Delete my subscription to the shared collection.
								// 2) Set the state of the subscription in the inviter's PO
								//    Box to "declined".
								//

								Member toMember = cCol.GetMemberByID( subMsg.ToID );
								if ( toMember == null )
								{
									log.Debug( "  handling case where identity is declining a subscription" );

									// I am not a member of this shared collection and want to decline the subscription.
									// Open the post office box of the from and decline the subscription
									POBox.POBox fromPOBox = 
										POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.FromID );

									if ( fromPOBox != null )
									{
										Subscription cFromMemberSub = 
											fromPOBox.GetSubscriptionByCollectionID( cCol.ID, subMsg.ToID );
										if( cFromMemberSub != null )
										{
											log.Debug(  "declining subscription in fromPOBox." );
											cFromMemberSub.Decline();
											fromPOBox.Commit( cFromMemberSub );
										}
									}

									// Remove the subscription from the "toIdentity" PO box
									log.Debug( "  removing subscription from toPOBox." );
									toPOBox.Commit( toPOBox.Delete( cSub ) );
								}
								else if ( toMember.IsOwner )
								{
									// I am the owner of the shared collection?
									log.Debug( "  handling case where identity is owner of collection" );

									ICSList memberlist = cCol.GetMemberList();
									foreach( ShallowNode sNode in memberlist )
									{
										Member cMember = new Member( cCol, sNode );

										// Get the member's POBox
										POBox.POBox memberPOBox = 
											POBox.POBox.FindPOBox(
											store, 
											cCol.Domain, 
											cMember.UserID );

										if ( memberPOBox != null )
										{
											// Search for the matching subscription
											Subscription memberSub = 
												memberPOBox.GetSubscriptionByCollectionID( cCol.ID, cMember.UserID );

											if( memberSub != null )
											{
												log.Debug( "  removing invitation from toPOBox." );
												memberPOBox.Commit( memberPOBox.Delete( memberSub ) );
											}
										}
									}

									// Now search for all nodes that contain the "sbColID" property
									// which will find all subscriptions to users that have not
									// accepted or declined the subscription

									Property sbProp = new Property( "SbColID", cCol.ID );
									ICSList subList = store.GetNodesByProperty( sbProp, SearchOp.Equal );
									foreach ( ShallowNode sn in subList )
									{
										Collection col = store.GetCollectionByID( sn.CollectionID );
										if ( col != null )
										{
											Subscription sub = new Subscription( col, sn );
											if ( sub != null )
											{
												col.Commit( col.Delete( sub ) );
											}
										}
									}

									// Delete the shared collection itself
									log.Debug( "  deleting shared collection." );
									cCol.Commit( cCol.Delete() );
								}
								else
								{
									// I am a member of the shared collection.
									log.Debug( "  handling case where identity is a member of the collection" );
									cCol.Commit( cCol.Delete( toMember ) );

									// Remove the subscription from the "toIdentity" PO box
									Subscription cMemberSub = 
										toPOBox.GetSubscriptionByCollectionID( cCol.ID, toMember.UserID );
									if( cMemberSub != null )
									{
										log.Debug( "  removing subscription from owner's POBox." );
										toPOBox.Commit( toPOBox.Delete( cMemberSub ) );
									}

									if ( subMsg.FromID != subMsg.ToID )
									{
										// open the post office box of the From user
										POBox.POBox fromPOBox = 
											POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.FromID ); 

										if ( fromPOBox != null )
										{
											// Remove the subscription from the "fromIdentity" PO box
											Subscription cFromMemberSub = 
												fromPOBox.GetSubscriptionByCollectionID( cCol.ID, toMember.UserID );

											if( cFromMemberSub != null )
											{
												log.Debug( "  removing subscription from toPOBox." );
												fromPOBox.Commit( fromPOBox.Delete( cFromMemberSub ) );
											}
										}
									}
								}

								status = POBoxStatus.Success;
							}
							else
							{
								// The shared collection does not exist but
								// if any subscriptions are still associated 
								// delete them from the store

								Property sbProp = new Property( "SbColID", cCol.ID );
								ICSList subList = store.GetNodesByProperty( sbProp, SearchOp.Equal );
								foreach ( ShallowNode sn in subList )
								{
									// Collection should be a PO Box
									Collection col = store.GetCollectionByID( sn.CollectionID );
									if ( col != null )
									{
										Subscription sub = new Subscription( col, sn );
										if ( sub != null )
										{
											col.Commit( col.Delete( sub ) );
										}
									}
								}

								log.Debug( "  collection not found" );
								status = POBoxStatus.UnknownCollection;
							}
						}
						else
						{
							log.Debug( "  from or to identity does not match" );
							status = POBoxStatus.UnknownIdentity;
						}

						e.Dispose();
					}
					else
					{
						// The Delivered state is the only other state that the subscription can be in. If
						// it is then it has already been accepted by another client.
						log.Debug( "  subscription has already been accepted." );
						status = POBoxStatus.AlreadyAccepted;
					}
				}
			}
			else
			{
				status = POBoxStatus.UnknownPOBox;
			}

			log.Debug( "DeclinedSubscription exit  status: " + status.ToString() );
			return status;
		}

		/// <summary>
		/// Acknowledge the subscription.
		/// </summary>
		/// <param name="subMsg"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		AckSubscription( SubscriptionMsg subMsg )
		{
			POBoxStatus status;

			log.Debug( "Acksubscription - called" );
			log.Debug( "  subscription: " + subMsg.SubscriptionID );
			log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name);

			if ( subMsg.ToID != Thread.CurrentPrincipal.Identity.Name )
			{
				log.Error( "specified \"toIdentity\" is not the caller" );
				return POBoxStatus.UnknownIdentity;
			}

			// open the post office box
			Store store = Store.GetStore();
			POBox.POBox poBox = Simias.POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.FromID );
			if ( poBox != null )
			{
				ICSList list = poBox.Search( Message.MessageIDProperty, subMsg.SubscriptionID, SearchOp.Equal);
				if ( list.Count == 0 )
				{
					log.Debug( "  subscription: " + subMsg.SubscriptionID + " does not exist");

					// See if the toIdentity already exists in the memberlist of the shared collection.  If he has 
					// already accepted he can't decline from another machine.
					status = AlreadyAMember( store, subMsg.SharedCollectionID, subMsg.ToID );
				}
				else
				{
					// get the subscription object
					ICSEnumerator e = list.GetEnumerator() as ICSEnumerator; e.MoveNext();
					Subscription cSub = new Subscription( poBox, e.Current as ShallowNode );

					// Must be in the Responded state.
					switch ( cSub.SubscriptionState )
					{
						case SubscriptionStates.Responded:
						{
							// Identities need to match up
							if ( ( subMsg.FromID == cSub.FromIdentity ) && ( subMsg.ToID == cSub.ToIdentity ) )
							{
								// Delete the subscription from the inviters PO box.
								poBox.Commit( poBox.Delete(cSub) );
								status = POBoxStatus.Success;;
							}
							else
							{
								log.Debug( "  to or from identity does not match" );
								status = POBoxStatus.UnknownIdentity;
							}
							break;
						}

						default:
						{
							log.Debug( "  invalid state = {0}", cSub.SubscriptionState.ToString() );
							status = POBoxStatus.InvalidState;
							break;
						}
					}
				}
			}
			else
			{
				log.Debug("  PO Box not found");
				status = POBoxStatus.UnknownPOBox;
			}

			log.Debug( "AckSubscription exit  status: " + status.ToString() );
			return status;
		}

		/// <summary>
		/// Get the subscription information
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="subscriptionID"></param>
		/// <param name="collectionID"></param>
		/// <returns>success:subinfo  failure:null</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		SubscriptionInformation 
		GetSubscriptionInfo(string domainID, string identityID, string subscriptionID, string collectionID)
		{
			SubscriptionInformation subInfo = new SubscriptionInformation();

			log.Debug("GetSubscriptionInfo - called");
			log.Debug("  for subscription: " + subscriptionID);

			// open the post office box
			Store store = Store.GetStore();
			POBox.POBox poBox =	Simias.POBox.POBox.FindPOBox(store, domainID, identityID);
			if (poBox != null)
			{
				ICSList list = poBox.Search( Message.MessageIDProperty, subscriptionID, SearchOp.Equal );
				if ( list.Count == 0 )
				{
					log.Debug("  subscription does not exist");
					subInfo.Status = AlreadyAMember( store, collectionID, identityID );
				}
				else
				{
					// Generate the subscription info object and return it
					ICSEnumerator e = list.GetEnumerator() as ICSEnumerator; e.MoveNext();
					Subscription cSub = new Subscription(poBox, e.Current as ShallowNode);

					switch ( cSub.SubscriptionState )
					{
						case SubscriptionStates.Responded:
						{
							// Validate the shared collection
							Collection cSharedCollection = store.GetCollectionByID(cSub.SubscriptionCollectionID);
							if (cSharedCollection != null)
							{
								subInfo.GenerateFromSubscription(cSub);
								subInfo.Status = POBoxStatus.Success;
							}
							else
							{
								log.Debug("  collection not found");
								subInfo.Status = POBoxStatus.UnknownCollection;
							}
							break;
						}

						default:
						{
							log.Debug( "  invalid state = {0}", cSub.SubscriptionState.ToString() );
							subInfo.Status = POBoxStatus.InvalidState;
							break;
						}
					}
				}
			}
			else
			{
				log.Debug("  getSubscriptionInfo - PO Box not found");
				subInfo.Status = POBoxStatus.UnknownPOBox;
			}

			log.Debug( "GetSubscriptionInfo exit  status: " + subInfo.Status.ToString() );
			return subInfo;
		}

		/// <summary>
		/// Invite a user to a shared collection
		/// </summary>
		/// <param name="subMsg"></param>
		/// <returns>True if successful. False if not.</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		Invite( SubscriptionMsg subMsg )
		{
			POBoxStatus			status = POBoxStatus.UnknownError;
			Store				store = Store.GetStore();
			Collection			sharedCollection = null;
			Simias.POBox.POBox	poBox = null;
			Subscription		cSub = null;

			// Verify domain
			Domain cDomain = store.GetDomain( subMsg.DomainID );
			if ( cDomain == null )
			{
				log.Debug( "  invalid Domain ID!" );
				return POBoxStatus.UnknownDomain;
			}

			log.Debug( "Invite - called" );
			log.Debug( "  DomainID: " + subMsg.DomainID );
			log.Debug( "  FromUserID: " + subMsg.FromID );
			log.Debug( "  ToUserID: " + subMsg.ToID );

			// Verify the fromMember is the caller
			log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name );

			// Verify and get additional information about the "To" user
			Member toMember = cDomain.GetMemberByID( subMsg.ToID );
			if ( toMember == null )
			{
				log.Debug( "  specified \"toUserID\" does not exist in the domain!" );
				return POBoxStatus.UnknownIdentity;
			}

			// In peer-to-peer the collection won't exist 
			sharedCollection = store.GetCollectionByID( subMsg.SharedCollectionID ); 
			if ( sharedCollection == null )
			{
				log.Debug( "  shared collection does not exist" );
			}

			// Don't check for the fromMember in the domain if this is workgroup.
			if ( cDomain.ConfigType != Domain.ConfigurationType.Workgroup )
			{
				Member fromMember = cDomain.GetMemberByID( subMsg.FromID );
				if ( fromMember != null )
				{
					// Check that the sender has sufficient rights to invite.
					if ( sharedCollection != null )
					{
						Member collectionMember = sharedCollection.GetMemberByID( fromMember.UserID );
						if ( ( collectionMember == null ) || ( collectionMember.Rights != Access.Rights.Admin ) )
						{
							log.Debug( " sender does not have rights to invite to this collection." );
							return POBoxStatus.InvalidAccessRights;
						}
					}
					else
					{
						// The collection must exist in enterprise.
						log.Debug( " shared collection does not exist on enterprise" );
						return POBoxStatus.UnknownCollection;
					}
				}
				else
				{
					log.Debug( "  specified \"fromUserID\" does not exist in the domain!" );
					return POBoxStatus.UnknownIdentity;
				}
			}

			if ( subMsg.AccessRights > (int) Simias.Storage.Access.Rights.Admin)
			{
				return POBoxStatus.InvalidAccessRights;
			}

			try
			{
				log.Debug( "  looking up POBox for: " + subMsg.ToID );
				poBox = POBox.POBox.GetPOBox( store, subMsg.DomainID, subMsg.ToID );

				cSub = 
					new Subscription( 
					subMsg.SharedCollectionName + " subscription", 
					"Subscription", 
					subMsg.FromID );

				cSub.SubscriptionState = Simias.POBox.SubscriptionStates.Received;
				cSub.ToName = toMember.Name;
				cSub.ToIdentity = subMsg.ToID;
				cSub.FromName = subMsg.FromName;
				cSub.FromIdentity = subMsg.FromID;
				cSub.SubscriptionRights = (Simias.Storage.Access.Rights) subMsg.AccessRights;
				cSub.MessageID = subMsg.SubscriptionID;

				string appPath = this.Context.Request.ApplicationPath.TrimStart( new char[] {'/'} );
				appPath += "/POBoxService.asmx";

				log.Debug("  application path: " + appPath);

				cSub.SubscriptionCollectionID = subMsg.SharedCollectionID;
				cSub.SubscriptionCollectionType = subMsg.SharedCollectionType;
				cSub.SubscriptionCollectionName = subMsg.SharedCollectionName;
				cSub.DomainID = cDomain.ID;
				cSub.DomainName = cDomain.Name;
				cSub.SubscriptionKey = Guid.NewGuid().ToString();
				cSub.MessageType = "Outbound";  // ????

				if ( sharedCollection != null )
				{
					DirNode dirNode = sharedCollection.GetRootDirectory();
					if( dirNode != null )
					{
						cSub.DirNodeID = dirNode.ID;
						cSub.DirNodeName = dirNode.Name;
					}
				}
				else
				{
					cSub.DirNodeID = subMsg.DirNodeID;
					cSub.DirNodeName = subMsg.DirNodeName;
				}

				poBox.Commit( cSub );
				status = POBoxStatus.Success;
			}
			catch(Exception e)
			{
				log.Error("  failed creating subscription");
				log.Error(e.Message);
				log.Error(e.StackTrace);
			}

			log.Debug( "Invite - exit" );
			return status;
		}

/* DEADCODE 
		/// <summary>
		/// Return the Default Domain
		/// </summary>
		/// <param name="dummy">Dummy parameter so stub generators won't produce empty structures</param>
		/// <returns>default domain</returns>
		[WebMethod(EnableSession = true)]
		public string GetDefaultDomain(int dummy)
		{
			return(Store.GetStore().DefaultDomain);
		}
*/		
	}

	/// <summary>
	/// </summary>
	[Serializable]
	public class SubscriptionInformation
	{
		/// <summary>
		/// </summary>
		public string   Name;
		/// <summary>
		/// </summary>
		public string	MsgID;
		/// <summary>
		/// </summary>
		public string	FromID;
		/// <summary>
		/// </summary>
		public string	FromName;
		/// <summary>
		/// </summary>
		public string	ToID;
		/// <summary>
		/// </summary>
		public string	ToNodeID;
		/// <summary>
		/// </summary>
		public string	ToName;
		/// <summary>
		/// </summary>
		public int		AccessRights;

		/// <summary>
		/// </summary>
		public string	CollectionID;
		/// <summary>
		/// </summary>
		public string	CollectionName;
		/// <summary>
		/// </summary>
		public string	CollectionType;

		/// <summary>
		/// </summary>
		public string	DirNodeID;
		/// <summary>
		/// </summary>
		public string	DirNodeName;

		/// <summary>
		/// </summary>
		public string	DomainID;
		/// <summary>
		/// </summary>
		public string	DomainName;

		/// <summary>
		/// </summary>
		public int		State;
		/// <summary>
		/// </summary>
		public int		Disposition;

		public POBoxStatus Status;

		/// <summary>
		/// </summary>
		public SubscriptionInformation()
		{

		}

		internal void GenerateFromSubscription(Subscription cSub)
		{
			this.Name = cSub.Name;
			this.MsgID = cSub.MessageID;
			this.FromID = cSub.FromIdentity;
			this.FromName = cSub.FromName;
			this.ToID = cSub.ToIdentity;
			this.ToNodeID = cSub.ToMemberNodeID;
			this.ToName = cSub.ToName;
			this.AccessRights = (int) cSub.SubscriptionRights;

			this.CollectionID = cSub.SubscriptionCollectionID;
			this.CollectionName = cSub.SubscriptionCollectionName;
			this.CollectionType = cSub.SubscriptionCollectionType;

			this.DirNodeID = cSub.DirNodeID;
			this.DirNodeName = cSub.DirNodeName;

			this.DomainID = cSub.DomainID;
			this.DomainName = cSub.DomainName;

			this.State = (int) cSub.SubscriptionState;
			this.Disposition = (int) cSub.SubscriptionDisposition;
		}
	}
}
