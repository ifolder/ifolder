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
//using Simias;
//using Simias.Event;

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

				// Parse the config file.
				XmlNode runtimeNode = configDoc.DocumentElement.SelectSingleNode("/configuration/runtime");
				if (runtimeNode != null)
				{
					// Get the simias element.
					XmlNode simiasNode = runtimeNode.FirstChild.FirstChild;

					// Get the log4net element.
					XmlNode log4netNode = simiasNode.NextSibling;

					// Replace the codeBase element.
					replaceCodeBase(simiasNode, installDir);
					replaceCodeBase(log4netNode, installDir);
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
			Process process;
			
		}

		private void StartShell()
		{
		}
	}
}

