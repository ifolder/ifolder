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
using System.Web;
using System.Net;
using System.Web.Services.Protocols;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Novell.Directory.Ldap;
using Simias.LdapProvider;

namespace LdapUserUser
{	class UserAdd
	{
		static bool verbose = false;
		static string username = null;
		static string password = null;
		static string sn = null;
		static string ldapHomeServer = null;
		static string adminName = null;
		static string adminPassword = null;
		static string ldapURL= null;
		

		/// <summary>
		/// Display operation details if verbose is set
		/// </summary>
		/// <returns>True if successful.</returns>
		public static bool DisplayStrings(string str)
		{
			if( verbose == true)	
				Console.WriteLine(str);
			return true;
		}

		/// <summary>
		/// Parses the command line parameters gathering user info
		/// for the Simias registration.
		/// </summary>
		/// <param name="args">Command line parameters.</param>
		/// <returns>True if successful.</returns>
		private static bool ParseCommandLine( string[] args )
		{
			bool status = false;
				
			for ( int i = 0; i < args.Length; ++i )
			{
				switch ( args[i].ToLower() )
				{
					case "-h":
					{
						if ( ++i < args.Length )
						{
							ldapURL = args[i];
						}
						break;
					}

					case "-d":
					{
						if ( ++i < args.Length )
						{
							adminName = args[i];
						}
						break;
					}

					case "-w":
					{
						if ( ++i < args.Length )
						{
							adminPassword = args[i];
						}
						break;
					}

					case "-u":
					{
						if ( ++i < args.Length )
						{
							username = args[i];
						}
						break;
					}

					case "-s":
					{
						if ( ++i < args.Length )
						{
							sn = args[i];
						}
						break;
					}

					case "-c":
					{
						if ( ++i < args.Length )
						{
							password = args[i];
						}
						break;
					}

					case "-i":
					{
						if ( ++i < args.Length )
						{
							ldapHomeServer = args[i];
						}
						break;
					}
						
					case "--verbose":
					{
						verbose = true;
						break;
					}

					default:
					{
						return status;
					}
				}
			}

		 	if(username != null && ldapURL != null && adminPassword != null && adminName != null)	
				status=true;
			return status;
		}

		private static void ShowUseage()
		{
			Console.WriteLine("Usage: iFolderLdapUserUpdate.sh -h <Ldap URL> -d <admin DN> -w <admin password> -u <user DN> [-s <surname>] [-c <user password>] [-i <iFolder Home Server>] --verbose ");
			Console.WriteLine("");
			Console.WriteLine("Example Usage: iFolderLdapUserUpdate.sh -h ldaps://10.10.10.10 -d cn=admin,o=novell -w secret -u cn=abc,o=novell -s xyz  -c secret -i 10.10.10.10 ");
			Console.WriteLine("        Usage: iFolderLdapUserUpdate.sh -h ldap://10.10.10.10 -d cn=admin,o=novell -w secret -u cn=mmm,o=novell -s nnn  -c secret");
			Console.WriteLine("");
			return;
		}
		
		
		public static void Main(string[] args)
		{
			if ( args.Length == 0 ||  args.Length < 8 )
			{
				ShowUseage();
				return;
			}
			
			Console.WriteLine("----------------------------------------------------------------------");
			Console.WriteLine("User Add / update tool, Creates / Updates users with iFolderHomeServer");
			Console.WriteLine("----------------------------------------------------------------------");
			DisplayStrings("About to parse command Line arguments");
			if(ParseCommandLine( args ) == false)
			{
				DisplayStrings("Command Line parse failed, required arguments are not present");
				ShowUseage();
				return;
			}
			DisplayStrings("Command Line arguments are correct");
                        Uri ldapUrl = new Uri(ldapURL);
			LdapUtility ldapUtility =  new LdapUtility(ldapUrl.ToString() , adminName, adminPassword);
			Console.WriteLine(String.Format("Connecting to {0}", ldapUrl.ToString()));
			ldapUtility.Connect();
			Console.WriteLine(String.Format("Successfully connected to {0} as {1}",ldapUrl.ToString(), adminName ));
			Console.WriteLine(String.Format("Querying for directory type..."));
			LdapDirectoryType directoryType = ldapUtility.QueryDirectoryType();
			if ( directoryType.Equals( LdapDirectoryType.Unknown ) )
                        {
                                Console.WriteLine("Unable to determine directory type for {0}, Failed to create/update User", ldapUtility.Host );
				return;
                        }
			if(sn == null)
				sn = username;
			if(password == null)
				password = "password";
			DisplayStrings(String.Format("Checking user presence, if user is not present user creation is attempted "));
			if(ldapUtility.CreateUser(username, password, sn, ldapHomeServer ))
			{
                                Console.WriteLine("");
                                Console.WriteLine("Created {0} successfully with iFolderUserProvision object class and iFolderHomeServer attributes.", username );
			}
			else
			{
				DisplayStrings(String.Format("User {0} already exits in {1} ", username, ldapUrl.ToString()));
				switch ( directoryType )
				{
					case LdapDirectoryType.ActiveDirectory:
					{
						if(ldapHomeServer != null)
						{
							DisplayStrings(String.Format("Updating user {0} with {1} ", username, "iFolderHomeServer"));
							if(ldapUtility.UpdateUserObject(username, "iFolderHomeServer", ldapHomeServer, false) == false)
								ldapUtility.UpdateUserObject(username, "iFolderHomeServer", ldapHomeServer, true);
							DisplayStrings(String.Format("Updated"));
						}
						break;
					}
					case LdapDirectoryType.eDirectory:
					{
						DisplayStrings(String.Format("Updating user {0} with {1} ", username, "iFolderUserProvision"));
						if(ldapUtility.UpdateUserObject(username, "objectclass", "iFolderUserProvision", false ) == false)
							DisplayStrings(String.Format("{0} is already present in {1} object", "iFolderUserProvision", username));
						else
							DisplayStrings(String.Format("Updated"));
						if(ldapHomeServer != null)
						{
							DisplayStrings(String.Format("Updating user {0} with {1} ", username, "iFolderHomeServer"));
							if(ldapUtility.UpdateUserObject(username, "iFolderHomeServer", ldapHomeServer, false) == false)
								ldapUtility.UpdateUserObject(username, "iFolderHomeServer", ldapHomeServer, true);
							DisplayStrings(String.Format("Updated"));
						}
						break;
					}
					case LdapDirectoryType.OpenLDAP:
					{
						// OpenLdap Extending of schema is not yet implemnted, With OpenLdap Extending of schema is implemented 
						// this update user object will also work.
						DisplayStrings(String.Format("Updating user {0} with {1} ", username, "iFolderUserProvision"));
						if(ldapUtility.UpdateUserObject(username, "objectclass", "iFolderUserProvision", false ) == false)
							DisplayStrings(String.Format("{0} is already present in {1} object", "iFolderUserProvision", username));
						else
							DisplayStrings(String.Format("Updated"));
						if(ldapHomeServer != null)
						{
							DisplayStrings(String.Format("Updating user {0} with {1} ", username, "iFolderHomeServer"));
							if(ldapUtility.UpdateUserObject(username, "iFolderHomeServer", ldapHomeServer, false) == false)
								ldapUtility.UpdateUserObject(username, "iFolderHomeServer", ldapHomeServer, true);
							DisplayStrings(String.Format("Updated"));
						}
						break;
					}
				}
			}

			
	
					
			return;
		}
	}

