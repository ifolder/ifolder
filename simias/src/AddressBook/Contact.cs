/***********************************************************************
 *  Contact.cs - Contact class for a vCard 3.0 contact.
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
using System.IO;
using System.Collections;
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
	/// A contact is equivalent to a Node in the collection server.
	/// </summary>
	public class Contact : IEnumerable, IEnumerator
	{
		#region Class Members
		internal	Node			thisNode = null;
		internal	Collection		collection = null;
		private		AddressBook		addressBook = null;
		private		IEnumerator		thisEnum = null;
		private		string			userName = "";
		private		string			id;
		private		string			url;
		private		string			nickName;
		private		string			title;
		private		string			role;
		private		string			birthday;
		private		string			productID;
		private		string			workforceID;
		private		string			note;
		private		string			manager;
		private		string			blogUrl;
		private		string			organization;

		// Private members used for caching objects to the contact before
		// the contact has been added to an address list
		private		ArrayList		addressList = null;
		private		ArrayList		nameList = null;
		private		Stream			photoStream = null;
		internal	ArrayList		emailList = null;
		internal	ArrayList		phoneList = null;

		// Map for keeping track of what properties actually changed
		private ChangeMap		propertyChangeMap = 0;

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
					return(this.GetFullName());
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
				return(userName);
			}

			set
			{
				userName = value;
				propertyChangeMap |= ChangeMap.username;
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
				if(this.title != null)
				{
					return(this.title);
				}

				return("");
			}

			set
			{
				this.title = value;
				propertyChangeMap |= ChangeMap.title;
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
				if (this.role != null)
				{
					return(this.role);
				}

				return("");
			}

			set
			{
				this.role = value;
				propertyChangeMap |= ChangeMap.role;
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
				if (this.nickName != null)
				{
					return(this.nickName);
				}

				return("");
			}

			set
			{
				this.nickName = value;
				propertyChangeMap |= ChangeMap.nickname;
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
				if (this.birthday != null)
				{
					return(this.birthday);
				}

				return("");
			}

			set
			{
				this.birthday = value;
				propertyChangeMap |= ChangeMap.birthday;
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
				if(this.productID != null)
				{
					return(this.productID);
				}

				return("");
			}

			set
			{
				this.productID = value;
				propertyChangeMap |= ChangeMap.productid;
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
				if(this.url != null)
				{
					return(this.url);
				}

				return("");
			}

			set
			{
				this.url = value;
				propertyChangeMap |= ChangeMap.url;
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
				if (this.note != null)
				{
					return(this.note);
				}

				return("");
			}

			set
			{
				this.note = value;
				propertyChangeMap |= ChangeMap.note;
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
				if (this.workforceID != null)
				{
					return(this.workforceID);
				}

				return("");
			}

			set
			{
				this.workforceID = value;
				propertyChangeMap |= ChangeMap.workforce;
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
				if (this.manager != null)
				{
					return(this.manager);
				}

				return("");
			}

			set
			{
				this.manager = value;
				propertyChangeMap |= ChangeMap.manager;
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
				if(this.blogUrl != null)
				{
					return(this.blogUrl);
				}

				return("");
			}

			set
			{
				this.blogUrl = value;
				propertyChangeMap |= ChangeMap.blog;
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
				if(this.organization != null)
				{
					return(this.organization);
				}

				return("");
			}

			set
			{
				this.organization = value;
				propertyChangeMap |= ChangeMap.organization;
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
		/// ID -
		/// !NOTE! Doc incomplete
		/// </summary>
		public string ID
		{
			get
			{
				if(this.id != null)
				{
					return(this.id);
				}

				return("");
			}
		}

		/// <summary>
		/// IsCurrentUser -
		/// !NOTE! Doc incomplete
		/// </summary>
		public bool IsCurrentUser
		{
			get
			{
				return(Environment.UserName == UserName);
			}
		}
		#endregion

		#region Constructors
		internal Contact(Collection collection)
		{
			this.collection = collection;
		}

		internal Contact(Collection collection, string type)
		{
			this.collection = collection;
		}

		/// <summary>
		/// Simple Contact constructor
		/// </summary>
		public Contact()
		{
			this.collection = null;
			this.addressList = new ArrayList();
			this.nameList = new ArrayList();
			this.emailList = new ArrayList();
			this.phoneList = new ArrayList();
		}

		#endregion

		#region Private Methods

		internal void Add(Collection collection, AddressBook addressBook, string identityGuid)
		{
			try
			{
				this.collection = collection;
				this.addressBook = addressBook;
				
				if (userName != null && userName != "")
				{
					this.thisNode = new Node(this.collection, userName, Guid.NewGuid().ToString(), Common.contactType);
					//this.thisNode = new Node(this.collection, userName, userGuid, Common.contactType);
					this.thisNode.SetParent(this.collection);
					this.id = this.thisNode.Id;

					// Add the product ID property
					if (this.productID == null)
					{
						this.productID = "Novell.AddressBook";
					}

					// Store the guid of the identity in the contact
					//this.thisNode.Properties.ModifyProperty( Common.identityProperty, userGuid );
					//this.identity = userGuid;

					//
					// Check if any dirty data exists in the node
					//

					foreach(Address address in this.addressList)
					{
						this.AddAddress(address);
					}
					this.addressList.Clear();

					foreach(Name name in this.nameList)
					{
						this.AddName(name);
					}
					this.nameList.Clear();

					/*
					foreach(Email mail in this.emailList)
					{
						this.AddEmailAddress(mail);
					}
					this.emailList.Clear();
					*/

					/*
					foreach(Telephone phone in this.phoneList)
					{
						this.AddTelephoneNumber(phone);
					}
					this.phoneList.Clear();
					*/

					if (this.photoStream != null)
					{
						this.ImportPhoto(this.photoStream);
						this.photoStream.Close();
						this.photoStream = null;
					}

					//this.thisNode.Save();

					//this.Create(userName);
					//this.collection.Commit(true);
					return;
				}
			}
			catch{}
			throw new ApplicationException(Common.addressBookExceptionHeader + "Contact - missing mandatory properties");
		}

		internal void SetDirty()
		{
			this.propertyChangeMap |= ChangeMap.unknown;
		}

		internal void SetDirty(ChangeMap dirtyProperty)
		{
			this.propertyChangeMap |= dirtyProperty;
		}

		private	string GetFullName()
		{
			Name	tmpName = null;
			string fn = null;

			try
			{
				tmpName = this.GetPreferredName();
			}
			catch
			{
				return(null);
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
		internal void ToObject(Collection parentCollection, string contactID)
		{
			this.collection = parentCollection;

			try
			{
				Node cNode = this.collection.GetNodeById(contactID);
				if ( cNode != null )
				{
					if (cNode.Type == Common.contactType)
					{
						this.userName = cNode.Name;
						this.id = cNode.Id;
						this.thisNode = cNode;

						try
						{
							this.nickName = cNode.Properties.GetSingleProperty(Common.nicknameProperty).ToString();
						}
						catch{}

						try
						{
							this.title = cNode.Properties.GetSingleProperty(Common.titleProperty).ToString();
						}
						catch{}

						try
						{
							this.role = cNode.Properties.GetSingleProperty(Common.roleProperty).ToString();
						}
						catch{}

						try
						{
							this.birthday = cNode.Properties.GetSingleProperty(Common.birthdayProperty).ToString();
						}
						catch{}

						try
						{
							this.productID = cNode.Properties.GetSingleProperty(Common.productIDProperty).ToString();
						}
						catch{}

						try
						{
							this.workforceID = cNode.Properties.GetSingleProperty(Common.workForceIDProperty).ToString();
						}
						catch{}

						try
						{
							this.note = cNode.Properties.GetSingleProperty(Common.noteProperty).ToString();
						}
						catch{}

						try
						{
							this.manager = cNode.Properties.GetSingleProperty(Common.managerIDProperty).ToString();
						}
						catch{}

						try
						{
							this.url = cNode.Properties.GetSingleProperty(Common.urlProperty).ToString();
						}
						catch{}

						try
						{
							this.organization = cNode.Properties.GetSingleProperty(Common.organizationProperty).ToString();
						}
						catch{}

						try
						{
							this.blogUrl = cNode.Properties.GetSingleProperty(Common.blogProperty).ToString();
						}
						catch{}

						try
						{
							// Load up email addresses
							this.emailList.Clear();
							MultiValuedList	mList = cNode.Properties.GetProperties(Common.emailProperty);
							foreach(Property p in mList)
							{
								//Email tmpMail = new Email(this.collection, this.thisNode, this, (string) p.Value);
								this.emailList.Add(new Email(this, (string) p.Value));
							}
						}
						catch{}

						try
						{
							// Load up telephone numbers
							this.phoneList.Clear();
							MultiValuedList	mList = cNode.Properties.GetProperties(Common.phoneProperty);
							foreach(Property p in mList)
							{
								this.phoneList.Add(new Telephone(this, (string) p.Value));
							}
						}
						catch{}
						return;
					}
				}
			}
			catch{}
			throw new ApplicationException(Common.addressBookExceptionHeader + "Specified contact does not exist");
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

			throw new ApplicationException(Common.addressBookExceptionHeader + "A preferred email address does not exist");
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
				this.phoneList.Add(telephone);

				/*
				if (this.thisNode != null)
				{
					telephone.Add(this.collection, this.thisNode, this);
				}
				else
				{
					this.phoneList.Add(telephone);
				}
				*/
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
		/// An exception is raised if a preferred telephone number
		/// does not exist on the contact.
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

			throw new ApplicationException(Common.addressBookExceptionHeader + "A preferred telephone number does not exist");
		}

		/// <summary>
		/// Commits any changes to the Contact object.
		/// </summary>
		public void Commit()
		{
			if (propertyChangeMap != 0 && this.thisNode != null)
			{
				try
				{
					this.thisNode.Name = userName;

					if ((propertyChangeMap & ChangeMap.url) == ChangeMap.url)
					{
						if (this.url != null)
						{
							this.thisNode.Properties.ModifyProperty(Common.urlProperty, this.url);
						}
						else
						{
							Property p = new Property(Common.urlProperty, this.url);
							this.thisNode.Properties.DeleteSingleProperty(p);
						}
					}

					if ((propertyChangeMap & ChangeMap.nickname) == ChangeMap.nickname)
					{
						if (this.nickName != null)
						{
							this.thisNode.Properties.ModifyProperty(Common.nicknameProperty, this.nickName);
						}
						else
						{
							this.thisNode.Properties.DeleteProperties(Common.nicknameProperty);
						}
					}

					if ((propertyChangeMap & ChangeMap.title) == ChangeMap.title)
					{
						if (this.title != null)
						{
							this.thisNode.Properties.ModifyProperty(Common.titleProperty, this.title);
						}
						else
						{
							Property p = new Property(Common.titleProperty, this.title);
							this.thisNode.Properties.DeleteSingleProperty(p);
						}
					}

					if ((propertyChangeMap & ChangeMap.role) == ChangeMap.role)
					{
						if (this.role != null)
						{
							this.thisNode.Properties.ModifyProperty(Common.roleProperty, this.role);
						}
						else
						{
							Property p = new Property(Common.roleProperty, this.role);
							this.thisNode.Properties.DeleteSingleProperty(p);
						}
					}

					if ((propertyChangeMap & ChangeMap.birthday) == ChangeMap.birthday)
					{
						if (this.birthday != null)
						{
							this.thisNode.Properties.ModifyProperty(Common.birthdayProperty, this.birthday);
						}
						else
						{
							Property p = new Property(Common.birthdayProperty, this.birthday);
							this.thisNode.Properties.DeleteSingleProperty(p);
						}
					}

					if ((propertyChangeMap & ChangeMap.productid) == ChangeMap.productid)
					{
						if (this.productID != null)
						{
							this.thisNode.Properties.ModifyProperty(Common.productIDProperty, this.productID);
						}
						else
						{
							Property p = new Property(Common.productIDProperty, this.productID);
							this.thisNode.Properties.DeleteSingleProperty(p);
						}
					}

					if ((propertyChangeMap & ChangeMap.workforce) == ChangeMap.workforce)
					{
						if (this.workforceID != null)
						{
							this.thisNode.Properties.ModifyProperty(Common.workForceIDProperty, this.workforceID);
						}
						else
						{
							Property p = new Property(Common.workForceIDProperty, this.workforceID);
							this.thisNode.Properties.DeleteSingleProperty(p);
						}
					}

					if ((propertyChangeMap & ChangeMap.manager) == ChangeMap.manager)
					{
						if (this.manager != null)
						{
							this.thisNode.Properties.ModifyProperty(Common.managerIDProperty, this.manager);
						}
						else
						{
							Property p = new Property(Common.managerIDProperty, this.manager);
							this.thisNode.Properties.DeleteSingleProperty(p);
						}
					}

					if ((propertyChangeMap & ChangeMap.note) == ChangeMap.note)
					{
						if (this.note != null)
						{
							this.thisNode.Properties.ModifyProperty(Common.noteProperty, this.note);
						}
						else
						{
							Property p = new Property(Common.noteProperty, this.note);
							this.thisNode.Properties.DeleteSingleProperty(p);
						}
					}

					if ((propertyChangeMap & ChangeMap.blog) == ChangeMap.blog)
					{
						if (this.blogUrl != null)
						{
							this.thisNode.Properties.ModifyProperty(Common.blogProperty, this.blogUrl);
						}
						else
						{
							Property p = new Property(Common.blogProperty, this.blogUrl);
							this.thisNode.Properties.DeleteSingleProperty(p);
						}
					}

					if ((propertyChangeMap & ChangeMap.organization) == ChangeMap.organization)
					{
						if (this.organization != null)
						{
							this.thisNode.Properties.ModifyProperty(Common.organizationProperty, this.organization);
						}
						else
						{
							Property p = new Property(Common.organizationProperty, this.organization);
							this.thisNode.Properties.DeleteSingleProperty(p);
						}
					}

					if (this.emailList.Count > 0 && ((propertyChangeMap & ChangeMap.email) == ChangeMap.email))
					{
						Email.PersistToStore(this);
					}

					if (this.phoneList.Count > 0 && ((propertyChangeMap & ChangeMap.phone) == ChangeMap.phone))
					{
						Telephone.PersistToStore(this);
					}

					this.collection.Commit(true);
					propertyChangeMap = 0;
				}
				catch
				{
					//this.thisNode.Abort();
				}
			}
		}

		/// <summary>
		/// Deletes the current Contact object.
		/// </summary>
		public void Delete()
		{
			try
			{
				// Delete all children as well
				this.thisNode.Delete(true);
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
				this.propertyChangeMap |= ChangeMap.address;
				if (this.collection != null && this.thisNode != null)
				{
					addr.Create(this.collection, this.thisNode, this);
					return;
				}
				else
				{
					this.addressList.Add(addr);
					return;
				}
			}
			catch{}
			throw new ApplicationException(Common.addressBookExceptionHeader + "Name object created");
		}

		/// <summary>
		/// Gets a list of addresses attached to this contact.
		/// </summary>
		public IABList GetAddresses()
		{
			IABList cList = new IABList();

			try
			{
				if (this.thisNode != null)
				{
					foreach(Node cNode in this.thisNode)
					{
						if (cNode.Type == Common.addressProperty)
						{
							cList.Add(GetAddress(cNode.Id));
						}
					}
				}
				else
				{
					foreach(Address addr in this.addressList)
					{
						cList.Add(addr);
					}
				}
			}
			catch{}
			return(cList);

			//			return new AddressEnumerator( this.thisNode, this );
		}

		/// <summary>
		/// Gets the preferred address for this contact.
		/// </summary>
		public Address GetPreferredAddress()
		{
			IABList addrList = this.GetAddresses();

			foreach(Address addr in addrList)
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
				Address address = new Address();
				address.ToObject(this.collection, this.thisNode, this, addressID);
				address.Delete();
			}
			catch
			{
				throw new ApplicationException(Common.addressBookExceptionHeader + "Address " + addressID + " not found" );
			}
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
				Address address = new Address();
				address.ToObject(this.collection, this.thisNode, this, addressID);
				return(address);
			}
			catch
			{
				throw new ApplicationException( Common.addressBookExceptionHeader +  "Address " + addressID + " not found" );
			}
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
				this.propertyChangeMap |= ChangeMap.name;
				if (this.collection != null && this.thisNode != null)
				{
					name.Create(this.collection, thisNode, this);
					return;
				}
				else
				{
					this.nameList.Add(name);
					return;
				}
			}
			catch{}
			throw new ApplicationException(Common.addressBookExceptionHeader + "Name object created");
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
		/// an exception is raised.
		/// </remarks>
		/// <returns>A Name object with at least given and family properties.</returns>
		public Name GetName(string nameID)
		{
			try
			{
				Name name = new Name();
				name.ToObject(this.collection, this.thisNode, this, nameID);
				return(name);
			}
			catch
			{
				throw new ApplicationException(Common.addressBookExceptionHeader + "Name " + nameID + " not found");
			}
		}

		/// <summary>
		/// Gets the preferred Name for this contact.
		/// </summary>
		/// <remarks>
		/// vCard Name is a structured property consisting of:
		/// Family Name, Given Name, Additional Names, Honorific Prefixes
		/// and Honorific Suffixes
		/// 
		/// If the contact does not have a preferred Name an exception is raised.
		/// </remarks>
		/// <returns>A Name object with at least valid given and family properties.</returns>
		public Name GetPreferredName()
		{
			IABList names = this.GetNames();

			foreach(Name name in names)
			{
				if (name.Preferred == true)
				{
					return(name);
				}
			}

			throw new ApplicationException(Common.abExceptionHeader + "Preferred name was not found" );
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
				if (this.thisNode != null)
				{
					foreach(Node cnode in thisNode)
					{
						if (cnode.Type == Common.nameProperty)
						{
							cList.Add(GetName(cnode.Id));
						}
					}
				}
				else
				{
					foreach(Name name in this.nameList)
					{
						cList.Add(name);
					}
				}
			}
			catch{}
			return(cList);
		}

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
					return this.photoStream;
				else
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
						this.collection.Commit(true);
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

					/*
					if (bWriter != null)
					{
						bWriter.Close();
					}
					*/
				}
			}

			return(finished);
		}

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

					/*
					srcCard = ExportVCard();

					int i;
					while(true)
					{
						i = srcCard.ReadByte();
						if(i == -1)
						{
							break;
						}

						dstCard.WriteByte((byte) i);
					}
					*/
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

			// Write out the preferred email address
			try
			{
				IABList mailEnum = this.GetEmailAddresses();

				foreach(Email tmpMail in mailEnum)
				{
					foreach(char c in emailHdr)
					{
						vCard.WriteByte(Convert.ToByte(c));
					}

					/*
					if (tmpMail.Preferred == true || tmpMail.Types > 0)
					{
						vCard.WriteByte(Convert.ToByte(';'));
					}
					*/

					if (tmpMail.Preferred == true)
					{
						vCard.WriteByte(Convert.ToByte(';'));
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

						vCard.WriteByte(Convert.ToByte('O'));
						vCard.WriteByte(Convert.ToByte('T'));
						vCard.WriteByte(Convert.ToByte('H'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('R'));
					}

					/*
					if ((tmpMail.Types & EmailTypes.work) == EmailTypes.work)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						vCard.WriteByte(Convert.ToByte('W'));
						vCard.WriteByte(Convert.ToByte('O'));
						vCard.WriteByte(Convert.ToByte('R'));
						vCard.WriteByte(Convert.ToByte('K'));
					}

					if ((tmpMail.Types & EmailTypes.personal) == EmailTypes.personal)
					{
						vCard.WriteByte(Convert.ToByte(';'));

						vCard.WriteByte(Convert.ToByte('H'));
						vCard.WriteByte(Convert.ToByte('O'));
						vCard.WriteByte(Convert.ToByte('M'));
						vCard.WriteByte(Convert.ToByte('E'));
					}
					*/

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

					if (tmpPhone.Preferred == true || (uint) tmpTypes > 0)
					{
						vCard.WriteByte(Convert.ToByte(';'));
						vCard.WriteByte(Convert.ToByte('T'));
						vCard.WriteByte(Convert.ToByte('Y'));
						vCard.WriteByte(Convert.ToByte('P'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('='));
					}

					if (tmpPhone.Preferred == true)
					{
						foreach( char c in prefHdr)
						{
							vCard.WriteByte((byte) c);
						}

						tmpTypes &= ~PhoneTypes.preferred;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.work) == PhoneTypes.work)
					{
						foreach(char c in workHdr)
						{
							vCard.WriteByte((byte) c);
						}

						tmpTypes &= ~PhoneTypes.work;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.home) == PhoneTypes.home)
					{
						foreach(char c in homeHdr)
						{
							vCard.WriteByte((byte) c);
						}

						tmpTypes &= ~PhoneTypes.home;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.voice) == PhoneTypes.voice)
					{
						vCard.WriteByte((byte) 'V');
						vCard.WriteByte((byte) 'O');
						vCard.WriteByte((byte) 'I');
						vCard.WriteByte((byte) 'C');
						vCard.WriteByte((byte) 'E');

						tmpTypes &= ~PhoneTypes.voice;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.msg) == PhoneTypes.msg)
					{
						vCard.WriteByte(Convert.ToByte('M'));
						vCard.WriteByte(Convert.ToByte('S'));
						vCard.WriteByte(Convert.ToByte('G'));

						tmpTypes &= ~PhoneTypes.msg;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.cell) == PhoneTypes.cell)
					{
						vCard.WriteByte(Convert.ToByte('C'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('L'));
						vCard.WriteByte(Convert.ToByte('L'));

						tmpTypes &= ~PhoneTypes.cell;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.bbs) == PhoneTypes.bbs)
					{
						vCard.WriteByte(Convert.ToByte('B'));
						vCard.WriteByte(Convert.ToByte('B'));
						vCard.WriteByte(Convert.ToByte('S'));

						tmpTypes &= ~PhoneTypes.bbs;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.car) == PhoneTypes.car)
					{
						vCard.WriteByte(Convert.ToByte('C'));
						vCard.WriteByte(Convert.ToByte('A'));
						vCard.WriteByte(Convert.ToByte('R'));

						tmpTypes &= ~PhoneTypes.car;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.isdn) == PhoneTypes.isdn)
					{
						vCard.WriteByte(Convert.ToByte('I'));
						vCard.WriteByte(Convert.ToByte('S'));
						vCard.WriteByte(Convert.ToByte('D'));
						vCard.WriteByte(Convert.ToByte('N'));

						tmpTypes &= ~PhoneTypes.isdn;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.fax) == PhoneTypes.fax)
					{
						vCard.WriteByte(Convert.ToByte('F'));
						vCard.WriteByte(Convert.ToByte('A'));
						vCard.WriteByte(Convert.ToByte('X'));

						tmpTypes &= ~PhoneTypes.fax;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.modem) == PhoneTypes.modem)
					{
						vCard.WriteByte(Convert.ToByte('M'));
						vCard.WriteByte(Convert.ToByte('O'));
						vCard.WriteByte(Convert.ToByte('D'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('M'));

						tmpTypes &= ~PhoneTypes.modem;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.pager) == PhoneTypes.pager)
					{
						vCard.WriteByte(Convert.ToByte('P'));
						vCard.WriteByte(Convert.ToByte('A'));
						vCard.WriteByte(Convert.ToByte('G'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('R'));

						tmpTypes &= ~PhoneTypes.pager;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.video) == PhoneTypes.video)
					{
						vCard.WriteByte(Convert.ToByte('V'));
						vCard.WriteByte(Convert.ToByte('I'));
						vCard.WriteByte(Convert.ToByte('D'));
						vCard.WriteByte(Convert.ToByte('E'));
						vCard.WriteByte(Convert.ToByte('O'));

						tmpTypes &= ~PhoneTypes.video;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.pcs) == PhoneTypes.pcs)
					{
						vCard.WriteByte((byte) 'P');
						vCard.WriteByte((byte) 'C');
						vCard.WriteByte((byte) 'S');

						tmpTypes &= ~PhoneTypes.pcs;
						if ((uint) tmpTypes > 0)
						{
							vCard.WriteByte((byte) ',');
						}
					}

					if ((tmpPhone.Types & PhoneTypes.voip) == PhoneTypes.voip)
					{
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
			nodeEnum = this.parentNode.GetEnumerator();
		}

		/// <summary>
		/// Gets the current node contained in thisNode
		/// </summary>
		public object Current
		{
			get
			{
				Node cNode = (Node) this.nodeEnum.Current;
				Address addr = this.parentContact.GetAddress(cNode.Id);
				return(addr);
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
