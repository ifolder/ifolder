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
	createMode = NO;

	// Initialized the controls
	[name setStringValue:@""];
	[name setEnabled:NO];
	[state setStringValue:@""];
	[state setEnabled:NO];
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
	[loginout setEnabled:NO];
	[loginout setTitle:NSLocalizedString(@"Log In", @"Login/out button on accounts dialog")];
	
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

			[domains addObject:dom];
			if([[dom isDefault] boolValue])
				defaultDomain = dom;
		}
	}
	@catch(NSException *ex)
	{
		NSLog(@"Exception in GetDomains: %@ - %@", [ex name], [ex reason]);
	}
}




- (IBAction)loginoutClicked:(id)sender
{
	if(	createMode )
	{
		[self activateAccount];
	}
	else
	{
		if(selectedDomain != nil)
		{
			if([selectedDomain authenticated])
			{
				[self logoutAccount];
			}
			else
			{
				[self loginAccount];
			}
		}
	}
}




- (void)loginAccount
{
	if( (selectedDomain != nil) &&
		(![selectedDomain authenticated]) &&
		( [[password stringValue] length] > 0 ) )
	{
		[[NSApp delegate] setupSimiasProxies:[selectedDomain host]];

		@try
		{
			AuthStatus *authStatus = [[simiasService LoginToRemoteDomain:[selectedDomain ID]
										usingPassword:[password stringValue]] retain];

			unsigned int statusCode = [[authStatus statusCode] unsignedIntValue];
			
			switch(statusCode)
			{
				case ns1__StatusCodes__Success:		// Success
				case ns1__StatusCodes__SuccessInGrace:		// SuccessInGrace
				{
					[loginout setEnabled:YES];
					[loginout setTitle:NSLocalizedString(@"Log Out", @"Login/Logout Button on Accounts Dialog")];
					[state setStringValue:NSLocalizedString(@"Logged in", @"Login status on Accounts Dialog")];
					[selectedDomain setValue:[NSNumber numberWithBool:YES] forKeyPath:@"properties.authenticated"];
					
					if( (authStatus != nil) && ([authStatus remainingGraceLogins] < [authStatus totalGraceLogins]) )
					{
						NSBeginAlertSheet(NSLocalizedString(@"Expired Password", @"Error Dialog Title when password has expired"), 
						NSLocalizedString(@"OK", @"Error Dialog Button when password has expired"), nil, nil, 
						parentWindow, nil, nil, nil, nil, 
						[NSString stringWithFormat:NSLocalizedString(@"Your password has expired.  You have %d grace logins remaining.", 
									@"Error Dialog Message when password has expired"), [authStatus remainingGraceLogins]]);
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
								initWithCert:certRef forHost:[selectedDomain host]];
						
						[NSApp beginSheet:[certSheet window] modalForWindow:parentWindow
							modalDelegate:self didEndSelector:@selector(certSheetDidEnd:returnCode:contextInfo:) contextInfo:certRef];
					}
					@catch(NSException *ex)
					{
						NSLog(@"Exception getting cert.");
					}						
					break;
				}
				case ns1__StatusCodes__UnknownUser:		// UnknownUser
				case ns1__StatusCodes__InvalidCredentials:		// InvalidCredentials
				case ns1__StatusCodes__InvalidPassword:		// InvalidPassword
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", @"Error Dialog title when password invalid"), 
					NSLocalizedString(@"OK", @"Error Dialog button when password invalid"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The username or password is invalid.  Please try again.", @"Error Dialog Message when password invalid"));
					break;
				}
				case ns1__StatusCodes__AccountDisabled:		// AccountDisabled
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", @"Error Dialog title when user account disabled"), 
					NSLocalizedString(@"OK", @"Error Dialog button when user account disabled"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The user account is disabled.  Please contact your network administrator for assistance.", @"Error Dialog Message when user account disabled"));
					break;
				}
				case ns1__StatusCodes__SimiasLoginDisabled:
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", @"Error dialog title when iFolder account disabled"), 
					NSLocalizedString(@"OK", @"Error dialog button when iFolder account disabled"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The iFolder account is disabled.  Please contact your network administrator for assistance.", @"Error dialog message when iFolder account disabled"));
					break;
				}
				case ns1__StatusCodes__AccountLockout:		// AccountLockout
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", @"Error dialog title when account is locked"), 
					NSLocalizedString(@"OK", @"Error dialog button when account is locked"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The user account has been locked out.  Please contact your network administrator for assistance.", @"Error dialog message when account is locked"));
					break;
				}
				case ns1__StatusCodes__UnknownDomain:		// UnknownDomain
				{
					NSBeginAlertSheet(NSLocalizedString(@"iFolder server is unknown", @"Error dialog title when domain is unknown"), 
					NSLocalizedString(@"OK", @"Error dialog button when domain is unknown"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The server specified can not be located.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", @"Error dialog message when domain is unknown"));
					break;
				}
				case ns1__StatusCodes__InternalException:		// InternalException
				case ns1__StatusCodes__MethodNotSupported:	// MethodNotSupported
				case ns1__StatusCodes__Timeout:	// Timeout
				case ns1__StatusCodes__AmbiguousUser:		// AmbiguousUser
				case ns1__StatusCodes__Unknown:	// Unknown
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", @"General error dialog title for login"), 
					NSLocalizedString(@"OK", @"General error dialog button for login"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", @"General error dialog message for login"));
					break;
				}
			}

			[authStatus release];
		}
		@catch (NSException *e)
		{
			NSLog(@"Exception thrown calling ConnectToDomain: %@", [e name]);
			NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", @"Exception error dialog title for login"), 
			NSLocalizedString(@"OK", @"Exception error button title for login"), nil, nil, 
			parentWindow, nil, nil, nil, nil, 
			NSLocalizedString(@"An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", @"Exception error dialog message for login"));
		}
	}
}




