/***********************************************************************
 *  ContactList.cs - ContactList class 
 *  !NOTE! this class is obsolete and will be removed in the future.
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
 *  Author: Brady Anderson <banderso@novell.com>
 * 
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using Simias.Storage;
using Novell.AddressBook;
using Simias.Identity;

namespace Novell.AddressBook
{
	/// <summary>
	/// Summary description for the ContactList class.
	/// </summary>
	public class ContactList : IEnumerable, IEnumerator
	{
		#region Class Members

		/// <summary>
		/// The user that opened the collection store.
		/// </summary>
//		private string		storeUserName = null;

		/// <summary>
		/// Path to the store that this instance represents.
		/// </summary>
		//private string	storePath = null;

		private Store		store = null;

		//private	Collection	addressCollection = null;
		private	IEnumerator	contactEnum = null;

		private	Collection	collection = null;

		#endregion

		#region Constructors
		internal ContactList(Store MyStore)
		{
			this.store = MyStore;
		}
		#endregion

		#region Private Methods

		// TODO: Need a method to get the store via an identity

		/// <summary>
		/// Get an instance of the global contact list collection.  If the global contact
		/// list doesn't exist go ahead and create it.
		/// </summary>
		///	<returns>An object that represents the global contact list collection.</returns>
		private Collection GetContactList()
		{
			// Enumerate through the collections looking for our global contact list
			Collection	tmpCollection;
			IEnumerator storeEnum;

			storeEnum = store.GetEnumerator();
			while(storeEnum.MoveNext() == true)
			{
				tmpCollection = (Collection) storeEnum.Current;
				
				Property p = tmpCollection.Properties.GetSingleProperty( "IsContactList" );
				if (p != null && (bool)p.Value == true)
				{
					return(tmpCollection);
				}
			}

			// Obviously if we get to here our global contact list does not exist so let's 
			// go ahead and create it.

			tmpCollection = store.CreateCollection("CAB:ContactList", new Uri(Path.GetFullPath(".")));
			if(tmpCollection != null)
			{
				tmpCollection.Properties.AddProperty( "IsContactList", true);
				this.collection.Commit();
				// BUGBUG have to save somewhere first
				//tmpCollection.Commit();
				return(tmpCollection);
			}
			else
			{
				throw new Exception("Could not create the \"Global Contact List\"");
			}
		}		

		#endregion

		#region Internal Methods

		/// <summary>
		/// Authenticates the current user to the collection store and returns an object 
		/// used to manipulate the global contact list.
		/// </summary>
		///	<returns>An object that represents a connection to an address book context.</returns>
		///
		/*
		internal void Create()
		{
			//
			// Create a contact list for this security domain
			//

			this.collection = store.CreateCollection(Common.contactListType);
			if(this.collection != null)
			{
				collection.Properties.AddProperty( 
					"AB:Workspace", 
					"CLWS:" + Environment.MachineName + ":" + Environment.UserName);

				// Save and commit
				//collection.Save();
				//collection.CommitChanges();

				//
				// If we created the contact list for the local workspace we
				// need to create a contact with the local identity!
				//

				Contact contact = this.CreateContact(Environment.UserName);
				contact.AddIdentity(store.StoreOwner);
				collection.Commit();
			}
			else
			{
				throw new Exception("Could not create the \"Global Contact List\"");
			}
		}
		*/

		/// <summary>
		/// Method: Load
		/// Abstract: Contact lists are created only through the manager class.  The 
		/// FinalConstructor method is called after construction so exceptions can be 
		/// generated back to the manager method "CreateContactList".
		/// 
		/// </summary>
		/// 
		internal void Load(Store callingStore, string listID)
		{
			this.store = callingStore;
			this.collection = store.GetCollectionById(listID);

			// Make sure this collection has our store propery
			if (this.collection.Name != Common.contactListType)
			{
				// Raise an exception here
			}
		}

		#endregion

		#region Public Methods

		/*
		[ Obsolete( "This method is marked for eventual removal. Use method 'AddressBook' class instead.", false ) ]
		public Contact CreateContact(string userName)
		{
			Contact contact = new Contact(collection);

			try
			{
				contact.Add(this.collection, null, null);
				//contact.Create(userName);
				return(contact);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				throw new ApplicationException("Contact " + userName + "not created");
				//return(null);
			}
		}
		*/

		[ Obsolete( "This method is marked for eventual removal. Use method 'AddressBook' class instead.", false ) ]
		public Contact GetContact(string contactID)
		{
			Contact contact = new Contact(this.collection);

			try
			{
				contact.ToObject(this.collection, contactID);
				return(contact);
			}
			catch
			{
				return(null);
			}
		}

		/// <summary>
		/// Retrieve the contact the specified identity is associated to.
		/// </summary>
		/// <param name="identityID">Specified Identity ID</param>
		/// <remarks>
		/// Each contact contains an associated primary identity.
		/// Consumers may call the Identity interface to retrieve their current
		/// identity.  With the ID of the current identity, consumers can then
		/// issue this method call to retrieve full contact information for the
		/// specified identity.
		/// 
		/// If an associated contact is not found an application exception is raised
		/// </remarks>
		/// <returns>A Contact object with at minimum a valid username property.</returns>
		[ Obsolete( "This method is marked for eventual removal. Use method 'AddressBook' class instead.", false ) ]
		public Contact GetContactByIdentity(string identityID)
		{
			// First make sure we get the master ID
			IIdentityFactory idFactory = IdentityManager.Connect();
			IIdentity masterID = idFactory.GetIdentityFromUserGuid( identityID );

			ICSList searchResults = 
				//this.collection.Search(Common.identityProperty, masterID.Id, Property.Operator.Equal);
				this.collection.Search(Common.identityProperty, masterID.UserGuid, Property.Operator.Equal);

			try
			{
				foreach(Node node in searchResults)
				{
					if (node.Type == Common.contactType)
					{
						return(this.GetContact(node.Id));
					}
				}
			}
			catch{}
			throw new ApplicationException("Contact node does not exist");
		}

		[ Obsolete( "This method is marked for eventual removal. Use method 'AddressBook' class instead.", false ) ]
		public ICSEnumerator SearchUsername( string searchString, Property.Operator queryOperator )
		{
			return((ICSEnumerator) this.collection.Search(Property.ObjectName, searchString, queryOperator).GetEnumerator());
		}

		[ Obsolete( "This method is marked for eventual removal. Use method 'AddressBook' class instead.", false ) ]
		public ICSEnumerator SearchEmail( string searchString, Property.Operator queryOperator )
		{
			return((ICSEnumerator) this.collection.Search(Common.emailProperty, searchString, queryOperator).GetEnumerator());
		}
		#endregion

		#region IEnumerable
		public IEnumerator GetEnumerator()
		{
			//Console.WriteLine("AddressBook::GetEnumerator called");
			//Reset();
			contactEnum = this.collection.GetEnumerator();
			return(this);
		}

		/*
			Implementation of the iEnumerator members        
		*/

		public bool MoveNext()
		{
			//
			// TODO
			// Make sure the node is an address book contact
			//

			if(contactEnum != null)
			{
				while(contactEnum.MoveNext() == true)
				{
					Node tmpNode = (Node) contactEnum.Current;
					if (tmpNode.Type == Common.contactType)
					{
						return(true);
					}
				}

				return(false);
			}

			return(false);
		}

		public object Current
		{
			get
			{
				if(contactEnum != null)
				{
					//Console.WriteLine("AddressBook::Current called");
					Node currentNode = (Node) contactEnum.Current;
					//Console.WriteLine("Contact: {0}", currentNode.Name);
					return((object) this.GetContact(currentNode.Id));
				}
				else
				{
					return(null);
				}
			}
		}

		public void Reset()
		{
			//Console.WriteLine("AddressBook::Reset called");
			contactEnum.Reset();
		}

		#endregion
	}
}
