#ifndef __libcclib_h__
#define __libcclib_h__
/*============================================================================
=  Novell Software Development Kit
=
=  Copyright (C) 2001-2002 Novell, Inc. All Rights Reserved.
=
=  This work is subject to U.S. and international copyright laws and treaties.
=  Use and redistribution of this work is subject  to  the  license  agreement
=  accompanying  the  software  development kit (SDK) that contains this work.
=  However, no part of this work may be revised and/or  modified  without  the
=  prior  written consent of Novell, Inc. Any use or exploitation of this work
=  without authorization could subject the perpetrator to criminal  and  civil
=  liability. 
=
=  Source(s): Novell Header
=
=  Interfaces to private CLib context agent, libcclib.nlm.
=
=  libcclib.h
==============================================================================
*/
#ifndef _SIZE_T
# define _SIZE_T
typedef unsigned int size_t;
#endif

typedef struct
{
   int  (*ThreadGroupGetID)  ( void );

   int  (*ThreadGroupCreate) ( const char *name, int *threadGroupID );
   int  (*ThreadGroupDispose)( int threadGroupID );
   int  (*ThreadGroupUnwrap) ( int threadGroupID, int restoredThreadGroupID );
   int  (*ThreadGroupWrap)   ( int threadGroupID );

   int  (*ThreadCreate)      ( int threadGroupID,
                               void (*start_routine)( void *arg ), void *arg,
                               size_t stackSize, unsigned long flags,
                               int *threadID );
   void (*__UnloadBroker)    ( void );
   void *reserved1;
   void *reserved[8];
} clibctx_t;


/* prototypes... */
int   CLibLoadContextBroker  ( void *module, const char *callback );
int   CLibUnloadContextBroker( clibctx_t *broker );

/* prototype for imitation... */
int   MyCallBack( clibctx_t *broker );


#endif
