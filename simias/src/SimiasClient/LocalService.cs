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
 *  Author: Mike Lasky
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Services.Protocols;

namespace Simias.Client
{
	/// <summary>
	/// Class that deals with 
	/// </summary>
	public class LocalService
	{
		#region Class Members

		/// <summary>
		/// Name of the local password file that is used to authenticate local
		/// web services.
		/// </summary>
		static private readonly string LocalPasswordFile = ".local.if";

		/// <summary>
		/// Caches the local password.
		/// </summary>
		static private string localPassword = null;

		/// <summary>
		/// Same container is used for all local web services.
		/// </summary>
		static private CookieContainer cookies = new CookieContainer();

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the local password.
		/// </summary>
		static private void GetLocalPassword()
		{
			lock( typeof( LocalService ) )
			{
				if ( localPassword == null )
				{
					Configuration config = new Configuration();
					string path = Path.Combine( config.StorePath, LocalPasswordFile );

					try
					{
						using ( StreamReader sr = new StreamReader( path ) )
						{
							localPassword = sr.ReadLine();
						}
					}
					catch
					{}
				}
			}
		}

		/// <summary>
		/// Pings the local web service to get simias started if it is not
		/// started already.
		/// </summary>
		static private void Ping()
		{
			try
			{
				SimiasWebService webService = new SimiasWebService();
				webService.Url = Manager.LocalServiceUrl.ToString() + "/Simias.asmx";
				webService.PingSimias();
			}
			catch
			{}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Connects the specified webClient to its web service.
		/// </summary>
		/// <param name="webClient">HttpWebClientProtocol object.</param>
		static public void Start( HttpWebClientProtocol webClient )
		{
			// Start the web service so the password file will be created.
			while ( localPassword == null )
			{
				Ping();
				GetLocalPassword();
				if ( localPassword == null )
				{
					Thread.Sleep( 500 );
				}
			}

			webClient.Credentials = new NetworkCredential( Environment.UserName, localPassword );
			webClient.PreAuthenticate = true;

			// BUGBUG!! - Force mono to authenticate everytime until cookies work on a loopback
			// connection.
#if WINDOWS
			webClient.CookieContainer = cookies;
#endif
		}

		/// <summary>
		/// Clears the cached credentials.
		/// </summary>
		static public void ClearCredentials()
		{
			localPassword = null;
		}

		#endregion
	}
}
