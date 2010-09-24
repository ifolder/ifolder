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
*                 $Modified by: Satyam <ssutapalli@novell.com> 11/02/2008 Removed windowDidLoad and implemented it directly before showWindow
*                 $Modified by: Satyam <ssutapalli@novell.com> 10/04/2008 Made public private, and fix for setting passphrase if "None" is selected
*                 $Modified by: Satyam <ssutapalli@novell.com> 30/05/2009 Changed "None" to "Server_Default"
*                 $Modified by: Satyam <ssutapalli@novell.com> 02/06/2009 Made iFolderDomain available in whole class and changed "None" in RA to "Server_Default"
*-----------------------------------------------------------------------------
* This module is used to:
*			This class is written in connection with Accepting pass phrase dialog. It is inherited from
*			NSWindowController which provides default functionality of window. It will receive pass
*			phrase from the user, and stores in simias
*******************************************************************************/

#import "iFolderEncryptController.h"
#import "simiasStub.h"
#import "SimiasService.h"
#import "applog.h"
#import "AcceptCertSheetController.h"
#import "AuthStatus.h"
#import "iFolderData.h"
#include "SecurityInterface/SFCertificatePanel.h"


@implementation iFolderEncryptController

SimiasService* simiasServiceHolder=nil;
NSString* domainIDHolder=nil;
static id encryptInstance = nil;

//=======================================================================
// encryptionInstance
// Forcing the nib to load by creating shared instance of the UI
//=======================================================================
+ (id)encryptionInstance
{
	if ( nil == encryptInstance )
	{	
		encryptInstance = [[[self class] alloc] init];
		[[encryptInstance window] center];
	}
		
	return encryptInstance;
}

//=======================================================================
// init
// This method is just like constructor for the user interface
//=======================================================================
- (id)init
{
	if (self = [super initWithWindowNibName:@"EnterPassPhrase" owner:self])
	{
	}	
	return self;
}

//==========================================================================
// okOK
// This method will be called whenever OK button is clicked on UI. In this
// get the RACertificate from simias and show it up if not authenticated. 
// When certificate is accepted, "certSheetDidEnd" function is called with
// required information for further processing. If no certificate is 
// selected, it should show an alert and handle it in "certSheetDidEnd"
//==========================================================================
- (IBAction) okOK:(id)sender
{
	recoveryAgentText = [recoveryAgent objectValueOfSelectedItem];
	
	if( [recoveryAgent indexOfSelectedItem] != -1 && ![recoveryAgentText isEqualToString:NSLocalizedString(@"Server_Default",@"Server_Default encrypt RA")] )
	{
		@try
		{
			SecCertificateRef certRef = [simiasServiceHolder GetRACertificateOnClient:domainIDHolder withRecoveryAgent:recoveryAgentText];

			AcceptCertSheetController *certSheet = [[AcceptCertSheetController alloc]
					initWithCert:certRef forRAgent:recoveryAgentText];  
			
			[NSApp beginSheet:[certSheet window] modalForWindow:[self window]
					modalDelegate:self didEndSelector:@selector(certSheetDidEnd:returnCode:contextInfo:) contextInfo:certRef];
		}
		@catch(NSException *ex)
		{
			ifexconlog(@"GetCertificate", ex);
		}
	}
	else
	{
		/*
		int option = NSRunAlertPanel(NSLocalizedString(@"No Recovery agent",@"No Encryption Certificate Title"),
						NSLocalizedString(@"There is no Recovery Agent selected. Encrypted data cannot be recovered later, if passphrase is lost. Do you want to continue?",
										  @"No Encryption Certificate Message"),
						NSLocalizedString(@"Yes",@"No Encryption Certificate button"),
						NSLocalizedString(@"No",@" No Certificate Alert panel button"),
						nil);
						
		if( option == NSAlertDefaultReturn )
		{*/
		
			[self certSheetDidEnd:nil returnCode:1 contextInfo:nil];
		//}
	}
}

