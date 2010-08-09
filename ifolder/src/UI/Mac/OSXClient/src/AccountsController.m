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
*                 $Modified by: Satyam <ssutapalli@novell.com>	01/02/2007  Validating spaces in host and username when account creation
*                 $Modified by: Satyam <ssutapalli@novell.com>  20/03/2008  Updated code related to set "Default Domain"
*                 $Modified by: Satyam <ssutapalli@novell.com>  10/04/2008  Added DefaultiFolder functionality
*                 $Modified by: Satyam <ssutapalli@novell.com>  23/09/2008  Setting the host details to get new certificate when user is moved to a different server 
*                 $Modified by: Satyam <ssutapalli@novell.com>  02/06/2009  Changed the strings according to HF recommendations
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

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
#import "iFolderEncryptController.h"
#import "VerifyPassPhraseController.h"
#import "DefaultiFolderSheetController.h"

#include "simiasStub.h"
#include "Security/Security.h"
#import "AcceptCertSheetController.h"
#include "SecurityInterface/SFCertificatePanel.h"
#include "applog.h"

@interface AccountsController(PRIVATE)	//PRIVATE METHODS

- (void)activateAccount;
- (void)loginAccount;
- (void)logoutAccount;

@end

@implementation AccountsController

//========================================================================
// awakeFromNib
// Method that will be called before loading Nib
//========================================================================
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
	
	selectedDomain = nil;

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
		ifexconlog(@"Accounts:awakefromNib:GetDomains", ex);
	}
}



//========================================================================
// loginoutClicked
// When the button login/logout clicked, this method is called
//========================================================================
- (IBAction)loginoutClicked:(id)sender
{
	[loginout setEnabled:NO];
	[progressIndicator setHidden:NO];
	[progressIndicator startAnimation:self];
	
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

	[loginout setEnabled:YES];
	[progressIndicator setHidden:YES];
	[progressIndicator stopAnimation:self];
	
	[[iFolderData sharedInstance] refresh:NO];
}

