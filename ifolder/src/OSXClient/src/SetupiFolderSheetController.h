/* SetupiFolderSheetController */

#import <Cocoa/Cocoa.h>

@interface SetupiFolderSheetController : NSWindowController
{
    IBOutlet NSTextField *iFolderName;
    IBOutlet NSTextField *SharedBy;
    IBOutlet NSTextField *Rights;
    IBOutlet NSTextField *pathField;
	IBOutlet NSTextField *iFolderID;
	IBOutlet NSTextField *domainID;
	IBOutlet id setupSheet;
	IBOutlet id mainWindow;

}
- (IBAction) showWindow:(id)sender;
- (IBAction)browseForPath:(id)sender;
- (IBAction)cancelSetup:(id)sender;
- (IBAction)setupiFolder:(id)sender;


@end
