#import "iFolderApplication.h"
#import "iFolderWindowController.h"
#import "SyncLogWindowController.h"
#import "LoginWindowController.h"
#import "iFolderPrefsController.h"
#import "AboutBoxController.h"
#import "Simias.h"



@implementation iFolderApplication

//===================================================================
// showSyncLog
// Shows the iFolder sync Log to the user
//===================================================================
- (IBAction)showSyncLog:(id)sender
{
	[[SyncLogWindowController sharedInstance] showWindow:self];
}




//===================================================================
// showAboutBox
// Shows the iFolder About box to the user
//===================================================================
- (IBAction)showAboutBox:(id)sender
{
	[[AboutBoxController sharedInstance] showPanel:sender];
}




//===================================================================
// showPrefs
// Shows the preferences window
//===================================================================
- (IBAction)showPrefs:(id)sender
{
	[[iFolderPrefsController sharedInstance] showWindow:self];
}




//===================================================================
// showiFolderWindow
// Shows the main iFolder Window
//===================================================================
- (IBAction)showiFolderWindow:(id)sender;
{
	[[iFolderWindowController sharedInstance] showWindow:self];
}




//===================================================================
// addLog
// Adds entry to the iFolder Sync Log
//===================================================================
- (void)initializeSimiasEvents
{
	SimiasEventInitialize();
}




//===================================================================
// addLog
// Adds entry to the iFolder Sync Log
//===================================================================
- (void)addLog:(NSString *)entry
{
	[SyncLogWindowController logEntry:entry];
}




//===================================================================
// showLoginWindow
// Shows the login window the the user
//===================================================================
- (void)showLoginWindow:(NSString *)domainID
{
	NSLog(@"Show the Login Window Here");
//	if(loginController == nil)
//	{
//		loginController = [[LoginWindowController alloc] initWithWindowNibName:@"LoginWindow"];
//	}

//	iFolderDomain *dom = [keyedDomains objectForKey:domainID];
	
//	[[loginController window] center];
//	[loginController showLoginWindow:self withHost:[dom host] withDomain:domainID];
}




//===================================================================
// addLogTS
// adds a log entry to the iFolderSync Log and it is thread safe
//===================================================================
- (void)addLogTS:(NSString *)entry
{
	[self performSelectorOnMainThread:@selector(addLog:) 
				withObject:entry waitUntilDone:YES ];	
}




//===================================================================
// showLoginWindowTS
// Shows the Login Window and is Thread Safe
//===================================================================
- (void)showLoginWindowTS:(NSString *)domainID
{
	[self performSelectorOnMainThread:@selector(showLoginWindow:) 
				withObject:domainID waitUntilDone:YES ];	
}




//===================================================================
// applicationDidFinishLaunching
// Application Delegate method, called when the application is done
// loading
//===================================================================
- (void)applicationDidFinishLaunching:(NSNotification*)notification
{
	[self addLog:@"Starting Simias Process"];
	NSLog(@"Starting Simias Process");
    [NSThread detachNewThreadSelector:@selector(startSimiasThread:)
        toTarget:self withObject:nil];

}




//===================================================================
// applicationWillTerminate
// Application Delegate method, called with the application is going
// to end
//===================================================================
- (void)applicationWillTerminate:(NSNotification *)notification
{
	[self addLog:@"Unregistering events..."];
	SimiasEventDisconnect();
	[self addLog:@"Shutting down Simias..."];
	[ [Simias getInstance] stop];
	[self addLog:@"Simias is shut down"];
}




//===================================================================
// startSimiasThread
// This is a method called on a new thread to start up the Simias
// process.  When done, this will call back on the main thread
// the simiasDidFinishStarting
//===================================================================
- (void)startSimiasThread:(id)arg
{
	NSLog(@"In Starting Simias Thread");
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];

	BOOL simiasRunning = NO;
/*
	@try
	{
		NSLog(@"Checking for existing Simias...");
		simiasRunning = [ifolderService Ping];
	}
	@catch (NSException *e)
	{
		NSLog(@"Simias is not running");
		simiasRunning = NO;
	}

	NSLog(@"Out of check...");
*/

	if(!simiasRunning)
	{
		iFolderService *threadWebService;
		
		threadWebService = [[iFolderService alloc] init];		
		
		// Startup simias Process
		[ [Simias getInstance] start];	

		
		while(!simiasRunning)
		{
			@try
			{
				simiasRunning = [threadWebService Ping];
			}
			@catch (NSException *e)
			{
				simiasRunning = NO;
				[NSThread sleepUntilDate:[[NSDate date] addTimeInterval:.5]];
			}
		}
	}

	[self performSelectorOnMainThread:@selector(simiasDidFinishStarting:) 
					withObject:nil waitUntilDone:YES ];

    [pool release];
}




//===================================================================
// simiasDidFinishStarting
// This method is called on the MainThread when the simias process
// is up and running.
//===================================================================
- (void)simiasDidFinishStarting:(id)arg
{
	[self addLog:@"initializing Simias Events"];
	NSLog(@"initializing Simias Events");
	[self initializeSimiasEvents];

/*
	[self addLog:@"iFolder reading all domains"];
	@try
	{
		int domainCount;
		NSArray *newDomains = [simiasService GetDomains:NO];

		for(domainCount = 0; domainCount < [newDomains count]; domainCount++)
		{
			iFolderDomain *newDomain = [newDomains objectAtIndex:domainCount];
			
			if( [[newDomain isDefault] boolValue] )
				defaultDomain = newDomain;

			[self addDomain:newDomain];
		}
		
		NSArray *newiFolders = [ifolderService GetiFolders];
		if(newiFolders != nil)
		{
			[ifoldersController addObjects:newiFolders];
		}
	}
	@catch (NSException *e)
	{
		[self addLog:@"Reading domains failed with exception"];
	}
*/
	
	// Setup the double click black magic
//	[iFolderTable setDoubleAction:@selector(doubleClickedTable:)];
	
	// TODO: Show all of the windows that were open when quit last
//	[self showWindow:self];
}








@end
