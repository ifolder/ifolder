#import "iFolderPrefsController.h"
#import "iFolderDomain.h"
#import "iFolderService.h"
#import "MainWindowController.h"

@implementation iFolderPrefsController

- (void)awakeFromNib
{
	[self setShouldCascadeWindows:NO];
	[self setWindowFrameAutosaveName:@"iFolder Preferences"];
	
	[[self window] setContentSize:[generalView frame].size];
	[[self window] setContentView: generalView];
	[[self window] setTitle:@"iFolder Preferences: General"];
	
	[self setupToolbar];

	[toolbar setSelectedItemIdentifier:@"General"];


	webService = [[iFolderService alloc] init];

	@try
	{
		int x;
		
		NSArray *newDomains = [webService GetDomains];
	
		// add all domains that are not workgroup
		for(x=0; x < [newDomains count]; x++)
		{
			iFolderDomain *dom = [newDomains objectAtIndex:x];
			NSString *domainID = [[dom properties] valueForKey:@"ID"];
			
			if([domainID compare:WORKGROUP_DOMAIN] != 0)
			{
				[domainsController addObject:dom];
			}
		}

//		[domainsController addObjects:newDomains];
	}
	@catch (NSException *e)
	{
		[[NSApp delegate] addLog:@"Reading domains failed with exception"];
	}

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




- (BOOL)selectionShouldChangeInTableView:(NSTableView *)aTableView
{
	int selIndex = [domainsController selectionIndex];
	iFolderDomain *dom = [[domainsController arrangedObjects] objectAtIndex:selIndex];
	if([[dom properties] objectForKey:@"Authenticated"] != nil)
	{
		NSBeginAlertSheet(@"Save Account", @"Save", @"Don't Save", @"Cancel", 
			[self window], self, @selector(changeSelectionResponse:returnCode:contextInfo:), nil, (void *)selIndex, 
			@"The selected account has not been logged in to and saved.  Would you like to login and save it now?");
	}
	return YES;
}



- (IBAction)addDomain:(id)sender
{
	[domainsController add:sender];
}



- (IBAction)removeDomain:(id)sender
{
	[domainsController remove:sender];
}



- (IBAction)loginToDomain:(id)sender
{
	int selIndex = [domainsController selectionIndex];
	iFolderDomain *dom = [[domainsController arrangedObjects] objectAtIndex:selIndex];
	if([[dom properties] objectForKey:@"Authenticated"] != nil)
	{
		if( ([[dom UserName] length] > 0) &&
			([[dom Host] length] > 0) &&
			([[dom Password] length] > 0) )
		{
			if([[NSApp delegate] connectToDomain:dom] == YES)
			{
				// we logged in, the domain object should be updated
			}
			else
			{
				NSBeginAlertSheet(@"Login Failed", @"OK", nil, nil, 
					[self window], nil, nil, nil, nil, 
					@"Login failed, please try again.");
			}
		}
	}
}



- (void)changeSelectionResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	switch(returnCode)
	{
		case NSAlertDefaultReturn:
			[domainsController setSelectionIndex:(int)contextInfo];
			// login 
//			iFolderDomain *dom = [[domainsController arrangedObjects] objectAtIndex:(int)contextInfo];
			break;
		case NSAlertAlternateReturn:
			[domainsController removeObjectAtArrangedObjectIndex:(int)contextInfo];
			break;
		case NSAlertOtherReturn:
		case NSAlertErrorReturn:
			[domainsController setSelectionIndex:(int)contextInfo];
			break;
	}
}

@end
