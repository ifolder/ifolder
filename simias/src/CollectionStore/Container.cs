/***********************************************************************
 *  Container.cs 
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
 * 
 ***********************************************************************/

using System;
using System.Collections;

namespace Simias.Storage
{
	/// <summary>
	/// This object allows enumeration by an ICSEnumerator object on a container.
	/// </summary>
	internal class Container : IEnumerable, ICSEnumerator
	{
		#region Class Members
		/// <summary>
		/// Array that will hold all of the multiple values.
		/// </summary>
		private ArrayList valueList = new ArrayList();

		/// <summary>
		/// Enumerator object for the container.
		/// </summary>
		private IEnumerator valueEnumerator;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the count of values contained in this object.
		/// </summary>
		[ Obsolete( "This will be removed next iteration", false ) ]
		public int Count
		{
			get { return valueList.Count; }
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds an object to the container.
		/// </summary>
		/// <param name="value">Adds a value to the container.</param>
		public void Add( object value )
		{
			valueList.Add( value );
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Method used by clients to enumerate the objects in the container.
		/// </summary>
		/// <remarks>
		/// The client must call Dispose() to free up system resources before releasing
		/// the reference to the ICSEnumerator.
		/// </remarks>
		/// <returns>This object.</returns>
		public IEnumerator GetEnumerator()
		{
			valueEnumerator = valueList.GetEnumerator();
			return this;
		}

		#region IEnumerator Members
		/// <summary>
		/// Sets the enumerator to its initial position, which is before
		/// the first element in the collection.
		/// </summary>
		public void Reset()
		{
			valueEnumerator.Reset();
		}

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public object Current
		{
			get { return valueEnumerator.Current; }
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
			return valueEnumerator.MoveNext();
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
		#endregion
	}
}
