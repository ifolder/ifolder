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
using System.Text.RegularExpressions;
using Simias.Storage;

namespace Novell.AddressBook
{
	/// <summary>
	/// Well known address book types.
	/// </summary>
	public enum AddressBookType : int
	{
		/// <summary>
		/// indicates a private address book - non shared
		/// </summary>
		Private,
 
		/// <summary>
		/// indicates a private address book shared to the world
		/// </summary>
		Public,

		/// <summary>
		/// indicates a personal but shared address book
		/// </summary>
		Shared,
 
		/// <summary>
		/// indicates a global address book - ex. corporate
		/// </summary>
		Global 
	};

	/// <summary>
	/// User rights on an address book
	/// </summary>
	public enum AddressBookRights : int
	{ 
		/// <summary>
		/// indicates undefined rights
		/// FIX don't need this
		/// </summary>
		Undefined,
 
		/// <summary>
		/// indicates the user has read rights only on the address book
		/// </summary>
		ReadOnly,
 
		/// <summary>
		/// indicates the user has read/write rights on the address book
		/// </summary>
		ReadWrite
	};

	/// 
	/// <summary>
	/// Summary description for AddressBook.
	/// </summary>
	public class AddressBook : IEnumerable, IEnumerator
	{
		#region Class Members
		internal	Store				store;
		internal	Collection			collection;
		private		bool				deleted;
		private		bool				changed;
		private		IEnumerator			contactEnum = null;
		private		AddressBookType		addressBookType;
		private		AddressBookRights	addressBookRights;
		private		bool				defaultBook;
		private		string				friendlyName;
//		private		string				domain;

		#endregion

		#region Constructors

		/// <summary>
		/// Simple AddressBook constructor
		/// </summary>
		public	AddressBook()
		{
			this.CommonInitialization();
		}

		/// <summary>
		/// AddressBook constructor which includes the friendly name
		/// </summary>
		public AddressBook(string friendlyName)
		{
			this.CommonInitialization();
			this.friendlyName = friendlyName;
		}

		/// <summary>
		/// AddressBook constructor which includes the type and rights
		/// </summary>
		public AddressBook(AddressBookType type, AddressBookRights rights)
		{
			this.CommonInitialization();
			this.addressBookType = type;
			this.addressBookRights = rights;
		}

		/// <summary>
		/// AddressBook constructor which includes all properties - friendly name, type and rights
		/// </summary>
		public AddressBook(string friendlyName, AddressBookType type, AddressBookRights rights)
		{
			this.CommonInitialization();
			this.addressBookType = type;
			this.addressBookRights = rights;
			this.friendlyName = friendlyName;
		}

		/// <summary>
		/// AddressBook constructor which includes all properties - friendly name, type and rights
		/// and sets the new address book as default for this store.
		/// </summary>
		public AddressBook(string friendlyName, AddressBookType type, AddressBookRights rights, bool defaultBook)
		{
			this.CommonInitialization();
			this.addressBookType = type;
			this.addressBookRights = rights;
			this.friendlyName = friendlyName;
			this.defaultBook = defaultBook;
		}

		internal AddressBook(Store store)
		{
			this.store = store;
		}

		#endregion

		#region Internal Methods

		internal void CommonInitialization()
		{
			this.deleted = false;
			this.changed = false;
			this.defaultBook = false;
			this.collection = null;
			this.store = null;
			this.contactEnum = null;
			this.friendlyName = "Novell Address Book";
			this.addressBookType = AddressBookType.Private;
			this.addressBookRights = AddressBookRights.Undefined;
		}

		// FIXUP
		internal void BuildVCardName(Contact cContact, IEnumerator nameEnum)
		{
			Name	name = new Name();

			try
			{
				// Family name should be the first token
				if ((string) nameEnum.Current != "")
				{
					name.Family = (string) nameEnum.Current;
				}

				// Next should come the given name
				if (nameEnum.MoveNext())
				{
					if ((string) nameEnum.Current != "")
					{
						name.Given = (string) nameEnum.Current;

						// Enough for a valid name
						// At minimum we need a given and family name

						if (name.Family != "")
						{
							cContact.AddName(name);
						}
					}

					// Next is the other name
					if(nameEnum.MoveNext())
					{
						if ((string) nameEnum.Current != "")
						{
							name.Other = (string) nameEnum.Current;
						}

						// Next is prefix
						if(nameEnum.MoveNext())
						{
							if ((string) nameEnum.Current != "")
							{
								name.Prefix = (string) nameEnum.Current;
							}

							// Last is suffix
							if(nameEnum.MoveNext())
							{
								if ((string) nameEnum.Current != "")
								{
									name.Suffix = (string) nameEnum.Current;
								}
							}	
						}
					}
				}
			}
			catch{}
		}

