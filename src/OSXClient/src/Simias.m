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
    
    return sharedInstance;
}


+ (Simias *)getInstance
{
    return sharedInstance ? sharedInstance : [[self alloc] init];
}



-(NSString *)simiasURL
{
	return simiasurl;
}



//Starts the simias process
-(BOOL)start
{
	if(simiasTask == nil)
	{
		int SimiasPID;

		SimiasPID = [self getCurrentSimiasPID];
		
		if(SimiasPID != 0)
		{
			// We need to kill the current Simias PID!
			ifconlog2(@"*********Simias process found on PID: %d", SimiasPID);
		}

		if(SimiasPID == 0)
		{
			NSArray *parameters = nil;
			
			// Detect if the std port is available
			NSSocketPort *tmpPort = [[NSSocketPort alloc] initWithTCPPort:0];
			if(tmpPort == nil)
				return NO;
				
			struct sockaddr_in addrIn = *(struct sockaddr_in*)[[tmpPort address] bytes];
			NSString *port = [NSString stringWithFormat:@"%d", addrIn.sin_port];
			[tmpPort release];
			tmpPort = nil;
			parameters = [NSArray arrayWithObjects:port, nil];

			simiasurl = [[NSString stringWithFormat:@"http://127.0.0.1:%@", port] retain];

			simiasTask = [[NSTask alloc] init];
			stdInPipe = [[NSPipe alloc] init];
			[simiasTask setLaunchPath:[NSString stringWithCString:SIMIAS_BINARY]];
			[simiasTask setArguments:parameters];
			[simiasTask setStandardInput:stdInPipe];

			stdInHandle = [stdInPipe fileHandleForWriting];
		}
	}

	if((simiasTask != nil) && (![simiasTask isRunning]))
		[simiasTask launch];

	if((simiasTask != nil) && ([simiasTask isRunning]))
		return YES;
	else
		return NO;
}



//Stops the simias process
-(BOOL)stop
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
	else
	{
		int SimiasPID;

		SimiasPID = [self getCurrentSimiasPID];
	
		if(SimiasPID > 0)
		{
			// We need to kill the current Simias PID!
			ifconlog2(@"iFolder killing process PID: %d", SimiasPID);
			kill(SimiasPID, SIGKILL);
			ifconlog2(@"Waiting for process to die PID: %d", SimiasPID);
			sleep(3);
			SimiasPID = [self getCurrentSimiasPID];
			while(SimiasPID > 0)
			{
				ifconlog2(@"Waiting for process to die PID: %d", SimiasPID);
				sleep(2);
				SimiasPID = [self getCurrentSimiasPID];
			}
			ifconlog2(@"Process is dead PID: %d", SimiasPID);
			sleep(5);
			popen("rm -rf ~/.wapi", "r");
		}
	}
	return YES;
}




-(int) getCurrentSimiasPID
{
	int simiasPID = 0;
    int	mib[6] = {0,0,0,0,0,0}; //used for sysctl call.
    int	SuccessfullyGotProcessInformation;
    size_t sizeOfBufferRequired = 0; //set to zero to start with.
    int error = 0;
    long NumberOfRunningProcesses = 0;
    unsigned int Counter = 0;
    struct kinfo_proc* BSDProcessInformationStructure = NULL;
    pid_t CurrentExaminedProcessPID = 0;
    char* CurrentExaminedProcessName = NULL;

    mib[0] = CTL_KERN;
    mib[1] = KERN_PROC;
    mib[2] = KERN_PROC_ALL;

    SuccessfullyGotProcessInformation = FALSE;
    while (SuccessfullyGotProcessInformation == FALSE)
    {
        error = sysctl(mib, 3, NULL, &sizeOfBufferRequired, NULL, 0);

        if (error != 0) 
        {
			// if we got an error, just return 0
			return 0;
        }
    
        BSDProcessInformationStructure = (struct kinfo_proc*) malloc(sizeOfBufferRequired);

        if (BSDProcessInformationStructure == NULL)
        {
			return 0;
        }
    
        error = sysctl(mib, 3, BSDProcessInformationStructure, &sizeOfBufferRequired, NULL, 0);
    
        //Here we successfully got the process information.  Thus set the variable to end this sysctl calling loop
        if (error == 0)
        {
            SuccessfullyGotProcessInformation = TRUE;
        }
        else 
        {
			// free it and loop, the processes may have changed
            free(BSDProcessInformationStructure); 
        }
    }//end while loop

    NumberOfRunningProcesses = sizeOfBufferRequired / sizeof(struct kinfo_proc);  
    
    for (Counter = 0 ; Counter < NumberOfRunningProcesses ; Counter++)
    {
        //Getting PID of process we are examining
        CurrentExaminedProcessPID = BSDProcessInformationStructure[Counter].kp_proc.p_pid; 
    
        //Getting name of process we are examining
        CurrentExaminedProcessName = BSDProcessInformationStructure[Counter].kp_proc.p_comm; 

        if ((CurrentExaminedProcessPID > 0) //Valid PID
           && ((strncmp(CurrentExaminedProcessName, "mono", MAXCOMLEN) == 0))) //name matches
        {	
			// now get the command line arguments and see if any start SimiasServer.exe
			int mib2[4];
			char *arguments;
			size_t arguments_size = 0;
			char *endarg;

			mib2[0] = CTL_KERN;
			mib2[1] = KERN_PROCARGS;
			mib2[2] = CurrentExaminedProcessPID;
			mib2[3] = 0;

			if(sysctl(mib2, 3, NULL, &arguments_size, NULL, 0) >= 0)
			{
				arguments = (char *) malloc(arguments_size);
				if (sysctl(mib2, 3, arguments, &arguments_size, NULL, 0) >= 0)
				{
					char *curarg;

					curarg = &arguments[0];
					endarg = &(arguments[arguments_size]);
					
					while(curarg < endarg)
					{
						if(*curarg != 0)
						{
							if(strstr(curarg, "SimiasServer.exe") != NULL)
								simiasPID = CurrentExaminedProcessPID;
						}

						while(*curarg != 0)
							curarg++;
					
						// skip the zero at the end of the string
						curarg++;
					}
				}		
			}
        }
    }//end looking through process list

    free(BSDProcessInformationStructure); //done with allocated buffer so release.

	return simiasPID;
}


@end
