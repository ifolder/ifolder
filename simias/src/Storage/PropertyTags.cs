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
using System.Collections;

namespace Simias.Storage
{
	/// <summary>
	/// Represents a property name/value pair for a node.  Properties have
	/// well-defined syntax types.
	/// </summary>
	public class PropertyTags
	{
		#region Class Members
		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Ace = "Ace";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Collision = "Collision";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string CreationTime = "CreationTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Credential = "Credential";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string DefaultDomain = "DefaultDomain";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string DirCreationTime = "DirCreationTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string DirLastAccessTime = "DirLastAccessTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string DirLastWriteTime = "DirLastWriteTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Domain = "Domain";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string DomainName = "DomainName";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string FileCreationTime = "FileCreationTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string FileLastAccessTime = "FileLastAccessTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string FileLastWriteTime = "FileLastWriteTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string FileLength = "FileLength";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LinkReference = "LinkReference";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LocalIncarnation = "LocalIncarnation";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string MasterIncarnation = "MasterIncarnation";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string ModifyTime = "ModifyTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Owner = "Owner";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Parent = "Parent";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string PublicKey = "PublicKey";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Root = "Root";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Syncable = "Syncable";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string TombstoneType = "TombstoneType";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Types = "Types";

		/// <summary>
		/// Hashtable providing quick lookup to well-known system properties.
		/// </summary>
		private static Hashtable systemPropertyTable;
		#endregion

		#region Constructors
		/// <summary>
		/// Static constructor for the object.
		/// </summary>
		static PropertyTags()
		{
			// Allocate the tables to hold the reserved property names.
			systemPropertyTable = new Hashtable( 30, new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer() );

			// Add the well-known system properties to the hashtable.  Don't need to add values
			// with them.  Just need to know if they exist.
			systemPropertyTable.Add( Ace, null );
			systemPropertyTable.Add( BaseSchema.ObjectId, null );
			systemPropertyTable.Add( BaseSchema.ObjectName, null );
			systemPropertyTable.Add( BaseSchema.ObjectType, null );
			systemPropertyTable.Add( BaseSchema.CollectionId, null );
			systemPropertyTable.Add( Collision, null );
			systemPropertyTable.Add( CreationTime, null );
			systemPropertyTable.Add( Credential, null );
			systemPropertyTable.Add( DirCreationTime, null );
			systemPropertyTable.Add( DirLastAccessTime, null );
			systemPropertyTable.Add( DirLastWriteTime, null );
			systemPropertyTable.Add( DefaultDomain, null );
			systemPropertyTable.Add( Domain, null );
			systemPropertyTable.Add( DomainName, null );
			systemPropertyTable.Add( FileCreationTime, null );
			systemPropertyTable.Add( FileLastAccessTime, null );
			systemPropertyTable.Add( FileLastWriteTime, null );
			systemPropertyTable.Add( FileLength, null );
			systemPropertyTable.Add( LinkReference, null );
			systemPropertyTable.Add( LocalIncarnation, null );
			systemPropertyTable.Add( MasterIncarnation, null );
			systemPropertyTable.Add( ModifyTime, null );
			systemPropertyTable.Add( Owner, null );
			systemPropertyTable.Add( Parent, null );
			systemPropertyTable.Add( PublicKey, null );
			systemPropertyTable.Add( Root, null );
			systemPropertyTable.Add( Syncable, null );
			systemPropertyTable.Add( TombstoneType, null );
			systemPropertyTable.Add( Types, null );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Determines if the propertyName is a system (non-editable) property.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <returns>True if propertyName specifies a system property, otherwise false is returned.</returns>
		static public bool IsSystemProperty( string propertyName )
		{
			return systemPropertyTable.Contains( propertyName );
		}
		#endregion
	}
}
