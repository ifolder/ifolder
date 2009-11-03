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
using System.Net;
using System.Collections;

using Simias.Host;
using Simias.Service;
using Simias.Storage;

namespace Simias.Identity
{

	/// <summary>
	/// Summary description for IProvisionUser.
	/// </summary>
	public interface IProvisionUserProvider
	{
		/// <summary>
		/// </summary>
		string ProvisionUser( string userName );
	}

	/// <summary>
	/// Common User provision provider that provisions the user based on 
	/// different methods in an order
	/// 
	/// </summary>
	public class UserProvisionProvider
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( UserProvisionProvider ) );

		/// <summary>
		/// Constructor for the load balance provider.
		/// 
		/// Read all the hosts that are members of the domain and
		/// keep in a sorted list.  Register for member node changes
		/// so we know when new hosts come and go in the system.
		/// </summary>
		public UserProvisionProvider()
		{
		}

		#region IProvisionUserProvider Members
		/// <summary>
		/// </summary>
		public static string ProvisionUser( string userName )
		{
			log.Debug(String.Format("ProvisionUser {0}",userName));
			AttributeProvisionUserProvider attributeUserProvider = new AttributeProvisionUserProvider();
			return attributeUserProvider.ProvisionUser(userName);
		}

		/// <summary>Manual ProvisionUser method
		/// </summary>
		public static string ManualProvisionUser( string userName,string host )
		{
			log.Debug(String.Format("ManualProvisionUser {0}",userName));
			ManualProvisionUserProvider manualUserProvider = new ManualProvisionUserProvider();
			return manualUserProvider.ProvisionUser(userName, host);
		}
		#endregion
	}

	public class HostEntry : IComparable
	{
		HostNode	host;
		int			userCount;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hNode"></param>
		internal HostEntry( HostNode hNode )
		{
			host = hNode;
			ICSList members = hNode.GetHostedMembers();
			userCount = members.Count;
		}

        /// <summary>
        /// Gets the HostInfo object 
        /// </summary>
		internal Simias.Host.HostInfo Info
		{
			get { return new Simias.Host.HostInfo( host ); }
		}

        /// <summary>
        /// Gets HostNode object
        /// </summary>
		internal HostNode Host
		{
			get { return host; }
		}

        /// <summary>
        /// Adds host as the HomeServer for member
        /// </summary>
        /// <param name="domain">domain where it is to be added</param>
        /// <param name="member">member object</param>
		internal void AddMember( Domain domain, Member member )
		{
			member.HomeServer = host;
			System.Threading.Interlocked.Increment( ref userCount );
		}

		#region IComparable Members
        /// <summary>
        /// Icoparable function to compare to objects
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
		public int CompareTo( object obj )
		{
			HostEntry he = obj as HostEntry;
			return userCount.CompareTo( he.userCount );
		}
		#endregion
	}


    /// <summary>
    /// Provision the user manually class
    /// </summary>
	public class ManualProvisionUserProvider
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( ManualProvisionUserProvider ) );


		ArrayList hosts = new ArrayList();
		Domain domain;

        /// <summary>
        /// For this domain, adds all hostentry objects to hosts arraylist
        /// </summary>
		private void SetHostEntryList()
		{
			Store store = Store.GetStore();
			domain = store.GetDomain( store.DefaultDomain );
			HostNode[] hArray = HostNode.GetHosts( domain.ID );
			foreach ( HostNode host in hArray )
			{
				HostEntry hostentry = new HostEntry( host );
				hosts.Add( hostentry );
			}
			
			hosts.Sort();
		}

        /// <summary>
        /// Reads HomeServer property of given username
        /// </summary>
        /// <param name="userName">username to be searched</param>
        /// <returns>Null if HomeServer does not exist otherwise homeserver of the member as HostNode</returns>
		public HostNode checkUserProvisionedByName(string userName)
		{
                        HostNode hostnode = null;
                        Member member = domain.GetMemberByName( userName );
                        if ( member != null )
                                hostnode = member.HomeServer;
                        return hostnode;
		}

		#region IProvisionUserProvider Members
		/// <summary>
        /// Provision this user to given host
		/// </summary>
		public string ProvisionUser( string userName, string host )
		{
			HostNode hostnode = null;
			SetHostEntryList();
			Member member = domain.GetMemberByName( userName );
			log.Debug(String.Format("ProvisionUser {0}",userName));
			hostnode = checkUserProvisionedByName(userName);
			if ( hostnode == null )
			{
				foreach(HostEntry hentry in hosts)
				{
					Uri  publicUri = new Uri(hentry.Host.PublicUrl);
					IPHostEntry pubIPHostEntry = null;
					try
					{
    						pubIPHostEntry = Dns.Resolve(publicUri.Host);
					}
					catch{}
					if(String.Compare(publicUri.Host, host) == 0 ||
						String.Compare(pubIPHostEntry.HostName,host) == 0  )
                                        {
                                        	log.Debug(String.Format("Setting the home server for {0} to {1}",userName,
                                                host ));
                                                hostnode = hentry.Host;
                                                hentry.AddMember( domain, member );
						return hostnode == null ? null : hostnode.UserID;
                                        }
					else
					{
						foreach (IPAddress ip in pubIPHostEntry.AddressList)
    						{	
							if(String.Compare(ip.ToString(),host) == 0 )
							{
								log.Debug(String.Format(
								"Setting the home server for {0} to {1} {2}",
									userName,ip.ToString(),host));
								hostnode = hentry.Host;
								hentry.AddMember( domain, member );
								return hostnode == null ? null : hostnode.UserID;
							}
    						}
					}

				}
			}
			return hostnode == null ? null : hostnode.UserID;
		}

		#endregion
	}

	/// <summary>
	/// </summary>
	public class NearestSlaveProvisionUserProvider : IProvisionUserProvider
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( NearestSlaveProvisionUserProvider ) );

		ArrayList hosts = new ArrayList();
		Domain domain;

        /// <summary>
        /// Adds all HostEntry objects to hosts arraylist
        /// </summary>
		private void SetHostEntryList()
		{
			Store store = Store.GetStore();
			domain = store.GetDomain( store.DefaultDomain );
			HostNode[] hArray = HostNode.GetHosts( domain.ID );
			foreach ( HostNode host in hArray )
			{
				HostEntry hostentry = new HostEntry( host );
				hosts.Add( hostentry );
			}
			
			hosts.Sort();
		}

		#region IProvisionUserProvider Members
		/// <summary>
        /// Provision this user by calling SetUserHomeServer function
		/// </summary>
		public string ProvisionUser( string userName )
		{
			HostNode hostnode = null;
			SetHostEntryList();
			log.Debug(String.Format("ProvisionUser {0}",userName));
			hostnode = checkUserProvisionedByName(userName);
			if ( hostnode == null )
			{
				string Dn = GetUserDN(userName);
				hostnode = SetUserHomeServer(Dn);
			}
			return hostnode == null ? null : hostnode.UserID;
		}

                /// <summary>
                /// Gets the distinguished name from the member name.
                /// </summary>
                /// <param name="user">The user name.</param>
                /// <returns>The distinguished name found.</returns>
                private string GetUserDN( string user)
                {
                        Member member = null;
                        string distinguishedName = String.Empty;

                        if ( domain != null )
                        {
                                member = domain.GetMemberByName( user );
                                if ( member != null )
                                {
                                        Property dn = member.Properties.GetSingleProperty( "DN" );
                                        if ( dn != null )
                                                distinguishedName = dn.ToString();
                                }
                        }

                        return distinguishedName;
                }


        /// <summary>
        /// Checks if this userName is already provisioned
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>null if not provisioned, else it retuns HomeServer as HostNode object</returns>
		public HostNode checkUserProvisionedByName(string userName)
		{
                        HostNode hostnode = null;
                        Member member = domain.GetMemberByName( userName );
                        if ( member != null )
                                hostnode = member.HomeServer;
                        return hostnode;
		}

        /// <summary>
        /// Checks if this DN is already provisioned
        /// </summary>
        /// <param name="Dn">DN of this user</param>
        /// <returns>null if not provisioned, else it retuns HomeServer as HostNode object</returns>
		public HostNode checkUserProvisionedByDN(string Dn)
		{
                        HostNode hostnode = null;
                        Member member = domain.GetMemberByDN( Dn );
                        if ( member != null )
                                hostnode = member.HomeServer;
                        return hostnode;
		}

        /// <summary>
        /// Sets HomeServer for this DN, which is nearest 
        /// </summary>
        /// <param name="Dn">DN of user</param>
        /// <returns>HomeServer if succesful, else null</returns>
		public HostNode SetUserHomeServer(string Dn)
		{
			HostNode hostnode = null;
			Member member = domain.GetMemberByDN( Dn );
			hostnode = checkUserProvisionedByDN(Dn);
			log.Debug(String.Format("SetUserHomeServer {0}",Dn));
			if ( hostnode == null )
			{
				if( Dn.ToLower().IndexOf("ou") >= 0)
				{
					foreach(HostEntry hentry in hosts)
					{
					//Org Unit to machine name mapping is to be done...
						string orgUnit = "";
						string pubUrl = hentry.Host.PublicUrl;
						if(pubUrl.IndexOf(orgUnit) >= 0)
						{
							log.Debug(String.Format("Setting the home server for {0} to {1}",Dn,
								orgUnit ));
							hostnode = hentry.Host;
							hentry.AddMember( domain, member );
							return hostnode;
						}
					}
				}
				else
				{
					//Check groups and then provision the user. 
					string groupList = String.Empty;
                                        try
                                        {
                                                groupList = member.Properties.GetSingleProperty( "UserGroups" ).Value as string;
                                        }
                                        catch{}
					if(groupList != String.Empty && groupList != "")
					{
                                        	string[] groupArray = groupList.Split(new char[] { ';' });
                                        	foreach(string group in groupArray)
                                        	{
							hostnode = SetUserHomeServer(group);
							if ( hostnode != null )
							{
								log.Debug(String.Format("Setting the home server for {0} to {1}'s homeserver ",Dn,group));
								HostEntry hentry = new HostEntry( hostnode );
								hentry.AddMember( domain, member );
								return hostnode;
							}
                                        	}
					}
				}
			}
			return hostnode;
		}
		#endregion
	}

	/// <summary>
	/// </summary>
	public class AttributeProvisionUserProvider : IProvisionUserProvider
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(AttributeProvisionUserProvider));
		ArrayList hosts = new ArrayList();
		Domain domain;

        /// <summary>
        /// Adds all HostEntry objects into hosts arraylist
        /// </summary>
		private void SetHostEntryList()
		{
			Store store = Store.GetStore();
			domain = store.GetDomain( store.DefaultDomain );
			HostNode[] hArray = HostNode.GetHosts( domain.ID );
			foreach ( HostNode host in hArray )
			{
				HostEntry hostentry = new HostEntry( host );
				hosts.Add( hostentry );
			}
			
			hosts.Sort();
		}

		#region IProvisionUserProvider Members
		/// <summary>
        /// Provision this user by calling SetUserHomeServer
		/// </summary>
		public string ProvisionUser( string userName )
		{
			HostNode hostnode = null;
			SetHostEntryList();
			log.Debug(String.Format("ProvisionUser {0}",userName));
			hostnode = checkUserProvisionedByName(userName);
			if ( hostnode == null )
			{
				string Dn = GetUserDN(userName);
				hostnode = SetUserHomeServer(Dn);
			}
			return hostnode == null ? null : hostnode.UserID;
		}

                /// <summary>
                /// Gets the distinguished name from the member name.
                /// </summary>
                /// <param name="user">The user name.</param>
                /// <returns>The distinguished name found.</returns>
                private string GetUserDN( string user)
                {
                        Member member = null;
                        string distinguishedName = String.Empty;

                        if ( domain != null )
                        {
                                member = domain.GetMemberByName( user );
                                if ( member != null )
                                {
                                        Property dn = member.Properties.GetSingleProperty( "DN" );
                                        if ( dn != null )
                                                distinguishedName = dn.ToString();
                                }
                        }

                        return distinguishedName;
                }


        /// <summary>
        /// Based on name, check whether this user is provisioned or not
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>HostNode if user is provisioned otherwise null</returns>
		public HostNode checkUserProvisionedByName(string userName)
		{
                        HostNode hostnode = null;
                        Member member = domain.GetMemberByName( userName );
                        if ( member != null )
                                hostnode = member.HomeServer;
                        return hostnode;
		}

        /// <summary>
        /// Based on DN, check whther this user is provisioned or not
        /// </summary>
        /// <param name="Dn">User's DN</param>
        /// <returns>HostNode if user is provisioned otherwise null</returns>
		public HostNode checkUserProvisionedByDN(string Dn)
		{
                        HostNode hostnode = null;
                        Member member = domain.GetMemberByDN( Dn );
                        if ( member != null )
                                hostnode = member.HomeServer;
                        return hostnode;
		}

        /// <summary>
        /// Sets user home server based on LdapHomeAttribute
        /// </summary>
        /// <param name="Dn">User's DN</param>
        /// <returns>HostNode if set successfully otherwise null</returns>
		public HostNode SetUserHomeServer(string Dn)
		{
			HostNode hostnode = null;
			Member member = domain.GetMemberByDN( Dn );
			hostnode = checkUserProvisionedByDN(Dn);
			log.Debug(String.Format("In SetUserHomeServer {0}",Dn));
			if ( hostnode == null )
			{
			        string ldapHome = String.Empty;
                       		try
                                {
                               		ldapHome=member.Properties.GetSingleProperty( "LdapHomeAttribute" ).Value as string;
                                }
                                catch{}
				if(ldapHome != String.Empty &&  ldapHome != "")
				{
					IPHostEntry ldapIPHostEntry = null;
					try
					{
    						ldapIPHostEntry = Dns.Resolve(ldapHome);
					}
					catch{}
					
					foreach(HostEntry hentry in hosts)
					{
						Uri  publicUri = new Uri(hentry.Host.PublicUrl);
						IPHostEntry pubIPHostEntry = null;
						try
						{
    							pubIPHostEntry = Dns.Resolve(publicUri.Host);
						}
						catch{}
						if(String.Compare(publicUri.Host, ldapHome) == 0 ||
						String.Compare(publicUri.Host, ldapIPHostEntry.HostName ) == 0  ||
						String.Compare(pubIPHostEntry.HostName,ldapHome ) == 0 ||
						String.Compare(pubIPHostEntry.HostName,ldapIPHostEntry.HostName ) == 0  )
						{
							log.Debug(String.Format("Setting the home server for {0} to {1} {2}",
								Dn,ldapIPHostEntry.HostName,pubIPHostEntry.HostName ));
							hostnode = hentry.Host;
							hentry.AddMember( domain, member );
							return hostnode;
						}
						else
						{

							foreach (IPAddress ip in ldapIPHostEntry.AddressList)
    							{	
								foreach (IPAddress pubip in pubIPHostEntry.AddressList)
    								{	
									if(ip.Equals(pubip) == true)
									{
										log.Debug(String.Format(
										"Setting the home server for {0} to {1} {2}",
											Dn,ip.ToString(),pubip.ToString()));
										hostnode = hentry.Host;
										hentry.AddMember( domain, member );
										return hostnode;
									}
								}
    							}
/*
							// Alias name check is not required as of now, As of now it gets 
							// resolved with DNS name and IP. In case of alias check we can make
							// use of this code. 
							foreach (string alias in ldapIPHostEntry.Aliases)
    							{	
							    foreach (string pubalias in pubIPHostEntry.Aliases)
    							    {	
								if(String.Compare(publicUri.Host, alias) == 0 ||
								String.Compare(pubIPHostEntry.HostName,alias) == 0 ||
								String.Compare(pubalias,alias ) == 0  )
								{
									log.Debug(String.Format(
										"Setting the home server for {0} to {1} ",
										Dn,alias));
									hostnode = hentry.Host;
									hentry.AddMember( domain, member );
									return hostnode;
								}
							    }
							}
*/
						}
					}
				}
				else
				{
					//Check groups and then provision the user. 
					string groupList = String.Empty;
                                        try
                                        {
                                                groupList = member.Properties.GetSingleProperty( "UserGroups" ).Value as string;
                                        }
                                        catch{}
					if(groupList != String.Empty && groupList != "")
					{
                                        	string[] groupArray = groupList.Split(new char[] { ';' });
                                        	foreach(string group in groupArray)
                                        	{
							hostnode = SetUserHomeServer(group);
							if ( hostnode != null )
							{
								log.Debug(String.Format("Setting the home server for {0} to {1}'s homeserver ",Dn,group));
								HostEntry hentry = new HostEntry( hostnode );
								hentry.AddMember( domain, member );
								return hostnode;
							}
                                        	}
					}
				}
			}
			return hostnode;
		}
		#endregion
	}
}
