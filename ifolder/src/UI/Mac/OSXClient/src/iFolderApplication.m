/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Calvin Gaisford <cgaisford@novell.com>
*                 $Modified by: Satyam <ssutapalli@novell.com>  01-01-2008      Added notification for sync fail
*                 $Modified by: Satyam <ssutapalli@novell.com>  13-02-2008  Added auto refresh if remember password is selected
*                 $Modified by: Satyam <ssutapalli@novell.com>  22-05-208   Commented log statements
*                 $Modified by: Satyam <ssutapalli@novell.com>  18-06-2008  Added notification for creating new iFolder
*                 $Modified by: Satyam <ssutapalli@novell.com>  16-07-2008  UI Refresh timer added
*                 $Modified by: Satyam <ssutapalli@novell.com>  14-08-2008  Changed help file path
*                 $Modified by: Satyam <ssutapalli@novell.com>  17-09-2008  Commented the code which uses poBoxID of a domain
*                 $Modified by: Satyam <ssutapalli@novell.com>  08-10-2008  Handling processNodeEvents as threads to fix sync issue
*                 $Modified by: Satyam <ssutapalli@novell.com>  21-10-2008  Changed the way of getting language id in showHelp
*                 $Modified by: Satyam <ssutapalli@novell.com>  28-04-2009  Adding check not to display/log .DS_Store/Thumbs.db policy restriction details
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/
 
#import "iFolderApplication.h"
#import "iFolderWindowController.h"
#import "SyncLogWindowController.h"
#import "LoginWindowController.h"
#import "iFolderPrefsController.h"
#import "AboutBoxController.h"
#import "iFolderData.h"
#import "Simias.h"
#import "SimiasEventData.h"
#import "SMEvents.h"
#import "iFolderNotificationController.h"
#import "config.h"
#import "AuthStatus.h"
#include "simiasStub.h"
#include "applog.h"
#import "iFolderEncryptController.h"
#import "VerifyPassPhraseController.h"

#include <SystemConfiguration/SCDynamicStoreCopySpecific.h>
#include <SystemConfiguration/SCSchemaDefinitions.h>
#include <SystemConfiguration/SCDynamicStoreKey.h>
#include <CoreFoundation/CoreFoundation.h>
#include <Foundation/NSLocale.h>

@implementation iFolderApplication

static SCDynamicStoreRef dynStoreRef = NULL;
static CFRunLoopSourceRef dynStoreRunLoopRef = NULL;

void dynStoreCallBack(SCDynamicStoreRef store, CFArrayRef changedKeys, void *info);
//===================================================================
// awakeFromNib
// When this class is loaded from the nib, startup simias and wait
// since our app isn't useful without simias
//===================================================================
-(void)awakeFromNib
{
	simiasIsLoaded = NO;
	[self killSimias];
	
	// this baby will get cocoa objects ready for mutlitple threads
    [NSThread detachNewThreadSelector:@selector(enableThreads:)
        toTarget:self withObject:nil];

	// wait until cocoa is safely in multithreaded mode
	while(![NSThread isMultiThreaded])
	{
		// wait until it is
		ifconlog1(@"Waiting for app to enable multithreading");
		[NSThread sleepUntilDate:[[NSDate date] addTimeInterval:.5] ];
	}
	
    [NSThread detachNewThreadSelector:@selector(startSimias:)
        toTarget:self withObject:nil];
	
	readOnlyNotifications = [[NSMutableDictionary alloc] init];
	iFolderFullNotifications = [[NSMutableDictionary alloc] init];
	//syncFailNotifications = [[NSMutableDictionary alloc] init];	
}


-(void) dealloc
{
	[readOnlyNotifications release];
	[iFolderFullNotifications release];
	//[syncFailNotifications release];
	
	[super dealloc];
}

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
// showHelp
// Shows the main iFolder Window
//===================================================================
- (IBAction)showHelp:(id)sender
{
	NSUserDefaults* defs = [NSUserDefaults standardUserDefaults];
	NSArray* languages = [defs objectForKey:@"AppleLanguages"];
	NSString* preferredLang = [languages objectAtIndex:0];  
		
	NSString *helpPath = [NSString stringWithFormat:@"%s/share/ifolder3/help/%@/index.html", IFOLDER_PREFIX, preferredLang];
	
	if([[NSFileManager defaultManager] fileExistsAtPath:helpPath] == NO)
	{
		ifconlog1(@"Language help not found, defaulting to english (en)");
		helpPath = [NSString stringWithFormat:@"%s/share/ifolder3/help/en/index.html", IFOLDER_PREFIX];
	}

	[[NSWorkspace sharedWorkspace] openURL:[NSURL URLWithString:[NSString stringWithFormat:@"file://localhost%@", helpPath]]];
}



//===================================================================
// update
// check for updates
//===================================================================
- (IBAction)update:(id)sender
{
        int counter;
		ifconlog1(@"update..");
		SimiasService *simiasService = [[SimiasService alloc] init];
		
		NSArray *domains = [simiasService GetDomains:NO];
		BOOL connectPrompt=YES;
		BOOL latestClient= YES;
		clientUpdate   *cliUpdate= Nil;
	//	NSString status;
		if(domains != nil )
		{
			NSDictionary *infoDictionary;
			infoDictionary = [[NSBundle mainBundle] infoDictionary];
			NSString* currentVersion = [infoDictionary objectForKey:@"CFBundleVersion"];
			NSLog(@"currentVersion%@", currentVersion);
			iFolderService *ifService = [[iFolderService alloc] init];
			
			for(counter=0;counter<[domains count];counter++)
			{
				iFolderDomain *dom = [domains objectAtIndex:counter];
				NSLog(@"domain ID %@", [dom ID]);
				if([dom authenticated]){
					connectPrompt=NO;
					@try
					{
						cliUpdate = [ ifService CheckForMacUpdate:[dom ID] forCurrentVersion:currentVersion];
						if([cliUpdate Status] != Latest){
							cliUpdate=Nil;
							[[iFolderData sharedInstance] clientUpdates:[dom ID] ];
							latestClient= NO; //atleast one of the domain has update
						}	
					}@catch(NSException* ex){
						ifexconlog(@"Upgrade :Exception ", ex);
						NSRunAlertPanel(NSLocalizedString(@"iFolder client upgarde",@"iFolderClientUpgradeTitle"),
									NSLocalizedString(@"Unable to complete the operation .Try again",@"Unable to complete the operation message"),
									NSLocalizedString(@"OK",@"OK Button"), nil,nil);
		
					} @finally {
						[iFolderService dealloc];
					}
					}
			}
			if(connectPrompt){ //not logged in to any domain 
			NSRunAlertPanel(NSLocalizedString(@"iFolder client upgarde",@"iFolderClientUpgradeTitle"),
							NSLocalizedString(@"Unable to search for updates as you are not connected to a domain. Ensure that you are connected to a domain and try again",@"notconnectedMessage"),
							NSLocalizedString(@"OK",@"OK Button"), nil,nil);
			} else if(latestClient){
			NSRunAlertPanel(NSLocalizedString(@"iFolder client upgarde",@"iFolderClientUpgradeTitle"),
							NSLocalizedString(@"You are using the latest available version",@"clientstatus"),
							NSLocalizedString(@"OK",@"OK Button"), nil,nil);

			}
		}
		else 
		{  //no domain/account added yet 
			NSRunAlertPanel(NSLocalizedString(@"iFolder client upgarde",@"iFolderClientUpgradeTitle"),
							NSLocalizedString(@"Unable to search for updates as you are not connected to a domain. Ensure that you are connected to a domain and try again",@"notconnectedMessage"),
							NSLocalizedString(@"OK",@"OK Button"), nil,nil);
		}
			
	
	
}




