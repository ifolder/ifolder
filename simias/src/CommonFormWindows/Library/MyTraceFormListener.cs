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
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;

namespace Simias
{
	/// <summary>
	/// My Trace Listener
	/// </summary>
	public class MyTraceFormListener : TraceListener
	{
		private static readonly string NO_CATEGORY = "?";

		private ListView list;
		private TreeView tree;
		private bool scrollLock = false;
		private decimal sizeLimit = -1;

		public delegate void WriteDelegate(string message, string category);

		/// <summary>
		/// Default Constructor
		/// </summary>
		public MyTraceFormListener(TreeView tree, ListView list)
		{
			this.tree = tree;
			this.list = list;
		}

		/// <summary>
		/// Write a message to the trace listener.
		/// </summary>
		/// <param name="message">The trace message.</param>
		public override void Write(string message)
		{
			WriteLine(message, NO_CATEGORY);
		}

		/// <summary>
		/// Write a message to the trace listener.
		/// </summary>
		/// <param name="message">The trace message.</param>
		public override void WriteLine(string message)
		{
			WriteLine(message, NO_CATEGORY);
		}

		/// <summary>
		/// Write a message to the trace listener.
		/// </summary>
		/// <param name="message">The trace message.</param>
		/// <param name="category">The category of the message.</param>
		public override void Write(string message, string category)
		{
			WriteLine(message, category);
		}

		/// <summary>
		/// Write a message to the trace listener.
		/// </summary>
		/// <param name="message">The trace message.</param>
		/// <param name="category">The category of the message.</param>
		public override void WriteLine(string message, string category)
		{
			tree.Invoke(new WriteDelegate(WriteCallBack), new string[] { message, category });
		}

		public void WriteCallBack(string message, string category)
		{
			
			// add category
			TreeNode node = GetCategory(category);

			string[] categories = Regex.Split(category, @"(\.)", RegexOptions.Compiled);

			// add message
			ListViewItem item = new ListViewItem(new string[]
				{ message, categories[categories.Length - 1],
					DateTime.Now.ToShortTimeString() + " "
					+ DateTime.Now.ToShortDateString() });
			item = list.Items.Add(item);

			if ((sizeLimit != -1) && (list.Items.Count > sizeLimit))
			{
				while (list.Items.Count > sizeLimit)
				{
					list.Items.RemoveAt(0);
				}
			}
			
			// associate category and message
			if ((node.Tag == null) || (node.GetType() != typeof(ArrayList)))
			{
				node.Tag = new ArrayList();
			}

			// TODO: a database of options?
			if (node.Checked)
			{
				(node.Tag as ArrayList).Add(item);

				if (!scrollLock)
				{
					item.EnsureVisible();
				}
			}
		}

		private TreeNode GetCategory(string category)
		{
			TreeNode node = null;

			string[] list = Regex.Split(category, @"(\.)", RegexOptions.Compiled);
			
			tree.BeginUpdate();

			TreeNodeCollection nodes = tree.Nodes;

			for(int i=0; i < list.Length; i++)
			{
				string text = list[i];

				// skip periods
				if (text.Equals(".")) continue;

				node = ContainsNodeWithText(nodes, text);

				if (node == null)
				{
					node = nodes.Add(text);
					node.Checked = true;
				}

				node.EnsureVisible();

				nodes = node.Nodes;
			}

			tree.EndUpdate();

			return node;
		}

		private TreeNode ContainsNodeWithText(TreeNodeCollection nodes, string text)
		{
			TreeNode result = null;

			if ((nodes != null) && (nodes.Count > 0))
			{
				foreach(TreeNode node in nodes)
				{
					if (node.Text.Equals(text))
					{
						result = node;
						break;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// The state of the scroll lock.
		/// </summary>
		public bool ScrollLock
		{
			set { scrollLock = value; }
		}

		/// <summary>
		/// The number of entries the list view will hold.
		/// </summary>
		public decimal SizeLimit
		{
			get { return sizeLimit; }
			set { sizeLimit = value; }
		}
	}
}
