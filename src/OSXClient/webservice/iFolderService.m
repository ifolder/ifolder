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
#import "Simias.h"

typedef struct gsoap_creds
{
	char *username;
	char *password;
} GSOAP_CREDS;

@implementation iFolderService

void init_gsoap(struct soap *pSoap, GSOAP_CREDS *creds);
void cleanup_gsoap(struct soap *pSoap, GSOAP_CREDS *creds);

NSDictionary *getiFolderProperties(struct ns1__iFolderWeb *ifolder);
NSDictionary *getiFolderUserProperties(struct ns1__iFolderUser *user);
NSDictionary *getDiskSpaceProperties(struct ns1__DiskSpace *ds);
NSDictionary *getSyncSizeProperties(struct ns1__SyncSize *ss);
NSDictionary *getConflictProperties(struct ns1__Conflict *conflict);


- (id)init 
{
	[super init];
	simiasURL = [[NSString stringWithFormat:@"%@/simias10/iFolder.asmx", [[Simias getInstance] simiasURL]] retain];
	NSLog(@"Initialized iFolderService on URL: %@", simiasURL);
    return self;
}
-(void)dealloc
{
	[simiasURL release];
    [super dealloc];
}



-(BOOL) Ping
{
    struct soap soap;
	GSOAP_CREDS creds;
    BOOL isRunning = NO;
    int err_code;

    struct _ns1__Ping ns1__Ping;
    struct _ns1__PingResponse ns1__PingResponse;

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__Ping (&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &ns1__Ping,
            &ns1__PingResponse);

    if (err_code == SOAP_OK)
    {
        isRunning = YES;
    }
	else
	{
		NSString *faultString = [NSString stringWithUTF8String:soap.fault->faultstring];

		cleanup_gsoap(&soap, &creds);

		[NSException raise:faultString format:@"Error in Ping"];
	}

    cleanup_gsoap(&soap, &creds);

    return isRunning;
}




-(NSArray *) GetiFolders
{
	NSMutableArray *ifolders = nil;
	
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	struct _ns1__GetAlliFolders getiFoldersMessage;
	struct _ns1__GetAlliFoldersResponse getiFoldersResponse;

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__GetAlliFolders(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
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

				[newiFolder setProperties:getiFolderProperties(curiFolder)];
				
				[ifolders addObject:newiFolder];
			}
		}
    }

    cleanup_gsoap(&soap, &creds);

	return ifolders;
}




-(iFolder *) GetiFolder:(NSString *)iFolderID
{
	iFolder *ifolder = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__GetiFolder			getiFolderMessage;
	struct _ns1__GetiFolderResponse getiFolderResponse;
	
	getiFolderMessage.iFolderID = (char *)[iFolderID UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__GetiFolder(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getiFolderMessage,
            &getiFolderResponse);

	if(soap.error)
	{
	    cleanup_gsoap(&soap, &creds);
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in GetiFolder"];
	}
	else
	{
		struct ns1__iFolderWeb *curiFolder;
		curiFolder = getiFolderResponse.GetiFolderResult;
		
		if(curiFolder == NULL)
		{
		    cleanup_gsoap(&soap, &creds);
			[NSException raise:@"Invalid iFolderID" format:@"Error in GetiFolder"];
		}

		ifolder = [[[iFolder alloc] init] retain];

		[ifolder setProperties:getiFolderProperties(curiFolder)];
    }

    cleanup_gsoap(&soap, &creds);

	return [ifolder autorelease];
}




-(iFolder *) GetAvailableiFolder:(NSString *)iFolderID
					inCollection:(NSString *)collectionID
{
	iFolder *ifolder = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (iFolderID != nil), @"iFolderID was nil");
	NSAssert( (collectionID != nil), @"collectionID was nil");

	struct _ns1__GetiFolderInvitation			getiFolderMessage;
	struct _ns1__GetiFolderInvitationResponse	getiFolderResponse;
	
	getiFolderMessage.iFolderID = (char *)[iFolderID UTF8String];
	getiFolderMessage.POBoxID = (char *)[collectionID UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__GetiFolderInvitation(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getiFolderMessage,
            &getiFolderResponse);

	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in GetAvailableiFolder"];
	}
	else
	{
		ifolder = [[[iFolder alloc] init] retain];
		
		struct ns1__iFolderWeb *curiFolder;
			
		curiFolder = getiFolderResponse.GetiFolderInvitationResult;

		[ifolder setProperties:getiFolderProperties(curiFolder)];
    }

    cleanup_gsoap(&soap, &creds);

	return [ifolder autorelease];
}




