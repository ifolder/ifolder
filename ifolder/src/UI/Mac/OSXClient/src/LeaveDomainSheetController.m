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
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import "LeaveDomainSheetController.h"
#import "AccountsController.h"

@implementation LeaveDomainSheetController

//===========================================================================
// showWindow
// Display the delete domain controller window
//===========================================================================
- (IBAction)showWindow:(id)sender showSystemName:(NSString *)SystemName
			showServer:(NSString *)Server showUserName:(NSString *)UserName
{
	[leaveAll setState:NO];

	[systemName setStringValue:SystemName];
	[server setStringValue:Server];
	[userName setStringValue:UserName];
	
	[NSApp beginSheet:leaveDomainSheet modalForWindow:prefsWindow
		modalDelegate:self didEndSelector:NULL contextInfo:nil];
}

//===========================================================================
// cancelLeaveDomain
// Cancel and close the dialog without removing the domain
//===========================================================================
- (IBAction)cancelLeaveDomain:(id)sender
{
	[leaveDomainSheet orderOut:nil];
	[NSApp endSheet:leaveDomainSheet];
}

//===========================================================================
// leaveDomain
// Delete the domain and close the window
//===========================================================================
- (IBAction)leaveDomain:(id)sender
{
	BOOL localOnly = ![leaveAll state];
	[accountsController leaveSelectedDomain:localOnly];

	[leaveDomainSheet orderOut:nil];
	[NSApp endSheet:leaveDomainSheet];
}

@end
