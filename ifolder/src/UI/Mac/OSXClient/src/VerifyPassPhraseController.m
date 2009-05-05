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
*                 $Author: Satyam <ssutapalli@novell.com> 07/01/2007	Receive's pass phrase from user
*                 $Modified by: 
*-----------------------------------------------------------------------------
* This module is used to:
*			This class is written in connection with Verify pass phrase dialog. It is inherited from
*			NSWindowController which provides default functionality of window. It will receive pass
*			phrase from the user, valides it via simias, stores permanently as per user options.
*******************************************************************************/


#import "VerifyPassPhraseController.h"
#import "simiasStub.h"
#import "SimiasService.h"
#import "applog.h"
#import "AuthStatus.h"

@implementation VerifyPassPhraseController

SimiasService* verSimiasServiceHolder = nil;
NSString* verDomainIDHolder = nil;
static id verifyInstance = nil;


//=======================================================================
// verifyPPInstance
// Forcing the nib to load by creating shared instance of the UI
//=======================================================================
+(id) verifyPPInstance
{
	if( nil == verifyInstance )
	{
		verifyInstance = [[[self class] alloc] init];
		[[verifyInstance window] center];
	}
	
	return verifyInstance;
}

//=======================================================================
// init
// This method is just like constructor for the user interface
//=======================================================================
-(id) init
{
	if(self = [super initWithWindowNibName:@"VerifyPassPhrase" owner:self])
	{
	}
	return self;
}

//=======================================================================
// windowDidLoad
// Notification sent when just before window loaded and going to display.
//=======================================================================
- (void) windowDidLoad
{
	[okButton setEnabled:NO];
}

//=======================================================================
// onOK
// This method will be called when user clicks "OK" button. In this we 
// validate the pass phrase and if success, store the pass phrase and 
// close the UI. Else display an alert.
//=======================================================================
- (IBAction)onOK:(id)sender
{
	int passPhraseStatus;
	@try
	{
		AuthStatus* authStatus = [verSimiasServiceHolder ValidatePassPhrase:verDomainIDHolder withPassPhrase:[enterPassPhrase stringValue]];
		passPhraseStatus =  [[authStatus statusCode] intValue];
	}
	@catch(NSException* e)
	{
		ifexconlog(@"VerifyPassPhrase:onOK", e);
	}

	
	switch(passPhraseStatus)
	{
		case ns1__StatusCodes__Success:		// Success
			@try
			{
				[verSimiasServiceHolder StorePassPhrase:verDomainIDHolder 
								PassPhrase:[enterPassPhrase stringValue]
								Type:Basic 
								andRememberPP:[savePassPhrase state]];
			}
			@catch(NSException *ex)
			{
				ifexconlog(@"VerifyPassPhrase:onOK:Success",ex);
			}
			
			verDomainIDHolder = nil;
			verSimiasServiceHolder = nil;
			[NSApp stopModal];
			[enterPassPhrase setStringValue:@""];
			[[self window] orderOut:nil];
			
			break;

		case ns1__StatusCodes__PassPhraseInvalid:	//Invalid passphrase
			NSRunAlertPanel(NSLocalizedString(@"Invalid Passphrase",@"Invalid passphrase title"),
							NSLocalizedString(@"The passphrase entered is invalid",@"Invalid passphrase"),
							NSLocalizedString(@"OK",@"Verify passphrase default button"),
							nil,nil);
			
			break;

		default:
			NSLog(NSLocalizedString(@"Invalid PassPhrase Status",@"Passphrase status"));
	}
	
}

//=======================================================================
// onCancel
// This method will be called when user clicks "Cancel" button. It will
// just closes the UI and de-initializes variables.
//=======================================================================
- (IBAction)onCancel:(id)sender
{
	@try
	{
		if(![verSimiasServiceHolder GetRememberPassPhraseOption:verDomainIDHolder])
		{
			[verSimiasServiceHolder StorePassPhrase:verDomainIDHolder PassPhrase:@"" Type:None andRememberPP:NO];
		}
	}
	@catch(NSException* ex)
	{
		ifexconlog(@"Cannot store empty passphrase:VerifyPassPhrase::onCancel",ex);
	}
	
	verDomainIDHolder = nil;
	verSimiasServiceHolder = nil;
	[enterPassPhrase setStringValue:@""];
	[NSApp stopModal];
	[[self window] orderOut:nil];;
}

//=======================================================================
// controlTextDidChange
// This method will be called when notification comes from the controls
// about text change. In this we validate the text controls.
//=======================================================================
-(void) controlTextDidChange:(NSNotification *)aNotification
{
	if([aNotification object] == enterPassPhrase)
	{
		if( ![[enterPassPhrase stringValue] isEqualToString:@""] )
		{
			[okButton setEnabled:YES];
		}
		else
		{
			[okButton setEnabled:NO];
		}
	}
}

//=======================================================================
// setAndShow
// This method will be called to show the UI. It also gets domain and 
// simias process to handle different tasks.
//=======================================================================
-(void) setAndShow:(SimiasService*)service andDomain:(NSString*) domain
{
	verDomainIDHolder = domain;
	verSimiasServiceHolder = service;
	
	if(nil != verSimiasServiceHolder && nil != verDomainIDHolder)
	{
		[self showWindow:nil];
	}
}

@end
