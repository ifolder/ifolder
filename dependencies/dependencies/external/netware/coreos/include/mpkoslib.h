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
; * $Workfile:   mpkoslib.h  $
; * $Revision$
; * $Modtime : 27 Aug 1997 12:25:12  $
; * $Author  : CGRIFFIN  $
; ****************************************************************************
; */		// LEAVE THIS ALONE.

//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//
// THIS FILE CONTAINS INFORMATION MEANT FOR NOVELL INTERNAL USE ONLY
//
// The APIs in the file may be accessed from kernel mode (R0) only.
//
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


//
// BEGIN C INCLUDE PORTION
//

#ifndef _MPKOSLIB_H
#define _MPKOSLIB_H


#include	<mpktypes.h>
#include	<mpklib.h>

#define		MPK_THREAD_NON_PREEMPTIVE	0  
#define		MPK_THREAD_PREEMPTIVE		1  


struct  CpuUtilization
{
	UINT	Utilization;	// processor utilization in this time unit
	UINT	DiskRequests;	// # IOs begun in this time unit
	UINT	res[6];			// future information
};

void	kSuspend(void);
ERROR	kResume(THREAD ThreadHandle);

ERROR	kSuspendThreadNonBlocking(THREAD ThreadHandle);

ERROR	kUnblockThreadNoResume(THREAD ThreadHandle);
ERROR	kUnblockThreadWithResume(THREAD ThreadHandle);

ERROR	kGetThreadOnCpu(PROCESSOR, THREAD *);

UINT32	kGetThreshold (void);
void	kSetThreshold (UINT32 newThreshold);

ERROR	kGetCPUUtilization (LONG cpu,
		struct CpuUtilization *UtililizationElements,
		UINT *ElementCount, UINT *TicksPerElement,
		UINT *MilliSecsPerElement);

ERROR	kSetThreadArgument(THREAD ThreadHandle, void *data);

LONG   	kGetThreadState(THREAD ThreadHandle);
LONG   	kGetThreadSuspendReason(THREAD ThreadHandle);

ERROR	kGetTotalThreadTime(THREAD ThreadHandle, UINT64 *microsecs);

ERROR	kGetThreadLoadHandle(THREAD ThreadHandle, HANDLE *pLoadHandle);
ERROR	kSetThreadLoadHandle(THREAD ThreadHandle, HANDLE LoadHandle);

ERROR	kSetExSetShimRetAddress(LONG address);

ERROR	kGetWorkerThreadFlag(THREAD ThreadHandle, UINT *flag);

ERROR	kGetThreadAddressSpaceID(THREAD ThreadHandle,
								struct AddressSpace **AddressSpace);

ERROR	kGetThreadStartAddress(THREAD ThreadHandle, void **StartAddress);

//
// For NPL use only.
//

ERROR	kMutexGetOwner(MUTEX MutexHandle, THREAD *pThreadHandle);

//
// The following APIs will be here until we decide
// which header they will go into.
//

//
// Structure to get information about a specific resource type
// definition.
//

typedef	struct	_ResourceTypeDefInfoStruct
{
	UINT			ResourceType;
	BYTE			ResourceTypeDescription[MAX_NAME_LENGTH + 1];
} ResourceTypeDefInfoStruct;

typedef	struct	_ResourceTagInfoStruct
{
	RESOURCETAG		ResourceTagHandle;
	UINT			ResourceType;
	MODULEHANDLE	ModuleHandle;
	UINT			ResourceCount;
	BYTE			ResourceTagDescription[MAX_NAME_LENGTH + 1];
} ResourceTagInfoStruct;


ERROR	IncrementResourceTagResourceCount(RESOURCETAG RTHandle);
ERROR	DecrementResourceTagResourceCount(RESOURCETAG RTHandle);
UINT	GetDefinedResourceTypeCount(void);
ERROR	GetResourceTypeDefinitionList(UINT *pResTypeArray, UINT NumSlots,
										UINT *pSlotsFilled);
ERROR	GetModuleResourceTagCount(MODULEHANDLE ModuleHandle,
									UINT *pRTagCount);
ERROR	GetModuleResourceTagList(MODULEHANDLE ModuleHandle,
						RESOURCETAG *pResTagArray, UINT NumSlots,
						UINT *pSlotsFilled);
ERROR	GetModuleResourceTagListByResourceType(MODULEHANDLE ModuleHandle,
						UINT ResourceType, RESOURCETAG *pResTagArray,
						UINT NumSlots, UINT *pSlotsFilled);

UINT	GetDefinedResourceTypeCount(void);
ERROR	GetResourceTypeDefinitionInfo(UINT ResourceType,
						ResourceTypeDefInfoStruct *pResourceTypeDefInfoStruct);

