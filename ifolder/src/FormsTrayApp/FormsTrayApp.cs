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
using Novell.iFolderCom;
using Novell.Win32Util;
using CustomUIControls;
using Simias;
using Simias.Event;
using Simias.Storage;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for FormsTrayApp.
	/// </summary>
	public class FormsTrayApp : Form
	{
        #region Class Members
		private System.ComponentModel.IContainer components;

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

//		private bool noTray = false;

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
		//private EventSubscriber subscriber;
		private IProcEventClient eventClient;
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
			if (args.Length == 1 && args[0].Equals("-configure"))
			{
				// Set the run key in the registry.
				GlobalProperties.SetRunValue(true);
			}
			else
			{
				// Check for currently running instance.
				Process[] iFolderProcess = Process.GetProcessesByName("iFolderApp");
				if (iFolderProcess.Length == 1)
				{
					Application.Run(new FormsTrayApp());
				}
				else
				{
					// TODO: Localize
					MessageBox.Show("There is already an instance of iFolder running.");
				}
			}
		}

		/// <summary>
		/// Constructs a FormsTrayApp object.
		/// </summary>
		public FormsTrayApp()
		{
			InitializeComponent();

			this.components = new System.ComponentModel.Container();

			// Set up how the form should be displayed.
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Text = "iFolder Services";

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
			catch (Exception e)
			{
//				noTray = true;
			}		

			Type t = notifyIcon1.GetType();
			hwnd = ((NativeWindow)t.GetField("window",System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(notifyIcon1)).Handle;
			iconID = (int)t.GetField("id",System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(notifyIcon1);

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
			GlobalProperties globalProperties = new GlobalProperties(ifWebService, eventClient);
			globalProperties.ShowDialog();
		}

		private void menuHelp_Click(object sender, System.EventArgs e)
		{
			// TODO - need to use locale-specific path
			string helpPath = Path.Combine(Application.StartupPath, @"help\en\doc\user\data\front.html");

			try
			{
				Process.Start(helpPath);
			}
			catch (Exception ex)
			{
				// TODO: Localize
				MessageBox.Show("Unable to open help file: \n" + helpPath, "Help File Not Found");
			}
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
					iFolderSettings ifolderSettings = ifWebService.GetSettings();
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
			try
			{
				simiasProc = new Process();
				ProcessStartInfo simiasStartInfo = new ProcessStartInfo(Path.Combine(Application.StartupPath, "simias.cmd"));
				simiasStartInfo.CreateNoWindow = true;
				simiasStartInfo.UseShellExecute = false;
				simiasStartInfo.RedirectStandardInput = true;
				simiasProc.StartInfo = simiasStartInfo;
				simiasProc.Start();

				ifWebService = new iFolderWebService();
				ifWebService.Ping();
				//iFolderManager.CreateDefaultExclusions(config);

				synkEvent = new AutoResetEvent(false);

				animateDelegate = new AnimateDelegate(AnimateIcon);

				// Start the icon animation worker thread.
				if (workerThread == null)
				{
					Console.WriteLine("Creating worker thread");
					workerThread = new Thread(new ThreadStart(AnimateWorker));
					Console.WriteLine("Starting worker thread");
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
				}

/*				subscriber = new EventSubscriber();
				subscriber.NodeChanged += new NodeEventHandler(subscriber_NodeChanged);
				subscriber.NodeCreated += new NodeEventHandler(subscriber_NodeCreated);
				subscriber.NodeDeleted += new NodeEventHandler(subscriber_NodeDeleted);
*/			}
			catch (WebException ex)
			{
				ShutdownTrayApp(ex);
			}
			catch (Exception ex)
			{
				ShutdownTrayApp(ex);
			}
		}

//		private void serviceManager_Shutdown(ShutdownEventArgs args)
//		{
//			ShutdownTrayApp();
//		}

		private void errorHandler(SimiasException e, object context)
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

						// TODO: Localize
						balloonTip.ShowBalloon(
							hwnd,
							iconID,
							BalloonType.Info,
							"Action Required",
							"A collision has been detected in iFolder:\n" + ifolder.Name);

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
				if (eventArgs.Type != "Collection")
				{
					iFolder ifolder = ifWebService.GetSubscription(eventArgs.Collection, eventArgs.Node);

					// If the iFolder is available and doesn't exist locally, post a notification.
					if (ifolder != null)
					{
						if (ifolder.State.Equals("Available") && (ifWebService.GetiFolder(ifolder.CollectionID) == null))
						{
							// TODO: check this...
							//this.Text = "A message needs your attention";

							NotifyIconBalloonTip balloonTip = new NotifyIconBalloonTip();

							// TODO: Localize
							balloonTip.ShowBalloon(
								hwnd,
								iconID,
								BalloonType.Info,
								"Action Required",
								"A subscription has just been received from " + ifolder.Owner);

							// TODO: Change the icon?
						}
					}
					else
					{
						iFolderUser ifolderUser = ifWebService.GetiFolderUserFromNodeID(eventArgs.Collection, eventArgs.Node);
						if (ifolderUser != null)
						{
							ifolder = ifWebService.GetiFolder(eventArgs.Collection);

							NotifyIconBalloonTip balloonTip = new NotifyIconBalloonTip();

							// TODO: Localize
							balloonTip.ShowBalloon(
								hwnd,
								iconID,
								BalloonType.Info,
								"New Membership",
								ifolderUser.Name + " has just joined iFolder " + ifolder.Name);

							// TODO: Change the icon?
						}
					}
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

/*		private void subscriber_NodeCreated(NodeEventArgs args)
		{
			try
			{
				POBox poBox = POBox.GetPOBoxByID(Store.GetStore(), args.Collection);
				if (poBox != null)
				{
					Node node = poBox.GetNodeByID(args.ID);
					if (node != null)
					{
						Subscription sub = new Subscription(node);

						if ((sub.SubscriptionState == SubscriptionStates.Received) &&
							(!poBox.Domain.Equals(Domain.WorkGroupDomainID)))
						{
							animateIcon = true;

							// TODO: check this...
							this.Text = "A message needs your attention";

							NotifyIconBalloonTip balloonTip = new NotifyIconBalloonTip();
							balloonTip.ShowBalloon(
								hwnd,
								iconID,
								BalloonType.Info,
								"Action Required",
								"A subscription has just been received from " + sub.FromName);

							synkEvent.Set();
						}
					}
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				//logger.Debug(ex, "OnNodeCreated");
			}
		}

		private void subscriber_NodeDeleted(NodeEventArgs args)
		{
			// TODO: implement this if needed.
		}

		private void subscriber_NodeChanged(NodeEventArgs args)
		{
			POBox poBox = POBox.GetPOBoxByID(Store.GetStore(), args.Collection);
			if (poBox != null)
			{
				Node node = poBox.GetNodeByID(args.ID);
				if (node != null)
				{
					Subscription sub = new Subscription(node);

					if (sub.SubscriptionState == SubscriptionStates.Pending)
					{
						animateIcon = true;

						// TODO: this doesn't work.
						this.Text = "A message needs your attention";

						NotifyIconBalloonTip balloonTip = new NotifyIconBalloonTip();
						balloonTip.ShowBalloon(
							hwnd,
							iconID,
							BalloonType.Info,
							"Action Required",
							"A subscription from " + sub.ToName + " needs your approval.");
						synkEvent.Set();
					}
				}
			}
			else
			{
				Collection c = Store.GetStore().GetCollectionByID(args.Collection);
				if (c != null)
				{
					if (c.HasCollisions() && c.IsType(c, typeof(iFolder).Name))
					{
						animateIcon = true;

						NotifyIconBalloonTip balloonTip = new NotifyIconBalloonTip();
						balloonTip.ShowBalloon(
							hwnd,
							iconID,
							BalloonType.Info,
							"Action Required",
							"A collision has been detected in iFolder:\n" + c.Name);
						synkEvent.Set();
					}
				}
			}
		}*/
		#endregion

		#region Private Methods
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
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
			this.notifyIcon1.Text = "iFolder Services";
			this.notifyIcon1.Visible = true;
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
			this.contextMenu1.Popup += new System.EventHandler(this.contextMenu1_Popup);
			// 
			// menuTools
			// 
			this.menuTools.Index = 0;
			this.menuTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuStoreBrowser,
																					  this.menuEventLogReader});
			this.menuTools.Text = "Tools";
			this.menuTools.Visible = false;
			// 
			// menuStoreBrowser
			// 
			this.menuStoreBrowser.Index = 0;
			this.menuStoreBrowser.Text = "Store Browser";
			this.menuStoreBrowser.Visible = false;
			this.menuStoreBrowser.Click += new System.EventHandler(this.menuStoreBrowser_Click);
			// 
			// menuEventLogReader
			// 
			this.menuEventLogReader.Index = 1;
			this.menuEventLogReader.Text = "Event Log Reader";
			this.menuEventLogReader.Visible = false;
			this.menuEventLogReader.Click += new System.EventHandler(this.menuEventLogReader_Click);
			// 
			// menuSeparator1
			// 
			this.menuSeparator1.Index = 1;
			this.menuSeparator1.Text = "-";
			this.menuSeparator1.Visible = false;
			// 
			// menuProperties
			// 
			this.menuProperties.DefaultItem = true;
			this.menuProperties.Index = 2;
			this.menuProperties.Text = "My iFolders...";
			this.menuProperties.Click += new System.EventHandler(this.menuProperties_Click);
			// 
			// menuJoin
			// 
			this.menuJoin.Index = 3;
			this.menuJoin.Text = "Join Enterprise Server";
			this.menuJoin.Click += new System.EventHandler(this.menuJoin_Click);
			// 
			// menuHelp
			// 
			this.menuHelp.Index = 4;
			this.menuHelp.Text = "Help...";
			this.menuHelp.Click += new System.EventHandler(this.menuHelp_Click);
			// 
			// menuItem10
			// 
			this.menuItem10.Index = 5;
			this.menuItem10.Text = "-";
			// 
			// menuExit
			// 
			this.menuExit.Index = 6;
			this.menuExit.Text = "Exit";
			this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
			// 
			// FormsTrayApp
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Name = "FormsTrayApp";

		}

		private void ShutdownTrayApp(Exception ex)
		{
			Cursor.Current = Cursors.WaitCursor;

			if (ex != null)
			{
				// TODO: Localize.
				MessageBox.Show("A fatal error was encountered during iFolder initialization.\n\n" + ex.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}

			try
			{
				if (eventClient != null)
				{
					eventClient.Deregister();
				}

				if ((simiasProc != null) && !simiasProc.HasExited)
				{
					StreamWriter simiasStdIn = simiasProc.StandardInput;
					simiasStdIn.Write(simiasStdIn.NewLine);
				}

				// TODO: shutdown gracefully
				Process[] simiasAppProcess = Process.GetProcessesByName("SimiasApp");
				if (simiasAppProcess.Length == 1)
				{
					simiasAppProcess[0].Kill();
				}

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
			notifyIcon1.Visible = false;
			Application.Exit();
		}

		private void CleanupTrayApp(Exception ex)
		{
			// TODO: Localize.
			MessageBox.Show("A fatal error was encountered during iFolder initialization.\n\n" + ex.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			notifyIcon1.Visible = false;
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
