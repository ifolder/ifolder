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
using Simias;
using Simias.Storage;

namespace Novell.AddressBook
{
	/// <summary>
	/// The default type is "work", "postal" and "parcel".
	/// There can exist only one "preferred" address per contact.
	/// </summary>
	public enum AddressTypes : uint
	{
		/// <summary>
		/// indicates a preferred-use address
		/// </summary>
		preferred = 0x00000001,

		/// <summary>
		/// indicates a delivery address for a residence
		/// </summary>
		home =		0x00000002,

		/// <summary>
		/// indicates a delivery address for a place of work
		/// </summary>
		work =		0x00000004,

		/// <summary>
		/// indicates a delivery address for somewhere other than primary residence or work
		/// ex. vacation home
		/// </summary>
		other =		0x00000008,

		/// <summary>
		/// indicates a domestic delivery address
		/// </summary>
		dom =		0x00000010,

		/// <summary>
		/// indicates an international delivery address
		/// </summary>
		intl =		0x00000020,

		/// <summary>
		/// indicates a postal address
		/// </summary>
		postal =	0x00000040,

		/// <summary>
		/// indicates a parcel address
		/// </summary>
		parcel =    0x00000080
	}

	/// <summary>
	/// Summary description for an address.
    /// An address is equivalent to a Node in the collection server.
	/// </summary>
	public class Address : Node
	{
		#region Class Members
		private Contact			parentContact;
		private Group			parentGroup;
//		private bool			preferred = false;
		private AddressTypes	defaultTypes = AddressTypes.work | AddressTypes.parcel | AddressTypes.postal;

		#endregion

		#region Properties

		/// <summary>
		/// Type - type of address 
		///
		/// Type Value: Single value integer
		///
		/// Example: Home, Work, Apartment, etc.
		///
		/// </summary>

