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

using Simias;
using Simias.Storage;

namespace Simias.Location
{
	/// <summary>
	/// Location Service
	/// </summary>
	public class LocationService
	{
		private static readonly string section = "Location";
		private static readonly string key = "Providers";

		private Configuration configuration;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="configuration">The simias configuration file.</param>
		public LocationService(Configuration configuration)
		{
			this.configuration = configuration;
		}

		/// <summary>
		/// Locate the collection master.
		/// </summary>
		/// <param name="collection">The collection id.</param>
		/// <returns>A URI object containing the location of the collection master, or null.</returns>
		/// <remarks>The first non-null URI from the provider list is always returned.</remarks>
		public Uri Locate(string collection)
		{
			Uri result = null;

			LocationProviderList providers = new LocationProviderList(configuration);

			foreach(Type type in providers)
			{
				try
				{
					MyTrace.WriteLine("Type: {0}", type);

					ILocationProvider provider = (ILocationProvider)Activator.CreateInstance(type);

					provider.Configure(configuration);

					result = provider.Locate(collection);
				}
				catch(Exception e)
				{
					MyTrace.WriteLine(e);
				}

				if (result != null) break;
			}

			return result;
		}
	}
}
