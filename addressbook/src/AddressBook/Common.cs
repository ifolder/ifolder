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
using System.Collections;
using System.IO;

namespace Novell.AddressBook
{
	/// <summary>
	/// Common class is used for setting up common types across the
	/// AddressBook project.
	/// </summary>
	public class Common
	{
		#region Class Members

		internal static string addressBookType = "AB:AddressBook";
		internal static string addressBookProperty = "AB:AddressBook";
		internal static string addressBookTypeProperty = "AB:AddressBookType";
		internal static string addressBookRightsProperty = "AB:AddressBookRights";

		/// <summary>
		/// email property which is contained in a contact
		/// </summary>
		internal static string	emailProperty = "VC:EMAIL";

		/// <summary>
		/// title property which is contained in a contact
		/// </summary>
		internal static string	titleProperty = "VC:TITLE";

		/// <summary>
		/// full name property which is contained in a contact
		/// </summary>
		internal static string	fnProperty = "VC:FN";

		/// <summary>
		/// role property which is contained in a contact
		/// </summary>
		internal static string	roleProperty = "VC:ROLE";

		/// <summary>
		/// nick name property which is contained in a contact
		/// </summary>
		internal static string	nicknameProperty = "VC:NICKNAME";

		/// <summary>
		/// birthday property which is contained in a contact
		/// </summary>
		internal static string	birthdayProperty = "VC:BDAY";

		/// <summary>
		/// url property which is contained in a contact
		/// </summary>
		internal static string	urlProperty = "VC:URL";

		/// <summary>
		/// organization property which is contained in a contact
		/// </summary>
		internal static string	organizationProperty = "VC:ORG";

		/// <summary>
		/// vCard Note property which is contained in a contact
		/// </summary>
		internal const string noteProperty = "VC:NOTE";

		/// <summary>
		/// vCard Telephone property which is contained in a contact
		/// </summary>
		internal const string phoneProperty = "VC:TEL";

		/// <summary>
		/// vCard schema version currently supported by the address book
		/// </summary>
		internal static string	vcardSchemaVersion = "3.0";

		/// <summary>
		/// blogUrl property which is contained in a contact
		/// </summary>
		internal static string	blogProperty = "AB:BLOG";

		/// <summary>
		/// web cam url property which is contained in a contact
		/// </summary>
		internal static string	webcamProperty = "AB:WEBCAM";

		/// <summary>
		/// calendar url property which is contained in a contact
		/// </summary>
		internal static string	calProperty = "AB:CAL";

		/// <summary>
		/// instant message property which is contained in a contact
		/// </summary>
		internal static string	imProperty = "AB:IM";

		/// <summary>
		/// Collection Store a contact is known by
		/// </summary>
		internal const string contactType = "AB:Contact";

		/// <summary>
		/// Node type a group is known by in the Collection Store
		/// </summary>
		internal const string groupType = "AB:Group";

		/// <summary>
		/// vCard N (Name) property which is contained in a contact
		/// </summary>
		internal const string nameProperty = "VC:N";

		/// <summary>
		/// vCard given property which is contained in a name
		/// 
		/// </summary>
		internal const string givenProperty = "VC:Given";

		/// <summary>
		/// vCard family property which is contained in a name
		/// 
		/// </summary>
		internal const string familyProperty = "VC:Family";

		/// <summary>
		/// vCard ADR (Address) property which is contained in a contact
		/// </summary>
		internal const string addressProperty = "VC:ADR";

		/// <summary>
		/// vCard PHOTO property which is contained in a contact
		/// </summary>
		internal const string photoProperty = "VC:PHOTO";

		/// <summary>
		/// vCard SOUND property which is contained in a contact
		/// </summary>
		internal const string soundProperty = "VC:SOUND";

		/// <summary>
		/// vCard LOGO property which is contained in a contact
		/// </summary>
		internal const string logoProperty = "VC:LOGO";

		/// <summary>
		/// workForceID propoerty contained in a contact
		/// NOTE! This property is not defined in the vCard schema
		/// </summary>
		internal const string workForceIDProperty = "AB:WorkForceID";

