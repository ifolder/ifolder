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

		static internal string XmlAssemblyAttr = "assembly";
		static internal string XmlEnabledAttr = "enabled";
		static internal string XmlNameAttr = "name";
		static internal string MutexBaseName = "ServiceManagerMutex___";

		private Thread startThread = null;
		private Thread stopThread = null;
		private ArrayList serviceList = new ArrayList();

		private const string ModulesDirectory = "modules";
		private const string ServiceConfigFiles = "*.conf";

		private ManualResetEvent servicesStarted = new ManualResetEvent(false);
		private ManualResetEvent servicesStopped = new ManualResetEvent(true);
		private DefaultSubscriber subscriber = null;

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
				// Add the Change Log Service and Local Domain Service by default.
				serviceList.Add( new ThreadServiceCtl( "Simias Change Log Service", "SimiasLib", "Simias.Storage.ChangeLog" ) );
				serviceList.Add( new ThreadServiceCtl( "Simias Local Domain Provider", "SimiasLib", "Simias.LocalProvider" ) );

				// Check if there is an overriding modules directory in the simias data area.
				string[] confFileList = null;
				string modulesDir = Path.Combine( Store.StorePath, ModulesDirectory );
				if ( Directory.Exists( modulesDir ) )
				{
					confFileList = Directory.GetFiles( modulesDir, ServiceConfigFiles );
				}

				// Check if there are any service configuration files.
				if ( ( confFileList == null ) || ( confFileList.Length == 0 ) )
				{
					// There is no overriding directory, use the default one.
					modulesDir = SimiasSetup.modulesdir;
					confFileList = Directory.GetFiles( modulesDir, ServiceConfigFiles );
				}

				// Get all of the configuration files from the modules directory.
				foreach ( string confFile in confFileList )
				{
					string assembly = Path.GetFileNameWithoutExtension( confFile );
					XmlDocument confDoc = new XmlDocument();
					confDoc.Load( confFile );

					// Get the XmlElement for the Services.
					foreach ( XmlElement serviceNode in confDoc.DocumentElement )
					{
						serviceList.Add( new ThreadServiceCtl( assembly, serviceNode ) );
					}
				}
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
