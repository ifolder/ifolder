/***********************************************************************
 *  WorkGroupIdentityTests.cs - Implements unit tests for the 
 *  WorkGroupIdentity assembly.
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
 *  Author: Mike Lasky <mlasky@novell.com>
 * 
 ***********************************************************************/

using System;
using NUnit.Framework;
using Simias.Identity;

namespace Simias.Identity.Tests
{
	/// <summary>
	/// Test cases for Iteration 0 stories.
	/// </summary>
	[TestFixture]
	public class WorkGroupIdentityTests
	{
		#region Class Members
		private IIdentityFactory idFactory;
		private IIdentity identity;
		#endregion

		#region Test Setup
		/// <summary>
		/// Performs pre-initialization tasks.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			// Create a user that can be impersonated.
			idFactory = IdentityManager.Connect();
			identity = idFactory.Create( "cameron", "novell" );
		}
		#endregion

		#region Iteration Tests
		/// <summary>
		/// Connects to a Store and creates, commits and deletes a Collection.
		/// </summary>
		[Test]
		public void IdentityTests()
		{
			string userGuid = Guid.NewGuid().ToString();
			identity.SetKeyChainItem( "MyMachineName", userGuid, "novell" );

			string credential = idFactory.GetIdentityFromUserGuid( userGuid ).GetCredentialFromUserGuid( userGuid );
			Console.WriteLine( "Credential = " + credential );

			Console.WriteLine( "UserGuid   = " + identity.UserGuid );
			Console.WriteLine( "Domain     = " + identity.DomainName );
			Console.WriteLine( "Credential = " + identity.Credential );
			Console.WriteLine( "UserId     = " + identity.UserId );

			// Look for the alternate identity by it's domain name.
			userGuid = identity.GetUserGuidFromDomain( identity.DomainName );
			if ( userGuid == null )
			{
				throw new ApplicationException( "Cannot find user guid for domain " + identity.DomainName );
			}

			Console.WriteLine( "UserGuid for Domain {0} = {1}", identity.DomainName, userGuid );

			userGuid = identity.GetUserGuidFromDomain( "MyMachineName" );
			if ( userGuid == null )
			{
				throw new ApplicationException( "Cannot find user guid for domain " + "MyMachineName" );
			}

			Console.WriteLine( "UserGuid for Domain {0} = {1}", "MyMachineName", userGuid );
		}
		#endregion

		#region Test Clean Up
		/// <summary>
		/// Clean up for tests.
		/// </summary>
		[TestFixtureTearDown]
		public void Cleanup()
		{
		}
		#endregion
	}
}
