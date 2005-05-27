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

#import "PropSharingController.h"
#import "iFolderService.h"
#import "iFolderApplication.h"
#import "iFolderWindowController.h"
#import "iFolder.h"
#import "User.h"
#import "MemberSearchResults.h"

@implementation PropSharingController


-(void)awakeFromNib
{
	ifolderService = [[iFolderService alloc] init];

	keyedUsers = [[NSMutableDictionary alloc] init];
	
	searchResults = nil;

	[[userSearch cell] setSearchMenuTemplate:templateMenu];
	// valid search attributes: "Given" "Family" "FN"
	searchAttribute = @"FN";
	
	NSLog(@"Reading all iFolder Users");

	@try
	{
		int userCount;
		curiFolder = [[[iFolderWindowController sharedInstance] selectediFolder] retain];
		if(curiFolder != nil)
		{
			NSString *iFolderID = [curiFolder ID];
			NSArray *newUsers = [ifolderService GetiFolderUsers:iFolderID];
			if(newUsers != nil)
			{
				for(userCount = 0; userCount < [newUsers count]; userCount++)
				{
					User *newUser = [newUsers objectAtIndex:userCount];

					[self addUser:newUser];
					if([[curiFolder OwnerUserID] compare:[newUser UserID]] == 0)
						[ownerName setStringValue:[newUser FN]];
				}
			}
			isOwner = ([[curiFolder OwnerUserID] compare:[curiFolder CurrentUserID]] == 0);
			hasAdminRights = ([[curiFolder CurrentUserRights] compare:@"Admin"] == 0); 
		}
	}
	@catch (NSException *e)
	{
		NSLog(@"Reading iFolder Users failed with an exception");
	}
	[self searchUsers:self];
}


-(void)dealloc
{
	if(searchResults != nil)
	{
		[searchResults release];
		searchResults = nil;
	}

	[curiFolder release];
	[ifolderService release];
	[keyedUsers release];
	[super dealloc];
}



-(void) addUser:(User *)newUser
{
	if([keyedUsers objectForKey:[newUser UserID]] == nil)
	{
		[usersController addObject:newUser];
		[keyedUsers setObject:newUser forKey:[newUser UserID] ];
	}
}




- (IBAction)addSelectedUsers:(id)sender
{
	if(!hasAdminRights)
	{
		NSBeginAlertSheet(NSLocalizedString(@"Insufficient Access", nil), 
		NSLocalizedString(@"OK", nil), nil, nil, 
		propertiesWindow, nil, nil, nil, nil, 
		NSLocalizedString(@"You do not have access rights to add users to this iFolder.  Contact the owner of the iFolder if changes need to be made.", nil));
	}
	else
	{
		if([searchedUsers numberOfSelectedRows] > 20)
		{
			NSBeginAlertSheet(NSLocalizedString(@"A large number of users is selected", nil), 
				NSLocalizedString(@"Yes", nil), NSLocalizedString(@"Cancel", nil),
				nil, propertiesWindow, self, 
				@selector(addSelectedUsersResponse:returnCode:contextInfo:), 
				nil, nil, 
				[NSString stringWithFormat:NSLocalizedString(@"Do you want to share with all %d selected users?", nil), [searchedUsers numberOfSelectedRows]]);
		}
		else
		{
			[self addAllSelectedUsers];
		}
	}
}


- (void)addSelectedUsersResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	switch(returnCode)
	{
		case NSAlertDefaultReturn:
		{
			[self addAllSelectedUsers];
			break;
		}
	}
}


- (void)addAllSelectedUsers
{
	NSLog(@"Adding selected users");

	NSIndexSet *idxset = [searchedUsers selectedRowIndexes];

	NSString *rights;
	switch([defaultAccess indexOfSelectedItem])
	{
		default:
		case 0:
			rights = @"ReadWrite";
			break;
		case 1:
			rights = @"ReadOnly";
			break;
		case 2:
			rights = @"Admin";
			break;
	}

	int i;
	unsigned int curindex = [idxset firstIndex];
	
	for(i=0; i < [idxset count]; i++)
	{
		User *selUser = [searchResults objectAtIndex:curindex];

		// Don't try to add them if they are already in the list
		if([keyedUsers objectForKey:[selUser UserID]] == nil)
		{
			@try
			{
				User *newUser = [ifolderService
									AddAndInviteUser:[selUser UserID] 
									MemberName:[selUser Name]
									GivenName:[selUser FirstName]
									FamilyName:[selUser Surname]
									iFolderID:[curiFolder ID]
									PublicKey:nil
									Rights:rights];

//				User *newUser = [ifolderService
//								InviteUser:[selUser UserID] 
//								toiFolder:[curiFolder ID]
//								withRights:rights];
				
				[self addUser:newUser];
			}
			@catch (NSException *e)
			{
				NSLog(@"Exception in AddAndInviteUser: %@", [e name] );
			}
		}
		
		curindex = [idxset indexGreaterThanIndex:curindex];
	}
}