	/// <summary>
	/// LDAP Utility Object
	/// </summary>
	public class LdapUtility
	{
		#region Class Members
		/// <summary>
		/// LDAP Scheme
		/// </summary>
		public static readonly string LDAP_SCHEME = "ldap";

		/// <summary>
		/// Secure LDAP Scheme
		/// </summary>
		public static readonly string LDAP_SCHEME_SECURE = "ldaps";

		/// <summary>
		/// AD userAccountControl flags
		/// </summary>
                [Flags]
                private enum ADS_USER_FLAGS
                {
                        SCRIPT = 0X0001,
                        ACCOUNTDISABLE = 0X0002,
                        HOMEDIR_REQUIRED = 0X0008,
                        LOCKOUT = 0X0010,
                        PASSWD_NOTREQD = 0X0020,
                        PASSWD_CANT_CHANGE = 0X0040,
                        ENCRYPTED_TEXT_PASSWORD_ALLOWED = 0X0080,
                        TEMP_DUPLICATE_ACCOUNT = 0X0100,
                        NORMAL_ACCOUNT = 0X0200,
                        INTERDOMAIN_TRUST_ACCOUNT = 0X0800,
                        WORKSTATION_TRUST_ACCOUNT = 0X1000,
                        SERVER_TRUST_ACCOUNT = 0X2000,
                        DONT_EXPIRE_PASSWD = 0X10000,
                        MNS_LOGON_ACCOUNT = 0X20000,
                        SMARTCARD_REQUIRED = 0X40000,
                        TRUSTED_FOR_DELEGATION = 0X80000,
                        NOT_DELEGATED = 0X100000,
                        USE_DES_KEY_ONLY = 0x200000,
                        DONT_REQUIRE_PREAUTH = 0x400000,
                        PASSWORD_EXPIRED = 0x800000,
                        TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 0x1000000
                }

