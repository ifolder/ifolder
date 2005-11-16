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
 *  Authors:
 *		Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/


using System;
using System.IO;
using System.Collections;
using Gtk;

using Simias.Client;
using Simias.Client.Event;

using Novell.iFolder.Events;
using Novell.iFolder.Controller;

namespace Novell.iFolder
{
	public delegate void FolderSelectedHandler(object o, FolderSelectedArgs args);

	public class RemoteFoldersWidget : VBox
	{
		// for the statusbar
		const int ctx = 1;
		private iFolderWebService	ifws;
		private SimiasWebService	simws;
		private iFolderData			ifdata;

		private Hashtable			curRemoteFolders;

		// curDomain should be set to the domain selected in the
		// Domain Filter or if "all" domains are selected, this should be
		// set to null.
		private DomainInformation	curDomain;
		private DomainInformation[] curDomains;

		private Gtk.ListStore		DomainListStore;
		private DomainController	domainController;

		// Manager object that knows about simias resources.
		private Manager				simiasManager;

		private ComboBox			remoteDomainsComboBox;
		private Entry				remoteSearchEntry;
		private Button				RemoteCancelSearchButton;
		private uint				remoteSearchTimeoutID;

		private Paned				remotePaned;
		
		private Notebook			RemoteDetailsNotebook;
		
		private Gdk.Pixbuf			remoteFolderPixbuf;

		private ListStore			remoteFoldersListStore;
		private iFolderIconView		remoteFoldersIconView;

		private Button				DownloadAndSyncButton;
		private Button				DeleteFromServerButton;
		
		private Label				RemoteNameLabel;
		private Label				RemoteSizeLabel;
		private Label				RemoteOwnerLabel;
		private Label				RemoteServerLabel;
		private TextView			RemoteDescriptionTextView;
		
		public event FolderSelectedHandler FolderSelected;
		
		public iFolderHolder SelectedFolder
		{
			get
			{
				iFolderHolder holder = null;
				
				TreePath[] selection = remoteFoldersIconView.SelectedItems;
				if (selection.Length > 0)
				{
					TreeModel tModel = remoteFoldersIconView.Model;
					TreeIter iter;
					if (tModel.GetIter(out iter, selection[0]))
						holder = (iFolderHolder)tModel.GetValue(iter, 2);
				}

				return holder;
			}
		}

		/// <summary>
		/// Default constructor for iFolderWindow
		/// </summary>
		public RemoteFoldersWidget(iFolderWebService webService, SimiasWebService SimiasWS)
			: base (false, 0)
		{
			if(webService == null)
				throw new ApplicationException("iFolderWebServices was null");

			ifws = webService;
			simws = SimiasWS;
			ifdata = iFolderData.GetData(Util.GetSimiasManager());
			curRemoteFolders = new Hashtable();

			curDomain = null;
			curDomains = null;

			remoteSearchTimeoutID = 0;
			
			CreateWidgets();

			RefreshDomains(true);
			RefreshFolders(true);
			
			domainController = DomainController.GetDomainController(simiasManager);
			if (domainController != null)
			{
				domainController.DomainAdded +=
					new DomainAddedEventHandler(OnDomainAddedEvent);
				domainController.DomainDeleted +=
					new DomainDeletedEventHandler(OnDomainDeletedEvent);
			}

			// Set up an event to refresh when the window is
			// being drawn
			this.Realized += new EventHandler(OnRealizeWidget);
		}
		
		~RemoteFoldersWidget()
		{
			if (domainController != null)
			{
				domainController.DomainAdded -=
					new DomainAddedEventHandler(OnDomainAddedEvent);
				domainController.DomainDeleted -=
					new DomainDeletedEventHandler(OnDomainDeletedEvent);
			}
		}




		/// <summary>
		/// Set up the UI inside the Window
		/// </summary>
		private void CreateWidgets()
		{
			// Search Bar
			this.PackStart(CreateSearchBar(), false, false, 0);

			// Content Area
			this.PackStart(CreateContentArea(), true, true, 0);
		}
		
