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

typedef struct soap_struct
{
	char				*username;
	char				*password;
	struct soap			*soap;
	NSRecursiveLock		*instanceLock;	
} SOAP_DATA;

@implementation iFolderService

void handle_soap_error(void *soapData, NSString *methodName);
struct soap *lockSoap(void *soapData);
void unlockSoap(void *soapData);

NSDictionary *getiFolderProperties(struct ns1__iFolderWeb *ifolder);
NSDictionary *getiFolderUserProperties(struct ns1__iFolderUser *user);
NSDictionary *getDiskSpaceProperties(struct ns1__DiskSpace *ds);
NSDictionary *getSyncSizeProperties(struct ns1__SyncSize *ss);
NSDictionary *getConflictProperties(struct ns1__Conflict *conflict);


- (id)init 
{
	SOAP_DATA	*pSoap;
	[super init];
	simiasURL = [[NSString stringWithFormat:@"%@/simias10/iFolder.asmx", [[Simias getInstance] simiasURL]] retain];
	NSLog(@"Initialized iFolderService on URL: %@", simiasURL);

	soapData = malloc(sizeof(SOAP_DATA));
	pSoap = (SOAP_DATA *)soapData;
	// the following code used to be done in init_gsoap

	pSoap->soap = malloc(sizeof(struct soap));
	if(pSoap->soap != NULL)
	{
		soap_init2(pSoap->soap, (SOAP_C_UTFSTRING | SOAP_IO_DEFAULT), (SOAP_C_UTFSTRING | SOAP_IO_DEFAULT));
		soap_set_namespaces(pSoap->soap, iFolder_namespaces);
		// Set the timeout for send and receive to 30 seconds
		pSoap->soap->recv_timeout = 30;
		pSoap->soap->send_timeout = 30;		
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
	
	pSoap->instanceLock = [[NSRecursiveLock alloc] init];
	
    return self;
}


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

	[simiasURL release];
    [super dealloc];
}



-(BOOL) Ping
{
    int err_code;
	struct soap *pSoap = lockSoap(soapData);

    struct _ns1__Ping ns1__Ping;
    struct _ns1__PingResponse ns1__PingResponse;

    err_code = soap_call___ns1__Ping (pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &ns1__Ping,
            &ns1__PingResponse);


	handle_soap_error(soapData, @"iFolderService.Ping");

	unlockSoap(soapData);

    return YES;
}




-(NSArray *) GetiFolders
{
	NSMutableArray *ifolders = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);

	struct _ns1__GetAlliFolders getiFoldersMessage;
	struct _ns1__GetAlliFoldersResponse getiFoldersResponse;

    err_code = soap_call___ns1__GetAlliFolders(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getiFoldersMessage,
            &getiFoldersResponse);

	handle_soap_error(soapData, @"iFolderService.GetiFolders");

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

	unlockSoap(soapData);

	return ifolders;
}




-(iFolder *) GetiFolder:(NSString *)iFolderID
{
	iFolder *ifolder = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);

	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__GetiFolder			getiFolderMessage;
	struct _ns1__GetiFolderResponse getiFolderResponse;
	
	getiFolderMessage.iFolderID = (char *)[iFolderID UTF8String];

    err_code = soap_call___ns1__GetiFolder(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getiFolderMessage,
            &getiFolderResponse);

	handle_soap_error(soapData, @"iFolderService.GetiFolder");

	struct ns1__iFolderWeb *curiFolder = getiFolderResponse.GetiFolderResult;
	
	if(curiFolder == NULL)
	{
		unlockSoap(soapData);
		[NSException raise:@"Invalid iFolderID" format:@"Error in GetiFolder"];
	}

	ifolder = [[[iFolder alloc] init] retain];

	[ifolder setProperties:getiFolderProperties(curiFolder)];

	unlockSoap(soapData);

	return [ifolder autorelease];
}




