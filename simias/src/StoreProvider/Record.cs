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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.Xml;
using System.Collections;

namespace Simias.Storage.Provider
{
	/// <summary>
	/// Represents a Collection Store Provider record.
	/// </summary>
	public class Record : IEnumerable
	{
		private XmlElement recordEl;
		/// <summary>
		/// The Name of the Record.
		/// </summary>
		public string Name;
		/// <summary>
		/// The ID of the Record.
		/// </summary>
		public string Id;
		/// <summary>
		/// The type of the Record.
		/// </summary>
		public string Type;
		
		/// <summary>
		/// Creates a Record from an XML Element. Can be used to enumerate
		/// the Properties contained in this record.
		/// </summary>
		/// <param name="recordXml">The XML Element that describes the Record.</param>
		public Record(XmlElement recordXml)
		{
			recordEl = recordXml;

			// Get the Name, ID, and type.
			Name = recordEl.GetAttribute(XmlTags.NameAttr);
			Id = recordEl.GetAttribute(XmlTags.IdAttr);
			Type = recordEl.GetAttribute(XmlTags.TypeAttr);
		
			// Make sure this is a valid record.
			if (Name == null || Id == null || Type == null)
			{
				throw new FormatException();
			}
		}

		/// <summary>
		/// Creates a Record from the strings.
		/// </summary>
		/// <param name="name">The name of the Record</param>
		/// <param name="id">The ID of the Record.</param>
		/// <param name="type">The type of the Record.</param>
		public Record(string name, string id, string type)
		{
			recordEl = null;
			Name = name;
			Id = id;
			Type = type;
		}

		/// <summary>
		/// Called to get an XML node for this Record.  This node does not include
		/// the property nodes.
		/// </summary>
		/// <param name="doc">The document that the XML node belongs to.</param>
		/// <returns>The XmlElement representing this Record.</returns>
		public XmlElement ToXml(XmlDocument doc)
		{
			recordEl = doc.CreateElement(XmlTags.ObjectTag);
			recordEl.SetAttribute(XmlTags.NameAttr, Name);
			recordEl.SetAttribute(XmlTags.IdAttr, Id);
			recordEl.SetAttribute(XmlTags.TypeAttr, Type);
			return recordEl;
		}

		#region IEnumerable Members

		/// <summary>
		/// Gets the Property enumerator of this Record.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return (new PropertyEnumerator(recordEl.SelectNodes(XmlTags.PropertyTag)));
		}

		#endregion
	}

	/// <summary>
	/// Class to enumerate through properties of a record.
	/// </summary>
	public class PropertyEnumerator : IEnumerator
	{
		IEnumerator	properties;

		/// <summary>
		/// Exposes an XmlNodeList as Properties.
		/// </summary>
		/// <param name="pList"></param>
		public PropertyEnumerator(XmlNodeList pList)
		{
			properties = pList.GetEnumerator();
		}

		#region IEnumerator Members

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first Property in the collection.
		/// </summary>
		public void Reset()
		{
			properties.Reset();
		}

		/// <summary>
		/// Gets the current Property in the collection.
		/// </summary>
		public object Current
		{
			get
			{
				return new Property((XmlElement)properties.Current);
			}
		}

		/// <summary>
		/// Advances the enumerator to the next Property in the collection.
		/// </summary>
		/// <returns></returns>
		public bool MoveNext()
		{
			return properties.MoveNext();
		}

		#endregion
	}

	#region Property class

	/// <summary>
	/// Class to convert a property to and from XML.
	/// </summary>
	public class Property
	{
		private XmlElement property;
		/// <summary>
		/// Property Name.
		/// </summary>
		public string Name;
		/// <summary>
		/// Property Type.
		/// </summary>
		public string Type;
		/// <summary>
		/// Property Value.
		/// </summary>
		public string Value;
		/// <summary>
		/// Property Flags.
		/// </summary>
		public string Flags;
	
		/// <summary>
		/// Creates a Property from an XML element.
		/// </summary>
		/// <param name="propertyEl">The XML element for this Property.</param>
		public Property(XmlElement propertyEl)
		{
			property = propertyEl;
			Name = property.GetAttribute(XmlTags.NameAttr);
			Type = property.GetAttribute(XmlTags.TypeAttr);
			Value = property.InnerXml;
			Flags = property.GetAttribute(XmlTags.FlagsAttr);
			if (string.Empty == Flags)
			{
				Flags = "0";
			}
		}

		/// <summary>
		/// Called to create an XML node that represents a property.
		/// </summary>
		/// <param name="doc">The document the XML node belongs to.</param>
		/// <param name="Name">The name of the property.</param>
		/// <param name="Type">The type of the property.</param>
		/// <param name="Flags">The flags of the property.</param>
		/// <param name="Value">The value of the property.</param>
		/// <returns>The created XMLElement.</returns>
		public static XmlElement CreateXmlNode(XmlDocument doc, string Name, string Type, string Flags, string Value)
		{
			XmlElement node = doc.CreateElement(XmlTags.PropertyTag);
			node.SetAttribute(XmlTags.NameAttr, Name);
			node.SetAttribute(XmlTags.TypeAttr, Type);
			node.SetAttribute(XmlTags.FlagsAttr, Flags);
			node.InnerXml = Value;
			return node;
		}
	}

	#endregion
}