		/// <summary>
		/// mailStop propoerty contained in a contact
		/// NOTE! This property is not defined in the vCard schema
		/// </summary>
		internal const string mailStopProperty = "AB:MailStop";

		/// <summary>
		/// pobox contained in an ADR property
		/// </summary>
		internal const string	poBoxProperty = "VC:Pobox";

		/// <summary>
		/// Extended address contained in an ADR property
		/// </summary>
		internal const string	extendedProperty = "VC:Extadd";

		/// <summary>
		/// Street address contained in a ADR property
		/// NOTE! This property is not defined in the vCard schema
		/// </summary>
		internal const string	streetProperty = "VC:Street";

		/// <summary>
		/// Locality contained in an ADR property
		/// </summary>
		internal const string	localityProperty = "VC:Locality";

		/// <summary>
		/// Region contained in an ADR property
		/// </summary>
		internal const string	regionProperty = "VC:Region";

		/// <summary>
		/// Postal code contained in an ADR property
		/// </summary>
		internal const string	zipProperty = "VC:Pcode";

		/// <summary>
		/// Country contained in an ADR property
		/// </summary>
		internal const string	countryProperty = "VC:Country";

		/// <summary>
		/// Common preferred property contained in a contact
		/// </summary>
		internal const string	preferredProperty = "VC:Preferred";

		/// <summary>
		/// managerID propoerty contained in a contact
		/// manager's work force ID for the contact
		/// </summary>
		internal const string managerIDProperty = "AB:ManagerID";
		
		/// <summary>
		/// vCard PRODID property which is contained in a contact
		/// </summary>
		internal const string productIDProperty = "VC:PRODID";

		/// <summary>
		/// Address Book Identity property contained in a contact
		/// !NOTE! will be obsolete
		/// </summary>
		internal const string identityProperty = "AB:Identity";

		/// <summary>
		/// Collection name for a contact list
		/// !NOTE! obsolete
		/// </summary>
		internal const string contactListType = "AB:ContactList";

		/// <summary>
		/// AddressTypes (ex. work, home, other, dom, intl etc.)
		/// </summary>
		internal const string addressTypesProperty = "VC:AddressTypes";

		/// <summary>
		/// AddressTypes (ex. work, home, other, dom, intl etc.)
		/// </summary>
		internal const string groupDescriptionProperty = "AB:GroupDescription";

		/// <summary>
		/// Address Book property referecing a member's UserID
		/// </summary>
		internal const string userIDProperty = "AB:UserID";
		
		/// <summary>
		/// A vCard LABEL property may contain the following types
		/// </summary>
		internal static string[] labelTypes = new string[] {"home", "work", "dom", "intl", "postal", "parcel", "other"};

		/// <summary>
		/// VCard propreties the address book will currently import
		/// </summary>
		internal static string[] vCardProperties = new string[] {"ADR", "BDAY", "EMAIL", "LOGO", "N", "NICKNAME", "NOTE", "ORG", "PHOTO", "ROLE", "SOUND", "TEL", "TITLE", "URL", "X-NAB-BLOG", "X-NAB-USERNAME"};

		/// <summary>
		/// Exception header for all application exceptions thrown by the address book
		/// </summary>
		internal const string abExceptionHeader = "Novell.AddressBook.Exception - ";
		internal const string addressBookExceptionHeader = "Novell.AddressBook.Exception - ";

		// Relationship property names
		internal const string contactToAddressBook = "AB:ContactToAddressBook";
		internal const string nameToContact = "AB:NameToContact";
		internal const string addressToContact = "AB:AddressToContact";
		internal const string photoToContact = "AB:PhotoToContact";
		internal const string contactToPhoto = "AB:ContactToPhoto";
		internal const string groupToAddressBook = "AB:GroupToAddressBook";
		internal const string groupToLogo = "AB:GroupToLogo";
		internal const string addressToGroup = "AB:AddressToGroup";
		internal const string contactToGroup = "AB:ContactToGroup";
		internal const string groupToContact = "AB:GroupToContact";

		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods
		#endregion
	}
}
