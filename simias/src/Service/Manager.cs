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
using System.Net;
using System.Net.Sockets;
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

		static private Configuration conf;
		static private Process webProcess = null;
		static private EventHandler appDomainUnloadEvent;

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
		private const string CFG_WebServicePath = "WebServicePath";
		private const string CFG_ShowOutput = "WebServiceOutput";
		private const string CFG_WebServiceUri = "WebServiceUri";
		private const string XmlServiceTag = "Service";

		private ManualResetEvent servicesStarted = new ManualResetEvent(false);
		private ManualResetEvent servicesStopped = new ManualResetEvent(true);
		private DefaultSubscriber	subscriber = null;

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
			// configure
			SimiasLogManager.Configure(config);
			SimiasRemoting.Configure(config);

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

		private void Monitor()
		{
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
		/// 
		/// </summary>
		static public void Start()
		{
			lock ( typeof( Manager ) )
			{
				// Make sure the process is not already started.
				if ( webProcess == null )
				{
					// Set up the process info to start the XSP process.
					webProcess = new Process();
					appDomainUnloadEvent = new EventHandler( xspProcessExited );
					webProcess.Exited += appDomainUnloadEvent;

					// Get the web service path from the configuration file.
					Configuration config = ( conf != null ) ? conf : Configuration.GetConfiguration();
					string webPath = config.Get( CFG_Section, CFG_WebServicePath, Directory.GetCurrentDirectory() );
					string webApp = Path.Combine( webPath, String.Format( "bin{0}SimiasApp.exe", Path.DirectorySeparatorChar ) );

					webProcess.StartInfo.FileName = webApp;
					webProcess.StartInfo.UseShellExecute = false;
					webProcess.StartInfo.RedirectStandardInput = true;
					webProcess.StartInfo.CreateNoWindow = ( String.Compare( config.Get( CFG_Section, CFG_ShowOutput, false.ToString() ), "True", true ) == 0 ) ? false : true;
					webProcess.EnableRaisingEvents = true;

					if ( !Path.IsPathRooted( webPath ) )
					{
						throw new SimiasException( String.Format( "Web service path must be absolute: {0}", webPath ) );
					}

					// See if there is already a uri specified in the configuration file.
					Uri uri = null;
					if ( config.Exists( CFG_Section, CFG_WebServiceUri ) )
					{
						uri = new Uri( config.Get( CFG_Section, CFG_WebServiceUri, null ) );
					}
					else
					{
						// Get the dynamic port that xsp should use and write it out to the config file.
						string virtualRoot = String.Format( "/simias10/{0}", Environment.UserName );
						uri = new Uri( new UriBuilder( "http", IPAddress.Loopback.ToString(), GetXspPort(), virtualRoot ).ToString() );
						config.Set( CFG_Section, CFG_WebServiceUri, uri.ToString() );
					}

					// Strip off the volume if it exists and the file name and make the path absolute from the root.
					string appPath = String.Format( "{0}{1}", Path.DirectorySeparatorChar, webPath.Remove( 0, Path.GetPathRoot( webPath ).Length ) );
					webProcess.StartInfo.Arguments = String.Format( "--applications {0}:{1} --port {2}", uri.PathAndQuery, appPath, uri.Port.ToString() );
					webProcess.Start();

					logger.Info( "SimiasApp process has been started." );
				}
			}
		}

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
		/// Shuts down the XSP process.
		/// </summary>
		static public void Stop()
		{
			lock ( typeof( Manager ) )
			{
				if ( webProcess != null )
				{
					// Remove the exit event handler before shutting down the process.
					webProcess.Exited -= appDomainUnloadEvent;

					// Tell XSP to terminate and wait for it to exit.
					webProcess.StandardInput.WriteLine( "" );
					webProcess.WaitForExit();
					webProcess = null;
					logger.Info( "SimiasApp Process has been stopped." );
				}
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

		/// <summary>
		/// Callback that gets notified when the XSP process terminates.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static private void xspProcessExited(object sender, EventArgs e)
		{
			lock( typeof( Manager ) )
			{
				if ( webProcess != null )
				{
					logger.Info( "XSP process has exited. Restarting..." );
					webProcess = null;
					Start();
				}
			}
		}

		static private int GetXspPort()
		{
			Socket s = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			try
			{
				s.Bind( new IPEndPoint( IPAddress.Loopback, 0 ) );
				return ( s.LocalEndPoint as IPEndPoint ).Port;
			}
			finally
			{
				s.Close();
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

		/// <summary>
		/// Gets the local service url so that applications can talk to the local webservice.
		/// </summary>
		static public Uri LocalServiceUrl
		{
			get
			{
				// Get the configuration object.
				Configuration config = ( conf != null ) ? conf : Configuration.GetConfiguration();
				string uriString = config.Get( CFG_Section, CFG_WebServiceUri, null );
				return ( uriString != null ) ? new Uri( uriString ) : null;
			}
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
