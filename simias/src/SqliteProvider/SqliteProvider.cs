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
using System.Text;
using System.Xml;
using System.Collections;
using Simias.Storage.Provider;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.IO;
using System.Data;

namespace Simias.Storage.Provider.Sqlite
{
	#region RecordTable class
	/// <summary>
	/// Table That holds the shallow record information. One entry exists
	/// for each node in the store.
	/// The columns are as follows
	/// ID			The ID for the record. 
	///	Name		The name of the record.
	///	Type		The record type.
	/// </summary>
	class RecordTable
	{
		internal static readonly string	TableName = "T_Record";
		internal static readonly string	Id = "ID";
		internal static readonly string	Name = "Name";
		internal static readonly string	Type = "Type";
		internal static readonly string CollectionId = "ColId";

		private static string createString = string.Format(
			"CREATE TABLE {0} ({1} char(40), {2} varchar(255), {3} varchar(255), {4} char(40), PRIMARY KEY({1}))",
			TableName,
			Id,
			Name,
			Type,
			CollectionId);

		private static string idIndexString = string.Format(
			"CREATE INDEX I_{1}_{0} ON {0} ({1})",
			TableName,
			Id);

		private static string nameIndexString = string.Format(
			"CREATE INDEX I_{1}_{0} ON {0} ({1})",
			TableName,
			Name);

		private static string typeIndexString = string.Format(
			"CREATE INDEX I_{1}_{0} ON {0} ({1})",
			TableName,
			Type);

		public static void Create(IDbCommand command)
		{
			// Create the Table.
			command.CommandText = createString;

			command.ExecuteNonQuery();
			// Create the indexes for the Id, Name, and Type.
			command.CommandText = idIndexString;
			command.ExecuteNonQuery();

			command.CommandText = nameIndexString;
			command.ExecuteNonQuery();

			command.CommandText = typeIndexString;
			command.ExecuteNonQuery();
		}

		public static void Insert(Record record, string collectionId, IDbCommand command)
		{
			// Add the Record to the record table.
			command.CommandText = string.Format(
				"INSERT INTO {0} values('{1}','{2}','{3}','{4}')",
				TableName,
				record.Id,
				record.Name.Replace("'", "''"),
				record.Type.Replace("'", "''"),
				collectionId);
			command.ExecuteNonQuery();
		}

		public static void Delete(string recordId, IDbCommand command)
		{
			command.CommandText = string.Format(
				"DELETE FROM {0} Where {1} = '{2}'", 
				TableName,
				Id,
				recordId);
			command.ExecuteNonQuery();
		}

		public static void RemoveCollection(string collectionId, IDbCommand command)
		{
			command.CommandText = string.Format(
				"DELETE FROM {0} WHERE {1} = '{2}'",
				TableName,
				CollectionId,
				collectionId);
			command.ExecuteNonQuery();
		}


		public static bool Select(string recordId, IDbCommand command, out string sName, out string sType)
		{
			bool found = false;
			
			command.CommandText = string.Format(
				"SELECT {0},{1} FROM {2} WHERE {3} = '{4}'", 
				Name,
				Type,
				TableName,
				Id,
				recordId);
			IDataReader reader = command.ExecuteReader();
			if (reader.Read())
			{
				sName = reader[0].ToString();
				sType = reader[1].ToString();
				found = true;
			}
			else
			{
				sName = null;
				sType = null;
			}
			reader.Close();
			
			return found;
		}
	};
	#endregion
		
	#region SchemaTable class
	/// <summary>
	/// Property Table. Holds the property schema definitions.
	/// The columns are as follows.
	/// Name		The property Name.
	/// Type		The syntax of the property.
	/// </summary>
	class SchemaTable
	{
		private static Hashtable		nameTable = new Hashtable();
		internal static readonly string	TableName = "T_Schema";
		internal static readonly string	Name = "Name";
		internal static readonly string	Type = "Type";

		private static string createString = string.Format(
			"CREATE TABLE {0} ({1} varchar(255) PRIMARY KEY, {2} varchar(255))",
			TableName,
			Name,
			Type);

