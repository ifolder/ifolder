//
//  CreateiFolderSheetController.h
//  iFolder
//
//  Created by Calvin Gaisford on 12/18/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import "iFolderDomain.h"


@interface CreateiFolderSheetController : NSWindowController 
{
	IBOutlet id createSheet;
	IBOutlet id mainWindow;
	IBOutlet NSPopUpButton *domainSelector;
	IBOutlet NSTextField *pathField;
	IBOutlet NSTextField *domainIDField;
	
	iFolderDomain *selectedDomain;
}

- (IBAction) showWindow:(id)sender;
- (IBAction) cancelCreating:(id)sender;
- (IBAction) createiFolder:(id)sender;
- (IBAction) browseForPath:(id)sender;

- (iFolderDomain *)selectedDomain;
- (void)setSelectedDomain:(iFolderDomain *)aSelectedDomain;

@end
