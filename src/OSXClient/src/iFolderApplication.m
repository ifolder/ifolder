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
	iFolderDomain *dom = [ifolderdata getDomain:domainID];
	if(dom != nil)
	{
		if(loginWindowController == nil)
		{
			loginWindowController = [[LoginWindowController alloc] initWithWindowNibName:@"LoginWindow"];
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
	[self addLog:@"Starting Simias Process"];
	NSLog(@"Starting Simias Process");
    [NSThread detachNewThreadSelector:@selector(startSimiasThread:)
        toTarget:self withObject:nil];

	ifolderdata = [[iFolderData alloc] init];
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
	NSLog(@"Creating and loading iFolderData");
	[ifolderdata refresh];

	[self addLog:@"initializing Simias Events"];
	NSLog(@"initializing Simias Events");
	[self initializeSimiasEvents];

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



@end
