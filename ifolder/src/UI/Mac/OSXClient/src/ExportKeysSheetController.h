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
*                 $Author: Satyam <ssutapalli@novell.com> 07/02/2007	Export Encrypted keys for PP recovery
*                 $Modified by: Satyam <ssutapalli@novell.com> 10/04/2008 Added validation for ifolder path
*                 $Modified by: Satyam <ssutapalli@novell.com> 09/09/2008 UI controls enable/disable according to domain's encryption/regular policy
*-----------------------------------------------------------------------------
* This module is used to:
*			This class is usefull for exporting the encrypted pass phrase keys in form of 
*           xml format. This xml file has to be sent to trusted party to get it decrypted.
*******************************************************************************/
/* ExportKeysSheetController */

#import <Cocoa/Cocoa.h>
@class iFolderWindowController;

@interface ExportKeysSheetController : NSWindowController
{
    IBOutlet NSTextField *domainID;
    IBOutlet NSTextField *emailID;
    IBOutlet NSButton *exportButton;
	IBOutlet NSButton *browseButton;
    IBOutlet id exportSheet;
    IBOutlet NSTextField *filePath;
    IBOutlet NSPopUpButton *ifolderAccount;
    IBOutlet iFolderWindowController *ifolderWindowController;
    IBOutlet id mainWindow;
    IBOutlet NSTextField *recoveryAgent;
	
	NSString *raNameForDomain;
}
- (IBAction)onAccountChange:(id)sender;
- (IBAction)onBrowse:(id)sender;
- (IBAction)onCancel:(id)sender;
- (IBAction)onExport:(id)sender;
- (IBAction)showWindow:(id)sender;
- (void)textDidChange:(NSNotification *)aNotification;

@end
