using System;
using Novell.AddressBook;
using Simias.Storage;

namespace SimpleTests
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			bool	found;
			Manager addressManager;

			//
			// TODO: Add code to start application here
			//

			try
			{
				Console.WriteLine("Connecting to the collection store\n");
				addressManager = Manager.Connect( );
			}
			catch(Exception e)
			{
				throw new ApplicationException("failed to connect to the collection store");
			}
			
			// Enumerate the current address books
			Console.WriteLine("Enumerating current address books");
			found = false;
			foreach(AddressBook tmpAddressBook in addressManager)
			{
				found = true;
				Console.WriteLine("book name: {0}", tmpAddressBook.Name);
			}
			if (found == false)
			{
				Console.WriteLine("  no address books found!");
			}
			Console.WriteLine("");
			

			AddressBook addressBook =
				addressManager.CreateAddressBook(
					"Frequent Contacts",
					AddressBook.AddressBookType.Private, 
					AddressBook.AddressBookRights.ReadOnly);

			Console.WriteLine("Address Book: {0}", addressBook.Name);
			Console.WriteLine("ID:           {0}", addressBook.ID);
			Console.WriteLine("Type:         {0}", addressBook.Type);
			Console.WriteLine("Rights:       {0}", addressBook.Rights);
			Console.WriteLine("");

			// Create a contact in the "Frequent Contact" address book

			Contact contact = addressBook.CreateContact("banderso");
			if(contact != null)
			{
				contact.Birthday = "1961-12-19";
				contact.EMail = "banderso@novell.com";
				contact.FN = "Brady Anderson";
				contact.NickName = "Brady";
				contact.Title = "Simias Hacker";
				contact.URL = "http://igolfinutah.com";

				Console.WriteLine("Contact Info");
				Console.WriteLine("ID:           {0}", contact.ID);
				Console.WriteLine("Username:     {0}", contact.UserName);
				Console.WriteLine("Full Name:    {0}", contact.FN);
				Console.WriteLine("Nick Name:    {0}", contact.NickName);
				Console.WriteLine("Birthday:     {0}", contact.Birthday);
				Console.WriteLine("Title:        {0}", contact.Title);
				Console.WriteLine("E-Mail:       {0}", contact.EMail);
				Console.WriteLine("URL:          {0}", contact.URL);

				// Create a home address for this contact
				Address address = contact.CreateAddress(Novell.AddressBook.Address.AddressType.Personal, "84004");
				if(address != null)
				{
					address.Street = "5027 North River Park Way";
					address.Region = "Provo";
					address.Locality = "Utah";
					address.Country = "USA";
			
					Console.WriteLine("Address Type: {0}", address.FriendlyType);
					Console.WriteLine("Type:         {0}", address.Type);
					Console.WriteLine("Street:       {0}", address.Street);
					Console.WriteLine("City:         {0}", address.Region);
					Console.WriteLine("State:        {0}", address.Locality);
					Console.WriteLine("Country:      {0}", address.Country);
					Console.WriteLine("ZIP:          {0}", address.PostalCode);
				}

				// contact.DeleteAddress(address.ID);

				// Delete the contact
				addressBook.DeleteContact(contact.ID);
			}


			// Enumerate the current address books
			Console.WriteLine("Enumerating current address books");
			found = false;
			foreach(AddressBook tmpAddressBook in addressManager)
			{
				found = true;
				Console.WriteLine("book name: {0}", tmpAddressBook.Name);
			}
			if (found == false)
			{
				Console.WriteLine("  no address books found!");
			}
			Console.WriteLine("");


			/*
			  Create a normal collection in the system which shouldn't be returned in
			  the address book enumeration 
			*/

			/*
			Store testStore = Store.Open( "some path", "test", "test" );
			Collection testCollection = testStore.CreateCollection( "Simias Collection" );
			testCollection.Commit();

			// Enumerate the current address books
			Console.WriteLine("Enumerating current address books");
			foreach(AddressBook tmpAddressBook in addressManager)
			{
				Console.WriteLine("book name: {0}", tmpAddressBook.Name);
			}
			Console.WriteLine("");

			testStore.DeleteCollection( testCollection.Id , true);
			*/

			addressManager.DeleteAddressBook(addressBook.ID);

			// Reset the enumeration and try again
			addressManager.Reset();
			Console.WriteLine("Enumerating current address books (2)");
			foreach(AddressBook tmpAddressBook in addressManager)
			{
				Console.WriteLine("book name: {0}", tmpAddressBook.Name);
			}
			Console.WriteLine("");
		 
		}
	}
}
