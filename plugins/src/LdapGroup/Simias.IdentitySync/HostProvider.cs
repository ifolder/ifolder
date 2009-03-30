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
*                 $Author: Mahabaleshwar Asundi <amahabaleshwar@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Security.Cryptography;
using System.Threading;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Authentication;
using Simias.Client.Event;
using Simias.Identity;
//using Simias.Provision;

namespace Simias.Host
{
	/// <summary>
	/// HostInfo class
	/// </summary>
	public class HostInfo
	{
		/// <summary>
		/// Host's unique ID
		/// </summary>
		public string ID;

		/// <summary>
		/// Host's name
		/// </summary>
		public string Name;

		/// <summary>
		/// Host's user/member ID which is consistent
		/// across all collections the host is 
		/// a member of.
		/// </summary>
		public string MemberID;

		/// <summary>
		/// External facing address for clients
		/// </summary>
		public string PublicAddress;

		/// <summary>
		/// Internal facing address for server to 
		/// server communication.
		/// </summary>
		public string PrivateAddress;

		/// <summary>
		/// Public key for host to host authentication
		/// </summary>
		public string PublicKey;

		/// <summary>
		/// true = Master, false = Slave
		/// </summary>
		public bool Master;

		/// <summary>
		/// HostInfo default constructor
		/// </summary>
		public HostInfo()
		{
		}

		internal HostInfo( HostNode node )
		{
			ID = node.ID;
			MemberID = node.UserID;
			Name = node.Name;
			PublicAddress = node.PublicUrl;
			PrivateAddress = node.PrivateUrl;
			PublicKey = node.PublicKey.ToXmlString( false );
			Master = node.IsMasterHost;
		}
	}

	/// <summary>
    /// slave setup
	/// </summary>
	public class SlaveSetup
	{
		private static string tempHostFileName = ".host.xml";
		private static string tempPPKFileName = ".hostppk.xml";
		private static string tempDomainFileName = ".domain.xml";
		private static string tempOwnerFileName = ".owner.xml";

        /// <summary>
        /// Saves the XML document
        /// </summary>
        /// <param name="storePath">path where to store</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="sObject">object to be written</param>
		private static void SaveXmlDoc( string storePath, string fileName, string sObject )
		{
			string fullPath = Path.Combine( storePath, fileName );
			StreamWriter stream = new StreamWriter( File.Open( fullPath, FileMode.Create, FileAccess.Write, FileShare.None ) );
			stream.Write( sObject );
			stream.Close();
		}

        /// <summary>
        /// Get the xml document from the given path 
        /// </summary>
        /// <param name="storePath">store path </param>
        /// <param name="fileName">Name of the file to be read</param>
        /// <returns>string object contating the full file</returns>
		private static string GetXmlDoc( string storePath, string fileName )
		{
			string fullPath = Path.Combine( storePath, fileName );
			StreamReader stream = new StreamReader( File.Open( fullPath, FileMode.Open, FileAccess.Read, FileShare.None ) );
			string objectXml = stream.ReadToEnd();
			stream.Close();
			return objectXml;
		}
	
        /// <summary>
        /// Get the domain
        /// </summary>
        /// <param name="storePath">store path</param>
        /// <returns>domain object</returns>
		internal static Domain GetDomain( string storePath )
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml( GetXmlDoc( storePath, tempDomainFileName ) );
			Domain domain = (Domain)Node.NodeFactory( Store.GetStore(), doc );
			domain.Proxy = true;
			return domain;
		}

        /// <summary>
        /// stores the domain object into tempdomainFileName
        /// </summary>
        /// <param name="storePath">store path</param>
        /// <param name="domain">domain object to be stored</param>
		private static void SaveDomain( string storePath, string domain )
		{
			SaveXmlDoc( storePath, tempDomainFileName, domain );
		}

