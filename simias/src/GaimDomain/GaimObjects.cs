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
		#region Class Members
		private string accountName = null;
		private string accountProtocolID = null;
		private string name = null;
		private string alias = null;
		private string simiasURL = null;
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

		/// <summary>
		/// Gets the buddy's alias or null if there is none
		/// </summary>
		public string Alias
		{
			get
			{
				if (alias != null) return alias;
				
				// Parse alias from the xmlBuddyNode
				XmlNode node =
					xmlBuddyNode.SelectSingleNode("alias/text()");
				if (node != null)
				{
					alias = node.Value;
				}
				
				return alias;
			}
		}

		/// <summary>
		/// Gets the buddy's SimiasURL or null if there is none
		/// </summary>
		public string SimiasURL
		{
			get
			{
				if (simiasURL != null) return simiasURL;
				
				// Parse simiasURL from the xmlBuddyNode
				XmlNode node =
					xmlBuddyNode.SelectSingleNode("setting[@name='simias-url']/text()");
				if (node != null)
				{
					simiasURL = node.Value;
				}
				
				return simiasURL;
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
		private string alias = null;
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
		
		/// <summary>
		/// Gets the account Alias if any
		/// </summary>
		public string Alias
		{
			get
			{
				if (alias != null) return alias;
				
				// Parse alias from the xmlBuddyNode
				XmlNode node =
					xmlNode.SelectSingleNode("alias/text()");
				if (node != null)
				{
					alias = node.Value;
				}
				
				return alias;
			}
		}

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
