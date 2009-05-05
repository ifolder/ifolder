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
*                 $Author: Calvin Gaisford <cgaisford@novell.com> 
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
		/// <summary>
		/// </summary>
		public string Name;
		/// <summary>
		/// </summary>
		public string UserID;
		/// <summary>
		/// </summary>
		public string Rights;
		/// <summary>
		/// </summary>
		public string ID;
		/// <summary>
		/// </summary>
		public string State;
		/// <summary>
		/// </summary>
		public string iFolderID;
		/// <summary>
		/// </summary>
		public bool IsOwner;
		/// <summary>
		/// </summary>
		public string FirstName;
		/// <summary>
		/// </summary>
		public string Surname;
		/// <summary>
		/// </summary>
		public string FN;

		/// <summary>
		/// </summary>
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

		/// <summary>
		/// </summary>
		/// <param name="domain"></param>
		/// <param name="member"></param>
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
/*
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
*/
		/// <summary>
		/// </summary>
		/// <param name="sub"></param>
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
/*
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
*/
	}
}
