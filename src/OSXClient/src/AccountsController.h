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

@class iFolderDomain;
@class SimiasService;
@class LeaveDomainSheetController;

@interface AccountsController : NSObject
{
    IBOutlet NSTableView *accounts;
    IBOutlet NSButton *activate;
    IBOutlet NSButton *addAccount;
    IBOutlet NSButton *defaultAccount;
    IBOutlet NSButton *enableAccount;
    IBOutlet NSTextField *host;
    IBOutlet NSTextField *name;
    IBOutlet NSSecureTextField *password;
    IBOutlet NSButton *rememberPassword;
    IBOutlet NSButton *removeAccount;
    IBOutlet NSTextField *userName;
    IBOutlet NSView *view;
	IBOutlet NSWindow	*parentWindow;
	IBOutlet LeaveDomainSheetController	*leaveDomainController;

	SimiasService		*simiasService;	
	NSMutableArray		*domains;
	iFolderDomain		*selectedDomain;
	iFolderDomain		*defaultDomain;

	BOOL				createMode;
}

- (IBAction)activateAccount:(id)sender;
- (IBAction)addAccount:(id)sender;
- (IBAction)removeAccount:(id)sender;
- (IBAction)toggleActive:(id)sender;
- (IBAction)toggleDefault:(id)sender;
- (IBAction)toggleSavePassword:(id)sender;

- (void)awakeFromNib;

-(void)refreshData;

// NSTableViewDelegates
- (BOOL)selectionShouldChangeInTableView:(NSTableView *)aTableView;

// delegate for error sheet
- (void)changeAccountResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;

// Delegates for TableView
-(int)numberOfRowsInTableView:(NSTableView *)aTableView;
-(id)tableView:(NSTableView *)aTableView objectValueForTableColumn:(NSTableColumn *)aTableColumn row:(int)rowIndex;
-(void)tableView:(NSTableView *)aTableView setObjectValue:(id)anObject forTableColumn:(NSTableColumn *)aTableColumn row:(int)rowIndex;
- (void)tableViewSelectionDidChange:(NSNotification *)aNotification;

// Delegates for text view
- (void)controlTextDidChange:(NSNotification *)aNotification;

-(void)leaveSelectedDomain:(BOOL)localOnly;

@end
