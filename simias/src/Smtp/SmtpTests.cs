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
					Configuration config = new Configuration("./");
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


