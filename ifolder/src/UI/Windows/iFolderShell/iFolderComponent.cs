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
*                 $Author: Bruce Getter <bgetter@novell.com>
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
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Globalization;
using Microsoft.Win32;
using Novell.Win32Util;
using Simias.Client;
using Novell.iFolder.Web;
using System.Reflection;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Interface used for COM
	/// </summary>
	public interface IiFolderComponent
	{
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
		bool CreateiFolder([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path);

		/*/// <summary>
		/// Reverts the specified path back to a normal folder.
		/// </summary>
		/// <param name="path">The path to revert back to a normal folder.</param>
		void DeleteiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);*/

        /// <summary>
        /// Displays the Revert to Normal Folder dialog
        /// </summary>
        /// <param name="path"></param>
        void RevertToNormal([MarshalAs(UnmanagedType.LPWStr)] string path);

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
		void ShowHelp([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string helpFile);

		/// <summary>
		/// Gets the name of the language directory where resource files are installed.
		/// </summary>
		/// <returns>The name of the language directory.</returns>
		String GetLanguageDirectory();
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
        static private SimiasWebService simws = null;
		static private long ticks = 0;
		static private readonly long delta = 50000000; // 5 seconds
		private static readonly string displayConfirmationDisabled = "DisplayConfirmationDisabled";
		private static readonly string displayTrayIconDisabled = "DisplayTrayIconDisabled";
		private static readonly string iFolderKey = @"SOFTWARE\Novell\iFolder";
        private static bool simiasStarting = false;
        private static bool simiasRunning = false;
        private static string simiasStartingLock = "simiasStartingLock";
        private static string simiasRunningLock = "simiasRunningLock";

		private System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(iFolderAdvanced));
        
        public static bool SimiasRunning
        {
            get
            {
                return simiasRunning;
            }
            set
            {
                lock (simiasRunningLock)
                {
                    simiasRunning = value;
                }
            }
        }

        public static bool SimiasStarting
        {
            get
            {
                return simiasStarting;
            }
            set
            {
                lock (simiasStartingLock)
                {
                    simiasStarting = value;
                }
            }
        }

		/// <summary>
		/// Constructs an iFolderComponent object.
		/// </summary>
		public iFolderComponent()
		{
			try
			{
				connectToWebService();
			}
			catch (WebException e)
			{
				ifWebService = null;
				if (e.Status == WebExceptionStatus.ProtocolError)
				{
					LocalService.ClearCredentials();
				}
			}
			catch
			{
				// Ignore.
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
				// Make sure the user has read/write access to the directory.
				if (Win32Security.AccessAllowed(path))
				{
                    connectToWebService();
					if (ifWebService != null)
					{
						return ifWebService.CanBeiFolder(path);
					}
				}
			}
			catch (WebException e)
			{
				ifWebService = null;
				if (e.Status == WebExceptionStatus.ProtocolError)
				{
					LocalService.ClearCredentials();
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Caught exception - " + e.Message);
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
			iFolderWeb ifolder = null;
			hasConflicts = false;
			try
			{
				connectToWebService();
				if (ifWebService != null)
				{
					ifolder = ifWebService.GetiFolderByLocalPath(path);
					if (ifolder != null)
					{
						hasConflicts = ifolder.HasConflicts;
					}
				}
			}
			catch (WebException e)
			{
				ifWebService = null;
				if (e.Status == WebExceptionStatus.ProtocolError)
				{
					LocalService.ClearCredentials();
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Caught exception - " + e.Message);
			}

			return ifolder != null;
		}

		/// <summary>
		/// Converts the specified path into an iFolder.
		/// </summary>
		/// <param name="path">The path to convert into an iFolder.</param>
		/// <returns>This method returns <b>true</b> if the specified path is successfully converted into an iFolder; otherwise, <b>false</b>.</returns>
		public bool CreateiFolder([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			CreateiFolder createiFolder = new CreateiFolder();
            connectToWebService();
			createiFolder.iFolderWebService = ifWebService;
            createiFolder.simiasWebService = simws;
			createiFolder.iFolderPath = path;
			createiFolder.LoadPath = dllPath;
			if (DialogResult.OK == createiFolder.ShowDialog())
				return true;

			return false;
		}
        
		/// <summary>
		/// Reverts the specified path back to a normal folder. Also remove iFolder / Membership.
		/// </summary>
		/// <param name="path">The path to revert back to a normal folder.</param>

        public void RevertToNormal([MarshalAs(UnmanagedType.LPWStr)] string path)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                connectToWebService();
                RevertiFolder revertiFolder = new RevertiFolder();
                iFolderWeb ifolder = ifWebService.GetiFolderByLocalPath(path);
                bool IsMaster = (ifolder.CurrentUserID == ifolder.OwnerID);
                if (!IsMaster)
                    revertiFolder.removeFromServer.Text = resourceManager.GetString("AlsoRemoveMembership");

                revertiFolder.removeFromServer.Enabled = simws.GetDomainInformation(ifolder.DomainID).Authenticated;

                if (revertiFolder.ShowDialog() == DialogResult.Yes)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    if (ifWebService != null && ifolder != null)
                    {
                        ifWebService.RevertiFolder(ifolder.ID);
                        if(revertiFolder.RemoveFromServer)
                        {
                            if (IsMaster)
                            {
                                ifWebService.DeleteiFolder(ifolder.DomainID, ifolder.ID);
                            }  
                            else
                            {
                                ifWebService.DeclineiFolderInvitation(ifolder.DomainID, ifolder.ID);
                            }
                        }
                    }
                }
                revertiFolder.Dispose();
            }
            catch (WebException e)
            {
                ifWebService = null;
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    LocalService.ClearCredentials();
                }
            }
            catch (Exception e)
            {
                Cursor.Current = Cursors.Default;
                Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("iFolderRevertError"), 
                    resourceManager.GetString("revertErrorTitle"), e.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                mmb.ShowDialog();
                mmb.Dispose();
            }
            Cursor.Current = Cursors.Default;
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
			string windowName = string.Format(resourceManager.GetString("iFolderProperties"), Path.GetFileName(path));

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
                    
                    ifolderAdvanced.DomainName = (simws.GetDomainInformation((ifWebService.GetiFolderByLocalPath(path)).DomainID)).Name;
                    ifolderAdvanced.DomainUrl = (simws.GetDomainInformation((ifWebService.GetiFolderByLocalPath(path)).DomainID)).HostUrl;
                  

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
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("propertiesDialogError"), resourceManager.GetString("propertiesErrorTitle"), e.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();

					ifWebService = null;
					if (e.Status == WebExceptionStatus.ProtocolError)
					{
						LocalService.ClearCredentials();
					}
				}
				catch (Exception e)
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("propertiesDialogError"), resourceManager.GetString("propertiesErrorTitle"), e.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
			}
		}

        public static bool AdvancedConflictResolver(iFolderWebService ifWebService, iFolderWeb selectedItem)
        {
            const string assemblyName = @"lib\plugins\EnhancedConflictResolution.dll";
            const string enhancedConflictResolverClass = "Novell.EnhancedConflictResolution.EnhancedConflictResolver";
            const string showConflictResolverMethod = "Show";
            System.Object[] args = new System.Object[3];

            try
            {
                if (assemblyName != null)
                {
                    Assembly idAssembly = Assembly.LoadFrom(assemblyName);
                    if (idAssembly != null)
                    {
                        Type type = idAssembly.GetType(enhancedConflictResolverClass);
                        if (null != type)
                        {
                            args[0] = ifWebService;
                            args[1] = Application.StartupPath;
                            args[2] = selectedItem;
                            System.Object enhancedConflictResolver = Activator.CreateInstance(type, args);
                            try
                            {
                                MethodInfo method = type.GetMethod(showConflictResolverMethod, Type.EmptyTypes);
                                if (null != method)
                                {
                                    method.Invoke(enhancedConflictResolver, null);
                                    return true;
                                }
                            }
                            catch (Exception )
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception )
            {
            }
            return false;
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
                iFolderWeb ifolder = ifWebService.GetiFolderByLocalPath(path);
                if (!iFolderComponent.AdvancedConflictResolver(ifWebService, ifolder))
                {                    
                    ConflictResolver conflictResolver = new ConflictResolver();
                    conflictResolver.iFolder = ifolder;
                    conflictResolver.iFolderWebService = ifWebService;
                    conflictResolver.LoadPath = dllPath;
                    conflictResolver.Show();
                }
			}
			catch (Exception ex)
			{
				MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("conflictDialogError"), resourceManager.GetString("conflictErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}
		}

		/// <summary>
		/// Display the iFolder confirmation dialog.
		/// </summary>
		/// <param name="dllPath">The path where the assembly was loaded from.</param>
		/// <param name="path">The path of the iFolder.</param>
		public void NewiFolderWizard([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				if (DisplayConfirmationEnabled)
				{
					NewiFolder newiFolder = new NewiFolder();
					newiFolder.FolderName = path;
					newiFolder.LoadPath = dllPath;
					newiFolder.ShowDialog();
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
		public void ShowHelp([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string helpFile)
		{
			// TODO: - may need to pass in a specific page to load.
            string defaulthelpfile = "bookinfo.html";
			string helpPath = helpFile.Equals(string.Empty) ? 
				Path.Combine(Path.Combine(Path.Combine(dllPath, "help"), GetLanguageDirectory()), defaulthelpfile) :
				Path.Combine(Path.Combine(Path.Combine(dllPath, "help"), GetLanguageDirectory()), helpFile);

			if (!File.Exists(helpPath))
			{
				// Fall back to English if the language isn't installed.
				helpPath = helpFile.Equals(string.Empty) ? 
					Path.Combine(dllPath, @"help\en\bookinfo.html") :
					Path.Combine(Path.Combine(dllPath, @"help\en"), helpFile);
			}

			try
			{
				Process.Start(helpPath);
			}
			catch (Exception e)
			{
				MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("helpFileError") + "\n" + helpPath, resourceManager.GetString("helpErrorTitle"), e.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}
		}

		/// <summary>
		/// Gets the name of the language directory where resource files are installed.
		/// </summary>
		/// <returns>The name of the language directory.</returns>
		public String GetLanguageDirectory()
		{
			return iFolderAdvanced.GetLanguageDirectory();
		}

		private void connectToWebService()
		{
			if (ifWebService == null)
			{
                if (simiasRunning == false)
                {
                    Process[] processArray = Process.GetProcessesByName("simias");
                    if (processArray == null || processArray.Length == 0)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("simias is not running dude..."));
                        return;
                    }
                    simiasRunning = true;
                }
                System.Diagnostics.Debug.WriteLine(string.Format("In connectToWebService(). Simias is running...", SimiasRunning));
                /// If simias is not started do not start the web services...
                //if (SimiasRunning == false)
                //    return;
                // Use the language stored in the registry.
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(GetLanguageDirectory());

				// Check if the iFolder Client is running.
				string windowName = resourceManager.GetString("iFolderServices") + Environment.UserName;
                Novell.Win32Util.Win32Window window = Novell.Win32Util.Win32Window.FindWindow(null, windowName);
				if (window != null)
				{
                    DateTime currentTime = DateTime.Now;
					if ((currentTime.Ticks - ticks) > delta)
					{
						ticks = currentTime.Ticks;

						// The registry holds the information needed to connect to the web service.
						RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Novell\iFolder");
						if (regKey != null)
						{
							string webServiceUri = regKey.GetValue("WebServiceUri") as string;
							string simiasDataPath = regKey.GetValue("SimiasDataPath") as string;
							if ((webServiceUri != null) && (simiasDataPath != null))
							{
								ifWebService = new iFolderWebService();
								ifWebService.Url = webServiceUri + "/iFolder.asmx";
								LocalService.Start(ifWebService, new Uri(webServiceUri), simiasDataPath);

                                simws = new SimiasWebService();
                                simws.Url = webServiceUri + "/Simias.asmx";
                                LocalService.Start(simws, new Uri(webServiceUri), simiasDataPath);
                			}

							regKey.Close();
						}
					}
				}
				else
				{
                    throw new Exception("iFolder Client not running");
				}
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if the Confirmation dialog should be displayed when a new 
		/// iFolder is created.
		/// </summary>
		static public bool DisplayConfirmationEnabled
		{
			get
			{
				int display;
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

					// Get the notify share value ... default the value to 0 (enabled).
					display = (int)regKey.GetValue(displayConfirmationDisabled, 0);
				}
				catch
				{
					return true;
				}

				return (display == 0);
			}
			set
			{
				// Create/open the iFolder key.
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

				if (value)
				{
					// Delete the value.
					regKey.DeleteValue(displayConfirmationDisabled, false);
				}
				else
				{
					// Set the disable value.
					regKey.SetValue(displayConfirmationDisabled, 1);
				}
			}
		}
		
		/// <summary>
      	/// Gets/sets a value indicating if the Confirmation dialog should be displayed when a new 
	      /// iFolder is created.
            /// </summary>
	      static public bool DisplayTrayIconEnabled
            {
           		get
            	{
                		int display;
   		            try
            		{
		                  // Create/open the iFolder key.
            		      RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

		                  // Get the notify share value ... default the value to 0 (enabled).
            		      display = (int)regKey.GetValue(displayTrayIconDisabled, 0);
                 		}
	                  catch
      	            {
    	  		          return true;
	            	}

      	            return (display == 0);
            	}
            	set
            	{
                		// Create/open the iFolder key.
              		RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);
	                  if (value)
          			{
	                  // Delete the value.
      	            regKey.DeleteValue(displayTrayIconDisabled, false);
            	      }
                		else
	                  {
          			      // Set the disable value.
		                  regKey.SetValue(displayTrayIconDisabled, 1);
                  	}
           		}
      	}  
	}
}
