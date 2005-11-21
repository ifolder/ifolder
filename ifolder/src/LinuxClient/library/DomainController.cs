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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Authors:
 *		Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/

using System;
using System.Collections;

using Novell.iFolder.Events;

using Simias.Client;
using Simias.Client.Authentication;

namespace Novell.iFolder.Controller
{
	public class DomainController
	{
		private static DomainController instance = null;
		
		/// <summary>
		/// Member that provides acces to the iFolder Web Service
		/// </summary>
		private iFolderWebService ifws = null;


		/// <summary>
		/// Member that provides acces to the Simias Web Service
		/// </summary>
		private SimiasWebService simws = null;

		/// <summary>
		/// Hashtable to hold the domains
		/// </summary>
		private Hashtable keyedDomains;

		/// <summary>
		/// Member to keep track of the default domain
		/// </summary>
		private string defDomainID = null;
		
		private SimiasEventBroker eventBroker = null;
		private Manager simiasManager;
		
		///
		/// Events
		///
		public event DomainAddedEventHandler DomainAdded;
		public event DomainDeletedEventHandler DomainDeleted;
		public event DomainHostModifiedEventHandler DomainHostModified;
		public event DomainLoggedInEventHandler DomainLoggedIn;
		public event DomainLoggedOutEventHandler DomainLoggedOut;
		public event DomainUpEventHandler DomainUp;
		public event DomainNeedsCredentialsEventHandler DomainNeedsCredentials;
		public event DomainActivatedEventHandler DomainActivated;
		public event DomainInactivatedEventHandler DomainInactivated;
		public event DomainNewDefaultEventHandler NewDefaultDomain;
		public event DomainInGraceLoginPeriodEventHandler DomainInGraceLoginPeriod;
		
		private DomainController()
		{
			this.simiasManager = Util.GetSimiasManager();
			string localServiceUrl = simiasManager.WebServiceUri.ToString();
			try
			{
				ifws = new iFolderWebService();
				ifws.Url = localServiceUrl + "/iFolder.asmx";
				LocalService.Start(ifws, simiasManager.WebServiceUri, simiasManager.DataPath);
			}
			catch(Exception e)
			{
				ifws = null;
				throw new Exception("Unable to create ifolder web service in iFolderController");
			}
			try
			{
				simws = new SimiasWebService();
				simws.Url = localServiceUrl + "/Simias.asmx";
				LocalService.Start(simws, simiasManager.WebServiceUri, simiasManager.DataPath);
			}
			catch(Exception e)
			{
				simws = null;
				throw new Exception("Unable to create simias web service in iFolderController");
			}

			keyedDomains = new Hashtable();
			
			Refresh();

			// Register with the SimiasEventBroker to get Simias Events
			eventBroker = SimiasEventBroker.GetSimiasEventBroker(simiasManager);
			if (eventBroker != null)
			{
				eventBroker.DomainUpEventFired +=
					new DomainUpEventHandler(OnDomainUpEvent);
				eventBroker.DomainAdded +=
					new DomainAddedEventHandler(OnDomainAddedEvent);
				eventBroker.DomainDeleted +=
					new DomainDeletedEventHandler(OnDomainDeletedEvent);
			}
		}
		
		~DomainController()
		{
			if (eventBroker != null)
			{
				eventBroker.DomainUpEventFired -=
					new DomainUpEventHandler(OnDomainUpEvent);
				eventBroker.DomainAdded -=
					new DomainAddedEventHandler(OnDomainAddedEvent);
				eventBroker.DomainDeleted -=
					new DomainDeletedEventHandler(OnDomainDeletedEvent);
			}
		}
		
		public static DomainController GetDomainController()
		{
			lock (typeof(DomainController))
			{
				if (instance == null)
				{
					instance = new DomainController();
				}
				
				return instance;
			}
		}
		
		/// <summary>
		/// Reads the Simias Domains by calling the Simias Web Service
		/// </summary>
		public void Refresh()
		{
			lock (typeof(DomainController))
			{
				// Refresh the Domains
				keyedDomains.Clear();
				DomainInformation[] domains = null;
				try
				{
					domains = simws.GetDomains(false);
				}
				catch(Exception e)
				{
					domains = null;
				}

				if(domains != null)
				{
					foreach(DomainInformation domain in domains)
					{
						if(domain.IsDefault)
							defDomainID = domain.ID;

						AddDomainToHashtable(domain);
					}
				}
			}
		}

