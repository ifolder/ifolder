#import "ConflictWindowController.h"
#import "iFolder.h"
#import "iFolderService.h"
#import "iFolderWindowController.h"
#import "iFolderApplication.h"


@implementation ConflictWindowController

static ConflictWindowController *conflictSharedInstance = nil;


+ (ConflictWindowController *)sharedInstance
{
	if(conflictSharedInstance == nil)
	{
		conflictSharedInstance = [[ConflictWindowController alloc] initWithWindowNibName:@"ConflictResolver"];
	}

    return conflictSharedInstance;
}


- (void)windowWillClose:(NSNotification *)aNotification
{
	if(conflictSharedInstance != nil)
	{
		[ifolder release];
		[conflictSharedInstance release];
		conflictSharedInstance = nil;
	}
}


- (void)awakeFromNib
{
	if([[NSUserDefaults standardUserDefaults] boolForKey:PREFKEY_WINPOS])
	{
		[super setShouldCascadeWindows:NO];
		[super setWindowFrameAutosaveName:@"ifolder_conflict_resolver"];
	}

	ifolderService = [[iFolderService alloc] init];

	ifolder = [[[iFolderWindowController sharedInstance] selectediFolder] retain];

	if(ifolder != nil)
	{
		[ifolderName setStringValue:[ifolder Name]];
		[ifolderPath setStringValue:[ifolder Path]];
		
		if([ifolder HasConflicts])
		{
			NSArray *ifolderconflicts = nil;
			
			@try
			{
				ifolderconflicts = [ifolderService GetiFolderConflicts:[ifolder ID]];
			}
			@catch(NSException *e)
			{
				ifolderconflicts = nil;
			}

			if(ifolderconflicts != nil)
			{
				NSMutableDictionary *nameConflicts = [[NSMutableDictionary alloc] init];
				int objCount;
				
				for(objCount = 0; objCount < [ifolderconflicts count]; objCount++)
				{
					iFolderConflict *conflict = [ifolderconflicts objectAtIndex:objCount];
					if( [[[conflict properties] objectForKey:@"IsNameConflict"] boolValue] == YES)
					{
						NSString *curKey = [[[conflict properties] objectForKey:@"Location"] lowercaseString];
						iFolderConflict *oldConflict = [nameConflicts objectForKey:curKey];
						if(oldConflict != nil)
						{
							[oldConflict mergeNameConflicts:conflict];
							continue;
						}
						else
							[nameConflicts setObject:conflict forKey:curKey];
					}
					
					if( [[ifolder CurrentUserRights] compare:@"ReadOnly"] == 0 )
					{
						[[conflict properties] setValue:[NSNumber numberWithBool:YES] forKey:@"IsReadOnly"];
					}

					[ifoldersController addObject:conflict];
				}
			}
		}
	}

}


- (void)windowDidBecomeMain:(NSNotification *)aNotification
{
	if( [[ifolder CurrentUserRights] compare:@"ReadOnly"] == 0 )
	{
		NSBeginAlertSheet(NSLocalizedString(@"You have read only access", @"Read Only Access Warning Message"), 
			NSLocalizedString(@"OK", @"Read Only Access Warning button"), nil, nil,
			[self window], self, nil, nil, NULL, 
			NSLocalizedString(@"Your ability to resolve conflicts is limited because you have read-only access to this iFolder.  Name conflicts must be renamed locally.  File conflicts will be overwritten by the version of the file on the server.", @"Read Only Access Warning details"));
	}
}



- (IBAction)saveLocal:(id)sender
{
	[self resolveFileConflicts:YES];
}

- (IBAction)saveServer:(id)sender
{
	[self resolveFileConflicts:NO];
}


