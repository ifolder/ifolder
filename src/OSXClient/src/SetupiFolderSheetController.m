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
 
 
#import "SetupiFolderSheetController.h"
#import "MainWindowController.h"

@implementation SetupiFolderSheetController

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

- (IBAction)cancelSetup:(id)sender
{
	[setupSheet orderOut:nil];
	[NSApp endSheet:setupSheet];
}

- (IBAction)setupiFolder:(id)sender
{
	if( ( [ [iFolderID stringValue] length] > 0) &&
		( [ [pathField stringValue] length] > 0 ) )
	{
		[setupSheet orderOut:nil];
		[NSApp endSheet:setupSheet];

		[[NSApp delegate] acceptiFolderInvitation:[iFolderID stringValue]
							InDomain:[domainID stringValue]
							toPath:[pathField stringValue] ];
	}
}

@end
