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
*                 $Author: Satyam <ssutapalli@novell.com> 25/03/2008	Advanced Conflict Resolver
*                 $Modified by: Satyam <ssutapalli@novell.com> 10/04/2008	Fix to create recurssive directories
*-----------------------------------------------------------------------------
* This module is used to:
*			This is used to handle advanced conflict resolutions
*******************************************************************************/

#import "AdvConflictController.h"
#import "iFolderWindowController.h"

#import "iFolderData.h"

@implementation AdvConflictController

//=======================================================================
// browsePath
// Method to select the path of conflict bin
//=======================================================================
- (IBAction)browsePath:(id)sender
{
	int result;
	
	NSOpenPanel *oPanel = [NSOpenPanel openPanel];
	
	[oPanel setAllowsMultipleSelection:NO];
	[oPanel setCanChooseDirectories:YES];
	[oPanel setCanChooseFiles:NO];
	[oPanel setTitle:@"Save to Conflict Bin Path"];
	
	result = [oPanel runModalForDirectory:NSHomeDirectory() file:nil types:nil];
	
	if (result != NSOKButton)
	{
		return;
	}
	
	[conflictBinPath setStringValue:[oPanel directory]];
}

//=======================================================================
// closePanel
// Method to close the panel. Infact, it will order out from the windows.
//=======================================================================
- (IBAction)closePanel:(id)sender
{
	if([[NSFileManager defaultManager] fileExistsAtPath:tempPath])
	{
		[[NSFileManager defaultManager] removeFileAtPath:tempPath handler:nil];	
		tempPath = nil;
	}
	[[iFolderData sharedInstance] synciFolderNow:[ifolder ID]];
	[setupSheet orderOut:nil];
	[NSApp endSheet:setupSheet];
}

//=======================================================================
// conflictBinMatrixClicked
// Called When the conflict bin radio buttons are selected/deselected
//=======================================================================
- (IBAction)conflictBinMatrixClicked:(id)sender
{
	if([conflictMatrix selectedColumn] == 0)
	{
		[ifolderMatrix setState:1 atRow:0 column:1];
		localOnly = NO;
		return;
	}
	[ifolderMatrix setState:1 atRow:0 column:0];
	localOnly = YES;
}

//=======================================================================
// enableConflictBin
// Method enable/disable the controls when "Enable" button is clicked
//=======================================================================
- (IBAction)enableConflictBin:(id)sender
{
	if([enableButton state])
	{
		[conflictMatrix setEnabled:YES];
		[conflictBinPath setEnabled:YES];
		[browseButton setEnabled:YES];		
		[self ifolderMatrixClicked:sender];
	}
	else
	{
		[conflictMatrix setEnabled:NO];
		[conflictBinPath setEnabled:NO];
		[browseButton setEnabled:NO];	
	}
}

//=======================================================================
// ifolderMatrixClicked
// Called When the iFolder version radio buttons are selected/deselected
//=======================================================================
- (IBAction)ifolderMatrixClicked:(id)sender
{
	if([ifolderMatrix selectedColumn] == 0)
	{
		localOnly = YES;
		if([conflictMatrix isEnabled])
		{
			[conflictMatrix setState:1 atRow:0 column:1];
		}
	}
	else
	{
		localOnly = NO;
		if([conflictMatrix isEnabled])
		{
			[conflictMatrix setState:1 atRow:0 column:0];
		}
		
	}
}

//=======================================================================
// localFileOpen
// Method called to show the local version of the conflict
//=======================================================================
- (IBAction)localFileOpen:(id)sender
{
	[self openConflictFile:YES];
}

