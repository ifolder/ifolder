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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *	Author: Boyd Timothy <btimothy@novell.com>
 ***********************************************************************/

using System;
using System.Net;
using Gtk;
using Gnome;
using Novell.iFolder;
using Simias.Client;
using Novell.iFolder.Controller;

namespace Novell.iFolder.Nautilus
{
	public class NautilusiFolder
	{
		private static SimiasEventBroker	simiasEventBroker;
		private static iFolderWebService	ifws;
		private static SimiasWebService	simws;
		private static iFolderData		ifdata;
		private static Manager			simiasManager;
		private static DomainController	domainController;
		private static bool				forceShutdown = false;
		
		public static int Main (string[] args)
		{
			// Don't do anything if nothing was specified to do
			if (args.Length == 0)
				return 0;

			// Make sure this process is a gnome program
			Gnome.Program program = 
				new Program ("Nautilus-Extension-UI", "0.1.0", Modules.UI, args);
				
			// Get the localized strings loaded
			Util.InitCatalog();
			
			switch (args [0]) {
				case "share":
					return showShareDialog (args);
				case "properties":
					return showPropertiesDialog (args);
				case "help":
					return showHelp (args);
			}
			
			program.Run ();
			return 0;
		}
		
		public static void StartSimias(string[] args)
		{
			bool simiasRunning = false;

			simiasManager = Util.CreateSimiasManager(args);
			
			simiasManager.Start();
			
			string localServiceUrl = simiasManager.WebServiceUri.ToString();
			ifws = new iFolderWebService();
			ifws.Url = localServiceUrl + "/iFolder.asmx";
			LocalService.Start(ifws, simiasManager.WebServiceUri, simiasManager.DataPath);
			
			simws = new SimiasWebService();
			simws.Url = localServiceUrl + "/Simias.asmx";
			LocalService.Start(simws, simiasManager.WebServiceUri, simiasManager.DataPath);
			
			while (!simiasRunning)
			{
				try
				{
					ifws.Ping();
					simiasRunning = true;
				}
				catch(Exception e)
				{
					Console.WriteLine(e.Message);
				}
				
				if (forceShutdown)
					ForceQuit();
				
				// Wait and ping again
				System.Threading.Thread.Sleep(10);
			}
			
			if (forceShutdown)
				ForceQuit();
			else
			{
				try
				{
					simiasEventBroker = SimiasEventBroker.GetSimiasEventBroker();
					
					// set up to have data ready for events
					ifdata = iFolderData.GetData();
					
					domainController = DomainController.GetDomainController();
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
					ifws = null;
					ForceQuit();
				}
			}
		}
		
		public static void ForceQuit()
		{
			try
			{
				simiasManager.Stop();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
			
			System.Environment.Exit(-1);
		}
		
		public static int StopSimias()
		{
			try
			{
				simiasManager.Stop();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				return -1;
			}

			return 0;
		}
		
		private static int showShareDialog (string[] args)
		{
			if (args.Length < 2) {
				Console.Write ("ERROR: iFolder ID not specified\n");
				return -1;
			}

			StartSimias(args);
			
			iFolderPropertiesDialog propsDialog;
			propsDialog = new iFolderPropertiesDialog (args [1], simiasManager);
			propsDialog.CurrentPage = 1;
			propsDialog.Run ();
			propsDialog.Hide ();
			propsDialog.Destroy ();

			return StopSimias();
		}
		
		private static int showPropertiesDialog (string[] args)
		{
			if (args.Length < 2) {
				Console.Write ("ERROR: iFolder ID not specified\n");
				return -1;
			}
			
			StartSimias(args);
			
			iFolderPropertiesDialog propsDialog;
			propsDialog = new iFolderPropertiesDialog (args [1], simiasManager);
			propsDialog.CurrentPage = 0;
			propsDialog.Run ();
			propsDialog.Hide ();
			propsDialog.Destroy ();

			return StopSimias();
		}
		
		private static int showHelp (string[] args)
		{
			Util.ShowHelp("front.html", null);
			return 0;
		}
	}
}