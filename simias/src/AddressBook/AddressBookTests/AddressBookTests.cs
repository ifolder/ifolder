using System;
using NUnit.Framework;
using Novell.AddressBook;


namespace Novell.AddressBook.Tests
{
	/// <summary>
	/// Summary description for Iteration0Tests.
	/// </summary>
	[TestFixture]
	public class Iteration0Tests
	{
		Manager aBManager;

		[TestFixtureSetUp]
		public void Init()
		{
			aBManager = Manager.Connect();
		}

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

		[Test]
		public void CreateDeleteAddressBook()
		{
			/*
			AddressBook addressBook =
				aBManager.CreateAddressBook(
					Guid.NewGuid().ToString(),
					AddressBook.AddressBookType.Private, 
					AddressBook.AddressBookRights.ReadOnly);

			Console.WriteLine("Address Book: {0}", addressBook.Name);
			Console.WriteLine("ID:           {0}", addressBook.ID);
			Console.WriteLine("Type:         {0}", addressBook.Type);
			Console.WriteLine("Rights:       {0}", addressBook.Rights);
			Console.WriteLine("");
			*/

		//	AddressBook tmpBook = addressBookManager.GetAddressBook(addressBook.ID);

			/*
			Console.WriteLine("Enumerating after creating {0}", addressBook.Name);
			foreach(AddressBook addressBook1 in addressBookManager)
			{
				Console.WriteLine("book name: {0}", addressBook1.Name);
			}
			*/
			
			//aBManager.DeleteAddressBook(addressBook.ID);
		}

		
		public class Tests
		{
			static void Main()
			{
				Iteration0Tests tests = new Iteration0Tests();
				tests.Init();
				//tests.EnumerateMyAddressBooks();
				tests.CreateDeleteAddressBook();
			}
		}
	}
}


