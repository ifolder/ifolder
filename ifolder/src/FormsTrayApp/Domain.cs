/****************************************************************************
 |
 | Copyright (c) [2007] Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 | Author: Bruce Getter <bgetter@novell.com>
 |
 |***************************************************************************/

using System;
using System.Text;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for Domain.
	/// </summary>
	public class Domain
	{
		private DomainInformation domainInfo;
		private string name;
		private bool showAll = false;

		/// <summary>
		/// Constructs a Domain object.
		/// </summary>
		/// <param name="domainInfo">The web service DomainInformation object to base this domain object on.</param>
		public Domain(DomainInformation domainInfo)
		{
			this.domainInfo = domainInfo;
		}

		/// <summary>
		/// Constructs a Domain object representing a wild card "Show All" Domain.
		/// </summary>
		/// <param name="name">The name of the Domain.</param>
		public Domain(string name)
		{
			this.name = name;
			this.showAll = true;
		}

		#region Properties
		/// <summary>
		/// Gets the web service domain object.
		/// </summary>
		public DomainInformation DomainInfo
		{
			get { return domainInfo; }
			set { domainInfo = value; }
		}

		/// <summary>
		/// Gets/sets the name of the domain object.
		/// </summary>
		public string Name
		{
			get 
			{
				return showAll ? name : domainInfo.Name;
			}
			set { name = value; }
		}

		/// <summary>
		/// Gets the ID of the domain object.
		/// </summary>
		public string ID
		{
			get
			{
				if (showAll)
					return name;
				else
					return domainInfo.ID;
			}
		}

		/// <summary>
		/// Gets a value indicating if this is the wild card domain.
		/// </summary>
		public bool ShowAll
		{
			get { return showAll; }
		}
		#endregion

		/// <summary>
		/// Gets a string representation of the Domain object.
		/// </summary>
		/// <returns>A string representing the name of the Domain object.</returns>
		public override string ToString()
		{
			if (showAll)
				return name;
			else
				return domainInfo.Name;
		}
	}
}
