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
using Simias.Storage;
using Simias.Client;
using Simias.Client.Event;

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

		private static Manager instance;
		static private Configuration conf;

		static internal string XmlTypeAttr = "type";
		static internal string XmlAssemblyAttr = "assembly";
		static internal string XmlEnabledAttr = "enabled";
		static internal string XmlNameAttr = "name";
		static internal string MutexBaseName = "ServiceManagerMutex___";

		private Thread startThread = null;
		private Thread stopThread = null;
		private XmlElement servicesElement;
		private ArrayList serviceList = new ArrayList();

		private const string CFG_Section = "ServiceManager";
		private const string CFG_Services = "Services";
		private const string XmlServiceTag = "Service";

		private ManualResetEvent servicesStarted = new ManualResetEvent(false);
		private ManualResetEvent servicesStopped = new ManualResetEvent(true);
		private DefaultSubscriber	subscriber = null;
		static private int thresholdTimeLimit = 10;
		static private float growthLimit = 80;

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
		/// <param name="config">The Configuration location to manage.</param>
		public Manager(Configuration config)
		{
			lock(typeof(Manager))
			{
				if (instance != null)
					throw new SimiasException("Only one instance of Manager is allowed.");

				instance = this;
			}

			// configure
			SimiasLogManager.Configure(config);

			// Get an event subscriber to handle shutdown events.
			subscriber = new DefaultSubscriber();
			subscriber.SimiasEvent +=new SimiasEventHandler(OnSimiasEvent);

			lock (this)
			{
				conf = config;

				// Get the XmlElement for the Services.
				servicesElement = config.GetElement(CFG_Section, CFG_Services);

				XmlNodeList serviceNodes = servicesElement.SelectNodes(XmlServiceTag);
				foreach (XmlElement serviceNode in serviceNodes)
				{
					ServiceType sType = (ServiceType)Enum.Parse(typeof(ServiceType), serviceNode.GetAttribute(XmlTypeAttr));
					switch (sType)
					{
						case ServiceType.Process:
							serviceList.Add(new ProcessServiceCtl(config, serviceNode));
							break;
						case ServiceType.Thread:
							serviceList.Add(new ThreadServiceCtl(config, serviceNode));
							break;
					}
				}

				installDefaultServices();

				// TODO: Remove when mono compacts the heap.
				if (config.Exists("ClientRollOver", "GrowthLimit"))
				{
					string tempString = config.Get( "ClientRollOver", "GrowthLimit", null);
					growthLimit = Convert.ToSingle( tempString );
				}

				if (config.Exists("ClientRollOver", "ThresholdTime"))
				{
					string tempString = config.Get("ClientRollOver", "ThresholdTime", null);
					thresholdTimeLimit = Convert.ToInt32( tempString );
				}
				// TODO: End

				// Start a monitor thread to keep the services running.
				Thread mThread = new Thread(new ThreadStart(Monitor));
				mThread.IsBackground = true;
				mThread.Start();
			}
		}

		#endregion

		#region Callbacks

		private void OnSimiasEvent(SimiasEventArgs args)
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
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, ex.Message);
			}
		}
		
		#endregion

		#region Monitor

		/// <summary>
		/// Hack used to get the virtual memory size of the SimiasApp process. This
		/// is only used on Linux to determine if the SimiasApp process needs to be
		/// restarted in order to get around the non-compacting heap problem on mono.
		/// TODO: Can be removed when mono compacts the heap.
		/// </summary>
		/// <returns>The amount of memory in bytes consumed by the SimiasApp process.</returns>
		private float GetSimiasMemorySize()
		{
			float memSize = 1;

			try
			{
				// First entry will be the virtual memory size in pages.
				using ( StreamReader sr = new StreamReader( "/proc/self/statm" ) )
				{
					String[] lines = sr.ReadLine().Split( null );
					memSize = Convert.ToSingle( lines[ 0 ].Trim() ) * 4096;
				}
			}
			catch
			{}

			return memSize;
		}

		private void Monitor()
		{
			// TODO: This can be removed when mono compacts the heap.
			bool isClient = !Store.GetStore().IsEnterpriseServer;
			DateTime thresholdTime = DateTime.Now;
			float initialMemorySize = 0;
			if ( MyEnvironment.Mono && isClient )
			{
				Thread.Sleep( 1000 * 60 );
				thresholdTime += new TimeSpan(0, thresholdTimeLimit, 0);
				initialMemorySize = GetSimiasMemorySize();
				logger.Debug( "Intialize memory size = {0} KB", initialMemorySize / 1024 );
			}
			// TODO: End

			while (true)
			{
				try
				{
					lock (this)
					{
						// Make sure all running processes are still running.
						foreach (ServiceCtl svc in serviceList)
						{
							if (svc.state == State.Running && svc.HasExited)
							{
								// The service has exited. Restart it.
								logger.Info("\"{0}\" service exited.  Restarting ....", svc.Name);
								svc.state = State.Stopped;
								svc.Start();
							}
						}
					}

					// TODO: This can be removed when mono compacts the heap.
					// Check how much memory is being used by the process.
					if (MyEnvironment.Mono && isClient)
					{
						float delta = GetSimiasMemorySize() - initialMemorySize;
						if ( delta > 0 )
						{
							float growthPercentage = (delta / initialMemorySize) * 100;
							logger.Debug("Simias memory growth = {0}%", growthPercentage);
							if (growthPercentage > growthLimit)
							{
								// See if the memory useage has been up for a sufficient period of time.
								if ( DateTime.Now >= thresholdTime )
								{
									// Send out an event that will cause the application to restart.
									logger.Info( "Hit memory threshold. Restarting SimiasApp." );
									EventPublisher publisher = new EventPublisher();
									publisher.RaiseEvent(new NotifyEventArgs("Simias-Restart", "The application is restarting.", DateTime.Now));
								}
							}
							else
							{
								// The memory has fallen below the threshold. Restart the time.
								thresholdTime = DateTime.Now + new TimeSpan(0, thresholdTimeLimit, 0);
							}
						}
						else
						{
							// The memory has fallen below the threshold. Restart the time.
							thresholdTime = DateTime.Now + new TimeSpan(0, thresholdTimeLimit, 0);
						}
					}
					// TODO: End
				}
				catch
				{
				}

				Thread.Sleep(1000 * 60);
			}
		}

		#endregion

		#region Installation methods.

		/// <summary>
		/// Installs the default services.
		/// </summary>
		private void installDefaultServices()
		{
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
								InternalUninstall(oldSvc.Name);
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
			if (InternalUninstall(svcName))
			{
				logger.Info("{0} service uninstalled", svcName);
			}
			else
			{
				logger.Warn("{0} service not installed", svcName);
			}
		}

		/// <summary>
		/// Unistall the specified service.
		/// </summary>
		/// <param name="svcName">The name of the service.</param>
		private bool InternalUninstall(string svcName)
		{
			bool bStatus = false;
			lock (this)
			{
				serviceList.Remove(GetService(svcName));
				XmlElement el = (XmlElement)servicesElement.SelectSingleNode(string.Format("//{0}[@{1}='{2}']",XmlServiceTag, XmlNameAttr, svcName));
				if (el != null)
				{
					servicesElement.RemoveChild(el);
					conf.SetElement(CFG_Section, CFG_Services, servicesElement);
					bStatus = true;
				}
			}
			return bStatus;
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
					messageDispatcher(new StartMessage(svc));
				}
			}
			servicesStopped.Reset();
			servicesStarted.Set();
			logger.Info("Services started.");
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
				messageDispatcher(new StopMessage(svc));
			}
			servicesStarted.Reset();
			servicesStopped.Set();
			logger.Info("Services stopped.");
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
		public static ServiceCtl GetService(string name)
		{
			lock (instance)
			{
				foreach (ServiceCtl svc in instance)
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
