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
#include "simiasStub.h"
#include "simias.nsmap"

@implementation SimiasService


void init_simias_gsoap(struct soap *pSoap);
void cleanup_simias_gsoap(struct soap *pSoap);
NSDictionary *getDomainProperties(struct ns1__DomainInformation *domainInfo);


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
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &getDomainsMessage,
            &getDomainsResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"iFolderService.GetDomains"];
	}
	else
	{
		domains = [[[NSMutableArray alloc]
				initWithCapacity:getDomainsResponse.GetDomainsResult->__sizeDomainInformation] autorelease];
		int counter;
		for(counter=0;counter<getDomainsResponse.GetDomainsResult->__sizeDomainInformation;counter++)
		{
			struct ns1__DomainInformation *curDomain;
			
			curDomain = getDomainsResponse.GetDomainsResult->DomainInformation[counter];
			iFolderDomain *newDomain = [[[iFolderDomain alloc] init] autorelease];

			[newDomain setProperties:getDomainProperties(curDomain)];
			
			[domains addObject:newDomain];
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
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
		domain = [[[iFolderDomain alloc] init] autorelease];
		
		struct ns1__DomainInformation *curDomain;
			
		curDomain = connectToDomainResponse.ConnectToDomainResult;
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
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
// SaveDomainPassword
// Saves the password in the store
//----------------------------------------------------------------------------
-(void)SaveDomainPassword:(NSString *)domainID password:(NSString *)password
{
    struct soap soap;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SaveDomainCredentials			saveDomainCredsMessage;
	struct _ns1__SaveDomainCredentialsResponse	saveDomainCredsResponse;

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

    err_code = soap_call___ns1__SaveDomainCredentials(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
// GetSavedDomainPassword
// gets the saved password from the store
//----------------------------------------------------------------------------
-(NSString *)GetSavedDomainPassword:(NSString *)domainID
{
	NSString *password = nil;
    struct soap soap;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__GetSavedDomainCredentials			getDomainCredsMessage;
	struct _ns1__GetSavedDomainCredentialsResponse	getDomainCredsResponse;

	getDomainCredsMessage.domainID = (char *)[domainID cString];

    init_simias_gsoap (&soap);

    err_code = soap_call___ns1__GetSavedDomainCredentials(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
		if(getDomainCredsResponse.GetSavedDomainCredentialsResult == ns1__CredentialType__Basic)
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
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