		private bool GrabRemoteSearchEntryFocus()
		{
			remoteSearchEntry.GrabFocus();
			return false;
		}		
		
		private Widget CreateSearchBar()
		{
			Frame toolbarFrame = new Frame();
//			vbox.PackStart(toolbarFrame, false, false, 0);
			toolbarFrame.ShadowType = ShadowType.EtchedOut;
			
			HBox hbox = new HBox(false, 4);
			toolbarFrame.Add(hbox);
//			vbox.PackStart(hbox, false, false, 0);

			hbox.BorderWidth = 4;
			
			hbox.PackStart(new Label(""), false, false, 0); // spacer
			
			Label l = new Label(Util.GS("Filter by Server:"));
			hbox.PackStart(l, false, false, 0);

			remoteDomainsComboBox = new ComboBox();
			hbox.PackStart(remoteDomainsComboBox, false, false, 0);

			DomainListStore = new ListStore(typeof(DomainInformation));
			remoteDomainsComboBox.Model = DomainListStore;
			
			CellRenderer domainTR = new CellRendererText();
			remoteDomainsComboBox.PackStart(domainTR, true);

			remoteDomainsComboBox.SetCellDataFunc(domainTR,
				new CellLayoutDataFunc(RemoteDomainComboBoxCellTextDataFunc));

			remoteDomainsComboBox.ShowAll();

			hbox.PackStart(new Label(""), true, true, 0); // spacer
			
			Image stopImage = new Image(Stock.Stop, Gtk.IconSize.Menu);
			stopImage.SetAlignment(0.5F, 0F);
			
			RemoteCancelSearchButton = new Button(stopImage);
			hbox.PackEnd(RemoteCancelSearchButton, false, false, 0);
			RemoteCancelSearchButton.Relief = ReliefStyle.None;
			RemoteCancelSearchButton.Sensitive = false;
			
			RemoteCancelSearchButton.Clicked +=
				new EventHandler(OnRemoteCancelSearchButton);

			remoteSearchEntry = new Entry();
			hbox.PackEnd(remoteSearchEntry, false, false, 0);
			remoteSearchEntry.SelectRegion(0, -1);
			remoteSearchEntry.CanFocus = true;
			remoteSearchEntry.Changed +=
				new EventHandler(OnRemoteSearchEntryChanged);
		
			l = new Label(Util.GS("Search:"));
			hbox.PackEnd(l, false, false, 0);
			
			return toolbarFrame;
		}
		
		private void OnRemoteCancelSearchButton(object o, EventArgs args)
		{
			// FIXME: Implement OnRemoteCancelSearchButton
			remoteSearchEntry.Text = "";
			remoteSearchEntry.GrabFocus();
		}
		
		private void OnRemoteSearchEntryChanged(object o, EventArgs args)
		{
			if (remoteSearchTimeoutID != 0)
			{
				GLib.Source.Remove(remoteSearchTimeoutID);
				remoteSearchTimeoutID = 0;
			}

			if (remoteSearchEntry.Text.Length > 0)
				RemoteCancelSearchButton.Sensitive = true;
			else
				RemoteCancelSearchButton.Sensitive = false;

			remoteSearchTimeoutID = GLib.Timeout.Add(
				500, new GLib.TimeoutHandler(RemoteSearchCallback));
		}
		
		private bool RemoteSearchCallback()
		{
			SearchRemoteFolders();
			return false;
		}
		
		private void SearchRemoteFolders()
		{
			remoteFoldersListStore.Clear();
			curRemoteFolders.Clear();

			string searchString = remoteSearchEntry.Text;
			if (searchString != null)
			{
				searchString = searchString.Trim();
				if (searchString.Length > 0)
					searchString = searchString.ToLower();
			}

			iFolderHolder[] ifolders = ifdata.GetiFolders();
			if(ifolders != null)
			{
				foreach(iFolderHolder holder in ifolders)
				{
					if (holder.iFolder.IsSubscription)
					{
						TreeIter iter;
						if (searchString == null || searchString.Trim().Length == 0)
						{
							// Add everything in
							iter = remoteFoldersListStore.AppendValues(remoteFolderPixbuf, holder.iFolder.Name, holder);
							curRemoteFolders[holder.iFolder.CollectionID] = iter;
						}
						else
						{
							// Search the iFolder's Name (for now)
							string name = holder.iFolder.Name;
							if (name != null)
							{
								name = name.ToLower();
								if (name.IndexOf(searchString) >= 0)
								{
									iter = remoteFoldersListStore.AppendValues(remoteFolderPixbuf, holder.iFolder.Name, holder);
									curRemoteFolders[holder.iFolder.CollectionID] = iter;
								}
							}
						}
					}
				}
			}
			
			remoteFoldersIconView.RefreshIcons();
		}
		
