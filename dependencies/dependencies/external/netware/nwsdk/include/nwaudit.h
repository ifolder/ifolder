/******************************************************************************

  %name: nwaudit.h %
  %version: 3 %
  %date_modified: Wed Dec 18 12:05:36 1996 %
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

#if ! defined ( NWAUDIT_H )
#define NWAUDIT_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#if ! defined ( NWADEVNT_H )
#include "nwadevnt.h"
#endif

#include "npackon.h"

/* Error codes */
#define ERROR_AUDITING_NOT_INITIALIZED  224

/* bit map sizes */
#define NW_AUDIT_NUMBER_EVENT_BITS      256
#define NW_AUDIT_NUMBER_EVENT_BITS_TNW  512

/* audit ID types */
#define AUDIT_ID_IS_TRUSTED_NETWARE       2
#define AUDIT_ID_IS_CONTAINER             1
#define AUDIT_ID_IS_VOLUME                0

/* audit flags */
#define DiscardAuditRcdsOnErrorFlag 0x01
#define ConcurrentVolAuditorAccess  0x02
#define DualLevelPasswordsActive    0x04
#define BroadcastWarningsToAllUsers 0x08
#define LevelTwoPasswordSet         0x10
#define ArchiveAuditFileOnErrorFlag 0x20

typedef struct tagNWADOpenStatus
{
   nuint32 auditingStatus;
   nuint32 isTrustedNetWare;
   nuint32 trustedNetWareStatus;
   nuint32 reserved1; /* Future status */
   nuint32 reserved2;
   nuint32 reserved3;
   nuint32 reserved4;
} NWADOpenStatus, N_FAR *pNWADOpenStatus;

typedef struct tagTIMESTAMP
{
   nuint32 seconds;
   nuint16 replicaNumber;
   nuint16 event;
} TIMESTAMP;

typedef struct tagNWAuditBitMap
{
   nuint8 bitMap[NW_AUDIT_NUMBER_EVENT_BITS / 8];
}NWAuditBitMap, N_FAR *pNWAuditBitMap;

typedef struct tagNWAuditBitMapTNW
{
   nuint8 bitMap[NW_AUDIT_NUMBER_EVENT_BITS_TNW / 8];
}NWAuditBitMapTNW, N_FAR *pNWAuditBitMapTNW;

typedef struct tagNWAuditFileList
{
   nuint32 fileCreateDateTime[16];
   nuint32 fileSize[16];
}NWAuditFileList, N_FAR *pNWAuditFileList;

typedef struct tagNWConfigHeader
{
   nuint16        fileVersionDate;
   nuint8         auditFlags;
   nuint8         errMsgDelayMinutes;
   nuint8         reserved1[16];
   nuint32        auditFileMaxSize;
   nuint32        auditFileSizeThreshold;
   nuint32        auditRecordCount;
   nuint32        historyRecordCount;
   nuint8         reserved2[16];
   nuint32        reserved3[3];
   nuint8         auditEventBitMap[NW_AUDIT_NUMBER_EVENT_BITS / 8];
   nuint32        auditFileCreationDateTime;
   nuint8         reserved4[8];
   nuint16        auditFlags2;
   nuint16        fileVersionDate2;
   nuint8         fileArchiveDays;
   nuint8         fileArchiveHour;
   nuint8         numOldAuditFilesToKeep;
   nuint8         reserved5;
   nuint32        headerChecksum;
   nuint32        headerModifiedCounter;
   nuint32        reserved6;
   /* Trusted NetWare uses the following two fields */
   nuint8         newBitMap[64];  /* Tusted NetWare uses this bit map instead of volumeAuditEventBitMap above */
   nuint8         reserved7[64];
} NWConfigHeader, N_FAR *pNWConfigHeader;

typedef struct tagNWDSContainerConfigHdr
{
   nuint16     fileVersionDate;
   nuint8      auditFlags;
   nuint8      errMsgDelayMinutes;
   nuint32     containerID;
   nuint32     reserved1;
   TIMESTAMP   creationTS;
   nuint32     bitMap;
   nuint32     auditFileMaxSize;
   nuint32     auditFileSizeThreshold;
   nuint32     auditRecordCount;
   nuint16     replicaNumber;
   nuint8      enabledFlag;
   nuint8      fileArchiveDays;
   nuint8      fileArchiveHour;
   nuint8      numOldFilesToKeep;
   nuint16     numberReplicaEntries;
   nuint32     auditFileCreationDateTime;
   nuint8      reserved2[8];
   nuint32     partitionID;
   nuint32     headerChecksum;
   nuint32     reserved3[4];
   nuint32     auditDisabledCounter;
   nuint32     auditEnabledCounter;
   nuint8      reserved4[32];
   nuint32     hdrModifiedCounter;
   nuint32     fileResetCounter;
   /* Trusted NetWare uses the following two fields */
   nuint8      newBitMap[64]; /* Tusted NetWare uses this bit map */
   nuint8      reserved5[64];
} NWDSContainerConfigHdr, N_FAR *pNWDSContainerConfigHdr;

