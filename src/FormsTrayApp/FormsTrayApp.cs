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
using Simias.Sync;
using Novell.iFolder;
using Novell.iFolder.iFolderCom;
using Novell.iFolder.Win32Util;
using Simias;
using Simias.Event;
using Simias.Service;

namespace Novell.iFolder.FormsTrayApp
{
	/// <summary>
	/// Summary description for FormsTrayApp.
	/// </summary>
	public class FormsTrayApp : Form
	{
        #region Class Members
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(FormsTrayApp));
		private System.ComponentModel.IContainer components;

		private Thread workerThread = null;

		private Icon trayIcon;
        private const int numberOfIcons = 10;
		private Icon[] uploadIcons = new Icon[numberOfIcons];

		private SyncManagerStates syncState = SyncManagerStates.Idle;
		protected AutoResetEvent synkEvent = null;

//		private MyTraceForm traceForm;

		private bool noTray = false;

		private delegate void AnimateDelegate(int index);
		private AnimateDelegate animateDelegate;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem10;
		private System.Windows.Forms.MenuItem menuStoreBrowser;
		private System.Windows.Forms.MenuItem menuSeparator1;
		private System.Windows.Forms.MenuItem menuInvitationWizard;
		private System.Windows.Forms.MenuItem menuAddressBook;
		private System.Windows.Forms.MenuItem menuTraceWindow;
		private System.Windows.Forms.MenuItem menuProperties;
		private System.Windows.Forms.MenuItem menuHelp;
		private System.Windows.Forms.MenuItem menuExit;
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuConflictResolver;

		private Manager serviceManager;
		private System.Windows.Forms.MenuItem menuMyiFolders;
		private System.Windows.Forms.MenuItem menuPOBox;
		private System.Windows.Forms.MenuItem menuConnect;
		private System.Windows.Forms.MenuItem menuConnectServer;
		private iFolderManager ifManager;
		private Configuration config;
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
					MessageBox.Show("There is already an instance of iFolder running.");
				}
			}
		}

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
				for (int i = 0; i < numberOfIcons; i++)
				{
					string upIcon = string.Format(Path.Combine(basePath, "ifolder_sync{0}.ico"), i+1);
					uploadIcons[i] = new Icon(upIcon);
				}
			
				notifyIcon1.Icon = trayIcon;
				this.ShowInTaskbar = false;
				this.WindowState = FormWindowState.Minimized;
				//this.Hide();

				Win32Window win32Window = new Win32Window();
				win32Window.Window = this.Handle;
				win32Window.MakeToolWindow();
			}
			catch (Exception e)
			{
				logger.Debug(e, "Loading icons");
				noTray = true;
			}		

			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormsTrayApp_Closing);
			this.Load += new System.EventHandler(FormsTrayApp_Load);
		}

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

		private void menuConnectServer_Click(object sender, System.EventArgs e)
		{
			ServerInfo serverInfo = new ServerInfo(this.config);
			if (DialogResult.OK != serverInfo.ShowDialog())
			{
				// TODO: post a message.
			}
		}

		private void menuPOBox_Click(object sender, System.EventArgs e)
		{
			MessageForm messages = new MessageForm(config);
			messages.ShowDialog();
		}

		private void menuInvitationWizard_Click(object sender, System.EventArgs e)
		{
// Check for currently running instance and switch to it.
//			Win32Window win32Window = Win32Window.FindWindow(null, "InvitationWizard");
//			if (win32Window != null)
//			{
//				win32Window.BringWindowToTop();
//			}
//			else
			{
				Process.Start(Path.Combine(Application.StartupPath, "InvitationWizard.exe"));
			}
		}

		private void menuAddressBook_Click(object sender, System.EventArgs e)
		{
// Check for currently running instance and switch to it.
//			Win32Window win32Window = Win32Window.FindWindow(null, "FormsAddrBook");
//			if (win32Window != null)
//			{
//				win32Window.BringWindowToTop();
//			}
//			else
			{
				Process.Start(Path.Combine(Application.StartupPath, "ContactBrowser.exe"));
			}
		}

		private void menuConflictResolver_Click(object sender, System.EventArgs e)
		{
			Win32Window win32Window = Win32Window.FindWindow(null, "Conflict Resolver");
			if (win32Window != null)
			{
				win32Window.BringWindowToTop();
			}
			else
			{
				ConflictResolver conflictResolver = new ConflictResolver();
				conflictResolver.Show();
			}
		}

		private void menuTraceWindow_Click(object sender, System.EventArgs e)
		{
			/*			menuTraceWindow.Checked = !menuTraceWindow.Checked;
						if (menuTraceWindow.Checked)
						{
							// Display the trace window.
							this.traceForm.Show();
						}
						else
						{
							this.traceForm.Hide();
						}*/
		}

		private void menuProperties_Click(object sender, System.EventArgs e)
		{
			GlobalProperties globalProperties = new GlobalProperties(config);
			globalProperties.ServiceManager = this.serviceManager;
			globalProperties.IFManager = this.ifManager;
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
				logger.Debug(ex, "Opening help");
				MessageBox.Show("Unable to open help file: \n" + helpPath, "Help File Not Found");
			}
		}

		private void menuExit_Click(object sender, System.EventArgs e)
		{
			// Disable the exit menu item so it cannot be clicked again.
			menuExit.Enabled = false;

			ShutdownTrayApp();
		}

		private void contextMenu1_Popup(object sender, System.EventArgs e)
		{
			// Show/hide store browser menu item based on whether or not the file is installed.
			this.menuStoreBrowser.Visible = this.menuSeparator1.Visible = File.Exists(Path.Combine(Application.StartupPath, "StoreBrowser.exe"));
		}

		private void notifyIcon1_DoubleClick(object sender, System.EventArgs e)
		{
			menuProperties_Click(sender, e);
		}

		private void FormsTrayApp_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			ShutdownTrayApp();
		}

