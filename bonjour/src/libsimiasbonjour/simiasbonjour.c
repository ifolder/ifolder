/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

#include "simiasbonjour.h"

typedef struct tagMemberInfoCtx2
{
	DNSServiceErrorType		CBError;
	PMemberInfo				pInfo;

} MemberInfoCtx2, *PMemberInfoCtx2;

typedef struct tagMemberInfoCtx
{
	DNSServiceErrorType		CBError;
	char					*pName;
	char					*pServicePath;
	unsigned char			*pPublicKey;
	char					*pHost;
	int						*pPort;
} MemberInfoCtx, *PMemberInfoCtx;

typedef struct tagCollectionInfoCtx
{
	DNSServiceErrorType		CBError;
	PCollectionInfo			pInfo;

} CollectionInfoCtx, *PCollectionInfoCtx;

// Internal
typedef struct tagMemberBrowseContext
{
	MemberBrowseCallback	Callback;
	void					*pCallersCtx;
} MemberBrowseContext, *PMemberBrowseContext;

static
void
DNSSD_API
ResolveCollectionInfoCallback(
	DNSServiceRef		client, 
	DNSServiceFlags		flags, 
	uint32_t			ifIndex, 
	DNSServiceErrorType errorCode,
	const char			*pFullName, 
	const char			*pHostTarget, 
	uint16_t			opaqueport, 
	uint16_t			txtLen, 
	const char			*pTxt,
	void				*pContext)
{
	PCollectionInfoCtx pCtx = ( PCollectionInfoCtx ) pContext;
	unsigned char label[32];
	const unsigned char *src = ( unsigned char *) pTxt;
	unsigned char *dst;
	union { uint16_t s; u_char b[2]; } port = { opaqueport };
	uint16_t PortAsNumber = ((uint16_t)port.b[0]) << 8 | port.b[1];

	(void) client;       // Unused
	(void) ifIndex;      // Unused

	printf( "ResolveCollectionInfoCallback called" );
	printf( "errorCode: %d\n", errorCode );

	pCtx->CBError = errorCode;
	if ( errorCode == 0 )
	{
		pCtx->pInfo->Port = ( int ) PortAsNumber;
		strcpy( pCtx->pInfo->HostName, pHostTarget );

		//printf( "Host: %s\n", pInfoCtx->pHost );

		// Parse TXT strings
		// iFolder Member TXT strings are registered in the following format:
		// CollectionName=<collection name>
		// Description=<collection descriptoin>
		// ServicePath=<path>

		if (*src)
		{
			int total = 0;
			int	compLength;
			while ( ( total < txtLen ) && *src != '\0' )
			{
				compLength = *src++;
				total += compLength;

				// can't be good
				if ( total > txtLen )
				{
					continue;
				}

				// Get the label
				dst = label;
				while( *src != '=' && *src != '\0' && compLength > 0 )
				{
					*dst++ = *src++;
					compLength--;
				}

				// Past the = and null terminate
				*dst++ = *src++;
				compLength--;
				*dst = 0;

				if ( strcmp( (char *) label, collectionLabel ) == 0 )
				{
					dst = (unsigned char *) pCtx->pInfo->Name;
				}
				else
				if ( strcmp( (char *) label, descriptionLabel ) == 0 )
				{
					dst = (unsigned char *) pCtx->pInfo->Description;
				}
				else
				if ( strcmp( (char *) label, serviceLabel ) == 0 )
				{
					dst = (unsigned char *) pCtx->pInfo->ServicePath;
				}
				else
				{
					pCtx->CBError = -1;
					return;
				}

				while( ( compLength > 0 ) && *src != '\0' )
				{
					*dst++ = *src++;
					compLength--;
				}

				*dst = 0;
			}
		}
	}
}


