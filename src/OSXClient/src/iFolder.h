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
-(NSString *)Name;
-(NSString *)ID;
-(NSString *)Path;

-(void) updateDisplayInformation;


@end