/*		private void traceForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!this.traceForm.Shutdown)
			{
				// If the form didn't receive a shutdown notification,
				// cancel the Closing event.
				e.Cancel = true;

				// Hide the trace window.
				this.traceForm.Hide();
			}
		}*/

		private void FormsTrayApp_Load(object sender, System.EventArgs e)
		{
			try
			{
				config = Configuration.GetConfiguration();

				SimiasLogManager.Configure(config);
			
				SyncProperties props = new SyncProperties(config);
				props.LogicFactory = typeof(SynkerA);
			
				serviceManager = new Manager(config);
				serviceManager.StartServices();

				// Wait for the services to start.
				while (!serviceManager.ServiceStarted)
				{
					Application.DoEvents();
					Thread.Sleep(100);
				}

				serviceManager.Shutdown += new ShutdownEventHandler(serviceManager_Shutdown);

				// Now that the services are started, enable the exit menu item.
				menuExit.Enabled = true;

				ifManager = iFolderManager.Connect();

				synkEvent = new AutoResetEvent(false);

//				animateDelegate = new AnimateDelegate(AnimateIcon);

				// Start the icon animation worker thread.
				if (workerThread == null)
				{
					Console.WriteLine("Creating worker thread");
					workerThread = new Thread(new ThreadStart(AnimateWorker));
					Console.WriteLine("Starting worker thread");
					workerThread.Start();
				}

				// Create the trace window ... initially hidden.
/*				traceForm = new MyTraceForm();
				traceForm.Closing += new System.ComponentModel.CancelEventHandler(traceForm_Closing);

				// Trace messages will immediately be written, so we need the window handle to be created.
				traceForm.CreateControl();

				// For some reason the handle isn't really created until it is referenced.
				IntPtr handle = traceForm.Handle;

				// Enable the tracer menu item.
				this.menuItemTracer.Enabled = true;
*/			}
			catch (SimiasException ex)
			{
				ex.LogFatal();
				CleanupTrayApp();
			}
			catch (Exception ex)
			{
				logger.Fatal(ex, "Initializing tray app");
				CleanupTrayApp();
			}
		}

		private void syncManager_ChangedState(SyncManagerStates state)
		{
			syncState = state;

			if (state == SyncManagerStates.Active)
			{
				synkEvent.Set();
			}
		}

		private void serviceManager_Shutdown(ShutdownEventArgs args)
		{
			ShutdownTrayApp();
		}
		#endregion

		#region Private Methods
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuStoreBrowser = new System.Windows.Forms.MenuItem();
			this.menuSeparator1 = new System.Windows.Forms.MenuItem();
			this.menuMyiFolders = new System.Windows.Forms.MenuItem();
			this.menuConnect = new System.Windows.Forms.MenuItem();
			this.menuConnectServer = new System.Windows.Forms.MenuItem();
			this.menuPOBox = new System.Windows.Forms.MenuItem();
			this.menuInvitationWizard = new System.Windows.Forms.MenuItem();
			this.menuAddressBook = new System.Windows.Forms.MenuItem();
			this.menuConflictResolver = new System.Windows.Forms.MenuItem();
			this.menuTraceWindow = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuProperties = new System.Windows.Forms.MenuItem();
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
																						 this.menuStoreBrowser,
																						 this.menuSeparator1,
																						 this.menuMyiFolders,
																						 this.menuConnect,
																						 this.menuPOBox,
																						 this.menuInvitationWizard,
																						 this.menuAddressBook,
																						 this.menuConflictResolver,
																						 this.menuTraceWindow,
																						 this.menuItem7,
																						 this.menuProperties,
																						 this.menuHelp,
																						 this.menuItem10,
																						 this.menuExit});
			this.contextMenu1.Popup += new System.EventHandler(this.contextMenu1_Popup);
			// 
			// menuStoreBrowser
			// 
			this.menuStoreBrowser.Index = 0;
			this.menuStoreBrowser.Text = "Store Browser";
			this.menuStoreBrowser.Visible = false;
			this.menuStoreBrowser.Click += new System.EventHandler(this.menuStoreBrowser_Click);
			// 
			// menuSeparator1
			// 
			this.menuSeparator1.Index = 1;
			this.menuSeparator1.Text = "-";
			this.menuSeparator1.Visible = false;
			// 
			// menuMyiFolders
			// 
			this.menuMyiFolders.Index = 2;
			this.menuMyiFolders.Text = "My iFolders";
			this.menuMyiFolders.Visible = false;
			// 
			// menuConnect
			// 
			this.menuConnect.Index = 3;
			this.menuConnect.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this.menuConnectServer});
			this.menuConnect.Text = "Connect";
			// 
			// menuConnectServer
			// 
			this.menuConnectServer.Index = 0;
			this.menuConnectServer.Text = "Enterprise server...";
			this.menuConnectServer.Click += new System.EventHandler(this.menuConnectServer_Click);
			// 
			// menuPOBox
			// 
			this.menuPOBox.Index = 4;
			this.menuPOBox.Text = "Subscriptions...";
			this.menuPOBox.Click += new System.EventHandler(this.menuPOBox_Click);
			// 
			// menuInvitationWizard
			// 
			this.menuInvitationWizard.Index = 5;
			this.menuInvitationWizard.Text = "Invitation Wizard...";
			this.menuInvitationWizard.Click += new System.EventHandler(this.menuInvitationWizard_Click);
			// 
			// menuAddressBook
			// 
			this.menuAddressBook.Index = 6;
			this.menuAddressBook.Text = "Address Book...";
			this.menuAddressBook.Click += new System.EventHandler(this.menuAddressBook_Click);
			// 
			// menuConflictResolver
			// 
			this.menuConflictResolver.Index = 7;
			this.menuConflictResolver.Text = "Conflict Resolver...";
			this.menuConflictResolver.Click += new System.EventHandler(this.menuConflictResolver_Click);
			// 
			// menuTraceWindow
			// 
			this.menuTraceWindow.Enabled = false;
			this.menuTraceWindow.Index = 8;
			this.menuTraceWindow.Text = "Trace Window";
			this.menuTraceWindow.Click += new System.EventHandler(this.menuTraceWindow_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 9;
			this.menuItem7.Text = "-";
			// 
			// menuProperties
			// 
			this.menuProperties.DefaultItem = true;
			this.menuProperties.Index = 10;
			this.menuProperties.Text = "Properties...";
			this.menuProperties.Click += new System.EventHandler(this.menuProperties_Click);
			// 
			// menuHelp
			// 
			this.menuHelp.Index = 11;
			this.menuHelp.Text = "Help...";
			this.menuHelp.Click += new System.EventHandler(this.menuHelp_Click);
			// 
			// menuItem10
			// 
			this.menuItem10.Index = 12;
			this.menuItem10.Text = "-";
			// 
			// menuExit
			// 
			this.menuExit.Enabled = false;
			this.menuExit.Index = 13;
			this.menuExit.Text = "Exit";
			this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
			// 
			// FormsTrayApp
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Name = "FormsTrayApp";

		}