typedef struct tagNWAuditStatus
{
   nuint16  auditingVersionDate;
   nuint16  auditFileVersionDate;
   nuint32  auditingEnabledFlag;
   nuint32  auditFileSize;
   nuint32  modifiedCounter;
   nuint32  auditFileMaxSize;
   nuint32  auditFileSizeThreshold;
   nuint32  auditRecordCount;
   nuint32  auditingFlags;
} NWAuditStatus, N_FAR *pNWAuditStatus;

typedef struct tagNWAuditRecord
{
   nuint32 recordLength;
   pnuint8 record;
} NWAuditRecord, N_FAR *pNWAuditRecord;

typedef struct tagNWADAuditPolicy
{
   nuint16 policyVersionDate;
   nuint16 reserved;
   nuint8  auditFlags[4];
   nuint32 auditFileMaxSize;
   nuint32 auditFileThresholdSize;
   nuint8  fileArchiveDays;
   nuint8  fileArchiveHour;
   nuint8  numOldAuditFilesToKeep;
   nuint8  userRestrictionFlag;
   nuint32 modifiedCounter;
   nuint32 auditOverflowFileSize;
   nuint32 reservedLong;
   nuint8  auditedEventBitMap[256];
} NWADAuditPolicy, N_FAR *pNWADAuditPolicy;

#ifdef __cplusplus
   extern "C" {
#endif

N_EXTERN_LIBRARY( void )
NWGetNWADVersion
(
  pnuint8 majorVersion,
  pnuint8 minorVersion,
  pnuint8 revisionLevel,
  pnuint8 betaReleaseLevel
);

/* allocate auditHandle for use in other Auditing calls */
N_EXTERN_LIBRARY( NWRCODE )
NWADOpen
(
   NWCONN_HANDLE     conn,
   nuint32           auditIDType,
   nuint32           auditID,
   pnptr             auditHandle,   /* allocate auditHandle */
   pNWADOpenStatus   openStatus
);

/* free auditHandle and NULL the pointer */
N_EXTERN_LIBRARY( NWRCODE )
NWADClose
(
   pnptr          auditHandle  /* free auditHandle */
);

#define NWGetVolumeAuditStats(a, b, c, d) \
      NWADGetStatus(a, AUDIT_ID_IS_VOLUME, b, c, d)
#define NWDSGetContainerAuditStats(a, b, c, d) \
      NWADGetStatus(a, AUDIT_ID_IS_CONTAINER, b, c, d)
N_EXTERN_LIBRARY( NWRCODE )
NWADGetStatus
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   pNWAuditStatus auditStatus,
   nuint16        bufferSize
);

#define NWGetAuditingFlags(a, b, c, d) \
   NWADGetFlags(a, AUDIT_ID_IS_VOLUME, b, c, d)
#define NWDSGetAuditingFlags(a, b, c, d) \
   NWADGetFlags(a, AUDIT_ID_IS_CONTAINER, b, c, d)
N_EXTERN_LIBRARY( NWRCODE )
NWADGetFlags
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle,
   pnuint8        flags
);

#define NWLoginAsVolumeAuditor(a, b, c, d) \
      NWADLogin(a, AUDIT_ID_IS_VOLUME, b, c, d)
#define NWDSLoginAsContainerAuditor(a, b, c, d) \
      NWADLogin(a, AUDIT_ID_IS_CONTAINER, b, c, d)
N_EXTERN_LIBRARY( NWRCODE )
NWADLogin
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle,
   pnuint8        password
);

#define NWInitAuditLevelTwoPassword NWADInitLevelTwoPassword
N_EXTERN_LIBRARY( NWRCODE )
NWADInitLevelTwoPassword
(
   nptr     auditHandle,
   pnuint8  password
);

#define NWLogoutAsVolumeAuditor(a, b, c) \
   NWADLogout(a, AUDIT_ID_IS_VOLUME, b, c)