		private Widget CreateContentArea()
		{
			remotePaned = new HPaned();
			remotePaned.Position = 220;
			
			remotePaned.Add1(CreateRemoteActionsPane());
			remotePaned.Add2(CreateRemoteIconViewPane());
			
			return remotePaned;
		}
		
		private Widget CreateRemoteActionsPane()
		{
			RemoteDetailsNotebook = new Notebook();
			RemoteDetailsNotebook.ShowTabs = false;
			RemoteDetailsNotebook.AppendPage(new Label("Select a folder"), null);
			
			
			VBox vbox = new VBox(false, 0);
			RemoteDetailsNotebook.AppendPage(vbox, null);
			vbox.PackStart(CreateRemoteInfo(), false, false, 0);
			vbox.PackStart(CreateRemoteDetails(), true, true, 0);
			
			return RemoteDetailsNotebook;
		}
		
		private Widget CreateRemoteInfo()
		{
			VBox vbox = new VBox(false, 0);
			
			// folder128.png
			Gdk.Pixbuf folderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("folder128.png"));
			folderPixbuf = folderPixbuf.ScaleSimple(64, 64, Gdk.InterpType.Bilinear);
			Image folderImage = new Image(folderPixbuf);
			folderImage.SetAlignment(0.5F, 0);
			
			vbox.PackStart(folderImage, false, false, 0);

			RemoteNameLabel = new Label("");
			RemoteNameLabel.UseMarkup = true;
			RemoteNameLabel.UseUnderline = false;
			RemoteNameLabel.Xalign = 0.5F;
			vbox.PackStart(RemoteNameLabel, false, true, 5);

			RemoteSizeLabel = new Label("47 MB");
			RemoteSizeLabel.UseMarkup = true;
			RemoteSizeLabel.UseUnderline = false;
			RemoteSizeLabel.Xalign = 0.5F;
			vbox.PackStart(RemoteSizeLabel, false, true, 0);
			
			RemoteOwnerLabel = new Label("");
			RemoteOwnerLabel.UseMarkup = true;
			RemoteOwnerLabel.UseUnderline = false;
			RemoteOwnerLabel.Xalign = 0.5F;
			vbox.PackStart(RemoteOwnerLabel, false, true, 0);
			
			RemoteServerLabel = new Label("");
			RemoteServerLabel.UseMarkup = true;
			RemoteServerLabel.UseUnderline = false;
			RemoteServerLabel.Xalign = 0.5F;
			vbox.PackStart(RemoteServerLabel, false, true, 0);
			
			return vbox;
		}
		
		private Widget CreateRemoteDetails()
		{
			VBox vbox = new VBox(false, 0);
			
			Label l = new Label(Util.GS("Description:"));
			l.Xalign = 0;
			vbox.PackStart(l, false, false, 0);

			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			vbox.PackStart(sw, true, true, 0);
			sw.VscrollbarPolicy = PolicyType.Automatic;
			sw.HscrollbarPolicy = PolicyType.Automatic;

			RemoteDescriptionTextView = new TextView();
			RemoteDescriptionTextView.Editable = false;
			RemoteDescriptionTextView.Sensitive = false;
			RemoteDescriptionTextView.WrapMode = WrapMode.Word;
			sw.Add(RemoteDescriptionTextView);
			
			return vbox;
		}
		
