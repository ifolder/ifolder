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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Runtime.InteropServices;
using Simias.Storage;
using Simias.POBox;

namespace Novell.iFolder.iFolderCom
{
	/// <summary>
	/// Class to hold objects displayed in the shareWith listview.
	/// </summary>
	[ComVisible(false)]
	public class ShareListMember
	{
		private Subscription subscription;
		private Member member;
		private bool added = false;
		private bool changed = false;

		#region Constructors
		/// <summary>
		/// Constructs a ShareListMember object.
		/// </summary>
		public ShareListMember()
		{
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets and Sets the Added flag.
		/// </summary>
		public bool Added
		{
			get { return added; }
			set { added = value; }
		}

		/// <summary>
		/// Gets and Sets the Changed flag.
		/// </summary>
		public bool Changed
		{
			get { return changed; }
			set { changed = value; }
		}

		/// <summary>
		/// Gets and Sets the current contact.
		/// </summary>
		public Member Member
		{
			get { return member; }
			set { member = value; }
		}

		/// <summary>
		/// Gets/sets the Subscription object.
		/// </summary>
		public Subscription Subscription
		{
			get { return subscription; }
			set { subscription = value; }
		}

		/// <summary>
		/// Gets/sets the Rights.
		/// </summary>
		public Access.Rights Rights
		{
			get
			{
				if (Member != null)
				{
					return Member.Rights;
				}
				else
				{
					return Subscription.SubscriptionRights;
				}
			}

			set
			{
				if (Member != null)
				{
					Member.Rights = value;
				}
				else
				{
					Subscription.SubscriptionRights = value;
				}
			}
		}
		#endregion
	}
}
