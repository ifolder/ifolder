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

namespace Simias.Service
{
	/// <summary>
	/// Summary description for ThreadService.
	/// </summary>
	public class ThreadServiceCtl : ServiceCtl
	{
		const string XmlClassAttr = "class";
		IThreadService	service = null;
		string			classType;
		
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

		#region IServiceCtl members

		/// <summary>
		/// 
		/// </summary>
		public override void Start()
		{
			lock (this)
			{
				if (state == State.Stopped && enabled)
				{
					// Load the assembly and start it.
					Assembly pAssembly = AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension(assembly));
					Type pType = null;
					Type[] types = pAssembly.GetExportedTypes();
					foreach (Type t in types)
					{
						if (t.FullName.Equals(classType))
						{
							pType = t;
							break;
						}
					}

					// If we did not find our type return a null.
					if (pType != null)
					{
						service = (IThreadService)pAssembly.CreateInstance(classType);
						service.Start(conf);
						state = State.Running;
					}
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
					service.Stop();
					service = null;
					state = State.Stopped;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Kill()
		{
			Stop();
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
					service.Pause();
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
					service.Resume();
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
			service.Custom(message, data);
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