-(void)resolveFileConflicts:(BOOL)saveLocal
{
	NSArray *selectedConflicts = [ifoldersController selectedObjects];
	NSMutableArray *removeConflicts = [[NSMutableArray alloc] init];

	int conCounter;
	for(conCounter = 0; conCounter < [selectedConflicts count]; conCounter++)
	{
		iFolderConflict *conflict = [selectedConflicts objectAtIndex:conCounter];
		if( [[[conflict properties] objectForKey:@"IsNameConflict"] boolValue] == NO)
		{
			NSString *iFolderID = [[conflict properties] objectForKey:@"iFolderID"];
			NSString *conID = [[conflict properties] objectForKey:@"ConflictID"];
		
			@try
			{
				[ifolderService ResolveFileConflict:iFolderID withID:conID localChanges:saveLocal];
				[removeConflicts addObject:conflict];
			}
			@catch(NSException *ex)
			{
				NSBeginAlertSheet(NSLocalizedString(@"Error resolving conflict", nil), 
					NSLocalizedString(@"OK", nil), nil, nil,
					[self window], self, nil, nil, NULL, 
					[ex name]);
			}
		}
	}
	[ifoldersController removeObjects:removeConflicts];
}


- (IBAction)renameFile:(id)sender
{
	if([ifoldersController selectionIndex] != NSNotFound)
	{
		BOOL validName = NO;

		iFolderConflict *conflict = [[ifoldersController arrangedObjects] objectAtIndex:[ifoldersController selectionIndex]];
		NSString *iFolderID = [[conflict properties] objectForKey:@"iFolderID"];
		NSString *newName = [[conflict properties] objectForKey:@"Name"];
		NSString *localName = [[conflict properties] objectForKey:@"LocalName"];
		NSString *serverName = [[conflict properties] objectForKey:@"ServerName"];
		NSString *localID = [[conflict properties] objectForKey:@"LocalConflictID"];
		NSString *serverID = [[conflict properties] objectForKey:@"ConflictID"];

		NSString *fileDirectory = [[[conflict properties] objectForKey:@"Location"] stringByDeletingLastPathComponent];
		NSString *newPath = [fileDirectory stringByAppendingPathComponent:newName];

		if([[NSFileManager defaultManager] fileExistsAtPath:newPath])
		{
			NSBeginAlertSheet(NSLocalizedString(@"The specified name already exists", @"Resolve has duplicate name error message"), 
				NSLocalizedString(@"OK", @"Resolve has duplicate name error button"), nil, nil,
				[self window], self, nil, nil, NULL, 
				NSLocalizedString(@"Please choose a different name.", @"Resolve has duplicate name error details"));
		}
		else
		{
			validName = YES;
		}

		if(validName)
		{
			if(serverID != nil)
			{
				if([[ifolder CurrentUserRights] compare:@"ReadOnly"] == 0)
				{
					@try
					{
						[ifolderService RenameAndResolveConflict:iFolderID withID:serverID usingFileName:newName];
						[ifoldersController removeObjectAtArrangedObjectIndex:[ifoldersController selectionIndex]];
					}
					@catch(NSException *ex)
					{
						NSBeginAlertSheet(NSLocalizedString(@"Error resolving conflict", nil), 
							NSLocalizedString(@"OK", nil), nil, nil,
							[self window], self, nil, nil, NULL, 
							[ex name]);
					}
				}
				else
				{
					@try
					{
						[ifolderService ResolveNameConflict:iFolderID withID:localID usingName:newName];
						[ifolderService ResolveNameConflict:iFolderID withID:serverID usingName:serverName];
						[ifoldersController removeObjectAtArrangedObjectIndex:[ifoldersController selectionIndex]];
					}
					@catch(NSException *ex)
					{
						NSBeginAlertSheet(NSLocalizedString(@"Error resolving conflict", nil), 
							NSLocalizedString(@"OK", nil), nil, nil,
							[self window], self, nil, nil, NULL, 
							[ex name]);
					}
				}
			}
			else
			{
				@try
				{
					[ifolderService ResolveNameConflict:iFolderID withID:localID usingName:newName];
					[ifoldersController removeObjectAtArrangedObjectIndex:[ifoldersController selectionIndex]];
				}
				@catch(NSException *ex)
				{
					NSBeginAlertSheet(NSLocalizedString(@"Error resolving conflict", nil), 
						NSLocalizedString(@"OK", nil), nil, nil,
						[self window], self, nil, nil, NULL, 
						[ex name]);
				}
			}
		}
	}
}




@end