-(iFolder *) CreateiFolder:(NSString *)Path InDomain:(NSString *)DomainID
{
	iFolder *ifolder = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (Path != nil), @"Path was nil");
	NSAssert( (DomainID != nil), @"DomainID was nil");

	struct _ns1__CreateiFolderInDomain createiFolderMessage;
	struct _ns1__CreateiFolderInDomainResponse createiFolderResponse;
	
	createiFolderMessage.Path = (char *)[Path UTF8String];
	createiFolderMessage.DomainID = (char *)[DomainID UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__CreateiFolderInDomain(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
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

		[ifolder setProperties:getiFolderProperties(curiFolder)];
    }

    cleanup_gsoap(&soap, &creds);

	return ifolder;
}




-(iFolder *) AcceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)DomainID toPath:(NSString *)localPath
{
	iFolder *ifolder = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (localPath != nil), @"Path was nil");
	NSAssert( (DomainID != nil), @"DomainID was nil");
	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__AcceptiFolderInvitation acceptiFolderMessage;
	struct _ns1__AcceptiFolderInvitationResponse acceptiFolderResponse;
	
	acceptiFolderMessage.iFolderID = (char *)[iFolderID UTF8String];
	acceptiFolderMessage.DomainID = (char *)[DomainID UTF8String];
	acceptiFolderMessage.LocalPath = (char *)[localPath UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__AcceptiFolderInvitation(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
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
		[ifolder setProperties:getiFolderProperties(curiFolder)];
    }

    cleanup_gsoap(&soap, &creds);

	return ifolder;
}




-(void) DeclineiFolderInvitation:(NSString *)iFolderID fromDomain:(NSString *)DomainID
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (DomainID != nil), @"DomainID was nil");
	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__DeclineiFolderInvitation			declineiFolderMessage;
	struct _ns1__DeclineiFolderInvitationResponse	declineiFolderResponse;
	
	declineiFolderMessage.iFolderID = (char *)[iFolderID UTF8String];
	declineiFolderMessage.DomainID = (char *)[DomainID UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__DeclineiFolderInvitation(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &declineiFolderMessage,
            &declineiFolderResponse);

	if(soap.error)
	{
		cleanup_gsoap(&soap, &creds);

		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in DeclineiFolderInvitation:fromDomain"];
	}

    cleanup_gsoap(&soap, &creds);
}




-(iFolder *) RevertiFolder:(NSString *)iFolderID
{
	iFolder *ifolder = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__RevertiFolder			revertiFolderMessage;
	struct _ns1__RevertiFolderResponse	revertiFolderResponse;
	
	revertiFolderMessage.iFolderID = (char *)[iFolderID UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__RevertiFolder(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &revertiFolderMessage,
            &revertiFolderResponse);

	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in DeleteiFolder"];
	}
	else
	{
		ifolder = [ [iFolder alloc] init];
		
		struct ns1__iFolderWeb *curiFolder;
			
		curiFolder = revertiFolderResponse.RevertiFolderResult;
		[ifolder setProperties:getiFolderProperties(curiFolder)];
    }	

    cleanup_gsoap(&soap, &creds);
	
	return ifolder;	
}




-(void) SynciFolderNow:(NSString *)iFolderID
{
	iFolder *ifolder = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__SynciFolderNow			syncNowMessage;
	struct _ns1__SynciFolderNowResponse syncNowResponse;
	
	syncNowMessage.iFolderID = (char *)[iFolderID UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__SynciFolderNow(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &syncNowMessage,
            &syncNowResponse);

	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in DeleteiFolder"];
	}

    cleanup_gsoap(&soap, &creds);
}




