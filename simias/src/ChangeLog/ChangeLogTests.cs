using System;
using System.Collections;
using System.IO;
using System.Threading;

using Simias;
using Simias.Service;
using NUnit.Framework;

using Simias.Storage;

namespace Simias.Storage.Tests
{
	/// <summary>
	/// Test cases for Iteration 0 stories.
	/// </summary>
	[TestFixture]
	public class ChangeLogTests
	{
		#region Class Members
		// Object used to access the store.
		private Store store = null;

		// Path to store.
		private string basePath = Path.Combine( Directory.GetCurrentDirectory(), "CollectionStoreTestDir" );
		private Manager manager;
		#endregion

		#region Test Setup
		/// <summary>
		/// Performs pre-initialization tasks.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			Configuration config = Configuration.CreateDefaultConfig( basePath );

			manager = new Manager( config );
			manager.StartServices();
			manager.WaitForServicesStarted();

			// Connect to the store.
			store = Store.GetStore();
		}
		#endregion

		#region Iteration Tests
		/// <summary>
		/// Creates objects in a collection and verifies that the log file changes are being seen.
		/// </summary>
		[Test]
		public void Test1()
		{
			// Create a new collection and remember its ID.
			Collection collection = new Collection( store, "CS_TestCollection", store.DefaultDomain);

			try
			{
				// Commit the collection.
				collection.Commit();

				// Create a change log reader.
				ChangeLogReader logReader = new ChangeLogReader( collection );
				
				// Get a cookie to track the event changes that we've seen.
				EventContext eventCookie = logReader.GetEventContext();

				// Now start creating a bunch of collection events.
				Node[] nodeList = new Node[ 100 ];
				for( int i = 0; i < 100; ++i )
				{
					nodeList[ i ] = new Node( "CS_TestNode-" + i );
				}

				// Commit the changes.
				collection.Commit( nodeList );

				// Now get the events that have been generated.
				ArrayList changeList;
				bool moreData = logReader.GetEvents( eventCookie, out changeList );

				foreach( ChangeLogRecord rec in changeList )
				{
					Console.WriteLine( "Found change: Record ID: {0}, TimeStamp: {1}, Operation: {2}, Node ID: {3}", rec.RecordID, rec.Epoch, rec.Operation, rec.EventID );
				}

				// Delete all of the node objects.
				collection.Commit( collection.Delete( nodeList ) );

				// Get the delete events.
				moreData = logReader.GetEvents( eventCookie, out changeList );

				foreach( ChangeLogRecord rec in changeList )
				{
					Console.WriteLine( "Found change: Record ID: {0}, TimeStamp: {1}, Operation: {2}, Node ID: {3}", rec.RecordID, rec.Epoch, rec.Operation, rec.EventID );
				}
			}
			finally
			{
				// Delete the collection.
				collection.Commit( collection.Delete() );
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
			// Delete the database.  Must be store owner to delete the database.
			store.Delete();

			manager.StopServices();
			manager.WaitForServicesStopped();

			// Remove the created directory.
			string dirPath = Path.Combine( Directory.GetCurrentDirectory(), "CollectionStoreTestDir" );
			if ( Directory.Exists( dirPath ) )
			{
//				Directory.Delete( dirPath, true );
			}
		}
		#endregion
	}
}
