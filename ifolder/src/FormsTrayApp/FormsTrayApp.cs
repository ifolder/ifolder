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
using Novell.iFolderCom;
using Novell.Win32Util;
using CustomUIControls;
using Simias.Client;
using Simias.Event;
using Simias.Storage;
using Simias.Sync;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for FormsTrayApp.
	/// </summary>
	public class FormsTrayApp : Form
	{
        #region Class Members
		private System.ComponentModel.IContainer components;
		private System.Resources.ResourceManager resourceManager;
		private bool shutdown = false;

		private Thread workerThread = null;

		private Icon trayIcon;
		private Icon startupIcon;
        private const int numberOfIcons = 2;//10;
		private Icon[] uploadIcons = new Icon[numberOfIcons];

		private bool animateIcon = false;

		/// <summary>
		/// Event used to animate the notify icon.
		/// </summary>
		protected AutoResetEvent synkEvent = null;

		private delegate void AnimateDelegate(int index);
		private AnimateDelegate animateDelegate;
		private System.Windows.Forms.MenuItem menuItem10;
		private System.Windows.Forms.MenuItem menuSeparator1;
		private System.Windows.Forms.MenuItem menuProperties;
		private System.Windows.Forms.MenuItem menuHelp;
		private System.Windows.Forms.MenuItem menuExit;
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuEventLogReader;
		//private Configuration config;
		private Process simiasProc;
		private iFolderWebService ifWebService;
		private iFolderSettings ifolderSettings = null;
		private IProcEventClient eventClient;
		private GlobalProperties globalProperties;
		private bool eventError = false;
		private IntPtr hwnd;
		private System.Windows.Forms.MenuItem menuJoin;
		private System.Windows.Forms.MenuItem menuStoreBrowser;
		private System.Windows.Forms.MenuItem menuTools;
		private int iconID;
		//private const int waitTime = 3000;
		#endregion

		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length == 1 && args[0].Equals("-checkautorun"))
			{
				// Check if auto-run is enabled.
				if (!GlobalProperties.IsRunEnabled())
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
			resourceManager = new System.Resources.ResourceManager(typeof(FormsTrayApp));

			// Check for currently running instance.
			Process[] iFolderProcess = Process.GetProcessesByName("iFolderApp");
			if (iFolderProcess.Length != 1)
			{
				MessageBox.Show(resourceManager.GetString("iFolderRunning"));
				shutdown = true;
			}
			else
			{
				InitializeComponent();

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
					uploadIcons[0] = new Icon(trayIcon, trayIcon.Size);
					uploadIcons[1] = new Icon(Path.Combine(basePath, "ifolder_message.ico"));
	//				for (int i = 0; i < numberOfIcons; i++)
	//				{
	//					string upIcon = string.Format(Path.Combine(basePath, "ifolder_sync{0}.ico"), i+1);
	//					uploadIcons[i] = new Icon(upIcon);
	//				}
			
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

		#region Event Handlers
		private void menuStoreBrowser_Click(object sender, System.EventArgs e)
		{
			Process.Start(Path.Combine(Application.StartupPath, "StoreBrowser.exe"));
		}

		private void menuEventLogReader_Click(object sender, System.EventArgs e)
		{
			Process.Start(Path.Combine(Application.StartupPath, "EventLogReader.exe"));
		}

		private void menuJoin_Click(object sender, System.EventArgs e)
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

			ServerInfo serverInfo = new ServerInfo(ifWebService);
			serverInfo.ShowDialog();
		}

		private void menuProperties_Click(object sender, System.EventArgs e)
		{
			if (!globalProperties.Visible)
			{
				globalProperties.ShowDialog();
			}
		}

		private void menuHelp_Click(object sender, System.EventArgs e)
		{
			new iFolderComponent().ShowHelp(Application.StartupPath);
		}

		private void menuExit_Click(object sender, System.EventArgs e)
		{
			// Disable the exit menu item so it cannot be clicked again.
			menuExit.Enabled = false;

			ShutdownTrayApp(null);
		}

		private void messages_MessagesServiced(object sender, EventArgs e)
		{
			// TODO: we may need to check that no conflicts exist before we stop animating the icon.
			this.animateIcon = false;
		}

		private void contextMenu1_Popup(object sender, System.EventArgs e)
		{
			// Show/hide store browser menu item based on whether or not the file is installed.
			menuStoreBrowser.Visible = File.Exists(Path.Combine(Application.StartupPath, "StoreBrowser.exe"));
			menuEventLogReader.Visible = File.Exists(Path.Combine(Application.StartupPath, "EventLogReader.exe"));
			menuSeparator1.Visible = menuTools.Visible = menuStoreBrowser.Visible | menuEventLogReader.Visible;

			// Check to see if we have already connected to an enterprise server.
			try
			{
				// If the join menu is already hidden, we don't need to check.
				if (menuJoin.Visible)
				{
					ifolderSettings = ifWebService.GetSettings();
					menuJoin.Visible = !ifolderSettings.HaveEnterprise;
				}
			}
			catch
			{
				// Ignore.
			}
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
					Simias.Service.Manager.Start();

					ifWebService = new iFolderWebService();
					ifWebService.Url = Simias.Service.Manager.LocalServiceUrl.ToString() + "/iFolder.asmx";
					ifWebService.Ping();
					//iFolderManager.CreateDefaultExclusions(config);

					synkEvent = new AutoResetEvent(false);

					animateDelegate = new AnimateDelegate(AnimateIcon);

					// Start the icon animation worker thread.
					if (workerThread == null)
					{
						workerThread = new Thread(new ThreadStart(AnimateWorker));
						workerThread.Start();
					}

					notifyIcon1.Icon = trayIcon;

					// Set up the event handlers to watch for create, delete, and change events.
					eventClient = new IProcEventClient(new IProcEventError(errorHandler), null);
					eventClient.Register();
					if (!eventError)
					{
						eventClient.SetEvent(IProcEventAction.AddNodeChanged, new IProcEventHandler(trayApp_nodeChangeHandler));
						eventClient.SetEvent(IProcEventAction.AddNodeCreated, new IProcEventHandler(trayApp_nodeCreateHandler));
						eventClient.SetEvent(IProcEventAction.AddNodeDeleted, new IProcEventHandler(trayApp_nodeDeleteHandler));
						eventClient.SetEvent(IProcEventAction.AddCollectionSync, new IProcEventHandler(trayApp_collectionSyncHandler));
					}

					globalProperties = new GlobalProperties(ifWebService, eventClient);
				}
				catch (WebException ex)
				{
					ShutdownTrayApp(ex);
				}
				catch (Exception ex)
				{
					ShutdownTrayApp(ex);
				}
			}
		}