//========================================================================
// loginAccount
// Login the account with credentials provided
//========================================================================
- (void)loginAccount
{

	[self validateDomainAddress];

	if( (selectedDomain != nil) &&
		(![selectedDomain authenticated]) &&
		( [[password stringValue] length] > 0 ) )
	{

		@try
		{
			if( [[selectedDomain host] compare:[host stringValue]] != 0 )
			{
				// Set the current host address, I'm not sure what to do if this fails
				[simiasService SetDomainHostAddress:[selectedDomain ID] 
									withAddress:[host stringValue]
									forUser:[userName stringValue]
									withPassword:[password stringValue]];

				[[NSApp delegate] setupSimiasProxies:[host stringValue]];
			}
			else
				[[NSApp delegate] setupSimiasProxies:[selectedDomain host]];
			

			AuthStatus *authStatus = [[simiasService LoginToRemoteDomain:[selectedDomain ID]
										usingPassword:[password stringValue]] retain];
		
			unsigned int statusCode = [[authStatus statusCode] unsignedIntValue];
			
			switch(statusCode)
			{
				case ns1__StatusCodes__Success:		// Success
				case ns1__StatusCodes__SuccessInGrace:		// SuccessInGrace
				{
					[loginout setEnabled:YES];
					[host setEnabled:NO];
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
					[[iFolderData sharedInstance] clientUpdates:[selectedDomain ID]  showstatus:NO]; //Check for new client available or not
					
					[[iFolderData sharedInstance] checkForEncryption:[selectedDomain ID] atLogin:YES];
					break;
				}
				case ns1__StatusCodes__InvalidCertificate:
				{
					@try
					{
						if( [authStatus userName] != nil )
						{
							[selectedDomain setHost:[authStatus userName]];
						}
						//SecCertificateRef certRef = [simiasService GetCertificate:[host stringValue]];
						SecCertificateRef certRef = [simiasService GetCertificate:[selectedDomain host]];
						AcceptCertSheetController *certSheet = [[AcceptCertSheetController alloc]
								initWithCert:certRef forHost:[selectedDomain host]];
						
						[NSApp beginSheet:[certSheet window] modalForWindow:parentWindow
							modalDelegate:self didEndSelector:@selector(certSheetDidEnd:returnCode:contextInfo:) contextInfo:certRef];
					}
					@catch(NSException *ex)
					{
						ifexconlog(@"GetCertificate", ex);
					}						
					break;
				}
				case ns1__StatusCodes__UnknownUser:		// UnknownUser
				case ns1__StatusCodes__InvalidCredentials:		// InvalidCredentials
				case ns1__StatusCodes__InvalidPassword:		// InvalidPassword
				{
					NSBeginAlertSheet(NSLocalizedString(@"The username or password is invalid", @"Error Dialog message when password invalid"), 
					NSLocalizedString(@"OK", @"Error Dialog button when password invalid"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"Please verify the information entered and try again.", @"Error Dialog details when password invalid"));
					break;
				}
				case ns1__StatusCodes__AccountDisabled:		// AccountDisabled
				{
					NSBeginAlertSheet(NSLocalizedString(@"The user account is disabled", @"Error Dialog message when user account disabled"), 
					NSLocalizedString(@"OK", @"Error Dialog button when user account disabled"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"Please contact your network administrator for assistance.", @"Error Dialog details when user account disabled"));
					break;
				}
				case ns1__StatusCodes__SimiasLoginDisabled:
				{
					NSBeginAlertSheet(NSLocalizedString(@"The iFolder account is disabled", @"Error dialog message when iFolder account disabled"), 
					NSLocalizedString(@"OK", @"Error dialog button when iFolder account disabled"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"Please contact your network administrator for assistance.", @"Error dialog details when iFolder account disabled"));
					break;
				}
				case ns1__StatusCodes__AccountLockout:		// AccountLockout
				{
					NSBeginAlertSheet(NSLocalizedString(@"The user account has been locked out", @"Error dialog message when account is locked"), 
					NSLocalizedString(@"OK", @"Error dialog button when account is locked"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"Please contact your network administrator for assistance.", @"Error dialog details when account is locked"));
					break;
				}
				case ns1__StatusCodes__UnknownDomain:		// UnknownDomain
				{
					NSBeginAlertSheet(NSLocalizedString(@"The server specified can not be located", @"Error dialog message when domain is unknown"), 
					NSLocalizedString(@"OK", @"Error dialog button when domain is unknown"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"Verify the information entered by you and try again. If the problem still persists, contact your network administrator.", @"Error dialog details when domain is unknown"));
					break;
				}
				case ns1__StatusCodes__InternalException:		// InternalException
				case ns1__StatusCodes__MethodNotSupported:	// MethodNotSupported
				case ns1__StatusCodes__Timeout:	// Timeout
				case ns1__StatusCodes__AmbiguousUser:		// AmbiguousUser
				case ns1__StatusCodes__Unknown:	// Unknown
				{
					NSBeginAlertSheet(NSLocalizedString(@"An error was encountered while connecting to the iFolder server", @"General error dialog message for login"), 
					NSLocalizedString(@"OK", @"General error dialog button for login"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"Verify the information entered by you and try again. If the problem still persists, contact your network administrator.", @"General error dialog details for login"));
					break;
				}
				case ns1__StatusCodes__UserAlreadyMoved:
				{
					//[simiasService RemoveCertFromTable:[selectedDomain hostURL]];
					[self loginoutClicked:self];
				}
			}

			[authStatus release];
		}
		@catch (NSException *e)
		{
			ifexconlog(@"AccountsController:loginAccount", e);
			NSBeginAlertSheet(NSLocalizedString(@"An error was encountered while connecting to the iFolder server", @"Exception error dialog message for login"), 
			NSLocalizedString(@"OK", @"Exception error button title for login"), nil, nil, 
			parentWindow, nil, nil, nil, nil, 
			NSLocalizedString(@"Verify the information entered by you and try again. If the problem still persists, contact your network administrator.", @"Exception error dialog details for login"));
		}
	}
}



