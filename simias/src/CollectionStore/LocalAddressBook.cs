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
using System.IO;
using System.Xml;

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents the local address book that contains identity objects which 
	/// represent users of the collection store. 
	/// </summary>
	public class LocalAddressBook : Collection
	{
		#region Constructor
		/// <summary>
		/// Constructor for this object that creates the local address book.
		/// </summary>
		/// <param name="storeObject">Store object.</param>
		/// <param name="bookName">Name of the address book.</param>
		/// <param name="bookID">The globally unique identifier for this object.</param>
		/// <param name="ownerGuid">Owner identifier of this object.</param>
		/// <param name="domainName">Name of the domain that this address book belongs to.</param>
		internal LocalAddressBook( Store storeObject, string bookName, string bookID, string ownerGuid, string domainName ) :
			base ( storeObject, bookName, bookID, ownerGuid, domainName, storeObject.GetStoreManagedPath( bookID ) )
		{
			// Add the properties that make this an address book.
			properties.AddNodeProperty( Property.DefaultAddressBook, true );
			properties.AddNodeProperty( Property.LocalAddressBook, true );
			Synchronizable = false;
		}

		/// <summary>
		/// Constructor for creating an existing LocalAddressBook object.
		/// </summary>
		/// <param name="storeObject">Store object.</param>
		/// <param name="labDocument">Xml document that describes a LocalAddressBook object.</param>
		internal LocalAddressBook( Store storeObject, XmlDocument labDocument ) :
			base( storeObject, labDocument )
		{
		}

		/// <summary>
		/// Constructor to create an existing LocalAddressBook object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="node">Node object to construct this object from.</param>
		internal LocalAddressBook( Store storeObject, Node node ) :
			base( storeObject, node )
		{
			if ( !IsType( node, "LocalAddressBook" ) )
			{
				throw new ApplicationException( "Cannot construct object from specified type." );
			}
		}
		#endregion
	}
}
