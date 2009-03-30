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
*                 $Author: Kalidas Balakrishnan <bkalidas@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Novell.iFolder.Utility;


namespace Novell.iFolder.Utility
{
    /// <summary>
    /// Key Recovery class
    /// </summary>
    class KeyRecovery
    {
        #region MemberVariables
        public RSACryptoServiceProvider rsadec;
#if DBUG
        public RSACryptoServiceProvider rsaenc;
#endif //DEBUG
        private string titleTag = "CryptoKeyRecovery";
        private string CollectionIDTag = "iFolderCollection";
        private string iFolderIDTag = "iFolderID";
        private string KeyTag = "Key";
        public Option keyPath = new Option("key-path,kp", "Private Key", "Path to the Private key file (.p12 format)", true, null);
        public Option inpath = new Option("input-path,ip", "Encrypted Key file path", "Path to the Encrypted key file", true, null);
        public Option outpath = new Option("output-path,op", "Decrypted Key file path", "Path to the Decrypted key file", true, null);
        public Option pvkPass = new Option("private-pass,pp", "Private Key Password", "Password to decrypt the Private key", true, null);
        public BoolOption OTP = new BoolOption("onetime,ot", "Encrypt result key", "Encrypt the decrypted key with one time passpharse", true, true);
        public Option OTPass = new Option("onetime-pass,s", "One Time Password", "Enter One Time passphrase to Encrypt with", true, null);
        public NoPromptOption prompt = new NoPromptOption("prompt", "Prompt For Options", "Prompt the user for missing options", false, null);
        public NoPromptOption help = new NoPromptOption("help,?", "Usage Help", "Show This Screen", false, null);
        string[] args = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Contructor for the class.
        /// </summary>
        /// <param name="cmdargs">command line arguments</param>
        KeyRecovery(string[] cmdargs)
        {
            args = cmdargs;
        }
        #endregion

        /// <summary>
        /// Main Function
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
            string certname = "";
            string pass = "";
            KeyRecovery prg = new KeyRecovery(args);
            prg.OTP.OnOptionEntered = new Option.OptionEnteredHandler(prg.OnOTP);
            prg.ParseArguments();
            certname = Path.GetFullPath(prg.keyPath.Value);
            pass = prg.pvkPass.Value;
            X509Certificate2 xcert;
            try
            {
                if (pass.Length > 0)
                    xcert = new X509Certificate2(certname, pass);
                else
                    throw new ArgumentNullException("pass");
                prg.rsadec = xcert.PrivateKey as RSACryptoServiceProvider;
#if DBUG
            string locmess = "Welcome to iFolder 3.6";
            string locCert = "e:/Data/ifolder/arul_1024_2.der";
            string encmess;
            byte [] expo = {1,0,1};
            byte[] pubke;
            byte[] p12pubkey;
            RSAParameters rsa = new RSAParameters();
            X509Certificate enccert = X509Certificate.CreateFromCertFile(locCert);
            //X509Certificate2 enccert = new X509Certificate2(locCert);
            RSAParameters rsaprv = prg.rsadec.ExportParameters(false);
            //prg.rsaenc = enccert.PublicKey.Key as RSACryptoServiceProvider;
            //RSAParameters rsapub = prg.rsaenc.ExportParameters(false);
            
            pubke = enccert.GetPublicKey();
            p12pubkey = xcert.GetPublicKey();
            rsa.Modulus = new byte[128];
            Array.Copy(pubke, 7, rsa.Modulus, 0, 128);
            rsa.Exponent = expo;
            prg.rsaenc = new RSACryptoServiceProvider();
            prg.rsaenc.ImportParameters(rsa);
            
            Console.WriteLine("Public Key String: {0}", Convert.ToBase64String(xcert.GetPublicKey()));
            Console.WriteLine("Public Key String: {0}", Convert.ToBase64String(enccert.GetPublicKey()));
            //Console.WriteLine("Public Key String: {0}", enccert.ToString(true));
            //Console.WriteLine("Public Key String: {0}", xcert.ToString(true));
            Console.WriteLine("Public Key String Stripped: {0}", Convert.ToBase64String(rsa.Modulus));
            Console.WriteLine("Public Key String2 Stripped: {0}", Convert.ToBase64String(rsaprv.Modulus));
            Console.WriteLine("Enc mess {0}:", encmess = Convert.ToBase64String(prg.rsaenc.Encrypt(Encoding.UTF8.GetBytes(locmess), false)));
            Console.WriteLine("Denc mess {0}:", Encoding.UTF8.GetString(prg.rsadec.Decrypt(Convert.FromBase64String(encmess), false)));
#endif //DBG
                prg.ProcessInputKeyFile();
            }
            catch (Exception exp)
            {
                if(pass == null)
                    Console.WriteLine("Private Key Password Null Exception ");
                else
                    Console.WriteLine("Exception {0}", exp.Message);
            }
        }

