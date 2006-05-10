/***********************************************************************
 *  $RCSfile: iFolder.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author: Brady Anderson (banderso@novell.com)
 * 
 ***********************************************************************/
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Xml;

namespace iFolder.Client
{
	public enum Rights 
	{
		/// <remarks/>
		Deny,
    
		/// <remarks/>
		ReadOnly,
    
		/// <remarks/>
		ReadWrite,
    
		/// <remarks/>
		Admin,
	}

	/// <summary>
	/// An iFolder object
	/// </summary>
	public class iFolder
	{
		#region iFolder Types
		/// <summary>
		/// iFolder Information
		/// </summary>
		private string name = String.Empty;
		private string id = String.Empty;
		private string description = String.Empty;
		private Domain domain = null;
		private string path;
		
		/// <summary>
		/// User Information
		/// </summary>
		private string ownerid = String.Empty;
		private string ownername = String.Empty;
		private Rights rights;

		/// <summary>
		/// Web Service Paths
		/// </summary>
		private readonly static string iFolderWebPath = "/simias10/iFolderWeb.asmx";

		private static bool certificatePolicyInitialized = false;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the friendly name of the iFolder
		/// </summary>
		public string Name
		{
			get 
			{ 
				return name;
			}
		}
		
		/// <summary>
		/// Gets the unique id of the iFolder
		/// </summary>
		public string ID
		{
			get 
			{ 
				return id;
			}
		}
		
		/// <summary>
		/// iFolder's detailed description
		/// Note: optional property
		/// </summary>
		public string Description
		{
			get 
			{ 
				return description;
			}
		}
		
		/// <summary>
		/// Gets the members right's to the iFolder
		/// </summary>
		public Rights Rights
		{
			get 
			{ 
				return rights;
			}
		}

		/// <summary>
		/// Gets the domain the ifolder belongs to
		/// </summary>
		public string DomainID
		{
			get 
			{ 
				return domain.ID;
			}
		}

		/// <summary>
		/// Gets the user id of the iFolder owner
		/// </summary>
		public string OwnerID
		{
			get 
			{ 
				return ownerid;
			}
		}

		/// <summary>
		/// Gets the user name of the iFolder owner
		/// </summary>
		public string OwnerName
		{
			get 
			{ 
				return ownername;
			}
		}

		/// <summary>
		/// Gets the configured local path for the iFolder
		/// N/A on remote or available iFolders
		/// </summary>
		public string LocalPath
		{
			get 
			{ 
				return path;
			}
		}
		#endregion

		#region Constructors
		private iFolder()
		{
			SetDefaultiFolderDirectory();
		}

		private iFolder( Domain domain )
		{
			SetDefaultiFolderDirectory();
			this.domain = domain;
		}

		private iFolder( string iFolderID )
		{
			SetDefaultiFolderDirectory();
			this.id = iFolderID;

			string configPath = GetiFolderConfigPathFromID( iFolderID );
			if ( configPath == null )
			{
				throw new UnknowniFolderException( iFolderID );
			}

			this.ConfigFileToiFolder( configPath );
		}

		private iFolder( Domain domain, string iFolderID )
		{
			SetDefaultiFolderDirectory();
			this.domain = domain;

			string configPath = GetiFolderConfigPathFromID( iFolderID );
			if ( configPath == null )
			{
				throw new UnknowniFolderException( iFolderID );
			}

			ConfigFileToiFolder( configPath );
		}
		#endregion
		
