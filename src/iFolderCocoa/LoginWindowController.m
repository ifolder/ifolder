#import "LoginWindowController.h"

@implementation LoginWindowController

- (IBAction)cancel:(id)sender
{
	[[self window] orderOut:nil];
}

- (IBAction)login:(id)sender
{
	if( ( [ [userNameField stringValue] length] > 0 ) &&
		( [ [passwordField stringValue] length] > 0 ) &&
		( [ [serverField stringValue] length] > 0 ) )
	{
		[[NSApp delegate] login:[userNameField stringValue] withPassword:[passwordField stringValue] 
					toServer:[serverField stringValue] ];
	}
}

@end
