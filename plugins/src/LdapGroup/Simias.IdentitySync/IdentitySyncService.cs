/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Mahabaleshwar Asundi <amahabaleshwar@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Web;
using Novell.Directory.Ldap;

using Simias.Client;
using Simias.Storage;

namespace Simias.IdentitySynchronization
{
	/// <summary>
	/// The status of a member during a given synchronization cycle.
	/// </summary>
	public enum MemberStatus
	{
		/// <summary>
		/// The member was created.
		/// </summary>
		Created,

		/// <summary>
		/// The member was updated.
		/// </summary>
		Updated,

		/// <summary>
		/// The member was not changed.
		/// </summary>
		Unchanged,

		/// <summary>
		/// The member was disabled.
		/// </summary>
		Disabled,

		/// <summary>
		/// The member was deleted.
		/// </summary>
		Deleted
	}
	
	///
	/// <summary>
	/// Class used to maintain state across an external identity
	/// synchronization cycle.
	/// </summary>
	///
	public class State
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log =
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		internal Property syncGuid;
		private Store store;
		private int disabled;
		private int deleted;
		private int created;
		private int updated;
		private int reportedErrors;
		private ArrayList syncMessages;
		private int processed;
		private DateTime endTime;
		private DateTime startTime;
		private Domain domain;
		
                /// <summary>
                /// Group Quota Restriction Method.
                /// </summary>
                private enum QuotaRestriction
                {
                                // For current Implementation, enum value AllAdmins is not used, can be used in future
                                UI_Based,
                                Sync_Based
                };


		#endregion
		
		#region Properties
		/// <summary>
		/// Gets the Domain object
		/// </summary>
		public Domain SDomain
		{
			get { return domain; }
		}

		/// <summary>
		/// Gets the number of objects that were processed.
		/// </summary>
		public int Processed
		{
			get { return processed; }
		}

		/// <summary>
		/// Gets the number of errors reported.
		/// </summary>
		public int Errors
		{
			get { return reportedErrors; }
		}

		/// <summary>
		/// Gets an array of synchronization messages.
		/// </summary>
		public string[] Messages
		{
			get
			{
				if ( syncMessages.Count == 0 )
				{
					return null;
				}
				
				return syncMessages.ToArray( typeof( string ) ) as string[];
			}
		}

		/// <summary>
		/// Gets the number of objects that were created.
		/// </summary>
		public int Created
		{
			get { return created; }
		}

		/// <summary>
		/// Gets the number of objects that were updated.
		/// </summary>
		public int Updated
		{
			get { return updated; }
		}

		/// <summary>
		/// Gets the number of objects that were deleted.
		/// </summary>
		public int Deleted
		{
			get { return deleted; }
		}

		/// <summary>
		/// Gets the number of objects that were disabled.
		/// </summary>
		public int Disabled
		{
			get { return disabled; }
		}

		/// <summary>
		/// Gets the time that the synchronization cycle started.
		/// </summary>
		public DateTime StartTime
		{
			get { return startTime; }
		}

		/// <summary>
		/// Gets the time that the synchronization cycle ended.
		/// </summary>
		public DateTime EndTime
		{
			get { return endTime; }
			set { endTime = value; }
		}

		/// <summary>
		/// Gets the GUID associated with the current synchronization cycle.
		/// </summary>
		public Property SyncGuid
		{
			get { return syncGuid; }
		}
		#endregion
		
		#region Constructors
		/// <summary>
		/// Constructs a State object.
		/// </summary>
		/// <param name="DomainID">The identifier of the domain to synchronize.</param>
		public State( string DomainID )
		{
			store = Store.GetStore();
			domain = store.GetDomain( DomainID );
			if ( domain == null )
			{
				throw new ArgumentException( "DomainID" );
			}
			
			syncGuid = new Property( "SyncGuid", Guid.NewGuid().ToString() );
			syncGuid.LocalProperty = true;
			
			syncMessages = new ArrayList();
			reportedErrors = 0;
			processed = 0;
			
			startTime = DateTime.Now;
		}
		#endregion

		#region Private Methods
        /// <summary>
        /// Checks if properties match
        /// </summary>
        /// <param name="One">First property</param>
        /// <param name="Two">Second property</param>
        /// <returns>true if they match</returns>
		private bool PropertiesEqual( Property One, Property Two )
		{
			if ( One.Type == Two.Type )
			{
				switch ( One.Type )
				{
					case Simias.Storage.Syntax.String:
					{
						if ( One.Value as string == Two.Value as string )
						{
							return true;
						}
						break;
					}
					
					case Simias.Storage.Syntax.Boolean:
					{
						if ( (bool) One.Value == (bool) Two.Value )
						{
							return true;
						}
						break;
					}
					
					case Simias.Storage.Syntax.Byte:
					{
						if ( (byte) One.Value == (byte) Two.Value )
						{
							return true;
						}
						break;
					}
					
					case Simias.Storage.Syntax.Char:
					{
						if ( (char) One.Value == (char) Two.Value )
						{
							return true;
						}
						break;
					}
					
					case Simias.Storage.Syntax.DateTime:
					{
						if ( (DateTime) One.Value == (DateTime) Two.Value )
						{
							return true;
						}
						break;
					}
					
					case Simias.Storage.Syntax.Int16:
					{
						if ( (short) One.Value == (short) Two.Value )
						{
							return true;
						}
						break;
					}
					
					case Simias.Storage.Syntax.Int32:
					{
						if ( (System.Int32) One.Value == (System.Int32) Two.Value )
						{
							return true;
						}
						break;
					}
					
					case Simias.Storage.Syntax.Int64:
					{
						if ( (System.Int64) One.Value == (System.Int64) Two.Value )
						{
							return true;
						}
						break;
					}
					
					case Simias.Storage.Syntax.UInt16:
					{
						if ( (ushort) One.Value == (ushort) Two.Value )
						{
							return true;
						}
						break;
					}
					
					case Simias.Storage.Syntax.UInt32:
					{
						if ( (System.UInt32) One.Value == (System.UInt32) Two.Value )
						{
							return true;
						}
						break;
					}
					
					case Simias.Storage.Syntax.UInt64:
					{
						if ( (System.UInt64) One.Value == (System.UInt64) Two.Value )
						{
							return true;
						}
						break;
					}
				}
			}
			
			return false;
		}
		#endregion
		