//=======================================================================
// openConflictFile
// This method will open the conflict file depending on local or server
// version button is pressed.
//=======================================================================
- (void)openConflictFile:(BOOL)local
{
	if([conflictTableView selectedRow] == -1)
	{
		return;
	}

	if(local) //show local file
	{
		if(![[NSWorkspace sharedWorkspace] openFile:[hidenLocalFilePath stringValue]])
		{
			[self showFile:[hidenLocalFilePath stringValue]];
		}
	}
	else //show server file
	{
		//openPath = [openPath stringByAppendingString:[hidenServerFilePath stringValue]];
		if(![[NSWorkspace sharedWorkspace] openFile:[hidenServerFilePath stringValue]])
		{
			if([[NSFileManager defaultManager] fileExistsAtPath:tempPath])
			{
				[[NSFileManager defaultManager] removeFileAtPath:tempPath handler:nil];	
			}

			if([[NSFileManager defaultManager] copyPath:[hidenServerFilePath stringValue] toPath:tempPath handler:nil])
			{
				if(![[NSWorkspace sharedWorkspace] openFile:tempPath])
				{
					[self showFile:[hidenServerFilePath stringValue]];
				}	
			}
			else
			{
				[self showFile:[hidenServerFilePath stringValue]];
			}
		}
	}
}

- (void)showFile:(NSString*)pathOfFile
{
	int result;
	NSOpenPanel *oPanel = [NSOpenPanel openPanel];
	
	[oPanel setAllowsMultipleSelection:NO];
	[oPanel setCanChooseDirectories:NO];
	[oPanel setCanChooseFiles:YES];
	[oPanel setTitle:NSLocalizedString(@"Choose application to open with: ",@"ApplicationSelectionTitle")];
	
	NSString *lastPath = @"/Applications";
	result = [oPanel runModalForDirectory:lastPath file:nil types:nil];
	
	if (result == NSOKButton)
	{
		[[NSWorkspace sharedWorkspace] openFile:pathOfFile withApplication:[oPanel filename]];
	}
}
//=======================================================================
// prepareToShow
// Method to prepare the dialog before showing
//=======================================================================
- (void)prepareToShow
{
	/*
	[localFileName setStringValue:@""];
	[localFileDate setStringValue:@""];
	[localFileSize setStringValue:@""];
	[serverFileDate setStringValue:@""];
	[serverFileName setStringValue:@""];
	[serverFileSize setStringValue:@""];
	*/
	[[conflictController content] removeAllObjects];
	
	[ifolderMatrix setState:1 atRow:0 column:0];
	localOnly = YES;
	[enableButton setState:NO];
	[conflictBinPath setStringValue:@""];
	[self enableConflictBin:nil];
}