		public static void Create(IDbCommand command)
		{
			command.CommandText = createString;
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Used to add a property to the property table.
		/// If the property already exists the call is successful.
		/// </summary>
		/// <param name="name">Name of the property</param>
		/// <param name="type">The syntax of the property</param>
		/// <param name="command">The command object used to issue the query.</param>
		public static void Insert(string name, string type, IDbCommand command)
		{
			// Check in the name table for this property.
			if (!nameTable.Contains(name))
			{
				string pType = null;
				string safeName = name.Replace("'", "''");
			
				// See if exists in the DataBase.
				command.CommandText = string.Format(
					"SELECT Type FROM {0} WHERE {1} = '{2}'",
					TableName,
					Name,
					safeName);

				pType = (string)command.ExecuteScalar();

				if (pType == null)
				{
					// Add this property to the table.
					command.CommandText = string.Format(
						"INSERT INTO {0} values('{1}','{2}')", 
						TableName,
						safeName,
						type.Replace("'", "''"));
					command.ExecuteNonQuery();
				}
				// Add the property to the hash table.
				nameTable.Add(name, type);
			}
		}
	};
	#endregion

	#region ValueTable class
	/// <summary>
	/// The value table holds the properties for a record. One table
	/// exists for each collection.  There is also a collection table
	/// that holds a copy of all the collection records.  This allows
	/// for an efficient search across collecitons.
	/// The columns are as follows.
	/// Value_Id		The id of this property for this record. Used to
	///					order properties on a record.
	/// PropertyName	The name of the property.
	/// PropertyType	The syntax of the property.
	/// RecordId		The record id that this property belongs to.
	/// Value			The value of the property.
	/// Flags			The flags of the property.
	/// </summary>
	class ValueTable
	{
		internal static readonly string vTableName = "T_ValueTable";
		internal static readonly string	Id = "ID";
		internal static readonly string	Name = "Name";
		internal static readonly string	Type = "Type";
		internal static readonly string	RecordId = "RecordID";
		internal static readonly string	Value = "Value";
		internal static readonly string	Flags = "Flags";

		private static string createString = string.Format(
			"CREATE TABLE '{0}' ({1} char(40), {2} char(255), {3} char(40), {4} char(40), {5} INTEGER, {6} int)",
			"{0}",
			Id,
			Name,
			Type,
			RecordId,
			Value,
			Flags);

		private static string createIdIndexString = string.Format(
			"CREATE INDEX 'I_{1}_{0}' ON '{0}' ({1})",
			"{0}",
			RecordId);

		private static string createValueIndexString = string.Format(
			"CREATE INDEX 'I_{2}_{0}' ON '{0}' ({1}, {2})", 
			"{0}",
			Name,
			Value);

		/// <summary>
		/// Called to create a value table. There will be one table
		/// for each collection plus one table for all collections.
		/// A transaction must be held when calling.
		/// </summary>
		/// <param name="tableName">Name of the table to create.</param>
		/// <param name="command">The command object used to issue the query.</param>
		public static void Create(string tableName, IDbCommand command)
		{
			//string safeTableName = tableName.Replace("'", "''");
			string safeTableName = vTableName;

			// Create the Collection Table.
			command.CommandText = string.Format(createString, safeTableName);
			command.ExecuteNonQuery();

			// Create the Id Index.
			command.CommandText = string.Format(createIdIndexString, safeTableName);
			command.ExecuteNonQuery();

			// Create the Value Index.
			command.CommandText = string.Format(createValueIndexString, safeTableName);
			command.ExecuteNonQuery();
		}

		public static void Drop(string table, IDbCommand command)
		{
			command.CommandText = string.Format("DROP TABLE '{0}'", table.Replace("'", "''"));
			command.ExecuteNonQuery();
		}


