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

			string installDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);

			string windowsDir = Environment.GetEnvironmentVariable("windir");
			//fixConfigFile(ifolderAppPath);
			fixConfigFile(Path.Combine(windowsDir, "explorer.exe.config"), installDir);
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

		private bool fixConfigFile(string configFilePath, string installDir)
		{
			try
			{
				configDoc = new XmlDocument();
				configDoc.Load(configFilePath);

				XmlNode runtimeNode = configDoc.DocumentElement.SelectSingleNode("/configuration/runtime");
				if (runtimeNode != null)
				{
					XmlNode simiasNode = runtimeNode.FirstChild.FirstChild;
					XmlNode log4netNode = simiasNode.NextSibling;

					replaceCodeBase(simiasNode, installDir);
					replaceCodeBase(log4netNode, installDir);
				}


				XmlTextWriter xtw = new XmlTextWriter(configFilePath, Encoding.ASCII);
				try
				{
					xtw.Formatting = Formatting.Indented;
					configDoc.WriteTo(xtw);
				}
				finally
				{
					xtw.Close();
				}
			}
			catch
			{
				return false;
			}

			return true;
		}

		private void replaceCodeBase(XmlNode node, string path)
		{
			XmlNode identity = node.FirstChild;
			XmlNode codebase = identity.NextSibling;

			Uri fileURI = new Uri("file:///" + Path.Combine(path, ((XmlElement)identity).GetAttribute("name") + ".dll"));
			string ns = codebase.GetNamespaceOfPrefix(String.Empty);
			XmlElement element = configDoc.CreateElement(String.Empty, "codeBase", ns);
			element.SetAttribute("href", fileURI.AbsoluteUri);

			node.ReplaceChild(element, codebase);
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