		/// <summary>
		/// LDAP connection
		/// </summary>
		private LdapConnection connection;
		private string host;
		private int port;
		private bool secure;
		private string dn;
		private string password;
		private LdapDirectoryType ldapType = LdapDirectoryType.Unknown;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="url">LDAP URL</param>
		/// <param name="dn">LDAP User DN</param>
		/// <param name="password">LDAP User Password</param>
		public LdapUtility(string url, string dn, string password)
		{
			Uri ldapUrl = new Uri(url);

			host = ldapUrl.Host;

			// secure
			secure = ldapUrl.Scheme.ToLower().Equals(
				LDAP_SCHEME_SECURE) ? true : false;

			// port
			
			port = (ldapUrl.Port != -1) ? ldapUrl.Port : (secure ?
				LdapSettings.UriPortLdaps : LdapSettings.UriPortLdap);
			

			this.dn = dn;
			this.password = password;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Connect and/or Bind to the LDAP Server
		/// </summary>
		public void Connect()
		{
			connection = new LdapConnection();
			connection.SecureSocketLayer = secure;
			try
			{
				connection.Connect(host, port);
				connection.Bind(dn, password);
			}
			catch(LdapException ex)
			{
				if(ex.ResultCode == LdapException.INVALID_CREDENTIALS)
				{
					Console.WriteLine("Invalid Credentials passed, Enter correct set of credentials");
					System.Environment.Exit( -1 );
				}
				else
				{
					Console.WriteLine("Ldap Server connect failed with the follwing error {0}:{1}", ex.ResultCode.ToString(), ex.Message);
					Console.WriteLine("Exited");
					throw ex;
				}
			}
		}


		/// <summary>
		/// Disconnect from the LDAP Server
		/// </summary>
		public void Disconnect()
		{
			connection.Disconnect();
			connection = null;
		}

		/// <summary>
		/// Create a New User in the LDAP Tree
		/// </summary>
		/// <param name="dn">The New User DN</param>
		/// <param name="password">The New User Password</param>
		/// <param name="sn">New User sn</param>
		/// <param name="ldapHomeServer">New User ldapHomeServer</param>
		/// <returns>true, if the user was created. false, if the user already exists.</returns>
		public bool CreateUser(string dn, string password, string sn, string ldapHomeServer)
		{
			bool created = true;
			try
			{
				connection.Read(dn);
				created = false;
			}
			catch
			{
				UserAdd.DisplayStrings("");
				UserAdd.DisplayStrings("Attempting to create user, with the following ldif");
				UserAdd.DisplayStrings("");
				UserAdd.DisplayStrings("--------------------------------------------------");
				LdapAttributeSet attributeSet = new LdapAttributeSet();
				switch ( ldapType )
				{
					case LdapDirectoryType.ActiveDirectory:
					{
						Regex cnRegex=null;
						int AccEnable = (int)ADS_USER_FLAGS.NORMAL_ACCOUNT | (int)ADS_USER_FLAGS.DONT_EXPIRE_PASSWD; // Flags set to 66048 
						string quotedPassword = "\"" + password + "\"";
						char [] unicodePassword = quotedPassword.ToCharArray();
						sbyte [] userPassword = new sbyte[unicodePassword.Length * 2];

						for (int i=0; i<unicodePassword.Length; i++) {
							userPassword[i*2 + 1] = (sbyte) (unicodePassword[i] >> 8);
							userPassword[i*2 + 0] = (sbyte) (unicodePassword[i] & 0xff);
						}

						if(dn.ToLower().StartsWith("cn="))
							cnRegex = new Regex(@"^cn=(.*?),.*$",RegexOptions.IgnoreCase | RegexOptions.Compiled);
						else if (dn.ToLower().StartsWith("uid="))
							cnRegex = new Regex(@"^uid=(.*?),.*$",RegexOptions.IgnoreCase | RegexOptions.Compiled);
						string cn = cnRegex.Replace(dn, "$1");

						// create user attributes
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "dn", dn));
						attributeSet.Add(new LdapAttribute("objectClass", "user"));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "objectClass", "user"));
						attributeSet.Add(new LdapAttribute("objectClass", "InetOrgPerson"));
						UserAdd.DisplayStrings(String.Format("{0}: {1}","objectClass", "InetOrgPerson" ));
						attributeSet.Add(new LdapAttribute("cn", cn));
						UserAdd.DisplayStrings(String.Format("{0}: {1}","cn", cn ));
						attributeSet.Add(new LdapAttribute("SamAccountName", cn));
						UserAdd.DisplayStrings(String.Format("{0}: {1}","SamAccountName", cn ));
						attributeSet.Add(new LdapAttribute("sn", sn));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "sn", sn));
						attributeSet.Add(new LdapAttribute("userAccountControl", AccEnable.ToString()));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "userAccountControl", AccEnable.ToString()));
						attributeSet.Add(new LdapAttribute("UnicodePwd", userPassword));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "UnicodePwd", "xxxxxxx"));

						LdapEntry entry = new LdapEntry(dn, attributeSet);
						connection.Add(entry);
						if(ldapHomeServer != null)
						{
							if(UpdateUserObject(dn, "iFolderHomeServer", ldapHomeServer, false) == false)
								UpdateUserObject(dn, "iFolderHomeServer", ldapHomeServer, true);
						}
						break;
					}
					case LdapDirectoryType.eDirectory:
					{
						// parse the cn
						Regex cnRegex=null;
						if(dn.ToLower().StartsWith("cn="))
							cnRegex = new Regex(@"^cn=(.*?),.*$",RegexOptions.IgnoreCase | RegexOptions.Compiled);
						else if (dn.ToLower().StartsWith("uid="))
							cnRegex = new Regex(@"^uid=(.*?),.*$",RegexOptions.IgnoreCase | RegexOptions.Compiled);
						string cn = cnRegex.Replace(dn, "$1");

						// create user attributes
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "Dn", dn ));
						attributeSet.Add(new LdapAttribute("objectClass", "inetOrgPerson"));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "objectClass", "inetOrgPerson"));
						attributeSet.Add(new LdapAttribute("objectclass", "iFolderUserProvision"));
						UserAdd.DisplayStrings(String.Format("{0}: {1}","objectclass", "iFolderUserProvision" ));
						attributeSet.Add(new LdapAttribute("cn", cn));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "cn", cn));
						attributeSet.Add(new LdapAttribute("sn", sn));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "sn", sn ));
						attributeSet.Add(new LdapAttribute("userPassword", password));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "userPassword", "xxxxxxxx"));

						LdapEntry entry = new LdapEntry(dn, attributeSet);
						connection.Add(entry);
						UpdateUserObject(dn, "objectclass", "iFolderUserProvision", false );
						if(ldapHomeServer != null)
						{
							if(UpdateUserObject(dn, "iFolderHomeServer", ldapHomeServer, false) == false)
								UpdateUserObject(dn, "iFolderHomeServer", ldapHomeServer, true);
						}
						break;
					}
					case LdapDirectoryType.OpenLDAP:
					{
						Regex uidRegex = new Regex(@"^(.*?)=(.*?),.*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
						string uid = uidRegex.Replace(dn, "$2");

						// I think we can get away with just creating an inetOrgPerson ...
						// we don't need a posixAccount ... hmm, maybe a shadowAccount
						// so that the password can expire?
						attributeSet.Add(new LdapAttribute("objectClass", "inetOrgPerson"));//new string[]{"inetOrgPerson", "posixAccount", "shadowAccount"}));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "Dn", dn ));
						attributeSet.Add(new LdapAttribute("uid", uid));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "uid", uid));
						attributeSet.Add(new LdapAttribute("cn", uid));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "cn", uid));
						attributeSet.Add(new LdapAttribute("sn", sn));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "sn", sn));
						attributeSet.Add(new LdapAttribute("givenName", uid));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "givenName", uid));
						attributeSet.Add(new LdapAttribute("displayName", uid));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "displayName", uid));
						attributeSet.Add(new LdapAttribute("objectclass", "iFolderUserProvision"));
						// TODO: Need to encrypt the password first.
						attributeSet.Add(new LdapAttribute("userPassword", password));
						UserAdd.DisplayStrings(String.Format("{0}: {1}", "userPassword", "xxxxxxx"));

						LdapEntry entry = new LdapEntry(dn, attributeSet);
						connection.Add(entry);
						UpdateUserObject(dn, "objectclass", "iFolderUserProvision", false );
						if(ldapHomeServer != null)
						{
							if(UpdateUserObject(dn, "iFolderHomeServer", ldapHomeServer, false) == false)
								UpdateUserObject(dn, "iFolderHomeServer", ldapHomeServer, true);
						}
						break;
					}
				}
                                    
				UserAdd.DisplayStrings("");
				UserAdd.DisplayStrings("--------------------------------------------------");
			}

			// result
			return created;
		}

		/// <summary>
		/// Update User object with a attribute and its value
		/// </summary>
		/// <param name="dn">The New User DN</param>
		/// <param name="Attrname">The New attribute name</param>
		/// <param name="">New User sn</param>
		/// <returns>true, if the user was created. false, if the user already exists.</returns>
		public bool UpdateUserObject(string dn, string Attrname, string AttrValue, bool replace)
		{
			LdapAttribute ldpAttr= new LdapAttribute(Attrname, AttrValue);
			LdapModification modification = null;
			if(replace == true)
				modification = new LdapModification(LdapModification.REPLACE, ldpAttr);
			else
				modification = new LdapModification(LdapModification.ADD, ldpAttr);
			try
			{
				connection.Modify(dn, modification);
			}
			catch(LdapException ex)
			{
				if(ex.ResultCode == LdapException.ATTRIBUTE_OR_VALUE_EXISTS)
					return false;
				else
				{
					Console.WriteLine("Failed to modify the user object for {0}", AttrValue);
					Console.WriteLine("Error Message: {0}:{1}", ex.ResultCode.ToString(), ex.Message);
					throw ex;
				}
			}
			return true;
		}

		/// <summary>
		/// Queries to find the type of directory  
		/// </summary>
		/// <returns>The LDAP directory type.</returns>
		public LdapDirectoryType QueryDirectoryType()
		{

			LdapAttribute attr	= null;
			LdapEntry entry	= null;
			bool eDirectory	= false;
			LdapSearchResults lsc=connection.Search("",
												LdapConnection.SCOPE_BASE,
												"objectClass=*",
												null,
												false);
			UserAdd.DisplayStrings(String.Format(""));
			UserAdd.DisplayStrings(String.Format("============="));
			while (lsc.hasMore())
			{
				entry = null;				
				try 
				{
					entry = lsc.next();
				}
				catch(LdapException e) 
				{
					Console.WriteLine("Error: " + e.LdapErrorMessage);
					continue;
				}
				UserAdd.DisplayStrings(String.Format("Entry DN: {0} ", entry.DN));
				LdapAttributeSet attributeSet = entry.getAttributeSet();
				System.Collections.IEnumerator ienum =  attributeSet.GetEnumerator();
				
				while(ienum.MoveNext())
				{
					attr = (LdapAttribute)ienum.Current;
					string attributeName = attr.Name;
					string attributeVal = attr.StringValue;
					UserAdd.DisplayStrings(String.Format("{0}: {1}",attributeName, attributeVal ));

					if(	String.Equals(attributeVal, "Novell, Inc.")==true )
					{
						eDirectory = true;
						UserAdd.DisplayStrings(String.Format("Directory Type: {0} ", "Novell eDirectory"));
						break;
					}
				}
			}	
			
			if ( eDirectory == true)
			{
				ldapType = LdapDirectoryType.eDirectory;
			}
			else
			{
				entry = connection.Read( "" );
				attr = entry.getAttribute( "defaultNamingContext" );
				if ( attr != null )
				{
					UserAdd.DisplayStrings(String.Format("Directory Type: {0} ", "Active Directory"));
					ldapType = LdapDirectoryType.ActiveDirectory;
				}
				else
				{
					UserAdd.DisplayStrings(String.Format("Directory Type: {0} ", "OpenLdap Directory"));
					ldapType = LdapDirectoryType.OpenLDAP;
				}
			}

			UserAdd.DisplayStrings(String.Format("============="));
			UserAdd.DisplayStrings(String.Format(""));
			return ldapType;		
			
		}
		#endregion

		#region Properties
		/// <summary>
		/// LDAP Bind User DN
		/// </summary>
		public string DN
		{
			get { return dn; }
		}

		/// <summary>
		/// LDAP Host
		/// </summary>
		public string Host
		{
			get { return host; }
		}

		/// <summary>
		/// Gets the LDAP directory type.
		/// </summary>
		public LdapDirectoryType DirectoryType
		{
			get { return ldapType; }
		}

		/// <summary>
		/// LDAP Bind User Password
		/// </summary>
		public string Password
		{
			get { return password; }
		}

		/// <summary>
		/// LDAP Port
		/// </summary>
		public int Port
		{
			get { return port; }
		}

		/// <summary>
		/// LDAP Secure Connection
		/// </summary>
		public bool Secure
		{
			get { return secure; }
		}
		#endregion
	}
}