-(iFolder *) GetAvailableiFolder:(NSString *)iFolderID
					inCollection:(NSString *)collectionID
{
	iFolder *ifolder = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);

	NSAssert( (iFolderID != nil), @"iFolderID was nil");
	NSAssert( (collectionID != nil), @"collectionID was nil");

	struct _ns1__GetiFolderInvitation			getiFolderMessage;
	struct _ns1__GetiFolderInvitationResponse	getiFolderResponse;
	
	getiFolderMessage.iFolderID = (char *)[iFolderID UTF8String];
	getiFolderMessage.POBoxID = (char *)[collectionID UTF8String];

    err_code = soap_call___ns1__GetiFolderInvitation(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getiFolderMessage,
            &getiFolderResponse);

	handle_soap_error(soapData, @"iFolderService.GetAvailableiFolder:inCollection");

	struct ns1__iFolderWeb *curiFolder;

	curiFolder = getiFolderResponse.GetiFolderInvitationResult;
	if(curiFolder == NULL)
	{
		unlockSoap(soapData);
		[NSException raise:@"Invalid iFolderID" format:@"iFolderService.GetAvailableiFolder:inCollection"];
	}

	ifolder = [[[iFolder alloc] init] retain];

	[ifolder setProperties:getiFolderProperties(curiFolder)];

    unlockSoap(soapData);

	return [ifolder autorelease];
}




-(iFolder *) CreateiFolder:(NSString *)Path InDomain:(NSString *)DomainID
{
	iFolder *ifolder = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);

	NSAssert( (Path != nil), @"Path was nil");
	NSAssert( (DomainID != nil), @"DomainID was nil");

	struct _ns1__CreateiFolderInDomain createiFolderMessage;
	struct _ns1__CreateiFolderInDomainResponse createiFolderResponse;
	
	createiFolderMessage.Path = (char *)[Path UTF8String];
	createiFolderMessage.DomainID = (char *)[DomainID UTF8String];

    err_code = soap_call___ns1__CreateiFolderInDomain(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &createiFolderMessage,
            &createiFolderResponse);

	handle_soap_error(soapData, @"iFolderService.CreateiFolder:inDomain");

	ifolder = [ [iFolder alloc] init];
	
	struct ns1__iFolderWeb *curiFolder;
		
	curiFolder = createiFolderResponse.CreateiFolderInDomainResult;

	if(curiFolder == NULL)
	{
		unlockSoap(soapData);
		[NSException raise:@"Invalid iFolderID" format:@"iFolderService.CreateiFolder:inDomain"];
	}


	[ifolder setProperties:getiFolderProperties(curiFolder)];

    unlockSoap(soapData);

	return ifolder;
}




-(iFolder *) AcceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)DomainID toPath:(NSString *)localPath
{
	iFolder *ifolder = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);

	NSAssert( (localPath != nil), @"Path was nil");
	NSAssert( (DomainID != nil), @"DomainID was nil");
	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__AcceptiFolderInvitation acceptiFolderMessage;
	struct _ns1__AcceptiFolderInvitationResponse acceptiFolderResponse;
	
	acceptiFolderMessage.iFolderID = (char *)[iFolderID UTF8String];
	acceptiFolderMessage.DomainID = (char *)[DomainID UTF8String];
	acceptiFolderMessage.LocalPath = (char *)[localPath UTF8String];

    err_code = soap_call___ns1__AcceptiFolderInvitation(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &acceptiFolderMessage,
            &acceptiFolderResponse);


	handle_soap_error(soapData, @"iFolderService.AcceptiFolderInvitation:inDomain");

	ifolder = [ [iFolder alloc] init];
	
	struct ns1__iFolderWeb *curiFolder;
		
	curiFolder = acceptiFolderResponse.AcceptiFolderInvitationResult;

	if(curiFolder == NULL)
	{
		unlockSoap(soapData);
		[NSException raise:@"Invalid iFolderID" format:@"iFolderService.AcceptiFolderInvitation:inDomain"];
	}


	[ifolder setProperties:getiFolderProperties(curiFolder)];

    unlockSoap(soapData);

	return ifolder;
}




