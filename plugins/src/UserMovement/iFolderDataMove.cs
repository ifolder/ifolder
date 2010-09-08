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
*                 $Mod Date: 28 - 05 - 2008
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Starts User move service and manages the user move queue >
*
*
*******************************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Threading;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Service;
using Simias.Storage;
using Simias.Sync;

namespace Simias.UserMovement
{
	/// <summary>
	/// </summary>
	public class DataMove 
	{
		#region Types

		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof(DataMove));

	        /// <summary>
                /// Service Lock Object
                /// </summary>
		private static string lockObj = "locker";

	        /// <summary>
                /// User Move Queue
                /// </summary>
		private static Queue eventQueue;

	        /// <summary>
                /// User Move Process Event
                /// </summary>
		private static AutoResetEvent queueEvent;

	        /// <summary>
                /// User Move thread, separate thread to process user move requests
                /// </summary>
		private static Thread moveThread;

	        /// <summary>
                /// Automatic queue processing time and interval. By default after every 30 sec's queue processing 
		/// starts automatically and manual this event can be set. This event is set manually when user gets
		/// added to the queue.
                /// </summary>
		private static int QueEventScheduleTime = 30;

	        /// <summary>
                /// Service thread run state. 
                /// </summary>
		private static bool down = false;

	        /// <summary>
                /// Service run state.
                /// </summary>
		private static bool started = false;

	        /// <summary>
                /// Queue Processing State
                /// </summary>
		private static bool QueueProcessingState = false;

	        /// <summary>
                /// Store Variable
                /// </summary>
		private static Store store;

	        /// <summary>
                /// Domain Variable
                /// </summary>
		private static Domain domain;

		private static  string currentiFolderID = "";

                /// <summary>
                /// enum to denote iFolder move state
                /// </summary>
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

		#endregion

		#region Constructors

		/// <summary>
                /// Constructor for creating a new UserMove object.
                /// </summary>
		public DataMove()
		{
			store = Store.GetStore();
			domain = store.GetDomain( store.DefaultDomain );
		}
		#endregion

		#region Private Methods

                /// <summary>
                /// IsiFolderAlreadyInQueueOrMoved check whether iFolder is in queue or already moved.  
                /// </summary>
                /// <param name="ifDataMove">iFolder data move object</param>
                /// <returns> True if the iFolder is already moved,
                /// false if iFolder is not moved or in queue</returns>
		static public bool IsiFolderAlreadyInQueueOrMoved( iFolderDataMove ifDataMove )
		{
			log.Debug("IsiFolderAlreadyInQueueOrMoved: Checking iFolder id {0}:{1} in the Data Move Queue", ifDataMove.iFolderID, ifDataMove.iFolderName);

			lock( eventQueue )
			{
				if ( eventQueue.Count != 0 )
				{
					Array ifDataMoveList=Array.CreateInstance(typeof(iFolderDataMove), eventQueue.Count);
					eventQueue.CopyTo( ifDataMoveList, 0);
					foreach(iFolderDataMove ifDM in ifDataMoveList)
					{
						if(ifDM.iFolderID == ifDataMove.iFolderID)
						{
							log.Debug("IsiFolderAlreadyInQueueOrMoved: iFolder {0}:{1} is already in the queue", ifDataMove.iFolderID, ifDataMove.iFolderName);
							return false;
						}
					}
				}
			}

                        store = Store.GetStore();
                        domain = store.GetDomain(store.DefaultDomain);
			Member member = domain.GetMemberByID(ifDataMove.MemberUserID);
			if(member != null)
			{
				int state = member.iFolderMoveState(domain.ID, false, ifDataMove.iFolderID, 0, 0);
				if(state == (int)iFolderMoveState.Completed )	
				{
					log.Debug("State completed for: {0}", ifDataMove.iFolderName);
					member.iFolderMoveState(domain.ID, true, ifDataMove.iFolderID, 0, 0);
					return true;
				}
				else if(state == (int)iFolderMoveState.NotStarted)	
				{
					log.Debug("State already moved for: {0}", ifDataMove.iFolderName);
					if(store.GetCollectionByID(ifDataMove.iFolderID) != null)
                                		return true;
				}
			}
			return false;
		}

