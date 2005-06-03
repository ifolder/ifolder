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
using System.Collections;
using System.Net;
using System.IO;
using System.Threading;
using System.Web;
using System.Xml;

using Simias;
using Simias.Authentication;
using Simias.Client;
using Simias.POBox;
using Simias.Security.Web.AuthenticationService;
using Simias.Storage;
using Simias.Sync;

using Novell.Security.ClientPasswordManager;

// Alias
using PostOffice = Simias.POBox;
using SCodes = Simias.Authentication.StatusCodes;


namespace Simias.DomainServices
{
	/// <summary>
	/// Simias Domain Agent
	/// </summary>
	public class DomainAgent
	{
		#region Class Members
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(DomainAgent));
	
		/// <summary>
		/// Service type for this service.
		/// </summary>
		private static string DomainServiceType = "Domain Service";
		private static string DomainServicePath = "/simias10/DomainService.asmx";

		/// <summary>
		/// Property name for declaring a domain active/inactive.
		/// If the property doesn't exist on a Domain, then that
		/// domain by default is active
		/// </summary>
		private readonly string activePropertyName = "Active";

		private Store store = Store.GetStore();
		private static Hashtable domainTable = new Hashtable();
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		public DomainAgent()
		{
		}
		#endregion

		#region Private Methods
		private void CreateDomainProxy(Store store, string userID, DomainInfo info, Uri hostAddress)
		{
			// Make sure the domain doesn't exist.
			if (store.GetCollectionByID(info.ID) == null)
			{
				log.Debug("Creating Domain Proxy: {0}", info.Name);

				// Create a new domain
				Domain domain = new Domain(store, info.Name, info.ID, info.Description, SyncRoles.Slave, Domain.ConfigurationType.ClientServer);
				domain.Proxy = true;
				domain.SetType( domain, "Enterprise" );
			
				// Mark the domain inactive until we get the POBox created
				Property p = new Property( activePropertyName, false );
				p.LocalProperty = true;
				domain.Properties.AddNodeProperty( p );

				p = new Property( PropertyTags.HostAddress, hostAddress );
				p.LocalProperty = true;
				domain.Properties.AddNodeProperty( p );

				// Create domain member.
				Access.Rights rights = ( Access.Rights )Enum.Parse( typeof( Access.Rights ), info.MemberRights );
				Member member = new Member( info.MemberNodeName, info.MemberNodeID, userID, rights, null );
				member.Proxy = true;
				member.IsOwner = true;

				// commit
				domain.Commit( new Node[] { domain, member } );
			}
		}

		private void CreatePOBoxProxy(Store store, string domainID, ProvisionInfo info)
		{
			if (store.GetCollectionByID(info.POBoxID) == null)
			{
				log.Debug( "Creating PO Box Proxy: {0}", info.POBoxName );

				// Create a new POBox
				PostOffice.POBox poBox = new PostOffice.POBox(store, info.POBoxName, info.POBoxID, domainID);
				poBox.Priority = 0;
				poBox.Proxy = true;
			
				// Create member.
				Access.Rights rights = ( Access.Rights )Enum.Parse( typeof( Access.Rights ), info.MemberRights );
				Member member = new Member( info.MemberNodeName, info.MemberNodeID, info.UserID, rights, null );
				member.Proxy = true;
				member.IsOwner = true;
			
				// commit
				poBox.Commit( new Node[] { poBox, member } );
			}
		}

