/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright  Unpublished Work of Novell, Inc. All Rights Reserved.
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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Net;

using Simias;
using Simias.Client;

using Novell.Security.ClientPasswordManager;

namespace Simias.Client
{
	/// <summary>
	/// Defines the credential types stored on a domain.
	/// </summary>
	[Serializable]
	public enum CredentialType
	{
		/// <summary>
		/// Credentials have not been set on this domain.
		/// </summary>
		None,

		/// <summary>
		/// Credentials are not required for this domain.
		/// </summary>
		NotRequired,

		/// <summary>
		/// HTTP basic credentials.
		/// </summary>
		Basic,

		/// <summary>
		/// Public/Private key credentials.
		/// </summary>
		PPK
	}

	/// <summary>
	/// Status codes returned from Autheticate methods
	/// </summary>
	[Serializable]
	public enum AuthenticationStatus
	{
		/// <summary>
		/// The method was successful.
		/// </summary>
		Success,

		/// <summary>
		/// The username or password was invalid
		/// </summary>
		InvalidCredentials,

		/// <summary>
		/// The domain ID does not exist in the Simias store
		/// </summary>
		InvalidDomain,

		/// <summary>
		/// The authentication service was unable to connect
		/// to the domain
		/// </summary>
		ConnectDomainFailure,

		/// <summary>
		/// The authentication service was able to authenticate
		/// to the remote service but failed to apply the 
		/// credentials to the local credential wallet
		/// </summary>
		CredentialCacheFailure,

		/// <summary>
		/// An unknown error was realized.
		/// </summary>
		UnknownError
	};

	/// <summary>
	/// Summary description for Credentials
	/// </summary>
	public class DomainAuthentication
	{
		private string serviceName;
		private string domainID;
		private string password;
		private static CertPolicy certPolicy;
//		private string dialogTitle;
//		private System.Object owner;

		/// <summary>
		/// Static constructor to authenticate using the popup dialog
		/// </summary>
		// CRG: took this constructor out because we can't do this
		// it creates a dependency we can't have
/*		public DomainAuthentication(string domainID, string title, System.Object o)
		{
			this.domainID = domainID;
			this.dialogTitle = title;
			this.owner = o;
		}
*/
		/// <summary>
		/// Static constructor for the object.
		/// </summary>
		static DomainAuthentication()
		{
			// Set the credential policy for this process.
			certPolicy = new CertPolicy();
		}

		/// <summary>
		/// Static constructor to authenticate straight-away
		/// </summary>
		public DomainAuthentication(string serviceName, string domainID, string password)
		{
			this.serviceName = serviceName;
			this.domainID = domainID;
			this.password = password;
		}

		/// <summary>
		/// Authenticate to a remote Simias server
		/// </summary>
		/// <returns>AuthenticationStatus object - AuthenticationStatus.Success if successful.</returns>
		public AuthenticationStatus Authenticate()
		{
			AuthenticationStatus status = AuthenticationStatus.UnknownError;

			try
			{
				SimiasWebService simiasSvc = new SimiasWebService();
				simiasSvc.Url = 
					Simias.Client.Manager.LocalServiceUrl.ToString() +
					"/Simias.asmx";

				DomainInformation cInfo = simiasSvc.GetDomainInformation(this.domainID);
				if (cInfo != null)
				{
					// If the password is null, then check and see if credentials have
					// been set on this process previously.
					if (this.password == null)
					{
						// DEBUG
						if (MyEnvironment.Mono)
							Console.WriteLine("Password is null.");

						NetCredential netCredential = new NetCredential(
							this.serviceName, 
							this.domainID, 
							true, 
							cInfo.MemberName, 
							null);

						NetworkCredential credentials = 
							netCredential.GetCredential(
								new Uri(cInfo.RemoteUrl), 
								"BASIC");

						if (credentials != null)
						{
							// DEBUG
							if (MyEnvironment.Mono)
								Console.WriteLine("Retrieved credentials.");

							this.password = credentials.Password;
						}
						else
						{
							if (MyEnvironment.Mono)
								Console.WriteLine("Failed to get credentials.");

//							PasswordDialog pwdDlg = 
//								new PasswordDialog(this.dialogTitle, cInfo.Name);
//							pwdDlg.Invoke(this.owner);
//							this.password = pwdDlg.password;
						}
					}

					// Remote domain
					DomainService domainSvc = new DomainService();
					DomainInfo cDomainInfo = null;

					domainSvc.Url = cInfo.RemoteUrl;
					domainSvc.Credentials = 
						new NetworkCredential(cInfo.MemberName, this.password);

					try
					{
						// Call the remote domain service and attempt to
						// get Domain Information.  This will force an
						// authentication to occurr
						cDomainInfo = domainSvc.GetDomainInfo(cInfo.MemberUserID);
					}
					catch(WebException webEx)
					{
						if (webEx.Status == System.Net.WebExceptionStatus.ProtocolError ||
							webEx.Status == System.Net.WebExceptionStatus.TrustFailure)
						{
							// DEBUg
							if (MyEnvironment.Mono)
								Console.WriteLine("Invalid credentials");

							status = AuthenticationStatus.InvalidCredentials;
						}
						else
							if (webEx.Status == System.Net.WebExceptionStatus.ConnectFailure)
						{
							// DEBUG
							if (MyEnvironment.Mono)
								Console.WriteLine("ConnectDomainFailure");

							status = AuthenticationStatus.ConnectDomainFailure;
						}
						// DEBUG
						else
						{
							if (MyEnvironment.Mono)
								Console.WriteLine("Caught web exception: {0}", webEx.Message);
						}
					}

					if (cDomainInfo != null)
					{
						// TODO DomainCredentials need to support an empty string.
						// Change "Not Needed" to empty when fixed.
						if (this.password == null)
							password = "Not Needed";

						int lStatus = 
							simiasSvc.SetDomainCredentials(
								cInfo.ID, cInfo.MemberUserID, this.password);
						if (lStatus == 0)
						{
							// Set the credentials in this process.
							new NetCredential(
								this.serviceName, 
								this.domainID, 
								true, 
								cInfo.MemberName, 
								this.password);

							status = AuthenticationStatus.Success;
						}
						else
						{
							status = AuthenticationStatus.CredentialCacheFailure;
						}
					}
				}
				else
				{
					status = AuthenticationStatus.InvalidDomain;
				}
			}
			catch(Exception ex)
			{
				// DEBUG
				if (MyEnvironment.Mono)
					Console.WriteLine("Authentication - caught exception: {0}", ex.Message);
			}

			return(status);
		}
	}
}
