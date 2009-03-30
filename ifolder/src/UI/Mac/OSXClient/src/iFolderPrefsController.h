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
*                 $Modified by: Satyam <ssutapalli@novell.com>  20/3/2008   Added validation for sync dialog in preferences
*                 $Modified by: Satyam <ssutapalli@novell.com>  9/4/2008   Added Action outlet for show/hide toolbar
*-----------------------------------------------------------------------------
* This module is used to:
*       	iFolder Preferences dialog controller 
*
*******************************************************************************/

#import <Cocoa/Cocoa.h>

@class AccountsController;
@class iFolderService;

@interface iFolderPrefsController : NSWindowController
{
    IBOutlet NSView				*generalView;
    IBOutlet NSView				*accountsView;
    IBOutlet NSView				*notifyView;
    IBOutlet NSView				*syncView;
	IBOutlet NSView				*blankView;
	IBOutlet AccountsController	*accountsController;
	IBOutlet NSButton			*enableSync;
	IBOutlet NSPopUpButton		*syncUnits;
	IBOutlet NSTextField		*syncValue;
	IBOutlet NSTextField		*syncLabel;
	
	iFolderService				*ifolderService;	

	NSToolbar				*toolbar;
	NSMutableDictionary		*toolbarItemDict;	
	NSMutableArray			*toolbarItemArray;
	int						modalReturnCode;
}

- (IBAction)toggleSyncEnabled:(id)sender;
- (IBAction)updateSyncValue:(id)sender;
- (IBAction)changeSyncUnits:(id)sender;
- (IBAction)playSound:(id)sender;
- (IBAction)showHideToolbar:(id)sender;

+ (iFolderPrefsController *)sharedInstance;

-(void)showAccountsWindow;

- (void) updateSize:(NSSize)newSize;

// Toobar Delegates
- (NSToolbarItem *)toolbar:(NSToolbar *)toolbar
	itemForItemIdentifier:(NSString *)itemIdentifier
	willBeInsertedIntoToolbar:(BOOL)flag;
- (NSArray *)toolbarAllowedItemIdentifiers:(NSToolbar *)toolbar;
- (NSArray *)toolbarDefaultItemIdentifiers:(NSToolbar *)toolbar;
- (NSArray *)toolbarSelectableItemIdentifiers:(NSToolbar *)toolbar;


- (void)windowWillClose:(NSNotification *)aNotification;

// user actions
- (void)generalPreferences:(NSToolbarItem *)item;
- (void)accountPreferences:(NSToolbarItem *)item;
- (void)syncPreferences:(NSToolbarItem *)item;
- (void)notifyPreferences:(NSToolbarItem *)item;

- (void)setupToolbar;
-(void)saveCurrentSyncInterval;

@end
