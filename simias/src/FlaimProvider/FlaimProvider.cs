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
using System.Runtime.InteropServices;
using System.IO;
using Simias;
using Simias.Service;
using Simias.Storage.Provider;

namespace Simias.Storage.Provider.Flaim
{
	/// <summary>
	/// Class that implements the Novell.iFolder.Storage.Provider.IProvider interface 
	/// using the FAIM record manager.
	/// </summary>
	public class FlaimProvider : IProvider
	{
		#region Varibles
		FlaimServer				Flaim;
		ProviderConfig			conf;
		
		#endregion
		
		#region Constructors Destructor

		/// <summary>
		/// Constructs a flaim provider.
		/// </summary>
		/// <param name="conf">The config object for this provider.</param>
		public FlaimProvider(ProviderConfig conf)
		{
			this.conf = conf;
			Flaim = FlaimServer.GetServer();
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~FlaimProvider()
		{
			Dispose(true);
		}


		#endregion

		#region IProvider Members
		#region Store Calls

		/// <summary>
		/// Called to Create a new Collection Store at the specified location.
		/// </summary>
		public void CreateStore()
		{
			FlaimError.Error rc = Flaim.CreateStore();
			if (FlaimError.IsError(rc))
			{
				throw (new CreateException("Flaim DB", FlaimError.GetException(rc)));
			}
		}

		/// <summary>
		/// Called to Delete the opened CollectionStore.
		/// </summary>
		public void DeleteStore()
		{
			FlaimError.Error rc = Flaim.DeleteStore();
			if (FlaimError.IsError(rc))
			{
				// We had an error the store was not deleted.
				throw(new DeleteException("Flaim DB", FlaimError.GetException(rc)));
			}
		}

		/// <summary>
		/// Called to Open an existing Collection store at the specified location.
		/// </summary>
		public void OpenStore()
		{
			FlaimError.Error rc = Flaim.OpenStore();
			if (FlaimError.IsError(rc))
			{
				if (rc == FlaimError.Error.FERR_IO_PATH_NOT_FOUND)
				{
					// The files do not exist.
					throw new ApplicationException();
				}
				else
				{
					// We had an error the store was not opened.
					throw(new OpenException("Flaim DB", FlaimError.GetException(rc)));
				}
			}
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
			FlaimError.Error rc = Flaim.DeleteContainer(name);
			if (FlaimError.IsError(rc))
			{
				// We had an error the container was not deleted.
				throw(new DeleteException(name, FlaimError.GetException(rc)));
			}
		}

		#endregion

		#region Record Calls.
		/// <summary>
		/// Used to Create, Modify or Delete records from the store.
		/// </summary>
		/// <param name="container">The container that the commit applies to.</param>
		/// <param name="createDoc">The records to create or modify.</param>
		/// <param name="deleteDoc">The records to delete.</param>
		public void CommitRecords(string container, XmlDocument createDoc, XmlDocument deleteDoc)
		{
			FlaimError.Error rc = Flaim.CommitRecords(container, createDoc, deleteDoc);
			if (FlaimError.IsError(rc))
			{
				// We had an error the commit was not successful.
				throw(new CommitException(createDoc, deleteDoc, FlaimError.GetException(rc)));
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
			string record = Flaim.GetRecord(recordId);
			if (record != null)
			{
				doc = new XmlDocument();
				doc.LoadXml(record);
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
			string record = Flaim.GetRecord(recordId);
			if (record != null)
			{
				doc = new XmlDocument();
				doc.LoadXml(record);
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
			return Flaim.Search(query);
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
		/// Called to dispose the object.
		/// </summary>
		/// <param name="inFinalize">Specifize if called from finalizer.</param>
		private void Dispose(bool inFinalize)
		{
			if (inFinalize)
			{
				GC.SuppressFinalize(this);
			}
		}

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