- (void)logoutAccount
{
	if( (selectedDomain != nil) &&
		([selectedDomain authenticated]) )
	{
		@try
		{
			AuthStatus *authStatus = [[simiasService LogoutFromRemoteDomain:[selectedDomain ID]] retain];

			unsigned int statusCode = [[authStatus statusCode] unsignedIntValue];

			if(statusCode == ns1__StatusCodes__Success)
			{
				[loginout setEnabled:YES];
				[loginout setTitle:NSLocalizedString(@"Log In", @"Login/Logout Button on Accounts Dialog")];
				[state setStringValue:NSLocalizedString(@"Logged out", @"Login/Logout status on Accounts dialog")];
				[selectedDomain setValue:[NSNumber numberWithBool:NO] forKeyPath:@"properties.authenticated"];
			}
			else
			{
				NSLog(@"Error returned from LogoutFromRemoteDomain: %d", statusCode);
				NSBeginAlertSheet(NSLocalizedString(@"Error logging out of Account", @"Error Logout Dialog title on Accounts dialog"), 
				NSLocalizedString(@"OK", @"Error Logout Dialog button on Accounts dialog"), nil, nil, 
				parentWindow, nil, nil, nil, nil, 
				NSLocalizedString(@"An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", @"Error Logout Dialog message on Accounts dialog"));
			}

			[authStatus release];
		}
		@catch (NSException *e)
		{
			NSLog(@"Exception thrown calling LogoutFromRemoteDomain: %@", [e name]);
			NSBeginAlertSheet(NSLocalizedString(@"Error logging out of Account", @"Error Logout Dialog title on Accounts dialog"), 
			NSLocalizedString(@"OK", @"Error Logout Dialog button on Accounts dialog"), nil, nil, 
			parentWindow, nil, nil, nil, nil, 
			NSLocalizedString(@"An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", @"Error Logout Dialog message on Accounts dialog"));
		}
	}
}