ERROR	GetModuleResourceTagInfo(RESOURCETAG RTagHandle,
						MODULEHANDLE ModuleHandle,
						ResourceTagInfoStruct *pRTagInfoStruct);

//
// Structure to get information about a specific module handle.
//

typedef	struct	_ModuleInfoStruct
{
	MODULEHANDLE	ModuleHandle;
	HANDLE			AddressSpaceHandle;
	BYTE			ModuleNameArray[MAX_NAME_LENGTH + 1];

	//
	// WARNING!!!:
	// 		Keep the next field in sync. with LoadDefinitionStructure.
	//

	BYTE			ModuleDescriptionArray[128];
	UINT			ModuleType;
	UINT			ModuleFlags;
	UINT			ModuleMajorVersion;
	UINT			ModuleMinorVersion;
	UINT			ModuleRevision;
	UINT			Year;
	UINT			Month;
	UINT			Day;
	void			*ModuleCodeAddress;
	UINT			ModuleCodeLength;
	void			*ModuleDataAddress;
	UINT			ModuleDataLength;
	UINT			TotalNLMMemoryImage;	// In bytes.
	UINT			ModuleFreeMemory;		// In bytes.
	UINT			ModuleFreeNodes;
	UINT			ModuleAllocatedMemory;	// In bytes.
	UINT			ModuleAllocatedNodes;
	UINT			ModuleTotalMemory;		// In bytes.
											// (= Allocated memory +
											// free memory + overhead).
} ModuleInfoStruct;

//
// Values for ModuleFlags field:
//

#define		MODULE_REENTRANT				0x00000001
#define		MODULE_LOAD_MULTIPLE			0x00000002
#define		MODULE_LOAD_IN_KERNEL_ONLY		0x00000010
#define		MODULE_AUTO_UNLOAD				0x00000040
#define		MODULE_LOAD_IN_USER_MODE		0x00000200
#define		MODULE_PREEMPTIBLE				0x00004000



UINT	GetLoadedModuleCount(void);

//
// Returns a snapshot of modules in InternalModuleList.
//

ERROR	GetInternalModuleList(MODULEHANDLE *pModuleHandleArray, UINT NumSlots,
								UINT *pSlotsFilled);

//
// Returns a snapshot of modules in LoadedList (subset of InternalModuleList).
//

ERROR	GetLoadedModuleList(MODULEHANDLE *pModuleHandleArray, UINT NumSlots,
								UINT *pSlotsFilled);

ERROR	GetModuleInfo(MODULEHANDLE ModuleHandle,
						ModuleInfoStruct *pModuleInfoStruct);

//
// The above APIs will be here until we decide
// which header they will go into.
//



//
// Temporary API for CLIB use to create a classical thread
// in a specified application.
//


LONG	kCMakeProcess(LONG schedulingPriority, void (*codeAddress)(void),
						void *stackAddressHigh, LONG stackLength,
						BYTE *threadName, struct ResourceTagStructure *RTag,
						APPL AppHandle);

//
// CLIB data APIs
//

ERROR	kSetThreadLibraryContext(THREAD ThreadHandle, void *context);
void	*kGetThreadLibraryContext(THREAD ThreadHandle);

//
// CLIB thread data area APIs
//

ERROR	kSetCustomThreadDataArea(THREAD ThreadHandle, void *data);
void	*kGetCustomThreadDataArea(THREAD ThreadHandle);

//
// NPL data APIs
//

ERROR	kSetNPLThreadData(THREAD ThreadHandle, void *data);
void	*kGetNPLThreadData(THREAD ThreadHandle);

//
// NDS data APIs
//

ERROR	kSetThreadNDSData(THREAD ThreadHandle, void *data);
void	*kGetThreadNDSData(THREAD ThreadHandle);

//
// Java data APIs
//

ERROR	kSetThreadJavaData(THREAD ThreadHandle, void *data);
void	*kGetThreadJavaData(THREAD ThreadHandle);

//
// WinSock Data APIs
//

ERROR	kSetThreadWinSockData(THREAD ThreadHandle, void *data);
void	*kGetThreadWinSockData(THREAD ThreadHandle);


//
// Security Parameter Data API
//

ERROR kGetThreadSecurityParameter( THREAD ThreadHandle, void **value );

//
// Keyed Data APIs
//
ERROR kSetCurrentThreadKeyedData( void *Data );
ERROR kGetCurrentThreadKeyedData( void **Data );
ERROR kSetThreadKeyedData( THREAD ThreadHandle, void *Data );
ERROR kGetThreadKeyedData( THREAD ThreadHandle, void **Data );


//
//  Thread Inheritance Overriding of thread creation (USED by external module
//  Loaders to have current thread Override its inheritance properties so that
//  entry points of loading modules can have any threads they created owned by
//  the loaded module.
//

