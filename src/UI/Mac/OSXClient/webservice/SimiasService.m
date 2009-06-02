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
*                 $Modified by: Satyam <ssutapalli@novell.com>   29/02/2008      Updated getDomainProperties and getAuthStatus methods with new properties, and increased timeout to 60 seconds
*                 $Modified by: Satyam <ssutapalli@novell.com>   20/03/2008      Added GetDefaultDomainID functionality
*                 $Modified by: Satyam <ssutapalli@novell.com>   08/04/2008      Added DefaultiFolder functionality
*                 $Modified by: Satyam <ssutapalli@novell.com>  02/06/2009     Added new functions required for Forgot PP dialog
*-----------------------------------------------------------------------------
* This module is used to:
*       	Connect to Simias on Server via gSoap 
*
*******************************************************************************/

#import "SimiasService.h"
#include <simiasStub.h>
#include <simias.nsmap>
#import "Simias.h"
#import "AuthStatus.h"
#import "MemberSearchResults.h"
#import "applog.h"

typedef struct soap_struct
{
	char				*username;
	char				*password;
	struct soap			*soap;
	NSRecursiveLock		*instanceLock;	
} SOAP_DATA;

@implementation SimiasService

void handle_simias_soap_error(void *soapData, NSString *methodName);
struct soap *lockSimiasSoap(void *soapData);
void unlockSimiasSoap(void *soapData);

NSDictionary *getDomainProperties(struct ns1__DomainInformation *domainInfo);
NSDictionary *getAuthStatus(struct ns1__Status *status);

//----------------------------------------------------------------------------
// init
// Initialize the soap related things here
//----------------------------------------------------------------------------
- (id)init 
{
	SOAP_DATA	*pSoap;
	[super init];
	simiasURL = [[NSString stringWithFormat:@"%@/Simias.asmx", [[Simias getInstance] simiasURL]] retain];
	
	soapData = malloc(sizeof(SOAP_DATA));
	pSoap = (SOAP_DATA *)soapData;
	// the following code used to be done in init_gsoap

	pSoap->soap = malloc(sizeof(struct soap));
	if(pSoap->soap != NULL)
	{
		soap_init2(pSoap->soap, (SOAP_IO_KEEPALIVE | SOAP_C_UTFSTRING | SOAP_IO_DEFAULT), (SOAP_IO_KEEPALIVE | SOAP_C_UTFSTRING | SOAP_IO_DEFAULT));
		soap_set_namespaces(pSoap->soap, simias_namespaces);
		// Set the timeout for send and receive to 300 seconds
		pSoap->soap->recv_timeout = 60;
		pSoap->soap->send_timeout = 60;
	}
	
	pSoap->username = malloc(1024);
	if(pSoap->username != NULL)
	{
		memset(pSoap->username, 0, 1024);
	}
	pSoap->password = malloc(1024);
	if(pSoap->password != NULL)
	{
		memset(pSoap->password, 0, 1024);
	}

	if( (pSoap->username != NULL) && (pSoap->password != NULL) && (pSoap->soap != NULL) )
	{
		if(simias_get_web_service_credential(pSoap->username, pSoap->password) == 0)
		{
			pSoap->soap->userid = pSoap->username;
			pSoap->soap->passwd = pSoap->password;
		}
	}
	
	[self readCredentials];
	
	pSoap->instanceLock = [[NSRecursiveLock alloc] init];
	
    return self;
}

//----------------------------------------------------------------------------
// dealloc
// Deallocate the resources before cleaning
//----------------------------------------------------------------------------
-(void)dealloc
{
	SOAP_DATA	*pSoap;
	pSoap = (SOAP_DATA *)soapData;	
	
	if(pSoap->username != NULL)
	{
		free(pSoap->username);
		pSoap->username = NULL;
	}
	if(pSoap->password != NULL)
	{
		free(pSoap->password);
		pSoap->password = NULL;
	}
	
	soap_done(pSoap->soap);

	[pSoap->instanceLock release];
	
	free(pSoap);
	pSoap = NULL;
	soapData = NULL;

	if(simiasURL != nil)	
		[simiasURL release];
    [super dealloc];
}



//----------------------------------------------------------------------------
// ValidCredentials
// Checks the specified domainID and userID for valid Credentials
//----------------------------------------------------------------------------
-(BOOL)ValidCredentials:(NSString *)domainID forUser:(NSString *)userID
{
	BOOL validCreds = NO;
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);

    struct _ns1__ValidCredentials ns1__validCreds;
    struct _ns1__ValidCredentialsResponse ns1__validCredsResponse;

    err_code = soap_call___ns1__ValidCredentials (pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &ns1__validCreds,
            &ns1__validCredsResponse);

    if (err_code == SOAP_OK)
    {
		validCreds = (BOOL) ns1__validCredsResponse.ValidCredentialsResult;
    }

	unlockSimiasSoap(soapData);

	return validCreds;
}




//----------------------------------------------------------------------------
// GetDomains
// Reads domains from store
//----------------------------------------------------------------------------
-(NSArray *) GetDomains:(BOOL)onlySlaves
{
	NSMutableArray *domains = nil;
        int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);
	
	struct _ns1__GetDomains getDomainsMessage;
	struct _ns1__GetDomainsResponse getDomainsResponse;

	getDomainsMessage.onlySlaves = onlySlaves;

        err_code = soap_call___ns1__GetDomains(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getDomainsMessage,
            &getDomainsResponse);

	// This will free the soap structure and creds and throw an exception if there is an error
	handle_simias_soap_error(soapData, @"SimiasService.GetDomains");

	int domainCount = getDomainsResponse.GetDomainsResult->__sizeDomainInformation;
	if(domainCount > 0)
	{	
		domains = [[NSMutableArray alloc] initWithCapacity:domainCount];

		int counter;
		for(counter=0;counter<domainCount;counter++)
		{
			struct ns1__DomainInformation *curDomain;
			
			curDomain = getDomainsResponse.GetDomainsResult->DomainInformation[counter];
			iFolderDomain *newDomain = [[iFolderDomain alloc] init];

			[newDomain setProperties:getDomainProperties(curDomain)];
			
			[domains addObject:newDomain];
		}
	}

	unlockSimiasSoap(soapData);

	return domains;
}




