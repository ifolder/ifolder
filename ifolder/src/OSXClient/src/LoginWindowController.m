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
#import "AuthStatus.h"
#include "simiasStub.h"


@implementation LoginWindowController


- (IBAction)cancel:(id)sender
{
	[[self window] orderOut:nil];
}


- (IBAction)authenticate:(id)sender
{
	[passwordField selectText:self];
	
	if( ( [authDomainID length] > 0 ) &&
		( [[passwordField stringValue] length] > 0 ) )
	{
		@try
		{
			AuthStatus *authStatus = [[[NSApp delegate] authenticateToDomain:authDomainID 
										withPassword:[passwordField stringValue]] retain];
										
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
					[[self window] orderOut:nil];									
					break;
				}
				case ns1__StatusCodes__InvalidCertificate:
				{
					NSLog(@"Invalid Certificate was found on the server");
					break;
				}
				case ns1__StatusCodes__UnknownUser:		// UnknownUser
				case ns1__StatusCodes__InvalidCredentials:		// InvalidCredentials
				case ns1__StatusCodes__InvalidPassword:		// InvalidPassword
				{
					NSBeginAlertSheet(NSLocalizedString(@"iFolder login failed", nil), 
					NSLocalizedString(@"OK", nil), nil, nil, 
					[self window], nil, nil, nil, nil, 
					NSLocalizedString(@"The user name or password is invalid.  Please try again.", nil));
					break;
				}
				case ns1__StatusCodes__AccountDisabled:		// AccountDisabled
				{
					NSBeginAlertSheet(NSLocalizedString(@"iFolder login failed", nil), 
					NSLocalizedString(@"OK", nil), nil, nil, 
					[self window], nil, nil, nil, nil, 
					NSLocalizedString(@"The user account is disabled.  Please contact your network administrator for assistance.", nil));
					break;
				}
				case ns1__StatusCodes__AccountLockout:		// AccountLockout
				{
					NSBeginAlertSheet(NSLocalizedString(@"iFolder login failed", nil), 
					NSLocalizedString(@"OK", nil), nil, nil, 
					[self window], nil, nil, nil, nil, 
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
					NSBeginAlertSheet(NSLocalizedString(@"iFolder login failed", nil), 
					NSLocalizedString(@"OK", nil), nil, nil, 
					[self window], nil, nil, nil, nil, 
					NSLocalizedString(@"An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", nil));
					break;
				}
			}
			[authStatus release];
		}
		@catch (NSException *e)
		{
			NSBeginAlertSheet(NSLocalizedString(@"iFolder login failed", nil), 
				NSLocalizedString(@"OK", nil), nil, nil, 
				[self window], nil, nil, nil, nil, 
				NSLocalizedString(@"An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator.", nil));
		}
	}
}




- (void)showLoginWindow:(id)sender withDomain:(iFolderDomain *)domain
{
	if(authDomainID == nil)
	{
		[serverField setStringValue:[domain name]];
		[userNameField setStringValue:[domain userName]];
		authDomainID = [[domain ID] retain];
	}

	[self showWindow:sender];
	// Make the app icon bounce
	[NSApp requestUserAttention:NSCriticalRequest];
}




@end
