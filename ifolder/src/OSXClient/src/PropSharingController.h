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
