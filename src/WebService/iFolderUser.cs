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

		public iFolderUser()
		{
		}

		public iFolderUser(Simias.Storage.Member member)
		{
			this.Name = member.Name;
			this.UserID = member.UserID;
			this.Rights = member.Rights.ToString();
			this.ID = member.ID;
			this.State = "Member";
		}


		public iFolderUser(Simias.POBox.Subscription sub)
		{
			this.Name = sub.ToName;
			this.UserID = sub.ToIdentity;
			this.Rights = sub.SubscriptionRights.ToString();
			this.ID = sub.ID;
			this.State = "Invited";

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
