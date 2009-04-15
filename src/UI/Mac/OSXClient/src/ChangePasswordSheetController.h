/* ChangePasswordSheetController */

#import <Cocoa/Cocoa.h>
@class iFolderWindowController;

@interface ChangePasswordSheetController : NSWindowController
{
    IBOutlet NSButton *changeButton;
    IBOutlet id changePasswordSheet;
    IBOutlet NSTextField *domainID;
    IBOutlet NSSecureTextField *enterNewPassword;
    IBOutlet NSSecureTextField *enterOldPassword;
    IBOutlet NSPopUpButton *ifolderAccount;
    IBOutlet iFolderWindowController *iFolderWindowController;
    IBOutlet id mainWindow;
    IBOutlet NSButton *rememberPassword;
    IBOutlet NSSecureTextField *retypePassword;
}
- (IBAction)onCancel:(id)sender;
- (IBAction)onChange:(id)sender;
@end
