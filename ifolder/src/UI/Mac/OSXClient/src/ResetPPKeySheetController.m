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
*                 $Author: Satyam <ssutapalli@novell.com> 06/02/2007	For resetting passphrase
*                 $Modified by: Satyam <ssutapalli@novell.com> 13/05/2008 Removed storing of PP after reset
*                 $Modified by: Satyam <ssutapalli@novell.com> 14/08/2008 Enable/Disable controls depending on iFolder domain's encryption policy
*                 $Modified by: Satyam <ssutapalli@novell.com> 20/08/2008 Storing the PP after resetting instead of asking for re-login
*                 $Modified by: Satyam <ssutapalli@novell.com> 09/09/2008 Updating UI controls depending on domains encryption/reglar policy
*-----------------------------------------------------------------------------
* This module is used to:
*			This class will be used to reset the passphrase. It will ask for old passphrase,
*           asks for new passphrase with confirmation and then validates and update the 
*           new passphrase.
*******************************************************************************/

#import "ResetPPKeySheetController.h"
#import "iFolderData.h"
#import "applog.h"
#import "AcceptCertSheetController.h"
#import "AuthStatus.h"
#include "SecurityInterface/SFCertificatePanel.h"

@implementation ResetPPKeySheetController

//=======================================================================
// awakeFromNib
// Method to set default's related to UI
//=======================================================================
-(void)awakeFromNib
{
	[domainID bind:@"value" toObject:[[iFolderData sharedInstance] domainArrayController]
	   withKeyPath:@"selection.properties.ID" options:nil];
	
	[ifolderAccount bind:@"contentValues" toObject:[[iFolderData sharedInstance] domainArrayController]
			 withKeyPath:@"arrangedObjects.properties.name" options:nil];
	
	[ifolderAccount bind:@"selectedIndex" toObject:[[iFolderData sharedInstance] domainArrayController]
			 withKeyPath:@"selectionIndex" options:nil];	
	
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:enterPassphrase];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:newPassphrase];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:retypePassphrase];
}

//=======================================================================
// onAccountChange
// This is action method when ever ifolder account is changed. It will 
// update the Recovery agents list according to the account.
//=======================================================================
- (IBAction)onAccountChange:(id)sender
{
	[recoveryAgent removeAllItems];
	
	NSArray* recAgents = nil;
	recAgents = [[iFolderData sharedInstance] getRAListOnClient:[domainID stringValue]];
	
	if(recAgents != nil)
	{
		[recoveryAgent addItemsWithObjectValues:recAgents];
		[recoveryAgent setNumberOfVisibleItems:[recAgents count]+1];
	}
	else
	{
		[recoveryAgent setNumberOfVisibleItems:1];
	}
	
	[recoveryAgent addItemWithObjectValue:NSLocalizedString(@"None",@"None Text") ];
	[recoveryAgent selectItemAtIndex:0];
	
	if([[iFolderData sharedInstance] getSecurityPolicy:[domainID stringValue]] == 0 || 
	   ([[iFolderData sharedInstance] getSecurityPolicy:[domainID stringValue]] != 0 && ![[iFolderData sharedInstance] isPassPhraseSet:[domainID stringValue]])) 
	{
		[enterPassphrase setStringValue:@""];
		[newPassphrase setStringValue:@""];
		[retypePassphrase setStringValue:@""];
		
		[enterPassphrase setEnabled:NO];
		[newPassphrase setEnabled:NO];
		[recoveryAgent setEnabled:NO];
		[rememberPassphrase setEnabled:NO];
		[resetButton setEnabled:NO];
		[retypePassphrase setEnabled:NO];
	}
	else
	{
		[enterPassphrase setEnabled:YES];
		[newPassphrase setEnabled:YES];
		[recoveryAgent setEnabled:YES];
		[rememberPassphrase setEnabled:YES];
		[retypePassphrase setEnabled:YES];		
	}
}

//=======================================================================
// onCancel
// To take action when "cancel" button is clicked. This will just close
// the panel
//=======================================================================
- (IBAction)onCancel:(id)sender
{
	[resetPPSheet orderOut:nil];
	[NSApp endSheet:resetPPSheet];
}

