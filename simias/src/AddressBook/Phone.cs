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
using System.IO;
using System.Collections;
using Simias.Storage;

namespace Novell.AddressBook
{
	/// <summary>
	/// The default type is "voice" and "work". 
	/// vCard does not enforce any exclusionary combinations even though
	/// some seem to be.  For example a video conferencing telephone
	/// number will not also be used as a bbs number.
	///
	/// There can exist only one "preferred" telephone number
	/// per contact.
	/// </summary>
	public enum PhoneTypes : uint
	{
		/// <summary>
		/// indicates a preferred-use telephone number
		/// </summary>
		preferred = 0x00000001,

		/// <summary>
		/// indicates a telephone number associated with a residence
		/// </summary>
		home =		0x00000002,

		/// <summary>
		/// indicates a telephone number associated with a place of work
		/// </summary>
		work =		0x00000004,

		/// <summary>
		/// indicates a telephone number location other than home or work
		/// </summary>
		other =		0x00000008,

		/// <summary>
		/// indicates a cellular telephone
		/// </summary>
		cell =		0x00000010,

		/// <summary>
		/// indicates a telephon number that has voice messaging capabilities
		/// </summary>
		msg =		0x00000020,

		/// <summary>
		/// indicates a voice telephone number
		/// </summary>
		voice =		0x00000040,

		/// <summary>
		/// indicates a facsimile telephone number
		/// </summary>
		fax =		0x00000080,

		/// <summary>
		/// indicates a video conferencing telephone number
		/// </summary>
		video =		0x00000100,

		/// <summary>
		/// indicates a telephone number to a paging device
		/// </summary>
		pager =		0x00000200,

		/// <summary>
		/// indicates a telephone number to a bulletin board system
		/// </summary>
		bbs =		0x00000400,

		/// <summary>
		/// indicates a telephone number to a MODEM
		/// </summary>
		modem =		0x00000800,

		/// <summary>
		/// indicates a telephone number location to a car phone
		/// </summary>
		car =		0x00001000,

		/// <summary>
		/// indicates an ISDN service telephone number
		/// </summary>
		isdn =		0x00002000,

		/// <summary>
		/// indicates a personal communication services telephone number
		/// </summary>
		pcs =		0x00004000,

		/// <summary>
		/// indicates a voice over IP telephone number
		/// </summary>
		voip =		0x00008000
	}

	/// <summary>
	/// As per the vCard 3.0 spec, a Contact may contain multiple
	///	phone numbers.
	/// </summary>
	public class Telephone
	{
		#region Class Members
		Contact						parentContact;
		string						phoneNumber;
		PhoneTypes					phoneTypes;
		private const PhoneTypes	defaultPhoneTypes = (PhoneTypes.voice | PhoneTypes.work);
		#endregion

		#region Properties
		/// <summary>
		/// Preferred:	true if this e-mail is the contact's preferred e-mail
		///				false if this e-mail is a secondary e-mail
		///
		/// Type Value: Single bool value
		///
		/// </summary>
		public bool Preferred
		{
			get
			{
				if ((this.phoneTypes & PhoneTypes.preferred) == PhoneTypes.preferred)
				{
					return(true);
				}
				else
				{
					return(false);
				}
			}

			set
			{
				if (value == true)
				{
					this.phoneTypes |= PhoneTypes.preferred;
					if (this.parentContact != null)
					{
						// There can only be one preferred make sure it's this object
						foreach (Telephone tmpPhone in this.parentContact.phoneList)
						{
							if (tmpPhone.phoneNumber != this.phoneNumber)
							{
								if (tmpPhone.Preferred == true)
								{
									tmpPhone.Preferred = false;
								}
							}
						}
					}
				}
				else
				{
					this.phoneTypes &= ~PhoneTypes.preferred;
				}

				/*
				if (this.parentContact != null)
				{
					this.parentContact.SetDirty(ChangeMap.phone);
				}
				*/
			}
		}

