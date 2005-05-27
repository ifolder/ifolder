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
#import "iFolderNotificationController.h"
#import "config.h"
#import "AuthStatus.h"
#include "simiasStub.h"


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
// showHelp
// Shows the main iFolder Window
//===================================================================
- (IBAction)showHelp:(id)sender
{
	[[NSWorkspace sharedWorkspace] openURL:[NSURL URLWithString:
		[NSString stringWithFormat:@"file://localhost%s/share/ifolder-se/help/en/doc/user/data/front.html", IFOLDER_PREFIX]]];
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

	[iFolderWindowController updateStatusTS:NSLocalizedString(@"Loading synchronization process...", nil)];
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
													PREFKEY_NOTIFYSOUND,
													nil];

	NSArray *values = [NSArray arrayWithObjects:	[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithInt:0],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithBool:YES],
													[NSNumber numberWithInt:0],
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

	NSLog(@"Creating and loading iFolderData");
	[ifolderdata refresh:NO];
	
	// Startup the event processing thread
    [NSThread detachNewThreadSelector:@selector(simiasEventThread:)
        toTarget:self withObject:nil];

	[iFolderWindowController updateStatusTS:NSLocalizedString(@"Idle...", nil)];

	// If there are no domains when the app first launches, the show a dialog that
	// will help get an account created
	if([ifolderdata getDomainCount] < 1)
	{
		int rc;
		
		rc = NSRunAlertPanel(NSLocalizedString(@"Setup  iFolder Account", nil), 
							NSLocalizedString(@"The iFolder client is now installed and ready to use.  To begin using iFolder, you must first setup an iFolder account.  Would you like to setup your iFolder account now?", nil),
							NSLocalizedString(@"Yes", nil), 
							NSLocalizedString(@"No", nil),
							nil);

		if(rc == NSAlertDefaultReturn)
		{
			[[iFolderPrefsController sharedInstance] showAccountsWindow];
		}
	}
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
	[self addLog:NSLocalizedString(@"Shutting down Simias...", nil)];
	[ [Simias getInstance] stop];
	[self addLog:NSLocalizedString(@"Simias is shut down", nil)];

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
//			else
//			{
//				NSLog(@"***** UNHANDLED EVENT ****  Type: %@", [sme eventType]);
//			}
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
	NSLog(@"Received a \"%@\" event with message \"%@\"", [smne type], [smne message]);

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
			NSLog(@"Exception Getting Domain Password %@", [ex name]);
		}

		if(savedPassword != nil)
		{
			NSLog(@"Credentials found on domain, authenticating...");

			@try
			{
				AuthStatus *authStatus = [[simiasService LoginToRemoteDomain:[smne message] 
											usingPassword:savedPassword] retain];

				unsigned int statusCode = [[authStatus statusCode] unsignedIntValue];

				[authStatus release];
			
				if(	(statusCode == ns1__StatusCodes__Success) ||
					(statusCode == ns1__StatusCodes__SuccessInGrace) )
					NSLog(@"Successfully authenticated to domain %@", [smne message]);
				else
				{
					NSLog(@"Unable to authenticate, status: %d", statusCode);
					showLoginDialog = YES;
				}
			}
			@catch (NSException *e)
			{
				NSLog(@"Exception authenticating to domain: %@", [e name]);
				showLoginDialog = YES;
			}			
		}
		else
			showLoginDialog = YES;

		[simiasService release];

		if(showLoginDialog)
			[self showLoginWindowTS:[smne message]];
	}
}




