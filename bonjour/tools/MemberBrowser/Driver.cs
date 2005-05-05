using System;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

using Gtk;
using GLib;

namespace MemberBrowser
{
	/// <summary>
	/// Summary description for Driver.
	/// </summary>
	class Driver : Gtk.Window
	{
    	public string m_AppName = "iFolder Member Browser";  	
    
    	private VBox m_FrmPanel = null;
    	private VBox m_MenuAndToolBarPanel = null;
    	private MenuBar m_MainMenuBar = null;
		private Toolbar m_MainToolbar = null;
		private AccelGroup m_AccelGroup = null;

		private	static ListStore store = null;
		private static IntPtr browseHandle;
		private static System.Threading.Thread browseThread = null;
		static internal ArrayList memberList = new ArrayList();

#if DARWIN
		private const string nativeLib = "libsimdezvous.dylib";
#else
		private const string nativeLib = "libsimdezvous";
#endif

		public enum MemberStatus : int
		{
			Up = 1,
			Down
		}

		public class Member
		{
			public int			TouchedBy;
			public MemberStatus	Status;
			public DateTime		LastUpdate;
			public int			Port;
			public string		ID;
			public string		Name;
			public string		ServicePath;
			public string		Host;
			public string		Address;
			public string		PublicKey;
		}

		[ StructLayout( LayoutKind.Sequential, CharSet=CharSet.Ansi ) ]
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

			public int Port = 0;
		}

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

		#region DllImports
		[ DllImport( nativeLib, CharSet=CharSet.Auto ) ]
		private 
		extern 
		static 
		kErrorType
		GetMemberInfo(
			[MarshalAs(UnmanagedType.LPStr)] string	ID,
			[In, Out] MemberInfo Info);

		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		kErrorType
		BrowseMembersInit( MemberBrowseCallback	callback, ref IntPtr handle );
		
		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		kErrorType
		BrowseMembersShutdown( int handle );

		[ DllImport( nativeLib ) ]
		private 
		extern 
		static 
		kErrorType
		BrowseMembers( int handle, int timeout );
		#endregion

		private bool MemberManager()
		{
			bool	foundMember;
			string	viewName;
			lock( Driver.memberList )
			{
				TreeIter iter;
				Member member;
				IEnumerator memberEnum = Driver.memberList.GetEnumerator();
				while( memberEnum.MoveNext() )
				{
					foundMember = false;
					member = memberEnum.Current as Member;

					// First see if the member is in the list
					bool results = store.GetIterFirst( out iter );
					while ( results == true )
					{
						viewName = (string ) store.GetValue( iter, 0 );
						if ( viewName == member.Name )
						{
							foundMember = true;
							break;
						}

						results = store.IterNext( ref iter );
					}

					if ( member.Status == MemberStatus.Down && foundMember == true )
					{
						store.Remove( ref iter );
						Driver.memberList.Remove( member );
						memberEnum = Driver.memberList.GetEnumerator();
					}
					else
					if ( foundMember == true )
					{
						// If the last update was more than n seconds then recheck the address
						DateTime current = DateTime.Now;
						if ( current.Ticks > ( member.LastUpdate.Ticks + (10000 * 1000 * 60) ) )
						{
							member.LastUpdate = current;
							IPHostEntry host = Dns.GetHostByName( member.Host );
							if ( host != null )
							{
								long addr = host.AddressList[0].Address;
								string address = 
									String.Format( "{0}.{1}.{2}.{3}:{4}", 
										( addr & 0x000000FF ),
										( ( addr >> 8 ) & 0x000000FF ),
										( ( addr >> 16 ) & 0x000000FF ),
										( ( addr >> 24 ) & 0x000000FF ),
										member.Port );

								if ( address != member.Address )
								{
									store.SetValue( iter, 0, address );
									member.Address = address;
								}
							}
						}
					}
					else
					{
						IPHostEntry host = Dns.GetHostByName( member.Host );
						if ( host != null )
						{
							long addr = host.AddressList[0].Address;
							member.Address = 
								String.Format( "{0}.{1}.{2}.{3}:{4}", 
								( addr & 0x000000FF ),
								( ( addr >> 8 ) & 0x000000FF ),
								( ( addr >> 16 ) & 0x000000FF ),
								( ( addr >> 24 ) & 0x000000FF ),
								member.Port );

							member.LastUpdate = DateTime.Now;
							store.AppendValues( member.Name, member.ServicePath, member.Address );
						}
					}
				}
			}

			return true;
		}

		private bool BrowseThreadStartup()
		{
			Driver.browseThread = new System.Threading.Thread( new ThreadStart( MemberBrowsing ) );
			Driver.browseThread.IsBackground = true;
			Driver.browseThread.Start();

			// Setup a reoccurring callback so we can manage our list
			GLib.Timeout.Add( 5000, new GLib.TimeoutHandler( MemberManager ) );

			// once and done
			return false;
		}