		public static void Insert(string table, Record record, IDbCommand command)
		{
			int valueId = 0;
			//string safeTable = table.Replace("'", "''");
			string safeTable = vTableName;

			StringBuilder sb = new StringBuilder();

			foreach(Property property in record)
			{
				SchemaTable.Insert(property.Name, property.Type, command);
				// Add this record to the table.
				sb.Append(string.Format("INSERT INTO '{0}' values('{1}','{2}','{3}','{4}','{5}','{6}');", 
					safeTable,
					valueId++, 
					property.Name,
					property.Type,
					record.Id, 
					property.Value.Replace("'", "''"), 
					property.Flags.Replace("'", "''")));
				
				/*
				command.CommandText = string.Format("INSERT INTO '{0}' values('{1}','{2}','{3}','{4}','{5}','{6}')", 
					safeTable,
					valueId++, 
					property.Name,
					property.Type,
					record.Id, 
					property.Value.Replace("'", "''"), 
					property.Flags.Replace("'", "''"));
				command.ExecuteNonQuery();
				*/
			}
			sb.Length = sb.Length - 1;
			command.CommandText = sb.ToString();
			command.ExecuteNonQuery();
		}

		public static void Delete(string table, string recordId, IDbCommand command)
		{
			//string safeTable = table.Replace("'", "''");
			string safeTable = vTableName;

			// Delete the Values.
			command.CommandText = string.Format(
				"DELETE FROM '{0}' Where {1} = '{2}'", 
				safeTable,
				RecordId, 
				recordId);
			command.ExecuteNonQuery();
		}

		public static IDataReader Select(string table, string recordId, IDbCommand command)
		{
			//string safeTable = table.Replace("'", "''");
			string safeTable = vTableName;

			command.CommandText = string.Format(
				"SELECT {1}, {2}, {3}, {4} FROM '{0}' WHERE {5} = '{6}' ORDER by {7}", 
				safeTable,
				Name,
				Type,
				Flags,
				Value,
				RecordId,
				recordId,
				Id);
			return command.ExecuteReader();
		}

		public static bool Read(IDataReader reader, out string sName, out string sType, out string sFlags, out string sValue)
		{
			bool propertyFound = false;

			if (reader.Read())
			{
				sName = reader[0].ToString();
				sType = reader[1].ToString();
				sFlags = reader[2].ToString();
				sValue = reader[3].ToString();
				propertyFound = true;
			}
			else
			{
				sName = null;
				sType = null;
				sFlags = null;
				sValue = null;
			}
			return propertyFound;
		}
	};
	#endregion

	/// <summary>
	/// Class used to keep a connection per thread.
	/// </summary>
	internal class InternalConnection
	{
		internal SqliteConnection		sqliteDb;
		internal IDbCommand				command;

		/// <summary>
		/// Called to initialize the DataBase.
		/// </summary>
		internal InternalConnection()
		{
			sqliteDb = new SqliteConnection();
		}

		internal void Dispose()
		{
			command.Dispose();
			sqliteDb.Close();
		}
	}

	#region SqliteProvider class

	/// <summary>
	/// Class that implements the Simias.Storage.Provider.IProvider interface 
	/// using sqlite embeded DatatBase.
	/// 
	/// The following tables exist in the database.
	/// RecordTable			There is one record table it has an entry for each
	///						record in the store.
	///	SchemaTable			There is one SchemaTable it holds a defenition of
	///						all property types defined in the store.
	///	ValueTable			One ValueTable for each collection plus one collection
	///						to hold all collections exists.  All records for a
	///						given collection will exist in a ValueTable for that
	///						collection.  The record that represents the collection
	///						is stored twice. Once in the global collection table
	///						and once in the table for that collection.  This is
	///						done for faster searches.
	/// </summary>
	public class SqliteProvider : MarshalByRefObject, IProvider
	{
		#region Varibles

		Hashtable				connectionTable = new Hashtable();

		Configuration			conf;

		/// <summary>
		/// Collection Table. Holds all collection nodes.
		/// </summary>
		const string			CollectionTable = "T_Collection";
		
		/// <summary>
		/// All tables are prefixed with this string.
		/// </summary>
		const string			TablePrefix = "T_";
		/// <summary>
		/// All Index Table are prefixed with this string.
		/// </summary>
		
		bool					AlreadyDisposed;
		string					DbPath;
		const string			Name = "ColSqlite.db";
		const string 			version = "0.1";
		string					storePath;
		bool					opened = false;
		#endregion

