/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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
 *      Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Xml;

//using Simias;

namespace Simias.Gaim
{
	/// <summary>
	/// The purpose of this file is to parse a <buddy> element from the
	/// blist.xml file so that it's easier to deal with finding information
	/// about a buddy.
	/// </summary>
	public class GaimBuddy
	{
		internal class SimiasInfo
		{
			public string userID;
			public string simiasURL;
			
			public SimiasInfo(string UserID, string SimiasURL)
			{
				userID = UserID;
				simiasURL = SimiasURL;
			}
		}
	
		#region Class Members
		private string accountName = null;
		private string accountProtocolID = null;
		private string name = null;
//		private string alias = null;
		private Hashtable simiasInfos;
		private XmlNode xmlBuddyNode = null;

		#endregion

		#region Properties
		/// <summary>
		/// Gets the name of the Gaim Account that this buddy is associated with
		/// </summary>
		public string AccountName
		{
			get
			{
				if (accountName != null) return accountName;
				
				// Parse accountName from the xmlBuddyNode
				XmlAttribute attr = xmlBuddyNode.Attributes["account"];
				if (attr != null)
				{
					accountName = attr.Value;
				}
				
				return accountName;
			}
		}
		
		/// <summary>
		/// Gets the ID of the Gaim Protocol (prpl-oscar)
		/// </summary>
		public string AccountProtocolID
		{
			get
			{
				if (accountProtocolID != null) return accountProtocolID;
				
				// Parse accountProtocolID from the xmlBuddyNode
				XmlAttribute attr = xmlBuddyNode.Attributes["proto"];
				if (attr != null)
				{
					accountProtocolID = attr.Value;
				}
				
				return accountProtocolID;
			}
		}
		
		/// <summary>
		/// Gets the buddy's screenname
		/// </summary>
		public string Name
		{
			get
			{
				if (name != null) return name;
				
				// Parse name from the xmlBuddyNode
				XmlNode node =
					xmlBuddyNode.SelectSingleNode("name/text()");
				if (node != null)
				{
					name = node.Value;
				}
				
				return name;
			}
		}

//		/// <summary>
//		/// Gets the buddy's alias or null if there is none
//		/// </summary>
//		public string Alias
//		{
//			get
//			{
//				if (alias != null) return alias;
//				
//				// Parse alias from the xmlBuddyNode
//				XmlNode node =
//					xmlBuddyNode.SelectSingleNode("alias/text()");
//				if (node != null)
//				{
//					alias = node.Value;
//				}
//				
//				return alias;
//			}
//		}

		public string MungedID
		{
			get
			{
				return AccountName + ":" + AccountProtocolID + ":" + Name;
			}
		}

		public string[] MachineNames
		{
			get
			{
				ArrayList machineNames = new ArrayList(simiasInfos.Keys);
				return (string[])machineNames.ToArray(typeof(string));
			}
		}
		
		#endregion

		#region Constructors

		public GaimBuddy(XmlNode buddyNode)
		{
			// Do a lazy parse.  The getters to the parsing only when needed
			xmlBuddyNode = buddyNode;

			// If the buddy is not "prpl-oscar" throw an exception!
			string protoID = this.AccountProtocolID;
			if (protoID == null)
			{
				throw new Exception("Gaim Buddy has no protocol id");
			}
			if (protoID != "prpl-oscar")
			{
				throw new GaimProtocolNotSupportedException(protoID);
			}
			
			simiasInfos = new Hashtable();
			
			ParseSimiasInfo();
		}

		#endregion
		
		#region Internal Methods
		
		///
		/// Reads in the different UserIDs and SimiasURL settings
		///
		internal void ParseSimiasInfo()
		{
			XmlNodeList userIDSettings = xmlBuddyNode.SelectNodes("setting[starts-with(@name, 'simias-user-id:')]");
			foreach(XmlNode userIDSettingNode in userIDSettings)
			{
				string userID = userIDSettingNode.InnerText;
				string simiasURL = null;
				string machineName = ParseMachineName(userIDSettingNode);
				if (machineName != null)
				{
					XmlNode simiasURLSetting = 
						xmlBuddyNode.SelectSingleNode(string.Format("setting[@name='simias-url:{0}']/text()", machineName));
					if (simiasURLSetting != null)
					{
						simiasURL = simiasURLSetting.Value;

						SimiasInfo simiasInfo = new SimiasInfo(userID, simiasURL);
						simiasInfos.Add(machineName, simiasInfo);
					}
				}
			}
		}
		
		internal string ParseMachineName(XmlNode userIDSettingNode)
		{
			string machineName = null;

			XmlAttributeCollection attribs = userIDSettingNode.Attributes;
			if (attribs != null)
			{
				for(int i = 0; i < attribs.Count; i++)
				{
					XmlNode attrib = attribs.Item(i);
					string attribValue = attrib.Value;
					if (attribValue.StartsWith("simias-user-id:"))
					{
						int colonPos = attribValue.IndexOf(':');
						if (colonPos > 0)
						{
							machineName = 
								attribValue.Substring(colonPos, 
													  attribValue.Length - colonPos);
						}
					}
				}
			}
			
			return machineName;
		}
		
		#endregion
		
		#region Public Methods
		
