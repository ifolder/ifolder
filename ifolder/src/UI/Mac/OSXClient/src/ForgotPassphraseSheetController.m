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
*                 $Author: Satyam <ssutapalli@novell.com> 02/06/2009	Forgot passphrase controller
*-----------------------------------------------------------------------------
* This module is used to:
*			This is used to handle forgot pass phrase handlers
*******************************************************************************/

#import "iFolderApplication.h"
#import "ForgotPassphraseSheetController.h"
#import "iFolderData.h"
#import "iFolderDomain.h"
#import "applog.h"
#import "AuthStatus.h"

@implementation ForgotPassphraseSheetController


-(void)awakeFromNib
{
	[domainID bind:@"value" toObject:[[iFolderData sharedInstance] domainArrayController]
	   withKeyPath:@"selection.properties.ID" options:nil];
	
	[ifolderAccount bind:@"contentValues" toObject:[[iFolderData sharedInstance] domainArrayController]
			 withKeyPath:@"arrangedObjects.properties.name" options:nil];
	
	[ifolderAccount bind:@"selectedIndex" toObject:[[iFolderData sharedInstance] domainArrayController]
			 withKeyPath:@"selectionIndex" options:nil];	
	
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:enterNewPP];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:retypePP];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:userName];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:password];
	statusCode = 9999;
}


- (IBAction)onAccountChange:(id)sender
{
	NSString* defaultRAName = @"DEFAULT";
	NSString* raName = [[iFolderData sharedInstance] getRAName:[domainID stringValue]];
	NSLog(@"%@ is the raName",raName);
	
	if([raName isEqualToString:defaultRAName] == YES)
	{
		[userName setStringValue:@""];
		[password setStringValue:@""];
		[enterNewPP setStringValue:@""];
		[retypePP setStringValue:@""];
		[userName setEnabled:YES];
		[password setEnabled:YES];
		[enterNewPP setEnabled:YES];
		[retypePP setEnabled:YES];
		
		[resetButton setEnabled:NO];			
	}
	else
	{
		[userName setStringValue:@""];
		[password setStringValue:@""];
		[enterNewPP setStringValue:@""];
		[retypePP setStringValue:@""];
		[userName setEnabled:NO];
		[password setEnabled:NO];
		[enterNewPP setEnabled:NO];
		[retypePP setEnabled:NO];
		[resetButton setEnabled:NO];
		NSRunAlertPanel(NSLocalizedString(@"Cannot reset passphrase",@"Dont support title"),NSLocalizedString(@"This iFolder account does not support server side passphrase recovery. To reset your passphrase, use the Export, Key Recovery and Import options from the Security Menu.",@"Dont support message"),NSLocalizedString(@"OK",@"OKButton Text"),nil,nil);
	}
}

- (IBAction)onCancel:(id)sender
{
	if(statusCode != 9999 && statusCode != 0 && statusCode != 1)
	{
		NSRunAlertPanel(NSLocalizedString(@"Domain Logout",@"Domain logout title"),NSLocalizedString(@"Unable to authenticate to the domain. You have been logged out of the domain",@"Domain logout message"),NSLocalizedString(@"OK",@"OKButton Text"),nil,nil);
	}
	[[iFolderData sharedInstance] refresh:NO];
	[forgotPPSheet orderOut:nil];
	[NSApp endSheet:forgotPPSheet];
}

- (IBAction)setNewPP:(id)sender
{
	statusCode = 9999;
	@try
	{
		iFolderDomain* domID = [[iFolderData sharedInstance] getDomain:[domainID stringValue]];
	
		if([[userName stringValue] isEqualToString:[domID userName]] == YES)
		{
			[[iFolderData sharedInstance] logoutFromRemoteDomain:[domainID stringValue]];
			
			AuthStatus *authStatus = [[iFolderData sharedInstance] loginToRemoteDomain:[domainID stringValue] usingPassword:[password stringValue]];
			
			statusCode = [[authStatus statusCode] unsignedIntValue];

			if(statusCode == 0/*ns1__StatusCodes__Success*/ || statusCode == 1/*ns1__StatusCodes__SuccessInGrace*/)
			{
				[[iFolderData sharedInstance] exportRecoverImport:[domainID stringValue] forUser:[domID userID] withPassphrase:[enterNewPP stringValue]];
				
				[[iFolderData sharedInstance] clearPassPhrase: [domainID stringValue]];
				[[iFolderData sharedInstance] storePassPhrase: [domainID stringValue] PassPhrase:[enterNewPP stringValue]  andRememberPP:[rememberPP state]];
				
				[[iFolderData sharedInstance] refresh:NO];
				[forgotPPSheet orderOut:nil];
				[NSApp endSheet:forgotPPSheet];
			}
			else
			{
				NSRunAlertPanel(NSLocalizedString(@"Authentication failed",@"Authentication failed title"),[NSString stringWithFormat:  NSLocalizedString(@"Unable to authenticate to the domain \"%@\"",@"Authentication failed message"),[domID name]],NSLocalizedString(@"OK",@"OKButton Text"),nil,nil);
			}
		}
		else
		{
			NSRunAlertPanel(NSLocalizedString(@"Invalid Username",@"Invalid UserName"),NSLocalizedString(@"Verify the username that you entered",@"Check UserName"),NSLocalizedString(@"OK",@"OKButton Text"),nil,nil);
		}
	}
	@catch(NSException* ex)
	{
		ifexconlog(@"Error in export,recover and import:ForgotPassphrase::setNewPP",ex);
		NSRunAlertPanel(NSLocalizedString(@"Passphrase reset failed",@"ForgotPP reset failed title"),NSLocalizedString(@"Cannot reset passphrase. You have been logged out of the domain",@"ForgotPP reset failed message"),NSLocalizedString(@"OK",@"OKButton Text"),nil,nil);
	}
}

- (IBAction)showWindow:(id)sender
{
	[NSApp beginSheet:forgotPPSheet modalForWindow:mainWindow
		modalDelegate:self didEndSelector:NULL contextInfo:nil];	

	[self onAccountChange:nil];
}


- (void)textDidChange:(NSNotification *)aNotification
{
	if([aNotification object] == enterNewPP || [aNotification object] == retypePP || [aNotification object] == userName || [aNotification object] == password )
	{
		if(([[enterNewPP stringValue] compare:@""] != NSOrderedSame) &&
		   ([[retypePP stringValue] compare:@""] != NSOrderedSame) &&
			([[userName stringValue] compare:@""] != NSOrderedSame) &&
			([[password stringValue] compare:@""] != NSOrderedSame) &&
		   ([[enterNewPP stringValue] isEqualToString:[retypePP stringValue]] == YES ) &&
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
