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
@class SyncLogWindowController;
@class CreateiFolderSheetController;

@interface MainWindowController : NSWindowController
{
	LoginWindowController					*loginController;
	iFolderService							*webService;
	NSMutableArray							*domains;
	NSMutableArray							*ifolders;
    IBOutlet NSArrayController				*ifoldersController;
    IBOutlet NSArrayController				*domainsController;
	IBOutlet SyncLogWindowController		*syncLogController;
	IBOutlet CreateiFolderSheetController	*createSheetController;
	
	iFolder					*selectediFolder;	
	NSToolbar				*toolbar;
	NSMutableDictionary		*items;
}

- (IBAction)showLoginWindow:(id)sender;
- (IBAction)showSyncLog:(id)sender;
- (IBAction)refreshWindow:(id)sender;


- (void)login:(NSString *)username withPassword:(NSString *)password toServer:(NSString *)server;
- (void)createiFolder:(NSString *)path inDomain:(NSString *)domainID;
- (void)AcceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath;


- (void)awakeFromNib;
- (void)dealloc;

- (void)addDomain:(iFolderDomain *)newDomain;
- (void)addiFolder:(iFolder *)newiFolder;
- (void)addLog:(NSString *)entry;

- (void)initializeSimiasEvents;

// Toobar Delegates
- (NSToolbarItem *)toolbar:(NSToolbar *)toolbar
	itemForItemIdentifier:(NSString *)itemIdentifier
	willBeInsertedIntoToolbar:(BOOL)flag;
- (NSArray *)toolbarAllowedItemIdentifiers:(NSToolbar *)toolbar;
- (NSArray *)toolbarDefaultItemIdentifiers:(NSToolbar *)toolbar;
- (int)count;

- (void)toolbarWillAddItem:(NSNotification *)notification;
- (void)toolbarDidRemoveItem:(NSNotification *)notification;

// user actions
- (void)toolbaritemclicked:(NSToolbarItem *)item;

// menu actions
- (IBAction)showHideToolbar:(id)sender;
- (IBAction)customizeToolbar:(id)sender;

// menu validation
- (BOOL)validateUserInterfaceItem:(id)anItem;


- (void)setupToolbar;

@end

#endif	// __MAIN_WINDOW_CONTROLLER