//===================================================================
// initializeSimiasEvents
// Initializes the Simias Event client
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
	[logController addObject:[NSString stringWithFormat:@"%@ %@", 
			[[NSDate date] descriptionWithCalendarFormat:@"%Y-%m-%d %H:%M:%S" timeZone:nil locale:nil], 
			entry]];

	if([[logController arrangedObjects] count] > 500)
	{
		[logController removeObjectAtArrangedObjectIndex:0];
	}
}




//===================================================================
// clearLog
// clears the log
//===================================================================
- (void)clearLog
{
	if([[logController arrangedObjects] count] > 0)
	{
		NSIndexSet *allIndexes = [[NSIndexSet alloc]
								initWithIndexesInRange:
								NSMakeRange(0,[[logController arrangedObjects] count])];
		[logController removeObjectsAtArrangedObjectIndexes:allIndexes];
	}
}




//===================================================================
// logArrayController
// returns the NSArrayController for the log so windows can bind
// to it and display the log
//===================================================================
-(NSArrayController *)logArrayController
{
	return logController;
}




//===================================================================
// simiasIsRunning
// This flag lets other parts of the app know that simias is up and
// running.
//===================================================================
-(BOOL)simiasIsRunning
{
	return simiasIsLoaded;
}




//===================================================================
// showLoginWindow
// Shows the login window the the user
//===================================================================
- (void)showLoginWindow:(NSString *)domainID
{
	iFolderDomain *dom = [[iFolderData sharedInstance] getDomain:domainID];
	if(dom != nil)
	{
		if(loginWindowController == nil)
		{
			loginWindowController = 
				[[LoginWindowController alloc] initWithWindowNibName:@"LoginWindow"];
		}

		[[loginWindowController window] center];
		[loginWindowController showLoginWindow:self withDomain:dom];
	}
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
	ifconlog1(@"Loading application defaults");
	[self setupApplicationDefaults];

   if([[NSUserDefaults standardUserDefaults] integerForKey:PREFKEY_WINONSTARTUP] == 0) //Restore Window option
	{
		
	// yes, so only restore it was there before
		if([[NSUserDefaults standardUserDefaults] boolForKey:STATE_SHOWMAINWINDOW])		
		{
			ifconlog1(@"Showing iFolder Window");
			[self showiFolderWindow:self];
		}
		if([[NSUserDefaults standardUserDefaults] boolForKey:STATE_SHOWLOGWINDOW])		
		{
			ifconlog1(@"Showing Sync Log Window");
			[self showSyncLog:self];
		}
		

	}
	else if([[NSUserDefaults standardUserDefaults] integerForKey:PREFKEY_WINONSTARTUP] == 2) //Normal start up
	{
		[self showiFolderWindow:self];
		
		[[NSUserDefaults standardUserDefaults] setBool:YES forKey:STATE_SHOWMAINWINDOW];	
		[[NSUserDefaults standardUserDefaults] setBool:NO forKey:STATE_SHOWLOGWINDOW];
	}
	
	else
	{
		[[NSUserDefaults standardUserDefaults] setBool:NO forKey:STATE_SHOWMAINWINDOW];	//Hide main window on start up
		[[NSUserDefaults standardUserDefaults] setBool:NO forKey:STATE_SHOWLOGWINDOW];
		
	}

	[self setupProxyMonitor];

	if([self simiasIsRunning] == NO)
		[iFolderWindowController updateStatusTS:NSLocalizedString(@"Loading synchronization process...", @"Initial iFolder Status Message")];
	refreshTimer = [NSTimer scheduledTimerWithTimeInterval:300 target:self selector:@selector(refreshTimerCall:) userInfo:nil repeats:NO]; //5 minutes by default
}


//===================================================================
// setupApplicationDefaults
// This will load the defaults and if they are not there, set them
// up for the first time
//===================================================================
- (void)setupApplicationDefaults
{

	NSArray *keys	= [NSArray arrayWithObjects:	PREFKEY_WINPOS,
													PREFKEY_WINONSTARTUP,
													PREFKEY_CLICKIFOLDER,
													PREFKEY_NOTIFYIFOLDERS,
                                                                                                        PREFKEY_CREATEIFOLDER,
													PREFKEY_NOTIFYCOLL,
													PREFKEY_NOTIFYUSER,
													PREFKEY_NOTIFYBYINDEX,
				                                                                        PREFKEY_NOTIFYQUOTAVIOLATION,
				                                                                        PREFKEY_NOTIFYSIZEVIOLATION,
				                                                                        PREFKEY_NOTIFYEXCLUDEFILE,
				                                                                        PREFKEY_NOTIFYDISKFULL,
				                                                                        PREFKEY_NOTIFYPERMISSIONDENIED,
				                                                                        PREFKEY_NOTIFYPATHLENGTHEXCEEDS,
													PREFKEY_SYNCFAIL,
													STATE_SHOWMAINWINDOW,
													PREFKEY_NOTIFYSOUND,
													nil];

	NSArray *values = [NSArray arrayWithObjects:	[NSNumber numberWithBool:YES],
													[NSNumber numberWithInt:2],
													[NSNumber numberWithInt:0],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithInt:0],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													@"Glass",
													nil];

	NSDictionary *defaults = [[NSMutableDictionary alloc]
								initWithObjects:values forKeys:keys];

	[[NSUserDefaults standardUserDefaults] registerDefaults:defaults];
}




//===================================================================
// postSimiasInit
// This should be called by the startup thread that is starting simias
// so we know it's ok to go ahead and do stuff
//===================================================================
- (void)postSimiasInit:(id)arg
{
	runThreads = YES;
	simiasIsLoaded = YES;

	ifconlog1(@"reading Simias credentials");
	[[iFolderData sharedInstance] readCredentials];

	ifconlog1(@"initializing Simias Events");
	[self initializeSimiasEvents];


	ifconlog1(@"Creating and loading iFolderData");
	[[iFolderData sharedInstance] refresh:NO];

	// Startup the event processing thread
    [NSThread detachNewThreadSelector:@selector(simiasEventThread:)
        toTarget:self withObject:nil];

	[iFolderWindowController updateStatusTS:NSLocalizedString(@"Idle...", @"iFolder Window Status Message")];

	// If there are no domains when the app first launches, the show a dialog that
	// will help get an account created
	if([[iFolderData sharedInstance] getDomainCount] < 1)
	{
		int rc;
		
		rc = NSRunAlertPanel(NSLocalizedString(@"Set Up iFolder Account", @"Initial Setup Dialog Title"), 
							NSLocalizedString(@"To begin using iFolder, you must first set up an iFolder account.", @"Initial Setup Dialog Message"),
 							NSLocalizedString(@"Yes", @"Initial Setup Dialog Button"), 
 							NSLocalizedString(@"No", @"Initial Setup Dialog Button"),
 							nil);

		if(rc == NSAlertDefaultReturn)
		{
			[[iFolderPrefsController sharedInstance] showAccountsWindow];
		}
	}
	else
	{
		int counter;
		SimiasService *simiasService = [[SimiasService alloc] init];
		NSArray *domains = [simiasService GetDomains:NO];
		if(domains != nil)
		{
			for(counter=0;counter<[domains count];counter++)
			{
				iFolderDomain *dom = [domains objectAtIndex:counter];
				if(![simiasService GetRememberPassPhraseOption:[dom ID]])
				{
					[simiasService StorePassPhrase:[dom ID] PassPhrase:@"" Type:None andRememberPP:NO];
				}
			}
		}
		[simiasService release];
	}
}


//===================================================================
// startSimias
// This is a thread safe method that the simias startup routine calls
// once simias is up and running
//===================================================================
- (void)startSimias:(id)arg
{
	BOOL simiasStarted;

	ifconlog1(@"Starting Simias Process");	

	//[[Simias getInstance] stop];
	simiasStarted = [[Simias getInstance] start];
	if(simiasStarted == NO)
	{
		//[[Simias getInstance] stop];
		simiasStarted = [[Simias getInstance] start];
	}
	
	if(simiasStarted == YES)
	{
		ifconlog1(@"Simias Process Running");	
		[self performSelectorOnMainThread:@selector(postSimiasInit:) 
				withObject:nil waitUntilDone:NO ];			
	}
	else
	{
		ifconlog1(@"Simias Process Failed to start");	
	}
}