-(User *) GetiFolderUser:(NSString *)userID
{
	User *user = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (userID != nil), @"userID was nil");

	struct _ns1__GetiFolderUser			getUserMessage;
	struct _ns1__GetiFolderUserResponse	getUserResponse;

	getUserMessage.UserID = (char *)[userID UTF8String];

    init_gsoap (&soap, &creds);

    err_code = soap_call___ns1__GetiFolderUser(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getUserMessage,
            &getUserResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in GetiFolderUserFromNodeID"];
	}
	else
	{
		struct ns1__iFolderUser *curUser;
		curUser = getUserResponse.GetiFolderUserResult;

		if(curUser == NULL)
		{
		    cleanup_gsoap(&soap, &creds);
			[NSException raise:@"Invalid User" format:@"Error in GetiFolderUser"];
		}
	
		user = [[User alloc] init];
		[user setProperties:getiFolderUserProperties(curUser)];			
    }

    cleanup_gsoap(&soap, &creds);

	return user;
}




-(User *) GetiFolderUserFromNodeID:(NSString *)nodeID inCollection:(NSString *)collectionID
{
	User *user = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (nodeID != nil), @"nodeID was nil");
	NSAssert( (collectionID != nil), @"collectionID was nil");

	struct _ns1__GetiFolderUserFromNodeID			getUserMessage;
	struct _ns1__GetiFolderUserFromNodeIDResponse	getUserResponse;

	getUserMessage.CollectionID = (char *)[collectionID UTF8String];
	getUserMessage.NodeID = (char *)[nodeID UTF8String];

    init_gsoap (&soap, &creds);

    err_code = soap_call___ns1__GetiFolderUserFromNodeID(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getUserMessage,
            &getUserResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in GetiFolderUserFromNodeID"];
	}
	else
	{
		struct ns1__iFolderUser *curUser;
		curUser = getUserResponse.GetiFolderUserFromNodeIDResult;

		if(curUser == NULL)
		{
		    cleanup_gsoap(&soap, &creds);
			[NSException raise:@"Invalid User" format:@"Error in GetiFolderUserFromNodeID"];
		}
	
		user = [[User alloc] init];
		[user setProperties:getiFolderUserProperties(curUser)];			
    }

    cleanup_gsoap(&soap, &creds);

	return user;
}



