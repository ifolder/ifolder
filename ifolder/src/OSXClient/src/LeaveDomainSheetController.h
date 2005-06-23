/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

#import <Cocoa/Cocoa.h>

@class AccountsController;


@interface LeaveDomainSheetController : NSWindowController
{
    IBOutlet NSButton			*leaveAll;
    IBOutlet AccountsController *accountsController;
	IBOutlet NSWindow			*prefsWindow;
	IBOutlet NSWindow			*leaveDomainSheet;

	IBOutlet NSTextField		*systemName;
	IBOutlet NSTextField		*server;
	IBOutlet NSTextField		*userName;
}

- (IBAction)showWindow:(id)sender showSystemName:(NSString *)SystemName
			showServer:(NSString *)Server showUserName:(NSString *)UserName;
- (IBAction)cancelLeaveDomain:(id)sender;
- (IBAction)leaveDomain:(id)sender;
@end
