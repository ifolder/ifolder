/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Calvin Gaisford <cgaisford@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import "SyncLogWindowController.h"
#import "iFolderApplication.h"

@implementation SyncLogWindowController


static SyncLogWindowController *syncLogInstance = nil;

//===============================================================
// sharedInstance
// Get the shared instance of log window controller
//===============================================================
+ (SyncLogWindowController *)sharedInstance
{
	if(syncLogInstance == nil)
	{
		syncLogInstance = [[SyncLogWindowController alloc] initWithWindowNibName:@"LogWindow"];
	}

    return syncLogInstance;
}



//===============================================================
// windowWillClose
// Delegate method called before closing the window
//===============================================================
- (void)windowWillClose:(NSNotification *)aNotification
{
	if(syncLogInstance != nil)
	{
		[syncLogInstance release];
		syncLogInstance = nil;
		[[NSUserDefaults standardUserDefaults] setBool:NO forKey:STATE_SHOWLOGWINDOW];
	}
}



//===============================================================
// awakeFromNib
// Delegate method called before invoking Nib
//===============================================================
-(void)awakeFromNib
{
	[self setupToolbar];

	if([[NSUserDefaults standardUserDefaults] boolForKey:PREFKEY_WINPOS])
	{
		[super setShouldCascadeWindows:NO];
		[super setWindowFrameAutosaveName:@"ifolder_log_window"];
	}
	
    NSMutableDictionary *bindingOptions = [NSMutableDictionary dictionary];
    	
	// binding options for "name"
	[bindingOptions setObject:@"" forKey:@"NSNullPlaceholder"];

//    [accountsColumn bind:@"value" toObject:[[NSApp delegate] DomainsController]
//	  withKeyPath:@"arrangedObjects.properties.Name" options:bindingOptions];
	
	// bind the table column to the log to display it's contents
	[logColumn bind:@"value" toObject:[[NSApp delegate] logArrayController]
					withKeyPath:@"arrangedObjects" options:bindingOptions];	

	[[NSUserDefaults standardUserDefaults] setBool:YES forKey:STATE_SHOWLOGWINDOW];
}



//===============================================================
// dealloc
// Deallocate the resources previously allocated
//===============================================================
-(void)dealloc
{
	[toolbar release];
	[toolbarItems release];	
	[toolbarItemKeys release];	
    [super dealloc];
}



//===============================================================
// clearLog
// Clears the log messages from log window
//===============================================================
- (IBAction)clearLog:(id)sender
{
	[[NSApp delegate] clearLog];
}



//===============================================================
// saveLog
// Save the messages in the log
//===============================================================
- (IBAction)saveLog:(id)sender
{
	int result;
	NSSavePanel *sPanel = [NSSavePanel savePanel];
	
	result = [sPanel runModalForDirectory:NSHomeDirectory() file:@"iFolder Synchronization Log.txt"];
	
	if (result == NSOKButton)
	{
		FILE *fd;
		
		fd = fopen([[sPanel filename] cString], "w");
		
		if(fd != NULL)
		{
			NSArray *logArray;
			int logCounter;
			logArray = [[[NSApp delegate] logArrayController] arrangedObjects];
			
			for(logCounter=0; logCounter < [logArray count]; logCounter++)
			{
				size_t bytesWritten;
				size_t bytesToWrite;
			
				NSString *logEntry = 
					[NSString stringWithFormat:@"%@\n", [logArray objectAtIndex:logCounter] ];
				bytesToWrite = [logEntry cStringLength];
				
				bytesWritten = fwrite([logEntry cString], 1, bytesToWrite, fd);

				if(bytesToWrite != bytesWritten)
					break;
			}
			fclose(fd);
		}
		else
		{
			// Error opening file
			NSBeginAlertSheet(NSLocalizedString(@"Unable to save log to specified location", @"Log file save error"), 
			NSLocalizedString(@"OK", @"Log file save error"), nil, nil, 
			[self window], nil, nil, nil, nil, 
			NSLocalizedString(@"iFolder was unable to create a file in the location you specified.  The log file was not saved.", @"Log file save details"));
		}
	}
}



//===============================================================
// showHideToolbar
// Displays or hides the toolbar in the log window
//===============================================================
- (IBAction)showHideToolbar:(id)sender
{
	[toolbar setVisible:![toolbar isVisible]];
}



//===============================================================
// customizeToolbar
// Customize the tool bar items
//===============================================================
- (IBAction)customizeToolbar:(id)sender
{
	[toolbar runCustomizationPalette:sender];
}



// Toobar Delegates
//===============================================================
// toolbar
// Returns the toolbar items for the item identifier
//===============================================================
- (NSToolbarItem *)toolbar:(NSToolbar *)toolbar
	itemForItemIdentifier:(NSString *)itemIdentifier
	willBeInsertedIntoToolbar:(BOOL)flag
{
	return[toolbarItems objectForKey:itemIdentifier];
}

//===============================================================
// toolbarAllowedItemIdentifiers
// Returns the tool bar Item keys
//===============================================================
- (NSArray *)toolbarAllowedItemIdentifiers:(NSToolbar *)toolbar
{
	return toolbarItemKeys;
}

//===============================================================
// toolbarDefaultItemIdentifiers
// Get the default items in the tool bar
//===============================================================
- (NSArray *)toolbarDefaultItemIdentifiers:(NSToolbar *)toolbar
{
	return [toolbarItemKeys subarrayWithRange:NSMakeRange(0,2)];
}

//===============================================================
// setupToolbar
// Set up the items with their names, icons etc of the toolbar items
//===============================================================
- (void)setupToolbar
{
	toolbarItems =		[[NSMutableDictionary alloc] init];
	toolbarItemKeys =	[[NSMutableArray alloc] init];

	// New iFolder ToolbarItem
	NSToolbarItem *item=[[NSToolbarItem alloc] initWithItemIdentifier:@"Save"];
	[item setPaletteLabel:NSLocalizedString(@"Save Synchronization Log", @"LogWindow Toolbar Pallette button - save")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Save", @"LogWindow Toolbar button - save")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Save the syncrhonization log", @"LogWindow Toolbar tooltip - save")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(saveLog:)];
	[item setImage:[NSImage imageNamed:@"save32"]];
    [toolbarItems setObject:item forKey:@"SaveLog"]; // add to toolbar list
	[toolbarItemKeys addObject:@"SaveLog"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"Clear"];
	[item setPaletteLabel:NSLocalizedString(@"Clear Synchronization Log", @"LogWindow Toolbar Pallette button - clear")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Clear", @"LogWindow Toolbar button - clear")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Clear the synchronization log", @"LogWindow Toolbar tooltip - clear")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(clearLog:)];
	[item setImage:[NSImage imageNamed:@"clear32"]];
    [toolbarItems setObject:item forKey:@"ClearLog"]; // add to toolbar list
	[toolbarItemKeys addObject:@"ClearLog"];
	[item release];
	
	toolbar = [[NSToolbar alloc] initWithIdentifier:@"SyncLogToolbar"];
	[toolbar setDelegate:self];
	[toolbar setAllowsUserCustomization:NO];
	[toolbar setAutosavesConfiguration:NO];
	[[self window] setToolbar:toolbar];
}






@end