//=======================================================================
// onReset
// This action method will be called when "Reset" button is clicked. At
// first it will validate the UI entries, and shows the certificate retrieved.
// Else warning/alert is displayed.
//=======================================================================
- (IBAction)onReset:(id)sender
{
	int option;
	NSString* recoveryAgentText = [recoveryAgent objectValueOfSelectedItem];
	
	
	if( [recoveryAgent indexOfSelectedItem] != -1 && [recoveryAgentText isEqualToString:NSLocalizedString(@"None",@"None Text")] )
	{
		option = NSRunAlertPanel(NSLocalizedString(@"No Recovery agent",@"No Encryption Certificate Title"),
								NSLocalizedString(@"There is no Recovery Agent selected. Encrypted data cannot be recovered later, if passphrase is lost. Do you want to continue?",@"No Encryption Certificate Message"),
								NSLocalizedString(@"Yes",@"No Encryption Certificate button"),
								NSLocalizedString(@"No",@" No Certificate Alert panel button"),
								nil);
		
		if( option != NSAlertDefaultReturn )
		{
			return;
		}
	}
	
	if( ([[enterPassphrase stringValue] compare:@""] != NSOrderedSame) &&
		([[newPassphrase stringValue] compare:@""] != NSOrderedSame) &&
		([[retypePassphrase stringValue] compare:@""] != NSOrderedSame) &&
		([[newPassphrase stringValue] isEqualToString:[retypePassphrase stringValue]] == YES ))
	{
		if([[iFolderData sharedInstance] validatePassPhrase:[domainID stringValue] andPassPhrase:[enterPassphrase stringValue]])
		{
			if(![recoveryAgentText isEqualToString:NSLocalizedString(@"None",@"None Text")])
			{
				SecCertificateRef certRef = [[iFolderData sharedInstance] getCertificate:[domainID stringValue] withRAName:recoveryAgentText];				
				if(certRef != NULL)
				{
					@try
					{
						AcceptCertSheetController *certSheet = [[AcceptCertSheetController alloc]
							initWithCert:certRef forRAgent:recoveryAgentText];  
						
						[NSApp beginSheet:[certSheet window] modalForWindow:resetPPSheet
							modalDelegate:self didEndSelector:@selector(certSheetDidEnd:returnCode:contextInfo:) contextInfo:nil];
					}
					@catch(NSException *ex)
					{
						ifexconlog(@"onReset", ex);
					}
				}
			}
			else
			{
				[self certSheetDidEnd:nil returnCode:1 contextInfo:nil];
			}
		}
		else
		{
			return;
		}
	}
	else
	{
		NSRunAlertPanel(NSLocalizedString(@"Check Passphrase",@"ResetPP Check PP Title"),
	                    NSLocalizedString(@"Please check whether passphrase's entered are not null, also check New passphrase and Retype passphrase are same",@"ResetPP Check PP MEssage"),
						NSLocalizedString(@"OK",@"OKButton Text"),
						nil,nil);
	}
}

//============================================================================
// certSheetDidEnd
// This will be the call back function from the certificate window.
// Process the information received related to certificate. 
// Get the public key of the certificate. Set the passphrase. If successfully
// set, close the dialog..
//============================================================================
- (void)certSheetDidEnd:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	if(returnCode)
	{
		if([[iFolderData sharedInstance] reSetPassPhrase:[domainID stringValue] oldPassPhrase:[enterPassphrase stringValue] passPhrase:[newPassphrase stringValue] withRAName:[recoveryAgent objectValueOfSelectedItem]])
		{
			[[iFolderData sharedInstance] clearPassPhrase: [domainID stringValue]];
			[[iFolderData sharedInstance] storePassPhrase: [domainID stringValue] PassPhrase:[newPassphrase stringValue]  andRememberPP:[rememberPassphrase state]];
			
			[resetPPSheet orderOut:nil];
			[NSApp endSheet:resetPPSheet];	
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
// showWindow
// This method will be used to show the import keys window and sets 
// default values in UI. 
//=======================================================================
- (IBAction)showWindow:(id)sender
{
	[self onAccountChange:nil];
	[enterPassphrase setStringValue:@""];
	[newPassphrase setStringValue:@""];
	[retypePassphrase setStringValue:@""];
	[resetButton setEnabled:NO];
	
	[NSApp beginSheet:resetPPSheet modalForWindow:mainWindow
		modalDelegate:self didEndSelector:NULL contextInfo:nil];
}

//=======================================================================
// textDidChange
// Selector method for validating the text fields and activating import
// button accordingly. The validation is that none of the text fields must
// be empty as well as new passphrase and retype passphrase are same.
//=======================================================================
- (void)textDidChange:(NSNotification *)aNotification
{
	if([aNotification object] == enterPassphrase || [aNotification object] == newPassphrase || [aNotification object] == retypePassphrase )
	{
		if(([[enterPassphrase stringValue] compare:@""] != NSOrderedSame) &&
		   ([[newPassphrase stringValue] compare:@""] != NSOrderedSame) &&
		   ([[retypePassphrase stringValue] compare:@""] != NSOrderedSame) &&
		   ([[retypePassphrase stringValue] isEqualToString:[newPassphrase stringValue]] == YES ) &&
		   [ifolderAccount indexOfSelectedItem] != -1 )
		{
			[resetButton setEnabled:YES];
		}
		else
		{
			[resetButton setEnabled:NO];
		}
	}
}

@end
