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

using Novell.AddressBook;

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
		/// Get all the users in the Gaim Domain
		/// </summary>
		[WebMethod(Description="GetAllBuddies")]
		[SoapDocumentMethod]
		public GaimBuddy[] GetAllBuddies()
		{
			ArrayList buddies = new ArrayList();
			Simias.Storage.Domain domain = GaimDomain.GetDomain();
			if (domain == null)
			{
				throw new SimiasException("Could not get Simias.Storage.Domain from GaimDomain!");
			}
			
			ICSList domainMembers = domain.GetMemberList();
			foreach (ShallowNode sNode in domainMembers)
			{
				Simias.Storage.Member member =	
					new Simias.Storage.Member(domain, sNode);

				GaimBuddy buddy = new GaimBuddy(member);
				buddies.Add(buddy);
			}

			return (GaimBuddy[]) buddies.ToArray(typeof(GaimBuddy));
		}
		
		/// <summary>
		/// Pokes the Synchronization Thread to run/update
		/// </summary>
		[WebMethod(Description="SynchronizeMemberList")]
		[SoapDocumentMethod]
		public void SynchronizeMemberList()
		{
			GaimDomain.SynchronizeMembers();
		}
	}

	/// <summary>
	/// This class exists only to represent a Member and should only be
	/// used in association with the GaimDomainService class.
	/// </summary>
	[Serializable]
	public class GaimBuddy
	{
		public string Name;
		public string UserID;
		public string Rights;
		public string ID;
		public bool IsOwner;

		public string GaimAccountName;
		public string GaimAccountProto;
		public string GaimAlias;
		public string IPAddress;
		public string IPPort;

		public GaimBuddy()
		{
		}

		public GaimBuddy(Simias.Storage.Member member)
		{
			this.Name = member.Name;
			this.UserID = member.UserID;
			this.Rights = member.Rights.ToString();
			this.ID = member.ID;
			this.IsOwner = member.IsOwner;

			Simias.Storage.PropertyList pList = member.Properties;
			Simias.Storage.Property prop = pList.GetSingleProperty("Gaim:AccountName");
			if (prop != null)
				this.GaimAccountName = (string) prop.Value;
			else
				this.GaimAccountName = "";
				
			prop = pList.GetSingleProperty("Gaim:AccountProto");
			if (prop != null)
				this.GaimAccountProto = (string) prop.Value;
			else
				this.GaimAccountProto = "";
				
			prop = pList.GetSingleProperty("Gaim:Alias");
			if (prop != null)
				this.GaimAlias = (string) prop.Value;
			else
				this.GaimAlias = "";
				
			prop = pList.GetSingleProperty("Gaim:IPAddress");
			if (prop != null)
				this.IPAddress = (string) prop.Value;
			else
				this.IPAddress = "";
				
			prop = pList.GetSingleProperty("Gaim:IPPort");
			if (prop != null)
				this.IPPort = (string) prop.Value;
			else
				this.IPPort = "";
		}
	}
}
