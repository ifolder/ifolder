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

#import "CreateiFolderSheetController.h"
#import "iFolderWindowController.h"


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

- (iFolderDomain *)selectedDomain { return selectedDomain; }
- (void)setSelectedDomain:(iFolderDomain *)aSelectedDomain
{
    if (selectedDomain != aSelectedDomain) {
        [selectedDomain autorelease];
        selectedDomain = [aSelectedDomain retain];
    }
}

@end
