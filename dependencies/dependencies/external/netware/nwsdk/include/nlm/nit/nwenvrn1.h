/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 2000 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwenvrn1.h
==============================================================================
*/

#ifndef _NWENVRN1_H_
#define _NWENVRN1_H_

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>


/************************************************************************/

#define ENVSERV_OVERHEAD_SIZE (2*(signed int)sizeof(WORD))
#define ENVSERV_BUFFER1_SIZE  (512 + ENVSERV_OVERHEAD_SIZE)
#define ENVSERV_CONN_TYPE_286 286
#define ENVSERV_CONN_TYPE_386 386

/************************************************************************/
/* Structures                                                           */
/************************************************************************/
#include <npackon.h>

typedef struct tagCONN_USAGE
{
   LONG  systemElapsedTime;
   BYTE  bytesRead[6];
   BYTE  bytesWritten[6];
   LONG  totalRequestPackets;
} CONN_USAGE;

/************************************************************************/

typedef struct tagDISK_CACHE_STATS
{
   LONG  systemElapsedTime;
   WORD  cacheBufferCount;
   WORD  cacheBufferSize;
   WORD  dirtyCacheBuffers;
   LONG  cacheReadRequests;
   LONG  cacheWriteRequests;
   LONG  cacheHits;
   LONG  cacheMisses;
   LONG  physicalReadRequests;
   LONG  physicalWriteRequests;
   WORD  physicalReadErrors;
   WORD  physicalWriteErrors;
   LONG  cacheGetRequests;
   LONG  cacheFullWriteRequests;
   LONG  cachePartialWriteRequests;
   LONG  backgroundDirtyWrites;
   LONG  backgroundAgedWrites;
   LONG  totalCacheWrites;
   LONG  cacheAllocations;
   WORD  thrashingCount;
   WORD  LRUBlockWasDirtyCount;
   WORD  readBeyondWriteCount;
   WORD  fragmentedWriteCount;
   WORD  cacheHitOnUnavailCount;
   WORD  cacheBlockScrappedCount;
} DISK_CACHE_STATS;

/************************************************************************/

typedef struct tagDISK_CHANNEL_STATS
{
   LONG  systemElapsedTime;
   WORD  channelState;  /* DCS_RUNNING, DCS_BEINGSTOPPED,
                         DCS_ , DCS_NONFUNCTIONAL */
   WORD  channelSyncState;
   BYTE  driverType;
   BYTE  driverMajorVersion;
   BYTE  driverMinorVersion;
   char  driverDescription[65];
   WORD  IOAddr1;
   WORD  IOAddr1Size;
   WORD  IOAddr2;
   WORD  IOAddr2Size;
   BYTE  sharedMem1Seg[3];     /*hi-low-middle format*/
   WORD  sharedMem1Ofs;
   BYTE  sharedMem2Seg[3];     /*hi-low-middle format*/
   WORD  sharedMem2Ofs;
   BYTE  interrupt1Used;
   BYTE  interrupt1;
   BYTE  interrupt2Used;
   BYTE  interrupt2;
   BYTE  DMAChannel1Used;
   BYTE  DMAChannel1;
   BYTE  DMAChannel2Used;
   BYTE  DMAChannel2;
   WORD  reserved2;
   char  configDescription[80];
} DISK_CHANNEL_STATS;

/************************************************************************/

typedef struct tagDRIVE_MAP_TABLE
{
   LONG  systemElapsedTime;
   BYTE  SFTLevel;
   BYTE  logicalDriveCount;
   BYTE  physicalDriveCount;
   BYTE  diskChannelTable[5];
   WORD  pendingIOCommands;
   BYTE  mappingTable[32];
   BYTE  driveMirrorTable[32];
   BYTE  deadMirrorTable[32];
   BYTE  remirroredDrive;
   BYTE  reserved;
   LONG  remirroredBlock;
   WORD  SFTErrorTable[60];
} DRIVE_MAP_TABLE;

