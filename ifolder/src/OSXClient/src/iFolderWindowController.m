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
#import "iFolder.h"
#import "iFolderDomain.h"
#import "iFolderData.h"


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

	[super setShouldCascadeWindows:NO];
	[super setWindowFrameAutosaveName:@"iFolderWindow"];

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
}




- (IBAction)refreshWindow:(id)sender
{
	[[NSApp delegate] addLog:@"Refreshing iFolder view"];

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
		NSBeginAlertSheet(@"No iFolder Domains", @"OK", nil, nil,
			[self window], self, nil, nil, NULL, 
			@"A new iFolder cannot be created because you have not attached to any iFolder Servers.");
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
	NSBeginAlertSheet(@"Revert iFolder", @"Yes", @"Cancel", nil,
		[self window], self, @selector(revertiFolderResponse:returnCode:contextInfo:), nil, (void *)selIndex, 
		@"Are you sure you want to revert the selected iFolder and make it a normal folder?");
}


- (void)revertiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	switch(returnCode)
	{
		case NSAlertDefaultReturn:		// Revert iFolder
		{
			iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:(int)contextInfo];

			NSLog(@"Reverting iFolder %@", [ifolder Name]);

			[[NSApp delegate] addLog:[NSString stringWithFormat:@"Reverting iFolder %@", [ifolder Name]]];

			@try
			{
				[[iFolderData sharedInstance] revertiFolder:[ifolder ID]];		
			}
			@catch (NSException *e)
			{
				NSRunAlertPanel(@"Error reverting iFolder", [e name], @"OK",nil, nil);
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

			[[NSApp delegate] addLog:[NSString stringWithFormat:@"Deleting iFolder at index %@", [ifolder Name]]];

			@try
			{
				[[iFolderData sharedInstance] deleteiFolder:[ifolder ID]];
//				[ifolderService DeleteiFolder:[ifolder ID]];
//				[ifoldersController removeObject:ifolder];
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
		([ifolder IsSubscription] == NO) )
		[[NSWorkspace sharedWorkspace] openFile:path];
	else
		[self setupiFolder:sender];
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
		NSRunAlertPanel(@"Error creating iFolder", [e name], @"OK",nil, nil);
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
		NSRunAlertPanel(@"Error connecting to Server", [e name], @"OK",nil, nil);
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
	int selIndex = [ifoldersController selectionIndex];
	
	if(action == @selector(newiFolder:))
	{
		return YES;
	}
	else if(action == @selector(setupiFolder:))
	{
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
		if (selIndex != NSNotFound)
		{
			if( ([[[ifoldersController arrangedObjects] objectAtIndex:selIndex] IsSubscription] == NO) &&
				([[[[ifoldersController arrangedObjects] objectAtIndex:selIndex] valueForKeyPath:@"properties.IsWorkgroup"] boolValue] == NO) )
				return YES;
		}
		return NO;
	}
	
	return YES;
}



- (NSArrayController *)DomainsController
{
	return nil;//domainsController;
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








@end