		/// <summary>
		/// Returns an array of the current Simias Domains
		/// </summary>
		public DomainInformation[] GetDomains()
		{
			lock(typeof(DomainController))
			{
				DomainInformation[] domains = new DomainInformation[keyedDomains.Count];

				ICollection icol = keyedDomains.Values;
				icol.CopyTo(domains, 0);

				return domains;
			}
		}

		/// <summary>
		/// Returns the domain marked as the default
		/// </summary>
		public DomainInformation GetDefaultDomain()
		{
			lock(typeof(DomainController))
			{
				if (keyedDomains.Contains(defDomainID))
					return (DomainInformation)keyedDomains[defDomainID];
				else
				{
					defDomainID = null;
					return null;
				}
			}
		}
		
		/// <summary>
		/// Sets a new default domain
		/// </summary>
		public void SetDefaultDomain(string domainID)
		{
			lock(typeof(DomainController))
			{
				if (defDomainID == null || domainID != defDomainID)
				{
					if (!keyedDomains.Contains(domainID))
					{
						// FIXME: Change this to InvalidDomainException
						throw new Exception("InvalidDomainException");
					}
					
					DomainInformation dom = (DomainInformation)keyedDomains[domainID];
					
					try
					{
						simws.SetDefaultDomain(domainID);
						dom.IsDefault = true;
						string oldDomainID = defDomainID;
						defDomainID = domainID;

						if (oldDomainID != null && keyedDomains.Contains(oldDomainID))
						{
							DomainInformation oldDefaultDomain = (DomainInformation)keyedDomains[oldDomainID];
							oldDefaultDomain.IsDefault = false;
						}
						
						if (NewDefaultDomain != null)
							NewDefaultDomain(this, new NewDefaultDomainEventArgs(oldDomainID, defDomainID));
					}
					catch (Exception e)
					{
						throw e;
					}
				}
			}
		}

		/// <summary>
		/// Returns the specified domain
		/// </summary>
		public DomainInformation GetDomain(string domainID)
		{
			lock(typeof(DomainController))
			{
				if (keyedDomains.Contains(domainID))
					return (DomainInformation)keyedDomains[domainID];
				else
					return null;
			}
		}
		
		/// <summary>
		/// Call this to connect and authenticate to a brand new domain
		/// </summary>
		public DomainInformation AddDomain(string host, string username, string password, bool bSavePassword, bool bSetAsDefault)
		{
			DomainInformation dom = null;
			
			SetHttpProxyForHost(host);

			try
			{
				dom = simws.ConnectToDomain(username, password, host);
				if (dom != null &&
					(dom.StatusCode == StatusCodes.Success ||
					 dom.StatusCode == StatusCodes.SuccessInGrace))
				{
					// Add this Domain to our cache and notify handlers
					AddDomainToHashtable(dom);
					
					if (bSetAsDefault)
					{
						try
						{
							simws.SetDefaultDomain(dom.ID);
							dom.IsDefault = true;
							
							string oldDomainID = null;
							if (defDomainID != null && defDomainID != dom.ID)
							{
								oldDomainID = defDomainID;
								lock (typeof(DomainController))
								{
									DomainInformation oldDefaultDomain = (DomainInformation)keyedDomains[oldDomainID];
									if (oldDefaultDomain != null)
										oldDefaultDomain.IsDefault = false;
								}
							}
							
							defDomainID = dom.ID;
							
							if (NewDefaultDomain != null)
								NewDefaultDomain(this, new NewDefaultDomainEventArgs(oldDomainID, defDomainID));
						}
						catch {}
					}

					// Notify DomainAddedEventHandlers
					if (DomainAdded != null)
						DomainAdded(this, new DomainEventArgs(dom.ID));
				}
			}
			catch (Exception e)
			{
				if (e.Message.IndexOf("Simias.ExistsException") != -1 ||
					e.Message.IndexOf("already exists") != -1)
				{
					throw new DomainAccountAlreadyExistsException("An account with this domain already exists");
				}
				else
				{
					throw e;
				}
			}
			
			return dom;
		}
		
