#ifndef _NWLIB_H_
#define _NWLIB_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwlib.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>
                                                                          

#ifdef __cplusplus
extern "C" {
#endif

extern int *__get_thread_data_area_ptr
(
   void
);

extern void *GetDataAreaPtr
(
   LONG libraryHandle
);

extern int DeregisterLibrary
(
   LONG libraryHandle
);

extern LONG RegisterLibrary
(
   int (*cleanupFunc)( void *dataAreaPtr )
);

extern int SaveDataAreaPtr
(
   LONG  libraryHandle, 
   void *dataAreaPtr
);

#ifdef __cplusplus
}
#endif


#define Thread_Data_Area (*__get_thread_data_area_ptr())


#endif