		/// <summary>
		/// Login to a remote domain using username and password
		/// Assumes a slave domain has been provisioned locally
		/// </summary>
		/// <param name="host">The uri to the host.</param>
		/// <param name="domainID">ID of the remote domain.</param>
		/// <param name="networkCredential">The credentials to authenticate with.</param>
		/// <param name="calledRecursive">True if called recursively.</param>
		/// <returns>
		/// The status of the remote authentication
		/// </returns>
		private 
		Simias.Authentication.Status
		Login(Uri host, string domainID, NetworkCredential networkCredential, bool calledRecursive)
		{
			HttpWebResponse response = null;

			Simias.Authentication.Status status =	
				new Simias.Authentication.Status( SCodes.Unknown );

			Uri loginUri = 
				new Uri( host, Simias.Security.Web.AuthenticationService.Login.Path );
			HttpWebRequest request = WebRequest.Create( loginUri ) as HttpWebRequest;
			WebState webState = new WebState(domainID);
			webState.InitializeWebRequest(request, domainID);
			request.Credentials = networkCredential;
			request.PreAuthenticate = true;
			
			if ( domainID != null && domainID != "")
			{
				request.Headers.Add( 
					Simias.Security.Web.AuthenticationService.Login.DomainIDHeader,
					domainID);
			}

			request.Headers.Add(
				Simias.Security.Web.AuthenticationService.Login.BasicEncodingHeader,
				System.Text.Encoding.Default.WebName );
			
			request.Method = "POST";
			request.ContentLength = 0;

			try
			{
				request.GetRequestStream().Close();
				response = request.GetResponse() as HttpWebResponse;
				if ( response != null )
				{
					string grace = 
						response.GetResponseHeader( 
							Simias.Security.Web.AuthenticationService.Login.GraceTotalHeader );
					if ( grace != null && grace != "" )
					{
						status.statusCode = SCodes.SuccessInGrace;
						status.TotalGraceLogins = Convert.ToInt32( grace );

						grace = 
							response.GetResponseHeader( 
								Simias.Security.Web.AuthenticationService.Login.GraceRemainingHeader );
						if ( grace != null && grace != "" )
						{
							status.RemainingGraceLogins = Convert.ToInt32( grace );
						}
						else
						{
							// fail to worst case
							status.RemainingGraceLogins = 0;
						}
					}
					else
					{
						status.statusCode = SCodes.Success;
					}
				}
			}
			catch(WebException webEx)
			{
				if (webEx.Status == WebExceptionStatus.TrustFailure)
				{
					// The Certificate is invalid.
					status.statusCode = SCodes.InvalidCertificate;
				}
				else
				{
					response = webEx.Response as HttpWebResponse;
					if (response != null)
					{
						// Look for our special header to give us more
						// information why the authentication failed
						string iFolderError = 
							response.GetResponseHeader( 
							Simias.Security.Web.AuthenticationService.Login.SimiasErrorHeader );

						if ( iFolderError != null && iFolderError != "" )
						{
							if ( iFolderError == StatusCodes.InvalidPassword.ToString() )
							{
								status.statusCode = SCodes.InvalidPassword;
							}
							else
								if ( iFolderError == StatusCodes.AccountDisabled.ToString() )
							{
								status.statusCode = SCodes.AccountDisabled;
							}
							else
								if ( iFolderError == StatusCodes.AccountLockout.ToString() )
							{
								status.statusCode = SCodes.AccountLockout;
							}
							else
								if ( iFolderError == StatusCodes.AmbiguousUser.ToString() )
							{
								status.statusCode = SCodes.AmbiguousUser;
							}
							else
								if ( iFolderError == StatusCodes.UnknownDomain.ToString() )
							{
								status.statusCode = SCodes.UnknownDomain;
							}
							else
								if ( iFolderError == StatusCodes.InternalException.ToString() )
							{
								status.statusCode = SCodes.InternalException;
							}
							else
								if ( iFolderError == StatusCodes.UnknownUser.ToString() )
							{
								status.statusCode = SCodes.UnknownUser;
							}
							else
								if ( iFolderError == StatusCodes.MethodNotSupported.ToString() )
							{
								status.statusCode = SCodes.MethodNotSupported;
							}
							else
								if ( iFolderError == StatusCodes.InvalidCredentials.ToString() )
							{
								// This could have failed because of iChain.
								// Check for a via header.
								string viaHeader = response.Headers.Get("via");
								if (viaHeader != null && !calledRecursive)
								{
									// Try again.
									return Login(host, domainID, networkCredential, true);
								}
								status.statusCode = SCodes.InvalidCredentials;
							}
							else
								if ( iFolderError == StatusCodes.SimiasLoginDisabled.ToString() )
							{
								status.statusCode = SCodes.SimiasLoginDisabled;
							}
						}
						else if (response.StatusCode == HttpStatusCode.Unauthorized)
						{
							// This call is a free call on the server.
							// If we get a 401 we must have iChain between us.
							// The user was invalid.
							status.statusCode = SCodes.UnknownUser;
						}
					}
					else
					{
						log.Debug(webEx.Message);
						log.Debug(webEx.StackTrace);
					}
				}
			}
			catch(Exception ex)
			{
				log.Debug(ex.Message);
				log.Debug(ex.StackTrace);
			}

			return status;
		}

