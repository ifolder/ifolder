/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/
 
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


@implementation iFolderApplication


//===================================================================
// awakeFromNib
// When this class is loaded from the nib, startup simias and wait
// since our app isn't useful without simias
//===================================================================
-(void)awakeFromNib
{
	NSLog(@"Waiting for app to enable multithreading");
	simiasIsLoaded = NO;

	// this baby will get cocoa objects ready for mutlitple threads
    [NSThread detachNewThreadSelector:@selector(enableThreads:)
        toTarget:self withObject:nil];

	// wait until cocoa is safely in multithreaded mode
	while(![NSThread isMultiThreaded])
	{
		// wait until it is
		NSLog(@"Waiting for app to enable multithreading");
		[NSThread sleepUntilDate:[[NSDate date] addTimeInterval:.5] ];
	}

	NSLog(@"initializing Simias Events");
	[self initializeSimiasEvents];
	
	NSLog(@"Starting Simias Process");
	[ [Simias getInstance] start];
//	[self startSimiasThread:self];


	ifolderdata = [[iFolderData alloc] init];
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
			[[NSDate date] descriptionWithCalendarFormat:@"%m/%d/%Y %H:%M:%S" timeZone:nil locale:nil], 
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
	iFolderDomain *dom = [ifolderdata getDomain:domainID];
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
	[self setupApplicationDefaults];

	// check if we should restore windows
	if([[NSUserDefaults standardUserDefaults] boolForKey:PREFKEY_RESTOREWIN])
	{
		// yes, so only restore it was there before
		if([[NSUserDefaults standardUserDefaults] boolForKey:STATE_SHOWMAINWINDOW])		
			[self showiFolderWindow:self];
		if([[NSUserDefaults standardUserDefaults] boolForKey:STATE_SHOWLOGWINDOW])		
			[self showSyncLog:self];
	}
	else
		[self showiFolderWindow:self];

	[iFolderWindowController updateStatusTS:@"Loading synchronization process..."];
}


//===================================================================
// setupApplicationDefaults
// This will load the defaults and if they are not there, set them
// up for the first time
//===================================================================
- (void)setupApplicationDefaults
{

	NSArray *keys	= [NSArray arrayWithObjects:	PREFKEY_WINPOS,
													PREFKEY_RESTOREWIN,
													PREFKEY_CLICKIFOLDER,
													PREFKEY_NOTIFYIFOLDERS,
													PREFKEY_NOTIFYCOLL,
													PREFKEY_NOTIFYUSER,
													PREFKEY_NOTIFYBYINDEX,
													STATE_SHOWMAINWINDOW,
													nil];

	NSArray *values = [NSArray arrayWithObjects:	[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithInt:0],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithInt:0],
													[NSNumber numberWithBool:YES],
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

	NSLog(@"Creating and loading iFolderData");
	[ifolderdata refresh:NO];
		
	// Startup the event processing thread
    [NSThread detachNewThreadSelector:@selector(simiasEventThread:)
        toTarget:self withObject:nil];

	[iFolderWindowController updateStatusTS:@"Idle..."];
}




//===================================================================
// simiasHasStarted
// This is a thread safe method that the simias startup routine calls
// once simias is up and running
//===================================================================
-(void)simiasHasStarted
{
	[self performSelectorOnMainThread:@selector(postSimiasInit:) 
				withObject:nil waitUntilDone:NO ];
}




//===================================================================
// applicationWillTerminate
// Application Delegate method, called with the application is going
// to end
//===================================================================
- (void)applicationWillTerminate:(NSNotification *)notification
{
	runThreads = NO;
	[self addLog:@"Shutting down Simias..."];
	[ [Simias getInstance] stop];
	[self addLog:@"Simias is shut down"];

	[self addLog:@"Unregistering events..."];
	SimiasEventDisconnect();
}




