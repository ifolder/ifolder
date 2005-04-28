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
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using NUnit.Framework;

using Simias;
using Simias.Client;
using Simias.Policy;
using Simias.POBox;
using Simias.Service;
using Simias.Storage;
using Simias.Storage.Provider;
using Simias.Sync;


namespace Simias.Storage.Tests
{
	/// <summary>
	/// Test cases for Iteration 0 stories.
	/// </summary>
	[TestFixture]
	public class CollectionStoreTests
	{
		#region Class Members
		// Enumerated type to use for property test.
		private enum TestType { TestType1, TestType2 };

		// Object used to access the store.
		private Store store = null;
		private Service.Manager manager = null;
		#endregion

		#region Test Setup
		/// <summary>
		/// Performs pre-initialization tasks.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			// Connect to the store.
			store = Store.GetStore();
			manager = new Simias.Service.Manager( Configuration.GetConfiguration() );
			manager.StartServices();
			manager.WaitForServicesStarted();
		}
		#endregion

		#region Iteration Tests
		/// <summary>
		/// Connects to a Store and creates, commits and deletes a Collection.
		/// </summary>
		[Test]
		public void CreateCollectionTest()
		{
			// Create a new collection and remember its ID. Use all the special XML characters in the name.
			Collection collection = new Collection( store, "&<>\"\'CS_TestCollection", store.LocalDomain );

			// Remember the id for later.
			string ID = collection.ID;

			try
			{
				// Now commit it to the store.
				collection.Commit();

				// Make sure the collection exists.
				if ( store.GetCollectionByID( ID ) == null )
				{
					throw new ApplicationException( "Collection was committed but does not exist in the store" );
				}

				// This was added to make sure that we handle all the XML special characters in the ShallowNode list.
				foreach( ShallowNode sn in collection )
				{
					if ( ( sn.ID == collection.ID ) && ( sn.Name != collection.Name ) )
					{
						throw new ApplicationException( "Special XML characters not being handled properly." );
					}
				}
			}
			finally
			{
				// Delete the collection.
				collection.Commit( collection.Delete() );
				if ( store.GetCollectionByID( ID ) != null )
				{
					throw new ApplicationException( "Collection object not deleted" );
				}
			}
		}


		/// <summary>
		/// Create a collection and adds a child node to the collection.
		/// </summary>
		[Test]
		public void CreateChildNodeTest()
		{
			// Create the collection.
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				// Create a node subordinate to this collection.
				Node child = new Node( "CS_ChildNode" );

				// Add a relationship that will reference the parent Node.
				Relationship parentChild = new Relationship( collection.ID, collection.ID );
				child.Properties.AddProperty( "MyParent", parentChild );

				// Commit this collection.
				Node[] commitList = { collection, child };
				collection.Commit( commitList );

				// Search this collection for this child.
				bool foundChild = false;
				ICSList results = collection.Search( "MyParent", parentChild );
				foreach ( ShallowNode shallowNode in results )
				{
					Node node = new Node( collection, shallowNode );
					if ( node.ID == child.ID )
					{
						foundChild = true;
						break;
					}
				}

				// Make sure the child was found.
				if ( !foundChild )
				{
					throw new ApplicationException( "CreateChildNode: Hierarchical linkage failure" );
				}

				// Delete the child node and then delete the tombstone.
				collection.Commit( collection.Delete( child ) );

				// See if the child node still exists.
				if ( collection.GetSingleNodeByName( "CS_ChildNode" ) != null )
				{
					throw new ApplicationException( "Child node not deleted." );
				}
			}
			finally
			{
				// Get rid of this collection.
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Performs the various types of searches.
		/// </summary>
		[Test]
		public void SearchTest()
		{
			// Create the collection.
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				string s1 = "The quick red fox jumps over the lazy brown dog&.";
				string s2 = "The lazy brown dog& doesn't< >care what the quick red fox does.";

				// Add some properties that can be searched for.
				collection.Properties.AddProperty( "CS_String", s1 );
				collection.Properties.AddProperty( "CS_Int", 291 );
				collection.Commit();
			
				Node node = new Node( "CS_Node" );
				node.Properties.AddProperty( "CS_String", s2 );
				node.Properties.AddProperty( "CS_Int", -291 );
				collection.Commit( node );

				// Do the various types of searches for strings and values.

				// Should return s1.
				ICSEnumerator e = ( ICSEnumerator )collection.Search( "CS_String", s1, SearchOp.Equal ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Node sNode = new Node( collection, e.Current as ShallowNode );
						Property p = sNode.Properties.GetSingleProperty( "CS_String" );
						if ( ( string )p.Value != s1 )
						{
							throw new ApplicationException( "Unexpected search results" );
						}

						// Should be no more items.
						if ( e.MoveNext() )
						{
							throw new ApplicationException( "Unexpected search results" );
						}
					}
					else
					{
						throw new ApplicationException( "Unexpected search results" );
					}
				}
				finally
				{
					e.Dispose();
				}


				// Should return s2.
				e = ( ICSEnumerator )collection.Search( "CS_String", s1, SearchOp.Not_Equal ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Node sNode = new Node( collection, e.Current as ShallowNode );
						Property p = sNode.Properties.GetSingleProperty( "CS_String" );
						if ( ( string )p.Value != s2 )
						{
							throw new ApplicationException( "Unexpected search results" );
						}

						// Should only be one.
						if ( e.MoveNext() )
						{
							throw new ApplicationException( "Unexpected search results" );
						}
					}
					else
					{
						throw new ApplicationException( "Unexpected search results" );
					}
				}
				finally
				{
					e.Dispose();
				}

	
				// Should return s1 and s2.
				e = ( ICSEnumerator )collection.Search( "CS_String", "The", SearchOp.Begins ).GetEnumerator();
				try
				{
					int count = 0;
					while ( e.MoveNext() ) ++count;
					if ( count != 2 )
					{
						throw new ApplicationException( "Unexpected search results" );
					}
				}
				finally
				{
					e.Dispose();
				}


