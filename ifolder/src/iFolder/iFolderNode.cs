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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;
using Simias.Storage;

namespace Novell.iFolder
{
	/// <summary>
	/// The default type is "work" and "internet".
	/// There can exist only one "preferred" email address per contact.
	/// "internet" and "x400" are exclusive to each other
	/// </summary>
	public enum NodeType : uint
	{
		/// <summary>
		/// node represents a file
		/// </summary>
		file = 0x00000001,

		/// <summary>
		/// node represents a folder
		/// </summary>
		folder = 0x00000002
	}


	/// <summary>
	/// Represents a file or folder in an iFolder.
	/// </summary>
	public class iFolderNode
	{
#region Class Members
		private iFolder 	ifolder;
		private Node 		node;
		private NodeType	type;

		private const string iFolderFileType = "File";
		private const string iFolderDirectoryType = "Directory";
#endregion




#region Properties
		/// <summary>
		/// Gets the iFolder to which this iFoldeNode belongs
		/// </summary>
		public iFolder iFolder
		{
			get
			{
				return(ifolder);
			}
		}

		/// <summary>
		/// Gets the well-known node type of the iFolderFile.
		/// </summary>
		public NodeType Type
		{
			get
			{
				return(type);
			}
		}

		/// <summary>
		/// Gets/sets the name of the iFolderFile.
		/// </summary>
		public string Name
		{
			get
			{
				return(node.Name);
			}

			set
			{
				node.Name = value;
			}
		}

		/// <summary>
		/// Gets/sets the description of the iFolderFile.
		/// </summary>
		public string Description
		{
			get
			{
				return (node.Properties.
					GetSingleProperty("Description").ToString());
			}

			set
			{
				node.Properties.ModifyProperty("Description", value);
			}
		}
#endregion




#region Constructors
		internal iFolderNode(iFolder ifolder, Node node)
		{
			this.ifolder= ifolder;
			this.node = node;

			// Set the type on the node
			if(node.Type == iFolderFileType)
				this.type = NodeType.file;
			else if(node.Type == iFolderDirectoryType)
				this.type = NodeType.folder;
		}
#endregion
	}
}