//====================================================================
// renameFile
// Renames the file to avoid conflict
//====================================================================
- (IBAction)renameFile:(id)sender
{
	if([conflictController selectionIndex] == NSNotFound)
		return;

	iFolderConflict *conflict = [[conflictController arrangedObjects] objectAtIndex:[conflictController selectionIndex]];
	NSString *iFolderID = [[conflict properties] objectForKey:@"iFolderID"];
	NSString *newName = [renameNewFileName stringValue];
	NSString *localName = [[conflict properties] objectForKey:@"LocalName"];
	NSString *serverName = [[conflict properties] objectForKey:@"ServerName"];
	NSString *localID = [[conflict properties] objectForKey:@"LocalConflictID"];
	NSString *serverID = [[conflict properties] objectForKey:@"ServerConflictID"];

	NSString *fileDirectory = [[[conflict properties] objectForKey:@"Location"] stringByDeletingLastPathComponent];
	NSString *newPath = [fileDirectory stringByAppendingPathComponent:newName];

	if(![[iFolderData sharedInstance] checkFileName:newName]) {
		NSBeginAlertSheet(NSLocalizedString(@"The specified name contains invalid characters", @"New name has invalid characters"), 
				  NSLocalizedString(@"OK", @"Resolve has duplicate name error button"), nil, nil,
				  [self window], self, nil, nil, NULL, 
				  NSLocalizedString(@"Ensure that the name does not contain these characters : '\\', ':', '*', '?', '\"', '<', '>', '|'", @"List of invalid characters displayed."));
		return;
	}

	if([[NSFileManager defaultManager] fileExistsAtPath:newPath])
	{
		NSBeginAlertSheet(NSLocalizedString(@"The specified name already exists", @"Resolve has duplicate name error message"), 
				  NSLocalizedString(@"OK", @"Resolve has duplicate name error button"), nil, nil,
				  [self window], self, nil, nil, NULL, 
				  NSLocalizedString(@"Please choose a different name.", @"Resolve has duplicate name error details"));
		return;
	}

	if(serverID != nil)
	{
		if([[ifolder CurrentUserRights] compare:@"ReadOnly"] == 0)
		{
			@try
			{
				[[iFolderData sharedInstance] renameAndResolveConflict:iFolderID withID:serverID usingFileName:newName];
				[conflictController removeObjectAtArrangedObjectIndex:[conflictController selectionIndex]];
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
				[[iFolderData sharedInstance] resolveNameConflict:iFolderID withID:localID usingName:newName];

				// If this is because of an illegal character which is supported on Mac, there may not
				// be a server conflict so check here
				[[iFolderData sharedInstance] resolveNameConflict:iFolderID withID:serverID usingName:serverName];

				[conflictController removeObjectAtArrangedObjectIndex:[conflictController selectionIndex]];
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
			[[iFolderData sharedInstance] resolveNameConflict:iFolderID withID:localID usingName:newName];
			[conflictController removeObjectAtArrangedObjectIndex:[conflictController selectionIndex]];
		}
		@catch(NSException *ex)
		{
			NSBeginAlertSheet(NSLocalizedString(@"Error resolving conflict", nil), 
					  NSLocalizedString(@"OK", nil), nil, nil,
					  [self window], self, nil, nil, NULL, 
					  [ex name]);
		}
	}

	//Hack : Hide Rename NSBox if conflictController has nothing in it.
	if ([[conflictController arrangedObjects] count] == 0)
		[renameBox setHidden:TRUE];
}

//=======================================================================
// saveFile
// Method that will decide to save the file whether local or server version
// to be saved and whether a local copy is needed to save or not.
//=======================================================================
- (IBAction)saveFile:(id)sender
{
	BOOL isDir;
	NSString* path = nil;
	
	int selectedConflict = [conflictController selectionIndex];
	iFolderConflict* conflict = [[conflictController arrangedObjects] objectAtIndex:selectedConflict];
	NSString* conflictType = [[conflict properties] objectForKey:@"ConflictType"];
	if([conflictType isEqualToString:@"File"])
	{
		if([enableButton state])
		{
			if([[conflictBinPath stringValue] isEqualToString:@""])
			{
				NSRunAlertPanel(NSLocalizedString(@"Path field is empty",@"AdvConflict Empty Conflict Bin Title"),
								NSLocalizedString(@"Specify a valid path for the Conflict bin",@"AdvConflict Empty Conflict Bin Message"),
								NSLocalizedString(@"OK",@"OK Button"),nil,nil);
				return;
			}
			
			path = [conflictBinPath stringValue];	
			
			if(![[iFolderData sharedInstance] createDirectoriesRecurssively:[conflictBinPath stringValue]])
			{
				return;
			}
			
			if(![path hasSuffix:@"/"])
			{
				path = [path stringByAppendingString:@"/"];
			}
		}
		@try
	{
		[[iFolderData sharedInstance] resolveEnhancedFileConflict:[ifolder ID] havingConflictID:[[conflict properties] objectForKey:@"ConflictID"] hasLocalChange:localOnly withConflictBinPath:path];
		[conflictController removeObjectAtArrangedObjectIndex:selectedConflict];
	}
		@catch(NSException* ex)
	{
			NSRunAlertPanel(NSLocalizedString(@"iFolder Conflict Exception",@"iFolder Conflict Exception Title"),
							[NSString stringWithFormat:NSLocalizedString(@"An exception was encountered while resolving the conflict : %@",@"iFolder Conflict Exception Message"),ex],
							NSLocalizedString(@"OK",@"OK Button"),nil,nil);
	}
	}
	else if([conflictType isEqualToString:@"Name"])
	{
		NSRunAlertPanel(NSLocalizedString(@"File Name Conflict",@"Name conflict title"),
						NSLocalizedString(@"Fail to resolve Name Conflict, rename the local file to resolve the conflict.",@"Name conflict message"),
						NSLocalizedString(@"OK",@"OK Button"),nil,nil);
	}
	
	//Hack : Hide Rename NSBox if conflictController has nothing in it.
	if ([[conflictController arrangedObjects] count] == 0)
		[renameBox setHidden:TRUE];

	/*
	 @try
	 {
		 [[iFolderData sharedInstance] resolveEnhancedFileConflict:[ifolder ID] havingConflictID:[[conflict properties] objectForKey:@"ConflictID"] hasLocalChange:localOnly withConflictBinPath:path];
		 [conflictController removeObjectAtArrangedObjectIndex:selectedConflict];
	 }
	 @catch(NSException* ex)
	 {
		 NSRunAlertPanel(NSLocalizedString(@"iFolder Conflict Exception",@"iFolder Conflict Exception Title"),
						 [NSString stringWithFormat:NSLocalizedString(@"An exception was encountered while resolving the conflict : %@",@"iFolder Conflict Exception Message"),ex],
						 NSLocalizedString(@"OK",@"OK Button"),nil,nil);
	 }
	 */
}
	
//=======================================================================
// serverFileOpen
//  Method called to show the server version of the conflict
//=======================================================================		
- (IBAction)serverFileOpen:(id)sender
{
	[self openConflictFile:NO];
}

//=======================================================================
// showWindow
// Method called to prepare the window before showing and then it shows
// with appropriate details
//=======================================================================
- (IBAction)showWindow:(id)sender
{
	[self prepareToShow];
	tempPath = @"/tmp/if_conflict_file";
	
	ifolder = [[iFolderWindowController sharedInstance] selectediFolder];
	
	if(ifolder != nil)
	{
		[conflictName setStringValue:[ifolder Name]];
		[locationName setStringValue:[ifolder Path]];
		
		if([ifolder HasConflicts])
		{
			@try
			{
				conflictFiles = [[iFolderData sharedInstance] getiFolderConflicts:[ifolder ID]];
			}
			@catch(NSException*)
			{
				conflictFiles = nil;
			}
			
			if(conflictFiles != nil)
			{
				 int objCount;
				 NSMutableDictionary *nameConflicts = [[NSMutableDictionary alloc] init];

				 for(objCount = 0; objCount < [conflictFiles count]; objCount++)
				 {
					 iFolderConflict *conflict = [conflictFiles objectAtIndex:objCount];
					 if( [[[conflict properties] objectForKey:@"IsNameConflict"] boolValue] == YES)
					 {
						[[conflict properties] setValue:[NSString stringWithString:@"Name"] forKey:@"ConflictType"];
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
					 else
					 {
						 [[conflict properties] setValue:[NSString stringWithString:@"File"] forKey:@"ConflictType"];
					 }
					 
					 if( [[ifolder CurrentUserRights] compare:@"ReadOnly"] == 0 )
					 {
						 [[conflict properties] setValue:[NSNumber numberWithBool:YES] forKey:@"IsReadOnly"];
					 }
					 
					 [conflictController addObject:conflict];
				 }
				 
			}
		}
	}
	
	//Show the panel
	[NSApp beginSheet:setupSheet modalForWindow:mainWindow modalDelegate:self didEndSelector:NULL contextInfo:nil];
	
	if( [[ifolder CurrentUserRights] compare:@"ReadOnly"] == 0 )
	{
		NSBeginAlertSheet(NSLocalizedString(@"You have read only access", @"Read Only Access Warning Message"), 
						  NSLocalizedString(@"OK", @"OK Button"), nil, nil,
						  [self window], self, nil, nil, NULL, 
						  NSLocalizedString(@"Your ability to resolve conflicts is limited because you have read-only access to this iFolder.  Name conflicts must be renamed locally.  File conflicts will be overwritten by the version of the file on the server.", @"Read Only Access Warning details"));
	}
}
@end