		private Widget CreateRemoteIconViewPane()
		{
			ScrolledWindow sw = new ScrolledWindow();
//			sw.ShadowType = Gtk.ShadowType.EtchedIn;

			remoteFoldersListStore = new ListStore(typeof(Gdk.Pixbuf), typeof(string), typeof(iFolderHolder));
			remoteFoldersIconView = new iFolderIconView(remoteFoldersListStore);
			remoteFoldersIconView.SelectionMode = SelectionMode.Single;

			remoteFoldersIconView.ButtonPressEvent +=
				new ButtonPressEventHandler(OnRemoteFoldersButtonPressed);
			
			remoteFoldersIconView.SelectionChanged +=
				new EventHandler(OnRemoteFoldersSelectionChanged);

			sw.Add(remoteFoldersIconView);
			
			remoteFolderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("folder128.png"));
			remoteFolderPixbuf = remoteFolderPixbuf.ScaleSimple(64, 64, Gdk.InterpType.Bilinear);
			iFolderHolder[] ifolders = ifdata.GetiFolders();
			if(ifolders != null)
			{
				foreach(iFolderHolder holder in ifolders)
				{
					if (holder.iFolder.IsSubscription)
					{
						TreeIter iter;
						iter = remoteFoldersListStore.AppendValues(remoteFolderPixbuf, holder.iFolder.Name, holder);
						curRemoteFolders[holder.iFolder.CollectionID] = iter;
					}
				}
			}

