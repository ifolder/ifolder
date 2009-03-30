/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 | Author: 
 |***************************************************************************/


#import "AboutBoxController.h"

@implementation AboutBoxController

static AboutBoxController *aboutSharedInstance = nil;


- (id)init 
{
    if (aboutSharedInstance) 
	{
        [self dealloc];
    } 
	else
	{
        aboutSharedInstance = [super init];
    }
    
    return aboutSharedInstance;
}


+ (AboutBoxController *)sharedInstance
{
    return aboutSharedInstance ? aboutSharedInstance : [[self alloc] init];
}




- (void)windowWillClose:(NSNotification *)aNotification
{
	if(aboutSharedInstance != nil)
	{
		[aboutSharedInstance release];
		aboutSharedInstance = nil;
	}
}




- (IBAction)showPanel:(id)sender
{
    if (!versionField)
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

        theWindow = [versionField window];
        
        // Get the info dictionary (Info.plist)
        infoDictionary = [[NSBundle mainBundle] infoDictionary];

        // Get the localized info dictionary (InfoPlist.strings)
        localInfoBundle = CFBundleGetMainBundle();
        localInfoDict = (NSDictionary *)
                        CFBundleGetLocalInfoDictionary( localInfoBundle );

       // Setup the app name field
        appName = [localInfoDict objectForKey:@"CFBundleName"];
        
        // Set the about box window title
        [theWindow setTitle:[NSString stringWithFormat:NSLocalizedString(@"About %@", nil), appName]];
        
        // Setup the version field
        versionString = [infoDictionary objectForKey:@"CFBundleVersion"];
        [versionField setStringValue:[NSString stringWithFormat:NSLocalizedString(@"Version %@", nil), 
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
 
	   if (![[versionField window] isVisible])
    {
        currentPosition = 0;
        restartAtTop = NO;
        startTime = [NSDate timeIntervalSinceReferenceDate]; // + 3.0;
        [creditsField scrollPoint:NSMakePoint( 0, 0 )];
    }
    
    // Show the window
    [[versionField window] makeKeyAndOrderFront:nil];
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