-(NSArray *) GetiFolderUsers:(NSString *)ifolderID
{
	NSMutableArray *users = nil;
	
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__GetiFolderUsers		getUsersMessage;
	struct _ns1__GetiFolderUsersResponse getUsersResponse;

	getUsersMessage.iFolderID = (char *)[ifolderID UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__GetiFolderUsers(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getUsersMessage,
            &getUsersResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in GetiFolderUsers"];
	}
	else
	{
		int usercount = getUsersResponse.GetiFolderUsersResult->__sizeiFolderUser;
		if(usercount > 0)
		{
			users = [[NSMutableArray alloc] initWithCapacity:usercount];
			
			int counter;
			for( counter = 0; counter < usercount; counter++ )
			{
				struct ns1__iFolderUser *curUser;
			
				curUser = getUsersResponse.GetiFolderUsersResult->iFolderUser[counter];
				User *newUser = [[User alloc] init];

				[newUser setProperties:getiFolderUserProperties(curUser)];
				
				[users addObject:newUser];
			}
		}
    }

    cleanup_gsoap(&soap, &creds);

	return users;
}




-(NSArray *) GetDomainUsers:(NSString *)domainID withLimit:(int)numUsers
{
	NSMutableArray *users = nil;
	
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__GetDomainUsers			getUsersMessage;
	struct _ns1__GetDomainUsersResponse getUsersResponse;

	getUsersMessage.DomainID = (char *)[domainID UTF8String];
	getUsersMessage.numUsers = numUsers;

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__GetDomainUsers(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getUsersMessage,
            &getUsersResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in GetDomainUsers"];
	}
	else
	{
		int usercount = getUsersResponse.GetDomainUsersResult->__sizeiFolderUser;
		if(usercount > 0)
		{
			users = [[NSMutableArray alloc] initWithCapacity:usercount];
			
			int counter;
			for( counter = 0; counter < usercount; counter++ )
			{
				struct ns1__iFolderUser *curUser;
			
				curUser = getUsersResponse.GetDomainUsersResult->iFolderUser[counter];
				User *newUser = [[User alloc] init];

				[newUser setProperties:getiFolderUserProperties(curUser)];
				
				[users addObject:newUser];
			}
		}
    }

    cleanup_gsoap(&soap, &creds);

	return users;
}




-(NSArray *) SearchDomainUsers:(NSString *)domainID withString:(NSString *)searchString
{
	NSMutableArray *users = nil;
	
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SearchForDomainUsers			searchUsersMessage;
	struct _ns1__SearchForDomainUsersResponse	searchUsersResponse;

	searchUsersMessage.DomainID = (char *)[domainID UTF8String];
	searchUsersMessage.SearchString =  (char *)[searchString UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__SearchForDomainUsers(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &searchUsersMessage,
            &searchUsersResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in SearchDomainUsers"];
	}
	else
	{
		int usercount = searchUsersResponse.SearchForDomainUsersResult->__sizeiFolderUser;
		if(usercount > 0)
		{
			users = [[NSMutableArray alloc] initWithCapacity:usercount];
			
			int counter;
			for( counter = 0; counter < usercount; counter++ )
			{
				struct ns1__iFolderUser *curUser;
			
				curUser = searchUsersResponse.SearchForDomainUsersResult->iFolderUser[counter];
				User *newUser = [[User alloc] init];

				[newUser setProperties:getiFolderUserProperties(curUser)];
				
				[users addObject:newUser];
			}
		}
    }

    cleanup_gsoap(&soap, &creds);

	return users;
}



-(User *) InviteUser:(NSString *)userID toiFolder:(NSString *)ifolderID withRights:(NSString *)rights
{
	User *newUser = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (userID != nil), @"userID was nil");
	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (rights != nil), @"rights was nil");

	struct _ns1__InviteUser			inviteUserMessage;
	struct _ns1__InviteUserResponse	inviteUserResponse;
	
	inviteUserMessage.iFolderID = (char *)[ifolderID UTF8String];
	inviteUserMessage.UserID = (char *)[userID UTF8String];
	inviteUserMessage.Rights = (char *)[rights UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__InviteUser(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &inviteUserMessage,
            &inviteUserResponse);

	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in InviteUser:toiFolder:withRights:"];
	}
	else
	{
		newUser = [ [User alloc] init];
		
		[newUser setProperties:getiFolderUserProperties(
							inviteUserResponse.InviteUserResult)];
    }

    cleanup_gsoap(&soap, &creds);

	return newUser;
}




-(void) RemoveUser:(NSString *)userID fromiFolder:(NSString *)ifolderID
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (userID != nil), @"userID was nil");
	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__RemoveiFolderUser			removeUserMessage;
	struct _ns1__RemoveiFolderUserResponse	removeUserResponse;
	
	removeUserMessage.iFolderID = (char *)[ifolderID UTF8String];
	removeUserMessage.UserID = (char *)[userID UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__RemoveiFolderUser(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &removeUserMessage,
            &removeUserResponse);

	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in RemoveUser:fromiFolder:"];
	}

    cleanup_gsoap(&soap, &creds);
}



-(DiskSpace *)GetiFolderDiskSpace:(NSString *)ifolderID
{
	DiskSpace *ds = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__GetiFolderDiskSpace			getDSMessage;
	struct _ns1__GetiFolderDiskSpaceResponse	getDSResponse;

	getDSMessage.iFolderID = (char *)[ifolderID UTF8String];

    init_gsoap (&soap, &creds);

    err_code = soap_call___ns1__GetiFolderDiskSpace(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getDSMessage,
            &getDSResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in GetiFolderDiskSpace"];
	}
	else
	{
		struct ns1__DiskSpace *curDS;
		curDS = getDSResponse.GetiFolderDiskSpaceResult;

		if(curDS == NULL)
		{
		    cleanup_gsoap(&soap, &creds);
			[NSException raise:@"Invalid iFolderID" format:@"Error in GetiFolderDiskSpace"];
		}
	
		ds = [[DiskSpace alloc] init];
		[ds setProperties:getDiskSpaceProperties(curDS)];			
    }

    cleanup_gsoap(&soap, &creds);

	return ds;
}




