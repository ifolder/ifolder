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

void init_simias_gsoap(struct soap *pSoap);
void cleanup_simias_gsoap(struct soap *pSoap);
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
    int err_code;

    struct _ns1__ValidCredentials ns1__validCreds;
    struct _ns1__ValidCredentialsResponse ns1__validCredsResponse;

    init_simias_gsoap (&soap);
    err_code = soap_call___ns1__ValidCredentials (&soap,
            [simiasURL cString], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &ns1__validCreds,
            &ns1__validCredsResponse);

    if (err_code == SOAP_OK)
    {
		validCreds = (BOOL) ns1__validCredsResponse.ValidCredentialsResult;
    }

    cleanup_simias_gsoap(&soap);

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
    int err_code;

	struct _ns1__GetDomains getDomainsMessage;
	struct _ns1__GetDomainsResponse getDomainsResponse;

	getDomainsMessage.onlySlaves = onlySlaves;

    init_simias_gsoap (&soap);
    err_code = soap_call___ns1__GetDomains(
			&soap,
            [simiasURL cString], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getDomainsMessage,
            &getDomainsResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"SimiasService.GetDomains"];
	}
	else
	{
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
    }

    cleanup_simias_gsoap(&soap);

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
    int err_code;

	NSAssert( (UserName != nil), @"UserName was nil");
	NSAssert( (Password != nil), @"Password was nil");
	NSAssert( (Host != nil), @"Host was nil");

	struct _ns1__ConnectToDomain connectToDomainMessage;
	struct _ns1__ConnectToDomainResponse connectToDomainResponse;
	
	connectToDomainMessage.UserName = (char *)[UserName cString];
	connectToDomainMessage.Password = (char *)[Password cString];
	connectToDomainMessage.Host = (char *)[Host cString];

    init_simias_gsoap (&soap);

    err_code = soap_call___ns1__ConnectToDomain(
			&soap,
            [simiasURL cString], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &connectToDomainMessage,
            &connectToDomainResponse);
 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in ConnectToDomain"];
	}
	else
	{
		struct ns1__DomainInformation *curDomain;
		curDomain = connectToDomainResponse.ConnectToDomainResult;
		if(curDomain == NULL)
		{
			cleanup_simias_gsoap(&soap);
			[NSException raise:@"Unable to connect to domain"
							format:@"Error in ConnectToDomain"];		
		}

		domain = [[[iFolderDomain alloc] init] autorelease];
		[domain setProperties:getDomainProperties(curDomain)];
    }

    cleanup_simias_gsoap(&soap);

	return domain;
}



//----------------------------------------------------------------------------
// LeaveDomain
// Leaves the domain specified and if specified, leaves it from all machines
//----------------------------------------------------------------------------
-(void) LeaveDomain:(NSString *)domainID withOption:(BOOL)localOnly
{
    struct soap soap;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__LeaveDomain			leaveDomainMessage;
	struct _ns1__LeaveDomainResponse	leaveDomainResponse;

	leaveDomainMessage.DomainID = (char *)[domainID cString];
	leaveDomainMessage.LocalOnly = localOnly;

    init_simias_gsoap (&soap);

    err_code = soap_call___ns1__LeaveDomain(
			&soap,
            [simiasURL cString], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &leaveDomainMessage,
            &leaveDomainResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in LeaveDomain"];
	}

    cleanup_simias_gsoap(&soap);
}




//----------------------------------------------------------------------------
// SetDomainPassword
// Saves the password in the store
//----------------------------------------------------------------------------
-(void)SetDomainPassword:(NSString *)domainID password:(NSString *)password
{
    struct soap soap;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SetDomainCredentials			saveDomainCredsMessage;
	struct _ns1__SetDomainCredentialsResponse	saveDomainCredsResponse;

	if(password == nil)
	{
		saveDomainCredsMessage.domainID = (char *)[domainID cString];
		saveDomainCredsMessage.credentials = (char *)NULL;
		saveDomainCredsMessage.type = ns1__CredentialType__None;
	}
	else
	{
		saveDomainCredsMessage.domainID = (char *)[domainID cString];
		saveDomainCredsMessage.credentials = (char *)[password cString];
		saveDomainCredsMessage.type = ns1__CredentialType__Basic;
	}

    init_simias_gsoap (&soap);

    err_code = soap_call___ns1__SetDomainCredentials(
			&soap,
            [simiasURL cString], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &saveDomainCredsMessage,
            &saveDomainCredsResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in SaveDomainCredentials"];
	}

    cleanup_simias_gsoap(&soap);
}




