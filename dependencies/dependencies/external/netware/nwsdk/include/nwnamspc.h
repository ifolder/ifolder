/******************************************************************************

  %name: nwnamspc.h %
  %version: 17 %
  %date_modified: Wed Jun 14 14:24:20 2000 %
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

#if ! defined ( NWNAMSPC_H )
#define NWNAMSPC_H

#if ! defined ( NWCALDEF_H )
# include "nwcaldef.h"
#endif

#if ! defined ( NWDIRECT_H )
#include  "nwdirect.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

#ifndef SUCCESSFUL
#define SUCCESSFUL                0
#endif

#define MORE_NS_TO_READ           0
#define NO_EXTENDED_NS_INFO       9
#define NS_EOF                    0x8910

#define NW_NS_DOS     0
#define NW_NS_MAC     1
#define NW_NS_NFS     2
#define NW_NS_FTAM    3
#define NW_NS_OS2     4
#define NW_NS_LONG    4

#define NW_DS_DOS     0
#define NW_DS_MAC     1
#define NW_DS_FTAM    2

typedef struct
{
  nuint8  volNumber;
  nuint8  srcNameSpace;
  nuint32 srcDirBase;
  nuint8  dstNameSpace;
  nuint32 dstDirBase;
} NW_IDX;

typedef struct NWNSINFO
{
  nuint32 NSInfoBitMask;
  nuint32 fixedBitMask;
  nuint32 reservedBitMask;
  nuint32 extendedBitMask;
  nuint16 fixedBitsDefined;
  nuint16 reservedBitsDefined;
  nuint16 extendedBitsDefined;
  nuint32 fieldsLenTable[32];
  nuint8  hugeStateInfo[16];
  nuint32 hugeDataLength;
} NW_NS_INFO;

typedef struct
{
  nuint32 spaceAlloc;
  nuint32 attributes;
  nuint16 flags;
  nuint32 dataStreamSize;
  nuint32 totalStreamSize;
  nuint16 numberOfStreams;
  nuint16 creationTime;
  nuint16 creationDate;
  nuint32 creatorID;
  nuint16 modifyTime;
  nuint16 modifyDate;
  nuint32 modifierID;
  nuint16 lastAccessDate;
  nuint16 archiveTime;
  nuint16 archiveDate;
  nuint32 archiverID;
  nuint16 inheritedRightsMask;
  nuint32 dirEntNum;
  nuint32 DosDirNum;
  nuint32 volNumber;
  nuint32 EADataSize;
  nuint32 EAKeyCount;
  nuint32 EAKeySize;
  nuint32 NSCreator;
  nuint8  nameLength;
  nstr8   entryName[256];
} NW_ENTRY_INFO;

typedef struct
{
  nuint32 spaceAlloc;
  nuint32 attributes;
  nuint16 flags;
  nuint32 dataStreamSize;
  nuint32 totalStreamSize;
  nuint16 numberOfStreams;
  nuint16 creationTime;
  nuint16 creationDate;
  nuint32 creatorID;
  nuint16 modifyTime;
  nuint16 modifyDate;
  nuint32 modifierID;
  nuint16 lastAccessDate;
  nuint16 archiveTime;
  nuint16 archiveDate;
  nuint32 archiverID;
  nuint16 inheritedRightsMask;
  nuint32 dirEntNum;
  nuint32 DosDirNum;
  nuint32 volNumber;
  nuint32 EADataSize;
  nuint32 EAKeyCount;
  nuint32 EAKeySize;
  nuint32 NSCreator;
  nuint16 nameLength;
  nstr8   entryName[766];  /* 255*3 + 1 */
} NW_ENTRY_INFO_EXT;

typedef struct
{
  nuint32 dataStreamNumber;
  nuint32 dataStreamFATBlocksSize;    
} NW_DATA_STREAM_FAT_INFO;

typedef struct
{
  nuint32   dataStreamNumber;
  nuint32   dataStreamSize;
} NW_DATA_STREAM_SIZE_INFO;

typedef struct
{
  nuint32 MACCreateTime;
  nuint32 MACBackupTime;
} NW_MAC_TIME;

