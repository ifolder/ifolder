#ifndef _NWSEMAPH_H_
#define _NWSEMAPH_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwsemaph.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

#define SEMAPHORE_TRACKING_ON    1
#define SEMAPHORE_TRACKING_OFF   0


#ifdef __cplusplus
extern "C"
{
#endif

extern int CloseLocalSemaphore
(
   LONG semaphoreHandle
);

extern int ExamineLocalSemaphore
(
   LONG semaphoreHandle
);

extern LONG OpenLocalSemaphore
(
   long initialValue
);

extern int SignalLocalSemaphore
(
   LONG semaphoreHandle
);

extern int TimedWaitOnLocalSemaphore
(
   LONG semaphoreHandle,
   LONG timeOut
);

extern int WaitOnLocalSemaphore
(
   LONG semaphoreHandle
);

extern int SetSemaphoreTrackingFlag
( 
   int state 
);

#ifdef __cplusplus
}
#endif


#endif
