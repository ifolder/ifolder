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
using System.Xml;

using Simias.Client;
using Simias.Sync;

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents a Domain object in the Collection Store.
	/// </summary>
	public class Domain : Collection
	{
		#region Class Members
		/// <summary>
		/// Defines the different types of synchronization configurations.
		/// </summary>
		public enum ConfigurationType
		{
			/// <summary>
			/// Doesn't sychronize.
			/// </summary>
			None,

			/// <summary>
			/// Workgroup (e.g. Rendevous, Gaim, etc)
			/// </summary>
			Workgroup,

			/// <summary>
			/// Client/Server (e.g. Enterprise, SimpleServer, etc)
			/// </summary>
			ClientServer
		}

		/// <summary>
		/// Configuration section name where enterprise key value pairs are stored.
		/// </summary>
		static public string SectionName = "Domain";
		static public string AdminDNTag = "AdminDN";
		static public string Encoding = "Encoding";
		#endregion

		#region Properties
		/// <summary>
		/// Gets the domain configuration type.
		/// </summary>
		public ConfigurationType ConfigType
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.DomainType );
				return ( p != null ) ? ( ConfigurationType )p.Value : ConfigurationType.None;
			}
		}

		/// <summary>
		/// Gets or sets the domain description.
		/// </summary>
		public string Description
		{
			get 
			{ 
				Property p = properties.GetSingleProperty( PropertyTags.Description );
				return ( p != null ) ? p.Value as String : null;
			}

			set { properties.ModifyNodeProperty( PropertyTags.Description, value ); }
		}
		#endregion

		#region Constructors
		/// <param name="store">Store object.</param>
		/// <param name="domainName">Name of the domain.</param>
		/// <param name="domainID">Well known unique identifier for the domain.</param>
		/// <param name="description">String that describes this domain.</param>
		/// <param name="role">The type of synchronization role this domain has.</param>
		/// <param name="configType">The synchronization configuration type for this domain.</param>
		public Domain( Store store, string domainName, string domainID, string description, SyncRoles role, ConfigurationType configType ) :
			base ( store, domainName, domainID, NodeTypes.DomainType, domainID )
		{
			// Add the description attribute.
			if ( ( description != null ) && ( description.Length > 0 ) )
			{
				properties.AddNodeProperty( PropertyTags.Description, description );
			}

			// Add the sync role for this collection.
			Property p = new Property( PropertyTags.SyncRole, role );
			p.LocalProperty = true;
			properties.AddNodeProperty( p );

			// Add the configuration type.
			p = new Property( PropertyTags.DomainType, configType );
			p.LocalProperty = true;
			properties.AddNodeProperty( p );
		}

		/// <summary>
		/// Constructor to create an existing Domain object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="node">Node object to construct this object from.</param>
		public Domain( Store storeObject, Node node ) :
			base( storeObject, node )
		{
		}

		/// <summary>
		/// Constructor for creating an existing Domain object from a ShallowNode.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="shallowNode">A ShallowNode object.</param>
		public Domain( Store storeObject, ShallowNode shallowNode ) :
			base( storeObject, shallowNode )
		{
		}

		/// <summary>
		/// Constructor to create an existing Domain object from an Xml document object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document object to construct this object from.</param>
		internal Domain( Store storeObject, XmlDocument document ) :
			base( storeObject, document )
		{
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns if the specified user's login is disabled.
		/// </summary>
		/// <param name="userID">User ID for the member to check.</param>
		/// <returns>True if the login for the specified user is disabled.</returns>
		public bool IsLoginDisabled( string userID )
		{
			Member member = GetMemberByID( userID );
			if ( member == null )
			{
				throw new DoesNotExistException( "The specified user does not exist." );
			}

			Property p = member.Properties.GetSingleProperty( PropertyTags.LoginDisabled );
			return ( p != null ) ? ( bool )p.Value : false;
		}

		/// <summary>
		/// Sets the specified user's login disabled status.
		/// </summary>
		/// <param name="userID">User ID for the member to set the status for.</param>
		/// <param name="disable">True to disable login or False to enable login.</param>
		public void SetLoginDisabled( string userID, bool disable )
		{
			Member member = GetMemberByID( userID );
			if ( member == null )
			{
				throw new DoesNotExistException( "The specified user does not exist." );
			}

			if ( disable )
			{
				member.Properties.ModifyNodeProperty( PropertyTags.LoginDisabled, true );
				Commit( member );
			}
			else
			{
				Property p = member.Properties.GetSingleProperty( PropertyTags.LoginDisabled );
				if ( p != null )
				{
					p.DeleteProperty();
					Commit( member );
				}
			}
		}

		/// <summary>
		/// Obtains the string representation of this instance.
		/// </summary>
		/// <returns>The friendly name of the domain.</returns>
		public override string ToString()
		{
			return Name;
		}
		#endregion
	}
}