-(void) DeclineiFolderInvitation:(NSString *)iFolderID fromDomain:(NSString *)DomainID
{
    int err_code;
	struct soap *pSoap = lockSoap(soapData);
	
	NSAssert( (DomainID != nil), @"DomainID was nil");
	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__DeclineiFolderInvitation			declineiFolderMessage;
	struct _ns1__DeclineiFolderInvitationResponse	declineiFolderResponse;
	
	declineiFolderMessage.iFolderID = (char *)[iFolderID UTF8String];
	declineiFolderMessage.DomainID = (char *)[DomainID UTF8String];

    err_code = soap_call___ns1__DeclineiFolderInvitation(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &declineiFolderMessage,
            &declineiFolderResponse);

	handle_soap_error(soapData, @"iFolderService.DeclineiFolderInvitation:fromDomain");

	unlockSoap(soapData);
}




-(iFolder *) RevertiFolder:(NSString *)iFolderID
{
	iFolder *ifolder = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__RevertiFolder			revertiFolderMessage;
	struct _ns1__RevertiFolderResponse	revertiFolderResponse;
	
	revertiFolderMessage.iFolderID = (char *)[iFolderID UTF8String];

    err_code = soap_call___ns1__RevertiFolder(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &revertiFolderMessage,
            &revertiFolderResponse);

	handle_soap_error(soapData, @"iFolderService.RevertiFolder");


	ifolder = [ [iFolder alloc] init];
	
	struct ns1__iFolderWeb *curiFolder;
		
	curiFolder = revertiFolderResponse.RevertiFolderResult;

	if(curiFolder == NULL)
	{
		unlockSoap(soapData);
		[NSException raise:@"Invalid iFolderID" format:@"iFolderService.RevertiFolder"];
	}

	[ifolder setProperties:getiFolderProperties(curiFolder)];

    unlockSoap(soapData);
	
	return ifolder;	
}




-(void) DeleteiFolder:(NSString *)iFolderID
{
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__DeleteiFolder			deleteiFolderMessage;
	struct _ns1__DeleteiFolderResponse	deleteiFolderResponse;
	
	deleteiFolderMessage.iFolderID = (char *)[iFolderID UTF8String];

    err_code = soap_call___ns1__DeleteiFolder(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &deleteiFolderMessage,
            &deleteiFolderResponse);

	handle_soap_error(soapData, @"iFolderService.DeleteiFolder");

	unlockSoap(soapData);
}




-(void) SynciFolderNow:(NSString *)iFolderID
{
	iFolder *ifolder = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (iFolderID != nil), @"iFolderID was nil");

	struct _ns1__SynciFolderNow			syncNowMessage;
	struct _ns1__SynciFolderNowResponse syncNowResponse;
	
	syncNowMessage.iFolderID = (char *)[iFolderID UTF8String];

    err_code = soap_call___ns1__SynciFolderNow(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &syncNowMessage,
            &syncNowResponse);

	handle_soap_error(soapData, @"iFolderService.SynciFolderNow");

	unlockSoap(soapData);
}




-(User *) GetiFolderUser:(NSString *)userID
{
	User *user = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (userID != nil), @"userID was nil");

	struct _ns1__GetiFolderUser			getUserMessage;
	struct _ns1__GetiFolderUserResponse	getUserResponse;

	getUserMessage.UserID = (char *)[userID UTF8String];

    err_code = soap_call___ns1__GetiFolderUser(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getUserMessage,
            &getUserResponse);

	handle_soap_error(soapData, @"iFolderService.GetiFolderUser");

	struct ns1__iFolderUser *curUser;
	curUser = getUserResponse.GetiFolderUserResult;

	if(curUser == NULL)
	{
		unlockSoap(soapData);
		[NSException raise:@"Invalid User" format:@"Error in GetiFolderUser"];
	}

	user = [[User alloc] init];
	[user setProperties:getiFolderUserProperties(curUser)];			

	unlockSoap(soapData);

	return user;
}




