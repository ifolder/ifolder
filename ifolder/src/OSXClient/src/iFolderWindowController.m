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
	[bindingOptions setObject:@"No Name" forKey:@"NSNullPlaceholder"];

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
	[[NSApp delegate] addLog:NSLocalizedString(@"Refreshing iFolder view", nil)];

	if([[NSApp delegate] simiasIsRunning])
		[[iFolderData sharedInstance] refresh:NO];

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
		NSBeginAlertSheet(NSLocalizedString(@"No iFolder Domains", nil), NSLocalizedString(@"OK", nil), nil, nil,
			[self window], self, nil, nil, NULL, 
			NSLocalizedString(@"A new iFolder cannot be created because you have not attached to any iFolder Servers.", nil));
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
	NSBeginAlertSheet(NSLocalizedString(@"Revert iFolder", nil), NSLocalizedString(@"Yes", nil), NSLocalizedString(@"Cancel", nil), nil,
		[self window], self, @selector(revertiFolderResponse:returnCode:contextInfo:), nil, (void *)selIndex, 
		NSLocalizedString(@"Are you sure you want to revert the selected iFolder and make it a normal folder?", nil));
}


- (void)revertiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	switch(returnCode)
	{
		case NSAlertDefaultReturn:		// Revert iFolder
		{
			iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:(int)contextInfo];

			NSLog(NSLocalizedString(@"Reverting iFolder %@", nil), [ifolder Name]);

			[[NSApp delegate] addLog:[NSString stringWithFormat:NSLocalizedString(@"Reverting iFolder %@", nil), [ifolder Name]]];

			@try
			{
				[[iFolderData sharedInstance] revertiFolder:[ifolder ID]];		
			}
			@catch (NSException *e)
			{
				NSRunAlertPanel(NSLocalizedString(@"Error reverting iFolder", nil), [e name], NSLocalizedString(@"OK", nil),nil, nil);
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
	NSBeginAlertSheet(NSLocalizedString(@"Delete iFolder", nil), NSLocalizedString(@"Yes", nil), 
		NSLocalizedString(@"Cancel", nil), nil,
		[self window], self, @selector(deleteiFolderResponse:returnCode:contextInfo:), nil, (void *)selIndex, 
		NSLocalizedString(@"Are you sure you want to delete the selected iFolder?", nil));
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

			NSLog(@"Deleting iFolder at index %@", [ifolder Name]);

			@try
			{
				[[iFolderData sharedInstance] deleteiFolder:[ifolder ID]];
//				[ifolderService DeleteiFolder:[ifolder ID]];
//				[ifoldersController removeObject:ifolder];
			}
			@catch (NSException *e)
			{
				NSRunAlertPanel(NSLocalizedString(@"Error deleting iFolder", nil), [e name], NSLocalizedString(@"OK", nil),nil, nil);
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
		NSRunAlertPanel(NSLocalizedString(@"Error creating iFolder", nil), [e name], NSLocalizedString(@"OK", nil),nil, nil);
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
		NSRunAlertPanel(NSLocalizedString(@"Error connecting to Server", nil), [e name], NSLocalizedString(@"OK", nil),nil, nil);
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
				([[[[ifoldersController arrangedObjects] objectAtIndex:selIndex] valueForKeyPath:@"properties.IsWorkgroup"] boolValue] == NO) )
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
	[item setPaletteLabel:NSLocalizedString(@"Create a new iFolder", nil)]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"New", nil)]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Create a new iFolder", nil)]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(newiFolder:)];
	[item setImage:[NSImage imageNamed:@"newifolder32"]];
    [toolbarItems setObject:item forKey:@"NewiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"NewiFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"SetupiFolder"];
	[item setPaletteLabel:NSLocalizedString(@"Setup iFolder", nil)]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Setup", nil)]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Setup a shared iFolder", nil)]; // tooltip
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

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"SynciFolder"];
	[item setPaletteLabel:NSLocalizedString(@"Sync iFolder", nil)]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Sync", nil)]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Sync selected iFolder now", nil)]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(synciFolder:)];
	[item setImage:[NSImage imageNamed:@"sync32"]];
    [toolbarItems setObject:item forKey:@"SynciFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"SynciFolder"];
	[item release];
	

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"ShareiFolder"];
	[item setPaletteLabel:NSLocalizedString(@"Share an iFolder", nil)]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Share", nil)]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Share an iFolder", nil)]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(shareiFolder:)];
	[item setImage:[NSImage imageNamed:@"share32"]];
    [toolbarItems setObject:item forKey:@"ShareiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"ShareiFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"ResolveConflicts"];
	[item setPaletteLabel:NSLocalizedString(@"Resolve Conflicts", nil)]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Resolve", nil)]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Resolve file conflicts", nil)]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(resolveConflicts:)];
	[item setImage:[NSImage imageNamed:@"conflict32"]];
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






@end
