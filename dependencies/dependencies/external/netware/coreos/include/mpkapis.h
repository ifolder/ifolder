COMMENT();	// LEAVE THIS ALONE.
/*(			// LEAVE THIS ALONE.

IF	0		; LEAVE THIS ALONE.
; *****************************************************************************
; *
; *	(C) Copyright 1988-1997 Novell, Inc.
; *	All Rights Reserved.
; *
; *	This program is an unpublished copyrighted work which is proprietary
; *	to Novell, Inc. and contains confidential information that is not
; *	to be reproduced or disclosed to any other person or entity without
; *	prior written consent from Novell, Inc. in each and every instance.
; *
; *	WARNING:  Unauthorized reproduction of this program as well as
; *	unauthorized preparation of derivative works based upon the
; *	program or distribution of copies by sale, rental, lease or
; *	lending are violations of federal copyright laws and state trade
; *	secret laws, punishable by civil and criminal penalties.
; *
; *	INCLUDE FILE THAT CAN BE INCLUDED IN BOTH C AND ASSEMBLY SOURCE FILES.
; *
; * $Workfile:   mpkapis.h  $
; * $Revision$
; * $Modtime : $
; * $Author  : $
; ****************************************************************************
; */		// LEAVE THIS ALONE.

//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//
// This include file contains information that is intended to be made
// public.  All the information contained here will be exposed
// third party developers.
//
// Any information that is meant for Novell Internal Use Only should
// NOT be present here.
//
// The APIs in this include file may also be made available in user mode.
//
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


//
// BEGIN C INCLUDE PORTION
//

#ifndef _MPKAPIS_H
#define _MPKAPIS_H


#include	<mpktypes.h>

//
// Thread attributes.
//

//
// Use the next one with care.  It does not
// have any bits set, unlike the other thread attributes.
//

#define	THREAD_ATTRIB_DETACHED		0x00000000
#define	THREAD_ATTRIB_JOINABLE		0x00000001

//
// Maximum name length for object names - Not including NULL byte
//

#define MAX_NAME_LENGTH		(63)

THREAD	kStartThread(BYTE *name, void *(*StartAddress)(THREAD, void *),
			void *StackAddressHigh, LONG StackSize, void *Argument);
THREAD	kCreateThread(BYTE *name, void *(*StartAddress)(THREAD, void *),
			void *StackAddressHigh, LONG StackSize, void *Argument);
ERROR	kScheduleThread(THREAD ThreadHandle);

void	kYieldThread(void);

//
// Put the calling thread to sleep for the specified
// time (in milliseconds).
//

ERROR	kDelayThread(UINT DelayAmount);

ERROR	kSuspendThread(THREAD ThreadHandle);
ERROR	kResumeThread(THREAD ThreadHandle);

ERROR	kGetThreadName(THREAD ThreadHandle, BYTE *buffer, LONG bufferLength);
ERROR	kSetThreadName(THREAD ThreadHandle, BYTE *buffer);

THREAD	kCurrentThread(void);

ERROR	kDestroyThread(THREAD ThreadHandle);

void	kExitThread(void *ExitStatus);
ERROR	kGetThreadExitCode(THREAD ThreadHandle, void **pExitStatusPtr);

ERROR	kThreadCheckForSuspendKill(void);

ERROR	kGetThreadAttributes(THREAD ThreadHandle, UINT32 *pAttribPtr);
ERROR	kSetThreadAttributes(THREAD ThreadHandle, UINT32 ThreadAttrib);

ERROR	kSetThreadPriority( THREAD ThreadHandle, LONG priority );
LONG	kGetThreadPriority( THREAD ThreadHandle );

ERROR	kGetThreadList(APPL app, THREAD *buffer,
					UINT32 numberOfSlots, UINT32 *numberOfSlotsFilled);

ERROR  	kSetThreadUserData(THREAD ThreadHandle, void *data);
void   	*kGetThreadUserData(THREAD ThreadHandle);


//
// APIs for entering and exiting the "Classic NetWare Binding."
//

void	kEnterNetWare(void);
void	kExitNetWare(void);

//
// Unlike kExitNetWare() which exits only the most recently
// entered CNB, this API exits any and all previously entered
// CNBs.
//

void	kExitClassicNetWare(void);


//
// App APIs
//

APPL	kCreateApplication( BYTE *name );
ERROR	kDestroyApplication( APPL application );
APPL	kGetApplicationHandle( BYTE *name );
ERROR	kSetApplicationShare( APPL application, LONG share );
LONG	kGetApplicationShare( APPL application );
ERROR	kGetApplicationName(APPL AppHandle, BYTE *BufferPtr, LONG BufferLength);
ERROR	kGetApplicationThreadCount(APPL AppHandle, LONG *AppThreadCountPtr);

