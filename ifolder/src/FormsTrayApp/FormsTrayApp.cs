/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Collections;
using Novell.iFolderCom;
using Novell.Win32Util;
using CustomUIControls;
using Simias.Client;
using Simias.Client.Event;
using Novell.iFolder.Install;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for FormsTrayApp.
	/// </summary>
	public class FormsTrayApp : Form
	{
        #region Class Members
		/// <summary>
		/// Well known identitifer for workgroup.
		/// </summary>
		static public string WorkGroupDomainID = "363051d1-8841-4c7b-a1dd-71abbd0f4ada";

		// Delegates used to marshal back to the control's creation thread.
		private delegate void SyncCollectionDelegate(CollectionSyncEventArgs syncEventArgs);
		private SyncCollectionDelegate syncCollectionDelegate;
		private delegate void SyncFileDelegate(FileSyncEventArgs syncEventArgs);
		private SyncFileDelegate syncFileDelegate;

		private delegate void CreateChangeEventDelegate(iFolderWeb ifolder, iFolderUser ifolderUser, string eventData);
		private CreateChangeEventDelegate createChangeEventDelegate;

		private delegate void NotifyMessageDelegate(NotifyEventArgs notifyEventArgs);
		private NotifyMessageDelegate notifyMessageDelegate;

		private System.ComponentModel.IContainer components;
		static private System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(FormsTrayApp));
		private bool shutdown = false;

		private Icon trayIcon;
		private Icon startupIcon;
        private const int numberOfSyncIcons = 10;
		private Icon[] syncIcons = new Icon[numberOfSyncIcons];
		private int index = 0;

		private Queue eventQueue;
		private Thread worker = null;

		private Hashtable initialSyncCollections = new Hashtable();

		/// <summary>
		/// Event used to signal that there are events in the queue that need to be processed.
		/// </summary>
		protected AutoResetEvent workEvent = null;

		private ServerInfo serverInfo = null;
		private bool loginCancelled = false;
		private System.Windows.Forms.MenuItem menuItem10;
		private System.Windows.Forms.MenuItem menuSeparator1;
		private System.Windows.Forms.MenuItem menuProperties;
		private System.Windows.Forms.MenuItem menuHelp;
		private System.Windows.Forms.MenuItem menuExit;
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuEventLogReader;
		private iFolderWebService ifWebService = null;
		private SimiasWebService simiasWebService;
		private IProcEventClient eventClient;
		private GlobalProperties globalProperties;
		private Preferences preferences;
		private SyncLog syncLog;
		private bool eventError = false;
		private IntPtr hwnd;
		private System.Windows.Forms.MenuItem menuStoreBrowser;
		private System.Windows.Forms.MenuItem menuTools;
		private System.Windows.Forms.Timer syncAnimateTimer;
		private System.Windows.Forms.MenuItem menuSyncLog;
		private System.Windows.Forms.MenuItem menuPreferences;
		private System.Windows.Forms.MenuItem menuAccounts;
		private int iconID;
		#endregion

		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length == 1 && args[0].Equals("-checkautorun"))
			{
				// Check if auto-run is enabled.
				if (!Preferences.IsRunEnabled())
				{
					// Auto-run is disabled ... exit.
					return;
				}
			}

			Application.Run(new FormsTrayApp());
		}

		/// <summary>
		/// Constructs a FormsTrayApp object.
		/// </summary>
		public FormsTrayApp()
		{
			syncCollectionDelegate = new SyncCollectionDelegate(syncCollection);
			syncFileDelegate = new SyncFileDelegate(syncFile);

			createChangeEventDelegate = new CreateChangeEventDelegate(createChangeEvent);
			notifyMessageDelegate = new NotifyMessageDelegate(notifyMessage);

			// Check for currently running instance.  Search for existing window ...
			string windowName = resourceManager.GetString("$this.Text") + " " + Environment.UserName;
			Novell.Win32Util.Win32Window window = Novell.Win32Util.Win32Window.FindWindow(null, windowName);
			if (window != null)
			{
				MessageBox.Show(resourceManager.GetString("iFolderRunning"));
				shutdown = true;
			}
			else
			{
				InitializeComponent();

				// Append the username to the window string so we can search for it.
				this.Text += " " + Environment.UserName;

				this.components = new System.ComponentModel.Container();

				// Set up how the form should be displayed.
				this.ClientSize = new System.Drawing.Size(292, 266);

				// The Icon property sets the icon that will appear
				// in the systray for this application.
				try
				{
					string basePath = Path.Combine(Application.StartupPath, "res");
					this.Icon = new Icon(Path.Combine(Application.StartupPath, "ifolder_app.ico"));

					trayIcon = new Icon(Path.Combine(basePath, "ifolder_loaded.ico"));
					startupIcon = new Icon(Path.Combine(basePath, "ifolder-startup.ico"));
					syncIcons[0] = new Icon(trayIcon, trayIcon.Size);
					for (int i = 0; i < numberOfSyncIcons; i++)
					{
						string syncIcon = string.Format(Path.Combine(basePath, "ifolder_sync{0}.ico"), i+1);
						syncIcons[i] = new Icon(syncIcon);
					}
			
					notifyIcon1.Icon = startupIcon;
					this.ShowInTaskbar = false;
					this.WindowState = FormWindowState.Minimized;
					//this.Hide();

					Win32Window win32Window = new Win32Window();
					win32Window.Window = this.Handle;
					win32Window.MakeToolWindow();
				}
				catch
				{
				}		

				Type t = notifyIcon1.GetType();
				hwnd = ((NativeWindow)t.GetField("window",System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(notifyIcon1)).Handle;
				iconID = (int)t.GetField("id",System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(notifyIcon1);
			}

			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormsTrayApp_Closing);
			this.Load += new System.EventHandler(FormsTrayApp_Load);
		}

		/// <summary>
		/// Disposes the object.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose( bool disposing )
		{
			// Clean up any components being used.
			if( disposing )
				if (components != null)
					components.Dispose();

			base.Dispose( disposing );
		}

		/// <summary>
		/// Method used to check for a client update on the server.
		/// </summary>
		/// <param name="domainID">The ID of the domain.</param>
		/// <param name="userName">The name of the user.</param>
		/// <param name="password">The user's password.</param>
		/// <returns><b>True</b> if an update exists and has been started; otherwise, <b>False</b>.</returns>
		static public bool CheckForClientUpdate(string domainID, string userName, string password)
		{
			bool updateStarted = false;
			ClientUpgrade cUpgrade = new ClientUpgrade(domainID, userName, password);
			string version = cUpgrade.CheckForUpdate();
			if ( version != null )
			{
				// Pop up a dialog here and ask if the user wants to update the client.
				MyMessageBox mmb = new MyMessageBox(string.Format(resourceManager.GetString("clientUpgradePrompt"), version), resourceManager.GetString("clientUpgradeTitle"), string.Empty, MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question);
				DialogResult result = mmb.ShowDialog();
				if ( result == DialogResult.Yes )
				{
					updateStarted = cUpgrade.RunUpdate();
					if ( updateStarted == false )
					{
						mmb = new MyMessageBox(resourceManager.GetString("clientUpgradeFailure"), resourceManager.GetString("clientUpgradeTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
						mmb.ShowDialog();
					}
				}
			}

			return updateStarted;
		}

		#region Event Handlers
		private void menuStoreBrowser_Click(object sender, System.EventArgs e)
		{
			Process.Start(Path.Combine(Application.StartupPath, "StoreBrowser.exe"));
		}

		private void menuEventLogReader_Click(object sender, System.EventArgs e)
		{
			Process.Start(Path.Combine(Application.StartupPath, "EventLogReader.exe"));
		}

		private void menuAccounts_Click(object sender, System.EventArgs e)
		{
			// TODO: this may need to move if the server connect dialog goes away.
			// Check for old versions of the iFolder client.
/*			iFolderMigration migrate = new iFolderMigration(config);
			if (migrate.CanBeMigrated())
			{
				if (DialogResult.Yes == MessageBox.Show("An old version of iFolder has been detected.  Do you want to migrate your old iFolder client settings to the new iFolder client?", "Migrate iFolder Client Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
				{
					// Migrate the settings.
					migrate.MigrateSettings();
				}
				else
				{
					// Set state so that we don't ask again.
					migrate.SetMigratedValue(0);
				}
			}*/

			if (preferences.Visible)
			{
				preferences.Activate();
			}
			else
			{
				preferences.Show();
			}

			preferences.SelectAccounts();
		}

		private void menuProperties_Click(object sender, System.EventArgs e)
		{
			if (globalProperties.Visible)
			{
				globalProperties.Activate();
			}
			else
			{
				globalProperties.Show();
			}
		}

		private void menuPreferences_Click(object sender, System.EventArgs e)
		{
			if (preferences.Visible)
			{
				preferences.Activate();
			}
			else
			{
				preferences.Show();
			}

			preferences.SelectGeneral();
		}

		private void menuSyncLog_Click(object sender, System.EventArgs e)
		{
			if (syncLog.Visible)
			{
				syncLog.Activate();
			}
			else
			{
				syncLog.Show();
			}
		}

		private void menuHelp_Click(object sender, System.EventArgs e)
		{
			new iFolderComponent().ShowHelp(Application.StartupPath, string.Empty);
		}

		private void menuExit_Click(object sender, System.EventArgs e)
		{
			// Disable the exit menu item so it cannot be clicked again.
			menuExit.Enabled = false;

			ShutdownTrayApp(null);
		}

		private void contextMenu1_Popup(object sender, System.EventArgs e)
		{
			// Show/hide store browser menu item based on whether or not the file is installed.
			menuStoreBrowser.Visible = File.Exists(Path.Combine(Application.StartupPath, "StoreBrowser.exe"));
			menuEventLogReader.Visible = File.Exists(Path.Combine(Application.StartupPath, "EventLogReader.exe"));
			menuSeparator1.Visible = menuTools.Visible = menuStoreBrowser.Visible | menuEventLogReader.Visible;
		}

		private void notifyIcon1_DoubleClick(object sender, System.EventArgs e)
		{
			menuProperties_Click(sender, e);
		}

		private void FormsTrayApp_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			ShutdownTrayApp(null);
		}

		private void FormsTrayApp_Load(object sender, System.EventArgs e)
		{
			if (shutdown)
			{
				this.Close();
			}
			else
			{
				try
				{
					Manager.Start();

					ifWebService = new iFolderWebService();
					ifWebService.Url = Manager.LocalServiceUrl.ToString() + "/iFolder.asmx";
					simiasWebService = new SimiasWebService();
					simiasWebService.Url = Simias.Client.Manager.LocalServiceUrl.ToString() + "/Simias.asmx";

					eventQueue = new Queue();
					workEvent = new AutoResetEvent(false);

					// Set up the event handlers to watch for create, delete, and change events.
					eventClient = new IProcEventClient(new IProcEventError(errorHandler), null);
					eventClient.Register();
					if (!eventError)
					{
						eventClient.SetEvent(IProcEventAction.AddNodeChanged, new IProcEventHandler(trayApp_nodeEventHandler));
						eventClient.SetEvent(IProcEventAction.AddNodeCreated, new IProcEventHandler(trayApp_nodeEventHandler));
						eventClient.SetEvent(IProcEventAction.AddNodeDeleted, new IProcEventHandler(trayApp_nodeEventHandler));
						eventClient.SetEvent(IProcEventAction.AddCollectionSync, new IProcEventHandler(trayApp_collectionSyncHandler));
						eventClient.SetEvent(IProcEventAction.AddFileSync, new IProcEventHandler(trayApp_fileSyncHandler));
						eventClient.SetEvent(IProcEventAction.AddNotifyMessage, new IProcEventHandler(trayApp_notifyMessageHandler));
					}

					// Instantiate the GlobalProperties dialog.
					globalProperties = new GlobalProperties(ifWebService, simiasWebService, eventClient);

					// Create the control so that we can use the delegate to write sync events to the log.
					// For some reason, the handle isn't created until it is referenced.
					globalProperties.CreateControl();
					IntPtr handle = globalProperties.Handle;

					preferences = new Preferences(ifWebService, simiasWebService);
					preferences.EnterpriseConnect += new Novell.FormsTrayApp.Preferences.EnterpriseConnectDelegate(preferences_EnterpriseConnect);
					preferences.ChangeDefaultDomain += new Novell.FormsTrayApp.Preferences.ChangeDefaultDomainDelegate(preferences_EnterpriseConnect);
					preferences.RemoveDomain += new Novell.FormsTrayApp.Preferences.RemoveDomainDelegate(preferences_RemoveDomain);
					preferences.ShutdownTrayApp += new Novell.FormsTrayApp.Preferences.ShutdownTrayAppDelegate(preferences_ShutdownTrayApp);
					preferences.UpdateDomain += new Novell.FormsTrayApp.Preferences.UpdateDomainDelegate(preferences_UpdateDomain);
					preferences.CreateControl();
					handle = preferences.Handle;

					syncLog = new SyncLog(eventClient);
					syncLog.CreateControl();
					handle = syncLog.Handle;

					// Cause the web service to start.
					ifWebService.Ping();

					try
					{
						// Pre-load the servers and accounts list.
						globalProperties.InitializeServerList();

//						DomainWeb[] domains;
//						domains = ifWebService.GetDomains();
//						foreach (DomainWeb dw in domains)

						DomainInformation[] domains;
						domains = this.simiasWebService.GetDomains(false);
						foreach(DomainInformation dw in domains)
						{
							if (dw.IsSlave)
							{
								preferences.AddDomainToList(dw);
							}

							globalProperties.AddDomainToList(dw);
						}
					}
					catch{}

					if (worker == null)
					{
						worker = new Thread(new ThreadStart(eventThreadProc));
						worker.Start();
					}

					notifyIcon1.Icon = trayIcon;
				}
				catch (Exception ex)
				{
					ShutdownTrayApp(ex);
				}
			}
		}

		private void errorHandler(ApplicationException e, object context)
		{
			eventError = true;
		}

		private void trayApp_nodeEventHandler(SimiasEventArgs args)
		{
			NodeEventArgs eventArgs = args as NodeEventArgs;

			lock (eventQueue.SyncRoot)
			{
				// Put the event in the queue
				eventQueue.Enqueue(eventArgs);

				// Signal that there are events in the queue.
				workEvent.Set();
			}
		}

		private void trayApp_collectionSyncHandler(SimiasEventArgs args)
		{
			CollectionSyncEventArgs syncEventArgs = args as CollectionSyncEventArgs;
			BeginInvoke(syncCollectionDelegate, new object[] {syncEventArgs});
		}

		private void trayApp_fileSyncHandler(SimiasEventArgs args)
		{
			FileSyncEventArgs syncEventArgs = args as FileSyncEventArgs;
			BeginInvoke(syncFileDelegate, new object[] {syncEventArgs});
		}

		private void trayApp_notifyMessageHandler(SimiasEventArgs args)
		{
			NotifyEventArgs notifyEventArgs = args as NotifyEventArgs;
			BeginInvoke(notifyMessageDelegate, new object[] {notifyEventArgs});
		}

		private void serverInfo_EnterpriseConnect(object sender, DomainConnectEventArgs e)
		{
			globalProperties.AddDomainToList(e.DomainWeb);
		}


		private void preferences_EnterpriseConnect(object sender, DomainConnectEventArgs e)
		{
			globalProperties.AddDomainToList(e.DomainWeb);
		}

		private void preferences_RemoveDomain(object sender, DomainRemoveEventArgs e)
		{
			globalProperties.RemoveDomainFromList(e.DomainWeb, e.DefaultDomainID);
		}

		private void preferences_ShutdownTrayApp(object sender, EventArgs e)
		{
			ShutdownTrayApp(null);
		}

		private void preferences_UpdateDomain(object sender, DomainConnectEventArgs e)
		{
			globalProperties.UpdateDomain(e.DomainWeb);
		}

		private void syncAnimateTimer_Tick(object sender, System.EventArgs e)
		{
			notifyIcon1.Icon = syncIcons[index];

			if (++index > (numberOfSyncIcons - 1))
			{
				index = 0;
			}
		}

		private void serverInfo_Closed(object sender, EventArgs e)
		{
			if (serverInfo != null)
			{
				// Keep track of the cancelled state.
				loginCancelled = serverInfo.Cancelled;

				bool update = serverInfo.UpdateStarted;
				serverInfo.Dispose();
				serverInfo = null;

				// If an update is in progress, shutdown the trayapp.
				if (update)
				{
					ShutdownTrayApp(null);
				}
			}
		}
		#endregion

		#region Private Methods
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FormsTrayApp));
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuTools = new System.Windows.Forms.MenuItem();
			this.menuStoreBrowser = new System.Windows.Forms.MenuItem();
			this.menuEventLogReader = new System.Windows.Forms.MenuItem();
			this.menuSeparator1 = new System.Windows.Forms.MenuItem();
			this.menuProperties = new System.Windows.Forms.MenuItem();
			this.menuAccounts = new System.Windows.Forms.MenuItem();
			this.menuPreferences = new System.Windows.Forms.MenuItem();
			this.menuSyncLog = new System.Windows.Forms.MenuItem();
			this.menuHelp = new System.Windows.Forms.MenuItem();
			this.menuItem10 = new System.Windows.Forms.MenuItem();
			this.menuExit = new System.Windows.Forms.MenuItem();
			this.syncAnimateTimer = new System.Windows.Forms.Timer(this.components);
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.ContextMenu = this.contextMenu1;
			this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
			this.notifyIcon1.Text = resources.GetString("notifyIcon1.Text");
			this.notifyIcon1.Visible = ((bool)(resources.GetObject("notifyIcon1.Visible")));
			this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuTools,
																						 this.menuSeparator1,
																						 this.menuProperties,
																						 this.menuAccounts,
																						 this.menuPreferences,
																						 this.menuSyncLog,
																						 this.menuHelp,
																						 this.menuItem10,
																						 this.menuExit});
			this.contextMenu1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("contextMenu1.RightToLeft")));
			this.contextMenu1.Popup += new System.EventHandler(this.contextMenu1_Popup);
			// 
			// menuTools
			// 
			this.menuTools.Enabled = ((bool)(resources.GetObject("menuTools.Enabled")));
			this.menuTools.Index = 0;
			this.menuTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuStoreBrowser,
																					  this.menuEventLogReader});
			this.menuTools.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuTools.Shortcut")));
			this.menuTools.ShowShortcut = ((bool)(resources.GetObject("menuTools.ShowShortcut")));
			this.menuTools.Text = resources.GetString("menuTools.Text");
			this.menuTools.Visible = ((bool)(resources.GetObject("menuTools.Visible")));
			// 
			// menuStoreBrowser
			// 
			this.menuStoreBrowser.Enabled = ((bool)(resources.GetObject("menuStoreBrowser.Enabled")));
			this.menuStoreBrowser.Index = 0;
			this.menuStoreBrowser.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuStoreBrowser.Shortcut")));
			this.menuStoreBrowser.ShowShortcut = ((bool)(resources.GetObject("menuStoreBrowser.ShowShortcut")));
			this.menuStoreBrowser.Text = resources.GetString("menuStoreBrowser.Text");
			this.menuStoreBrowser.Visible = ((bool)(resources.GetObject("menuStoreBrowser.Visible")));
			this.menuStoreBrowser.Click += new System.EventHandler(this.menuStoreBrowser_Click);
			// 
			// menuEventLogReader
			// 
			this.menuEventLogReader.Enabled = ((bool)(resources.GetObject("menuEventLogReader.Enabled")));
			this.menuEventLogReader.Index = 1;
			this.menuEventLogReader.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuEventLogReader.Shortcut")));
			this.menuEventLogReader.ShowShortcut = ((bool)(resources.GetObject("menuEventLogReader.ShowShortcut")));
			this.menuEventLogReader.Text = resources.GetString("menuEventLogReader.Text");
			this.menuEventLogReader.Visible = ((bool)(resources.GetObject("menuEventLogReader.Visible")));
			this.menuEventLogReader.Click += new System.EventHandler(this.menuEventLogReader_Click);
			// 
			// menuSeparator1
			// 
			this.menuSeparator1.Enabled = ((bool)(resources.GetObject("menuSeparator1.Enabled")));
			this.menuSeparator1.Index = 1;
			this.menuSeparator1.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuSeparator1.Shortcut")));
			this.menuSeparator1.ShowShortcut = ((bool)(resources.GetObject("menuSeparator1.ShowShortcut")));
			this.menuSeparator1.Text = resources.GetString("menuSeparator1.Text");
			this.menuSeparator1.Visible = ((bool)(resources.GetObject("menuSeparator1.Visible")));
			// 
			// menuProperties
			// 
			this.menuProperties.DefaultItem = true;
			this.menuProperties.Enabled = ((bool)(resources.GetObject("menuProperties.Enabled")));
			this.menuProperties.Index = 2;
			this.menuProperties.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuProperties.Shortcut")));
			this.menuProperties.ShowShortcut = ((bool)(resources.GetObject("menuProperties.ShowShortcut")));
			this.menuProperties.Text = resources.GetString("menuProperties.Text");
			this.menuProperties.Visible = ((bool)(resources.GetObject("menuProperties.Visible")));
			this.menuProperties.Click += new System.EventHandler(this.menuProperties_Click);
			// 
			// menuAccounts
			// 
			this.menuAccounts.Enabled = ((bool)(resources.GetObject("menuAccounts.Enabled")));
			this.menuAccounts.Index = 3;
			this.menuAccounts.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuAccounts.Shortcut")));
			this.menuAccounts.ShowShortcut = ((bool)(resources.GetObject("menuAccounts.ShowShortcut")));
			this.menuAccounts.Text = resources.GetString("menuAccounts.Text");
			this.menuAccounts.Visible = ((bool)(resources.GetObject("menuAccounts.Visible")));
			this.menuAccounts.Click += new System.EventHandler(this.menuAccounts_Click);
			// 
			// menuPreferences
			// 
			this.menuPreferences.Enabled = ((bool)(resources.GetObject("menuPreferences.Enabled")));
			this.menuPreferences.Index = 4;
			this.menuPreferences.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuPreferences.Shortcut")));
			this.menuPreferences.ShowShortcut = ((bool)(resources.GetObject("menuPreferences.ShowShortcut")));
			this.menuPreferences.Text = resources.GetString("menuPreferences.Text");
			this.menuPreferences.Visible = ((bool)(resources.GetObject("menuPreferences.Visible")));
			this.menuPreferences.Click += new System.EventHandler(this.menuPreferences_Click);
			// 
			// menuSyncLog
			// 
			this.menuSyncLog.Enabled = ((bool)(resources.GetObject("menuSyncLog.Enabled")));
			this.menuSyncLog.Index = 5;
			this.menuSyncLog.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuSyncLog.Shortcut")));
			this.menuSyncLog.ShowShortcut = ((bool)(resources.GetObject("menuSyncLog.ShowShortcut")));
			this.menuSyncLog.Text = resources.GetString("menuSyncLog.Text");
			this.menuSyncLog.Visible = ((bool)(resources.GetObject("menuSyncLog.Visible")));
			this.menuSyncLog.Click += new System.EventHandler(this.menuSyncLog_Click);
			// 
			// menuHelp
			// 
			this.menuHelp.Enabled = ((bool)(resources.GetObject("menuHelp.Enabled")));
			this.menuHelp.Index = 6;
			this.menuHelp.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuHelp.Shortcut")));
			this.menuHelp.ShowShortcut = ((bool)(resources.GetObject("menuHelp.ShowShortcut")));
			this.menuHelp.Text = resources.GetString("menuHelp.Text");
			this.menuHelp.Visible = ((bool)(resources.GetObject("menuHelp.Visible")));
			this.menuHelp.Click += new System.EventHandler(this.menuHelp_Click);
			// 
			// menuItem10
			// 
			this.menuItem10.Enabled = ((bool)(resources.GetObject("menuItem10.Enabled")));
			this.menuItem10.Index = 7;
			this.menuItem10.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuItem10.Shortcut")));
			this.menuItem10.ShowShortcut = ((bool)(resources.GetObject("menuItem10.ShowShortcut")));
			this.menuItem10.Text = resources.GetString("menuItem10.Text");
			this.menuItem10.Visible = ((bool)(resources.GetObject("menuItem10.Visible")));
			// 
			// menuExit
			// 
			this.menuExit.Enabled = ((bool)(resources.GetObject("menuExit.Enabled")));
			this.menuExit.Index = 8;
			this.menuExit.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuExit.Shortcut")));
			this.menuExit.ShowShortcut = ((bool)(resources.GetObject("menuExit.ShowShortcut")));
			this.menuExit.Text = resources.GetString("menuExit.Text");
			this.menuExit.Visible = ((bool)(resources.GetObject("menuExit.Visible")));
			this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
			// 
			// syncAnimateTimer
			// 
			this.syncAnimateTimer.Tick += new System.EventHandler(this.syncAnimateTimer_Tick);
			// 
			// FormsTrayApp
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "FormsTrayApp";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");

		}

		private void syncCollection(CollectionSyncEventArgs syncEventArgs)
		{
			switch (syncEventArgs.Action)
			{
				case Action.StartSync:
				{
					// Animate the icon.
					syncAnimateTimer.Start();
					break;
				}
				case Action.StopSync:
				{
					// Stop the icon animation.
					syncAnimateTimer.Stop();
					notifyIcon1.Icon = trayIcon;
					notifyIcon1.Text = resourceManager.GetString("notifyIcon1.Text");

					if (initialSyncCollections.Contains(syncEventArgs.ID) && syncEventArgs.Successful)
					{
						// If the collection is in the initial sync list and the sync finished successfully,
						// remove it from the list.
						initialSyncCollections.Remove(syncEventArgs.ID);
					}

					break;
				}
			}
		}

		private void syncFile(FileSyncEventArgs syncEventArgs)
		{
			try
			{
				if (syncEventArgs.SizeRemaining == syncEventArgs.SizeToSync)
				{
					notifyIcon1.Text = resourceManager.GetString("notifyIcon1.Text") + "\n" + 
						string.Format(resourceManager.GetString(syncEventArgs.Direction == Direction.Uploading ? "uploading" : "downloading") , syncEventArgs.Name);
				}
			}
			catch {}
		}

		private void createChangeEvent(iFolderWeb ifolder, iFolderUser ifolderUser, string eventData)
		{
			try
			{
				if (ifolder.HasConflicts)
				{
					if (preferences.NotifyCollisionEnabled)
					{
						NotifyIconBalloonTip balloonTip = new NotifyIconBalloonTip();

						string message = string.Format(resourceManager.GetString("collisionMessage"), ifolder.Name);

						balloonTip.ShowBalloon(
							hwnd,
							iconID,
							BalloonType.Info,
							resourceManager.GetString("actionRequiredTitle"),
							message);

						// TODO: Change the icon?
					}
				}
				else if (ifolder.State.Equals("Available") && eventData.Equals("NodeCreated"))
				{
					if (preferences.NotifyShareEnabled)
					{
						NotifyIconBalloonTip balloonTip = new NotifyIconBalloonTip();

						string message = string.Format(resourceManager.GetString("subscriptionMessage"), ifolder.Owner);

						balloonTip.ShowBalloon(
							hwnd,
							iconID,
							BalloonType.Info,
							resourceManager.GetString("actionRequiredTitle"),
							message);

						// TODO: Change the icon?
					}
				}
				else if (ifolderUser != null)
				{
					if (preferences.NotifyJoinEnabled)
					{
						NotifyIconBalloonTip balloonTip = new NotifyIconBalloonTip();

						string message = string.Format(resourceManager.GetString("newMemberMessage"), 
							(ifolderUser.FN != null) && !ifolderUser.FN.Equals(string.Empty) ? ifolderUser.FN : ifolderUser.Name, 
							ifolder.Name);
							
						balloonTip.ShowBalloon(
							hwnd,
							iconID,
							BalloonType.Info,
							resourceManager.GetString("newMemberTitle"),
							message);

						// TODO: Change the icon?
					}
				}
			}
			catch {}
		}

		private void notifyMessage(NotifyEventArgs notifyEventArgs)
		{
			switch (notifyEventArgs.EventData)
			{
				case "Domain-Up":
				{
					try
					{
						// Only display the login dialog if it hasn't been previously cancelled.
						if (!loginCancelled)
						{
							// Only display one dialog.
							if (serverInfo == null)
							{
								// See if credentials have already been set in
								// this process before showing the user the
								// login dialog.
								DomainAuthentication domainAuth =
									new DomainAuthentication(
									"iFolder",
									notifyEventArgs.Message,
									null);

								if (domainAuth.Authenticate() != 
									AuthenticationStatus.Success)
								{
									string userID;
									string credentials;

									string domainID = notifyEventArgs.Message;

									// See if there is a password saved on this domain.
									CredentialType credType = simiasWebService.GetSavedDomainCredentials(domainID, out userID, out credentials);
									if ((credType == CredentialType.Basic) && (credentials != null))
									{
										// There are credentials that were saved on the domain. Use them to authenticate.
										// If the authentication fails for any reason, pop up and ask for new credentials.
										domainAuth = new DomainAuthentication("iFolder", domainID, credentials);
										AuthenticationStatus authStatus = domainAuth.Authenticate();
										if (authStatus == AuthenticationStatus.Success)
										{
											break;
										}
										else if (authStatus == AuthenticationStatus.InvalidCredentials)
										{
											// There are bad credentials stored. Remove them.
											simiasWebService.SaveDomainCredentials(domainID, null, CredentialType.None);
										}
									}

									serverInfo = new ServerInfo(ifWebService, domainID);
									serverInfo.Closed += new EventHandler(serverInfo_Closed);
									serverInfo.Show();
								}
							}
						}
					}
					catch {}
					break;
				}
			}
		}

		private void ShutdownTrayApp(Exception ex)
		{
			Cursor.Current = Cursors.WaitCursor;

			if (ex != null)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("fatalErrorMessage"), resourceManager.GetString("fatalErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}

			try
			{
				if (eventClient != null)
				{
					eventClient.Deregister();
				}

				// Shut down the web server.
				Manager.Stop();

				if ((worker != null) && worker.IsAlive)
				{
					worker.Abort();
				}
			}
			catch
			{
				// Ignore.
			}

			Cursor.Current = Cursors.Default;

			if (notifyIcon1 != null)
			{
				notifyIcon1.Visible = false;
			}

			Application.Exit();
		}

		private void eventThreadProc()
		{
			while (true)
			{
				NodeEventArgs eventArgs = null;
				int count;
				lock (eventQueue.SyncRoot)
				{
					count = eventQueue.Count;
					if (count > 0)
					{
						eventArgs = (NodeEventArgs)eventQueue.Dequeue();
					}
				}

				iFolderWeb ifolder = null;
				iFolderUser ifolderUser = null;
				try
				{
					switch (eventArgs.EventData)
					{
						case "NodeChanged":
						{
							if (eventArgs.Type == "Collection")
							{
								ifolder = ifWebService.GetiFolder(eventArgs.Collection);
							}
							else if (eventArgs.Type.Equals("Node") && preferences.IsPOBox(eventArgs.Collection))
							{
								ifolder = ifWebService.GetiFolderInvitation(eventArgs.Collection, eventArgs.Node);
							}
							break;
						}
						case "NodeCreated":
						{
							switch (eventArgs.Type)
							{
								case "Collection":
								{
									if (eventArgs.SlaveRev == 0)
									{
										// The collection was just created, keep track of it and don't post
										// any notifications until it has successfully synced.
										initialSyncCollections.Add(eventArgs.Collection, null);
									}

									ifolder = ifWebService.GetiFolder(eventArgs.Collection);
									break;
								}
								case "Node":
								{
									if (preferences.IsPOBox(eventArgs.Collection))
									{
										ifolder = ifWebService.GetiFolderInvitation(eventArgs.Collection, eventArgs.Node);

										// If the iFolder is not Available or it exists locally, we don't need to process the event.
										if (!ifolder.State.Equals("Available") || (ifWebService.GetiFolder(ifolder.CollectionID) != null))
										{
											ifolder = null;
										}
									}
									break;
								}
								case "Member":
								{
									// TODO: This currently displays a notification for each member added to an iFolder ...
									// so when an iFolder is accepted and synced down the first time, a notification occurs for each
									// member of the iFolder.  A couple of ways to solve this:
									// 1. Keep track of the first sync and don't display any notifications until the initial sync has successfully completed.
									// 2. Queue up the added members and only display a single notification ... some sort of time interval would need to be used.
									ifolderUser = ifWebService.GetiFolderUserFromNodeID(eventArgs.Collection, eventArgs.Node);
									if ((ifolderUser != null) && !preferences.IsCurrentUser(ifolderUser.UserID))
									{
										ifolder = ifWebService.GetiFolder(eventArgs.Collection);
									}
									break;
								}
							}
							break;
						}
						case "NodeDeleted":
						{
							BeginInvoke(globalProperties.deleteEventDelegate, new object[] {eventArgs.Node});
							break;
						}
					}
				}
				catch
				{
					// Ignore.
				}
						
				if (ifolder != null)
				{
					// Don't post notifications during the initial sync of the collection.
					if (!initialSyncCollections.Contains(eventArgs.Collection))
					{
						BeginInvoke(createChangeEventDelegate, new object[] {ifolder, ifolderUser, eventArgs.EventData});
					}
					BeginInvoke(globalProperties.createChangeEventDelegate, new object[] {ifolder, eventArgs.EventData});
				}

				if (count <= 1)
				{
					// Go to sleep until there are more events in the queue.
					workEvent.WaitOne();
				}
			}
		}
		#endregion

/*		private const int WM_MYID = 0xbd1;

		[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
		protected override void WndProc(ref System.Windows.Forms.Message m) 
		{
			Debug.WriteLine("Message = " + m.Msg.ToString());
			// Listen for operating system messages.
			switch (m.Msg)
			{
				// TODO: need to get this message to fire from the balloon window.
				case WM_MYID:
					break;                
			}
			base.WndProc(ref m);
		}*/
	}
}
