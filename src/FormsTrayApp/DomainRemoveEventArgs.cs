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

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for DomainConnectEventArgs.
	/// </summary>
	public class DomainRemoveEventArgs : EventArgs
	{
		private DomainInformation domainWeb;
		private string defaultDomainID;

		/// <summary>
		/// Constructs a DomainConnectEventArgs object.
		/// </summary>
		public DomainRemoveEventArgs(DomainInformation domainWeb, string defaultDomainID)
		{
			this.domainWeb = domainWeb;
			this.defaultDomainID = defaultDomainID;
		}

		/// <summary>
		/// Gets the DomainWeb object.
		/// </summary>
		public DomainInformation DomainWeb
		{
			get { return domainWeb; }
		}

		/// <summary>
		/// Gets the new default domain ID.
		/// </summary>
		public string DefaultDomainID
		{
			get { return defaultDomainID; }
		}
	}
}