//============================================================================
// certSheetDidEnd
// Process the information received related to certificate. 
// Get the public key of the certificate. Set the passphrase. If successfully
// set, close the dialog, clear the temp variables and come out.
//============================================================================
- (void) certSheetDidEnd:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{	
	NSString* publicKey = nil;
	NSString* tempRAName = [recoveryAgent objectValueOfSelectedItem];
	if(returnCode)
	{
		if(![tempRAName isEqualToString:NSLocalizedString(@"Server_Default",@"Server_Default encrypt RA")])
		{
			@try
			{
				publicKey = [simiasServiceHolder GetPublicKey:domainIDHolder forRecoveryAgent:recoveryAgentText];
			}
			@catch(NSException *ex)
			{
				ifexconlog(@"GetPublicKey", ex);
			}		
		}
		else
		{
			publicKey = [[iFolderData sharedInstance] getDefaultServerPublicKey:domainIDHolder forUser:nil];
			tempRAName = @"DEFAULT";
		}
		 
		int ppStatus;
	
		@try
		{
			AuthStatus* authStatus = [simiasServiceHolder SetPassPhrase:domainIDHolder passPhrase:[enterPassPhrase stringValue] 
										recoveryAgent:tempRAName andPublicKey:publicKey];

			ppStatus = [[authStatus statusCode] intValue];
		}
		@catch(NSException* ex)
		{
			ifexconlog(@"SetPassPhrase",ex);
		}
		
		if(ppStatus == ns1__StatusCodes__Success)
		{
			@try
			{
				[simiasServiceHolder StorePassPhrase:domainIDHolder PassPhrase:[enterPassPhrase stringValue] Type:Basic andRememberPP:[rememberPassPhrase state]];
				if( [simiasServiceHolder IsPassPhraseSet:domainIDHolder] == YES )
				{
					NSRunAlertPanel(NSLocalizedString(@"Passphrase set",@"Passphrase set"),
									NSLocalizedString(@"Successfully set the passphrase.",@"Passphrase success message"),
									NSLocalizedString(@"OK",@"OKButton Text"),
									nil,nil);
				}
				
				domainIDHolder = nil;
				simiasServiceHolder = nil;
				[NSApp stopModal];
				[enterPassPhrase setStringValue:@""];
				[retypePassPhrase setStringValue:@""];
				[[self window] orderOut:nil];;
			}
			@catch(NSException* ex)
			{
				NSRunAlertPanel(NSLocalizedString(@"Passphrase store error",@"Cannot store passphrase title"),
								NSLocalizedString(@"Unable to store Passphrase.",@"Cannot store passphrase message"),
								NSLocalizedString(@"Yes",@"Cannot store passphrase button"),
								nil,nil);
			}
		}
	}
	else
	{
		NSRunAlertPanel(NSLocalizedString(@"Certifiate Error",@"Certificate Error Title"),
	                    NSLocalizedString(@"User did not accept certificate, not storing or authenticating",@"Didn't accept Certificate"),
						NSLocalizedString(@"OK",@"OKButton Text"),
						nil,nil);
		
	}
}

//=======================================================================
// onCancel
// This method will be called when "cancel" button is clicked. It will 
// clear the variables and closes the UI
//=======================================================================
- (IBAction) onCancel:(id)sender
{
	@try
	{
		if(![simiasServiceHolder GetRememberPassPhraseOption:domainIDHolder])
		{
				[simiasServiceHolder StorePassPhrase:domainIDHolder PassPhrase:@"" Type:None andRememberPP:NO];
		}
	}
	@catch(NSException* ex)
	{
		ifexconlog(@"Cannot store empty passphrase:iFolderEncryptController::onCancel",ex);
	}
	
	domainIDHolder = nil;
	simiasServiceHolder = nil;
	[NSApp stopModal];
	[enterPassPhrase setStringValue:@""];
	[retypePassPhrase setStringValue:@""];
	[[self window] orderOut:nil];
}


//=======================================================================
// controlTextDidChange
// This method will be called when notification comes from the controls
// about text change. In this we validate the text controls.
//=======================================================================
-(void) controlTextDidChange:(NSNotification *)aNotification
{
	if( ([aNotification object] == enterPassPhrase) ||
				([aNotification object] == retypePassPhrase) )
	{
		if( [enterPassPhrase stringValue] != nil	&&  ![[enterPassPhrase stringValue] isEqualToString:@""] &&
			[retypePassPhrase stringValue] != nil	&&  ![[retypePassPhrase stringValue] isEqualToString:@""] &&
			[[enterPassPhrase stringValue] isEqualToString:[retypePassPhrase stringValue]] )
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
- (void) setAndShow:(SimiasService*)service andDomain:(NSString*)domain 
{
	domainIDHolder = domain;
	simiasServiceHolder = service;
	
	if(nil == simiasServiceHolder || nil == domainIDHolder)
	{
		return;
	}	
	
	domainInfo = [simiasServiceHolder GetDomainInformation:domainIDHolder];
	NSString *domIP = [domainInfo host];
	NSString *domName = [domainInfo name];
	
	[domainNameLabel setStringValue:[[[domName stringByAppendingString:@"("] stringByAppendingString:domIP] stringByAppendingString:@")"]];
	
	NSArray* recoveryAgents = nil;
	[recoveryAgent removeAllItems];
	[recoveryAgent addItemWithObjectValue:NSLocalizedString(@"Server_Default",@"Server_Default encrypt RA") ];
	
	if( (recoveryAgents = [simiasServiceHolder GetRAListOnClient:domainIDHolder]) != nil )
	{	
		[recoveryAgent addItemsWithObjectValues:recoveryAgents];
	}

	[recoveryAgent selectItemAtIndex:0];
	
	[okButton setEnabled:NO];
	
	[self showWindow:nil];
}

@end
