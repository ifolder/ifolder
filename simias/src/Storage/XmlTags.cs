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
	/// Summary description for XmlTags.
	/// </summary>
	public class XmlTags
	{
		/// <summary>
		/// Xml Tag for a list of objects.
		/// </summary>
		public const string ObjectListTag = "ObjectList";
		/// <summary>
		/// Xml Tag for an object definition.
		/// </summary>
		public const string ObjectTag = "Object";
		/// <summary>
		/// Xml tag for a property definition.
		/// </summary>
		public const string PropertyTag = "Property";
		/// <summary>
		/// Xml attribute for name.
		/// </summary>
		public const string NameAttr = "name";
		/// <summary>
		/// Xml attribute for id.
		/// </summary>
		public const string IdAttr = "id";
		/// <summary>
		/// Xml attribute for colliction id.
		/// </summary>
		public const string CIdAttr = "cid";
		/// <summary>
		/// Xml attribute for type.
		/// </summary>
		public const string TypeAttr = "type";

		/// <summary>
		/// Xml attribute for property flags.
		/// </summary>
		public const string FlagsAttr = "flags";
	}
}
