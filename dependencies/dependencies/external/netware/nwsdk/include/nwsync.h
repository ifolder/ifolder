/******************************************************************************

  %name: nwsync.h %
  %version: 5 %
  %date_modified: Tue Oct 19 15:48:50 1999 %
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

#if ! defined ( NWSYNC_H )
#define NWSYNC_H

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct
{
  nuint16 connNumber;
  nuint16 taskNumber;
  nuint8  lockStatus;
} LOGICAL_LOCK;

typedef struct
{
  nuint16 useCount;
  nuint16 shareableLockCount;
  nuint8  locked;
  nuint16 nextRequest;
  nuint16 numRecords;
  LOGICAL_LOCK logicalLock[128];
  nuint16 curRecord;
} LOGICAL_LOCKS;

typedef struct
{
  nuint16 taskNumber;
  nuint8  lockStatus;
  nstr8   logicalName[128];
} CONN_LOGICAL_LOCK;

typedef struct
{
  nuint16 nextRequest;
  nuint16 numRecords;
  nuint8  records[508];
  nuint16 curOffset;
  nuint16 curRecord;
} CONN_LOGICAL_LOCKS;

typedef struct
{
  nuint16 loggedCount;
  nuint16 shareableLockCount;
  nuint32 recordStart;
  nuint32 recordEnd;
  nuint16 connNumber;
  nuint16 taskNumber;
  nuint8  lockType;
} PHYSICAL_LOCK;

typedef struct
{
  nuint16 nextRequest;
  nuint16 numRecords;
  PHYSICAL_LOCK locks[32];
  nuint16 curRecord;
  nuint8  reserved[8];
} PHYSICAL_LOCKS;

typedef struct
{
  nuint16 taskNumber;
  nuint8  lockType;
  nuint32 recordStart;
  nuint32 recordEnd;
} CONN_PHYSICAL_LOCK;

typedef struct
{
  nuint16 nextRequest;
  nuint16 numRecords;
  CONN_PHYSICAL_LOCK locks[51];
  nuint16 curRecord;
  nuint8  reserved[22];
} CONN_PHYSICAL_LOCKS;

typedef struct
{
  nuint16 connNumber;
  nuint16 taskNumber;
} SEMAPHORE;

typedef struct
{
  nuint16 nextRequest;
  nuint16 openCount;
  nuint16 semaphoreValue;
  nuint16 semaphoreCount;
  SEMAPHORE semaphores[170];
  nuint16 curRecord;
} SEMAPHORES;

typedef struct
{
  nuint16 openCount;
  nuint16 semaphoreValue;
  nuint16 taskNumber;
  nstr8   semaphoreName[128];
} CONN_SEMAPHORE;

typedef struct
{
  nuint16 nextRequest;
  nuint16 numRecords;
  nuint8  records[508];
  nuint16 curOffset;
  nuint16 curRecord;
} CONN_SEMAPHORES;


N_EXTERN_LIBRARY( NWCCODE )
NWScanPhysicalLocksByFile
(
   NWCONN_HANDLE          conn,
   NWDIR_HANDLE           dirHandle,
   const nstr8 N_FAR    * path,
   nuint8                 dataStream,
   pnint16                iterHandle,
   PHYSICAL_LOCK  N_FAR * lock,
   PHYSICAL_LOCKS N_FAR * locks
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanLogicalLocksByConn
(
   NWCONN_HANDLE              conn,
   nuint16                    connNum,
   pnint16                    iterHandle,
   CONN_LOGICAL_LOCK  N_FAR * logicalLock,
   CONN_LOGICAL_LOCKS N_FAR * logicalLocks
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanPhysicalLocksByConnFile
(
   NWCONN_HANDLE               conn,
   nuint16                     connNum,
   NWDIR_HANDLE                dirHandle,
   const nstr8         N_FAR * path,
   nuint8                      dataStream,
   pnint16                     iterHandle,
   CONN_PHYSICAL_LOCK  N_FAR * lock,
   CONN_PHYSICAL_LOCKS N_FAR * locks
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanLogicalLocksByName
(
   NWCONN_HANDLE         conn,
   const nstr8   N_FAR * logicalName,
   pnint16               iterHandle,
   LOGICAL_LOCK  N_FAR * logicalLock,
   LOGICAL_LOCKS N_FAR * logicalLocks
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanSemaphoresByConn
(
   NWCONN_HANDLE           conn,
   nuint16                 connNum,
   pnint16                 iterHandle,
   CONN_SEMAPHORE  N_FAR * semaphore,
   CONN_SEMAPHORES N_FAR * semaphores
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanSemaphoresByName
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * semName,
   pnint16             iterHandle,
   SEMAPHORE   N_FAR * semaphore,
   SEMAPHORES  N_FAR * semaphores
);

N_EXTERN_LIBRARY( NWCCODE )
NWSignalSemaphore
(
   NWCONN_HANDLE  conn,
   nuint32        semHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWCloseSemaphore
(
   NWCONN_HANDLE  conn,
   nuint32        semHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWOpenSemaphore
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * semName,
   nint16              initSemHandle,
   pnuint32            semHandle,
   pnuint16            semOpenCount
);

N_EXTERN_LIBRARY( NWCCODE )
NWExamineSemaphore
(
   NWCONN_HANDLE  conn,
   nuint32        semHandle,
   pnint16        semValue,
   pnuint16       semOpenCount
);

N_EXTERN_LIBRARY( NWCCODE )
NWWaitOnSemaphore
(
   NWCONN_HANDLE  conn,
   nuint32        semHandle,
   nuint16        timeOutValue
);

#ifdef __cplusplus
}
#endif

#include "npackoff.h"
#endif
