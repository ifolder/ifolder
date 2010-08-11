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
*                 $Author: Raghavendra Prasad <praghavendra@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace EnhancedMenuItems
{
     class IconMenuItem : MenuItem
     {
        string mtext ;
        System.Drawing.Image m_icon; //The Image to be displayed along with the menu text.
        
        /*constructor : Override the DrawItemEvent and MeasureItems to owner draw the Images and the menu text*/
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="menutext"></param>
        /// <param name="ico"></param>
         public IconMenuItem ( string menutext,Image ico ) : base ( menutext )
        {
            this.Text = menutext;
            this.OwnerDraw = true ;
            this.DrawItem += new DrawItemEventHandler ( drawItem ) ;
            this.MeasureItem += new MeasureItemEventHandler ( measureItem ) ;
            m_icon = ico ;
            mtext = menutext ;
         }


         /// <summary>
         /// Draws menu tiems based on the events
         /// </summary>
         /// <param name="sender"></param>
         /// <param name="e"></param>
        private void drawItem ( object sender,DrawItemEventArgs e )
        {
            MenuItem menuitem = (MenuItem)sender;
            SolidBrush menuBrush = null;
            if (menuitem.Enabled == false)
            {
                menuBrush = new SolidBrush(SystemColors.GrayText); //Disabled Menu Items Must have gray Text.               
            }
            else
            {
                if ((e.State & DrawItemState.Selected) != 0)
                {
                  menuBrush = new SolidBrush(SystemColors.HighlightText); //Selected menu Item Text is highlighted
                }
                else
                {
                  menuBrush = new SolidBrush(SystemColors.MenuText); 
                }
            }

            /*Background Highlight must be done always; Even if the menuitem is diabled.*/
            if ((e.State & DrawItemState.Selected) != 0)
            {
                 e.Graphics.FillRectangle(SystemBrushes.Highlight,e.Bounds); // The background is highlighted.
            }
            else
            {
                e.Graphics.FillRectangle(SystemBrushes.Menu,e.Bounds);
            }

            /*TODO: Verify the choice of font for menu item text*/
            Font menuFont = new Font("Courier", 8);
            StringFormat myFormat = new StringFormat();
            myFormat.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show; // Show the mnemonic underline under the hotkey.
                
            if (menuitem.Enabled == true)
            {
                /*Draw the colored image if enabled.*/
                e.Graphics.DrawImage(m_icon, e.Bounds.Left + 5, e.Bounds.Top + 5, 16, 16);
                e.Graphics.DrawString(menuitem.Text, new Font("Courier", 8), menuBrush, (float)(e.Bounds.Left + this.m_icon.Width + 5), (float)(e.Bounds.Top + 5),myFormat);                
                
            }
            else if(menuitem.Enabled==false)
            {
                /*Make the images transparent(grayscale) when disabled.*/
                ControlPaint.DrawImageDisabled(e.Graphics, m_icon,e.Bounds.Left+5,e.Bounds.Top+5, Color.Transparent);
                e.Graphics.DrawString(menuitem.Text, new Font("Courier", 8), menuBrush, (float)(e.Bounds.Left + this.m_icon.Width + 5), (float)(e.Bounds.Top + 5),myFormat);
            }
        }

         /// <summary>
         /// sets the item's height and width
         /// </summary>
         /// <param name="sender"></param>
         /// <param name="e"></param>
        private void measureItem ( object sender, MeasureItemEventArgs e )
        {
            /*TODO: Verify if this size is acceptable.*/
            e.ItemHeight = 20 ;
            e.ItemWidth = 150 ;
        }
    }
    public class IconMenuItems
    {
        private IconMenuItem menuViewRefresh;
        private IconMenuItem menuActionOpen;
        private IconMenuItem menuActionCreate;
        private IconMenuItem menuActionRevert;
        private IconMenuItem menuActionShare;
        private IconMenuItem menuActionSync;
        private IconMenuItem menuActionProperties;
        private IconMenuItem menuActionResolve;
        private IconMenuItem menuActionAccept;
        private IconMenuItem menuActionMerge;
        private IconMenuItem menuHelpHelp;
        private IconMenuItem menuHelpUpgrade;
        private IconMenuItem menuHelpAbout;
        private IconMenuItem menuActionRemove;
        private MenuItem menuItem1;
        private IconMenuItem menuViewAccounts;
        private IconMenuItem menuViewLog;
        private IconMenuItem menuEditPrefs;
        private IconMenuItem menuResetPassphrase;
        private IconMenuItem menuResetPassword;
        private IconMenuItem menuRecoverKeys;
        private MenuItem menuItem7;

        /// <summary>
        /// It created the menu items
        /// </summary>
        /// <param name="iFolderMenuItem"></param>
        /// <returns></returns>
        public bool CreateMenu(System.Windows.Forms.MenuItem iFolderMenuItem)
        {
            try
            {
                //Menu Action menu items are enhanced to Icon Menu Items       
                if (iFolderMenuItem.Name.CompareTo("menuAction") == 0)
                {
                    MenuItem tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuActionCreate", true)[0];
                    Image menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-upload16.png"));
                    menuActionCreate = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuActionCreate.Enabled = tempMenuItem.Enabled;
                    this.menuActionCreate.Index = 0;
                    this.menuActionCreate.Shortcut = tempMenuItem.Shortcut;
                    this.menuActionCreate.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuActionCreate.Text = tempMenuItem.Text;
                    this.menuActionCreate.Visible = tempMenuItem.Visible;
                    this.menuActionCreate.Name = "iMenuActionCreate"; //Associate a new name.
                    iFolderMenuItem.MenuItems.Add(menuActionCreate);
                    
                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuActionAccept", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath,@"res\ifolder-download16.png"));
                    menuActionAccept = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuActionAccept.Enabled = tempMenuItem.Enabled;
                    this.menuActionAccept.Index = 1;
                    this.menuActionAccept.Shortcut = tempMenuItem.Shortcut;
                    this.menuActionAccept.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuActionAccept.Text = tempMenuItem.Text;
                    this.menuActionAccept.Visible = tempMenuItem.Visible;
                    this.menuActionAccept.Name = "iMenuActionAccept";
                    iFolderMenuItem.MenuItems.Add(menuActionAccept);
                    
                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuActionMerge", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-download16.png"));
                    menuActionMerge = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuActionMerge.Enabled = tempMenuItem.Enabled;
                    this.menuActionMerge.Index = 2;
                    this.menuActionMerge.Shortcut = tempMenuItem.Shortcut;
                    this.menuActionMerge.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuActionMerge.Text = tempMenuItem.Text;
                    this.menuActionMerge.Visible = tempMenuItem.Visible;
                    this.menuActionMerge.Name = "iMenuActionMerge";
                    iFolderMenuItem.MenuItems.Add(menuActionMerge);
                    
                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuActionRemove", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder_dis2_16.png"));
                    menuActionRemove = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuActionRemove.Enabled = tempMenuItem.Enabled;
                    this.menuActionRemove.Index = 3;
                    this.menuActionRemove.Shortcut = tempMenuItem.Shortcut;
                    this.menuActionRemove.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuActionRemove.Text = tempMenuItem.Text;
                    this.menuActionRemove.Visible = tempMenuItem.Visible;
                    this.menuActionRemove.Name = "iMenuActionRemove";
                    iFolderMenuItem.MenuItems.Add(menuActionRemove);
                    
                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuItem7", true)[0];
                    menuItem7 = tempMenuItem;
                    this.menuItem7.Enabled = tempMenuItem.Enabled;
                    this.menuItem7.Index = 4;
                    this.menuItem7.Shortcut = tempMenuItem.Shortcut;
                    this.menuItem7.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuItem7.Text = tempMenuItem.Text;
                    this.menuItem7.Visible = true;
                    this.menuItem7.Name = "iMenuItem7";
                    iFolderMenuItem.MenuItems.Add(menuItem7);

                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuActionOpen", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\documentOpen16.png"));
                    menuActionOpen = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuActionOpen.Enabled = tempMenuItem.Enabled;
                    this.menuActionOpen.Index = 5;
                    this.menuActionOpen.Shortcut = tempMenuItem.Shortcut;
                    this.menuActionOpen.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuActionOpen.Text = tempMenuItem.Text;
                    this.menuActionOpen.Visible = tempMenuItem.Visible;
                    this.menuActionOpen.Name = "iMenuActionOpen";
                    iFolderMenuItem.MenuItems.Add(menuActionOpen);
                    
                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuActionShare", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\shareWith16.png"));
                    menuActionShare = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuActionShare.Enabled = tempMenuItem.Enabled;
                    this.menuActionShare.Index = 6;
                    this.menuActionShare.Shortcut = tempMenuItem.Shortcut;
                    this.menuActionShare.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuActionShare.Text = tempMenuItem.Text;
                    this.menuActionShare.Visible = tempMenuItem.Visible;
                    this.menuActionShare.Name = "iMenuActionShare";
                    iFolderMenuItem.MenuItems.Add(menuActionShare);
                    

                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuActionResolve", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\resolveConflict16.png"));
                    menuActionResolve = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuActionResolve.Enabled = tempMenuItem.Enabled;
                    this.menuActionResolve.Index = 7;
                    this.menuActionResolve.Shortcut = tempMenuItem.Shortcut;
                    this.menuActionResolve.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuActionResolve.Text = tempMenuItem.Text;
                    this.menuActionResolve.Visible = tempMenuItem.Visible;
                    this.menuActionResolve.Name = "iMenuActionResolve";
                    iFolderMenuItem.MenuItems.Add(menuActionResolve);
                    
                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuActionRevert", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\revertToFolder.png"));
                    menuActionRevert = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuActionRevert.Enabled = tempMenuItem.Enabled;
                    this.menuActionRevert.Index = 7;
                    this.menuActionRevert.Shortcut = tempMenuItem.Shortcut;
                    this.menuActionRevert.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuActionRevert.Text = tempMenuItem.Text;
                    this.menuActionRevert.Visible = tempMenuItem.Visible;
                    this.menuActionRevert.Name = "iMenuActionRevert";
                    iFolderMenuItem.MenuItems.Add(menuActionRevert);
                    
                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuActionProperties", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\documentProperties16.png"));
                    menuActionProperties = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuActionProperties.Enabled = tempMenuItem.Enabled;
                    this.menuActionProperties.Index = 7;
                    this.menuActionProperties.Shortcut = tempMenuItem.Shortcut;
                    this.menuActionProperties.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuActionProperties.Text = tempMenuItem.Text;
                    this.menuActionProperties.Visible = tempMenuItem.Visible;
                    this.menuActionProperties.Name = "iMenuActionProperties";
                    iFolderMenuItem.MenuItems.Add(menuActionProperties);
                    
                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuActionSync", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-sync16.png"));
                    menuActionSync = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuActionSync.Enabled = tempMenuItem.Enabled;
                    this.menuActionSync.Index = 8;
                    this.menuActionSync.Shortcut = tempMenuItem.Shortcut;
                    this.menuActionSync.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuActionSync.Text = tempMenuItem.Text;
                    this.menuActionSync.Visible = tempMenuItem.Visible;
                    this.menuActionSync.Name = "iMenuActionSync";
                    iFolderMenuItem.MenuItems.Add(menuActionSync);
                   
                    return true; // if reached here then return success
                }
                else if (iFolderMenuItem.Name.CompareTo("menuHelp") == 0)
                {
                    MenuItem tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuHelpHelp", true)[0];
                    Image menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\help16.png"));
                    menuHelpHelp = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuHelpHelp.Enabled = tempMenuItem.Enabled;
                    this.menuHelpHelp.Text = tempMenuItem.Text;
                    this.menuHelpHelp.Visible = tempMenuItem.Visible;
                    //this.menuHelpHelp.Index = tempMenuItem.Index;
                    this.menuHelpHelp.Index = 0;
                    this.menuHelpHelp.Shortcut = tempMenuItem.Shortcut;
                    this.menuHelpHelp.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuHelpHelp.Name = "iMenuHelpHelp";
                    iFolderMenuItem.MenuItems.Add(menuHelpHelp);

                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuHelpUpgrade", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\about16.png"));
                    menuHelpUpgrade = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuHelpUpgrade.Enabled = tempMenuItem.Enabled;
                    this.menuHelpUpgrade.Text = tempMenuItem.Text;
                    this.menuHelpUpgrade.Visible = tempMenuItem.Visible;
                    this.menuHelpUpgrade.Index = 1;
                    this.menuHelpUpgrade.Shortcut = tempMenuItem.Shortcut;
                    this.menuHelpUpgrade.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuHelpUpgrade.Name = "iMenuHelpUpgrade";
                    iFolderMenuItem.MenuItems.Add(menuHelpUpgrade);

                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuHelpAbout", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\about16.png"));
                    menuHelpAbout = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuHelpAbout.Enabled = tempMenuItem.Enabled;
                    this.menuHelpAbout.Text = tempMenuItem.Text;
                    this.menuHelpAbout.Visible = tempMenuItem.Visible;
                    this.menuHelpAbout.Index = 2;
                    this.menuHelpAbout.Shortcut = tempMenuItem.Shortcut;
                    this.menuHelpAbout.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuHelpAbout.Name = "iMenuHelpAbout";
                    iFolderMenuItem.MenuItems.Add(menuHelpAbout);

                    return true;
                }
                else if (iFolderMenuItem.Name.Equals("menuEdit"))
                {
                    MenuItem tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuViewAccounts", true)[0];
                    Image menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\accountSettings16.png"));
                    menuViewAccounts = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuViewAccounts.Enabled = tempMenuItem.Enabled;
                    this.menuViewAccounts.Text = tempMenuItem.Text;
                    this.menuViewAccounts.Visible = tempMenuItem.Visible;
                    this.menuViewAccounts.Index = tempMenuItem.Index;
                    this.menuViewAccounts.Shortcut = tempMenuItem.Shortcut;
                    this.menuViewAccounts.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuViewAccounts.Name = "iMenuViewAccounts";
                    iFolderMenuItem.MenuItems.Add(menuViewAccounts);

                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuEditPrefs", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\preferences16.png"));
                    menuEditPrefs = new IconMenuItem(tempMenuItem.Text,menuImage);
                    this.menuEditPrefs.Enabled  =tempMenuItem.Enabled;
                    this.menuEditPrefs.Text  =tempMenuItem.Text.ToString();
                    this.menuEditPrefs.Visible = tempMenuItem.Visible;
                    this.menuEditPrefs.Index = tempMenuItem.Index;
                    this.menuEditPrefs.Shortcut = tempMenuItem.Shortcut;
                    this.menuEditPrefs.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuEditPrefs.Name = "iMenuEditPrefs";
                    iFolderMenuItem.MenuItems.Add(menuEditPrefs);
                    
                    return true;
                                    
                }
                else if (iFolderMenuItem.Name.Equals("menuView"))
                {
                    MenuItem tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuViewRefresh", true)[0];
                    Image menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\refresh16.png"));
                    menuViewRefresh = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuViewRefresh.Enabled = tempMenuItem.Enabled;
                    this.menuViewRefresh.Text = tempMenuItem.Text;
                    this.menuViewRefresh.Shortcut = tempMenuItem.Shortcut;
                    this.menuViewRefresh.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuViewRefresh.Name = "iMenuViewRefresh";
                    iFolderMenuItem.MenuItems.Add(menuViewRefresh);

                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuItem1", true)[0];
                    this.menuItem1 = tempMenuItem;
                    iFolderMenuItem.MenuItems.Add(menuItem1);

                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuViewLog", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\sync_log2.png"));
                    menuViewLog = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuViewLog.Enabled = tempMenuItem.Enabled;
                    this.menuViewLog.Text = tempMenuItem.Text;
                    this.menuViewLog.Shortcut = tempMenuItem.Shortcut;
                    this.menuViewLog.ShowShortcut = tempMenuItem.ShowShortcut;
                    this.menuViewLog.Name = "iMenuViewLog";
                    iFolderMenuItem.MenuItems.Add(menuViewLog);

                    return true;
                    
                }
                else if (iFolderMenuItem.Name.Equals("menuSecurity"))
                {
                    MenuItem tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuRecoverKeys", true)[0];
                    Image menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\keyRecovery16.png"));
                    menuRecoverKeys = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuRecoverKeys.Index = tempMenuItem.Index;
                    this.menuRecoverKeys.Enabled = tempMenuItem.Enabled;
                    this.menuRecoverKeys.Text = tempMenuItem.Text;
                    this.menuRecoverKeys.Visible = tempMenuItem.Visible;
                    this.menuRecoverKeys.Name = "iMenuRecoverKeys";
                    iFolderMenuItem.MenuItems.Add(menuRecoverKeys);

                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuResetPassphrase", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\resetPassphrase16.png"));
                    menuResetPassphrase = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuResetPassphrase.Visible = tempMenuItem.Enabled;
                    this.menuResetPassphrase.Index = tempMenuItem.Index;
                    this.menuResetPassphrase.Text = tempMenuItem.Text;
                    this.menuResetPassphrase.Visible = tempMenuItem.Visible;
                    this.menuResetPassphrase.Name = "iMenuResetPassphrase";
                    iFolderMenuItem.MenuItems.Add(menuResetPassphrase);

                    tempMenuItem = iFolderMenuItem.MenuItems.Find("MenuResetPassword", true)[0];
                    menuImage = Image.FromFile(Path.Combine(Application.StartupPath, @"res\resetPassphrase16.png"));
                    menuResetPassword = new IconMenuItem(tempMenuItem.Text, menuImage);
                    this.menuResetPassword.Visible = tempMenuItem.Enabled;
                    this.menuResetPassword.Index = tempMenuItem.Index;
                    this.menuResetPassword.Text = tempMenuItem.Text;
                    this.menuResetPassword.Visible = tempMenuItem.Visible;
                    this.menuResetPassword.Name = "iMenuResetPassword";
                    iFolderMenuItem.MenuItems.Add(this.menuResetPassword);

                    return true;
                }
            }
            catch
            { 
                //Ignore
                return false;
            }

            return false;
        }
    }
}
