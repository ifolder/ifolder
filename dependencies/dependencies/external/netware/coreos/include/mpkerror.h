COMMENT();	// LEAVE THIS ALONE.
/*(			// LEAVE THIS ALONE.

if	0		; LEAVE THIS ALONE.
;*****************************************************************************
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
; * $Author$
; * $Modtime:   21 Jun 2000 09:54:24  $
; * $Workfile:   mpkerror.h  $
; * $Revision$                                                                              
; *
; ****************************************************************************
; */		// LEAVE THIS ALONE.

//
// BEGIN C INCLUDE PORTION
//

#ifndef _MPKERROR_H_
#define _MPKERROR_H_

#include	<mpktypes.h>

#define kSUCCESS			                               (0)
#define kBUSY				                               (1)

#define ERROR_INVALID_HANDLE                      (0x1002)
#define ERROR_NOT_IN_GROUP                        (0x1003)
#define ERROR_FAILURE                             (0x1004)
#define ERROR_THREAD_SCHEDULED                    (0x1005)
#define ERROR_NO_MEMORY                           (0x1006)
#define ERROR_LIST_READER_COUNT_0                 (0x1007)
#define ERROR_INITIALIZATION_FAILURE              (0x1008)
#define ERROR_NO_HANDLES                          (0x1009)
#define ERROR_THREAD_STACK_TOO_SMALL              (0x100d)
#define ERROR_STACK_OVERFLOW                      (0x100e)
#define ERROR_INVALID_MUTEX                       (0x1010)
#define ERROR_CPU_NUMBER_INVALID                  (0x1011)
#define ERROR_CPU_NOT_AVAILABLE                   (0x1012)
#define ERROR_CPU_CANT_ACTIVATE                   (0x1013)
#define ERROR_PROCESSOR_STATE_INVALID             (0x1014)
#define ERROR_INVALID_SEMAPHORE                   (0x1015)
#define ERROR_INVALID_BARRIER                     (0x1016)
#define ERROR_INVALID_RWLOCK                      (0x1017)
#define ERROR_INVALID_PARAMETER                   (0x1018)
#define ERROR_INVALID_THREAD                      (0x101A)
#define ERROR_BUFFER_OVERFLOW                     (0x101B)
#define ERROR_REQUEST_DENIED                      (0x101D)
#define ERROR_THREAD_NOT_FOUND                    (0x101E)
#define ERROR_THREAD_UNBLOCK_FUNCTION_INVALID     (0x101F)
#define ERROR_THREAD_NOT_SUSPENDED                (0x1020)
#define ERROR_INVALID_QUE                         (0x1021)
#define ERROR_INVALID_LIST                        (0x1022)
#define ERROR_INVALID_CALLOUT                     (0x1023)
#define ERROR_TIMEOUT                             (0x1024)
#define ERROR_ALREADY_OWNER                       (0x1025)
#define ERROR_THREAD_DETACHED                     (0x1026)
#define ERROR_INVALID_KERNEL_CV                   (0x1027)
#define ERROR_INVALID_USER_CV                     (0x1028)
#define ERROR_INVALID_APPLICATION                 (0x1029)
#define	ERROR_INVALID_MODULE_HANDLE				  (0x102A)
#define	ERROR_INVALID_RTAG_HANDLE				  (0x102B)
#define	ERROR_SLEEP_INTERRUPTED					  (0x102C)
#define ERROR_STACK_UNDERFLOW                     (0x102D)
#define	ERROR_STACK_POINTER_OUT_OF_RANGE		  (0x102E)
#define	ERROR_INVALID_RESOURCE_TYPE				0x102F
#define	ERROR_THREAD_TERMINATED					0x1030
#define	ERROR_INVALID_SPIN_RWLOCK				0x1031
#define	ERROR_INVALID_QUE_LIGHT					0x1032



#define ERROR_INVALID_PROCESSOR_SPECIFIED         (0x1101)
#define ERROR_SECONDARY_PROCESSOR_DOES_NOT_EXIST  (0x1102)
#define ERROR_PROCESSOR_ALREADY_ON_LINE           (0x1103)
#define ERROR_PROCESSOR_PRE_INIT_FAILED           (0x1104)
#define ERROR_PROCESSOR_ACTIVATION_FAILED         (0x1105)
#define ERROR_PROCESSOR_START_REQUEST_DENIED      (0x1106)
#define ERROR_PROCESSOR_ALREADY_OFF_LINE          (0x1107)
#define ERROR_PROCESSOR_CLEANUP_FAILED            (0x1108)
#define ERROR_PROCESSOR_DEACTIVATION_FAILED       (0x1109)
#define ERROR_PROCESSOR_STOP_REQUEST_DENIED       (0x110A)

#define	ERROR_RWLOCK_UPGRADE_FAILED			       0x110B
#define	ERROR_NOT_LOCK_OWNER					   0x110C

#define FAULT_NO_MEMORY                                (1)
#define FAULT_SchedThreadBegin                         (2)
#define FAULT_StartThread                              (9)
#define	FAULT_DONTDOTHAT							  (10)
#define FAULT_AppInitialize                           (12)
#define FAULT_Bad_CPU_CR2                             (13)
#define FAULT_ITEM_MISSING_ON_LIST                    (14)
#define FAULT_ProcessorComingUp                       (15)
#define FAULT_IdleThreadCreationError                 (16)
#define FAULT_ThreadStateIsNot_TERMINAL               (17)
#define FAULT_ContextSwitchInISR                      (18)

//this should go into mpkoslib.h

