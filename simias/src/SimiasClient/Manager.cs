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

namespace Simias.Client
{
	/// <summary>
	/// System Manager
	/// </summary>
	public class Manager
	{
		#region Class Members
		/// <summary>
		/// XML configuation tags.
		/// </summary>
		private const string CFG_Section = "ServiceManager";
		private const string CFG_Services = "Services";
		private const string CFG_WebServicePath = "WebServicePath";
		private const string CFG_WebServiceUri = "WebServiceUri";
		private const string CFG_WebServicePort = "WebServicePort";
		#endregion

		#region Properties
		/// <summary>
		/// Gets the path to the web service directory. Returns a null if the web service path
		/// does not exist.
		/// </summary>
		static public string LocalServicePath
		{
			get
			{
				Configuration config = new Configuration();
				return config.Get(CFG_Section, CFG_WebServicePath);
			}
		}

		/// <summary>
		/// Gets the port number to talk to the web service on. Returns a -1 if the web service port
		/// does not exist.
		/// </summary>
		static public int LocalServicePort
		{
			get
			{
				Configuration config = new Configuration();
				string portString = config.Get(CFG_Section, CFG_WebServicePort);
				return (portString != null) ? Convert.ToInt32(portString) : -1;
			}
		}

		/// <summary>
		/// Gets the local service url so that applications can talk to the local webservice.
		/// Returns a null if the local service url does not exist.
		/// </summary>
		static public Uri LocalServiceUrl
		{
			get
			{
				Configuration config = new Configuration();
				string uriString = config.Get(CFG_Section, CFG_WebServiceUri);
				return (uriString != null) ? new Uri(uriString) : null;
			}
		}
		#endregion
	}
}
