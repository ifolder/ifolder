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
		

		#region Constructor

		/// <summary>
		/// 
		/// </summary>
		/// <param name="conf"></param>
		/// <param name="serviceElement"></param>
		public ThreadServiceCtl(string assembly, XmlElement serviceElement) :
			base(assembly, serviceElement)
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
		public ThreadServiceCtl(string name, string assembly, string classType) :
			base(name, assembly)
		{
			this.classType = classType;
		}

		#endregion

		
		#region IServiceCtl members

		/// <summary>
		/// 
		/// </summary>
		public override void Start()
		{
			lock (typeof(ThreadServiceCtl))
			{
				// Load the assembly and start it.
				Assembly pAssembly = AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension(Assembly));
				service = (IThreadService)pAssembly.CreateInstance(classType);
				service.Start();
				state = Service.State.Running;
				Manager.logger.Info("\"{0}\" service started.", Name);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Stop()
		{
			lock (this)
			{
				service.Stop();
				service = null;
				state = Service.State.Stopped;
				Manager.logger.Info("\"{0}\" service stopped.", Name);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Kill()
		{
			lock (this)
			{
				service.Stop();
				service = null;
				state = Service.State.Stopped;
				Manager.logger.Info("\"{0}\" service killed.", Name);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Pause()
		{
			lock (this)
			{
				service.Pause();
				state = Service.State.Paused;
				Manager.logger.Info("\"{0}\" service paused.", Name);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Resume()
		{
			lock (this)
			{
				service.Resume();
				state = Service.State.Running;
				Manager.logger.Info("\"{0}\" service resumed.", Name);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public override void Custom(int message, string data)
		{
			service.Custom(message, data);
			Manager.logger.Info("\"{0}\" service message {1}.", Name, message);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		public override void ToXml(XmlElement element)
		{
			base.ToXml(element);
			element.SetAttribute(XmlClassAttr, classType);
		}

		/// <summary>
		/// Called to check if the service has exited.
		/// </summary>
		public override bool HasExited
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}
