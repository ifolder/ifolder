/* LoginWindowController */

#import <Cocoa/Cocoa.h>
#import <MainWindowController.h>

@interface LoginWindowController : NSWindowController
{
    IBOutlet NSTextField *passwordField;
    IBOutlet NSTextField *serverField;
    IBOutlet NSTextField *userNameField;
}
- (IBAction)cancel:(id)sender;
- (IBAction)login:(id)sender;
@end