static
void
DNSSD_API
ResolveInfoCallback(
	DNSServiceRef		client, 
	DNSServiceFlags		flags, 
	uint32_t			ifIndex, 
	DNSServiceErrorType errorCode,
	const char			*pFullName, 
	const char			*pHostTarget, 
	uint16_t			opaqueport, 
	uint16_t			txtLen, 
	const char			*pTxt,
	void				*pContext)
{
	PMemberInfoCtx2 pCtx = ( PMemberInfoCtx2 ) pContext;
	unsigned char label[32];
	const unsigned char *src = ( unsigned char *) pTxt;
	unsigned char *dst;
	union { uint16_t s; u_char b[2]; } port = { opaqueport };
	uint16_t PortAsNumber = ((uint16_t)port.b[0]) << 8 | port.b[1];

	(void) client;       // Unused
	(void) ifIndex;      // Unused

	//printf( "ResolveInfoCallback called" );
	//printf( "errorCode: %d\n", errorCode );

	pCtx->CBError = errorCode;
	if ( errorCode == 0 )
	{
		pCtx->pInfo->Port = ( int ) PortAsNumber;
		strcpy( pCtx->pInfo->HostName, pHostTarget );

		//printf( "Host: %s\n", pInfoCtx->pHost );

		// Parse TXT strings
		// iFolder Member TXT strings are registered in the following format:
		// MemberName=<member name>
		// ServicePath=<path>
		// PK=<pub key>
		// PK2=<rest of the public key>

		if (*src)
		{
			int total = 0;
			int	compLength;
			while ( ( total < txtLen ) && *src != '\0' )
			{
				compLength = *src++;
				total += compLength;

				// can't be good
				if ( total > txtLen )
				{
					continue;
				}

				// Get the label
				dst = label;
				while( *src != '=' && *src != '\0' && compLength > 0 )
				{
					*dst++ = *src++;
					compLength--;
				}

				// Past the = and null terminate
				*dst++ = *src++;
				compLength--;
				*dst = 0;

				if ( strcmp( (char *) label, memberLabel ) == 0 )
				{
					dst = (unsigned char *) pCtx->pInfo->Name;
				}
				else
				if ( strcmp( (char *) label, serviceLabel ) == 0 )
				{
					dst = (unsigned char *) pCtx->pInfo->ServicePath;
				}
				else
				if ( strcmp( (char *) label, keyLabel ) == 0 )
				{
					dst = (unsigned char *) pCtx->pInfo->PublicKey;
				}
				else
				{
					pCtx->CBError = -1;
					return;
				}

				while( ( compLength > 0 ) && *src != '\0' )
				{
					*dst++ = *src++;
					compLength--;
				}

				*dst = 0;
			}
		}
	}
}


static
void 
DNSSD_API 
RegistrationCallback(
	DNSServiceRef		client, 
	DNSServiceFlags		flags, 
	DNSServiceErrorType errorCode,
	const char			*pName, 
	const char			*pType, 
	const char			*pDomain, 
	void				*pCtx)
{
	int *cbErr = (int *) pCtx;

	(void) client; // Unused
	(void) flags;  // Unused

	//printf( "Got a reply for %s.%s%s: \n", pName, pType, pDomain );
	printf( "  RegistrationCallback::errorCode: %d\n", errorCode );
	*cbErr = errorCode;
}

