/* MainWindowController */

#import <Cocoa/Cocoa.h>
#import <iFolderService.h>

@class LoginWindowController;  // Forward declaration

@interface MainWindowController : NSWindowController
{
	LoginWindowController	*_loginController;
	iFolderService			*webService;
//	NSMutableDictionary		*domains;
}

-(void)login:(NSString *)username withPassword:(NSString *)password toServer:(NSString *)server;

- (void)showLoginWindow;

- (IBAction)login:(id)sender;

@end
