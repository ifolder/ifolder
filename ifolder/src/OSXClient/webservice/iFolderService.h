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
//#include <Carbon/Carbon.h>
#import "iFolder.h"
#import "User.h"
#import "DiskSpace.h"
#import "SyncSize.h"
#import "iFolderConflict.h"


@interface iFolderService : NSObject
{
	NSString	*simiasURL;
	void		*soapData;
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
-(void) DeleteiFolder:(NSString *)iFolderID;

// You should call RevertiFolder and DeclineiFolderInvitation instead of delete
//-(void) DeleteiFolder:(NSString *)iFolderID;

-(void) SynciFolderNow:(NSString *)iFolderID;

-(User *) GetiFolderUserFromNodeID:(NSString *)nodeID inCollection:(NSString *)collectionID;
-(User *) GetiFolderUser:(NSString *)userID;
-(NSArray *) GetiFolderUsers:(NSString *)ifolderID;
-(NSArray *) GetDomainUsers:(NSString *)domainID withLimit:(int)numUsers;
-(NSArray *) SearchDomainUsers:(NSString *)domainID withString:(NSString *)searchString;

-(NSArray *) GetiFolderConflicts:(NSString *)ifolderID;
-(void) ResolveFileConflict:(NSString *)ifolderID withID:(NSString *)conflictID localChanges:(bool)saveLocal;
-(void) ResolveNameConflict:(NSString *)ifolderID withID:(NSString *)conflictID usingName:(NSString *)newName;
-(void) RenameAndResolveConflict:(NSString *)ifolderID withID:(NSString *)conflictID usingFileName:(NSString *)newFileName;

-(User *) AddAndInviteUser:(NSString *)memberID 
					MemberName:(NSString *)memberName
					GivenName:(NSString *)givenName
					FamilyName:(NSString *)familyName
					iFolderID:(NSString *)ifolderID
					PublicKey:(NSString *)publicKey
					Rights:(NSString *)rights;
-(User *) InviteUser:(NSString *)userID toiFolder:(NSString *)ifolderID withRights:(NSString *)rights;
-(void) RemoveUser:(NSString *)userID fromiFolder:(NSString *)ifolderID;

-(DiskSpace *)GetiFolderDiskSpace:(NSString *)ifolderID;
-(DiskSpace *)GetUserDiskSpace:(NSString *)userID;
-(void)SetiFolderDiskSpace:(long long)limit oniFolder:(NSString *)ifolderID;
-(SyncSize *)CalculateSyncSize:(NSString *)ifolderID;

-(int)GetDefaultSyncInterval;
-(void)SetDefaultSyncInterval:(int)syncInterval;

-(void)SetUserRights:(NSString *)ifolderID forUser:(NSString *)userID withRights:(NSString *)rights;
-(void)ChanageOwner:(NSString *)ifolderID toUser:(NSString *)userID oldOwnerRights:(NSString *)rights;


@end

#endif // __iFolderService__