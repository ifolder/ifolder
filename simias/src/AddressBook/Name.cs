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
using Simias.Storage;
using Novell.AddressBook;

namespace Novell.AddressBook
{
	/// <summary>
	/// Summary description for a Name.
    /// Name is a structured attribute in the vCard schema.
    /// Names are contained by Contacts
    /// A name consists of:
    ///   Family Name = Family
    ///   Given Name = Given
    ///   Additional Names = Other
    ///   Honorific Prefixes = Prefix
    ///   Honorific Suffixes = Suffix
	/// </summary>
	public class Name
	{
		#region Class Members
		/// <summary>
		/// Well known address book types.
		/// </summary>
		//internal const string	nameProperty = "vCard:Name";
		internal const string	otherProperty = "VC:Other";
		internal const string	prefixProperty = "VC:Prefix";
		internal const string	suffixProperty = "VC:Suffix";
		internal const string	preferredProperty = "VC:Preferred";
		private Contact			parentContact;
		private Node			thisNode;
		private	bool			preferred;
		private string			given;
		private string			family;
		private string			other;
		private string			prefix;
		private string			suffix;
		private string			id;
		#endregion

		#region Properties

		/// <summary>
		/// Family - family name
		///
		/// Type Value: A single text value
		///
		/// Example: given Brady Anderson - Anderson is the "Family" portion of the name
		///
		/// </summary>

		public string Family
		{
			get
			{	
				if (this.family != null)
				{
					return(this.family);
				}

				return("");
			}

			set
			{
				this.family = value;
				if (this.parentContact != null)
				{
					this.parentContact.SetDirty(ChangeMap.name);
				}
			}
		}

		/// <summary>
		/// Given - given name
		///
		/// Type Value: A single text value
		///
		/// Example: given Brady Anderson - Brady is the "Given" portion of the name 
		///
		/// </summary>
		public string Given
		{
			get
			{
				if (this.given != null)
				{
					return(this.given);
				}

				return("");
			}

			set
			{
				this.given = value;
				if (this.parentContact != null)
				{
					this.parentContact.SetDirty(ChangeMap.name);
				}
			}
		}

		/// <summary>
		/// Other - other name(s)
		///
		/// Type Value: A single text value
		///
		/// Example: given Brady Adam Anderson - Adam is considered "Other" in the name
		///
		/// </summary>
		public string Other
		{
			get
			{
				if (this.other != null)
				{
					return(this.other);
				}

				return("");
			}

			set
			{
				this.other = value;
				if (this.parentContact != null)
				{
					this.parentContact.SetDirty(ChangeMap.name);
				}
			}
		}

		/// <summary>
		/// Prefix - Honorific Prefixes
		///
		/// Type Value: multi-value - text
		///
		/// Example: given Mr. Brady Anderson - Mr. is considered the prefix
		///
		/// </summary>
		public string Prefix
		{
			get
			{
				if (this.prefix != null)
				{
					return(this.prefix);
				}

				return("");
			}

			set
			{
				this.prefix = value;
				if (this.parentContact != null)
				{
					this.parentContact.SetDirty(ChangeMap.name);
				}
			}
		}

		/// <summary>
		/// Suffix - Honorific Suffixes
		///
		/// Type Value: multi-value - text
		///
		/// Example: given Mr. Brady Anderson III - "III" is considered the suffix
		///
		/// </summary>
		public string Suffix
		{
			get
			{
				if (this.suffix != null)
				{
					return(this.suffix);
				}

				return("");
			}

			set
			{
				this.suffix = value;
				if (this.parentContact != null)
				{
					this.parentContact.SetDirty(ChangeMap.name);
				}
			}
		}

		/// <summary>
		/// FN - Specifies the formatted text corresponding to the name of
		/// the object this contact represents.  This type is based on the 
		/// semantics of the X.520 Common Name attribute.
		///
		/// Type Value: A read-only single text value
		///
		/// Example: FN:Mr. John Q. Public\, Esq.
		/// Note: The FN property is built from the preferred Name attribute 
		///       which is a complex attribute.
		///
		/// </summary>
		public string FN
		{
			get
			{
				return(this.GetFullName());
			}
		}
		
		/// <summary>
		/// Preferred - The contact's preferred name
		///
		/// Type Value: single value - boolean
		/// </summary>
		public bool Preferred
		{
			get
			{
				return(this.preferred);
			}

			set
			{
				this.preferred = value;
				if (this.parentContact != null)
				{
					this.parentContact.SetDirty(ChangeMap.name);
				}
			}
		}

