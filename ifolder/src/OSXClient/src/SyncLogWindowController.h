/* SyncLogWindowController */

#import <Cocoa/Cocoa.h>

@interface SyncLogWindowController : NSWindowController
{
	NSMutableArray					*synclog;
    IBOutlet NSArrayController *logController;
}

- (IBAction)clearLog:(id)sender;
- (IBAction)saveLog:(id)sender;

- (void)logEntry:(NSString *)entry;

@end
