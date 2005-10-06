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
	// Quick and dirty - I need to clean this up and
	// put some thought into it
	public class BonjourUsers
	{
		static public string StagedName = "StagedMember";
		static public string HostProperty = "RHost";
		static public string PathProperty = "RPath";
		static public string PortProperty = "RPort";
		static public string KeyProperty = "RPublicKey";

		static internal ArrayList memberList = new ArrayList();
		static internal ArrayList stagedList = new ArrayList();

		//static internal	UserLock memberListLock = new UserLock();

		public BonjourUsers()
		{
		}

		static public bool LoadMembersFromDomain( bool verifyUniqueness )
		{
			lock( typeof( Simias.mDns.BonjourUsers ) )
			{
				// Add the members in the store
				Simias.Storage.Domain domain = Store.GetStore().GetDomain( Simias.mDns.Domain.ID );
				ICSList memberlist = domain.GetMemberList();
				foreach(ShallowNode sNode in memberlist)
				{
					// Get the member from the list
					Simias.Storage.Member member =
						new Simias.Storage.Member( domain, sNode);

					if ( verifyUniqueness == true )
					{
						// FIXME::
					}

					Property p = new Property( "InStore", true );
					member.Properties.AddProperty( p );
					BonjourUsers.memberList.Add( member );
				}
			}

			return true;
		}

		static public bool AddMember( Member nMember, bool verifyUniqueness )
		{
			bool duplicate = false;
			lock( typeof( Simias.mDns.BonjourUsers ) )
			{
				if ( verifyUniqueness == true )
				{
					foreach( Member cMember in BonjourUsers.memberList )
					{
						if ( cMember.UserID == nMember.UserID )
						{
							duplicate = true;
							break;
						}
					}
				}

				if ( duplicate == false )
				{
					BonjourUsers.memberList.Add( nMember );
				}
			}

			if ( duplicate == true )
			{
				return false;
			}
			return true;
		}

		static public bool AddStagedMember( Member nMember )
		{
			lock( typeof( Simias.mDns.BonjourUsers ) )
			{
				foreach( Member cMember in BonjourUsers.stagedList )
				{
					if ( cMember.UserID == nMember.UserID )
					{
						return false;
					}
				}

				BonjourUsers.stagedList.Add( nMember );
			}

			return true;
		}

		static public Member GetMember( string memberID )
		{
			Member cMember = null;

			foreach( Member rMember in BonjourUsers.memberList )
			{
				if ( rMember.UserID == memberID )
				{
					cMember = rMember;
					break;
				}
			}

			return cMember;
		}

		static public string GetMembersPublicKey( string memberID )
		{
			string publicKey = null;
			foreach( Member rMember in BonjourUsers.memberList )
			{
				if ( rMember.UserID == memberID )
				{
					Property p = rMember.Properties.GetSingleProperty( BonjourUsers.KeyProperty );
					if ( p != null )
					{
						publicKey = p.Value.ToString();
					}
					break;
				}
			}

			return publicKey;
		}

		static public bool UpdateMember( Member member )
		{
			foreach( Member cMember in BonjourUsers.memberList )
			{
				if ( cMember.UserID == member.UserID )
				{
					// Out with the old in with the new
					BonjourUsers.memberList.Remove( cMember );
					BonjourUsers.memberList.Add( member );
					return true;
				}
			}

			return false;
		}

		static public bool RemoveMember( string memberID )
		{
			lock( typeof( Simias.mDns.BonjourUsers ) )
			{
				foreach( Member cMember in BonjourUsers.memberList )
				{
					if ( cMember.UserID == memberID )
					{
						BonjourUsers.memberList.Remove( cMember );
						return true;
					}
				}

			}

			return false;
		}

		static public bool RemoveStagedMember( string memberID )
		{
			lock( typeof( Simias.mDns.BonjourUsers ) )
			{
				foreach( Member cMember in BonjourUsers.stagedList )
				{
					if ( memberID == cMember.UserID )
					{
						BonjourUsers.stagedList.Remove( cMember );
						return true;
					}
				}

			}

			return false;
		}
	}


	/// <summary>
	/// Class used to broadcast the current user
	/// to the Rendezvous/mDns network
	/// </summary>
	public class User
	{
#if DARWIN
		private const string nativeLib = "libsimiasbonjour.dylib";
#else
		private const string nativeLib = "libsimiasbonjour";
#endif

		[ StructLayout( LayoutKind.Sequential, CharSet=CharSet.Ansi ) ]
		public class MemberInfo
		{
			[ MarshalAs( UnmanagedType.ByValTStr, SizeConst=128 ) ]
			public String Name = null;

			[ MarshalAs( UnmanagedType.ByValTStr, SizeConst=128 ) ]
			public String Host = null;

			[ MarshalAs( UnmanagedType.ByValTStr, SizeConst=128 ) ]
			public String ServicePath = null;

			[ MarshalAs( UnmanagedType.ByValTStr, SizeConst=256 ) ]
			public String PublicKey = null;

			public int Port;
		}

		#region DllImports
		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		User.kErrorType
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
		User.kErrorType
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
		User.kErrorType
		DeregisterLocalMember(string ID, int Cookie);

		[ DllImport( nativeLib, CharSet=CharSet.Auto ) ]
		private 
		extern 
		static 
		User.kErrorType
		GetMemberInfo(
			[MarshalAs(UnmanagedType.LPStr)] string	ID,
			[In, Out] MemberInfo Info);

		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		User.kErrorType
		BrowseMembersInit( MemberBrowseCallback	callback, ref IntPtr handle );
		
		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		User.kErrorType
		BrowseMembersShutdown( int handle );

		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		User.kErrorType
		BrowseMembers( int handle, int timeout );
		#endregion

		#region Class Members
		private static bool registered = false;
		private static string  mDnsUserName;
		private static string  mDnsUserID = "";
		private static readonly string configSection = "ServiceManager";
		private static readonly string configServices = "WebServiceUri";
		private static Uri webServiceUri = null;
		private static IntPtr userHandle;
		private static IntPtr browseHandle;
		private static Thread browseThread = null;
		private static Thread registerThread = null;

		// State for maintaining the Rendezvous user list

		/*
		// TEMP need to fix protection level here
		internal static UserLock	memberListLock;
		internal static ArrayList	memberList;
		*/

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// possible error code values 
		public enum kErrorType : int
		{
			kDNSServiceErr_NoError             = 0,
			kDNSServiceErr_Unknown             = -65537,       /* 0xFFFE FFFF */
			kDNSServiceErr_NoSuchName          = -65538,
			kDNSServiceErr_NoMemory            = -65539,
			kDNSServiceErr_BadParam            = -65540,
			kDNSServiceErr_BadReference        = -65541,
			kDNSServiceErr_BadState            = -65542,
			kDNSServiceErr_BadFlags            = -65543,
			kDNSServiceErr_Unsupported         = -65544,
			kDNSServiceErr_NotInitialized      = -65545,
			kDNSServiceErr_AlreadyRegistered   = -65547,
			kDNSServiceErr_NameConflict        = -65548,
			kDNSServiceErr_Invalid             = -65549,
			kDNSServiceErr_Firewall            = -65550,
			kDNSServiceErr_Incompatible        = -65551,        /* client library incompatible with daemon */
			kDNSServiceErr_BadInterfaceIndex   = -65552,
			kDNSServiceErr_Refused             = -65553,
			kDNSServiceErr_NoSuchRecord        = -65554,
			kDNSServiceErr_NoAuth              = -65555,
			kDNSServiceErr_NoSuchKey           = -65556,
			kDNSServiceErr_NATTraversal        = -65557,
			kDNSServiceErr_DoubleNAT           = -65558,
			kDNSServiceErr_BadTime             = -65559
			/* mDNS Error codes are in the range
				 * FFFE FF00 (-65792) to FFFE FFFF (-65537) */
		};

		/* General flags used in functions defined below */
		public enum kDNSServiceFlags : int
		{
			kDNSServiceFlagsMoreComing          = 0x1,
			/* MoreComing indicates to a callback that at least one more result is
			 * queued and will be delivered following immediately after this one.
			 * Applications should not update their UI to display browse
			 * results when the MoreComing flag is set, because this would
			 * result in a great deal of ugly flickering on the screen.
			 * Applications should instead wait until until MoreComing is not set,
			 * and then update their UI.
			 * When MoreComing is not set, that doesn't mean there will be no more
			 * answers EVER, just that there are no more answers immediately
			 * available right now at this instant. If more answers become available
			 * in the future they will be delivered as usual.
			 */

			kDNSServiceFlagsAdd                 = 0x2,
			kDNSServiceFlagsDefault             = 0x4,
			/* Flags for domain enumeration and browse/query reply callbacks.
			 * "Default" applies only to enumeration and is only valid in
			 * conjuction with "Add".  An enumeration callback with the "Add"
			 * flag NOT set indicates a "Remove", i.e. the domain is no longer
			 * valid.
			 */

			kDNSServiceFlagsNoAutoRename        = 0x8,
			/* Flag for specifying renaming behavior on name conflict when registering
			 * non-shared records. By default, name conflicts are automatically handled
			 * by renaming the service.  NoAutoRename overrides this behavior - with this
			 * flag set, name conflicts will result in a callback.  The NoAutorename flag
			 * is only valid if a name is explicitly specified when registering a service
			 * (i.e. the default name is not used.)
			 */

			kDNSServiceFlagsShared              = 0x10,
			kDNSServiceFlagsUnique              = 0x20,
			/* Flag for registering individual records on a connected
			 * DNSServiceRef.  Shared indicates that there may be multiple records
			 * with this name on the network (e.g. PTR records).  Unique indicates that the
			 * record's name is to be unique on the network (e.g. SRV records).
			 */

			kDNSServiceFlagsBrowseDomains       = 0x40,
			kDNSServiceFlagsRegistrationDomains = 0x80,
			/* Flags for specifying domain enumeration type in DNSServiceEnumerateDomains.
			 * BrowseDomains enumerates domains recommended for browsing, RegistrationDomains
			 * enumerates domains recommended for registration.
			 */

			kDNSServiceFlagsLongLivedQuery      = 0x100,
			/* Flag for creating a long-lived unicast query for the DNSServiceQueryRecord call. */

			kDNSServiceFlagsAllowRemoteQuery    = 0x200,
			/* Flag for creating a record for which we will answer remote queries
			 * (queries from hosts more than one hop away; hosts not directly connected to the local link).
			 */

			kDNSServiceFlagsForceMulticast      = 0x400
			/* Flag for signifying that a query or registration should be performed exclusively via multicast DNS,
			 * even for a name in a domain (e.g. foo.apple.com.) that would normally imply unicast DNS.
			 */
		};

		#endregion

		#region Properties
		/// <summary>
		/// Gets the current user's mDns ID
		/// </summary>
		public string ID
		{
			get { return( User.mDnsUserID ); }
		}

		/// <summary>
		/// Gets the mDnsDomain's friendly name
		/// </summary>
		public string Name
		{
			get { return( User.mDnsUserName ); }
		}
		#endregion

		#region Constructors

		/// <summary>
		/// Static constructor for mDns
		/// </summary>
		static User()
		{
			// Get the configured/generated web service path and store
			// it away.  The port and path are broadcast in Rendezvous

			try
			{
				XmlElement servicesElement = 
					Store.GetStore().Config.GetElement( configSection, configServices );
				webServiceUri = new Uri( servicesElement.GetAttribute( "value" ) );
				if ( webServiceUri != null )
				{
					log.Debug( "Web Service URI: " + webServiceUri.ToString() );
					log.Debug( "Absolute Path: " + webServiceUri.AbsolutePath );
				}
			}
			catch( Exception ws )
			{
				log.Error( "Exception getting the \"webServiceUr\"" );
				log.Error( ws.Message );
				log.Error( ws.StackTrace );
			}

			/*
			User.memberListLock = new UserLock();
			User.memberList = new ArrayList();
			*/

			userHandle = new IntPtr( 0 );

			try
			{

				Simias.Storage.Domain rDomain = 
					Store.GetStore().GetDomain( Simias.mDns.Domain.ID );
				if ( rDomain == null )
				{
					// Instantiating the mDns.Domain should create the Bonjour domain
					Simias.mDns.Domain mdnsDomain = new Simias.mDns.Domain();
					rDomain = Store.GetStore().GetDomain( Simias.mDns.Domain.ID );
				}

				if ( rDomain != null )
				{
					User.mDnsUserID = rDomain.Owner.UserID;
					User.mDnsUserName = rDomain.Owner.Name;
				}
				else
				{
					throw new ApplicationException( "Bonjour Domain does not exist" );
				}
			}
			catch( Exception e )
			{
				log.Debug( e.Message );
				log.Debug( e.StackTrace );
				throw e;
			}
		}

		/// <summary>
		/// Constructor for newing up an mDns user object.
		/// </summary>
		internal User()
		{
		}

		#endregion

		#region Internal Methods
		internal static void RegisterUser()
		{
			log.Debug( "RegisterUser called" );

			User.registerThread = new Thread( new ThreadStart( User.RegisterThread ) );
			User.registerThread.IsBackground = true;
			User.registerThread.Start();

		}

		internal static void RegisterThread()
		{
			User.kErrorType status;

			if ( registered == true )
			{
				log.Debug( "User already registered.  Calling Unregister first" );
				User.UnregisterUser();
			}

			if ( webServiceUri == null )
			{
				//throw new SimiasException( "Web Service URI not configured" );
				throw new ApplicationException( "Web Service URI not configured" );
			}

			//
			// Register the user and as an iFolder member with
			// the mDnsResponder
			//
			try
			{
				RSACryptoServiceProvider publicKey = Store.GetStore().CurrentUser.PublicKey;
				short sport = (short) webServiceUri.Port;

				RegistrationCallback myCallback = new RegistrationCallback( RegisterCallback );

				do
				{
					log.Debug( "RegisterLocalMember" );
					log.Debug( "  UserID:	" + User.mDnsUserID );
					log.Debug( "  Username: " + User.mDnsUserName );
					log.Debug( "  ServicePath: " + webServiceUri.AbsolutePath );
					log.Debug( "  Public Key:  " + publicKey.ToXmlString( false ) );
					//log.Debug( "  calling RegisterWithCallback" );
					status = 
						RegisterLocalMember( 
							User.mDnsUserID, 
							User.mDnsUserName,
							IPAddress.HostToNetworkOrder( sport ),
							webServiceUri.AbsolutePath,
							publicKey.ToXmlString( false ),
							ref userHandle );

					if ( status != User.kErrorType.kDNSServiceErr_NoError )
					{
						log.Debug( "RegisterLocalMember failed  Status: " + status.ToString() );
						Thread.Sleep( 60000 );
					}
				} while ( status != User.kErrorType.kDNSServiceErr_NoError );


				/*
				kErrorType status =
					RegisterLocalMember( 
						User.mDnsUserID, 
						User.mDnsUserName,
						IPAddress.HostToNetworkOrder( sport ),
						webServiceUri.AbsolutePath,
						publicKey.ToXmlString( false ),
						ref userHandle );

				if ( status != kErrorType.kDNSServiceErr_NoError )
				{
					throw new SimiasException( "Failed to register local member with Bonjour" );
				}
				*/

				log.Debug( "RegisterLocalMember: " + status.ToString() );
			}
			catch( Exception e2 )
			{
				log.Error( e2.Message );
				log.Error( e2.StackTrace );
			}	
		
			log.Debug( "RegisterUser exit" );
		}

		internal static void UnregisterUser()
		{
			if ( registered == true )
			{
				DeregisterLocalMember( User.mDnsUserID, userHandle.ToInt32() );
				registered = false;
			}
		}

		internal
		static 
		bool 
		RegisterCallback( 
			int			handle,
			int			flags,
			uint		ifIndex,
			kErrorType	errorCode,
			[MarshalAs(UnmanagedType.I4)] int userHandle)
		{ 
			if ( errorCode == kErrorType.kDNSServiceErr_NoError )
			{
				log.Debug( "User handle: " + userHandle.ToString() );
			}
			else
			{
				log.Debug( 
					"Received an error on RegisterCallback.  status: " + errorCode.ToString() );
			}
			return true;
		}


		internal static void StartMemberBrowsing()
		{
			User.browseHandle = new IntPtr( 0 );
			User.browseThread = new Thread( new ThreadStart( User.BrowseThread ) );
			User.browseThread.IsBackground = true;
			User.browseThread.Start();
		}

		internal static void StopMemberBrowsing()
		{
			if ( browseHandle.ToInt32() != 0 )
			{
				BrowseMembersShutdown( browseHandle.ToInt32() );
				Thread.Sleep( 100 );
				if (User.browseThread.IsAlive == true )
				{
					// Shutdown the thread
					//User.browseThread.Interrupt();
				}
			}
		}

		internal static void BrowseThread()
		{
			User.kErrorType status;
			User.browseHandle = new IntPtr( 0 );
			MemberBrowseCallback myCallback = new MemberBrowseCallback( MemberCallback );

			do
			{
				log.Debug( "  calling BrowseMembersInit" );
				status = BrowseMembersInit( myCallback, ref User.browseHandle );
				if ( status == User.kErrorType.kDNSServiceErr_NoError )
				{
					// A timeout is returning success so we're OK
					log.Debug( "  calling BrowseMembers" );
					status = BrowseMembers( User.browseHandle.ToInt32(), 30 );
					log.Debug( "  return from BrowseMembers - status: " + status.ToString() );
				}
			} while ( status == User.kErrorType.kDNSServiceErr_NoError );

			log.Debug( "BrowseThread down..." );
			log.Debug( "Status: " + status.ToString() );
		}

		internal
		static 
		bool 
		MemberCallback( 
			int			handle,
			int			flags,
			uint		ifIndex,
			kErrorType	errorCode,
			[MarshalAs(UnmanagedType.LPStr)] string serviceName,
			[MarshalAs(UnmanagedType.LPStr)] string regType,
			[MarshalAs(UnmanagedType.LPStr)] string domain,
			[MarshalAs(UnmanagedType.I4)] int context)
		{ 
			if ( errorCode == kErrorType.kDNSServiceErr_NoError )
			{
				log.Debug( "MemberCallback for: " + serviceName );
				if ( ( flags & (int) kDNSServiceFlags.kDNSServiceFlagsAdd ) == 
					(int) kDNSServiceFlags.kDNSServiceFlagsAdd )
				{
					Member rMember = 
						new Member( 
							BonjourUsers.StagedName,
							serviceName,
							Simias.Storage.Access.Rights.ReadWrite );

					bool added = BonjourUsers.AddStagedMember( rMember );
					if ( added == true )
					{
						// Force a meta-data sync
						Simias.mDns.Sync.SyncNow("");
					}
				}
				else
				{
					log.Debug( "Removing member: " + serviceName );
					if ( BonjourUsers.RemoveStagedMember( serviceName ) == false )
					{
						BonjourUsers.RemoveMember( serviceName );
					}
				}
			}
			else
			{
				log.Debug( 
					"Received an error on MemberCallback.  status: " + errorCode.ToString() );
			}
			return true;
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Method to move members from the staged list to the real mdns member list
		/// </summary>
		/// <returns>n/a</returns>
		public void SynchronizeMembers()
		{
			// FIXME::define sizes
			//char[] trimNull = { '\0' };

			log.Debug( "Syncing Bonjour members" );
			Simias.Storage.Member rMember;
			lock( typeof( Simias.mDns.BonjourUsers ) )
			{
				User.kErrorType ccode;
				log.Debug( "Current Members" );
				foreach( Member cMember in BonjourUsers.memberList )
				{
					log.Debug( "  " + cMember.Name );
					MemberInfo info = new MemberInfo();
					ccode = GetMemberInfo( cMember.UserID, info );
					if ( ccode == kErrorType.kDNSServiceErr_NoError )
					{
						log.Debug( "  Friendly Name: " + info.Name );
						log.Debug( "  Service Path:  " + info.ServicePath );
						log.Debug( "  Host:          " + info.Host );
						log.Debug( "  Port:          " + info.Port.ToString() );
					}
				}

				IEnumerator memberEnum = BonjourUsers.stagedList.GetEnumerator();
				while( memberEnum.MoveNext() )
				{
					rMember = memberEnum.Current as Simias.Storage.Member;

					// Go get the rest of the meta-data for this user
					User.kErrorType status;

					try
					{
						log.Debug( "Calling GetMemberInfo for: " + rMember.UserID );

						MemberInfo info = new MemberInfo();
						status = GetMemberInfo( rMember.UserID, info );
						if ( status == kErrorType.kDNSServiceErr_NoError )
						{
							log.Debug( "  Friendly Name: " + info.Name );
							log.Debug( "  Service Path:  " + info.ServicePath );
							log.Debug( "  Host:          " + info.Host );
							log.Debug( "  Port:          " + info.Port.ToString() );
							
							rMember.Name = info.Name;
							rMember.FN = rMember.Name;

							Property host = 
								new Property( BonjourUsers.HostProperty, info.Host );
							host.LocalProperty = true;
							rMember.Properties.AddProperty( host );

							Property path = 
								new Property( 
								BonjourUsers.PathProperty,
								info.ServicePath );
							path.LocalProperty = true;
							rMember.Properties.AddProperty( path );

							Property rport = new Property( BonjourUsers.PortProperty, info.Port );
							rMember.Properties.AddProperty( rport );

							/*
							UTF8Encoding utf8 = new UTF8Encoding();
							string pubKey = utf8.GetString( infoPublicKey );
							*/

							Property key = 
								new Property( 
									BonjourUsers.KeyProperty, 
									info.PublicKey );

							rMember.Properties.AddProperty( key );

							log.Debug( "Adding meta-data for: " + rMember.Name );
							BonjourUsers.stagedList.Remove( rMember );
							if ( BonjourUsers.AddMember( rMember, true ) == false )
							{
								BonjourUsers.UpdateMember( rMember );
							}

							// The collection was changed so reset up a new enumerator
							memberEnum = BonjourUsers.stagedList.GetEnumerator();
						}
					}
					catch ( Exception e2 )
					{
						log.Error( e2.Message );
						log.Error( e2.StackTrace );
						break;
					}
				}
			}

			log.Debug( "Syncing mDns exit" );
		}

		public 
		delegate 
		bool 
		MemberBrowseCallback(
			int			handle,
			int			flags,
			uint		ifIndex,
			kErrorType	errorCode,
			[MarshalAs(UnmanagedType.LPStr)] string serviceName,
			[MarshalAs(UnmanagedType.LPStr)] string regType,
			[MarshalAs(UnmanagedType.LPStr)] string domain,
			[MarshalAs(UnmanagedType.I4)] int context);

		public 
		delegate 
		bool 
		RegistrationCallback(
			int			handle,
			int			flags,
			uint		ifIndex,
			kErrorType	errorCode,
			[MarshalAs(UnmanagedType.I4)] int userHandle);
	}


		#endregion
	}

