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
using System.IO;
using System.Threading;

using NUnit.Framework;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Location;
using Simias.Service;

namespace Simias.Location.Tests
{
	/// <summary>
	/// Location Tests
	/// </summary>
	[TestFixture]
	public class LocationTests : Assertion
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public LocationTests()
		{
		}

		/// <summary>
		/// Test the default locate functionality
		/// </summary>
		[Test]
		public void TestLocate()
		{
			// remove store
			string path = Path.GetFullPath("./location1");

			// clean-up
			if (Directory.Exists(path)) Directory.Delete(path, true);
			
			// configuration
			Configuration config = new Configuration(path);

			// create collection
			Store store = new Store(config);
			Collection collection = new Collection(store, "Location 1");
			collection.Commit();

			// locate collection
			LocationService service = new LocationService(config);

			Uri location = service.Locate(collection.ID);

			Assert("Location Not Found!", location != null);

			Console.WriteLine("Location: {0}", location);

			store.Dispose();
		}
		
		/// <summary>
		/// Test the Reunion locate functionality
		/// </summary>
		[Test]
		public void TestLocateReunion()
		{
			// remove store
			string path = Path.GetFullPath("./location2");

			// clean-up
			if (Directory.Exists(path)) Directory.Delete(path, true);
			
			// configuration
			Configuration config = new Configuration(path);

			// create collection
			Store store = new Store(config);
			Collection collection = new Collection(store, "Location 2");
			collection.Commit();

			// start services
			Manager manager = new Manager(config);

			manager.StartServices();

			// locate collection
			LocationService service = new LocationService(config);

			service.Register(collection.ID);

			Uri location = service.Locate(collection.ID);

			Assert("Location Not Found!", location != null);

			Console.WriteLine("Location: {0}", location);

			service.Unregister(collection.ID);

			store.Dispose();

			manager.StopServices();
		}
	}
}
