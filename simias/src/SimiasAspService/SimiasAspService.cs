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
	public class SimiasAspService : IThreadService
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
		public SimiasAspService()
		{
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
				IPAddress ipaddr = null;
				ushort port;

				port = Convert.ToUInt16 (8086);
				ipaddr = IPAddress.Parse ("0.0.0.0");

				IWebSource webSource = new XSPWebSource (ipaddr, port);
				server = new ApplicationServer (webSource);

				server.Verbose = false;

				// not sure what this does
				// but it startup up asmx file and xsp did it
				server.AddApplicationsFromCommandLine("/:.");

				if (server.Start (false) == false)
				{
					log.Error("Simias ASP.Net Service failed to start");
				}
				else
				{
					log.Debug("Simias ASP.Net Service started");
				}
			}
			catch (Exception e) 
			{
				log.Error("Error starting Simias ASP.Net Service: {0}", 
						e.Message);
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
				if(server != null)
				{
					server.Stop();
					log.Debug("Simias ASP.Net Service stopped");
				}
				else
				{
					log.Error("Stop was called in Simias ASP.Net when the server object was null");
				}
			}
			catch (Exception e) 
			{
				log.Error("Error Stopping Simias ASP.Net Service: {0}", 
						e.Message);
			}
		}
		#endregion
	}
}
