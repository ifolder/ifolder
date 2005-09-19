//
// server.cs: Web Server that uses ASP.NET hosting
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// (C) Copyright 2004 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web.Hosting;

namespace Mono.ASPNET
{
	public class Server
	{
		private static ApplicationServer server;

		static void ShowVersion ()
		{
			Assembly assembly = Assembly.GetExecutingAssembly ();
			string version = assembly.GetName ().Version.ToString ();
			object [] att = assembly.GetCustomAttributes (typeof (AssemblyTitleAttribute), false);
			//string title = ((AssemblyTitleAttribute) att [0]).Title;
			att = assembly.GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false);
			string copyright = ((AssemblyCopyrightAttribute) att [0]).Copyright;
			att = assembly.GetCustomAttributes (typeof (AssemblyDescriptionAttribute), false);
			string description = ((AssemblyDescriptionAttribute) att [0]).Description;
			Console.WriteLine ("{0} {1}\n(c) {2}\n{3}",
					Path.GetFileName (assembly.Location), version, copyright, description);
		}

		static void ShowHelp ()
		{
#if MODMONO_SERVER
			Console.WriteLine ("mod-mono-server.exe is a ASP.NET server used from mod_mono.");
			Console.WriteLine ("Usage is:\n");
			Console.WriteLine ("    mod-mono-server.exe [...]");
			Console.WriteLine ();
			Console.WriteLine ("    The arguments --filename and --port are mutually exlusive.");
			Console.WriteLine ("    --filename file: a unix socket filename to listen on.");
			Console.WriteLine ("                    Default value: /tmp/mod_mono_server");
			Console.WriteLine ("                    AppSettings key name: MonoUnixSocket");
			Console.WriteLine ();
#else
			Console.WriteLine ("XSP server is a sample server that hosts the ASP.NET runtime in a");
			Console.WriteLine ("minimalistic HTTP server\n");
			Console.WriteLine ("Usage is:\n");
			Console.WriteLine ("    xsp.exe [...]");
			Console.WriteLine ();
#endif
			Console.WriteLine ("    --port N: n is the tcp port to listen on.");
#if MODMONO_SERVER
			Console.WriteLine ("                    Default value: none");
#else
			Console.WriteLine ("                    Default value: 8080");
#endif
			Console.WriteLine ("                    AppSettings key name: MonoServerPort");
			Console.WriteLine ();
			Console.WriteLine ("    --address addr: addr is the ip address to listen on.");
#if MODMONO_SERVER
			Console.WriteLine ("                    Default value: 127.0.0.1");
#else
			Console.WriteLine ("                    Default value: 0.0.0.0");
#endif
			Console.WriteLine ("                    AppSettings key name: MonoServerAddress");
			Console.WriteLine ();
			Console.WriteLine ("    --root rootdir: the server changes to this directory before");
			Console.WriteLine ("                    anything else.");
			Console.WriteLine ("                    Default value: current directory.");
			Console.WriteLine ("                    AppSettings key name: MonoServerRootDir");
			Console.WriteLine ();
			Console.WriteLine ("    --appconfigfile FILENAME: adds application definitions from the XML");
			Console.WriteLine ("                    configuration file. See sample configuration file that");
			Console.WriteLine ("                    comes with the server.");
			Console.WriteLine ("                    AppSettings key name: MonoApplicationsConfigFile");
			Console.WriteLine ();
			Console.WriteLine ("    --appconfigdir DIR: adds application definitions from all XML files");
			Console.WriteLine ("                    found in the specified directory DIR. Files must have");
			Console.WriteLine ("                    '.webapp' extension");
			Console.WriteLine ("                    AppSettings key name: MonoApplicationsConfigDir");
			Console.WriteLine ();
			Console.WriteLine ("    --applications APPS:");
			Console.WriteLine ("                    a comma separated list of virtual directory and");
			Console.WriteLine ("                    real directory for all the applications we want to manage");
			Console.WriteLine ("                    with this server. The virtual and real dirs. are separated");
			Console.WriteLine ("                    by a colon. Optionally you may specify virtual host name");
			Console.WriteLine ("                    and a port.");
			Console.WriteLine ();
			Console.WriteLine ("                           [[hostname:]port:]VPath:realpath,...");
			Console.WriteLine ();
			Console.WriteLine ("                    Samples: /:.");
			Console.WriteLine ("                           the virtual / is mapped to the current directory.");
			Console.WriteLine ();
			Console.WriteLine ("                            /blog:../myblog");
			Console.WriteLine ("                           the virtual /blog is mapped to ../myblog");
			Console.WriteLine ();
			Console.WriteLine ("                            myhost.someprovider.net:/blog:../myblog");
			Console.WriteLine ("                           the virtual /blog at myhost.someprovider.net is mapped to ../myblog");
			Console.WriteLine ();
			Console.WriteLine ("                            /:.,/blog:../myblog");
			Console.WriteLine ("                           Two applications like the above ones are handled.");
			Console.WriteLine ("                    Default value: /:.");
			Console.WriteLine ("                    AppSettings key name: MonoApplications");
			Console.WriteLine ();
#if MODMONO_SERVER
			Console.WriteLine ("    --terminate: gracefully terminates a running mod-mono-server instance.");
			Console.WriteLine ("                 All other options but --filename or --address and --port");
			Console.WriteLine ("                 are ignored if this option is provided.");
#endif
			Console.WriteLine ("    --nonstop: don't stop the server by pressing enter. Must be used");
			Console.WriteLine ("               when the server has no controlling terminal.");
			Console.WriteLine ();
			Console.WriteLine ("    --version: displays version information and exits.");
			Console.WriteLine ("    --verbose: prints extra messages. Mainly useful for debugging.");

			Console.WriteLine ();
		}

