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
	public class Name : Node
	{
		#region Class Members
		/// <summary>
		/// Well known address book types.
		/// </summary>
		internal const string	otherProperty = "VC:Other";
		internal const string	prefixProperty = "VC:Prefix";
		internal const string	suffixProperty = "VC:Suffix";
		private Contact			parentContact;
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
				try
				{
					return(this.Properties.GetSingleProperty( Common.familyProperty ).ToString());
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
						this.Properties.ModifyProperty( Common.familyProperty, value );
					}
					else
					{
						this.Properties.DeleteProperties( Common.familyProperty );
					}
				}
				catch{}
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
				try
				{
					return(this.Properties.GetSingleProperty( otherProperty ).ToString());
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
						this.Properties.ModifyProperty( otherProperty, value );
					}
					else
					{
						this.Properties.DeleteProperties( otherProperty );
					}
				}
				catch{}
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
				try
				{
					return(this.Properties.GetSingleProperty( prefixProperty ).ToString());
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
						this.Properties.ModifyProperty( prefixProperty, value );
					}
					else
					{
						this.Properties.DeleteProperties( prefixProperty );
					}
				}
				catch{}
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
				try
				{
					return(this.Properties.GetSingleProperty( suffixProperty ).ToString());
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
						this.Properties.ModifyProperty( suffixProperty, value );
					}
					else
					{
						this.Properties.DeleteProperties( suffixProperty );
					}
				}
				catch{}
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
		/// Simple Name constructor
		/// </summary>
		public Name() : base ("")
		{
			this.Properties.AddProperty( Common.preferredProperty, true );
		}

		/// <summary>
		/// Name constructor which includes the given and family properties
		/// </summary>
		public Name(string given, string family) : base (given)
		{
			this.Properties.AddProperty( Common.familyProperty, family );
			this.Properties.AddProperty( Common.preferredProperty, true );
		}

		internal Name(Contact parentContact, Node cNode) : base (cNode)
		{
			this.parentContact = parentContact;
		}
		#endregion

		#region Internal Methods

		internal string GetFullName()
		{
			string fn = "";

			if((Prefix != "") && (Prefix.Length > 0))
			{
				fn += Prefix + " ";
			}

			if((Given != "") && (Given.Length > 0))
			{
				fn += Given + " ";
			}

			if((Other != "") && (Other.Length > 0))
			{
				fn += Other + " ";
			}

			if((Family != "") && (Family.Length > 0))
			{
				fn += Family;
			}

			if((Suffix != "") && (Suffix.Length > 0))
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

			//
			// Add a relationship for Name node and Contact node.
			//

			// Can't create and add the relationship object if the contact
			// hasn't been attached to the address book
			if (parentContact.addressBook != null)
			{
				Relationship parentChild = new 
					Relationship( 
						parentContact.addressBook.ID, 
						parentContact.ID );

				this.Properties.AddProperty( Common.nameToContact, parentChild );
			}

			this.parentContact.nameList.Add(this);
		}

		internal static bool PrepareToCommit(Contact contact)
		{
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

			return(true);
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Delete this Name from the name list attached to the contact record
		/// </summary>
		public void Delete()
		{
			try
			{
				if (this.parentContact != null)
				{
					this.parentContact.nameList.Remove(this);
					if (this.parentContact.addressBook != null)
					{
						this.parentContact.addressBook.Commit(
							this.parentContact.addressBook.Delete(this));
					}
				}
			}
			catch{}
			return;
		}

		#endregion
	}
}