/*ERROR MpkError( ERROR error_code );
void MpkFault( ERROR error_code );

#define ReturnMpkError( error_code ) \
 {    \
 return( MpkError( error_code ) );  \
 }


***************************************************************************/
#endif /* _MPKERROR_H__ */

#if 0  // LEAVE THIS ALONE.
ELSE  ; LEAVE THIS ALONE.

;************************************************************************
;****************  BEGIN ASSEMBLY INCLUDE PORTION.  *********************
;************************************************************************

kSUCCESS			                          EQU     0)
kBUSY				                          EQU     1)

ERROR_INVALID_HANDLE                     EQU     1002h
ERROR_NOT_IN_GROUP                       EQU     1003h
ERROR_FAILURE                            EQU     1004h
ERROR_THREAD_SCHEDULED                   EQU     1005h
ERROR_NO_MEMORY                          EQU     1006h
ERROR_LIST_READER_COUNT_0                EQU     1007h
ERROR_INITIALIZATION_FAILURE             EQU     1008h
ERROR_NO_HANDLES                         EQU     1009h
ERROR_THREAD_STACK_TOO_SMALL             EQU     100dh
ERROR_STACK_OVERFLOW                     EQU     100eh
ERROR_INVALID_MUTEX                      EQU     1010h
ERROR_CPU_NUMBER_INVALID                 EQU     1011h
ERROR_CPU_NOT_AVAILABLE                  EQU     1012h
ERROR_CPU_CANT_ACTIVATE                  EQU     1013h
ERROR_PROCESSOR_STATE_INVALID            EQU     1014h
ERROR_INVALID_SEMAPHORE                  EQU     1015h
ERROR_INVALID_BARRIER                    EQU     1016h
ERROR_INVALID_RWLOCK                     EQU     1017h
ERROR_INVALID_PARAMETER                  EQU     1018h
ERROR_INVALID_THREAD                     EQU     101Ah
ERROR_BUFFER_OVERFLOW                    EQU     101Bh
ERROR_REQUEST_DENIED                     EQU     101Dh
ERROR_THREAD_NOT_FOUND                   EQU     101Eh
ERROR_THREAD_UNBLOCK_FUNCTION_INVALID    EQU     101Fh
ERROR_THREAD_NOT_SUSPENDED               EQU     1020h
ERROR_INVALID_QUE                        EQU     1021h
ERROR_INVALID_LIST                       EQU     1022h
ERROR_INVALID_CALLOUT                    EQU     1023h
ERROR_TIMEOUT                            EQU     1024h
ERROR_ALREADY_OWNER                      EQU     1025h
ERROR_THREAD_DETACHED                    EQU     1026h
ERROR_INVALID_KERNEL_CV                  EQU     1027h
ERROR_INVALID_USER_CV                    EQU     1028h
ERROR_INVALID_APPLICATION                EQU     1029h
ERROR_INVALID_MODULE_HANDLE				 EQU	 102AH
ERROR_INVALID_RTAG_HANDLE				 EQU	 102BH
ERROR_SLEEP_INTERRUPTED					 EQU	 102CH
ERROR_STACK_UNDERFLOW                    EQU	 102DH
ERROR_STACK_POINTER_OUT_OF_RANGE		 EQU	 102EH
ERROR_INVALID_RESOURCE_TYPE				EQU		102FH
ERROR_THREAD_TERMINATED					EQU		1030H
ERROR_INVALID_SPIN_RWLOCK				EQU		1031H
ERROR_INVALID_QUE_LIGHT					EQU		1032H


ERROR_INVALID_PROCESSOR_SPECIFIED        EQU     1101h
ERROR_SECONDARY_PROCESSOR_DOES_NOT_EXIST EQU     1102h
ERROR_PROCESSOR_ALREADY_ON_LINE          EQU     1103h
ERROR_PROCESSOR_PRE_INIT_FAILED          EQU     1104h
ERROR_PROCESSOR_ACTIVATION_FAILED        EQU     1105h
ERROR_PROCESSOR_START_REQUEST_DENIED     EQU     1106h
ERROR_PROCESSOR_ALREADY_OFF_LINE         EQU     1107h
ERROR_PROCESSOR_CLEANUP_FAILED           EQU     1108h
ERROR_PROCESSOR_DEACTIVATION_FAILED      EQU     1109h
ERROR_PROCESSOR_STOP_REQUEST_DENIED      EQU     110Ah

ERROR_RWLOCK_UPGRADE_FAILED				 EQU	 110BH
ERROR_NOT_LOCK_OWNER					 EQU	 110CH

FAULT_NO_MEMORY                          EQU     1
FAULT_SchedThreadBegin                   EQU     2
FAULT_StartThread                        EQU     9
FAULT_DONTDOTHAT                         EQU     10
FAULT_AppInitialize                      EQU     12
FAULT_Bad_CPU_CR2                        EQU     13
FAULT_ITEM_MISSING_ON_LIST               EQU     14
FAULT_ProcessorComingUp                  EQU     15
FAULT_IdleThreadCreationError            EQU     16
FAULT_ThreadStateIsNot_TERMINAL          EQU     17
FAULT_ContextSwitchInISR                 EQU     18

;****************************************************************************
;*********************** END OF FILE ****************************************
;****************************************************************************

ENDIF ; LEAVE THIS ALONE.

COMMENT(); // LEAVE THIS ALONE
#endif
/* (  // LEAVE THIS ALONE
; END ; REMOVE THE FIRST SEMICOLON IF THIS FILE ENDS A COMPILATION UNIT IN A .386 FILE.
;*/   // LEAVE THIS ALONE

