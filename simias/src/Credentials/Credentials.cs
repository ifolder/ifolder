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
using Simias.Domain;
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
		private string memberID;
		private string domainID;
		private Store store;

		/// <summary>
		/// Constructor for checking if credentials exist for collection
		/// </summary>
		public Credentials(string collectionID)
		{
			this.collectionID = collectionID;
			this.store = Store.GetStore();
		}

		/// <summary>
		/// Constructor for checking if credentials exist for a domain
		/// and a member
		/// </summary>
		public Credentials(string domainID, string memberID)
		{
			this.domainID = domainID;
			this.memberID = memberID;
			this.store = Store.GetStore();
		}

		/// <summary>
		/// Gets the credentials (if they exist) that are set against
		/// the collection ID passed in the constructor.
		/// </summary>
		/// <returns>NetworkCredential object which can be assigned to the "Credentials" property in a proxy class.</returns>
		public NetworkCredential GetCredentials()
		{
			//
			// From the collection ID we need to figure out
			// the Realm, Username etc.
			//

			NetworkCredential realCreds = null;
			Simias.Storage.Domain cDomain = null;
			Simias.Storage.Member cMember = null;

			try
			{
				Store	store = Store.GetStore();
				if (this.collectionID != null)
				{
					// Validate the shared collection
					Collection cCol = store.GetCollectionByID(collectionID);
					if (cCol != null)
					{
						cMember = cCol.GetCurrentMember();
						if (cMember != null)
						{
							cDomain = store.GetDomain(cCol.Domain);
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
				else
				{
					cDomain = this.store.GetDomain(this.domainID);
					Roster cRoster = cDomain.GetRoster(this.store);
					cMember = cRoster.GetMemberByID(this.memberID);
				}

				//
				// Verify the domain is not marked "inactive"
				//

				if ( new DomainAgent().IsDomainActive( cDomain.ID ) == true )
				{
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
						log.Debug("Credentials::GetCredentials - credentials not found");
					}
				}
			}
			catch{}
			return(realCreds);
		}
	}
}