-(DiskSpace *)GetUserDiskSpace:(NSString *)userID
{
	DiskSpace *ds = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (userID != nil), @"userID was nil");

	struct _ns1__GetUserDiskSpace			getDSMessage;
	struct _ns1__GetUserDiskSpaceResponse	getDSResponse;

	getDSMessage.UserID = (char *)[userID UTF8String];

    init_gsoap (&soap, &creds);

    err_code = soap_call___ns1__GetUserDiskSpace(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getDSMessage,
            &getDSResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in GetUserDiskSpace"];
	}
	else
	{
		struct ns1__DiskSpace *curDS;
		curDS = getDSResponse.GetUserDiskSpaceResult;

		if(curDS == NULL)
		{
		    cleanup_gsoap(&soap, &creds);
			[NSException raise:@"Invalid userID" format:@"Error in GetUserDiskSpace"];
		}
	
		ds = [[DiskSpace alloc] init];
		[ds setProperties:getDiskSpaceProperties(curDS)];			
    }

    cleanup_gsoap(&soap, &creds);

	return ds;
}




-(void)SetiFolderDiskSpace:(long long)limit oniFolder:(NSString *)ifolderID
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__SetiFolderDiskSpaceLimit			setDSMessage;
	struct _ns1__SetiFolderDiskSpaceLimitResponse	setDSResponse;

	setDSMessage.Limit = limit;
	setDSMessage.iFolderID = (char *)[ifolderID UTF8String];

    init_gsoap (&soap, &creds);

    err_code = soap_call___ns1__SetiFolderDiskSpaceLimit(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setDSMessage,
            &setDSResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in SetiFolderDiskSpace"];
	}

    cleanup_gsoap(&soap, &creds);
}



-(SyncSize *)CalculateSyncSize:(NSString *)ifolderID
{
	SyncSize *ss = nil;
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__CalculateSyncSize			getSSMessage;
	struct _ns1__CalculateSyncSizeResponse	getSSResponse;

	getSSMessage.iFolderID = (char *)[ifolderID UTF8String];

    init_gsoap (&soap, &creds);

    err_code = soap_call___ns1__CalculateSyncSize(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getSSMessage,
            &getSSResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in CalculateSyncSize"];
	}
	else
	{
		struct ns1__SyncSize *curSS;
		curSS = getSSResponse.CalculateSyncSizeResult;

		if(curSS == NULL)
		{
		    cleanup_gsoap(&soap, &creds);
			[NSException raise:@"Invalid iFolderID" format:@"Error in CalculateSyncSize"];
		}
	
		ss = [[SyncSize alloc] init];
		[ss setProperties:getSyncSizeProperties(curSS)];			
    }

    cleanup_gsoap(&soap, &creds);

	return ss;
}


-(int)GetDefaultSyncInterval
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	struct _ns1__GetDefaultSyncInterval			getIntervalMessage;
	struct _ns1__GetDefaultSyncIntervalResponse getIntervalResponse;
	
    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__GetDefaultSyncInterval(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getIntervalMessage,
            &getIntervalResponse);

	if(soap.error)
	{
	    cleanup_gsoap(&soap, &creds);
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in DeleteiFolder"];
	}
	
	int interval = getIntervalResponse.GetDefaultSyncIntervalResult;

    cleanup_gsoap(&soap, &creds);

	return interval;
}




-(void)SetDefaultSyncInterval:(int)syncInterval
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	struct _ns1__SetDefaultSyncInterval			setIntervalMessage;
	struct _ns1__SetDefaultSyncIntervalResponse setIntervalResponse;
	
	setIntervalMessage.Interval = syncInterval;
	
    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__SetDefaultSyncInterval(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setIntervalMessage,
            &setIntervalResponse);

	if(soap.error)
	{
	    cleanup_gsoap(&soap, &creds);
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in DeleteiFolder"];
	}

    cleanup_gsoap(&soap, &creds);
}




