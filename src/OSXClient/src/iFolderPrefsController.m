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
 
#import "iFolderPrefsController.h"
#import "MainWindowController.h"
#import "AccountsController.h"

@implementation iFolderPrefsController

- (void)awakeFromNib
{
	[self setShouldCascadeWindows:NO];
//	[self setWindowFrameAutosaveName:@"iFolder Preferences"];
	
	[[self window] setContentSize:[generalView frame].size];
	[[self window] setContentView: generalView];
	[[self window] setTitle:@"iFolder Preferences: General"];
	
	[self setupToolbar];

	[toolbar setSelectedItemIdentifier:@"General"];

	modalReturnCode = 0;

	// Setup the controls
}



- (void) updateSize:(NSSize)newSize
{
	NSRect oldFrameRect = [[self window] frame];
	NSRect oldViewRect =  [[[self window] contentView] frame];

	int toolbarSize = oldFrameRect.size.height - oldViewRect.size.height;
	int newY = oldFrameRect.origin.y + oldFrameRect.size.height - newSize.height - toolbarSize;

	[[self window] setFrame: NSMakeRect(oldFrameRect.origin.x, newY, newSize.width, newSize.height + toolbarSize) 
			display: YES animate: YES];
}


// Toobar Delegates
- (NSToolbarItem *)toolbar:(NSToolbar *)toolbar
	itemForItemIdentifier:(NSString *)itemIdentifier
	willBeInsertedIntoToolbar:(BOOL)flag
{
	return[toolbarItemDict objectForKey:itemIdentifier];
}


- (NSArray *)toolbarAllowedItemIdentifiers:(NSToolbar *)toolbar
{
	return toolbarItemArray;
}


- (NSArray *)toolbarSelectableItemIdentifiers:(NSToolbar *)toolbar
{
	return toolbarItemArray;
}


- (NSArray *)toolbarDefaultItemIdentifiers:(NSToolbar *)toolbar
{
	return toolbarItemArray;
}


- (void)generalPreferences:(NSToolbarItem *)item
{
	if([[self window] contentView] != generalView)
	{
		NSLog(@"Switching view to general Page");
		[[self window] setContentView: blankView];
		[self updateSize:[generalView frame].size];
		[[self window] setContentView: generalView];
		[[self window] setTitle:@"iFolder Preferences: General"];	
	}
}


- (void)accountPreferences:(NSToolbarItem *)item
{
	if([[self window] contentView] != accountsView)
	{
		NSLog(@"Switching view to accounts Page");
//		[accountsController refreshData];
		[[self window] setContentView: blankView];
		[self updateSize:[accountsView frame].size];
		[[self window] setContentView: accountsView];
		[[self window] setTitle:@"iFolder Preferences: Accounts"];	
	}
}


- (void)syncPreferences:(NSToolbarItem *)item
{
	if([[self window] contentView] != syncView)
	{
		NSLog(@"Switching view to Synchronization Page");
		[[self window] setContentView: blankView];
		[self updateSize:[syncView frame].size];
		[[self window] setContentView: syncView];
		[[self window] setTitle:@"iFolder Preferences: Synchronization"];	
	}
}


- (void)notifyPreferences:(NSToolbarItem *)item
{
	if([[self window] contentView] != notifyView)
	{
		NSLog(@"Switching view to notify Page");
		[[self window] setContentView: blankView];
		[self updateSize:[notifyView frame].size];
		[[self window] setContentView: notifyView];
		[[self window] setTitle:@"iFolder Preferences: Notification"];	
	}
}



- (void)setupToolbar
{
	toolbarItemDict = [[NSMutableDictionary alloc] init];
	toolbarItemArray = [[NSMutableArray alloc] init];

	NSToolbarItem *item=[[NSToolbarItem alloc] initWithItemIdentifier:@"General"];
	[item setPaletteLabel:@"General"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"General"]; // name for the item in the toolbar
	[item setToolTip:@"General Settings"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(generalPreferences:)];
	[item setImage:[NSImage imageNamed:@"prefs-general"]];
    [toolbarItemDict setObject:item forKey:@"General"]; // add to toolbar list
	[toolbarItemArray addObject:@"General"];
	[item release];	

	
	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"Accounts"];
	[item setPaletteLabel:@"Accounts"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Accounts"]; // name for the item in the toolbar
	[item setToolTip:@"Accounts"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(accountPreferences:)];
	[item setImage:[NSImage imageNamed:@"prefs-accounts"]];
    [toolbarItemDict setObject:item forKey:@"Accounts"]; // add to toolbar list
	[toolbarItemArray addObject:@"Accounts"];
	[item release];


	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"Synchronization"];
	[item setPaletteLabel:@"Synchronization"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Synchronization"]; // name for the item in the toolbar
	[item setToolTip:@"Synchronization"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(syncPreferences:)];
	[item setImage:[NSImage imageNamed:@"prefs-sync"]];
    [toolbarItemDict setObject:item forKey:@"Synchronization"]; // add to toolbar list
	[toolbarItemArray addObject:@"Synchronization"];
	[item release];
	
	
	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"Notification"];
	[item setPaletteLabel:@"Notification"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Notification"]; // name for the item in the toolbar
	[item setToolTip:@"Notification"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(notifyPreferences:)];
	[item setImage:[NSImage imageNamed:@"prefs-notification"]];
    [toolbarItemDict setObject:item forKey:@"Notification"]; // add to toolbar list
	[toolbarItemArray addObject:@"Notification"];
	[item release];	


	toolbar = [[NSToolbar alloc] initWithIdentifier:@"iFolderPrefsToolbar"];
	[toolbar setDelegate:self];
	[toolbar setAllowsUserCustomization:NO];
	[toolbar setAutosavesConfiguration:NO];
	[[self window] setToolbar:toolbar];
}



/*
	This is old binding code that I didn't want to loose so it's down here

    NSMutableDictionary *bindingOptions = [NSMutableDictionary dictionary];
    	
	// binding options for "name"
	[bindingOptions setObject:@"No Name" forKey:@"NSNullPlaceholder"];

	// binding for "price" column
    NSTableColumn *accountsColumn = [accountView tableColumnWithIdentifier:@"Accounts"];

    [accountsColumn bind:@"value" toObject:[[NSApp delegate] DomainsController]
	  withKeyPath:@"arrangedObjects.properties.Name" options:bindingOptions];

	// binding for selected "Name" field
    [accountName bind:@"value" toObject:[[NSApp delegate] DomainsController]
				 withKeyPath:@"selection.properties.Name" options:bindingOptions];


	// binding for selected "Host" field
    [host bind:@"value" toObject:[[NSApp delegate] DomainsController]
				 withKeyPath:@"selection.properties.Host" options:bindingOptions];	  	
    [host bind:@"enabled" toObject:[[NSApp delegate] DomainsController]
				 withKeyPath:@"selection.properties.CanEditHost" options:bindingOptions];	


	// binding for selected "UserName" field
    [userName bind:@"value" toObject:[[NSApp delegate] DomainsController]
				 withKeyPath:@"selection.properties.UserName" options:bindingOptions];	
    [userName bind:@"enabled" toObject:[[NSApp delegate] DomainsController]
				 withKeyPath:@"selection.properties.CanEditUserName" options:bindingOptions];	


	// binding for selected "Password" field
    [password bind:@"value" toObject:[[NSApp delegate] DomainsController]
				 withKeyPath:@"selection.properties.Password" options:bindingOptions];	
    [password bind:@"enabled" toObject:[[NSApp delegate] DomainsController]
				 withKeyPath:@"selection.properties.CanEditPassword" options:bindingOptions];	
*/



@end