typedef struct
{
  nuint32 spaceAlloc;
  nuint32 attributes;
  nuint16 flags;
  nuint32 dataStreamSize;
  nuint32 totalStreamSize;
  nuint16 numberOfStreams;
  nuint32 EADataSize;
  nuint32 EAKeyCount;
  nuint32 EAKeySize;
  nuint16 archiveTime;
  nuint16 archiveDate;
  nuint32 archiverID;
  nuint16 modifyTime;
  nuint16 modifyDate;
  nuint32 modifierID;
  nuint16 lastAccessDate;
  nuint16 creationTime;
  nuint16 creationDate;
  nuint32 creatorID;
  nuint32 NSCreator;
  nuint32 dirEntNum;
  nuint32 DosDirNum;
  nuint32 volNumber;
  nuint16 inheritedRightsMask;
  nuint16 currentReferenceID;
  nuint32 NSFileAttributes;
  nuint32 numberOfDataStreamFATInfo;
  NW_DATA_STREAM_FAT_INFO dataStreamFATInfo[3];
  nuint32 numberOfDataStreamSizeInfo;
  NW_DATA_STREAM_SIZE_INFO dataStreamSizeInfo[3];
  nint32  secondsRelativeToTheYear2000;
  nuint8  DOSNameLen;
  nstr8   DOSName[13];
  nuint32 flushTime;
  nuint32 parentBaseID; 
  nuint8  MacFinderInfo[32];
  nuint32 siblingCount;
  nuint32 effectiveRights;
  NW_MAC_TIME MacTime;  
  nuint16 lastAccessedTime;  
  nuint8  nameLength;
  nstr8   entryName[256];
} NW_ENTRY_INFO2;


typedef struct _MODIFY_DOS_INFO
{
  nuint32   attributes;
  nuint16   createDate;
  nuint16   createTime;
  nuint32   creatorID;
  nuint16   modifyDate;
  nuint16   modifyTime;
  nuint32   modifierID;
  nuint16   archiveDate;
  nuint16   archiveTime;
  nuint32   archiverID;
  nuint16   lastAccessDate;
  nuint16   inheritanceGrantMask;
  nuint16   inheritanceRevokeMask;
  nuint32   maximumSpace;
} MODIFY_DOS_INFO;

typedef struct
{
  nuint8  volNumber;
  nuint32 dirNumber;
  nuint32 searchDirNumber;
} SEARCH_SEQUENCE;

typedef struct
{
  pnstr8  srcPath;
  pnstr8  dstPath;
  nuint16 dstPathSize;
} NW_NS_PATH;

typedef struct
{
  nuint8  openCreateMode;
  nuint16 searchAttributes;
  nuint32 reserved;
  nuint32 createAttributes;
  nuint16 accessRights;
  nuint32 NetWareHandle;
  nuint8  openCreateAction;
} NW_NS_OPENCREATE, NW_NS_OPEN;


/* open/create modes */
#define OC_MODE_OPEN      0x01
#define OC_MODE_TRUNCATE  0x02
#define OC_MODE_REPLACE   0x02
#define OC_MODE_CREATE    0x08

/* open/create results */
#define OC_ACTION_NONE     0x00
#define OC_ACTION_OPEN     0x01
#define OC_ACTION_CREATE   0x02
#define OC_ACTION_TRUNCATE 0x04
#define OC_ACTION_REPLACE  0x04

