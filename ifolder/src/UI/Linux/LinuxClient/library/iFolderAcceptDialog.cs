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
  *                 $Author: Calvin Gaisford <cgaisford@novell.com>
  *                          Boyd Timothy <btimothy@novell.com>
  *                 $Modified by: <Modifier>
  *                 $Mod Date: <Date Modified>
  *                 $Revision: 0.0
  *-----------------------------------------------------------------------------
  * This module is used to:
  *        <Description of the functionality of the file >
  *
  *
  *******************************************************************************/

using Gtk;
using System;

namespace Novell.iFolder
{
    /// <summary>
    /// class iFolderAcceptDialog
    /// </summary>
	public class iFolderAcceptDialog : FileChooserDialog
	{
		private string	initialPath;
		private bool merge;
		
        /// <summary>
        /// Gets the Path selected
        /// </summary>
		public new string Path
		{
			get
			{
				return this.Filename;
			}
		}
		
        /// <summary>
        /// Dummy Constructor
        /// </summary>
        public iFolderAcceptDialog(iFolderWeb ifolder, string initialPath) : this(ifolder, initialPath, false)
		{
		}
		
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ifolder">iFolder Web Object</param>
        /// <param name="initialPath">Initial Path</param>
        /// <param name="merge">true if merge else false</param>
		public iFolderAcceptDialog(iFolderWeb ifolder, string initialPath, bool merge)
				: base("", "", null, FileChooserAction.SelectFolder, Stock.Cancel, ResponseType.Cancel)
        {
        	if( !merge)
        	{
				this.Title =
					string.Format(Util.GS("Download \"{0}\" to..."), ifolder.Name);
			}
			else
			{
				this.Title =
					string.Format(Util.GS("Merge \"{0}\" to..."), ifolder.Name);			
			}
        	this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));

        	this.initialPath = initialPath;

        	this.SelectMultiple = false;
        	this.LocalOnly = true;
        	//this.CurrentName = ifolder.Name;
        	
        	if (this.initialPath != null && this.initialPath.Length > 0)
        		this.SetCurrentFolder(this.initialPath);
        		
        	if( !merge)
				this.AddButton(Util.GS("_Download"), ResponseType.Ok);
			else
				this.AddButton(Util.GS("_Merge"), ResponseType.Ok);			
        }
	}
}
