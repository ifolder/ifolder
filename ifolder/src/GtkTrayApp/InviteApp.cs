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

using Gtk;
using Gdk;
using Gnome;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	/// <summary>
	/// This class represents the Invitation Wizard Application.
	/// An instance of the InvitationWizard is instantiated and run.
	/// </summary>
	public class InviteApp 
	{
		/// <summary>
		/// This is the main method.
		/// </summary>
		public static void Main (string[] args)
		{
			Gnome.Program program =
				new Program("invitation-wizard", "0.10.0", Modules.UI, args);

			InvitationWizard inviteWiz;

			// If at least one command line argument was given, pass it
			// into the InvitationWizard to use as the invitation file.
			if(args.Length > 0)
				inviteWiz = new InvitationWizard(args[0]);
			else
				inviteWiz = new InvitationWizard();

			inviteWiz.WizardClosed += new EventHandler(on_wizard_closed);
			inviteWiz.ShowAll();

			program.Run();
		}

		public static void on_wizard_closed(object o, EventArgs args) 
		{
			Application.Quit();
		}
	}
}