//===================================================================
// startSimiasThread
// This is a method called on a new thread to start up the Simias
// process.  When done, this will call back on the main thread
// the simiasDidFinishStarting
//===================================================================
- (void)startSimiasThread:(id)arg
{
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
		NSLog(@"Simias is not running, starting....");
		iFolderService *threadWebService;
		
		// Startup simias Process
		[ [Simias getInstance] start];
		
		// Wait 5 seconds for simias to load, then start pinging
		[NSThread sleepUntilDate:[[NSDate date] addTimeInterval:1]];

		while(!simiasRunning)
		{
			threadWebService = [[iFolderService alloc] init];		
		
			@try
			{
				NSLog(@"Pinging simias to check for startup...");
				simiasRunning = [threadWebService Ping];
				NSLog(@"Back from pinging simias!");
			}
			@catch(NSException *e)
			{
				NSLog(@"Exception in Ping: %@ - %@", [e name], [e reason]);
				NSLog(@"Failed to ping simias!");
				simiasRunning = NO;
			}
			// I tried doing this in the catch above but sometimes we fail
			// and don't throw and exception so I moved it here to avoid a 
			// tight loop
			if(!simiasRunning)
			{
				NSLog(@"Simias is not running, sleeping.");
				[NSThread sleepUntilDate:[[NSDate date] addTimeInterval:1]];
			}

			NSLog(@"freeing up threadWebService");
			[threadWebService release];
			threadWebService = nil;
		}
	}
}




//===================================================================
// authenticateToDomain
// This will authenticate using the specified password and domainID
//===================================================================
- (BOOL)authenticateToDomain:(NSString *)domainID withPassword:(NSString *)password
{
	SimiasService *simiasService;
	simiasService = [[SimiasService alloc] init];		

	@try
	{
		[simiasService LoginToRemoteDomain:domainID usingPassword:password];
		return YES;
	}
	@catch (NSException *e)
	{
		NSLog(@"failed to authenticate");
	}
	return NO;
}




//===================================================================
// enableThreads
// This method does nothing but is used to set app into
// multi-threaded mode
//===================================================================
- (void)enableThreads:(id)arg
{
	NSLog(@"Enabling multithreaded processing");
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
		NSLog(@"simiasEventThread going to sleep...");
//		[iFolderWindowController updateStatusTS:@"Idle..."];
//		[iFolderWindowController updateProgress:-1 withMin:0 withMax:0];
		[sed blockUntilEvents];
		NSLog(@"simias EventThread woke up to process events");
		
		[self processNotifyEvents];
		[self processCollectionSyncEvents];
		[self processNodeEvents];
		[self processFileSyncEvents];

	}
    [pool release];	
}




//===================================================================
// processNotifyEvents
// method to loop through all notify events and process them
//===================================================================
- (void)processNotifyEvents
{
	SimiasEventData *sed = [SimiasEventData sharedInstance];

	// Take care of Notify Events
	while([sed hasNotifyEvents])
	{
		SMNotifyEvent *smne = [[sed popNotifyEvent] retain];

		NSLog(@"Got Notify Message %@", [smne message]);
		
		if([[smne type] compare:@"Domain-Up"] == 0)
		{
			[self showLoginWindowTS:[smne message]];
		}
		[smne release];
	}
}