//========================================================================
// logoutAccount
// Logout the account
//========================================================================
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
				@try
			    {
					if(![simiasService GetRememberPassPhraseOption:[selectedDomain ID]])
					{
						[simiasService StorePassPhrase:[selectedDomain ID] PassPhrase:@"" Type:None andRememberPP:NO];
					}
			    }
				@catch(NSException *ex)
			    {
					ifexconlog(@"Exception in StorePassPhrase:AccountsController::logoutAccount",ex);
			    }
				
				[loginout setEnabled:YES];
				[loginout setTitle:NSLocalizedString(@"Log In", @"Login/Logout Button on Accounts Dialog")];
				[state setStringValue:NSLocalizedString(@"Logged out", @"Login/Logout status on Accounts dialog")];
				[selectedDomain setValue:[NSNumber numberWithBool:NO] forKeyPath:@"properties.authenticated"];
				[host setEnabled:YES];
			}
			else
			{
				ifconlog2(@"Error returned from LogoutFromRemoteDomain: %d", statusCode);
				NSBeginAlertSheet(NSLocalizedString(@"An error was encountered while logging out of the iFolder server.", @"Error Logout Dialog message on Accounts dialog"), 
				NSLocalizedString(@"OK", @"Error Logout Dialog button on Accounts dialog"), nil, nil, 
				parentWindow, nil, nil, nil, nil, 
				NSLocalizedString(@"If the problem persists, please contact your network administrator.", @"Error Logout Dialog details on Accounts dialog"));
				[host setEnabled:NO];
			}

			[authStatus release];
		}
		@catch (NSException *e)
		{
			ifexconlog(@"LogoutFromRemoteDomain", e);
			NSBeginAlertSheet(NSLocalizedString(@"An error was encountered while logging out of the iFolder server.", @"Error Logout Dialog message on Accounts dialog"), 
			NSLocalizedString(@"OK", @"Error Logout Dialog button on Accounts dialog"), nil, nil, 
			parentWindow, nil, nil, nil, nil, 
			NSLocalizedString(@"If the problem persists, please contact your network administrator.", @"Error Logout Dialog details on Accounts dialog"));
			[host setEnabled:NO];
		}
	}
}



