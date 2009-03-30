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
*                 $Author: Satyam <ssutapalli@novell.com> 06/02/2007	For resetting passphrase
*                 $Modified by: Satyam <ssutapalli@novell.com> 14/08/2008 Enable/Disable controls depending on iFolder domain's encryption policy
*-----------------------------------------------------------------------------
* This module is used to:
*			This class will be used to reset the passphrase. It will ask for old passphrase,
*           asks for new passphrase with confirmation and then validates and update the 
*           new passphrase.
*******************************************************************************/

/* ResetPPKeySheetController */

#import <Cocoa/Cocoa.h>
@class iFolderWindowController;

@interface ResetPPKeySheetController : NSWindowController
{
    IBOutlet NSTextField *domainID;
    IBOutlet NSTextField *enterPassphrase;
    IBOutlet NSPopUpButton *ifolderAccount;
    IBOutlet iFolderWindowController *iFolderWindowController;
    IBOutlet id mainWindow;
    IBOutlet NSTextField *newPassphrase;
    IBOutlet NSComboBox *recoveryAgent;
    IBOutlet NSButton *rememberPassphrase;
    IBOutlet NSButton *resetButton;
    IBOutlet id resetPPSheet;
    IBOutlet NSTextField *retypePassphrase;
}
- (IBAction)onAccountChange:(id)sender;
- (IBAction)onCancel:(id)sender;
- (IBAction)onReset:(id)sender;
- (IBAction)showWindow:(id)sender;
- (void) certSheetDidEnd:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;
- (void)textDidChange:(NSNotification *)aNotification;
@end
