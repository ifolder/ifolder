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

#define	kDNSMaxID			32
#define	kDNSMaxServicePath	128
#define	kDNSMaxPublicKey	256
#define	kDNSMaxTextualIP	16

// Note: Rendezvous' max domain name is 1005 but for now we're
// just going to support local. so our's is much smaller
#define	kDNSMaxDomainName	64

#ifndef kDNSServiceMaxServiceName
#define kDNSServiceMaxServiceName 64
#endif

#ifndef kDNSServiceInterfaceIndexAny 
#define kDNSServiceInterfaceIndexAny 0
#endif

#ifndef DNSSD_API
#define DNSSD_API
#endif

static const char memberType[] = "_ifolder_member._tcp";
static const char domainType[] = "local.";

static const char memberLabel[] = "MemberName=";
static const char serviceLabel[] = "ServicePath=";
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
	char				Name[kDNSServiceMaxServiceName];
	char				IPAddress[kDNSMaxTextualIP];
	char				ServicePath[kDNSMaxServicePath];
	unsigned char		PublicKey[kDNSMaxPublicKey];
	char				HostName[kDNSMaxDomainName];
	int			Port;

} Members, *PMembers;

typedef struct tagMemberInfo
{
	char				Name[kDNSServiceMaxServiceName];
	char				ServicePath[kDNSMaxServicePath];
	unsigned char		PublicKey[kDNSMaxPublicKey];
	char				HostName[kDNSMaxDomainName];
	int			Port;

} MemberInfo, *PMemberInfo;

/*
extern
DNSServiceErrorType
DNSSD_API 
GetMembers(
	int					timeout,
	int					bufferLength,
	PMembers			pMembers,
	int					*pMembersAdded);
*/

extern
DNSServiceErrorType
DNSSD_API 
GetMemberInfo2(char *pID, PMemberInfo pInfo);

DNSServiceErrorType
DNSSD_API 
GetMemberInfo(
	char				*pID,
	char				*pName,
	char				*pServicePath,
	unsigned char		*pPublicKey,
	char				*pHost,
	int					*pPort);

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


