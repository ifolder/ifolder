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

#import "SimiasService.h"
#include <simiasStub.h>
#include <simias.nsmap>
#import "Simias.h"
#import "AuthStatus.h"


@implementation SimiasService

void init_simias_gsoap(struct soap *pSoap, GSOAP_CREDS *creds);
void cleanup_simias_gsoap(struct soap *pSoap, GSOAP_CREDS *creds);
void handle_simias_soap_error(struct soap *pSoap, GSOAP_CREDS *creds, NSString *methodName);
NSDictionary *getDomainProperties(struct ns1__DomainInformation *domainInfo);
NSDictionary *getAuthStatus(struct ns1__Status *status);

- (id)init 
{
	[super init];
	simiasURL = [[NSString stringWithFormat:@"%@/simias10/Simias.asmx", [[Simias getInstance] simiasURL]] retain];
	NSLog(@"Initialized SimiasService on URL: %@", simiasURL);
    return self;
}
-(void)dealloc
{
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
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

    struct _ns1__ValidCredentials ns1__validCreds;
    struct _ns1__ValidCredentialsResponse ns1__validCredsResponse;

    init_simias_gsoap (&soap, &creds);
    err_code = soap_call___ns1__ValidCredentials (&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &ns1__validCreds,
            &ns1__validCredsResponse);

    if (err_code == SOAP_OK)
    {
		validCreds = (BOOL) ns1__validCredsResponse.ValidCredentialsResult;
    }

    cleanup_simias_gsoap(&soap, &creds);

	return validCreds;
}




//----------------------------------------------------------------------------
// GetDomains
// Reads domains from store
//----------------------------------------------------------------------------
-(NSArray *) GetDomains:(BOOL)onlySlaves
{
	NSMutableArray *domains = nil;
	
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	struct _ns1__GetDomains getDomainsMessage;
	struct _ns1__GetDomainsResponse getDomainsResponse;

	getDomainsMessage.onlySlaves = onlySlaves;

    init_simias_gsoap (&soap, &creds);
    err_code = soap_call___ns1__GetDomains(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getDomainsMessage,
            &getDomainsResponse);

	// This will free the soap structure and creds and throw an exception if there is an error
	handle_simias_soap_error(&soap, &creds, @"SimiasService.GetDomains");

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

    cleanup_simias_gsoap(&soap, &creds);

	return domains;
}




//----------------------------------------------------------------------------
// ConnectToDomain
// "Joins" a new domain
//----------------------------------------------------------------------------
-(iFolderDomain *) ConnectToDomain:(NSString *)UserName usingPassword:(NSString *)Password andHost:(NSString *)Host
{
	iFolderDomain *domain = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (UserName != nil), @"UserName was nil");
	NSAssert( (Password != nil), @"Password was nil");
	NSAssert( (Host != nil), @"Host was nil");

	struct _ns1__ConnectToDomain connectToDomainMessage;
	struct _ns1__ConnectToDomainResponse connectToDomainResponse;
	
	connectToDomainMessage.UserName = (char *)[UserName UTF8String];
	connectToDomainMessage.Password = (char *)[Password UTF8String];
	connectToDomainMessage.Host = (char *)[Host UTF8String];

    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__ConnectToDomain(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &connectToDomainMessage,
            &connectToDomainResponse);



 	if(soap.error)
	{
		if( (soap_soap_error_check(soap.error)) && 
			(soap.fault != NULL) && 
			(soap.fault->faultstring != NULL) &&
			(strstr(soap.fault->faultstring, "Domain") == soap.fault->faultstring) &&
			(strstr(soap.fault->faultstring, "already exists") != NULL) )
		{
			NSString *faultString = [NSString stringWithUTF8String:soap.fault->faultstring];
			cleanup_simias_gsoap(&soap, &creds);
			[NSException raise:[NSString stringWithFormat:@"%@", faultString]
					format:@"DomainExistsError"];
		}
		else
		{
			// This will free the soap structure and creds and throw an exception if there is an error
			handle_simias_soap_error(&soap, &creds, @"SimiasService.ConnectToDomain");
		}
	}
	else
	{
		struct ns1__DomainInformation *curDomain;
		curDomain = connectToDomainResponse.ConnectToDomainResult;
		if(curDomain == NULL)
		{
			cleanup_simias_gsoap(&soap, &creds);
			[NSException raise:@"Unable to connect to domain"
							format:@"Error in ConnectToDomain"];		
		}

		domain = [[[iFolderDomain alloc] init] autorelease];
		[domain setProperties:getDomainProperties(curDomain)];
    }

    cleanup_simias_gsoap(&soap, &creds);

	return domain;
}