		#region Public Methods
		/// <summary>
		/// External sync providers must call this method after
		/// retrieving member information from the external identity store.
		/// Username is the distinguishing property validated against the
		/// domain.
		/// </summary>
		public void ProcessMember(
			string UserGuid,
			string Username,
			string Given,
			string Last,
			string FN,
			string DN,
			Property[] Properties,
			bool Group,
			string groupmembers,
			string groupmembership,
			string iFolderHomeServer)
		{
			log.Debug( "Processing member: " + Username + " , " + groupmembers + " , " + groupmembership + ".");
			Simias.Storage.Member member = null;
			MemberStatus status = MemberStatus.Unchanged;
			bool UserRenamed = false;

			if(Username == null || Username.Equals(String.Empty))
			{
				log.Debug("Empty Username, canot create blank user in iFolder, returning");
				return;
			}			

			try
			{
				member = domain.GetMemberByName( Username );
				if (member == null && !String.IsNullOrEmpty(UserGuid)){
					//Note: In case of renamed users teh user name will be different and the GUID will be same.
					//Search if the user already exist with the GUID. 
					member = domain.GetMemberByID(UserGuid);
					if (member != null){
						//TBD:iFolder needs to handle the renamed users properly.
						log.Info("This user is renamed on the LDAP server. Old Username = {0}, new Username= {1}", member.Name, Username);
						UserRenamed = true;
					}
				}
			}
			catch{}

			if ( member != null )
			{
				bool timeStampPresent = false;
				string CurrentDn = null;
				try
				{
					CurrentDn = member.Properties.GetSingleProperty( "DN" ).Value as string;
				}
				catch{}

				if(UserGuid != null && String.Compare(member.UserID, UserGuid) != 0)
					if(CurrentDn != null && DN != null && String.Compare(CurrentDn.ToLower(), DN.ToLower()) != 0)
						return;
					
				if ( Properties != null )
				{
					// check if the properties provided by the identity sync
					// provider have changed the member object
					// The LdapTimeStamp attribute is passed in the Properties array,
					// so checking this first will allow us to tell if the user object
					// has changed.
					foreach( Property prop in Properties )
					{
						if ( !timeStampPresent && prop.Name.Equals( "LdapTimeStamp" ) )
						{
							timeStampPresent = true;
						}

						bool propChanged = false;
						Property tmp = member.Properties.GetSingleProperty( prop.Name );
						if ( tmp == null )
						{
							propChanged = true;
						}
						else if ( tmp.MultiValuedProperty == true || 
							prop.MultiValuedProperty == true )
						{
							propChanged = true;
						}
						else if ( PropertiesEqual( tmp, prop ) == false )
						{
							propChanged = true;
						}
					
						if ( propChanged == true )
						{
							log.Debug( "Property: {0} has changed", prop.Name );
							member.Properties.ModifyProperty( prop );
							status = MemberStatus.Updated;
						}
						if ( FN != null && FN != "" && FN != member.FN )
						{
							// This is special scenario where withour time stamp change, FN can be changed
							status = MemberStatus.Updated;
						}
					}
				}

				// If the timestamp is not present or it has changed, then check/update 
				// the rest of the attributes.
				if ( !timeStampPresent || ( status != MemberStatus.Unchanged ) )
				{
					//
					// Not sure if I modify a property with the same
					// value that already exists will force a node
					// update and consequently a synchronization so I'll
					// check just to be sure.
					//

					// First name change?
					if ( Given != null && Given != "" && Given != member.Given )
					{
						log.Debug( "Property: {0} has changed", "Given" );
						member.Given = Given;
						status = MemberStatus.Updated;
					}
				
					// Last name change?
					if ( Last != null && Last != "" && Last != member.Family )
					{
						log.Debug( "Property: {0} has changed", "Family" );
						member.Family = Last;
						status = MemberStatus.Updated;
					}
				
					if ( FN != null && FN != "" && FN != member.FN )
					{
						log.Debug( "Property: {0} has changed", "FN" );
						member.FN = FN;
						status = MemberStatus.Updated;
					}
				
					string dn = null;

					try
					{
						dn = member.Properties.GetSingleProperty( "DN" ).Value as string;
					}
					catch{}
					Property dnProp = new Property( "DN", DN );
                                        //Commenting this line as this is required in client as well.
                                        //we construct user objects based on DN value, that this fails without this
					//dnProp.ServerOnlyProperty = true;
					if ( DN != null && DN != "" && DN != dn )
					{
						log.Debug( "Property: {0} has changed", "DN" );
						member.Properties.ModifyProperty( dnProp );
						status = MemberStatus.Updated;
					}
				}
				if(Group == true)
				{
					string oldMemberList = String.Empty;
					MultiValuedList mvl = member.Properties.GetProperties( "MembersList" );
					if( mvl != null && mvl.Count > 0)
					{
						foreach( Property p in mvl )
						{
							if( p != null)
							{
								oldMemberList += p.Value as string;
								oldMemberList += ";";
							}
						}
					}
					member.Properties.DeleteProperties("MembersList");

					string[] newMemberArray = groupmembers.Split(new char[] { ';' });
					foreach(string newMember in newMemberArray)
					{
						if(newMember != null && newMember != String.Empty && newMember != "" )
						{
							Property memberinGroup = new Property( "MembersList", newMember);
							memberinGroup.ServerOnlyProperty = true;
                                        		member.Properties.AddNodeProperty( memberinGroup );
						}
					}

					UpdateMemberList(oldMemberList,groupmembers,DN);
				}
				{
					string oldGroupList = String.Empty;
					try
					{
						oldGroupList = member.Properties.GetSingleProperty( "UserGroups" ).Value as string;
					}
					catch{}
					string updatedGroupList = oldGroupList;
					string tmpGroup = oldGroupList; 
					if(tmpGroup !=  null && tmpGroup.IndexOf(groupmembership) < 0)
					{
						updatedGroupList += groupmembership;
						Property userGroups = new Property( "UserGroups", updatedGroupList );
						userGroups.ServerOnlyProperty = true;
						member.Properties.ModifyProperty( userGroups );
					}
				}
				try
				{
					string ldapHomeStr = member.Properties.GetSingleProperty( "LdapHomeAttribute" ).Value as string;
					if(ldapHomeStr != null && ldapHomeStr != iFolderHomeServer )
					{
						Property ldapHome = new Property( "LdapHomeAttribute", iFolderHomeServer);
						ldapHome.ServerOnlyProperty = true;
						member.Properties.ModifyProperty( ldapHome );
					}
				}
				catch(Exception)
				{
					Property ldapHome = new Property( "LdapHomeAttribute", iFolderHomeServer);
					ldapHome.ServerOnlyProperty = true;
					member.Properties.ModifyProperty( ldapHome );
				}
			}
			else
			{
				// Couldn't find the member in the domain
				// so create it.
				
				string guid = 
					( UserGuid != null && UserGuid != "" )
						? UserGuid : Guid.NewGuid().ToString();
				try
				{
					member = new
						Member(
							Username,
							guid,
							Simias.Storage.Access.Rights.ReadOnly,
							Given,
							Last );

					member.FN = FN;
					
					Property dn = new Property( "DN", DN );
                                        //Commenting this line as this is required in client as well.
                                        //we construct user objects based on DN value, that this fails without this
					//dn.ServerOnlyProperty = true;
					member.Properties.ModifyProperty( dn );
					if(Group == true)
					{
						string[] newMemberArray = groupmembers.Split(new char[] { ';' });
						foreach(string newMember in newMemberArray)
                                        	{
                                               	 	if(newMember != null && newMember != String.Empty && newMember != "" )
                                                	{
                                 	                        Property memberinGroup = new Property( "MembersList", newMember);
                                        	                memberinGroup.ServerOnlyProperty = true;
                                                	        member.Properties.AddNodeProperty( memberinGroup );
                                                	}
                                        	}

						Property groupType = new Property( "GroupType", "Global" );
						groupType.ServerOnlyProperty = true;
						member.Properties.ModifyProperty( groupType );
					}
					Property userGroups = new Property( "UserGroups", groupmembership );
					userGroups.ServerOnlyProperty = true;
					member.Properties.ModifyProperty( userGroups );
					Property ldapHome = new Property( "LdapHomeAttribute", iFolderHomeServer);
					ldapHome.ServerOnlyProperty = true;
					member.Properties.ModifyProperty( ldapHome );
					
					// commit all properties passed in from
					// the provider
					foreach( Property prop in Properties )
					{
						member.Properties.ModifyProperty( prop );
					}
			
					// while creating member, if sum of allocated disk quota per user is equal to aggregate disk quota set on group
					// then set 0 MB as disk quota limit for this user
					{
						string [] GroupIDs = domain.GetMemberFamilyList(member.UserID);
						foreach(string GroupID in GroupIDs)
						{
							bool IsChildGroup = false;
							Member GroupAsMember = domain.GetMemberByID(GroupID);
                                                        string ParentGroups = null;
							Property p = GroupAsMember.Properties.GetSingleProperty( "UserGroups" );
							if(p != null)
								ParentGroups = p.Value as string;

							if(ParentGroups != null && ParentGroups != String.Empty)
							{
								// no point in 
								IsChildGroup = true;
							}
							long GroupDiskQuota = GroupAsMember.AggregateDiskQuota;
							if(GroupDiskQuota < 0)
							{
									
								if( IsChildGroup )
									continue;
								else
								{
									Simias.Policy.DiskSpaceQuota.Set(member, -1);
									log.Debug("MEMBERADD: No aggregate Disk Quota set for group, so it will set unlimited space for newly added member");
									break;
								}

							}
							if( domain.GroupQuotaRestrictionMethod == (int)QuotaRestriction.UI_Based )
							{
								// add the allocated disk quota per user for whole group
								long SizeAllocatedToMembers = 0;
								string [] MembersList = domain.GetGroupsMemberList( GroupID );
								foreach( string GroupMemberID in MembersList)
								{
									long MemberAllocation = Simias.Policy.DiskSpaceQuota.Get( domain.GetMemberByID(GroupMemberID)).Limit;
									if( MemberAllocation >= 0 )
										SizeAllocatedToMembers += MemberAllocation;
								}
								if( SizeAllocatedToMembers >= GroupDiskQuota )
								{
									log.Debug("MEMBERADD: Sum of Disk Quota per user for all users is equal to aggregate disk quota set for group, so committing 0 MB for newly added user");
									Simias.Policy.DiskSpaceQuota.Set(member, 0);	
									break;
								}
								
							}
						}
					}
					
					status = MemberStatus.Created;
				}
				catch( Exception ex )
				{
					log.Debug("Failed Creating member: "+Username + ex.Message);
					this.ReportError( "Failed creating member: " + Username + ex.Message );
					return;
				}
			}

			if( UserRenamed == true )
			{
				// First store the current username to provide grace login from thick clients/web-access. Since thick clients will be using
				// older usernames, so they must continue login and then get the new username. provide 30 days such grace logins
				string counter = DateTime.Now.Ticks.ToString();
				string UserNames = null;
				string CountAndDN = member.OldDN;
				if( CountAndDN != null)
				{
					//consider older UserNames also and add new one.
					string[] Elements = CountAndDN.Split(new char[] { ':' });
					if ( Elements != null && Elements.Length > 1 )
					{
						UserNames = Elements[2];
						string [] names = UserNames.Split( new char[] { ';' } );
						if( Array.IndexOf( names, Username ) < 0 )
						{		
							// take care that new username is not same as any of older ones
							UserNames += ( ";" + member.Name );
						}
					}
					else UserNames = ";" + member.Name;
				}
				else UserNames = ";" + member.Name;
				member.OldDN = counter + ":" + Username + ":" + UserNames ;

				member.Name = Username;
				log.Debug(" changed the username property with new value :"+Username);
			}
			
			member.Properties.ModifyProperty( syncGuid );
			domain.Commit( member );
			
			if ( status == MemberStatus.Created || 
					status == MemberStatus.Updated )
			{
				this.ReportInformation(
					String.Format(
						"Member: {0} Status: {1}",
						member.Name,
						status.ToString() ) );

				// Update counters
				if ( status == MemberStatus.Created )
				{
					created++;
				}
				else
				{
					updated++;
				}
			}	
			
			processed++;
		}

