/***********************************************************************
 *  FormsTrayApp.cs - The iFolder tray app using Windows.Forms
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
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
using Novell.iFolder.Win32Util;
using Simias;
using Simias.Event;


namespace Novell.iFolder.FormsTrayApp
{
	/// <summary>
	/// Summary description for FormsTrayApp.
	/// </summary>
	public class FormsTrayApp : Form
	{
		#region Class Members
		private NotifyIcon notifyIcon1;
		private ContextMenu contextMenu1;
		private MenuItem menuItemExit;
		private MenuItem menuItemInviteWizard;
		private MenuItem menuItemAddrBook;
		private MenuItem menuItemTracer;
		private MenuItem menuItemBrowser;
		private System.ComponentModel.IContainer components;

		private SyncManager syncManager = null;
		private Thread workerThread = null;

		private Icon trayIcon;
        private const int numberOfIcons = 10;
		private Icon[] uploadIcons = new Icon[numberOfIcons];

		private SyncManagerStates syncState = SyncManagerStates.Idle;
		protected AutoResetEvent synkEvent = null;

		private MyTraceForm traceForm;

		private bool noTray = false;

		private delegate void AnimateDelegate(int index);
		private AnimateDelegate animateDelegate;

		private	EventPublisher publisher;
		//System.Diagnostics.Process monitor;
		//private const int waitTime = 3000;
		#endregion

		[STAThread]
		static void Main(string[] args)
		{
			Application.Run(new FormsTrayApp());
		}

		public FormsTrayApp()
		{
			InitializeComponent();

			this.components = new System.ComponentModel.Container();
			this.contextMenu1 = new ContextMenu();
			this.menuItemExit = new MenuItem("E&xit");
			this.menuItemInviteWizard = new MenuItem("&Invitation Wizard");
			this.menuItemAddrBook = new MenuItem("&Address Book");
			this.menuItemTracer = new MenuItem("Trace Window");
			this.menuItemBrowser = new MenuItem("Store Browser");

			// Initialize contextMenu1
			if (File.Exists(Path.Combine(Environment.CurrentDirectory, "StoreBrowser.exe")))
			{
				this.contextMenu1.MenuItems.AddRange(
					new MenuItem[] {this.menuItemExit, this.menuItemInviteWizard, this.menuItemAddrBook, this.menuItemTracer, this.menuItemBrowser});
			}
			else
			{
				this.contextMenu1.MenuItems.AddRange(
					new MenuItem[] {this.menuItemExit, this.menuItemInviteWizard, this.menuItemAddrBook, this.menuItemTracer});
			}

			// Initialize menuItemExit
			this.menuItemExit.Index = 0;
			this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);

			// Initialize menuItemInviteWizard
			this.menuItemInviteWizard.Index = 0;
			this.menuItemInviteWizard.Click += new System.EventHandler(this.menuItemInviteWizard_Click);

			// Initialize menuItemAddrBook
			this.menuItemAddrBook.Index = 0;
			this.menuItemAddrBook.Click += new System.EventHandler(menuItemAddrBook_Click);

			// Initialize menuItemTracer
			this.menuItemTracer.Index = 0;
			this.menuItemTracer.Click += new System.EventHandler(menuItemTracer_Click);

			// Initialize menuItemBrowser
			this.menuItemBrowser.Index = 0;
			this.menuItemBrowser.Click += new EventHandler(menuItemBrowser_Click);

			// Set up how the form should be displayed.
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Text = "iFolder Services";

			// Create the NotifyIcon.
			this.notifyIcon1 = new NotifyIcon(this.components);

			// The Icon property sets the icon that will appear
			// in the systray for this application.
			try
			{
				string basePath = Path.Combine(Application.StartupPath, "res");
				this.Icon = new Icon(Path.Combine(basePath, "ifolder_loaded.ico"));

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

				Win32Window win32Window = new Win32Window(this.Handle);
				win32Window.MakeToolWindow();
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception caught: " + e.ToString());
				noTray = true;
			}		

			// The ContextMenu property sets the menu that will
			// appear when the systray icon is right clicked.
			notifyIcon1.ContextMenu = this.contextMenu1;

			// The Text property sets the text that will be displayed,
			// in a tooltip, when the mouse hovers over the systray icon.
			notifyIcon1.Text = "iFolder Services";
			notifyIcon1.Visible = true;

			// Handle the DoubleClick event to activate the form.
			//notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);

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
//		private void notifyIcon1_DoubleClick(object Sender, EventArgs e)
//		{
			// Show the form when the user double clicks on the notify icon.

			// Set the WindowState to normal if the form is minimized.
//			if (this.WindowState == FormWindowState.Minimized)
//				this.WindowState = FormWindowState.Normal;

//			this.ShowInTaskbar = true;

			// Activate the form.
//			this.Activate();
//		}

		private void menuItemExit_Click(object Sender, System.EventArgs e)
		{
			// Close the form, which closes the application.
			//this.Close();

			Cursor.Current = Cursors.WaitCursor;
			if ((workerThread != null) && workerThread.IsAlive)
			{
				workerThread.Abort();
			}

			if (syncManager != null)
			{
				syncManager.Stop();
			}

			if (publisher != null)
			{
				ServiceEventArgs args = new ServiceEventArgs(0, ServiceEventArgs.ServiceEvent.Shutdown);
				publisher.RaiseServiceEvent(args);
			}

			/*
			if (monitor != null)
			{
				// Give the broker a chance to send the shutdown event.
				Thread.Sleep(waitTime);
				monitor.Kill();
			}
			*/

			Cursor.Current = Cursors.Default;
			//			traceForm.Close();
			Application.Exit();
		}

		private void menuItemInviteWizard_Click(object Sender, System.EventArgs e)
		{
			// Check for currently running instance and switch to it.
//			Win32Window win32Window = Win32Window.FindWindow(null, "InvitationWizard");
//			if (win32Window != null)
//			{
//				win32Window.BringWindowToTop();
//			}
//			else
			{
				Process.Start("InvitationWizard.exe");
			}
		}

		private void menuItemAddrBook_Click(object sender, System.EventArgs e)
		{
			// Check for currently running instance and switch to it.
//			Win32Window win32Window = Win32Window.FindWindow(null, "FormsAddrBook");
//			if (win32Window != null)
//			{
//				win32Window.BringWindowToTop();
//			}
//			else
			{
				Process.Start("Book.exe");
			}
		}

		private void FormsTrayApp_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			if ((workerThread != null) && workerThread.IsAlive)
			{
				workerThread.Abort();
			}

			if (syncManager != null)
			{
				syncManager.Stop();
			}

			if (publisher != null)
			{
				ServiceEventArgs args = new ServiceEventArgs(0, ServiceEventArgs.ServiceEvent.Shutdown);
				publisher.RaiseServiceEvent(args);
			}

			/*
			if (monitor != null)
			{
				// Give the broker a chance to send the shutdown event.
				Thread.Sleep(waitTime);
				monitor.Kill();
			}
			*/

			Cursor.Current = Cursors.Default;
			Application.Exit();
		}

		private void traceForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!this.traceForm.Shutdown)
			{
				// If the form didn't receive a shutdown notification,
				// cancel the Closing event.
				e.Cancel = true;

				// Hide the trace window.
				this.traceForm.Hide();
			}
		}

		private void menuItemTracer_Click(object sender, System.EventArgs e)
		{
			// Display the trace window.
			this.traceForm.Show();
		}

		private void menuItemBrowser_Click(object sender, EventArgs e)
		{
			Process.Start("StoreBrowser.exe");
		}

		private void FormsTrayApp_Load(object sender, System.EventArgs e)
		{
			// Start the event broker.
			// TODO - check for currently running broker.
			/*
			monitor = new Process();
			monitor.StartInfo.RedirectStandardInput = true;
			monitor.StartInfo.RedirectStandardInput = true;
			monitor.StartInfo.CreateNoWindow = true;
			monitor.StartInfo.UseShellExecute = false;
			monitor.StartInfo.FileName = "CsEventBroker.exe";
			monitor.Start();
			*/
			this.publisher = new EventPublisher(new Configuration(), "Event_Domain");

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

			// Create the trace window.
			traceForm = new MyTraceForm();
			traceForm.Closing += new System.ComponentModel.CancelEventHandler(traceForm_Closing);
			traceForm.Show();

			Console.WriteLine("Creating sync object");

			SyncProperties properties = new SyncProperties();

			// Get the logic factory from the config file.
			Configuration config = new Configuration();
			string logicFactory = config.Get("iFolderApp", "SyncLogic", "SynkerA");
			switch (logicFactory)
			{
				case "SynkerA":
					properties.DefaultLogicFactory = typeof(SynkerA);
					break;
				case "SyncLogicFactoryLite":
					properties.DefaultLogicFactory = typeof(SyncLogicFactoryLite);
					break;
				default:
					break;
			}

			try
			{
				syncManager = new SyncManager(properties);
				syncManager.ChangedState += new ChangedSyncStateEventHandler(syncManager_ChangedState);

				// Trace levels.
				MyTrace.Switch.Level = TraceLevel.Verbose;
				Log.SetLevel("verbose");

				Console.WriteLine("Starting sync object");
				syncManager.Start();
			}
			catch(Exception exception)
			{
				MessageBox.Show("Exception caught in SyncManager:\n\n" + exception.Message);
				this.Close();
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
		#endregion

		private void InitializeComponent()
		{
			//System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FormsTrayApp));
			this.SuspendLayout();
			//
			// FormsTrayApp
			//
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 266);
			//this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormsTrayApp";
			this.ResumeLayout(false);
		}

		private void AnimateIcon(int index)
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
		}

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
	}
}
