/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

using Simias;
using Simias.Storage;

namespace Simias.mDns
{
	/// <summary>
	/// Class that represents a mDns Domain object in the Collection Store.
	/// </summary>
	public class Domain
	{
		#region Class Members

		/// <summary>
		/// Well known identitifer for mDns workgroup.
		/// </summary>
		public static readonly string ID = "74d3a71f-daae-4a36-b9f3-6466081f6401";

		/// <summary>
		/// Friendly name for the workgroup domain.
		/// </summary>
		private string mDnsDomainName = "Peer-to-Peer (P2P)";

		private string description = "";
		private string mDnsHostName;
		private string mDnsUserName;
		private string mDnsUserID;
		//private string mDnsPOBoxID;

		// For saving mdns user info to a xml file
		private const string configDir = "mdns";
		private const string configFile = "mdnsinfo.xml";
		private const string rootLabel = "MemberInfo";
		private const string nameLabel = "Name";
		private const string idLabel = "ID";

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Properties

		/// <summary>
		/// Gets the mDnsDomain's friendly ID
		/// </summary>
		public string Name
		{
			get { return( this.mDnsDomainName ); }
		}

		/// <summary>
		/// Gets the mDnsDomain's description
		/// </summary>
		public string Description
		{
			get { return( this.description ); }
		}

		/// <summary>
		/// Gets the current mDns Host Name
		/// </summary>
		public string Host
		{
			get { return( this.mDnsHostName ); }
		}

		/// <summary>
		/// Gets the mDnsDomain's current user
		/// </summary>
		public string UserName
		{
			get { return( this.mDnsUserName ); }
		}

