#import "SyncLogWindowController.h"

@implementation SyncLogWindowController

-(void)awakeFromNib
{
	[super setShouldCascadeWindows:NO];
	[super setWindowFrameAutosaveName:@"ifolder_log_window"];
}


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
