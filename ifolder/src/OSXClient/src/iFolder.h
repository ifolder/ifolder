//
//  iFolder.h
//  iFolder
//
//  Created by Calvin Gaisford on 12/17/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#include "iFolderStub.h"

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

@interface iFolder : NSObject
{
	NSMutableDictionary * properties;
	NSImage				* icon;
	NSString			* location;
	NSString			* state;
}

-(NSMutableDictionary *) properties;
-(void) setProperties: (NSDictionary *)newProperties;

-(void) setgSOAPProperties:(struct ns1__iFolderWeb *)ifolder;

-(NSImage *)Image;
-(NSString *)Location;
-(NSString *)Status;
-(NSNumber *)IsSubscription;

-(void) updateDisplayInformation;


@end
