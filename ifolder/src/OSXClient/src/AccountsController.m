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

#import "AccountsController.h"
#import "iFolderApplication.h"
#import "SimiasService.h"
#import "iFolderDomain.h"
#import "LeaveDomainSheetController.h"


@implementation AccountsController

- (void)awakeFromNib
{
	NSLog(@"Accounts Controller Awoke from Nib");
	createMode = NO;

	// Initialized the controls
	[name setStringValue:@""];
	[name setEnabled:NO];
	[host setStringValue:@""];
	[host setEnabled:NO];
	[userName setStringValue:@""];
	[userName setEnabled:NO];

	[password setStringValue:@""];
	[password setEnabled:NO];

	[rememberPassword setState:0];
	[rememberPassword setEnabled:NO];
	[enableAccount setState:0];
	[enableAccount setEnabled:NO];
	[defaultAccount setState:0];
	[defaultAccount setEnabled:NO];
	[activate setEnabled:NO];
	
	[removeAccount setEnabled:NO];

	domains = [[NSMutableArray alloc] init];	
	simiasService = [[SimiasService alloc] init];

	@try
	{
		int x;
		NSArray *newDomains = [simiasService GetDomains:YES];
		// add all domains that are not workgroup
		for(x=0; x < [newDomains count]; x++)
		{
			iFolderDomain *dom = [newDomains objectAtIndex:x];

			NSLog(@"Adding domain %@", [dom name]);
			[domains addObject:dom];
			if([[dom isDefault] boolValue])
				defaultDomain = dom;
		}
	}
	@catch(NSException ex)
	{
		[[NSApp delegate] addLog:@"Reading domains failed with exception"];
		NSLog(@"Exception in GetDomains: %@ - %@", [ex name], [ex reason]);
	}
}


- (IBAction)activateAccount:(id)sender
{
	NSLog(@"Activate Account clicked");

	if( ([[host stringValue] length] > 0) &&
		([[userName stringValue] length] > 0) &&
		([[password stringValue] length] > 0) )
	{
		@try
		{
			iFolderDomain *newDomain = [simiasService ConnectToDomain:[userName stringValue] 
				usingPassword:[password stringValue] andHost:[host stringValue]];

			createMode = NO;			
			[domains addObject:newDomain];
			[accounts reloadData];
			if(defaultDomain != nil)
				[defaultDomain setValue:NO forKeyPath:@"properties.isDefault"];
				
			defaultDomain = newDomain;

			NSMutableIndexSet    *childRows = [NSMutableIndexSet indexSet];
			[childRows addIndex:([domains count] - 1)];
			[accounts selectRowIndexes:childRows byExtendingSelection:NO];			
		}
		@catch (NSException *e)
		{
			NSBeginAlertSheet(@"Activation failed", @"OK", nil, nil, 
				parentWindow, nil, nil, nil, nil, 
				[NSString stringWithFormat:@"Activation failed with the error: %@", [e name]]);
		}
	}
}



- (IBAction)addAccount:(id)sender
{
	NSLog(@"Add Account Clicked");
	createMode = YES;
	[accounts deselectAll:self];

	[name setStringValue:@"<new account>"];
	[name setEnabled:YES];
	[host setStringValue:@""];
	[host setEnabled:YES];
	[userName setStringValue:@""];
	[userName setEnabled:YES];
	[password setStringValue:@""];
	[password setEnabled:YES];

	[rememberPassword setState:NO];
	[rememberPassword setEnabled:YES];
	[enableAccount setState:YES];
	[enableAccount setEnabled:NO];
	[defaultAccount setState:YES];
	[defaultAccount setEnabled:NO];

	[removeAccount setEnabled:NO];	

	[activate setEnabled:NO];
	[parentWindow makeFirstResponder:host];
}




- (IBAction)removeAccount:(id)sender
{
	NSLog(@"Remove Account Clicked");
	[leaveDomainController showWindow:self];
}


-(void)leaveSelectedDomain:(BOOL)localOnly
{
	@try
	{
		[simiasService LeaveDomain:[selectedDomain ID] withOption:localOnly];

		[domains removeObject:selectedDomain];
		[accounts reloadData];
		[accounts deselectAll:self];
		NSLog(@"SetDomainActive Succeded.");
	}
	@catch(NSException ex)
	{
		NSLog(@"SetDomainActive Failed with an exception.");
	}
}



- (IBAction)toggleActive:(id)sender
{
	if([enableAccount state] == YES)
	{
		@try
		{
			[simiasService SetDomainActive:[selectedDomain ID]];	
			NSLog(@"SetDomainActive Succeded.");
		}
		@catch(NSException ex)
		{
			NSLog(@"SetDomainActive Failed with an exception.");
		}
	}
	else
	{
		@try
		{
			[simiasService SetDomainInactive:[selectedDomain ID]];	
			NSLog(@"SetDomainInactive Succeded.");
		}
		@catch(NSException ex)
		{
			NSLog(@"SetDomainInactive Failed with an exception.");
		}
	}
}




- (IBAction)toggleDefault:(id)sender
{
	if([defaultAccount state] == YES)
	{
		@try
		{
			[simiasService SetDefaultDomain:[selectedDomain ID]];	
			if(defaultDomain != nil)
				[defaultDomain setValue:NO forKeyPath:@"properties.isDefault"];
						
			defaultDomain = selectedDomain;
			NSLog(@"SetDefaultDomain Succeded.");
		}
		@catch(NSException ex)
		{
			NSLog(@"SetDefaultDomain Failed with an exception.");
		}
	}
}




