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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/
using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using Simias.Storage;

namespace Novell.Collaboration
{
	/// 
	/// <summary>
	/// Summary description for Slog.
	/// </summary>
	public class Slog : Collection
	{
		#region Class Members
		#endregion

		#region Constructors
		/// <summary>
		/// Creates a Slog Collection
		/// </summary>
		/// <param name="store">The store for this Slog.</param>
		/// <param name="name">The friendly name that of this Slog.</param>
		public Slog(Store store, string name) :
			base(store, name, store.DefaultDomain)
		{
			// Set the type of the collection.
			this.SetType(this, typeof(Slog).Name);
		}


		/// <summary>
		/// Constructor for creating a Slog object from an exising node.
		/// </summary>
		/// <param name="store">The store object for this Slog.</param>
		/// <param name="node">The node object to construct the Slog.</param>
		public Slog(Store store, Node node)
			: base(store, node)
		{
			// Make sure this collection has our store propery
			if (!this.IsType( this, typeof(Slog).Name ) )
			{
				// Raise an exception here
				throw new ApplicationException( "Invalid Slog collection." );
			}
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that can iterate through the ICSList.
		/// </summary>
		/// <returns>An IEnumerator object.</returns>
		public new IEnumerator GetEnumerator()
		{
			ICSList results = this.Search(	PropertyTags.Types, 
											typeof(SlogEntry).Name, 
											SearchOp.Equal );

			EntryEnumerator eEnumerator = 
					new EntryEnumerator(this, results.GetEnumerator());
			return eEnumerator;
		}
		#endregion
	}

	/// <summary>
	/// Class used for enumerating contacts
	/// </summary>
	public class EntryEnumerator : IEnumerator
	{
		#region Class Members
		private IEnumerator		entryEnum = null;
		private Slog			slog = null;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor used to instantiate this object by means of an enumerator.
		/// </summary>
		/// 
		internal EntryEnumerator(Slog s, IEnumerator enumerator)
		{
			this.slog = s;
			this.entryEnum = enumerator;
		}
		#endregion

		#region IEnumerator Members
		/// <summary>
		/// Sets the enumerator to its initial position
		/// </summary>
		public void Reset()
		{
			entryEnum.Reset();
		}

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public object Current
		{
			get
			{
				try
				{
					ShallowNode sNode = (ShallowNode) entryEnum.Current;
					return(new SlogEntry(slog, sNode));
				}
				catch{}
				return(null);
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
			return entryEnum.MoveNext();
		}
		#endregion
	}
}


