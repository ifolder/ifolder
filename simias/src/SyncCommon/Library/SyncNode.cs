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
		public static readonly string FileNodeType = "File";
		public static readonly string DirectoryNodeType = "Directory";

		private Node baseNode;

		public SyncNode(Node node)
		{
			this.baseNode = node;
		}

		public virtual void Commit()
		{
			baseNode.Commit();
		}
		
		public void Delete()
		{
			ulong incarnation = baseNode.MasterIncarnation;

			foreach(NodeStream ns in baseNode.GetStreamList())
			{
				ns.Delete(true);
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

		public void UpdateIncarnation(ulong incarnation)
		{
			// sync role
			baseNode.CollectionNode.LocalStore.ImpersonateUser(Access.SyncOperatorRole);

			baseNode.UpdateIncarnation(incarnation);

			// remove sync role
			baseNode.CollectionNode.LocalStore.Revert();
		}

		public string ToXml()
		{
			return baseNode.CollectionNode.LocalStore.ExportSingleNodeToXml(baseNode.CollectionNode, ID).OuterXml;
		}

		public object GetProperty(string name)
		{
			return GetProperty(name, null);
		}

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

		public void SetProperty(string name, object value)
		{
			SetProperty(name, value, false);
		}

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

		public virtual void Dispose()
		{
			this.baseNode = null;
		}

		#endregion

		#region Properties
		
		public Node BaseNode
		{
			get { return baseNode; }
		}

		public string ID
		{
			get { return baseNode.Id; }
		}

		public string Name
		{
			get { return baseNode.Name; }
		}

		public bool IsFile
		{
			get { return baseNode.Type.Equals(FileNodeType); }
		}

		public bool IsDirectory
		{
			get { return baseNode.Type.Equals(DirectoryNodeType); }
		}

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

		public ulong MasterIncarnation
		{
			get { return baseNode.MasterIncarnation; }
		}

		public ulong LocalIncarnation
		{
			get { return baseNode.LocalIncarnation; }
		}

		#endregion
	}
}
