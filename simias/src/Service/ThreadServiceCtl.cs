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
using System.Collections;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Threading;

namespace Simias.Service
{
	/// <summary>
	/// Summary description for ThreadService.
	/// </summary>
	public class ThreadServiceCtl : ServiceCtl
	{
		const string XmlClassAttr = "class";
		IThreadService			service = null;
		string					classType;
		static Queue			messageQueue = new Queue();
		static ManualResetEvent	queueEvent = new ManualResetEvent(false);
		static Thread			messageThread = null;

		class SvcMessage : Message
		{
			internal ThreadServiceCtl service;

			internal SvcMessage(ThreadServiceCtl service, MessageCode message) :
				this(service, message, 0, "")
			{
			}

			internal SvcMessage(ThreadServiceCtl service, MessageCode message, int customMsg, string data) :
				base(message, customMsg, data)
			{
				this.service = service;
			}
		}
		
		#region Constructor

		/// <summary>
		/// 
		/// </summary>
		/// <param name="conf"></param>
		/// <param name="serviceElement"></param>
		public ThreadServiceCtl(Configuration conf, XmlElement serviceElement) :
			base(conf, serviceElement)
		{
			classType = serviceElement.GetAttribute(XmlClassAttr);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="conf"></param>
		/// <param name="name"></param>
		/// <param name="assembly"></param>
		/// <param name="classType"></param>
		public ThreadServiceCtl(Configuration conf, string name, string assembly, string classType) :
			base(conf, name, assembly)
		{
			this.classType = classType;
		}

		#endregion

		private void postMessage(SvcMessage msg)
		{
			lock (messageQueue)
			{
				messageQueue.Enqueue(msg);
				queueEvent.Set();
			}
		}

		private static void messageDispatcher()
		{
			SvcMessage msg;
			ThreadServiceCtl svcCtl;
			while (true)
			{	
				try
				{
					queueEvent.WaitOne();
					lock (messageQueue)
					{
						queueEvent.Reset();
						msg = (SvcMessage)messageQueue.Dequeue();
						svcCtl = msg.service;
					}
					switch (msg.MajorMessage)
					{
						case MessageCode.Start:
							if (svcCtl.State == State.Stopped && svcCtl.Enabled)
							{
								// Load the assembly and start it.
								Assembly pAssembly = AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension(svcCtl.Assembly));
								svcCtl.service = (IThreadService)pAssembly.CreateInstance(svcCtl.classType);
								svcCtl.service.Start(svcCtl.conf);
								svcCtl.state = State.Running;
							}
							break;
						case MessageCode.Stop:
							if (svcCtl.state == State.Running || svcCtl.state == State.Paused)
							{
								svcCtl.service.Stop();
								svcCtl.state = State.Stopped;
								svcCtl.service = null;
							}
							break;
						case MessageCode.Pause:
							if (svcCtl.state == State.Running)
							{
								svcCtl.service.Pause();
								svcCtl.state = State.Paused;
							}
							break;
						case MessageCode.Resume:
							if (svcCtl.state == State.Paused)
							{
								svcCtl.service.Resume();
								svcCtl.state = State.Running;
							}
							break;
						case MessageCode.Custom:
							svcCtl.service.Custom(msg.CustomMessage, msg.Data);
							break;
					}
				}
				catch {}
			}
		}

		#region IServiceCtl members

		/// <summary>
		/// 
		/// </summary>
		public override void Start()
		{
			lock (typeof(ThreadServiceCtl))
			{
				if (messageThread == null)
				{
					messageThread = new Thread(new ThreadStart(messageDispatcher));
					messageThread.IsBackground = true;
					messageThread.Start();
				}

				postMessage(new SvcMessage(this, MessageCode.Start));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Stop()
		{
			lock (this)
			{
				postMessage(new SvcMessage(this, MessageCode.Stop));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Kill()
		{
			lock (this)
			{
				postMessage(new SvcMessage(this, MessageCode.Stop));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Pause()
		{
			lock (this)
			{
				postMessage(new SvcMessage(this, MessageCode.Pause));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Resume()
		{
			lock (this)
			{
				postMessage(new SvcMessage(this, MessageCode.Resume));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public override void Custom(int message, string data)
		{
			postMessage(new SvcMessage(this, MessageCode.Custom, message, data));
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		public override void ToXml(XmlElement element)
		{
			base.ToXml(element);
			element.SetAttribute(Manager.XmlTypeAttr, ServiceType.Thread.ToString());
			element.SetAttribute(XmlClassAttr, classType);
		}

		#endregion
	}
}
