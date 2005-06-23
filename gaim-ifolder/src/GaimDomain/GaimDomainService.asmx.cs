/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
 *  Author(s):
 *		Boyd Timothy <btimothy@novell.com>
 *		Rob (this code is a mostly copy-n-paste from him)
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

using Simias;
using Simias.Client;
using Simias.Domain;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;

namespace Simias.Gaim.DomainService
{
	/// <summary>
	/// Domain Service
	/// </summary>
	[WebService(
		Namespace="http://novell.com/simias/domain",
		Name="Gaim Domain Service",
		Description="Web Service providing access to Gaim Domain Server functionality.")]
	public class GaimDomainService : System.Web.Services.WebService
	{
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructor
		/// </summary>
		public GaimDomainService()
		{
		}
		
		/// <summary>
		/// Get domain information
		/// </summary>
		/// <param name="userID">The user ID of the member requesting domain information.</param>
		/// <returns>A GaimDomainInfo object that contains information about the enterprise server.</returns>
		/// 
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public GaimDomainInfo GetGaimDomainInfo()
		{
			// domain
			Simias.Storage.Domain domain = GaimDomain.GetDomain();
			if ( domain == null )
			{
				throw new SimiasException( "Gaim domain does not exist" );
			}

			GaimDomainInfo info = new GaimDomainInfo();
			info.ID = domain.ID;
			info.Name = domain.Name;
			info.Description = domain.Description;
			
			return info;
		}

		/// <summary>
		/// Pokes the Synchronization Thread to run/update
		/// </summary>
		[WebMethod(Description="SynchronizeMemberList", EnableSession=true)]
		[SoapDocumentMethod]
		public void SynchronizeMemberList()
		{
			log.Debug("GaimDomainService.SynchronizeMemberList() entered");
			Simias.Gaim.Sync.SyncNow(null);
		}
		
		/// <summary>
		/// This method is called by the Gaim iFolder Plugin when it
		/// receives a ping response message (which contains a Simias
		/// URL for a buddy.
		///
		/// Causes the Gaim Domain to re-read/sync the specified buddy's
		/// information into Simias.  We don't want to provide the ability
		/// to directly update the SimiasURL from the WebService so that
		/// any random program can't just muck with the data.
		/// </summary>
		[WebMethod(Description="UpdateMember", EnableSession=true)]
		[SoapDocumentMethod]
		public void UpdateMember(string AccountName, string AccountProtocolID, string BuddyName, string MachineName)
		{
			log.Debug("GaimDomainService.UpdateMember() entered");
			GaimDomain.UpdateMember(AccountName, AccountProtocolID, BuddyName, MachineName);
		}

		/// <summary>
		/// Returns the Machine Name (Host Name less the domain), User ID (The Gaim
		/// Domain Owner's UserID/ACE), Simias URL (Local Service URL).
		/// </summary>
		/// <returns>
		/// Returns true if all the "out" parameters were set correctly.  Otherwise, the
		/// function returns false, which signals there was an error and the out parameters
		/// are incomplete.
		/// </returns>
		[WebMethod(Description="GetUserInfo", EnableSession=true)]
		[SoapDocumentMethod]
		public bool GetUserInfo(out string MachineName, out string UserID, out string SimiasURL)
		{
			MachineName = null;
			UserID = null;
			SimiasURL = null;

			// Machine Name
			MachineName = GetMachineName();

			// UserID
			Simias.Storage.Domain domain = GaimDomain.GetDomain();
			if (domain != null)
			{
				UserID = Store.GetStore().GetUserIDFromDomainID(domain.ID);
			}
			
			// SimiasURL
			SimiasURL = Manager.LocalServiceUrl.ToString();
			
			if (MachineName != null && MachineName.Length > 0
				&& UserID != null && UserID.Length > 0
				&& SimiasURL != null && SimiasURL.Length > 0)
				return true;
			
			return false;
		}

		/// <summary>
		/// Returns the Machine Name.
		/// </summary>
		/// <returns>
		/// Returns the MachineName of the computer we're running on.
		/// </returns>
		[WebMethod(Description="GetMachineName returns the machine name of the computer we're running on.", EnableSession=true)]
		[SoapDocumentMethod]
		public string GetMachineName()
		{
			string machineName = Environment.MachineName.ToLower();
			// If a machine name with domain is added, remove the domain information
			int firstDot = machineName.IndexOf('.');
			if (firstDot > 0)
				machineName = machineName.Substring(0, firstDot);
				
			return machineName;
		}

