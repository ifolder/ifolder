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
			Simias.Gaim.GaimDomain gaimDomain = new Simias.Gaim.GaimDomain( false );
			Simias.Storage.Domain domain = gaimDomain.GetGaimDomain( false, "" );
			if ( domain == null )
			{
				throw new SimiasException( "Gaim domain does not exist" );
			}

			GaimDomainInfo info = new GaimDomainInfo();
			info.ID = domain.ID;
			info.Name = domain.Name;
			info.Description = domain.Description;
			
			info.RosterID = domain.Roster.ID;
			info.RosterName = domain.Roster.Name;

			return info;
		}

		/// <summary>
		/// Adds a new Buddy to the Gaim Domain Roster.  If the buddy already
		/// exists, any modified information will be updated in the roster.
		/// </summary>
		/// <param name="acountName">The Gaim account name (local user's screenname).</param>
		/// <param name="accountProto">The Gaim account protocol (i.e., prpl-oscar).</param>
		/// <param name="buddyName">The buddy's screenname</param>
		/// <param name="alias">The buddy's alias</param>
		/// <param name="ipAddr">IP Address of the Simias WebService running on the buddy's computer.</param>
		/// <param name="ipPort">IP Port of the Simias WebService running on the buddy's computer. </param>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public void AddGaimBuddy(string accountName, string accountProto, string buddyName,
								 string alias, string ipAddr, string ipPort)
		{
			Simias.Gaim.GaimDomain gaimDomain = new Simias.Gaim.GaimDomain(false);
			Simias.Storage.Domain domain = gaimDomain.GetGaimDomain(false, "");
			if (domain == null)
			{
				throw new SimiasException("Gaim domain does not exist");
			}
			
			// If user already exists, call UpdateGaimBuddy(accountName, accountProto,
			//												buddyName, alias, ipAddr, ipPort);
		}

		/// <summary>
		/// Removes a buddy from the Gaim Domain Roster and from any
		/// collections the buddy is a member of.
		/// </summary>
		/// <param name="acountName">The Gaim account name (local user's screenname).</param>
		/// <param name="accountProto">The Gaim account protocol (i.e., prpl-oscar).</param>
		/// <param name="buddyName">The buddy's screenname</param>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public void RemoveGaimBuddy(string accountName, string accountProto, string buddyName)
		{
			Simias.Gaim.GaimDomain gaimDomain = new Simias.Gaim.GaimDomain(false);
			Simias.Storage.Domain domain = gaimDomain.GetGaimDomain(false, "");
			if (domain == null)
			{
				throw new SimiasException("Gaim domain does not exist");
			}
			
			// FIXME: Search through the Gaim Domain Roster looking for the buddy
			// and when/if the buddy is found, remove all memberships from collections
			// and from the Gaim Domain Roster.
		}
		
		/// <summary>
		/// Updates the Buddy's alias, ipAddr, and ipPort
		/// </summary>
		/// <param name="acountName">The Gaim account name (local user's screenname).</param>
		/// <param name="accountProto">The Gaim account protocol (i.e., prpl-oscar).</param>
		/// <param name="buddyName">The buddy's screenname</param>
		/// <param name="alias">The buddy's alias</param>
		/// <param name="ipAddr">IP Address of the Simias WebService running on the buddy's computer.</param>
		/// <param name="ipPort">IP Port of the Simias WebService running on the buddy's computer. </param>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public void UpdateGaimBuddy(string accountName, string accountProto, string buddyName,
									string alias, string ipAddr, string ipPort)
		{
			Simias.Gaim.GaimDomain gaimDomain = new Simias.Gaim.GaimDomain(false);
			Simias.Storage.Domain domain = gaimDomain.GetGaimDomain(false, "");
			if (domain == null)
			{
				throw new SimiasException("Gaim domain does not exist");
			}
			
			// FIXME: Implement UpdateGaimBuddy()
		}
	}
}
