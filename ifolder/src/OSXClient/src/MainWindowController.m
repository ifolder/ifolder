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

#import "MainWindowController.h"
#import "SyncLogWindowController.h"
#import "LoginWindowController.h"
#import "iFolderPrefsController.h"
#include "SimiasEventHandlers.h"



@implementation MainWindowController


-(id)init
{
    [super init];
    return self;
}




- (void)dealloc
{
	[toolbar release];
	[toolbarItems release];	
	[toolbarItemKeys release];	
    [super dealloc];
}




-(void)awakeFromNib
{
	[self setupToolbar];

	[super setShouldCascadeWindows:NO];
	[super setWindowFrameAutosaveName:@"ifolder_window"];

/*
    int i;
	
	items = [[NSMutableDictionary alloc] init];

    for(i=0;i<50;i++)
	{
        NSString *name=[[NSString alloc] initWithFormat:@"Item %d",i];
        NSToolbarItem *item=[[NSToolbarItem alloc] initWithItemIdentifier:name];

        [item setPaletteLabel:name]; // name for the "Customize Toolbar" sheet
        [item setLabel:name]; // name for the item in the toolbar
        [item setToolTip:[NSString stringWithFormat:@"This is item %d",i]]; // tooltip
        [item setTarget:self]; // what should happen when it's clicked
        [item setAction:@selector(toolbaritemclicked:)];
		NSImage *icon = [NSImage imageNamed:@"ifolder-new"];
		[icon setScalesWhenResized:YES];
		[item setImage:icon];
        
        [items setObject:item forKey:name]; // add to toolbar list

        [item release];
        [name release];
    }


	
	toolbar = [[NSToolbar alloc] initWithIdentifier:@"iFolderToolbar"];
	[toolbar setDelegate:self];
	[toolbar setAllowsUserCustomization:YES];
	[toolbar setAutosavesConfiguration:YES];
	[[self window] setToolbar:toolbar];
*/	

	webService = [[iFolderService alloc] init];

	[self addLog:@"initializing Simias Events"];

	[self initializeSimiasEvents];

	[self addLog:@"iFolder reading all domains"];

	@try
	{
		NSArray *newDomains = [webService GetDomains];

		[domainsController addObjects:newDomains];
		
		NSArray *newiFolders = [webService GetiFolders];
		if(newiFolders != nil)
		{
			[ifoldersController addObjects:newiFolders];
		}

		// if we have less than two domains, we don't have enterprise
		// so we better ask the user to login
/*
		if([newDomains count] < 2)
			[self showLoginWindow:self];
		else
		{
			[self showWindow:self];
		}
*/
	}
	@catch (NSException *e)
	{
		[self addLog:@"Reading domains failed with exception"];
	}

	// Show the main iFolder window
	// fix this so it uses prefs from last time run
	[self showWindow:self];
}



- (IBAction)refreshWindow:(id)sender
{
	[self addLog:@"Refreshing iFolder view"];

	@try
	{
		NSArray *newiFolders = [webService GetiFolders];
		if(newiFolders != nil)
		{
			[ifoldersController setContent:newiFolders];
		}

		// if we have less than two domains, we don't have enterprise
		// so we better ask the user to login
//		if([newDomains count] < 2)
//			[self showLoginWindow:self];
//		else
	}
	@catch (NSException *e)
	{
		[self addLog:@"Refreshing failed with exception"];
	}
}




- (IBAction)showLoginWindow:(id)sender
{
	if(loginController == nil)
	{
		loginController = [[LoginWindowController alloc] initWithWindowNibName:@"LoginWindow"];
	}
	
	[[loginController window] center];
	[loginController showWindow:self];
}


- (IBAction)showPrefs:(id)sender
{
	if(prefsController == nil)
	{
		prefsController = [[iFolderPrefsController alloc] initWithWindowNibName:@"Preferences"];
	}
	
	[prefsController showWindow:self];
}



- (IBAction)showSyncLog:(id)sender
{
	if(syncLogController == nil)
	{
		syncLogController = [[SyncLogWindowController alloc] initWithWindowNibName:@"SyncLogWindow"];
	}
	
	[syncLogController showWindow:self];
}



- (IBAction)showHideToolbar:(id)sender
{
	[toolbar setVisible:![toolbar isVisible]];
}



