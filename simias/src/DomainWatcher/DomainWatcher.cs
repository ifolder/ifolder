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
using Simias.DomainServices;
using Simias.Location;

//using Simias.Client.Event;
using Simias.Event;
using Simias.Storage;
using Simias.Sync;

using Novell.Security.ClientPasswordManager;
using SCodes = Simias.Authentication.StatusCodes;

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
		private int				waitTime = ( 30 * 1000 );
		private int				initialWait = ( 5 * 1000 );

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
			EventPublisher cEvent = new EventPublisher();

			string userID;
			string credentials;

			// Let the caller know we're good to go
			this.started = true;

			do 
			{
				//
				// Cycle through the domains
				//

				try
				{
					ICSList domainList = store.GetDomainList();
					foreach( ShallowNode shallowNode in domainList )
					{
						Domain cDomain = store.GetDomain( shallowNode.ID );
					
						// Make sure this domain is a slave since we don't watch
						// mastered domains.
						if ( cDomain.Role == SyncRoles.Slave )
						{
							Member cMember;
							log.Debug( "checking Domain: " + cDomain.Name );

							DomainAgent domainAgent = new DomainAgent();
							if ( domainAgent.IsDomainActive( cDomain.ID ) == false )
							{
								log.Debug( "Domain: " + cDomain.Name + " is off-line" );
								continue;
							}

							if ( domainAgent.IsDomainAutoLoginEnabled( cDomain.ID ) == false )
							{
								log.Debug( "Domain: " + cDomain.Name + " auto-login is disabled" );
								continue;
							}

							CredentialType credType = 
								store.GetDomainCredentials( cDomain.ID, out userID, out credentials );

							// Only basic type authentication is supported right now.
							if ( credType != CredentialType.Basic )
							{
								cMember = cDomain.GetCurrentMember();
								credentials = null;
							}
							else
							{
								cMember = cDomain.GetMemberByID( userID );
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

							Uri cUri = Locate.ResolveLocation( cDomain.ID );
							NetworkCredential netCreds = cCreds.GetCredential( cUri, "BASIC" );
							if ( ( netCreds == null ) || firstTime )
							{
								bool raiseEvent = false;
								Simias.Authentication.Status authStatus;

								if ( netCreds == null )
								{
									authStatus = 
										domainAgent.Login( 
											cDomain.ID, 
											Guid.NewGuid().ToString(),
											"12" );

									if ( authStatus.statusCode == SCodes.UnknownUser )
									{
										raiseEvent = true;
									}
								}
								else
								{
									authStatus = 
										domainAgent.Login( 
											cDomain.ID, 
											netCreds.UserName,
											netCreds.Password );

									if ( authStatus.statusCode == SCodes.InvalidPassword )
									{
										raiseEvent = true;
									}
								}

								if ( raiseEvent == true )
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
				}
				catch(Exception e)
				{
					log.Error(e.Message);
					log.Error(e.StackTrace);
				}

				stopEvent.WaitOne( ( firstTime == true ) ? initialWait : waitTime, false );
				firstTime = false;

			} while( this.stop == false );

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
