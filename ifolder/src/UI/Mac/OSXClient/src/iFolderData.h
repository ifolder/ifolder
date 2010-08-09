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
*                 $Modified by: Satyam <ssutapalli@novell.com>	29/02/2007     Implemented Merge iFolder process
*                 $Modified by: Satyam <ssutapalli@novell.com>	10/04/2008     Added functionality for default ifolder
*                 $Modified by: Satyam <ssutapalli@novell.com>	22/05/2008     Modified Create iFolder method signature
*                 $Modified by: Satyam <ssutapalli@novell.com>	20/08/2008     Added functionality to clear PP and getting remember PP option
*                 $Modified by: Satyam <ssutapalli@novell.com>  09/09/2008     Added new function for calling IsPassPhraseSet from Simias
*                 $Modified by: Satyam <ssutapalli@novell.com>  02/06/2009     Added new functions required for Forgot PP dialog
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import <Cocoa/Cocoa.h>
#include "SecurityInterface/SFCertificatePanel.h"

@class iFolder;
@class iFolderDomain;
@class User;
@class iFolderService;
@class SimiasService;
@class SyncSize;
@class MemberSearchResults;
@class clientUpdate;
@class AuthStatus;


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
	NSArrayController				*loggedDomainsController;
	
//	NSObjectController				*ifolderDataAlias;

	NSRecursiveLock					*instanceLock;
	iFolderDomain					*defaultDomain;
	clientUpdate                    *cliUpdate;
	BOOL                             forceQuit;
}

+ (iFolderData *)sharedInstance;
//- (BOOL)ForceQuit;

-(NSArrayController *)domainArrayController;
-(NSArrayController *)loggedDomainArrayController;

-(NSArrayController *)ifolderArrayController;
//-(NSObjectController *)dataAlias;

- (void)refresh:(BOOL)onlyDomains;

-(void)_addiFolder:(iFolder *)ifolder;
-(void)_deliFolder:(NSString *)ifolderID;
-(NSString *)getiFolderID:(NSString *)subscriptionID;
-(void)_addDomain:(iFolderDomain *)domain;

-(BOOL)isDomain:(NSString *)domainID;
-(BOOL)isiFolder:(NSString *)ifolderID;
-(BOOL)isiFolderByPath:(NSString*)localPath;
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
-(void)selectDefaultLoggedDomain;

-(NSString*)createiFolder:(NSString *)path inDomain:(NSString *)domainID withSSL:(BOOL)ssl usingAlgorithm:(NSString *)encrAlgthm usingPassPhrase:(NSString *)passPhrase;
-(void)acceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath canMerge:(BOOL)merge;
-(void)declineiFolderInvitation:(NSString *)iFolderID 
									fromDomain:(NSString *)domainID;
-(void)revertiFolder:(NSString *)iFolderID;
-(void)deleteiFolder:(NSString *)ifolderID fromDomain:(NSString *) DomainID;
-(void)synciFolderNow:(NSString *)ifolderID;


-(void)setUsersAdded:(NSString *)ifolderID;
-(BOOL)usersAdded:(NSString *)ifolderID;
-(void)clearUsersAdded:(NSString *)ifolderID;

-(SyncSize *)getSyncSize:(NSString *)ifolderID;

-(MemberSearchResults *) InitMemberSearchResults;
-(void)readCredentials;

-(int) getSecurityPolicy:(NSString*)domainID;
-(NSString*) getPassPhrase:(NSString*)domID;
-(NSString*)getRAName:(NSString*)domainID;
-(BOOL)exportiFoldersCryptoKeys:(NSString*)domainID withfilePath:(NSString*)filePath;
-(BOOL)importiFoldersCryptoKeys:(NSString*)domainID withNewPP:(NSString*)newPassPhrase onetimePassPhrase:(NSString*)otPassPhrase andFilePath:(NSString*)filePath;
-(NSArray*)getRAListOnClient:(NSString*)domainID;
-(BOOL)validatePassPhrase:(NSString*)domainID andPassPhrase:(NSString*)pPhrase;
-(BOOL)reSetPassPhrase:(NSString*)domainID oldPassPhrase:(NSString*)oldPP passPhrase:(NSString*)newPP withRAName:(NSString*)raName;
-(SecCertificateRef)getCertificate:(NSString*)domainID withRAName:(NSString*)raName;
-(void)storePassPhrase: (NSString*)domainID	PassPhrase:(NSString*)passPhrase andRememberPP:(BOOL)rememberPassPhrase;
-(NSArray *) getiFolderConflicts:(NSString *)ifolderID;
-(void)resolveEnhancedFileConflict:(NSString*)ifolderID havingConflictID:(NSString*)conflictID hasLocalChange:(BOOL)localOnly withConflictBinPath:(NSString*)conflictBinPath;
-(BOOL)createDirectoriesRecurssively:(NSString*)path;
-(void)checkForEncryption:(NSString*)domID atLogin:(BOOL)loginFlag;
-(NSString*)getDefaultiFolder:(NSString*)domID;
-(BOOL)defaultAccountInDomainID:(NSString*)domainID foriFolderID:(NSString*)ifolderID;
-(void)clearPassPhrase:(NSString*)domainID;
-(BOOL)getRememberPassphraseOption: (NSString*) domainID;
-(BOOL)canOwnerBeChanged:(NSString*)newUserID forDomain:(NSString*)domainID;
-(BOOL) isPassPhraseSet:(NSString*)domainID;
-(void)clientUpdates:(NSString*)domID showstatus:(BOOL)show;
//- (void)runUpdates:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo; // This method is to handle NSBeginAlertSheet to handle the upgrade client
-(NSNumber*)changeUserPassword:(NSString*)domainID changePassword:(NSString*)oldPasswd withNewPassword:(NSString*)newPasswd;
-(void)setDomainPassword:(NSString*)domainID withPassword:(NSString*)newPasswd;
-(NSString*)getDefaultServerPublicKey:(NSString*)domainID forUser:(NSString*)userID;
-(void) exportRecoverImport:(NSString*)domainID forUser:(NSString*)userID withPassphrase:(NSString*)newPP;
-(AuthStatus*)loginToRemoteDomain:(NSString*)domainID usingPassword:(NSString*)password;
-(iFolderDomain*)connectToDomain:(NSString *)UserName 
				   usingPassword:(NSString *)Password andHost:(NSString *)Host;
-(AuthStatus *) logoutFromRemoteDomain:(NSString *)domainID;
-(int)getLoggedDomainCount;
@end
