/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwenvrn.h
==============================================================================
*/

#ifndef _NWENVRN_H_
#define _NWENVRN_H_

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>


#define TYPE_NORMAL_SERVER 0
#define TYPE_IO_ENGINE     1
#define TYPE_MS_ENGINE     2

#define LOADER_TYPE_DOS     1
#define LOADER_TYPE_OS2     2
#define LOADER_TYPE_MSWIN31 3

#define RSUPER 0x03

#include <npackon.h>

typedef struct tagFILE_SERV_INFO
{
   char    serverName[48];
   BYTE    netwareVersion;
   BYTE    netwareSubVersion;
   WORD    maxConnectionsSupported;
   WORD    connectionsInUse;
   WORD    maxVolumesSupported;
   BYTE    revisionLevel;
   BYTE    SFTLevel;
   BYTE    TTSLevel;
   WORD    peakConnectionsUsed;
   BYTE    accountingVersion;
   BYTE    VAPversion;
   BYTE    queingVersion;
   BYTE    printServerVersion;
   BYTE    virtualConsoleVersion;
   BYTE    securityRestrictionLevel;
   BYTE    internetBridgeSupport;
   BYTE    reserved[60];
   BYTE    CLibMajorVersion;
   BYTE    CLibMinorVersion;
   BYTE    CLibRevision;
} FILE_SERV_INFO;

#include <npackoff.h>

#ifdef __cplusplus
extern "C" {
#endif

extern int CheckConsolePrivileges
(
   void
);

extern int CheckNetWareVersion
(
   WORD majorVersion,  
   WORD minorVersion,
   WORD revisionNumber,  
   WORD minimumSFTLevel,
   WORD minimumTTSLevel
);

extern int ClearConnectionNumber
(
   WORD connectionNumber
);

extern int DisableFileServerLogin
(
   void
);

extern int DisableTransactionTracking
(
   void
);

extern int DownFileServer
(
   int forceFlag
);

extern int EnableFileServerLogin
(
   void
);

extern int EnableTransactionTracking
(
   void
);

extern int GetBinderyObjectDiskSpaceLeft
(
   WORD  connectionID,
   long  binderyObjectID,
   long *systemElapsedTime,
   long *unusedDiskBlocks,
   BYTE *restrictionEnforced
);

extern int GetDiskUtilization
(
   long  objectID, 
   char  volumeNumber,
   LONG *usedDirectories, 
   LONG *usedFiles,
   LONG *usedBlocks
);

#if 0
extern void GetFileServerConnectionID
(
   char *fileServerName,
   WORD *connectionID
);
#endif

extern void GetFileServerDateAndTime
(
   BYTE *dateAndTime
);

extern int GetFileServerDescriptionStrings
(
   char *company_Name, 
   char *revision,
   char *revisionDate,
   char *copyrightNotice
);

extern int GetFileServerLoginStatus
(
   int *loginEnabledFlag
);

extern void GetFileServerName
(
   WORD  connectionID, 
   char *fileServerName
);

extern int GetServerConfigurationInfo
(
   int *serverType,
   int *loaderType
);

extern int GetServerInformation
(
   int             returnSize, 
   FILE_SERV_INFO *serverInfo
);

extern LONG GetServerMemorySize
(
   void
);

extern LONG GetServerUtilization
(
   void
);

extern int SendConsoleBroadcast
(
   const char *message, 
   WORD        connectionCount,
   const WORD *connectionList
);

extern int SetFileServerDateAndTime
(
   WORD year, 
   WORD month, 
   WORD day,
   WORD hour, 
   WORD minute, 
   WORD second
);

#ifdef __cplusplus
}
#endif


#endif
