/* IFFoldersView */

#import <Cocoa/Cocoa.h>
#import "SubViewController.h"

@interface iFoldersView : NSView
{
    IBOutlet NSButton *createButton;
    IBOutlet NSTableView *ifolderTable;
    IBOutlet SubViewController *owner;
}

- (IBAction)delete:(id)sender;
- (IBAction)new:(id)sender;
- (IBAction)refresh:(id)sender;
@end
