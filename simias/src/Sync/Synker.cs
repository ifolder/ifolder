/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Dale Olds <olds@novell.com>
 * 
 ***********************************************************************/
using System;
using System.Collections;
using System.Threading;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;

[assembly: AssemblyTitle("Sync")]
[assembly: AssemblyDescription("Novell's Simias Sync Component")]
[assembly: AssemblyCompany("Novell, Inc.")]
[assembly: AssemblyProduct("Simias 3.0")]
[assembly: AssemblyVersion("0.1.*")]

namespace Simias.Sync
{

//---------------------------------------------------------------------------

/// <summary>
/// delegate passed to to be called with status updates, right now just
/// </summary>
public delegate void StatusUpdate(bool active);

/// <summary>
/// Interface to the Sync class
/// </summary>
public interface ISync
{
	/// <summary>
	/// start the sync process.
	/// </summary>
	/// <param name="statusUpdate">callback to report any change in sync state</param>	/// <param name="storeLocation">location of collection store (should be a file:/// URI) or null</param>
	/// <param name="myHostName">needs to go away, currently used to determine if a collection URI refers to this store</param>
	/// <param name="serverPort">local port on which to accept update requests</param>
	/// <param name="syncInterval">seconds to delay between synchronization passes</param>
	void Start(StatusUpdate statusUpdate, Uri storeLocation,
			string myHostName, int serverPort, int syncInterval);

	/// <summary>
	/// stop the synchronizing collections
	/// </summary>
	void Stop();
}

#if false
//---------------------------------------------------------------------------
internal class SyncClient
{
	private Uri storeLocation;
	private SyncPoint target;
	private StatusUpdate statusUpdate;

	public void Start(StatusUpdate statusUpdate, SyncPoint target,
			Uri storeLocation)
	{
		lock (this)
		{
			Trace.WriteLine(String.Format("SyncClient.Start(): [Url: {0}, Id: {1}]",
				target.masterUri, target.userId));

			if (this.statusUpdate != null && this.statusUpdate != statusUpdate
					|| target == null)
				throw new ArgumentException();
			this.statusUpdate = statusUpdate;
			this.storeLocation = storeLocation;
			if (this.target == null)
			{
				this.target = target;
				if (!ThreadPool.QueueUserWorkItem(new WaitCallback(Runner)))
					throw new SystemException("unexpected ThreadPool failure");
			}
			else
				Console.WriteLine("ignoring client start, previous client still running");
		}
	}

	public void Stop()
	{
		lock (this)
			target = null;
	}

	/* performs one synchronization pass for one collection in the store.
	 * This is typically called from a timer or threadpool thread. Make sure
	 * to catch all exceptions and do something with them, or the exception
	 * winds out to the thread pool and is lost.
	 */
	public void Runner(object unused)
	{
		Client client = null;
		statusUpdate(true);
		try
		{
			SyncPoint target;
			Uri storeLocation;

			lock (this)
			{
				target = this.target;
				storeLocation = this.storeLocation;
			}

			Trace.WriteLine(String.Format("SyncClient.Runner(): [Url: {0}, Id: {1}]",
				target.masterUri, target.userId));

			UriBuilder uri = new UriBuilder(target.masterUri);
			if (uri.Scheme != "nifp")
				throw new ArgumentException();
			uri.Scheme = "http";
			uri.Path = "/sync.rem";

			client = new Client(uri.ToString(), target.userId,
					target.credential, target.collectionId);

			SyncSession ss = (SyncSession)client.session;
			
			Trace.WriteLine(String.Format("SyncClient.Runner(): [Session Version: {0}]",
				ss.Version));

			SyncPass.Run(storeLocation, target.collectionId, ss);
			//Console.WriteLine("connected to {0}, version {1}", uri.ToString(), sp.Version);
		}
		catch (Exception e)
		{
			Trace.WriteLine(String.Format("SyncClient.Runner(): [Exception: {0}]",
				e.Message));
			
			Console.WriteLine("Uncaught exception in SyncClient.Runner: {0}\n{1}",
					e.Message, e.StackTrace);
		}
		finally
		{
			if (client != null)
				client.Stop();
			lock (this)
				target = null;
			statusUpdate(false);
		}
	}
}

//---------------------------------------------------------------------------
public class SyncServer
{
	private int port = 0;
	private Uri storeLocation = null;
	private StatusUpdate statusUpdate;
	private Server server = null;

	public void Start(StatusUpdate statusUpdate, int port, Uri storeLocation)
	{
		lock (this)
		{
			if (port == 0
				|| (this.statusUpdate != null && this.statusUpdate != statusUpdate)
				|| (this.storeLocation != null && this.storeLocation != storeLocation))
				throw new ArgumentException();

			if (port != this.port || storeLocation != this.storeLocation)
				Stop();
			else if (this.port != 0)
			{
				//Console.WriteLine("ignoring unecessary restart of server");
				return;
			}

			Trace.WriteLine(String.Format("SyncServer.Start(): [Store: {0}, Port: {1}]",
				storeLocation, port));

			this.port = port;
			this.storeLocation = storeLocation;
			this.statusUpdate = statusUpdate;

			Console.WriteLine("Starting server");
			server = new Server(port, storeLocation,
					new SessionFactory(SyncSession.SessionFactory));
			server.Start();
		}
		Console.WriteLine("Sync listening on port {0}", port);
	}