		internal static void MemberBrowsing()
		{
			kErrorType status;
			Driver.browseHandle = new IntPtr( 0 );
			MemberBrowseCallback myCallback = new MemberBrowseCallback( MemberCallback );

			status = BrowseMembersInit( myCallback, ref Driver.browseHandle );
			if ( status == kErrorType.kDNSServiceErr_NoError )
			{
				status = BrowseMembers( Driver.browseHandle.ToInt32(), 300 );
			}

			return;
		}

		private static bool MemberCallback( 
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
				if ( ( flags & (int) kDNSServiceFlags.kDNSServiceFlagsAdd ) == 
					(int) kDNSServiceFlags.kDNSServiceFlagsAdd )
				{

					MemberInfo info = new MemberInfo();
					kErrorType status = GetMemberInfo( serviceName, info );
					if ( status == kErrorType.kDNSServiceErr_NoError )
					{
						Member member = new Member();
						member.ID = serviceName;
						member.Name = info.Name;
						member.ServicePath = info.ServicePath;
						member.PublicKey = info.PublicKey;
						member.Host = info.Host;
						member.LastUpdate = DateTime.Now;
						member.TouchedBy = 1;
						member.Status = MemberStatus.Up;
						member.Port = info.Port;

						lock( Driver.memberList )
						{
							Driver.memberList.Add( member );
						}
						//store.AppendValues( info.Name, info.ServicePath, "137.65.58.34" );
					}
				}
				else
				{
					foreach( Member member in Driver.memberList )
					{
						if ( member.ID == serviceName )
						{
							member.Status = MemberStatus.Down;
							member.TouchedBy = 1;
							break;
						}
					}
				}
			}

