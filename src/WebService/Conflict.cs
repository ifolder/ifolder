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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using Simias;
using Simias.Storage;

using System;

namespace Novell.iFolder.Web
{
	/// <summary>
	/// This class exists only to represent a Member and should only be
	/// used in association with the iFolderWebService class.
	/// </summary>
	[Serializable]
	public class Conflict
	{
		public string	iFolderID;
		public string	ConflictID;
		public string	LocalName;
		public string	LocalDate;
		public string	LocalSize;
		public bool 	IsNameConflict;

		public string	ServerName;
		public string	ServerDate;
		public string	ServerSize;

		public Conflict()
		{
		}




		public Conflict(Collection col, Node node)
		{
			iFolderID = col.ID;
			ConflictID = node.ID;
			Simias.Sync.Conflict conflict = new Simias.Sync.Conflict(col, node);
			if(conflict.IsFileNameConflict)
			{
				IsNameConflict = true;
				FileNode serverFileNode = new FileNode(node);
				Node localNode = col.GetNodeFromCollision(node);
				FileNode localFileNode = new FileNode(localNode);
				
				LocalName = localFileNode.GetFileName();
				LocalDate = localFileNode.LastWriteTime.ToString();
				LocalSize = "N/A";
			
				ServerName = serverFileNode.GetFileName();
				ServerDate = serverFileNode.LastWriteTime.ToString();
				ServerSize = "N/A";
			}
			else
			{
				IsNameConflict = false;
				LocalName = conflict.NonconflictedPath;
			}
		}




		public static void Resolve(Collection col, Node node, 
										bool localChangesWin)
		{
			Simias.Sync.Conflict conflict = new Simias.Sync.Conflict(col, node);
			if(conflict.IsFileNameConflict)
			{
				throw new Exception("Resolve must be called with a new file name in the case of a Name Conflict");
			}
			else
				conflict.Resolve(localChangesWin);
		}




		public static void Resolve(Collection col, Node node, 
										string newNodeName)
		{
			Simias.Sync.Conflict conflict = new Simias.Sync.Conflict(col, node);
			if(conflict.IsFileNameConflict)
			{
				conflict.Resolve(newNodeName);
			}
			else
				throw new Exception("Resolve must be called with a boolean option of which version wins, server or local.  This call is for Name conflicts");
		}
	}
}