-(void)killSimias
{
	NSLog(@"Killing previous simias process if any");
	//[NSTask launchedTaskWithLaunchPath:@"/usr/bin/killall" arguments:[NSArray arrayWithObjects:@"mono", nil]];	
	NSTask *task = [[NSTask alloc] init];
	[task setLaunchPath: @"/bin/sh"];
	NSArray* arguments;
	arguments = [NSArray arrayWithObjects:@"-c",@"ps -e | grep -v grep | grep simias | awk '{print $1}' | xargs kill -9",nil];	
	[task setArguments:arguments];

    [task launch];
	[task release];
/*
	NSBundle *bundle = [NSBundle mainBundle];	
	 NSString *stripperPath;
     stripperPath = [bundle pathForAuxiliaryExecutable: @"killsimias.sh"];

    NSTask *task = [[NSTask alloc] init];
    [task setLaunchPath: @"/bin/sh"];
	NSArray* arguments;
	arguments = [NSArray arrayWithObjects:stripperPath,nil];	
	[task setArguments:arguments];

	NSPipe *readPipe = [NSPipe pipe];
    NSFileHandle *readHandle = [readPipe fileHandleForReading];
	[task setStandardOutput: readPipe];
	
    [task launch];
	
	NSData *data;
	data = [readHandle readDataToEndOfFile];
	
	if(data)
	{
		NSString *strippedString = [[NSString alloc] initWithData: data encoding: NSUTF8StringEncoding];
		NSLog(@"%@",strippedString);
		[strippedString release];
	}
	
    [task release];
	[NSThread sleepUntilDate:[[NSDate date] addTimeInterval:1]];
	*/
}


//===================================================================
// applicationWillTerminate
// Application Delegate method, called with the application is going
// to end
//===================================================================
- (void)applicationWillTerminate:(NSNotification *)notification
{
	if(simiasIsLoaded)
	{
		// Remove the monitor for proxy settings from the run loop and
		// release the reference
		if(dynStoreRunLoopRef != NULL)
		{
			CFRunLoopRemoveSource(CFRunLoopGetCurrent(), dynStoreRunLoopRef, kCFRunLoopCommonModes);
			CFRelease(dynStoreRunLoopRef);
			dynStoreRunLoopRef = NULL;
		}

		// Release the proxy setting monitor
		if(dynStoreRef != NULL)
		{
			CFRelease(dynStoreRef);
			dynStoreRef = NULL;
		}
		

		runThreads = NO;
		SimiasEventDisconnect();
				
		[self addLog:NSLocalizedString(@"Shutting down Simias...", @"Sync Log Message")];
		ifconlog1(@"Calling to stop Simias");			
		if([ [Simias getInstance] stop])
			ifconlog1(@"Simias has been stopped");			
		else
		{
			ifconlog1(@"Simias has NOT been stopped and so forcing to quit simias");			
			[self killSimias];
		}
		
		[self addLog:NSLocalizedString(@"Simias is shut down", @"Sync Log Message")];

	
	}
}




//===================================================================
// applicationShouldHandleReopen
// NSApplication Delegate method, called when the user reactivates
// iFolder by double-clicking the application in Finder or when they
// click on our already running icon in the dock.
//===================================================================
- (BOOL)applicationShouldHandleReopen:(NSApplication *)theApplication hasVisibleWindows:(BOOL)flag
{
	// Make sure the "My iFolders" window is showing
	[self showiFolderWindow:self];

	// Return NO to let the default behavior know that we already
	// took care of everything needed.
	return NO;
}


//============================================================================
// applicationShouldTerminate
// NSApplication Delegate method, called when the user quits the application
// This will ask whether to terminate or not. Depending on user's choice, it 
// will proceed.
//=============================================================================
- (NSApplicationTerminateReply)applicationShouldTerminate:(NSApplication *)sender
{
	int counter;
	int answer; 
	
	/*if( ![[iFolderData sharedInstance] ForceQuit] )
	{
		answer = NSRunAlertPanel(NSLocalizedString(@"Exit Application",@"Quit Application Title"),
								 NSLocalizedString(@"If you exit the Novell iFolder application, changes in your iFolder will no longer be tracked. The next time you login, Novell iFolder will reconcile any differences between your iFolder and Server.\n\nAre you sure you want to exit the Application?",@"Quit Application Message"),
								 NSLocalizedString(@"Quit",@"Quit Application Quit button"),
								 NSLocalizedString(@"Cancel",@"Quit Application Cancel button"),
								 nil);								 								 								 							
	}
											
	if(answer == NSAlertDefaultReturn || [[iFolderData sharedInstance] ForceQuit] )
	{*/
		SimiasService *simiasService = [[SimiasService alloc] init];
		
		NSArray* availableDomains  = [simiasService GetDomains:NO];
		for(counter = 0;counter < [availableDomains count]; counter++)
		{
			iFolderDomain *dom = [availableDomains objectAtIndex:counter];
			@try
			{
				if(![simiasService GetRememberPassPhraseOption:[dom ID]])
				{
					[simiasService StorePassPhrase:[dom ID] PassPhrase:@"" Type:None andRememberPP:NO];				
				}
			}
			@catch(NSException* ex)
			{
				ifexconlog(@"Exception in StorePassPhrase:iFolderApplication::applicationShouldTerminate",ex);
			}

		}
		[simiasService release];
		
		return NSTerminateNow;
//}
		
		
	return NSTerminateCancel;
}

//===================================================================
// enableThreads
// This method does nothing but is used to set app into
// multi-threaded mode
//===================================================================
- (void)enableThreads:(id)arg
{
	ifconlog1(@"Enabling multithreaded processing");
}




//===================================================================
// simiasEventThread
// This method contains the code to deal with all simias events
//===================================================================
- (void)simiasEventThread:(id)arg
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];

	while(runThreads)
	{
		SimiasEventData *sed = [SimiasEventData sharedInstance];
		[sed blockUntilEvents];

		while([sed hasEvents])
		{
			SMEvent *sme = [[sed popEvent] retain];
			
			if([[sme eventType] compare:@"NotifyEventArgs"] == 0)
			{
				[self processNotifyEvent:(SMNotifyEvent *)sme];
			}
			else if([[sme eventType] compare:@"CollectionSyncEventArgs"] == 0)
			{
				[self processCollectionSyncEvent:(SMCollectionSyncEvent *)sme];
			}
			else if([[sme eventType] compare:@"NodeEventArgs"] == 0)
			{
				[self processNodeEvent:(SMNodeEvent *)sme];
			}
			else if([[sme eventType] compare:@"FileSyncEventArgs"] == 0)
			{
				[self processFileSyncEvent:(SMFileSyncEvent *)sme];
			}
			else
			{
				ifconlog2(@"simiasEventThread encountered an unhandled event type: %@", [sme eventType]);
			}
			[sme release];
		}
	}
    [pool release];	
}




