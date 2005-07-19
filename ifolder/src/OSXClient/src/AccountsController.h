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
@class iFolderService;
@class LeaveDomainSheetController;
@class VerticalBarView;

@interface AccountsController : NSObject
{
    IBOutlet NSTableView *accounts;
    IBOutlet NSButton *loginout;
    IBOutlet NSButton *addAccount;
    IBOutlet NSButton *defaultAccount;
    IBOutlet NSButton *enableAccount;
    IBOutlet NSTextField *host;
    IBOutlet NSTextField *name;
	IBOutlet NSTextField *state;
    IBOutlet NSSecureTextField *password;
    IBOutlet NSButton *rememberPassword;
    IBOutlet NSButton *removeAccount;
    IBOutlet NSTextField *userName;
    IBOutlet NSView *view;
	IBOutlet NSWindow	*parentWindow;
	IBOutlet LeaveDomainSheetController	*leaveDomainController;
	
	IBOutlet NSTextView		*domainDescription;
	IBOutlet NSTextField	*freeSpace;
	IBOutlet NSTextField	*usedSpace;
	IBOutlet NSTextField	*totalSpace;
	IBOutlet VerticalBarView	*vertBar;

	SimiasService		*simiasService;	
	iFolderService		*ifolderService;	

	NSMutableArray		*domains;
	iFolderDomain		*selectedDomain;
	iFolderDomain		*defaultDomain;

	BOOL				createMode;
	BOOL				isFirstDomain;
}

- (IBAction)loginoutClicked:(id)sender;
- (IBAction)addAccount:(id)sender;
- (IBAction)removeAccount:(id)sender;
- (IBAction)toggleActive:(id)sender;
- (IBAction)toggleDefault:(id)sender;
- (IBAction)toggleSavePassword:(id)sender;

- (void)activateAccount;
- (void)loginAccount;
- (void)logoutAccount;

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
- (void)certSheetDidEnd:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;


@end