//----------------------------------------------------------------------------
// ConnectToDomain
// "Joins" a new domain
//----------------------------------------------------------------------------
-(iFolderDomain *) ConnectToDomain:(NSString *)UserName usingPassword:(NSString *)Password andHost:(NSString *)Host
{
	iFolderDomain *domain = nil;
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);	

	NSAssert( (UserName != nil), @"UserName was nil");
	NSAssert( (Password != nil), @"Password was nil");
	NSAssert( (Host != nil), @"Host was nil");

	struct _ns1__ConnectToDomain connectToDomainMessage;
	struct _ns1__ConnectToDomainResponse connectToDomainResponse;
	
	connectToDomainMessage.UserName = (char *)[UserName UTF8String];
	connectToDomainMessage.Password = (char *)[Password UTF8String];
	connectToDomainMessage.Host = (char *)[Host UTF8String];

    err_code = soap_call___ns1__ConnectToDomain(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &connectToDomainMessage,
            &connectToDomainResponse);
	
 	if(pSoap->error)
	{
		if( (soap_soap_error_check(pSoap->error)) && 
			(pSoap->fault != NULL) && 
			(pSoap->fault->faultstring != NULL) &&
			(strstr(pSoap->fault->faultstring, "Domain") == pSoap->fault->faultstring) &&
			(strstr(pSoap->fault->faultstring, "already exists") != NULL) )
		{
			NSString *faultString = [NSString stringWithUTF8String:pSoap->fault->faultstring];
			
			unlockSimiasSoap(soapData);
			[NSException raise:[NSString stringWithFormat:@"%@", faultString]
					format:@"DomainExistsError"];
		}
		else
		{
			// This will free the soap structure and creds and throw an exception if there is an error
			handle_simias_soap_error(soapData, @"SimiasService.ConnectToDomain");
		}
	}
	else
	{
		struct ns1__DomainInformation *curDomain;
		curDomain = connectToDomainResponse.ConnectToDomainResult;
		if(curDomain == NULL)
		{
			unlockSimiasSoap(soapData);
			[NSException raise:@"Unable to connect to domain"
							format:@"Error in ConnectToDomain"];		
		}

		domain = [[[iFolderDomain alloc] init] autorelease];
		[domain setProperties:getDomainProperties(curDomain)];
    }
	unlockSimiasSoap(soapData); 
	return domain;
}



//----------------------------------------------------------------------------
// LeaveDomain
// Leaves the domain specified and if specified, leaves it from all machines
//----------------------------------------------------------------------------
-(void) LeaveDomain:(NSString *)domainID withOption:(BOOL)localOnly
{
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);	

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__LeaveDomain			leaveDomainMessage;
	struct _ns1__LeaveDomainResponse	leaveDomainResponse;

	leaveDomainMessage.DomainID = (char *)[domainID UTF8String];
	leaveDomainMessage.LocalOnly = localOnly;

    err_code = soap_call___ns1__LeaveDomain(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &leaveDomainMessage,
            &leaveDomainResponse);

	handle_simias_soap_error(soapData, @"SimiasService.LeaveDomain");

	unlockSimiasSoap(soapData);
}




//----------------------------------------------------------------------------
// SetDomainPassword
// Saves the password in the store
//----------------------------------------------------------------------------
-(void)SetDomainPassword:(NSString *)domainID password:(NSString *)password
{
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);
	
	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SetDomainCredentials			saveDomainCredsMessage;
	struct _ns1__SetDomainCredentialsResponse	saveDomainCredsResponse;

	if(password == nil)
	{
		saveDomainCredsMessage.domainID = (char *)[domainID UTF8String];
		saveDomainCredsMessage.credentials = (char *)NULL;
		saveDomainCredsMessage.type = ns1__CredentialType__None;
	}
	else
	{
		saveDomainCredsMessage.domainID = (char *)[domainID UTF8String];
		saveDomainCredsMessage.credentials = (char *)[password UTF8String];
		saveDomainCredsMessage.type = ns1__CredentialType__Basic;
	}

    err_code = soap_call___ns1__SetDomainCredentials(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &saveDomainCredsMessage,
            &saveDomainCredsResponse);

	handle_simias_soap_error(soapData, @"SimiasService.SetDomainPassword");

	unlockSimiasSoap(soapData);
}




//----------------------------------------------------------------------------
// GetDomainPassword
// gets the saved password from the store
//----------------------------------------------------------------------------
-(NSString *)GetDomainPassword:(NSString *)domainID
{
	NSString *password = nil;
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);	

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__GetDomainCredentials			getDomainCredsMessage;
	struct _ns1__GetDomainCredentialsResponse	getDomainCredsResponse;

	getDomainCredsMessage.domainID = (char *)[domainID UTF8String];

    err_code = soap_call___ns1__GetDomainCredentials(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getDomainCredsMessage,
            &getDomainCredsResponse);

	handle_simias_soap_error(soapData, @"SimiasService.GetDomainPassword");

	if(getDomainCredsResponse.GetDomainCredentialsResult == ns1__CredentialType__Basic)
	{
		password = [NSString stringWithUTF8String:getDomainCredsResponse.credentials];
	}

	unlockSimiasSoap(soapData);

	return password;
}




//----------------------------------------------------------------------------
// GetSavedDomainPassword
// gets the saved password from the store
//----------------------------------------------------------------------------
-(void)SetDomainActive:(NSString *)domainID
{
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);	

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SetDomainActive			setDomainActiveMessage;
	struct _ns1__SetDomainActiveResponse	setDomainActiveResponse;

	setDomainActiveMessage.domainID = (char *)[domainID UTF8String];

    err_code = soap_call___ns1__SetDomainActive(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setDomainActiveMessage,
            &setDomainActiveResponse);

	handle_simias_soap_error(soapData, @"SimiasService.SetDomainActive");

	unlockSimiasSoap(soapData);
}




//----------------------------------------------------------------------------
// GetSavedDomainPassword
// gets the saved password from the store
//----------------------------------------------------------------------------
-(void)SetDomainInactive:(NSString *)domainID
{
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);	

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SetDomainInactive			setDomainInactiveMessage;
	struct _ns1__SetDomainInactiveResponse	setDomainInactiveResponse;

	setDomainInactiveMessage.domainID = (char *)[domainID UTF8String];

    err_code = soap_call___ns1__SetDomainInactive(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setDomainInactiveMessage,
            &setDomainInactiveResponse);

	handle_simias_soap_error(soapData, @"SimiasService.SetDomainInactive");

	unlockSimiasSoap(soapData);
}




//----------------------------------------------------------------------------
// SetDefaultDomain
// Set the specified domainID as the default domain
//----------------------------------------------------------------------------
-(void) SetDefaultDomain:(NSString *)domainID
{
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);	

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SetDefaultDomain			setDefaultDomainMessage;
	struct _ns1__SetDefaultDomainResponse	setDefaultDomainResponse;

	setDefaultDomainMessage.domainID = (char *)[domainID UTF8String];

    err_code = soap_call___ns1__SetDefaultDomain(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setDefaultDomainMessage,
            &setDefaultDomainResponse);

	handle_simias_soap_error(soapData, @"SimiasService.SetDefaultDomain");

	unlockSimiasSoap(soapData);
}




//----------------------------------------------------------------------------
// LoginToRemoteDomain
// Provide the credentials to authenticate to a domain
//----------------------------------------------------------------------------
-(AuthStatus *) LoginToRemoteDomain:(NSString *)domainID usingPassword:(NSString *)password
{
	AuthStatus *authStatus = nil;
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);	

	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (password != nil), @"password was nil");

	struct _ns1__LoginToRemoteDomain			loginToDomainMessage;
	struct _ns1__LoginToRemoteDomainResponse	loginToDomainResponse;
	
	loginToDomainMessage.domainID = (char *)[domainID UTF8String];
	loginToDomainMessage.password = (char *)[password UTF8String];

    err_code = soap_call___ns1__LoginToRemoteDomain(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &loginToDomainMessage,
            &loginToDomainResponse);

	handle_simias_soap_error(soapData, @"SimiasService.LoginToRemoteDomain");

	struct ns1__Status *status = loginToDomainResponse.LoginToRemoteDomainResult;
	if(status == NULL)
	{
		unlockSimiasSoap(soapData);
		[NSException raise:@"Authentication returned null object"
						format:@"Error in AuthenticateToDomain"];		
	}

	authStatus = [[[AuthStatus alloc] init] autorelease];
	[authStatus setProperties:getAuthStatus(status)];

    unlockSimiasSoap(soapData);

	return authStatus;
}




