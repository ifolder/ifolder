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
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

using System;

using Simias;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Storage.Tests
{
	/// <summary>
	/// Class used to test the FindMember methods.
	/// </summary>
	public class FindMemberTests
	{
		#region Class Members
		private const int MemberCount = 11;
		private Store store;
		#endregion

		#region Properties
		private string NewID
		{
			get { return Guid.NewGuid().ToString().ToLower(); }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="store">Handle to the store.</param>
		public FindMemberTests( Store store )
		{
			this.store = store;
		}
		#endregion

		#region Private Methods
		private Domain AddEnterpriseMembers( Domain domain )
		{
			Node[] nodeList = new Node[ MemberCount - 1 ];

			nodeList[ 0 ] = new Member( "ecombs", NewID, Access.Rights.ReadWrite, "Earle", "Combs" );
			nodeList[ 1 ] = new Member( "mkoenig", NewID, Access.Rights.ReadWrite, "Mark", "Koenig" );
			nodeList[ 2 ] = new Member( "bruth", NewID, Access.Rights.ReadWrite, "Babe", "Ruth" );
			nodeList[ 3 ] = new Member( "lgehrig", NewID, Access.Rights.ReadWrite, "Lou", "Gehrig" );
			nodeList[ 4 ] = new Member( "bmeusel", NewID, Access.Rights.ReadWrite, "Bob", "Meusel" );
			nodeList[ 5 ] = new Member( "tlazzeri", NewID, Access.Rights.ReadWrite, "Tony", "Lazzeri" );
			nodeList[ 6 ] = new Member( "ldurocher", NewID, Access.Rights.ReadWrite, "Leo", "Durocher" );
			nodeList[ 7 ] = new Member( "jgrabowski", NewID, Access.Rights.ReadWrite, "Johnny", "Grabowski" );
			nodeList[ 8 ] = new Member( "bbengough", NewID, Access.Rights.ReadWrite, "Benny", "Bengough" );
			nodeList[ 9 ] = new Member( "bdickey", NewID, Access.Rights.ReadWrite, "Bill", "Dickey" );

			domain.Commit( nodeList );
			return domain;
		}

		private void CheckMemberOrder( Member[] list )
		{
			Member previous = null;
			foreach ( Member m in list )
			{
				if ( previous != null )
				{
					if ( String.Compare( previous.FN, m.FN ) > 0 ) throw new ApplicationException( "Member list is not in sorted order." );
				}

				previous = m;
			}
		}

		private Domain CreateEnterpriseDomain()
		{
			Domain domain = 
				new Domain( 
					store, 
					"iFolder Test Domain", 
					NewID,
					"iFolder Enterprise Test Domain", 
					SyncRoles.Master, 
					Domain.ConfigurationType.ClientServer );

			domain.SetType( domain, "Enterprise" );

			Identity identity = store.CurrentUser;
			Member owner = new Member( identity.Name, identity.ID, Access.Rights.Admin );
			owner.IsOwner = true;
			domain.Commit( new Node[] { domain, owner } );

			store.AddDomainIdentity( domain.ID, owner.UserID, null, CredentialType.None );

			LocalDatabase ldb = store.GetDatabaseObject();
			ldb.DefaultDomain = domain.ID;

			return domain;
		}

		private void FindFirstEnumerateAll( string domainID )
		{
			string context;
			Member[] memberList;
			int total;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers( 
					domainID, 
					MemberCount + 1, 
					out context, 
					out memberList, 
					out total );

			if ( moreEntries ) throw new ApplicationException( "More entries returned." );
			if ( memberList == null ) throw new ApplicationException( "No member list returned." );
			if ( memberList.Length != total ) throw new ApplicationException( "Not all entries returned in list." );
			if ( total != MemberCount ) throw new ApplicationException( "Total entries is incorrect." );

			CheckMemberOrder( memberList );
			DomainProvider.FindCloseDomainMembers( domainID, context );
		}

		private void FindFirstEnumerateByCount( string domainID, int count )
		{
			string context;
			Member[] memberList;
			int total;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers( 
				domainID, 
				count, 
				out context, 
				out memberList, 
				out total );

			if ( total != MemberCount ) throw new ApplicationException( "Total entries is incorrect." );

			if ( count <= total )
			{
				if ( moreEntries == false ) throw new ApplicationException( "No more entries." );
			}
			else
			{
				if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
			}

			if ( count > 0 )
			{
				if ( memberList == null ) throw new ApplicationException( "No member list returned." );
				if ( count < total )
				{
					if ( memberList.Length != count ) throw new ApplicationException( "Member list is not correct." );
				}
				else
				{
					if ( memberList.Length != total ) throw new ApplicationException( "Member list is not correct." );
				}

				CheckMemberOrder( memberList );
			}
			else
			{
				if ( memberList != null ) throw new ApplicationException( "Member list returned." );
			}

			DomainProvider.FindCloseDomainMembers( domainID, context );
		}

		private void FindNextEnumerateAll( string domainID )
		{
			string context;
			Member[] memberList;
			int total;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers( 
					domainID, 
					0, 
					out context, 
					out memberList, 
					out total );

			if ( moreEntries == false ) throw new ApplicationException( "No more entries." );
			if ( memberList != null ) throw new ApplicationException( "Member list is not correct." );
			if ( total != MemberCount ) throw new ApplicationException( "Total entries is incorrect." );

			moreEntries = 
				DomainProvider.FindNextDomainMembers(
					domainID,
					ref context,
					MemberCount + 1,
					out memberList );

			if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
			if ( memberList == null ) throw new ApplicationException( "No member list returned." );
			if ( memberList.Length != total ) throw new ApplicationException( "Member list is not correct." );

			CheckMemberOrder( memberList );
			DomainProvider.FindCloseDomainMembers( domainID, context );
		}

		private void FindNextEnumerateByCount( string domainID, int count )
		{
			string context;
			Member[] memberList;
			int total;
			int i;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers( 
				domainID, 
				0, 
				out context, 
				out memberList, 
				out total );

			if ( moreEntries == false ) throw new ApplicationException( "No more entries." );
			if ( memberList != null ) throw new ApplicationException( "Member list returned." );
			if ( total != MemberCount ) throw new ApplicationException( "Total entries is incorrect." );

			for ( i = 0; i < total; )
			{
				moreEntries = 
					DomainProvider.FindNextDomainMembers(
						domainID,
						ref context,
						count,
						out memberList );

				if ( count > 0 )
				{
					if ( memberList == null ) throw new ApplicationException( "No member list returned." );
					i += count;

					if ( i <= total )
					{
						if ( moreEntries == false ) throw new ApplicationException( "No more entries." );
						if ( memberList.Length != count ) throw new ApplicationException( "Member list is incorrect." );
					}
					else
					{
						if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
						if ( memberList.Length != ( total % count ) ) throw new ApplicationException( "Member list is incorrect." );
					}
				}
				else
				{
					if ( moreEntries == false ) throw new ApplicationException( "No more entries." );
					if ( memberList != null ) throw new ApplicationException( "Member list returned." );
					break;
				}
			}

			DomainProvider.FindCloseDomainMembers( domainID, context );
		}

		private void FindPreviousEnumerateAll( string domainID )
		{
			string context;
			Member[] memberList;
			int total;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers( 
					domainID, 
					0, 
					out context, 
					out memberList, 
					out total );

			if ( total != MemberCount ) throw new ApplicationException( "Total entries is incorrect." );
			if ( moreEntries == false ) throw new ApplicationException( "No entries returned." );
			if ( memberList != null ) throw new ApplicationException( "Member list returned." );

			moreEntries =
				DomainProvider.FindSeekDomainMembers(
					domainID,
					ref context,
					MemberCount,
					0,
					out memberList );

			if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
			if ( memberList != null ) throw new ApplicationException( "Member list returned." );

			moreEntries = 
				DomainProvider.FindPreviousDomainMembers(
					domainID,
					ref context,
					MemberCount + 1,
					out memberList );

			if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
			if ( memberList == null ) throw new ApplicationException( "No member list returned." );
			if ( memberList.Length != total ) throw new ApplicationException( "Member list is incorrect." );

			CheckMemberOrder( memberList );
			DomainProvider.FindCloseDomainMembers( domainID, context );
		}

		private void FindPreviousEnumerateByCount( string domainID, int count )
		{
			string context;
			Member[] memberList;
			int total;
			int i;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers( 
				domainID, 
				0,
				out context, 
				out memberList, 
				out total );

			if ( total != MemberCount ) throw new ApplicationException( "Total entries is incorrect." );
			if ( moreEntries == false ) throw new ApplicationException( "No entries returned." );
			if ( memberList != null ) throw new ApplicationException( "Member list returned." );

			moreEntries = 
				DomainProvider.FindSeekDomainMembers(
					domainID,
					ref context,
					MemberCount,
					0,
					out memberList );

			if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
			if ( memberList != null ) throw new ApplicationException( "Member list returned." );

			for ( i = 0; i < total; )
			{
				moreEntries = 
					DomainProvider.FindPreviousDomainMembers(
						domainID,
						ref context,
						count,
						out memberList );

				if ( count > 0 )
				{
					if ( memberList == null ) throw new ApplicationException( "No member list returned." );
					i += count;

					if ( i <= total )
					{
						if ( moreEntries == false ) throw new ApplicationException( "No more entries." );
						if ( memberList.Length != count ) throw new ApplicationException( "Member list is incorrect." );
					}
					else
					{
						if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
						if ( memberList.Length != ( total % count ) ) throw new ApplicationException( "Member list is incorrect." );
					}
				}
				else
				{
					if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
					if ( memberList != null ) throw new ApplicationException( "Member list returned." );
					break;
				}
			}

			DomainProvider.FindCloseDomainMembers( domainID, context );
		}

		private void FindSeekEnumerateAll( string domainID )
		{
			string context;
			Member[] memberList;
			int total;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers( 
					domainID, 
					0, 
					out context, 
					out memberList, 
					out total );

			if ( total != MemberCount ) throw new ApplicationException( "Total entries is incorrect." );
			if ( moreEntries == false ) throw new ApplicationException( "No entries returned." );
			if ( memberList != null ) throw new ApplicationException( "Member list returned." );

			moreEntries =
				DomainProvider.FindSeekDomainMembers(
					domainID,
					ref context,
					0,
					MemberCount + 1,
					out memberList );

			if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
			if ( memberList == null ) throw new ApplicationException( "No member list returned." );
			if ( memberList.Length != total ) throw new ApplicationException( "Member list is incorrect." );

			CheckMemberOrder( memberList );
			DomainProvider.FindCloseDomainMembers( domainID, context );
		}

		private void FindSeekEnumerateByCount( string domainID, int count )
		{
			string context;
			Member[] memberList;
			int total;
			int i;
            
			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers( 
					domainID, 
					0,
					out context, 
					out memberList, 
					out total );

			if ( total != MemberCount ) throw new ApplicationException( "Total entries is incorrect." );
			if ( moreEntries == false ) throw new ApplicationException( "No entries returned." );
			if ( memberList != null ) throw new ApplicationException( "Member list returned." );

			for ( i = -1; i <= total; ++i )
			{
				moreEntries = 
					DomainProvider.FindSeekDomainMembers(
						domainID,
						ref context,
						i,
						count,
						out memberList );

				if ( count > 0 )
				{
					if ( i < 0 )
					{
						if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
						if ( memberList != null ) throw new ApplicationException( "Member list returned." );
					}
					else if ( i < total )
					{
						if ( memberList == null ) throw new ApplicationException( "No member list returned." );

						if ( ( total - i ) >= count )
						{
							if ( moreEntries == false ) throw new ApplicationException( "No more entries." );
							if ( memberList.Length != count ) throw new ApplicationException( "Member list is incorrect." );
						}
						else
						{
							if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
							if ( memberList.Length != ( total - i ) ) throw new ApplicationException( "Member list is incorrect." );
						}
					}
					else
					{
						if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
						if ( memberList != null ) throw new ApplicationException( "Member list returned." );
					}
				}
				else
				{
					if ( i < 0 )
					{
						if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
					}
					else if ( i < total )
					{
						if ( moreEntries == false ) throw new ApplicationException( "No more entries." );
					}
					else
					{
						if ( moreEntries == true ) throw new ApplicationException( "More entries returned." );
					}

					if ( memberList != null ) throw new ApplicationException( "Member list returned." );
				}
			}

			DomainProvider.FindCloseDomainMembers( domainID, context );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Runs the member enumeration tests.
		/// </summary>
		public void RunTests()
		{
			// Create an enterprise domain and populate it.
			Domain domain = AddEnterpriseMembers( CreateEnterpriseDomain() );

			// Test FindFirst()
			FindFirstEnumerateAll( domain.ID );
			for ( int i = -1; i <= MemberCount + 1; ++i )
			{
				FindFirstEnumerateByCount( domain.ID, i );
			}

			// Test FindNext()
			FindNextEnumerateAll( domain.ID );
			for ( int i = -1; i <= MemberCount + 1; ++i )
			{
				FindNextEnumerateByCount( domain.ID, i );
			}

			// Test FindPrevious()
			FindPreviousEnumerateAll( domain.ID );
			for ( int i = -1; i <= MemberCount + 1; ++i )
			{
				FindPreviousEnumerateByCount( domain.ID, i );
			}

			// Test FindSeek()
			FindSeekEnumerateAll( domain.ID );
			for ( int i = -1; i <= MemberCount + 1; ++i )
			{
				FindSeekEnumerateByCount( domain.ID, i );
			}

			// Remove the domain.
			domain.Commit( domain.Delete() );
		}
		#endregion
	}
}