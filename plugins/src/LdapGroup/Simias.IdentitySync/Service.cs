/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Mahabaleshwar Asundi <amahabaleshwar@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

using Simias;
using Simias.Event;
using Simias.POBox;
using Simias.Service;
using Simias.Storage;


namespace Simias.Identity
{
	/// <summary>
	/// Class the handles presence as a service
	/// </summary>
	public class IdentityManagement : IThreadService
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Simias.Identity.Authentication authProvider = null;
		private Simias.Identity.IUserProvider userProvider = null;
		private Simias.IIdentitySyncProvider syncProvider = null;
		
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public IdentityManagement()
		{
		}
		#endregion
		
		#region Private Members
        /// <summary>
        /// Reads Simnias.config file to get which directory server is being used and loads that
        /// </summary>
        /// <returns>true if Loading of IdentitySuncProvider succeeds</returns>
		private bool LoadIdentityProvider()
		{
			bool status = false;
			
			// Bootstrap the identity provider from the Simias.config file
			Simias.Configuration config = Store.Config;
			string assemblyName = config.Get( "Identity", "ServiceAssembly" );
			string userClass = config.Get( "Identity", "Class" );
			
			if ( assemblyName != null && userClass != null )
			{
				log.Debug( "Identity assembly: {0}  class: {1}", assemblyName, userClass );
				Assembly idAssembly = Assembly.LoadWithPartialName( assemblyName );
				if ( idAssembly != null )
				{
					Type type = idAssembly.GetType( userClass );
					if ( type != null )
					{
						userProvider = Activator.CreateInstance( type ) as IUserProvider;
						if ( userProvider != null )
						{
							log.Debug( "created user provider instance" );
							User.RegisterProvider( userProvider );
							status = true;
							
							// does this provider support external syncing?
							foreach( Type ctype in idAssembly.GetTypes() )
							{
								foreach( Type itype in ctype.GetInterfaces() )
								{
									if ( Simias.IdentitySynchronization.Service.master && itype == typeof( Simias.IIdentitySyncProvider ) )
									{
										syncProvider = 
											Activator.CreateInstance( ctype ) as IIdentitySyncProvider;
										if ( syncProvider != null )
										{
											Simias.IdentitySynchronization.Service.Register( syncProvider );
											log.Debug( "created sync provider instance" );
										}
										else
										{
											log.Debug( "failed to create an instance of IIdentitySyncProvider" );
										}
										break;
									}	
								}
								
								if ( syncProvider != null )
								{
									break;
								}
							}
						}
						else
							log.Debug( "userProvider is null" );
					}							
					else
						log.Debug( "Assembly type is null" );
				}
				else
					log.Debug( "Unable to load  Assembly" );
			}
			
			// If we couldn't load the configured provider
			// load the internal user/identity provider
			if ( status == false )
			{
				if ( userProvider == null )
				{
					log.Info( "Could not load the configured user provider - loading InternalUser" );
					userProvider = new Simias.Identity.InternalUser();
					User.RegisterProvider( userProvider );
					status = true;
				}	
			}
			
			return status;
		}
		#endregion

		#region IThreadService Members
		/// <summary>
		/// Starts the thread service.
		/// </summary>
		public void Start()
		{
			log.Debug( "IdentityManagement service Start called" );

			// Instantiate the server domain
			// The domain will be created the first time the
			// server is run
               		EnterpriseDomain enterpriseDomain = new EnterpriseDomain( true );
           		if ( enterpriseDomain != null )
             		{

				// Valid enterprise domain - start the external
				// identity sync service
				Simias.IdentitySynchronization.Service.Start();
				
				if ( userProvider == null )
				{
					LoadIdentityProvider();
				}	
				
				// Register with the domain provider service.
				if ( authProvider == null )
				{
					authProvider = new Simias.Identity.Authentication();
					DomainProvider.RegisterProvider( this.authProvider );
				}
			}

		}

		/// <summary>
		/// Resumes a paused service. 
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Custom.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public int Custom(int message, string data)
		{
			return 0;
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			log.Debug( "Stop called" );

			Simias.IdentitySynchronization.Service.Stop();
			
			if ( syncProvider != null )
			{
				IdentitySynchronization.Service.Unregister( syncProvider );
				syncProvider = null;
			}
			
			if ( authProvider != null )
			{
				DomainProvider.Unregister( authProvider );
				authProvider = null;
			}
			
			if ( userProvider != null )
			{
				User.UnregisterProvider( userProvider );
				userProvider = null;
			}
		}
		#endregion
	}
}
