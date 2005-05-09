/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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

namespace Simias.Storage.Tests
{
	/// <summary>
	/// Class used to test the collection lock functionality.
	/// </summary>
	public class CollectionLockTests
	{
		#region Class Members

		private Store store;

		#endregion

		#region Properties
		#endregion

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="store">Handle to the store.</param>
		public CollectionLockTests( Store store )
		{
			this.store = store;
		}

		#endregion

		#region Private Methods

		private void LockAndWriteTest()
		{
			Collection c = new Collection( store, "LockAndWriteTest", store.LocalDomain );
			c.Lock( "MyLockString" );
			c.Commit();
			c.Commit( c.Delete() );
		}

		private void LockAndFailWriteTest()
		{
			Collection c = new Collection( store, "LockAndFailWriteTest", store.LocalDomain );
			c.Lock( "MyLockString" );
			c.Commit();

			try
			{
				Collection c2 = store.GetCollectionByID( c.ID );
				c2.Properties.AddProperty( "Property1", "This is a test." );

				try
				{
					c2.Commit();
					throw new ApplicationException( "Collection was not locked." );
				}
				catch ( LockException )
				{}
			}
			finally
			{
				c.Commit( c.Delete() );
			}
		}

		private void LockAndUnlockWriteTest()
		{
			Collection c = new Collection( store, "LockAndUnlockWriteTest", store.LocalDomain );
			c.Lock( "MyLockString" );
			c.Commit();

			try
			{
				Collection c2 = store.GetCollectionByID( c.ID );
				c2.Properties.AddProperty( "Property1", "This is a test." );

				try
				{
					c2.Commit();
					throw new ApplicationException( "Collection was not locked." );
				}
				catch ( LockException )
				{}

				c.Unlock( "MyLockString" );
				c2.Commit();
			}
			finally
			{
				c.Commit( c.Delete() );
			}
		}

		private void CopyLockTest1()
		{
			Collection c = new Collection( store, "LockAndUnlockWriteTest", store.LocalDomain );
			c.Lock( "MyLockString" );
			c.Commit();

			try
			{
				Collection c2 = new Collection( c );
				c2.Properties.AddProperty( "Property1", "This is a test." );
				c2.Commit();
			}
			finally
			{
				c.Commit( c.Delete() );
			}
		}

		private void CopyLockTest2()
		{
			Collection c = new Collection( store, "LockAndUnlockWriteTest", store.LocalDomain );
			c.Lock( "MyLockString" );
			c.Commit();

			try
			{
				Collection c2 = store.GetCollectionByID( c.ID );
				c2.Lock( "MyLockString" );
				c2.Properties.AddProperty( "Property1", "This is a test." );
				c2.Commit();
				c2.Unlock( "MyLockString" );
			}
			finally
			{
				c.Commit( c.Delete() );
			}
		}

		private void MultipleLockTest()
		{
			Collection c = new Collection( store, "MultipleLockTest", store.LocalDomain );
			c.Lock( "MyLockString" );
			c.Commit();

			try
			{
				try
				{
					Collection c2 = store.GetCollectionByID( c.ID );
					c2.Properties.AddProperty( "Property1", "This is a test." );
					c2.Commit();
					throw new ApplicationException( "Collection should have been locked." );
				}
				catch ( LockException )
				{}

				// Locking multiple times should only require one unlock.
				c.Lock( "MyLockString" );
				c.Lock( "MyLockString" );
				c.Lock( "MyLockString" );
				c.Lock( "MyLockString" );

				// Single unlock
				c.Unlock( "MyLockString" );

				Collection c3 = store.GetCollectionByID( c.ID );
				c3.Properties.AddProperty( "Property1", "This is a test." );
				c3.Commit();
			}
			finally
			{
				c.Commit( c.Delete() );
			}
		}

		private void MultipleLockFailTest()
		{
			Collection c = new Collection( store, "MultipleLockFailTest", store.LocalDomain );
			c.Lock( "MyLockString" );
			try
			{
				c.Lock( "MyLockString2" );
				throw new ApplicationException( "Obtained more that one lock on a single collection." );
			}
			catch ( AlreadyLockedException )
			{}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Runs the collection owner tests.
		/// </summary>
		public void RunTests()
		{
			LockAndWriteTest();
			LockAndFailWriteTest();
			LockAndUnlockWriteTest();
			CopyLockTest1();
			CopyLockTest2();
			MultipleLockTest();
			MultipleLockFailTest();
		}

		#endregion
	}
}