struct ThreadInheritanceOverrideStruct
{
			void	*ModuleHandle;				// New Thread Owner Handle
			APPL	ApplicationHandle;			// ! NULL, application owner
			UINT	ClassicNetWareFlag;			// 0= False, !0= True (be in Classic Netware)
            UINT	Reserved[5];
};


ERROR  kCurrentThreadInheritanceOverride(
			struct ThreadInheritanceOverrideStruct   *Override,
			void (*FunctionToCall)( void *),   
			void   *Argument );




//---------------------------------------------------------------------------
// APIs used by auditing in FILESYS.NLM
//---------------------------------------------------------------------------

ERROR	kSetThreadAuditUserID( THREAD ThreadHandle, LONG userID );
LONG 	kGetThreadAuditUserID( THREAD ThreadHandle );

ERROR	kThreadAcquireAuditIDNameFlag( THREAD ThreadHandle );
ERROR	kThreadReleaseAuditIDNameFlag( THREAD ThreadHandle );

LONG 	kThreadAcquireAuditGenerateRecordFlag( THREAD ThreadHandle );

#define		THREAD_AUDIT_SUCCESS		0
#define		THREAD_AUDIT_BUSY			1
#define 	THREAD_AUDIT_REENTRANT		2

ERROR	kThreadReleaseAuditGenerateRecordFlag( THREAD ThreadHandle );

//
// For use of Novell Labs in certifying a NLM
//

void	kContextSwitchMonitor( THREAD ThreadHandle, UINT microseconds );
ERROR	kEnableContextSwitchMonitor( THREAD ThreadHandle );
ERROR	kDisableContextSwitchMonitor( THREAD ThreadHandle );

//
// Memory Protection
//

ERROR kClearThreadUserSpaceSuspend( THREAD th);
ERROR kSetThreadUserSpaceSuspend( THREAD th);
void	kSetCurrentThreadCallingFromUserSpace(void);
void	kClearCurrentThreadCallingFromUserSpace(void);
void	kSetThreadCallingFromUserSpace( THREAD ThreadHandle );
void	kClearThreadCallingFromUserSpace( THREAD ThreadHandle );

//
// APIs to validate appropriate handles.
//

ERROR	kValidateThread(THREAD ThreadHandle);
ERROR	kAppValidateApp(APPL AppHandle);
ERROR	kRWLockValidate(RWLOCK RWLockHandle);
ERROR	kMutexValidate(MUTEX MutexHandle);
ERROR	kSemaphoreValidate(SEMAPHORE SemaHandle);
ERROR	kBarrierValidate(BARRIER BarrierHandle);
ERROR	kQueValidate(QUE QueHandle);
ERROR	kQueLightValidate(QUE_LIGHT QueHandle);
ERROR	kSpinRWLockValidate(SPINRWLOCK RWLockHandle);
ERROR	kConditionValidate(CONDITION CVHandle);

//
// --------------------------------------------------------------
//
//
// The necessity of these APIs have to be investigated.
// This prevents us from going into a model where the synchronization
// variables need not be kernel objects.
//


#define MUTEX_NAME_LENGTH		32
#define MAX_MUTEX_INFO_COPY 	16

struct  kMutexInformation
{
    BYTE	name[MUTEX_NAME_LENGTH];	// name of this mutex
    THREAD	owner;		// current owner of the mutex
    LONG	count;		// count of waiters on mutex + owner
};

LONG	kMutexQuery(struct kMutexInformation info[MAX_MUTEX_INFO_COPY],
					LONG *nextFlag );
LONG	kMutexCount(void);

#define	SEMAPHORE_NAME_LENGTH	32
#define	MAX_SEMAPHORE_INFO_COPY	16

struct	kSemaphoreInformation
{
    BYTE	name[SEMAPHORE_NAME_LENGTH];	// name of this semaphore
    THREAD	owner;		// current owner of the semaphore
    LONG	count;		// count of waiters on semaphore + owner
};

UINT	kSemaphoreQuery(struct kSemaphoreInformation info[MAX_SEMAPHORE_INFO_COPY],
						UINT *nextFlag );
UINT	kSemaphoreCount(void);


//
// --------------------------------------------------------------
//



//
// Novell Internal Use only.  Access routines for queue routines useful
// for walking the queue.
//

#define	kQueNextElement(X)			(((QLINK *) (X))->link)
#define	kQueClearLinkField(X)		(((QLINK *) (X))->link = NULL)
#define	kQueLightNextElement(X)		(((QLINK_LIGHT *) (X))->pNextNode)
#define	kQueLightClearLinkField(X)	(((QLINK_LIGHT *) (X))->pNextNode = NULL)

// simple structure to reflect the first LONG field of
// a QUEUE structure. Used for fast determination if anything
// is on the queue. Assumes the head pointer is the 1st
// field of the internal structure in QUE.C