/************************************************************************/

typedef struct tagSERVER_LAN_IO
{
   LONG  systemElapsedTime;
   WORD  maxRoutingBuffersAvail;
   WORD  maxRoutingBuffersUsed;
   WORD  routingBuffersInUse;
   LONG  totalFileServicePackets;
   WORD  fileServicePacketsBuffered;
   WORD  invalidConnPacketCount;
   WORD  badLogicalConnCount;
   WORD  packetsRcvdDuringProcCount;
   WORD  reprocessedRequestCount;
   WORD  badSequenceNumberPacketCount;
   WORD  duplicateReplyCount;
   WORD  acknowledgementsSent;
   WORD  badRequestTypeCount;
   WORD  attachDuringProcCount;
   WORD  attachWhileAttachingCount;
   WORD  forgedDetachRequestCount;
   WORD  badConnNumberOnDetachCount;
   WORD  detachDuringProcCount;
   WORD  repliesCanceledCount;
   WORD  hopCountDiscardCount;
   WORD  unknownNetDiscardCount;
   WORD  noDGroupBufferDiscardCount;
   WORD  outPacketNoBufferDiscardCount;
   WORD  IPXNotMyNetworkCount;
   LONG  NetBIOSPropagationCount;
   LONG  totalOtherPackets;
   LONG  totalRoutedPackets;
} SERVER_LAN_IO;

/************************************************************************/

typedef struct tagSERVER_MISC_INFO
{
   LONG  systemElapsedTime;
   BYTE  processorType;
   BYTE  reserved;
   BYTE  serviceProcessCount;
   BYTE  serverUtilizationPercent;
   WORD  maxBinderyObjectsAvail;
   WORD  maxBinderyObjectsUsed;
   WORD  binderyObjectsInUse;
   WORD  serverMemoryInK;
   WORD  serverWastedMemoryInK;
   WORD  dynamicAreaCount;
   LONG  dynamicSpace1;
   LONG  maxUsedDynamicSpace1;
   LONG  dynamicSpaceInUse1;
   LONG  dynamicSpace2;
   LONG  maxUsedDynamicSpace2;
   LONG  dynamicSpaceInUse2;
   LONG  dynamicSpace3;
   LONG  maxUsedDynamicSpace3;
   LONG  dynamicSpaceInUse3;
} SERVER_MISC_INFO;

/************************************************************************/

typedef struct tagFILE_SYS_STATS
{
   LONG  systemElapsedTime;
   WORD  maxOpenFiles;
   WORD  maxFilesOpened;
   WORD  currOpenFiles;
   LONG  totalFilesOpened;
   LONG  totalReadRequests;
   LONG  totalWriteRequests;
   WORD  currChangedFATSectors;
   LONG  totalChangedFATSectors;
   WORD  FATWriteErrors;
   WORD  fatalFATWriteErrors;
   WORD  FATScanErrors;
   WORD  maxIndexFilesOpened;
   WORD  currOpenIndexedFiles;
   WORD  attachedIndexFiles;
   WORD  availableIndexFiles;
} FILE_SYS_STATS;

/************************************************************************/

typedef struct tagLAN_CONFIG
{
   BYTE  networkAddress[4];
   BYTE  hostAddress[6];
   BYTE  LANDriverInstalled;
   BYTE  optionNumber;
   char  configurationText[160]; /* One or more null terminated strings. */
} LAN_CONFIG;

/************************************************************************/

typedef struct tagPHYS_DISK_STATS
{
   LONG  systemElapsedTime;
   BYTE  diskChannel;
   BYTE  diskRemovable;
   BYTE  driveType;
   BYTE  controllerDriveNumber;
   BYTE  controllerNumber;
   BYTE  controllerType;
   LONG  driveSize;        /* in 4096 byte blocks */
   WORD  driveCylinders;
   BYTE  driveHeads;
   BYTE  sectorsPerTrack;
   char  driveDefinition[64];
   WORD  IOErrorCount;
   LONG  hotFixStart;         /* only meaningful with SFT I or greater */
   WORD  hotFixSize;       /* only meaningful with SFT I or greater */
   WORD  hotFixBlockAvailable;/* only meaningful with SFT I or greater */
   BYTE  hotFixDisabled;      /* only meaningful with SFT I or greater */
} PHYS_DISK_STATS;

