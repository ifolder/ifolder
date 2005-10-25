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

		#region ContainerCalls Calls.
		
		/// <summary>
		/// Called to create a container to hold records.  This call does not need to
		/// be made.  If a record is created and the container does not exist. it will be created.
		/// </summary>
		/// <param name="name">The name of the container.</param>
		void CreateContainer(string name);

		/// <summary>
		/// Called to Delete a record container.  
		/// This call is deep (all records contained are deleted).
		/// </summary>
		/// <param name="name">The name of the container.</param>
		void DeleteContainer(string name);

		#endregion

		#region Record Calls.

		/// <summary>
		/// Used to Create, Modify or Delete records from the store.
		/// </summary>
		/// <param name="container">The container that the commit applies to.</param>
		/// <param name="createDoc">The records to create or modify.</param>
		/// <param name="deleteDoc">The records to delete.</param>
		void CommitRecords(string container, XmlDocument createDoc, XmlDocument deleteDoc);
		
		/// <summary>
		/// Called to get a Record.  The record is returned as an XML string representation.  
		/// </summary>
		/// <param name="recordId">string that contains the ID of the Record to retrieve</param>
		/// <param name="container">The container that holds the record.</param>
		/// <returns>XMLDocument describing the Record</returns>
		XmlDocument GetRecord(string recordId, string container);

		/// <summary>
		/// Called to get a shallow record.
		/// </summary>
		/// <param name="recordId">The record id to get.</param>
		/// <returns>XmlDocument describing the shallow Record.</returns>
		XmlDocument GetShallowRecord(string recordId);

		#endregion
		
		#region Query Calls

		/// <summary>
		/// Method used to search for Records using the specified query.
		/// </summary>
		/// <param name="query">Query used for this search</param>
		/// <returns></returns>
		IResultSet Search(Query query);

		#endregion


	}
}