typedef struct 
{
	QLINK	*head;
} FQUEUE;

#define FastFirstQLINKNoLock(queue)	(((FQUEUE *)(queue))->head)

struct ResourceTagStructure *kGetThreadResourceTagValue( THREAD ThreadHandle );

//-------------------------------------------------------------------------
// APIs for legacy support ONLY.
//-------------------------------------------------------------------------

ERROR	kSpinRWReadTryLock(SPINRWLOCK rwlock);
ERROR	kSpinRWWriteTryLock(SPINRWLOCK rwlock);
ERROR	kSemaphoreSignalAll(SEMAPHORE SemaHandle);

//
// Novell internal use only.  Note that all other reader writer
// lock APIs are public APIs.  This is meant for supporting CLIB
// ONLY and should not be used for other purposes.  All others
// have to use the explicit read or write unlock APIs.
//

ERROR	kRWUnlock(RWLOCK RWLockHandle);

//
// Novell Internal use only.  Meant for use by the system call
// interface layer only!!!
//

ERROR	kRWLockFreeInternal(RWLOCK RWLockHandle);
ERROR	kMutexFreeInternal(MUTEX MutexHandle);

//
// For CLIB use only!!!
//

ERROR	kSemaphoreFreeLegacy(SEMAPHORE SemaHandle);
ERROR	kMutexUnlockSpecial(MUTEX MutexHandle);


//---------------------------------------------------------------------------
// BEGIN APIs which exist specifically for Platform Support Modules
//---------------------------------------------------------------------------

//----------------------------------------------
//	Defines for kRegisterPhysicalProcessor 
// and kDeRegisterPhysicalProcessor
//----------------------------------------------

#define TIMER_AVAILABLE_FALSE             0x0
#define TIMER_AVAILABLE_TRUE              0x1

#define TIMER_BOUND_TO_INTERRUPT_FALSE    0x0
#define TIMER_BOUND_TO_INTERRUPT_TRUE     0x2

#define TIMER_ONESHOT_SUPPORTED_FALSE     0x0
#define TIMER_ONESHOT_SUPPORTED_TRUE      0x4

#define TIMER_PERIODIC_SUPPORTED_FALSE    0x0
#define TIMER_PERIODIC_SUPPORTED_TRUE     0x8

typedef struct T_PROCESSOR_TAG {
	UINT		PTProcessorNumber;
	UINT		PTProcessorFamily;
	UINT		PTProcessorModel;
	UINT		PTProcessorStepping;
	UINT		PTTimer0StatusFlags;
	UINT		PTTimer0InterruptNumber;
	UINT		PTTimer1StatusFlags;
	UINT		PTTimer1InterruptNumber;
	UINT		PTReserved[8];
} T_PROCESSOR_TAG;

typedef T_PROCESSOR_TAG ProcTagStruct;
typedef T_PROCESSOR_TAG * PROCTAG;

extern ERROR kRegisterPhysicalProcessor(
	PROCTAG processorTag);

extern ERROR kDeRegisterPhysicalProcessor(
	PROCTAG processorTag);

//----------------------------------------------
//
//----------------------------------------------
void kAddNewProcessor(
	UINT processorNumber);

//----------------------------------------------
//	Defines for kPSMLoadRequest
//----------------------------------------------

#define REQUIRED_PSM_SPEC_MAJOR_VERSION 3
#define REQUIRED_PSM_SPEC_MINOR_VERSION 0

#define OS_MODE_UNI_OR_MULTI_PROCESSOR      0
#define OS_MODE_UNI_OR_MULTI_PROCESSOR_SFT3 1

extern ERROR kPSMLoadRequest(
	UINT PSMSpecMajorVersion,
	UINT PSMSpecMinorVersion,
	UINT * operatingSystemMode);

extern ERROR kSetPSMSwitchTableFunction(
	UINT processor,
	UINT functionID,
	ADDR function);

extern ERROR kGetPSMSwitchTableFunction(
	UINT processor,
	UINT functionID,
	ADDR * functionPtr);

//----------------------------------------------
//	Defines for kPSMDetect()
//----------------------------------------------
#define PSM_DETECT_NAME_BUFFER_SIZE 13

#define PSM_DETECT_COMMAND_INITIALIZE				0
#define PSM_DETECT_COMMAND_RESPOND					1
#define PSM_DETECT_COMMAND_FETCH_RESPONSE			2

#define PSM_SUPPORT_NO_RESPONSE_YET					0
#define PSM_SUPPORT_FALSE 								10
#define PSM_SUPPORT_TRUE								30
#define PSM_SUPPORT_TRUE_PSM_HIGHLY_CONFIDENT	40

extern ERROR kPSMDetect(
	UINT detectCommand,
	UINT8 fileNameBuffer[PSM_DETECT_NAME_BUFFER_SIZE],
	UINT * supportResponseCode);

