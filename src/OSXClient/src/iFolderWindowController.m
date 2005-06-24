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

#import "iFolderWindowController.h"
#import "iFolderApplication.h"
#import "CreateiFolderSheetController.h"
#import "SetupiFolderSheetController.h"
#import "PropertiesWindowController.h"
#import "ConflictWindowController.h"
#import "iFolder.h"
#import "iFolderDomain.h"
#import "iFolderData.h"
#import "MCTableView.h"


@implementation iFolderWindowController


static iFolderWindowController *sharedInstance = nil;


+ (iFolderWindowController *)sharedInstance
{
	if(sharedInstance == nil)
	{
		sharedInstance = [[iFolderWindowController alloc] initWithWindowNibName:@"iFolderWindow"];
	}

    return sharedInstance;
}




+(void)updateStatusTS:(NSString *)message
{
	if(sharedInstance != nil)
	{
		[sharedInstance performSelectorOnMainThread:@selector(updateStatus:) 
					withObject:message waitUntilDone:NO ];		
	}
}




-(void)updateStatus:(NSString *)message
{
	[statusText setStringValue:message];
}


+(void)updateProgress:(double)curVal withMin:(double)minVal withMax:(double)maxVal
{
	if(sharedInstance != nil)
	{
		[sharedInstance updateProgress:curVal withMin:minVal withMax:maxVal];
	}
}


-(void)updateProgress:(double)curVal withMin:(double)minVal withMax:(double)maxVal
{
	if(curVal == -1)
		[statusProgress setHidden:YES];
	else
	{
		[statusProgress setHidden:NO];
		[statusProgress setMinValue:minVal];
		[statusProgress setMaxValue:maxVal];
		[statusProgress setDoubleValue:curVal];
	}
}




- (void)windowWillClose:(NSNotification *)aNotification
{
	if(sharedInstance != nil)
	{
		[sharedInstance release];
		sharedInstance = nil;
		[[NSUserDefaults standardUserDefaults] setBool:NO forKey:STATE_SHOWMAINWINDOW];		
	}
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

	if([[NSUserDefaults standardUserDefaults] boolForKey:PREFKEY_WINPOS])
	{
		[super setShouldCascadeWindows:NO];
		[super setWindowFrameAutosaveName:@"iFolderWindow"];
	}

	ifolderService = [[iFolderService alloc] init];
	simiasService = [[SimiasService alloc] init];

	ifoldersController = [[iFolderData sharedInstance] ifolderArrayController];

    NSMutableDictionary *bindingOptions = [NSMutableDictionary dictionary];
    	
	// binding options for "name"
	[bindingOptions setObject:@"" forKey:@"NSNullPlaceholder"];

	// bind the table column to the log to display it's contents

	[iconColumn bind:@"value" toObject:ifoldersController
					withKeyPath:@"arrangedObjects.properties.Image" options:bindingOptions];
	[nameColumn bind:@"value" toObject:ifoldersController
					withKeyPath:@"arrangedObjects.properties.Name" options:bindingOptions];	
	[locationColumn bind:@"value" toObject:ifoldersController
					withKeyPath:@"arrangedObjects.properties.Location" options:bindingOptions];	
	[statusColumn bind:@"value" toObject:ifoldersController
					withKeyPath:@"arrangedObjects.properties.Status" options:bindingOptions];	

	// Setup the double click black magic
	[iFolderTable setDoubleAction:@selector(doubleClickedTable:)];
	
	[[NSUserDefaults standardUserDefaults] setBool:YES forKey:STATE_SHOWMAINWINDOW];		
}




- (IBAction)refreshWindow:(id)sender
{
	int domainCount;
	[[NSApp delegate] addLog:NSLocalizedString(@"Refreshing iFolder view", @"Log Message when refreshing main window")];

	if([[NSApp delegate] simiasIsRunning])
		[[iFolderData sharedInstance] refresh:NO];

	// Get all of the domains and refresh their POBoxes
	NSArray *domains = [[iFolderData sharedInstance] getDomains];
	
	for(domainCount = 0; domainCount < [domains count]; domainCount++)
	{
		iFolderDomain *dom = [domains objectAtIndex:domainCount];
		
		if(dom != nil)
		{
			@try
			{
				NSLog(@"Calling to refresh on poBox %@", [dom poBoxID]);
				[ifolderService SynciFolderNow:[dom poBoxID]];
			}
			@catch (NSException *e)
			{
			}
		}
	}
	
	// calling refresh on iFolderData calls refreshDomains
//	[self refreshDomains];

//	NSArray *newiFolders = [[iFolderData sharedInstance] getiFolders];
//	if(newiFolders != nil)
//	{
//		[ifoldersController setContent:newiFolders];
//	}
}