	public void Stop()
	{
		lock (this)
		{
			if (server != null)
			{
				Trace.WriteLine(String.Format("SyncServer.Stop(): [Store: {0}, Port: {1}]",
					storeLocation, port));

				server.Stop();
				server = null;
			}
		}
		Console.WriteLine("Stopped server");
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// The controlling class for collection synchronization
/// </summary>
public class Synker: ISync
{
	private Hashtable clients = null;
	private SyncServer server = null;
	private Timer timer = null;
	private Uri storeLocation = null;
	private int serverPort = 0, activeCount = 0;
	private string myHostName = null;
	private StatusUpdate statusUpdate = null;

	/// <summary>
	/// entry point to initialize this object and begin the periodic
	/// synchronization passes through the collection store
	/// </summary>
	/// <param name="statusUpdate">callback to report any change in sync state</param>
	/// <param name="storeLocation">location of collection store (should be a file:/// URI) or null</param>
	/// <param name="myHostName">needs to go away, currently used to determine if a collection URI refers to this store</param>
	/// <param name="serverPort">local port on which to accept update requests</param>
	/// <param name="syncInterval">seconds to delay between synchronization passes</param>
	public void Start(StatusUpdate statusUpdate, Uri storeLocation,
			string myHostName, int serverPort, int syncInterval)
	{
		if (syncInterval == 0 || serverPort == 0 || myHostName == null)
			throw new ArgumentException();

		lock (this)
		{
			Trace.WriteLine(String.Format("Synker.Start(): [Store: {0}, Host: {1}, Port: {2}]",
				storeLocation, myHostName, serverPort));

			this.serverPort = serverPort;
			this.storeLocation = storeLocation;
			this.myHostName = myHostName;
			this.statusUpdate = statusUpdate;
			if (timer == null)
				timer = new Timer(new TimerCallback(Runner), null,
					0, syncInterval * 1000);
			else
				timer.Change(0, syncInterval * 1000);
		}
	}

	/// <summary>
	/// entry point to stop the synchronization threads and prepare to shut down
	/// </summary>
	public void Stop()
	{
		Hashtable oldClients = null;
		SyncServer oldServer = null;
		lock (this)
		{
			Trace.WriteLine(String.Format("Synker.Stop(): [Store: {0}, Host: {1}, Port: {2}]",
				storeLocation, myHostName, serverPort));

			timer.Change(Timeout.Infinite, Timeout.Infinite);
			oldClients = clients;
			clients = null;
			oldServer = server;
			server = null;
		}

		if (oldServer != null)
			oldServer.Stop();
		if (oldClients != null)
			foreach (SyncClient client in oldClients.Values)
				client.Stop();
		GC.Collect();
	}

	/* TODO: this method should probably be protected with its own mutex so
	 * that statusUpdate calls cannot get out of order, a quick hack would
	 * be to just make an extra call periodically to clean up if we got off.
	 */
	private void StatusAccumulator(bool active)
	{
		if (active)
		{
			if (Interlocked.Increment(ref activeCount) == 1
					&& statusUpdate != null)
				statusUpdate(true);
		}
		else
		{
			int newcount = Interlocked.Decrement(ref activeCount);
			if (newcount == 0 && statusUpdate != null)
				statusUpdate(false);
			if (newcount < 0)
				Console.WriteLine("active count is now negative!");
				//throw new ArgumentOutOfRangeException();
		}
	}

	/* initiates one synchronization pass for every collection in the store.
	 * This is typically called from a timer or threadpool thread. Make sure
	 * to catch all exceptions and do something with them, or the exception
	 * winds out to the thread pool and is lost.
	 */
	private void Runner(Object obj)
	{
		StatusAccumulator(true);
		try
		{
			lock(this)
			{
				Trace.WriteLine(String.Format("Synker.Runner(): [Store: {0}, Host: {1}, Port: {2}]",
					storeLocation, myHostName, serverPort));

				Console.WriteLine("Synker.Runner: '{0}' {1}:{2}", storeLocation, myHostName, serverPort);

				// get list of all collections in this store
				Hashtable targets = SyncPoint.GetTargets(storeLocation);
				if (clients == null)
					clients = new Hashtable();

				/* kill any active clients for collections that are no
				 * longer in the target list
				 */
				ArrayList badClients = new ArrayList();
				foreach (DictionaryEntry client in clients)
					if (!targets.ContainsKey((string)client.Key))
					{
						((SyncClient)client.Value).Stop();
						badClients.Add((string)client.Key);
					}
				foreach (string badClient in badClients)
					clients.Remove(badClient);
				Console.WriteLine("removed {0} expired clients", badClients.Count);

				/* start or reset clients as needed for each target. Start
				 * or reset one server object if we are master of one or
				 * more collections.
				 */
				StatusUpdate statUpd = new StatusUpdate(StatusAccumulator);
				foreach (DictionaryEntry i in targets)
				{
					SyncPoint target = (SyncPoint)i.Value;
					//if (target.masterUri.Host.Equals("master"))
					if (target.masterUri.Host == myHostName
						&& target.masterUri.Port == serverPort)
					{
						if (server == null)
							server = new SyncServer();
						server.Start(statUpd, serverPort, storeLocation);
						continue;
					}
					string cid = target.collectionId;
					if (!clients.ContainsKey(cid))
						clients[cid] = new SyncClient();
					((SyncClient)clients[cid]).Start(statUpd,
							target, storeLocation);
				}
			}
		}
		catch (Exception e)
		{
			Trace.WriteLine(String.Format("Synker.Runner(): [Exception: {0}]",
				e.Message));
			
			Console.WriteLine("Uncaught exception in Synker.Runner: {0}", e.Message);
			// TODO: rethrow?
		}
		finally
		{
			StatusAccumulator(false);
		}
	}
}

#endif
//===========================================================================
}
