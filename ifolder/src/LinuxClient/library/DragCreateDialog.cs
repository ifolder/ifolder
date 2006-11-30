/***********************************************************************
 *  $RCSfile: DragCreateDialog.cs,v $
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
 *  Authors:
 *		Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/

using Gtk;
using System;

namespace Novell.iFolder
{
	public class DragCreateDialog : Dialog
	{
		private DomainInformation[]	domains;
		private string				defaultDomainID;
		private ComboBox			domainComboBox;
		private ComboBox			security_lvl_ComboBox;
		private string				initialPath;
		private string				folderName;
		private string				folderPath;
//		TextView						descriptionTextView;
		private Expander				optionsExpander;

		public string iFolderPath
		{
			get
			{
				return this.initialPath;
			}
		}

		public string DomainID
		{
			get
			{
				int activeIndex = domainComboBox.Active;
				if (activeIndex >= 0)
					return domains[activeIndex].ID;
				else
					return "0";
			}
		}
		public int Encrypt_Status
		{
			get
			{
				return security_lvl_ComboBox.Active;
			}
		}
		public string Description
		{
			get
			{
//				return descriptionTextView.Buffer.Text;
				return "";
			}
//			set
//			{
//				descriptionTextView.Buffer.Text = value;
//			}
		}

		///
		/// defaultDomainID: If the main iFolders window is currently
		/// filtering the list of domains, this parameter is used to allow this
		/// dialog to respect the currently selected domain.
		public DragCreateDialog(Gtk.Window parentWindow, DomainInformation[] domainArray, string defaultDomainID, string initialPath)
				: base(Util.GS("Convert to an iFolder..."), parentWindow,
				DialogFlags.Modal | DialogFlags.DestroyWithParent | DialogFlags.NoSeparator,
				Stock.Help, ResponseType.Help, Stock.Cancel, ResponseType.Cancel, Stock.Ok, ResponseType.Ok)
		{
			domains = domainArray;
			this.defaultDomainID = defaultDomainID;

			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));

			this.initialPath = initialPath;
			
//			this.SetPolicy((int)this.AllowShrink, (int)this.AllowGrow, 1);
			
//			if (this.initialPath != null && this.initialPath.Length > 0)
//				this.SetCurrentFolder(this.initialPath);
			Widget widgets = SetupWidgets();
			widgets.ShowAll();

			this.VBox.Add(widgets);
		}
		
		private Widget SetupWidgets()
		{
			VBox vbox = new VBox();
			vbox.BorderWidth = 10;
			vbox.Spacing = 10;
			
			Table table = new Table(3, 2, false);
			vbox.PackStart(table, true, true, 0);
			
			table.ColumnSpacing = 12;
			table.RowSpacing = 12;
			
			int lastSlashPos =
				initialPath.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
			
			if (lastSlashPos < 0)
			{
				folderName = initialPath;
				folderPath = "";
			}
			else
			{
				folderName = initialPath.Substring(lastSlashPos + 1);
				folderPath = initialPath.Substring(0, lastSlashPos);
			}
			
			///
			/// Name
			///
			Label label = new Label(string.Format("Name:"));
			label.Xalign = 0;
			table.Attach(label, 0, 1, 0, 1,
						 AttachOptions.Shrink | AttachOptions.Fill, 0, 0, 0);
			
			label = new Label(folderName);
			label.Xalign = 0;
			label.UseUnderline = false;
			table.Attach(label, 1, 2, 0, 1,
						 AttachOptions.Shrink | AttachOptions.Fill, 0, 0, 0);
			
			///
			/// Folder
			///
			label = new Label(string.Format("Folder:"));
			label.Xalign = 0;
			label.Yalign = 0;
			table.Attach(label, 0, 1, 1, 2,
						 AttachOptions.Shrink | AttachOptions.Fill, 0, 0, 0);
			
			label = new Label(folderPath);
			label.Xalign = 0;
			label.UseUnderline = false;
			table.Attach(label, 1, 2, 1, 2,
						 AttachOptions.Shrink | AttachOptions.Fill, 0, 0, 0);
			
			// More options expander
			table.Attach(CreateMoreOptionsExpander(defaultDomainID),
						 0, 2, 2, 3,
						 AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
			
			return vbox;
		}
		
		private Widget CreateMoreOptionsExpander(string defaultDomainID)
		{
			optionsExpander = new Expander(Util.GS("More options"));
			optionsExpander.Activated += new EventHandler(OnOptionsExpanded);

			Table optionsTable = new Table(2, 3, false);
			optionsExpander.Add(optionsTable);
			
			optionsTable.ColumnSpacing = 10;
			optionsTable.RowSpacing = 10;
			optionsTable.SetColSpacing(0, 30);
			
			Label l = new Label(Util.GS("iFolder Account:"));
			l.Xalign = 0;
			optionsTable.Attach(l, 1,2,0,1,
								AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			l = new Label(Util.GS("Security Level:"));
			l.Xalign = 0;
			optionsTable.Attach(l, 1,2,1,2, AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			// Set up Domains
			domainComboBox = ComboBox.NewText();
			optionsTable.Attach(domainComboBox, 2,3,0,1,
								AttachOptions.Expand | AttachOptions.Fill, 0,0,0);

			security_lvl_ComboBox = ComboBox.NewText();
			optionsTable.Attach(security_lvl_ComboBox, 2,3,1,2,
								AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			security_lvl_ComboBox.Active = 0;

			
			int defaultDomain = 0;
			for (int x = 0; x < domains.Length; x++)
			{
				domainComboBox.AppendText(domains[x].Name);
				if (defaultDomainID != null)
				{
					if (defaultDomainID == domains[x].ID)
						defaultDomain = x;
				}
				else
					defaultDomain = x;
			}
			
			domainComboBox.Active = defaultDomain;

/*
			l = new Label(Util.GS("Description:"));
			l.Xalign = 0;
			optionsTable.Attach(l, 1,2,1,2,
								AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			
			descriptionTextView = new TextView();
			descriptionTextView.LeftMargin = 4;
			descriptionTextView.RightMargin = 4;
			descriptionTextView.Editable = true;
			descriptionTextView.CursorVisible = true;
			descriptionTextView.AcceptsTab = false;
			descriptionTextView.WrapMode = WrapMode.WordChar;
			
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = ShadowType.EtchedIn;
			sw.Add(descriptionTextView);
			optionsTable.Attach(sw, 2,3,1,2,
								AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
*/
			
			optionsTable.ShowAll();
			
			return optionsExpander;
		}
		
		private void OnOptionsExpanded(object o, EventArgs args)
		{
			// Resize the dialog
			if (!optionsExpander.Expanded)
				this.Resize(20, 20);
		}
	}
}
