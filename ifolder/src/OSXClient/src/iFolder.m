/**********************************************************************
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

#import "iFolder.h"
#import "User.h"


@implementation iFolder

- (id) init
{
	if(self = [super init])
	{
		NSArray *keys = [NSArray arrayWithObjects:
			@"Name", nil];
			
		NSArray *values		= [NSArray arrayWithObjects:
			@"New iFolder", nil];
		
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
		NSNumber *totalSyncCount = [properties objectForKey:@"totalSyncCount"];
		NSNumber *outOfSyncCount = [properties objectForKey:@"outOfSyncCount"];
		NSNumber *syncState = [properties objectForKey:@"syncState"];
	
		[properties autorelease];
		properties = [[NSMutableDictionary alloc] initWithDictionary:newProperties];

		// Preserve the state of the sync even if we refresh for some reason
		if(totalSyncCount != nil)
			[properties setObject:totalSyncCount forKey:@"totalSyncCount"];
		if(outOfSyncCount != nil)
			[properties setObject:outOfSyncCount forKey:@"outOfSyncCount"];
		if(syncState != nil)
			[properties setObject:syncState forKey:@"syncState"];

		[self updateDisplayInformation];
	}
}



-(void) setSyncProperties: (NSDictionary *)syncProperties
{
	if(properties != syncProperties)
	{
		NSNumber *totalSyncCount = [syncProperties objectForKey:@"totalSyncCount"];
		NSNumber *outOfSyncCount = [syncProperties objectForKey:@"outOfSyncCount"];
		NSNumber *syncState = [syncProperties objectForKey:@"syncState"];
	
		if(totalSyncCount != nil)
			[properties setObject:totalSyncCount forKey:@"totalSyncCount"];
		if(outOfSyncCount != nil)
			[properties setObject:outOfSyncCount forKey:@"outOfSyncCount"];
		if(syncState != nil)
			[properties setObject:syncState forKey:@"syncState"];

		[self updateDisplayInformation];
	}
}




-(int) syncState
{
	NSNumber *num = [properties objectForKey:@"syncState"];
	if(num != nil)
		return [num intValue];
	else
		return 0;
}
-(void) setSyncState:(int)syncState
{
	[properties setObject:[NSNumber numberWithInt:syncState] forKey:@"syncState"];
	[self updateDisplayInformation];
}

-(unsigned long) outOfSyncCount
{
	NSNumber *num = [properties objectForKey:@"outOfSyncCount"];
	if(num != nil)
		return [num unsignedLongValue];
	else
		return 0;
}

-(void) setOutOfSyncCount:(unsigned long)outOfSyncCount
{
	[properties setObject:[NSNumber numberWithUnsignedLong:outOfSyncCount] forKey:@"outOfSyncCount"];
}


-(NSString *)Name
{
	return [properties objectForKey:@"Name"];
}

-(NSString *)ID
{
	return [properties objectForKey:@"ID"];
}

-(NSString *)CollectionID
{
	return [properties objectForKey:@"CollectionID"];
}

-(NSString *)Path
{
	return [properties objectForKey:@"Path"];
}

-(BOOL)IsSubscription
{
	NSNumber *num = [properties objectForKey:@"IsSubscription"];
	if(num != nil)
		return [num boolValue];
	else
		return NO;
}

-(BOOL)HasConflicts
{
	NSNumber *num = [properties objectForKey:@"HasConflicts"];
	if(num != nil)
		return [num boolValue];
	else
		return NO;
}


-(NSString *)DomainID
{
	return [properties objectForKey:@"DomainID"];
}

-(NSString *)OwnerUserID
{
	return [properties objectForKey:@"OwnerID"];
}

-(NSString *)OwnerName
{
	return [properties objectForKey:@"Owner"];
}

-(NSString *)CurrentUserID
{
	return [properties objectForKey:@"CurrentUserID"];
}

-(NSString *)CurrentUserRights
{
	return [properties objectForKey:@"CurrentUserRights"];
}

-(NSString *)State
{
	return [properties objectForKey:@"State"];
}

-(NSString *)LastSync
{
	return [properties objectForKey:@"LastSyncTime"];
}

-(NSString *)Role
{
	return [properties objectForKey:@"Role"];
}

-(long)SyncInterval
{
	return [ [properties objectForKey:@"SyncInterval"] longValue];
}

-(long)EffectiveSyncInterval
{
	return [ [properties objectForKey:@"EffectiveSyncInterval"] longValue];
}



-(void) updateDisplayInformation
{
	if([self IsSubscription])
	{
		if([ [properties objectForKey:@"State"] isEqualToString:@"Available"])
			[properties setObject:NSLocalizedString(@"Not set up", @"iFolder Status Message") forKey:@"Status"];
		else if([ [properties objectForKey:@"State"] isEqualToString:@"WaitConnect"])
			[properties setObject:NSLocalizedString(@"Waiting to connect", @"iFolder Status Message") forKey:@"Status"];
		else if([ [properties objectForKey:@"State"] isEqualToString:@"WaitSync"])
			[properties setObject:NSLocalizedString(@"Waiting to synchronize", @"iFolder Status Message") forKey:@"Status"];
		else
			[properties setObject:NSLocalizedString(@"Unknown", @"iFolder Status Message") forKey:@"Status"];

		if([ [properties objectForKey:@"State"] isEqualToString:@"Available"])
		{
			// set the location to the owner's name
			[properties setObject:[properties objectForKey:@"Owner"]
								forKey:@"Location"];
		}
		
		NSImage *image = [NSImage imageNamed:@"serverifolder24"];
		[image setScalesWhenResized:YES];
		[properties setObject:image forKey:@"Image"];
	}
	else
	{
		if([self syncState] == SYNC_STATE_SYNCING)
			[properties setObject:NSLocalizedString(@"Synchronizing", @"iFolder Status Message") forKey:@"Status"];
		else if([self syncState] == SYNC_STATE_PREPARING)
			[properties setObject:NSLocalizedString(@"Preparing to synchronize", @"iFolder Status Message") forKey:@"Status"];
		else if([ [properties objectForKey:@"HasConflicts"] boolValue])
			[properties setObject:NSLocalizedString(@"Has conflicts", @"iFolder Status Message") forKey:@"Status"];
		else if([self syncState] == SYNC_STATE_DISCONNECTED)
			[properties setObject:NSLocalizedString(@"Server unavailable", @"iFolder Status Message") forKey:@"Status"];
		else if( ([self syncState] == SYNC_STATE_OUT_OF_SYNC) &&
				 ([self outOfSyncCount] > 0) )
		{
			[properties setObject:[NSString stringWithFormat:NSLocalizedString(@"%u items not synchronized", @"iFolder Status Message"), [self outOfSyncCount]] forKey:@"Status"];
		}
		else if( ([self syncState] == 0) ||
				 ([self syncState] == SYNC_STATE_OK) )
		{
			if([ [properties objectForKey:@"State"] isEqualToString:@"WaitSync"])
				[properties setObject:NSLocalizedString(@"Waiting to synchronize", @"iFolder Status Message") forKey:@"Status"];
			else
				[properties setObject:NSLocalizedString(@"OK", @"iFolder Status Message") forKey:@"Status"];
		}

		// update the location
		NSString *location = [properties objectForKey:@"Path"];
		if(location != nil)
			[properties setObject:location forKey:@"Location"];
		else
			[properties setObject:[properties objectForKey:@"Owner"]
							forKey:@"Location"];
			
		NSImage *image = [NSImage imageNamed:@"ifolder24"];
		[image setScalesWhenResized:YES];
		[properties setObject:image forKey:@"Image"];
	}
}

-(void)SetOwner:(User *)user
{
	[properties setObject:[user Name] forKey:@"Owner"];
	[properties setObject:[user UserID] forKey:@"OwnerID"];
	[self updateDisplayInformation];	
}


@end