//----------------------------------------------------------------------------
// GetCertificate
// Gets the certificate for a specified host
//----------------------------------------------------------------------------
-(SecCertificateRef) GetCertificate:(NSString *)host
{
	SecCertificateRef certRef = NULL;

    int err_code;
	CSSM_DATA certData;

	struct soap *pSoap = lockSimiasSoap(soapData);	

	NSAssert( (host != nil), @"host was nil");

	struct _ns1__GetCertificate			getCertMessage;
	struct _ns1__GetCertificateResponse	getCertResponse;
	
	getCertMessage.host = (char *)[host UTF8String];

    err_code = soap_call___ns1__GetCertificate(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getCertMessage,
            &getCertResponse);

	handle_simias_soap_error(soapData, @"SimiasService.GetCertificate");

	struct xsd__base64Binary *certResult = getCertResponse.GetCertificateResult;
	if(certResult == NULL)
	{
		unlockSimiasSoap(soapData);
		[NSException raise:@"GetCertificate returned null object"
						format:@"Error in GetCertificate"];		
	}

	certData.Data = certResult->__ptr;
	certData.Length = certResult->__size;

	OSStatus status =  SecCertificateCreateFromData ( &certData,
											CSSM_CERT_X_509v3,
											CSSM_CERT_ENCODING_BER,
											&certRef);

    unlockSimiasSoap(soapData);

	return certRef;
}



//----------------------------------------------------------------------------
// StoreCertificate
// Stores the certificate so it can be used when users login to the host
//----------------------------------------------------------------------------
-(void) StoreCertificate:(SecCertificateRef)cert forHost:(NSString *)host
{
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);	

	NSAssert( (host != nil), @"host was nil");
	NSAssert( (cert != NULL), @"cert was NULL");

	struct _ns1__StoreCertificate			storeCertMessage;
	struct _ns1__StoreCertificateResponse	storeCertResponse;
	struct xsd__base64Binary				certBinary;
	CSSM_DATA								certData;

	OSStatus status = SecCertificateGetData (cert, &certData);
	if(status == 0)
	{
		certBinary.__ptr = certData.Data;
		certBinary.__size = certData.Length;
		certBinary.id = NULL;
		certBinary.type = NULL;
		certBinary.option = NULL;

		storeCertMessage.host = (char *)[host UTF8String];
		storeCertMessage.certificate = &certBinary;

		err_code = soap_call___ns1__StoreCertificate(
				pSoap,
				[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
				NULL,
				&storeCertMessage,
				&storeCertResponse);

		handle_simias_soap_error(soapData, @"SimiasService.StoreCertificate");

		//unlockSimiasSoap(soapData);
	}
	unlockSimiasSoap(soapData);
}




//----------------------------------------------------------------------------
// DisableDomainAutoLogin
// Disables Simias from asking for password
//----------------------------------------------------------------------------
-(void) DisableDomainAutoLogin:(NSString *)domainID
{
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);	

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__DisableDomainAutoLogin			disableDomainMessage;
	struct _ns1__DisableDomainAutoLoginResponse	disableDomainResponse;

	disableDomainMessage.domainID = (char *)[domainID UTF8String];

    err_code = soap_call___ns1__DisableDomainAutoLogin(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &disableDomainMessage,
            &disableDomainResponse);

	handle_simias_soap_error(soapData, @"SimiasService.DisableDomainAutoLogin");

	unlockSimiasSoap(soapData);
}




//----------------------------------------------------------------------------
// LogoutFromRemoteDomain
// Will cause simias to no longer talk to this domain (logout)
//----------------------------------------------------------------------------
-(AuthStatus *) LogoutFromRemoteDomain:(NSString *)domainID
{
	AuthStatus *authStatus = nil;
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);	
	

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__LogoutFromRemoteDomain			logoutDomainMessage;
	struct _ns1__LogoutFromRemoteDomainResponse	logoutDomainResponse;

	logoutDomainMessage.domainID = (char *)[domainID UTF8String];

    err_code = soap_call___ns1__LogoutFromRemoteDomain(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &logoutDomainMessage,
            &logoutDomainResponse);

	handle_simias_soap_error(soapData, @"SimiasService.LogoutFromRemoteDomain");

	struct ns1__Status *status = logoutDomainResponse.LogoutFromRemoteDomainResult;
	if(status == NULL)
	{
		unlockSimiasSoap(soapData);
		[NSException raise:@"Authentication returned null object"
						format:@"Error in AuthenticateToDomain"];		
	}

	authStatus = [[[AuthStatus alloc] init] autorelease];
	[authStatus setProperties:getAuthStatus(status)];

	unlockSimiasSoap(soapData);

	return authStatus;
}



//----------------------------------------------------------------------------
// SetProxyAddress
// Sets the Proxy Address and credentials to be used when communicating
// with the iFolder Server
//----------------------------------------------------------------------------
-(BOOL) SetProxyAddress:(NSString *)hostURI 
				ProxyURI:(NSString *)proxyURI
				ProxyUser:(NSString *)proxyUser 
				ProxyPassword:(NSString *)proxyPassword
{
    int err_code;
	BOOL setProxyResult = NO;
	struct soap *pSoap = lockSimiasSoap(soapData);	

	struct _ns1__SetProxyAddress			setProxyMessage;
	struct _ns1__SetProxyAddressResponse	setProxyResponse;

	NSAssert( (hostURI != nil), @"hostURI was nil");

	setProxyMessage.hostUri = (char *)[hostURI UTF8String];
	if(proxyURI != nil)
		setProxyMessage.proxyUri = (char *)[proxyURI UTF8String];
	else
		setProxyMessage.proxyUri = NULL;
	if(proxyUser != nil)
		setProxyMessage.proxyUser = (char *)[proxyUser UTF8String];
	else
		setProxyMessage.proxyUser = NULL;
	if(proxyPassword != nil)
		setProxyMessage.proxyPassword = (char *)[proxyPassword UTF8String];
	else
		setProxyMessage.proxyPassword = NULL;

    err_code = soap_call___ns1__SetProxyAddress(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setProxyMessage,
            &setProxyResponse);

	handle_simias_soap_error(soapData, @"SimiasService.SetProxyAddress");

	setProxyResult = (BOOL) setProxyResponse.SetProxyAddressResult;

	unlockSimiasSoap(soapData);

	return setProxyResult;
}



