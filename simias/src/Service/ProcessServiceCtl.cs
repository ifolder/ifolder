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
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using Simias.Client;
using Simias.Storage;

namespace Simias.Service
{
	/// <summary>
	/// Summary description for ProcessService.
	/// </summary>
	public class ProcessServiceCtl : ServiceCtl
	{
		System.Diagnostics.Process process = null;
		StreamWriter writer;
		StreamReader reader;
		static internal string Error = "ERROR";
		
		#region Constructor

		internal ProcessServiceCtl(XmlElement serviceElement) :
			base(serviceElement)
		{
		}

		/// <summary>
		/// Construct a ProcessServiceCtl.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="assembly"></param>
		public ProcessServiceCtl(string name, string assembly) :
			base(name, assembly)
		{
		}

		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		void postMessage(Message msg)
		{
			writer.WriteLine(msg.ToString());
			string line;
			do
			{
				line = reader.ReadLine();
				if (line != null)
				{
					string [] results = line.Split(Message.seperator, 2);
                    if (results[0].Equals(Message.messageSignature))
					{
						if (results.Length == 2 && results[1].Equals(Error))
						{
							throw new SimiasException(string.Format("\"{0}\" service failed {1} request", msg.service.name, msg.MajorMessage.ToString()));
						}
						break;
					}
				}
				else break;
			} while (true);
		}

		#region ServiceCtl members

		/// <summary>
		/// 
		/// </summary>
		public override void Start()
		{
			lock (this)
			{
				if (state == State.Stopped && enabled)
				{
					// The service is not running start it.
					process = new Process();
					process.StartInfo.RedirectStandardInput = true;
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.CreateNoWindow = true;
					process.StartInfo.UseShellExecute = false;
					if (MyEnvironment.Mono)
					{
						process.StartInfo.FileName = "mono";
						process.StartInfo.Arguments = assembly + " ";
					}
					else
					{
						process.StartInfo.FileName = assembly;
						process.StartInfo.Arguments = null;
					}
					process.StartInfo.Arguments += "\"" + Store.StorePath +"\"";
					process.Start();
					reader = process.StandardOutput;
					writer = process.StandardInput;
					postMessage(new StartMessage(this));
					state = Service.State.Running;
					Manager.logger.Info("\"{0}\" service started.", Name);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Stop()
		{
			lock (this)
			{
				if (state == State.Running || state == State.Paused)
				{
					postMessage(new StopMessage(this));
					if (process.WaitForExit(20000))
					{
						process = null;
						reader = null;
						writer = null;
						state = Service.State.Stopped;
						Manager.logger.Info("\"{0}\" service stopped.", Name);
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Kill()
		{
			lock (this)
			{
				if (process != null)
				{
					process.Kill();
					if (process.WaitForExit(20000))
					{
						process = null;
						state = Service.State.Stopped;
						Manager.logger.Info("\"{0}\" service killes.", Name);
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Pause()
		{
			lock (this)
			{
				if (state == State.Running)
				{
					postMessage(new PauseMessage(this));
					state = Service.State.Paused;
					Manager.logger.Info("\"{0}\" service paused.", Name);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Resume()
		{
			lock (this)
			{
				if (state == State.Paused)
				{
					postMessage(new ResumeMessage(this));
					state = Service.State.Running;
					Manager.logger.Info("\"{0}\" service resumed.", Name);
				}
			}

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public override void Custom(int message, string data)
		{
			lock (this)
			{
				postMessage(new CustomMessage(this, message, data));
				Manager.logger.Info("\"{0}\" service message {1}.", Name, message);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		public override void ToXml(XmlElement element)
		{
			base.ToXml(element);
			element.SetAttribute(Manager.XmlTypeAttr, ServiceType.Process.ToString());
		}

		/// <summary>
		/// Called to check if the Service has exited.
		/// </summary>
		public override bool HasExited
		{
			get
			{
				if (process != null)
				{
					return process.HasExited;
				}
				else
				{
					return false;
				}
			}
		}


		#endregion
	}
}
