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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/


using System;
using Simias;
using Simias.Location;
using Simias.Storage;

namespace Novell.iFolder.Web
{
	/// <summary>
	/// This class exists to pass Domain information through the web service
	/// </summary>
	[Serializable]
	public class DomainWeb
	{
		public string ID;
		public string POBoxID;
		public string Name;
		public string Description;
		public string Host;
		public string UserID;
		public string UserName;
		public bool IsDefault;
		public bool IsSlave;
		public bool IsEnabled;
		public bool IsConnected;

		public DomainWeb()
		{}


		public DomainWeb(string domainID)
		{
			Store store = Store.GetStore();

			this.ID = domainID;
			Domain domain = store.GetDomain(domainID);
			this.Name = domain.Name;
			this.Description = domain.Description;
			this.Host = Locate.Resolve(domainID);
			this.UserID = store.GetUserIDFromDomainID(domainID);
			this.IsDefault = domainID.Equals(store.DefaultDomain);


			Simias.POBox.POBox poBox = Simias.POBox.POBox.FindPOBox(store, 
						domainID, 
						UserID);

			if(poBox != null)
			{
				this.POBoxID = poBox.ID;
			}

			this.UserName = domain.GetMemberByID(this.UserID).Name;
			this.IsSlave = domain.Role.Equals(Simias.Sync.SyncRoles.Slave);
			this.IsEnabled = new Simias.Domain.DomainAgent().IsDomainActive(domainID);
		}

	}
}
