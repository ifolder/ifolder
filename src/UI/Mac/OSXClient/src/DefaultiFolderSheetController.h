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
*                 $Author: Satyam <ssutapalli@novell.com>    2/04/2008    Default iFolder functionality
*                 $Modified by: Satyam <ssutapalli@novell.com> 22/05/2008 Added Secure sync option during upload
*                 $Modified by: Satyam <ssutapalli@novell.com> 02/12/2008 Removed user defined class for hiding controls and using SDK method
*-----------------------------------------------------------------------------
* This module is used to:
*        To create default iFolder for a domain
*
*
*******************************************************************************/

/* DefaultiFolderSheetController */

#import <Cocoa/Cocoa.h>

@interface DefaultiFolderSheetController : NSWindowController
{
    IBOutlet NSButton *browseButton;
    IBOutlet NSButton *createButton;
    IBOutlet NSButton *createDefaultiFolder;
    IBOutlet id defaultiFolderWindow;
    IBOutlet NSTextField *ifolderPath;
    IBOutlet id mainWindow;
    IBOutlet NSMatrix *security;
	IBOutlet NSTextField *securityText;
	IBOutlet NSButton *defaultSecureSync;
	
	@private
	
	enum SecurityState
	{
		encryption = 1,
		enforceEncryption = 2
	}secState;
	int secPolicy;
	BOOL isUpload;
}
- (IBAction)onBrowse:(id)sender;
- (IBAction)onCancel:(id)sender;
- (IBAction)onCreate:(id)sender;
- (IBAction)onCreateDefaultiFolder:(id)sender;
- (void)setDomainAndShow:(NSString*)domainID;
- (void)setSecurityButtonState;
- (void)textDidChange:(NSNotification *)aNotification;
@end
