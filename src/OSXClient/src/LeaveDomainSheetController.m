#import "LeaveDomainSheetController.h"
#import "AccountsController.h"

@implementation LeaveDomainSheetController


- (IBAction) showWindow:(id)sender
{
	[leaveAll setState:NO];
	
	[NSApp beginSheet:leaveDomainSheet modalForWindow:prefsWindow
		modalDelegate:self didEndSelector:NULL contextInfo:nil];
}

- (IBAction)cancelLeaveDomain:(id)sender
{
	[leaveDomainSheet orderOut:nil];
	[NSApp endSheet:leaveDomainSheet];
}

- (IBAction)leaveDomain:(id)sender
{
	BOOL localOnly = ![leaveAll state];
	[accountsController leaveSelectedDomain:localOnly];

	[leaveDomainSheet orderOut:nil];
	[NSApp endSheet:leaveDomainSheet];
}

@end
