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
using System.IO;
using System.Collections;
using Simias;
using Simias.Storage;

namespace Novell.AddressBook
{
	/// <summary>
	/// Bitmap of changed "known" properties
	/// </summary>
	internal enum ChangeMap : uint
	{
		username =		0x00000001,
		title =			0x00000002,
		role =			0x00000004,
		organization =	0x00000008,
		nickname =		0x00000010,
		birthday =		0x00000020,
		url =			0x00000040,
		blog =			0x00000080,
		workforce =		0x00000100,
		manager =		0x00000200,
		note =			0x00000400,
		productid =		0x00000800,
		photo =			0x00001000,
		logo =			0x00002000,
		sound =			0x00004000,
		email =			0x00008000,
		phone =			0x00010000,
		address =		0x00020000,
		name =			0x00040000,
		unknown =		0x08000000
	}

	/// <summary>
	/// Summary description for contact.
	/// A contact is equivalent to a Node in the collection store.
	/// </summary>
	public class Contact : Node, IEnumerable, IEnumerator
	{
		#region Class Members
		internal	AddressBook		addressBook = null;
		private		IEnumerator		thisEnum = null;

		// Private members used for caching objects to the contact before
		// the contact has been added to an address list
		internal	ArrayList		addressList;
		internal	ArrayList		emailList;
		internal	ArrayList		nameList;
		internal	ArrayList		phoneList;
		private		Stream			photoStream = null;

		// Map for keeping track of what properties actually changed
		//private		ChangeMap		propertyChangeMap = 0;

		#endregion

		#region Properties
		/// <summary>
		/// FN - Specifies the formatted text corresponding to the name of
		/// the object this contact represents.  This type is based on the semantics
		/// of the X.520 Common Name attribute.
		///
		/// Type Value: A read-only single text value
		///
		/// Example: FN:Mr. John Q. Public\, Esq.
		/// Note: The FN property is built from the preferred Name attribute 
		/// // which is a complex attribute.
		///
		/// </summary>
		public string FN
		{
			get
			{
				try
				{
					// FIXUP
					//return(this.GetFullName());
					return("");
				}
				catch
				{
					return("");
				}
			}
		}