//		private void serviceManager_Shutdown(ShutdownEventArgs args)
//		{
//			ShutdownTrayApp();
//		}

		private void errorHandler(ApplicationException e, object context)
		{
			eventError = true;
		}

		private void trayApp_nodeChangeHandler(SimiasEventArgs args)
		{
			NodeEventArgs eventArgs = args as NodeEventArgs;

			if (eventArgs.Type == "Collection")
			{
				iFolder ifolder = ifWebService.GetiFolder(eventArgs.Collection);
				if (ifolder != null)
				{
					if (ifolder.HasConflicts)
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
			}
		}

		private void trayApp_nodeCreateHandler(SimiasEventArgs args)
		{
			NodeEventArgs eventArgs = args as NodeEventArgs;

			try
			{
				switch (eventArgs.Type)
				{
					case "Node":
					{
						iFolder ifolder = ifWebService.GetSubscription(eventArgs.Collection, eventArgs.Node);

						// If the iFolder is available and doesn't exist locally, post a notification.
						if ((ifolder != null) && ifolder.State.Equals("Available") && (ifWebService.GetiFolder(ifolder.CollectionID) == null))
						{
							// TODO: check this...
							//this.Text = "A message needs your attention";

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
						break;
					}
					case "Member":
					{
						if (ifolderSettings == null)
						{
							ifolderSettings = ifWebService.GetSettings();
						}

						// TODO: This currently displays a notification for each member added to an iFolder ...
						// so when an iFolder is accepted and synced down the first time, a notification occurs for each
						// member of the iFolder.  A couple of ways to solve this:
						// 1. Keep track of the first sync and don't display any notifications until the initial sync has successfully completed.
						// 2. Queue up the added members and only display a single notification ... some sort of time interval would need to be used.
						iFolderUser ifolderUser = ifWebService.GetiFolderUserFromNodeID(eventArgs.Collection, eventArgs.Node);
						if ((ifolderUser != null) && (!ifolderUser.UserID.Equals(ifolderSettings.CurrentUserID)))
						{
							iFolder ifolder = ifWebService.GetiFolder(eventArgs.Collection);

							NotifyIconBalloonTip balloonTip = new NotifyIconBalloonTip();

							string message = string.Format(resourceManager.GetString("newMemberMessage"), ifolderUser.Name, ifolder.Name);
							
							balloonTip.ShowBalloon(
								hwnd,
								iconID,
								BalloonType.Info,
								resourceManager.GetString("newMemberTitle"),
								message);

							// TODO: Change the icon?
						}
						break;
					}
					default:
						break;
				}
			}
			catch
			{
				// Ignore.
			}
		}

		private void trayApp_nodeDeleteHandler(SimiasEventArgs args)
		{
			NodeEventArgs eventArgs = args as NodeEventArgs;

			// TODO: implement if needed.
		}

		private void trayApp_collectionSyncHandler(SimiasEventArgs args)
		{
			CollectionSyncEventArgs syncEventArgs = args as CollectionSyncEventArgs;

			switch (syncEventArgs.Action)
			{
				case Action.StartSync:
				{
					// TODO: start icon animation.
					break;
				}
				case Action.StopSync:
				{
					// TODO: stop icon animation
					break;
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
			this.menuJoin = new System.Windows.Forms.MenuItem();
			this.menuHelp = new System.Windows.Forms.MenuItem();
			this.menuItem10 = new System.Windows.Forms.MenuItem();
			this.menuExit = new System.Windows.Forms.MenuItem();
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
																						 this.menuJoin,
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
			// menuJoin
			// 
			this.menuJoin.Enabled = ((bool)(resources.GetObject("menuJoin.Enabled")));
			this.menuJoin.Index = 3;
			this.menuJoin.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuJoin.Shortcut")));
			this.menuJoin.ShowShortcut = ((bool)(resources.GetObject("menuJoin.ShowShortcut")));
			this.menuJoin.Text = resources.GetString("menuJoin.Text");
			this.menuJoin.Visible = ((bool)(resources.GetObject("menuJoin.Visible")));
			this.menuJoin.Click += new System.EventHandler(this.menuJoin_Click);
			// 
			// menuHelp
			// 
			this.menuHelp.Enabled = ((bool)(resources.GetObject("menuHelp.Enabled")));
			this.menuHelp.Index = 4;
			this.menuHelp.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuHelp.Shortcut")));
			this.menuHelp.ShowShortcut = ((bool)(resources.GetObject("menuHelp.ShowShortcut")));
			this.menuHelp.Text = resources.GetString("menuHelp.Text");
			this.menuHelp.Visible = ((bool)(resources.GetObject("menuHelp.Visible")));
			this.menuHelp.Click += new System.EventHandler(this.menuHelp_Click);
			// 
			// menuItem10
			// 
			this.menuItem10.Enabled = ((bool)(resources.GetObject("menuItem10.Enabled")));
			this.menuItem10.Index = 5;
			this.menuItem10.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuItem10.Shortcut")));
			this.menuItem10.ShowShortcut = ((bool)(resources.GetObject("menuItem10.ShowShortcut")));
			this.menuItem10.Text = resources.GetString("menuItem10.Text");
			this.menuItem10.Visible = ((bool)(resources.GetObject("menuItem10.Visible")));
			// 
			// menuExit
			// 
			this.menuExit.Enabled = ((bool)(resources.GetObject("menuExit.Enabled")));
			this.menuExit.Index = 6;
			this.menuExit.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuExit.Shortcut")));
			this.menuExit.ShowShortcut = ((bool)(resources.GetObject("menuExit.ShowShortcut")));
			this.menuExit.Text = resources.GetString("menuExit.Text");
			this.menuExit.Visible = ((bool)(resources.GetObject("menuExit.Visible")));
			this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
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

		private void ShutdownTrayApp(Exception ex)
		{
			Cursor.Current = Cursors.WaitCursor;

			if (ex != null)
			{
				MessageBox.Show(resourceManager.GetString("fatalErrorMessage") + "\n\n" + ex.Message, resourceManager.GetString("fatalErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}

			try
			{
				if (eventClient != null)
				{
					eventClient.Deregister();
				}

				// Shut down the web server.
				Simias.Service.Manager.Stop();

				if ((workerThread != null) && workerThread.IsAlive)
				{
					workerThread.Abort();
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

		private void AnimateIcon(int index)
		{
			if (animateIcon)
			{
				notifyIcon1.Icon = uploadIcons[index];
			}
			else
			{
				notifyIcon1.Icon = trayIcon;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// The worker thread for animating the notify icon.
		/// </summary>
		public void AnimateWorker()
		{
			int i = 0;
			while (true)
			{
				IAsyncResult r = BeginInvoke(animateDelegate, new object[] {i});
				if (animateIcon)
				{
					Thread.Sleep(1000);
				}
				else
				{
					synkEvent.WaitOne();
				}

				if (++i > (numberOfIcons - 1))
				{
					i = 0;
				}
			}
		}
		#endregion

		private const int WM_MYID = 0xbd1;

		/// <summary>
		/// Process messages in the Windows message loop.
		/// </summary>
		/// <param name="m"></param>
		/*[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
		protected override void WndProc(ref System.Windows.Forms.Message m) 
		{
//			Debug.WriteLine("Message = " + m.Msg.ToString());
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
