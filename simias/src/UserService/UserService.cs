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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Threading;
using System.Collections;
using Simias.Event;

namespace Simias
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class UserService
	{
		ServiceEventSubscriber		serviceEventWatcher;
		int							processId;
		ManualResetEvent			shutdownEvent = new ManualResetEvent(false);
		Configuration				conf;
		
		protected void Run(Configuration conf)
		{
			this.conf = conf;
			processId = System.Diagnostics.Process.GetCurrentProcess().Id;
			Thread t1 = new Thread(new ThreadStart(waitForShutdown));
			t1.Start();
			t1.Join();
		}

		public UserService()
		{
			serviceEventWatcher = new ServiceEventSubscriber(conf);
			serviceEventWatcher.ServiceControl += new ServiceEventHandler(OnServiceEvent);
		}

		protected virtual void OnStart()
		{
			System.Diagnostics.Debug.WriteLine("UserService.OnStart called in Base class");
		}

		protected virtual void OnStop()
		{
		}

		protected virtual void OnShutdown()
		{
		}

		protected virtual void OnReconfigure()
		{
		}

		private void waitForShutdown()
		{
			shutdownEvent.WaitOne();
			serviceEventWatcher.Dispose();
			OnShutdown();
		}

		private void OnServiceEvent(ServiceEventArgs args)
		{
			if (args.Target == this.processId || args.Target == ServiceEventArgs.TargetAll)
			{
				switch (args.ControlEvent)
				{
					case ServiceControl.Shutdown:
						shutdownEvent.Set();
						break;
					case ServiceControl.Reconfigure:
						OnReconfigure();
						break;
				}
			}
		}
	}
}
