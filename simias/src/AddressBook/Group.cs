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

/***********************************************************************
 * This source file implements the Group class.
 * Groups are implemented as Node objects contained within an 
 * AddressBook (collection).  Relationship objects are used to link
 * Contacts to a Group
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Collections;
using Simias;
using Simias.Storage;

namespace Novell.AddressBook
{
	/// <summary>
	/// Summary description for Group.
	/// A Group is equivalent to a Node in the collection store.
	/// </summary>
	public class Group : Node
	{
		#region Class Members
		internal	AddressBook		addressBook = null;
//		private		IEnumerator		thisEnum = null;
		internal	ArrayList		addressList;
		private		Stream			logoStream = null;

		#endregion

		#region Properties

		/// <summary>
		/// Description of the Group
		/// !NOTE! Doc incomplete
		/// </summary>
		public string Description
		{
			get
			{
				try
				{
					return(
						this.Properties.GetSingleProperty(
							Common.groupDescriptionProperty).ToString());
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
						this.Properties.ModifyProperty(
							Common.groupDescriptionProperty, 
							(string) value);
					}
					else
					{
						this.Properties.DeleteProperties(
							Common.groupDescriptionProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Url: Specifies the a url to the Group's web page
		///
		/// Type Value: Single text value
		///
		/// Example: http://www.eatatjoes.com
		///
		/// </summary>
		public string Url
		{
			get
			{
				try
				{
					return(
						this.Properties.GetSingleProperty(
							Common.urlProperty).ToString());
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
						this.Properties.ModifyProperty(
							Common.urlProperty, 
							(string) value);
					}
					else
					{
						this.Properties.DeleteProperties(
							Common.urlProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// NOTE: latest notes about the Group
		///
		/// Type Value: single value - text
		///
		/// Example: blah, blah, blah...
		///
		/// </summary>
		public string Note
		{
			get
			{
				try
				{
					return(
						this.Properties.GetSingleProperty(
							Common.noteProperty).ToString());
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
						this.Properties.ModifyProperty(
							Common.noteProperty, 
							(string) value);
					}
					else
					{
						this.Properties.DeleteProperties(
							Common.noteProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Blog: Specifies the Group's blog url
		///
		/// Type Value: Single text value
		///
		/// Example: http://simiasteam.blogdomain.com
		///
		/// </summary>
		public string Blog
		{
			get
			{
				try
				{
					return(
						this.Properties.GetSingleProperty(
							Common.blogProperty).ToString());
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
						this.Properties.ModifyProperty(
							Common.blogProperty, 
							(string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.blogProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Calendar: Specifies the Group's calendar url
		///
		/// Type Value: Single text value
		///
		/// Example: http://simiasteam.company.com/groupname/ical
		///
		/// </summary>
		public string Calendar
		{
			get
			{
				try
				{
					return(
						this.Properties.GetSingleProperty(
							Common.calProperty).ToString());
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
						this.Properties.ModifyProperty(
							Common.calProperty, 
							(string) value);
					}
					else
					{
						this.Properties.DeleteProperties(Common.calProperty);
					}
				}
				catch{}
			}
		}
		#endregion

		#region Constructors

		internal Group(AddressBook addressBook, Node cNode) : base(cNode)
		{
			this.addressList = new ArrayList();
			this.ToObject();
		}

		/// <summary>
		/// Simple Group constructor
		/// </summary>
		public Group() : base("")
		{
			this.addressList = new ArrayList();
		}

		/// <summary>
		/// Group constructor that names the group upon construction
		/// </summary>
		public Group(string groupName) : base(groupName)
		{
			this.addressList = new ArrayList();
		}

		#endregion

		#region Private Methods

		internal void Add(AddressBook addressBook)
		{
			try
			{
				this.addressBook = addressBook;
				if (this.Name != null && this.Name != "")
				{
					// Add a relationship that will reference the parent Node.
					Relationship parentChild = new
						Relationship( addressBook.ID, this.ID );
					this.Properties.AddProperty( Common.groupToAddressBook, parentChild );

					if (this.addressList.Count > 0)
					{
						foreach(Address cAddress in this.addressList)
						{
							Relationship addressAndContact = new
								Relationship(addressBook.ID, this.ID );
							cAddress.Properties.AddProperty( Common.addressToGroup, addressAndContact );
						}
					}
					return;
				}
			}
			catch{}
			throw new ApplicationException(
						Common.addressBookExceptionHeader + "Group - missing \"Name\" property");
		}

		/// <summary>
		/// Retrieves a group from the collection store and deserializes
		/// to the properties in the Group object.
		/// <remarks>
		/// An exception is thrown if the contact can't be find by the
		/// specified ID
		/// </remarks>
		/// <returns>None</returns>
		/// </summary>
		internal void ToObject()
		{
			try
			{
				// Load up the addresses
				this.addressList.Clear();
				Relationship parentChild =
					new Relationship( this.addressBook.ID, this.ID );
				ICSList results =
					this.addressBook.Search( Common.addressToGroup, parentChild );
				foreach ( ShallowNode cShallow in results )
				{
					Node cNode = new Node(this.addressBook, cShallow);
					if (this.addressBook.IsType(cNode, Common.addressProperty) == true)
					{
						this.addressList.Add(new Group(this.addressBook, cNode));
					}
				}
			}
			catch{}
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Commits any changes to the Group object.
		/// </summary>
		public void Commit()
		{
			if (this.addressBook != null)
			{
				this.addressBook.SetType(this, Common.groupType);

				int nodesToCommit = 1 + this.addressList.Count;

				Node[] commitList = new Node[nodesToCommit];
				int	i = 0;
				commitList[i++] = this;

				if (this.addressList.Count > 0)
				{
					Novell.AddressBook.Address.PrepareToCommit(this);
					foreach(Address cAddr in this.addressList)
					{
						this.addressBook.SetType(cAddr, Common.addressProperty);
						commitList[i++] = cAddr;
					}
				}

				this.addressBook.Commit(commitList);
			}
		}

		/// <summary>
		/// Deletes the current Group object.
		/// </summary>
		public Node[] Delete(bool commit)
		{
			Node[] nodeList = new Node[1 + this.addressList.Count];
			int	idx = 0;

			foreach(Address cAddress in this.addressList)
			{
				nodeList[idx++] = cAddress;
			}
			this.addressList.Clear();

			try
			{
				if (this.addressBook != null &&
					this.addressBook != null)
				{
					nodeList[idx++] = this;

					if (commit == true)
					{
						this.addressBook.Commit(
							this.addressBook.Delete(nodeList));
					}
					else
					{
						this.addressBook.Delete(nodeList);
					}
				}
			}
			catch{}

			// Return the list of nodes that were deleted
			return(nodeList);
		}

		/// <summary>
		/// Deletes the current Group object.
		/// </summary>
		public void Delete()
		{
			this.Delete(true);
		}

		/// <summary>
		/// Adds a contact to the group
		/// </summary>
		/// <remarks>
		/// If for any reason the contact cannot be added
		/// an exception is raised.
		/// </remarks>
		public void AddContact(Contact cContact)
		{
			try
			{
				// Add a relationship that will reference the contact Node.
				Relationship parentChild = new
					Relationship( this.ID, cContact.ID );
				this.Properties.AddProperty( Common.groupToContact, parentChild );
				return;
			}
			catch{}
			throw new ApplicationException(Common.addressBookExceptionHeader + "Failed adding Contact");
		}

		/// <summary>
		/// Gets the list of contacts attached to this Group.
		/// </summary>
		public IABList GetContactList()
		{
			IABList cList = new IABList();

			try
			{
				MultiValuedList	mList = this.Properties.GetProperties(Common.groupToContact);
				foreach(Property p in mList)
				{
					if (p != null)
					{
						Simias.Storage.Relationship relationship = 
							(Simias.Storage.Relationship) p.Value;

						Node cContactNode = 
							this.addressBook.GetNodeByID(relationship.NodeID);
						if (this.addressBook.IsType(cContactNode, Common.contactType) == true)
						{
							cList.Add(this.addressBook.GetContact(cContactNode.ID));
						}

					}
				}

				/*
				Relationship parentChild =
					new Relationship( this.ID, this.ID );
				ICSList results =
					this.addressBook.collection.Search( Common.groupToContact, parentChild );
				foreach ( ShallowNode cShallow in results )
				{
					Node cNode = new Node(this.addressBook.collection, cShallow);
					if (this.addressBook.collection.IsType(cNode, Common.contactType) == true)
					{
						cList.Add(new Contact(this.addressBook, cNode));
					}
				}
				*/
			}
			catch{}
			return(cList);
		}

		/// <summary>
		/// Remove contact from group
		/// </summary>
		/// <remarks>
		/// </remarks>
		public void RemoveContact(Contact cContact)
		{
			try
			{
				MultiValuedList	mList = this.Properties.GetProperties(Common.groupToContact);
				foreach(Property p in mList)
				{
					if (((Relationship) p.Value).NodeID == cContact.ID)
					{
						p.Delete();
						break;
					}
				}
			}
			catch{}
			return;
		}

		/// <summary>
		/// Adds a vCard ADR property to the Group.
		/// </summary>
		/// <remarks>
		/// vCard ADR is a structured property consisting of:
		/// Street, Region, Locality, Country and Extended Address
		///
		/// Each Group may have one preferred ADR.
		///
		/// If for any reason the ADR cannot be created an exception is raised.
		/// </remarks>
		public void AddAddress(Address addr)
		{
			try
			{
				addr.Add(this);
				return;
			}
			catch{}
			throw new ApplicationException(Common.addressBookExceptionHeader + "Failed adding Address");
		}

		/// <summary>
		/// Gets a list of addresses attached to this Group.
		/// </summary>
		public IABList GetAddresses()
		{
			IABList cList = new IABList();

			try
			{
				foreach(Address addr in this.addressList)
				{
					cList.Add(addr);
				}
			}
			catch{}
			return(cList);
		}

		/// <summary>
		/// Gets the preferred address for this Group.
		/// </summary>
		public Address GetPreferredAddress()
		{
			foreach(Address addr in this.addressList)
			{
				if (addr.Preferred == true)
				{
					return(addr);
				}
			}

			return(null);
		}

		/// <summary>
		/// Deletes an Address record in a Group.
		/// A Group may contain 0 to many address records
		/// </summary>
		public void DeleteAddress(string addressID)
		{
			try
			{
				foreach(Address addr in this.addressList)
				{
					if (addr.ID == addressID)
					{
						addr.Delete();
						return;
					}
				}
			}
			catch{}

			// FIXME - log this condition
			return;
		}

		/// <summary>
		/// Retrieve a vCard ADR property based on an address ID.
		/// </summary>
		/// <param name="addressID">Address ID</param>
		/// <remarks>
		/// vCard Address is a structured property consisting of:
		/// Post Office Box, Extended Address, Street Address, Locality,
		/// Region, Postal Code and Country
		///
		/// Each contact may have one preferred Address.
		///
		/// If for any reason the Address can't be retrieved an an exception is raised.
		/// </remarks>
		/// <returns>An Address object with a valid postal code property.</returns>
		public Address GetAddress(string addressID)
		{
			try
			{
				foreach(Address cAddress in this.addressList)
				{
					if (cAddress.ID == addressID)
					{
						return(cAddress);
					}
				}
			}
			catch{}

			// FIXME - log this condition
			return(null);
		}

		/// <summary>
		/// Export the Group logo via a binary Stream object.
		/// </summary>
		/// <remarks>
		/// vCard LOGO is a single valued binary property
		///
		/// An exception is raised if the Group does not contain
		/// a logo property.
		///
		/// NOTE: The caller is expected to close the returned
		/// stream object when finished.
		/// </remarks>
		/// <returns>A binary stream object which the caller can read from.</returns>
		public Stream ExportLogo()
		{
			if (this.addressBook != null)
			{
				try
				{
					Property p = this.Properties.GetSingleProperty(Common.groupToLogo);
					if (p != null)
					{
						Simias.Storage.Relationship relationship =
							(Simias.Storage.Relationship) p.Value;

						Node cLogoNode =
							this.addressBook.GetNodeByID(relationship.NodeID);

						if (cLogoNode != null)
						{
							StoreFileNode sfn = new StoreFileNode(cLogoNode);
							
							return(new
								FileStream(
									sfn.GetFullPath(this.addressBook),
									FileMode.Open,
									FileAccess.Read,
									FileShare.Read ));
						}
					}
				}
				catch{}
				throw new ApplicationException(Common.addressBookExceptionHeader + "Logo property does not exist");
			}
			else
			{
				if(this.logoStream != null)
				{
					return(this.logoStream);
				}

				throw new ApplicationException(Common.addressBookExceptionHeader + "Logo property does not exist");
			}
		}

		/// <summary>
		/// Imports a logo photo from a a file name.
		/// </summary>
		/// <param name="fileName">Source Filename</param>
		/// <remarks>
		/// vCard PHOTO is a single valued binary property
		/// </remarks>
		/// <returns>true if the photo was successfully imported.</returns>
		public bool ImportLogo(string fileName)
		{
			bool	results = false;
			Stream	srcStream = null;

			try
			{
				srcStream = new FileStream(fileName, FileMode.Open);
				results = this.ImportLogo(srcStream);
				// BUGBUG store the source file name in the store - where it came from
			}
			catch{}
			finally
			{
				if (srcStream != null)
				{
					srcStream.Close();
				}
			}

			return(results);
		}

		/// <summary>
		/// Imports a logo photo from a stream object.
		/// </summary>
		/// <param name="srcStream">Source Stream</param>
		/// <remarks>
		/// vCard LOGO is a single valued binary property
		/// </remarks>
		/// <returns>true if the logo photo was successfully imported.</returns>
		public bool	ImportLogo(Stream srcStream)
		{
			bool			finished = false;
			StoreFileNode	sfn = null;
			//NodeStream		photoStream = null;
			//Stream			dstStream = null;

			if (this.addressBook != null)
			{
				try
				{
					// See if a photo stream already exists for this contact node.
					// If one is found - delete it
					Property p =
						this.Properties.GetSingleProperty(Common.groupToLogo);
					if (p != null)
					{
						Simias.Storage.Relationship relationship =
							(Simias.Storage.Relationship) p.Value;

						Node cLogoNode = 
							this.addressBook.GetNodeByID(relationship.NodeID);
						if (cLogoNode != null)
						{
							this.addressBook.Delete(cLogoNode);
							this.addressBook.Commit(cLogoNode);
						}
					}
				}
				catch{}

				// Create the new node
				try
				{
					sfn =
						new StoreFileNode(
								Common.logoProperty, 
								srcStream);

					Relationship parentChild = new
						Relationship(
							this.addressBook.ID,
							sfn.ID);

					this.Properties.ModifyProperty(Common.groupToLogo, parentChild);
					this.addressBook.Commit(sfn);
					this.addressBook.Commit(this);
					finished = true;
				}
				catch{}
			}
			else
			{
				BinaryReader	bReader = null;
				BinaryWriter	bWriter = null;

				// Copy the logo photo into the cached stream
				try
				{
					// Create the new stream in the file system
					this.logoStream = new MemoryStream();
					bWriter = new BinaryWriter(this.logoStream);

					// Copy the source stream
					bReader = new BinaryReader(srcStream);
					bReader.BaseStream.Position = 0;
					bWriter.BaseStream.Position = 0;

					//bWriter.Write(bReader.BaseStream, 0, bReader.BaseStream.Length);
					
					// BUGBUG better algo for copying
					int i = 0;
					while(true)
					{
						i = bReader.BaseStream.ReadByte();
						if(i == -1)
						{
							break;
						}

						bWriter.BaseStream.WriteByte((byte) i);
					}

					bWriter.BaseStream.Position = 0;
					finished = true;
				}
				catch{}
				finally
				{
					if (bReader != null)
					{
						bReader.Close();
					}
				}
			}

			return(finished);
		}

		#endregion
	}
}
