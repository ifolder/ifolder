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

#include "SimiasEventHandlers.h"
#include "iFolderApplication.h"
#include "SMEvents.h"
#include "SimiasEventData.h"

SimiasEventClient simiasEventClient;


void SimiasEventInitialize(void)
{
	if(sec_init (&simiasEventClient, SimiasEventStateCallBack, &simiasEventClient) != 0)
	{
		[[NSApp delegate] addLogTS:NSLocalizedString(@"Error initializing the events", nil)];
		return;
	}

	if(sec_register(simiasEventClient) != 0)
	{
		[[NSApp delegate] addLogTS:NSLocalizedString(@"Error registering the events", nil)];
		return;
	}

	[[NSApp delegate] addLogTS:NSLocalizedString(@"Events initialized and registered", nil)];
}


void SimiasEventDisconnect(void)
{
	sec_set_event(simiasEventClient, ACTION_NODE_CREATED, false, nil, nil);
	sec_set_event(simiasEventClient, ACTION_NODE_DELETED, false, nil, nil);
	sec_set_event(simiasEventClient, ACTION_NODE_CHANGED, false, nil, nil);

	sec_set_event(simiasEventClient, ACTION_COLLECTION_SYNC, false, nil, nil);
	sec_set_event(simiasEventClient, ACTION_FILE_SYNC, false, nil, nil);
	sec_set_event(simiasEventClient, ACTION_NOTIFY_MESSAGE, false, nil, nil);

	if(sec_deregister(simiasEventClient) != 0)
	{
		[[NSApp delegate] addLogTS:NSLocalizedString(@"Error deregistering the events", nil)];
		return;
	}

	[[NSApp delegate] addLogTS:NSLocalizedString(@"events de-registered", nil)];
}


int SimiasEventStateCallBack(SEC_STATE_EVENT state_event, const char *message, void *data)
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];
	
	SimiasEventClient *sec = (SimiasEventClient *)data;

	switch(state_event)
	{
		case SEC_STATE_EVENT_CONNECTED:
			NSLog(@"Event client connected");
			[[NSApp delegate] simiasHasStarted];
			sec_set_event(*sec, ACTION_NODE_CREATED, true, (SimiasEventFunc)SimiasEventNode, nil);
			sec_set_event(*sec, ACTION_NODE_DELETED, true, (SimiasEventFunc)SimiasEventNode, nil);
			sec_set_event(*sec, ACTION_NODE_CHANGED, true, (SimiasEventFunc)SimiasEventNode, nil);

			sec_set_event(*sec, ACTION_COLLECTION_SYNC, true, (SimiasEventFunc)SimiasEventSyncCollection, nil);
			sec_set_event(*sec, ACTION_FILE_SYNC, true, (SimiasEventFunc)SimiasEventSyncFile, nil);
			sec_set_event(*sec, ACTION_NOTIFY_MESSAGE, true, (SimiasEventFunc)SimiasEventNotifyMessage, nil);
			break;
		case SEC_STATE_EVENT_DISCONNECTED:
			NSLog(@"Event client disconnected!");
			break;
		case SEC_STATE_EVENT_ERROR:
			NSLog(@"ERROR with Simias Event Client");
			break;
	}
    [pool release];	
	return 0;
}


int SimiasEventNode(SimiasNodeEvent *nodeEvent, void *data)
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];
//	NSLog(@"SimiasNodeEvent fired: %s", nodeEvent->node);

	NSDictionary *neProps = [getNodeEventProperties(nodeEvent) retain];
	SMNodeEvent *ne = [[SMNodeEvent alloc] init];
	[ne setProperties:neProps];
	[[SimiasEventData sharedInstance] pushEvent:ne];

	[neProps release];
	[ne release];
    [pool release];	
    return 0;
}
NSDictionary *getNodeEventProperties(SimiasNodeEvent *nodeEvent)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];

	if(nodeEvent->event_type != nil)
		[newProperties setObject:[NSString stringWithUTF8String:nodeEvent->event_type] forKey:@"eventType"];
	if(nodeEvent->action != nil)
		[newProperties setObject:[NSString stringWithUTF8String:nodeEvent->action] forKey:@"action"];
	if(nodeEvent->time != nil)
		[newProperties setObject:[NSString stringWithUTF8String:nodeEvent->time] forKey:@"time"];
	if(nodeEvent->source != nil)
		[newProperties setObject:[NSString stringWithUTF8String:nodeEvent->source] forKey:@"source"];
	if(nodeEvent->collection != nil)
		[newProperties setObject:[NSString stringWithUTF8String:nodeEvent->collection] forKey:@"collectionID"];
	if(nodeEvent->type != nil)
		[newProperties setObject:[NSString stringWithUTF8String:nodeEvent->type] forKey:@"type"];
	if(nodeEvent->event_id != nil)
		[newProperties setObject:[NSString stringWithUTF8String:nodeEvent->event_id] forKey:@"event_id"];
	if(nodeEvent->node != nil)
		[newProperties setObject:[NSString stringWithUTF8String:nodeEvent->node] forKey:@"nodeID"];
	if(nodeEvent->flags != nil)
		[newProperties setObject:[NSString stringWithUTF8String:nodeEvent->flags] forKey:@"flags"];
	if(nodeEvent->master_rev != nil)
		[newProperties setObject:[NSString stringWithUTF8String:nodeEvent->master_rev] forKey:@"master_rev"];
	if(nodeEvent->slave_rev != nil)
		[newProperties setObject:[NSString stringWithUTF8String:nodeEvent->slave_rev] forKey:@"slave_rev"];
	if(nodeEvent->file_size != nil)
		[newProperties setObject:[NSString stringWithUTF8String:nodeEvent->file_size] forKey:@"file_size"];

	return [newProperties autorelease];
}




