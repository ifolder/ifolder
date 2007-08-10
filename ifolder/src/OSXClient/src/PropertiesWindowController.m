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

#import "PropertiesWindowController.h"
#import "PropGeneralController.h"
#import "PropSharingController.h"
#import "iFolderWindowController.h"
#import "iFolder.h"

@implementation PropertiesWindowController


static PropertiesWindowController *sharedInstance = nil;


+ (PropertiesWindowController *)sharedInstance
{
	if(sharedInstance == nil)
	{
		sharedInstance = [[PropertiesWindowController alloc] initWithWindowNibName:@"Properties"];
	}

    return sharedInstance;
}




-(void)awakeFromNib
{
	[tabView selectTabViewItemAtIndex:initalTab];

	userMenuItem = [[NSMenuItem allocWithZone:[NSMenu menuZone]] initWithTitle:@"Access" action:NULL keyEquivalent:@""];
	[userMenu setTitle:NSLocalizedString(@"Access", @"Menu Tile for the Access menu for the sharing properties dialog")];
    [userMenuItem setSubmenu:userMenu];
    [[NSApp mainMenu] addItem:userMenuItem];	
}




- (void)windowWillClose:(NSNotification *)aNotification
{
	if(sharedInstance != nil)
	{
		if(userMenuItem != nil)
		{
			[[NSApp mainMenu] removeItem:userMenuItem];
			[userMenuItem release];
			userMenuItem = nil;
		}
		[sharedInstance release];
		sharedInstance = nil;
	}
}




- (void)tabView:(NSTabView *)tabView didSelectTabViewItem:(NSTabViewItem *)tabViewItem
{
	if(tabViewItem != sharingItem)
	{
		[searchDrawer close];	
	}
}



- (void)setSharingTab
{
	initalTab = 1;
}


- (void)setGeneralTab
{
	initalTab = 0;
}


@end
