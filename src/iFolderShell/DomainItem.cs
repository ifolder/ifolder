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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Text;
using System.Runtime.InteropServices;

using Simias.Client;

namespace Novell.iFolderCom
{
	/// <summary>
	/// A class used to represent a domain in a list.
	/// </summary>
	[ComVisible(false)]
	public class DomainItem
	{
		private string name;
		private string id;

		/// <summary>
		/// Constructs a DomainInfo object.
		/// </summary>
		/// <param name="name">The name of the object.</param>
		/// <param name="ID">The ID of the object.</param>
		public DomainItem(string name, string ID)
		{
			this.name = name;
			this.id = ID;
		}

		#region Properties
		/// <summary>
		/// Gets/sets the name of the domain object.
		/// </summary>
		public string Name
		{
			get { return name;	}
			set { name = value; }
		}

		/// <summary>
		/// Gets the ID of the domain object.
		/// </summary>
		public string ID
		{
			get	{ return id; }
		}
		#endregion

		/// <summary>
		/// Gets a string representation of the Domain object.
		/// </summary>
		/// <returns>A string representing the name of the Domain object.</returns>
		public override string ToString()
		{
			return name;
		}
	}
}
