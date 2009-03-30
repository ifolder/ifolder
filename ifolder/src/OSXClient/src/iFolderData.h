/***********************************************************************
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

#import <Cocoa/Cocoa.h>

@class iFolder;
@class iFolderDomain;
@class User;
@class iFolderService;
@class SimiasService;
@class SyncSize;
@class MemberSearchResults;

@interface iFolderData : NSObject 
{
	iFolderService					*ifolderService;
	SimiasService					*simiasService;

	NSMutableDictionary				*ifolderUserChanges;

	NSMutableDictionary				*keyedDomains;
	NSMutableDictionary				*keyediFolders;		
	NSMutableDictionary				*keyedSubscriptions;

	NSMutableArray					*ifolderdomains;
	NSMutableArray					*ifolders;
	NSArrayController				*domainsController;
	NSArrayController				*ifoldersController;
	
//	NSObjectController				*ifolderDataAlias;

	NSRecursiveLock					*instanceLock;
	iFolderDomain					*defaultDomain;
}

+ (iFolderData *)sharedInstance;

-(NSArrayController *)domainArrayController;
-(NSArrayController *)ifolderArrayController;
//-(NSObjectController *)dataAlias;

- (void)refresh:(BOOL)onlyDomains;

-(void)_addiFolder:(iFolder *)ifolder;
-(void)_deliFolder:(NSString *)ifolderID;
-(NSString *)getiFolderID:(NSString *)subscriptionID;
-(void)_addDomain:(iFolderDomain *)domain;

-(BOOL)isDomain:(NSString *)domainID;
-(BOOL)isiFolder:(NSString *)ifolderID;
-(BOOL)isPOBox:(NSString *)nodeID;

-(iFolder *)getiFolder:(NSString *)ifolderID;
-(iFolder *)readiFolder:(NSString *)ifolderID;

-(iFolder *)getAvailableiFolder:(NSString *)ifolderID;
-(iFolder *)readAvailableiFolder:(NSString *)ifolderID 
									inCollection:(NSString *)collectionID;

-(iFolderDomain *)getDomain:(NSString *)domainID;
-(iFolderDomain *)getPOBoxDomain:(NSString *)poBoxID;

-(NSArray *)getDomains;
-(NSArray *)getiFolders;
-(iFolderDomain *)getDefaultDomain;
-(int)getDomainCount;
-(void)selectDefaultDomain;

-(void)createiFolder:(NSString *)path inDomain:(NSString *)domainID;
-(void)acceptiFolderInvitation:(NSString *)iFolderID 
									InDomain:(NSString *)domainID 
									toPath:(NSString *)localPath;
-(void)declineiFolderInvitation:(NSString *)iFolderID 
									fromDomain:(NSString *)domainID;
-(void)revertiFolder:(NSString *)iFolderID;
-(void)deleteiFolder:(NSString *)ifolderID;
-(void)synciFolderNow:(NSString *)ifolderID;


-(void)setUsersAdded:(NSString *)ifolderID;
-(BOOL)usersAdded:(NSString *)ifolderID;
-(void)clearUsersAdded:(NSString *)ifolderID;

-(SyncSize *)getSyncSize:(NSString *)ifolderID;

-(MemberSearchResults *) InitMemberSearchResults;
-(void)readCredentials;

 @end
