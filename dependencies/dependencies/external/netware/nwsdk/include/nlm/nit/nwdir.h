/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwdir.h
==============================================================================
*/

#ifndef _NWDIR_H_
#define _NWDIR_H_

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <nwtypes.h>
#include <nwfattr.h>


/* Trustee Access Rights in a network directory */

#define TA_NONE          0x0000
#define TA_READ          0x0001
#define TA_WRITE         0x0002
#define TA_CREATE        0x0008
#define TA_DELETE        0x0010
#define TA_ACCESSCONTROL 0x0020
#define TA_SEEFILES      0x0040
#define TA_MODIFY        0x0080
#define TA_SUPERVISOR    0x0100
#define TA_ALL           0x01FB
/* Old names for rights */
#define TA_OPEN          0x0004
#define TA_OWNERSHIP     0x0020
#define TA_SEARCH        0x0040

#define BYTES_PER_SECTOR  512

/* define volume types */

#define VINetWare386    0
#define VINetWare286    1
#define VINetWare386v30 2
#define VINetWare386v31 3

/* define the extended volume information status flag bits */

#define NWSubAllocEnabledBit    0x01
#define NWCompressionEnabledBit 0x02
#define NWMigrationEnabledBit   0x04
#define NWAuditingEnabledBit    0x08
#define NWReadOnlyEnabledBit    0x10

#include <npackon.h>

struct AnswerStructure
{
   BYTE           ALevelNumber;
   MisalignedLONG AMaximumAmount;
   MisalignedLONG ACurrentAmount;
};

typedef struct tagVOLUME_STATS
{
   long systemElapsedTime;
   BYTE volumeNumber;
   BYTE logicalDriveNumber;
   WORD sectorsPerBlock;
   long startingBlock;
   WORD totalBlocks;
   WORD availableBlocks;
   WORD totalDirectorySlots;
   WORD availableDirectorySlots;
   WORD maxDirectorySlotsUsed;
   BYTE isHashing;
   BYTE isRemovable;
   BYTE isMounted;
   char volumeName[17];
   LONG purgableBlocks;
   LONG notYetPurgableBlocks;
} VOLUME_STATS;

typedef struct tagVOLUME_INFO
{
   long  systemElapsedTime;
   BYTE  volumeNumber;
   BYTE  logicalDriveNumber;
   WORD  sectorsPerBlock;
   short startingBlock;
   LONG  totalBlocks;
   LONG  availableBlocks;
   LONG  totalDirectorySlots;
   LONG  availableDirectorySlots;
   BYTE  isHashing;
   BYTE  isRemovable;
   BYTE  isMounted;
   char  volumeName[17];
   LONG  purgableBlocks;
   LONG  notYetPurgableBlocks;
} VOLUME_INFO;

typedef struct ExtendedVolInfo_tag
{
   LONG volType;
   LONG statusFlag;
   LONG sectorSize;
   LONG sectorsPerCluster;
   LONG volSizeInClusters;
   LONG freeClusters;
   LONG subAllocFreeableClusters;
   LONG freeableLimboSectors;
   LONG nonfreeableLimboSectors;
   LONG availSubAllocSectors;
   LONG nonuseableSubAllocSectors;
   LONG subAllocClusters;
   LONG numDataStreams;
   LONG numLimboDataStreams;
   LONG oldestDelFileAgeInTicks;
   LONG numCompressedDataStreams;
   LONG numCompressedLimboDataStreams;
   LONG numNoncompressibleDataStreams;
   LONG precompressedSectors;
   LONG compressedSectors;
   LONG numMigratedDataStreams;
   LONG migratedSectors;
   LONG clustersUsedByFAT;
   LONG clustersUsedByDirs;
   LONG clustersUsedByExtDirs;
   LONG totalDirEntries;
   LONG unusedDirEntries;
   LONG totalExtDirExtants;
   LONG unusedExtDirExtants;
   LONG extAttrsDefined;
   LONG extAttrExtantsUsed;
   LONG directoryServicesObjectID;
   LONG lastModifiedDateAndTime;
} NWVolExtendedInfo;

#include <npackoff.h>

