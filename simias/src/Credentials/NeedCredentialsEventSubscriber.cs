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

using Simias.Event;
using Simias.Client.Event;

namespace Simias.src.Common
{
	#region Delegate Definitions.

	/// <summary>
	/// Delegate definition for handling need credentials events.
	/// </summary>
	public delegate void NeedCredentialsEventHandler(NeedCredentialsEventArgs args);

	#endregion

	/// <summary>
	/// Summary description for NeedCredentialsEventSubscriber.
	/// </summary>
	public class NeedCredentialsEventSubscriber
	{
		#region Events

		/// <summary>
		/// Event to recieve Need Credentials event.
		/// </summary>
		public event NeedCredentialsEventHandler NeedCredentials;
		
		#endregion

		#region Private Fields

		DefaultSubscriber	subscriber = null;
		bool		enabled;
		bool		alreadyDisposed;
		
		#endregion

		#region Constructor/Finalizer


		public NeedCredentialsEventSubscriber()
		{
			enabled = true;
			alreadyDisposed = false;
			subscriber = new DefaultSubscriber();
			subscriber.SimiasEvent += new SimiasEventHandler(OnNeedCredentials);
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~NeedCredentialsEventSubscriber()
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

		private void OnNeedCredentials(SimiasEventArgs args)
		{
			try
			{
				if (enabled)
				{
					NeedCredentialsEventArgs cArgs = args as NeedCredentialsEventArgs;
					if (cArgs != null)
					{
						if (NeedCredentials != null)
						{
							Delegate[] cbList = NeedCredentials.GetInvocationList();
							foreach (NeedCredentialsEventHandler cb in cbList)
							{
								try 
								{ 
									cb(cArgs);
								}
								catch
								{
									NeedCredentials -= cb;
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
					subscriber.SimiasEvent -= new SimiasEventHandler(OnNeedCredentials);
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
