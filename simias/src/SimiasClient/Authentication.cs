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

using Novell.Security.ClientPasswordManager;

namespace Simias.Client.Authentication
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
	/// Summary description for Credentials
	/// </summary>
	public class DomainAuthentication
	{
		private string serviceName;
		private string domainID;
		private string password;
		private static CertPolicy certPolicy;

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
		/// <returns>Simias.Client.Authentication.Status object</returns>
		public Status Authenticate()
		{
			Status status = null;

			try
			{
				SimiasWebService simiasSvc = new SimiasWebService();
				simiasSvc.Url = 
					Simias.Client.Manager.LocalServiceUrl.ToString() +
					"/Simias.asmx";

				DomainInformation cInfo = simiasSvc.GetDomainInformation( this.domainID );
				if ( cInfo != null )
				{
					// If the password is null, then check and see if credentials have
					// been set on this process previously.
					if ( this.password == null )
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
							this.password = "";
						}
					}

					// Call Simias for a remote domain authentication
					status =
						simiasSvc.LoginToRemoteDomain( 
							this.domainID, 
							this.password );

					if (status.statusCode == StatusCodes.Success ||
						status.statusCode == StatusCodes.SuccessInGrace )
					{
						// Set the credentials in this process.
						new NetCredential(
							this.serviceName, 
							this.domainID, 
							true, 
							cInfo.MemberName, 
							this.password);
					}
				}
				else
				{
					//status = new Status( StatusCodes.UnknownDomain );
				}
			}
			catch(Exception ex)
			{
				// DEBUG
				if (MyEnvironment.Mono)
					Console.WriteLine( "Authentication - caught exception: {0}", ex.Message );

				//status = new Status( StatusCodes.InternalException );
				//status.ExceptionMessage = ex.Message;
			}

			return status;
		}
	}
}
