#ifndef _NWCONN_H_
#define _NWCONN_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwconn.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

/* Structures and typedefs for connection services */
#define IPX_TRANSPORT_ADDRESS 1
#define IPX_TRANSPORT_LENGTH  12

#define UDP_TRANSPORT_ADDRESS 8
#define UDP_TRANSPORT_LENGTH  4

#define TCP_TRANSPORT_ADDRESS 9
#define TCP_TRANSPORT_LENGTH  4

#include <npackon.h>

struct UserNameStruct
{
   BYTE   UserName[48];
   LONG   ObjectID;
};

#include <npackoff.h>

#ifdef __cplusplus
extern "C"
{
#endif

extern int AttachByAddress
(
   BYTE  transType,
   LONG  transLen,
   const BYTE *transBuf,
   WORD *fileServerID
);

extern int AttachToFileServer
(
   const char *fileServerName,
   WORD *fileServerID
);

extern int GetConnectionFromID
(
   WORD       *fileServerID
);

extern int GetConnectionInformation
(
   WORD  connectionNumber,
   char *objectName,
   WORD *objectType,
   long *objectID,
   BYTE *loginTime
);

extern int GetConnectionList
(
   LONG  objectID,
   LONG  lastConnection,
   LONG *numberOfConnections,
   LONG *connectionList,
   LONG  connectionSize
);

extern WORD GetConnectionNumber
(
   void
);

extern int GetDefaultConnectionID
(
   void
);

extern int GetDefaultFileServerID
(
   void
);

extern int GetFileServerID
(
   const char *fileServerName,
   WORD       *fileServerID
);

extern int GetInternetAddress
(
   WORD  connectionNumber,
   char *networkNumber,
   char *physicalNodeAddress
);

extern int GetLANAddress
(
   LONG  boardNumber,
   BYTE *nodeAddress
);

extern int GetMaximumNumberOfStations
(
   void
);

extern LONG GetNetNumber
(
   int   boardNumber
);

extern int GetObjectConnectionNumbers
(
   const char *objectName,
   WORD  objectType,
   WORD *numberOfConnections,
   WORD *connectionList,
   WORD  maxConnections
);

extern void GetStationAddress
(
   BYTE *physicalNodeAddress
);

extern int GetUserNameFromNetAddress
(
   const BYTE            *internetAddress,
   int                    sequenceNumber,
   struct UserNameStruct *userNameP
);

extern int LoginToFileServer
(
   const char *objectName,
   WORD  objectType,
   const char *objectPassword
);

extern void Logout
(
   void
);

extern void LogoutFromFileServer
(
   WORD fileServerID
);

extern int NWDSGetCurrentUser
(
   void
);

extern int NWDSSetCurrentUser
(
   int userHandle
);

extern int NWDSSetPreferredDSTree
(
   int         length,
   const char *treeName
);

extern LONG NWGetPacketBurstBufferCount(void);

extern int NWGetSecurityLevel(void);

extern int NWNCPSend
(
   BYTE        functionCode,
   const void *sendPacket,
   WORD        sendLen,
   void       *replyBuf,
   WORD        replyLen
);

extern LONG NWSetPacketBurstBufferCount(LONG numberOfBuffers);

extern int NWSetSecurityLevel
(
   int SecurityLevel
);

extern int SetConnectionCriticalErrorHandler
(
   int (*func)( int fileServerID, int connection, int err )
);

#ifdef __cplusplus
}
#endif


#endif
