#import "PropSharingController.h"
#import "iFolderService.h"
#import "MainWindowController.h"

@implementation PropSharingController


-(void)awakeFromNib
{
	ifolderService = [[iFolderService alloc] init];

	keyedUsers = [[NSMutableDictionary alloc] init];

	NSLog(@"Reading all iFolder Users");

	@try
	{
		int userCount;
		ifolderID = [[NSApp delegate] seletediFolderID];
		if(ifolderID != nil)
		{
			NSArray *newUsers = [ifolderService GetiFolderUsers:ifolderID ];
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
}

- (IBAction)refreshWindow:(id)sender
{
	NSLog(@"Refreshing Window");
}

- (IBAction)removeSelectedUsers:(id)sender
{
	NSLog(@"Removing Selected users");
}

- (IBAction)searchUsers:(id)sender
{
	NSLog(@"Searching for users");
/*
	@try
	{
		if(ifolderID != nil)
		{
			NSArray *newUsers = [ifolderService GetiFolderUsers:curiFolder ];
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

	foundUsersController	
*/
}

@end
