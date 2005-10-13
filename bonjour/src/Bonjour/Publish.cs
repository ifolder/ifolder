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
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Storage;

namespace Simias.mDns
{
	/// <summary>
	/// Class for registering all P2P Simias collections with the mdns network
	/// </summary>
	public class Publish
	{
        internal class PublishedCollection
        {
            public bool published;
            public string id;
			public IntPtr cookie;
        }

#if DARWIN
		private const string nativeLib = "libsimiasbonjour.dylib";
#else
		private const string nativeLib = "libsimiasbonjour";
#endif

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly string configSection = "ServiceManager";
		private readonly string configWebService = "WebServiceUri";

		private bool disposing = false;
		private bool publishing = false;
		private Thread publishThread = null;
		private AutoResetEvent downEvent = null;
		private ArrayList collections = null;
		private EventSubscriber subscriber = null;
		private Store store = null;

		#region DllImports
		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		Types.kErrorType
		RegisterCollection(
			string		ID,
			string		Name,
			string		Description,
			short		Port,
			string		ServicePath,
			ref IntPtr	Cookie);

		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		Types.kErrorType
		DeregisterCollection(string ID, int Cookie);
		#endregion

		#region Properties
		/// <summary>
		/// Returns if this instance is actively publishing
        /// P2P collections
		/// </summary>
		public bool Publishing
		{
			get { return publishing; }
		}
		#endregion
		
		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public Publish()
		{
			store = Store.GetStore();
			collections = new ArrayList();
		}
		#endregion

		#region Private Methods

		private 
		Types.kErrorType 
		RegisterP2PCollection( string ID, string Name, string Description)
		{
			log.Debug( "RegisterP2PCollection called" );
			log.Debug( "  " + Name );
			Types.kErrorType status = Types.kErrorType.kDNSServiceErr_NoError;

			XmlElement servicesElement = 
				Store.GetStore().Config.GetElement( configSection, configWebService );
			Uri webServiceUri = new Uri( servicesElement.GetAttribute( "value" ) );
			if ( webServiceUri != null )
			{
				try
				{
					foreach( PublishedCollection publishedCollection in this.collections )
					{
						if ( publishedCollection.id == ID )
						{
							log.Debug( "  already registered" );
							status = Types.kErrorType.kDNSServiceErr_AlreadyRegistered;
							break;
						}
					}
				}
				catch( Exception e )
				{
					log.Error( e.Message );
					log.Error( e.StackTrace );
				}

				if ( status == Types.kErrorType.kDNSServiceErr_NoError )
				{
					// register and add
					PublishedCollection pubCollection = new PublishedCollection();
					pubCollection.published = false;
					pubCollection.cookie = new IntPtr( 0 );

					status = 
						RegisterCollection( 
							ID, 
							Name, 
							Description, 
							IPAddress.HostToNetworkOrder( (short) webServiceUri.Port ),
							webServiceUri.AbsolutePath,
							ref pubCollection.cookie );
					if ( status == Types.kErrorType.kDNSServiceErr_NoError )
					{
						pubCollection.published = true;
						pubCollection.id = ID;

						lock( this.collections )
						{
							this.collections.Add( pubCollection );
						}
						log.Debug( "  successful registration" );
					}
				}
			}
			else
			{
				log.Debug( "  unable to get the local webservice URL" );
				status = Types.kErrorType.kDNSServiceErr_Unknown;
			}

			log.Debug( "RegisterP2PCollection exit" );
			return status;
		}

		private 
		Types.kErrorType 
		UnregisterP2PCollection( string ID )
		{
			log.Debug( "UnregisterP2PCollection called" );
			Types.kErrorType status = Types.kErrorType.kDNSServiceErr_NoSuchName;

			PublishedCollection removed = null;
			foreach( PublishedCollection publishedCollection in this.collections )
			{
				if ( publishedCollection.id == ID )
				{
					removed = publishedCollection;
					break;
				}
			}

			if ( removed != null )
			{
				lock ( this.collections )
				{
					this.collections.Remove( removed );
				}

				status = DeregisterCollection( ID, removed.cookie.ToInt32() );
			}

			log.Debug( "UnregisterP2PCollection exit - status: " + status.ToString() );
			return status;
		}

