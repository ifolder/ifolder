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
using System.Net;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Web;

using Simias.Client;

namespace Simias.Server
{
	/// <summary>
	/// Simias Server
	/// </summary>
	public class SimiasServer
	{
		private static readonly string Path = @"simias10";
		private static readonly string Dir = MyEnvironment.Unix ? @"../web" : @"web";
		private static readonly int Port = 8086;
		private static readonly string Page = @"Simias.asmx";
		private static readonly string Application = @"SimiasApp.exe";
		private static readonly string Mono = @"mono";

		/// <summary>
		/// The main entry point for the application
		/// </summary>
		static void Main(string[] args)
		{
            Console.WriteLine();
            Console.WriteLine("SIMIAS");
            Console.WriteLine();

            int port = Port;

			// parse a port parameter
			if (args.Length > 0)
			{
				try
				{
					port = int.Parse(args[0]);
				}
				catch { }
			}

			// application 
            string application = System.IO.Path.Combine(SimiasSetup.webbindir, Application);

            // arguments
            string arguments = String.Format("--applications /{0}:{1} --port {2}",
                Path, Dir, port);

            // adjust for mono
            if (MyEnvironment.Mono)
            {
                arguments = String.Format("{0} {1}", application, arguments);
                application = Mono;
            }

            // start
            Process p = new Process();
            p.StartInfo.FileName = application;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.CreateNoWindow = false;
            p.EnableRaisingEvents = true;
            p.Start();
            
            Thread.Sleep(TimeSpan.FromSeconds(1));

            // ping
            UriBuilder ub = new UriBuilder("http://localhost");
            ub.Port = port;
            ub.Path = String.Format("{0}/{1}", Path, Page);

            Console.WriteLine("Pinging: {0}", ub);

			try
            {
				SimiasWebService svc = new SimiasWebService();
				svc.Url = ub.Uri.ToString();
				svc.PingSimias();
            }
            catch(Exception)
            {
				// ignore
			}

			if (!p.HasExited)
			{
				// wait
				Console.ReadLine();

				// stop
				p.StandardInput.WriteLine();
				p.WaitForExit();
			}
		}
	}
}
