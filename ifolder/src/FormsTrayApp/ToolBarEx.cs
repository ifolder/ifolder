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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using Novell.Win32Util;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Extends the ToolBar class to include additional image lists.
	/// </summary>
	public class ToolBarEx : System.Windows.Forms.ToolBar
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private ImageList disabledImageList;
		private ImageList hotImageList;

		/// <summary>
		/// Constructs a ToolBarEx object.
		/// </summary>
		public ToolBarEx()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
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

		#region Properties
		/// <summary>
		/// Gets or sets the collection of images available to the disabled toolbar button controls.
		/// </summary>
		public ImageList DisabledImageList
		{
			get { return disabledImageList; }
			set 
			{
				disabledImageList = value;
				Win32Window.SendMessage(Handle, Win32Window.TB_SETDISABLEDIMAGELIST, IntPtr.Zero, value.Handle);
			}
		}

		/// <summary>
		/// Gets or sets the collection of images available to the hot toolbar button controls.
		/// </summary>
		public ImageList HotImageList
		{
			get { return hotImageList; }
			set
			{
				hotImageList = value;
				Win32Window.SendMessage(Handle, Win32Window.TB_SETHOTIMAGELIST, IntPtr.Zero, value.Handle);
			}
		}
		#endregion
	}
}
