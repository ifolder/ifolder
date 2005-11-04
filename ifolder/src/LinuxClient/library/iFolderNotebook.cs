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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/


using Gtk;
using System;
using System.Collections;

namespace Novell.iFolder
{
	public class iFolderNotebook : Gtk.VBox
	{
		private Notebook	myNotebook;
		private Toolbar		myToolbar;
		private Tooltips	myTooltips;
		
		private Hashtable	myPages;
		private Hashtable	myToolbarButtons;
		
		public int CurrentPage
		{
			get
			{
				return myNotebook.CurrentPage;
			}
			set
			{
				try
				{
					ToggleToolButton toggleButton;

					Widget child = myNotebook.CurrentPageWidget;
					if (child != null)
					{
						toggleButton =
							(ToggleToolButton)myToolbarButtons[child];
						if (toggleButton != null && toggleButton.Active == true)
						{
							toggleButton.Toggled -=
								new EventHandler(OnToolbarButtonToggled);
							toggleButton.Active = false;
							toggleButton.Toggled +=
								new EventHandler(OnToolbarButtonToggled);
						}
					}
					
					myNotebook.CurrentPage = value;
					
					toggleButton =
						(ToggleToolButton)myToolbarButtons[myNotebook.CurrentPageWidget];
					if (toggleButton != null && toggleButton.Active == false)
					{
						toggleButton.Toggled -=
							new EventHandler(OnToolbarButtonToggled);
						toggleButton.Active = true;
						toggleButton.Toggled +=
							new EventHandler(OnToolbarButtonToggled);
					}
				}
				catch{}
			}
		}
	
		public iFolderNotebook() : base(false, 0)
		{
			myPages = new Hashtable();
			myToolbarButtons = new Hashtable();

			SetupWidgets();
		}
		
		private void SetupWidgets()
		{
			this.PackStart(CreateToolbar(), false, false, 0);
			this.PackStart(CreateNotebook(), true, true, 0);
		}
		
		private Widget CreateToolbar()
		{
			myToolbar = new Toolbar();
			myTooltips = new Tooltips();
			
			return myToolbar;
		}
		
		private Widget CreateNotebook()
		{
			myNotebook = new Notebook();
			myNotebook.ShowTabs = false;
			
			return myNotebook;
		}
		
		/// <summary>
		///	Appends a page to the notebook using a ToggleToolButton instead of
		/// the normal Notebook Tab.
		/// </summary>
		public int AppendPage(Widget child, ToggleToolButton tab)
		{
			myToolbar.Insert(tab, -1);
		
			int pageNum = myNotebook.AppendPage(child, null);
			
			myToolbarButtons[child] = tab;
			myPages[tab] = child;
			
			tab.Toggled += new EventHandler(OnToolbarButtonToggled);

			child.ShowAll();
			
			return pageNum;
		}
		
		private void OnToolbarButtonToggled(object o, EventArgs args)
		{
			Widget child = (Widget)myPages[o];
			if (child != null)
			{
				int pageNum = myNotebook.PageNum(child);
				this.CurrentPage = pageNum;
			}
		}
	}
}
