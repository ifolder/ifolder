/***********************************************************************
 *	$RCSfile$
 *
 *	Copyright (C) 2004 Novell, Inc.
 *
 *	This program is free software; you can redistribute it and/or
 *	modify it under the terms of the GNU General Public
 *	License as published by the Free Software Foundation; either
 *	version 2 of the License, or (at your option) any later version.
 *
 *	This program is distributed in the hope that it will be useful,
 *	but WITHOUT ANY WARRANTY; without even the implied warranty of
 *	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.	See the GNU
 *	General Public License for more details.
 *
 *	You should have received a copy of the GNU General Public
 *	License along with this program; if not, write to the Free
 *	Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *	Author: Paul Thomas <pthomas@novell.com>
 *
 ***********************************************************************/

using System;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Xml;
using System.IO;
using System.Text;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Provides custom installation for iFolderApp.exe.
	/// </summary>
	// Set 'RunInstaller' attribute to true.
	[RunInstaller(true)]
	public class iFolderAppInstallerClass: Installer
	{
		private XmlDocument configDoc;

		/// <summary>
		/// Constructor.
		/// </summary>
		public iFolderAppInstallerClass() :base()
		{
			// Attach the 'Committed' event.
			this.Committed += new InstallEventHandler(iFolderAppInstaller_Committed);
			// Attach the 'Committing' event.
			this.Committing += new InstallEventHandler(iFolderAppInstaller_Committing);
	
		}
		/// <summary>
		/// Event handler for 'Committing' event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void iFolderAppInstaller_Committing(object sender, InstallEventArgs e)
		{
			//Console.WriteLine("");
			//Console.WriteLine("Committing Event occured.");
			//Console.WriteLine("");
		}
		/// <summary>
		/// Event handler for 'Committed' event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void iFolderAppInstaller_Committed(object sender, InstallEventArgs e)
		{
			//Console.WriteLine("");
			//Console.WriteLine("Committed Event occured.");
			//Console.WriteLine("");
		}
		/// <summary>
		/// Override the 'Install' method.
		/// </summary>
		/// <param name="savedState"></param>
		public override void Install(IDictionary savedState)
		{
			base.Install(savedState);
			Console.WriteLine("iFolderApp Install");

			// Get the directory where the assembly is running.
			string installDir = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;

			// Get the Windows directory.
			string windowsDir = Environment.GetEnvironmentVariable("windir");

			fixAppConfigFile(Path.Combine(windowsDir, "explorer.exe.config"), installDir);
			fixWebConfigFile(Path.Combine(installDir, @"web\web.config"));
			fixBootstrapConfigFile(Path.Combine(installDir, @"etc\simias-client-bootstrap.config"), Path.Combine(installDir, "web"));
		}
		/// <summary>
		/// Override the 'Commit' method.
		/// </summary>
		/// <param name="savedState"></param>
		public override void Commit(IDictionary savedState)
		{
            Console.WriteLine("iFolderApp Commit");
			base.Commit(savedState);
		}
		/// <summary>
		/// Override the 'Rollback' method.
		/// </summary>
		/// <param name="savedState"></param>
		public override void Rollback(IDictionary savedState)
		{
            Console.WriteLine("iFolderApp Rollback");
			base.Rollback(savedState);
		}
		/// <summary>
		/// Override the 'Uninstall' method.
		/// </summary>
		/// <param name="savedState"></param>
        public override void Uninstall(IDictionary savedState)
		{
			if (savedState == null)
			{
				throw new InstallException("iFolderApp Uninstall: savedState should not be null");
			}
			else
			{
				base.Uninstall(savedState);
				Console.WriteLine( "iFolderApp Uninstall" );

				// Kill SimiasApp
                Process[] simiasAppProcesses = Process.GetProcessesByName("SimiasApp");
				foreach (Process process in simiasAppProcesses)
				{
					try
					{
						process.Kill(); // This will throw if the process is no longer running
					}
					catch {}
					process.Close();
				}
				// Kill iFolderApp
                Process[] ifolderAppProcesses = Process.GetProcessesByName("iFolderApp");
				foreach (Process process in ifolderAppProcesses)
				{
					try
					{
						process.Kill(); // This will throw if the process is no longer running
					}
					catch {}
					process.Close();
				}
			}
		}

		private bool fixAppConfigFile(string configFilePath, string installDir)
		{
			try
			{
				// Load the config file.
				configDoc = new XmlDocument();
				configDoc.Load(configFilePath);

				bool found = false;

				// Get all of the dependentAssembly elements.
				XmlNodeList nodeList = configDoc.GetElementsByTagName("dependentAssembly");
				foreach (XmlNode n in nodeList)
				{
					// Look for an assembly called simiasclient.
					XmlNodeList nList2 = ((XmlElement)n).GetElementsByTagName("assemblyIdentity");
					foreach (XmlNode n2 in nList2)
					{
						string name = ((XmlElement)n2).GetAttribute("name");
						if (name.Equals("simiasclient"))
						{
							// We found it, update this node so that it has the correct path.
							replaceCodeBase(n, installDir);
							found = true;
							break;
						}
					}

					if (found)
						break;
				}

				if (!found)
				{
					// Get the assemblyBinding element.
					XmlNode assemblyBindingNode = null;
					nodeList = configDoc.GetElementsByTagName("assemblyBinding");
					foreach (XmlNode n in nodeList)
					{
						// There should only be one.
						assemblyBindingNode = n;
						break;
					}

					if (assemblyBindingNode == null)
					{
						// We didn't find the assemblyBinding element, so we need to create it.
						XmlNode runtime = configDoc.SelectSingleNode("/configuration/runtime");
						if (runtime == null)
						{
							// No runtime element either, so create it.
							runtime = configDoc.CreateNode(XmlNodeType.Element, "runtime", null);
							XmlNode configuration = configDoc.SelectSingleNode("/configuration");
							configuration.AppendChild(runtime);
						}

						assemblyBindingNode = configDoc.CreateElement(string.Empty, "assemblyBinding", "urn:schemas-microsoft-com:asm.v1");
						runtime.AppendChild(assemblyBindingNode);
					}

					if (assemblyBindingNode != null)
					{
						// Build the path to the assembly
						Uri fileURI = new Uri(Path.Combine(installDir, Path.Combine(@"web\bin", "simiasclient.dll")));

						// Get the namespace from the node.
						string ns = assemblyBindingNode.GetNamespaceOfPrefix(String.Empty);

						// Create an element with the correct path.
						XmlElement dependentAssembly = configDoc.CreateElement(String.Empty, "dependentAssembly", ns);
						XmlElement assemblyIdentity = configDoc.CreateElement(String.Empty, "assemblyIdentity", ns);
						assemblyIdentity.SetAttribute("name", "simiasclient");
						XmlElement codeBase = configDoc.CreateElement(String.Empty, "codeBase", ns);
						codeBase.SetAttribute("href", fileURI.AbsoluteUri);
						dependentAssembly.AppendChild(assemblyIdentity);
						dependentAssembly.AppendChild(codeBase);

						// Add the element.
						assemblyBindingNode.AppendChild(dependentAssembly);
					}
				}

				saveXmlFile(configDoc, configFilePath);
			}
			catch
			{
				return false;
			}

			return true;
		}

		private void fixBootstrapConfigFile(string bootstrapConfigFile, string installPath)
		{
			configDoc = new XmlDocument();
			configDoc.Load(bootstrapConfigFile);

			XmlElement sectionElement = (XmlElement)configDoc.DocumentElement.SelectSingleNode("//section[@name='ServiceManager']");
			if (sectionElement != null)
			{
				XmlNode webServicePathNode = sectionElement.SelectSingleNode("//setting[@name='WebServicePath']");
				if (webServicePathNode != null)
				{
					((XmlElement)webServicePathNode).SetAttribute("value", installPath);
				}
			}

			saveXmlFile(configDoc, bootstrapConfigFile);
		}

		private void fixWebConfigFile(string webConfigFile)
		{
			configDoc = new XmlDocument();
			configDoc.Load(webConfigFile);

			// Build the path.
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "iFolder");

			// Make sure the path exists.
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			// Set access on the path so that members of the Users group can access it.
			new Security().SetAccess(
				path, 
				Security.CONTAINER_INHERIT_ACE | Security.OBJECT_INHERIT_ACE,
				Security.GENERIC_EXECUTE | Security.GENERIC_READ | Security.GENERIC_WRITE);
 
			XmlNode compilation = configDoc.DocumentElement.SelectSingleNode("/configuration/system.web/compilation");
			if (compilation != null)
			{
				// modify the existing entry.
				((XmlElement)compilation).SetAttribute("tempDirectory", path);
			}
			else
			{
				// create the entry.
				XmlNode webNode = configDoc.DocumentElement.SelectSingleNode("/configuration/system.web");
				if (webNode != null)
				{

					XmlElement element = configDoc.CreateElement("compilation");
					element.SetAttribute("tempDirectory", path);
					webNode.AppendChild(element);
				}
			}

			saveXmlFile(configDoc, webConfigFile);
		}

		private void replaceCodeBase(XmlNode node, string path)
		{
			XmlNode identity = node.FirstChild;
			XmlNode codebase = identity.NextSibling;

			// Build the path to the assembly
			Uri fileURI = new Uri(Path.Combine(path, Path.Combine(@"web\bin", ((XmlElement)identity).GetAttribute("name") + ".dll")));

			// Get the namespace from the node.
			string ns = codebase.GetNamespaceOfPrefix(String.Empty);

			// Create a new element with the correct path.
			XmlElement element = configDoc.CreateElement(String.Empty, "codeBase", ns);
			element.SetAttribute("href", fileURI.AbsoluteUri);

			// Replace the element.
			node.ReplaceChild(element, codebase);
		}

		private void saveXmlFile(XmlDocument doc, string path)
		{
			// Save the config file.
			XmlTextWriter xtw = new XmlTextWriter(path, Encoding.ASCII);
			try
			{
				xtw.Formatting = Formatting.Indented;

				doc.WriteTo(xtw);
			}
			finally
			{
				xtw.Close();
			}
		}

		private void StopShell()
		{
		}

		private void StartShell()
		{
		}
	}
}

