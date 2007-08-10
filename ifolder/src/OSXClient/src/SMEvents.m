/***********************************************************************
 |  $RCSfile$
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 |  Author: Calvin Gaisford <cgaisford@novell.com>
 | 
 ***********************************************************************/

#import "SMEvents.h"

@implementation SMEvent

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

-(void) dealloc
{
	[properties release];
	[super dealloc];
}

-(NSMutableDictionary *) properties
{
	return properties;
}

-(void) setProperties: (NSDictionary *)newProperties
{
	if(properties != newProperties)
	{
		[properties autorelease];
		properties = [[NSMutableDictionary alloc] initWithDictionary:newProperties];
	}
}

-(NSString *)eventType
{
	return [properties objectForKey:@"eventType"];
}

@end


@implementation SMNotifyEvent
-(NSString *)message
{
	return [properties objectForKey:@"message"];
}
-(NSString *)time
{
	return [properties objectForKey:@"time"];
}
-(NSString *)type
{
	return [properties objectForKey:@"type"];
}
@end


@implementation SMFileSyncEvent
-(NSString *)collectionID
{
	return [properties objectForKey:@"collectionID"];	
}
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
-(BOOL) isDelete
{
	NSString *delStr = [properties objectForKey:@"delete_str"];
	return ([delStr compare:@"False"] != 0);
}
-(NSString *)name
{
	return [properties objectForKey:@"name"];	
}
-(double)size
{
	return [[properties objectForKey:@"size"] doubleValue];	
}
-(double)sizeToSync
{
	return [[properties objectForKey:@"sizeToSync"] doubleValue];	
}
-(double)sizeRemaining
{
	return [[properties objectForKey:@"sizeRemaining"] doubleValue];	
}
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
-(NSString *)status
{
	return [properties objectForKey:@"status"];
}
@end



@implementation SMCollectionSyncEvent
-(NSString *)name
{
	return [properties objectForKey:@"name"];
}
-(NSString *)ID
{
	return [properties objectForKey:@"ID"];
}
-(BOOL)connected
{
	NSString *doneStr = [properties objectForKey:@"connected"];
	return ([doneStr compare:@"True"] == 0);
}
-(int)syncAction
{
	NSString *actStr = [properties objectForKey:@"action"];
	if([actStr compare:@"StartSync"] == 0)
		return SYNC_ACTION_START;
	else if([actStr compare:@"StartLocalSync"] == 0)
		return SYNC_ACTION_LOCAL;
	else
		return SYNC_ACTION_STOP;
}
@end



@implementation SMNodeEvent
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
-(NSString *)time
{
	return [properties objectForKey:@"time"];
}
-(NSString *)source
{
	return [properties objectForKey:@"source"];
}
-(NSString *)collectionID
{
	return [properties objectForKey:@"collectionID"];
}
-(NSString *)type
{
	return [properties objectForKey:@"type"];
}
-(NSString *)event_id
{
	return [properties objectForKey:@"event_id"];
}
-(NSString *)nodeID
{
	return [properties objectForKey:@"nodeID"];
}
-(NSString *)flags
{
	return [properties objectForKey:@"flags"];
}
-(NSString *)master_rev
{
	return [properties objectForKey:@"master_rev"];
}
-(NSString *)slave_rev
{
	return [properties objectForKey:@"slave_rev"];
}
-(NSString *)file_size
{
	return [properties objectForKey:@"file_size"];
}
@end