//----------------------------------------------------------------------------
// SetDomainHostAddress
// Sets the Address for the specified domainID
//----------------------------------------------------------------------------
-(BOOL) SetDomainHostAddress:(NSString *)domainID
							withAddress:(NSString *)hostAddress
							forUser:(NSString *)userName
							withPassword:(NSString *)password
{
    int err_code;
	BOOL setHostResult = NO;
	struct soap *pSoap = lockSimiasSoap(soapData);	

	struct _ns1__SetDomainHostAddress			setAddrMessage;
	struct _ns1__SetDomainHostAddressResponse	setAddrResponse;

	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (hostAddress != nil), @"hostAddress was nil");
	NSAssert( (userName != nil), @"userName was nil");
	NSAssert( (password != nil), @"password was nil");

	setAddrMessage.domainID = (char *)[domainID UTF8String];
	setAddrMessage.hostAddress = (char *)[hostAddress UTF8String];
	setAddrMessage.user = (char *)[userName UTF8String];
	setAddrMessage.password = (char *)[password UTF8String];
	
    err_code = soap_call___ns1__SetDomainHostAddress(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setAddrMessage,
            &setAddrResponse);

	handle_simias_soap_error(soapData, @"SimiasService.SetDomainHostAddress");

	setHostResult = (BOOL) setAddrResponse.SetDomainHostAddressResult;

	unlockSimiasSoap(soapData);

	return setHostResult;
}

//----------------------------------------------------------------------------
// IsPassPhraseSet
// Check whether pass phrase is set for the domain or not
//----------------------------------------------------------------------------
-(BOOL) IsPassPhraseSet:(NSString*)domainID
{
	struct soap *pSoap = lockSimiasSoap(soapData);	
	
	struct _ns1__IsPassPhraseSet		passPhraseInput;
	struct _ns1__IsPassPhraseSetResponse 	passPhraseResponse;

	NSAssert( (domainID != nil), @"domainID was nil");

	passPhraseInput.DomainID = (char*)[domainID UTF8String];

	int err_code = soap_call___ns1__IsPassPhraseSet(
			pSoap,
			[simiasURL UTF8String],	//http://127.0.0.1:8086/simias10/Simias.asmx
			NULL,
			&passPhraseInput,
			&passPhraseResponse);

	handle_simias_soap_error(soapData, @"SimiasService.IsPassPhraseSet");

	BOOL isPassPhraseSet = (BOOL) passPhraseResponse.IsPassPhraseSetResult;
	
	unlockSimiasSoap(soapData);

	return isPassPhraseSet;
}

