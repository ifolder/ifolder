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

@class iFolder;
@class iFolderDomain;
@class User;
@class iFolderService;
@class SimiasService;

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
	
	NSObjectController				*ifolderDataAlias;

	NSRecursiveLock					*instanceLock;
	iFolderDomain					*defaultDomain;
}

+ (iFolderData *)sharedInstance;

-(NSArrayController *)domainArrayController;
-(NSArrayController *)ifolderArrayController;

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


-(void)setUsersAdded:(NSString *)ifolderID;
-(BOOL)usersAdded:(NSString *)ifolderID;
-(void)clearUsersAdded:(NSString *)ifolderID;

 @end