/************************************************************************/

typedef struct tagTTS_STATS
{
   LONG  systemElapsedTime;
   BYTE  TTS_Supported;
   BYTE  TTS_Enabled;
   WORD  TTS_VolumeNumber;
   WORD  TTS_MaxOpenTransactions;
   WORD  TTS_MaxTransactionsOpened;
   WORD  TTS_CurrTransactionsOpen;
   LONG  TTS_TotalTransactions;
   LONG  TTS_TotalWrites;
   LONG  TTS_TotalBackouts;
   WORD  TTS_UnfilledBackouts;
   WORD  TTS_DiskBlocksInUse;
   LONG  TTS_FATAllocations;
   LONG  TTS_FileSizeChanges;
   LONG  TTS_FilesTruncated;
   BYTE  numberOfTransactions;
} TTS_STATS;

typedef struct tagTTS_CONNECTIONS
{
   BYTE  connectionNumber;
   BYTE  taskNumber;
} TTS_CONNECTIONS;

/************************************************************************/

typedef struct CONN_OPEN_FILES_286
{
   BYTE  taskNumber;
   BYTE  lockType;
   BYTE  accessControl;
   BYTE  lockFlag;
   BYTE  volumeNumber;
   WORD  dirEntry;
   char  fileName[14];
} CONN_OPEN_FILES_286;

typedef struct CONN_OPEN_FILES_386
{
   WORD  taskNumber;
   BYTE  lockType;
   BYTE  accessControl;
   BYTE  lockFlag;
   BYTE  volumeNumber;
   LONG  parentDirEntry;
   LONG  dirEntry;
   BYTE  forkCount;
   BYTE  nameSpace;
   BYTE  nameLength;
   BYTE  fileName[256];
} CONN_OPEN_FILES_386;

typedef struct CONN_OPEN_FILES
{
   WORD  unionType;
   union
   {
      CONN_OPEN_FILES_286 con286;
      CONN_OPEN_FILES_386 con386;
   } u;
} CONN_OPEN_FILES;


/************************************************************************/

typedef struct CONN_SEMAPHORE_286
{
   WORD  openCount;
   BYTE  semaphoreValue;
   BYTE  taskNumber;
   BYTE  nameLength;
   BYTE  semaphoreName[255];  /* Length preceeded name.*/
} CONN_SEMAPHORE_286;

typedef struct CONN_SEMAPHORE_386
{
   WORD  openCount;
   WORD  semaphoreValue;
   WORD  taskNumber;
   BYTE  nameLength;
   BYTE  semaphoreName[255];  /* Length preceeded name.*/
} CONN_SEMAPHORE_386;

typedef struct CONN_SEMAPHORE
{
   WORD  unionType;
   union
   {
      CONN_SEMAPHORE_286 con286;
      CONN_SEMAPHORE_386 con386;
   } u;
} CONN_SEMAPHORE;

/************************************************************************/

typedef struct CONN_TASK_INFO_286
{
   WORD  unionType;
   BYTE  lockStatus;
   union
   {
      struct
      {
         BYTE  taskNumber;
         WORD  beginAddress;
         WORD  endAddress;
         BYTE  volumeNumber;
         WORD  directoryEntry;
         BYTE  nameLength;
         BYTE  name;       /* 1st byte - more follow*/
      } LockStatus1;

      struct
      {
         BYTE  taskNumber;
         BYTE  volumeNumber;
         WORD  directoryEntry;
         BYTE  nameLength;
         BYTE  name;       /* 1st byte - more follow*/
      } LockStatus2;

      struct
      {
         BYTE  taskNumber;
         BYTE  nameLength;
         BYTE  name;       /* 1st byte - more follow*/
      } LockStatus3Or4;
   } waitRecord;            /* BYTE NumberOfActiveTasks - follows waitRecord*/
} CONN_TASK_INFO_286;

