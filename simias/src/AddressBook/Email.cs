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
using System.Text.RegularExpressions;
using Simias.Storage;

namespace Novell.AddressBook
{
	/// <summary>
	/// The default type is "work" and "internet".
	/// There can exist only one "preferred" email address per contact.
	/// "internet" and "x400" are exclusive to each other
	/// </summary>
	public enum EmailTypes : uint
	{
		/// <summary>
		/// indicates a preferred-use email address
		/// </summary>
		preferred = 0x00000001,

		/// <summary>
		/// indicates an email address for personal activity
		/// </summary>
		personal =	0x00000002,

		/// <summary>
		/// indicates an email address for work activity
		/// </summary>
		work =		0x00000004,

		/// <summary>
		/// indicates an email address for other activity besides personal or work
		/// </summary>
		other =		0x00000008,

		/// <summary>
		/// indicates an email address of type internet
		/// </summary>
		internet =  0x00000100,

		/// <summary>
		/// indicates an email address of type x.400
		/// </summary>
		x400 =      0x00000200
	}

	/// <summary>
	/// As per the vCard 3.0 spec, a Contact may contain multiple
	///	email addresses.  Each email address also contains a Type
	///	property as well as an Association property.
	///	A vCard 3.0 compliant email address could look like
	///	the following: EMAIL;TYPE=internet,pref:jane_doe@abc.com
	///	
	///	Type may contain one of the following 
	/// values: "internet" or "x400"
	///	Association may contain one of the following
	/// values: "personal", "work" or "other"
	///
	///	Also, the owner of a Contact may designate one email
	///	address as "preferred"
	///
	///	Email addresses are defaulted to: Type=internet and 
	///	Association=work
	/// </summary>

	public class Email
	{
		#region Class Members
		Contact			parentContact;
		string			address;
		EmailTypes		emailTypes;
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
				if ((this.emailTypes & EmailTypes.preferred) == EmailTypes.preferred)
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
					this.emailTypes |= EmailTypes.preferred;
					if (this.parentContact != null)
					{
						// There can only be one preferred make sure it's this object
						foreach (Email tmpMail in this.parentContact.emailList)
						{
							if (tmpMail.address != this.address)
							{
								if (tmpMail.Preferred == true)
								{
									tmpMail.Preferred = false;
								}
							}
						}
					}
				}
				else
				{
					this.emailTypes &= ~EmailTypes.preferred;
				}