		#region Private Methods
		/// <summary>
		/// Private method to retrieve a saved iFolder
		/// configuration file and deserialize the
		/// contents back to an iFolder object.
		/// </summary>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		private void ConfigFileToiFolder( string configFile )
		{
			XmlDocument doc = new XmlDocument();
			doc.Load( configFile );

			// Make sure the document is valid and then process
			XmlNode rootNode = doc.FirstChild;

			// If there is an xml declaration attached to the document
			// move past it
			if ( rootNode.NodeType == XmlNodeType.XmlDeclaration )
			{
				rootNode = rootNode.NextSibling;
				if ( rootNode == null )
				{
					throw new ApplicationException( "Not a valid iFolder configuration" );
				}
			}

			if ( rootNode.NodeType != XmlNodeType.Element )
			{
				throw new ApplicationException( "Not a valid iFolder configuration" );
			}
		
			if ( rootNode.Name.ToLower() != "ifolder" )
			{
				throw new ApplicationException( "Not a valid ifolder configuration" );
			}

			// get the attributes
			foreach( XmlAttribute attr in rootNode.Attributes )
			{
				switch( attr.Name.ToLower() )
				{
					case "id":
					{
						this.id = attr.InnerText;
						break;
					}

					case "name":
					{
						this.name = attr.InnerText;
						break;
					}

					case "description":
					{
						this.description = attr.InnerText;
						break;
					}

					case "path":
					{
						this.path = attr.InnerText;
						break;
					}

					case "rights":
					{
						switch( attr.InnerText.ToLower() )
						{
							case "deny":
							{
								this.rights = Rights.Deny;
								break;
							}

							case "readonly":
							{
								this.rights = Rights.ReadOnly;
								break;
							}

							case "readwrite":
							{
								this.rights = Rights.ReadWrite;
								break;
							}

							case "admin":
							{
								this.rights = Rights.Admin;
								break;
							}
						}
						break;
					}
				}
			}

			// Get member information, host information and default info
			foreach( XmlNode node in rootNode.ChildNodes )
			{
				switch( node.Name.ToLower() )
				{
					case "owner":
					{
						XmlAttribute attr = node.Attributes[ "id" ];
						if ( attr != null )
						{
							this.ownerid = attr.InnerText;
						}

						attr = node.Attributes[ "name" ];
						if ( attr != null )
						{
							this.ownername = attr.InnerText;
						}
						break;
					}

					case "domain":
					{
						XmlAttribute attr = node.Attributes[ "id" ];
						if ( attr != null )
						{
							this.domain = Domain.GetDomainByID( attr.InnerText );
						}
						break;
					}
				}
			}
		}

		/// <summary>
		/// Method to delete a local configuration file
		/// for a subscribed iFolder
		/// </summary>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		static private void DeleteConfigurationFile( string iFolderID )
		{
			// Build a path to store/ifolder
			string fullPath = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			if ( fullPath == null || fullPath.Length == 0 )
			{
				fullPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
			}

			fullPath += 
				Path.DirectorySeparatorChar.ToString() + 
				Configuration.StoreComponent +
				Path.DirectorySeparatorChar.ToString() + 
				Configuration.iFolderComponent +
				Path.DirectorySeparatorChar.ToString() +
				iFolderID;

			if ( File.Exists( fullPath ) == true )
			{
				File.Delete( fullPath );
			}
			else
			{
				throw new UnknowniFolderException( iFolderID );
			}
		}

		/// <summary>
		/// Private method to return a full path
		/// to an iFolder configuration file.
		/// </summary>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		private string GetiFolderConfigPathFromID( string iFolderID )
		{
			string fullPath = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			if ( fullPath == null || fullPath.Length == 0 )
			{
				fullPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
			}

			fullPath += 
				Path.DirectorySeparatorChar.ToString() + 
				Configuration.StoreComponent +
				Path.DirectorySeparatorChar.ToString() + 
				Configuration.iFolderComponent + 
				Path.DirectorySeparatorChar.ToString() +
				iFolderID;
			if ( File.Exists( fullPath ) == true )
			{
				return fullPath;
			}

			throw new UnknowniFolderException( iFolderID );
		}


		/// <summary>
		private XmlDocument iFolderToXmlDocument()
		{
			XmlDocument doc = new XmlDocument();
			XmlNode n = doc.CreateXmlDeclaration( "1.0", "utf-8", "" );
			doc.AppendChild( n );

			XmlNode root = doc.CreateElement( "ifolder" );
			XmlAttribute attr = doc.CreateAttribute( "id" );
			attr.Value = this.id;
			root.Attributes.SetNamedItem( attr );

			attr = doc.CreateAttribute( "name" );
			attr.Value = this.name;
			root.Attributes.SetNamedItem( attr );

			if ( this.description != String.Empty )
			{
				attr = doc.CreateAttribute( "description" );
				attr.Value = this.description;
				root.Attributes.SetNamedItem( attr );
			}

			attr = doc.CreateAttribute( "rights" );
			attr.Value = this.rights.ToString();
			root.Attributes.SetNamedItem( attr );

			attr = doc.CreateAttribute( "path" );
			attr.Value = this.path;
			root.Attributes.SetNamedItem( attr );

			XmlNode node;
			node = doc.CreateElement( "owner" );
			attr = doc.CreateAttribute( "id" );
			attr.Value = this.ownerid;
			node.Attributes.SetNamedItem( attr );
			attr = doc.CreateAttribute( "name" );
			attr.Value = this.ownername;
			node.Attributes.SetNamedItem( attr );

			root.AppendChild( node );

			node = doc.CreateElement( "domain" );
			attr = doc.CreateAttribute( "id" );
			attr.Value = this.domain.ID;
			node.Attributes.SetNamedItem( attr );
			attr = doc.CreateAttribute( "name" );
			attr.Value = this.domain.Name;
			node.Attributes.SetNamedItem( attr );
			root.AppendChild( node );

			doc.AppendChild( root );
			return doc;
		}

