/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwafp.h
==============================================================================
*/

#ifndef _NWAFP_H_
#define _NWAFP_H_

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

#include <npackon.h>

typedef struct tagAFPFILEINFO
{
   long entryID;
   long parentID;
   WORD attributes;
   long dataForkLength;
   long resourceForkLength;
   WORD numOffspring;
   WORD creationDate;
   WORD accessDate;
   WORD modifyDate;
   WORD modifyTime;
   WORD backupDate;
   WORD backupTime;
   BYTE finderInfo[32];
   char longName[33];
   char pad1;
   long ownerID;
   char shortName[13];
   char pad2;
   WORD accessPrivileges;
   BYTE proDosInfo[6];
} AFPFILEINFO;

typedef struct tagAFPSETINFO
{
   WORD attributes;
   WORD creationDate;
   WORD accessDate;
   WORD modifyDate;
   WORD modifyTime;
   WORD backupDate;
   WORD backupTime;
   BYTE finderInfo[32];
   BYTE proDosInfo[6];
} AFPSETINFO;

#include <npackoff.h>

#ifdef __cplusplus
extern "C" {
#endif

extern int AFPAllocTemporaryDirHandle
(
   WORD        connectionID,
   BYTE        volumeNum,
   long        AFPEntryID,
   const char *AFPPathString,
   BYTE       *NetWareDirectoryHandle,
   BYTE       *AccessRights
);

extern int AFPCreateDirectory
(
   WORD        connectionID,
   BYTE        volumeNum,
   long        AFPEntryID,
   const BYTE *finderInfo,
   const char *AFPPathString,
   long       *newAFPEntryID
);

extern int AFPCreateFile
(
   WORD        connectionID,
   BYTE        volumeNum,
   long        AFPEntryID,
   BYTE        deleteExistingFile,
   const BYTE *finderInfo,
   const char *AFPPathString,
   long       *newAFPEntryID
);

extern int AFPDelete
(
   WORD        connectionID,
   BYTE        volumeNum,
   long        AFPEntryID,
   const char *AFPPathString
);

extern int AFPDirectoryEntry
(
   WORD        connectionID,
   BYTE        directoryHandle,
   const char *pathName
);

extern int AFPGetEntryIDFromName
(
   WORD        connectionID,
   BYTE        volumeNum,
   long        AFPEntryID,
   const char *AFPPathString,
   long       *newAFPEntryID
);

extern int AFPGetEntryIDFromNetWareHandle
(
   WORD        connectionID,
   const BYTE *NetWareHandle,
   BYTE       *volumeID,
   long       *AFPEntryID,
   BYTE       *forkIndicator
);

extern int AFPGetEntryIDFromPathName
(
   WORD        connectionID,
   BYTE        directoryHandle,
   const char *pathName,
   long       *AFPEntryID
);

extern int AFPGetFileInformation
(
   WORD        connectionID,
   BYTE        volumeNum,
   long        AFPEntryID,
   WORD        requestBitMap,
   const char *AFPPathString,
   WORD        strucSize,
   AFPFILEINFO *AFPFileInfo
);

extern int AFPOpenFileFork
(
   WORD        connectionID,
   BYTE        volumeNum,
   long        AFPEntryID,
   BYTE        forkIndicator,
   BYTE        accessMode,
   const char *AFPPathString,
   long       *fileID,
   long       *forkLength,
   BYTE       *NetWareHandle,
   int        *fileHandle
);

extern int AFPRename
(
   WORD        connectionID,
   BYTE        volumeNum,
   long        AFPSourceEntryID,
   long        AFPDestEntryID,
   const char *AFPSourcePath,
   const char *AFPDestPath
);

extern int AFPScanFileInformation
(
   WORD         connectionID,
   BYTE         volumeNum,
   long         AFPEntryID,
   long        *AFPLastSeenID,
   WORD         searchBitMap,
   WORD         requestBitMap,
   const char  *AFPPathString,
   WORD         strucSize,
   AFPFILEINFO *AFPScanFileInfo
);

extern int AFPSetFileInformation
(
   WORD        connectionID,
   BYTE        volumeNum,
   long        AFPEntryID,
   WORD        requestBitMap,
   const char *AFPPathString,
   WORD        strucSize,
   const AFPSETINFO *AFPSetInfo
);

extern int AFPSupported
(
   WORD connectionID
);

#ifdef __cplusplus
}
#endif


#endif
