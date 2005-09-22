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
		/// The service name.
		/// </summary>
		internal string		name;
		/// <summary>
		/// The assembly the service is in.
		/// </summary>
		internal string		assembly;
		/// <summary>
		/// Used to enable or disable the service.
		/// </summary>
		internal bool			enabled = true;
		/// <summary>
		/// The running state of the service.
		/// </summary>
		internal State			state = State.Stopped;
		
		/// <summary>
		/// Initializes the Service Control object.
		/// </summary>
		/// <param name="serviceElement">XML element that describes the service.</param>
		protected ServiceCtl(XmlElement serviceElement)
		{
			name = serviceElement.GetAttribute(Manager.XmlNameAttr);
			assembly = serviceElement.GetAttribute(Manager.XmlAssemblyAttr);
			enabled = bool.Parse(serviceElement.GetAttribute(Manager.XmlEnabledAttr));
		}

		/// <summary>
		/// Initializes the Service Control object.
		/// </summary>
		/// <param name="name">The name of ther service.</param>
		/// <param name="assembly">The assembly where the service exists.</param>
		protected ServiceCtl(string name, string assembly)
		{
			this.name = name;
			this.assembly = assembly;
		}
			
		/// <summary>
		/// Called to start the service.
		/// </summary>
		public abstract void Start();
		/// <summary>
		/// Called to stop the service.
		/// </summary>
		public abstract void Stop();
		/// <summary>
		/// Called to Kill the service. Stop should be used instead.
		/// This will force the service down.
		/// </summary>
		public abstract void Kill();
		/// <summary>
		/// Called to pause the service.
		/// </summary>
		public abstract void Pause();
		/// <summary>
		/// Called to resume a paused service.
		/// </summary>
		public abstract void Resume();
		/// <summary>
		/// Called to send a custom control to the service.
		/// </summary>
		/// <param name="message">The message for the service.</param>
		/// <param name="data">The data of the message.</param>
		public abstract void Custom(int message, string data);

		/// <summary>
		/// Sets the service parameters in the XML element.
		/// </summary>
		/// <param name="element">The element to initialize.</param>
		public virtual void ToXml(XmlElement element)
		{
			element.SetAttribute(Manager.XmlNameAttr, name);
			element.SetAttribute(Manager.XmlAssemblyAttr, assembly);
			element.SetAttribute(Manager.XmlEnabledAttr, enabled.ToString());
		}
		
		/// <summary>
		/// Gets the service name.
		/// </summary>
		public string Name
		{
			get { return name;}
		}

		/// <summary>
		/// Gets the assembly that contains this service.
		/// </summary>
		public string Assembly
		{
			get { return assembly;}
		}

		/// <summary>
		/// Gets the running state of the service.
		/// </summary>
		public State State
		{
			get { return state;}
		}

		/// <summary>
		/// Gets or Sets the enabled state of the service.
		/// </summary>
		public bool Enabled
		{
			get { return enabled;}
			set { enabled = value;}
		}

		/// <summary>
		/// Called to check if the service has exited.
		/// </summary>
		public abstract bool HasExited
		{
			get;
		}
	}
}
