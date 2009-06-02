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
*                 $Author: Satyam <ssutapalli@novell.com> 02/06/2009	Forgot passphrase controller
*-----------------------------------------------------------------------------
* This module is used to:
*			This is used to handle forgot pass phrase handlers
*******************************************************************************/

/* ForgotPassphraseSheetController */

#import <Cocoa/Cocoa.h>
@class iFolderWindowController;

@interface ForgotPassphraseSheetController : NSWindowController
{
	//LoginWindowController *loginWindowController;
    IBOutlet NSTextField *domainID;
    IBOutlet NSSecureTextField *enterNewPP;
    IBOutlet id forgotPPSheet;
    IBOutlet NSPopUpButton *ifolderAccount;
    IBOutlet iFolderWindowController *iFolderWindowController;
    IBOutlet id mainWindow;
    IBOutlet NSTextField *password;
    IBOutlet NSButton *rememberPP;
    IBOutlet NSButton *resetButton;
    IBOutlet NSSecureTextField *retypePP;
    IBOutlet NSTextField *userName;
	unsigned int statusCode;
}
- (IBAction)onAccountChange:(id)sender;
- (IBAction)onCancel:(id)sender;
- (IBAction)setNewPP:(id)sender;
- (IBAction)showWindow:(id)sender;
- (void)textDidChange:(NSNotification *)aNotification;

@end
