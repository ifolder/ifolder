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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Diagnostics;

namespace Simias.Client
{
	/// <summary>
	/// My Environment
	/// </summary>
	public class MyEnvironment
	{
		private static MyPlatformID platformID;
		private static MyRuntimeID runtimeID;

		/// <summary>
		/// Static Constructor
		/// </summary>
		static MyEnvironment()
		{
			SetRuntimeID();
			SetPlatformID();
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		private MyEnvironment()
		{
		}

		/// <summary>
		/// The MyPlatformID value of the current operating system on the local machine.
		/// </summary>
		public static MyPlatformID Platform
		{
			get { return platformID; }
		}

		/// <summary>
		/// The MyRuntimeID value of the current common runtime on the local machine.
		/// </summary>
		public static MyRuntimeID Runtime
		{
			get { return runtimeID; }
		}

		/// <summary>
		/// Is the current common runtime Mono?
		/// </summary>
		public static bool Mono
		{
			get { return runtimeID == MyRuntimeID.Mono; }
		}

		/// <summary>
		/// Is the current common runtime .Net?
		/// </summary>
		public static bool DotNet
		{
			get { return runtimeID == MyRuntimeID.DotNet; }
		}

		/// <summary>
		/// Is the current operating system Unix?
		/// </summary>
		public static bool Unix
		{
			get { return platformID == MyPlatformID.Unix; }
		}

		/// <summary>
		/// Is the current operating system Windows?
		/// </summary>
		public static bool Windows
		{
			get { return platformID == MyPlatformID.Windows; }
		}

		/// <summary>
		/// Discover and set the runtime ID
		/// </summary>
		private static void SetRuntimeID()
		{
			if (Type.GetType("Mono.Runtime", false) != null)
			{
				// Mono
				runtimeID = MyRuntimeID.Mono;
			}
			else
			{
				// assume DotNet
				runtimeID = MyRuntimeID.DotNet;
			}
		}

		/// <summary>
		/// Discover and set the platform ID
		/// </summary>
		private static void SetPlatformID()
		{
			if (Mono && ((int)Environment.OSVersion.Platform == 128))
			{
				// Unix
				platformID = MyPlatformID.Unix;
			}
			else
			{
				// Windows
				platformID = MyPlatformID.Windows;
			}
		}
	}
}
