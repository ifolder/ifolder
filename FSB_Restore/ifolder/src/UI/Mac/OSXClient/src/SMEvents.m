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

#import "SMEvents.h"

@implementation SMEvent


//====================================================================
// init
// Initialize the simias event
//====================================================================
- (id) init
{
	if(self = [super init])
	{
		NSArray *keys = [NSArray arrayWithObjects:nil];
			
		NSArray *values	= [NSArray arrayWithObjects:nil];
		
		properties = [[NSMutableDictionary alloc]
			initWithObjects:values forKeys: keys];
	}
	return self;
}

//====================================================================
// dealloc
// Deallocate the resources here
//====================================================================
-(void) dealloc
{
	[properties release];
	[super dealloc];
}

//====================================================================
// properties
// Get the properties of the simias event
//====================================================================
-(NSMutableDictionary *) properties
{
	return properties;
}

//====================================================================
// setProperties
// Set the properties for the simias event
//====================================================================
-(void) setProperties: (NSDictionary *)newProperties
{
	if(properties != newProperties)
	{
		[properties autorelease];
		properties = [[NSMutableDictionary alloc] initWithDictionary:newProperties];
	}
}

//====================================================================
// eventType
// Get the type of event
//====================================================================
-(NSString *)eventType
{
	return [properties objectForKey:@"eventType"];
}

@end


@implementation SMNotifyEvent
//====================================================================
// message
// Message associated with Notify event
//====================================================================
-(NSString *)message
{
	return [properties objectForKey:@"message"];
}

//====================================================================
// time
// Get the time from the list of properties of simias event 
//====================================================================
-(NSString *)time
{
	return [properties objectForKey:@"time"];
}

//====================================================================
// type
// Type of the event 
//====================================================================
-(NSString *)type
{
	return [properties objectForKey:@"type"];
}
@end


@implementation SMFileSyncEvent
//====================================================================
// collectionID
// Events collection ID
//====================================================================
-(NSString *)collectionID
{
	return [properties objectForKey:@"collectionID"];	
}

//====================================================================
// objectType
// Find whether it is directory or file or unknown type
//====================================================================
-(int)objectType
{
	NSString *objStr = [properties objectForKey:@"objectType"];
	if([objStr compare:@"Directory"] == 0)
		return FILE_SYNC_DIRECTORY;
	else if([objStr compare:@"File"] == 0)
		return FILE_SYNC_FILE;
	else
		return FILE_SYNC_UNKNOWN;
}

//====================================================================
// isDelete
// Find whether it is delete event or not
//====================================================================
-(BOOL) isDelete
{
	NSString *delStr = [properties objectForKey:@"delete_str"];
	return ([delStr compare:@"False"] != 0);
}

//====================================================================
// name
// Name of the event
//====================================================================
-(NSString *)name
{
	return [properties objectForKey:@"name"];	
}

//====================================================================
// size
// Size of the ifolder
//====================================================================
-(double)size
{
	return [[properties objectForKey:@"size"] doubleValue];	
}

//====================================================================
// sizeToSync
// Size of the iFolder to sync
//====================================================================
-(double)sizeToSync
{
	return [[properties objectForKey:@"sizeToSync"] doubleValue];	
}

//====================================================================
// sizeRemaining
// Remaining size of the iFolder to sync
//====================================================================
-(double)sizeRemaining
{
	return [[properties objectForKey:@"sizeRemaining"] doubleValue];	
}

//====================================================================
// direction
// Direction of iFolder sync
//====================================================================
-(int)direction
{
	NSString *dirStr = [properties objectForKey:@"direction"];

	if([dirStr compare:@"Downloading"] == 0)
		return FILE_SYNC_DOWNLOADING;
	else if([dirStr compare:@"Uploading"] == 0)
		return FILE_SYNC_UPLOADING;
	else
		return FILE_SYNC_LOCAL;
}

//====================================================================
// status
// Current status of sync
//====================================================================
-(NSString *)status
{
	return [properties objectForKey:@"status"];
}
@end



@implementation SMCollectionSyncEvent
//====================================================================
// name
// Name of the colleciton sync event
//====================================================================
-(NSString *)name
{
	return [properties objectForKey:@"name"];
}

//====================================================================
// ID
// ID of the collection sync event
//====================================================================
-(NSString *)ID
{
	return [properties objectForKey:@"ID"];
}

//====================================================================
// connected
// Whether connected or not
//====================================================================
-(BOOL)connected
{
	NSString *doneStr = [properties objectForKey:@"connected"];
	return ([doneStr compare:@"True"] == 0);
}

//====================================================================
// syncAction
// Status of sync whether started, local sync or stopped
//====================================================================
-(int)syncAction
{
	NSString *actStr = [properties objectForKey:@"action"];
	if([actStr compare:@"StartSync"] == 0)
		return SYNC_ACTION_START;
	else if([actStr compare:@"StartLocalSync"] == 0)
		return SYNC_ACTION_LOCAL;
	else if([actStr compare:@"NoPassphrase"] == 0)
		return SYNC_ACTION_NOPASSPHRASE;
	else if([actStr compare:@"DisabledSync"] == 0)
		return SYNC_ACTION_DISABLEDSYNC;
		else
		return SYNC_ACTION_STOP;
}
@end



@implementation SMNodeEvent
//====================================================================
// action
// Whether node created or deleted or changed
//====================================================================
-(int)action
{
	NSString *actStr = [properties objectForKey:@"action"];
	if([actStr compare:@"NodeCreated"] == 0)
		return NODE_CREATED;
	else if([actStr compare:@"NodeDeleted"] == 0)
		return NODE_DELETED;
	else
		return NODE_CHANGED;
}

//====================================================================
// time
// Time at which event took place
//====================================================================
-(NSString *)time
{
	return [properties objectForKey:@"time"];
}

//====================================================================
// source
// Source of the event
//====================================================================
-(NSString *)source
{
	return [properties objectForKey:@"source"];
}

//====================================================================
// collectionID
// CollectionID of the event
//====================================================================
-(NSString *)collectionID
{
	return [properties objectForKey:@"collectionID"];
}

//====================================================================
// type
// Type of the event
//====================================================================
-(NSString *)type
{
	return [properties objectForKey:@"type"];
}

//====================================================================
// event_id
// ID of the event
//====================================================================
-(NSString *)event_id
{
	return [properties objectForKey:@"event_id"];
}

//====================================================================
// nodeID
// Node id of the event
//====================================================================
-(NSString *)nodeID
{
	return [properties objectForKey:@"nodeID"];
}

//====================================================================
// flags
// Flags set for the event
//====================================================================
-(NSString *)flags
{
	return [properties objectForKey:@"flags"];
}

//====================================================================
// master_rev
// Is it master revision
//====================================================================
-(NSString *)master_rev
{
	return [properties objectForKey:@"master_rev"];
}

//====================================================================
// slave_rev
// Is it slave revision
//====================================================================
-(NSString *)slave_rev
{
	return [properties objectForKey:@"slave_rev"];
}

//====================================================================
// file_size
// Size of the file
//====================================================================
-(NSString *)file_size
{
	return [properties objectForKey:@"file_size"];
}
@end

