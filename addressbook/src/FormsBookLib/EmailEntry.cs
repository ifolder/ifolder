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
using Novell.AddressBook;

namespace Novell.iFolder.FormsBookLib
{
	/// <summary>
	/// An email entry for a contact.
	/// </summary>
	public class EmailEntry : GenericEntry
	{
		private Email email;

		/// <summary>
		/// Initializes a new instance of the EmailEntry class.
		/// </summary>
		public EmailEntry()
		{
		}

		/// <summary>
		/// Gets/sets the email entry.
		/// </summary>
		public Email EMail
		{
			get
			{
				return email;
			}
			set
			{
				email = value;
			}
		}
	}
}