			return true;
		}


		#region Wizard generated code    	
		protected void InitializeComponents()
    	{
			// Create a new instance of the global
			// Accelerator group
			m_AccelGroup = new AccelGroup();

    		// Set the Windows characteristics
    		this.Title = m_AppName; 
    		this.SetDefaultSize( 400, 300 );
    		this.DeleteEvent += new DeleteEventHandler( OnMyWindowDelete );
			this.AddAccelGroup( m_AccelGroup );
    
    		// Vertical panel that host all other panels
    		m_FrmPanel = new VBox( false, 3 );
    		
			// Vertical panel that will host the 
			// menubar and the toolbar
			m_MenuAndToolBarPanel = new VBox( false, 2 );
    
			#region Menus
    		
    		// Menu bar configuration
    		m_MainMenuBar = new MenuBar();
			
			/*
    		// File menu
    		Menu FileMenu = new Menu();
    		MenuItem FileMenuItem = new MenuItem("_File");
    		FileMenuItem.Submenu = FileMenu;
			FileMenu.AccelGroup = m_AccelGroup;
    		
			// File New menu item
			Gtk.ImageMenuItem NewMenuItem = new ImageMenuItem("gtk-new", m_AccelGroup);
			NewMenuItem.Activated += new EventHandler(NewMenuItem_OnActivate);
			FileMenu.Append(NewMenuItem);
    		
			// File Open menu item
			Gtk.ImageMenuItem OpenMenuItem = new ImageMenuItem("gtk-open", m_AccelGroup);
			OpenMenuItem.Activated += new EventHandler(OpenMenuItem_OnActivate);
			FileMenu.Append(OpenMenuItem);
    		
			// File Save menu item
			Gtk.ImageMenuItem SaveMenuItem = new ImageMenuItem("gtk-save", m_AccelGroup);
    		SaveMenuItem.Activated += new EventHandler(SaveMenuItem_OnActivate);
    		FileMenu.Append(SaveMenuItem);
    		
    		// File SaveAs menu item
			Gtk.ImageMenuItem SaveAsMenuItem = new ImageMenuItem("gtk-save-as", m_AccelGroup);
    		SaveAsMenuItem.Activated += new EventHandler(SaveAsMenuItem_OnActivate);
    		FileMenu.Append(SaveAsMenuItem);
    		
    		// File Separator menu item
    		SeparatorMenuItem Separator1MenuItem = new SeparatorMenuItem();
    		FileMenu.Append(Separator1MenuItem);
    		
    		// File Exit menu item
			Gtk.ImageMenuItem ExitMenuItem = new ImageMenuItem("gtk-quit", m_AccelGroup);
    		ExitMenuItem.Activated += new EventHandler(ExitMenuItem_OnActivate);
    		FileMenu.Append(ExitMenuItem);
    		
    		// Add the menus to the menubar	
			m_MainMenuBar.Append(FileMenuItem);
			*/
    		#endregion

			#region Toolbar
			
			m_MainToolbar = new Toolbar();
			
			// Toolbar buttons
			Gtk.ToolButton button1 = new ToolButton("gtk-new");
			button1.Clicked +=new EventHandler(button1_Clicked);

			// Add the buttons to the toolbar
			// File operations buttons
			m_MainToolbar.Add( button1 );
			
			#endregion

			#region List
			TreeView tv = new TreeView();
			tv.HeadersVisible = true;
			m_FrmPanel.Add( tv );

			TreeViewColumn tvColumnMember = new TreeViewColumn();
			CellRenderer tvColRend = new CellRendererText();
			tvColumnMember.Title = "Member";
			tvColumnMember.PackStart( tvColRend, true );
			tvColumnMember.AddAttribute( tvColRend, "text", 0 );
			tvColumnMember.MinWidth = 120;
			tv.AppendColumn( tvColumnMember );

			TreeViewColumn tvColumnPath = new TreeViewColumn();
			CellRenderer tvPathRend = new CellRendererText();
			tvColumnPath.Title = "Service Path";
			tvColumnPath.MinWidth = 120;
			tvColumnPath.PackStart( tvPathRend, true );
			tvColumnPath.AddAttribute( tvPathRend, "text", 1 );
			tv.AppendColumn( tvColumnPath );

			TreeViewColumn tvColumnAddress = new TreeViewColumn();
			CellRenderer tvAddrRend = new CellRendererText();
			tvColumnAddress.Title = "Address";
			tvColumnAddress.MinWidth = 120;
			tvColumnAddress.PackStart( tvAddrRend, true );
			tvColumnAddress.AddAttribute( tvAddrRend, "text", 2 );
			tv.AppendColumn( tvColumnAddress );

			//this.store = new ListStore( typeof( string ), typeof( string ) );
			Driver.store = new ListStore( typeof( string ), typeof( string ), typeof( string ) );
			tv.Model = store;

			TreeIter iter = new TreeIter();

			/*
			iter = store.AppendValues( "banderso@BRADY-T41P", "/simias10/banderso", "137.65.58.34" );
			iter = store.AppendValues( "mlasky@MLASKY-DELL", "/simias10/mlasky", "137.65.58.93" );
			iter = store.AppendValues( "cgaisford@Calvins-Powerbook", "/simias10", "137.65.58.12" );
			*/

			/*
			for( int i = 0; i < 5; i++ )
			{
				iter = store.AppendValues( "Point " + i.ToString() );
			}
			*/

			#endregion

			/*
			// Add the menubar to the panel
			m_MenuAndToolBarPanel.PackStart(m_MainMenuBar, false, false, 0);
    		
			// Add the toolbar to the panel
			m_MenuAndToolBarPanel.PackStart(m_MainToolbar, false, false, 0);
			*/
   		
    		// Add the panel to the main window
    		m_FrmPanel.PackStart( m_MenuAndToolBarPanel, false, false, 0 );
    		this.Add( m_FrmPanel );

			GLib.Timeout.Add( 1000, new GLib.TimeoutHandler( this.BrowseThreadStartup ) );
		}

		#endregion
  	   	
    	public Driver() : base ("Driver")
    	{
    		InitializeComponents();
    		this.ShowAll();
    	}
    	
    	private void OnMyWindowDelete (object o, DeleteEventArgs args)
    	{
			BrowseMembersShutdown( Driver.browseHandle.ToInt32() );
			System.Threading.Thread.Sleep( 100 );

			Application.Quit();
			args.RetVal = true;
    	}
    	
    	#region Menu handlers
    	
		private void NewMenuItem_OnActivate(object o, EventArgs args)
		{
		}
    	
		private void ExitMenuItem_OnActivate(object o, EventArgs args)
		{
			BrowseMembersShutdown( Driver.browseHandle.ToInt32() );
			System.Threading.Thread.Sleep( 100 );

			Application.Quit();
		}

		private void AboutMenuItem_OnActivate(object o, EventArgs args)
		{
			System.Text.StringBuilder AuthorStringBuild = 
				new System.Text.StringBuilder ();
			String []authors = new String[] { "Brady Anderson <banderso@novell.com>" };
            
			AuthorStringBuild.Append( "Driver version 1.0\n\n" );
			AuthorStringBuild.Append( "iFolder Member Browser.\n" );
			AuthorStringBuild.Append( "Copyright (c) 2005\n\n" );
			AuthorStringBuild.AppendFormat(
				"Authors:\n\t{0}\n\t",
				authors[0] ); 
            
			MessageDialog md = 
				new MessageDialog(
						this,
						DialogFlags.DestroyWithParent,
						MessageType.Info,
						ButtonsType.Ok, 
						AuthorStringBuild.ToString() );
            
			md.Modal = true;
			int result = md.Run();
			md.Hide();
			return;
		}

		#endregion

		#region Toolbar buttons handlers

		// File New clicked event
		private void button1_Clicked(object sender, EventArgs e)
		{
			return;
		}

		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) 
		{
			Application.Init();
			new Driver();
			Application.Run();
		}

		public delegate bool MemberBrowseCallback(
			int			handle,
			int			flags,
			uint		ifIndex,
			kErrorType	errorCode,
			[MarshalAs(UnmanagedType.LPStr)] string serviceName,
			[MarshalAs(UnmanagedType.LPStr)] string regType,
			[MarshalAs(UnmanagedType.LPStr)] string domain,
			[MarshalAs(UnmanagedType.I4)] int context);

	}    
}