//===================================================================
// processNotifyEvents
// method to loop through all notify events and process them
//===================================================================
- (void)processNotifyEvent:(SMNotifyEvent *)smne
{
	ifconlog3(@"Received a \"%@\" event with message \"%@\"", [smne type], [smne message]);

	if([[smne type] compare:@"Domain-Up"] == 0)
	{
		// We need to attempt to connect to Simias to see if the Password has been saved
		SimiasService *simiasService = [[SimiasService alloc] init];
		NSString *savedPassword = nil;
		BOOL	showLoginDialog = NO;

		@try
		{
			savedPassword = [simiasService GetDomainPassword:[smne message]];
		}
		@catch(NSException *ex)
		{
			ifexconlog(@"processNotifyEvent:GetDomainPassword", ex);
		}

		if(savedPassword != nil)
		{
			ifconlog1(@"Credentials found on domain, authenticating...");

			iFolderDomain *dom = [[iFolderData sharedInstance] getDomain:[smne message]];
			if(dom != nil)
				[[NSApp delegate] setupSimiasProxies:[dom host]];
			else
				ifconlog1(@"Unable to locate domain to setup proxies");

			@try
			{
				AuthStatus *authStatus = [[simiasService LoginToRemoteDomain:[smne message] 
											usingPassword:savedPassword] retain];

				unsigned int statusCode = [[authStatus statusCode] unsignedIntValue];

				[authStatus release];
			
				if(	(statusCode == ns1__StatusCodes__Success) ||
					(statusCode == ns1__StatusCodes__SuccessInGrace) )
				{
					//Check for encryption & passphrase
					iFolderService *ifService = [[iFolderService alloc] init];
					
					int secPolicy = 0;
					
					secPolicy = [ifService GetSecurityPolicy:[smne message]];
					if(secPolicy % 2 != 0)
					{
						if([simiasService IsPassPhraseSet:[smne message]])
						{
							NSString* passPhraseValue = nil;
							if([simiasService GetRememberPassPhraseOption:[smne message]])
							{
								passPhraseValue = [simiasService GetPassPhrase:[smne message]];
							}
							if(passPhraseValue == nil || [passPhraseValue compare:@""] == NSOrderedSame)
							{
								[[VerifyPassPhraseController verifyPPInstance] setAndShow:simiasService andDomain:[smne message]];
								[NSApp runModalForWindow:[[VerifyPassPhraseController verifyPPInstance] window]];
							}
						}
						else
						{
							[[iFolderEncryptController encryptionInstance] setAndShow:simiasService andDomain:[smne message]];
							[NSApp runModalForWindow:[[iFolderEncryptController encryptionInstance] window]];	
						}
					}
					[ifService release];
					
					//Added the refresh code to update iFolder Window after login from 
					int domainCount;
					[[NSApp delegate] addLog:NSLocalizedString(@"Refreshing iFolder view", @"Log Message when refreshing main window")];
					
					if([[NSApp delegate] simiasIsRunning])
					{
						[[iFolderData sharedInstance] refresh:NO];
					}
						
					// Get all of the domains and refresh their POBoxes
					/*
					NSArray *domains = [[iFolderData sharedInstance] getDomains];
					
					for(domainCount = 0; domainCount < [domains count]; domainCount++)
					{
						iFolderDomain *dom = [domains objectAtIndex:domainCount];
					
						if(dom != nil)
						{
							[[iFolderData sharedInstance] synciFolderNow:[dom poBoxID]];
						}
					}
					*/
					
					ifconlog2(@"Successfully authenticated to domain %@", [smne message]);
					if( (statusCode == ns1__StatusCodes__SuccessInGrace) && (dom != nil) )
					{
						ifconlog1(@"Grace logins are expired");					
						NSRunAlertPanel(NSLocalizedString(@"iFolder Password Expired", @"Grace Login Warning Dialog Title"), 
								[NSString stringWithFormat:NSLocalizedString(@"Your password has expired.  You have %d grace logins remaining.", 
								@"Grace Login Warning Dialog message"), [[dom remainingGraceLogins] intValue] ],
								nil, nil, nil);
					}
				}
				else
				{
					ifconlog2(@"Unable to authenticate, status: %d", statusCode);
					showLoginDialog = YES;
				}
			}
			@catch (NSException *e)
			{
				ifconlog2(@"Exception authenticating to domain: %@", [e name]);
				showLoginDialog = YES;
			}			
		}
		else
			showLoginDialog = YES;

		[simiasService release];

		iFolderDomain *dom = [[iFolderData sharedInstance] getDomain:[smne message]];
		
		if(showLoginDialog && (dom!=nil && ![dom authenticated]))
		{
			[self showLoginWindowTS:[smne message]];
		}
	}
}




//===================================================================
// processNodeEvents
// method to loop through all node events and process them
//===================================================================
- (void)processNodeEvent:(SMNodeEvent *)ne
{
	//ifconlog2(@"processNodeEvent DATA IS : %@", [ne type]);
	// Handle all Node events here
	if([[ne type] compare:@"Subscription"] == 0)
	{
		// First check to see if this is a POBox 'cause we
		// don't care much about it if'n it aint.
		if([[iFolderData sharedInstance] isPOBox:[ne collectionID]])
		{
			[self performSelectorOnMainThread:@selector(processSubscriptionNodeEvent:) 
								   withObject:ne waitUntilDone:YES ];	
			//[self processSubscriptionNodeEvent:ne];
		}
	}
	else if([[ne type] compare:@"Collection"] == 0)
	{
		[self performSelectorOnMainThread:@selector(processCollectionNodeEvent:) 
							   withObject:ne waitUntilDone:YES ];	
		//[self processCollectionNodeEvent:ne];
	}
	else if([[ne type] compare:@"Member"] == 0)
	{
		[self performSelectorOnMainThread:@selector(processUserNodeEvent:) 
							   withObject:ne waitUntilDone:YES ];	

		//[self processUserNodeEvent:ne];
	}
	/*
	else
	{
		ifconlog2(@"processNodeEvent encountered an unhandled event type: %@", [ne type]);
	}
	*/
}




//===================================================================
// processCollectionNodeEvent
// method to loop through all node events and process them
//===================================================================
- (void)processCollectionNodeEvent:(SMNodeEvent *)colNodeEvent
{
	switch([colNodeEvent action])
	{
		case NODE_CREATED:
		{
			ifconlog2(@"processCollectionNodeEvent NODE_CREATED: %@", [colNodeEvent collectionID]);

			// not sure if we should read on every one but I think we
 			// need to in case of a new iFolder
			[[iFolderData sharedInstance] 
								readiFolder:[colNodeEvent collectionID]];
			break;
		}
		case NODE_DELETED:
		{
			iFolder *ifolder = [[iFolderData sharedInstance] getiFolder:[colNodeEvent collectionID]];
			
			if( (ifolder != nil) &&
				(![ifolder IsSubscription]) )
			{
				// remove it from the list if it's not a subscription
				[[iFolderData sharedInstance] 
								_deliFolder:[colNodeEvent collectionID]];
			}
			
			break;
		}

/*
		I'm not sure I still need to handle a collection node changed, I'm pulling it out
		because it's all handled in sync code if needed
		case NODE_CHANGED:
		{
			BOOL isiFolder = [[iFolderData sharedInstance] 
									isiFolder:[colNodeEvent collectionID]];

			if(isiFolder)
			{
				iFolder *ifolder = [[iFolderData sharedInstance]
									getiFolder:[colNodeEvent collectionID]];
									
				if(![ifolder isSynchronizing])
				{
					ifconlog1(@"Collection is not syncing, processing a NODE_CHANGED on a collection");

					[[iFolderData sharedInstance] 
										readiFolder:[colNodeEvent collectionID]];
				}
				
				// only do notifications now when the sync ends
//				if([ifolder HasConflicts])
//					[[iFolderData sharedInstance] setHasConflicts:[ifolder ID]];
//					[iFolderNotificationController collisionNotification:ifolder];								
			}
			break;
		}
*/

	}
}




