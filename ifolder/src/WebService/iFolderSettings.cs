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
		public string DefaultPOBoxID;
		public bool HaveEnterprise;
		public bool DisplayConfirmation;
		public string EnterpriseName;
		public string EnterpriseDescription;
		public int DefaultSyncInterval;
		public bool UseProxy;
		public string ProxyHost;
		public int ProxyPort;
		public string CurrentUserID;
		public string CurrentUserName;

		public iFolderSettings()
		{
			Store store = Store.GetStore();

			// I don't know how to do this but I know we'll need it
			UseProxy = false;

			string dcString = store.Config.Get("iFolderUI",
				"Display Confirmation",
				Boolean.TrueString);

			DisplayConfirmation = String.Compare(dcString,
				Boolean.TrueString,
				true) == 0;

			// Get the interval for the current machine.
			DefaultSyncInterval = 
				Simias.Policy.SyncInterval.GetInterval();

			DefaultDomainID = store.DefaultDomain;
			CurrentUserID = store.GetUserIDFromDomainID(DefaultDomainID);
			
			Simias.POBox.POBox poBox = Simias.POBox.POBox.FindPOBox(store, 
						DefaultDomainID, 
						CurrentUserID);

			if (DefaultDomainID == Domain.WorkGroupDomainID)
			{
				if (poBox == null)
				{
					// Create the POBox on workgroup if it doesn't exist.
					poBox = Simias.POBox.POBox.GetPOBox(store, 
						DefaultDomainID);
				}

				// Check if there is an enterprise domain, but the default is
				// selected to be workgroup.
				HaveEnterprise = store.IsEnterpriseClient;
			}
			else
			{
				HaveEnterprise = true;
				Domain enterpriseDomain = store.GetDomain(DefaultDomainID);
				if(enterpriseDomain != null)
				{
					EnterpriseName = enterpriseDomain.Name;
					EnterpriseDescription = enterpriseDomain.Description;
				}
			}

			if(poBox != null)
			{
				DefaultPOBoxID = poBox.ID;
			}

			// If the LocalIncarnation number is zero, this collection is a
			// proxy and there is no current member yet.
			Roster roster = store.GetRoster(DefaultDomainID);
			if(!roster.IsProxy)
			{
				Member currentMember = roster.GetCurrentMember();
				if(currentMember != null)
				{
					CurrentUserName = currentMember.Name;
				}
			}
		}

		public static void SetDisplayConfirmation(bool displayConfirmation)
		{
			Configuration config = Configuration.GetConfiguration();
			config.Set("iFolderUI", "Display Confirmation", 
										displayConfirmation.ToString());
		}
	}
}