		#endregion

		#region Internal Methods
		/// <summary>
		/// Removes all traces of the domain from this machine.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to remove.</param>
		internal void RemoveDomainInformation( string domainID )
		{
			// Cannot remove the local domain.
			if ( domainID == store.LocalDomain )
			{
				throw new SimiasException("The local domain cannot be removed.");
			}

			// If the default domain is the one that is being deleted, set a new one.
			if (store.DefaultDomain == domainID)
			{
				// If there are no other domains present, there is no default.
				string defaultDomain = null;

				// Set the new default domain.
				ICSList dList = store.GetDomainList();
				foreach(ShallowNode sn in dList)
				{
					// Find the first domain that is not the one being deleted or is the
					// local domain.
					if ((sn.ID != domainID) && (sn.ID != store.LocalDomain))
					{
						defaultDomain = sn.ID;
						break;
					}
				}

				// Set the new default domain.
				store.DefaultDomain = defaultDomain;
			}

			// Get a list of all the collections that belong to this domain and delete them.
			ICSList cList = store.GetCollectionsByDomain(domainID);
			foreach(ShallowNode sn in cList)
			{
				Collection c = new Collection(store, sn);
				c.Commit(c.Delete());
			}

			// Remove the local domain information.
			store.DeleteDomainIdentity(domainID);
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Attach to an enterprise system.
		/// </summary>
		/// <param name="host">Url of the enterprise server.</param>
		/// <param name="user">User to provision on the server.</param>
		/// <param name="password">Password to validate user.</param>
		/// <returns>
		/// The Domain ID of the newly attached Domain
		/// </returns>
		public Simias.Authentication.Status Attach(string host, string user, string password)
		{
			Store store = Store.GetStore();

			// Get a URL to our service.
			Uri domainServiceUrl = null;
			Simias.Authentication.Status status = null;
			try
			{
				domainServiceUrl = WSInspection.GetServiceUrl( host, DomainServiceType, user, password );
			}
			catch (WebException we)
			{
				if (we.Status == WebExceptionStatus.TrustFailure)
				{
					status = new Simias.Authentication.Status();
					status.statusCode = Simias.Authentication.StatusCodes.InvalidCertificate;
					return status;
				}
				else if ((we.Status == WebExceptionStatus.Timeout) || 
						 (we.Status == WebExceptionStatus.NameResolutionFailure))
				{
					status = new Simias.Authentication.Status();
					status.statusCode = Simias.Authentication.StatusCodes.UnknownDomain;
					return status;
				}
			}
			if ( domainServiceUrl == null )
			{
				// There was a failure in obtaining the service url. Try a hard coded one.
				domainServiceUrl = new Uri( Uri.UriSchemeHttp + Uri.SchemeDelimiter + host + DomainServicePath );
			}

			// Build a credential from the user name and password.
			NetworkCredential myCred = new NetworkCredential( user, password ); 

			// Create the domain service web client object.
			DomainService domainService = new DomainService();
			domainService.Url = domainServiceUrl.ToString();
			domainService.Credentials = myCred;
			domainService.Proxy = ProxyState.GetProxyState( domainServiceUrl );

			// Check to see if this domain already exists in this store.
			string domainID = domainService.GetDomainID();
			if ( ( domainID != null ) && ( store.GetDomain( domainID ) != null ) )
			{
				throw new ExistsException( String.Format( "Domain {0}", domainID ) );
			}

			status = 
				this.Login( 
					new Uri( domainServiceUrl.Scheme + Uri.SchemeDelimiter + host ), 
					domainID,
					myCred,
					false);
			if ( ( status.statusCode != SCodes.Success ) && ( status.statusCode != SCodes.SuccessInGrace ) )
			{
				return status;
			}

			// Get just the path portion of the URL.
			string hostString = domainServiceUrl.ToString();
			int startIndex = hostString.LastIndexOf( "/" );
			Uri hostUri = new Uri( hostString.Remove( startIndex, hostString.Length - startIndex ) );

			// The web state object lets the connection use common state information.
			WebState webState = new WebState(domainID);
			webState.InitializeWebClient(domainService, domainID);

			// Save the credentials
			CredentialCache myCache = new CredentialCache();
			myCache.Add(new Uri(domainService.Url), "Basic", myCred);
			domainService.Credentials = myCache;
			domainService.Timeout = 30000;

			log.Debug("Calling " + domainService.Url + " to provision the user");

			// provision user
			ProvisionInfo provisionInfo = domainService.ProvisionUser(user, password);
			if (provisionInfo == null)
			{
				throw new ApplicationException("User does not exist on server.");
			}
				
			log.Debug("the user has been provisioned on the remote domain");

			// get domain info
			DomainInfo domainInfo = domainService.GetDomainInfo(provisionInfo.UserID);

			// Create domain proxy
			CreateDomainProxy(store, provisionInfo.UserID, domainInfo, hostUri);

			// Create PO Box proxy
			CreatePOBoxProxy(store, domainInfo.ID, provisionInfo);

			// create domain identity mapping.
			store.AddDomainIdentity(domainInfo.ID, provisionInfo.UserID);

			// authentication was successful - save the credentials
			new NetCredential( "iFolder", domainInfo.ID, true, user, password );

			// Domain is ready to sync
			this.SetDomainActive( domainInfo.ID );
			status.DomainID = domainInfo.ID;
			return status;
		}

		/// <summary>
		/// Check if the domain is marked Active or in a connected state
		/// </summary>
		/// <param name="domainID">The identifier of the domain to check status on.</param>
		public bool IsDomainActive(string domainID)
		{
			bool active = true;

			try
			{
				Domain cDomain = store.GetDomain( domainID );
					
				// Make sure this domain is a slave 
				if ( cDomain.Role == SyncRoles.Slave )
				{
					Property p = 
						cDomain.Properties.GetSingleProperty( this.activePropertyName );

					if ( p != null && (bool) p.Value == false )
					{
						active = false;
					}
				}
			}
			catch{}
			return active;
		}

		/// <summary>
		/// Login to a remote domain using username and password
		/// Assumes a slave domain has been provisioned locally
		/// </summary>
		/// <param name="domainID">ID of the remote domain.</param>
		/// <param name="user">Member to login as</param>
		/// <param name="password">Password to validate user.</param>
		/// <returns>
		/// The status of the remote authentication
		/// </returns>
		public 
		Simias.Authentication.Status
		Login(string domainID, string user, string password)
		{
			Simias.Authentication.Status status = null;
			Domain cDomain = store.GetDomain( domainID );
			if ( cDomain != null )
			{
				if ( cDomain.Role == SyncRoles.Slave )
				{
					NetworkCredential netCred = new NetworkCredential( user, password );
					status = this.Login( DomainProvider.ResolveLocation(domainID), domainID, netCred, false );
					if ( status.statusCode == SCodes.Success ||
						status.statusCode == SCodes.SuccessInGrace )
					{
						new NetCredential( "iFolder", domainID, true, user, password );
						SetDomainState(domainID, true, true);
					}
				}
				else
				{
					status = new Simias.Authentication.Status( SCodes.UnknownDomain );
				}
			}
			else
			{
				status = new Simias.Authentication.Status( SCodes.UnknownDomain );
			}

			return status;
		}

		/// <summary>
		/// Logout from a domain.
		/// </summary>
		/// <param name="domainID">The ID of the domain.</param>
		/// <returns>The status of the logout.</returns>
		public
		Simias.Authentication.Status
		Logout(string domainID)
		{
			// Get the domain.
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(domainID);
			if( domain == null )
			{
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
			}

			// Set the state for this domain.
			SetDomainState(domainID, false, false);

			// Clear the password from the cache.
			Member member = domain.GetMemberByID( store.GetUserIDFromDomainID( domainID ) );
			if ( member != null )
			{
				// Clear the entry from the cache.
				NetCredential netCredential = new NetCredential( "iFolder", domainID, true, member.Name, null );
				Uri uri = new Uri(DomainProvider.ResolveLocation(domainID), "/DomainService.asmx");
				netCredential.Remove(uri, "BASIC");
			}
			// Clear the cookies for this Uri.
			WebState.ResetWebState(domainID);

			return new Simias.Authentication.Status(SCodes.Success);
		}

		/// <summary>
		/// Sets the status of the specified domain to Active.
		/// </summary>
		/// <param name="domainID">The identifier of the domain.</param>
		public void SetDomainActive(string domainID)
		{
			try
			{
				Domain cDomain = store.GetDomain( domainID );
				if ( cDomain.Role == SyncRoles.Slave )
				{
					Property p = new Property( this.activePropertyName, true );
					p.LocalProperty = true;
					cDomain.Properties.ModifyNodeProperty( p );
					cDomain.Commit();
				}
			}
			catch( Exception e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace);
			}
		}

