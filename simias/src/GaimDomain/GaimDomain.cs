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
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Client;

namespace Simias.Gaim
{
	/// <summary>
	/// Class to initialize/verify a Gaim domain in the Collection Store
	/// </summary>
	public class GaimDomain
	{
		#region Class Members

		/// <summary>
		/// Well known ID for Gaim Workgroup Domain
		/// </summary>
		public static readonly string ID = "4a9ff9d6-8139-11d9-960e-000d936ac9c4";

		/// <summary>
		/// Friendly name for the workgroup domain.
		/// </summary>
		private string domainName = "Gaim Buddy List";
		private string hostAddress;
		private string description = "";
		private string hostName;
		private string userName;
		private string userID;
		private string poBoxID;
		private static string syncMethodPref = "all";
		private static bool bPruneOldMembers = false;

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Properties

		/// <summary>
		/// Gets the Gaim domain's friendly ID
		/// </summary>
		public string Name
		{
			get { return(this.domainName); }
		}

		/// <summary>
		/// Gets the Gaim domain's description
		/// </summary>
		public string Description
		{
			get { return(this.description); }
		}

		/// <summary>
		/// Gets the Gaim domain's host address
		/// </summary>
		public string Host
		{
			get { return(this.hostName); }
		}
		
		/// <summary>
		/// Gets the Gaim Domain's current user
		/// </summary>
		public string User
		{
			get { return( this.userName ); }
		}
		#endregion

		#region Constructors
		
		/// This domain so that it is NOT created if
		/// the user does not have Gaim installed/configured/etc.  The
		/// Gaim domain should only be created if the user has it installed
		/// and has a prpl-oscar account setup in .gaim/accounts.xml.
		/// Once this is verified, create the domain inside the Sync
		/// code using the prpl-oscar username as the domain owner.
		/// Build the Gaim Domain Member List from .gaim/blist.xml.
		///
		/// The Gaim Plugin needs to be modified so that it populates
		/// location information of the buddy into .gaim/blist.xml.  Then,
		/// when the LocationProvider is asked for location information
		/// about a buddy, the LocationProvider just parses blist.xml for
		/// the information.
		///
		/// The Gaim Plugin needs to be modified so that it is able to
		/// read AIM buddy profiles and search for an iFolder Plugin ID
		/// in an HTML comment.  A prerequisite is that the profile
		/// information should be set on the remote side to add in the
		/// hidden HTML comment.  If the buddy is determined to have the
		/// iFolder Plugin installed, then at buddy signon, we should
		/// send special messages to retrieve the buddy's POBoxURL and
		/// then store this as extra information in .gaim/blist.xml.
		
		/// <summary>
		/// Constructor for creating a new Gaim Domain object.
		/// </summary>
		private GaimDomain( bool init, string username )
		{
			hostName = Environment.MachineName;
			userName = username;

			domainName = "Gaim Buddy List (" + username + ")";
			description = "Workgroup Domain built from " + username + "'s Gaim Buddy List";

			if ( init == true )
			{
				this.Init();
			}
		}

		/// <summary>
		/// Constructor for creating a new Gaim Domain object.
		/// </summary>
		private GaimDomain( bool init, string username, string description ) 
		{
			hostName = Environment.MachineName;
			userName = username;
			this.description = description;

			if ( init == true )
			{
				this.Init();
			}
		}
		#endregion