//========================================================================
// activateAccount
// Activate the account
//========================================================================
- (void)activateAccount
{
	[self validateDomainAddress];

	if( ([[host stringValue] length] > 0) &&
		([[userName stringValue] length] > 0) &&
		([[password stringValue] length] > 0) )
	{
		unsigned int statusCode;
		AuthStatus *authStatus = nil;

		[[NSApp delegate] setupSimiasProxies:[host stringValue]];

		@try
		{
			
			
			NSString* HostName = [host stringValue];
				
			NSString* substring = nil;
			if([HostName hasPrefix:@"http://"] == TRUE)
				substring = [HostName substringFromIndex:7];
			else if([HostName hasPrefix:@"https://"] == TRUE)
				substring = [HostName substringFromIndex:8];
			else
				substring = HostName;
			
			NSLog(@"%@ is the substring",substring);
			[host setStringValue:[@"https://" stringByAppendingString:substring]];
			//NSString* newHost = [@"https://" stringByAppendingFormat:substring];
			//NSLog(@"%@ is the newHost",newHost);
			
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
					// Copying the windows code and just ignoring what happens here
					[simiasService LoginToRemoteDomain:[newDomain ID] usingPassword:[password stringValue]];
				}
				@catch (NSException *e)
				{
					ifexconlog(@"LoginToRemoteDomain", e);
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
							isFirstDomain = NO;
						}
						@catch(NSException *ex)
						{
							ifexconlog(@"SetDefaultDomain", ex);
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
							ifexconlog(@"SetDomainPassword", ex);
						}			
					}

					createMode = NO;			
					[domains addObject:newDomain];
					[accounts reloadData];

					NSMutableIndexSet    *childRows = [NSMutableIndexSet indexSet];
					[childRows addIndex:([domains count] - 1)];
					[accounts selectRowIndexes:childRows byExtendingSelection:NO];
					
					[[iFolderData sharedInstance] refresh:YES];
					
					if(statusCode == ns1__StatusCodes__SuccessInGrace)
					{
						NSBeginAlertSheet(NSLocalizedString(@"Expired Password", @"Grace Login Warning Dialog Title"), 
						NSLocalizedString(@"OK", @"Grace Login Warning Dialog button"), nil, nil, 
						parentWindow, nil, nil, nil, nil, 
						[NSString stringWithFormat:NSLocalizedString(@"Your password has expired.  You have %d grace logins remaining.", 
									@"Grace Login Warning Dialog message"), [[newDomain remainingGraceLogins] intValue] ]);
					}
					[[iFolderData sharedInstance] clientUpdates:[selectedDomain ID] showstatus:NO]; //Check for new client available or not
					
					[[iFolderData sharedInstance] checkForEncryption:[newDomain ID] atLogin:YES];

					[defaultiFolderController setDomainAndShow:[newDomain ID]];
					
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
						ifexconlog(@"GetCertificate", ex);
					}
					break;
				}
				case ns1__StatusCodes__UnknownUser:		// UnknownUser
				case ns1__StatusCodes__InvalidCredentials:		// InvalidCredentials
				case ns1__StatusCodes__InvalidPassword:		// InvalidPassword
				{
					NSBeginAlertSheet(NSLocalizedString(@"The username or password is invalid", @"Error Dialog message when password invalid"), 
					NSLocalizedString(@"OK", @"Error Dialog button when password invalid"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"Please verify the information entered and try again.", @"Error Dialog details when password invalid"));
					break;
				}
				case ns1__StatusCodes__AccountDisabled:		// AccountDisabled
				{
					NSBeginAlertSheet(NSLocalizedString(@"The user account is disabled", @"Error Dialog message when user account disabled"), 
					NSLocalizedString(@"OK", @"Error Dialog button when user account disabled"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"Please contact your network administrator for assistance.", @"Error Dialog details when user account disabled"));
					break;
				}
				case ns1__StatusCodes__SimiasLoginDisabled:
				{
					NSBeginAlertSheet(NSLocalizedString(@"The iFolder account is disabled", @"Error dialog message when iFolder account disabled"), 
					NSLocalizedString(@"OK", @"Error dialog button when iFolder account disabled"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"Please contact your network administrator for assistance.", @"Error dialog details when iFolder account disabled"));
					break;
				}
				case ns1__StatusCodes__AccountLockout:		// AccountLockout
				{
					NSBeginAlertSheet(NSLocalizedString(@"The user account has been locked out", @"Error dialog message when account is locked"), 
					NSLocalizedString(@"OK", @"Error dialog button when account is locked"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"Please contact your network administrator for assistance.", @"Error dialog details when account is locked"));
					break;
				}
				case ns1__StatusCodes__UnknownDomain:		// UnknownDomain
				{
					NSBeginAlertSheet(NSLocalizedString(@"The server specified can not be located", @"Error dialog message when domain is unknown"), 
					NSLocalizedString(@"OK", @"Error dialog button when domain is unknown"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"Verify the information entered by you and try again. If the problem still persists, contact your network administrator.", @"Error dialog details when domain is unknown"));
					break;
				}
				case ns1__StatusCodes__InternalException:		// InternalException
				case ns1__StatusCodes__MethodNotSupported:	// MethodNotSupported
				case ns1__StatusCodes__Timeout:	// Timeout
				case ns1__StatusCodes__AmbiguousUser:		// AmbiguousUser
				case ns1__StatusCodes__Unknown:	// Unknown
				{
					NSBeginAlertSheet(NSLocalizedString(@"An error was encountered while connecting to the iFolder server", @"General error dialog message for login"), 
					NSLocalizedString(@"OK", @"General error dialog button for login"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"Verify the information entered by you and try again. If the problem still persists, contact your network administrator.", @"General error dialog details for login"));
					break;
				}
			}
		}
		@catch (NSException *e)
		{
			ifexconlog(@"ConnectToDomain", e);
			if([[e reason] isEqualToString:@"DomainExistsError"])
			{
				NSBeginAlertSheet(NSLocalizedString(@"Account Already Exists", @"Attach Error Account Exists message"), 
					NSLocalizedString(@"OK", @"Attach Error Account Exists Button"), nil, nil, 
					parentWindow, nil, nil, nil, nil, 
					NSLocalizedString(@"An account for this server already exists for your current local login identity on this computer.  Only one account per server is allowed.", @"Attach Error Account Exists details"));
			}
			else
			{
				NSBeginAlertSheet(NSLocalizedString(@"An error was encountered while connecting to the iFolder server", @"Exception error message for login"), 
				NSLocalizedString(@"OK", @"Exception error button title for login"), nil, nil, 
				parentWindow, nil, nil, nil, nil, 
				NSLocalizedString(@"Verify the information entered by you and try again. If the problem still persists, contact your network administrator.", @"Exception error dialog details for login"));
			}
		}
	}
}

