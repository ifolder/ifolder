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
using Simias.POBox;
using Simias.Storage;
using Simias.Sync;

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
		/// <summary>
		/// Creates a personal subscription for an iFolder that can be used by this user on another
		/// machine to sync down this iFolder.
		/// </summary>
		/// <param name="ab">AddressBook object that was created.</param>
		private void CreatePersonalSubscription( AddressBook ab )
		{
			// Get the current member for this iFolder.
			Member member = ab.GetCurrentMember();

			// Get or create a POBox for the user.
			POBox poBox = POBox.GetPOBox( store, ab.Domain, member.UserID );

			// Create a subscription for this iFolder in the POBox.
			Subscription subscription = poBox.CreateSubscription( ab, member, typeof( AddressBook ).Name );

			// Set the 'To:' field in the subscription the the current user, so that this subscription cannot
			// be used by any other person.
			subscription.ToName = member.Name;
			subscription.ToIdentity = member.UserID;
			subscription.ToPublicKey = member.PublicKey;
			subscription.SubscriptionRights = member.Rights;
			subscription.SubscriptionState = SubscriptionStates.Ready;

			// TODO: This may not be right in the future.
			// Get the master URL from the domain.
			Domain domain = store.GetDomain( ab.Domain );
			//subscription.SubscriptionCollectionURL = domain.MasterUrl.ToString();

			// Commit the subscription to the POBox.
			poBox.Commit( subscription );
		}

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

			Store store = Store.GetStore();
			if(store != null)
			{
				addressManager = new Manager(store);
			}

			return(addressManager);
		}

		#endregion

		#region Public Methods

/*
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
*/

/*
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
		}
*/

		/// <summary>
		/// Gets an address book object by the address book ID
		/// </summary>
		///	<returns>An AddressBook object</returns>
		public AddressBook CreateAddressBook(string name)
		{
			AddressBook abook = null;
			try
			{
				abook = new AddressBook(store, name);
				abook.Commit();
				CreatePersonalSubscription( abook );
				return (abook);
			}
			catch
			{
				throw new ApplicationException("Unable to create AddressBook");
			}
		}


		/// <summary>
		/// Gets an address book object by the address book ID
		/// </summary>
		///	<returns>An AddressBook object</returns>
		public AddressBook GetAddressBook(string bookID)
		{
			AddressBook abook = null;
			try
			{
				Collection collection = store.GetCollectionByID(bookID);
				if(collection != null)
				{
					abook = new AddressBook(this.store, collection);
				}
				return(abook);
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
				cBook = GetAddressBook( ((ShallowNode)bookEnum.Current).ID );
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
			ICSList	abList = this.store.GetCollectionsByType(Common.addressBookType);

			foreach(ShallowNode sNode in abList)
			{
				AddressBook cBook = GetAddressBook(sNode.ID);
				bookList.Add(cBook);
			}

			return(bookList);
			//return new AddressBookEnumerator( this.store );
		}


		/// <summary>
		/// Get an enumerator for enuming address books
		/// </summary>
		///	<returns>An IEnumerator object</returns>
		public Contact GetContact(Member member)
		{
			return GetContact(member.UserID);
		}


		/// <summary>
		/// Get an enumerator for enuming address books
		/// </summary>
		///	<returns>An IEnumerator object</returns>
		public Contact GetContact(string UserID)
		{
			Contact foundContact = null;
			ICSList	abList = this.store.GetCollectionsByType(Common.addressBookType);
			foreach(ShallowNode sNode in abList)
			{
				AddressBook cBook = GetAddressBook(sNode.ID);
				
				ICSList results = cBook.Search(	Common.userIDProperty,
												UserID,
												SearchOp.Equal );

				// Instead of looping trough the results, we are just
				// going to return the first one
				foreach(ShallowNode cNode in results)
				{
					foundContact = cBook.GetContact(cNode.ID);
					return foundContact;
				}
			}

			return foundContact;
		}


		/// <summary>
		/// Get an enumerator for enuming address books
		/// </summary>
		///	<returns>An IEnumerator object</returns>
		public Contact GetContact(Collection col, string UserID)
		{
			Contact foundContact = null;
			if(col.IsType(col, Common.addressBookType))
			{
				AddressBook cBook = new AddressBook(store, col);
				ICSList results = cBook.Search(	Common.userIDProperty,
												UserID,
												SearchOp.Equal );

				// Instead of looping trough the results, we are just
				// going to return the first one
				foreach(ShallowNode cNode in results)
				{
					foundContact = cBook.GetContact(cNode.ID);
					return foundContact;
				}
			}

			return GetContact(UserID);
		}
		#endregion

		#region IEnumerable
		/// <summary>
		/// Get an enumerator
		/// </summary>
		///	<returns>An IEnumerator object</returns>
		public IEnumerator GetEnumerator()
        {
			ICSList	abList = this.store.GetCollectionsByType(Common.addressBookType);
			storeEnum = abList.GetEnumerator();
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
					AddressBook tmpBook = GetAddressBook( ((ShallowNode) storeEnum.Current).ID);
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

	/// <summary>
	/// Summary description for the ABManager class.
	/// </summary>
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
