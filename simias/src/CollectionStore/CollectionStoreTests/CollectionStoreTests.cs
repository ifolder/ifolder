/***********************************************************************
 *  CollectionStoreTests.cs - Implements unit tests for the Collection
 *  Store assembly.
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
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Threading;
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
		#endregion

		#region Test Setup
		/// <summary>
		/// Performs pre-initialization tasks.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			// Connect to the store.
			store = Store.Connect( new Uri( Path.Combine( Directory.GetCurrentDirectory(), "CollectionStoreTestDir" ) ) );

			// Add another identity to the database.
			LocalAddressBook localAb = store.GetLocalAddressBook();

			// Check to see if the identity already exist.
			if ( localAb.GetSingleIdentityByName( "cameron" ) == null )
			{
				localAb.AddIdentity( "cameron" );
				localAb.Commit( true );
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
			Collection collection = store.CreateCollection( "CS_TestCollection" );

			// Remember the id for later.
			string id = collection.Id;

			try
			{
				// Now commit it to the store.
				collection.Commit();

				// Make sure the collection exists.
				if ( store.GetCollectionById( id ) == null )
				{
					throw new ApplicationException( "Collection was committed but does not exist in the store" );
				}
			}
			finally
			{
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );

				// Delete the collection.
				collection.Delete();
				if ( store.GetCollectionById( id ) != null )
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
			Collection collection = store.CreateCollection( "CS_TestCollection" );
			try
			{
				// Create a node subordinate to this collection.
				Node child = collection.CreateChild( "CS_ChildNode", "Child" );

				// Commit this collection.
				collection.Commit( true );

				// Search this collection for this child.
				bool foundChild = false;
				foreach ( Node node in collection )
				{
					if ( ( node.Type == "Child" ) && ( node.Id == child.Id ) )
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
				child.Delete( true ).Delete();

				// See if the child node still exists.
				if ( collection.GetNodeById( child.Id ) != null )
				{
					throw new ApplicationException( "Child node not deleted." );
				}
			}
			finally
			{
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );

				// Get rid of this collection.
				collection.Delete( true );
			}
		}

		/// <summary>
		/// Performs the various types of searches.
		/// </summary>
		[Test]
		public void SearchTest()
		{
			// Create the collection.
			Collection collection = store.CreateCollection( "CS_TestCollection" );
			try
			{
				string s1 = "The quick red fox jumps over the lazy brown dog.";
				string s2 = "The lazy brown dog doesn't care what the quick red fox does.";

				// Add some properties that can be searched for.
				collection.Properties.AddProperty( "CS_String", s1 );
				collection.Properties.AddProperty( "CS_Int", 291 );
			
				Node child = collection.CreateChild( "CS_Child" );
				child.Properties.AddProperty( "CS_String", s2 );
				child.Properties.AddProperty( "CS_Int", -291 );

				collection.Commit( true );

				// Do the various types of searches for strings and values.

				// Should return s1.
				ICSEnumerator e = ( ICSEnumerator )collection.Search( "CS_String", s1, Property.Operator.Equal ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Property p = ( ( Node )e.Current ).Properties.GetSingleProperty( "CS_String" );
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
				e = ( ICSEnumerator )collection.Search( "CS_String", s1, Property.Operator.Not_Equal ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Property p = ( ( Node )e.Current ).Properties.GetSingleProperty( "CS_String" );
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
				e = ( ICSEnumerator )collection.Search( "CS_String", "The", Property.Operator.Begins ).GetEnumerator();
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
				e = ( ICSEnumerator )collection.Search( "CS_String", "fox does.", Property.Operator.Ends ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Property p = ( ( Node )e.Current ).Properties.GetSingleProperty( "CS_String" );
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
				e = ( ICSEnumerator )collection.Search( "CS_String", "lazy brown dog", Property.Operator.Contains ).GetEnumerator();
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
				e = ( ICSEnumerator )collection.Search( "CS_Int", 291, Property.Operator.Equal ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Property p = ( ( Node )e.Current ).Properties.GetSingleProperty( "CS_Int" );
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
				e = ( ICSEnumerator )collection.Search( "CS_Int", 291, Property.Operator.Not_Equal ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Property p = ( ( Node )e.Current ).Properties.GetSingleProperty( "CS_Int" );
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
				e = ( ICSEnumerator )collection.Search( "CS_Int", 0, Property.Operator.Greater ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Property p = ( ( Node )e.Current ).Properties.GetSingleProperty( "CS_Int" );
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
				e = ( ICSEnumerator )collection.Search( "CS_Int", 0, Property.Operator.Less ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Property p = ( ( Node )e.Current ).Properties.GetSingleProperty( "CS_Int" );
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
				e = ( ICSEnumerator )collection.Search( "CS_Int", -291, Property.Operator.Greater_Equal ).GetEnumerator();
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
				e = ( ICSEnumerator )collection.Search( "CS_Int", 281, Property.Operator.Less_Equal ).GetEnumerator();
				try
				{
					if ( e.MoveNext() )
					{
						Property p = ( ( Node )e.Current ).Properties.GetSingleProperty( "CS_Int" );
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
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );

				// Delete the collection.
				collection.Delete( true );
			}
		}


		/// <summary>
		/// Searches for various property types on a collection.
		/// </summary>
		[Test]
		public void PropertyStoreAndRetrieveTest()
		{

			// Create a collection to add properties to.
			Collection collection = store.CreateCollection( "CS_TestCollection" );
			try
			{
				string id = collection.Id;
			
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
				collection = ( Collection )collection.GetNodeById( id );

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
				collection = ( Collection )collection.GetNodeById( id );

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
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );

				// Delete the collection.
				collection.Delete();
			}
		}


		/// <summary>
		/// Deletes a Property from a Collection.
		/// </summary>
		[Test]
		public void PropertyMethodsTest()
		{
			Collection collection = store.CreateCollection( "CS_TestCollection" );
			try
			{
				// First property to add.
				Property p1 = new Property( "CS_String", "This is the first string" );
				p1.Flags = 0x0010;
				collection.Properties.AddProperty( p1 );

				// p2 should follow p1.
				Property p2 = new Property( "CS_String", "This is the second string" );
				collection.Properties.AddProperty( p2 );

				// Insert before p1.
				Property p3 = new Property( "CS_String", "This is inserted before the first string" );
				p1.InsertBefore( p3 );

				// Change p3's value and insert it after p2.  This will remove it from before p1.
				p3.Value = "This is inserted after the second string";
				p2.InsertAfter( p3 );

				// Add a property with a similar name and make sure that it is not found.
				Property p4 = new Property( "CS_Strings", "This is a similar property" );
				collection.Properties.AddProperty( p4 );

				// Get a list of the CS_String properties on this collection.
				MultiValuedList mvl = collection.Properties.GetProperties( "CS_String" );

				// Should be 3 strings.
				int count = 0;
				IEnumerator e = mvl.GetEnumerator();
				while ( e.MoveNext() ) ++count;

				if ( count != 3 )
				{
					throw new ApplicationException( "Not all properties were returned" );
				}

				// Delete p3.
				p3.Delete();

				// Get the list again.  There should only be two.
				mvl = collection.Properties.GetProperties( "CS_String" );

				count = 0;
				e = mvl.GetEnumerator();
				while ( e.MoveNext() ) ++count;

				if ( count != 2 )
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
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );

				// Delete the collection.
				collection.Delete();
			}
		}


		/// <summary>
		/// Tries to add a reserved system property.
		/// </summary>
		[Test]
		[ExpectedException( typeof( ApplicationException ) )]
		public void AddSystemPropertyTest()
		{
			Collection collection = store.CreateCollection( "CS_TestCollection" );
			try
			{
				// Shouldn't allow this.
				collection.Properties.AddProperty( "collectionid", "1234-5678-90" );
			}
			finally
			{
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );

				// Delete the collection.
				collection.Delete();
			}
		}


		/// <summary>
		/// Tests the file interface.
		/// </summary>
		[Test]
		public void FileTest()
		{
			Collection collection = store.CreateCollection( "CS_TestCollection", new Uri( Path.Combine( Directory.GetCurrentDirectory(), "CS_TestCollection" ) ) );
			try
			{
				string fileData = "How much wood can a woodchuck chuck if a woodchuck could chuck wood?";

				// Create a file in the file system.
				string filePath = Path.Combine( collection.DocumentRoot.LocalPath, "Test.txt" );
				FileStream fs = new FileStream( filePath, FileMode.Create, FileAccess.Write, FileShare.None );
				StreamWriter sw = new StreamWriter( fs );
				sw.WriteLine( fileData );
				sw.Close();

				// Add this file to the collection, rooted at the collection.
				FileEntry fe = collection.AddFileEntry( "CS_TestFile", Path.DirectorySeparatorChar + "Test.txt" );

				// Add a directory to the collection also.
				string testDir = Path.Combine( Directory.GetCurrentDirectory(), "CS_TestCollection" + Path.DirectorySeparatorChar + "MyTestDir" );
				Directory.CreateDirectory( testDir );
				DirectoryEntry de = collection.AddDirectoryEntry( "CS_TestDir", testDir );

				// Commit all changes.
				collection.Commit( true );

				// Get the file entries from the collection.
				ICSList entryList = collection.GetFileSystemEntryList();
				foreach ( FileSystemEntry fse in entryList )
				{
					if ( fse.IsDirectory )
					{
						if ( fse.Name != "CS_TestDir" )
						{
							throw new ApplicationException( "Found unexpected directory." );
						}
					}
					else
					{
						if ( ( fse as FileEntry ).Name != "CS_TestFile" )
						{
							throw new ApplicationException( "Found unexpected file." );
						}
					}
				}

				// Add a local property to the stream object.
				Property p1 = new Property( "TestProperty", "This is a test" );
				p1.LocalProperty = true;
				fe.Properties.AddProperty( p1 );
				collection.Commit();

				// Export the node to make sure that this property doesn't appear.
				XmlDocument xd = store.ExportSingleNodeToXml( collection, collection.Id );
				
				// Search for the property that should have been removed.
				XmlNodeList xnl = xd.SelectNodes( "//Property[@name = 'TestProperty']" );
				if ( xnl.Count != 0 )
				{
					throw new ApplicationException( "Failed to remove local property" );
				}

				// Delete the node stream object.
				fe.Delete();
			}
			finally
			{
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );

				// Delete the collection.
				collection.Delete( true );
			}
		}

		/// <summary>
		/// Test the access control functionality.
		/// </summary>
		[Test]
		public void AccessControlTest()
		{
			Collection collection1 = store.CreateCollection( "CS_TestCollection1" );
			Collection collection2 = store.CreateCollection( "CS_TestCollection2" );
			Collection collection3;

			try
			{
				// Commit the collections.
				collection1.Commit();
				collection2.Commit();

				// Get a user that can be impersonated.
				Identity user = store.GetLocalAddressBook().GetSingleIdentityByName( "cameron" );
				collection1.SetUserAccess( user.Id, Access.Rights.ReadWrite );
				collection1.Commit();

				try
				{
					// Try again to access the collection.
					store.ImpersonateUser( user.Id );
					collection3 = store.GetCollectionById( collection1.Id );
					collection3.Properties.AddProperty( "DisplayName", "Access Collection" );
					collection3.Commit();

					try
					{
						// Try to change the collection ownership.
						collection3.ChangeOwner( user.Id, Access.Rights.ReadOnly );
						throw new ApplicationException( "Change ownership access control check on impersonation failed" );
					}
					catch ( UnauthorizedAccessException )
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
				catch ( UnauthorizedAccessException )
				{
					// This is expected.
				}

				// Change the ownership on the collection.
				collection3.ChangeOwner( user.Id, Access.Rights.ReadOnly );
				collection3.Commit();

				// Make sure that it changed.
				if ( collection3.Owner != user.Id )
				{
					throw new ApplicationException( "Collection ownership did not change" );
				}

				try
				{
					// Enumerate the collections. Only collection1 one should be returned.
					store.ImpersonateUser( user.Id );
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
				if ( count != 4 )
				{
					e2.Dispose();
					throw new ApplicationException( "Enumeration access control without impersonation failed" );
				}

				e2.Dispose();

				// Set world rights on collection2.
				collection2.SetUserAccess( Access.WorldRole, Access.Rights.ReadWrite );
				collection2.Commit();

				try
				{
					store.ImpersonateUser( user.Id );
					collection3 = store.GetCollectionById( collection2.Id );
				}
				finally
				{
					store.Revert();
				}
			}
			finally
			{
				try
				{
					// Operate as the store owner in order to delete the collections.
					store.ImpersonateUser( Access.StoreAdminRole );

					// Get rid of the root paths.
					Directory.Delete( collection1.DocumentRoot.LocalPath, true );
					Directory.Delete( collection2.DocumentRoot.LocalPath, true );

					// Delete the collections.
					collection1.Delete();
					collection2.Delete();
				}
				finally
				{
					store.Revert();
				}
			}
		}

		/// <summary>
		/// Test the serialization/deserialization.
		/// </summary>
		[Test]
		public void SerializeTest()
		{
			Collection collection = store.CreateCollection( "CS_TestCollection" );
			try
			{
				string fileData = "How much wood can a woodchuck chuck if a woodchuck could chuck wood?";

				// Create a file in the file system.
				string filePath = Path.Combine( collection.DocumentRoot.LocalPath, "Test.txt" );
				FileStream fs = new FileStream( filePath, FileMode.Create, FileAccess.Write, FileShare.None );
				StreamWriter sw = new StreamWriter( fs );
				sw.WriteLine( fileData );
				sw.Close();

				// Create some children nodes.
				collection.CreateChild( "ChildNode1" );
				FileEntry fe = collection.CreateChild( "ChildNode2" ).AddFileEntry( "CS_TestFile", "Test.txt" );

				// Commit the collection.
				collection.Commit( true );

				try
				{
					// Have to do all of this as the synchronization operator.
					store.ImpersonateUser( Access.SyncOperatorRole );

					// Get the document root for the collection.
					Uri documentRoot = new Uri( Directory.GetParent( collection.DocumentRoot.LocalPath ).FullName );

					// Serialize/Deserialize the collection as the database owner and the collection existing.
					XmlDocument doc = store.ExportNodesToXml( collection, collection.Id, true );
					store.ImportNodesFromXml( doc, documentRoot );

					// Delete the collection so it can be restored.
					collection.Delete( true );
					collection = store.ImportNodesFromXml( doc, documentRoot );

					// Deserialize the collection as a different identity.
					// Get a user that can be impersonated.
					Identity user = store.GetLocalAddressBook().GetSingleIdentityByName( "cameron" );

					try
					{
						try
						{
							// Impersonate the user.
							store.ImpersonateUser( user.Id );
							store.ImportNodesFromXml( doc, documentRoot );
							throw new ApplicationException( "Expected exception for improper deserialization access" );
						}
						finally
						{
							// Always revert back.
							store.Revert();
						}
					}
					catch ( UnauthorizedAccessException )
					{
						// This is expected.
					}

					// Set an ACE giving the impersonating user read/write rights to the collection.
					collection.SetUserAccess( user.Id, Access.Rights.ReadWrite );
					collection.Commit();

					// Reserialize the collection so it contains the ace added for the impersonating user.
					doc = store.ExportNodesToXml( collection, collection.Id, true );

					try
					{
						// Impersonate the user.
						store.ImpersonateUser( user.Id );
						store.ImportNodesFromXml( doc, documentRoot );
					}
					finally
					{
						// Always revert back.
						store.Revert();
					}

					// Then delete the collection and let the impersonating user restore it.
					collection.Delete( true );

					try
					{
						// Impersonate the user.
						store.ImpersonateUser( user.Id );
						store.ImportNodesFromXml( doc, documentRoot );
					}
					finally
					{
						// Always revert back.
						store.Revert();
					}

					// Get back the original collection object.
					collection = store.GetCollectionById( collection.Id );
				}
				finally
				{
					store.Revert();
				}
			}
			finally
			{
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );

				try
				{
					// Delete the collection.  Have to impersonate because we are no longer the owner.
					store.ImpersonateUser( Access.StoreAdminRole );
					collection.Delete( true );
				}
				finally
				{
					store.Revert();
				}
			}
		}

		/// <summary>
		/// Tests the get-by-name functionality.
		/// </summary>
		[Test]
		public void GetByNameTest()
		{
			Collection collection = store.CreateCollection( "CS_TestCollection" );
			try
			{
				// Commit the collection.
				collection.Commit();

				// Get the collection by name.
				ICSList list = store.GetCollectionsByName( "CS_Test.*" );
				ICSEnumerator e = ( ICSEnumerator )list.GetEnumerator();

				if ( e.MoveNext() == false )
				{
					throw new ApplicationException( "Cannot find collection by name" );
				}

				// Create nodes in the collection.
				collection = ( Collection )e.Current;
				collection.CreateChild( "Node1" ).CreateChild( "Node2" ).CreateChild( "Node3" );
				collection.Commit( true );

				// Get "Node3".
				e = ( ICSEnumerator )collection.GetNodesByName( "Node3" ).GetEnumerator();
				if ( !e.MoveNext() )
				{
					throw new ApplicationException( "Cannot get node by leaf name" );
				}

				Node node1 = ( Node )e.Current;
				if ( e.MoveNext() )
				{
					throw new ApplicationException( "Cannot get node by leaf name" );
				}

				e.Dispose();

				// Get /CS_TetstCollection/Node1/Node2/Node3
				Node node2 = collection.GetSingleNodeByPathName( "/CS_TestCollection/Node1/Node2/Node3" );
				if ( node2 == null )
				{
					throw new ApplicationException( "Cannot get node by name" );
				}

				if ( node1.Id != node2.Id )
				{
					throw new ApplicationException( "Found nodes do not reference same object" );
				}
			}
			finally
			{
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );

				// Delete the collection.
				collection.Delete( true );
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
		/// Tests the ability to rollback pre-committed changes on a collection.
		/// </summary>
		[Test]
		public void RollbackTest()
		{
			Collection collection = store.CreateCollection( "CS_TestCollection" );
			try
			{
				// Commit the collection.
				collection.Commit();

				// Change the collection.
				collection.Properties.AddProperty( "New Property", "This is a test" );

				// Rollback the change from the node interface.
				( ( Node )collection ).Rollback();

				// The added property should be gone.
				Property p = collection.Properties.GetSingleProperty( "New Property" );
				if ( p != null )
				{
					throw new ApplicationException( "Added property did not rollback" );
				}

				// Add nodes to the collection.
				Node node = collection.CreateChild( "CS_TestNode" );
				collection.Commit( true );

				// Modify the collection and the node.
				collection.Properties.AddProperty( "New Property", "This is a test" );
				node.Properties.AddProperty( "New Property", "This is a test" );

				// Roll back the collection changes.
				collection.Rollback();

				// The node and collection should no longer have the property.
				p = node.Properties.GetSingleProperty( "New Property" );
				if ( p != null )
				{
					throw new ApplicationException( "Collection rollback on node failed." );
				}

				p = collection.Properties.GetSingleProperty( "New Property" );
				if ( p != null )
				{
					throw new ApplicationException( "Collection rollback collection failed." );
				}
			}
			finally
			{
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );

				// Delete the collection.
				collection.Delete( true );
			}
		}

		/// <summary>
		/// Test the ability to create a node from new.
		/// </summary>
		[Test]
		public void NewupNodeTest()
		{
			Collection collection = store.CreateCollection( "CS_TestCollection" );
			try
			{
				// Commit the collection.
				collection.Commit();

				// Create a node associated with the collection.
				Node node = new Node( collection, "CS_TestNode_1", Guid.NewGuid().ToString(), Node.Generic );
				node.Properties.AddProperty( "CS_TestProperty", "This is a test" );

				// Create a child node.
				node.CreateChild( "CS_TestNode_2", Node.Generic );
				
				// A commit should cause an exception.
				try
				{
					collection.Commit( true );
					throw new ApplicationException( "Commit with no parent succeeded." );
				}
				catch
				{
					// This is expected.
				}

				// Add the node to the collection.
				node.SetParent( collection );
				collection.Commit( true );

				// Find the added node.
				if ( collection.GetNodeById( node.Id ) == null )
				{
					throw new ApplicationException( "New committed node not found." );
				}
			}
			finally
			{
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );

				// Delete the collection.
				collection.Delete( true );
			}
		}

		/// <summary>
		/// Test to get the database object.
		/// </summary>
		[Test]
		public void GetDatabaseObjectTest()
		{
			Collection dbo = store.GetDatabaseObject();
			if ( dbo == null )
			{
				throw new ApplicationException( "Could not find database object." );
			}
		}

		/// <summary>
		/// Move a collection root test.
		/// </summary>
		[Test]
		public void MoveCollectionRootTest()
		{
			Collection collection = store.CreateCollection( "CS_TestCollection", new Uri( Path.Combine( Directory.GetCurrentDirectory(), "CS_TestCollection" ) ) );
			try
			{
				string fileData = "How much wood can a woodchuck chuck if a woodchuck could chuck wood?";
				string filePath = null;

				// Create some files in the file system.
				for ( int i = 0; i < 10; ++i )
				{
					string fileName = "Test" + i + ".txt";
					filePath = Path.Combine( collection.DocumentRoot.LocalPath, fileName );
					FileStream fs = new FileStream( filePath, FileMode.Create, FileAccess.Write, FileShare.None );
					StreamWriter sw = new StreamWriter( fs );
					sw.WriteLine( fileData );
					sw.Close();

					// Add this file to the collection, rooted at the collection.
					collection.AddFileEntry( "CS_TestFile", fileName );
				}

				// Commit all changes.
				collection.Commit( true );

				// Move the collection to a new directory.
				string newDirString = Path.Combine( Directory.GetCurrentDirectory(), "CS_MovedTestCollection" );
				Directory.CreateDirectory( newDirString );
				newDirString = Path.Combine( newDirString, "TestDir" );
				collection.DocumentRoot = new Uri( newDirString );

				// Get the new document root and make sure that it changed.
				if ( collection.DocumentRoot.LocalPath != newDirString )
				{
					throw new ApplicationException( "MoveRoot call failed." );
				}

				// Verify that the file can still be gotten.
				ICSList feList = collection.GetFileSystemEntryList();
				foreach ( FileSystemEntry tempEntry in feList )
				{
					if ( !tempEntry.Exists )
					{
						throw new ApplicationException( "Cannot find file after move of collection." );
					}
				}
			}
			finally
			{
				string oldDir = Path.Combine( Directory.GetCurrentDirectory(), "CS_TestCollection" );
				if ( Directory.Exists( oldDir ) )
				{
					Directory.Delete( oldDir, true );
				}

				string newDir = Path.Combine( Directory.GetCurrentDirectory(), "CS_MovedTestCollection" );
				if ( Directory.Exists( newDir ) )
				{
					Directory.Delete( newDir, true );
				}
                
				// Delete the collection.
				collection.Delete( true );
			}
		}

		/// <summary>
		/// Tests the dropping of tombstones and dirty nodes.
		/// </summary>
		[Test]
		public void TombstoneTest()
		{
			Collection collection = store.CreateCollection( "CS_TestCollection" );
			try
			{
				// Create a bunch of nodes.
				collection.CreateChild( "Node1" );
				collection.CreateChild( "Node2" );
				collection.CreateChild( "Node3" );
				collection.Commit( true );

				// Delete the nodes.
				foreach( Node delNode in collection )
				{
					if ( delNode.Id != collection.Id )
					{
						delNode.Delete();
					}
				}

				// There should not be any nodes left.
				if ( collection.HasChildren )
				{
					throw new ApplicationException( "Tombstones showing up as nodes." );
				}
			}
			finally
			{
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );
                
				// Delete the collection.
				collection.Delete( true );
			}
		}

		/// <summary>
		/// Tests the incarnation values that they get updated correctly.
		/// </summary>
		[Test]
		public void TestIncarnationValues()
		{
			Collection collection = store.CreateCollection( "CS_TestCollection" );
			try
			{
				// Create a child node.
				Node node1 = collection.CreateChild( "Node1" );

				// The incarnation number on the collection and child node should be zero.
				if ( ( collection.LocalIncarnation != 0 ) || ( node1.LocalIncarnation != 0 ) )
				{
					throw new ApplicationException( "Local incarnation value is not zero." );
				}

				// Commit the collection which should increment the values both to one.
				collection.Commit( true );

				// The incarnation number on the collection and child node should be one.
				if ( ( collection.LocalIncarnation != 1 ) || ( node1.LocalIncarnation != 1 ) )
				{
					throw new ApplicationException( "Local incarnation value is not one." );
				}

				// Change just the node and commit it.
				node1.Properties.AddProperty( "Test Property", "This is a test." );
				node1.Commit();

				// The incarnation number on the collection and child node should be two.
				if ( ( collection.LocalIncarnation != 2 ) || ( node1.LocalIncarnation != 2 ) )
				{
					throw new ApplicationException( "Local incarnation value is not two." );
				}

				// Commit just the collection with no changes to it.
				collection.Commit();

				// The incarnation number on the collection should be 3 and child node should be two.
				if ( ( collection.LocalIncarnation != 3 ) || ( node1.LocalIncarnation != 2 ) )
				{
					throw new ApplicationException( "Local incarnation value is not three and two." );
				}

				// Add another new child node and commit it.
				Node node2 = node1.CreateChild( "Node2" );
				collection.Commit( true );

				// The incarnation number on the collection should be 4 and child node1 should be two
				// and child node2 should be one.
				if ( ( collection.LocalIncarnation != 4 ) || ( node1.LocalIncarnation != 2 ) || ( node2.LocalIncarnation != 1 ) )
				{
					throw new ApplicationException( "Local incarnation value is not four, two, and one." );
				}

				try
				{
					// This next method requires synchronization access.
					store.ImpersonateUser( Access.SyncOperatorRole );

					// Finally, set the master version of the collection.
					collection.UpdateIncarnation( 1 );
				}
				finally
				{
					store.Revert();
				}

				// The incarnation number on the collection should be 1.1
				if ( ( collection.LocalIncarnation != 1 ) && ( collection.MasterIncarnation != 1 ) )
				{
					throw new ApplicationException( "Local incarnation value is not one and master incarnation is not one." );
				}
			}
			finally
			{
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );
                
				// Delete the collection.
				collection.Delete( true );
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
			Identity identity = new Identity( localAb, "newguy" );

			RSACryptoServiceProvider credential = RsaKeyStore.CreateRsaKeys();

			// Add two aliases to the identity.
			for ( int i = 0; i < 10; ++i )
			{
				identity.CreateAlias( "Mike's Domain " + i, Guid.NewGuid().ToString(), credential );
			}

			// Commit the changes.
			identity.Commit();

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
			Collection collection = store.CreateCollection( "CS_TestCollection" );
			try
			{
				// Add a file to the collection object.
				collection.AddFileEntry( "CS_TestFile", "Test.txt" );
				collection.Commit();

				// See if the collection can be located.
				ICSList list = store.GetNodesAssociatedWithPath( collection.Id, "Test.txt" );
				IEnumerator e = list.GetEnumerator();
				if ( !e.MoveNext() )
				{
					throw new ApplicationException( "Cannot find associated node." );
				}
			}
			finally
			{
				// Get rid of the root path.
				Directory.Delete( collection.DocumentRoot.LocalPath, true );
                
				// Delete the collection.
				collection.Delete( true );
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
			store.ImpersonateUser( Access.StoreAdminRole );
			store.Delete();
			store.Dispose();

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