		/// <summary>
		/// Returns the Public/Private Key pair generated by Simias.  The Gaim
		/// Plugin should only ever call this once (the very first time it's
		/// enabled.  From then on, it will use the same ones regardless of if
		/// the user deletes their Simias store.  These credentials will be
		/// used to verify the user's machine and to authenticate the user
		/// during the accept collection and synchronization process.
		/// </summary>
		/// <param name="PublicCredential">XML string representing the public RSA credential (only the public key)</param>
		/// <param name="PrivateCredential">XML string representing the complete RSA credential (public & private key included)</param>
		/// <returns>
		/// Returns true if all the "out" parameters were set correctly.  Otherwise, the
		/// function returns false, which signals there was an error and the out parameters
		/// are incomplete.
		/// </returns>
		[WebMethod(Description="GetRSACredential", EnableSession=true)]
		[SoapDocumentMethod]
		public bool GetRSACredential(out string PublicCredential, out string PrivateCredential)
		{
			PublicCredential = null;
			PrivateCredential = null;
			
			Identity ident = Store.GetStore().CurrentUser;
			
			RSACryptoServiceProvider rsaProvider = ident.Credential;
			PublicCredential = rsaProvider.ToXmlString(false);
			PrivateCredential = rsaProvider.ToXmlString(true);
			
			if (PublicCredential != null && PublicCredential.Length > 0
				&& PrivateCredential != null && PrivateCredential.Length > 0)
				return true;
			
			return false;
		}
		
		/// <summary>
		/// RSAEncryptString() uses RsaCryptoXml (A .NET RSACryptoServiceProvider
		/// in XML format representing a public key) to encrypt UnencryptedString.
		/// </summary>
		/// <param name="RsaCryptoXml">XML string representing the a .NET RSACryptoServiceProvider</param>
		/// <param name="UnencryptedString">The string to be encrypted with the RSACryptoServiceProvider</param>
		/// <returns>
		/// Returns a string encrypted with the RSACryptoServiceProvider and Base64 Encoded.
		/// </returns>
		[WebMethod(Description="RSAEncryptString() uses RsaCryptoXml (A .NET RSACryptoServiceProvider in XML format representing a public key) to encrypt UnencryptedString.", EnableSession=true)]
		[SoapDocumentMethod]
		public string RSAEncryptString(string RsaCryptoXml, string UnencryptedString)
		{
			RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
			rsaProvider.FromXmlString(RsaCryptoXml);

			byte[] stringBytes = new UTF8Encoding().GetBytes(UnencryptedString);
			byte[] encryptedText = rsaProvider.Encrypt(stringBytes, false);
			
			return Convert.ToBase64String(encryptedText);
		}
		
		/// <summary>
		/// RSADecryptString() uses RsaCryptoXml (A .NET RSACryptoServiceProvider
		/// in XML format representing a private key) to decrypt EncryptedString.
		/// </summary>
		/// <param name="RsaCryptoXml">XML string representing the a .NET RSACryptoServiceProvider</param>
		/// <param name="EncryptedString">A Base64 Encoded string to be decrypted with the RSACryptoServiceProvider</param>
		/// <returns>
		/// Returns a string decrypted with the RSACryptoServiceProvider
		/// </returns>
		[WebMethod(Description="RSADecryptString() uses RsaCryptoXml (A .NET RSACryptoServiceProvider in XML format representing a private key) to decrypt EncryptedString.", EnableSession=true)]
		[SoapDocumentMethod]
		public string RSADecryptString(string RsaCryptoXml, string EncryptedString)
		{
			RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
			rsaProvider.FromXmlString(RsaCryptoXml);

			byte[] stringBytes = Convert.FromBase64String(EncryptedString);
			byte[] decryptedText = rsaProvider.Decrypt(stringBytes, false);

			UTF8Encoding utf8 = new UTF8Encoding();
			return utf8.GetString( decryptedText );
		}
		
		/// <summary>
		/// Generates a Base64 Encoded DES Symmetric Key
		/// </summary>
		/// <returns>
		/// Returns a Base64 Encoded DES Symmetric Key
		/// </returns>
		[WebMethod(Description="Generates a Base64 Encoded DES Symmetric Key", EnableSession=true)]
		[SoapDocumentMethod]
		public string GenerateDESKey()
		{
			DESCryptoServiceProvider des = (DESCryptoServiceProvider) DESCryptoServiceProvider.Create();
			return Convert.ToBase64String(des.Key);
		}
		
