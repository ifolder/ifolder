/***********************************************************************
 *  iFolderStatusIconApp.m
 * 
 *  Copyright (C) 2006 Novell, Inc.
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

#import "iFolderStatusIconApp.h"

#include <ifolder-client.h>

@implementation iFolderStatusIconApp

//===================================================================
// awakeFromNib
// When this class is loaded from the nib
//===================================================================
-(void)awakeFromNib
{
	int err;
	appStatusItem = [[[NSStatusBar systemStatusBar] statusItemWithLength:NSVariableStatusItemLength] retain];
	[appStatusItem setMenu:appMenu];
	[appStatusItem setHighlightMode:YES];	// if this is not set, your menu will not highlight when clicked
	
	// FIXME: Change the icon to ifolder-starting-up.png
	[appStatusItem setImage:[NSImage imageNamed:@"ifolderbw22"]];		// you do have an icon, right?

	err = ifolder_client_initialize();
	if (err == IFOLDER_SUCCESS)
	{
		NSLog(@"The iFolder Client initialized successfully!");

		// FIXME: Change the icon to ifolder-idle.png
		[appStatusItem setImage:[NSImage imageNamed:@"ifolderbw22"]];		// you do have an icon, right?
	}
	else
	{
		NSLog(@"The iFolder Client failed to initialize: %d", err);
		// FIXME: popup an error and don't let the app continue
	}
}

//===================================================================
// showiFolders
// bring up the main iFolder window
//===================================================================
- (IBAction)showiFolders:(id)sender
{
}

//===================================================================
// startFullSync
// start a synchronization for every iFolder (one by one)
//===================================================================
- (IBAction)startFullSync:(id)sender
{
	[appStatusItem setImage:[NSImage imageNamed:@"syncbw22"]];
}

//===================================================================
// stopSync
// halt all synchronization activity
//===================================================================
- (IBAction)stopSync:(id)sender
{
	[appStatusItem setImage:[NSImage imageNamed:@"ifolderbw22"]];
}

//===================================================================
// showPreferences
// bring up the preferences window
//===================================================================
- (IBAction)showPreferences:(id)sender
{
}

//===================================================================
// showHelp
// bring up the user documentation
//===================================================================
- (IBAction)showHelp:(id)sender
{
}

//===================================================================
// showAbout
// bring up the about screen
//===================================================================
- (IBAction)showAbout:(id)sender
{
}



//===================================================================
// applicationDidFinishLaunching
// Application Delegate method, called when the application is done
// loading
//===================================================================
- (void)applicationDidFinishLaunching:(NSNotification*)notification
{
	// for later stuff
}


//===================================================================
// applicationWillTerminate
// Application Delegate method, called with the application is going
// to end
//===================================================================
- (void)applicationWillTerminate:(NSNotification *)notification
{
	// for later stuff
	int err = ifolder_client_uninitialize();
	if (err != IFOLDER_SUCCESS)
		NSLog(@"There was an error uninitializing the iFolder Client: %d", err);
	else
		NSLog(@"The iFolder Client uninitialized successfully.");
}


@end