        #region Arguments

        /// <summary>
        /// Parse the Command-Line Arguments
        /// </summary>
        void ParseArguments()
        {
            if (args.Length == 0)
            {
                // prompt
                Prompt.CanPrompt = true;
                prompt.Value = true.ToString();
                PromptForArguments();
            }
            else
            {
                // parse arguments
                Options.ParseArguments(this, args);

                // help
                if (help.Assigned)
                {
                    ShowUsage();
                }

                if (prompt.Assigned)
                {
                    Prompt.CanPrompt = true;
                    PromptForArguments();
                }
                else
                {
#if DBUG
					// show options for debugging
					Options.WriteOptions( this, Console.Out );
					Console.WriteLine();
#endif
                    // check for required options
                    Options.CheckRequiredOptions(this);
                }
            }
        }

        /// <summary>
        /// Prompt for Arguments
        /// </summary>
        void PromptForArguments()
        {
            Console.WriteLine();

            Option[] options = Options.GetOptions(this);
            foreach (Option option in options)
            {
                Prompt.ForOption(option);
            }

            Console.WriteLine();
            Console.WriteLine("Working...");
            Console.WriteLine();
        }

        #endregion

        #region Help
        /// <summary>
        /// Method to show HELP
        /// </summary>
        private void ShowUsage()
        {
            Console.WriteLine("USAGE: key_converter <Path to Private Key file> <Password for Key file> <Path to Encrypted Key file> <Path to Decrypted key file>");
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine();

            Option[] options = Options.GetOptions(this);

            foreach (Option o in options)
            {
                int nameCount = 0;
                foreach (string name in o.Names)
                {
                    Console.Write("{0}--{1}", nameCount == 0 ? "\n\t" : ", ", name);
                    nameCount++;
                }

                // Format the description.
                string description = o.Description == null ? o.Title : o.Description;
                Regex lineSplitter = new Regex(@".{0,50}[^\s]*");
                MatchCollection matches = lineSplitter.Matches(description);
                Console.WriteLine();
                if (o.Required)
                    Console.WriteLine("\t\t(REQUIRED)");
                foreach (Match line in matches)
                {
                    Console.WriteLine("\t\t{0}", line.Value.Trim());
                }
            }

            Console.WriteLine();

            Environment.Exit(-1);
        }
        #endregion

        #region MemberFunctions
        /// <summary>
        /// One Time Passphrase 
        /// </summary>
        /// <returns>Returns true or false based on One Time Passphrase is required or not</returns>
        private bool OnOTP()
        {
            if (OTP.Value == false)
            {
                OTPass.Required = false;
                OTPass.Prompt = false;
            }
            return true;
        }

