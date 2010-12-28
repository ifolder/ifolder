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
using Novell.iFolder.Web;
using Simias.Client;
using Novell.iFolder;
using Novell.iFolderCom;
using System.Windows.Forms;
using System.Threading;
using Novell.FormsTrayApp;
using Novell.AutoAccountHelper;
using Novell.Wizard;

namespace Novell.AutoAccount
{
    public class AutoAccount
    {
        UserAccount[] userAccount;
        private bool iFolderCreationConfirmation = true, iFolderShareNotify = true, userJoinNotify = true, conflictNotify = true, autoSync = true;
        private int syncInterval = 0;
        private int defaultSyncInterval = 5;
        private SimiasWebService simws;
        private Manager simiasManager;
        private iFolderWebService ifWebService;
        private GlobalProperties globalProperties;
        private bool isValid;   //Holds XML validation thru schema
        private bool passPhraseStatus;
        private bool encryptionEnforced, encryptionEnabled;
        private bool upload;
        Novell.FormsTrayApp.FormsTrayApp formsTrayApp;

        const string sourceFileName = "AutoAccount.xml";
        const string sourceXsdName = "AutoAccount.xsd";
        readonly string sourceFilePath;
        readonly string sourceXsdPath;

        // The following are the tag/attribute names of AutoAccount.xml file
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
        const string forceMergeXML = "forcemerge";
        //const string passphraseXML = "passphrase";
        //const string rememberPassphraseXML = "remember-passphrase";
        //const string recoveryAgentXML = "recovery-agent";
        const string promptXML = "prompt-to-accept-cert";
        const string generalPrefXML = "general-preferences";
        const string iFolderCreationConfirmationXML = "iFolder-creation-confirmation";
        const string iFolderShareNotifyXML = "iFolder-share-notify";
        const string userJoinNotifyXML = "user-join-notify";
        const string conflictNotifyXML = "conflict-notify";
        const string autoSyncXML = "auto-sync";
        const string autoSyncIntervalXML = "interval";

        /// <summary>
        /// get/set length of user account
        /// </summary>
        public int AccountCount
        {
            get
            {
                return userAccount.Length;
            }
        }

        /// <summary>
        /// get/set the autoaccountfilepath
        /// </summary>
        public string AutoAccountFilePath
        {
            get
            {
                return sourceFilePath;
            }
        }

        /// <summary>
        /// get/set autoaccountfilename
        /// </summary>
        public string AutoAccountFileName
        {
            get
            {
                return sourceFileName;
            }
        }

        //Sets XML & XSD files' path
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ifws"></param>
        /// <param name="s"></param>
        /// <param name="mngr"></param>
        /// <param name="prop"></param>
        /// <param name="trayApp"></param>
        public AutoAccount(iFolderWebService ifws, SimiasWebService s, Manager mngr, GlobalProperties prop, Novell.FormsTrayApp.FormsTrayApp trayApp)
        {
            ifWebService = ifws;
            simws = s;
            simiasManager = mngr;
            sourceFilePath = Path.Combine(iFolderLogManager.LogConfDirPath, sourceFileName);
            sourceXsdPath = Path.Combine(SimiasSetup.prefix, sourceXsdName);
            globalProperties = prop;
            isValid = true;
            formsTrayApp = trayApp;
        }

        //Validates XML file
        /// <summary>
        /// whether this document is valid or not
        /// </summary>
        /// <returns>true/false</returns>
        bool IsValidDocument()
        {
            try
            {
                XmlSchemaSet sc = new XmlSchemaSet();
                sc.Add(null, sourceXsdPath);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = sc;
                settings.ValidationEventHandler += new ValidationEventHandler(MyValidationEventHandler);

                XmlReader reader = XmlReader.Create(sourceFilePath,settings);

                while (reader.Read()) ;
                reader.Close();
            }
            catch (XmlSchemaException ee)
            {
                isValid = false;
                Novell.FormsTrayApp.FormsTrayApp.log.Info("XML Schema Validation Error :" + ee.Message);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("LineNumber = {0}", ee.LineNumber);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("LinePosition = {0}", ee.LinePosition);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("StackTrace = {0}", ee.StackTrace);
            }
            catch (XmlException ee)
            {
                isValid = false;
                Novell.FormsTrayApp.FormsTrayApp.log.Info("Error :" + ee.Message);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("LineNumber = {0}", ee.LineNumber);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("LinePosition = {0}", ee.LinePosition);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("StackTrace = {0}", ee.StackTrace);
            }
            catch (Exception e)
            {
                isValid = false;
                Novell.FormsTrayApp.FormsTrayApp.log.Info("Error: " + e.Message);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("Source = {0}", e.Source);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("Type of Exception = {0}", e.GetType());
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("StackTrace = {0}", e.StackTrace);
            }

            return isValid;
        }

        
        /// <summary>
        /// Handle Validation event by logging error, line and position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void MyValidationEventHandler(object sender,
                                            ValidationEventArgs args)
        {
            isValid = false;
            Novell.FormsTrayApp.FormsTrayApp.log.Info("Validation Error: " + args.Message);
            Novell.FormsTrayApp.FormsTrayApp.log.Debug("LineNumber = {0}", args.Exception.LineNumber);
            Novell.FormsTrayApp.FormsTrayApp.log.Debug("LinePosition = {0}", args.Exception.LinePosition);
        }


