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
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using Simias;


namespace Simias.Event
{
	#region Delegate Definitions.

	/// <summary>
	/// Delegate definition for handling collection events.
	/// </summary>
	public delegate void CollectionEventHandler(CollectionEventArgs args);

	/// <summary>
	/// This delegate is used temporarily until I figure out why explorer
	/// fails with the other handler.
	/// </summary>
	public delegate void CollectionEventHandlerS(EventType changeType, string args);
	
	#endregion

	#region EventBroker class

	/// <summary>
	/// Class used to broker events to the subscribed clients.
	/// </summary>
	public class EventBroker : MarshalByRefObject
	{
		#region Fields

		bool shuttingDown = false;
		Queue eventQueue;
		ManualResetEvent queued;
		static bool serviceRegistered = false;
		static bool clientRegistered = false;
		static EventBroker instance = null;

		#endregion

		#region Events

		/// <summary>
		/// 
		/// </summary>
		public event CollectionEventHandlerS CollectionEvent;
		
		#endregion

		#region Constructor

		public EventBroker()
		{
			// Start a thread to handle events.
			eventQueue = new Queue();
			queued = new ManualResetEvent(false);
			System.Threading.Thread t = new Thread(new ThreadStart(EventThread));
			t.IsBackground = true;
			t.Start();
		}

		#endregion

		#region EventThread

		private void EventThread()
		{
			while (!shuttingDown)
			{
				try
				{
					queued.WaitOne();
					CollectionEventArgs args = null;
					lock (eventQueue)
					{
						if (eventQueue.Count > 0)
						{
							args = (CollectionEventArgs)eventQueue.Dequeue();
						}
						else
						{
							queued.Reset();
							continue;
						}
					}
						
					if (CollectionEvent != null)
					{
						Delegate[] cbList = CollectionEvent.GetInvocationList();
						foreach (CollectionEventHandlerS cb in cbList)
						{
							try 
							{ 
								cb(args.ChangeType, args.MarshallToString());
							}
							catch 
							{
								// Remove the offending delegate.
								CollectionEvent -= cb;
								MyTrace.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
							}
						}
					}
				}
				catch{}
			}
		}

		#endregion

		#region Event Signalers

		/// <summary>
		/// Called to raise an event.
		/// </summary>
		/// <param name="args">The arguments for the event.</param>
		public void RaiseEvent(CollectionEventArgs args)
		{
			lock (eventQueue)
			{
				eventQueue.Enqueue(args);
				queued.Set();
			}
			//			MyTrace.WriteLine("Recieved Event {0}", args.ChangeType.ToString());
		}

		#endregion

		#region statics

		private const string CFG_Section = "EventService";
		private const string CFG_AssemblyKey = "Assembly";
		private const string CFG_Assembly = "CsEventBroker";
		private const string CFG_UriKey = "Uri";
		private const string CFG_Uri = "tcp://localhost/EventBroker";
		private const string channelName = "SimiasEventChannel";

		public static EventBroker GetBroker(Configuration conf)
		{
			if (instance != null)
			{
				return instance;
			}
			else
			{
				string serviceUri = conf.Get(CFG_Section, CFG_UriKey, CFG_Uri);
				if (new Uri(serviceUri).Port == -1)
				{
					return null;
				}
				EventBroker.RegisterClientChannel(conf);
				return (EventBroker)Activator.GetObject(typeof(EventBroker), serviceUri);
			}
		}

		/// <summary>
		/// Method to register a client channel.
		/// </summary>
		public static bool RegisterClientChannel(Configuration conf)
		{
			lock (typeof( EventBroker))
			{
				if (!clientRegistered)
				{
					Hashtable props = new Hashtable();
					props["port"] = 0;

					BinaryServerFormatterSinkProvider
						serverProvider = new BinaryServerFormatterSinkProvider();
					BinaryClientFormatterSinkProvider
						clientProvider = new BinaryClientFormatterSinkProvider();
#if !MONO
					serverProvider.TypeFilterLevel =
						System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
#endif
					TcpChannel chan = new
						TcpChannel(props,clientProvider,serverProvider);
					ChannelServices.RegisterChannel(chan);

						clientRegistered = true;
				}
			}
			return clientRegistered;
		}

		/// <summary>
		/// Method to register the server channel.
		/// </summary>
		public static void RegisterService(Configuration conf)
		{
            string serviceString = CFG_Uri + "_" + conf.StorePath.GetHashCode().ToString();
			Uri serviceUri = new Uri (serviceString);
			
			Hashtable props = new Hashtable();
			props["port"] = 0; //serviceUri.Port;
			props["name"] = channelName + conf.StorePath.GetHashCode().ToString();
			props["rejectRemoteRequests"] = true;

			BinaryServerFormatterSinkProvider
				serverProvider = new BinaryServerFormatterSinkProvider();
			BinaryClientFormatterSinkProvider
				clientProvider = new BinaryClientFormatterSinkProvider();
#if !MONO
			serverProvider.TypeFilterLevel =
				System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
#endif
			TcpChannel chan = new
				TcpChannel(props,clientProvider,serverProvider);
			ChannelServices.RegisterChannel(chan);

			
			instance = new EventBroker();
			ObjRef brokerRef = RemotingServices.Marshal(instance, serviceUri.AbsolutePath.TrimStart('/'));
			
			string [] uriList = chan.GetUrlsForUri(serviceUri.AbsolutePath.TrimStart('/'));
			if (uriList.Length == 1)
			{
				string service = uriList[0];
				Uri sUri = new Uri(service);
				if (!sUri.IsLoopback)
				{
					service = sUri.Scheme + Uri.SchemeDelimiter + "localhost:" + sUri.Port + sUri.AbsolutePath;
				}
				serviceRegistered = true;
				clientRegistered = true;
				conf.Set(CFG_Section, CFG_UriKey, service);
			}
		}

		/// <summary>
		/// Method to deregister the server channel.
		/// </summary>
		public static void DeRegisterService(Configuration conf)
		{
			if (instance != null)
			{
				RemotingServices.Disconnect(instance);
				instance = null;
			}
			ChannelServices.UnregisterChannel(ChannelServices.GetChannel(channelName + conf.StorePath.GetHashCode().ToString()));
			serviceRegistered = false;
		}
	
		#endregion
		
		#region MarshallByRef Overrides

		/// <summary>
		/// This will be used as a singleton do not expire the object.
		/// </summary>
		/// <returns>null (Do not expire object).</returns>
		public override Object InitializeLifetimeService()
		{
			return null;
		}

		#endregion
	}

	#endregion
}