DNSServiceErrorType
DNSSD_API 
RegisterLocalMember(
	char				*pID,
	char				*pName,
	short				Port,
	char				*pServicePath,
	unsigned char		*pPublicKey,
	DNSServiceRef		*pCookie)
{
	char				keyPartOne[256];
	char				keyPartTwo[256];
	char				txtStrings[2048];
	DNSServiceErrorType err;
	DNSServiceErrorType	cbErr;
	DNSServiceFlags		flags;
	int txtLength;

	printf( "RegisterLocalMember - called\n" );

	pid_t pid = getpid();

	// Valid Parameters?
	if ( pID == NULL || pName == NULL || 
		pServicePath == NULL || pPublicKey == NULL ||
		pCookie == NULL )
	{
		return kDNSServiceErr_BadParam;
	}

	if ( ( strlen( (char *) pPublicKey ) + strlen( keyLabel ) ) > 255 )
	{
		strncpy( keyPartOne, (char *) pPublicKey, 250 );
		keyPartOne[250] = '\0';
		strcpy( keyPartTwo, (char *) &pPublicKey[250] );

		txtLength =
			sprintf( 
				txtStrings, 
				"%c%s%s%c%s%s%c%s%s%c%s%s", 
				(unsigned char) ( strlen( pName ) + sizeof( memberLabel ) - 1 ),
				memberLabel,
				pName,
				(unsigned char) ( strlen( pServicePath ) + sizeof( serviceLabel ) - 1 ),
				serviceLabel,
				pServicePath,
				(unsigned char) ( (int) 250 + (int) sizeof( keyLabel ) - (int) 1),
				keyLabel,
				keyPartOne,
				(unsigned char) ( strlen( keyPartTwo ) + sizeof( key2Label ) - 1 ),
				key2Label,
				keyPartTwo );
	}
	else
	{
		txtLength =
			sprintf( 
				txtStrings, 
				"%c%s%s%c%s%s%c%s%s", 
				(unsigned char) ( strlen( pName ) + sizeof( memberLabel ) - 1 ),
				memberLabel,
				pName,
				(unsigned char) ( strlen( pServicePath ) + sizeof( serviceLabel ) - 1 ),
				serviceLabel,
				pServicePath,
				(unsigned char) ( strlen( (char *) pPublicKey ) + (int) sizeof( keyLabel ) - (int) 1 ),
				keyLabel,
				pPublicKey );
	}

	*pCookie = NULL;
	cbErr = kDNSServiceErr_Unknown;
	flags = kDNSServiceFlagsNoAutoRename;
	
	//Opaque16 registerPort = { { pid >> 8, pid & 0xFF } };
	err = 
		DNSServiceRegister(
			pCookie, 
			flags,
			kDNSServiceInterfaceIndexAny, 
			pID, 
			memberType,
			NULL, //"", 
			NULL, 
			Port,
			//registerPort.NotAnInteger, 
			(uint16_t) txtLength,
			txtStrings, 
			RegistrationCallback, 
			(void *) &cbErr );

	if ( err == 0 )
	{
		printf( "  DNSServiceRegister returned success\n" );
		err = DNSServiceProcessResult( *pCookie );
		if ( err == 0 )
		{
			err = cbErr;
		}
	}
	else
	{
		printf( "  DNSServiceRegister failed - status: %d\n", err );
	}

	printf( "RegisterLocalMember exit - returning: %d\n", err );
	return (int) err;
}


DNSServiceErrorType
DNSSD_API 
DeregisterLocalMember(
	char				*pID,
	DNSServiceRef		Cookie)
{
	DNSServiceRefDeallocate( Cookie );
	return 0;
}

/*
	Register a published collection with the MDNS network
*/

DNSServiceErrorType
DNSSD_API 
RegisterCollection(
	char				*pID,
	char				*pName,
	char				*pDescription,
	short				Port,
	char				*pServicePath,
	DNSServiceRef		*pCookie)
{
	//char				keyPartOne[256];
//	char				keyPartTwo[256];
	char				txtStrings[2048];
	DNSServiceErrorType err;
	DNSServiceErrorType	cbErr;
	DNSServiceFlags		flags;
	int txtLength;

	printf( "RegisterCollection - called\n" );

	pid_t pid = getpid();

	// Valid Parameters?
	if ( pID == NULL || pName == NULL || pDescription == NULL ||
		pServicePath == NULL || pCookie == NULL )
	{
		return kDNSServiceErr_BadParam;
	}

	txtLength =
		sprintf( 
			txtStrings, 
			"%c%s%s%c%s%s%c%s%s", 
			(unsigned char) ( strlen( pName ) + sizeof( collectionLabel ) - 1 ),
			collectionLabel,
			pName,
			(unsigned char) ( strlen( pDescription ) + sizeof( descriptionLabel ) - 1 ),
			descriptionLabel,
			pDescription,
			(unsigned char) ( strlen( pServicePath ) + sizeof( serviceLabel ) - 1 ),
			serviceLabel,
			pServicePath);

	*pCookie = NULL;
	cbErr = kDNSServiceErr_Unknown;
	flags = kDNSServiceFlagsNoAutoRename;
	
	//Opaque16 registerPort = { { pid >> 8, pid & 0xFF } };
	err = 
		DNSServiceRegister(
			pCookie, 
			flags,
			kDNSServiceInterfaceIndexAny, 
			pID, 
			collectionType,
			NULL, //"", 
			NULL, 
			Port,
			//registerPort.NotAnInteger, 
			(uint16_t) txtLength,
			txtStrings, 
			RegistrationCallback, 
			(void *) &cbErr );

	if ( err == 0 )
	{
		printf( "  DNSServiceRegister returned success\n" );
		err = DNSServiceProcessResult( *pCookie );
		if ( err == 0 )
		{
			err = cbErr;
		}
	}
	else
	{
		printf( "  DNSServiceRegister failed - status: %d\n", err );
	}

	printf( "RegisterCollection exit - returning: %d\n", err );
	return (int) err;
}

