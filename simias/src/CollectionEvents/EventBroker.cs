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
	public delegate void CollectionEventHandler(CollectionRootChangedEventArgs args);

	/// <summary>
	/// Delegate definition for handling collection events.
	/// </summary>
	public delegate void NodeEventHandler(NodeEventArgs args);
	
	/// <summary>
	/// Delegate definition for file events.
	/// </summary>
	public delegate void FileEventHandler(FileEventArgs args);

	/// <summary>
	/// Delegate definition for rename events.
	/// </summary>
	public delegate void FileRenameEventHandler(FileRenameEventArgs args);

	/// <summary>
	/// Delegate definition for hanling service control events.
	/// </summary>
	public delegate void ServiceEventHandler(ServiceEventArgs args);

	/// <summary>
	/// Used to get around a marshalling problem seen with explorer.
	/// </summary>
	public delegate void InternalEventHandler(CollectionEventArgs.EventType type, string args);

	#endregion

	#region EventBroker class

	/// <summary>
	/// Class used to broker events to the subscribed clients.
	/// </summary>
	public class EventBroker : MarshalByRefObject
	{
		EventBroker instance = null;
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
		public event CollectionEventHandler CollectionRootChanged;
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

		/// <summary>
		/// Used to work around a marshalling problem seen with explorer.
		/// </summary>
		public event InternalEventHandler InternalEvent;

		#endregion

		#region Event Signalers

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventType"></param>
		/// <param name="args"></param>
		public void RaiseEvent(CollectionEventArgs.EventType eventType, CollectionEventArgs args)
		{
			MyTrace.WriteLine("Recieved Event {0}", eventType.ToString());
			if (InternalEvent != null)
			{
				string eventArgs = args.MarshallToString();
				foreach (InternalEventHandler cb in InternalEvent.GetInvocationList())
				{
					try
					{
						cb(eventType, eventArgs);
					}
					catch
					{
						InternalEvent -= cb;
						MyTrace.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
					}
				}
			}
		}

		/// <summary>
		/// Used to publish a Collection root changed event.
		/// </summary>
		/// <param name="args">The arguments of the event.</param>
		[OneWay]
		public void RaiseCollectionRootChangedEvent(CollectionRootChangedEventArgs args)
		{
			MyTrace.WriteLine("Recieved CollectionRootChangedEvent ID = {0}", args.ID);
			if (CollectionRootChanged != null)
			{
				Delegate[] cbList = CollectionRootChanged.GetInvocationList();
				foreach (CollectionEventHandler cb in cbList)
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

		/// <summary>
		/// Used to publish a Node event.
		/// </summary>
		/// <param name="args">The arguments of the event.</param>
		[OneWay]
		public void RaiseNodeEvent(NodeEventArgs args)
		{
			NodeEventHandler eHandler;
			switch (args.ChangeType)
			{
				case NodeEventArgs.EventType.Created:
					MyTrace.WriteLine("Received Node Create Event. ID = {0}", args.ID);
					eHandler = NodeCreated;
					break;
				case NodeEventArgs.EventType.Changed:
					MyTrace.WriteLine("Received Node Changed Event. ID = {0}", args.ID);
					eHandler = NodeChanged;
					break;
				case NodeEventArgs.EventType.Deleted:
					MyTrace.WriteLine("Received Node Delete Event. ID = {0}", args.ID);
					eHandler = NodeDeleted;
					break;
				default:
					eHandler = null;
					break;
			}

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
							case NodeEventArgs.EventType.Created:
								NodeCreated -= cb;
								break;
							case NodeEventArgs.EventType.Changed:
								NodeChanged -= cb;
								break;
							case NodeEventArgs.EventType.Deleted:
								NodeDeleted -= cb;
								break;
							default:
								break;
						}
						MyTrace.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
					}
				}
			}
		}

		

		/// <summary>
		/// Used to publish a File event.
		/// </summary>
		/// <param name="args">The arguments of the event.</param>
		[OneWay]
		public void RaiseFileEvent(FileEventArgs args)
		{
			FileEventHandler eHandler;
			switch (args.ChangeType)
			{
				case FileEventArgs.EventType.Created:
					MyTrace.WriteLine("Received File Create Event. Name = {0}", args.Name);
					eHandler = FileCreated;
					break;
				case FileEventArgs.EventType.Changed:
					MyTrace.WriteLine("Received File Changed Event. Name = {0}", args.Name);
					eHandler = FileChanged;
					break;
				case FileEventArgs.EventType.Deleted:
					MyTrace.WriteLine("Received File Delete Event. Name = {0}", args.Name);
					eHandler = FileDeleted;
					break;
				case FileEventArgs.EventType.Renamed:
					MyTrace.WriteLine("Received File Rename Event. OldName = {0} NewName = {1}", ((FileRenameEventArgs)args).OldName, args.Name);
					eHandler = null;
					if (FileRenamed != null)
					{
						Delegate[] cbList = FileRenamed.GetInvocationList();
						foreach (FileRenameEventHandler cb in cbList)
						{
							try 
							{ 
								cb((FileRenameEventArgs)args);
							}
							catch 
							{
								// Remove the offending delegate.
								FileRenamed -= cb;
								MyTrace.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
							}
						}
					}
					break;
				default:
					eHandler = null;
					break;
			}

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
							case FileEventArgs.EventType.Created:
								FileCreated -= cb;
								break;
							case FileEventArgs.EventType.Changed:
								FileChanged -= cb;
								break;
							case FileEventArgs.EventType.Deleted:
								FileDeleted -= cb;
								break;
						}
						MyTrace.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
					}
				}
			}
		}

		/// <summary>
		/// Used to publish a service control event to listening service.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		[OneWay]
		public void RaiseServiceEvent(ServiceEventArgs args)
		{
			if (ServiceControl != null)
			{
				Delegate[] cbList = ServiceControl.GetInvocationList();
				foreach (ServiceEventHandler cb in cbList)
				{
					try 
					{ 
						cb(args);
					}
					catch 
					{
						// Remove the offending delegate.
						ServiceControl -= cb;
						MyTrace.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
					}
				}
			}
			if (args.Target == ServiceEventArgs.TargetAll && args.EventType == ServiceEventArgs.ServiceEvent.Shutdown)
			{
				System.Diagnostics.Process.GetCurrentProcess().Kill();
			}
		}

		#endregion

		#region statics

		private const string CFG_Section = "EventService";
		private const string CFG_AssemblyKey = "Assembly";
		private const string CFG_Assembly = "CsEventBroker";
		private const string CFG_UriKey = "Uri";
		private const string CFG_Uri = "tcp://localhost/EventBroker";

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

		/// <summary>
		/// Method to register the server channel.
		/// </summary>
		public static void RegisterService(Configuration conf)
		{
			string serviceString = CFG_Uri + "_" + conf.BasePath.GetHashCode().ToString();
			Uri serviceUri = new Uri (serviceString);
			
			Hashtable props = new Hashtable();
			props["port"] = 0; //serviceUri.Port;
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

			string [] s = chan.GetUrlsForUri(serviceUri.AbsolutePath.TrimStart('/'));
			if (s.Length == 1)
			{
				conf.Set(CFG_Section, CFG_UriKey, s[0]);
			}
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
				System.Threading.Thread.Sleep(500);
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
}
