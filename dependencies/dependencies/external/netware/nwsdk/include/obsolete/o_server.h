/******************************************************************************

  %name: o_server.h %
  %version: 2 %
  %date_modified: Tue Aug 29 18:45:34 2000 %
  $Copyright:

  Copyright (c) 1999 Novell, Inc.  All Rights Reserved.

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/


/*
 * This file contains prototypes for library function calls that are being
 * deprecated. They are preserved here in the interest of all legacy software
 * that depends on them. Please update such software to use the preferred API
 * calls.
 *
 * DO NOT INCLUDE THIS HEADER EXPLICITLY!!
 *
 * Include "nwserver.h" and use a compiler switch to define INCLUDE_OBSOLETE
 * (i.e. -DINCLUDE_OBSOLETE).
 */

#ifndef _OBSOLETE_NWSERVER_H
#define _OBSOLETE_NWSERVER_H

#ifdef __cplusplus
extern "C" {
#endif

typedef struct
{
   nuint8 networdAddress[4];
   nuint8 hostAddress[6];
   nuint8 boardInstalled;
   nuint8 optionNumber;
   nuint8 configurationText1[80];
   nuint8 configurationText2[80];
} NWLAN_CONFIG;

typedef struct
{
   nuint32  systemElapsedTime;
   nuint16  maxRoutingBuffersAvail;
   nuint16  maxRoutingBuffersUsed;
   nuint16  routingBuffersInUse;
   nuint32  totalFileServicePackets;
   nuint16  fileServicePacketsBuffered;
   nuint16  invalidConnPacketCount;
   nuint16  badLogicalConnCount;
   nuint16  packetsRcvdDuringProcCount;
   nuint16  reprocessedRequestCount;
   nuint16  badSequenceNumberPacketCount;
   nuint16  duplicateReplyCount;
   nuint16  acknowledgementsSent;
   nuint16  badRequestTypeCount;
   nuint16  attachDuringProcCount;
   nuint16  attachWhileAttachingCount;
   nuint16  forgedDetachRequestCount;
   nuint16  badConnNumberOnDetachCount;
   nuint16  detachDuringProcCount;
   nuint16  repliesCanceledCount;
   nuint16  hopCountDiscardCount;
   nuint16  unknownNetDiscardCount;
   nuint16  noDGroupBufferDiscardCount;
   nuint16  outPacketNoBufferDiscardCount;
   nuint16  IPXNotMyNetworkCount;
   nuint32  NetBIOSPropagationCount;
   nuint32  totalOtherPackets;
   nuint32  totalRoutedPackets;
}  SERVER_LAN_IO_STATS;

typedef struct
{
   nuint32 systemElapsedTime;
   nuint8  diskChannel;
   nuint8  diskRemovable;
   nuint8  driveType;
   nuint8  controllerDriveNumber;
   nuint8  controllerNumber;
   nuint8  controllerType;
   nuint32 driveSize;            /* in 4096 byte blocks */
   nuint16 driveCylinders;
   nuint8  driveHeads;
   nuint8  sectorsPerTrack;
   nuint8  driveDefinition[64];
   nuint16 IOErrorCount;
   nuint32 hotFixStart;          /* only meaningful with SFT I or greater */
   nuint16 hotFixSize;           /* only meaningful with SFT I or greater */
   nuint16 hotFixBlockAvailable; /* only meaningful with SFT I or greater */
   nuint8  hotFixDisabled;       /* only meaningful with SFT I or greater */
} PHYS_DSK_STATS;

typedef struct
{
   nuint32 systemElapsedTime;
   nuint16 channelState;
   nuint16 channelSyncState;
   nuint8  driverType;
   nuint8  driverMajorVersion;
   nuint8  driverMinorVersion;
   nuint8  driverDescription[65];
   nuint16 IOAddr1;
   nuint16 IOAddr1Size;
   nuint16 IOAddr2;
   nuint16 IOAddr2Size;
   nuint8  sharedMem1Seg[3];
   nuint16 sharedMem1Ofs;
   nuint8  sharedMem2Seg[3];
   nuint16 sharedMem2Ofs;
   nuint8  interrupt1Used;
   nuint8  interrupt1;
   nuint8  interrupt2Used;
   nuint8  interrupt2;
   nuint8  DMAChannel1Used;
   nuint8  DMAChannel1;
   nuint8  DMAChannel2Used;
   nuint8  DMAChannel2;
   nuint16 reserved2;
   nuint8  configDescription[80];
}  DSK_CHANNEL_STATS;

typedef struct
{
   nuint32 systemElapsedTime;
   nuint16 cacheBufferCount;
   nuint16 cacheBufferSize;
   nuint16 dirtyCacheBuffers;
   nuint32 cacheReadRequests;
   nuint32 cacheWriteRequests;
   nuint32 cacheHits;
   nuint32 cacheMisses;
   nuint32 physicalReadRequests;
   nuint32 physicalWriteRequests;
   nuint16 physicalReadErrors;
   nuint16 physicalWriteErrors;
   nuint32 cacheGetRequests;
   nuint32 cacheFullWriteRequests;
   nuint32 cachePartialWriteRequests;
   nuint32 backgroundDirtyWrites;
   nuint32 backgroundAgedWrites;
   nuint32 totalCacheWrites;
   nuint32 cacheAllocations;
   nuint16 thrashingCount;
   nuint16 LRUBlockWasDirtyCount;
   nuint16 readBeyondWriteCount;
   nuint16 fragmentedWriteCount;
   nuint16 cacheHitOnUnavailCount;
   nuint16 cacheBlockScrappedCount;
} DSK_CACHE_STATS;

typedef struct
{
   nuint32 systemElapsedTime;
   nuint16 maxOpenFiles;
   nuint16 maxFilesOpened;
   nuint16 currOpenFiles;
   nuint32 totalFilesOpened;
   nuint32 totalReadRequests;
   nuint32 totalWriteRequests;
   nuint16 currChangedFATSectors;
   nuint32 totalChangedFATSectors;
   nuint16 FATWriteErrors;
   nuint16 fatalFATWriteErrors;
   nuint16 FATScanErrors;
   nuint16 maxIndexFilesOpened;
   nuint16 currOpenIndexedFiles;
   nuint16 attachedIndexFiles;
   nuint16 availableIndexFiles;
} FILESYS_STATS;

typedef struct
{
   nuint32 systemElapsedTime;
   nuint8  SFTSupportLevel;
   nuint8  logicalDriveCount;
   nuint8  physicalDriveCount;
   nuint8  diskChannelTable[5];
   nuint16 pendingIOCommands;
   nuint8  driveMappingTable[32];
   nuint8  driveMirrorTable[32];
   nuint8  deadMirrorTable[32];
   nuint8  reMirrorDriveNumber;
   nuint8  reserved;
   nuint32 reMirrorCurrentOffset;
   nuint16 SFTErrorTable[60];
}  DRV_MAP_TABLE;

/* structures for NWGetFileServerMiscInfo (2.2 only) */
typedef struct tNW_MEM_AREAS
{
   nuint32 total;    /* total amount of memory in dynamic memory area */
   nuint32 max;      /* amount of memory in dynamic memory area that has been in use since server was brought up */
   nuint32 cur;      /* amount of memory in dynamic memory area currently in use */
} NW_DYNAMIC_MEM;

typedef struct tNW_FS_MISC
{
   nuint32 upTime;        /* how long file server's been up in 1/18 ticks (wraps at 0xffffffff) */
   nuint8  processor;      /* 1 = 8086/8088, 2 = 80286       */
   nuint8  reserved;
   nuint8  numProcs;       /* number processes that handle incoming service requests */
   nuint8  utilization;    /* server utilization percentage (0-100), updated once/sec */
   nuint16 configuredObjs; /* max number of bindery objects file server will track - 0=unlimited & next 2 fields have no meaning */
   nuint16 maxObjs;        /* max number of bindery objects that have been used concurrently since file server came up */
   nuint16 curObjs;        /* actual number of bindery objects currently in use on server */
   nuint16 totalMem;       /* total amount of memory (in K) installed on server */
   nuint16 unusedMem;      /* amount of memory server has determined it is not using */
   nuint16 numMemAreas;    /* number of dynamic memory areas (1-3) */
   NW_DYNAMIC_MEM dynamicMem[3];
} NW_FS_INFO;

N_EXTERN_LIBRARY( NWCCODE )
NWDetachFromFileServer
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileServerName
(
   NWCONN_HANDLE  conn,
   pnstr8         serverName
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileServerVersion
(
   NWCONN_HANDLE conn,
   pnuint16 serverVersion
);


N_EXTERN_LIBRARY( NWCCODE )
NWGetDiskChannelStats
(
   NWCONN_HANDLE  conn,
   nuint8         channelNum,
   DSK_CHANNEL_STATS N_FAR * statBuffer
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetDiskCacheStats
(
   NWCONN_HANDLE  conn,
   DSK_CACHE_STATS N_FAR * statBuffer
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFSDriveMapTable
(
   NWCONN_HANDLE  conn,
   DRV_MAP_TABLE N_FAR * tableBuffer
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFSLANDriverConfigInfo
(
   NWCONN_HANDLE  conn,
   nuint8         lanBoardNum,
   NWLAN_CONFIG N_FAR * lanConfig
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileServerLANIOStats
(
   NWCONN_HANDLE  conn,
   SERVER_LAN_IO_STATS N_FAR * statBuffer
);

/* this function is 2.2 specific */
N_EXTERN_LIBRARY( NWCCODE )
NWGetFileServerMiscInfo
(
   NWCONN_HANDLE  conn,
   NW_FS_INFO N_FAR * fsInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetPhysicalDiskStats
(
   NWCONN_HANDLE  conn,
   nuint8         physicalDiskNum,
   PHYS_DSK_STATS N_FAR * statBuffer
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileSystemStats
(
   NWCONN_HANDLE  conn,
   FILESYS_STATS N_FAR * statBuffer
);

#ifdef __cplusplus
}
#endif

#endif   /* _OBSOLETE_NWSERVER_H */
