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
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Security.Permissions;
using System.Threading;


//[assembly:PermissionSetAttribute(SecurityAction.RequestMinimum, Name = "FullTrust")]
namespace Simias
{
	/// <summary>
	/// Class to Subscibe to collection events.
	/// </summary>
	public class EventSubscriber : MarshalByRefObject, IDisposable
	{
		#region Events

		/// <summary>
		/// Delegate for Change events.
		/// </summary>
		public event EventHandler Changed;
		/// <summary>
		/// Delegate for Create events
		/// </summary>
		public event EventHandler Created;
		/// <summary>
		/// Delegate for Delete events
		/// </summary>
		public event EventHandler Deleted;
		/// <summary>
		/// Delegate for Rename events.
		/// </summary>
		public event EventHandler Renamed;

		/// <summary>
		/// Delegate used to control services in the system.
		/// </summary>
		public event ServiceEventHandler ServiceControl;

		#endregion

		#region Private Fields

		EventBroker broker;
		bool		enabled;
		Regex		nameFilter;
		Regex		typeFilter;
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
			nameFilter = null;
			typeFilter = null;
			this.collectionId = collectionId;
			this.rootPath = rootPath;
			alreadyDisposed = false;
			
			EventBroker.RegisterClientChannel();
			
			broker = new EventBroker();
			broker.Changed += new EventHandler(OnChanged);
			broker.Created += new EventHandler(OnCreated);
			broker.Deleted += new EventHandler(OnDeleted);
			broker.Renamed += new EventHandler(OnRenamed);
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
		public string NameFilter
		{
			get
			{
				return nameFilter.ToString();
			}
			set
			{
				if (value != null)
					nameFilter = new Regex(value);
				else
					nameFilter = null;
			}
		}

		/// <summary>
		/// Gets and Sets the Type Filter.
		/// </summary>
		public string TypeFilter
		{
			get
			{
				return typeFilter.ToString();
			}
			set
			{
				if (value != null)
                    typeFilter = new Regex(value);
				else
					typeFilter = null;
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
		public void OnChanged(EventArgs args)
		{
			callDelegate(Changed, args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Create events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		public void OnCreated(EventArgs args)
		{
			callDelegate(Created, args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Delete events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		public void OnDeleted(EventArgs args)
		{
			callDelegate(Deleted, args);
		}

		/// <summary>
		/// Callback used by the EventBroker for Rename events.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		public void OnRenamed(EventArgs args)
		{
			callDelegate(Renamed, args);
		}

		/// <summary>
		/// Callback used to control services in the Simias System.
		/// </summary>
		/// <param name="targetProcess">The Id of the target process.</param>
		/// <param name="t">The control event type.</param>
		public void OnServiceControl(int targetProcess, ServiceEventType t)
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

		#region Private Methods
		private void callDelegate(EventHandler eHandler, EventArgs args)
		{
			if (applyFilter(args))
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
		}

		/// <summary>
		/// Called to apply the subscribers filter.
		/// </summary>
		/// <param name="args">The arguments supplied with the event.</param>
		/// <returns>True If matches the filter. False no match.</returns>
		private bool applyFilter(EventArgs args)
		{
			if (enabled)
			{
				if (collectionId == null || args.Path == collectionId)
				{
					if (nameFilter == null || nameFilter.IsMatch(args.Node))
					{
						if (typeFilter == null || typeFilter.IsMatch(args.Type))
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
					broker.Changed -= new EventHandler(OnChanged);
					broker.Created -= new EventHandler(OnCreated);
					broker.Deleted -= new EventHandler(OnDeleted);
					broker.Renamed -= new EventHandler(OnRenamed);
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
