/***********************************************************************
 |  $RCSfile: BonjourDomainProviderUI.cs,v $
 |
 | Copyright (c) 2007 Novell, Inc.
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
 |  Authors:
 |		Boyd Timothy <btimothy@novell.com>
 |
 ***********************************************************************/

using System;

using Gtk;

using Simias.Client;

using Novell.iFolder;
 
namespace Novell.iFolder.DomainProvider
{
	public class BonjourDomainProviderUI : IDomainProviderUI
	{
		#region Class Members

		private const string id = "74d3a71f-daae-4a36-b9f3-6466081f6401";
		private const string name = "iFolder Peer to Peer (P2P) DomainProviderUI";
		private const string description = "This module provides the UI for the iFolder Peer to Peer (P2P) domain";
		
		#endregion
		
		#region Properties
		
		public string ID
		{
			get
			{
				return id;
			}
		}
		
		public string Name
		{
			get
			{
				return name;
			}
		}
		
		public string Description
		{
			get
			{
				return description;
			}
		}
		
		public bool CanDelete
		{
			get
			{
				return false;
			}
		}
		
		public bool HasDetails
		{
			get
			{
				return true;
			}
		}
		
		#endregion
		
		#region Public Methods

		public AccountDialog CreateAccountDialog(Window parent, DomainInformation domain)
		{
			BonjourAccountDialog dialog =
				new BonjourAccountDialog(parent, domain);

			return dialog;
		}
		
		#endregion
		
		#region Private Methods
		#endregion
	}
}
