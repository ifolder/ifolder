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
		private Node conflictNode;
		private string conflictPath;
		private bool check = false;
		private bool nameValidated = false;
		#endregion

		/// <summary>
		/// Instantiates a CollisionNode object.
		/// </summary>
		public CollisionNode()
		{
		}

		#region Properties
		/// <summary>
		/// Gets/sets the FileNode representing the local file.
		/// </summary>
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

		/// <summary>
		/// Gets/sets the node representing the conflict.
		/// </summary>
		public Node ConflictNode
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

		/// <summary>
		/// Gets/sets the path of the conflict files.
		/// </summary>
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

		/// <summary>
		/// Gets/sets a value indicating whether to use the local file when resolving an update conflict.
		/// </summary>
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

		/// <summary>
		/// Gets/sets a value indicating whether a valid name has been entered for a name conflict.
		/// </summary>
		public bool NameValidated
		{
			get
			{
				return nameValidated;
			}

			set
			{
				nameValidated = value;
			}
		}
		#endregion
	}
}
