//
//  CreateiFolderSheetController.h
//  iFolder
//
//  Created by Calvin Gaisford on 12/18/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

#import <Cocoa/Cocoa.h>


@interface CreateiFolderSheetController : NSWindowController 
{
	IBOutlet id createSheet;
	IBOutlet id mainWindow;
	IBOutlet NSComboBox *domainSelector;
	IBOutlet NSTextField *pathField;
	IBOutlet NSTextField *domainIDField;
}

- (IBAction) showWindow:(id)sender;
- (IBAction) cancelCreating:(id)sender;
- (IBAction) createiFolder:(id)sender;
- (IBAction) browseForPath:(id)sender;

@end
