/***********************************************************************
 *  EventBroker.cs - Event broker class.
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
 *  Author: Russ Young <ryoung@novell.com>
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
		static ManualResetEvent shutdown;
		static bool serviceRegistered = false;

		#endregion

		#region Events

		/// <summary>
		/// 
		/// </summary>
		public event CollectionEventHandlerS CollectionEvent;

		/// <summary>
		/// 
		/// </summary>
		public event ServiceEventHandler	ServiceEvent;
	
		#endregion

		#region Constructor

		public EventBroker()
		{
			// Start a thread to handle events.
			eventQueue = new Queue();
			queued = new ManualResetEvent(false);

			if (serviceRegistered)
			{
				System.Threading.Thread t = new Thread(new ThreadStart(EventThread));
				t.IsBackground = true;
				t.Start();
			}
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
						
					if (CollectionEvent != null && args.ChangeType != EventType.ServiceControl)
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
					else
					{
						if (ServiceEvent != null)
						{
							Delegate[] cbList = ServiceEvent.GetInvocationList();
							foreach (ServiceEventHandler cb in cbList)
							{
								try 
								{ 
									cb((ServiceEventArgs)args);
								}
								catch 
								{
									// Remove the offending delegate.
									ServiceEvent -= cb;
									MyTrace.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
								}
							}
						}
						if (((ServiceEventArgs)args).ControlEvent == ServiceControl.Shutdown)
						{
							if (((ServiceEventArgs)args).Target == ServiceEventArgs.TargetAll || 
								((ServiceEventArgs)args).Target == System.Diagnostics.Process.GetCurrentProcess().Id)
							{
								shuttingDown = true;
							}
						}
					}
				}
				catch{}
			}
			shutdown.Set();
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
#if DynamicPort
		private const string CFG_Uri = "tcp://localhost/EventBroker";
#else
		private const string CFG_Uri = "tcp://localhost:7782/EventBroker";
#endif

		public static bool overrideConfig = false;

		static bool RunInProcess()
		{
			if (overrideConfig == true)
				return false;
			else
				return true;
		}
		
		/// <summary>
		/// Method to register a client channel.
		/// </summary>
		public static void RegisterClientChannel(Configuration conf)
		{
			lock (typeof( EventBroker))
			{
				// Check if we should run in process.
				if (!RunInProcess())
				{
					startService(conf);
					string serviceUri = conf.Get(CFG_Section, CFG_UriKey, CFG_Uri);
					bool registered = false;

					WellKnownClientTypeEntry [] cta = RemotingConfiguration.GetRegisteredWellKnownClientTypes();
					foreach (WellKnownClientTypeEntry ct in cta)
					{
						Type t = typeof(EventBroker);
						Type t1 = ct.ObjectType;
						if (Type.Equals(t, t1))
						{
							registered = true;
							break;
						}
					}
					if (!registered)
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

						//ChannelServices.RegisterChannel(new TcpChannel());
						RemotingConfiguration.RegisterWellKnownClientType(typeof(EventBroker), serviceUri);
					}
				}
			}
		}

		/// <summary>
		/// Method to register the server channel.
		/// </summary>
		public static void RegisterService(ManualResetEvent shutdownEvent, Configuration conf)
		{
			shutdown = shutdownEvent;
			string serviceString = CFG_Uri + "_" + conf.BasePath.GetHashCode().ToString();
			Uri serviceUri = new Uri (serviceString);
			
			Hashtable props = new Hashtable();
#if DynamicPort
			props["port"] = 0; //serviceUri.Port;
#else		
			props["port"] = serviceUri.Port;
#endif
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

			RemotingConfiguration.RegisterWellKnownServiceType(
				typeof(EventBroker), serviceUri.AbsolutePath.TrimStart('/'), WellKnownObjectMode.Singleton);

			string [] s;
#if DynamicPort
			s = chan.GetUrlsForUri(serviceUri.AbsolutePath.TrimStart('/'));
			if (s.Length == 1)
			{
				conf.Set(CFG_Section, CFG_UriKey, s[0]);
			}
#else
			conf.Set(CFG_Section, CFG_UriKey, serviceUri.ToString());
			Console.WriteLine(serviceUri.AbsolutePath);
#endif
			serviceRegistered = true;
		}
	
		private static void startService(Configuration conf)
		{
			bool createdMutex;
			string mutexName = "EventBrokerMutex___" + conf.BasePath.GetHashCode().ToString();
			Mutex mutex = new Mutex(true, mutexName, out createdMutex);
			if (createdMutex)
			{
				Uri assemblyPath = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase));
				string serviceName = conf.Get(CFG_Section, CFG_AssemblyKey, CFG_Assembly) + ".exe";
				serviceName = Path.Combine(assemblyPath.LocalPath, serviceName);
			
				// The service is not running start it.
				System.Diagnostics.Process service = new Process();
				service.StartInfo.CreateNoWindow = true;
				service.StartInfo.UseShellExecute = false;
				if (MyEnvironment.Mono)
				{
					service.StartInfo.FileName = "mono";
					service.StartInfo.Arguments = serviceName + " ";
				}
				else
				{
					service.StartInfo.FileName = serviceName;
					service.StartInfo.Arguments = null;
				}
				service.StartInfo.Arguments += Path.GetDirectoryName(conf.BasePath) + " " + mutexName;
				service.Start();
                mutex.ReleaseMutex();
				Thread.Sleep(100);
				while (mutex.WaitOne(0, false))
				{
					mutex.ReleaseMutex();
					Thread.Sleep(100);
					//Console.WriteLine("After Release = {0}", DateTime.Now.Ticks);
				}
			}
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

	#region InProcessEventBroker class

	internal class InProcessEventBroker : MarshalByRefObject, IDisposable
	{
		#region fields

		// This is a singleton per Store.
		static Hashtable	instanceTable = new Hashtable();
		int					count;
		Configuration		conf;
		Queue				eventQueue;
		ManualResetEvent	queued;
		EventBroker			broker;
		bool				alreadyDisposed;

		#endregion

		#region Events

		/// <summary>
		/// Delegate to handle Collection Creations.
		/// A Node or Collection has been created.
		/// </summary>
		public event NodeEventHandler NodeCreated;
		/// <summary>
		/// Delegate to handle Collection Deletions.
		/// A Node or Collection has been deleted.
		/// </summary>
		public event NodeEventHandler NodeDeleted;
		/// <summary>
		/// Delegate to handle Collection Changes.
		/// A Node or Collection modification.
		/// </summary>
		public event NodeEventHandler NodeChanged;
		/// <summary>
		/// Delegate to handle Collection Root Path changes.
		/// </summary>
		public event CollectionRootChangedHandler CollectionRootChanged;
		/// <summary>
		/// Delegate to handle File Creations.
		/// </summary>
		public event FileEventHandler FileCreated;
		/// <summary>
		/// Delegate to handle File Deletions.
		/// </summary>
		public event FileEventHandler FileDeleted;
		/// <summary>
		/// Delegate to handle Files Changes.
		/// </summary>
		public event FileEventHandler FileChanged;
		/// <summary>
		/// Delegate to handle File Renames.
		/// </summary>
		public event FileRenameEventHandler FileRenamed;
		/// <summary>
		/// Delegate used to control services in the system.
		/// </summary>
		public event ServiceEventHandler ServiceControl;

		#endregion

		#region Factory Methods

		internal static InProcessEventBroker GetSubscriberBroker(Configuration conf)
		{
			InProcessEventBroker instance;
			lock (typeof(InProcessEventBroker))
			{
				if (!instanceTable.Contains(conf.BasePath))
				{
					instance = new InProcessEventBroker(conf);
					instanceTable.Add(conf.BasePath, instance);
				}
				else
				{
					instance = (InProcessEventBroker)instanceTable[conf.BasePath];
				}
				++instance.count;
			}
			return instance;
		}

		
		#endregion

		#region Constructor / Finalizer

		InProcessEventBroker(Configuration conf)
		{
			alreadyDisposed = false;
			this.conf = conf;
			count = 0;
			EventBroker.RegisterClientChannel(conf);
			broker = new EventBroker();

			broker.CollectionEvent += new CollectionEventHandlerS(OnCollectionEventS);
			// Start a thread to handle events.
			eventQueue = new Queue();
			queued = new ManualResetEvent(false);
			System.Threading.Thread t = new Thread(new ThreadStart(EventThread));
			t.IsBackground = true;
			t.Start();
		}

		~InProcessEventBroker()
		{
			Dispose(true);
		}

		#endregion

		#region Publish Call.

		/// <summary>
		/// Called to publish a collection event.
		/// </summary>
		/// <param name="args"></param>
		internal void RaiseEvent(CollectionEventArgs args)
		{
			if (broker != null)
				broker.RaiseEvent(args);
		}

		#endregion

		#region Callbacks

		/// <summary>
		/// Callback used by the EventBroker for Collection events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		public void OnCollectionEvent(CollectionEventArgs args)
		{
			queueEvent(args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Collection events.
		/// </summary>
		/// <param name="changeType">The type of event.</param>
		/// <param name="args">Arguments for the event.</param>
		public void OnCollectionEventS(EventType changeType, string args)
		{
			CollectionEventArgs eArgs = null;
			switch (changeType)
			{
				case EventType.CollectionRootChanged:
					eArgs = new CollectionRootChangedEventArgs(args);
					break;
				case EventType.FileChanged:
				case EventType.FileCreated:
				case EventType.FileDeleted:
					eArgs = new FileEventArgs(args);
					break;
				case EventType.FileRenamed:
					eArgs = new FileRenameEventArgs(args);
					break;
				case EventType.NodeChanged:
				case EventType.NodeCreated:
				case EventType.NodeDeleted:
					eArgs = new NodeEventArgs(args);
					break;
			}
			if (eArgs != null)
				queueEvent(eArgs);
		}

		#endregion

		#region Queue and Thread Methods.

		private void queueEvent(CollectionEventArgs args)
		{
			lock (eventQueue)
			{
				eventQueue.Enqueue(args);
				queued.Set();
			}
		}

		private void EventThread()
		{
			while (!alreadyDisposed)
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

					switch (args.ChangeType)
					{
						case EventType.NodeCreated:
							callNodeDelegate(NodeCreated, (NodeEventArgs)args);
							break;
						case EventType.NodeDeleted:
							callNodeDelegate(NodeDeleted, (NodeEventArgs)args);
							break;
						case EventType.NodeChanged:
							callNodeDelegate(NodeChanged, (NodeEventArgs)args);
							break;
						case EventType.CollectionRootChanged:
							callCrcDelegate((CollectionRootChangedEventArgs)args);
							break;
						case EventType.FileCreated:
							callFileDelegate(FileCreated, (FileEventArgs)args);
							break;
						case EventType.FileDeleted:
							callFileDelegate(FileDeleted, (FileEventArgs)args);
							break;
						case EventType.FileChanged:
							callFileDelegate(FileChanged, (FileEventArgs)args);
							break;
						case EventType.FileRenamed:
							callFileRenDelegate((FileRenameEventArgs)args);
							break;
					}
				}
				catch {}
			}
		}

		#endregion

		#region Delegate Invokers

		void callNodeDelegate(NodeEventHandler eHandler, NodeEventArgs args)
		{
			if (eHandler != null)
			{
				Delegate[] cbList = eHandler.GetInvocationList();
				foreach (NodeEventHandler cb in cbList)
				{
					try 
					{ 
						cb(args);
					}
					catch 
					{
						// Remove the offending delegate.
						switch (args.ChangeType)
						{
							case EventType.NodeCreated:
								NodeCreated -= cb;
								break;
							case EventType.NodeDeleted:
								NodeDeleted -= cb;
								break;
							case EventType.NodeChanged:
								NodeChanged -= cb;
								break;
						}
						MyTrace.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
					}
				}
			}
		}

		void callCrcDelegate(CollectionRootChangedEventArgs args)
		{
			if (CollectionRootChanged != null)
			{
				Delegate[] cbList = CollectionRootChanged.GetInvocationList();
				foreach (CollectionRootChangedHandler cb in cbList)
				{
					try 
					{ 
						cb(args);
					}
					catch 
					{
						// Remove the offending delegate.
						CollectionRootChanged -= cb;
						MyTrace.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
					}
				}
			}
		}

		void callFileDelegate(FileEventHandler eHandler, FileEventArgs args)
		{
			if (eHandler != null)
			{
				Delegate[] cbList = eHandler.GetInvocationList();
				foreach (FileEventHandler cb in cbList)
				{
					try 
					{ 
						cb(args);
					}
					catch 
					{
						// Remove the offending delegate.
						switch (args.ChangeType)
						{
							case EventType.FileCreated:
								FileCreated -= cb;
								break;
							case EventType.FileDeleted:
								FileDeleted -= cb;
								break;
							case EventType.FileChanged:
								FileChanged -= cb;
								break;
						}
						MyTrace.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
					}
				}
			}
		}

		void callFileRenDelegate(FileRenameEventArgs args)
		{
			if (FileRenamed != null)
			{
				Delegate[] cbList = FileRenamed.GetInvocationList();
				foreach (FileRenameEventHandler cb in cbList)
				{
					try 
					{ 
						cb(args);
					}
					catch 
					{
						// Remove the offending delegate.
						FileRenamed -= cb;
						MyTrace.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
					}
				}
			}
		}

		#endregion

		#region private Dispose

		private void Dispose(bool inFinalize)
		{
			try 
			{
				if (!alreadyDisposed)
				{
					lock (typeof(InProcessEventBroker))
					{
						--count;
						if (count == 0)
						{
							instanceTable.Remove(conf);
							alreadyDisposed = true;
							broker.CollectionEvent -= new CollectionEventHandlerS(OnCollectionEventS);
							
							// Signal thread so it can exit.
							queued.Set();
							if (!inFinalize)
							{
								GC.SuppressFinalize(this);
							}
						}
					}
				}
			}
			catch {};
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}

	#endregion
}
