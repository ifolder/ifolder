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
	public class UserMove : IThreadService
	{
		#region Types

		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof(UserMove));

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

	        /// <summary>
                /// Domain Variable
                /// </summary>
		DataMove DMove = null;


		#endregion

		#region Constructors

		/// <summary>
                /// Constructor for creating a new UserMove object.
                /// </summary>
		public UserMove()
		{
			store = Store.GetStore();
			domain = store.GetDomain( store.DefaultDomain );
		}
		#endregion

		#region Private Methods

                /// <summary>
                /// IsUserAlreadyInQueue check users presence in the move queue
                /// </summary>
                /// <param name="ifUserMove">iFolder user move object</param>
                /// <returns> True if user exists in the queue,
                /// false if the user is not in the queue.</returns>
		static public bool IsUserAlreadyInQueue( iFolderUserMove ifUserMove )
		{
			log.Debug("IsUserAlreadyInQueue: Checking User id {0}:{1} in the User Move Queue", ifUserMove.member.FN, ifUserMove.member.UserID);

			lock( eventQueue )
			{
				if ( eventQueue.Count != 0 )
				{
					Array ifUserMoveList=Array.CreateInstance(typeof(iFolderUserMove), eventQueue.Count);
					eventQueue.CopyTo( ifUserMoveList, 0);
					foreach(iFolderUserMove ifUM in ifUserMoveList)
					{
						if(ifUM.member.UserID == ifUserMove.member.UserID)
						{
							log.Debug("IsUserAlreadyInQueue: User {0}:{1} is already in the queue", ifUserMove.member.FN, ifUserMove.member.UserID);
							return true;
						}
					}
				}
			}
			return false;
		}

                /// <summary>
                /// Adds user move request to the user move queue
                /// </summary>
                /// <param name="ifUserMove">iFolder user move object</param>
                /// <returns> True if user does not exists in the queue,
                /// false if the user is already in the queue.</returns>
		static public bool Add( iFolderUserMove ifUserMove )
		{
			log.Debug("Add: Adding User id {0}:{1} to the User Move Queue", ifUserMove.member.FN, ifUserMove.member.UserID);

			lock( eventQueue )
			{
				if ( eventQueue.Count != 0 )
				{
					Array ifUserMoveList=Array.CreateInstance(typeof(iFolderUserMove), eventQueue.Count);
					eventQueue.CopyTo( ifUserMoveList, 0);
					foreach(iFolderUserMove ifUM in ifUserMoveList)
					{
						if(ifUM.member.UserID == ifUserMove.member.UserID)
						{
							log.Debug("Add: User {0}:{1} is already in the queue", ifUserMove.member.FN, ifUserMove.member.UserID);
							return false;
						}
					}
				}
				eventQueue.Enqueue( ifUserMove );
				log.Debug("Add: User id {0} is included in the queue", ifUserMove.member.UserID);
				
			}
			return true;
		}


                /// <summary>
                /// Sets the Queue event for processing the user move queue
                /// </summary>
                /// <returns> nothing.</returns>
		static public void SetQueueEvent( )
		{
			log.Debug("SetQueueEvent: UserMove Queue Process event set");
			if(QueueProcessingState == false)
				queueEvent.Set();
		}


                /// <summary>
                /// Processes the User Move queue, either periodically or based on the manual reset event.
                /// </summary>
                /// <returns> Nothing.</returns>
		static private void ProcessEvents()
		{
                        store = Store.GetStore();
                        domain = store.GetDomain(store.DefaultDomain);
			iFolderUserMove ifUserMove = null;
			while( down == false )
			{
				queueEvent.WaitOne(QueEventScheduleTime * 1000 , false);
				if(down == true)
					break;
				try
				{
					iFolderUserMove.UpdateUserMoveQueue();
				}
				catch(Exception e)
				{
					log.Debug( "ProcessEvents: Update User Move Queue Failed {0}:{1}",e.Message, e.StackTrace);
				}
				QueueProcessingState = true;
				while( true )
				{
					ifUserMove = null;
					lock( eventQueue )
					{
						if ( eventQueue.Count == 0 )
						{
							queueEvent.Reset();
							break;
						}
						ifUserMove = eventQueue.Dequeue() as iFolderUserMove;
					}
					Member tmpmember = domain.GetMemberByID(ifUserMove.member.UserID);
					if(tmpmember != null)
						ifUserMove.member = tmpmember;
					try
					{
						if(!ifUserMove.ProcessMovement(domain.ID))
						{	
							Add(ifUserMove);
							Thread.Sleep(5 * 1000);
						}
					}
					catch(Exception e)
					{
						log.Debug( "ProcessEvents: Exception {0} {1} received for {2}",e.Message, e.StackTrace, ifUserMove.member.UserID );
						Add(ifUserMove);
						Thread.Sleep(30 * 1000);
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
			log.Debug( "Starting User Move service..." );
			lock( lockObj )
			{
				if ( started == true )
				{
					log.Info( "UserMove Service is already running." );
					return ;
				}
				down = false;
				eventQueue = new Queue();
				queueEvent = new AutoResetEvent( false );
				moveThread = new Thread( new ThreadStart( ProcessEvents ) );
				moveThread.IsBackground = true;
				moveThread.Name = "User Move thread";
				moveThread.Start();
				started = true;
				SetQueueEvent();
			}
			log.Debug( "User Move service started..." );
			DMove = new DataMove();
			DMove.Start();
		}

                /// <summary>
                /// Resumes a paused service.
                /// </summary>
                public void Resume()
                {
                }

                /// <summary>
                /// Pauses a service's execution.
                /// </summary>
                public void Pause()
                {
                }

                /// <summary>
                /// Custom.
                /// </summary>
                /// <param name="message"></param>
                /// <param name="data"></param>
                public int Custom(int message, string data)
                {
                        return 0;
                }



		/// <summary>
		/// Called to stop the user Move service.
		/// </summary>
		public void Stop()
		{
			if(DMove != null)
				DMove.Stop();
			down = true;
			queueEvent.Set();
			Thread.Sleep( 32 );
			if(QueueProcessingState == true)
				moveThread.Abort();	
			started = false;
			log.Debug( "User Move Service stopped..." );
		}

	}
}