//----------------------------------------------
//
//----------------------------------------------
extern void kDoRealModeHalt(
	void);

extern UINT	kReturnRealModeTransitionCount(
	void);

extern UINT32 RealModeTimerFlag;


UINT kRealTimeDelayUsingPentiumTSC(
	UINT microSeconds);

ERROR kIgnoreNextRealModeInterrupt(
		UINT interruptNumber);

//----------------------------------------------
// Defines for kSetLostIntDetectionMode
//----------------------------------------------
#define PIC_LOST_HARDWARE_DETECTION_FALSE	0
#define PIC_LOST_HARDWARE_DETECTION_TRUE  1

extern ERROR kSetLostIntDetectionMode(
		UINT processorNumber,
		UINT interruptNumber,
		UINT checkForLostInterruptFlag);

extern UINT kIncrementSpuriousIntCount(
		UINT interruptNumber);

extern UINT kIncrementLostIntCount(
		UINT interruptNumber);


//---------------------------------------------------------------------------
// END APIs which exist specifically for Platform Support Modules
//---------------------------------------------------------------------------


// -----------------------------------------------------------------------
//                    Interrrupt Statistics Functions
// -----------------------------------------------------------------------

//----------------------------------------------
// Defines for use with kGetInterruptStatus
//----------------------------------------------
#define FREE_INTERRUPT				0
#define BUS_INTERRUPT				1
#define TIMER_INTERRUPT				2
#define LOCAL_INTERRUPT				3
#define IPI_INTERRUPT				4
#define SYSTEM_INTERRUPT			5

/*
** To determine is an interrupt is used and how many service routines
** are shared on that interrupt.
*/
extern ERROR kGetInterruptStatus(
	UINT interruptNumber,
	UINT * interruptStatus,
	UINT * numberOfServiceRoutines);

// -----------------------------------------------------------------------

/*
** To get the total number of interrupts taken across all processors.
*/
extern ERROR kGetAllIntStatsForAllCPUs(
	UINT * totalInterruptTime,
	UINT * totalInterruptCount);

/*
** To get the total number of interrupts taken by this processor since
** it was last brought on line.
*/
extern ERROR kGetAllIntStatsForSingleCPU(
	UINT cpuNumber,
	UINT * cpuTotalInterruptTime,
	UINT * cpuTotalInterruptCount);

// -----------------------------------------------------------------------
// For the following 6 APIs the user must register for the event
// EVENT_INTERRUPT_HANDLER_CHANGE.  The OS signals this event when
// an interrupt handler has been added or removed.  When this happens
// the array which contains the handler information is changed.  And it
// it will be necessary to call kGetInterruptStatus() to determine the 
// current state of the interrupt.

/*
** Puts the resource tag description string for this interrupt in the buffer.
*/
extern ERROR kGetInterruptDescription(
	UINT interruptNumber,
	UINT8 buffer[],
	UINT bufferSize);

/*
**	Gets the total interrupt count for this interrupt across all processors.
*/
extern ERROR kGetSingleIntStatsForAllCPUs(
	UINT interruptNumber,
	UINT * totalInterruptTime,
	UINT * totalInterruptCount,
	UINT * totalSpuriousInterruptCount,
	UINT * totalLostInterruptCount);

/*
**	Gets the total interrupt count for this interrupt for the specified
** processor.
*/
extern ERROR kGetSingleIntStatsForSingleCPU(
	UINT cpuNumber,
	UINT interruptNumber,
	UINT * cpuTotalInterruptTime,
	UINT * cpuTotalInterruptCount,
	UINT * cpuTotalSpuriousInterruptCount,
	UINT * cpuTotalLostInterruptCount);

// -----------------------------------------------------------------------
/*
** Puts the resource tag description string for this ISR in the buffer.
*/
extern ERROR kGetISRDescription(
	UINT interruptNumber,
	UINT serviceRoutineIndexNumber,
	UINT8 * buffer,
	UINT bufferSize);

/*
**	Gets the total interrupt count for this ISR across all processors.
*/
extern ERROR kGetISRStatsForAllCPUs(
	UINT interruptNumber,
	UINT serviceRoutineIndexNumber,
	UINT * totalInterruptCount);

/*
**	Gets the total interrupt count for this ISR for the specified
** processor.
*/
extern ERROR kGetISRStatsForSingleCPU(
	UINT cpuNumber,
	UINT interruptNumber,
	UINT serviceRoutineIndexNumber,
	UINT * cpuTotalInterruptCount);

// -----------------------------------------------------------------------
//	Interrupt Information Functions
// -----------------------------------------------------------------------

extern ERROR kGetVectorAssignedToInterrupt(
	UINT interrupt,
	UINT * hardwareVector);

