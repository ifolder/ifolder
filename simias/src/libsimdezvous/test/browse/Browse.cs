// project created on 3/22/2005 at 11:18 PM
using System;
using System.Runtime.InteropServices;

class MainClass
{
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

	[ StructLayout( LayoutKind.Sequential, CharSet=CharSet.Ansi )]
	public class MemberInfo
	{
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst=64 ) ]
		public String Name = null;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst=128 ) ]
		public String ServicePath = null;

		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst=256 ) ]
		public String PublicKey = null;

		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst=64 ) ]
		public String Host = null;

		public int Port;
	}


	#region DllImports

	[ DllImport( "simdezvous", CharSet=CharSet.Auto ) ]
	private 
	extern 
	static 
	kErrorType
	GetMemberInfo(
		[MarshalAs(UnmanagedType.LPStr)] string	ID,
		[In, Out] MemberInfo Info);

	[ DllImport( "simdezvous" ) ]
	private 
	extern 
	static 
	kErrorType
	BrowseMembersInit( MemberBrowseCallback	callback, ref IntPtr handle );
		
	[ DllImport( "simdezvous" ) ]
	private 
	extern 
	static 
	kErrorType
	BrowseMembersShutdown( int handle );

	[ DllImport( "simdezvous" ) ]
	private 
	extern 
	static 
	kErrorType
	BrowseMembers( int handle, int timeout );
	#endregion
	
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
	
	
	public static bool MemberCallback( 
		int			handle,
		int			flags,
		uint		ifIndex,
		kErrorType	errorCode,
		[MarshalAs(UnmanagedType.LPStr)] string serviceName,
		[MarshalAs(UnmanagedType.LPStr)] string regType,
		[MarshalAs(UnmanagedType.LPStr)] string domain,
		[MarshalAs(UnmanagedType.I4)] int context)
	{
		Console.WriteLine( "ID:            " + serviceName );
		
		MemberInfo info = new MemberInfo();
		kErrorType err = GetMemberInfo( serviceName, info );
		if ( err == kErrorType.kDNSServiceErr_NoError )
		{
			Console.WriteLine( "Friendly Name: " + info.Name );
			Console.WriteLine( "Service Path:  " + info.ServicePath );
			Console.WriteLine( "Host:          " + info.Host );
			Console.WriteLine( "Port:          " + info.Port.ToString() );
		}
		else
		{
			Console.WriteLine( "GetMemberInfo2 returned error: " + err.ToString() );
		}
		
		return true;
	}
	
	public static void Main(string[] args)
	{
		kErrorType status;
		IntPtr browseHandle = new IntPtr( 0 );
		MemberBrowseCallback myCallback = new MemberBrowseCallback( MemberCallback );

		do
		{
			Console.WriteLine( "  calling BrowseMembersInit" );
			status = BrowseMembersInit( myCallback, ref browseHandle );
			if ( status == kErrorType.kDNSServiceErr_NoError )
			{
				Console.WriteLine( "Browse Handle: " + browseHandle.ToString() );
				// A timeout is returning success so we're OK
				Console.WriteLine( "  calling BrowseMembers" );
				status = BrowseMembers( browseHandle.ToInt32(), 30 );
			}
		} while ( status == kErrorType.kDNSServiceErr_NoError );

		Console.WriteLine( "Out of do while loop" );
		Console.WriteLine( "Status: " + status.ToString() );
	}
	
}