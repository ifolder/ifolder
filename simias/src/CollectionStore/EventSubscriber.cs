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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Event;


//[assembly:PermissionSetAttribute(SecurityAction.RequestMinimum, Name = "FullTrust")]
namespace Simias.Storage
{
	#region Delegate Definitions.

	/// <summary>
	/// Delegate definition for handling collection events.
	/// </summary>
	public delegate void NodeEventHandler(NodeEventArgs args);

	/// <summary>
	/// Used to get around a marshalling problem seen with explorer.
	/// </summary>
	internal delegate void TemporayEventHandler(EventType type, string args);

	
	#endregion
	
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
		
		#endregion

		#region Private Fields

		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(EventSubscriber));
		DefaultSubscriber	subscriber = null;
		bool		enabled;
		Regex		fileNameFilter;
		Regex		fileTypeFilter;
		string		nodeIdFilter;
		string		nodeTypeFilter;
		Regex		nodeTypeRegex;
		string		collectionId;
		bool		alreadyDisposed;
		
		#endregion

		#region Constructor/Finalizer

		/// <summary>
		/// Creates a Subscriber to watch the specified Collection.
		/// </summary>
		/// <param name="collectionId">The collection to watch for events.</param>
		public EventSubscriber(string collectionId)
		{
			enabled = true;
			fileNameFilter = null;
			fileTypeFilter = null;
			nodeIdFilter = null;
			nodeTypeFilter = null;
			nodeTypeRegex = null;
			this.collectionId = collectionId;
			alreadyDisposed = false;
			
			subscriber = new DefaultSubscriber();
			subscriber.SimiasEvent += new SimiasEventHandler(OnCollectionEvent);
		}

		/// <summary>
		/// Create a Subscriber to monitor changes in the complete Collection Store.
		/// </summary>
		public EventSubscriber() :
			this((string)null)
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

		private void OnCollectionEvent(SimiasEventArgs args)
		{
			try
			{
				NodeEventArgs nodeArgs = args as NodeEventArgs;
				if (nodeArgs != null)
				{
					if (applyNodeFilter(nodeArgs))
					{
						EventType changeType = (EventType)Enum.Parse(typeof(EventType), (string)nodeArgs.EventData, false);
						switch (changeType)
						{
							case EventType.NodeChanged:
								if (NodeChanged != null)
								{
									Delegate[] cbList = NodeChanged.GetInvocationList();
									foreach (NodeEventHandler cb in cbList)
									{
										try 
										{ 
											cb(nodeArgs);
										}
										catch(Exception ex)
										{
											logger.Debug(ex, "Delegate {0}.{1} failed", cb.Target, cb.Method);
											NodeChanged -= cb;
										}
									}
								}
								break;
							case EventType.NodeCreated:
								if (NodeCreated != null)
								{
									Delegate[] cbList = NodeCreated.GetInvocationList();
									foreach (NodeEventHandler cb in cbList)
									{
										try 
										{ 
											cb(nodeArgs);
										}
										catch(Exception ex)
										{
											logger.Debug(ex, "Delegate {0}.{1} failed", cb.Target, cb.Method);
											NodeCreated -= cb;
										}
									}
								}
								break;
							case EventType.NodeDeleted:
								if (NodeDeleted != null)
								{
									Delegate[] cbList = NodeDeleted.GetInvocationList();
									foreach (NodeEventHandler cb in cbList)
									{
										try 
										{ 
											cb(nodeArgs);
										}
										catch(Exception ex)
										{
											logger.Debug(ex, "Delegate {0}.{1} failed", cb.Target, cb.Method);
											NodeDeleted -= cb;
										}
									}
								}
								break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				new SimiasException(args.ToString(), ex);
			}
		}

		#endregion

		#region Private Methods

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

		private void Dispose(bool inFinalize)
		{
			try 
			{
				if (!alreadyDisposed)
				{
					alreadyDisposed = true;
					
					// Deregister delegates.
					subscriber.SimiasEvent -= new SimiasEventHandler(OnCollectionEvent);
					subscriber.Dispose();
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