-(User *) GetiFolderUserFromNodeID:(NSString *)nodeID inCollection:(NSString *)collectionID
{
	User *user = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (nodeID != nil), @"nodeID was nil");
	NSAssert( (collectionID != nil), @"collectionID was nil");

	struct _ns1__GetiFolderUserFromNodeID			getUserMessage;
	struct _ns1__GetiFolderUserFromNodeIDResponse	getUserResponse;

	getUserMessage.CollectionID = (char *)[collectionID UTF8String];
	getUserMessage.NodeID = (char *)[nodeID UTF8String];

    err_code = soap_call___ns1__GetiFolderUserFromNodeID(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getUserMessage,
            &getUserResponse);


	handle_soap_error(soapData, @"iFolderService.GetiFolderUserFromNodeID");

	struct ns1__iFolderUser *curUser;
	curUser = getUserResponse.GetiFolderUserFromNodeIDResult;

	if(curUser == NULL)
	{
		unlockSoap(soapData);
		[NSException raise:@"Invalid User" format:@"Error in GetiFolderUserFromNodeID"];
	}

	user = [[User alloc] init];
	[user setProperties:getiFolderUserProperties(curUser)];			

	unlockSoap(soapData);

	return user;
}



-(NSArray *) GetiFolderUsers:(NSString *)ifolderID
{
	NSMutableArray *users = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__GetiFolderUsers		getUsersMessage;
	struct _ns1__GetiFolderUsersResponse getUsersResponse;

	getUsersMessage.iFolderID = (char *)[ifolderID UTF8String];

    err_code = soap_call___ns1__GetiFolderUsers(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getUsersMessage,
            &getUsersResponse);

	handle_soap_error(soapData, @"iFolderService.GetiFolderUsers");

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

    unlockSoap(soapData);

	return users;
}




-(NSArray *) GetDomainUsers:(NSString *)domainID withLimit:(int)numUsers
{
	NSMutableArray *users = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__GetDomainUsers			getUsersMessage;
	struct _ns1__GetDomainUsersResponse getUsersResponse;

	getUsersMessage.DomainID = (char *)[domainID UTF8String];
	getUsersMessage.numUsers = numUsers;

    err_code = soap_call___ns1__GetDomainUsers(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getUsersMessage,
            &getUsersResponse);

	handle_soap_error(soapData, @"iFolderService.GetDomainUsers");

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

	unlockSoap(soapData);

	return users;
}




-(NSArray *) SearchDomainUsers:(NSString *)domainID withString:(NSString *)searchString
{
	NSMutableArray *users = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);
	
	NSAssert( (domainID != nil), @"domainID was nil");

	struct _ns1__SearchForDomainUsers			searchUsersMessage;
	struct _ns1__SearchForDomainUsersResponse	searchUsersResponse;

	searchUsersMessage.DomainID = (char *)[domainID UTF8String];
	searchUsersMessage.SearchString =  (char *)[searchString UTF8String];

    err_code = soap_call___ns1__SearchForDomainUsers(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &searchUsersMessage,
            &searchUsersResponse);

	handle_soap_error(soapData, @"iFolderService.SearchDomainUsers");

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

    unlockSoap(soapData);

	return users;
}




-(User *) AddAndInviteUser:(NSString *)memberID 
					MemberName:(NSString *)memberName
					GivenName:(NSString *)givenName
					FamilyName:(NSString *)familyName
					iFolderID:(NSString *)ifolderID
					PublicKey:(NSString *)publicKey
					Rights:(NSString *)rights
{
	User *newUser = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (memberName != nil), @"memberName was nil");
//	NSAssert( (givenName != nil), @"givenName was nil");
//	NSAssert( (familyName != nil), @"familyName was nil");
	NSAssert( (memberID != nil), @"memberID was nil");
