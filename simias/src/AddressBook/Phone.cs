/***********************************************************************
 *  Phone.cs - Class for defining a vCard 3.0 TEL property.
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
		Collection					parentCollection;
		Node						thisNode;
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
				try
				{
					if (value == true)
					{
						this.phoneTypes |= PhoneTypes.preferred;
					}
					else
					{
						this.phoneTypes &= ~PhoneTypes.preferred;
					}

					// If this object is attached to the store - fix it up
					if (this.thisNode != null)
					{
						char[] semiSep = new char[]{';'};
						MultiValuedList mList = null;

						// Remove the preferred bit off all other phone properties
						if (value == true)
						{
							mList = this.thisNode.Properties.GetProperties(Common.phoneProperty);
							foreach(Property p in mList)
							{
								IEnumerator enumTokens = p.Value.ToString().Split(semiSep).GetEnumerator();

								// First token should be the number
								if (enumTokens.MoveNext())
								{
									string	tmpNumber = (string) enumTokens.Current;

									// Now the types
									if (enumTokens.MoveNext())
									{
										PhoneTypes tmpTypes = 
											(PhoneTypes) Convert.ToUInt32((string) enumTokens.Current, 16);
										if (tmpNumber != this.phoneNumber && 
											((tmpTypes & PhoneTypes.preferred) == PhoneTypes.preferred))
										{
											tmpTypes &= ~PhoneTypes.preferred;
											tmpNumber += ";" + Convert.ToString((uint) tmpTypes, 16);
											p.Value = tmpNumber;

											this.thisNode.Properties.ModifyProperty(p);
										}
									}
								}
							}
						}

						mList = this.thisNode.Properties.GetProperties(Common.phoneProperty);
						foreach(Property p in mList)
						{
							IEnumerator enumTokens = p.Value.ToString().Split(semiSep).GetEnumerator();

							// First token should be the number itself
							enumTokens.MoveNext();

							if ((string) enumTokens.Current == this.phoneNumber)
							{
								p.Delete();
								break;
							}
						}

						// Add the serialized one back
						this.thisNode.Properties.AddProperty(Common.phoneProperty, this.Serialize());
						this.parentContact.SetDirty();
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Address: actual telephone number
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
				else
				{
					return("");
				}
			}

			set
			{
				// If this object is attached to the store - fix it up
				if (this.thisNode != null)
				{
					char[] semiSep = new char[]{';'};
					MultiValuedList	mList = this.thisNode.Properties.GetProperties(Common.phoneProperty);
					foreach(Property p in mList)
					{
						IEnumerator enumTokens = p.Value.ToString().Split(semiSep).GetEnumerator();

						// First token should be the number itself
						enumTokens.MoveNext();

						if ((string) enumTokens.Current == this.phoneNumber)
						{
							p.Delete();
							break;
						}
					}

					// Add the serialized one back
					this.phoneNumber = value;
					this.thisNode.Properties.AddProperty(Common.phoneProperty, this.Serialize());
					this.parentContact.SetDirty();
				}
				else
				{
					this.phoneNumber = value;
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

				// If this object is attached to the store - fix it up
				if (this.thisNode != null)
				{
					char[] semiSep = new char[]{';'};
					MultiValuedList	mList = this.thisNode.Properties.GetProperties(Common.phoneProperty);
					foreach(Property p in mList)
					{
						IEnumerator enumTokens = p.Value.ToString().Split(semiSep).GetEnumerator();

						// First token should be the telephone number itself 
						enumTokens.MoveNext();

						if ((string) enumTokens.Current == this.phoneNumber)
						{
							p.Delete();
							break;
						}
					}

					// Add the serialized one back
					this.thisNode.Properties.AddProperty(Common.phoneProperty, this.Serialize());
					this.parentContact.SetDirty();
				}
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

		internal Telephone(Collection parentCollection, Node parentNode, string serializedNumber)
		{
			this.parentCollection = parentCollection;
			this.thisNode = parentNode;
			this.Unserialize(serializedNumber);
		}

		internal Telephone(Collection parentCollection, Node parentNode, Contact contact, string serializedNumber)
		{
			this.parentCollection = parentCollection;
			this.thisNode = parentNode;
			this.parentContact = contact;
			this.Unserialize(serializedNumber);
		}

		#endregion

		#region Private Methods
		internal bool Add(Collection collection, Node node, Contact contact)
		{
			this.parentCollection = collection;
			this.thisNode = node;
			this.parentContact = contact;

			//
			// If the telephone number already exists, delete the existing property
			//

			foreach(Telephone tmpPhone in contact.GetTelephoneNumbers())
			{
				if(tmpPhone.Number == this.Number)
				{
					Property p = new Property(Common.phoneProperty, tmpPhone.Serialize());
					this.thisNode.Properties.DeleteSingleProperty(p);
					break;
				}
			}

			if (this.Preferred == true)
			{
				//
				// If another property is already preferred delete, change it
				//

				foreach(Telephone tmpPhone in contact.GetTelephoneNumbers())
				{
					if(tmpPhone.Preferred == true)
					{
						Property p = new Property(Common.phoneProperty, tmpPhone.Serialize());
						this.thisNode.Properties.DeleteSingleProperty(p);
						tmpPhone.Preferred = false;
						p.SetValue(tmpPhone.Serialize());
						this.thisNode.Properties.AddProperty(p);
						break;
					}
				}
			}

			//
			// Add the phone to the store
			//

			Property p1 = new Property(Common.phoneProperty, this.Serialize());
			this.thisNode.Properties.AddProperty(p1);

			//this.thisNode.Save();
			this.parentContact.SetDirty();
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
		/// Commits the telehone object to the store
		/// </summary>
		[ Obsolete( "This method is marked for eventual removal. It's no longer necessary to call 'Commit'.", false ) ]
		public void Commit()
		{
			try
			{
				if (this.parentCollection != null)
				{
					this.parentCollection.Commit();
				}
			}
			catch{}
		}

		/// <summary>
		/// Delete this TEL from the phone list attached to the contact record
		/// </summary>
		public void Delete()
		{
			try
			{
				Property p = new Property(Common.phoneProperty, this.Serialize());
				p.Delete();
			}
			catch{}
			return;
		}

		#endregion
	}
}
