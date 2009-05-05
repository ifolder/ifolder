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
  *                 $Author: Ravi Kumar <rkumar@novell.com>
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

namespace Novell.iFolder
{
        /// <summary>
        /// class iFolderNetworkDialog
        /// </summary>
        public class iFolderNetworkDialog : Dialog
        {
                private CheckButton cbutton;

                /// <summary>
                /// Get if Dialog should be hidden or not
                /// </summary>
                public bool HideDialog
                {
                        get
                        {
                                return cbutton.Active;
                        }
                }

                /// <summary>
                /// Constructor
                /// </summary>
                public iFolderNetworkDialog() : base()
                {
                        this.Title = "";
                        this.HasSeparator = false;
                        this.Resizable = false;

                        this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
			 Gdk.Pixbuf bigiFolder =
                                new Gdk.Pixbuf(Util.ImagesPath("ifolder48.png"));
                        Image folderImage = new Image(bigiFolder);

                        VBox vbox = new VBox();
                        vbox.BorderWidth = 10;
                        vbox.Spacing = 10;

                        HBox h = new HBox();
//                      h.BorderWidth = 10;
                        h.Spacing = 12;

                        folderImage.SetAlignment(0.5F, 0);
                        h.PackStart(folderImage, false, false, 0);

                        VBox vbox2 = new VBox();
                        vbox2.Spacing = 10;

                        Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
                                                                Util.GS("Network Events") +
                                                                "</span>");
                        l.LineWrap = false;
                        l.UseMarkup = true;
                        l.Selectable = false;
                        l.Xalign = 0;
                        l.Yalign = 0;
                        vbox2.PackStart(l, false, false, 0);

                        l = new Label(Util.GS("Novell iFolder does not currently support this machine's Network card to detect Network events. DBus failed to initialize for the Network card."));
                        l.LineWrap = true;
                        l.Xalign = 0;
                        vbox2.PackStart(l, true, true, 0);

                        h.PackEnd(vbox2, true, true, 0);

                        vbox.PackStart(h);

                        Alignment cbAlignment = new Alignment(1, 1, 1, 0);
                        vbox.PackStart(cbAlignment, true, true, 0);

                        cbutton =
                                new CheckButton(Util.GS("Do not show this message again."));
                        cbAlignment.Add(cbutton);

                        vbox.ShowAll();
                        this.VBox.Add(vbox);

                        this.AddButton(Stock.Close, ResponseType.Ok);
                }
        }
}