- (IBAction)refreshWindow:(id)sender
{
	NSLog(@"Refreshing Window");
}





- (IBAction)removeSelectedUsers:(id)sender
{
	if(!hasAdminRights)
	{
		NSBeginAlertSheet(NSLocalizedString(@"Insufficient Access", nil), 
		NSLocalizedString(@"OK", nil), nil, nil, 
		propertiesWindow, nil, nil, nil, nil, 
		NSLocalizedString(@"You do not have access rights to remove a user from this iFolder.  Contact the owner of the iFolder if changes need to be made.", nil));
	}
	else
	{
		NSArray *selectedUsers = [usersController selectedObjects];
		if([selectedUsers count] > 0)
		{
			NSBeginAlertSheet(NSLocalizedString(@"Remove selected users?", nil), 
				NSLocalizedString(@"Yes", nil), NSLocalizedString(@"Cancel", nil),
				nil, propertiesWindow, self, 
				@selector(removeSelectedUsersResponse:returnCode:contextInfo:), 
				nil, nil, 
				NSLocalizedString(@"Are you sure you want to remove the selected users?", nil));
		}
	}
}


- (void)removeSelectedUsersResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	switch(returnCode)
	{
		case NSAlertDefaultReturn:
		{
			NSLog(@"Removing Selected users");
			NSArray *selectedUsers = [usersController selectedObjects];

			int i;
			for(i=0; i < [selectedUsers count]; i++)
			{
				User *selUser = [selectedUsers objectAtIndex:i];
				
				if( ([[curiFolder OwnerUserID] compare:[selUser UserID]] != 0) &&
					([[curiFolder CurrentUserID] compare:[selUser UserID]] != 0) )
				{
					@try
					{
						[ifolderService	RemoveUser:[selUser UserID] 
												fromiFolder:[curiFolder ID]];

						[usersController removeObject:selUser];
						[keyedUsers removeObjectForKey:[selUser UserID]];
					}
					@catch (NSException *e)
					{
						NSLog(@"Adding iFolder User failed");
					}
				}
			}
			break;
		}
	}
}



- (IBAction)searchUsers:(id)sender
{
	if(curiFolder != nil)
	{
		NSLog(@"Searching for users");
		if([[userSearch stringValue] length] > 0)
		{
			if(searchResults != nil)
			{
				[searchResults release];
				searchResults = nil;
			}
			// valid search attributes: "Given" "Family" "FN"
			searchResults = [[MemberSearchResults alloc] init];
			[searchResults searchMembers:[curiFolder DomainID] onAttribute:searchAttribute usingValue:[userSearch stringValue]];
		}
		else
		{
			if(searchResults != nil)
			{
				[searchResults release];
				searchResults = nil;
			}
			searchResults = [[MemberSearchResults alloc] init];
			[searchResults getAllMembers:[curiFolder DomainID]];
		}

		if(searchResults != nil)
		{
			[[searchedColumn headerCell] 
				setStringValue:[NSString stringWithFormat:NSLocalizedString(@"User count: %d", nil),[searchResults count] ]];
		}

		[searchedUsers reloadData];
	}
}



- (IBAction)grantFullControl:(id)sender
{
	[self setSelectedUserRights:@"Admin"];
}


- (IBAction)grantReadWrite:(id)sender
{
	[self setSelectedUserRights:@"ReadWrite"];
}


- (IBAction)grantReadOnly:(id)sender
{
	[self setSelectedUserRights:@"ReadOnly"];
}


- (IBAction)makeOwner:(id)sender
{
	NSArray *selectedUsers = [usersController selectedObjects];

	if([selectedUsers count] == 1)
	{
		User *selUser = [selectedUsers objectAtIndex:0];

		@try
		{
			[ifolderService	ChanageOwner:[curiFolder ID]
									toUser:[selUser UserID] 
									oldOwnerRights:@"Admin"];
			[selUser setIsOwner:YES];
			[selUser setRights:@"Admin"];
			NSString *oldOwnerID = [curiFolder OwnerUserID];
			[curiFolder SetOwner:selUser];
			[ownerName setStringValue:[selUser FN]];			
			
			NSArray *allUsers = [usersController arrangedObjects];
			int counter = 0;
			for(counter=1; counter<[allUsers count]; counter++)
			{
				User *curUser = [allUsers objectAtIndex:counter];
				if([[curUser UserID] compare:oldOwnerID] == 0)
				{
					[curUser setIsOwner:NO];
					break;
				}
			}
		}
		@catch (NSException *e)
		{
			NSLog(@"Adding iFolder User failed");
		}
	}
}




