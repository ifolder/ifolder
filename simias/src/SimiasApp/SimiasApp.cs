/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Net;

using Mono.ASPNET;

namespace Simias.Web
{
	/// <summary>
	/// Simias Application
	/// </summary>
	class SimiasApp
	{

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			ApplicationServer server;
			IPAddress ipaddr = null;
			ushort port;

			// CRG: This is a total hack for now
			string curdir = Directory.GetCurrentDirectory();
			string winweb = curdir + 
								System.IO.Path.DirectorySeparatorChar + "web";
			string nixweb = curdir + System.IO.Path.DirectorySeparatorChar + 
						".." + System.IO.Path.DirectorySeparatorChar + "web";

			Console.WriteLine(curdir);
			Console.WriteLine(winweb);
			Console.WriteLine(nixweb);

			// check for web dir on Windows
			if(Directory.Exists(winweb))
			{
				Console.WriteLine("We are getting here");
				Environment.CurrentDirectory = winweb;
			}
			else if(Directory.Exists(nixweb))
			{
				Environment.CurrentDirectory = nixweb;
//				Directory.SetCurrentDirectory(nixweb);
				Console.WriteLine("Just set: " + nixweb);
			}

			Console.WriteLine("Setting root to: {0}", Directory.GetCurrentDirectory());

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
				Console.WriteLine("SimiasApp failed to start");
			}
			else
			{
				Console.WriteLine("SimiasApp Asp.Net started");
			}
			Console.WriteLine("Press any key to quit");
			Console.Read();

			server.Stop();
			return 0;
        }



	}
}
