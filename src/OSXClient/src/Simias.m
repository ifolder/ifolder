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
    
    return sharedInstance;
}


+ (Simias *)getInstance
{
    return sharedInstance ? sharedInstance : [[self alloc] init];
}


//Starts the simias process
-(void)start
{
	if(simiasTask == nil)
	{
		NSArray *parameters = [NSArray arrayWithObjects:nil];

		simiasTask = [[NSTask alloc] init];
		stdInPipe = [[NSPipe alloc] init];
		[simiasTask setLaunchPath:[NSString stringWithCString:SIMIAS_BINARY]];
		[simiasTask setArguments:parameters];
		[simiasTask setStandardInput:stdInPipe];

	    stdInHandle = [stdInPipe fileHandleForWriting];
	}

	if(![simiasTask isRunning])
		[simiasTask launch];
}



//Stops the simias process
-(void)stop
{
	if(simiasTask != nil)
	{
		char *returnChar = "\r\n";

		if([simiasTask isRunning])
		{
			int termcount = 15;

			NSLog(@"Sending a signal to Simias to terminate");
						
			[stdInHandle writeData:[NSData dataWithBytes:returnChar length:2]];

//			[NSThread sleepUntilDate:[[NSDate date] addTimeInterval:2]];

			// for some reason, waitUntilExit was crashing but this loop doesn't
			// so, I'll stick with my own loop
			// I also added a nice countdown.  If simias can't end in 10 seconds,
			// we are going to make it end
			while(	(simiasTask != nil) &&
					([simiasTask isRunning]) )
			{
				if(termcount == 5)
				{
					NSLog(@"Interrupting the simias process");
					[simiasTask interrupt];
					[NSThread sleepUntilDate:[[NSDate date] addTimeInterval:2]];
					termcount--;					
				}
				else if(termcount == 0)
				{
					NSLog(@"Terminating the simias process");
					[simiasTask terminate];
					[NSThread sleepUntilDate:[[NSDate date] addTimeInterval:1]];
				}
				else
				{
					NSLog(@"Waiting to terminate Simias: %d", termcount);
					[NSThread sleepUntilDate:[[NSDate date] addTimeInterval:2]];
					termcount--;
				}
			}
//			[simiasTask waitUntilExit];
		}	
	}
}


@end
