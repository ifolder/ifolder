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
using Simias;
using Simias.Storage;

namespace Novell.Collaboration
{
	/// <summary>
	/// Summary description for the SlogManager class.
	/// </summary>
	public class SlogManager : IEnumerable, IEnumerator
	{
		#region Class Members
		/// <summary>
		/// Path to the store that this instance represents.
		/// </summary>
		private Store		store = null;

		private	IEnumerator	storeEnum = null;
		#endregion

		#region Constructors
		internal SlogManager(Store myStore)
		{
			this.store = myStore;
		}
		#endregion

		#region Static Methods
		/// <summary>
		/// Connects to Store and returns the SlogManager
		/// </summary>
		///	<returns>An object managing Slogs.</returns>
		public static SlogManager Connect( )
		{
			SlogManager	mgr = null;

			Store store = Store.GetStore();
			if(store != null)
			{
				mgr = new SlogManager(store);
			}

			return(mgr);
		}

		/// <summary>
		/// Connects to Store and returns the SlogManager
		/// </summary>
		///	<returns>An object managing Slogs.</returns>
		public static SlogManager Connect(Configuration cConfig)
		{
			SlogManager	mgr = null;
			Store store = new Store( cConfig );

			if(store != null)
			{
				mgr = new SlogManager(store);
			}

			return(mgr);
		}


		/// <summary>
		/// Creates a Slog Collection
		/// </summary>
		/// <param name="name">The friendly name that of this Slog.</param>
		public Slog CreateSlog(string name)
		{
			Slog slog = new Slog(this.store, name);
			return slog;
		}


		/// <summary>
		/// Gets an address book object by the address book ID
		/// </summary>
		///	<returns>An AddressBook object</returns>
		public Slog GetSlog(string slogID)
		{
			Slog slog = null;
			try
			{
				Collection collection = store.GetCollectionByID(slogID);
				if(collection != null)
				{
					slog = new Slog(this.store, collection);
				}
				return(slog);
			}
			catch
			{
				throw new ApplicationException("Slog not found");
			}
		}
		#endregion

		#region IEnumerable
		/// <summary>
		/// Get an enumerator
		/// </summary>
		///	<returns>An IEnumerator object</returns>
		public IEnumerator GetEnumerator()
        {
			ICSList	abList = this.store.GetCollectionsByType(typeof(Slog).Name);
			storeEnum = abList.GetEnumerator();
			return(this);
        }


		/// <summary>
		/// Move to the next address book
		/// </summary>
		///	<returns>TRUE/FALSE</returns>
		public bool MoveNext()
        {
			while(storeEnum.MoveNext())
			{
				return(true);
			}

			return(false);
        }

		/// <summary>
		/// Get the current address book
		/// </summary>
		///	<returns>An AddressBook object</returns>
		public object Current
        {
            get
            {
				try
				{
					Slog slog =
						GetSlog( ((ShallowNode) storeEnum.Current).ID);
					return((object) slog);
				}
				catch{}
				return(null);
            }
        }

		/// <summary>
		/// Reset the enumerator object so the caller can restart
		/// the enumeration.
		/// </summary>
		public void Reset()
        {
			storeEnum.Reset();
        }

		#endregion
	}
}
