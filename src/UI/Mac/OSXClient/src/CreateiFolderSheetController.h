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
*                 $Modified by: Satyam <ssutapalli@novell.com> 22/04/2008   Added secure sync for creating iFolder
*-----------------------------------------------------------------------------
* This module is used to:
*        	Create iFolder dialog
*
*******************************************************************************/

#import <Cocoa/Cocoa.h>
#import "iFolderDomain.h"

@class iFolderWindowController;

@interface CreateiFolderSheetController : NSWindowController 
{
	IBOutlet id createSheet;
	IBOutlet id mainWindow;
	IBOutlet NSPopUpButton	*domainSelector;
	IBOutlet NSTextField	*pathField;
	IBOutlet NSTextField	*domainIDField;
	IBOutlet NSMatrix		*securityMode;
	IBOutlet NSButton       *okButton;
	IBOutlet NSButton      *secureSync;
	IBOutlet iFolderWindowController *ifolderWindowController;
	
@private

	int secPolicy;
	enum SecurityState
	{
		encryption = 1,
		enforceEncryption = 2
	}secState;
}

- (IBAction) showWindow:(id)sender;
- (IBAction) cancelCreating:(id)sender;
- (IBAction) createiFolder:(id)sender;
- (IBAction) browseForPath:(id)sender;
- (IBAction) domainSelectionChanges:(id)sender;


@end