		private void PublishThread()
		{
			log.Debug( "PublishThread - starting..." );

			if ( publishing == true )
			{
				log.Debug( "Instance is already publishing" );
				return;
			}

			try
			{
				foreach( ShallowNode sn in store.GetCollectionsByDomain( Simias.mDns.Domain.ID ) )
				{
					Collection collection = new Collection( store, sn );

					if ( !collection.IsBaseType( collection, NodeTypes.DomainType ) &&
						 !collection.IsBaseType( collection, NodeTypes.POBoxType ) )
					{
						RegisterP2PCollection( collection.ID, collection.Name, "Simias published collection" );
					}
				}

				// Register with the store for collection creates and deletes
				this.subscriber = new EventSubscriber();
				this.subscriber.NodeTypeFilter = NodeTypes.CollectionType;
				this.subscriber.NodeCreated += new NodeEventHandler( OnCollectionCreated );
				this.subscriber.NodeDeleted += new NodeEventHandler( OnCollectionDeleted );
				this.subscriber.Enabled = true;

				this.publishing = true;
			}
			catch( Exception e2 )
			{
				log.Error( e2.Message );
				log.Error( e2.StackTrace );
			}	
		
			log.Debug( "PublishThread down..." );
		}

		private void OnCollectionCreated( NodeEventArgs args )
		{
			log.Debug( "OnCollectionCreated called - Node ID: " + args.ID );
			try
			{
				Collection collection = store.GetCollectionByID( args.ID );
				if ( collection != null )
				{
					log.Debug( "  collection: " + collection.Name );
					if ( collection.Role == Simias.Sync.SyncRoles.Master ) // &&
						 //collection.IsBaseType( collection, NodeTypes.FileNodeType ) == true )
					{
						RegisterP2PCollection( collection.ID, collection.Name, "Simias (2) published collection" );
					}
				}
				else
				{
					log.Debug( "  failed GetCollectionByID" );
				}
			}
			catch( Exception onCreated )
			{
				log.Error( onCreated.Message );
				log.Error( onCreated.StackTrace );
			}
		}

		private void OnCollectionDeleted( NodeEventArgs args )
		{
			log.Debug( "OnCollectionDeleted called - Node ID: " + args.ID );
			try
			{
				UnregisterP2PCollection( args.ID );
			}
			catch( Exception onDeleted )
			{
				log.Error( onDeleted.Message );
				log.Error( onDeleted.StackTrace );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Method to register the user with the mdns net
		/// If the object does not contain all the properties
		/// necessary to register an exception will the thrown.
		/// 
		/// Note: the registration is process is asynchronous
		/// operation so callers will need to poll the Registered
		/// property for knowledge of the successful registration.
		/// Fixme:: add a method with a callback status
		/// </summary>
		public void StartPublishing()
		{
			log.Debug( "StartPublishing called" );

			// for register, unregister, re-register case
			disposing = false;

			if ( publishing == true )
			{
				ApplicationException ae = new ApplicationException( "Already publishing" );
				throw ae;
			}

			publishThread = new Thread( new ThreadStart( PublishThread ) );
			publishThread.IsBackground = true;
			publishThread.Start();
			return;
		}

		public void StopPublishing()
		{
			disposing = true;
			downEvent.Set();

			if ( publishing == true )
			{
				lock ( this.collections )
				{
					// loop through the published collections and deregister them
					foreach( PublishedCollection publishedCollection in this.collections )
					{
						DeregisterCollection( publishedCollection.id, publishedCollection.cookie.ToInt32() );
					}
					this.collections.Clear();
				}
				publishing = false;
			}
		}
		#endregion
	}
}
