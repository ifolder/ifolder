#ifndef _NWCNTASK_H_
#define _NWCNTASK_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwcntask.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

#define LOGIN_WITHOUT_PASSWORD ((char *) N_TRUE)


#ifdef __cplusplus
extern "C"
{
#endif

extern LONG AllocateBlockOfTasks
(
   LONG numberWanted
);

extern BYTE CheckIfConnectionActive
(
   LONG connection
);

extern int DisableConnection
(
   LONG connection
);

extern int EnableConnection
(
   LONG connection
);

extern LONG GetCurrentConnection
(
   void
);

extern WORD GetCurrentFileServerID
(
   void
);

extern LONG GetCurrentTask
(
   void
);

extern int LoginObject
(
   LONG  connection,
   const char *objectName,
   WORD  objectType,
   const char *password
);

extern int LogoutObject
(
   LONG connection
);

extern int ReturnBlockOfTasks
(
   LONG startingTask,
   LONG numberOfTasks
);

extern int ReturnConnection
(
   LONG connection
);

extern int ReturnLocalConnection
(
   LONG connection
);

extern LONG SetCurrentConnection
(
   LONG connectionNumber
);

extern WORD SetCurrentFileServerID
(
   WORD connectionID
);

extern LONG SetCurrentTask
(
   LONG taskNumber
);

#ifdef __cplusplus
}
#endif


#endif