DNSServiceErrorType
DNSSD_API 
DeregisterCollection(
	char				*pID,
	DNSServiceRef		Cookie)
{
	DNSServiceRefDeallocate( Cookie );
	return 0;
}


DNSServiceErrorType
DNSSD_API 
GetMemberInfo(
	char				*pID,
	PMemberInfo			pInfo)
{
	DNSServiceErrorType err;
	DNSServiceRef		client = NULL;
	MemberInfoCtx2		infoCtx;

	// Valid Parameters?
	if ( pID == NULL || pInfo == NULL )
	{
		return kDNSServiceErr_BadParam;
	}

	//printf(" GetMemberInfo:calling DNSServiceResolve");

	infoCtx.pInfo = pInfo;

	err = 
		DNSServiceResolve(
			&client, 
			0, 
			kDNSServiceInterfaceIndexAny,
			pID,
			memberType,
			domainType,
			ResolveInfoCallback,
			(void *) &infoCtx );

	if ( err == kDNSServiceErr_NoError )
	{
		fd_set				readfds;
		int					dns_sd_fd;
		int					nfds;
		int					result;
		int					stop = 0;
		struct timeval		tv;

		dns_sd_fd = DNSServiceRefSockFD( client );
		if ( dns_sd_fd == -1 )
		{
			return kDNSServiceErr_NotInitialized;
		}

		nfds = dns_sd_fd + 1;
		while ( !stop )
		{
			// 1. Set up the fd_set as usual here.
			// This example client has no file descriptors of its own,
			// but a real application would call FD_SET to add them to the set here
			FD_ZERO( &readfds );

			// 2. Add the fd for our client(s) to the fd_set
			FD_SET( dns_sd_fd, &readfds );

			// 3. Set up the timeout.
			tv.tv_sec = 10; // this resolve should succeed quickly
			tv.tv_usec = 0;

			err = kDNSServiceErr_NoError;
			result = select( nfds, &readfds, (fd_set*) NULL, (fd_set*) NULL, &tv );
			if ( result > 0 )
			{
				if ( FD_ISSET( dns_sd_fd , &readfds ) )
				{	
					err = DNSServiceProcessResult( client );
					if ( err == kDNSServiceErr_NoError )
					{
						err = infoCtx.CBError;
						if ( err == kDNSServiceErr_NoError )
						{
							stop = 1;
						}
					}
				}

				if ( err != kDNSServiceErr_NoError ) 
				{ 
					//fprintf( stderr, "DNSServiceProcessResult returned %d\n", err );
					stop = 1;
				}
			}
			else
			{
				//printf("select() returned %d errno %d %s\n", result, errno, strerror( errno ) );
				if ( errno != EINTR )
				{
					stop = 1;
				}
			}
		}

		DNSServiceRefDeallocate( client );
	}

	return err;
}