/* return info mask */
#define IM_NAME               0x0001L
#define IM_ENTRY_NAME         0x0001L
#define IM_SPACE_ALLOCATED    0x0002L
#define IM_ATTRIBUTES         0x0004L
#define IM_SIZE               0x0008L
#define IM_TOTAL_SIZE         0x0010L
#define IM_EA                 0x0020L
#define IM_ARCHIVE            0x0040L
#define IM_MODIFY             0x0080L
#define IM_CREATION           0x0100L
#define IM_OWNING_NAMESPACE   0x0200L
#define IM_DIRECTORY          0x0400L
#define IM_RIGHTS             0x0800L
#define IM_ALMOST_ALL         0x0FEDL
#define IM_ALL                0x0FFFL
#define IM_REFERENCE_ID       0x1000L
#define IM_NS_ATTRIBUTES      0x2000L
#define IM_DATASTREAM_SIZES   0x4000L
#define IM_DATASTREAM_ACTUAL  0x4000L
#define IM_DATASTREAM_LOGICAL 0x8000L
#define IM_LASTUPDATEDINSECONDS 0x00010000L
#define IM_DOSNAME              0x00020000L
#define IM_FLUSHTIME            0x00040000L
#define IM_PARENTBASEID         0x00080000L
#define IM_MACFINDER            0x00100000L
#define IM_SIBLINGCOUNT         0x00200000L
#define IM_EFECTIVERIGHTS       0x00400000L
#define IM_MACTIME              0x00800000L
#define IM_LASTACCESSEDTIME     0x01000000L
#define IM_EXTENDED_ALL         0x01FFF000L
#define IM_NSS_LARGE_SIZES      0x40000000L
#define IM_COMPRESSED_INFO      0x80000000L
#define IM_NS_SPECIFIC_INFO     0x80000000L

/* access rights attributes */
#ifndef AR_READ_ONLY
#define AR_READ            0x0001
#define AR_WRITE           0x0002
#define AR_READ_ONLY       0x0001
#define AR_WRITE_ONLY      0x0002
#define AR_DENY_READ       0x0004
#define AR_DENY_WRITE      0x0008
#define AR_COMPATIBILITY   0x0010
#define AR_WRITE_THROUGH   0x0040
#define AR_OPEN_COMPRESSED 0x0100
#endif

/* Trustee Access Rights in a network directory */
/* NOTE: TA_OPEN is obsolete in 3.x */
#ifndef TA_NONE
#define TA_NONE               0x00
#define TA_READ               0x01
#define TA_WRITE              0x02
#define TA_CREATE             0x08
#define TA_DELETE             0x10
#define TA_ACCESSCONTROL      0x20
#define TA_SEEFILES           0x40
#define TA_MODIFY             0x80
#define TA_ALL                0xFB
/* Old names for rights */
#define TA_OPEN               0x04
#define TA_OWNERSHIP          0x20
#define TA_SEARCH             0x40
/* Misc defines */
#define TA_SUPERVISOR         0x0100
#define TA_ALL_16             0x01FB
#endif

/* search attributes */
#ifndef SA_HIDDEN
#define SA_NORMAL         0x0000
#define SA_HIDDEN         0x0002
#define SA_SYSTEM         0x0004
#define SA_SUBDIR_ONLY    0x0010
#define SA_SUBDIR_FILES   0x8000
#define SA_ALL            0x8006
#endif

#define NW_TYPE_FILE      0x8000
#define NW_TYPE_SUBDIR    0x0010

#define NW_NAME_CONVERT     0x03
#define NW_NO_NAME_CONVERT  0x04

/* modify mask - use with MODIFY_DOS_INFO structure */
#define DM_FILENAME               0x0001L
#define DM_ATTRIBUTES             0x0002L
#define DM_CREATE_DATE            0x0004L
#define DM_CREATE_TIME            0x0008L
#define DM_CREATOR_ID             0x0010L
#define DM_ARCHIVE_DATE           0x0020L
#define DM_ARCHIVE_TIME           0x0040L
#define DM_ARCHIVER_ID            0x0080L
#define DM_MODIFY_DATE            0x0100L
#define DM_MODIFY_TIME            0x0200L
#define DM_MODIFIER_ID            0x0400L
#define DM_LAST_ACCESS_DATE       0x0800L
#define DM_INHERITED_RIGHTS_MASK  0x1000L
#define DM_MAXIMUM_SPACE          0x2000L

#if defined( N_PLAT_NLM )
   #define NWGetNSLoadedList     NWGetNSLoadedList2
   #define NWGetNSInfo           NWGetNSInfo2
#endif

