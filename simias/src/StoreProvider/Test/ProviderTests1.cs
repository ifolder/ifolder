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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using System.Xml;

namespace Simias.Storage.Provider
{
	/// <summary>
	/// Unit Test for the Flaim Storage Provider.
	/// </summary>
	/// 
	[TestFixture]
	public class Tests
	{
		#region Member variables
		IProvider	provider;
		string		recordName = "record";
		string		recordId = "123456789";
		const string recordType = "collection";
		const int	firstRecordId = 1;
		const int	lastRecordId = 100;
		string		collectionId;
		int			transactionSize = 100;
		#endregion

		
		/// <summary>
		/// Used to setup all the needed resources before the tests are run.
		/// </summary>
		//[TestFixtureSetUp]
		public void Init(string providerAssembly, string providerClass)
		{
			collectionId = recordId + firstRecordId;
			bool created;
			provider = Provider.Connect(providerAssembly, providerClass, ".", out created);
		}

		/// <summary>
		/// Used to cleanup any resorces created by the test.
		/// </summary>
		[TestFixtureTearDown]
		public void Destroy()
		{
			provider.DeleteStore();
			provider.Dispose();
		}
		
		/// <summary>
		/// Test to create a collection.
		/// </summary>
		[Test]
		public void CollectionTest()
		{
			try
			{
				CreateCollection(collectionId);
			}
			finally
			{
				// Now cleanup.
				DeleteCollection(collectionId);
			}
		}
		
		/// <summary>
		/// Test case to create an object in the store.
		/// </summary>
		[Test]
		public void CreateRecordTest()
		{
			int recordCount = 1;
			try
			{
				// Create an object.
				TimeSpan elapsedTime = CreateObjects(recordCount);
				// Read the object.
				GetObjects(recordCount);
			}
			finally
			{
				// Now cleanup.
				DeleteCollection(collectionId);
			}
		}


		/// <summary>
		/// Test case to test the performance of the provider.
		/// </summary>
		[Test]
		public void PerformanceTest()
		{
			int recordCount = lastRecordId;
			try
			{
				// Create the objects.
				TimeSpan elapsedTime = CreateObjects(recordCount);
				Console.WriteLine("Created {0} Objects in {1} seconds.", recordCount, elapsedTime.TotalSeconds);
				// Now read the objects.
				elapsedTime = GetObjects(recordCount);
				Console.WriteLine("Read {0} Objects in {1} seconds.", recordCount, elapsedTime.TotalSeconds);

				DateTime t1 = DateTime.Now;
				internalSearch(BaseSchema.ObjectName, Query.Operator.Equal, recordName+recordCount, Syntax.String, true); 
				Console.WriteLine("Found Object in {0} seconds.", (DateTime.Now - t1).TotalSeconds);

				// Now delete the collection.
				elapsedTime = DeleteObjects(recordCount);
				Console.WriteLine("Deleted {0} Objects in {1} seconds.", recordCount, elapsedTime.TotalSeconds);
			}
			finally
			{
				DeleteCollection(collectionId);
			}
		}

		/// <summary>
		/// Test case to delete an object in the store.
		/// </summary>
		[Test]
		public void DeleteRecordTest()
		{
			int recordCount = 1;
			try
			{
				CreateObjects(recordCount);
				DeleteObjects(recordCount);
				ValidateDelete(recordCount);
			}
			finally
			{
				DeleteCollection(collectionId);
			}
		}

		/// <summary>
		/// Test case to find objects that match the supported queries.
		/// </summary>
		[Test]
		public void QueryTest()
		{
			int recordCount = 1;
			try
			{
				CreateObjects(recordCount);
				Search();
			}
			finally
			{
				DeleteCollection(collectionId);
			}
		}

		/// <summary>
		/// Test case to locate an object by id in the object store.
		/// </summary>
		[Test]
		public void GetRecordTest()
		{
			int recordCount = 1;
			try
			{
				CreateObjects(recordCount);
				GetObjects(recordCount);
			}
			finally
			{
				DeleteCollection(collectionId);
			}
		}

