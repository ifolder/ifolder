#import "SetupiFolderSheetController.h"

@implementation SetupiFolderSheetController

- (IBAction) showWindow:(id)sender
{
	if( [ [iFolderID stringValue] length] > 0)
	{
		// Because we use the same dialog, clear out the path
		[pathField setStringValue:@""];
	
		[NSApp beginSheet:setupSheet modalForWindow:mainWindow
			modalDelegate:self didEndSelector:NULL contextInfo:nil];
	}
}

- (IBAction)browseForPath:(id)sender
{
	int result;
	NSOpenPanel *oPanel = [NSOpenPanel openPanel];
	
	[oPanel setAllowsMultipleSelection:NO];
	[oPanel setCanChooseDirectories:YES];
	[oPanel setCanChooseFiles:NO];
	NSString *lastPath = [pathField stringValue];
	if([lastPath length] > 0)
		result = [oPanel runModalForDirectory:lastPath file:nil types:nil];
	else
		result = [oPanel runModalForDirectory:NSHomeDirectory() file:nil types:nil];
	
	if (result == NSOKButton)
	{
		NSString *dirName = [oPanel directory];
		[pathField setStringValue:dirName];
	}
}

- (IBAction)cancelSetup:(id)sender
{
	[setupSheet orderOut:nil];
	[NSApp endSheet:setupSheet];
}

- (IBAction)setupiFolder:(id)sender
{
	if( ( [ [iFolderID stringValue] length] > 0) &&
		( [ [pathField stringValue] length] > 0 ) )
	{
		[[NSApp delegate] AcceptiFolderInvitation:[iFolderID stringValue]
							InDomain:[domainID stringValue]
							toPath:[pathField stringValue] ];

		[setupSheet orderOut:nil];
		[NSApp endSheet:setupSheet];
	}
}

@end
