/***********************************************************************
 *  Name.cs - Class for defining a vCard 3.0 Name property.
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
		bool					changed = false;
		//internal const string	nameProperty = "vCard:Name";
		internal const string	otherProperty = "vCard:Other";
		internal const string	prefixProperty = "vCard:Prefix";
		internal const string	suffixProperty = "vCard:Suffix";
		internal const string	preferredProperty = "vCard:Preferred";
		private Collection		parentCollection;
		private Node			parentNode;
		private Contact			parentContact;
		private Node			thisNode;
		private	bool			preferred;
		private string			given;
		private string			family;
		private string			other;
		private string			prefix;
		private string			suffix;
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
					return(family);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				family = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty(Common.familyProperty, family);
					parentContact.SetDirty();
				}

				changed = true;
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
					return(given);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				given = value;
				if (thisNode != null)
				{
					thisNode.Name = given;
					parentContact.SetDirty();
				}
				changed = true;
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
					return(other);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				other = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty(otherProperty, other);
					parentContact.SetDirty();
				}

				changed = true;
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
					return(prefix);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				prefix = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty(prefixProperty, prefix);
					parentContact.SetDirty();
				}
				changed = true;
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
					return(suffix);
				}
				catch
				{
					return("");
				}
			}

			set
			{
				suffix = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty(suffixProperty, suffix);
					parentContact.SetDirty();
				}

				changed = true;
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
				return(preferred);
			}

			set
			{
				preferred = value;
				if (thisNode != null)
				{
					thisNode.Properties.ModifyProperty(preferredProperty, preferred);
					parentContact.SetDirty();
				}
				changed = true;
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

		internal Name(Collection collection, Node parentNode)
		{
			this.parentNode = parentNode;
			this.parentCollection = collection;
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

		internal void Create(Collection collection, Node parentNode, Contact contact)
		{
			this.parentCollection = collection;
			this.parentNode = parentNode;
			this.parentContact = contact;

			// Make sure the mandatory properties exist
			this.thisNode = parentNode.CreateChild(this.given, Common.nameProperty);
			this.thisNode.Properties.AddProperty(Common.familyProperty, this.family);

			if (this.other != null && this.other != "")
			{
				thisNode.Properties.AddProperty(otherProperty, this.other);
			}

			if (this.prefix != null && this.prefix != "")
			{
				thisNode.Properties.AddProperty(prefixProperty, this.prefix);
			}

			if (this.suffix != null && this.suffix != "")
			{
				thisNode.Properties.AddProperty(suffixProperty, this.suffix);
			}

			thisNode.Properties.AddProperty(preferredProperty, this.preferred);
			contact.SetDirty();
		}

		internal void ToObject(Collection collection, Node parentNode, Contact contact, string objectID)
		{
			this.parentCollection = collection;
			this.parentNode = parentNode;
			this.parentContact = contact;

			try
			{
				this.thisNode = this.parentCollection.GetNodeById(objectID);

				// Read in all the properties
				this.given = this.thisNode.Name;
				this.preferred = Convert.ToBoolean(thisNode.Properties.GetSingleProperty( preferredProperty ).ToString());
				this.family = thisNode.Properties.GetSingleProperty( Common.familyProperty ).ToString();

				// Non-mandatory properties may not exist
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

		/*
		public void Commit()
		{
			if (this.thisNode != null && this.parentCollection != null)
			{
				if (changed == true)
				{
					this.parentNode.Name = this.given;
					thisNode.Properties.ModifyProperty(Common.familyProperty, this.family);
					thisNode.Properties.ModifyProperty(otherProperty, this.other);
					thisNode.Properties.ModifyProperty(prefixProperty, this.prefix);
					thisNode.Properties.ModifyProperty(suffixProperty, this.suffix);
					thisNode.Properties.ModifyProperty(preferredProperty, this.preferred);

					this.thisNode.Save();
					this.parentCollection.Commit();
				
					changed = false;
					return;
				}
			}

			throw new ApplicationException("AddressBook::Name object has not been added to a contact");
		}
		*/


		/// <summary>
		/// Delete this Name from the name list attached to the contact record
		/// </summary>
		public void Delete()
		{
			if (this.thisNode != null)
			{
				this.thisNode.Delete(true);
			}
		}

		#endregion
	}
}
