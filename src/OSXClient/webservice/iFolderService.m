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



-(NSArray *) GetDomains
{
	NSMutableArray *domains = nil;
	
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
		domains = [[NSMutableArray alloc]
				initWithCapacity:getDomainsResponse.GetDomainsResult->__sizeDomainWeb];
		int counter;
		for(counter=0;counter<getDomainsResponse.GetDomainsResult->__sizeDomainWeb;counter++)
		{
			struct ns1__DomainWeb *curDomain;
			
			curDomain = getDomainsResponse.GetDomainsResult->DomainWeb[counter];
			iFolderDomain *newDomain = [[iFolderDomain alloc] init];
			
			[newDomain setgSOAPProperties:curDomain];
			
			[domains addObject:newDomain];
		}
    }
	else
	{
		[NSException raise:@"GetDomainsException" format:@"An error happened when calling GetDomains"];
	}

    cleanup_gsoap(&soap);

	return domains;
}




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

    init_gsoap (&soap);
    err_code = soap_call___ns1__ConnectToDomain(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &connectToDomainMessage,
            &connectToDomainResponse);

    if (err_code == SOAP_OK)
    {
		domain = [ [iFolderDomain alloc] init];
		
		struct ns1__DomainWeb *curDomain;
			
		curDomain = connectToDomainResponse.ConnectToDomainResult;
		[domain setgSOAPProperties:curDomain];
    }
	else
	{
		[NSException raise:@"ConnectToDomainException" format:@"An error happened when calling ConnectToDomain"];
	}

    cleanup_gsoap(&soap);

	return domain;
}




-(NSArray *) GetiFolders
{
	NSMutableArray *ifolders = nil;
	
    struct soap soap;
    int err_code;

	struct _ns1__GetAlliFolders getiFoldersMessage;
	struct _ns1__GetAlliFoldersResponse getiFoldersResponse;

    init_gsoap (&soap);
    err_code = soap_call___ns1__GetAlliFolders(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &getiFoldersMessage,
            &getiFoldersResponse);

    if (err_code == SOAP_OK)
    {
		int iFolderCount = getiFoldersResponse.GetAlliFoldersResult->__sizeiFolderWeb;
		if(iFolderCount > 0)
		{
			ifolders = [[NSMutableArray alloc] initWithCapacity:iFolderCount];
			
			int counter;
			for( counter = 0; counter < iFolderCount; counter++ )
			{
				struct ns1__iFolderWeb *curiFolder;
			
				curiFolder = getiFoldersResponse.GetAlliFoldersResult->iFolderWeb[counter];
				iFolder *newiFolder = [[iFolder alloc] init];
			
				[newiFolder setgSOAPProperties:curiFolder];
				
				[ifolders addObject:newiFolder];
			}
		}
    }
	else
	{
		[NSException raise:@"GetAlliFoldersException" format:@"An error happened when calling GetAlliFolders"];
	}

    cleanup_gsoap(&soap);

	return ifolders;
}




-(iFolder *) CreateiFolder:(NSString *)Path InDomain:(NSString *)DomainID
{
	iFolder *ifolder = nil;
    struct soap soap;
    int err_code;

	NSAssert( (Path != nil), @"Path was nil");
	NSAssert( (DomainID != nil), @"DomainID was nil");

	struct _ns1__CreateiFolderInDomain createiFolderMessage;
	struct _ns1__CreateiFolderInDomainResponse createiFolderResponse;
	
	createiFolderMessage.Path = (char *)[Path cString];
	createiFolderMessage.DomainID = (char *)[DomainID cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__CreateiFolderInDomain(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &createiFolderMessage,
            &createiFolderResponse);

    if (err_code == SOAP_OK)
    {
		ifolder = [ [iFolder alloc] init];
		
		struct ns1__iFolderWeb *curiFolder;
			
		curiFolder = createiFolderResponse.CreateiFolderInDomainResult;
		[ifolder setgSOAPProperties:curiFolder];
    }
	else
	{
		[NSException raise:@"CreateiFolder:inDomain" format:@"An error happened when calling CreateiFolder:inDomain"];
	}

    cleanup_gsoap(&soap);

	return ifolder;
}




-(iFolder *) AcceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)DomainID toPath:(NSString *)localPath
{
	iFolder *ifolder = nil;
    struct soap soap;
    int err_code;

	NSAssert( (localPath != nil), @"Path was nil");
	NSAssert( (DomainID != nil), @"DomainID was nil");
	NSAssert( (iFolderID != nil), @"DomainID was nil");

	struct _ns1__AcceptiFolderInvitation acceptiFolderMessage;
	struct _ns1__AcceptiFolderInvitationResponse acceptiFolderResponse;
	
	acceptiFolderMessage.iFolderID = (char *)[iFolderID cString];
	acceptiFolderMessage.DomainID = (char *)[DomainID cString];
	acceptiFolderMessage.LocalPath = (char *)[localPath cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__AcceptiFolderInvitation(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &acceptiFolderMessage,
            &acceptiFolderResponse);

    if (err_code == SOAP_OK)
    {
		ifolder = [ [iFolder alloc] init];
		
		struct ns1__iFolderWeb *curiFolder;
			
		curiFolder = acceptiFolderResponse.AcceptiFolderInvitationResult;
		[ifolder setgSOAPProperties:curiFolder];
    }
	else
	{
		[NSException raise:@"AcceptiFolderInvitation:inDomain" format:@"An error happened when calling AcceptiFolderInvitation:inDomain"];
	}

    cleanup_gsoap(&soap);

	return ifolder;
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