        /// <summary>
        /// Process The Input Kep File
        /// </summary>
        private void ProcessInputKeyFile()
        {
            string strKey = string.Format("//{0}/{1}", CollectionIDTag, KeyTag);
            string strID = string.Format("//{0}/{1}", CollectionIDTag, iFolderIDTag);
            string decKey;
            byte[] decKeyByteArray;
            try
            {
                string inKeyPath = Path.GetFullPath(inpath.Value);
                string outKeyPath = Path.GetFullPath(outpath.Value);
                XmlDocument encFile = new XmlDocument();
                encFile.Load(inKeyPath);
                XmlNodeList keyNodeList, idNodeList;

                XmlElement root = encFile.DocumentElement;

                keyNodeList = root.SelectNodes(strKey);
                idNodeList = root.SelectNodes(strID);

                XmlDocument document = new XmlDocument();
                XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
                document.InsertBefore(xmlDeclaration, document.DocumentElement);
                XmlElement title = document.CreateElement(titleTag);
                document.AppendChild(title);
                int i = 0;
                foreach (XmlNode idNode in idNodeList)
                {
                    if (idNode.InnerText == null || idNode.InnerText == String.Empty)
                        continue;
                    Console.WriteLine(idNode.InnerText);
                    XmlNode newNode = document.CreateNode("element", CollectionIDTag, "");
                    newNode.InnerText = "";
                    document.DocumentElement.AppendChild(newNode);
                    XmlNode innerNode = document.CreateNode("element", iFolderIDTag, "");
                    innerNode.InnerText = idNode.InnerText;
                    newNode.AppendChild(innerNode);
                    {
                        XmlNode keyNode = keyNodeList[i++];
                        Console.WriteLine(decKey = keyNode.InnerText);
                        decKeyByteArray = Convert.FromBase64String(decKey);
                        XmlNode newElem2 = document.CreateNode("element", KeyTag, "");
                        if (decKey == null || decKey == String.Empty)
                            continue;
                        if (OTP.Value == true)
                            newElem2.InnerText = DecodeMessage(decKeyByteArray, OTPass.Value);
                        else
                            newElem2.InnerText = DecodeMessage(decKeyByteArray);
                        newNode.AppendChild(newElem2);
                    }
                }
                if (File.Exists(outKeyPath))
                    File.Delete(outKeyPath);
                document.Save(outKeyPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while processing" + e.Message + e.StackTrace);
            }
        }

        /// <summary>
        /// Message Decoder
        /// </summary>
        /// <param name="encmess">Byte stream which needs to be decoded</param>
        /// <returns>decoded string from te byte stream</returns>
        private string DecodeMessage(byte[] encmess)
        {
            string mess = null;
            try
            {
                mess = Convert.ToBase64String(rsadec.Decrypt(encmess, false));
#if DBUG
                Console.WriteLine("encrypted mess {0}", Convert.ToBase64String(rsadec.Decrypt(rsaenc.Encrypt(Convert.FromBase64String(mess), false), false)));
#endif
            }
            catch (CryptographicException cExp)
            {
                Console.WriteLine("Crpto Error {0}", cExp.Message);
            }
            return mess;
        }

        /// <summary>
        /// Decoder of Byte Stream to a string with One time  PP
        /// </summary>
        /// <param name="encmess">A byte stream which needs to be decoded</param>
        /// <param name="otpass">One time PP</param>
        /// <returns>Decoded string</returns>
        private string DecodeMessage(byte[] encmess, string otpass)
        {
            string retStr = null;
            byte[] mess;
            try
            {
                mess = rsadec.Decrypt(encmess, false);
                TripleDESCryptoServiceProvider tdesp = new TripleDESCryptoServiceProvider();
                byte[] IV ={ 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
                byte[] input = HashPassPhrase(otpass);
                tdesp.KeySize = input.Length * 8;
                byte[] output = Transform(mess, tdesp.CreateEncryptor(input, IV));
                retStr = Convert.ToBase64String(output);
            }
            catch (CryptographicException cryExp)
            {
                Console.WriteLine("Crpto Error {0}", cryExp.Message);
            }
            return retStr;
        }

        /// <summary>
        /// Hash PassPhrase
        /// </summary>
        /// <param name="Passphrase">Passphrase which needs to be hashed - string</param>
        /// <returns> A byte stream - hashed PP</returns>
        private byte[] HashPassPhrase(string Passphrase)
        {
            /*change to PasswordDeriveBytes.CryptDeriveKey once the  implementation is done mono

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Passphrase, salt);
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = pdb.CryptDeriveKey("TripleDES", "SHA1", 192, tdes.IV);
            //tdes.Key is the NewPassphrase
			
            */
            byte[] NewPassphrase = null;
            byte[] salt ={ 0x49, 0x46, 0x4F, 0x4C, 0x44, 0x45, 0x52 };
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] data = utf8.GetBytes(Passphrase);
            try
            {
                HMACSHA1 sha1 = new HMACSHA1();
                sha1.Key = salt;
                for (int i = 0; i < 1000; i++)
                {
                    sha1.ComputeHash(data);
                    data = sha1.Hash;
                }
                NewPassphrase = new byte[data.Length + 4]; //20+4
                Array.Copy(data, 0, NewPassphrase, 0, data.Length);
                Array.Copy(data, 0, NewPassphrase, 20, 4);
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception {0}", exp.Message);
            }

            return NewPassphrase;
        }

        /// <summary>
        /// Transform a byte stream to a crypto byte stream
        /// </summary>
        /// <param name="input">A byte stream which needs to be transformed</param>
        /// <param name="CryptoTransform">Defines the basic opeartion of Crypting</param>
        /// <returns>A transformed byte stream</returns>
        private byte[] Transform(byte[] input, ICryptoTransform CryptoTransform)
        {
            byte[] result = null;
            try
            {
                // create the necessary streams
                MemoryStream memStream = new MemoryStream();
                CryptoStream cryptStream = new CryptoStream(memStream, CryptoTransform, CryptoStreamMode.Write);
                // transform the bytes as requested
                cryptStream.Write(input, 0, input.Length);
                cryptStream.FlushFinalBlock();
                // Read the memory stream and
                // convert it back into byte array
                memStream.Position = 0;
                result = memStream.ToArray();
                // close and release the streams
                memStream.Close();
                cryptStream.Close();
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception {0}", exp.Message);
            }
            // hand back the encrypted buffer
            return result;
        }
        #endregion

#if DBG 
        private byte[] EncodeMessage(string mess)
        {
            byte[] encMess;
            encMess = rsaenc.Encrypt(Encoding.UTF8.GetBytes(mess), false);
            return encMess;
        }
#endif //DEBUG
    }
}
