/***********************************************************************
 *  Property.cs - Class that represents an attribute which is used to
 *  describe the object represented by a Node.
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
 *  Author: Mike Lasky <mlasky@novell.com>
 * 
 ***********************************************************************/

using System;
using System.Collections;
using System.Xml;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
	/// <summary>
	/// Represents a property name/value pair for a node.  Properties have
	/// well-defined syntax types.
	/// </summary>
	public class Property
	{
		#region Class Members

		#region Constant and Static Members
		/// <summary>
		/// Initial size for the system property table.
		/// </summary>
		private const int initialSysPropTableSize = 20;

		/// <summary>
		/// Defines the property syntaxes.
		/// </summary>
		public enum Syntax 
		{
			/// <summary>
			/// Represents text; that is, a series of Unicode characters. A string is a sequential 
			/// collection of Unicode characters, typically used to represent text, while a String 
			/// is a sequential collection of System.Char objects that represents a string. The 
			/// value of the String is the content of the sequential collection, and the value is 
			/// immutable.
			/// </summary>
			String, 

			/// <summary>
			/// Represents an 8-bit signed integer value. The SByte value type represents 
			/// integers with values ranging from negative 128 to positive 127.
			/// </summary>
			SByte, 
			
			/// <summary>
			/// Represents an 8-bit unsigned integer value. The Byte value type represents 
			/// unsigned integers with values ranging from 0 to 255.
			/// </summary>
			Byte, 

			/// <summary>
			/// Represents a 16-bit signed integer value. The Int16 value type represents signed 
			/// integers with values ranging from negative 32768 through positive 32767.
			/// </summary>
			Int16, 

			/// <summary>
			/// Represents a 16-bit unsigned integer value. The UInt16 value type represents 
			/// unsigned integers with values ranging from 0 to 65535.
			/// </summary>
			UInt16, 

			/// <summary>
			/// Represents a 32-bit signed integer value. The Int32 value type represents signed 
			/// integers with values ranging from negative 2,147,483,648 through positive 2,147,483,647.
			/// </summary>
			Int32, 

			/// <summary>
			/// Represents a 32-bit unsigned integer value. The UInt32 value type represents unsigned 
			/// integers with values ranging from 0 to 4,294,967,295.
			/// </summary>
			UInt32, 

			/// <summary>
			/// Represents a 64-bit signed integer value. The Int64 value type represents integers
			/// with values ranging from negative 9,223,372,036,854,775,808 through 
			/// positive 9,223,372,036,854,775,807.
			/// </summary>
			Int64, 

			/// <summary>
			/// Represents a 64-bit unsigned integer value. The UInt64 value type represents unsigned 
			/// integers with values ranging from 0 to 18,446,744,073,709,551,615.
			/// </summary>
			UInt64, 

			/// <summary>
			/// Represents a Unicode character. The Char value type represents a Unicode character,
			/// also called a Unicode code point, and is implemented as a 16-bit number ranging in
			/// value from hexadecimal 0x0000 to 0xFFFF.
			/// </summary>
			Char, 

			/// <summary>
			/// Represents a single-precision floating point number. The Single value type represents a 
			/// single-precision 32-bit number with values ranging from negative 3.402823e38 to 
			/// positive 3.402823e38, as well as positive or negative zero, PositiveInfinity, 
			/// NegativeInfinity, and not a number (NaN).
			/// </summary>
			Single, 

			/// <summary>
			/// Represents a Boolean value. Instances of this type have values of either true or false.
			/// </summary>
			Boolean, 

			/// <summary>
			/// Represents an instant in time, typically expressed as a date and time of day. The DateTime 
			/// value type represents dates and times with values ranging from 12:00:00 midnight, 
			/// January 1, 0001 Anno Domini (Common Era) to 11:59:59 P.M., December 31, 9999 A.D. (C.E.)
			/// </summary>
			DateTime,

			/// <summary>
			/// Provides an object representation of a uniform resource identifier (URI) and easy access 
			/// to the parts of the URI. A URI is a compact representation of a resource available to 
			/// your application on the Internet.
			/// </summary>
			Uri,

			/// <summary>
			/// Represents an XML document. This class implements the W3C Document Object Model (DOM) 
			/// Level 1 Core and the Core DOM Level 2. The DOM is an in-memory (cache) tree representation 
			/// of an XML document and enables the navigation and editing of this document.
			/// </summary>
			XmlDocument,

			/// <summary>
			/// Represents a time interval.  The value of an instance of TimeSpan represents a period of time. 
			/// That value is the number of ticks contained in the instance and can range from Int64.MinValue 
			/// to Int64.MaxValue. A tick is the smallest unit of time that can be specified, and is equal to 
			/// 100 nanoseconds. Both the specification of a number of ticks and the value of a TimeSpan can 
			/// be positive or negative.
			/// </summary>
			TimeSpan
		};

		/// <summary>
		/// Supported store search operators.
		/// </summary>
		public enum Operator
		{
			/// <summary>
			/// Used to compare if two values are equal.
			/// </summary>
			Equal         = Persist.Query.Operator.Equal,

			/// <summary>
			/// Used to compare if two values are not equal.
			/// </summary>
			Not_Equal     = Persist.Query.Operator.Not_Equal,

			/// <summary>
			/// Used to compare if a string value begins with a sub-string value.
			/// </summary>
			Begins        = Persist.Query.Operator.Begins,

			/// <summary>
			/// Used to compare if a string value ends with a sub-string value.
			/// </summary>
			Ends          = Persist.Query.Operator.Ends,

			/// <summary>
			/// Used to compare if a string value contains a sub-string value.
			/// </summary>
			Contains      = Persist.Query.Operator.Contains,

			/// <summary>
			/// Used to compare if a value is greater than another value.
			/// </summary>
			Greater       = Persist.Query.Operator.Greater,

			/// <summary>
			/// Used to compare if a value is less than another value.
			/// </summary>
			Less          = Persist.Query.Operator.Less,

			/// <summary>
			/// Used to compare if a value is greater than or equal to another value.
			/// </summary>
			Greater_Equal = Persist.Query.Operator.Greater_Equal,

			/// <summary>
			/// Used to compare if a value is less than or equal to another value.
			/// </summary>
			Less_Equal    = Persist.Query.Operator.Less_Equal
		};

		/// <summary>
		/// Types of flags that are defined on the property.
		/// </summary>
		internal const uint Hidden = 0x00010000;
		internal const uint Local  = 0x00020000;

		/// <summary>
		///  Mask used to get just the system flags.
		/// </summary>
		private const uint SystemFlagMask = 0xFFFF0000;

		#region Well known Property names
		//
		// These names are not exported outside of the assembly.
		//
	
		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string IDAttr = Persist.Provider.IdAttr;

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string NameAttr = Persist.Provider.NameAttr;

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string PropertyTag = Persist.Provider.PropertyTag;

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string ObjectListTag = Persist.Provider.ObjectListTag;

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string ObjectTag = Persist.Provider.ObjectTag;

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string TypeAttr = Persist.Provider.TypeAttr;

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string FlagsAttr = Persist.Provider.FlagsAttr;

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string Ace = "Ace";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string NodeFileSystemEntry = "NodeFileSystemEntry";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string FileSystemEntryTag = "FsEntry";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string EntryType = "EntryType";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string FileType = "File";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string DirectoryType = "Directory";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string MasterIncarnation = "MasterIncarnation";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string LocalIncarnation = "LocalIncarnation";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string Shareable = "Shareable";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string Syncable = "Syncable";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string LocalAddressBook = "AB:Local";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string AddressBookType = "AB:AddressBook";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string Alias = "AB:Alias";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string Credential = "AB:Credential";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string AliasParameters = "AliasParameters";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string TrueStr = "1";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		internal const string FalseStr = "0";


		//
		// These names are made public to the client application.
		//

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string CollectionID = Persist.Provider.CollectionId;

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string CreationTime = "CreationTime";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string ModifyTime = "ModifyTime";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string ParentID = "ParentID";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string ObjectId = Persist.Provider.ObjectId;

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string ObjectName = Persist.Provider.ObjectName;

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string ObjectType = Persist.Provider.ObjectType;

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string Owner = "Owner";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string DocumentRoot = "DocumentRoot";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string DocumentPath = "DocumentPath";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string IDPath = "IdPath";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string FileCreationTime = "FileCreationTime";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string FileLastAccessTime = "FileLastAccessTime";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string FileLastWriteTime = "FileLastWriteTime";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string FileLength = "FileLength";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string DomainName = "DomainName";

		/// <summary>
		/// Well known XML attribute.
		/// </summary>
		public const string IdentityType = "AB:Contact";
		#endregion

		/// <summary>
		/// Hashtable providing quick lookup to well-known system properties.
		/// </summary>
		static private Hashtable systemPropertyTable;
		#endregion

		/// <summary>
		/// This XmlElement contains all the information about the property.
		/// </summary>
		private XmlElement xmlProperty;

		/// <summary>
		/// The property list where this property is stored.
		/// </summary>
		private PropertyList propertyList;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the name of this property.
		/// </summary>
		public string Name
		{
			get { return xmlProperty.GetAttribute( Property.NameAttr ); }
			set 
			{ 
				// Make sure the current user has write access to the collection.
				CheckAccessControl( false );
				SetPropertyAttribute( Property.NameAttr, value ); 
			}
		}

		/// <summary>
		/// Gets and sets the syntax type of this property.
		/// </summary>
		public Syntax Type
		{
			get { return ( Syntax )Enum.Parse( typeof( Syntax ), xmlProperty.GetAttribute( Property.TypeAttr ) ); }
			set 
			{ 
				// Make sure the current user has write access to the collection.
				CheckAccessControl( false );
				SetPropertyAttribute( Property.TypeAttr, value.ToString() ); 
			}
		}

		/// <summary>
		/// Gets and sets the flags for this property.
		/// </summary>
		public ushort Flags
		{
			get { return ( ushort )PropertyFlags; }
			set { PropertyFlags = ( PropertyFlags & SystemFlagMask ) | ( uint )value; }
		}

		/// <summary>
		/// Gets the string representation of the property syntax type.
		/// </summary>
		public string TypeString
		{
			get { return Type.ToString(); }
		}

		/// <summary>
		/// Gets and sets the value of this property as an object.
		/// </summary>
		public object Value
		{
			get { return GetValue(); }
			set { SetValue( value ); }
		}

		/// <summary>
		/// Gets the owning document for this xml node.
		/// </summary>
		internal XmlDocument OwnerDocument
		{
			get { return xmlProperty.OwnerDocument; }
		}

		/// <summary>
		/// Allows the corresponding XML element to be set into the object when this property 
		/// has been added to the property list.
		/// </summary>
		internal XmlElement XmlProperty
		{
			get { return xmlProperty; }
			set { xmlProperty = value; }
		}

		/// <summary>
		/// Allows the property list to be set into the object when this property has been 
		/// added to the property list.
		/// </summary>
		internal PropertyList XmlPropertyList
		{
			get { return propertyList; }
			set { propertyList = value; }
		}

		/// <summary>
		/// Gets whether this property is associated with a node or if it is a new property object.
		/// </summary>
		public bool IsAssociatedProperty
		{
			get { return ( propertyList == null ) ? false : true; }
		}

		/// <summary>
		/// Gets and sets the hidden status of a property.
		/// </summary>
		internal bool HiddenProperty
		{
			get { return ( PropertyFlags & Hidden ) == Hidden ? true : false; }

			set 
			{
				if ( value )
				{
					PropertyFlags |= Hidden;
				}
				else
				{
					PropertyFlags &= ~Hidden;
				}
			}
		}

		/// <summary>
		/// Gets and sets the transient status of a property.  If this flag is false, this property will synchronize
		/// to other stores.  If it is true, it is a property local to this store only.  This flag is set to false by
		/// default.
		/// </summary>
		public bool LocalProperty
		{
			get { return ( PropertyFlags & Local ) == Local ? true : false; }

			set 
			{
				if ( value )
				{
					PropertyFlags |= Local;
				}
				else
				{
					PropertyFlags &= ~Local;
				}
			}
		}

		/// <summary>
		/// Gets and sets all of the flags for the property object.
		/// </summary>
		internal uint PropertyFlags
		{
			get 
			{
				// Check to see if this property contains any flags.
				uint propertyFlags = 0;
				if ( xmlProperty.HasAttribute( FlagsAttr ) )
				{
					propertyFlags = Convert.ToUInt32( xmlProperty.GetAttribute( FlagsAttr ) );
				}

				return propertyFlags;
			}

			set 
			{
				// Set the new flags in the xml document.
				SetPropertyAttribute( Property.FlagsAttr, value.ToString() );
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Static constructor for the object.
		/// </summary>
		static Property()
		{
			// Allocate the tables to hold the reserved property names.
			systemPropertyTable = new Hashtable( initialSysPropTableSize, new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer() );

			// Add the well-known system properties to the hashtable.  Don't need to add values
			// with them.  Just need to know if they exist.
			systemPropertyTable.Add( IDAttr, null );
			systemPropertyTable.Add( NameAttr, null );
			systemPropertyTable.Add( PropertyTag, null );
			systemPropertyTable.Add( ObjectListTag, null );
			systemPropertyTable.Add( ObjectTag, null );
			systemPropertyTable.Add( TypeAttr, null );
			systemPropertyTable.Add( FlagsAttr, null );
			systemPropertyTable.Add( Ace, null );
			systemPropertyTable.Add( FileSystemEntryTag, null );
			systemPropertyTable.Add( EntryType, null );
			systemPropertyTable.Add( FileType, null );
			systemPropertyTable.Add( DirectoryType, null );
			systemPropertyTable.Add( MasterIncarnation, null );
			systemPropertyTable.Add( LocalIncarnation, null );
			systemPropertyTable.Add( Shareable, null );
			systemPropertyTable.Add( Syncable, null );
			systemPropertyTable.Add( NodeFileSystemEntry, null );
			systemPropertyTable.Add( LocalAddressBook, null );
			systemPropertyTable.Add( Alias, null );
			systemPropertyTable.Add( Credential, null );
			systemPropertyTable.Add( AliasParameters, null );

			systemPropertyTable.Add( CollectionID, null );
			systemPropertyTable.Add( CreationTime, null );
			systemPropertyTable.Add( ModifyTime, null );
			systemPropertyTable.Add( ParentID, null );
			systemPropertyTable.Add( ObjectId, null );
			systemPropertyTable.Add( ObjectName, null );
			systemPropertyTable.Add( ObjectType, null );
			systemPropertyTable.Add( Owner, null );
			systemPropertyTable.Add( DocumentRoot, null );
			systemPropertyTable.Add( DocumentPath, null );
			systemPropertyTable.Add( IDPath, null );
			systemPropertyTable.Add( FileCreationTime, null );
			systemPropertyTable.Add( FileLastAccessTime, null );
			systemPropertyTable.Add( FileLastWriteTime, null );
			systemPropertyTable.Add( FileLength, null );
			systemPropertyTable.Add( DomainName, null );
			systemPropertyTable.Add( IdentityType, null );
			systemPropertyTable.Add( AddressBookType, null );
		}

		/// <summary>
		/// Constructor used by the PropertyList.IEnumerable interface to create property objects.
		/// </summary>
		/// <param name="propertyList">The property list where this property is stored.</param>
		/// <param name="xmlProperty">An XML element object that contains the name and value for this property.</param>
		internal Property( PropertyList propertyList, XmlElement xmlProperty )
		{
			this.propertyList = propertyList;
			this.xmlProperty = xmlProperty;
		}

		/// <summary>
		/// Constructs a property object.
		/// </summary>
		/// <param name="name">Name of the property to construct.</param>
		/// <param name="syntax">Syntax type of the value.</param>
		/// <param name="propertyValue">Value of the property.</param>
		private Property( string name, Syntax syntax, string propertyValue )
		{
			propertyList = null;

			XmlDocument propDoc = new XmlDocument();
			xmlProperty = propDoc.CreateElement( Property.PropertyTag );

			// Must guarantee order on the attributes.
			XmlAttribute xmlAttr = propDoc.CreateAttribute( Property.NameAttr );
			xmlAttr.Value = name;
			xmlProperty.Attributes.Prepend( xmlAttr );

			xmlAttr = propDoc.CreateAttribute( Property.TypeAttr );
			xmlAttr.Value = syntax.ToString();
			xmlProperty.Attributes.Append( xmlAttr );

			if ( syntax == Syntax.XmlDocument )
			{
				xmlProperty.InnerXml = propertyValue;
			}
			else
			{
				xmlProperty.InnerText = propertyValue;
			}
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="property">Property.</param>
		public Property( Property property )
		{
			propertyList = null;

			XmlDocument propDoc = new XmlDocument();
			xmlProperty = ( XmlElement )propDoc.ImportNode( property.xmlProperty, true );
			propDoc.AppendChild( xmlProperty );
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Object representation of the value.</param>
		public Property( string name, object propertyValue )
		{
			propertyList = null;
			Syntax syntax = GetSyntaxType( propertyValue );

			XmlDocument propDoc = new XmlDocument();
			xmlProperty = propDoc.CreateElement( Property.PropertyTag );

			// Must guarantee order on the attributes.
			XmlAttribute xmlAttr = propDoc.CreateAttribute( Property.NameAttr );
			xmlAttr.Value = name;
			xmlProperty.Attributes.Prepend( xmlAttr );

			xmlAttr = propDoc.CreateAttribute( Property.TypeAttr );
			xmlAttr.Value = syntax.ToString();
			xmlProperty.Attributes.Append( xmlAttr );

			if ( syntax == Syntax.XmlDocument )
			{
				xmlProperty.InnerXml = GetValueFromPropertyObject( syntax, propertyValue );
			}
			else
			{
				xmlProperty.InnerText = GetValueFromPropertyObject( syntax, propertyValue );
			}
		}

		/// <summary>
		/// Constructs a property
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">String value of the property.</param>
		public Property( string name, string propertyValue ) :
			this( name, Syntax.String, propertyValue )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Signed 8-bit value of the property.</param>
		public Property( string name, sbyte propertyValue ) :
			this ( name, Syntax.SByte, propertyValue.ToString() )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Unsigned 8-bit value of the property.</param>
		public Property( string name, byte propertyValue ) :
			this( name, Syntax.Byte, propertyValue.ToString() )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Signed 16-bit value of the property.</param>
		public Property( string name, short propertyValue ) :
			this( name, Syntax.Int16, propertyValue.ToString() )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Unsigned 16-bit value of the property.</param>
		public Property( string name, ushort propertyValue ) :
			this( name, Syntax.UInt16, propertyValue.ToString() )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Signed 32-bit value of the property.</param>
		public Property( string name, int propertyValue ) :
			this( name, Syntax.Int32, propertyValue.ToString() )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Unsigned 32-bit value of the property.</param>
		public Property( string name, uint propertyValue ) :
			this( name, Syntax.UInt32, propertyValue.ToString() )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Signed 64-bit value of the property.</param>
		public Property( string name, long propertyValue ) :
			this( name, Syntax.Int64, propertyValue.ToString() )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Unsigned 64-bit value of the property.</param>
		public Property( string name, ulong propertyValue ) :
			this( name, Syntax.UInt64, propertyValue.ToString() )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Unicode character value of the property.</param>
		public Property( string name, char propertyValue ) :
			this( name, Syntax.Char, (( ushort )propertyValue).ToString() )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Single precision 32-bit value of the property.</param>
		public Property( string name, float propertyValue ) :
			this( name, Syntax.Single, propertyValue.ToString() )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Boolean value of the property.</param>
		public Property( string name, bool propertyValue ) :
			this( name, Syntax.Boolean, propertyValue ? "1" : "0" )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">DateTime object of the property.</param>
		public Property( string name, DateTime propertyValue ) :
			this( name, Syntax.DateTime, propertyValue.Ticks.ToString() )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">Uri object of the property.</param>
		public Property( string name, Uri propertyValue ) :
			this( name, Syntax.Uri, propertyValue.ToString() )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">XmlDocument object of the property.</param>
		public Property( string name, XmlDocument propertyValue ) :
			this( name, Syntax.XmlDocument, propertyValue.InnerXml )
		{
		}

		/// <summary>
		/// Constructs a property.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="propertyValue">TimeSpan object of the property.</param>
		public Property( string name, TimeSpan propertyValue ) :
			this( name, Syntax.TimeSpan, propertyValue.Ticks.ToString() )
		{
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Checks to make sure the current user has write access before allowing the property to be changed.
		/// Also checks to make sure that system properties are not being set.
		/// </summary>
		/// <param name="validateProperty">If true the property is checked to see if it is a system property.</param>
		private void CheckAccessControl( bool validateProperty )
		{
			if ( validateProperty && IsSystemProperty() )
			{
				throw new ApplicationException( "Cannot delete a system property" );
			}

			// Make sure the current user has write access to the collection.  If there is no collection object
			// associated with this property, then allow the write.
			if ( IsAssociatedProperty )
			{
				if ( !propertyList.CollectionNode.IsAccessAllowed( Access.Rights.ReadWrite ) )
				{
					throw new UnauthorizedAccessException( "Current user does not have collection modify right." );
				}
			}
		}

		/// <summary>
		/// Returns the syntax type from a C# object.
		/// </summary>
		/// <param name="propertyValue">Value type</param>
		/// <returns>The syntax type that represents the property value.</returns>
		private Syntax GetSyntaxType( object propertyValue )
		{
			Syntax syntax;

			// Get the type of object from propertyValue.
			// There is no top level type that matches this object.  Check for an enumerated type.
			if ( propertyValue.GetType().BaseType.Name == "Enum" )
			{
				syntax = ( Syntax )Enum.Parse( typeof( Syntax ),  Enum.GetUnderlyingType( propertyValue.GetType() ).Name );
			}
			else
			{
				syntax = ( Syntax )Enum.Parse( typeof( Syntax ), propertyValue.GetType().Name );
			}

			return syntax;
		}

		/// <summary>
		/// Returns an object containing the value of this property.
		/// </summary>
		/// <returns>An object that contains the value of the property.</returns>
		private object GetValue()
		{
			object propertyValue;

			// Based on the syntax type, create an object and stuff in the property value.
			switch ( Type )
			{
				case Syntax.String:
					propertyValue = xmlProperty.InnerText;
					break;

				case Syntax.SByte:
					propertyValue = SByte.Parse( xmlProperty.InnerText );
					break;

				case Syntax.Byte:
					propertyValue = Byte.Parse( xmlProperty.InnerText );
					break;

				case Syntax.Int16:
					propertyValue =  Int16.Parse( xmlProperty.InnerText );
					break;

				case Syntax.UInt16:
					propertyValue =  UInt16.Parse( xmlProperty.InnerText );
					break;

				case Syntax.Int32:
					propertyValue =  Int32.Parse( xmlProperty.InnerText );
					break;

				case Syntax.UInt32:
					propertyValue =  UInt32.Parse( xmlProperty.InnerText );
					break;

				case Syntax.Int64:
					propertyValue =  Int64.Parse( xmlProperty.InnerText );
					break;

				case Syntax.UInt64:
					propertyValue =  UInt64.Parse( xmlProperty.InnerText );
					break;

				case Syntax.Char:
					propertyValue =  ( Char )UInt16.Parse( xmlProperty.InnerText );
					break;
					
				case Syntax.Single:
					propertyValue =  Single.Parse( xmlProperty.InnerText );
					break;

				case Syntax.Boolean:
					propertyValue =  Boolean.Parse( ( xmlProperty.InnerText == "0" ) ? Boolean.FalseString : Boolean.TrueString );
					break;

				case Syntax.DateTime:
					long timeValue = Int64.Parse( xmlProperty.InnerText );
					propertyValue = new DateTime( timeValue );
					break;

				case Syntax.Uri:
					propertyValue = new Uri( xmlProperty.InnerText );
					break;

				case Syntax.XmlDocument:
					propertyValue =  new XmlDocument();
					( ( XmlDocument )propertyValue ).LoadXml( xmlProperty.InnerXml );
					break;

				case Syntax.TimeSpan:
					long tickValue = Int64.Parse( xmlProperty.InnerText );
					propertyValue = new TimeSpan( tickValue );
					break;

				default:
					throw new ArgumentException( "Invalid syntax type specified" );
			}

			return propertyValue;
		}

		/// <summary>
		/// Returns a string value representing the property value.
		/// </summary>
		/// <param name="syntax">Syntax type of propertyValue parameter.</param>
		/// <param name="propertyValue">Object that containst the propertyValue and syntax type.</param>
		/// <returns>A string object representing the property value.</returns>
		private string GetValueFromPropertyObject( Syntax syntax, object propertyValue )
		{
			string valueString;

			// Convert the propertyValue object type to a supported property syntax type.
			switch ( syntax )
			{
				case Syntax.String:
					valueString = ( String )propertyValue;
					break;

				case Syntax.SByte:
					valueString = ( ( SByte )propertyValue ).ToString();
					break;

				case Syntax.Byte:
					valueString = ( ( Byte )propertyValue ).ToString();
					break;

				case Syntax.Int16:
					valueString = ( ( Int16 )propertyValue ).ToString();
					break;

				case Syntax.UInt16:
					valueString = ( ( UInt16 )propertyValue ).ToString();
					break;

				case Syntax.Int32:
					valueString = ( ( Int32 )propertyValue ).ToString();
					break;

				case Syntax.UInt32:
					valueString = ( ( UInt32 )propertyValue ).ToString();
					break;

				case Syntax.Int64:
					valueString = ( ( Int64 )propertyValue ).ToString();
					break;

				case Syntax.UInt64:
					valueString = ( ( UInt64 )propertyValue ).ToString();
					break;

				case Syntax.Char:
					valueString = ( ( UInt16 )( ( char )propertyValue )).ToString();
					break;
				
				case Syntax.Single:
					valueString = ( ( Single )propertyValue ).ToString();
					break;

				case Syntax.Boolean:
					valueString = ( ( Boolean )propertyValue == true ) ? "1" : "0";
					break;

				case Syntax.DateTime:
					valueString = ( ( DateTime ) propertyValue ).Ticks.ToString();
					break;

				case Syntax.Uri:
					valueString = ( ( Uri )propertyValue ).ToString();
					break;

				case Syntax.XmlDocument:
					valueString = ( ( XmlDocument )propertyValue ).InnerXml;
					break;

				case Syntax.TimeSpan:
					valueString = ( ( TimeSpan ) propertyValue ).Ticks.ToString();
					break;

				default:
					// There is no top level type that matches this object.  Check for an enumerated type.
					if ( propertyValue.GetType().BaseType.Name == "System.Enum" )
					{
						valueString = ( ( Int32 )propertyValue ).ToString();
					}
					else
					{
						throw new ArgumentException( "An invalid syntax type was specified", "propertyValue" );
					}
					break;
			}

			return valueString;
		}

		/// <summary>
		/// Sets the specified attribute on the property.
		/// </summary>
		/// <param name="attributeName">Name of the property attribute.</param>
		/// <param name="attributeValue">Attribute value.</param>
		private void SetPropertyAttribute( string attributeName, string attributeValue )
		{
			XmlAttribute xmlAttr = xmlProperty.OwnerDocument.CreateAttribute( attributeName );
			xmlAttr.Value = attributeValue;

			switch ( attributeName )
			{
				case NameAttr:
					xmlProperty.Attributes.Prepend( xmlAttr );
					break;

				case TypeAttr:
					xmlProperty.Attributes.InsertAfter( xmlAttr, xmlProperty.Attributes[ NameAttr ] );
					break;

				case FlagsAttr:
					xmlProperty.Attributes.InsertAfter( xmlAttr, xmlProperty.Attributes[ TypeAttr ] );
					break;
			}

			SetPropertyChanged();
		}

		/// <summary>
		/// Sets state that this property has changed and to reflect that change back to the node.
		/// If this property is not attached to a property list, ignore the indication.
		/// </summary>
		private void SetPropertyChanged()
		{
			if ( propertyList != null )
			{
				propertyList.SetListChanged();
			}
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Deletes this property from the property list.
		/// </summary>
		internal void DeleteProperty()
		{
			// Only delete if the property is attached to a list.
			if ( propertyList != null )
			{
				// Create a detached property since this is being removed from the list.
				XmlDocument tempDoc = new XmlDocument();
				tempDoc.AppendChild( tempDoc.ImportNode( xmlProperty, true ) );

				// Remove all the children of this xml property node.
				xmlProperty.RemoveAll();

				// Remove this node from its parent.
				xmlProperty.ParentNode.RemoveChild( xmlProperty );
				SetPropertyChanged();

				// Setup the now detached property.
				propertyList = null;
				xmlProperty = ( XmlElement )tempDoc.FirstChild;
			}
		}

		/// <summary>
		/// Inserts the specified property after this property.
		/// </summary>
		/// <param name="insertProperty">Property to insert.</param>
		internal void InsertPropertyAfter( Property insertProperty )
		{
			if ( propertyList != null )
			{
				// If the insert property is not from the document where it is to be inserted,
				// the property must first be imported into the DOM.
				if ( insertProperty.xmlProperty.OwnerDocument != xmlProperty.OwnerDocument )
				{
					insertProperty.xmlProperty = ( XmlElement )OwnerDocument.ImportNode( insertProperty.xmlProperty, true );
					insertProperty.propertyList = propertyList;
				}

				// Insert the property after this one.
				xmlProperty.ParentNode.InsertAfter( insertProperty.xmlProperty, xmlProperty );
				SetPropertyChanged();
			}
			else
			{
				throw new ArgumentException( "existingProperty not in property list" );
			}
		}

		/// <summary>
		/// Inserts the specified property before this property.
		/// </summary>
		/// <param name="insertProperty">Property to insert.</param>
		internal void InsertPropertyBefore( Property insertProperty )
		{
			if ( propertyList != null )
			{
				// If the insert property is not from the document where it is to be inserted,
				// the property must first be imported into the DOM.
				if ( insertProperty.xmlProperty.OwnerDocument != xmlProperty.OwnerDocument )
				{
					insertProperty.xmlProperty = ( XmlElement )OwnerDocument.ImportNode( insertProperty.xmlProperty, true );
					insertProperty.propertyList = propertyList;
				}

				// Insert the property before this one.
				xmlProperty.ParentNode.InsertBefore( insertProperty.xmlProperty, xmlProperty );
				SetPropertyChanged();
			}
			else
			{
				throw new ArgumentException( "existingProperty not in property list" );
			}
		}

		/// <summary>
		/// Determines if the propertyName is a system property.
		/// </summary>
		/// <returns>true if propertyName specifies a system property.</returns>
		internal bool IsSystemProperty()
		{
			return systemPropertyTable.Contains( xmlProperty.GetAttribute( Property.NameAttr ) );
		}

		/// <summary>
		/// Maps a Property.Operator value to a Persist.Query.Operator type.
		/// </summary>
		/// <param name="op">Property.Operator type to map.</param>
		/// <returns>The value that mapped to Persist.Query.Operator.</returns>
		static internal Persist.Query.Operator MapQueryOp( Property.Operator op )
		{
			return ( Persist.Query.Operator )Enum.Parse( typeof( Persist.Query.Operator ), op.ToString() );
		}

		/// <summary>
		/// Sets the value of this property.
		/// </summary>
		/// <param name="propertyValue">Property value to set.</param>
		internal void SetPropertyValue( object propertyValue )
		{
			// Based on the syntax type, create an object and stuff in the property value.
			switch ( GetSyntaxType( propertyValue ) )
			{
				case Syntax.String:
					SetPropertyValue( ( string )propertyValue );
					break;

				case Syntax.SByte:
					SetPropertyValue( ( sbyte )propertyValue );
					break;

				case Syntax.Byte:
					SetPropertyValue( ( byte )propertyValue );
					break;

				case Syntax.Int16:
					SetPropertyValue( ( short )propertyValue );
					break;

				case Syntax.UInt16:
					SetPropertyValue( ( ushort )propertyValue );
					break;

				case Syntax.Int32:
					SetPropertyValue( ( int )propertyValue );
					break;

				case Syntax.UInt32:
					SetPropertyValue( ( uint )propertyValue );
					break;

				case Syntax.Int64:
					SetPropertyValue( ( long )propertyValue );
					break;

				case Syntax.UInt64:
					SetPropertyValue( ( ulong )propertyValue );
					break;

				case Syntax.Char:
					SetPropertyValue( ( char )propertyValue );
					break;
					
				case Syntax.Single:
					SetPropertyValue( ( float )propertyValue );
					break;

				case Syntax.Boolean:
					SetPropertyValue( ( bool )propertyValue );
					break;

				case Syntax.DateTime:
					SetPropertyValue( ( DateTime )propertyValue );
					break;

				case Syntax.Uri:
					SetPropertyValue( ( Uri )propertyValue );
					break;

				case Syntax.XmlDocument:
					SetPropertyValue( ( XmlDocument )propertyValue );
					break;

				case Syntax.TimeSpan:
					SetPropertyValue( ( TimeSpan )propertyValue );
					break;

				default:
					throw new ArgumentException( "Invalid object type" );
			}
		}

		/// <summary>
		/// Sets a property value into the property.
		/// </summary>
		/// <param name="propertyValue">Property value to set in property.</param>
		internal void SetPropertyValue( Property propertyValue )
		{
			if ( Type != propertyValue.Type )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			if ( Type == Syntax.XmlDocument )
			{
				xmlProperty.InnerXml = propertyValue.xmlProperty.InnerXml;
			}
			else
			{
				xmlProperty.InnerText = propertyValue.xmlProperty.InnerText;
			}

			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a string value into the property.
		/// </summary>
		/// <param name="propertyValue">string value to set in property.</param>
		internal void SetPropertyValue( string propertyValue )
		{
			if ( Type != Syntax.String )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue;
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets an sbyte value into the property.
		/// </summary>
		/// <param name="propertyValue">sbyte value to set in property.</param>
		internal void SetPropertyValue( sbyte propertyValue )
		{
			if ( Type != Syntax.SByte )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue.ToString();
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a byte value into the property.
		/// </summary>
		/// <param name="propertyValue">byte value to set in property.</param>
		internal void SetPropertyValue( byte propertyValue )
		{
			if ( Type != Syntax.Byte )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue.ToString();
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a short value into the property.
		/// </summary>
		/// <param name="propertyValue">short value to set in property.</param>
		internal void SetPropertyValue( short propertyValue )
		{
			if ( Type != Syntax.Int16 )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue.ToString();
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a ushort value into the property.
		/// </summary>
		/// <param name="propertyValue">ushort value to set in property.</param>
		internal void SetPropertyValue( ushort propertyValue )
		{
			if ( Type != Syntax.UInt16 )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue.ToString();
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets an int value into the property.
		/// </summary>
		/// <param name="propertyValue">int value to set in property.</param>
		internal void SetPropertyValue( int propertyValue )
		{
			if ( Type != Syntax.Int32 )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue.ToString();
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a uint value into the property.
		/// </summary>
		/// <param name="propertyValue">uint value to set in property.</param>
		internal void SetPropertyValue( uint propertyValue )
		{
			if ( Type != Syntax.UInt32 )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue.ToString();
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a long value into the property.
		/// </summary>
		/// <param name="propertyValue">long value to set in property.</param>
		internal void SetPropertyValue( long propertyValue )
		{
			if ( Type != Syntax.Int64 )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue.ToString();
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a ulong value into the property.
		/// </summary>
		/// <param name="propertyValue">ulong value to set in property.</param>
		internal void SetPropertyValue( ulong propertyValue )
		{
			if ( Type != Syntax.UInt64 )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue.ToString();
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a char value into the property.
		/// </summary>
		/// <param name="propertyValue">char value to set in property.</param>
		internal void SetPropertyValue( char propertyValue )
		{
			if ( Type != Syntax.Char )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = ( ( ushort )propertyValue ).ToString();
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a float value into the property.
		/// </summary>
		/// <param name="propertyValue">float value to set in property.</param>
		internal void SetPropertyValue( float propertyValue )
		{
			if ( Type != Syntax.Single )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue.ToString();
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a bool value into the property.
		/// </summary>
		/// <param name="propertyValue">bool value to set in property.</param>
		internal void SetPropertyValue( bool propertyValue )
		{
			if ( Type != Syntax.Boolean )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue ? "1" : "0";
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a DateTime value into the property.
		/// </summary>
		/// <param name="propertyValue">DateTime value to set in property.</param>
		internal void SetPropertyValue( DateTime propertyValue )
		{
			if ( Type != Syntax.DateTime )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue.Ticks.ToString();
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a Uri value into the property.
		/// </summary>
		/// <param name="propertyValue">Uri value to set in property.</param>
		internal void SetPropertyValue( Uri propertyValue )
		{
			if ( Type != Syntax.Uri )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue.ToString();
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets an Xml value into the property.
		/// </summary>
		/// <param name="propertyValue">Xml value to set in property.</param>
		internal void SetPropertyValue( XmlDocument propertyValue )
		{
			if ( Type != Syntax.XmlDocument )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerXml = propertyValue.InnerXml;
			SetPropertyChanged();
		}

		/// <summary>
		/// Sets a TimeSpan value into the property.
		/// </summary>
		/// <param name="propertyValue">TimeSpan value to set in property.</param>
		internal void SetPropertyValue( TimeSpan propertyValue )
		{
			if ( Type != Syntax.TimeSpan )
			{
				throw new ApplicationException( "Cannot mix property types" );
			}

			xmlProperty.InnerText = propertyValue.Ticks.ToString();
			SetPropertyChanged();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Deletes this property from the property list.
		/// </summary>
		public void Delete()
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			DeleteProperty();
		}

		/// <summary>
		/// Inserts the specified property after this property.
		/// </summary>
		/// <param name="insertProperty">Property to insert.</param>
		public void InsertAfter( Property insertProperty )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			InsertPropertyAfter( insertProperty );
		}

		/// <summary>
		/// Inserts the specified property before this property.
		/// </summary>
		/// <param name="insertProperty">Property to insert.</param>
		public void InsertBefore( Property insertProperty )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			InsertPropertyBefore( insertProperty );
		}

		/// <summary>
		/// Sets the value of this property.
		/// </summary>
		/// <param name="propertyValue">Property value to set.</param>
		public void SetValue( Property propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets the value of this property.
		/// </summary>
		/// <param name="propertyValue">Property value to set.</param>
		public void SetValue( object propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a string value into the property.
		/// </summary>
		/// <param name="propertyValue">string value to set in property.</param>
		public void SetValue( string propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets an sbyte value into the property.
		/// </summary>
		/// <param name="propertyValue">sbyte value to set in property.</param>
		public void SetValue( sbyte propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a byte value into the property.
		/// </summary>
		/// <param name="propertyValue">byte value to set in property.</param>
		public void SetValue( byte propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a short value into the property.
		/// </summary>
		/// <param name="propertyValue">short value to set in property.</param>
		public void SetValue( short propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a ushort value into the property.
		/// </summary>
		/// <param name="propertyValue">ushort value to set in property.</param>
		public void SetValue( ushort propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets an int value into the property.
		/// </summary>
		/// <param name="propertyValue">int value to set in property.</param>
		public void SetValue( int propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a uint value into the property.
		/// </summary>
		/// <param name="propertyValue">uint value to set in property.</param>
		public void SetValue( uint propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a long value into the property.
		/// </summary>
		/// <param name="propertyValue">long value to set in property.</param>
		public void SetValue( long propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a ulong value into the property.
		/// </summary>
		/// <param name="propertyValue">ulong value to set in property.</param>
		public void SetValue( ulong propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a char value into the property.
		/// </summary>
		/// <param name="propertyValue">char value to set in property.</param>
		public void SetValue( char propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a float value into the property.
		/// </summary>
		/// <param name="propertyValue">float value to set in property.</param>
		public void SetValue( float propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a bool value into the property.
		/// </summary>
		/// <param name="propertyValue">bool value to set in property.</param>
		public void SetValue( bool propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a DateTime value into the property.
		/// </summary>
		/// <param name="propertyValue">DateTime value to set in property.</param>
		public void SetValue( DateTime propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a Uri value into the property.
		/// </summary>
		/// <param name="propertyValue">Uri value to set in property.</param>
		public void SetValue( Uri propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets an Xml value into the property.
		/// </summary>
		/// <param name="propertyValue">Xml value to set in property.</param>
		public void SetValue( XmlDocument propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Sets a TimeSpan value into the property.
		/// </summary>
		/// <param name="propertyValue">TimeSpan value to set in property.</param>
		public void SetValue( TimeSpan propertyValue )
		{
			// Make sure proper access is obtained.
			CheckAccessControl( true );
			SetPropertyValue( propertyValue );
		}

		/// <summary>
		/// Converts the property value of this instance to its equivalent string.
		/// </summary>
		/// <returns>A string representing the value of this object.</returns>
		public override string ToString()
		{
			string output = null;

			switch ( Type )
			{
				case Syntax.String:
					output = ( ( string )GetValue() ).ToString();
					break;
					
				case Syntax.SByte:
					output = ( ( sbyte )GetValue() ).ToString();
					break;

				case Syntax.Byte:
					output = ( ( byte )GetValue() ).ToString();
					break;

				case Syntax.Int16:
					output = ( ( short )GetValue() ).ToString();
					break;

				case Syntax.UInt16:
					output = ( ( ushort )GetValue() ).ToString();
					break;

				case Syntax.Int32:
					output = ( ( int )GetValue() ).ToString();
					break;

				case Syntax.UInt32:
					output = ( ( uint )GetValue() ).ToString();
					break;

				case Syntax.Int64:
					output = ( ( long )GetValue() ).ToString();
					break;

				case Syntax.UInt64:
					output = ( ( ulong )GetValue() ).ToString();
					break;

				case Syntax.Char:
					output = ( ( char )GetValue() ).ToString();
					break;
					
				case Syntax.Single:
					output = ( ( float )GetValue() ).ToString();
					break;

				case Syntax.Boolean:
					output = ( ( bool )GetValue() ).ToString();
					break;

				case Syntax.DateTime:
					output = ( ( DateTime )GetValue() ).ToString();
					break;

				case Syntax.Uri:
					output = ( ( Uri )GetValue() ).ToString();
					break;

				case Syntax.XmlDocument:
					output = ( ( XmlDocument )GetValue() ).ToString();
					break;

				case Syntax.TimeSpan:
					output = ( ( TimeSpan )GetValue() ).ToString();
					break;
			}

			return output;
		}

		/// <summary>
		///	Converts the property value of this instance to its equivalent string using the specified 
		///	culture-specific format information.
		/// </summary>
		/// <param name="ifProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <returns>A string representing the value of this object.</returns>
		public string ToString( IFormatProvider ifProvider )
		{
			string output = null;

			switch ( Type )
			{
				case Syntax.String:
					output = ( ( string )GetValue() ).ToString( ifProvider );
					break;

				case Syntax.SByte:
					output = ( ( sbyte )GetValue() ).ToString( ifProvider );
					break;

				case Syntax.Byte:
					output = ( ( byte )GetValue() ).ToString( ifProvider );
					break;

				case Syntax.Int16:
					output = ( ( short )GetValue() ).ToString( ifProvider );
					break;

				case Syntax.UInt16:
					output = ( ( ushort )GetValue() ).ToString( ifProvider );
					break;

				case Syntax.Int32:
					output = ( ( int )GetValue() ).ToString( ifProvider );
					break;

				case Syntax.UInt32:
					output = ( ( uint )GetValue() ).ToString( ifProvider );
					break;

				case Syntax.Int64:
					output = ( ( long )GetValue() ).ToString( ifProvider );
					break;

				case Syntax.UInt64:
					output = ( ( ulong )GetValue() ).ToString( ifProvider );
					break;

				case Syntax.Char:
					output = ( ( char )GetValue() ).ToString( ifProvider );
					break;
					
				case Syntax.Single:
					output = ( ( float )GetValue() ).ToString( ifProvider );
					break;

				case Syntax.Boolean:
					output = ( ( bool )GetValue() ).ToString( ifProvider );
					break;

				case Syntax.DateTime:
					output = ( ( DateTime )GetValue() ).ToString( ifProvider );
					break;

				default:
					throw new ArgumentException( "Objects of type " + Type.ToString() + " do not take formatters" );
			}

			return output;
		}

		/// <summary>
		/// Converts the property value of this instance to its equivalent string using the specified format.
		/// </summary>
		/// <param name="format">A string that specifies the return format.</param>
		/// <returns>A string representing the value of this object.</returns>
		public string ToString( string format )
		{
			string output = null;

			switch ( Type )
			{
				case Syntax.SByte:
					output = ( ( sbyte )GetValue() ).ToString( format );
					break;

				case Syntax.Byte:
					output = ( ( byte )GetValue() ).ToString( format );
					break;

				case Syntax.Int16:
					output = ( ( short )GetValue() ).ToString( format );
					break;

				case Syntax.UInt16:
					output = ( ( ushort )GetValue() ).ToString( format );
					break;

				case Syntax.Int32:
					output = ( ( int )GetValue() ).ToString( format );
					break;

				case Syntax.UInt32:
					output = ( ( uint )GetValue() ).ToString( format );
					break;

				case Syntax.Int64:
					output = ( ( long )GetValue() ).ToString( format );
					break;

				case Syntax.UInt64:
					output = ( ( ulong )GetValue() ).ToString( format );
					break;

				case Syntax.Single:
					output = ( ( float )GetValue() ).ToString( format );
					break;

				case Syntax.DateTime:
					output = ( ( DateTime )GetValue() ).ToString( format );
					break;

				default:
					throw new ArgumentException( "Objects of type " + Type.ToString() + " do not take formatters" );
			}

			return output;
		}

		/// <summary>
		/// Converts the numeric value of this instance to its equivalent string using the specified 
		/// format and culture-specific format information.
		/// </summary>
		/// <param name="format">A string that specifies the return format.</param>
		/// <param name="ifProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
		/// <returns>A string representing the value of this object.</returns>
		public string ToString( string format, IFormatProvider ifProvider )
		{
			string output = null;

			switch ( Type )
			{
				case Syntax.SByte:
					output = ( ( sbyte )GetValue() ).ToString( format, ifProvider );
					break;

				case Syntax.Byte:
					output = ( ( byte )GetValue() ).ToString( format, ifProvider );
					break;

				case Syntax.Int16:
					output = ( ( short )GetValue() ).ToString( format, ifProvider );
					break;

				case Syntax.UInt16:
					output = ( ( ushort )GetValue() ).ToString( format, ifProvider );
					break;

				case Syntax.Int32:
					output = ( ( int )GetValue() ).ToString( format, ifProvider );
					break;

				case Syntax.UInt32:
					output = ( ( uint )GetValue() ).ToString( format, ifProvider );
					break;

				case Syntax.Int64:
					output = ( ( long )GetValue() ).ToString( format, ifProvider );
					break;

				case Syntax.UInt64:
					output = ( ( ulong )GetValue() ).ToString( format, ifProvider );
					break;

				case Syntax.Single:
					output = ( ( float )GetValue() ).ToString( format, ifProvider );
					break;

				case Syntax.DateTime:
					output = ( ( DateTime )GetValue() ).ToString( format, ifProvider );
					break;

				default:
					throw new ArgumentException( "Objects of type " + Type.ToString() + " do not take formatters" );
			}

			return output;
		}
		#endregion
	}
}
