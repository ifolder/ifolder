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
using System.IO;
using System.Web.Mail;
using System.Net;
using System.Text;

using Simias;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Invite
{
	/// <summary>
	/// Invitation Service Class
	/// </summary>
	public class InvitationService
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(InvitationService));

		/// <summary>
		/// Hidden Constructor
		/// </summary>
		private InvitationService()
		{
		}

		/// <summary>
		/// Generate an inviation for a user to a collection
		/// </summary>
		/// <param name="collection">The collection object</param>
		/// <param name="identity">The user identity</param>
		/// <returns>The generated invitation object</returns>
		public static Invitation CreateInvitation(Collection collection, string identity)
		{
			// get the collection
			SyncCollection syncCollection = new SyncCollection(collection);

			// generate the invitation
			Invitation invitation = syncCollection.CreateInvitation(identity);

			return invitation;
		}

		/// <summary>
		/// Invite a user to a collection
		/// </summary>
		/// <param name="invitation">The invitation</param>
		public static void Invite(Invitation invitation)
		{
			// TODO: remove hard-coded answer
			IInvitationProvider ip = (IInvitationProvider)Activator.CreateInstance(
				"SmtpInvitationProvider", "Simias.Invite.SmtpInvitationProvider").Unwrap();

			// invite
			ip.Invite(invitation);
		}
		
		/// <summary>
		/// Accept a collection invitation on the current machine
		/// </summary>
		/// <param name="store">The collection store object</param>
		/// <param name="invitation">The invitation object</param>
		public static void Accept(Store store, Invitation invitation)
		{
			// default local path ?
			if ((invitation.RootPath == null) || (invitation.RootPath.Length == 0))
			{
				invitation.RootPath = Invitation.DefaultRootPath;
			}
			
			// create the slave collection with the invitation
			SyncCollection collection = new SyncCollection(store, invitation);

			// save the new collection
			collection.Commit();

			MyTrace.WriteLine("Invitation Accepted: {0}", invitation);
		}
	}
}