//========================================================================
// addAccount
// Method to create new account in the client
//========================================================================
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



//========================================================================
// removeAccount
// Remove the account from list
//========================================================================
- (IBAction)removeAccount:(id)sender
{
	[leaveDomainController showWindow:self
				showSystemName:[selectedDomain name]
				showServer:[selectedDomain host]
				showUserName:[selectedDomain userName]];
}


//========================================================================
// certSheetDidEnd
// Response to handle whether user accepted certificate or not
//========================================================================
- (void)certSheetDidEnd:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	SecCertificateRef certRef = (SecCertificateRef)contextInfo;
		
	if(returnCode)
	{
		@try
		{
			NSMutableString *serverName = nil;
			if(selectedDomain == nil)
			{
				serverName = [NSMutableString stringWithCapacity:[[host stringValue] length]+10];
				[serverName setString:[host stringValue]];
			}
			else
			{
				serverName = [NSMutableString stringWithCapacity:[[selectedDomain host] length]+10];
				[serverName setString:[selectedDomain host]];
			}
			
			//Replace or add https:// at the start of domain server
			//NSMutableString* serverName = [NSMutableString stringWithCapacity:[[host stringValue] length]+10];
			//[serverName setString:[host stringValue]];
			//NSMutableString* serverName = [NSMutableString stringWithCapacity:[[host stringValue] length]+10];
			//[serverName setString:[host stringValue]];

			NSRange httpAvailable = [[serverName lowercaseString] rangeOfString:@"http"];
			NSRange httpsAvailable = [[serverName lowercaseString] rangeOfString:@"https"];
			
			if(httpsAvailable.location != 0) //https not at starting
			{
				if(httpAvailable.location == 0) //http at starting
				{
					[serverName replaceCharactersInRange:httpAvailable withString:@"https"]; //replace http with https
				}
				else
				{
					[serverName insertString:@"https://" atIndex:0]; //if not http added add https
				}
				[host setStringValue:serverName];
			}
			
			[simiasService StoreCertificate:certRef forHost:[host stringValue]];
			[self loginoutClicked:self];
		}
		@catch(NSException *ex)
		{
			ifexconlog(@"StoreCertificate", ex);
		}						
	}
	else
	{
		NSRunAlertPanel(NSLocalizedString(@"Certifiate Error",@"Certificate Error Title"),
	                    NSLocalizedString(@"User did not accept certificate, not storing or authenticating",@"Didn't accept Certificate"),
						NSLocalizedString(@"OK",@"OKButton Text"),
						nil,nil);
		ifconlog1(NSLocalizedString(@"User did not accept certificate, not storing or authenticating",@"Didn't accept Certificate"));
	}
	
	if(certRef != NULL)
	{
		CFRelease(certRef);
	}
}


//----------------------------------------------------------------------------
// leaveSelectedDomain
// This will remove the domain through Simias. 
// At first, it will hold the selected domain ID to be removed and then removes
// the selected domain. Then it tries to get the next default domain id from Simias.
// If available, it iterates throught the available domains and sets the default 
// domain. Then it updates the domains array for removed domain and checks if any
// other domains are available. If no, sets the "isFirstDomain" to YES. Updates
// the UI.
//----------------------------------------------------------------------------
//========================================================================
// leaveSelectedDomain
// Delete the domain selected in tableview
//========================================================================
-(void)leaveSelectedDomain:(BOOL)localOnly
{
	NSString* selectedDomID = [selectedDomain ID];
	
	NSString* defaultDomainID = nil;
	int counter;
	
	@try
	{
		[simiasService LeaveDomain:[selectedDomain ID] withOption:localOnly];
	}
	@catch(NSException *ex)	
	{
		ifexconlog(@"LeaveDomain", ex);
	}
	
	if([selectedDomID isEqualToString:[defaultDomain ID]])
	{
		defaultDomainID = [simiasService GetDefaultDomainID];
	}
	
	// Delete current defaultDomain and set the new domain to be the default
	if(defaultDomainID != nil)
	 {
		for(counter=0;counter<[domains count];counter++)
		{
			iFolderDomain *dom = [domains objectAtIndex:counter];
			
			if([[dom ID] isEqualToString:defaultDomainID])
			{
				[defaultDomain setValue:[NSNumber numberWithBool:NO] forKeyPath:@"properties.isDefault"];
				defaultDomain = dom;
				[defaultDomain setValue:[NSNumber numberWithBool:YES] forKeyPath:@"properties.isDefault"];
			}
		}
    }
	
    [domains removeObject:selectedDomain];
    
	if([domains count] == 0)
	{
		isFirstDomain = YES;
	}

	[accounts reloadData];
	[accounts deselectAll:self];
	[[iFolderData sharedInstance] refresh:NO];
}


