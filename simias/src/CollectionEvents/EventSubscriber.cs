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

		#region Private Fields

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
		/// <param name="collectionId">The collection to watch for events.</param>
		/// <param name="rootPath">The Root Path for the collection.</param>
		public EventSubscriber(string collectionId, string rootPath)
		{
			enabled = true;
			fileNameFilter = null;
			fileTypeFilter = null;
			nodeIdFilter = null;
			nodeTypeFilter = null;
			nodeTypeRegex = null;
			this.collectionId = collectionId;
			this.rootPath = rootPath;
			alreadyDisposed = false;
			
			EventBroker.RegisterClientChannel();
			
			broker = new EventBroker();
			broker.NodeChanged += new NodeEventHandler(OnNodeChanged);
			broker.NodeCreated += new NodeEventHandler(OnNodeCreated);
			broker.NodeDeleted += new NodeEventHandler(OnNodeDeleted);
			broker.CollectionRootChanged += new CollectionEventHandler(OnCollectionRootChanged);
			broker.FileChanged += new FileEventHandler(OnFileChanged);
			broker.FileCreated += new FileEventHandler(OnFileCreated);
			broker.FileDeleted += new FileEventHandler(OnFileDeleted);
			broker.FileRenamed += new FileRenameEventHandler(OnFileRenamed);
			broker.ServiceControl += new ServiceEventHandler(OnServiceControl);
		}

		/// <summary>
		/// Create a Subscriber to monitor changes in the complete Collection Store.
		/// </summary>
		public EventSubscriber() :
			this((string)null, (string)null)
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
		[OneWay]
		public void OnNodeChanged(NodeEventArgs args)
		{
			callNodeDelegate(NodeChanged, args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Create events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		[OneWay]
		public void OnNodeCreated(NodeEventArgs args)
		{
			callNodeDelegate(NodeCreated, args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Delete events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		[OneWay]
		public void OnNodeDeleted(NodeEventArgs args)
		{
			callNodeDelegate(NodeDeleted, args);
		}

		/// <summary>
		/// Callback for Collection Root Change events.
		/// </summary>
		/// <param name="args"></param>
		[OneWay]
		public void OnCollectionRootChanged(CollectionRootChangedEventArgs args)
		{
			if (applyNodeFilter(args))
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
						}
					}
				}
			}
		}

		/// <summary>
		/// Callback used by the EventBroker for Change events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		[OneWay]
		public void OnFileChanged(FileEventArgs args)
		{
			callFileDelegate(FileChanged, args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Create events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		[OneWay]
		public void OnFileCreated(FileEventArgs args)
		{
			callFileDelegate(FileCreated, args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Delete events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		[OneWay]
		public void OnFileDeleted(FileEventArgs args)
		{
			callFileDelegate(FileDeleted, args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Rename events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		[OneWay]
		public void OnFileRenamed(FileRenameEventArgs args)
		{
			if (applyFileFilter(args))
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
						}
					}
				}
			}
		}

		/// <summary>
		/// Callback used to control services in the Simias System.
		/// </summary>
		/// <param name="targetProcess">The Id of the target process.</param>
		/// <param name="t">The control event type.</param>
		[OneWay]
		public void OnServiceControl(ServiceEventArgs args)
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
					}
				}
			}
		}

		#endregion

		#region Private Methods
		private void callNodeDelegate(NodeEventHandler eHandler, NodeEventArgs args)
		{
			if (applyNodeFilter(args))
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
							eHandler -= cb;
						}
					}
				}
			}
		}

		private void callFileDelegate(FileEventHandler eHandler, FileEventArgs args)
		{
			if (applyFileFilter(args))
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
							eHandler -= cb;
						}
					}
				}
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
					broker.NodeChanged -= new NodeEventHandler(OnNodeChanged);
					broker.NodeCreated -= new NodeEventHandler(OnNodeCreated);
					broker.NodeDeleted -= new NodeEventHandler(OnNodeDeleted);
					broker.CollectionRootChanged -= new CollectionEventHandler(OnCollectionRootChanged);
					broker.FileChanged -= new FileEventHandler(OnFileChanged);
					broker.FileCreated -= new FileEventHandler(OnFileCreated);
					broker.FileDeleted -= new FileEventHandler(OnFileDeleted);
					broker.FileRenamed -= new FileRenameEventHandler(OnFileRenamed);
					broker.ServiceControl -= new ServiceEventHandler(OnServiceControl);
					if (!inFinalize)
					{
						GC.SuppressFinalize(this);
					}
				}
			}
			catch {};
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
	}
}
