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
/* VerifyPassPhraseController */

#import <Cocoa/Cocoa.h>
@class SimiasService;

@interface VerifyPassPhraseController : NSWindowController
{
    IBOutlet NSSecureTextField *enterPassPhrase;
    IBOutlet NSButton *okButton;
    IBOutlet NSButton *savePassPhrase;

@private
	
	SimiasService* simiasService;
	NSString* domainID;
}

+(id) verifyPPInstance;
- (IBAction)onCancel:(id)sender;
- (IBAction)onOK:(id)sender;

//Method to check the textfield and activate OK button
-(void) controlTextDidChange:(NSNotification *)aNotification;
-(void) setAndShow:(SimiasService*)service andDomain:(NSString*) domain;
@end