-(NSArray *) GetiFolderConflicts:(NSString *)ifolderID
{
	NSMutableArray *conflicts = nil;
	
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__GetiFolderConflicts			getConflictsMessage;
	struct _ns1__GetiFolderConflictsResponse	getConflictsResponse;

	getConflictsMessage.iFolderID = (char *)[ifolderID UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__GetiFolderConflicts(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getConflictsMessage,
            &getConflictsResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in GetiFolderUsers"];
	}
	else
	{
		int conflictcount = getConflictsResponse.GetiFolderConflictsResult->__sizeConflict;
		if(conflictcount > 0)
		{
			conflicts = [[NSMutableArray alloc] initWithCapacity:conflictcount];
			
			int counter;
			for( counter = 0; counter < conflictcount; counter++ )
			{
				struct ns1__Conflict *curConflict;
			
				curConflict = getConflictsResponse.GetiFolderConflictsResult->Conflict[counter];
				iFolderConflict *newConflict = [[iFolderConflict alloc] init];

				[newConflict setProperties:getConflictProperties(curConflict)];
				
				[conflicts addObject:newConflict];
			}
		}
    }

    cleanup_gsoap(&soap, &creds);

	return conflicts;
}




-(void) ResolveFileConflict:(NSString *)ifolderID withID:(NSString *)conflictID localChanges:(bool)saveLocal
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (conflictID != nil), @"conflictID was nil");

	struct _ns1__ResolveFileConflict			resolveMessage;
	struct _ns1__ResolveFileConflictResponse	resolveResponse;

	resolveMessage.iFolderID = (char *)[ifolderID UTF8String];
	resolveMessage.conflictID = (char *)[conflictID UTF8String];
	resolveMessage.localChangesWin = saveLocal;

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__ResolveFileConflict(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &resolveMessage,
            &resolveResponse);

 	if(soap.error)
	{
	    cleanup_gsoap(&soap, &creds);
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in ResolveFileConflict"];
	}

    cleanup_gsoap(&soap, &creds);
}




-(void) ResolveNameConflict:(NSString *)ifolderID withID:(NSString *)conflictID usingName:(NSString *)newName
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (conflictID != nil), @"conflictID was nil");
	NSAssert( (newName != nil), @"newName was nil");

	struct _ns1__ResolveNameConflict			resolveMessage;
	struct _ns1__ResolveNameConflictResponse	resolveResponse;

	resolveMessage.iFolderID = (char *)[ifolderID UTF8String];
	resolveMessage.conflictID = (char *)[conflictID UTF8String];
	resolveMessage.newLocalName = (char *)[newName UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__ResolveNameConflict(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &resolveMessage,
            &resolveResponse);

 	if(soap.error)
	{
	    cleanup_gsoap(&soap, &creds);
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in ResolveNameConflict"];
	}

    cleanup_gsoap(&soap, &creds);
}




-(void)SetUserRights:(NSString *)ifolderID forUser:(NSString *)userID withRights:(NSString *)rights
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (userID != nil), @"userID was nil");
	NSAssert( (rights != nil), @"rights was nil");

	struct _ns1__SetUserRights			setRightsMessage;
	struct _ns1__SetUserRightsResponse	setRightsResponse;

	setRightsMessage.iFolderID = (char *)[ifolderID UTF8String];
	setRightsMessage.UserID = (char *)[userID UTF8String];
	setRightsMessage.Rights = (char *)[rights UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__SetUserRights(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setRightsMessage,
            &setRightsResponse);

 	if(soap.error)
	{
	    cleanup_gsoap(&soap, &creds);
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in SetUserRights"];
	}

    cleanup_gsoap(&soap, &creds);
}




