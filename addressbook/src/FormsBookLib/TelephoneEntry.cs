using System;
using Novell.AddressBook;

namespace Novell.iFolder.FormsBookLib
{
	/// <summary>
	/// Summary description for TelephoneEntry.
	/// </summary>
	public class TelephoneEntry
	{
		private Telephone phone;
		private bool add;
		private bool remove;

		public TelephoneEntry()
		{
			remove = false;
		}

		public Telephone Phone
		{
			get
			{
				return phone;
			}
			set
			{
				phone = value;
			}
		}

		public bool Add
		{
			get
			{
				return add;
			}
			set
			{
				add = value;
			}
		}

		public bool Remove
		{
			get
			{
				return remove;
			}
			set
			{
				remove = value;
			}
		}
	}
}
