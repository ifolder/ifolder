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

+ (iFolderPrefsController *)sharedInstance;

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

					
@end
