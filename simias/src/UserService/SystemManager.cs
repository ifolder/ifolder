/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ
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

namespace Simias
{
	/// <summary>
	/// System Manager
	/// </summary>
	public class SystemManager
	{
		private Configuration configuration;
		XmlDocument servicesDoc = new XmlDocument();
		XmlDocument workersDoc = new XmlDocument();
		ArrayList processList = new ArrayList();
		ArrayList workerList = new ArrayList();
		ArrayList mutexList = new ArrayList();
		const string CFG_Section = "SystemManager";
		const string CFG_Services = "Services";
		const string CFG_ServiceDefaults = "<ServiceList><Service name=\"CsEventBroker.exe\"/></ServiceList>";
		const string XmlServiceTag = "Service";
		const string XmlNameAttr = "name";
		const string XmlArgsAttr = "args";
		const string CFG_Workers = "Workers";
		const string CFG_WorkerDefaults = "<WorkerList><Worker assembly =\"a\" type=\"b\"/></WorkerList>";
		const string XmlWorkerTag = "Worker";
		const string XmlAssemblyAttr = "assembly";
		const string XmlTypeAttr = "type";
		const string mutexBaseName = "ServiceManagerMutex___";
		
		public SystemManager(Configuration conf)
		{
			configuration = conf;
			servicesDoc.LoadXml(conf.Get(CFG_Section, CFG_Services, CFG_ServiceDefaults));
			//workersDoc.LoadXml(conf.Get(CFG_Section, CFG_Workers, CFG_WorkerDefaults));
		}

		public void StartServices()
		{
			string mutexName = mutexBaseName + configuration.BasePath.GetHashCode().ToString();
			Mutex mutex = new Mutex(false, mutexName);
			if (mutex.WaitOne(3000, false))
			{
				mutexList.Add(mutex);
				XmlElement root = servicesDoc.DocumentElement;
				XmlNodeList serviceList = root.SelectNodes(XmlServiceTag);

				foreach (XmlElement service in serviceList)
				{
					string name = service.GetAttribute(XmlNameAttr);
					string arguments = service.GetAttribute(XmlArgsAttr);

					string servicePath;
					if (!Path.IsPathRooted(name))
					{
						Uri assemblyPath = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase));
						servicePath = Path.Combine(assemblyPath.LocalPath, name);
					}
					else
					{
						servicePath = name;
					}
					
					// The service is not running start it.
					System.Diagnostics.Process process = new Process();
					process.StartInfo.CreateNoWindow = true;
					process.StartInfo.UseShellExecute = false;
					if (MyEnvironment.Mono)
					{
						process.StartInfo.FileName = "mono";
						process.StartInfo.Arguments = servicePath + " ";
					}
					else
					{
						process.StartInfo.FileName = servicePath;
						process.StartInfo.Arguments = null;
					}
					process.StartInfo.Arguments += "\"" + configuration.BasePath + "\" ";
					process.StartInfo.Arguments += arguments;
					process.Start();
					processList.Add(process);
				}
			}
			else
			{
				throw new ApplicationException("Services Already running");
			}
		}

		private void startWorkers()
		{
			XmlElement root = workersDoc.DocumentElement;
			XmlNodeList workerNodes = root.SelectNodes(XmlWorkerTag);

			foreach (XmlElement worker in workerNodes)
			{
				string assembly = worker.GetAttribute(XmlAssemblyAttr);
				string typeName = worker.GetAttribute(XmlTypeAttr);

				string workerPath;
				if (!Path.IsPathRooted(assembly))
				{
					Uri assemblyPath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase));
					workerPath = Path.Combine(assemblyPath.LocalPath, assembly);
				}
				else
				{
					workerPath = assembly;
				}

				// Load the assembly and start it.
				Assembly pAssembly = AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension(assembly));
				Type pType = null;
				Type[] types = pAssembly.GetExportedTypes();
				foreach (Type t in types)
				{
					if (t.FullName.Equals(typeName))
					{
						pType = t;
						break;
					}
				}

				// If we did not find our type return a null.
				if (pType != null)
				{
					ICollectionWorker workerObj = (ICollectionWorker)pAssembly.CreateInstance(typeName);
					workerObj.Start(configuration);
					workerList.Add(workerObj);
				}
			}
		}

		private void stopWorkers()
		{
			foreach (ICollectionWorker worker in workerList)
			{
				worker.Stop();
			}
		}

		public void stopWorker(ICollectionWorker worker)
		{
			foreach (ICollectionWorker worker1 in workerList)
			{
				if (worker == worker1)
					worker.Stop();
			}
		}

		public void StartService(Process service)
		{
			if (service.HasExited)
			{
				foreach(Process process in ProcessList)
				{
					if (process.StartInfo.FileName == service.StartInfo.FileName)
					{
						process.Start();
					}
				}
			}
				
		}
	
		public void StopServices()
		{
			new EventPublisher(configuration).RaiseEvent(new ServiceEventArgs(ServiceEventArgs.TargetAll, ServiceControl.Shutdown));
		}

		public void StopService(Process service)
		{
			new EventPublisher(configuration).RaiseEvent(new ServiceEventArgs(service.Id, ServiceControl.Shutdown));
		}

		#region Properties
		
		public Configuration Config
		{
			get { return configuration; }
		}

		public ArrayList ProcessList
		{
			get { return processList;}
		}

		#endregion
	}

	internal class ServiceInfo
	{
		string path;
		string parameters;

		internal ServiceInfo(string servicePath)
		{
			path = servicePath;
			parameters = null;
		}

		internal ServiceInfo(string servicePath, string parameters):
			this(servicePath)
		{
			this.parameters = parameters;
		}
	}
}
