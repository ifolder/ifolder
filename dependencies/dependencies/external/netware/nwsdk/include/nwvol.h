/******************************************************************************

  %name: nwvol.h %
  %version: 7 %
  %date_modified: Tue Oct 19 13:37:06 1999 %
  $Copyright:

  Copyright (c) 1989-1995 Novell, Inc.  All Rights Reserved.                      

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

#if ! defined ( NWVOL_H )
#define NWVOL_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#if ! defined ( NWAPIDEF_H )
#include "nwapidef.h"
#endif


#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

/* define volume types  */

#define VINetWare386    0
#define VINetWare286    1
#define VINetWare386v30 2
#define VINetWare386v31 3

/*    define the extended volume information status flag bits  */

#define NWSubAllocEnabledBit    0x01
#define NWCompressionEnabledBit 0x02
#define NWMigrationEnabledBit   0x04
#define NWAuditingEnabledBit    0x08
#define NWReadOnlyEnabledBit    0x10
#define NWPSSEnabledBit			  0x80000000L

/* define the constant for volume request flag for NWScanMountedVolumeList */

#define NW_VOLUME_NUMBER_ONLY       0
#define NW_VOLUME_NUMBER_AND_NAME   1

typedef struct
{
  nuint32 objectID;
  nuint32 restriction;
} NWOBJ_REST;

typedef struct
{
  nuint8  numberOfEntries;
  struct
  {
    nuint32 objectID;
    nuint32 restriction;
  } resInfo[12];
} NWVolumeRestrictions;

typedef struct
{
  nuint8  numberOfEntries;
  struct
  {
    nuint32 objectID;
    nuint32 restriction;
  } resInfo[16];
} NWVOL_RESTRICTIONS;

typedef struct
{
  nint32    systemElapsedTime;
  nuint8    volumeNumber;
  nuint8    logicalDriveNumber;
  nuint16   sectorsPerBlock;
  nuint16   startingBlock;
  nuint16   totalBlocks;
  nuint16   availableBlocks;
  nuint16   totalDirectorySlots;
  nuint16   availableDirectorySlots;
  nuint16   maxDirectorySlotsUsed;
  nuint8    isHashing;
  nuint8    isCaching;
  nuint8    isRemovable;
  nuint8    isMounted;
  nstr8     volumeName[16];
} VOL_STATS;


typedef struct ExtendedVolInfo_tag
{
  nuint32 volType;
  nuint32 statusFlag;
  nuint32 sectorSize;
  nuint32 sectorsPerCluster;
  nuint32 volSizeInClusters;
  nuint32 freeClusters;
  nuint32 subAllocFreeableClusters;
  nuint32 freeableLimboSectors;
  nuint32 nonfreeableLimboSectors;
  nuint32 availSubAllocSectors;            /* non freeable */
  nuint32 nonuseableSubAllocSectors;
  nuint32 subAllocClusters;
  nuint32 numDataStreams;
  nuint32 numLimboDataStreams;
  nuint32 oldestDelFileAgeInTicks;
  nuint32 numCompressedDataStreams;
  nuint32 numCompressedLimboDataStreams;
  nuint32 numNoncompressibleDataStreams;
  nuint32 precompressedSectors;
  nuint32 compressedSectors;
  nuint32 numMigratedDataStreams;
  nuint32 migratedSectors;
  nuint32 clustersUsedByFAT;
  nuint32 clustersUsedByDirs;
  nuint32 clustersUsedByExtDirs;
  nuint32 totalDirEntries;
  nuint32 unusedDirEntries;
  nuint32 totalExtDirExtants;
  nuint32 unusedExtDirExtants;
  nuint32 extAttrsDefined;
  nuint32 extAttrExtantsUsed;
  nuint32 DirectoryServicesObjectID;
  nuint32 volLastModifiedDateAndTime;
} NWVolExtendedInfo;

typedef struct NWVolMountNumWithName_tag
{
	nuint32 volumeNumber;
	nstr8   volumeName[NW_MAX_VOLUME_NAME_LEN];  
} NWVolMountNumWithName;


N_EXTERN_LIBRARY( NWCCODE )
NWGetDiskUtilization
(
  NWCONN_HANDLE   conn,
  nuint32         objID,
  nuint8          volNum,
  pnuint16        usedDirectories,
  pnuint16        usedFiles,
  pnuint16        usedBlocks
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjDiskRestrictions
(
  NWCONN_HANDLE   conn,
  nuint8          volNumber,
  nuint32         objectID,
  pnuint32        restriction,
  pnuint32        inUse
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanVolDiskRestrictions
(
  NWCONN_HANDLE   conn,
  nuint8          volNum,
  pnuint32        iterhandle,
  NWVolumeRestrictions N_FAR * volInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanVolDiskRestrictions2
(
  NWCONN_HANDLE   conn,
  nuint8          volNum,
  pnuint32        iterhandle,
  NWVOL_RESTRICTIONS N_FAR * volInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWRemoveObjectDiskRestrictions
(
  NWCONN_HANDLE   conn,
  nuint8          volNum,
  nuint32         objID
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetObjectVolSpaceLimit
(
  NWCONN_HANDLE   conn,
  nuint16         volNum,
  nuint32         objID,
  nuint32         restriction
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetVolumeInfoWithHandle
(
  NWCONN_HANDLE   conn,
  NWDIR_HANDLE    dirHandle,
  pnstr8          volName,
  pnuint16        totalBlocks,
  pnuint16        sectorsPerBlock,
  pnuint16        availableBlocks,
  pnuint16        totalDirEntries,
  pnuint16        availableDirEntries,
  pnuint16        volIsRemovableFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetVolumeInfoWithNumber
(
  NWCONN_HANDLE   conn,
  nuint16         volNum,
  pnstr8          volName,
  pnuint16        totalBlocks,
  pnuint16        sectorsPerBlock,
  pnuint16        availableBlocks,
  pnuint16        totalDirEntries,
  pnuint16        availableDirEntries,
  pnuint16        volIsRemovableFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetVolumeName
(
  NWCONN_HANDLE   conn,
  nuint16         volNum,
  pnstr8          volName
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetVolumeNumber
(
  NWCONN_HANDLE       conn,
  const nstr8 N_FAR * volName,
  pnuint16            volNum
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetVolumeStats
(
  NWCONN_HANDLE     conn,
  nuint8            volNum,
  VOL_STATS N_FAR * volInfo
);

#if defined( N_PLAT_NLM )
   #define    NWGetExtendedVolumeInfo     NWGetExtendedVolumeInfo2
#endif

N_EXTERN_LIBRARY( NWCCODE )
NWGetExtendedVolumeInfo
(
  NWCONN_HANDLE   conn,
  nuint16         volNum,
  NWVolExtendedInfo N_FAR * volInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanMountedVolumeList
(
   NWCONN_HANDLE  		conn,
   nuint32              volRequestFlags,
   nuint32        		nameSpace,
	pnuint32       		iterHandle,
   nuint32              numberItems,
   pnuint32             numberReturned,
	NWVolMountNumWithName	N_FAR * volMountArr
);

#ifdef __cplusplus
}
#endif

#include "npackoff.h"
#endif
