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
using System.Text.RegularExpressions;
using System.Xml;

namespace Simias.Storage
{
	/// <summary>
	/// Represents the list of values for a multi-valued property.
	/// </summary>
	public class MultiValuedList : IEnumerable
	{
		#region Class Members
		/// <summary>
		/// Name of this property.
		/// </summary>
		private string name;

		/// <summary>
		/// The property list where this multivalued property is stored.
		/// </summary>
		private PropertyList propertyList;

		/// <summary>
		/// Array that will hold all of the multiple values.
		/// </summary>
		private ArrayList valueList = new ArrayList();
		#endregion

		#region Properties
		/// <summary>
		/// Gets the count of values contained in this object.
		/// </summary>
		public int Count
		{
			get { return valueList.Count; }
		}

		/// <summary>
		/// Gets the specified property.
		/// </summary>
		public Property this[ int index ]
		{
			get { return new Property( propertyList, ( XmlElement )valueList[ index ] ); }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructs the object.
		/// </summary>
		/// <param name="propertyList">The property list where this multivalued property is stored.</param>
		/// <param name="name">Name of the property that represents the multiple values.</param>
		/// <param name="hiddenProperties">If true, return hidden properties.</param>
		internal MultiValuedList( PropertyList propertyList, string name, bool hiddenProperties )
		{
			this.propertyList = propertyList;
			this.name = name;
            
			// Build a regular expression class to use as the comparision.
			Regex searchName = new Regex( "^" + name + "$", RegexOptions.IgnoreCase );

			// Build the list of property values.
			// Walk each property node and do a case-insensitive compare on the names.
			foreach ( XmlElement x in propertyList.PropertyRoot )
			{
				Property p = new Property( propertyList, x );
				if ( ( hiddenProperties || !p.HiddenProperty ) && searchName.IsMatch( p.Name ) )
				{
					valueList.Add( x );
				}
			}
		}

		/// <summary>
		/// Constructs the object.
		/// </summary>
		/// <param name="propertyList">The property list where this multivalued property is stored.</param>
		/// <param name="attribute">Attribute name of the property that represents the multiple values.</param>
		/// <param name="attributeValue">Attribute value to look for.  May be null which means all properties
		/// containing the attribute will be returned.</param>
		internal MultiValuedList( PropertyList propertyList, string attribute, string attributeValue )
		{
			Regex searchValue = null;

			this.propertyList = propertyList;
			this.name = attribute;

			// Build a regular expression class to use as the comparision.
			if ( attributeValue != null )
			{
				searchValue = new Regex( "^" + attributeValue + "$", RegexOptions.IgnoreCase );
			}

			// Build the list of property values.
			// Walk each property node and do a case-insensitive compare on the attributes and then the values.
			foreach ( XmlElement x in propertyList.PropertyRoot )
			{
				// Check if this xml node contains the specified attribute.
				if ( x.HasAttribute( attribute ) )
				{
					if ( ( attributeValue == null ) || ( searchValue.IsMatch( x.InnerText ) ) )
					{
						valueList.Add( x );
					}
				}
			}
		}

		/// <summary>
		/// Constructs the object.
		/// </summary>
		/// <param name="propertyList">The property list where this multivalued property is stored.</param>
		/// <param name="flagBits">Specifies that all properties with the specified flags bits set are returned.</param>
		internal MultiValuedList( PropertyList propertyList, uint flagBits )
		{
			this.propertyList = propertyList;
			this.name = XmlTags.FlagsAttr;

			// Build the list of property values.
			// Walk each property node and do a check to see if the flagbits match.
			foreach ( XmlElement x in propertyList.PropertyRoot )
			{
				// Check if this xml node contains the specified attribute.
				if ( x.HasAttribute( XmlTags.FlagsAttr ) )
				{
					// See if the flag bits match.
					if ( ( Convert.ToUInt32( x.GetAttribute( XmlTags.FlagsAttr ) ) & flagBits ) == flagBits )
					{
						valueList.Add( x );
					}
				}
			}
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Method used by clients to enumerate the properties in the list.
		/// </summary>
		/// <remarks>
		/// The client must call Dispose() to free up system resources before releasing
		/// the reference to the ICSEnumerator.
		/// </remarks>
		/// <returns>A property object that can enumerate the property list.</returns>
		public IEnumerator GetEnumerator()
		{
			return new MultiValuedEnumerator( propertyList, valueList );
		}

		/// <summary>
		/// Enumerator class that allows enumeration of property objects within a MultiValuedList.
		/// </summary>
		private class MultiValuedEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// The enumerator that we will use to enumerate the DOM tree.
			/// </summary>
			private IEnumerator multiValuedEnumerator;

			/// <summary>
			/// The property list where this multivalued property is stored.
			/// </summary>
			private PropertyList propertyList;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor used to instaniate this object by means of an enumerator.
			/// </summary>
			/// <param name="propertyList">The property list where this multivalued property is stored.</param>
			/// <param name="valueList">ArrayList that contains the multivalued properties for a node.</param>
			internal MultiValuedEnumerator( PropertyList propertyList, ArrayList valueList )
			{
				this.propertyList = propertyList;
				multiValuedEnumerator = valueList.GetEnumerator();
			}
			#endregion

			#region IEnumerator Members
			/// <summary>
			/// Sets the enumerator to its initial position, which is before
			/// the first element in the collection.
			/// </summary>
			public void Reset()
			{
				multiValuedEnumerator.Reset();
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get	{ return new Property( propertyList, ( XmlElement )multiValuedEnumerator.Current ); }
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element; 
			/// false if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				return multiValuedEnumerator.MoveNext();
			}
			#endregion

			#region IDisposable Members
			/// <summary>
			/// This is declared here to satisfy the interface requirements, but the MultiValuedEnumerator
			/// does not use any unmanaged resources that it needs to dispose of.
			/// </summary>
			public void Dispose()
			{
			}
			#endregion
		}
		#endregion
	}
}
