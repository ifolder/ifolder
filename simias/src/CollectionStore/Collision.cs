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
using System.Xml;

using Simias;

namespace Simias.Storage
{
	/// <summary>
	/// Class used to get to the Collision data.
	/// </summary>
	internal class Collision
	{
		#region Class Members
		/// <summary>
		/// Type of collision that this object represents.
		/// </summary>
		private CollisionType type;

		/// <summary>
		/// Context data associated with this collision.
		/// </summary>
		private string contextData;

		/// <summary>
		/// Types of collisions.
		/// </summary>
		public enum CollisionType
		{ 
			/// <summary>
			/// Collision is a result of a Node object update.
			/// </summary>
			Node, 

			/// <summary>
			/// Collision is a result of a file conflict.
			/// </summary>
			File 
		};
		#endregion

		#region Properties
		/// <summary>
		/// Type of collision that this object represents.
		/// </summary>
		public CollisionType Type
		{
			get { return type; }
		}

		/// <summary>
		/// Context data associated with this collision.
		/// </summary>
		public string ContextData
		{
			get { return contextData; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new object.
		/// </summary>
		/// <param name="type">Type of collision that this object represents.</param>
		/// <param name="contextData">Context data associated with this collision.</param>
		public Collision( CollisionType type, string contextData )
		{
			this.type = type;
			this.contextData = contextData;
		}

		/// <summary>
		/// Constructor for creating a new object.
		/// </summary>
		/// <param name="xmlElement">XmlElement that contains the collision data.</param>
		public Collision( XmlElement xmlElement ) :
			this( ( CollisionType )Enum.Parse( typeof( CollisionType ), xmlElement.GetAttribute( CollisionList.TypeTag ) ), xmlElement.InnerText )
		{
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the hashcode for this object.
		/// </summary>
		/// <returns>A hash code for the current Object.</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		/// <summary>
		/// Equals method for Collision object.
		/// </summary>
		/// <param name="obj">Collision object to compare.</param>
		/// <returns>True if objects are equal, otherwise false is returned.</returns>
		public override bool Equals( object obj )
		{
			Collision collision = obj as Collision;
			return ( ( type == collision.type ) && ( contextData == collision.ContextData ) ) ? true : false;
		}

		/// <summary>
		/// Operator override method for equality.
		/// </summary>
		/// <param name="c1">First Collision object.</param>
		/// <param name="c2">Second Collision object.</param>
		/// <returns>True if objects are equal, otherwise false is returned.</returns>
		static public bool operator==( Collision c1, Collision c2 )
		{
			return c1.Equals( c2 );
		}

		/// <summary>
		/// Operator override method for inequality.
		/// </summary>
		/// <param name="c1">First Collision object.</param>
		/// <param name="c2">Second Collision object.</param>
		/// <returns>True if objects are not equal, otherwise false is returned.</returns>
		static public bool operator!=( Collision c1, Collision c2 )
		{
			return !c1.Equals( c2 );
		}
		#endregion
	}

	/// <summary>
	/// Represents Node object collisions or file conflicts in the Collection Store.
	/// </summary>
	internal class CollisionList : IEnumerable
	{
		#region Class Members
		/// <summary>
		/// Root of the collision document.
		/// </summary>
		private const string CollisionListTag = "CollisionList";

		/// <summary>
		/// Type tag for collision.
		/// </summary>
		internal const string TypeTag = "Type";

		/// <summary>
		/// Xml document used as a collision list.
		/// </summary>
		private XmlDocument document;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the XML document that contains the collision list.
		/// </summary>
		public XmlDocument Document
		{
			get { return document; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new object.
		/// </summary>
		public CollisionList()
		{
			document = new XmlDocument();
			document.AppendChild( document.CreateElement( CollisionListTag ) );
		}

		/// <summary>
		/// Constructor for creating a new object from a string representation.
		/// </summary>
		/// <param name="document">Xml document that contains the collision list.</param>
		public CollisionList( XmlDocument document )
		{
			this.document = document;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a collision of the specified type to the collision list.
		/// </summary>
		/// <param name="collision">Collision object to add.</param>
		public void Add( Collision collision )
		{
			XmlElement element = document.CreateElement( PropertyTags.Collision );
			element.SetAttribute( TypeTag, collision.Type.ToString() );
			element.InnerText = collision.ContextData;
			document.DocumentElement.AppendChild( element );
		}

		/// <summary>
		/// Deletes the specified collision from the list.
		/// </summary>
		/// <param name="collision">Collision object to delete.</param>
		public void Delete( Collision collision )
		{
			foreach ( XmlElement xe in document.DocumentElement )
			{
				if ( new Collision( xe ) == collision )
				{
					xe.RemoveAll();
					xe.ParentNode.RemoveChild( xe );
					break;
				}
			}
		}

		/// <summary>
		/// Modifies the existing collision if there is one, otherwise adds the specified collision
		/// to the list.
		/// </summary>
		/// <param name="collision">Collection object add.</param>
		public void Modify( Collision collision )
		{
			// Get the first child element if it exists.
			XmlElement element = document.DocumentElement.FirstChild as XmlElement;
			if ( element != null )
			{
				element.SetAttribute( TypeTag, collision.Type.ToString() );
				element.InnerText = collision.ContextData;
			}
			else
			{
				Add( collision );
			}
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Gets an enumerator for all of the Collision objects belonging to this collection.
		/// </summary>
		/// <returns>An IEnumerator object.</returns>
		public IEnumerator GetEnumerator()
		{
			return new CollisionEnumerator( document );
		}

		/// <summary>
		/// Enumerator class for the collision object that allows enumeration of enumeration objects.
		/// </summary>
		private class CollisionEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// Indicates whether this object has been disposed.
			/// </summary>
			private bool disposed = false;

			/// <summary>
			/// Enumerator for the document.
			/// </summary>
			private IEnumerator documentEnumerator;

			/// <summary>
			/// The number of items in the collision list.
			/// </summary>
			private int count;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor for the CollisionEnumerator object.
			/// </summary>
			/// <param name="document">Xml document containing the collision list.</param>
			public CollisionEnumerator( XmlDocument document )
			{
				documentEnumerator = document.DocumentElement.GetEnumerator();
				count = document.DocumentElement.ChildNodes.Count;
			}
			#endregion

			#region Properties
			/// <summary>
			/// Gets the total number of objects contained in the search.
			/// </summary>
			public int Count
			{
				get { return count; }
			}
			#endregion

			#region IEnumerator Members
			/// <summary>
			/// Sets the enumerator to its initial position, which is before
			/// the first element in the collection.
			/// </summary>
			public void Reset()
			{
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				documentEnumerator.Reset();
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get
				{
					if ( disposed )
					{
						throw new DisposedException( this );
					}

					XmlElement element = documentEnumerator.Current as XmlElement;
					return new Collision( element );
				}
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
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				return documentEnumerator.MoveNext();
			}
			#endregion

			#region IDisposable Members
			/// <summary>
			/// Not needed by this class.
			/// </summary>
			public void Dispose()
			{
			}
			#endregion
		}
		#endregion
	}
}