			return sw;
		}
		
		private void OnRemoteFoldersButtonPressed(object o, ButtonPressEventArgs args)
		{
			TreePath tPath =
				remoteFoldersIconView.GetPathAtPos((int)args.Event.X,
												   (int)args.Event.Y);
			if (tPath != null)
			{
				TreeModel tModel = remoteFoldersIconView.Model;
				
				TreeIter iter;
				if (tModel.GetIter(out iter, tPath))
				{
					string folderName =
						(string)tModel.GetValue(iter, 1);
					if (folderName != null)
						Console.WriteLine("Folder clicked: {0}", folderName);
				}
			}
		}
		
		private void OnRemoteFoldersSelectionChanged(object o, EventArgs args)
		{
Console.WriteLine("iFolderWindow.OnRemoteFoldersSelectionChanged()");
			iFolderHolder holder = null;
			
			TreePath[] selection = remoteFoldersIconView.SelectedItems;
			if (selection.Length == 0)
			{
//				DownloadAndSyncButton.Sensitive = false;
//				DeleteFromServerButton.Sensitive = false;
				
				RemoteDetailsNotebook.CurrentPage = 0;
			}
			else
			{
				TreeModel tModel = remoteFoldersIconView.Model;
				for (int i = 0; i < selection.Length; i++)
				{
					TreeIter iter;
					if (tModel.GetIter(out iter, selection[i]))
					{
						holder =
							(iFolderHolder)tModel.GetValue(iter, 2);
						if (holder != null)
						{
							RemoteNameLabel.Markup =
								string.Format("<span size=\"large\" weight=\"bold\">{0}</span>", holder.iFolder.Name);
							RemoteSizeLabel.Markup =
								string.Format("{0} MB", "47");
							RemoteOwnerLabel.Markup =
								string.Format("Owner: {0}", holder.iFolder.Owner);
							DomainInformation domain = domainController.GetDomain(holder.iFolder.DomainID);
							if (domain != null)
								RemoteServerLabel.Markup =
									string.Format("Server: {0}", domain.Name);
							else
								RemoteServerLabel.Text = "";
							
							if (holder.iFolder.Description != null)
								RemoteDescriptionTextView.Buffer.Text = holder.iFolder.Description;
							else
								RemoteDescriptionTextView.Buffer.Text = "";
						}
					}
				}

				RemoteDetailsNotebook.CurrentPage = 1;
			}
			
			if (FolderSelected != null)
				FolderSelected(this, new FolderSelectedArgs(holder));
		}

		private void RemoveSynchronizedFolderHandler(object o,  EventArgs args)
		{
Console.WriteLine("RemoveSynchronizedFolderHandler");

		}

		private void OnRealizeWidget(object o, EventArgs args)
		{
			remotePaned.Position = 0;

			remoteSearchTimeoutID = GLib.Timeout.Add(
				100, new GLib.TimeoutHandler(GrabRemoteSearchEntryFocus));
		}

		private void RefreshFoldersHandler(object o, EventArgs args)
		{
			RefreshFolders(true);
		}


		public void RefreshFolders(bool readFromSimias)
		{
			curRemoteFolders.Clear();

			if(readFromSimias)
				ifdata.Refresh();

			iFolderHolder[] ifolders = ifdata.GetiFolders();
			if(ifolders != null)
			{
				foreach(iFolderHolder holder in ifolders)
				{
					// Only add in subscriptions
					if (!holder.iFolder.IsSubscription) continue;
					
					if (curDomain == null || curDomain.ID == "0"
						|| curDomain.ID == holder.iFolder.DomainID)
					{
						TreeIter iter = remoteFoldersListStore.AppendValues(remoteFolderPixbuf, holder.iFolder.Name, holder);
						curRemoteFolders[holder.iFolder.CollectionID] = iter;
					}
				}
			}
		}

		public void iFolderChanged(string iFolderID)
		{
			iFolderHolder ifHolder = ifdata.GetiFolder(iFolderID);
			TreeIter iter;

			if (curRemoteFolders.ContainsKey(iFolderID))
			{
				// FIXME: Determine if the state has changed to know if we should update the pixbuf
				iter = (TreeIter)curRemoteFolders[iFolderID];
				remoteFoldersListStore.SetValue(iter, 2, ifHolder);
			}
		}

		public void iFolderDeleted(string iFolderID)
		{
			TreeIter iter;
		
			if (curRemoteFolders.ContainsKey(iFolderID))
			{
				// FIXME: Determine if the state has changed to know if we should update the pixbuf
				iter = (TreeIter)curRemoteFolders[iFolderID];
				remoteFoldersListStore.Remove(ref iter);
			}
		}

		private void RemoteDomainComboBoxCellTextDataFunc(
				CellLayout cell_layout,
				CellRenderer cell,
				TreeModel tree_model,
				TreeIter iter)
		{
			// FIXME: Figure out how much space is available and truncate the server text if needed
			DomainInformation domain =
				(DomainInformation)tree_model.GetValue(iter, 0);
			if (domain != null)
				((CellRendererText) cell).Text = domain.Name;
		}

		public void iFolderCreated(string iFolderID)
		{
			iFolderHolder ifHolder = ifdata.GetiFolder(iFolderID);
			TreeIter iter;

			if (!curRemoteFolders.ContainsKey(iFolderID))
			{
				if (ifHolder.iFolder.IsSubscription)
				{
					iter = remoteFoldersListStore.AppendValues(remoteFolderPixbuf, ifHolder.iFolder.Name, ifHolder);
					curRemoteFolders[iFolderID] = iter;
				}
			}
		}

		public void RefreshDomains(bool readFromSimias)
		{
			if(readFromSimias)
				ifdata.RefreshDomains();
			
			DomainListStore.Clear();

			curDomains = ifdata.GetDomains();
			if (curDomains != null)
			{
				DomainInformation selectAllDomain = new DomainInformation();
				selectAllDomain.ID = "0";
				selectAllDomain.Name = Util.GS("Show All");
				DomainListStore.AppendValues(selectAllDomain);

				foreach(DomainInformation domain in curDomains)
				{
					DomainListStore.AppendValues(domain);
				}
			}
			
			remoteDomainsComboBox.Active = 0;		// Show All
		}
		
		private void OnDomainAddedEvent(object sender, DomainEventArgs args)
		{
			RefreshDomains(true);
			RefreshFolders(true);
		}
		
		private void OnDomainDeletedEvent(object sender, DomainEventArgs args)
		{
			RefreshDomains(true);
			RefreshFolders(true);
		}
	}
	
	public class FolderSelectedArgs : EventArgs
	{
		private iFolderHolder holder;
		
		public iFolderHolder Folder
		{
			get
			{
				return holder;
			}
		}
		
		public FolderSelectedArgs(iFolderHolder holder)
		{
			this.holder = holder;
		}
	}
}
