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
using Simias.DomainServices;
using Simias.Storage;
using Simias.Sync;
using Simias.Security.Web.AuthenticationService;
//using Simias.POBox;

namespace Simias.Web
{
	/// <summary>
	/// Supported store search operators.
	/// </summary>
	public enum SearchType
	{
		/// <summary>
		/// Used to compare if two values are equal.
		/// </summary>
		Equal,

		/// <summary>
		/// Used to compare if two values are not equal.
		/// </summary>
		Not_Equal,

		/// <summary>
		/// Used to compare if a string value begins with a sub-string value.
		/// </summary>
		Begins,

		/// <summary>
		/// Used to compare if a string value ends with a sub-string value.
		/// </summary>
		Ends,

		/// <summary>
		/// Used to compare if a string value contains a sub-string value.
		/// </summary>
		Contains,

		/// <summary>
		/// Used to compare if a value is greater than another value.
		/// </summary>
		Greater,

		/// <summary>
		/// Used to compare if a value is less than another value.
		/// </summary>
		Less,

		/// <summary>
		/// Used to compare if a value is greater than or equal to another value.
		/// </summary>
		Greater_Equal,

		/// <summary>
		/// Used to compare if a value is less than or equal to another value.
		/// </summary>
		Less_Equal,

		/// <summary>
		/// Used to test for existence of a property.
		/// </summary>
		Exists,

		/// <summary>
		/// Used to do a case sensitive compare.
		/// </summary>
		CaseEqual
	};

	/// <summary>
	/// Class that represents a member that has rights to a collection.
	/// </summary>
	[ Serializable ]
	public class MemberInfo
	{
		/// <summary>
		/// </summary>
		public string	ObjectID;

		/// <summary>
		/// The user identitifer for this object.
		/// </summary>
		public string 	UserID;

		/// <summary>
		/// </summary>
		public string	Name;

		/// <summary>
		/// the member's given (first) name
		/// or null if the property is not present.
		/// </summary>
		public string	GivenName;

		/// <summary>
		/// The member's family (last) name
		/// or null if the property is not present.
		/// </summary>
		public string	FamilyName;

		/// <summary>
		/// The member's full name
		/// or null if the property is not present.
		/// </summary>
		public string	FullName;

		/// <summary>
		/// The members's access rights.
		/// </summary>
		public int	AccessRights;

		/// <summary>
		/// Whether this Member object is the collection owner.
		/// </summary>
		public bool	IsOwner;


		/// <summary>
		/// </summary>
		public MemberInfo()
		{
		}

		//[ NonSerializable ]
		internal MemberInfo( Simias.Storage.Member member )
		{
			this.Name = member.Name;
			this.ObjectID = member.ID;
			this.UserID = member.UserID;
			this.GivenName = member.Given;
			this.FamilyName = member.Family;
			this.FullName = member.FN;
			this.AccessRights = (int) member.Rights;
			this.IsOwner = member.IsOwner;
		}
	}

	/// <summary>
	/// This is the core of the SimiasServce.  All of the methods in the
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
		/// Add a member to a domain.
		/// </summary>
		/// <param name="DomainID">The ID of the domain to add the member to.</param>
		/// <param name="MemberName">The name of the member.</param>
		/// <param name="MemberID">The ID of the member.</param>
		/// <param name="PublicKey">The public key for the member.</param>
		/// <param name="GivenName">The given name for the member.</param>
		/// <param name="FamilyName">The family name for the member.</param>
		[WebMethod(Description="Add a member to the domain.")]
		[SoapDocumentMethod]
		public void AddMemberToDomain(string DomainID, string MemberName, string MemberID, string PublicKey, string GivenName, string FamilyName)
		{
			try
			{
				Domain domain = Store.GetStore().GetDomain( DomainID );
				Simias.Storage.Member member = domain.GetMemberByName( MemberName );
				if ( member == null )
				{
					bool given;
					member = new Simias.Storage.Member( MemberName, MemberID, Access.Rights.ReadOnly );

					if ( PublicKey != null )
					{
						member.Properties.AddProperty( "PublicKey", PublicKey );
					}

					if ( GivenName != null && GivenName != "" )
					{
						member.Given = GivenName;
						given = true;
					}
					else
					{
						given = false;
					}

					if ( FamilyName != null && FamilyName != "" )
					{
						member.Family = FamilyName;
						if ( given == true )
						{
							member.FN = GivenName + " " + FamilyName;
						}
					}

					domain.Commit( member );
				}
			}
			catch{}
		}

