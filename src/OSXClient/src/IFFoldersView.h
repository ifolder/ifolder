/* IFFoldersView */

#import <Cocoa/Cocoa.h>
#import "IFSubViewController.h"

@interface IFFoldersView : NSView
{
    IBOutlet NSButton *createButton;
    IBOutlet NSTableView *ifolderTable;
    IBOutlet IFSubViewController *owner;
}

- (IBAction)delete:(id)sender;
- (IBAction)new:(id)sender;
- (IBAction)refresh:(id)sender;
@end