		InternalConnection sqliteConn
		{
			get
			{
				InternalConnection instance;
				bool newConnection = false;
				object threadId = Thread.CurrentThread.GetHashCode();
				lock (connectionTable)
				{
					if (connectionTable.Contains(threadId))
					{
						instance = (InternalConnection)connectionTable[threadId];	
					}
					else
					{
						instance = new InternalConnection();
						connectionTable.Add(threadId, instance);
						newConnection = true;
					}
				}
				if (newConnection && opened)
				{
					OpenStore();
				}

				return instance;
			}
		}

		#region Constructor/Finalizer
		/// <summary>
		/// 
		/// </summary>
		/// <param name="conf">The Configuration object used for this instance.</param>
		public SqliteProvider(Configuration conf)
		{
			this.conf = conf;
			storePath = Path.GetFullPath(conf.Path);
			DbPath = System.IO.Path.Combine(storePath, Name);
			//sqliteDb = new SqliteConnection();
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~SqliteProvider()
		{
			// Call the dispose method with true to indicate that
			// It was called from the finalizer.
			Dispose(true);
		}
		#endregion
		
		#region Private Methods

		/// <summary>
		/// Called to create a record in the specified table.
		/// A transaction must be held when called.
		/// </summary>
		/// <param name="table">Table to create the record in.</param>
		/// <param name="collectionId">The collection that this node belongs to.</param>
		/// <param name="recordXml">Xml that represents the record.</param>
		/// <param name="command">Command object used to control database.</param>
		private void CreateRecord(string table, string collectionId, XmlElement recordXml, IDbCommand command)
		{
			Record record = new Record(recordXml);
			
			// Delete the old record first.
			try 
			{
				InternalDeleteRecord(record.Id, collectionId, command);
			}
			catch {}

			RecordTable.Insert(record, collectionId, command);
		
			// Now add to the value table.
			ValueTable.Insert(table, record, command);

			// MultiTableTable
			//if (record.Id == collectionId)
			//{
			//	// Now Add the collection to the global collection Table.
			//	// don't add it to the node table. It will be added later.
			//	ValueTable.Delete(CollectionTable, record.Id, command);
			//	ValueTable.Insert(CollectionTable, record, command);
			//}
			//
		}

		/// <summary>
		/// Called to remove a collection and all its contents from the DataBase.  A transaction must
		/// be held when calling.
		/// </summary>
		/// <param name="collectionId">The collection Id to remove.</param>
		/// <param name="command">Command object used to control database.</param>
		private void RemoveCollection(string collectionId, IDbCommand command)
		{
			// MultiTableTable
			//// Remove the collection table.
			//ValueTable.Drop(TablePrefix+collectionId, command);
			
			// Now remove from the Collection Table.
			ValueTable.Delete(CollectionTable, collectionId, command);

			// Now remove all nodes from the Record table that belong to the collection.
			RecordTable.RemoveCollection(collectionId, command);
		}

		/// <summary>
		/// Method Used to create a record in the store.
		/// The transaction must already be owned.
		/// </summary>
		/// <param name="doc">XML document that describes the records</param>
		/// <param name="collectionId">The collection to create the records in.</param>
		/// <param name="command">Command object used to control database.</param>
		private void CreateRecords(XmlDocument doc, string collectionId, IDbCommand command)
		{
			string table = TablePrefix + collectionId;
			XmlElement root = doc.DocumentElement;
			XmlNodeList recordList = root.SelectNodes(XmlTags.ObjectTag);

			foreach (XmlElement recordEl in recordList)
			{
				CreateRecord(table, collectionId, recordEl, command);
			}
		}

		/// <summary>
		/// Called to delete a Record.  The record is specified by it ID.
		/// </summary>
		/// <param name="recordId">string that contains the ID of the Object to delete</param>
		/// <param name="collectionId">The id of the collection that contains this object</param>
		/// <param name="command">Command object used to control database.</param>
		public void InternalDeleteRecord(string recordId, string collectionId, IDbCommand command)
		{
			// Delete from the record table.
			RecordTable.Delete(recordId, command);
			
			// Delete the Values.
			ValueTable.Delete(TablePrefix+collectionId, recordId, command);
		}

		/// <summary>
		/// Called to cleanup unmanaged resources.
		/// </summary>
		/// <param name="inFinalize"></param>
		private void Dispose(bool inFinalize)
		{
			if (!AlreadyDisposed)
			{
				if (!inFinalize)
				{
					GC.SuppressFinalize(this);
				}

				lock (connectionTable)
				{
					if (connectionTable.Count > 0)
					{
						foreach (DictionaryEntry entry in connectionTable)
						{
							InternalConnection conn = (InternalConnection)entry.Value;
							conn.Dispose();
						}
						connectionTable.Clear();
					}
				}
				AlreadyDisposed = true;
			}
		}

		/// <summary>
		/// Called to initialize the DataBase.
		/// </summary>
		private void Init(InternalConnection conn)
		{
			IDbCommand command = conn.command = conn.sqliteDb.CreateCommand();
			
			// Turn of synchronous access.
			command.CommandText = "PRAGMA cache_size = 10000";
			command.ExecuteNonQuery();

			command.CommandText = "PRAGMA synchronous = OFF";
			command.ExecuteNonQuery();
		}

		#endregion

		#region IProvider Members

		#region Store Calls

		/// <summary>
		/// Called to Create a new Collection Store at the specified location.
		/// </summary>
		public void CreateStore()
		{
			InternalConnection conn = sqliteConn;
			SqliteConnection sqliteDb = conn.sqliteDb;
			lock (sqliteDb)
			{
				// Make sure the Data Base does not exist.
				if (!File.Exists(DbPath))
				{
					// Create the store
					sqliteDb.ConnectionString = "URI=file:" + DbPath;
					sqliteDb.Open();
					opened = true;

					// Set the version.
					conf.Version = version;
					
					IDbTransaction trans = sqliteDb.BeginTransaction();
					try
					{
						// Create the Record Table.
						Init(conn);
						IDbCommand command = conn.command;
						RecordTable.Create(command);
						SchemaTable.Create(command);
						ValueTable.Create(CollectionTable, command);
					}
					catch
					{
						trans.Rollback();
						throw new CSPException("Failed to create DataBase", Provider.Error.Create);
					}
					trans.Commit();
				}
				else
				{
					throw (new CSPException("DataBase already exists", Provider.Error.Exists));
				}
			}
			AlreadyDisposed = false;
		}


		/// <summary>
		/// Called to Delete the opened CollectionStore.
		/// </summary>
		public void DeleteStore()
		{
			Dispose(false);
			File.Delete(DbPath);
			Provider.Delete(storePath);
		}


		/// <summary>
		/// Called to Open an existing Collection store at the specified location.
		/// </summary>
		public void OpenStore()
		{
			InternalConnection conn = sqliteConn;
			SqliteConnection sqliteDb = conn.sqliteDb;
			if (File.Exists(DbPath))
			{
				// Make sure the version is correct.
				if (conf.Version != version)
				{
					throw new CSPException("Wrong DataBase Version", Provider.Error.Version);
				}
				sqliteDb.ConnectionString = "URI=file:" + DbPath;
				sqliteDb.Open();
				opened = true;
				Init(conn);
				AlreadyDisposed = false;
			}
			else
			{
				throw new CSPException("Does Not exist", 0);
			}
		}

		
		#endregion

		#region Collection Calls.

		/// <summary>
		/// Called to create or modify a collection.
		/// </summary>
		/// <param name="collectionId">The Id of the collection to create.</param>
		public void CreateCollection(string collectionId)
		{
			// Nothing to do.
		}

		/// <summary>
		/// Called to delete an existing collection.
		/// </summary>
		/// <param name="collectionId">The ID of the collection to delete.</param>
		public void DeleteCollection(string collectionId)
		{
			InternalConnection conn = sqliteConn;
			IDbTransaction trans = conn.sqliteDb.BeginTransaction();
			try
			{
				RemoveCollection(collectionId, conn.command);
			}
			catch 
			{
				trans.Rollback();
				throw; // (ex);
			}
			trans.Commit();
		}

		#endregion

		#region Record Calls.

		/// <summary>
		/// Called to create or modify 1 or more Records
		/// </summary>
		/// <param name="recordXml">Xml string that describes the new/modified Records.</param>
		/// <param name="collectionId">The id of the collection containing these objects.</param>
		public void CreateRecord(string recordXml, string collectionId)	
		{
			InternalConnection conn = sqliteConn;
			IDbTransaction trans = conn.sqliteDb.BeginTransaction();
			try
			{
				if (recordXml != null)
				{
					XmlDocument createXml = new XmlDocument();
					createXml.LoadXml(recordXml);
					CreateRecords(createXml, collectionId, conn.command);
				}
			}
			catch 
			{
				trans.Rollback();
				throw; // (ex);
			}
			trans.Commit();
		}

		/// <summary>
		/// Called to delete a Record.  The record is specified by it ID.
		/// </summary>
		/// <param name="recordId">string that contains the ID of the Object to delete</param>
		/// <param name="collectionId">The id of the collection that contains this object</param>
		public void DeleteRecord(string recordId, string collectionId)
		{
			InternalConnection conn = sqliteConn;
			IDbTransaction trans = conn.sqliteDb.BeginTransaction();
			try
			{
				InternalDeleteRecord(recordId, collectionId, conn.command);
			}
			catch //(System.Exception ex)
			{
				trans.Rollback();
				throw; // ex;
			}
            
			trans.Commit();
		}

		/// <summary>
		/// Called to delete 1 or more Records.
		/// </summary>
		/// <param name="recordXml">Xml string describing Records to delete.</param>
		/// <param name="collectionId">The id of the collection that contains these objects.</param>
		public void DeleteRecords(string recordXml, string collectionId)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(recordXml);
			XmlElement root = doc.DocumentElement;
			XmlNodeList objectList = root.SelectNodes(XmlTags.ObjectTag);
			
			InternalConnection conn = sqliteConn;
			IDbTransaction trans = conn.sqliteDb.BeginTransaction();
			try
			{
				foreach (XmlElement recordEl in objectList)
				{
					// Get ID.
					string id = recordEl.GetAttribute(XmlTags.IdAttr);
					if (id != null)
					{
						InternalDeleteRecord(id, collectionId, conn.command);
					}
				}
			}
			catch
			{
				Console.WriteLine("Failed Delete");
				trans.Rollback();
				throw; // (ex);
			}
			trans.Commit();
		}

