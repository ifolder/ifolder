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

namespace Simias.Service
{
	/// <summary>
	/// Process services must inherit from this class.
	/// </summary>
	public abstract class BaseProcessService
	{
		#region Fields

		State						state = State.Stopped;
		ManualResetEvent			shutdownEvent = new ManualResetEvent(false);
		Configuration				conf;

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
			
			conf = new Configuration(confPath);
		}

		#endregion

		#region Helper Methods used by the service

		/// <summary>
		/// Called to get the Simias.Configuration object.
		/// </summary>
		/// <returns>The configuration for the service.</returns>
		protected Configuration GetConfiguration()
		{
			return conf;
		}

		/// <summary>
		/// Called to start the service.
		/// </summary>
		protected void Run()
		{
			Thread t1 = new Thread(new ThreadStart(MessageDispatcher));
			t1.IsBackground = true;
			t1.Start();

			// Wait for shutdown.
			shutdownEvent.WaitOne();
		}

		#endregion

		#region Private methods

		/// <summary>
		/// This is the message thread. The messages are dispatched to the
		/// proper method.
		/// </summary>
		private void MessageDispatcher()
		{
			while (true)
			{	
				try
				{
					string line;
					line = Console.ReadLine();
					Message msg = new Message(line);
					switch (msg.MajorMessage)
					{
						case MessageCode.Start:
							start();
							sendComplete(msg);
							break;
						case MessageCode.Stop:
							stop();
							sendComplete(msg);
							break;
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
				catch {}
			}
		}

		/// <summary>
		/// Called to signal that the messag has been consumed.
		/// </summary>
		/// <param name="msg"></param>
		private void sendComplete(Message msg)
		{
			Console.WriteLine(msg.GetType().ToString());
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
					shutdownEvent.Set();
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
