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
			int status;
			this.started = true;
			do 
			{
				//
				// Cycle through the domains
				// uh, right now there can only be one Enterprise Domain
				//

				try
				{
					Simias.Storage.Domain cDomain = store.GetDomain(store.DefaultDomain);
					if (cDomain != null &&
						cDomain.ID != Simias.Storage.Domain.WorkGroupDomainID) 
					{
						log.Debug("checking Domain: " + cDomain.Name);
						Roster cRoster = cDomain.GetRoster(store);
						Member cMember = cRoster.GetCurrentMember();

						// Can we talk to the domain?
						// Check to see if a full set of credentials exist
						// for this domain

						NetCredential cCreds = 
							new NetCredential(
								"iFolder", 
								cDomain.ID, 
								true, 
								cMember.Name, 
								null);

						Uri cUri = new Uri(cDomain.HostAddress.ToString());
						if (cCreds.GetCredential(cUri, "BASIC") == null)
						{
							// Create the domain service web client object.
							DomainService domainSvc = new DomainService();
							domainSvc.Url = 
								cDomain.HostAddress.ToString() + "/DomainService.asmx";

                            domainSvc.Timeout = 30000;
							
							try
							{
                                log.Debug("Calling remote domain at: " + domainSvc.Url);
								domainSvc.GetDomainInfo(cMember.ID);
								status = 0;
							}
							catch(WebException webEx)
							{
								status = -1;
                                log.Error("failed getting Domain Information  status: " + webEx.Status.ToString());
								if (webEx.Status == System.Net.WebExceptionStatus.ProtocolError ||
									webEx.Status == System.Net.WebExceptionStatus.TrustFailure )
								{
									status = 0;
								}
							}
                            catch(Exception ex)
                            {
                                status = -1;
                                log.Error("failed getting Domain Information - normal exception status: " + ex.Message);
                            }

							if (status == 0)
							{
								EventPublisher cEvent = new EventPublisher();
								Simias.Client.Event.NotifyEventArgs cArg =
									new Simias.Client.Event.NotifyEventArgs(
									"Domain-Up", 
									cDomain.ID, 
									System.DateTime.Now);

								cEvent.RaiseEvent(cArg);
							}

							domainSvc = null;
						}
					}
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
