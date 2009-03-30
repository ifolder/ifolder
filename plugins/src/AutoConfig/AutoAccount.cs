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
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using System.IO;
using System.Net;
using Novell.iFolder.Controller;
using Simias.Client;
using Novell.iFolder;
using Novell.AutoAccountHelper;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

namespace Novell.AutoAccount
{
    public class AutoAccount
    {
        UserAccount[] userAccount;
        private bool iFolderCreationConfirmation = true, iFolderShareNotify = true, userJoinNotify = true, conflictNotify = true, autoSync = true;
        private int syncInterval = 0;
        private const int defaultSyncInterval = 300;
        private SimiasWebService        simws;
        private DomainInformation ConnectedDomain;
        private bool encryptionEnforced = false, passphraseEntered = false, encryptionEnabled = false;
        private bool isValid;

        const string sourceFileName = "AutoAccount.xml";
        const string sourceXsdName = "AutoAccount.xsd";
        readonly string sourceFilePath;
        readonly string sourceXsdPath;
        const string autoAccountXML = "auto-account";
        const string userAccountXML = "user-account";
        const string defaultAttributeXML = "default";
        const string userIdXML = "user-id";
        const string rememberPasswordXML = "remember-password";
        const string serverXML = "server";
        const string defaultiFolderXML = "default-ifolder";
        const string pathXML = "path";
        const string encryptedXML = "encrypted";
		const string secureSyncXML = "securesync";
/*        const string passphraseXML = "passphrase";
        const string rememberPassphraseXML = "remember-passphrase";
        const string recoveryAgentXML = "recovery-agent";
        */
        const string promptXML = "prompt-to-accept-cert";
        const string generalPrefXML = "general-preferences";
        const string iFolderCreationConfirmationXML = "iFolder-creation-confirmation";
        const string iFolderShareNotifyXML = "iFolder-share-notify";
        const string userJoinNotifyXML = "user-join-notify";
        const string conflictNotifyXML = "conflict-notify";
        const string autoSyncXML = "auto-sync";
        const string autoSyncIntervalXML = "interval";
        
        /// <summary>
        /// gets/sets useraccount length
        /// </summary>
        public int AccountCount
        {
            get
            {
                return userAccount.Length;
            }
        }
        
        /// <summary>
        /// gets/sets autoaccount file path
        /// </summary>
        public string AutoAccountFilePath 
        {
            get
            {
                return sourceFilePath;
            }
        }
        
        /// <summary>
        /// gets/sets auto account filename
        /// </summary>
        public string AutoAccountFileName 
        {
            get
            {
                return sourceFileName;
            }
        }
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="s"></param>
        public AutoAccount(SimiasWebService s)
        {
            simws = s;
            sourceFilePath = Path.Combine(iFolderLogManager.LogConfDirPath, sourceFileName);
            sourceXsdPath = Path.Combine(SimiasSetup.bindir, sourceXsdName);
            isValid = true;
        }

        /// <summary>
        /// checks whether the document is valid or not
        /// </summary>
        /// <returns>true if valid</returns>
        bool IsValidDocument()
        {
            try
            {
                XmlSchemaCollection xsc = null;
                XmlTextReader r = new XmlTextReader(sourceXsdPath);
                xsc = new XmlSchemaCollection();
                xsc.Add(null, r);


                XmlValidatingReader v = new XmlValidatingReader(File.Open(sourceFilePath, FileMode.Open, FileAccess.Read), XmlNodeType.Document, null);

                v.Schemas.Add(xsc);

                v.ValidationType = ValidationType.Schema;
                v.ValidationEventHandler += new ValidationEventHandler(MyValidationEventHandler);

                while (v.Read())
                {
                    // Can add code here to process the content.
                }
                v.Close();
            }
            catch (XmlSchemaException ee)
            {
                isValid = false;
                iFolderWindow.log.Info("XML Schema Validation Error :" + ee.Message);
                iFolderWindow.log.Debug("LineNumber = {0}", ee.LineNumber);
                iFolderWindow.log.Debug("LinePosition = {0}", ee.LinePosition);
            }
            catch (XmlException ee)
            {
                isValid = false;
                iFolderWindow.log.Info("Error: " + ee.Message);
                iFolderWindow.log.Debug("LineNumber = {0}", ee.LineNumber);
                iFolderWindow.log.Debug("LinePosition = {0}", ee.LinePosition);
                iFolderWindow.log.Debug("StackTrace = {0}", ee.StackTrace);
            }
            catch (Exception e)
            {
                isValid = false;
                iFolderWindow.log.Info("Error: " + e.Message);
                iFolderWindow.log.Debug("Source = {0}", e.Source);
                iFolderWindow.log.Debug("Type of Exception = {0}", e.GetType());
            }

            return isValid;
        }