        /// <summary>
        /// Parsing XML File ie get the details from file and hold in variables
        /// </summary>
        /// <returns></returns>
        public bool parseResponseFile()
        {
            int i = 0;
            try
            {
                if (!File.Exists(sourceXsdPath))
                {
                    Novell.FormsTrayApp.FormsTrayApp.log.Info("File {0} is not available for validation. Bypassing XSD validation", sourceXsdPath);
                }
                else
                {
                    if (!IsValidDocument()) //Check for XML Validity
                    {
                        Novell.FormsTrayApp.FormsTrayApp.log.Info("XML Schema Validation failed");
                        return false;
                    }
                    else
                    {
                        Novell.FormsTrayApp.FormsTrayApp.log.Info("XML Schema Validation passed");
                    }
                }

                XPathDocument xpd = new XPathDocument(sourceFilePath);
                XPathNavigator nav = ((IXPathNavigable)xpd).CreateNavigator();
                XPathNodeIterator iter = nav.Select("/" + autoAccountXML + "/" + userAccountXML);   //Get the Children "UserAccount" under root "Auto account" : See XML File
                XPathNodeIterator newIter = null;
                userAccount = new UserAccount[iter.Count];
                while (iter.MoveNext())
                {
                    userAccount[i] = new UserAccount();
                    newIter = iter.Current.SelectDescendants(XPathNodeType.Element, true);  //For descandents of useraccount
                    while (newIter.MoveNext())
                    {
                        switch (newIter.Current.Name)
                        {
                            case userAccountXML:
                                if (newIter.Current.MoveToFirstAttribute())
                                {
                                    //Check the attribute whether it is defaultAccount or not
                                    if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                        (string.Compare(newIter.Current.Value, "false", true) == 0))
                                    {
                                        userAccount[i].DefaultAccount = Boolean.Parse(newIter.Current.Value.ToLower());
                                    }
                                    else
                                    {
                                        Novell.FormsTrayApp.FormsTrayApp.log.Info("Invalid value for {0} attribute. Using default value...", userAccountXML);
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
                                    Novell.FormsTrayApp.FormsTrayApp.log.Debug(arg,"{0} argument is null", userIdXML);
                                }
                                
                                break;
                            case rememberPasswordXML:
                                if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                    (string.Compare(newIter.Current.Value, "false", true) == 0))
                                {
                                    userAccount[i].RememberPassword = Boolean.Parse(newIter.Current.Value.ToLower());
                                }
                                else
                                {
                                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Invalid value for {0}. Using default value...", rememberPasswordXML);
                                }
                                break;
                            case defaultiFolderXML:
                                if (newIter.Current.MoveToFirstAttribute())
                                {
                                    if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                        (string.Compare(newIter.Current.Value, "false", true) == 0))
                                    {
                                        userAccount[i].NeedDefaultiFolder = Boolean.Parse(newIter.Current.Value.ToLower());
                                    }
                                    else
                                    {
                                        Novell.FormsTrayApp.FormsTrayApp.log.Info("Invalid value for {0} attribute. Using default value...", defaultiFolderXML);
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
                                    Novell.FormsTrayApp.FormsTrayApp.log.Debug(arg,"{0} argument is null", pathXML);
                                }
                                
                                break;
                            case encryptedXML:
                                if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                    (string.Compare(newIter.Current.Value, "false", true) == 0))
                                {
                                    userAccount[i].Encrypted = Boolean.Parse(newIter.Current.Value.ToLower());
                                }
                                else
                                {
                                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Invalid value for {0}. Using default value...", encryptedXML);
                                }
                                break;
                                
                            case secureSyncXML:
                                if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                    (string.Compare(newIter.Current.Value, "false", true) == 0))
                                {
                                    userAccount[i].SecureSync = Boolean.Parse(newIter.Current.Value.ToLower());
                                }
                                else
                                {
                                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Invalid value for {0}. Using default value ...", secureSyncXML);
                                }
                                break;

                            case forceMergeXML:
                                if ((string.Compare(newIter.Current.Value, "true", true ) == 0) ||
                                    string.Compare(newIter.Current.Value, "false", true) == 0)
                                {
                                    userAccount[i].ForceMerge = Boolean.Parse(newIter.Current.Value.ToLower());
                                }
                                else
                                {
                                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Invalid value for {0}. Using default value ...", forceMergeXML);
                                }
                                break;

                                
                            /*case passphraseXML:
                                userAccount[i].Passphrase = newIter.Current.Value;
                                break;                             
                            case rememberPassphraseXML:
                                if ((string.Compare(newIter.Current.Value, "true", true) == 0) ||
                                    (string.Compare(newIter.Current.Value, "false", true) == 0))
                                {
                                    userAccount[i].RememberPassphrase = Boolean.Parse(newIter.Current.Value.ToLower());
                                }
                                else
                                {
                                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Invalid value for {0}. Using default value...", rememberPassphraseXML);
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
                                    userAccount[i].PromptForInvalidCert = Boolean.Parse(newIter.Current.Value.ToLower());
                                }
                                else
                                {
                                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Invalid value for {0}. Using default value...", promptXML);
                                }
                                break;
                        }
                    }
                    userAccount[i].Valid = true;
                    i++;
                }

                //Get General Preferences from XML
                try
                {
                    iter = nav.Select("/" + autoAccountXML + "/" + generalPrefXML);
                    /*If there are any errors in parsing General preferences, use Default Values 
                     * for them and return true */
                    if (iter.Count != 1)
                    {
                        Novell.FormsTrayApp.FormsTrayApp.log.Info("Errors while parsing General prefrences. Count of genPrefs {0}", iter.Count);
                        return true;
                    }
                    iter.MoveNext();
                    newIter = iter.Current.SelectDescendants(XPathNodeType.Element, false);
                    while (newIter.MoveNext())
                    {
                        if ((string.Compare(newIter.Current.Value, "true", true) != 0) &&
                            (string.Compare(newIter.Current.Value, "false", true) != 0))
                        {
                            Novell.FormsTrayApp.FormsTrayApp.log.Info("Invalid value for {0}. Using default value..", newIter.Current.Name);
                            continue;
                        }
                        switch (newIter.Current.Name)
                        {
                            case iFolderCreationConfirmationXML:
                                iFolderCreationConfirmation = Boolean.Parse(newIter.Current.Value.ToLower());
                                break;
                            case iFolderShareNotifyXML:
                                iFolderShareNotify = Boolean.Parse(newIter.Current.Value.ToLower());
                                break;
                            case userJoinNotifyXML:
                                userJoinNotify = Boolean.Parse(newIter.Current.Value.ToLower());
                                break;
                            case conflictNotifyXML:
                                conflictNotify = Boolean.Parse(newIter.Current.Value.ToLower());
                                break;
                            case autoSyncXML:
                                autoSync = Boolean.Parse(newIter.Current.Value.ToLower());
                                if (autoSync)
                                {
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
                                                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Improper value provided for auto sync duration. It must be either seconds or minutes or hours or days");
                                                    break;
                                            }
                                        }

                                        if (syncInterval < defaultSyncInterval)
                                        {
                                            Novell.FormsTrayApp.FormsTrayApp.log.Info("Sync Interval value {0} is invalid. Using the default sync interval of {1} seconds.", syncInterval, defaultSyncInterval);
                                            syncInterval = defaultSyncInterval;
                                        }

                                        newIter.Current.MoveToParent();
                                    }
                                }
                                else
                                    syncInterval = System.Threading.Timeout.Infinite;
                                
                                break;
                        }
                    }
                }
                catch (Exception ee)
                {
                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Error: {0}", ee.Message);
                    Novell.FormsTrayApp.FormsTrayApp.log.Debug("Exception {0}", ee.StackTrace);
                }
            }
            catch (XmlException xe)
            {
                Novell.FormsTrayApp.FormsTrayApp.log.Info("Error: {0}", xe.Message);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("XmlException {0}", xe.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                Novell.FormsTrayApp.FormsTrayApp.log.Info("Error: {0}", e.Message);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("Stacktrace is --> {0}", e.StackTrace);
                return false;
            }
            return true;
        }


        /// <summary>
        /// For debug purpose print the details
        /// </summary>
        void printAutoAccount()
        {
            int i = 0;
            for (i = 0; i < userAccount.Length && userAccount[i] != null; i++)
            {
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("User-Acccount Attribute {0}", userAccount[i].DefaultAccount);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("Server {0}", userAccount[i].Server);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("Username {0}", userAccount[i].UserName);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("Password {0}", userAccount[i].Password);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("Remember Password {0}", userAccount[i].RememberPassword);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("DefaultiFolder Attribute {0}", userAccount[i].NeedDefaultiFolder);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("DefaultiFolder Path {0}", userAccount[i].DefaultiFolderPath);
                //Novell.FormsTrayApp.FormsTrayApp.log.Debug("Passphrase {0}", userAccount[i].Passphrase);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("Encrypted {0}", userAccount[i].Encrypted);
                //Novell.FormsTrayApp.FormsTrayApp.log.Debug("Remember Passphrase {0}", userAccount[i].RememberPassphrase);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("Need Prompt {0}", userAccount[i].PromptForInvalidCert);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("-----------------------------------------------------");
            }
            Novell.FormsTrayApp.FormsTrayApp.log.Debug("iFolderCreationConfirmation {0}", iFolderCreationConfirmation);
            Novell.FormsTrayApp.FormsTrayApp.log.Debug("iFolderShareNotify {0}", iFolderShareNotify);
            Novell.FormsTrayApp.FormsTrayApp.log.Debug("userJoinNotify {0}", userJoinNotify);
            Novell.FormsTrayApp.FormsTrayApp.log.Debug("conflictNotify {0}", conflictNotify);
            Novell.FormsTrayApp.FormsTrayApp.log.Debug("syncInterval {0}", syncInterval);
            Novell.FormsTrayApp.FormsTrayApp.log.Debug("autoSync {0}", autoSync);
        }

        /// <summary>
        /// Check whether it is auto account creation, or account creation wizard
        /// </summary>
        /// <returns></returns>
        public ParseStatus IsAutoAccountCreationEnabled()
        {
            ParseStatus ret = ParseStatus.Success;
            bool found = false, wizardFound = false;
            Novell.FormsTrayApp.FormsTrayApp.log.Info("Source file path is {0}", sourceFilePath);
            if (File.Exists(sourceFilePath))
            {
                if (!parseResponseFile())//If response file is not parsed properly
                {
                     //Check for atleast one account is valid
                    for (int i = 0; userAccount != null && i < userAccount.Length && userAccount[i] != null; i++)
                    {
                        if (userAccount[i].Valid)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        ret = ParseStatus.ParseError;
                }
                else //if XML file is parsed properly
                {
                    if (userAccount != null)
                    {
                        for (int i = 0; i < userAccount.Length; i++)
                        {
                            /*
                            byte[] ipbytearray = System.Text.Encoding.ASCII.GetBytes(userAccount[i].Server);
                            System.Net.IPAddress ipad = new System.Net.IPAddress(ipbytearray);
                            System.Net.Dns.GetHostEntry(ipad);
                            */
                            //Check for valid IP Address
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
                                    Novell.FormsTrayApp.FormsTrayApp.log.Error("Check for IP details of the Server{0} : Invalid IP address", userAccount[i].Server);
                                }
                            }
                            else
                            {
                                try
                                {
                                    IPHostEntry ifolderServer = Dns.GetHostEntry(userAccount[i].Server);
                                }
                                catch (Exception ex)
                                {
                                    userAccount[i].Valid = false;
                                    Novell.FormsTrayApp.FormsTrayApp.log.Error("Check for Server details: Server {0} is invalid & Error is {1}", userAccount[i].Server, ex.ToString());
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
                                    userAccount[i].Valid = false;                                    
                                    Novell.FormsTrayApp.FormsTrayApp.log.Error("Check for DNS Name:Invalid DNS Name {0} and Error is {1}", userAccount[i].Server, ex1.Message);                                
                                }
                            }
                            */ 
                            //If server IP and username are available
                            if (userAccount[i].Server != "" && userAccount[i].UserName != "")
                            {
                                found = true;
                            }
                            //If server ip is no null and username is not available
                            else if (userAccount[i].Server != "" && userAccount[i].UserName == "")
                            {
                                userAccount[i].Wizard = true;
                                wizardFound = true;
                            }

                        }

                        if (!found && !wizardFound) //If auto account (IP and username available)
                            ret = ParseStatus.IncompleteMandatoryFields;
                        else if (!found && wizardFound)//if account creation wizard(IP available, no Username)
                            ret = ParseStatus.OnlyWizardAccounts;
                        else if (found && wizardFound)//Combination of both for some entries in XML
                            ret = ParseStatus.AutoAndWizard;

                        printAutoAccount();
                    }
                }
            }
            else
            {
                ret = ParseStatus.FileNotFound;
            }
            return ret;
        }


        /// <summary>
        /// Create account by using Connecting.cs
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="acceptPassword"></param>
        /// <returns></returns>
        private bool AddDomainHelper(UserAccount userAccount, bool acceptPassword)
        {
            bool status = false;
            do
            {
                string pwd = "";
                if (acceptPassword)
                {
                    status = EnterPasswordPopup.ShowEnterPasswordPopup(userAccount.Server, userAccount.UserName, out pwd, userAccount.RememberPassword);
                    if (status)
                    {
                        userAccount.Password = pwd;
                        //Novell.FormsTrayApp.FormsTrayApp.log.Info("Password accepted is {0} Status : {1}", userAccount.Password, status);
                    }
                    else 
                        break;
                }
                Connecting con = new Connecting(ifWebService, simws, simiasManager, userAccount.Server, userAccount.UserName, userAccount.Password, userAccount.DefaultAccount, userAccount.RememberPassword, userAccount.PromptForInvalidCert);
                con.EnterpriseConnect += new Connecting.EnterpriseConnectDelegate(AutoAccount_EnterpriseConnect);
                Novell.FormsTrayApp.FormsTrayApp.log.Debug("Invoking CreateDomain--> Server: {0} UserName: {1}", userAccount.Server, userAccount.UserName);
                status = CreateDomain(con);
                if (status == false)
                {
                    System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(Connecting));
                    MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("serverConnectError"),
                                resourceManager.GetString("serverConnectErrorTitle"),
                                "",
                                MyMessageBoxButtons.OK,
                                MyMessageBoxIcon.Error,
                                MyMessageBoxDefaultButton.Button1);
                    mmb.ShowDialog();
                }
            } while (status == false);
            Novell.FormsTrayApp.FormsTrayApp.log.Info("CreateDomain--> Server: {0} UserName: {1} Result: {2}", userAccount.Server, userAccount.UserName, status);
            return status;
        }

        /// <summary>
        /// If all entries in XML has IP and no username, we will arrive here
        /// </summary>
        private void AddWizardAccount()
        {
            bool firstAcct = false; //Only one defaultAccount is available on client
            DomainInformation[] domains;
            domains = this.simws.GetDomains(false);
            if (domains.Length.Equals(0))
                firstAcct = true;
            else
                firstAcct = false;
            Novell.FormsTrayApp.FormsTrayApp.log.Info("No. of domains {0}", domains.Length);
            for (int i = 0; i < userAccount.Length; i++)
            {
                if (userAccount[i].Wizard)
                {
                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Starting Account wizard for {0}, FirstAccount {1}", userAccount[i].Server, firstAcct);
                    bool result = ShowAccountWizard(firstAcct, userAccount[i].Server);
                    if (firstAcct && result)
                        firstAcct = false;
                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Done with Account wizard for {0}", userAccount[i].Server);
                }
            }
        }

        /// <summary>
        /// show account wizard
        /// </summary>
        /// <param name="firstAcct"></param>
        /// <param name="serverIP"></param>
        /// <returns></returns>
        public bool ShowAccountWizard(bool firstAcct, string serverIP)
        {
            try
            {
                AccountWizard accountWizard = new AccountWizard(ifWebService, simws, simiasManager, firstAcct, globalProperties.PreferenceDialog, serverIP);
                accountWizard.EnterpriseConnect += new Novell.Wizard.AccountWizard.EnterpriseConnectDelegate(AutoAccount_EnterpriseConnect);
                formsTrayApp.WizardStatus = true;
                DialogResult result = accountWizard.ShowDialog();
                formsTrayApp.WizardStatus = false;
                if (result == DialogResult.OK)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Novell.FormsTrayApp.FormsTrayApp.log.Info("ShowAccountWizard Error: {0}", ex.Message);
            }
            return false;
        }


        /// <summary>
        /// Entry point for AutoAccount Feature
        /// </summary>
        /// <returns></returns>
        public bool CreateAccounts()
        {
            ParseStatus ret = ParseStatus.Success;
            bool status = false;
            ret = IsAutoAccountCreationEnabled();
            switch (ret)
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
        /// If username and IP are available for an entry, use this to create a domain
        /// </summary>
        /// <returns></returns>
        private bool AddDomain()
        {
            int i = 0, firstIndex = -1; //Used to set whether defaultAccount or not
            bool status, atleastOneSuccess = false;
            formsTrayApp.WizardStatus = true;
            for (i = 0; i < userAccount.Length; i++)
            {
                if (userAccount[i].Valid && userAccount[i].Server != "" && userAccount[i].UserName != "")
                {
                    if (firstIndex == -1)
                    {
                        firstIndex = i;
                        userAccount[i].DefaultAccount = true;
                        //Novell.FormsTrayApp.FormsTrayApp.log.Info("Setting the account {0} as default account", userAccount[i].Server);
                    }
                    status = AddDomainHelper(userAccount[i], true);
                    if (status)
                    {
                        atleastOneSuccess = true;
                    }
                    else
                    {   //if cannot add a domain, reset firstIndex so that next account will be default account
                        if (firstIndex == i)
                        {
                            firstIndex = -1;
                        }
                    }
                    
                }
            }
            formsTrayApp.WizardStatus = false;
            return atleastOneSuccess;
        }

        /// <summary>
        /// Add the domain to the Array list on window to display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoAccount_EnterpriseConnect(object sender, DomainConnectEventArgs e)
        {
            globalProperties.AddDomainToList(e.DomainInfo);
            Novell.FormsTrayApp.FormsTrayApp.log.Debug("AutoAccount_EnterpriseConnect...");
        }

        /// <summary>
        /// Set General Preferences
        /// </summary>
        /// <returns></returns>
        public bool SetPreferences()
        {

            bool result = true;            
            System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(Preferences));

            Cursor.Current = Cursors.WaitCursor;

            // Check and update auto start setting.
            /*if (autoStart.Checked != IsRunEnabled())
            {
                setAutoRunValue(!autoStart.Checked);
            }*/


            Preferences.NotifyShareEnabled = iFolderShareNotify;
            Preferences.NotifyCollisionEnabled = conflictNotify;
            Preferences.NotifyJoinEnabled = userJoinNotify;

            // Check and update display confirmation setting.
            iFolderComponent.DisplayConfirmationEnabled = iFolderCreationConfirmation;

            try
            {
                // Check and update default sync interval.
                int currentInterval = ifWebService.GetDefaultSyncInterval();
                if ((syncInterval != currentInterval) ||
                   (autoSync != (currentInterval != System.Threading.Timeout.Infinite)))
                {
                    try
                    {
                        // Save the default sync interval.
                        ifWebService.SetDefaultSyncInterval(autoSync ? syncInterval : System.Threading.Timeout.Infinite);
                        Novell.FormsTrayApp.FormsTrayApp.log.Info("SyncInterval set value is {0}", ifWebService.GetDefaultSyncInterval());
                        if (autoSync)
                        {
                            // Update the displayed value.
                            //displaySyncInterval((int)syncValueInSeconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        Novell.FormsTrayApp.FormsTrayApp.log.Info("{0}:{1}", resourceManager.GetString("PreferencesErrorTitle"), resourceManager.GetString("saveSyncError"));
                        Novell.FormsTrayApp.FormsTrayApp.log.Info("Error: {0}", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                Novell.FormsTrayApp.FormsTrayApp.log.Info("{0} : {1}", resourceManager.GetString("PreferencesErrorTitle"), resourceManager.GetString("readSyncError"));
                Novell.FormsTrayApp.FormsTrayApp.log.Info("Error: {0}", ex.Message);
            }

            Cursor.Current = Cursors.Default;

            return result;
        }

        //For a domai, check for passphrase and set it.
        /*
        private bool SetPassphrase(Connecting con, string serverIP)
        {
            
            Status status = null;
            //string publicKey = null, recoveryAgent = null;
            bool retValue = false;
            UserAccount acct = GetUserAccount(serverIP);

            //Get the recoveryagent tag from file and check it:
            //If default, get the certificate, publickey, set the passphrase and store it.
            if (acct.RecoveryAgent == null || acct.RecoveryAgent == "" || (string.Compare(acct.RecoveryAgent, "None", true) == 0))
            {
                recoveryAgent = null;
                publicKey = null;
            }            
            else if (string.Compare(acct.RecoveryAgent, "Default", true) == 0)
            {
                string[] rAgents = simws.GetRAListOnClient(con.DomainInformation.ID);
                if (rAgents.Length >= 1)
                {
                    recoveryAgent = rAgents[0];
                    byte[] CertificateObj = this.simws.GetRACertificateOnClient(con.DomainInformation.ID, recoveryAgent);
                    System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(CertificateObj);
                    publicKey = Convert.ToBase64String(cert.GetPublicKey());
                }
            }
            
            try
            {
                status = simws.SetPassPhrase(con.DomainInformation.ID, acct.Passphrase, recoveryAgent, publicKey);
            }
            catch (Exception ex)
            {
                Novell.FormsTrayApp.FormsTrayApp.log.Info("IsPassphraseSetException ", ex.Message);
            }
            if (null != status && status.statusCode == StatusCodes.Success)
            {
                simws.StorePassPhrase(con.DomainInformation.ID, acct.Passphrase, CredentialType.Basic, acct.RememberPassphrase);
                retValue = true;
            }            
            return retValue;
           
            return false;
        }*/

        /// <summary>
        /// Get user account
        /// </summary>
        /// <param name="serv"></param>
        /// <returns>user account object</returns>
        private UserAccount GetUserAccount(string serv)
        {
            for (int i = 0; i < userAccount.Length; i++)
            {
                if (userAccount[i].Server == serv)
                    return userAccount[i];
            }
            return null;
        }

        /// <summary>
        /// Create the domain
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        public bool CreateDomain(Connecting con)
        {
            bool status = false, defaultiFolderEncrypted = false;
            string serverIP = string.Copy(con.ServerName);
            bool connectResult = con.initialConnect();
            string str = "";
            UserAccount acct = null;

            if (connectResult)
            {
                int policy = 0;
                Novell.FormsTrayApp.FormsTrayApp.log.Info("Domain creation succeeded.....");
                if (this.ifWebService != null)
                {
                    //Get Security policy for encryption
                    policy = this.ifWebService.GetSecurityPolicy(con.DomainInformation.ID);
                    if (((policy & (int)SecurityState.enforceEncryption) == (int)SecurityState.enforceEncryption))
                        encryptionEnforced = true;
                    if (((policy & (int)SecurityState.encryption) == (int)SecurityState.encryption))
                        encryptionEnabled = true;
                }

                iFolderWeb ifolder = null;
                str = simws.GetDefaultiFolder(con.DomainInformation.ID);    //Get the default iFolder. if null, u need to upload
                if (str == null || str == "")
                {
                    // Create iFolder
                    upload = true;
                }
                else
                {
                    ifolder = this.ifWebService.GetiFolder(str);
                    if (ifolder != null)
                    {
                        if (ifolder.encryptionAlgorithm == null || ifolder.encryptionAlgorithm == "")
                            defaultiFolderEncrypted = false;
                        else
                            defaultiFolderEncrypted = true;
                    }
                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Default iFolder Encryption status {0}", defaultiFolderEncrypted);
                }
                acct = GetUserAccount(serverIP);
                /* Set the passphrase if the client has asked for it or if it has been enforced by server */

                if (encryptionEnforced || (acct.Encrypted && encryptionEnabled))    //Get Passphrase
                {
                    /*
                    if (acct.Passphrase != null && acct.Passphrase != "")
                    {
                        Novell.FormsTrayApp.FormsTrayApp.log.Info("Passphrase supplied in XML file is {0}", acct.Passphrase);
                        passPhraseStatus = SetPassphrase(con,serverIP);
                    }
                    else
                    {
                     */
                        //Novell.FormsTrayApp.FormsTrayApp.log.Info("Passphrase not supplied in XML file");
                        //DialogResult rc;
                        /*EnterPassphraseDialog enterPassPhrase = new EnterPassphraseDialog(con.DomainInformation.ID, this.simws, false);
                        do
                        {
                            rc = enterPassPhrase.ShowDialog();
                        } while (rc == DialogResult.Cancel && !enterPassPhrase.DisposeDialog);*/
                        try
                        {
                            passPhraseStatus = simws.IsPassPhraseSet(con.DomainInformation.ID);
                        }
                        catch (Exception)
                        {
                            Novell.FormsTrayApp.FormsTrayApp.log.Error("Passphrase is not set");
                            return true;
                        }

                        if (passPhraseStatus == true)
                        {
                            // if passphrase not given during login
                            string passphrasecheck = null;
                            passphrasecheck = simws.GetPassPhrase(con.DomainInformation.ID);
                            if (passphrasecheck == null || passphrasecheck == "")
                            {
                                VerifyPassphraseDialog vpd = new VerifyPassphraseDialog(con.DomainInformation.ID, this.simws);
                                vpd.ShowDialog();
                                passPhraseStatus = vpd.PassphraseStatus;
                                vpd.Close();
                                vpd.Dispose();
                            }
                            else
                            {
                                passPhraseStatus = true;
                            }
                        }
                        else
                        {
                            // Passphrase not enterd at the time of login...
                            EnterPassphraseDialog enterPassPhrase;
                            if (ifWebService != null)
                                enterPassPhrase = new EnterPassphraseDialog(con.DomainInformation.ID, this.simws, ifWebService);
                            else
                                enterPassPhrase = new EnterPassphraseDialog(con.DomainInformation.ID, this.simws);
                            enterPassPhrase.ShowDialog();
                            passPhraseStatus = enterPassPhrase.PassphraseStatus;
                            enterPassPhrase.Close();
                            enterPassPhrase.Dispose();
                        }
                        /*
                        EnterPassphraseDialog enterPassPhrase = new EnterPassphraseDialog(con.DomainInformation.ID, this.simws);
                        rc = enterPassPhrase.ShowDialog();
                        passPhraseStatus = enterPassPhrase.PassphraseStatus;
                        */
                        if (!passPhraseStatus)
                            Novell.FormsTrayApp.FormsTrayApp.log.Info("Passphrase not supplied by user");
                        /*enterPassPhrase.Close();
                        enterPassPhrase.Dispose();*/
                    //}
                }
                    /*
                else
                {
                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Encryption not activated...");
                }*/

                if (!acct.NeedDefaultiFolder)
                {
                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Default iFolder Creation is disabled");
                    return true;
                }

                if (defaultiFolderEncrypted && !passPhraseStatus)
                {
                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Passphrase is not provided...Encrypted Default iFolder Download failed");
                    return true;
                }

                if (upload) //Upload default iFolder
                {
                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Trying to create default iFolder...");
                    if ((encryptionEnforced || (acct.Encrypted && encryptionEnabled)) && !passPhraseStatus)
                    {
                        Novell.FormsTrayApp.FormsTrayApp.log.Info("Passphrase is not provided...Encrypted Default iFolder Creation failed");
                    }
                    else
                    {
                        ifolder = CreateDefaultiFolder(con,serverIP);
                        if (ifolder != null)
                        {
                            status = true;
                            simws.DefaultAccount(con.DomainInformation.ID, ifolder.ID);
                        }
                        else
                        {
                            Novell.FormsTrayApp.FormsTrayApp.log.Info("Creation of default iFolder failed..");
                        }
                    }
                }
                else
                {
                    // Download iFolder
                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Trying to download default iFolder...");
                    ifolder = this.ifWebService.GetiFolder(str);
                    if (ifolder != null)
                    {
                        status = DownloadiFolder(ifolder, con, serverIP);
                        Novell.FormsTrayApp.FormsTrayApp.log.Info("Download iFolder result {0}", status);
                    }
                }
            }

            return connectResult;
        }

        /// <summary>
        /// Getdefaultpath 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="domainName"></param>
        /// <returns>string , full path</returns>
        private string GetDefaultPath(string userName, string domainName)
        {
            System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));
            string appdata = System.Environment.GetEnvironmentVariable("APPDATA");
            int i = appdata.LastIndexOf("\\");
            appdata = appdata.Substring(0, i);
            appdata = appdata + Resource.GetString("ifolderDirText") + "\\" + domainName + "\\" + userName;
            return appdata;
        }

        /// <summary>
        /// created the default ifolder
        /// </summary>
        /// <param name="con"></param>
        /// <param name="serverIP"></param>
        /// <returns>ifolderweb object after successful creation</returns>
        private iFolderWeb CreateDefaultiFolder(Connecting con, string serverIP)
        {
            UserAccount acct = GetUserAccount(serverIP);
            string path = acct.DefaultiFolderPath;
            iFolderWeb ifolder = null;

            if (path == null || path == "")
            {
                path = GetDefaultPath(acct.UserName, con.DomainInformation.Name);
                Novell.FormsTrayApp.FormsTrayApp.log.Info("Default iFolder creation path from Xml File is Null. Generated Default path is {0}", path);
            }
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                di.Create();
                                
                if (encryptionEnforced || (acct.Encrypted && encryptionEnabled))
                {
                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Creating encrypted default iFolder--> path : {0}, id {1}", path, con.DomainInformation.ID);                    
                    ifolder = this.ifWebService.CreateiFolderInDomainEncr(path, con.DomainInformation.ID, acct.SecureSync, "BlowFish", simws.GetPassPhrase(con.DomainInformation.ID));                        
                    
                }
                else
                {
                    Novell.FormsTrayApp.FormsTrayApp.log.Info("Creating un-encrypted iFolder--> path : {0}, id {1}", path, con.DomainInformation.ID);
                    ifolder = this.ifWebService.CreateiFolderInDomainEncr(path, con.DomainInformation.ID, acct.SecureSync, null, null);                 
                }
            }
            catch (Exception ex)
            {
                // Unable to create the folder
                DisplayErrorMesg(ex);
                return null;
            }

            return ifolder;
        }

        /// <summary>
        /// Download the ifolder
        /// </summary>
        /// <param name="defaultiFolder"></param>
        /// <param name="con"></param>
        /// <param name="serverIP"></param>
        /// <returns>true/false</returns>
        private bool DownloadiFolder(iFolderWeb defaultiFolder, Connecting con, string serverIP)
        {
            UserAccount acct = GetUserAccount(serverIP);
            string path = acct.DefaultiFolderPath;
            if (path == null || path == "")
            {
                path = GetDefaultPath(acct.UserName, con.DomainInformation.Name);
                Novell.FormsTrayApp.FormsTrayApp.log.Info("Default iFolder creation path from Xml File is Null. Generated Default path is {0}", path);
            }
            Novell.FormsTrayApp.FormsTrayApp.log.Info("Default iFolder download path is {0}", path);

            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                if (di.Name == defaultiFolder.Name)
                {
                    path = Directory.GetParent(path).ToString();
                    di = new DirectoryInfo(path);
                }
                di.Create();
                iFolderWeb ifolder = null;
                if (!acct.ForceMerge)
                    ifolder = this.ifWebService.AcceptiFolderInvitation(defaultiFolder.DomainID, defaultiFolder.ID, path);
                else
                    ifolder = this.ifWebService.MergeiFolder(defaultiFolder.DomainID, defaultiFolder.ID, Path.Combine(path,defaultiFolder.Name));

                globalProperties.AddToAcceptedFolders(ifolder);
            }
            catch (Exception ex)
            {
                DisplayErrorMesg(ex);
                return false;
            }
         
            return true;
        }

        /// <summary>
        /// Display the error message
        /// </summary>
        /// <param name="ex"></param>
        private void DisplayErrorMesg(Exception ex)
        {
            string message;
            System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(CreateiFolder));
            System.Resources.ResourceManager resourcemanager = new System.Resources.ResourceManager(typeof(GlobalProperties));
            string caption = resourceManager.GetString("pathInvalidErrorTitle");

            if (ex.Message.IndexOf("InvalidCharactersPath") != -1)
            {
                message = resourceManager.GetString("invalidCharsError");
            }
            else if (ex.Message.IndexOf("AtOrInsideStorePath") != -1)
            {
                message = resourceManager.GetString("pathInStoreError");
            }
            else if (ex.Message.IndexOf("ContainsStorePath") != -1)
            {
                message = resourceManager.GetString("pathContainsStoreError");
            }
            else if (ex.Message.IndexOf("SystemDirectoryPath") != -1)
            {
                message = resourceManager.GetString("systemDirError");
            }
            else if (ex.Message.IndexOf("SystemDrivePath") != -1)
            {
                message = resourceManager.GetString("systemDriveError");
            }
            else if (ex.Message.IndexOf("IncludesWinDirPath") != -1)
            {
                message = resourceManager.GetString("winDirError");
            }
            else if (ex.Message.IndexOf("IncludesProgFilesPath") != -1)
            {
                message = resourceManager.GetString("progFilesDirError");
            }
            else if (ex.Message.IndexOf("ContainsCollectionPath") != -1)
            {
                message = resourceManager.GetString("containsiFolderError");
            }
            else if (ex.Message.IndexOf("AtOrInsideCollectionPath") != -1)
            {
                message = resourceManager.GetString("pathIniFolderError");
            }
            else if (ex.Message.IndexOf("RootOfDrivePath") != -1)
            {
                message = resourceManager.GetString("rootDriveError");
            }
            else if (ex.Message.IndexOf("NotFixedDrivePath") != -1)
            {
                message = resourceManager.GetString("networkPathError");
            }
            else if (ex.Message.IndexOf("PathExists") != -1)
            {
                message = resourcemanager.GetString("pathExistsError");
            }
            else if (this.upload == true)
            {
                message = resourceManager.GetString("iFolderCreateError");
            }
            else
            {
                message = resourceManager.GetString("acceptError");
            }

            Novell.FormsTrayApp.FormsTrayApp.log.Info("{0} : {1}", caption, message);

        }
    }
}
