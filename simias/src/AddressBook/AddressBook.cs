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
	public enum AddressBookType
	{ 
		/// <summary>
		/// indicates a global address book - ex. corporate
		/// </summary>
		Global, 

		/// <summary>
		/// indicates a private address book - non shared
		/// </summary>
		Private, 

		/// <summary>
		/// indicates a personal but shared address book
		/// </summary>
		Shared
	};

	/// <summary>
	/// User rights on an address book
	/// </summary>
	public enum AddressBookRights
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
		private		string				domain;

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

		internal void BuildVCardAddress(Contact cContact, string addrLine)
		{
			Address address = new Address();

			string  addressLine = "";
			string	typesLine = "";
			string  tmpString;

			Regex o = new Regex(@":");
			IEnumerator enumAddrLine = o.Split(addrLine).GetEnumerator();

			if(enumAddrLine.MoveNext())
			{
				typesLine = (string) enumAddrLine.Current;
				if (enumAddrLine.MoveNext())
				{
					addressLine = (string) enumAddrLine.Current;
				}
			}

			if (typesLine != "")
			{
				uint	addrTypes = 0;

				// BUGBUG change this to a match
				Regex p = new Regex(@";|=");
				IEnumerator enumTypes = p.Split(typesLine).GetEnumerator();

				while(enumTypes.MoveNext())
				{
					tmpString = (string) enumTypes.Current;
					tmpString.ToUpper();

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
			}

			if (addressLine != "")
			{
				Regex a = new Regex(@";");
				IEnumerator enumAddress = a.Split(addressLine).GetEnumerator();

				// PO box
				if( enumAddress.MoveNext())
				{
					if ((string) enumAddress.Current != "")
					{
						address.PostalBox = (string) enumAddress.Current;
					}

					// Extended Address
					if (enumAddress.MoveNext())
					{
						if ((string) enumAddress.Current != "")
						{
							address.ExtendedAddress = (string) enumAddress.Current;
						}

						// Street Address
						if (enumAddress.MoveNext())
						{
							if ((string) enumAddress.Current != "")
							{
								address.Street = (string) enumAddress.Current;
							}

							// Locality/City
							if (enumAddress.MoveNext())
							{
								if ((string) enumAddress.Current != "")
								{
									address.Locality = (string) enumAddress.Current;
								}

								// Region/State/Province
								if (enumAddress.MoveNext())
								{
									if ((string) enumAddress.Current != "")
									{
										address.Region = (string) enumAddress.Current;
									}

									// Postal code
									if (enumAddress.MoveNext())
									{
										if ((string) enumAddress.Current != "")
										{
											address.PostalCode = (string) enumAddress.Current;
											cContact.AddAddress(address);
										}

										// Country
										if (enumAddress.MoveNext())
										{
											if ((string) enumAddress.Current != "")
											{
												address.Country = (string) enumAddress.Current;
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Method: Add
		/// Abstract: Address books are added through the AddressBook.Manager class.
		/// </summary>
		internal void Add(Store store)
		{
			// Create the address book in the default
			this.collection = store.CreateCollection(this.friendlyName, "AB:AddressBook");

			this.collection.Properties.AddProperty( "AB:AddressBookType", (Int32) this.addressBookType);
			this.collection.Properties.AddProperty( "AB:AddressBookRights", (Int32) this.addressBookRights);
			if (this.defaultBook == true)
			{
				this.collection.Properties.AddProperty( "AB:Default", this.defaultBook);
			}

			this.collection.Commit(true);
			this.changed = false;
		}

		/// <summary>
		/// Method: ToObject
		/// Abstract: Method to pull property data from the collection store
		/// and populate the members in the address book object.
		/// </summary>
		internal void ToObject(string addressBookID)
		{
			if (this.store == null)
			{
				throw new ApplicationException("AddressBook::Missing store handle");
			}

			Collection tmpCollection = this.store.GetCollectionById(addressBookID);

			// Make sure the returned collection is in fact an address book
			if (tmpCollection.Type != "AB:AddressBook")
			{
				throw new ApplicationException("AddressBook::Invalid ID parameter - not an address book");
			}

			this.collection = tmpCollection;
			this.friendlyName = tmpCollection.Name;

			// Load up the previously persisted properties
			try
			{
				this.addressBookType = (AddressBookType) 
					this.collection.Properties.GetSingleProperty("AB:AddresssBookType").Value;
			}
			catch{}

			try
			{
				this.addressBookRights = (AddressBookRights)
					this.collection.Properties.GetSingleProperty("AB:AddresssBookRights").Value;
			}
			catch{}

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

			// defaultBook is not mandatory
			try
			{
				this.defaultBook = 
					Convert.ToBoolean(this.collection.Properties.GetSingleProperty("AB:Default").ToString());
			}
			catch
			{
				this.defaultBook = false;
			}

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
					return(this.domain);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				if (this.addressBookRights != AddressBookRights.ReadOnly)
				{
					this.domain = value;
					this.changed = true;
				}
			}
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

		#region Public Methods

		/// <summary>
		/// Method: AddContact
		/// Abstract: Method to create a contact in the store
		/// this method binds the contact to a known identityId
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

			this.collection.Commit(true);
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

			this.collection.Delete(true);
			this.deleted = true;
		}

		/// <summary>
		/// Retrieve a contact using a specified id.
		/// </summary>
		/// <param name="id">Specified contact ID</param>
		/// <remarks>
		/// If a contact is not found an application exception is raised
		/// </remarks>
		/// <returns>A Contact object with at minimum a valid username property.</returns>
		public Contact GetContact(string id)
		{
			if (this.collection != null)
			{
				Contact contact = new Contact();

				// ToObject will throw a not found exception if a contact
				// can't be found with the specified id
				contact.ToObject(this, id);
				return(contact);
			}
			else
			{
				throw new ApplicationException(Common.addressBookExceptionHeader + "The address book has not been added to the store");
			}
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
			try
			{
				bool	foundEnd = false;
				String	line;

				// Make sure this is a valid vCard
				line = vCardStream.ReadLine();
				line.ToUpper();
				if (line == "BEGIN:VCARD")
				{
					// Now check the version
					line = vCardStream.ReadLine();
					line.ToUpper();

					if (line == "VERSION:2.1" || line == "VERSION:3.0")
					{
						Contact cContact = new Contact();

						// Read and display lines from the file until the end of 
						// the file is reached.
						while ((line = vCardStream.ReadLine()) != null) 
						{	
							if (line == "END:VCARD")
							{
								foundEnd = true;
								break;
							}

							Regex o = new Regex(@";|:");
							IEnumerator enumTokens = o.Split(line).GetEnumerator();
							while(enumTokens.MoveNext())
							{
								string token = (string) enumTokens.Current;
								token.ToUpper();

								if (token == "N")
								{
									Regex n = new Regex(@":");
									IEnumerator enumProperties = n.Split(line).GetEnumerator();

									// Prime the enumerator and move to the first token
									if (enumProperties.MoveNext())
									{
										// Move past the token(s) before the : and on to the property values
										if (enumProperties.MoveNext())
										{
											Regex nt = new Regex(@";");
											IEnumerator nameTokens = 
												nt.Split((string) enumProperties.Current).GetEnumerator();
											if(nameTokens.MoveNext())
											{
												this.BuildVCardName(cContact, nameTokens);
											}
										}
									}
								}
								else
								if (token == "ADR")
								{
									if (enumTokens.MoveNext())
									{
										this.BuildVCardAddress(cContact, line);
									}
								}
								else
								if (token == "EMAIL")
								{
									bool			nextResults;
									Email			tmpMail = new Email();
									//EmailTypes		mailTypes;
									string			tmpString;
									uint			mailTypes = 0;

									nextResults = enumTokens.MoveNext();
									while(nextResults == true)
									{
										if ((string) enumTokens.Current != "")
										{
											tmpString = (string) enumTokens.Current;
											nextResults = enumTokens.MoveNext();
											if(nextResults == false)
											{
												tmpMail.Address = tmpString;

												// vCard doesn't have work/personal types
												// attached to an e-mail address so if the
												// e-mail isn't preferred, we'll default to 
												// "other"
												if (tmpMail.Preferred == false)
												{
													mailTypes |= (uint) EmailTypes.other;
												}

												if (mailTypes != 0)
												{
													tmpMail.Types = (EmailTypes) mailTypes;
												}

												cContact.AddEmailAddress(tmpMail);
											}
											else
											{
												tmpString.ToUpper();
												if(tmpString == "PREF")
												{
													tmpMail.Preferred = true;
													mailTypes |= (uint) EmailTypes.work;
												}
												else
												if (tmpString == "INTERNET")
												{
													mailTypes &= (uint) ~EmailTypes.x400;
													mailTypes |= (uint) EmailTypes.internet;
												}
												else
												if (tmpString == "X400")
												{
													mailTypes &= (uint) ~EmailTypes.internet;
													mailTypes |= (uint) EmailTypes.x400;
												}
											}
										}
									}
								}
								else
								if (token == "TEL")
								{
									bool			nextResults;
									Telephone		tmpPhone = new Telephone();
									string			tmpString;
									uint			phoneTypes = 0;

									nextResults = enumTokens.MoveNext();
									while(nextResults == true)
									{
										if ((string) enumTokens.Current != "")
										{
											tmpString = (string) enumTokens.Current;
											nextResults = enumTokens.MoveNext();
											if(nextResults == false)
											{
												tmpPhone.Number = tmpString;

												// If no types exist in the vCard stay with the
												// default types
												if (phoneTypes != 0)
												{
													tmpPhone.Types = (PhoneTypes) phoneTypes;
												}

												cContact.AddTelephoneNumber(tmpPhone);
											}
											else
											{
												tmpString.ToUpper();
												if(tmpString == "PREF")
												{
													phoneTypes |= (uint) PhoneTypes.preferred;
												}
												else
												if (tmpString == "VOICE")
												{
													phoneTypes |= (uint) PhoneTypes.voice;
												}
												else
												if (tmpString == "MSG")
												{
													phoneTypes |= (uint) PhoneTypes.msg;
												}
												else
												if (tmpString == "WORK")
												{
													phoneTypes |= (uint) PhoneTypes.work;
												}
												else
												if (tmpString == "HOME")
												{
													phoneTypes |= (uint) PhoneTypes.home;
												}
												else
												if (tmpString == "CELL")
												{
													phoneTypes |= (uint) PhoneTypes.cell;
												}
												else
												if (tmpString == "VOIP")
												{
													phoneTypes |= (uint) PhoneTypes.voip;
												}
											}
										}
									}
								}
								else
								if (token == "NICKNAME")
								{
									if (enumTokens.MoveNext())
									{
										if ((string) enumTokens.Current != "")
										{
											cContact.Nickname = (string) enumTokens.Current;
										}
									}
								}
								else
								if (token == "TITLE")
								{
									if (enumTokens.MoveNext())
									{
										if ((string) enumTokens.Current != "")
										{
											cContact.Title = (string) enumTokens.Current;
										}
									}
								}
								else
								if (token == "NOTE")
								{
									if (enumTokens.MoveNext())
									{
										if ((string) enumTokens.Current != "")
										{
											cContact.Note = (string) enumTokens.Current;
										}
									}
								}
								else
								if (token == "BDAY")
								{
									if (enumTokens.MoveNext())
									{
										if ((string) enumTokens.Current != "")
										{
											cContact.Birthday = (string) enumTokens.Current;
										}
									}
								}
								else
								if (token == "ORG")
								{
									if (enumTokens.MoveNext())
									{
										if ((string) enumTokens.Current != "")
										{
											cContact.Organization = (string) enumTokens.Current;
										}
									}
								}
								else
								if (token == "URL")
								{
									if (enumTokens.MoveNext())
									{
										if ((string) enumTokens.Current != "")
										{
											cContact.Url = (string) enumTokens.Current;
										}
									}
								}
								else
								if (token == "PHOTO")
								{
									byte[] binaryData = null;

									try
									{
										// BUGBUG need to add the encoding type
										Regex ph = new Regex(@":");
										IEnumerator enumTokens1 = ph.Split(line).GetEnumerator();

										// First line (Type and Encoding)
										enumTokens1.MoveNext();

										// Second line actual base 64 encoded data
										enumTokens1.MoveNext();
										binaryData = Convert.FromBase64String((string) enumTokens1.Current);

										MemoryStream tmpPhoto = new MemoryStream();
										BinaryWriter bWriter = new BinaryWriter(tmpPhoto);

										bWriter.Write(binaryData, 0, binaryData.Length);
										cContact.ImportPhoto(tmpPhoto);
									}
									catch(Exception e)
									{
										Console.WriteLine(e.Message);
										Console.WriteLine(e.StackTrace);
									}
									break;
								}
								else
								if (token == "X-NAB-USERNAME")
								{
									if (enumTokens.MoveNext())
									{
										if ((string) enumTokens.Current != "")
										{
											cContact.UserName = (string) enumTokens.Current;
										}
									}
								}
								else
								{
									break;
								}
							}
						}

						// If we found the END:VCARD tag add the contact
						if (foundEnd == true)
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
								return(cContact);
							}
							catch{}
						}
					}
				}
			}
			catch{}
			return(null);
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
		public IABList SearchUsername( string searchString, Property.Operator queryOperator )
		{
			if (this.collection != null)
			{
				try
				{
					IABList cList = new IABList();
					//ArrayList cList = new ArrayList();

					ICSEnumerator	nodeEnum = (ICSEnumerator) 
						this.collection.Search(Property.ObjectName, searchString, queryOperator).GetEnumerator();

					while(nodeEnum.MoveNext())
					{
						Node cNode = (Node) nodeEnum.Current;
						if (cNode.Type == Common.contactType)
						{
							cList.Add(this.GetContact(cNode.Id));
						}
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
		public IABList SearchEmail( string searchString, Property.Operator queryOperator )
		{
			if (this.collection != null)
			{
				try
				{
					IABList cList = new IABList();

					ICSEnumerator	nodeEnum = (ICSEnumerator) 
						this.collection.Search(Common.emailProperty, searchString, queryOperator).GetEnumerator();

					while(nodeEnum.MoveNext())
					{
						Node cNode = (Node) nodeEnum.Current;
						if (cNode.Type == Common.contactType)
						{
							cList.Add(this.GetContact(cNode.Id));
						}
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
		/// <param name="queryOperator"></param>
		/// <remarks>
		/// !FINISH API DOC!
		/// </remarks>
		/// <returns>A list of contacts which matched the search string.</returns>
		public IABList SearchFirstName( string searchString, Property.Operator queryOperator )
		{
			if (this.collection != null)
			{
				try
				{
					IABList cList = new IABList();

					ICSEnumerator	nodeEnum = (ICSEnumerator) 
						this.collection.Search(Property.ObjectName, searchString, queryOperator).GetEnumerator();

					while(nodeEnum.MoveNext())
					{
						Node cNode = (Node) nodeEnum.Current;
						if (cNode.Type == Common.nameProperty)
						{
							cList.Add(this.GetContact(cNode.GetParent().Id));
						}
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
		/// <param name="queryOperator"></param>
		/// <remarks>
		/// !FINISH API DOC!
		/// </remarks>
		/// <returns>A list of contacts which matched the search string.</returns>
		public IABList SearchLastName( string searchString, Property.Operator queryOperator )
		{
			if (this.collection != null)
			{
				try
				{
					IABList cList = new IABList();

					ICSEnumerator	nodeEnum = (ICSEnumerator) 
						this.collection.Search(Common.familyProperty, searchString, queryOperator).GetEnumerator();

					while(nodeEnum.MoveNext())
					{
						Node cNode = (Node) nodeEnum.Current;
						if (cNode.Type == Common.nameProperty)
						{
							cList.Add(this.GetContact(cNode.GetParent().Id));
						}
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
			//
			// TODO
			// Make sure the node is an address book contact
			//

			try
			{
				while(contactEnum.MoveNext())
				{
					Node tmpNode = (Node) contactEnum.Current;
					if (tmpNode.Type == Common.contactType)
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
					Node cNode = (Node) contactEnum.Current;
					return((object) this.GetContact(cNode.Id));
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
						tmpBook.ToObject(currentNode.Id);
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