//----------------------------------------------------------------------------
// GetDomainPassword
// gets the saved password from the store
//----------------------------------------------------------------------------
-(NSString *)GetDomainPassword:(NSString *)domainID
{
	NSString *password = nil;
    struct soap soap;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__GetDomainCredentials			getDomainCredsMessage;
	struct _ns1__GetDomainCredentialsResponse	getDomainCredsResponse;

	getDomainCredsMessage.domainID = (char *)[domainID cString];

    init_simias_gsoap (&soap);

    err_code = soap_call___ns1__GetDomainCredentials(
			&soap,
            [simiasURL cString], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getDomainCredsMessage,
            &getDomainCredsResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in SaveDomainCredentials"];
	}
	else
	{
		if(getDomainCredsResponse.GetDomainCredentialsResult == ns1__CredentialType__Basic)
		{
			password = [NSString stringWithCString:getDomainCredsResponse.credentials];
		}
	}

    cleanup_simias_gsoap(&soap);

	return password;
}




//----------------------------------------------------------------------------
// GetSavedDomainPassword
// gets the saved password from the store
//----------------------------------------------------------------------------
-(void)SetDomainActive:(NSString *)domainID
{
    struct soap soap;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SetDomainActive			setDomainActiveMessage;
	struct _ns1__SetDomainActiveResponse	setDomainActiveResponse;

	setDomainActiveMessage.domainID = (char *)[domainID cString];

    init_simias_gsoap (&soap);

    err_code = soap_call___ns1__SetDomainActive(
			&soap,
            [simiasURL cString], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setDomainActiveMessage,
            &setDomainActiveResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in SetDomainActive"];
	}

    cleanup_simias_gsoap(&soap);
}




//----------------------------------------------------------------------------
// GetSavedDomainPassword
// gets the saved password from the store
//----------------------------------------------------------------------------
-(void)SetDomainInactive:(NSString *)domainID
{
    struct soap soap;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SetDomainInactive			setDomainInactiveMessage;
	struct _ns1__SetDomainInactiveResponse	setDomainInactiveResponse;

	setDomainInactiveMessage.domainID = (char *)[domainID cString];

    init_simias_gsoap (&soap);

    err_code = soap_call___ns1__SetDomainInactive(
			&soap,
            [simiasURL cString], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setDomainInactiveMessage,
            &setDomainInactiveResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in SetDomainInactive"];
	}

    cleanup_simias_gsoap(&soap);
}




//----------------------------------------------------------------------------
// SetDefaultDomain
// Set the specified domainID as the default domain
//----------------------------------------------------------------------------
-(void) SetDefaultDomain:(NSString *)domainID
{
    struct soap soap;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SetDefaultDomain			setDefaultDomainMessage;
	struct _ns1__SetDefaultDomainResponse	setDefaultDomainResponse;

	setDefaultDomainMessage.domainID = (char *)[domainID cString];

    init_simias_gsoap (&soap);

    err_code = soap_call___ns1__SetDefaultDomain(
			&soap,
            [simiasURL cString], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setDefaultDomainMessage,
            &setDefaultDomainResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in SetDefaultDomain"];
	}

    cleanup_simias_gsoap(&soap);
}




