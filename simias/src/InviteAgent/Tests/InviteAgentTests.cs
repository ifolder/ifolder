/***********************************************************************
 *  $RCSfile$
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
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.IO;

using NUnit.Framework;

using Simias;
using Simias.Sync;
using Simias.Agent;
using Simias.Storage;
using Novell.AddressBook;

namespace Simias.Agent.Tests
{
	/// <summary>
	/// Agent Tests
	/// </summary>
	[TestFixture]
	public class AgentTests
	{
		string testStorePath;
		Uri testStoreUri;
		int id = 0;
		Store store;
		Collection collection;

		/// <summary>
		/// The default constructor
		/// </summary>
		public AgentTests()
		{
			MyTrace.SendTraceToStandardOutput();
		}

		/// <summary>
		/// Set up the test case
		/// </summary>
		[SetUp]
		public void SetUp()
		{
			// the test store
			testStorePath = Path.GetFullPath(".agent" + id++);
			testStoreUri = new Uri(testStorePath);

			// clear any stale store
			if (Directory.Exists(testStorePath))
			{
				Directory.Delete(testStorePath, true);
			}

			// store
			store = Store.Connect(testStoreUri);

			// create collection
			collection = store.CreateCollection("Agent Collection");
			collection.Properties.AddProperty(SyncCollection.HostPropertyName, "localhost");
			collection.Properties.AddProperty(SyncCollection.PortPropertyName, SyncProperties._DefaultPort);
			collection.Commit(true);
		}

		/// <summary>
		/// Tear down the test case
		/// </summary>
		[TearDown]
		public void TearDown()
		{
			// remove collection
			collection.Delete(true);

			// kludge for the store provider
			GC.Collect();

			// remove store
			store.ImpersonateUser(Access.StoreAdminRole, null);
			store.Delete();
			store = null;
		}

		/// <summary>
		/// Test the creating of an invitation
		/// </summary>
		[Test]
		public void TestInvite()
		{
			// user
			Novell.AddressBook.Manager abManager = Novell.AddressBook.Manager.Connect(testStoreUri);
			
			Novell.AddressBook.AddressBook ab = abManager.OpenDefaultAddressBook();

			Contact owner = ab.GetContactByIdentity(collection.Owner);
			owner.UserName = "JDoe";
			Name oName = new Name("John", "Doe");
			oName.Preferred = true;
			owner.AddName(oName);
			owner.EMail = "denali@novell.com";
			owner.Commit();

			Contact guest = new Contact();
			guest.UserName = "JDoe";
			ab.AddContact(guest);
			Name gName = new Name("John", "Doe");
			gName.Preferred = true;
			guest.AddName(gName);
			guest.EMail = "denali@novell.com";
			guest.Commit();

			// rights
			collection.SetUserAccess(guest.Identity, Access.Rights.ReadWrite);
			collection.Commit(true);

			// invite
			AgentFactory factory = new AgentFactory(testStorePath);
			factory.GetInviteAgent().Invite(guest.Identity, collection);
		}

		/// <summary>
		/// Test the accepting of the invitation
		/// </summary>
		[Test]
		public void TestAccept()
		{
			Invitation invitation = new Invitation();

			invitation.CollectionId = "9876543210";
			invitation.CollectionName = "Team Folder";
			invitation.Identity = "1234567890";
			invitation.Domain = "novell";
			invitation.MasterHost = "192.168.2.1";
			invitation.MasterPort = "6437";
			invitation.CollectionRights = Access.Rights.ReadWrite.ToString();
			invitation.Message = "Our Team's New Collection";

			invitation.FromName = "John Doe";
			invitation.FromEmail = "denali@novell.com";
			invitation.ToName = "Denali";
			invitation.ToEmail = "denali@novell.com";

			AgentFactory factory = new AgentFactory(testStorePath);
			factory.GetInviteAgent().Accept(invitation);
		}

		/// <summary>
		/// Test the creating of a bad invitation
		/// </summary>
		[Test]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void TestBadInvite1()
		{
			// user
			Novell.AddressBook.Manager abManager = Novell.AddressBook.Manager.Connect(testStoreUri);
			
			Novell.AddressBook.AddressBook ab = abManager.OpenDefaultAddressBook();

			Contact owner = ab.GetContactByIdentity(collection.Owner);
			owner.UserName = "JDoe";
			Name oName = new Name("John", "Doe");
			oName.Preferred = true;
			owner.AddName(oName);
			// MAKE BAD:
			owner.EMail = "";
			owner.Commit();

			Contact guest = new Contact();
			guest.UserName = "JDoe";
			ab.AddContact(guest);
			Name gName = new Name("John", "Doe");
			gName.Preferred = true;
			guest.AddName(gName);
			guest.EMail = "denali@novell.com";
			guest.Commit();

			// rights
			collection.SetUserAccess(guest.Identity, Access.Rights.ReadWrite);
			collection.Commit(true);

			// invite
			AgentFactory factory = new AgentFactory(testStorePath);
			factory.GetInviteAgent().Invite(guest.Identity, collection);
		}
		
		/// <summary>
		/// Test the creating of a bad invitation
		/// </summary>
		[Test]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void TestBadInvite2()
		{
			// user
			Novell.AddressBook.Manager abManager = Novell.AddressBook.Manager.Connect(testStoreUri);
			
			Novell.AddressBook.AddressBook ab = abManager.OpenDefaultAddressBook();

			Contact owner = ab.GetContactByIdentity(collection.Owner);
			owner.UserName = "JDoe";
			Name oName = new Name("John", "Doe");
			oName.Preferred = true;
			owner.AddName(oName);
			owner.EMail = "denali@novell.com";
			owner.Commit();

			Contact guest = new Contact();
			guest.UserName = "JDoe1";
			ab.AddContact(guest);
			Name gName = new Name("John", "Doe");
			gName.Preferred = true;
			guest.AddName(gName);
			// MAKE BAD: guest.EMail = "denali@novell.com";
			guest.Commit();

			// rights
			collection.SetUserAccess(guest.Identity, Access.Rights.ReadWrite);
			collection.Commit(true);

			// invite
			AgentFactory factory = new AgentFactory(testStorePath);
			factory.GetInviteAgent().Invite(guest.Identity, collection);
		}
	}
}
