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
	nodeEventQueue = [[SMQueue alloc] init];
	colSyncEventQueue = [[SMQueue alloc] init];
	fileSyncEventQueue = [[SMQueue alloc] init];
	notifyEventQueue = [[SMQueue alloc] init];
	simiasEventDataLock	= [[NSLock alloc] init];
	simiasHasDataLock = [[NSConditionLock alloc] initWithCondition:NO_EVENTS];
	return self;
}


//===================================================================
// pushNotifyEvent
// pushes an SMNotifyEvent on the queue
//===================================================================
-(void)pushNotifyEvent:(SMNotifyEvent *)notifyEvent
{
	[simiasEventDataLock lock];
	[notifyEventQueue push:notifyEvent];
//	NSLog(@"SMNotifyEvent Pushed... count:%d", [notifyEventQueue count]);
	[simiasHasDataLock unlockWithCondition:HAS_EVENTS];
	[simiasEventDataLock unlock];
}

//===================================================================
// popNotifyEvent
// pops an SMNotifyEvent from the queue
//===================================================================
-(SMNotifyEvent *)popNotifyEvent
{
	SMNotifyEvent *ne;
	[simiasEventDataLock lock];
	ne = [[notifyEventQueue pop] retain];
//	NSLog(@"SMNotifyEvent Popped... count:%d", [notifyEventQueue count]);
	[simiasEventDataLock unlock];
	return [ne autorelease];
}
//===================================================================
// hasNotifyEvents
// determines if there are NotifyEvents
//===================================================================
- (BOOL) hasNotifyEvents
{
	BOOL hasEvents = false;
	[simiasEventDataLock lock];
	hasEvents = ![notifyEventQueue isEmpty];
	[simiasEventDataLock unlock];
	return hasEvents;
}

//===================================================================
// pushFileSyncEvent
// pushes an SMFileSyncEvent to the queue
//===================================================================
-(void)pushFileSyncEvent:(SMFileSyncEvent *)fileSyncEvent
{
	[simiasEventDataLock lock];
	[fileSyncEventQueue push:fileSyncEvent];
//	NSLog(@"SMFileSyncEvent pushed... count:%d", [fileSyncEventQueue count]);
	[simiasHasDataLock unlockWithCondition:HAS_EVENTS];
	[simiasEventDataLock unlock];
}
//===================================================================
// popFileSyncEvent
// pops an SMFileSyncEvent from the queue
//===================================================================
-(SMFileSyncEvent *)popFileSyncEvent
{
	SMFileSyncEvent *fse;
	[simiasEventDataLock lock];
	fse = [[fileSyncEventQueue pop] retain];
//	NSLog(@"SMFileSyncEvent popped... count:%d", [fileSyncEventQueue count]);
	[simiasEventDataLock unlock];
	return [fse autorelease];
}
//===================================================================
// hasFileSyncEvents
// determines if there are FileSyncEvents
//===================================================================
- (BOOL) hasFileSyncEvents
{
	BOOL hasEvents = false;
	[simiasEventDataLock lock];
	hasEvents = ![fileSyncEventQueue isEmpty];
	[simiasEventDataLock unlock];
	return hasEvents;
}




//===================================================================
// pushCollectionSyncEvent
// pushes an SMCollectionSyncEvent on the queue
//===================================================================
- (void) pushCollectionSyncEvent:(SMCollectionSyncEvent *)colSyncEvent
{
	[simiasEventDataLock lock];
	[colSyncEventQueue push:colSyncEvent];
//	NSLog(@"SMCollectionSyncEvent pushed... count:%d", [colSyncEventQueue count]);
	[simiasHasDataLock unlockWithCondition:HAS_EVENTS];
	[simiasEventDataLock unlock];
}
//===================================================================
// popCollectionSyncEvent
// pops an SMCollectionSyncEvent from the queue
//===================================================================
- (SMCollectionSyncEvent *) popCollectionSyncEvent
{
	SMCollectionSyncEvent *cse;
	[simiasEventDataLock lock];
	cse = [[colSyncEventQueue pop] retain];
//	NSLog(@"SMCollectionSyncEvent popped... count:%d", [colSyncEventQueue count]);
	[simiasEventDataLock unlock];
	return [cse autorelease];
}
//===================================================================
// hasCollectionSyncEvents
// determines if there are CollectionSyncEvents
//===================================================================
- (BOOL) hasCollectionSyncEvents
{
	BOOL hasEvents = false;
	[simiasEventDataLock lock];
	hasEvents = ![colSyncEventQueue isEmpty];
	[simiasEventDataLock unlock];
	return hasEvents;
}




//===================================================================
// pushNodeEvent
// pushes an SMNodeEvent on the queue
//===================================================================
- (void) pushNodeEvent:(SMNodeEvent *)nodeEvent
{
	[simiasEventDataLock lock];
	[nodeEventQueue push:nodeEvent];
//	NSLog(@"SMNodeEvent pushed... count:%d", [nodeEventQueue count]);
	[simiasHasDataLock unlockWithCondition:HAS_EVENTS];
	[simiasEventDataLock unlock];
}
//===================================================================
// popNodeEvent
// pops an SMNodeEvent from the queue
//===================================================================
- (SMNodeEvent *) popNodeEvent
{
	SMNodeEvent *ne;
	[simiasEventDataLock lock];
	ne = [[nodeEventQueue pop] retain];
//	NSLog(@"SMNodeEvent popped... count:%d", [nodeEventQueue count]);
	[simiasEventDataLock unlock];
	return [ne autorelease];
}
//===================================================================
// hasNodeEvents
// determines if there are Node Events
//===================================================================
- (BOOL) hasNodeEvents
{
	BOOL hasEvents = false;
	[simiasEventDataLock lock];
	hasEvents = ![nodeEventQueue isEmpty];
	[simiasEventDataLock unlock];
	return hasEvents;
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