		public static string ParseMachineName(string simiasMemberName)
		{
			// Make sure this ends with a )
			int length = simiasMemberName.Length;
			if (simiasMemberName[length - 1] != ')')
				throw new ArgumentException("The specified simiasMemberName doesn't end with a closing parenthesis");
				
			int openingParen = simiasMemberName.LastIndexOf('(');
			if (openingParen <= 0)
				throw new ArgumentException("The specified simiasMemberName doesn't contain an opening parenthesis or it starts the string");
			
			return simiasMemberName.Substring(openingParen + 1, length - openingParen - 1);
		}
		
		public string GetSimiasMemberName(string machineName)
		{
			return string.Format("{0} ({1})", name, machineName);
		}

		public string GetSimiasUserID(string machineName)
		{
			if (simiasInfos.Contains(machineName))
			{
				SimiasInfo simiasInfo = (SimiasInfo)simiasInfos[machineName];
				return simiasInfo.userID;
			}
			
			return null;
		}
		
		public string GetSimiasURL(string machineName)
		{
			if (simiasInfos.Contains(machineName))
			{
				SimiasInfo simiasInfo = (SimiasInfo)simiasInfos[machineName];
				return simiasInfo.simiasURL;
			}
			
			return null;
		}
		
		public string GetSimiasURLByUserID(string userID)
		{
			string simiasURL = null;
			foreach (SimiasInfo simiasInfo in simiasInfos.Values)
			{
				if (simiasInfo.userID.Equals(userID))
				{
					simiasURL = simiasInfo.simiasURL;
					break;
				}
			}
			
			return simiasURL;
		}
		
		/// <summary>
		/// Parses the mungedID into its separate parts.
		/// </summary>
		/// <param name="mungedID">A munged ID for a Gaim Buddy which is in the following format: [Account Name]:[Account Protocol ID]:[Buddy Name].</param>
		/// <param name="accountName">Filled with [Account Name] from the mungedID.</param>
		/// <param name="accountProtocolID">Filled with [Account Protocol ID] from the mungedID.</param>
		/// <param name="buddyName">Filled with [Buddy Name] from the mungedID.</param>
		/// <returns>
		/// True if the mungedID was parsed successfully.  Otherwise, false is returned.
		/// If false is returned the "out" parameters will not be valid.
		/// </returns>
		public static bool ParseMungedID(string mungedID, out string accountName, out string accountProtocolID, out string buddyName)
		{
			accountName = null;
			accountProtocolID = null;
			buddyName = null;
			
			if (mungedID == null || mungedID.Length == 0)
				return false;

			int firstColon = mungedID.IndexOf(':');
			int lastColon = mungedID.LastIndexOf(':');
			
			if (firstColon < 0 || lastColon < 0 || firstColon == lastColon)
				return false;
				
			accountName = mungedID.Substring(0, firstColon);
			accountProtocolID = mungedID.Substring(firstColon + 1, lastColon - firstColon - 1);
			buddyName = mungedID.Substring(lastColon + 1);
			
			if (accountName.Length == 0
				|| accountProtocolID.Length == 0 
				|| buddyName.Length == 0)
				return false;
			
			return true;
		}
				
		#endregion
	}
	
	/// <summary>
	/// The purpose of this file is to parse an <account> element from the
	/// accounts.xml file so that it's easier to deal with finding information
	/// about a Gaim Account.
	/// </summary>
	public class GaimAccount
	{
		#region Class Members
		private string name = null;
		private string protoID = null;
//		private string alias = null;
		private XmlNode xmlNode = null;

		#endregion

		#region Properties
		/// <summary>
		/// Gets the name of the Gaim Account (user screenname)
		/// </summary>
		public string Name
		{
			get
			{
				if (name != null) return name;
				
				// Parse name from the XmlNode
				XmlNode node =
					xmlNode.SelectSingleNode("name/text()");
				if (node != null)
				{
					name = node.Value;
				}
				
				return name;
			}
		}
		
		/// <summary>
		/// Gets the ID of the Gaim Protocol (prpl-oscar)
		/// </summary>
		public string ProtocolID
		{
			get
			{
				if (protoID != null) return protoID;
				// Parse name from the XmlNode
				XmlNode node =
					xmlNode.SelectSingleNode("protocol/text()");
				if (node != null)
				{
					protoID = node.Value;
				}
				
				return protoID;
			}
		}
		
//		/// <summary>
//		/// Gets the account Alias if any
//		/// </summary>
//		public string Alias
//		{
//			get
//			{
//				if (alias != null) return alias;
//				
//				// Parse alias from the xmlBuddyNode
//				XmlNode node =
//					xmlNode.SelectSingleNode("alias/text()");
//				if (node != null)
//				{
//					alias = node.Value;
//				}
//				
//				return alias;
//			}
//		}

		#endregion

		#region Constructors

		public GaimAccount(XmlNode accountNode)
		{
			// Do a lazy parse.  The getters to the parsing only when needed
			xmlNode = accountNode;
		}

		#endregion
	}

	/// <summary>
	/// Right now, we only support the AIM (prpl-oscar) protocol because
	/// of the stuff we have to do in the user profile (to auto-determine
	/// whether a buddy has the iFolder Plugin installed.  This is just a
	/// matter of time...if you've got time to add in support for other
	/// protocols, jump right in. :)
	/// </summary>
	public class GaimProtocolNotSupportedException : Exception
	{
		/// <summary>
		/// Create a GaimProtocolNotSupportedException.
		/// </summary>
		public GaimProtocolNotSupportedException(string protoID):
			base(string.Format("This protocol is not supported in the Gaim iFolder Plugin: {0}", protoID))
		{
		}
	}
}
