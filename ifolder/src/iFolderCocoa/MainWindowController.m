#import "MainWindowController.h"

@implementation MainWindowController

-(void)awakeFromNib
{
	[self showLoginWindow];
}

- (void)showLoginWindow
{
	if(_loginController == nil)
	{
		_loginController = [[LoginWindowController alloc] initWithWindowNibName:@"LoginWindow"];
	}
	[_loginController showWindow:self];
}

-(void)login:(NSString *)username withPassword:(NSString *)password toServer:(NSString *)server
{
	[[_loginController window] orderOut:nil];
}

- (IBAction)login:(id)sender
{
	[self showLoginWindow];
}


@end