#define NWDSLogoutAsContainerAuditor(a, b, c) \
   NWADLogout(a, AUDIT_ID_IS_CONTAINER, b, c)
N_EXTERN_LIBRARY( NWRCODE )
NWADLogout
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle
);

#define NWChangeAuditorPassword(a, b, c, d, e) \
      NWADChangePassword(a, AUDIT_ID_IS_VOLUME, b, c, d, e)
#define NWDSChangeAuditorPassword(a, b, c, d, e) \
      NWADChangePassword(a, AUDIT_ID_IS_CONTAINER, b, c, d, e)
N_EXTERN_LIBRARY( NWRCODE )
NWADChangePassword
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle,
   pnuint8        newPassword,
   nuint8         level
);

#define NWCheckAuditAccess(a, b) \
   NWADCheckAccess(a, AUDIT_ID_IS_VOLUME, b)
#define NWDSCheckAuditAccess(a, b) \
   NWADCheckAccess(a, AUDIT_ID_IS_CONTAINER, b)
N_EXTERN_LIBRARY( NWRCODE )
NWADCheckAccess
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID
);

#define NWCheckAuditLevelTwoAccess(a, b, c) \
   NWADCheckLevelTwoAccess(a, AUDIT_ID_IS_VOLUME, b, c)
#define NWDSCheckAuditLevelTwoAccess(a, b, c) \
   NWADCheckLevelTwoAccess(a, AUDIT_ID_IS_CONTAINER, b, c)
N_EXTERN_LIBRARY( NWRCODE )
NWADCheckLevelTwoAccess
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle
);

#define NWEnableAuditingOnVolume(a, b, c) \
   NWADEnable(a, AUDIT_ID_IS_VOLUME, b, c)
#define NWDSEnableAuditingOnContainer(a, b, c) \
   NWADEnable(a, AUDIT_ID_IS_CONTAINER, b, c)
N_EXTERN_LIBRARY( NWRCODE )
NWADEnable
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle
);

#define NWDisableAuditingOnVolume(a, b, c) \
   NWADDisable(a, AUDIT_ID_IS_VOLUME, b, c)
#define NWDSDisableAuditingOnContainer(a, b, c) \
   NWADDisable(a, AUDIT_ID_IS_CONTAINER, b, c)
N_EXTERN_LIBRARY( NWRCODE )
NWADDisable
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle
);

#define NWIsUserBeingAudited(a, b, c, d) \
   NWADIsObjectAudited(a, AUDIT_ID_IS_VOLUME, b, d)
#define NWDSIsObjectBeingAudited(a, b, c) \
   NWADIsObjectAudited(a, AUDIT_ID_IS_CONTAINER, b, c)
N_EXTERN_LIBRARY( NWRCODE )
NWADIsObjectAudited
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nuint32        userObjectID
);

#define NWAddAuditProperty(a, b, c, d) \
   NWADChangeObjectProperty(a, AUDIT_ID_IS_VOLUME, b, c, d, 1)
#define NWRemoveAuditProperty(a, b, c, d) \
   NWADChangeObjectProperty(a, AUDIT_ID_IS_VOLUME, b, c, d, 0)
#define NWDSChangeObjectAuditProperty(a, b, c, d, e) \
   NWADChangeObjectProperty(a, AUDIT_ID_IS_CONTAINER, b, c, d, e)
N_EXTERN_LIBRARY( NWRCODE )
NWADChangeObjectProperty
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle,
   nuint32        objectID,
   nuint8         auditFlag  /* 0:remove 1:add */
);

/* Volume audit call only */
#define NWReadAuditingBitMap(a, b, c, d, e)\
   NWADReadBitMap(a, b, d, e)
N_EXTERN_LIBRARY( NWRCODE )
NWADReadBitMap
(
   NWCONN_HANDLE  conn,
   nuint32        auditID,  /* can only be volume */
   NWAuditBitMap  N_FAR *buffer,
   nuint16        bufferSize
);

#define NWReadAuditConfigHeader(a, b, c, d, e) \
   NWADReadConfigHeader(a, AUDIT_ID_IS_VOLUME, b, c, d, e)
#define NWDSReadAuditConfigHeader(a, b, c, d, e) \
   NWADReadConfigHeader(a, AUDIT_ID_IS_CONTAINER, b, c, d, e)
N_EXTERN_LIBRARY( NWRCODE )
NWADReadConfigHeader
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle,
   nptr           buffer,
   nuint16        bufferSize
);

