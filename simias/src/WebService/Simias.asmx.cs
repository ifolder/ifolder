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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Novell.Security.ClientPasswordManager;

using Simias;
using Simias.Client;
using Simias.Domain;
using Simias.Storage;
using Simias.Sync;
//using Simias.POBox;

namespace Simias.Web
{
	/// <summary>
	/// This is the core of the iFolderServce.  All of the methods in the
	/// web service are implemented here.
	/// </summary>
	[WebService(
	Namespace="http://novell.com/simias/web/",
	Name="Simias Web Service",
	Description="Web Service providing access to Simias")]
	public class SimiasService : WebService
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SimiasService));

		/// <summary>
		/// Creates the SimiasService and sets up logging
		/// </summary>
		public SimiasService()
		{
		}
	
		/// <summary>
		/// WebMethod that returns the Simias information
		/// </summary>
		/// <returns>
		/// string with Simias information
		/// </returns>
		[WebMethod(Description="GetSimiasInformation")]
		[SoapDocumentMethod]
		public string GetSimiasInformation()
		{
			return "TODO: Implement the Simias Web Service";
		}


		/// <summary>
		/// WebMethod to get information about a specified domain 
		/// </summary>
		/// <returns>
		/// 0 success, !0 failed
		/// </returns>
		[WebMethod(Description="GetDomainInformation")]
		[SoapDocumentMethod]
		public
		DomainInformation
		GetDomainInformation(string domainID)
		{
			DomainInformation cDomainInfo = null;
			try
			{
				Store store = Store.GetStore();

				Simias.Storage.Domain cDomain = store.GetDomain(domainID);
				Roster cRoster = cDomain.GetRoster(store);
				Member cMember = cRoster.GetCurrentMember();

				cDomainInfo = new DomainInformation();

				cDomainInfo.Type =
					(domainID == Simias.Storage.Domain.WorkGroupDomainID)
						? DomainType.Workgroup
						: DomainType.Enterprise;

				cDomainInfo.Active = new DomainAgent().IsDomainActive( cDomain.ID );
				cDomainInfo.ID = domainID;
				cDomainInfo.Name = cDomain.Name;
				cDomainInfo.Description = cDomain.Description;
				cDomainInfo.RosterID = cRoster.ID;
				cDomainInfo.RosterName = cRoster.Name;
				cDomainInfo.RosterID = cRoster.ID;
				cDomainInfo.MemberID = cMember.UserID;
				cDomainInfo.MemberName = cMember.Name;
				cDomainInfo.RemoteUrl = 
					cDomain.HostAddress.ToString() + "/DomainService.asmx";
			}
			catch(Exception e)
			{
				log.Debug(e.Message);
				log.Debug(e.StackTrace);
				cDomainInfo = null;
			}

			return(cDomainInfo);
		}


		/// <summary>
		/// WebMethod to get a list of local domain IDs
		/// </summary>
		/// <returns>
		/// 0 success, !0 failed
		/// </returns>
		[WebMethod(Description="GetDomainList")]
		[SoapDocumentMethod]
		public
		string[]
		GetDomainList(bool onlySlaves)
		{
			ArrayList domainIDs = new ArrayList();

			try
			{
				Store store = Store.GetStore();
				LocalDatabase ldb = store.GetDatabaseObject();
				ICSList domainList = ldb.GetNodesByType( "Domain" );
				foreach( ShallowNode shallowNode in domainList )
				{
					// Get them all?
					if ( onlySlaves == false )
					{
						domainIDs.Add( shallowNode.ID );
					}
					else
					{
						Simias.Storage.Domain cDomain = store.GetDomain( shallowNode.ID );
						Roster cRoster = cDomain.GetRoster(store);
						if ( cRoster.Role.ToString() == "Slave" )
						{
							domainIDs.Add( cDomain.ID );
						}
					}
				}
			}
			catch(Exception e)
			{
				log.Error(e.Message);
				log.Error(e.StackTrace);
			}

			return( ( domainIDs.Count == 0 ) ? null : (string[]) domainIDs.ToArray( typeof( string ) ) );
		}


		/// <summary>
		/// WebMethod that sets credential information for a 
		/// member to a domain
		/// </summary>
		/// <returns>
		/// 0 success, !0 failed
		/// </returns>
		[WebMethod(Description="SetDomainCredentials")]
		[SoapDocumentMethod]
		public
		int SetDomainCredentials(
				string			domainID,
				string			memberID,
				string			password)
		{
			int status = -1;
			try
			{
				Store store = Store.GetStore();

				// Make sure the Domain ID is not workgroup
				if (domainID != Simias.Storage.Domain.WorkGroupDomainID)
				{
					Simias.Storage.Domain cDomain = store.GetDomain(domainID);
					Roster cRoster = cDomain.GetRoster(store);
					Member cMember = cRoster.GetMemberByID(memberID);

					NetCredential cCreds = 
						new NetCredential("iFolder", domainID, true, cMember.Name, password);

					if (cCreds != null)
					{
						status = 0;
					}
				}
			}
			catch(Exception e)
			{
				log.Debug(e.Message);
				log.Debug(e.StackTrace);
			}

			return(status);
		}

		/// <summary>
		/// WebMethod to set a slave domain "active"
		/// A Domain marked "active" will synchronize
		/// collections, subscriptions etc. to the remote server
		/// </summary>
		/// <returns>
		/// 0 success, !0 failed
		/// </returns>
		[WebMethod(Description="SetDomainActive - enables synchronization to the remote server")]
		[SoapDocumentMethod]
		public int SetDomainActive(string domainID)
		{
			DomainAgent domainAgent = new DomainAgent();
			domainAgent.SetDomainActive( domainID );
			return(0);
		}

		/// <summary>
		/// WebMethod to mark a slave domain "inactive"
		/// Marking a domain inactive disables all synchronization
		/// to the remote machine.
		/// </summary>
		/// <returns>
		/// 0 success, !0 failed
		/// </returns>
		[WebMethod(Description="SetDomainInactive - disables remote synchronization")]
		[SoapDocumentMethod]
		public int SetDomainInactive(string domainID)
		{
			DomainAgent domainAgent = new DomainAgent();
			domainAgent.SetDomainInactive( domainID );
			return(0);
		}

		/// <summary>
		/// WebMethod that checks to see if a full set of credentials
		/// has been set on a domain for a specified user
		/// </summary>
		/// <returns>
		/// true - valid credentials for member on the domain, false
		/// </returns>
		[WebMethod(Description="ValidCredentials")]
		[SoapDocumentMethod]
		public
		bool ValidCredentials(string domainID, string memberID)
		{
			bool status = false;
			try
			{
				Store store = Store.GetStore();

				// Make sure the Domain ID is not workgroup
				if (domainID != Simias.Storage.Domain.WorkGroupDomainID)
				{
					// roster
					Simias.Storage.Domain cDomain = store.GetDomain(domainID);
					Roster roster = cDomain.GetRoster(store);

					// find user
					Member cMember = roster.GetMemberByID(memberID);

					NetCredential cCreds = 
						new NetCredential("iFolder", domainID, true, cMember.Name, null);

					UriBuilder cUri = 
						new UriBuilder(
							this.Context.Request.Url.Scheme,
							this.Context.Request.Url.Host,
							this.Context.Request.Url.Port,
							this.Context.Request.ApplicationPath.TrimStart( new char[] {'/'} ));

					NetworkCredential realCreds = cCreds.GetCredential(cUri.Uri, "BASIC");
					if (realCreds != null)
					{
						status = true;
					}
				}
			}
			catch(Exception e)
			{
				log.Debug(e.Message);
				log.Debug(e.StackTrace);
			}

			return(status);
		}

		/// <summary>
		/// Saves the domain credentials to the local store.
		/// </summary>
		/// <param name="domainID">The ID of the domain to set the credentials on.</param>
		/// <param name="credentials">Credentials to set.</param>
		/// <param name="type">Type of credentials.</param>
		[WebMethod(Description="Save domain credentials to the local store")]
		[SoapDocumentMethod]
		public void SaveDomainCredentials(string domainID, string credentials, CredentialType type)
		{
			Store store = Store.GetStore();
			store.SetDomainCredentials(domainID, credentials, type);
		}

		/// <summary>
		/// Gets the saved credentials from the specified domain object.
		/// </summary>
		/// <param name="domainID">The ID of the domain to set the credentials on.</param>
		/// <param name="userID">Gets the ID of the user.</param>
		/// <param name="credentials">Gets the credentials for the domain.</param>
		/// <returns>The type of credentials.</returns>
		[WebMethod(Description="Get the saved credentials from a domain")]
		[SoapDocumentMethod]
		public CredentialType GetSavedDomainCredentials(string domainID, out string userID, out string credentials)
		{
			Store store = Store.GetStore();
			return store.GetDomainCredentials(domainID, out userID, out credentials);
		}
	}

	/// <summary>
	/// Type of Domain (Enterprise/Workgroup)
	/// </summary>
	[Serializable]
	public enum DomainType
	{
		/// <summary>
		/// Workgroup domain
		/// </summary>
		Workgroup,

		/// <summary>
		/// Enterprise domain
		/// </summary>
		Enterprise,
	};

	/// <summary>
	/// Domain information
	/// </summary>
	[Serializable]
	public class DomainInformation
	{
		/// <summary>
		/// Domain Type
		/// </summary>
		public DomainType Type;

		/// <summary>
		/// Domain Active
		/// true - a state where collections belonging
		/// to the domain can synchronize, remote invitations
		/// can occur etc.
		/// false - no remote actions will take place
		/// </summary>
		public bool Active;

		/// <summary>
		/// Domain Name
		/// </summary>
		public string Name;

		/// <summary>
		/// Domain Description
		/// </summary>
		public string Description;

		/// <summary>
		/// Domain ID
		/// </summary>
		public string ID;

		/// <summary>
		/// Domain Roster (Member List) Collection ID
		/// </summary>
		public string RosterID;

		/// <summary>
		/// Domain Roster (Member List) Collection Name
		/// </summary>
		public string RosterName;

		/// <summary>
		/// The unique member/user ID.
		/// </summary>
		public string MemberID;

		/// <summary>
		/// The name of the member object
		/// </summary>
		public string MemberName;

		/// <summary>
		/// Url to the remote domain service
		/// </summary>
		public string RemoteUrl;

		/// <summary>
		/// Constructor
		/// </summary>
		public DomainInformation()
		{
		}

		/// <summary>
		/// Create a string representation
		/// </summary>
		/// <returns>A string representation</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			
			string newLine = Environment.NewLine;

			builder.AppendFormat("Domain Information{0}", newLine);
			builder.AppendFormat("  ID               : {0}{1}", this.ID, newLine);
			builder.AppendFormat("  Type             : {0}{1}", this.Type.ToString(), newLine);
			builder.AppendFormat("  Name             : {0}{1}", this.Name, newLine);
			builder.AppendFormat("  Description      : {0}{1}", this.Description, newLine);
			builder.AppendFormat("  Roster ID        : {0}{1}", this.RosterID, newLine);
			builder.AppendFormat("  Roster Name      : {0}{1}", this.RosterName, newLine);
			builder.AppendFormat("  Member Node ID   : {0}{1}", this.MemberID, newLine);
			builder.AppendFormat("  Member Node Name : {0}{1}", this.MemberName, newLine);
			builder.AppendFormat("  Remote Url       : {0}{1}", this.RemoteUrl, newLine);

			return builder.ToString();
		}
	}
}