//	NSAssert( (publicKey != nil), @"publicKey was nil");
	NSAssert( (rights != nil), @"rights was nil");

	struct _ns1__AddAndInviteUser			addinviteUserMessage;
	struct _ns1__AddAndInviteUserResponse	addinviteUserResponse;
	
	addinviteUserMessage.iFolderID = (char *)[ifolderID UTF8String];
	addinviteUserMessage.MemberName = (char *)[memberName UTF8String];
	if(givenName != nil)
		addinviteUserMessage.GivenName = (char *)[givenName UTF8String];
	else
		addinviteUserMessage.GivenName = NULL;
	if(familyName != nil)
		addinviteUserMessage.FamilyName = (char *)[familyName UTF8String];
	else
		addinviteUserMessage.FamilyName = NULL;
	addinviteUserMessage.MemberID = (char *)[memberID UTF8String];
	if(publicKey != nil)
		addinviteUserMessage.PublicKey = (char *)[publicKey UTF8String];
	else
		addinviteUserMessage.PublicKey = NULL;
	addinviteUserMessage.Rights = (char *)[rights UTF8String];

    err_code = soap_call___ns1__AddAndInviteUser(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &addinviteUserMessage,
            &addinviteUserResponse);


	handle_soap_error(soapData, @"iFolderService.AddAndInviteUser:");

	struct ns1__iFolderUser *curUser;
	curUser = addinviteUserResponse.AddAndInviteUserResult;

	if(curUser == NULL)
	{
		unlockSoap(soapData);
		[NSException raise:@"Invalid User" format:@"iFolderService.AddAndInviteUser:"];
	}

	newUser = [ [User alloc] init];
	
	[newUser setProperties:getiFolderUserProperties(curUser)];

    unlockSoap(soapData);

	return newUser;
}




-(User *) InviteUser:(NSString *)userID toiFolder:(NSString *)ifolderID withRights:(NSString *)rights
{
	User *newUser = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (userID != nil), @"userID was nil");
	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (rights != nil), @"rights was nil");

	struct _ns1__InviteUser			inviteUserMessage;
	struct _ns1__InviteUserResponse	inviteUserResponse;
	
	inviteUserMessage.iFolderID = (char *)[ifolderID UTF8String];
	inviteUserMessage.UserID = (char *)[userID UTF8String];
	inviteUserMessage.Rights = (char *)[rights UTF8String];

    err_code = soap_call___ns1__InviteUser(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &inviteUserMessage,
            &inviteUserResponse);


	handle_soap_error(soapData, @"iFolderService.InviteUser:toiFolder:withRights:");


	struct ns1__iFolderUser *curUser;
	curUser = inviteUserResponse.InviteUserResult;

	if(curUser == NULL)
	{
		unlockSoap(soapData);
		[NSException raise:@"Invalid User" format:@"iFolderService.InviteUser:toiFolder:withRights:"];
	}


	newUser = [ [User alloc] init];
	
	[newUser setProperties:getiFolderUserProperties(curUser)];

    unlockSoap(soapData);

	return newUser;
}




-(void) RemoveUser:(NSString *)userID fromiFolder:(NSString *)ifolderID
{
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (userID != nil), @"userID was nil");
	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__RemoveiFolderUser			removeUserMessage;
	struct _ns1__RemoveiFolderUserResponse	removeUserResponse;
	
	removeUserMessage.iFolderID = (char *)[ifolderID UTF8String];
	removeUserMessage.UserID = (char *)[userID UTF8String];

    err_code = soap_call___ns1__RemoveiFolderUser(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &removeUserMessage,
            &removeUserResponse);

	handle_soap_error(soapData, @"iFolderService.RemoveUser:fromiFolder:");

    unlockSoap(soapData);
}