typedef struct CONN_TASK_INFO_386
{
   WORD  unionType;
   BYTE  lockStatus;
   union
   {
      struct
      {
         WORD  taskNumber;
         LONG  beginAddress;
         LONG  endAddress;
         WORD  volumeNumber;
         LONG  parentID;
         LONG  directoryEntry;
         BYTE  forkCount;
         BYTE  nameSpace;
         BYTE  nameLength;
         BYTE  name;       /* 1st byte - more follow*/
      } LockStatus1;

      struct
      {
         WORD  taskNumber;
         WORD  volumeNumber;
         LONG  parentID;
         LONG  directoryEntry;
         BYTE  forkCount;
         BYTE  nameSpace;
         BYTE  nameLength;
         BYTE  name;       /* 1st byte - more follow*/
      } LockStatus2;


      struct
      {
         WORD  taskNumber;
         BYTE  nameLength;
         BYTE  name;       /* 1st byte - more follow*/
      } LockStatus3Or4;

   } waitRecord;            /* BYTE NumberOfActiveTasks - follows waitRecord*/

} CONN_TASK_INFO_386;

typedef struct CONN_TASK_PAIRS_286
{
   BYTE  task;
   BYTE  taskStatus;
} CONN_TASK_PAIRS_286;

typedef struct CONN_TASK_PAIRS_386
{
   WORD  task;
   BYTE  taskStatus;
} CONN_TASK_PAIRS_386;

/************************************************************************/

typedef struct CONN_USING_FILE_REQ_286
{
   WORD  lastRecordSeen;
   BYTE  directoryHandle;
   BYTE  pathLength;
   BYTE  path[255];
} CONN_USING_FILE_REQ_286;

typedef struct CONN_USING_FILE_REQ_386
{
   BYTE  forkType;
   BYTE  volume;
   LONG  directoryID;
   WORD  nextRecord;
} CONN_USING_FILE_REQ_386;

typedef struct CONN_USING_FILE_REQUEST
{
   WORD  unionType;
   WORD  reserved1;
   BYTE  reserved2;
   union
   {
      CONN_USING_FILE_REQ_286 req286;
      CONN_USING_FILE_REQ_386 req386;
   } request;
} CONN_USING_FILE_REQUEST;

typedef struct CONN_USING_FILE_REPLY_286
{
   WORD  useCount;
   WORD  openCount;
   WORD  openForReadCount;
   WORD  openForWriteCount;
   WORD  denyReadCount;
   WORD  denyWriteCount;
   WORD  nextRequestRecord;
   BYTE  locked;
   WORD  numberOfRecords;  /* Connection records follow */
} CONN_USING_FILE_REPLY_286;

typedef struct CONN_USING_FILE_REPLY_386
{
   WORD  nextRequestRecord;
   WORD  useCount;
   WORD  openCount;
   WORD  openForReadCount;
   WORD  openForWriteCount;
   WORD  denyReadCount;
   WORD  denyWriteCount;
   BYTE  locked;
   BYTE  forkCount;
   WORD  numberOfRecords;  /* connection records follow */
} CONN_USING_FILE_REPLY_386;

typedef struct CONN_USING_FILE_RECORD_286
{
   WORD  connectionNumber;
   BYTE  taskNumber;
   BYTE  lockType;
   BYTE  accessFlags;
   BYTE  lockStatus;
} CONN_USING_FILE_RECORD_286;

typedef struct CONN_USING_FILE_RECORD_386
{
   WORD  connectionNumber;
   WORD  taskNumber;
   BYTE  lockType;
   BYTE  accessFlags;
   BYTE  lockStatus;
} CONN_USING_FILE_RECORD_386;

