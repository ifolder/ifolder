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
using System.Threading;
using Simias;
using Simias.Event;
using Simias.Client.Event;


namespace Simias.Sync
{
	#region Delegate Definitions.

	/// <summary>
	/// Delegate definition for handling file events.
	/// </summary>
	public delegate void FileSyncEventHandler(FileSyncEventArgs args);

	/// <summary>
	/// Used to get around a marshalling problem seen with explorer.
	/// </summary>
	public delegate void CollectionSyncEventHandler(CollectionSyncEventArgs args);

	
	#endregion
	
	/// <summary>
	/// Class to Subscibe to collection events.
	/// </summary>
	public class SyncEventSubscriber : IDisposable
	{
		#region Events

		/// <summary>
		/// Event to recieve collection sync event.
		/// </summary>
		public event CollectionSyncEventHandler CollectionSync;
		/// <summary>
		/// Event to handle File sync events.
		/// </summary>
		public event FileSyncEventHandler FileSync;
		
		#endregion

		#region Private Fields

		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(SyncEventSubscriber));
		DefaultSubscriber	subscriber = null;
		bool		enabled;
		bool		alreadyDisposed;
		
		#endregion

		#region Constructor/Finalizer

		/// <summary>
		/// Creates a Subscriber to watch for sync events.
		/// </summary>
		public SyncEventSubscriber()
		{
			enabled = true;
			alreadyDisposed = false;
			subscriber = new DefaultSubscriber();
			subscriber.SimiasEvent += new SimiasEventHandler(OnSyncEvent);
		}

		
		/// <summary>
		/// Finalizer.
		/// </summary>
		~SyncEventSubscriber()
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

		#endregion

		#region Callbacks

		private void OnSyncEvent(SimiasEventArgs args)
		{
			try
			{
				if (enabled)
				{
					CollectionSyncEventArgs cArgs = args as CollectionSyncEventArgs;
					if (cArgs != null)
					{
						if (CollectionSync != null)
						{
							Delegate[] cbList = CollectionSync.GetInvocationList();
							foreach (CollectionSyncEventHandler cb in cbList)
							{
								try 
								{ 
									cb(cArgs);
								}
								catch(Exception ex)
								{
									logger.Debug(ex, "Delegate {0}.{1} failed", cb.Target, cb.Method);
									CollectionSync -= cb;
								}
							}
						}
					}
					else
					{
						FileSyncEventArgs fArgs = args as FileSyncEventArgs;
						if (fArgs != null)
						{
							if (FileSync != null)
							{
								Delegate[] cbList = FileSync.GetInvocationList();
								foreach (FileSyncEventHandler cb in cbList)
								{
									try 
									{ 
										cb(fArgs);
									}
									catch(Exception ex)
									{
										logger.Debug(ex, "Delegate {0}.{1} failed", cb.Target, cb.Method);
										FileSync -= cb;
									}
								}
							}
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

		private void Dispose(bool inFinalize)
		{
			try 
			{
				if (!alreadyDisposed)
				{
					alreadyDisposed = true;
					
					// Deregister delegates.
					subscriber.SimiasEvent -= new SimiasEventHandler(OnSyncEvent);
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
