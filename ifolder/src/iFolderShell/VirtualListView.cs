/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com
 |
 | Author: Bruce Getter <bgetter@novell.com>
 |**************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Novell.Win32Util;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for VirtualListView.
	/// </summary>
	[ComVisible(false)]
	public class VirtualListView : System.Windows.Forms.ListView
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public VirtualListView()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitComponent call
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Properties
		/// <summary>
		/// Gets/sets the number of items in the listview.
		/// </summary>
		public int Count
		{
			get
			{
				return Win32.SendMessage(Handle, Win32.LVM_GETITEMCOUNT, 0, 0);
			}
			set
			{
				Win32.SendMessage(this.Handle, Win32.LVM_SETITEMCOUNT, value, Win32.LVSICF_NOINVALIDATEALL | Win32.LVSICF_NOSCROLL);
			}
		}

		/// <summary>
		/// Gets the number of items selected in the list view.
		/// </summary>
		public int SelectedCount
		{
			get
			{
				return Win32.SendMessage(Handle, Win32.LVM_GETSELECTEDCOUNT, 0, 0);
			}
		}

		/// <summary>
		/// Gets an array containing the indices of the selected items.
		/// </summary>
		new public ArrayList SelectedIndices
		{
			get
			{
				ArrayList list = new ArrayList();

				int index = -1;
				while (true)
				{
					index = Win32.SendMessage(Handle, Win32.LVM_GETNEXTITEM, index, Win32.LVNI_SELECTED);
					if (index == -1)
						break;

					list.Add(index);
				}

				return list;
			}
		}

		/// <summary>
		/// Gets an array of the selected items.
		/// </summary>
		new public ArrayList SelectedItems
		{
			get
			{
				ArrayList list = new ArrayList();

				Win32.LVITEM lvi = new Win32.LVITEM();

				lvi.iItem = -1;
				while (true)
				{
					lvi.iItem = Win32.SendMessage(Handle, Win32.LVM_GETNEXTITEM, lvi.iItem, Win32.LVNI_SELECTED);
					if (lvi.iItem == -1)
						break;

					lvi.mask = Win32.LVIF_TEXT | Win32.LVIF_IMAGE;
					lvi.cchTextMax = 1024;
					lvi.pszText = Marshal.AllocHGlobal(lvi.cchTextMax);
					try
					{

						int result = Win32.SendMessage(
							Handle,
							Win32.LVM_GETITEM,
							0,
							ref lvi);
						if (result > 0)
						{
							string str = Marshal.PtrToStringUni(lvi.pszText);
							list.Add(new ListViewItem(str, lvi.iImage));
						}
					}
					finally
					{
						Marshal.FreeHGlobal(lvi.pszText);
					}
				}

				return list;
			}
		}

		/// <summary>
		/// Gets the index of the top-most viewable item in the listview.
		/// </summary>
		public int TopItemIndex
		{
			get 
			{
				return Win32.SendMessage(Handle, Win32.LVM_GETTOPINDEX, 0, 0);
			}
		}
		#endregion

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.Style |= Win32.LVS_OWNERDATA | Win32.LVS_REPORT | Win32.LVS_SHOWSELALWAYS;

				return cp;
			}
		}

		protected override void WndProc(ref Message m)
		{
			switch(m.Msg)
			{
				case Win32.WM_DESTROY:
					// Remove the items from the listview (this prevents an exception that occurs
					// when there are selected items in the list).
					Win32.SendMessage(Handle, Win32.LVM_DELETEALLITEMS, 0, 0);
					base.WndProc(ref m);
					break;

				default:
					try
					{
						base.WndProc(ref m);
					}
					catch {}
					break;
			}
		}
	}
}
