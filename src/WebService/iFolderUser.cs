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
 *
 ***********************************************************************/


using System;

using Simias;
using Simias.Storage;

namespace Novell.iFolder.Web
{
	/// <summary>
	/// This class exists only to represent a Member and should only be
	/// used in association with the iFolderWebService class.
	/// </summary>
	[Serializable]
	public class iFolderUser
	{
		public string Name;
		public string UserID;
		public string Rights;
		public string ID;
		public string State;
		public string iFolderID;
		public bool IsOwner;
		public string FirstName;
		public string Surname;
		public string FN;

		public iFolderUser()
		{
		}
/*
		public iFolderUser(Simias.Storage.Member member)
		{
			this.Name = member.Name;
			this.UserID = member.UserID;
			this.Rights = member.Rights.ToString();
			this.ID = member.ID;
			this.State = "Member";
			this.IsOwner = member.IsOwner;
		}
*/

		public iFolderUser( Simias.Storage.Domain domain, Simias.Storage.Member member )
		{
			this.Name = member.Name;
			this.UserID = member.UserID;
			this.ID = member.ID;
			this.State = "Member";
			this.IsOwner = member.IsOwner;
			this.Rights = member.Rights.ToString();

			if ( member.Given != null )
			{
				this.Surname = member.Family;
				this.FirstName = member.Given;
				this.FN = member.FN;
			}
			else
			{
				if ( domain != null )
				{
					Simias.Storage.Member dMember = domain.GetMemberByID( member.UserID );
					if ( dMember != null )
					{
						this.Surname = dMember.Family;
						this.FirstName = dMember.Given;
						this.FN = dMember.FN;
					}
				}
			}
		}

		public iFolderUser(Simias.Storage.Member member,
							Novell.AddressBook.Contact contact)
		{
			if(contact != null)
			{
				Novell.AddressBook.Name name;
			
				name = contact.GetPreferredName();
				if(name != null)
				{
					this.Surname = name.Family;
					this.FirstName = name.Given;
				}
				this.FN = contact.FN;
			}

			this.Name = member.Name;
			this.UserID = member.UserID;
			this.ID = member.ID;
			this.State = "Member";
			this.IsOwner = member.IsOwner;
			this.Rights = member.Rights.ToString();
		}

		public iFolderUser( Simias.POBox.Subscription sub )
		{
			this.Name = sub.ToName;
			this.UserID = sub.ToIdentity;

			// Need to get the member for first and last name
			Simias.Storage.Domain domain = Store.GetStore().GetDomain( sub.DomainID );
			if ( domain != null )
			{
				Simias.Storage.Member simMem = domain.GetMemberByID( sub.ToIdentity );
				if ( simMem != null )
				{
					this.Surname = simMem.Family;
					this.FirstName = simMem.Given;
					this.FN = simMem.FN;
				}
			}

			this.Rights = sub.SubscriptionRights.ToString();
			this.ID = sub.ID;
			this.iFolderID = sub.SubscriptionCollectionID;
			this.State = "Invited";
			this.IsOwner = false;

			if(sub.SubscriptionState == 
				Simias.POBox.SubscriptionStates.Invited)
			{
				this.State = "WaitSync";
			}
			else if(sub.SubscriptionState == 
				Simias.POBox.SubscriptionStates.Posted)
			{
				this.State = "Invited";
			}
			else if(sub.SubscriptionState == 
				Simias.POBox.SubscriptionStates.Pending)
			{
				this.State = "AccessRequest";
			}
			else if(sub.SubscriptionState == 
				Simias.POBox.SubscriptionStates.Responded)
			{
				if(sub.SubscriptionDisposition ==
					Simias.POBox.SubscriptionDispositions.Declined)
					this.State = "Declined";
				else
					this.State = "Unknown";
			}
			else
			{
				this.State = "Unknown";
			}
		}

		public iFolderUser(Simias.POBox.Subscription sub,
							Novell.AddressBook.Contact contact)
		{
			if(contact != null)
			{
				Novell.AddressBook.Name name;
			
				name = contact.GetPreferredName();
				if(name != null)
				{
					this.Surname = name.Family;
					this.FirstName = name.Given;
				}
				this.FN = contact.FN;
			}

			this.Name = sub.ToName;
			this.UserID = sub.ToIdentity;
			this.Rights = sub.SubscriptionRights.ToString();
			this.ID = sub.ID;
			this.iFolderID = sub.SubscriptionCollectionID;
			this.State = "Invited";
			this.IsOwner = false;

			if(sub.SubscriptionState == 
							Simias.POBox.SubscriptionStates.Invited)
			{
				this.State = "WaitSync";
			}
			else if(sub.SubscriptionState == 
							Simias.POBox.SubscriptionStates.Posted)
			{
				this.State = "Invited";
			}
			else if(sub.SubscriptionState == 
							Simias.POBox.SubscriptionStates.Pending)
			{
				this.State = "AccessRequest";
			}
			else if(sub.SubscriptionState == 
							Simias.POBox.SubscriptionStates.Responded)
			{
				if(sub.SubscriptionDisposition ==
								Simias.POBox.SubscriptionDispositions.Declined)
					this.State = "Declined";
				else
					this.State = "Unknown";
			}
			else
			{
				this.State = "Unknown";
			}
		}
	}
}
