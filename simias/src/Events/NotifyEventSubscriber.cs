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
 *  Author: Mike Lasky
 *
 ***********************************************************************/
using System;

using Simias;
using Simias.Client.Event;


namespace Simias.Event
{
	#region Delegate Definitions.
	/// <summary>
	/// Delegate definition for handling notify events.
	/// </summary>
	public delegate void NotifyEventHandler( NotifyEventArgs args );
	#endregion
	
	/// <summary>
	/// Class to Subscibe to notify events.
	/// </summary>
	public class NotifyEventSubscriber : IDisposable
	{
		#region Class Members
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(NotifyEventSubscriber));
		private DefaultSubscriber subscriber = null;
		private bool enabled;
		private bool alreadyDisposed;

		/// <summary>
		/// Event to recieve notify events.
		/// </summary>
		public event NotifyEventHandler NotifyEvent;
		#endregion

		#region Properties
		/// <summary>
		/// Gets and set the enabled state.
		/// </summary>
		public bool Enabled
		{
			get { return enabled; }
			set	{ enabled = value; }
		}
		#endregion

		#region Constructor/Finalizer
		/// <summary>
		/// Creates a Subscriber to watch for notify events.
		/// </summary>
		public NotifyEventSubscriber()
		{
			enabled = true;
			alreadyDisposed = false;
			subscriber = new DefaultSubscriber();
			subscriber.SimiasEvent += new SimiasEventHandler( OnNotifyEvent );
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~NotifyEventSubscriber()
		{
			Dispose( true );
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Callback that is called when a Simias event is occurs.
		/// </summary>
		/// <param name="args">A SimiasEventArgs structure that contains event information.</param>
		private void OnNotifyEvent( SimiasEventArgs args )
		{
			try
			{
				if ( enabled )
				{
					NotifyEventArgs nArgs = args as NotifyEventArgs;
					if ( ( nArgs != null ) && ( NotifyEvent != null ) )
					{
						Delegate[] cbList = NotifyEvent.GetInvocationList();
						foreach ( NotifyEventHandler cb in cbList )
						{
							try 
							{ 
								cb( nArgs );
							}
							catch(Exception ex)
							{
								logger.Debug(ex, "Delegate {0}.{1} failed", cb.Target, cb.Method);
								NotifyEvent -= cb;
							}
						}
					}
				}
			}
			catch ( Exception ex )
			{
				new SimiasException( args.ToString(), ex );
			}
		}

		/// <summary>
		/// Dispose method.
		/// </summary>
		/// <param name="inFinalize">Set to true if called from Finalizer.</param>
		private void Dispose( bool inFinalize )
		{
			try 
			{
				if ( !alreadyDisposed )
				{
					alreadyDisposed = true;
					
					// Deregister delegates.
					subscriber.SimiasEvent -= new SimiasEventHandler( OnNotifyEvent );
					subscriber.Dispose();
					if ( !inFinalize )
					{
						GC.SuppressFinalize( this );
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
			Dispose( false );
		}
		#endregion
	}
}
