/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
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
using Simias.DomainServices;
using Simias.Event;
using Simias.Storage;
using Simias.Sync;
using Novell.Security.ClientPasswordManager;

namespace Simias.Authentication
{
	/// <summary>
	/// status codes returned by remote authentication modules
	/// </summary>
	[Serializable]
	public enum StatusCodes : uint
	{
		/// <summary>
		/// Successful authentication
		/// </summary>
		Success = 0x00000000,

		/// <summary>
		/// Successful authentication but within a grace login period
		/// </summary>
		SuccessInGrace = 0x00000001,

		/// <summary>
		/// The certificate is invalid.
		/// </summary>
		InvalidCertificate = 0x00000002,

		/// <summary>
		/// Invalid or Unknown user specified
		/// </summary>
		UnknownUser = 0x1f000001,

		/// <summary>
		/// Ambigous user - more than one user exists 
		/// </summary>
		AmbiguousUser = 0x1f000002,

		/// <summary>
		/// The credentials may have invalid characters etc.
		/// </summary>
		InvalidCredentials = 0x1f000003,

		/// <summary>
		/// Invalid password specified
		/// </summary>
		InvalidPassword = 0x1f000020,

		/// <summary>
		/// The account has been disabled by an administrator
		/// </summary>
		AccountDisabled = 0x1f000040,

		/// <summary>
		/// The account has been locked due to excessive login failures
		/// or possibly the grace logins have all been consumed
		/// </summary>
		AccountLockout = 0x1f000041,

		/// <summary>
		/// The simias account has been disabled by the administrator.
		/// </summary>
		SimiasLoginDisabled = 0x1f000042,

		/// <summary>
		/// The specified domain was unknown
		/// </summary>
		UnknownDomain = 0x1f000060,

		/// <summary>
		/// Authentication failed due to an internal exception
		/// </summary>
		InternalException = 0x1f000100,

		/// <summary>
		/// The authentication provider does not support the method
		/// </summary>
		MethodNotSupported = 0x1f000101,

		/// <summary>
		/// The operation timed out on the client request
		/// </summary>
		Timeout = 0x1f000102,

		/// <summary>
		/// Authentication failed with an unknown reason
		/// </summary>
		Unknown = 0x1f001fff
	}

	/// <summary>
	/// Defines the Status class which
	/// is returned on all remote authentication methods.
	/// </summary>
	[Serializable]
	public class Status
	{
		public Status()
		{
			statusCode = StatusCodes.Unknown;
		}

		public Status(StatusCodes status)
		{
			statusCode = status;
		}

		/// <summary>
		/// Status of the authentication.
		/// Must always be a valid status code
		/// </summary>
		public StatusCodes		statusCode;

		/// <summary>
		/// Unique ID of the domain.
		/// Valid on a successful authentication.
		/// </summary>
		public string			DomainID;

		/// <summary>
		/// Unique ID of the user
		/// Valid on a successful authentication
		/// </summary>
		public string			UserID;

		/// <summary>
		/// UserName 
		/// 
		/// Valid if the authentication was successful
		/// </summary>
		public string			UserName;

		/// <summary>
		/// Distinguished or unique user name used for
		/// the authentication.  This member can be
		/// the same as the UserName
		/// 
		/// Valid if the authentication was successful
		/// </summary>
		public string			DistinguishedUserName;

		/// <summary>
		/// ExceptionMessage returned when an internal
		/// exception occurred while trying to authenticate
		/// the user.
		/// 
		/// Valid if status == StatusCode.InternalException
		/// </summary>
		public string			ExceptionMessage;

		/// <summary>
		/// TotalGraceLogins the number of allowed on this account by policy
		/// 
		/// Valid if status == StatusCode.SuccessInGrace
		/// </summary>
		public int				TotalGraceLogins;

		/// <summary>
		/// RemainingGraceLogins the number of grace logins left on this account
		/// 
		/// Valid if status == StatusCode.SuccessInGrace
		/// </summary>
		public int				RemainingGraceLogins;
	}

