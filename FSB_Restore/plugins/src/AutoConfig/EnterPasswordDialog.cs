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
*                 $Author: Ashok Singh <siashok@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using Gtk;
using System;
using Novell.AutoAccountHelper;

namespace Novell.iFolder
{
    public class EnterPasswordDialog: Dialog
    {
        private Entry        PasswordEntry;
        private CheckButton    RememberPasswordCheckButton;
        private UserAccount userAccount;
        private Image iFolderBanner;
        private Image iFolderScaledBanner;
        private Gdk.Pixbuf ScaledPixbuf;
       
        /// <summary>
        /// Gets username
        /// </summary>
        public string UserNameText
        {
            get
            {
                return userAccount.UserName;
            }
        }
        
        /// <summary>
        /// Gets password
        /// </summary>
        public string PasswordText
        {
            get
            {
                return PasswordEntry.Text;
            }
        }

        /// <summary>
        /// Gets whether remember password checkbox is active
        /// </summary>
        public bool SavePassword
        {
            get
            {
                return RememberPasswordCheckButton.Active;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="acct"></param>
        public EnterPasswordDialog(UserAccount acct) : base()
         {
            userAccount = acct;
            SetupDialog();
        }
    
        /// <summary>
        /// Showing iFolder Account creation wizard
        /// </summary>
        private void SetupDialog()
        {
            this.Title = Util.GS("iFolder Account");

            this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
            this.HasSeparator = false;
            this.SetDefaultSize (450, 100);
            this.Modal = true;
            this.DestroyWithParent = true;
            
            HBox imagebox = new HBox();
            imagebox.Spacing = 0;
            iFolderBanner = new Image(
                    new Gdk.Pixbuf(Util.ImagesPath("ifolder-banner.png")));
            imagebox.PackStart(iFolderBanner, false, false, 0);

            ScaledPixbuf = 
                new Gdk.Pixbuf(Util.ImagesPath("ifolder-banner-scaler.png"));
            iFolderScaledBanner = new Image(ScaledPixbuf);
            iFolderScaledBanner.ExposeEvent += 
                    new ExposeEventHandler(OnBannerExposed);
            imagebox.PackStart(iFolderScaledBanner, true, true, 0);
            this.VBox.PackStart (imagebox, false, true, 0);
            

            Table table = new Table(5, 3, false);
            this.VBox.PackStart(table, false, false, 0);
            table.ColumnSpacing = 6;
            table.RowSpacing = 6;
            table.BorderWidth = 12;

            // Row 1
            Label l = new Label(Util.GS("Enter your iFolder password"));
            table.Attach(l, 0,3, 0,1,
                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
            l.LineWrap = true;
            l.Xalign = 0.0F;

            // Row 2
            table.Attach(new Label(""), 0,1, 1,2,
                AttachOptions.Fill, 0,12,0); // spacer
            l = new Label(Util.GS("iFolder Server:"));
            table.Attach(l, 1,2, 1,2,
                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
            l.LineWrap = true;
            l.Xalign = 0.0F;
            l = new Label(userAccount.Server);
            table.Attach(l, 2,3, 1,2,
                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
            l.LineWrap = true;
            l.Xalign = 0.0F;
            
            // Row 3 
            table.Attach(new Label(""), 0,1, 2,3,
                AttachOptions.Fill, 0,12,0); // spacer
            l = new Label(Util.GS("_User Name:"));
            table.Attach(l, 1,2, 2,3,
                AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
            l.LineWrap = true;
            l.Xalign = 0.0F;
            l = new Label(userAccount.UserName);
            table.Attach(l, 2,3, 2,3,
                AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
            l.LineWrap = true;
            l.Xalign = 0.0F;

            // Row 4
            l = new Label(Util.GS("_Password:"));
            table.Attach(l, 1,2, 3,4,
                AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
            l.Xalign = 0.0F;
            PasswordEntry = new Entry();
            table.Attach(PasswordEntry, 2,3, 3,4,
                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
            l.MnemonicWidget = PasswordEntry;
            PasswordEntry.Visibility = false;

            PasswordEntry.Changed += new EventHandler(OnFieldsChanged);
            PasswordEntry.ActivatesDefault = true;

            // Row 5
            RememberPasswordCheckButton = new CheckButton(Util.GS("_Remember my password"));
            table.Attach(RememberPasswordCheckButton, 2,3, 4,5,
                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
            
            RememberPasswordCheckButton.Active = userAccount.RememberPassword;
            
            this.VBox.ShowAll();
        

            this.AddButton(Stock.Cancel, ResponseType.Cancel);
            this.AddButton(Stock.Ok, ResponseType.Ok);
            this.SetResponseSensitive(ResponseType.Ok, false);
            this.DefaultResponse = ResponseType.Ok;
            PasswordEntry.GrabFocus();
        }    
        
        /// <summary>
        /// Event handler called onBannerExposed
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        private void OnBannerExposed(object o, ExposeEventArgs args)
        {
            if(args.Event.Count > 0)
                return;

            Gdk.Pixbuf spb = 
                ScaledPixbuf.ScaleSimple(iFolderScaledBanner.Allocation.Width,
                                        iFolderScaledBanner.Allocation.Height,
                                        Gdk.InterpType.Nearest);

            Gdk.GC gc = new Gdk.GC(iFolderScaledBanner.GdkWindow);

            spb.RenderToDrawable(iFolderScaledBanner.GdkWindow,
                                            gc,
                                            0, 0,
                                            args.Event.Area.X,
                                            args.Event.Area.Y,
                                            args.Event.Area.Width,
                                            args.Event.Area.Height,
                                            Gdk.RgbDither.Normal,
                                            0, 0);
        }
        
        /// <summary>
        /// Event handler to handle Fields changed event
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private void OnFieldsChanged(object obj, EventArgs args)
        {
            bool enableOK = false;

            if(PasswordEntry.Text.Length > 0)
                enableOK = true;

            this.SetResponseSensitive(ResponseType.Ok, enableOK);
        }
        
        /// <summary>
        /// Show the enter password dialog box
        /// </summary>
        /// <param name="acct"></param>
        /// <returns>true if password matches</returns>
        static public bool ShowEnterPasswordDialog(UserAccount acct )
        {
            int result;
            bool status = false;
            
            EnterPasswordDialog epd = new EnterPasswordDialog( acct );
            if (!Util.RegisterModalWindow(epd))
            {
                epd.Destroy();
                epd = null;
                return false;
            }
            do
            {
                result = epd.Run();
                if( result == (int)ResponseType.Cancel || result == (int) ResponseType.DeleteEvent)
                { 
                    iFolderMsgDialog dg = new iFolderMsgDialog(
                        epd, 
                        iFolderMsgDialog.DialogType.Warning,
                        iFolderMsgDialog.ButtonSet.YesNo,
                         Util.GS("No Password"),
                         Util.GS("Entering password is cancelled"),
                         Util.GS("If password is not provided, Automatic Account creation will fail. Do you want to continue?"));
                    int rc = dg.Run();
                    dg.Hide();
                    dg.Destroy();
                    if( (ResponseType)rc == ResponseType.Yes )
                        break;
                }
            }while( result == (int)ResponseType.Cancel || result == (int) ResponseType.DeleteEvent );
            
            if( result == (int)ResponseType.Ok )
            {
                acct.Password = epd.PasswordText;
                acct.RememberPassword = epd.SavePassword;
                status = true;
            }
            else
                status = false;
            epd.Hide();
               epd.Destroy();
            return status;
        }
    }
}