//===================================================================
// processSubscriptionNodeEvent
// method to loop through all node events and process them
//===================================================================
- (void)processSubscriptionNodeEvent:(SMNodeEvent *)subNodeEvent
{
	switch([subNodeEvent action])
	{
		case NODE_CREATED:
		{
			ifconlog2(@"processSubscriptionNodeEvent NODE_CREATED: %@", [subNodeEvent nodeID]);
			iFolder *ifolder = [[iFolderData sharedInstance] 
									readAvailableiFolder:[subNodeEvent nodeID]
									inCollection:[subNodeEvent collectionID]];

			// if this wasn't an iFolder before we read it, notify the user
			if( (ifolder != nil) &&
				([[ifolder OwnerUserID] compare:[ifolder CurrentUserID]] != 0) )
				[iFolderNotificationController newiFolderNotification:ifolder];
			break;
		}
		case NODE_DELETED:
		{
			ifconlog2(@"processSubscriptionNodeEvent NODE_DELETED: %@", [subNodeEvent nodeID]);

			// Because we use the iFolder ID to hold subscriptions in the
			// dictionary, we need to get the iFolderID we used
			NSString *ifolderID = [[iFolderData sharedInstance]
										getiFolderID:[subNodeEvent nodeID]];
			if(ifolderID == nil)
				return;
				
			if([[iFolderData sharedInstance] isiFolder:ifolderID])
			{
				[[iFolderData sharedInstance] 
								_deliFolder:ifolderID];
			}
			break;
		}
		case NODE_CHANGED:
		{
			ifconlog2(@"processSubscriptionNodeEvent NODE_CHANGED: %@", [subNodeEvent nodeID]);

			// Because we use the iFolder ID to hold subscriptions in the
			// dictionary, we need to get the iFolderID we used
			NSString *ifolderID = [[iFolderData sharedInstance]
										getiFolderID:[subNodeEvent nodeID]];
			if(ifolderID == nil)
				return;
				
			[[iFolderData sharedInstance] readAvailableiFolder:[subNodeEvent nodeID]
												inCollection:[subNodeEvent collectionID]];
			break;
		}
	}
}




//===================================================================
// processUserNodeEvent
// method to loop through all user events and process them
//===================================================================
- (void)processUserNodeEvent:(SMNodeEvent *)userNodeEvent
{
	switch([userNodeEvent action])
	{
		case NODE_CREATED:
		{
			[[iFolderData sharedInstance] setUsersAdded:[userNodeEvent collectionID]];
			break;
		}
		case NODE_DELETED:
		{
			break;
		}
		case NODE_CHANGED:
		{
			break;
		}
	}
}




//===================================================================
// processCollectionSyncEvents
// method to loop through all collection sync events and process them
//===================================================================
- (void)processCollectionSyncEvent:(SMCollectionSyncEvent *)cse
{
	[self performSelectorOnMainThread:@selector(handleCollectionSyncEvent:) 
			withObject:cse waitUntilDone:YES ];				
}




//===================================================================
// handleCollectionSyncEvent
// this method does the work of updating status for the collection sync
// event and MUST run on the main thread
//===================================================================
- (void)handleCollectionSyncEvent:(SMCollectionSyncEvent *)colSyncEvent
{
	SMCollectionSyncEvent *cse = [colSyncEvent retain];

	BOOL updateData = NO;


	// This is special cased code for these collections
	// UGLY!!!!!
	if([[cse name] hasPrefix:@"POBox:"])
	{
		iFolderDomain *dom = [[iFolderData sharedInstance] getPOBoxDomain:[cse ID]];
		NSString *syncMessage;
		
		switch([cse syncAction])
		{
			case SYNC_ACTION_LOCAL:
			{
				if(dom != nil)
				{
					syncMessage = [NSString
							stringWithFormat:NSLocalizedString(@"Checking for new iFolders on: %@", @"iFolder Window Status Message"), 
							[dom name]];			
				}
				else
				{
					syncMessage = NSLocalizedString(@"Checking for new iFolders...", @"iFolder Window Status Message");			
				}
				[iFolderWindowController updateStatusTS:syncMessage];
				[self addLogTS:syncMessage];

				break;
			}
				
			case SYNC_ACTION_STOP:
			{
				if(dom != nil)
				{
					syncMessage = [NSString
							stringWithFormat:NSLocalizedString(@"Done checking for new iFolders on: %@", @"iFolder Window Status Message"), 
							[dom name]];			
				}
				else
				{
					syncMessage = NSLocalizedString(@"Done checking for new iFolders.", @"iFolder Window Status Message");			
				}

				[iFolderWindowController updateStatusTS:NSLocalizedString(@"Idle...", @"iFolder Window Status Message")];
				[self addLogTS:syncMessage];
				break;
			}
		}

		return;
	}
//  I had this special case code in here but it would eliminate an iFolder named "LocalDatabase"
//  so I took it out
//	else if([[cse name] compare:@"LocalDatabase"] == 0)
//		return;
//	else if([[cse name] compare:@"Local"] == 0)
//		return;

	iFolder *ifolder = [[iFolderData sharedInstance] 
							getiFolder:[cse ID]];

	switch([cse syncAction])
	{
		case SYNC_ACTION_LOCAL:
		{
			if(ifolder != nil)
			{
				[[iFolderData sharedInstance] readiFolder:[cse ID]];
				[ifolder setSyncState:SYNC_STATE_PREPARING];
				[[iFolderData sharedInstance] clearUsersAdded:[cse ID]];

				NSString *syncMessage = [NSString
							stringWithFormat:NSLocalizedString(@"Checking for changes: %@", @"iFolder Window Status Message"), 
							[cse name]];
				[iFolderWindowController updateStatusTS:syncMessage];
				[self addLogTS:syncMessage];
			}
			break;
		}
		case SYNC_ACTION_DISABLEDSYNC:
		{
			if(ifolder != nil)
			{
				[ifolder setSyncState:SYNC_STATE_DISABLEDSYNC];
				itemSyncCount = 0;
				NSString* syncMessage = [NSString stringWithFormat:NSLocalizedString(@"Synchronization disabled for %@",@"iFolder disabled by admin"),[cse name]];
				[self addLogTS:syncMessage];
				[iFolderWindowController updateStatusTS:NSLocalizedString(@"Idle...", @"iFolder Window Status Message")];
			}
			break;
		}
		
		case SYNC_ACTION_NOPASSPHRASE:
		{
			if(ifolder != nil)
			{
				[ifolder setSyncState:SYNC_STATE_NOPASSPHRASE];
				itemSyncCount = 0;
				NSString* syncMessage = [NSString stringWithFormat:NSLocalizedString(@"Passphrase not provided, will not sync the folder \"%@\"",@"No passphrase set"),[cse name]];
				[self addLogTS:syncMessage];
				[iFolderWindowController updateStatusTS:NSLocalizedString(@"Idle...", @"iFolder Window Status Message")];
			}
			break;
		}
			
		case SYNC_ACTION_START:
		{
			if(ifolder != nil)
			{
				[[iFolderData sharedInstance] readiFolder:[cse ID]];
				[ifolder setSyncState:SYNC_STATE_SYNCING];
				[[iFolderData sharedInstance] clearUsersAdded:[cse ID]];
		
				SyncSize *ss = [[[iFolderData sharedInstance] getSyncSize:[cse ID]] retain];			
				[ifolder setOutOfSyncCount:[ss SyncNodeCount]];
				itemSyncCount = 0;
				totalSyncCount = [ss SyncNodeCount];

				[ss release];

				if([[ifolder Role] compare:@"Master"] == 0)
					break;
		
				NSString *syncMessage = [NSString
							stringWithFormat:NSLocalizedString(@"Synchronizing: %@", @"iFolder Window Status Message"), 
							[cse name]];
				[iFolderWindowController updateStatusTS:syncMessage];
				[self addLogTS:syncMessage];
			}
			break;
		}
		case SYNC_ACTION_STOP:
		{
			itemSyncCount = 0;
			totalSyncCount = 0;

			if(ifolder != nil)
			{
				[[iFolderData sharedInstance] readiFolder:[cse ID]];

				if([ifolder HasConflicts])
				{
					ifconlog1(@"iFolder has collisions, notifying user");
					[iFolderNotificationController collisionNotification:ifolder];								
				}

				if([[iFolderData sharedInstance] usersAdded:[cse ID]])
				{
					[iFolderNotificationController newUserNotification:ifolder];								
				}

				if([[ifolder Role] compare:@"Master"] == 0)
				{
					break;
				}

		
				NSString *syncMessage;

				SyncSize *ss = [[[iFolderData sharedInstance] getSyncSize:[cse ID]] retain];			
				if([ss SyncNodeCount] == 0)
				{
					if([cse connected])
					{
					  if ([ifolder syncState] != SYNC_STATE_DISABLEDSYNC) {
						[ifolder setSyncState:SYNC_STATE_OK];
						syncMessage = [NSString
							stringWithFormat:NSLocalizedString(@"Finished synchronization: %@", @"Sync Log Message"), 
							[cse name]];
					    }
					}
					else
					{
						[ifolder setSyncState:SYNC_STATE_DISCONNECTED];
						syncMessage = [NSString
									stringWithFormat:NSLocalizedString(@"Failed synchronization: %@",  @"Sync Log Message"), 
									[cse name]];
					}
				}
				else
				{
					[ifolder setOutOfSyncCount:[ss SyncNodeCount]];
					[ifolder setSyncState:SYNC_STATE_OUT_OF_SYNC];
					syncMessage = [NSString
									stringWithFormat:NSLocalizedString(@"Not synchronized: %@",  @"Sync Log Message"), 
									[cse name]];
				}
				[ss release];

				[iFolderWindowController updateStatusTS:NSLocalizedString(@"Idle...", @"iFolder Window Status Message")];
				[self addLogTS:syncMessage];

				// sending current value of -1 hides the control
				[iFolderWindowController updateProgress:-1 withMin:0 withMax:0];
			}
			break;
		}
	}
}




