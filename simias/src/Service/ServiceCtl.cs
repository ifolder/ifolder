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
using System.Xml;

namespace Simias.Service
{
	/// <summary>
	/// Used to control a Simias Service.
	/// </summary>
	public abstract class ServiceCtl
	{
		/// <summary>
		/// 
		/// </summary>
		protected string		name;
		/// <summary>
		/// 
		/// </summary>
		protected string		assembly;
		/// <summary>
		/// 
		/// </summary>
		protected Configuration	conf;
		/// <summary>
		/// 
		/// </summary>
		protected bool			enabled = true;
		/// <summary>
		/// 
		/// </summary>
		protected State			state = State.Stopped;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="conf"></param>
		/// <param name="serviceElement"></param>
		protected ServiceCtl(Configuration conf, XmlElement serviceElement)
		{
			this.conf = conf;
			name = serviceElement.GetAttribute(Manager.XmlNameAttr);
			assembly = serviceElement.GetAttribute(Manager.XmlAssemblyAttr);
			enabled = bool.Parse(serviceElement.GetAttribute(Manager.XmlEnabledAttr));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="conf"></param>
		/// <param name="name"></param>
		/// <param name="assembly"></param>
		protected ServiceCtl(Configuration conf, string name, string assembly)
		{
			this.conf = conf;
			this.name = name;
			this.assembly = assembly;
		}
			
		/// <summary>
		/// 
		/// </summary>
		public abstract void Start();
		/// <summary>
		/// 
		/// </summary>
		public abstract void Stop();
		/// <summary>
		/// 
		/// </summary>
		public abstract void Kill();
		/// <summary>
		/// 
		/// </summary>
		public abstract void Pause();
		/// <summary>
		/// 
		/// </summary>
		public abstract void Resume();
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public abstract void Custom(int message, string data);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		public virtual void ToXml(XmlElement element)
		{
			element.SetAttribute(Manager.XmlNameAttr, name);
			element.SetAttribute(Manager.XmlAssemblyAttr, assembly);
			element.SetAttribute(Manager.XmlEnabledAttr, enabled.ToString());
		}
		
		/// <summary>
		/// 
		/// </summary>
		public string Name
		{
			get { return name;}
		}

		/// <summary>
		/// 
		/// </summary>
		public string Assembly
		{
			get { return assembly;}
		}

		/// <summary>
		/// 
		/// </summary>
		public State State
		{
			get { return state;}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool Enable
		{
			get { return enabled;}
			set { enabled = value;}
		}
	}
}