/*		private void AnimateIcon(int index)
		{
			Console.WriteLine("In AnimateIcon: state = " + syncState.ToString());
			switch (syncState)
			{
				case SyncManagerStates.Active:
					notifyIcon1.Icon = uploadIcons[index];
					break;
				default:
					notifyIcon1.Icon = trayIcon;
					break;
			}

			Console.WriteLine("Leaving AnimateWorker");
		}*/

		private void ShutdownTrayApp()
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if ((workerThread != null) && workerThread.IsAlive)
				{
					workerThread.Abort();
				}

				// Stop the services.
				serviceManager.StopServices();

				// Wait for the services to stop.
				while (!serviceManager.ServicesStopped)
				{
					Application.DoEvents();
					Thread.Sleep(100);
				}
			}
			catch (SimiasException e)
			{
				e.LogError();
			}
			catch (Exception e)
			{
				logger.Debug(e, "Shutting down");
			}

			Cursor.Current = Cursors.Default;
			notifyIcon1.Visible = false;
			Application.Exit();
		}

		private void CleanupTrayApp()
		{
			MessageBox.Show("A fatal error was encountered during iFolder initialization.  Please see the log file for additional information", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			if (serviceManager != null)
			{
				serviceManager.StopServices();
				while (!serviceManager.ServicesStopped)
				{
					Application.DoEvents();
					Thread.Sleep(100);
				}
			}

			Application.Exit();
		}
		#endregion

		#region Public Methods
		public void AnimateWorker()
		{
			int i = 0;
			while (true)
			{
				Console.WriteLine("In AnimateWorker: state = " + syncState.ToString());
				IAsyncResult r = BeginInvoke(animateDelegate, new object[] {i});
				if (syncState == SyncManagerStates.Active)
				{
					Thread.Sleep(100);
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
	}
}
