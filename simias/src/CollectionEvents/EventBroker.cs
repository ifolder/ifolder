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


namespace Simias
{
	#region Delegate Definitions.

	/// <summary>
	/// Delegate definition for handling collection events.
	/// </summary>
	public delegate void EventHandler(EventArgs args);

	/// <summary>
	/// Delegate definition for rename events.
	/// </summary>
	public delegate void RenameEventHandelr(EventArgs oldArgs, EventArgs newArgs);

	/// <summary>
	/// Delegate definition for hanling service control events.
	/// </summary>
	public delegate void ServiceEventHandler(int targetProcess, ServiceEventType e);

	#endregion

	#region ServiceEventType enum

	/// <summary>
	/// Service Events.
	/// </summary>
	public enum ServiceEventType
	{
		/// <summary>
		/// The service should shutdown.
		/// </summary>
		Shutdown = 1,
		/// <summary>
		/// The service should reconfigure.
		/// </summary>
		Reconfigure = 2
	};

	#endregion

	#region CollectionEventArgs.

	/// <summary>
	/// The event arguments for a Collection event.
	/// </summary>
	[Serializable]
	public class EventArgs
	{
		string node;
		string path;
		string type;
		string source;
		string oldNode;
		string oldPath;
		
		/// <summary>
		/// Constructs a CollectionEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="node">The object of the event.</param>
		/// <param name="path">The path of the object.</param>
		/// <param name="type">The Type of the Node.</param>
		public EventArgs(string source, string node, string path, string type)
		{
			this.source = source;
			this.node = node;
			this.path = path;
			this.type = type;
			this.oldNode = null;
			this.oldPath = null;
		}

		/// <summary>
		/// Constructs a CollectionEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="node">The object of the event.</param>
		/// <param name="path">The path of the object.</param>
		/// <param name="type">The Type of the Node.</param>
		public EventArgs(string source, string node, string path, string oldNode, string oldPath, string type) :
			this(source, node, path, type)
		{
			this.oldNode = oldNode;
			this.oldPath = oldPath;
		}

		/// <summary>
		/// Gets the ID of the affected Node/Collection.
		/// </summary>
		public string Node
		{
			get {return node;}
		}
		
		/// <summary>
		/// Gets the ID of the containing Collection.
		/// </summary>
		public string Path
		{
			get {return path;}
		}

		/// <summary>
		/// Gets the Type of the affected Node.
		/// </summary>
		public string Type
		{
			get {return type;}
		}

		/// <summary>
		/// Gets the Old Node if event is rename.
		/// </summary>
		public string OldNode
		{
			get {return oldNode;}
		}

		/// <summary>
		/// Gets the old path if the event is a rename.
		/// </summary>
		public string OldPath
		{
			get {return oldPath;}
		}
	}
	
	#endregion

	#region EventBroker class

	/// <summary>
	/// Class used to broker events to the subscribed clients.
	/// </summary>
	public class EventBroker : MarshalByRefObject
	{
		#region Events

		/// <summary>
		/// Delegate to handle Collection Changes.
		/// A Node or Collection modification.
		/// </summary>
		public event EventHandler Changed;
		/// <summary>
		/// Delegate to handle Collection Creations.
		/// A Node or Collection has been created.
		/// </summary>
		public event EventHandler Created;
		/// <summary>
		/// Delegate to handle Collection Deletions.
		/// A Node or Collection has been deleted.
		/// </summary>
		public event EventHandler Deleted;
		/// <summary>
		/// Delegate to handle Collection Renames.
		/// A Node or Collection has been renamed.
		/// </summary>
		public event EventHandler Renamed;

		/// <summary>
		/// Delegate used to control services in the system.
		/// </summary>
		public event ServiceEventHandler ServiceControl;

		
		#endregion

		#region Event Signalers

		/// <summary>
		/// Used to publish a Collection change event.
		/// </summary>
		/// <param name="args">The arguments of the event.</param>
		[OneWay]
		public void FireChanged(EventArgs args)
		{
			callDelegate(Changed, args);
		}

		/// <summary>
		/// Used to publish a Collection create event.
		/// </summary>
		/// <param name="args">The arguments of the event.</param>
		[OneWay]
		public void FireCreated(EventArgs args)
		{
			callDelegate(Created, args);
		}

		/// <summary>
		/// Used to publish a Collection delete event.
		/// </summary>
		/// <param name="args">The arguments of the event.</param>
		[OneWay]
		public void FireDeleted(EventArgs args)
		{
			callDelegate(Deleted, args);
		}

		/// <summary>
		/// Used to publish a Collection rename event.
		/// </summary>
		/// <param name="args">The arguments of the event.</param>
		[OneWay]
		public void FireRenamed(EventArgs args)
		{
			callDelegate(Renamed, args);
		}

		/// <summary>
		/// Used to publish a service control event to listening service.
		/// </summary>
		/// <param name="t"></param>
		[OneWay]
		public void FireServiceControl(int targetProcess, ServiceEventType t)
		{
			if (ServiceControl != null)
			{
				Delegate[] cbList = ServiceControl.GetInvocationList();
				foreach (ServiceEventHandler cb in cbList)
				{
					try 
					{ 
						cb(targetProcess, t);
					}
					catch 
					{
						// Remove the offending delegate.
						ServiceControl -= cb;
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

		#region Private Event Invocation

		/// <summary>
		/// Used to call the delegate.  This is used instead of just calling the
		/// delegate directly so that on exceptions the rest of the delegates will
		/// still be called.  If an exception occurrs the offending delegate is
		/// removed from the list.
		/// </summary>
		/// <param name="eHandler">The delegates to call.</param>
		/// <param name="args">The args to pass.</param>
		private void callDelegate(EventHandler eHandler, EventArgs args)
		{
			if (eHandler != null)
			{
				Delegate[] cbList = eHandler.GetInvocationList();
				foreach (EventHandler cb in cbList)
				{
					try 
					{ 
						cb(args);
					}
					catch 
					{
						// Remove the offending delegate.
						eHandler -= cb;
					}
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
}
