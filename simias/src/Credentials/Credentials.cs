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
using System.Net;
using Simias;
using Simias.Client;
using Simias.Event;
using Simias.Storage;
using Simias.Sync;
using Novell.Security.ClientPasswordManager;

namespace Simias.Authentication
{
	/// <summary>
	/// Summary description for Credentials
	/// </summary>
	public class Credentials
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Credentials));
		private string collectionID;

		public Credentials(string collectionID)
		{
			this.collectionID = collectionID;
		}

		public NetworkCredential GetCredentials()
		{
			//
			// From the collection ID we need to figure out
			// the Realm, Username etc.
			//

			NetworkCredential realCreds = null;

			try
			{
				Store	store = Store.GetStore();

				// Validate the shared collection
				Collection cCol = store.GetCollectionByID(collectionID);
				if (cCol != null)
				{
					Simias.Storage.Member cMember = cCol.GetCurrentMember();
					if (cMember != null)
					{
						Simias.Storage.Domain cDomain = store.GetDomain(cCol.Domain);

						NetCredential cCreds = 
							new NetCredential(
							"iFolder", 
							cDomain.ID, 
							true, 
							cMember.Name, 
							null);

						Uri cUri = new Uri(cDomain.HostAddress.ToString());
						realCreds = cCreds.GetCredential(cUri, "BASIC");
						if (realCreds == null)
						{
							log.Info("Credentials::GetCredentials - credentials not found");

							// Generate a "needs credentials" event
							/*
							EventPublisher cEvent = new EventPublisher();
							Simias.Client.Event.NotifyEventArgs cArg =
								new Simias.Client.Event.NotifyEventArgs(
								"Need-Credentials", 
								collectionID, 
								System.DateTime.Now);

							cEvent.RaiseEvent(cArg);
							*/
						}
					}
					else
					{
						log.Debug("Credentials::GetCredentials - current member not found");
					}
				}
				else
				{
					log.Debug("Credentials::GetCredentials - collection not found");
				}
			}
			catch{}
			return(realCreds);
		}
	}
}