//----------------------------------------------------------------------------
// LeaveDomain
// Leaves the domain specified and if specified, leaves it from all machines
//----------------------------------------------------------------------------
-(void) LeaveDomain:(NSString *)domainID withOption:(BOOL)localOnly
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__LeaveDomain			leaveDomainMessage;
	struct _ns1__LeaveDomainResponse	leaveDomainResponse;

	leaveDomainMessage.DomainID = (char *)[domainID UTF8String];
	leaveDomainMessage.LocalOnly = localOnly;

    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__LeaveDomain(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &leaveDomainMessage,
            &leaveDomainResponse);

	handle_simias_soap_error(&soap, &creds, @"SimiasService.LeaveDomain");

    cleanup_simias_gsoap(&soap, &creds);
}




//----------------------------------------------------------------------------
// SetDomainPassword
// Saves the password in the store
//----------------------------------------------------------------------------
-(void)SetDomainPassword:(NSString *)domainID password:(NSString *)password
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

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

    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__SetDomainCredentials(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &saveDomainCredsMessage,
            &saveDomainCredsResponse);

	handle_simias_soap_error(&soap, &creds, @"SimiasService.SetDomainPassword");

    cleanup_simias_gsoap(&soap, &creds);
}




//----------------------------------------------------------------------------
// GetDomainPassword
// gets the saved password from the store
//----------------------------------------------------------------------------
-(NSString *)GetDomainPassword:(NSString *)domainID
{
	NSString *password = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__GetDomainCredentials			getDomainCredsMessage;
	struct _ns1__GetDomainCredentialsResponse	getDomainCredsResponse;

	getDomainCredsMessage.domainID = (char *)[domainID UTF8String];

    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__GetDomainCredentials(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getDomainCredsMessage,
            &getDomainCredsResponse);

	handle_simias_soap_error(&soap, &creds, @"SimiasService.GetDomainPassword");

	if(getDomainCredsResponse.GetDomainCredentialsResult == ns1__CredentialType__Basic)
	{
		password = [NSString stringWithUTF8String:getDomainCredsResponse.credentials];
	}

    cleanup_simias_gsoap(&soap, &creds);

	return password;
}




//----------------------------------------------------------------------------
// GetSavedDomainPassword
// gets the saved password from the store
//----------------------------------------------------------------------------
-(void)SetDomainActive:(NSString *)domainID
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SetDomainActive			setDomainActiveMessage;
	struct _ns1__SetDomainActiveResponse	setDomainActiveResponse;

	setDomainActiveMessage.domainID = (char *)[domainID UTF8String];

    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__SetDomainActive(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setDomainActiveMessage,
            &setDomainActiveResponse);

	handle_simias_soap_error(&soap, &creds, @"SimiasService.SetDomainActive");

    cleanup_simias_gsoap(&soap, &creds);
}




//----------------------------------------------------------------------------
// GetSavedDomainPassword
// gets the saved password from the store
//----------------------------------------------------------------------------
-(void)SetDomainInactive:(NSString *)domainID
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SetDomainInactive			setDomainInactiveMessage;
	struct _ns1__SetDomainInactiveResponse	setDomainInactiveResponse;

	setDomainInactiveMessage.domainID = (char *)[domainID UTF8String];

    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__SetDomainInactive(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setDomainInactiveMessage,
            &setDomainInactiveResponse);

	handle_simias_soap_error(&soap, &creds, @"SimiasService.SetDomainInactive");

    cleanup_simias_gsoap(&soap, &creds);
}




//----------------------------------------------------------------------------
// SetDefaultDomain
// Set the specified domainID as the default domain
//----------------------------------------------------------------------------
-(void) SetDefaultDomain:(NSString *)domainID
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SetDefaultDomain			setDefaultDomainMessage;
	struct _ns1__SetDefaultDomainResponse	setDefaultDomainResponse;

	setDefaultDomainMessage.domainID = (char *)[domainID UTF8String];

    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__SetDefaultDomain(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setDefaultDomainMessage,
            &setDefaultDomainResponse);

	handle_simias_soap_error(&soap, &creds, @"SimiasService.SetDefaultDomain");

    cleanup_simias_gsoap(&soap, &creds);
}




//----------------------------------------------------------------------------
// LoginToRemoteDomain
// Provide the credentials to authenticate to a domain
//----------------------------------------------------------------------------
-(AuthStatus *) LoginToRemoteDomain:(NSString *)domainID usingPassword:(NSString *)password
{
	AuthStatus *authStatus = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (password != nil), @"password was nil");

	struct _ns1__LoginToRemoteDomain			loginToDomainMessage;
	struct _ns1__LoginToRemoteDomainResponse	loginToDomainResponse;
	
	loginToDomainMessage.domainID = (char *)[domainID UTF8String];
	loginToDomainMessage.password = (char *)[password UTF8String];

    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__LoginToRemoteDomain(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &loginToDomainMessage,
            &loginToDomainResponse);

	handle_simias_soap_error(&soap, &creds, @"SimiasService.LoginToRemoteDomain");

	struct ns1__Status *status = loginToDomainResponse.LoginToRemoteDomainResult;
	if(status == NULL)
	{
		cleanup_simias_gsoap(&soap, &creds);
		[NSException raise:@"Authentication returned null object"
						format:@"Error in AuthenticateToDomain"];		
	}

	authStatus = [[[AuthStatus alloc] init] autorelease];
	[authStatus setProperties:getAuthStatus(status)];

    cleanup_simias_gsoap(&soap, &creds);

	return authStatus;
}