DNSServiceErrorType
DNSSD_API 
BrowseMembersInit(
	DNSServiceBrowseReply       callback,
	DNSServiceRef				*pHandle)
{
	DNSServiceErrorType err;
	DNSServiceRef		client;
	//PMemberBrowseContext *pCtx;

	err = 
		DNSServiceBrowse(
			&client, 
			0,
			kDNSServiceInterfaceIndexAny,
			memberType,
			NULL, //dom,
			callback, 
			NULL);

	if ( err == 0 )
	{
		*pHandle = client;
	}

	return err;
}


DNSServiceErrorType
DNSSD_API 
BrowseMembers(DNSServiceRef client, int timeout)
{
	DNSServiceErrorType err;
	fd_set				readfds;
	int					dns_sd_fd;
	int					nfds;
	int					result;
	int					stop = 0;
	struct timeval		tv;
	
	//printf( "BrowseMembers called\n" );
	//printf( "  handle: %u\n", client );

	dns_sd_fd = DNSServiceRefSockFD( client );
	if ( dns_sd_fd == -1 )
	{
		return kDNSServiceErr_NotInitialized;
	}

	nfds = dns_sd_fd + 1;

	while ( !stop )
	{
		// 1. Set up the fd_set as usual here.
		// This example client has no file descriptors of its own,
		// but a real application would call FD_SET to add them to the set here
		FD_ZERO( &readfds );

		// 2. Add the fd for our client(s) to the fd_set
		FD_SET( dns_sd_fd, &readfds );

		// 3. Set up the timeout.
		tv.tv_sec = timeout;
		tv.tv_usec = 0;

		err = kDNSServiceErr_NoError;
		result = select( nfds, &readfds, (fd_set*) NULL, (fd_set*) NULL, &tv );
	    if (result > 0)
		{
			if ( FD_ISSET( dns_sd_fd , &readfds ) )
			{	
				//printf( "calling DNSServiceProcessResult\n" );
				err = DNSServiceProcessResult( client );
			}

			if (err) 
			{ 
				//fprintf( stderr, "DNSServiceProcessResult returned %d\n", err );
				stop = 1;
			}
		}
		else
		{
			//printf("select() returned %d errno %d %s\n", result, errno, strerror( errno ) );
			if ( errno != EINTR )
			{
				stop = 1;
			}
		}
	}

	return err;
}

DNSServiceErrorType
DNSSD_API 
BrowseMembersShutdown(DNSServiceRef client)
{
	//printf( "BrowseMembersShutdown called\n" );
	
	DNSServiceRefDeallocate( client );
	return kDNSServiceErr_NoError;
}

DNSServiceErrorType
DNSSD_API 
GetCollectionInfo(
	char				*pID,
	PCollectionInfo		pInfo)
{
	DNSServiceErrorType err;
	DNSServiceRef		client = NULL;
	CollectionInfoCtx	infoCtx;

	// Valid Parameters?
	if ( pID == NULL || pInfo == NULL )
	{
		return kDNSServiceErr_BadParam;
	}

	printf( "GetCollectionInfo:calling DNSServiceResolve" );

	infoCtx.pInfo = pInfo;

	err = 
		DNSServiceResolve(
			&client, 
			0, 
			kDNSServiceInterfaceIndexAny,
			pID,
			collectionType,
			domainType,
			ResolveCollectionInfoCallback,
			(void *) &infoCtx );

	if ( err == kDNSServiceErr_NoError )
	{
		fd_set				readfds;
		int					dns_sd_fd;
		int					nfds;
		int					result;
		int					stop = 0;
		struct timeval		tv;

		dns_sd_fd = DNSServiceRefSockFD( client );
		if ( dns_sd_fd == -1 )
		{
			return kDNSServiceErr_NotInitialized;
		}

		nfds = dns_sd_fd + 1;
		while ( !stop )
		{
			// 1. Set up the fd_set as usual here.
			// This example client has no file descriptors of its own,
			// but a real application would call FD_SET to add them to the set here
			FD_ZERO( &readfds );

			// 2. Add the fd for our client(s) to the fd_set
			FD_SET( dns_sd_fd, &readfds );

			// 3. Set up the timeout.
			tv.tv_sec = 10; // this resolve should succeed quickly
			tv.tv_usec = 0;

			err = kDNSServiceErr_NoError;
			result = select( nfds, &readfds, (fd_set*) NULL, (fd_set*) NULL, &tv );
			if ( result > 0 )
			{
				if ( FD_ISSET( dns_sd_fd , &readfds ) )
				{	
					err = DNSServiceProcessResult( client );
					if ( err == kDNSServiceErr_NoError )
					{
						err = infoCtx.CBError;
						if ( err == kDNSServiceErr_NoError )
						{
							stop = 1;
						}
					}
				}

				if ( err != kDNSServiceErr_NoError ) 
				{ 
					//fprintf( stderr, "DNSServiceProcessResult returned %d\n", err );
					stop = 1;
				}
			}
			else
			{
				//printf("select() returned %d errno %d %s\n", result, errno, strerror( errno ) );
				if ( errno != EINTR )
				{
					stop = 1;
				}
			}
		}

		DNSServiceRefDeallocate( client );
	}

	return err;
}

