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
*                 $Modified by: Satyam <ssutapalli@novell.com>  18-06-2008  Added notification for creating new iFolder
*                 $Modified by: Satyam <ssutapalli@novell.com>  16-07-2008  UI Refresh timer added
*-----------------------------------------------------------------------------
* This module is used to:
*               Main Application of iFolder
*
*******************************************************************************/

#import <Cocoa/Cocoa.h>


#define PREFKEY_WINPOS			@"general.winposition"
#define PREFKEY_WINONSTARTUP	@"general.winonstartup"
#define PREFKEY_CLICKIFOLDER	@"general.clickifolder"
#define STATE_SHOWMAINWINDOW	@"state.showmainwindow"
#define STATE_SHOWLOGWINDOW		@"state.showlogwindow"
#define PREFKEY_CREATEIFOLDER   @"notify.createifolder"

#define PREFKEY_NOTIFYIFOLDERS	@"notify.ifolders"
#define PREFKEY_NOTIFYCOLL		@"notify.collisions"
#define PREFKEY_NOTIFYUSER		@"notify.users"
#define PREFKEY_NOTIFYBYINDEX	@"notify.byindex"
#define PREFKEY_NOTIFYSOUND		@"notify.soundName"
#define PREFKEY_NOTIFYQUOTAVIOLATION    @"notify.quotaviolation"
#define PREFKEY_NOTIFYSIZEVIOLATION     @"notify.sizeviolation"
#define PREFKEY_NOTIFYEXCLUDEFILE       @"notify.excludefile"
#define PREFKEY_NOTIFYDISKFULL          @"notify.diskfull"
#define PREFKEY_NOTIFYPERMISSIONDENIED  @"notify.permissiondenied"
#define PREFKEY_NOTIFYPATHLENGTHEXCEEDS @"notify.pathlength"
#define PREFKEY_SYNCFAIL		@"notify.syncfail"


@class LoginWindowController;
@class iFolder;
@class iFolderData;
@class SMNotifyEvent;
@class SMFileSyncEvent;
@class SMCollectionSyncEvent;
@class SMNodeEvent;
@class AuthStatus;



@interface iFolderApplication : NSObject
{
	LoginWindowController			*loginWindowController;
	BOOL							runThreads;
	BOOL							simiasIsLoaded;
	NSMutableArray					*logEntries;
    IBOutlet NSArrayController		*logController;
	NSMutableDictionary				*readOnlyNotifications;	
	NSMutableDictionary				*iFolderFullNotifications;
	//NSMutableDictionary				*syncFailNotifications;
		
	unsigned long					itemSyncCount;
	unsigned long					totalSyncCount;
	NSTimer*                        refreshTimer;
}

//==========================================
// IBAction Methods
//==========================================
- (IBAction)showSyncLog:(id)sender;
- (IBAction)showAboutBox:(id)sender;
- (IBAction)showPrefs:(id)sender;
- (IBAction)showiFolderWindow:(id)sender;
- (IBAction)showHelp:(id)sender;
- (IBAction)update:(id)sender;



//==========================================
// All other methods
//==========================================
- (void)showLoginWindow:(NSString *)domainID;
- (void)addLog:(NSString *)entry;
- (void)initializeSimiasEvents;
- (NSArrayController *)logArrayController;
- (BOOL)simiasIsRunning;
- (void)clearLog;
- (void)setupApplicationDefaults;


//==========================================
// Thread Safe calls
//==========================================
- (void)addLogTS:(NSString *)entry;
- (void)showLoginWindowTS:(NSString *)domainID;


//==========================================
// NSApplication Delegates
//==========================================
- (void)applicationDidFinishLaunching:(NSNotification*)notification;
- (void)applicationWillTerminate:(NSNotification *)notification;
- (BOOL)applicationShouldHandleReopen:(NSApplication *)theApplication hasVisibleWindows:(BOOL)flag;
- (NSApplicationTerminateReply)applicationShouldTerminate:(NSApplication *)sender;


//==========================================
// Simias startup and shutdown methods
//==========================================
- (void)startSimias:(id)arg;
- (void)postSimiasInit:(id)arg;


//==========================================
// Simias Event thread methods
//==========================================
- (void)enableThreads:(id)arg;
- (void)simiasEventThread:(id)arg;

- (void)processNotifyEvent:(SMNotifyEvent *)smne;
- (void)processNodeEvent:(SMNodeEvent *)ne;
- (void)processCollectionSyncEvent:(SMCollectionSyncEvent *)cse;
- (void)processFileSyncEvent:(SMFileSyncEvent *)fse;

- (void)handleFileSyncEvent:(SMFileSyncEvent *)fileSyncEvent;
- (void)handleCollectionSyncEvent:(SMCollectionSyncEvent *)colSyncEvent;
- (void)processSubscriptionNodeEvent:(SMNodeEvent *)nodeNodeEvent;
- (void)processCollectionNodeEvent:(SMNodeEvent *)nodeNodeEvent;
- (void)processUserNodeEvent:(SMNodeEvent *)userNodeEvent;


//==========================================
// Proxy calls to read OS X Proxy Settings
//==========================================
- (NSString *)getHTTPProxyURI:(NSString *)host UseHTTPS:(BOOL)useHTTPS;
- (void) setupSimiasProxies:(NSString *)host;
- (void) setupProxyMonitor;

- (void) refreshTimerCall:(NSTimer*)refTimer;
- (void) stopRefreshTimer;
- (void) startRefreshTimer;
-(void)killSimias;
@end