		/// <summary>
		/// Sets the status of the specified domain to Inactive.
		/// setting a domain to inactive will disable all 
		/// synchronization activity.
		/// </summary>
		/// <param name="domainID">The identifier of the domain.</param>
		public void SetDomainInactive(string domainID)
		{
			try
			{
				Domain cDomain = store.GetDomain( domainID );
				if ( cDomain.Role == SyncRoles.Slave )
				{
					Property p = new Property( this.activePropertyName, false );
					p.LocalProperty = true;
					cDomain.Properties.ModifyNodeProperty( p );
					cDomain.Commit();
				}
			}
			catch( Exception e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace);
			}
		}

		/// <summary>
		/// Removes this workstation from the domain or removes the workstation from all machines
		/// owned by the user.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to remove.</param>
		/// <param name="localOnly">If true then only this workstation is removed from the domain.
		/// If false, then the domain will be deleted from every workstation that the user owns.</param>
		public void Unattach(string domainID, bool localOnly)
		{
			// Cannot remove the local domain.
			if ( domainID == store.LocalDomain )
			{
				throw new SimiasException("The local domain cannot be removed.");
			}

			// Get the domain object.
			Simias.Storage.Domain domain = store.GetDomain(domainID);
			if (domain == null)
			{
				throw new SimiasException("The domain does not exist in the store.");
			}

			// Get who the user is in the specified domain.
			string userID = store.GetUserIDFromDomainID(domainID);
			Member member = domain.GetMemberByID( store.GetUserIDFromDomainID( domainID ) );
			
			// This information needs to be gathered before the local domain collections are deleted.
			// Set the address to the server.
			Uri uri = DomainProvider.ResolveLocation(domainID);
			if (uri == null)
			{
				throw new SimiasException(String.Format("Cannot get location for domain {0}.", domain.Name));
			}
			// Construct the web client.
			DomainService domainService = new DomainService();
			domainService.Url = uri.ToString() + "/DomainService.asmx";
			if (!localOnly)
			{
				WebState webState = new WebState(domainID, userID);
				webState.InitializeWebClient(domainService, domainID);
			}

			
			// Find the user's POBox for this domain.
			POBox.POBox poBox = POBox.POBox.FindPOBox(store, domainID, userID);
			if (poBox == null)
			{
				throw new SimiasException(String.Format("Cannot find POBox belonging to domain {0}", domainID));
			}

			try
			{
				// Delete the POBox for this domain which will start the domain cleanup process.
				poBox.Commit(poBox.Delete());

				// Remove the domain from the table
				lock (domainTable)
				{
					domainTable.Remove(domainID);
				}

				if (!localOnly)
				{
					// Remove the user from the domain server.
					domainService.RemoveServerCollections(domainID, userID);
				}
			}
			finally
			{
				// Clear the password from the cache.
				if (member != null)
				{
					NetCredential netCredential = new NetCredential( "iFolder", domainID, true, member.Name, null );
					uri = new Uri(uri, "/DomainService.asmx");
					netCredential.Remove(uri, "BASIC");
				}
				// Clear the cookies for this Uri.
				WebState.ResetWebState(domainID);
			}
		}

		/// <summary>
		/// Create the master on the server.
		/// </summary>
		/// <param name="collection">Collection to create on the enterprise server.</param>
		public void CreateMaster(Collection collection)
		{
			// Get the network location of the server where this collection is to be created.
			Uri uri = DomainProvider.ResolveLocation(collection);
			if (uri == null)
			{
				throw new SimiasException(String.Format("The network location could not be determined for collection {0}.", collection.ID));
			}

			// Construct the web client.
			DomainService domainService = new DomainService();
			domainService.Url = uri.ToString() + "/DomainService.asmx";
			WebState webState = new WebState(collection.Domain, store.GetUserIDFromDomainID(collection.Domain));
			webState.InitializeWebClient(domainService, collection.Domain);
			
			string rootID = null;
			string rootName = null;

			DirNode rootNode = collection.GetRootDirectory();
			if (rootNode != null)
			{
				rootID = rootNode.ID;
				rootName = rootNode.Name;
			}

			Member member = collection.Owner;

			domainService.CreateMaster(
				collection.ID, 
				collection.Name, 
				rootID, 
				rootName, 
				member.UserID, 
				member.Name, 
				member.ID, 
				member.Rights.ToString() );

			collection.CreateMaster = false;
			collection.Commit();
		}

		public void SetDomainState(string DomainID, bool Authenticated, bool AutoLogin)
		{
			lock (domainTable)
			{
				DomainState domainState = (DomainState)domainTable[DomainID];
				if (domainState == null)
				{
					domainState = new DomainState();
				}

				domainState.Authenticated = Authenticated;
				domainState.AutoLogin = AutoLogin;
				domainTable[DomainID] = domainState;
			}
		}

		public bool IsDomainAuthenticated(string DomainID)
		{
			DomainState domainState;

			lock (domainTable)
			{
				domainState = (DomainState)domainTable[DomainID];
			}

			if (domainState == null)
			{
				return false;
			}
			
			return domainState.Authenticated;
		}

		public bool IsDomainAutoLoginEnabled(string DomainID)
		{
			DomainState domainState;

			lock (domainTable)
			{
				domainState = (DomainState)domainTable[DomainID];
			}

			if (domainState == null)
			{
				return true;
			}

			return domainState.AutoLogin;
		}
		#endregion
	}

	internal class DomainState
	{
		#region Class Members
		private bool authenticated = false;
		private bool autoLogin = true;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a DomainState object.
		/// </summary>
		public DomainState()
		{
		}

		/// <summary>
		/// Constructs a DomainState object.
		/// </summary>
		/// <param name="Authenticated">A value indicating if the domain has been authenticated to.</param>
		/// <param name="AutoLogin">A value indicating if the client should attempt to automatically
		/// login to the domain.</param>
		public DomainState(bool Authenticated, bool AutoLogin)
		{
			authenticated = Authenticated;
			autoLogin = AutoLogin;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Get/sets a value indicating if the domain has been authenticated to.
		/// </summary>
		public bool Authenticated
		{
			get { return authenticated; }
			set { authenticated = value; }
		}

		/// <summary>
		/// Gets/sets a value indicating if the client should attempt to
		/// automatically login to the domain.
		/// </summary>
		public bool AutoLogin
		{
			get { return autoLogin; }
			set { autoLogin = value; }
		}
		#endregion
	}			  
}
