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
#import <iFolderService.h>

#ifndef __MAIN_WINDOW_CONTROLLER
#define __MAIN_WINDOW_CONTROLLER


// Forward Declarations
@class LoginWindowController;
@class iFolderPrefsController;
@class SyncLogWindowController;
@class CreateiFolderSheetController;
@class SetupiFolderSheetController;
@class PropertiesWindowController;

@interface MainWindowController : NSWindowController
{
	LoginWindowController					*loginController;
	iFolderPrefsController					*prefsController;
	PropertiesWindowController				*propertiesController;
	iFolderService							*webService;
	NSMutableArray							*domains;
	NSMutableArray							*ifolders;
    IBOutlet NSArrayController				*ifoldersController;
    IBOutlet NSArrayController				*domainsController;
	IBOutlet SyncLogWindowController		*syncLogController;
	IBOutlet CreateiFolderSheetController	*createSheetController;
	IBOutlet SetupiFolderSheetController	*setupiFolderController;
	IBOutlet NSTableView					*iFolderTable;


	NSToolbar				*toolbar;
	NSMutableDictionary		*toolbarItems;
	NSMutableArray			*toolbarItemKeys;
	NSMutableDictionary		*keyedDomains;
	NSMutableDictionary		*keyediFolders;
}


- (IBAction)showSyncLog:(id)sender;
- (IBAction)refreshWindow:(id)sender;
- (IBAction)showHideToolbar:(id)sender;
- (IBAction)customizeToolbar:(id)sender;
- (IBAction)showPrefs:(id)sender;
- (IBAction)newiFolder:(id)sender;
- (IBAction)setupiFolder:(id)sender;
- (IBAction)revertiFolder:(id)sender;
- (void)revertiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;
- (IBAction)deleteiFolder:(id)sender;
- (void)deleteiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;
- (IBAction)openiFolder:(id)sender;
- (IBAction)showProperties:(id)sender;
- (IBAction)shareiFolder:(id)sender;

- (void)doubleClickedTable:(id)sender;

- (BOOL)connectToDomain:(iFolderDomain *)domain;
- (BOOL)authenticateToDomain:(NSString *)domainID withPassword:(NSString *)password;
- (void)createiFolder:(NSString *)path inDomain:(NSString *)domainID;
- (void)acceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath;

- (void)showLoginWindow:(NSString *)domainID;

- (void)awakeFromNib;
- (void)dealloc;

- (void)addDomain:(iFolderDomain *)newDomain;
- (void)addiFolder:(iFolder *)newiFolder;
- (void)addLog:(NSString *)entry;

- (void)initializeSimiasEvents;

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


@end

#endif	// __MAIN_WINDOW_CONTROLLER