		/// <summary>
		/// UserName -
		/// !NOTE! Doc incomplete
		/// </summary>
		public string UserName
		{
			get
			{
				try
				{
					return(this.Name);
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Name = value;
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Title -
		/// !NOTE! Doc incomplete
		/// </summary>
		public string Title
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(Common.titleProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(Common.titleProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.titleProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Role -
		/// !NOTE! Doc incomplete
		/// </summary>
		public string Role
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(Common.roleProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(Common.roleProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.roleProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Nickname -
		/// !NOTE! Doc incomplete
		/// </summary>
		public string Nickname
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(Common.nicknameProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(Common.nicknameProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.nicknameProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Birthday: Specifies the birth date of the object this contact represents.
		///
		/// Type Value: Single date value
		///
		/// Example: Birthday:1961-12-19
		///
		/// </summary>
		public string Birthday
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(Common.birthdayProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(Common.birthdayProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.birthdayProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// EMail: Specifies the preferred e-mail address for this contact
		/// Helper property
		/// If the caller performs a set the default types will be
		/// added to the e-mail record (association=work, type=internet)
		///
		/// Type Value: Single text value
		///
		/// Example: banderso@novell.com
		///
		/// </summary>
		public string EMail
		{
			get
			{
				try
				{
					return(GetPreferredEmailAddress().Address);
				}
				catch{}
				return("");
			}

			set
			{
				try
				{ 
					Email tmpMail = new Email((EmailTypes.internet | EmailTypes.work), value);
					tmpMail.Preferred = true;
					this.AddEmailAddress(tmpMail);
				}
				catch{}
			}
		}

		/// <summary>
		/// ProductID: Specifies the product that created the contact/vCard
		///
		/// Type Value: Single text value
		///
		/// Example: Novell.Address.Book
		///
		/// </summary>
		public string ProductID
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(Common.productIDProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(Common.productIDProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.productIDProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Url: Specifies the default url to the contact's home page
		///
		/// Type Value: Single text value
		///
		/// Example: http://www.eatatjoes.com
		///
		/// </summary>
		public string Url
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(Common.urlProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(Common.urlProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.urlProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// NOTE: Specifies the default url to the contact's home page
		///
		/// Type Value: single value - text
		///
		/// Example: blah, blah, blah...
		///
		/// </summary>
		public string Note
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(Common.noteProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(Common.noteProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.noteProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// NOTE: Contact's work force ID
		///
		/// Type Value: single value - text
		///
		/// Example: 2480
		///
		/// </summary>
		public string WorkForceID
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(Common.workForceIDProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(Common.workForceIDProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.workForceIDProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// NOTE: Manager's work force ID
		///
		/// Type Value: single value - text
		///
		/// Example: 2480
		///
		/// </summary>
		public string ManagerID
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(Common.managerIDProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(Common.managerIDProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.managerIDProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Blog: Specifies the contact's blog url
		///
		/// Type Value: Single text value
		///
		/// Example: http://johndoe.blogdomain.com
		///
		/// </summary>
		public string Blog
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(Common.blogProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(Common.blogProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.blogProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Organization: Specifies the organization the contact belongs to
		///
		/// Type Value: Single text value
		///
		/// Example: finance;provo;ut;somecompany
		///
		/// </summary>
		public string Organization
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(Common.organizationProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(Common.organizationProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.organizationProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Version: Specifies the vCard version of schema used for this contact
		///
		/// Type Value: Single text value
		///
		/// Example: 3.0
		///
		/// </summary>
		public string Version
		{
			get { return(Common.vcardSchemaVersion); }
		}

		/// <summary>
		/// IsCurrentUser -
		/// !NOTE! Doc incomplete
		/// </summary>
		public bool IsCurrentUser
		{
			get
			{
				try
				{
					// FIXUP
					/*
					if (this.addressBook.store.CurrentUser == this.ID)
					{
						return(true);
					}
					*/

					return(true);
				}
				catch{}
				return(false);
			}
		}
		#endregion

		#region Constructors

		internal Contact(AddressBook addressBook, Node cNode) : base(cNode)
		{
			this.addressBook = addressBook;
			this.addressList = new ArrayList();
			this.nameList = new ArrayList();
			this.emailList = new ArrayList();
			this.phoneList = new ArrayList();

			this.ToObject();
		}

		/// <summary>
		/// Simple Contact constructor
		/// </summary>
		public Contact() : base("")
		{
			this.addressList = new ArrayList();
			this.nameList = new ArrayList();
			this.emailList = new ArrayList();
			this.phoneList = new ArrayList();
		}

		#endregion

		#region Private Methods

		internal void Add(AddressBook addressBook)
		{
			try
			{
				this.addressBook = addressBook;
				if (this.Name != null && this.Name != "")
				{
					// Add a relationship that will reference the parent Node.
					Relationship parentChild = new 
						Relationship( addressBook.collection.ID, this.ID );
					this.Properties.AddProperty( Common.contactToAddressBook, parentChild );

					// If any Name objects were added to the contact before it was attached
					// to an address book, we'll need to create relationships for them as well
					if (this.nameList.Count > 0)
					{
						foreach(Name cName in this.nameList)
						{
							Relationship nameAndContact = new 
								Relationship(addressBook.collection.ID, this.ID );
							cName.Properties.AddProperty( Common.nameToContact, nameAndContact );
						}
					}

					// If any Address objects were added to the contact before it was attached
					// to an address book, we'll need to create relationships for them as well
					if (this.addressList.Count > 0)
					{
						foreach(Address cAddress in this.addressList)
						{
							Relationship addressAndContact = new 
								Relationship(addressBook.collection.ID, this.ID );
							cAddress.Properties.AddProperty( Common.addressToContact, addressAndContact );
						}
					}
					return;
				}
			}
			catch{}
			throw new ApplicationException(Common.addressBookExceptionHeader + "Contact - missing mandatory properties");
		}

		private	string GetFullName()
		{
			Name	tmpName = null;
			string fn = "";

			try
			{
				tmpName = this.GetPreferredName();
			}
			catch
			{
				return("");
			}

			if (tmpName != null)
			{
				fn = tmpName.FN;
			}

			return(fn);
		}

		/// <summary>
		/// Retrieves a contact from the collection store and deserializes
		/// to the properties in the contact object.
		/// </summary>
		/// <param name="parentCollection">Parent collection for this contact</param>
		/// <param name="contactID">The node ID of contact to retrieve</param>
		/// <remarks>
		/// An exception is thrown if the contact can't be find by the 
		/// specified ID
		/// </remarks>
		/// <returns>None</returns>
		internal void ToObject()
		{
			try
			{
				// Load up email addresses
				this.emailList.Clear();
				MultiValuedList	mList = this.Properties.GetProperties(Common.emailProperty);
				foreach(Property p in mList)
				{
					this.emailList.Add(new Email(this, (string) p.Value));
				}
			}
			catch{}

			try
			{
				// Load up telephone numbers
				this.phoneList.Clear();
				MultiValuedList	mList = this.Properties.GetProperties(Common.phoneProperty);
				foreach(Property p in mList)
				{
					this.phoneList.Add(new Telephone(this, (string) p.Value));
				}
			}
			catch{}

			try
			{
				// Load up any names
				this.nameList.Clear();
				Relationship parentChild = 
					new Relationship( this.addressBook.collection.ID, this.ID );
				ICSList results = 
					this.addressBook.collection.Search( Common.nameToContact, parentChild );
				foreach ( ShallowNode cShallow in results )
				{
					Node cNode = new Node(this.addressBook.collection, cShallow);
					if (this.addressBook.collection.IsType(cNode, Common.nameProperty) == true)
					{
						this.nameList.Add(new Name(this, cNode));
					}
				}
			}
			catch{}

			try
			{
				// Load up the addresses
				this.addressList.Clear();
				Relationship parentChild = 
					new Relationship( this.addressBook.collection.ID, this.ID );
				ICSList results = 
					this.addressBook.collection.Search( Common.addressToContact, parentChild );
				foreach ( ShallowNode cShallow in results )
				{
					Node cNode = new Node(this.addressBook.collection, cShallow);
					if (this.addressBook.collection.IsType(cNode, Common.addressProperty) == true)
					{
						this.addressList.Add(new Address(this, cNode));
					}
				}
			}
			catch{}
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Add an e-mail address to this contact.
		/// </summary>
		/// <param name="email">Email object</param>
		/// <remarks>
		/// e-mail consists of the e-mail name itself and the type of e-mail
		/// such as: "work", "personal" etc.
		/// 
		/// Each contact may have multiple e-mail addresses but only one can be preferred.
		/// 
		/// If for any reason the e-mail object cannot be added to the contact
		/// an exception will be raised.
		/// </remarks>
		/// <returns>A Name object with valid given and family properties.</returns>

		public void AddEmailAddress(Email email)
		{
			try
			{
				email.Add(this);
			}
			catch{}
			return;
		}

		/// <summary>
		/// Gets all of the e-mail addresses added to this contact
		/// </summary>
		/// <returns>An IABList object that contains the e-mail addresses.</returns>
		public IABList GetEmailAddresses()
		{
			IABList cList = new IABList();

			try
			{
				foreach(Email tmpMail in this.emailList)
				{
					cList.Add(tmpMail);
				}
			}
			catch{}
			return(cList);
		}

		/// <summary>
		/// Gets the preferred e-mail address
		/// </summary>
		/// <returns>The preferred Email object.</returns>
		public Email GetPreferredEmailAddress()
		{
			foreach(Email tmpMail in this.emailList)
			{
				if(tmpMail.Preferred == true)
				{
					return(tmpMail);
				}
			}

			return(null);
			//throw new ApplicationException(Common.addressBookExceptionHeader + "A preferred email address does not exist");
		}

		/// <summary>
		/// Add an telephone number to this contact.
		/// </summary>
		/// <param name="telephone">Telephone object</param>
		/// <remarks>
		/// Telephone consists of the number itself and the type of phone
		/// such as: "WORK", "HOME" etc.
		/// 
		/// Each contact may have multiple telephone numbers but only one can be preferred.
		/// 
		/// If for any reason the telephone object cannot be added to the contact
		/// an exception will be raised.
		/// </remarks>
		public void AddTelephoneNumber(Telephone telephone)
		{
			try
			{
				telephone.Add(this);
			}
			catch{}
			return;
		}

		/// <summary>
		/// Gets all Telephone objects attached to this contact.
		/// </summary>
		/// <returns>A list of Telephone objects.</returns>
		public IABList GetTelephoneNumbers()
		{
			IABList cList = new IABList();

			try
			{
				foreach(Telephone phone in this.phoneList)
				{
					cList.Add(phone);
				}
			}
			catch{}
			return(cList);
		}

		/// <summary>
		/// Gets the preferred telephone number
		/// If a preferred telephone number does not exist a null 
		/// is returned.
		/// </summary>
		/// <returns>The preferred Telephone object.</returns>
		public Telephone GetPreferredTelephoneNumber()
		{
			foreach(Telephone tmpNumber in this.GetTelephoneNumbers())
			{
				if(tmpNumber.Preferred == true)
				{
					return(tmpNumber);
				}
			}

			return(null);
		}

		/// <summary>
		/// Commits any changes to the Contact object.
		/// </summary>
		public void Commit()
		{
			if (this.addressBook != null &&
				this.addressBook.collection != null)
			{
				this.addressBook.collection.SetType(this, Common.contactType);

				int nodesToCommit = 1 + this.nameList.Count + this.addressList.Count;

				Node[] commitList = new Node[nodesToCommit];
				int	i = 0;
				commitList[i++] = this;

				if (this.emailList.Count > 0)
				{
					Email.PersistToStore(this);
				}

				if (this.phoneList.Count > 0)
				{
					Telephone.PersistToStore(this);
				}

				if (this.nameList.Count > 0)
				{
					Novell.AddressBook.Name.PrepareToCommit(this);
					foreach(Name cName in this.nameList)
					{
						this.addressBook.collection.SetType(cName, Common.nameProperty);
						commitList[i++] = cName;
					}
				}

				if (this.addressList.Count > 0)
				{
					Novell.AddressBook.Address.PrepareToCommit(this);
					foreach(Address cAddr in this.addressList)
					{
						this.addressBook.collection.SetType(cAddr, Common.addressProperty);
						commitList[i++] = cAddr;
					}
				}

				this.addressBook.collection.Commit(commitList);
			}
		}

		/// <summary>
		/// Deletes the current Contact object.
		/// </summary>
		public void Delete()
		{
			try
			{
				// First delete all child nodes
				if (this.nameList.Count > 0)
				{
					foreach(Name cName in this.nameList)
					{
						cName.Delete();
					}

					this.nameList.Clear();
				}

				if (this.addressList.Count > 0)
				{
					foreach(Address cAddress in this.addressList)
					{
						cAddress.Delete();
					}

					this.addressList.Clear();
				}

				this.Delete();
			}
			catch{}
		}

		/// <summary>
		/// Adds a vCard ADR property to the contact.
		/// </summary>
		/// <remarks>
		/// vCard ADR is a structured property consisting of:
		/// Street, Region, Locality, Country and Extended Address
		/// 
		/// Each contact may have one preferred ADR.
		/// 
		/// If for any reason the ADR cannot be created an exception is raised.
		/// </remarks>
		public void AddAddress(Address addr)
		{
			try
			{
				addr.Add(this);
				return;
			}
			catch{}
			throw new ApplicationException(Common.addressBookExceptionHeader + "Failed adding Address");
		}

		/// <summary>
		/// Gets a list of addresses attached to this contact.
		/// </summary>
		public IABList GetAddresses()
		{
			IABList cList = new IABList();

			try
			{
				foreach(Address addr in this.addressList)
				{
					cList.Add(addr);
				}
			}
			catch{}
			return(cList);
		}

		/// <summary>
		/// Gets the preferred address for this contact.
		/// </summary>
		public Address GetPreferredAddress()
		{
			foreach(Address addr in this.addressList)
			{
				if (addr.Preferred == true)
				{
					return(addr);
				}
			}

			throw new ApplicationException(Common.addressBookExceptionHeader + "Preferred \"Address\" was not found" );
		}

		/// <summary>
		/// Deletes an Address record in a contact.
		/// A contact may contain 0 to many address records
		/// </summary>
		public void DeleteAddress(string addressID)
		{
			try
			{
				foreach(Address addr in this.addressList)
				{
					if (addr.ID == addressID)
					{
						addr.Delete();
						return;
					}
				}
			}
			catch{}
			throw new ApplicationException(Common.addressBookExceptionHeader + "Address " + addressID + " not found" );
		}

		/// <summary>
		/// Retrieve a vCard ADR property based on an address ID.
		/// </summary>
		/// <param name="addressID">Address ID</param>
		/// <remarks>
		/// vCard Address is a structured property consisting of:
		/// Post Office Box, Extended Address, Street Address, Locality, 
		/// Region, Postal Code and Country
		/// 
		/// Each contact may have one preferred Address.
		/// 
		/// If for any reason the Address can't be retrieved an an exception is raised.
		/// </remarks>
		/// <returns>An Address object with a valid postal code property.</returns>
		public Address GetAddress(string addressID)
		{
			try
			{
				foreach(Address cAddress in this.addressList)
				{
					if (cAddress.ID == addressID)
					{
						return(cAddress);
					}
				}
			}
			catch{}
			return(null);
			//throw new ApplicationException( Common.addressBookExceptionHeader +  "Address " + addressID + " not found" );
		}

		/// <summary>
		/// Adds a vCard Name property to the contact.
		/// </summary>
		/// <remarks>
		/// vCard Name is a structured property consisting of:
		/// Family Name, Given Name, Additional Names, Honorific Prefixes
		/// and Honorific Suffixes
		/// 
		/// Each contact may have one preferred Name.
		/// 
		/// If for any reason the Name cannot be created an exception is raised.
		/// </remarks>
		public void AddName(Name name)
		{
			try
			{
				name.Add(this);
				return;
			}
			catch{}
			throw new ApplicationException(Common.addressBookExceptionHeader + "Name object not added");
		}

		/// <summary>
		/// Gets a Name object by specified ID.
		/// </summary>
		/// <remarks>
		/// vCard Name is a structured property consisting of:
		/// Family Name, Given Name, Additional Names, Honorific Prefixes
		/// and Honorific Suffixes
		/// 
		/// If the contact does not have a Name by the specified ID 
		/// a null is returned.
		/// </remarks>
		/// <returns>A Name object with at least given and family properties.</returns>
		public Name GetName(string nameID)
		{
			if (this.nameList.Count > 0)
			{
				foreach(Name tmpName in this.nameList)
				{
					if (tmpName.ID == nameID)
					{
						return(tmpName);
					}
				}
			}

			// For now I won't refresh from the database

			return(null);

			/*

			// go see if a name object has been
			// added underneath us
			Name cName = null;

			if (this.addressBook != null &&
				this.addressBook.collection != null)
			{
				try
				{
					Node cNode = this.addressBook.collection.GetNodeByID(nameID);
					if ( cNode != null )
					{
						if (this.addressBook.collection.IsType(cNode, Common.nameProperty) == true)
						{
							cName = new Name(this, cNode);
						}
					}
				}
				catch(Exception e)
				{
					Console.WriteLine(e.Message);
					cName = null;
				}
			}

			return(cName);
			*/
		}

		/// <summary>
		/// Gets the preferred Name for this contact.
		/// </summary>
		/// <remarks>
		/// vCard Name is a structured property consisting of:
		/// Family Name, Given Name, Additional Names, Honorific Prefixes
		/// and Honorific Suffixes
		/// 
		/// If the contact does not have a preferred Name a
		/// null is returned.
		/// </remarks>
		/// <returns>A Name object with at least valid given and family properties.</returns>
		public Name GetPreferredName()
		{
			foreach(Name cName in this.nameList)
			{
				if (cName.Preferred == true)
				{
					return(cName);
				}
			}

			return(null);
		}

		/// <summary>
		/// Gets all Name objects attached to this contact.
		/// </summary>
		/// <remarks>
		/// vCard Name is a structured property consisting of:
		/// Family Name, Given Name, Additional Names, Honorific Prefixes
		/// and Honorific Suffixes
		/// </remarks>
		/// <returns>A list of Name objects.</returns>
		public IABList GetNames()
		{
			IABList cList = new IABList();

			try
			{
				foreach(Name cName in this.nameList)
				{
					cList.Add(cName);
				}
			}
			catch{}
			return(cList);
		}

		/*
		/// <summary>
		/// Export the contact photo via a binary Stream object.
		/// </summary>
		/// <remarks>  
		/// vCard PHOTO is a single valued binary property
		/// 
		/// An exception is raised if the contact does not contain
		/// a photo property.
		/// 
		/// NOTE: The caller is expected to close the returned 
		/// stream object when finished.
		/// </remarks>
		/// <returns>A binary stream object which the caller can read from.</returns>
		public Stream ExportPhoto()
		{
			if (this.thisNode != null)
			{

				try
				{
					foreach(NodeStream nodeStream in this.thisNode.GetStreamList())
					{
						if (nodeStream.Name == this.thisNode.Id + Common.photoProperty)
						{
							return(nodeStream.Open(FileMode.Open, FileAccess.Read));
						}
					}
				}
				catch{}
				throw new ApplicationException(Common.addressBookExceptionHeader + "Photo property does not exist");
			}
			else
			{
				if(this.photoStream != null)
				{
					return(this.photoStream);
				}

				throw new ApplicationException(Common.addressBookExceptionHeader + "Photo property does not exist");
			}
		}

		/// <summary>
		/// Imports a photo from a a file name.
		/// </summary>
		/// <param name="fileName">Source Filename</param>
		/// <remarks>  
		/// vCard PHOTO is a single valued binary property
		/// </remarks>
		/// <returns>true if the photo was successfully imported.</returns>
		public bool ImportPhoto(string fileName)
		{
			bool	results = false;
			Stream	srcStream = null;

			try
			{
				srcStream = new FileStream(fileName, FileMode.Open);
				results = this.ImportPhoto(srcStream);
				// BUGBUG store the source file name in the store - where it came from
			}
			catch{}
			finally
			{
				if (srcStream != null)
				{
					srcStream.Close();
				}
			}

			return(results);
		}

		/// <summary>
		/// Imports a photo from a stream object.
		/// </summary>
		/// <param name="srcStream">Source Stream</param>
		/// <remarks>  
		/// vCard PHOTO is a single valued binary property
		/// </remarks>
		/// <returns>true if the photo was successfully imported.</returns>
		public bool	ImportPhoto(Stream srcStream)
		{
			bool			finished = false;
			BinaryReader	bReader = null;
			BinaryWriter	bWriter = null;
			NodeStream		photoStream = null;
			Stream			dstStream = null;

			if (this.thisNode != null)
			{
				// See if a photo stream already exists for this contact node.
				// If one is found - delete it
				ICSList searchResults = this.thisNode.GetStreamList();

				try
				{
					foreach(NodeStream nodeStream in searchResults)
					{
						if (nodeStream.Type == Common.photoProperty)
						{
							nodeStream.Delete(true);
							break;
						}
					}
				}
				catch{};

				try
				{
					photoStream = 
						this.thisNode.AddStream(
							this.thisNode.Id + Common.photoProperty, 
							Common.photoProperty,
							this.thisNode.Id);

					// Create the new stream in the file system
					dstStream = photoStream.Open(FileMode.Create, FileAccess.ReadWrite);
					bWriter = new BinaryWriter(dstStream);

					// Copy the source stream
					bReader = new BinaryReader(srcStream);
					bReader.BaseStream.Position = 0;
					bWriter.BaseStream.Position = 0;

					//bWriter.Write(bReader.BaseStream, 0, bReader.BaseStream.Length);
					//bWriter.Write(binaryData, 0, binaryData.Length);

					// BUGBUG better algo for copying
					int i = 0;
					while(true)
					{
						i = bReader.BaseStream.ReadByte();
						if(i == -1)
						{
							break;
						}

						bWriter.BaseStream.WriteByte((byte) i);
					}

					finished = true;
				}
				catch{}
				finally
				{
					if (bReader != null)
					{
						bReader.Close();
					}

					if (bWriter != null)
					{
						bWriter.Close();
					}

					if (dstStream != null)
					{
						dstStream.Close();
					}

					if (photoStream != null && finished == true)
					{
						//photoStream.Save();
						//this.thisNode.Save();
						this.addressBook.collection.Commit(true);
					}
				}
			}
			else
			{
				// Copy the photo into the cached stream
				try
				{
					// Create the new stream in the file system
					this.photoStream = new MemoryStream();
					bWriter = new BinaryWriter(this.photoStream);

					// Copy the source stream
					bReader = new BinaryReader(srcStream);
					bReader.BaseStream.Position = 0;
					bWriter.BaseStream.Position = 0;

					//bWriter.Write(bReader.BaseStream, 0, bReader.BaseStream.Length);
					
					// BUGBUG better algo for copying
					int i = 0;
					while(true)
					{
						i = bReader.BaseStream.ReadByte();
						if(i == -1)
						{
							break;
						}

						bWriter.BaseStream.WriteByte((byte) i);
					}

					bWriter.BaseStream.Position = 0;
					finished = true;
				}
				catch{}
				finally
				{
					if (bReader != null)
					{
						bReader.Close();
					}

					//if (bWriter != null)
					//{
					//	bWriter.Close();
					//}
				}
			}

			return(finished);
		}
		*/

		/*
		/// <summary>
		/// Export a standard vCard to the specified file.
		/// </summary>
		/// <param name="filePath">Source Stream</param>
		/// <remarks>  
		/// Export the contact to the specified file in standard vCard format
		/// </remarks>
		/// <returns>true if the vCard was successfully exported.</returns>
		public bool ExportVCard(string filePath)
		{
			bool	results = false;
			Stream	dstCard = null;
			Stream	srcCard = null;

			try
			{
				dstCard = new FileStream(filePath, FileMode.Create);
				if (dstCard != null)
				{
					ExportVCard(dstCard);
					dstCard.Flush();

					//
					//srcCard = ExportVCard();

					//int i;
					//while(true)
					//{
					//	i = srcCard.ReadByte();
					//	if(i == -1)
					//	{
					//		break;
					//	}

					//	dstCard.WriteByte((byte) i);
					//}
					//
				}
			}
			catch{}
			finally
			{
				if (dstCard != null)
				{
					dstCard.Close();
				}

				if (srcCard != null)
				{
					srcCard.Close();
				}
			}

			return(results);
		}
		*/

		/*
		/// <summary>
		/// Export a standard vCard via a memory stream.
		/// </summary>
		/// <remarks>  
		/// Export the contact in standard vCard format to a memory stream.
		/// </remarks>
		/// <returns>Memory stream which can be read from</returns>
		public void ExportVCard(Stream vCard)
		{
			//MemoryStream	vCard = new MemoryStream();
			const string	beginHdr = "BEGIN:VCARD";
			const string	versionHdr = "VERSION:3.0";
			const string	endHdr = "END:VCARD";
			const string	emailHdr = "EMAIL";
			const string	prefHdr = "PREF";
			const string	homeHdr = "HOME";
			const string	workHdr = "WORK";
			const string	photoHdr = "PHOTO;ENCODING=b;";
			const string	jpegHdr = "TYPE=JPEG:";
			const string	prodidHdr = "PRODID:";
			const string	urlHdr = "URL:";
			const string	bdayHdr = "BDAY:";
			const string	noteHdr = "NOTE:";
			const string	uuidHdr = "UID:";
			const string	usernameHdr = "X-NAB-USERNAME:";
			const string	blogHdr = "X-NAB-BLOG:";
			const string	uidHdr = "X-NAB-UID:";
			const string	typeHdr = "TYPE=";

			foreach(char c in beginHdr)
			{
				vCard.WriteByte(Convert.ToByte(c));
			}

			vCard.WriteByte(0xD);
			vCard.WriteByte(0xA);

			foreach(char c in versionHdr)
			{
				vCard.WriteByte(Convert.ToByte(c));
			}

			vCard.WriteByte(0xD);
			vCard.WriteByte(0xA);

			// Convert PRODID
			try
			{
				if (this.ProductID != null && this.ProductID != "")
				{
					foreach(char c in prodidHdr)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}
					
					foreach(char c in this.ProductID)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}

					vCard.WriteByte(0xD);
					vCard.WriteByte(0xA);
				}
			}
			catch{}

			// Convert UUID
			try
			{
				if (this.ID != null && this.ID != "")
				{
					foreach(char c in uuidHdr)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}
					
					foreach(char c in this.ID)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}

					vCard.WriteByte(0xD);
					vCard.WriteByte(0xA);
					
					// Novell Address Book UID
					foreach(char c in uidHdr)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}
					
					foreach(char c in this.ID)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}

					vCard.WriteByte(0xD);
					vCard.WriteByte(0xA);
				}
			}
			catch{}

			// Convert UserName
			try
			{
				if (this.UserName != "")
				{
					foreach(char c in usernameHdr)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}
					
					foreach(char c in this.UserName)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}

					vCard.WriteByte(0xD);
					vCard.WriteByte(0xA);
				}
			}
			catch{}

			// Convert the preferred name
			try
			{
				Name prefName = this.GetPreferredName();
				if (prefName != null)
				{
					vCard.WriteByte(Convert.ToByte('N'));
					vCard.WriteByte(Convert.ToByte(':'));

					if (prefName.Family != null && prefName.Family != "")
					{
						foreach(char c in prefName.Family)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}

					vCard.WriteByte(Convert.ToByte(';'));


					if (prefName.Given != null && prefName.Given != "")
					{
						foreach(char c in prefName.Given)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}

					vCard.WriteByte(Convert.ToByte(';'));

					vCard.WriteByte(Convert.ToByte(';'));
					vCard.WriteByte(0xD);
					vCard.WriteByte(0xA);
				}
			}
			catch{}

			// Write out the Full Name (FN) portion of the vCard
			string fullName = this.FN;
			if (fullName != null && fullName != "")
			{
				vCard.WriteByte((byte) 'F');
				vCard.WriteByte(Convert.ToByte('N'));
				vCard.WriteByte(Convert.ToByte(':'));

				foreach(char c in fullName)
				{
					vCard.WriteByte(Convert.ToByte(c));
				}

				vCard.WriteByte(0xD);
				vCard.WriteByte(0xA);
			}

			// Write out the organization
			if (this.Organization != "")
			{
				vCard.WriteByte((byte) 'O');
				vCard.WriteByte((byte) 'R');
				vCard.WriteByte((byte) 'G');
				vCard.WriteByte((byte) ':');

				foreach(char c in this.Organization)
				{
					vCard.WriteByte((byte) c);
				}

				vCard.WriteByte(0xD);
				vCard.WriteByte(0xA);
			}

			// Write out the title
			if (this.Title != "")
			{
				vCard.WriteByte((byte) 'T');
				vCard.WriteByte((byte) 'I');
				vCard.WriteByte((byte) 'T');
				vCard.WriteByte((byte) 'L');
				vCard.WriteByte((byte) 'E');
				vCard.WriteByte((byte) ':');

				foreach(char c in this.Title)
				{
					vCard.WriteByte((byte) c);
				}

				vCard.WriteByte(0xD);
				vCard.WriteByte(0xA);
			}

			// Write out the role
			if (this.Role != "")
			{
				vCard.WriteByte((byte) 'R');
				vCard.WriteByte((byte) 'O');
				vCard.WriteByte((byte) 'L');
				vCard.WriteByte((byte) 'E');
				vCard.WriteByte((byte) ':');

				foreach(char c in this.Role)
				{
					vCard.WriteByte((byte) c);
				}

				vCard.WriteByte(0xD);
				vCard.WriteByte(0xA);
			}

			// Write out birthday
			if (this.Birthday != "")
			{
				foreach(char c in bdayHdr)
				{
					vCard.WriteByte(Convert.ToByte(c));
				}

				foreach(char c in this.Birthday)
				{
					vCard.WriteByte((byte) c);
				}

				vCard.WriteByte(0xD);
				vCard.WriteByte(0xA);
			}

			// Write out the web page url
			try
			{
				if (this.Url != "")
				{
					foreach(char c in urlHdr)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}
					
					foreach(char c in this.Url)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}

					vCard.WriteByte(0xD);
					vCard.WriteByte(0xA);
				}
			}
			catch{}

			// Write out the blog url
			try
			{
				if (this.Blog != "")
				{
					foreach(char c in blogHdr)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}
					
					foreach(char c in this.Blog)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}

					vCard.WriteByte(0xD);
					vCard.WriteByte(0xA);
				}
			}
			catch{}

			// Write out all addresses attached to this contact
			try
			{
				IABList addrEnum = this.GetAddresses();
				foreach(Address tmpAddress in addrEnum)
				{
					vCard.WriteByte(Convert.ToByte('A'));
					vCard.WriteByte(Convert.ToByte('D'));
					vCard.WriteByte(Convert.ToByte('R'));

					if (tmpAddress.Preferred == true)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						foreach( char c in prefHdr)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}

					if ((tmpAddress.Types & AddressTypes.home) == AddressTypes.home)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						foreach( char c in homeHdr)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}

					if ((tmpAddress.Types & AddressTypes.work) == AddressTypes.work)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						foreach( char c in workHdr)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}

					if ((tmpAddress.Types & AddressTypes.dom) == AddressTypes.dom)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						vCard.WriteByte(Convert.ToByte('D'));
						vCard.WriteByte(Convert.ToByte('O'));
						vCard.WriteByte(Convert.ToByte('M'));
					}

					if ((tmpAddress.Types & AddressTypes.intl) == AddressTypes.intl)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						vCard.WriteByte(Convert.ToByte('I'));
						vCard.WriteByte(Convert.ToByte('N'));
						vCard.WriteByte(Convert.ToByte('T'));
						vCard.WriteByte(Convert.ToByte('L'));
					}

					if ((tmpAddress.Types & AddressTypes.parcel) == AddressTypes.parcel)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						vCard.WriteByte(Convert.ToByte('P'));
						vCard.WriteByte(Convert.ToByte('A'));
						vCard.WriteByte(Convert.ToByte('R'));
						vCard.WriteByte(Convert.ToByte('C'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('L'));
					}

					if ((tmpAddress.Types & AddressTypes.postal) == AddressTypes.postal)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						vCard.WriteByte(Convert.ToByte('P'));
						vCard.WriteByte(Convert.ToByte('O'));
						vCard.WriteByte(Convert.ToByte('S'));
						vCard.WriteByte(Convert.ToByte('T'));
						vCard.WriteByte(Convert.ToByte('A'));
						vCard.WriteByte(Convert.ToByte('L'));
					}

					vCard.WriteByte(Convert.ToByte(':'));

					if (tmpAddress.PostalBox != "")
					{
						foreach(char c in tmpAddress.PostalBox)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}

					}
					vCard.WriteByte(Convert.ToByte(';'));

					if (tmpAddress.ExtendedAddress != "")
					{
						foreach(char c in tmpAddress.ExtendedAddress)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}
					vCard.WriteByte(Convert.ToByte(';'));

					if (tmpAddress.Street != "")
					{
						foreach(char c in tmpAddress.Street)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}
					vCard.WriteByte(Convert.ToByte(';'));

					if (tmpAddress.Locality != "")
					{
						foreach(char c in tmpAddress.Locality)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}
					vCard.WriteByte(Convert.ToByte(';'));

					if (tmpAddress.Region != "")
					{
						foreach(char c in tmpAddress.Region)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}
					vCard.WriteByte(Convert.ToByte(';'));

					if (tmpAddress.PostalCode != "")
					{
						foreach(char c in tmpAddress.PostalCode)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}
					vCard.WriteByte(Convert.ToByte(';'));

					if (tmpAddress.Country != null && tmpAddress.Country != "")
					{
						foreach(char c in tmpAddress.Country)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}

					vCard.WriteByte(0xD);
					vCard.WriteByte(0xA);
				}
			}
			catch{}

			// Write out the email addresses
			try
			{
				IABList mailEnum = this.GetEmailAddresses();

				foreach(Email tmpMail in mailEnum)
				{
					foreach(char c in emailHdr)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}

					//if (tmpMail.Preferred == true || tmpMail.Types > 0)
					//{
					//	vCard.WriteByte(Convert.ToByte(';'));
					//}

					if (tmpMail.Preferred == true)
					{
						vCard.WriteByte(Convert.ToByte(';'));
						
						foreach(char c in typeHdr)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
						
						foreach(char c in prefHdr)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}

					if ((tmpMail.Types & EmailTypes.internet) == EmailTypes.internet)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						vCard.WriteByte(Convert.ToByte('I'));
						vCard.WriteByte(Convert.ToByte('N'));
						vCard.WriteByte(Convert.ToByte('T'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('R'));
						vCard.WriteByte(Convert.ToByte('N'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('T'));
					}

					if ((tmpMail.Types & EmailTypes.x400) == EmailTypes.x400)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						vCard.WriteByte(Convert.ToByte('X'));
						vCard.WriteByte(Convert.ToByte('4'));
						vCard.WriteByte(Convert.ToByte('0'));
						vCard.WriteByte(Convert.ToByte('0'));
					}

					if ((tmpMail.Types & EmailTypes.other) == EmailTypes.other)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						foreach(char c in typeHdr)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}

						vCard.WriteByte(Convert.ToByte('O'));
						vCard.WriteByte(Convert.ToByte('T'));
						vCard.WriteByte(Convert.ToByte('H'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('R'));
					}


					if ((tmpMail.Types & EmailTypes.work) == EmailTypes.work)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						foreach(char c in typeHdr)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}

						vCard.WriteByte(Convert.ToByte('W'));
						vCard.WriteByte(Convert.ToByte('O'));
						vCard.WriteByte(Convert.ToByte('R'));
						vCard.WriteByte(Convert.ToByte('K'));
					}

					if ((tmpMail.Types & EmailTypes.personal) == EmailTypes.personal)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						foreach(char c in typeHdr)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}

						vCard.WriteByte(Convert.ToByte('H'));
						vCard.WriteByte(Convert.ToByte('O'));
						vCard.WriteByte(Convert.ToByte('M'));
						vCard.WriteByte(Convert.ToByte('E'));
					}


					// Now the actual e-mail address
					vCard.WriteByte(Convert.ToByte(':'));
					foreach(char c in tmpMail.Address)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}

					vCard.WriteByte(0xD);
					vCard.WriteByte(0xA);
				}
			}
			catch{}


			// Write out the NOTE into the stream
			try
			{
				if (this.Note != null && this.Note != "")
				{
					foreach(char c in noteHdr)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}

					foreach(char c in this.Note)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}

					vCard.WriteByte(0xD);
					vCard.WriteByte(0xA);

				}
			}
			catch{}

			// Write out all telephone numbers attached to this contact
			try
			{
				PhoneTypes tmpTypes;
				IABList phoneEnum = this.GetTelephoneNumbers();
				foreach(Telephone tmpPhone in phoneEnum)
				{
					tmpTypes = tmpPhone.Types;

					vCard.WriteByte(Convert.ToByte('T'));
					vCard.WriteByte(Convert.ToByte('E'));
					vCard.WriteByte(Convert.ToByte('L'));

					if (tmpPhone.Preferred == true)
					{
						vCard.WriteByte(Convert.ToByte(';'));
						
						foreach(char c in typeHdr)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}

						foreach( char c in prefHdr)
						{
							vCard.WriteByte((byte) c);
						}
					}

					if ((tmpPhone.Types & PhoneTypes.work) == PhoneTypes.work)
					{
						vCard.WriteByte(Convert.ToByte(';'));
						
						foreach(char c in typeHdr)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
						
						foreach(char c in workHdr)
						{
							vCard.WriteByte((byte) c);
						}
					}

					if ((tmpPhone.Types & PhoneTypes.home) == PhoneTypes.home)
					{
						vCard.WriteByte(Convert.ToByte(';'));
						
						foreach(char c in typeHdr)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}

						foreach(char c in homeHdr)
						{
							vCard.WriteByte((byte) c);
						}
					}

					if ((tmpPhone.Types & PhoneTypes.voice) == PhoneTypes.voice)
					{
						vCard.WriteByte(Convert.ToByte(';'));
					
						vCard.WriteByte((byte) 'V');
						vCard.WriteByte((byte) 'O');
						vCard.WriteByte((byte) 'I');
						vCard.WriteByte((byte) 'C');
						vCard.WriteByte((byte) 'E');
					}

					if ((tmpPhone.Types & PhoneTypes.msg) == PhoneTypes.msg)
					{
						vCard.WriteByte(Convert.ToByte(';'));
					
						vCard.WriteByte(Convert.ToByte('M'));
						vCard.WriteByte(Convert.ToByte('S'));
						vCard.WriteByte(Convert.ToByte('G'));
					}

					if ((tmpPhone.Types & PhoneTypes.cell) == PhoneTypes.cell)
					{
						vCard.WriteByte(Convert.ToByte(';'));
	
						foreach(char c in typeHdr)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
				
						vCard.WriteByte(Convert.ToByte('C'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('L'));
						vCard.WriteByte(Convert.ToByte('L'));
					}

					if ((tmpPhone.Types & PhoneTypes.bbs) == PhoneTypes.bbs)
					{
						vCard.WriteByte(Convert.ToByte(';'));
						vCard.WriteByte(Convert.ToByte('B'));
						vCard.WriteByte(Convert.ToByte('B'));
						vCard.WriteByte(Convert.ToByte('S'));
					}

					if ((tmpPhone.Types & PhoneTypes.car) == PhoneTypes.car)
					{
						vCard.WriteByte(Convert.ToByte(';'));
					
						vCard.WriteByte(Convert.ToByte('C'));
						vCard.WriteByte(Convert.ToByte('A'));
						vCard.WriteByte(Convert.ToByte('R'));
					}

					if ((tmpPhone.Types & PhoneTypes.isdn) == PhoneTypes.isdn)
					{
						vCard.WriteByte(Convert.ToByte(';'));
						vCard.WriteByte(Convert.ToByte('I'));
						vCard.WriteByte(Convert.ToByte('S'));
						vCard.WriteByte(Convert.ToByte('D'));
						vCard.WriteByte(Convert.ToByte('N'));
					}

					if ((tmpPhone.Types & PhoneTypes.fax) == PhoneTypes.fax)
					{
						vCard.WriteByte(Convert.ToByte(';'));
						vCard.WriteByte(Convert.ToByte('F'));
						vCard.WriteByte(Convert.ToByte('A'));
						vCard.WriteByte(Convert.ToByte('X'));
					}

					if ((tmpPhone.Types & PhoneTypes.modem) == PhoneTypes.modem)
					{
						vCard.WriteByte(Convert.ToByte(';'));
						vCard.WriteByte(Convert.ToByte('M'));
						vCard.WriteByte(Convert.ToByte('O'));
						vCard.WriteByte(Convert.ToByte('D'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('M'));
					}

					if ((tmpPhone.Types & PhoneTypes.pager) == PhoneTypes.pager)
					{
						vCard.WriteByte(Convert.ToByte(';'));
						vCard.WriteByte(Convert.ToByte('P'));
						vCard.WriteByte(Convert.ToByte('A'));
						vCard.WriteByte(Convert.ToByte('G'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('R'));
					}

					if ((tmpPhone.Types & PhoneTypes.video) == PhoneTypes.video)
					{
						vCard.WriteByte(Convert.ToByte(';'));
						vCard.WriteByte(Convert.ToByte('V'));
						vCard.WriteByte(Convert.ToByte('I'));
						vCard.WriteByte(Convert.ToByte('D'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('O'));
					}

					if ((tmpPhone.Types & PhoneTypes.pcs) == PhoneTypes.pcs)
					{
						vCard.WriteByte((byte) ';');
						vCard.WriteByte((byte) 'P');
						vCard.WriteByte((byte) 'C');
						vCard.WriteByte((byte) 'S');
					}

					if ((tmpPhone.Types & PhoneTypes.voip) == PhoneTypes.voip)
					{
						vCard.WriteByte(Convert.ToByte(';'));
						vCard.WriteByte((byte) 'V');
						vCard.WriteByte((byte) 'O');
						vCard.WriteByte((byte) 'I');
						vCard.WriteByte((byte) 'P');
					}

					vCard.WriteByte(Convert.ToByte(':'));
					if (tmpPhone.Number != "")
					{
						foreach(char c in tmpPhone.Number)
						{
							vCard.WriteByte(Convert.ToByte(c));
						}
					}

					vCard.WriteByte(0xD);
					vCard.WriteByte(0xA);
				}
			}
			catch{}

			// Write out the PHOTO into the stream
			try
			{
				byte[] binaryData;
				Stream srcPhoto = this.ExportPhoto();
				if (srcPhoto != null)
				{
					binaryData = new byte[srcPhoto.Length];
					
					long bytesRead = srcPhoto.Read(binaryData, 0,(int) srcPhoto.Length);

					// Convert the binary input into Base64 UUEncoded output.
					// Each 3 byte sequence in the source data becomes a 4 byte
					// sequence in the character array. 
					long arrayLength = (long) ((4.0d/3.0d) * binaryData.Length);
    
					// If array length is not divisible by 4, go up to the next
					// multiple of 4.
					if (arrayLength % 4 != 0) 
					{
						arrayLength += 4 - arrayLength % 4;
					}
    
					char[] base64CharArray = new char[arrayLength];

					try
					{
						int bytesConverted =
							System.Convert.ToBase64CharArray(
								binaryData, 
								0,
								binaryData.Length,
								base64CharArray,
								0);

						if (bytesConverted > 0)
						{
							foreach(char c in photoHdr)
							{
								vCard.WriteByte(Convert.ToByte(c));
							}

							foreach(char c in jpegHdr)
							{
								vCard.WriteByte(Convert.ToByte(c));
							}

							vCard.WriteByte(0xD);
							vCard.WriteByte(0xA);

							for(int i = 0; i < bytesConverted; i++)
							{
								vCard.WriteByte(Convert.ToByte(base64CharArray[i]));
							}

							vCard.WriteByte(0xD);
							vCard.WriteByte(0xA);
						}
					}
					catch(Exception conv)
					{
						Console.WriteLine(conv.Message);
						throw new ApplicationException("Failed to Base64 encode photo stream");
					}
				}
			}
			catch{}

			// Write the trailer record into the stream
			foreach(char c in endHdr)
			{
				vCard.WriteByte(Convert.ToByte(c));
			}

			vCard.WriteByte(0xD);
			vCard.WriteByte(0xA);

			return;
			//vCard.Position = 0;
			//return(vCard);
		}
		*/

		/*
		public IEnumerator GetTelephoneTypes()
		{
			return(new TelephoneTypes());
		}
		*/

		#endregion

		#region IEnumerable

		/// <summary>
		/// Gets an enumerator object
		/// </summary>
		public IEnumerator GetEnumerator()
		{
			Console.WriteLine("Contact::GetEnumerator called");
			//Reset();
			//thisEnum = thisNode.GetEnumerator();
			return(this);
		}

		/*
			Implementation of the iEnumerator members
		*/

		/// <summary>
		/// Move to the next object
		/// </summary>
		public bool MoveNext()
		{
			Console.WriteLine("Contact::MoveNext called");
			return(thisEnum.MoveNext());
		}

		/// <summary>
		/// Gets the current object
		/// </summary>
		public object Current
		{
			get
			{
				Console.WriteLine("Contact::Current called");
				return(thisEnum.Current);
			}
		}

		/// <summary>
		/// Resets the enumerator so the caller can start over
		/// </summary>
		public void Reset()
		{
			Console.WriteLine("Contact::Reset called");
			thisEnum.Reset();
		}

		#endregion
	}

	/// <summary>
	/// Class used for enumerating all address objects attached to the contact
	/// </summary>
	public class AddressEnumerator : IEnumerable, IEnumerator
	{
		#region Class Members

		private	Contact		parentContact;
		private	Node		parentNode;
		private	IEnumerator	nodeEnum = null;

		#endregion

		#region Properties
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor used to instantiate this object by means of an enumerator.
		/// </summary>
		/// 
		internal AddressEnumerator(Node parentNode, Contact parentContact)
		{
			this.parentNode = parentNode;
			this.parentContact = parentContact;
			this.Reset();
		}
		#endregion

		#region IEnumerator Members

		/// <summary>
		/// Gets an enumerator object
		/// </summary>
		public IEnumerator GetEnumerator()
		{
			Reset();
			return(this);
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is the first string
		/// in the string array
		/// </summary>
		public void Reset()
		{
			//nodeEnum = this.parentNode.GetEnumerator();
		}

		/// <summary>
		/// Gets the current node contained in thisNode
		/// </summary>
		public object Current
		{
			get
			{
				Node cNode = (Node) this.nodeEnum.Current;
				//Address addr = this.parentContact.GetAddress(cNode.Id);
				//return(addr);

				// remove when we fixup addr
				return(null);
			}
		}

		/// <summary>
		/// Advances the enumerator to the next address of the contact
		/// </summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element; 
		/// false if the enumerator has passed the end of the collection.
		/// </returns>
		public bool MoveNext()
		{
			while(this.nodeEnum.MoveNext() == true)
			{
				Node tmpNode = (Node) this.nodeEnum.Current;
				if(tmpNode.Type == Common.addressProperty)
				{
					return(true);
				}
			}

			return(false);
		}
		#endregion
	}
}
