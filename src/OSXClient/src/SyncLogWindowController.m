#import "SyncLogWindowController.h"

@implementation SyncLogWindowController

- (IBAction)clearLog:(id)sender
{
}

- (IBAction)saveLog:(id)sender
{
}

- (void)logEntry:(NSString *)entry
{
	[logController addObject:[NSString stringWithFormat:@"%@ %@", 
			[[NSDate date] descriptionWithCalendarFormat:@"%m/%d/%Y %H:%M:%S" timeZone:nil locale:nil], 
			entry]];
}


@end