//===================================================================
// processNodeEvents
// method to loop through all node events and process them
//===================================================================
- (void)processNodeEvents
{
	SimiasEventData *sed = [SimiasEventData sharedInstance];

	// Take care of Collection Sync
	while([sed hasNodeEvents])
	{
		SMNodeEvent *ne = [[sed popNodeEvent] retain];
		// Do a whole hell of a lot of work

		// Handle all Node events here
		if([[ne type] compare:@"Subscription"] == 0)
		{
			// First check to see if this is a POBox 'cause we
			// don't care much about it if'n it aint.
			NSLog(@"processingNodeEvents checking for valid POBox");
			if([[iFolderData sharedInstance] isPOBox:[ne collectionID]])
			{
				[self processSubscriptionNodeEvent:ne];
			}
		}
		else if([[ne type] compare:@"Collection"] == 0)
		{
			[self processCollectionNodeEvent:ne];
		}
		else
		{
			NSLog(@"***** UNHANDLED NODE EVENT ****  Type: %@", [ne type]);
		}

		[ne release];
	}
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
			NSLog(@"processCollectionNodeEvent NODE_CREATED");

			// not sure if we should read on every one but I think we
 			// need to in case of a new iFolder
			[[iFolderData sharedInstance] 
								readiFolder:[colNodeEvent collectionID]];
			break;
		}
		case NODE_DELETED:
		{
			NSLog(@"processCollectionNodeEvent NODE_DELETED");

			iFolder *ifolder = [[iFolderData sharedInstance]
									getiFolder:[colNodeEvent collectionID]];
			if( (ifolder != nil) &&
				(![ifolder IsSubscription]) )
			{
				// remove it from the list if it's not a subscription
				[[iFolderData sharedInstance] 
								_deliFolder:[colNodeEvent collectionID]];
			}
			break;
		}
		case NODE_CHANGED:
		{
			NSLog(@"processCollectionNodeEvent NODE_CHANGED");

			BOOL isiFolder = [[iFolderData sharedInstance] 
									isiFolder:[colNodeEvent collectionID]];

			if(isiFolder)
			{
				[[iFolderData sharedInstance] 
								readiFolder:[colNodeEvent collectionID]];
			}
			break;
		}
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
			NSLog(@"processSubscriptionNodeEvent NODE_CREATED");
			iFolder *ifolder = [[iFolderData sharedInstance] 
									readAvailableiFolder:[subNodeEvent nodeID]
									inCollection:[subNodeEvent collectionID]];
