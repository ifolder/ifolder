/* AboutBoxController */

#import <Cocoa/Cocoa.h>

@interface AboutBoxController : NSObject
{
    IBOutlet id copyrightField;
    IBOutlet id creditsField;
    IBOutlet id versionField;

    NSTimer *scrollTimer;
    float currentPosition;
    float maxScrollHeight;
    NSTimeInterval startTime;
    BOOL restartAtTop;
}

+ (AboutBoxController *)sharedInstance;
- (IBAction)showPanel:(id)sender;


@end
