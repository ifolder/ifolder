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
		public static string Ace = "Ace";

		/// <summary>
		/// Well know property name.
		/// </summary>
		public static string AddressBookType = "AB:AddressBook";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string Alias = "Alias";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string ClientCredential = "ClientCredential";

		/// <summary>
		/// Well known property name;
		/// </summary>
		public static string ClientPublicKey = "ClientPublicKey";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string Collision = "Collision";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string ContactType = "AB:Contact";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string CreationTime = "CreationTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string DefaultAddressBook = "AB:Default";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string DefaultWorkGroup = "DefaultWorkGroup";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string DirCreationTime = "DirCreationTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string DirLastAccessTime = "DirLastAccessTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string DirLastWriteTime = "DirLastWriteTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string DomainName = "DomainName";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string FileCreationTime = "FileCreationTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string FileLastAccessTime = "FileLastAccessTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string FileLastWriteTime = "FileLastWriteTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string FileLength = "FileLength";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string LocalAddressBook = "AB:Local";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string LinkReference = "LinkReference";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string LocalDatabase = "LocalDatabase";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string LocalIncarnation = "LocalIncarnation";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string MasterIncarnation = "MasterIncarnation";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string ModifyTime = "ModifyTime";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string Owner = "Owner";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string Parent = "Parent";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string Root = "Root";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string ServerCredential = "ServerCredential";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string Shareable = "Shareable";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string Syncable = "Syncable";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string Types = "Types";

		/// <summary>
		/// Well known property name.
		/// </summary>
		public static string WorkGroup = "WorkGroup";

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
			systemPropertyTable.Add( AddressBookType, null );
			systemPropertyTable.Add( Alias, null );
			systemPropertyTable.Add( BaseSchema.ObjectId, null );
			systemPropertyTable.Add( BaseSchema.ObjectName, null );
			systemPropertyTable.Add( BaseSchema.ObjectType, null );
			systemPropertyTable.Add( BaseSchema.CollectionId, null );
			systemPropertyTable.Add( ClientCredential, null );
			systemPropertyTable.Add( ClientPublicKey, null );
			systemPropertyTable.Add( Collision, null );
			systemPropertyTable.Add( CreationTime, null );
			systemPropertyTable.Add( DirCreationTime, null );
			systemPropertyTable.Add( DirLastAccessTime, null );
			systemPropertyTable.Add( DirLastWriteTime, null );
			systemPropertyTable.Add( DefaultAddressBook, null );
			systemPropertyTable.Add( DomainName, null );
			systemPropertyTable.Add( FileCreationTime, null );
			systemPropertyTable.Add( FileLastAccessTime, null );
			systemPropertyTable.Add( FileLastWriteTime, null );
			systemPropertyTable.Add( FileLength, null );
			systemPropertyTable.Add( LinkReference, null );
			systemPropertyTable.Add( LocalAddressBook, null );
			systemPropertyTable.Add( LocalDatabase, null );
			systemPropertyTable.Add( LocalIncarnation, null );
			systemPropertyTable.Add( MasterIncarnation, null );
			systemPropertyTable.Add( ModifyTime, null );
			systemPropertyTable.Add( Owner, null );
			systemPropertyTable.Add( Parent, null );
			systemPropertyTable.Add( Root, null );
			systemPropertyTable.Add( ServerCredential, null );
			systemPropertyTable.Add( Shareable, null );
			systemPropertyTable.Add( Syncable, null );
			systemPropertyTable.Add( Types, null );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Determines if the propertyName is a system (non-editable) property.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <returns>True if propertyName specifies a system property, otherwise false is returned.</returns>
		public static bool IsSystemProperty( string propertyName )
		{
			return systemPropertyTable.Contains( propertyName );
		}
		#endregion
	}
}
