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
; * $Workfile:   mpklib.h  $
; * $Revision$
; * $Modtime : 27 Aug 1997 12:24:46  $
; * $Author  : CGRIFFIN  $
; ****************************************************************************
; */		// LEAVE THIS ALONE.


//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//
// This include file contains information that is intended for Novell
// Internal as well as controlled third party access (Ex: driver developers).
// Any information that is meant for Novell Internal Use Only should
// NOT be present here.
//
// The routines in the include file should be called only from kernel mode
// (Ring 0).
//
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


//
// BEGIN C INCLUDE PORTION
//

#ifndef _MPKLIB_H
#define _MPKLIB_H


#include	<mpktypes.h>
#include	<mpkapis.h>

#define	MAXIMUM_NUMBER_OF_PROCESSORS	32

/* read only variable */
extern PROCESSOR CpuCurrentProcessor;

ERROR	kBindThread( THREAD ThreadHandle, PROCESSOR processorNumber );
ERROR	kUnbindThread( THREAD ThreadHandle );

#define	NOT_BOUND_TO_CPU	((UINT)(-1))

ERROR	kGetEffectiveBinding (THREAD ThreadHandle, LONG *binding);

PROCESSOR	kCurrentThreadBinding( THREAD ThreadHandle );

ERROR	kSetThreadKernelStackInfo(LONG StackAddressLow, LONG StackSize);
ERROR  	kGetThreadStackInfo(THREAD ThreadHandle, LONG *StackAddressLow,
							LONG *StackSize);
LONG   	kGetCurrentAvailableStackSize(void);

//
// User stack memory information structure.
//

typedef struct   UserStackInfo
{
	ADDR	StackAddressLow;
	LONG	UserStackSize;
}  USERSTACK_INFO;

LONG	kGetUserStackInfo(THREAD ThreadHandle, USERSTACK_INFO *UserStackInfo);


ERROR	kGetThreadProcessorAssignment (THREAD ThreadHandle, LONG *processor);

//
// New WorkToDo APIs
//

struct WorkToDoStructure;	//lint !e763

ERROR kScheduleWorkToDo( struct WorkToDoStructure *work );
ERROR kScheduleWorkToDoDirected( struct WorkToDoStructure *work, PROCESSOR processorNumber );
ERROR kCancelWorkToDo( struct WorkToDoStructure *work);

ERROR kScheduleFastWorkToDoLocal( struct WorkToDoStructure *fwork );
ERROR kScheduleFastWorkToDoDirected( struct WorkToDoStructure *fwork, PROCESSOR processorNumber );
ERROR kScheduleFastWorkToDo( struct WorkToDoStructure *fwork );

/*

//
// simple structure to reflect the first LONG field of
// a QUEUE structure. Used for fast determination if anything
// is on the queue. Assumes the head pointer is the 1st
// field of the internal structure in QUE.C

typedef struct 
{
	QLINK			*head;		// MUST BE FIRST LONG OF STRUC: pointer to head of queue
} FQUEUE;

#define FastFirstQLINKNoLock( queue ) \
		((FQUEUE*)(queue))->head
*/

//
// SpinLock Definitions.  Novell Internal Use Only.
//

ERROR	kSpinLockInit(SPINLOCK *pSpinLock);
void	kSpinLock(SPINLOCK *pSpinLock);
ERROR	kSpinTryLock(SPINLOCK *pSpinLock);
void	kSpinUnlock(SPINLOCK *pSpinLock);
FLAGS	kSpinLockDisable(SPINLOCK *pSpinLock);
ERROR	kSpinTryLockDisable(SPINLOCK *pSpinLock, FLAGS *flags);
void	kSpinUnlockRestore(SPINLOCK *pSpinLock, FLAGS flags);


//
// Spinlock protecting the system DMA channels.  Used by
// 16 bit LAN and disk drivers.
// WARNING!!!:
// 		Must be called with interrupts disabled.
//

void	DMAMutexLock(void);
void	DMAMutexUnlock(void);

//
// Spinning version of reader writer locks.
//

SPINRWLOCK	kSpinRWLockAlloc(BYTE *NamePtr);
ERROR		kSpinRWLockFree(SPINRWLOCK rwlock);
ERROR		kSpinRWReadLock(SPINRWLOCK rwlock);
ERROR		kSpinRWReadUnlock(SPINRWLOCK rwlock);
ERROR		kSpinRWWriteLock(SPINRWLOCK rwlock);
ERROR		kSpinRWWriteUnlock(SPINRWLOCK rwlock);


