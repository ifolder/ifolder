/* LeaveDomainSheetController */

#import <Cocoa/Cocoa.h>

@class AccountsController;


@interface LeaveDomainSheetController : NSWindowController
{
    IBOutlet NSButton			*leaveAll;
    IBOutlet AccountsController *accountsController;
	IBOutlet NSWindow			*prefsWindow;
	IBOutlet NSWindow			*leaveDomainSheet;
}

- (IBAction)showWindow:(id)sender;
- (IBAction)cancelLeaveDomain:(id)sender;
- (IBAction)leaveDomain:(id)sender;
@end