		/// <summary>
		/// Private method to setup the iFolderWeb proxy object.
		/// </summary>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		private void iFolderWebToiFolder( iFolderClient.iFolder ifldr )
		{
			this.id = ifldr.ID;
			this.name = ifldr.Name;
			this.description = ifldr.Description;
			this.rights = ( Rights ) ifldr.Rights;
			this.ownerid = ifldr.OwnerID;
			this.ownername = ifldr.OwnerUserName;
		}

		/// <summary>
		/// Method to setup the default iFolder path
		/// </summary>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		private void SetDefaultiFolderDirectory()
		{
			// Build a path to the default iFolder directory
			path = Environment.GetFolderPath( Environment.SpecialFolder.Personal );
			if ( path == null || path.Length == 0 )
			{
				path = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
			}

			path += Path.DirectorySeparatorChar.ToString() + Configuration.DefaultiFolderDirectory;
		}

		/// <summary>
		/// Private method to setup the iFolderWeb proxy object.
		/// </summary>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		private iFolderClient.iFolderWeb SetupWebService( Domain domain )
		{
			iFolderClient.iFolderWeb ifweb = new iFolderClient.iFolderWeb();
			ifweb.CookieContainer = domain.Host.cookies;
			ifweb.Url = domain.Host.Address + iFolderWebPath;
			//domainService.Proxy = ProxyState.GetProxyState( domainServiceUrl );
			
			ifweb.Credentials = domain.Credentials;
			ifweb.PreAuthenticate = true;
		
			return ifweb;
		}

