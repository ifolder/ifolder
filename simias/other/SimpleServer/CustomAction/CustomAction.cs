using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Simias
{
	/// <summary>
	/// Summary description for CustomAction.
	/// </summary>
	public class CustomAction : System.Windows.Forms.Form
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance for this object.
		/// </summary>
		public CustomAction()
		{
		}
		#endregion

		#region Private Methods

		/// <summary>
		/// Adds the required services to the 'etc\simias-client-bootstrap.config' file.
		/// </summary>
		/// <param name="applicationPath">The path to where the Simple Server application is installed.</param>
		private void AddServices( string applicationPath )
		{
			string configFilePath = Path.Combine( applicationPath, Path.Combine( "etc", "simias-client-bootstrap.config" ) );

			// Open the client bootstrap configuration file.
			XmlDocument doc = new XmlDocument();
			doc.Load( configFilePath );

			// See if there is a domain section.
			XmlElement domainSection = doc.SelectSingleNode( "/configuration/section[@name = 'Domain']" ) as XmlElement;
			if ( domainSection == null )
			{
				// Add the domain setting.
				domainSection = doc.CreateElement( "section" );
				domainSection.SetAttribute( "name", "Domain" );
				doc.DocumentElement.AppendChild( domainSection );
			}

			// See if there is a simple server setting.
			XmlElement ssElement = domainSection.SelectSingleNode( "setting[@name = 'SimpleServerName']" ) as XmlElement;
			if ( ssElement == null )
			{
				// Add the simple server setting.
				ssElement = doc.CreateElement( "setting" );
				ssElement.SetAttribute( "name", "SimpleServerName" );
				ssElement.SetAttribute( "value", "Simias SimpleServer" );
				domainSection.AppendChild( ssElement );
			}

			
			// Look for the ServiceManager element.
			XmlElement serviceElement = doc.SelectSingleNode( "/configuration/section[@name = 'ServiceManager']" ) as XmlElement;
			if ( serviceElement == null )
			{
				serviceElement = doc.CreateElement( "section" );
				serviceElement.SetAttribute( "name", "ServiceManager" );
				doc.DocumentElement.AppendChild( serviceElement );
			}


			// Look for the Services element.
			XmlElement servicesElement = serviceElement.SelectSingleNode( "setting[@name = 'Services']" ) as XmlElement;
			if ( servicesElement == null )
			{
				servicesElement = doc.CreateElement( "setting" );
				servicesElement.SetAttribute( "name", "Services" );
				serviceElement.AppendChild( servicesElement );
			}
			else
			{
				XmlElement[] childNodes = new XmlElement[ servicesElement.ChildNodes.Count ];
				for ( int i = 0; i < servicesElement.ChildNodes.Count; ++i )
				{
					childNodes[ i ] = servicesElement.ChildNodes[ i ] as XmlElement;
				}
				
				foreach( XmlElement child in childNodes )
				{
					servicesElement.RemoveChild( child );
				}
			}

			// Create the change log entry.
			XmlElement changeLog = doc.CreateElement( "Service" );
			changeLog.SetAttribute( "name", "Simias Change Log Service" );
			changeLog.SetAttribute( "assembly", "Simias" );
			changeLog.SetAttribute( "enabled", "True" );
			changeLog.SetAttribute( "type", "Thread" );
			changeLog.SetAttribute( "class", "Simias.Storage.ChangeLog" );
			servicesElement.AppendChild( changeLog );

			// Add in just the services required for simple server.
			XmlElement simiasService = doc.CreateElement( "Service" );
			simiasService.SetAttribute( "name", "Simias Simple Server" );
			simiasService.SetAttribute( "assembly", "Simias.SimpleServer.dll" );
			simiasService.SetAttribute( "enabled", "True" );
			simiasService.SetAttribute( "type", "Thread" );
			simiasService.SetAttribute( "class", "Simias.SimpleServer.Service" );
			servicesElement.AppendChild( simiasService );

			// Set the WebServicePath.
			XmlElement wsElement = serviceElement.SelectSingleNode( "setting[@name = 'WebServicePath']" ) as XmlElement;
			if ( wsElement == null )
			{
				wsElement = doc.CreateElement( "setting" );
				wsElement.SetAttribute( "name", "WebServicePath" );
			}

			wsElement.SetAttribute( "value", Path.Combine( applicationPath, "web" ) );

			// Write the changes back to the file.
			XmlTextWriter xtw = new XmlTextWriter(configFilePath, Encoding.UTF8);
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

		private void RemoveSimiasDirectory()
		{
			// Kill SimiasServer.exe
			Process[] simiasServerProcesses = Process.GetProcessesByName( "SimiasServer" );
			foreach ( Process process in simiasServerProcesses )
			{
				try
				{
					process.Kill(); // This will throw if the process is no longer running
					if ( !process.HasExited )
					{
						// Wait for the process to die.
						System.Threading.Thread.Sleep( 2000 );
					}
				}
				catch {}
				process.Close();
			}

			// Kill SimiasApp
			Process[] simiasAppProcesses = Process.GetProcessesByName( "SimiasApp" );
			foreach ( Process process in simiasAppProcesses )
			{
				try
				{
					process.Kill(); // This will throw if the process is no longer running
					if ( !process.HasExited )
					{
						// Wait for the process to die.
						System.Threading.Thread.Sleep( 2000 );
					}
				}
				catch {}
				process.Close();
			}

			// Delete the simias directory.
			string configPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "simias" );
			try
			{
				Directory.Delete( configPath, true );
			}
			catch {}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			base.Dispose( disposing );
		}

		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main( string[] args )
		{
			CustomAction ca = new CustomAction();

			try
			{
				switch( args[ 0 ].ToLower() )
				{
					case "/a":
					case "-a":
					{
						// Add the Simias Server services to the bootstrap configuration file.
						ca.AddServices( args[ 1 ] );
						break;
					}

					case "/r":
					case "-r":
					{
						// Remove the Simias directory.
						ca.RemoveSimiasDirectory();
						break;
					}
				}
			
				return 0;
			}
			catch
			{
				return -1;
			}
		}
	}
}
