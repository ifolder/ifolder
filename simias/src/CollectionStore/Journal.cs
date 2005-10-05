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
 *  Author: Bruce Getter <bgetter@novell.com>
 *			Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

using System;
using System.Xml;

using Simias.Client;

namespace Simias.Storage
{
	/// <summary>
	/// Summary description for Journal.
	/// </summary>
	public class Journal : Node
	{
		#region Class Members
		/// <summary>
		/// Handle to the store.
		/// </summary>
		private Store store = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the store handle.
		/// </summary>
		private Store StoreReference
		{
			get
			{
				if ( store == null )
				{
					store = Store.GetStore();
				}

				return store;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new Journal object.
		/// </summary>
		/// <param name="store">A handle to the store.</param>
		/// <param name="journalName">The name of the journal.</param>
		/// <param name="journalGuid">Unique identifier for the journal.</param>
		internal Journal( Store store, string journalName, string journalGuid ) :
			base ( journalName, journalGuid, NodeTypes.JournalType )
		{
			this.store = store;	
		}

		/// <summary>
		/// Constructor that creates a Journal object from a Node object.
		/// </summary>
		/// <param name="node">Node object to create the Journal object from.</param>
		internal Journal( Node node ) :
			base( node )
		{
			if ( type != NodeTypes.JournalType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.IdentityType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates a Journal object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the specified Node object belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the Journal object from.</param>
		internal Journal( Collection collection, ShallowNode shallowNode ) :
			base( collection, shallowNode )
		{
			if ( type != NodeTypes.JournalType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.IdentityType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates a Journal object from an Xml document object.
		/// </summary>
		/// <param name="document">Xml document object to create the Journal object from.</param>
		internal Journal( XmlDocument document ) :
			base( document )
		{
			if ( type != NodeTypes.JournalType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.IdentityType, type ) );
			}
		}
		#endregion

		#region Private Methods
		#endregion

		#region Internal Methods
		#endregion
	}
}