-(DiskSpace *)GetiFolderDiskSpace:(NSString *)ifolderID
{
	DiskSpace *ds = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__GetiFolderDiskSpace			getDSMessage;
	struct _ns1__GetiFolderDiskSpaceResponse	getDSResponse;

	getDSMessage.iFolderID = (char *)[ifolderID UTF8String];

    err_code = soap_call___ns1__GetiFolderDiskSpace(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getDSMessage,
            &getDSResponse);

	handle_soap_error(soapData, @"iFolderService.GetiFolderDiskSpace");

	struct ns1__DiskSpace *curDS;
	curDS = getDSResponse.GetiFolderDiskSpaceResult;

	if(curDS == NULL)
	{
		unlockSoap(soapData);
		[NSException raise:@"Invalid iFolderID" format:@"Error in GetiFolderDiskSpace"];
	}

	ds = [[DiskSpace alloc] init];
	[ds setProperties:getDiskSpaceProperties(curDS)];			

    unlockSoap(soapData);

	return ds;
}




-(DiskSpace *)GetUserDiskSpace:(NSString *)userID
{
	DiskSpace *ds = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (userID != nil), @"userID was nil");

	struct _ns1__GetUserDiskSpace			getDSMessage;
	struct _ns1__GetUserDiskSpaceResponse	getDSResponse;

	getDSMessage.UserID = (char *)[userID UTF8String];

    err_code = soap_call___ns1__GetUserDiskSpace(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getDSMessage,
            &getDSResponse);

	handle_soap_error(soapData, @"iFolderService.GetUserDiskSpace");

	struct ns1__DiskSpace *curDS;
	curDS = getDSResponse.GetUserDiskSpaceResult;

	if(curDS == NULL)
	{
		unlockSoap(soapData);
		[NSException raise:@"Invalid userID" format:@"Error in GetUserDiskSpace"];
	}

	ds = [[DiskSpace alloc] init];
	[ds setProperties:getDiskSpaceProperties(curDS)];			

    unlockSoap(soapData);

	return ds;
}




-(void)SetiFolderDiskSpace:(long long)limit oniFolder:(NSString *)ifolderID
{
    int err_code;
    struct soap *pSoap = lockSoap(soapData);
	
	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__SetiFolderDiskSpaceLimit			setDSMessage;
	struct _ns1__SetiFolderDiskSpaceLimitResponse	setDSResponse;

	setDSMessage.Limit = limit;
	setDSMessage.iFolderID = (char *)[ifolderID UTF8String];

    err_code = soap_call___ns1__SetiFolderDiskSpaceLimit(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setDSMessage,
            &setDSResponse);

	handle_soap_error(soapData, @"iFolderService.SetiFolderDiskSpace");

	unlockSoap(soapData);
}



-(SyncSize *)CalculateSyncSize:(NSString *)ifolderID
{
	SyncSize *ss = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__CalculateSyncSize			getSSMessage;
	struct _ns1__CalculateSyncSizeResponse	getSSResponse;

	getSSMessage.iFolderID = (char *)[ifolderID UTF8String];

    err_code = soap_call___ns1__CalculateSyncSize(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getSSMessage,
            &getSSResponse);

	handle_soap_error(soapData, @"iFolderService.CalculateSyncSize");

	struct ns1__SyncSize *curSS;
	curSS = getSSResponse.CalculateSyncSizeResult;

	if(curSS == NULL)
	{
		unlockSoap(soapData);
		[NSException raise:@"Invalid iFolderID" format:@"Error in CalculateSyncSize"];
	}

	ss = [[SyncSize alloc] init];
	[ss setProperties:getSyncSizeProperties(curSS)];			

    unlockSoap(soapData);

	return [ss autorelease];
}


-(int)GetDefaultSyncInterval
{
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	struct _ns1__GetDefaultSyncInterval			getIntervalMessage;
	struct _ns1__GetDefaultSyncIntervalResponse getIntervalResponse;
	
    err_code = soap_call___ns1__GetDefaultSyncInterval(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getIntervalMessage,
            &getIntervalResponse);

	handle_soap_error(soapData, @"iFolderService.GetDefaultSyncInterval");
	
	int interval = getIntervalResponse.GetDefaultSyncIntervalResult;

    unlockSoap(soapData);

	return interval;
}




