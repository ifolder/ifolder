#include "simdezvous.h"

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

// Internal
typedef struct tagMemberBrowseContext
{
	MemberBrowseCallback	Callback;
	void					*pCallersCtx;
} MemberBrowseContext, *PMemberBrowseContext;

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
	void				*pCtx)
{
	PMemberInfoCtx pInfoCtx = ( PMemberInfoCtx) pCtx;
	unsigned char label[32];
	const unsigned char *src = ( unsigned char *) pTxt;
	unsigned char *dst;
	union { uint16_t s; u_char b[2]; } port = { opaqueport };
	uint16_t PortAsNumber = ((uint16_t)port.b[0]) << 8 | port.b[1];

	(void) client;       // Unused
	(void) ifIndex;      // Unused
	(void) txtLen;       // Unused

	//printf( "ResolveInfoCallback called" );
	//printf( "errorCode: %d\n", errorCode );

	pInfoCtx->CBError = errorCode;
	if ( errorCode == 0 )
	{
		*pInfoCtx->pPort = PortAsNumber;
		strcpy( pInfoCtx->pHost, pHostTarget );

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
					dst = (unsigned char *) pInfoCtx->pName;
				}
				else
				if ( strcmp( label, serviceLabel ) == 0 )
				{
					dst = (unsigned char *) pInfoCtx->pServicePath;
				}
				else
				if ( strcmp( label, keyLabel ) == 0 )
				{
					dst = (unsigned char *) pInfoCtx->pPublicKey;
				}
				else
				{
					pInfoCtx->CBError = -1;
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
ResolveInfoCallback2(
	DNSServiceRef		client, 
	DNSServiceFlags		flags, 
	uint32_t			ifIndex, 
	DNSServiceErrorType errorCode,
	const char			*pFullName, 
	const char			*pHostTarget, 
	uint16_t			opaqueport, 
	uint16_t			txtLen, 
	const char			*pTxt,
	void				*pCtx)
{
	PMemberInfoCtx2 pInfoCtx = ( PMemberInfoCtx2 ) pCtx;
	PMemberInfo pInfo = ( PMemberInfo ) pInfoCtx->pInfo;

	const char *src = pTxt;
	char *dst;
	union { uint16_t s; u_char b[2]; } port = { opaqueport };
	uint16_t PortAsNumber = ((uint16_t)port.b[0]) << 8 | port.b[1];

	(void) client;       // Unused
	(void) ifIndex;      // Unused
	(void) txtLen;       // Unused

	//printf( "ResolveInfoCallback called" );
	//printf( "errorCode: %d\n", errorCode );

	pInfoCtx->CBError = errorCode;
	if ( errorCode == 0 )
	{
		pInfo->Port = PortAsNumber;
		strcpy( pInfo->HostName, pHostTarget );

		//printf( "Host: %s\n", pInfo->HostName );

		// Parse TXT strings
		// iFolder Member TXT strings are registered in the following format:
		// MemberName=<member name>
		// ServicePath=<path>
		// PublicKey=<pub key>

		if (*src)
		{
			int	compLength;
			int inValue = 0;
		
			// Get the member name
			compLength = *src++;
			dst = pInfo->Name;
			while( compLength-- )
			{
				if ( *src == '=' )
				{
					inValue = 1;
					src++;
				}
				else
				if ( inValue == 1 )
				{
					*dst++ = *src++;
				}	
				else
				{
					src++;
				}
			}
			*dst = 0;
			//printf( "Member Name: %s\n", pInfo->Name );

			// Get the service path
			compLength = *src++;
			dst = pInfo->ServicePath;
			inValue = 0;
			while( compLength-- )
			{
				if ( *src == '=' )
				{
					inValue = 1;
					src++;
				}
				else
				if ( inValue == 1 )
				{
					*dst++ = *src++;
				}
				else
				{
					src++;
				}
			}
			*dst = 0;
			//printf( "Service Path: %s\n", pInfo->ServicePath );

			// Get the public key
			compLength = *src++;
			dst = pInfo->PublicKey;
			inValue = 0;
			while( compLength-- )
			{
				if ( *src == '=' )
				{
					inValue = 1;
					src++;
				}
				else
				if ( inValue == 1 )
				{
					*dst++ = *src++;
				}
				else
				{
					src++;
				}
			}
			*dst = 0;
			//printf( "Public Key: %s\n", pInfo->PublicKey );
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

	//printf( "Got a reply for %s.%s%s: ", pName, pType, pDomain );
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
	int txtLength;

	pid_t pid = getpid();

	// Valid Parameters?
	if ( pID == NULL || pName == NULL || 
		pServicePath == NULL || pPublicKey == NULL ||
		pCookie == NULL )
	{
		return -1;
	}

	/*
	if ( (strlen( pPublicKey) + strlen( keyLabel ) )PublicKeyLength == 0 )
	{
		return -1;
	}
	*/

	if ( ( strlen( pPublicKey ) + strlen( keyLabel ) ) > 255 )
	{
		strncpy( keyPartOne, pPublicKey, 250 );
		keyPartOne[250] = '\0';
		strcpy( keyPartTwo, &pPublicKey[250] );

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
				(unsigned char) ( strlen( pPublicKey ) + (int) sizeof( keyLabel ) - (int) 1 ),
				keyLabel,
				pPublicKey );
	}

	*pCookie = NULL;
	cbErr = kDNSServiceErr_Unknown;

	//Opaque16 registerPort = { { pid >> 8, pid & 0xFF } };
	err = 
		DNSServiceRegister(
			pCookie, 
			0, 
			kDNSServiceInterfaceIndexAny, 
			pID, 
			"_ifolder_member._tcp.", 
			NULL, //"", 
			NULL, 
			Port,
			//registerPort.NotAnInteger, 
			(uint16_t) txtLength,
			txtStrings, 
			RegistrationCallback, 
			(void *) (&cbErr));

	if ( err == 0 )
	{
		err = DNSServiceProcessResult( *pCookie );
		if ( err == 0 )
		{
			err = cbErr;
		}
	}

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

DNSServiceErrorType
DNSSD_API 
GetMemberInfo(
	char				*pID,
	char				*pName,
	char				*pServicePath,
	unsigned char		*pPublicKey,
	char				*pHost,
	int					*pPort)
{
	DNSServiceErrorType err;
	DNSServiceRef		client = NULL;
	MemberInfoCtx		infoCtx;

	// Valid Parameters?
	if ( pID == NULL || pName == NULL || pServicePath == NULL ||
		pPublicKey == NULL || pHost == NULL || pPort == NULL)
	{
		return kDNSServiceErr_BadParam;
	}

	infoCtx.CBError = kDNSServiceErr_Unknown;
	infoCtx.pName = pName;
	infoCtx.pServicePath = pServicePath;
	infoCtx.pPublicKey = pPublicKey;
	infoCtx.pHost = pHost;
	infoCtx.pPort = pPort;

	//printf(" GetMemberInfo:calling DNSServiceResolve");
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

		//	nfds = dns_sd_fd + 1;
		nfds = dns_sd_fd;

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
GetMemberInfo2(
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

	infoCtx.CBError = kDNSServiceErr_Unknown;
	infoCtx.pInfo = pInfo;

	printf(" GetMemberInfo:calling DNSServiceResolve");
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
		err = DNSServiceProcessResult( client );
		if ( err == kDNSServiceErr_NoError )
		{
			err = infoCtx.CBError;
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
	//printf( "BrowseMembersInit called\n" );

	err = 
		DNSServiceBrowse(
			&client, 
			0,
			kDNSServiceInterfaceIndexAny,
			"_ifolder_member._tcp",
			NULL, //dom,
			callback, 
			NULL);

	//printf( "err (from browse): %d\n", (long int) err);
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

	dns_sd_fd = DNSServiceRefSockFD( client );
	if ( dns_sd_fd == -1 )
	{
		return kDNSServiceErr_NotInitialized;
	}

//	nfds = dns_sd_fd + 1;
	nfds = dns_sd_fd;
	//printf("descriptor = %d\n", nfds);

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
//		result = select( nfds, &readfds, &readfds, &readfds, &tv);
//		result = select( nfds, &readfds, &readfds, &readfds, (struct timeval *)NULL );
	    if (result > 0)
		{
			if ( FD_ISSET( dns_sd_fd , &readfds ) )
			{	
				err = DNSServiceProcessResult( client );
			}

			if (err) 
			{ 
				fprintf( stderr, "DNSServiceProcessResult returned %d\n", err );
				stop = 1;
			}
		}
		else
		{
			printf("select() returned %d errno %d %s\n", result, errno, strerror( errno ) );
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