		private void Init()
		{
			hostAddress = MyDns.GetHostName();
			Store store = Store.GetStore();

			try
			{
				Uri localUri = Manager.LocalServiceUrl;

				//
				// Verify the local Rendezvous user exists in the local database
				//
				LocalDatabase ldb = store.GetDatabaseObject();
				Member ldbMember;
				Node memberNode = ldb.GetSingleNodeByName( userName );
				if (memberNode == null)
				{
					// Create a local member which is the owner of the Gaim Domain
					ldbMember = new Member( userName, Guid.NewGuid().ToString(), Access.Rights.Admin );
					ldbMember.IsOwner = true;

					// Save the local database changes.
					ldb.Commit( new Node[] { ldbMember } );
				}
				else
				{
					ldbMember = new Member( memberNode );
				}

				userID = ldbMember.ID;

				//
				// Verify the Gaim workgroup domain exists
				//

				Simias.Storage.Domain rDomain = store.GetDomain( ID );
				if (rDomain == null)
				{
					// Create the Gaim Workgroup Domain
					rDomain = 
						new Simias.Storage.Domain(
							store, 
							this.domainName,
							Simias.Gaim.GaimDomain.ID,
							this.description, 
							Simias.Sync.SyncRoles.Master, 
							Simias.Storage.Domain.ConfigurationType.Workgroup );

					rDomain.SetType( rDomain, "GaimDomain" );
					rDomain.SetType( rDomain, "Workgroup" );

					// Create the owner member for the domain.
					Member member = 
						new Member(
							userName, 
							ldbMember.ID,
							Access.Rights.Admin );

					member.IsOwner = true;

					// Add on the SimiasURL so that the Location Provider can determine the
					// POBoxUrl for the Gaim Domain
					Simias.Storage.Property p = new Property("Gaim:SimiasURL", localUri.ToString());
					p.LocalProperty = true;
					member.Properties.AddProperty(p);

					rDomain.Commit( new Node[] { rDomain, member } );

					// Create the name mapping.
					store.AddDomainIdentity( rDomain.ID, member.UserID );

					GaimService.RegisterLocationProvider();
				}

				//
				// Verify the POBox for the local Gaim user
				//
			
				Member pMember;
				Simias.POBox.POBox poBox = null;
				string poBoxName = "POBox:" + Simias.Gaim.GaimDomain.ID + ":" + ldbMember.ID;

				try
				{
					poBox = Simias.POBox.POBox.FindPOBox( store, Simias.Gaim.GaimDomain.ID, ldbMember.ID );
				}
				catch{}
				if (poBox == null)
				{
					poBox = new Simias.POBox.POBox( store, poBoxName, ID );
					pMember = 
						new Member( ldbMember.Name, ldbMember.ID, Access.Rights.ReadWrite );
					pMember.IsOwner = true;
					poBox.Commit(new Node[] { poBox, pMember });
				}
				else
				{
					// verify member in POBox
					pMember = poBox.GetMemberByID( ldbMember.ID );
					if (pMember == null)
					{
						pMember = 
							new Member( ldbMember.Name, ldbMember.ID, Access.Rights.ReadWrite );
						pMember.IsOwner = true;
						poBox.Commit(new Node[] { pMember });
					}
				}

				poBoxID = poBox.ID;
			}
			catch( Exception e1 )
			{
				log.Error(e1.Message);
				log.Error(e1.StackTrace);

				throw e1;
				// FIXME:: rethrow the exception
			}			
		}

		#region Public Methods

		/// <summary>
		/// Checks if the local/master Gaim Domain exists.
		/// </summary>
		/// <returns>true if the domain exists otherwise false</returns>
		public bool Exists()
		{
			bool exists = false;
			Simias.Storage.Domain gaimDomain = null;

			try
			{
				Store store = Store.GetStore();
				gaimDomain = store.GetDomain( ID );
				if ( gaimDomain != null )
				{
					userID = gaimDomain.GetMemberByName( userName ).ID;
					Simias.POBox.POBox pobox = 
						Simias.POBox.POBox.FindPOBox( store, ID, userID );
					poBoxID = pobox.ID;
					exists = true;
				}
			}
			catch{}
			return exists;
		}

		/// <summary>
		/// Method to get the Gaim Domain
		/// </summary>
		public static Simias.Storage.Domain GetDomain()
		{
			//
			//  Check if the Gaim Domain exists in the store
			//
			Simias.Storage.Domain gaimDomain = null;

			try
			{
				Store store = Store.GetStore();
				gaimDomain = store.GetDomain(ID);
				if (gaimDomain != null)
				{
					return gaimDomain;
				}
			}
			catch(Exception ggd)
			{
				log.Error( ggd.Message );
				log.Error( ggd.StackTrace );
			}

			return gaimDomain;
		}