- (void)activateAccount
{
	if( ([[host stringValue] length] > 0) &&
		([[userName stringValue] length] > 0) &&
		([[password stringValue] length] > 0) )
	{
		unsigned int statusCode;
		AuthStatus *authStatus = nil;

		[[NSApp delegate] setupSimiasProxies:[host stringValue]];

		@try
		{
			iFolderDomain *newDomain = [simiasService ConnectToDomain:[userName stringValue] 
				usingPassword:[password stringValue] andHost:[host stringValue]];

			statusCode = [[newDomain statusCode] unsignedIntValue];
			
			// Check to see if we are in grace, if we are, we need to call to get the authStatus so we can
			// tell the user they are limited by grace logins
			// For some reason we now need to do this even if we succeed, probably to set some
			// state down in Simias.
			if(	(statusCode == ns1__StatusCodes__Success) ||
				(statusCode == ns1__StatusCodes__SuccessInGrace) )
			{
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
					// Set the authenticated to true if the above code was successful
					[newDomain setValue:[NSNumber numberWithBool:YES] forKeyPath:@"properties.authenticated"];

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
						@catch(NSException *ex)
						{
							NSLog(@"SetDefaultDomain Failed with an exception.");
						}
					}

					if([rememberPassword state] == YES)
					{
						@try
						{
							[simiasService SetDomainPassword:[newDomain ID] password:[password stringValue]];	
						}
						@catch(NSException *ex)
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
						NSBeginAlertSheet(NSLocalizedString(@"Expired Password", @"Grace Login Warning Dialog Title"), 
						NSLocalizedString(@"OK", @"Grace Login Warning Dialog button"), nil, nil, 
						parentWindow, nil, nil, nil, nil, 
						[NSString stringWithFormat:NSLocalizedString(@"Your password has expired.  You have %d grace logins remaining.", 
									@"Grace Login Warning Dialog message"), [authStatus remainingGraceLogins]]);
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
								initWithCert:certRef forHost:[host stringValue]];
						
						[NSApp beginSheet:[certSheet window] modalForWindow:parentWindow
							modalDelegate:self didEndSelector:@selector(certSheetDidEnd:returnCode:contextInfo:) contextInfo:certRef];
					}
					@catch(NSException *ex)
					{
						NSLog(@"Exception getting cert.");
					}						
					break;
				}
				case ns1__StatusCodes__UnknownUser:		// UnknownUser
				case ns1__StatusCodes__InvalidCredentials:		// InvalidCredentials
				case ns1__StatusCodes__InvalidPassword:		// InvalidPassword
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", @"Error Dialog title when password invalid"), 
					NSLocalizedString(@"OK", @"Error Dialog button when password invalid"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The username or password is invalid.  Please try again.", @"Error Dialog Message when password invalid"));
					break;
				}
				case ns1__StatusCodes__AccountDisabled:		// AccountDisabled
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", @"Error Dialog title when user account disabled"), 
					NSLocalizedString(@"OK", @"Error Dialog button when user account disabled"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The user account is disabled.  Please contact your network administrator for assistance.", @"Error Dialog Message when user account disabled"));
					break;
				}
				case ns1__StatusCodes__SimiasLoginDisabled:
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", @"Error dialog title when iFolder account disabled"), 
					NSLocalizedString(@"OK", @"Error dialog button when iFolder account disabled"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The iFolder account is disabled.  Please contact your network administrator for assistance.", @"Error dialog message when iFolder account disabled"));
					break;
				}
				case ns1__StatusCodes__AccountLockout:		// AccountLockout
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", @"Error dialog title when account is locked"), 
					NSLocalizedString(@"OK", @"Error dialog button when account is locked"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The user account has been locked out.  Please contact your network administrator for assistance.", @"Error dialog message when account is locked"));
					break;
				}
				case ns1__StatusCodes__UnknownDomain:		// UnknownDomain
				{
					NSBeginAlertSheet(NSLocalizedString(@"iFolder server is unknown", @"Error dialog title when domain is unknown"), 
					NSLocalizedString(@"OK", @"Error dialog button when domain is unknown"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"The server specified can not be located.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", @"Error dialog message when domain is unknown"));
					break;
				}
				case ns1__StatusCodes__InternalException:		// InternalException
				case ns1__StatusCodes__MethodNotSupported:	// MethodNotSupported
				case ns1__StatusCodes__Timeout:	// Timeout
				case ns1__StatusCodes__AmbiguousUser:		// AmbiguousUser
				case ns1__StatusCodes__Unknown:	// Unknown
				{
					NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", @"General error dialog title for login"), 
					NSLocalizedString(@"OK", @"General error dialog button for login"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", @"General error dialog message for login"));
					break;
				}
			}
		}
		@catch (NSException *e)
		{
			NSLog(@"Exception thrown calling ConnectToDomain: %@", [e reason]);
			if([[e reason] isEqualToString:@"DomainExistsError"])
			{
				NSBeginAlertSheet(NSLocalizedString(@"Account Already Exists", @"Attach Error Account Exists Title"), 
					NSLocalizedString(@"OK", @"Attach Error Account Exists Button"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"An account for this server already exists for your current local login identity on this computer.  Only one account per server is allowed.", @"Attach Error Account Exists Message"));
			}
			else
			{
				NSBeginAlertSheet(NSLocalizedString(@"Unable to Connect to iFolder Server", @"Exception error dialog title for login"), 
				NSLocalizedString(@"OK", @"Exception error button title for login"), nil, nil, 
				parentWindow, nil, nil, nil, nil, 
				NSLocalizedString(@"An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", @"Exception error dialog message for login"));
			}
		}
	}
}



- (IBAction)addAccount:(id)sender
{
	createMode = YES;
	[accounts deselectAll:self];

	[name setStringValue:@""];
	[name setEnabled:YES];
	[state setStringValue:NSLocalizedString(@"Logged out", @"Initial Status of login state on Accounts dialog")];
	[state setEnabled:YES];
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

	[loginout setEnabled:NO];
	[parentWindow makeFirstResponder:host];
}