//===================================================================
// processFileSyncEvents
// method to loop through all file sync events and process them
//===================================================================
- (void)processFileSyncEvent:(SMFileSyncEvent *)fse
{
	if([fse objectType] != FILE_SYNC_UNKNOWN)
	{
		[self performSelectorOnMainThread:@selector(handleFileSyncEvent:) 
				withObject:fse waitUntilDone:YES ];				
	}
}




//===================================================================
// handleFileSyncEvent
// this method does the work of updating status for the files sync
// event and MUST run on the main thread
//===================================================================
- (void)handleFileSyncEvent:(SMFileSyncEvent *)fileSyncEvent
{
	SMFileSyncEvent *fse = [fileSyncEvent retain];

	NSString *syncMessage = nil;
	NSString *syncItemMessage = nil;

	//ifconlog2(@"File sync event fired: %@", [fse name]);

	if([fse objectType] != FILE_SYNC_UNKNOWN)
	{
		if([[fse status] compare:@"Success"] == 0)
		{
			BOOL updateLog = NO;

			if([fse direction] != FILE_SYNC_LOCAL)
			{
				if([fse sizeRemaining] == [fse sizeToSync])
				{
					if( (itemSyncCount >= totalSyncCount) ||
						(totalSyncCount == 0) )
					{
						SyncSize *ss = [[[iFolderData sharedInstance] getSyncSize:[fse collectionID]] retain];			
						itemSyncCount = 0;
						totalSyncCount = [ss SyncNodeCount];
						[ss release];
						// If we had a problem reading this dude, set it to one
						if(totalSyncCount == 0)
						{
							totalSyncCount = 1;
						}
					}

					itemSyncCount += 1;
					if(itemSyncCount >= totalSyncCount)
						itemSyncCount = totalSyncCount;
				
					updateLog = YES;
					if([fse sizeToSync] > 0)
					{
						[iFolderWindowController updateProgress:0
												withMin:0
												withMax:[fse sizeToSync]];
					}
					else
					{
						// sending current value of -1 hides the control
						[iFolderWindowController updateProgress:-1 withMin:0 withMax:0];
					}
				}
				else
				{
					updateLog = NO;

					if([fse sizeToSync] == 0)
					{
						[iFolderWindowController updateProgress:100
												withMin:0
												withMax:100];
					}
					else
					{
						[iFolderWindowController updateProgress:([fse sizeToSync] - [fse sizeRemaining])
												withMin:0
												withMax:[fse sizeToSync]];
					}
				}
				
				syncItemMessage = [NSString stringWithFormat:NSLocalizedString(@"%u of %u items - ", @"prefix on a file sync for the iFolder Window status message"),
									itemSyncCount, totalSyncCount];
			}
			
			switch([fse direction])
			{
				case FILE_SYNC_LOCAL:
				{
					if([fse objectType] == FILE_SYNC_FILE)
					{
						[self addLogTS:[NSString
							stringWithFormat:NSLocalizedString(@"Found changes in file: %@", @"Sync Log Message"), 
							[fse name]]];
					}
					else
					{
						[self addLogTS:[NSString
							stringWithFormat:NSLocalizedString(@"Found local change in folder: %@", @"Sync Log Message"), 
							[fse name]]];
					}	
					break;
				}
				case FILE_SYNC_UPLOADING:
				{
					if([fse isDelete])
					{
						syncMessage = [NSString
							stringWithFormat:NSLocalizedString(@"Deleting on server: %@", @"iFolder Window Status Message"), 
							[fse name]];
						[iFolderWindowController updateStatusTS:
							[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
						if(updateLog)
							[self addLogTS:syncMessage];
/*
						}
						else
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Deleting on server: %@", nil), 
								[fse name]];
							[iFolderWindowController updateStatusTS:
								[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
							if(updateLog)
								[self addLogTS:syncMessage];
						}	
*/
					}
					else
					{
						if([fse objectType]  == FILE_SYNC_FILE)
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Uploading file: %@", @"iFolder Window Status Message"), 
								[fse name]];
							[iFolderWindowController updateStatusTS:
								[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
							if(updateLog)
								[self addLogTS:syncMessage];
						}
						else
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Uploading folder: %@", @"iFolder Window Status Message"), 
								[fse name]];
							[iFolderWindowController updateStatusTS:
								[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
							if(updateLog)
								[self addLogTS:syncMessage];
						}
					}
					break;
				}
				case FILE_SYNC_DOWNLOADING:
				{
					if([fse isDelete])
					{
						if([fse objectType] == FILE_SYNC_FILE)
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Deleting file: %@", @"iFolder Window Status Message"), 
								[fse name]];
							[iFolderWindowController updateStatusTS:
								[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
							if(updateLog)
								[self addLogTS:syncMessage];
						}
						else
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Deleting folder: %@", @"iFolder Window Status Message"), 
								[fse name]];
							[iFolderWindowController updateStatusTS:
								[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
							if(updateLog)
								[self addLogTS:syncMessage];
						}	
					}
					else
					{
						if([fse objectType] == FILE_SYNC_FILE)
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Downloading file: %@", @"iFolder Window Status Message"), 
								[fse name]];
							[iFolderWindowController updateStatusTS:
								[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
							if(updateLog)
								[self addLogTS:syncMessage];
						}
						else
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Downloading folder: %@", @"iFolder Window Status Message"), 
								[fse name]];
							[iFolderWindowController updateStatusTS:
								[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
							if(updateLog)
								[self addLogTS:syncMessage];
						}
					}
					break;
				}
			}
		}
		else
		{
			iFolder *ifolder = [[iFolderData sharedInstance] getiFolder:[fse collectionID]];
			
			// For all other cases, check to make sure we don't have more stuff to do that counted items
			if( (itemSyncCount >= totalSyncCount) ||
				(totalSyncCount == 0) )
			{
				SyncSize *ss = [[[iFolderData sharedInstance] getSyncSize:[fse collectionID]] retain];			
				itemSyncCount = 0;
				totalSyncCount = [ss SyncNodeCount];
				[ss release];
				// If we had a problem reading this dude, set it to one
				if(totalSyncCount == 0)
				{
					totalSyncCount = 1;
				}
			}

			itemSyncCount += 1;
			if(itemSyncCount >= totalSyncCount)
				itemSyncCount = totalSyncCount;

			syncItemMessage = [NSString stringWithFormat:NSLocalizedString(@"%u of %u items - ", @"prefix on a file sync for the iFolder Window status message"),
								itemSyncCount, totalSyncCount];

			[iFolderWindowController updateProgress:100
									withMin:0
									withMax:100];

			// Conflict message
			if(	([[fse status] compare:@"UpdateConflict"] == 0) ||
				([[fse status] compare:@"FileNameConflict"] == 0) )
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"Conflict occurred: %@", @"iFolder Window Status Message"), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
			}
			/*
			// Policy does not allow this file
			else if([[fse status] compare:@"Policy"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"Policy prevented synchronization: %@", @"iFolder Window Status Message"), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
				
				if(ifolder != nil)
					[iFolderNotificationController policyNotification:ifolder];
			}
			*/
			// Insuficient rights
			else if([[fse status] compare:@"Access"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"Insufficient rights prevented complete synchronization: \"%@\"", @"iFolder Access Notification Message"), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
				
				NSString *notificationFileDetails = [NSString stringWithFormat:@"%@###%@",[ifolder Name],[fse name]];
				
				if(ifolder != nil)
					[iFolderNotificationController accessNotification:notificationFileDetails];
			}
			// Locked
			else if([[fse status] compare:@"Locked"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"The iFolder is locked: \"%@\"", @"iFolder Locked Notification Message"), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
				
				NSString *notificationFileDetails = [NSString stringWithFormat:@"%@###%@",[ifolder Name],[fse name]];
				
				if(ifolder != nil)
					[iFolderNotificationController lockedNotification:notificationFileDetails];
			}
			// PolicyQuota
			else if([[fse status] compare:@"PolicyQuota"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"Full iFolder prevented synchronization: \"%@\"", @"iFolder Window Status Message"), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];

				if([iFolderFullNotifications objectForKey:[fse collectionID]] == nil)
				{
					// if the current iFolder is not found, add it to the syncFail notifications
					// and notify
					[iFolderFullNotifications setObject:[fse collectionID] forKey:[fse collectionID]];

					//iFolder *ifolder = [[iFolderData sharedInstance] getiFolder:[fse collectionID]];
					
					// if this wasn't an iFolder before we read it, notify the user
					NSString *notificationFileDetails = [NSString stringWithFormat:@"%@###%@",[ifolder Name],[fse name]];
									
					if(ifolder != nil)
						[iFolderNotificationController iFolderFullNotification:notificationFileDetails];	
				}
			}
			// DiskFull
			else if([[fse status] compare:@"DiskFull"] == 0)
			{
				if([fse direction] == FILE_SYNC_DOWNLOADING)
				{
					syncMessage = [NSString
						stringWithFormat:NSLocalizedString(@"Insufficient disk space on this computer prevented synchronization: \"%@\"", @"iFolder Window Status Message"), 
						[fse name]];
				}
				else
				{
					syncMessage = [NSString
						stringWithFormat:NSLocalizedString(@"Insufficient disk space on the server prevented synchronization: \"%@\"", @"iFolder Window Status Message"), 
						[fse name]];
				}
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];

				NSString *notificationFileDetails = [NSString stringWithFormat:@"%@###%@",[ifolder Name],[fse name]];
				
				if(ifolder != nil)
					[iFolderNotificationController diskFullNotification:notificationFileDetails];
			}
			// PolicySize
			else if([[fse status] compare:@"PolicySize"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"A size restriction policy prevented complete synchronization: \"%@\"", @"iFolder Policy Size Notification Message"), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
				
				NSString *notificationFileDetails = [NSString stringWithFormat:@"%@###%@",[ifolder Name],[fse name]];
				
				if(ifolder != nil)
					[iFolderNotificationController policySizeNotification:notificationFileDetails];
			}
			// PolicyType
			else if([[fse status] compare:@"PolicyType"] == 0)
			{
				NSRange thumbsDB = [[fse name] rangeOfString:@"Thumbs.db"] ;
				NSRange dsStore = [[fse name] rangeOfString:@".DS_Store"];

				if(thumbsDB.location == NSNotFound || dsStore.location == NSNotFound)
				{
					syncMessage = [NSString stringWithFormat:NSLocalizedString(@"A file type restriction policy prevented complete synchronization: \"%@\"", @"iFolder Policy Type Notification Message"), [fse name]];
					
					[iFolderWindowController updateStatusTS:[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
					   
					//[iFolderNotificationController syncFailNotification:[fse name]];
					[self addLogTS:syncMessage];
					   
					NSString *notificationFileDetails = [NSString stringWithFormat:@"%@###%@",[ifolder Name],[fse name]];
					   
					if(ifolder != nil)
					{
						[iFolderNotificationController policyTypeNotification:notificationFileDetails];					   	
					}
				}
				
				
//				if([syncFailNotifications objectForKey:[fse collectionID]] == nil && 
//						[[fse name] caseInsensitiveCompare:@"Thumbs.db"] != NSOrderedSame /*&&
//						/[[fse name] caseInsensitiveCompare:@".DS_Store"] != NSOrderedSame*/)
//				{
//					// if the current iFolder is not found, add it to the readOnly notifications
//					// and notify
//					[syncFailNotifications setObject:[fse collectionID] forKey:[fse collectionID]];
					
//					//iFolder *ifolder = [[iFolderData sharedInstance] getiFolder:[fse collectionID]];
					
//					// if this wasn't an iFolder before we read it, notify the user
//					if(ifolder != nil)
//						[iFolderNotificationController syncFailNotification:ifolder];
//				}
			}
			// ReadOnly
			else if([[fse status] compare:@"ReadOnly"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"Read-only iFolder prevented synchronization: \"%@\"", @"iFolder Read-only notification message"), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
				
				if([readOnlyNotifications objectForKey:[fse collectionID]] == nil)
				{
					// if the current iFolder is not found, add it to the readOnly notifications
					// and notify
					[readOnlyNotifications setObject:[fse collectionID] forKey:[fse collectionID]];
					
					//iFolder *ifolder = [[iFolderData sharedInstance] getiFolder:[fse collectionID]];
					
					// if this wasn't an iFolder before we read it, notify the user
					NSString *notificationFileDetails = [NSString stringWithFormat:@"%@###%@",[ifolder Name],[fse name]];
									
					if(ifolder != nil)
						[iFolderNotificationController readOnlyNotification:notificationFileDetails];
				}
			}
			else if([[fse status] compare:@"PathTooLong"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"Path is too long for the file \"%@\" to be downloaded",@"iFolder Window Status Message"),[fse name]];
				[iFolderWindowController updateStatusTS:[NSString stringWithFormat:@"%@%@",syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
			}
			else if([[fse status] compare:@"IOError"] == 0)
			{
				if([fse direction] == FILE_SYNC_DOWNLOADING)
				{
					syncMessage = [NSString
							stringWithFormat:NSLocalizedString(@"Unable to write files in the folder. Verify the permissions on your local folder.",
											   @"iFolder Window Status Message")];
				}
				else
				{
					syncMessage = [NSString
							stringWithFormat:NSLocalizedString(@"Unable to read files from the folder. Verify the permissions on your local folder.",
											   @"iFolder Window Status Message")];
				}

				[iFolderWindowController updateStatusTS:[NSString stringWithFormat:@"%@%@",syncItemMessage, syncMessage]];

				NSString *notificationMessageDetails = [NSString stringWithFormat:@"%@###%@###%@",[ifolder Name],[fse name], syncMessage];
				[iFolderNotificationController ioErrorNotification:notificationMessageDetails];

				[self addLogTS:syncMessage];
			}
			// All other errors
			else
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"iFolder failed synchronization: %@", @"iFolder Window Status Message"), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
			}

		}
	}
	[fse release];
}




