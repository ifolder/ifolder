//
//  CreateiFolderSheetController.m
//  iFolder
//
//  Created by Calvin Gaisford on 12/18/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

#import "CreateiFolderSheetController.h"


@implementation CreateiFolderSheetController


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
	if( ( selectedDomain != nil ) &&
		( [ [pathField stringValue] length] > 0 ) )
	{
		[[NSApp delegate] createiFolder:[pathField stringValue] inDomain:[domainIDField stringValue] ];
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

- (iFolderDomain *)selectedDomain { return selectedDomain; }
- (void)setSelectedDomain:(iFolderDomain *)aSelectedDomain
{
    if (selectedDomain != aSelectedDomain) {
        [selectedDomain autorelease];
        selectedDomain = [aSelectedDomain retain];
    }
}

@end
