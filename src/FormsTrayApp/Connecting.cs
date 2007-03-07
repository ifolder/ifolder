/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Threading;

using Novell.iFolderCom;
using Simias.Client;
using Simias.Client.Authentication;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for Connecting.
	/// </summary>
	public class Connecting : System.Windows.Forms.Form
	{
		#region Class Members

		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(Connecting));
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label1;
		private bool first = true;
		private string server;
		private string user;
		private string password;
		private bool defaultServer;
		private bool rememberPassword;
		private bool updatePasswordPreference = false;
		private SimiasWebService simiasWebService;
		private Manager simiasManager;
		private DomainInformation domainInfo = null;
		private Thread connectThread;
		private delegate void DisplayMessageDelegate(string message, string title, string details, MyMessageBoxButtons buttons, MyMessageBoxIcon icon, MyMessageBoxDefaultButton defaultButton);
		private DisplayMessageDelegate displayMessageDelegate;
		private delegate void ConnectDoneDelegate();
		private ConnectDoneDelegate connectDoneDelegate;
		private bool connectResult;
		private DialogResult messageDialogResult;
		protected AutoResetEvent messageEvent = new AutoResetEvent( false );
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion

		#region Constructor

		public Connecting( SimiasWebService simiasWebService, Manager simiasManager, string server, string user, string password, bool defaultServer, bool rememberPassword ) :
			this( simiasWebService, simiasManager, password )
		{
			this.server = server;
			this.user = user;
			this.defaultServer = defaultServer;
			this.rememberPassword = rememberPassword;
		}

		public Connecting( SimiasWebService simiasWebService, Manager simiasManager, DomainInformation domainInfo, string password, bool rememberPassword ) :
			this( simiasWebService, simiasManager, domainInfo, password )
		{
			this.updatePasswordPreference = true;
			this.rememberPassword = rememberPassword;
		}

		public Connecting( SimiasWebService simiasWebService, Manager simiasManager, DomainInformation domainInfo, string password ) :
			this( simiasWebService, simiasManager, password )
		{
			this.domainInfo = domainInfo;
		}

		public Connecting( SimiasWebService simiasWebService, Manager simiasManager, DomainInformation domainInfo ) :
			this( simiasWebService, simiasManager, domainInfo, null )
		{
		}

		private Connecting( SimiasWebService simiasWebService, Manager simiasManager, string password )
		{
			this.simiasWebService = simiasWebService;
			this.simiasManager = simiasManager;
			this.password = password;

			displayMessageDelegate = new DisplayMessageDelegate( displayMessage );
			connectDoneDelegate = new ConnectDoneDelegate( connectDone );

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		#endregion

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Connecting));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.AccessibleDescription = resources.GetString("pictureBox1.AccessibleDescription");
			this.pictureBox1.AccessibleName = resources.GetString("pictureBox1.AccessibleName");
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pictureBox1.Anchor")));
			this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
			this.pictureBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pictureBox1.Dock")));
			this.pictureBox1.Enabled = ((bool)(resources.GetObject("pictureBox1.Enabled")));
			this.pictureBox1.Font = ((System.Drawing.Font)(resources.GetObject("pictureBox1.Font")));
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pictureBox1.ImeMode")));
			this.pictureBox1.Location = ((System.Drawing.Point)(resources.GetObject("pictureBox1.Location")));
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pictureBox1.RightToLeft")));
			this.pictureBox1.Size = ((System.Drawing.Size)(resources.GetObject("pictureBox1.Size")));
			this.pictureBox1.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("pictureBox1.SizeMode")));
			this.pictureBox1.TabIndex = ((int)(resources.GetObject("pictureBox1.TabIndex")));
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Text = resources.GetString("pictureBox1.Text");
			this.pictureBox1.Visible = ((bool)(resources.GetObject("pictureBox1.Visible")));
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// Connecting
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.ControlBox = false;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pictureBox1);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "Connecting";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Activated += new System.EventHandler(this.Connecting_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers

		private void Connecting_Activated(object sender, System.EventArgs e)
		{
			if ( first )
			{
				first = false;

				connectThread = new Thread( new ThreadStart( connectToServer ) );
				connectThread.Start();
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Delegate used when successfully connected to Enterprise Server.
		/// </summary>
		public delegate void EnterpriseConnectDelegate(object sender, DomainConnectEventArgs e);
		/// <summary>
		/// Occurs when successfully connected to enterprise.
		/// </summary>
		public event EnterpriseConnectDelegate EnterpriseConnect;

		#endregion

		#region Private Methods 

		private bool authenticate()
		{
			bool result = false;

			try
			{
				// See if credentials have already been set in this process.
				DomainAuthentication domainAuth =
					new DomainAuthentication(
					"iFolder",
					domainInfo.ID,
					null);

				Status status = domainAuth.Authenticate(simiasManager.WebServiceUri, simiasManager.DataPath);
				switch (status.statusCode)
				{
					case StatusCodes.InvalidCertificate:
						byte[] byteArray = simiasWebService.GetCertificate(domainInfo.Host);
						System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(byteArray);

						BeginInvoke( displayMessageDelegate, 
							new object[] { string.Format(resourceManager.GetString("verifyCert"), domainInfo.Host), resourceManager.GetString("verifyCertTitle"), cert.ToString(true), MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button2 } );
						messageEvent.WaitOne();
						if ( messageDialogResult == DialogResult.Yes )
						{
							simiasWebService.StoreCertificate(byteArray, domainInfo.Host);
							result = authenticate();
						}
						break;
					case StatusCodes.Success:
						result = true;
						break;
					case StatusCodes.SuccessInGrace:
						BeginInvoke( displayMessageDelegate, 
							new object[] { string.Format(resourceManager.GetString("graceLogin"), status.RemainingGraceLogins),
											 resourceManager.GetString("graceLoginTitle"),
											 string.Empty,
											 MyMessageBoxButtons.OK,
											 MyMessageBoxIcon.Information, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						result = true;
						break;
					default:
					{
						string userID;

						// See if there is a password saved on this domain.
						CredentialType credType = simiasWebService.GetDomainCredentials(domainInfo.ID, out userID, out password);
						if ((credType == CredentialType.Basic) && (password != null))
						{
							// There are credentials that were saved on the domain. Use them to authenticate.
							// If the authentication fails for any reason, pop up and ask for new credentials.
							domainAuth = new DomainAuthentication("iFolder", domainInfo.ID, password);
							Status authStatus = domainAuth.Authenticate(simiasManager.WebServiceUri, simiasManager.DataPath);
							switch (authStatus.statusCode)
							{
								case StatusCodes.Success:
									result = true;
									break;
								case StatusCodes.SuccessInGrace:
									BeginInvoke( displayMessageDelegate, 
										new object[] { string.Format(resourceManager.GetString("graceLogin"), status.RemainingGraceLogins),
														 resourceManager.GetString("graceLoginTitle"),
														 string.Empty,
														 MyMessageBoxButtons.OK,
														 MyMessageBoxIcon.Information, MyMessageBoxDefaultButton.Button1 } );
									messageEvent.WaitOne();
									result = true;
									break;
								case StatusCodes.AccountDisabled:
									BeginInvoke( displayMessageDelegate, 
										new object[] { resourceManager.GetString("accountDisabled"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
									messageEvent.WaitOne();
									break;
								case StatusCodes.AccountLockout:
									BeginInvoke( displayMessageDelegate, 
										new object[] { resourceManager.GetString("accountLockout"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
									messageEvent.WaitOne();
									break;
								case StatusCodes.SimiasLoginDisabled:
									BeginInvoke( displayMessageDelegate, 
										new object[] { resourceManager.GetString("iFolderAccountDisabled"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
									messageEvent.WaitOne();
									break;
								case StatusCodes.UnknownUser:
								case StatusCodes.InvalidPassword:
								case StatusCodes.InvalidCredentials:
									// There are bad credentials stored. Remove them.
									simiasWebService.SetDomainCredentials(domainInfo.ID, null, CredentialType.None);
									BeginInvoke( displayMessageDelegate, 
										new object[] { resourceManager.GetString("failedAuth"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
									messageEvent.WaitOne();
									break;
							}
						}
						break;
					}
				}
			}
			catch {}

			return result;
		}

		private void connectDone()
		{
			if ( connectResult )
			{
				DialogResult = DialogResult.OK;
			}
			else
			{
				DialogResult = DialogResult.No;
			}

			Close();
		}

		/// <summary>
		/// Thread used to connect to the server.
		/// </summary>
		private void connectToServer()
		{
			if ( this.domainInfo != null )
			{
				if ( password == null )
				{
					connectResult = authenticate();
				}
				else
				{
					connectResult = login();
				}
			}
			else
			{
				connectResult = initialConnect();
			}

			/*
			 *	Add  code here for showing passphrase dialogs depending on I/P;
			 */
			if( connectResult)
			{
				// Check which dialog to display.
				/*	if passphrase is not set show verify dialog. else check for remember option 
				 *	and if the passphrase differs show the verify dialog.
				 */
				
				bool passPhraseStatus = false;
				
				bool passphraseStatus = this.simiasWebService.IsPassPhraseSet(this.domainInfo.ID);
				/*
				if( passphraseStatus )
					MessageBox.Show("passphrase", "passphrase is set");
				else
					MessageBox.Show("passphrase", "passphrase is not set");
					*/
				if(passphraseStatus == true)
				{
					// if passphrase not given during login
					string passphrasecheck = null;
					if( this.simiasWebService.GetRememberOption(this.domainInfo.ID) == true )
					{
						passphrasecheck = this.simiasWebService.GetPassPhrase(this.domainInfo.ID);
					}
					if( passphrasecheck == null || passphrasecheck =="")
					{
					//	MessageBox.Show( "passphrase is not remembered", "passphrase");
						Novell.iFolderCom.VerifyPassphraseDialog vpd = new Novell.iFolderCom.VerifyPassphraseDialog(this.domainInfo.ID, this.simiasWebService);
						vpd.ShowDialog();
						passPhraseStatus = vpd.PassphraseStatus;
					}
					else
					{
						passPhraseStatus = true;
					}

				}
				/*
				if(passphraseStatus == true)
				{
					// if passphrase not given during login
					string passphrasecheck = this.simiasWebService.GetPassPhrase(this.domainInfo.ID);
					if( passphrasecheck == null || passphrasecheck =="")
					{
						TrayApp.VerifyPassphraseDialog vpd = new TrayApp.VerifyPassphraseDialog(this.domainInfo.ID, this.simiasWebService);
						vpd.ShowDialog();
						passPhraseStatus = vpd.PassphraseStatus;
					}
					else
					{
						passPhraseStatus = true;
					}
				}
				*/
				else
				{
					// Passphrase not set
				//	MessageBox.Show("Showing Enter passphrase dialog", "passphrase");
					EnterPassphraseDialog enterPassPhrase= new EnterPassphraseDialog(this.domainInfo.ID, this.simiasWebService);
					enterPassPhrase.ShowDialog();
					passPhraseStatus = enterPassPhrase.PassphraseStatus;
				}
				if( passPhraseStatus == false)
				{
					// No Passphrase
				}
			}
			BeginInvoke( this.connectDoneDelegate );
		}

		private void displayMessage( string message, string title, string details, MyMessageBoxButtons buttons, MyMessageBoxIcon icon, MyMessageBoxDefaultButton defaultButton )
		{
			MyMessageBox mmb = new MyMessageBox( message, title, details, buttons, icon, defaultButton );
			messageDialogResult = mmb.ShowDialog();
			messageEvent.Set();
		}

		private bool initialConnect()
		{
			bool result = false;

			try
			{
				// Set the proxy for this url.
				SetProxyForDomain( server, true );

				// Connect to the domain now that the proxy is set.
				domainInfo = simiasWebService.ConnectToDomain( user, password, server );

				switch (domainInfo.StatusCode)
				{
					case StatusCodes.InvalidCertificate:
						byte[] byteArray = simiasWebService.GetCertificate(server);
						System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(byteArray);
						BeginInvoke( displayMessageDelegate, 
							new object[] { string.Format(resourceManager.GetString("verifyCert"), server), resourceManager.GetString("verifyCertTitle"), cert.ToString(true), MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button2 } );
						messageEvent.WaitOne();
						if ( messageDialogResult == DialogResult.Yes )
						{
							simiasWebService.StoreCertificate(byteArray, server);
							result = initialConnect();
						}
						break;
					case StatusCodes.Success:
					case StatusCodes.SuccessInGrace:
						// Set the credentials in the current process.
						
						DomainAuthentication domainAuth = new DomainAuthentication("iFolder", domainInfo.ID, password);
						domainAuth.Authenticate(simiasManager.WebServiceUri, simiasManager.DataPath);

						domainInfo.Authenticated = true;

						if ( rememberPassword )
						{
							try
							{
								simiasWebService.SetDomainCredentials(domainInfo.ID, password, CredentialType.Basic);
							}
							catch (Exception ex)
							{
								BeginInvoke( displayMessageDelegate, 
									new object[] { resourceManager.GetString("savePasswordError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
								messageEvent.WaitOne();
							}
						}

						try
						{
							bool res = FormsTrayApp.ClientUpdates(domainInfo.ID);
							
							if (res == false)
							{
								// remove the domain.....
								try
								{
									simiasWebService.LeaveDomain(domainInfo.ID, false);
								}
								catch{ }
								MessageBox.Show(" You need to upgrade the client to connect to this server.", "Unable to Login", MessageBoxButtons.OK);
								return false;
							}
					
							// Check for an update.
//TODO:							if (FormsTrayApp.CheckForClientUpdate(domainInfo.ID))
							{
//								if (ShutdownTrayApp != null)
								{
									// Shut down the tray app.
//									ShutdownTrayApp(this, new EventArgs());
								}
							}
						}
						catch(Exception ex) // Ignore
						{
							MessageBox.Show(ex.ToString(), "error in webservice", MessageBoxButtons.OK);
						}

						if ( defaultServer )
						{
							try
							{
								simiasWebService.SetDefaultDomain(domainInfo.ID);
								domainInfo.IsDefault = true;
							}
							catch (Exception ex)
							{
								BeginInvoke( displayMessageDelegate, 
									new object[] { resourceManager.GetString("setDefaultError"), resourceManager.GetString("accountErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
								messageEvent.WaitOne();
							}
						}

						if (domainInfo.StatusCode.Equals(StatusCodes.SuccessInGrace))
						{
							BeginInvoke( displayMessageDelegate, 
								new object[] { string.Format(resourceManager.GetString("graceLogin"), domainInfo.RemainingGraceLogins),
												 resourceManager.GetString("graceLoginTitle"),
												 string.Empty,
												 MyMessageBoxButtons.OK,
												 MyMessageBoxIcon.Information, MyMessageBoxDefaultButton.Button1 } );
							messageEvent.WaitOne();
						}
						
						if (EnterpriseConnect != null)
						{
							// Fire the event telling that a new domain has been added.
							EnterpriseConnect( this, new DomainConnectEventArgs( domainInfo ) );
						}

						result = true;
						break;
					case StatusCodes.InvalidCredentials:
					case StatusCodes.InvalidPassword:
					case StatusCodes.UnknownUser:
						BeginInvoke( displayMessageDelegate, 
							new object[] { resourceManager.GetString("failedAuth"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						break;
					case StatusCodes.AccountDisabled:
						BeginInvoke( displayMessageDelegate, 
							new object[] { resourceManager.GetString("accountDisabled"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						break;
					case StatusCodes.AccountLockout:
						BeginInvoke( displayMessageDelegate, 
							new object[] { resourceManager.GetString("accountLockout"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						break;
					case StatusCodes.SimiasLoginDisabled:
						BeginInvoke( displayMessageDelegate, 
							new object[] { resourceManager.GetString("iFolderAccountDisabled"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						break;
					case StatusCodes.UnknownDomain:
						BeginInvoke( displayMessageDelegate, 
							new object[] { resourceManager.GetString("unknownDomain"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						break;
					default:
						BeginInvoke( displayMessageDelegate, 
							new object[] { resourceManager.GetString("serverConnectError"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						break;
				}
			}
			catch (Exception ex)
			{
				if ((ex.Message.IndexOf("Simias.ExistsException") != -1) ||
					(ex.Message.IndexOf("already exists") != -1))
				{
					BeginInvoke( displayMessageDelegate, 
						new object[] { resourceManager.GetString("alreadyJoined"), resourceManager.GetString("alreadyJoinedTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information, MyMessageBoxDefaultButton.Button1 } );
					messageEvent.WaitOne();
				}
				else
				{
					BeginInvoke( displayMessageDelegate, 
						new object[] { resourceManager.GetString("serverConnectError"), resourceManager.GetString("serverConnectErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
					messageEvent.WaitOne();
				}
			}

			return result;
		}

		private bool login()
		{
			bool result = false;

			DomainAuthentication domainAuth = new DomainAuthentication("iFolder", domainInfo.ID, password);
			Status authStatus = domainAuth.Authenticate(simiasManager.WebServiceUri, simiasManager.DataPath);
			switch (authStatus.statusCode)
			{
				case StatusCodes.InvalidCertificate:
					byte[] byteArray = simiasWebService.GetCertificate(domainInfo.Host);
					System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(byteArray);

					BeginInvoke( displayMessageDelegate, 
						new object[] { string.Format(resourceManager.GetString("verifyCert"), domainInfo.Host), resourceManager.GetString("verifyCertTitle"), cert.ToString(true), MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button2 } );
					messageEvent.WaitOne();
					if ( messageDialogResult == DialogResult.Yes )
					{
						simiasWebService.StoreCertificate(byteArray, domainInfo.Host);
						result = login();
					}
					break;
				case StatusCodes.Success:
				case StatusCodes.SuccessInGrace:
					result = true;
					if (authStatus.statusCode.Equals(StatusCodes.SuccessInGrace))
					{
						BeginInvoke( displayMessageDelegate, 
							new object[] { string.Format(resourceManager.GetString("graceLogin"), authStatus.RemainingGraceLogins),
											 resourceManager.GetString("graceLoginTitle"),
											 string.Empty,
											 MyMessageBoxButtons.OK,
											 MyMessageBoxIcon.Information, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
					}

					if ( updatePasswordPreference )
					{
						try
						{
							if ( rememberPassword )
							{
								simiasWebService.SetDomainCredentials( domainInfo.ID, password, CredentialType.Basic );
							}
							else
							{
								simiasWebService.SetDomainCredentials( domainInfo.ID, null, CredentialType.None );
							}
						}
						catch (Exception ex)
						{
							BeginInvoke( displayMessageDelegate, 
								new object[] { resourceManager.GetString("savePasswordError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
							messageEvent.WaitOne();
						}
					}

					try
					{
						bool res = FormsTrayApp.ClientUpdates(domainInfo.ID);
						if(res == false)
						{
							MessageBox.Show("You need to upgrade your client to connect to the server.", "Login denied", MessageBoxButtons.OK);
							try
							{
								DomainAuthentication domainAuth1 = new DomainAuthentication("iFolder", domainInfo.ID, null);
								Status auth = domainAuth1.Logout(simiasManager.WebServiceUri, simiasManager.DataPath);
							}
							catch(Exception ex)
							{
								//MessageBox.Show(ex.ToString(), "error in login", MessageBoxButtons.OK);
							}
							return false;
							//this.simiasWebService.
						}
						
/* TODO:						bool update = FormsTrayApp.CheckForClientUpdate(domainInfo.ID);
						if (update)
						{
							if (ShutdownTrayApp != null)
							{
								// Shut down the tray app.
								ShutdownTrayApp(this, new EventArgs());
							}
						}*/
					}
					catch // Ignore
					{
					}
					break;
				case StatusCodes.InvalidCredentials:
				case StatusCodes.InvalidPassword:
				case StatusCodes.UnknownUser:
					BeginInvoke( displayMessageDelegate, 
						new object[] { resourceManager.GetString("failedAuth"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
					messageEvent.WaitOne();
					break;
				case StatusCodes.AccountDisabled:
					BeginInvoke( displayMessageDelegate, 
						new object[] { resourceManager.GetString("accountDisabled"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
					messageEvent.WaitOne();
					break;
				case StatusCodes.AccountLockout:
					BeginInvoke( displayMessageDelegate, 
						new object[] { resourceManager.GetString("accountLockout"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
					messageEvent.WaitOne();
					break;
				case StatusCodes.SimiasLoginDisabled:
					BeginInvoke( displayMessageDelegate, 
						new object[] { resourceManager.GetString("iFolderAccountDisabled"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
					messageEvent.WaitOne();
					break;
				default:
					BeginInvoke( displayMessageDelegate, 
						new object[] { resourceManager.GetString("serverConnectError"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
					messageEvent.WaitOne();
					break;
			}

			return result;
		}

		#endregion

		#region Public Methods
		/// <summary>
		/// Sets the proxy for the domain.
		/// </summary>
		/// <param name="hostUrl"></param>
		/// <param name="unknownScheme"></param>
		public void SetProxyForDomain( string hostUrl, bool unknownScheme )
		{
			UriBuilder ubHost = new UriBuilder( hostUrl );

			// If a domain name was passed in without a scheme, a proxy will
			// need to be setup for both http and https schemes because we don't
			// know how it will ultimately be sent.
			if ( unknownScheme )
			{
				// Set the proxy for http.
				ubHost.Scheme = Uri.UriSchemeHttp;
				ubHost.Port = 80;
				SetProxyForDomain( ubHost.Uri.ToString(), false );

				// Now set it for https.
				ubHost.Scheme = Uri.UriSchemeHttps;
				ubHost.Port = 443;
				SetProxyForDomain( ubHost.Uri.ToString(), false );
			}
			else
			{
				// Set any proxy information for this domain.
				IWebProxy iwp = GlobalProxySelection.Select;
				if ( !iwp.IsBypassed( ubHost.Uri ) )
				{
					string proxyUser = null;
					string proxyPassword = null;

					Uri proxyUri = iwp.GetProxy( ubHost.Uri );
					if ( iwp.Credentials != null )
					{
						NetworkCredential netCred = iwp.Credentials.GetCredential( proxyUri, "Basic" );
						if ( netCred != null )
						{
							proxyUser = netCred.UserName;
							proxyPassword = netCred.Password;
						}
					}

					// The scheme for the proxy address needs to match the scheme for the host address.
					simiasWebService.SetProxyAddress( ubHost.Uri.ToString(), proxyUri.ToString(), proxyUser, proxyPassword );
				}
			}
		}
		#endregion

		#region Properties

		/// <summary>
		/// Gets the domain information.
		/// </summary>
		public DomainInformation DomainInformation
		{
			get { return this.domainInfo; }
		}

		/// <summary>
		/// Gets the password for the user.
		/// </summary>
		public string Password
		{
			get { return password; }
		}

		#endregion
	}
}
