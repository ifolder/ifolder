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
	NSLog(@"ConflictWindowController Awoke from Nib");

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
						iFolderConflict *oldConflict = [nameConflicts objectForKey:[[conflict properties] objectForKey:@"Location"]];
						if(oldConflict != nil)
						{
							[oldConflict mergeNameConflicts:conflict];
							continue;
						}
						else
							[nameConflicts setObject:conflict forKey:[[conflict properties] objectForKey:@"Location"]];
					}

					[ifoldersController addObject:conflict];
				}
			}
		}
	}
}




- (IBAction)saveLocal:(id)sender
{
	NSLog(@"Resolving conflict to Local copy");
	[self resolveFileConflicts:YES];
}

- (IBAction)saveServer:(id)sender
{
	NSLog(@"Resolving conflict to Server copy");
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
					[ex description]);
			}
		}
	}
	[ifoldersController removeObjects:removeConflicts];
}


- (IBAction)renameFile:(id)sender
{
	if([ifoldersController selectionIndex] != NSNotFound)
	{
		iFolderConflict *conflict = [[ifoldersController arrangedObjects] objectAtIndex:[ifoldersController selectionIndex]];
		NSString *iFolderID = [[conflict properties] objectForKey:@"iFolderID"];
		NSString *newName = [[conflict properties] objectForKey:@"Name"];
		NSString *localName = [[conflict properties] objectForKey:@"LocalName"];
		NSString *serverName = [[conflict properties] objectForKey:@"ServerName"];
		NSString *localID = [[conflict properties] objectForKey:@"LocalConflictID"];
		NSString *serverID = [[conflict properties] objectForKey:@"ConflictID"];

		if(localName != nil)
		{
			// it's a local rename
			if( [localName compare:newName] == 0)
			{
				NSBeginAlertSheet(NSLocalizedString(@"File names match", nil), 
					NSLocalizedString(@"OK", nil), nil, nil,
					[self window], self, nil, nil, NULL, 
					NSLocalizedString(@"You must select a new name in order to resolve this conflict.", nil));
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
						[ex description]);
				}
			}
		}
		else
		{
			// it's a server rename
			if( [serverName compare:newName] == 0)
			{
				NSBeginAlertSheet(NSLocalizedString(@"File names match", nil), 
					NSLocalizedString(@"OK", nil), nil, nil,
					[self window], self, nil, nil, NULL, 
					NSLocalizedString(@"You must select a new name in order to resolve this conflict.", nil));
			}
			else
			{
				@try
				{
					[ifolderService ResolveNameConflict:iFolderID withID:serverID usingName:newName];
					[ifoldersController removeObjectAtArrangedObjectIndex:[ifoldersController selectionIndex]];
				}
				@catch(NSException *ex)
				{
					NSBeginAlertSheet(NSLocalizedString(@"Error resolving conflict", nil), 
						NSLocalizedString(@"OK", nil), nil, nil,
						[self window], self, nil, nil, NULL, 
						[ex description]);
				}
			}
		}
		
	}
}




@end