		// FIXUP
		internal void BuildVCardAddress(Contact cContact, IEnumerator propertyTokens, IEnumerator valueTokens)
		{
			Address address = new Address();
			uint	addrTypes = 0;
			uint	valuesAdded = 0;

			try
			{
				while(propertyTokens.MoveNext())
				{
					string tmpString = ((string) propertyTokens.Current).ToUpper();

					if (tmpString == "PREF")
					{
						address.Preferred = true;
					}
					else
					if (tmpString == "WORK")
					{
						addrTypes |= (uint) AddressTypes.work;
					}
					else
					if( tmpString == "HOME")
					{
						addrTypes |= (uint) AddressTypes.home;
					}
					else
					if( tmpString == "OTHER")
					{
						addrTypes |= (uint) AddressTypes.other;
					}
					else
					if (tmpString == "DOM")
					{
						addrTypes |= (uint) AddressTypes.dom;
					}
					else
					if(tmpString == "INTL")
					{
						addrTypes |= (uint) AddressTypes.intl;
					}
					else
					if (tmpString == "POSTAL")
					{
						addrTypes |= (uint) AddressTypes.parcel;
					}
					else
					if (tmpString == "PARCEL")
					{
						addrTypes |= (uint) AddressTypes.parcel;
					}
				}

				if (addrTypes != 0)
				{
					address.Types = (AddressTypes) addrTypes;
				}

				// PO box
				//if ( valueTokens.MoveNext())
				//{
					if ((string) valueTokens.Current != "")
					{
						address.PostalBox = (string) valueTokens.Current;
						valuesAdded++;
					}

					// Extended Address
					if (valueTokens.MoveNext())
					{
						if ((string) valueTokens.Current != "")
						{
							address.ExtendedAddress = (string) valueTokens.Current;
							valuesAdded++;
						}

						// Street Address
						if (valueTokens.MoveNext())
						{
							if ((string) valueTokens.Current != "")
							{
								address.Street = (string) valueTokens.Current;
								valuesAdded++;
							}

							// Locality/City
							if (valueTokens.MoveNext())
							{
								if ((string) valueTokens.Current != "")
								{
									address.Locality = (string) valueTokens.Current;
									valuesAdded++;
								}

								// Region/State/Province
								if (valueTokens.MoveNext())
								{
									if ((string) valueTokens.Current != "")
									{
										address.Region = (string) valueTokens.Current;
										valuesAdded++;
									}

									// Postal code
									if (valueTokens.MoveNext())
									{
										if ((string) valueTokens.Current != "")
										{
											address.PostalCode = (string) valueTokens.Current;
											valuesAdded++;
										}

										// Country
										if (valueTokens.MoveNext())
										{
											if ((string) valueTokens.Current != "")
											{
												address.Country = (string) valueTokens.Current;
												valuesAdded++;
											}
										}
									}
								}
							}
						}
					}

					if (valuesAdded != 0)
					{
						cContact.AddAddress(address);
					}
				//}
			}
			catch{}
		}

		/// <summary>
		/// Method: Add
		/// Abstract: Address books are added through the AddressBook.Manager class.
		/// </summary>
		internal void Add(Store store)
		{
			this.store = store;

			// Create the address book in the default
			//this.collection = store.CreateCollection(this.friendlyName, "AB:AddressBook");
			this.collection = new Collection(store, this.friendlyName);

			this.collection.SetType(this.collection, Common.addressBookProperty);
			Property pType = new Property( Common.addressBookTypeProperty, (Int32) this.addressBookType);
			this.collection.Properties.AddProperty( pType );
			Property pRights = new Property( Common.addressBookRightsProperty, (Int32) this.addressBookRights);
			this.collection.Properties.AddProperty( pRights );

			/*
			if (this.defaultBook == true)
			{
				this.collection.Properties.AddProperty( Property.DefaultAddressBook, true );
			}
			*/

			this.collection.Commit();
			this.changed = false;
		}

