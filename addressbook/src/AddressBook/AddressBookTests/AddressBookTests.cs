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
using NUnit.Framework;
using Simias;
using Simias.Storage;
using Novell.AddressBook;

namespace Novell.AddressBook.Tests
{
	/// <summary>
	/// Summary description for Iteration0Tests.
	/// </summary>
	[TestFixture]
	public class Iteration0Tests
	{
		private string basePath = Path.Combine( Directory.GetCurrentDirectory(), "AddressBookTestDir" );

		Manager abManager;

		/// <summary>
		/// 
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			Console.WriteLine("Init called");

			Console.WriteLine("Connecting to the AddressBook manager");
			abManager = ABManager.Connect( new Configuration( basePath ) );
			//abManager = ABManager.Connect( new Configuration ("C:\\"));
			//abManager = ABManager.Connect();

			Console.WriteLine("Init exit");
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void EnumerateMyAddressBooks()
		{
			/*
			aBManager.CreateAddressBook("My Address Book 1", AddressBook.AddressBookType.Private, AddressBook.AddressBookRights.ReadWrite);
			aBManager.CreateAddressBook("My Address Book 2", AddressBook.AddressBookType.Private, AddressBook.AddressBookRights.ReadWrite);
			aBManager.CreateAddressBook("My Address Book 3", AddressBook.AddressBookType.Private, AddressBook.AddressBookRights.ReadWrite);
			aBManager.CreateAddressBook("My Address Book 4", AddressBook.AddressBookType.Private, AddressBook.AddressBookRights.ReadWrite);
			aBManager.CreateAddressBook("My Address Book 5", AddressBook.AddressBookType.Private, AddressBook.AddressBookRights.ReadWrite);
			Console.WriteLine("Enumerating current address books");
			foreach(AddressBook addressBook in aBManager)
			{
				Console.WriteLine("book name: {0}", addressBook.Name);
				aBManager.DeleteAddressBook(addressBook.ID);
			}
			*/
			Console.WriteLine("");
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void OpenDefaultBook()
		{
			Console.WriteLine("OpenDefaultBook called");
			AddressBook defaultBook = abManager.OpenDefaultAddressBook();
			Console.WriteLine("Default Book: " + defaultBook.Name);
			Console.WriteLine("ID:           " + defaultBook.ID);
			Console.WriteLine("Type:         " + defaultBook.Type.ToString());
			Console.WriteLine("Rights:       " + defaultBook.Rights.ToString());
			Console.WriteLine("OpenDefaultBook exit");
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void BasicContactTests()
		{
			const string tstUsername = "ian";
			const string tstTitle = "Software Tester";
			const string tstRole = "Contributor";
			const string tstBirthday = "11/02/1989";
			const string tstBlog = "http://bradyanderson.org/bloggers/ian";
			const string tstNode = "Game freak!";
			const string tstUrl = "http://bradyanderson.org/ian";

			AddressBook tstBook;

			tstBook = new 
				AddressBook(
					"TestBook1",
					Novell.AddressBook.AddressBookType.Private,
					Novell.AddressBook.AddressBookRights.ReadWrite,
					false);

			abManager.AddAddressBook(tstBook);

			// Create a contact in the new book

			Contact tstContact = new Contact();
			tstContact.UserName = tstUsername;
			tstContact.Title = tstTitle;
			tstContact.Role = tstRole;
			tstContact.Birthday = tstBirthday;
			tstContact.Blog = tstBlog;
			tstContact.Note = tstNode;
			tstContact.Url = tstUrl;

			tstBook.AddContact(tstContact);
			tstContact.Commit();

			// Now read the contact back and verify properties
			Contact cContact = tstBook.GetContact(tstContact.ID);
			if (cContact != null)
			{
				Console.WriteLine("Username:  " + cContact.UserName);
				if (cContact.UserName != tstUsername)
				{
					throw new ApplicationException( "BasicContactTests::mismatched UserName" );
				}

				Console.WriteLine("Title:     " + cContact.Title);
				if (cContact.Title != tstTitle)
				{
					throw new ApplicationException( "BasicContactTests::mismatched Title" );
				}

				Console.WriteLine("Role:      " + cContact.Role);
				if (cContact.Role != tstRole)
				{
					throw new ApplicationException( "BasicContactTests::mismatched Role" );
				}

				Console.WriteLine("Birthday:  " + cContact.Birthday);
				if (cContact.Birthday != tstBirthday)
				{
					throw new ApplicationException( "BasicContactTests::mismatched Birthday" );
				}

				Console.WriteLine("Blog:      " + cContact.Blog);
				if (cContact.Blog != tstBlog)
				{
					throw new ApplicationException( "BasicContactTests::mismatched Blog" );
				}

				Console.WriteLine("Url:       " + cContact.Url);
				if (cContact.Url != tstUrl)
				{
					throw new ApplicationException( "BasicContactTests::mismatched Url" );
				}
			}

			tstBook.Delete();
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void EmailTests()
		{
			const string tstUsername = "emailtestuser";

			const string emailOneAddress = "emailtestuser@novell.com";
			EmailTypes emailOneTypes = (EmailTypes.work | EmailTypes.internet | EmailTypes.preferred);

			const string emailTwoAddress = "emailtestuser@hotmail.com";
			EmailTypes emailTwoTypes = (EmailTypes.personal | EmailTypes.internet);

			AddressBook tstBook;

			tstBook = new 
				AddressBook(
					"TestBookForEmailTests",
					Novell.AddressBook.AddressBookType.Private,
					Novell.AddressBook.AddressBookRights.ReadWrite,
					false);

			abManager.AddAddressBook(tstBook);

			// Create a contact in the new book

			Contact tstContact = new Contact();
			tstContact.UserName = tstUsername;

			// Create a couple email objects
			Email emailOne = new Email(emailOneTypes, emailOneAddress);
			Email emailTwo = new Email(emailTwoTypes, emailTwoAddress);

			tstContact.AddEmailAddress(emailOne);
			tstContact.AddEmailAddress(emailTwo);

			tstBook.AddContact(tstContact);
			tstContact.Commit();

			// Now read the contact back and verify properties
			Contact cContact = tstBook.GetContact(tstContact.ID);
			if (cContact != null)
			{
				Console.WriteLine("Username:  " + cContact.UserName);
				if (cContact.UserName != tstUsername)
				{
					throw new ApplicationException( "EmailTests::mismatched UserName" );
				}

				Email prefEmail = cContact.GetPreferredEmailAddress();
				if (prefEmail != null)
				{
					Console.WriteLine("Preferred Email: " + prefEmail.Address);
					if (prefEmail.Address != emailOneAddress)
					{
						throw new ApplicationException( "EmailTests::incorrect preferred eMail" );
					}

					if (prefEmail.Types != emailOneTypes)
					{
						throw new ApplicationException( "EmailTests::incorrect preferred eMail types" );
					}
				}

				IABList emails = cContact.GetEmailAddresses();
	
				Console.WriteLine("looping through email addresses");
				foreach(Email cMail in cContact.GetEmailAddresses())
				{
					Console.WriteLine("Email: " + cMail.Address);
				}
			}

			tstBook.Delete();
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void PhoneTests()
		{
			Console.WriteLine("Starting \"Phone Tests\"");
			const string tstUsername = "phonetestuser";

			const string phoneOneNumber = "801-861-3130";
			PhoneTypes phoneOneTypes = (PhoneTypes.work | PhoneTypes.voice | PhoneTypes.preferred);

			const string phoneTwoNumber = "801-224-6692";
			PhoneTypes phoneTwoTypes = (PhoneTypes.home | PhoneTypes.voice);

			const string phoneThreeNumber = "801-318-4858";
			PhoneTypes phoneThreeTypes = 
				(PhoneTypes.cell | PhoneTypes.voice | PhoneTypes.msg | PhoneTypes.preferred);

			AddressBook tstBook;

			tstBook = new 
				AddressBook(
				"TestBookForPhoneTests",
				Novell.AddressBook.AddressBookType.Private,
				Novell.AddressBook.AddressBookRights.ReadWrite,
				false);

			abManager.AddAddressBook(tstBook);

			// Create a contact in the new book

			Contact tstContact = new Contact();
			tstContact.UserName = tstUsername;

			// Create a couple email objects
			Telephone phoneOne = new Telephone(phoneOneNumber, phoneOneTypes);
			Telephone phoneTwo = new Telephone(phoneTwoNumber, phoneTwoTypes);

			tstContact.AddTelephoneNumber(phoneOne);
			tstContact.AddTelephoneNumber(phoneTwo);

			tstBook.AddContact(tstContact);
			tstContact.Commit();

			// Now read the contact back and verify
			Contact cContact = tstBook.GetContact(tstContact.ID);
			if (cContact != null)
			{
				Console.WriteLine("Username:  " + cContact.UserName);
				if (cContact.UserName != tstUsername)
				{
					throw new ApplicationException( "EmailTests::mismatched UserName" );
				}

				Telephone pref = cContact.GetPreferredTelephoneNumber();
				if (pref != null)
				{
					Console.WriteLine("Preferred Phone: " + pref.Number);
					if (pref.Number != phoneOneNumber)
					{
						throw new ApplicationException( "PhoneTests::incorrect preferred phone" );
					}

					if (pref.Types != phoneOneTypes)
					{
						throw new ApplicationException( "PhoneTests::incorrect preferred phone types" );
					}
				}

				IABList phoneList = cContact.GetTelephoneNumbers();
	
				Console.WriteLine("looping through phone numbers");
				foreach(Telephone cPhone in cContact.GetTelephoneNumbers())
				{
					Console.WriteLine("Phone Number: " + cPhone.Number);
				}
			}

			tstBook.Delete();
			Console.WriteLine("Ending \"Phone Tests\"");
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void CreateDeleteAddressBook()
		{
			Console.WriteLine("CreateDeleteAddressBook called");
			AddressBook book1 = null;
			AddressBook book2 = null;
			AddressBook book3 = null;
			AddressBook book4 = null;

			try
			{
				book1 = new 
					AddressBook(
						"Book1",
						Novell.AddressBook.AddressBookType.Public,
						Novell.AddressBook.AddressBookRights.ReadOnly,
						false);

				abManager.AddAddressBook(book1);

				book3 = abManager.GetAddressBook(book1.ID);
				book4 = abManager.GetAddressBookByName("Book1");

				foreach(AddressBook cBook in abManager.GetAddressBooks())
				{
					Console.WriteLine("book name: {0}", cBook.Name);
					Console.WriteLine("type:      " + cBook.Type.ToString());
					Console.WriteLine("rights:    " + cBook.Rights.ToString());
				}

				book1.Delete();
				book1 = null;

				foreach(AddressBook cBook in abManager.GetAddressBooks())
				{
					Console.WriteLine("book name: {0}", cBook.Name);
				}

				/*
				foreach(AddressBook cBook in abManager.GetAddressBooks())
				{
					Console.WriteLine("book name: {0}", cBook.Name);
				}
				*/
			}
			catch{}

			if (book1 != null)
			{
				book1.Delete();
			}

			if (book2 != null)
			{
				book2.Delete();
			}

			if (book3 != null)
			{
				book3.Delete();
			}

			if (book4 != null)
			{
				book4.Delete();
			}

			Console.WriteLine("CreateDeleteAddressBook exit");
		}

		/// <summary>
		/// 
		/// </summary>
		public void SearchEmailTest()
		{
			Console.WriteLine("");
			Console.WriteLine("Starting \"Search Email Test\"");
			const string tstUserOne = "test1";
			const string tstEmailOne = "test1@yahoo.com";
			const string tstUserTwo = "test2";
			const string tstEmailTwo = "test2@gmail.com";

			AddressBook tstBook;

			tstBook = new 
				AddressBook(
					"TestBookForSearchEmailTest",
					Novell.AddressBook.AddressBookType.Private,
					Novell.AddressBook.AddressBookRights.ReadWrite,
					false);

			abManager.AddAddressBook(tstBook);

			// Create two contacts in the new book

			Contact tstContactOne = new Contact();
			tstContactOne.UserName = tstUserOne;
			tstContactOne.EMail = tstEmailOne;
			tstBook.AddContact(tstContactOne);
			tstContactOne.Commit();

			Contact tstContactTwo = new Contact();
			tstContactTwo.UserName = tstUserTwo;
			tstContactTwo.EMail = tstEmailTwo;
			tstBook.AddContact(tstContactTwo);
			tstContactTwo.Commit();

			// Now do a search for the first guy

			IEnumerator e = tstBook.SearchEmail(tstEmailOne, Simias.Storage.SearchOp.Begins).GetEnumerator();
			if (e.MoveNext())
			{
				Contact cContact = (Contact) e.Current;
				Console.WriteLine("Mail: " + cContact.EMail);
				if (cContact.EMail != tstEmailOne)
				{
					throw new ApplicationException( "SearchEmailTest::found the wrong contact" );
				}
			}
			else
			{
				throw new ApplicationException( "SearchEmailTest::failed to find contact" );
			}
			
			tstBook.Delete();
			Console.WriteLine("Ending \"Search Email Test\"");
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void BasicNameTests()
		{
			Console.WriteLine("");
			Console.WriteLine("Starting \"Basic Name Tests\"");
			const string tstUsername = "testuser";

			const string firstName = "Ian";
			const string lastName = "Anderson";
			const string otherName = "Adam";
			const string prefix = "Mr.";
			const string suffix = "II";

			AddressBook tstBook = null;

			try
			{
				tstBook = new 
					AddressBook(
						"TestBookForBasicNameTests",
						Novell.AddressBook.AddressBookType.Private,
						Novell.AddressBook.AddressBookRights.ReadWrite,
						false);

				abManager.AddAddressBook(tstBook);

				// Create a contact in the new book
				Contact tstContact = new Contact();
				tstContact.UserName = tstUsername;

				// Create a name object
				Name tstName = new Name(firstName, lastName);
				tstName.Other = otherName;
				tstName.Prefix = prefix;
				tstName.Suffix = suffix;
				tstName.Preferred = true;

				Console.WriteLine("Adding new contact");
				Console.WriteLine("Adding Name object to contact");
				Console.WriteLine("  First:  " + firstName);
				Console.WriteLine("  Last:   " + lastName);

				tstContact.AddName(tstName);
				tstBook.AddContact(tstContact);
				//tstContact.AddName(tstName);
				tstContact.Commit();

				// Now read the name back and verify
				Name cName = tstContact.GetName(tstName.ID);
				if (cName != null)
				{
					if (cName.Given != firstName)
					{
						throw new ApplicationException( "BasicNameTests::first name does not match" );
					}

					if (cName.Family != lastName)
					{
						throw new ApplicationException( "BasicNameTests::last name does not match" );
					}

					if (cName.Other != otherName)
					{
						throw new ApplicationException( "BasicNameTests::other name does not match" );
					}

					if (cName.Prefix != prefix)
					{
						throw new ApplicationException( "BasicNameTests::prefix name does not match" );
					}

					if (cName.Suffix != suffix)
					{
						throw new ApplicationException( "BasicNameTests::suffix name does not match" );
					}
				}

				// Now get a new contact
				Console.WriteLine("");
				Console.WriteLine("Getting a new instance of the Contact");
				Contact cContact = tstBook.GetContact(tstContact.ID);
				if (cContact != null)
				{
					Console.WriteLine("Getting a new instance of the Name");
					cName = cContact.GetName(tstName.ID);
					if (cName != null)
					{
						Console.WriteLine("Prefix:     " + cName.Prefix);
						if (cName.Prefix != prefix)
						{
							throw new ApplicationException( "BasicNameTests::prefix name does not match" );
						}

						Console.WriteLine("First Name: " + cName.Given);
						if (cName.Given != firstName)
						{
							throw new ApplicationException( "BasicNameTests::first name does not match" );
						}

						Console.WriteLine("Other Name: " + cName.Other);
						if (cName.Other != otherName)
						{
							throw new ApplicationException( "BasicNameTests::other name does not match" );
						}

						Console.WriteLine("Last Name:  " + cName.Family);
						if (cName.Family != lastName)
						{
							throw new ApplicationException( "BasicNameTests::last name does not match" );
						}

						Console.WriteLine("Suffix:     " + cName.Suffix);
						if (cName.Suffix != suffix)
						{
							throw new ApplicationException( "BasicNameTests::suffix name does not match" );
						}
					}
				}
			}
			finally
			{
				if (tstBook != null)
				{
					tstBook.Delete();
				}
			}

			Console.WriteLine("Ending \"Basic Name Tests\"");
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void BasicAddressTest()
		{
			Console.WriteLine("");
			Console.WriteLine("Starting \"Basic Address Test\"");
			const string tstUsername = "testuser";

			AddressTypes addrTypes = (AddressTypes.home | AddressTypes.postal);
			const string street = "295 East 100 North";
			const string city = "Salina";
			const string state = "UT";
			const string zip = "84654";
			const string country = "USA";
			const string postOfficeBox = "1411";
			AddressBook tstBook = null;

			try
			{
				tstBook = new 
					AddressBook(
						"TestBookForBasicAddressTest",
						Novell.AddressBook.AddressBookType.Private,
						Novell.AddressBook.AddressBookRights.ReadWrite,
						false);

				abManager.AddAddressBook(tstBook);

				// Create a contact in the new book
				Contact tstContact = new Contact();
				tstContact.UserName = tstUsername;

				// Create an Address object
				Address addr = new Address(zip);
				addr.Street = street;
				addr.Locality = city;
				addr.Region = state;
				addr.Country = country;
				addr.PostalBox = postOfficeBox;
				addr.Types = addrTypes;

				// toggle a bit
				addr.Preferred = true;
				addr.Preferred = false;
				addr.Preferred = true;

				Console.WriteLine("Adding new contact");
				Console.WriteLine("Adding Address object to contact");
				Console.WriteLine("  Street:  " + addr.Street);
				Console.WriteLine("  City:    " + addr.Locality);
				Console.WriteLine("  Zip:     " + addr.PostalCode);

				tstContact.AddAddress(addr);
				tstBook.AddContact(tstContact);
				tstContact.Commit();

				// Now read the name back and verify
				Address cAddr = tstContact.GetAddress(addr.ID);
				if (cAddr != null)
				{
					if (cAddr.Street != street)
					{
						throw new ApplicationException( "BasicAddressTest::street does not match" );
					}

					if (cAddr.Locality != city)
					{
						throw new ApplicationException( "BasicAddressTest::city does not match" );
					}

					if (cAddr.Region != state)
					{
						throw new ApplicationException( "BasicAddressTest::state does not match" );
					}

					if (cAddr.Country != country)
					{
						throw new ApplicationException( "BasicAddressTest::country does not match" );
					}

					if (cAddr.PostalCode != zip)
					{
						throw new ApplicationException( "BasicAddressTest::zip does not match" );
					}

					if (cAddr.PostalBox != postOfficeBox)
					{
						throw new ApplicationException( "BasicAddressTest::post office box does not match" );
					}

					if (cAddr.Preferred == false)
					{
						throw new ApplicationException( "BasicAddressTest::address should be preferred" );
					}
				}

				// Now get a new contact instance
				Console.WriteLine("");
				Console.WriteLine("Getting a new instance of the Contact");
				Contact cContact = tstBook.GetContact(tstContact.ID);
				if (cContact != null)
				{
					Console.WriteLine("Testing GetPreferredAddress");
					Address prefAddress = cContact.GetPreferredAddress();
					if (prefAddress != null)
					{
						if (prefAddress.ID != cAddr.ID)
						{
							throw new ApplicationException( "BasicAddressTest::wrong preferred address" );
						}
					}
					else
					{
						throw new ApplicationException( "BasicAddressTest::failed to get preferred address" );
					}

					Console.WriteLine("Getting a new instance of Address");
					Address cAddress = cContact.GetAddress(cAddr.ID);
					if (cAddress != null)
					{
						Console.WriteLine("Street:     " + cAddress.Street);
						if (cAddress.Street != cAddr.Street)
						{
							throw new ApplicationException( "BasicAddressTest::street does not match" );
						}

						Console.WriteLine("City:       " + cAddress.Locality);
						if (cAddress.Locality != cAddr.Locality)
						{
							throw new ApplicationException( "BasicAddressTest::city does not match" );
						}

						Console.WriteLine("State:      " + cAddress.Region);
						if (cAddress.Region != cAddr.Region)
						{
							throw new ApplicationException( "BasicAddressTest::state does not match" );
						}

						Console.WriteLine("Zip:        " + cAddress.PostalCode);
						if (cAddress.PostalCode != cAddr.PostalCode)
						{
							throw new ApplicationException( "BasicAddressTest::zip does not match" );
						}

						Console.WriteLine("PO Box:     " + cAddress.PostalBox);
						if (cAddress.PostalBox != cAddr.PostalBox)
						{
							throw new ApplicationException( "BasicAddressTest::po box does not match" );
						}

						Console.WriteLine("Country:    " + cAddress.Country);
						if (cAddress.Country != cAddr.Country)
						{
							throw new ApplicationException( "BasicAddressTest::country does not match" );
						}

						Console.WriteLine("Preferred:  " + cAddress.Preferred.ToString());
						if (cAddress.Types != cAddr.Types)
						{
							throw new ApplicationException( "BasicAddressTest::address types don't match" );
						}
					}
					else
					{
						throw new ApplicationException( "BasicAddressTest::failed to get Address I just saved" );
					}
				}
			}
			finally
			{
				if (tstBook != null)
				{
					tstBook.Delete();
				}
			}

			Console.WriteLine("Ending \"Basic Address Test\"");
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void SearchNameTest()
		{
			Console.WriteLine("");
			Console.WriteLine("Starting \"Search Name Test\"");
			const string userOne = "smele";
			const string userOneFirst = "Stone";
			const string userOneLast = "Mele";

			const string userTwo = "sgonzales";
			const string userTwoFirst = "Samantha";
			const string userTwoLast = "Gonazales";

			const string userThree = "banderson";
			const string userThreeFirst = "Braylee";
			const string userThreeLast = "Anderson";

			const string userFour = "ianderson";
			const string userFourFirst = "Ian";
			const string userFourLast = "Anderson";

			AddressBook tstBook = null;

			try
			{
				tstBook = new 
					AddressBook(
					"TestBookForSearchLastNameTest",
						Novell.AddressBook.AddressBookType.Private,
						Novell.AddressBook.AddressBookRights.ReadWrite,
						false);

				abManager.AddAddressBook(tstBook);

				// Create the contacts in the new book
				Console.WriteLine("Creating test contacts");
				Console.WriteLine("   Adding " + userOne);
				Contact user1 = new Contact();
				user1.UserName = userOne;
				Name user1name = new Name(userOneFirst, userOneLast);
				user1.AddName(user1name);
				tstBook.AddContact(user1);
				user1.Commit();

				Console.WriteLine("   Adding " + userTwo);
				Contact user2 = new Contact();
				user2.UserName = userTwo;
				Name user2name = new Name(userTwoFirst, userTwoLast);
				user2.AddName(user2name);
				tstBook.AddContact(user2);
				user2.Commit();

				Console.WriteLine("   Adding " + userThree);
				Contact user3 = new Contact();
				user3.UserName = userThree;
				Name user3name = new Name(userThreeFirst, userThreeLast);
				user3.AddName(user3name);
				tstBook.AddContact(user3);
				user3.Commit();

				Console.WriteLine("   Adding " + userFour);
				Contact user4 = new Contact();
				user4.UserName = userFour;
				Name user4name = new Name(userFourFirst, userFourLast);
				user4.AddName(user4name);
				tstBook.AddContact(user4);
				user4.Commit();

				Console.WriteLine("");
				Console.WriteLine("Searching for " + userOneLast);
				Console.WriteLine("Should find one");
				IABList results = tstBook.SearchLastName(userOneLast, Simias.Storage.SearchOp.Equal);
				foreach(Contact cContact in results)
				{
					Name prefName = cContact.GetPreferredName();
					if (prefName != null)
					{
						Console.WriteLine(
							"   Found: " + 
							prefName.Given + 
							" " + 
							prefName.Family);
					}
				}

				Console.WriteLine("Searching for " + userThreeLast);
				Console.WriteLine("Should find two");
				results = tstBook.SearchLastName(userThreeLast, Simias.Storage.SearchOp.Equal);
				foreach(Contact cContact in results)
				{
					Name prefName = cContact.GetPreferredName();
					if (prefName != null)
					{
						Console.WriteLine("   Found: " + prefName.Given + " " + prefName.Family);
					}
				}

				Console.WriteLine("Searching for: " + userTwoFirst);
				Console.WriteLine("Should find one");
				results = tstBook.SearchFirstName(userTwoFirst, Simias.Storage.SearchOp.Equal);
				foreach(Contact cContact in results)
				{
					Name prefName = cContact.GetPreferredName();
					if (prefName != null)
					{
						Console.WriteLine("   Found: " + prefName.Given + " " + prefName.Family);
					}
				}

				Console.WriteLine("Searching (begins) for: s");
				Console.WriteLine("Should find two");
				results = tstBook.SearchFirstName("s", Simias.Storage.SearchOp.Begins);
				foreach(Contact cContact in results)
				{
					Name prefName = cContact.GetPreferredName();
					if (prefName != null)
					{
						Console.WriteLine("   Found: " + prefName.Given + " " + prefName.Family);
					}
				}

				Console.WriteLine("Searching (username) for: " + userFour);
				Console.WriteLine("Should find one");
				results = tstBook.SearchUsername(userFour, Simias.Storage.SearchOp.Equal);
				foreach(Contact cContact in results)
				{
					Name prefName = cContact.GetPreferredName();
					if (prefName != null)
					{
						Console.WriteLine("   Found: " + prefName.Given + " " + prefName.Family);
					}
				}
			}
			finally
			{
				if (tstBook != null)
				{
					tstBook.Delete();
				}
			}

			Console.WriteLine("Ending \"Search Name Test\"");
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void EnumContactsTest()
		{
			Console.WriteLine("");
			Console.WriteLine("Starting \"Enumerate Contacts Test\"");

			/*
			const string[] contactNames = 
				new string["ContactOne", "ContactTwo", "ContactThree", "ContactFour", "ContactFive"];
			*/

			string[] contactNames = {"ContactOne", "ContactTwo", "ContactThree", "ContactFour", "ContactFive"};

			Contact[] contacts = new Contact[contactNames.Length];

			AddressBook tstBook = null;

			try
			{
				tstBook = new 
					AddressBook(
					"TestBookForEnumerateContacts",
						Novell.AddressBook.AddressBookType.Private,
						Novell.AddressBook.AddressBookRights.ReadWrite,
						false);

				abManager.AddAddressBook(tstBook);

				Console.WriteLine("Adding");
				for(int i = 0; i < contactNames.Length; i++)
				{
					Console.WriteLine("   " + contactNames[i]);
					contacts[i] = new Contact();
					contacts[i].UserName = contactNames[i];
					tstBook.AddContact(contacts[i]);
					contacts[i].Commit();
				}

				Console.WriteLine("Enumerating contacts");
				int numFound = 0;
				int idx;
				foreach(Contact rContact in tstBook)
				{
					Console.WriteLine("   " + rContact.UserName);

					// Now go find it
					for(idx = 0; idx < contactNames.Length; idx++)
					{
						if (rContact.UserName == contactNames[idx])
						{
							break;
						}
					}

					if (idx == contactNames.Length)
					{
						throw new ApplicationException( "EnumContactsTest::failed to find committed contact" );
					}

					numFound++;
				}

				if (numFound != contactNames.Length)
				{
					throw new ApplicationException( "EnumContactsTest::didn't find all the contacts" );
				}
			}
			finally
			{
				if (tstBook != null)
				{
					tstBook.Delete();
				}
			}

			Console.WriteLine("Ending \"Enumerate Contacts Test\"");
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void ImportPhotoTest()
		{
			Console.WriteLine("");
			Console.WriteLine("Starting \"Import Photo Test\"");
			const string userOne = "smele";
			const string userOneFirst = "Stone";
			const string userOneLast = "Mele";
			const string userOneMail = "smele@yahoo.com";

			AddressBook tstBook = null;

			try
			{
				tstBook = new 
					AddressBook(
						"TestBookForImportPhotoTest",
						Novell.AddressBook.AddressBookType.Private,
						Novell.AddressBook.AddressBookRights.ReadWrite,
						false);

				abManager.AddAddressBook(tstBook);

				// Create the contacts in the new book
				Console.WriteLine("Creating contact");
				Console.WriteLine("   Adding " + userOne);
				Contact user1 = new Contact();
				user1.UserName = userOne;
				user1.EMail = userOneMail;
				Name user1name = new Name(userOneFirst, userOneLast);
				user1.AddName(user1name);
				tstBook.AddContact(user1);

				Console.WriteLine("Importing photo");
				string picPath = 
					".." + 
					Path.DirectorySeparatorChar + 
					".." + 
					Path.DirectorySeparatorChar + 
					"02480.jpg";
				user1.ImportPhoto(picPath);
				user1.Commit();

				// Import it again to test the delete and add case
				user1.ImportPhoto(picPath);
				user1.Commit();

				Stream fsOut = user1.ExportPhoto();
				fsOut.Close();
			}
			finally
			{
				if (tstBook != null)
				{
					tstBook.Delete();
				}
			}

			Console.WriteLine("Ending \"Import Photo Test\"");
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void InstantMessageTests()
		{
			Console.WriteLine("Starting \"Instant Message Tests\"");
			const string tstUsername = "testuser";

			const string im1Address = "banderso";
			const string im1Provider = "GroupWise";
			IMTypes im1Types = (IMTypes.work);

			const string im2Address = "hackerboy";
			const string im2Provider = "AIM";
			IMTypes im2Types = (IMTypes.home | IMTypes.preferred);

			const string im3Address = "banderso@novell.com";
			const string im3Provider = "MSN";
			IMTypes im3Types = (IMTypes.other);

			AddressBook tstBook = null;

			try
			{
				tstBook = new 
					AddressBook(
					"TestBookForIMTests",
					Novell.AddressBook.AddressBookType.Private,
					Novell.AddressBook.AddressBookRights.ReadWrite,
					false);

				abManager.AddAddressBook(tstBook);

				// Create a contact in the new book

				Contact tstContact = new Contact();
				tstContact.UserName = tstUsername;

				// create the IM objects
				IM imOne = new IM(im1Address, im1Provider, im1Types);
				IM imTwo = new IM(im2Address, im2Provider, im2Types);
				IM imThree = new IM(im3Address, im3Provider, im3Types);

				// Add 2 before we add to the address book
				tstContact.AddInstantMessage(imOne);
				tstContact.AddInstantMessage(imTwo);

				tstBook.AddContact(tstContact);
				tstContact.Commit();

				// Now add the third and commit
				tstContact.AddInstantMessage(imThree);
				tstContact.Commit();

				// Now read the contact back and verify
				Contact cContact = tstBook.GetContact(tstContact.ID);
				if (cContact != null)
				{
					Console.WriteLine("Username:  " + cContact.UserName);
					if (cContact.UserName != tstUsername)
					{
						throw new ApplicationException( "InstantMessageTests::mismatched UserName" );
					}

					IM pref = cContact.GetPreferredInstantMessage();
					if (pref != null)
					{
						Console.WriteLine("Preferred IM: " + pref.Address);
						if (pref.Address != im2Address)
						{
							throw new ApplicationException( "InstantMessageTests::incorrect preferred IM" );
						}

						if (pref.Types != im2Types)
						{
							throw new ApplicationException( "InstantMessageTests::incorrect preferred IM types" );
						}
					}

					//IABList imList = cContact.GetInstantMessageAccounts();
	
					Console.WriteLine("looping through IM objects");
					foreach(IM cIM in cContact.GetInstantMessageAccounts())
					{
						Console.WriteLine("");
						Console.WriteLine("IM Provider: " + cIM.Provider);
						Console.WriteLine("IM Name:     " + cIM.Address);
					}
				}
			}
			finally
			{
				if (tstBook != null)
				{
					tstBook.Delete();
				}
			}
			Console.WriteLine("Ending \"Instant Message Tests\"");
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void CleanupTest()
		{
			Console.WriteLine("Starting \"Cleanup Test\"");
			const string tstUsername = "cleanupuser";

			const string im1Address = "banderso";
			const string im1Provider = "GroupWise";
			IMTypes im1Types = (IMTypes.work);

			AddressBook tstBook = null;

			try
			{
				tstBook = new 
					AddressBook(
					"TestBookForCleanupTest",
					Novell.AddressBook.AddressBookType.Private,
					Novell.AddressBook.AddressBookRights.ReadWrite,
					false);

				abManager.AddAddressBook(tstBook);

				// Create a contact in the new book

				Contact tstContact = new Contact();
				tstContact.UserName = tstUsername;

				// create the IM objects
				IM imOne = new IM(im1Address, im1Provider, im1Types);
				tstContact.AddInstantMessage(imOne);

				// create a name
				Name cName = new Name("Brady", "Anderson");
				tstContact.AddName(cName);
				string nameID = cName.ID;

				// create an address
				Address cAddress = new Address("84606");
				cAddress.Street = "1800 South Novell Place";
				tstContact.AddAddress(cAddress);
				string addressID = cAddress.ID;

				tstBook.AddContact(tstContact);
				tstContact.Commit();
				string contactID = tstContact.ID;

				// Now delete the contact and try and get the objects from the 
				// collection store
				tstContact.Delete();

				// Muck with a deleted node
				if (cAddress.Street != "")
				{
					throw new ApplicationException( "CleanupTest::Street should have been an empty string" );
				}
			}
			finally
			{
				if (tstBook != null)
				{
					tstBook.Delete();
				}
			}
			Console.WriteLine("Ending \"Cleanup Test\"");
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void BasicGroupTest()
		{
			Console.WriteLine("");
			Console.WriteLine("Starting \"Basic Group Test\"");
			const string userOne = "smele";
			const string userOneFirst = "Stone";
			const string userOneLast = "Mele";

			const string userTwo = "sgonzales";
			const string userTwoFirst = "Samantha";
			const string userTwoLast = "Gonazales";

			const string userThree = "banderson";
			const string userThreeFirst = "Braylee";
			const string userThreeLast = "Anderson";

			const string userFour = "ianderson";
			const string userFourFirst = "Ian";
			const string userFourLast = "Anderson";

			AddressBook tstBook = null;

			try
			{
				tstBook = new 
					AddressBook(
						"TestBookForSearchLastNameTest",
						Novell.AddressBook.AddressBookType.Private,
						Novell.AddressBook.AddressBookRights.ReadWrite,
						false);

				abManager.AddAddressBook(tstBook);

				// Create the contacts in the new book
				Console.WriteLine("Creating test contacts");
				Console.WriteLine("   Adding " + userOne);
				Contact user1 = new Contact();
				user1.UserName = userOne;
				Name user1name = new Name(userOneFirst, userOneLast);
				user1.AddName(user1name);
				tstBook.AddContact(user1);
				user1.Commit();

				Console.WriteLine("   Adding " + userTwo);
				Contact user2 = new Contact();
				user2.UserName = userTwo;
				Name user2name = new Name(userTwoFirst, userTwoLast);
				user2.AddName(user2name);
				tstBook.AddContact(user2);
				user2.Commit();

				Console.WriteLine("   Adding " + userThree);
				Contact user3 = new Contact();
				user3.UserName = userThree;
				Name user3name = new Name(userThreeFirst, userThreeLast);
				user3.AddName(user3name);
				tstBook.AddContact(user3);
				user3.Commit();

				Console.WriteLine("   Adding " + userFour);
				Contact user4 = new Contact();
				user4.UserName = userFour;
				Name user4name = new Name(userFourFirst, userFourLast);
				user4.AddName(user4name);
				tstBook.AddContact(user4);
				user4.Commit();

				// Now create a group and add the contacts to the Group
				Group cGroup = new Group("TestGroup");
				cGroup.Description = "Cool guys";
				cGroup.Note = "nieces and newphews";

				tstBook.AddGroup(cGroup);

				cGroup.AddContact(user1);
				cGroup.AddContact(user2);
				cGroup.AddContact(user3);
				cGroup.AddContact(user4);
				cGroup.Commit();

				// Now enumerate the contacts in the list
				foreach(Contact cContact in cGroup.GetContactList())
				{
					Console.WriteLine(cContact.UserName);
				}

				// Now remove user two from the group and re-enumerate
				cGroup.RemoveContact(user2);
				cGroup.Commit();

				// Now enumerate the contacts in the list
				foreach(Contact cContact in cGroup.GetContactList())
				{
					Console.WriteLine(cContact.UserName);
				}

			}
			finally
			{
				if (tstBook != null)
				{
					tstBook.Delete();
				}
			}

			Console.WriteLine("Ending \"Search Name Test\"");
		}

		/// <summary>
		/// 
		/// </summary>
		public class Tests
		{
			static void Main()
			{
				Iteration0Tests tests = new Iteration0Tests();
				tests.Init();
				tests.OpenDefaultBook();
				tests.EnumerateMyAddressBooks();
				tests.CreateDeleteAddressBook();
				tests.BasicContactTests();
				tests.EmailTests();
				tests.PhoneTests();
				tests.SearchEmailTest();
				tests.BasicNameTests();
				tests.EnumContactsTest();
				tests.BasicAddressTest();
				tests.SearchNameTest();
				//tests.ImportPhotoTest();
				tests.InstantMessageTests();
				tests.CleanupTest();
				tests.BasicGroupTest();
			}
		}
	}
}


