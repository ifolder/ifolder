/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Calvin Gaisford <cgaisford@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import <Cocoa/Cocoa.h>
#import <iFolderService.h>

//#include <Carbon/Carbon.h>
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
