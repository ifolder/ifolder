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
		private Node			thisNode;
		private string			locality;
		private string			region;
		private string			country;
		private string			zip;
		private string			street;
		private string			poBox;
		private string			extAddress;
		private string			mailStop;
//		private bool			preferred = false;
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
						this.Properties.ModifyProperty( Common.addressTypesProperty, value );
					}
					else
					{
						this.Properties.ModifyProperty( 
							Common.addressTypesProperty, 
							AddressTypes.work | AddressTypes.parcel | AddressTypes.postal);
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
					return(
						Convert.ToBoolean(
							this.Properties.GetSingleProperty( 
								Common.preferredProperty ).ToString()));
				}
				catch{}
				return(false);
			}

			set
			{
				try
				{
					this.Properties.ModifyProperty( Common.preferredProperty, value );
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
		}

		/// <summary>
		/// An Address constructor which includes the zip code
		/// Zip code is a mandatory property for ADR
		/// </summary>
		public Address(string zip) : base(zip)
		{
		}

		internal Address(Contact parentContact) : base("")
		{
			this.parentContact = parentContact;
		}

		internal Address(Contact parentContact, Node cNode) : base(cNode)
		{
			this.parentContact = parentContact;
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
			// Add a relationship for Name node and Contact node.
			//

			// Can't create and add the relationship object if the contact
			// hasn't been attached to the address book
			if (parentContact.addressBook != null &&
				parentContact.addressBook.collection != null)
			{
				Relationship parentChild = new 
					Relationship( 
					parentContact.addressBook.collection.ID, 
					parentContact.ID );

				this.Properties.AddProperty( "AddressToContact", parentChild );
			}

			this.parentContact.addressList.Add(this);
		}

		/// <summary>
		/// Called by the static method PersistToStore
		/// </summary>
		/// <returns>nothing</returns>
		/*
		private void Commit()
		{
			try
			{
				if (this.parentContact != null)
				{
					// New object?
					if (this.ID == "")
					{
						Node cNode = null;
						//Node cNode = parentContact.thisNode.CreateChild(this.zip, Common.addressProperty);
						//cNode.Properties.AddProperty(Common.preferredProperty, this.preferred);
						//cNode.Properties.AddProperty(Common.addressTypesProperty, this.addressTypes);
						//this.thisNode = cNode;
						//this.id = cNode.Id;
					}

					if (this.street != null)
					{
						if (this.street != "")
						{
							this.thisNode.Properties.ModifyProperty( Common.streetProperty, this.street);
						}
						else
						{
							this.thisNode.Properties.DeleteProperties( Common.streetProperty );
						}
					}

					if (this.locality != null)
					{
						if (this.locality != "")
						{
							this.thisNode.Properties.ModifyProperty( Common.localityProperty, this.locality);
						}
						else
						{
							this.thisNode.Properties.DeleteProperties( Common.localityProperty );
						}
					}

					if (this.region != null)
					{
						if (this.region != "")
						{
							this.thisNode.Properties.ModifyProperty( Common.regionProperty, this.region );
						}
						else
						{
							this.thisNode.Properties.DeleteProperties( Common.regionProperty );
						}
					}

					if (this.poBox != null)
					{
						if (this.poBox != "")
						{
							this.thisNode.Properties.ModifyProperty( Common.poBoxProperty, this.poBox );
						}
						else
						{
							this.thisNode.Properties.DeleteProperties( Common.poBoxProperty );
						}
					}

					if (this.extAddress != null)
					{
						if (this.extAddress != "")
						{
							this.thisNode.Properties.ModifyProperty( Common.extendedProperty, this.extAddress );
						}
						else
						{
							this.thisNode.Properties.DeleteProperties( Common.extendedProperty );
						}
					}

					if (this.mailStop != null)
					{
						if (this.mailStop != "")
						{
							this.thisNode.Properties.ModifyProperty( Common.mailStopProperty, this.mailStop );
						}
						else
						{
							this.thisNode.Properties.DeleteProperties( Common.mailStopProperty );
						}
					}

					if (this.country != null)
					{
						if (this.country != "")
						{
							this.thisNode.Properties.ModifyProperty( Common.countryProperty, this.country );
						}
						else
						{
							this.thisNode.Properties.DeleteProperties( Common.countryProperty );
						}
					}

					if (this.addressTypes == 0)
					{
						this.addressTypes = AddressTypes.work | AddressTypes.parcel | AddressTypes.postal;
					}

					this.thisNode.Properties.ModifyProperty( Common.addressTypesProperty, this.addressTypes );
				}
			}
			catch{}
			// FIXME - log commit errors
		}
		*/

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
					if ((tmpAddress.addressTypes & AddressTypes.work) == AddressTypes.work)
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

			/*
			// To the collection store they go!
			foreach(Address tmpAddress in contact.addressList)
			{
				tmpAddress.Commit();
			}
			*/

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

				if (this.thisNode != null)
				{
					this.Delete();
				}
			}
		}

		#endregion
	}
}