		/// <summary>
		/// DESEncryptString() uses DESKey (A Base64 encoded DES key) to
		/// encrypt UnencryptedString.
		/// </summary>
		/// <param name="DESKey">The Base64 Encoded Key</param>
		/// <param name="UnencryptedString">The string to be encrypted</param>
		/// <returns>
		/// Returns a string encrypted with the DES key and Base64 encoded.
		/// </returns>
		[WebMethod(Description="DESEncryptString() uses DESKey (A Base64 encoded DES key) to encrypt UnencryptedString.", EnableSession=true)]
		[SoapDocumentMethod]
		public string DESEncryptString(string DESKey, string UnencryptedString)
		{
			return Crypto.EncryptData(DESKey, UnencryptedString);
		}

		/// <summary>
		/// DESDecryptString() uses DESKey (A Base64 encoded DES key) to
		/// decrypt EncryptedString.
		/// </summary>
		/// <param name="DESKey">The Base64 Encoded Key</param>
		/// <param name="EncryptedString">The Base64 encoded + encrypted string to be decrypted</param>
		/// <returns>
		/// Returns a string decrypted with the DES key
		/// </returns>
		[WebMethod(Description="DESDecryptString() uses DESKey (A Base64 encoded DES key) to decrypt EncryptedString.", EnableSession=true)]
		[SoapDocumentMethod]
		public string DESDecryptString(string DESKey, string EncryptedString)
		{
			return Crypto.DecryptData(DESKey, EncryptedString);
		}

//		internal string GetMachineName()
//		{
//			string machineName = Environment.MachineName.ToLower();
//			// If a machine name with domain is added, remove the domain information
//			int firstDot = machineName.IndexOf('.');
//			if (firstDot > 0)
//				machineName = machineName.Substring(0, firstDot);
//				
//			return machineName;
//		}

		/// <summary>
		/// Class for encrypting strings.
		/// </summary>
		internal class Crypto
		{
			//////////////////////////
			//Function to encrypt data
			public static string EncryptData(string base64Key, string
				strData)
			{
				string strResult; //Return Result

				DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();

				descsp.Key	= Convert.FromBase64String(base64Key);
				descsp.IV	= Convert.FromBase64String(base64Key);

				ICryptoTransform desEncrypt = descsp.CreateEncryptor();

				MemoryStream mOut = new MemoryStream();
				CryptoStream encryptStream = new CryptoStream(mOut,
					desEncrypt, CryptoStreamMode.Write);

				byte[] rbData = UnicodeEncoding.Unicode.GetBytes(strData);
				try
				{
					encryptStream.Write(rbData, 0,
						rbData.Length);
				}
				catch
				{
					// Catch it
				}

				// ADD: tell crypto stream you are done encrypting
				encryptStream.FlushFinalBlock();

				if (mOut.Length == 0)
					strResult = "";
				else
				{
					// ADD: use ToArray(). GetBuffer
					byte []buff = mOut.ToArray();
					strResult = Convert.ToBase64String(buff, 0,
						buff.Length);
				}

				try
				{
					encryptStream.Close();
				}
				catch
				{
					// Catch it
				}

				return strResult;
			}

			//////////////////////////
			//Function to decrypt data
			public static string DecryptData(string base64Key, string
				strData)
			{
				string strResult;

				DESCryptoServiceProvider descsp = new
					DESCryptoServiceProvider();

				descsp.Key = Convert.FromBase64String(base64Key);
				descsp.IV = Convert.FromBase64String(base64Key);

				ICryptoTransform desDecrypt = descsp.CreateDecryptor();

				MemoryStream mOut = new MemoryStream();
				CryptoStream decryptStream = new CryptoStream(mOut,
					desDecrypt, CryptoStreamMode.Write);
				char [] carray = strData.ToCharArray();
				byte[] rbData = Convert.FromBase64CharArray(carray,
					0, carray.Length);
				try
				{
					decryptStream.Write(rbData, 0,
						rbData.Length);
				}
				catch
				{
					// Catch it
				}

				decryptStream.FlushFinalBlock();

				UnicodeEncoding aEnc = new UnicodeEncoding();
				strResult = aEnc.GetString(mOut.ToArray());

				try
				{
					decryptStream.Close();
				}
				catch
				{
					// Catch it
				}

				return strResult;
			}
		}	
	
	}
}