//
//  iFolderService.m
//  iFolder
//
//  Created by Calvin Gaisford on 12/14/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

#import "iFolderService.h"
#include "iFolderStub.h"
#include "ifolder.nsmap"

@implementation iFolderService

void init_gsoap(struct soap *pSoap);
void cleanup_gsoap(struct soap *pSoap);


-(bool) Ping
{
    struct soap soap;
    bool isRunning = false;
    int err_code;

    struct _ns1__Ping ns1__Ping;
    struct _ns1__PingResponse ns1__PingResponse;

    init_gsoap (&soap);
    err_code = soap_call___ns1__Ping (&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &ns1__Ping,
            &ns1__PingResponse);

    if (err_code == SOAP_OK)
    {
        isRunning = true;
    }

    cleanup_gsoap(&soap);

    return isRunning;
}



-(NSMutableDictionary *) GetDomains
{
	NSMutableDictionary *domains = nil;
	
    struct soap soap;
    int err_code;

	struct _ns1__GetDomains getDomainsMessage;
	struct _ns1__GetDomainsResponse getDomainsResponse;

    init_gsoap (&soap);
    err_code = soap_call___ns1__GetDomains(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &getDomainsMessage,
            &getDomainsResponse);

    if (err_code == SOAP_OK)
    {
		domains = [[NSMutableDictionary alloc]
				initWithCapacity:getDomainsResponse.GetDomainsResult->__sizeDomainWeb];
		int counter;
		for(counter=0;counter<getDomainsResponse.GetDomainsResult->__sizeDomainWeb;counter++)
		{
			struct ns1__DomainWeb *curDomain;
			
			curDomain = getDomainsResponse.GetDomainsResult->DomainWeb[counter];
			IFDomain *newDomain = [[IFDomain alloc] init];
			
			[newDomain from_gsoap:curDomain];
			
			[domains setObject:newDomain forKey:newDomain->ID];
		}
    }
	else
	{
		[NSException raise:@"GetDomainsException" format:@"An error happened when calling GetDomains"];
	}

    cleanup_gsoap(&soap);

	return domains;
}


-(IFDomain *) ConnectToDomain:(NSString *)username usingPassword:(NSString *)password andHost:(NSString *)host
{
	IFDomain *domain = nil;
    struct soap soap;
    int err_code;

	NSAssert( (username != nil), @"username was nil");
	NSAssert( (password != nil), @"password was nil");
	NSAssert( (host != nil), @"host was nil");

	struct _ns1__ConnectToDomain connectToDomainMessage;
	struct _ns1__ConnectToDomainResponse connectToDomainResponse;
	
	connectToDomainMessage.UserName = (char *)[username cString];
	connectToDomainMessage.Password = (char *)[password cString];
	connectToDomainMessage.Host = (char *)[host cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__ConnectToDomain(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &connectToDomainMessage,
            &connectToDomainResponse);

    if (err_code == SOAP_OK)
    {
		domain = [ [IFDomain alloc] init];
		
		struct ns1__DomainWeb *curDomain;
			
		curDomain = connectToDomainResponse.ConnectToDomainResult;
		[domain from_gsoap:curDomain];
    }
	else
	{
		[NSException raise:@"ConnectToDomainException" format:@"An error happened when calling ConnectToDomain"];
	}

    cleanup_gsoap(&soap);

	return domain;
}






void init_gsoap(struct soap *pSoap)
{
	soap_init(pSoap);
	soap_set_namespaces(pSoap, iFolder_namespaces);
}

void cleanup_gsoap(struct soap *pSoap)
{
	soap_end(pSoap);
}

@end