		/// <summary>
		/// Called to ge a Record.  The record is returned as an XML string representation.  
		/// </summary>
		/// <param name="recordId">string that contains the ID of the Record to retrieve</param>
		/// <param name="collectionId">The id of the collection that contains this Record.</param>
		/// <returns>XML string describing the Record</returns>
		public string GetRecord(string recordId, string collectionId)
		{
			string recordXml = null;
			XmlDocument doc = new XmlDocument();
			
			string Name;
			string Type;
			SqliteConnection sqliteDb = sqliteConn.sqliteDb;
			IDbCommand command = sqliteConn.command;
			if (RecordTable.Select(recordId, command, out Name, out Type))
			{
				XmlElement node, root;
				root = doc.CreateElement(XmlTags.ObjectListTag);
				doc.AppendChild(root);
				node = new Record(Name, recordId, Type).ToXml(doc);			
				root.AppendChild(node);
				
				// Now get the properties.
				IDataReader reader = ValueTable.Select(TablePrefix+collectionId, recordId, command);
				if (reader != null)
				{
					string Flags;
					string Value;
					while (ValueTable.Read(reader, out Name, out Type, out Flags, out Value))
					{
						XmlElement property = Property.CreateXmlNode(doc, Name, Type, Flags, Value);
						node.AppendChild(property);
					}
				}
				recordXml = doc.InnerXml;
				reader.Close();
			}
			return (recordXml);
		}

