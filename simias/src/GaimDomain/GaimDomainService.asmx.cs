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
			Simias.Gaim.GaimDomain gaimDomain = new Simias.Gaim.GaimDomain( false );
			Simias.Storage.Domain domain = gaimDomain.GetDomain( false, "" );
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

		internal Member FindBuddyInDomain(Simias.Gaim.GaimDomain gaimDomain, string accountName,
										  string accountProto, string buddyName)
		{
			Simias.Storage.Domain domain = gaimDomain.GetDomain(false, null);
			if (domain == null) {
				throw new SimiasException("Could not get Simias.Storage.Domain from GaimDomain!");
			}
		
			// Check to see if the buddy already exists
			Member member = null;
			string buddyMungedID = GetGaimMungedID(accountName, accountProto, buddyName);
			ICSList domainMembers = domain.GetNodesByName(buddyName);
			foreach (ShallowNode sNode in domainMembers)
			{
				Simias.Storage.Member aMember =	
					new Simias.Storage.Member(domain, sNode);
					
				Simias.Storage.PropertyList pList = aMember.Properties;
				Simias.Storage.Property p = pList.GetSingleProperty("Gaim:MungedID");
				if (p != null && ((string) p.Value) == buddyMungedID)
				{
					member = aMember;
					break;
				}
			}

			return member;			
		}
		
		internal string GetGaimMungedID(string accountName, string accountProto, string buddyName)
		{
			return accountName + ":" + accountProto + ":" + buddyName;
		}

		/// <summary>
		/// Gets the Simias Node ID of the Gaim Domain POBox
		/// </summary>
		[WebMethod(Description="GetGaimPOBoxID")]
		[SoapDocumentMethod]
		public string GetGaimPOBoxID()
		{
			Simias.Gaim.GaimDomain gaimDomain = new Simias.Gaim.GaimDomain(false);
			if (gaimDomain == null)
			{
				throw new SimiasException("Gaim Domain does not exist!");
			}

//			Simias.POBox.POBox poBox = gaimDomain.GetGaimPOBox();
			
//			if (poBox != null) {
//				if (poBox.ID == null) {
//					System.Console.WriteLine("poBox.ID is null");
//					return null;
//				} else {
//					System.Console.WriteLine("poBox.ID = {0}", poBox.ID);
//					return poBox.ID;
//				}
//			} else {
//				System.Console.WriteLine("poBox is null!");
//				return null;
//			}
			return null;
		}

		/// <summary>
		/// Adds a new Buddy to the Gaim Domain.  If the buddy already
		/// exists, any modified information will be updated in the domain.
		/// </summary>
		/// <param name="acountName">The Gaim account name (local user's screenname).</param>
		/// <param name="accountProto">The Gaim account protocol (i.e., prpl-oscar).</param>
		/// <param name="buddyName">The buddy's screenname</param>
		/// <param name="alias">The buddy's alias</param>
		/// <param name="ipAddr">IP Address of the Simias WebService running on the buddy's computer.</param>
		/// <param name="ipPort">IP Port of the Simias WebService running on the buddy's computer. </param>
		[WebMethod(Description="AddGaimBuddy")]
		[SoapDocumentMethod]
		public void AddGaimBuddy(string accountName, string accountProto, string buddyName,
								 string alias, string ipAddr, string ipPort)
		{
			Simias.Gaim.GaimDomain gaimDomain = new Simias.Gaim.GaimDomain(false);
			if (gaimDomain == null)
			{
				throw new SimiasException("Gaim Domain does not exist!");
			}
			
			Simias.Storage.Domain domain = gaimDomain.GetDomain(false, null);
			if (domain == null)
			{
				throw new SimiasException("Could not get Simias.Storage.Domain from GaimDomain!");
			}
			
			// Check to see if the buddy already exists
			Member member = FindBuddyInDomain(gaimDomain, accountName, accountProto, buddyName);
			if (member != null)
			{
				UpdateGaimBuddy(member, gaimDomain, alias, ipAddr, ipPort);
				return;
			}

			//
			// Create a new member and add on the properties
			//
			
			// Create the member
			member = new Member(buddyName, Guid.NewGuid().ToString(), Access.Rights.ReadWrite);
			
			// Gaim Munge ID (Account Name + Account Proto + Buddy Name) for faster lookups
			Simias.Storage.Property p = new Property("Gaim:MungedID",
													 GetGaimMungedID(accountName, accountProto, buddyName));
			p.LocalProperty = true;
			member.Properties.AddProperty(p);
			
			// Gaim Account Name
			p = new Property("Gaim:AccountName", accountName);
			p.LocalProperty = true;
			member.Properties.AddProperty(p);
			
			// Gaim Account Protocol
			p = new Property("Gaim:AccountProto", accountProto);
			p.LocalProperty = true;
			member.Properties.AddProperty(p);
			
			// Buddy Alias
			if (alias != null && alias.Length > 0)
			{
				p = new Property("Gaim:Alias", alias);
				p.LocalProperty = true;
				member.Properties.AddProperty(p);
			}
			
			// Buddy IP Address
			if (ipAddr != null && ipAddr.Length > 0)
			{
				p = new Property("Gaim:IPAddress", ipAddr);
				p.LocalProperty = true;
				member.Properties.AddProperty(p);
			}
			
			// Buddy IP Port
			if (ipPort != null && ipPort.Length > 0)
			{
				p = new Property("Gaim:IPPort", ipPort);
				p.LocalProperty = true;
				member.Properties.AddProperty(p);
			}
			
			// Commit the changes
			domain.Commit(member);
		}

		/// <summary>
		/// Removes a buddy from the Gaim Domain and from any
		/// collections the buddy is a member of.
		/// </summary>
		/// <param name="acountName">The Gaim account name (local user's screenname).</param>
		/// <param name="accountProto">The Gaim account protocol (i.e., prpl-oscar).</param>
		/// <param name="buddyName">The buddy's screenname</param>
		[WebMethod(Description="RemoveGaimBuddy")]
		[SoapDocumentMethod]
		public void RemoveGaimBuddy(string accountName, string accountProto, string buddyName)
		{
			Simias.Gaim.GaimDomain gaimDomain = new Simias.Gaim.GaimDomain(false);
			if (gaimDomain == null)
			{
				throw new SimiasException("Gaim Domain does not exist!");
			}
			
			Simias.Storage.Domain domain = gaimDomain.GetDomain(false, null);
			if (domain == null)
			{
				throw new SimiasException("Could not get Simias.Storage.Domain from GaimDomain!");
			}
			
			// Check to see if the buddy already exists
			Member member = FindBuddyInDomain(gaimDomain, accountName, accountProto, buddyName);
			if (member != null)
			{
				domain.Delete(member);
				domain.Commit();
			}
			else
			{
				throw new SimiasException("Did not find buddy");
			}
		}
		
		internal void UpdateGaimBuddy(Member member, Simias.Gaim.GaimDomain gaimDomain, string alias, string ipAddr, string ipPort)
		{
			Simias.Storage.PropertyList pList = member.Properties;
			Simias.Storage.Property p;
			
			Simias.Storage.Domain domain = gaimDomain.GetDomain(false, null);
			if (domain == null)
			{
				throw new SimiasException("Could not get Simias.Storage.Domain from GaimDomain!");
			}
			
			// Buddy Alias
			if (alias != null && alias.Length > 0)
			{
				if (pList.HasProperty("Gaim:Alias"))
				{
					pList.ModifyProperty("Gaim:Alias", alias);
				}
				else
				{
					p = new Property("Gaim:Alias", alias);
					p.LocalProperty = true;
					member.Properties.AddProperty(p);
				}
			}
			
			// Buddy IP Address
			if (ipAddr != null && ipAddr.Length > 0)
			{
				if (pList.HasProperty("Gaim:IPAddress"))
				{
					pList.ModifyProperty("Gaim:IPAddress", alias);
				}
				else
				{
					p = new Property("Gaim:IPAddress", ipAddr);
					p.LocalProperty = true;
					member.Properties.AddProperty(p);
				}
			}
			
			// Buddy IP Port
			if (ipPort != null && ipPort.Length > 0)
			{
				if (pList.HasProperty("Gaim:IPPort"))
				{
					pList.ModifyProperty("Gaim:IPPort", alias);
				}
				else
				{
					p = new Property("Gaim:IPPort", ipPort);
					p.LocalProperty = true;
					member.Properties.AddProperty(p);
				}
			}
			
			// Commit the changes
			domain.Commit(member);
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
		[WebMethod(Description="UpdateGaimBuddy")]
		[SoapDocumentMethod]
		public void UpdateGaimBuddy(string accountName, string accountProto, string buddyName,
									string alias, string ipAddr, string ipPort)
		{
			Simias.Gaim.GaimDomain gaimDomain = new Simias.Gaim.GaimDomain(false);
			if (gaimDomain == null)
			{
				throw new SimiasException("Gaim Domain does not exist!");
			}
			
			Member member = FindBuddyInDomain(gaimDomain, accountName, accountProto, buddyName);
			if (member != null)
			{
				UpdateGaimBuddy(member, gaimDomain, alias, ipAddr, ipPort);
				return;
			}
			else
			{
				throw new SimiasException("Did not find buddy");
			}
		}
		
		/// <summary>
		/// Get all the users in the Gaim Domain
		/// </summary>
		[WebMethod(Description="GetAllBuddies")]
		[SoapDocumentMethod]
		public GaimBuddy[] GetAllBuddies()
		{
			ArrayList buddies = new ArrayList();
			Simias.Gaim.GaimDomain gaimDomain = new Simias.Gaim.GaimDomain(false);
			if (gaimDomain == null)
			{
				throw new SimiasException("Gaim Domain does not exist!");
			}
			
			Simias.Storage.Domain domain = gaimDomain.GetDomain(false, null);
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
