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
*                 $Author: Mahabaleshwar Asundi <amahabaleshwar@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

using Simias;
using Simias.Storage;

namespace Simias.LdapProvider
{
	public class ProxyUser
	{
		public readonly string ProxyPasswordFile = ".simias.ppf";
		private readonly string LdapSection = "LdapAuthentication";
		private readonly string ProxyDNKey = "ProxyDN";
		private Store store = Store.GetStore();

		#region Properties
		/// <summary>
		/// Retrieve the proxy user's distinguished name
		/// The dn always exists in the Simias configuration file
		/// </summary>
		public string UserDN
		{
			get
			{ 
				Configuration SimiasConfig = new Configuration( Store.StorePath, true) ;
				return (SimiasConfig.Get( LdapSection, ProxyDNKey));
			}
		}

		/// <summary>
		/// Retrieve the proxy user's password
		/// The proxy user's password is first retrieved from
		/// the configuration file and then stored encrypted in the
		/// enterprise domain.  If the proxy user password is changed
		/// the Simias management utility simply needs to reset the
		/// password back into the configuration file.
		/// </summary>
		public string Password
		{
			get
			{
				string proxyPassword = null;

				try
				{
					string ppfPath = Path.Combine( Store.StorePath, ProxyPasswordFile );
					if ( !File.Exists( ppfPath ) )
					{
						proxyPassword = this.GetProxyPasswordFromStore();
					}
					else
					{
						using ( StreamReader sr = File.OpenText( ppfPath ) )
						{
							// Password should be a single line of text.
							proxyPassword = sr.ReadLine();
						}

						// See if the password was successfully read from the file.
						if ( proxyPassword != null )
						{
							// Save the password to the store.
							if ( SaveProxyPasswordToStore( proxyPassword ) == true )
							{
								// Delete the password file.
								File.Delete( ppfPath );
							}
						}
					}
				}
				catch{}
				return( proxyPassword );
			}

			set
			{
				string ppfPath = Path.Combine( Store.StorePath, ProxyPasswordFile );

				// Save the password to the store.
				if ( SaveProxyPasswordToStore( value ) )
				{
					// Delete the password file.
					if ( File.Exists( ppfPath ) )
					{
						File.Delete( ppfPath );
					}
				}
				else
				{
					using ( StreamWriter sw = File.CreateText( ppfPath ) )
					{
						sw.WriteLine( value );
					}
				}
			}
		}
		#endregion

		#region Private Methods
        /// <summary>
        /// Get proxy password from simias store
        /// </summary>
        /// <returns></returns>
		private string GetProxyPasswordFromStore()
		{
			string password = null;

			try
			{
				Domain domain = store.GetDomain( store.DefaultDomain );
				string encodedCypher = domain.Properties.GetSingleProperty( "ProxyPassword" ).ToString();
				byte[] cypher = Convert.FromBase64String( encodedCypher );
				RSACryptoServiceProvider credential = store.CurrentUser.Credential;
				password = new UTF8Encoding().GetString( credential.Decrypt( cypher, false ) );
			}
			catch{}
			return password;
		}

        /// <summary>
        /// Encrypt the proxy password and store in simias-store
        /// </summary>
        /// <param name="password"></param>
        /// <returns>true if storage is successful</returns>
		private bool SaveProxyPasswordToStore( string password )
		{
			try
			{
				Domain domain = store.GetDomain( store.DefaultDomain );

				RSACryptoServiceProvider credential = 
					Store.GetStore().CurrentUser.Credential;

				byte[] cypher = credential.Encrypt( new UTF8Encoding().GetBytes( password ), false );
				string encryptedPassword = Convert.ToBase64String( cypher );

				Property proxyPwd = new Property( "ProxyPassword", encryptedPassword );
				proxyPwd.LocalProperty = true;
				domain.Properties.ModifyProperty( proxyPwd );
				domain.Commit( domain );
				return true;
			}
			catch{}
			return false;
		}
		#endregion
	}
}