		#endregion
		
		#region Query Calls

		/// <summary>
		/// Method used to search for Records using the specified query.
		/// </summary>
		/// <param name="query">Query used for this search</param>
		/// <returns></returns>
		public IResultSet Search(Query query)
		{
			IResultSet resultSet = null;
			string	op = null;
			bool isAttribute = false;
			string attribute = null;

			string safeValue = query.Value.Replace("'", "''");
			string safeProperty = query.Property.Replace("'", "''");
			
			switch (query.Property)
			{
				case BaseSchema.ObjectName:
					attribute = RecordTable.Name;
					isAttribute = true;
					break;
				case BaseSchema.ObjectId:
					attribute = RecordTable.Id;
					isAttribute = true;
					break;
				case BaseSchema.ObjectType:
					attribute = RecordTable.Type;
					isAttribute = true;
					break;
			}
			switch (query.Operation)
			{
				case Query.Operator.Equal:
					op = string.Format("= '{0}'", safeValue);
					break;
				case Query.Operator.Not_Equal:
					op = string.Format("!= '{0}'", safeValue);
					break;
				case Query.Operator.Begins:
					op = string.Format("LIKE '{0}%'", safeValue);
					break;
				case Query.Operator.Ends:
					op = string.Format("LIKE '%{0}'", safeValue);
					break;
				case Query.Operator.Contains:
					op = string.Format("LIKE '%{0}%'", safeValue);
					break;
				case Query.Operator.Greater:
					op = string.Format("> '{0}'", safeValue);
					break;
				case Query.Operator.Less:
					op = string.Format("< '{0}'", safeValue);
					break;
				case Query.Operator.Greater_Equal:
					op = string.Format(">= '{0}'", safeValue);
					break;
				case Query.Operator.Less_Equal:
					op = string.Format("<= '{0}'", safeValue);
					break;
			}

			if (op != null)
			{
				string selectNodes;
				if (isAttribute)
				{
					if (query.CollectionId != null)
					{
						selectNodes = string.Format(
							"SELECT DISTINCT {0},{1},{2} FROM {3} WHERE {4} {5} AND {6} = '{7}'",
							RecordTable.Id,
							RecordTable.Name,
							RecordTable.Type,
							RecordTable.TableName,
							attribute, 
							op, 
							RecordTable.CollectionId,
							query.CollectionId.Replace("'", "''"));
					}
					else
					{
						// Only look for collection nodes.
						selectNodes = string.Format(
							"SELECT DISTINCT {0},{1},{2} FROM {3} WHERE {4} {5} AND {2} LIKE 'Collection%'", 
							RecordTable.Id,
							RecordTable.Name,
							RecordTable.Type,
							RecordTable.TableName,
							attribute, 
							op);
					}
				}
				else
				{
					if (query.CollectionId != null)
					{
						selectNodes = string.Format(
							"SELECT {0},{1},{2} FROM {3} WHERE {0} IN (Select {4} from '{5}' WHERE {6} = '{7}' AND {8} {9})",
							RecordTable.Id,
							RecordTable.Name,
							RecordTable.Type,
							RecordTable.TableName,
							ValueTable.RecordId,
							ValueTable.vTableName, //TablePrefix + query.CollectionId.Replace("'", "''"),
							ValueTable.Name,
							safeProperty,
							ValueTable.Value,
							op);
					}
					else
					{
						selectNodes = string.Format(
							"SELECT {0},{1},{2} FROM {3} WHERE {0} IN (Select {4} from '{5}' WHERE {6} = '{7}' AND {8} {9})",
							RecordTable.Id,
							RecordTable.Name,
							RecordTable.Type,
							RecordTable.TableName,
							ValueTable.RecordId,
							ValueTable.vTableName, //CollectionTable,
							ValueTable.Name,
							safeProperty,
							ValueTable.Value,
							op);
					}
				}
				IDbCommand command = sqliteConn.command;
				command.CommandText = selectNodes;
				IDataReader reader = command.ExecuteReader();
				resultSet = new SqliteResultSet(reader);
			}
			
			return (resultSet);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Property to get the directory to where the store is rooted.
		/// </summary>
		public Uri StoreDirectory
		{
			get {return new Uri(storePath);}
		}
		
		#endregion

		#endregion
		
		#region IDisposable Members

		/// <summary>
		/// Method used to cleanup unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}

	#endregion
}
