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
#import "CreateiFolderSheetController.h"
#import "SetupiFolderSheetController.h"
#import "PropertiesWindowController.h"

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
	[super setWindowFrameAutosaveName:@"iFolderWindow"];

	webService = [[iFolderService alloc] init];
	keyedDomains = [[NSMutableDictionary alloc] init];
	keyediFolders = [[NSMutableDictionary alloc] init];
	

	[self addLog:@"initializing Simias Events"];

	[self initializeSimiasEvents];

	[self addLog:@"iFolder reading all domains"];

	@try
	{
		int domainCount;
		NSArray *newDomains = [webService GetDomains];

		for(domainCount = 0; domainCount < [newDomains count]; domainCount++)
		{
			[self addDomain:[newDomains objectAtIndex:domainCount]];
		}
//		[domainsController addObjects:newDomains];
		
		NSArray *newiFolders = [webService GetiFolders];
		if(newiFolders != nil)
		{
			[ifoldersController addObjects:newiFolders];
		}
	}
	@catch (NSException *e)
	{
		[self addLog:@"Reading domains failed with exception"];
	}
	
	// Setup the double click black magic
	[iFolderTable setDoubleAction:@selector(doubleClickedTable:)];
	

	// TODO: Show all of the windows that were open when quit last
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
	}
	@catch (NSException *e)
	{
		[self addLog:@"Refreshing failed with exception"];
	}
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




- (IBAction)newiFolder:(id)sender
{
	[createSheetController showWindow:self];
}




- (IBAction)setupiFolder:(id)sender
{
	// We don't have to tell the sheet anything about the iFolder because
	// it's all bound in the nib
	[setupiFolderController showWindow:sender];
}




- (IBAction)revertiFolder:(id)sender
{
	int selIndex = [ifoldersController selectionIndex];
	NSBeginAlertSheet(@"Revert iFolder", @"Yes", @"Cancel", nil,
		[self window], self, @selector(revertiFolderResponse:returnCode:contextInfo:), nil, (void *)selIndex, 
		@"Are you sure you want to revert the selected iFolder and make it a normal folder?");
}


- (void)revertiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	switch(returnCode)
	{
		case NSAlertDefaultReturn:		// Revert iFolder
			NSLog(@"Reverting iFolder at index %d", (int)contextInfo);
			break;
	}
}


- (IBAction)deleteiFolder:(id)sender
{
	int selIndex = [ifoldersController selectionIndex];
	NSBeginAlertSheet(@"Delete iFolder", @"Yes", @"Cancel", nil,
		[self window], self, @selector(deleteiFolderResponse:returnCode:contextInfo:), nil, (void *)selIndex, 
		@"Are you sure you want to delete the selected iFolder?");
}


- (void)deleteiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	switch(returnCode)
	{
		case NSAlertDefaultReturn:		// Revert iFolder
		{
			iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:(int)contextInfo];

			NSLog(@"Deleting iFolder at index %@", [ifolder Name]);

			[self addLog:[NSString stringWithFormat:@"Deleting iFolder at index %@", [ifolder Name]]];

			@try
			{
				[webService DeleteiFolder:[ifolder ID]];
				[ifoldersController removeObjectAtArrangedObjectIndex:(int)contextInfo];
			}
			@catch (NSException *e)
			{
				NSRunAlertPanel(@"Error deleting iFolder", [e name], @"OK",nil, nil);
			}
			break;
		}
	}
}




- (IBAction)openiFolder:(id)sender
{
	int selIndex = [ifoldersController selectionIndex];
	
	iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:selIndex];
	NSString *path = [ifolder Path];
	if(	([path length] > 0) &&
		([[ifolder IsSubscription] boolValue] == NO) )
		[[NSWorkspace sharedWorkspace] openFile:path];
	else
		[self setupiFolder:sender];
}




- (IBAction)showProperties:(id)sender
{
	if(propertiesController == nil)
	{
		propertiesController = [[PropertiesWindowController alloc] initWithWindowNibName:@"Properties"];
	}
	
	[propertiesController showWindow:self];
}




- (void)showLoginWindow:(NSString *)domainID
{
	if(loginController == nil)
	{
		loginController = [[LoginWindowController alloc] initWithWindowNibName:@"LoginWindow"];
	}

	iFolderDomain *dom = [keyedDomains objectForKey:domainID];
	
	[[loginController window] center];
	[loginController showLoginWindow:self withHost:[dom Host] withDomain:domainID];
}





