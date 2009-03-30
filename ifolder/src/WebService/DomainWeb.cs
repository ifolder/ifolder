/****************************************************************************
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
 | Author: Calvin Gaisford <cgaisford@novell.com>
 |**************************************************************************/

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
