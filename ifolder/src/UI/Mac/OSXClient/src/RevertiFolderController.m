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
*                 $Author: Satya <ssutapalli@novell.com> 
*                 $Modified by: Satyam <ssutapalli@novell.com>  10/03/2007    Fix in revert-iFolder in getting ifolder id
*-----------------------------------------------------------------------------
* This module is used to:
*        New window for confirming to revert iFolder
*
*******************************************************************************/

#import "RevertiFolderController.h"
#import "iFolderWindowController.h"
#import "iFolderApplication.h"
#import "iFolder.h"
#import "iFolderDomain.h"
#import "iFolderData.h"

@implementation RevertiFolderController

//=================================================================================
// cancelRevert 
// Action called when cancel button is clicked.
// This will just close the panel.
//=================================================================================
- (IBAction)cancelRevert:(id)sender
{
    [setupSheet orderOut:nil];
	[NSApp endSheet:setupSheet];
}

//=================================================================================
// revertiFolder 
// Action called when OK button is clicked.
// This will first close the panel, if no exceptions found it will convert ifolder
// selected into normal one. Then it checks whether to deleter from server or not 
// proceed accordingly.
//=================================================================================

- (IBAction)revertiFolder:(id)sender
{
		[setupSheet orderOut:nil];
		[NSApp endSheet:setupSheet];
		
		iFolder *ifolder = [[iFolderWindowController sharedInstance] selectediFolder];
		
		int checkForServer = [deleteFromServer state];
		if(checkForServer)
		{
			@try
			{
				[[iFolderData sharedInstance] deleteiFolder:[ifolder ID] fromDomain:[ifolder DomainID]];
			}
			@catch (NSException *e)
			{
				NSRunAlertPanel(NSLocalizedString(@"iFolder Deletion Error", @"iFolder delete error dialog title"), NSLocalizedString(@"An error was encountered while deleting the iFolder.", @"iFolder delete error dialog message"), NSLocalizedString(@"OK", @"iFolder delete error dialog button"),nil, nil);
			}
		}
		else
		{
			@try
			{
				[[iFolderData sharedInstance] revertiFolder:[ifolder ID]];		
			}
			@catch (NSException *e)
			{
				NSRunAlertPanel(NSLocalizedString(@"iFolder Revert Error", @"Revert error Dialog title"), 
								NSLocalizedString(@"An error was encountered while reverting the iFolder.", @"Revert error Dialog message"), 
								NSLocalizedString(@"OK", @"Revert error Dialog button"),nil, nil);
			}
		}
		
	}

//=================================================================================
// showWindow 
// Action called to show up the Panel to confirm reverting ifolder.
//=================================================================================
- (IBAction)showWindow:(id)sender
{
	[deleteFromServer setState:0];
	[NSApp beginSheet:setupSheet modalForWindow:mainWindow
			modalDelegate:self didEndSelector:NULL contextInfo:nil];
}

@end
