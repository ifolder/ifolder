#ifndef _NWDFS_H_
#define _NWDFS_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwdfs.h
==============================================================================
*/
#include <nwtypes.h>
#include <nwintxx.h>

/* completion codes */
#define DFSFailedCompletion           -1
#define DFSNormalCompletion           0
#define DFSInsufficientSpace          1
#define DFSVolumeSegmentDeactivated   4
#define DFSTruncationFailure          16
#define DFSHoleInFileError            17
#define DFSParameterError             18
#define DFSOverlapError               19
#define DFSSegmentError               20
#define DFSBoundryError               21
#define DFSInsufficientLimboFileSpace 22
#define DFSNotInDirectFileMode        23
#define DFSOperationBeyondEndOfFile   24
#define DFSOutOfHandles               129
#define DFSHardIOError                131
#define DFSInvalidFileHandle          136
#define DFSNoReadPrivilege            147
#define DFSNoWritePrivilege           148
#define DFSFileDetached               149
#define DFSInsufficientMemory         150
#define DFSInvalidVolume              152
#define DFSIOLockError                162


struct FileMapStructure
{
   LONG fileBlock;
   LONG volumeBlock;
   LONG numberOfBlocks;
};

struct VolumeInformationStructure
{
   LONG VolumeAllocationUnitSizeInBytes;
   LONG VolumeSizeInAllocationUnits;
   LONG VolumeSectorSize;
   LONG AllocationUnitsUsed;
   LONG AllocationUnitsFreelyAvailable;
   LONG AllocationUnitsInDeletedFilesNotAvailable;
   LONG AllocationUnitsInAvailableDeletedFiles;
   LONG NumberOfPhysicalSegmentsInVolume;
   LONG PhysicalSegmentSizeInAllocationUnits[64];
};

struct DFSCallBackParameters
{
   LONG localSemaphoreHandle;
   LONG completionCode;
};


/*-------------------------------------------------------------------------
 *	Definition of setSizeFlags
 *-------------------------------------------------------------------------*/
#define SETSIZE_NON_SPARSE_FILE 0x00000001 /* Alloc blocks to extend the file */
#define SETSIZE_NO_ZERO_FILL	 0x00000002	/* Do not zero fill the newly
											 				allocated blocks   				*/
#define SETSIZE_UNDO_ON_ERR	 0x00000004	/* In non sparse cases truncate
											 				back to original eof if an
											 				error occurs.						*/
#define SETSIZE_PHYSICAL_ONLY	 0x00000008 /* Change the physical EOF	only,
															dont change logical EOF. This
															means non sparse for the expand
															case.									*/
#define SETSIZE_LOGICAL_ONLY	 0x00000010	/* Change only the logical EOF,
															expand will always be sparse,
															and truncate won't free physical
															blocks.								*/

#ifdef __cplusplus
extern "C"
{
#endif

extern LONG DFSclose
(
   LONG fileHandle
);

extern LONG DFScreat
(
   const BYTE *fileName,
   LONG  permission,
   LONG  flagBits
);

extern LONG DFSExpandFile
(
   LONG fileHandle,
   LONG fileBlockNumber,
   LONG numberOfBlocks,
   LONG volumeBlockNumber,
   LONG segmentNumber
);

extern LONG DFSFreeLimboVolumeSpace
(
   LONG volumeNumber,
   LONG numberOfBlocks
);

extern LONG DFSsopen
(
   const BYTE *fileName,
   LONG  access,
   LONG  share,
   LONG  permission,
   LONG  flagBits,
   LONG  dataStream
);

extern LONG DFSRead
(
   LONG  fileHandle,
   LONG  startingSector,
   LONG  sectorCount,
   void *buffer
);

extern LONG DFSReadNoWait
(
   LONG                          fileHandle,
   LONG                          startingSector,
   LONG                          sectorCount,
   void                         *buffer,
   struct DFSCallBackParameters *callBackNode
);

extern LONG DFSReturnFileMappingInformation
(
   LONG                     fileHandle,
   LONG                     startingBlockNumber,
   LONG                    *numberOfEntries,
   LONG                     tableSize,
   struct FileMapStructure *table
);

extern LONG DFSReturnVolumeBlockInformation
(
   LONG  volumeNumber,
   LONG  startingBlockNumber,
   LONG  numberOfBlocks,
   BYTE *buffer
);

extern LONG DFSReturnVolumeMappingInformation
(
   LONG                               volumeNumber,
   struct VolumeInformationStructure *volumeInformation
);

#ifdef INT64
extern LONG DFSSetDataSize
(
	LONG					handle,
	unsigned INT64	newFileSize,
	LONG					setSizeFlags
);
#endif

extern LONG DFSSetEndOfFile
(
   LONG handle,
   LONG newFileSize,
   LONG returnTruncatedBlocksFlag
);

extern LONG DFSWrite
(
   LONG fileHandle,
   LONG startingSector,
   LONG sectorCount,
   const void *buffer
);

extern LONG DFSWriteNoWait
(
   LONG                          fileHandle,
   LONG                          startingSector,
   LONG                          sectorCount,
   const void                   *buffer,
   struct DFSCallBackParameters *callBackNode
);

#ifdef __cplusplus
}
#endif


#endif
