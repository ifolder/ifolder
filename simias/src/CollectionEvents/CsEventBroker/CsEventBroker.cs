/***********************************************************************
 *  CsEventBroker.cs - A service to boker events.
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
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using Simias;

namespace Simias.Event
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class CsEventBroker
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			string mutexName;
			string confPath;
			if (args.Length != 2)
			{
				return;
			}
			else
			{
				confPath = args[0];
				mutexName = args[1];
			}

			ManualResetEvent shutdownEvent = new ManualResetEvent(false);
			EventBroker.RegisterService(shutdownEvent, new Configuration(args[0]));
			bool createdMutex;
			Mutex mutex = new Mutex(true, mutexName, out createdMutex);
			if (!createdMutex)
			{
				mutex.WaitOne();
			}
			// Wait (forever) until we are killed.
			shutdownEvent.WaitOne();
			mutex.Close();
		}
	}
}