-(void)ChanageOwner:(NSString *)ifolderID toUser:(NSString *)userID oldOwnerRights:(NSString *)rights
{
    struct soap soap;
	GSOAP_CREDS creds;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (userID != nil), @"userID was nil");
	NSAssert( (rights != nil), @"rights was nil");

	struct _ns1__ChangeOwner			changeOwnerMessage;
	struct _ns1__ChangeOwnerResponse	changeOwnerResponse;

	changeOwnerMessage.iFolderID = (char *)[ifolderID UTF8String];
	changeOwnerMessage.NewOwnerUserID = (char *)[userID UTF8String];
	changeOwnerMessage.OldOwnerRights = (char *)[rights UTF8String];

    init_gsoap (&soap, &creds);
    err_code = soap_call___ns1__ChangeOwner(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &changeOwnerMessage,
            &changeOwnerResponse);

 	if(soap.error)
	{
	    cleanup_gsoap(&soap, &creds);
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in ChanageOwner"];
	}

    cleanup_gsoap(&soap, &creds);
}




NSDictionary *getiFolderProperties(struct ns1__iFolderWeb *ifolder)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];

	if(ifolder->DomainID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->DomainID] forKey:@"DomainID"];

	if(ifolder->ID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->ID] forKey:@"ID"];

	if(ifolder->ManagedPath != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->ManagedPath] forKey:@"ManagedPath"];

	if(ifolder->UnManagedPath != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->UnManagedPath] forKey:@"Path"];

	if(ifolder->Name != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->Name] forKey:@"Name"];

	if(ifolder->Owner != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->Owner] forKey:@"Owner"];

	if(ifolder->OwnerID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->OwnerID] forKey:@"OwnerID"];

	if(ifolder->Type != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->Type] forKey:@"Type"];

	if(ifolder->Description != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->Description] forKey:@"Description"];

	if(ifolder->State != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->State] forKey:@"State"];

	if(ifolder->CurrentUserID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->CurrentUserID] forKey:@"CurrentUserID"];

	if(ifolder->CurrentUserRights != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->CurrentUserRights] forKey:@"CurrentUserRights"];

	if(ifolder->CollectionID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->CollectionID] forKey:@"CollectionID"];

	if(ifolder->LastSyncTime != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->LastSyncTime] forKey:@"LastSyncTime"];

	if(ifolder->Role != nil)
		[newProperties setObject:[NSString stringWithUTF8String:ifolder->Role] forKey:@"Role"];

	[newProperties setObject:[NSNumber numberWithLong:ifolder->EffectiveSyncInterval] forKey:@"EffectiveSyncInterval"];

	[newProperties setObject:[NSNumber numberWithLong:ifolder->SyncInterval] forKey:@"SyncInterval"];

	[newProperties setObject:[NSNumber numberWithBool:ifolder->IsSubscription] forKey:@"IsSubscription"];

	[newProperties setObject:[NSNumber numberWithBool:ifolder->IsWorkgroup] forKey:@"IsWorkgroup"];

	[newProperties setObject:[NSNumber numberWithBool:ifolder->HasConflicts] forKey:@"HasConflicts"];
	
	return newProperties;
}


NSDictionary *getiFolderUserProperties(struct ns1__iFolderUser *user)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];

	if(user->Name != nil)
		[newProperties setObject:[NSString stringWithUTF8String:user->Name] forKey:@"Name"];
	if(user->UserID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:user->UserID] forKey:@"UserID"];
	if(user->Rights != nil)
		[newProperties setObject:[NSString stringWithUTF8String:user->Rights] forKey:@"Rights"];
	if(user->ID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:user->ID] forKey:@"ID"];
	if(user->State != nil)
		[newProperties setObject:[NSString stringWithUTF8String:user->State] forKey:@"State"];
	if(user->iFolderID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:user->iFolderID] forKey:@"iFolderID"];
	if(user->FirstName != nil)
		[newProperties setObject:[NSString stringWithUTF8String:user->FirstName] forKey:@"FirstName"];
	if(user->Surname != nil)
		[newProperties setObject:[NSString stringWithUTF8String:user->Surname] forKey:@"Surname"];
	if( (user->FN != nil) && (strlen(user->FN) > 0) )
		[newProperties setObject:[NSString stringWithUTF8String:user->FN] forKey:@"FN"];
	else
		[newProperties setObject:[NSString stringWithUTF8String:user->Name] forKey:@"FN"];

	[newProperties setObject:[NSNumber numberWithBool:user->IsOwner] forKey:@"IsOwner"];
	
	return newProperties;
}





