using System;
using Novell.AddressBook;

namespace ABCmd
{
	/// <summary>
	/// Summary description for ABCmd.
	/// </summary>
	class ABCmd
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			bool		enumBooks = false;
			int			count = 1;
			Manager		addressManager = null;
            string      addressBook = null;
            string      contact = null;

			foreach(string arg in args)
			{
				count++;

				if (arg == "/?" || arg == "/h" || arg == "/H")
				{
					Console.WriteLine("usage: ABCmd /a<Address Book> /i<Contact> /ea /ec /ca /cc");
					Console.WriteLine("  /ea = Enumerate address books");
                    Console.WriteLine("  /ec = Enumerate contacts");                    
                    Console.WriteLine("  /a = Address book");
                    Console.WriteLine("  /i = Contact/Identity");
                    Console.WriteLine("  /ca = Create address book");
                    Console.WriteLine("  /cc = Create contact");
                    Console.WriteLine("  /da = Delete address book");
                    Console.WriteLine("  /dc = Delete contact");
					return;
				}

				if (addressManager == null)
				{
					Console.WriteLine("Connecting to the Address Book Manager...\n");
					addressManager = Manager.Connect( );
					/*
					try
					{
						Console.WriteLine("Connecting to the Address Book Manager...\n");
						addressManager = Manager.Connect( );
					}
					catch(Exception e)
					{
						Console.WriteLine("Failed to connect to the Address Book");
						return;
					}
					*/
				}

				//
				// Enumerate existing address books
				//

				if(arg == "/ea")
				{
					bool found = false;
					Console.WriteLine("Enumerating Address Books");
					foreach(AddressBook book in addressManager)
					{
						found = true;
						Console.WriteLine("GUID: {0} Name: {1}", book.ID, book.Name);
					}
					if (found == false)
					{
						Console.WriteLine("  no address books found!");
					}
					Console.WriteLine("");
				}

				/*
					Address Book
				*/ 
				 
				if(arg.Substring(0, 2) == "/a")
				{
					addressBook = arg.Remove(0, 2);
					Console.WriteLine("Address Book: {0}", addressBook);
				}

				if(arg.Substring(0, 2) == "/i")
				{
					contact = arg.Remove(0, 2);
					Console.WriteLine("Contact: {0}", contact);
				}

				if(arg == "/ca")
				{
                    if (addressBook != null)
                    {
                        Console.WriteLine("Creating address book {0}", addressBook);

                        AddressBook ab =
                            addressManager.CreateAddressBook(
                                addressBook,
                                AddressBook.AddressBookType.Private, 
                                AddressBook.AddressBookRights.ReadWrite);

                    }
				}


				//
				// Delete an address book
				//

				if(arg == "/da")
				{
                    if (addressBook != null)
                    {
                        Console.WriteLine("Deleting book: {0}", addressBook);
                        foreach(AddressBook book in addressManager)
                        {
                            if (book.Name == addressBook)
                            {
                                addressManager.DeleteAddressBook(book.ID);
                                break;
                            }
                        }
                    }
				}

                //
                // Create a contact
                //

				if(arg == "/cc")
				{
                    if (addressBook != null && contact != null)
                    {
                        Console.WriteLine("Creating contact {0} in {1}", contact, addressBook);
                        foreach(AddressBook book in addressManager)
                        {
                            if (book.Name == addressBook)
                            {
                                book.CreateContact(contact);
                                break;
                            }
                        }
                    }
				}

                //
                // Enumerate contacts
                //

				if(arg == "/ec")
				{
                    if (addressBook != null)
                    {
                        foreach(AddressBook book in addressManager)
                        {
                            if (book.Name == addressBook)
                            {
                                Console.WriteLine("Enumerating contacts in {0}", addressBook);
                                foreach(Contact cnt in book)
                                {
                                    Console.WriteLine("  {0}", cnt.UserName);
                                }
                            }                         
                        }
                    }
				}

			}
		}
	}
}