typedef struct CONN_USING_FILE_REPLY
{
   WORD unionType;
   union
   {
      struct CONN_USING_FILE_REPLY_286 rep286;
      struct CONN_USING_FILE_REPLY_386 rep386;
   } reply;
} CONN_USING_FILE_REPLY;

/************************************************************************/

typedef struct LOGICAL_RECORD_INFO_286
{
   WORD  useCount;
   WORD  shareableLockCount;
   WORD  nextRequestRecord;
   BYTE  locked;
   BYTE  numberOfRecords;
} LOGICAL_RECORD_INFO_286;


typedef struct LOGICAL_RECORD_INFO_386
{
   WORD  useCount;
   WORD  shareableLockCount;
   BYTE  locked;
   WORD  nextRequestRecord;
   WORD  numberOfRecords;
} LOGICAL_RECORD_INFO_386;

typedef struct LOGICAL_RECORD_286
{
   WORD  connectionNumber;
   BYTE  taskNumber;
   BYTE  lockStatus;
} LOGICAL_RECORD_286;

typedef struct LOGICAL_RECORD_386
{
   WORD  connectionNumber;
   WORD  taskNumber;
   BYTE  lockStatus;
} LOGICAL_RECORD_386;

typedef struct LOGICAL_RECORD_INFO
{
   WORD  unionType;
   union
   {
      LOGICAL_RECORD_INFO_286 lr286;
      LOGICAL_RECORD_INFO_386 lr386;
   } u;
} LOGICAL_RECORD_INFO;

typedef struct LOGICAL_RECORD_REQUEST
{
   WORD  reserved1;
   BYTE  reserved2;
   WORD  nextRecord;
   BYTE  nameLength;
   BYTE  name[255];
} LOGICAL_RECORD_REQUEST;

/************************************************************************/

typedef struct CONN_LOGICAL_RECORD_286
{
   WORD  nextRequest;
   BYTE  numberOfRecords;
} CONN_LOGICAL_RECORD_286;

typedef struct CONN_LOGICAL_RECORD_386
{
   WORD  nextRequest;
   WORD  numberOfRecords;
} CONN_LOGICAL_RECORD_386;

typedef struct CONN_LOGICAL_RECORD
{
   WORD  unionType;
   union
   {
      CONN_LOGICAL_RECORD_286 lr286;
      CONN_LOGICAL_RECORD_386 lr386;
   } u;
} CONN_LOGICAL_RECORD;

typedef struct CONN_LOGICAL_RECORD_BLOCK_286
{
   BYTE  taskNumber;
   BYTE  lockStatus;
   BYTE  lockNameLength;
   BYTE  lockName;      /* 1st byte - more follow */
} CONN_LOGICAL_RECORD_BLOCK_286;

typedef struct CONN_LOGICAL_RECORD_BLOCK_386
{
   WORD  taskNumber;
   BYTE  lockStatus;
   BYTE  lockNameLength;
   BYTE  lockName;      /* 1st byte - more follow */
} CONN_LOGICAL_RECORD_BLOCK_386;

/************************************************************************/

typedef struct FILE_PHYSICAL_RECORD_LOCK_286
{
   WORD  nextRequest;
   BYTE  numberOfLocks;
   BYTE  reserved;
} FILE_PHYSICAL_RECORD_LOCK_286;

typedef struct FILE_PHYSICAL_RECORD_LOCK_386
{
   WORD  nextRequest;
   WORD  numberOfLocks;
} FILE_PHYSICAL_RECORD_LOCK_386;

typedef struct FILE_PHYSICAL_RECORD_LOCK
{
   WORD unionType;
   union
   {
      FILE_PHYSICAL_RECORD_LOCK_286 pr286;
      FILE_PHYSICAL_RECORD_LOCK_286 pr386;
   } u;
} FILE_PHYSICAL_RECORD_LOCK;

