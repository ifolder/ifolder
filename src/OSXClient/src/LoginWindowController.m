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
 
#import "LoginWindowController.h"
#import "iFolderDomain.h"
#import "iFolderApplication.h"
#import "SimiasService.h"
#import "AuthStatus.h"
#include "simiasStub.h"
#include "Security/Security.h"
#import "AcceptCertSheetController.h"
#include "SecurityInterface/SFCertificatePanel.h"


@implementation LoginWindowController


- (IBAction)cancel:(id)sender
{
	SimiasService *simiasService = nil;
	if([authDomainID length] > 0 )
	{
		NSLog(@"Disabling auto login on Domain: %@", authDomainID);
		
		simiasService = [[SimiasService alloc] init];

		@try
		{
			[simiasService DisableDomainAutoLogin:authDomainID];
		}
		@catch(NSException *e)
		{
			NSLog(@"Failed to disable auto login on Domain: %@", authDomainID);
			// not sure what to do here but just ignore it for now I guess
		}
	}
	
	if(authDomainID != nil)
	{
		[authDomainID release];
		authDomainID = nil;
	}
	
	if(authDomainHost != nil)
	{
		[authDomainHost release];
		authDomainHost = nil;
	}

	[[self window] orderOut:nil];
}



- (void)windowWillClose:(NSNotification *)aNotification
{
	if(authDomainID != nil)
	{
		[authDomainID release];
		authDomainID = nil;
	}

	if(authDomainHost != nil)
	{
		[authDomainHost release];
		authDomainHost = nil;
	}
	
}



- (IBAction)authenticate:(id)sender
{
	SimiasService *simiasService = nil;

	[passwordField selectText:self];
	
	if( ( [authDomainID length] > 0 ) &&
		( [[passwordField stringValue] length] > 0 ) )
	{
		simiasService = [[SimiasService alloc] init];

		if(authDomainHost != nil)
		{
			[[NSApp delegate] setupSimiasProxies:authDomainHost];
		}

		@try
		{
			AuthStatus *authStatus = [[simiasService LoginToRemoteDomain:authDomainID 
										usingPassword:[passwordField stringValue]] retain];

//			AuthStatus *authStatus = [[[NSApp delegate] authenticateToDomain:authDomainID 
//										withPassword:[passwordField stringValue]] retain];
										
			unsigned int statusCode = [[authStatus statusCode] unsignedIntValue];
			
			switch(statusCode)
			{
				case ns1__StatusCodes__Success:		// Success
				case ns1__StatusCodes__SuccessInGrace:		// SuccessInGrace
				{
					if( (authStatus != nil) && ([authStatus remainingGraceLogins] < [authStatus totalGraceLogins]) )
					{
						NSRunAlertPanel(
							NSLocalizedString(@"Expired Password", nil), 
							[NSString stringWithFormat:NSLocalizedString(@"Your password has expired.  You have %d grace logins remaining.", nil), 
								[authStatus remainingGraceLogins]],
							NSLocalizedString(@"OK", nil), nil, nil);
					}

					if(authDomainID != nil)
					{
						[authDomainID release];
						authDomainID = nil;
					}
					[[self window] orderOut:nil];								
					break;
				}
				case ns1__StatusCodes__InvalidCertificate:
				{
					@try
					{
						SecCertificateRef certRef = [simiasService GetCertificate:[serverField stringValue]];

						AcceptCertSheetController *certSheet = [[AcceptCertSheetController alloc]
								initWithCert:certRef forHost:authDomainHost];
						
						[NSApp beginSheet:[certSheet window] modalForWindow:[self window]
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
					NSBeginAlertSheet(NSLocalizedString(@"The username or password is invalid", @"Error Dialog message when password invalid"), 
					NSLocalizedString(@"OK", @"Error Dialog button when password invalid"), nil, nil, 
					[self window], nil, nil, nil, nil, 
					NSLocalizedString(@"Please verify the information entered and try again.", @"Error Dialog details when password invalid"));
					break;
				}
				case ns1__StatusCodes__AccountDisabled:		// AccountDisabled
				{
					NSBeginAlertSheet(NSLocalizedString(@"The user account is disabled", @"Error Dialog message when user account disabled"), 
					NSLocalizedString(@"OK", @"Error Dialog button when user account disabled"), nil, nil, 
					[self window], nil, nil, nil, nil, 
					NSLocalizedString(@"Please contact your network administrator for assistance.", @"Error Dialog details when user account disabled"));
					break;
				}
				case ns1__StatusCodes__SimiasLoginDisabled:
				{
					NSBeginAlertSheet(NSLocalizedString(@"The iFolder account is disabled", @"Error dialog message when iFolder account disabled"), 
					NSLocalizedString(@"OK", @"Error dialog button when iFolder account disabled"), nil, nil, 
					[self window], nil, nil, nil, nil, 
					NSLocalizedString(@"Please contact your network administrator for assistance.", @"Error dialog details when iFolder account disabled"));
					break;
				}
				case ns1__StatusCodes__AccountLockout:		// AccountLockout
				{
					NSBeginAlertSheet(NSLocalizedString(@"The user account has been locked out", @"Error dialog message when account is locked"), 
					NSLocalizedString(@"OK", @"Error dialog button when account is locked"), nil, nil, 
					[self window], nil, nil, nil, nil, 
					NSLocalizedString(@"Please contact your network administrator for assistance.", @"Error dialog details when account is locked"));
					break;
				}
				case ns1__StatusCodes__UnknownDomain:		// UnknownDomain
				{
					NSBeginAlertSheet(NSLocalizedString(@"The server specified can not be located", @"Error dialog message when domain is unknown"), 
					NSLocalizedString(@"OK", @"Error dialog button when domain is unknown"), nil, nil, 
					[self window], nil, nil, nil, nil, 
					NSLocalizedString(@"Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", @"Error dialog details when domain is unknown"));
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
					[self window], nil, nil, nil, nil, 
					NSLocalizedString(@"Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", @"General error dialog title for login"));
					break;
				}
			}
			[authStatus release];
		}
		@catch (NSException *e)
		{
			NSBeginAlertSheet(NSLocalizedString(@"An error was encountered while connecting to the iFolder server", @"Exception error message title for login"), 
			NSLocalizedString(@"OK", @"Exception error button title for login"), nil, nil, 
			[self window], nil, nil, nil, nil, 
			NSLocalizedString(@"Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", @"Exception error dialog details for login"));
		}

		if(simiasService != nil)
			[simiasService release];
	}
}




- (void)showLoginWindow:(id)sender withDomain:(iFolderDomain *)domain
{
	if(authDomainID == nil)
	{
		[serverField setStringValue:[domain name]];
		[userNameField setStringValue:[domain userName]];
		authDomainID = [[domain ID] retain];
		authDomainHost = [[domain host] retain];
	}

	[self showWindow:sender];
	// Make the app icon bounce
	[NSApp requestUserAttention:NSCriticalRequest];
}



- (void)certSheetDidEnd:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	SecCertificateRef certRef = (SecCertificateRef)contextInfo;
	
	if(returnCode)
	{
		@try
		{
			SimiasService *simiasService = [[SimiasService alloc] init];
			[simiasService StoreCertificate:certRef forHost:[serverField stringValue]];
			[self authenticate:self];
		}
		@catch(NSException *ex)
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



@end
