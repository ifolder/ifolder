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
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using Simias;
using Simias.Service;

namespace Simias.Event
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class EventService : IThreadService //BaseProcessService
	{
		Configuration conf;
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		/*static void Main()
		{
			EventService service = new EventService();
			service.Run();
		}
*/
		public /*protected override*/ void Start(Configuration conf)
		{
			this.conf = conf;
			//EventBroker.RegisterService(GetConfiguration());
			EventBroker.RegisterService(conf);
		}

		public /*protected override*/ void Stop()
		{
			//EventBroker.DeRegisterService(GetConfiguration());
			EventBroker.DeRegisterService(conf);
		}

		public /*protected override*/ void Pause()
		{
		}

		public /*protected override*/ void Resume()
		{
		}

		public /*protected override*/ void Custom(int message, string data)
		{
		}
	}
}