NSDictionary *getDiskSpaceProperties(struct ns1__DiskSpace *ds)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];

	[newProperties setObject:[NSNumber numberWithLongLong:ds->AvailableSpace] forKey:@"AvailableSpace"];
	[newProperties setObject:[NSNumber numberWithLongLong:ds->Limit] forKey:@"Limit"];
	[newProperties setObject:[NSNumber numberWithLongLong:ds->UsedSpace] forKey:@"UsedSpace"];
	
	return newProperties;
}



NSDictionary *getSyncSizeProperties(struct ns1__SyncSize *ss)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];

	[newProperties setObject:[NSNumber numberWithUnsignedLong:ss->SyncNodeCount] forKey:@"SyncNodeCount"];
	[newProperties setObject:[NSNumber numberWithUnsignedLongLong:ss->SyncByteCount] forKey:@"SyncByteCount"];
	
	return newProperties;
}


NSDictionary *getConflictProperties(struct ns1__Conflict *conflict)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];
	bool haveName = NO;
	bool haveLocation = NO;

	if(conflict->iFolderID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:conflict->iFolderID] forKey:@"iFolderID"];

	if(conflict->ConflictID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:conflict->ConflictID] forKey:@"ConflictID"];

	if(conflict->LocalName != nil)
	{
		[newProperties setObject:[NSString stringWithUTF8String:conflict->LocalName] forKey:@"LocalName"];
		[newProperties setObject:[NSString stringWithUTF8String:conflict->LocalName] forKey:@"Name"];
		haveName = YES;
	}

	if(conflict->LocalDate != nil)
		[newProperties setObject:[NSString stringWithUTF8String:conflict->LocalDate] forKey:@"LocalDate"];

	if(conflict->LocalSize != nil)
		[newProperties setObject:[NSString stringWithUTF8String:conflict->LocalSize] forKey:@"LocalSize"];

	if(conflict->LocalFullPath != nil)
	{
		[newProperties setObject:[NSString stringWithUTF8String:conflict->LocalFullPath] forKey:@"LocalFullPath"];
		[newProperties setObject:[NSString stringWithUTF8String:conflict->LocalFullPath] forKey:@"Location"];
		haveLocation = YES;
	}

	if(conflict->ServerName != nil)
	{
		[newProperties setObject:[NSString stringWithUTF8String:conflict->ServerName] forKey:@"ServerName"];
		if(!haveName)
			[newProperties setObject:[NSString stringWithUTF8String:conflict->ServerName] forKey:@"Name"];
	}

	if(conflict->ServerDate != nil)
		[newProperties setObject:[NSString stringWithUTF8String:conflict->ServerDate] forKey:@"ServerDate"];

	if(conflict->ServerSize != nil)
		[newProperties setObject:[NSString stringWithUTF8String:conflict->ServerSize] forKey:@"ServerSize"];

	if(conflict->ServerFullPath != nil)
	{
		[newProperties setObject:[NSString stringWithUTF8String:conflict->ServerFullPath] forKey:@"ServerFullPath"];
		if(!haveLocation)
			[newProperties setObject:[NSString stringWithUTF8String:conflict->ServerFullPath] forKey:@"Location"];
	}

	[newProperties setObject:[NSNumber numberWithBool:conflict->IsNameConflict] forKey:@"IsNameConflict"];

	return newProperties;
}



void init_gsoap(struct soap *pSoap, GSOAP_CREDS *creds)
{
	soap_init2(pSoap, SOAP_C_UTFSTRING, SOAP_C_UTFSTRING);
//	soap_init(pSoap);
	soap_set_namespaces(pSoap, iFolder_namespaces);
	
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
}



void cleanup_gsoap(struct soap *pSoap, GSOAP_CREDS *creds)
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
