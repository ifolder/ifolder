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

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in GetDomains"];
	}
	else
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
 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in ConnectToDomain"];
	}
	else
	{
		domain = [ [iFolderDomain alloc] init];
		
		struct ns1__DomainWeb *curDomain;
			
		curDomain = connectToDomainResponse.ConnectToDomainResult;
		[domain setgSOAPProperties:curDomain];
    }

    cleanup_gsoap(&soap);

	return domain;
}




-(void) AuthenticateToDomain:(NSString *)DomainID usingPassword:(NSString *)Password
{
    struct soap soap;
    int err_code;

	NSAssert( (DomainID != nil), @"DomainID was nil");
	NSAssert( (Password != nil), @"Password was nil");

	struct _ns1__AuthenticateToDomain authenticateToDomainMessage;
	struct _ns1__AuthenticateToDomainResponse authenticateToDomainResponse;
	
	authenticateToDomainMessage.DomainID = (char *)[DomainID cString];
	authenticateToDomainMessage.Password = (char *)[Password cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__AuthenticateToDomain(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &authenticateToDomainMessage,
            &authenticateToDomainResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in AuthenticateToDomain"];
	}

    cleanup_gsoap(&soap);
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

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in GetAlliFolders"];
	}
	else
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

	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in CreateiFolder:inDomain"];
	}
	else
	{
		ifolder = [ [iFolder alloc] init];
		
		struct ns1__iFolderWeb *curiFolder;
			
		curiFolder = createiFolderResponse.CreateiFolderInDomainResult;
		[ifolder setgSOAPProperties:curiFolder];
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


	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in AcceptiFolderInvitation:inDomain"];
	}
	else
	{
		ifolder = [ [iFolder alloc] init];
		
		struct ns1__iFolderWeb *curiFolder;
			
		curiFolder = acceptiFolderResponse.AcceptiFolderInvitationResult;
		[ifolder setgSOAPProperties:curiFolder];
    }

    cleanup_gsoap(&soap);

	return ifolder;
}




-(void)DeleteiFolder:(NSString *)iFolderID
{
	iFolder *ifolder = nil;
    struct soap soap;
    int err_code;

	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__DeleteiFolder deleteiFolderMessage;
	struct _ns1__DeleteiFolderResponse deleteiFolderResponse;
	
	deleteiFolderMessage.iFolderID = (char *)[iFolderID cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__DeleteiFolder(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &deleteiFolderMessage,
            &deleteiFolderResponse);

	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in DeleteiFolder"];
	}

    cleanup_gsoap(&soap);
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