typedef struct FILE_PHYSICAL_RECORD_286
{
   WORD  loggedCount;
   WORD  shareLockCount;
   LONG  recordStart;
   LONG  recordEnd;
   WORD  connectionNumber;
   BYTE  taskNumber;
   BYTE  lockType;
} FILE_PHYSICAL_RECORD_286;

typedef struct FILE_PHYSICAL_RECORD_386
{
   WORD  loggedCount;
   WORD  shareLockCount;
   LONG  recordStart;
   LONG  recordEnd;
   WORD  connectionNumber;
   WORD  taskNumber;
   BYTE  lockType;
} FILE_PHYSICAL_RECORD_386;

typedef struct FILE_PHYSICAL_REQUEST_286
{
   WORD  lastRecord;
   BYTE  directoryHandle;
   BYTE  pathLength;
   BYTE  name[255];
} FILE_PHYSICAL_REQUEST_286;

typedef struct FILE_PHYSICAL_REQUEST_386
{
   BYTE  forkType;
   BYTE  volume;
   LONG  directoryID;
   WORD  next;
} FILE_PHYSICAL_REQUEST_386;

typedef struct FILE_PHYSICAL_RECORD_REQUEST
{
   WORD  unionType;
   WORD  reserved1;
   BYTE  reserved2;
   union
   {
      FILE_PHYSICAL_REQUEST_286 pr286;
      FILE_PHYSICAL_REQUEST_386 pr386;
   } u;
} FILE_PHYSICAL_RECORD_REQUEST;

/************************************************************************/

typedef struct CONN_RECORD_LOCKS_286
{
   WORD  nextRecord;
   BYTE  numberOfLocks;
   BYTE  reserved;       /* record locks follow */
} CONN_RECORD_LOCKS_286;

typedef struct CONN_RECORD_LOCKS_386
{
   WORD  nextRecord;
   WORD  numberOfLocks;  /* record locks follow */
} CONN_RECORD_LOCKS_386;

typedef struct CONN_RECORD_LOCKS
{
   WORD  unionType;
   union
   {
      CONN_RECORD_LOCKS_286 rl286;
      CONN_RECORD_LOCKS_386 rl386;
   } u;
} CONN_RECORD_LOCKS;

typedef struct CONN_LOCK_RECORD_286
{
   BYTE  taskNumber;
   BYTE  lockFlag;
   LONG  recordStart;
   LONG  recordEnd;
} CONN_LOCK_RECORD_286;

typedef struct CONN_LOCK_RECORD_386
{
   WORD  taskNumber;
   BYTE  lockFlag;
   LONG  recordStart;
   LONG  recordEnd;
} CONN_LOCK_RECORD_386;

typedef struct CONN_LOCK_REQUEST_286
{
   WORD  connectionNumber;
   WORD  lastRecord;
   BYTE  volume;
   WORD  directoryID;
   BYTE  pathLength;
   BYTE  fileName[14];
} CONN_LOCK_REQUEST_286;

typedef struct CONN_LOCK_REQUEST_386
{
   WORD  connectionNumber;
   BYTE  forkType;
   BYTE  volume;
   LONG  directoryID;
   WORD  next;
} CONN_LOCK_REQUEST_386;

typedef struct CONN_LOCK_REQUEST
{
   WORD  unionType;
   WORD  reserved1;
   BYTE  reserved2;
   union
   {
      CONN_LOCK_REQUEST_286 lr286;
      CONN_LOCK_REQUEST_386 lr386;
   } u;
} CONN_LOCK_REQUEST;

/************************************************************************/

typedef struct SEMAPHORE_INFO_286
{
   WORD  nextRequest;
   WORD  openCount;
   BYTE  semaphoreValue;
   BYTE  numberOfRecords;
} SEMAPHORE_INFO_286;

typedef struct SEMAPHORE_INFO_386
{
   WORD  nextRequest;
   WORD  openCount;
   WORD  semaphoreValue;
   WORD  numberOfRecords;
} SEMAPHORE_INFO_386;

