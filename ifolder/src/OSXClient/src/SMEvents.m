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
	else
		return FILE_SYNC_UPLOADING;
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
-(BOOL)isDone
{
	NSString *doneStr = [properties objectForKey:@"successful"];
	return ([doneStr compare:@"True"] == 0);
}
-(int)syncAction
{
	NSString *actStr = [properties objectForKey:@"action"];
	if([actStr compare:@"StartSync"] == 0)
		return SYNC_ACTION_START;
	else
		return SYNC_ACTION_STOP;
}
@end
