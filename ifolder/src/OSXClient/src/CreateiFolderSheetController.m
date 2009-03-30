/***********************************************************************
 |  $RCSfile$
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 |  Author: Calvin Gaisford <cgaisford@novell.com>
 | 
 ***********************************************************************/

#import "CreateiFolderSheetController.h"
#import "iFolderWindowController.h"
#import "iFolderData.h"

@implementation CreateiFolderSheetController


-(void)awakeFromNib
{
	// bind the fields up to our data
	[domainIDField bind:@"value" toObject:[[iFolderData sharedInstance] domainArrayController]
				withKeyPath:@"selection.properties.ID" options:nil];

	[domainSelector bind:@"contentValues" toObject:[[iFolderData sharedInstance] domainArrayController]
				withKeyPath:@"arrangedObjects.properties.name" options:nil];

	[domainSelector bind:@"selectedIndex" toObject:[[iFolderData sharedInstance] domainArrayController]
				withKeyPath:@"selectionIndex" options:nil];


}



- (IBAction) showWindow:(id)sender
{
	// Because we use the same dialog, clear out the path
	[pathField setStringValue:@""];
	
	[NSApp beginSheet:createSheet modalForWindow:mainWindow
		modalDelegate:self didEndSelector:NULL contextInfo:nil];
}




- (IBAction) cancelCreating:(id)sender
{
	[createSheet orderOut:nil];
	[NSApp endSheet:createSheet];
}




- (IBAction) createiFolder:(id)sender
{
	if(	( [ [domainIDField stringValue] length] > 0 ) &&
		( [ [pathField stringValue] length] > 0 ) )
	{
		[ifolderWindowController createiFolder:[pathField stringValue] inDomain:[domainIDField stringValue] ];
		[createSheet orderOut:nil];
		[NSApp endSheet:createSheet];
	}
}




- (IBAction) browseForPath:(id)sender
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


@end