//
// Ring 0 condition variables.
//

ERROR	kCVAlloc(BYTE *pCVNamePtr, KCONDITION *pCVHandlePtr);
ERROR	kCVDestroy(KCONDITION CVHandle);
ERROR	kCVWait(KCONDITION CVHandle, SPINLOCK *pSpinLockPtr);
ERROR	kCVTimedWait(KCONDITION CVHandle, SPINLOCK *pSpinLockPtr,
							LONG MilliSecTimeOutInterval);
ERROR	kCVSignal(KCONDITION CVHandle);
ERROR	kCVBroadcast(KCONDITION CVHandle);


//
// Node definition for the doubly linked list.
//

typedef	struct	_LIST_NODE
{
	struct	_LIST_NODE	*pNextNode;
	struct	_LIST_NODE	*pPreviousNode;
} LIST_NODE;

//
// The doubly linked list list-head.
//

typedef	struct	_KDBL_LIST_HEAD
{
	LIST_NODE	*pFirstNode;
	LIST_NODE	*pLastNode;
	UINT32		NodeCount;		// # of nodes in list.
	UINT32		Signature;
	RWLOCK		RWLockHandle;	// Reader writer lock protecting the list.
	UINT32		Reserved[3];	// For cache alignment.
} KDBL_LIST_HEAD;

ERROR	kDoublyLinkedListAlloc(
	HANDLE *pListHeadHandle);
ERROR	kDoublyLinkedListFree(HANDLE ListHeadHandle);
ERROR	kDoublyLinkedListInit(HANDLE ListHeadHandle);
ERROR	kDoublyLinkedListAddElement(HANDLE ListHeadHandle,
										LIST_NODE *pListNode);
ERROR	kDoublyLinkedListAddElementNoLock(HANDLE ListHeadHandle,
										LIST_NODE *pListNode);
ERROR	kDoublyLinkedListDeleteElement(HANDLE ListHeadHandle,
										LIST_NODE *pListNode);
ERROR	kDoublyLinkedListDeleteElementNoLock(HANDLE ListHeadHandle,
										LIST_NODE *pListNode);
ERROR	kDoublyLinkedListReadLock(HANDLE ListHeadHandle);
ERROR	kDoublyLinkedListReadUnlock(HANDLE ListHeadHandle);
ERROR	kDoublyLinkedListWriteLock(HANDLE ListHeadHandle);
ERROR	kDoublyLinkedListWriteUnlock(HANDLE ListHeadHandle);
ERROR	kDoublyLinkedListGetFirstElementNoLock(HANDLE ListHeadHandle,
											LIST_NODE **ppListNode);

#define	kDoublyLinkedListGetNextElement(pListNode)		((pListNode)->pNextNode)


//
// Callout related. See also CALLOUT.C.
//

typedef struct _CALLOUT_STRUCT
{
	//
	// Begin caller visible portion.
	//

	struct	_CALLOUT_STRUCT	*pNext;
	struct	_CALLOUT_STRUCT	*pPrev;
	void	(*pFunction)(void *);	// Routine to be called. (caller supplied)
	void	*FunctionArgument;		// Argument to be passed (caller supplied).
	UINT32	TimeOutValue;			// In timer minor ticks.
									// (caller supplied)

	//
	// End of caller visible portion.  The following
	// fields are defined in CALLOUT.C.  Keep in sync
	// with CALLOUT.C.
	//

	UINT32	Reserved1[3];
} CALLOUT_STRUCT;


ERROR	kScheduleCallOut(CALLOUT_STRUCT *pCallOutStruct);
ERROR	kCancelCallOut(CALLOUT_STRUCT *pCallOutStruct);

//
// API used to decide the # of ticks to be used for a callout's
// timeout value.
//

UINT	GetTimerMinorTicksPerSecond(void);

//
// Returns the "polling level" supported by the OS.
//
//	Poll support level:
//		0 - Polling not supported.
//		1 - Limited support, polling called infrequenly.
//		2 - Polling fully supported, polling may become infrequent,
//				interrupt backup recommended.
//		3 - Polling fully supported, no need for interrupt backup.
//

UINT	GetPollSupportLevel(void);

HANDLE	kRegisterPoll(void (*pPollRoutine)(void *),
						void *PollRoutineArgument);
ERROR	kUnregisterPoll(HANDLE PollHandle);
ERROR	kSuspendPoll(HANDLE PollHandle);
ERROR	kResumePoll(HANDLE PollHandle);

