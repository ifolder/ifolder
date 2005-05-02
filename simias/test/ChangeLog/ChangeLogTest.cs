using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

using NUnit.Framework;

using Simias;
using Simias.Client;
using Simias.Service;
using Simias.Storage;

namespace ChangeLogTest
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	[TestFixture]
	public class ChangeLogTest
	{
		#region Class Members
		private Store store;
		private Simias.Service.Manager manager;
		private string collectionID = Guid.NewGuid().ToString();
		private EventContext eventCookie;
		#endregion

		#region Test Setup
		/// <summary>
		/// Performs pre-initialization tasks.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			manager = Simias.Service.Manager.GetManager();
			manager.StartServices();
			manager.WaitForServicesStarted();

			store = Store.GetStore();
		}
		#endregion

		#region Iteration Tests
		/// <summary>
		/// Test
		/// </summary>
		[Test]
		public void Test1()
		{
			// Create a new collection and remember its ID.
			Collection collection = new Collection( store, "CS_TestCollection", collectionID, store.DefaultDomain );

			// Commit the collection.
			collection.Commit();
			Thread.Sleep( 100 );

			// Create a change log reader.
			ChangeLogReader logReader = new ChangeLogReader( collection );
		
			// Get a cookie to track the event changes that we've seen.
			eventCookie = logReader.GetEventContext();

			// Now start creating a bunch of collection events.
			Node[] nodeList = new Node[ 200 ];
			for( int i = 0; i < 200; ++i )
			{
				nodeList[ i ] = new Node( "CS_TestNode-" + i );
			}

			// Commit the changes.
			collection.Commit( nodeList );

			// Now get the events that have been generated.
			int eventCount = 0;
			while ( eventCount != 200 )
			{
				Thread.Sleep( 100 );

				try
				{
					ArrayList changeList;
					logReader.GetEvents( eventCookie, out changeList );
					eventCount += changeList.Count;
				}
				catch ( CookieExpiredException )
				{
					Console.WriteLine( "Cookie expired - Have to dredge." );
					break;
				}
			}
		}

		/// <summary>
		/// Test
		/// </summary>
		[Test]
		public void Test2()
		{
			// Create a new collection and remember its ID.
			Collection collection = store.GetCollectionByID( collectionID );

			// Create a change log reader.
			ChangeLogReader logReader = new ChangeLogReader( collection );
			
			// Build a list of all the node in the collection.
			ArrayList tempList = new ArrayList( 200 );
			foreach ( ShallowNode sn in collection )
			{
				if ( sn.Type == NodeTypes.NodeType )
				{
					tempList.Add( new Node( collection, sn ) );
				}
			}

			// Delete all of the node objects.
			Node[] nodeList = tempList.ToArray( typeof( Node ) ) as Node[];
			collection.Commit( collection.Delete( nodeList ) );

			// Get the delete events.
			int eventCount = 0;
			while ( eventCount != 200 )
			{
				Thread.Sleep( 100 );

				try
				{
					ArrayList changeList;
					logReader.GetEvents( eventCookie, out changeList );
					eventCount += changeList.Count;
				}
				catch ( CookieExpiredException )
				{
					Console.WriteLine( "Cookie expired - Have to dredge." );
					break;
				}
			}
		}

		/// <summary>
		/// Test
		/// </summary>
		[Test]
		public void Test3()
		{
			Collection collection = store.GetCollectionByID( collectionID );
			ChangeLogReader logReader = new ChangeLogReader( collection );

			ArrayList changeList;
			logReader.GetEvents( eventCookie, out changeList );
			if ( changeList.Count > 0 )
			{
				throw new ApplicationException( "Still have unconsumed events." );
			}
		}
		#endregion

		#region Test Clean Up
		/// <summary>
		/// Clean up for tests.
		/// </summary>
		[TestFixtureTearDown]
		public void Cleanup()
		{
			manager.StopServices();
			manager.WaitForServicesStopped();
			store.Delete();
		}
		#endregion
	}
}
