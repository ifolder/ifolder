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
 *  Author: banderso@novell.com
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting;
using System.Threading;
using System.Web;

using Simias;
using Simias.Client;
using Simias.Domain;

//using Simias.Client.Event;
using Simias.Event;
using Simias.Storage;
using Simias.Sync;

using Novell.Security.ClientPasswordManager;

namespace Simias.DomainWatcher
{
	/// <summary>
	/// DomainWatcher Manager
	/// </summary>
	public class Manager : IDisposable
	{
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(typeof(Simias.DomainWatcher.Manager));

		private bool			started = false;
		private	bool			stop = false;
		private Configuration	config;
		private Thread			watcherThread = null;
		private AutoResetEvent	stopEvent;
		private Store			store;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="config">Simias configuration</param>
		public Manager(Configuration config)
		{
			this.config = config;
			
			// store
			store = Store.GetStore();
		}

		/// <summary>
		/// Start the Watcher.
		/// </summary>
		public void Start()
		{
			log.Debug("Start called");

			try
			{
				lock(this)
				{
					if (started == false)
					{
						this.watcherThread = new Thread(new ThreadStart(this.WatcherThread));
						this.watcherThread.IsBackground = true;
						this.watcherThread.Priority = ThreadPriority.BelowNormal;
						this.stopEvent = new AutoResetEvent(false);

						this.watcherThread.Start();
					}
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to start Domain Watcher thread.");
				throw e;
			}

			log.Debug("Start exit");
		}

		/// <summary>
		/// Stop the Domain Watcher Thread.
		/// </summary>
		public void Stop()
		{
			log.Debug("Stop called");
			try
			{
				lock(this)
				{
					// Set state and then signal the event
					this.stop = true;
					this.stopEvent.Set();
					Thread.Sleep(32);
					this.stopEvent.Close();
					Thread.Sleep(0);
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to stop Domain Watcher.");
				throw e;
			}
			log.Debug("Stop exit");
		}

		/// <summary>
		/// Domain Watcher Thread.
		/// </summary>
		public void WatcherThread()
		{
			log.Debug("WatcherThread started");
			bool firstTime = true;
			int status;
			this.started = true;
			EventPublisher cEvent = new EventPublisher();

			string userID;
			string credentials;

			do 
			{
				//
				// Cycle through the domains
				//

				try
				{
					LocalDatabase ldb = store.GetDatabaseObject();
					ICSList domainList = ldb.GetNodesByType( "Domain" );
					foreach( ShallowNode shallowNode in domainList )
					{
						Simias.Storage.Domain cDomain = store.GetDomain( shallowNode.ID );
					
						// Make sure this domain is a slave since we don't watch
						// mastered domains.  We can tell by looking at the Roster

						Roster cRoster = cDomain.Roster;
						if ( cRoster.Role == SyncRoles.Slave )
						{
							Member cMember;
							log.Debug("checking Domain: " + cDomain.Name);

							if ( new DomainAgent().IsDomainActive( cDomain.ID ) == false )
							{
								log.Debug( "Domain: " + cDomain.Name + " is off-line" );
								continue;
							}

							CredentialType credType = 
								store.GetDomainCredentials(cDomain.ID, out userID, out credentials);

							// Only basic type authentication is supported right now.
							if ( credType != CredentialType.Basic )
							{
								cMember = cRoster.GetCurrentMember();

								// TODO: DomainCredentials need to support an empty string.
								// Change "Not Needed" to empty when fixed.
								if ( ( credType == CredentialType.None ) || ( credType == CredentialType.NotRequired ) )
								{
									credentials = "Not Needed";
								}
							}
							else
							{
								cMember = cRoster.GetMemberByID(userID);
							}

							// Can we talk to the domain?
							// Check to see if a full set of credentials exist
							// for this domain

							NetCredential cCreds = 
								new NetCredential(
								"iFolder", 
								cDomain.ID, 
								true, 
								cMember.Name, 
								credentials);

							Uri cUri = new Uri(cDomain.HostAddress.ToString());
							NetworkCredential netCreds = cCreds.GetCredential(cUri, "BASIC");
							if ((netCreds == null) || firstTime)
							{
								// Create the domain service web client object.
								DomainService domainSvc = new DomainService();
								domainSvc.Url = 
									cDomain.HostAddress.ToString() + "/DomainService.asmx";
								domainSvc.Credentials = netCreds;
								domainSvc.Timeout = 30000;
							
								try
								{
									log.Debug("Calling remote domain at: " + domainSvc.Url);
									domainSvc.GetDomainInfo(userID);
									// CRG: Change this to a one so
									// we don't cause an event to happen
									status = 1;
								}
								catch(WebException webEx)
								{
									status = -1;
									log.Error("failed getting Domain Information  status: " + webEx.Status.ToString());
									if (webEx.Status == System.Net.WebExceptionStatus.ProtocolError ||
										webEx.Status == System.Net.WebExceptionStatus.TrustFailure )
									{
										credentials = null;
										status = 0;
									}
								}
								catch(Exception ex)
								{
									status = -1;
									log.Error("failed getting Domain Information - normal exception status: " + ex.Message);
								}

								domainSvc = null;

								if (status == 0)
								{
									Simias.Client.Event.NotifyEventArgs cArg =
										new Simias.Client.Event.NotifyEventArgs(
										"Domain-Up", 
										cDomain.ID, 
										System.DateTime.Now);

									cEvent.RaiseEvent(cArg);
								}
							}
						}
					}

					firstTime = false;
				}
				catch(Exception e)
				{
					log.Error(e.Message);
					log.Error(e.StackTrace);
				}

				stopEvent.WaitOne((30 * 1000), false);

			} while(this.stop == false);

			this.started = false;
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Stop();
		}

		#endregion
	}
}
