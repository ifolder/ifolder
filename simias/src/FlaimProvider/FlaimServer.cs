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
using System.Xml;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Simias;
using Simias.Storage.Provider;

namespace Simias.Storage.Provider.Flaim
{
	internal class Flaim4
	{
		IntPtr					pStore;
		static IntPtr			pDB;
		string					DbPath;
		static bool				opened = false;
		static Hashtable		handleTable = new Hashtable();
		
		#region Flaim Wrapper Imports
		/// <summary>
		/// Fuction to Create the underlying flaim data base. 
		/// </summary>
		/// <param name="path">The full path of the data base including name.</param>
		/// <param name="ipStore">Handle to the Created Store.</param>
		/// <param name="ipDB">Pointer to the DataBase object created.</param>
		/// <returns></returns>
		[DllImport("FlaimWrapper")]
		private static extern FlaimError.Error FWCreateStore(string path, out IntPtr ipStore, out IntPtr ipDB);
		internal FlaimError.Error CreateStore()
		{
			FlaimError.Error rc = FlaimError.Error.FERR_FAILURE;
			lock (handleTable)
			{
				rc = FWCreateStore(DbPath, out pStore, out pDB);
				if (FlaimError.IsSuccess(rc))
				{
					handleTable.Add(pStore, pStore);
					opened = true;
				}
			}
			return rc;
		}

