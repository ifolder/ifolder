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
using Simias.Storage;

namespace Novell.iFolder.Web
{
	/// <summary>
	/// This class exists to pass settings information about the Simias
	/// setup via the WebService
	/// </summary>
	[Serializable]
	public class iFolderSettings
	{
		public string DefaultDomainID;
		public bool HaveEnterprise;
		public bool DisplayConfirmation;
		public string EnterpriseName;
		public string EnterpriseDescription;
		public int DefaultSyncInterval;
		public bool UseProxy;
		public string ProxyHost;
		public int ProxyPort;

		public iFolderSettings()
		{
			Store store = Store.GetStore();
			DefaultDomainID = store.DefaultDomain;
			
			HaveEnterprise = false;
			Domain enterpriseDomain = null;
			LocalDatabase localDB = store.GetDatabaseObject();
			ICSList domainList = localDB.GetNodesByType(typeof(Domain).Name);
			foreach (ShallowNode sn in domainList)
			{
				if (!sn.Name.Equals(Domain.WorkGroupDomainName))
				{
					HaveEnterprise = true;
					enterpriseDomain = store.GetDomain(sn.ID);
					break;
				}
			}

			if (enterpriseDomain != null)
			{
				EnterpriseName = enterpriseDomain.Name;
				EnterpriseDescription = enterpriseDomain.Description;
			}
			
			Configuration config = Configuration.GetConfiguration();
			bool defaultValue = true;
			DisplayConfirmation = config.Get("iFolderUI", "Display Confirmation", defaultValue.ToString()) == defaultValue.ToString();

			DefaultSyncInterval = 
				Simias.Policy.SyncInterval.GetInterval();

			// I don't know how to do this but I know we'll need it
			UseProxy = false;
		}

		public static void SetDisplayConfirmation(bool displayConfirmation)
		{
			Configuration config = Configuration.GetConfiguration();
			config.Set("iFolderUI", "Display Confirmation", displayConfirmation.ToString());
		}
	}
}