N_EXTERN_LIBRARY( NWCCODE )
NWGetDirectoryBase
(
   NWCONN_HANDLE       conn,
   nuint8              dirHandle,
   const nstr8 N_FAR * path,
   nuint8              dstNamSpc,
   NW_IDX N_FAR *      idxStruct
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetDirectoryBaseExt
(
   NWCONN_HANDLE       conn,
   nuint8              dirHandle,
   const nstr8 N_FAR * path,
   nuint8              dstNamSpc,
   NW_IDX N_FAR *      idxStruct
);


N_EXTERN_LIBRARY( NWCCODE )
NWScanNSEntryInfo
(
   NWCONN_HANDLE           conn,
   nuint8                  dirHandle,
   nuint8                  namSpc,
   nuint16                 attrs,
   SEARCH_SEQUENCE N_FAR * sequence,
   const nstr8     N_FAR * searchPattern,
   nuint32                 retInfoMask,
   NW_ENTRY_INFO   N_FAR * entryInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanNSEntryInfoExt
(
   NWCONN_HANDLE               conn,
   nuint8                      dirHandle,
   nuint8                      namSpc,
   nuint16                     attrs,
   SEARCH_SEQUENCE N_FAR *     sequence,
   const nstr8     N_FAR *     searchPattern,
   nuint32                     retInfoMask,
   NW_ENTRY_INFO_EXT   N_FAR * entryInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanNSEntryInfo2
(
   NWCONN_HANDLE           conn,
   nuint8                  dirHandle,
   nuint8                  namSpc,
   nuint16                 attrs,
   SEARCH_SEQUENCE N_FAR * sequence,
   const nstr8     N_FAR * searchPattern,
   nuint32                 retInfoMask,
   NW_ENTRY_INFO2  N_FAR * entryInfo2
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetNSLoadedList
(
   NWCONN_HANDLE  conn,
   nuint8         volNum,
   nuint8         maxListLen,
   pnuint8        NSLoadedList,
   pnuint8        actualListLen
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetOwningNameSpace
(
   NWCONN_HANDLE       conn,
   nuint8              dirHandle,
   const nstr8 N_FAR * path,
   pnuint8             namSpc
);

N_EXTERN_LIBRARY( NWCCODE )
NWOpenCreateNSEntry
(
   NWCONN_HANDLE            conn,
   nuint8                   dirHandle,
   nuint8                   namSpc,
   const nstr8      N_FAR * path,
   NW_NS_OPENCREATE N_FAR * NSOpenCreate,
   NWFILE_HANDLE    N_FAR * fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWOpenCreateNSEntryExt
(
   NWCONN_HANDLE            conn,
   nuint8                   dirHandle,
   nuint8                   namSpc,
   const nstr8      N_FAR * path,
   NW_NS_OPENCREATE N_FAR * NSOpenCreate,
   NWFILE_HANDLE    N_FAR * fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWOpenNSEntry
(
   NWCONN_HANDLE         conn,
   nuint8                dirHandle,
   nuint8                namSpc,
   nuint8                dataStream,
   const nstr8   N_FAR * path,
   NW_NS_OPEN    N_FAR * NSOpen,
   NWFILE_HANDLE N_FAR * fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWOpenNSEntryExt
(
   NWCONN_HANDLE         conn,
   nuint8                dirHandle,
   nuint8                namSpc,
   nuint8                dataStream,
   const nstr8   N_FAR * path,
   NW_NS_OPEN    N_FAR * NSOpen,
   NWFILE_HANDLE N_FAR * fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetLongName
(
   NWCONN_HANDLE       conn,
   nuint8              dirHandle,
   nuint8              namSpc,
   const nstr8 N_FAR * dstPath,
   nuint16             dstType,
   const nstr8 N_FAR * longName
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetLongName
(
   NWCONN_HANDLE       conn,
   nuint8              dirHandle,
   const nstr8 N_FAR * path,
   nuint8              srcNamSpc,
   nuint8              dstNamSpc,
   pnstr8              longName
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetLongNameExt
(
   NWCONN_HANDLE       conn,
   nuint8              dirHandle,
   const nstr8 N_FAR * path,
   nuint8              srcNamSpc,
   nuint8              dstNamSpc,
   pnstr8              longName
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetNSInfo
(
   NWCONN_HANDLE        conn,
   const NW_IDX N_FAR * idxStruct,
   NW_NS_INFO   N_FAR * NSInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWWriteNSInfo
(
   NWCONN_HANDLE            conn,
   const NW_IDX     N_FAR * idxStruct,
   const NW_NS_INFO N_FAR * NSInfo,
   const nuint8     N_FAR * data
);

N_EXTERN_LIBRARY( NWCCODE )
NWWriteNSInfoExt
(
   NWCONN_HANDLE            conn,
   const NW_IDX     N_FAR * idxStruct,
   const NW_NS_INFO N_FAR * NSInfo,
   const nuint8     N_FAR * data
);

N_EXTERN_LIBRARY( NWCCODE )
NWWriteExtendedNSInfo
(
   NWCONN_HANDLE            conn,
   const NW_IDX     N_FAR * idxStruct,
   NW_NS_INFO       N_FAR * NSInfo,
   const nuint8     N_FAR * data
);

N_EXTERN_LIBRARY( NWCCODE )
NWReadNSInfo
(
   NWCONN_HANDLE            conn,
   const NW_IDX     N_FAR * idxStruct,
   const NW_NS_INFO N_FAR * NSInfo,
   pnuint8                  data
);

N_EXTERN_LIBRARY( NWCCODE )
NWReadNSInfoExt
(
   NWCONN_HANDLE            conn,
   const NW_IDX     N_FAR * idxStruct,
   const NW_NS_INFO N_FAR * NSInfo,
   pnuint8                  data
);

N_EXTERN_LIBRARY( NWCCODE )
NWReadExtendedNSInfo
(
   NWCONN_HANDLE        conn,
   const NW_IDX N_FAR * idxStruct,
   NW_NS_INFO   N_FAR * NSInfo,
   pnuint8              data
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetNSPath
(
   NWCONN_HANDLE  conn,
   nuint8         dirHandle,
   nuint16        fileFlag,
   nuint8         srcNamSpc,
   nuint8         dstNamSpc,
   NW_NS_PATH N_FAR *NSPath
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetNSPathExt
(
   NWCONN_HANDLE  conn,
   nuint8         dirHandle,
   nuint16        fileFlag,
   nuint8         srcNamSpc,
   nuint8         dstNamSpc,
   NW_NS_PATH N_FAR *NSPath
);

N_EXTERN_LIBRARY( NWCCODE )
NWAllocTempNSDirHandle2
(
   NWCONN_HANDLE       conn,
   nuint8              dirHandle,
   const nstr8 N_FAR * path,
   nuint8              namSpc,
   pnuint8             newDirHandle,
   nuint8              newNamSpc
);

N_EXTERN_LIBRARY( NWCCODE )
NWAllocTempNSDirHandle2Ext
(
   NWCONN_HANDLE       conn,
   nuint8              dirHandle,
   const nstr8 N_FAR * path,
   nuint8              namSpc,
   pnuint8             newDirHandle,
   nuint8              newNamSpc
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetNSEntryInfo
(
   NWCONN_HANDLE         conn,
   nuint8                dirHandle,
   const nstr8 N_FAR *   path,
   nuint8                srcNamSpc,
   nuint8                dstNamSpc,
   nuint16               searchAttrs,
   nuint32               retInfoMask,
   NW_ENTRY_INFO N_FAR * entryInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetNSEntryInfoExt
(
   NWCONN_HANDLE             conn,
   nuint8                    dirHandle,
   const nstr8 N_FAR *       path,
   nuint8                    srcNamSpc,
   nuint8                    dstNamSpc,
   nuint16                   searchAttrs,
   nuint32                   retInfoMask,
   NW_ENTRY_INFO_EXT N_FAR * entryInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWNSGetMiscInfo
(
   NWCONN_HANDLE       conn,
   nuint8              dirHandle,
   const nstr8 N_FAR * path,
   nuint8              dstNameSpace,
   NW_IDX      N_FAR * idxStruct
);

N_EXTERN_LIBRARY( NWCCODE )
NWOpenDataStream
(
  NWCONN_HANDLE         conn,
  nuint8                dirHandle,
  const nstr8   N_FAR * fileName,
  nuint16               dataStream,
  nuint16               attrs,
  nuint16               accessMode,
  pnuint32              NWHandle,
  NWFILE_HANDLE N_FAR * fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWNSRename
(
   NWCONN_HANDLE       conn,
   nuint8              dirHandle,
   nuint8              namSpc,
   const nstr8 N_FAR * oldName,
   nuint16             oldType,
   const nstr8 N_FAR * newName,
   nuint8              renameFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWNSRenameExt
(
   NWCONN_HANDLE       conn,
   nuint8              dirHandle,
   nuint8              namSpc,
   const nstr8 N_FAR * oldName,
   nuint16             oldType,
   const nstr8 N_FAR * newName,
   nuint8              renameFlag
);


N_EXTERN_LIBRARY( NWCCODE )
NWSetNSEntryDOSInfo
(
   NWCONN_HANDLE           conn,
   nuint8                  dirHandle,
   const nstr8     N_FAR * path,
   nuint8                  namSpc,
   nuint16                 searchAttrs,
   nuint32                 modifyDOSMask,
   MODIFY_DOS_INFO N_FAR * dosInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetNSEntryDOSInfoExt
(
   NWCONN_HANDLE           conn,
   nuint8                  dirHandle,
   const nstr8     N_FAR * path,
   nuint8                  namSpc,
   nuint16                 searchAttrs,
   nuint32                 modifyDOSMask,
   MODIFY_DOS_INFO N_FAR * dosInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFullPath
(
   NWCONN_HANDLE  conn,
   nuint8         volNum,
   nuint32        dirBase,
   nuint16        handleFlag,
   nint           srcNamSpc,
   nint           dstNamSpc,
   nuint16        maxPathLen,
   pnstr8         path,
   pnuint16       pathType
);

N_EXTERN_LIBRARY( NWCCODE )
NWDeleteNSEntry
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * fileName,
   nuint8              nameSpace,
   nuint16             searchAttr
);

N_EXTERN_LIBRARY( NWCCODE )
NWDeleteNSEntryExt
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * fileName,
   nuint8              nameSpace,
   nuint16             searchAttr
);

N_EXTERN_LIBRARY( NWCCODE )
NWNSGetDefaultNS
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   pnuint8             pbuDefaultNameSpace
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanNSEntryInfoSet
(
   NWCONN_HANDLE           conn,
   NWDIR_HANDLE            dirHandle,
   nuint8                  buNameSpace,
   nuint16                 suAttr,
   SEARCH_SEQUENCE N_FAR * pIterHnd,
   const nstr8     N_FAR * pbstrSrchPattern,
   nuint32                 luRetMask,
   pnuint8                 pbuMoreEntriesFlag,
   pnuint16                psuNumReturned,
   nuint16                 suNumItems,
   NW_ENTRY_INFO   N_FAR * pEntryInfo
);
  
N_EXTERN_LIBRARY( NWCCODE )
NWAddTrusteeToNSDirectory
(
   NWCONN_HANDLE       conn,
   nuint8              namSpc,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   nuint32             trusteeID,
   nuint8              rightsMask
);

N_EXTERN_LIBRARY( NWCCODE )
NWDeleteTrusteeFromNSDirectory
(
   NWCONN_HANDLE       conn,
   nuint8              namSpc,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * dirPath,
   nuint32             objID
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanNSDirectoryForTrustees
(
   NWCONN_HANDLE  conn,
   nuint8         namSpc,
   NWDIR_HANDLE   dirHandle,
   const nstr8 N_FAR * pbstrSrchPath,
   pnuint32       pluIterHnd,
   pnstr8         pbstrDirName,
   pnuint32       pluDirDateTime,
   pnuint32       pluOwnerID,
   TRUSTEE_INFO N_FAR * trusteeList
);

#ifdef NWDOS
#define __NWGetCurNS(a, b, c) NW_NS_DOS
#else
N_EXTERN_LIBRARY( nuint16 )
__NWGetCurNS
(
   NWCONN_HANDLE  conn,
   nuint8         dirHandle,
   pnstr8         path
);
#endif

#if defined N_PLAT_NLM
N_EXTERN_LIBRARY( nuint8 )
SetCurrentNameSpace
(
   nuint8 newNameSpace
);

N_EXTERN_LIBRARY( nuint8 )
SetTargetNameSpace
(
   nuint8 newNameSpace
);


#endif

#ifdef __cplusplus
}
#endif

   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_namspc.h"
   #endif 
#endif /* !defined(__NOVELL_LIBC__) */

#include "npackoff.h"
#endif
