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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;

using Simias;
using Simias.Event;
using Simias.Service;
using Simias.Storage;
using Mono.ASPNET;

namespace Simias.Web
{
	/// <summary>
	/// Class the handles presence as a service
	/// </summary>
	public class SimiasWebService : IThreadService
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
				SimiasLogManager.GetLogger( typeof( ChangeLog ) );

		/// <summary>
		/// Configuration object for the Collection Store.
		/// </summary>
		private Configuration config;
		private ApplicationServer server;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public SimiasWebService()
		{
			IPAddress ipaddr = null;
			ushort port;

			port = Convert.ToUInt16 (8080);

			ipaddr = IPAddress.Parse ("0.0.0.0");

			IWebSource webSource;

			webSource = new XSPWebSource (ipaddr, port);
			server = new ApplicationServer (webSource);

			server.Verbose = false;

/*			if (apps != null)
				server.AddApplicationsFromCommandLine (apps);

			if (appConfigFile != null)
				server.AddApplicationsFromConfigFile (appConfigFile);
*/
			server.AddApplicationsFromCommandLine("/:.");
/*
			if (apps == null && appConfigDir == null && appConfigFile == null)
				server.AddApplicationsFromCommandLine ("/:.");
			{
			}
*/			

		}
		#endregion

		#region IThreadService Members
		/// <summary>
		/// Starts the thread service.
		/// </summary>
		/// <param name="config">
		/// Configuration file object that indicates which Collection 
		/// Store to use.
		/// </param>
		public void Start( Configuration config )
		{
			try 
			{
				if (server.Start (true) == false)
				{
					log.Error("The Web Service failed to start");
				}
				else
				{
					log.Error("The Web Service started");
				}
			}
			catch (Exception e) 
			{
				log.Error("There was a problem with the web server: {0}", e.Message);
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
		public void Custom(int message, string data)
		{
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			try 
			{
				server.Stop();
				log.Error("The Web Service stopped");
			}
			catch (Exception e) 
			{
				log.Error("Stopping the Web Service resulted in: {0}", e.Message);
			}
		}
		#endregion
	}
}
