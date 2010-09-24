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
*                 $Author: Satyam <ssutapalli@novell.com> 25/03/2008	Advanced Conflict Resolver
*                 $Modified by: 
*-----------------------------------------------------------------------------
* This module is used to:
*			This is used to handle advanced conflict resolutions
*******************************************************************************/

/* AdvConflictController */

#import <Cocoa/Cocoa.h>
@class iFolder;

@interface AdvConflictController : NSWindowController
{
    IBOutlet NSButton *browseButton;
    IBOutlet NSTextField *conflictBinPath;
    IBOutlet NSArrayController *conflictController;
    IBOutlet NSMatrix *conflictMatrix;
    IBOutlet NSTextField *conflictName;
    IBOutlet NSTableView *conflictTableView;
    IBOutlet NSTableColumn *conflictTypeColumn;
    IBOutlet NSButton *enableButton;
    IBOutlet NSTableColumn *folderColumn;
    IBOutlet NSTextField *hidenLocalFilePath;
    IBOutlet NSTextField *hidenServerFilePath;
    IBOutlet NSMatrix *ifolderMatrix;
    IBOutlet NSTextField *localFileDate;
    IBOutlet NSTextField *localFileName;
    IBOutlet NSTextField *localFileSize;
    IBOutlet NSTextField *locationName;
    IBOutlet id mainWindow;
    IBOutlet NSTableColumn *nameColumn;
    IBOutlet NSTextField *serverFileDate;
    IBOutlet NSTextField *serverFileName;
    IBOutlet NSTextField *serverFileSize;
    IBOutlet NSTextField *renameNewFileName;
    IBOutlet NSBox *renameBox;
    IBOutlet NSBox *serverVersionBox;
    IBOutlet NSBox *localVersionBox;
    IBOutlet id setupSheet;
	
	iFolder				*ifolder;
	BOOL localOnly;
	NSString* tempPath;
	NSArray* conflictFiles;
}

- (IBAction)browsePath:(id)sender;
- (IBAction)closePanel:(id)sender;
- (IBAction)conflictBinMatrixClicked:(id)sender;
- (IBAction)enableConflictBin:(id)sender;
- (IBAction)ifolderMatrixClicked:(id)sender;
- (IBAction)localFileOpen:(id)sender;
- (void)openConflictFile:(BOOL)local;
- (void)prepareToShow;
- (IBAction)saveFile:(id)sender;
- (IBAction)renameFile:(id)sender;
- (IBAction)serverFileOpen:(id)sender;
- (IBAction)showWindow:(id)sender;
- (void)showFile:(NSString*)pathOfFile;
@end