				// Should return s2.
				e = ( ICSEnumerator )collection.Search( "CS_String", "fox does.", SearchOp.Ends ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Node sNode = new Node( collection, e.Current as ShallowNode );
						Property p = sNode.Properties.GetSingleProperty( "CS_String" );
						if ( ( string )p.Value != s2 )
						{
							throw new ApplicationException( "Unexpected search results" );
						}

						// Should only be one.
						if ( e.MoveNext() )
						{
							throw new ApplicationException( "Unexpected search results" );
						}
					}
					else
					{
						throw new ApplicationException( "Unexpected search results" );
					}
				}
				finally
				{
					e.Dispose();
				}


				// Should return s1 and s2.
				e = ( ICSEnumerator )collection.Search( "CS_String", "lazy brown dog&", SearchOp.Contains ).GetEnumerator();
				try
				{
					int count = 0;
					while ( e.MoveNext() ) ++count;
					if ( count != 2 )
					{
						throw new ApplicationException( "Unexpected search results" );
					}
				}
				finally
				{
					e.Dispose();
				}


				// Should return 291.
				e = ( ICSEnumerator )collection.Search( "CS_Int", 291, SearchOp.Equal ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Node sNode = new Node( collection, e.Current as ShallowNode );
						Property p = sNode.Properties.GetSingleProperty( "CS_Int" );
						if ( ( int )p.Value != 291 )
						{
							throw new ApplicationException( "Unexpected search results" );
						}

						// Should only return one.
						if ( e.MoveNext() )
						{
							throw new ApplicationException( "Unexpected search results" );
						}
					}
					else
					{
						throw new ApplicationException( "Unexpected search results" );
					}
				}
				finally
				{
					e.Dispose();
				}


				// Should return -291.
				e = ( ICSEnumerator )collection.Search( "CS_Int", 291, SearchOp.Not_Equal ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Node sNode = new Node( collection, e.Current as ShallowNode );
						Property p = sNode.Properties.GetSingleProperty( "CS_Int" );
						if ( ( int )p.Value != -291 )
						{
							throw new ApplicationException( "Unexpected search results" );
						}

						// Should only return one.
						if ( e.MoveNext() )
						{
							throw new ApplicationException( "Unexpected search results" );
						}
					}
					else
					{
						throw new ApplicationException( "Unexpected search results" );
					}
				}
				finally
				{
					e.Dispose();
				}


				// Should return 291.
				e = ( ICSEnumerator )collection.Search( "CS_Int", 0, SearchOp.Greater ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Node sNode = new Node( collection, e.Current as ShallowNode );
						Property p = sNode.Properties.GetSingleProperty( "CS_Int" );
						if ( ( int )p.Value != 291 )
						{
							throw new ApplicationException( "Unexpected search results" );
						}

						// Should only be one.
						if ( e.MoveNext() )
						{
							throw new ApplicationException( "Unexpected search results" );
						}
					}
					else
					{
						throw new ApplicationException( "Unexpected search results" );
					}
				}
				finally
				{
					e.Dispose();
				}


				// Should return -291.
				e = ( ICSEnumerator )collection.Search( "CS_Int", 0, SearchOp.Less ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Node sNode = new Node( collection, e.Current as ShallowNode );
						Property p = sNode.Properties.GetSingleProperty( "CS_Int" );
						if ( ( int )p.Value != -291 )
						{
							throw new ApplicationException( "Unexpected search results" );
						}

						// Should only return one.
						if ( e.MoveNext() )
						{
							throw new ApplicationException( "Unexpected search results" );
						}
					}
					else
					{
						throw new ApplicationException( "Unexpected search results" );
					}
				}
				finally
				{
					e.Dispose();
				}


				// Should return 291 and -291.
				e = ( ICSEnumerator )collection.Search( "CS_Int", -291, SearchOp.Greater_Equal ).GetEnumerator();
				try
				{
					int count = 0;
					while ( e.MoveNext() ) ++count;
					if ( count != 2 )
					{
						throw new ApplicationException( "Unexpected search results" );
					}
				}
				finally
				{
					e.Dispose();
				}


				// Should return -291.
				e = ( ICSEnumerator )collection.Search( "CS_Int", 281, SearchOp.Less_Equal ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Node sNode = new Node( collection, e.Current as ShallowNode );
						Property p = sNode.Properties.GetSingleProperty( "CS_Int" );
						if ( ( int )p.Value != -291 )
						{
							throw new ApplicationException( "Unexpected search results" );
						}

						// Should only be one.
						if ( e.MoveNext() )
						{
							throw new ApplicationException( "Unexpected search results" );
						}
					}
					else
					{
						throw new ApplicationException( "Unexpected search results" );
					}
				}
				finally
				{
					e.Dispose();
				}
			}
			finally
			{
				// Delete the collection.
				collection.Commit( collection.Delete() );
			}
		}


		/// <summary>
		/// Searches for various property types on a collection.
		/// </summary>
		[Test]
		public void PropertyStoreAndRetrieveTest()
		{

			// Create a collection to add properties to.
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				string ID = collection.ID;
			
				XmlDocument xmlTestDocument = new XmlDocument();
				xmlTestDocument.LoadXml( "<ObjectList><Object name='XmlTest' type='string' id='1234' /></ObjectList>" );
				Uri uri = new Uri( "http://google.com" );

				// Add the maximum value of each property type.
				collection.Properties.AddProperty( "CS_String", "Hello world!" );
				collection.Properties.AddProperty( "CS_Sbyte", SByte.MaxValue );
				collection.Properties.AddProperty( "CS_Byte", Byte.MaxValue );
				collection.Properties.AddProperty( "CS_Short", Int16.MaxValue );
				collection.Properties.AddProperty( "CS_Ushort", UInt16.MaxValue );
				collection.Properties.AddProperty( "CS_Int", Int32.MaxValue );
				collection.Properties.AddProperty( "CS_Uint", UInt32.MaxValue );
				collection.Properties.AddProperty( "CS_Long", Int64.MaxValue );
				collection.Properties.AddProperty( "CS_Ulong", UInt64.MaxValue );
				collection.Properties.AddProperty( "CS_Char", Char.MaxValue );
				collection.Properties.AddProperty( "CS_Float", Single.MaxValue );
				collection.Properties.AddProperty( "CS_Bool", true );
				collection.Properties.AddProperty( "CS_DateTime", DateTime.MaxValue );
				collection.Properties.AddProperty( "CS_Uri", uri );
				collection.Properties.AddProperty( "CS_XmlDocument", xmlTestDocument);
				collection.Properties.AddProperty( "CS_TimeSpan", new TimeSpan( DateTime.MaxValue.Ticks ) );
				collection.Properties.AddProperty( "CS_Enum", TestType.TestType1 );
				collection.Commit();

				// Force the object to be retreived from the data base.
				collection.Refresh( collection );

				// Get each property individually and make sure that the values are valid.
				Property p = collection.Properties.GetSingleProperty( "CS_String" );
				if ( ( string )p.Value != "Hello world!" )
				{
					throw new ApplicationException( "Add CS_String Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Sbyte" );
				if ( ( sbyte )p.Value != SByte.MaxValue )
				{
					throw new ApplicationException( "Add CS_Sbyte Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Byte" );
				if ( ( byte )p.Value != Byte.MaxValue )
				{
					throw new ApplicationException( "Add CS_Byte Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Short" );
				if ( ( short )p.Value != Int16.MaxValue )
				{
					throw new ApplicationException( "Add CS_Short Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Ushort" );
				if ( ( ushort )p.Value != UInt16.MaxValue )
				{
					throw new ApplicationException( "Add CS_Ushort Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Int" );
				if ( ( int )p.Value != Int32.MaxValue )
				{
					throw new ApplicationException( "Add CS_Int Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Uint" );
				if ( ( uint )p.Value != UInt32.MaxValue )
				{
					throw new ApplicationException( "Add CS_Uint Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Long" );
				if ( ( long )p.Value != Int64.MaxValue )
				{
					throw new ApplicationException( "Add CS_Long Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Ulong" );
				if ( ( ulong )p.Value != UInt64.MaxValue )
				{
					throw new ApplicationException( "Add CS_Ulong Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Char" );
				if ( ( char )p.Value != Char.MaxValue )
				{
					throw new ApplicationException( "Add CS_Char Value mismatch" );
				}

				// The Single.ToString() function looses some precision.
				p = collection.Properties.GetSingleProperty( "CS_Float" );
				if ( ( ( float )p.Value ).ToString() != Single.MaxValue.ToString() )
				{
					throw new ApplicationException( "Add CS_Float Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Bool" );
				if ( ( bool )p.Value != true )
				{
					throw new ApplicationException( "Add CS_Bool Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_DateTime" );
				if ( ( DateTime )p.Value != DateTime.MaxValue )
				{
					throw new ApplicationException( "Add CS_DateTime Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Uri" );
				if ( !( ( Uri )p.Value ).Equals( uri ) )
				{
					throw new ApplicationException( "Add CS_Uri Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_XmlDocument" );
				if ( ( ( XmlDocument )p.Value ).OuterXml != xmlTestDocument.OuterXml )
				{
					throw new ApplicationException( "Add CS_XmlDocument Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_TimeSpan" );
				if ( ( TimeSpan )p.Value != new TimeSpan( DateTime.MaxValue.Ticks ) )
				{
					throw new ApplicationException( "Add CS_TimeSpan Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Enum" );
				if ( ( TestType )p.Value != TestType.TestType1 )
				{
					throw new ApplicationException( "Add CS_Enum Value mismatch" );
				}

				// Modify the properties to their minimums and verify.
				xmlTestDocument.LoadXml( "<ObjectList><Object name = 'myObject' id = 'myId' type = 'Node' /></ObjectList>" );
				uri = new Uri( Directory.GetCurrentDirectory() );

				collection.Properties.ModifyProperty( "CS_String", "Hello world again!" );
				collection.Properties.ModifyProperty( "CS_Sbyte", SByte.MinValue );
				collection.Properties.ModifyProperty( "CS_Byte", Byte.MinValue );
				collection.Properties.ModifyProperty( "CS_Short", Int16.MinValue );
				collection.Properties.ModifyProperty( "CS_Ushort", UInt16.MinValue );
				collection.Properties.ModifyProperty( "CS_Int", Int32.MinValue );
				collection.Properties.ModifyProperty( "CS_Uint", UInt32.MinValue );
				collection.Properties.ModifyProperty( "CS_Long", Int64.MinValue );
				collection.Properties.ModifyProperty( "CS_Ulong", UInt64.MinValue );
				collection.Properties.ModifyProperty( "CS_Char", Char.MinValue );
				collection.Properties.ModifyProperty( "CS_Float", Single.MinValue );
				collection.Properties.ModifyProperty( "CS_Bool", false );
				collection.Properties.ModifyProperty( "CS_DateTime", DateTime.MinValue );
				collection.Properties.ModifyProperty( "CS_Uri", uri );
				collection.Properties.ModifyProperty( "CS_XmlDocument", xmlTestDocument);
				collection.Properties.ModifyProperty( "CS_TimeSpan", new TimeSpan( DateTime.MinValue.Ticks ) );
				collection.Properties.ModifyProperty( "CS_Enum", TestType.TestType2 );
				collection.Commit();

				// Force the object to be retreived from the data base.
				collection.Refresh( collection );

				// Get each property individually and make sure that the values are valid.
				p = collection.Properties.GetSingleProperty( "CS_String" );
				if ( ( string )p.Value != "Hello world again!" )
				{
					throw new ApplicationException( "Modify CS_String Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Sbyte" );
				if ( ( sbyte )p.Value != SByte.MinValue )
				{
					throw new ApplicationException( "Modify CS_Sbyte Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Byte" );
				if ( ( byte )p.Value != Byte.MinValue )
				{
					throw new ApplicationException( "Modify CS_Byte Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Short" );
				if ( ( short )p.Value != Int16.MinValue )
				{
					throw new ApplicationException( "Modify CS_Short Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Ushort" );
				if ( ( ushort )p.Value != UInt16.MinValue )
				{
					throw new ApplicationException( "Modify CS_Ushort Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Int" );
				if ( ( int )p.Value != Int32.MinValue )
				{
					throw new ApplicationException( "Modify CS_Int Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Uint" );
				if ( ( uint )p.Value != UInt32.MinValue )
				{
					throw new ApplicationException( "Modify CS_Uint Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Long" );
				if ( ( long )p.Value != Int64.MinValue )
				{
					throw new ApplicationException( "Modify CS_Long Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Ulong" );
				if ( ( ulong )p.Value != UInt64.MinValue )
				{
					throw new ApplicationException( "Modify CS_Ulong Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Char" );
				if ( ( char )p.Value != Char.MinValue )
				{
					throw new ApplicationException( "Modify CS_Char Value mismatch" );
				}

				// The Single.ToString() function looses some precision.
				p = collection.Properties.GetSingleProperty( "CS_Float" );
				if ( ( ( float )p.Value ).ToString() != Single.MinValue.ToString() )
				{
					throw new ApplicationException( "Modify CS_Float Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Bool" );
				if ( ( bool )p.Value != false )
				{
					throw new ApplicationException( "Modify CS_Bool Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_DateTime" );
				if ( ( DateTime )p.Value != DateTime.MinValue )
				{
					throw new ApplicationException( "Modify CS_DateTime Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Uri" );
				if ( ( ( Uri )p.Value ).ToString() != uri.ToString() )
				{
					throw new ApplicationException( "Modify CS_Uri Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_XmlDocument" );
				if ( ( ( XmlDocument )p.Value ).OuterXml != xmlTestDocument.OuterXml )
				{
					throw new ApplicationException( "Modify CS_XmlDocument Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_TimeSpan" );
				if ( ( TimeSpan )p.Value != new TimeSpan( DateTime.MinValue.Ticks ) )
				{
					throw new ApplicationException( "Modify CS_TimeSpan Value mismatch" );
				}

				p = collection.Properties.GetSingleProperty( "CS_Enum" );
				if ( ( TestType )p.Value != TestType.TestType2 )
				{
					throw new ApplicationException( "Modify CS_Enum Value mismatch" );
				}
			}
			finally
			{
				// Delete the collection.
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Deletes a Property from a Collection.
		/// </summary>
		[Test]
		public void PropertyMethodsTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				// First property to add.
				Property p1 = new Property( "CS_String", "This is the first string" );
				p1.Flags = 0x0010;
				collection.Properties.AddProperty( p1 );

				// Second property to add.
				Property p2 = new Property( "CS_String", "This is the second string" );
				collection.Properties.AddProperty( p2 );

				// Add a property with a similar name and make sure that it is not found.
				Property p3 = new Property( "CS_Strings", "This is a similar property" );
				collection.Properties.AddProperty( p3 );

				// Get a list of the CS_String properties on this collection.
				MultiValuedList mvl = collection.Properties.GetProperties( "CS_String" );

				// Should be 2 strings.
				int count = 0;
				IEnumerator e = mvl.GetEnumerator();
				while ( e.MoveNext() ) ++count;

				if ( count != 2 )
				{
					throw new ApplicationException( "Not all properties were returned" );
				}

				// Delete p2.
				p2.Delete();

				// Get the list again.  There should only be one.
				mvl = collection.Properties.GetProperties( "CS_String" );

				count = 0;
				e = mvl.GetEnumerator();
				while ( e.MoveNext() ) ++count;

				if ( count != 1 )
				{
					throw new ApplicationException( "Failed to delete property" );
				}

				// Delete all of the properties.
				collection.Properties.DeleteProperties( "CS_String" );

				// Now there should be none.
				mvl = collection.Properties.GetProperties( "CS_String" );

				count = 0;
				e = mvl.GetEnumerator();
				while ( e.MoveNext() ) ++count;

				if ( count != 0 )
				{
					throw new ApplicationException( "All properties not deleted" );
				}

				// Enumerate all of the properties.  There should be no ACE property.
				foreach ( Property p in collection.Properties )
				{
					if ( p.Name == "Ace" )
					{
						throw new ApplicationException( "Invisible property is showing up" );
					}
				}

				// Get a property that doesn't exist.
				Property p5 = collection.Properties.GetSingleProperty( "Test that property doesn't exist" );
				if ( p5 != null )
				{
					throw new ApplicationException( "Found a property that doesn't exist." );
				}

				// Get a property that is hidden.
				p5 = collection.Properties.GetSingleProperty( "Ace" );
				if ( p5 != null )
				{
					throw new ApplicationException( "Found a property that is hidden." );
				}
			}
			finally
			{
			}
		}

		/// <summary>
		/// Tries to add a reserved system property.
		/// </summary>
		[Test]
		[ExpectedException( typeof( InvalidOperationException ) )]
		public void AddSystemPropertyTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				// Shouldn't allow this.
				collection.Properties.AddProperty( "collectionid", "1234-5678-90" );
			}
			finally
			{
			}
		}


		/// <summary>
		/// Tests the file interface.
		/// </summary>
		[Test]
		public void FileTest()
		{
			string fileData = "How much wood can a woodchuck chuck if a woodchuck could chuck wood?";
			string rootDir = Path.Combine( Directory.GetCurrentDirectory(), "CS_TestCollection" );

			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				// Create a node to represent the collection root directory.
				Directory.CreateDirectory( rootDir );
				DirNode rootNode = new DirNode( collection, rootDir );

				// Create a file in the file system.
				string filePath = Path.Combine( rootDir, "Test.txt" );
				FileStream fs = new FileStream( filePath, FileMode.Create, FileAccess.Write, FileShare.None );
				StreamWriter sw = new StreamWriter( fs );
				sw.WriteLine( fileData );
				sw.Close();

				// Add this file to the collection, rooted at the collection.
				FileNode fileNode = new FileNode( collection, rootNode, "Test.txt" );				

				// Commit all changes.
				Node[] commitList = { collection, rootNode, fileNode };
				collection.Commit( commitList );

				// Get the file entry from the collection.
				ICSList entryList = collection.Search( BaseSchema.ObjectType, "FileNode", SearchOp.Equal );
				foreach ( ShallowNode sn in entryList )
				{
					FileNode fn = new FileNode( collection, sn );
					collection.Commit( collection.Delete( fn ) );
				}
			}
			finally
			{
				if ( Directory.Exists( rootDir ) )
				{
					Directory.Delete( rootDir, true );
				}

				// Delete the collection.
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Test the access control functionality.
		/// </summary>
		[Test]
		public void AccessControlTest()
		{
			Collection collection1 = new Collection( store, "CS_TestCollection1", store.LocalDomain );
			Collection collection2 = new Collection( store, "CS_TestCollection2", store.LocalDomain );
			Collection collection3;

			try
			{
				// Commit the collections.
				collection1.Commit();
				collection2.Commit();

				// Get a user that can be impersonated.
				Member member = new Member( "cameron", Guid.NewGuid().ToString(), Access.Rights.ReadWrite );
				collection1.Commit( member );

				collection3 = store.GetCollectionByID( collection1.ID );

				try
				{
					collection3.Impersonate( member );
					collection3.Properties.AddProperty( "DisplayName", "Access Collection" );
					collection3.Commit();

					try
					{
						// Try to change the collection ownership.
						collection3.Commit( collection3.ChangeOwner( member, Access.Rights.ReadOnly ) );
						throw new ApplicationException( "Change ownership access control check on impersonation failed" );
					}
					catch ( AccessException )
					{
						// This is expected.
						member.IsOwner = false;
					}

					// test the access search
					ICSList members = collection3.GetMembersByRights(Access.Rights.ReadWrite);
									
					if ( !members.GetEnumerator().MoveNext() )
					{
						throw new ApplicationException( "Members were not found during a rights search." );
					}
				}
				finally
				{
					collection3.Revert();
				}

				// test the name search
				ICSList list = store.GetCollectionsByName("CS_", SearchOp.Begins);
				
				if ( !list.GetEnumerator().MoveNext() )
				{
					throw new ApplicationException( "Collections were not found during a name search." );
				}

				try
				{
					// Change the member's rights to have access to change members.
					member.Rights = Access.Rights.Admin;
					collection1.Commit( member );
					collection1.Impersonate( member );
					try
					{
						// Try and down-grade the owner's rights.
						Member owner = collection1.Owner;
						owner.Rights = Access.Rights.ReadOnly;
						collection1.Commit( owner );
						throw new ApplicationException( "Block owner rights change failed" );
					}
					finally
					{
						collection1.Revert();
					}
				}
				catch ( SimiasException )
				{
					// This is expected.
				}

				// Change the ownership on the collection.
				collection3.Commit( collection3.ChangeOwner( member, Access.Rights.ReadOnly ) );

				// Make sure that it changed.
				if ( collection3.Owner.UserID != member.UserID )
				{
					throw new ApplicationException( "Collection ownership did not change" );
				}

				// Set world rights on collection2.
				Member world = new Member( "World", Access.World, Access.Rights.ReadWrite );
				collection2.Commit( world );
			}
			finally
			{
				collection1.Commit( collection1.Delete() );
				collection2.Commit( collection2.Delete() );
			}
		}

		/// <summary>
		/// Test the serialization/deserialization.
		/// </summary>
		[Test]
		public void SerializeTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				// Commit the collection.
				collection.Commit();

				// Get the who I am.
				Member member = collection.GetCurrentMember();

				// Serialize the collection object.
				MemoryStream cms = new MemoryStream();
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize( cms, new Node( collection ) );

				MemoryStream oms = new MemoryStream();
				bf.Serialize( oms, new Node( collection.Owner ) );

				// Delete the collection so it can be restored.
				collection.Commit( collection.Delete() );

				// Reset the stream before deserializing.
				cms.Position = 0;
				oms.Position = 0;

				// Restore the collection.
				collection = new Collection( store, ( Node )bf.Deserialize( cms ) );
				Member owner = new Member( ( Node )bf.Deserialize( oms ) );
				collection.ImportNode( collection, true, collection.LocalIncarnation );
				collection.ImportNode( owner, true, owner.LocalIncarnation );
				collection.IncarnationUpdate = 2;
				owner.IncarnationUpdate = 2;
				collection.Impersonate( member );
				collection.Commit( new Node[] { collection, owner } );
			}
			finally
			{
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Tests the get-by-name functionality.
		/// </summary>
		[Test]
		public void GetByNameTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				// Commit the collection.
				collection.Commit();

				// Get the collection by name.
				ICSList list = store.GetCollectionsByName( "CS_TestCollection" );
				ICSEnumerator e = list.GetEnumerator() as ICSEnumerator;
				if ( e.MoveNext() == false )
				{
					throw new ApplicationException( "Cannot find collection by name" );
				}

				collection = new Collection( store, e.Current as ShallowNode );
				e.Dispose();

				// Create a node in the collection.
				collection.Commit( new Node( "CS_Node" ) );

				// Get the node by name.
				list = collection.GetNodesByName( "CS_Node" );
				e = list.GetEnumerator() as ICSEnumerator;
				if ( !e.MoveNext() )
				{
					throw new ApplicationException( "Cannot get node by name" );
				}

				e.Dispose();
			}
			finally
			{
				// Delete the collection.
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Tests the ability to abort pre-committed changes on a collection.
		/// </summary>
		[Test]
		public void AbortTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				// Commit the collection.
				collection.Commit();

				// Change the collection.
				collection.Properties.AddProperty( "New Property", "This is a test" );

				// Abort the change.
				collection.Abort( collection );

				// The added property should be gone.
				Property p = collection.Properties.GetSingleProperty( "New Property" );
				if ( p != null )
				{
					throw new ApplicationException( "Added property did not rollback" );
				}
			}
			finally
			{
				// Delete the collection.
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Tests the incarnation values that they get updated correctly.
		/// </summary>
		[Test]
		public void TestIncarnationValues()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				// Create a node.
				Node nodeA = new Node( "CS_NodeA" );

				// The incarnation number on the collection and child node should be zero.
				if ( ( collection.LocalIncarnation != 0 ) || ( nodeA.LocalIncarnation != 0 ) )
				{
					throw new ApplicationException( "Local incarnation value is not zero." );
				}

				// Commit the collection which should increment the values both to one.
				Node[] commitList = { collection, nodeA };
				collection.Commit( commitList );

				// The incarnation number on the collection and child node should be one.
				if ( ( collection.LocalIncarnation != 1 ) || ( nodeA.LocalIncarnation != 1 ) )
				{
					throw new ApplicationException( "Local incarnation value is not one." );
				}

				// Change just the node and commit it.
				nodeA.Properties.AddProperty( "Test Property", "This is a test." );
				collection.Commit( nodeA );

				collection.Refresh( nodeA );
				collection.Refresh();

				// The incarnation number on the collection should be one and child node should be two.
				if ( ( collection.LocalIncarnation != 1 ) || ( nodeA.LocalIncarnation != 2 ) )
				{
					Console.WriteLine( "collection incarnation = {0}", collection.LocalIncarnation );
					Console.WriteLine( "nodeA incarnation = {0}", nodeA.LocalIncarnation );
					throw new ApplicationException( "Local incarnation value is not two." );
				}

				// Commit just the collection with no changes to it.
				collection.Commit();

				collection.Refresh( nodeA );
				collection.Refresh();

				// The incarnation number on the collection should be 1 and the node should be two.
				if ( ( collection.LocalIncarnation != 1 ) || ( nodeA.LocalIncarnation != 2 ) )
				{
					throw new ApplicationException( "Local incarnation value is not one and two." );
				}

				// Add another new child node and commit it.
				Node nodeB = new Node( "CS_NodeB" );
				collection.Commit( nodeB );

				collection.Refresh( nodeA );
				collection.Refresh( nodeB );
				collection.Refresh();

				// The incarnation number on the collection should be 1 and nodeA should be two
				// and nodeB should be one.
				if ( ( collection.LocalIncarnation != 1 ) || ( nodeA.LocalIncarnation != 2 ) || ( nodeB.LocalIncarnation != 1 ) )
				{
					throw new ApplicationException( "Local incarnation value is not one, two, and one." );
				}

				// Finally, set the master version of the collection.
				collection.ImportNode( collection, false, collection.LocalIncarnation );
				collection.IncarnationUpdate = 1;
				collection.Commit();
				collection.Refresh();

				// The incarnation number on the collection should be 1.1
				if ( ( collection.LocalIncarnation != 1 ) && ( collection.MasterIncarnation != 1 ) )
				{
					throw new ApplicationException( "Local incarnation value is not one and master incarnation is not one." );
				}
			}
			finally
			{
				// Delete the collection.
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		///  Tests the finding all nodes asssociated with a file.
		/// </summary>
		[Test]
		public void FileToNodeTest()
		{
			string rootDir = Path.Combine( Directory.GetCurrentDirectory(), "CS_TestCollection" );

			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );

			try
			{
				// Add a root directory.
				Directory.CreateDirectory( rootDir );
				DirNode rootNode = new DirNode( collection, rootDir );

				// Add a file to the collection object.
				FileNode fileNode = new FileNode( collection, rootNode, "Test.txt" );

				Node[] commitList = { collection, rootNode, fileNode };
				collection.Commit( commitList );

				// See if the node can be located.
				ICSList results = collection.Search( BaseSchema.ObjectName, fileNode.Name, SearchOp.Equal );
				ICSEnumerator e = results.GetEnumerator() as ICSEnumerator;
				if ( !e.MoveNext() )
				{
					throw new ApplicationException( "Cannot find associated node." );
				}

				e.Dispose();
			}
			finally
			{
				if ( Directory.Exists( rootDir ) )
				{
					Directory.Delete( rootDir, true );
				}

				// Delete the collection.
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Tests the merge attribute part of CollectionStore.
		/// </summary>
		[Test]
		public void MergeNodeTest()
		{
			// Get a second handle to the current store.
			Store mergeStore = Store.GetStore();

			// Create a collection using the primary store handle.
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );

			try
			{
				// Commit the collection.
				collection.Commit();

				// Get a handle to the store through the merge store.
				Collection mergeCollection = mergeStore.GetCollectionByID( collection.ID );

				// Add a property through the primary store handle.
				collection.Properties.AddProperty( "CS_TestMergeProperty", "This is a test" );
				collection.Commit();

				// The merge store should not see this property.
				Property p = mergeCollection.Properties.GetSingleProperty( "CS_TestMergeProperty" );
				if ( p != null )
				{
					throw new ApplicationException( "Unexpected property." );
				}

				// Add a property through the merge handle.
				mergeCollection.Properties.AddProperty( "CS_TestNewProperty", "This is a new property" );
				mergeCollection.Commit();

				// Should be able to see both properties now through the merge handle.
				mergeCollection.Refresh( mergeCollection );
				p = mergeCollection.Properties.GetSingleProperty( "CS_TestMergeProperty" );
				if ( p == null )
				{
					throw new ApplicationException( "Cannot get merged property." );
				}

				p = mergeCollection.Properties.GetSingleProperty( "CS_TestNewProperty" );
				if ( p == null )
				{
					throw new ApplicationException( "Cannot get merged property." );
				}

				// Now modify the same single value.
				collection.Properties.AddProperty( "CS_TestModifyProperty", ( int )1 );
				collection.Commit();

				mergeCollection.Refresh( mergeCollection );

				collection.Properties.ModifyProperty( "CS_TestModifyProperty", ( int ) 4 );
				collection.Commit();

				mergeCollection.Properties.ModifyProperty( "CS_TestModifyProperty", ( int )2 );
				mergeCollection.Commit();

				if ( ( int )collection.Properties.GetSingleProperty( "CS_TestModifyProperty" ).Value != 4 )
				{
					throw new ApplicationException( "Value unexpectedly modified." );
				}

				if ( ( int )mergeCollection.Properties.GetSingleProperty( "CS_TestModifyProperty" ).Value != 2 )
				{
					throw new ApplicationException( "Value unexpectedly modified." );
				}

				// Refresh the collection and the value should change to 2.
				collection.Refresh( collection );
				if ( ( int )collection.Properties.GetSingleProperty( "CS_TestModifyProperty" ).Value != 2 )
				{
					throw new ApplicationException( "Value unexpectedly modified." );
				}

				// Now modify the property to be a multivalued property.
				p = collection.Properties.GetSingleProperty( "CS_TestModifyProperty" );
				p.SetValue( ( int )5 );
				p.MultiValuedProperty = true;
				collection.Commit();

				// Update to the latest.
				mergeCollection.Refresh( mergeCollection );

				// Modify after the commit.
				collection.Properties.ModifyProperty( "CS_TestModifyProperty", ( int ) 6 );
				collection.Commit();

				mergeCollection.Properties.ModifyProperty( "CS_TestModifyProperty", ( int )7 );
				mergeCollection.Commit();

				// Should have two properties.
				collection.Refresh( collection );
				MultiValuedList mvl = collection.Properties.GetProperties( "CS_TestModifyProperty" );
				if ( mvl.Count != 2 )
				{
					throw new ApplicationException( "Invalid property count." );
				}

				// Change the name of the collection.
				collection.Name = "New Collection Name";
				collection.Commit();
				mergeCollection.Refresh( mergeCollection );

				// Should be able to see the new name.
				if ( mergeCollection.Name != collection.Name )
				{
					throw new ApplicationException( "Invalid name." );
				}

				collection.Name = "Another new name";
				collection.Abort( collection );
				if ( collection.Name == "Another new name" )
				{
					throw new ApplicationException( "Name change was not aborted." );
				}
			}
			finally
			{
				// Delete the collection.
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		///  Tests the Enumeration of nodes in a collection.
		/// </summary>
		[Test]
		public void EnumerateNodesTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				Node[] commitList = { collection,
									  new Node( "CS_Node1" ),
									  new Node( "CS_Node2" ),
									  new Node( "CS_Node3" ),
									  new Node( "CS_Node4" ),
									  new Node( "CS_Node5" ),
									  new Node( "CS_Node6" ),
									  new Node( "CS_Node7" ),
									  new Node( "CS_Node8" ),
									  new Node( "CS_Node9" ) };

				collection.Commit( commitList );

				int count = 0;
				foreach( ShallowNode sn in collection )
				{
					foreach( Node node in commitList )
					{
						if ( node.ID == sn.ID )
						{
							++count;
							break;
						}
					}
				}

				if ( count != 10 )
				{
					throw new ApplicationException( "Enumeration failed." );
				}
			}
			finally
			{
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		///  Tests the StoreFileNode objects.
		/// </summary>
		[Test]
		public void StoreFileNodeTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				// Create a file in the file system.
				string filePath = Path.Combine( Directory.GetCurrentDirectory(), "StoreFileNodeTest.txt" );
				FileStream fs = new FileStream( filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None );
				StreamWriter sw = new StreamWriter( fs );
				sw.WriteLine( "This is a test" );
				sw.Flush();

				// Position the stream back to the beginning.
				fs.Position = 0;

				// Create the Node object.
				StoreFileNode sfn = new StoreFileNode( "MyFile", fs );

				// The file should not exist yet.
				if ( File.Exists( sfn.GetFullPath( collection ) ) )
				{
					throw new ApplicationException( "Store managed file should not exist yet." );
				}

				Node[] nodeList = { collection, sfn };
				collection.Commit( nodeList );

				// Now the file should exist.
				if ( !File.Exists( sfn.GetFullPath( collection ) ) )
				{
					throw new ApplicationException( "Store managed file should exist." );
				}

				// Delete the node object and the file should still exist.
				collection.Delete( sfn );

				// The file should still exist.
				if ( !File.Exists( sfn.GetFullPath( collection ) ) )
				{
					throw new ApplicationException( "Store managed file should exist after delete." );
				}

				// Commit the change.
				collection.Commit( sfn );

				// The file should not exist.
				if ( File.Exists( sfn.GetFullPath( collection ) ) )
				{
					throw new ApplicationException( "Store managed file should not exist after delete and commit." );
				}
			}
			finally
			{
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Test the collision operations.
		/// </summary>
		[Test]
		public void CollisionTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				// Commit the collection.
				collection.Commit();

				// Create a node object.
				Node node = new Node( "CS_TestNode" );
				collection.Commit( node );

				// Create a collision Node that represents the collection object and commit it to the collision
				// collection.
				Node collisionNode = collection.CreateCollision( node, false );
				collection.Commit( collisionNode );

				collisionNode = collection.CreateCollision( node, false );
				collection.Commit( collisionNode );

				// Should have a collision.
				if ( !collection.HasCollisions() )
				{
					throw new ApplicationException( "Collision node not created." );
				}

				// Find the collision Node.
				ICSList colList = collection.GetCollisions();
				ICSEnumerator e = colList.GetEnumerator() as ICSEnumerator;
				if ( e.MoveNext() )
				{
					if ( ( e.Current as ShallowNode ).ID != node.ID )
					{
						throw new ApplicationException( "Found wrong collision node." );
					}

					if ( e.MoveNext() )
					{
						throw new ApplicationException( "Collision node exception." );
					}
				}
				else
				{
					throw new ApplicationException( "Collision node exception." );
				}

				e.Dispose();


				// Reconstitute the collision Node and get the Node that it represents.
				Node collectionNode = collection.GetNodeFromCollision( collisionNode );
				if ( collisionNode.ID != collectionNode.ID )
				{
					throw new ApplicationException( "Cannot find proper Node from collision." );
				}

				// Delete the collision node.
				collection.Commit( collection.DeleteCollision( collisionNode ) );

				// Should not be anymore collisions.
				if ( collection.HasCollisions() )
				{
					throw new ApplicationException( "Cannot delete collision node." );
				}

				// Create a file collision.
				collisionNode = collection.CreateCollision( collisionNode, true );
				collection.Commit( collisionNode );

				// Get the collision back.
				collectionNode = collection.GetNodeFromCollision( collisionNode );
				if ( collectionNode != null )
				{
					throw new ApplicationException( "File conflict returned wrong collision." );
				}

				// Resolve the collision node.
				collectionNode = collection.ResolveCollision( collisionNode, collisionNode.LocalIncarnation, true );
				collection.Commit( collectionNode );

				// Should not be anymore collisions.
				if ( collection.HasCollisions() )
				{
					throw new ApplicationException( "Cannot delete collision node." );
				}
			}
			finally
			{
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Tests the GetCollectionBy... methods.
		/// </summary>
		[Test]
		public void GetCollectionByTest()
		{
			string userID = Guid.NewGuid().ToString();
			string domainID = Guid.NewGuid().ToString();

			// Create a new domain.
			Domain domain = new Domain( store, "Okalhoma", domainID, "Test domain", SyncRoles.Master, Domain.ConfigurationType.ClientServer );
			Member domainOwner = new Member( "wally", userID, Access.Rights.Admin );
			domainOwner.IsOwner = true;
			domain.Commit( new Node[] { domain, domainOwner } );
			store.AddDomainIdentity( domainID, userID ); 

			// Create a whole bunch of collections.
			Collection[] c = new Collection[ 10 ];
			for ( int i = 0; i < 10; ++i )
			{
				c[ i ] = new Collection( store, String.Format( "{0}", i + 1 ), domainID );
				c[ i ].Commit();
			}

			try
			{
				string newUserID = Guid.NewGuid().ToString();

				// Add a new member to some of the collections.
				for ( int i = 0; i < 5; ++i )
				{
					Member member = new Member( "cameron", newUserID, Access.Rights.ReadOnly );
					c[ i ].Commit( member );
				}

				ICSList list = store.GetCollectionsByUser( newUserID );
				foreach ( ShallowNode sn in list )
				{
					Collection col = new Collection( store, sn );
					if ( Convert.ToInt32( col.Name, 10 ) > 5 )
					{
						throw new ApplicationException( "GetCollectionsByUser() returned wrong collections" );
					}
				}

				// Search for all collections in the default domain.
				list = store.GetCollectionsByDomain( domainID );
				if ( list.Count != 11 )
				{
					throw new ApplicationException( "GetCollectionsByDomain() did not return all of the collections." );
				}

				// Add some new types.
				for ( int i = 0; i < 5; ++i )
				{
					c[ i * 2 ].SetType( c[ i * 2 ], "TestType" );
					c[ i * 2 ].Commit();
				}

				// Search by type.
				list = store.GetCollectionsByType( "TestType" );
				if ( list.Count != 5 )
				{
					throw new ApplicationException( "GetCollectionsByType() did not return all of the collections." );
				}
			}
			finally
			{
				for ( int i = 0; i < 10; ++i )
				{
					c[ i ].Commit( c[ i ].Delete() );
				}

				store.DeleteDomainIdentity( domainID );
			}
		}

		/// <summary>
		/// Test the storage of file object in a collection.
		/// </summary>
		[Test]
		public void CollectionStorageTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );

			string rootDir = Path.Combine( Directory.GetCurrentDirectory(), "CS_TestCollection" );
			if ( !Directory.Exists( rootDir ) ) Directory.CreateDirectory( rootDir );

			try
			{
				long fileSize = 0;
				ArrayList nodeList = new ArrayList();

				// Add the collection object to the commit list.
				nodeList.Add( collection );

				// Create a dirNode for the rootDir.
				DirNode dirNode = new DirNode( collection, rootDir );
				nodeList.Add( dirNode );

				Random ran = new Random();

				// Create a bunch of files of random size.
				for ( int i = 0; i < 10; ++i )
				{
					string fileName = String.Format( "Test{0}.bin", i );
					using ( FileStream fs = new FileStream( Path.Combine( rootDir, fileName ), FileMode.Create, FileAccess.Write ) )
					{
						byte[] array = new byte[ ran.Next( 128 * 1024 ) ];
						ran.NextBytes( array );
						fs.Write( array, 0, array.Length );
						fileSize += array.Length;
					}

					// Add this file to the collection.
					FileNode fn = new FileNode( collection, dirNode, fileName );
					nodeList.Add( fn );
				}

				// Commit all of the nodes.
				collection.Commit( nodeList.ToArray( typeof( Node ) ) as Node[] );

				// Make sure that the sizes compare.
				if ( collection.StorageSize != fileSize )
				{
					throw new ApplicationException( "Added file sizes do not match collection storage size." );
				}

				// Now add some store managed files.
				for ( int i = 0; i < 10; ++i )
				{
					string fileName = String.Format( "SMFTest{0}.bin", i );
					using ( FileStream fs = new FileStream( Path.Combine( rootDir, fileName ), FileMode.Create, FileAccess.ReadWrite, FileShare.None ) )
					{
						byte[] array = new byte[ ran.Next( 128 * 1024 ) ];
						ran.NextBytes( array );
						fs.Write( array, 0, array.Length );
						fileSize += array.Length;

						fs.Position = 0;
						StoreFileNode sfn = new StoreFileNode( fileName, fs );
						collection.Commit( sfn );
					}
				}

				// Refresh the collection and make sure the sizes still match.
				collection.Refresh();
				if ( collection.StorageSize != fileSize )
				{
					throw new ApplicationException( "Added store managed file sizes do not match collection storage size." );
				}

				// Remove a the store managed files.
				foreach( ShallowNode sn in collection )
				{
					if ( sn.Type == NodeTypes.StoreFileNodeType )
					{
						StoreFileNode sfn = new StoreFileNode( collection, sn );
						fileSize -= sfn.Length;
						collection.Commit( collection.Delete( sfn ) );

						// Make sure the sizes still match.
						collection.Refresh();
						if ( collection.StorageSize != fileSize )
						{
							throw new ApplicationException( "Deleted store managed file sizes do not match collection storage size." );
						}
					}
				}

				// Finally, modify an existing FileNode and make sure that the size changes. We don't need to modify
				// the file in the file system, just change the length on the FileNode and commit.
				FileNode fileNode = collection.GetSingleNodeByType( NodeTypes.FileNodeType ) as FileNode;
				fileNode.Length += 100000;
				collection.Commit( fileNode );

				collection.Refresh();
				if ( collection.StorageSize != ( fileSize + 100000 ) )
				{
					throw new ApplicationException( "File size update does not match collection storage." );
				}
			}
			finally
			{
				if ( Directory.Exists( rootDir ) ) Directory.Delete( rootDir, true );
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Tests the disk space quota functionality.
		/// </summary>
		[Test]
		public void DiskQuotaTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			collection.Commit();

			// Create a dir entry.
			DirNode dn = new DirNode( collection, Directory.GetCurrentDirectory() );
			collection.Commit( dn );

			try
			{
				Member member = collection.GetCurrentMember();

				// Create a system-wide disk space policy.
				DiskSpaceQuota.Create( store.LocalDomain, 2048 );

				// Make sure that there is a limit set.
				if ( DiskSpaceQuota.GetLimit( store.LocalDomain ) != 2048 )
				{
					throw new ApplicationException( "Domain disk quota not set." );
				}

				// Get a quota object.
				DiskSpaceQuota dsq = DiskSpaceQuota.Get( member );
				
				// Check the aggregate limit.
				if ( dsq.Limit != 2048 )
				{
					throw new ApplicationException( "Aggregate domain disk quota not set." );
				}

				// Now apply the quota so that it will pass.
				if ( dsq.Allowed( 1024 ) == false )
				{
					throw new ApplicationException( "Domain disk quota failed." );
				}

				// Now apply the quota so that it will fail.
				if ( dsq.Allowed( 2049 ) == true )
				{
					throw new ApplicationException( "Domain disk quota failed." );
				}


				// Create a PO box for the member.
				POBox.POBox.GetPOBox( store, store.LocalDomain, member.UserID );

				// Apply a quota on the member.
				DiskSpaceQuota.Create( member, 1024 );

				// Make sure that there is a limit set.
				if ( DiskSpaceQuota.GetLimit( member ) != 1024 )
				{
					throw new ApplicationException( "Member disk quota not set." );
				}

				// Get a quota object.
				dsq = DiskSpaceQuota.Get( member );
				
				// Check the aggregate limit.
				if ( dsq.Limit != 1024 )
				{
					throw new ApplicationException( "Aggregate member disk quota not set." );
				}

				// Now apply the quota so that it will pass.
				if ( dsq.Allowed( 512 ) == false )
				{
					throw new ApplicationException( "Member disk quota failed." );
				}

				// Now apply the quota so that it will fail.
				if ( dsq.Allowed( 513 ) == true )
				{
					throw new ApplicationException( "Member disk quota failed." );
				}


				// Apply a quota on the collection.
				DiskSpaceQuota.Create( collection, 512 );

				// Make sure that there is a limit set.
				if ( DiskSpaceQuota.GetLimit( collection ) != 512 )
				{
					throw new ApplicationException( "Collection disk quota not set." );
				}

				// Get a quota object.
				dsq = DiskSpaceQuota.Get( member, collection );
				
				// Check the aggregate limit.
				if ( dsq.Limit != 512 )
				{
					throw new ApplicationException( "Aggregate collection disk quota not set." );
				}

				// Now apply the quota so that it will pass.
				if ( dsq.Allowed( 256 ) == false )
				{
					throw new ApplicationException( "Collection disk quota failed." );
				}

				// Now apply the quota so that it will fail.
				if ( dsq.Allowed( 257 ) == true )
				{
					throw new ApplicationException( "Collection disk quota failed." );
				}

				// Reset the member's quota lower.
				DiskSpaceQuota.Create( member, 128 );

				// Get a quota object.
				dsq = DiskSpaceQuota.Get( member, collection );
				
				// Check the aggregate limit.
				if ( dsq.Limit != 128 )
				{
					throw new ApplicationException( "Aggregate member disk quota not set." );
				}

				// Check the available space.
				if ( dsq.AvailableSpace != 128 )
				{
					throw new ApplicationException( "Invalid available space." );
				}

				// Now apply the quota so that it will pass.
				if ( dsq.Allowed( 128 ) == false )
				{
					throw new ApplicationException( "Member disk quota failed." );
				}

				// Add a 128 byte file entry with out a file, just so it will be counted against the quota.
				FileNode fn = new FileNode( collection, dn, "TempFile" );
				fn.Length = 128;
				collection.Commit( fn );

				// Now check available space.
				if ( dsq.AvailableSpace != 0 )
				{
					throw new ApplicationException( "Invalid available space." );
				}

				// Now apply the quota so that it will fail.
				if ( dsq.Allowed( 1 ) == true )
				{
					throw new ApplicationException( "Member disk quota failed." );
				}
			}
			finally
			{
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Tests the file type filter functionality.
		/// </summary>
		[Test]
		public void FileTypeFilterTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			collection.Commit();

			try
			{
				Member member = collection.GetCurrentMember();
				FileTypeEntry[] dfte = new FileTypeEntry[] { new FileTypeEntry( ".mP3$", false, true ), 
															 new FileTypeEntry( ".avi$", false ) };

				// Create a system-wide file filter policy.
				FileTypeFilter.Create( store.LocalDomain, dfte );

				// Get a filter object.
				FileTypeFilter ftf = FileTypeFilter.Get( member );
				
				// Now apply the filter so that it will pass.
				if ( ftf.Allowed( "myfile.txt" ) == false )
				{
					throw new ApplicationException( "Domain file type filter failed." );
				}

				// Now apply the filter so that it will fail.
				if ( ftf.Allowed( "myfile.mp3" ) == true )
				{
					throw new ApplicationException( "Domain file type filter failed." );
				}

				// Create an additive filter.
				FileTypeEntry[] lfte = new FileTypeEntry[] { new FileTypeEntry( ".mov$", false, true ) };

				// Apply a file type filter on the current user on this machine.
				FileTypeFilter.Create( lfte );

				// Get a filter object.
				ftf = FileTypeFilter.Get( member );
				
				// Check the aggregate list.
				if ( ftf.FilterList.Length != 3 )
				{
					throw new ApplicationException( "Aggregate member filter not set." );
				}

				// Now apply the filter so that it will pass.
				if ( ftf.Allowed( "myfile.txt" ) == false )
				{
					throw new ApplicationException( "Member filter failed." );
				}

				// Now apply the filter so that it will fail.
				if ( ftf.Allowed( "myfile.mov" ) == true )
				{
					throw new ApplicationException( "Member filter failed." );
				}


				// Apply a filter on the collection.
				FileTypeEntry[] cfte = new FileTypeEntry[] { new FileTypeEntry( ".wav$", false, true ) };
				FileTypeFilter.Create( collection, cfte );

				// Get a quota object.
				ftf = FileTypeFilter.Get( member, collection );
				
				// Check the aggregate list.
				if ( ftf.FilterList.Length != 4 )
				{
					throw new ApplicationException( "Aggregate collection filter not set." );
				}

				// Now apply the filter so that it will pass.
				if ( ftf.Allowed( "myfile.txt" ) == false )
				{
					throw new ApplicationException( "Collection filter failed." );
				}

				// Now apply the filter so that it will fail.
				if ( ftf.Allowed( "myfile.wav" ) == true )
				{
					throw new ApplicationException( "Collection filter failed." );
				}


				// Create a POBox for the user.
				POBox.POBox.GetPOBox( store, store.LocalDomain, member.UserID );

				// Reset the member's filter to allow only .doc files.
				FileTypeEntry[] mfte = new FileTypeEntry[] { new FileTypeEntry( ".doc$", true, true ) };
				FileTypeFilter.Create( member, mfte );

				// Get a filter object.
				ftf = FileTypeFilter.Get( member, collection );
				
				// Now apply the filter so that it will pass.
				if ( ftf.Allowed( "myfile.doc" ) == false )
				{
					throw new ApplicationException( "Member filter failed." );
				}

				// Now apply the filter so that it will fail.
				if ( ftf.Allowed( "myfile.txt" ) == true )
				{
					throw new ApplicationException( "Member filter failed." );
				}

				// Remove all policies.
				FileTypeFilter.Delete( store.LocalDomain );
				FileTypeFilter.Delete( member );
				FileTypeFilter.Delete();
			}
			finally
			{
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Tests the file type filter functionality.
		/// </summary>
		[Test]
		public void FileTypeFilterTest2()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			collection.Commit();

			try
			{
				// Clean all policies off 

				Member member = collection.GetCurrentMember();
				FileTypeEntry[] dfte = new FileTypeEntry[] { new FileTypeEntry( ".bmp$", true ), 
															 new FileTypeEntry( ".jpg$", true ) };

				// Create a system-wide file filter policy.
				FileTypeFilter.Create( store.LocalDomain, dfte );

				// Get a filter object.
				FileTypeFilter ftf = FileTypeFilter.Get( member );
				
				// Now apply the filter so that it will pass.
				if ( ftf.Allowed( "myfile.bmp" ) == false )
				{
					throw new ApplicationException( "Domain file type filter failed." );
				}

				// Now apply the filter so that it will pass.
				if ( ftf.Allowed( "myfile.jpg" ) == false )
				{
					throw new ApplicationException( "Domain file type filter failed." );
				}

				// Now apply the filter so that it will fail.
				if ( ftf.Allowed( "myfile.txt" ) == true )
				{
					throw new ApplicationException( "Member filter failed." );
				}

				FileTypeFilter.Delete( store.LocalDomain );
			}
			finally
			{
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Tests the file size filter functionality.
		/// </summary>
		[Test]
		public void FileSizeFilterTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			collection.Commit();

			try
			{
				Member member = collection.GetCurrentMember();

				// Create a system-wide file size limit policy.
				FileSizeFilter.Create( store.LocalDomain, 2048 );

				// Make sure that there is a limit set.
				if ( FileSizeFilter.GetLimit( store.LocalDomain ) != 2048 )
				{
					throw new ApplicationException( "File size limit not set." );
				}

				// Get a filter object.
				FileSizeFilter fsf = FileSizeFilter.Get( member );
				
				// Check the aggregate limit.
				if ( fsf.Limit != 2048 )
				{
					throw new ApplicationException( "Aggregate file size limit not set." );
				}

				// Now apply the size filter so that it will pass.
				if ( fsf.Allowed( 2048 ) == false )
				{
					throw new ApplicationException( "Domain file size limit failed." );
				}

				// Now apply the size filter so that it will fail.
				if ( fsf.Allowed( 2049 ) == true )
				{
					throw new ApplicationException( "Domain file size limit failed." );
				}


				// Create a PO box for the member.
				POBox.POBox.GetPOBox( store, store.LocalDomain, member.UserID );

				// Apply a file size filter on the member.
				FileSizeFilter.Create( member, 1024 );

				// Make sure that there is a limit set.
				if ( FileSizeFilter.GetLimit( member ) != 1024 )
				{
					throw new ApplicationException( "Member file size limit not set." );
				}

				// Get a file size filter object.
				fsf = FileSizeFilter.Get( member );
				
				// Check the aggregate limit.
				if ( fsf.Limit != 1024 )
				{
					throw new ApplicationException( "Aggregate member file size limit not set." );
				}

				// Now apply the size so that it will pass.
				if ( fsf.Allowed( 1024 ) == false )
				{
					throw new ApplicationException( "Member file size limit failed." );
				}

				// Now apply the size so that it will fail.
				if ( fsf.Allowed( 1025 ) == true )
				{
					throw new ApplicationException( "Member file size limit failed." );
				}


				// Apply a file size limit on the collection.
				FileSizeFilter.Create( collection, 512 );

				// Make sure that there is a limit set.
				if ( FileSizeFilter.GetLimit( collection ) != 512 )
				{
					throw new ApplicationException( "Collection file size limit not set." );
				}

				// Get a file size limit object.
				fsf = FileSizeFilter.Get( member, collection );
				
				// Check the aggregate limit.
				if ( fsf.Limit != 512 )
				{
					throw new ApplicationException( "Aggregate collection file size limit not set." );
				}

				// Now apply the size so that it will pass.
				if ( fsf.Allowed( 512 ) == false )
				{
					throw new ApplicationException( "Collection file size limit failed." );
				}

				// Now apply the size so that it will fail.
				if ( fsf.Allowed( 513 ) == true )
				{
					throw new ApplicationException( "Collection file size limit failed." );
				}

				// Reset the member's quota lower.
				FileSizeFilter.Create( member, 128 );

				// Get a file size limit object.
				fsf = FileSizeFilter.Get( member, collection );
				
				// Check the aggregate limit.
				if ( fsf.Limit != 128 )
				{
					throw new ApplicationException( "Aggregate member file size limit not set." );
				}

				// Remove the file size limit by setting it to zero.
				FileSizeFilter.Create( member, 0 );
			}
			finally
			{
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Tests the sync interval functionality.
		/// </summary>
		[Test]
		public void SyncIntervalTest()
		{
			// Remove the default workstation sync policy.
			SyncInterval.Delete();

			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			collection.Commit();

			try
			{
				Member member = collection.GetCurrentMember();

				// Create a system-wide sync interval policy.
				SyncInterval.Create( store.LocalDomain, 120 );

				// Make sure that there is an interval set.
				if ( SyncInterval.GetInterval( store.LocalDomain ) != 120 )
				{
					throw new ApplicationException( "Domain sync interval not set." );
				}

				// Get a sync interval object.
				SyncInterval si = SyncInterval.Get( member );
				
				// Check the aggregate interval.
				if ( si.Interval != 120 )
				{
					throw new ApplicationException( "Aggregate interval limit not set." );
				}

				// Create a PO box for the member.
				POBox.POBox.GetPOBox( store, store.LocalDomain, member.UserID );

				// Apply a sync interval on the member.
				SyncInterval.Create( member, 180 );

				// Make sure that there is an interval set.
				if ( SyncInterval.GetInterval( member ) != 180 )
				{
					throw new ApplicationException( "Member sync interval not set." );
				}

				// Get a file size filter object.
				si = SyncInterval.Get( member );
				
				// Check the aggregate interval.
				if ( si.Interval != 180 )
				{
					throw new ApplicationException( "Aggregate member sync interval not set." );
				}


				// Apply a sync interval on the collection.
				SyncInterval.Create( collection, 240 );

				// Make sure that there is an interval set.
				if ( SyncInterval.GetInterval( collection ) != 240 )
				{
					throw new ApplicationException( "Collection sync interval not set." );
				}

				// Get a sync interval object.
				si = SyncInterval.Get( member, collection );
				
				// Check the aggregate interval.
				if ( si.Interval != 240 )
				{
					throw new ApplicationException( "Aggregate collection sync interval not set." );
				}


				// Apply a sync interval on the current user.
				SyncInterval.Create( 200 );

				// Make sure that there is an interval set.
				if ( SyncInterval.GetInterval() != 200 )
				{
					throw new ApplicationException( "Current member sync interval not set." );
				}

				// Get a sync interval object.
				si = SyncInterval.Get( member, collection );

				// The collection interval should still apply.
				if ( si.Interval != 240 )
				{
					throw new ApplicationException( "Aggregate collection sync interval not set." );
				}

				// Delete the sync interval on the collection.
				SyncInterval.Delete( collection );

				// Get a sync interval object.
				si = SyncInterval.Get( member, collection );

				// The current user interval should now apply.
				if ( si.Interval != 200 )
				{
					throw new ApplicationException( "Aggregate current user sync interval not set." );
				}
			}
			finally
			{
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Test the backup/restore apis
		/// </summary>
		[Test]
		public void BackupRestoreTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			try
			{
				// Commit the collection.
				collection.Commit();

				// Turn the node into a string.
				string cString = collection.Properties.ToString( false );
				string oString = collection.Owner.Properties.ToString( false );

				// Delete the collection and restore it again.
				collection.Commit( collection.Delete() );

				// Restore the collection back over the top of the existing one.
				XmlDocument cDoc = new XmlDocument();
				cDoc.LoadXml( cString );
				collection = new Collection( store, new Node( cDoc ) );

				XmlDocument oDoc = new XmlDocument();
				oDoc.LoadXml( oString );
				Member owner = new Member( new Node( oDoc ) );

				// Set the node's state to restore.
				collection.RestoreNode( collection );
				collection.RestoreNode( owner );
				collection.Commit( new Node[] { collection, owner } );

				// Get the collection to prove that it is back.
				collection.Refresh( collection );
			}
			finally
			{
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Test the commit/delete api to handle holes in the array.
		/// </summary>
		[Test]
		public void CommitDeleteHoleTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			collection.Commit();

			try
			{
				Node[] list = new Node[ 10 ];
				for ( int i = 0; i < list.Length; ++i )
				{
					if ( ( i % 2 ) == 0 )
					{
						list[ i ] = new Node( String.Format( "Test Node {0}", i ) );
					}
					else
					{
						list [ i ] = null;
					}
				}

				collection.Commit( list );
				collection.Commit( collection.Delete( list ) );
			}
			finally
			{
				collection.Commit( collection.Delete() );
			}
		}

		/// <summary>
		/// Tests the collection locks.
		/// </summary>
		[Test]
		public void CollectionLockTest()
		{
			Collection collection1 = new Collection( store, "CS_TestCollection1", store.LocalDomain );
			Collection collection2 = new Collection( store, "CS_TestCollection2", store.LocalDomain );
			collection1.Commit();

			try
			{
				collection1.Lock( Collection.LockType.Permanent );
				collection2.Lock( Collection.LockType.Backup );

				ICSList list = store.GetLockedCollections();
				foreach( ShallowNode sn in list )
				{
					if ( sn.ID != collection1.ID && sn.ID != collection2.ID )
					{
						throw new ApplicationException( "Found unlocked collections." );
					}
				}

				list = store.GetLockedCollections( Collection.LockType.Backup );
				foreach( ShallowNode sn in list )
				{
					if ( sn.ID != collection2.ID )
					{
						throw new ApplicationException( "Found wrong locked collection." );
					}
				}

				collection1.Impersonate( collection1.GetCurrentMember() );
				collection1.Properties.AddProperty( "New Property", "Text" );
				try
				{
					collection1.Commit();
				}
				catch ( LockException )
				{}

				try
				{
					collection1.Commit( collection1.Delete() );
				}
				catch ( LockException )
				{}

				collection1.Revert();

				collection1.Unlock();
				collection2.Unlock();

				list = store.GetLockedCollections();
				if ( list.Count > 0 )
				{
					throw new ApplicationException( "Found locked collections." );
				}

				list = store.GetLockedCollections( Collection.LockType.Backup );
				if ( list.Count > 0 )
				{
					throw new ApplicationException( "Found locked collection." );
				}

				collection2.Properties.AddProperty( "New Property", "Text" );
				collection2.Commit();
				collection1.Commit();
			}
			finally
			{
				collection1.Commit( collection1.Delete() );
				collection2.Commit( collection2.Delete() );
			}
		}

		/// <summary>
		/// Tests the collection owner.
		/// </summary>
		[Test]
		public void CollectionOwnerTest()
		{
			CollectionOwnerTests test = new CollectionOwnerTests( store );
			test.RunTests();
		}

		/// <summary>
		/// Tests the DomainProvider interface.
		/// </summary>
		[Test]
		public void DomainProviderTest()
		{
			FindMemberTests test = new FindMemberTests( store );
			test.RunTests();
		}

		/// <summary>
		/// Tests the SetDomainServerAddress.
		/// </summary>
		[Test]
		public void DomainServerAddressTest()
		{
			DomainServerAddressTests test = new DomainServerAddressTests( store );
			test.RunTests();
		}

		/// <summary>
		/// Tests the SetCursor function on the ICSEnumerator.
		/// </summary>
		[Test]
		public void ICSEnumeratorTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection", store.LocalDomain );
			collection.Commit();

			try
			{
				// Add a bunch of nodes.
				for ( int i = 0; i < 30; ++i )
				{
					Node n = new Node( "Test Node " + i.ToString() );
					collection.Commit( n );
				}

				// Search for all of the nodes by name.
				ICSList list = collection.Search( BaseSchema.ObjectName, "Test", SearchOp.Begins );
				if ( list.Count != 30 )
				{
					throw new ApplicationException( "Cannot find all of the nodes1." );
				}

				// Search forward getting each node.
				ICSEnumerator e = list.GetEnumerator() as ICSEnumerator;
				int count = 0;
				while ( e.MoveNext() ) ++count;
				if ( count != list.Count )
				{
					throw new ApplicationException( "Cannot find all of the nodes2." );
				}

				// Set the cursor back 10 nodes.
				if ( !e.SetCursor( IndexOrigin.END, -10 ) )
				{
					throw new ApplicationException( "Cannot set cursor from end." );
				}

				// There should be only 10 nodes left.
				count = 0;
				while ( e.MoveNext() ) ++count;
				if ( count != 10 )
				{
					throw new ApplicationException( "No data after set cursor." );
				}
			}
			finally
			{
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
			// Stop the services.
			manager.StopServices();
			manager.WaitForServicesStopped();

			// Delete the database.  Must be store owner to delete the database.
			store.Delete();

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
