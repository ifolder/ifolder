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
#import "iFolderService.h"
#import "iFolderDomain.h"
#import "LeaveDomainSheetController.h"
#import "iFolderData.h"
#import "VerticalBarView.h"
#import "DiskSpace.h"
#import "AuthStatus.h"
#include "simiasStub.h"
#include "Security/Security.h"
#import "AcceptCertSheetController.h"
#include "SecurityInterface/SFCertificatePanel.h"

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
	ifolderService = [[iFolderService alloc] init];
	
	isFirstDomain = YES;

	@try
	{
		int x;
		NSArray *newDomains = [simiasService GetDomains:YES];
		// add all domains that are not workgroup
		for(x=0; x < [newDomains count]; x++)
		{
			iFolderDomain *dom = [newDomains objectAtIndex:x];
			isFirstDomain = NO;

			NSLog(@"Adding domain %@", [dom name]);
			[domains addObject:dom];
			if([[dom isDefault] boolValue])
				defaultDomain = dom;
		}
	}
	@catch(NSException ex)
	{
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
		unsigned int statusCode;
		AuthStatus *authStatus = nil;

		@try
		{
			iFolderDomain *newDomain = [simiasService ConnectToDomain:[userName stringValue] 
				usingPassword:[password stringValue] andHost:[host stringValue]];

			statusCode = [[newDomain statusCode] unsignedIntValue];
			
			// Check to see if we are in grace, if we are, we need to call to get the authStatus so we can
			// tell the user they are limited by grace logins
			if(statusCode == ns1__StatusCodes__SuccessInGrace)
			{
				NSLog(@"ConnectToDomain returned status code of in grace");
				@try
				{
					authStatus = [[simiasService LoginToRemoteDomain:[newDomain ID] usingPassword:[password stringValue]] retain];
					statusCode = [[authStatus statusCode] unsignedIntValue];
				}
				@catch (NSException *e)
				{
					NSLog(@"LoginToRemoteDomain Failed");
					NSLog([e reason]);
				}
			}
			
			switch(statusCode)
			{
				case ns1__StatusCodes__Success:		// Success
				case ns1__StatusCodes__SuccessInGrace:		// SuccessInGrace
				{
					if([defaultAccount state] == YES)
					{
						@try
						{
							[simiasService SetDefaultDomain:[newDomain ID]];	
							if(defaultDomain != nil)
								[defaultDomain setValue:[NSNumber numberWithBool:NO] forKeyPath:@"properties.isDefault"];
										
							// set the new domain to be the default
							defaultDomain = newDomain;
							[defaultDomain setValue:[NSNumber numberWithBool:YES] forKeyPath:@"properties.isDefault"];
						}
						@catch(NSException ex)
						{
							NSLog(@"SetDefaultDomain Failed with an exception.");
						}
					}

					if([rememberPassword state] == YES)
					{
						@try
						{
							[simiasService SetDomainPassword:[newDomain ID] password:[password stringValue]];	
							NSLog(@"Saving password succeeded.");
						}
						@catch(NSException ex)
						{
							NSLog(@"Saving domain password Failed with an exception.");
						}			
					}

					createMode = NO;			
					[domains addObject:newDomain];
					[accounts reloadData];

					NSMutableIndexSet    *childRows = [NSMutableIndexSet indexSet];
					[childRows addIndex:([domains count] - 1)];
					[accounts selectRowIndexes:childRows byExtendingSelection:NO];
					
					[[iFolderData sharedInstance] refresh:YES];
					
					if( (authStatus != nil) && ([authStatus remainingGraceLogins] < [authStatus totalGraceLogins]) )
					{
						NSBeginAlertSheet(NSLocalizedString(@"Expired Password", nil), 
						NSLocalizedString(@"OK", nil), nil, nil, 
						parentWindow, nil, nil, nil, nil, 
						[NSString stringWithFormat:NSLocalizedString(@"Your password has expired.  You have %d grace logins remaining.", 
									nil), [authStatus remainingGraceLogins]]);
						[authStatus release];
						authStatus = nil;
					}
					break;
				}
				case ns1__StatusCodes__InvalidCertificate:
				{
					@try
					{
						SecCertificateRef certRef = [simiasService GetCertificate:[host stringValue]];

						AcceptCertSheetController *certSheet = [[AcceptCertSheetController alloc]
								initWithCert:certRef];
						
						[NSApp beginSheet:[certSheet window] modalForWindow:parentWindow
							modalDelegate:self didEndSelector:@selector(certSheetDidEnd:returnCode:contextInfo:) contextInfo:certRef];
					}
					@catch(NSException ex)
					{
						NSLog(@"Exception getting cert.");
					}						
					break;
				}
				case ns1__StatusCodes__UnknownUser:		// UnknownUser
				case ns1__StatusCodes__InvalidCredentials:		// InvalidCredentials
				case ns1__StatusCodes__InvalidPassword:		// InvalidPassword
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", nil), 
					NSLocalizedString(@"OK", nil), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The user name or password is invalid.  Please try again.", nil));
					break;
				}
				case ns1__StatusCodes__AccountDisabled:		// AccountDisabled
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", nil), 
					NSLocalizedString(@"OK", nil), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The user account is disabled.  Please contact your network administrator for assistance.", nil));
					break;
				}
				case ns1__StatusCodes__AccountLockout:		// AccountLockout
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", nil), 
					NSLocalizedString(@"OK", nil), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The user account has been locked out.  Please contact your network administrator for assistance.", nil));
					break;
				}
				case ns1__StatusCodes__UnknownDomain:		// UnknownDomain
				case ns1__StatusCodes__InternalException:		// InternalException
				case ns1__StatusCodes__MethodNotSupported:	// MethodNotSupported
				case ns1__StatusCodes__Timeout:	// Timeout
				case ns1__StatusCodes__AmbiguousUser:		// AmbiguousUser
				case ns1__StatusCodes__Unknown:	// Unknown
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", nil), 
					NSLocalizedString(@"OK", nil), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", nil));
					break;
				}
			}
		}
		@catch (NSException *e)
		{
			NSLog(@"Exception thrown calling ConnectToDomain: %@", [e name]);
			NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", nil), 
				NSLocalizedString(@"OK", nil), nil, nil, 
				parentWindow, nil, nil, nil, nil, 
				NSLocalizedString(@"An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", nil));
		}
	}
}



