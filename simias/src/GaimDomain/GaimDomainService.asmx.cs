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
 *  Author(s):
 *		Boyd Timothy <btimothy@novell.com>
 *		Rob (this code is a mostly copy-n-paste from him)
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

using Simias;
using Simias.Client;
using Simias.Domain;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;

namespace Simias.Gaim.DomainService
{
	/// <summary>
	/// Domain Service
	/// </summary>
	[WebService(
		Namespace="http://novell.com/simias/domain",
		Name="Gaim Domain Service",
		Description="Web Service providing access to Gaim Domain Server functionality.")]
	public class GaimDomainService : System.Web.Services.WebService
	{
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructor
		/// </summary>
		public GaimDomainService()
		{
		}
		
		/// <summary>
		/// Get domain information
		/// </summary>
		/// <param name="userID">The user ID of the member requesting domain information.</param>
		/// <returns>A GaimDomainInfo object that contains information about the enterprise server.</returns>
		/// 
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public GaimDomainInfo GetGaimDomainInfo()
		{
			// domain
			Simias.Storage.Domain domain = GaimDomain.GetDomain();
			if ( domain == null )
			{
				throw new SimiasException( "Gaim domain does not exist" );
			}

			GaimDomainInfo info = new GaimDomainInfo();
			info.ID = domain.ID;
			info.Name = domain.Name;
			info.Description = domain.Description;
			
			return info;
		}

		/// <summary>
		/// Pokes the Synchronization Thread to run/update
		/// </summary>
		[WebMethod(Description="SynchronizeMemberList")]
		[SoapDocumentMethod]
		public void SynchronizeMemberList()
		{
			log.Debug("GaimDomainService.SynchronizeMemberList() entered");
			Simias.Gaim.Sync.SyncNow(null);
		}
		
		/// <summary>
		/// This method is called by the Gaim iFolder Plugin when it
		/// receives a ping response message (which contains a Simias
		/// URL for a buddy.
		///
		/// Causes the Gaim Domain to re-read/sync the specified buddy's
		/// information into Simias.  We don't want to provide the ability
		/// to directly update the SimiasURL from the WebService so that
		/// any random program can't just muck with the data.
		/// </summary>
		[WebMethod(Description="UpdateMember")]
		[SoapDocumentMethod]
		public void UpdateMember(string AccountName, string AccountProtocolID, string BuddyName, string MachineName)
		{
			log.Debug("GaimDomainService.UpdateMember() entered");
			GaimDomain.UpdateMember(AccountName, AccountProtocolID, BuddyName, MachineName);
		}

		/// <summary>
		/// Returns the Machine Name (Host Name less the domain), User ID (The Gaim
		/// Domain Owner's UserID/ACE), Simias URL (Local Service URL).
		/// </summary>
		/// <returns>
		/// Returns true if all the "out" parameters were set correctly.  Otherwise, the
		/// function returns false, which signals there was an error and the out parameters
		/// are incomplete.
		/// </returns>
		[WebMethod(Description="GetUserInfo")]
		[SoapDocumentMethod]
		public bool GetUserInfo(out string MachineName, out string UserID, out string SimiasURL)
		{
			MachineName = null;
			UserID = null;
			SimiasURL = null;

			// Machine Name
			MachineName = GetMachineName().ToLower();

			// UserID
			Simias.Storage.Domain domain = GaimDomain.GetDomain();
			if (domain != null)
			{
				UserID = Store.GetStore().GetUserIDFromDomainID(domain.ID);
			}
			
			// SimiasURL
			SimiasURL = Manager.LocalServiceUrl.ToString();
			
			if (MachineName != null && MachineName.Length > 0
				&& UserID != null && UserID.Length > 0
				&& SimiasURL != null && SimiasURL.Length > 0)
				return true;
			
			return false;
		}
		
		internal string GetMachineName()
		{
			string machineName = Environment.MachineName;
			// If a machine name with domain is added, remove the domain information
			int firstDot = machineName.IndexOf('.');
			if (firstDot > 0)
				machineName = machineName.Substring(0, firstDot);
				
			return machineName;
		}
	}
}