-(void)SetDefaultSyncInterval:(int)syncInterval
{
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	struct _ns1__SetDefaultSyncInterval			setIntervalMessage;
	struct _ns1__SetDefaultSyncIntervalResponse setIntervalResponse;
	
	setIntervalMessage.Interval = syncInterval;
	
    err_code = soap_call___ns1__SetDefaultSyncInterval(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setIntervalMessage,
            &setIntervalResponse);

	handle_soap_error(soapData, @"iFolderService.SetDefaultSyncInterval");

	unlockSoap(soapData);
}




-(NSArray *) GetiFolderConflicts:(NSString *)ifolderID
{
	NSMutableArray *conflicts = nil;
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (ifolderID != nil), @"ifolderID was nil");

	struct _ns1__GetiFolderConflicts			getConflictsMessage;
	struct _ns1__GetiFolderConflictsResponse	getConflictsResponse;

	getConflictsMessage.iFolderID = (char *)[ifolderID UTF8String];

    err_code = soap_call___ns1__GetiFolderConflicts(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &getConflictsMessage,
            &getConflictsResponse);

	handle_soap_error(soapData, @"iFolderService.GetiFolderConflicts");

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

    unlockSoap(soapData);

	return conflicts;
}




-(void) ResolveFileConflict:(NSString *)ifolderID withID:(NSString *)conflictID localChanges:(bool)saveLocal
{
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (conflictID != nil), @"conflictID was nil");

	struct _ns1__ResolveFileConflict			resolveMessage;
	struct _ns1__ResolveFileConflictResponse	resolveResponse;

	resolveMessage.iFolderID = (char *)[ifolderID UTF8String];
	resolveMessage.conflictID = (char *)[conflictID UTF8String];
	resolveMessage.localChangesWin = saveLocal;

    err_code = soap_call___ns1__ResolveFileConflict(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &resolveMessage,
            &resolveResponse);

	handle_soap_error(soapData, @"iFolderService.ResolveFileConflict");

	unlockSoap(soapData);
}




-(void) ResolveNameConflict:(NSString *)ifolderID withID:(NSString *)conflictID usingName:(NSString *)newName
{
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (conflictID != nil), @"conflictID was nil");
	NSAssert( (newName != nil), @"newName was nil");

	struct _ns1__ResolveNameConflict			resolveMessage;
	struct _ns1__ResolveNameConflictResponse	resolveResponse;

	resolveMessage.iFolderID = (char *)[ifolderID UTF8String];
	resolveMessage.conflictID = (char *)[conflictID UTF8String];
	resolveMessage.newLocalName = (char *)[newName UTF8String];

    err_code = soap_call___ns1__ResolveNameConflict(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &resolveMessage,
            &resolveResponse);

	handle_soap_error(soapData, @"iFolderService.ResolveNameConflict");

	unlockSoap(soapData);
}




-(void) RenameAndResolveConflict:(NSString *)ifolderID withID:(NSString *)conflictID usingFileName:(NSString *)newFileName
{
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (conflictID != nil), @"conflictID was nil");
	NSAssert( (newFileName != nil), @"newFileName was nil");

	struct _ns1__RenameAndResolveConflict			resolveMessage;
	struct _ns1__RenameAndResolveConflictResponse	resolveResponse;

	resolveMessage.iFolderID = (char *)[ifolderID UTF8String];
	resolveMessage.conflictID = (char *)[conflictID UTF8String];
	resolveMessage.newFileName = (char *)[newFileName UTF8String];

    err_code = soap_call___ns1__RenameAndResolveConflict(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &resolveMessage,
            &resolveResponse);

	handle_soap_error(soapData, @"iFolderService.RenameAndResolveConflict");

	unlockSoap(soapData);
}




