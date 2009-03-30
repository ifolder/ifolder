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

namespace Novell.AutoAccountHelper
{
    /// <summary>
    /// enum to denote parsing status
    /// </summary>
    public enum ParseStatus
    {
        Success,
        ParseError,
        FileNotFound,
        IncompleteMandatoryFields,
        OnlyWizardAccounts,
        AutoAndWizard
    };

    public class UserAccount
    {
        private string server, userid, passwd, defaultiFolderPath/*, passphrase, recoveryAgent*/;
        private bool rememberPassword, /*rememberPassphrase,*/ defaultAccount, encrypted, promptForInvalidCert, secureSync;
        private bool needDefaultiFolder, valid, wizard;

        /// <summary>
        /// constructor
        /// </summary>
        public UserAccount()
        {
            //recoveryAgent = "None";
            rememberPassword = false;
            //rememberPassphrase = false;
            encrypted = false;
            promptForInvalidCert = false;
            defaultAccount = false;
            needDefaultiFolder = true;
            passwd = "";
            secureSync = false;
        }

        /// <summary>
        /// Gets/Sets the server
        /// </summary>
        public string Server
        {
            get
            {
                return server;
            }
            set
            {
                server = value;
            }
        }

        /// <summary>
        /// Gets/Sets the username as id
        /// </summary>
        public string UserName
        {
            get
            {
                return userid;
            }
            set
            {
                userid = value;
            }
        }
         
        /// <summary>
        /// Gets/Sets the password
        /// </summary>
        public string Password 
        {
            get
            {
                return passwd;
            }
            set
            {
                passwd = value;
            }
        }

        /// <summary>
        /// Gets/Sets the default iFolder path
        /// </summary>
        public string DefaultiFolderPath
        {
            get
            {
                return defaultiFolderPath;
            }
            set
            {
                defaultiFolderPath = value;
            }
        }
            
        /*public string Passphrase
        {
            get 
            {
                return passphrase;
            }
            set
            {
                passphrase = value;
            }
        }*/

        /// <summary>
        /// Gets/Sets the remember password option
        /// </summary>
        public bool RememberPassword
        {
            get
            {
                return rememberPassword;
            }
            set
            {
                rememberPassword = value;
            }
        }
        
        /*public bool RememberPassphrase
        {
            get
            {
                return rememberPassphrase;
            }
            set
            {
                rememberPassphrase = value;
            }
        }
        
        public string RecoveryAgent 
        {
            get
            {
                return recoveryAgent;
            }
            set
            {
                recoveryAgent = value;
            }
        }*/
        
        /// <summary>
        /// Gets/Sets the default account
        /// </summary>
        public bool DefaultAccount
        {
            get
            {
                return defaultAccount;
            }
            set
            {
                defaultAccount = value;
            }
        }
        
        /// <summary>
        /// Gets/Sets the encrypted option
        /// </summary>
        public bool Encrypted
        {
            get
            {
                return encrypted;
            }
            set
            {
                encrypted = value;
            }
        }
        /// <summary>
        /// Gets/Sets tje Secure sync option 
        /// </summary>
        public bool SecureSync
        {
            get
            {
                return secureSync;
            }
            set
            {
                secureSync = value;
            }
        }

        /// <summary>
        /// Gets/Sets the value of PromptForInvalidCert
        /// </summary>
        public bool PromptForInvalidCert
        {
            get
            {
                return promptForInvalidCert;
            }
            set
            {
                promptForInvalidCert = value;
            }
        }
        
        /// <summary>
        /// Gets/Sets the Need of default iFolder
        /// </summary>
        public bool NeedDefaultiFolder
        {
            get
            {
                return needDefaultiFolder;
            }
            set
            {
                needDefaultiFolder = value;
            }
        }
        
        /// <summary>
        /// Gets/Sets the valid
        /// </summary>
        public bool Valid
        {
            get
            {
                return valid;
            }
            set
            {
                valid = value;
            }
        }

        /// <summary>
        /// Gets/Sets the wizard value
        /// </summary>
        public bool Wizard
        {
            get
            {
                return wizard;
            }
            set
            {
                wizard = value;
            }
        }

    }

}
