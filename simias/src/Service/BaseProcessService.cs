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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using Simias.Storage;

namespace Simias.Service
{
	/// <summary>
	/// Process services must inherit from this class.
	/// </summary>
	public abstract class BaseProcessService
	{
		#region Fields

		State						state = State.Stopped;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseProcessService()
		{
			string confPath = Environment.GetCommandLineArgs()[1];
		
			if (confPath == null || confPath.Length == 0 || !Directory.Exists(confPath))
			{
				Console.WriteLine("Usage: (service) (Configuration Path)");
				throw new ApplicationException("Invalid Service Arguments");
			}
			
            // configure logging
            SimiasLogManager.Configure(Store.StorePath);
		}

		#endregion

		#region Helper Methods used by the service

		/// <summary>
		/// Called to start the service.
		/// </summary>
		protected void Run()
		{
			MessageDispatcher();
		}

		#endregion

		#region Private methods

		/// <summary>
		/// This is the message thread. The messages are dispatched to the
		/// proper method.
		/// </summary>
		private void MessageDispatcher()
		{
			Message msg = null;
			while (true)
			{	
				try
				{
					string line;
					line = Console.ReadLine();
					if (line != null)
					{
						msg = new Message(null, line);
						switch (msg.MajorMessage)
						{
							case MessageCode.Start:
								start();
								sendComplete(msg);
								break;
							case MessageCode.Stop:
								stop();
								sendComplete(msg);
								return;
							case MessageCode.Pause:
								pause();
								sendComplete(msg);
								break;
							case MessageCode.Resume:
								resume();
								sendComplete(msg);
								break;
							case MessageCode.Custom:
								custom(msg.CustomMessage, msg.Data);
								sendComplete(msg);
								break;
						}
					}
					else
					{
						try
						{
							// The controling process has gone away, stop the service.
							stop();
						}
						catch {}
						return;
					}
				}
				catch
				{
					if (msg != null)
					{
						sendError(msg);
						msg = null;
					}
				}
			}
		}

		/// <summary>
		/// Called to signal that the messag has been consumed.
		/// </summary>
		/// <param name="msg"></param>
		private void sendComplete(Message msg)
		{
			Console.WriteLine(Message.messageSignature);
		}

		/// <summary>
		/// Called to signal that the messag has been consumed with an error.
		/// </summary>
		/// <param name="msg"></param>
		private void sendError(Message msg)
		{
			Console.WriteLine("{0};{1}", Message.messageSignature, ProcessServiceCtl.Error);
		}

		/// <summary>
		/// Processes a MessageCode.Start.
		/// </summary>
		private void start()
		{
			lock (this)
			{
				state = State.Running;
				// Call the start method on the service.
				Start();
			}
		}

		/// <summary>
		/// Processes a MessageCode.Stop.
		/// </summary>
		private void stop()
		{
			lock (this)
			{
				if (state == State.Running || state == State.Paused)
				{
					Stop();
					state = State.Stopped;
				}
			}
		}

		/// <summary>
		/// Processes a MessageCode.Pause.
		/// </summary>
		private void pause()
		{
			lock (this)
			{
				Pause();
				state = State.Paused;
			}
		}

		/// <summary>
		/// Processes a MessageCode.Resume.
		/// </summary>
		private void resume()
		{
			lock (this)
			{
				Resume();
				state = State.Running;
			}
		}

		/// <summary>
		/// Processes custom messages.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		private void custom(int message, string data)
		{
			lock (this)
			{
				Custom(message, data);
			}
		}

		#endregion

		#region Methods that must be implemented by the service.

		/// <summary>
		/// Called to start the service.
		/// </summary>
		protected abstract void Start();
		/// <summary>
		/// Called to stop the service.
		/// </summary>
		protected abstract void Stop();
		/// <summary>
		/// Called to pause the service.
		/// </summary>
		protected abstract void Pause();
		/// <summary>
		/// Called to resume the service.
		/// </summary>
		protected abstract void Resume();
		/// <summary>
		/// Called to control the service.  This is service defines behavior.
		/// </summary>
		/// <param name="message">The custom message number.</param>
		/// <param name="data">A string that can have extra data.</param>
		protected abstract void Custom(int message, string data);

		#endregion
	}
}
