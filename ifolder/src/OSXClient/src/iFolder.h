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


#define SYNC_STATE_PREPARING		1
#define SYNC_STATE_SYNCING			2
#define SYNC_STATE_OK				3
#define SYNC_STATE_OUT_OF_SYNC		4
#define SYNC_STATE_DISCONNECTED		5


/*
	@public
		NSString	*DomainID;
		NSString	*ID;
		NSString	*ManagedPath;
		NSString	*UnManagedPath;
		NSString	*Name;
		NSString	*Owner;
		NSString	*OwnerID;
		NSString	*Type;
		NSString	*Description;
		NSString	*State;
		NSString	*CurrentUserID;
		NSString	*CurrentUserRights;
		NSString	*CollectionID;
		NSString	*LastSyncTime;
		NSNumber	EffectiveSyncInterval;
		NSNumber	SyncInterval;
		NSNumber	IsSubscription;
		NSNumber	IsWorkgroup;
		NSNumber	HasConflicts;
}
*/

@class User;

@interface iFolder : NSObject
{
	NSMutableDictionary * properties;
}

-(NSMutableDictionary *) properties;
-(void) setProperties: (NSDictionary *)newProperties;

-(int) syncState;
-(void) setSyncState:(int)syncState;

-(unsigned long) outOfSyncCount;
-(void) setOutOfSyncCount:(unsigned long)outOfSyncCount;

-(BOOL)IsSubscription;
-(BOOL)HasConflicts;
-(NSString *)Name;
-(NSString *)ID;
-(NSString *)CollectionID;
-(NSString *)Path;
-(NSString *)DomainID;
-(NSString *)OwnerUserID;
-(NSString *)OwnerName;
-(NSString *)CurrentUserID;
-(NSString *)CurrentUserRights;
-(NSString *)State;
-(NSString *)LastSync;
-(NSString *)Role;
-(long)SyncInterval;
-(long)EffectiveSyncInterval;
-(void)SetOwner:(User *)user;

-(void) updateDisplayInformation;


@end
