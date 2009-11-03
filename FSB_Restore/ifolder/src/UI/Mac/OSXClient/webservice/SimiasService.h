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
*                 $Modified by: Satyam <ssutapalli@novell.com>   20/03/2008      Added GetDefaultDomainID functionality
*                 $Modified by: Satyam <ssutapalli@novell.com>   08/04/2008      Added DefaultiFolder functionality
*                 $Modified by: Satyam <ssutapalli@novell.com>  02/06/2009     Added new functions required for Forgot PP dialog
*-----------------------------------------------------------------------------
* This module is used to:
* 		Connect to the server via gSoap. 
*
*******************************************************************************/

#ifndef __SimiasService__
#define __SimiasService__

#import <Cocoa/Cocoa.h>
#include <simiasStub.h>
//#include <Carbon/Carbon.h>
#import "iFolderDomain.h"
#include "Security/Security.h"

@class AuthStatus;
@class MemberSearchResults;

@interface SimiasService : NSObject
{
	NSString	*simiasURL;
	void		*soapData;	
	
	enum 
	{
		None = 0,
		NotRequired = 1,
		Basic = 2,
		PPK = 3,
	}CredentialType;
}

-(NSArray *) GetDomains:(BOOL)onlySlaves;
-(iFolderDomain *) ConnectToDomain:(NSString *)UserName 
				usingPassword:(NSString *)Password andHost:(NSString *)Host;
-(void) LeaveDomain:(NSString *)domainID withOption:(BOOL)localOnly;
-(BOOL) ValidCredentials:(NSString *)domainID forUser:(NSString *)userID;
-(void) SetDomainPassword:(NSString *)domainID password:(NSString *)password;
-(NSString *) GetDomainPassword:(NSString *)domainID;
-(void) SetDomainActive:(NSString *)domainID;
-(void) SetDomainInactive:(NSString *)domainID;
-(void) SetDefaultDomain:(NSString *)domainID;

-(SecCertificateRef) GetCertificate:(NSString *)host;
-(void) StoreCertificate:(SecCertificateRef)cert forHost:(NSString *)host;


-(void) DisableDomainAutoLogin:(NSString *)domainID;
-(AuthStatus *) LoginToRemoteDomain:(NSString *)domainID 
						usingPassword:(NSString *)password;
-(AuthStatus *) LogoutFromRemoteDomain:(NSString *)domainID;

-(BOOL) SetProxyAddress:(NSString *)hostURI 
				ProxyURI:(NSString *)proxyURI
				ProxyUser:(NSString *)proxyUser 
				ProxyPassword:(NSString *)proxyPassword;

-(BOOL) SetDomainHostAddress:(NSString *)domainID
				withAddress:(NSString *)hostAddress
				forUser:(NSString *)userName
				withPassword:(NSString *)password;

-(MemberSearchResults *) InitMemberSearchResults;
-(void)readCredentials;

-(BOOL) IsPassPhraseSet:(NSString*)domainID;
-(BOOL) GetRememberPassPhraseOption:(NSString*)domainID;
-(NSString*) GetPassPhrase:(NSString*)domainID;
-(AuthStatus*) SetPassPhrase:(NSString*)domainID 
				passPhrase:(NSString*)pPhrase 
				recoveryAgent:(NSString*)rAgent 
				andPublicKey:(NSString*)publicKey;
-(void) StorePassPhrase:(NSString *)domainID
		 	PassPhrase:(NSString *)passPhrase
			Type:(int)type
			andRememberPP:(BOOL)rememberPassPhrase;
-(AuthStatus*) ValidatePassPhrase:(NSString*)domainID withPassPhrase:(NSString*) passPhrase;
-(NSArray*) GetRAListOnClient:(NSString*)domainID;
-(SecCertificateRef) GetRACertificateOnClient:(NSString*)domainID withRecoveryAgent:(NSString*)rAgent;
-(void) StoreRACertificate:(SecCertificateRef)cert forRecoveryAgent:(NSString *)rAgent;
-(NSString*) GetPublicKey:(NSString*)domainID forRecoveryAgent:(NSString*)rAgent;
-(iFolderDomain *) GetDomainInformation:(NSString*)domainID;
-(void)ExportiFolderCryptoKeys:(NSString*)domainID withFile:(NSString*)filePath;
-(void)ImportiFoldersCryptoKeys:(NSString*)domainID withNewPP:(NSString*)newPassPhrase onetimePassPhrase:(NSString*)otPassPhrase andFilePath:(NSString*)filePath;
-(AuthStatus*)ReSetPassPhrase:(NSString*)domainID oldPassPhrase:(NSString*)oldPassPhrase passPhrase:(NSString*)passPhrase withRAName:(NSString*)raName andPublicKey:(NSString*)pubKey;
-(NSString*) GetDefaultDomainID;
-(NSString*) GetDefaultiFolder:(NSString*)domainID;
-(BOOL) DefaultAccountInDomainID:(NSString*)domainID foriFolderID:(NSString*)ifolderID;
-(void) RemoveCertFromTable:(NSString*)hostURL;
-(void) ExportRecoverImport:(NSString*)domainID forUser:(NSString*)userID withPassphrase:(NSString*)newPP;
-(NSString*) GetDefaultPublicKey:(NSString*)domainID;
@end

#endif // __SimiasService__
