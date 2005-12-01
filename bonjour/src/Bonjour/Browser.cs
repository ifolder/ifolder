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
	/// Browser is responsible for discovering other
	/// Simias users on the Bonjour network.
	/// 
	/// Browser also exports an api for enumerating
	/// the users both in the local domain as well
	/// as users on the net
	/// </summary>
	public class Browser
	{
#if DARWIN
		private const string nativeLib = "libsimiasbonjour.dylib";
#else
		private const string nativeLib = "libsimiasbonjour";
#endif

		#region MemberInfo
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
		#endregion

		#region DllImports
		[ DllImport( nativeLib, CharSet=CharSet.Auto ) ]
		private 
		extern 
		static 
		Types.kErrorType
		GetMemberInfo(
			[MarshalAs(UnmanagedType.LPStr)] string	ID,
			[In, Out] MemberInfo Info);

		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		Types.kErrorType
		BrowseMembersInit( MemberBrowseCallback	callback, ref IntPtr handle );
		
		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		Types.kErrorType
		BrowseMembersShutdown( int handle );

		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		Types.kErrorType
		BrowseMembers( int handle, int timeout );
		#endregion

		#region Private Members
		private IntPtr browseHandle;
		private Thread browseThread = null;
		private bool browsing = false;
		private bool stopBrowsing = false;

		private const string host = "RHost";
		private const string path = "RPath";
		private const string port = "RPort";
		private const string key = "RPublicKey";

		public static ArrayList MemberList = new ArrayList();
		public static string MemberListLock = "LockIt";
		
		// Note: change to false when releasing or checking in
		private bool extraDebug = true;

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Properties
		/// <summary>
		/// Gets the host property name used in Simias
		/// </summary>
		public string HostProperty
		{
			get { return host; }
		}

		/// <summary>
		/// Gets the path property name used in Simias
		/// </summary>
		public string PathProperty
		{
			get { return path; }
		}

		/// <summary>
		/// Gets the port property name used in the simias member object
		/// </summary>
		public string PortProperty
		{
			get { return port; }
		}

		/// <summary>
		/// Gets the key property name used in simias member object
		/// </summary>
		public string KeyProperty
		{
			get { return key; }
		}
		#endregion

		public Browser()
		{
		}

		#region Internal Methods
		internal bool LoadMembersFromDomain( bool verifyUniqueness )
		{
			// Add the members in the store
			Simias.Storage.Domain domain = Store.GetStore().GetDomain( Simias.mDns.Domain.ID );
			ICSList memberlist = domain.GetMemberList();

			lock( Simias.mDns.Browser.MemberListLock )
			{
				foreach(ShallowNode sNode in memberlist)
				{
					// Get the member from the list
					Simias.Storage.Member member =
						new Simias.Storage.Member( domain, sNode );

					if ( verifyUniqueness == true )
					{
						// FIXME::
					}

					Property p = new Property( "InStore", true );
					member.Properties.AddProperty( p );
					MemberList.Add( member );
				}
			}

			return true;
		}

		internal Member GetMember( string memberID )
		{
			Member member = null;

			try
			{
				foreach( Member cMember in MemberList )
				{
					if ( cMember.UserID == memberID )
					{
						member = cMember;
						break;
					}
				}
			}
			catch{} // In case a member is added/deleting while we're enumerating
			return member;
		}

		internal string GetMembersPublicKey( string memberID )
		{
			string publicKey = null;
			try
			{
				foreach( Member cMember in MemberList )
				{
					if ( cMember.UserID == memberID )
					{
						publicKey = cMember.Properties.GetSingleProperty( KeyProperty ).Value as string;
						break;
					}
				}

			}			
			catch{}
			return publicKey;
		}
		#endregion

		#region Private Methods
		private bool AddMember( Member member, bool verifyUniqueness )
		{
			bool added = false;
			bool duplicate = false;
			//lock( typeof( Simias.mDns.Browser ) )
			lock( MemberListLock  )
			{
				if ( verifyUniqueness == true )
				{
					foreach( Member cMember in MemberList )
					{
						if ( cMember.UserID == member.UserID )
						{
							duplicate = true;
							break;
						}
					}
				}

				if ( duplicate == false )
				{
					if ( member.Name != null && member.Name != "" &&
						member.Properties.GetSingleProperty( this.HostProperty ).Value != null &&
						member.Properties.GetSingleProperty( this.PortProperty ).Value != null &&
						member.Properties.GetSingleProperty( this.PathProperty ).Value != null )
					{
						MemberList.Add( member );
						added = true;
					}
				}
			}

			return added;
		}

		private bool UpdateMember( Member member )
		{
			bool updated = false;
			lock( MemberListLock )
			{
				foreach( Member cMember in MemberList )
				{
					if ( cMember.UserID == member.UserID )
					{
						// Out with the old in with the new
						MemberList.Remove( cMember );
						MemberList.Add( member );
						updated = true;
						break;
					}
				}
			}

			return updated;
		}

		private bool RemoveMember( string memberID )
		{
			bool removed = false;
			lock( MemberListLock )
			{
				foreach( Member cMember in MemberList )
				{
					if ( cMember.UserID == memberID )
					{
						MemberList.Remove( cMember );
						removed = true;
						break;
					}
				}
			}
			return removed;
		}

		private
		delegate 
		bool 
		MemberBrowseCallback(
			int					handle,
			int					flags,
			uint				ifIndex,
			Types.kErrorType	errorCode,
			[MarshalAs(UnmanagedType.LPStr)] string serviceName,
			[MarshalAs(UnmanagedType.LPStr)] string regType,
			[MarshalAs(UnmanagedType.LPStr)] string domain,
			[MarshalAs(UnmanagedType.I4)] int context);

		private void DumpCurrentMembers()
		{
			log.Debug( "Current Bonjour Members" );
			foreach( Member member in MemberList )
			{
				log.Debug( "  ID:   " + member.UserID );
				log.Debug( "  Name: " + member.Name );
				log.Debug( "  Host: " + member.Properties.GetSingleProperty( this.HostProperty ).Value as string );
				log.Debug( "  Port: " + member.Properties.GetSingleProperty( this.PortProperty ).Value as string );
				log.Debug( "  Path: " + member.Properties.GetSingleProperty( this.PathProperty ).Value as string );
				log.Debug( "" );

			}
		}

		private void BrowseThread()
		{
			Types.kErrorType status;
			browseHandle = new IntPtr( 0 );
			MemberBrowseCallback myCallback = new MemberBrowseCallback( MemberCallback );

			do
			{
				log.Debug( "  calling BrowseMembersInit" );
				status = BrowseMembersInit( myCallback, ref browseHandle );
				if ( status == Types.kErrorType.kDNSServiceErr_NoError )
				{
					// A timeout is returning success so we're OK
					log.Debug( "  calling BrowseMembers" );
					status = BrowseMembers( browseHandle.ToInt32(), 60 );
					browsing = true;
					log.Debug( "  return from BrowseMembers - status: " + status.ToString() );
				}

				if ( stopBrowsing == true )
				{
					log.Debug( "Detected: stop browsing" );
					break;
				}

				if ( extraDebug == true )
				{
					DumpCurrentMembers();
				}

			} while ( status == Types.kErrorType.kDNSServiceErr_NoError );

			log.Debug( "BrowseThread down..." );
			log.Debug( "Status: " + status.ToString() );
		}

		private
		static 
		bool 
		MemberCallback( 
			int					handle,
			int					flags,
			uint				ifIndex,
			Types.kErrorType	errorCode,
			[MarshalAs(UnmanagedType.LPStr)] string serviceName,
			[MarshalAs(UnmanagedType.LPStr)] string regType,
			[MarshalAs(UnmanagedType.LPStr)] string domain,
			[MarshalAs(UnmanagedType.I4)] int context)
		{ 
			if ( errorCode == Types.kErrorType.kDNSServiceErr_NoError )
			{
				Types.kErrorType status;
				log.Debug( "MemberCallback for: " + serviceName );

				Simias.mDns.Browser browser = new Simias.mDns.Browser();

				if ( ( flags & (int) Types.kDNSServiceFlags.kDNSServiceFlagsAdd ) == 
					(int) Types.kDNSServiceFlags.kDNSServiceFlagsAdd )
				{
					// Call back down to Bonjour to get the rest of 
					// the information we need
					MemberInfo info = new MemberInfo();
					status = GetMemberInfo( serviceName, info );
					if ( status == Types.kErrorType.kDNSServiceErr_NoError )
					{
						log.Debug( "  user: " + info.Name );
						log.Debug( "  host: " + info.Host );
						log.Debug( "  port: " + info.Port.ToString() );
						
						Simias.Storage.Member member =
							new Member( info.Name, serviceName, Simias.Storage.Access.Rights.ReadWrite );
						if ( member != null )
						{
							member.FN = info.Name;
							Property host = 
								new Property( browser.HostProperty, info.Host );
							host.LocalProperty = true;
							member.Properties.AddProperty( host );

							Property path = 
								new Property( browser.PathProperty,	info.ServicePath );
							path.LocalProperty = true;
							member.Properties.AddProperty( path );

							Property rport = new Property( browser.PortProperty, info.Port.ToString() );
							rport.LocalProperty = true;
							member.Properties.AddProperty( rport );

							Property key = new Property( browser.KeyProperty, info.PublicKey );
							key.LocalProperty = true;
							member.Properties.AddProperty( key );

							if ( browser.AddMember( member, true ) == false )
							{
								browser.UpdateMember( member );
							}
						}
					}
				}
				else
				{
					log.Debug( "Removing member: " + serviceName );
					browser.RemoveMember( serviceName );
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
		public void StartBrowsing()
		{
			if ( browsing == true )
			{
				ApplicationException ae = new ApplicationException( "Already browsing for members!" );
				throw ae;
			}

			stopBrowsing = false;
			browseHandle = new IntPtr( 0 );
			browseThread = new Thread( new ThreadStart( BrowseThread ) );
			browseThread.IsBackground = true;
			browseThread.Start();
		}

		public void StopBrowsing()
		{
			stopBrowsing = true;
			if ( browseHandle.ToInt32() != 0 )
			{
				BrowseMembersShutdown( browseHandle.ToInt32() );
				Thread.Sleep( 32 );
				if ( browseThread.IsAlive == true )
				{
					// Signal the event to shut him down
				}
			}
		}
		#endregion
	}
}