		/// <summary>
		/// Gets the mDnsDomain current user's ID
		/// </summary>
		public string UserID
		{
			get { return( this.mDnsUserID ); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for creating a new mDnsDomain object.
		/// </summary>
		internal Domain( )
		{
			this.Init( null );
		}

		/// <summary>
		/// Constructor for creating a new mDnsDomain object.
		/// </summary>
		/// <param name="description">String that describes this domain.</param>
		internal Domain( string description ) 
		{
			this.Init( description );
		}
		#endregion

		internal void Init( string createDescription )
		{
			bool firstTime = false;
			Member member = null;
			Store store = Store.GetStore();

			try
			{
				//
				// Verify the Peers (bonjour) domain exists
				//

				Simias.Storage.Domain rDomain = store.GetDomain( Simias.mDns.Domain.ID );
				if (rDomain == null)
				{
					mDnsHostName = Environment.MachineName;
					mDnsUserName = Environment.UserName + "@" + Environment.MachineName;

					if ( createDescription != null && createDescription != "" )
					{
						description = createDescription;
					}
					else
					{
						description = mDnsUserName + "'s " + mDnsDomainName + " Domain";
					}

					// Create the Peers (bonjour) domain
					rDomain = 
						new Simias.Storage.Domain(
							store, 
							this.mDnsDomainName,
							Simias.mDns.Domain.ID,
							this.description, 
							Simias.Sync.SyncRoles.Master,
							Simias.Storage.Domain.ConfigurationType.Workgroup );

					rDomain.SetType( rDomain, "Peer-to-Peer (P2P)" );

					// See if we have setup an mdns user on this box previously
					member = this.GetMemberIDFromFile( mDnsUserName );
					if ( member == null )
					{
						// Create the owner member for the domain.
						member = 
							new Member( 
								mDnsUserName, 
								Guid.NewGuid().ToString().ToLower(), 
								Access.Rights.Admin );
						firstTime = true;
					}

					member.IsOwner = true;
					mDnsUserID = member.UserID;
					rDomain.Commit( new Node[] { rDomain, member } );

					// Create the name mapping.
					store.AddDomainIdentity( rDomain.ID, member.UserID );

					if ( firstTime == true )
					{
						SaveMemberInfoToFile( member );
					}
				}
				else
				{
					member = rDomain.Owner;
					if ( member == null )
					{
						string errMsg = String.Format( "Cannot find the owner of domain {1}.", rDomain.Name );
						log.Error( errMsg );
						throw new SimiasException( errMsg );
					}

					mDnsUserName = member.Name;
					mDnsUserID = member.UserID;
					mDnsHostName = Environment.MachineName;
				}

				//
				// Verify the POBox for the local user
				//

				Simias.POBox.POBox.GetPOBox( store, rDomain.ID, mDnsUserID );
				//Simias.POBox.POBox poBox = Simias.POBox.POBox.GetPOBox( store, rDomain.ID, mDnsUserID );
				//mDnsPOBoxID = poBox.ID;
			}
			catch( Exception e1 )
			{
				log.Error( e1.Message );
				log.Error( e1.StackTrace );

				throw e1;
			}			
		}

		/// <summary>
		/// Method to prod the domain to see if the host name has changed.
		/// If it has the domain instance will update itself and return true.
		/// If not false is returned
		/// </summary>
		internal bool CheckForHostChange()
		{
			if ( mDnsHostName != Environment.MachineName )
			{
				mDnsHostName = Environment.MachineName;
				mDnsUserName = Environment.UserName + "@" + Environment.MachineName;
				return true;
			}

			return false;
		}

			/// <summary>
		/// Method to use previously saved mdns user info.
		/// </summary>
		internal Member GetMemberInfoFromFile()
		{
			Member	member = null;
			string	id = null;
			string	name = null;

			string path = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			if ( ( path == null ) || ( path.Length == 0 ) )
			{
				path = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
				if ( ( path == null ) || ( path.Length == 0 ) )
				{
					return null;
				}
			}

			path = path + Path.DirectorySeparatorChar + configDir;
			if ( Directory.Exists( path ) == false )
			{
				return null;
			}

			path += Path.DirectorySeparatorChar + configFile;

			// Load the configuration document from the file.
			XmlDocument mdnsInfo = new XmlDocument();
			try
			{
				mdnsInfo.Load( path ); 
				XmlElement domainElement = mdnsInfo.DocumentElement;
				if ( domainElement.Name == rootLabel )
				{
					for ( int i = 0; i < domainElement.ChildNodes.Count; i++ )
					{
						if ( domainElement.ChildNodes[i].Name == nameLabel )
						{
							name = domainElement.ChildNodes[i].InnerText;
						}
						else
						if ( domainElement.ChildNodes[i].Name == idLabel )
						{
							id = domainElement.ChildNodes[i].InnerText;
						}
					}
				}

				if ( name != null && id != null )
				{
					member = new Member( name, id, Access.Rights.Admin );
				}
			}
			catch{}
			return member;
		}

		/// <summary>
		/// Method to use previously saved mdns user ID.
		/// </summary>
		internal Member GetMemberIDFromFile( string friendlyName )
		{
			Member	member = null;
			string	id = null;
			//string	name = null;

			string path = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			if ( ( path == null ) || ( path.Length == 0 ) )
			{
				path = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
				if ( ( path == null ) || ( path.Length == 0 ) )
				{
					return null;
				}
			}

			path = path + Path.DirectorySeparatorChar + configDir;
			if ( Directory.Exists( path ) == false )
			{
				return null;
			}

			path += Path.DirectorySeparatorChar + configFile;

			// Load the configuration document from the file.
			XmlDocument mdnsInfo = new XmlDocument();
			try
			{
				mdnsInfo.Load( path ); 
				XmlElement domainElement = mdnsInfo.DocumentElement;
				if ( domainElement.Name == rootLabel )
				{
					for ( int i = 0; i < domainElement.ChildNodes.Count; i++ )
					{
						if ( domainElement.ChildNodes[i].Name == idLabel )
						{
							id = domainElement.ChildNodes[i].InnerText;
						}
					}
				}

				if ( id != null )
				{
					member = new Member( friendlyName, id, Access.Rights.Admin );
				}
			}
			catch{}
			return member;
		}

		/// <summary>
		/// Method to save the member's name and GUID to a config file.
		/// The purpose here is to attempt to always use the same GUID 
		/// for the mdns user on this machine.  If a user deletes their
		/// Simias store and restarts we should be able to use the same
		/// mdns user and guid that was previously used.
		/// </summary>
		internal bool SaveMemberInfoToFile( Member member )
		{
			string path = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			if ( ( path == null ) || ( path.Length == 0 ) )
			{
				path = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
				if ( path == null || path.Length == 0 )
				{
					return false;
				}
			}

			path += Path.DirectorySeparatorChar + configDir;
			if ( Directory.Exists( path ) == false )
			{
				Directory.CreateDirectory( path );
			}

			// Delete the file if it exists
			path += Path.DirectorySeparatorChar + configFile;
			File.Delete( path );

			// Create an XML document that will be serialized to the file
			// Load the configuration document from the file.
			XmlTextWriter writer = new XmlTextWriter( path, null );
			writer.Formatting = Formatting.Indented;

			writer.WriteStartDocument( true );
			writer.WriteStartElement( rootLabel );
			writer.WriteElementString( nameLabel, member.Name );
			writer.WriteElementString( idLabel, member.UserID );
			writer.WriteEndElement(); 

			writer.Flush();
			writer.Close();

			return true;
		}

		#region Public Methods

		/// <summary>
		/// Checks if the local/master Peers (bonjour) domain exists.
		/// </summary>
		/// <returns>true if the domain exists otherwise false</returns>
		public bool Exists()
		{
			bool exists = false;
			Simias.Storage.Domain mdnsDomain = null;

			try
			{
				Store store = Store.GetStore();
				mdnsDomain = store.GetDomain( ID );
				if ( mdnsDomain != null )
				{
					//Member owner = mdnsDomain.Owner;
					mDnsUserID = mdnsDomain.Owner.UserID; //mdnsDomain.GetMemberByName( mDnsUserName ).ID;
					mDnsUserName = mdnsDomain.Owner.Name;
					Simias.POBox.POBox.FindPOBox( store, ID, mDnsUserID );
					//Simias.POBox.POBox pobox = Simias.POBox.POBox.FindPOBox( store, ID, mDnsUserID );
					//mDnsPOBoxID = pobox.ID;
					exists = true;
				}
			}
			catch{}
			return exists;
		}

		/// <summary>
		/// Obtains the string representation of this instance.
		/// </summary>
		/// <returns>The friendly name of the domain.</returns>
		public override string ToString()
		{
			return this.mDnsDomainName;
		}
		#endregion
	}
}
