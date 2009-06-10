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
*                 $Author: Abhilash<gabhilash@novell.com>
*                 $Modified by: 
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.IO;


namespace Novell.Wizard
{
    class Inner_keyRecovery
    {
        public RSACryptoServiceProvider rsadec;
        private string titleTag = "CryptoKeyRecovery";
        private string CollectionIDTag = "iFolderCollection";
        private string iFolderIDTag = "iFolderID";
        private string KeyTag = "Key";
        public Inner_keyRecovery()
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        /// <param name="pass"></param>
        /// <param name="xcert"></param>
        /// <param name="flag"></param>
        /// <param name="oneTimePassword"></param>
        public void ProcessInputKeyFile(string inputPath, string outputPath, string pass,X509Certificate2 xcert,bool flag,string oneTimePassword )
        {
            string strKey = string.Format("//{0}/{1}", CollectionIDTag, KeyTag);
            string strID = string.Format("//{0}/{1}", CollectionIDTag, iFolderIDTag);
            string decKey;
            byte[] decKeyByteArray;

            rsadec = xcert.PrivateKey as RSACryptoServiceProvider;
            try
            {
                string inKeyPath = Path.GetFullPath(inputPath);
                string outKeyPath = Path.GetFullPath(outputPath);
                XmlDocument encFile = new XmlDocument();
                encFile.Load(inKeyPath);
                XmlNodeList keyNodeList, idNodeList;
                
                XmlElement root = encFile.DocumentElement;

                keyNodeList = root.SelectNodes(strKey);
                idNodeList = root.SelectNodes(strID);

                System.Xml.XmlDocument document = new XmlDocument();
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
                        if (flag == true)
                            newElem2.InnerText = DecodeMessage(decKeyByteArray, oneTimePassword);
                        else
                            newElem2.InnerText = DecodeMessage(decKeyByteArray);
                        newNode.AppendChild(newElem2);
                    }
                }
                if (File.Exists(outKeyPath))
                    File.Delete(outKeyPath);
                document.Save(outKeyPath);
            }
            catch (Exception)
            {
                //Console.WriteLine("Exception while processing" + e.Message + e.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encmess"></param>
        /// <returns></returns>
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
            catch (CryptographicException)
            {
                //Console.WriteLine("Crpto Error {0}", cExp.Message);
            }
            return mess;
        }

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
            catch (CryptographicException)
            {
                //Console.WriteLine("Crpto Error {0}", cryExp.Message);
            }
            return retStr;
        }

        private byte[] HashPassPhrase(string Passphrase)
        {
           
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
            catch (Exception)
            {
                //Console.WriteLine("Exception {0}", exp.Message);
            }

            return NewPassphrase;
        }

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
            catch (Exception)
            {
               // Console.WriteLine("Exception {0}", exp.Message);
            }
            // hand back the encrypted buffer
            return result;
        }
    }
}
