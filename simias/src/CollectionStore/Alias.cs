/***********************************************************************
 *  Alias.cs - Class that represents a user from another domain in the 
 *  Collection Store.
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Mike Lasky <mlasky@novell.com>
 *			Brady Anderson <banderso@novell.com>
 * 
 ***********************************************************************/

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
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
		/// Credential used to authenticate alias in its domain.
		/// </summary>
		private RSA credential;
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
		public string Id
		{
			get { return id; }
		}

		/// <summary>
		/// Gets the keyValues used to authenticate alias in its domain.
		/// </summary>
		public RSAParameters PublicKey
		{
			get { return credential.ExportParameters( false ); }
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
			this.domain = alias.DocumentElement.GetAttribute( Property.DomainName );
			this.id = alias.DocumentElement.GetAttribute( Property.IDAttr );
			this.credential = RSA.Create();
			this.credential.FromXmlString( alias.DocumentElement.InnerText );
		}

		/// <summary>
		/// Constructor for creating a new alias object.
		/// </summary>
		/// <param name="domain">Domain where this alias exists.</param>
		/// <param name="id">Unique identifier for this alias.</param>
		/// <param name="keyValues">RSAParameters object used to authenticate alias in its domain.</param>
		internal Alias( string domain, string id, RSAParameters keyValues )
		{
			this.domain = domain;
			this.id = id;
			this.credential = RSA.Create();
			this.credential.ImportParameters( keyValues );
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Gets a xml string representation of the alias object.  This is the format used to store the object in the
		/// collection store.
		/// </summary>
		/// <returns>An string that represents the serialized alias object.</returns>
		internal string ToXmlString()
		{
			// Create an xml document that will hold the serialized object.
			XmlDocument alias = new XmlDocument();
			XmlElement aliasRoot = alias.CreateElement( Property.AliasParameters );
			alias.AppendChild( aliasRoot );

			// Set the attributes on the object.
			aliasRoot.SetAttribute( Property.DomainName, domain );
			aliasRoot.SetAttribute( Property.IDAttr, id );

			// Set the value of the element which is the credential.
			aliasRoot.InnerText = credential.ToXmlString( false );
			return alias.OuterXml;
		}
		#endregion
	}
}
