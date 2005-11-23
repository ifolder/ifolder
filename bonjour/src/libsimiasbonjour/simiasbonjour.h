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
#include <dns_sd.h>

#include <ctype.h>
#include <stdio.h>			// For stdout, stderr
#include <stdlib.h>			// For exit()
#include <string.h>			// For strlen(), strcpy(), bzero()
#include <errno.h>          // For errno, EINTR
#include <time.h>
#include <sys/types.h>      // For u_char

#ifdef _WIN32
#include <process.h>
typedef	int	pid_t;
#define	getpid	_getpid
#define	strcasecmp	_stricmp
#define snprintf _snprintf
#else
#include <sys/time.h>		// For struct timeval
#include <unistd.h>         // For getopt() and optind
#include <arpa/inet.h>		// For inet_addr()
#include <netinet/in.h>		// for in_addr
#endif

#define	kDNSMaxID					32
#define	kSimiasMaxServicePath		128
#define	kSimiasMaxPublicKey			256
#define kSimiasMaxDescription		256
#define	kDNSMaxTextualIP			16

// Note: Bonjour's max domain name is 1005 but for now we're
// just going to support local. so our's is much smaller
#define	kSimiasMaxDomainName		128
#define	kSimiasMaxServiceName		128

#ifndef kDNSServiceInterfaceIndexAny 
#define kDNSServiceInterfaceIndexAny 0
#endif

/*
#ifndef DNSSD_API
#define DNSSD_API
#endif
*/

static const char memberType[] = "_ifolder_member._tcp";
static const char collectionType[] = "_simias_collection._tcp";
static const char domainType[] = "local.";

static const char memberLabel[] = "MemberName=";
static const char serviceLabel[] = "ServicePath=";
static const char collectionLabel[] = "Collection=";
static const char descriptionLabel[] = "Description=";
static const char keyLabel[] = "PK=";
static const char key2Label[] = "PK2=";

typedef void (DNSSD_API *MemberBrowseCallback)
(
    uint32_t            interfaceIndex,
	DNSServiceFlags		flags, 
    const char          *pName,
	void				*pCtx
);

typedef struct tagMembers
{
	char				ID[kDNSMaxID];
	char				Name[kSimiasMaxServiceName];
	char				HostName[kSimiasMaxDomainName];
	char				IPAddress[kDNSMaxTextualIP];
	char				ServicePath[kSimiasMaxServicePath];
	unsigned char		PublicKey[kSimiasMaxPublicKey];
	int					Port;

} Members, *PMembers;

typedef struct tagMemberInfo
{
	char				Name[kSimiasMaxServiceName];
	char				HostName[kSimiasMaxDomainName];
	char				ServicePath[kSimiasMaxServicePath];
	unsigned char		PublicKey[kSimiasMaxPublicKey];
	int			        Port;

} MemberInfo, *PMemberInfo;


typedef struct tagCollections
{
	char				ID[kDNSMaxID];
	char				Name[kSimiasMaxServiceName];
	char				HostName[kSimiasMaxDomainName];
	char				IPAddress[kDNSMaxTextualIP];
	char				ServicePath[kSimiasMaxServicePath];
	unsigned char		Description[kSimiasMaxDescription];
	int					Port;

} Collections, *PCollections;

typedef struct tagCollectionInfo
{
	char				Name[kSimiasMaxServiceName];
	char				HostName[kSimiasMaxDomainName];
	char				ServicePath[kSimiasMaxServicePath];
	char				Description[kSimiasMaxDescription];
	int			        Port;

} CollectionInfo, *PCollectionInfo;

extern
DNSServiceErrorType
DNSSD_API 
GetMemberInfo(char *pID, PMemberInfo pInfo);

extern
DNSServiceErrorType
DNSSD_API 
DeregisterLocalMember(char *pID, DNSServiceRef Cookie);

extern
DNSServiceErrorType
DNSSD_API 
RegisterLocalMember(
	char					*pID,
	char					*pName,
	short					Port,
	char					*pServicePath,
	unsigned char			*pPublicKey,
	DNSServiceRef			*pCookie);

extern
DNSServiceErrorType
DNSSD_API 
BrowseMembersInit(
	DNSServiceBrowseReply       callback,
	DNSServiceRef				*pHandle);

extern
DNSServiceErrorType
DNSSD_API 
BrowseMembers(DNSServiceRef client, int timeout);

extern
DNSServiceErrorType
DNSSD_API 
BrowseMembersShutdown(DNSServiceRef client);