		/// <summary>
		/// Number: actual telephone number
		/// Type Value: String
		/// ex. 801-861-3130
		/// </summary>
		public string Number
		{
			get
			{
				if (this.phoneNumber != null)
				{
					return(this.phoneNumber);
				}

				return("");
			}

			set
			{
				this.phoneNumber = value;
				if (this.parentContact != null)
				{
					// number distinguishes so there can't be two phone objects with the same number
					foreach (Telephone tmpPhone in this.parentContact.phoneList)
					{
						if (tmpPhone != this)
						{
							if (tmpPhone.phoneNumber == this.phoneNumber)
							{
								this.parentContact.phoneList.Remove(tmpPhone);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Type: type of telephone number
		/// Type Value: uint
		/// </summary>
		public PhoneTypes Types
		{
			get
			{
				return(this.phoneTypes);
			}

			set
			{
				this.phoneTypes = value;
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Simple Telephone constructor
		/// </summary>
		public Telephone()
		{
			this.phoneTypes = defaultPhoneTypes;
		}	

		/// <summary>
		/// Telephone constructor which includes the phone number
		/// </summary>
		public Telephone(string number)
		{
			this.phoneNumber = number;
			this.phoneTypes = defaultPhoneTypes;
		}

		/// <summary>
		/// Telephone constructor which includes the phone number and types
		/// </summary>
		public Telephone(string number, PhoneTypes types)
		{
			this.phoneNumber = number;
			this.phoneTypes = types;
		}

		internal Telephone(Contact contact, string serializedNumber)
		{
			this.parentContact = contact;
			this.Unserialize(serializedNumber);
		}
		#endregion

		#region Private Methods
		internal bool Add(Contact contact)
		{
			this.parentContact = contact;

			//
			// If the phone number exists, delete the existing one
			//

			foreach(Telephone tmpPhone in contact.phoneList)
			{
				if(tmpPhone.Number == this.Number)
				{
					contact.phoneList.Remove(tmpPhone);
					break;
				}
			}

			if (this.Preferred == true)
			{
				//
				// If another property is already preferred, change it
				//

				foreach(Telephone tmpPhone in contact.phoneList)
				{
					if(tmpPhone.Preferred == true)
					{
						tmpPhone.Preferred = false;
					}
				}
			}

			//
			// Add the new phone number to the list
			//

			contact.phoneList.Add(this);
			//contact.SetDirty(ChangeMap.phone);
			return(true);
		}

		internal static bool PersistToStore(Contact contact)
		{
			// Anything in the list to persist?
			if (contact.phoneList.Count == 0)
			{
				return(false);
			}

			// First delete the property
			contact.Properties.DeleteProperties(Common.phoneProperty);

			// assume no preferred is set
			bool foundPreferred = false;

			// Make sure we have a preferred
			foreach(Telephone tmpPhone in contact.phoneList)
			{
				if (tmpPhone.Preferred == true)
				{
					foundPreferred = true;
					break;
				}
			}

			if (foundPreferred == false)
			{
				// No preferred do we have one typed WORK?
				foreach(Telephone tmpPhone in contact.phoneList)
				{
					if ((tmpPhone.phoneTypes & PhoneTypes.work) == PhoneTypes.work)
					{
						tmpPhone.Preferred = true;
						break;
					}
				}

				// Any will do
				if (foundPreferred == false)
				{
					foreach(Telephone tmpPhone in contact.phoneList)
					{
						tmpPhone.Preferred = true;
						break;
					}
				}
			}

			// To the collection store they go!
			foreach(Telephone tmpPhone in contact.phoneList)
			{
				Property p = new Property(Common.phoneProperty, tmpPhone.Serialize());
				contact.Properties.AddProperty(p);
			}

			return(true);
		}

		// Currently the telphone object is serialized in the collection
		// store in the following format: telephone-number;phone-types
		// ex. 801-318-4858;0x000000053
		private bool Unserialize(string sPhone)
		{
			// take a serialized phone string and load up
			// the class members with valid values

			// Actual telephone number comes first
			int index = 0;
			this.phoneNumber = "";
			while(index < sPhone.Length && sPhone[index] != ';')
			{
				this.phoneNumber += Convert.ToString(sPhone[index++]);
			}

			// past the ;
			index++;
			string tmpType = "";
			while(index < sPhone.Length)
			{
				tmpType += Convert.ToString(sPhone[index++]);
			}

			if (tmpType != "")
			{
				this.phoneTypes = (PhoneTypes) Convert.ToUInt32(tmpType, 16);
			}

			return(true);
		}

		private string Serialize()
		{
			string	serialized;

			// phone number must be present
			if (this.phoneNumber == null)
			{
				return(null);
			}

			serialized = this.phoneNumber;
			serialized += ";" + Convert.ToString((uint) this.phoneTypes, 16);
			return(serialized);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Delete this TEL from the phone list attached to the contact record
		/// </summary>
		public void Delete()
		{
			try
			{
				if (this.parentContact != null)
				{
					this.parentContact.phoneList.Remove(this);
				}
			}
			catch{}
			return;
		}

		#endregion
	}
}
