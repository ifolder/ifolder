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
using Simias.Event;

namespace Simias.Service
{
	/// <summary>
	/// System Manager
	/// </summary>
	public class Manager : IEnumerable
	{
		#region fields

		/// <summary>
		/// Used to log service events.
		/// </summary>
		static public ISimiasLog logger = Simias.SimiasLogManager.GetLogger(typeof(Manager));

		Thread startThread = null;
		Thread stopThread = null;
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
		internal static string MutexBaseName = "ServiceManagerMutex___";
		ManualResetEvent servicesStarted = new ManualResetEvent(false);
		ManualResetEvent servicesStopped = new ManualResetEvent(true);
		DefaultSubscriber	subscriber = null;

		#region Events
		/// <summary>
		/// Delegate to handle Shutdown events.
		/// </summary>
		public event ShutdownEventHandler Shutdown;
		
		#endregion

		#endregion
		
		#region Constructor

		/// <summary>
		/// Creates a Manager for the specified Configuration.
		/// </summary>
		/// <param name="conf">The Configuration location to manage.</param>
		public Manager(Configuration conf)
		{
			// Get an event subscriber to handle shutdown events.
			subscriber = DefaultSubscriber.GetDefaultSubscriber(conf);
			subscriber.CollectionEvent +=new CollectionEventHandler(OnCollectionEvent);

			// configure logging
			SimiasLogManager.Configure(conf);

			lock (this)
			{
				string mutexName = MutexBaseName + conf.StorePath.GetHashCode().ToString();
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

		#region Callbacks

		private void OnCollectionEvent(SimiasEventArgs args)
		{
			try
			{
				string typeString = args.GetType().ToString();
				switch (typeString)
				{
					case "Simias.Service.ShutdownEventArgs":
						if (Shutdown != null)
							Shutdown((ShutdownEventArgs)args);
						break;
				}
			}
			catch (Exception ex)
			{
				new SimiasException(args.ToString(), ex);
			}
		}

		#endregion

		#region Message handling

		internal delegate void ServiceEvent(Message msg);
		internal ServiceEvent ServiceDelegate;
		
		private void postMessage(Message msg)
		{
			ServiceDelegate(msg);
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
							}
							break;
						case MessageCode.Stop:
							if (svcCtl.state == State.Running || svcCtl.state == State.Paused)
							{
								svcCtl.Stop();
							}
							break;
						case MessageCode.Pause:
							if (svcCtl.state == State.Running)
							{
								svcCtl.Pause();
							}
							break;
						case MessageCode.Resume:
							if (svcCtl.state == State.Paused)
							{
								svcCtl.Resume();
							}
							break;
						case MessageCode.Custom:
							svcCtl.Custom(msg.CustomMessage, msg.Data);
							break;
						case MessageCode.StartComplete:
							servicesStopped.Reset();
							servicesStarted.Set();
							logger.Info("Services started.");
							break;
						case MessageCode.StopComplete:
							servicesStarted.Reset();
							servicesStopped.Set();
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
			Install(new ProcessServiceCtl(conf, "Simias Event Service", "EventService.exe"), 0);
			//Install(new ThreadServiceCtl(conf, "Simias File Monitor Service", "Simias", "Simias.Event.FsWatcher"), 1);
			Install(new ThreadServiceCtl(conf, "Simias Change Log Service", "Simias", "Simias.Storage.ChangeLog"), 2);
			Install(new ThreadServiceCtl(conf, "Simias Dredge Service", "Simias", "Simias.Sync.DredgeService"), 3);
			Install(new ProcessServiceCtl(conf, "Simias Multi-Cast DNS Service", "mDnsService.exe"), 4);
			Install(new ProcessServiceCtl(conf, "Simias Sync Service", "SyncService.exe"));
			Install(new ProcessServiceCtl(conf, "Simias PO Service", "POService.exe"));
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
					conf.SetElement(CFG_Section, CFG_Services, servicesElement);
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
								if (index < i)
									Install(svc, index);
								else
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
					conf.SetElement(CFG_Section, CFG_Services, servicesElement);
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
				startThread = new Thread(new ThreadStart(StartServicesThread));
				startThread.Start();
			}
		}

		private void StartServicesThread()
		{
			foreach (ServiceCtl svc in this)
			{
				if (svc.State == State.Stopped)
				{
					postMessage(new StartMessage(svc));
				}
			}
			postMessage(new StartComplete());
			startThread = null;
		}
	
		/// <summary>
		/// Stop the installed services.
		/// This call is asynchronous. Use ServicesStarted to now when this call has finished.
		/// </summary>
		public void StopServices()
		{
			lock (this)
			{
				stopThread = new Thread(new ThreadStart(StopServicesThread));
				stopThread.Start();
			}
		}

		private void StopServicesThread()
		{
			for (int i = serviceList.Count; i > 0; --i)
			{
				ServiceCtl svc = (ServiceCtl)serviceList[i-1];
				postMessage(new StopMessage(svc));
			}
			postMessage(new StopComplete());
			stopThread = null;
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

	#region Delegate Definitions.

	/// <summary>
	/// Delegate definition for handling shutdown events.
	/// </summary>
	public delegate void ShutdownEventHandler(ShutdownEventArgs args);

	#endregion

	/// <summary>
	/// Event class for shutdown requests.
	/// </summary>
	[Serializable]
	public class ShutdownEventArgs : SimiasEventArgs
	{
	}
}