//----------------------------------------------------------------------------
// GetRememberPassPhraseOption
// Find and get whether remember pass phrase option is set or not
//----------------------------------------------------------------------------
-(BOOL) GetRememberPassPhraseOption:(NSString*)domainID
{
	struct soap *pSoap = lockSimiasSoap(soapData);	

	struct _ns1__GetRememberOption		getoptionInput;
	struct _ns1__GetRememberOptionResponse	getoptionResponse;

	NSAssert( (domainID != nil), @"domainID was nil");

	getoptionInput.domainID = (char*)[domainID UTF8String];

	int err_code = soap_call___ns1__GetRememberOption(
			pSoap,
			[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
			NULL,
			&getoptionInput,
			&getoptionResponse);

	handle_simias_soap_error(soapData, @"SimiasService.GetRememberPassPhraseOption");

	BOOL rememberOption = (BOOL) getoptionResponse.GetRememberOptionResult;
	
	unlockSimiasSoap(soapData);
	
	return rememberOption;
}

//----------------------------------------------------------------------------
// GetPassPhrase
// Get the pass phrase set for the domain
//----------------------------------------------------------------------------
-(NSString*) GetPassPhrase:(NSString*)domainID
{
	struct soap *pSoap = lockSimiasSoap(soapData);	
	struct _ns1__GetPassPhrase		getPassPhraseInput;
	struct _ns1__GetPassPhraseResponse 	getPassPhraseResponse;

	NSAssert( (domainID != nil), @"domainID was nil");

	getPassPhraseInput.domainID = (char*) [domainID UTF8String];

	int err_code = soap_call___ns1__GetPassPhrase(
			pSoap,
			[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
			NULL, 
			&getPassPhraseInput,
			&getPassPhraseResponse);

	handle_simias_soap_error(soapData, @"SimiasService.GetPassPhrase");
	
	NSString *passPhrase = nil;
	@try
	{
		if(getPassPhraseResponse.GetPassPhraseResult != nil)
			passPhrase = [NSString stringWithUTF8String:getPassPhraseResponse.GetPassPhraseResult];
	}
	@catch(NSException *ex)
	{
		NSLog(@"Exception in getting pass phrase");
	}

	unlockSimiasSoap(soapData);

	return passPhrase;
}

//----------------------------------------------------------------------------
// SetPassPhrase:passPhrase:recoveryAgent:andPublicKey
// Set th epass phrase with new pass phrase for recovery agent with public key
//----------------------------------------------------------------------------
-(AuthStatus*) SetPassPhrase:(NSString*)domainID passPhrase:(NSString*)pPhrase 
				recoveryAgent:(NSString*)rAgent andPublicKey:(NSString*)publicKey
{
	struct soap *pSoap = lockSimiasSoap(soapData);	
	
	struct _ns1__SetPassPhrase		setPassPhraseInput;
	struct _ns1__SetPassPhraseResponse	setPassPhraseResponse;

	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (pPhrase != nil), @"pPhrase was nil");
	NSAssert( (rAgent != nil), @"rAgent was nil");

	setPassPhraseInput.DomainID = (char*) [domainID UTF8String];
	setPassPhraseInput.PassPhrase = (char*) [pPhrase UTF8String];
	setPassPhraseInput.RAName = (char*) [rAgent UTF8String];
	setPassPhraseInput.RAPublicKey = (char*) [publicKey UTF8String];

	int err_code = soap_call___ns1__SetPassPhrase(
			pSoap,
			[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
			NULL,
			&setPassPhraseInput,
			&setPassPhraseResponse);

	handle_simias_soap_error(soapData, @"SimiasService.SetPassPhrase");

	AuthStatus *authStatus = nil;
	struct ns1__Status *status = setPassPhraseResponse.SetPassPhraseResult;
	if(status == NULL)
	{
                unlockSimiasSoap(soapData);
                [NSException raise:@"SetPassPhrase returned null object"
                                                format:@"Error in Setting Passphrase"];
	}

	authStatus = [[[AuthStatus alloc] init] autorelease];
	[authStatus setProperties:getAuthStatus(status)];

	unlockSimiasSoap(soapData);
	
	return authStatus;
}

//----------------------------------------------------------------------------
// StorePassPhrase:PassPhrase:Type:andRememberPP
// Store the pass phrase for the domain and to remember it or not
//----------------------------------------------------------------------------
-(void)StorePassPhrase: (NSString*)domainID
                        PassPhrase:(NSString*)passPhrase
                        Type:(int)type
                        andRememberPP:(BOOL)rememberPassPhrase
{
	struct soap *pSoap = lockSimiasSoap(soapData);	
	
	struct _ns1__StorePassPhrase		storePassPhraseInput;
	struct _ns1__StorePassPhraseResponse	storePassPhraseResponse;

	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (passPhrase != nil), @"PassPhrase was nil");

	storePassPhraseInput.domainID = (char*) [domainID UTF8String];
	storePassPhraseInput.passPhrase = (char*) [passPhrase UTF8String];
	storePassPhraseInput.type = type;
	storePassPhraseInput.rememberPassPhrase = rememberPassPhrase;

	int err_code = soap_call___ns1__StorePassPhrase(
			pSoap,
			[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
			NULL,
			&storePassPhraseInput,
			&storePassPhraseResponse);

	handle_simias_soap_error(soapData, @"SimiasService.StorePassPhrase"); 

	unlockSimiasSoap(soapData);
}

//----------------------------------------------------------------------------
// ValidatePassPhrase:withPassPhrase
// Validate the pass phrase for the domain
//----------------------------------------------------------------------------
-(AuthStatus*) ValidatePassPhrase:(NSString*)domainID withPassPhrase:(NSString*) passPhrase
{
	struct soap *pSoap = lockSimiasSoap(soapData);	

	struct _ns1__ValidatePassPhrase		validatePassPhraseInput;
	struct _ns1__ValidatePassPhraseResponse	validatePassPhraseResponse;

	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (passPhrase != nil), @"PassPhrase was nil");

	validatePassPhraseInput.DomainID = (char*) [domainID UTF8String];
	validatePassPhraseInput.PassPhrase = (char*) [passPhrase UTF8String];

	int err_code = soap_call___ns1__ValidatePassPhrase(
			pSoap,
			[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
			NULL,
			&validatePassPhraseInput,
			&validatePassPhraseResponse);

	handle_simias_soap_error(soapData, @"SimiasService.ValidatePassPhrase");


	AuthStatus *authStatus = nil;
	struct ns1__Status *status = validatePassPhraseResponse.ValidatePassPhraseResult;
	
	if(status == NULL)
	{
                unlockSimiasSoap(soapData);
                [NSException raise:@"ValidatePassPhrase returned null object"
                                                format:@"Error in Validating Passphrase"];
	}

	authStatus = [[[AuthStatus alloc] init] autorelease];
	[authStatus setProperties:getAuthStatus(status)];

	unlockSimiasSoap(soapData);
	
	return authStatus;
}

//----------------------------------------------------------------------------
// GetRAListOnClient
// Get the list of recovery agents
//----------------------------------------------------------------------------
-(NSArray*) GetRAListOnClient:(NSString*)domainID
{
	struct soap *pSoap = lockSimiasSoap(soapData);	

	struct _ns1__GetRAListOnClient		getRAListInput;
	struct _ns1__GetRAListOnClientResponse	getRAListResponse;	

	NSAssert( (domainID != nil), @"domainID was nil");

	getRAListInput.DomainID = (char*) [domainID UTF8String];

	int err_code = soap_call___ns1__GetRAListOnClient(
			pSoap,
			[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
			NULL,
			&getRAListInput,
			&getRAListResponse);

	handle_simias_soap_error(soapData, @"SimiasService.GetRAListOnClient");

	NSMutableArray* raList = nil;
	int raListCount = getRAListResponse.GetRAListOnClientResult->__sizestring ;	

	if( raListCount > 0 )
	{
		raList = [[NSMutableArray alloc] initWithCapacity:raListCount];
	
		int counter;
		for(counter = 0; counter < raListCount; counter++)
		{
			[raList addObject:[ NSString stringWithUTF8String: getRAListResponse.GetRAListOnClientResult->string[counter] ]];
		}
	}

	unlockSimiasSoap(soapData);

	return raList;
}

//----------------------------------------------------------------------------
// GetRACertificateOnClient:withRecoveryAgent
// Get the recovery agent certificate available on client
//----------------------------------------------------------------------------
-(SecCertificateRef) GetRACertificateOnClient:(NSString*)domainID withRecoveryAgent:(NSString*)rAgent
{	
	struct soap *pSoap = lockSimiasSoap(soapData);	

	struct _ns1__GetRACertificateOnClient		getRACertificateInput;
	struct _ns1__GetRACertificateOnClientResponse	getRACertificateResponse;

	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (rAgent != nil), @"rAgent was nil");

	getRACertificateInput.DomainID = (char*)[domainID UTF8String];
	getRACertificateInput.rAgent = (char*)[rAgent UTF8String];

	int err_code = soap_call___ns1__GetRACertificateOnClient(
			pSoap,
			[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
			NULL,
			&getRACertificateInput,
			&getRACertificateResponse);

	handle_simias_soap_error(soapData, @"SimiasService.GetRACertificateOnClient");

	SecCertificateRef certRef = NULL;
	CSSM_DATA certData;
	
	struct xsd__base64Binary *certResult = getRACertificateResponse.GetRACertificateOnClientResult;
	if(certResult == NULL)
        {
                unlockSimiasSoap(soapData);
                [NSException raise:@"GetRACertificateOnClient returned null object"
                                                format:@"Error in GetRACertificateOnClient"];
        }
	certData.Data = certResult->__ptr;
        certData.Length = certResult->__size;
	
	OSStatus status =  SecCertificateCreateFromData ( &certData, 
							CSSM_CERT_X_509v2, 
							CSSM_CERT_ENCODING_BER, 
							&certRef);

	unlockSimiasSoap(soapData);

	return certRef;
}

//----------------------------------------------------------------------------
// StoreRACertificate:forRecoveryAgent
// Store the recovery agent certificate for the recovery agent
//----------------------------------------------------------------------------
-(void) StoreRACertificate:(SecCertificateRef)cert forRecoveryAgent:(NSString *)rAgent
{
	struct soap *pSoap = lockSimiasSoap(soapData);	

	struct _ns1__StoreRACertificate				storeRACertificateInput;
	struct _ns1__StoreRACertificateResponse		storeRACertificateResponse;
	struct xsd__base64Binary					certBinary;
	CSSM_DATA									certData;
	
	NSAssert( (cert != nil), @"cert was NULL");	
	NSAssert( (rAgent != nil), @"rAgent was nil"); 

	OSStatus status = SecCertificateGetData(cert, &certData);

	if(status == 0)
	{
		certBinary.__ptr = certData.Data;
		certBinary.__size = certData.Length;
		certBinary.id = NULL;
		certBinary.type = NULL;
		certBinary.option = NULL;
		
		storeRACertificateInput.rAgent = (char *)[rAgent UTF8String];
                storeRACertificateInput.certificate = &certBinary;

		int err_code = soap_call___ns1__StoreRACertificate(
				pSoap,
				[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
				NULL,
				&storeRACertificateInput,
				&storeRACertificateResponse);

		handle_simias_soap_error(soapData, @"SimiasService.StoreRACertificate");
	}

	unlockSimiasSoap(soapData);
}

//----------------------------------------------------------------------------
// GetPublicKey:forRecoveryAgent
// Get the public key for the recovery agent
//----------------------------------------------------------------------------
-(NSString*) GetPublicKey:(NSString*)domainID forRecoveryAgent:(NSString*)rAgent
{
	struct soap *pSoap = lockSimiasSoap(soapData);	

	struct _ns1__GetPublicKey			publicKeyInput;
	struct _ns1__GetPublicKeyResponse	publicKeyResponse;

	NSAssert( (domainID != nil), @"domainID was nil");	
	NSAssert( (rAgent != nil), @"rAgent was nil"); 

	publicKeyInput.DomainID = (char*)[domainID UTF8String];
	publicKeyInput.rAgent = (char*)[rAgent UTF8String];

	int err_code = soap_call___ns1__GetPublicKey(
			pSoap,
			[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
			NULL,
			&publicKeyInput,
			&publicKeyResponse);
			
	handle_simias_soap_error(soapData, @"SimiasService.GetPublicKey");

	NSString* pKey = [NSString stringWithUTF8String:publicKeyResponse.GetPublicKeyResult];

	unlockSimiasSoap(soapData);
	
	return pKey;
}

//----------------------------------------------------------------------------
// GetDomainInformation
// Get domain related information for the domain
//----------------------------------------------------------------------------
-(iFolderDomain *) GetDomainInformation:(NSString*)domainID
{
	struct soap *pSoap = lockSimiasSoap(soapData);	
	
	struct _ns1__GetDomainInformation			domainInfoInput;
	struct _ns1__GetDomainInformationResponse	domainInfoResponse;
	
	NSAssert( (domainID != nil), @"domainID was nil");
	
	domainInfoInput.domainID = (char*)[domainID UTF8String];
	
	int err_code = soap_call___ns1__GetDomainInformation(
			pSoap,
			[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
			NULL,
			&domainInfoInput,
			&domainInfoResponse);
			
	handle_simias_soap_error(soapData,@"SimiasService.GetDomainInformation");
			
	struct ns1__DomainInformation *domInfo = domainInfoResponse.GetDomainInformationResult;
	if(domInfo == NULL)
	{
		unlockSimiasSoap(soapData);
		[NSException raise:@"Unable to connect to domain"
					format:@"Error in ConnectToDomain"];		
	}
	
	iFolderDomain *newDomain = [[[iFolderDomain alloc] init] autorelease];
	[newDomain setProperties:getDomainProperties(domInfo)];
	
	unlockSimiasSoap(soapData);
	
	return newDomain;
}

//----------------------------------------------------------------------------
// ExportiFolderCryptoKeys:withFile
// Export the encryption keys at file path provided
//----------------------------------------------------------------------------
-(void)ExportiFolderCryptoKeys:(NSString*)domainID withFile:(NSString*)filePath
{
	struct soap *pSoap = lockSimiasSoap(soapData);
	
	struct _ns1__ExportiFoldersCryptoKeys          exportKeysInput;
	struct _ns1__ExportiFoldersCryptoKeysResponse  exportKeysResponse;
	
	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (filePath != nil), @"filePath was nil");
	
	exportKeysInput.DomainID = (char*)[domainID UTF8String];
	exportKeysInput.FilePath = (char*)[filePath UTF8String];
	
	int err_code = soap_call___ns1__ExportiFoldersCryptoKeys(
				 pSoap,
				 [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
				 NULL,
				 &exportKeysInput,
				 &exportKeysResponse);
	
	handle_simias_soap_error(soapData,@"SimiasService.ExportiFolderCryptoKeys");
	
	unlockSimiasSoap(soapData);
}

//----------------------------------------------------------------------------
// ImportiFoldersCryptoKeys:withNewPP:onetimePassPhrase:andFilePath
// Import the encryption keys with new passphrase, one time pass phrase from the file available at File path
//----------------------------------------------------------------------------
-(void)ImportiFoldersCryptoKeys:(NSString*)domainID withNewPP:(NSString*)newPassPhrase onetimePassPhrase:(NSString*)otPassPhrase andFilePath:(NSString*)filePath
{
	struct soap *pSoap = lockSimiasSoap(soapData);
	
	struct _ns1__ImportiFoldersCryptoKeys         importCryptoInput;
	struct _ns1__ImportiFoldersCryptoKeysResponse importCryptoResponse;
	
	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (newPassPhrase != nil), @"newPassPhrase was nil");
	NSAssert( (filePath != nil), @"filePath was nil");
	
	importCryptoInput.DomainID = (char*)[domainID UTF8String];
	importCryptoInput.NewPassphrase = (char*)[newPassPhrase UTF8String];
	importCryptoInput.OneTimePassphrase = (char*)[otPassPhrase UTF8String];
	importCryptoInput.FilePath = (char*)[filePath UTF8String];
	
	int err_code = soap_call___ns1__ImportiFoldersCryptoKeys(
				 pSoap,
				 [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
				 NULL,
				 &importCryptoInput,
				 &importCryptoResponse);
	
	handle_simias_soap_error(soapData,@"SimiasService.ImportiFoldersCryptoKeys");
	
	unlockSimiasSoap(soapData);
}

//----------------------------------------------------------------------------
// ReSetPassPhrase:oldPassPhrase:passPhrase:withRAName
// Reset the pass phrase by taking old and new pass phrases for the receovery agent
//----------------------------------------------------------------------------
-(AuthStatus*)ReSetPassPhrase:(NSString*)domainID oldPassPhrase:(NSString*)oldPassPhrase passPhrase:(NSString*)passPhrase withRAName:(NSString*)raName andPublicKey:(NSString*)pubKey
{
	struct soap *pSoap = lockSimiasSoap(soapData);
	
	struct _ns1__ReSetPassPhrase           resetPPInput;
	struct _ns1__ReSetPassPhraseResponse   resetPPResponse;
	
	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (oldPassPhrase != nil), @"oldPassPhrase was nil");
	NSAssert( (passPhrase != nil), @"passPhrase was nil");
	NSAssert( (raName != nil), @"raName was nil");
	NSAssert( (pubKey != nil), @"pubKey was nil");
	
	resetPPInput.DomainID = (char*)[domainID UTF8String];
	resetPPInput.OldPassPhrase = (char*)[oldPassPhrase UTF8String];
	resetPPInput.PassPhrase = (char*)[passPhrase UTF8String];
	resetPPInput.RAName = (char*)[raName UTF8String];
	resetPPInput.RAPublicKey = (char*)[pubKey UTF8String];
	
	int err_code = soap_call___ns1__ReSetPassPhrase(
				 pSoap,
				 [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
				 NULL,
				 &resetPPInput,
				 &resetPPResponse);
	
	handle_simias_soap_error(soapData,@"SimiasService.ReSetPassPhrase");
	
	AuthStatus *authStatus = nil;
	struct ns1__Status *status = resetPPResponse.ReSetPassPhraseResult;
	
	if(status == NULL)
	{
		unlockSimiasSoap(soapData);
		[NSException raise:@"ReSetPassPhrase returned null object"
					format:@"Error in Reseting Passphrase"];
	}
	
	authStatus = [[[AuthStatus alloc] init] autorelease];
	[authStatus setProperties:getAuthStatus(status)];
	
	unlockSimiasSoap(soapData);
	
	return authStatus;
}


//----------------------------------------------------------------------------
// GetDefaultDomainID
// Retruns the default domain ID set by Simias
//----------------------------------------------------------------------------
-(NSString*) GetDefaultDomainID
{
	struct soap *pSoap = lockSimiasSoap(soapData);
	
	struct _ns1__GetDefaultDomainID           ddIDInput;
	struct _ns1__GetDefaultDomainIDResponse   ddIDResponse;
	
	int err_code = soap_call___ns1__GetDefaultDomainID(
				pSoap,
				[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
				NULL,
				&ddIDInput,
				&ddIDResponse);
	
	handle_simias_soap_error(soapData,@"SimiasService.GetDefaultDomainID");
	
	NSString *domainID = nil;
	@try
	{
		if(ddIDResponse.GetDefaultDomainIDResult != nil)
			domainID = [NSString stringWithUTF8String:ddIDResponse.GetDefaultDomainIDResult];
	}
	@catch(NSException *ex)
	{
		ifexconlog(@"SimiasService.GetDefaultDomainID",ex);
	}

	unlockSimiasSoap(soapData);
	
	return domainID;
}

//----------------------------------------------------------------------------
// GetDefaultiFolder
// Returns the default ifolder path for a particular domain depending on domain
// ID if already set
//----------------------------------------------------------------------------
-(NSString*) GetDefaultiFolder:(NSString*)domainID
{
	NSString* defaultiFolder = nil;
	struct soap *pSoap = lockSimiasSoap(soapData);
	
	NSAssert( (domainID != nil), @"domainID was nil");
	
	struct _ns1__GetDefaultiFolder         defaultiFolderInput;
	struct _ns1__GetDefaultiFolderResponse defaultiFolderResponse;
	
	defaultiFolderInput.DomainID = (char*)[domainID UTF8String];
	
	int err_code = soap_call___ns1__GetDefaultiFolder(
				   pSoap,
				   [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
				   NULL,
				   &defaultiFolderInput,
				   &defaultiFolderResponse);
	
	handle_simias_soap_error(soapData,@"SimiasService.GetDefaultDomainID");
	
	@try
	{
		if(defaultiFolderResponse.GetDefaultiFolderResult != nil)
			defaultiFolder = [NSString stringWithUTF8String:defaultiFolderResponse.GetDefaultiFolderResult];
	}
	@catch(NSException *ex)
	{
		ifexconlog(@"SimiasService.GetDefaultiFolder",ex);
	}
	
	unlockSimiasSoap(soapData);
	
	return defaultiFolder;
}

//----------------------------------------------------------------------------
// DefaultAccountInDomainID:foriFolderID:
// After creating the default iFolder upload, this has to be called to set
// default ifolder for a domain. Else it is treated as normal formal by that
// domain
//----------------------------------------------------------------------------
-(BOOL) DefaultAccountInDomainID:(NSString*)domainID foriFolderID:(NSString*)ifolderID
{
	BOOL setDefaultiFolder;
	struct soap *pSoap = lockSimiasSoap(soapData);
	
	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	
	struct _ns1__DefaultAccount          defaultAccountInput;
	struct _ns1__DefaultAccountResponse  defaultAccountResponse;
	
	defaultAccountInput.DomainID = (char*)[domainID UTF8String];
	defaultAccountInput.iFolderID = (char*)[ifolderID UTF8String];
	
	int err_code = soap_call___ns1__DefaultAccount(
				   pSoap,
				   [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
				   NULL,
				   &defaultAccountInput,
				   &defaultAccountResponse);
	
	setDefaultiFolder = (BOOL)defaultAccountResponse.DefaultAccountResult;
	unlockSimiasSoap(soapData);
	
	return setDefaultiFolder;
}


//----------------------------------------------------------------------------
// RemoveCertFromTable
// Removes the certificate from the table when user has moved. This is called 
// when user is loggin in for secnd time or more.
//----------------------------------------------------------------------------
-(void) RemoveCertFromTable:(NSString*)hostURL
{
	struct soap *pSoap = lockSimiasSoap(soapData);
	
	NSAssert( (hostURL != nil), @"hostURL was nil");
	
	struct _ns1__RemoveCertFromTable           removeCertInput;
	struct _ns1__RemoveCertFromTableResponse   removeCertResponse;
	
	removeCertInput.host = (char*)[hostURL UTF8String];
	
	int err_code = soap_call___ns1__RemoveCertFromTable(
					pSoap,
					[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
					NULL,
					&removeCertInput,
					&removeCertResponse);
	
	handle_simias_soap_error(soapData,@"SimiasService.RemoveCertFromTable");
	
	unlockSimiasSoap(soapData);
}

//----------------------------------------------------------------------------
// ExportRecoverImport
// Exports key's, recover's new keys and import them
//----------------------------------------------------------------------------
-(void) ExportRecoverImport:(NSString*)domainID forUser:(NSString*)userID withPassphrase:(NSString*)newPP
{
	struct soap *pSoap = lockSimiasSoap(soapData);
	
	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (userID != nil), @"userID was nil");
	NSAssert( (newPP != nil), @"newPP was nil");
	
	struct _ns1__ExportRecoverImport           exportRecoverImportInput;
	struct _ns1__ExportRecoverImportResponse   exportRecoverImportResponse;
	
	exportRecoverImportInput.DomainID = (char*)[domainID UTF8String];
	exportRecoverImportInput.UserID = (char*)[userID UTF8String];
	exportRecoverImportInput.NewPassphrase = (char*)[newPP UTF8String];
	
	NSLog(@"Before export recover import in simiasservice");
	int err_code = soap_call___ns1__ExportRecoverImport(
														pSoap,
														[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
														NULL,
														&exportRecoverImportInput,
														&exportRecoverImportResponse);
	NSLog(@"after export recover import in simiasservice");	
	handle_simias_soap_error(soapData,@"SimiasService.ExportRecoverImport");
	
	unlockSimiasSoap(soapData);
}



//----------------------------------------------------------------------------
// GetDefaultPublicKey
// Gets the default public key for a domain
//----------------------------------------------------------------------------
-(NSString*) GetDefaultPublicKey:(NSString*)domainID
{
	NSString *publicKey = nil;

	struct soap *pSoap = lockSimiasSoap(soapData);
	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__GetDefaultPublicKey          getDefaultPublicKeyInput;
	struct _ns1__GetDefaultPublicKeyResponse  getDefaultPublicKeyResponse;
	
	getDefaultPublicKeyInput.DomainID = (char*)[domainID UTF8String];
	
	int err_code = soap_call___ns1__GetDefaultPublicKey(
														pSoap,
														[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
														NULL,
														&getDefaultPublicKeyInput,
														&getDefaultPublicKeyResponse);
	
	handle_simias_soap_error(soapData,@"SimiasService.GetDefaultPublicKey");
	
	@try
	{
		if(getDefaultPublicKeyResponse.GetDefaultPublicKeyResult != nil)
			publicKey = [NSString stringWithUTF8String:getDefaultPublicKeyResponse.GetDefaultPublicKeyResult];
	}
	@catch(NSException *ex)
	{
		ifexconlog(@"SimiasService.GetDefaultPublicKey",ex);
	}
	
	unlockSimiasSoap(soapData);
	
	return publicKey;
}


//----------------------------------------------------------------------------
// getDomainProperties
// Prepares the properties to be store in the Domain object
//----------------------------------------------------------------------------
NSDictionary *getDomainProperties(struct ns1__DomainInformation *domainInfo)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];
	
	// Setup properties from the domainWeb object
	if(domainInfo->ID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:domainInfo->ID] forKey:@"ID"];
	if(domainInfo->POBoxID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:domainInfo->POBoxID] forKey:@"poboxID"];
	if(domainInfo->Name != nil)
		[newProperties setObject:[NSString stringWithUTF8String:domainInfo->Name] forKey:@"name"];
	if(domainInfo->Description != nil)
		[newProperties setObject:[NSString stringWithUTF8String:domainInfo->Description] forKey:@"description"];
	if(domainInfo->Host != nil)
		[newProperties setObject:[NSString stringWithUTF8String:domainInfo->Host] forKey:@"host"];
	if(domainInfo->HostUrl != nil)
		[newProperties setObject:[NSString stringWithUTF8String:domainInfo->HostUrl] forKey:@"hostURL"];
	if(domainInfo->RemoteUrl != nil)
		[newProperties setObject:[NSString stringWithUTF8String:domainInfo->RemoteUrl] forKey:@"remoteURL"];
	if(domainInfo->MemberUserID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:domainInfo->MemberUserID] forKey:@"userID"];
	if(domainInfo->MemberName != nil)
		[newProperties setObject:[NSString stringWithUTF8String:domainInfo->MemberName] forKey:@"userName"];
	[newProperties setObject:[NSNumber numberWithBool:domainInfo->IsDefault] forKey:@"isDefault"];
	[newProperties setObject:[NSNumber numberWithBool:domainInfo->IsSlave] forKey:@"isSlave"];
	[newProperties setObject:[NSNumber numberWithBool:domainInfo->Active] forKey:@"isEnabled"];
	[newProperties setObject:[NSNumber numberWithBool:domainInfo->Authenticated] forKey:@"authenticated"];
	[newProperties setObject:[NSNumber numberWithUnsignedInt:domainInfo->StatusCode] forKey:@"statusCode"];
	[newProperties setObject:[NSNumber numberWithInt:domainInfo->RemainingGraceLogins] forKey:@"remainingGraceLogins"];
	[newProperties setObject:[NSNumber numberWithUnsignedInt:domainInfo->Type] forKey:@"type"];
	
	return newProperties;
}




//----------------------------------------------------------------------------
// getAuthStatus
// Prepares the properties to be store in the AuthStatus object
//----------------------------------------------------------------------------
NSDictionary *getAuthStatus(struct ns1__Status *status)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];
	// Setup properties from the domainWeb object
	[newProperties setObject:[NSNumber numberWithUnsignedInt:status->statusCode] forKey:@"statusCode"];
	
	if(status->DomainID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:status->DomainID] forKey:@"domainID"];
	if(status->UserID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:status->UserID] forKey:@"userID"];
	if(status->UserName != nil)
		[newProperties setObject:[NSString stringWithUTF8String:status->UserName] forKey:@"userName"];
	if(status->DistinguishedUserName != nil)
		[newProperties setObject:[NSString stringWithUTF8String:status->DistinguishedUserName] forKey:@"distinguishedUserName"];
	if(status->ExceptionMessage != nil)
		[newProperties setObject:[NSString stringWithUTF8String:status->ExceptionMessage] forKey:@"exceptionMessage"];
	
	[newProperties setObject:[NSNumber numberWithInt:status->TotalGraceLogins] forKey:@"totalGraceLogins"];
	[newProperties setObject:[NSNumber numberWithInt:status->RemainingGraceLogins] forKey:@"remainingGraceLogins"];
	[newProperties setObject:[NSNumber numberWithInt:status->DaysUntilPasswordExpires] forKey:@"daysUntilPasswordExpires"];
	
	return newProperties;
}



//----------------------------------------------------------------------------
// handle_simias_soap_error
// This will check the soap structure for any errors and throw an appropriate
// exception based on that error
//----------------------------------------------------------------------------
void handle_simias_soap_error(void *soapData, NSString *methodName)
{
	SOAP_DATA *pSoapStruct = (SOAP_DATA *)soapData;
	struct soap *pSoap = pSoapStruct->soap;
	
	int error = pSoap->error;
 	if(error)
	{
		if(soap_soap_error_check(error))
		{
			if( (pSoap->fault != NULL) && (pSoap->fault->faultstring != NULL) )
			{
				NSString *faultString = [NSString stringWithUTF8String:pSoap->fault->faultstring];
				unlockSimiasSoap(soapData);
				[NSException raise:[NSString stringWithFormat:@"%@", faultString]
						format:@"Exception in %@", methodName];
			}
			else
			{
				unlockSimiasSoap(soapData);
				[NSException raise:[NSString stringWithFormat:@"SOAP Error %d", error]
						format:@"SOAP error in %@", methodName];
			}
		}
		else if(soap_http_error_check(error))
		{
			unlockSimiasSoap(soapData);
			[NSException raise:[NSString stringWithFormat:@"HTTP Error %d", error]
					format:@"HTTP error in %@", methodName];
		}
		else if(soap_tcp_error_check(error))
		{
			unlockSimiasSoap(soapData);
			[NSException raise:[NSString stringWithFormat:@"TCP Error %d", error]
					format:@"TCP error in %@", methodName];
		}
		else if(soap_ssl_error_check(error))
		{
			unlockSimiasSoap(soapData);
			[NSException raise:[NSString stringWithFormat:@"SSL Error %d", error]
					format:@"SSL error in %@", methodName];
		}
		else if(soap_xml_error_check(error))
		{
			unlockSimiasSoap(soapData);
			[NSException raise:[NSString stringWithFormat:@"XML Error %d", error]
					format:@"XML error in %@", methodName];
		}
		else
		{
			unlockSimiasSoap(soapData);
			[NSException raise:[NSString stringWithFormat:@"Error %d", error]
					format:@"Error in %@", methodName];
		}
	}
}


//----------------------------------------------------------------------------
// lockSimiasSoap
// Lock soap to prevent access from simias
//----------------------------------------------------------------------------
struct soap *lockSimiasSoap(void *soapData)
{
	SOAP_DATA *pSoap = (SOAP_DATA *)soapData;

	[pSoap->instanceLock lock];
	
	return pSoap->soap;
}

//----------------------------------------------------------------------------
// unlockSimiasSoap
// Unlock soap to access by simias
//----------------------------------------------------------------------------
void unlockSimiasSoap(void *soapData)
{
	SOAP_DATA *pSoap = (SOAP_DATA *)soapData;

	soap_end(pSoap->soap);

	[pSoap->instanceLock unlock];
}

//----------------------------------------------------------------------------
// InitMemberSearchResults
// Initialize the searach for members
//----------------------------------------------------------------------------
-(MemberSearchResults *) InitMemberSearchResults
{
	MemberSearchResults *searchResults = [[MemberSearchResults alloc] initWithSoapData:soapData];
	return searchResults;
}

//----------------------------------------------------------------------------
// readCredentials
// Read the credentials and store in current soap's credentials
//----------------------------------------------------------------------------
-(void)readCredentials
{
	SOAP_DATA *pSoap = (SOAP_DATA *)soapData;
	
	if( (pSoap->username != NULL) && (pSoap->password != NULL) && (pSoap->soap != NULL) )
	{
		if(simias_get_web_service_credential(pSoap->username, pSoap->password) == 0)
		{
			pSoap->soap->userid = pSoap->username;
			pSoap->soap->passwd = pSoap->password;
		}
	}
	if(simiasURL != nil)
		[simiasURL release];
	
	simiasURL = [[NSString stringWithFormat:@"%@/Simias.asmx", [[Simias getInstance] simiasURL]] retain];
}


@end
