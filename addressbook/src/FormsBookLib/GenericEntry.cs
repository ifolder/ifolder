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

namespace Novell.iFolder.FormsBookLib
{
	/// <summary>
	/// The base class for entries in a contact.
	/// </summary>
	public class GenericEntry
	{
		private bool add;
		private bool remove;

		/// <summary>
		/// Initializes a new instance of the GenericEntry class.
		/// </summary>
		public GenericEntry()
		{
			remove = false;
			add = false;
		}

		#region Properties
		/// <summary>
		/// Gets/sets a value indicating if the contact has been added.
		/// </summary>
		public bool Add
		{
			get
			{
				return add;
			}
			set
			{
				add = value;
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if the contact has been removed.
		/// </summary>
		public bool Remove
		{
			get
			{
				return remove;
			}
			set
			{
				remove = value;
			}
		}
		#endregion
	}
}
