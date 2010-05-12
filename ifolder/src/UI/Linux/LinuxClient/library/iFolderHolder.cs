/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
  *
  *                 $Author: Calvin Gaisford <cgaisford@novell.com>
  *                          Boyd Timothy <btimothy@novell.com>
  *                 $Modified by: <Modifier>
  *                 $Mod Date: <Date Modified>
  *                 $Revision: 0.0
  *-----------------------------------------------------------------------------
  * This module is used to:
  *        <Description of the functionality of the file >
  *
  *
  *******************************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Text;
using Gtk;

using Simias.Client;
using Simias.Client.Event;

using Novell.iFolder.Events;
using Novell.iFolder.Controller;
//using NetworkManager;


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
		Disconnected,

		/// <summary>
		/// Passphrase not provided. 
		/// </summary>
		NoPassphrase,

		///<summary>
		/// Network Cable Disconnected.
		/// </summmary>
//		NetworkCableDisc,
		
		RevertAndDelete	
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
		public uint			objectsToSync;
		
//		private static bool unitTested = false;

		public iFolderHolder(iFolderWeb ifolder)
		{
			this.ifolder		= ifolder;
			this.state			= iFolderState.Initial;
			this.objectsToSync	= 0;
			UpdateDisplayData();
			
//			if (!unitTested)
//			{
//				UnitTest();
//				unitTested = true;
//			}
		}
		
/*
		private static void UnitTest()
		{
			Debug.PrintLine("============= Unit Testing GetFriendlyTime() ==============");
			Debug.PrintLine("\tDateTime.Now: {0}", GetFriendlyTime(DateTime.Now));
			
			DateTime dateTime = DateTime.Now.Subtract(TimeSpan.FromSeconds(30));
			string friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t30 Seconds Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromSeconds(59));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t59 Seconds Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromSeconds(60));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t60 Seconds Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromSeconds(61));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t61 Seconds Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t1 Minute Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(2));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t2 Minutes Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(30));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t30 Minutes Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(59));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t59 Minutes Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(60));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t60 Minutes Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(61));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t61 Minutes Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(62));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t62 Minutes Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(120));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t120 Minutes Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(121));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t121 Minutes Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(122));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t122 Minutes Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromDays(1));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t1 Day Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromDays(2));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t2 Days Ago: {0}", friendlyTime);
			
			dateTime = DateTime.Now.Subtract(TimeSpan.FromDays(5));
			friendlyTime = GetFriendlyTime(dateTime);
			Debug.PrintLine("\t5 Days Ago: {0}", friendlyTime);
		}
*/
		
        /// <summary>
        /// Constructor
        /// </summary>
		protected iFolderHolder()
		{
			this.ifolder		= null;
			this.state			= iFolderState.Initial;
			this.objectsToSync	= 0;
		}

        /// <summary>
        /// Gets  / Sets iFolder
        /// </summary>
		public iFolderWeb iFolder
		{
			get{ return ifolder; }

			set
			{
				this.ifolder = value;
				UpdateDisplayData();
			}
		}

        /// <summary>
        /// Gets the Path
        /// </summary>
		public string Path
		{
			get{ return path; }
		}

        /// <summary>
        /// Gets the StateString
        /// </summary>
		public string StateString
		{
			get
			{
				UpdateDisplayData();
				return stateString;
			}
		}

        /// <summary>
        /// Gets / Sets the iFolder State
        /// </summary>
		public iFolderState State
		{
			get{ return state; }
			set
			{
				this.state = value;
				UpdateDisplayData();
			}
		}
		
        /// <summary>
        /// Gets / Sets the Objects to Sync
        /// </summary>
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

        /// <summary>
        /// UpdateDisplay Data
        /// </summary>
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
						case iFolderState.NoPassphrase:
							stateString = Util.GS("Passphrase not provided");
							break;
						case iFolderState.Disconnected:
							stateString = Util.GS("Server unavailable");
							break;
						case iFolderState.RevertAndDelete:
							stateString = Util.GS("Deletion In Prgoress");
							break;
/*						case iFolderState.NetworkCableDisc:
							stateString=Util.GS("Network unavailable");   	
							break; */
						case iFolderState.Normal:
						default:
							if (objectsToSync > 0)
								stateString = string.Format(Util.GS("{0} items not synchronized"), objectsToSync);
							else
							{
								string lastSyncTime = iFolder.LastSyncTime;
								if (lastSyncTime == null || lastSyncTime.Length == 0)
									stateString = Util.GS("OK");
								else
								{
									try
									{
										DateTime dateTime = 
											DateTime.Parse(lastSyncTime);
										
										string friendlyTime =
											GetFriendlyTime(dateTime);

										stateString =
											string.Format(
												Util.GS("Synchronized: {0}"),
												friendlyTime);
									}
									catch
									{
										stateString =
											string.Format(
												Util.GS("Synchronized: {0}"),
												lastSyncTime);
									}
								}
							}
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
		
        /// <summary>
        /// Get Friendly Time
        /// </summary>
        /// <param name="dateTime">Date Time</param>
        /// <returns>string - Time</returns>
		private static string GetFriendlyTime(DateTime dateTime)
		{
			DateTime now = DateTime.Now;
			
			TimeSpan span = now.Subtract(dateTime);
			
			// FIXME: Once we move into hours, fix this up so we know when to say, "Yesterday"/etc.
			if (span.TotalSeconds < 60)
				return Util.GS("Less than a minute ago");
			
			int totalMinutes = (int)span.TotalMinutes;
			if (totalMinutes == 1)
				return Util.GS("1 minute ago");
			
			if (totalMinutes < 60)
				return string.Format(Util.GS("{0} minutes ago"),
									   totalMinutes);

			int lastSyncDay		= dateTime.DayOfYear;
			int nowDay			= now.DayOfYear;
			
			if (lastSyncDay == nowDay)
			{
				// The last synchronization happened TODAY!
				if (span.Minutes == 0)
				{
					if (span.Hours == 1)
						return Util.GS("1 hour ago");
					else
						return string.Format(
							Util.GS("{0} hours ago"),
							span.Hours);
				}
				else if (span.Minutes == 1)
				{
					if (span.Hours == 1)
						return Util.GS("1 hour, 1 minute ago");
					else
						return string.Format(
							Util.GS("{0} hours, 1 minute ago"),
							span.Hours);
				}
				else
				{
					if (span.Hours == 1)
						return string.Format(
							Util.GS("1 hour, {0} minutes ago"),
							span.Minutes);
					else
						return string.Format(
							Util.GS("{0} hour, {1} minutes ago"),
							span.Hours,
							span.Minutes);
				}
			}
			else if ((nowDay - lastSyncDay) == 1)
			{
				// The last sync happened YESTERDAY!
				return Util.GS("Yesterday");
			}
			else
				return dateTime.ToShortDateString();
		}
	}
	
//	public class DummyiFolderHolder : iFolderHolder
//	{
//		public DummyiFolderHolder()
//		{
//		}
//	}
}