/* volume auditing only */
#define NWWriteAuditingBitMap(a, b, c, d) \
   NWADWriteBitMap(a, b, c, d)
N_EXTERN_LIBRARY( NWRCODE )
NWADWriteBitMap
(
   NWCONN_HANDLE  conn,
   nuint32        auditID,  /* can only be volume */
   nptr           auditHandle,
   NWAuditBitMap  N_FAR *buffer
);

#define NWWriteAuditConfigHeader(a, b, c, d) \
   NWADWriteConfigHeader(a, AUDIT_ID_IS_VOLUME, b, c, d)
#define NWDSWriteAuditConfigHeader(a, b, c, d) \
   NWADWriteConfigHeader(a, AUDIT_ID_IS_CONTAINER, b, c, d)
N_EXTERN_LIBRARY( NWRCODE )
NWADWriteConfigHeader
(
   NWCONN_HANDLE   conn,
   nuint32         auditIDType,
   nuint32         auditID,
   nptr            auditHandle,
   pNWConfigHeader buffer
);

#define NWResetAuditingFile(a, b, c) \
   NWADResetFile(a, AUDIT_ID_IS_VOLUME, b, c)
#define NWDSResetAuditingFile(a, b, c) \
   NWADResetFile(a, AUDIT_ID_IS_CONTAINER, b, c)
N_EXTERN_LIBRARY( NWRCODE )
NWADResetFile
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle
);

/* NWADOpenReadFile will only work with NetWare version 4.10 or newer */
N_EXTERN_LIBRARY( NWRCODE )
NWADOpenRecordFile
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle,
   nint16         fileCode,
   pnptr          recordHandle  /* Allocate record handle */
);

/* NWADInitRead will only work with NetWare version 4.10 or newer */
N_EXTERN_LIBRARY( NWRCODE )
NWADReadRecord
(
   nptr        recordHandle,   /* Allocated in NWADOpenRecordFile */
   nuint16     maxSize,
   nint16      direction,
   pnuint8     buffer,
   pnuint16    bufferSize,
   pnuint8     eofFlag,
   pnuint32    offsetPtr
);

/* NWADInitRead will only work with NetWare version 4.10 or newer */
N_EXTERN_LIBRARY( NWRCODE )
NWADCloseRecordFile
(
   pnptr       recordHandle  /* Free record handle */
);

#define NWCloseOldAuditingFile(a, b, c) \
   NWADCloseOldFile(a, AUDIT_ID_IS_VOLUME, b, c)
#define NWDSCloseOldAuditingFile(a, b, c) \
   NWADCloseOldFile(a, AUDIT_ID_IS_CONTAINER, b, c)
N_EXTERN_LIBRARY( NWRCODE )
NWADCloseOldFile
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle
);

#define NWDeleteOldAuditingFile(a, b, c) \
   NWADDeleteFile(a, AUDIT_ID_IS_VOLUME, b, c)
#define NWDSDeleteOldAuditingFile(a, b, c) \
   NWADDeleteFile(a, AUDIT_ID_IS_CONTAINER, b, c)
N_EXTERN_LIBRARY( NWRCODE )
NWADDeleteFile
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle
);

/* NetWare version 4.10 or newer */
N_EXTERN_LIBRARY( NWRCODE )
NWADGetFileList
(
   NWCONN_HANDLE     conn,
   nuint32           auditIDType,
   nuint32           auditID,
   nptr              auditHandle,
   pNWAuditFileList  fileList
);

N_EXTERN_LIBRARY( NWRCODE )
NWADDeleteOldFile
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle,
   nuint32        fileCode
);

/* Trusted NetWare and Volume Only */
N_EXTERN_LIBRARY( NWRCODE )
NWADRestartVolumeAuditing
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID
);

/* Trusted NetWare Only */
N_EXTERN_LIBRARY( NWRCODE )
NWADSetPassword
(
   NWCONN_HANDLE  conn,
   nuint32        auditIDType,
   nuint32        auditID,
   nptr           auditHandle,
   pnuint8        newPassword
);

/* Trusted NetWare Only */
N_EXTERN_LIBRARY( NWRCODE )
NWADAppendExternalRecords
(
   NWCONN_HANDLE  conn,
   nuint32        auditFileObjectID,
   nuint32        vendorID,
   nuint32        numberRecords,
   pNWAuditRecord recordsPtr
);

#ifdef __cplusplus
   }
#endif

#include "npackoff.h"
#endif   /* NWAUDIT_H */