extern ERROR kGetInterruptAssignedToVector(
	UINT hardwareVector,
	UINT * interrupt);

extern UINT kReturnMaximumSystemInterrupts();

extern UINT kReturnMaximumSharedInterrupts();

// -----------------------------------------------------------------------
// Interrupt Support Definitions
// -----------------------------------------------------------------------

// -----------------------------------------------------------------------
// Interrupt Service Routine Return Codes
// -----------------------------------------------------------------------
#define	INTERRUPT_SERVICED							0
#define	INTERRUPT_NOT_SERVICED						1

// -----------------------------------------------------------------------
//Interrupt Setup Return Codes
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
//Interrupt Mask/Unmask/EOI/Clear/Distribute Return Codes
// -----------------------------------------------------------------------
#define	ISUCCESS											0
#define	IERR_HARDWARE_FAILURE					   5
#define	IERR_INVALID_INTTAG							20
#define  ISUCCESS_PARTIAL_DISTRIBUTION          26
#define	IERR_INTERRUPT_NOT_ALLOCATED				41

// -----------------------------------------------------------------------
//Interrupt Alloc Return Codes (30)
// -----------------------------------------------------------------------
#define	ISUCCESS											0
#define	IERR_INVALID_RESOURCE_TAG					24
#define	IERR_UNAVAILABLE_VECTOR						30

// -----------------------------------------------------------------------
//Interrupt Free Return Codes  (40)
// -----------------------------------------------------------------------
#define	ISUCCESS											0
#define	IERR_INVALID_INTERRUPT						21
#define	IERR_INVALID_RESOURCE_TAG					24
#define	IERR_INTERRUPT_STILL_IN_USE				40
#define	IERR_INTERRUPT_NOT_ALLOCATED				41

// -----------------------------------------------------------------------
//Inter Processor Return Codes (50)
// -----------------------------------------------------------------------
#define	ISUCCESS											0
#define	IERR_HARDWARE_FAILURE						5
#define	IERR_INVALID_INTTAG							20
#define	IERR_INVALID_VECTOR							50
#define	IERR_NO_DESTINATION_SPECIFIED				51

// -----------------------------------------------------------------------
//Timer Interrupt Return Codes  (60)
// -----------------------------------------------------------------------
#define	ISUCCESS											0
#define	IERR_HARDWARE_FAILURE						5
#define	IERR_INVALID_INTTAG							20
#define	ISUCCESS_TIMER_SET							60
#define	ISUCCESS_TIMER_NOT_SET						61
#define	IERR_TIMER_MODE_NOT_SUPPORTED				62
#define	IERR_TIMER_INTERVAL_NOT_SUPPORTED		63

// -----------------------------------------------------------------------
// Interrupt Setup Flag Definitions
// -----------------------------------------------------------------------
#define	FLAGS_SHARE_INTERRUPT						0x00000001
#define	FLAGS_CALL_FROM_REAL_MODE					0x00000004
#define	FLAGS_SHARE_WITH_REAL_MODE					0x00000008
#define	FLAGS_ISR_MP_ENABLED							0x20000000
#define	FLAGS_SAVE_FPU_STATE							0x40000000

// -----------------------------------------------------------------------
//	Miscellaneous Interrupt Support Functions
// -----------------------------------------------------------------------
ERROR kCallOSInterrupt(
	UINT interruptNumber);

ERROR kShutdownDOSClient(
	void);

UINT kFlushAllPendingInterrupts(
	UINT minimumMicroSecondDelay);


// -----------------------------------------------------------------------
// MPK Non Maskable Interrupt Functions 
// -----------------------------------------------------------------------

UINT NonMaskableInterruptCheck(void);

UINT NonMaskableInterruptReset(void);

UINT NonMaskableInterruptSend(UINT8 destinationProcessors[MAXIMUM_NUMBER_OF_PROCESSORS]);


// -----------------------------------------------------------------------
// MPK Bus Interrupt Functions 
// -----------------------------------------------------------------------

extern UINT BusInterruptAlloc(
     UINT * interruptPtr,
     struct ResourceTagStructure * resourceTag);

//Defined in MPKLIB.H
//extern UINT BusInterruptClear(
//     INTTAG interruptTag);

extern UINT BusInterruptDistribute(
     INTTAG interruptTag,
     BYTE   destinationProcessors[MAXIMUM_NUMBER_OF_PROCESSORS]);

//Defined in MPKLIB.H
//extern UINT BusInterruptEOI(
//     INTTAG interruptTag);

extern UINT BusInterruptFree(
     UINT interrupt,
     struct ResourceTagStructure * resourceTag);

//Defined in MPKLIB.H
//extern UINT BusInterruptMask(
//     INTTAG interruptTag);

