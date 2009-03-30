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
*                 $Author: Satyam <ssutapalli@novell.com> 06/02/2007	Import decrypted passphrase
*                 $Modified by: Satyam <ssutapalli@novell.com> 09/09/2008 UI controls enable/disable according to domain's encryption/regular policy
*-----------------------------------------------------------------------------
* This module is used to:
*			This class will be used to import the decrypted xml file received from trusted party and
*           then set the new pass phrase accordingly.
*******************************************************************************/
/* ImportKeysSheetController */

#import <Cocoa/Cocoa.h>
@class iFolderWindowController;

@interface ImportKeysSheetController : NSWindowController
{
    IBOutlet NSTextField *domainID;
    IBOutlet NSTextField *filePath;
    IBOutlet NSPopUpButton *ifolderAccount;
    IBOutlet iFolderWindowController *iFolderWindowController;
    IBOutlet NSButton *importButton;
	IBOutlet NSButton *browseButton;
    IBOutlet id importSheet;
    IBOutlet id mainWindow;
    IBOutlet NSSecureTextField *newPassphrase;
    IBOutlet NSSecureTextField *onetimePassphrase;
    IBOutlet NSSecureTextField *retypePassphrase;
}
- (IBAction)onAccountChange:(id)sender;
- (IBAction)onBrowse:(id)sender;
- (IBAction)onCancel:(id)sender;
- (IBAction)onImport:(id)sender;
- (IBAction)showWindow:(id)sender;
- (void)textDidChange:(NSNotification *)aNotification;

@end
