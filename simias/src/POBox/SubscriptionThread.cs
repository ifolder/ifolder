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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using System.Runtime.Remoting;

using Simias;
using Simias.Authentication;
using Simias.Storage;
using Simias.Sync;
using Simias.Sync.Client;
using Simias.Mail;

namespace Simias.POBox
{
	/// <summary>
	/// SubscriptionThread
	/// </summary>
	public class SubscriptionThread
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SubscriptionThread));
		private static readonly string	poServiceLabel = "/POService.asmx";
		
		private POBox poBox;
		private Subscription subscription;
		private Hashtable threads;
		private string poServiceUrl;

		/// <summary>
		/// Constructor
		/// </summary>
		public SubscriptionThread(POBox poBox, Subscription subscription, Hashtable threads)
		{
			this.poBox = poBox;
			this.subscription = subscription;
			this.threads = threads;

			if (subscription.SubscriptionCollectionURL != null && 
				subscription.SubscriptionCollectionURL != "")
			{
				this.poServiceUrl = subscription.SubscriptionCollectionURL + poServiceLabel;
			}
			else
			{
				Collection cCollection = 
					poBox.StoreReference.GetCollectionByID(subscription.SubscriptionCollectionID); 
				if (cCollection == null)
				{
					throw new ApplicationException("Invalid shared collection ID");
				}

				SyncCollection sc = new SyncCollection(cCollection);
				poServiceUrl = sc.MasterUrl.ToString() + poServiceLabel;
			}
		}

		/// <summary>
		/// Thread Run
		/// </summary>
		public void Run()
		{
			bool done = false;

			try
			{
				while(!done)
				{
					try
					{
						switch(subscription.SubscriptionState)
						{
							// invited (master)
							case SubscriptionStates.Invited:
								done = DoInvited();
								break;

							// TODO: fix and cleanup states
							//case SubscriptionStates.Pending:

							// replied (slave)
							case SubscriptionStates.Replied:
								done = DoReplied();
								break;

							// delivered (slave)
							case SubscriptionStates.Delivered:
								done = DoDelivered();
								break;

							default:
								break;
						}
					}
					catch(Exception e)
					{
						done = false;
						log.Debug(e, "Ignored");
					}

					if (!done)
					{
						Thread.Sleep(TimeSpan.FromSeconds(10));
					}
				}
			}
			catch(Exception e)
			{
				log.Debug(e, "Ignored");
			}
			finally
			{
				lock(threads.SyncRoot)
				{
					threads.Remove(subscription.ID);
				}
			}
		}

		private bool DoInvited()
		{
			bool result = true;
			if (poBox.Domain == Simias.Storage.Domain.WorkGroupDomainID)
			{
				// TODO: Localize
			
				MailMessage message = new MailMessage();

				message.BodyFormat = MailFormat.Text;

				message.From = subscription.FromAddress;
				message.To = subscription.ToAddress;

				message.Subject =
					String.Format("Shared iFolder Invitation - {0}", subscription.SubscriptionCollectionName);
			
				// body
				StringBuilder buffer = new StringBuilder();
			
				buffer.AppendFormat("{0} invites you to participate in the shared iFolder named \"{1}\".\n",
					subscription.FromName, subscription.SubscriptionCollectionName);

				// TODO: Owner currently cannot be resolved to a friendly name.  Also need to add "Novell iFolder server" for enterprise.
				// buffer.AppendFormat("This iFolder is hosted by {0} on a personal computer.\n\n", invitation.Owner);
				buffer.Append("\n");

				if (subscription.CollectionDescription != null)
				{
					buffer.AppendFormat("{0}\n\n", subscription.CollectionDescription);
				}

				string rights;
				switch (subscription.SubscriptionRights)
				{
					case Storage.Access.Rights.Admin:
						rights = "Full Control";
						break;
					case Storage.Access.Rights.ReadWrite:
						rights = "Read/Write";
						break;
					default:
						rights = "Read Only";
						break;
				}

				buffer.AppendFormat("{0} assigned you {1} rights to this shared iFolder.\n\n",
					subscription.FromName, rights);

				buffer.Append("You can participate from one or more computers with the iFolder client. For information or download, see the iFolder Web site at http://www.ifolder.com. \n\n");

				buffer.Append("To accept and set up the shared iFolder on this computer, open the attached Collection Subscription Information (CSI) file. Repeat this process on each computer where you want to set up the shared iFolder.\n\n");

				buffer.Append("If you do not accept within 7 days, the invitation  automatically expires. To decline immediately, open the CSI file and select Decline.");

				message.Body = buffer.ToString();

				// invitation attachment
				string filename = Path.Combine(Path.GetTempPath(),
					subscription.SubscriptionCollectionName + "_" + subscription.FromName
					+ SubscriptionInfo.Extension);

				SubscriptionInfo info = subscription.GenerateInfo(poBox.StoreReference);
				info.Save(filename);

				MailAttachment attachment = new MailAttachment(filename);
			
				message.Attachments.Add(attachment);

				// send
				SmtpMail.Send(message);

				// delete invitation file
				File.Delete(filename);

				log.Info("Invitation Sent: {0}", info);

				// update subscription
				subscription.SubscriptionState = SubscriptionStates.Posted;
				poBox.Commit(subscription);
			}
			else
			{
				log.Info("SubscriptionThread::DoInvited called");
				POBoxService poService = new POBoxService();
				poService.Url = this.poServiceUrl;
				poService.PreAuthenticate = true;
				poService.CookieContainer = new CookieContainer();
				Credentials cSimiasCreds = 
					new Credentials(subscription.SubscriptionCollectionID);
				poService.Credentials = cSimiasCreds.GetCredentials();
				if (poService.Credentials == null)
				{
					log.Info("  no credentials - back to sleep");
					return (false);
				}
				

				//
				// Make sure the shared collection has sync'd to the server before inviting
				//

				Collection cSharedCollection = 
					Store.GetStore().GetCollectionByID(subscription.SubscriptionCollectionID);
				if (cSharedCollection == null)
				{
					return (false);
				}

				if (cSharedCollection.MasterIncarnation == 0)
				{
					log.Debug(
						"Failed POBoxService::Invite - collection: {0} hasn't sync'd to the server yet",
						subscription.SubscriptionCollectionID);
					return (false);
				}

				//
				// Make sure the subscription node has sync'd up to the server as well
				//

				Node cNode = this.poBox.Refresh(subscription);
				if (cNode.MasterIncarnation == 0)
				{
					// force the PO box to be sync'd right away
					SyncClient.ScheduleSync(poBox.ID);

					log.Debug(
						"Failed POBoxService::Invite - inviter's subscription {0} hasn't sync'd to the server yet",
						subscription.MessageID);


					return (false);
				}
			
				/*
				POBoxStatus wsStatus;
				try
				{
					wsStatus = POBoxStatus.UnknownError;
					wsStatus =
						poService.VerifyCollection(
							subscription.DomainID,
							subscription.SubscriptionCollectionID);
				}
				catch(Exception e)
				{
					log.Debug("failed verifying remote collection");
					log.Debug(e.Message);
					log.Debug(e.StackTrace);
				}

				if (wsStatus != POBoxStatus.Success)
				{
					log.Debug(
						"Failed POBoxService::Invite - collection: {0} hasn't sync'd to the server yet",
						subscription.SubscriptionCollectionID);
					log.Debug("POBoxStatus: " + wsStatus.ToString());
					return (false);
				}
				*/

				// This is an enterprise pobox contact the POService.
				log.Debug("Connecting to the Post Office Service : {0}", subscription.POServiceURL);

				try
				{
					// Set the remote state to received.
					// And post the subscription to the server.
					Simias.Storage.Member me = poBox.GetCurrentMember();
					subscription.FromIdentity = me.UserID;
					subscription.FromName = me.Name;

					string subID =
						poService.Invite(
							subscription.DomainID,
							subscription.FromIdentity,
							subscription.ToIdentity,
							subscription.SubscriptionCollectionID,
							subscription.SubscriptionCollectionType,
							(int) subscription.SubscriptionRights);
					if (subID != null && subID != "")
					{
						// FIXME:: sync my PostOffice right now!
						subscription.SubscriptionState = SubscriptionStates.Posted;
						subscription.MessageID = subID;
						poBox.Commit(subscription);
					}
					else
					{
						log.Debug("Failed the remote invite call");
						result = false;
					}
				}
				catch
				{
					log.Debug("Failed POBoxService::Invite - target: " + poService.Url);
					result = false;
				}
			}

			return result;
		}

		private bool DoReplied()
		{
			log.Info("DoReplied - Connecting to the Post Office Service : {0}", subscription.POServiceURL);
			log.Info("  calling the PO Box server to accept/reject subscription");
			log.Info("  domainID: " + subscription.DomainID);
			log.Info("  fromID:   " + subscription.FromIdentity);
			log.Info("  toID:     " + subscription.ToIdentity);
			log.Info("  SubID:    " + subscription.MessageID);

			bool result = false;
			POBoxService poService = new POBoxService();
			poService.CookieContainer = new CookieContainer();
			poService.PreAuthenticate = true;
			poService.Url = this.poServiceUrl;
			POBoxStatus	wsStatus = POBoxStatus.UnknownError;

			// Get credentials for the request
			Credentials cSimiasCreds = 
				new Credentials(subscription.DomainID, subscription.ToIdentity);
			poService.Credentials = cSimiasCreds.GetCredentials();
			if (poService.Credentials == null)
			{
				log.Info("  no credentials - back to sleep");
				return result;
			}

			try
			{
				if (subscription.SubscriptionDisposition == SubscriptionDispositions.Accepted)
				{
					log.Info("  subscription accepted!");
					wsStatus =
						poService.AcceptedSubscription(
							subscription.DomainID,
							subscription.FromIdentity,
							subscription.ToIdentity,
							subscription.MessageID);

					// update local subscription
					if (wsStatus == POBoxStatus.Success)
					{
						subscription.SubscriptionState = SubscriptionStates.Delivered;
						poBox.Commit(subscription);
					}
					else
					if (wsStatus == POBoxStatus.UnknownSubscription)
					{
						log.Debug("Failed accepting/declining a subscription");
						log.Debug("The subscription did not exist on the server");
						log.Debug("Deleting the local subscription");

						poBox.Commit(poBox.Delete(subscription));

						// return true so the thread controlling the
						// subscription will die off
						result = true;
					}
					else
					{
						log.Debug(
							"Failed Accepting/Declining a subscription.  Status: " + 
							wsStatus.ToString());
					}
				}
				else
				if (subscription.SubscriptionDisposition == SubscriptionDispositions.Declined)
				{
					log.Info("  subscription declined");
					wsStatus =
						poService.DeclinedSubscription(
							subscription.DomainID,
							subscription.FromIdentity,
							subscription.ToIdentity,
							subscription.MessageID);

					if (wsStatus == POBoxStatus.Success)
					{
						// This subscription is done!
						result = true;
					}
					else
					if (wsStatus == POBoxStatus.UnknownCollection)
					{
						log.Debug("Failed declining a subscription");
						log.Debug("The Collection did not exist on the server");
						log.Debug("Deleting the local subscription");

						poBox.Commit(poBox.Delete(subscription));
						result = true;
					}
				}
			}
			catch(Exception e)
			{
				log.Debug("DoReplied failed updating originator's PO box");
				log.Debug(e.Message);
				log.Debug(e.StackTrace);
			}

			return result;
		}

		private bool DoDelivered()
		{
			bool result = false;

			log.Info("DoDelivered::Connecting to the Post Office Service : {0}", this.poServiceUrl);
			log.Info("  calling the PO Box server to get subscription state");
			log.Info("  domainID: " + subscription.DomainID);
			log.Info("  fromID:   " + subscription.FromIdentity);
			log.Info("  SubID:    " + subscription.MessageID);

			POBoxService poService = new POBoxService();
			poService.Url = this.poServiceUrl;
			poService.PreAuthenticate = true;
			poService.CookieContainer = new CookieContainer();

			Credentials cSimiasCreds = 
				new Credentials(subscription.DomainID, subscription.ToIdentity);
			poService.Credentials = cSimiasCreds.GetCredentials();
			if (poService.Credentials == null)
			{
				log.Info("  no credentials - back to sleep");
				return(result);
			}

			try
			{
				SubscriptionInformation subInfo =
					poService.GetSubscriptionInfo(
						subscription.DomainID,
						subscription.FromIdentity,
						subscription.MessageID);

				if (subInfo != null)
				{
					log.Info("  subInfo.FromName: " + subInfo.FromName);
					log.Info("  subInfo.ToName: " + subInfo.ToName);
					log.Info("  subInfo.State: " + subInfo.State.ToString());
					log.Info("  subInfo.Disposition: " + subInfo.Disposition.ToString());
				}

				// update subscription
				if (subInfo.State == (int) SubscriptionStates.Responded)
				{
					// create proxy
					if (subInfo.Disposition == (int) SubscriptionDispositions.Accepted)
					{
						log.Debug("Creating collection...");

						// do not re-create the proxy
						if (poBox.StoreReference.GetCollectionByID(subscription.SubscriptionCollectionID) == null)
						{
							SubscriptionDetails details = new SubscriptionDetails();
							details.DirNodeID = subInfo.DirNodeID;
							details.DirNodeName = subInfo.DirNodeName;
							details.CollectionUrl = subInfo.CollectionUrl;

							log.Debug("Collection URL: " + subInfo.CollectionUrl);

							subscription.SubscriptionRights = 
								(Simias.Storage.Access.Rights) subInfo.AccessRights;

							// save details
							subscription.AddDetails(details);
							poBox.Commit(subscription);
					
							// create slave stub
							subscription.ToMemberNodeID = subInfo.ToNodeID;
							subscription.CreateSlave(poBox.StoreReference);
						}

						// acknowledge the message
						// which removes the originator's 
						POBoxStatus wsStatus =
							poService.AckSubscription(
								subscription.DomainID,
								subscription.FromIdentity, 
								subscription.ToIdentity,
								subscription.MessageID);

						if (wsStatus == POBoxStatus.Success)
						{
							// done with the subscription - move to local subscription to the ready state
							subscription.SubscriptionState = SubscriptionStates.Ready;
							poBox.Commit(subscription);
						}
						else
						if (wsStatus == POBoxStatus.UnknownSubscription)
						{
							log.Debug("Failed acknowledging a subscription");
							log.Debug("The subscription did not exist on the server");
							log.Debug("Deleting the local subscription");

							poBox.Commit(poBox.Delete(subscription));

							// return true so the thread controlling the
							// subscription will die off
							result = true;
						}
						else
						{
							log.Debug(
								"Failed Acking a subscription.  Status: " + 
								wsStatus.ToString());
						}
					}
					else
					{
						// Remove the subscription from the local PO box
						poBox.Commit(poBox.Delete(subscription));
					}

					// done
					result = true;
				}
			}
			catch(Exception e)
			{
				log.Debug("SubscriptionThread::DoDelivered failed with an exception");
				log.Debug(e.Message);
				log.Debug(e.StackTrace);
			}

			return result;
		}
	}
}