		/// <summary>
		/// This function will read the ~/.gaim/prefs.xml and update any
		/// preferences that have changed so the Synchronize Members function
		/// will behave accordingly.
		/// </summary>
		public static void UpdatePreferences()
		{
			XmlDocument prefsDoc = new XmlDocument();
			try
			{
				prefsDoc.Load(GetGaimConfigDir() + "/prefs.xml");
			}
			catch (Exception e)
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
				return;
			}
			XmlElement topPrefElement = prefsDoc.DocumentElement;

			//
			// Sync Interval
			//
			XmlNode syncIntervalNode = 
				topPrefElement.SelectSingleNode("//pref[@name='plugins']/pref[@name='simias']/pref[@name='sync_interval']/@value");

			if (syncIntervalNode != null)
			{
				string syncIntervalString = syncIntervalNode.Value;
				if (syncIntervalString != null)
				{
					int syncInterval = Int32.Parse(syncIntervalString);
					
					Simias.Gaim.Sync.UpdateSyncInterval(syncInterval);
				}
			}
			
			//
			// Type of member sync
			//
			XmlNode syncMethodNode =
				topPrefElement.SelectSingleNode("//pref[@name='plugins']/pref[@name='simias']/pref[@name='sync_method']/@value");

			if (syncMethodNode != null)
			{
				string syncMethod = syncMethodNode.Value;
				if (syncMethod != null && syncMethod != syncMethodPref)
				{
					syncMethodPref = syncMethod;
				}
			}
			
			//
			// Prune old members
			//
			XmlNode pruneMembersNode =
				topPrefElement.SelectSingleNode("//pref[@name='plugins']/pref[@name='simias']/pref[@name='prune_members']/@value");

			if (pruneMembersNode != null)
			{
				string pruneMembers = pruneMembersNode.Value;
				if (pruneMembers != null)
				{
					if (pruneMembers == "1")
					{
						bPruneOldMembers = true;
					}
					else
					{
						bPruneOldMembers = false;
					}
				}
			}
			
		}
		
		/// <summary>
		/// This function is called each time a sync interval is called.  If a
		/// user does not have Gaim installed on their computer, a Gaim Domain
		/// will not be created in Simias.  The same is true if the DO have
		/// Gaim installed, but not the Gaim iFolder Plugin installed/enabled.
		/// Since SychronizeMembers() is called periodically, it will continue
		/// to check for Gaim + iFolder Plugin and create the Domain when it
		/// finds those two things in existence.
		/// </summary>
		public static void SynchronizeMembers()
		{
			log.Debug("SynchronizeMembers() called");
			
			if (!IsGaimPluginEnabled())
			{
				log.Debug("Gaim Plugin is not installed/enabled");
				return;
			}
			
			Simias.Storage.Domain domain = GetDomain();
			if (domain == null)
			{
				// Time to create the Gaim Domain
				GaimAccount gaimAccount = GetDefaultGaimAccount();
				if (gaimAccount == null)
				{
					log.Debug("No default (AIM/prpl-oscar) Gaim Account");
					return;
				}
				
				Simias.Gaim.GaimDomain gaimDomain =
					new Simias.Gaim.GaimDomain(true, gaimAccount.Name);

				// Try again to get the domain
				domain = GetDomain();
			}
			
			if (domain == null) return; // The domain must not be ready yet
			
			// Sync all buddies into the domain
			SyncBuddies(domain);
		}

		/// <summary>
		/// Obtains the string representation of this instance.
		/// </summary>
		/// <returns>The friendly name of the domain.</returns>
		public override string ToString()
		{
			return this.domainName;
		}
		#endregion
		
		#region More Internal Methods

		/// <summary>
		/// This function is responsible for returning the path to the Gaim
		/// Configuration directory.  In Windows, this is typically:
		///
		///     C:\Documents and Settings\<username>\Application Data\.gaim
		///
		/// In Linux/Mac this is usually:
		///
		///    ~/.gaim
		/// </summary>
		internal static string GetGaimConfigDir()
		{
			string gaimConfigDir = "";
			if (MyEnvironment.Windows)
			{
				string userProfileDir = Environment.GetEnvironmentVariable("USERPROFILE");
				gaimConfigDir = string.Format("{0}\\Application Data\\.gaim", userProfileDir);
			}
			else
			{
				string userHomeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				gaimConfigDir = string.Format("{0}/.gaim", userHomeDir);
			}

			return gaimConfigDir;
		}
		
