#import "AboutBoxController.h"

@implementation AboutBoxController

static AboutBoxController *sharedInstance = nil;


- (id)init 
{
    if (sharedInstance) 
	{
        [self dealloc];
    } 
	else
	{
        sharedInstance = [super init];
    }
    
    return sharedInstance;
}


+ (AboutBoxController *)sharedInstance
{
    return sharedInstance ? sharedInstance : [[self alloc] init];
}



- (IBAction)showPanel:(id)sender
{
    if (!appNameField)
    {
        NSWindow *theWindow;
        NSString *creditsPath;
        NSAttributedString *creditsString;
        NSString *appName;
        NSString *versionString;
        NSString *copyrightString;
        NSDictionary *infoDictionary;
        CFBundleRef localInfoBundle;
        NSDictionary *localInfoDict;

       if (![NSBundle loadNibNamed:@"AboutBox" owner:self])
        {
        	// int NSRunCriticalAlertPanel(NSString *title, 
        	//		NSString *msg, NSString *defaultButton, 
        	//		NSString *alternateButton, NSString *otherButton, ...);
        	
            NSLog( @"Failed to load AboutBox.nib" );
            NSBeep();
            return;
        }

        theWindow = [appNameField window];
        
        // Get the info dictionary (Info.plist)
        infoDictionary = [[NSBundle mainBundle] infoDictionary];

        // Get the localized info dictionary (InfoPlist.strings)
        localInfoBundle = CFBundleGetMainBundle();
        localInfoDict = (NSDictionary *)
                        CFBundleGetLocalInfoDictionary( localInfoBundle );

       // Setup the app name field
        appName = [localInfoDict objectForKey:@"CFBundleName"];
        [appNameField setStringValue:appName];
        
        // Set the about box window title
        [theWindow setTitle:[NSString stringWithFormat:@"About %@", appName]];
        
        // Setup the version field
        versionString = [infoDictionary objectForKey:@"CFBundleVersion"];
        [versionField setStringValue:[NSString stringWithFormat:@"Version %@", 
                                                          versionString]];

        // Setup our credits
        creditsPath = [[NSBundle mainBundle] pathForResource:@"Credits" 
                                             ofType:@"rtf"];

        creditsString = [[NSAttributedString alloc] initWithPath:creditsPath 
                                                    documentAttributes:nil];
        
        [creditsField replaceCharactersInRange:NSMakeRange( 0, 0 ) 
                      withRTF:[creditsString RTFFromRange:
                               NSMakeRange( 0, [creditsString length] ) 
                                             documentAttributes:nil]];


        // Setup the copyright field
        copyrightString = [localInfoDict objectForKey:@"NSHumanReadableCopyright"];
        [copyrightField setStringValue:copyrightString];
        
        // Prepare some scroll info
        maxScrollHeight = [[creditsField string] length];
        
        // Setup the window
        [theWindow setExcludedFromWindowsMenu:YES];
        [theWindow setMenu:nil];
        [theWindow center];
    }
 
	   if (![[appNameField window] isVisible])
    {
        currentPosition = 0;
        restartAtTop = NO;
        startTime = [NSDate timeIntervalSinceReferenceDate]; // + 3.0;
        [creditsField scrollPoint:NSMakePoint( 0, 0 )];
    }
    
    // Show the window
    [[appNameField window] makeKeyAndOrderFront:nil];
}



- (void)windowDidBecomeKey:(NSNotification *)notification
{
    scrollTimer = [NSTimer scheduledTimerWithTimeInterval:1/4 
                           target:self 
                           selector:@selector(scrollCredits:) 
                           userInfo:nil 
                           repeats:YES];
}



- (void)windowDidResignKey:(NSNotification *)notification
{
    [scrollTimer invalidate];
}



- (void)scrollCredits:(NSTimer *)timer
{
    if ([NSDate timeIntervalSinceReferenceDate] >= startTime)
    {
        if (restartAtTop)
        {
            // Reset the startTime
            startTime = [NSDate timeIntervalSinceReferenceDate]; // + 3.0;
            restartAtTop = NO;
            
            // Set the position
            [creditsField scrollPoint:NSMakePoint( 0, 0 )];
            
            return;
        }
        if (currentPosition >= maxScrollHeight) 
        {
            // Reset the startTime
            startTime = [NSDate timeIntervalSinceReferenceDate]; // + 3.0;
            
            // Reset the position
            currentPosition = 0;
            restartAtTop = YES;
        }
        else
        {
            // Scroll to the position
            [creditsField scrollPoint:NSMakePoint( 0, currentPosition )];
            
            // Increment the scroll position
            currentPosition += 0.005;
        }
    }
}


@end
