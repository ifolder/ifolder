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

#import "SimiasEventData.h"
#include "SMEvents.h"
#import "SMQueue.h"

#define NO_EVENTS 0
#define HAS_EVENTS 1

@implementation SimiasEventData

static SimiasEventData	*sharedSimiasEventData = nil;

+ (SimiasEventData *)sharedInstance
{
    @synchronized(self)
	{
        if (sharedSimiasEventData == nil)
		{
            sharedSimiasEventData = [[self alloc] init];
        }
    }
    return sharedSimiasEventData;
}

- (id)init 
{
	simiasEventQueue = [[SMQueue alloc] init];
	simiasEventDataLock	= [[NSLock alloc] init];
	simiasHasDataLock = [[NSConditionLock alloc] initWithCondition:NO_EVENTS];
	return self;
}


//===================================================================
// pushEvent
// pushes an Event on the queue
//===================================================================
-(void)pushEvent:(SMEvent *)event
{
	[simiasEventDataLock lock];
	[simiasEventQueue push:event];
	[simiasHasDataLock unlockWithCondition:HAS_EVENTS];
	[simiasEventDataLock unlock];
}

//===================================================================
// popEvent
// pops an Event from the queue
//===================================================================
-(SMEvent *)popEvent
{
	SMEvent *sme;
	[simiasEventDataLock lock];
	sme = [[simiasEventQueue pop] retain];
	[simiasEventDataLock unlock];
	return [sme autorelease];
}
//===================================================================
// hasNotifyEvents
// determines if there are events
//===================================================================
- (BOOL) hasEvents
{
	BOOL eventExist = false;
	[simiasEventDataLock lock];
	eventExist = ![simiasEventQueue isEmpty];
	[simiasEventDataLock unlock];
	return eventExist;
}



//===================================================================
// blockUntilEvents
// Blocks anyone calling until there are events in a queue
//===================================================================
-(void)blockUntilEvents
{
	[simiasHasDataLock lockWhenCondition:HAS_EVENTS];
	[simiasHasDataLock unlockWithCondition:NO_EVENTS];
}




// -------------- Singleton overrides --------------
+ (id)allocWithZone:(NSZone *)zone
{
    @synchronized(self)
	{
        if (sharedSimiasEventData == nil)
		{
            return [super allocWithZone:zone];
        }
    }
    return sharedSimiasEventData;
}
- (id)copyWithZone:(NSZone *)zone
{
    return self;
}

- (id)retain
{
    return self;
}
- (unsigned)retainCount
{
    return UINT_MAX;  //denotes an object that cannot be released
}
- (void)release
{
    //do nothing
}
- (id)autorelease
{
    return self;
}


@end
