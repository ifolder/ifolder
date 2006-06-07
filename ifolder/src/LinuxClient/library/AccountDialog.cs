/***********************************************************************
 *  $RCSfile: AccountDialog.cs,v $
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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/


using Gtk;
using System;

namespace Novell.iFolder
{
	public abstract class AccountDialog : Dialog
	{
		protected Window parentWindow;
		protected DomainInformation domain;

		public string DomainID
		{
			get { return domain.ID; }
		}


		public AccountDialog(Window parent, DomainInformation curDomain)
			: base()
		{
			parentWindow = parent;
			domain = curDomain;
		}
		
		~AccountDialog()
		{
		}
	}
}