//----------------------------------------------------------------------------
// GetCertificate
// Gets the certificate for a specified host
//----------------------------------------------------------------------------
-(SecCertificateRef) GetCertificate:(NSString *)host
{
	SecCertificateRef certRef = NULL;

    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;
	CSSM_DATA certData;

	NSAssert( (host != nil), @"host was nil");

	struct _ns1__GetCertificate			getCertMessage;
	struct _ns1__GetCertificateResponse	getCertResponse;
	
	getCertMessage.host = (char *)[host UTF8String];

    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__GetCertificate(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getCertMessage,
            &getCertResponse);

	handle_simias_soap_error(&soap, &creds, @"SimiasService.GetCertificate");

	struct xsd__base64Binary *certResult = getCertResponse.GetCertificateResult;
	if(certResult == NULL)
	{
		cleanup_simias_gsoap(&soap, &creds);
		[NSException raise:@"GetCertificate returned null object"
						format:@"Error in GetCertificate"];		
	}

	certData.Data = certResult->__ptr;
	certData.Length = certResult->__size;

	OSStatus status =  SecCertificateCreateFromData ( &certData,
											CSSM_CERT_X_509v3,
											CSSM_CERT_ENCODING_BER,
											&certRef);

    cleanup_simias_gsoap(&soap, &creds);

	return certRef;
}



//----------------------------------------------------------------------------
// StoreCertificate
// Stores the certificate so it can be used when users login to the host
//----------------------------------------------------------------------------
-(void) StoreCertificate:(SecCertificateRef)cert forHost:(NSString *)host
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

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

		init_simias_gsoap (&soap, &creds);
		err_code = soap_call___ns1__StoreCertificate(
				&soap,
				[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
				NULL,
				&storeCertMessage,
				&storeCertResponse);

		handle_simias_soap_error(&soap, &creds, @"SimiasService.StoreCertificate");

		cleanup_simias_gsoap(&soap, &creds);
	}
}




//----------------------------------------------------------------------------
// DisableDomainAutoLogin
// Disables Simias from asking for password
//----------------------------------------------------------------------------
-(void) DisableDomainAutoLogin:(NSString *)domainID
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__DisableDomainAutoLogin			disableDomainMessage;
	struct _ns1__DisableDomainAutoLoginResponse	disableDomainResponse;

	disableDomainMessage.domainID = (char *)[domainID UTF8String];

    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__DisableDomainAutoLogin(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &disableDomainMessage,
            &disableDomainResponse);

	handle_simias_soap_error(&soap, &creds, @"SimiasService.DisableDomainAutoLogin");

    cleanup_simias_gsoap(&soap, &creds);
}




//----------------------------------------------------------------------------
// LogoutFromRemoteDomain
// Will cause simias to no longer talk to this domain (logout)
//----------------------------------------------------------------------------
-(AuthStatus *) LogoutFromRemoteDomain:(NSString *)domainID
{
	AuthStatus *authStatus = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__LogoutFromRemoteDomain			logoutDomainMessage;
	struct _ns1__LogoutFromRemoteDomainResponse	logoutDomainResponse;

	logoutDomainMessage.domainID = (char *)[domainID UTF8String];

    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__LogoutFromRemoteDomain(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &logoutDomainMessage,
            &logoutDomainResponse);

	handle_simias_soap_error(&soap, &creds, @"SimiasService.LogoutFromRemoteDomain");

	struct ns1__Status *status = logoutDomainResponse.LogoutFromRemoteDomainResult;
	if(status == NULL)
	{
		cleanup_simias_gsoap(&soap, &creds);
		[NSException raise:@"Authentication returned null object"
						format:@"Error in AuthenticateToDomain"];		
	}

	authStatus = [[[AuthStatus alloc] init] autorelease];
	[authStatus setProperties:getAuthStatus(status)];


    cleanup_simias_gsoap(&soap, &creds);

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
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;
	BOOL setProxyResult = NO;

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

    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__SetProxyAddress(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setProxyMessage,
            &setProxyResponse);

	handle_simias_soap_error(&soap, &creds, @"SimiasService.SetProxyAddress");

	setProxyResult = (BOOL) setProxyResponse.SetProxyAddressResult;

    cleanup_simias_gsoap(&soap, &creds);

	return setProxyResult;
}



