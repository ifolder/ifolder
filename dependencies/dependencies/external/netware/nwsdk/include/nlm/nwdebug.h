#ifndef _NWDEBUG_H_
#define _NWDEBUG_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwdebug.h
==============================================================================
*/
#include <nwtypes.h>

/* library-debug flags */
#define CLIB_CONTEXT_CHECK    0x002 /* CLib Context */
#define CLIB_MEMCHECK         0x004 /* Memory Overwrites */
#define CLIB_RESOURCECHECK    0x020 /* Resource Check */
#define CLIB_THREAD_CHECK     0x200 /* Thread Check */
#define CLIB_SEMCHECK         0x080 /* Semaphore Checking */
#define CLIB_RING_BELL        0x040

#ifdef DEBUG
# define BumpFunctionCount(n)    if (NWValidateDebugProfile()) \
                                    NWBumpFunctionCount(n)
#else
# define BumpFunctionCount(n)
#endif

/* dynamic setting and clearing of breakpoints */
#define EXECUTION_BREAKPOINT     0
#define WRITE_BREAKPOINT         1
#define READ_WRITE_BREAKPOINT    3

#ifdef __cplusplus
extern "C"
{
#endif

/* prototypes... */
void  NWClearBreakpoint( int breakpoint );
int   NWSetBreakpoint( LONG address, int breakType);

int   NWDebugPrintf( const char *format, ... );
int   NWValidateDebugProfile( void );
void  NWBumpFunctionCount( const char *name );
void  NWDisplayBinaryAtAddr( const void *addr );
void  NWDisplayDoubleAtAddr( const void *addr );
void  NWDisplayLConvAtAddr( const void *lc );
void  NWDisplayStringAtAddr( const char *s, size_t len );
void  NWDisplayTMAtAddr( const void *t );
void  NWDisplayUnicodeAtAddr( const void *s, size_t len );
void  NWEnableDebugProfile( int flag );
void  EnterDebugger( void );

/* additional debugging prototypes... */
LONG  GetDebugSettings( void );
void  SetDebugSettings( LONG Settings );
LONG  GetNLMIDFromNLMName( const BYTE *NLMName );
BYTE  *GetDebugErrorMessage( void );
LONG  GetMemoryUsage( LONG NLMID );

#ifdef __cplusplus
}
#endif

#endif