ERROR	kGetApplicationList(APPL *buffer, UINT32 numberOfSlots,
								UINT32 *numberOfSlotsFilled);
ERROR	kGetNumberOfApplications(UINT *numberOfApplications);

//ERROR	kGetAppExecutionTime(APPL application, UINT64 *pExecTime);

ERROR	kSetThreadApplication(APPL AppHandle, THREAD ThreadHandle);
APPL	kGetThreadApplication(THREAD ThreadHandle);

//
// Reader Writer (Blocking version) Lock APIs.
//

//
// #defines used by kRWLockInfo.
//

#define	RWLOCK_STATUS_FREE			0x1111
#define	RWLOCK_STATUS_READ_LOCK		0x2222
#define	RWLOCK_STATUS_WRITE_LOCK	0x4444

RWLOCK	kRWLockAlloc(BYTE *RWLockName);
ERROR	kRWLockFree(RWLOCK RWLockHandle);
ERROR	kRWReadLock(RWLOCK RWLockHandle);
ERROR	kRWReadUnlock(RWLOCK RWLockHandle);
ERROR	kRWReadTryLock(RWLOCK RWLockHandle);
ERROR	kRWWriteLock(RWLOCK RWLockHandle);
ERROR	kRWWriteUnlock(RWLOCK RWLockHandle);
ERROR	kRWWriteTryLock(RWLOCK RWLockHandle);
ERROR	kRWWriterToReader(RWLOCK RWLockHandle);
ERROR	kRWReaderToWriter(RWLOCK RWLockHandle);
ERROR	kRWLockInfo(RWLOCK RWLockHandle, LONG *LockStatus,
							THREAD *OwnerThread);
//
// Mutex APIs.
//

MUTEX	kMutexAlloc(BYTE *MutexName);
ERROR	kMutexFree(MUTEX MutexHandle);
ERROR	kMutexLock(MUTEX MutexHandle);
ERROR	kMutexTryLock(MUTEX MutexHandle );
ERROR	kMutexTimedWait(MUTEX MutexHandle, UINT32 MilliSecondTimeOut);
ERROR	kMutexUnlock(MUTEX MutexHandle);
UINT	kMutexRecursiveCount(MUTEX MutexHandle);
UINT	kMutexWaitCount(MUTEX MutexHandle);

//
// Semaphore APIs.
//

//
// Maximum value that can be specified as the initial
// semaphore value.
//

#define		SEMAPHORE_VALUE_MAX		0x7fffffff

SEMAPHORE	kSemaphoreAlloc(BYTE *pSemaName, UINT SemaCount);
ERROR		kSemaphoreFree(SEMAPHORE SemaHandle);
ERROR		kSemaphoreWait(SEMAPHORE SemaHandle);
ERROR		kSemaphoreTimedWait(SEMAPHORE SemaHandle, UINT MilliSecondTimeOut);
ERROR		kSemaphoreSignal(SEMAPHORE SemaHandle);
ERROR		kSemaphoreTry(SEMAPHORE SemaHandle);
UINT		kSemaphoreExamineCount(SEMAPHORE SemaHandle);
UINT		kSemaphoreWaitCount(SEMAPHORE SemaHandle);

//
// Barrier APIs.
//

BARRIER	kBarrierAlloc(BYTE *pBarrierName, UINT BarrierValue);
ERROR	kBarrierFree(BARRIER BarrierHandle);
ERROR	kBarrierWait(BARRIER BarrierHandle);
ERROR	kBarrierIncrement(BARRIER BarrierHandle);
ERROR	kBarrierDecrement(BARRIER BarrierHandle);
ERROR	kBarrierThreadCount(BARRIER BarrierHandle, UINT *pThreadCount);
ERROR	kBarrierWaitCount(BARRIER BarrierHandle, UINT *pWaitingThreadCount);

//
// "User" Condition variables.  (Can be used either in Ring 0 or in Ring 3).
//

ERROR	kConditionAlloc(BYTE *pCVNamePtr, CONDITION *pCVHandlePtr);
ERROR	kConditionDestroy(CONDITION CVHandle);
ERROR	kConditionWait(CONDITION CVHandle, MUTEX MutexHandle);
ERROR	kConditionTimedWait(CONDITION CVHandle, MUTEX MutexHandle,
							LONG MilliSecTimeOutInterval);
ERROR	kConditionSignal(CONDITION CVHandle);
ERROR	kConditionBroadcast(CONDITION CVHandle);


//
// Atomic functions.
//

void	atomic_inc(LONG *AddressPtr);
void	atomic_dec(LONG *AddressPtr);
void	atomic_add(LONG *AddressPtr, LONG Value);
void	atomic_sub(LONG *AddressPtr, LONG Value);
LONG	atomic_bts(LONG *BitBase, LONG BitOffset);
LONG	atomic_btr(LONG *BitBase, LONG BitOffset);
LONG	atomic_xchg(LONG *address, LONG value);