- (IBAction)toggleSavePassword:(id)sender
{
	NSString *newPassword = nil;

	if([rememberPassword state] != NO)
	{
		if([[password stringValue] length] > 0)
		{
			NSLog(@"Saving password...");
			newPassword = [password stringValue];
		}
		else
			NSLog(@"Saved password was nil, removing saved password...");
	}
	else
	{
		NSLog(@"Removing saved password...");
	}

	@try
	{
		[simiasService SetDomainPassword:[selectedDomain ID] password:newPassword];	
		NSLog(@"Saving password succeeded.");
	}
	@catch(NSException ex)
	{
	}
}


-(void)refreshData
{
}


- (void)tableViewSelectionDidChange:(NSNotification *)aNotification
{
	NSLog(@"The selection changed");
	if([accounts selectedRow] == -1)
		selectedDomain = nil;
	else
		selectedDomain = [domains objectAtIndex:[accounts selectedRow]];

	if(selectedDomain != nil)
	{
		createMode = NO;
		[name setStringValue:[selectedDomain name]];
		[name setEnabled:YES];
		[host setStringValue:[selectedDomain host]];
		[host setEnabled:NO];
		[userName setStringValue:[selectedDomain userName]];
		[userName setEnabled:NO];
		if([selectedDomain password] != nil)
			[password setStringValue:[selectedDomain password]];
		[password setEnabled:YES];

		NSString *savedPassword = nil;
		
		@try
		{
			savedPassword = [simiasService GetDomainPassword:[selectedDomain ID]];
		}
		@catch(NSException ex)
		{
		}

		if(savedPassword == nil)
		{
			[rememberPassword setState:0];
		}
		else
		{
			[rememberPassword setState:1];
			[password setStringValue:savedPassword];
		}

		[rememberPassword setEnabled:YES];
		[enableAccount setState:[[selectedDomain isEnabled] boolValue]];
		[enableAccount setEnabled:YES];
		[defaultAccount setState:[[selectedDomain isDefault] boolValue]];
		[defaultAccount setEnabled:![[selectedDomain isDefault] boolValue]];
		
		[activate setEnabled:NO];
		[removeAccount setEnabled:YES];
	}
	else
	{
		[name setStringValue:@""];
		[name setEnabled:NO];
		[host setStringValue:@""];
		[host setEnabled:NO];
		[userName setStringValue:@""];
		[userName setEnabled:NO];

		[password setStringValue:@""];
		[password setEnabled:NO];

		[rememberPassword setState:0];
		[rememberPassword setEnabled:NO];
		[enableAccount setState:0];
		[enableAccount setEnabled:NO];
		[defaultAccount setState:0];
		[defaultAccount setEnabled:NO];	
		
		[activate setEnabled:NO];
		[removeAccount setEnabled:NO];		
	}
}


- (BOOL)selectionShouldChangeInTableView:(NSTableView *)aTableView
{
	NSLog(@"Selection Change queried");

/*
	int selIndex = [domainsController selectionIndex];
	iFolderDomain *dom = [[domainsController arrangedObjects] objectAtIndex:selIndex];
	if([[[dom properties] objectForKey:@"CanActivate"] boolValue] == true )
	{
		NSBeginAlertSheet(@"Save Account", @"Save", @"Don't Save", @"Cancel", 
			[self window], self, @selector(changeAccountResponse:returnCode:contextInfo:), nil, (void *)selIndex, 
			@"The selected account has not been logged in to and saved.  Would you like to login and save it now?");
	}
*/
	return YES;
}



- (void)changeAccountResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
/*
	switch(returnCode)
	{
		case NSAlertDefaultReturn:
			[domainsController setSelectionIndex:(int)contextInfo];
			// login 
			iFolderDomain *dom = [[domainsController arrangedObjects] objectAtIndex:(int)contextInfo];
			break;
		case NSAlertAlternateReturn:
			[domainsController removeObjectAtArrangedObjectIndex:(int)contextInfo];
			break;
		case NSAlertOtherReturn:
		case NSAlertErrorReturn:
			[domainsController setSelectionIndex:(int)contextInfo];
			break;
	}
*/
}


// Delegates for TableView
-(int)numberOfRowsInTableView:(NSTableView *)aTableView
{
	return [domains count];
}

-(id)tableView:(NSTableView *)aTableView objectValueForTableColumn:(NSTableColumn *)aTableColumn row:(int)rowIndex
{
	return [[domains objectAtIndex:rowIndex] name];
}

-(void)tableView:(NSTableView *)aTableView setObjectValue:(id)anObject forTableColumn:(NSTableColumn *)aTableColumn row:(int)rowIndex
{
}

- (void)controlTextDidChange:(NSNotification *)aNotification;
{
	if(	([aNotification object] == userName) ||
		([aNotification object] == host) ||
		([aNotification object] == password) )
	{
		if( ([host stringValue] != nil) && ([[host stringValue] length] > 0) &&
			([userName stringValue] != nil) && ([[userName stringValue] length] > 0) &&
			([password stringValue] != nil) && ([[password stringValue] length] > 0) &&
			(createMode) )
			[activate setEnabled:YES];
		else
			[activate setEnabled:NO];
	}
}


@end
