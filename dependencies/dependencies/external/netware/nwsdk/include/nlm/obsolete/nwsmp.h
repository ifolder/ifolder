#ifndef _NWSMP_H_
#define _NWSMP_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1996-1998 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwsmp.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>

typedef nuint32   mutex_t;
typedef nuint32   rmutex_t;
typedef nuint32   cond_t;
typedef nuint32   spin_t;
typedef nuint32   rwlock_t;
typedef nuint32   barrier_t;
typedef nuint32   thread_desc_t;

#ifdef __cplusplus
extern "C"
{
#endif

nuint32  NWSMPIsAvailable( void );
nint32   NWSMPIsLoaded ( void );

/* Thread Migration */
nuint32  NWNumberOfRegisteredProcessors( void );
void     NWSMPThreadToMP( void );
void     NWSMPThreadToNetWare( void );

/* (for backward compatibility) */
void     NWThreadToMP( void );
void     NWThreadToNetWare( void );

/* Mutex Locks */
mutex_t  NWSMPMutexSleepAlloc( const nstr *name );
nuint32  NWSMPMutexDestroy( mutex_t mutex ); 
nuint32  NWSMPMutexTryLock( mutex_t mutex ); 
nuint32  NWSMPMutexLock( mutex_t mutex );
nuint32  NWSMPMutexUnlock( mutex_t mutex );

/* Reader Writer Locks */
nuint32  NWSMPRWReadLock( rwlock_t rwlock ); 
nuint32  NWSMPRWTryReadLock( rwlock_t rwlock ); 
nuint32  NWSMPRWTryWriteLock( rwlock_t rwlock ); 
nuint32  NWSMPRWUnlock( rwlock_t rwlock ); 
nuint32  NWSMPRWWriteLock( rwlock_t rwlock );
rwlock_t *NWSMPRWLockAlloc( const nstr *name );
nuint32  NWSMPRWLockDestroy( rwlock_t rwlock );

/* Recursive Mutex Locks */ 
rmutex_t NWSMPRMutexAlloc( const nstr *name ); 
nuint32  NWSMPRMutexDestroy( rmutex_t rmutex ); 
nuint32  NWSMPRMutexLock( rmutex_t rmutex ); 
thread_desc_t  NWSMPRMutexOwner( rmutex_t rmutex ); 
nuint32  NWSMPRMutexTryLock( rmutex_t rmutex ); 
nuint32  NWSMPRMutexUnlock( rmutex_t rmutex ); 

/* Condition Locks */ 
cond_t   NWSMPCondAlloc( const nstr *name ); 
nuint32  NWSMPCondBroadcast( cond_t c ); 
nuint32  NWSMPCondDestroy( cond_t cond ); 
nuint32  NWSMPCondWait( cond_t c, mutex_t mutex ); 
nuint32  NWSMPCondSignal( cond_t c );

/* Spin Locks */ 
nuint32  NWSMPSpinAlloc( const nstr *name ); 
nuint32  NWSMPSpinDestroy( nuint32 lock ); 
nint32   NWSMPSpinLock( nuint32 timer ); 
nint32   NWSMPSpinTryLock( nuint32 timer ); 
void     NWSMPSpinUnlock( nuint32 timer ); 

/* Barrier Locks */
barrier_t   NWSMPBarrierAlloc( nuint32 count, const nstr *name ); 
nuint32  NWSMPBarrierDestroy( barrier_t barrier ); 
nuint32  NWSMPBarrierWait( barrier_t barrier ); 

#ifdef __cplusplus
}
#endif

#endif
