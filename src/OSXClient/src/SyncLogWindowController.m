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
	NSString *logMessage = [NSString stringWithFormat:@"%@ %@", [[NSDate date] descriptionWithCalendarFormat:@"%m/%d/%Y %H:%M:%S" timeZone:nil locale:nil], entry];
	[logController addObject:logMessage];
}


@end
