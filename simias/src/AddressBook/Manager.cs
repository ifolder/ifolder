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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/
using System;
using System.Collections;
using System.IO;
using Simias;
using Simias.Storage;

//using Simias.Identity;

namespace Novell.AddressBook
{
	/// <summary>
	/// Summary description for the Manager class.
	/// </summary>
	public class Manager : IEnumerable, IEnumerator
	{
		#region Class Members
		/// <summary>
		/// The user that opened the address manager.
		/// </summary>
		private string		storeUserName = null;

		/// <summary>
		/// Path to the store that this instance represents.
		/// </summary>
		//private string	storePath = null;

		private Store		store = null;

		//private	Collection	addressCollection = null;
		private	IEnumerator	storeEnum = null;

		#endregion

		#region Properties
		#endregion

		#region Constructors
		internal Manager(Store myStore)
		{
			this.store = myStore;
		}
		#endregion

		#region Private Methods
		/*
		private static Store GetStore(string userName, string password)
		{
			//bool	createStore = false;
			string	path;

			path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if(path == "")
			{
				path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                path += "/.csstore";

                DirectoryInfo dir = new DirectoryInfo(path);
                if (dir.Exists == false)
                {
                    dir.Create();
                }
			}

			return Store.Connect( new Uri(path));
		}
		*/
		#endregion

		#region Static Methods
		/// <summary>
		/// Authenticates the current user to the collection store and returns an object 
		/// used to manipulate address books.
		/// </summary>
		///	<returns>An object that represents a connection to the address manager.</returns>
		public static Manager Connect( )
		{
			Manager	addressManager = null;

			Store store = new Store( new Configuration(null) );
			if(store != null)
			{
				addressManager = new Manager(store);
				if(addressManager != null)
				{
					addressManager.storeUserName = Environment.UserName;
				}
			}

			return(addressManager);
		}

		/// <summary>
		/// Connects to the collection store with a caller defined config object. 
		/// </summary>
		///	<returns>An object that represents a connection to the address manager.</returns>
		public static Manager Connect(Configuration cConfig)
		{
			Manager	addressManager = null;
			Store store = new Store( cConfig );
			//Store store = Store.Connect(callerConfig);
			if(store != null)
			{
				addressManager = new Manager(store);
				if(addressManager != null)
				{
					addressManager.storeUserName = Environment.UserName;
				}
			}

			return(addressManager);
		}

		/*
		/// <summary>
		/// Connects to the Address Book Manager using a known database path.
		/// </summary>
		/// <param name="dbPath">Path that specifies where to create or open the database.</param>
		///	<returns>An object that represents a connection to the address book manager.</returns>
		public static Manager Connect(Uri dbPath )
		{
			Manager	addressManager = null;

			Store store = Store.Connect(dbPath);
			if(store != null)
			{
				addressManager = new Manager(store);
				if(addressManager != null)
				{
					addressManager.storeUserName = Environment.UserName;
				}
			}

			return(addressManager);
		}
		*/
		#endregion

		#region Public Methods

		/// <summary>
		/// Adds an address book to the store
		/// </summary>
		/// <remarks>
		/// Add will throw an exception if the address book cannot
		/// be added to the store or if the address book
		/// object is incomplete.
		/// </remarks>
		public void	AddAddressBook(AddressBook addressBook)
		{
			addressBook.Add(this.store);
		}

		/// <summary>
		/// Opens the default or local address book.
		/// The collection store always creates the local address
		/// book when the store is created so we should always find it
		/// </summary>
		///	<returns>An AddressBook object</returns>
		public AddressBook OpenDefaultAddressBook()
		{
			try
			{
				LocalAddressBook lAddressBook = this.store.GetLocalAddressBook();
				return(this.GetAddressBookByName(lAddressBook.Name));
			}
			catch{}
			return(null);

			/*
			try
			{
				AddressBook addrBook = 
					new AddressBook(
							idFactory.CurrentId.DomainName + ":" + Environment.UserName,
							AddressBookType.Private, 
							AddressBookRights.ReadWrite, 
							true);

				addrBook.Domain = idFactory.CurrentId.DomainName;

				// Add commits the address book to the store
				addrBook.Add(this.store);

				// Create a contact for the current user
				Contact tmpContact = new Contact();
				tmpContact.UserName = Environment.UserName;

				addrBook.AddContact(tmpContact, idFactory.CurrentId.UserGuid);
				//addrBook.AddContact(tmpContact);
				tmpContact.Commit();
				return(addrBook);
			}
			catch
			{
				throw new ApplicationException(Common.addressBookExceptionHeader + "failed to create the default address book");
			}
			*/
		}

		/// <summary>
		/// Gets an address book object by the address book ID
		/// </summary>
		///	<returns>An AddressBook object</returns>
		public AddressBook GetAddressBook(string bookID)
		{
			try
			{
				AddressBook addressBook = new AddressBook(this.store);
				addressBook.ToObject(bookID);
				return(addressBook);
			}
			catch
			{
				throw new ApplicationException(Common.addressBookExceptionHeader + bookID + " not found");
			}
		}

		/// <summary>
		/// Gets an address book object by the friendly name
		/// Note: Friendly names are not guaranteed unique so
		/// this method will return the first one found
		/// </summary>
		///	<returns>An AddressBook object</returns>
		public AddressBook GetAddressBookByName(string friendlyName)
		{
			AddressBook cBook = null;
			ICSList	abList = this.store.GetCollectionsByName(friendlyName);
			IEnumerator	bookEnum = abList.GetEnumerator();
			if(bookEnum.MoveNext())
			{
				cBook = new AddressBook(this.store);
				cBook.ToObject(((ShallowNode) bookEnum.Current).ID);
			}

			return(cBook);
		}

		/// <summary>
		/// Get an enumerator for enuming address books
		/// </summary>
		///	<returns>An IEnumerator object</returns>
		public ArrayList GetAddressBooks()
		{
			ArrayList bookList = new ArrayList();
			ICSList	abList = this.store.GetCollectionsByType("AB:AddressBook");

			foreach(ShallowNode sNode in abList)
			{
				AddressBook cBook = new AddressBook(this.store);
				cBook.ToObject(sNode.ID);
				bookList.Add(cBook);
			}

			return(bookList);
			//return new AddressBookEnumerator( this.store );
		}
		#endregion

		#region IEnumerable
		/// <summary>
		/// Get an enumerator
		/// </summary>
		///	<returns>An IEnumerator object</returns>
		public IEnumerator GetEnumerator()
        {
			storeEnum = store.GetEnumerator();
			return(this);
        }

        /*
            Implementation of the iEnumerator members        
        */

		/// <summary>
		/// Move to the next address book
		/// </summary>
		///	<returns>TRUE/FALSE</returns>
		public bool MoveNext()
        {
			while(storeEnum.MoveNext())
			{
				Node cNode = (Node) storeEnum.Current;
				if (cNode.Type == "AB:AddressBook")
				{
					return(true);
				}
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
					Node cNode = (Node) storeEnum.Current;
					AddressBook tmpBook = new AddressBook(this.store);
					tmpBook.ToObject(cNode.ID);
					return((object) tmpBook);
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

	public class ABManager : Manager
	{
		#region Constructors
		internal ABManager(Store myStore) : base (myStore)
		{
			//this.store = myStore;
		}
		#endregion
	}
}
