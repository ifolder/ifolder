#import "PropSharingController.h"
#import "iFolderService.h"
#import "MainWindowController.h"
#import "iFolder.h"
#import "User.h"

@implementation PropSharingController


-(void)awakeFromNib
{
	ifolderService = [[iFolderService alloc] init];

	keyedUsers = [[NSMutableDictionary alloc] init];

	NSLog(@"Reading all iFolder Users");

	@try
	{
		int userCount;
		curiFolder = [[[NSApp delegate] selectediFolder] retain];
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
				}
			}
		}
	}
	@catch (NSException *e)
	{
		[[NSApp delegate] addLog:@"Reading iFolder Users failed with an exception"];
	}
	[self searchUsers:self];
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
	NSLog(@"Adding selected users");
	NSArray *selectedUsers = [foundUsersController selectedObjects];

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
	for(i=0; i < [selectedUsers count]; i++)
	{
		User *selUser = [selectedUsers objectAtIndex:i];

		// Don't try to add them if they are already in the list
		if([keyedUsers objectForKey:[selUser UserID]] == nil)
		{
			@try
			{
				User *newUser = [ifolderService
								InviteUser:[selUser UserID] 
								toiFolder:[curiFolder ID]
								withRights:rights];
				
				[self addUser:newUser];
			}
			@catch (NSException *e)
			{
				[[NSApp delegate] addLog:@"Adding iFolder User failed"];
			}
		}
	}
}




- (IBAction)refreshWindow:(id)sender
{
	NSLog(@"Refreshing Window");
}





- (IBAction)removeSelectedUsers:(id)sender
{
	NSArray *selectedUsers = [usersController selectedObjects];
	if([selectedUsers count] > 0)
	{
		NSBeginAlertSheet(@"Remove selected users?", @"Yes", @"Cancel", nil,
			propertiesWindow, self, 
			@selector(removeSelectedUsersResponse:returnCode:contextInfo:), 
			nil, nil, 
			@"Are you sure you want to remove the selected users?");
	}
}


- (void)removeSelectedUsersResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	switch(returnCode)
	{
		case NSAlertDefaultReturn:		// Revert iFolder
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
						[[NSApp delegate] addLog:@"Adding iFolder User failed"];
					}
				}
			}
			break;
		}
	}
}



- (IBAction)searchUsers:(id)sender
{
	NSLog(@"Searching for users");
	if([[userSearch stringValue] length] > 0)
	{
		@try
		{
			if(curiFolder != nil)
			{
				NSArray *newUsers = [ifolderService 
										SearchDomainUsers:[curiFolder DomainID]
										withString:[userSearch stringValue] ];

				if(newUsers != nil)
					[foundUsersController setContent:newUsers];
				else
					[foundUsersController setContent: nil];
			}
		}
		@catch (NSException *e)
		{
			[[NSApp delegate] addLog:@"Searching iFolder Users failed with an exception"];
		}
	}
	else
	{
		@try
		{
			if(curiFolder != nil)
			{
				NSArray *newUsers = [ifolderService 
										GetDomainUsers:[curiFolder DomainID]
										withLimit:25];

				if(newUsers != nil)
					[foundUsersController setContent:newUsers];
				else
					[foundUsersController setContent: nil];
			}
		}
		@catch (NSException *e)
		{
			[[NSApp delegate] addLog:@"Searching iFolder Users failed with an exception"];
		}
	}
}

@end
