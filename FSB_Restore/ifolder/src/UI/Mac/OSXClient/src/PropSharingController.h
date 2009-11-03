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
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import <Cocoa/Cocoa.h>

@class iFolderService;
@class User;
@class iFolder;
@class MemberSearchResults;

@interface PropSharingController : NSObject
{
	iFolderService				*ifolderService;
	MemberSearchResults			*searchResults;
	NSMutableArray				*users;
	NSMutableArray				*foundUsers;
	NSMutableDictionary			*keyedUsers;
	iFolder						*curiFolder;
	BOOL						hasAdminRights;
	BOOL						isOwner;
	NSString					*searchAttribute;
	User                        *currentUser;

    IBOutlet NSArrayController	*usersController;
    IBOutlet NSTableView		*currentUsers;
    IBOutlet NSTableView		*searchedUsers;
	IBOutlet NSTableColumn		*searchedColumn;
    IBOutlet NSSearchField		*userSearch;
	IBOutlet NSPopUpButton		*defaultAccess;
	IBOutlet NSWindow			*propertiesWindow;
	IBOutlet NSTextField		*ownerName;
	IBOutlet NSMenu				*templateMenu;
	IBOutlet NSMenuItem			*fullItem;
	IBOutlet NSMenuItem			*firstItem;
	IBOutlet NSMenuItem			*lastItem;
	IBOutlet NSButton			*removeUsers;
	IBOutlet NSButton			*addUsers;
}

- (IBAction)addSelectedUsers:(id)sender;
- (IBAction)refreshWindow:(id)sender;
- (IBAction)removeSelectedUsers:(id)sender;
- (IBAction)searchUsers:(id)sender;
- (IBAction)grantFullControl:(id)sender;
- (IBAction)grantReadWrite:(id)sender;
- (IBAction)grantReadOnly:(id)sender;
- (IBAction)makeOwner:(id)sender;
- (IBAction)searchFullName:(id)sender;
- (IBAction)searchFirstName:(id)sender;
- (IBAction)searchLastName:(id)sender;

- (void)removeSelectedUsersResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;
- (void)awakeFromNib;
- (void)addUser:(User *)newUser;

- (void)addSelectedUsersResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;
- (void)addAllSelectedUsers;

- (BOOL)validateUserInterfaceItem:(id)anItem;
- (BOOL)selectionContainsOwnerorCurrent;

-(void)setSelectedUserRights:(NSString *)rights;

// Delegates for TableView
-(int)numberOfRowsInTableView:(NSTableView *)aTableView;
-(id)tableView:(NSTableView *)aTableView objectValueForTableColumn:(NSTableColumn *)aTableColumn row:(int)rowIndex;


@end
