/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Bruce Getter <bgetter@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

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
				if (value != null)
				{
					disabledImageList = value;
					disabledImageList.RecreateHandle += new EventHandler(disabledImageList_RecreateHandle);
					Win32.SendMessage(Handle, Win32Window.TB_SETDISABLEDIMAGELIST, IntPtr.Zero, disabledImageList.Handle);
				}
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
				if (value != null)
				{
					hotImageList = value;
					hotImageList.RecreateHandle += new EventHandler(hotImageList_RecreateHandle);
					Win32.SendMessage(Handle, Win32Window.TB_SETHOTIMAGELIST, IntPtr.Zero, hotImageList.Handle);
				}
			}
		}
		#endregion

		#region Event Handlers
		private void disabledImageList_RecreateHandle(object sender, EventArgs e)
		{
			Win32.SendMessage(Handle, Win32Window.TB_SETDISABLEDIMAGELIST, IntPtr.Zero, disabledImageList.Handle);
		}

		private void hotImageList_RecreateHandle(object sender, EventArgs e)
		{
			Win32.SendMessage(Handle, Win32Window.TB_SETHOTIMAGELIST, IntPtr.Zero, hotImageList.Handle);
		}
		#endregion
	}
}