+(void)refreshDomainsTS
{
	if(sharedInstance != nil)
	{
		[sharedInstance performSelectorOnMainThread:@selector(refreshDomains:) 
					withObject:self waitUntilDone:YES ];
	}
}
-(void)refreshDomains:(id)args
{
/*
	NSArray *newDomains = [[iFolderData sharedInstance] getDomains];
	if(newDomains != nil)
	{
		[domainsController setContent:newDomains];
	}
*/
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
	if([[iFolderData sharedInstance] getDomainCount] < 1)
	{
		NSBeginAlertSheet(NSLocalizedString(@"Set Up iFolder Account", @"Error dialog Title new iFolder"), NSLocalizedString(@"OK", @"Error dialog button new iFolder"), nil, nil,
			[self window], self, nil, nil, NULL, 
			NSLocalizedString(@"To begin using iFolder, you must first set up an iFolder account.", @"Error dialog message new iFolder"));
	}
	else
	{
		[[iFolderData sharedInstance] selectDefaultDomain];
		[createSheetController showWindow:self];
	}
}




- (IBAction)setupiFolder:(id)sender
{
	// We don't have to tell the sheet anything about the iFolder because
	// it's all bound in the nib
	[setupSheetController showWindow:sender];
}




- (IBAction)revertiFolder:(id)sender
{
	int selIndex = [ifoldersController selectionIndex];
	NSBeginAlertSheet(NSLocalizedString(@"Revert iFolder Confirmation", @"Revert iFolder Dialog Title"), NSLocalizedString(@"Yes", @"Revert iFolder Dialog button"), NSLocalizedString(@"Cancel", @"Revert iFolder Dialog button"), nil,
		[self window], self, @selector(revertiFolderResponse:returnCode:contextInfo:), nil, (void *)selIndex, 
		NSLocalizedString(@"This reverts the iFolder back to a normal folder and leaves the files intact.  The iFolder is then available from the server and must be set up in a different location to synchronize.  Are you sure you want to revert the iFolder to a normal folder?", @"Revert iFolder Dialog message"));
}


- (void)revertiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	switch(returnCode)
	{
		case NSAlertDefaultReturn:		// Revert iFolder
		{
			iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:(int)contextInfo];

			@try
			{
				[[iFolderData sharedInstance] revertiFolder:[ifolder ID]];		
			}
			@catch (NSException *e)
			{
				NSRunAlertPanel(NSLocalizedString(@"iFolder Revert Error", @"Revert error Dialog title"),  NSLocalizedString(@"An error was encountered while reverting the iFolder.", @"Revert error Dialog message"), NSLocalizedString(@"OK", @"Revert error Dialog button"),nil, nil);
			}
			break;
		}
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
	iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:selIndex];
	if([[ifolder OwnerUserID] compare:[ifolder CurrentUserID]] == 0)
	{
		NSBeginAlertSheet(NSLocalizedString(@"Delete iFolder", @"Delete iFolder Dialog Title"), NSLocalizedString(@"Yes", @"Delete iFolder Dialog button"), 
			NSLocalizedString(@"Cancel", @"Delete iFolder Dialog button"), nil,
			[self window], self, @selector(deleteiFolderResponse:returnCode:contextInfo:), nil, (void *)selIndex, 
			NSLocalizedString(@"This removes the iFolder from your local computer.  Because you are the owner, the iFolder is deleted from the server and all member computers.  The iFolder cannot be recovered or re-shared on another computer.  The files are not deleted from your local hard drive.  Are you sure you want to delete the iFolder?", @"Delete iFolder Dialog message"));
	}
	else
	{
		NSBeginAlertSheet(NSLocalizedString(@"Delete iFolder", @"Delete iFolder Dialog Title"), NSLocalizedString(@"Yes", @"Delete iFolder Dialog button"), 
			NSLocalizedString(@"Cancel", @"Delete iFolder Dialog button"), nil,
			[self window], self, @selector(deleteiFolderResponse:returnCode:contextInfo:), nil, (void *)selIndex, 
			NSLocalizedString(@"This removes you as a member of the iFolder.  You cannot access the iFolder unless the owner re-invites you.  The files are not deleted from your local hard drive.  Are you sure you want to delete the iFolder?", @"Delete iFolder Dialog message"));
	}
}



