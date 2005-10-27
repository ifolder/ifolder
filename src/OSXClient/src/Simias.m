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

#import "Simias.h"
#import "config.h"

#import <netinet/in.h>
#include <arpa/inet.h>
#include <errno.h>
#include <sys/sysctl.h>
#include <signal.h>
#include "applog.h"


@implementation Simias

static Simias *sharedInstance = nil;


- (id)init 
{
    if (sharedInstance) 
	{
        [self dealloc];
    } 
	else
	{
        sharedInstance = [super init];
    }
	
	simiasManager = AllocateManager();
	SetShowConsole(simiasManager, 1);
	SetVerbose(simiasManager, 1);
    return sharedInstance;
}


-(void) dealloc
{
	FreeManager(simiasManager);
	[super dealloc];
}



+ (Simias *)getInstance
{
    return sharedInstance ? sharedInstance : [[self alloc] init];
}



-(NSString *)simiasURL
{
	const char *url = GetWebServiceUrl(simiasManager);
	if(url == NULL)
		return @"";
	else
		return [NSString stringWithCString:url];
}



//Starts the simias process
-(BOOL)start
{
	if( Start(simiasManager) !=  NULL )
	{
		return YES;
	}
	else
		return NO;
}



//Stops the simias process
-(BOOL)stop
{
	if(Stop(simiasManager))
		return YES;
	else
		return NO;
}



@end
