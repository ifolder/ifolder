/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Ashok Singh <siashok@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Novell.iFolderCom;
using System.IO;
using Novell.AutoAccountHelper;


namespace Novell.FormsTrayApp
{
    public partial class EnterPasswordPopup : Form
    {
        private string user, passwd, serverIP;
        private bool rememberPasswd;
        public bool passwordStatus, dialogDispose;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="serv"></param>
        /// <param name="username"></param>
        /// <param name="remPass"></param>
        public EnterPasswordPopup(string serv, string username, bool remPass)
        {
            serverIP = serv;
            user = username;
            rememberPasswd = remPass;
            passwordStatus = false;
            InitializeComponent();
            userName.Text = user;
            serverValue.Text = serverIP;
            btnOk.Enabled = false;
            dialogDispose = false;
            this.AcceptButton = btnOk;
        }

        /// <summary>
        /// Button clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOk_Click(object sender, EventArgs e)
        {
            passwd = password.Text;
            passwordStatus = true;
            rememberPasswd = rememberPassword.Checked;
            this.Close();
            this.Dispose();
        }

        /// <summary>
        /// whether to show enterpasswordpopup
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="username"></param>
        /// <param name="pass"></param>
        /// <param name="remPass"></param>
        /// <returns>true if it has to show the popup</returns>
        static public bool ShowEnterPasswordPopup(string ip, string username, out string pass, bool remPass)
        {
            DialogResult result;

            EnterPasswordPopup epd = new EnterPasswordPopup(ip, username,remPass);
            epd.password.Select();//to set the focus
            epd.btnOk.Focus();
            epd.rememberPassword.Checked = remPass;
            
            do
            {
                result = epd.ShowDialog();
            } while (result == DialogResult.Cancel && !epd.dialogDispose);
            pass = epd.Password;
            return epd.passwordStatus;
        }

        /// <summary>
        /// Text changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void password_TextChanged(object sender, EventArgs e)
        {
            bool enableOkBtn = false;
            if (password.Text.Length > 0)
                enableOkBtn = true;
            btnOk.Enabled = enableOkBtn;
        }

        /// <summary>
        /// Button clicked event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnterPasswordPopup));
            MyMessageBox mmb = new MyMessageBox(resources.GetString("NoPasswordMsg"), resources.GetString("NoPasswordTitle"), string.Empty, MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question);
            mmb.StartPosition = FormStartPosition.CenterScreen;
            DialogResult rc = mmb.ShowDialog();
            if (rc == DialogResult.Yes)
            {
                this.Close();
                this.Dispose();
                dialogDispose = true;
            }
        }

        /// <summary>
        /// Gets password
        /// </summary>
        public string Password
        {
            get
            {
                return passwd;
            }
        }

        /// <summary>
        /// Gets remember password option
        /// </summary>
        public bool RememberPassword
        {
            get
            {
                return rememberPasswd;
            }
        }

    }
}
