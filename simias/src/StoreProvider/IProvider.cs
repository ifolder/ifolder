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
using System.Xml;

namespace Simias.Storage.Provider
{
	/// <summary>
	/// Collection Store Provider interface.
	/// </summary>
	public interface IProvider : IDisposable
	{
		#region Store Calls

		/// <summary>
		/// Called to Create a new Collection Store at the specified location.
		/// </summary>
		void CreateStore();

		/// <summary>
		/// Called to Delete the opened CollectionStore.
		/// </summary>
		void DeleteStore();

		/// <summary>
		/// Called to Open an existing Collection store at the specified location.
		/// </summary>
		void OpenStore();
		
		#endregion

		#region Collection Calls.
		/// <summary>
		/// Called to create or modify a collection.
		/// </summary>
		/// <param name="collectionId">The Id of the collection to create.</param>
		void CreateCollection(string collectionId);

		/// <summary>
		/// Called to delete an existing collection.
		/// </summary>
		/// <param name="collectionId">The ID of the collection to delete.</param>
		void DeleteCollection(string collectionId);
		#endregion

		#region Record Calls.

		/// <summary>
		/// Called to create or modify 1 or more Records
		/// </summary>
		/// <param name="recordXml">Xml string that describes the new/modified Records.</param>
		/// <param name="collectionId">The id of the collection containing these objects.</param>
		void CreateRecord(string recordXml, string collectionId);		

		/// <summary>
		/// Called to delete a Record.  The record is specified by it ID.
		/// </summary>
		/// <param name="recordId">string that contains the ID of the Object to delete</param>
		/// <param name="collectionId">The id of the collection that contains this object</param>
		void DeleteRecord(string recordId, string collectionId);

		/// <summary>
		/// Called to delete 1 or more Records.
		/// </summary>
		/// <param name="recordXml">Xml string describing Records to delete.</param>
		/// <param name="collectionId">The id of the collection that contains these objects.</param>
		void DeleteRecords(string recordXml, string collectionId);

		/// <summary>
		/// Called to ge a Record.  The record is returned as an XML string representation.  
		/// </summary>
		/// <param name="recordId">string that contains the ID of the Record to retrieve</param>
		/// <param name="collectionId">The id of the collection that contains this Record.</param>
		/// <returns>XML string describing the Record</returns>
		string GetRecord(string recordId, string collectionId);

		#endregion
		
		#region Query Calls

		/// <summary>
		/// Method used to search for Records using the specified query.
		/// </summary>
		/// <param name="query">Query used for this search</param>
		/// <returns></returns>
		IResultSet Search(Query query);

		#endregion

		#region Properties

		/// <summary>
		/// Property to get the directory to where the store is rooted.
		/// </summary>
		Uri StoreDirectory
		{
			get;
		}

		#endregion
	}
}
