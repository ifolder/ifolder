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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

// This entire namespace (Simias.Mail) was taken and modified 
// from the mono project.  It was part of the System.Web.Mail namespace.
// We needed a way to send mail on all of the platforms we supported
// and found that the mono implementation of System.Web.Mail was much
// more usable.  We changed the namespace and included the code here
// so we could use it for the peer notification on all of the platforms
// including .Net on windows.


using System;
using System.IO;
using NUnit.Framework;

namespace Simias.Mail.Tests
{
	[TestFixture]
		public class SmtpTests
		{
			public string smtpHost;
			public string fromAddr;
			public string toAddr;

			[TestFixtureSetUp]
				public void Init()
				{
					Console.WriteLine("");
					Console.WriteLine("=== Setting up Smtp Tests ===");
					Configuration config = Configuration.CreateDefaultConfig("./");
					smtpHost = config.Get("Simias.Mail", "smtpServer", "mail");
					fromAddr = config.Get("Simias.Mail.Tests", "fromAddr", "Simias.Mail.Tests.SmtpTests@localhost");
					toAddr = config.Get("Simias.Mail.Tests", "toAddr", "denali@novell.com");
				}

			[Test]
				public void SendSimpleWithServer()
				{
					Console.WriteLine("=== Running SendSimpleWithServer ===");
					Console.WriteLine("Sending mail to : " + toAddr);
					if(!SmtpMail.Send(smtpHost, fromAddr, toAddr,
								"Simias.Smtp.SendSimpleWithServer Test", 
								"This is the Simias.Smtp.SendSimpleMail Test.  You are receiving this email because somebody was kind enough to include you in the Simias.Smtp test cases and verify that Simias can send email"))
					{
						throw new Exception("SendMail failed");
					}
				}

			[Test]
				public void SendSimpleWithoutServer()
				{
					Console.WriteLine("=== Running SendSimpleWithoutServer ===");
					Console.WriteLine("Sending mail to : " + toAddr);
					if(!SmtpMail.Send(fromAddr, toAddr,
								"Simias.Smtp.SendSimpleWithoutServer Test", 
								"This is the Simias.Smtp.SendSimpleMail Test.  You are receiving this email because somebody was kind enough to include you in the Simias.Smtp test cases and verify that Simias can send email"))
					{
						throw new Exception("SendMail failed");
					}
				}

			[Test]
				public void SendAttachmentWithServer()
				{
					Console.WriteLine("=== Running SentAttachmentWithServer ===");
					Console.WriteLine("Sending mail to : " + toAddr);

					MailMessage message = new MailMessage();

					message.BodyFormat = MailFormat.Text;

					message.From = fromAddr;
					message.To = toAddr;

					message.Subject = "Simias.Smtp SendAttachmentMail Test";

					// body
					message.Body = "This is the body of a message and there should be one attachment to this guy";

					MailAttachment attachment = new MailAttachment("smtptst1.gif");
					message.Attachments.Add(attachment);

					attachment = new MailAttachment("smtptst2.gif");
					message.Attachments.Add(attachment);

					// send
					if(!SmtpMail.Send(smtpHost, message))
					{
						throw new Exception("SendMail failed");
					}
				}

			[Test]
				public void SendAttachmentWithoutServer()
				{
					Console.WriteLine("=== Running SentAttachmentWithoutServer ===");
					Console.WriteLine("Sending mail to : " + toAddr);

					MailMessage message = new MailMessage();

					message.BodyFormat = MailFormat.Text;

					message.From = fromAddr;
					message.To = toAddr;

					message.Subject = "Simias.Smtp SendAttachmentMail Test";

					// body
					message.Body = "This is the body of a message and there should be one attachment to this guy";

					MailAttachment attachment = new MailAttachment("smtptst1.gif");
					message.Attachments.Add(attachment);

					attachment = new MailAttachment("smtptst2.gif");
					message.Attachments.Add(attachment);

					// send
					if(!SmtpMail.Send(message))
					{
						throw new Exception("SendMail failed");
					}
				}

			[TestFixtureTearDown]
				public void Cleanup()
				{
				}
		}

	public class Tests
	{
		static void Main()
		{
			SmtpTests tests = new SmtpTests();
			tests.Init();
			tests.SendSimpleWithServer();
			tests.SendSimpleWithoutServer();
			tests.SendAttachmentWithServer();
			tests.SendAttachmentWithoutServer();
			tests.Cleanup();
		}
	}
}


