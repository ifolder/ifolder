/******************************************************************************

  %name: nwtts.h %
  %version: 3 %
  %date_modified: Wed Dec 18 12:07:23 1996 %
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

#if ! defined ( NWTTS_H )
#define NWTTS_H

#if ! defined ( NWCALDEF_H )
# include "nwcaldef.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct
{
  nuint32 systemElapsedTime;
  nuint8  TTS_Supported;
  nuint8  TTS_Enabled;
  nuint16 TTS_VolumeNumber;
  nuint16 TTS_MaxOpenTransactions;
  nuint16 TTS_MaxTransactionsOpened;
  nuint16 TTS_CurrTransactionsOpen;
  nuint32 TTS_TotalTransactions;
  nuint32 TTS_TotalWrites;
  nuint32 TTS_TotalBackouts;
  nuint16 TTS_UnfilledBackouts;
  nuint16 TTS_DiskBlocksInUse;
  nuint32 TTS_FATAllocations;
  nuint32 TTS_FileSizeChanges;
  nuint32 TTS_FilesTruncated;
  nuint8  numberOfTransactions;
  struct
  {
    nuint8 connNumber;
    nuint8 taskNumber;
  } connTask[235];
} TTS_STATS;

N_EXTERN_LIBRARY( NWCCODE )
NWTTSAbortTransaction
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWTTSBeginTransaction
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWTTSIsAvailable
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWTTSGetControlFlags
(
   NWCONN_HANDLE  conn,
   pnuint8        controlFlags
);

N_EXTERN_LIBRARY( NWCCODE )
NWTTSSetControlFlags
(
   NWCONN_HANDLE  conn,
   nuint8         controlFlags
);

N_EXTERN_LIBRARY( NWCCODE )
NWTTSEndTransaction
(
   NWCONN_HANDLE  conn,
   pnuint32       transactionNum
);

N_EXTERN_LIBRARY( NWCCODE )
NWTTSTransactionStatus
(
   NWCONN_HANDLE  conn,
   nuint32        transactionNum
);

N_EXTERN_LIBRARY( NWCCODE )
NWTTSGetProcessThresholds
(
   NWCONN_HANDLE  conn,
   pnuint8        logicalLockLevel,
   pnuint8        physicalLockLevel
);

N_EXTERN_LIBRARY( NWCCODE )
NWTTSSetProcessThresholds
(
   NWCONN_HANDLE  conn,
   nuint8         logicalLockLevel,
   nuint8         physicalLockLevel
);

N_EXTERN_LIBRARY( NWCCODE )
NWTTSGetConnectionThresholds
(
   NWCONN_HANDLE  conn,
   pnuint8        logicalLockLevel,
   pnuint8        physicalLockLevel
);

N_EXTERN_LIBRARY( NWCCODE )
NWTTSSetConnectionThresholds
(
   NWCONN_HANDLE  conn,
   nuint8         logicalLockLevel,
   nuint8         physicalLockLevel
);

N_EXTERN_LIBRARY( NWCCODE )
NWEnableTTS
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWDisableTTS
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetTTSStats
(
   NWCONN_HANDLE  conn,
   TTS_STATS N_FAR * ttsStats
);

#ifdef __cplusplus
}
#endif

#include "npackoff.h"
#endif
