using System;
using System.Runtime.InteropServices;
using Simias;
using Simias.Storage;

namespace Novell.iFolder.iFolderCom
{
	/// <summary>
	/// Summary description for CollisionNode.
	/// </summary>
	[ComVisible(false)]
	public class CollisionNode
	{
		#region Class Members
		private FileNode localNode;
		private ShallowNode conflictNode;
		private string conflictPath;
		private bool check = false;
		#endregion

		public CollisionNode()
		{
		}

		#region Properties
		public FileNode LocalNode
		{
			get
			{
				return this.localNode;
			}

			set
			{
				this.localNode = value;
			}
		}

		public ShallowNode ConflictNode
		{
			get
			{
				return this.conflictNode;
			}

			set
			{
				this.conflictNode = value;
			}
		}

		public string ConflictPath
		{
			get
			{
				return this.conflictPath;
			}
			
			set
			{
				this.conflictPath = value;
			}
		}

		public bool Checked
		{
			get
			{
				return check;
			}

			set
			{
				check = value;
			}
		}
		#endregion
	}
}
