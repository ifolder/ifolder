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
*                 $Author: Mahabaleshwar M Asundi
*                 $Mod Date: 09-04-2008
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        < iFolder user movement class >
*
*
*******************************************************************************/

using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Collections;

using Simias;
using Simias.Event;
using Simias.Server;
using Simias.POBox;
using Simias.Service;
using Simias.Storage;
using Simias.Policy;

namespace Simias.UserMovement
{
	/// <summary>
	/// iFolder user Move class
	/// </summary>
	public class iFolderUserMove
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(iFolderUserMove));

		public SimiasAccessLogger logger = null;

                /// <summary>
                /// Domain Object
                /// </summary>
		static Domain domain;

                /// <summary>
                /// Store Object
                /// </summary>
		static Store store;

                /// <summary>
                /// Users Current Home Server HostNode object
                /// </summary>
		HostNode CurrentHomeServer;

                /// <summary>
                /// Users New Home Server HostNode object
                /// </summary>
		HostNode NewHomeServer;

                /// <summary>
                /// Users Master Server HostNode object
                /// </summary>
		HostNode MasterHomeServer;

                /// <summary>
                /// Users member object instance
                /// </summary>
		public Member member;

		ICertificatePolicy defPolicy;

		public enum iFolderMoveState
                {
			/// <summary>
                        /// iFolder Move is not started
                        /// </summary>
                        NotStarted,

                        /// <summary>
                        /// iFolder Move is already started
                        /// </summary>
                        Started,

                        /// <summary>
                        /// iFolder Move Completed
                        /// </summary>
                        Completed
		};
	

		~iFolderUserMove()
		{
			ServicePointManager.CertificatePolicy = defPolicy;
		}
        /// <summary>
        /// Set the certificate policy
        /// </summary>
		private void SetCert()
		{
			defPolicy = ServicePointManager.CertificatePolicy;
			ServicePointManager.CertificatePolicy = new SingleCertificatePolicy();
		}

                /// <summary>
                /// Starting point API for user reprovision, Entry point to the UserMove DLL. 
		/// Initiates the user move request and places the user in the user move queue.
                /// </summary>
                /// <param name="userID">User id of the user to be moved</param>
                /// <param name="masterServer">HostNode instance of master server</param>
                /// <param name="currentServer">HostNode instance of users current server</param>
                /// <param name="newServer">HostNode instance of the users new server</param>
                /// <returns> Hostnode value of the server, null if somthing fails.</returns>
		public static HostNode Reprovision(string userID, HostNode masterServer, HostNode currentServer, HostNode newServer)
		{
			log.Debug("Reprovision: User move request received for user id {0} ", userID);
			try
			{
				iFolderUserMove ifUserMove = new iFolderUserMove();
				ifUserMove.SetCert();
				store = Store.GetStore();
				domain = store.GetDomain(store.DefaultDomain);
				ifUserMove.member = domain.GetMemberByID(userID);
				ifUserMove.CurrentHomeServer = currentServer;
				ifUserMove.NewHomeServer = newServer;
				ifUserMove.MasterHomeServer = masterServer;
				String UserName = "user";
				if( ifUserMove.member != null)
				{
					UserName = ifUserMove.member.Name;
				}
				ifUserMove.logger = new SimiasAccessLogger(UserName, null);
				if(ifUserMove.CurrentHomeServer.UserID == ifUserMove.NewHomeServer.UserID ||								Simias.UserMovement.UserMove.IsUserAlreadyInQueue(ifUserMove) == true)
					return currentServer;
				if(MoveUserObject(ifUserMove) == false)
					return null;
				if(Simias.UserMovement.UserMove.Add(ifUserMove) == false)
					return null;
				try
				{
					if( ifUserMove.logger != null)
						ifUserMove.logger.LogAccess("User Move", "From: " + ifUserMove.CurrentHomeServer.PublicUrl, "To: " + ifUserMove.NewHomeServer.PublicUrl, "Started");
				}
				catch(Exception ex1)
				{
					log.Info("Exception while logging the user move status into access log. {0}--{1}", ex1.Message, ex1.StackTrace);
				}
				Simias.UserMovement.UserMove.SetQueueEvent();
			}
			catch(Exception e)
			{
			 	log.Debug("Reprovision: Exception {0} and stacktrace: {1}", e.Message, e.StackTrace);
				return null;
			}
			return currentServer ;
		}

                /// <summary>
                /// Initiates the user move and disables the user for further logging in.
                /// </summary>
                /// <param name="ifUserMove">iFolder user move object</param>
                /// <returns> True on success, and false on failure.</returns>
		private static bool MoveUserObject(iFolderUserMove ifUserMove)
		{
			log.Debug("MoveUserObject: Moving User {0}:{1} ", ifUserMove.member.FN, ifUserMove.member.UserID);
			bool result = false;
			string userID = store.GetUserIDFromDomainID(domain.ID);
			try
			{
				if(ifUserMove.MasterHomeServer.UserID == HostNode.GetLocalHost().UserID)
				{
					// On Master, so no need to make web-service call to master again.. see bug 405409
					result = DisableUserLocallyOnMaster(domain.ID, ifUserMove.member.UserID, ifUserMove.NewHomeServer.UserID);	
					if( !result )
					{
						log.Debug("MoveUserObject: User disable failed id {0} ", ifUserMove.member.UserID);
						return false;
					}
				}
				else
				{	
	                        	SimiasConnection smConn = new SimiasConnection(domain.ID, userID, SimiasConnection.AuthType.PPK, ifUserMove.MasterHomeServer);
	                        	SimiasWebService svc = new SimiasWebService();
        	                	svc.Url = ifUserMove.MasterHomeServer.PublicUrl;

	                        	smConn.Authenticate ();
        	                	smConn.InitializeWebClient(svc, "Simias.asmx");
 		                       	result = svc.DisableUser(domain.ID, ifUserMove.member.UserID, ifUserMove.NewHomeServer.UserID);
					smConn.ClearConnection();
					if( !result )
					{
						log.Debug("MoveUserObject: User disable failed id {0} ", ifUserMove.member.UserID);
						return false;
					}
				}
			}
			catch(Exception e)
			{
				log.Debug("MoveUserObject: Exception {0} {1}", e.Message, e.StackTrace);
				return false;
			}
			return true;
		}

                /// <summary>
                /// Disables the user locally on master (applicable only for master server.
                /// </summary>
                /// <param name="DomainID">ID of domain</param>
                /// <param name="UserID">ID of user being moved to another server</param>
                /// <param name="NewHomeServerID">ID of new home server of the user</param>
                /// <returns> True on success, and false on failure.</returns>
		public static bool DisableUserLocallyOnMaster(string DomainID, string UserID, string NewHomeServerID)	
		{
			log.Debug("DisableUserLocallyOnMaster called on master");
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				member.UserMoveState = (int)Member.userMoveStates.Initialized;
				domain.Commit( member );
				member.NewHomeServer = NewHomeServerID;
				if(domain.IsLoginDisabledForUser(member))
				{
					member.LoginAlreadyDisabled = true;
				}
				else
					domain.SetLoginDisabled(member.UserID, true);
				member.UserMoveState = (int)Member.userMoveStates.UserDisabled;
				domain.Commit( member );
			}
			catch( Exception ex)
			{
				log.Debug("DisableUserLocallyOnMaster Exception : {0}", ex.Message);
				return false;
			}
			return true;
		}

                /// <summary>
                /// Replaces the users current home server to new server selected.
                /// </summary>
                /// <param name="ifUserMove">iFolder user move object</param>
                /// <returns> True on success, and false on failure.</returns>
		private static bool UpdateHomeServer(iFolderUserMove ifUserMove)
		{
			log.Debug("UpdateHomeServer: User id {0} ", ifUserMove.member.UserID);
                        store = Store.GetStore();
                        domain = store.GetDomain(store.DefaultDomain);
			string userID = store.GetUserIDFromDomainID(domain.ID);
			bool result = false;
			try
			{
				if(ifUserMove.MasterHomeServer.UserID == HostNode.GetLocalHost().UserID)
				{
					// On Master, so no need to make web-service call to master again.. see bug 405409
					result = UpdateHomeServerLocallyOnMaster(domain.ID, ifUserMove.member.UserID, ifUserMove.NewHomeServer.UserID);	
					if( !result )
					{
						log.Debug("MoveUserObject: Usermove update homeserver failed id {0} ", ifUserMove.member.UserID);
						return false;
					}
				}
				else
				{	
	                        	SimiasConnection smConn = new SimiasConnection(domain.ID, userID, SimiasConnection.AuthType.PPK, ifUserMove.MasterHomeServer);
 		                       	SimiasWebService svc = new SimiasWebService();
                	        	svc.Url = ifUserMove.MasterHomeServer.PublicUrl;

                        		smConn.Authenticate ();
                        		smConn.InitializeWebClient(svc, "Simias.asmx");
                        		result = svc.UpdateHomeServer(domain.ID, ifUserMove.member.UserID, ifUserMove.NewHomeServer.UserID);
					smConn.ClearConnection();
					if( !result )
					{
						log.Debug("Home server update failed for user id {0} ", ifUserMove.member.UserID);
						return false;
					}
				}
			}
			catch(Exception e)
			{
				log.Debug("UpdateHomeServer Failed, {0} {1}", e.Message, e.StackTrace);
				return false;
			}
			return true;
		}

                /// <summary>
                /// Updates home server locally on master (applicable only for master server.
                /// </summary>
                /// <param name="DomainID">ID of domain</param>
                /// <param name="UserID">ID of user being moved to another server</param>
                /// <param name="NewHomeServerID">ID of new home server of the user</param>
                /// <returns> True on success, and false on failure.</returns>
		public static bool UpdateHomeServerLocallyOnMaster(string DomainID, string UserID, string NewHomeServerID)	
		{
			log.Debug("UpdateHomeServerLocallyOnMaster called on master");
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				member.UserMoveState = (int)Member.userMoveStates.Reprovision;
				domain.Commit( member );
				member.HomeServer =new HostNode(domain.GetMemberByID(NewHomeServerID)); 
				member.DeleteProperty = PropertyTags.NewHostID;
				if(member.LoginAlreadyDisabled == false)
					domain.SetLoginDisabled(member.UserID, false);
				else
					member.DeleteProperty = PropertyTags.LoginAlreadyDisabled;
				member.UserMoveState = (int)Member.userMoveStates.MoveCompleted;
				domain.Commit( member );
			}
			catch( Exception ex)
			{
				log.Debug("UpdateHomeServerLocallyOnMaster Exception : {0}", ex.Message);
				return false;
			}
			return true;
		}

                /// <summary>
                /// Gets the list of user owned ifolder ID's
                /// </summary>
                /// <param name="userID">User id</param>
                /// <returns> array of user owned ifolder ID's.</returns>
		public static string[] GetiFoldersByMember(string userID)
		{
                        CatalogEntry[] catalogEntries;
			ArrayList list = new ArrayList();
                        catalogEntries = Catalog.GetAllEntriesByUserID (userID);
                        foreach(CatalogEntry ce in catalogEntries)
                        {
                               if (ce.OwnerID == userID)
				{
					list.Add(ce.CollectionID);
				}
                        }
			return (string[])list.ToArray(typeof(string));
		}

                /// <summary>
                /// Updates the user move queue with list of users to be moved.
                /// </summary>
                /// <returns> Nothing.</returns>
		public static void UpdateUserMoveQueue()
		{
			log.Debug("UpdateUserMoveQueue: Updating user move queue");
                        Store store = Store.GetStore();
                        Domain domain = store.GetDomain(store.DefaultDomain);
			Collection c = domain;
			ICSList members = c.GetMemberList();	
			foreach(ShallowNode sn in members)
			{
				Member member = new Member(c, sn);
				if (!member.IsType("Host"))
				{
					if(member.UserMoveState >= 0  && member.NewHomeServer != null )
					{
						iFolderUserMove ifUserMove = new iFolderUserMove();
						ifUserMove.SetCert();
                                		ifUserMove.member = member;
                                		ifUserMove.CurrentHomeServer = member.HomeServer;
                                		ifUserMove.NewHomeServer = new HostNode(domain.GetMemberByID(member.NewHomeServer));
                                		ifUserMove.MasterHomeServer = HostNode.GetMaster(domain.ID);
						ifUserMove.logger = new SimiasAccessLogger(ifUserMove.member.Name, null);
						if(member.HomeServer.UserID == HostNode.GetLocalHost().UserID)
						{
							log.Debug("UpdateUserMoveQueue: Adding User {0} to queue ", member.UserID);
                                			Simias.UserMovement.UserMove.Add(ifUserMove);
						}
					}	
				}
			}	
			return;
		}

                /// <summary>
                /// Updates the user move state on master.
                /// </summary>
                /// <param name="member">member instnce</param>
                /// <param name="masterHost">HostNode instance of master</param>
                /// <param name="userMoveStatus">User move state</param>
                /// <returns> True if on success and false on Failure.</returns>
		public static bool UpdateUserMoveState(Member member, HostNode masterHost, int userMoveStatus)
		{
			log.Debug("UpdateUserMoveState: User id {0}:{1} ", member.UserID, member.FN);
                        store = Store.GetStore();
                        domain = store.GetDomain(store.DefaultDomain);
			string userID = store.GetUserIDFromDomainID(domain.ID);
			
			bool result= false;

			if(masterHost.UserID == HostNode.GetLocalHost().UserID)
			{
				// On Master, so no need to make web-service call to master again.. see bug 405409
				result = UpdateUserMoveStateLocallyOnMaster(domain.ID, member.UserID, userMoveStatus);	
				if( !result )
				{
					log.Debug("MoveUserObject: Usermove state update failed id {0} ", member.UserID);
					return false;
				}
			}
			else
			{	
       	 	                SimiasConnection smConn = new SimiasConnection(domain.ID, userID, SimiasConnection.AuthType.PPK, masterHost);
                	        SimiasWebService svc = new SimiasWebService();
                        	svc.Url = masterHost.PublicUrl;

                        	smConn.Authenticate ();
     		                smConn.InitializeWebClient(svc, "Simias.asmx");
               			result = svc.UpdateUserMoveState(domain.ID, member.UserID, userMoveStatus);
				smConn.ClearConnection();
				if( !result )
				{
					log.Debug("UpdateUserMoveState: Failed for user {0} ", member.UserID);
					return false;
				}
			}
			return true;
		}

                /// <summary>
                /// Updates user move state locally on master (applicable only for master server.
                /// </summary>
                /// <param name="DomainID">ID of domain</param>
                /// <param name="UserID">ID of user being moved to another server</param>
                /// <param name="UserMoveState">int value indicatins the current state</param>
                /// <returns> True on success, and false on failure.</returns>
		public static bool UpdateUserMoveStateLocallyOnMaster(string DomainID, string UserID, int UserMoveState)	
		{
			log.Debug("UpdateUserMoveStateLocallyOnMaster called on master");
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				member.UserMoveState = UserMoveState;
				domain.Commit( member );
			}
			catch( Exception ex)
			{
				log.Debug("UpdateUserMoveStateLocallyOnMaster Exception : {0}", ex.Message);
				return false;
			}
			return true;
		}

                /// <summary>
                /// Updates the local properties on remote server
                /// </summary>
                /// <param name="member">member object</param>
                /// <param name="newHost">HostNode instance of new server</param>
                /// <returns> True on success and false on failure.</returns>
		public static bool UpdateLocalProperties(Member member, HostNode newHost)
		{
			log.Debug("UpdateLocalProperties: User id {0}:{1} ",  member.FN, member.UserID);
			if(member.EncryptionKey == "" && member.EncryptionVersion == "" && member.EncryptionBlob == "" && member.RAName == "" && member.RAPublicKey == "")
				return true;
                        store = Store.GetStore();
                        domain = store.GetDomain(store.DefaultDomain);
			string userID = store.GetUserIDFromDomainID(domain.ID);
                        SimiasConnection smConn = new SimiasConnection(domain.ID, userID, SimiasConnection.AuthType.PPK, newHost);
                        SimiasWebService svc = new SimiasWebService();
                        svc.Url = newHost.PublicUrl;

                        smConn.Authenticate ();
                        smConn.InitializeWebClient(svc, "Simias.asmx");
                        bool result = svc.UpdateLocalProperties(domain.ID, member.UserID, member.EncryptionKey, member.EncryptionVersion, member.EncryptionBlob, member.RAName, member.RAPublicKey);
			smConn.ClearConnection();
			if( !result )
			{
				log.Debug("UpdateLocalProperties failed for User id {0} ", member.UserID);
				return false;
			}
			return true;
		}

                /// <summary>
                /// ProcessMovement, processes the user move request based on the user move state.
                /// </summary>
                /// <param name="domainID">Domain id</param>
                /// <returns> True on success and false on failure.</returns>
		public bool ProcessMovement(string domainID)
		{
			log.Debug("ProcessMovement: User id {0}:{1} ", member.FN, member.UserID);
			string[] iFolderList;
			switch((int) member.UserMoveState)	
			{
				default:
					log.Debug("ProcessMovement: state  Nousermove {0}", member.FN);
					if((int)member.UserMoveState > (int)Member.userMoveStates.MoveCompleted)
					{
						if(!UpdateUserMoveState(member,MasterHomeServer,(int)Member.userMoveStates.Nousermove))
							return false;
					}
					else
						return true;
					break;
				case (int)Member.userMoveStates.Nousermove:
					log.Debug("ProcessMovement: state  Nousermove {0}", member.FN);
					return false;
				case (int)Member.userMoveStates.Initialized:
					log.Debug("ProcessMovement: state  Initialized {0}", member.FN);
					store = Store.GetStore();
					domain = store.GetDomain(domainID);
					if(MoveUserObject(this) == false)
						return false;
					goto case (int)Member.userMoveStates.UserDisabled;
				case (int)Member.userMoveStates.UserDisabled:
					log.Debug("ProcessMovement: state  UserDisabled {0}", member.FN);
					if(!UpdateUserMoveState(member,MasterHomeServer,(int)Member.userMoveStates.DataMoveStarted))
						return false;
					try
					{
						if( logger != null)
							logger.LogAccess("User Move", "From: " + CurrentHomeServer.PublicUrl, "To: " + NewHomeServer.PublicUrl, "DataMove Started");
					}
					catch(Exception ex1)
					{
						log.Info("Exception while logging the user move status into access log. {0}--{1}", ex1.Message, ex1.StackTrace);
					}
					goto case (int)Member.userMoveStates.DataMoveStarted;
				case (int)Member.userMoveStates.DataMoveStarted:
					log.Debug("ProcessMovement: state  DataMoveStarted {0}", member.FN);
					iFolderList = GetiFoldersByMember(member.UserID);
					log.Debug("ProcessMovement: DataMoveStarted {0} About to process list, ", member.FN);
					foreach(string id in iFolderList)
					{
						int state = member.iFolderMoveState(domainID, false, id, 0, 0);
						log.Debug("ProcessMovement: DataMoveStarted, iFolder {0} Status {1}", id, state.ToString());
						if(state == (int)iFolderMoveState.NotStarted || state == (int)iFolderMoveState.Started)
						{
							store = Store.GetStore();
                        				Collection col = store.GetCollectionByID(id);
							if(col != null)
							{
								long iFolderSize = col.StorageSize;
								if(state == (int)iFolderMoveState.NotStarted)
									member.iFolderMoveState(domainID, true, id, (int)iFolderMoveState.Started, iFolderSize);
								log.Debug("ProcessMovement: About to move DATA for ifolder ID {0}", id);
								if(!MoveiFolderData(domainID, member.UserID, id, NewHomeServer))
									return false;
								member.iFolderMoveState(domainID, true, id, (int)iFolderMoveState.Completed, iFolderSize);
								if( logger != null)
									logger.LogAccess("User Move", "iFolder Name: " + col.Name, "iFolder ID: " +id, "Data Movement Completed");
							}
							else	
								member.iFolderMoveState(domainID, true, id, (int)iFolderMoveState.Completed, 0);
						}
					}
					log.Debug("ProcessMovement: DataMoveStarted {0}, Moved all iFolders", member.FN);
					goto case (int)Member.userMoveStates.Reprovision;
				case (int)Member.userMoveStates.Reprovision:
					log.Debug("ProcessMovement: state  Reprovision {0}", member.FN);
					if(!UpdateLocalProperties(member, NewHomeServer))
						return false;
					if(UpdateHomeServer(this) == false)
						return false;
					goto case (int)Member.userMoveStates.MoveCompleted;
				case (int)Member.userMoveStates.MoveCompleted:
					log.Debug("ProcessMovement: state  MoveCompleted {0}", member.FN);
					iFolderList = GetiFoldersByMember(member.UserID);
					foreach(string id in iFolderList)
					{
						member.iFolderMoveState(domainID, true, id, 0, 0);
					}
					member.DeleteProperty = PropertyTags.EncryptionKey;
					member.DeleteProperty = PropertyTags.EncryptionVersion;
					member.DeleteProperty = PropertyTags.EncryptionBlob;
					member.DeleteProperty = PropertyTags.RAName;
					member.DeleteProperty = PropertyTags.RAPublicKey;
					member.UpdateSearchContexts(false);
					UpdateUserMoveState(member,MasterHomeServer,(int)Member.userMoveStates.Nousermove);
					try
					{
						if( logger != null)
							logger.LogAccess("User Move", "From: " + CurrentHomeServer.PublicUrl, "To: " + NewHomeServer.PublicUrl, "Completed"); 
					}
					catch(Exception ex1)
					{
						log.Info("Exception while logging the user move status into access log. {0}--{1}", ex1.Message, ex1.StackTrace);
					}
					break;
			}
			return true;
		}


                /// <summary>
                /// This method moves the iFolder from one server to another.
                /// </summary>
                /// <param name="domainID"> domainID of the iFolder server domain</param>
		/// <param name="userID"> user id of the owner of the iFolder to be moved. </param>
		/// <param name="iFolderID"> ID of the iFolder to be moved.</param>
		/// <param name="NewServer"> Host node objest of the new server to which the data is to be moved.</param>
                /// <returns> True if the data is moved successfully.
                /// false if the otherwise.</returns>
                private static bool MoveiFolderData(string domainID, string userID, string iFolderID, HostNode NewServer)
                {
                        log.Debug("MoveiFolderData: Moving iFolder {0} for user {1}..", iFolderID, userID);
                        Store store = Store.GetStore();
                        Collection col = store.GetCollectionByID(iFolderID);
                        if( col == null)
                        {
                                log.Debug("Failed to Get collecection for iFolder: {0} ", iFolderID);
                                return true;
                        }
                        col.DataMovement = true;
                        col.Commit();
			log.Debug("Setting the datamove property for collection: {0}", col.ID);
                        Catalog.AddCollectionForMovement(iFolderID, true);
                        string DirNodeID = col.GetRootDirectory().ID;
                        Member member = col.GetMemberByID (userID);
                        if( member == null)
			{
                                log.Debug("MoveiFolderData: Collection member node is null. {0} Cannot be moved.", col.ID);
				return false;
			}
                        string MemberUserID = userID;
                        string UserID = store.GetUserIDFromDomainID(domainID);
                        SimiasConnection smConn = new SimiasConnection(domainID, UserID, SimiasConnection.AuthType.PPK, NewServer);
                        SimiasWebService svc = new SimiasWebService();
                        svc.Url = NewServer.PublicUrl;
                        smConn.Authenticate ();
                        smConn.InitializeWebClient(svc, "Simias.asmx");
                        bool status = false;
                        try
                        {
                                status = svc.DownloadiFolder(iFolderID, col.Name, col.Domain, UserID, DirNodeID, MemberUserID, member.ID, null);
                        }
                        catch(Exception ex)
                        {
                                smConn.ClearConnection();
                                log.Debug("MoveiFolderData: Exception in remote downloadiFolder method: {0} {1}", ex.Message, ex.StackTrace);
                                throw new Exception(String.Format("Exception in remote downloadiFolder method: {0} {1}", ex.Message, ex.StackTrace));
                        }
                        smConn.ClearConnection();
                        if( status == true )
                        {
                                col.Commit(col.Delete());
                                log.Debug("MoveiFolderData: deleted the collection on this host... ");
				log.Debug("MoveiFolderData: Sleeping for 10 secs as a work around for #515190");
				Thread.Sleep(10000);
				log.Debug("MoveiFolderData: Out of sleep");
                                Catalog.SetHostForCollection(iFolderID, NewServer.UserID);
                                Catalog.RemoveCollectionForMovement(iFolderID);
                        }
                        else
                        {
                                log.Debug("MoveiFolderData: The collection is not moved properly. sync returned false.");
				return false;
                        }
                        return true;
                }

                /// <summary>
                /// ProcessSlaveRemoval, processes the Slave server removal request based on the user move state.
                /// </summary>
                /// <param name="domainID">Domain id</param>
                /// <returns> True on success and false on failure.</returns>
		public bool ProcessSlaveRemoval(string domainID)
		{
			log.Debug("ProcessMovement: User id {0}:{1} ", member.FN, member.UserID);
			string[] iFolderList;
			switch((int) member.UserMoveState)	
			{
				default:
					log.Debug("ProcessMovement: state  Nousermove {0}", member.FN);
					return false;
				case (int)Member.userMoveStates.Nousermove:
					log.Debug("ProcessMovement: state  Nousermove {0}", member.FN);
					return false;
				case (int)Member.userMoveStates.Initialized:
					log.Debug("ProcessMovement: state  Initialized {0}", member.FN);
					store = Store.GetStore();
					domain = store.GetDomain(domainID);
					goto case (int)Member.userMoveStates.UserDisabled;
				case (int)Member.userMoveStates.UserDisabled:
					log.Debug("ProcessMovement: state  UserDisabled {0}", member.FN);
					if(!UpdateUserMoveState(member,MasterHomeServer,(int)Member.userMoveStates.DataMoveStarted))
						return false;
					goto case (int)Member.userMoveStates.DataMoveStarted;
				case (int)Member.userMoveStates.DataMoveStarted:
					log.Debug("ProcessMovement: state  DataMoveStarted {0}", member.FN);
					iFolderList = GetiFoldersByMember(member.UserID);
					foreach(string id in iFolderList)
					{
						int state = member.iFolderMoveState(domainID, false, id, 0, 0);
						if(state == (int)iFolderMoveState.NotStarted || state == (int)iFolderMoveState.Started)
						{
							if(state == (int)iFolderMoveState.NotStarted)
								member.iFolderMoveState(domainID, true, id, (int)iFolderMoveState.Started, 0);
							log.Debug("ProcessMovement: About to move DATA for ifolder ID {0}", id);
							if(!MoveiFolderData(domainID, member.UserID, id, NewHomeServer))
								return false;
							member.iFolderMoveState(domainID, true, id, (int)iFolderMoveState.Completed, 0);
						}
					}
					goto case (int)Member.userMoveStates.Reprovision;
				case (int)Member.userMoveStates.Reprovision:
					log.Debug("ProcessMovement: state  Reprovision {0}", member.FN);
					if(UpdateHomeServer(this) == false)
						return false;
					goto case (int)Member.userMoveStates.MoveCompleted;
				case (int)Member.userMoveStates.MoveCompleted:
					log.Debug("ProcessMovement: state  MoveCompleted {0}", member.FN);
					if(!UpdateLocalProperties(member, NewHomeServer))
						return false;
					iFolderList = GetiFoldersByMember(member.UserID);
					foreach(string id in iFolderList)
					{
						member.iFolderMoveState(domainID, true, id, 0, 0);
					}
					member.DeleteProperty = PropertyTags.EncryptionKey;
					member.DeleteProperty = PropertyTags.EncryptionVersion;
					member.DeleteProperty = PropertyTags.EncryptionBlob;
					member.DeleteProperty = PropertyTags.RAName;
					member.DeleteProperty = PropertyTags.RAPublicKey;
					member.UpdateSearchContexts(false);
					UpdateUserMoveState(member,MasterHomeServer,(int)Member.userMoveStates.Nousermove);
					break;
			}
			return true;
		}


	}
        /// <summary>
        /// Single Certificate Policy
        /// </summary>
        internal class SingleCertificatePolicy : ICertificatePolicy
        {

                /// <summary>
                /// Constructor
                /// </summary>
                public SingleCertificatePolicy(  )
                {
                }

                /// <summary>
                /// Check Validation Result
                /// </summary>
                /// <param name="srvPoint"></param>
                /// <param name="certificate"></param>
                /// <param name="request"></param>
                /// <param name="certificateProblem"></param>
                /// <returns></returns>
                public bool CheckValidationResult( ServicePoint srvPoint,
                        System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        WebRequest request,
                        int certificateProblem )
                {
                        return true;
                }
        }

	/// <summary>
	/// iFolder Data Move class, used only by the DATA receiving server.
	/// </summary>
	public class iFolderDataMove
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(iFolderDataMove));

                /// <summary>
                /// Domain Object ID
                /// </summary>
		public string DomainID;	

                /// <summary>
                /// iFolder's ID to be moved
                /// </summary>
		public string iFolderID; 
		
                /// <summary>
                /// iFolder's Name 
                /// </summary>
		public string iFolderName;

                /// <summary>
                /// iFolder's HostID
                /// </summary>
		public string HostID;

                /// <summary>
                /// DirNode ID
                /// </summary>
		public string DirNodeID;

                /// <summary>
                /// Member UserID
                /// </summary>
		public string MemberUserID;

                /// <summary>
                /// Collection Member Node ID
                /// </summary>
		public string colMemberNodeID;

                /// <summary>
                /// iFolder's ID to be moved
                /// </summary>
		public string iFolderLocalPath;


                /// <summary>
                /// Starting point API for user reprovision, Entry point to the UserMove DLL. 
		/// Initiates the user move request and places the user in the user move queue.
                /// </summary>
                /// <param name="iFolderID">iFolder ID to be moved</param>
                /// <param name="iFolderName">iFolder Name</param>
                /// <param name="DomainID">Domain ID</param>
                /// <param name="HostID"> Host ID</param>
                /// <param name="DirNodeID"> Dir Node ID</param>
                /// <param name="MemberUserID">iFolder members UserID </param>
                /// <param name="colMemberNodeID"> Collection Member Node ID</param>
                /// <param name="iFolderLocalPath"> iFolder Local Path </param>
                /// <returns> true if already moved, false if put in queue or not in queue</returns>
		public static bool MoveiFolder(string iFolderID, string iFolderName, string DomainID, string HostID, string DirNodeID, string MemberUserID, string colMemberNodeID, string iFolderLocalPath)
		{
			bool Status = false;
			log.Debug("MoveiFolder: Date move request received for id {0} {1} {2} {3} {4} {5} {6} {7}", iFolderID, iFolderName, DomainID, HostID, DirNodeID, MemberUserID, colMemberNodeID, iFolderLocalPath);
			try
			{
				iFolderDataMove ifDataMove = new iFolderDataMove();
				ifDataMove.iFolderID = iFolderID;
				ifDataMove.iFolderName = iFolderName;
				ifDataMove.DomainID = DomainID;
				ifDataMove.HostID = HostID;
				ifDataMove.DirNodeID = DirNodeID;
				ifDataMove.MemberUserID = MemberUserID;
				ifDataMove.colMemberNodeID = colMemberNodeID;
				ifDataMove.iFolderLocalPath = iFolderLocalPath;
				
				if(Simias.UserMovement.DataMove.IsiFolderAlreadyInQueueOrMoved(ifDataMove) == true)
					return true;
				
				Simias.UserMovement.DataMove.Add(ifDataMove);
				Simias.UserMovement.DataMove.SetQueueEvent();
			}
			catch(Exception e)
			{
			 	log.Debug("MoveiFolder: Exception {0} and stacktrace: {1}", e.Message, e.StackTrace);
			}
			return Status;
		}
	}

}