                /// <summary>
                /// Adds iFolder move request to the iFolder move queue
                /// </summary>
                /// <param name="ifDataMove">iFolder data move object</param>
                /// <returns> false in all cases.</returns>
		static public bool Add( iFolderDataMove ifDataMove )
		{
			log.Debug("Add: Adding iFolder id {0}:{1} to the Data Move Queue", ifDataMove.iFolderID, ifDataMove.iFolderName);

			lock( eventQueue )
			{
				if ( eventQueue.Count != 0 )
				{
					Array ifDataMoveList=Array.CreateInstance(typeof(iFolderDataMove), eventQueue.Count);
					eventQueue.CopyTo( ifDataMoveList, 0);
					foreach(iFolderDataMove ifDM in ifDataMoveList)
					{
						if(ifDM.iFolderID == ifDataMove.iFolderID)
						{
							log.Debug("Add: iFolder {0}:{1} is already in the queue", ifDataMove.iFolderID, ifDataMove.iFolderName);
							return false;
						}
					}
				}
				if( currentiFolderID == ifDataMove.iFolderID )
				{
					log.Debug("Not adding in the queue since this iFolder is being synced now. {0}", currentiFolderID);
					return false;
				}
				eventQueue.Enqueue( ifDataMove );
				log.Debug("Add: iFolder id {0} is included in the queue", ifDataMove.iFolderID);
				
			}
			return false;
		}


                /// <summary>
                /// Sets the Queue event for processing the user move queue
                /// </summary>
                /// <returns> nothing.</returns>
		static public void SetQueueEvent( )
		{
			log.Debug("SetQueueEvent: DataMove Queue Process event set");
			if(QueueProcessingState == false)
				queueEvent.Set();
		}


                /// <summary>
                /// Processes the Data Move queue, either periodically or based on the manual reset event.
                /// </summary>
                /// <returns> Nothing.</returns>
		static private void ProcessEvents()
		{
                        store = Store.GetStore();
                        domain = store.GetDomain(store.DefaultDomain);
			iFolderDataMove ifDataMove = null;
			Member member = null;
			while( down == false )
			{
				queueEvent.WaitOne(QueEventScheduleTime * 1000 , false);
				if(down == true)
					break;
				QueueProcessingState = true;
				while( true )
				{
					ifDataMove = null;
					lock( eventQueue )
					{
						if ( eventQueue.Count == 0 )
						{
							queueEvent.Reset();
							break;
						}
						ifDataMove = eventQueue.Dequeue() as iFolderDataMove;
					}
					try
					{
						member = domain.GetMemberByID(ifDataMove.MemberUserID);
						if(member != null)
						{
							currentiFolderID = ifDataMove.iFolderID;
							member.iFolderMoveState(domain.ID, true, ifDataMove.iFolderID, (int)iFolderMoveState.Started, 0);
							if(!Collection.DownloadCollectionLocally(ifDataMove.iFolderID, ifDataMove.iFolderName, ifDataMove.DomainID, ifDataMove.HostID, ifDataMove.DirNodeID, ifDataMove.MemberUserID, ifDataMove.colMemberNodeID, ifDataMove.iFolderLocalPath, ifDataMove.sourceFileCount, ifDataMove.sourceDirCount))
							{
								currentiFolderID = "";	
								log.Debug( "downloadifolder returned false for {0} ",ifDataMove.iFolderID );
								Add(ifDataMove);
								Thread.Sleep(5 * 1000);
							}
							else
							{
								/// Recreate the catalog entry...
								Simias.Server.Catalog.RecreateEntryForCollection(ifDataMove.iFolderID);
								member.iFolderMoveState(domain.ID, true, ifDataMove.iFolderID, (int)iFolderMoveState.Completed, 0);
							}
							currentiFolderID = "";
						}
						else
						{
							log.Debug( "member {0} is not present in domain ",ifDataMove.MemberUserID );
							log.Debug( "removing {0} from Data move queue",ifDataMove.iFolderID);
						}
					}
					catch(Exception e)
					{
						log.Debug( "Data ProcessEvents: Expection {0} {1} received for {2}",e.Message, e.StackTrace, ifDataMove.iFolderID );
						Add(ifDataMove);
						Thread.Sleep(30 * 1000);
					}
					finally
					{
						currentiFolderID = "";
					}
				}
				QueueProcessingState = false;
			}
		}


		#endregion

                /// <summary>
                /// Starts the User Move Service
                /// </summary>
                /// <returns>Nothing</returns>
		public void Start()
		{
			log.Debug( "Starting data Move thread..." );
			lock( lockObj )
			{
				if ( started == true )
				{
					log.Info( "Data move thread is already running." );
					return ;
				}
				down = false;
				eventQueue = new Queue();
				queueEvent = new AutoResetEvent( false );
				moveThread = new Thread( new ThreadStart( ProcessEvents ) );
				moveThread.IsBackground = true;
				moveThread.Name = "User Move, iFolder download thread";
				moveThread.Start();
				started = true;
				SetQueueEvent();
			}
			log.Debug( "Data Move thread started..." );
		}

		/// <summary>
		/// Called to stop the user Move service.
		/// </summary>
		public void Stop()
		{
			down = true;
			queueEvent.Set();
			Thread.Sleep( 32 );
			if(QueueProcessingState == true)
                                moveThread.Abort();
			started = false;
			log.Debug( "Data Move thread stopped..." );
		}

	}
}
