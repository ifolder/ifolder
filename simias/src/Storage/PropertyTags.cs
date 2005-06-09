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
		static public string CollisionPolicy = "CollPol";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string CollectionLock = "Lock";

		/// <summary>
		/// Does the master collection need to be created?
		/// </summary>
		static public string CreateMaster = "Create Master Collection";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string CreationTime = "Create";

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
		static public string DomainType = "DomainType";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public readonly string Family = "Family";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public readonly string FullName = "FN";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string FileLength = "Length";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LoginDisabled = "LoginDisabled";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string FileSystemPath = "FsPath";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public readonly string Given = "Given";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string HostAddress = "HostUri";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LastLoginTime = "LastLogin";

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
		static public string LocalPassword = "LocalPwd";

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
		static public string Originator = "Orginator";

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
		static public string PreviousOwner = "PrevOwner";

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
		static public string StorageSize = "StorageSize";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string StoreVersion = "Version";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string SyncRole = "Sync Role";

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
		/// Well known property name.
		/// </summary>
		static public string SyncStatusTag = "SyncStatus";

		/// <summary>
		/// Well known perperty name.
		/// The file represented by this node has disapeared.
		/// </summary>
		static public string GhostFile = "GhostFile";
		
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
			systemPropertyTable.Add( CollisionPolicy, null );
			systemPropertyTable.Add( CollectionLock, null );
			systemPropertyTable.Add( CreateMaster, null );
			systemPropertyTable.Add( CreationTime, null );
			systemPropertyTable.Add( DefaultDomain, null );
			systemPropertyTable.Add( Domain, null );
			systemPropertyTable.Add( DomainID, null );
			systemPropertyTable.Add( DomainType, null );
			systemPropertyTable.Add( FileLength, null );
			systemPropertyTable.Add( FileSystemPath, null );
			systemPropertyTable.Add( FullName, null );
			systemPropertyTable.Add( HostAddress, null );
			systemPropertyTable.Add( LastLoginTime, null );
			systemPropertyTable.Add( LastAccessTime, null );
			systemPropertyTable.Add( LastWriteTime, null );
			systemPropertyTable.Add( LinkReference, null );
			systemPropertyTable.Add( LocalIncarnation, null );
			systemPropertyTable.Add( LocalPassword, null );
			systemPropertyTable.Add( LoginDisabled, null );
			systemPropertyTable.Add( MasterIncarnation, null );
			systemPropertyTable.Add( MasterUrl, null );
			systemPropertyTable.Add( NodeCreationTime, null );
			systemPropertyTable.Add( NodeUpdateTime, null );
			systemPropertyTable.Add( Originator, null );
			systemPropertyTable.Add( Owner, null );
			systemPropertyTable.Add( Parent, null );
			systemPropertyTable.Add( PreviousOwner, null );
			systemPropertyTable.Add( PolicyID, null );
			systemPropertyTable.Add( PolicyAssociation, null );
			systemPropertyTable.Add( Priority, null );
			systemPropertyTable.Add( PublicKey, null );
			systemPropertyTable.Add( Root, null );
			systemPropertyTable.Add( StorageSize, null );
			systemPropertyTable.Add( StoreVersion, null );
			systemPropertyTable.Add( SyncRole, null );
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