		/// <summary>
		/// This function will return true only if the following conditions are
		/// met:
		///     (1) Gaim is installed
		///     (2) The Gaim iFolder Plugin is installed
		/// </summary>
		internal static bool IsGaimPluginEnabled()
		{
			XmlDocument prefsDoc = new XmlDocument();
			try
			{
				prefsDoc.Load(GetGaimConfigDir() + "/prefs.xml");
			}
			catch (Exception e)
			{
				// Don't cause any errors to log...for the case where Gaim isn't installed or the plugin isn't installed/enabled
				return false;
			}
			XmlElement topPrefElement = prefsDoc.DocumentElement;

			XmlNodeList loadedPlugins = 
				topPrefElement.SelectNodes("//pref[@name='gaim']/pref[@name='gtk']/pref[@name='plugins']/pref[@name='loaded']/item");

			if (loadedPlugins != null)
			{
				// Look through the list to find "gifolder"
				foreach (XmlNode loadedPlugin in loadedPlugins)
				{
					XmlAttribute attr = loadedPlugin.Attributes["value"];
					if (attr != null)
					{
						string value = attr.Value;
						if (value != null)
						{
							if (value.IndexOf("gifolder") > 0)
							{
								return true;
							}
						}
					}
				}
			}		
		
			return false;
		}
		
		internal static GaimAccount GetDefaultGaimAccount()
		{
			GaimAccount[] accounts = GetGaimAccounts();
			if (accounts == null) return null;
			
			// Loop through the array until the first AIM/prpl-oscar account
			// is found (the one we'll call our default)
			
			for (int i = 0; i < accounts.Length; i++)
			{
				string acctProto = accounts[i].ProtocolID;
				if (acctProto != null && acctProto == "prpl-oscar")
				{
					return accounts[i];
				}
			}
			
			return null;
		}
		
		internal static GaimAccount[] GetGaimAccounts()
		{
			ArrayList accounts = new ArrayList();
			XmlDocument accountsDoc = new XmlDocument();
			try
			{
				accountsDoc.Load(GetGaimConfigDir() + "/accounts.xml");
			}
			catch (Exception e)
			{
				log.Error(e.Message);
				log.Error(e.StackTrace);
				return (GaimAccount[])accounts.ToArray(typeof(Simias.Gaim.GaimAccount));
			}
			XmlElement accountsElement = accountsDoc.DocumentElement;
			
			XmlNodeList accountNodes =
				accountsElement.SelectNodes("//account");
			if (accountNodes == null)
				return (GaimAccount[])accounts.ToArray(typeof(Simias.Gaim.GaimAccount));
			
			foreach (XmlNode accountNode in accountNodes)
			{
				try
				{
					GaimAccount account = new GaimAccount(accountNode);
					accounts.Add(account);
				}
				catch (Exception e)
				{
					log.Error( e.Message );
					log.Error( e.StackTrace );
				}
			}
			
			return (GaimAccount[])accounts.ToArray(typeof(Simias.Gaim.GaimAccount));
		}
		
		internal static GaimBuddy[] GetBuddies(bool iFolderEnabledOnly)
		{
			ArrayList buddies = new ArrayList();
			XmlDocument blistDoc = new XmlDocument();
			try
			{
				blistDoc.Load(GetGaimConfigDir() + "/blist.xml");
			}
			catch (Exception e)
			{
				log.Error(e.Message);
				log.Error(e.StackTrace);
				return (GaimBuddy[])buddies.ToArray(typeof(Simias.Gaim.GaimBuddy));
			}
			XmlElement gaimElement = blistDoc.DocumentElement;
			
			XmlNodeList buddyNodes = null;
			if (iFolderEnabledOnly)
			{
				buddyNodes = gaimElement.SelectNodes("//buddy[setting[@name='simias-url']]");
			}
			else
			{
				buddyNodes = gaimElement.SelectNodes("//buddy");
			}
			if (buddyNodes == null)
				return (GaimBuddy[])buddies.ToArray(typeof(Simias.Gaim.GaimBuddy));
			
			foreach (XmlNode buddyNode in buddyNodes)
			{
				try
				{
					GaimBuddy buddy = new GaimBuddy(buddyNode);
					buddies.Add(buddy);
				}
				catch (Exception e)
				{
					// Ignore errors (i.e., spare the log file)
				}
			}
			
			return (GaimBuddy[])buddies.ToArray(typeof(Simias.Gaim.GaimBuddy));
		}
		