- (IBAction)resolveConflicts:(id)sender
{
	[[ConflictWindowController sharedInstance] showWindow:self];
}



- (void)deleteiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	switch(returnCode)
	{
		case NSAlertDefaultReturn:		// Revert iFolder
		{
			iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:(int)contextInfo];

			@try
			{
				[[iFolderData sharedInstance] deleteiFolder:[ifolder ID]];
//				[ifolderService DeleteiFolder:[ifolder ID]];
//				[ifoldersController removeObject:ifolder];
			}
			@catch (NSException *e)
			{
				NSRunAlertPanel(NSLocalizedString(@"iFolder Deletion Error", @"iFolder delete error dialog title"), NSLocalizedString(@"An error was encountered while deleting the iFolder.", @"iFolder delete error dialog message"), NSLocalizedString(@"OK", @"iFolder delete error dialog button"),nil, nil);
			}
			break;
		}
	}
}




- (IBAction)openiFolder:(id)sender
{
	int selIndex = [ifoldersController selectionIndex];

	if(selIndex != NSNotFound)
	{
		iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:selIndex];
		NSString *path = [ifolder Path];

		if(	([path length] > 0) &&
			([ifolder IsSubscription] == NO) )
		{
			if([[NSUserDefaults standardUserDefaults] integerForKey:PREFKEY_CLICKIFOLDER] == 0)
				[[NSWorkspace sharedWorkspace] openFile:path];
			else
				[self showProperties:self];
		}
		else
			[self setupiFolder:sender];
	}
}




- (IBAction)shareiFolder:(id)sender
{
	[[PropertiesWindowController sharedInstance] setSharingTab];
	[[PropertiesWindowController sharedInstance] showWindow:self];
}




- (IBAction)showProperties:(id)sender
{
	[[PropertiesWindowController sharedInstance] setGeneralTab];
	[[PropertiesWindowController sharedInstance] showWindow:self];
}




- (void)doubleClickedTable:(id)sender
{
	[self openiFolder:sender];
}





- (void)createiFolder:(NSString *)path inDomain:(NSString *)domainID
{
	@try
	{
		[[iFolderData sharedInstance] createiFolder:path inDomain:domainID];
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		NSRunAlertPanel(NSLocalizedString(@"Error creating iFolder", @"iFolder create error dialog title"), [e name], NSLocalizedString(@"OK", @"iFolder create error dialog button"),nil, nil);
	}
}





- (void)acceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath
{
	@try
	{
		[[iFolderData sharedInstance] 
				acceptiFolderInvitation:iFolderID
				InDomain:domainID
				toPath:localPath];
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		NSRunAlertPanel(NSLocalizedString(@"Error setting up iFolder", @"iFolder setup error dialog title"), [e name], NSLocalizedString(@"OK", @"iFolder setup error dialog button"),nil, nil);
	}
}




- (void)addDomain:(iFolderDomain *)newDomain
{
//	[domainsController addObject:newDomain];
//	[keyedDomains setObject:newDomain forKey:[newDomain ID] ];
}





