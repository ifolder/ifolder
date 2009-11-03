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
*                 $Author: Satyam <ssutapalli@novell.com> 
*                 $Modified by: Satyam <ssutapalli@novell.com>  10/03/2007    Fix in revert-iFolder in getting ifolder id
*-----------------------------------------------------------------------------
* This module is used to:
*        	New window for confirming to revert iFolder
*
*
*******************************************************************************/

/* RevertiFolderController */

#import <Cocoa/Cocoa.h>

@class iFolder;

@interface RevertiFolderController : NSWindowController
{
    IBOutlet NSButton *deleteFromServer;
    IBOutlet id mainWindow;
    IBOutlet id setupSheet;
	
	NSArrayController *ifoldersController;
}
- (IBAction)cancelRevert:(id)sender;
- (IBAction)revertiFolder:(id)sender;
- (IBAction)showWindow:(id)sender;


@end
