/***********************************************************************
 |  $RCSfile$
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
 |  Author: Calvin Gaisford <cgaisford@novell.com>
 | 
 ***********************************************************************/

#import <Cocoa/Cocoa.h>


#define PREFKEY_WINPOS			@"general.winposition"
#define PREFKEY_RESTOREWIN		@"general.restorewin"
#define PREFKEY_CLICKIFOLDER	@"general.clickifolder"
#define PREFKEY_NOTIFYIFOLDERS	@"notify.ifolders"
#define PREFKEY_NOTIFYCOLL		@"notify.collisions"
#define PREFKEY_NOTIFYUSER		@"notify.users"
#define PREFKEY_NOTIFYBYINDEX	@"notify.byindex"
#define PREFKEY_NOTIFYSOUND		@"notify.soundName"
#define STATE_SHOWMAINWINDOW	@"state.showmainwindow"
#define STATE_SHOWLOGWINDOW		@"state.showlogwindow"


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
		
	unsigned long					itemSyncCount;
	unsigned long					totalSyncCount;
}

//==========================================
// IBAction Methods
//==========================================
- (IBAction)showSyncLog:(id)sender;
- (IBAction)showAboutBox:(id)sender;
- (IBAction)showPrefs:(id)sender;
- (IBAction)showiFolderWindow:(id)sender;
- (IBAction)showHelp:(id)sender;



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

@end
