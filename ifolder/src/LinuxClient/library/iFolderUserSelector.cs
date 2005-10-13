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
 *		Calvin Gaisford <cgaisford@novell.com>
 *		Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/


using System;
using Gtk;
using System.Collections;

using Simias.Client;

namespace Novell.iFolder
{
	/// <summary>
	/// This is the properties dialog for an iFolder.
	/// </summary>
	public class iFolderUserSelector : Dialog
	{
		private SimiasWebService 	simws;
		private string				domainID;
		private Gdk.Pixbuf			UserPixBuf;

		private Gtk.TreeView		SelTreeView;
		private Gtk.ListStore		SelTreeStore;
		private BigList				memberList;
		private MemberListModel		memberListModel;
		
		private Gtk.OptionMenu		SearchAttribOptionMenu;

		private Gtk.Entry			SearchEntry;
		private Gtk.Button			UserAddButton;
		private Gtk.Button			UserDelButton;
		private uint				searchTimeoutID;
		private Hashtable			selectedUsers;
		
		public const int			NumOfMembersToReturnDefault = 25;
		
		public MemberInfo[] SelectedUsers
		{
			get
			{
				ArrayList list = new ArrayList();
				TreeIter iter;

				if(SelTreeStore.GetIterFirst(out iter))
				{
					do
					{
						MemberInfo member = (MemberInfo)
											SelTreeStore.GetValue(iter,0);
						list.Add(member);
					}
					while(SelTreeStore.IterNext(ref iter));
				}

				return (MemberInfo[]) (list.ToArray(typeof(MemberInfo)));
			}
		}



		/// <summary>
		/// Default constructor for iFolderPropertiesDialog
		/// </summary>
		public iFolderUserSelector(	Gtk.Window parent,
									SimiasWebService SimiasWS,
									string domainID)
			: base()
		{
			this.Title = Util.GS("Select Users");
			if (SimiasWS == null)
				throw new ApplicationException("SimiasWebService was null");
			this.simws = SimiasWS;
			this.domainID = domainID;
			this.HasSeparator = false;
//			this.BorderWidth = 10;
			this.Resizable = true;
			this.Modal = true;
			if(parent != null)
				this.TransientFor = parent;

			InitializeWidgets();

			searchTimeoutID = 0;
			selectedUsers = new Hashtable();
		}




		/// <summary>
		/// Default destructor
		/// </summary>
		~iFolderUserSelector()
		{
			// Close any searches we haven't already closed
			memberListModel.CloseSearch();
		}




		/// <summary>
		/// Set up the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.SetDefaultSize (500, 400);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolderuser.png"));

			VBox dialogBox = new VBox();
			dialogBox.Spacing = 10;
			dialogBox.BorderWidth = 10;
			this.VBox.PackStart(dialogBox, true, true, 0);


			//------------------------------
			// Find Entry
			//------------------------------
			Table findTable = new Table(2, 3, false);
			dialogBox.PackStart(findTable, false, false, 0);
			findTable.ColumnSpacing = 20;
			findTable.RowSpacing = 5;

			Label findLabel = new Label(Util.GS("Find:"));
			findLabel.Xalign = 0;
			findTable.Attach(findLabel, 0, 1, 0, 1,
				AttachOptions.Shrink, 0, 0, 0);

			SearchAttribOptionMenu = new OptionMenu();
			Menu m = new Menu();
			m.Append(new MenuItem(Util.GS("First Name")));
			m.Append(new MenuItem(Util.GS("Last Name")));
			m.Append(new MenuItem(Util.GS("Full Name")));
			SearchAttribOptionMenu.Menu = m;
			SearchAttribOptionMenu.ShowAll();
			SearchAttribOptionMenu.SetHistory(2);
			SearchAttribOptionMenu.Changed += new EventHandler(OnSearchAttribOptionMenuChanged);
			findTable.Attach(SearchAttribOptionMenu, 1, 2, 0, 1,
				AttachOptions.Shrink, 0, 0, 0);

