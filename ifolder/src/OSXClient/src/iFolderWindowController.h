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
#import <iFolderService.h>
#import <SimiasService.h>


// Forward Declarations
@class CreateiFolderSheetController;
@class SetupiFolderSheetController;
@class PropertiesWindowController;
@class iFolder;


@interface iFolderWindowController : NSWindowController
{
    IBOutlet SetupiFolderSheetController	*setupSheetController;
    IBOutlet CreateiFolderSheetController	*createSheetController;
    IBOutlet NSTableView					*iFolderTable;
	IBOutlet NSTextField					*statusText;
	IBOutlet NSProgressIndicator			*statusProgress;
	IBOutlet NSTableColumn					*iconColumn;
	IBOutlet NSTableColumn					*nameColumn;
	IBOutlet NSTableColumn					*locationColumn;
	IBOutlet NSTableColumn					*statusColumn;

    NSArrayController						*ifoldersController;
	
	PropertiesWindowController				*propertiesController;
	
	NSToolbar								*toolbar;
	NSMutableDictionary						*toolbarItems;
	NSMutableArray							*toolbarItemKeys;
}

+ (iFolderWindowController *)sharedInstance;
- (void)windowWillClose:(NSNotification *)aNotification;

- (IBAction)customizeToolbar:(id)sender;
- (IBAction)deleteiFolder:(id)sender;
- (IBAction)doubleClickedTable:(id)sender;
- (IBAction)newiFolder:(id)sender;
- (IBAction)openiFolder:(id)sender;
- (IBAction)refreshWindow:(id)sender;
- (IBAction)revertiFolder:(id)sender;
- (IBAction)setupiFolder:(id)sender;
- (IBAction)showProperties:(id)sender;
- (IBAction)shareiFolder:(id)sender;
- (IBAction)synciFolder:(id)sender;

+(void)updateStatusTS:(NSString *)message;
-(void)updateStatus:(NSString *)message;

+(void)updateProgress:(double)curVal withMin:(double)minVal withMax:(double)maxVal;
-(void)updateProgress:(double)curVal withMin:(double)minVal withMax:(double)maxVal;

//- (IBAction)showHideToolbar:(id)sender;

- (void)revertiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;
- (void)deleteiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;


- (void)awakeFromNib;
- (void)dealloc;

- (void)addDomain:(iFolderDomain *)newDomain;

-(iFolder *)selectediFolder;

// menu validation
- (BOOL)validateUserInterfaceItem:(id)anItem;

//==========================================
// Toolbar Methods
//==========================================
- (void)setupToolbar;
- (NSToolbarItem *)toolbar:(NSToolbar *)toolbar
	itemForItemIdentifier:(NSString *)itemIdentifier
	willBeInsertedIntoToolbar:(BOOL)flag;
- (NSArray *)toolbarAllowedItemIdentifiers:(NSToolbar *)toolbar;
- (NSArray *)toolbarDefaultItemIdentifiers:(NSToolbar *)toolbar;


//==========================================
// Methods to create/delete/revert iFolders
//==========================================
- (void)createiFolder:(NSString *)path inDomain:(NSString *)domainID;
- (void)acceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath;
- (BOOL)handleiFolderError:(NSString *)error;

+(void)refreshDomainsTS;
-(void)refreshDomains:(id)args;




@end
