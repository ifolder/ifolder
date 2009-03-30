/***********************************************************************
 |  $RCSfile: iFolderAcceptDialog.cs,v $
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
 |  Authors:
 |		Calvin Gaisford <cgaisford@novell.com>
 |		Boyd Timothy <btimothy@novell.com>
 |
 ***********************************************************************/

using Gtk;
using System;

namespace Novell.iFolder
{
	public class iFolderAcceptDialog : FileChooserDialog
	{
		private string	initialPath;
		
		public new string Path
		{
			get
			{
				return this.CurrentFolder;
			}
		}
		
		public iFolderAcceptDialog(iFolderWeb ifolder, string initialPath)
				: base("", "", null, FileChooserAction.SelectFolder, Stock.Cancel, ResponseType.Cancel)
        {
			this.Title =
				string.Format(Util.GS("Download \"{0}\" to..."), ifolder.Name);
        	this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));

        	this.initialPath = initialPath;

        	this.SelectMultiple = false;
        	this.LocalOnly = true;
        	this.CurrentName = ifolder.Name;
        	
        	if (this.initialPath != null && this.initialPath.Length > 0)
        		this.SetCurrentFolder(this.initialPath);
        		
			this.AddButton(Util.GS("_Download"), ResponseType.Ok);
        }
	}
}
