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
using System.IO;

namespace Novell.iFolder.Web
{
	/// <summary>
	/// This class exists only to represent a Member and should only be
	/// used in association with the iFolderWebService class.
	/// </summary>
	[Serializable]
	public class Conflict
	{
		private const long kilobyte = 1024;
		private const long megabyte = 1048576;

		/// <summary>
		/// The ID of the iFolder with the conflict.
		/// </summary>
		public string	iFolderID;

		/// <summary>
		/// The ID of the conflict.
		/// </summary>
		public string	ConflictID;

		/// <summary>
		/// The name of the local file.
		/// </summary>
		public string	LocalName;

		/// <summary>
		/// The timestamp on the local file.
		/// </summary>
		public string	LocalDate;

		/// <summary>
		/// The size of the local file.
		/// </summary>
		public string	LocalSize;

		/// <summary>
		/// The full path of the local file.
		/// </summary>
		public string	LocalFullPath;

		/// <summary>
		/// Tells if this conflict is a name conflict.
		/// </summary>
		public bool 	IsNameConflict;

		/// <summary>
		/// The name of the server file.
		/// </summary>
		public string	ServerName;

		/// <summary>
		/// The timestamp on the server file.
		/// </summary>
		public string	ServerDate;

		/// <summary>
		/// The size of the server file.
		/// </summary>
		public string	ServerSize;

		/// <summary>
		/// The full path of the server (conflict) file.
		/// </summary>
		public string	ServerFullPath;

		/// <summary>
		/// Constructs a Conflict object.
		/// </summary>
		public Conflict()
		{
		}




		/// <summary>
		/// Constructs a Conflict object.
		/// </summary>
		/// <param name="col">The collection containing the conflict.</param>
		/// <param name="node">The conflicting node.</param>
		public Conflict(Collection col, Node node)
		{
			iFolderID = col.ID;
			ConflictID = node.ID;
			Simias.Sync.Conflict conflict = new Simias.Sync.Conflict(col, node);
			if(conflict.IsFileNameConflict)
			{
				IsNameConflict = true;

				FileNode fileNode = new FileNode(node);
				string name = Path.GetFileName(conflict.NonconflictedPath);
				if (name.Equals(Path.GetFileName(conflict.FileNameConflictPath)))
				{
					LocalName = name;
					LocalDate = fileNode.LastWriteTime.ToString();
					LocalSize = formatFileSize(fileNode.Length);
					LocalFullPath = conflict.FileNameConflictPath;
				}
				else
				{
					ServerName = name;
					ServerDate = fileNode.LastWriteTime.ToString();
					ServerSize = formatFileSize(fileNode.Length);
					ServerFullPath = conflict.NonconflictedPath;
				}
			}
			else
			{
				IsNameConflict = false;
				FileNode localFileNode = new FileNode(node);
				Node serverNode = col.GetNodeFromCollision(node);
				FileNode serverFileNode = new FileNode(serverNode);
				
				LocalName = localFileNode.GetFileName();
				LocalDate = localFileNode.LastWriteTime.ToString();
				LocalSize = formatFileSize(localFileNode.Length);
				LocalFullPath = conflict.NonconflictedPath;
			
				ServerName = serverFileNode.GetFileName();
				ServerDate = serverFileNode.LastWriteTime.ToString();
				ServerSize = formatFileSize(serverFileNode.Length);
				ServerFullPath = conflict.UpdateConflictPath;
			}
		}




		/// <summary>
		/// A method used to resolve a conflict.
		/// </summary>
		/// <param name="col">The collection containing the conflict.</param>
		/// <param name="node">The conflicting node.</param>
		/// <param name="localChangesWin">Set to <b>True</b> to overwrite the server node (or file) with the local node (or file), 
		/// set to <b>False</b> to overwrite the local node (or file) with the server node (or file).</param>
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




		/// <summary>
		/// A method used to resolve a name conflict.
		/// </summary>
		/// <param name="col">The collection containing the conflict.</param>
		/// <param name="node">The conflicting node.</param>
		/// <param name="newNodeName">The new name to assign the node (file).</param>
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




		/// <summary>
		/// A method used to rename a conflicting (local) file and resolve the conflicted (server) file
		/// to the same name.
		/// </summary>
		/// <param name="col">The collection containing the conflict.</param>
		/// <param name="node">The conflicted node.</param>
		/// <param name="newFileName">The new name to assign to the conflicting file.</param>
		public static void RenameConflictingAndResolve(Collection col, Node node, string newFileName)
		{
			Simias.Sync.Conflict conflict = new Simias.Sync.Conflict(col, node);
			if ((conflict != null) && conflict.IsFileNameConflict)
			{
				conflict.RenameConflictingFile(newFileName);
				conflict.Resolve(Path.GetFileName(conflict.NonconflictedPath));
			}
			else
			{
				throw new Exception("RenameConflictingAndResolve can only be called on a name collision conflict.");
			}
		}

		
	
	
		private string formatFileSize(long fileLength)
		{
			string fileSize;

			if (fileLength < kilobyte)
			{
				fileSize = fileLength.ToString() + " bytes";
			}
			else if (fileLength < megabyte)
			{
				fileSize = Math.Round((double)fileLength / kilobyte, 4).ToString() + " KB";
			}
			else
			{
				fileSize = Math.Round((double)fileLength / megabyte, 4).ToString() + " MB";
			}

			return fileSize;
		}
	}
}
