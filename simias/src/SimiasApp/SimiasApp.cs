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

using Simias;
using Simias.Service;
using Simias.Event;
using Simias.Sync;

namespace Simias
{
	/// <summary>
	/// Simias Application
	/// </summary>
	class SimiasApp
	{
		private Manager manager;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
            SimiasApp simias = new SimiasApp();

            // do command
            return simias.DoCommand(args);
        }

        private SimiasApp()
        {
        }

        private int DoCommand(string[] args)
        {
			int result = -1;
			
			Console.WriteLine("Simias Application");
			Console.WriteLine();
			
			// check arguments
            if (args.Length < 1)
			{
				ShowUsage();
				return -1;
			}

			// create configuration
			Configuration config;

            if (args.Length > 1)
            {
                config = Configuration.CreateDefaultConfig(Path.GetFullPath(args[1]));
            }
            else
            {
                config = Configuration.GetConfiguration();
            }

			// command
			string command = args[0];

			switch(command)
			{
				// start the services
				case "start":
				{
					Console.WriteLine("Starting...");

					// configure logging
					SimiasLogManager.Configure(config);
			
					// set the sync properties
					SyncProperties props = new SyncProperties(config);
					props.LogicFactory = typeof(SynkerA);
			
					// create a manager
					manager = new Manager(config);

					// add a shutdown event handler
					manager.Shutdown +=new ShutdownEventHandler(this.Shutdown);
				
					// start services and wait
					manager.StartServices();
					manager.WaitForServicesStarted();

					Console.WriteLine("Running...");

					// hang around for the end
					manager.WaitForServicesStopped();
					
					Console.WriteLine("Stopped.");

					result = 0;
					break;
				}

				case "stop":
				{
					Console.WriteLine("Stopping...");

					// publish the shutdown event
					EventPublisher p = new EventPublisher(config);

					p.RaiseEvent(new ShutdownEventArgs());

					result = 0;
					break;
				}

				default:
				{
					Console.WriteLine("Unkown Command: {0}", command);
					ShowUsage();
					break;
				}
			}

			Console.WriteLine("Done.");

			return result;
		}

		/// <summary>
		/// Handle the shutdown event.
		/// </summary>
		/// <param name="args">Shutdown Event Arguments</param>
		private void Shutdown(ShutdownEventArgs args)
		{
			Console.WriteLine("Stopping...");
			
            // stop services and wait
			manager.StopServices();
			manager.WaitForServicesStopped();
		}

		private static void ShowUsage()
		{
			Console.WriteLine();
			Console.WriteLine("USAGE: SimiasApp [start|stop] [path]");
			Console.WriteLine();
		}
	}
}