//===================================================================
// CollectionSyncEvents Handlers
//===================================================================
int SimiasEventSyncCollection(SimiasCollectionSyncEvent *collectionEvent, void *data)
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];
//	NSLog(@"SimiasCollectionSyncEvent fired: %s", collectionEvent->name);

	NSDictionary *cseProps = [getCollectionSyncEventProperties(collectionEvent) retain];
	SMCollectionSyncEvent *cse = [[SMCollectionSyncEvent alloc] init];
	[cse setProperties:cseProps];
	[[SimiasEventData sharedInstance] pushEvent:cse];


	
	[cseProps release];
	[cse release];
    [pool release];	
    return 0;
}
NSDictionary *getCollectionSyncEventProperties(SimiasCollectionSyncEvent *collectionEvent)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];

	if(collectionEvent->event_type != nil)
		[newProperties setObject:[NSString stringWithUTF8String:collectionEvent->event_type] forKey:@"eventType"];
	if(collectionEvent->name != nil)
		[newProperties setObject:[NSString stringWithUTF8String:collectionEvent->name] forKey:@"name"];
	if(collectionEvent->id != nil)
		[newProperties setObject:[NSString stringWithUTF8String:collectionEvent->id] forKey:@"ID"];
	if(collectionEvent->action != nil)
		[newProperties setObject:[NSString stringWithUTF8String:collectionEvent->action] forKey:@"action"];
	if(collectionEvent->connected != nil)
		[newProperties setObject:[NSString stringWithUTF8String:collectionEvent->connected] forKey:@"connected"];

	return [newProperties autorelease];
}





//===================================================================
// FileSyncEvents Handlers
//===================================================================
int SimiasEventSyncFile(SimiasFileSyncEvent *fileEvent, void *data)
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];
//	NSLog(@"SimiasFileSyncEvent fired: %s", fileEvent->name);

	NSDictionary *fseProps = [getFileSyncEventProperties(fileEvent) retain];
	SMFileSyncEvent *fse = [[SMFileSyncEvent alloc] init];
	[fse setProperties:fseProps];
	[[SimiasEventData sharedInstance] pushEvent:fse];

	[fseProps release];
	[fse release];
    [pool release];	
    return 0;
}
NSDictionary *getFileSyncEventProperties(SimiasFileSyncEvent *fileEvent)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];

	if(fileEvent->event_type != nil)
		[newProperties setObject:[NSString stringWithUTF8String:fileEvent->event_type] forKey:@"eventType"];
	if(fileEvent->collection_id != nil)
		[newProperties setObject:[NSString stringWithUTF8String:fileEvent->collection_id] forKey:@"collectionID"];
	if(fileEvent->name != nil)
		[newProperties setObject:[NSString stringWithUTF8String:fileEvent->name] forKey:@"name"];
	if(fileEvent->object_type != nil)
		[newProperties setObject:[NSString stringWithUTF8String:fileEvent->object_type] forKey:@"objectType"];
	if(fileEvent->delete_str != nil)
		[newProperties setObject:[NSString stringWithUTF8String:fileEvent->delete_str] forKey:@"delete_str"];
	if(fileEvent->size != nil)
		[newProperties 
			setObject:[NSNumber numberWithDouble:[[NSString stringWithUTF8String:fileEvent->size] doubleValue]]
			forKey:@"size"];
	if(fileEvent->size_to_sync != nil)
		[newProperties 
			setObject:[NSNumber numberWithDouble:[[NSString stringWithUTF8String:fileEvent->size_to_sync] doubleValue]]
			forKey:@"sizeToSync"];
	if(fileEvent->size_remaining != nil)
		[newProperties 
			setObject:[NSNumber numberWithDouble:[[NSString stringWithUTF8String:fileEvent->size_remaining] doubleValue]]
			forKey:@"sizeRemaining"];
	if(fileEvent->direction != nil)
		[newProperties setObject:[NSString stringWithUTF8String:fileEvent->direction] forKey:@"direction"];
	if(fileEvent->status != nil)
		[newProperties setObject:[NSString stringWithUTF8String:fileEvent->status] forKey:@"status"];

	return [newProperties autorelease];
}




//===================================================================
// NotifyEvent Handlers
//===================================================================
int SimiasEventNotifyMessage(SimiasNotifyEvent *notifyEvent, void *data)
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];
//	NSLog(@"SimiasNotifyEvent fired: %s - %s", notifyEvent->message, notifyEvent->type);

	NSDictionary *neProps = [getNotifyEventProperties(notifyEvent) retain];
	SMNotifyEvent *ne = [[SMNotifyEvent alloc] init];
	[ne setProperties:neProps];
	[[SimiasEventData sharedInstance] pushEvent:ne];

	[neProps release];
	[ne release];
    [pool release];
    return 0;
}
NSDictionary *getNotifyEventProperties(SimiasNotifyEvent *notifyEvent)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];

	if(notifyEvent->event_type != nil)
		[newProperties setObject:[NSString stringWithUTF8String:notifyEvent->event_type] forKey:@"eventType"];
	if(notifyEvent->message != nil)
		[newProperties setObject:[NSString stringWithUTF8String:notifyEvent->message] forKey:@"message"];
	if(notifyEvent->time != nil)
		[newProperties setObject:[NSString stringWithUTF8String:notifyEvent->time] forKey:@"time"];
	if(notifyEvent->type != nil)
		[newProperties setObject:[NSString stringWithUTF8String:notifyEvent->type] forKey:@"ID"];

	return [newProperties autorelease];
}
