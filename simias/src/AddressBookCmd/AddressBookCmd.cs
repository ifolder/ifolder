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
		public  bool			deleteContact = false;
		public	bool			exportVCard = false;
		public	bool			listBooks = false;
		public  bool			listContacts = false;
		public	bool			listProperties = false;
		public	bool			updateContact = false;
		public	bool			verbose = true;
		public	string			newAddressBook = null;
		public	string			listContactsBy = null;
		public	string			contactName = null;
		public	string			currentContactName = null;
		public	string			addressBookName = null;
		public	string			vCardFile = null;
		private	ArrayList		propertyList = null;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			//
			// TODO: Add code to start application here
			//

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
							// delete a contact
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
		}

		static void DisplayUsage()
		{
			Console.WriteLine("AddressBookCmd /b <AddressBook> /l<list> /a<add> /d<delete> /e<export> /t /v");
			Console.WriteLine("   /b <address book> - book to run commands against"); 
			Console.WriteLine("   /c <contact> - contact to execute commands against");
			Console.WriteLine("   /av <vcard file> - add or import vcard file");
			Console.WriteLine("   /ab <address book> - add/create a new address book");
			Console.WriteLine("   /db <address book> - delete an address book");
			Console.WriteLine("   /dc <contact> - delete a contact from selected addres book");
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
			Console.WriteLine("Listing properties for contact: " + cContact.UserName);
			if (cContact.FN != "")
			{
				Console.WriteLine("   FN: " + cContact.FN);
			}

			/*
			if (cContact.EMail != "")
			{
				Console.WriteLine("   Email: " + cContact.EMail);
			}
			*/

			if (cContact.Title != "")
			{
				Console.WriteLine("   Title: " + cContact.Title);
			}

			if (cContact.Role != "")
			{
				Console.WriteLine("   Role: " + cContact.Role);
			}

			IABList mList = cContact.GetEmailAddresses();
			foreach(Email tMail in mList)
			{
				Console.Write("   Email: " + tMail.Address);
				if ((tMail.Types & EmailTypes.work) == EmailTypes.work)
				{
					Console.Write(" WORK");
				}

				if ((tMail.Types & EmailTypes.personal) == EmailTypes.personal)
				{
					Console.Write(" PERSONAL");
				}

				if (tMail.Preferred == true)
				{
					Console.Write(" PREFERRED");
				}

				Console.WriteLine("");
			}

			IABList tList = cContact.GetTelephoneNumbers();
			foreach(Telephone tPhone in tList)
			{
				Console.Write("   Telephone: " + tPhone.Number);
				if ((tPhone.Types & PhoneTypes.home) == PhoneTypes.home)
				{
					Console.Write(" HOME");
				}

				if ((tPhone.Types & PhoneTypes.work) == PhoneTypes.work)
				{
					Console.Write(" WORK");
				}

				if ((tPhone.Types & PhoneTypes.other) == PhoneTypes.other)
				{
					Console.Write(" OTHER");
				}

				if ((tPhone.Types & PhoneTypes.cell) == PhoneTypes.cell)
				{
					Console.Write(" CELL");
				}

				if ((tPhone.Types & PhoneTypes.voice) == PhoneTypes.voice)
				{
					Console.Write(" VOICE");
				}

				if (tPhone.Preferred == true)
				{
					Console.Write(" PREFERRED");
				}

				Console.WriteLine("");
			}

			if (cContact.WorkForceID != "")
			{
				Console.WriteLine("   Work Force ID: " + cContact.WorkForceID);
			}

			if (cContact.Organization != "")
			{
				Console.WriteLine("   Organization: " + cContact.Organization);
			}

			if (cContact.ManagerID != "")
			{
				Console.WriteLine("   Manager ID: " + cContact.ManagerID);
			}

			if (cContact.Blog != "")
			{
				Console.WriteLine("   Blog Address: " + cContact.Blog);
			}

			if (cContact.Url != "")
			{
				Console.WriteLine("   Url Address: " + cContact.Url);
			}

			if (cContact.Note != "")
			{
				Console.WriteLine("   Note: " + cContact.Note);
			}
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
				cAddressBook = abManager.GetAddressBookByName(addressBookName);
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
							if (enumTokens.MoveNext())
							{
								cContact.EMail = (string) enumTokens.Current;
								cUpdated = true;
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
								cContact.NickName = (string) enumTokens.Current;
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
