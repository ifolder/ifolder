/***********************************************************************
 *  EventSubscriber.cs - An event subscriber class.
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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Security.Permissions;
using System.Threading;


//[assembly:PermissionSetAttribute(SecurityAction.RequestMinimum, Name = "FullTrust")]
namespace Simias.Event
{
	/// <summary>
	/// Class to Subscibe to collection events.
	/// </summary>
	public class EventSubscriber : MarshalByRefObject, IDisposable
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
		
		#endregion

		#region Private Fields

		Queue eventQueue;
		ManualResetEvent queued;
		EventBroker broker;
		bool		enabled;
		Regex		fileNameFilter;
		Regex		fileTypeFilter;
		string		nodeIdFilter;
		string		nodeTypeFilter;
		Regex		nodeTypeRegex;
		string		collectionId;
		string		rootPath;
		bool		alreadyDisposed;
		
		#endregion

		#region Constructor/Finalizer

		/// <summary>
		/// Creates a Subscriber to watch the specified Collection.
		/// </summary>
		/// <param name="conf">Configuration Object.</param>
		/// <param name="collectionId">The collection to watch for events.</param>
		/// <param name="rootPath">The Root Path for the collection.</param>
		public EventSubscriber(Configuration conf, string collectionId, string rootPath)
		{
			eventQueue = new Queue();
			queued = new ManualResetEvent(false);

			enabled = true;
			fileNameFilter = null;
			fileTypeFilter = null;
			nodeIdFilter = null;
			nodeTypeFilter = null;
			nodeTypeRegex = null;
			this.collectionId = collectionId;
			this.rootPath = rootPath;
			alreadyDisposed = false;
			
			EventBroker.RegisterClientChannel(conf);
			
			broker = new EventBroker();
			broker.NodeChanged += new NodeEventHandler(OnNodeChanged);
			broker.NodeCreated += new NodeEventHandler(OnNodeCreated);
			broker.NodeDeleted += new NodeEventHandler(OnNodeDeleted);
			broker.CollectionRootChanged += new CollectionRootChangedHandler(OnCollectionRootChanged);
			broker.FileChanged += new FileEventHandler(OnFileChanged);
			broker.FileCreated += new FileEventHandler(OnFileCreated);
			broker.FileDeleted += new FileEventHandler(OnFileDeleted);
			broker.FileRenamed += new FileRenameEventHandler(OnFileRenamed);
			//broker.InternalEvent += new InternalEventHandler(broker_InternalEvent);
			System.Threading.Thread t = new Thread(new ThreadStart(EventThread));
			t.Start();
		}

		/// <summary>
		/// Create a Subscriber to monitor changes in the complete Collection Store.
		/// </summary>
		/// <param name="conf">Configuration object.</param>
		public EventSubscriber(Configuration conf) :
			this(conf, (string)null, (string)null)
		{
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~EventSubscriber()
		{
			Dispose(true);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets and set the enabled state.
		/// </summary>
		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				enabled = value;
			}
		}

		/// <summary>
		/// Gets and Sets the NameFilter.
		/// </summary>
		public string FileNameFilter
		{
			get
			{
				return fileNameFilter.ToString();
			}
			set
			{
				if (value != null)
					fileNameFilter = new Regex(value);
				else
					fileNameFilter = null;
			}
		}

		/// <summary>
		/// Gets and Sets the Type Filter.
		/// </summary>
		public string FileTypeFilter
		{
			get
			{
				return fileTypeFilter.ToString();
			}
			set
			{
				if (value != null)
                    fileTypeFilter = new Regex(value);
				else
					fileTypeFilter = null;
			}
		}

		/// <summary>
		/// Gets and Sets the Node ID filter for collection events.
		/// </summary>
		public string NodeIDFilter
		{
			get {return nodeIdFilter;}
			set {nodeIdFilter = value;}
		}

		/// <summary>
		/// Gets and sets the Node type filter for collection events.
		/// </summary>
		public string NodeTypeFilter
		{
			get {return nodeTypeFilter;}
			set 
			{
				if (value != null)
				{
					nodeTypeFilter = value;
					nodeTypeRegex = new Regex(@"\.*" + nodeTypeFilter);
				}
				else
				{
					nodeTypeFilter = null;
					nodeTypeRegex = null;
				}
			}
		}

		/// <summary>
		/// Gets and Sets the collection to filter on.
		/// </summary>
		public string CollectionId
		{
			get
			{
				return collectionId;
			}
			set
			{
				if (value != null)
					collectionId = value;
				else
					collectionId = null;
			}
		}

		#endregion

		#region Callbacks

		/// <summary>
		/// Callback used by the EventBroker for Change events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		//[OneWay]
		public void OnNodeChanged(NodeEventArgs args)
		{
			if (applyNodeFilter(args))
				queueEvent(args);
			//callNodeDelegate(NodeChanged, args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Create events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		//[OneWay]
		public void OnNodeCreated(NodeEventArgs args)
		{
			if (applyNodeFilter(args))
				queueEvent(args);
			//callNodeDelegate(NodeCreated, args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Delete events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		//[OneWay]
		public void OnNodeDeleted(NodeEventArgs args)
		{
			if (applyNodeFilter(args))
				queueEvent(args);
			//callNodeDelegate(NodeDeleted, args);
		}

		/// <summary>
		/// Callback for Collection Root Change events.
		/// </summary>
		/// <param name="args"></param>
		//[OneWay]
		public void OnCollectionRootChanged(CollectionRootChangedEventArgs args)
		{
			if (applyNodeFilter(args))
				queueEvent(args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Change events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		//[OneWay]
		public void OnFileChanged(FileEventArgs args)
		{
			if (applyFileFilter(args))
				queueEvent(args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Create events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		//[OneWay]
		public void OnFileCreated(FileEventArgs args)
		{
			if (applyFileFilter(args))
				queueEvent(args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Delete events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		//[OneWay]
		public void OnFileDeleted(FileEventArgs args)
		{
			if (applyFileFilter(args))
				queueEvent(args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Rename events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		//[OneWay]
		public void OnFileRenamed(FileRenameEventArgs args)
		{
			if (applyFileFilter(args))
				queueEvent(args);
		}

		#endregion

		#region Private Methods

		private void queueEvent(CollectionEventArgs args)
		{
			lock (eventQueue)
			{
				eventQueue.Enqueue(args);
				queued.Set();
			}
		}
		
		/// <summary>
		/// Called to apply the subscribers filter.
		/// </summary>
		/// <param name="args">The arguments supplied with the event.</param>
		/// <returns>True If matches the filter. False no match.</returns>
		private bool applyNodeFilter(NodeEventArgs args)
		{
			if (enabled)
			{
				if (collectionId == null || args.Collection == collectionId)
				{
					if (this.nodeIdFilter == null || nodeIdFilter == args.Node)
					{
						if (nodeTypeFilter == null || nodeTypeRegex.IsMatch(args.Type))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Called to apply the subscribers filter.
		/// </summary>
		/// <param name="args">The arguments supplied with the event.</param>
		/// <returns>True If matches the filter. False no match.</returns>
		private bool applyFileFilter(FileEventArgs args)
		{
			if (enabled)
			{
				if (collectionId == null || args.Collection == collectionId)
				{
					if (fileNameFilter == null || fileNameFilter.IsMatch(args.Name))
					{
						if (fileTypeFilter == null || fileTypeFilter.IsMatch(args.Type))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private void Dispose(bool inFinalize)
		{
			try 
			{
				if (!alreadyDisposed)
				{
					alreadyDisposed = true;
					// Signal thread so it can exit.
					queued.Set();

					// Deregister delegates.
					broker.NodeChanged -= new NodeEventHandler(OnNodeChanged);
					broker.NodeCreated -= new NodeEventHandler(OnNodeCreated);
					broker.NodeDeleted -= new NodeEventHandler(OnNodeDeleted);
					broker.CollectionRootChanged -= new CollectionRootChangedHandler(OnCollectionRootChanged);
					broker.FileChanged -= new FileEventHandler(OnFileChanged);
					broker.FileCreated -= new FileEventHandler(OnFileCreated);
					broker.FileDeleted -= new FileEventHandler(OnFileDeleted);
					broker.FileRenamed -= new FileRenameEventHandler(OnFileRenamed);
					if (!inFinalize)
					{
						GC.SuppressFinalize(this);
					}
				}
			}
			catch {};
		}

		private void EventThread()
		{
			while (!alreadyDisposed)
			{
				try
				{
					queued.WaitOne();
					lock (eventQueue)
					{
						if (eventQueue.Count > 0)
						{
							CollectionEventArgs args = (CollectionEventArgs)eventQueue.Dequeue();

							switch (args.ChangeType)
							{
								case EventType.NodeCreated:
									NodeCreated((NodeEventArgs)args);
									break;
								case EventType.NodeDeleted:
									NodeDeleted((NodeEventArgs)args);
									break;
								case EventType.NodeChanged:
									NodeChanged((NodeEventArgs)args);
									break;
								case EventType.CollectionRootChanged:
									CollectionRootChanged((CollectionRootChangedEventArgs)args);
									break;
								case EventType.FileCreated:
									FileCreated((FileEventArgs)args);
									break;
								case EventType.FileDeleted:
									FileDeleted((FileEventArgs)args);
									break;
								case EventType.FileChanged:
									FileChanged((FileEventArgs)args);
									break;
								case EventType.FileRenamed:
									FileRenamed((FileRenameEventArgs)args);
									break;
							}
						}
						else
						{
							queued.Reset();
						}
					}
				}
				catch {}
			}
		}

		#endregion

		#region MarshalByRefObject overrides

		/// <summary>
		/// This object should not time out.
		/// </summary>
		/// <returns></returns>
		public override Object InitializeLifetimeService()
		{
			return null;
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Called to cleanup any resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		#endregion

		public void broker_InternalEvent(EventType type, string args)
		{
			Console.WriteLine(args);
		}
	}
}
