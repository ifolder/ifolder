using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Web;
using System.Web.SessionState;
using System.Web.Services;
using System.Web.Services.Protocols;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Simias.Web;


namespace SimiasTests
{
	/// <summary>
	/// Summary description for Actions
	/// </summary>
	[WebService(Namespace="http://novell.com/simiastests/actions/")]
	public class Actions : System.Web.Services.WebService
	{
		public Actions()
		{
			//
			// TODO: Add constructor logic here
			//
			InitializeComponent();
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		#endregion

		/// <summary>
		/// Ping
		/// Method for clients to determine if the test 
		/// web service is up and running.
		/// </summary>
		/// <param name="sleepFor"></param>
		/// <returns>0</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public int Ping(int sleepFor)
		{
			Thread.Sleep(sleepFor * 1000);
			return 0;
		}

		/// <summary>
		/// RemoteAuthentication
		/// Authenticate to a specified enterprise domain
		/// </summary>
		/// <param name="preAuthenticate"></param>
		/// <param name="host"></param>
		/// <param name="user"></param>
		/// <param name="password"></param>
		/// <returns>0</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public int RemoteAuthentication(bool preAuthenticate, string host, string user, string password)
		{
			Console.WriteLine("SimiasTests.Actions.RemoteAuthentication called");
			int status = 0;
			string serviceUrl = "/simias10/DomainService.asmx";
		
			// Create the domain service web client object.
			DomainService domainService = new DomainService();
			domainService.Url = "http://" + host + serviceUrl;
		
			//
			// Setup the credentials
			//
			NetworkCredential myCred = new NetworkCredential(user, password);
			CredentialCache myCache = new CredentialCache();
			myCache.Add(new Uri(domainService.Url), "Basic", myCred);
			domainService.Credentials = myCache;
			domainService.CookieContainer = new CookieContainer();
			domainService.PreAuthenticate = preAuthenticate;
			
			Console.WriteLine(" Host URL: " + domainService.Url);
			Console.WriteLine(" PreAuthenticate: " + preAuthenticate.ToString());
			Console.WriteLine(" User: " + user);


			// provision user
			try
			{
				Console.WriteLine(" calling proxy method");
				ProvisionInfo provisionInfo = domainService.ProvisionUser(user, password);
				Console.WriteLine(" UserID:  " + provisionInfo.UserID);
				Console.WriteLine(" POBoxID: " + provisionInfo.POBoxID);
			}
			catch(WebException webEx)
			{
				status = -1;
				Console.WriteLine(webEx.Status.ToString());
			}
			catch(Exception ex)
			{
				status = -1;
				Console.WriteLine(ex.Message);
			}

			Console.WriteLine("SimiasTests.Actions.RemoteAuthentication exit");
			return status;
		}

		/// <summary>
		/// GetArray
		/// Return an array of specified characters
		/// </summary>
		/// <param name="sizeToReturn"></param>
		/// <param name="charToReturn"></param>
		/// <returns>0</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public char[] GetArray(int sizeToReturn, char charToReturn)
		{
			char[] returnChars = new char[sizeToReturn];
			for (int i = 0; i < sizeToReturn; i++)
			{
				returnChars[i] = charToReturn;
			}

			return returnChars;
		}
	}
}
