using System;
using System.Collections;
using System.IO;
using System.Threading;

using Simias;
using Simias.Service;
using Simias.Storage;

namespace ChangeLogTest
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class ChangeLogTest
	{
		#region Class Members
		private Store store;
		private Manager manager;
		private string collectionID = Guid.NewGuid().ToString();
		#endregion

		#region Constructor
		public ChangeLogTest()
		{
			Configuration config = new Configuration( Directory.GetCurrentDirectory() );

			manager = new Manager( config );
			manager.StartServices();
			manager.WaitForServicesStarted();

			store = new Store( config );
		}
		#endregion

		#region Public Methods
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
			EventContext eventCookie = logReader.GetEventContext();

			// Now start creating a bunch of collection events.
			Node[] nodeList = new Node[ 200 ];
			for( int i = 0; i < 200; ++i )
			{
				nodeList[ i ] = new Node( "CS_TestNode-" + i );
			}

			// Commit the changes.
			collection.Commit( nodeList );
			Thread.Sleep( 100 );

			// Now get the events that have been generated.
			ArrayList changeList;
			bool moreData;
			
			try
			{
				moreData = logReader.GetEvents( eventCookie, out changeList );
				Console.WriteLine( "Found {0} change records.", changeList.Count );
				foreach( ChangeLogRecord rec in changeList )
				{
					Console.WriteLine( "Found change: Record ID: {0}, TimeStamp: {1}, Operation: {2}, Node ID: {3}", rec.RecordID, rec.Epoch, rec.Operation, rec.EventID );
				}
			}
			catch ( CookieExpiredException )
			{
				Console.WriteLine( "Cookie expired - Have to dredge." );
			}
		}

		public void Test2()
		{
			// Create a new collection and remember its ID.
			Collection collection = store.GetCollectionByID( collectionID );

			// Create a change log reader.
			ChangeLogReader logReader = new ChangeLogReader( collection );
			
			// Get a cookie to track the event changes that we've seen.
			EventContext eventCookie = logReader.GetEventContext();

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
			Thread.Sleep( 100 );

			// Now get the events that have been generated.
			ArrayList changeList;
			bool moreData;

			// Get the delete events.
			try
			{
				moreData = logReader.GetEvents( eventCookie, out changeList );
				Console.WriteLine( "Found {0} change records.", changeList.Count );
				foreach( ChangeLogRecord rec in changeList )
				{
					Console.WriteLine( "Found change: Record ID: {0}, TimeStamp: {1}, Operation: {2}, Node ID: {3}", rec.RecordID, rec.Epoch, rec.Operation, rec.EventID );
				}
			}
			catch ( CookieExpiredException )
			{
				Console.WriteLine( "Cookie expired - Have to dredge." );
			}
		}

		public void Test3()
		{
			manager.StopServices();
			manager.WaitForServicesStopped();
			manager.StartServices();
			manager.WaitForServicesStarted();
		}

		public void Stop()
		{
			store.Delete();

			manager.StopServices();
			manager.WaitForServicesStopped();
		}
		#endregion

		[STAThread]
		static void Main(string[] args)
		{
			ChangeLogTest test = new ChangeLogTest();
			test.Test1();
			test.Test2();
			test.Test3();
			test.Stop();
		}
	}
}
