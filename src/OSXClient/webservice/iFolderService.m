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
    bool isRunning = false;
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

    cleanup_gsoap(&soap);

	return domains;
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
