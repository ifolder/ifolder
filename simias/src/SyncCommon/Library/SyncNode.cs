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
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.IO;

using Simias;
using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Node
	/// </summary>
	public class SyncNode : IDisposable
	{
		/// <summary>
		/// The file node type string.
		/// </summary>
		public static readonly string FileNodeType = "File";
		
		/// <summary>
		/// The directory node type string.
		/// </summary>
		public static readonly string DirectoryNodeType = "Directory";

		private Node baseNode;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="node">The base node object.</param>
		public SyncNode(Node node)
		{
			this.baseNode = node;
		}

		/// <summary>
		/// Commit any changes to the base node.
		/// </summary>
		public virtual void Commit()
		{
			baseNode.Commit();
		}
		
		/// <summary>
		/// Delete the base node and its file entries.
		/// </summary>
		public void Delete()
		{
			ulong incarnation = baseNode.MasterIncarnation;

			foreach(FileSystemEntry fse in baseNode.GetFileSystemEntryList())
			{
				fse.Delete(true);
			}

			Node ts = baseNode.Delete();

			// check directory
			if (IsDirectory)
			{
				string dirPath = Path.Combine(Path.GetDirectoryName(baseNode.CollectionNode.DocumentRoot.LocalPath), NodePath);

				if (Directory.Exists(dirPath))
				{
					Directory.Delete(dirPath, true);
				}
			}

			// remove tombstones of nodes that have not been synced 
			if (incarnation == 0)
			{
				ts.Delete();
			}

			Dispose();
		}

		/// <summary>
		/// Update the incarnation number of the base node.
		/// </summary>
		/// <param name="incarnation">The new incarnation number.</param>
		public void UpdateIncarnation(ulong incarnation)
		{
			// sync role
			baseNode.CollectionNode.LocalStore.ImpersonateUser(Access.SyncOperatorRole);

			baseNode.UpdateIncarnation(incarnation);

			// remove sync role
			baseNode.CollectionNode.LocalStore.Revert();
		}

		/// <summary>
		/// Get the node object data in xml format.
		/// </summary>
		/// <returns>An xml data string of the node object.</returns>
		public string ToXml()
		{
			return baseNode.CollectionNode.LocalStore.ExportSingleNodeToXml(baseNode.CollectionNode, ID).OuterXml;
		}

		/// <summary>
		/// Get a property value from the base node.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <returns>The value of the property.</returns>
		public object GetProperty(string name)
		{
			return GetProperty(name, null);
		}

		/// <summary>
		/// Get a poperty value from the base node.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="defaultValue">A default value to return if the property has no value.</param>
		/// <returns>The property value, if it exists, or the default value.</returns>
		public object GetProperty(string name, object defaultValue)
		{
			object result = defaultValue;

			Property p = baseNode.Properties.GetSingleProperty(name);

			if (p != null)
			{
				result = p.Value;
			}

			return result;
		}

		/// <summary>
		/// Set the value of the given property of the base node.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <param name="value">The new property value.</param>
		public void SetProperty(string name, object value)
		{
			SetProperty(name, value, false);
		}

		/// <summary>
		/// Set the value of the given property of the base node.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <param name="value">The new property value.</param>
		/// <param name="local">Is this a local only property? (non-synced)</param>
		public void SetProperty(string name, object value, bool local)
		{
			if (value != null)
			{
				Property p = new Property(name, value);
				p.LocalProperty = local;

				baseNode.Properties.ModifyProperty(p);
			}
			else
			{
				baseNode.Properties.DeleteSingleProperty(name);
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose this object.
		/// </summary>
		public virtual void Dispose()
		{
			this.baseNode = null;
		}

		#endregion

		#region Properties
		
		/// <summary>
		/// The base node object.
		/// </summary>
		public Node BaseNode
		{
			get { return baseNode; }
		}

		/// <summary>
		/// The node ID.
		/// </summary>
		public string ID
		{
			get { return baseNode.Id; }
		}

		/// <summary>
		/// The node name.
		/// </summary>
		public string Name
		{
			get { return baseNode.Name; }
		}

		/// <summary>
		/// Is the base node a file node?
		/// </summary>
		public bool IsFile
		{
			get { return baseNode.Type.Equals(FileNodeType); }
		}

		/// <summary>
		/// Is the base node a directory node?
		/// </summary>
		public bool IsDirectory
		{
			get { return baseNode.Type.Equals(DirectoryNodeType); }
		}

		/// <summary>
		/// The node path.
		/// </summary>
		public string NodePath
		{
			get
			{
				string path = baseNode.PathName;

				if (path.StartsWith(@"/") || path.StartsWith(@"\\"))
				{
					path = "." + path;
				}

				return path;
			}
		}

		/// <summary>
		/// The master incarnation number of the base node.
		/// </summary>
		public ulong MasterIncarnation
		{
			get { return baseNode.MasterIncarnation; }
		}

		/// <summary>
		/// The local incarantion number of the base node.
		/// </summary>
		public ulong LocalIncarnation
		{
			get { return baseNode.LocalIncarnation; }
		}

		#endregion
	}
}
