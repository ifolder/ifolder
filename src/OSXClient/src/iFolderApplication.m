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
	[self startSimiasThread:self];

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
	runThreads = YES;

	NSLog(@"Creating and loading iFolderData");
	[ifolderdata refresh];
		
	// Startup the event processing thread
    [NSThread detachNewThreadSelector:@selector(simiasEventThread:)
        toTarget:self withObject:nil];

	[self showiFolderWindow:self];

	[iFolderWindowController updateStatusTS:@"Idle..."];
}




//===================================================================
// applicationWillTerminate
// Application Delegate method, called with the application is going
// to end
//===================================================================
- (void)applicationWillTerminate:(NSNotification *)notification
{
	runThreads = NO;
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
		
		threadWebService = [[iFolderService alloc] init];		
		
		// Startup simias Process
		[ [Simias getInstance] start];	

		
		while(!simiasRunning)
		{
			@try
			{
				NSLog(@"Pinging simias to check for startup...");
				simiasRunning = [threadWebService Ping];
			}
			@catch(NSException *e)
			{
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
		NSLog(@"simiasEventThread about to block");
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
		
		
		
		
		
		[self performSelectorOnMainThread:@selector(handleNodeEvent:) 
				withObject:ne waitUntilDone:YES ];				

		[ne release];
	}
}




//===================================================================
// handleNodeEvent
// this method does the work of updating status for the node
// event and MUST run on the main thread
//===================================================================
- (void)handleNodeEvent:(SMNodeEvent *)nodeEvent
{
	NSLog(@"iFolderApplication:handleNodeEvent called");

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

	if([ [iFolderData sharedInstance] isiFolder:[cse ID] ])
	{
		BOOL updateData = NO;
		
		iFolder *ifolder = [[[iFolderData sharedInstance] 
								getiFolder:[cse ID] updateData:NO] retain];

		if([cse syncAction] == SYNC_ACTION_START)
			[ifolder setIsSynchronizing:YES];
		else
		{
			[ifolder setIsSynchronizing:NO];
			if( [ [ifolder State] isEqualToString:@"WaitSync"])
				updateData = YES;
		}

		if( [ifolder Path] == nil )
			updateData = YES;
			
		if(updateData)
		{
			[[iFolderData sharedInstance] 
						getiFolder:[cse ID] updateData:YES];
		}

		[ifolder release];
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
