/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/


using System;
using System.Xml;

using Simias;
using Simias.Storage;

namespace Novell.iFolder.Web
{
	/// <summary>
	/// This class exists only to represent a Member and should only be
	/// used in association with the iFolderWebService class.
	/// </summary>
	[Serializable]
	public class JournalEntry
	{
		/// <summary>
		/// </summary>
		public string Type;
		/// <summary>
		/// </summary>
		public string TimeStamp;
		/// <summary>
		/// </summary>
		public string UserID;
		/// <summary>
		/// </summary>
		public string UserName;

		/// <summary>
		/// </summary>
		public JournalEntry()
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="domain"></param>
		/// <param name="member"></param>
		public JournalEntry( Domain domain, XmlElement journalElement )
		{
			this.Type = journalElement.Name;
			this.UserID = journalElement.GetAttribute( "userID" );
			DateTime time = new DateTime( Int64.Parse( journalElement.GetAttribute( "ts" ) ) );
			this.TimeStamp = time.ToString();

			Member member = domain.GetMemberByID( this.UserID );
			if ( member != null )
			{
				if ( member.FN != null && 
					 !member.FN.Equals( string.Empty ))
				{
					this.UserName = member.FN;
				}
				else
				{
					this.UserName = member.Name;
				}
			}
		}
	}
}
