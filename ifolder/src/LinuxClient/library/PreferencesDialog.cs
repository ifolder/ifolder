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


using System;
using System.IO;
using System.Collections;
using Gtk;
using Simias.Client.Event;

namespace Novell.iFolder
{

	/// <summary>
	/// This is the main iFolder Window.  This window implements all of the
	/// client code for iFolder.
	/// </summary>
	public class PreferencesDialog : Dialog
	{
		private iFolderWebService		ifws;

		private Gtk.Notebook			PrefNoteBook;
		private PrefsGeneralPage		generalPage;
		private PrefsAccountsPage		accountsPage;


		/// <summary>
		/// Default constructor for iFolderWindow
		/// </summary>
		public PreferencesDialog(iFolderWebService webService)
			: base()
		{
			if(webService == null)
				throw new ApplicationException("iFolderWebServices was null");

			ifws = webService;

			this.HasSeparator = false;
			this.Title = Util.GS("iFolder Preferences");

			InitializeWidgets();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.SetDefaultSize (300, 480);

			// Create an extra vbox to add the spacing
			VBox dialogBox = new VBox();
			this.VBox.PackStart(dialogBox);
			dialogBox.BorderWidth = 7;
			dialogBox.Spacing = 7;

			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));
			this.WindowPosition = Gtk.WindowPosition.Center;

			//-----------------------------
			// Setup the Notebook (tabs)
			//-----------------------------
			PrefNoteBook = new Notebook();

			generalPage = new PrefsGeneralPage(this, ifws);
			PrefNoteBook.AppendPage( generalPage,
										new Label(Util.GS("_General")));

			accountsPage = new PrefsAccountsPage(this, ifws);
			PrefNoteBook.AppendPage( accountsPage,
										new Label(Util.GS("_Accounts")));

			dialogBox.PackStart(PrefNoteBook, true, true, 0);

			this.AddButton(Stock.Close, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);

			this.Response += 
					new ResponseHandler(DialogResponseHandler);

		}




		private void DialogResponseHandler(object o, ResponseArgs args)
		{
			switch(args.ResponseId)
			{
				case Gtk.ResponseType.Help:
					Util.ShowHelp("front.html", this);
					break;
				default:
				{
					this.Hide();
					this.Destroy();
					break;
				}
			}
		}




		private void CloseEventHandler(object o, EventArgs args)
		{
			CloseWindow();
		}




		private void CloseWindow()
		{
			this.Hide();
			this.Destroy();
		}




		public void HelpEventHandler(object o, EventArgs args)
		{
			Util.ShowHelp("front.html", this);
		}


	}
}
