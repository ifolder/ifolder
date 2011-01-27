/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Paul Thomas <pthomas@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Xml;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Simias.Client;
using Microsoft.Win32;

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
		private void iFolderAppInstaller_Committing(object sender, InstallEventArgs e)
		{
			//Console.WriteLine("");
			//Console.WriteLine("Committing Event occured.");
			//Console.WriteLine("");
		}
		/// <summary>
		/// Event handler for 'Committed' event.
		/// </summary>
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
			fixWebConfigFile(Path.Combine(installDir, @"lib\simias\web\web.config"));
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
            //string regstring = "AA81D832-3B41-497c-B508-E9D02F8DF421";
			if (savedState == null)
			{
				throw new InstallException("iFolderApp Uninstall: savedState should not be null");
			}
			else
			{
				base.Uninstall(savedState);
				Console.WriteLine( "iFolderApp Uninstall" );

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

				// Build the path to the iFolder application data directory.
				string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "iFolder");

				try
				{
					// Delete the directory
					Directory.Delete(path, true);
				}
				catch {} // Ignore.
                try
                {
                    RegistryKey reg = Registry.ClassesRoot.CreateSubKey(@"CLSID\{AA81D832-3B41-497c-B508-E9D02F8DF421}\InProcServer32");
                    if (reg != null)
                    {
                        reg.DeleteSubKey(Application.ProductVersion);
                    }
                    reg.Close();
                }
                catch { } // Ignore.
			}
		}

        /// <summary>
        /// Fix the Application COnfig file 
        /// </summary>
        /// <param name="configFilePath">Path where condif file is placed</param>
        /// <param name="installDir">Dir where iFolder is installed</param>
        /// <returns>true on success else false</returns>
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
						Uri fileURI = new Uri(Path.Combine(installDir, Path.Combine(@"lib\simias\web\bin", "simiasclient.dll")));

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

        /// <summary>
        /// Fix the web config file
        /// </summary>
        /// <param name="webConfigFile">Path to the config file</param>
		private void fixWebConfigFile(string webConfigFile)
		{
			configDoc = new XmlDocument();
			configDoc.Load(webConfigFile);

			// Build the path to the iFolder application data directory.
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "iFolder");

			try
			{
				// Delete the directory
				Directory.Delete(path, true);
			}
			catch {} // Ignore.
			
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

        /// <summary>
        /// Replace the Code Base
        /// </summary>
        /// <param name="node">XML node</param>
        /// <param name="path">Path</param>
		private void replaceCodeBase(XmlNode node, string path)
		{
			XmlNode identity = node.FirstChild;
			XmlNode codebase = identity.NextSibling;

			// Build the path to the assembly
			Uri fileURI = new Uri(Path.Combine(path, Path.Combine(@"lib\simias\web\bin", ((XmlElement)identity).GetAttribute("name") + ".dll")));

			// Get the namespace from the node.
			string ns = codebase.GetNamespaceOfPrefix(String.Empty);

			// Create a new element with the correct path.
			XmlElement element = configDoc.CreateElement(String.Empty, "codeBase", ns);
			element.SetAttribute("href", fileURI.AbsoluteUri);

			// Replace the element.
			node.ReplaceChild(element, codebase);
		}

        /// <summary>
        /// Save the XML document
        /// </summary>
        /// <param name="doc">XML document to be saved</param>
        /// <param name="path">path where file needs to be saved</param>
		private void saveXmlFile(XmlDocument doc, string path)
		{
			// Save the config file.
			XmlTextWriter xtw = new XmlTextWriter(path, Encoding.UTF8);
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

        /// <summary>
        /// Stop Shell
        /// </summary>
		private void StopShell()
		{
		}

        /// <summary>
        /// Start Shell
        /// </summary>
		private void StartShell()
		{
		}
	}
}

