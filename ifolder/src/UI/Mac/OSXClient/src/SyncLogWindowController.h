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
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        	Synchronization log dialog 
*
*******************************************************************************/
 
#import <Cocoa/Cocoa.h>

@interface SyncLogWindowController : NSWindowController
{
	IBOutlet NSWindow				*window;
	IBOutlet NSTableColumn			*logColumn;

	
	NSToolbar						*toolbar;
	NSMutableDictionary				*toolbarItems;
	NSMutableArray					*toolbarItemKeys;
}

+ (SyncLogWindowController *)sharedInstance;

- (IBAction)clearLog:(id)sender;
- (IBAction)saveLog:(id)sender;
- (IBAction)customizeToolbar:(id)sender;
- (IBAction)showHideToolbar:(id)sender;


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
