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

#import <Cocoa/Cocoa.h>
#import <iFolderService.h>

#include <Carbon/Carbon.h>
#include "simias-event-client.h"


// Globals
extern SimiasEventClient simiasEventClient;


// Functions
void SimiasEventInitialize(void);
void SimiasEventDisconnect(void);
int SimiasEventStateCallBack(SEC_STATE_EVENT state_event, const char *message, void *data);
int SimiasEventNode(SimiasNodeEvent *nodeEvent, void *data);
int SimiasEventSyncCollection(SimiasCollectionSyncEvent *collectionEvent, void *data);
int SimiasEventSyncFile(SimiasFileSyncEvent *fileEvent, void *data);
int SimiasEventNotifyMessage(SimiasNotifyEvent *notifyEvent, void *data);

NSDictionary *getNotifyEventProperties(SimiasNotifyEvent *notifyEvent);
NSDictionary *getFileSyncEventProperties(SimiasFileSyncEvent *fileEvent);
NSDictionary *getCollectionSyncEventProperties(SimiasCollectionSyncEvent *collectionEvent);
NSDictionary *getNodeEventProperties(SimiasNodeEvent *nodeEvent);