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

#import "MainWindowController.h"
#import "SyncLogWindowController.h"
#import "LoginWindowController.h"
#include "SimiasEventHandlers.h"



@implementation MainWindowController


-(id)init
{
    [super init];
    return self;
}




- (void)dealloc
{
    [super dealloc];
}




-(void)awakeFromNib
{
	webService = [[iFolderService alloc] init];

	[self addLog:@"initializing Simias Events"];

	[self initializeSimiasEvents];

	[self addLog:@"iFolder reading all domains"];

	@try
	{
		NSArray *newDomains = [webService GetDomains];

		[domainsController addObjects:newDomains];
		
		NSArray *newiFolders = [webService GetiFolders];
		if(newiFolders != nil)
		{
			[ifoldersController addObjects:newiFolders];
		}

		// if we have less than two domains, we don't have enterprise
		// so we better ask the user to login
		if([newDomains count] < 2)
			[self showLoginWindow:self];
		else
			[self showWindow:self];
	}
	@catch (NSException *e)
	{
		[self addLog:@"Reading domains failed with exception"];
		[self showWindow:self];
	}
}



- (IBAction)refreshWindow:(id)sender
{
	[self addLog:@"Refreshing iFolder view"];

	@try
	{
		NSArray *newiFolders = [webService GetiFolders];
		if(newiFolders != nil)
		{
			[ifoldersController setContent:newiFolders];
		}

		// if we have less than two domains, we don't have enterprise
		// so we better ask the user to login
//		if([newDomains count] < 2)
//			[self showLoginWindow:self];
//		else
	}
	@catch (NSException *e)
	{
		[self addLog:@"Refreshing failed with exception"];
	}
}




- (IBAction)showLoginWindow:(id)sender
{
	if(loginController == nil)
	{
		loginController = [[LoginWindowController alloc] initWithWindowNibName:@"LoginWindow"];
	}
	
	[[loginController window] center];
	[loginController showWindow:self];
}


- (IBAction)showSyncLog:(id)sender
{
	if(syncLogController == nil)
	{
		syncLogController = [[SyncLogWindowController alloc] initWithWindowNibName:@"SyncLogWindow"];
	}
	
	[syncLogController showWindow:self];
}



-(void)login:(NSString *)username withPassword:(NSString *)password toServer:(NSString *)server
{
	@try
	{
		iFolderDomain *domain = [webService ConnectToDomain:username usingPassword:password andHost:server];
		[domainsController addObject:domain];
		[self refreshWindow:self];
		[self showWindow:self];

	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		NSRunAlertPanel(@"Error connecting to Server", [e name], @"OK",nil, nil);
	}

}




- (void)createiFolder:(NSString *)path inDomain:(NSString *)domainID
{
	@try
	{
		iFolder *newiFolder = [webService CreateiFolder:path InDomain:domainID];
		[ifoldersController addObject:newiFolder];
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		NSRunAlertPanel(@"Error connecting to Server", [e name], @"OK",nil, nil);
	}
}




- (void)AcceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath
{
	@try
	{
		iFolder *newiFolder = [webService AcceptiFolderInvitation:iFolderID InDomain:domainID toPath:localPath];
		[ifoldersController addObject:newiFolder];
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		NSRunAlertPanel(@"Error connecting to Server", [e name], @"OK",nil, nil);
	}
}




- (void)addDomain:(iFolderDomain *)newDomain
{
	[domainsController addObject:newDomain];
}




- (void)addiFolder:(iFolder *)newiFolder
{
	[ifoldersController addObject:newiFolder];
}



- (void)addLog:(NSString *)entry
{
	if(syncLogController != nil)
	{
		[syncLogController logEntry:entry];
	}
}

- (void)initializeSimiasEvents
{
	SimiasEventInitialize();
}





@end