//----------------------------------------------------------------------------
// SetDomainHostAddress
// Sets the Address for the specified domainID
//----------------------------------------------------------------------------
-(BOOL) SetDomainHostAddress:(NSString *)domainID
							NewHostAddress:(NSString *)hostAddress
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;
	BOOL setHostResult = NO;

	struct _ns1__SetDomainHostAddress			setAddrMessage;
	struct _ns1__SetDomainHostAddressResponse	setAddrResponse;

	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (hostAddress != nil), @"hostAddress was nil");

	setAddrMessage.domainID = (char *)[domainID UTF8String];
	setAddrMessage.hostAddress = (char *)[hostAddress UTF8String];
	
    init_simias_gsoap (&soap, &creds);

    err_code = soap_call___ns1__SetDomainHostAddress(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setAddrMessage,
            &setAddrResponse);

	handle_simias_soap_error(&soap, &creds, @"SimiasService.SetDomainHostAddress");

	setHostResult = (BOOL) setAddrResponse.SetDomainHostAddressResult;

    cleanup_simias_gsoap(&soap, &creds);

	return setHostResult;
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

	return newProperties;
}


//----------------------------------------------------------------------------
// handle_simias_soap_error
// This will check the soap structure for any errors and throw an appropriate
// exception based on that error
//----------------------------------------------------------------------------
void handle_simias_soap_error(struct soap *pSoap, GSOAP_CREDS *creds, NSString *methodName)
{
	int error = pSoap->error;
	
 	if(error)
	{
		if(soap_soap_error_check(error))
		{
			if( (pSoap->fault != NULL) && (pSoap->fault->faultstring != NULL) )
			{
				NSString *faultString = [NSString stringWithUTF8String:pSoap->fault->faultstring];
				cleanup_simias_gsoap(pSoap, creds);
				[NSException raise:[NSString stringWithFormat:@"%@", faultString]
						format:@"Exception in %@", methodName];
			}
			else
			{
				cleanup_simias_gsoap(pSoap, creds);
				[NSException raise:[NSString stringWithFormat:@"SOAP Error %d", error]
						format:@"SOAP error in %@", methodName];
			}
		}
		else if(soap_http_error_check(error))
		{
			cleanup_simias_gsoap(pSoap, creds);
			[NSException raise:[NSString stringWithFormat:@"HTTP Error %d", error]
					format:@"HTTP error in %@", methodName];
		}
		else if(soap_tcp_error_check(error))
		{
			cleanup_simias_gsoap(pSoap, creds);
			[NSException raise:[NSString stringWithFormat:@"TCP Error %d", error]
					format:@"TCP error in %@", methodName];
		}
		else if(soap_ssl_error_check(error))
		{
			cleanup_simias_gsoap(pSoap, creds);
			[NSException raise:[NSString stringWithFormat:@"SSL Error %d", error]
					format:@"SSL error in %@", methodName];
		}
		else if(soap_xml_error_check(error))
		{
			cleanup_simias_gsoap(pSoap, creds);
			[NSException raise:[NSString stringWithFormat:@"XML Error %d", error]
					format:@"XML error in %@", methodName];
		}
		else
		{
			cleanup_simias_gsoap(pSoap, creds);
			[NSException raise:[NSString stringWithFormat:@"Error %d", error]
					format:@"Error in %@", methodName];
		}
	}
}




void init_simias_gsoap(struct soap *pSoap, GSOAP_CREDS *creds)
{
	soap_init2(pSoap, (SOAP_C_UTFSTRING | SOAP_IO_DEFAULT), (SOAP_C_UTFSTRING | SOAP_IO_DEFAULT));
//	soap_init2(pSoap, SOAP_C_UTFSTRING, SOAP_C_UTFSTRING);
//	soap_init(pSoap);
	soap_set_namespaces(pSoap, simias_namespaces);

	creds->username = malloc(1024);
	if(creds->username != NULL)
	{
		memset(creds->username, 0, 1024);
	}
	creds->password = malloc(1024);
	if(creds->password != NULL)
	{
		memset(creds->password, 0, 1024);
	}

	if( (creds->username != NULL) && (creds->password != NULL) )
	{
		if(simias_get_web_service_credential(creds->username, creds->password) == 0)
		{
			pSoap->userid = creds->username;
			pSoap->passwd = creds->password;
		}
	}

	// Set the timeout for send and receive to 30 seconds
	pSoap->recv_timeout = 30;
	pSoap->send_timeout = 30;
}




void cleanup_simias_gsoap(struct soap *pSoap, GSOAP_CREDS *creds)
{
	if(creds->username != NULL)
	{
		free(creds->username);
		creds->username = NULL;
	}
	if(creds->password != NULL)
	{
		free(creds->password);
		creds->password = NULL;
	}

	soap_end(pSoap);
}




@end