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

using Simias;
using Simias.Storage;
using Simias.Invite;
using Simias.Sync;

namespace Simias.Invite.Tests
{
	/// <summary>
	/// Invitation Tests
	/// </summary>
	[TestFixture]
	public class InvitationTests
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(InvitationTests));

		string testStorePath;
		Uri testStoreUri;
		int id = 0;
		Store store;
		Collection collection;

		/// <summary>
		/// The default constructor
		/// </summary>
		public InvitationTests()
		{
			MyTrace.SendToConsole();
		}

		/// <summary>
		/// Set up the test case
		/// </summary>
		[SetUp]
		public void SetUp()
		{
			// the test store
			testStorePath = Path.GetFullPath(".invitation" + id++);
			testStoreUri = new Uri(testStorePath);

			// clear any stale store
			if (Directory.Exists(testStorePath))
			{
				Directory.Delete(testStorePath, true);
			}

			// store
			store = Store.Connect(testStoreUri, null);

			// create collection
			collection = store.CreateCollection("Invitation Collection");
			collection.Properties.AddProperty(SyncCollection.HostPropertyName, "localhost");
			collection.Properties.AddProperty(SyncCollection.PortPropertyName, SyncProperties.SuggestedPort);
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
			store.ImpersonateUser(Access.StoreAdminRole);
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
			Invitation invitation = InvitationService.CreateInvitation(collection,
				collection.LocalStore.CurrentUser);

			invitation.FromName = "JDoe";
			invitation.FromEmail = "denali@novell.com";

			invitation.ToName = "John Doe";
			invitation.ToEmail = "denali@novell.com";

			// invite
			InvitationService.Invite(invitation);
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

			InvitationService.Accept(store, invitation);
		}

		/// <summary>
		/// Test the creating of a bad invitation
		/// </summary>
		[Test]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void TestBadInvite1()
		{
			// user
			Invitation invitation = InvitationService.CreateInvitation(collection,
				collection.LocalStore.CurrentUser);

			invitation.FromName = "JDoe";
			// BAD: invitation.FromEmail = "denali@novell.com";

			invitation.ToName = "John Doe";
			invitation.ToEmail = "denali@novell.com";

			// invite
			InvitationService.Invite(invitation);
		}
		
		/// <summary>
		/// Test the creating of a bad invitation
		/// </summary>
		[Test]
		[ExpectedException(typeof(System.ArgumentNullException))]
		public void TestBadInvite2()
		{
			// user
			Invitation invitation = InvitationService.CreateInvitation(collection,
				collection.LocalStore.CurrentUser);

			invitation.FromName = "JDoe";
			invitation.FromEmail = "denali@novell.com";

			invitation.ToName = "John Doe";
			// BAD: invitation.ToEmail = "denali@novell.com";

			// invite
			InvitationService.Invite(invitation);
		}
	}
}