		/// <summary>
		/// Method: ToObject
		/// Abstract: Method to pull property data from the collection store
		/// and populate the members in the address book object.
		/// </summary>
		internal void ToObject(string addressBookID)
		{
			Property p;
			if (this.store == null)
			{
				throw new ApplicationException("AddressBook::Missing store handle");
			}

			this.collection = this.store.GetCollectionByID(addressBookID);

			// Make sure the returned collection is in fact an address book
			if (this.collection.IsType(this.collection, Common.addressBookProperty) == false)
			{
				this.collection = null;
				throw new ApplicationException("AddressBook::Invalid ID parameter - not an address book");
			}

			this.friendlyName = this.collection.Name;

			// Load up the previously persisted properties
			try
			{
				p = this.collection.Properties.GetSingleProperty(Common.addressBookTypeProperty);
				if (p != null)
				{
					this.addressBookType = (AddressBookType) p.Value;
				}
			}
			catch{}

			try
			{
				p = this.collection.Properties.GetSingleProperty(Common.addressBookRightsProperty);
				if (p != null)
				{
					this.addressBookRights = (AddressBookRights) p.Value;
				}
			}
			catch{}

			/*
			// domain is not mandatory
			try
			{
//				this.domain = this.collection.Properties.GetSingleProperty( "AB:Domain").ToString();
				this.domain = this.collection.Properties.GetSingleProperty("DomainName").ToString();
			}
			catch
			{
				this.domain = "";
			}
			*/

			// defaultBook is not mandatory

			// FIXUP
			this.defaultBook = false;
			/*
			try
			{
				this.defaultBook = 
					Convert.ToBoolean(this.collection.Properties.GetSingleProperty(Property.DefaultAddressBook).ToString());
			}
			catch
			{
				this.defaultBook = false;
			}
			*/

			this.deleted = false;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Name
		/// !FINISH! 
		/// </summary>
		public string Name
		{
			get 
			{
				return(this.friendlyName);
			}

			set 
			{ 
				this.friendlyName = value;
				changed = true;
			}
		}

		/// <summary>
		/// ID
		/// !FINISH!
		/// </summary>
		public string ID
		{
			get
			{ 
				try
				{
					if (this.collection != null)
					{
						return((string)this.collection.Properties.GetSingleProperty( "CollectionID" ).Value);
					}
					else
					{
						return("");
					}
				}
				catch
				{
					return("");
				}
			}
		}

		/// <summary>
		/// Default
		/// !FINISH!
		/// </summary>
		public bool	Default
		{
			get
			{ 
				return(this.defaultBook);
			}
		}

		/// <summary>
		/// Domain
		/// !FINISH!
		/// </summary>
		public string	Domain
		{
			get
			{ 
				try
				{
					if (this.collection != null)
					{
						// FIXUP
						//return((string)this.collection.Properties.GetSingleProperty( Property.DomainName ).Value);
						return("");
					}
					else
					{
						return("");
					}
				}
				catch
				{
					return("");
				}
			}

			/*
			set
			{
				if (this.addressBookRights != AddressBookRights.ReadOnly)
				{
					this.domain = value;
					this.changed = true;
				}
			}
			*/
		}

		/// <summary>
		/// Type
		/// !FINISH!
		/// </summary>
		public AddressBookType Type
		{
			get
			{ 
				return( this.addressBookType );
				//return((AddressBookType) this.collection.Properties.GetSingleProperty( "AB:AddresssBookType").Value);
			}
		}

		/// <summary>
		/// Rights
		/// !FINISH!
		/// </summary>
		public AddressBookRights Rights
		{
			get
			{ 
				return(this.addressBookRights);
				//return((AddressBookRights) this.collection.Properties.GetSingleProperty( "AB:AddresssBookRights").Value);
			}
		}
		#endregion


		/// <summary>
		/// Method: AddContact
		/// Abstract: Method to create a contact in the store
		/// this method binds the contact to a known identityId
		/// </summary>
		internal void AddContact(Contact contact, string identityId)
		{
			if (this.collection != null)
			{
				//
				// Add will throw an exception if the contact book
				// cannot be added to the address book or if the contact
				// is not complete
				//

				//contact.Add(this.collection, this, identityId);
				contact.Add(this);
			}
			else
			{
				throw new ApplicationException("AddressBook::The address book has not been added to the store");
			}
		}

		internal bool AddVCardContact(Contact cContact)
		{
			int		foundState;

			// Make sure we have a preferred Name and Address
			try
			{
				IABList	names = cContact.GetNames();
				foundState = 0;
				foreach(Name name in names)
				{
					foundState++;
					if (name.Preferred == true)
					{
						foundState = -1;
						break;
					}
				}

				if (foundState > 0)
				{
					// No preferred Name - set the first one preferred
					foreach(Name name in names)
					{
						name.Preferred = true;
						break;
					}
				}

				IABList	addresses = cContact.GetAddresses();
				foundState = 0;
				foreach(Address addr in addresses)
				{
					foundState++;
					if (addr.Preferred == true)
					{
						foundState = -1;
						break;
					}
				}

				if (foundState > 0)
				{
					// No preferred Address - set the first one preferred
					foreach(Address addr in addresses)
					{
						addr.Preferred = true;
						break;
					}
				}

				if (cContact.UserName == "")
				{
					try
					{
						Email pref = cContact.GetPreferredEmailAddress();
						cContact.UserName = pref.Address;
					}
					catch
					{
						// No preferred e-mail so let's use first and last name
						try
						{
							Name name = cContact.GetPreferredName();

							string	firstLast = name.Given;
							firstLast += name.Family;
							cContact.UserName = firstLast;
						}
						catch{}
					}
				}

				this.AddContact(cContact);
				cContact.Commit();
				return(true);
			}
			catch{}
			return(false);
		}

		#region Public Methods

		/// <summary>
		/// Method: AddContact
		/// Abstract: Method to add a contact to an address book.
		/// The contact is not persisted to the store until the
		/// commit method is called on the contact.
		/// </summary>
		public void	AddContact(Contact contact)
		{
			if (this.collection != null)
			{
				//
				// Add will throw an exception if the contact book
				// cannot be added to the address book or if the contact
				// is not complete
				//

				contact.Add(this);
			}
			else
			{
				throw new ApplicationException(Common.addressBookExceptionHeader + "The address book has not been added to the store");
			}
		}

		/// <summary>
		/// Method: Commit
		/// Abstract: Method to commit the address book changes to the store.
		/// </summary>
		public void Commit()
		{
			if (this.collection == null)
			{
				throw new ApplicationException(Common.addressBookExceptionHeader + "The address book has not been added to the store");
			}

			this.collection.Name = this.friendlyName;
			this.collection.Properties.ModifyProperty("AB:AddressBookType", this.addressBookType);
			this.collection.Properties.ModifyProperty("AB:AddressBookRights", this.addressBookRights);

			this.collection.Commit();
			//his.changed = false;
		}

		/// <summary>
		/// Method: Delete
		/// Abstract: Method to delete an address book.
		/// </summary>
		public void Delete()
		{
			if (this.collection == null)
			{
				throw new ApplicationException("AddressBook::The address book has not been added to the store");
			}

			this.collection.Commit( this.collection.Delete() );
			this.deleted = true;
		}

		/// <summary>
		/// Retrieve a contact using a specified id.
		/// </summary>
		/// <param name="id">Specified contact ID</param>
		/// <remarks>
		/// If the contact is not found a null is returned.
		/// </remarks>
		/// <returns>A Contact object with at minimum a valid username property.</returns>
		public Contact GetContact(string id)
		{
			Contact cContact = null;
			if (this.collection != null)
			{
				try
				{
					Node cNode = this.collection.GetNodeByID(id);
					if ( cNode != null )
					{
						if (this.collection.IsType(cNode, Common.contactType) == true)
						{
							cContact = new Contact(this, cNode);
						}
					}
				}
				catch(Exception e)
				{
					Console.WriteLine(e.Message);
					cContact = null;
				}
			}

			return(cContact);
		}

		/// <summary>
		/// Import a vCard via a full path to a vCard file.
		/// </summary>
		/// <param name="fileName">vCard file to import</param>
		/// <remarks>
		/// If any error occurs importing and building a contact object,
		/// a null will be returned
		/// </remarks>
		/// <returns>A Contact object associated to this address book.</returns>
		public Contact	ImportVCard(string	fileName)
		{
			Contact			newContact = null;
			StreamReader	reader = null;

			try
			{
				reader = new StreamReader(fileName);
				if (reader != null)
				{
					newContact = this.ImportVCard(reader);
				}
			}
			catch{}
			finally
			{
				if (reader != null)
				{
					reader.Close();
				}
			}

			return(newContact);
		}

		/// <summary>
		/// Import a vCard via a stream.
		/// </summary>
		/// <param name="vCardStream">StreamReader to a previously opened vCard file</param>
		/// <remarks>
		/// If any error occurs importing and building a contact object,
		/// a null will be returned
		/// </remarks>
		/// <returns>A Contact object associated to this address book.</returns>
		public Contact ImportVCard(StreamReader vCardStream)
		{
			Contact	firstContact = null;
			try
			{
				bool	parsing = true;
				Contact	cContact = null;
				int		vCardState = 0;
				int		version = 0;
				char [] cChar = new char[1];
				char [] propertySeps = new char []{';', '.', '='};
				char [] valueSeps = new char [] {';'};

				// Make sure this is a valid vCard
				while(parsing)
				{
					string	property = "";
					String	propertyValues = "";

					cChar[0] = '\0';
					while(vCardStream.Read(cChar, 0, 1) != 0)
					{
						//cChar = vCardStream.Read();
						if (cChar[0] == 13)
						{
							if (vCardStream.Peek() == 10)
							{
								vCardStream.Read();
							}
							break;
						}

						if (cChar[0] == ':')
						{
							// PHOTO, SOUND and LOGO are handled a little bit different in that
							// the value (base64 encoded data) starts on the next line from the
							// property tag
							if (property.StartsWith("PHOTO"))
							{
								int status;
								int	nChars = 0;
								if (vCardStream.Peek() == 13 ||
									vCardStream.Peek() == 10)
								{
									// swallow the cr/lf
									vCardStream.ReadLine();
								}

								while (true)
								{
									status = vCardStream.Read(cChar, 0, 1);
									if (status != 0)
									{
										if (cChar[0] == 13)
										{
											if (vCardStream.Peek() == 10)
											{
												vCardStream.Read();
											}

											//Console.WriteLine("Photo stream length: {0}", nChars);
											break;
										}

										propertyValues += cChar[0];
										nChars++;
									}
									else
									{
										break;
									}
								}
							}
							else
							{
								cChar[0] = 'Z';
								propertyValues = vCardStream.ReadLine();
							}	break;
						}
						else
						{
							property += cChar[0];
						}
					} 

					if (cChar[0] == '\0')
					{
						break;
					}

					if (propertyValues == "")
					{
						continue;
					}

					// Start of a new vCard
					if (property == "BEGIN" && propertyValues == "VCARD")
					{
						if (vCardState > 3)
						{
							// Finish off a previous vCard
							if (AddVCardContact(cContact) == true)
							{
								if (firstContact == null)
								{
									firstContact = cContact;
								}
							}
						}

						version = 0;
						vCardState = 1;
					}
					else
					if (property == "VERSION")
					{
						if (propertyValues == "2.1")
						{
							version = 2;
							if (vCardState == 1)
							{
								cContact = new Contact();
								vCardState++;
							}
						}
						else
						if (propertyValues == "3.0")
						{
							version = 3;
							if (vCardState == 1)
							{
								cContact = new Contact();
								vCardState++;
							}
						}
					}
					else
					if (property == "END" && propertyValues == "VCARD")
					{
						if (vCardState > 3)
						{
							// Finish off the current card and add
							// to the address book

							if (AddVCardContact(cContact) == true)
							{
								if (firstContact == null)
								{
									firstContact = cContact;
								}
							}
						}

						vCardState = 0;
					}
					else
					if (vCardState >= 2)
					{
						string	propertyToken = "";

						// Tokenize the property and property attributes
						IEnumerator propTokens = property.Split(propertySeps).GetEnumerator();

						// Make sure we have a valid token.  The property itself
						// might not be the first token in the collection.
						while(propTokens.MoveNext())
						{
							for(int i = 0; i < Common.vCardProperties.Length; i++)
							{
								if ((string) propTokens.Current == Common.vCardProperties[i])
								{
									propertyToken = Common.vCardProperties[i];
									break;
								}
							}

							if (propertyToken != "")
							{
								break;
							}
						}

						if (propertyToken == "")
						{
							// Continue to the next line
							continue;
						}

						propTokens.Reset();

						//
						// Check if this property is one that does not
						// need to tokenize the property values
						//
						if (propertyToken == "NICKNAME")
						{
							cContact.Nickname = propertyValues;
							vCardState++;
						}
						else
						if (propertyToken == "TITLE")
						{
							cContact.Title = propertyValues;
							vCardState++;
						}
						else
						if (propertyToken == "NOTE")
						{
							cContact.Note = propertyValues;
							vCardState++;
						}
						else
						if (propertyToken == "BDAY")
						{
							cContact.Birthday = propertyValues;
							vCardState++;
						}
						else
						if (propertyToken == "ORG")
						{
							cContact.Organization = propertyValues;
							vCardState++;
						}
						else
						if (propertyToken == "URL")
						{
							cContact.Url = propertyValues;
							vCardState++;
						}
						else
						if (propertyToken == "X-NAB-USERNAME")
						{
							cContact.UserName = propertyValues;
							vCardState++;
						}
						else
						if (propertyToken == "X-NAB-BLOG")
						{
							cContact.Blog = propertyValues;
							vCardState++;
						}
						else
						if (propertyToken == "PHOTO")
						{
							byte[] binaryData = null;

							try
							{
								// FIXME need to add the encoding type
								// First line (Type and Encoding)
								//propValueTokens.MoveNext();

								// Second line actual base 64 encoded data
								//propValueTokens.MoveNext();
								binaryData = Convert.FromBase64String(propertyValues);

								MemoryStream tmpPhoto = new MemoryStream();
								BinaryWriter bWriter = new BinaryWriter(tmpPhoto);

								bWriter.Write(binaryData, 0, binaryData.Length);
								// FIXUP
								//cContact.ImportPhoto(tmpPhoto);
								vCardState++;
							}
							catch(Exception e)
							{
								Console.WriteLine(e.Message);
								Console.WriteLine(e.StackTrace);
							}
						}
						else
						{
							// tokenize the values past the property separator
							//Regex pv = new Regex(@";");
							IEnumerator propValueTokens = propertyValues.Split(valueSeps).GetEnumerator();
							if(propValueTokens.MoveNext())
							{
								if (propertyToken == "N")
								{
									this.BuildVCardName(cContact, propValueTokens);
									vCardState++;
								}
								else
								if (propertyToken == "ADR")
								{
									this.BuildVCardAddress(cContact, propTokens, propValueTokens);
								}
								else
								if (propertyToken == "EMAIL")
								{
									Email			tmpMail = new Email();
									string			tmpString;
									uint			mailTypes = 0;

									tmpMail.Address = propertyValues;
									while(propTokens.MoveNext())
									{
										tmpString = ((string) propTokens.Current).ToLower();
										if(tmpString == "pref")
										{
											mailTypes |= (uint) EmailTypes.preferred;
										}
										else
										if (tmpString == "internet")
										{
											mailTypes &= (uint) ~EmailTypes.x400;
											mailTypes |= (uint) EmailTypes.internet;
										}
										else
										if (tmpString == "x400")
										{
											mailTypes &= (uint) ~EmailTypes.internet;
											mailTypes |= (uint) EmailTypes.x400;
										}
										else
										if (tmpString == "work")
										{
											mailTypes |= (uint) EmailTypes.work;
										}
										else
										if (tmpString == "home")
										{
											mailTypes |= (uint) EmailTypes.personal;
										}
										else
										if (tmpString == "personal")
										{
											mailTypes |= (uint) EmailTypes.personal;
										}
										else
										if (tmpString == "other")
										{
											mailTypes |= (uint) EmailTypes.other;
										}
									}

									// vCard 2.1 doesn't have work/personal types
									// attached to an e-mail address so we'll default
									// to type "other"
									if ((mailTypes & (uint) EmailTypes.work) != (uint) EmailTypes.work &&
										(mailTypes & (uint) EmailTypes.personal) != (uint) EmailTypes.personal &&
										(mailTypes & (uint) EmailTypes.other) != (uint) EmailTypes.other)
									{
										mailTypes |= (uint) EmailTypes.other;
									}

									tmpMail.Types = (EmailTypes) mailTypes;
									cContact.AddEmailAddress(tmpMail);
									vCardState++;
								}
								else
								if (propertyToken == "TEL")
								{
									Telephone		tmpPhone = new Telephone();
									string			tmpString;
									uint			phoneTypes = 0;

									tmpPhone.Number = propertyValues;
									while(propTokens.MoveNext())
									{
										tmpString = ((string) propTokens.Current).ToLower();
										if(tmpString == "pref")
										{
											phoneTypes |= (uint) PhoneTypes.preferred;
										}
										else
										if (tmpString == "voice")
										{
											phoneTypes |= (uint) PhoneTypes.voice;
										}
										else
										if (tmpString == "msg")
										{
											phoneTypes |= (uint) PhoneTypes.msg;
										}
										else
										if (tmpString == "work")
										{
											phoneTypes |= (uint) PhoneTypes.work;
										}
										else
										if (tmpString == "home")
										{
											phoneTypes |= (uint) PhoneTypes.home;
										}
										else
										if (tmpString == "other")
										{
											phoneTypes |= (uint) PhoneTypes.other;
										}
										else
										if (tmpString == "cell")
										{
											phoneTypes |= (uint) PhoneTypes.cell;
										}
										else
										if (tmpString == "fax")
										{
											phoneTypes |= (uint) PhoneTypes.fax;
										}
										else
										if (tmpString == "voip")
										{
											phoneTypes |= (uint) PhoneTypes.voip;
										}
										else
										if (tmpString == "car")
										{
											phoneTypes |= (uint) PhoneTypes.car;
										}
										else
										if (tmpString == "pager")
										{
											phoneTypes |= (uint) PhoneTypes.pager;
										}
									}

									// If no types exist in the vCard stay with the
									// default types
									if (phoneTypes != 0)
									{
										tmpPhone.Types = (PhoneTypes) phoneTypes;
									}

									cContact.AddTelephoneNumber(tmpPhone);
									vCardState++;
								}
							}
						}
					}
				}
			}
			catch{}
			return(firstContact);
		}

		/// <summary>
		/// Find a user(s) by Username
		/// </summary>
		/// <param name="searchString"></param>
		/// <param name="queryOperator">how to query (begin, contains, ends)</param>
		/// <remarks>
		/// !FINISH API DOC!
		/// </remarks>
		/// <returns>A list of contacts which matched the search string.</returns>
		public IABList SearchUsername( string searchString, Simias.Storage.SearchOp searchOperator )
		{
			Node cNode;
			if (this.collection != null)
			{
				try
				{
					IABList cList = new IABList();

					ICSEnumerator	nodeEnum = (ICSEnumerator) 
						this.collection.Search(BaseSchema.ObjectName, searchString, searchOperator).GetEnumerator();

					try
					{
						while(nodeEnum.MoveNext())
						{
							cNode = new Node( this.collection, nodeEnum.Current as ShallowNode);
							if (this.collection.IsType(cNode, Common.contactType) == true)
							{
								cList.Add(this.GetContact(cNode.ID));
							}
						}
					}
					finally
					{
						nodeEnum.Dispose();
					}

					return(cList);
				}
				catch{}
				return(null);
			}
			else
			{
				throw new ApplicationException(Common.addressBookExceptionHeader + "The address book has not been added to the store");
			}
		}

		/// <summary>
		/// Find a user(s) by e-mail address
		/// </summary>
		/// <param name="searchString"></param>
		/// <param name="queryOperator"></param>
		/// <remarks>
		/// !FINISH API DOC!
		/// </remarks>
		/// <returns>A list of contacts which matched the search string.</returns>
		public
		IABList
		SearchEmail( string searchString, Simias.Storage.SearchOp searchOperator )
		{
			Node cNode;
			if (this.collection != null)
			{
				try
				{
					IABList cList = new IABList();

					ICSEnumerator	nodeEnum = (ICSEnumerator) 
						this.collection.Search(
							Common.emailProperty, 
							searchString, 
							searchOperator).GetEnumerator();

					try
					{
						while(nodeEnum.MoveNext())
						{
							cNode = new Node( this.collection, nodeEnum.Current as ShallowNode);
							if (this.collection.IsType(cNode, Common.contactType) == true)
							{
								cList.Add(this.GetContact(cNode.ID));
							}
						}
					}
					finally
					{
						nodeEnum.Dispose();
					}

					return(cList);
				}
				catch{}
				return(null);
			}
			else
			{
				throw new ApplicationException("AddressBook::The address book has not been added to the store");
			}
		}

		/// <summary>
		/// Find a user(s) by first name
		/// </summary>
		/// <param name="searchString"></param>
		/// <param name="searchOperator"></param>
		/// <remarks>
		/// !FINISH API DOC!
		/// </remarks>
		/// <returns>A list of contacts which matched the search string.</returns>
		public
		IABList 
		SearchFirstName( string searchString, Simias.Storage.SearchOp searchOperator )
		{
			if (this.collection != null)
			{
				try
				{
					IABList cList = new IABList();

					ICSEnumerator	nodeEnum = (ICSEnumerator) 
						this.collection.Search(
							BaseSchema.ObjectName, 
							searchString, 
							searchOperator).GetEnumerator();

					try
					{
						while(nodeEnum.MoveNext())
						{
							Node cNode = new Node( this.collection, nodeEnum.Current as ShallowNode);
							if (this.collection.IsType(cNode, Common.nameProperty) == true)
							{
								Property p = 
									cNode.Properties.GetSingleProperty(Common.nameToContact);
								if (p != null)
								{
									Simias.Storage.Relationship relationship = 
										(Simias.Storage.Relationship) p.Value;

									Node cParentNode = this.collection.GetNodeByID(relationship.NodeID);
									if (this.collection.IsType(cParentNode, Common.contactType) == true)
									{
										cList.Add(this.GetContact(cParentNode.ID));
									}
								}
							}
						}
					}
					finally
					{
						nodeEnum.Dispose();
					}

					return(cList);
				}
				catch{}
				return(null);
			}
			else
			{
				throw new ApplicationException("AddressBook::The address book has not been added to the store");
			}
		}

		/// <summary>
		/// Find a user(s) by last name
		/// </summary>
		/// <param name="searchString"></param>
		/// <param name="searchOperator"></param>
		/// <remarks>
		/// !FINISH API DOC!
		/// </remarks>
		/// <returns>A list of contacts which matched the search string.</returns>
		public IABList SearchLastName( string searchString, Simias.Storage.SearchOp searchOperator )
		{
			if (this.collection != null)
			{
				try
				{
					IABList cList = new IABList();

					ICSEnumerator	nodeEnum = (ICSEnumerator) 
						this.collection.Search(
							Common.familyProperty, 
							searchString, 
							searchOperator).GetEnumerator();

					try
					{
						while(nodeEnum.MoveNext())
						{
							Node cNode = new Node( this.collection, nodeEnum.Current as ShallowNode);
							if (this.collection.IsType(cNode, Common.nameProperty) == true)
							{
								Property p = 
									cNode.Properties.GetSingleProperty(Common.nameToContact);
								if (p != null)
								{
									Simias.Storage.Relationship relationship = 
										(Simias.Storage.Relationship) p.Value;

									Node cParentNode = 
										this.collection.GetNodeByID(relationship.NodeID);
									if (this.collection.IsType(cParentNode, Common.contactType) == true)
									{
										cList.Add(this.GetContact(cParentNode.ID));
									}
								}
							}
						}
					}
					finally
					{
						nodeEnum.Dispose();
					}

					return(cList);
				}
				catch{}
				return(null);
			}
			else
			{
				throw new ApplicationException("AddressBook::The address book has not been added to the store");
			}
		}

		#endregion

		#region IEnumerable
		/// <summary>
		/// Get an enumerator for contacts within the current
		/// address book.
		/// </summary>
		/// <remarks>
		/// !FINISH API DOC!
		/// </remarks>
		/// <returns>An IEnumerator object.</returns>
		public IEnumerator GetEnumerator()
		{
			contactEnum = this.collection.GetEnumerator();
			return(this);
		}

		/*
			Implementation of the iEnumerator members        
		*/

		/// <summary>
		/// Move to the next contact object in the list
		/// address book.
		/// </summary>
		/// <remarks>
		/// !FINISH API DOC!
		/// </remarks>
		/// <returns>TRUE/FALSE.</returns>
		public bool MoveNext()
		{
			try
			{
				while(contactEnum.MoveNext())
				{
					Node cNode = new Node( this.collection, contactEnum.Current as ShallowNode);
					if (this.collection.IsType(cNode, Common.contactType) == true)
					{
						return(true);
					}
				}
			}
			catch{}
			return(false);
		}

		/// <summary>
		/// Get the current contact object
		/// </summary>
		/// <remarks>
		/// !FINISH API DOC!
		/// </remarks>
		/// <returns>current Contact object</returns>
		public object Current
		{
			get
			{
				try
				{
					Node cNode = new Node( this.collection, contactEnum.Current as ShallowNode);
					if (this.collection.IsType(cNode, Common.contactType) == true)
					{
						return((object) this.GetContact(cNode.ID));
					}
				}
				catch{}
				return(null);
			}
		}

		/// <summary>
		/// Resets the enumerator
		/// </summary>
		/// <remarks>
		/// !FINISH API DOC!
		/// </remarks>
		public void Reset()
		{
			//Console.WriteLine("AddressBook::Reset called");
			contactEnum.Reset();
		}
		#endregion
	}

