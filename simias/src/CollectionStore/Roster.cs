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
	/// Class that represents the list of users contained in the domain 
	/// that a collection can be shared with.
	/// </summary>
	public class Roster : Collection
	{
		#region Properties
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for this object that creates the Roster object.
		/// </summary>
		/// <param name="storeObject">Store object.</param>
		/// <param name="domain">Domain object that this object will be associated with.</param>
		public Roster( Store storeObject, Domain domain ) :
			this ( storeObject, Guid.NewGuid().ToString(), domain )
		{
		}

		/// <summary>
		/// Constructor for this object that creates the Roster object.
		/// </summary>
		/// <param name="storeObject">Store object.</param>
		/// <param name="rosterID">Identifier for the roster.</param>
		/// <param name="domain">Domain object that this object will be associated with.</param>
		public Roster( Store storeObject, string rosterID, Domain domain ) :
			base ( storeObject, domain.Name + " Roster", rosterID, NodeTypes.RosterType, domain.ID )
		{
		}

		/// <summary>
		/// Constructor to create an existing Roster object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="node">Node object to construct this object from.</param>
		public Roster( Store storeObject, Node node ) :
			base( storeObject, node )
		{
		}

		/// <summary>
		/// Constructor for creating an existing Roster object from a ShallowNode.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="shallowNode">A ShallowNode object.</param>
		public Roster( Store storeObject, ShallowNode shallowNode ) :
			base( storeObject, shallowNode )
		{
		}

		/// <summary>
		/// Constructor to create an existing Roster object from an Xml document object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document object to construct this object from.</param>
		internal Roster( Store storeObject, XmlDocument document ) :
			base( storeObject, document )
		{
		}
		#endregion
	}
}
