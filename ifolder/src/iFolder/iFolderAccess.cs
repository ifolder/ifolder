/***********************************************************************
 *  $RCSfile$
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
			get
			{
				return(rights);
			}
		}

		/// <summary>
		/// Gets the contact to which the rights are assigned
		/// </summary>
		public Contact Contact
		{
			get
			{
				return(contact);
			}
		}

		/// <summary>
		/// Constructor for the IFACEList to use to construct entries
		/// </summary>
		internal IFAccessControlEntry(iFolder.Rights rights, Contact contact)
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
		internal IFAccessControlList(Collection collection, AddressBook ab)
		{
			ICSList icsList = collection.GetAccessControlList();

			this.ifACLEnumerator = new IFACLEnumerator( ab, 
					icsList.GetEnumerator());
		}

		public IEnumerator GetEnumerator()
		{
			return ifACLEnumerator;
		}


		private class IFACLEnumerator : IEnumerator
		{
			private IEnumerator iEnumerator;
			private AddressBook	ab;

			public IFACLEnumerator( AddressBook ab,
									IEnumerator iEnumerator )
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
					AccessControlEntry ace = 
						(AccessControlEntry)iEnumerator.Current;

					Contact c = ab.GetContact(ace.Id);
					IFAccessControlEntry iface = new 
						IFAccessControlEntry((iFolder.Rights)ace.Rights, c);

					return( iface );
				}
			}

			public bool MoveNext()
			{
				// The collection has some entries that are well known
				// and should not be exposed at the iFolder level
				// Loop past these while enumerating
				while(true)
				{
					if(iEnumerator.MoveNext() == false)
						return false;

					AccessControlEntry ace = 
							(AccessControlEntry)iEnumerator.Current;

					if(ace.WellKnown == false)
						return true;
				}
			}
		}
	}
}