#ifdef __cplusplus
extern "C" {
#endif

extern int AddSpaceRestrictionForDirectory
(
   const char *pathName,
   int         value,
   LONG        allowWildCardsFlag
);

extern int AddTrustee
(
   const char *pathName,
   LONG        trusteeID,
   WORD        newRights
);

extern int AddUserSpaceRestriction
(
   int  volume,
   LONG trusteeID,
   LONG value
);

extern int ChangeDirectoryEntry
(
   const char                   *pathName,
   const struct ModifyStructure *modifyVector,
   LONG                          modifyBits,
   LONG                          allowWildCardsFlag
);

extern int ConvertNameToFullPath
(
   char *partialPath,
   char *fullPath
);

extern int ConvertNameToVolumePath
(
   const char *fileName,
   char       *volumePath
);

extern int DeleteTrustee
(
   const char *pathName,
   LONG        trusteeID
);

extern int DeleteUserSpaceRestriction
(
   int  volume,
   LONG trusteeID
);

extern int GetAvailableUserDiskSpace
(
   const char *pathName,
   LONG       *availableSpace
);

extern int GetDiskSpaceUsedByObject
(
   long  trusteeID,
   int   volume,
   LONG *usedSpace
);

extern int GetEffectiveRights
(
   const char *pathName,
   WORD       *accessRights
);

extern int GetMaximumUserSpaceRestriction
(
   long  trusteeID,
   int   volume,
   LONG *maxRestriction
);

extern LONG GetNumberOfVolumes
(
   void
);

extern int GetVolumeInformation
(
   WORD          connectionID,
   BYTE          volumeNumber,
   int           structSize,
   VOLUME_STATS *volumeStatistics
);

extern int GetVolumeInfoWithNumber
(
   BYTE  volumeNumber,
   char *volumeName,
   WORD *totalBlocks,
   WORD *sectorsPerBlock,
   WORD *availableBlocks,
   WORD *totalDirectorySlots,
   WORD *availableDirectorySlots, 
   WORD *volumeIsRemovable
);

extern int GetVolumeName
(
   int   volumeNumber,
   char *volumeName
);

extern int GetVolumeNumber
(
   const char *volumeName, 
   int        *volumeNumber
);

extern int GetVolumeStatistics
(
   WORD         connectionID,
   BYTE         volumeNumber,
   int          structSize,
   VOLUME_INFO *volumeStatistics
);

extern void _makepath
(
   char       *path,
   const char *drive,
   const char *dir,
   const char *fname,
   const char *ext
);

extern int ModifyInheritedRightsMask
(
   const char *path,
   WORD        revokeRightsMask, 
   WORD        grantRightsMask
);

extern int NWGetExtendedVolumeInfo
(
   int                volNumber,
   char              *volName,
   NWVolExtendedInfo *volInfo
);

extern int NWVolumeIsCDROM
(
	LONG volNumber,
	LONG *isCDROM
);

extern int ParsePath
(
   const char *path,
   char       *server,
   char       *volume, 
   char       *directories
);

extern int PurgeTrusteeFromVolume
(
   int  volume, 
   LONG trusteeID
);

extern int ReturnSpaceRestrictionForDirectory
(
   const char *pathName,
   LONG        numberOfStructuresToReturn,
   BYTE       *answerBuffer,
   LONG       *numberOfStructuresReturned
);

extern int ScanBinderyObjectTrusteePaths
(
   LONG  objectID,
   BYTE  volumeNumber,
   int  *sequenceNumber,
   WORD *trusteeAccessMask,
   char *trusteePathName
);

extern int ScanTrustees
(
   const char *pathName,
   LONG        startingOffset,
   LONG        vectorSize,
   LONG       *trusteeVector,
   WORD       *maskVector,
   LONG       *actualVectorSize
);

extern int ScanUserSpaceRestrictions
(
   int   volume,
   LONG *sequenceNumber,
   LONG  numberOfTrusteesToReturn, 
   LONG *answerArea, 
   LONG *numberOfTrusteesReturned
);

extern int SetDirectoryInfo
(
   const char *pathName,
   const BYTE *newCreationDateAndTime,
   LONG        newOwnerObjectID,
   WORD        inheritedRightsMask
);

extern BYTE SetWildcardTranslationMode
(
   BYTE newMode
);

extern void _splitpath
(
   const char *path,
   char       *drive,
   char       *dir,
   char       *fname,
   char       *ext
);

extern char *StripFileServerFromPath
(
   const char *path,
   char       *server
);

extern int UpdateDirectoryEntry
(
   int handle
);

#ifdef __cplusplus
}
#endif


#endif
