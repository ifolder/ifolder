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

	ifolderService = [[iFolderService alloc] init];
	simiasService = [[SimiasService alloc] init];

	keyedDomains = [[NSMutableDictionary alloc] init];
	keyediFolders = [[NSMutableDictionary alloc] init];
	

	[self addLog:@"initializing Simias Events"];

	[self initializeSimiasEvents];

	[self addLog:@"iFolder reading all domains"];

	@try
	{
		int domainCount;
		NSArray *newDomains = [simiasService GetDomains:NO];

		for(domainCount = 0; domainCount < [newDomains count]; domainCount++)
		{
			iFolderDomain *newDomain = [newDomains objectAtIndex:domainCount];
			
			if( [[newDomain isDefault] boolValue] )
				defaultDomain = newDomain;

			[self addDomain:newDomain];
		}
//		[domainsController addObjects:newDomains];
		
		NSArray *newiFolders = [ifolderService GetiFolders];
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
		NSArray *newiFolders = [ifolderService GetiFolders];
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
	
	[createSheetController setSelectedDomain:defaultDomain];
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



- (IBAction)synciFolder:(id)sender
{
	int selIndex = [ifoldersController selectionIndex];
	iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:selIndex];
	@try
	{
		[ifolderService SynciFolderNow:[ifolder ID]];
	}
	@catch (NSException *e)
	{
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
				[ifolderService DeleteiFolder:[ifolder ID]];
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
	[loginController showLoginWindow:self withHost:[dom host] withDomain:domainID];
}





- (IBAction)shareiFolder:(id)sender
{
}




- (void)doubleClickedTable:(id)sender
{
	[self openiFolder:sender];
}





- (BOOL)authenticateToDomain:(NSString *)domainID withPassword:(NSString *)password
{
	@try
	{
		[simiasService LoginToRemoteDomain:domainID usingPassword:password];
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
		iFolder *newiFolder = [ifolderService CreateiFolder:path InDomain:domainID];
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
		iFolder *newiFolder = [ifolderService AcceptiFolderInvitation:iFolderID InDomain:domainID toPath:localPath];
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
				(action == @selector(shareiFolder:)) ||
				(action == @selector(synciFolderNow:)) )
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



- (NSArrayController *)DomainsController
{
	return domainsController;
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
	return [toolbarItemKeys subarrayWithRange:NSMakeRange(0,8)];
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
	[item setImage:[NSImage imageNamed:@"newifolder24"]];
    [toolbarItems setObject:item forKey:@"NewiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"NewiFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"SetupiFolder"];
	[item setPaletteLabel:@"Setup iFolder"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Setup"]; // name for the item in the toolbar
	[item setToolTip:@"Setup a shared iFolder"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(setupiFolder:)];
	[item setImage:[NSImage imageNamed:@"setup24"]];
    [toolbarItems setObject:item forKey:@"SetupiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"SetupiFolder"];
	[item release];
	
	item=[[NSToolbarItem alloc] initWithItemIdentifier:NSToolbarSpaceItemIdentifier];
	[toolbarItems setObject:item forKey:NSToolbarSpaceItemIdentifier];
	[toolbarItemKeys addObject:NSToolbarSpaceItemIdentifier];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"SynciFolder"];
	[item setPaletteLabel:@"Sync iFolder"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Sync"]; // name for the item in the toolbar
	[item setToolTip:@"Sync selected iFolder now"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(synciFolder:)];
	[item setImage:[NSImage imageNamed:@"sync24"]];
    [toolbarItems setObject:item forKey:@"SynciFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"SynciFolder"];
	[item release];
	

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"ShareiFolder"];
	[item setPaletteLabel:@"Share an iFolder"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Share"]; // name for the item in the toolbar
	[item setToolTip:@"Share an iFolder"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(shareiFolder:)];
	[item setImage:[NSImage imageNamed:@"share24"]];
    [toolbarItems setObject:item forKey:@"ShareiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"ShareiFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"ResolveConflicts"];
	[item setPaletteLabel:@"Resolve Conflicts"]; // name for the "Customize Toolbar" sheet
	[item setLabel:@"Resolve"]; // name for the item in the toolbar
	[item setToolTip:@"Resolve file conflicts"]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(resolveConflicts:)];
	[item setImage:[NSImage imageNamed:@"conflict24"]];
    [toolbarItems setObject:item forKey:@"ResolveConflicts"]; // add to toolbar list
	[toolbarItemKeys addObject:@"ResolveConflicts"];
	[item release];


	item=[[NSToolbarItem alloc] initWithItemIdentifier:NSToolbarFlexibleSpaceItemIdentifier];
	[toolbarItems setObject:item forKey:NSToolbarFlexibleSpaceItemIdentifier];
	[toolbarItemKeys addObject:NSToolbarFlexibleSpaceItemIdentifier];
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



/*
static MyObject *sharedInstance = nil;

+ (MyObject *)sharedController
{
return sharedInstance;
}

- (void)awakeFromNib
{
sharedInstance = self;
// etc...
}
*/

@end
