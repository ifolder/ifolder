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
				String.Format("Simias Invitation - {0}", invitation.CollectionName);
			
			// body
			StringBuilder buffer = new StringBuilder();
			
			buffer.AppendFormat("{0} is sharing the \"{1}\" {2} with you.\n\n",
				invitation.FromName, invitation.CollectionName, invitation.CollectionType);

			if (invitation.Message != null)
			{
				buffer.AppendFormat("{0}\n\n", invitation.Message);
			}

			buffer.AppendFormat("You have been given {0} rights.\n\n",
				invitation.CollectionRights);

			buffer.AppendFormat("To add the {0} to this client, open the attached file.\n\n",
				invitation.CollectionType);

			buffer.AppendFormat("Before accepting this {0} you must install the Simias client.\n\n",
				invitation.CollectionType);

			buffer.AppendFormat("For details, go to http://www.novell.com/ifolder .\n\n");

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

			MyTrace.WriteLine("Invitation Sent: {0}", invitation);
		}

		/// <summary>
		/// Validate the invitation for Email
		/// </summary>
		/// <param name="invitation">The invitation object</param>
		private void Validate(Invitation invitation)
		{
			if (invitation.FromEmail == null || invitation.FromEmail.Length == 0)
			{
				MyTrace.WriteLine("Invalid Invitation (No From Email Address): {0}",
					invitation);
				
				throw new ArgumentNullException("FromEmail");
			}

			if (invitation.ToEmail == null || invitation.ToEmail.Length == 0)
			{
				MyTrace.WriteLine("Invalid Invitation (No To Email Address): {0}",
					invitation);
				
				throw new ArgumentNullException("ToEmail");
			}

		}
	}
}
