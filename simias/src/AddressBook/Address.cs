/***********************************************************************
 *  Address.cs- Address Class for implementing the ADR property
 *  in the vCard 3.0 specification.
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
	public class Address
	{
		#region Class Members
		/// <summary>
		/// Well known address book types.
		/// </summary>
		//public enum AddressType { Personal, Work, Vacation, Other };

		bool					changed = false;
		private Collection		parentCollection;
		private Node			parentNode;
		private Contact			parentContact;
        private string			parentNodeID;
		private Node			thisNode;
		private string			locality;
		private string			region;
		private string			country;
		private string			zip;
		private string			street;
		private string			poBox;
		private string			extAddress;
		private string			mailStop;
		private bool			preferred;
		private AddressTypes	addressTypes = AddressTypes.work | AddressTypes.parcel | AddressTypes.postal;

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
				if (thisNode != null)
				{
					addressTypes = (AddressTypes)
						thisNode.Properties.GetSingleProperty( Common.addressTypesProperty ).Value;
				}

				return(addressTypes);
			}

			set
			{
				// BUGBUG validate the address types first
				addressTypes = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty( Common.addressTypesProperty, addressTypes);
					parentContact.SetDirty();
				}
				changed = true;
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
					if (thisNode != null)
					{
						street = thisNode.Properties.GetSingleProperty( Common.streetProperty ).ToString();
					}

					return(street);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				street = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty( Common.streetProperty, street);
					parentContact.SetDirty();
				}
				changed = true;
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
					if (thisNode != null)
					{
						locality = thisNode.Properties.GetSingleProperty( Common.localityProperty ).ToString();
					}

					return(locality);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				locality = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty( Common.localityProperty, locality);
					parentContact.SetDirty();
				}
				changed = true;
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
					if (thisNode != null)
					{
						region = thisNode.Properties.GetSingleProperty( Common.regionProperty ).ToString();
					}

					return(region);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				region = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty( Common.regionProperty, region);
					parentContact.SetDirty();
				}

				changed = true;
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
					if (thisNode != null)
					{
						country = thisNode.Properties.GetSingleProperty( Common.countryProperty ).ToString();
					}

					return(country);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				country = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty( Common.countryProperty, country);
					parentContact.SetDirty();
				}

				changed = true;
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
					if (thisNode != null)
					{
						poBox = thisNode.Properties.GetSingleProperty( Common.poBoxProperty ).ToString();
					}

					return(poBox);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				poBox = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty( Common.poBoxProperty, poBox);
					parentContact.SetDirty();
				}
				changed = true;
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
					if (thisNode != null)
					{
						extAddress = thisNode.Properties.GetSingleProperty( Common.extendedProperty ).ToString();
					}

					return(extAddress);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				extAddress = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty( Common.extendedProperty, extAddress);
					parentContact.SetDirty();
				}
				changed = true;
			}
		}

		/// <summary>
		/// PostalCode - postal or zip code
		/// Mandatory property in the Address class.
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
				if (thisNode != null)
				{
					zip = thisNode.Name;
				}

				return(zip);
			}

			set
			{
				zip = value;
				if (thisNode != null)
				{
					thisNode.Name = zip;
					parentContact.SetDirty();
				}
				changed = true;
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
					if (thisNode != null)
					{
						mailStop = thisNode.Properties.GetSingleProperty( Common.mailStopProperty ).ToString();
					}

					return(mailStop);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				mailStop = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty( Common.mailStopProperty, mailStop);
					parentContact.SetDirty();
				}
				changed = true;
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
				if (thisNode != null)
				{
					preferred = 
						Convert.ToBoolean(thisNode.Properties.GetSingleProperty( Common.preferredProperty ).ToString());
				}

				return(preferred);
			}

			set
			{
				preferred = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty( Common.preferredProperty, preferred);
					parentContact.SetDirty();
				}
				changed = true;
			}
		}

		/// <summary>
		/// ID - ID of the ADR structured property
		///
		/// Type Value: A single text value
		/// </summary>
		public string ID
		{
			get
			{
				try
				{
					return((string) thisNode.Id);
				}
				catch
				{
					return("");
				}
			}
		}
		#endregion

		#region Constructors

		/// <summary>
		/// Simple Address constructor
		/// </summary>
		public Address()
		{
		}

		/// <summary>
		/// An Address constructor which includes the zip code
		/// Zip code is a mandatory property for ADR
		/// </summary>
		public Address(string zip)
		{
			this.zip = zip;
		}

		internal Address(Collection collection, Node parentNode, Contact parentContact)
		{
            this.parentNode = parentNode;
			this.parentNodeID = parentNode.Id;
			this.parentCollection = collection;
			this.parentContact = parentContact;
		}

		internal Address(Collection collection, Node parentNode, Contact parentContact, string addressID)
		{
			this.parentNode = parentNode;
			this.parentNodeID = parentNode.Id;
			this.parentCollection = collection;
			this.parentContact = parentContact;

			this.thisNode = collection.GetNodeById(addressID);
		}
		#endregion

		#region Internal Methods

		/// <summary>
		/// Creates a structured address property which is contained by a contact record.
		/// If the method fails an exception is raised
		/// </summary>
		/// <param name="collection">parent collection - AddressBook</param>
		/// <param name="parentNode"></param>
		/// <param name="parentContact"></param>
		/// <remarks>
		/// The postalCode is the only field required to create an address record.
		/// </remarks>
		/// <returns>nothing if successful - raises an application exception if an error occurs.</returns>
		public void Create(Collection collection, Node parentNode, Contact parentContact)
		{
			// BUGBUG validate the mandatory properties - zip code

			Node cNode = parentNode.CreateChild(this.zip, Common.addressProperty);
			cNode.Properties.AddProperty(Common.preferredProperty, this.preferred);
			cNode.Properties.AddProperty(Common.addressTypesProperty, this.addressTypes);

			if (this.street != null && this.street != "")
			{
				cNode.Properties.AddProperty( Common.streetProperty, this.street);
			}

			if (this.locality != null && this.locality != "")
			{
				cNode.Properties.AddProperty( Common.localityProperty, this.locality);
			}

			if (this.region != null && this.region != "")
			{
				cNode.Properties.AddProperty( Common.regionProperty, this.region);
			}

			if (this.poBox != null && this.poBox != "")
			{
				cNode.Properties.AddProperty( Common.poBoxProperty, this.poBox);
			}

			if (this.extAddress != null && this.extAddress != "")
			{
				cNode.Properties.AddProperty( Common.extendedProperty, this.extAddress);
			}

			if (this.mailStop != null && this.mailStop != "")
			{
				cNode.Properties.AddProperty( Common.mailStopProperty, this.mailStop);
			}

			if (this.country != null && this.Country != "")
			{
				cNode.Properties.AddProperty( Common.countryProperty, this.country);
			}

			parentContact.SetDirty();

			this.parentCollection = collection;
			this.parentNode = parentNode;
			this.parentContact = parentContact;
			this.thisNode = cNode;
		}

		/// <summary>
		/// Retrieves an address object from the store and sets all
		/// value properties in the object.
		/// </summary>
		internal void ToObject(Collection collection, Node parentNode, Contact parentContact, string addressID)
		{
			this.parentCollection = collection;
			this.parentNode = parentNode;
			this.parentContact = parentContact;

			try
			{
				this.thisNode = parentCollection.GetNodeById(addressID);

				//this.zip = this.thisNode.Properties.GetSingleProperty( Common.zipProperty ).ToString();
				this.zip = this.thisNode.Name;
				this.preferred = 
					Convert.ToBoolean(this.thisNode.Properties.GetSingleProperty( Common.preferredProperty ).ToString());
				this.addressTypes = (AddressTypes)
					this.thisNode.Properties.GetSingleProperty( Common.addressTypesProperty ).Value;

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
			catch
			{
				throw new ApplicationException(Common.addressBookExceptionHeader + "Address node not found");
			}
		}

		/// <summary>
		/// Copies all properties from the object to the store.
		/// </summary>
		internal void ToStore(Collection collection, Node parentNode, Contact contact, string addressID)
		{
			this.parentCollection = collection;
			this.parentNode = parentNode;

			try
			{
				if (this.changed == true)
				{
					if (this.thisNode != null)
					{
						this.thisNode = parentCollection.GetNodeById(addressID);
					}

					// zip code and preferred are mandatory properties
					thisNode.Name = this.zip;
					thisNode.Properties.ModifyProperty(Common.preferredProperty, this.preferred);

					if (this.street != null)
					{
						thisNode.Properties.ModifyProperty( Common.streetProperty, this.street);
					}

					if (this.locality != null)
					{
						thisNode.Properties.ModifyProperty( Common.localityProperty, this.locality);
					}

					if (this.region != null)
					{
						thisNode.Properties.ModifyProperty( Common.regionProperty, this.region);
					}

					if (this.poBox != null)
					{
						thisNode.Properties.ModifyProperty( Common.poBoxProperty, this.poBox);
					}

					if (this.extAddress != null)
					{
						thisNode.Properties.ModifyProperty( Common.extendedProperty, this.extAddress);
					}

					if (this.mailStop != null)
					{
						thisNode.Properties.ModifyProperty( Common.mailStopProperty, this.mailStop);
					}

					if (this.country != null)
					{
						thisNode.Properties.ModifyProperty( Common.countryProperty, this.country);
					}

					contact.SetDirty();
					this.changed = false;
				}
			}
			catch
			{
				throw new ApplicationException(Common.addressBookExceptionHeader + "Address node not found");
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Commits any changed properties to the address object.
		/// </summary>
		/// <returns>nothing</returns>
		public void Commit()
		{
			if (changed == true)
			{
				try
				{
					this.ToStore(this.parentCollection, this.parentNode, this.parentContact, this.ID);
					this.parentContact.SetDirty();
					changed = false;
				}
				catch{}
				//this.thisNode.Save();
			}
		}

		/// <summary>
		/// Deletes the current address
		/// </summary>
		public void Delete()
		{
			this.thisNode.Delete(true);
		}

		#endregion
	}
}
