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
using System.Threading;
using System.Text;
using System.IO;
using System.Runtime.Remoting;

using Simias;
using Simias.Mail;

namespace Simias.POBox
{
	/// <summary>
	/// SubscriptionThread
	/// </summary>
	public class SubscriptionThread
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SubscriptionThread));
		
		private POBox poBox;
		private Subscription subscription;
		private Hashtable threads;

		/// <summary>
		/// Constructor
		/// </summary>
		public SubscriptionThread(POBox poBox, Subscription subscription, Hashtable threads)
		{
			this.poBox = poBox;
			this.subscription = subscription;
			this.threads = threads;
		}

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
							case SubscriptionStates.Pending:

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
				// This is an enterprise pobox contact the POService.
				log.Debug("Connecting to the Post Office Service : {0}", subscription.POServiceURL);
				PostOffice po = (PostOffice)Activator.GetObject(typeof(PostOffice),
					subscription.POServiceURL.ToString());
			
				if (po == null)
					throw new ApplicationException("No Post-Office Service");

				// Set the remote state to received.
				// And post the subscription to the server.
				Simias.Storage.Member me = poBox.GetCurrentMember();
				subscription.FromIdentity = me.UserID;
				subscription.FromName = me.Name;
				subscription.SubscriptionState = SubscriptionStates.Received;
				po.Post(subscription.FromIdentity, subscription);

				subscription.SubscriptionState = SubscriptionStates.Posted;
				poBox.Commit(subscription);
			}

			return true;
		}

		private bool DoReplied()
		{
			log.Debug("Connecting to the Post Office Service : {0}", subscription.POServiceURL);
			PostOffice po = (PostOffice)Activator.GetObject(typeof(PostOffice),
				subscription.POServiceURL.ToString());
			
			if (po == null)
				throw new ApplicationException("No Post-Office Service");

			// post subscription
			subscription.SubscriptionState = SubscriptionStates.Pending;
			po.Post(subscription.ToIdentity, subscription);

			// update subscription
			subscription.SubscriptionState = SubscriptionStates.Delivered;
			poBox.Commit(subscription);

			// always return false to drop to the next state
			return false;
		}

		private bool DoDelivered()
		{
			bool result = false;

			log.Debug("Connecting to the Post Office Service : {0}", subscription.POServiceURL);
			PostOffice po = (PostOffice)Activator.GetObject(typeof(PostOffice),
				subscription.POServiceURL.ToString());
			
			if (po == null)
				throw new ApplicationException("No Post-Office Service");

			// post subscription
			SubscriptionStatus status = po.GetSubscriptionStatus(subscription.DomainID,
				subscription.FromIdentity, subscription.MessageID);

			// update subscription
			if (status.State == SubscriptionStates.Responded)
			{
				// create proxy
				if (status.Disposition == SubscriptionDispositions.Accepted)
				{
					log.Debug("Creating collection...");

					// do not re-create the proxy
					if (poBox.StoreReference.GetCollectionByID(subscription.SubscriptionCollectionID) == null)
					{
						// get and save details
						subscription.AddDetails(po.GetSubscriptionDetails(subscription.DomainID,
							subscription.FromIdentity, subscription.SubscriptionCollectionID));
						poBox.Commit(subscription);
					
						// create slave stub
						subscription.CreateSlave(poBox.StoreReference);
					}
				}

				// acknowledge the message
				po.AckSubscription(subscription.DomainID,
					subscription.FromIdentity, subscription.MessageID);

				// done with the subscription - move to ready state
				subscription.SubscriptionState = SubscriptionStates.Ready;
				poBox.Commit(subscription);

				// done
				result = true;
			}
			
			return result;
		}
	}
}