			SearchEntry = new Gtk.Entry(Util.GS("<Enter text to find a user>"));
			SearchEntry.SelectRegion(0, -1);
			SearchEntry.CanFocus = true;
			SearchEntry.GrabFocus();
			SearchEntry.Changed += new EventHandler(OnSearchEntryChanged);
			findTable.Attach(SearchEntry, 2, 3, 0, 1,
				AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
				
			Label findHelpTextLabel = new Label(Util.GS("(Full or partial name)"));
			findHelpTextLabel.Xalign = 0;
			findTable.Attach(findHelpTextLabel, 2,3,1,2,
				AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
			


			//------------------------------
			// Selection Area
			//------------------------------
			HBox selBox = new HBox();
			selBox.Spacing = 10;
			dialogBox.PackStart(selBox, true, true, 0);


			//------------------------------
			// All Users tree
			//------------------------------
			memberListModel = new MemberListModel(domainID, simws);
			
			memberList = new BigList(memberListModel);
//			memberList.SetSizeRequest(100, 400);
			// FIXME: Fix up the BigList class to support both horizontal and vertical scrolling and then use the scroll adjustments to construct the ScrolledWindow
			ScrolledWindow sw = new ScrolledWindow(memberList.HAdjustment, memberList.VAdjustment);
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.Add(memberList);
			selBox.PackStart(sw, true, true, 0);
			memberList.ItemSelected += new ItemSelected(OnMemberIndexSelected);
			memberList.ItemActivated += new ItemActivated(OnMemberIndexActivated);


			//------------------------------
			// Buttons
			//------------------------------
			VBox btnBox = new VBox();
			btnBox.Spacing = 10;
			selBox.PackStart(btnBox, false, true, 0);

			UserAddButton = new Button(Util.GS("_Add >>"));
			btnBox.PackStart(UserAddButton, false, true, 0);
			UserAddButton.Clicked += new EventHandler(OnAddButtonClicked);

			UserDelButton = new Button(Util.GS("_Remove"));
			btnBox.PackStart(UserDelButton, false, true, 0);
			UserDelButton.Clicked += new EventHandler(OnRemoveButtonClicked);


			//------------------------------
			// Selected Users tree
			//------------------------------
			SelTreeView = new TreeView();
			ScrolledWindow ssw = new ScrolledWindow();
			ssw.ShadowType = Gtk.ShadowType.EtchedIn;
			ssw.Add(SelTreeView);
			selBox.PackStart(ssw, true, true, 0);

			// Set up the iFolder TreeView
			SelTreeStore = new ListStore(typeof(MemberInfo));
			SelTreeView.Model = SelTreeStore;

			// Set up Pixbuf and Text Rendering for "iFolder Users" column
			CellRendererPixbuf smcrp = new CellRendererPixbuf();
			TreeViewColumn selmemberColumn = new TreeViewColumn();
			selmemberColumn.PackStart(smcrp, false);
			selmemberColumn.SetCellDataFunc(smcrp, new TreeCellDataFunc(
						UserCellPixbufDataFunc));
			CellRendererText smcrt = new CellRendererText();
			selmemberColumn.PackStart(smcrt, false);
			selmemberColumn.SetCellDataFunc(smcrt, new TreeCellDataFunc(
						UserCellTextDataFunc));
			selmemberColumn.Title = Util.GS("Selected Users");
			selmemberColumn.Resizable = true;
			SelTreeView.AppendColumn(selmemberColumn);
			SelTreeView.Selection.Mode = SelectionMode.Multiple;

			SelTreeView.Selection.Changed += new EventHandler(
						OnSelUserSelectionChanged);

			UserPixBuf = 
				new Gdk.Pixbuf(Util.ImagesPath("ifolderuser.png"));

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);

			SearchiFolderUsers();
		}




