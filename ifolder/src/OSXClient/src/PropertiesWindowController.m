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

	userMenuItem = [[NSMenuItem allocWithZone:[NSMenu menuZone]] initWithTitle:@"Users" action:NULL keyEquivalent:@""];
	[userMenu setTitle:NSLocalizedString(@"Users", nil)];
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
