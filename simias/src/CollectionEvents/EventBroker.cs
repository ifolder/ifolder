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

	#endregion

	#region EventBroker class

	/// <summary>
	/// Class used to broker events to the subscribed clients.
	/// </summary>
	public class EventBroker : MarshalByRefObject
	{
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
		
		#endregion

		#region Event Signalers

		/// <summary>
		/// Used to publish a Collection root changed event.
		/// </summary>
		/// <param name="args">The arguments of the event.</param>
		[OneWay]
		public void RaiseCollectionRootChangedEvent(CollectionRootChangedEventArgs args)
		{
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
						System.Diagnostics.Debug.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
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
					eHandler = NodeCreated;
					break;
				case NodeEventArgs.EventType.Changed:
					eHandler = NodeChanged;
					break;
				case NodeEventArgs.EventType.Deleted:
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
						System.Diagnostics.Debug.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
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
					eHandler = FileCreated;
					break;
				case FileEventArgs.EventType.Changed:
					eHandler = FileChanged;
					break;
				case FileEventArgs.EventType.Deleted:
					eHandler = FileDeleted;
					break;
				case FileEventArgs.EventType.Renamed:
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
								System.Diagnostics.Debug.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
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
						System.Diagnostics.Debug.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
					}
				}
			}
		}

		/// <summary>
		/// Used to publish a service control event to listening service.
		/// </summary>
		/// <param name="t"></param>
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
						System.Diagnostics.Debug.WriteLine(new System.Diagnostics.StackFrame().GetMethod() + ": Listener removed");
					}
				}
			}
		}

		#endregion

		#region statics

		private const string CFG_Section = "EventService";
		private const string CFG_AssemblyKey = "Assembly";
		private const string CFG_Assembly = "CsEventBroker";
		private const string CFG_UriKey = "Uri";
		private const string CFG_Uri = "tcp://localhost:7654/EventBroker";
		
		public static void RegisterClientChannel()
		{
			string service = new Simias.Configuration().Get(CFG_Section, CFG_AssemblyKey, CFG_Assembly);
			Process [] process = Process.GetProcessesByName(service);
			if (process.Length >= 1)
			{				
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
					string serviceUri = new Simias.Configuration().Get(CFG_Section, CFG_UriKey, CFG_Uri);
					RemotingConfiguration.RegisterWellKnownClientType(typeof(EventBroker), serviceUri);
				}
			}
		}

		public static void RegisterService()
		{
			Uri serviceUri = new Uri (new Simias.Configuration().Get(CFG_Section, CFG_UriKey, CFG_Uri));
			
			Hashtable props = new Hashtable();
			props["port"] = serviceUri.Port;

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