		private void UserCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			MemberInfo member = (MemberInfo) tree_model.GetValue(iter,0);
			if( (member.FullName != null) && (member.FullName.Length > 0) )
			{
				if (memberListModel.IsDuplicateFullName(member.FullName))
					((CellRendererText) cell).Text = string.Format("{0} ({1})", member.FullName, member.Name);
				else
					((CellRendererText) cell).Text = member.FullName;
			}
			else
				((CellRendererText) cell).Text = member.Name;
		}




		private void UserCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
//			iFolderUser user = (iFolderUser) tree_model.GetValue(iter,0);
			((CellRendererPixbuf) cell).Pixbuf = UserPixBuf;
		}




		public void OnMemberIndexSelected(int index)
		{
			if (index >= 0)
			{
				UserAddButton.Sensitive = true;
			}
			else
			{
				UserAddButton.Sensitive = false;
			}
		}




		private void OnMemberIndexActivated(int index)
		{
			OnAddButtonClicked(null, null);
		}




		public void OnSelUserSelectionChanged(object o, EventArgs args)
		{
			TreeSelection tSelect = SelTreeView.Selection;

			if(tSelect.CountSelectedRows() > 0)
			{
				UserDelButton.Sensitive = true;
			}
			else
			{
				UserDelButton.Sensitive = false;
			}
		}




		public void OnSearchAttribOptionMenuChanged(object o, EventArgs args)
		{
			// Prevent a call to SearchCallback if a timeout call has been added
			if (searchTimeoutID != 0)
			{
				Gtk.Timeout.Remove(searchTimeoutID);
				searchTimeoutID = 0;
			}
				
//			// If there's existing text in the search entry, restart
//			// the search with the new search type.
//			if (SearchEntry.Text.Length > 0 && SearchEntry.Text != Util.GS("<Enter text to find a user>"))
//			{
				// No need to wait for the user to type anything else.
				// Perform the search right now.
				SearchiFolderUsers();
//			}
		}




		public void OnSearchEntryChanged(object o, EventArgs args)
		{
			if(searchTimeoutID != 0)
			{
				Gtk.Timeout.Remove(searchTimeoutID);
				searchTimeoutID = 0;
			}

			searchTimeoutID = Gtk.Timeout.Add(500, new Gtk.Function(
						SearchCallback));
		}




		private bool SearchCallback()
		{
			SearchiFolderUsers();
			return false;
		}




		private void SearchiFolderUsers()
		{
			if (this.GdkWindow != null)
			{
				this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
			}

			UserAddButton.Sensitive = false;
			UserDelButton.Sensitive = false;

			if(SearchEntry.Text.Length > 0 && SearchEntry.Text != Util.GS("<Enter text to find a user>"))
			{
				int searchAttribIndex = SearchAttribOptionMenu.History;
				string searchAttribute;
				switch(searchAttribIndex)
				{
					case 1:
						searchAttribute = "Family";
						break;
					case 2:
						searchAttribute = "FN";
						break;
					case 0:
					default:
						searchAttribute = "Given";
						break;
				}
				
				PerformInitialSearch(searchAttribute, SearchEntry.Text);
			}
			else
			{
				// Populate the UserTreeStore with the first 25 domain users
				PerformInitialSearch(null, null);
			}

			if (this.GdkWindow != null)
			{
				this.GdkWindow.Cursor = null; // reset to parent's cursor (default)
			}
		}



		/// <summary>
		/// Performs the specified search and populates the UserTreeStore
		/// <returns>true if any users were found by this search, false otherwise</returns>
		/// </summary>
		private void PerformInitialSearch(string searchAttribute, string searchString)
		{
			string searchContext;
			MemberInfo[] memberInfoA;
			int totalMembers;
			
			if (searchString == null)
			{
				simws.FindFirstMembers(
					domainID,
					NumOfMembersToReturnDefault,
					out searchContext,
					out memberInfoA,
					out totalMembers);
			}
			else
			{
				simws.FindFirstSpecificMembers(
					domainID,
					searchAttribute,
					SearchEntry.Text,
					SearchType.Begins,
					NumOfMembersToReturnDefault,
					out searchContext,
					out memberInfoA,
					out totalMembers);
			}
			
			memberListModel.Reinitialize(searchContext, memberInfoA, totalMembers);
			memberList.Reload();
			memberList.Refresh();

			// The code in this if statement fixes Bug 87444 (User Selector
			// dialog is not refreshed when performing a search).  By forcing
			// the first item to be selected, this bug no longer happens.
			if (totalMembers > 0)
			{
				memberList.Selected = 0;
			}
		}



		private void OnAddButtonClicked(object o, EventArgs args)
		{
			int selectedIndex = memberList.Selected;
			if (selectedIndex >= 0)
			{
				MemberInfo memberInfo = null;
				try
				{
					memberInfo = memberListModel.GetMemberInfo(selectedIndex);
				}
				catch(Exception e)
				{
					Console.WriteLine(e.Message);
				}
				if (memberInfo != null)
				{
					if (!selectedUsers.ContainsKey(memberInfo.UserID))
					{
						selectedUsers.Add(memberInfo.UserID, memberInfo);
						SelTreeStore.AppendValues(memberInfo);
					}
				}
			}
		}


		public void OnRemoveButtonClicked(object o, EventArgs args)
		{
			TreeModel tModel;
			Queue   iterQueue;

			iterQueue = new Queue();
			TreeSelection tSelect = SelTreeView.Selection;
			Array treePaths = tSelect.GetSelectedRows(out tModel);
			// remove compiler warning
			if(tModel != null)
				tModel = null;

			// We can't remove anything while getting the iters
			// because it will change the paths and we'll remove
			// the wrong stuff.
			foreach(TreePath tPath in treePaths)
			{
				TreeIter iter;

				if(SelTreeStore.GetIter(out iter, tPath))
				{
					iterQueue.Enqueue(iter);
				}
			}

			// Now that we have all of the TreeIters, loop and
			// remove them all
			while(iterQueue.Count > 0)
			{
				TreeIter iter = (TreeIter) iterQueue.Dequeue();
				MemberInfo member = 
						(MemberInfo) SelTreeStore.GetValue(iter,0);
				selectedUsers.Remove(member.UserID);
				SelTreeStore.Remove(ref iter);
			}
		}
	}
	
	internal class MemberListModel : IListModel
	{
		private string domainID;
		private string searchContext;
		private int total = 0;
		private Hashtable memberInfos;
		
		// The next two Hashtables are just for determining duplicates.
		// According to BenM, it's faster to use Hashtables for just checking
		// "is x in collection" than using an ArrayList, so that's why these
		// are Hashtables.
		private Hashtable memberFullNames; 
		private Hashtable duplicateMembers;
		private SimiasWebService simws;
		
		#region Constructors
		
		public MemberListModel(String DomainID, SimiasWebService SimiasWS)
		{
			domainID = DomainID;
			simws = SimiasWS;

			searchContext = null;
			total = 0;
			memberInfos = new Hashtable();
			memberFullNames = new Hashtable();
			duplicateMembers = new Hashtable();
		}
		
		#endregion
		
		#region Properties
		
		public string SearchContext
		{
			get
			{
				return searchContext;
			}
		}
		
		#endregion
		
		#region Public Methods
		
		public void Reinitialize(string SearchContext, MemberInfo[] MemberList, int Total)
		{
			CloseSearch();	// Close an existing search if present
			memberInfos.Clear();
			memberFullNames.Clear();
			duplicateMembers.Clear();

			searchContext = SearchContext;

			if (MemberList != null)
			{
				for (int i = 0; i < MemberList.Length; i++)
				{
					MemberInfo memberInfo = MemberList[i];

					if (memberFullNames.Contains(memberInfo.FullName))
					{
						// If the one we've stored has the same username (CN)
						// then it's not really a duplicate.
						string username = (string) memberFullNames[memberInfo.FullName];
						if (!username.Equals(memberInfo.Name))
						{
							// We've found a duplicate
							duplicateMembers[memberInfo.FullName] = 0;
						}
					}
					else
						memberFullNames[memberInfo.FullName] = memberInfo.Name;

					memberInfos[i] = memberInfo;
				}
			
				if (MemberList.Length >= Total)
				{
					// We have all the results and can close the search
					CloseSearch();
				}
			}
			
			total = Total;
		}
		
		public void CloseSearch()
		{
			if (searchContext != null)
			{
				try
				{
					simws.FindCloseMembers(domainID, searchContext);
					searchContext = null;
				}
				catch(Exception e)
				{
				}
			}
		}
		
		public MemberInfo GetMemberInfo(int index)
		{
			if (index < 0 || index >= total || (total == 0))
			{
				Console.WriteLine("MemberListModel.GetMemberInfo() called with index out of the range or when total == 0");

				// FIXME: Figure out the right exception to throw here
				throw new Exception("GetValue called when no items are present");
			}
			
			MemberInfo memberInfoReturn;
			
			if (memberInfos.Contains(index))
			{
				memberInfoReturn = (MemberInfo)memberInfos[index];
			}
			else
			{
				if (searchContext == null)
				{
					// Somehow the searchContext was closed out prematurely
					// FIXME: Figure out a better exception for when a searchContext is nulled out prematurely
					throw new Exception("searchContext was closed too soon");
				}
				
				// Here's where the good stuff comes.  If we don't have it in our
				// hash table (and we didn't fail with a bad index earlier, that means
				// we need to go search for more data from the Domain Provider, store
				// the results into memberInfos and return the MemberInfo being asked
				// for.
				try
				{
					MemberInfo[] newMemberList;
					simws.FindSeekMembers(domainID, ref searchContext,
										  index, iFolderUserSelector.NumOfMembersToReturnDefault,
										  out newMemberList);
					int currentIndex = index;
					foreach(MemberInfo memberInfo in newMemberList)
					{
						if (memberFullNames.Contains(memberInfo.FullName))
						{
							// If the one we've stored has the same username (CN)
							// then it's not really a duplicate.
							string username = (string) memberFullNames[memberInfo.FullName];
							if (!username.Equals(memberInfo.Name))
							{
								// We've found a duplicate
								duplicateMembers[memberInfo.FullName] = 0;
							}
						}
						else
							memberFullNames[memberInfo.FullName] = memberInfo.Name;

						memberInfos[currentIndex] = memberInfo;
						currentIndex++;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("Exception thrown calling simws.FindSeekMembers(): {0}", e.Message);
				}
				
				memberInfoReturn = (MemberInfo)memberInfos[index];
			}
			
			if (memberInfoReturn == null)
				throw new Exception("Could not find the specified member");
			
			return memberInfoReturn;
		}
		
		public bool IsDuplicateFullName(string FullName)
		{
			return duplicateMembers.Contains(FullName);
		}
		
		#endregion
		
		#region IListModel Interface Implementation
		
		public int Rows
		{
			get
			{
				return total;
			}
		}
		
		public string GetValue(int row)
		{
			MemberInfo memberInfo = null;
			try
			{
				memberInfo = GetMemberInfo(row);
			}
			catch(Exception e)
			{
				Console.WriteLine(string.Format("{0}: {1}", row, e.Message));
				return Util.GS("Unknown");
			}
			string fullName = memberInfo.FullName;
			
			if (fullName != null && fullName.Length > 0)
			{
				if (duplicateMembers.Contains(memberInfo.FullName))
					return string.Format("{0} ({1})", fullName, memberInfo.Name);
				else
					return string.Format("{0}", fullName);
			}

			return memberInfo.Name;
		}
		
		public string GetDescription(int row)
		{
			return GetValue(row);
		}
		
		#endregion
	}
}
