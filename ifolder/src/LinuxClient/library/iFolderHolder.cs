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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Authors:
 *		Calvin Gaisford <cgaisford@novell.com>
 *		Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/


using System;
using System.IO;
using System.Collections;
using System.Text;
using Gtk;

using Simias.Client;
using Simias.Client.Event;

using Novell.iFolder.Events;
using Novell.iFolder.Controller;

namespace Novell.iFolder
{
	/// <summary>
	/// iFolder states.
	/// </summary>
	public enum iFolderState
	{
		/// <summary>
		/// Initial state before anything has happened
		/// </summary>
		Initial,

		/// <summary>
		/// The Normal state.
		/// </summary>
		Normal,

		/// <summary>
		/// The Synchronizing state.
		/// </summary>
		Synchronizing,

		/// <summary>
		/// The FailedSync state.
		/// </summary>
		FailedSync,

		/// <summary>
		/// Synchronizing with the local store.
		/// </summary>
		SynchronizingLocal,

		/// <summary>
		/// Unable to connect to the server.
		/// </summary>
		Disconnected
	}
	
	/// <summary>
	/// This is a holder class for iFolders so the client can place
	/// extra data with an iFolder about it's status and such.
	/// </summary>
	public class iFolderHolder
	{
		private iFolderWeb		ifolder;
		private iFolderState	state;
		private string			stateString;
		private string			path;
		private uint			objectsToSync;

		public iFolderHolder(iFolderWeb ifolder)
		{
			this.ifolder	= ifolder;
			state			= iFolderState.Initial;
			objectsToSync	= 0;
			UpdateDisplayData();
		}

		public iFolderWeb iFolder
		{
			get{ return ifolder; }

			set
			{
				this.ifolder = value;
				UpdateDisplayData();
			}
		}

		public string Path
		{
			get{ return path; }
		}

		public string StateString
		{
			get{ return stateString; }
		}

		public iFolderState State
		{
			get{ return state; }
			set
			{
				this.state = value;
				UpdateDisplayData();
			}
		}
		
		public uint ObjectsToSync
		{
			get
			{
				return objectsToSync;
			}
			set
			{
				objectsToSync = value;
				UpdateDisplayData();
			}
		}

		private void UpdateDisplayData()
		{
			if (state == iFolderState.Synchronizing)
			{
				if (objectsToSync > 0)
					stateString = string.Format(Util.GS("{0} items to synchronize"), objectsToSync);
				else
					stateString = Util.GS("Synchronizing");
			}
			else if (state == iFolderState.SynchronizingLocal)
			{
				stateString = Util.GS("Checking for changes");
			}
			else
			{
				if (iFolder.HasConflicts)
				{
					stateString = Util.GS("Has conflicts");
				}
				else
				{
					switch (state)
					{
						case iFolderState.Initial:
							switch (iFolder.State)
							{
								case "Available":
									stateString = Util.GS("Not set up");
									break;
								case "WaitConnect":
									stateString = Util.GS("Waiting to connect");
									break;
								case "WaitSync":
								case "Local":
									stateString = Util.GS("Waiting to synchronize");
//									stateString = Util.GS("OK");
									break;
								default:
									stateString = Util.GS("Unknown");
									break;
							}
							break;
						case iFolderState.FailedSync:
							stateString = Util.GS("Incomplete synchronization");
							break;
						case iFolderState.Disconnected:
							stateString = Util.GS("Server unavailable");
							break;
						case iFolderState.Normal:
						default:
							if (objectsToSync > 0)
								stateString = string.Format(Util.GS("{0} items not synchronized"), objectsToSync);
							else
								stateString = Util.GS("OK");
							break;
					}
				}
			}

			if(iFolder.IsSubscription)
			{
				if(iFolder.State == "Available")
					path = iFolder.Owner;
			}
			else
			{
				path = iFolder.UnManagedPath;
			}
		}
	}
}