		/// <summary>
		/// Method to Update the modified group member list
		/// Used by ProcessMember method
		/// </summary>
		public void UpdateMemberList(string oldMemberList,string newMemberList,string groupDn)
		{
			log.Debug( "In UpdateMemberList: " + oldMemberList + " , " + newMemberList + " , " + groupDn + ".");
			string deleteGroupMembershiplist = String.Empty;
			if( oldMemberList == null ||  oldMemberList == "" )
				return;
			string[] oldMemberArray = oldMemberList.Split(new char[] { ';' });

                        foreach(string oldMember in oldMemberArray)
                        {
				if(newMemberList.IndexOf(oldMember) < 0)
				{
					deleteGroupMembershiplist += oldMember;	
					deleteGroupMembershiplist += ";";	
				}
			}
			if(deleteGroupMembershiplist != String.Empty)
				DeleteGroupMembership(deleteGroupMembershiplist, groupDn);
			return;
		}

		/// <summary>
		/// To check whether for any of this member's group, there exists a secondary admin or not
		/// </summary>
		/// <param name="OldGroupList">the group list which user belonged to before deleted from e-dir</param>
		/// <param name="AllAdministeredGroupDNs">all groups DNs which are being monitored by one secondary admin or other</param>
		/// <returns>
		/// true if there exists a secondary admin for this user, false otherwise
		/// </returns>
		public bool UsersGroupIsMonitored(string OldGroupList, ArrayList AllAdministeredGroupDNs)
		{
			if( OldGroupList == "" )
				return false;

			string[] UsersGroupArray = OldGroupList.Split(new char[] { ';' });

			foreach(string GroupDN in AllAdministeredGroupDNs)
			{
				
				if( Array.IndexOf( UsersGroupArray, GroupDN ) >= 0)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Return all GroupIDs for which there is a secondary administrator
		/// </summary>
		/// <param name="UserID">userID for which check has to be done.</param>
		/// <returns>
		/// true if there exists a secondary admin for this user, false otherwise
		/// </returns>
		public ArrayList GetAllAdministeredGroupDNs()
		{
			// may return repeated DNs, but very less in number and no extra iteration effect in other calling functions.
	
			ArrayList AllAdministeredGroupDNs = new ArrayList();
			Member SecondaryAdmin = null;
			string [] MonitoredGroups = null;
			
			ICSList members = domain.GetMembersByRights(Access.Rights.Secondary);
			foreach(ShallowNode sn in members)
			{
				SecondaryAdmin = new Member(domain, sn);
				MonitoredGroups = SecondaryAdmin.GetMonitoredGroups();
				foreach(string MonitoredGroupID in MonitoredGroups)
				{
					Member GroupAsMember = domain.GetMemberByID(MonitoredGroupID);
					Property p = GroupAsMember.Properties.GetSingleProperty( "DN" );
					string FullDN = ( p != null ) ? p.Value as String : null;
					if( FullDN != null )
					{
						AllAdministeredGroupDNs.Add(FullDN);
					}
				}	
				
			}
			return AllAdministeredGroupDNs;
		}

		/// <summary>
		/// Check if this property exists with the member or not
		/// </summary>
		public bool MemberPropExists(Member mem)
		{
			Property p = mem.Properties.GetSingleProperty( "TempUserGroups" );
			return ( p != null ? true : false );
		}		


		/// <summary>
		/// Method to delete the group entry from member object 
		/// Used by UpdateMemberList method
		/// </summary>
		public void DeleteGroupMembership(string membersList,string groupDn)
		{
			log.Debug( " In DeleteGroupMembership: " + membersList + " , " + groupDn + ".");
			string[] memberArray = membersList.Split(new char[] { ';' });

			// Useful to decide whether we need to store the UserGroups property temporarily
			ArrayList AllAdministeredGroupDNs = GetAllAdministeredGroupDNs();

                        foreach(string memberDn in memberArray)
                        {
                                if(memberDn.Equals(String.Empty))
				{
					// in case of empty string,continue
					continue;
				}

				Simias.Storage.Member member = null;
				bool CreateTempUserGroups = false;
				try
				{
					member = domain.GetMemberByDN( memberDn );
				}
				catch{}

				if ( member != null )
				{
					string oldGroupList = String.Empty;
					string newGroupList = String.Empty;
					try
					{
						oldGroupList = member.Properties.GetSingleProperty( "UserGroups" ).Value as string;

						// If group/secondary admin exist for any of this member's groups, then store the whole UserGroups prop as temp
						// because later when his iFolders become orphan, then we need the group admin from the group he belongs
						// also, if this temp property was stored earlier (either during previous groupprocessing or previous LdapSync)
						// then do not store because it will overwrite the original UserGroups entry

						CreateTempUserGroups = ( !MemberPropExists( member ) && UsersGroupIsMonitored( oldGroupList, AllAdministeredGroupDNs) ) ? true : false;
					}
					catch{}
					string[] oldGroupArray = oldGroupList.Split(new char[] { ';' });
                        		foreach(string oldGroup in oldGroupArray)
                        		{
						if(String.Compare(groupDn, oldGroup) != 0 && oldGroup != "")
						{
							newGroupList += oldGroup;
							newGroupList += ";";
						}
					}
					Property userGroups = new Property( "UserGroups", newGroupList);
					member.Properties.ModifyProperty( userGroups );
					
					if( CreateTempUserGroups )
					{
						Property tempUserGroups = new Property( "TempUserGroups", oldGroupList);
						tempUserGroups.ServerOnlyProperty = true;
						member.Properties.ModifyProperty( tempUserGroups );
						log.Debug(" DeleteGroupMembership, the old grouplist added as tempgrouplist");
					}	

					domain.Commit( member );
				}
				else
					log.Debug( "DeleteGroupMembership GetMemberByDN returned null");
			}
			
			Member GroupAsMember = domain.GetMemberByDN(groupDn);		
			if ( GroupAsMember != null )
			{
				log.Debug("For a deleted group, there is no need of storing MembersList property, so delete that property");
				GroupAsMember.Properties.DeleteProperties( "MembersList" );
				domain.Commit(GroupAsMember);
			}
	
			return;
		}

		/// <summary>
		/// Method to modify the membersList entry from group object 
		/// Used by UpdateMemberList method, Take all member list of this group and remove memberDn
		/// </summary>
		public void DeleteUserGroupMembership(string groupList,string memberDn)
		{
			log.Debug( " In DeleteUserGroupMembership: " + groupList + " , " + memberDn + ".");
			string[] groupArray = groupList.Split(new char[] { ';' });

                        foreach(string groupDn in groupArray)
                        {
				if(groupDn != "")
				{
					Simias.Storage.Member member = null;
					try
					{
						member = domain.GetMemberByDN( groupDn );
					}
					catch{}
					if ( member != null )
					{
						MultiValuedList mvl = member.Properties.GetProperties( "MembersList" );
						if( mvl != null && mvl.Count > 0)
						{
							ArrayList entries = new ArrayList();
							foreach( Property p in mvl )
							{
								if(p != null)
									entries.Add(p.Value as string);
							}
							member.Properties.DeleteProperties("MembersList");
							foreach( string memDN in entries )
							{
								if(memDN != null)
									if(String.Compare(memberDn, memDN) != 0)
									{
										Property memberInGroup = new Property( "MembersList", memDN);
										memberInGroup.ServerOnlyProperty = true;
                                        					member.Properties.AddNodeProperty( memberInGroup );
									}
							}
						}
						domain.Commit( member );
					}
					else
						log.Debug( "DeleteUserGroupMembership GetMemberByDN returned null");
				}
				//else
				//	log.Debug( "DeleteUserGroupMembership GetMemberByDN returned null");
			//}
			}
			return;
		}

		/// <summary>
		/// Method to report errors against a specific sync cycle
		/// Used by the Sync Service and Sync Providers.
		/// </summary>
		public void ReportError( string ErrorMsg )
		{
			reportedErrors++;
			log.Debug( "Error occured in Sync");
			log.Debug(
				String.Format(
					"ERROR:{0} - {1}",
					DateTime.Now.ToString(),
					ErrorMsg ) );
			syncMessages.Add(
				String.Format(
					"ERROR:{0} - {1}",
					DateTime.Now.ToString(),
					ErrorMsg ) );
		}
		
		/// <summary>
        /// Method to report information against a specific sync cycle
        /// Used by the Sync Service and Sync Providers.
		/// </summary>
		/// <param name="Message"></param>
		public void ReportInformation( string Message )
		{
			syncMessages.Add(
				String.Format(
					"INFO:{0} - {1}",
					DateTime.Now.ToString(),
					Message ) );
		}
		#endregion
	}
	
	/// <summary>
	/// Class that implements the identity sync provider functionality.
	/// </summary>
	public class Service
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log =
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// List that holds the registered providers.
		/// </summary>
		static internal Hashtable registeredProviders = new Hashtable();
		
		static private IIdentitySyncProvider current = null;
		static AutoResetEvent syncEvent = null;
		static bool running = false;
		static bool quit;
		static internal bool syncOnStart = true;
		static internal int syncInterval = 60 * 60 * 24; //24 Hrs
		static internal int deleteGracePeriod = 60 * 60 * 24 * 5;  // 5 days
		static internal bool syncDisabled = false;
		static Thread syncThread = null;
		static private int waitForever = 0x1FFFFFFF;
		static internal string status;
		static internal DateTime upSince;
		static internal int cycles = 0;
		static internal bool master = true;
		
		static internal IdentitySynchronization.State lastState = null;
		static string disabledAtProperty = "IdentitySynchronization:DisabledAt";
		#endregion

		#region Properties
		/// <summary>
		/// Gets the number of registered providers.
		/// </summary>
		static public int Count
		{
			get { return registeredProviders.Count; }
		}

		/// <summary>
		/// Gets the number of synchronization cycles that have performed.
		/// </summary>
		static public int Cycles
		{
			get { return cycles; }
		}

		/// <summary>
		/// Gets/sets the number of seconds that lapse before an unknown user
		/// is removed from the domain.
		/// </summary>
		static public int DeleteGracePeriod
		{
			get 
			{ 
			    Store store = Store.GetStore ();
			    Domain domain = store.GetDomain ( store.DefaultDomain );
			    if( domain.Properties.GetSingleProperty( "DeleteGracePeriod" ) != null)
			 	deleteGracePeriod = (int) domain.Properties.GetSingleProperty( "DeleteGracePeriod" ).Value;
			    return deleteGracePeriod;
			}
			set
			{ 
			    Store store = Store.GetStore ();
			    Domain domain = store.GetDomain ( store.DefaultDomain );
			    Property deleteGracePeriodProp = new Property( "DeleteGracePeriod" , value );
			    deleteGracePeriodProp.LocalProperty = true;
			    domain.Properties.ModifyProperty( deleteGracePeriodProp );
			    domain.Commit( domain );

			    deleteGracePeriod = value; 
			}
		}

		/// <summary>
		/// Gets the last state of the identity synchronization.
		/// </summary>
		static public IdentitySynchronization.State LastState
		{
			get { return lastState; }
		}

		/// <summary>
		/// Returns the registered identity providers.
		/// </summary>
		static public IIdentitySyncProvider[] Providers
		{
			get
			{
				IIdentitySyncProvider[] providers =
					new IIdentitySyncProvider[ registeredProviders.Count ];
				lock ( typeof( IdentitySynchronization.Service ) )
				{
					registeredProviders.CopyTo( providers, 0 );
				}
				return providers;
			}
		}

		/// <summary>
		/// Gets the registered providers.
		/// </summary>
		static public Hashtable RegisteredProviders
		{
			get { return registeredProviders; }
		}

		/// <summary>
		/// Gets the status.
		/// </summary>
		static public string Status
		{
			get { return status; }
		}

		/// <summary>
		/// Sets a value indicating if synchronization is disabled.
		/// </summary>
		static public bool SyncDisabled
		{
			set { syncDisabled = value; }
		}

		/// <summary>
		/// Gets/sets the last LDAP Sync time.
		/// </summary>
		static public DateTime LastLdapSyncTime
		{
			get 
			{ 
                            Store store = Store.GetStore ();
			    DateTime ret;
                            Domain domain = store.GetDomain ( store.DefaultDomain );
                            if( domain.Properties.GetSingleProperty( "LastLdapSyncTime" ) != null)
			    	ret = (DateTime) domain.Properties.GetSingleProperty( "LastLdapSyncTime" ).Value;
			    else
			    	ret = DateTime.Now; 
			    return ret;
			}
			set 
			{
			    //Save this value in Domain.
			    Store store = Store.GetStore ();
			    Domain domain = store.GetDomain ( store.DefaultDomain );
			    Property LastLdapSyncTimeProp = new Property( "LastLdapSyncTime" , value);
			    LastLdapSyncTimeProp.LocalProperty = true;
			    domain.Properties.ModifyProperty( LastLdapSyncTimeProp );
			    domain.Commit( domain );
			}
		}

		/// <summary>
		/// Gets/sets the number of seconds in the synchronization interval.
		/// </summary>
		static public int SyncInterval
		{
			get 
			{ 
                            Store store = Store.GetStore ();
                            Domain domain = store.GetDomain ( store.DefaultDomain );
                            if( domain.Properties.GetSingleProperty( "IDSyncInterval" ) != null)
			    	syncInterval = (int) domain.Properties.GetSingleProperty( "IDSyncInterval" ).Value;
			    return syncInterval; 
			}
			set {
			    //Save this value in Domain.
			    Store store = Store.GetStore ();
			    Domain domain = store.GetDomain ( store.DefaultDomain );
			    Property syncIntervalProp = new Property( "IDSyncInterval" , value );
			    syncIntervalProp.LocalProperty = true;
			    domain.Properties.ModifyProperty( syncIntervalProp );
			    domain.Commit( domain );

			    syncInterval = value; 
			}
		}

		/// <summary>
		/// Gets the date and time when the synchronization service was started.
		/// </summary>
		static public DateTime UpSince
		{
			get { return upSince; }
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// If the user is removed from the domain scope, his POBox
		/// should get removed from the system rather than orphaned
		/// </summary>
		private static void DeletePOBox( IdentitySynchronization.State State, Member Zombie )
		{
			try
			{
				Store store = Store.GetStore();
				ICSList cList = store.GetCollectionsByOwner( Zombie.UserID );
				foreach( ShallowNode sn in cList )
				{
					Collection c = new Collection( store, sn );
					if ( ( c.Domain == State.SDomain.ID ) &&
						( (Node) c).IsBaseType( NodeTypes.POBoxType ) )
					{
						c.Commit( c.Delete() );
						
						Property dn = Zombie.Properties.GetSingleProperty( "DN" );
						string userName = ( dn.Value != null ) ? dn.Value as string : Zombie.Name;
						string logMessage =
							String.Format(
								"Removed {0}'s POBox from Domain: {1}",
								userName,
								State.SDomain.Name );
						
						log.Info( logMessage );
						State.ReportInformation( logMessage );
						break;
					}
				}
			}
			catch( Exception e2 )
			{
				log.Error( e2.Message );
				log.Error( e2.StackTrace );
			}
		}

		/// <summary>
		/// Method to orphan all collections where
		/// where Zombie is the owner of the collection.
		/// Ownership of orphaned collections is assigned over to the
		/// domain administrator.  The previous owner's DN is saved in
		/// the "OrphanedOwner" property on the collection.
		/// </summary>
		private
		static
		void OrphanCollections( IdentitySynchronization.State State, Member Zombie, string [] GroupIDs )
		{
			string dn =	Zombie.Properties.GetSingleProperty( "DN" ).Value as string;
			if ( dn == null || dn == "" )
			{
				dn = Zombie.Name;
			}
			log.Debug( "OC :  dn for deleted user is  "+dn );
			string GroupOrphOwnerID = null;
			bool CheckForSecondaryAdmin = true;

			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			ICSList cList = store.GetCollectionsByOwner( Zombie.UserID );
			foreach ( ShallowNode sn in cList )
			{
				// Remove the user as a member of this collection.
				Collection c = new Collection( store, sn );

				// Only look for collections from the specified domain and
				// don't allow this user's membership removed from the domain.
				if ( ( c.Domain == State.SDomain.ID ) &&
					!( (Node) c).IsBaseType( NodeTypes.DomainType ) &&
					!( (Node) c).IsBaseType( NodeTypes.POBoxType ) )
				{
					Member member = c.GetMemberByID( Zombie.UserID );
					if (member != null && member.IsOwner == true )
					{
						if( CheckForSecondaryAdmin == true && GroupIDs != null && GroupIDs.Length > 0 ) //make sure this cond gets executed only once, even if collections change
						{
							// foreach group this zombie user belongs to, check if the group has a right secondary admin
							foreach( string groupID in GroupIDs)
							{
								if(groupID == Zombie.UserID)
								{
									// zombie user should not be iterated
									continue;
								}
								ICSList SecondaryAdmins = domain.GetMembersByRights(Access.Rights.Secondary);
								foreach(ShallowNode sns in SecondaryAdmins)
								{
									Member SecondaryAdminMem = new Member(domain, sns);
									long Preference = 0;
									Preference = SecondaryAdminMem.GetPreferencesForGroup(groupID);
									//check, if this secondary admin has rights to own Orphan iFolders of this group
									if (Preference == 0)
									{
										// Secondary admin is not owner of this group, check for next sec admin
										continue;
									}	
									else
									{
										log.Debug("The returned Preference is :"+Preference);
										GroupOrphOwnerID = ( (Preference & (int)512) != (int)512) ? null : SecondaryAdminMem.UserID;
										if(GroupOrphOwnerID != null)
										{
											break;
										}
									}
								}	
								// We want this check to be performed only once for one zombie user, so disable the check
								// so that for other collections of same owner, it does not search same data again.
								CheckForSecondaryAdmin = false;
								if(GroupOrphOwnerID != null)
								{
									break;
								}
							}
						}
						// Don't remove an orphaned collection.
						if ( ( member.UserID != State.SDomain.Owner.UserID ) )
						{
							
							//
							// The desired IT behavior is to orphan all collections
							// where the zombie user is the owner of the collection.
							// Policy could dictate and force the collection deleted at
							// a later time but the job of the sync code is to
							// orphan the collection and assign the Simias admin as
							// the new owner.
							//

							// Adding the code so that, if zombie user is member of a group and the group has a setting
							// so that all orphaned iFolders should be owned by groupadmin, then primary admin will not
							// get the ownership

							string SimiasAdminUserID = ( GroupOrphOwnerID == null ? State.SDomain.Owner.UserID : GroupOrphOwnerID );
							Member SimiasAdminAsMember = domain.GetMemberByID(SimiasAdminUserID);

							// Simias Admin must be a member first before ownership
							// can be transfered
							Member adminMember =
								c.GetMemberByID( SimiasAdminUserID );
							if ( adminMember == null )
							{
								adminMember =
									new Member(
											SimiasAdminAsMember.Name,
											SimiasAdminAsMember.UserID,
											Simias.Storage.Access.Rights.Admin );
								c.Commit( adminMember );
							}

							Property prevProp = new Property( "OrphanedOwner", dn );
							prevProp.LocalProperty = true;
							c.Properties.ModifyProperty( prevProp );
							c.Commit();

							c.Commit( c.ChangeOwner( adminMember, Simias.Storage.Access.Rights.Admin ) );

							// Now remove the old member
							c.Commit( c.Delete( c.Refresh( member ) ) );
								
							string logMessage =
								String.Format(
									"Orphaned Collection: {0} - previous owner: {1}",
									c.Name,
									dn );
							log.Info( logMessage );
							State.ReportInformation( logMessage );
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Method to remove membership from all collections that
		/// the zombie user is a member of.
		/// This method does not handle the case where the zombie
		/// user is the owner of a collection.
		/// </summary>
		private
		static
		void RemoveMemberships( IdentitySynchronization.State State, Member Zombie )
		{
			Store store = Store.GetStore();
			
			// Get all of the collections that this user is member of.
			ICSList cList = store.GetCollectionsByUser( Zombie.UserID );
			foreach ( ShallowNode sn in cList )
			{
				// Remove the user as a member of this collection.
				Collection c = new Collection( store, sn );

				// Only look for collections from the specified domain and
				// don't allow this user's membership removed from the domain itself.
				if ( ( c.Domain == State.SDomain.ID ) &&
					!( (Node) c).IsBaseType( NodeTypes.DomainType ) )
				{
					Member member = c.GetMemberByID( Zombie.UserID );
					if (member != null && member.IsOwner == false )
					{
						// Not the owner, just remove the membership.
						c.Commit( c.Delete( member ) );
						Property dn = Zombie.Properties.GetSingleProperty( "DN" );
						string userName = ( dn.Value != null ) ? dn.Value as string : Zombie.Name;
						
						string logMessage =
							String.Format(
								"Removed {0}'s membership from Collection: {1}",
								userName,
								c.Name );
						log.Info( logMessage );
						State.ReportInformation( logMessage );
					}
				}
			}
		}
		
		/// <summary>
		/// Method to to check and handle members that were not
		/// processed by the external sync provider in the
		/// current sync cycle.
		/// </summary>
		private static void ProcessDeletedMembers( IdentitySynchronization.State State )
		{
			// check for deleted members
			log.Debug( "Checking for deleted members.." );

			try
			{
				Property syncGUID = State.SyncGuid;
				
				ICSList	deleteList =
					State.SDomain.Search( "SyncGuid", syncGUID.Value, SearchOp.Not_Equal );
				foreach( ShallowNode cShallow in deleteList )
				{
					Node cNode = new Node( State.SDomain, cShallow );
					if ( cNode.IsType( "Member" ) == true )
					{
						Member cMember = new Member( cNode );
						string dn =
							cMember.Properties.GetSingleProperty( "DN" ).Value as string;

					 	ProcessDeletedGroupEntries(State, cMember);
						// See if this account has been previously disabled
						if ( State.SDomain.IsLoginDisabled( cMember.UserID ) == true )
						{
							// Did the sync service disable the account?
							Property p = cMember.Properties.GetSingleProperty( disabledAtProperty );
							if ( p != null )
							{
								DateTime dt = (DateTime) p.Value;

								// OK, this guy has been disabled past
								// the policy time so delete him from the
								// domain roster
								if ( dt.AddSeconds( DeleteGracePeriod ) < DateTime.Now )
								{
									string [] groupIDs = State.SDomain.GetDeletedMembersGroupList(cMember.UserID);
									if ( groupIDs != null )
										log.Debug("This deleted member was member of {0} groups. ",groupIDs.Length);
									DeletePOBox( State, cMember );
									OrphanCollections( State, cMember, groupIDs );
									RemoveMemberships( State, cMember );

									// gather log info before commit
									string fn = cMember.Name;
									string id = cMember.UserID;

									State.SDomain.Commit( State.SDomain.Delete( cNode ) );

									string logMessage =
										String.Format(
											"Removed DN: {0} FN: {1} ID: {2} from Domain: {3}",
											dn,
											fn,
											id,
											State.SDomain.Name );
									log.Info( logMessage );
									State.ReportInformation( logMessage );
								}
								
								continue;
							}
						}
						
						log.Debug( " disabling member: " + cMember.Name );
						State.SDomain.SetLoginDisabled( cMember.UserID, true );
	
						Property disable = new Property( disabledAtProperty, DateTime.Now );
						disable.LocalProperty = true;
						cMember.Properties.ModifyProperty( disable );
						State.SDomain.Commit( cMember );
						
						string disabledMessage =
							String.Format(
								"Disabled Member: {0} DN: {1} ID: {2} from Domain: {3}",
								cMember.Name,
								dn,
								cMember.UserID,
								State.SDomain.Name );
								
						log.Info( disabledMessage );
						State.ReportInformation( disabledMessage );
						disabledMessage = null;
					}
				}
			}
			catch( Exception e1 )
			{
				string locMessage = "Exception checking/deleting members ";
				State.ReportError( locMessage + e1.Message );
				log.Error( locMessage );
				log.Error( e1.Message );
				log.Error( e1.StackTrace );
			}
		}

	public static void ProcessDeletedGroupEntries(IdentitySynchronization.State State, Member cMember)
	{							
		string dn =
                               cMember.Properties.GetSingleProperty( "DN" ).Value as string;

									string group = null;
									try
									{
										group = cMember.Properties.GetSingleProperty( "GroupType" ).Value as string;
									}
									catch{}
									if(group != null && group != "" && String.Compare(group.ToLower(),"global") == 0)
									{
										string DN = null;
										string membersList = "";
										try
										{
											DN = cMember.Properties.GetSingleProperty( "DN" ).Value as string;
											MultiValuedList mvl = cMember.Properties.GetProperties( "MembersList" );
											if( mvl != null && mvl.Count > 0)
											{
												foreach( Property prop in mvl )
												{
													if( prop.Value != null)
													{
														membersList += prop.Value as string;
														membersList += ";";
													}
												}
											}
										}
										catch{}
										if ( DN != null && DN != "" && membersList != "" )
										{
										       State.DeleteGroupMembership(membersList, dn );
										}
									}	
									else if(group != null && group != "" && String.Compare(group.ToLower(),"local") == 0)
									{
										//local groups should not be deleted, because it is not synced.
									}
									else if(group == null || group == "" )
									{
										string DN = null;
										string groupList = null;
										try
										{
											DN = cMember.Properties.GetSingleProperty( "DN" ).Value as string;
											groupList = cMember.Properties.GetSingleProperty( "UserGroups" ).Value as string;
										}
										catch{log.Debug("ExceptionException!!!!");}
										if ( DN != null && DN != "" && groupList != null && groupList != "")
										{
											State.DeleteUserGroupMembership(groupList,DN);
										}
									}

		}

		#endregion

		#region Public Methods
		
		/// <summary>
		/// Method for registering external synchronization providers
		/// </summary>
		/// <param name="provider">An ILocationProvider interface object.</param>
		static public void Register( IIdentitySyncProvider provider )
		{
			lock ( typeof( IdentitySynchronization.Service ) )
			{
				log.Debug( "Registering provider {0}.", provider.Name );
				registeredProviders.Add( provider.Name, provider );
				syncEvent.Set();
			}
		}

		/// <summary>
		/// Method for registering external synchronization providers
		/// </summary>
		/// <param name="provider">An ILocationProvider interface object.</param>
		static public void Unregister( IIdentitySyncProvider provider )
		{
			lock ( typeof( IdentitySynchronization.Service ) )
			{
				log.Debug( "Unregistering provider {0}.", provider.Name );
				registeredProviders.Remove( provider.Name );
			}
		}
		
		/// <summary>
		/// Sync Interval set
		/// </summary>
		public static int SetDeleteMemberGracePeriod( int seconds )
		{
			log.Debug( "SetDeleteMemberGracePeriod Called now : " + seconds );
			DeleteGracePeriod = seconds;
			return 0;
		}

		/// <summary>
		/// Returns Sync details 
		/// </summary>
		public static string GetSynchronizationDetails( int option )
		{
			string result = String.Empty;
			log.Debug( "GetSynchronizationDetails Called now : " + option );
			switch(option)
			{
				case 1:
					return UpSince.ToLongTimeString();
				break;
				case 2:
					return Cycles.ToString();
				break;
				case 3:
					return DeleteGracePeriod.ToString();
				break;
				case 4:
					return SyncInterval.ToString();
				break;
				case 5:
					return Status;
				break;
				default:
					return result;
				break;
			}	
		}

		/// <summary>
		/// Sync Interval set
		/// </summary>
		public static int SetSyncInterval( int seconds )
		{
			log.Debug( "SetSyncInterval Called now : " + seconds );
			SyncInterval = seconds;
			return 0;
		}

		/// <summary>
		/// Force a synchronization cycle immediately
		/// </summary>
		public static int SyncNow( string data )
		{
			log.Debug( "SyncNow called" );

			//NOTE : Phase 1 Changes for GEO Based Identity Sync
// 			if ( !master )
// 			{
// 				log.Debug( "Identity sync service disabled in Slave" );
// 				return -1;
// 			}

			if ( running == false )
			{
				log.Debug( "  synchronization service not running" );
				return -1;
			}
			
			log.Info( "SyncNow method invoked" );
			
			syncEvent.Set();
			log.Debug( "SyncNow finished" );
			return 0;
		}
		
		/// <summary>
		/// Starts the external identity sync container
		/// </summary>
		/// <returns>N/A</returns>
		static public void Start( )
		{
//NOTE : Phase 1 Changes for GEO Based Identity Sync
// 			Simias.Configuration config = Store.Config;
// 			string cfgValue = config.Get( "Server", "MasterAddress" );
// 			log.Debug("cfgValue {0}", cfgValue);
// 			if ( cfgValue != null ) // || cfgValue != String.Empty )
// 			{
// 				master = false;
// 				log.Debug( "Identity sync service disabled in Slave" );
// 				return;
// 			}
			if ( running == true )
			{
				log.Debug( "Identity sync service is already running" );
				return;
			}
			
			log.Debug( "Start - called" );

			quit = false;
			syncEvent = new AutoResetEvent( false );
			syncThread = new Thread( new ThreadStart( SyncThread ) );
			syncThread.IsBackground = true;
			syncThread.Start();
		}

		/// <summary>
		/// Stops the external identity sync container
		/// </summary>
		/// <returns>N/A</returns>
		static public void Stop( )
		{
			log.Debug( "Stop called" );
			quit = true;
			try
			{
				syncEvent.Set();
				Thread.Sleep( 32 );
				log.Debug( "Stop finished" );
			}
			catch(Exception e)
			{
				log.Debug( "failed with an exception" );
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}
			
			return;
		}
		
		/// <summary>
		/// long term synchronization thread
		/// responsible for enforcing sync cycles
		/// and calling the sync providers.
		/// </summary>
		/// <returns>N/A</returns>
		private static void SyncThread()
		{
			log.Debug( "SyncThread - starting" );
			log.Debug( "  waiting for providers to load" );
			
			Simias.IdentitySynchronization.Service.upSince = DateTime.Now;
			Simias.IdentitySynchronization.Service.cycles = 0;
			
			syncEvent.WaitOne( 1000 * 10, false );

 		        Store store = Store.GetStore ();
 			Domain domain = store.GetDomain ( store.DefaultDomain );

			while ( quit == false )
			{
 				if( domain.Properties.GetSingleProperty( "IDSyncInterval" ) == null)
			       		SyncInterval = 60 * 60 * 24; //24 Hrs Default Value. Save it in Domain.
 				else
				{
					Simias.Configuration config = Store.Config;
					string cfgValue = config.Get( "Identity", "LdapSyncOnRestart" );
					if ( cfgValue != null && cfgValue != String.Empty && String.Compare(cfgValue.ToLower(),"no") == 0)
					{
						syncOnStart = false;	
						DateTime CurrentTime = DateTime.Now;
						DateTime LastSyncTime =  LastLdapSyncTime;
						if(LastSyncTime.AddSeconds( SyncInterval) > CurrentTime)
						{
							TimeSpan tspan = LastSyncTime.AddSeconds( SyncInterval) - CurrentTime;
							syncInterval = (int)tspan.TotalSeconds;
						}
						else
							syncOnStart = true;
					}
					else
 			       			syncInterval = SyncInterval;
				}
				running = true;
				
				if ( registeredProviders.Count == 0 )
				{
					log.Debug( "No registered identity sync providers - disabling sync service" );
					syncDisabled = true;
				}
				
				if ( syncDisabled == true )
				{
					Simias.IdentitySynchronization.Service.status = "disabled";
					syncEvent.WaitOne( waitForever, false );
				}
				else
				if ( syncOnStart == false )
				{
					log.Debug( "Ldap Sync will wait for {0} seconds", syncInterval.ToString());
					Simias.IdentitySynchronization.Service.status = "waiting";
					syncEvent.WaitOne( syncInterval * 1000, false );
				}
				
				if ( quit == true )
				{
					continue;
				}
				
				
				log.Debug( "Start - syncing identities" );
				Simias.IdentitySynchronization.State state = null;
				Simias.IdentitySynchronization.Service.status = "running";
				
				try
				{
					// Create a state object which is passed to the providers
					// For now we only know how to sync the default domain
					state = new Simias.IdentitySynchronization.State( Store.GetStore().DefaultDomain );

 					// Cycle thru the providers.
					foreach( IIdentitySyncProvider prov in registeredProviders.Values )
					{
						current = prov;
						LastLdapSyncTime = DateTime.Now;
						current.Start( state );
						current = null;
					}
					if ( state.Errors == 0 )
					{
						ProcessDeletedMembers( state );
					}
					else 
					{
						log.Error("Errors occured during Identity Sync, skipping processing of user deletion");
					}
					// moving this call after ProcessDeletedMembers() because in ProcessDeletedMembers(), disabling of users will
					// take place which will be used by ProcessMembersAndGroupsLocally..
					ProcessMembersAndGroupsLocally( state );
					// if the grace interval for renamed users has expired, then delete the flag that says login is allowed
					CleanGraceLoginForRenamed();
				}
				catch( Exception ex )
				{
					log.Error( ex.Message );
					log.Error( ex.StackTrace );
				}
				finally
				{
					state.EndTime = DateTime.Now;
					Simias.IdentitySynchronization.Service.cycles++;
					Simias.IdentitySynchronization.Service.lastState = null;
					Thread.Sleep( 32 );
					Simias.IdentitySynchronization.Service.lastState = state;
				}
				
				// Always wait after the first iteration
				syncOnStart = false;
				log.Debug( "Stop - syncing identities" );
			}
			
			Simias.IdentitySynchronization.Service.status = "shutdown";
			syncEvent.Close();
			syncThread = null;
			running = false;
		}

        /// <summary>
        /// Read all the member entries and group entries locally and modify such that non-existent entries aer removed from the list.
        /// No exception should be thrown , this is a utility kind, which even if fails, does not cause any harm. This is to ensure that
        /// unwanted entries are removed from user's and group's list.
        /// </summary>
        /// <param name="state"></param>
                private static void ProcessMembersAndGroupsLocally(IdentitySynchronization.State state)
                {

                        string MemberList = "";
                        string GroupList = null;
                        string MemList = null;
                        Member memObject = null;
			Store store = null;
			Domain domain = null;
                        Property dn = null;
                        try{
                                store = Store.GetStore();
                                domain = store.GetDomain( store.DefaultDomain );
                                if ( domain == null )
                                {
                                        log.Debug("ProcessMembersAndGroupsLocally : domain was null, returning before completion");
                                        return;
                                }

                                ICSList FullMemList = domain.GetMemberList();
                                foreach (ShallowNode sn in FullMemList)
                                {
                                        memObject = new Member(domain, sn);
                                        if(memObject == null)
                                        {
                                                log.Debug("ProcessMembersAndGroupsLocally : Could not form member object, Continuing with other objects");
                                                continue;
                                        }
                                        dn = memObject.Properties.GetSingleProperty( "DN" );
                                        if(dn == null)
                                        {
                                                log.Debug("ProcessMembersAndGroupsLocally : DN property was null for userid :  "+memObject.UserID);
                                                continue;
                                        }
                                        if(dn != null)
                                        {
                                                MemberList = ( dn.Value != null ) ? dn.Value as string : "";
                                                MemberList += ";";
                                                Property UserGroupsProp = memObject.Properties.GetSingleProperty( "UserGroups" );
                                                if(UserGroupsProp != null)
                                                {
                                                        GroupList = UserGroupsProp.Value as string;
                                                        if( GroupList != null && GroupList != String.Empty && GroupList != "")
                                                        {
                                                                string[] GroupArray = GroupList.Split(new char[] { ';' });
                                                                foreach(string GroupDN in GroupArray)
                                                                {
                                                                        if(GroupDN != null && GroupDN != "")
                                                                        {
                                                                                Member groupMember = domain.GetMemberByDN(GroupDN);
                                                                                if(groupMember == null)
                                                                                {
                                                                                        // modify member's UserGroups property and remove this non-existent group DN
                                                                                        log.Debug("Group DN {0} does not exist in the domain, so removing from member's grouplist",GroupDN);
                                                                                        // remove group's DN from the MemberList (its memberlist currently) 
                                                                                        state.DeleteGroupMembership(MemberList, GroupDN);
                                                                                }
                                                                        }
                                                                }
                                                        }
                                                }
                                                // Till now, For this member, all non-existent group entries (if any) are removed.

                                                // Now see if its property MemberList has some entries which are non-existent, if yes remove them too

                                                // Take a fresh member object which has latest committed value
                                                memObject = domain.GetMemberByID(memObject.UserID);
						try
						{
							MultiValuedList mvl = memObject.Properties.GetProperties( "MembersList" );
							if(mvl != null && mvl.Count > 0)
							{
								foreach( Property p in mvl )
								{
									string memberDN = p.Value as string;
									if(memberDN != null && memberDN != "")
									{
										Member mem = domain.GetMemberByDN(memberDN);
										if(mem == null)
										{
                                                                                        // modify group's MemberList property and remove this non-existent member DN
                                                                                        log.Debug("Member DN {0} does not exist in the domain, so remove from Group's Memberslist",memberDN);
                                                                                        // remove member's DN from the MemberList (its grouplist currently) 
                                                                                        state.DeleteUserGroupMembership(MemberList, memberDN);
											
										}
									}
								}
							}
						}
						catch{}

						// during ProcessMembers, if user was removed from group (in e-dir), or removed from e-dir 
						// then we delete the usergroups prop, but in both cases we store a tempusergroups prop... In first case
						// it is not needed, because user is removed really from group, in second case it is required because
						// during orphaned iFolder ownership, a policy may be there to be owned by secondary admins, so we need
						// the groups the deleted user belonged to... so find out first case and delete the user.

						// Did the sync service disable the account?
						Property disabledAt = memObject.Properties.GetSingleProperty( disabledAtProperty );
						if ( disabledAt == null )
						{
							// delete this temp prop only if user was not disabled by sync service
							Property tempUserGroupsProp = memObject.Properties.GetSingleProperty( "TempUserGroups" );
							if( tempUserGroupsProp != null)
							{
								
								memObject.Properties.DeleteSingleProperty("TempUserGroups");
								domain.Commit( memObject );
								log.Debug("The user was disabled because he was removed from group (not from e-dir), so tempgrouplist prop is not useful going forward so it was deleted, For this user, disabledAt prop was null , userDN :"+dn);
							}
						}
						else
							log.Debug("The user was disabled by ldapsync (because he was deleted in e-dir), so if user has tempgrouplist prop, then it would not be deleted , userDN :"+dn);
		

                                        }
                                }
				ConvertXMLPoliciesToString(state);
			
                        }catch(Exception e)
                        {
                                log.Debug("ProcessMembersAndGroupsLocally : Exception {0}, returning before completion ",e.ToString());
                        }

                }

		/// <summary>
		/// This function will check the grace login period for renamed users and if it expires, then delete the property so that login with old username is no more allo
wed
		/// <summary>
		private static void CleanGraceLoginForRenamed()
		{
			int RenameGraceLogin = 60 * 60 * 24 * 30; // 30 days
			Member member = null;
			Store store = Store.GetStore();
			Domain domain = store.GetDomain( store.DefaultDomain );
			ICSList list = domain.Search( "OldDN", "*", SearchOp.Exists );
			foreach ( ShallowNode sn in list )
			{
				log.Debug("plugin GMBON: checking the list");
				member = new Member( domain, sn );
				string Elements = member.OldDN;
				if( Elements == null ) continue;
				string [] CountAndUserNames = Elements.Split(new char[] { ':' });
				if ( CountAndUserNames != null && CountAndUserNames.Length >= 1 )
				{
					// user is found, now check for grace login period
					DateTime counter = new DateTime(Convert.ToInt64( CountAndUserNames[0] ) ) ;
					if ( counter.AddSeconds( RenameGraceLogin ) < DateTime.Now )
					{
						// the grace interval has expired, so delete this olddn property
						log.Debug("Since grace interval for older name login is expired for renamed user, deleting the property :"+member.Name);
						member.OldDN = null;
						domain.Commit( member );	
					}
					
				}
			}
		}


		
        /// <summary>
        /// This function will browse all member node and if any XML type policy is there, it will change that to string type policy	
        /// </summary>
        /// <param name="state"></param>
                private static void ConvertXMLPoliciesToString(IdentitySynchronization.State state)
                {

                        Member memObject = null;
			Store store = null;
			Domain domain = null;
                        Property dn = null;
			bool XmlRule = false;
			Simias.Policy.Rule rule = null;
                        try{
                                store = Store.GetStore();
                                domain = store.GetDomain( store.DefaultDomain );

                                ICSList FullMemList = domain.GetMemberList();
                                foreach (ShallowNode sn in FullMemList)
                                {
					if (sn.IsBaseType(NodeTypes.MemberType))
					{
	                                        memObject = new Member(domain, sn);
	                                        if(memObject == null)
        	                                {
                	                                log.Debug("ConvertXMLPoliciesToString : Could not form member object, returning before completion");
                        	                        return;
                                	        }
						if (!memObject.IsType("Host"))
                                        	{
							// Its a member object, browse for XML policies
							string [] PolicyIDs = {Simias.Policy.FileTypeFilter.FileTypeFilterPolicyID, Simias.Policy.iFolderLimit.iFolderLimitPolicyID, Simias.Policy.DiskSpaceQuota.DiskSpaceQuotaPolicyID, Simias.Policy.FileSizeFilter.FileSizeFilterPolicyID};
							foreach(string PolicyID in PolicyIDs)
							{
								MultiValuedList mvl = memObject.Properties.GetProperties(PolicyID);
								if(mvl == null || mvl.Count == 0)
	       	                         			{
									// no policies on this member, continue for next member 	
									continue;
								}
								ArrayList RuleList = new ArrayList();
								XmlRule = false;
								foreach( Property p in mvl )
								{
									rule = null;
									if( !(p.Type == Syntax.String) )
									{
										// Add all the XMl rules to list
										rule = new Simias.Policy.Rule( p.Value  );
										RuleList.Add(rule);
										XmlRule = true;
									}
								}
								if(XmlRule)
								{
									// member has xml rules, so commit back the string format.
									memObject.Properties.DeleteProperties(PolicyID);
									foreach ( Simias.Policy.Rule xrule in RuleList )
									{
										Property p = new Property( PolicyID, xrule.ToString() );
										p.ServerOnlyProperty = true;
										memObject.Properties.AddNodeProperty( p );
									}
									log.Debug("ConvertXMLPoliciesToString : going to commit the string policy for policy id :"+PolicyID);
									domain.Commit(memObject);
									XmlRule = false;
								}
							}
						}
					}
				}
			} catch {}

			}

		#endregion
	}
}
