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
*                 $Modified by: $Modified by: Satya <ssutapalli@novell.com> 22/02/2008   Added Simias calls related to Encryption
*                 $Modified by: Satya <ssutapalli@novell.com> 29/02/2008   Added Merge functionality of simias
*                 $Modified by: Satya <ssutapalli@novell.com> 10/03/2008   Added IsiFolder simias functionality
*                 $Modified by: Satya <ssutapalli@novell.com> 24/03/2008   Added GetLimitPolicy simias functionality
*                 $Modified by: Satya <ssutapalli@novell.com> 22/05/2008   Added SetiFolderSecureSync simias functionality
*                 $Modified by: Satyam <ssutapalli@novell.com>  02/06/2009     Added new functions required for Forgot PP dialog
*-----------------------------------------------------------------------------
* This module is used to:
*       	Connect to local simias via gSoap 
*
*******************************************************************************/

#ifndef __iFolderService__
#define __iFolderService__

#import <Cocoa/Cocoa.h>
//#include <Carbon/Carbon.h>
#import "iFolder.h"
#import "User.h"
#import "DiskSpace.h"
#import "SyncSize.h"
#import "iFolderConflict.h"
#import "clientUpdate.h"


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
-(iFolder *) CreateEncryptediFolder:(NSString *) Path InDomain:(NSString *)DomainID 
					withSSL:(BOOL)SslFlag usingAlgorithm:(NSString *)EncrAlgthm usingPassPhrase:(NSString *)PassPhrase;
					
-(iFolder *) AcceptiFolderInvitation:(NSString *)iFolderID
					InDomain:(NSString *)DomainID toPath:(NSString *)localPath;

-(void) DeclineiFolderInvitation:(NSString *)iFolderID fromDomain:(NSString *)DomainID;
-(iFolder *) RevertiFolder:(NSString *)iFolderID;
-(void) DeleteiFolder:(NSString *)iFolderID fromDomain:(NSString *) DomainID;
-(iFolder *) MergeiFolder:(NSString *)iFolderID
				 InDomain:(NSString *)DomainID toPath:(NSString *)localPath;

// You should call RevertiFolder and DeclineiFolderInvitation instead of delete
//-(void) DeleteiFolder:(NSString *)iFolderID;

-(void) SynciFolderNow:(NSString *)iFolderID;

-(User *) GetiFolderUserFromNodeID:(NSString *)nodeID inCollection:(NSString *)collectionID;
-(User *) GetiFolderUser:(NSString *)userID;
-(NSArray *) GetiFolderUsers:(NSString *)ifolderID;
-(NSArray *) GetDomainUsers:(NSString *)domainID withLimit:(int)numUsers;
-(NSArray *) SearchDomainUsers:(NSString *)domainID withString:(NSString *)searchString;

-(NSArray *) GetiFolderConflicts:(NSString *)ifolderID;
-(void) ResolveFileConflict:(NSString *)ifolderID withID:(NSString *)conflictID localChanges:(BOOL)saveLocal;
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
-(void)readCredentials;

-(int) GetSecurityPolicy:(NSString*)domainID;
-(NSString*)GetRAName:(NSString*)domainID;
-(BOOL)IsiFolder:(NSString*)localPath;
//Enhanced Conflicts
-(void)ResolveEnhancedFileConflict:(NSString*)ifolderID havingConflictID:(NSString*)conflictID hasLocalChange:(BOOL)localOnly withConflictBinPath:(NSString*)conflictBinPath;
-(int)GetLimitPolicyStatus:(NSString*)domainID;
-(void)SetiFolderSecureSync:(NSString*)ifolderID withSSL:(BOOL)ssl;
-(BOOL) CanOwnerBeChanged:(NSString*)newUserID forDomain:(NSString*) domainID;
-(clientUpdate*) CheckForMacUpdate:(NSString*)domainID forCurrentVersion:(NSString*)curVer;
-(BOOL) RunClientUpdate:(NSString*)domainID withDownloadPath:(NSString*)path;
-(NSNumber*) ChangePassword:(NSString*)domainID changePassword:(NSString*)oldPasswd withNewPassword:(NSString*)newPasswd;
-(NSString*)GetDefaultServerPublicKey:(NSString*)domainID forUser:(NSString*)userID;
@end

#endif // __iFolderService__
