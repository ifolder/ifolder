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
using System.Diagnostics;
using System.Collections;
using System.Xml;
using System.Threading;
using System.Reflection;
using Simias;

namespace Simias.Service
{
	/// <summary>
	/// System Manager
	/// </summary>
	public class Manager : IEnumerable
	{
		#region fields

		private Configuration conf;
		XmlElement servicesElement;
		ArrayList serviceList = new ArrayList();
		Mutex serviceMutex = null;
		const string CFG_Section = "ServiceManager";
		const string CFG_Services = "Services";
		const string XmlServiceTag = "Service";
		internal static string XmlTypeAttr = "type";
		internal static string XmlAssemblyAttr = "assembly";
		internal static string XmlEnabledAttr = "enabled";
		internal static string XmlNameAttr = "name";
		const string mutexBaseName = "ServiceManagerMutex___";

		#endregion
		
		#region Constructor

		/// <summary>
		/// Creates a Manager for the specified Configuration.
		/// </summary>
		/// <param name="conf">The Configuration location to manage.</param>
		public Manager(Configuration conf)
		{
			lock (this)
			{
				string mutexName = mutexBaseName + conf.StorePath.GetHashCode().ToString();
				Mutex mutex = new Mutex(false, mutexName);
				if (serviceMutex != null || mutex.WaitOne(200, false))
				{
					serviceMutex = mutex;
					this.conf = conf;
					// Get the XmlElement for the Services.
					servicesElement = conf.GetElement(CFG_Section, CFG_Services);

					XmlNodeList serviceNodes = servicesElement.SelectNodes(XmlServiceTag);
					foreach (XmlElement serviceNode in serviceNodes)
					{
						ServiceType sType = (ServiceType)Enum.Parse(typeof(ServiceType), serviceNode.GetAttribute(XmlTypeAttr));
						switch (sType)
						{
							case ServiceType.Process:
								serviceList.Add(new ProcessServiceCtl(conf, serviceNode));
								break;
							case ServiceType.Thread:
								serviceList.Add(new ThreadServiceCtl(conf, serviceNode));
								break;
						}
					}
					if (serviceList.Count == 0)
					{
						installDefaultServices();
					}
				}
				else
				{
					mutex.Close();
					throw new ApplicationException("Services Already running");
				}
			}
		}

		#endregion

		#region Installation methods.

		/// <summary>
		/// Installs the default services.
		/// </summary>
		private void installDefaultServices()
		{
			Install(new ProcessServiceCtl(conf, "Simias Service", "EventService.exe"));
			Install(new ThreadServiceCtl(conf, "File Watcher Service", "FsWatcher", "Simias.Event.FsWatcher"));
		}

		/// <summary>
		/// Installs the specified service at the specified index.
		/// </summary>
		/// <param name="svc">The service to install</param>
		/// <param name="index">The index to install at.</param>
		public void Install(ServiceCtl svc, int index)
		{
			lock (this)
			{
				if (GetService(svc.Name) == null)
				{
					if (index > serviceList.Count)
					{
						serviceList.Add(svc);
					}
					else
					{
						serviceList.Insert(index, svc);
					}
					XmlElement el = servicesElement.OwnerDocument.CreateElement(XmlServiceTag);
					svc.ToXml(el);
					XmlElement refEl = (XmlElement)servicesElement.FirstChild;
					while(index-- != 0 && refEl != null)
					{
						refEl = (XmlElement)refEl.NextSibling;
					}

					if (refEl != null)
					{
						servicesElement.InsertBefore(el, refEl);
					}
					else
					{
						servicesElement.AppendChild(el);
					}
					conf.SetElement(servicesElement);
				}
			}
		}

		/// <summary>
		/// Install the specified service.
		/// </summary>
		/// <param name="svc">The service to install.</param>
		public void Install(ServiceCtl svc)
		{
			Install(svc, serviceList.Count + 1);
		}

		/// <summary>
		/// Unistall the specified service.
		/// </summary>
		/// <param name="svcName">The name of the service.</param>
		public void Uninstall(string svcName)
		{
			lock (this)
			{
				serviceList.Remove(GetService(svcName));
				XmlElement el = (XmlElement)servicesElement.SelectSingleNode(string.Format("//{0}[@{1}='{2}']",XmlServiceTag, XmlNameAttr, svcName));
				if (el != null)
				{
					servicesElement.RemoveChild(el);
					conf.SetElement(servicesElement);
				}
			}
		}

		#endregion

		#region Control Methods.

		/// <summary>
		/// Start the installed services.
		/// </summary>
		public void StartServices()
		{
			lock (this)
			{
				string mutexName = mutexBaseName + conf.StorePath.GetHashCode().ToString();
				Mutex mutex = new Mutex(false, mutexName);
				if (serviceMutex != null || mutex.WaitOne(200, false))
				{
					serviceMutex = mutex;
					foreach (ServiceCtl svc in this)
					{
						try
						{
							svc.Start();
						}
						catch 
						{
							//log4net.LogManager.
						}
					}
				}
				else
				{
					mutex.Close();
					throw new ApplicationException("Services Already running");
				}
			}
		}

		/// <summary>
		/// Stop the installed services.
		/// </summary>
		public void StopServices()
		{
			lock (this)
			{
				foreach (ServiceCtl svc in this)
				{
					svc.Stop();
				}
			}
		}

		/// <summary>
		/// Get the named ServiceCtl object.
		/// </summary>
		/// <param name="name">The name of the service to get.</param>
		/// <returns>The ServiceCtl for the service.</returns>
		public ServiceCtl GetService(string name)
		{
			lock (this)
			{
				foreach (ServiceCtl svc in this)
				{
					if (svc.Name.Equals(name))
						return svc;
				}
				return null;
			}
		}

		#endregion

		#region Properties
		
		/// <summary>
		/// Get the configuration for this instance.
		/// </summary>
		public Configuration Config
		{
			get { return conf; }
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Gets the enumerator for the ServiceCtl objects.
		/// </summary>
		/// <returns>An enumerator.</returns>
		public IEnumerator GetEnumerator()
		{
			return serviceList.GetEnumerator();
		}

		#endregion
	}
}