		public DomainInformation UpdateDomainHostAddress(string domainID, string host)
		{
			lock (typeof(DomainController))
			{
				DomainInformation dom = (DomainInformation)keyedDomains[domainID];
				if (dom != null)
				{
					if (String.Compare(dom.Host, host, true) != 0)
					{
						try
						{
							simws.SetDomainHostAddress(domainID, host);
							
							dom = simws.GetDomainInformation(domainID);
							keyedDomains[domainID] = dom;
	
							// Notify DomainHostModifiedEventHandlers
							if (DomainHostModified != null)
								DomainHostModified(this, new DomainEventArgs(domainID));
						}
						catch (Exception e)
						{
							// FIXME: Determine if any exceptions can be thrown by this
							throw e;
						}
					}
	
					dom.StatusCode = StatusCodes.Success;
				}
				else
				{
					// FIXME: Throw DomainDoesNotExistException
				}
				
				return dom;
			}
		}

		/// <summary>
		/// Call this to remove/detach from a Simias Domain
		/// </summary>
		public void RemoveDomain(string domainID, bool deleteiFoldersOnServer)
		{
			simws.LeaveDomain(domainID, !deleteiFoldersOnServer);
		}

		/// <summary>
		/// Authenticate to the domain with the specified password
		/// </summary>
		public Status AuthenticateDomain(string domainID, string password, bool bSavePassword)
		{
			Status status;
			DomainAuthentication domainAuth =
				new DomainAuthentication(
					"iFolder",
					domainID,
					password);

			try
			{
				status = domainAuth.Authenticate(simiasManager.WebServiceUri, simiasManager.DataPath);
				if (status.statusCode == StatusCodes.Success ||
					status.statusCode == StatusCodes.SuccessInGrace)
				{
					if (bSavePassword)
					{
						try
						{
							if (password != null && password.Length > 0)
								simws.SetDomainCredentials(domainID, password, CredentialType.Basic);
							else
								simws.SetDomainCredentials(domainID, null, CredentialType.None);
						}
						catch (Exception e)
						{
							Console.WriteLine("Error saving the password: {0}", e.Message);
						}
					}
					
					HandleDomainLoggedIn(domainID, status);
				}
			}
			catch (Exception e)
			{
				status = null;
			}

			return status;
		}
		
		/// <summary>
		/// Call this to prevent the auto login feature from being called again
		/// </summary>
		public void DisableDomainAutoLogin(string domainID)
		{
			try
			{
				simws.DisableDomainAutoLogin(domainID);
			}
			catch {}
		}
		
		public string GetDomainPassword(string domainID)
		{
			lock (typeof(DomainController))
			{
				DomainInformation dom = (DomainInformation)keyedDomains[domainID];
				if (dom != null)
				{
					string userID;
					string credentials;
					try
					{
						CredentialType credType = simws.GetDomainCredentials(
							dom.ID, out userID, out credentials);
						if (credentials != null && credType == CredentialType.Basic)
						{
							return credentials;
						}
					}
					catch {}
				}
				
				return null;
			}
		}
		
		public void ActivateDomain(string domainID)
		{
			lock (typeof(DomainController))
			{
				try
				{
					DomainInformation dom = (DomainInformation)keyedDomains[domainID];
					if (dom == null)
					{
						// FIXME: Replace this with a real class named InvalidDomainException
						throw new Exception("Invalid Domain ID");
					}
					
					simws.SetDomainActive(domainID);
					dom.Active = true;
					
					if (DomainActivated != null)
						DomainActivated(this, new DomainEventArgs(domainID));
				}
				catch (Exception e)
				{
					throw e;
				}
			}
		}
		
		public void InactivateDomain(string domainID)
		{
			lock (typeof(DomainController))
			{
				try
				{
					DomainInformation dom = (DomainInformation)keyedDomains[domainID];
					if (dom == null)
					{
						// FIXME: Replace this with a real class named InvalidDomainException
						throw new Exception("Invalid Domain ID");
					}
					
					simws.SetDomainInactive(domainID);
					dom.Active = false;
	
					if (DomainInactivated != null)
						DomainInactivated(this, new DomainEventArgs(domainID));
				}
				catch (Exception e)
				{
					throw e;
				}
			}
		}
		
		public void ClearDomainPassword(string domainID)
		{
			try
			{
				simws.SetDomainCredentials(domainID, null, CredentialType.None);
			}
			catch {}
		}
		
		public void SetDomainPassword(string domainID, string password)
		{
			try
			{
				simws.SetDomainCredentials(domainID, password, CredentialType.Basic);
			}
			catch {}
		}