	/// <summary>
	/// Container object that encapsulates an ICSEnumerator.
	/// </summary>
	public class IABList : IEnumerable
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
		internal IABList()
		{
			this.valueList = new ArrayList();
			this.iEnumerator = null;
		}

		/// <summary>
		/// Constructor for the object.
		/// </summary>
		/// <param name="icsEnumerator">Enumerator that contains objects.</param>
		internal IABList( ICSEnumerator icsEnumerator )
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
			if ( valueList != null )
			{
				valueList.Add( value );
			}
			else
			{
				throw new ApplicationException( "AddressBook::Cannot add to this type of enumerator" );
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
			if ( valueList != null )
			{
				return new IABListEnumerator( valueList.GetEnumerator() );
			}
			else
			{
				return iEnumerator;
			}
		}
		#endregion

		/// <summary>
		/// Class used to implement the enumeration for the IABList class.
		/// </summary>
		private class IABListEnumerator : IEnumerator
		{
			#region Class Members
			/// <summary>
			/// Enumerator used to enumerate list items.
			/// </summary>
			private IEnumerator iEnumerator;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructs the object.
			/// </summary>
			/// <param name="iEnumerator">Enumerator from the ICSList object.</param>
			public IABListEnumerator( IEnumerator iEnumerator )
			{
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
				iEnumerator.Reset();
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get { return iEnumerator.Current; }
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
				return iEnumerator.MoveNext();
			}
			#endregion
		}
	}