        /// <summary>
        /// Gets the Owner for this store
        /// </summary>
        /// <param name="storePath">store path</param>
        /// <returns>member object for the owner</returns>
		internal static Member GetOwner( string storePath )
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml( GetXmlDoc( storePath, tempOwnerFileName ) );
			Member owner = (Member)Node.NodeFactory( Store.GetStore(), doc );
			owner.Proxy = true;
			return owner;
		}

        /// <summary>
        /// saves owber name into tempOwnerFileName
        /// </summary>
        /// <param name="storePath">store path</param>
        /// <param name="owner">string </param>
		private static void SaveOwner( string storePath, string owner )
		{
			SaveXmlDoc( storePath, tempOwnerFileName, owner );
		}

        /// <summary>
        /// Get the host from given store and tempHostFileName
        /// </summary>
        /// <param name="storePath">store path</param>
        /// <returns>HostNode</returns>
		internal static HostNode GetHost( string storePath )
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml( GetXmlDoc( storePath, tempHostFileName ) );
			HostNode hnode = new HostNode( Node.NodeFactory( Store.GetStore(), doc ) );
			return hnode;
		}

        /// <summary>
        /// saves host name into tempHostFileName
        /// </summary>
        /// <param name="storePath">store path</param>
        /// <param name="host">string object</param>
		private static void SaveHost( string storePath, string host )
		{
			SaveXmlDoc( storePath, tempHostFileName, host );
		}

        /// <summary>
        /// Get the PPK Key
        /// </summary>
        /// <param name="storePath">store path</param>
        /// <returns>keys</returns>
		internal static RSACryptoServiceProvider GetKeys( string storePath )
		{
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
			string keys = GetXmlDoc( storePath, tempPPKFileName );
			rsa.FromXmlString( keys );
			return rsa;
		}

		/// <summary>
        /// creates the keys
		/// </summary>
		public static RSACryptoServiceProvider CreateKeys( string storePath )
		{
			try
			{
				RSACryptoServiceProvider rsa = GetKeys( storePath );
				return rsa;
			}
			catch
			{
				RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
				return rsa;
			}
		}

		/// <summary>
        /// saves keys into PPK filename
		/// </summary>
		public static void SaveInitObjects( string storePath, string domain, string owner, string host, RSACryptoServiceProvider rsa )
		{
			SaveDomain( storePath, domain );
			SaveOwner( storePath, owner );
			SaveHost( storePath, host );
			SaveXmlDoc( storePath, tempPPKFileName, rsa.ToXmlString( true ) );
		}
		
		/// <summary>
		/// Delets all temp filename such as tempHostFileName etc..
		/// </summary>
		/// <param name="storePath"></param>
		internal static void DeleteTempSetupFiles( string storePath )
		{
			File.Delete( Path.Combine( storePath, tempDomainFileName ) );
			File.Delete( Path.Combine( storePath, tempDomainFileName ) );
			File.Delete( Path.Combine( storePath, tempHostFileName ) );
			File.Delete( Path.Combine( storePath, tempPPKFileName ) );
		}
	}

	/// <summary>
	/// Summary description for HostDomainProvider.
	/// </summary>
	public class HostProvider
	{
		private Domain hostDomain;
		private HostNode host;
		private Store store = Store.GetStore();
		
		private static string ServerSection = "Server";
		private static string ServerNameKey = "Name";
		private static string PublicAddressKey = "PublicAddress";
		private static string PrivateAddressKey = "PrivateAddress";
		private static string MasterAddressKey = "MasterAddress";
		/// <summary>
		/// The ID of the Host domain. used for Host to Host authentication.
		/// </summary>
		private CollectionSyncClient syncClient;
		private AutoResetEvent syncEvent = new AutoResetEvent(false);
		private SimiasConnection connection;
		
		/// <summary>
		/// Construct a Host domain.
		/// </summary>
		/// <param name="domain">The enterprise domain.</param>
		public HostProvider( Domain domain )
		{
			hostDomain = domain;

			// Check if this is the master server.
			bool master = ( hostDomain.Role == SyncRoles.Master ) ? true : false;
			
			// Get the HostDomain
			// If the HostNode does not exist create it.
			lock( typeof( HostProvider ) )
			{
				// Check if the host node exists.
				string hName = Store.Config.Get( ServerSection, ServerNameKey );

				// Make sure a master host can run without any pre-configured settings
				// so if the public address wasn't configured get a non-loopback local
				// address and configure the public address with it.
				string publicAddress = Store.Config.Get( ServerSection, PublicAddressKey );
				if ( publicAddress == null || publicAddress == String.Empty )
				{
					// Get the first non-localhost address
					string[] addresses = MyDns.GetHostAddresses();
					foreach( string addr in addresses )
					{
						if ( IPAddress.IsLoopback( IPAddress.Parse( addr ) ) == false )
						{
							publicAddress = addr;
							break;
						}
					}
				}

				string privateAddress = Store.Config.Get( ServerSection, PrivateAddressKey );
				if ( privateAddress == null || privateAddress == String.Empty )
				{
					if ( publicAddress != null )
					{
						privateAddress = publicAddress;
					}
				}

				string masterAddress = Store.Config.Get( ServerSection, MasterAddressKey );
				Member mNode = hostDomain.GetMemberByName( hName );
				host = ( mNode == null ) ? null : new HostNode( mNode );
				if ( host == null )
				{
					RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
					if( master == true )
					{
						host = new HostNode( hName, System.Guid.NewGuid().ToString(), publicAddress, privateAddress, rsa );
						host.Rights = Simias.Storage.Access.Rights.Admin;
						host.IsMasterHost = true;
					}
					else
					{
						host = SlaveSetup.GetHost( Store.StorePath );
						host.Proxy = true;
						rsa = SlaveSetup.GetKeys( Store.StorePath );
						// TODO remove
						Property p = new Property( PropertyTags.HostAddress, new Uri( masterAddress ) );
						p.LocalProperty = true;
						hostDomain.Properties.AddNodeProperty( p );
						// END TODO
					}

					host.IsLocalHost = true;
					hostDomain.Commit( new Node[] { hostDomain, host } );
					
					// Now Associate this host with the local identity.
					store.AddDomainIdentity( hostDomain.ID, host.UserID, rsa.ToXmlString(true), CredentialType.PPK );
					SlaveSetup.DeleteTempSetupFiles( Store.StorePath );
				}
				else
				{
					// Make sure the address has not changed.
					bool hostChanged = false;
					if (host.PublicUrl != publicAddress)
					{
						host.PublicUrl = publicAddress;
						hostChanged = true;
					}
					if (host.PrivateUrl != privateAddress)
					{
						host.PrivateUrl = privateAddress;
						hostChanged = true;
					}

					if ( hostChanged == true )
					{
						hostDomain.Commit(host);
					}
				}
			}

			if ( master == true )
			{
				// Register the ProvisionUser Provider.
				//ProvisionService.RegisterProvider( new LoadBalanceProvisionUserProvider() );
				//ProvisionService.RegisterProvider( new MasterHostProvisionProvider() );
			}
			else
			{
				// Now start the sync process for the domain.
				Thread syncThread = new Thread( new ThreadStart( SyncDomain ) );
				syncThread.IsBackground = true;
				syncThread.Start();
			}
		}

        /// <summary>
        /// Puts the domain into sync queue for sync
        /// </summary>
		private void SyncDomain()
		{
			int retry = 10;
			while( true )
			{
				try
				{
					// Get a connection object to the server.
					connection = new SimiasConnection( hostDomain.ID, host.UserID, SimiasConnection.AuthType.PPK, hostDomain );

					// We need to get a one time password to use to authenticate.
					connection.Authenticate();
					break;
				}
				catch
				{
					Thread.Sleep( 10000 );
					if ( retry <= 0 )
					{
						break;
					}
				}
			}
			
			syncClient = new CollectionSyncClient( hostDomain.ID, new TimerCallback( TimerFired ) );
			while ( true )
			{
				syncEvent.WaitOne();
				try
				{
					syncClient.SyncNow();
				}
				catch {}
				syncClient.Reschedule( true, 30 );
			}
		}

		/// <summary>
		/// Called by The CollectionSyncClient when it is time to run another sync pass.
		/// </summary>
		/// <param name="collectionClient">The client that is ready to sync.</param>
		public void TimerFired( object collectionClient )
		{
			while(CollectionSyncClient.running)
				Thread.Sleep(1000);
			syncEvent.Set();
		}
	}
}