	/// <summary>
	/// Summary description for Credentials
	/// </summary>
	public class Credentials
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Credentials));
		private string collectionID;
		private string memberID;
		private string domainID;
		private Store store;

		/// <summary>
		/// Constructor for checking if credentials exist for collection
		/// </summary>
		public Credentials(string collectionID)
		{
			this.collectionID = collectionID;
			this.store = Store.GetStore();
		}

		/// <summary>
		/// Constructor for checking if credentials exist for a domain
		/// and a member
		/// </summary>
		public Credentials(string domainID, string memberID)
		{
			this.domainID = domainID;
			this.memberID = memberID;
			this.store = Store.GetStore();
		}

		/// <summary>
		/// Constructor for checking if credentials exist for a 
		/// collection and member
		/// </summary>
		public Credentials(string domainID, string collectionID, string memberID)
		{
			this.domainID = domainID;
			this.collectionID = collectionID;
			this.memberID = memberID;
			this.store = Store.GetStore();
		}


		/// <summary>
		/// Gets the credentials (if they exist) that are set against
		/// the collection ID passed in the constructor.
		/// </summary>
		/// <returns>NetworkCredential object which can be assigned to the "Credentials" property in a proxy class.</returns>
		public NetworkCredential GetCredentials()
		{
			//
			// From the collection ID we need to figure out
			// the Realm, Username etc.
			//

			NetworkCredential realCreds = null;
			Simias.Storage.Domain cDomain = null;
			Simias.Storage.Member cMember = null;

			string memberName = "";
			string memberID = this.memberID;

			try
			{
				if ( this.domainID != null && this.collectionID != null && this.memberID != null )
				{
					cDomain = store.GetDomain( this.domainID );
					if ( cDomain != null )
					{
						cMember = cDomain.GetMemberByID( this.memberID );
						if ( cMember != null )
						{
							memberName = cMember.Name;
							memberID = cMember.UserID;
						}
					}
				}
				else
				if ( this.collectionID != null )
				{
					// Validate the shared collection
					Collection cCol = this.store.GetCollectionByID( collectionID );
					if ( cCol != null )
					{
						cMember = 
							( this.memberID != null ) 
								? cCol.GetMemberByID( this.memberID ) 
								: cCol.GetCurrentMember();
						if ( cMember != null )
						{
							memberName = cMember.Name;
							memberID = cMember.UserID;
							cDomain = this.store.GetDomain( cCol.Domain );
						}
						else
						{
							log.Debug( "Credentials::GetCredentials - current member not found" );
						}
					}
					else
					{
						log.Debug( "Credentials::GetCredentials - collection not found" );
					}
				}
				else
				{
					cDomain = this.store.GetDomain( this.domainID );
					cMember = cDomain.GetMemberByID( this.memberID );
					if ( cMember != null )
					{
						memberName = cMember.Name;
						memberID = cMember.UserID;
					}
				}

				//
				// Verify the domain is not marked "inactive" and that a non-workgroup
				// domain is marked authenticated.
				//

				DomainAgent domainAgent = new DomainAgent();
				if ( domainAgent.IsDomainActive( cDomain.ID ) &&
					 (domainAgent.IsDomainAuthenticated( cDomain.ID ) ||
					 cDomain.ConfigType.Equals(Simias.Storage.Domain.ConfigurationType.Workgroup)))
				{
					NetCredential cCreds = 
						new NetCredential(
							"iFolder", 
							this.domainID,
							true, 
							memberName,
							null );

					Uri cUri = DomainProvider.ResolveLocation( this.domainID );
					realCreds = cCreds.GetCredential( cUri, "BASIC" );
					if ( realCreds == null )
					{
						// Check if creds exist for the user ID
						cCreds = 
							new NetCredential(
								"iFolder", 
								this.domainID, 
								true, 
								memberID,
								null );

						realCreds = cCreds.GetCredential( cUri, "BASIC" );
						if ( realCreds == null && this.collectionID != null )
						{
							// Check if creds exist for the user ID and by collection
							cCreds = 
								new NetCredential(
								"iFolder", 
								this.collectionID, 
								true, 
								memberID,
								null );

							realCreds = cCreds.GetCredential( cUri, "BASIC" );
						}
					}
				}
			}
			catch{}
			return(realCreds);
		}
	}
}