		public DomainInformation GetPOBoxDomain(string poBoxID)
		{
			lock(typeof(DomainController))
			{
				DomainInformation[] domains = this.GetDomains();
				foreach (DomainInformation domain in domains)
				{
					if(domain.POBoxID.Equals(poBoxID))
						return domain;
				}

				return null;
			}
		}

		/// <summary>
		/// Adds the domain to the keyedDomains hashtable
		/// </summary>
		private void AddDomainToHashtable(DomainInformation newDomain)
		{
			lock (typeof(DomainController) )
			{
				if(newDomain != null)
				{
					keyedDomains[newDomain.ID] = newDomain;
				}
			}
		}

		private void RemoveDomainFromHashtable(string domainID)
		{
			lock (typeof(DomainController) )
			{
				if(keyedDomains.ContainsKey(domainID))
				{
					DomainInformation dom = (DomainInformation)keyedDomains[domainID];
					keyedDomains.Remove(domainID);

					// If the domain we just removed was the default, ask
					// simias for the new default domain (if any domains still
					// exist).
					if (dom.IsDefault)
					{
						try
						{
							string newDefaultDomainID = simws.GetDefaultDomainID();
							if (newDefaultDomainID != null)
							{
								// Update the default domain
								if (keyedDomains.ContainsKey(newDefaultDomainID))
								{
									DomainInformation newDefaultDomain =
										(DomainInformation)keyedDomains[newDefaultDomainID];
									newDefaultDomain.IsDefault = true;
									string oldDomainID = null;
									if (defDomainID != null)
									{
										DomainInformation oldDefaultDomain = (DomainInformation)keyedDomains[defDomainID];
										if (oldDefaultDomain != null)
										{
											oldDefaultDomain.IsDefault = false;
											oldDomainID = defDomainID;
										}
									}
	
									defDomainID = newDefaultDomain.ID;									

									if (NewDefaultDomain != null)
										NewDefaultDomain(this, new NewDefaultDomainEventArgs(oldDomainID, newDefaultDomainID));
								}
							}
							else
								defDomainID = null;
						}
						catch {}
					}
				}
			}
		}

		///
		/// Simias Event Handlers
		///
		private void OnDomainUpEvent(object o, DomainEventArgs args)
		{
			// Nofity DomainUpEventHandlers
			if (DomainUp != null)
				DomainUp(this, args);
				
			Status authenticationStatus = AuthenticateDomain(args.DomainID);
			
			if (authenticationStatus == null ||
				((authenticationStatus.statusCode != StatusCodes.Success) &&
				(authenticationStatus.statusCode != StatusCodes.SuccessInGrace)))
			{
				// The authentication failed for whatever reason so retry by
				// setting an Http Proxy first.
				authenticationStatus = AuthenticateDomainWithProxy(args.DomainID);
			}

			if (authenticationStatus != null &&
				((authenticationStatus.statusCode == StatusCodes.Success) ||
				(authenticationStatus.statusCode == StatusCodes.SuccessInGrace)))
			{
				HandleDomainLoggedIn(args.DomainID, authenticationStatus);
			}
			else
			{
				// Notify DomainNeedsCredentialsEventHandlers
				if (DomainNeedsCredentials != null)
					DomainNeedsCredentials(this, args);
			}
		}
		
		private void HandleDomainLoggedIn(string domainID, Status status)
		{
			// Update our cache of the DomainInformation object
			try
			{
				DomainInformation dom =
					simws.GetDomainInformation(domainID);
				if (dom != null)
				{
					dom.Authenticated = true;
				
					if (keyedDomains.Contains(dom.ID))
						keyedDomains[dom.ID] = dom;
					else
					{
						// For whatever reason, we don't already have
						// record of this domain, so add it now.
						AddDomainToHashtable(dom);

						// Notify DomainAddedEventHandlers
						if (DomainAdded != null)
							DomainAdded(this, new DomainEventArgs(domainID));
					}
				}
			}
			catch{}
				
			// Notify DomainLoggedInEventHandlers
			if (DomainLoggedIn != null)
				DomainLoggedIn(this, new DomainEventArgs(domainID));

			if (status.statusCode == StatusCodes.SuccessInGrace)
			{
				if (status.RemainingGraceLogins < status.TotalGraceLogins)
				{
					// Notify DomainInGraceLoginPeriod
					if (DomainInGraceLoginPeriod != null)
					{
						DomainInGraceLoginPeriodEventArgs graceEventArgs =
							new DomainInGraceLoginPeriodEventArgs(
								domainID,
								status.RemainingGraceLogins);
						DomainInGraceLoginPeriod(this, graceEventArgs);
					}
				}
			}
		}
		