//========================================================================
// toggleActive
// To handle whether the domain must be set active or not
//========================================================================
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
			ifexconlog(@"SetDomainActive", ex);
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
			ifexconlog(@"SetDomainInactive", ex);
		}
	}
}



//========================================================================
// toggleDefault
// Method that will be called when default domain to be toggled
//========================================================================
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
				ifexconlog(@"SetDefaultDomain", ex);
			}
		}
	}
}



//========================================================================
// toggleSavePassword
// When checkbox for password changes, this method is called
//========================================================================
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
				ifconlog1(@"Saved password was nil, removing saved password...");
		}
		else
		{
			ifconlog1(@"Removing saved password...");
		}

		@try
		{
			[simiasService SetDomainPassword:[selectedDomain ID] password:newPassword];	
		}
		@catch(NSException *ex)
		{
			ifexconlog(@"SetDomainPassword", ex);
		}
	}
}

//========================================================================
// tableViewSelectionDidChange
// Delegate method that will be called when table view selection changes
//========================================================================
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

//========================================================================
// selectionShouldChangeInTable
// Change the selction in table view
//========================================================================
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


//========================================================================
// changeAccountResponse
// Handle the response sent by accounts dialog
//========================================================================
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
//========================================================================
// numberOfRowsInTableView
// Get the number of rows in tableView
//========================================================================
-(int)numberOfRowsInTableView:(NSTableView *)aTableView
{
	return [domains count];
}

//========================================================================
// tableView
// Get the object at row mentioned
//========================================================================
-(id)tableView:(NSTableView *)aTableView objectValueForTableColumn:(NSTableColumn *)aTableColumn row:(int)rowIndex
{
	return [[domains objectAtIndex:rowIndex] name];
}

//========================================================================
// tableView
// Set the object in table view at column and row specified
//========================================================================
-(void)tableView:(NSTableView *)aTableView setObjectValue:(id)anObject forTableColumn:(NSTableColumn *)aTableColumn row:(int)rowIndex
{
}

//========================================================================
// controlTextDidChange
// Delegate method called whenever edit boxes are edited/added with new text
//========================================================================
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

//========================================================================
// validateDomainAddress
// Validates domain address with no spaces 
//========================================================================
-(void)validateDomainAddress
{
	NSCharacterSet* charSet = [NSCharacterSet whitespaceCharacterSet];
	NSString* hostNameWithSpaces = [[host stringValue] stringByTrimmingCharactersInSet:charSet];
	NSMutableString* hostNameWithoutSpaces = [NSMutableString string];
	NSScanner* hostNameScanner = [NSScanner scannerWithString:hostNameWithSpaces];
	NSString* nameComponent = nil;
	
	do
	{
		if([hostNameScanner scanUpToCharactersFromSet:charSet intoString:&nameComponent])
		{
			[hostNameWithoutSpaces appendString:nameComponent];
		}
	}while(![hostNameScanner isAtEnd]);
	
	[host setStringValue:[hostNameWithoutSpaces stringByTrimmingCharactersInSet:charSet]];

	if([userName isEnabled])
		[userName setStringValue:[ [userName stringValue] stringByTrimmingCharactersInSet:charSet]];
}
@end
