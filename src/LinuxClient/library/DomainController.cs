/***********************************************************************
 *  $RCSfile: DomainController.cs,v $
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
		private iFolderWebService	ifws;


		/// <summary>
		/// Member that provides acces to the Simias Web Service
		/// </summary>
		private SimiasWebService	simws;

		/// <summary>
		/// Hashtable to hold the domains
		/// </summary>
		private Hashtable			keyedDomains;

		/// <summary>
		/// Member to keep track of the default domain
		/// </summary>
		private string				defDomainID;
		
		private SimiasEventBroker eventBroker = null;
		private Manager simiasManager;

		public static Status upgradeStatus;
		
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
		public event DomainClientUpgradeAvailableEventHandler DomainClientUpgradeAvailable;
		
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
				throw new Exception("Unable to create ifolder web service in DomainController");
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
				throw new Exception("Unable to create simias web service in DomainController");
			}

			keyedDomains = new Hashtable();
			defDomainID = "0";
			
			Refresh();

			// Register with the SimiasEventBroker to get Simias Events
			eventBroker = SimiasEventBroker.GetSimiasEventBroker();
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
				if (defDomainID != null && keyedDomains.Contains(defDomainID))
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
		/// Returns the Recovery Agent List for the specified Domain
		/// </summary>
	        public string[] GetRAList (string domainID)
		{
		    DomainInformation di = GetDomain (domainID);
		    SimiasWebService sws = new SimiasWebService();
		    sws.Url = di.HostUrl + "/Simias.asmx";

//		    string[] ragents = sws.GetRAList();
		    //TODO :
		    string[] ragents = new string [1];
		    ragents[0] = "   ";

		    return ragents;
		}


		/// <summary>
		/// Returns the Recovery Agent List for the specified Domain
		/// </summary>
	        public Status IsPassPhraseSet (string domainID)
		{
		    return simws.IsPassPhraseSet (domainID);
		}

		/// <summary>
		/// Returns the Recovery Agent List for the specified Domain
		/// </summary>
	        public Status SetPassPhrase (string domainID, string passPhrase, string recoveryAgent)
		{
		    return simws.SetPassPhrase (domainID,"Arul", passPhrase, "public key", recoveryAgent);
		}

		/// <summary>
		/// Returns the Recovery Agent List for the specified Domain
		/// </summary>
	        public void StorePassPhrase (string domainID, string passPhrase, CredentialType type, bool persist)
		{
		    simws.StorePassPhrase (domainID, passPhrase, type, persist);
		}

		/// <summary>
		/// Returns the Recovery Agent List for the specified Domain
		/// </summary>
	        public Status ValidatePassPhrase (string domainID, string passPhrase)
		{
		    return simws.ValidatePassPhrase (domainID, passPhrase);
		}

		/// <summary>
		/// Returns the Certificate for the Recover Agent for the specified Domain
		/// </summary>
	        public byte[] GetRACertificate (string domainID, string recoveryAgent)
		{
		    DomainInformation di = GetDomain (domainID);
		    SimiasWebService sws = new SimiasWebService();
		    sws.Url = di.HostUrl + "/Simias.asmx";
		    
		    return sws.GetRACertificate(recoveryAgent);		    
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
					{
						DomainAddedIdleHandler addedHandler =
							new DomainAddedIdleHandler(dom.ID, this);
						GLib.Idle.Add(addedHandler.IdleHandler);
					}
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
		
		private void EmitDomainAdded(string domainID)
		{
			if (DomainAdded != null)
				DomainAdded(this, new DomainEventArgs(domainID));
		}
		
		public class DomainAddedIdleHandler
		{
			string domainID;
			DomainController domainController;
			
			public DomainAddedIdleHandler(string domainID,
										   DomainController domainController)
			{
				this.domainID = domainID;
				this.domainController = domainController;
			}
			
			public bool IdleHandler()
			{
				domainController.EmitDomainAdded(domainID);
				
				return false; // Don't keep calling this function
			}
		}
		
		public DomainInformation UpdateDomainHostAddress(string domainID, string host, string user, string password)
		{
			lock (typeof(DomainController))
			{
				DomainInformation dom = (DomainInformation)keyedDomains[domainID];
				DomainInformation updatedDomain = null;
				if (dom != null)
				{
					if (String.Compare(dom.Host, host, true) != 0)
					{
						try
						{
							if (simws.SetDomainHostAddress(domainID, host, user, password))
							{
								updatedDomain = simws.GetDomainInformation(domainID);
	
								updatedDomain.StatusCode = StatusCodes.Success;

								keyedDomains[domainID] = updatedDomain;
	
								// Notify DomainHostModifiedEventHandlers
								if (DomainHostModified != null)
									DomainHostModified(this, new DomainEventArgs(domainID));
							}
							else
							{
								return null;
							}
						}
						catch (Exception e)
						{
							// FIXME: Determine if any exceptions can be thrown by this
							throw e;
						}
					}
				}
				else
				{
					// FIXME: Throw DomainDoesNotExistException
				}
				
				return updatedDomain;
			}
		}

		/// <summary>
		/// Call this to remove/detach from a Simias Domain
		/// </summary>
		public void RemoveDomain(string domainID, bool deleteiFoldersOnServer)
		{
			try
			{
				simws.LeaveDomain(domainID, !deleteiFoldersOnServer);
			}
			catch(Exception e)
			{
				iFolderMsgDialog dg = new iFolderMsgDialog(
					null,
					iFolderMsgDialog.DialogType.Error,
					iFolderMsgDialog.ButtonSet.Ok,
					"",
					Util.GS("Unable to remove the account"),
					Util.GS("iFolder encountered a problem removing the account.  Please restart iFolder and try again."),
					e.Message);
				dg.Run();
				dg.Hide();
				dg.Destroy();
			}
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
						}
					}
					
					status = HandleDomainLoggedIn(domainID, status);
				}
			}
			catch (Exception e)
			{
				status = null;
			}
			return status;
		}
		
		/// <summary>
		/// Call this to log out of the specified domain
		/// </summary>
		public void LogoutDomain(string domainID)
		{
			DomainAuthentication domainAuth =
				new DomainAuthentication(
					"iFolder",
					domainID,
					null);
			domainAuth.Logout(simiasManager.WebServiceUri, simiasManager.DataPath);

			// Update our cache of the DomainInformation object
			try
			{
				DomainInformation dom =
					simws.GetDomainInformation(domainID);
				if (dom != null)
				{
					dom.Authenticated = false;
				
					if (keyedDomains.Contains(dom.ID))
						keyedDomains[dom.ID] = dom;
					else
					{
						// For whatever reason, we don't already have
						// record of this domain, so add it now.
						AddDomainToHashtable(dom);

						// Notify DomainAddedEventHandlers
						if (DomainAdded != null)
						{
							DomainAddedIdleHandler addedHandler =
								new DomainAddedIdleHandler(domainID, this);
							GLib.Idle.Add(addedHandler.IdleHandler);
						}
					}
				}
			}
			catch{}
				
			// Notify DomainLoggedInEventHandlers
			if (DomainLoggedOut != null)
				DomainLoggedOut(this, new DomainEventArgs(domainID));
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
		
		public void CheckForNewiFolders()
		{
			lock(typeof(DomainController))
			{
				// Update the POBox for every domain so that the user can get
				// notification of new iFolder subscriptions.
				foreach(DomainInformation domain in keyedDomains.Values)
				{
					try
					{
						ifws.SynciFolderNow(domain.POBoxID);
					}
					catch
					{
					}
				}
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
					else
					{
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

			DomainLoginThread domainLoginThread =
				new DomainLoginThread(this);
			
			domainLoginThread.Completed +=
				new DomainLoginCompletedHandler(OnDomainLoginCompleted);
			
			domainLoginThread.Login(args.DomainID);
		}
		
		private void OnDomainLoginCompleted(object o, DomainLoginCompletedArgs args)
		{
			string domainID = args.DomainID;
			Status authStatus = args.AuthenticationStatus;

			if (authStatus != null &&
				((authStatus.statusCode == StatusCodes.Success) ||
				 (authStatus.statusCode == StatusCodes.SuccessInGrace)))
			{
				authStatus = HandleDomainLoggedIn(domainID, authStatus);
			}
			else
			{
				// Notify DomainNeedsCredentialsEventHandlers
				if (DomainNeedsCredentials != null)
					DomainNeedsCredentials(this, new DomainEventArgs(domainID));
			}
		}
		
		private Status HandleDomainLoggedIn(string domainID, Status status)
		{
		//	Status tempStatus = status;
			DomainController.upgradeStatus = status;
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
						{
							DomainAddedIdleHandler addedHandler =
								new DomainAddedIdleHandler(domainID, this);
							GLib.Idle.Add(addedHandler.IdleHandler);
						}
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
			
			// Check to see if there's a new version of the client available
			string newClientVersion = GetNewClientVersion(domainID);
			if (newClientVersion != null)
			{
				// New client is needed to connect to the server
				// Notify DomainClientUpgradeAvailable listeners
				if (DomainClientUpgradeAvailable != null)
				{
					DomainController domainController = DomainController.GetDomainController();
					DomainController.upgradeStatus.statusCode = StatusCodes.UpgradeNeeded;
					DomainClientUpgradeAvailableEventArgs args =
						new DomainClientUpgradeAvailableEventArgs(
							domainID, newClientVersion);
					DomainClientUpgradeAvailable(this, args);
				}
			}
			else
			{
				bool serverIsOld = IsServerOld(domainID);
				
				if(serverIsOld)
				{
					DomainController.upgradeStatus.statusCode = StatusCodes.ServerOld;
					DomainController domainController = DomainController.GetDomainController();
					DomainClientUpgradeAvailableEventArgs args =
						new DomainClientUpgradeAvailableEventArgs(
							domainID, newClientVersion);
					DomainClientUpgradeAvailable(this, args);
				}
				else
				{
					string upgradeAvailable = GetNewClientAvailable(domainID); 
					if( upgradeAvailable == null)
					{
						return DomainController.upgradeStatus;
					}
					Console.WriteLine("Upgrade available is: {0}", upgradeAvailable);
					DomainClientUpgradeAvailableEventArgs args =
						new DomainClientUpgradeAvailableEventArgs(
							domainID, upgradeAvailable);
					DomainClientUpgradeAvailable(this, args);				
				}
				
			}
			return DomainController.upgradeStatus;
		}

		private bool IsServerOld(string domainID)
		{
			return ifws.CheckForServerUpdate(domainID);
		}

		private string GetNewClientAvailable(string domainID)
		{
			string AvailableClientVersion = null;
			try
			{
				AvailableClientVersion = ifws.CheckForUpdatedClientAvailable(domainID);
			}
			catch
			{
			}
			return AvailableClientVersion;
		}

		/// <returns>Returns the version of a newer client or null if no new version exists.</returns>		
		private string GetNewClientVersion(string domainID)
		{
			string newClientVersion = null;

			try
			{
				newClientVersion = ifws.CheckForUpdatedClient(domainID);
			}
			catch(Exception e)
			{
				string domainName = domainID;
				DomainInformation dom = (DomainInformation) keyedDomains[domainID];
				if (dom != null)
					domainName = dom.Name;
				Console.WriteLine("Error checking for new version of iFolder Client on {0}: {1}", domainName, e.Message);
			}

			return newClientVersion;
		}
		
		public Status AuthenticateDomain(string domainID)
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
		
		public Status AuthenticateDomainWithProxy(string domainID)
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

		[GLib.ConnectBefore]
		private void OnDomainAddedEvent(object o, DomainEventArgs args)
		{
			lock (typeof(DomainController) )
			{
				if (args == null || args.DomainID == null)
					return;	// Prevent null object reference exception
					
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
				{
					DomainAddedIdleHandler addedHandler =
						new DomainAddedIdleHandler(args.DomainID, this);
					GLib.Idle.Add(addedHandler.IdleHandler);
				}
			}
		}

		[GLib.ConnectBefore]
		private void OnDomainDeletedEvent(object o, DomainEventArgs args)
		{
			lock (typeof(DomainController) )
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
	
	public class AddDomainThread
	{
		private DomainController	domainController;
		private string				serverName;
		private string				userName;
		private string				password;
		private bool				bRememberPassword;
		private bool				bSetAsDefault;
		
		private DomainInformation	domain;
		private Exception			e;
		
		public string ServerName
		{
			get
			{
				return serverName;
			}
		}
		
		public string Password
		{
			get
			{
				return password;
			}
		}
		
		public bool RememberPassword
		{
			get
			{
				return bRememberPassword;
			}
		}
		
		public DomainInformation Domain
		{
			get
			{
				return domain;
			}
		}
		
		public Exception Exception
		{
			get
			{
				return e;
			}
		}
		
		public event EventHandler Completed;
		
		public AddDomainThread(
					DomainController domainController,
					string serverName,
					string userName,
					string password,
					bool bRememberPassword,
					bool bSetAsDefault)
		{
			this.domainController = domainController;
			this.serverName = serverName;
			this.userName = userName;
			this.password = password;
			this.bRememberPassword = bRememberPassword;
			this.bSetAsDefault = bSetAsDefault;
			
			this.domain = null;
			this.e = null;
		}
		
		public void AddDomain()
		{
			System.Threading.Thread thread =
				new System.Threading.Thread(
					new System.Threading.ThreadStart(AddThread));
			thread.Start();
		}
		
		private void AddThread()
		{
			try
			{
				domain = domainController.AddDomain(
					serverName,
					userName,
					password,
					bRememberPassword,
					bSetAsDefault);
			}
			catch (Exception e)
			{
				this.e = e;
			}

			if (Completed != null)
			{
				AddDomainCompletedHandler completedHandler =
					new AddDomainCompletedHandler(this);
				GLib.Idle.Add(completedHandler.IdleHandler);
			}
		}
		
		private void AddCompleted()
		{
			if (Completed != null)
				Completed(this, EventArgs.Empty);
		}
		
		private class AddDomainCompletedHandler
		{
			public AddDomainThread thread;
			
			public AddDomainCompletedHandler(AddDomainThread thread)
			{
				this.thread = thread;
			}
			
			public bool IdleHandler()
			{
				thread.AddCompleted();
				
				return false;
			}
		}
	}
	
	public delegate void DomainLoginCompletedHandler(object sender, DomainLoginCompletedArgs args);
	
	public class DomainLoginThread
	{
		private DomainController	domainController;
		private string				domainID;
		private string				password;
		private bool				bSavePassword;
		private Status				authStatus;
		
		public Status AuthenticationStatus
		{
			get
			{
				return authStatus;
			}
		}
		
		public string DomainID
		{
			get
			{
				return domainID;
			}
		}
		
		public event DomainLoginCompletedHandler Completed;
//		public event EventHandler Completed;

		public DomainLoginThread(DomainController domainController)
		{
			this.domainController	= domainController;
			this.domainID			= null;
			this.password			= null;
			this.bSavePassword		= false;
			
			this.authStatus			= null;
		}
		
		/// <summary>
		/// This method should only be used during the DomainUp process when no
		/// user intervention is needed.  If the authentication fails, this
		/// will eventually cause the user to be prompted for more information
		/// but it is possible that if everything works, the user will never be
		/// interrupted.
		/// </summary>
		public void Login(string domainID)
		{
			this.domainID = domainID;
			
			System.Threading.Thread thread = 
				new System.Threading.Thread(
					new System.Threading.ThreadStart(LoginThread));
			thread.Start();
		}
		
		/// <summary>
		/// This method should only be called when the user actively kicks off
		/// a login from the accounts dialog or from the account wizard.
		/// </summary>
		public void Login(string domainID, string password, bool bSavePassword)
		{
			this.domainID			= domainID;
			this.password			= password;
			this.bSavePassword		= bSavePassword;
			
			System.Threading.Thread thread = 
				new System.Threading.Thread(
					new System.Threading.ThreadStart(LoginThreadWithArgs));
			thread.Start();
		}
		
		private void LoginThread()
		{
			try
			{
				authStatus = domainController.AuthenticateDomain(domainID);
				
				if (authStatus == null ||
					((authStatus.statusCode != StatusCodes.Success) &&
					 (authStatus.statusCode != StatusCodes.SuccessInGrace)))
				{
					authStatus =
						domainController.AuthenticateDomainWithProxy(
							domainID);
				}
			}
			catch(Exception e)
			{
				Console.WriteLine("Exception attempting a login: {0}", e.Message);
				authStatus = new Status();	// Default is UnknownStatus
			}

			if (Completed != null)
			{
				LoginThreadCompletedHandler completedHandler =
					new LoginThreadCompletedHandler(this);
				GLib.Idle.Add(completedHandler.IdleHandler);
			}
		}
		
		private void LoginThreadWithArgs()
		{
			try
			{
				authStatus = domainController.AuthenticateDomain(
					domainID, password, bSavePassword);
			}
			catch(Exception e)
			{
				Console.WriteLine("Exception attempting a login: {0}", e.Message);
				authStatus = new Status();	// Default is UnknownStatus
			}

			if (Completed != null)
			{
				LoginThreadCompletedHandler completedHandler =
					new LoginThreadCompletedHandler(this);
				GLib.Idle.Add(completedHandler.IdleHandler);
			}
		}
		
		private void LoginCompleted()
		{
			if (Completed != null)
				Completed(this, new DomainLoginCompletedArgs(domainID, authStatus));
		}
		
		private class LoginThreadCompletedHandler
		{
			public DomainLoginThread thread;
			
			public LoginThreadCompletedHandler(DomainLoginThread thread)
			{
				this.thread = thread;
			}
			
			public bool IdleHandler()
			{
				thread.LoginCompleted();
				
				return false;
			}
		}
	}
	
	public class DomainLoginCompletedArgs : EventArgs
	{
		private string domainID;
		private Status authStatus;
		
		public string DomainID
		{
			get{ return domainID; }
		}
		
		public Status AuthenticationStatus
		{
			get{ return authStatus; }
		}
		
		public DomainLoginCompletedArgs(string domainID, Status authStatus)
		{
			this.domainID = domainID;
			this.authStatus = authStatus;
		}
	}
}
