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

NSDictionary *getiFolderProperties(struct ns1__iFolderWeb *ifolder);
NSDictionary *getiFolderUserProperties(struct ns1__iFolderUser *user);
NSDictionary *getDiskSpaceProperties(struct ns1__DiskSpace *ds);
NSDictionary *getSyncSizeProperties(struct ns1__SyncSize *ss);


-(BOOL) Ping
{
    struct soap soap;
    BOOL isRunning = NO;
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
        isRunning = YES;
    }
	else
	{
		NSString *faultString = [NSString stringWithCString:soap.fault->faultstring];

		cleanup_gsoap(&soap);

		[NSException raise:faultString format:@"Error in Ping"];
	}

    cleanup_gsoap(&soap);

    return isRunning;
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

				[newiFolder setProperties:getiFolderProperties(curiFolder)];
				
				[ifolders addObject:newiFolder];
			}
		}
    }

    cleanup_gsoap(&soap);

	return ifolders;
}




-(iFolder *) GetiFolder:(NSString *)iFolderID
{
	iFolder *ifolder = nil;
    struct soap soap;
    int err_code;

	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__GetiFolder			getiFolderMessage;
	struct _ns1__GetiFolderResponse getiFolderResponse;
	
	getiFolderMessage.iFolderID = (char *)[iFolderID cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__GetiFolder(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &getiFolderMessage,
            &getiFolderResponse);

	if(soap.error)
	{
	    cleanup_gsoap(&soap);
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in GetiFolder"];
	}
	else
	{
		struct ns1__iFolderWeb *curiFolder;
		curiFolder = getiFolderResponse.GetiFolderResult;
		
		if(curiFolder == NULL)
		{
		    cleanup_gsoap(&soap);
			[NSException raise:@"Invalid iFolderID" format:@"Error in GetiFolder"];
		}

		ifolder = [[[iFolder alloc] init] retain];

		[ifolder setProperties:getiFolderProperties(curiFolder)];
    }

    cleanup_gsoap(&soap);

	return [ifolder autorelease];
}




-(iFolder *) GetAvailableiFolder:(NSString *)iFolderID
					inCollection:(NSString *)collectionID
{
	iFolder *ifolder = nil;
    struct soap soap;
    int err_code;

	NSAssert( (iFolderID != nil), @"iFolderID was nil");
	NSAssert( (collectionID != nil), @"collectionID was nil");

	struct _ns1__GetiFolderInvitation			getiFolderMessage;
	struct _ns1__GetiFolderInvitationResponse	getiFolderResponse;
	
	getiFolderMessage.iFolderID = (char *)[iFolderID cString];
	getiFolderMessage.POBoxID = (char *)[collectionID cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__GetiFolderInvitation(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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

    cleanup_gsoap(&soap);

	return [ifolder autorelease];
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

		[ifolder setProperties:getiFolderProperties(curiFolder)];
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
	NSAssert( (iFolderID != nil), @"iFolderID was nil");

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
		[ifolder setProperties:getiFolderProperties(curiFolder)];
    }

    cleanup_gsoap(&soap);

	return ifolder;
}




-(void)DeleteiFolder:(NSString *)iFolderID
{
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




-(iFolder *) RevertiFolder:(NSString *)iFolderID
{
	iFolder *ifolder = nil;
    struct soap soap;
    int err_code;

	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__RevertiFolder			revertiFolderMessage;
	struct _ns1__RevertiFolderResponse	revertiFolderResponse;
	
	revertiFolderMessage.iFolderID = (char *)[iFolderID cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__RevertiFolder(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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

    cleanup_gsoap(&soap);
	
	return ifolder;	
}




-(void) SynciFolderNow:(NSString *)iFolderID
{
	iFolder *ifolder = nil;
    struct soap soap;
    int err_code;

	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__SynciFolderNow			syncNowMessage;
	struct _ns1__SynciFolderNowResponse syncNowResponse;
	
	syncNowMessage.iFolderID = (char *)[iFolderID cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__SynciFolderNow(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &syncNowMessage,
            &syncNowResponse);

	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in DeleteiFolder"];
	}

    cleanup_gsoap(&soap);
}




-(User *) GetiFolderUser:(NSString *)userID
{
	User *user = nil;
    struct soap soap;
    int err_code;

	NSAssert( (userID != nil), @"userID was nil");

	struct _ns1__GetiFolderUser			getUserMessage;
	struct _ns1__GetiFolderUserResponse	getUserResponse;

	getUserMessage.UserID = (char *)[userID cString];

    init_gsoap (&soap);

    err_code = soap_call___ns1__GetiFolderUser(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
		    cleanup_gsoap(&soap);
			[NSException raise:@"Invalid User" format:@"Error in GetiFolderUser"];
		}
	
		user = [[User alloc] init];
		[user setProperties:getiFolderUserProperties(curUser)];			
    }

    cleanup_gsoap(&soap);

	return user;
}




-(User *) GetiFolderUserFromNodeID:(NSString *)nodeID inCollection:(NSString *)collectionID
{
	User *user = nil;
    struct soap soap;
    int err_code;

	NSAssert( (nodeID != nil), @"nodeID was nil");
	NSAssert( (collectionID != nil), @"collectionID was nil");

	struct _ns1__GetiFolderUserFromNodeID			getUserMessage;
	struct _ns1__GetiFolderUserFromNodeIDResponse	getUserResponse;

	getUserMessage.CollectionID = (char *)[collectionID cString];
	getUserMessage.NodeID = (char *)[nodeID cString];

    init_gsoap (&soap);

    err_code = soap_call___ns1__GetiFolderUserFromNodeID(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
		    cleanup_gsoap(&soap);
			[NSException raise:@"Invalid User" format:@"Error in GetiFolderUserFromNodeID"];
		}
	
		user = [[User alloc] init];
		[user setProperties:getiFolderUserProperties(curUser)];			
    }

    cleanup_gsoap(&soap);

	return user;
}



-(NSArray *) GetiFolderUsers:(NSString *)ifolderID
{
	NSMutableArray *users = nil;
	
    struct soap soap;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__GetiFolderUsers		getUsersMessage;
	struct _ns1__GetiFolderUsersResponse getUsersResponse;

	getUsersMessage.iFolderID = (char *)[ifolderID cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__GetiFolderUsers(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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

    cleanup_gsoap(&soap);

	return users;
}




-(NSArray *) GetDomainUsers:(NSString *)domainID withLimit:(int)numUsers
{
	NSMutableArray *users = nil;
	
    struct soap soap;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__GetDomainUsers			getUsersMessage;
	struct _ns1__GetDomainUsersResponse getUsersResponse;

	getUsersMessage.DomainID = (char *)[domainID cString];
	getUsersMessage.numUsers = numUsers;

    init_gsoap (&soap);
    err_code = soap_call___ns1__GetDomainUsers(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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

    cleanup_gsoap(&soap);

	return users;
}




-(NSArray *) SearchDomainUsers:(NSString *)domainID withString:(NSString *)searchString
{
	NSMutableArray *users = nil;
	
    struct soap soap;
    int err_code;

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SearchForDomainUsers			searchUsersMessage;
	struct _ns1__SearchForDomainUsersResponse	searchUsersResponse;

	searchUsersMessage.DomainID = (char *)[domainID cString];
	searchUsersMessage.SearchString =  (char *)[searchString cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__SearchForDomainUsers(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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

    cleanup_gsoap(&soap);

	return users;
}



-(User *) InviteUser:(NSString *)userID toiFolder:(NSString *)ifolderID withRights:(NSString *)rights
{
	User *newUser = nil;
    struct soap soap;
    int err_code;

	NSAssert( (userID != nil), @"userID was nil");
	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (rights != nil), @"rights was nil");

	struct _ns1__InviteUser			inviteUserMessage;
	struct _ns1__InviteUserResponse	inviteUserResponse;
	
	inviteUserMessage.iFolderID = (char *)[ifolderID cString];
	inviteUserMessage.UserID = (char *)[userID cString];
	inviteUserMessage.Rights = (char *)[rights cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__InviteUser(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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

    cleanup_gsoap(&soap);

	return newUser;
}




-(void) RemoveUser:(NSString *)userID fromiFolder:(NSString *)ifolderID
{
    struct soap soap;
    int err_code;

	NSAssert( (userID != nil), @"userID was nil");
	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__RemoveiFolderUser			removeUserMessage;
	struct _ns1__RemoveiFolderUserResponse	removeUserResponse;
	
	removeUserMessage.iFolderID = (char *)[ifolderID cString];
	removeUserMessage.UserID = (char *)[userID cString];

    init_gsoap (&soap);
    err_code = soap_call___ns1__RemoveiFolderUser(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &removeUserMessage,
            &removeUserResponse);

	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in RemoveUser:fromiFolder:"];
	}

    cleanup_gsoap(&soap);
}



-(DiskSpace *)GetiFolderDiskSpace:(NSString *)ifolderID
{
	DiskSpace *ds = nil;
    struct soap soap;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__GetiFolderDiskSpace			getDSMessage;
	struct _ns1__GetiFolderDiskSpaceResponse	getDSResponse;

	getDSMessage.iFolderID = (char *)[ifolderID cString];

    init_gsoap (&soap);

    err_code = soap_call___ns1__GetiFolderDiskSpace(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
		    cleanup_gsoap(&soap);
			[NSException raise:@"Invalid iFolderID" format:@"Error in GetiFolderDiskSpace"];
		}
	
		ds = [[DiskSpace alloc] init];
		[ds setProperties:getDiskSpaceProperties(curDS)];			
    }

    cleanup_gsoap(&soap);

	return ds;
}




-(DiskSpace *)GetUserDiskSpace:(NSString *)userID
{
	DiskSpace *ds = nil;
    struct soap soap;
    int err_code;

	NSAssert( (userID != nil), @"userID was nil");

	struct _ns1__GetUserDiskSpace			getDSMessage;
	struct _ns1__GetUserDiskSpaceResponse	getDSResponse;

	getDSMessage.UserID = (char *)[userID cString];

    init_gsoap (&soap);

    err_code = soap_call___ns1__GetUserDiskSpace(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
		    cleanup_gsoap(&soap);
			[NSException raise:@"Invalid userID" format:@"Error in GetUserDiskSpace"];
		}
	
		ds = [[DiskSpace alloc] init];
		[ds setProperties:getDiskSpaceProperties(curDS)];			
    }

    cleanup_gsoap(&soap);

	return ds;
}




-(void)SetiFolderDiskSpace:(long long)limit oniFolder:(NSString *)ifolderID
{
    struct soap soap;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__SetiFolderDiskSpaceLimit			setDSMessage;
	struct _ns1__SetiFolderDiskSpaceLimitResponse	setDSResponse;

	setDSMessage.Limit = limit;
	setDSMessage.iFolderID = (char *)[ifolderID cString];

    init_gsoap (&soap);

    err_code = soap_call___ns1__SetiFolderDiskSpaceLimit(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &setDSMessage,
            &setDSResponse);

 	if(soap.error)
	{
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in SetiFolderDiskSpace"];
	}

    cleanup_gsoap(&soap);
}



-(SyncSize *)CalculateSyncSize:(NSString *)ifolderID
{
	SyncSize *ss = nil;
    struct soap soap;
    int err_code;

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__CalculateSyncSize			getSSMessage;
	struct _ns1__CalculateSyncSizeResponse	getSSResponse;

	getSSMessage.iFolderID = (char *)[ifolderID cString];

    init_gsoap (&soap);

    err_code = soap_call___ns1__CalculateSyncSize(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
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
		    cleanup_gsoap(&soap);
			[NSException raise:@"Invalid iFolderID" format:@"Error in CalculateSyncSize"];
		}
	
		ss = [[SyncSize alloc] init];
		[ss setProperties:getSyncSizeProperties(curSS)];			
    }

    cleanup_gsoap(&soap);

	return ss;
}


-(int)GetDefaultSyncInterval
{
    struct soap soap;
    int err_code;

	struct _ns1__GetDefaultSyncInterval			getIntervalMessage;
	struct _ns1__GetDefaultSyncIntervalResponse getIntervalResponse;
	
    init_gsoap (&soap);
    err_code = soap_call___ns1__GetDefaultSyncInterval(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &getIntervalMessage,
            &getIntervalResponse);

	if(soap.error)
	{
	    cleanup_gsoap(&soap);
		[NSException raise:[NSString stringWithFormat:@"%s", soap.fault->faultstring]
					format:@"Error in DeleteiFolder"];
	}
	
	int interval = getIntervalResponse.GetDefaultSyncIntervalResult;

    cleanup_gsoap(&soap);

	return interval;
}




-(void)SetDefaultSyncInterval:(int)syncInterval
{
    struct soap soap;
    int err_code;

	struct _ns1__SetDefaultSyncInterval			setIntervalMessage;
	struct _ns1__SetDefaultSyncIntervalResponse setIntervalResponse;
	
	setIntervalMessage.Interval = syncInterval;
	
    init_gsoap (&soap);
    err_code = soap_call___ns1__SetDefaultSyncInterval(
			&soap,
            NULL, //http://127.0.0.1:8086/simias10/iFolder.asmx
            NULL,
            &setIntervalMessage,
            &setIntervalResponse);

	if(soap.error)
	{
	    cleanup_gsoap(&soap);
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


NSDictionary *getiFolderProperties(struct ns1__iFolderWeb *ifolder)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];

	if(ifolder->DomainID != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->DomainID] forKey:@"DomainID"];

	if(ifolder->ID != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->ID] forKey:@"ID"];

	if(ifolder->ManagedPath != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->ManagedPath] forKey:@"ManagedPath"];

	if(ifolder->UnManagedPath != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->UnManagedPath] forKey:@"Path"];

	if(ifolder->Name != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->Name] forKey:@"Name"];

	if(ifolder->Owner != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->Owner] forKey:@"Owner"];

	if(ifolder->OwnerID != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->OwnerID] forKey:@"OwnerID"];

	if(ifolder->Type != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->Type] forKey:@"Type"];

	if(ifolder->Description != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->Description] forKey:@"Description"];

	if(ifolder->State != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->State] forKey:@"State"];

	if(ifolder->CurrentUserID != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->CurrentUserID] forKey:@"CurrentUserID"];

	if(ifolder->CurrentUserRights != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->CurrentUserRights] forKey:@"CurrentUserRights"];

	if(ifolder->CollectionID != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->CollectionID] forKey:@"CollectionID"];

	if(ifolder->LastSyncTime != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->LastSyncTime] forKey:@"LastSyncTime"];

	if(ifolder->Role != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->Role] forKey:@"Role"];

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
		[newProperties setObject:[NSString stringWithCString:user->Name] forKey:@"Name"];
	if(user->UserID != nil)
		[newProperties setObject:[NSString stringWithCString:user->UserID] forKey:@"UserID"];
	if(user->Rights != nil)
		[newProperties setObject:[NSString stringWithCString:user->Rights] forKey:@"Rights"];
	if(user->ID != nil)
		[newProperties setObject:[NSString stringWithCString:user->ID] forKey:@"ID"];
	if(user->State != nil)
		[newProperties setObject:[NSString stringWithCString:user->State] forKey:@"State"];
	if(user->iFolderID != nil)
		[newProperties setObject:[NSString stringWithCString:user->iFolderID] forKey:@"iFolderID"];
	if(user->FirstName != nil)
		[newProperties setObject:[NSString stringWithCString:user->FirstName] forKey:@"FirstName"];
	if(user->Surname != nil)
		[newProperties setObject:[NSString stringWithCString:user->Surname] forKey:@"Surname"];
	if( (user->FN != nil) && (strlen(user->FN) > 0) )
		[newProperties setObject:[NSString stringWithCString:user->FN] forKey:@"FN"];
	else
		[newProperties setObject:[NSString stringWithCString:user->Name] forKey:@"FN"];

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


@end
