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
		
		#region Constructor

		internal ProcessServiceCtl(Configuration conf, XmlElement serviceElement) :
			base(conf, serviceElement)
		{
		}

		/// <summary>
		/// Construct a ProcessServiceCtl.
		/// </summary>
		/// <param name="conf"></param>
		/// <param name="name"></param>
		/// <param name="assembly"></param>
		public ProcessServiceCtl(Configuration conf, string name, string assembly) :
			base(conf, name, assembly)
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
					if (line.Equals(msg.GetType().ToString()))
					{
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
					process.StartInfo.Arguments += "\"" + conf.StorePath +"\"";
					process.Start();
					reader = process.StandardOutput;
					writer = process.StandardInput;
					postMessage(new Message(MessageCode.Start, 0, ""));
					state = State.Running;
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
					postMessage(new Message(MessageCode.Stop, 0, ""));
					if (process.WaitForExit(20000))
					{
						process = null;
						reader = null;
						writer = null;
						state = State.Stopped;
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
						state = State.Stopped;
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
					postMessage(new Message(MessageCode.Pause, 0, ""));
					state = State.Paused;
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
					postMessage(new Message(MessageCode.Resume, 0, ""));
					state = State.Running;
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
				postMessage(new Message(MessageCode.Custom, message, data));
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

		#endregion
	}
}
