/******************************************************************************
  Source module name:  ws2nlm.h
  Release Version:     1.08

  %name:ws2nlm.h %
  %version:1.5.1 %
  %date_modified:Sat Mar 11 14:33:28 2000 %
  $Copyright:

  Copyright (c) 1989-1997 Novell, Inc.  All Rights Reserved.                      

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

#if !defined(WS2NLM_H)
#define WS2NLM_H

#define _INC_WINDOWS


// New Address Types
#define AF_INET_ACP 25
#define AF_IPX_ACP  26
#define AF_ACP		  27

// NetWare Call back IOCTL Flags
#define SIO_RAWCALLBACKS ((DWORD)(0x1ADD0002))
#define SIO_WORKTODOCALLBACKS ((DWORD)(0x1ADD0004))

// NetWare Call WSASocket Flags
#define SKTS_RAWCALLBACKS ((DWORD)(0x40000000))
#define SKTS_WORKTODOCALLBACKS ((DWORD)(0x20000000))

// System flags not defined in NetWare
#define INFINITE 0xFFFFFFFF
#define WAIT_OBJECT_0 0
#define WAIT_ABANDONED ((DWORD)0x00000080L)
#define WAIT_TIMEOUT ((DWORD)0x00000102L)


//Various Types that may not be defined
#ifndef FARPROC
#define FARPROC void *
#endif
#ifndef FAR
#define FAR
#endif
#ifndef PASCAL
#define PASCAL
#endif
#ifndef CALLBACK
#define CALLBACK
#endif

#ifndef BYTE
typedef unsigned char BYTE;
#endif

#ifndef wsnchar
typedef unsigned char wsnchar;
#endif

#ifndef BOOL
typedef unsigned int BOOL;
#endif

#ifndef WORD
typedef unsigned short WORD;
#endif

#ifndef DWORD
typedef unsigned int DWORD;
#endif

#ifndef LPDWORD
typedef unsigned int *LPDWORD;
#endif

#ifndef ULONG
typedef unsigned long ULONG;
#endif

//
//
// Predefined Value Types.
//

#define REG_NONE                    ( 0 )   // No value type
#define REG_SZ                      ( 1 )   // Unicode nul terminated string
#define REG_EXPAND_SZ               ( 2 )   // Unicode nul terminated string
                                            // (with environment variable references)
#define REG_BINARY                  ( 3 )   // Free form binary
#define REG_DWORD                   ( 4 )   // 32-bit number
#define REG_DWORD_LITTLE_ENDIAN     ( 4 )   // 32-bit number (same as REG_DWORD)
#define REG_DWORD_BIG_ENDIAN        ( 5 )   // 32-bit number
#define REG_LINK                    ( 6 )   // Symbolic Link (unicode)
#define REG_MULTI_SZ                ( 7 )   // Multiple Unicode strings
#define REG_RESOURCE_LIST           ( 8 )   // Resource list in the resource map
#define REG_FULL_RESOURCE_DESCRIPTOR ( 9 )  // Resource list in the hardware description
#define REG_RESOURCE_REQUIREMENTS_LIST ( 10 )

#ifdef UNICODE
typedef unsigned short *LPTSTR;
#else
typedef char *LPTSTR;
#endif

typedef wchar_t	WCHAR;
typedef WCHAR	*LPWSTR;
typedef char	*LPBYTE;

//typedef int INT;
typedef char *LPSTR;

typedef int *LPINT;
typedef void *LPVOID;
typedef wsnchar CHAR;


#define HWND void *
#define HANDLE void *
typedef HANDLE  *LPHANDLE;

#ifndef IN
#define IN
#endif

#ifndef OUT
#define OUT
#endif

#if !defined (WIN32)
#define WIN32
#endif

typedef struct _OVERLAPPED {
    DWORD   Internal;
    DWORD   InternalHigh;
    DWORD   Offset;
    DWORD   OffsetHigh;
    HANDLE  hEvent;
} OVERLAPPED, *LPOVERLAPPED;
#define ERROR_IO_PENDING                 997L    // dderror

#define ERROR_OPERATION_ABORTED          995L

// BSD sockets Clashes that must first be maped to
// WS2_32.NLM calls before they can be converted to Winsock 2.
  
#define htonl WS2_32_htonl 
#define ntohl WS2_32_ntohl 
#define htons WS2_32_htons 
#define ntohs WS2_32_ntohs 
#define send WS2_32_send
#define recv WS2_32_recv
#define bind WS2_32_bind
#define listen WS2_32_listen
#define closesocket WS2_32_closesocket
#define getsockname WS2_32_getsockname
#define getpeername WS2_32_getpeername
#define getsockopt WS2_32_getsockopt
#define recvfrom WS2_32_recvfrom
#define select WS2_32_select
#define sendto WS2_32_sendto
#define setsockopt WS2_32_setsockopt
#define socket WS2_32_socket
#define inet_addr WS2_32_inet_addr
#define inet_ntoa WS2_32_inet_ntoa
#define gethostbyaddr WS2_32_gethostbyaddr
#define gethostbyname WS2_32_gethostbyname
#define gethostname WS2_32_gethostname
#define getprotobyname WS2_32_getprotobyname
#define getprotobynumber WS2_32_getprotobynumber
#define getservbyname WS2_32_getservbyname
#define getservbyport WS2_32_getservbyport


#include <winsock2.h>            // Winsock 2 extensions.

// BSD sockets Clashes that can be maped directly to WSA calls.
#define connect(s,name,namelen) WSAConnect(s,name,namelen, 0,0,0,0)
#define accept(s,addr,addrlen) WSAAccept(s,addr,addrlen,0,0)


#undef WIN32
#endif
