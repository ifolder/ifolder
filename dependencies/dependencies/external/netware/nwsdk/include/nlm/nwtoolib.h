#ifndef _NWTOOLIB_H_
#define _NWTOOLIB_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwtoolib.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>


#ifdef __cplusplus
extern "C"
{
#endif

extern int _NWDeregisterNLMLibrary
(
   int NLMHandle
);

extern int _NWGetErrno
(
   void
);

extern int _NWGetNWErrno(void);

extern void *_NWGetNLMLevelLibDataPtr
(
   int NLMID
);

extern void *_NWGetThreadGroupLevelLibDataPtr
(
   int threadGroupID
);

extern void *_NWGetThreadLevelLibDataPtr
(
   int threadID
);

extern int _NWLoadNLMMessageTable
(
   int     NLMHandle,
   char ***messageTable,
   int    *messageCount,
   int    *languageID
);

extern int _NWRegisterNLMLibrary
(
   int  NLMHandle,
   int  NLMFileHandle,
   int  (*readRoutine)(),
   void (*NLMBegin)( int NLMID, char *commandLine ),
   void (*NLMPreEnd)( int NLMID ),
   void (*NLMPostEnd)( int NLMID ),
   void (*NLMEndNoContext)( int NLMID ),
   int  (*threadGroupBegin)( int threadGroupID, int argc, char *argv[] ),
   void (*threadGroupEnd)( int threadGroupID ),
   int  (*threadBegin)( int threadID ),
   void (*threadEnd)( int threadID ),
   void (*threadReleaseFileResources)( int threadID )
);

extern void _NWSetErrno
(
   int errnoValue
);

extern void _NWSetNWErrno(int NWErrnoValue);

extern void _NWSetNLMLevelLibDataPtr
(
   int   NLMID,
   void *dataPtr
);

extern void _NWSetThreadGroupLevelLibDataPtr
(
   int   threadGroupID,
   void *dataPtr
);

extern void _NWSetThreadLevelLibDataPtr
(
   int   threadID,
   void *dataPtr
);

#ifdef __cplusplus
}
#endif


#endif
