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

using Novell.FormsTrayApp;
using Novell.iFolderCom;
using Simias.Client;
using Simias.Client.Authentication;

namespace Novell.Wizard
{
	/// <summary>
	/// Summary description for Connecting.
	/// </summary>
	public class Connecting : System.Windows.Forms.Form
	{
		#region Class Members

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label1;
		private bool first = true;
		private string server;
		private string user;
		private string password;
		private bool defaultServer;
		private bool rememberPassword;
		private SimiasWebService simiasWebService;
		private Manager simiasManager;
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

		public Connecting( SimiasWebService simiasWebService, Manager simiasManager, string server, string user, string password, bool defaultServer, bool rememberPassword )
		{
			this.simiasWebService = simiasWebService;
			this.simiasManager = simiasManager;
			this.server = server;
			this.user = user;
			this.password = password;
			this.defaultServer = defaultServer;
			this.rememberPassword = rememberPassword;

			displayMessageDelegate = new DisplayMessageDelegate( displayMessage );
			connectDoneDelegate = new ConnectDoneDelegate( connectDone );

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(16, 16);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(50, 50);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(88, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(320, 40);
			this.label1.TabIndex = 1;
			this.label1.Text = "Please wait while your iFolder account is connecting.";
			// 
			// Connecting
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(426, 88);
			this.ControlBox = false;
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pictureBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Connecting";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Connecting...";
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

		private bool connect()
		{
			bool result = false;

			try
			{
				// Set the proxy for this url.
				SetProxyForDomain( server, true );

				// Connect to the domain now that the proxy is set.
				DomainInformation domainInfo = simiasWebService.ConnectToDomain( user, password, server );

				switch (domainInfo.StatusCode)
				{
					case StatusCodes.InvalidCertificate:
						byte[] byteArray = simiasWebService.GetCertificate(server);
						System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(byteArray);
						BeginInvoke( displayMessageDelegate, 
							new object[] { string.Format("verifyCert {0}", server), "verifyCertTitle", cert.ToString(true), MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button2 } );
						messageEvent.WaitOne();
						if ( messageDialogResult == DialogResult.Yes )
						{
							simiasWebService.StoreCertificate(byteArray, server);
							result = connect();
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
							simiasWebService.SetDomainCredentials(domainInfo.ID, password, CredentialType.Basic);
						}

						try
						{
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
						catch // Ignore
						{
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
									new object[] { "setDefaultError", "accountErrorTitle", ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
								messageEvent.WaitOne();
							}
						}

						if (domainInfo.StatusCode.Equals(StatusCodes.SuccessInGrace))
						{
							BeginInvoke( displayMessageDelegate, 
								new object[] { string.Format("graceLogin", domainInfo.RemainingGraceLogins),
												"graceLoginTitle",
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
							new object[] { "failedAuth", "serverConnectErrorTitle", string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						break;
					case StatusCodes.AccountDisabled:
						BeginInvoke( displayMessageDelegate, 
							new object[] { "accountDisabled", "serverConnectErrorTitle", string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						break;
					case StatusCodes.AccountLockout:
						BeginInvoke( displayMessageDelegate, 
							new object[] { "accountLockout", "serverConnectErrorTitle", string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						break;
					case StatusCodes.SimiasLoginDisabled:
						BeginInvoke( displayMessageDelegate, 
							new object[] { "iFolderAccountDisabled", "serverConnectErrorTitle", string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						break;
					case StatusCodes.UnknownDomain:
						BeginInvoke( displayMessageDelegate, 
							new object[] { "unknownDomain", "serverConnectErrorTitle", string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						break;
					default:
						BeginInvoke( displayMessageDelegate, 
							new object[] { "serverConnectError", "serverConnectErrorTitle", string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
						messageEvent.WaitOne();
						break;
				}
			}
			catch (Exception ex)
			{
				//				Cursor.Current = Cursors.Default;
				if ((ex.Message.IndexOf("Simias.ExistsException") != -1) ||
					(ex.Message.IndexOf("already exists") != -1))
				{
					BeginInvoke( displayMessageDelegate, 
						new object[] { "alreadyJoined", "alreadyJoinedTitle", string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information, MyMessageBoxDefaultButton.Button1 } );
					messageEvent.WaitOne();
				}
				else
				{
					BeginInvoke( displayMessageDelegate, 
						new object[] { "serverConnectError", "serverConnectErrorTitle", ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error, MyMessageBoxDefaultButton.Button1 } );
					messageEvent.WaitOne();
				}
			}

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

		private void connectToServer()
		{
			connectResult = connect();

			BeginInvoke( this.connectDoneDelegate );
		}

		private void displayMessage( string message, string title, string details, MyMessageBoxButtons buttons, MyMessageBoxIcon icon, MyMessageBoxDefaultButton defaultButton )
		{
			MyMessageBox mmb = new MyMessageBox( message, title, details, buttons, icon, defaultButton );
			messageDialogResult = mmb.ShowDialog();
			messageEvent.Set();
		}

		#endregion

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
	}
}