//===================================================================
// setupProxyMonitor
// This will setup a monitor to watch for Proxy changes on the client
// so if they change or the network interface changes, ifolder will
// get the new configuration and continue to run
//===================================================================
- (void) setupProxyMonitor
{
	CFStringRef notifyKeys[2];
	
	SCDynamicStoreContext context;
	
	context.version = 0;
	context.info = NULL;
	context.retain = NULL;
	context.release = NULL;
	context.copyDescription = NULL;
	
	// Creates the dynamicStore Session which must be freed
	dynStoreRef = SCDynamicStoreCreate (NULL, (CFStringRef)@"iFolder 3", 
			dynStoreCallBack, &context);

	CFStringRef proxyRefStr = SCDynamicStoreKeyCreateProxies ( NULL ); 

	notifyKeys[0] = proxyRefStr;
	notifyKeys[1] = 0;
	
	CFArrayRef keysRef = CFArrayCreate(NULL, (const void **)notifyKeys, 1, NULL);

	if(keysRef != NULL)
	{
		if(SCDynamicStoreSetNotificationKeys ( dynStoreRef,
				keysRef, NULL ))
			ifconlog1(@"Setup to monitor proxy changes");
		
		dynStoreRunLoopRef = SCDynamicStoreCreateRunLoopSource(NULL, dynStoreRef, 0);
		CFRunLoopAddSource(CFRunLoopGetCurrent(), dynStoreRunLoopRef, kCFRunLoopCommonModes);
	
		CFRelease(keysRef);
	}
}