		internal static Member FindBuddyInDomain(Simias.Storage.Domain domain, GaimBuddy buddy)
		{
			// Check to see if the buddy already exists
			Member member = null;
			ICSList domainMembers = domain.GetNodesByName(buddy.Name);
			foreach (ShallowNode sNode in domainMembers)
			{
				Simias.Storage.Member aMember =	
					new Simias.Storage.Member(domain, sNode);
					
				Simias.Storage.PropertyList pList = aMember.Properties;
				Simias.Storage.Property p = pList.GetSingleProperty("Gaim:MungedID");
				if (p != null && ((string) p.Value) == buddy.MungedID)
				{
					member = aMember;
					break;
				}
			}

			return member;			
		}
		
		internal static void SyncBuddies(Simias.Storage.Domain domain)
		{
			GaimBuddy[] buddies;
			if (syncMethodPref == "plugin-enabled")
			{
				log.Debug("Synching only iFolder Plugin-enabled Buddies");

				// Only sync buddies that the Gaim iFolder Plugin has been able
				// to determine have the Gaim iFolder Plugin enabled
				buddies = GetBuddies(true);
			}
			else
			{
				log.Debug("Synching ALL Gaim Buddies");

				// Sync all buddies regardless of whether they have the Gaim
				// iFolder Plugin installed
				buddies = GetBuddies(false);
			}
			
			if (buddies == null) return;

			foreach (GaimBuddy buddy in buddies)
			{
				Member member =
					FindBuddyInDomain(domain, buddy);
				
				if (member == null)
				{
					CreateNewMember(domain, buddy);
				}
				else
				{
					UpdateMember(domain, member, buddy);
				}
			}

			if (bPruneOldMembers)
			{
				PruneOldMembers(domain, buddies);
			}
		}
		
		internal static void CreateNewMember(Simias.Storage.Domain domain, GaimBuddy buddy)
		{
			//
			// Create a new member and add on the properties
			//
			
			// Create the member
			Member member = new Member(buddy.Name, Guid.NewGuid().ToString(), Access.Rights.ReadWrite);
			
			// Gaim Munge ID (Account Name + Account Proto + Buddy Name) for faster lookups
			Simias.Storage.Property p =
				new Property("Gaim:MungedID", buddy.MungedID);
			p.LocalProperty = true;
			member.Properties.AddProperty(p);
			
			// Gaim Account Name
			p = new Property("Gaim:AccountName", buddy.AccountName);
			p.LocalProperty = true;
			member.Properties.AddProperty(p);
			
			// Gaim Account Protocol
			p = new Property("Gaim:AccountProto", buddy.AccountProtocolID);
			p.LocalProperty = true;
			member.Properties.AddProperty(p);
			
			if (buddy.Alias != null)
			{
				p = new Property("Gaim:Alias", buddy.Alias);
				p.LocalProperty = true;
				member.Properties.AddProperty(p);
				
				// Use the buddy alias for the member full name
				member.FN = buddy.Alias;

				string familyName = null;
				string givenName = null;
				int lastSpacePos = buddy.Alias.LastIndexOf(' ');
				if (lastSpacePos > 0)
				{
					familyName = buddy.Alias.Substring(lastSpacePos + 1);
					member.Family = familyName;

					givenName = buddy.Alias.Substring(0, lastSpacePos);
				}
				else
				{
					givenName = buddy.Alias;
				}
					
				member.Given = givenName;
			}
			
			// Buddy Simias URL
			if (buddy.SimiasURL != null)
			{
				p = new Property("Gaim:SimiasURL", buddy.SimiasURL);
				p.LocalProperty = true;
				member.Properties.AddProperty(p);
			}
			
			// Commit the changes
			domain.Commit(member);
		}

