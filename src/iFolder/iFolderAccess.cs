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
using Novell.AddressBook;
using Simias.Storage;

namespace Novell.iFolder
{
	/// <summary>
	/// Represents a file or folder in an iFolder.
	/// </summary>
	public class IFAccessControlEntry
	{
		private iFolder.Rights	rights;
		private Contact			contact;

		/// <summary>
		/// Gets the rights assigned in this ACE
		/// </summary>
		public iFolder.Rights Rights
		{
			get { return rights; }
		}

		/// <summary>
		/// Gets the contact to which the rights are assigned
		/// </summary>
		public Contact Contact
		{
			get { return contact; }
		}

		/// <summary>
		/// Constructor for the IFACEList to use to construct entries
		/// </summary>
		internal IFAccessControlEntry( iFolder.Rights rights, Contact contact )
		{
			this.rights = rights;
			this.contact = contact;
		}
	}



	/// <summary>
	/// Represents a file or folder in an iFolder.
	/// </summary>
	public class IFAccessControlList : IEnumerable
	{
		private IFACLEnumerator ifACLEnumerator;

		/// <summary>
		/// Constructor for the IFACEList to use to construct entries
		/// </summary>
		internal IFAccessControlList( Collection collection, Novell.AddressBook.AddressBook ab )
		{
			ifACLEnumerator = new IFACLEnumerator( ab, collection.GetAccessControlList().GetEnumerator() );
		}

		public IEnumerator GetEnumerator()
		{
			return ifACLEnumerator;
		}


		private class IFACLEnumerator : IEnumerator
		{
			private IEnumerator iEnumerator;
			private Novell.AddressBook.AddressBook	ab;

			public IFACLEnumerator( Novell.AddressBook.AddressBook ab, IEnumerator iEnumerator )
			{
				this.ab = ab;
				this.iEnumerator = iEnumerator;
			}

			public void Reset()
			{
				iEnumerator.Reset();
			}

			public object Current
			{
				get
				{
					try
					{
						AccessControlEntry ace = iEnumerator.Current as AccessControlEntry;
						Contact c = ab.GetContact( ace.ID );
						return new IFAccessControlEntry( iFolder.MapAccessRights( ace.Rights ), c );
					}
					catch( Exception e )
					{
						// This should never happen unless somebody calls
						// Current before MoveNext() which they are not
						// supposed to do
						return null;
					}
				}
			}

			public bool MoveNext()
			{
				// The collection has some entries that are well known
				// and should not be exposed at the iFolder level
				// Loop past these while enumerating
				bool hasData = iEnumerator.MoveNext();
				while ( hasData )
				{
					AccessControlEntry ace = iEnumerator.Current as AccessControlEntry;
					if ( ace.ID != Access.World )
					{
						break;
					}

					hasData = iEnumerator.MoveNext();
				}

				return hasData;
			}
		}
	}
}