- (IBAction)addAccount:(id)sender
{
	NSLog(@"Add Account Clicked");
	createMode = YES;
	[accounts deselectAll:self];

	[name setStringValue:NSLocalizedString(@"<new account>", nil)];
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
	if(isFirstDomain)
	{
		[defaultAccount setState:YES];
		[defaultAccount setEnabled:NO];
	}
	else
	{
		[defaultAccount setState:NO];
		[defaultAccount setEnabled:YES];
	}

	[removeAccount setEnabled:NO];	

	[activate setEnabled:NO];
	[parentWindow makeFirstResponder:host];
}




- (IBAction)removeAccount:(id)sender
{
	NSLog(@"Remove Account Clicked");
	[leaveDomainController showWindow:self];
}



- (void)certSheetDidEnd:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	SecCertificateRef certRef = (SecCertificateRef)contextInfo;
	
	if(returnCode)
	{
		@try
		{
			[simiasService StoreCertificate:certRef forHost:[host stringValue]];
			[self activateAccount:self];
		}
		@catch(NSException ex)
		{
			NSLog(@"Exception storing certificate.");
		}						
	}
	else
	{
		NSLog(@"User did not accept certificate, do not store or authenticate");
	}
	
	if(certRef != NULL)
	{
		NSLog(@"Releasing the Certificate");
		CFRelease(certRef);
	}
}



-(void)leaveSelectedDomain:(BOOL)localOnly
{
	@try
	{
		[simiasService LeaveDomain:[selectedDomain ID] withOption:localOnly];

		[domains removeObject:selectedDomain];
		[accounts reloadData];
		[accounts deselectAll:self];
		[[iFolderData sharedInstance] refresh:NO];
		NSLog(@"LeaveDomain Succeded.");
	}
	@catch(NSException ex)
	{
		NSLog(@"LeaveDomain Failed with an exception.");
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
	if(!createMode)
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
}




- (IBAction)toggleSavePassword:(id)sender
{
	if(!createMode)
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
}


-(void)refreshData
{
}


- (void)tableViewSelectionDidChange:(NSNotification *)aNotification
{
	NSLog(@"The selection changed");

	DiskSpace *ds = nil;	

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
		
		// Also update the details page
		[domainDescription setString:[selectedDomain description] ];

		@try
		{
			ds = [ifolderService GetUserDiskSpace:[selectedDomain userID]];
		}
		@catch(NSException ex)
		{
			ds = nil;
		}
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
	
	if(ds != nil)
	{
		[usedSpace setStringValue:[NSString stringWithFormat:@"%qi", 
										[ds UsedSpace]/(1024 * 1024)]];

		[totalSpace setStringValue:[NSString stringWithFormat:@"%qi", 
										([ds Limit]/(1024 * 1024))]];

		[freeSpace setStringValue:[NSString stringWithFormat:@"%qi", 
										([ds AvailableSpace]/(1024 * 1024))]];

		[vertBar setBarSize:[ds Limit] withFill:[ds UsedSpace]];
	}
	else
	{
		[usedSpace setStringValue:@""];
		[totalSpace setStringValue:@""];
		[freeSpace setStringValue:@"0"];
		[vertBar setBarSize:0 withFill:0];
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