		private void Persist()
		{
			XmlDocument doc = iFolderToXmlDocument();

			// Build a path to the store/domain
			string fullPath = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			if ( fullPath == null || fullPath.Length == 0 )
			{
				fullPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
			}

			fullPath += Path.DirectorySeparatorChar.ToString() + Configuration.StoreComponent;
			if ( Directory.Exists( fullPath ) == false )
			{
				Directory.CreateDirectory( fullPath );
			}

			fullPath += Path.DirectorySeparatorChar.ToString() + Configuration.iFolderComponent;
			if ( Directory.Exists( fullPath ) == false )
			{
				Directory.CreateDirectory( fullPath );
			}

			fullPath += Path.DirectorySeparatorChar.ToString() + this.ID;
			doc.Save( fullPath );
			return;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Static method to create an iFolder in a specified
		/// domain.
		/// Note: This method creates the iFolder on the remote
		/// server followed by a local subscribe.
		/// </summary>
		/// <param name="domain">Domain the iFolder belongs to</param>
		/// <param name="name">name</param>
		/// <param name="description">description</param>
		/// <returns>iFolder object - any failure throws an exception</returns>
		static public iFolder Create( Domain domain, string name, string description, string path )
		{
			if ( name == null || name == String.Empty )
			{
				throw new ArgumentNullException( name );
			}

			if ( description == null )
			{
				description = String.Empty;
			}

			iFolder ifolder = new iFolder( domain );
			iFolderClient.iFolderWeb ifweb = ifolder.SetupWebService( domain );
			iFolderClient.iFolder ifldr = ifweb.CreateiFolder( name, description );
			if ( ifldr != null )
			{
				if ( path != null && path != String.Empty )
				{
					if ( Directory.Exists( path ) == true )
					{
						ifolder.path = path;
					}
				}

				ifolder.iFolderWebToiFolder( ifldr );
				ifolder.Persist();
				return ifolder;
			}

			throw new CreateiFolderException( domain.ID, name );
		}
		
		/// <summary>
		/// Static method to delete an iFolder.
		/// A local subscription, if it exists, is deleted
		/// first followed by a delete of the remote iFolder.
		/// </summary>
		/// <param name="domain">Domain the iFolder belongs to</param>
		/// <param name="ifolderID">Unique iFolder ID</param>
		/// <returns>any failure throws an exception</returns>
		static public void Delete( Domain domain, string iFolderID )
		{
			if ( domain == null )
			{
				throw new ArgumentNullException( "domain" );
			}

			if ( iFolderID == null || iFolderID == String.Empty )
			{
				throw new ArgumentNullException( "iFolderID" );
			}

			// Delete this ifolder if configured on this machine.
			try
			{
				iFolder.DeleteConfigurationFile( iFolderID );
			}
			catch{}

			iFolder ifolder = new iFolder( domain );
			iFolderClient.iFolderWeb ifweb = ifolder.SetupWebService( domain );
			ifweb.DeleteiFolder( iFolderID );
		}

		/// <summary>
		/// Static method to subscribe to an available iFolder.
		/// This method does not provide a local file system
		/// path, which means the caller wants to mount this
		/// iFolder in the default iFolder path.
		/// </summary>
		/// <param name="domain">Domain the iFolder belongs to</param>
		/// <param name="iFolderID">Unique ID of the available iFolder.</param>
		/// <returns>iFolder object - any failure throws an exception</returns>
		static public iFolder Subscribe( Domain domain, string iFolderID )
		{
			iFolder ifolder = new iFolder( domain );
			iFolderClient.iFolderWeb ifweb = ifolder.SetupWebService( domain );

			iFolderClient.iFolder ifldr = ifweb.GetiFolder( iFolderID );
			if ( ifldr != null )
			{
				ifolder.iFolderWebToiFolder( ifldr );
				ifolder.Persist();

				return ifolder;
			}

			throw new UnknowniFolderException( iFolderID );
		}

		/// <summary>
		/// Static method to unsubscribe from a locally
		/// subscribed iFolder.
		/// </summary>
		/// <param name="iFolderID">Unique ID of the iFolder</param>
		/// <returns>Throws an exception upon failure</returns>
		static public void Unsubscribe( string iFolderID )
		{
			iFolder.DeleteConfigurationFile( iFolderID );
		}

		/// <summary>
		/// Static method to get all available iFolders
		/// the user owns or is a member of for the 
		/// specified domain.
		/// </summary>
		/// <param name="host">Host the caller is logging into</param>
		/// <param name="username">Friendly name of the user.</param>
		/// <param name="password">Password of the user.</param>
		/// <returns>Domain object</returns>
		static public iFolder[] GetAvailable( Domain domain )
		{
			// Get the list of iFolders this user is already
			// subscribed to.
			iFolder[] subscribed = iFolder.GetSubscribed();

			// BUGBUG need a paging method
			ArrayList available = new ArrayList();
			
			iFolder ifolder = new iFolder( domain );
			iFolderClient.iFolderWeb ifweb = ifolder.SetupWebService( domain );
			
			bool dontreturn;
			int total = 0;
			iFolderClient.iFolder[] ifolders = ifweb.GetiFolders( 0, 100000, out total );
			foreach( iFolderClient.iFolder ifldr in ifolders )
			{
				dontreturn = false;
				// Exist in the subscribed list?
				foreach( iFolder nif in subscribed )
				{
					if ( nif.ID == ifldr.ID )
					{
						dontreturn = true;
						break;
					}
				}

				if ( dontreturn == true )
				{
					continue;
				}

				ifolder = new iFolder( domain );
				ifolder.iFolderWebToiFolder( ifldr );
				available.Add( ifolder );
			}
			
			return available.ToArray( typeof( iFolder ) ) as iFolder[];
		}

		/// <summary>
		/// Static method to retrieve all the added/configured
		/// domains on this system.
		/// </summary>
		/// <param name="host">Host the caller is logging into</param>
		/// <param name="username">Friendly name of the user.</param>
		/// <param name="password">Password of the user.</param>
		/// <returns>Domain object</returns>
		static public iFolder[] GetSubscribed()
		{
			ArrayList ifolders = new ArrayList();
			try
			{
				// Build a path to the store/domain
				string fullPath = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
				if ( fullPath == null || fullPath.Length == 0 )
				{
					fullPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
				}

				fullPath += 
					Path.DirectorySeparatorChar.ToString() + 
					Configuration.StoreComponent +
					Path.DirectorySeparatorChar.ToString() + 
					Configuration.iFolderComponent;

				// GetFiles returns file in full path format
				string[] files = Directory.GetFiles( fullPath );
				foreach( string iFolderConfig in files )
				{
					iFolder ifolder = new iFolder();
					ifolder.ConfigFileToiFolder( iFolderConfig );
					ifolders.Add( ifolder );
				}
			}
			catch( System.Exception gd )
			{
				// log
			}

			return ifolders.ToArray( typeof( iFolder ) ) as iFolder[];
		}

		/// <summary>
		/// Static method to newup a subscribed iFolder by its unique ID
		/// </summary>
		/// <param name="domainID">Unique ID of the iFolder to newup</param>
		/// <returns>iFolder object</returns>
		static public iFolder GetiFolderByID( string iFolderID )
		{
			Domain domain = Domain.GetDefaultDomain();
			return new iFolder( domain, iFolderID );
		}

		/// <summary>
		/// Static method to newup a subscribed iFolder by its unique ID
		/// with a specified domain.
		/// </summary>
		/// <param name="domain">Specified domain</param>
		/// <param name="iFolderID">Unique ID of the iFolder to newup</param>
		/// <returns>iFolder object</returns>
		static public iFolder GetiFolderByID( Domain domain, string iFolderID )
		{
			return new iFolder( domain, iFolderID );
		}
		#endregion
	}
}	

