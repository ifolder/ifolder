#import "PreferencesController.h"

@implementation PreferencesController

static PreferencesController *prefsSharedInstance = nil;


+ (PreferencesController *)sharedInstance
{
	if(prefsSharedInstance == nil)
	{
		prefsSharedInstance = [[PreferencesController alloc] initWithWindowNibName:@"Preferences"];
	}

    return prefsSharedInstance;
}


- (void)windowWillClose:(NSNotification *)aNotification
{
	if(prefsSharedInstance != nil)
	{
		[prefsSharedInstance release];
		prefsSharedInstance = nil;
	}
}

- (IBAction)addAccount:(id)sender
{
}

- (IBAction)removeAccount:(id)sender
{
}

- (IBAction)showProperties:(id)sender
{
}

@end
