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
using System.Text;

using Simias.Sync;
using Simias.Mail;

namespace Simias.Invite
{
	/// <summary>
	/// Smtp Invitation Provider
	/// </summary>
	public class SmtpInvitationProvider : IInvitationProvider
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SmtpInvitationProvider));

		/// <summary>
		/// Invite a user to a collection
		/// </summary>
		/// <param name="invitation">The invitation to send</param>
		public void Invite(Invitation invitation)
		{
			// validate
			Validate(invitation);

			// TODO: Localize
			
			MailMessage message = new MailMessage();

			message.BodyFormat = MailFormat.Text;

			message.From = invitation.FromEmail;
			message.To = invitation.ToEmail;

			message.Subject =
				String.Format("Shared iFolder Invitation - {0}", invitation.CollectionName);
			
			// body
			StringBuilder buffer = new StringBuilder();
			
			buffer.AppendFormat("{0} invites you to participate in the shared iFolder named \"{1}\".\n",
				invitation.FromName, invitation.CollectionName);

			// TODO: Owner currently cannot be resolved to a friendly name.  Also need to add "Novell iFolder server" for enterprise.
			// buffer.AppendFormat("This iFolder is hosted by {0} on a personal computer.\n\n", invitation.Owner);
			buffer.Append("\n");

			if (invitation.Message != null)
			{
				buffer.AppendFormat("{0}\n\n", invitation.Message);
			}

			buffer.AppendFormat("{0} assigned you {1} rights to this shared iFolder.\n\n",
				invitation.FromName, invitation.CollectionRights);

			buffer.Append("You can participate from one or more computers with the iFolder client. For information or download, see the iFolder Web site at http://www.ifolder.com. \n\n");

			buffer.Append("To accept and set up the shared iFolder on this computer, open the attached iFolder Invitation (IFI) file. Repeat this process on each computer where you want to set up the shared iFolder.\n\n");

			buffer.Append("If you do not accept within 7 days, the invitation  automatically expires. To decline immediately, open the IFI file and select Decline.");

			message.Body = buffer.ToString();

			// invitation attachment
			string filename = Path.Combine(Path.GetTempPath(),
				Path.GetFileNameWithoutExtension(Path.GetTempFileName())
				+ Invitation.Extension);

			invitation.Save(filename);

			MailAttachment attachment = new MailAttachment(filename);
			
			message.Attachments.Add(attachment);

			// send
			SmtpMail.Send(message);

			// delete invitation file
			File.Delete(filename);

			log.Info("Invitation Sent: {0}", invitation);
		}

		/// <summary>
		/// Validate the invitation for Email
		/// </summary>
		/// <param name="invitation">The invitation object</param>
		private void Validate(Invitation invitation)
		{
			if (invitation.FromEmail == null || invitation.FromEmail.Length == 0)
			{
				log.Debug("Invalid Invitation (No From Email Address): {0}",
					invitation);
				
				throw new ArgumentNullException("FromEmail");
			}

			if (invitation.ToEmail == null || invitation.ToEmail.Length == 0)
			{
				log.Debug("Invalid Invitation (No To Email Address): {0}",
					invitation);
				
				throw new ArgumentNullException("ToEmail");
			}

		}
	}
}