//Defined in MPKLIB.H
//extern UINT BusInterruptSetup(
//     UINT * interruptPtr,
//     UINT (*serviceRoutine)(void *serviceRoutineParameter),
//     void * serviceRoutineParameter,
//     UINT32 flags,
//     UINT32 hardwareInstanceNumber,
//     struct ResourceTagStructure * resourceTag,
//     INTTAG * interruptTagPtr);

//Defined in MPKLIB.H
//extern UINT BusInterruptUnmask(
//     INTTAG interruptTag);

// -----------------------------------------------------------------------
// MPK Timer Interrupt Functions 
// -----------------------------------------------------------------------

extern UINT TimerInterruptAlloc(
     UINT * interruptPtr,
     struct ResourceTagStructure * resourceTag);

extern UINT TimerInterruptClear(
     INTTAG interruptTag);

extern UINT TimerInterruptEOI(
     INTTAG interruptTag);

extern UINT TimerInterruptFree(
     UINT interrupt,
     struct ResourceTagStructure * resourceTag);

extern UINT TimerInterruptMask(
     INTTAG interruptTag);

extern UINT TimerInterruptSetup(
     UINT interrupt,
     UINT (*serviceRoutine)(void *serviceRoutineParameter),
     void * serviceRoutineParameter,
     UINT32 flags,
     struct ResourceTagStructure *   resourceTag,
     INTTAG * interruptTagPtr);

extern UINT TimerInterruptStart(
     INTTAG interruptTag,
     UINT timerMode,
     UINT timeInterval,
     UINT conditionFlag,
     UINT * timeRemaining);

extern UINT TimerInterruptStop(
     INTTAG interruptTag,
     UINT * timeRemaining);

extern UINT TimerInterruptUnmask(
     INTTAG interruptTag);

// -----------------------------------------------------------------------
// MPK Local Interrupt Functions 
// -----------------------------------------------------------------------

extern UINT LocalInterruptAlloc(
     UINT * interruptPtr,
     struct ResourceTagStructure * resourceTag);

extern UINT LocalInterruptClear(
     INTTAG interruptTag);

extern UINT LocalInterruptEOI(
     INTTAG interruptTag);

extern UINT LocalInterruptFree(
     UINT interrupt,
     struct ResourceTagStructure * resourceTag);

extern UINT LocalInterruptMask(
     INTTAG interruptTag);

extern UINT LocalInterruptSetup(
     UINT interrupt,
     UINT (*serviceRoutine)(void *serviceRoutineParameter),
     void * serviceRoutineParameter,
     UINT32 flags,
     struct ResourceTagStructure *   resourceTag,
     INTTAG * interruptTagPtr);

extern UINT LocalInterruptUnmask(
     INTTAG interruptTag);

// -----------------------------------------------------------------------
// MPK Inter Processor Interrupt Functions 
// -----------------------------------------------------------------------

extern UINT InterProcessorInterruptAlloc(
     UINT * interruptPtr,
     struct ResourceTagStructure * resourceTag);

extern UINT InterProcessorInterruptClear(
     INTTAG interruptTag);

extern UINT InterProcessorInterruptEOI(
     INTTAG interruptTag);

extern UINT InterProcessorInterruptFree(
     UINT interrupt,
     struct ResourceTagStructure * resourceTag);

extern UINT InterProcessorInterruptSend(
     INTTAG interruptTag,
     BYTE destinationProcessors[MAXIMUM_NUMBER_OF_PROCESSORS]);

extern UINT InterProcessorInterruptSetup(
     UINT interrupt,
     UINT (*serviceRoutine)(void *serviceRoutineParameter),
     void * serviceRoutineParameter,
     UINT32 flags,
     struct ResourceTagStructure * resourceTag,
     INTTAG * interruptTagPtr);

// -----------------------------------------------------------------------
// MPK System Event Interrupt Functions 
// -----------------------------------------------------------------------

extern UINT SystemInterruptAlloc(
     UINT * interruptPtr,
     struct ResourceTagStructure *   resourceTag,
     UINT (*eoiRoutine)(INTTAG interruptTag),
     UINT (*maskRoutine)(INTTAG interruptTag),
     UINT (*unmaskRoutine)(INTTAG interruptTag));

extern UINT SystemInterruptClear(
     INTTAG interruptTag);

extern UINT SystemInterruptEOI(
     INTTAG interruptTag);

extern UINT SystemInterruptFree(
     UINT interrupt,
     struct ResourceTagStructure *   resourceTag);

extern UINT SystemInterruptMask(
     INTTAG interruptTag);

extern UINT SystemInterruptSetup(
     UINT interrupt,
     UINT (*serviceRoutine)(void *serviceRoutineParameter),
     void * serviceRoutineParameter,
     UINT32 flags,
     struct ResourceTagStructure * resourceTag,
     INTTAG * interruptTagPtr);

