/***********************************************************************
 *  Simias.Mail.SmtpMail.cs
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author:
 *         Lawrence Pit (loz@cable.a2000.nl)
 *         Per Arneng (pt99par@student.bth.se)
 * 
 ***********************************************************************/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Reflection;

namespace Simias.Mail
{
    /// <remarks>
    /// </remarks>
    public class SmtpMail
    {
        // Constructor		
        private SmtpMail ()
        {
            /* empty */
        }       

		public static bool Send(MailMessage message)
		{
			string smtpServer;
			smtpServer = new Configuration().Get("Simias.Mail", "smtpServer", "mail");
			return Send(smtpServer, message);
		}

        public static bool Send(string smtpServer, MailMessage message) 
        {
            try
            {
            	// wrap the MailMessage in a MailMessage wrapper for easier
            	// access to properties and to add some functionality
                MailMessageWrapper messageWrapper =
						new MailMessageWrapper( message );

                SmtpClient smtp = new SmtpClient (smtpServer);

                smtp.Send (messageWrapper);

                smtp.Close ();
            }
            catch(Exception e)
            {
				return false;
            }
			return true;
        }

        public static bool Send (string from, string to,
				string subject, string messageText) 
		{
			string smtpServer;
			smtpServer = new Configuration().Get("Simias.Mail", "smtpServer", "localhost");
			return Send(smtpServer, from, to, subject, messageText);
		}

        public static bool Send (string smtpServer, string from, string to,
				string subject, string messageText) 
        {
            MailMessage message = new MailMessage ();
            message.From = from;
            message.To = to;
            message.Subject = subject;
            message.Body = messageText;
            return Send (smtpServer, message);
        }
    }
} //namespace System.Web.Mail