		public static void UpdateMember(string AccountName, string AccountProtocolID, string BuddyName)
		{
			Simias.Storage.Domain domain = GaimDomain.GetDomain();
			if (domain == null) return;
		
			XmlDocument blistDoc = new XmlDocument();
			try
			{
				blistDoc.Load(GetGaimConfigDir() + "/blist.xml");
			}
			catch (Exception e)
			{
				log.Error(e.Message);
				log.Error(e.StackTrace);
				return;
			}
			XmlElement gaimElement = blistDoc.DocumentElement;
			
			string xPathQuery =
				string.Format("//buddy[@account='{0}' and @proto='{1}' and name='{2}' and setting[@name='simias-url']]",
							  AccountName, AccountProtocolID, BuddyName);
			XmlNode buddyNode = gaimElement.SelectSingleNode(xPathQuery);
			if (buddyNode != null)
			{
				try
				{
					GaimBuddy buddy = new GaimBuddy(buddyNode);
					Member member =
						FindBuddyInDomain(domain, buddy);
					
					if (member == null)
					{
						// This shouldn't happen, but just in case...
						CreateNewMember(domain, buddy);
					}
					else
					{
						UpdateMember(domain, member, buddy);
					}
				}
				catch (Exception e)
				{
					// Intentionally left blank
				}				
			}
		}

		internal static void UpdateMember(Simias.Storage.Domain domain, Member member, GaimBuddy buddy)
		{
			Simias.Storage.PropertyList pList = member.Properties;
			Simias.Storage.Property p;
			
			// Buddy Alias
			if (buddy.Alias != null && buddy.Alias.Length > 0)
			{
				if (pList.HasProperty("Gaim:Alias"))
				{
					pList.ModifyProperty("Gaim:Alias", buddy.Alias);
				}
				else
				{
					p = new Property("Gaim:Alias", buddy.Alias);
					p.LocalProperty = true;
					member.Properties.AddProperty(p);
				
					// Use the buddy alias for the member full name
					member.FN = buddy.Alias;
					
					string familyName = null;
					string givenName = null;
					int lastSpacePos = buddy.Alias.LastIndexOf(' ');
					if (lastSpacePos > 0)
					{
						familyName = buddy.Alias.Substring(lastSpacePos + 1);
						member.Family = familyName;

						givenName = buddy.Alias.Substring(0, lastSpacePos);
					}
					else
					{
						givenName = buddy.Alias;
					}
					
					member.Given = givenName;
				}
			}
			
			// Buddy Simias URL
			if (buddy.SimiasURL != null && buddy.SimiasURL.Length > 0)
			{
				if (pList.HasProperty("Gaim:SimiasURL"))
				{
					pList.ModifyProperty("Gaim:SimiasURL", buddy.SimiasURL);
				}
				else
				{
					p = new Property("Gaim:SimiasURL", buddy.SimiasURL);
					p.LocalProperty = true;
					member.Properties.AddProperty(p);
				}
			}
			
			// Commit the changes
			domain.Commit(member);
		}
		
		internal static bool IsMemberInBuddyList(Member member, GaimBuddy[] buddies)
		{
			foreach (GaimBuddy buddy in buddies)
			{
				Simias.Storage.PropertyList pList = member.Properties;
				Simias.Storage.Property p = pList.GetSingleProperty("Gaim:MungedID");
				if (p != null && ((string) p.Value) == buddy.MungedID)
				{
					return true;
				}
			}
			
			return false;
		}
		
		/// <summary>
		/// This method will remove any members of the domain who are no longer
		/// listed in the Gaim Buddy List
		/// </summary>
		internal static void PruneOldMembers(Simias.Storage.Domain domain, GaimBuddy[] buddies)
		{
			// Remove members from the domain if they are not in the list of buddies
			ICSList memberList = domain.GetMemberList();
			foreach(ShallowNode sNode in memberList)
			{
				// Get the member from the list
				Simias.Storage.Member member =
					new Simias.Storage.Member(domain, sNode);

				if (member.IsOwner) continue; // Don't nuke the owner

				if (!IsMemberInBuddyList(member, buddies))
				{
					domain.Commit(domain.Delete(member));
				}
			}
		}
		
		#endregion
	}
}
