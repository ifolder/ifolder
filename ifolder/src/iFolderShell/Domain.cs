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

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for Domain.
	/// </summary>
	public class Domain
	{
		private DomainWeb domainWeb;
		private string name;

		/// <summary>
		/// Constructs a Domain object.
		/// </summary>
		/// <param name="domainWeb">The web service domain object to base this domain object on.</param>
		public Domain(DomainWeb domainWeb)
		{
			this.domainWeb = domainWeb;
		}

		public Domain(string name)
		{
			this.name = name;
		}

		#region Properties
		/// <summary>
		/// Gets the web service domain object.
		/// </summary>
		public DomainWeb DomainWeb
		{
			get { return domainWeb;}
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public string ID
		{
			get
			{
				if ((name != null) && !name.Equals(string.Empty))
					return name;
				else
					return domainWeb.ID;
			}
		}
		#endregion

		public override string ToString()
		{
			if ((name != null) && !name.Equals(string.Empty))
				return name;
			else
				return domainWeb.Name;
		}
	}
}