		/// <summary>
		/// Fuction to open an existing flaim data base.
		/// </summary>
		/// <param name="path">The full path of the data base including name.</param>
		/// <param name="ipStore">Handle to the opened Store.</param>
		/// <param name="ipDB">Pointer to the DataBase object opened.</param>
		/// <returns></returns>
		[DllImport("FlaimWrapper")]
		private static extern FlaimError.Error FWOpenStore(string path, out IntPtr ipStore, out IntPtr ipDB);
		internal FlaimError.Error OpenStore()
		{
			FlaimError.Error rc = FlaimError.Error.FERR_FAILURE;
			lock (handleTable)
			{
				rc = FWOpenStore(DbPath, out pStore, out pDB);
				if (FlaimError.IsSuccess(rc))
				{
					handleTable.Add(pStore, pStore);
					opened = true;
				}
			}
			return rc;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dbName"></param>
		/// <returns></returns>
		[DllImport("FlaimWrapper")]
		private static extern FlaimError.Error FWDeleteStore(string dbName);
		internal FlaimError.Error DeleteStore()
		{
			FlaimError.Error rc = FlaimError.Error.FERR_FAILURE;
			// Close the current handle.
			CloseStore();

			// Now force the other instances closed.
			lock (handleTable)
			{
				foreach (IntPtr p in handleTable.Keys)
				{
					FWCloseStore(p);
				}
				handleTable.Clear();
				rc = FWDeleteStore(DbPath);
				if (FlaimError.IsSuccess(rc))
				{
					opened = false;
				}
			}
			pDB = IntPtr.Zero;
			return rc;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipStore"></param>
		[DllImport("FlaimWrapper")]
		private static extern void FWCloseStore(IntPtr ipStore);
		internal void CloseStore()
		{
			lock (handleTable)
			{
				if (pStore != IntPtr.Zero)
				{
					FWCloseStore(pStore);
					handleTable.Remove(pStore);
					pStore = IntPtr.Zero;
					if (handleTable.Count == 0)
						pDB = IntPtr.Zero;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipStore"></param>
		/// <param name="name"></param>
		/// <param name="id"></param>
		/// <param name="type"></param>
		/// <param name="newObject"></param>
		/// <param name="flmId"></param>
		/// <param name="ipObject"></param>
		/// <returns></returns>
		[DllImport("FlaimWrapper", CharSet=CharSet.Unicode)]
		private static extern FlaimError.Error FWCreateObject(IntPtr ipStore, string name, string id, string type, out int newObject, int flmId, out IntPtr ipObject);
		internal FlaimError.Error CreateObject(FlaimRecord record, int flmId, out bool usedId)
		{
			FlaimError.Error rc;
			int isNew;
			usedId = false;
			
			rc = FWCreateObject(pStore, record.Name, record.Id, record.Type, out isNew, flmId, out record.pRecord);

			if (FlaimError.IsSuccess(rc)) 
			{
				if (isNew == 1 && flmId != 0)
				{
					usedId = true;
				}
				// Now Create all of the properties.
				foreach(Property property in record)
				{
					rc = FWSetProperty(record.pRecord, property.Name, property.Type, property.Value, property.Flags);
					if (FlaimError.IsError(rc))
					{
						break;
					}
				}

				FWCloseObject(record.pRecord, FlaimError.IsSuccess(rc) ? false : true);
			}

			return rc;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipObject"></param>
		/// <param name="abort"></param>
		/// <returns></returns>
		[DllImport("FlaimWrapper", CharSet=CharSet.Unicode)]
		private static extern FlaimError.Error FWCloseObject(IntPtr ipObject, bool abort);
		internal FlaimError.Error CloseObject(FlaimRecord record, bool abort)
		{
			FlaimError.Error rc = FWCloseObject(record.pRecord, abort);
			record.pRecord = IntPtr.Zero;
			return rc;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipStore">Method to delete the specified object.</param>
		/// <param name="Id">Id of object to delete</param>
		/// <param name="flaimId">The flaim ID that was used for this record.</param>
		/// <returns></returns>
		[DllImport("FlaimWrapper", CharSet=CharSet.Unicode)]
		private static extern FlaimError.Error FWDeleteObject(IntPtr ipStore, string Id, out int flaimId);
		internal FlaimError.Error DeleteObject(string Id, out int flaimId)
		{
			FlaimError.Error rc;
			rc = FWDeleteObject(pStore, Id, out flaimId);
			return rc;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipStore"></param>
		/// <param name="Id"></param>
		/// <param name="length"></param>
		/// <param name="buffer"></param>
		/// <returns></returns>
		[DllImport("FlaimWrapper", CharSet=CharSet.Unicode)]
		private static extern FlaimError.Error FWGetObject(IntPtr ipStore, string Id, ref int length, [In, Out] char[] buffer);
		internal string GetRecord(string Id)
		{
			FlaimError.Error rc = FlaimError.Error.FERR_OK;
			char [] Buffer;
			int  length = 4096;
			do
			{
				Buffer = new char[length];
				rc = FWGetObject(pStore, Id, ref length, Buffer);
			} while (rc == FlaimError.Error.FERR_MEM);

			if (length > 0)
			{
				return (new string(Buffer, 0, length));
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipStore"></param>
		/// <returns></returns>
		[DllImport("FlaimWrapper")]
		private static extern FlaimError.Error FWBeginTrans(IntPtr ipStore);
		internal FlaimError.Error BeginTrans()
		{
			return FWBeginTrans(pStore);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipStore"></param>
		[DllImport("FlaimWrapper")]
		private static extern void FWAbortTrans(IntPtr ipStore);
		internal void AbortTrans()
		{
			FWAbortTrans(pStore);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipStore"></param>
		/// <returns></returns>
		[DllImport("FlaimWrapper")]
		private static extern FlaimError.Error FWEndTrans(IntPtr ipStore);
		internal FlaimError.Error EndTrans()
		{
			return FWEndTrans(pStore);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pStore"></param>
		/// <param name="properties"></param>
		/// <returns></returns>
		[DllImport("FlaimWrapper", CharSet=CharSet.Unicode)]
		private static extern FlaimError.Error FWSetProperties(IntPtr pStore, string properties);
		internal FlaimError.Error SetProperties(string properties)
		{
			return FWSetProperties(pStore, properties);
		}

		/// <summary>
		/// Set a property on the specified object.
		/// </summary>
		/// <param name="pObject">Pointer to the object that the property will be added to.</param>
		/// <param name="name">Property Name.</param>
		/// <param name="type">The type of the property.</param>
		/// <param name="value">The value of the property</param>
		/// <param name="flags">Flags that describe the property.</param>
		/// <returns></returns>
		[DllImport("FlaimWrapper", CharSet=CharSet.Unicode)]
		private static extern FlaimError.Error FWSetProperty(IntPtr pObject, string name, string type, string value, string flags);
		internal FlaimError.Error SetProperty(FlaimRecord record, string name, string type, string value, string flags)
		{
			return FWSetProperty(record.pRecord, name, type, value, flags);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipStore"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		[DllImport("FlaimWrapper", CharSet=CharSet.Unicode)]
		private static extern FlaimError.Error FWDefineField(IntPtr ipStore, string name, string type, int index);
		internal FlaimError.Error DefineField(string name, string type, int index)
		{
			return FWDefineField(pStore, name, type, index);
		}


		[DllImport("FlaimWrapper", CharSet=CharSet.Unicode)]
		private static extern FlaimError.Error FWSearch(IntPtr pStore, string collectionId, string name, int op, string value, string type, int caseSensitive, out int count, out IntPtr pResultSet);
		internal FlaimError.Error Search(Query query, out FlaimResultSet results)
		{
			FlaimError.Error rc = FlaimError.Error.FERR_OK;

			results = null;
			int		count;
			IntPtr	pFlaimResults;
			int		op = 0;
			string sValue = query.Value;
			int		caseSensitive = 0;
			
			switch (query.Operation)
			{
				case SearchOp.Equal:
					op = 103;		// FLM_EQ_OP
					break;
				case SearchOp.Not_Equal:
					op = 108;		// FLM_NE_OP
					break;
				case SearchOp.Begins:
					op = 105;		// FLM_MATCH_BEGIN_OP
					break;
				case SearchOp.Ends:
					op = 106;		// FLM_MATCH_END_OP
					break;
				case SearchOp.Contains:
					op = 107;		// FLM_CONTAINS_OP
					break;
				case SearchOp.Greater:
					op = 111;		// FLM_GT_OP
					break;
				case SearchOp.Less:
					op = 109;		// FLM_LT_OP
					break;
				case SearchOp.Greater_Equal:
					op = 112;		// FLM_GE_OP
					break;
				case SearchOp.Less_Equal:
					op = 110;		// FLM_LE_OP
					break;
				case SearchOp.Exists:
					switch (query.Type)
					{
						case Syntax.Boolean:
							op = 112;
							sValue = "0";
							break;
						case Syntax.Byte:
						case Syntax.Char:
						case Syntax.DateTime:
						case Syntax.Int16:
						case Syntax.Int32:
						case Syntax.Int64:
						case Syntax.SByte:
						case Syntax.TimeSpan:
						case Syntax.UInt16:
						case Syntax.UInt32:
						case Syntax.UInt64:
							op = 111;		// FLM_GT_OP
							sValue = Int64.MinValue.ToString();
							break;
							
						case Syntax.Relationship:
						case Syntax.String:
						case Syntax.Uri:
						case Syntax.XmlDocument:
							op = 105;
							sValue = "*";
							break;

						case Syntax.Single:
							op = 111;		// FLM_GT_OP
							sValue = Single.MinValue.ToString();
							break;
					}
					break;
				case SearchOp.CaseEqual:
					caseSensitive = 1;
					op = 103;		// FLM_EQ_OP
					break;
			}

			if (op != 0)
			{
				rc = FWSearch(pStore, query.CollectionId, query.Property, op, sValue, query.Type.ToString(), caseSensitive, out count, out pFlaimResults);
				if (FlaimError.IsSuccess(rc))
				{
					results = new FlaimResultSet(pFlaimResults, count);
				}
			}
			
			return (rc);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[DllImport("FlaimWrapper")]
		private static extern int FWNOP();

		#endregion
		
		internal Flaim4(string DbPath)
		{
			this.DbPath = DbPath;
			if (opened)
			{
				OpenStore();
			}
		}

		~Flaim4()
		{
			CloseStore();
		}
	}

	/// <summary>
	/// Summary description for FlaimServer.
	/// </summary>
	public class FlaimServer
	{
		static FlaimServer		instance = null;
		ProviderConfig			conf;
		static internal Queue	IdQueue;
		
		[ThreadStatic]
		static Flaim4			flaim;

		bool					AlreadyDisposed;
		string					DbPath;
		string					IdPath;
		const string			Name = "FlaimSimias.db";
		const string			IdName = "FlaimSimias.IDs";
		const string			version = "0.2";

		internal Flaim4 Flaim
		{
			get
			{
				lock (typeof(FlaimServer))
				{
					if (flaim == null)
					{
						flaim = new Flaim4(DbPath);
					}
				}
				return flaim;
			}
		}

		internal static FlaimServer GetServer()
		{
			lock (typeof(FlaimServer))
			{
				if (instance == null)
				{
					instance = new FlaimServer();
				}
				return instance;
			}
		}
		
		internal FlaimServer()
		{
			conf = new ProviderConfig();
			DbPath = Path.Combine(Path.GetFullPath(conf.Path), Name);

			// Read the Available Id queue from disk.
			try
			{
				IdPath = Path.Combine(conf.Path, IdName);
				Stream rS = File.OpenRead(IdPath);
				BinaryFormatter bf = new BinaryFormatter();
				IdQueue = (Queue)bf.Deserialize(rS);
				rS.Close();
				File.Delete(IdPath);
				AlreadyDisposed = false;
			}
			catch
			{
				IdQueue = new Queue();
			}
		}

		internal void Dispose(bool inFinalizer)
		{
			if (!AlreadyDisposed)
			{
				// Save the Ids that can be reused.
				AlreadyDisposed = true;
				if (!inFinalizer)
				{
					GC.SuppressFinalize(this);
				}
				Stream wS = File.OpenWrite(IdPath);
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(wS, IdQueue);
				wS.Close();
			}
		}
		
		#region Store Calls

		/// <summary>
		/// Called to Create a new Collection Store at the specified location.
		/// </summary>
		internal FlaimError.Error CreateStore()
		{
			FlaimError.Error rc = FlaimError.Error.FERR_FAILURE;
			lock (typeof(FlaimServer))
			{
				if (!Directory.Exists(DbPath))
				{
					// Create the store
					rc = Flaim.CreateStore();
					if (FlaimError.IsSuccess(rc))
					{
						// Set the version.
						conf.Version = version;
						AlreadyDisposed = false;
					}
				}
			}
			return rc;
		}

		/// <summary>
		/// Called to Delete the opened CollectionStore.
		/// </summary>
		internal FlaimError.Error DeleteStore()
		{
			lock (typeof(FlaimServer))
			{
				FlaimError.Error rc = Flaim.DeleteStore();
				flaim = null;
				return rc;
			}
		}

		/// <summary>
		/// Called to Open an existing Collection store at the specified location.
		/// </summary>
		internal FlaimError.Error OpenStore()
		{
			FlaimError.Error rc = FlaimError.Error.FERR_FAILURE;
			
			lock (typeof(FlaimServer))
			{
				rc = Flaim.OpenStore();
				if (FlaimError.IsSuccess(rc))
				{
					// Make sure the version is correct.
					string lv = conf.Version;
					if (lv != "0" && lv == version)
					{
						AlreadyDisposed = false;
					}
					else
					{
						rc = FlaimError.Error.FERR_UNSUPPORTED_VERSION;
						Flaim.CloseStore();
					}
				}
			}
			return rc;
		}
		
		#endregion

		#region ContainerCalls Calls.
		
		/// <summary>
		/// Called to create a container to hold records.  This call does not need to
		/// be made.  If a record is created and the container does not exist. it will be created.
		/// </summary>
		/// <param name="name">The name of the container.</param>
		void CreateContainer(string name)
		{
			// Nothing to do in flaim.
		}

		/// <summary>
		/// Called to Delete a record container.  
		/// This call is deep (all records contained are deleted).
		/// </summary>
		/// <param name="name">The name of the container.</param>
		internal FlaimError.Error DeleteContainer(string name)
		{
			FlaimError.Error rc = FlaimError.Error.FERR_OK;
			try
			{
				Query query = new Query(name, BaseSchema.CollectionId, SearchOp.Exists, name, Syntax.String);
				IResultSet results = Search(query);
				char []buffer = new char[4096];
				int count;
				
				while ((count = results.GetNext(ref buffer)) != 0)
				{
					XmlDocument delDoc = new XmlDocument();
					delDoc.LoadXml(new string(buffer, 0, count));
					rc = CommitRecords(name, null, delDoc);
				}
			}
			catch
			{
				if (FlaimError.IsSuccess(rc))
				{
					rc = FlaimError.Error.FERR_FAILURE;
				}
			}
			return rc;
		}

		#endregion

		#region Record Calls.

		/// <summary>
		/// Used to Create, Modify or Delete records from the store.
		/// </summary>
		/// <param name="container">The container that the commit applies to.</param>
		/// <param name="createDoc">The records to create or modify.</param>
		/// <param name="deleteDoc">The records to delete.</param>
		internal FlaimError.Error CommitRecords(string container, XmlDocument createDoc, XmlDocument deleteDoc)
		{
			FlaimError.Error rc = FlaimError.Error.FERR_OK;
			Flaim4 flaim = this.Flaim;

			try
			{
				Flaim.BeginTrans();
				try
				{
					if (createDoc != null)
					{
						XmlNodeList recordList = createDoc.DocumentElement.SelectNodes(XmlTags.ObjectTag);
						foreach (XmlElement recordEl in recordList)
						{
							bool reuseId;
							int flmId = 0;
							if (IdQueue.Count != 0)
							{
								flmId = (int)IdQueue.Peek();
								reuseId = true;
							}
				
							FlaimRecord record = new FlaimRecord(recordEl);
							rc = flaim.CreateObject(record, flmId, out reuseId);

							if (FlaimError.IsSuccess(rc))
							{
								if (reuseId)
								{
									flmId = (int)IdQueue.Dequeue();
								}
							}
							else
							{
								throw FlaimError.GetException(rc);
							}
						}
					}
					if (deleteDoc != null)
					{
						XmlNodeList recordList = deleteDoc.DocumentElement.SelectNodes(XmlTags.ObjectTag);
						foreach (XmlElement recordEl in recordList)
						{
							int flmId;
							FlaimRecord record = new FlaimRecord(recordEl);
							rc = flaim.DeleteObject(record.Id, out flmId);
							if (FlaimError.IsSuccess(rc))
							{
								IdQueue.Enqueue(flmId);
							}
							else if (rc != FlaimError.Error.FERR_NOT_FOUND)
							{
								throw FlaimError.GetException(rc);
							}
							else
							{
								rc = FlaimError.Error.FERR_OK;
							}
						}
					}
					Flaim.EndTrans();
				}
				catch
				{
					Flaim.AbortTrans();
				}
			}
			catch
			{
				if (FlaimError.IsSuccess(rc))
					rc = FlaimError.Error.FERR_FAILURE;
			}
			return rc;
		}
		
		/// <summary>
		/// Called to ge a Record.  The record is returned as an XML string representation.  
		/// </summary>
		/// <param name="recordId">string that contains the ID of the Record to retrieve</param>
		/// <returns>XML string describing the Record</returns>
		internal string GetRecord(string recordId)
		{
			return Flaim.GetRecord(recordId);
		}

		#endregion
		
		#region Query Calls

		/// <summary>
		/// Method used to search for Records using the specified query.
		/// </summary>
		/// <param name="query">Query used for this search</param>
		/// <returns></returns>
		internal IResultSet Search(Query query)
		{
			FlaimResultSet resultSet;
			FlaimError.Error rc = Flaim.Search(query, out resultSet);
			if (FlaimError.IsError(rc))
			{
				throw new SearchException(query.ToString());
			}
			return resultSet;
		}

		#endregion
	}
}
