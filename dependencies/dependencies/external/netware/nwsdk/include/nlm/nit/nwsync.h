/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwsync.h
==============================================================================
*/

#ifndef _NWSYNC_H_
#define _NWSYNC_H_

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>


#define _MAX_LOGREC_NAME 128
#define _MAX_SEMAPHORE_NAME 128


#ifdef __cplusplus
extern "C" {
#endif

extern int ClearFile
(
   const char *fileName
);

extern void ClearFileSet
(
   void
);

extern int ClearLogicalRecord
(
   const char *logicalRecordName
);

extern void ClearLogicalRecordSet
(
   void
);

extern int ClearPhysicalRecord
(
   int  fileHandle,
   long recordStartOffset,
   long recordLength
);

extern void ClearPhysicalRecordSet
(
   void
);

extern int CloseSemaphore
(
   long semaphoreHandle
);

extern int ExamineSemaphore
(
   long  semaphoreHandle,
   int  *semaphoreValue,
   WORD *openCount
);

extern int LockFileSet
(
   WORD timeoutLimit
);

extern int LockLogicalRecordSet
(
   WORD timeoutLimit
);

extern int LockPhysicalRecordSet
(
   BYTE lockDirective,
   WORD timeoutLimit
);

extern int LogFile
(
   const char *fileName,
   BYTE        lockDirective,
   WORD        timeoutLimit
);

extern int LogLogicalRecord
(
   const char *logicalRecordName,
   BYTE        lockDirective,
   WORD        timeoutLimit
);

extern int LogPhysicalRecord
(
   int  fileHandle,
   long recordStartOffset,
   long recordLength,
   BYTE lockDirective,
   WORD timeoutLimit
);

extern int OpenSemaphore
(
   const char *semaphoreName,
   int         initialValue,
   long       *semaphoreHandle,
   WORD       *openCount
);

extern int ReleaseFile
(
   const char *fileName
);

extern void ReleaseFileSet
(
   void
);

extern int ReleaseLogicalRecord
(
   const char *logicalRecordName
);

extern void ReleaseLogicalRecordSet
(
   void
);

extern int ReleasePhysicalRecord
(
   int  fileHandle,
   long recordStartOffset,
   long recordLength
);

extern void ReleasePhysicalRecordSet
(
   void
);

extern int SignalSemaphore
(
   long semaphoreHandle
);

extern int WaitOnSemaphore
(
   long semaphoreHandle,
   WORD timeoutLimit
);

#ifdef __cplusplus
}
#endif


#endif
