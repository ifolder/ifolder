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
 *  Author: banderso@novell.com
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting;
using System.Threading;
using System.Web;

using Simias;
//using Simias.Client;
//using Simias.Client.Event;
//using Simias.Event;
using Simias.Storage;
//using Simias.Sync;


namespace Simias.SystemMonitor
{
	/// <summary>
	/// Manager calls to start and stop the service
	/// </summary>
	public class Manager : IDisposable
	{
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(typeof(Simias.SystemMonitor.Manager));

		private bool			started = false;
		private	bool			stop = false;
        private int             waitTime = (60 * 1000);
		private Configuration	config;
		private Thread			monitorThread = null;
		private AutoResetEvent	stopEvent;
		private Store			store;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="config">Simias configuration</param>
		public Manager(Configuration config)
		{
			this.config = config;
			
			// store
			store = Store.GetStore();

		}

		/// <summary>
		/// Start the system monitor.
		/// </summary>
		public void Start()
		{
			log.Debug("Start called");

			try
			{
				lock(this)
				{
					if (started == false)
					{
						this.monitorThread = new Thread(new ThreadStart(this.SystemMonitorThread));
						this.monitorThread.IsBackground = true;
						this.monitorThread.Priority = ThreadPriority.BelowNormal;
						this.stopEvent = new AutoResetEvent(false);

						this.monitorThread.Start();
					}
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to start System Monitor thread.");
				throw e;
			}
		}

		/// <summary>
		/// Stop the system monitor.
		/// </summary>
		public void Stop()
		{
			log.Debug("Stop called");
			try
			{
				lock(this)
				{
					// Set state and then signal the event
					this.stop = true;
					this.stopEvent.Set();
					Thread.Sleep(32);
					this.stopEvent.Close();
					Thread.Sleep(0);
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to stop System Monitor.");
				throw e;
			}
		}

		/// <summary>
		/// System Monitor Thread.
		/// </summary>
		public void SystemMonitorThread()
		{
			log.Debug("SystemMonitorThread started");
            int iterations = 1;
			int status;
            int portThreads = 0;
            int workerThreads = 0;

			this.started = true;

			do 
			{
                try
                {
                    // First let's log the state of the threadpool
                    ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
                    log.Info("Thread Pool - Maximum Threads: " + workerThreads.ToString());

                    ThreadPool.GetAvailableThreads(out workerThreads, out portThreads);
                    log.Info("Thread Pool - Available Threads: " + workerThreads.ToString());

                    // Get the state of the garbage collector
                    log.Info("Garbage Collector - Maximum Generations: " + GC.MaxGeneration.ToString());
                    log.Info("Garbage Collector - Total Memory: " + GC.GetTotalMemory(false).ToString());

                    if ((iterations % 10) == 0)
                    {
                        log.Info("Collecting memory");
                        GC.Collect();
                    }

				}
				catch(Exception e)
				{
					log.Error(e.Message);
					log.Error(e.StackTrace);
				}

				stopEvent.WaitOne(waitTime, false);
                ++iterations;

			} while(this.stop == false);

			this.started = false;
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Stop();
		}

		#endregion
	}
}