//
// API to get the thread context of a suspended thread.
//

typedef	struct	_CONTEXT
{
	UINT	Register_EAX;
	UINT	Register_EBX;
	UINT	Register_ECX;
	UINT	Register_EDX;

	UINT	Register_ESI;
	UINT	Register_EDI;
	UINT	Register_EBP;

	UINT	Register_ESP;

	UINT	Register_EIP;
	UINT	Register_EFlags;

	UINT	Reserved[6];
} CONTEXT;

ERROR	kGetThreadContext(THREAD ThreadHandle, CONTEXT *pContextStruct);

//
// The following APIs will be retained here until we decide
// whether they have to be put in MPKOSLIB.H
//

ERROR	GetOpenScreenList(SCREEN *pScreenArray, UINT NumSlots, UINT *pSlotsFilled);
UINT	GetNumberOfOpenScreens(void);


//
// The above APIs will be retained here until we decide
// whether they have to be put in MPKOSLIB.H
//


// -----------------------------------------------------------------------
// Interrupt Support Definitions
// -----------------------------------------------------------------------

// -----------------------------------------------------------------------
// Interrupt Service Routine Return Codes
// -----------------------------------------------------------------------
#define	INTERRUPT_SERVICED							0
#define	INTERRUPT_NOT_SERVICED						1

// -----------------------------------------------------------------------
// Bus Interrupt Setup Return Codes
// -----------------------------------------------------------------------
#define	ISUCCESS											0
#define	IERR_INVALID_PARAMETER						1
#define	IERR_INTERRUPT_NOT_SHAREABLE				2
#define	IERR_SHARING_LIMIT_EXCEEDED				3
#define	IERR_REAL_MODE_SHARING_LIMIT_EXCEEDED	4
#define	IERR_HARDWARE_FAILURE						5
#define	IERR_HARDWARE_ROUTE_NOT_AVAILABLE		6
#define	IERR_MEMORY_ALLOCATION_FAILURE			8
#define	IERR_INTERRUPT_IS_SHAREABLE				9
#define	IERR_INVALID_INTTAG							20
#define	IERR_INVALID_INTERRUPT						21
#define	IERR_INVALID_FLAGS							22
#define	IERR_INVALID_HARDWARE_INSTANCE_NUMBER	23
#define	IERR_INVALID_RESOURCE_TAG					24
#define	IERR_INTERRUPT_NOT_ALLOCATED				41

// -----------------------------------------------------------------------
// Bus Interrupt Setup Flag Definitions
// -----------------------------------------------------------------------
#define	FLAGS_SHARE_INTERRUPT						0x00000001
#define	FLAGS_CALL_FROM_REAL_MODE					0x00000004
#define	FLAGS_SHARE_WITH_REAL_MODE					0x00000008
#define	FLAGS_ISR_MP_ENABLED							0x20000000
#define	FLAGS_SAVE_FPU_STATE							0x40000000

// -----------------------------------------------------------------------
// MPK Bus Interrupt Functions 
// -----------------------------------------------------------------------

extern UINT BusInterruptClear(
     INTTAG interruptTag);

extern UINT BusInterruptEOI(
     INTTAG interruptTag);

extern UINT BusInterruptMask(
     INTTAG interruptTag);

extern UINT BusInterruptSetup(
     UINT * interruptPtr,
     UINT (*serviceRoutine)(void *serviceRoutineParameter),
     void * serviceRoutineParameter,
     UINT32 flags,
     UINT32 hardwareInstanceNumber,
     struct ResourceTagStructure * resourceTag,
     INTTAG * interruptTagPtr);

extern UINT BusInterruptUnmask(
     INTTAG interruptTag);

// -----------------------------------------------------------------------
// Real Mode Event Definitions
// -----------------------------------------------------------------------
#define	REAL_MODE_NESTED_INTERRRUPTS           0x00000001
#define	REAL_MODE_INTERRUPTS_DISABLED          0x00000002


// -----------------------------------------------------------------------
// Processor Information APIs
// -----------------------------------------------------------------------

PROCESSOR kReturnCurrentProcessorID(void);

UINT kReturnOnLineProcessorCount(void);

UINT kReturnPhysicalProcessorCount(void);

// -----------------------------------------------------------------------
// Defines for kGetProcessorStatus
// -----------------------------------------------------------------------
#define PROCESSOR_OFF_LINE     0
#define PROCESSOR_ON_LINE      1

