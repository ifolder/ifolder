using System;
using System.Diagnostics;
using System.IO;
using System.Net;

using Simias.Client;

namespace Novell.iFolder.Install
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class ClientUpgrade
	{
		#region Class Members
		/// <summary>
		/// iFolder Client update files.
		/// </summary>
		private static string iFolderWindowsApplication = "iFolderApp.exe";

		/// <summary>
		/// Web service object to use for checking for client updates.
		/// </summary>
		private ClientUpdate service;

		/// <summary>
		/// Address to the host where the web service is running.
		/// </summary>
		private string hostAddress;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object.
		/// </summary>
		/// <param name="domainID">Identifier of the domain that the user belongs to.</param>
		/// <param name="userName">The name of the user making the request.</param>
		/// <param name="password">The user's password.</param>
		public ClientUpgrade( string domainID, string userName, string password )
		{
			// Build a network credential.
			NetworkCredential credential = BuildCredential( domainID, userName, password );
			if ( credential == null )
			{
				throw new ApplicationException( "No credential exists for specified user." );
			}

			// Get the address of the host service.
			DomainConfig domainCfg = new DomainConfig( domainID );
			hostAddress = domainCfg.ServiceUrl.ToString();

			// Setup the url to the server.
			service = new ClientUpdate();
			service.Url = hostAddress + "/ClientUpdate.asmx";
			service.CookieContainer = new CookieContainer();
			service.Credentials = credential;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Builds a NetworkCredential for the specified user.
		/// </summary>
		/// <param name="domainID">Identifier of the domain the user belongs to.</param>
		/// <param name="userName">Name of the user.</param>
		/// <param name="password">User's password.</param>
		/// <returns>A NetworkCredential object if successful. Otherwise a null is returned.</returns>
		private NetworkCredential BuildCredential( string domainID, string userName, string password )
		{
			NetworkCredential credential = null;

			// Connect to the local web service.
			SimiasWebService simiasSvc = new SimiasWebService();
			simiasSvc.Url = Manager.LocalServiceUrl.ToString() + "/Simias.asmx";

			// Get the local domain information.
			DomainInformation domainInfo = simiasSvc.GetDomainInformation( domainID );
			if ( domainInfo != null )
			{
				credential = new NetworkCredential( domainInfo.MemberName, password, domainInfo.RemoteUrl );
			}
			
			return credential;
		}

		/// <summary>
		/// Downloads the specified files in the list from the server.
		/// </summary>
		/// <param name="fileList">List of files to download.</param>
		/// <returns>The path to the downloaded files.</returns>
		private string DownloadFiles( string[] fileList )
		{
			// Create a temporary directory.
			string downloadDir = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString() );
			Directory.CreateDirectory( downloadDir );
		
			try
			{
				// Construct a WebClient to do the downloads.
				WebClient webClient = new WebClient();
				foreach ( string file in fileList )
				{
					string destFile = Path.Combine( downloadDir, Path.GetFileName( file ) );
					webClient.DownloadFile( Path.Combine( hostAddress, file ), destFile );
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
		#endregion

		#region Public Methods
		/// <summary>
		/// Checks to see if there is a newer client application on the domain server and
		/// prompts the user to upgrade.
		/// </summary>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		public string CheckForUpdate()
		{
			string updateVersion = null;

			// Get the current version of this client.
			string currentVersion = null;
			if ( MyEnvironment.Platform == MyPlatformID.Windows )
			{
				currentVersion = GetWindowsClientVersion();
			}
			else
			{
				// TODO: Get the current client version for Linux.
			}

			if ( currentVersion != null )
			{
				// Call to the web service to see if there is a version newer than the one
				// that is currently running.
				updateVersion = service.IsUpdateAvailable( MyEnvironment.Platform.ToString(), currentVersion );
			}

			return updateVersion;
		}

		/// <summary>
		/// Gets the updated client application and runs the installation program.
		/// Note: This call will return before the application is updated.
		/// </summary>
		/// <returns>True if the installation program is successfully started. Otherwise false is returned.</returns>
		public bool RunUpdate()
		{
			bool running = false;

			// Get the list of files needed for the update.
			string[] fileList = service.GetUpdateFiles();
			if ( fileList != null )
			{
				// Download the files in the list to a temporary directory.
				string downloadDir = DownloadFiles( fileList );
				if ( downloadDir != null )
				{
					if ( MyEnvironment.Platform == MyPlatformID.Windows )
					{
						// There should only be one file needed for the windows update.
						Process installProcess = new Process();
						installProcess.StartInfo.FileName = Path.Combine( downloadDir, Path.GetFileName( fileList[ 0 ] ) );
						installProcess.StartInfo.UseShellExecute = true;
						installProcess.StartInfo.CreateNoWindow = false;
						running = installProcess.Start();
					}
					else
					{
						// TODO: Run the Linux install.
					}
				}
			}

			return running;
		}
		#endregion
	}
}
