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

#define FILE_SYNC_UPLOADING		1
#define FILE_SYNC_DOWNLOADING	2

#define FILE_SYNC_FILE			1
#define FILE_SYNC_DIRECTORY		2
#define FILE_SYNC_UNKNOWN		3

#define SYNC_ACTION_START		1
#define SYNC_ACTION_STOP		2

#define NODE_CREATED			1
#define NODE_DELETED			2
#define NODE_CHANGED			3


@interface SMEvent : NSObject
{
	NSMutableDictionary * properties;
}

-(NSMutableDictionary *) properties;
-(void) setProperties: (NSDictionary *)newProperties;
-(NSString *)eventType;

@end


@interface SMNotifyEvent : SMEvent
{
	NSString *notifyEventMessage;
/*
typedef struct 
{
	char *event_type;
	char *message;
	char *time;
	char *type;
} SimiasNotifyEvent;
*/
}
-(NSString *)message;
-(NSString *)time;
-(NSString *)type;
@end


@interface SMFileSyncEvent : SMEvent
{
/*
typedef struct 
{
	char *event_type;
	char *collection_id;
	char *object_type;
	char *delete_str;
	char *name;
	char *size;
	char *size_to_sync;
	char *size_remaining;
	char *direction;
} SimiasFileSyncEvent;
*/
}
-(NSString *)collectionID;
-(BOOL) isDelete;
-(NSString *)name;
-(int)objectType;
-(double)size;
-(double)sizeToSync;
-(double)sizeRemaining;
-(int)direction;
@end


@interface SMCollectionSyncEvent : SMEvent
{
/*
typedef struct 
{
	char *event_type;
	char *name;
	char *id;
	char *action;
	char *successful;
} SimiasCollectionSyncEvent;
*/
}
-(NSString *)name;
-(NSString *)ID;
-(BOOL)isDone;
-(int)syncAction;
@end



@interface SMNodeEvent : SMEvent
{
/*
typedef struct
{
	char *event_type;
	char *action;
	char *time;
	char *source;
	char *collection;
	char *type;
	char *event_id;
	char *node;
	char *flags;
	char *master_rev;
	char *slave_rev;
	char *file_size;
} SimiasNodeEvent;
*/
}
-(NSString *)action;
-(NSString *)time;
-(NSString *)source;
-(NSString *)collectionID;
-(int)type;
-(NSString *)event_id;
-(NSString *)nodeID;
-(NSString *)flags;
-(NSString *)master_rev;
-(NSString *)slave_rev;
-(NSString *)file_size;
@end