ERROR kGetProcessorStatus(UINT processorNumber, UINT * processorStatus);

// -----------------------------------------------------------------------
// Processor Management APIs
// -----------------------------------------------------------------------

ERROR kStartProcessor(UINT processorNumber);

ERROR kStopProcessor(UINT processorNumber);

// -----------------------------------------------------------------------
// Preemption APIs 
// -----------------------------------------------------------------------

ERROR kSetNLMPreemption(struct LoadDefinitionStructure *module,
								BOOLEAN preemptableFlag);

ERROR kSetCodePreemption(ADDR codeAddress, LONG length, BOOLEAN preemptableFlag);

// -----------------------------------------------------------------------
// Critical Section APIs: If code is marked preemptible, then this allows
//	control programaticly by thread to selectively turn off preemption.
//	recommended: Run outside of a Critical Section
// -----------------------------------------------------------------------


ERROR	kEnterCriticalSection(void);	// stop any possible preemption
ERROR	kExitCriticalSection(void);	// allow preemption if possbile
BOOLEAN	kGetCriticalSectionStatus(void);


/****************************************************************************/
/****************************************************************************/
#endif		// _MPKLIB_H

#if	0		// LEAVE THIS ALONE.
ELSE		; LEAVE THIS ALONE.

;IFNDEF _MPKLIB_H ; MPKLIB.H Assembly Portion
;_MPKLIB_H = 1
;************************************************************************
; ***************  BEGIN ASSEMBLY INCLUDE PORTION.  *********************
;************************************************************************

MAXIMUM_NUMBER_OF_PROCESSORS             EQU   32

PROCESSOR_OFF_LINE                       EQU   0
PROCESSOR_ON_LINE                        EQU   1


; -----------------------------------------------------------------------
; Interrupt Service Routine Return Codes
; -----------------------------------------------------------------------
INTERRUPT_SERVICED                       EQU   0
INTERRUPT_NOT_SERVICED                   EQU   1

; -----------------------------------------------------------------------
; Bus Interrupt Return Codes
; -----------------------------------------------------------------------
ISUCCESS                                 EQU   0
IERR_INVALID_PARAMETER                   EQU   1
IERR_INTERRUPT_NOT_SHAREABLE             EQU   2
IERR_SHARING_LIMIT_EXCEEDED              EQU   3
IERR_REAL_MODE_SHARING_LIMIT_EXCEEDED    EQU   4
IERR_HARDWARE_FAILURE                    EQU   5
IERR_HARDWARE_ROUTE_NOT_AVAILABLE        EQU   6
IERR_MEMORY_ALLOCATION_FAILURE           EQU   8
IERR_INTERRUPT_IS_SHAREABLE              EQU   9
IERR_INVALID_INTTAG                      EQU   20
IERR_INVALID_INTERRUPT                   EQU   21
IERR_INVALID_FLAGS                       EQU   22
IERR_INVALID_HARDWARE_INSTANCE_NUMBER    EQU   23
IERR_INVALID_RESOURCE_TAG                EQU   24
IERR_INTERRUPT_NOT_ALLOCATED             EQU   41

; -----------------------------------------------------------------------
; Bus Interrupt Setup Flag Definitions
; -----------------------------------------------------------------------
FLAGS_SHARE_INTERRUPT                    EQU   01h
FLAGS_CALL_FROM_REAL_MODE                EQU   04h
FLAGS_SHARE_WITH_REAL_MODE               EQU   08h
FLAGS_ISR_MP_ENABLED                     EQU   020000000h
FLAGS_SAVE_FPU_STATE                     EQU   040000000h


; -----------------------------------------------------------------------
; Real Mode Event Definitions
; -----------------------------------------------------------------------
REAL_MODE_NESTED_INTERRUPTS              EQU   01h
REAL_MODE_INTERRUPTS_DISABLED            EQU   02h


;************************************************************************
;*****************  END ASSEMBLY INCLUDE PORTION.  **********************
;************************************************************************
;ENDIF ; _MPKLIB_H  MPKLIB.H Assembly Portion

;****************************************************************************
;*********************** END OF FILE ****************************************
;****************************************************************************

ENDIF	; LEAVE THIS ALONE.

COMMENT();	// LEAVE THIS ALONE
#endif
/* (		// LEAVE THIS ALONE
; END ; REMOVE THE FIRST SEMICOLON IF THIS FILE ENDS A COMPILATION UNIT IN A .386 FILE.
;*/			// LEAVE THIS ALONE
