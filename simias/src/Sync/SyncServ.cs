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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections;
using System.Threading;
using System.Diagnostics;

namespace Simias.Sync
{

/*---------------------------------------------------------------------------
 * a delegate such that an object derived from Session object can
 * update the service object about it's status.
 * 100% complete means that the object can be disposed.
 */
public delegate void ServStatusUpdate(string userId, string collectionId,
		uint percentComplete);

/*---------------------------------------------------------------------------
 * objects derived from this class are remoted for a particular resource
 * and authenticated user.
 *
 * TODO: Security question -- could a user from another session object forge
 * the identity of this object and hijack my session?
 *
 * Does Session really get us anything? Why not just use Object
 * since the client will have to cast anyway.
 */
public class Session: MarshalByRefObject
{
	protected string userId, collectionId;
	protected ServStatusUpdate updater;

	public Session(string userId, string collectionId, ServStatusUpdate updater)
	{
		this.userId = userId;
		this.collectionId = collectionId;
		this.updater = updater;
	}

	public void Done()
	{
		if (updater != null)
			updater(userId, collectionId, 100);
	}

	public virtual string Version
	{
		get { return "0.0.0"; }
	}
}

//---------------------------------------------------------------------------
public delegate Session SessionFactory(Uri storeLocation, string userId,
		string credential, string collectionId, ServStatusUpdate updater);

//---------------------------------------------------------------------------
public class Service: MarshalByRefObject
{
	private Hashtable sessions = new Hashtable();
	private SessionFactory sessionFactory;
	private Uri storeLocation;

	public Service(SessionFactory sessionFactory, Uri storeLocation)
	{
		this.sessionFactory = sessionFactory;
		this.storeLocation = storeLocation;
	}

	internal void UpdateSessionStatus(string userId, string collectionId,
		uint percentComplete)
	{
		if (percentComplete == 100)
			lock (this)
			{
				sessions.Remove(userId + ":" + collectionId);
			}
	}

	public Session StartSession(string userId, string credential, string collectionId)
	{
		//if (!Identity.Authentic(userId, credential))
		//	return null;
		Session obj = null;
		string clientId = userId + ":" + collectionId;
		lock (this)
		{
			try
			{
				obj = sessionFactory(storeLocation, userId, credential,
						collectionId, new ServStatusUpdate(UpdateSessionStatus));
				sessions[clientId] = obj;
			}
			catch (Exception e)
			{
				Log.Error("Caught known exception {0}, {1}",
					e.Message, e.StackTrace);
			}
			catch
			{
				Log.Error( "Caught unknown exception" );
			}
		}
		return obj;
	}

	public string GetStatus()
	{
		lock (this)
		{
			Log.Spew("Service.GetStatus(): [Store: {0}]", storeLocation);
			return "status is " + sessions.Count;
		}
	}

	public string GetVersion()
	{
		return "0.0.1";
	}
	
}

//---------------------------------------------------------------------------
public class Server
{
	static int serverTotal;

	int port, running = 0, serverNumber;
	Service obj = null;
	IChannel channel = null;
	ObjRef objRef = null;
	SessionFactory sessionFactory;
	Uri storeLocation;
	string host;
	bool useTCP;

	const string serviceTag = "sync.rem";

	public static string MakeUri(string host, int port, bool useTCP)
	{
		return String.Format("{0}://{1}:{2}/{3}",
				(useTCP? "tcp": "http"), host, port, serviceTag);
	}

	public Server(string host, int port, Uri storeLocation, SessionFactory sessionFactory, bool useTCP)
	{
		this.port = port;
		this.storeLocation = storeLocation;
		this.sessionFactory = sessionFactory;
		this.host = host;
		this.useTCP = useTCP;
		serverNumber = Interlocked.Increment(ref serverTotal) - 1;
	}

	public bool Start()
	{
		if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
			return false;

		obj = new Service(sessionFactory, storeLocation);
		channel = useTCP? (IChannel)new TcpServerChannel("RTServer" + serverNumber, port):
				(IChannel)new HttpServerChannel("RTServer" + serverNumber, port);
		ChannelServices.RegisterChannel(channel);
		objRef = RemotingServices.Marshal(obj, serviceTag);

		Log.Info("Server {0} is up and running from store '{1}'",
				MakeUri(host, port, useTCP), storeLocation);
		return true;
	}

	public bool Stop()
	{
		Log.Spew("Server.Stop() {0}", MakeUri(host, port, useTCP));
		
		if (Interlocked.CompareExchange(ref running, 0, 1) != 1)
			return false;

		if (obj != null)
		{
			RemotingServices.Disconnect(obj);
			obj = null;
		}
		if (channel != null)
		{
			//channel.StopListening(null);
			ChannelServices.UnregisterChannel(channel);
			channel = null;
		}
		Log.Info("Stopped server {0}", serverNumber);
		return true;
	}
}

//---------------------------------------------------------------------------
public class Client
{
	static int clientTotal;
	Service service = null;
	IChannel channel = null;
	int clientNumber;
	public Session session = null;

	public Client(string host, int port, string userId, string credential, string collectionId, bool useTCP)
	{
		clientNumber = Interlocked.Increment(ref clientTotal) - 1;
		channel = useTCP? (IChannel)new TcpClientChannel("RTClient" + clientNumber, null):
				(IChannel)new HttpClientChannel("RTClient" + clientNumber, null);
		ChannelServices.RegisterChannel(channel);
		string serverURL = Server.MakeUri(host, port, useTCP);
		service = (Service)Activator.GetObject(typeof(Service), serverURL);
		session = service.StartSession(userId, credential, collectionId);
		Log.Spew("connected to server at {0}", serverURL);
	}

	public void Stop()
	{
		session.Done();
		session = null;
		service = null;
		if (channel != null)
		{
			ChannelServices.UnregisterChannel(channel);
			channel = null;
		}
	}
}

//===========================================================================
}
