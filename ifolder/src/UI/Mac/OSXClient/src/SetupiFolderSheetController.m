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
 
#import "SetupiFolderSheetController.h"
#import "iFolderWindowController.h"
#import "iFolderData.h"

@implementation SetupiFolderSheetController

//===================================================================
// awakeFromNib
// Method that will be called before loading Nib
//===================================================================
-(void)awakeFromNib
{
	// bind the fields up to our data
	[domainID bind:@"value" toObject:[[iFolderData sharedInstance] ifolderArrayController]
				withKeyPath:@"selection.properties.DomainID" options:nil];
	[iFolderID bind:@"value" toObject:[[iFolderData sharedInstance] ifolderArrayController]
				withKeyPath:@"selection.properties.ID" options:nil];
	[iFolderName bind:@"value" toObject:[[iFolderData sharedInstance] ifolderArrayController]
				withKeyPath:@"selection.properties.Name" options:nil];
	[SharedBy bind:@"value" toObject:[[iFolderData sharedInstance] ifolderArrayController]
				withKeyPath:@"selection.properties.Owner" options:nil];
	[Rights bind:@"value" toObject:[[iFolderData sharedInstance] ifolderArrayController]
				withKeyPath:@"selection.properties.CurrentUserRights" options:nil];
}

//===================================================================
// showWindow
// Display the Create iFolder dialog
//===================================================================
- (IBAction) showWindow:(id)sender
{
	if( [ [iFolderID stringValue] length] > 0)
	{
		// Because we use the same dialog, clear out the path
		[pathField setStringValue:@""];
	
		[NSApp beginSheet:setupSheet modalForWindow:mainWindow
			modalDelegate:self didEndSelector:NULL contextInfo:nil];
	}
}

//===================================================================
// browseForPath
// Select the file by browsing for the required path
//===================================================================
- (IBAction)browseForPath:(id)sender
{
	int result;
	NSOpenPanel *oPanel = [NSOpenPanel openPanel];
	
	[oPanel setAllowsMultipleSelection:NO];
	[oPanel setCanChooseDirectories:YES];
	[oPanel setCanChooseFiles:NO];
	NSString *lastPath = [pathField stringValue];
	if([lastPath length] > 0)
		result = [oPanel runModalForDirectory:lastPath file:nil types:nil];
	else
		result = [oPanel runModalForDirectory:NSHomeDirectory() file:nil types:nil];
	
	if (result == NSOKButton)
	{
		NSString *dirName = [oPanel directory];
		[pathField setStringValue:dirName];
	}
}

//===================================================================
// cancelSetup
// Cancel and close the window
//===================================================================
- (IBAction)cancelSetup:(id)sender
{
	[setupSheet orderOut:nil];
	[NSApp endSheet:setupSheet];
}

//===================================================================
// setupiFolder
// Create the iFolder when user accepts
//===================================================================
- (IBAction)setupiFolder:(id)sender
{
	if( ( [ [iFolderID stringValue] length] > 0) &&
		( [ [pathField stringValue] length] > 0 ) )
	{
		[setupSheet orderOut:nil];
		[NSApp endSheet:setupSheet];
		NSFileManager *fm = [NSFileManager defaultManager];
		BOOL isDir;
		int option;
		NSString * folderName = @"";
		
		folderName= [[ [ folderName stringByAppendingString:[pathField stringValue] ]stringByAppendingString: @"/"] stringByAppendingString: [[ifolderWindowController selectediFolder] Name]];
		
		if((fm !=NULL  && [fm fileExistsAtPath:folderName isDirectory:&isDir] && isDir)||  ([[pathField stringValue] hasSuffix: [[ifolderWindowController selectediFolder] Name]]))
		{
			 option = NSRunAlertPanel(NSLocalizedString(@"A folder with the name you specified already exists",@"Folder already exists"),NSLocalizedString(@"Click Yes to merge or No to select a different location",@"Download default ifolder merge option"),NSLocalizedString(@"Yes",@"   Confirmation dialog to download default ifolder and merge"),NSLocalizedString(@"No",@"Negative confiramtion to download default ifolder and merge"),nil);
			
			if(option != NSAlertDefaultReturn)
			{
				return;
			}
			else
			{

				//iFolder* tempiFolder = [[iFolderData sharedInstance] mergeiFolder:defiFolderID InDomain:domID toPath:[ifolderPath stringValue]];
			[ifolderWindowController acceptiFolderInvitation:[iFolderID stringValue] 
												InDomain:[domainID stringValue]	
												toPath:[pathField stringValue] 
												canMerge:YES 
		];

				//merge = YES;
			}
		}
		else {
		[ifolderWindowController acceptiFolderInvitation:[iFolderID stringValue] 
												InDomain:[domainID stringValue]	
												toPath:[pathField stringValue] 
												canMerge:NO 
		];
            }
	}
}

@end