//===================================================================
// processNodeEvents
// method to loop through all node events and process them
//===================================================================
- (void)processNodeEvent:(SMNodeEvent *)ne
{
	// Handle all Node events here
	if([[ne type] compare:@"Subscription"] == 0)
	{
		// First check to see if this is a POBox 'cause we
		// don't care much about it if'n it aint.
		if([[iFolderData sharedInstance] isPOBox:[ne collectionID]])
		{
			[self processSubscriptionNodeEvent:ne];
		}
	}
	else if([[ne type] compare:@"Collection"] == 0)
	{
		[self processCollectionNodeEvent:ne];
	}
	else if([[ne type] compare:@"Member"] == 0)
	{
		[self processUserNodeEvent:ne];
	}
//	else
//	{
//		NSLog(@"***** UNHANDLED NODE EVENT ****  Type: %@", [ne type]);
//	}

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
//			NSLog(@"processCollectionNodeEvent NODE_DELETED");

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
					NSLog(@"Collection is not syncing, processing a NODE_CHANGED on a collection");

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
//			NSLog(@"processSubscriptionNodeEvent NODE_CREATED");
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
//			NSLog(@"processSubscriptionNodeEvent NODE_DELETED");

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
//			NSLog(@"processSubscriptionNodeEvent NODE_CHANGED");
	
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
//			NSLog(@"user created, marking as such in iFolderSharedData");
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
		switch([cse syncAction])
		{
			case SYNC_ACTION_LOCAL:
			{
				NSString *syncMessage = NSLocalizedString(@"Checking for new iFolders...", nil);
				[iFolderWindowController updateStatusTS:syncMessage];
				[self addLogTS:syncMessage];
				break;
			}
			case SYNC_ACTION_STOP:
			{
				NSString *syncMessage = NSLocalizedString(@"Done checking for new iFolders", nil);
				[iFolderWindowController updateStatusTS:NSLocalizedString(@"Idle...", nil)];
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
			}

			NSString *syncMessage = [NSString
							stringWithFormat:NSLocalizedString(@"Preparing to synchronize: %@", nil), 
							[cse name]];
			[iFolderWindowController updateStatusTS:syncMessage];
			[self addLogTS:syncMessage];
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
			}
		
			NSString *syncMessage = [NSString
							stringWithFormat:NSLocalizedString(@"Synchronizing: %@", nil), 
							[cse name]];
			[iFolderWindowController updateStatusTS:syncMessage];
			[self addLogTS:syncMessage];
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
					NSLog(@"iFolder has collisions, notifying user");
					[iFolderNotificationController collisionNotification:ifolder];								
				}

				if([[iFolderData sharedInstance] usersAdded:[cse ID]])
				{
					[iFolderNotificationController newUserNotification:ifolder];								
				}
			}
		
			NSString *syncMessage;
			SyncSize *ss = [[[iFolderData sharedInstance] getSyncSize:[cse ID]] retain];			
			if([ss SyncNodeCount] == 0)
			{
				if([cse connected])
				{
					[ifolder setSyncState:SYNC_STATE_OK];
					syncMessage = [NSString
						stringWithFormat:NSLocalizedString(@"Done synchronizing: %@", nil), 
						[cse name]];
				}
				else
				{
					[ifolder setSyncState:SYNC_STATE_DISCONNECTED];
					syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Sync failed to connect: %@", nil), 
								[cse name]];
				}
			}
			else
			{
				[ifolder setOutOfSyncCount:[ss SyncNodeCount]];
				[ifolder setSyncState:SYNC_STATE_OUT_OF_SYNC];
				syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Out of Sync: %@", nil), 
								[cse name]];
			}
			[ss release];

			[iFolderWindowController updateStatusTS:NSLocalizedString(@"Idle...", nil)];
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

	if([fse objectType] != FILE_SYNC_UNKNOWN)
	{
		if([[fse status] compare:@"Success"] == 0)
		{
			BOOL updateLog = NO;

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
			
			syncItemMessage = [NSString stringWithFormat:NSLocalizedString(@"%u of %u items - ", nil),
								itemSyncCount, totalSyncCount];

			switch([fse direction])
			{
				case FILE_SYNC_LOCAL:
				{
					if([fse objectType] == FILE_SYNC_FILE)
					{
						[self addLogTS:[NSString
							stringWithFormat:NSLocalizedString(@"Found local change in file: %@", nil), 
							[fse name]]];
					}
					else
					{
						[self addLogTS:[NSString
							stringWithFormat:NSLocalizedString(@"Found local change in folder: %@", nil), 
							[fse name]]];
					}	
					break;
				}
				case FILE_SYNC_UPLOADING:
				{
					if([fse isDelete])
					{
						if([fse objectType] == FILE_SYNC_FILE)
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Removing file from server: %@", nil), 
								[fse name]];
							[iFolderWindowController updateStatusTS:
								[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
							if(updateLog)
								[self addLogTS:syncMessage];
						}
						else
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Removing folder from server: %@", nil), 
								[fse name]];
							[iFolderWindowController updateStatusTS:
								[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
							if(updateLog)
								[self addLogTS:syncMessage];
						}	
					}
					else
					{
						if([fse objectType]  == FILE_SYNC_FILE)
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Uploading file: %@", nil), 
								[fse name]];
							[iFolderWindowController updateStatusTS:
								[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
							if(updateLog)
								[self addLogTS:syncMessage];
						}
						else
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Creating folder on server: %@", nil), 
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
								stringWithFormat:NSLocalizedString(@"Deleting file: %@", nil), 
								[fse name]];
							[iFolderWindowController updateStatusTS:
								[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
							if(updateLog)
								[self addLogTS:syncMessage];
						}
						else
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Removing folder: %@", nil), 
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
								stringWithFormat:NSLocalizedString(@"Downloading file: %@", nil), 
								[fse name]];
							[iFolderWindowController updateStatusTS:
								[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
							if(updateLog)
								[self addLogTS:syncMessage];
						}
						else
						{
							syncMessage = [NSString
								stringWithFormat:NSLocalizedString(@"Creating folder: %@", nil), 
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

			syncItemMessage = [NSString stringWithFormat:NSLocalizedString(@"%u of %u items - ", nil),
								itemSyncCount, totalSyncCount];

			[iFolderWindowController updateProgress:100
									withMin:0
									withMax:100];

			// Conflict message
			if(	([[fse status] compare:@"UpdateConflict"] == 0) ||
				([[fse status] compare:@"FileNameConflict"] == 0) )
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"Conflict occurred for: %@", nil), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
			}
			// Policy does not allow this file
			else if([[fse status] compare:@"Policy"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"A policy prevented a sync of: %@", nil), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
			}
			// Insuficient rights
			else if([[fse status] compare:@"Access"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"Insuficient rights to sync: %@", nil), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
			}
			// Locked
			else if([[fse status] compare:@"Locked"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"iFolder is locked and did not sync: %@", nil), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
			}
			// PolicyQuota
			else if([[fse status] compare:@"PolicyQuota"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"iFolder is full and did not sync: %@", nil), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
			}
			// PolicySize
			else if([[fse status] compare:@"PolicySize"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"A size restriction policy prevented a sync of: %@", nil), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
			}
			// PolicyType
			else if([[fse status] compare:@"PolicyType"] == 0)
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"A file type restriction policy prevented a sync of: %@", nil), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
			}
			// All other errors
			else
			{
				syncMessage = [NSString
					stringWithFormat:NSLocalizedString(@"iFolder failed to sync: %@", nil), 
					[fse name]];
				[iFolderWindowController updateStatusTS:
					[NSString stringWithFormat:@"%@%@", syncItemMessage, syncMessage]];
				[self addLogTS:syncMessage];
			}

		}
	}
	[fse release];
}








@end