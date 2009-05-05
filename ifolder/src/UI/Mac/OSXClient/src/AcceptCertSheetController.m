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
*                 $Author: <Creator>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        	Accept certificate dialog controller
*
*
*******************************************************************************/

#import "AcceptCertSheetController.h"

@implementation AcceptCertSheetController

//=============================================================
// initWithCert
// Initialize the window with the certificate
//=============================================================
- (AcceptCertSheetController *) initWithCert:(SecCertificateRef)CertRef forHost:(NSString *)Host
{
	if ((self = [super initWithWindowNibName:@"AcceptCert"]) != nil)
	{
		certRef = CertRef;
		host = Host;
		recoveryAgent = nil;
	}
	return self;
}

//=============================================================
// initWithCert
// Overloaded initialize the window with the certificate
//=============================================================
- (AcceptCertSheetController *) initWithCert:(SecCertificateRef)CertRef forRAgent:(NSString*)rAgent;
{
	if ((self = [super initWithWindowNibName:@"AcceptCert"]) != nil)
	{
		certRef = CertRef;
		recoveryAgent = rAgent;
		host = nil;
	}
	return self;
}

//===================================================================
// awakeFromNib
// When this class is loaded from the nib, startup simias and wait
// since our app isn't useful without simias
//===================================================================
-(void)awakeFromNib
{
	[certView setCertificate:certRef];
	[certView setDisplayTrust:NO];
	[certView setEditableTrust:NO];

	if(host == nil)	//Show Certificate for Recovery Agent
	{
		[messageTitle setStringValue:[NSString stringWithFormat:
			NSLocalizedString(@"Unable to Verify Identity \"%@\"", 
				@"Verify Encryption Certificate Title"), recoveryAgent]];

		[message setStringValue:[NSString stringWithFormat:
			NSLocalizedString(@"iFolder cannot verify the identity of the iFolder Server \"%@\".  The certificate for this iFolder Server was signed by an unknown certifying authority.  You might be connecting to a server that is pretending to be \"%@\" which could put your confidential information at risk.  Before accepting this certificate, you should check with your system administrator.  Do you want to accept this certificate permanently and continue to connect?", 
				@"Verify Encryption Certificate Message"), recoveryAgent,recoveryAgent]];
	}
	
	if(recoveryAgent == nil)	//Show Certificate for Host
	{
		[messageTitle setStringValue:[NSString stringWithFormat:
			NSLocalizedString(@"iFolder cannot verify the identity of the iFolder Server \"%@\".", 
				@"Accept Certificate Message"), host]];

		[message setStringValue:[NSString stringWithFormat:
			NSLocalizedString(@"The certificate for this iFolder Server was signed by an unknown certifying authority.  You might be connecting to a server that is pretending to be \"%@\" which could put your confidential information at risk.   Before accepting this certificate, you should check with your system administrator.  Do you want to accept this certificate permanently and continue to connect?", 
				@"Accept Certificate Details"), host]];
	}
}

//=============================================================
// accept
// Accept the certificate and return the code 1
//=============================================================
- (IBAction)accept:(id)sender
{
	//[[self window] orderOut:nil];
	[ [self window] close];
	[NSApp endSheet:[self window] returnCode:1];
}

//=============================================================
// decline
// Decline the certificate and return 0
//=============================================================
- (IBAction)decline:(id)sender
{
	[ [self window] close];
	//[[self window] orderOut:nil];
	[NSApp endSheet:[self window] returnCode:0];
}

@end