		[Flags]
		enum Options {
			NonStop = 1,
			Verbose = 1 << 1,
			Applications = 1 << 2,
			AppConfigDir = 1 << 3,
			AppConfigFile = 1 << 4,
			Root = 1 << 5,
			FileName = 1 << 6,
			Address = 1 << 7,
			Port = 1 << 8,
			Terminate = 1 << 9,
		}

		static void CheckAndSetOptions (string name, Options value, ref Options options)
		{
			if ((options & value) != 0) {
				ShowHelp ();
				Console.Error.WriteLine ();
				Console.Error.WriteLine ("ERROR: Option '{0}' duplicated.", name);
				Environment.Exit (1);
			}

			options |= value;
			if ((options & Options.FileName) != 0 &&
			    ((options & Options.Port) != 0 || (options & Options.Address) != 0)) {
				ShowHelp ();
				Console.Error.WriteLine ();
				Console.Error.WriteLine ("ERROR: --port/--address and --filename are mutually exclusive");
				Environment.Exit (1);
			}
		}

		public static int Start (string [] args)
		{
			bool nonstop = false;
			bool verbose = false;
			Trace.Listeners.Add (new TextWriterTraceListener (Console.Out));
			string apps = ConfigurationSettings.AppSettings ["MonoApplications"];
			string appConfigDir = ConfigurationSettings.AppSettings ["MonoApplicationsConfigDir"];
			string appConfigFile = ConfigurationSettings.AppSettings ["MonoApplicationsConfigFile"];
			string rootDir = ConfigurationSettings.AppSettings ["MonoServerRootDir"];
			object oport;
			string ip = ConfigurationSettings.AppSettings ["MonoServerAddress"];
#if MODMONO_SERVER
			string filename = ConfigurationSettings.AppSettings ["MonoUnixSocket"];
#endif
			if (ip == "" || ip == null)
				ip = "0.0.0.0";

			oport = ConfigurationSettings.AppSettings ["MonoServerPort"];
			if (oport == null)
				oport = 8080;

			Options options = 0;
			for (int i = 0; i < args.Length; i++){
				string a = args [i];
				
				switch (a){
#if MODMONO_SERVER
				case "--filename":
					CheckAndSetOptions (a, Options.FileName, ref options);
					filename = args [++i];
					break;
				case "--terminate":
					CheckAndSetOptions (a, Options.Terminate, ref options);
					break;
#endif
				case "--port":
					CheckAndSetOptions (a, Options.Port, ref options);
					oport = args [++i];
					break;
				case "--address":
					CheckAndSetOptions (a, Options.Address, ref options);
					ip = args [++i];
					break;
				case "--root":
					CheckAndSetOptions (a, Options.Root, ref options);
					rootDir = args [++i];
					break;
				case "--applications":
					CheckAndSetOptions (a, Options.Applications, ref options);
					apps = args [++i];
					break;
				case "--appconfigfile":
					CheckAndSetOptions (a, Options.AppConfigFile, ref options);
					appConfigFile = args [++i];
					break;
				case "--appconfigdir":
					CheckAndSetOptions (a, Options.AppConfigDir, ref options);
					appConfigDir = args [++i];
					break;
				case "--nonstop":
					nonstop = true;
					break;
				case "--help":
					ShowHelp ();
					return 0;
				case "--version":
					ShowVersion ();
					return 0;
				case "--verbose":
					verbose = true;
					break;
				default:
					Console.Error.WriteLine ("Unknown argument: {0}", a);
					ShowHelp ();
					return 1;
				}
			}

#if MODMONO_SERVER
			bool useTCP = ((options & Options.Port) != 0);
			if (!useTCP) {
				if (filename == null || filename == "")
					filename = "/tmp/mod_mono_server";

				if ((options & Options.Address) != 0) {
					ShowHelp ();
					Console.WriteLine ();
					Console.WriteLine ("ERROR: --address without --port");
					Environment.Exit (1);
				}
			}
#endif
			IPAddress ipaddr = null;
			ushort port;
			try {
				port = Convert.ToUInt16 (oport);
			} catch (Exception) {
				Console.Error.WriteLine ("The value given for the listen port is not valid: " + oport);
				return 1;
			}

			try {
				ipaddr = IPAddress.Parse (ip);
			} catch (Exception) {
				Console.Error.WriteLine ("The value given for the address is not valid: " + ip);
				return 1;
			}

			if (rootDir != null && rootDir != "") {
				try {
					Environment.CurrentDirectory = rootDir;
				} catch (Exception e) {
					Console.Error.WriteLine ("Error: {0}", e.Message);
					return 1;
				}
			}

			rootDir = Directory.GetCurrentDirectory ();
			
			IWebSource webSource;
#if MODMONO_SERVER
			if (useTCP) {
				webSource = new ModMonoTCPWebSource (ipaddr, port);
			} else {
				webSource = new ModMonoWebSource (filename);
			}

			if ((options & Options.Terminate) != 0) {
				if (verbose)
					Console.WriteLine ("Shutting down running mod-mono-server...");
				
				bool res = ((ModMonoWebSource) webSource).GracefulShutdown ();
				if (verbose)
					Console.WriteLine (res ? "Done." : "Failed");

				return (res) ? 0 : 1;
			}

			server = new ApplicationServer (webSource);
#else
			webSource = new XSPWebSource (ipaddr, port);
			server = new ApplicationServer (webSource);
#endif
			server.Verbose = verbose;

			if ( verbose )
				Console.Error.WriteLine (Assembly.GetExecutingAssembly ().GetName ().Name);
			if (apps != null)
				server.AddApplicationsFromCommandLine (apps);

			if (appConfigFile != null)
				server.AddApplicationsFromConfigFile (appConfigFile);

			if (appConfigDir != null)
				server.AddApplicationsFromConfigDirectory (appConfigDir);

			if (apps == null && appConfigDir == null && appConfigFile == null)
				server.AddApplicationsFromCommandLine ("/:.");
#if MODMONO_SERVER
			if (!useTCP) {
				Console.WriteLine ("Listening on: {0}", filename);
			} else
#endif
			{
				if ( verbose )
				{
					Console.Error.WriteLine ("Listening on port: {0}", port);
					Console.Error.WriteLine ("Listening on address: {0}", ip);
				}
			}
			
			if ( verbose )
				Console.Error.WriteLine ("Root directory: {0}", rootDir);

			try {
				if (server.Start (true) == false)
					return 2;

				if (!nonstop) 
				{
					if ( verbose )
						Console.Error.WriteLine ("Hit Return to stop the server.");

					Console.ReadLine ();
					server.Stop ();
				}
			} catch (Exception e) {
				Console.Error.WriteLine ("Error: {0}", e.Message);
				Console.Error.WriteLine ("Stack: {0}", e.StackTrace);
				return 1;
			}

			return 0;
		}

		public static void Stop()
		{
			server.Stop();
		}
	}
}

