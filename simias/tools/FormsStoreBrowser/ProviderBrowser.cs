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
using System.Net;

namespace StoreBrowser
{
	/// <summary>
	/// Summary description for ProviderBrowser.
	/// </summary>
	public class ProviderBrowser : IStoreBrowser
	{
		TreeView tView;
		RichTextBox  rBox;
		BrowserService browser;
		bool alreadyDisposed;

		public ProviderBrowser(TreeView view, RichTextBox box, string host, string userName, string password)
		{
			tView = view;
			rBox = box;
			browser = new BrowserService();
			browser.Url = host + "/SimiasBrowser.asmx";
			rBox.Show();
			tView.Dock = DockStyle.Left;
			alreadyDisposed = false;

			if (userName != null && userName != "")
			{
				this.browser.Credentials = new NetworkCredential(userName, password, host);
				this.browser.CookieContainer = new CookieContainer();
			}
			else
			{
				Simias.Client.LocalService.Start(browser);
			}
		}

		~ProviderBrowser()
		{
			Dispose(true);
		}

		public BrowserService StoreBrowser
		{
			get { return browser; }
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
			BrowserNode[] list = browser.EnumerateCollections();
			foreach(BrowserNode bn in list )
			{
				DisplayNode dspNode = new DisplayNode(bn);
				TreeNode colNode = new TreeNode(dspNode.ID);
				tNode.Nodes.Add(colNode);
				colNode.Tag = dspNode;
				colNode.Nodes.Add("Temp");
			}

			tView.EndUpdate();
		}

		public void ShowNode(TreeNode node)
		{
			rBox.BringToFront();

			if (node.Tag != null)
			{
				DisplayNode dspNode = (DisplayNode)node.Tag;
				StringWriter sw = new StringWriter();
				writeXml(dspNode.Document, sw);
				rBox.Clear();
				rBox.AppendText(sw.ToString());
			}
			else
			{
				rBox.Clear();
			}
		}

		public void AddChildren(TreeNode colNode)
		{
			if (colNode.Tag == null)
			{
				addCollections(colNode);
			}
			else
			{
				DisplayNode dspNode = (DisplayNode)colNode.Tag;
				if (dspNode.IsCollection )
				{
					BrowserNode[] list = browser.EnumerateNodes( dspNode.ID );
					foreach(BrowserNode bn in list)
					{
						DisplayNode n = new DisplayNode(bn);
						if (n.ID != dspNode.ID)
						{
							TreeNode tn = new TreeNode(n.ID, 1, 1);
							tn.Tag = n;
							colNode.Nodes.Add(tn);
						}
					}
				}
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
