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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

//
// Source file for registering multi-cast DNS services
//

using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels;

namespace Mono.P2p.mDnsResponder
{
	/// <summary>
	/// Summary description for ServiceRegistrationmDnsHost
	/// </summary>'
	public class ServiceRegistration : MarshalByRefObject
	{
		#region Class Members

		/*
		static internal ArrayList	hosts = new ArrayList();
		static internal Mutex		hostsMtx = new Mutex(false);
		*/
		#endregion

		#region Properties
		#endregion

		#region Constructors
		public ServiceRegistration()
		{
		}
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		public int	RegisterServiceLocation(string host, string serviceName, short port)
		{
			return(0);
		}
		
		public int	DeregisterServiceLocation(string host, string serviceName)
		{
			return(0);
		}

		public int	RegisterPointer(string domain, string target)
		{
			return(0);
		}
		
		public int	DeregisterPointer(string domain, string target)
		{
			return(0);
		}
		
		#endregion

		#region Public Methods
		#endregion
	}
	
	/// <summary>
	/// Summary description for ServiceRegistrationmDnsHost
	/// </summary>'
	public class Registration
	{
		#region Class Members
		#endregion

		#region Properties
		#endregion

		#region Constructors
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		public static int Startup()
		{
			HttpChannel chnl = new HttpChannel(6151);
			
			ChannelServices.RegisterChannel(chnl);
			
			RemotingConfiguration.RegisterWellKnownServiceType(
				typeof(ServiceRegistration),
				"ServiceRegistration.soap",
				WellKnownObjectMode.SingleCall);
				
			return(0);
		}
		
		public static int Shtudown()
		{
			return(0);
		}

		#endregion

		#region Public Methods
		#endregion
	}
	
}
