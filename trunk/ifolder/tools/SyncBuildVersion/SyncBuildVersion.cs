using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace Novell.iFolder.Build
{
	/// <summary>
	/// Copies the product version from iFolderApp.exe to ifolder-msi.ism.
	/// </summary>
	class SyncBuildVersion
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if ( args.Length != 2 )
			{
				Console.WriteLine( "Useage: iFolderVersionUpdate <iFolder Windows Package Directory> <Final bin directory>" );
				Environment.Exit( -2 );
			}

			// Find the ifolder application.
			string iFolderAppFileName = Path.Combine( args[ 1 ], "iFolderApp.exe" );
			if ( !File.Exists( iFolderAppFileName ) )
			{
				Console.WriteLine( "Error: Cannot file file: {0}", iFolderAppFileName );
				Environment.Exit( -1 );
			}

			// Get the ifolder application version.
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo( iFolderAppFileName );
			if ( versionInfo.ProductVersion == null )
			{
				Console.WriteLine( "Cannot get version from file: {0}", iFolderAppFileName );
			}

			// Find the MSI file.
			string msiFileName = Path.Combine( args[ 0 ], "ifolder-msi.ism" );
			if ( !File.Exists( msiFileName ) )
			{
				Console.WriteLine( "Error: Cannot file file: {0}", msiFileName );
				Environment.Exit( -1 );
			}

			// Tell what we're attempting to do.
			Console.WriteLine( "Updating version: {0} from {1} to {2}", versionInfo.ProductVersion, iFolderAppFileName, msiFileName );

			bool updatedVersion = false;
			try
			{
				// Load the file into an xml document.
				XmlDocument document = new XmlDocument();
				document.Load( msiFileName );

				// Create an XmlNamespaceManager for resolving namespaces.
				XmlNamespaceManager nsmgr = new XmlNamespaceManager( document.NameTable );
				nsmgr.AddNamespace( "dt", document.DocumentElement.NamespaceURI );

				// Search for the named service element.
				XmlNode parentNode = document.DocumentElement.SelectSingleNode( "table[@name='Property']", nsmgr );
				if ( parentNode != null )
				{
					// Now find the Product Version element.
					XmlNode childNode = parentNode.SelectSingleNode( "row/td[.='ProductVersion']", nsmgr );
					if ( childNode != null )
					{
						// The next sibling will be the version information.
						XmlNode versionNode = childNode.NextSibling;
						if ( versionNode != null )
						{
							// Set the new version.
							versionNode.InnerText = versionInfo.ProductVersion;

							// Save the XML back out to the file.
							XmlTextWriter xtw = new XmlTextWriter( msiFileName, Encoding.ASCII );
							try
							{
								xtw.Formatting = Formatting.Indented;
								document.WriteTo( xtw );
								updatedVersion = true;
							}
							finally
							{
								xtw.Close();
							}
						}
					}
				}
			}
			catch
			{}

			// Exit out the right way if the program was successful.
			Environment.Exit( updatedVersion ? 0 : -1 );
		}
	}
}
