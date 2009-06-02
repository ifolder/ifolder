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
*                 $Author: Calvin Gaisford <cgaisford@novell.com>
*                 $Modified by: Satyam <ssutapalli@novell.com> 29/02/2008    Added menu item "Merge" and its implementation
*                 $Modified by: Satyam <ssutapalli@novell.com> 22/05/2008    Bypassing Create iFolder directly to iFolderData
*                 $Modified by: Satyam <ssutapalli@novell.com> 13/10/2008    Vertical alignment of text field cell in table
*                 $Modified by: Satyam <ssutapalli@novell.com> 02/06/2009    Added menu item for "Forgot Passphrase"
*-----------------------------------------------------------------------------
* This module is used to:
*       	iFolder Main Window controller 
*
*******************************************************************************/

#import <Cocoa/Cocoa.h>
#import <iFolderService.h>
#import <SimiasService.h>


// Forward Declarations
@class CreateiFolderSheetController;
@class SetupiFolderSheetController;
@class PropertiesWindowController;
@class RevertiFolderController;
@class ExportKeysSheetController;
@class ImportKeysSheetController;
@class ResetPPKeySheetController;
@class ForgotPassphraseSheetController;
@class AdvConflictController;
@class DefaultiFolderSheetController;
@class iFolder;
@class iFolderTextFieldCell;
@class ChangePasswordSheetController;
@class ForgotPassphraseSheetController;


@interface iFolderWindowController : NSWindowController
{
    IBOutlet SetupiFolderSheetController	*setupSheetController;
    IBOutlet CreateiFolderSheetController	*createSheetController;
	IBOutlet RevertiFolderController        *revertiFolderController;
	IBOutlet ExportKeysSheetController      *exportKeysSheetController;
	IBOutlet ImportKeysSheetController      *importKeysSheetController;
	IBOutlet ResetPPKeySheetController      *resetPPKeySheetController;
	IBOutlet AdvConflictController          *advConflictSheetController;
	IBOutlet ChangePasswordSheetController  *changePasswordSheetController;
	IBOutlet ForgotPassphraseSheetController *forgotPPSheetController;
    IBOutlet NSTableView					*iFolderTable;
	IBOutlet NSTextField					*statusText;
	IBOutlet NSProgressIndicator			*statusProgress;
	IBOutlet NSTableColumn					*iconColumn;
	IBOutlet NSTableColumn					*nameColumn;
	IBOutlet NSTableColumn					*locationColumn;
	IBOutlet NSTableColumn					*statusColumn;

    NSArrayController						*ifoldersController;
	
	PropertiesWindowController				*propertiesController;
	
	NSToolbar								*toolbar;
	NSMutableDictionary						*toolbarItems;
	NSMutableArray							*toolbarItemKeys;
	iFolderTextFieldCell       *ifolderCell;
}

+ (iFolderWindowController *)sharedInstance;
- (void)windowWillClose:(NSNotification *)aNotification;

- (IBAction)customizeToolbar:(id)sender;
- (IBAction)deleteiFolder:(id)sender;
- (IBAction)mergeiFolder:(id)sender;
- (IBAction)doubleClickedTable:(id)sender;
- (IBAction)exportKeys:(id)sender;
- (IBAction)importKeys:(id)sender;
- (IBAction)newiFolder:(id)sender;
- (IBAction)openiFolder:(id)sender;
- (IBAction)refreshWindow:(id)sender;
- (IBAction)resetPassPhrase:(id)sender;
- (IBAction)revertiFolder:(id)sender;
- (IBAction)setupiFolder:(id)sender;
- (IBAction)showProperties:(id)sender;
- (IBAction)shareiFolder:(id)sender;
- (IBAction)synciFolder:(id)sender;
- (IBAction)resolveConflicts:(id)sender;
- (IBAction)changePassword:(id)sender;
- (IBAction)forgotPassphrase:(id)sender;

+(void)updateStatusTS:(NSString *)message;
-(void)updateStatus:(NSString *)message;

+(void)updateProgress:(double)curVal withMin:(double)minVal withMax:(double)maxVal;
-(void)updateProgress:(double)curVal withMin:(double)minVal withMax:(double)maxVal;

//- (IBAction)showHideToolbar:(id)sender;

- (void)deleteiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;

- (void)awakeFromNib;
- (void)dealloc;

- (void)addDomain:(iFolderDomain *)newDomain;

-(iFolder *)selectediFolder;

// menu validation
- (BOOL)validateUserInterfaceItem:(id)anItem;

//==========================================
// Toolbar Methods
//==========================================
- (void)setupToolbar;
- (NSToolbarItem *)toolbar:(NSToolbar *)toolbar
	itemForItemIdentifier:(NSString *)itemIdentifier
	willBeInsertedIntoToolbar:(BOOL)flag;
- (NSArray *)toolbarAllowedItemIdentifiers:(NSToolbar *)toolbar;
- (NSArray *)toolbarDefaultItemIdentifiers:(NSToolbar *)toolbar;


//==========================================
// Methods to create/delete/revert iFolders
//==========================================
- (void)acceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath canMerge:(BOOL)merge;
- (BOOL)handleiFolderError:(NSString *)error;

+(void)refreshDomainsTS;
-(void)refreshDomains:(id)args;

@end
 
