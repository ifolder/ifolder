/***********************************************************************
 |  $RCSfile$
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 |  Author: Calvin Gaisford <cgaisford@novell.com>
 | 
 ***********************************************************************/

#import <Cocoa/Cocoa.h>
#import "iFolderDomain.h"

@class iFolderWindowController;

@interface CreateiFolderSheetController : NSWindowController 
{
	IBOutlet id createSheet;
	IBOutlet id mainWindow;
	IBOutlet NSPopUpButton *domainSelector;
	IBOutlet NSTextField *pathField;
	IBOutlet NSTextField *domainIDField;
	IBOutlet iFolderWindowController *ifolderWindowController;
}

- (IBAction) showWindow:(id)sender;
- (IBAction) cancelCreating:(id)sender;
- (IBAction) createiFolder:(id)sender;
- (IBAction) browseForPath:(id)sender;


@end
