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
	/// Interface declaration for an enumerator that implements a dispose method.
	/// The Simias.Storage.Provider.iResultsSet requires that it be disposed
	/// by the same thread that allocated it.  This interface gives the applications
	/// a way to know that even though they receive an IEnumerator interface to Collection
	/// Store objects, the IEnumerator can always be upcasted to ICSEnumerator and
	/// Dispose() can then be called.
	/// </summary>
	/// <remarks>
	/// The client must call Dispose() to free up system resources before releasing
	/// the reference to the ICSEnumerator.
	/// </remarks>
	public interface ICSEnumerator : IEnumerator, IDisposable
	{
	}

	/// <summary>
	/// Container object that encapsulates an ICSEnumerator.
	/// </summary>
	public class ICSList : IEnumerable
	{
		#region Class Members
		/// <summary>
		/// Array that will hold all of the multiple values.
		/// </summary>
		private ArrayList valueList;

		/// <summary>
		/// Enumerator used to enumerate list items.
		/// </summary>
		private IEnumerator iEnumerator;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for the object.
		/// </summary>
		internal ICSList()
		{
			this.valueList = new ArrayList();
			this.iEnumerator = null;
		}

		/// <summary>
		/// Constructor for the object.
		/// </summary>
		/// <param name="icsEnumerator">Enumerator that contains objects.</param>
		internal ICSList( ICSEnumerator icsEnumerator )
		{
			this.valueList = null;
			this.iEnumerator = icsEnumerator;
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Adds an object to the container.
		/// </summary>
		/// <param name="value">Adds a value to the container.</param>
		internal void Add( object value )
		{
			lock ( this )
			{
				if ( valueList != null )
				{
					valueList.Add( value );
				}
				else
				{
					throw new ApplicationException( "Cannot add to this type of enumerator" );
				}
			}
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that can iterate through the ICSList.
		/// </summary>
		/// <returns>An ICSEnumerator object.</returns>
		public IEnumerator GetEnumerator()
		{
			lock ( this )
			{
				if ( valueList != null )
				{
					return new ICSListEnumerator( this, valueList.GetEnumerator() );
				}
				else
				{
					return iEnumerator;
				}
			}
		}
		#endregion

		/// <summary>
		/// Class used to implement the enumeration for the ICSList class.
		/// </summary>
		private class ICSListEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// List object that is being enumerated.
			/// </summary>
			private ICSList listObject;

			/// <summary>
			/// Enumerator used to enumerate list items.
			/// </summary>
			private IEnumerator iEnumerator;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructs the object.
			/// </summary>
			/// <param name="listObject">List object that is being enumerated.</param>
			/// <param name="iEnumerator">Enumerator from the ICSList object.</param>
			public ICSListEnumerator( ICSList listObject, IEnumerator iEnumerator )
			{
				this.listObject = listObject;
				this.iEnumerator = iEnumerator;
			}
			#endregion

			#region IEnumerator Members
			/// <summary>
			/// Sets the enumerator to its initial position, which is before
			/// the first element in the collection.
			/// </summary>
			public void Reset()
			{
				lock( listObject )
				{
					iEnumerator.Reset();
				}
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get 
				{
					lock( listObject )
					{
						return iEnumerator.Current; 
					}
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
				lock( listObject )
				{
					return iEnumerator.MoveNext();
				}
			}
			#endregion

			#region IDisposable Members
			/// <summary>
			/// This is declared here to satisfy the interface requirements, but the ICSListEnumerator
			/// does not use any unmanaged resources that it needs to dispose of.
			/// </summary>
			public void Dispose()
			{
			}
			#endregion
		}
	}
}
