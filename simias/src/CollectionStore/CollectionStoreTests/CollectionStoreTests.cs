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
using System.Xml;
using NUnit.Framework;

using Simias;
using Simias.Storage;
using Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider;


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

		// Path to store.
		private string basePath = Path.Combine( Directory.GetCurrentDirectory(), "CollectionStoreTestDir" );
		#endregion

		#region Test Setup
		/// <summary>
		/// Performs pre-initialization tasks.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			// Connect to the store.
			store = new Store( new Configuration( basePath ) );

			// Add another identity to the database.
			LocalAddressBook localAb = store.GetLocalAddressBook();

			// Check to see if the identity already exist.
			if ( localAb.GetSingleNodeByName( "cameron" ) == null )
			{
				localAb.Commit( new BaseContact( "cameron" ) );
			}
		}
		#endregion

		#region Iteration Tests
		/// <summary>
		/// Connects to a Store and creates, commits and deletes a Collection.
		/// </summary>
		[Test]
		public void CreateCollectionTest()
		{
			// Create a new collection and remember its ID.
			Collection collection = new Collection( store, "CS_TestCollection" );

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
			Collection collection = new Collection( store, "CS_TestCollection" );
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
			Collection collection = new Collection( store, "CS_TestCollection" );
			try
			{
				string s1 = "The quick red fox jumps over the lazy brown dog.";
				string s2 = "The lazy brown dog doesn't care what the quick red fox does.";

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
				e = ( ICSEnumerator )collection.Search( "CS_String", "lazy brown dog", SearchOp.Contains ).GetEnumerator();
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
			Collection collection = new Collection( store, "CS_TestCollection" );
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
			Collection collection = new Collection( store, "CS_TestCollection" );
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
			Collection collection = new Collection( store, "CS_TestCollection" );
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

			Collection collection = new Collection( store, "CS_TestCollection" );
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
			Collection collection1 = new Collection( store, "CS_TestCollection1" );
			Collection collection2 = new Collection( store, "CS_TestCollection2" );
			Collection collection3;

			try
			{
				// Commit the collections.
				collection1.Commit();
				collection2.Commit();

				// Get a user that can be impersonated.
				LocalAddressBook localAb = store.GetLocalAddressBook();
				BaseContact user = localAb.GetContactByName( "cameron" );
				collection1.SetUserAccess( user.ID, Access.Rights.ReadWrite );
				collection1.Commit();

				try
				{
					// Try again to access the collection.
					store.ImpersonateUser( user.ID );
					collection3 = store.GetCollectionByID( collection1.ID );
					collection3.Properties.AddProperty( "DisplayName", "Access Collection" );
					collection3.Commit();

					try
					{
						// Try to change the collection ownership.
						collection3.ChangeOwner( user.ID, Access.Rights.ReadOnly );
						throw new ApplicationException( "Change ownership access control check on impersonation failed" );
					}
					catch ( AccessException )
					{
						// This is expected.
					}
				}
				finally
				{
					store.Revert();
				}


				try
				{
					// Try and down-grade the owner's rights.
					collection1.SetUserAccess( collection1.Owner, Access.Rights.ReadWrite );
					throw new ApplicationException( "Block owner rights change failed" );
				}
				catch ( AccessException )
				{
					// This is expected.
				}

				// Change the ownership on the collection.
				collection3.ChangeOwner( user.ID, Access.Rights.ReadOnly );
				collection3.Commit();

				// Make sure that it changed.
				if ( collection3.Owner != user.ID )
				{
					throw new ApplicationException( "Collection ownership did not change" );
				}

				try
				{
					// Enumerate the collections. Only collection1 one should be returned.
					store.ImpersonateUser( user.ID );
					ICSEnumerator e1 = ( ICSEnumerator )store.GetEnumerator();
					if ( !e1.MoveNext() || e1.MoveNext() )
					{
						e1.Dispose();
						throw new ApplicationException( "Enumeration access control on impersonation failed" );
					}

					e1.Dispose();
				}
				finally
				{
					store.Revert();
				}

				// Now four collections should show up. The two that the test created and the one that
				// represents the database collection and the one that represents the local address book.
				ICSEnumerator e2 = ( ICSEnumerator )store.GetEnumerator();

				int count = 0;
				while ( e2.MoveNext() ) ++count;
				if ( count != 5 )
				{
					e2.Dispose();
					throw new ApplicationException( "Enumeration access control without impersonation failed" );
				}

				e2.Dispose();

				// Set world rights on collection2.
				collection2.SetUserAccess( Access.World, Access.Rights.ReadWrite );
				collection2.Commit();
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
			Collection collection = new Collection( store, "CS_TestCollection" );
			try
			{
				// Commit the collection.
				collection.Commit();

				// Serialize the collection object.
				MemoryStream ms = new MemoryStream();
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize( ms, new Node( collection ) );

				// Delete the collection so it can be restored.
				collection.Commit( collection.Delete() );

				// Reset the stream before deserializing.
				ms.Position = 0;

				// Restore the collection.
				collection = new Collection( store, ( Node )bf.Deserialize( ms ) );
				collection.ImportNode( collection, true, collection.LocalIncarnation );
				collection.IncarnationUpdate = 2;
				collection.Commit();
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
			Collection collection = new Collection( store, "CS_TestCollection" );
			try
			{
				// Commit the collection.
				collection.Commit();

				// Get the collection by name.
				ICSList list = store.GetCollectionsByName( "CS_Test.*" );
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
		/// Tests the opening of an already existing store.  I've been running into problems, especially
		/// with authentication where it would work differently depending on whether the store was
		/// created vs. already existing.
		/// </summary>
		[Test]
		public void StoreCreateDeleteTest()
		{
			// Dispose the current store object.
			store.Dispose();

			// Reopen the store which should be existing.
			Init();
		}

		/// <summary>
		/// Tests the ability to abort pre-committed changes on a collection.
		/// </summary>
		[Test]
		public void AbortTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection" );
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
		/// Tests the dropping of tombstones and dirty nodes.
		/// </summary>
		[Test]
		public void TombstoneTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection" );
			try
			{
				// Create a node.
				Node node = new Node( "CS_Node" );
				Node[] nodeList = { collection, node };
				collection.Commit( nodeList);

				// Delete the node.
				collection.Commit( collection.Delete( node ) );

				// Node should have turned into a tombstone.
				collection.Refresh( node );
				if ( !collection.IsType( node, "Tombstone" ) )
				{
					throw new ApplicationException( "Deleted node did not turn into a tombstone." );
				}

				// Now delete the tombstone.
				collection.Commit( collection.Delete( node ) );

				// It should be really gone now.
				// Node should have turned into a tombstone.
				if ( collection.GetNodeByID( node.ID ) != null )
				{
					throw new ApplicationException( "Tombstone cannot be deleted." );
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
			Collection collection = new Collection( store, "CS_TestCollection" );
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

				// The incarnation number on the collection and child node should be two.
				if ( ( collection.LocalIncarnation != 2 ) || ( nodeA.LocalIncarnation != 2 ) )
				{
					Console.WriteLine( "collection incarnation = {0}", collection.LocalIncarnation );
					Console.WriteLine( "nodeA incarnation = {0}", nodeA.LocalIncarnation );
					throw new ApplicationException( "Local incarnation value is not two." );
				}

				// Commit just the collection with no changes to it.
				collection.Commit();

				collection.Refresh( nodeA );
				collection.Refresh();

				// The incarnation number on the collection should be 3 and the node should be two.
				if ( ( collection.LocalIncarnation != 3 ) || ( nodeA.LocalIncarnation != 2 ) )
				{
					throw new ApplicationException( "Local incarnation value is not three and two." );
				}

				// Add another new child node and commit it.
				Node nodeB = new Node( "CS_NodeB" );
				collection.Commit( nodeB );

				collection.Refresh( nodeA );
				collection.Refresh( nodeB );
				collection.Refresh();

				// The incarnation number on the collection should be 4 and nodeA should be two
				// and nodeB should be one.
				if ( ( collection.LocalIncarnation != 4 ) || ( nodeA.LocalIncarnation != 2 ) || ( nodeB.LocalIncarnation != 1 ) )
				{
					throw new ApplicationException( "Local incarnation value is not four, two, and one." );
				}

				// Finally, set the master version of the collection.
				collection.ImportNode( collection, true, collection.LocalIncarnation );
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
		/// Tests the identity object and adds aliases.
		/// </summary>
		[Test]
		public void IdentityTest()
		{
			// Get the local address book.
			LocalAddressBook localAb = store.GetLocalAddressBook();

			// Add a new identity.
			BaseContact identity = new BaseContact( "newguy" );

			RSACryptoServiceProvider credential = RsaKeyStore.CreateRsaKeys();

			// Add aliases to the identity.
			for ( int i = 0; i < 10; ++i )
			{
				identity.CreateAlias( "Mike's Domain " + i, Guid.NewGuid().ToString(), credential.ToXmlString( false ) );
			}

			// Commit the changes.
			localAb.Commit( identity );

			// Get the aliases back.
			int count = 0;
			ICSList aliasList = identity.GetAliasList();
			foreach( Alias alias in aliasList )
			{
				// This check is make only to remove unused variable compiler warning.
				if ( alias != null )
				{
					++count;
				}
			}

			if ( count != 10 )
			{
				throw new ApplicationException( "Cannot find all of the aliases." );
			}
		}

		/// <summary>
		///  Tests the finding all nodes asssociated with a file.
		/// </summary>
		[Test]
		public void FileToNodeTest()
		{
			string rootDir = Path.Combine( Directory.GetCurrentDirectory(), "CS_TestCollection" );

			Collection collection = new Collection( store, "CS_TestCollection" );

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
			Store mergeStore = new Store( new Configuration( basePath ) );

			// Create a collection using the primary store handle.
			Collection collection = new Collection( store, "CS_TestCollection" );

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
			}
			finally
			{
				// Delete the collection.
				collection.Commit( collection.Delete() );

				// Release the merge store handle.
				mergeStore.Dispose();
			}
		}

		/// <summary>
		///  Tests the Enumeration of nodes in a collection.
		/// </summary>
		[Test]
		public void EnumerateNodesTest()
		{
			Collection collection = new Collection( store, "CS_TestCollection" );
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
			Collection collection = new Collection( store, "CS_TestCollection" );
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
				StoreFileNode sfn = new StoreFileNode( collection, "MyFile", fs );

				// The file should not exist yet.
				if ( File.Exists( sfn.GetFullPath( collection ) ) )
				{
					throw new ApplicationException( "Store managed file should not exist yet." );
				}

				collection.Commit( sfn );

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
			Collection collection = new Collection( store, "CS_TestCollection" );
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

				// Delete the collision node.
				collection.Commit( collection.DeleteCollision( collisionNode ) );

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

			// Remove the created directory.
			string dirPath = Path.Combine( Directory.GetCurrentDirectory(), "CollectionStoreTestDir" );
			if ( Directory.Exists( dirPath ) )
			{
				Directory.Delete( dirPath, true );
			}
		}
		#endregion
	}
}
