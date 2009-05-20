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
        private bool rememberPassword, /*rememberPassphrase, */defaultAccount, encrypted, promptForInvalidCert,secureSync, forcemerge;
        private bool needDefaultiFolder, valid, wizard;

        /// <summary>
        /// Constructor
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
            valid = false;
            wizard = false;
            passwd = "";
			secureSync = false;
	    forcemerge = false;
        }

        /// <summary>
        /// Gets/Sets Server
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
        /// Gets/Sets User Name
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
        /// Gets/Sets password
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
        /// Gets/Sets Default iFolder path
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
        /// Gets/Sets Remember Password field
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
        /// Gets/Sets Default account Field
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
        /// Gets/Sets Encrypted field
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
        /// Gets/Sets SecureSync field
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

	public bool ForceMerge
	{
	    get
            {
		return forcemerge;
	    }
	    set
	    {
		forcemerge = value;
	    }
	}
		
        /// <summary>
        /// Gets/Sets whether to prompt for invalid cert
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
        /// Gets/Sets whether Default iFolder is needed
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
        /// Gets/Sets valid
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
        /// Gets/Sets wizard
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
