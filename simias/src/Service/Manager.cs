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

		static private ISimiasLog logger = Simias.SimiasLogManager.GetLogger(typeof(Manager));

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
		ManualResetEvent servicesStarted = new ManualResetEvent(false);
		ManualResetEvent servicesStopped = new ManualResetEvent(true);
		
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

					ServiceDelegate += new ServiceEvent(messageDispatcher);
					
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
					installDefaultServices();
				}
				else
				{
					mutex.Close();
					serviceList.Clear();
					throw new ApplicationException("Services Already running");
				}
			}
		}

		#endregion

		#region Message handling

		internal delegate void ServiceEvent(Message msg);
		internal ServiceEvent ServiceDelegate;
		
		private void postMessage(Message msg)
		{
			ServiceDelegate.BeginInvoke(msg, null, null);
		}

		private void messageDispatcher(Message msg)
		{
			ServiceCtl	svcCtl = msg.service;
			try
			{
				lock (this)
				{
					switch (msg.MajorMessage)
					{
						case MessageCode.Start:
							if ( (svcCtl.State == Simias.Service.State.Stopped) && svcCtl.Enabled)
							{
								svcCtl.Start();
								svcCtl.state = State.Running;
								logger.Info("\"{0}\" service started.", svcCtl.Name);
							}
							break;
						case MessageCode.Stop:
							if (svcCtl.state == State.Running || svcCtl.state == State.Paused)
							{
								svcCtl.Stop();
								svcCtl.state = State.Stopped;
								logger.Info("\"{0}\" service stopped.", svcCtl.Name);
							}
							break;
						case MessageCode.Pause:
							if (svcCtl.state == State.Running)
							{
								svcCtl.Pause();
								svcCtl.state = State.Paused;
								logger.Info("\"{0}\" service Paused.", svcCtl.Name);
							}
							break;
						case MessageCode.Resume:
							if (svcCtl.state == State.Paused)
							{
								svcCtl.Resume();
								svcCtl.state = State.Running;
								logger.Info("\"{0}\" service resumed.", svcCtl.Name);
							}
							break;
						case MessageCode.Custom:
							svcCtl.Custom(msg.CustomMessage, msg.Data);
							break;
						case MessageCode.StartComplete:
							servicesStarted.Set();
							servicesStopped.Reset();
							logger.Info("Services started.");
							break;
						case MessageCode.StopComplete:
							servicesStopped.Set();
							servicesStarted.Reset();
							logger.Info("Services stopped.");
							break;
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, ex.Message);
			}
		}
		
		#endregion

		#region Installation methods.

		/// <summary>
		/// Installs the default services.
		/// </summary>
		private void installDefaultServices()
		{
			Install(new ProcessServiceCtl(conf, "Simias Event Service", "EventService.exe"));
			Install(new ThreadServiceCtl(conf, "Simias File Monitor Service", "Simias", "Simias.Event.FsWatcher"));
			Install(new ThreadServiceCtl(conf, "Simias Dredge Service", "Simias", "Simias.Sync.DredgeService"));
			Install(new ThreadServiceCtl(conf, "Simias Sync Service", "Simias", "Simias.Sync.SyncManagerService"));
			Install(new ProcessServiceCtl(conf, "multi-cast DNS Service", "Simias.Service.mDnsService.exe"));
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
					logger.Info("{0} service installed", svc.Name);
				}
				else
				{
					// Replace the existing service
					lock (this)
					{
						for (int i =0; i < serviceList.Count; ++i)
						{
							ServiceCtl oldSvc = (ServiceCtl)serviceList[i];
							if (oldSvc.Name.Equals(svc.Name))
							{
								svc.enabled = oldSvc.enabled;
								Uninstall(oldSvc.Name);
								Install(svc, i);
								break;
							}
						}
					}
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
					logger.Info("{0} service uninstalled", svcName);
				}
				else
				{
					logger.Warn("{0} service not installed", svcName);
				}
			}
		}

		#endregion

		#region Control Methods.

		/// <summary>
		/// Start the installed services.
		/// This call is asynchronous. Use ServicesStarted to now when this call has finished.
		/// </summary>
		public void StartServices()
		{
			lock (this)
			{
				foreach (ServiceCtl svc in this)
				{
					if (svc.State == State.Stopped)
					{
						postMessage(new StartMessage(svc));
					}
				}
				postMessage(new StartComplete());
			}
		}

		/// <summary>
		/// Stop the installed services.
		/// This call is asynchronous. Use ServicesStarted to now when this call has finished.
		/// </summary>
		public void StopServices()
		{
			lock (this)
			{
				for (int i = serviceList.Count; i > 0; --i)
				{
					ServiceCtl svc = (ServiceCtl)serviceList[i-1];
					postMessage(new StopMessage(svc));
				}
				postMessage(new StopComplete());
			}
		}

		/// <summary>
		/// Block until services are started.
		/// </summary>
		public void WaitForServicesStarted()
		{
			servicesStarted.WaitOne();
		}

		/// <summary>
		/// Block until services are stoped.
		/// </summary>
		public void WaitForServicesStopped()
		{
			servicesStopped.WaitOne();
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

		/// <summary>
		/// Gets the started state of the services.
		/// </summary>
		public bool ServiceStarted
		{
			get { return servicesStarted.WaitOne(0, false); }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool ServicesStopped
		{
			get { return servicesStopped.WaitOne(0, false); }
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
