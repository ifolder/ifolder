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

@class iFolderDomain;

@interface LoginWindowController : NSWindowController
{
    IBOutlet NSSecureTextField *passwordField;
    IBOutlet NSTextField *serverField;
    IBOutlet NSTextField *userNameField;

	NSString *authDomainID;
	NSString *authDomainHost;
}


- (IBAction)cancel:(id)sender;
- (IBAction)authenticate:(id)sender;

- (void)showLoginWindow:(id)sender withDomain:(iFolderDomain *)domain;
- (void)certSheetDidEnd:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;

@end
