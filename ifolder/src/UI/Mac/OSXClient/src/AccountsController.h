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
*                 $Author: Author: Calvin Gaisford <cgaisford@novell.com>
*                 $Modified by: Satyam <ssutapalli@novell.com>    10/04/2008    Added DefaultiFolder functionality
*-----------------------------------------------------------------------------
* This module is used to:
*        	Account dialog controller
*
*
*******************************************************************************/

#import <Cocoa/Cocoa.h>

@class iFolderDomain;
@class SimiasService;
@class iFolderService;
@class LeaveDomainSheetController;
@class DefaultiFolderSheetController;
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
	IBOutlet DefaultiFolderSheetController *defaultiFolderController;
	
	IBOutlet NSTextView		*domainDescription;
	IBOutlet NSTextField	*freeSpace;
	IBOutlet NSTextField	*usedSpace;
	IBOutlet NSTextField	*totalSpace;
	IBOutlet VerticalBarView	*vertBar;
	IBOutlet NSProgressIndicator *progressIndicator;

@private

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

//- (void)awakeFromNib;

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
-(void)validateDomainAddress;

@end