- (IBAction)customizeToolbar:(id)sender
{
	[toolbar runCustomizationPalette:sender];
}



-(void)login:(NSString *)username withPassword:(NSString *)password toServer:(NSString *)server
{
	@try
	{
		iFolderDomain *domain = [webService ConnectToDomain:username usingPassword:password andHost:server];
		[domainsController addObject:domain];
		[self refreshWindow:self];
		[self showWindow:self];

	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		NSRunAlertPanel(@"Login Error", [e name], @"OK",nil, nil);
	}

}




- (void)createiFolder:(NSString *)path inDomain:(NSString *)domainID
{
	@try
	{
		iFolder *newiFolder = [webService CreateiFolder:path InDomain:domainID];
		[ifoldersController addObject:newiFolder];
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		NSRunAlertPanel(@"Error creating iFolder", [e name], @"OK",nil, nil);
	}
}




- (void)AcceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath
{
	@try
	{
		iFolder *newiFolder = [webService AcceptiFolderInvitation:iFolderID InDomain:domainID toPath:localPath];
		[ifoldersController addObject:newiFolder];
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		NSRunAlertPanel(@"Error connecting to Server", [e name], @"OK",nil, nil);
	}
}




- (void)addDomain:(iFolderDomain *)newDomain
{
	[domainsController addObject:newDomain];
}




- (void)addiFolder:(iFolder *)newiFolder
{
	[ifoldersController addObject:newiFolder];
}



- (void)addLog:(NSString *)entry
{
	if(syncLogController != nil)
	{
		[syncLogController logEntry:entry];
	}
}

- (void)initializeSimiasEvents
{
	SimiasEventInitialize();
}


// Toobar Delegates
- (NSToolbarItem *)toolbar:(NSToolbar *)toolbar
	itemForItemIdentifier:(NSString *)itemIdentifier
	willBeInsertedIntoToolbar:(BOOL)flag
{
	return[toolbarItems objectForKey:itemIdentifier];
}


- (int)count
{
	return [toolbarItems count];
}


- (NSArray *)toolbarAllowedItemIdentifiers:(NSToolbar *)toolbar
{
	return toolbarItemKeys;
}




- (NSArray *)toolbarDefaultItemIdentifiers:(NSToolbar *)toolbar
{
	return [toolbarItemKeys subarrayWithRange:NSMakeRange(0,2)];
}




- (void)toolbaritemclicked:(NSToolbarItem *)item
{
	NSLog(@"Click %@!", [item label]);
}


- (BOOL)validateUserInterfaceItem:(id)anItem
{
	SEL action = [anItem action];
	
	if(action == @selector(showLoginWindow:))
	{
		return YES;
	}
	
	return YES;
}



- (void)setupToolbar
{
	toolbarItems =		[[NSMutableDictionary alloc] init];
	toolbarItemKeys =	[[NSMutableArray alloc] init];

	// New iFolder ToolbarItem
	NSToolbarItem *item=[[NSToolbarItem alloc] initWithItemIdentifier:@"New iFolder"];
	[item setPaletteLabel:@"Create a new iFolder"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"New iFolder"]; // name for the item in the toolbar
	[item setToolTip:@"Create a new iFolder"]; // tooltip
    [item setTarget:createSheetController]; // what should happen when it's clicked
    [item setAction:@selector(showWindow:)];
	[item setImage:[NSImage imageNamed:@"ifolder-new"]];
    [toolbarItems setObject:item forKey:@"New iFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"New iFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"Revert iFolder"];
	[item setPaletteLabel:@"Revert to normal folder"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Revert iFolder"]; // name for the item in the toolbar
	[item setToolTip:@"Revert to a normal folder"]; // tooltip
    [item setTarget:createSheetController]; // what should happen when it's clicked
    [item setAction:@selector(showWindow:)];
	[item setImage:[NSImage imageNamed:@"ifolderonserver24"]];
    [toolbarItems setObject:item forKey:@"Revert iFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"Revert iFolder"];
	[item release];

	toolbar = [[NSToolbar alloc] initWithIdentifier:@"iFolderToolbar"];
	[toolbar setDelegate:self];
	[toolbar setAllowsUserCustomization:YES];
	[toolbar setAutosavesConfiguration:YES];
	[[self window] setToolbar:toolbar];
}

@end
