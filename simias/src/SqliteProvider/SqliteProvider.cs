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

		/// <summary>
		/// Creates a Record Table.
		/// </summary>
		/// <param name="command">The command object to control sqlite.</param>
		public static void Create(IDbCommand command)
		{
			try
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
			catch (Exception ex)
			{
				throw new CreateException(command.CommandText, ex);
			}
		}

		/// <summary>
		/// Insert a record into the Record table.
		/// </summary>
		/// <param name="record">The record to insert.</param>
		/// <param name="collectionId">The collection the record belongs to.</param>
		/// <param name="command">The command object to control sqlite.</param>
		public static void Insert(Record record, string collectionId, IDbCommand command)
		{
			try
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
			catch (Exception ex)
			{
				throw new CreateException(command.CommandText, ex);
			}
		}

		/// <summary>
		/// Delete a record from the Record Table.
		/// </summary>
		/// <param name="recordId">The record to delete.</param>
		/// <param name="command">The command object to control sqlite.</param>
		public static void Delete(string recordId, IDbCommand command)
		{
			command.CommandText = string.Format(
				"DELETE FROM {0} Where {1} = '{2}'", 
				TableName,
				Id,
				recordId);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Remove a collection from the Record Table.
		/// </summary>
		/// <param name="collectionId">The collection to remove.</param>
		/// <param name="command">The command object to control sqlite.</param>
		public static void RemoveCollection(string collectionId, IDbCommand command)
		{
			command.CommandText = string.Format(
				"DELETE FROM {0} WHERE {1} = '{2}'",
				TableName,
				CollectionId,
				collectionId);
			command.ExecuteNonQuery();
		}


		/// <summary>
		/// Find a the specified record in the Record Table.
		/// </summary>
		/// <param name="recordId">The record to find.</param>
		/// <param name="command">The command object to control sqlite.</param>
		/// <param name="sName">The name of the record</param>
		/// <param name="sType">The type of the record.</param>
		/// <returns>True if found.</returns>
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

		/// <summary>
		/// Create the Schema Table.
		/// </summary>
		/// <param name="command">The command object to control sqlite.</param>
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
		internal static readonly string	Name = "Name";
		internal static readonly string	Type = "Type";
		internal static readonly string	RecordId = "RecordID";
		internal static readonly string	Value = "Value";
		internal static readonly string	Flags = "Flags";

		private static string createString = string.Format(
			"CREATE TABLE '{0}' ({1} char(255), {2} char(40), {3} char(40), {4} INTEGER, {5} int)",
			"{0}",
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
		/// <param name="command">The command object used to issue the query.</param>
		public static void Create(IDbCommand command)
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

		/// <summary>
		/// Delete the Value table.
		/// </summary>
		/// <param name="command">The command object to control sqlite.</param>
		public static void Drop(IDbCommand command)
		{
			command.CommandText = string.Format("DROP TABLE '{0}'", vTableName);
			command.ExecuteNonQuery();
		}


		/// <summary>
		/// Insert a record into the value table.
		/// </summary>
		/// <param name="record">The record to insert.</param>
		/// <param name="command">The command object to control sqlite.</param>
		public static void Insert(Record record, IDbCommand command)
		{
			string safeTable = vTableName;

			StringBuilder sb = new StringBuilder();

			foreach(Property property in record)
			{
				SchemaTable.Insert(property.Name, property.Type, command);
				// Add this record to the table.
				sb.Append(string.Format("INSERT INTO '{0}' values('{1}','{2}','{3}','{4}','{5}');", 
					safeTable,
					property.Name,
					property.Type,
					record.Id, 
					property.Value.Replace("'", "''"), 
					property.Flags.Replace("'", "''")));
				
			}
			sb.Length = sb.Length - 1;
			command.CommandText = sb.ToString();
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Delete a record from the value table.
		/// </summary>
		/// <param name="recordId">The record to delete.</param>
		/// <param name="command">The command object to control sqlite.</param>
		public static void Delete(string recordId, IDbCommand command)
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

		/// <summary>
		/// Find the record in the value table.
		/// </summary>
		/// <param name="recordId">The record to find.</param>
		/// <param name="command">The command object to control sqlite.</param>
		/// <returns>Data reader for the record.</returns>
		public static IDataReader Select(string recordId, IDbCommand command)
		{
			//string safeTable = table.Replace("'", "''");
			string safeTable = vTableName;

			command.CommandText = string.Format(
				"SELECT {1}, {2}, {3}, {4} FROM '{0}' WHERE {5} = '{6}'", 
				safeTable,
				Name,
				Type,
				Flags,
				Value,
				RecordId,
				recordId);
			return command.ExecuteReader();
		}

		/// <summary>
		/// Read the next property from the supplied datareader.
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="sName">The property name.</param>
		/// <param name="sType">The property type.</param>
		/// <param name="sFlags">The property flags</param>
		/// <param name="sValue">The value.</param>
		/// <returns>True if property read.</returns>
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

	#region InternalConnection class

	/// <summary>
	/// SQLITE needs to use a 
	/// Class used to keep a connection per thread.
	/// </summary>
	internal class InternalConnection
	{
		internal SqliteConnection		sqliteDb;
		internal IDbCommand				command;
		ProviderConfig					conf;
		Thread							thread;
		bool							AlreadyDisposed;

		/// <summary>
		/// Called to initialize the DataBase.
		/// </summary>
		internal InternalConnection(ProviderConfig conf, string DbPath)
		{
			this.conf = conf;
			sqliteDb = new SqliteConnection("URI=file:" + DbPath);
			thread = Thread.CurrentThread;
			AlreadyDisposed = false;
		}

		/// <summary>
		/// 
		/// </summary>
		~InternalConnection()
		{
			Dispose(true);
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inFinalize"></param>
		internal void Dispose(bool inFinalize)
		{
			if (!AlreadyDisposed)
			{
				if (!inFinalize)
				{
					GC.SuppressFinalize(this);
				}

				if (command != null)
					command.Dispose();
				sqliteDb.Close();
				AlreadyDisposed = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal bool IsAlive
		{
			get
			{
				return thread.IsAlive;
			}
		}

		/// <summary>
		/// Called to initialize the DataBase.
		/// </summary>
		private void Init()
		{
			command = sqliteDb.CreateCommand();
			command.CommandTimeout = 1000 * 120;

			// Turn of synchronous access.
			command.CommandText = "PRAGMA cache_size = " + conf.Get("CacheSize", "10000");
			command.ExecuteNonQuery();

			command.CommandText = "PRAGMA synchronous = " + conf.Get("Synchronous", "OFF");
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Called to Open an existing Collection store at the specified location.
		/// </summary>
		/// <param name="DbPath"></param>
		internal void OpenStore(string DbPath)
		{
			try
			{
				if (File.Exists(DbPath))
				{
					sqliteDb.Open();
					Init();
				}
				else
				{
					throw new System.IO.FileNotFoundException("DataBase Not found.");
				}
			}
			catch (Exception ex)
			{
				throw new OpenException(DbPath, ex);
			}
		}

		/// <summary>
		/// Called to Create a new Collection Store at the specified location.
		/// </summary>
		/// <param name="DbPath"></param>
		internal void CreateStore(string DbPath)
		{
			try
			{
				lock (sqliteDb)
				{
					// Make sure the Data Base does not exist.
					if (!File.Exists(DbPath))
					{
						// Create the store
						sqliteDb.ConnectionString = "URI=file:" + DbPath;
						sqliteDb.Open();
						
						IDbTransaction trans = sqliteDb.BeginTransaction();
						try
						{
							// Create the Record Table.
							Init();
							RecordTable.Create(command);
							SchemaTable.Create(command);
							ValueTable.Create(command);
						}
						catch (Exception ex)
						{
							trans.Rollback();
							File.Delete(DbPath);
							throw ex;
						}
						trans.Commit();
					}
					else
					{
						throw new ExistsException(DbPath);
					}
				}
				AlreadyDisposed = false;
			}
			catch (Exception ex)
			{
				throw new CreateException(DbPath, ex);
			}
		}
	}

	#endregion

	#region DbManager class

	internal class DbManager
	{
        static Hashtable	dbTable = new Hashtable(5);
		ProviderConfig		conf;
		internal Hashtable	connTable;
		internal bool		opened;
		int					lastCleanupCount = 0;
		int					count = 0;
		string				storePath;
		string				DbPath;
		const string		Name = "ColSqlite.db";

		DbManager(ProviderConfig conf)
		{
			this.conf = conf;
			connTable = new Hashtable();
			opened = false;
			storePath = Path.GetFullPath(conf.Path);
			DbPath = System.IO.Path.Combine(storePath, Name);
		}

		~DbManager()
		{
			Dispose(true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		internal InternalConnection GetConn()
		{
			InternalConnection instance;
			bool newConnection = false;
			object threadId = Thread.CurrentThread.GetHashCode();
			lock (connTable)
			{
				// See if we need to cleanup handles
				if (connTable.Count > lastCleanupCount + 20)
				{
					// If we are still greater cleanup dead connections.
					ArrayList badConns = new ArrayList();
					foreach (DictionaryEntry item in connTable)
					{
						InternalConnection conn = (InternalConnection)item.Value;
						if (!conn.IsAlive)
						{
							conn.Dispose();
							badConns.Add(item.Key);
						}
					}
					foreach (object key in badConns)
					{
						connTable.Remove(key);
					}
					lastCleanupCount = connTable.Count;
				}
				if (connTable.Contains(threadId))
				{
					instance = (InternalConnection)connTable[threadId];	
				}
				else
				{
					instance = new InternalConnection(conf, DbPath);
					connTable.Add(threadId, instance);
					newConnection = true;
				}
			}
			if (newConnection && opened)
			{
				DateTime startTime = DateTime.Now;
				while (true)
				{
					try
					{
						instance.OpenStore(DbPath);
						break;
					}
					catch (Exception ex)
					{
						if (((TimeSpan)(DateTime.Now - startTime)).Seconds > (2 * 60))
							throw ex;
						Thread.Sleep(10);
					}
				}
			}
			return instance;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="conf"></param>
		/// <returns></returns>
		internal static DbManager GetManager(ProviderConfig	 conf)
		{
			DbManager dbM = null;
			// Get the Hastable for this Store.
			lock (dbTable)
			{
				if (dbTable.Contains(conf.Path))
				{
					dbM = (DbManager)dbTable[conf.Path];
				}
				else
				{
					dbM = new DbManager(conf);
					dbTable.Add(conf.Path, dbM);
				}
				dbM.count++;
			}

			return dbM;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inFinalize"></param>
		internal void Dispose(bool inFinalize)
		{
			try
			{
				lock (connTable)
				{
					if (--count == 0 || inFinalize)
					{
						foreach (DictionaryEntry item in connTable)
						{
							InternalConnection conn = (InternalConnection)item.Value;
							conn.Dispose();
						}
						connTable.Clear();
						count = 0;
					}
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		/// <summary>
		/// 
		/// </summary>
		internal void OpenStore()
		{
			GetConn().OpenStore(DbPath);
			opened = true;
		}

		/// <summary>
		/// 
		/// </summary>
		internal void CreateStore()
		{
			GetConn().CreateStore(DbPath);
			opened = true;
		}

		/// <summary>
		/// 
		/// </summary>
		internal void Delete()
		{
			try
			{
				lock (connTable)
				{
					foreach (DictionaryEntry item in connTable)
					{
						InternalConnection conn = (InternalConnection)item.Value;
						conn.Dispose();
					}
					connTable.Clear();
					count = 0;
					File.Delete(DbPath);
				}
			}
			catch (Exception ex)
			{
				throw new DeleteException(DbPath, ex);
			}
		}
	}

	#endregion


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

		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(SqliteProvider));
		ProviderConfig			conf;
		DbManager				manager;
		bool					AlreadyDisposed;
		const string 			version = "0.2";
		
		#endregion

		
		#region Constructor/Finalizer
		/// <summary>
		/// 
		/// </summary>
		/// <param name="conf">The Configuration object used for this instance.</param>
		public SqliteProvider(ProviderConfig conf)
		{
			this.conf = conf;
			manager = DbManager.GetManager(conf);
			AlreadyDisposed = false;
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
		/// <param name="collectionId">The collection that this node belongs to.</param>
		/// <param name="recordXml">Xml that represents the record.</param>
		/// <param name="command">Command object used to control database.</param>
		private void CreateRecord(string collectionId, XmlElement recordXml, IDbCommand command)
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
			ValueTable.Insert(record, command);
		}

		/// <summary>
		/// Called to remove a collection and all its contents from the DataBase.  A transaction must
		/// be held when calling.
		/// </summary>
		/// <param name="collectionId">The collection Id to remove.</param>
		/// <param name="command">Command object used to control database.</param>
		private void RemoveCollection(string collectionId, IDbCommand command)
		{
			// Now remove from the collection node.
			ValueTable.Delete(collectionId, command);

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
			XmlElement root = doc.DocumentElement;
			XmlNodeList recordList = root.SelectNodes(XmlTags.ObjectTag);

			foreach (XmlElement recordEl in recordList)
			{
				CreateRecord(collectionId, recordEl, command);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="collectionId"></param>
		/// <param name="command"></param>
		public void DeleteRecords(XmlDocument doc, string collectionId, IDbCommand command)
		{
			XmlElement root = doc.DocumentElement;
			XmlNodeList objectList = root.SelectNodes(XmlTags.ObjectTag);
			foreach (XmlElement recordEl in objectList)
			{
				// Get ID.
				string id = recordEl.GetAttribute(XmlTags.IdAttr);
				if (id != null)
				{
					if (id == collectionId)
					{
						RemoveCollection(id, command);
						break;
					}
					else
					{
						InternalDeleteRecord(id, collectionId, command);
					}
				}
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
			ValueTable.Delete(recordId, command);
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

				manager.Dispose();
				AlreadyDisposed = true;
			}
		}

		#endregion

		#region IProvider Members

		#region Store Calls

		/// <summary>
		/// Called to Create a new Collection Store at the specified location.
		/// </summary>
		public void CreateStore()
		{
			manager.CreateStore();
			// Set the version.
			conf.Version = version;
		}


		/// <summary>
		/// Called to Delete the opened CollectionStore.
		/// </summary>
		public void DeleteStore()
		{
			Dispose();
			manager.Delete();
		}


		/// <summary>
		/// Called to Open an existing Collection store at the specified location.
		/// </summary>
		public void OpenStore()
		{
			// Make sure the version is correct.
			if (conf.Version != "0" && conf.Version != version)
			{
				throw new VersionException(conf.Path, conf.Version, version);
			}
			manager.OpenStore();
		}

		
		#endregion

		#region Container Calls.

		/// <summary>
		/// Called to create a container to hold records.  This call does not need to
		/// be made.  If a record is created and the container does not exist. it will be created.
		/// </summary>
		/// <param name="name">The name of the container.</param>
		public void CreateContainer(string name)
		{
			// Nothing to do.
		}

		/// <summary>
		/// Called to Delete a record container.  
		/// This call is deep (all records contained are deleted).
		/// </summary>
		/// <param name="name">The name of the container.</param>
		public void DeleteContainer(string name)
		{
			try
			{
				InternalConnection conn = manager.GetConn();
				IDbTransaction trans = conn.sqliteDb.BeginTransaction();
				try
				{
					RemoveCollection(name, conn.command);
				}
				catch (Exception ex)
				{
					trans.Rollback();
					throw ex;
				}
				trans.Commit();
			}
			catch (Exception ex)
			{
				throw new DeleteException(name, ex);
			}
		}

		#endregion

		#region Record Calls.

		/// <summary>
		/// Used to Create, Modify or Delete records from the store.
		/// </summary>
		/// <param name="collectionId">The collection that the records belong to.</param>
		/// <param name="createDoc">The records to create or modify.</param>
		/// <param name="deleteDoc">The records to delete.</param>
		public void CommitRecords(string collectionId, XmlDocument createDoc, XmlDocument deleteDoc)
		{
			try
			{
				InternalConnection conn = manager.GetConn();
				IDbTransaction trans = conn.sqliteDb.BeginTransaction();
				try
				{
					if (createDoc != null)
					{
						CreateRecords(createDoc, collectionId, conn.command);
					}
					if (deleteDoc != null)
					{
						DeleteRecords(deleteDoc, collectionId, conn.command);
					}
				}
				catch (Exception ex)
				{
					trans.Rollback();
					throw ex;
				}
				trans.Commit();
			}
			catch (Exception ex)
			{
				throw new CommitException(createDoc, deleteDoc, ex);
			}
		}

		/// <summary>
		/// Called to ge a Record.  The record is returned as an XML string representation.  
		/// </summary>
		/// <param name="recordId">string that contains the ID of the Record to retrieve</param>
		/// <param name="collectionId">The id of the collection that contains this Record.</param>
		/// <returns>XML string describing the Record</returns>
		public XmlDocument GetRecord(string recordId, string collectionId)
		{
			XmlDocument doc = null;
			
			string Name;
			string Type;
			InternalConnection conn = manager.GetConn();
			SqliteConnection sqliteDb = conn.sqliteDb;
			IDbCommand command = conn.command;
			if (RecordTable.Select(recordId, command, out Name, out Type))
			{
				doc = new XmlDocument();
				XmlElement node, root;
				root = doc.CreateElement(XmlTags.ObjectListTag);
				doc.AppendChild(root);
				node = new Record(Name, recordId, Type).ToXml(doc);			
				root.AppendChild(node);
				
				// Now get the properties.
				IDataReader reader = ValueTable.Select(recordId, command);
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
				reader.Close();
			}
			return (doc);
		}

		/// <summary>
		/// Called to get a shallow record.
		/// </summary>
		/// <param name="recordId">The record id to get.</param>
		/// <returns>XmlDocument describing the shallow Record.</returns>
		public XmlDocument GetShallowRecord(string recordId)
		{
			XmlDocument doc = null;
			
			string Name;
			string Type;
			InternalConnection conn = manager.GetConn();
			SqliteConnection sqliteDb = conn.sqliteDb;
			IDbCommand command = conn.command;
			if (RecordTable.Select(recordId, command, out Name, out Type))
			{
				doc = new XmlDocument();
				XmlElement node, root;
				root = doc.CreateElement(XmlTags.ObjectListTag);
				doc.AppendChild(root);
				node = new Record(Name, recordId, Type).ToXml(doc);			
				root.AppendChild(node);
			}
			return (doc);
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
			bool includeCollection = false;

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
				case SearchOp.Equal:
					op = string.Format("LIKE '{0}'", safeValue);
					break;
				case SearchOp.Not_Equal:
					op = string.Format("!= '{0}'", safeValue);
					break;
				case SearchOp.Begins:
					op = string.Format("LIKE '{0}%'", safeValue);
					break;
				case SearchOp.Ends:
					op = string.Format("LIKE '%{0}'", safeValue);
					break;
				case SearchOp.Contains:
					op = string.Format("LIKE '%{0}%'", safeValue);
					break;
				case SearchOp.Greater:
					op = string.Format("> '{0}'", safeValue);
					break;
				case SearchOp.Less:
					op = string.Format("< '{0}'", safeValue);
					break;
				case SearchOp.Greater_Equal:
					op = string.Format(">= '{0}'", safeValue);
					break;
				case SearchOp.Less_Equal:
					op = string.Format("<= '{0}'", safeValue);
					break;
				case SearchOp.Exists:
					op = string.Format("LIKE '%'");
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
							"SELECT DISTINCT {0},{1},{2},{3} FROM {4} WHERE {5} {6}", 
							RecordTable.Id,
							RecordTable.Name,
							RecordTable.Type,
							RecordTable.CollectionId,
							RecordTable.TableName,
							attribute, 
							op);

						includeCollection = true;
					}
				}
				else
				{
					if (query.CollectionId != null)
					{
						selectNodes = string.Format(
							"SELECT {0},{1},{2} FROM {3} WHERE {10} = '{11}' AND {0} IN (Select {4} from '{5}' WHERE {6} = '{7}' AND {8} {9})",
							RecordTable.Id,
							RecordTable.Name,
							RecordTable.Type,
							RecordTable.TableName,
							ValueTable.RecordId,
							ValueTable.vTableName, //TablePrefix + query.CollectionId.Replace("'", "''"),
							ValueTable.Name,
							safeProperty,
							ValueTable.Value,
							op,
							RecordTable.CollectionId,
							query.CollectionId);
					}
					else
					{
						selectNodes = string.Format(
							"SELECT {0},{1},{2},{3} FROM {4} WHERE {0} IN (Select {5} from '{6}' WHERE {7} = '{8}' AND {9} {10})",
							RecordTable.Id,
							RecordTable.Name,
							RecordTable.Type,
							RecordTable.CollectionId,
							RecordTable.TableName,
							ValueTable.RecordId,
							ValueTable.vTableName,
							ValueTable.Name,
							safeProperty,
							ValueTable.Value,
							op);

						includeCollection = true;
					}
				}
				InternalConnection conn = manager.GetConn();
				IDbCommand command = conn.command;
				command.CommandText = selectNodes;
				IDataReader reader = command.ExecuteReader();
				resultSet = new SqliteResultSet(reader, includeCollection);
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
			get {return new Uri(conf.Path);}
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
