/***********************************************************************
 *  Email.cs - Class definition for a vCard 3.0 email address
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
		Collection		parentCollection;
		Node			thisNode;
		Contact			parentContact;
		string			address;
//		string			originalSerializedMail;
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
				}
				else
				{
					this.emailTypes &= ~EmailTypes.preferred;
				}

				try
				{
					// If this object is attached to the store - fix it up
					if (this.thisNode != null)
					{
						char[] semiSep = new char[]{';'};
						MultiValuedList mList = null;

						// Remove the preferred bit off any other email properties
						if (value == true)
						{
							mList = this.thisNode.Properties.GetProperties(Common.emailProperty);
							foreach(Property p in mList)
							{
								IEnumerator enumTokens = p.Value.ToString().Split(semiSep).GetEnumerator();

								// First token should be the address itself (ex. banderso@novell.com)
								if (enumTokens.MoveNext())
								{
									string	tmpAddress = (string) enumTokens.Current;

									// Now the types
									if (enumTokens.MoveNext())
									{
										EmailTypes tmpTypes = 
											(EmailTypes) Convert.ToUInt32((string) enumTokens.Current, 16);
										if (tmpAddress != this.address && 
											((tmpTypes & EmailTypes.preferred) == EmailTypes.preferred))
										{
											tmpTypes &= ~EmailTypes.preferred;
											tmpAddress += ";" + Convert.ToString((uint) tmpTypes, 16);
											p.Value = tmpAddress;

											this.thisNode.Properties.ModifyProperty(p);
										}
									}
								}
							}
						}

						mList = this.thisNode.Properties.GetProperties(Common.emailProperty);
						foreach(Property p in mList)
						{
							IEnumerator enumTokens = p.Value.ToString().Split(semiSep).GetEnumerator();

							// First token should be the address itself (ex. banderso@novell.com)
							enumTokens.MoveNext();

							if ((string) enumTokens.Current == this.address)
							{
								p.Delete();
								break;
							}
						}

						// Add the serialized one back
						this.thisNode.Properties.AddProperty(Common.emailProperty, this.Serialize());
						this.parentContact.SetDirty();
					}
				}
				catch{}
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

				try
				{
					// If this object is attached to the store - fix it up
					if (this.thisNode != null)
					{
						char[] semiSep = new char[]{';'};
						MultiValuedList	mList = this.thisNode.Properties.GetProperties(Common.emailProperty);
						foreach(Property p in mList)
						{
							IEnumerator enumTokens = p.Value.ToString().Split(semiSep).GetEnumerator();

							// First token should be the address itself (ex. banderso@novell.com)
							enumTokens.MoveNext();

							if ((string) enumTokens.Current == this.address)
							{
								p.Delete();
								break;
							}
						}

						// Add the serialized one back
						this.thisNode.Properties.AddProperty(Common.emailProperty, this.Serialize());
						this.parentContact.SetDirty();
					}
				}
				catch{}
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
				try
				{
					// If this object is attached to the store - fix it up
					if (this.thisNode != null)
					{
						char[] semiSep = new char[]{';'};
						MultiValuedList	mList = this.thisNode.Properties.GetProperties(Common.emailProperty);
						foreach(Property p in mList)
						{
							IEnumerator enumTokens = p.Value.ToString().Split(semiSep).GetEnumerator();

							// First token should be the address itself (ex. banderso@novell.com)
							enumTokens.MoveNext();

							if ((string) enumTokens.Current == this.address)
							{
								p.Delete();
								break;
							}
						}

						// Add the serialized one back
						this.address = value;
						this.thisNode.Properties.AddProperty(Common.emailProperty, this.Serialize());
						this.parentContact.SetDirty();
					}
					else
					{
						this.address = value;
					}
				}
				catch{}
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
			this.parentCollection = parentCollection;
			this.thisNode = parentNode;
			this.Unserialize(serializedEmail);
		}

		internal Email(Collection parentCollection, Node parentNode, Contact contact, string serializedEmail)
		{
			this.parentCollection = parentCollection;
			this.thisNode = parentNode;
			this.parentContact = contact;
			this.Unserialize(serializedEmail);
		}

		#endregion

		#region Private Methods
		internal bool Add(Collection collection, Node node, Contact contact)
		{
			this.parentCollection = collection;
			this.thisNode = node;
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

			foreach(Email tmpMail in contact.GetEmailAddresses())
			{
				if(tmpMail.Address == this.Address)
				{
					Property p = new Property(Common.emailProperty, tmpMail.Serialize());
					this.thisNode.Properties.DeleteSingleProperty(p);
					break;
				}
			}

			if (this.Preferred == true)
			{
				//
				// If another property is already preferred delete, change it
				//

				foreach(Email tmpMail in contact.GetEmailAddresses())
				{
					if(tmpMail.Preferred == true)
					{
						Property p = new Property(Common.emailProperty, tmpMail.Serialize());
						this.thisNode.Properties.DeleteSingleProperty(p);
						tmpMail.Preferred = false;
						p.SetValue(tmpMail.Serialize());
						this.thisNode.Properties.AddProperty(p);
						break;
					}
				}
			}

			//
			// Add the address to the store
			//

			Property p1 = new Property(Common.emailProperty, this.Serialize());
			this.thisNode.Properties.AddProperty(p1);
			this.parentContact.SetDirty();

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
		/// Commits an email address
		/// !NOTE! obsolete in the future
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
		/// Delete this e-mail from the e-mail list attached to the contact record
		/// </summary>
		public void Delete()
		{
			try
			{
				Property p = new Property(Common.emailProperty, this.Serialize());
				p.Delete();
			}
			catch{}
			return;
		}

		#endregion
	}
}