DNSServiceErrorType
DNSSD_API 
BrowseCollectionsInit(
	DNSServiceBrowseReply       callback,
	DNSServiceRef				*pHandle)
{
	DNSServiceErrorType err;
	DNSServiceRef		client;
	//PMemberBrowseContext *pCtx;

	printf( "BrowseCollectionsInit called\n" );
	err = 
		DNSServiceBrowse(
			&client, 
			0,
			kDNSServiceInterfaceIndexAny,
			collectionType,
			NULL, //dom,
			callback, 
			NULL);

	if ( err == 0 )
	{
		*pHandle = client;
	}

	printf( "BrowseCollections exit  status: %d\n", err );
	return err;
}

DNSServiceErrorType
DNSSD_API 
BrowseCollections(DNSServiceRef client, int timeout)
{
	DNSServiceErrorType err;
	fd_set				readfds;
	int					dns_sd_fd;
	int					nfds;
	int					result;
	int					stop = 0;
	struct timeval		tv;
	
	printf( "BrowseCollections called\n" );
	printf( "  handle: %u\n", client );

	dns_sd_fd = DNSServiceRefSockFD( client );
	if ( dns_sd_fd == -1 )
	{
		return kDNSServiceErr_NotInitialized;
	}

	nfds = dns_sd_fd + 1;
	while ( !stop )
	{
		// 1. Set up the fd_set as usual here.
		// This example client has no file descriptors of its own,
		// but a real application would call FD_SET to add them to the set here
		FD_ZERO( &readfds );

		// 2. Add the fd for our client(s) to the fd_set
		FD_SET( dns_sd_fd, &readfds );

		// 3. Set up the timeout.
		tv.tv_sec = timeout;
		tv.tv_usec = 0;

		err = kDNSServiceErr_NoError;
		result = select( nfds, &readfds, (fd_set*) NULL, (fd_set*) NULL, &tv );
	    if (result > 0)
		{
			if ( FD_ISSET( dns_sd_fd , &readfds ) )
			{	
				//printf( "calling DNSServiceProcessResult\n" );
				err = DNSServiceProcessResult( client );
			}

			if (err) 
			{ 
				//fprintf( stderr, "DNSServiceProcessResult returned %d\n", err );
				stop = 1;
			}
		}
		else
		{
			//printf("select() returned %d errno %d %s\n", result, errno, strerror( errno ) );
			if ( errno != EINTR )
			{
				stop = 1;
			}
		}
	}

	return err;
}

DNSServiceErrorType
DNSSD_API 
BrowseCollectionsShutdown(DNSServiceRef client)
{
	printf( "BrowseCollectionsShutdown called\n" );
	
	DNSServiceRefDeallocate( client );
	return kDNSServiceErr_NoError;
}



