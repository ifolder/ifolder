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

#ifndef __iFolderService__
#define __iFolderService__

#import <Cocoa/Cocoa.h>
#include <Carbon/Carbon.h>
#import "iFolder.h"
#import "User.h"
#import "DiskSpace.h"
#import "SyncSize.h"


@interface iFolderService : NSObject
{
}

-(BOOL) Ping;
-(NSArray *) GetiFolders;
-(iFolder *) GetiFolder:(NSString *)iFolderID;
-(iFolder *) GetAvailableiFolder:(NSString *)iFolderID
					inCollection:(NSString *)collectionID;
-(iFolder *) CreateiFolder:(NSString *)Path InDomain:(NSString *)DomainID;
-(iFolder *) AcceptiFolderInvitation:(NSString *)iFolderID
					InDomain:(NSString *)DomainID toPath:(NSString *)localPath;

-(void) DeclineiFolderInvitation:(NSString *)iFolderID fromDomain:(NSString *)DomainID;
-(iFolder *) RevertiFolder:(NSString *)iFolderID;

// You should call RevertiFolder and DeclineiFolderInvitation instead of delete
//-(void) DeleteiFolder:(NSString *)iFolderID;

-(void) SynciFolderNow:(NSString *)iFolderID;

-(User *) GetiFolderUserFromNodeID:(NSString *)nodeID inCollection:(NSString *)collectionID;
-(User *) GetiFolderUser:(NSString *)userID;
-(NSArray *) GetiFolderUsers:(NSString *)ifolderID;
-(NSArray *) GetDomainUsers:(NSString *)domainID withLimit:(int)numUsers;
-(NSArray *) SearchDomainUsers:(NSString *)domainID withString:(NSString *)searchString;

-(User *) InviteUser:(NSString *)userID toiFolder:(NSString *)ifolderID withRights:(NSString *)rights;
-(void) RemoveUser:(NSString *)userID fromiFolder:(NSString *)ifolderID;

-(DiskSpace *)GetiFolderDiskSpace:(NSString *)ifolderID;
-(DiskSpace *)GetUserDiskSpace:(NSString *)userID;
-(void)SetiFolderDiskSpace:(long long)limit oniFolder:(NSString *)ifolderID;
-(SyncSize *)CalculateSyncSize:(NSString *)ifolderID;


-(int)GetDefaultSyncInterval;
-(void)SetDefaultSyncInterval:(int)syncInterval;



@end

#endif // __iFolderService__