- (BOOL)validateUserInterfaceItem:(id)anItem
{
	SEL action = [anItem action];

	if(action == @selector(newiFolder:))
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;

		return YES;
	}
	else if(action == @selector(setupiFolder:))
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;

		int selIndex = [ifoldersController selectionIndex];

		if (selIndex != NSNotFound)
		{
			if([[[ifoldersController arrangedObjects] objectAtIndex:selIndex]
						IsSubscription] == YES)
				return YES;
		}
		return NO;
	}
	else if(action == @selector(deleteiFolder:))
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;

		int selIndex = [ifoldersController selectionIndex];

		if (selIndex != NSNotFound)
		{
			return YES;
		}
		return NO;
	}
	else if(	(action == @selector(openiFolder:)) ||
				(action == @selector(showProperties:)) ||
				(action == @selector(shareiFolder:)) ||
				(action == @selector(synciFolder:)) )
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;

		int selIndex = [ifoldersController selectionIndex];

		if (selIndex != NSNotFound)
		{
			if([[[ifoldersController arrangedObjects] objectAtIndex:selIndex]
						IsSubscription] == NO)
				return YES;
		}
		return NO;
	}
	else if(action == @selector(revertiFolder:))
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;

		int selIndex = [ifoldersController selectionIndex];

		if (selIndex != NSNotFound)
		{
			if( ([[[ifoldersController arrangedObjects] objectAtIndex:selIndex] IsSubscription] == NO) &&
				([[[[ifoldersController arrangedObjects] objectAtIndex:selIndex] valueForKeyPath:@"properties.IsWorkgroup"] boolValue] == NO) &&
				([[[[ifoldersController arrangedObjects] objectAtIndex:selIndex] Role] compare:@"Master"] != 0) )
				return YES;
		}
		return NO;
	}
	else if(action == @selector(resolveConflicts:))
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;

		int selIndex = [ifoldersController selectionIndex];

		if (selIndex != NSNotFound)
		{
			if([[[ifoldersController arrangedObjects] objectAtIndex:selIndex] HasConflicts] == YES)
				return YES;
		}
		return NO;
	}
	
	return YES;
}


-(iFolder *)selectediFolder
{
	int selIndex = [ifoldersController selectionIndex];
	return [[ifoldersController arrangedObjects] objectAtIndex:selIndex];
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
	[item setPaletteLabel:NSLocalizedString(@"Create a new iFolder", @"iFolderWin Toolbar New Pallette Button")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"New", @"iFolderWin Toolbar New Selector Button")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Create a new iFolder", @"iFolderWin Toolbar New ToolTip")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(newiFolder:)];
	[item setImage:[NSImage imageNamed:@"newifolder32"]];
    [toolbarItems setObject:item forKey:@"NewiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"NewiFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"SetupiFolder"];
	[item setPaletteLabel:NSLocalizedString(@"Set Up iFolder", @"iFolderWin Toolbar Set Up Pallette Button")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Set Up", @"iFolderWin Toolbar Set Up Selector Button")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Set up the selected iFolder", @"iFolderWin Toolbar Set Up ToolTip")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(setupiFolder:)];
	[item setImage:[NSImage imageNamed:@"setup32"]];
    [toolbarItems setObject:item forKey:@"SetupiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"SetupiFolder"];
	[item release];
	
	item=[[NSToolbarItem alloc] initWithItemIdentifier:NSToolbarSpaceItemIdentifier];
	[toolbarItems setObject:item forKey:NSToolbarSpaceItemIdentifier];
	[toolbarItemKeys addObject:NSToolbarSpaceItemIdentifier];
	[item release];

	
	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"ShareiFolder"];
	[item setPaletteLabel:NSLocalizedString(@"Share an iFolder", @"iFolderWin Toolbar Share Pallette Button")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Share", @"iFolderWin Toolbar Share Selector Button")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Share the selected iFolder", @"iFolderWin Toolbar Share ToolTip")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(shareiFolder:)];
	[item setImage:[NSImage imageNamed:@"share32"]];
    [toolbarItems setObject:item forKey:@"ShareiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"ShareiFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"ResolveConflicts"];
	[item setPaletteLabel:NSLocalizedString(@"Resolve Conflicts", @"iFolderWin Toolbar Resolve Pallette Button")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Resolve", @"iFolderWin Toolbar Resolve Selector Button")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Resolve conflicts in the selected iFolder", @"iFolderWin Toolbar Resolve ToolTip")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(resolveConflicts:)];
	[item setImage:[NSImage imageNamed:@"conflict32"]];
    [toolbarItems setObject:item forKey:@"ResolveConflicts"]; // add to toolbar list
	[toolbarItemKeys addObject:@"ResolveConflicts"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"SynciFolder"];
	[item setPaletteLabel:NSLocalizedString(@"Synchronize iFolder", @"iFolderWin Toolbar Synchronize Pallette Button")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Synchronize", @"iFolderWin Toolbar Synchronize Selector Button")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Synchronize the selected iFolder", @"iFolderWin Toolbar Synchronize ToolTip")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(synciFolder:)];
	[item setImage:[NSImage imageNamed:@"sync32"]];
    [toolbarItems setObject:item forKey:@"SynciFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"SynciFolder"];
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






@end
