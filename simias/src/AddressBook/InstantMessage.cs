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
	/// The default type is "work". 
	///
	/// "home", "work" and "other" are not mutually exclusive.
	/// A contact may use one IM address for work and home.
	/// There can exist only one "preferred" instant message
	/// per contact.
	/// </summary>
	public enum IMTypes : uint
	{
		/// <summary>
		/// indicates a preferred-use instant message address
		/// </summary>
		preferred = 0x00000001,

		/// <summary>
		/// indicates an instant message address used primarily at home
		/// </summary>
		home =		0x00000002,

		/// <summary>
		/// indicates an instant message address used primary at work
		/// </summary>
		work =		0x00000004,

		/// <summary>
		/// indicates an instant message address not used for work or home
		/// </summary>
		other =		0x00000008,
	}

	/// <summary>
	/// As per the vCard 3.0 spec, a Contact may contain multiple
	///	phone numbers.
	/// </summary>
	public class IM
	{
		#region Class Members
		Contact						parentContact;
		string						address;
		string						provider;
		private const IMTypes		defaultIMTypes = (IMTypes.preferred | IMTypes.work | IMTypes.home);
		IMTypes						types = defaultIMTypes;
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
				if ((this.types & IMTypes.preferred) == IMTypes.preferred)
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
					this.types |= IMTypes.preferred;
					if (this.parentContact != null)
					{
						// There can only be one preferred make sure it's this object
						foreach (IM cIM in this.parentContact.imList)
						{
							if (cIM.address != this.address)
							{
								if (cIM.Preferred == true)
								{
									cIM.Preferred = false;
								}
							}
						}
					}
				}
				else
				{
					this.types &= ~IMTypes.preferred;
				}
			}
		}

		/// <summary>
		/// Address: actual IM address or moniker
		/// Type Value: String
		/// ex. igolfinutah
		/// </summary>
		public string Address
		{
			get
			{
				if (this.address != null)
				{
					return(this.address);
				}

				return("");
			}

			set
			{
				this.address = value;
				if (this.parentContact != null)
				{
					// address and provider distinguishes so there can't be two IM objects
					// with the same address and provider
					foreach (IM cIM in this.parentContact.imList)
					{
						if (cIM != this)
						{
							if ((cIM.address == this.address) &&
								(cIM.provider == this.provider))
							{
								this.parentContact.imList.Remove(cIM);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Provider: Instant Message provider
		/// Type Value: String
		/// ex. GroupWise
		/// </summary>
		public string Provider
		{
			get
			{
				if (this.provider != null)
				{
					return(this.provider);
				}

				return("");
			}

			set
			{
				this.provider = value;
				if (this.parentContact != null)
				{
					// address and provider distinguishes so there can't be two IM objects
					// with the same address and provider
					foreach (IM cIM in this.parentContact.imList)
					{
						if (cIM != this)
						{
							if ((cIM.address == this.address) &&
								(cIM.provider == this.provider))
							{
								this.parentContact.imList.Remove(cIM);
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
		public IMTypes Types
		{
			get
			{
				return(this.types);
			}

			set
			{
				try
				{
					this.types = value;
				}
				catch{}
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Simple IM constructor
		/// </summary>
		public IM()
		{
		}	

		/// <summary>
		/// IM constructor which includes the address and provider
		/// </summary>
		public IM(string address, string provider)
		{
			this.address = address;
			this.provider = provider;
		}

		/// <summary>
		/// IM constructor which includes all properties
		/// </summary>
		public IM(string address, string provider, IMTypes types)
		{
			this.address = address;
			this.provider = provider;
			this.types = types;
		}

		internal IM(Contact contact, string serializedIM)
		{
			this.parentContact = contact;
			this.Unserialize(serializedIM);
		}
		#endregion

		#region Private Methods
		internal bool Add(Contact contact)
		{
			this.parentContact = contact;

			//
			// If the address/provider exists, delete the existing one
			//

			foreach(IM cIM in contact.imList)
			{
				if(cIM.Address == this.address &&
					cIM.Provider == this.provider)
				{
					contact.imList.Remove(cIM);
					break;
				}
			}

			if (this.Preferred == true)
			{
				//
				// If another IM object is already preferred, change it
				//

				foreach(IM cIM in contact.imList)
				{
					if(cIM.Preferred == true)
					{
						cIM.Preferred = false;
					}
				}
			}

			//
			// Add the new IM object to the list
			//

			contact.imList.Add(this);
			return(true);
		}

		internal static bool PersistToStore(Contact contact)
		{
			// Anything in the list to persist?
			if (contact.imList.Count == 0)
			{
				return(false);
			}

			// First delete the existing property
			contact.Properties.DeleteProperties(Common.imProperty);

			// assume no preferred is set
			bool foundPreferred = false;

			// Make sure we have a preferred
			foreach(IM cIM in contact.imList)
			{
				if (cIM.Preferred == true)
				{
					foundPreferred = true;
					break;
				}
			}

			if (foundPreferred == false)
			{
				// No preferred so set the first one in the list
				foreach(IM cIM in contact.imList)
				{
					cIM.Preferred = true;
					break;
				}
			}

			// To the collection store they go!
			foreach(IM cIM in contact.imList)
			{
				Property p = new Property(Common.imProperty, cIM.Serialize());
				contact.Properties.AddProperty(p);
			}

			return(true);
		}

		// Currently the instant message object is serialized in the collection
		// store in the following format: address;provider;types
		// ex. igolfinutah;AIM;0x03
		private bool Unserialize(string sIM)
		{
			// take a serialized instant message string and load up
			// the class members with valid values

			// Actual address comes first
			int index = 0;
			this.address = "";
			while(index < sIM.Length && sIM[index] != ';')
			{
				this.address += Convert.ToString(sIM[index++]);
			}

			// past the ;
			index++;
			this.provider = "";
			while(index < sIM.Length && sIM[index] != ';')
			{
				this.provider += Convert.ToString(sIM[index++]);
			}

			index++;
			string tmpType = "";
			while(index < sIM.Length)
			{
				tmpType += Convert.ToString(sIM[index++]);
			}

			if (tmpType != "")
			{
				this.types = (IMTypes) Convert.ToUInt32(tmpType, 16);
			}

			return(true);
		}

		private string Serialize()
		{
			string	serialized;

			// address and provider must be present
			if (this.address == null || this.provider == null)
			{
				return(null);
			}

			serialized = this.address + ";" + this.provider + ";";
			serialized += Convert.ToString((uint) this.types, 16);
			return(serialized);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Delete this IM from the instant message list attached to the contact record
		/// </summary>
		public void Delete()
		{
			try
			{
				if (this.parentContact != null)
				{
					this.parentContact.imList.Remove(this);
				}
			}
			catch{}
			return;
		}

		#endregion
	}
}