- (IBAction)shareiFolder:(id)sender
{
}




- (void)doubleClickedTable:(id)sender
{
	[self openiFolder:sender];
}




- (BOOL)connectToDomain:(iFolderDomain *)domain
{
	@try
	{
		iFolderDomain *newDomain = [webService ConnectToDomain:[domain UserName] 
			usingPassword:[domain Password] andHost:[domain Host]];

		[self addDomain:newDomain];
		[self refreshWindow:self];

		NSDictionary *newProps = [newDomain properties];		

		[domain setProperties:newProps];
		return YES;
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		NSRunAlertPanel(@"Login Error", [e name], @"OK",nil, nil);
		return NO;
	}
}



- (BOOL)authenticateToDomain:(NSString *)domainID withPassword:(NSString *)password
{
	@try
	{
		[webService AuthenticateToDomain:domainID usingPassword:password];
		return YES;
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		NSRunAlertPanel(@"Login Error", [e name], @"OK",nil, nil);
		return NO;
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




- (void)acceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath
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
	[keyedDomains setObject:newDomain forKey:[newDomain ID] ];
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




- (BOOL)validateUserInterfaceItem:(id)anItem
{
	SEL action = [anItem action];
	
	if(action == @selector(showLoginWindow:))
	{
		return YES;
	}
	else if(action == @selector(newiFolder:))
	{
		return YES;
	}
	else if(action == @selector(setupiFolder:))
	{
		if ([ifoldersController selectionIndex] != NSNotFound)
		{
			if([[[ifoldersController selection] 
				valueForKeyPath:@"properties.IsSubscription"] boolValue] == YES)
				return YES;
		}
		return NO;
	}
	else if(	(action == @selector(deleteiFolder:)) ||
				(action == @selector(openiFolder:)) ||
				(action == @selector(showProperties:)) ||
				(action == @selector(shareiFolder:)) )
	{
		if ([ifoldersController selectionIndex] != NSNotFound)
		{
			if([[[ifoldersController selection] 
				valueForKeyPath:@"properties.IsSubscription"] boolValue] == NO)
				return YES;
		}
		return NO;
	}
	else if(action == @selector(revertiFolder:))
	{
		if ([ifoldersController selectionIndex] != NSNotFound)
		{
			if( ([[[ifoldersController selection] valueForKeyPath:@"properties.IsSubscription"] boolValue] == NO) &&
				([[[ifoldersController selection] valueForKeyPath:@"properties.IsWorkgroup"] boolValue] == NO) )
				return YES;
		}
		return NO;
	}

	
	return YES;
}





// Toobar Delegates
- (NSToolbarItem *)toolbar:(NSToolbar *)toolbar
	itemForItemIdentifier:(NSString *)itemIdentifier
	willBeInsertedIntoToolbar:(BOOL)flag
{
	return[toolbarItems objectForKey:itemIdentifier];
}


- (NSArray *)toolbarAllowedItemIdentifiers:(NSToolbar *)toolbar
{
	return toolbarItemKeys;
}


- (NSArray *)toolbarDefaultItemIdentifiers:(NSToolbar *)toolbar
{
	return [toolbarItemKeys subarrayWithRange:NSMakeRange(0,13)];
}


- (void)setupToolbar
{
	toolbarItems =		[[NSMutableDictionary alloc] init];
	toolbarItemKeys =	[[NSMutableArray alloc] init];

	// New iFolder ToolbarItem
	NSToolbarItem *item=[[NSToolbarItem alloc] initWithItemIdentifier:@"NewiFolder"];
	[item setPaletteLabel:@"Create a new iFolder"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"New"]; // name for the item in the toolbar
	[item setToolTip:@"Create a new iFolder"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(newiFolder:)];
	[item setImage:[NSImage imageNamed:@"ifolder-new"]];
    [toolbarItems setObject:item forKey:@"NewiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"NewiFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"SetupiFolder"];
	[item setPaletteLabel:@"Setup iFolder"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Setup"]; // name for the item in the toolbar
	[item setToolTip:@"Setup a shared iFolder"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(setupiFolder:)];
	[item setImage:[NSImage imageNamed:@"prefs-general"]];
    [toolbarItems setObject:item forKey:@"SetupiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"SetupiFolder"];
	[item release];
	
	item=[[NSToolbarItem alloc] initWithItemIdentifier:NSToolbarSpaceItemIdentifier];
	[toolbarItems setObject:item forKey:NSToolbarSpaceItemIdentifier];
	[toolbarItemKeys addObject:NSToolbarSpaceItemIdentifier];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"RevertiFolder"];
	[item setPaletteLabel:@"Revert iFolder"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Revert"]; // name for the item in the toolbar
	[item setToolTip:@"Revert to a normal folder"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(revertiFolder:)];
	[item setImage:[NSImage imageNamed:@"ifolderonserver24"]];
    [toolbarItems setObject:item forKey:@"RevertiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"RevertiFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"DeleteiFolder"];
	[item setPaletteLabel:@"Delete iFolder"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Delete"]; // name for the item in the toolbar
	[item setToolTip:@"Delete an iFolder"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(deleteiFolder:)];
	[item setImage:[NSImage imageNamed:@"prefs-general"]];
    [toolbarItems setObject:item forKey:@"DeleteiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"DeleteiFolder"];
	[item release];

	[toolbarItemKeys addObject:NSToolbarSpaceItemIdentifier];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"OpeniFolder"];
	[item setPaletteLabel:@"Open iFolder"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Open"]; // name for the item in the toolbar
	[item setToolTip:@"Open an iFolder in Finder"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(openiFolder:)];
	[item setImage:[NSImage imageNamed:@"prefs-general"]];
    [toolbarItems setObject:item forKey:@"OpeniFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"OpeniFolder"];
	[item release];


	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"iFolderProperties"];
	[item setPaletteLabel:@"iFolder Properties"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Properties"]; // name for the item in the toolbar
	[item setToolTip:@"Open properties of an iFolder"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(showProperties:)];
	[item setImage:[NSImage imageNamed:@"prefs-general"]];
    [toolbarItems setObject:item forKey:@"iFolderProperties"]; // add to toolbar list
	[toolbarItemKeys addObject:@"iFolderProperties"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"ShareiFolder"];
	[item setPaletteLabel:@"Share an iFolder"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Share"]; // name for the item in the toolbar
	[item setToolTip:@"Share an iFolder"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(shareiFolder:)];
	[item setImage:[NSImage imageNamed:@"prefs-general"]];
    [toolbarItems setObject:item forKey:@"ShareiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"ShareiFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:NSToolbarFlexibleSpaceItemIdentifier];
	[toolbarItems setObject:item forKey:NSToolbarFlexibleSpaceItemIdentifier];
	[toolbarItemKeys addObject:NSToolbarFlexibleSpaceItemIdentifier];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"RefreshiFolders"];
	[item setPaletteLabel:@"Refresh iFolders"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Refresh"]; // name for the item in the toolbar
	[item setToolTip:@"Refresh iFolders"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(refreshWindow:)];
	[item setImage:[NSImage imageNamed:@"prefs-general"]];
    [toolbarItems setObject:item forKey:@"RefreshiFolders"]; // add to toolbar list
	[toolbarItemKeys addObject:@"RefreshiFolders"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"Preferences"];
	[item setPaletteLabel:@"Preferences"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Preferences"]; // name for the item in the toolbar
	[item setToolTip:@"Preferences"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(showPrefs:)];
	[item setImage:[NSImage imageNamed:@"prefs-general"]];
    [toolbarItems setObject:item forKey:@"Preferences"]; // add to toolbar list
	[toolbarItemKeys addObject:@"Preferences"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:NSToolbarCustomizeToolbarItemIdentifier];
	[toolbarItems setObject:item forKey:NSToolbarCustomizeToolbarItemIdentifier];
	[toolbarItemKeys addObject:NSToolbarCustomizeToolbarItemIdentifier];
	[item release];

	toolbar = [[NSToolbar alloc] initWithIdentifier:@"iFolderToolbar"];
	[toolbar setDelegate:self];
	[toolbar setAllowsUserCustomization:YES];
	[toolbar setAutosavesConfiguration:YES];
	[[self window] setToolbar:toolbar];
}


@end
