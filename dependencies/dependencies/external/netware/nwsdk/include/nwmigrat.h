/******************************************************************************

  %name: nwmigrat.h %
  %version: 4 %
  %date_modified: Tue Oct 19 13:44:22 1999 %
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

#if ! defined ( NWMIGRAT_H )
#define NWMIGRAT_H

#if ! defined ( NTYPES_H )
# include "ntypes.h"
#endif

#if ! defined ( NWCALDEF_H )
# include "nwcaldef.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

#define MAX_NUM_OF_DATA_STREAMS       3
#define MAX_SIZE_OF_SM_STRING       128
#define MAX_SIZE_OF_SM_INFO         128
#define MAX_NUM_OF_SM                32

#define ERR_INVALID_SM_ID           240
#define ERR_SM_ALREADY_REGISTERED   241
#define ERR_SM_CREATE_FAILED        242
#define ERR_SM_CLOSE_FAILED         243
#define ERR_SM_WRITE_NO_SPACE       244
#define ERR_SM_WRITE_IO_ERROR       245
#define ERR_SM_READ_IO_ERROR        246
#define ERR_SM_OPEN_FAILED          247
#define ERR_SM_DELETE_FAILED        248

typedef struct
{
   nuint32 IOStatus;
   nuint32 InfoBlockSize;
   nuint32 AvailSpace;
   nuint32 UsedSpace;
   /* A length preceded string is followed by SMInfo data */
   nuint8 SMInfo[MAX_SIZE_OF_SM_STRING + MAX_SIZE_OF_SM_INFO];
}  SUPPORT_MODULE_INFO;

typedef struct
{
   nuint32 numberOfSMs;
   nuint32 SMIDs[MAX_NUM_OF_SM];
}  SUPPORT_MODULE_IDS;

#if defined( N_PLAT_NLM )
   #define  NWMoveFileToDM                NWMoveFileToDM2
   #define  NWMoveFileFromDM              NWMoveFileFromDM2
   #define  NWGetDMFileInfo               NWGetDMFileInfo2
   #define  NWGetDMVolumeInfo             NWGetDMVolumeInfo2
   #define  NWGetDefaultSupportModule     NWGetDefaultSupportModule2
   #define  NWSetDefaultSupportModule     NWSetDefaultSupportModule2
   #define  NWGetDataMigratorInfo         NWGetDataMigratorInfo2
   #define  NWGetSupportModuleInfo        NWGetSupportModuleInfo2
#endif

N_EXTERN_LIBRARY( NWCCODE )
NWMoveFileToDM
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   nuint8              nameSpace,
   nuint32             supportModuleID,
   nuint32             saveKeyFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWMoveFileFromDM
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   nuint8              nameSpace
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetDMFileInfo
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   nuint8              nameSpace,
   pnuint32            supportModuleID,
   pnuint32            restoreTime,
   pnuint32            dataStreams
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetDMVolumeInfo
(
   NWCONN_HANDLE  conn,
   nuint16        volume,
   nuint32        supportModuleID,
   pnuint32       numberOfFilesMigrated,
   pnuint32       totalMigratedSize,
   pnuint32       spaceUsedOnDM,
   pnuint32       limboSpaceUsedOnDM,
   pnuint32       spaceMigrated,
   pnuint32       filesInLimbo
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetSupportModuleInfo
(
   NWCONN_HANDLE  conn,
   nuint32        infomationLevel,
   nuint32        supportModuleID,
   pnuint8        returnInfo,
   pnuint32       returnInfoLen
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetDataMigratorInfo
(
   NWCONN_HANDLE  conn,
   pnuint32       DMPresentFlag,
   pnuint32       majorVersion,
   pnuint32       minorVersion,
   pnuint32       DMSMRegistered
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetDefaultSupportModule
(
   NWCONN_HANDLE  conn,
   pnuint32       supportModuleID
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetDefaultSupportModule
(
   NWCONN_HANDLE  conn,
   pnuint32       supportModuleID
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetSupportModuleCapacity
(
   NWCONN_HANDLE  conn,
   nuint32        luSupportModuleID,
   nuint32        luVolume,
   nuint32        luDirectoryBase,
   pnuint32       pluSMBlockSizeInSectors,
   pnuint32       pluSMTotalBlocks,
   pnuint32       pluSMUsedBlocks
);

#ifdef __cplusplus
}
#endif

#include "npackoff.h"
#endif
