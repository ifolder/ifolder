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

static SimiasEventClient simiasEventClient;


void SimiasEventInitialize(void)
{
	if(sec_init (&simiasEventClient, SimiasEventStateCallBack, &simiasEventClient) != 0)
	{
		[[NSApp delegate] addLogTS:@"Error initializing the Simias Event Client"];
		return;
	}

	if(sec_register(simiasEventClient) != 0)
	{
		[[NSApp delegate] addLogTS:@"Error registering the Simias Event Client"];
		return;
	}

	[[NSApp delegate] addLogTS:@"Simias Event Client initialized and registered"];
}


void SimiasEventDisconnect(void)
{
	sec_set_event(simiasEventClient, ACTION_NODE_CREATED, false, nil, nil);
	[[NSApp delegate] addLogTS:@"De-Registered from Node Created Events"];
	sec_set_event(simiasEventClient, ACTION_NODE_DELETED, false, nil, nil);
	[[NSApp delegate] addLogTS:@"De-Registered from Node Deleted Events"];
	sec_set_event(simiasEventClient, ACTION_NODE_CHANGED, false, nil, nil);
	[[NSApp delegate] addLogTS:@"De-Registered from Node Changed Events"];

	sec_set_event(simiasEventClient, ACTION_COLLECTION_SYNC, false, nil, nil);
	[[NSApp delegate] addLogTS:@"De-Registered from Collection Sync Events"];
	sec_set_event(simiasEventClient, ACTION_FILE_SYNC, false, nil, nil);
	[[NSApp delegate] addLogTS:@"De-Registered from File Sync Events"];
	sec_set_event(simiasEventClient, ACTION_NOTIFY_MESSAGE, false, nil, nil);
	[[NSApp delegate] addLogTS:@"De-Registered from Notify Message Events"];

	if(sec_deregister(simiasEventClient) != 0)
	{
		[[NSApp delegate] addLogTS:@"Error deregistering the Simias Event Client"];
		return;
	}

	[[NSApp delegate] addLogTS:@"Simias Event Client de-registered"];
}


int SimiasEventStateCallBack(SEC_STATE_EVENT state_event, const char *message, void *data)
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];
	
	SimiasEventClient *sec = (SimiasEventClient *)data;

	switch(state_event)
	{
		case SEC_STATE_EVENT_CONNECTED:
			[[NSApp delegate] addLogTS:@"Simias Event Client Event Connected... Registering for events"];
			sec_set_event(*sec, ACTION_NODE_CREATED, true, (SimiasEventFunc)SimiasEventNodeCreated, nil);
			[[NSApp delegate] addLogTS:@"Registered for Node Created Events"];
			sec_set_event(*sec, ACTION_NODE_DELETED, true, (SimiasEventFunc)SimiasEventNodeDeleted, nil);
			[[NSApp delegate] addLogTS:@"Registered for Node Deleted Events"];
			sec_set_event(*sec, ACTION_NODE_CHANGED, true, (SimiasEventFunc)SimiasEventNodeChanged, nil);
			[[NSApp delegate] addLogTS:@"Registered for Node Changed Events"];

			sec_set_event(*sec, ACTION_COLLECTION_SYNC, true, (SimiasEventFunc)SimiasEventSyncCollection, nil);
			[[NSApp delegate] addLogTS:@"Registered for Collection Sync Events"];
			sec_set_event(*sec, ACTION_FILE_SYNC, true, (SimiasEventFunc)SimiasEventSyncFile, nil);
			[[NSApp delegate] addLogTS:@"Registered for File Sync Events"];
			sec_set_event(*sec, ACTION_NOTIFY_MESSAGE, true, (SimiasEventFunc)SimiasEventNotifyMessage, nil);
			[[NSApp delegate] addLogTS:@"Registered for Notify Message Events"];

			break;
		case SEC_STATE_EVENT_DISCONNECTED:
			[[NSApp delegate] addLogTS:@"Simias Event Client Event Disconnected"];		
			break;
		case SEC_STATE_EVENT_ERROR:
			[[NSApp delegate] addLogTS:@"Simias Event Client Event Error!"];		
			break;
	}
    [pool release];	
	return 0;
}


int SimiasEventNodeCreated(SimiasNodeEvent *nodeEvent, void *data)
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];
	[[NSApp delegate] addLogTS:[NSString stringWithFormat:@"Node created: %s", nodeEvent->node]];
    [pool release];	
    return 0;
}

int SimiasEventNodeDeleted(SimiasNodeEvent *nodeEvent, void *data)
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];
	[[NSApp delegate] addLogTS:[NSString stringWithFormat:@"Node deleted: %s", nodeEvent->node]];
    [pool release];	
    return 0;
}

int SimiasEventNodeChanged(SimiasNodeEvent *nodeEvent, void *data)
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];
	[[NSApp delegate] addLogTS:[NSString stringWithFormat:@"Node changed: %s", nodeEvent->node]];

    [pool release];	
    return 0;
}

int SimiasEventSyncCollection(SimiasCollectionSyncEvent *collectionEvent, void *data)
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];
	[[NSApp delegate] addLogTS:[NSString stringWithFormat:@"Collection sync: %s", collectionEvent->name]];

    [pool release];	
    return 0;
}

int SimiasEventSyncFile(SimiasFileSyncEvent *fileEvent, void *data)
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];
	[[NSApp delegate] addLogTS:[NSString stringWithFormat:@"File Sync: %s", fileEvent->name]];

    [pool release];	
    return 0;
}

int SimiasEventNotifyMessage(SimiasNotifyEvent *notifyEvent, void *data)
{
    NSAutoreleasePool *pool=[[NSAutoreleasePool alloc] init];
	[[NSApp delegate] addLogTS:[NSString stringWithFormat:@"Authentication requested for: %s", notifyEvent->message]];

	[[NSApp delegate] showLoginWindow:[NSString stringWithCString:notifyEvent->message]];

    [pool release];	
    return 0;
}


