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
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using System.Security.Policy;
using System.Threading;
using Simias.Storage.Provider;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Simias.Storage.Provider.Fs
{
	#region FsDb class
	internal class FsDb
	{
		/// <summary>
		/// Queue used to keep track of Fs record IDs.
		/// When a node gets deleted the id is put on the queue
		/// so that a new object can reuse the ID.
		/// </summary>
		static Hashtable	dbTable = new Hashtable();
		int					refCount = 0;
		public IntPtr		pFWDB = IntPtr.Zero;
		string				dbName;
		bool				deleteDb = false;

		FsDb(string name)
		{
			dbName = name;
		}

		public static FsDb GetFsDb(string name)
		{
			FsDb FsDb;
			lock(dbTable)
			{
				FsDb = (FsDb)dbTable[name];
				if (FsDb == null)
				{
					FsDb = new FsDb(name);
					dbTable.Add(name, FsDb);
				}
				FsDb.refCount++;
			}
			return (FsDb);
		}

		public void Release()
		{
			lock(dbTable)
			{
				if (--refCount == 0)
				{
					dbTable.Remove(dbName);
					if (deleteDb)
					{
						FsProvider.DeleteStore(dbName);
					}
				}
			}
		}

		public bool DeleteDb
		{
			set {deleteDb = value;}
		}
	}
	#endregion

	/// <summary>
	/// Class that implements the Simias.Storage.Provider.IProvider interface 
	/// using the native file system.
	/// </summary>
	public class FsProvider : MarshalByRefObject, IProvider
	{
		#region Variables
		ProviderConfig			conf;
		bool					AlreadyDisposed;
		FsDb					FsDb;
		string					DbPath;
		const string			DbName = "Collection.db";
		const string			version = "0.1";
		string					storePath;
		#endregion
		
		#region Constructor/Destructor
		/// <summary>
		/// 
		/// </summary>
		/// <param name="conf">The configuration object used to configure this instance.</param>
		public FsProvider(ProviderConfig conf)
		{
			this.conf = conf;
			storePath = System.IO.Path.GetFullPath(conf.Path);
			DbPath = Path.Combine(storePath, DbName);
			FsDb = FsDb.GetFsDb(DbPath);
		}

		/// <summary>
		/// 
		/// </summary>
		~FsProvider()
		{
			Dispose(true);
		}
		#endregion

		#region Transaction class
		class Transaction
		{
			Mutex	mutex;
			public string	path;
			string	addPath;
			string	delPath;
			bool	deleteContainer = false;

			internal Transaction(string dbPath, string container)
			{
				path = Path.Combine(dbPath, container);
				addPath = Path.Combine(path, "add");
				delPath = Path.Combine(path, "del");
				mutex = new Mutex(false, "mutex_" + Path.GetFileName(path));
			}

			internal void Begin()
			{
				mutex.WaitOne();
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				if (!Directory.Exists(addPath))
				{
					Directory.CreateDirectory(addPath);
				}
				if (!Directory.Exists(delPath))
				{
					Directory.CreateDirectory(delPath);
				}
			}

			internal void BeginRead()
			{
				mutex.WaitOne();
			}

			internal void EndRead()
			{
				mutex.ReleaseMutex();
			}

			internal void Commit()
			{
				try
				{
					if (deleteContainer)
					{
						Directory.Delete(path, true);
					}
					else
					{
						// Commit the deletes.
						if (Directory.Exists(delPath))
						{
							Directory.Delete(delPath, true);
						}

						// Commit the Adds.
						string [] files = Directory.GetFiles(addPath);

						foreach (string f in files)
						{
							string filePath = Path.Combine(path, Path.GetFileName(f));
							if (File.Exists(filePath))
							{
								File.Delete(filePath);
							}
							File.Move(f, filePath);
						}
						Directory.Delete(addPath, true);
					}
				}
				catch
				{
				}
				mutex.ReleaseMutex();
			}

			internal void Abort()
			{
				try
				{
					deleteContainer = false;
					// Abort the Adds.
					Directory.Delete(addPath, true);

					// Abort the Deletes.
					string [] files = Directory.GetFiles(delPath);

					foreach (string f in files)
					{
						string filePath = Path.Combine(path, Path.GetFileName(f));
						if (File.Exists(filePath))
						{
							File.Delete(filePath);
						}
						File.Move(f, filePath);
					}
					Directory.Delete(delPath, true);
				}
				catch
				{
				}
				mutex.ReleaseMutex();
			}

			internal void addObject(string file, XmlElement xmlObject)
			{
				removeObject(file);
				string filePath = Path.Combine(addPath, file);
				XmlTextWriter xmlWriter = new XmlTextWriter(filePath, System.Text.Encoding.UTF8);
				//xmlWriter.Formatting = Formatting.Indented;
				xmlObject.WriteTo(xmlWriter);
				xmlWriter.Close();
			}

			internal void removeObject(string file)
			{
				string filePath = Path.Combine(path, file);
				if (File.Exists(filePath))
				{
					File.Move(filePath, Path.Combine(delPath, file));
				}
			}

			internal void DeleteContainer()
			{
				deleteContainer = true;
			}
		}
		#endregion

		#region Private Methods.
		internal static void DeleteStore(string path)
		{
			Directory.Delete(path, true);
			Provider.Delete(Path.GetDirectoryName(path));
		}

		/// <summary>
		/// Method Used to create a store Object.
		/// </summary>
		/// <param name="doc">XML document that describes the Objects</param>
		/// <param name="trans">The transaction object for this create.</param>
		private void CreateRecords(XmlDocument doc, Transaction trans)
		{
			XmlElement root = doc.DocumentElement;
			XmlNodeList objectList = root.SelectNodes(XmlTags.ObjectTag);

			// Build the path to the collection and make sure it exists.
			if (!Directory.Exists(trans.path))
			{
				Directory.CreateDirectory(trans.path);
			}

			foreach (XmlElement recordEl in objectList)
			{
				// Get the Name, ID, and type.
				string name = recordEl.GetAttribute(XmlTags.NameAttr);
				string id = recordEl.GetAttribute(XmlTags.IdAttr);
				string type = recordEl.GetAttribute(XmlTags.TypeAttr);
		
				// Make sure this is a valid record.
				if (name != null && id != null && type != null)
				{
					trans.addObject(id, recordEl);
				}
			}
		}

		/// <summary>
		/// Called to delete 1 or more Records.
		/// </summary>
		/// <param name="doc">Xml string describing Records to delete.</param>
		/// <param name="trans">The transaction object for this delete.</param>
		private void DeleteRecords(XmlDocument doc, Transaction trans)
		{
			XmlElement root = doc.DocumentElement;
			XmlNodeList objectList = root.SelectNodes(XmlTags.ObjectTag);
			
			foreach (XmlElement recordEl in objectList)
			{
				// Get ID.
				string id = recordEl.GetAttribute(XmlTags.IdAttr);
				trans.removeObject(id);
			}
		}


		private void Dispose(bool inFinalize)
		{
			if (!AlreadyDisposed)
			{
				// Close all of the handles that have been opened.
				FsDb.Release();

				if (!inFinalize)
				{
					//GC.SuppressFinalize(this);
				}
				AlreadyDisposed = true;
			}
		}

		#endregion

		#region IProvider Methods

		#region Store Calls

		/// <summary>
		/// Called to Create a new Collection Store at the specified location.
		/// </summary>
		public void CreateStore()
		{
			lock (FsDb)
			{
				// Create the store
				if (!Directory.Exists(DbPath))
				{
					Directory.CreateDirectory(DbPath);
					// Set the version.
					conf.Version = version;
 				}
			}

			AlreadyDisposed = false;
		}


		/// <summary>
		/// Called to Delete the opened CollectionStore.
		/// </summary>
		public void DeleteStore()
		{
			lock (FsDb)
			{
				FsDb.DeleteDb = true;
			}
		}


		/// <summary>
		/// Called to Open an existing Collection store at the specified location.
		/// </summary>
		public void OpenStore()
		{
			lock (FsDb)
			{
				// Open the store
				if (!Directory.Exists(DbPath))
				{
					throw new OpenException(DbPath);
				}
				// Make sure the version is correct.
				if (conf.Version != version)
				{
					throw new VersionException(DbPath, conf.Version, version);
				}
 			}
			
			AlreadyDisposed = false;
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
			Transaction trans = new Transaction(DbPath, name);
			trans.Begin();
            trans.Commit();
		}

		/// <summary>
		/// Called to Delete a record container.  
		/// This call is deep (all records contained are deleted).
		/// </summary>
		/// <param name="name">The name of the container.</param>
		public void DeleteContainer(string name)
		{
			Transaction trans = new Transaction(DbPath, name);
			trans.Begin();
			trans.DeleteContainer();
			trans.Commit();
		}
	
	#endregion

		#region Record Calls.

		/// <summary>
		/// Used to Create, Modify or Delete records from the store.
		/// </summary>
		/// <param name="container">The container to commit changes to.</param>
		/// <param name="createDoc">The records to create or modify.</param>
		/// <param name="deleteDoc">The records to delete.</param>
		public void CommitRecords(string container, XmlDocument createDoc, XmlDocument deleteDoc)
		{
			Transaction trans = new Transaction(DbPath, container);
			trans.Begin();
			try
			{
				if (createDoc != null)
				{
					CreateRecords(createDoc, trans);
				}
				if (deleteDoc != null)
				{
					DeleteRecords(deleteDoc, trans);
				}
			}
			catch 
			{
				trans.Abort();
				return;
			}

			trans.Commit();
		}
		
		/// <summary>
		/// Called to ge a Record.  The record is returned as an XML string representation.  
		/// </summary>
		/// <param name="recordId">string that contains the ID of the Record to retrieve</param>
		/// <param name="container">The conaiter to get the record from.</param>
		/// <returns>XML string describing the Record</returns>
		public XmlDocument GetRecord(string recordId, string container)
		{
			string recordPath = Path.Combine(DbPath, container);
			recordPath = Path.Combine(recordPath, recordId);
			XmlDocument doc = new XmlDocument();
			XmlElement rootElement = doc.CreateElement(XmlTags.ObjectListTag);
			doc.AppendChild(rootElement);

			XmlTextReader xmlReader = new XmlTextReader(recordPath);
			try
			{
				xmlReader.Read();
				rootElement.InnerXml = xmlReader.ReadOuterXml();
			}
			finally
			{
				xmlReader.Close();
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
			IResultSet	resultSet = null;
			string		pattern = null;
			bool		valueCompare = false;
			bool		isAttribute = false;
			string		attribute = null;
			
			try
			{
				switch (query.Property)
				{
					case BaseSchema.ObjectName:
						attribute = XmlTags.NameAttr;
						isAttribute = true;
						break;
					case BaseSchema.ObjectId:
						attribute = XmlTags.IdAttr;
						isAttribute = true;
						break;
					case BaseSchema.ObjectType:
						attribute = XmlTags.TypeAttr;
						isAttribute = true;
						break;
				}
			
				switch (query.Operation)
				{
					case SearchOp.Equal:
					case SearchOp.Not_Equal:
						if (isAttribute)
						{
							pattern = string.Format("<{0}[^>]*{1}=\"{2}\"[^>]*", XmlTags.ObjectTag, attribute, query.Value);
						}
						else
						{
							pattern = string.Format("<{0} name=\"{1}\" type=\"{2}\".*>{3}</{0}>", XmlTags.PropertyTag, query.Property, query.Type, query.Value);
						}
						break;
					case SearchOp.Begins:
						if (isAttribute)
						{
							pattern = string.Format("<{0}[^>]*{1}=\"{2}", XmlTags.ObjectTag, attribute, query.Value);
						}
						else
						{
							pattern = string.Format("<{0} name=\"{1}\" type=\"{2}\".*>{3}", XmlTags.PropertyTag, query.Property, query.Type, query.Value);
						}
						break;
					case SearchOp.Ends:
						if (isAttribute)
						{
							pattern = string.Format("<{0}[^>]*{1}=\"[^\"]*{2}\"", XmlTags.ObjectTag, attribute, query.Value);
						}
						else
						{
							pattern = string.Format("<{0} name=\"{1}\" type=\"{2}\".*>.*{3}</{0}>", XmlTags.PropertyTag, query.Property, query.Type, query.Value);
						}
						break;
					case SearchOp.Contains:
						if (isAttribute)
						{
							pattern = string.Format("<{0}[^>]*{1}=\"[^\"]*{2}", XmlTags.ObjectTag, attribute, query.Value);
						}
						else
						{
							pattern = string.Format("<{0} name=\"{1}\" type=\"{2}\".*>.*{3}.*</{0}>", XmlTags.PropertyTag, query.Property, query.Type, query.Value);
						}
						break;
					case SearchOp.Greater:
					case SearchOp.Less:
					case SearchOp.Greater_Equal:
					case SearchOp.Less_Equal:
						pattern = string.Format("<{0} name=\"{1}\" type=\"{2}\"[^>]*>", XmlTags.PropertyTag, query.Property, query.Type);
						valueCompare = true;
						break;
					case SearchOp.Exists:
						if (isAttribute)
						{
							pattern = string.Format("<{0}[^>]*{1}=\"[^>]*", XmlTags.ObjectTag, attribute, query.Value);
						}
						else
						{
							pattern = string.Format("<{0} name=\"{1}\" type=\"{2}\".*>", XmlTags.PropertyTag, query.Property, query.Type);
						}
						break;
				}

				if (pattern != null)
				{
					Regex expression = new Regex(pattern, RegexOptions.IgnoreCase);
					string path;
					string[] files;

					if (query.CollectionId != null)
					{
						path = Path.Combine(DbPath, query.CollectionId);
						files = Directory.GetFiles(path);
					}
					else
					{
						path = DbPath;
						files = Directory.GetDirectories(path);
					}
				
					Queue resultQ = new Queue();

					foreach(string sfile in files)
					{
						StreamReader sReader;
						// If we have a collection ID then search in the collection only
						// Else only search collections.
						try
						{
							if (query.CollectionId != null)
							{
								sReader = File.OpenText(sfile);
							}
							else
							{
								sReader = File.OpenText(Path.Combine(sfile, Path.GetFileName(sfile)));
							}
						}
						catch
						{
							continue;
						}
						string xmlString = sReader.ReadToEnd();
						sReader.Close();
						Match match = expression.Match(xmlString);
						if (match != Match.Empty)
						{
							if (valueCompare)
							{
								int diff;
								bool found = false;
								while (match != Match.Empty)
								{
									int startIndex = match.Index + match.Length;
									int length = xmlString.IndexOf('<', match.Index + match.Length) - startIndex;
									string s1 = xmlString.Substring(startIndex, length);
										
									switch (query.Type)
									{
										case Syntax.Byte:
										case Syntax.DateTime:
										case Syntax.TimeSpan:
										case Syntax.UInt16:
										case Syntax.UInt32:
										case Syntax.UInt64:
										case Syntax.Char:
										{
											ulong v1 = ulong.Parse(s1);
											ulong v2 = ulong.Parse(query.Value);
											diff = v1.CompareTo(v2);
											break;
										}
										case Syntax.SByte:
										case Syntax.Int16:
										case Syntax.Int32:
										case Syntax.Int64:
										{
											long v1 = long.Parse(s1);
											long v2 = long.Parse(query.Value);
											diff = v1.CompareTo(v2);
											break;
										}
										case Syntax.Single:
										{
											Double v1 = Double.Parse(s1);
											Double v2 = Double.Parse(query.Value);
											diff = v1.CompareTo(v2);
											break;
										}
										default:
											throw new SearchException(query.ToString());
									}
								
									switch (query.Operation)
									{
										case SearchOp.Greater:
											if (diff > 0) found = true;
											break;
										case SearchOp.Less:
											if (diff < 0) found = true;
											break;
										case SearchOp.Greater_Equal:
											if (diff > 0 || diff == 0) found = true;
											break;
										case SearchOp.Less_Equal:
											if (diff < 0 || diff == 0) found = true;
											break;
									}

									if (found)
									{
										string sObject = xmlString.Substring(0, xmlString.IndexOf('>')) + "/>";
										resultQ.Enqueue(sObject);
										break;
									}
									else
									{
										match = match.NextMatch();
									}
								}
							}
							else
							{
								// Save this file as a match.
								string sObject = xmlString.Substring(0, xmlString.IndexOf('>')) + "/>";
								resultQ.Enqueue(sObject);
							}
						}
						else if (query.Operation == SearchOp.Not_Equal)
						{
							// Save this as a miss.
							string sObject = xmlString.Substring(0, xmlString.IndexOf('>')) + "/>";
							resultQ.Enqueue(sObject);
						}
					}
					resultSet = new FsResultSet(resultQ);
				}
			}
			catch
			{
				resultSet = new FsResultSet(new Queue());
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
}
