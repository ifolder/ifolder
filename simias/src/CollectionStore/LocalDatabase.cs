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

using Simias.Client;

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents the local address book that contains identity objects which
	/// represent users of the collection store.
	/// </summary>
	public class LocalDatabase : Collection
	{
		#region Properties
		/// <summary>
		/// Gets or sets the default domain ID.
		/// </summary>
		public string DefaultDomain
		{
			get { return properties.GetSingleProperty( PropertyTags.DefaultDomain ).ToString(); }
			set { properties.ModifyNodeProperty( PropertyTags.DefaultDomain, value ); }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for this object that creates the local database object.
		/// </summary>
		/// <param name="storeObject">Store object.</param>
		/// <param name="domainID">Domain to which this collection belongs.</param>
		internal LocalDatabase( Store storeObject, string domainID ) :
			base ( storeObject, "LocalDatabase", Guid.NewGuid().ToString(), NodeTypes.LocalDatabaseType, domainID )
		{
			properties.AddNodeProperty( PropertyTags.DefaultDomain, Storage.Domain.WorkGroupDomainID );
		}

		/// <summary>
		/// Constructor to create an existing LocalDatabase object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="node">Node object to construct this object from.</param>
		internal LocalDatabase( Store storeObject, Node node ) :
			base( storeObject, node )
		{
			if ( type != NodeTypes.LocalDatabaseType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.LocalDatabaseType, type ) );
			}
		}

		/// <summary>
		/// Constructor for creating an existing LocalDatabase object from a ShallowNode.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="shallowNode">A ShallowNode object.</param>
		internal LocalDatabase( Store storeObject, ShallowNode shallowNode ) :
			base( storeObject, shallowNode )
		{
			if ( type != NodeTypes.LocalDatabaseType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.LocalDatabaseType, type ) );
			}
		}

		/// <summary>
		/// Constructor to create an existing LocalDatabase object from an Xml document object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document object to construct this object from.</param>
		internal LocalDatabase( Store storeObject, XmlDocument document ) :
			base( storeObject, document )
		{
			if ( type != NodeTypes.LocalDatabaseType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.LocalDatabaseType, type ) );
			}
		}
		#endregion
	}
}