		/// <summary>
		/// ID - Unique ID of the N structured property
		///
		/// Type Value: A single text value
		/// </summary>
		public string ID
		{
			get
			{
				if (this.id != null)
				{
					return(this.id);
				}

				return("");
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Simple Name constructor
		/// </summary>
		public Name()
		{
			this.preferred = false;
		}

		/// <summary>
		/// Name constructor which includes the given and family properties
		/// </summary>
		public Name(string given, string family)
		{
			this.preferred = false;
			this.given = given;
			this.family = family;
		}

		internal Name(Contact parentContact, string nameID)
		{
			this.ToObject(parentContact, nameID);
		}

		#endregion

		#region Internal Methods

		internal string GetFullName()
		{
			string fn = "";

			if((Prefix != null) && (Prefix.Length > 0))
			{
				fn += Prefix + " ";
			}

			if((Given != null) && (Given.Length > 0))
			{
				fn += Given + " ";
			}

			if((Other != null) && (Other.Length > 0))
			{
				fn += Other + " ";
			}

			if((Family != null) && (Family.Length > 0))
			{
				fn += Family;
			}

			if((Suffix != null) && (Suffix.Length > 0))
			{
				fn += " " + Suffix;
			}

			return(fn);
		}

		internal void Add(Contact parentContact)
		{
			this.parentContact = parentContact;

			// If we're preferred make sure nobody else is
			if (this.Preferred == true)
			{
				foreach(Name cName in parentContact.nameList)
				{
					if (cName.Preferred == true)
					{
						cName.Preferred = false;
					}
				}
			}

			this.parentContact.nameList.Add(this);
			this.parentContact.SetDirty(ChangeMap.name);
		}

		/// <summary>
		/// Called by the static method PersistToStore
		/// </summary>
		/// <returns>nothing</returns>
		private void Commit()
		{
			try
			{
				if (this.parentContact != null)
				{
					// Any valid members?
					if (this.given == null &&
						this.family == null &&
						this.other == null &&
						this.prefix == null &&
						this.suffix == null)
					{
						return;
					}

					// New object?
					if (this.ID == "")
					{
						Node cNode = parentContact.thisNode.CreateChild(this.given, Common.nameProperty);
						this.thisNode = cNode;
						this.id = cNode.Id;
					}

					try
					{
						if (this.family != null)
						{
							this.thisNode.Properties.ModifyProperty( Common.familyProperty, this.family );
						}
						else
						{
							this.thisNode.Properties.DeleteProperties( Common.familyProperty );
						}
					}
					catch{}

					try
					{
						if (this.other != null)
						{
							this.thisNode.Properties.ModifyProperty( otherProperty, this.other );
						}
						else
						{
							this.thisNode.Properties.DeleteProperties( otherProperty );
						}
					}
					catch{}

					try
					{
						if (this.prefix != null)
						{
							this.thisNode.Properties.ModifyProperty( prefixProperty, this.prefix );
						}
						else
						{
							this.thisNode.Properties.DeleteProperties( prefixProperty );
						}
					}
					catch{}

					try
					{
						if (this.suffix != null)
						{
							this.thisNode.Properties.ModifyProperty( suffixProperty, this.suffix );
						}
						else
						{
							this.thisNode.Properties.DeleteProperties( suffixProperty );
						}
					}
					catch{}

					try
					{
						if (this.Preferred == true)
						{
							this.thisNode.Properties.ModifyProperty( Common.preferredProperty, this.Preferred );
						}
						else
						{
							this.thisNode.Properties.DeleteProperties( Common.preferredProperty );
						}
					}
					catch{}
		
				}
			}
			catch{}
			// FIXME - log commit errors
		}

		internal static bool PersistToStore(Contact contact)
		{
			// The contact needs to be attached to the store in order to persist
			if (contact.thisNode == null)
			{
				return(false);
			}

			// Anything in the list to persist?
			if (contact.nameList.Count == 0)
			{
				return(false);
			}

			// assume no preferred is set
			bool foundPreferred = false;

			// Make sure we have a preferred
			foreach(Name cName in contact.nameList)
			{
				if (cName.Preferred == true)
				{
					foundPreferred = true;
					break;
				}
			}

			if (foundPreferred == false)
			{
				// Mark the first Name in the list preferred
				foreach(Name cName in contact.nameList)
				{
					cName.Preferred = true;
					break;
				}
			}

			// To the collection store they go!
			foreach(Name cName in contact.nameList)
			{
				cName.Commit();
			}

			return(true);
		}

		internal void ToObject(Contact contact, string objectID)
		{
			this.parentContact = contact;

			try
			{
				this.thisNode = this.parentContact.addressBook.collection.GetNodeById(objectID);
				this.id = this.thisNode.Id;
				this.given = this.thisNode.Name;

				try
				{
					this.preferred = 
						Convert.ToBoolean(thisNode.Properties.GetSingleProperty( preferredProperty ).ToString());
				}
				catch{}

				try
				{
					this.family = thisNode.Properties.GetSingleProperty( Common.familyProperty ).ToString();
				}
				catch{}

				try
				{
					this.other = thisNode.Properties.GetSingleProperty( otherProperty ).ToString();
				}
				catch{}

				try
				{
					this.prefix = thisNode.Properties.GetSingleProperty( prefixProperty ).ToString();
				}
				catch{}

				try
				{
					this.suffix = thisNode.Properties.GetSingleProperty( suffixProperty ).ToString();
				}
				catch{}
			}
			catch
			{
				throw new ApplicationException("AddressBook::Name node not found");
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Delete this Name from the name list attached to the contact record
		/// </summary>
		public void Delete()
		{
			if (this.parentContact != null)
			{
				this.parentContact.nameList.Remove(this);

				if (this.thisNode != null)
				{
					this.thisNode.Delete(true);
				}
			}
		}

		#endregion
	}
}
