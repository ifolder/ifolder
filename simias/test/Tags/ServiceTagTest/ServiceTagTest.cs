using System;
using System.Collections;
using System.Runtime;
using System.IO;
using System.Threading;

using Simias;
using Simias.Service;
using Simias.Storage;
using Simias.Tags;

namespace ServiceTagTest
{
	/// <summary>
	/// Simias tag testing
	/// </summary>
	public class Service : IThreadService
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		private AutoResetEvent testEvent = null;
		private Thread testThread = null;

		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public Service()
		{
		}
		#endregion

		#region IThreadService Members
		/// <summary>
		/// Starts the thread service.
		/// </summary>
		/// <param name="config">
		/// Configuration file object for the configured store 
		/// Store to use.
		/// </param>
		// public void Start( Configuration config )
		public void Start()
		{
			log.Debug( "Start called" );

			try
			{
				StartTestThread();
			}
			catch(Exception e)
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}
		}

		/// <summary>
		/// Resumes a paused service. 
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Custom.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public int Custom(int message, string data)
		{
			return 0;
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			log.Debug( "Stop called" );
			//StopTestThread();
		}
		#endregion

		#region Private Methods
		internal int StartTestThread()
		{
			int status = 0;

			try
			{
				testEvent = new AutoResetEvent( false );
				testThread = new Thread( new ThreadStart( TestThread ) );
				testThread.IsBackground = true;
				testThread.Start();
			}
			catch( SimiasException e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
				status = -1;
			}

			return status;
		}

		private void TestThread()
		{
			int testsPassed = 0;
			int testsFailed = 0;

			log.Debug( "Waiting a bit before we start the tests!" );
			testEvent.WaitOne( 30000, false );
			log.Debug( "Starting tag tests" );

			if ( this.TestOne() == true )
			{
				testsPassed++;
			}
			else
			{
				testsFailed++;
			}

			if ( this.TestTwo() == true )
			{
				testsPassed++;
			}
			else
			{
				testsFailed++;
			}

			if ( this.TestThree() == true )
			{
				testsPassed++;
			}
			else
			{
				testsFailed++;
			}

			if ( this.TestFour() == true )
			{
				testsPassed++;
			}
			else
			{
				testsFailed++;
			}

			if ( this.TestFive() == true )
			{
				testsPassed++;
			}
			else
			{
				testsFailed++;
			}

			if ( this.TestSix() == true )
			{
				testsPassed++;
			}
			else
			{
				testsFailed++;
			}

			if ( this.TestSeven() == true )
			{
				testsPassed++;
			}
			else
			{
				testsFailed++;
			}

			log.Debug( "  Test Results" );
			log.Debug( "    Passed: " + testsPassed.ToString() );
			log.Debug( "    Failed: " + testsFailed.ToString() );

			log.Debug( "Tests finished" );
		}

		private bool TestOne()
		{
			bool passed = true;
			log.Debug( "Starting TestOne" );
			Store store = Store.GetStore();
			Domain domain = store.GetDomain( store.LocalDomain );
			log.Debug( "  retrieved the default domain: " + domain.Name );

			log.Debug( "  creating two tags in the default domain" );
			Tag greenTag = new Tag( "Green" );
			Tag redTag = new Tag( "Red" );

			try
			{
				greenTag.Add( domain.ID );
				redTag.Add( domain.ID );
			}
			catch
			{
				passed = false;
			}

			log.Debug( "  querying for all tags on the default domain" );
			ICSList tagList = Simias.Tags.Query.Tags( domain );
			if ( tagList.Count >= 2 )
			{
				foreach( ShallowNode sn in tagList )
				{
					Tag tag = new Tag( sn );
					log.Debug( "  Tag: " + tag.Name );
				}
			}
			else
			{
				passed = false;
			}
		
			Node[] nodes = new Node[3];
			log.Debug( "  creating three generic nodes" );
			nodes[0] = new Node( "Node1" );
			nodes[1] = new Node( "Node2" );
			nodes[2] = new Node( "Node3" );
			domain.Commit( nodes );

			log.Debug( "  adding the green tag to node1" );
			greenTag.TagNode( domain.ID, nodes[0] );
			log.Debug( "  query for all nodes with the green tag - should find 1 node" );
			ICSList results = Simias.Tags.Query.Nodes( domain.ID, greenTag );
			log.Debug( "  found: " + results.Count.ToString() + " tag(s)" );
			if ( results.Count == 1 )
			{
				foreach( ShallowNode sn in results )
				{
					log.Debug( "  Node: " + sn.Name );
				}
			}
			else
			{
				passed = false;
			}

			log.Debug( "  adding red tag to node2" );
			redTag.TagNode( domain.ID, nodes[1] );

			log.Debug( "  query for all nodes with the red and green tag - " );
			log.Debug( "  should find two nodes" );
			Simias.Tags.Query query = new Simias.Tags.Query( domain.ID );
			query.AddTag( greenTag );
			query.AddTag( redTag );
			ICSList results2 = query.QueryNodes();
			if ( results2.Count == 2 )
			{
				foreach( ShallowNode sn in results2 )
				{
					log.Debug( "  Node: " + sn.Name );
				}
			}
			else
			{
				passed = false;
			}

			log.Debug( "  adding red tag to node3" );
			redTag.TagNode( domain.ID, nodes[2] );

			log.Debug( "  query for all nodes with the red and green tag - should find 3 nodes" );
			ICSList results3 = query.QueryNodes();
			if ( results3.Count == 3 )
			{
				foreach( ShallowNode sn in results3 )
				{
					log.Debug( "  Node: " + sn.Name );
				}
			}
			else
			{
				passed = false;
			}

			log.Debug( "  cleaning up nodes and tags" );
			domain.Commit( domain.Delete( nodes ) );
			greenTag.Remove( domain.ID );
			redTag.Remove( domain.ID );

			log.Debug( "TestOne finished\n" );
			return passed;
		}

		private bool TestTwo()
		{
			bool passed = true;
			log.Debug( "Starting TestTwo" );
			Store store = Store.GetStore();
			Domain domain = store.GetDomain( store.LocalDomain );
			log.Debug( "  retrieved the default domain: " + domain.Name );

			log.Debug( "  creating two tags in the default domain" );
			Tag purpleTag = new Tag( "Purple" );
			purpleTag.Add( domain.ID );

			Tag whiteTag = new Tag( "White" );
			whiteTag.Add( domain.ID );

			// Can I read the tag back
			Node whiteNode = domain.GetNodeByID( whiteTag.ID );
			if ( whiteNode == null )
			{
				log.Debug( "  nope couldn't read the white tag" );
			}

			Node[] nodes = new Node[500];
			log.Debug( "  creating five hundred generic nodes" );

			// Cleanup any stragglers
			for( int i = 0; i < 500; i++ )
			{
				Node node = domain.GetSingleNodeByName( "Node" + i.ToString() );
				if ( node != null )
				{
					domain.Commit( domain.Delete( node ) );
				}
			}

			for( int i = 0; i < 500; i++ )
			{
				nodes[i] = new Node( "Node" + i.ToString() );
			}

			domain.Commit( nodes );

			// Add purple to the first half
			log.Debug( "  adding the purple tag to half the nodes" );
			for( int i = 0; i < 250; i++ )
			{
				purpleTag.TagNode( domain.ID, nodes[i] );
			}

			log.Debug( "  adding the white tag to the last two nodes" );
			whiteTag.TagNode( domain.ID, nodes[498] );
			whiteTag.TagNode( domain.ID, nodes[499] );

			log.Debug( "  query for all nodes with the purple tag - should find 250 nodes" );
			ICSList results = Simias.Tags.Query.Nodes( domain.ID, purpleTag );
			log.Debug( "  found: " + results.Count.ToString() );
			foreach( ShallowNode sn in results )
			{
				log.Debug( "  Node: " + sn.Name );
			}

			log.Debug( "  query for all purple and white nodes - should find 252 nodes" );
			Simias.Tags.Query query = new Simias.Tags.Query( domain.ID );
			query.AddTag( whiteTag );
			query.AddTag( purpleTag );
			ICSList results2 = query.QueryNodes();
			log.Debug( "  Found: " + results2.Count.ToString() );
			foreach( ShallowNode sn in results2 )
			{
				log.Debug( "  Node: " + sn.Name );
			}

			log.Debug( "  cleaning up nodes and tags" );
			domain.Commit( domain.Delete( nodes ) );
			
			purpleTag.Remove( domain.ID );
			whiteTag.Remove( domain.ID );

			log.Debug( "TestTwo finished\n" );
			return passed;
		}

		private bool TestThree()
		{
			bool passed = true;
			log.Debug( "Starting TestThree" );
			log.Debug( "Create a tag and a node and leave them in the store" );

			Store store = Store.GetStore();
			Domain domain = store.GetDomain( store.LocalDomain );
			log.Debug( "  retrieved the default domain: " + domain.Name );

			log.Debug( "  creating \"ThreeTag\" in the default domain" );
			Tag threeTag = new Tag( "ThreeTag" );
			try
			{
				threeTag.Add( domain.ID );
			}
			catch( ExistsException e )
			{
				log.Debug( "  ThreeTag already exists in the tags library" );
			}

			log.Debug( "  creating ThreeNode" );
			Node threeNode = domain.GetSingleNodeByName( "ThreeNode" );
			if ( threeNode == null )
			{
				threeNode = new Node( "ThreeNode" );
				domain.Commit( threeNode );
				threeTag.TagNode( domain.ID, threeNode );
			}
			log.Debug( "Finished TestThree\n" );
			return passed;
		}

		private bool TestFour()
		{
			bool passed = true;
			log.Debug( "Starting TestFour" );
			log.Debug( "Create a tag and then attempt to create the same tag again" );

			Store store = Store.GetStore();
			Domain domain = store.GetDomain( store.LocalDomain );
			log.Debug( "  retrieved the default domain: " + domain.Name );

			log.Debug( "  creating \"DuplicateTag\" in the default domain" );
			Tag dupTag = new Tag( "DuplicateTag" );
			try
			{
				dupTag.Add( domain.ID );
			}
			catch
			{
				log.Debug( "  failed creating tag" );
				passed = false;
			}

			log.Debug( "  attempting to duplicate the same tag" );
			Tag dup2Tag = new Tag( "DuplicateTag" );
			try
			{
				dup2Tag.Add( domain.ID );
				log.Debug( "  an \"ExistsException\" should have been thrown" );
				passed = false;
			}
			catch( ExistsException e )
			{
				log.Debug( "  caught expected \"ExistsException\"" );
			}
			catch( Exception e2 )
			{
				log.Debug( "  An unexpected exception was caught" );
				passed = false;
			}

			dupTag.Remove( domain.ID );

			//domain.Commit( domain.Delete( dupTag ) );
			log.Debug( "Finished TestFour\n" );
			return passed;
		}

		private bool TestFive()
		{
			bool passed = true;
			log.Debug( "Starting TestFive" );
			log.Debug( "Create two tags, assign them to a node then delete one of the tags and verify it was deleted from the node" );

			Store store = Store.GetStore();
			Domain domain = store.GetDomain( store.LocalDomain );
			log.Debug( "  retrieved the default domain: " + domain.Name );

			Tag tagFiveOne = new Tag( "TestFiveTagOne" );
			Tag tagFiveTwo = new Tag( "TestFiveTagTwo" );

			try
			{
				log.Debug( "  creating \"TestFiveTagOne\" in the default domain" );
				tagFiveOne.Add( domain.ID );
			}
			catch
			{
				passed = false;
				log.Debug( "  failed creating TestFiveTagOne" );
			}

			log.Debug( "  creating \"TestFiveTagTwo\" in the default domain" );
			try
			{
				tagFiveTwo.Add( domain.ID );
			}
			catch( ExistsException e )
			{
				passed = false;
				log.Debug( "  failed creating TestFiveTagTwo with ExistsException" );
			}
			catch
			{
				passed = false;
				log.Debug( "  failed creating TestFiveTagTwo with an unknown exception" );
			}

			log.Debug( "  creating TestFiveNode" );
			Node testFiveNode = new Node( "TestFiveNode" );
			domain.Commit( testFiveNode );

			try
			{
				log.Debug( "  tagging TestFiveNode with \"TestFiveTagOne\" and \"TestFiveTagTwo\"" );
				tagFiveOne.TagNode( domain.ID, testFiveNode );
				tagFiveTwo.TagNode( domain.ID, testFiveNode );
			}
			catch
			{
				log.Debug( "  failed tagging node" );
				passed = false;
			}
			
			try
			{
				log.Debug( "  deleting tag: \"TestFiveTagTwo\"" );
				tagFiveTwo.Remove( domain.ID );
			}
			catch
			{
				log.Debug( "  failed removing TestFiveTagTwo from the library" );
				passed = false;
			}

			log.Debug( "  deleting node: " + testFiveNode.Name );
			domain.Commit( domain.Delete( testFiveNode ) );

			try
			{
				log.Debug( "  deleting tag: " + tagFiveOne.Name );
				tagFiveOne.Remove( domain.ID );
			}
			catch
			{
				log.Debug( "  failed removing TestFiveTagOne from the library" );
				passed = false;
			}

			log.Debug( "Finished TestFive\n" );
			return passed;
		}

		private bool TestSix()
		{
			bool passed = true;
			int numberTags = 1000;

			log.Debug( "Starting TestSix" );
			log.Debug( "Create " + numberTags.ToString() + " tags, issue a query to retrieve them, then verify them" );

			Store store = Store.GetStore();
			Domain domain = store.GetDomain( store.LocalDomain );
			log.Debug( "  retrieved the default domain: " + domain.Name );

			Hashtable tagTable = new Hashtable( numberTags );
			Tag[] tags = new Tag[ numberTags ];
			log.Debug( "  creating " + numberTags.ToString() + " tags." );
			for( int i = 0; i < numberTags; i++)
			{
				tags[i] = new Tag( "TestSixTag" + i.ToString() );
				tagTable.Add( tags[i].Name, tags[i] );
			}

			try 
			{
				Simias.Tags.Tag.Add( domain, tags );
			}
			catch( Exception e )
			{
				log.Debug( "  failed to create all tags!" );
				passed = false;
			}

			if ( passed == true )
			{
				int verified = 0;
				ICSList results = Simias.Tags.Query.Tags( domain );
				if ( results.Count > 0 )
				{
					foreach( ShallowNode sn in results )
					{
						if ( tagTable.Contains( sn.Name ) == true )
						{
							verified++;
						}
					}
				}

				if ( verified == numberTags )
				{
					log.Debug( "  verified all " + verified.ToString() + " tags." );
				}
				else
				{
					log.Debug( "  failed verifying all the tags.  verified: " + verified.ToString() + " of the " + numberTags.ToString() + " created" );
					passed = false;
				}
			}

			// Remove all the previously created tags
			log.Debug( "  removing tags from the collection" );
			Simias.Tags.Tag.Remove( domain, tags );
			log.Debug( "Finished TestSix\n" );
			return passed;
		}

		private bool TestSeven()
		{
			bool passed = true;
			log.Debug( "Starting TestSeven" );
			log.Debug( "Test the TagNode and UntagNode methods of the framework" );

			Store store = Store.GetStore();
			Domain domain = store.GetDomain( store.LocalDomain );
			log.Debug( "  retrieved the default domain: " + domain.Name );

			Tag tagOne = new Tag( "TestSevenTagOne" );
			Tag tagTwo = new Tag( "TestSevenTagTwo" );
			try
			{
				log.Debug( "  creating tag: " + tagOne.Name );
				tagOne.Add( domain.ID );

				log.Debug( "  creating tag: " + tagTwo.Name );
				tagTwo.Add( domain.ID );
			}
			catch
			{
				log.Debug( "  failed creating tags" );
				passed = false;
			}

			Node node = new Node( "TestSevenNode" );
			log.Debug( "  creating node: " + node.Name );
			domain.Commit( node );

			try
			{
				log.Debug( "  tagging node \"" + node.Name + "\" with tag \"" + tagOne.Name + "\"" );
				tagOne.TagNode( domain.ID, node );

				log.Debug( "  tagging node \"" + node.Name + "\" with tag \"" + tagTwo.Name + "\"" );
				tagTwo.TagNode( domain.ID, node );

				log.Debug( "  untagging node \"" + node.Name + "\" from tag \"" + tagOne.Name + "\"" );
				tagOne.UntagNode( domain.ID, node );
			}
			catch
			{
				log.Debug( "  failed tagging/untagging node" );
				passed = false;
			}

			log.Debug( "  deleting nodes and tags" );
			domain.Commit( domain.Delete( node ) );
			tagOne.Remove( domain.ID );
			tagTwo.Remove( domain.ID );
			log.Debug( "Finished TestSeven\n" );
			return passed;
		}
		#endregion
	}
}
