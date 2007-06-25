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
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;

using Simias.Client;
using Simias;

namespace Novell.iFolder.Install
{
	public enum UpgradeResult
	{
		Latest = 0,
		UpgradeNeeded = 1,
		ServerOld = 2,
		UpgradeAvailable = 3,
		Unknown =4,
	};
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class ClientUpgrade
	{
		#region Class Members
		/// <summary>
		/// Temporary directory used to copy the ifolder updates to.
		/// </summary>
		private static string iFolderUpdateDirectory = "ead51d60-cd98-4d35-8c7c-b43a2ca949c8";

		/// <summary>
		/// iFolder Client update files.
		/// </summary>
		private static string iFolderWindowsApplication = "iFolderApp.exe";
		private static string iFolderLinuxApplication = "iFolderClient.exe";
		private static readonly string LinuxPlatformFile = "/etc/issue";

		/// <summary>
		/// Strings used in the handler query.
		/// </summary>
		private static string PlatformQuery = "Platform";
		private static string FileQuery = "File";

		/// <summary>
		/// Web service object to use for checking for client updates.
		/// </summary>
		private ClientUpdate service = null;

		/// <summary>
		/// Address to the host where the web service is running.
		/// </summary>
		public string hostAddress;
		#endregion

		#region Constructor
		
		/// <summary>
		/// Initializes a new instance of the object.
		/// </summary>
		/// <param name="domainID">Identifier of the domain that the user belongs to.</param>
		private ClientUpgrade( string domainID )
		{
			// Set the web state when authentication is really required...
			WebState ws = new WebState(domainID, domainID);
			// Get the address of the host service.
			hostAddress = DomainProvider.ResolveLocation( domainID ).ToString();
			// Setup the url to the server.
			service = new ClientUpdate();
			service.Url = hostAddress + "/ClientUpdate.asmx";
			ws.InitializeWebClient(service, domainID);
		}

		#endregion

		#region Private Methods
		/// <summary>
		/// Downloads the specified files in the list from the server.
		/// </summary>
		/// <param name="fileList">List of files to download.</param>
		/// <returns>The path to the downloaded files.</returns>
		private string DownloadFiles( string[] fileList, string path )
		{
			string downloadDir = "";
			// Create the temporary directory.
			if( path == null)
				downloadDir = Path.Combine( Path.GetTempPath(), iFolderUpdateDirectory );
			else
				downloadDir = Path.Combine( path, iFolderUpdateDirectory);
			if ( Directory.Exists( downloadDir ) )
			{
				// Clean up the old directory.
				Directory.Delete( downloadDir, true );
			}

			// Create it new everytime.
			Directory.CreateDirectory( downloadDir );
		
			try
			{
				// Construct a WebClient to do the downloads.
				WebClient webClient = new WebClient();
				webClient.Credentials = service.Credentials; 
				foreach ( string file in fileList )
				{
					// Add the filename as the query string.
					NameValueCollection nvc = new NameValueCollection();
					if ( MyEnvironment.Platform == MyPlatformID.Windows )
						nvc.Add( PlatformQuery, MyEnvironment.Platform.ToString() );
					else
					{
						// FIXME: Add code here to handle Mac OS X
					
						nvc.Add( PlatformQuery, GetLinuxPlatformString() );
					}
					nvc.Add( FileQuery, file );
					webClient.QueryString = nvc;
					webClient.DownloadFile( hostAddress + "/ClientUpdateHandler.ashx", Path.Combine( downloadDir, file ) );
				}
			}
			catch
			{
				// Delete the directory.
				Directory.Delete( downloadDir, true );
				downloadDir = null;
			}

			return downloadDir;
		}

		/// <summary>
		/// Gets the version of the currently running Windows client.
		/// </summary>
		/// <returns>A Version object containing the version of the client if successful.
		/// Otherwise null is returned.</returns>
		private string GetWindowsClientVersion()
		{
			string version = null;

			string fullPath = Path.Combine( SimiasSetup.bindir, iFolderWindowsApplication );
			if ( File.Exists( fullPath ) )
			{
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo( fullPath );
				version = versionInfo.ProductVersion;
			}

			return version;
		}
		
		/// <summary>
		/// Gets a string to identify the linux platform this code is running
		/// on by returning the contents of the "/etc/issue" file.
		/// </summary>
		/// <returns>A string that identifies the linux platform if it can be
		/// determined.  Otherwise null is returned.</returns>
		private string GetLinuxPlatformString()
		{
			string platformString = null;
			
			if ( File.Exists( LinuxPlatformFile ) )
			{
				StreamReader sr = null;
				try
				{
					sr = new StreamReader( LinuxPlatformFile );
					string line = null;
					do
					{
						line = sr.ReadLine();
						if (line != null)
						{
							if (platformString == null)
								platformString = line;
							else
								platformString = string.Concat(platformString, line);
						}
					} while (line != null);
				}
				catch {}
				finally
				{
					if (sr != null)
						sr.Close();
				}
				
				if ( platformString != null )
					platformString = platformString.Trim();
			}
			
			return platformString;
		}
		
		/// <summary>
		/// Gets the version of the currently running Linux client.
		/// </summary>
		/// <returns>A Version object containing the version of the client if successful.
		/// Otherwise null is returned.</returns>
		private string GetLinuxClientVersion()
		{
			string version = null;

			string fullPath = Path.Combine( SimiasSetup.bindir, iFolderLinuxApplication );
			if ( File.Exists( fullPath ) )
			{
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo( fullPath );
				version = versionInfo.ProductVersion;
			}

			return version;
		}

		/// <summary>
		/// Checks to see if there is a newer client application available on the domain server and
		/// prompts the user to upgrade.
		/// </summary>
		/// <param name="domainID">The ID of the domain to check for updates against.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		private string CheckForUpdateAvailable()
		{
			string updateVersion = null;

			// Make sure that the service object is authenticated.
			if ( service != null )
			{
				// Get the current version of this client.
				string currentVersion = null;
				string platformString = null;
				if ( MyEnvironment.Platform == MyPlatformID.Windows )
				{
					platformString = MyEnvironment.Platform.ToString();
					currentVersion = GetWindowsClientVersion();
				}
				else if ( MyEnvironment.Platform == MyPlatformID.Unix )
				{
					// FIXME: Create a function for the Mac client
					platformString = GetLinuxPlatformString();
					currentVersion = GetLinuxClientVersion();
				}
				
				if ( platformString != null && currentVersion != null )
				{
					// Call to the web service to see if there is a version newer than the one
					// that is currently running.
					updateVersion = service.IsUpdateAvailableActual( platformString, currentVersion );
					/*
					bool status = service.IsServerOlder(platformString, currentVersion);
					if(status == true)
						updateVersion = "Server OLDER";
					else
						updateVersion = "server not older";
					*/
				}
			}

			return updateVersion;
		}

		
		/// <summary>
		/// Checks to see if there is a need for newer client application on the domain server and
		/// prompts the user to upgrade.
		/// </summary>
		/// <param name="domainID">The ID of the domain to check for updates against.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		private StatusCodes CheckForUpdate(out string ServerVersion)
		{
			string updateVersion = null;
			ServerVersion = null;
			// Make sure that the service object is authenticated.
			if ( service != null )
			{
				// Get the current version of this client.
				string currentVersion = null;
				string platformString = null;
				if ( MyEnvironment.Platform == MyPlatformID.Windows )
				{
					platformString = MyEnvironment.Platform.ToString();
					currentVersion = GetWindowsClientVersion();
				}
				else if ( MyEnvironment.Platform == MyPlatformID.Unix )
				{
					// FIXME: Create a function for the Mac client
					platformString = GetLinuxPlatformString();
					currentVersion = GetLinuxClientVersion();
				}
				
				if ( platformString != null && currentVersion != null )
				{
					// Call to the web service to see if there is a version newer than the one
					// that is currently running.
				//	updateVersion = service.IsUpdateAvailable( platformString, currentVersion );
					string serverVersion = null;
					StatusCodes stat = (StatusCodes)service.CheckForUpdate(platformString, currentVersion, out serverVersion );
					ServerVersion = serverVersion;
					return stat;
					/*
					bool status = service.IsServerOlder(platformString, currentVersion);
					if(status == true)
						updateVersion = "Server OLDER";
					else
						updateVersion = "server not older";
					*/
				}
			}

			return StatusCodes.Unknown;
		}

		/// <summary>
		/// Checks to see if the server is running an older
		/// version of simias
		/// </summary>
		/// <param name="domainID">The ID of the domain to check for updates against.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		private bool CheckForServerUpdate()
		{
			string updateVersion = null;
			bool serverOlder = false;

			// Make sure that the service object is authenticated.
			if ( service != null )
			{
				// Get the current version of this client.
				string currentVersion = null;
				string platformString = null;
				if ( MyEnvironment.Platform == MyPlatformID.Windows )
				{
					platformString = MyEnvironment.Platform.ToString();
					currentVersion = GetWindowsClientVersion();
				}
				else if ( MyEnvironment.Platform == MyPlatformID.Unix )
				{
					// FIXME: Create a function for the Mac client
					platformString = GetLinuxPlatformString();
					currentVersion = GetLinuxClientVersion();
				}

				if ( platformString != null && currentVersion != null )
				{
					// Call to the web service to see if there is a version newer than the one
					// that is currently running.
					serverOlder = service.IsServerOlder(platformString, currentVersion);
				}
			}

			return serverOlder;
		}

		/// <summary>
		/// Gets the updated client application and runs the installation program.
		/// Note: This call will return before the application is updated.
		/// </summary>
		/// <returns>True if the installation program is successfully started. Otherwise false is returned.</returns>
		private bool RunUpdate(string path)
		{
			bool running = false;

			Console.WriteLine("RunUpdate in ClientUpgrade");

			// Make sure that the service is authenticated.
			if ( service != null )
			{
				Console.WriteLine("GetUpdateFiles");
				// Get the list of files needed for the update.
				string[] fileList = service.GetUpdateFiles();
				if ( fileList != null )
				{
					Console.WriteLine("DownloadFiles");
					// Download the files in the list to a temporary directory.
					string downloadDir = DownloadFiles( fileList, path );
					if ( downloadDir != null )
					{
						running = true;

						//Install the client
						if ( MyEnvironment.Platform == MyPlatformID.Windows )
						{
							// There should only be one file needed for the windows update.
							Process installProcess = new Process();
							installProcess.StartInfo.FileName = Path.Combine( downloadDir, Path.GetFileName( fileList[ 0 ] ) );
							installProcess.StartInfo.UseShellExecute = true;
							installProcess.StartInfo.CreateNoWindow = false;
							running = installProcess.Start();
						}
						else if ( MyEnvironment.Platform == MyPlatformID.Unix )
						{
							// If the platform is Unix, this code will assume
							// that a script file named, "install-ifolder.sh"
							// exists.  It will be launched to run/control the
							// installation.
							string installScriptPath = Path.Combine( downloadDir, "install-ifolder.sh" );
							if ( File.Exists( installScriptPath ) )
							{
								Console.WriteLine("path :{0}", path);
								Console.WriteLine("downloadDir :{0}", downloadDir);
								Console.WriteLine("installScriptPath :{0}", installScriptPath);
								
								Process installProcess = new Process();
								
								installProcess.StartInfo.FileName = "sh";
								installProcess.StartInfo.WorkingDirectory = downloadDir;
								installProcess.StartInfo.Arguments = 
									string.Format("{0} {1}", installScriptPath, downloadDir);
								installProcess.StartInfo.UseShellExecute = true;
								installProcess.StartInfo.CreateNoWindow = false;
								try
								{
									running = installProcess.Start();
								}
								catch{}
							}
						}
						
					}
				}
			}

			return running;
		}
		
		#endregion

		#region Public Methods
		/// <summary>
		/// Checks to see if there is a newer client application on the domain server and
		/// prompts the user to upgrade.
		/// </summary>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		public static int CheckForUpdate(string domainID, out string serverVersion)
		{
			int retval = -1;
			string ServerVersion = null;
			ClientUpgrade cu = new ClientUpgrade(domainID);
			StatusCodes stat = cu.CheckForUpdate(out ServerVersion);
			switch( stat)
			{
				case StatusCodes.Success: 
						retval = (int)UpgradeResult.Latest;
						break;
				case StatusCodes.UpgradeNeeded:
						retval = (int)UpgradeResult.UpgradeNeeded;
						break;
				case StatusCodes.ServerOld: 
						retval = (int)UpgradeResult.ServerOld;
						break;
				case StatusCodes.OlderVersion:
						retval = (int)UpgradeResult.UpgradeAvailable;
						break;
				case StatusCodes.Unknown:
						retval = (int)UpgradeResult.Unknown;
						break;
				default:
						break;
			}
			serverVersion = ServerVersion;
			return retval;
		}

		public static bool CheckForServerUpdate(string domainID)
		{
			ClientUpgrade cu = new ClientUpgrade(domainID);
			return cu.CheckForServerUpdate();
		}

		public static string CheckForUpdateAvailable(string domainID)
		{
			ClientUpgrade cu = new ClientUpgrade(domainID);
			return cu.CheckForUpdateAvailable();
		}
			
		/// <summary>
		/// Gets the updated client application and runs the installation program.
		/// Note: This call will return before the application is updated.
		/// </summary>
		/// <param name="domainID">The ID of the domain to check for updates against.</param>
		/// <returns>True if the installation program is successfully started. Otherwise false is returned.</returns>
		public static bool RunUpdate(string domainID, string path)
		{
			Console.WriteLine("RunUpdate");
			ClientUpgrade cu = new ClientUpgrade(domainID);
			return cu.RunUpdate(path);
		}

		#endregion
	}
}
