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
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents a user from another domain in the Collection Store.
	/// </summary>
	public class Alias
	{
		#region Class Members
		/// <summary>
		/// Domain where this alias exists.
		/// </summary>
		private string domain;

		/// <summary>
		/// Unique identifier for this alias.
		/// </summary>
		private string id;

		/// <summary>
		/// Credential used to authenticate to the domain server.
		/// </summary>
		private RSACryptoServiceProvider credential;

		/// <summary>
		/// String used to reconstitute credentials;
		/// </summary>
		private string credentialString;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the domain where this alias exists.
		/// </summary>
		public string Domain
		{
			get { return domain; }
		}

		/// <summary>
		/// Gets the unique identifier for this alias.
		/// </summary>
		public string ID
		{
			get { return id; }
		}

		/// <summary>
		/// Gets the domain's public key.
		/// </summary>
		public RSACryptoServiceProvider PublicKey
		{
			get 
			{ 
				if ( credential == null )
				{
					credential = new RSACryptoServiceProvider( IdentityManager.dummyCsp );
					credential.FromXmlString( credentialString );
				}

				return credential; 
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating an existing alias object from a property object.
		/// </summary>
		/// <param name="aliasProperty">Property that represents the alias object.</param>
		internal Alias( Property aliasProperty )
		{
			// Get the alias parameters out of the property.
			XmlDocument alias = new XmlDocument();
			alias.LoadXml( aliasProperty.Value as string );
			this.domain = alias.DocumentElement.GetAttribute( PropertyTags.DomainName );
			this.id = alias.DocumentElement.GetAttribute( XmlTags.IdAttr );

			// Don't reconstitute the credentials yet. Wait until they are asked for.
			this.credential = null;
			this.credentialString = alias.InnerText;
		}

		/// <summary>
		/// Constructor for creating a new alias object.
		/// </summary>
		/// <param name="domain">Domain where this alias exists.</param>
		/// <param name="id">Unique identifier for this alias.</param>
		/// <param name="credential">The public key for the specified domain.</param>
		internal Alias( string domain, string id, RSACryptoServiceProvider credential )
		{
			this.domain = domain;
			this.id = id;
			this.credential = credential;
			this.credentialString = credential.ToXmlString( false );
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Gets a xml string representation of the alias object.  This is the format used to store the object in the
		/// collection store.
		/// </summary>
		/// <returns>A string that represents the serialized alias object.</returns>
		internal string ToXmlString()
		{
			// Create an xml document that will hold the serialized object.
			XmlDocument alias = new XmlDocument();
			XmlElement aliasRoot = alias.CreateElement( "AliasParameters" );
			alias.AppendChild( aliasRoot );

			// Set the attributes on the object.
			aliasRoot.SetAttribute( PropertyTags.DomainName, domain );
			aliasRoot.SetAttribute( XmlTags.IdAttr, id );
			aliasRoot.InnerText = credentialString;
			return alias.OuterXml;
		}
		#endregion
	}
}