		private Status AuthenticateDomain(string domainID)
		{
			// Attempt to authenticate.  If the authentication is successful,
			// the credentials were previously saved.
			DomainAuthentication domainAuth =
				new DomainAuthentication(
					"iFolder",
					domainID,
					null);

			try
			{
				return domainAuth.Authenticate(simiasManager.WebServiceUri, simiasManager.DataPath);
			}
			catch {}
			
			return null;
		}
		
		private Status AuthenticateDomainWithProxy(string domainID)
		{
			string userID;
			string credentials;

			DomainInformation dom = (DomainInformation)keyedDomains[domainID];
			if (dom == null)
				return null;

			try
			{
				SetHttpProxyForHost(dom.Host);
						
				CredentialType credentialType =
					simws.GetDomainCredentials(
						domainID,
						out userID,
						out credentials);
	
				if ((credentialType == CredentialType.Basic) &&
					(credentials != null))
				{
					// There are credentials that are saved on the domain.
					// Use these to attempt to authenticate.  If the
					// authentication fails, post a DomainNeedsCredentials
					// event.
					DomainAuthentication domainAuth =
						new DomainAuthentication(
							"iFolder",
							domainID,
							credentials);
							
					Status status = domainAuth.Authenticate(simiasManager.WebServiceUri, simiasManager.DataPath);
	
					if (status.statusCode == StatusCodes.InvalidCredentials)
					{
						// Remove the bad credentials.
						simws.SetDomainCredentials(domainID, null, CredentialType.None);
					}
					
					return status;
				}
			}
			catch {}
			
			return null;
		}

		private void SetHttpProxyForHost(string host)
		{
			// Check if a proxy needs to be set
			GnomeHttpProxy proxy = new GnomeHttpProxy(host);
			string user = null;
			string password = null;
			if (proxy.IsProxySet)
			{
				if (proxy.CredentialsSet)
				{
					user = proxy.Username;
					password = proxy.Password;
				}
						
				// Set unsecure address
				simws.SetProxyAddress(
					"http://" + host,
					"http://" + proxy.Host,
					user,
					password);
						
				if (!proxy.IsSecureProxySet)
				{
					// Set secure proxy
					simws.SetProxyAddress(
						"https://" + host,
						"http://" + proxy.Host,
						user,
						password);
				}
			}
			
			// Secure proxy
			if (proxy.IsSecureProxySet)
			{
				simws.SetProxyAddress(
					"https://" + host,
					"http://" + proxy.SecureHost,
					user,
					password);
			}
		}

		private void OnDomainAddedEvent(object o, DomainEventArgs args)
		{
			DomainInformation domain = (DomainInformation)keyedDomains[args.DomainID];
			if (domain != null)
			{
				// We (and others) already know about this
				// domain so do nothing about this event.
				return;
			}

			try
			{
				domain = simws.GetDomainInformation(args.DomainID);
			}
			catch (Exception e)
			{
				// FIXME: Add in some type of error logging to show that we
				// weren't able to get information about a newly added domain
				return;
			}

			AddDomainToHashtable(domain);

			// Notify DomainAddedEventHandlers
			if (DomainAdded != null)
				DomainAdded(this, args);
		}

		private void OnDomainDeletedEvent(object o, DomainEventArgs args)
		{
			DomainInformation domain = (DomainInformation)keyedDomains[args.DomainID];
			if (domain == null)
			{
				// We don't know about this domain so don't do anything.
				return;
			}
			
			RemoveDomainFromHashtable(args.DomainID);
			
			// Notify DomainDeletedEventHandlers
			if (DomainDeleted != null)
				DomainDeleted(this, args);
		}
	}

	public class DomainAccountAlreadyExistsException : Exception
	{
		/// <summary>
		/// Constructs a DomainAccountAlreadyExistsException.
		/// </summary>
		public DomainAccountAlreadyExistsException() : base()
		{
		}
		
		/// <summary>
		/// Constructs a DomainAccountAlreadyExistsException.
		/// </summary>
		/// <param name="message">The message describing the exception.</param>
		public DomainAccountAlreadyExistsException(string message) : base(message)
		{
		}
	}
}
