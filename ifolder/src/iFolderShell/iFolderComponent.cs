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
using System.Net;
using Novell.Win32Util;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Interface used for COM
	/// </summary>
	public interface IiFolderComponent
	{
		/// <summary>
		/// Gets/sets the description of the iFolder.
		/// </summary>
		//String Description{get; set;}

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
		/// Displays the Advanced Properties dialog for the specified iFolder.
		/// </summary>
		/// <param name="dllPath">The path where the assembly was loaded from.</param>
		/// <param name="path">The path of the iFolder.</param>
		/// <param name="tabPage">The index of the tab to display initially.</param>
		/// <param name="modal">Set to <b>true</b> to display the dialog modal.</param>
		void InvokeAdvancedDlg([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path, int tabPage, bool modal);

		/// <summary>
		/// Displays the Conflict Resolver dialog for the specified iFolder.
		/// </summary>
		/// <param name="dllPath">The path where the assembly was loaded from.</param>
		/// <param name="path">The path of the iFolder.</param>
		void InvokeConflictResolverDlg([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path);
		
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
		//private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(iFolderComponent));
		static private iFolderWebService ifWebService = null;
		static private long ticks = 0;
		static private readonly long delta = 50000000; // 5 seconds


//		public iFolderComponent(Uri location)
//		{
//			manager= iFolderManager.Connect(location);
//		}

		/// <summary>
		/// Constructs an iFolderComponent object.
		/// </summary>
		public iFolderComponent()
		{
			System.Diagnostics.Debug.WriteLine("In iFolderComponent()");

			try
			{
				connectToWebService();
			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.ConnectFailure)
				{
					ifWebService = null;
				}
			}
			catch (Exception e)
			{
				// TODO:
			}
		}

		/// <summary>
		/// Gets/sets the description of the iFolder.
		/// </summary>
		/*public String Description
		{
			get { return ifoldernode.Description; }
			set
			{
				ifoldernode.Description = value;

				// TODO - move this so that the commit can be done once at the end of
				// a bunch of modifies.
//				ifoldernode.iFolder.Commit();
			}
		}*/

		/// <summary>
		/// Checks to see if a path can become an iFolder.
		/// </summary>
		/// <param name="path">The path that is tested.</param>
		/// <returns>The method returns <b>true</b> if the specified path can become an ifolder; otherwise, <b>false</b>.</returns>
		public bool CanBeiFolder([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				connectToWebService();
				if (ifWebService != null)
				{
					return ifWebService.CanBeiFolder(path);
				}
			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.ConnectFailure)
				{
					ifWebService = null;
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Caught exception");
				//logger.Debug(e, "CanBeiFolder");
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
				connectToWebService();
				if (ifWebService != null)
				{
					//ifolder = manager.GetiFolderByPath(path);
					ifolder = ifWebService.GetiFolderByLocalPath(path);
					if (ifolder != null)
					{
						hasConflicts = ifolder.HasConflicts;
					}
				}
			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.ConnectFailure)
				{
					ifWebService = null;
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Caught exception");
				//logger.Debug(e, "IsiFolder");
			}

			return ifolder != null;
		}

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
				connectToWebService();
				if (ifWebService != null)
				{
					ifolder = ifWebService.CreateLocaliFolder(path);
				}
			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.ConnectFailure)
				{
					ifWebService = null;
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
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
				connectToWebService();
				if (ifWebService != null)
				{
					iFolder ifolder = ifWebService.GetiFolderByLocalPath(path);
					if (ifolder != null)
					{
						ifWebService.DeleteiFolder(ifolder.ID);
					}
				}
			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.ConnectFailure)
				{
					ifWebService = null;
				}
			}
			catch (Exception e)
			{
				//logger.Debug(e, "DeleteiFolderByPath");
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
				try
				{
					iFolderAdvanced ifolderAdvanced = new iFolderAdvanced();
					ifolderAdvanced.Name = path;
					ifolderAdvanced.Text = windowName;
					connectToWebService();
					ifolderAdvanced.CurrentiFolder = ifWebService.GetiFolderByLocalPath(path);
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
				catch (WebException e)
				{
					if (e.Status == WebExceptionStatus.ConnectFailure)
					{
						ifWebService = null;
					}
				}
				catch (Exception e)
				{
					// TODO:
				}
			}
		}

		/// <summary>
		/// Displays the Conflict Resolver dialog for the specified iFolder.
		/// </summary>
		/// <param name="dllPath">The path where the assembly was loaded from.</param>
		/// <param name="path">The path of the iFolder.</param>
		public void InvokeConflictResolverDlg([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				iFolder ifolder = ifWebService.GetiFolderByLocalPath(path);
				ConflictResolver conflictResolver = new ConflictResolver();
				conflictResolver.iFolder = ifolder;
				conflictResolver.iFolderWebService = ifWebService;
				conflictResolver.LoadPath = dllPath;
				conflictResolver.Show();		
			}
			catch (WebException e)
			{
				// TODO:
			}
			catch (Exception e)
			{
				// TODO:
			}
		}

		/// <summary>
		/// Display the iFolder confirmation dialog.
		/// </summary>
		/// <param name="dllPath">The path where the assembly was loaded from.</param>
		/// <param name="path">The path of the iFolder.</param>
		public void NewiFolderWizard([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			connectToWebService();
			try
			{
				iFolderSettings ifSettings = ifWebService.GetSettings();
				if (ifSettings.DisplayConfirmation)
				{
					NewiFolder newiFolder = new NewiFolder();
					newiFolder.FolderName = path;
					newiFolder.LoadPath = dllPath;
					newiFolder.iFolderWebService = ifWebService;
					newiFolder.Show();
				}
			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.ConnectFailure)
				{
					ifWebService = null;
				}
			}
			catch {}
		}

		/// <summary>
		/// Display the iFolder help.
		/// </summary>
		/// <param name="dllPath">The path where this assembly was loaded from.</param>
		public void ShowHelp([MarshalAs(UnmanagedType.LPWStr)] string dllPath)
		{
			// TODO - may need to pass in a specific page to load.
			// TODO - need to use locale-specific path
			string helpPath = Path.Combine(dllPath, @"help\en\doc\user\data\front.html");

			try
			{
				Process.Start(helpPath);
			}
			catch (Exception e)
			{
				// TODO: Localize.
				MessageBox.Show("Unable to open help file: \n" + helpPath, "Help File Not Found");
			}
		}

		private void connectToWebService()
		{
			if (ifWebService == null)
			{
				DateTime currentTime = DateTime.Now;
				if ((currentTime.Ticks - ticks) > delta)
				{
					ticks = currentTime.Ticks;
					ifWebService = new iFolderWebService();
				}
			}
		}
	}
}