-(void)SetUserRights:(NSString *)ifolderID forUser:(NSString *)userID withRights:(NSString *)rights
{
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (userID != nil), @"userID was nil");
	NSAssert( (rights != nil), @"rights was nil");

	struct _ns1__SetUserRights			setRightsMessage;
	struct _ns1__SetUserRightsResponse	setRightsResponse;

	setRightsMessage.iFolderID = (char *)[ifolderID UTF8String];
	setRightsMessage.UserID = (char *)[userID UTF8String];
	setRightsMessage.Rights = (char *)[rights UTF8String];

    err_code = soap_call___ns1__SetUserRights(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &setRightsMessage,
            &setRightsResponse);

	handle_soap_error(soapData, @"iFolderService.SetUserRights");

	unlockSoap(soapData);
}




-(void)ChanageOwner:(NSString *)ifolderID toUser:(NSString *)userID oldOwnerRights:(NSString *)rights
{
    int err_code;
	struct soap *pSoap = lockSoap(soapData);	

	NSAssert( (ifolderID != nil), @"ifolderID was nil");
	NSAssert( (userID != nil), @"userID was nil");
	NSAssert( (rights != nil), @"rights was nil");

	struct _ns1__ChangeOwner			changeOwnerMessage;
	struct _ns1__ChangeOwnerResponse	changeOwnerResponse;

	changeOwnerMessage.iFolderID = (char *)[ifolderID UTF8String];
	changeOwnerMessage.NewOwnerUserID = (char *)[userID UTF8String];
	changeOwnerMessage.OldOwnerRights = (char *)[rights UTF8String];

    err_code = soap_call___ns1__ChangeOwner(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &changeOwnerMessage,
            &changeOwnerResponse);

	handle_soap_error(soapData, @"iFolderService.ChanageOwner");
	unlockSoap(soapData);
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

		if(conflict->ConflictID != nil)
			[newProperties setObject:[NSString stringWithUTF8String:conflict->ConflictID] forKey:@"LocalConflictID"];
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

		if(conflict->ConflictID != nil)
			[newProperties setObject:[NSString stringWithUTF8String:conflict->ConflictID] forKey:@"ServerConflictID"];
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



//----------------------------------------------------------------------------
// handle_soap_error
// This will check the soap structure for any errors and throw an appropriate
// exception based on that error
//----------------------------------------------------------------------------
void handle_soap_error(void *soapData, NSString *methodName)
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
				unlockSoap(soapData);
				[NSException raise:[NSString stringWithFormat:@"%@", faultString]
						format:@"Exception in %@", methodName];
			}
			else
			{
				unlockSoap(soapData);
				[NSException raise:[NSString stringWithFormat:@"SOAP Error %d", error]
						format:@"SOAP error in %@", methodName];
			}
		}
		else if(soap_http_error_check(error))
		{
			unlockSoap(soapData);
			[NSException raise:[NSString stringWithFormat:@"HTTP Error %d", error]
					format:@"HTTP error in %@", methodName];
		}
		else if(soap_tcp_error_check(error))
		{
			unlockSoap(soapData);
			[NSException raise:[NSString stringWithFormat:@"TCP Error %d", error]
					format:@"TCP error in %@", methodName];
		}
		else if(soap_ssl_error_check(error))
		{
			unlockSoap(soapData);
			[NSException raise:[NSString stringWithFormat:@"SSL Error %d", error]
					format:@"SSL error in %@", methodName];
		}
		else if(soap_xml_error_check(error))
		{
			unlockSoap(soapData);
			[NSException raise:[NSString stringWithFormat:@"XML Error %d", error]
					format:@"XML error in %@", methodName];
		}
		else
		{
			unlockSoap(soapData);
			[NSException raise:[NSString stringWithFormat:@"Error %d", error]
					format:@"Error in %@", methodName];
		}
	}
}


struct soap *lockSoap(void *soapData)
{
	SOAP_DATA *pSoap = (SOAP_DATA *)soapData;

	[pSoap->instanceLock lock];
	
	return pSoap->soap;
}


void unlockSoap(void *soapData)
{
	SOAP_DATA *pSoap = (SOAP_DATA *)soapData;

	soap_end(pSoap->soap);

	[pSoap->instanceLock unlock];
}

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
}

@end
