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

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for DomainConnectEventArgs.
	/// </summary>
	public class DomainRemoveEventArgs : EventArgs
	{
		private DomainInformation domainInfo;
		private string defaultDomainID;

		/// <summary>
		/// Constructs a DomainConnectEventArgs object.
		/// </summary>
		public DomainRemoveEventArgs(DomainInformation domainInfo, string defaultDomainID)
		{
			this.domainInfo = domainInfo;
			this.defaultDomainID = defaultDomainID;
		}

		/// <summary>
		/// Gets the domainInfo object.
		/// </summary>
		public DomainInformation DomainInfo
		{
			get { return domainInfo; }
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