//----------------------------------------------------------------------------
// LoginToRemoteDomain
// Provide the credentials to authenticate to a domain
//----------------------------------------------------------------------------
-(AuthStatus *) LoginToRemoteDomain:(NSString *)domainID usingPassword:(NSString *)password
{
	AuthStatus *authStatus = nil;
    struct soap soap;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");
	NSAssert( (password != nil), @"password was nil");

	struct _ns1__LoginToRemoteDomain			loginToDomainMessage;
	struct _ns1__LoginToRemoteDomainResponse	loginToDomainResponse;
	
	loginToDomainMessage.domainID = (char *)[domainID cString];
	loginToDomainMessage.password = (char *)[password cString];

    init_simias_gsoap (&soap);
    err_code = soap_call___ns1__LoginToRemoteDomain(
			&soap,
            [simiasURL cString], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &loginToDomainMessage,
            &loginToDomainResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in AuthenticateToDomain"];
	}
	else
	{
		struct ns1__Status *status;
		status = loginToDomainResponse.LoginToRemoteDomainResult;
		if(status == NULL)
		{
			cleanup_simias_gsoap(&soap);
			[NSException raise:@"Authentication returned null object"
							format:@"Error in AuthenticateToDomain"];		
		}

		authStatus = [[[AuthStatus alloc] init] autorelease];
		[authStatus setProperties:getAuthStatus(status)];
	}

    cleanup_simias_gsoap(&soap);

	return authStatus;
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
		[newProperties setObject:[NSString stringWithCString:domainInfo->ID] forKey:@"ID"];
	if(domainInfo->POBoxID != nil)
		[newProperties setObject:[NSString stringWithCString:domainInfo->POBoxID] forKey:@"poboxID"];
	if(domainInfo->Name != nil)
		[newProperties setObject:[NSString stringWithCString:domainInfo->Name] forKey:@"name"];
	if(domainInfo->Description != nil)
		[newProperties setObject:[NSString stringWithCString:domainInfo->Description] forKey:@"description"];
	if(domainInfo->Host != nil)
		[newProperties setObject:[NSString stringWithCString:domainInfo->Host] forKey:@"host"];
	if(domainInfo->MemberUserID != nil)
		[newProperties setObject:[NSString stringWithCString:domainInfo->MemberUserID] forKey:@"userID"];
	if(domainInfo->MemberName != nil)
		[newProperties setObject:[NSString stringWithCString:domainInfo->MemberName] forKey:@"userName"];
	[newProperties setObject:[NSNumber numberWithBool:domainInfo->IsDefault] forKey:@"isDefault"];
	[newProperties setObject:[NSNumber numberWithBool:domainInfo->IsSlave] forKey:@"isSlave"];
	[newProperties setObject:[NSNumber numberWithBool:domainInfo->Active] forKey:@"isEnabled"];
	[newProperties setObject:[NSNumber numberWithBool:domainInfo->Authenticated] forKey:@"authenticated"];
	[newProperties setObject:[NSNumber numberWithUnsignedInt:domainInfo->StatusCode] forKey:@"statusCode"];

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
		[newProperties setObject:[NSString stringWithCString:status->DomainID] forKey:@"domainID"];
	if(status->UserID != nil)
		[newProperties setObject:[NSString stringWithCString:status->UserID] forKey:@"userID"];
	if(status->UserName != nil)
		[newProperties setObject:[NSString stringWithCString:status->UserName] forKey:@"userName"];
	if(status->DistinguishedUserName != nil)
		[newProperties setObject:[NSString stringWithCString:status->DistinguishedUserName] forKey:@"distinguishedUserName"];
	if(status->ExceptionMessage != nil)
		[newProperties setObject:[NSString stringWithCString:status->ExceptionMessage] forKey:@"exceptionMessage"];

	[newProperties setObject:[NSNumber numberWithInt:status->TotalGraceLogins] forKey:@"totalGraceLogins"];
	[newProperties setObject:[NSNumber numberWithInt:status->RemainingGraceLogins] forKey:@"remainingGraceLogins"];

	return newProperties;
}




void init_simias_gsoap(struct soap *pSoap)
{
	soap_init(pSoap);
	soap_set_namespaces(pSoap, simias_namespaces);
}




void cleanup_simias_gsoap(struct soap *pSoap)
{
	soap_end(pSoap);
}




@end
