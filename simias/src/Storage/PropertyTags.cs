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
		static public string CollectionLock = "Lock";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string CreationTime = "Create";

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
		static public string Description = "Description";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Domain = "Domain";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string DomainID = "DomainID";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string DomainRole = "Role";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string FileLength = "Length";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string FileSystemPath = "FsPath";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string HostAddress = "HostUri";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LastAccessTime = "Access";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LastWriteTime = "Write";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LinkReference = "LinkRef";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LocalIncarnation = "ClntRev";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string MasterIncarnation = "SrvRev";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string MasterUrl = "Master Url";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string NodeCreationTime = "NodeCreate";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string NodeUpdateTime = "NodeUpdate";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Owner = "Owner";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Parent = "ParentNode";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string PolicyID = "PolicyID";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string PolicyAssociation = "Association";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Priority = "Priority";

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
		static public string Sealed = "Sealed";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string StorageSize = "StorageSize";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Syncable = "Syncable";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string SystemPolicy = "SystemPolicy";

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
			systemPropertyTable.Add( CollectionLock, null );
			systemPropertyTable.Add( CreationTime, null );
			systemPropertyTable.Add( Credential, null );
			systemPropertyTable.Add( DefaultDomain, null );
			systemPropertyTable.Add( Domain, null );
			systemPropertyTable.Add( DomainID, null );
			systemPropertyTable.Add( DomainRole, null );
			systemPropertyTable.Add( FileLength, null );
			systemPropertyTable.Add( FileSystemPath, null );
			systemPropertyTable.Add( HostAddress, null );
			systemPropertyTable.Add( LastAccessTime, null );
			systemPropertyTable.Add( LastWriteTime, null );
			systemPropertyTable.Add( LinkReference, null );
			systemPropertyTable.Add( LocalIncarnation, null );
			systemPropertyTable.Add( MasterIncarnation, null );
			systemPropertyTable.Add( MasterUrl, null );
			systemPropertyTable.Add( NodeCreationTime, null );
			systemPropertyTable.Add( NodeUpdateTime, null );
			systemPropertyTable.Add( Owner, null );
			systemPropertyTable.Add( Parent, null );
			systemPropertyTable.Add( PolicyID, null );
			systemPropertyTable.Add( PolicyAssociation, null );
			systemPropertyTable.Add( Priority, null );
			systemPropertyTable.Add( PublicKey, null );
			systemPropertyTable.Add( Root, null );
			systemPropertyTable.Add( StorageSize, null );
			systemPropertyTable.Add( Syncable, null );
			systemPropertyTable.Add( SystemPolicy, null );
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
