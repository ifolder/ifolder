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
using Simias.Storage;

namespace Simias.mDns
{
	/// <summary>
	/// Class for registering or publishing an iFolder Bonjour user
	/// </summary>
	public class Register
	{
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
		//private readonly string configWebService = "WebServiceUri";
        private readonly string virtualPath = "/simias10";
		private Uri webServiceUri = null;

		private string userID;
		private string memberName;
		private bool registered = false;
		private bool disposing = false;
		private Thread registerThread = null;
		private static IntPtr userHandle;
		private AutoResetEvent downEvent = null;

		#region DllImports
		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		Types.kErrorType
		RegisterLocalMember(
			string		ID,
			string		Name,
			short		Port,
			string		ServicePath,
			string		PublicKey,
			ref IntPtr	Cookie);

		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		Types.kErrorType
		RegisterLocalMemberCallback(
			RegistrationCallback	callback,
			string				id,
			string				name,
			short				port,
			string				servicePath,
			string				publicKey,
			ref IntPtr			cookie);

		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		Types.kErrorType
		DeregisterLocalMember(string ID, int Cookie);
		#endregion

		#region Properties
		/// <summary>
		/// Gets/Sets the published user's GUID
		/// </summary>
		public string ID
		{
			get { return ( userID != null ) ? userID : ""; }
			set { userID = value; }
		}

		/// <summary>
		/// Gets/Sets the published user's member name
		/// </summary>
		public string Name
		{
			get { return ( memberName != null ) ? memberName : ""; }
			set { memberName = value; }
		}

		/// <summary>
		/// Returns true if the member is registered with Bonjour
		/// false - not registered
		/// </summary>
		public bool Registered
		{
			get { return( this.registered ); }
		}
		#endregion
		
		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public Register()
		{
			userHandle = new IntPtr( 0 );
		}

		/// <summary>
		/// Constructor with all necessary properties to register
		/// the user with Bonjour
		/// </summary>
		public Register( string id, string memberName )
		{
			userHandle = new IntPtr( 0 );

			this.userID = id;
			this.memberName = memberName;
		}
		#endregion

		#region Private Methods
		private void RegisterThread()
		{
			Types.kErrorType status = Types.kErrorType.kDNSServiceErr_Unknown;

			if ( registered == true )
			{
				log.Debug( "User already registered.  Calling Unregister first" );
				DeregisterLocalMember( userID, userHandle.ToInt32() );
			}

			//
			// Register the user and as an iFolder member with
			// the mDnsResponder
			//
			try
			{
				RSACryptoServiceProvider publicKey = Store.GetStore().CurrentUser.PublicKey;
				short sport = (short) webServiceUri.Port;

				//RegistrationCallback myCallback = new RegistrationCallback( RegisterCallback );

				do
				{
					log.Debug( "RegisterLocalMember" );
					log.Debug( "  UserID:	" + userID );
					log.Debug( "  Username: " + memberName );
					log.Debug( "  ServicePath: " + webServiceUri.AbsolutePath );
					log.Debug( "  Public Key:  " + publicKey.ToXmlString( false ) );
					//log.Debug( "  calling RegisterWithCallback" );
					status = 
						RegisterLocalMember( 
							userID, 
							memberName,
							IPAddress.HostToNetworkOrder( sport ),
							webServiceUri.AbsolutePath,
							publicKey.ToXmlString( false ),
							ref userHandle );

					if ( status != Types.kErrorType.kDNSServiceErr_NoError )
					{
						log.Debug( "RegisterLocalMember failed  Status: " + status.ToString() );
						downEvent.WaitOne( 30000, false );
						if ( disposing == true )
						{
							break;
						}
					}
				} while ( status != Types.kErrorType.kDNSServiceErr_NoError );

				log.Debug( "RegisterLocalMember: " + status.ToString() );
			}
			catch( Exception e2 )
			{
				log.Error( e2.Message );
				log.Error( e2.StackTrace );
			}	
		
			if ( status == Types.kErrorType.kDNSServiceErr_NoError )
			{
				registered = true;
			}
			log.Debug( "Register thread exit" );
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
		public void RegisterUser()
		{
			log.Debug( "RegisterUser called" );

            string[] myAddresses = null;

			// for register, unregister, re-register case
			disposing = false;

			// well-formed instance?
			if ( this.userID == null || this.userID == "" ||
				this.memberName == null || this.memberName == "" )
			{
				ApplicationException ae = new ApplicationException( "Missing necessary properties for registering" );
				throw ae;
			}

			if ( registered == true )
			{
				ApplicationException ae = new ApplicationException( memberName + " already registered" );
				throw ae;
			}

            myAddresses = Simias.MyDns.GetHostAddresses();
            //if ( myAddresses.Count == 0 )
            //{
            //    throw new ApplicationException( "Could not determine local address" );
            //}

            string fullPath = "http://" + myAddresses[0] + ":" + Store.LocalServicePort.ToString() + this.virtualPath;
            log.Debug( "Path: " + fullPath );
            webServiceUri = new Uri( fullPath );

            /*
			// Grab the web service uri simias is listening on
			//if ( webServiceUri != null )
			//{
				XmlElement servicesElement = 
					Store.GetStore().Config.GetElement( configSection, configWebService );
				webServiceUri = new Uri( "http://servicesElement.GetAttribute( "value" ) );
				if ( webServiceUri == null )
				{
					ApplicationException ae = new ApplicationException( "Could not obtain the local web service Uri" );
					throw ae;
				}
			//}
            */

			registerThread = new Thread( new ThreadStart( RegisterThread ) );
			registerThread.IsBackground = true;
			registerThread.Start();
			return;
		}

		public void UnregisterUser()
		{
			disposing = true;
			downEvent.Set();

			if ( registered == true )
			{
				DeregisterLocalMember( userID, userHandle.ToInt32() );
				registered = false;
			}
		}

		public 
		delegate 
		bool 
		RegistrationCallback(
			int			     handle,
			int			     flags,
			uint			 ifIndex,
			Types.kErrorType errorCode,
			[MarshalAs(UnmanagedType.I4)] int userHandle);
		#endregion
	}
}