//			if(ifolder != nil) trigger some event
			break;
		}
		case NODE_DELETED:
		{
			NSLog(@"processSubscriptionNodeEvent NODE_DELETED");

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
			NSLog(@"processSubscriptionNodeEvent NODE_CHANGED");
		
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
// processCollectionSyncEvents
// method to loop through all collection sync events and process them
//===================================================================
- (void)processCollectionSyncEvents
{
	SimiasEventData *sed = [SimiasEventData sharedInstance];

	// Take care of Collection Sync
	while([sed hasCollectionSyncEvents])
	{
		SMCollectionSyncEvent *cse = [[sed popCollectionSyncEvent] retain];
		[self performSelectorOnMainThread:@selector(handleCollectionSyncEvent:) 
				withObject:cse waitUntilDone:YES ];				
		[cse release];
	}
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
		
	iFolder *ifolder = [[iFolderData sharedInstance] 
							getiFolder:[cse ID]];

	if(ifolder != nil)
	{
		if([cse syncAction] == SYNC_ACTION_START)
			[ifolder setIsSynchronizing:YES];
		else
		{
			[ifolder setIsSynchronizing:NO];

			if( [ [ifolder State] isEqualToString:@"WaitSync"])
			{
				NSLog(@"handleCollectionSyncEvent: iFolder stat is WaitSync");
				updateData = YES;
			}
		}

		if( [ifolder Path] == nil )
		{
			NSLog(@"handleCollectionSyncEvent: iFolder Path is nil");
			updateData = YES;
		}
			
		if(updateData)
		{
			NSLog(@"handleCollectionSyncEvent calling getiFolder with updateData");
			[[iFolderData sharedInstance] readiFolder:[cse ID]];
		}
	}

	switch([cse syncAction])
	{
		case SYNC_ACTION_START:
		{
			NSString *syncMessage = [NSString
							stringWithFormat:@"Synchronizing: %@", 
							[cse name]];
			[iFolderWindowController updateStatusTS:syncMessage];
			[self addLogTS:syncMessage];
			break;
		}
		case SYNC_ACTION_STOP:
		{
			NSString *syncMessage;
			if([cse isDone])
				syncMessage = [NSString
							stringWithFormat:@"Done synchronizing: %@", 
							[cse name]];
			else
				syncMessage = [NSString
							stringWithFormat:@"Paused synchronizing: %@", 
							[cse name]];

			[iFolderWindowController updateStatusTS:@"Idle..."];
			[self addLogTS:syncMessage];

			// sending current value of -1 hides the control
			[iFolderWindowController updateProgress:-1 withMin:0 withMax:0];			
			break;
		}
	}
}




//===================================================================
// processFileSyncEvents
// method to loop through all file sync events and process them
//===================================================================
- (void)processFileSyncEvents
{
	SimiasEventData *sed = [SimiasEventData sharedInstance];

	// Take care of File Sync
	while([sed hasFileSyncEvents])
	{
		SMFileSyncEvent *fse = [[sed popFileSyncEvent] retain];
		if([fse objectType] != FILE_SYNC_UNKNOWN)
		{
			[self performSelectorOnMainThread:@selector(handleFileSyncEvent:) 
					withObject:fse waitUntilDone:YES ];				
		}
		[fse release];
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

	static NSString *fileSyncName = nil;
	NSString *syncMessage = nil;

	if([fse objectType] != FILE_SYNC_UNKNOWN)
	{
		BOOL updateLog = NO;

		if(fileSyncName == nil)
		{
			fileSyncName = [[NSString stringWithString:[fse name]] retain];
			updateLog = YES;
		}

		if([fileSyncName compare:[fse name]] != 0)
		{
			[fileSyncName release];
			fileSyncName = [[NSString stringWithString:[fse name]] retain];
			updateLog = YES;
		}
	
		switch([fse direction])
		{
			case FILE_SYNC_UPLOADING:
			{
				if([fse isDelete])
				{
					if([fse objectType] == FILE_SYNC_FILE)
					{
						syncMessage = [NSString
							stringWithFormat:@"Removing file from server: %@", 
							[fse name]];
						[iFolderWindowController updateStatusTS:syncMessage];
						if(updateLog)
							[self addLogTS:syncMessage];
					}
					else
					{
						syncMessage = [NSString
							stringWithFormat:@"Removing directory from server: %@", 
							[fse name]];
						[iFolderWindowController updateStatusTS:syncMessage];
						if(updateLog)
							[self addLogTS:syncMessage];
					}	
				}
				else
				{
					if([fse objectType]  == FILE_SYNC_FILE)
					{
						syncMessage = [NSString
							stringWithFormat:@"Uploading file: %@", 
							[fse name]];
						[iFolderWindowController updateStatusTS:syncMessage];
						if(updateLog)
							[self addLogTS:syncMessage];
					}
					else
					{
						syncMessage = [NSString
							stringWithFormat:@"Creating directory on server: %@", 
							[fse name]];
						[iFolderWindowController updateStatusTS:syncMessage];
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
							stringWithFormat:@"Deleting file: %@", 
							[fse name]];
						[iFolderWindowController updateStatusTS:syncMessage];
						if(updateLog)
							[self addLogTS:syncMessage];
					}
					else
					{
						syncMessage = [NSString
							stringWithFormat:@"Removing directory: %@", 
							[fse name]];
						[iFolderWindowController updateStatusTS:syncMessage];
						if(updateLog)
							[self addLogTS:syncMessage];
					}	
				}
				else
				{
					if([fse objectType] == FILE_SYNC_FILE)
					{
						syncMessage = [NSString
							stringWithFormat:@"Downloading file: %@", 
							[fse name]];
						[iFolderWindowController updateStatusTS:syncMessage];
						if(updateLog)
							[self addLogTS:syncMessage];
					}
					else
					{
						syncMessage = [NSString
							stringWithFormat:@"Creating directory: %@", 
							[fse name]];
						[iFolderWindowController updateStatusTS:syncMessage];
						if(updateLog)
							[self addLogTS:syncMessage];
					}
				}
				break;
			}
		}
		
		if([fse sizeRemaining] == [fse sizeToSync])
		{
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

	}
	[fse release];
}




@end
