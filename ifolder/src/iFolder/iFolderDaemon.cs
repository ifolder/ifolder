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
using System.Collections;
using Simias;
using Simias.Sync;

namespace Novell.iFolder
{
	public class iFolderDaemon
	{
		static Configuration conf;
		static Simias.Service.Manager sManager = null;

		public static void Main (string[] args)
		{
			// This is my huge try catch block to catch any exceptions
			// that are not caught
			try
			{
				Console.WriteLine("iFolder is Starting...");
				conf = new Configuration();

				SimiasLogManager.Configure(conf);

				SyncProperties props = new SyncProperties(conf);
				props.LogicFactory = typeof(SynkerA);

				sManager = new Simias.Service.Manager(conf);
				sManager.StartServices();
				sManager.WaitForServicesStarted();

				Console.WriteLine("iFolder is Running. Press any key to exit...");
				Console.Read();

				Console.WriteLine("iFolder is Shutting down...");

				sManager.StopServices();
				sManager.WaitForServicesStopped();

				Console.WriteLine("iFolder is Stopped.");
			}
			catch(Exception bigException)
			{
				if(sManager != null)
					sManager.StopServices();

				sManager.WaitForServicesStopped();
			}
		}
	}
}