extern UINT SystemInterruptUnmask(
     INTTAG interruptTag);

// -----------------------------------------------------------------------
//
// -----------------------------------------------------------------------

/****************************************************************************/
/****************************************************************************/
#endif		// _MPKOSLIB_H

#if	0		// LEAVE THIS ALONE.
ELSE		; LEAVE THIS ALONE.


;IFNDEF _MPKOSLIB_H ; MPKOSLIB.H Assembly Portion
;_MPKOSLIB_H = 1
;************************************************************************
; ***************  BEGIN ASSEMBLY INCLUDE PORTION.  *********************
;************************************************************************

;Some of these defines are defined in MPKLIB.H

;Interrupt Service Routine Return Codes
;INTERRUPT_SERVICED                         EQU   0
;INTERRUPT_NOT_SERVICED                     EQU   1

;Interrupt Setup Return Codes
;ISUCCESS                                   EQU   0
;IERR_INVALID_PARAMETER                     EQU   1
;IERR_INTERRUPT_NOT_SHAREABLE               EQU   2
;IERR_SHARING_LIMIT_EXCEEDED                EQU   3
;IERR_REAL_MODE_SHARING_LIMIT_EXCEEDED      EQU   4
;IERR_HARDWARE_FAILURE                      EQU   5
;IERR_HARDWARE_ROUTE_NOT_AVAILABLE          EQU   6
;IERR_MEMORY_ALLOCATION_FAILURE             EQU   8
;IERR_INTERRUPT_IS_SHAREABLE                EQU   9
;IERR_INVALID_INTTAG                        EQU   20
;IERR_INVALID_INTERRUPT                     EQU   21
;IERR_INVALID_FLAGS                         EQU   22
;IERR_INVALID_HARDWARE_INSTANCE_NUMBER      EQU   23
;IERR_INVALID_RESOURCE_TAG                  EQU   24
;IERR_INTERRUPT_NOT_ALLOCATED               EQU   41

;Interrupt Mask/Unmask/EOI/Clear/Distribut Return Codes
;ISUCCESS                                   EQU   0
;IERR_HARDWARE_FAILURE                      EQU   5
;IERR_INVALID_INTTAG                        EQU   20
ISUCCESS_PARTIAL_DISTRIBUTION               EQU   26
;IERR_INTERRUPT_NOT_ALLOCATED               EQU   41

;Interrupt Alloc Return Codes (30)
;ISUCCESS                                   EQU   0
;IERR_INVALID_RESOURCE_TAG                  EQU   24
IERR_UNAVAILABLE_VECTOR                     EQU   30

;Interrupt Free Return Codes  (40)
;ISUCCESS                                   EQU   0
;IERR_INVALID_RESOURCE_TAG                  EQU   24
IERR_INTERRUPT_STILL_IN_USE                 EQU   40
;IERR_INTERRUPT_NOT_ALLOCATED               EQU   41

;Inter Processor Return Codes (50)
;ISUCCESS                                   EQU   0
;IERR_HARDWARE_FAILURE                      EQU   5
;IERR_INVALID_INTTAG                        EQU   20
IERR_INVALID_VECTOR                         EQU   50
IERR_NO_DESTINATION_SPECIFIED               EQU   51

;Timer Interrupt Return Codes  (60)
;ISUCCESS                                   EQU   0
;IERR_HARDWARE_FAILURE                      EQU   5
;IERR_INVALID_INTTAG                        EQU   20
ISUCCESS_TIMER_SET                          EQU   60
ISUCCESS_TIMER_NOT_SET                      EQU   61
IERR_TIMER_MODE_NOT_SUPPORTED               EQU   62
IERR_TIMER_INTERVAL_NOT_SUPPORTED           EQU   63

;Interrupt Setup Flag Definitions
;FLAGS_SHARE_INTERRUPT                      EQU   0x00000001
;FLAGS_CALL_FROM_REAL_MODE                  EQU   0x00000004
;FLAGS_SHARE_WITH_REAL_MODE                 EQU   0x00000008
;FLAGS_ISR_MP_ENABLED                       EQU   0x20000000
;FLAGS_SAVE_FPU_STATE                       EQU   0x40000000


;************************************************************************
;*****************  END ASSEMBLY INCLUDE PORTION.  **********************
;************************************************************************
;ENDIF ; _MPKOSLIB_H  MPKOSLIB.H Assembly Portion


;****************************************************************************
;*********************** END OF FILE ****************************************
;****************************************************************************

ENDIF	; LEAVE THIS ALONE.

COMMENT();	// LEAVE THIS ALONE
#endif
/* (		// LEAVE THIS ALONE
; END ; REMOVE THE FIRST SEMICOLON IF THIS FILE ENDS A COMPILATION UNIT IN A .386 FILE.
;*/			// LEAVE THIS ALONE
