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

//using Mono.P2p.mDnsResponderApi;

namespace Simias.mDns
{
	// Quick and dirty - I need to clean this up and
	// put some thought into it
	public class RendezvousUsers
	{
		static public string StagedName = "StagedMember";
		static public string HostProperty = "RHost";
		static public string PathProperty = "RPath";
		static public string PortProperty = "RPort";
		static public string KeyProperty = "RPublicKey";

		static internal ArrayList memberList = new ArrayList();
		static internal ArrayList stagedList = new ArrayList();

		//static internal	UserLock memberListLock = new UserLock();

		public RendezvousUsers()
		{
		}

		static public bool LoadMembersFromDomain( bool verifyUniqueness )
		{
			lock( typeof( Simias.mDns.RendezvousUsers ) )
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
					RendezvousUsers.memberList.Add( member );
				}
			}

			return true;
		}

		static public bool AddMember( Member nMember, bool verifyUniqueness )
		{
			bool duplicate = false;
			//lock( typeof( Simias.mDns.RendezvousUsers ) )
			//{
				if ( verifyUniqueness == true )
				{
					foreach( Member cMember in RendezvousUsers.memberList )
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
					RendezvousUsers.memberList.Add( nMember );
				}
			//}

			if ( duplicate == true )
			{
				return false;
			}
			return true;
		}

		static public bool AddStagedMember( Member nMember )
		{
			//lock( typeof( Simias.mDns.RendezvousUsers ) )
			//{
				foreach( Member cMember in RendezvousUsers.stagedList )
				{
					if ( cMember.UserID == nMember.UserID )
					{
						return false;
					}
				}

				RendezvousUsers.stagedList.Add( nMember );
			//}

			return true;
		}

		static public Member GetMember( string memberID )
		{
			Member cMember = null;

			foreach( Member rMember in RendezvousUsers.memberList )
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
			foreach( Member rMember in RendezvousUsers.memberList )
			{
				if ( rMember.UserID == memberID )
				{
					Property p = rMember.Properties.GetSingleProperty( RendezvousUsers.KeyProperty );
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
			foreach( Member cMember in RendezvousUsers.memberList )
			{
				if ( cMember.UserID == member.UserID )
				{
					// Out with the old in with the new
					RendezvousUsers.memberList.Remove( cMember );
					RendezvousUsers.memberList.Add( member );
					return true;
				}
			}

			return false;
		}

		static public bool RemoveMember( string memberID )
		{
			//lock( typeof( Simias.mDns.RendezvousUsers ) )
			//{
				foreach( Member cMember in RendezvousUsers.memberList )
				{
					if ( cMember.UserID == memberID )
					{
						RendezvousUsers.memberList.Remove( cMember );
						return true;
					}
				}

			//}

			return false;
		}

		static public bool RemoveStagedMember( string memberID )
		{
			//lock( typeof( Simias.mDns.RendezvousUsers ) )
			//{
				foreach( Member cMember in RendezvousUsers.stagedList )
				{
					if ( memberID == cMember.UserID )
					{
						RendezvousUsers.stagedList.Remove( cMember );
						return true;
					}
				}

			//}

			return false;
		}
	}


	/// <summary>
	/// Class used to broadcast the current user
	/// to the Rendezvous/mDns network
	/// </summary>
	public class User
	{
		#region DllImports
		[ DllImport( "simdezvous" ) ]
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

		[ DllImport( "simdezvous" ) ]
		private 
		extern 
		static 
		User.kErrorType
		DeregisterLocalMember(string ID, int Cookie);

		[ DllImport( "simdezvous" ) ]
		private 
		extern 
		static 
		User.kErrorType
		GetMemberInfo(
			[MarshalAs(UnmanagedType.LPStr)] string	ID,
			[In, Out] char[]	MemberName,
			[In, Out] char[]	ServicePath,
			[In, Out] byte[]	PublicKey,
			[In, Out] char[]	HostName,
			ref       int		Port);

		[ DllImport( "simdezvous" ) ]
		private 
		extern 
		static 
		User.kErrorType
		BrowseMembersInit( MemberBrowseCallback	callback, ref IntPtr handle );
		
		[ DllImport( "simdezvous" ) ]
		private 
		extern 
		static 
		User.kErrorType
		BrowseMembersShutdown( int handle );

		[ DllImport( "simdezvous" ) ]
		private 
		extern 
		static 
		User.kErrorType
		BrowseMembers( int handle, int timeout );

		[ DllImport( "simdezvous" ) ]
		internal 
		extern 
		static 
		User.kErrorType
		ResolveAddress(
			[MarshalAs(UnmanagedType.LPStr)] string	hostName,
			int	BufferLength,
			[In, Out] char[] TextualIPAddress);
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

		//private const string nativeLibrary = "ifolder-rendezvous";

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
			XmlElement servicesElement = 
				Store.GetStore().Config.GetElement( configSection, configServices );
			webServiceUri = new Uri( servicesElement.GetAttribute( "value" ) );
			if ( webServiceUri != null )
			{
				log.Debug( "Web Service URI: " + webServiceUri.ToString() );
				log.Debug( "Absolute Path: " + webServiceUri.AbsolutePath );
			}

			/*
			User.memberListLock = new UserLock();
			User.memberList = new ArrayList();
			*/

			userHandle = new IntPtr(0);

			//Simias.mDns.Domain mdnsDomain = new Simias.mDns.Domain( true );
			//mDnsUserName = Environment.UserName + "@" + mdnsDomain.Host;
			User.mDnsUserName = Environment.UserName + "@" + Environment.MachineName;

			try
			{
				Simias.Storage.Domain rDomain = 
					Store.GetStore().GetDomain( Simias.mDns.Domain.ID );
				if ( rDomain != null )
				{
					User.mDnsUserID = rDomain.GetMemberByName( mDnsUserName).UserID;
				}
			}
			catch( Exception e )
			{
				log.Debug( e.Message );
				log.Debug( e.StackTrace );
			}
		}

		/// <summary>
		/// Constructor for newing up an mDns user object.
		/// </summary>
		internal User()
		{

			/*
			try
			{
				Simias.Storage.Domain rDomain = 
					Store.GetStore().GetDomain( Simias.mDns.Domain.ID );
				if ( rDomain != null )
				{
					this.mDnsUserID = rDomain.GetMemberByName( mDnsUserName).UserID;
				}
			}
			catch( Exception e )
			{
				log.Debug( e.Message );
				log.Debug( e.StackTrace );
			}
			*/
		}

		#endregion

		#region Internal Methods
		internal static void RegisterUser()
		{
			if ( registered == true )
			{
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
				//RSACryptoServiceProvider credential = Store.GetStore().CurrentUser.Credential;
				short sport = (short) webServiceUri.Port;

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
					throw new SimiasException( "Failed to register local member with Rendezvous" );
				}
			}
			catch( Exception e2 )
			{
				log.Error( e2.Message );
				log.Error( e2.StackTrace );
			}			
		}

		internal static void UnregisterUser()
		{
			if ( registered == true )
			{
				DeregisterLocalMember( User.mDnsUserID, userHandle.ToInt32() );
				registered = false;
			}
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
				Thread.Sleep( 1000 );
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
				status = BrowseMembersInit( myCallback, ref User.browseHandle );
				if ( status == User.kErrorType.kDNSServiceErr_NoError )
				{
					// A timeout is returning success so we're OK
					status = BrowseMembers( User.browseHandle.ToInt32(), 300 );
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
				// FIXME:: Need to handle the case where flags isn't set
				// to add so I can remove users as well


				log.Debug( "MemberCallback for: " + serviceName );

				if ( ( flags & (int) kDNSServiceFlags.kDNSServiceFlagsAdd ) == (int) kDNSServiceFlags.kDNSServiceFlagsAdd )
				{
					Member rMember = 
						new Member( 
							RendezvousUsers.StagedName,
							serviceName,
							Simias.Storage.Access.Rights.ReadWrite );

					bool added = RendezvousUsers.AddStagedMember( rMember );
					if ( added == true )
					{
						// Force a meta-data sync
						Simias.mDns.Sync.SyncNow("");
					}
				}
				else
				{
					log.Debug( "Removing member: " + serviceName );
					if ( RendezvousUsers.RemoveStagedMember( serviceName ) == false )
					{
						RendezvousUsers.RemoveMember( serviceName );
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
		/// FIXME::Temporary method to automatically synchronize all mDns users
		/// </summary>
		/// <returns>n/a</returns>
		public void SynchronizeMembers()
		{
			// FIXME::define sizes
			char[] trimNull = { '\0' };
			char[] infoHost = new char[ 64 ];
			char[]	infoName = new char[ 128 ];
			char[]	infoServicePath = new char[ 128 ];
			byte[]	infoPublicKey = new byte[ 512 ];

			log.Debug( "Syncing mDns members" );
			//Thread.Sleep( 30000 );

			Simias.Storage.Member rMember;
			lock( typeof( Simias.mDns.RendezvousUsers ) )
			{
				IEnumerator memberEnum = RendezvousUsers.stagedList.GetEnumerator();
				while( memberEnum.MoveNext() )
				{
					rMember = memberEnum.Current as Simias.Storage.Member;

					// Go get the rest of the meta-data for this user
					User.kErrorType status;

					try
					{
						int	port = 0;
						log.Debug( "Calling GetMemberInfo for: " + rMember.UserID );
						status = 
							GetMemberInfo( 
								rMember.UserID,
								infoName,
								infoServicePath,
								infoPublicKey,
								infoHost,
								ref port );

						if ( status == kErrorType.kDNSServiceErr_NoError )
						{
							rMember.Name = (new string( infoName )).TrimEnd( trimNull );
							rMember.FN = rMember.Name;

							Property host = 
								new Property( 
										RendezvousUsers.HostProperty, 
										(new string( infoHost )).TrimEnd( trimNull ) );
							host.LocalProperty = true;
							rMember.Properties.AddProperty( host );

							Property path = 
								new Property( 
								RendezvousUsers.PathProperty,
								(new string( infoServicePath )).TrimEnd( trimNull ) );
							path.LocalProperty = true;
							rMember.Properties.AddProperty( path );

							Property rport = new Property( RendezvousUsers.PortProperty, port );
							rMember.Properties.AddProperty( rport );

							UTF8Encoding utf8 = new UTF8Encoding();
							string pubKey = utf8.GetString( infoPublicKey );

							Property key = 
								new Property( 
									RendezvousUsers.KeyProperty, 
									pubKey.TrimEnd( trimNull ) );

							rMember.Properties.AddProperty( key );

							//rUser.PublicKey = (new string( infoPublicKey )).TrimEnd( trimNull );
							log.Debug( "Adding meta-data for: " + rMember.Name );
							RendezvousUsers.stagedList.Remove( rMember );
							if ( RendezvousUsers.AddMember( rMember, true ) == false )
							{
								RendezvousUsers.UpdateMember( rMember );
							}

							// The collection was changed so reset up a new enumerator
							memberEnum = RendezvousUsers.stagedList.GetEnumerator();
						}
					}
					catch ( Exception e2 )
					{
						log.Debug( e2.Message );
						log.Debug( e2.StackTrace );

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
		}
		#endregion
	}