//===================================================================
// dynStoreCallBack
// This callback gets called whenever the proxy settings on the client
// change.  This will adjust for network settings and such.
//===================================================================
void dynStoreCallBack(SCDynamicStoreRef store, CFArrayRef changedKeys, void *info)
{
	int x;
	NSArray *domains = [[iFolderData sharedInstance] getDomains];
	for(x=0; x < [domains count]; x++)
	{
		iFolderDomain *dom = [domains objectAtIndex:x];
		[[NSApp delegate] setupSimiasProxies:[dom host]];
	}
} 




//===================================================================
// getHTTPProxyURI
// Read the proxy settings and determine if the host passed in is in
// the ignore list.  Returns a URI (http:// or https://) based on
// the proxy settings in the system
//===================================================================
- (NSString *)getHTTPProxyURI:(NSString *)host UseHTTPS:(BOOL)useHTTPS
{
	NSString *proxyURI = nil;
	BOOL returnURI = NO;

	// Call to copy the System Configuration Proxies
	CFDictionaryRef ref = SCDynamicStoreCopyProxies(NULL);

	if(ref != NULL)
	{
		const void *proxyHost;
		const void *proxyPort;

		if(useHTTPS)
		{
			if(CFDictionaryGetValueIfPresent (ref, kSCPropNetProxiesHTTPSProxy, &proxyHost))
			{
				ifconlog2(@"HTTPS Proxy Value found %@", (NSString *)proxyHost);
				
				// If we found the HTTPSProxy, check now for a Port
				if(CFDictionaryGetValueIfPresent (ref, kSCPropNetProxiesHTTPSPort, &proxyPort))
				{
					ifconlog2(@"HTTPS Proxy Port found %@", (NSNumber *)proxyPort);
					// Even though this is https, set the http prefix because
					// you can't talk to a proxy using https
					proxyURI = [NSString stringWithFormat:@"http://%@:%@", (NSString *)proxyHost, (NSNumber *)proxyPort];
					returnURI = YES;
				}
			}
		}
		else
		{
			if(CFDictionaryGetValueIfPresent (ref, kSCPropNetProxiesHTTPProxy, &proxyHost))
			{
				ifconlog2(@"HTTP Proxy Value found %@", (NSString *)proxyHost);
				
				// If we found the HTTPSProxy, check now for a Port
				if(CFDictionaryGetValueIfPresent (ref, kSCPropNetProxiesHTTPPort, &proxyPort))
				{
					ifconlog2(@"HTTP Proxy Port found %@", (NSNumber *)proxyPort);
					proxyURI = [NSString stringWithFormat:@"http://%@:%@", (NSString *)proxyHost, (NSNumber *)proxyPort];
					returnURI = YES;
				}
			}
		}

		if(proxyURI != nil)
		{
			const void *exceptionArray;
			
			// We found a Proxy setting, now check our host to see if it is in the bypass list
			if(CFDictionaryGetValueIfPresent (ref, kSCPropNetProxiesExceptionsList, &exceptionArray))
			{
				CFIndex counter = 0;
				for(counter = 0; counter < CFArrayGetCount(exceptionArray); counter++)
				{
					CFStringRef bypassHost = CFArrayGetValueAtIndex(exceptionArray, counter);

					if([host compare:(NSString *)bypassHost] == NSOrderedSame)
					{
						ifconlog1(@"Host found in Proxy Bypass list, returning nil for proxy");
						proxyURI = nil;
						returnURI = NO;
						break;
					}
				}
			}
		}

		CFRelease(ref);
	}

	return proxyURI;
}




//===================================================================
// setupSimiasProxies
// Sets up both the http and https proxies or removes them for the
// current network interface and host passed in.
//===================================================================
- (void) setupSimiasProxies:(NSString *)host
{
	SimiasService *simiasService = [[SimiasService alloc] init];
	NSString *hostURI;
	
	hostURI = [NSString stringWithFormat:@"https://%@", host];
	
	NSString *httpsProxyURI = [self getHTTPProxyURI:host UseHTTPS:YES];
	
	@try
	{
		if([simiasService SetProxyAddress:hostURI
				ProxyURI:httpsProxyURI
				ProxyUser:nil 
				ProxyPassword:nil] == NO)
		{
			if(httpsProxyURI != nil)
				ifconlog2(@"iFolder was unable to setup the https proxy %@", httpsProxyURI);
			else
				ifconlog1(@"iFolder was unable to remove the https proxy setting");
		}
		else
		{
			if(httpsProxyURI != nil)
				ifconlog2(@"iFolder setup with https proxy %@", httpsProxyURI);
			else
				ifconlog1(@"iFolder setup without a https proxy");
		}
	}
	@catch (NSException *e)
	{
		if(httpsProxyURI != nil)
			ifconlog2(@"iFolder encountered an exception while setting up the https proxy %@", httpsProxyURI);
		else
			ifconlog1(@"iFolder encountered an exception while clearing the https proxy");

		ifexconlog(@"setupSmiasProxies", e);
	}

	hostURI = [NSString stringWithFormat:@"http://%@", host];

	NSString *httpProxyURI = [self getHTTPProxyURI:host UseHTTPS:NO];

	@try
	{
		if([simiasService SetProxyAddress:hostURI
				ProxyURI:httpProxyURI
				ProxyUser:nil 
				ProxyPassword:nil] == NO)
		{
			if(httpProxyURI != nil)
				ifconlog2(@"iFolder was unable to setup the http proxy %@", httpProxyURI);
			else
				ifconlog1(@"iFolder was unable to remove the http proxy setting");
		}
		else
		{
			if(httpProxyURI != nil)
				ifconlog2(@"iFolder setup with http proxy %@", httpProxyURI);
			else
				ifconlog1(@"iFolder setup without a http proxy");
		}
	}
	@catch (NSException *e)
	{
		if(httpProxyURI != nil)
			ifconlog2(@"iFolder encountered an exception while setting up the http proxy %@", httpProxyURI);
		else
			ifconlog1(@"iFolder encountered an exception while clearing the http proxy");

		ifexconlog(@"setupSmiasProxies", e);
	}
	
	[simiasService release];
}

- (void) refreshTimerCall:(NSTimer*)refTimer
{
	if([[NSApp delegate] simiasIsRunning])
		[[iFolderData sharedInstance] refresh:NO];
	refreshTimer = [NSTimer scheduledTimerWithTimeInterval:300 target:self selector:@selector(refreshTimerCall:) userInfo:nil repeats:NO]; //5 minutes by default
}

- (void) stopRefreshTimer
{
	[refreshTimer invalidate];
}

- (void) startRefreshTimer
{
	refreshTimer = [NSTimer scheduledTimerWithTimeInterval:300 target:self selector:@selector(refreshTimerCall:) userInfo:nil repeats:NO]; //5 minutes by default	
}

- (BOOL)validateUserInterfaceItem:(id)anItem
{
	SEL action = [anItem action];

	if(action == @selector(showPrefs:))
	{
		return [self simiasIsRunning];
	}
	return YES;
}
@end
