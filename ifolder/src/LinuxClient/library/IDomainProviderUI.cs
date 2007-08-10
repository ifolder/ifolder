/***********************************************************************
 |  $RCSfile: IDomainProviderUI.cs,v $
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
	public interface IDomainProviderUI
	{
		#region Properties
		
		string ID { get; }
		
		string Name { get; }
		
		string Description { get; }
		
		bool CanDelete { get; }
		
		bool HasDetails { get; }
		
		#endregion
		
		#region Public Methods
		
		AccountDialog CreateAccountDialog(Window parent, DomainInformation domain);
		
		#endregion
	}
}
