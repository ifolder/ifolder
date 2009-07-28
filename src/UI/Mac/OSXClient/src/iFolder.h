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


#define SYNC_STATE_PREPARING		1
#define SYNC_STATE_SYNCING			2
#define SYNC_STATE_OK				3
#define SYNC_STATE_OUT_OF_SYNC		4
#define SYNC_STATE_DISCONNECTED		5
#define SYNC_STATE_NOPASSPHRASE     6
#define SYNC_STATE_DISABLEDSYNC     7


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

-(void) setSyncProperties: (NSDictionary *)syncProperties;

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
-(BOOL)SSL;
-(NSString *)EncryptionAlgorithm; 
-(long)SyncInterval;
-(long)EffectiveSyncInterval;
-(BOOL) Shared;
-(void)SetOwner:(User *)user;

-(void) updateDisplayInformation;


@end
