/* MainWindowController */

#import <Cocoa/Cocoa.h>

@class LoginWindowController;  // Forward declaration

@interface MainWindowController : NSWindowController
{
	LoginWindowController	*_loginController;
}

-(void)login:(NSString *)username withPassword:(NSString *)password toServer:(NSString *)server;

- (void)showLoginWindow;

- (IBAction)login:(id)sender;

@end