				/*
				if (this.parentContact != null)
				{
					this.parentContact.SetDirty(ChangeMap.email);
				}
				*/
			}
		}

		/// <summary>
		/// Type: vCard addressing/association types for e-mail
		/// valid addressing types: "internet" or "x400"
		/// valid association types: "work", "personal", "other"
		///
		/// Type Value: EmailType
		/// </summary>
		public EmailTypes Types
		{
			get
			{
				return(this.emailTypes);
			}

			set
			{
				this.emailTypes = value;

				// internet and x400 are mutually exclusive
				if (((this.emailTypes & EmailTypes.internet) == EmailTypes.internet) &&
					((this.emailTypes & EmailTypes.x400) == EmailTypes.x400))
				{
					this.emailTypes &= ~EmailTypes.x400;
				}

				/*
				if (this.parentContact != null)
				{
					this.parentContact.SetDirty(ChangeMap.email);
				}
				*/
			}
		}

		/// <summary>
		/// Address: actual e-mail address
		/// Type Value: String
		/// ex. johndoe@novell.com
		/// </summary>
		public string Address
		{
			get
			{
				return(this.address);
			}

			set
			{
				this.address = value;
				if (this.parentContact != null)
				{
					// address distinguishes so there can't be two email objects with the same address
					foreach (Email tmpMail in this.parentContact.emailList)
					{
						if (tmpMail != this)
						{
							if (tmpMail.address == this.address)
							{
								this.parentContact.emailList.Remove(tmpMail);
							}
						}
					}

					/*
					this.parentContact.SetDirty(ChangeMap.email);
					*/
				}
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Simepl Email constructor
		/// </summary>
		public Email()
		{
			this.emailTypes = (EmailTypes.internet | EmailTypes.work);
		}	

		/// <summary>
		/// Email constructor which includes type and address
		/// </summary>
		public Email(EmailTypes types, string address)
		{
			this.address = address;

			// internet and x400 are mutually exclusive
			if (((types & EmailTypes.internet) == EmailTypes.internet) &&
				((types & EmailTypes.x400) == EmailTypes.x400))
			{
				types &= ~EmailTypes.x400;
			}

			this.emailTypes = types;
		}

		internal Email(Collection parentCollection, Node parentNode, string serializedEmail)
		{
//			this.parentCollection = parentCollection;
//			this.thisNode = parentNode;
			this.Unserialize(serializedEmail);
		}

		internal Email(Contact contact, string serializedEmail)
		{
			this.parentContact = contact;
			this.Unserialize(serializedEmail);
		}

		#endregion

		#region Private Methods

		internal static bool PersistToStore(Contact contact)
		{
			// The contact needs to be attached to the store in order to persist
			//if (contact.thisNode == null)
			//{
			//	return(false);
			//}

			// Anything in the list to persist?
			if (contact.emailList.Count == 0)
			{
				return(false);
			}

			// First delete the property
			contact.Properties.DeleteProperties(Common.emailProperty);

			// assume no preferred is set
			bool foundPreferred = false;

			// Make sure we have a preferred
			foreach(Email tmpMail in contact.emailList)
			{
				if (tmpMail.Preferred == true)
				{
					foundPreferred = true;
					break;
				}
			}

			if (foundPreferred == false)
			{
				// No preferred do we have one typed WORK?
				foreach(Email tmpMail in contact.emailList)
				{
					if ((tmpMail.emailTypes & EmailTypes.work) == EmailTypes.work)
					{
						tmpMail.Preferred = true;
						break;
					}
				}

				// Any will do
				if (foundPreferred == false)
				{
					foreach(Email tmpMail in contact.emailList)
					{
						tmpMail.Preferred = true;
						break;
					}
				}
			}

			// To the collection store they go!
			foreach(Email tmpMail in contact.emailList)
			{
				Property p = new Property(Common.emailProperty, tmpMail.Serialize());
				contact.Properties.AddProperty(p);
			}

			return(true);
		}

		internal bool Add(Contact contact)
		{
			this.parentContact = contact;

			//
			// Make sure the email object contains a valid type and
			// association
			//

			// internet and x400 are mutually exclusive
			if (((this.emailTypes & EmailTypes.internet) == EmailTypes.internet) &&
				((this.emailTypes & EmailTypes.x400) == EmailTypes.x400))
			{
				this.emailTypes &= ~EmailTypes.x400;
			}

			//
			// If the email address exists, delete the existing property
			//

			foreach(Email tmpMail in contact.emailList)
			{
				if(tmpMail.Address == this.Address)
				{
					contact.emailList.Remove(tmpMail);
					break;
				}
			}

			if (this.Preferred == true)
			{
				//
				// If another property is already preferred delete, change it
				//

				foreach(Email tmpMail in contact.emailList)
				{
					if(tmpMail.Preferred == true)
					{
						tmpMail.Preferred = false;
					}
				}
			}

			//
			// Add the new email address to the list
			//

			contact.emailList.Add(this);
			//contact.SetDirty(ChangeMap.email);
			return(true);
		}

		private bool Unserialize(string sMail)
		{
			// take a serialized email string and load up
			// the class member with valid tokens

			int	index = 0;

			// Email is the first item in the serialized string
			this.address = "";
			while(index < sMail.Length && sMail[index] != ';')
			{
				this.address += Convert.ToString(sMail[index++]);
			}

			// An e-mail address must exist in the serialized string to be valid
			if (this.address == "")
			{
				return(false);
			}

			// past the ;
			index++;
			string tmpType = "";
			while(index < sMail.Length)
			{
				tmpType += Convert.ToString(sMail[index++]);
			}

			if (tmpType != "")
			{
				this.emailTypes = (EmailTypes) Convert.ToUInt32(tmpType, 16);

				// internet and x400 are mutually exclusive
				if (((this.emailTypes & EmailTypes.internet) == EmailTypes.internet) &&
					((this.emailTypes & EmailTypes.x400) == EmailTypes.x400))
				{
					this.emailTypes &= ~EmailTypes.x400;
				}
			}
			else
			{
				this.emailTypes = (EmailTypes.work | EmailTypes.internet);
			}
			return(true);
		}

		private string Serialize()
		{
			string	serialized;

			// mail address must be present
			if (this.address == null)
			{
				return(null);
			}

			// e-mail address is first so begins with searches work
			// correctly.
			serialized = this.address + ";";
			serialized += Convert.ToString((uint) this.emailTypes, 16);

			return(serialized);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Delete this e-mail from the e-mail list attached to the contact record
		/// </summary>
		public void Delete()
		{
			try
			{
				if (this.parentContact != null)
				{
					this.parentContact.emailList.Remove(this);
					//this.parentContact.SetDirty(ChangeMap.email);
				}
			}
			catch{}
			return;
		}

		#endregion
	}
}