		/// <summary>
		/// Remove a member from a domain
		/// </summary>
		/// <param name="DomainID">The ID of the domain to remove the member from.</param>
		/// <param name="MemberID">The ID of the member to remove.</param>
		[WebMethod(Description="Remove a member from the domain.")]
		[SoapDocumentMethod]
		public void RemoveMemberFromDomain(string DomainID, string MemberID)
		{
			Domain domain = Store.GetStore().GetDomain(DomainID);
			Simias.Storage.Member member = domain.GetMemberByID( MemberID );
			if ( member != null )
			{
				domain.Commit( domain.Delete( member ) );
			}
		}



		/// <summary>
		/// End the search for domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain.</param>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstMembers
		/// or FindFirstSpecificMembers methods.</param>
		[WebMethod(Description="End the search for domain members.")]
		[SoapDocumentMethod]
		public void FindCloseMembers( string domainID, string searchContext )
		{
			DomainProvider.FindCloseDomainMembers( domainID, searchContext );
		}



		/// <summary>
		/// Starts a search for all domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="totalMembers">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(Description="Starts a search for all domain members.")]
		[SoapDocumentMethod]
		public bool FindFirstMembers( 
			string domainID, 
			int count,
			out string searchContext, 
			out MemberInfo[] memberList, 
			out int totalMembers )
		{
			Member[] tempList;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers(
					domainID, 
					count,
					out searchContext, 
					out tempList, 
					out totalMembers );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				memberList = new MemberInfo[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new MemberInfo( tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}



		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="attributeName">Attribute name to search.</param>
		/// <param name="searchString">String that contains a pattern to search for.</param>
		/// <param name="operation">Type of search operation to perform.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="totalMembers">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(Description="Starts a search for a specific set of domain members.")]
		[SoapDocumentMethod]
		public bool FindFirstSpecificMembers(
			string domainID, 
			string attributeName, 
			string searchString, 
			SearchType operation, 
			int count,
			out string searchContext, 
			out MemberInfo[] memberList, 
			out int totalMembers )
		{
			Member[] tempList;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers(
					domainID,
					attributeName,
					searchString,
					( Simias.Storage.SearchOp )Enum.ToObject( typeof( Simias.Storage.SearchOp ), operation ),
					count,
					out searchContext, 
					out tempList, 
					out totalMembers );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				memberList = new MemberInfo[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new MemberInfo( tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}



		/// <summary>
		/// Continues the search for domain members from the current record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by 
		/// FindFirstMembers or FindFirstSpecificMembers methods.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(Description="Continues the search for domain members from the current record location.")]
		[SoapDocumentMethod]
		public bool FindNextMembers( 
			string domainID, 
			ref string searchContext, 
			int count,
			out MemberInfo[] memberList )
		{
			Member[] tempList;

			bool moreEntries = DomainProvider.FindNextDomainMembers( domainID, ref searchContext, count, out tempList );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				memberList = new MemberInfo[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new MemberInfo( tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}



		/// <summary>
		/// Continues the search for domain members previous to the current record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by 
		/// FindFirstMembers or FindFirstSpecificMembers methods.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(Description="Continues the search for domain members previous to the current record location.")]
		[SoapDocumentMethod]
		public bool FindPreviousMembers( 
			string domainID, 
			ref string searchContext, 
			int count,
			out MemberInfo[] memberList )
		{
			Member[] tempList;

			bool moreEntries = DomainProvider.FindPreviousDomainMembers( domainID, ref searchContext, count, out tempList );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				memberList = new MemberInfo[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new MemberInfo( tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}




		/// <summary>
		/// Continues the search for domain members from the specified record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by 
		/// FindFirstMembers or FindFirstSpecificMembers method.</param>
		/// <param name="offset">Record offset to return members from.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(Description="Continues the search for domain members from the specified record location.")]
		[SoapDocumentMethod]
		public bool FindSeekMembers( 
			string domainID, 
			ref string searchContext, 
			int offset, 
			int count, 
			out MemberInfo[] memberList )
		{
			Member[] tempList;

			bool moreEntries = DomainProvider.FindSeekDomainMembers( 
				domainID, 
				ref searchContext, 
				offset, 
				count, 
				out tempList );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				memberList = new MemberInfo[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new MemberInfo( tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
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
		/// DomainInformation object
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
				cDomainInfo = new DomainInformation(domainID);
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
		/// WebMethod to get a list of local domains
		/// </summary>
		/// <returns>
		/// Array of DomainInformation objects
		/// </returns>
		[WebMethod(Description="Get a list of local domains")]
		[SoapDocumentMethod]
		public
		DomainInformation[]
		GetDomains(bool onlySlaves)
		{
			ArrayList domains = new ArrayList();

			try
			{
				Store store = Store.GetStore();
				ICSList domainList = store.GetDomainList();
				foreach( ShallowNode shallowNode in domainList )
				{
					try
					{
						// Get the information about this domain.
						DomainInformation domainInfo = new DomainInformation(shallowNode.ID);
						if ( ( ( onlySlaves == false ) &&
							( domainInfo.Type.Equals( DomainType.Master ) || domainInfo.Type.Equals( DomainType.Slave ) ) ) ||
							( domainInfo.Type.Equals( DomainType.Slave ) ) )
						{
							domains.Add(domainInfo);
						}
					}
					catch(Exception e)
					{
						log.Error(e.Message);
						log.Error(e.StackTrace);
					}
				}
			}
			catch(Exception e)
			{
				log.Error(e.Message);
				log.Error(e.StackTrace);
			}

			return((DomainInformation[]) domains.ToArray(typeof(DomainInformation)));
		}



		/// <summary>
		/// WebMethod to login or authenticate against a 
		/// remote domain.  The user must have previously joined
		/// or attached to this domain.
		/// </summary>
		/// <returns>
		/// Simias.Client.Authentication.Status status
		/// </returns>
		[WebMethod(Description="Login or authenticate to a remote domain")]
		[SoapDocumentMethod]
		public
		Simias.Authentication.Status
		LoginToRemoteDomain(string domainID, string password)
		{ 
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(domainID);
			if( domain == null )
			{
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
			}

			Simias.Storage.Member member = domain.GetCurrentMember();
			if( member == null )
			{
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownUser );
			}

			DomainAgent domainAgent = new DomainAgent();
			return domainAgent.Login( domainID, member.Name, password );
		}



		/// <summary>
		/// WebMethod to logout from a remote domain.
		/// The user must have previously joined and 
		/// authenticated to this domain.
		/// </summary>
		/// <returns>
		/// Simias.Client.Authentication.Status status
		/// </returns>
		[WebMethod(Description="Logout from a remote domain")]
		[SoapDocumentMethod]
		public
		Simias.Authentication.Status
		LogoutFromRemoteDomain(string domainID)
		{
			new DomainAgent().SetDomainState(domainID, false, false);
			return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.Success );
		}



		/// <summary>
		/// WebMethod to disable automatic authentication to a domain.
		/// </summary>
		/// <param name="domainID">The ID of the domain to disable automatic authentication to.</param>
		[WebMethod(Description="Disable automatic authentication to the specified domain")]
		[SoapDocumentMethod]
		public void	DisableDomainAutoLogin(string domainID)
		{
			new DomainAgent().SetDomainState(domainID, false, false);
		}




		/// <summary>
		/// WebMethod to check if a domain is "active"
		/// </summary>
		/// <param name = "domainID">
		/// The specified domain to check
		/// </param>
		/// <returns>
		/// 0 success, !0 failed
		/// </returns>
		[WebMethod(Description="WebMethod to check if a domain is active")]
		[SoapDocumentMethod]
		public bool IsDomainActive(string domainID)
		{
			return( new DomainAgent().IsDomainActive( domainID ) );
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

				// domain
				Domain domain = store.GetDomain(domainID);

				// find user
				Simias.Storage.Member cMember = domain.GetMemberByID( memberID );

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
			catch(Exception e)
			{
				log.Debug(e.Message);
				log.Debug(e.StackTrace);
			}

			return(status);
		}




		/// <summary>
		/// Sets the domain credentials in the local store.
		/// </summary>
		/// <param name="domainID">The ID of the domain to set the credentials
		/// on.</param>
		/// <param name="credentials">Credentials to set.</param>
		/// <param name="type">Type of credentials.</param>
		[WebMethod(Description="Sets domain credentials in the local store")]
		[SoapDocumentMethod]
		public void SetDomainCredentials(	string domainID, 
											string credentials, 
											CredentialType type)
		{
			Store store = Store.GetStore();
			store.SetDomainCredentials(domainID, credentials, type);
		}




		/// <summary>
		/// Gets the credentials from the specified domain object.
		/// </summary>
		/// <param name="domainID">The ID of the domain to set the credentials on.</param>
		/// <param name="userID">Gets the ID of the user.</param>
		/// <param name="credentials">Gets the credentials for the domain.</param>
		/// <returns>The type of credentials.</returns>
		[WebMethod(Description="Get the saved credentials from a domain")]
		[SoapDocumentMethod]
		public CredentialType GetDomainCredentials(string domainID, out string userID, out string credentials)
		{
			Store store = Store.GetStore();
			return store.GetDomainCredentials(domainID, out userID, out credentials);
		}




		/// <summary>
		/// WebMethod that connects up an iFolder Domain
		/// </summary>
		/// <param name = "UserName">
		/// The username to use to connect to the Domain
		/// </param>
		/// <param name = "Password">
		/// The password to use to connect to the Domain
		/// </param>
		/// <param name = "Host">
		/// The host of the enterprise server
		/// </param>
		/// <returns>
		/// The Domain object associated with this Server
		/// </returns>
		[WebMethod(Description="Connects to a Domain")]
		[SoapDocumentMethod]
		public DomainInformation ConnectToDomain(string UserName,
												 string Password,
												 string Host)
		{
			DomainInformation domainInfo = null;
			DomainAgent da = new DomainAgent();
			Simias.Authentication.Status status = da.Attach(Host, UserName, Password);
			if (status.statusCode == Simias.Authentication.StatusCodes.Success ||
				status.statusCode == Simias.Authentication.StatusCodes.SuccessInGrace)
			{
				domainInfo = new DomainInformation(status.DomainID);
				domainInfo.MemberName = UserName;
			}
			else
			{
				domainInfo = new DomainInformation();
			}

			domainInfo.StatusCode = status.statusCode;

			return domainInfo;
		}




		/// <summary>
		/// WebMethod that removes a domain account from the workstation.
		/// </summary>
		/// <param name = "DomainID">
		/// The ID of the domain that the account belongs to.
		/// </param>
		/// <param name = "LocalOnly">
		/// If true then the account is only removed from this workstation.
		/// If false, then the account will be deleted from every workstation 
		/// that the user owns.
		/// </param>
		[WebMethod(Description="Removes a domain account from the workstation")]
		[SoapDocumentMethod]
		public void LeaveDomain(string DomainID,
								bool LocalOnly)
		{
			DomainAgent da = new DomainAgent();
			da.Unattach(DomainID, LocalOnly);
		}
	



		/// <summary>
		/// WebMethod that changes the default domain.
		/// </summary>
		/// <param name="domainID">The ID of the domain to set as the default.</param>
		[WebMethod(Description="Change the default domain to the specified domain ID")]
		[SoapDocumentMethod]
		public void SetDefaultDomain(string domainID)
		{
			Store store = Store.GetStore();
			store.DefaultDomain = domainID;
		}




		/// <summary>
		/// WebMethod that gets the ID of the default domain.
		/// </summary>
		/// <returns>The ID of the default domain.</returns>
		[WebMethod(Description="Get the ID of the default domain")]
		[SoapDocumentMethod]
		public string GetDefaultDomainID()
		{
			Store store = Store.GetStore();
			return store.DefaultDomain;
		}

		/// <summary>
		/// WebMethod to get the certificate for the specified host.
		/// </summary>
		/// <param name="host"></param>
		/// <returns></returns>
		[WebMethod(Description="Get the certificate of the specified host.")]
		[SoapDocumentMethod]
		public byte[] GetCertificate(string host)
		{
			return Simias.Client.CertPolicy.GetCertificate(host);
		}

		/// <summary>
		/// WebMethod to Store the certificate for the specified host.
		/// </summary>
		/// <param name="certificate">The certificate to store.</param>
		/// <param name="host">The host the certificate belongs to.</param>
		[WebMethod(Description="Store the certificate for the specified host.")]
		[SoapDocumentMethod]
		public void StoreCertificate(byte[] certificate, string host)
		{
			Simias.Client.CertPolicy.StoreCertificate(certificate, host, true);
		}
	}




	/// <summary>
	/// Type of Domain (Enterprise/Workgroup)
	/// </summary>
	[Serializable]
	public enum DomainType
	{
		/// <summary>
		/// A Master Role
		/// </summary>
		Master,

		/// <summary>
		/// A Slave Role
		/// </summary>
		Slave,

		/// <summary>
		/// A Local Role
		/// </summary>
		Local,

		/// <summary>
		/// No Role
		/// </summary>
		None
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
		/// Indicates if the domain is authenticated or not.
		/// </summary>
		public bool Authenticated;

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
		/// The unique member/user ID.
		/// </summary>
		public string MemberUserID;

		/// <summary>
		/// The name of the member object
		/// </summary>
		public string MemberName;

		/// <summary>
		/// Url to the remote domain service
		/// </summary>
		public string RemoteUrl;

		/// <summary>
		/// POBox ID
		/// </summary>
        public string POBoxID;

		/// <summary>
		/// The host for this domain.
		/// </summary>
		public string Host;

		/// <summary>
		/// <b>True</b> if the local domain is a slave (client).
		/// </summary>
		public bool IsSlave;

		/// <summary>
		/// <b>True</b> if the local domain is the default domain.
		/// </summary>
		public bool IsDefault;

		/// <summary>
		/// The status of the authentication request.
		/// </summary>
		public Simias.Authentication.StatusCodes StatusCode;

		/// <summary>
		/// Constructor
		/// </summary>
		public DomainInformation()
		{
		}

		/// <summary>
		/// Constructs a DomainInformation object.
		/// </summary>
		/// <param name="domainID">The ID of the domain to base this object on.</param>
		public DomainInformation(string domainID)
		{
			Store store = Store.GetStore();

			Domain cDomain = store.GetDomain(domainID);
			Simias.Storage.Member cMember = cDomain.GetCurrentMember();
			Simias.POBox.POBox poBox = 
				Simias.POBox.POBox.FindPOBox(store, domainID, cMember.UserID);
			this.POBoxID = ( poBox != null ) ? poBox.ID : "";
			DomainAgent domainAgent = new DomainAgent();
			this.Active = domainAgent.IsDomainActive(cDomain.ID);
			this.Authenticated = domainAgent.IsDomainAuthenticated(cDomain.ID);
			this.Type = GetDomainTypeFromRole(cDomain.Role);
			this.ID = domainID;
			this.Name = cDomain.Name;
			this.Description = cDomain.Description;
			this.MemberUserID = cMember.UserID;
			this.MemberName = cMember.Name;

			Uri uri = DomainProvider.ResolveLocation(domainID);
			this.RemoteUrl = (uri != null) ?
				uri.ToString() + "/DomainService.asmx" :
				String.Empty;

			this.Host = (uri != null) ? uri.ToString() : String.Empty;
			this.IsSlave = cDomain.Role.Equals(Simias.Sync.SyncRoles.Slave);
			this.IsDefault = domainID.Equals(store.DefaultDomain);
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
			builder.AppendFormat("  Member User ID   : {0}{1}", this.MemberUserID, newLine);
			builder.AppendFormat("  Member Node Name : {0}{1}", this.MemberName, newLine);
			builder.AppendFormat("  Remote Url       : {0}{1}", this.RemoteUrl, newLine);
			builder.AppendFormat("  POBox ID         : {0}{1}", this.POBoxID, newLine);
			builder.AppendFormat("  Host             : {0}{1}", this.Host, newLine);

			return builder.ToString();
		}

		private DomainType GetDomainTypeFromRole(SyncRoles role)
		{
			DomainType type = DomainType.None;

			switch (role)
			{
				case SyncRoles.Master:
					type = DomainType.Master;
					break;

				case SyncRoles.Slave:
					type = DomainType.Slave;
					break;

				case SyncRoles.Local:
					type = DomainType.Local;
					break;
			}

			return type;
		}
	}
}