//
//  Que functions.
//

typedef struct qlink
{
	struct qlink	*link;
	void				*ptr;
	SINT64			value;
} QLINK;



QUE		kAllocQue(void);
QUE		kAllocQueNoSleep(void);
ERROR	kFreeQue(QUE QueHandle);
LONG	kQueCount(QUE QueHandle);
ERROR	kEnQue(QUE QueHandle, QLINK *NewNode);
ERROR	kEnQueNoLock(QUE QueHandle, QLINK *pNewNode);
ERROR	kEnQueOrdered(QUE QueHandle, QLINK *NewNode, SINT64 value);


ERROR	kEnQueOrderedNoLock(QUE QueHandle, QLINK *pNewNode, SINT64 value);
ERROR	kPushQue(QUE QueHandle, QLINK *NewNode);
ERROR	kPushQueNoLock(QUE QueHandle, QLINK *pNewNode);
ERROR	kPushQueOrdered(QUE QueHandle, QLINK *NewNode, SINT64 value);
ERROR	kPushQueOrderedNoLock(QUE QueHandle, QLINK *pNewNode, SINT64 value );
QLINK	*kDeQue(QUE QueHandle);
QLINK	*kDeQueNoLock(QUE QueHandle );
QLINK	*kDeQueByQLINK(QUE QueHandle, QLINK *Node);
QLINK	*kDeQueByQLINKNoLock(QUE QueHandle, QLINK *pNewNode);
QLINK	*kDeQueWait(QUE QueHandle);
QLINK	*kDeQueWaitNoLock(QUE QueHandle);
QLINK	*kDeQueAll(QUE QueHandle);
QLINK	*kDeQueAllNoLock(QUE QueHandle);
QLINK	*kFirstQLINKNoLock(QUE QueHandle);

//
// QueLight functions.
//

typedef struct qlink_light
{
	struct qlink_light	*pNextNode;
} QLINK_LIGHT;


QUE_LIGHT	kAllocQueLight(void);
QUE_LIGHT	kAllocQueLightNoSleep(void);
ERROR		kFreeQueLight(QUE_LIGHT queue);
UINT		kQueLightCount(QUE_LIGHT queue);
ERROR		kEnQueLight(QUE_LIGHT queue, QLINK_LIGHT *item);
ERROR		kEnQueLightNoLock( QUE_LIGHT queue, QLINK_LIGHT *item );
ERROR		kPushQueLight(QUE_LIGHT queue, QLINK_LIGHT *item);
ERROR		kPushQueLightNoLock(QUE_LIGHT queue, QLINK_LIGHT *item);
QLINK_LIGHT	*kDeQueLight(QUE_LIGHT queue);
QLINK_LIGHT	*kDeQueLightNoLock(QUE_LIGHT queue);
QLINK_LIGHT	*kDeQueLightByQueLink(QUE_LIGHT queue, QLINK_LIGHT *item);
QLINK_LIGHT	*kDeQueLightByQueLinkNoLock(QUE_LIGHT queue, QLINK_LIGHT *item);
QLINK_LIGHT	*kDeQueLightWait(QUE_LIGHT queue);
QLINK_LIGHT	*kDeQueLightWaitNoLock(QUE_LIGHT queue);
QLINK_LIGHT	*kDeQueLightAll(QUE_LIGHT queue);
QLINK_LIGHT	*kDeQueLightAllNoLock(QUE_LIGHT queue);
QLINK_LIGHT	*kFirstQueLinkLightNoLock(QUE_LIGHT queue);

#endif		// _MPKAPIS_H

#if	0		// LEAVE THIS ALONE.
ELSE		; LEAVE THIS ALONE.

;IFNDEF _MPKAPIS_H ; MPKAPIS.H Assembly Portion
;_MPKAPIS_H = 1
;************************************************************************
; ***************  BEGIN ASSEMBLY INCLUDE PORTION.  *********************
;************************************************************************






;************************************************************************
;*****************  END ASSEMBLY INCLUDE PORTION.  **********************
;************************************************************************
;ENDIF ; _MPKAPIS_H  MPKAPIS.H Assembly Portion

;****************************************************************************
;*********************** END OF FILE ****************************************
;****************************************************************************

ENDIF	; LEAVE THIS ALONE.

COMMENT();	// LEAVE THIS ALONE
#endif
/* (		// LEAVE THIS ALONE
; END ; REMOVE THE FIRST SEMICOLON IF THIS FILE ENDS A COMPILATION UNIT IN A .386 FILE.
;*/			// LEAVE THIS ALONE
