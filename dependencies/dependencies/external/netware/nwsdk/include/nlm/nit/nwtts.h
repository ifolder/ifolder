/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwtts.h
==============================================================================
*/

#ifndef _NWTTS_H_
#define _NWTTS_H_

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>


#ifdef __cplusplus
extern "C" {
#endif

extern int TTSAbortTransaction
(
   void
);

extern int TTSBeginTransaction
(
   void
);

extern int TTSEndTransaction
(
   long *transactionNumber
);

extern int TTSGetApplicationThresholds
(
   BYTE *logicalRecordLockThreshold,
   BYTE *physicalRecordLockThreshold
);

extern int TTSGetWorkstationThresholds
(
   BYTE *logicalRecordLockThreshold,
   BYTE *physicalRecordLockThreshold
);

extern int TTSIsAvailable
(
   void
);

extern int TTSSetApplicationThresholds
(
   BYTE logicalRecordLockThreshold,
   BYTE physicalRecordLockThreshold
);

extern int TTSSetWorkstationThresholds
(
   BYTE logicalRecordLockThreshold,
   BYTE physicalRecordLockThreshold
);

extern int TTSTransactionStatus
(
   long transactionNumber
);

#ifdef __cplusplus
}
#endif


#endif
