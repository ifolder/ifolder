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

#import "SyncLogWindowController.h"
#import "iFolderApplication.h"

@implementation SyncLogWindowController


static SyncLogWindowController *syncLogInstance = nil;


+ (SyncLogWindowController *)sharedInstance
{
	if(syncLogInstance == nil)
	{
		syncLogInstance = [[SyncLogWindowController alloc] initWithWindowNibName:@"LogWindow"];
	}

    return syncLogInstance;
}




- (void)windowWillClose:(NSNotification *)aNotification
{
	if(syncLogInstance != nil)
	{
		[syncLogInstance release];
		syncLogInstance = nil;
	}
}




-(void)awakeFromNib
{
	NSLog(@"SyncLogWindowController Awoke from Nib");
	[super setShouldCascadeWindows:NO];
	[super setWindowFrameAutosaveName:@"ifolder_log_window"];
	
    NSMutableDictionary *bindingOptions = [NSMutableDictionary dictionary];
    	
	// binding options for "name"
	[bindingOptions setObject:@"No Name" forKey:@"NSNullPlaceholder"];

//    [accountsColumn bind:@"value" toObject:[[NSApp delegate] DomainsController]
//	  withKeyPath:@"arrangedObjects.properties.Name" options:bindingOptions];
	
	// bind the table column to the log to display it's contents
	[logColumn bind:@"value" toObject:[[NSApp delegate] logArrayController]
					withKeyPath:@"arrangedObjects" options:bindingOptions];	
}




- (IBAction)clearLog:(id)sender
{
}




- (IBAction)saveLog:(id)sender
{
}



@end
