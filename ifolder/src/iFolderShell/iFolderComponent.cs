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
	public interface IiFolderComponent
	{
		String Description{get; set;}
		bool CanBeiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool IsiFolder([MarshalAs(UnmanagedType.LPWStr)] string path, out bool hasConflicts);
//		bool IsShareable([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool CreateiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);
		void DeleteiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool GetiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool IsiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool GetiFolderPropInit();
		bool GetNextiFolderProp(out string name, out string val);
		void InvokeAdvancedDlg([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path, [MarshalAs(UnmanagedType.LPWStr)] string tabPage, bool modal);
		void NewiFolderWizard([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path);
		void ShowHelp([MarshalAs(UnmanagedType.LPWStr)] string dllPath);
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

		public bool IsiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			return manager.IsPathIniFolder(path);
		}

		public bool GetiFolderPropInit()
		{
			// Set up the enumerator to get the Properties on the Node.
			propEnumerator = ( ICSEnumerator )ifoldernode.iFolder.Properties.GetEnumerator();

			return (propEnumerator != null);
		}

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

		public void InvokeAdvancedDlg([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path, [MarshalAs(UnmanagedType.LPWStr)] string tabPage, bool modal)
		{
			string windowName = "Advanced iFolder Properties for " + Path.GetFileName(path);

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
