/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;
using System.Text;
using System.IO;
using Simias.Storage;

namespace Novell.iFolder
{
	/// <summary>
	/// Represents a file in an iFolder.
	/// </summary>
	public class iFolderFile
	{
		#region Class Members

		private Collection collection;
		private iFolder ifolder;
		private Node parentNode;
		private Node thisNode;
		private NodeStream thisNodeStream;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the well-known node type of the iFolderFile.
		/// </summary>
		public string Type
		{
			get
			{
				return (thisNode.Type);
			}
		}

		/// <summary>
		/// Gets/sets the name of the iFolderFile.
		/// </summary>
		public string Name
		{
			get
			{
				return(thisNode.Name);
			}

			set
			{
				thisNode.Name = value;
			}
		}

		/// <summary>
		/// Gets/sets the description of the iFolderFile.
		/// </summary>
		public string Description
		{
			get
			{
				return (thisNode.Properties.GetSingleProperty("Description").ToString());
			}

			set
			{
				thisNode.Properties.ModifyProperty("Description", value);
			}
		}

/*
		/// TODO - do we need this any longer?  The File node holds this now.
 		/// <summary>
		/// Get/set the relative path of this iFolderFile
		/// </summary>
		public string RelativeName
		{
			get
			{
				return (thisNode.Properties.GetSingleProperty("IFRelativeName").ToString());
			}

			set
			{
				thisNode.Properties.AddProperty("IFRelativeName", value);
			}
		}
*/

		/// <summary>
		/// Gets/sets the parent node of the iFolderFile.
		/// </summary>
		public Node ParentNode
		{
			get
			{
				return (parentNode);
			}

			set
			{
				parentNode = value;
			}
		}

		/// <summary>
		/// Gets the node of the iFolderFile.
		/// </summary>
		public Node ThisNode
		{
			get
			{
				return (thisNode);
			}

			set
			{
				thisNode = value;
			}
		}
		#endregion
		
		#region Constructors
		//internal iFolderFile(iFolder ifolder, Node node)
		internal iFolderFile(Collection collection, iFolder ifolder, Node node)
		{
			this.collection= collection;
			this.ifolder= ifolder;
			ParentNode= node;
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Create an iFolderFile node in the parentNode.
		/// </summary>
		/// <param name="fileName">The file/directory name of the iFolderFile node to create.</param>
		/// <param name="isFullPath">Set to true if fileName specifies the full path and name; otherwise, set to false.</param>
		/// <param name="type">The type associated with this iFolderFile node.</param>
		internal void Create(string fileName, bool isFullPath, string type)
		{
			// Get the leaf name from the path.
			string name= Path.GetFileName(fileName);

			// Make sure the node doesn't already exist.
			iFolderFile ifolderfile = ifolder.GetiFolderFileByName(fileName);
			if (ifolderfile == null)
			{
				// Create the node.
				thisNode= parentNode.CreateChild(name, type);

				string relativePath= isFullPath ?
					fileName.Remove(0, ifolder.LocalPath.Length + 1) :
					fileName;

				// Create the stream node.
				if (type == iFolder.iFolderFileType)
				{
					thisNodeStream= thisNode.AddStream(name, relativePath);
				}
			}
			else
			{
				ThisNode = ifolderfile.ThisNode;
			}
		}

		/// <summary>
		/// Load a given iFolderFile node by ID.
		/// </summary>
		/// <param name="fileID">The ID of the iFolderFile node.</param>
		/// <returns></returns>
		internal bool Load(string fileID)
		{
			thisNode= collection.GetNodeById(fileID);
			if ( thisNode != null )
			{
				if ((thisNode.Type == iFolder.iFolderFileType) ||
					(thisNode.Type == iFolder.iFolderDirectoryType))
				{
					// Set the parent node.
					ParentNode= thisNode.GetParent();
					return(true);
				}
			}

			return(false);
		}

		internal bool Load(Node node)
		{
			thisNode= node;

			if ((thisNode.Type == iFolder.iFolderFileType) ||
				(thisNode.Type == iFolder.iFolderDirectoryType) ||
				(thisNode.Type == iFolder.iFolderType))
			{
				return true;
			}

			return false;
		}
		#endregion
	}
}
