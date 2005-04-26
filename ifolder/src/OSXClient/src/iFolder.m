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
		[properties autorelease];
		properties = [[NSMutableDictionary alloc] initWithDictionary:newProperties];
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
	[self updateDisplayInformation];
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
			[properties setObject:NSLocalizedString(@"Available", nil) forKey:@"Status"];
		else if([ [properties objectForKey:@"State"] isEqualToString:@"WaitConnect"])
			[properties setObject:NSLocalizedString(@"Waiting to Connect", nil) forKey:@"Status"];
		else if([ [properties objectForKey:@"State"] isEqualToString:@"WaitSync"])
			[properties setObject:NSLocalizedString(@"Waiting to Sync", nil) forKey:@"Status"];
		else
			[properties setObject:NSLocalizedString(@"Unknown", nil) forKey:@"Status"];

		if([ [properties objectForKey:@"State"] isEqualToString:@"Available"])
		{
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
			[properties setObject:NSLocalizedString(@"Synchronizing", nil) forKey:@"Status"];
		else if([self syncState] == SYNC_STATE_PREPARING)
			[properties setObject:NSLocalizedString(@"Preparing to synchronize", nil) forKey:@"Status"];
		else if([ [properties objectForKey:@"HasConflicts"] boolValue])
			[properties setObject:NSLocalizedString(@"Has Conflicts", nil) forKey:@"Status"];
		else if([self syncState] == SYNC_STATE_DISCONNECTED)
			[properties setObject:NSLocalizedString(@"Disconnected", nil) forKey:@"Status"];
		else if( ([self syncState] == SYNC_STATE_OUT_OF_SYNC) &&
				 ([self outOfSyncCount] > 0) )
		{
			[properties setObject:[NSString stringWithFormat:NSLocalizedString(@"%ul items out of sync", nil), [self outOfSyncCount]] forKey:@"Status"];
		}
		else if( ([self syncState] == 0) ||
				 ([self syncState] == SYNC_STATE_OK) )
		{
			if([ [properties objectForKey:@"State"] isEqualToString:@"WaitSync"])
				[properties setObject:NSLocalizedString(@"Waiting to Sync", nil) forKey:@"Status"];
			else
				[properties setObject:NSLocalizedString(@"OK", nil) forKey:@"Status"];
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