		public AddressTypes Types
		{

			get
			{	
				try
				{
					return((AddressTypes) this.Properties.GetSingleProperty( Common.addressTypesProperty ).Value);
				}
				catch{}
				return(0);
			}

			set
			{
				try
				{
					if (value != 0)
					{
						this.Properties.ModifyProperty( 
							Common.addressTypesProperty, 
							value );
					}
					else
					{
						this.Properties.ModifyProperty( 
							Common.addressTypesProperty, 
							this.defaultTypes);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Street - street address
		///
		/// Type Value: A single text value
		///
		/// Example: 1800 South Novell Place
		///
		/// </summary>
		public string Street
		{
			get
			{	
				try
				{
					return(this.Properties.GetSingleProperty( Common.streetProperty ).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty( Common.streetProperty, value );
					}
					else
					{
						this.Properties.DeleteProperties( Common.streetProperty );
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Locality -
		///
		/// Type Value: A single text value
		///
		/// Example: Provo
		///
		/// </summary>
		public string Locality
		{
			get
			{	
				try
				{
					return(this.Properties.GetSingleProperty( Common.localityProperty ).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty( Common.localityProperty, value );
					}
					else
					{
						this.Properties.DeleteProperties( Common.localityProperty );
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Region -
		///
		/// Type Value: A single text value
		///
		/// Example: UT
		///
		/// </summary>
		public string Region
		{
			get
			{	
				try
				{
					return(this.Properties.GetSingleProperty( Common.regionProperty ).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty( Common.regionProperty, value );
					}
					else
					{
						this.Properties.DeleteProperties( Common.regionProperty );
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Country - Specifies the
		///
		/// Type Value: A single text value
		///
		/// Example: USA - United States of America
		///
		/// </summary>
		public string Country
		{
			get
			{	
				try
				{
					return(this.Properties.GetSingleProperty( Common.countryProperty ).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty( Common.countryProperty, value );
					}
					else
					{
						this.Properties.DeleteProperties( Common.countryProperty );
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// PostalBox - Post Office Box
		///
		/// Type Value: A single text value
		///
		/// Example:
		///
		/// </summary>
		public string PostalBox
		{
			get
			{	
				try
				{
					return(this.Properties.GetSingleProperty( Common.poBoxProperty ).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty( Common.poBoxProperty, value );
					}
					else
					{
						this.Properties.DeleteProperties( Common.poBoxProperty );
					}
				}
				catch{}
			}

		}

		/// <summary>
		/// ExtendedAddress - Extended Address
		///
		/// Type Value: A single text value
		///
		/// Example:
		///
		/// </summary>
		public string ExtendedAddress
		{
			get
			{	
				try
				{
					return(this.Properties.GetSingleProperty( Common.extendedProperty ).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty( Common.extendedProperty, value );
					}
					else
					{
						this.Properties.DeleteProperties( Common.extendedProperty );
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// PostalCode - postal or zip code
		///
		/// Type Value: A single text value
		///
		/// Example: 84606
		///
		/// </summary>
		public string PostalCode
		{
			get
			{
				try
				{
					return(this.Name);
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Name = value;
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// MailStop - Contact's mailstop location
		/// Note! this property is not a standard vCard property
		///
		/// Type Value: A single text value
		///
		/// Example: PRV-H311
		///
		/// </summary>
		public string MailStop
		{
			get
			{	
				try
				{
					return(this.Properties.GetSingleProperty( Common.mailStopProperty ).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty( Common.mailStopProperty, value );
					}
					else
					{
						this.Properties.DeleteProperties( Common.mailStopProperty );
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Preferred - The contact's preferred address
		///
		/// Type Value: single value - boolean
		///
		/// </summary>
		public bool Preferred
		{
			get
			{	
				try
				{
					AddressTypes addrTypes = (AddressTypes)
						this.Properties.GetSingleProperty(
							Common.addressTypesProperty).Value;
					if ((addrTypes & AddressTypes.preferred) == AddressTypes.preferred)
					{
						return(true);
					}
				}
				catch{}
				return(false);
			}

			set
			{
				try
				{
					AddressTypes addrTypes = (AddressTypes)
						this.Properties.GetSingleProperty(
							Common.addressTypesProperty).Value;

					if (value == true)
					{
						addrTypes |= AddressTypes.preferred;
					}
					else
					{
						addrTypes &= ~AddressTypes.preferred;
					}

					this.Properties.ModifyProperty( Common.addressTypesProperty, addrTypes );
				}
				catch{}
			}	
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Simple Address constructor
		/// </summary>
		public Address() : base("")
		{
			this.Properties.ModifyProperty(
				Common.addressTypesProperty, 
				this.defaultTypes);
		}

		/// <summary>
		/// An Address constructor which includes the zip code
		/// Zip code is a mandatory property for ADR
		/// </summary>
		public Address(string zip) : base(zip)
		{
			this.Properties.ModifyProperty(
				Common.addressTypesProperty, 
				this.defaultTypes);
		}

		internal Address(Contact parentContact) : base("")
		{
			this.parentContact = parentContact;
			this.Properties.ModifyProperty( 
				Common.addressTypesProperty, 
				this.defaultTypes);
		}

		internal Address(Contact parentContact, Node cNode) : base(cNode)
		{
			this.parentContact = parentContact;
		}

		internal Address(Group parent, Node cNode) : base(cNode)
		{
			this.parentGroup = parent;
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// Adds a structured address property to a contact.
		/// If the method fails an exception is raised
		/// </summary>
		/// <param name="parentContact"></param>
		/// <remarks>
		/// The address is not commited to the store until a commit on the 
		/// contact is called.
		/// </remarks>
		/// <returns>nothing if successful - raises an application exception if an error occurs.</returns>
		internal void Add(Contact parentContact)
		{
			this.parentContact = parentContact;

			// If we're preferred make sure nobody else is
			if (this.Preferred == true)
			{
				foreach(Address cAddr in parentContact.addressList)
				{
					if (cAddr.Preferred == true)
					{
						cAddr.Preferred = false;
					}
				}
			}

			//
			// Add a relationship for Address node to Contact node.
			//

			// Can't create and add the relationship object if the contact
			// hasn't been attached to the address book
			if (parentContact.addressBook != null)
			{
				Relationship parentChild = new 
					Relationship( 
						parentContact.addressBook.ID, 
						parentContact.ID );

				this.Properties.AddProperty( Common.addressToContact, parentChild );
			}

			this.parentContact.addressList.Add(this);
		}

		/// <summary>
		/// Adds a structured address property to a Group.
		/// If the method fails an exception is raised
		/// </summary>
		/// <param name="parent"></param>
		/// <remarks>
		/// The address is not commited to the store until a commit on the 
		/// Group is called.
		/// </remarks>
		/// <returns>nothing if successful - raises an application exception if an error occurs.</returns>
		internal void Add(Group parent)
		{
			this.parentGroup = parent;

			// If we're preferred make sure nobody else is
			if (this.Preferred == true)
			{
				foreach(Address cAddr in parentGroup.addressList)
				{
					if (cAddr.Preferred == true)
					{
						cAddr.Preferred = false;
					}
				}
			}

			//
			// Add a relationship for Address node to Contact node.
			//

			// Can't create and add the relationship object if the contact
			// hasn't been attached to the address book
			if (parentContact.addressBook != null)
			{
				Relationship parentChild = new 
					Relationship( 
					parentContact.addressBook.ID, 
					parentGroup.ID );

				this.Properties.AddProperty( Common.addressToGroup, parentChild );
			}

			this.parentGroup.addressList.Add(this);
		}

		internal static bool PrepareToCommit(Contact contact)
		{
			// Anything in the list to persist?
			if (contact.addressList.Count == 0)
			{
				return(false);
			}

			// assume no preferred is set
			bool foundPreferred = false;

			// Make sure we have a preferred
			foreach(Address tmpAddress in contact.addressList)
			{
				if (tmpAddress.Preferred == true)
				{
					foundPreferred = true;
					break;
				}
			}

			if (foundPreferred == false)
			{
				// No preferred do we have one typed WORK?
				foreach(Address tmpAddress in contact.addressList)
				{
					AddressTypes addrTypes = (AddressTypes)
						tmpAddress.Properties.GetSingleProperty(Common.addressTypesProperty).Value;

					if ((addrTypes & AddressTypes.work) == AddressTypes.work)
					{
						tmpAddress.Preferred = true;
						break;
					}
				}

				// Any will do
				if (foundPreferred == false)
				{
					foreach(Address tmpAddress in contact.addressList)
					{
						tmpAddress.Preferred = true;
						break;
					}
				}
			}
			return(true);
		}

		internal static bool PrepareToCommit(Group cGroup)
		{
			// Anything in the list to persist?
			if (cGroup.addressList.Count == 0)
			{
				return(false);
			}

			// assume no preferred is set
			bool foundPreferred = false;

			// Make sure we have a preferred
			foreach(Address tmpAddress in cGroup.addressList)
			{
				if (tmpAddress.Preferred == true)
				{
					foundPreferred = true;
					break;
				}
			}

			if (foundPreferred == false)
			{
				// No preferred do we have one typed WORK?
				foreach(Address tmpAddress in cGroup.addressList)
				{
					AddressTypes addrTypes = (AddressTypes)
						tmpAddress.Properties.GetSingleProperty(Common.addressTypesProperty).Value;

					if ((addrTypes & AddressTypes.work) == AddressTypes.work)
					{
						tmpAddress.Preferred = true;
						break;
					}
				}

				// Any will do
				if (foundPreferred == false)
				{
					foreach(Address tmpAddress in cGroup.addressList)
					{
						tmpAddress.Preferred = true;
						break;
					}
				}
			}
			return(true);
		}

		/// <summary>
		/// Retrieves an address object from the store and sets all
		/// value properties in the object.
		/// </summary>
		internal void ToObject(Contact parentContact, string addressID)
		{
			this.parentContact = parentContact;

			/*
			try
			{
				this.thisNode = this.parentContact.addressBook.collection.GetNodeByID(addressID);
				if (this.thisNode != null)
				{
					this.id = this.ID;

					try
					{
						//this.zip = this.thisNode.Properties.GetSingleProperty( Common.zipProperty ).ToString();
						this.zip = this.Name;
					}
					catch{}

					try
					{
						this.addressTypes = (AddressTypes)
							this.thisNode.Properties.GetSingleProperty( Common.addressTypesProperty ).Value;
					}
					catch{}


					// Non-mandatory properties may not exist
					try
					{
						this.street = this.thisNode.Properties.GetSingleProperty( Common.streetProperty ).ToString();
					}
					catch{}

					try
					{
						this.locality = this.thisNode.Properties.GetSingleProperty( Common.localityProperty ).ToString();
					}
					catch{}

					try
					{
						this.region = this.thisNode.Properties.GetSingleProperty( Common.regionProperty ).ToString();
					}
					catch{}

					try
					{
						this.poBox = this.thisNode.Properties.GetSingleProperty( Common.poBoxProperty ).ToString();
					}
					catch{}

					try
					{
						this.extAddress = this.thisNode.Properties.GetSingleProperty( Common.extendedProperty ).ToString();
					}
					catch{}

					try
					{
						this.mailStop = this.thisNode.Properties.GetSingleProperty( Common.mailStopProperty ).ToString();
					}
					catch{}

					try
					{
						this.country = this.thisNode.Properties.GetSingleProperty( Common.countryProperty ).ToString();
					}
					catch{}
				}
			}
			catch
			{
				throw new ApplicationException(Common.addressBookExceptionHeader + "Address node not found");
			}
			*/
		}

		#endregion

		#region Public Methods


		/// <summary>
		/// Deletes the current address
		/// </summary>
		public void Delete()
		{
			if (this.parentContact != null)
			{
				this.parentContact.addressList.Remove(this);
				if (this.parentContact.addressBook != null)
				{
					this.parentContact.addressBook.Commit(
						this.parentContact.addressBook.Delete(this));
				}
			}
		}

		#endregion
	}
}
