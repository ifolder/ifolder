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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

using System;
using Novell.AddressBook;
using Novell.AddressBook.UI.gtk;
using System.Collections;
using Simias;
using Simias.Sync;
using Simias.Domain;
using System.Diagnostics;
using System.Threading;

using Gtk;
using Gdk;
using Gnome;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	public class iFolderApp 
	{
		static Configuration conf;
		static Simias.Service.Manager sManager = null;

		public static void Main (string[] args)
		{
			conf = Configuration.GetConfiguration();
			Process[] processes = 
				System.Diagnostics.Process.GetProcessesByName("iFolderApp");

			if(processes.Length > 1)
			{
				Console.WriteLine("iFolder is already running!");
				return;
			}

			Gnome.Program program =
				new Program("iFolder", "0.10.0", Modules.UI, args);
			
			// This is my huge try catch block to catch any exceptions
			// that are not caught
			try
			{
				ApplicationProperties ap = new ApplicationProperties();
				ap.ShowAll();

				sManager = new Simias.Service.Manager(conf);

				SimiasLogManager.Configure(conf);

				SyncProperties props = new SyncProperties(conf);
				props.LogicFactory = typeof(SynkerA);

				Console.WriteLine("Starting iFolder Services...");
				sManager.StartServices();
				sManager.WaitForServicesStarted();
				Console.WriteLine("iFolder is running.");

				program.Run();
			}
			catch(Exception bigException)
			{
				if(sManager != null)
					sManager.StopServices();

				CrashReport cr = new CrashReport();
				cr.CrashText = bigException.ToString();
				cr.Run();
				sManager.WaitForServicesStopped();
				Application.Quit();
			}
		}
	}
}