-(void)setSelectedUserRights:(NSString *)rights
{
	NSArray *selectedUsers = [usersController selectedObjects];

	int i;
	for(i=0; i < [selectedUsers count]; i++)
	{
		User *selUser = [selectedUsers objectAtIndex:i];
	
		if( ([[curiFolder OwnerUserID] compare:[selUser UserID]] != 0) &&
			([[curiFolder CurrentUserID] compare:[selUser UserID]] != 0) )
		{
			@try
			{
				[ifolderService	SetUserRights:[curiFolder ID]
										forUser:[selUser UserID] 
										withRights:rights];
				[selUser setRights:rights];
			}
			@catch (NSException *e)
			{
				NSLog(@"Setting user Rights failed");
			}
		}
	}
}




- (BOOL)validateUserInterfaceItem:(id)anItem
{
	SEL action = [anItem action];

	if(action == @selector(grantFullControl:))
	{
		if([[usersController selectedObjects] count] == 0)
			return NO;
			
		if( (!hasAdminRights) || [self selectionContainsOwnerorCurrent])
			return NO;
		else
			return YES;
	}
	else if(action == @selector(grantReadWrite:))
	{
		if([[usersController selectedObjects] count] == 0)
			return NO;

		if( (!hasAdminRights) || [self selectionContainsOwnerorCurrent])
			return NO;
		else
			return YES;
	}
	else if(action == @selector(grantReadOnly:))
	{
		if([[usersController selectedObjects] count] == 0)
			return NO;

		if( (!hasAdminRights) || [self selectionContainsOwnerorCurrent])
			return NO;
		else
			return YES;
	}
	else if(action == @selector(makeOwner:))
	{
		if([[usersController selectedObjects] count] == 0)
			return NO;

		if( (!isOwner) || [self selectionContainsOwnerorCurrent])
			return NO;
		else
			return YES;
	}

	return YES;
}




- (BOOL)selectionContainsOwnerorCurrent
{
	NSArray *selectedUsers = [usersController selectedObjects];

	int i;
	for(i=0; i < [selectedUsers count]; i++)
	{
		User *selUser = [selectedUsers objectAtIndex:i];
				
		if( ([[curiFolder OwnerUserID] compare:[selUser UserID]] == 0) ||
			([[curiFolder CurrentUserID] compare:[selUser UserID]] == 0) )
			return YES;
	}
	return NO;
}





// Delegates for TableView
-(int)numberOfRowsInTableView:(NSTableView *)aTableView
{
	if(searchResults != nil)
		return [searchResults count];
	else
		return 0;
}




-(id)tableView:(NSTableView *)aTableView objectValueForTableColumn:(NSTableColumn *)aTableColumn row:(int)rowIndex
{
	NSString *value = nil;
	if(searchResults != nil)
	{
		value = [[searchResults objectAtIndex:rowIndex] FN];
	}
	
	return value;
}




- (IBAction)searchFullName:(id)sender
{
	NSLog(@"Dude, search da' full name");
	[fullItem setState:NSOnState];
	[firstItem setState:NSOffState];
	[lastItem setState:NSOffState];
	[[userSearch cell] setSearchMenuTemplate:templateMenu];
	searchAttribute = @"FN";
	[self searchUsers:self];
}




- (IBAction)searchFirstName:(id)sender
{
	NSLog(@"Dude, search da' first name");
	[fullItem setState:NSOffState];
	[firstItem setState:NSOnState];
	[lastItem setState:NSOffState];
	[[userSearch cell] setSearchMenuTemplate:templateMenu];
	searchAttribute = @"Given";
	[self searchUsers:self];
}




- (IBAction)searchLastName:(id)sender
{
	NSLog(@"Dude, search da' last name");
	[fullItem setState:NSOffState];
	[firstItem setState:NSOffState];
	[lastItem setState:NSOnState];
	[[userSearch cell] setSearchMenuTemplate:templateMenu];
	searchAttribute = @"Family";
	[self searchUsers:self];
}


@end
