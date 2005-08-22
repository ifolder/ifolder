using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using Microsoft.Win32;

namespace Novell.iFolder
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class WorkgroupInstall : System.Windows.Forms.Form
	{

		#region Constructor
		/// <summary>
		/// Instantiates a new object.
		/// </summary>
		public WorkgroupInstall()
		{
		}
		#endregion

		#region Private Methods
		private void configureBonjour(string configFilePath, bool enabled)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(configFilePath);

		
			// Look for the ServiceManager element.
			XmlElement serviceMgrElement = doc.SelectSingleNode( "/configuration/section[@name = 'ServiceManager']" ) as XmlElement;


			// Look for the Services element.
			XmlElement servicesElement = serviceMgrElement.SelectSingleNode( "setting[@name = 'Services']" ) as XmlElement;


			// Look for the Bonjour service.
			XmlElement bonjourSvcElement = servicesElement.SelectSingleNode( "Service[@name='Bonjour Domain']" ) as XmlElement;
			if ( bonjourSvcElement == null)
			{
				// Create the Bonjour entry.
				bonjourSvcElement = doc.CreateElement( "Service" );
				bonjourSvcElement.SetAttribute( "name", "Bonjour Domain" );
				bonjourSvcElement.SetAttribute( "assembly", "Simias.Bonjour.dll" );
				bonjourSvcElement.SetAttribute( "enabled", enabled.ToString() );
				bonjourSvcElement.SetAttribute( "type", "Thread" );
				bonjourSvcElement.SetAttribute( "class", "Simias.mDns.Service" );
				servicesElement.AppendChild( bonjourSvcElement );
			}
			else
			{
				bonjourSvcElement.SetAttribute( "enabled", enabled.ToString() );
			}

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
		static int Main(string[] args) 
		{
			WorkgroupInstall wgInstall = new WorkgroupInstall();

			string bootstrapConfigPath = null;

			try
			{
				RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"CLSID\{AA81D830-3B41-497C-B508-E9D02F8DF421}\InprocServer32");
				string installPath = Path.GetDirectoryName(key.GetValue(null) as string);
				bootstrapConfigPath = Path.Combine(installPath, @"etc\simias-client-bootstrap.config");
			}
			catch {}

			try
			{

				string configFilePath = Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), @"simias\simias.config");
				switch (args[0].ToLower())
				{
					case "-e": // enable
					case "/e":
						if (bootstrapConfigPath != null)
						{
							wgInstall.configureBonjour(bootstrapConfigPath, true);
						}

						try
						{
							wgInstall.configureBonjour(configFilePath, true);
						}
						catch (FileNotFoundException) {}
						break;
					case "-d": // disable
					case "/d":
						if (bootstrapConfigPath != null)
						{
							wgInstall.configureBonjour(bootstrapConfigPath, false);
						}

						try
						{
							wgInstall.configureBonjour(configFilePath, false);
						}
						catch (FileNotFoundException) {}
						break;
				}
			}
			catch
			{
				return -1;
			}

			return 0;
		}
	}
}
