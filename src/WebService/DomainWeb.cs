/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
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
