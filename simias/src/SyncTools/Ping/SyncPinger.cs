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
using System.Threading;
using System.Net;

using Simias;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Sync.Tools
{
	/// <summary>
	/// Sync Pinger
	/// </summary>
	public class SyncPinger
	{
		private static readonly int interval = 1;

		private string host;
		private Thread worker;
		private bool working;
		
		private int sent;
		private int recv;
		private int min;
		private int max;
		private int total;

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="host">The sync server host.</param>
		private SyncPinger(string host)
		{
			this.host = host;
		}

		private void Start()
		{
			// counters
			sent = 0;
			recv = 0;
			min = 0;
			max = 0;
			total = 0;

			// thread
			worker = new Thread(new ThreadStart(this.DoWork));
			
			// start thread
			working = true;
			worker.Start();
		}

		private void Stop()
		{
			// stop thread
			working = false;
			worker.Interrupt();
			worker.Join();

			// stats
			Console.WriteLine("PING STATISTICS");
			Console.WriteLine("\tPings: Sent = {0}, Received = {1}, Lost = {2}",
				sent, recv, (sent - recv));
			Console.WriteLine("\tRTT: Average = {0}, Minimum = {1}, Maximum = {2}",
				((recv != 0) ? (total / recv) : 0), min, max);
		}

		/// <summary>
		/// Ping the sync server
		/// </summary>
		private void DoWork()
		{
			while(working)
			{
				SyncStoreInfo info;
				int start = 0, stop = 0;
	
				try
				{
					++sent;

					start = Environment.TickCount;

					info = SyncPing.PingStore(Store.GetStore(), host);
				}
				catch
				{
					info = null;
				}
				finally
				{
					stop = Environment.TickCount;
				}

				// time
				int time = stop - start;

				// output
				if (info != null)
				{
					++recv;
					
					Console.WriteLine(info);
					Console.WriteLine("\tRTT: {0}ms", time);
					
					total += time;
					min = Math.Min(min, time);
					max = Math.Max(max, time);
				}
				else
				{
					Console.WriteLine("Lost Ping");
				}
				Console.WriteLine();

				try
				{
					// sleep
					Thread.Sleep(TimeSpan.FromSeconds(interval));
				}
				catch
				{
					// ignore
				}
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			// title
			Console.WriteLine();
			Console.WriteLine("Simias Sync Ping");
			Console.WriteLine();

			// usage
			if (args.Length != 1)
			{
				Console.WriteLine("USAGE: SyncPing host");

				return -1;
			}

			// host
			string host = args[0];
			
			// info
			Console.WriteLine("Press [Enter] to exit...");
			Console.WriteLine();
			
			Console.WriteLine("PINGING {0}:", host);
			
			Console.WriteLine();

			SyncPinger pinger = new SyncPinger(host);
			
			pinger.Start();

			Console.ReadLine();

			pinger.Stop();

			return 0;
		}
	}
}
