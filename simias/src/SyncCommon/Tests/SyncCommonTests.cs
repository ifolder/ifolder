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

using NUnit.Framework;

using Simias.Sync;

namespace Simias.Sync.Tests
{
	/// <summary>
	/// Sync Common Tests
	/// </summary>
	[TestFixture]
	public class SyncCommonTests : Assertion
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SyncCommonTests()
		{
		}

		/// <summary>
		/// Test channel sinks enumeration.
		/// </summary>
		[Test]
		public void TestSinksEnum()
		{
			string[] names = Enum.GetNames(typeof(SyncChannelSinks));

			foreach(string name in names)
			{
				Console.WriteLine("Sink Enum: {0}", name);
			}
		}

		/// <summary>
		/// Test the sync store class.
		/// </summary>
		[Test]
		public void TestSyncStore()
		{
			string path = "./syncstore1";

			SyncStore store = new SyncStore(path);

			Assert(store.StorePath == Path.GetFullPath(path));
			Assert(store.ID == store.BaseStore.GetDatabaseObject().Id);

			store.Delete();
		}

		/// <summary>
		/// Test the sync collection class.
		/// </summary>
		[Test]
		public void TestSyncCollection()
		{
			string storePath = "./syncstore2";
			string collectionPath = "./synccollection2";

			SyncStore store = new SyncStore(storePath);

			SyncCollection sc = store.CreateCollection("synccollection2", collectionPath);

			Assert(sc != null);

			Console.WriteLine("Collection \"{0}\" Domain: {1}", sc.Name, sc.Domain);
			
			Assert(sc.Domain != null);
			
			Console.WriteLine("Collection \"{0}\" Access Identity: {1}", sc.Name, sc.AccessIdentity);
			
			Console.WriteLine("Current Identity: {0}", store.BaseStore.CurrentUser);

			Assert(sc.AccessIdentity != null);

			store.Delete();
		}

		/// <summary>
		/// Test the invitation serialization
		/// </summary>
		[Test]
		public void TestInvitationSerialization()
		{
			string path = "invitation.ifi";

			// create invitaiton 1
			Invitation invitation1 = new Invitation();
			invitation1.CollectionId = "9876543210";
			invitation1.CollectionName = "Team Collection";
			invitation1.Identity = "1234567890";
			invitation1.Domain = "novell";
			invitation1.MasterUri = new Uri("http://192.168.2.1:6437");
			invitation1.CollectionRights = "ReadWrite";
			invitation1.Message = "Our Team's New Collection";
			invitation1.FromName = "John Doe";
			invitation1.FromEmail = "denali@novell.com";
			invitation1.ToName = "Denali";
			invitation1.ToEmail = "denali@novell.com";

			// serialize
			invitation1.Save(path);

			// deserialize
			Invitation invitation2 = new Invitation(path);

			// validate serialization
			Assert(invitation1.CollectionId == invitation2.CollectionId);
			Assert(invitation1.CollectionName == invitation2.CollectionName);
			Assert(invitation1.CollectionRights == invitation2.CollectionRights);
			Assert(invitation1.CollectionType == invitation2.CollectionType);
			Assert(invitation1.Domain == invitation2.Domain);
			Assert(invitation1.FromEmail == invitation2.FromEmail);
			Assert(invitation1.FromName == invitation2.FromName);
			Assert(invitation1.Identity == invitation2.Identity);
			Assert(invitation1.MasterUri.Host == invitation2.MasterUri.Host);
			Assert(invitation1.MasterUri.Port == invitation2.MasterUri.Port);
			Assert(invitation1.Message == invitation2.Message);
			Assert(invitation1.RootPath == invitation2.RootPath);
			Assert(invitation1.ToEmail == invitation2.ToEmail);
			Assert(invitation1.ToName == invitation2.ToName);
		}
	}
}