	/// <summary>
	/// Class used for enumerating all e-mail addresses attached to a contact
	/// </summary>
	public class AddressBookEnumerator : IEnumerable, IEnumerator
	{
		#region Class Members
		private Store			myStore;
		private IEnumerator		bookEnum = null;
		#endregion

		#region Properties
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor used to instantiate this object by means of an enumerator.
		/// </summary>
		/// 
		internal AddressBookEnumerator(Store store)
		{
			this.myStore = store;
			bookEnum = this.myStore.GetEnumerator();
			this.Reset();
		}
		#endregion

		#region IEnumerator Members

		/// <summary>
		/// Get an enumerator for querying address books
		/// </summary>
		public IEnumerator GetEnumerator()
		{
			bookEnum = this.myStore.GetEnumerator();
			return(this);
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is the first string
		/// in the string array
		/// </summary>
		public void Reset()
		{
			bookEnum.Reset();
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
					if(bookEnum != null)
					{
						Node currentNode = (Node) bookEnum.Current;
						AddressBook tmpBook = new AddressBook(this.myStore);
						tmpBook.ToObject(currentNode.ID);
						return((object) tmpBook);
					}
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
			if(bookEnum != null)
			{
				while(bookEnum.MoveNext() == true)
				{
					Node tmpNode = (Node) bookEnum.Current;
					if (tmpNode.Type == "AB:AddressBook")
					{
						return(true);
					}
				}
			}

			return(false);
		}
		#endregion
	}
}