		/// <summary>
		/// Test to modify an existing record.
		/// </summary>
		[Test]
		public void ModifyRecordTest()
		{
			int recordCount = 1;
			try
			{
				CreateObjects(recordCount);
				string sObject = provider.GetRecord(recordId + 1, collectionId);
				if (sObject != null)
				{
					XmlDocument originalObject = new XmlDocument();
					originalObject.LoadXml(sObject);
					XmlElement root = originalObject.DocumentElement;
					XmlNode node = root.SelectSingleNode("*/" + XmlTags.PropertyTag);
					if (node != null)
					{
						node.ParentNode.RemoveChild(node);
					}
					provider.CreateRecord(originalObject.InnerXml, collectionId);
					string newObject = provider.GetRecord(recordId + 1, collectionId);
					if (newObject != null)
					{
						if (newObject.Equals(sObject))
						{
							throw new Exception("Failed to modify object");
						}
					}
				}
			}
			finally
			{
				DeleteCollection(collectionId);
			}
		}

		/// <summary>
		/// Method to build the object xml.
		/// </summary>
		/// <param name="includeProperties">If true include the properties.</param>
		/// <param name="recordCount">Number of records to create.</param>
		/// <returns>An Array of XmlDocuments</returns>
		private ArrayList GetObjectXml(int recordCount, bool includeProperties)
		{
			ArrayList list = new ArrayList();
			XmlDocument objectDoc = new XmlDocument();
			int i = firstRecordId;
			
			while (i <= recordCount)
			{
				XmlElement element, rootElement;
				rootElement = objectDoc.CreateElement(XmlTags.ObjectListTag);
				objectDoc.AppendChild(rootElement);
				for (; i <= recordCount; ++i)
				{
					element = objectDoc.CreateElement(XmlTags.ObjectTag);
					element.SetAttribute(XmlTags.NameAttr, recordName + i);
					element.SetAttribute(XmlTags.IdAttr, recordId + i);
					element.SetAttribute(XmlTags.TypeAttr, recordType);
					rootElement.AppendChild(element);

					if (includeProperties)
					{
						// Add properties to the object.
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My String", Syntax.String.ToString(), "0", "This is a \"string\""));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My SByte", Syntax.SByte.ToString(), "0", "-51"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My Byte", Syntax.Byte.ToString(), "0", "251"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My Int16", Syntax.Int16.ToString(), "0", "-4001"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My UInt16", Syntax.UInt16.ToString(), "0", "4001"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My Int32", Syntax.Int32.ToString(), "0", "-2147483641"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My UInt32", Syntax.UInt32.ToString(), "0", "4294967291"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My Int64", Syntax.Int64.ToString(), "0", "-4294967291"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My UInt64", Syntax.UInt64.ToString(), "0", "4294967291"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My Char", Syntax.Char.ToString(), "0", "3001"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My Single", Syntax.Single.ToString(), "0", "42949.67291"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My Boolean", Syntax.Boolean.ToString(), "0", "0"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My DateTime", Syntax.DateTime.ToString(), "0", "4294967291"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My Uri", Syntax.Uri.ToString(), "0", "http://server/path/doc.htm"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, "My Xml", Syntax.XmlDocument.ToString(), "0", "<tag1>this is xml</tag1>"));
						element.AppendChild(Property.CreateXmlNode(objectDoc, BaseSchema.CollectionId, Syntax.String.ToString(), "0", collectionId));
					}
					if (i % transactionSize == 0)
					{
						list.Add(objectDoc);
						objectDoc = new XmlDocument();
						++i;
						break;
					}
				}
			}
			if (objectDoc.ChildNodes.Count > 0)
			{
				list.Add(objectDoc);
			}
			return (list);
		}

		/// <summary>
		/// Create a collection.
		/// </summary>
		/// <param name="collectionId">The Id of the collection.</param>
		private void CreateCollection(string collectionId)
		{
			provider.CreateCollection(collectionId);
		}

		/// <summary>
		/// Called to delete a collection.
		/// </summary>
		/// <param name="collectionId">The ID of the collection to delete.</param>
		private void DeleteCollection(string collectionId)
		{
			provider.DeleteCollection(collectionId);
		}

		/// <summary>
		/// Creates lastRecordId - firstRecordId objects in the store.  The properties
		/// that are set are defined in objectProperties.
		/// </summary>
		/// <param name="recordCount">Number of records to create.</param>
		private TimeSpan CreateObjects(int recordCount)
		{
			ArrayList objectList = GetObjectXml(recordCount, true);
			DateTime startTime = DateTime.Now;
			provider.CreateCollection(collectionId);
			foreach (XmlDocument objectDoc in objectList)
			{
				//writeXml(recordDoc.InnerXml);
				provider.CreateRecord(objectDoc.InnerXml, collectionId);
			}
			TimeSpan deltaTime = DateTime.Now - startTime;
			return (deltaTime);
		}

		/// <summary>
		/// Get all of the objects from firstRecordId to lastRecordId.
		/// </summary>
		/// <param name="recordCount">Number of records to get.</param>
		private TimeSpan GetObjects(int recordCount)
		{	
			int i;
			DateTime startTime = DateTime.Now;
			for (i = firstRecordId; i <= recordCount; ++i)
			{
				string sObject = provider.GetRecord(recordId + i, collectionId);
				if (sObject == null)
				{
					throw (new ApplicationException("Failed to get Object" + recordId));
				}
				//writeXml(sObject);
			}
			TimeSpan deltaTime = DateTime.Now - startTime;
			return (deltaTime);
		}

		/// <summary>
		/// Method to search for the specified value for the given property.
		/// </summary>
		/// <param name="name">Property to search</param>
		/// <param name="op">Query operation</param>
		/// <param name="value">Value to match</param>
		/// <param name="type">The type of the value.</param>
		/// <param name="shouldMatch">If true the search should return a result.</param>
		private void internalSearch(string name, Query.Operator op, string value, Syntax type, bool shouldMatch)
		{
			IResultSet results;
			bool foundMatch = false;
			//results = provider.Search(new Query(name, op, value, type));
			results = provider.Search(new Query(collectionId, name, op, value, type));
			if (results != null)
			{
				char [] buf = new char[4096];
				int len;
				while ((len = results.GetNext(ref buf)) != 0)
				{
					foundMatch = true;
					//writeXml(new string(buf, 0, len));
				}
				if ((shouldMatch && !foundMatch) || (!shouldMatch && foundMatch))
				{
					Console.WriteLine(string.Format("Failed searching for \"{0}\" {1} \"{2}\" type = {3}", name, op.ToString(), value, type));
					//throw (new ApplicationException(string.Format("Searching for \"{0}\" {1} \"{2}\" type = {3}", name, op.ToString(), value, type)));
				}
				results.Dispose();
			}
		}


		/// <summary>
		/// Tries all query types for all defined property types.
		/// </summary>
		private void Search()
		{
			// Try all of the string searches.
			// First search for an attribute.
			internalSearch(BaseSchema.ObjectType, Query.Operator.Equal, "collection", Syntax.String, true); 
			internalSearch(BaseSchema.ObjectType, Query.Operator.Begins, "coll", Syntax.String, true); 
			internalSearch(BaseSchema.ObjectType, Query.Operator.Contains, "llec", Syntax.String, true); 
			internalSearch(BaseSchema.ObjectType, Query.Operator.Ends, "tion", Syntax.String, true); 
			internalSearch(BaseSchema.ObjectType, Query.Operator.Ends, "tiin", Syntax.String, false); 

			// Now search for a string.
			internalSearch(BaseSchema.CollectionId, Query.Operator.Equal, collectionId, Syntax.String, true); 
			internalSearch(BaseSchema.CollectionId, Query.Operator.Begins, collectionId.Substring(0, 4), Syntax.String, true); 
			internalSearch(BaseSchema.CollectionId, Query.Operator.Contains, collectionId.Substring(1, 5), Syntax.String, true); 
			internalSearch(BaseSchema.CollectionId, Query.Operator.Ends, collectionId.Substring(4), Syntax.String, true); 
			internalSearch(BaseSchema.CollectionId, Query.Operator.Ends, "tiin", Syntax.String, false); 

			// Search for SByte types
			internalSearch("My SByte", Query.Operator.Equal, "-51", Syntax.SByte, true); 
			internalSearch("My SByte", Query.Operator.Greater_Equal, "-51", Syntax.SByte, true); 
			internalSearch("My SByte", Query.Operator.Greater_Equal, "-52", Syntax.SByte, true); 
			internalSearch("My SByte", Query.Operator.Greater_Equal, "-50", Syntax.SByte, false); 
			internalSearch("My SByte", Query.Operator.Greater, "-51", Syntax.SByte, false); 
			internalSearch("My SByte", Query.Operator.Greater, "-52", Syntax.SByte, true); 
			internalSearch("My SByte", Query.Operator.Greater, "-50", Syntax.SByte, false); 
			internalSearch("My SByte", Query.Operator.Less_Equal, "-51", Syntax.SByte, true); 
			internalSearch("My SByte", Query.Operator.Less_Equal, "-52", Syntax.SByte, false); 
			internalSearch("My SByte", Query.Operator.Less_Equal, "-50", Syntax.SByte, true); 
			internalSearch("My SByte", Query.Operator.Less, "-51", Syntax.SByte, false); 
			internalSearch("My SByte", Query.Operator.Less, "-52", Syntax.SByte, false); 
			internalSearch("My SByte", Query.Operator.Less, "-50", Syntax.SByte, true); 

			// Search for Byte types
			internalSearch("My Byte", Query.Operator.Equal, "251", Syntax.Byte, true); 
			internalSearch("My Byte", Query.Operator.Greater_Equal, "251", Syntax.Byte, true); 
			internalSearch("My Byte", Query.Operator.Greater_Equal, "252", Syntax.Byte, false); 
			internalSearch("My Byte", Query.Operator.Greater_Equal, "250", Syntax.Byte, true); 
			internalSearch("My Byte", Query.Operator.Greater, "251", Syntax.Byte, false); 
			internalSearch("My Byte", Query.Operator.Greater, "252", Syntax.Byte, false); 
			internalSearch("My Byte", Query.Operator.Greater, "250", Syntax.Byte, true); 
			internalSearch("My Byte", Query.Operator.Less_Equal, "251", Syntax.Byte, true); 
			internalSearch("My Byte", Query.Operator.Less_Equal, "252", Syntax.Byte, true); 
			internalSearch("My Byte", Query.Operator.Less_Equal, "250", Syntax.Byte, false); 
			internalSearch("My Byte", Query.Operator.Less, "251", Syntax.Byte, false); 
			internalSearch("My Byte", Query.Operator.Less, "252", Syntax.Byte, true); 
			internalSearch("My Byte", Query.Operator.Less, "250", Syntax.Byte, false); 

			// Search for Int16
			internalSearch("My Int16", Query.Operator.Equal, "-4001", Syntax.Int16, true); 
			internalSearch("My Int16", Query.Operator.Greater_Equal, "-4001", Syntax.Int16, true); 
			internalSearch("My Int16", Query.Operator.Greater_Equal, "-4002", Syntax.Int16, true); 
			internalSearch("My Int16", Query.Operator.Greater_Equal, "-4000", Syntax.Int16, false); 
			internalSearch("My Int16", Query.Operator.Greater, "-4001", Syntax.Int16, false); 
			internalSearch("My Int16", Query.Operator.Greater, "-4002", Syntax.Int16, true); 
			internalSearch("My Int16", Query.Operator.Greater, "-4000", Syntax.Int16, false); 
			internalSearch("My Int16", Query.Operator.Less_Equal, "-4001", Syntax.Int16, true); 
			internalSearch("My Int16", Query.Operator.Less_Equal, "-4002", Syntax.Int16, false); 
			internalSearch("My Int16", Query.Operator.Less_Equal, "-4000", Syntax.Int16, true); 
			internalSearch("My Int16", Query.Operator.Less, "-4001", Syntax.Int16, false); 
			internalSearch("My Int16", Query.Operator.Less, "-4002", Syntax.Int16, false); 
			internalSearch("My Int16", Query.Operator.Less, "-4000", Syntax.Int16, true); 

			// Search for UInt16
			internalSearch("My UInt16", Query.Operator.Equal, "4001", Syntax.UInt16, true); 
			internalSearch("My UInt16", Query.Operator.Greater_Equal, "4001", Syntax.UInt16, true); 
			internalSearch("My UInt16", Query.Operator.Greater_Equal, "4002", Syntax.UInt16, false); 
			internalSearch("My UInt16", Query.Operator.Greater_Equal, "4000", Syntax.UInt16, true); 
			internalSearch("My UInt16", Query.Operator.Greater, "4001", Syntax.UInt16, false); 
			internalSearch("My UInt16", Query.Operator.Greater, "4002", Syntax.UInt16, false); 
			internalSearch("My UInt16", Query.Operator.Greater, "4000", Syntax.UInt16, true); 
			internalSearch("My UInt16", Query.Operator.Less_Equal, "4001", Syntax.UInt16, true); 
			internalSearch("My UInt16", Query.Operator.Less_Equal, "4002", Syntax.UInt16, true); 
			internalSearch("My UInt16", Query.Operator.Less_Equal, "4000", Syntax.UInt16, false); 
			internalSearch("My UInt16", Query.Operator.Less, "4001", Syntax.UInt16, false); 
			internalSearch("My UInt16", Query.Operator.Less, "4002", Syntax.UInt16, true); 
			internalSearch("My UInt16", Query.Operator.Less, "4000", Syntax.UInt16, false); 


			// Search for Int32
			internalSearch("My Int32", Query.Operator.Equal, "-2147483641", Syntax.Int32, true); 
			internalSearch("My Int32", Query.Operator.Greater_Equal, "-2147483641", Syntax.Int32, true); 
			internalSearch("My Int32", Query.Operator.Greater_Equal, "-2147483642", Syntax.Int32, true); 
			internalSearch("My Int32", Query.Operator.Greater_Equal, "-2147483640", Syntax.Int32, false); 
			internalSearch("My Int32", Query.Operator.Greater, "-2147483641", Syntax.Int32, false); 
			internalSearch("My Int32", Query.Operator.Greater, "-2147483642", Syntax.Int32, true); 
			internalSearch("My Int32", Query.Operator.Greater, "-2147483640", Syntax.Int32, false); 
			internalSearch("My Int32", Query.Operator.Less_Equal, "-2147483641", Syntax.Int32, true); 
			internalSearch("My Int32", Query.Operator.Less_Equal, "-2147483642", Syntax.Int32, false); 
			internalSearch("My Int32", Query.Operator.Less_Equal, "-2147483640", Syntax.Int32, true); 
			internalSearch("My Int32", Query.Operator.Less, "-2147483641", Syntax.Int32, false); 
			internalSearch("My Int32", Query.Operator.Less, "-2147483642", Syntax.Int32, false); 
			internalSearch("My Int32", Query.Operator.Less, "-2147483640", Syntax.Int32, true); 

			// Search for UInt32
			internalSearch("My UInt32", Query.Operator.Equal, "4294967291", Syntax.UInt32, true); 
			internalSearch("My UInt32", Query.Operator.Greater_Equal, "4294967291", Syntax.UInt32, true); 
			internalSearch("My UInt32", Query.Operator.Greater_Equal, "4294967292", Syntax.UInt32, false); 
			internalSearch("My UInt32", Query.Operator.Greater_Equal, "4294967290", Syntax.UInt32, true); 
			internalSearch("My UInt32", Query.Operator.Greater, "4294967291", Syntax.UInt32, false); 
			internalSearch("My UInt32", Query.Operator.Greater, "4294967292", Syntax.UInt32, false); 
			internalSearch("My UInt32", Query.Operator.Greater, "4294967290", Syntax.UInt32, true); 
			internalSearch("My UInt32", Query.Operator.Less_Equal, "4294967291", Syntax.UInt32, true); 
			internalSearch("My UInt32", Query.Operator.Less_Equal, "4294967292", Syntax.UInt32, true); 
			internalSearch("My UInt32", Query.Operator.Less_Equal, "4294967290", Syntax.UInt32, false); 
			internalSearch("My UInt32", Query.Operator.Less, "4294967291", Syntax.UInt32, false); 
			internalSearch("My UInt32", Query.Operator.Less, "4294967292", Syntax.UInt32, true); 
			internalSearch("My UInt32", Query.Operator.Less, "4294967290", Syntax.UInt32, false); 

			// Search for Int64
			internalSearch("My Int64", Query.Operator.Equal, "-4294967291", Syntax.Int64, true); 
			internalSearch("My Int64", Query.Operator.Greater_Equal, "-4294967291", Syntax.Int64, true); 
			internalSearch("My Int64", Query.Operator.Greater_Equal, "-4294967292", Syntax.Int64, true); 
			internalSearch("My Int64", Query.Operator.Greater_Equal, "-4294967290", Syntax.Int64, false); 
			internalSearch("My Int64", Query.Operator.Greater, "-4294967291", Syntax.Int64, false); 
			internalSearch("My Int64", Query.Operator.Greater, "-4294967292", Syntax.Int64, true); 
			internalSearch("My Int64", Query.Operator.Greater, "-4294967290", Syntax.Int64, false); 
			internalSearch("My Int64", Query.Operator.Less_Equal, "-4294967291", Syntax.Int64, true); 
			internalSearch("My Int64", Query.Operator.Less_Equal, "-4294967292", Syntax.Int64, false); 
			internalSearch("My Int64", Query.Operator.Less_Equal, "-4294967290", Syntax.Int64, true); 
			internalSearch("My Int64", Query.Operator.Less, "-4294967291", Syntax.Int64, false); 
			internalSearch("My Int64", Query.Operator.Less, "-4294967292", Syntax.Int64, false); 
			internalSearch("My Int64", Query.Operator.Less, "-4294967290", Syntax.Int64, true); 

			// Search for UInt64
			internalSearch("My UInt64", Query.Operator.Equal, "4294967291", Syntax.UInt64, true); 
			internalSearch("My UInt64", Query.Operator.Greater_Equal, "4294967291", Syntax.UInt64, true); 
			internalSearch("My UInt64", Query.Operator.Greater_Equal, "4294967292", Syntax.UInt64, false); 
			internalSearch("My UInt64", Query.Operator.Greater_Equal, "4294967290", Syntax.UInt64, true); 
			internalSearch("My UInt64", Query.Operator.Greater, "4294967291", Syntax.UInt64, false); 
			internalSearch("My UInt64", Query.Operator.Greater, "4294967292", Syntax.UInt64, false); 
			internalSearch("My UInt64", Query.Operator.Greater, "4294967290", Syntax.UInt64, true); 
			internalSearch("My UInt64", Query.Operator.Less_Equal, "4294967291", Syntax.UInt64, true); 
			internalSearch("My UInt64", Query.Operator.Less_Equal, "4294967292", Syntax.UInt64, true); 
			internalSearch("My UInt64", Query.Operator.Less_Equal, "4294967290", Syntax.UInt64, false); 
			internalSearch("My UInt64", Query.Operator.Less, "4294967291", Syntax.UInt64, false); 
			internalSearch("My UInt64", Query.Operator.Less, "4294967292", Syntax.UInt64, true); 
			internalSearch("My UInt64", Query.Operator.Less, "4294967290", Syntax.UInt64, false); 

			// Search for Char
			internalSearch("My Char", Query.Operator.Equal, "3001", Syntax.Char, true); 
			internalSearch("My Char", Query.Operator.Greater_Equal, "3001", Syntax.Char, true); 
			internalSearch("My Char", Query.Operator.Greater_Equal, "3002", Syntax.Char, false); 
			internalSearch("My Char", Query.Operator.Greater_Equal, "3000", Syntax.Char, true); 
			internalSearch("My Char", Query.Operator.Greater, "3001", Syntax.Char, false); 
			internalSearch("My Char", Query.Operator.Greater, "3002", Syntax.Char, false); 
			internalSearch("My Char", Query.Operator.Greater, "3000", Syntax.Char, true); 
			internalSearch("My Char", Query.Operator.Less_Equal, "3001", Syntax.Char, true); 
			internalSearch("My Char", Query.Operator.Less_Equal, "3002", Syntax.Char, true); 
			internalSearch("My Char", Query.Operator.Less_Equal, "3000", Syntax.Char, false); 
			internalSearch("My Char", Query.Operator.Less, "3001", Syntax.Char, false); 
			internalSearch("My Char", Query.Operator.Less, "3002", Syntax.Char, true); 
			internalSearch("My Char", Query.Operator.Less, "3000", Syntax.Char, false);
  
			// Search for Single
			internalSearch("My Single", Query.Operator.Equal, "42949.67291", Syntax.Single, true); 
			internalSearch("My Single", Query.Operator.Greater_Equal, "42949.67291", Syntax.Single, true); 
			internalSearch("My Single", Query.Operator.Greater_Equal, "42949.67292", Syntax.Single, false); 
			internalSearch("My Single", Query.Operator.Greater_Equal, "42949.67290", Syntax.Single, true); 
			internalSearch("My Single", Query.Operator.Greater, "42949.67291", Syntax.Single, false); 
			internalSearch("My Single", Query.Operator.Greater, "42949.67292", Syntax.Single, false); 
			internalSearch("My Single", Query.Operator.Greater, "42949.67290", Syntax.Single, true); 
			internalSearch("My Single", Query.Operator.Less_Equal, "42949.67291", Syntax.Single, true); 
			internalSearch("My Single", Query.Operator.Less_Equal, "42949.67292", Syntax.Single, true); 
			internalSearch("My Single", Query.Operator.Less_Equal, "42949.67290", Syntax.Single, false); 
			internalSearch("My Single", Query.Operator.Less, "42949.67291", Syntax.Single, false); 
			internalSearch("My Single", Query.Operator.Less, "42949.67292", Syntax.Single, true); 
			internalSearch("My Single", Query.Operator.Less, "42949.67290", Syntax.Single, false);

			// Search For Boolean
			internalSearch("My Boolean", Query.Operator.Equal, false.ToString(), Syntax.Boolean, true); 
			internalSearch("My Boolean", Query.Operator.Equal, true.ToString(), Syntax.Boolean, false); 
			
			// Search for DateTime
			internalSearch("My DateTime", Query.Operator.Equal, "4294967291", Syntax.DateTime, true); 
			internalSearch("My DateTime", Query.Operator.Greater_Equal, "4294967291", Syntax.DateTime, true); 
			internalSearch("My DateTime", Query.Operator.Greater_Equal, "4294967292", Syntax.DateTime, false); 
			internalSearch("My DateTime", Query.Operator.Greater_Equal, "4294967290", Syntax.DateTime, true); 
			internalSearch("My DateTime", Query.Operator.Greater, "4294967291", Syntax.DateTime, false); 
			internalSearch("My DateTime", Query.Operator.Greater, "4294967292", Syntax.DateTime, false); 
			internalSearch("My DateTime", Query.Operator.Greater, "4294967290", Syntax.DateTime, true); 
			internalSearch("My DateTime", Query.Operator.Less_Equal, "4294967291", Syntax.DateTime, true); 
			internalSearch("My DateTime", Query.Operator.Less_Equal, "4294967292", Syntax.DateTime, true); 
			internalSearch("My DateTime", Query.Operator.Less_Equal, "4294967290", Syntax.DateTime, false); 
			internalSearch("My DateTime", Query.Operator.Less, "4294967291", Syntax.DateTime, false); 
			internalSearch("My DateTime", Query.Operator.Less, "4294967292", Syntax.DateTime, true); 
			internalSearch("My DateTime", Query.Operator.Less, "4294967290", Syntax.DateTime, false); 

			// Search for Uri.
			internalSearch("My Uri", Query.Operator.Equal, "http://server/path/doc.htm", Syntax.Uri, true); 
			internalSearch("My Uri", Query.Operator.Begins, "http:", Syntax.Uri, true); 
			internalSearch("My Uri", Query.Operator.Contains, "/server/", Syntax.Uri, true); 
			internalSearch("My Uri", Query.Operator.Ends, "doc.htm", Syntax.Uri, true); 
			internalSearch("My Uri", Query.Operator.Ends, "doc.html", Syntax.Uri, false); 

			// Search for Xml.
			internalSearch("My Xml", Query.Operator.Equal, "<tag1>this is xml</tag1>", Syntax.XmlDocument, true); 
			internalSearch("My Xml", Query.Operator.Begins, "<tag1>", Syntax.XmlDocument, true); 
			internalSearch("My Xml", Query.Operator.Contains, "this is xml", Syntax.XmlDocument, true); 
			internalSearch("My Xml", Query.Operator.Ends, "</tag1>", Syntax.XmlDocument, true); 
			internalSearch("My Xml", Query.Operator.Ends, "</tag1//>", Syntax.XmlDocument, false);
		}

		/// <summary>
		/// Delete all the objects from firstRecordId to lastRecordId.
		/// </summary>
		private TimeSpan DeleteObjects(int recordCount)
		{
			ArrayList objectList = GetObjectXml(recordCount, false);

			DateTime startTime = DateTime.Now;
			
			foreach (XmlDocument objectDoc in objectList)
			{
				//writeXml(recordDoc.InnerXml);
				provider.DeleteRecords(objectDoc.InnerXml, collectionId);
			}
			TimeSpan deltaTime = DateTime.Now - startTime;
			return (deltaTime);
		}

		private TimeSpan DeleteColection(string collectionId)
		{
			DateTime startTime = DateTime.Now;
			provider.DeleteCollection(collectionId);
			TimeSpan deltaTime = DateTime.Now - startTime;
			return (deltaTime);
		}

		private void ValidateDelete(int recordCount)
		{
			// Make sure the records are gone.
			int i;
			for (i = firstRecordId; i <= recordCount; ++i)
			{
				string sObject = null;
				try
				{
					sObject = provider.GetRecord(recordId + i, collectionId);
				}
				catch {}
				if (sObject != null)
				{
					throw (new ApplicationException("Object Was not deleted." + recordId + i));
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void UnmanagedCall()
		{
			//provider.NOP(10000000);
		}

		private void writeXml(string s)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(s);
			XmlElement el = doc.DocumentElement;
			XmlTextWriter w = new XmlTextWriter(Console.Out);
			w.Formatting = Formatting.Indented;
			doc.WriteTo(w);
			Console.WriteLine();
		}
	}

	public class TestProperty
	{

	}
}
