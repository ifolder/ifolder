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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;

namespace Simias.Storage
{
	/// <summary>
	/// Represents syntax types that are supported by the simias database.
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
		TimeSpan,

		/// <summary>
		/// Represents a relationship to another object. The value of a relationship contains a Node object.
		/// This syntax type operates much like a soft link in a file system. The relationship
		/// might be broken if the Node object is deleted or cannot be referenced for any other reasons and
		/// will remain that way until fixed.
		/// </summary>
		Relationship
	};
}
