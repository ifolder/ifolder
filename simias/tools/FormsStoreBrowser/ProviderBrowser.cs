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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Xml;
using Simias;
using Simias.Storage;
using Simias.Storage.Provider;

namespace StoreBrowser
{
	/// <summary>
	/// Summary description for ProviderBrowser.
	/// </summary>
	public class ProviderBrowser : IStoreBrowser
	{
		TreeView tView;
		RichTextBox  rBox;
		IProvider provider;
		bool alreadyDisposed;

		public ProviderBrowser(TreeView view, RichTextBox box, string dbPath)
		{
			tView = view;
			rBox = box;
			bool created;
			provider = Provider.Connect(new ProviderConfig(Path.GetDirectoryName(dbPath)), out created);
			rBox.Show();
			tView.Dock = DockStyle.Left;
			alreadyDisposed = false;
		}

		~ProviderBrowser()
		{
			Dispose(true);
		}

		public void Show()
		{
			tView.Nodes.Clear();
			tView.BeginUpdate();
			TreeNode providerNode = new TreeNode("Provider");
			tView.Nodes.Add(providerNode);
			providerNode.Nodes.Add("temp");

			tView.EndUpdate();
		}

		private void addCollections(TreeNode tNode)
		{
			tView.BeginUpdate();
			tNode.Nodes.Clear();
			// Get each Collection.
			char [] buffer1 = new char[4096];
			IResultSet results1 = provider.Search(new Query("Types", SearchOp.Equal, "Collection", Syntax.String));
			int len1 = results1.GetNext(ref buffer1);
			if (len1 > 0)
			{
				XmlDocument doc1 = new XmlDocument();
				doc1.LoadXml(new string(buffer1, 0, len1));
				XmlNodeList recordList1 = doc1.DocumentElement.SelectNodes(XmlTags.ObjectTag);
				foreach (XmlElement recordEl1 in recordList1)
				{
					string id = recordEl1.GetAttribute(XmlTags.IdAttr);
					TreeNode colNode = new TreeNode(recordEl1.GetAttribute(XmlTags.IdAttr));
					tNode.Nodes.Add(colNode);
					colNode.Tag = id;
					colNode.Nodes.Add("Temp");
				}
			}
			results1.Dispose();
			tView.EndUpdate();
		}

		public void ShowNode(TreeNode node)
		{
			rBox.BringToFront();
			string id = node.Text;
			TreeNode parent = node.Parent;
            string colId = parent != null ? parent.Text : null;
			StringWriter sw = new StringWriter();
			writeXml(provider.GetRecord(id, colId), sw);
			rBox.Clear();
			rBox.AppendText(sw.ToString());
		}

		public void AddChildren(TreeNode colNode)
		{
			if (colNode.Tag == null)
			{
				addCollections(colNode);
			}
			else
			{
				string id = (string)colNode.Tag;
				char [] buffer = new char[4096];
				IResultSet results = provider.Search(new Query(id, BaseSchema.ObjectName, SearchOp.Begins, "", Syntax.String));
				int len = results.GetNext(ref buffer);
				if (len > 0)
				{
					XmlDocument doc = new XmlDocument();
					string s = new string(buffer, 0, len);
					doc.LoadXml(s);
					XmlNodeList recordList = doc.DocumentElement.SelectNodes(XmlTags.ObjectTag);
					foreach (XmlElement recordEl in recordList)
					{
						string nodeId = recordEl.GetAttribute(XmlTags.IdAttr);
						if (nodeId != id)
						{
							colNode.Nodes.Add(new TreeNode(recordEl.GetAttribute(XmlTags.IdAttr), 1, 1));
						}
					}
				}
				results.Dispose();
			}
		}

		private void writeXml(XmlDocument doc, TextWriter tw)
		{
			if (doc != null)
			{
				XmlTextWriter w = new XmlTextWriter(tw);
				w.Formatting = Formatting.Indented;
				doc.WriteTo(w);
			}
		}

		private void Dispose(bool inFinalize)
		{
			if (!alreadyDisposed)
			{
				if (!inFinalize)
				{
					GC.SuppressFinalize(this);
				}
				alreadyDisposed = true;
				provider.Dispose();
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}
}
