/***********************************************************************
 *  AddressBookCmd.cs - Simple command line application for driving
 *  address books and contacts.
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
using System.Text.RegularExpressions;
using Simias.Storage;
using Novell.AddressBook;


namespace AddressBookCmd
{
	/// <summary>
	/// Summary description for ABC
	/// </summary>
	public class Abc
	{
		public	bool			timeIt = false;
		public  bool			deleteProperty = false;
		public  bool			deleteContact = false;
		public	bool			exportVCard = false;
		public	bool			listBooks = false;
		public  bool			listContacts = false;
		public	bool			listProperties = false;
		public	bool			updateContact = false;
		public	bool			verbose = false;
		public	string			newAddressBook = null;
		public	string			listContactsBy = null;
		public	string			contactName = null;
		public	string			currentContactName = null;
		public	string			addressBookName = null;
		public	string			vCardFile = null;
		private ArrayList		addContactList = null;
		private	ArrayList		propertyList = null;
		private ArrayList       deletePropertyList = null;
		private ArrayList		addAddressList = null;
		private ArrayList		addNameList = null;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			//
			// TODO: Add code to start application here
			//

			if (args.Length == 0)
			{
				DisplayUsage();
				return;
			}

			Abc		cAbc = new Abc();

			for(int i = 0; i < args.Length; i++)
			{
				args[i].ToLower();

				if (args[i].Length == 2)
				{
					if (args[i] == "/?")
					{
						DisplayUsage();
						return;
					}
					else
					if (args[i] == "/b")
					{
						if (i + 1 <= args.Length)
						{
							if (args[++i] != null)
							{
								cAbc.addressBookName = args[i];
							}
						}
					}
					else
					if (args[i] == "/c")
					{
						if (i + 1 <= args.Length)
						{
							if (args[++i] != null)
							{
								cAbc.currentContactName = args[i];
							}
						}
					}
					else
						if (args[i] == "/l")
					{
						cAbc.listBooks = true;
					}
					else
					if (args[i] == "/t")
					{	
						cAbc.timeIt = true;
					}
					else
					if (args[i] == "/v")
					{
						cAbc.verbose = true;
					}
				}
				else
				if (args[i].Length >= 3)
				{
					if (args[i][0] == '/' && args[i][1] == 'l')
					{
						// list address books
						if (args[i][2] == 'b')
						{
							cAbc.listBooks = true;
						}
						else
						if (args[i][2] == 'c')
						{
							cAbc.listContacts = true;
						}
						else
						if (args[i][2] == 'p')
						{
							cAbc.listProperties = true;
						}
					}
					else
					if (args[i][0] == '/' && args[i][1] == 'a')
					{
						// adding something to the selected address book
						if (args[i][2] == 'v')
						{
							// Add vcard(s)
							if (i + 1 <= args.Length)
							{
								cAbc.vCardFile = args[++i];
							}
						}
						else
						if (args[i][2] == 'b')
						{
							// Add/Create new address book
							if (i + 1 <= args.Length)
							{
								cAbc.newAddressBook = args[++i];
							}
						}
						else
						if (args[i][2] == 'c')
						{
							// Add to the contact list
							if (i + 1 <= args.Length)
							{
								cAbc.AddContact(args[++i]);
							}
						}
						else
						if (args[i][2] == 'a')
						{
							// Add address property
							if (i + 1 <= args.Length)
							{
								cAbc.AddAddress(args[++i]);
							}
						}
						else
						if (args[i][2] == 'n')
						{
							// Add name property
							if (i + 1 <= args.Length)
							{
								cAbc.AddName(args[++i]);
							}
						}
					}
					else
					if (args[i][0] == '/' && args[i][1] == 'd')
					{
						// delete a contact
						if (args[i][2] == 'c')
						{
                            // delete a contact
							if (i + 1 <= args.Length)
							{
								cAbc.contactName = args[++i];
								cAbc.deleteContact = true;
							}
						}
						else
						// delete a property
						if (args[i][2] == 'p')
						{
							if (i + 1 <= args.Length)
							{
								cAbc.AddDeleteProperty(args[++i]);
								cAbc.deleteProperty = true;
							}
						}
					}
					else
					if (args[i][0] == '/' && args[i][1] == 'e')
					{
						// delete a contact
						if (args[i][2] == 'v')
						{
							cAbc.exportVCard = true;
						}
					}
					else
					if (args[i][0] == '/' && args[i][1] == 'u')
					{
						// update a contact
						if (args[i][2] == 'c')
						{
							// add/update a contact
							if (i + 1 <= args.Length)
							{
								cAbc.AddProperty(args[++i]);
								cAbc.updateContact = true;
							}
						}
					}
				}
			}

			cAbc.ProcessCommands();
			Environment.Exit(0);
		}

		static void DisplayUsage()
		{
			Console.WriteLine("AddressBookCmd /b <AddressBook> /l<list> /a<add> /d<delete> /e<export> /t /v");
			Console.WriteLine("   /b <address book> - book to run commands against"); 
			Console.WriteLine("   /c <contact> - contact to execute commands against");
			Console.WriteLine("   /av <vcard file> - add or import vcard file");
			Console.WriteLine("   /aa <address> - add address property (ex. zip=84604;pref;home)");
			Console.WriteLine("   /an <name> - add name property (ex. given=brady;family=anderson)");
			Console.WriteLine("   /ab <address book> - add/create a new address book");
			Console.WriteLine("   /ac <username> - add/create a new contact");
			Console.WriteLine("   /db <address book> - delete an address book");
			Console.WriteLine("   /dc <contact> - delete a contact from selected address book");
			Console.WriteLine("   /dp <property name> - delete the property");
			Console.WriteLine("   /ev <vcard file> - export a vCard");
			Console.WriteLine("   /lb = list address books");
			Console.WriteLine("   /lc = list contacts");
			Console.WriteLine("   /lp = list properties");
			Console.WriteLine("   /t = time it!");
			Console.WriteLine("   /uc <property=value> - update a property in a contact");
			Console.WriteLine("   /v = verbose");
			return;
		}

		private void ListProperties(Contact	cContact)
		{
			Console.WriteLine("ID=" + cContact.ID);
			Console.WriteLine("Username=" + cContact.UserName);

			if (cContact.FN != "")
			{
				Console.WriteLine("FN=" + cContact.FN);
			}

			IABList nList = cContact.GetNames();
			foreach(Name cName in nList)
			{
				Console.Write("N=(");
				Console.Write("ID=" + cName.ID);

				if (cName.Prefix != "")
				{
					Console.Write(";Prefix=");
					Console.Write(cName.Prefix);
				}

				if (cName.Given != "")
				{
					Console.Write(";Given=");
					Console.Write(cName.Given);
				}

				if (cName.Other != "")
				{
					Console.Write(";Other=");
					Console.Write(cName.Other);
				}

				if (cName.Family != "")
				{
					Console.Write(";Family=");
					Console.Write(cName.Family);
				}

				if (cName.Suffix != "")
				{
					Console.Write(";Suffix=");
					Console.Write(cName.Suffix);
				}

				if (cName.Preferred == true)
				{
					Console.Write(";PREF");
				}

				Console.WriteLine(")");
			}

			if (cContact.Nickname != "")
			{
				Console.WriteLine("Nickname=" + cContact.Nickname);
			}

			if (cContact.Title != "")
			{
				Console.WriteLine("Title=" + cContact.Title);
			}

			if (cContact.Role != "")
			{
				Console.WriteLine("Role=" + cContact.Role);
			}

			IABList mList = cContact.GetEmailAddresses();
			foreach(Email tMail in mList)
			{
				Console.Write("EMAIL=" + tMail.Address);
				if (tMail.Preferred == true)
				{
					Console.Write(";PREF");
				}

				if ((tMail.Types & EmailTypes.work) == EmailTypes.work)
				{
					Console.Write(";WORK");
				}

				if ((tMail.Types & EmailTypes.personal) == EmailTypes.personal)
				{
					Console.Write(";PERSONAL");
				}

				if ((tMail.Types & EmailTypes.internet) == EmailTypes.internet)
				{
					Console.Write(";INTERNET");
				}

				if ((tMail.Types & EmailTypes.x400) == EmailTypes.x400)
				{
					Console.Write(";X.400");
				}

				Console.WriteLine("");
			}

			IABList tList = cContact.GetTelephoneNumbers();
			foreach(Telephone tPhone in tList)
			{
				Console.Write("TEL=" + tPhone.Number);

				if (tPhone.Preferred == true)
				{
					Console.Write(";PREF");
				}

				if ((tPhone.Types & PhoneTypes.home) == PhoneTypes.home)
				{
					Console.Write(";HOME");
				}

				if ((tPhone.Types & PhoneTypes.work) == PhoneTypes.work)
				{
					Console.Write(";WORK");
				}

				if ((tPhone.Types & PhoneTypes.other) == PhoneTypes.other)
				{
					Console.Write(";OTHER");
				}

				if ((tPhone.Types & PhoneTypes.cell) == PhoneTypes.cell)
				{
					Console.Write(";CELL");
				}

				if ((tPhone.Types & PhoneTypes.voice) == PhoneTypes.voice)
				{
					Console.Write(";VOICE");
				}

				if ((tPhone.Types & PhoneTypes.fax) == PhoneTypes.fax)
				{
					Console.Write(";FAX");
				}

				if ((tPhone.Types & PhoneTypes.msg) == PhoneTypes.msg)
				{
					Console.Write(";MSG");
				}
				Console.WriteLine("");
			}

			if (cContact.Organization != "")
			{
				Console.WriteLine("ORG=" + cContact.Organization);
			}

			if (cContact.WorkForceID != "")
			{
				Console.WriteLine("WorkForceID=" + cContact.WorkForceID);
			}

			if (cContact.ManagerID != "")
			{
				Console.WriteLine("ManagerID=" + cContact.ManagerID);
			}

			if (cContact.Birthday != "")
			{
				Console.WriteLine("BDAY=" + cContact.Birthday);
			}

			if (cContact.Blog != "")
			{
				Console.WriteLine("BLOG=" + cContact.Blog);
			}

			if (cContact.Url != "")
			{
				Console.WriteLine("URL=" + cContact.Url);
			}

			IABList aList = cContact.GetAddresses();
			foreach(Address cAddress in aList)
			{
				Console.Write("ADR=(");

				if (cAddress.ID != "")
				{
					Console.Write("ID=" + cAddress.ID);
				}

				if (cAddress.Street != "")
				{
					Console.Write(";Street=" + cAddress.Street);
				}

				if (cAddress.Region != "")
				{
					Console.Write(";Region=" + cAddress.Region);
				}

				if (cAddress.Locality != "")
				{
					Console.Write(";Locality=" + cAddress.Locality);
				}

				if (cAddress.PostalCode != "")
				{
					Console.Write(";PostalCode=" + cAddress.PostalCode);
				}

				if (cAddress.Country != "")
				{
					Console.Write(";Country=" + cAddress.Country);
				}

				if (cAddress.Types != 0)
				{
					if ((cAddress.Types & AddressTypes.preferred) == AddressTypes.preferred)
					{
						Console.Write(";PREF");
					}								  
		
					if ((cAddress.Types & AddressTypes.home) == AddressTypes.home)
					{
						Console.Write(";HOME");
					}								  

					if ((cAddress.Types & AddressTypes.work) == AddressTypes.work)
					{
						Console.Write(";WORK");
					}					
			  
					if ((cAddress.Types & AddressTypes.other) == AddressTypes.other)
					{
						Console.Write(";OTHER");
					}
					
					if ((cAddress.Types & AddressTypes.postal) == AddressTypes.postal)
					{
						Console.Write(";POSTAL");
					}								  

					if ((cAddress.Types & AddressTypes.parcel) == AddressTypes.parcel)
					{
						Console.Write(";PARCEL");
					}								  

					if ((cAddress.Types & AddressTypes.dom) == AddressTypes.dom)
					{
						Console.Write(";DOMESTIC");
					}								  

					if ((cAddress.Types & AddressTypes.intl) == AddressTypes.intl)
					{
						Console.Write(";INTL");
					}								  
				}

				Console.WriteLine(")");
			}


			if (cContact.Note != "")
			{
				Console.WriteLine("   Note:          " + cContact.Note);
			}
		}

		public void	AddContact(string name)
		{
			if (this.addContactList == null)
			{
				this.addContactList = new ArrayList();
			}

			this.addContactList.Add(name);
		}

		// Property value string in the following format: property=value
		public void	AddProperty(string propertyValue)
		{
			if (this.propertyList == null)
			{
				this.propertyList = new ArrayList();
			}

			this.propertyList.Add(propertyValue);
		}

		// Address value string in the following format: zip=value;street=value;   etc.
		public void AddAddress(string addressValue)
		{
			if (this.addAddressList == null)
			{
				this.addAddressList = new ArrayList();
			}

			this.addAddressList.Add(addressValue);
		}

		// Name value string in the following format: given=value;family=value;   etc.
		public void AddName(string nameValue)
		{
			if (this.addNameList == null)
			{
				this.addNameList = new ArrayList();
			}

			this.addNameList.Add(nameValue);
		}

		// Property value string in the following format: property=value
		public void	AddDeleteProperty(string propertyValue)
		{
			if (this.deletePropertyList == null)
			{
				this.deletePropertyList = new ArrayList();
			}

			this.deletePropertyList.Add(propertyValue);
		}

		public void	ProcessCommands()
		{
			AddressBook	cAddressBook = null;
			Contact		cContact = null;
			Manager		abManager;
			DateTime	start = DateTime.Now;

			abManager = Manager.Connect( );
			if (addressBookName == null)
			{
				cAddressBook = abManager.OpenDefaultAddressBook();
			}
			else
			{
				try
				{
					cAddressBook = abManager.GetAddressBookByName(addressBookName);
				}
				catch
				{
					if (verbose == true)
					{
						Console.WriteLine("Address Book: {0} does not exist.", addressBookName);
					}
				}
			}

			if (cAddressBook == null)
			{
				return;
			}

			// Instantiate the current contact object
			if (currentContactName != null)
			{
				IABList cList = cAddressBook.SearchUsername(currentContactName, Property.Operator.Equal);

				IEnumerator	iEnum = cList.GetEnumerator();
				if(iEnum.MoveNext())
				{
					cContact = (Contact) iEnum.Current;
				}
			}

			if (listBooks == true)
			{
				foreach(AddressBook tmpBook in abManager)
				{
					Console.Write(tmpBook.Name);
					if (tmpBook.Default)
					{
						Console.Write(" - default");
					}

					Console.WriteLine("");
				}
			}

			if (this.addContactList != null)
			{
				foreach(String username in this.addContactList)
				{
					if (verbose == true)
					{
						Console.WriteLine("   Adding contact: " + username);
					}

					Contact	tmpContact = new Contact();
					tmpContact.UserName = username;
					cAddressBook.AddContact(tmpContact);
					tmpContact.Commit();
				}
			}

			if (deleteProperty == true && cContact != null)
			{
				bool	cUpdated = false;
				foreach(string propertyString in this.deletePropertyList)
				{
					Regex o = new Regex(@"=");
					IEnumerator enumTokens = o.Split(propertyString).GetEnumerator();
					while(enumTokens.MoveNext())
					{
						string token = (string) enumTokens.Current;
						token.ToLower();

						if (token == "organization")
						{
							cContact.Organization = null;
							cUpdated = true;
						}
						else
						if (token == "blog")
						{
							cContact.Blog = null;
							cUpdated = true;
						}
						else
						if (token == "url")
						{
							cContact.Url = null;
							cUpdated = true;
						}
						else
						if (token == "title")
						{
							cContact.Title = null;
							cUpdated = true;
						}
						else
						if (token == "role")
						{
							cContact.Role = null;
							cUpdated = true;
						}
						else
						if (token == "workforceid")
						{
							cContact.WorkForceID = null;
							cUpdated = true;
						}
						else
						if (token == "birthday")
						{
							cContact.Birthday = null;
							cUpdated = true;
						}
						else
						if (token == "email")
						{
							// email property line ex: email=banderso@novell.com
							if (enumTokens.MoveNext())
							{
								IABList eAddresses = cContact.GetEmailAddresses();
								foreach (Email tmpMail in eAddresses)
								{
									if (tmpMail.Address == (string) enumTokens.Current)
									{
										if (verbose == true)
										{
											Console.WriteLine("   Deleting e-mail address: " + tmpMail.Address);
										}
										tmpMail.Delete();
										cUpdated = true;
										break;
									}
								}
							}
						}
						else
						if (token == "managerid")
						{
							cContact.ManagerID = null;
							cUpdated = true;
							
						}
						else
						if (token == "note")
						{
							cContact.Note = null;
							cUpdated = true;
						}
						else
						if (token == "nickname")
						{
							cContact.Nickname = null;
							cUpdated = true;
						}
					}
				}

				if (cUpdated == true)
				{
					cContact.Commit();
				}
			}


			// Adding addresses to a contact?
			if (this.addAddressList != null && cContact != null)
			{
				bool			cUpdated = false;
				AddressTypes	addrTypes = 0;
				Address	cAddress = new Address();
				foreach(string addressString in this.addAddressList)
				{
					Regex o = new Regex(@";");
					IEnumerator enumTokens = o.Split(addressString).GetEnumerator();
					while(enumTokens.MoveNext())
					{
						string token = (string) enumTokens.Current;

						Regex t = new Regex(@"=");
						IEnumerator memberValueTokens = t.Split(token).GetEnumerator();

						if(memberValueTokens.MoveNext())
						{
							string member = (string) memberValueTokens.Current;
							member.ToLower();
							string memberValue;

							if (memberValueTokens.MoveNext())
							{
								memberValue = (string) memberValueTokens.Current;
							}
							else
							{
								memberValue = null;
							}

							if (member == "zip")
							{
								cAddress.PostalCode = memberValue;
								cUpdated = true;
							}
							else
							if (member == "postalcode")
							{
								cAddress.PostalCode = memberValue;
								cUpdated = true;
							}
							else
							if (member == "region")
							{
								cAddress.Region = memberValue;
								cUpdated = true;
							}
							else
							if (member == "locality")
							{
								cAddress.Locality = memberValue;
								cUpdated = true;
							}
							else
							if (member == "street")
							{
								cAddress.Street = memberValue;
								cUpdated = true;
							}
							else
							if (member == "country")
							{
								cAddress.Country = memberValue;
								cUpdated = true;
							}
							else
							if (member == "mailstop")
							{
								cAddress.MailStop = memberValue;
								cUpdated = true;
							}
							else
							if (member == "preferred" || member == "pref")
							{
								addrTypes |= AddressTypes.preferred;
								cUpdated = true;
							}
							else
							if (member == "work")
							{
								addrTypes |= AddressTypes.work;
								cUpdated = true;
							}
							else
							if (member == "home")
							{
								addrTypes |= AddressTypes.home; 
								cUpdated = true;
							}
							else
							if (member == "other")
							{
								addrTypes |= AddressTypes.other;
								cUpdated = true;
							}
							else
							if (member == "dom")
							{
								addrTypes |= AddressTypes.dom;
								cUpdated = true;
							}
							else
							if (member == "intl")
							{
								addrTypes |= AddressTypes.intl;
								cUpdated = true;
							}
							else
							if (member == "parcel")
							{
								addrTypes |= AddressTypes.parcel;
								cUpdated = true;
							}
							else
							if (member == "postal")
							{
								addrTypes |= AddressTypes.postal;
								cUpdated = true;
							}
						}
					}
				}

				if (cUpdated == true)
				{
					if (verbose == true)
					{
						Console.WriteLine("Adding new Address to Contact: " + cContact.UserName);
					}

					if (addrTypes != 0)
					{
						cAddress.Types = addrTypes;

						if (verbose == true)
						{
							Console.WriteLine("   Types=" + cAddress.Types);
						}
					}

					cContact.AddAddress(cAddress);
					cContact.Commit();
				}
			}

			// Adding name(s) to a contact?
			if (this.addNameList != null && cContact != null)
			{
				bool	cUpdated = false;
				foreach(string nameString in this.addNameList)
				{
					Name cName = new Name();

					Regex o = new Regex(@";");
					IEnumerator enumTokens = o.Split(nameString).GetEnumerator();
					while(enumTokens.MoveNext())
					{
						string token = (string) enumTokens.Current;

						Regex t = new Regex(@"=");
						IEnumerator memberValueTokens = t.Split(token).GetEnumerator();

						if(memberValueTokens.MoveNext())
						{
							string member = (string) memberValueTokens.Current;
							member.ToLower();
							string memberValue;

							if (memberValueTokens.MoveNext())
							{
								memberValue = (string) memberValueTokens.Current;
							}
							else
							{
								memberValue = null;
							}

							if (member == "given")
							{
								cName.Given = memberValue;
								cUpdated = true;
							}
							else
							if (member == "family")
							{
								cName.Family = memberValue;
								cUpdated = true;
							}
							else
							if (member == "other")
							{
								cName.Other = memberValue;
								cUpdated = true;
							}
							else
							if (member == "prefix")
							{
								cName.Prefix = memberValue;
								cUpdated = true;
							}
							else
							if (member == "suffix")
							{
								cName.Suffix = memberValue;
								cUpdated = true;
							}
							else
							if (member == "preferred")
							{
								cName.Preferred = true;
							}
							else
							if (member == "pref")
							{
								cName.Preferred = true;
							}
						}
					}

					if (cUpdated == true)
					{
						if (verbose == true)
						{
							Console.WriteLine("Adding new Name to Contact: " + cContact.UserName);
						}
						cContact.AddName(cName);
						cContact.Commit();
					}
				}
			}

			if (listProperties == true && cContact != null)
			{
				ListProperties(cContact);
			}

			if (vCardFile != null)
			{
				if (verbose == true)
				{
					Console.WriteLine("Importing vCard(s) from file: " + vCardFile);
				}

				cAddressBook.ImportVCard(vCardFile);
			}

			if (newAddressBook != null)
			{
				if (verbose == true)
				{
					Console.WriteLine("Creating Address Book: " + newAddressBook);
				}

				AddressBook nBook = 
					new AddressBook(newAddressBook, AddressBookType.Private, AddressBookRights.ReadWrite, false);

				abManager.AddAddressBook(nBook);
			}

			if (listContacts == true)
			{
				Console.WriteLine("{0} address book contacts:", cAddressBook.Name);
				foreach(Contact tmpContact in cAddressBook)
				{
					Console.WriteLine("   " + tmpContact.UserName);
				}
			}

			if (updateContact == true && cContact != null)
			{
				bool	cUpdated = false;
				foreach(string propertyString in this.propertyList)
				{

					Regex o = new Regex(@"=");
					IEnumerator enumTokens = o.Split(propertyString).GetEnumerator();
					while(enumTokens.MoveNext())
					{
						string token = (string) enumTokens.Current;
						token.ToLower();

						if (token == "organization")
						{
							if (enumTokens.MoveNext())
							{
								cContact.Organization = (string) enumTokens.Current;
								cUpdated = true;
							}
						}
						else
						if (token == "blog")
						{
							if (enumTokens.MoveNext())
							{
								if (verbose == true)
								{
									Console.WriteLine("Updating the blog attribute to: " + (string) enumTokens.Current);
								}
								cContact.Blog = (string) enumTokens.Current;
								cUpdated = true;
							}
						}
						else
						if (token == "url")
						{
							if (enumTokens.MoveNext())
							{
								cContact.Url = (string) enumTokens.Current;
								cUpdated = true;
							}
						}
						else
						if (token == "title")
						{
							if (enumTokens.MoveNext())
							{
								cContact.Title = (string) enumTokens.Current;
								cUpdated = true;
							}
						}
						else
						if (token == "role")
						{
							if (enumTokens.MoveNext())
							{
								cContact.Role = (string) enumTokens.Current;
								cUpdated = true;
							}
						}
						else
						if (token == "workforceid")
						{
							if (enumTokens.MoveNext())
							{
								cContact.WorkForceID = (string) enumTokens.Current;
								cUpdated = true;
							}
						}
						else
						if (token == "birthday")
						{
							if (enumTokens.MoveNext())
							{
								cContact.Birthday = (string) enumTokens.Current;
								cUpdated = true;
							}
						}
						else
						if (token == "email")
						{
							// email property line ex: email=banderso@novell.com;home;preferred
							if (enumTokens.MoveNext())
							{
								Regex x = new Regex(@";");

								IEnumerator enumTokens1 = x.Split((string) enumTokens.Current).GetEnumerator();

								if (enumTokens1.MoveNext())
								{
									EmailTypes	eTypes = 0;
									Email cMail = new Email();
									cMail.Address = (string) enumTokens1.Current;
									cUpdated = true;

									if (verbose == true)
									{
										Console.WriteLine("Adding email address: " + cMail.Address);
									}

									while(enumTokens1.MoveNext())
									{
										string typeToken = (string) enumTokens1.Current;
										typeToken.ToLower();

										if (typeToken == "preferred")
										{
											if (verbose == true)
											{
												Console.WriteLine("   Preferred");
											}
											cMail.Preferred = true;
										}
										else
										if (typeToken == "work")
										{
											if (verbose == true)
											{
												Console.WriteLine("   Work");
											}
											eTypes |= EmailTypes.work;
										}
										else
										if (typeToken == "personal")
										{
											if (verbose == true)
											{
												Console.WriteLine("   Personal");
											}
											eTypes |= EmailTypes.personal;
										}
										else
										if (typeToken == "other")
										{
											eTypes |= EmailTypes.other;
										}
									}

									if (eTypes != 0)
									{
										cMail.Types = eTypes;
									}

									cContact.AddEmailAddress(cMail);
								}
							}
						}
						else
						if (token == "managerid")
						{
							if (enumTokens.MoveNext())
							{
								cContact.ManagerID = (string) enumTokens.Current;
								cUpdated = true;
							}
						}
						else
						if (token == "note")
						{
							if (enumTokens.MoveNext())
							{
								cContact.Note = (string) enumTokens.Current;
								cUpdated = true;
							}
						}
						else
						if (token == "nickname")
						{
							if (enumTokens.MoveNext())
							{
								cContact.Nickname = (string) enumTokens.Current;
								cUpdated = true;
							}
						}
					}
				}

				if (cUpdated == true)
				{
					cContact.Commit();
				}
			}

			if (deleteContact == true)
			{
				IABList	cList;

				if (verbose == true)
				{
					Console.WriteLine("deleting contact: " + contactName);
				}

				cList = cAddressBook.SearchUsername(contactName, Property.Operator.Equal);
				foreach(Contact tmpContact in cList)
				{
					tmpContact.Delete();
				}
			}

			if (exportVCard == true)
			{
				if (cContact != null)
				{
					string  vCardFileName = "";
					if (cContact.FN != "")
					{
						vCardFileName += cContact.FN;
						vCardFileName += ".vcf";
					}
					else
					{
						vCardFileName = cContact.ID + ".vcf";
					}

					if (verbose == true)
					{
						Console.WriteLine("Exporting vCard: " + vCardFileName);
					}
					cContact.ExportVCard(vCardFileName);
				}
			}

			if (timeIt == true)
			{
				DateTime stop = DateTime.Now;

				// Difference in days, hours, and minutes.
				TimeSpan ts = stop - start;
				Console.WriteLine("");
				Console.WriteLine("Command(s) completed in: ");
				Console.WriteLine("   {0} seconds", ts.TotalSeconds);
				Console.WriteLine("   {0} milliseconds", ts.TotalMilliseconds);
			}
		}
	}
}