- (IBAction)removeAccount:(id)sender
{
	[leaveDomainController showWindow:self
				showSystemName:[selectedDomain name]
				showServer:[selectedDomain host]
				showUserName:[selectedDomain userName]];
}



- (void)certSheetDidEnd:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	SecCertificateRef certRef = (SecCertificateRef)contextInfo;
	
	if(returnCode)
	{
		@try
		{
			[simiasService StoreCertificate:certRef forHost:[host stringValue]];
			[self loginoutClicked:self];
		}
		@catch(NSException *ex)
		{
			NSLog(@"Exception storing certificate.");
		}						
	}
	else
	{
		NSLog(@"User did not accept certificate, not storing or authenticating");
	}
	
	if(certRef != NULL)
	{
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
	}
	@catch(NSException *ex)
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

			[selectedDomain setValue:[NSNumber numberWithBool:YES] forKeyPath:@"properties.isEnabled"];

			if([selectedDomain authenticated])
			{
				[loginout setEnabled:YES];
				[loginout setTitle:NSLocalizedString(@"Log Out", @"Login/out button on accounts dialog")];
				[state setStringValue:NSLocalizedString(@"Logged in", @"state on acocunts dialog")];
			}
			else
			{
				[loginout setEnabled:YES];
				[loginout setTitle:NSLocalizedString(@"Log In", @"Login/out button on accounts dialog")];
				[state setStringValue:NSLocalizedString(@"Logged out", @"state on acocunts dialog")];
			}
		}
		@catch(NSException *ex)
		{
			NSLog(@"SetDomainActive Failed with an exception.");
		}
	}
	else
	{
		@try
		{
			[simiasService SetDomainInactive:[selectedDomain ID]];	

			[selectedDomain setValue:[NSNumber numberWithBool:NO] forKeyPath:@"properties.isEnabled"];

			[loginout setEnabled:NO];
			[loginout setTitle:NSLocalizedString(@"Log In", @"Login/out button on accounts dialog")];
			[state setStringValue:NSLocalizedString(@"Disabled", @"state on acocunts dialog")];
		}
		@catch(NSException *ex)
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
					[defaultDomain setValue:[NSNumber numberWithBool:NO] forKeyPath:@"properties.isDefault"];
							
				defaultDomain = selectedDomain;
				[defaultDomain setValue:[NSNumber numberWithBool:YES] forKeyPath:@"properties.isDefault"];
				[[iFolderData sharedInstance] refresh:NO];
			}
			@catch(NSException *ex)
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
		}
		@catch(NSException *ex)
		{
			NSLog(@"Saving password failed with an exception");
		}
	}
}


-(void)refreshData
{
}


- (void)tableViewSelectionDidChange:(NSNotification *)aNotification
{
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

		if([[selectedDomain isEnabled] boolValue])
		{
			if([selectedDomain authenticated])
			{
				[loginout setEnabled:YES];
				[loginout setTitle:NSLocalizedString(@"Log Out", @"Login/out button on accounts dialog")];
				[state setStringValue:NSLocalizedString(@"Logged in", @"state on acocunts dialog")];
			}
			else
			{
				[loginout setEnabled:YES];
				[loginout setTitle:NSLocalizedString(@"Log In", @"Login/out button on accounts dialog")];
				[state setStringValue:NSLocalizedString(@"Logged out", @"state on acocunts dialog")];
			}
		}
		else
		{
			[loginout setEnabled:NO];
			[loginout setTitle:NSLocalizedString(@"Log In", @"Login/out button on accounts dialog")];
			[state setStringValue:NSLocalizedString(@"Disabled", @"state on acocunts dialog")];
		}
		
		[state setEnabled:YES];
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
		@catch(NSException *ex)
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
		
		[removeAccount setEnabled:YES];
		
		// Also update the details page
		[domainDescription setString:[selectedDomain description] ];

		@try
		{
			ds = [ifolderService GetUserDiskSpace:[selectedDomain userID]];
		}
		@catch(NSException *ex)
		{
			ds = nil;
		}
	}
	else
	{
		[name setStringValue:@""];
		[name setEnabled:NO];
		[state setStringValue:@""];
		[state setEnabled:NO];
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
		
		[loginout setEnabled:NO];
		[loginout setTitle:NSLocalizedString(@"Log In", @"Login/out button on accounts dialog")];
		
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
			[loginout setEnabled:YES];
		else if(createMode)
			[loginout setEnabled:NO];
	}
}


@end