typedef struct SEMAPHORE_INFO
{
   WORD  unionType;
   union
   {
      SEMAPHORE_INFO_286 si286;
      SEMAPHORE_INFO_386 si386;
   } u;
} SEMAPHORE_INFO;

typedef struct SEMAPHORE_INFO_RECORD_286
{
   WORD  connectionNumber;
   BYTE  taskNumber;
} SEMAPHORE_INFO_RECORD_286;

typedef struct SEMAPHORE_INFO_RECORD_386
{
   WORD  connectionNumber;
   WORD  taskNumber;
} SEMAPHORE_INFO_RECORD_386;

typedef struct SEMAPHORE_INFO_REQUEST
{
   WORD  reserved1;
   BYTE  reserved2;
   WORD  nextRecord;
   BYTE  nameLength;
   BYTE  name[255];
} SEMAPHORE_INFO_REQUEST;

#include <npackoff.h>

/************************************************************************/
/* Function Prototypes                                                  */
/************************************************************************/

#ifdef __cplusplus
extern "C"
{
#endif

extern int GetConnectionSemaphores
(
   WORD            connectionNumber,
   int            *lastRecord,
   int            *lastTask,
   int             structSize,
   CONN_SEMAPHORE *connectionSemaphore,
   void           *buffer,
   int             bufferSize
);

extern int GetConnectionsOpenFiles
(
   WORD             connectionNumber,
   int             *lastRecord,
   int             *lastTask,
   int              structSize,
   CONN_OPEN_FILES *openFiles,
   void            *buffer,
   int              bufferSize
);

extern int GetConnectionsTaskInformation
(
   WORD   connectionNumber,
   void **connectionTaskInfo,
   void  *buffer,
   int    bufferSize
);

extern int GetConnectionsUsageStats
(
   int         connectionNumber,
   CONN_USAGE *connectionUsage
);

extern int GetConnectionsUsingFile
(
   int   requestSize,
   void *request,
   void *buffer,
   int   bufferSize);

extern int GetDiskCacheStats
(
   DISK_CACHE_STATS *cacheStats
);

extern int GetDiskChannelStats
(
   int                 channelNumber,
   DISK_CHANNEL_STATS *diskChannelStats
);

extern int GetDriveMappingTable
(
   DRIVE_MAP_TABLE *driveMappingTable
);

extern int GetFileServerLANIOStats
(
   SERVER_LAN_IO *serverLANIOStats
);

extern int GetFileServerMiscInformation
(
   SERVER_MISC_INFO *miscInformation
);

extern int GetFileSystemStats
(
   FILE_SYS_STATS *fileSysStats
);

extern int GetLANDriverConfigInfo
(
   BYTE        LANBoardNumber,
   LAN_CONFIG *LANConfiguration
);

extern int GetLogicalRecordInformation
(
   int   requestSize,
   void *request,
   void *buffer,
   int   bufferSize
);

extern int GetLogicalRecordsByConnection
(
   WORD  connectionNumber,
   WORD  nextRecord,
   void *buffer,
   int   bufferSize
);

extern int GetPathFromDirectoryEntry
(
   BYTE  volumeNumber,
   WORD  directoryEntry,
   BYTE *pathLength,
   char *path
);

extern int GetPhysicalDiskStats
(
   BYTE             physicalDiskNumber,
   PHYS_DISK_STATS *physicalDiskStats
);

extern int GetPhysicalRecordLocksByFile
(
   int   requestSize,
   void *request,
   void *buffer,
   int   bufferSize
);

extern int GetPhysRecLockByConnectAndFile
(
   int   requestSize,
   void *request,
   void *buffer,
   int   bufferSize
);

extern int GetSemaphoreInformation
(
   int   requestSize,
   void *request,
   void *buffer,
   int   bufferSize
);

extern int TTSGetStats
(
   TTS_STATS *TTSStats,
   int        bufferLen,
   BYTE      *buffer
);

#ifdef __cplusplus
}
#endif

#endif
