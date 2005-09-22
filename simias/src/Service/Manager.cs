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
#if CLIENT_MEMORY_ROLL
		static private int thresholdTimeLimit = 10;
		static private float growthLimit = 80;
#endif

		#region Events
		/// <summary>
		/// Delegate to handle Shutdown events.
		/// </summary>
		public event ShutdownEventHandler Shutdown;
		
		#endregion

		#endregion
		
		#region Constructor

		/// <summary>
		/// Creates a Manager.
		/// </summary>
		private Manager()
		{
			// configure
			SimiasLogManager.Configure(Store.StorePath);

			// Get an event subscriber to handle shutdown events.
			subscriber = new DefaultSubscriber();
			subscriber.SimiasEvent +=new SimiasEventHandler(OnSimiasEvent);

			lock (this)
			{
				// Get the XmlElement for the Services.
				servicesElement = Store.GetStore().Config.GetElement(CFG_Section, CFG_Services);

				XmlNodeList serviceNodes = servicesElement.SelectNodes(XmlServiceTag);
				foreach (XmlElement serviceNode in serviceNodes)
				{
					ServiceType sType = (ServiceType)Enum.Parse(typeof(ServiceType), serviceNode.GetAttribute(XmlTypeAttr));
					switch (sType)
					{
						case ServiceType.Process:
							serviceList.Add(new ProcessServiceCtl(serviceNode));
							break;
						case ServiceType.Thread:
							serviceList.Add(new ThreadServiceCtl(serviceNode));
							break;
					}
				}

#if CLIENT_MEMORY_ROLL
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
#endif
				// Start a monitor thread to keep the services running.
				Thread mThread = new Thread(new ThreadStart(Monitor));
				mThread.IsBackground = true;
				mThread.Start();
			}

			// reset the log4net configurations after a lock on Flaim was obtained
			SimiasLogManager.ResetConfiguration();
		}

		#endregion

		#region Factory Method
		/// <summary>
		/// Gets the static instance of the Manager object.
		/// </summary>
		/// <returns>The static instance of the Manager object.</returns>
		static public Manager GetManager()
		{
			lock(typeof(Manager))
			{
				if (instance == null)
				{
					instance = new Manager();
				}

				return instance;
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
#if CLIENT_MEMORY_ROLL
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
#endif
		private void Monitor()
		{
#if CLIENT_MEMORY_ROLL
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
#endif
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
#if CLIENT_MEMORY_ROLL
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
#endif
				}
				catch
				{
				}

				Thread.Sleep(1000 * 60);
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
			// Set that the database is being shut down so that no more changes can be made.
			logger.Info("The database is being shut down.");
			Store.GetStore().ShutDown();

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