        /// <summary>
        /// Event handler to handle if the document is valid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void MyValidationEventHandler(object sender,
                                            ValidationEventArgs args)
        {
            isValid = false;
            iFolderWindow.log.Info("XML Schema Validation error: " + args.Message);
            iFolderWindow.log.Debug("LineNumber = {0}", args.Exception.LineNumber);
            iFolderWindow.log.Debug("LinePosition = {0}", args.Exception.LinePosition);
        }
        /// <summary>
        /// if it is a valid document, extract all fields
        /// </summary>
        /// <returns>true if extraction passes</returns>
        public bool parseResponseFile()
        {
            int i = 0;
            try
            {
                if (!File.Exists(sourceXsdPath))
                {
                    iFolderWindow.log.Info("File {0} is not available for validation. Bypassing XSD validation", sourceXsdPath);
                }
                else
                {
                    if (!IsValidDocument())
                    {
                        iFolderWindow.log.Info("XML Schema Validation failed");
                        return false;
                    }
                    else
                    {
                        iFolderWindow.log.Info("XML Schema Validation passed");
                    }
                }

                XPathDocument xpd = new XPathDocument(sourceFilePath);
                XPathNavigator nav = ((IXPathNavigable)xpd).CreateNavigator();
                XPathNodeIterator iter = nav.Select("/"+autoAccountXML+"/"+userAccountXML);
                XPathNodeIterator newIter = null;
                userAccount = new UserAccount[iter.Count];
                while( iter.MoveNext() )
                {
                    userAccount[i] = new UserAccount();
                    newIter = iter.Current.SelectDescendants(XPathNodeType.Element, true );
                    while(newIter.MoveNext())
                    {
                        switch( newIter.Current.Name )
                        {
                            case userAccountXML:
                                if (newIter.Current.MoveToFirstAttribute())
                                {
                                    if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                        (string.Compare(newIter.Current.Value, "false", true) == 0))
                                    {
                                        userAccount[i].DefaultAccount = Boolean.Parse(newIter.Current.Value);
                                    }
                                    else
                                    {
                                        iFolderWindow.log.Info("Invalid value for {0} attribute. Using default value...", userAccountXML);
                                    }
                                }
                                newIter.Current.MoveToParent();
                                break;
                            case serverXML:
                                userAccount[i].Server = newIter.Current.Value;
                                break;
                            case userIdXML:
                                //userAccount[i].UserName = newIter.Current.Value;
				 try
                                {
                                    userAccount[i].UserName = System.Environment.ExpandEnvironmentVariables(newIter.Current.Value);
                                }
                                catch (ArgumentException arg)
                                {
                                    userAccount[i].UserName = "";
                                    iFolderWindow.log.Debug(arg,"{0} is null", userIdXML);
                                }
                                break;
                            case rememberPasswordXML:
                                if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                    (string.Compare(newIter.Current.Value, "false", true) == 0))
                                {
                                    userAccount[i].RememberPassword = Boolean.Parse(newIter.Current.Value);
                                }
                                else
                                {
                                    iFolderWindow.log.Info("Invalid value for {0}. Using default value...", rememberPasswordXML);
                                }
                                break;
                            case defaultiFolderXML:
                                if (newIter.Current.MoveToFirstAttribute())
                                {
                                    if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                        (string.Compare(newIter.Current.Value, "false", true) == 0))
                                    {
                                        userAccount[i].NeedDefaultiFolder = Boolean.Parse(newIter.Current.Value);
                                    }
                                    else
                                    {
                                        iFolderWindow.log.Info("Invalid value for {0} attribute. Using default value...", defaultiFolderXML);
                                    }
                                }
                                newIter.Current.MoveToParent();
                                break;
                            case pathXML:
                                //userAccount[i].DefaultiFolderPath = newIter.Current.Value;
 				try
                                {
                                    userAccount[i].DefaultiFolderPath = System.Environment.ExpandEnvironmentVariables(newIter.Current.Value);
                                }
                                catch (ArgumentException arg)
                                {
                                    userAccount[i].DefaultiFolderPath = "";
                                    iFolderWindow.log.Debug(arg,"{0} argument is null", pathXML);
                                }

                                break;
                            case encryptedXML:
                                if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                    (string.Compare(newIter.Current.Value, "false", true) == 0))
                                {
                                    userAccount[i].Encrypted = Boolean.Parse(newIter.Current.Value);
                                }
                                else
                                {
                                    iFolderWindow.log.Info("Invalid value for {0}. Using default value...", encryptedXML);
                                }
                                break;
								case secureSyncXML:
                                if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                    (string.Compare(newIter.Current.Value, "false", true) == 0))
                                {
                                    userAccount[i].SecureSync = Boolean.Parse(newIter.Current.Value);
                                }
                                else
                                {
                                    iFolderWindow.log.Info("Invalid value for {0}. Using default value ...", secureSyncXML);
                                }
                                break;
								/*
                            case passphraseXML:
                                userAccount[i].Passphrase = newIter.Current.Value;
                                break;
                            case rememberPassphraseXML:
                                if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                    (string.Compare(newIter.Current.Value, "false", true) == 0))
                                {
                                    userAccount[i].RememberPassphrase = Boolean.Parse(newIter.Current.Value);
                                }
                                else
                                {
                                    iFolderWindow.log.Info("Invalid value for {0}. Using default value...", rememberPassphraseXML);
                                }
                                break;
                            case recoveryAgentXML:
                                userAccount[i].RecoveryAgent = newIter.Current.Value;
                                break;
                                */
                            case promptXML:
                                if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                    (string.Compare(newIter.Current.Value, "false", true) == 0))
                                {
                                    userAccount[i].PromptForInvalidCert = Boolean.Parse(newIter.Current.Value);
                                }
                                else
                                {
                                    iFolderWindow.log.Info("Invalid value for {0}. Using default value...", promptXML);
                                }
                                break;
                        }
                    }
                    userAccount[i].Valid = true;
                    i++;

                }
                try
                {
                    iter = nav.Select("/"+autoAccountXML+"/"+generalPrefXML);
                    /*If there are any errors in parsing General preferences, use Default Values 
                     * for them and return true */
                    if( iter.Count != 1 )
                    {
                        iFolderWindow.log.Debug("Errors while parsing General prefrences. Count of genPrefs {0}", iter.Count );
                        return true;
                    }
                    iter.MoveNext();
                    newIter = iter.Current.SelectDescendants(XPathNodeType.Element, false );
                    while(newIter.MoveNext())
                    {
                        if ((string.Compare(newIter.Current.Value, "true", true) != 0) &&
                            (string.Compare(newIter.Current.Value, "false", true) != 0))
                        {
                            iFolderWindow.log.Info("Invalid value for {0}. Using default value..", newIter.Current.Name);
                            continue;
                        }
                        switch( newIter.Current.Name )
                        {
                            case iFolderCreationConfirmationXML:
                                iFolderCreationConfirmation = Boolean.Parse(newIter.Current.Value);
                                break;
                            case iFolderShareNotifyXML:
                                iFolderShareNotify = Boolean.Parse(newIter.Current.Value);
                                break;
                            case userJoinNotifyXML:
                                userJoinNotify = Boolean.Parse(newIter.Current.Value);
                                break;
                            case conflictNotifyXML:
                                conflictNotify = Boolean.Parse(newIter.Current.Value);
                                break;
                            case autoSyncXML:
				if (newIter.Current.MoveToFirstAttribute())
                                {
                                	syncInterval = Int32.Parse(newIter.Current.Value);                                        

	                                if (newIter.Current.MoveToNextAttribute())
    	                            {
        			                    //Get the interval and convert to seconds
                                        switch (newIter.Current.Value.ToLower())
                                        {
                    	                     case "seconds":
                        		                 break;
                                             case "minutes":
                                                 syncInterval *= 60;
                                                 break;
                                             case "hours":
                                                 syncInterval *= 3600;
                                                 break;
                                             case "days":
                                                 syncInterval *= 86400;
                                                 break;
                                             default:
                                                 iFolderWindow.log.Info("Improper value provided for auto sync duration. It must be either seconds or minutes or hours or days");
                                                 break;
                                        }
									}

                                    if (syncInterval < defaultSyncInterval)
                                    {
                                    	iFolderWindow.log.Info("Sync Interval value {0} is invalid. Using the default sync interval of {1} seconds.", syncInterval, defaultSyncInterval);
                                        syncInterval = defaultSyncInterval;
                                    }

                                    newIter.Current.MoveToParent();
								}   


                                break;
                        }
                    }
                }
                catch(Exception ee)
                {
                    iFolderWindow.log.Info("Error: {0}", ee.Message);
                    iFolderWindow.log.Debug("Exception {0}", ee.StackTrace);
                }
                
            }
            catch(XmlException xe )
            {
                iFolderWindow.log.Info("Error: {0}", xe.Message);
                iFolderWindow.log.Debug("XmlException {0}", xe.StackTrace);
                return false;
            }
            catch(Exception e)
            {
                iFolderWindow.log.Info("Error: {0}", e.Message);
                iFolderWindow.log.Debug("Exception {0}", e.StackTrace);
                return false;
            }
            return true;
        }

        /// <summary>
        /// logging
        /// </summary>
        void printAutoAccount()
        {
            for( int i = 0; i<userAccount.Length && userAccount[i] != null; i++)
            {
                iFolderWindow.log.Debug("User-Acccount Attribute {0}",userAccount[i].DefaultAccount);
                iFolderWindow.log.Debug("Server {0}",userAccount[i].Server);
                iFolderWindow.log.Debug("Username {0}",userAccount[i].UserName);
                iFolderWindow.log.Debug("Password {0}",userAccount[i].Password);
                iFolderWindow.log.Debug("Remember Password {0}",userAccount[i].RememberPassword);
                iFolderWindow.log.Debug("DefaultiFolder Attribute {0}",userAccount[i].NeedDefaultiFolder);
                iFolderWindow.log.Debug("DefaultiFolder Path {0}",userAccount[i].DefaultiFolderPath);
               // iFolderWindow.log.Debug("Passphrase {0}",userAccount[i].Passphrase);
                iFolderWindow.log.Debug("Encrypted {0}",userAccount[i].Encrypted);
               // iFolderWindow.log.Debug("Remember Passphrase {0}",userAccount[i].RememberPassphrase);
                iFolderWindow.log.Debug("Need Prompt {0}",userAccount[i].PromptForInvalidCert);
                iFolderWindow.log.Debug("-----------------------------------------------------");
            }
            iFolderWindow.log.Debug("iFolderCreationConfirmation {0}", iFolderCreationConfirmation);
            iFolderWindow.log.Debug("iFolderShareNotify {0}", iFolderShareNotify );
            iFolderWindow.log.Debug("userJoinNotify {0}", userJoinNotify);
            iFolderWindow.log.Debug("conflictNotify {0}", conflictNotify);
            iFolderWindow.log.Debug("syncInterval {0}", syncInterval);
            iFolderWindow.log.Debug("autoSync {0}", autoSync);
        }

        /// <summary>
        /// Checks if auto account creation is enabled or not
        /// </summary>
        /// <returns>status of parsing</returns>
        public ParseStatus IsAutoAccountCreationEnabled()
        {
            ParseStatus ret = ParseStatus.Success;
            bool found = false, wizardFound = false;
            iFolderWindow.log.Info("Source file path is {0}", sourceFilePath );
            if( File.Exists( sourceFilePath ) )
            {
                if( !parseResponseFile() )
                {
                    if( userAccount != null )
                    {
                        for( int i = 0; i<userAccount.Length && userAccount[i] != null; i++ )
                        {
                            if( userAccount[i].Valid )
                            {
                                found = true;
                                break;
                            }
                        }
                    }    
                    if( !found )
                        ret = ParseStatus.ParseError;
                }
                else 
                {
                    if( userAccount != null )
                    {
                        for( int i = 0; i<userAccount.Length ; i++ )
                        {
				/*
				bool containsChar = false;
				foreach (char ch in userAccount[i].Server)
				{
					if (ch < 48 || ch > 57 && ch != 46)
					{
						containsChar = true;
						break;
					}
				}
				if (!containsChar)
				{
					//string pattern = @"^([1-9]|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])(\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])){3}$";
					string pattern = @"^([1-9]|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])$";
					System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.ExplicitCapture);
					if (!reg.Match(userAccount[i].Server).Success)
					{
						userAccount[i].Valid = false;
						iFolderWindow.log.Error("Check for IP details of the Server{0} : Invalid IP address", userAccount[i].Server);
					}
				}
				else
				{
					try
					{
						IPHostEntry ifolderServer = System.Net.Dns.GetHostEntry(userAccount[i].Server);
					}
					catch (Exception ex)
					{
						userAccount[i].Valid = false;
						iFolderWindow.log.Error("Check for Server details: Server {0} is invalid & Error is {1}", userAccount[i].Server, ex.ToString());
					}
				}
				
											
				if (!userAccount[i].Valid)
				{
					try
					{
						userAccount[i].Valid = true;
						System.Net.Dns.GetHostEntry(userAccount[i].Server);
					}
					catch (Exception ex1)
					{
						userAccount[i].Valid = false;									 		  iFolderWindow.log.Error("Check for DNS Name:Invalid DNS Name {0} and Error is {1}", userAccount[i].Server, ex1.Message); 							   
					}
				}
				*/

				if( userAccount[i].Server != "" && userAccount[i].UserName != "" )
                            	{
                               		found = true;
                            	}
                            	else if( userAccount[i].Server != ""  && userAccount[i].UserName == "")
                            	{
                               		userAccount[i].Wizard = true;
                                	wizardFound = true;
                                	iFolderWindow.log.Info("Setting Wizard to true for account {0}", userAccount[i].Server);
                            	}
                        }
                        if( !found && !wizardFound )
                            ret = ParseStatus.IncompleteMandatoryFields;
                        else if( !found && wizardFound )
                            ret = ParseStatus.OnlyWizardAccounts;
                        else if( found && wizardFound )
                            ret = ParseStatus.AutoAndWizard;
                        
                        printAutoAccount();
                    }
                }
            }
            else
            {
                ret = ParseStatus.FileNotFound;
            }
            iFolderWindow.log.Info("ParseStatus result {0}", ret);
            return ret;
        }
       
        /// <summary>
        /// starts account wizard
        /// </summary>
        private void AddWizardAccount()
        {
            //printAutoAccount();
            AddAccountWizard aaw = null;
            for( int i = 0; i<userAccount.Length; i++ )
            {
                if(userAccount[i].Wizard)
                {
                    iFolderWindow.log.Info("Starting account wizard for {0}", userAccount[i].Server);
                    aaw = new AddAccountWizard(simws, userAccount[i].Server);
                    if (!Util.RegisterModalWindow(aaw))
                    {
                        iFolderWindow.log.Debug("Making modal window didn't succeed");
                        try
                        {
                            Util.CurrentModalWindow.Present();
                        }
                        catch(Exception e)
                        {
                            iFolderWindow.log.Debug("Not able to make modal window {0}", e.Message);
                        }
                    }
                    WizardStack.Push(aaw);
                    aaw.ShowAll();
                    iFolderWindow.log.Debug("Done with ShowAll");
                }
            }
            try
            {
                Object o = WizardStack.Pop();
                if(null != o)
                    iFolderWindow.log.Debug("Popped up successfully");
            }
            catch(Exception e)
            {
                iFolderWindow.log.Info("Error: {0}", e.Message);
            }
        }
        
        /// <summary>
        /// Either adds domain or starts account wizard
        /// </summary>
        /// <returns>true if successful</returns>
        public bool CreateAccounts()
        {
            ParseStatus ret = ParseStatus.Success;
            bool status = false;
            ret = IsAutoAccountCreationEnabled();
            switch(ret)
            {
                case ParseStatus.Success:
                    status = AddDomain();
                    break;
                case ParseStatus.AutoAndWizard:
                    status = AddDomain();
                    AddWizardAccount();
                    status = true;
                    break;
                case ParseStatus.OnlyWizardAccounts:
                    AddWizardAccount();
                    status = true;
                    break;
            }
            return status;
        }
        
        /// <summary>
        /// adds domain in the domain thread
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="acceptPassword"></param>
        /// <returns>true if successful</returns>
        private bool AddDomainHelper(UserAccount userAccount, bool acceptPassword)
        {
            bool status = false;
            if( userAccount.Password == "" && acceptPassword )
            {
                status = EnterPasswordDialog.ShowEnterPasswordDialog( userAccount );
                if( !status )
                    return status;
            }
            AddDomainThread addDomainThread =
                new AddDomainThread(
                    DomainController.GetDomainController(),
                    userAccount.Server,
                    userAccount.UserName,
                    userAccount.Password,
                    userAccount.RememberPassword,
                    userAccount.DefaultAccount);
            
            addDomainThread.AddDomainSerial();
            if( addDomainThread.Domain != null  )
                iFolderWindow.log.Info("Name {0}, host {1}", addDomainThread.Domain.Name, addDomainThread.Domain.Host );
            else
                iFolderWindow.log.Info("Domain is null");
            return OnAddDomainCompleted( addDomainThread );
        }
        
        /// <summary>
        /// calls adddomainhelper function to add the particular domain
        /// </summary>
        /// <returns>true even if one useraccount is added successfully</returns>
        public bool AddDomain()
        {
            int firstIndex = -1;
            bool status, atleastOneSuccess = false ;
            for( int i = 0; i < userAccount.Length ; i++)
            {
                if( userAccount[i].Valid && userAccount[i].Server != "" && userAccount[i].UserName != "" )
                {
                    if( firstIndex == -1 )
                    {
                        firstIndex = i;
                        userAccount[i].DefaultAccount = true;
                        iFolderWindow.log.Info("Setting the account {0} as default account", userAccount[i].Server );
                    }
                    status = AddDomainHelper(userAccount[i], true); 
                    if( status )
                    {
                        atleastOneSuccess = true;
                    }
                    else
                    {
                        if( firstIndex == i )
                        {
                            firstIndex = -1;
                        }
                    }
                }
            }
            return atleastOneSuccess;
        }

        /// <summary>
        /// Gets the user account
        /// </summary>
        /// <param name="server"></param>
        /// <returns>UserAccount object</returns>
        private UserAccount GetUserAccount( string server )
        {
            for( int i = 0; i < userAccount.Length ; i++)
            {
                if( userAccount[i].Server == server )
                    return userAccount[i];
            }
            return null;
        }

		/*
        private bool SetPassphrase( UserAccount acct )
        {
            bool status = false;
            string recoveryAgent = null, publicKey = null;
            DomainController domainController = DomainController.GetDomainController();

            try
            {
                Status passPhraseStatus;
                if( acct.RecoveryAgent == null || acct.RecoveryAgent == "" || ( string.Compare(acct.RecoveryAgent, "None", true) == 0 ) )
                {
                    recoveryAgent = null;
                    publicKey = null;
                }
                else
                {
                    string[] list = domainController.GetRAList(ConnectedDomain.ID);
                    if( list.Length >= 1 )
                    {
                        recoveryAgent = list[0];
                        byte [] RACertificateObj = domainController.GetRACertificate(ConnectedDomain.ID, recoveryAgent);
                        if( RACertificateObj != null && RACertificateObj.Length != 0)
                        {
                            System.Security.Cryptography.X509Certificates.X509Certificate Cert = new System.Security.Cryptography.X509Certificates.X509Certificate(RACertificateObj);
                            publicKey = Convert.ToBase64String(Cert.GetPublicKey());
                        }

                    }
                }
                passPhraseStatus = simws.SetPassPhrase( ConnectedDomain.ID, acct.Passphrase, recoveryAgent, publicKey);
                if(passPhraseStatus.statusCode == StatusCodes.Success)
                {
                    simws.StorePassPhrase( ConnectedDomain.ID, acct.Passphrase, CredentialType.Basic, acct.RememberPassphrase );
                    status = true;
                }
                else
                {
                    iFolderWindow.log.Info("Setting of passphrase {0} failed", acct.Passphrase );    
                }
            }
            catch(Exception e)
            {
                iFolderWindow.log.Info("SetPassphrase Error: {0}", e.Message );
                iFolderWindow.log.Debug("SetPassphrase exception {0}", e.StackTrace );
            }
            return status;
        }
        */
        
        /// <summary>
        /// Event handler to handler domain completed event
        /// </summary>
        /// <param name="addDomainThread"></param>
        /// <returns></returns>
        private bool OnAddDomainCompleted(AddDomainThread addDomainThread)
        {
            DomainInformation dom = addDomainThread.Domain;
            Exception e = addDomainThread.Exception;
            bool success = false;
            
            if (dom == null && e != null)
            {
                if (e is DomainAccountAlreadyExistsException)
                {
                    iFolderWindow.log.Info("An account for this server already exists on the local machine.  Only one account per server is allowed.");
                }
                else
                {
                    iFolderWindow.log.Info("An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.");
                }
                return false;
            }
            
            if (dom == null) // This shouldn't happen, but just in case...
            {
                iFolderWindow.log.Info("An unknown error occured");
                return false;
            }

            string serverName = addDomainThread.ServerName;
            UserAccount acct = GetUserAccount(serverName);
            iFolderWindow.log.Info("Processing User Account--> ServerName {0}, UserName {1}", serverName, acct.UserName );

            switch(dom.StatusCode)
            {
                case StatusCodes.InvalidCertificate:

                    byte[] byteArray = simws.GetCertificate(serverName);
                    System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(byteArray);
                   
                    /* If the AutoAccount XML file says that the user should not be prompted for invalid XML file
                       then, accept the certificate by default
                    */
                    if( !acct.PromptForInvalidCert )
                    {
                        iFolderWindow.log.Info( "Certificate prompt is disabled..." );
                        simws.StoreCertificate(byteArray, serverName);
                        success = AddDomainHelper(acct, false);
                        break;
                    }
                    
                    iFolderMsgDialog dialog = new iFolderMsgDialog(
                        null,
                        iFolderMsgDialog.DialogType.Question,
                        iFolderMsgDialog.ButtonSet.YesNo,
                        "",
                        Util.GS("Accept the certificate of this server?"),
                        string.Format(Util.GS("iFolder is unable to verify \"{0}\" as a trusted server.  You should examine this server's identity certificate carefully."), serverName),
                        cert.ToString(true));

                    Gdk.Pixbuf certPixbuf = Util.LoadIcon("gnome-mime-application-x-x509-ca-cert", 48);
                    if (certPixbuf != null && dialog.Image != null)
                        dialog.Image.Pixbuf = certPixbuf;

                    int rc = dialog.Run();
                    dialog.Hide();
                    dialog.Destroy();
                    if(rc == -8) // User clicked the Yes button
                    {
                        simws.StoreCertificate(byteArray, serverName);
                        success = AddDomainHelper(acct, false);
                    }
                    break;
                case StatusCodes.Success:
                case StatusCodes.SuccessInGrace:

                    string    password = addDomainThread.Password;
                    bool    bRememberPassword = addDomainThread.RememberPassword;
                    DomainController dc = DomainController.GetDomainController();

                    Status authStatus =
                            dc.AuthenticateDomain(
                            dom.ID, password, bRememberPassword);

                    if (authStatus != null)
                    {
                        if (authStatus.statusCode == StatusCodes.Success ||
                            authStatus.statusCode == StatusCodes.SuccessInGrace)
                        {
                            // We connected successfully!
                            ConnectedDomain = dom;
                            //TODO - Check for recoveryagent status.
                            
                            success = true;
                            iFolderWindow.log.Info( "Domain --> Server: {0} UserName: {1} <-- created successfully", acct.Server, acct.UserName ); 
                            
                            iFolderWebService ifws = DomainController.GetiFolderService();

                            int policy = ifws.GetSecurityPolicy(dom.ID);
                
                            if( ((policy & (int)Novell.iFolder.SecurityState.enforceEncryption) == (int) Novell.iFolder.SecurityState.enforceEncryption) )
                                encryptionEnforced = true;
                            
                            if( ((policy & (int)SecurityState.encryption) == (int) SecurityState.encryption) )
                                encryptionEnabled = true;
                                
                            bool defaultiFolderEncrypted = false;
                            iFolderWeb defaultiFolder = null;
                            string defaultiFolderID = simws.GetDefaultiFolder( ConnectedDomain.ID );
                            if( defaultiFolderID != null && defaultiFolderID != "")
                            {
                                iFolderWindow.log.Info("Default iFolder is already present. Needs to be downloaded");
                                iFolderData ifdata = iFolderData.GetData();
                                defaultiFolder = ifdata.GetDefaultiFolder( defaultiFolderID );
                                if( defaultiFolder.encryptionAlgorithm != null && defaultiFolder.encryptionAlgorithm != "")
                                {
                                    defaultiFolderEncrypted = true;
                                    iFolderWindow.log.Info("Default iFolder is encrypted");
                                }
                                else
                                {
                                    iFolderWindow.log.Info("Default iFolder is NOT encrypted");
                                }
                            }

                            /* Set the passphrase if the client has asked for it or if it has been enforced by server */
                            if( encryptionEnforced || ( acct.Encrypted && encryptionEnabled ))
                            {
                            	try
                            	{
                            		passphraseEntered = simws.IsPassPhraseSet(ConnectedDomain.ID);
                            	}
				catch(Exception)
				{
					iFolderWindow.log.Error("Passphrase is not set for creating default ifolder");
                       			return true; 
				}

				if(true == passphraseEntered)
				{
					string passphrasecheck = null;
					passphrasecheck = simws.GetPassPhrase(ConnectedDomain.ID);

					if (passphrasecheck == null || passphrasecheck == "")
                      			{
                      				passphraseEntered=iFolderWindow.ShowVerifyDialog(dom.ID,simws);
					}
					else
					{
						passphraseEntered = true;
					}
				}
				/*
                                iFolderWindow.log.Info( "Setting the passphrase..." );
                                if( acct.Passphrase != null && acct.Passphrase != "" )
                                {
                                    passphraseEntered = SetPassphrase( acct );
                                }*/
                                else
                                {
                                    //Accept and set the passphrase
                                    iFolderWindow.log.Info("Attempting to accept passphrase for acct {0}", acct.UserName );
                                    passphraseEntered = iFolderWindow.ShowEnterPassPhraseDialog(dom.ID, simws);
                                }    
                            }
							if(!passphraseEntered)
							{
								iFolderWindow.log.Info("Passphrase not supplied by user");
							}

                            /* We are done, if the Default iFolder creation is disabled */
                            if( !acct.NeedDefaultiFolder )
                            {
                                iFolderWindow.log.Info("Default iFolder Creation is disabled");
                                break;
                            }    
                                
                            if( defaultiFolderEncrypted && !passphraseEntered )
                            {
                                iFolderWindow.log.Info("Passphrase is not provided...Default iFolder Download failed");
                                break;
                            }
                            
                            if( defaultiFolderID == null || defaultiFolderID == "")
                            {
                                if((encryptionEnforced || (acct.Encrypted && encryptionEnabled)) && !passphraseEntered)
                                {
                                    iFolderWindow.log.Info("Passphrase is not provided...Default iFolder creation failed");
                                }
                                else
                                {
                                    CreateDefaultiFolder(acct);
                                }
                            }
                            else
                            {   
                                // download the default ifolder
                                if( defaultiFolder != null)
                                    DownloadiFolder( defaultiFolder, acct );
                                else
                                    iFolderWindow.log.Info("iFolder object is null");
                            }            
                        }
                        else
                        {
                            iFolderWindow.log.Info("Error while authenticating.. Error code {0}", authStatus.statusCode);
                        }
                    }
                    else
                    {
                        iFolderWindow.log.Info("Unknown error while authenticating..");
                    }
                    break;
                case StatusCodes.InvalidCredentials:
                    break;
                default:
                    iFolderWindow.log.Info("Failed to connect {0}", dom.StatusCode);
                    break;
            }
            return success;
        }
        
        /// <summary>
        /// Gets the default path
        /// </summary>
        /// <param name="upload"></param>
        /// <param name="username"></param>
        /// <returns>path</returns>
        private string GetDefaultPath(bool upload, string username )
        {
            string str = Mono.Unix.UnixEnvironment.EffectiveUser.HomeDirectory;
            str += "/ifolder/"+ConnectedDomain.Name+"/"+username;
            if( upload == true )
                return str+"/"+"Default";
            else
                return str;
        }
        /// <summary>
        /// Creates default iFolder
        /// </summary>
        /// <param name="acct"></param>
        /// <returns>true if successful</returns>
        private bool CreateDefaultiFolder( UserAccount acct )
        {
            iFolderData ifdata = iFolderData.GetData();
            iFolderHolder ifHolder = null;
            string path = acct.DefaultiFolderPath;
            if( "" == path || null == path )
            {
                path = GetDefaultPath(true, acct.UserName);
                iFolderWindow.log.Info( "Default iFolder creation path from Xml File is Null. Generated Default path is {0}", path );
            }
            else
            {
                iFolderWindow.log.Info( "Default iFolder creation path from Xml File is {0}", path);
            }

            try
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);
                dir.Create();
                if( encryptionEnforced || ( acct.Encrypted && encryptionEnabled ) )
                {
                	iFolderWindow.log.Info("Creating encrypted iFolder at {0}", path );
               		ifHolder = ifdata.CreateiFolder(path, ConnectedDomain.ID, acct.SecureSync,"BlowFish");
                }
                else
                {
                    iFolderWindow.log.Info("Creating unencrypted iFolder");
                    ifHolder = ifdata.CreateiFolder(path, ConnectedDomain.ID,acct.SecureSync, null);
                }    

                if( null == ifHolder )
                {
                    iFolderWindow.log.Info("iFolder creation failed");
                    return false;
                }
                else
                {
                    iFolderWindow.log.Info("Setting iFolder as Default iFolder");
                    simws.DefaultAccount(ConnectedDomain.ID, ifHolder.iFolder.ID);
                    return true;
                }
            }
            catch( Exception ex)
            {
                AddAccountWizard.DisplayCreateOrSetupException(ex, false);
                return false;
            }
        }

        /// <summary>
        /// Downloads the default iFolder
        /// </summary>
        /// <param name="defaultiFolder"></param>
        /// <param name="acct"></param>
        /// <returns>true if successful</returns>
        private bool DownloadiFolder( iFolderWeb defaultiFolder, UserAccount acct )
        {
            iFolderData ifdata = iFolderData.GetData();
            string path = acct.DefaultiFolderPath;
            
            if( "" == path || null == path )
            {
                path = GetDefaultPath(false, acct.UserName);
            }
            iFolderWindow.log.Info("Downloading default iFolder at path {0}",  path );
            try
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);
                dir.Create();
                iFolderWindow.log.Info("Downloading default iFolder at path {0}", path );
                ifdata.AcceptiFolderInvitation( defaultiFolder.ID, defaultiFolder.DomainID, path);
                return true;
            }
            catch(Exception ex)
            {
                AddAccountWizard.DisplayCreateOrSetupException(ex, false);
                return false;
            }
        }

        /// <summary>
        /// Sets default preferences such as sync
        /// </summary>
        /// <param name="ifws"></param>
        public void SetPreferences(iFolderWebService ifws)
        {
            int syncSeconds = syncInterval;
            
            ClientConfig.Set(ClientConfig.KEY_SHOW_CREATION, iFolderCreationConfirmation);
            ClientConfig.Set(ClientConfig.KEY_NOTIFY_IFOLDERS, iFolderShareNotify);
            ClientConfig.Set(ClientConfig.KEY_NOTIFY_USERS, userJoinNotify);
            ClientConfig.Set(ClientConfig.KEY_NOTIFY_COLLISIONS, conflictNotify);
            
            if( !autoSync || syncInterval <= 0 )
                syncSeconds = -1;
            
            try
            {
                ifws.SetDefaultSyncInterval(syncSeconds);
				Novell.iFolder.ClientConfig.Set(ClientConfig.KEY_SYNC_UNIT, "Seconds");
            }
            catch(Exception e)
            {
                iFolderWindow.log.Info("Error while disabling AutoSync: {0}", e.Message);
                iFolderWindow.log.Debug("StackTrace {0}", e.StackTrace );
            }
        }

    }
}
