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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using Novell.iFolder;
using Simias.Storage;
using Novell.iFolder.Win32Util;
using Simias;

namespace Novell.iFolder.iFolderCom
{
	/// <summary>
	/// Interface used for COM
	/// </summary>
	public interface IiFolderComponent
	{
		/// <summary>
		/// Gets/sets the description of the iFolder.
		/// </summary>
		String Description{get; set;}

		/// <summary>
		/// Checks to see if a path can become an iFolder.
		/// </summary>
		/// <param name="path">The path that is tested.</param>
		/// <returns>The method returns <b>true</b> if the specified path can become an ifolder; otherwise, <b>false</b>.</returns>
		bool CanBeiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);

		/// <summary>
		/// Checks if the specified path is an iFolder.
		/// </summary>
		/// <param name="path">The path to test.</param>
		/// <param name="hasConflicts">This parameter returns <b>true</b> if the path is an iFolder and the iFolder contains conflicts.</param>
		/// <returns>This method returns <b>true</b> if the specified path is an iFolder; otherwise <b>false</b>.</returns>
		bool IsiFolder([MarshalAs(UnmanagedType.LPWStr)] string path, out bool hasConflicts);
//		bool IsShareable([MarshalAs(UnmanagedType.LPWStr)] string path);

		/// <summary>
		/// Converts the specified path into an iFolder.
		/// </summary>
		/// <param name="path">The path to convert into an iFolder.</param>
		/// <returns>This method returns <b>true</b> if the specified path is successfully converted into an iFolder; otherwise, <b>false</b>.</returns>
		bool CreateiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);

		/// <summary>
		/// Reverts the specified path back to a normal folder.
		/// </summary>
		/// <param name="path">The path to revert back to a normal folder.</param>
		void DeleteiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);

		/// <summary>
		/// Gets the node representing the specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		bool GetiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path);

		/// <summary>
		/// Checks to see if the specified path is contained in an iFolder.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		bool IsiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path);

		/// <summary>
		/// Initializes the properties enumerator.
		/// </summary>
		/// <returns>This method returns <b>true</b> if the properties enumerator is successfully initialized; otherwise, <b>false</b>.</returns>
		bool GetiFolderPropInit();

		/// <summary>
		/// Walks the properties enumerator.
		/// </summary>
		/// <param name="name">This parameter returns the name of the property.</param>
		/// <param name="val">This parameter returns the value of the property.</param>
		/// <returns>This method returns <b>true</b> if a property is successfully read; otherwise, <b>false</b>.</returns>
		bool GetNextiFolderProp(out string name, out string val);

		/// <summary>
		/// Displays the Advanced Properties dialog for the specified iFolder.
		/// </summary>
		/// <param name="dllPath">The path where the assembly was loaded from.</param>
		/// <param name="path">The path of the iFolder.</param>
		/// <param name="tabPage">The index of the tab to display initially.</param>
		/// <param name="modal">Set to <b>true</b> to display the dialog modal.</param>
		void InvokeAdvancedDlg([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path, int tabPage, bool modal);

		/// <summary>
		/// Display the iFolder confirmation dialog.
		/// </summary>
		/// <param name="dllPath">The path where the assembly was loaded from.</param>
		/// <param name="path">The path of the iFolder.</param>
		void NewiFolderWizard([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path);

		/// <summary>
		/// Display the iFolder help.
		/// </summary>
		/// <param name="dllPath">The path where this assembly was loaded from.</param>
		void ShowHelp([MarshalAs(UnmanagedType.LPWStr)] string dllPath);

		/// <summary>
		/// Check if the specified iFolder path has conflicts.
		/// </summary>
		/// <param name="path">The path of the iFolder.</param>
		/// <returns>This method returns <b>true</b> if the iFolder contains conflicts; otherwise, <b>false</b>.</returns>
		bool HasConflicts([MarshalAs(UnmanagedType.LPWStr)] string path);
	}

	/// <summary>
	/// Summary description for iFolderComponent.
	/// </summary>
	[
		ClassInterface(ClassInterfaceType.None),
		GuidAttribute("AA81D832-3B41-497c-B508-E9D02F8DF421")
	]
	public class iFolderComponent : IiFolderComponent
	{
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(iFolderComponent));
		static private iFolderManager manager = null;//= Manager.Connect();
		private iFolderNode ifoldernode;
		private ICSEnumerator propEnumerator;
		private ICSEnumerator aclEnumerator;

		private IEnumerator items;
//		public iFolderComponent(Uri location)
//		{
//			manager= iFolderManager.Connect(location);
//		}

		/// <summary>
		/// Constructs an iFolderComponent object.
		/// </summary>
		public iFolderComponent()
		{
			//
			// TODO: Add constructor logic here
			//
			System.Diagnostics.Debug.WriteLine("In iFolderComponent()");

			try
			{
				if (manager == null)
				{
					manager= iFolderManager.Connect();
				}
			}
			catch (SimiasException e)
			{
				e.LogError();
			}
			catch (Exception e)
			{
				logger.Debug(e, "Initialization");
			}
		}

		/// <summary>
		/// Gets/sets the description of the iFolder.
		/// </summary>
		public String Description
		{
			get { return ifoldernode.Description; }
			set
			{
				ifoldernode.Description = value;

				// TODO - move this so that the commit can be done once at the end of
				// a bunch of modifies.
//				ifoldernode.iFolder.Commit();
			}
		}

		/// <summary>
		/// Checks to see if a path can become an iFolder.
		/// </summary>
		/// <param name="path">The path that is tested.</param>
		/// <returns>The method returns <b>true</b> if the specified path can become an ifolder; otherwise, <b>false</b>.</returns>
		public bool CanBeiFolder([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				return manager.CanBeiFolder(path);
			}
			catch (SimiasException e)
			{
				e.LogError();
			}
			catch (Exception e)
			{
				logger.Debug(e, "CanBeiFolder");
			}

			return false;
		}

		/// <summary>
		/// Checks if the specified path is an iFolder.
		/// </summary>
		/// <param name="path">The path to test.</param>
		/// <param name="hasConflicts">This parameter returns <b>true</b> if the path is an iFolder and the iFolder contains conflicts.</param>
		/// <returns>This method returns <b>true</b> if the specified path is an iFolder; otherwise <b>false</b>.</returns>
		public bool IsiFolder([MarshalAs(UnmanagedType.LPWStr)] string path, out bool hasConflicts)
		{
			iFolder ifolder = null;
			hasConflicts = false;

			try
			{
				ifolder = manager.GetiFolderByPath(path);
				if (ifolder != null)
				{
					hasConflicts = ifolder.HasCollisions();
				}
			}
			catch (SimiasException e)
			{
				e.LogError();
			}
			catch (Exception e)
			{
				logger.Debug(e, "IsiFolder");
			}

			return ifolder != null;
		}

/*		public bool IsShareable([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				bool hasConflicts;
				if (IsiFolder(path, out hasConflicts))
				{
					return manager.GetiFolderByPath(path).Shareable;
				}
			}
			catch (SimiasException e)
			{
				e.LogError();
			}
			catch (Exception e)
			{
				logger.Debug(e, "GetiFolderByPath");
			}

			return false;
		}*/

		/// <summary>
		/// Converts the specified path into an iFolder.
		/// </summary>
		/// <param name="path">The path to convert into an iFolder.</param>
		/// <returns>This method returns <b>true</b> if the specified path is successfully converted into an iFolder; otherwise, <b>false</b>.</returns>
		public bool CreateiFolder([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			iFolder ifolder = null;
			try
			{
				ifolder = manager.CreateiFolder(path);
			}
			catch (SimiasException e)
			{
				e.LogError();
			}
			catch (Exception e)
			{
				logger.Debug(e, "CreateiFolder");
			}

			return (ifolder != null);
		}

		/// <summary>
		/// Reverts the specified path back to a normal folder.
		/// </summary>
		/// <param name="path">The path to revert back to a normal folder.</param>
		public void DeleteiFolder([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				manager.DeleteiFolderByPath(path);
			}
			catch (SimiasException e)
			{
				e.LogError();
			}
			catch (Exception e)
			{
				logger.Debug(e, "DeleteiFolderByPath");
			}
		}

		/// <summary>
		/// Gets the node representing the specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool GetiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			System.Diagnostics.Debug.WriteLine("In GetiFolderNode()");

			try
			{
				foreach(iFolder ifolder in manager)
				{
					if (path.StartsWith(ifolder.LocalPath))
					{
						ifoldernode = ifolder.GetiFolderNodeByPath(path);
						if (ifoldernode != null)
						{
							System.Diagnostics.Debug.WriteLine("GetiFolderNode() returning true");
							return true;
						}

						break;
					}
				}
			}
			catch (SimiasException e)
			{
				e.LogError();
			}
			catch (Exception e)
			{
				logger.Debug(e, "GetiFolderNode");
			}

			System.Diagnostics.Debug.WriteLine("GetiFolderNode() returning false");

			return false;
		}

		/// <summary>
		/// Checks to see if the specified path is contained in an iFolder.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool IsiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			return manager.IsPathIniFolder(path);
		}

		/// <summary>
		/// Initializes the properties enumerator.
		/// </summary>
		/// <returns>This method returns <b>true</b> if the properties enumerator is successfully initialized; otherwise, <b>false</b>.</returns>
		public bool GetiFolderPropInit()
		{
			// Set up the enumerator to get the Properties on the Node.
			propEnumerator = ( ICSEnumerator )ifoldernode.iFolder.Properties.GetEnumerator();

			return (propEnumerator != null);
		}

		/// <summary>
		/// Walks the properties enumerator.
		/// </summary>
		/// <param name="name">This parameter returns the name of the property.</param>
		/// <param name="val">This parameter returns the value of the property.</param>
		/// <returns>This method returns <b>true</b> if a property is successfully read; otherwise, <b>false</b>.</returns>
		public bool GetNextiFolderProp(out string name, out string val)
		{
			if (propEnumerator.MoveNext())
			{
				Property p = (Property)propEnumerator.Current;
				name = new string(p.Name.ToCharArray());
				val = new string(p.Value.ToString().ToCharArray());
				return true;
			}
			else
			{
				propEnumerator.Dispose();
				name = null;
				val = null;
				ifoldernode = null;
				return false;
			}
		}

		/// <summary>
		/// Displays the Advanced Properties dialog for the specified iFolder.
		/// </summary>
		/// <param name="dllPath">The path where the assembly was loaded from.</param>
		/// <param name="path">The path of the iFolder.</param>
		/// <param name="tabPage">The index of the tab to display initially.</param>
		/// <param name="modal">Set to <b>true</b> to display the dialog modal.</param>
		public void InvokeAdvancedDlg([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path, int tabPage, bool modal)
		{
			string windowName = "iFolder Properties for " + Path.GetFileName(path);

			// Search for existing window and bring it to foreground ...
			Win32Window win32Window = Win32Util.Win32Window.FindWindow(null, windowName);
			if (win32Window != null)
			{
				win32Window.BringWindowToTop();
			}
			else
			{
				iFolderAdvanced ifolderAdvanced = new iFolderAdvanced();
				ifolderAdvanced.Name = path;
				ifolderAdvanced.Text = windowName;
				ifolderAdvanced.CurrentiFolder = manager.GetiFolderByPath(path);
				ifolderAdvanced.LoadPath = dllPath;
				ifolderAdvanced.ActiveTab = tabPage;

				if (modal)
				{
					ifolderAdvanced.ShowDialog();
				}
				else
				{
					ifolderAdvanced.Show();
				}
			}
		}

		/// <summary>
		/// Display the iFolder confirmation dialog.
		/// </summary>
		/// <param name="dllPath">The path where the assembly was loaded from.</param>
		/// <param name="path">The path of the iFolder.</param>
		public void NewiFolderWizard([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			Configuration config = Configuration.GetConfiguration();
			string showWizard = config.Get("iFolderShell", "Show wizard", "true");
			if (showWizard == "true")
			{
				NewiFolder newiFolder = new NewiFolder();
				newiFolder.FolderName = path;
				newiFolder.LoadPath = dllPath;
				newiFolder.Show();
			}
		}

		/// <summary>
		/// Display the iFolder help.
		/// </summary>
		/// <param name="dllPath">The path where this assembly was loaded from.</param>
		public void ShowHelp([MarshalAs(UnmanagedType.LPWStr)] string dllPath)
		{
			// TODO - need to use locale-specific path
			string helpPath = Path.Combine(dllPath, @"help\en\doc\user\data\front.html");

			try
			{
				Process.Start(helpPath);
			}
			catch (Exception e)
			{
				logger.Debug(e, "Opening help");
				MessageBox.Show("Unable to open help file: \n" + helpPath, "Help File Not Found");
			}
		}

		/// <summary>
		/// Check if the specified iFolder path has conflicts.
		/// </summary>
		/// <param name="path">The path of the iFolder.</param>
		/// <returns>This method returns <b>true</b> if the iFolder contains conflicts; otherwise, <b>false</b>.</returns>
		public bool HasConflicts([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			iFolder ifolder = manager.GetiFolderByPath(path);
			if (ifolder != null)
			{
				return ifolder.HasCollisions();
			}

			return false;
		}
	}
}
