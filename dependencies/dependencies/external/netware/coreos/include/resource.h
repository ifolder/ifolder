COMMENT (); //		LEAVE THIS ALONE.
/*(		// LEAVE THIS ALONE.
IF	0	; LEAVE THIS ALONE.
;*****************************************************************************
;*
;*	(C) Copyright 1989-1996 Novell, Inc.
;*	All Rights Reserved.
;*
;*	This program is an unpublished copyrighted work which is proprietary
;*	to Novell, Inc. and contains confidential information that is not
;*	to be reproduced or disclosed to any other person or entity without
;*	prior written consent from Novell, Inc. in each and every instance.
;*
;*	WARNING:  Unauthorized reproduction of this program as well as
;*	unauthorized preparation of derivative works based upon the
;*	program or distribution of copies by sale, rental, lease or
;*	lending are violations of federal copyright laws and state trade
;*	secret laws, punishable by civil and criminal penalties.
;*
;*	INCLUDE FILE THAT CAN BE INCLUDED BOTH IN C AND ASSEMBLY SOURCES
;*
;*	$Workfile:   resource.h  $
;*	$Modtime:   27 Aug 1998 16:31:30  $
;*	$Revision$
;*	$Author$
;*
;*****************************************************************************
;*/

// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
// %%%%%%%%%%%%%%%%%%% BEGIN C INCLUDE PORTION. %%%%%%%%%%%%%%%%%%%%
// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

#ifndef __RESOURCE_H__
#define __RESOURCE_H__

// DEFECT286531
struct ResourceTagInfoStructure
{
	unsigned long moduleid_handle;
	unsigned long RTSignature;
	unsigned long RTResourceCount;
	unsigned long RTagCount;
};


struct ResourceTagStructure
{
	struct ResourceTagStructure *RTLink;
	struct LoadDefinitionStructure *RTModule;
	LONG RTSignature;
	LONG RTResourceCount;
	LONG RTCustomData[4];
	LONG RTFlags;
	BYTE RTDescriptionString[1];
};

struct ResourceSignatureStructure
{
	struct ResourceSignatureStructure *RSLink;
	LONG RSSignature;
	void (*RSCleanUpProcedure)(
				struct ResourceTagStructure *RTag,
				LONG RForceFlag);
	LONG RSFlags;
	struct ResourceTagStructure	*RSResourceTag;
	BYTE RSDescriptionString[DIM_0_OR_1];	
};

//
// RSFlags field:
//

#define	RESOURCE_TYPE_VALID		0x1

extern struct LoadDefinitionStructure *OSHandle;

/****************************************************************************/
/*  This area defines all of the resources signatures for NetWare 386		 */
/* 	Any additions to this area also need to be made in resources.inc		 */
/*  Values are High Low so they will be meaningful when viewed in memory.   */
/****************************************************************************/

#define AESProcessSignature				0x50534541			/* 'PSEA' */
/* #define AllocCacheAlignSignature		0x43524C41	*/		/* 'CRLA' */
#define AllocHighSignature				0x48524C41			/* 'HRLA' */
#define AllocIOSignature				0x49524C41			/* 'IRLA' */
#define AllocSignature					0x54524C41			/* 'TRLA' */
#define AllocStackSignature				0x53524C41			/* 'SRLA' */
#define AlternateKeyHandlerSignature	0x48424B41			/* 'HBKA' */
#define AuditServiceSignature			0x52455341			/* 'RESA' */
#define BreakpointSignature				0x54504B42			/* 'TPKB' */
#define BroadcastNotificationSignature	0x534E4342			/* 'SNCB' */
#define CacheBelow16MegMemorySignature	0x36314243			/* '61BC' */
#define CacheMovableMemorySignature		0x544D4D43			/* 'TMMC' */
#define CacheNonMovableMemorySignature	0x544D4E43			/* 'TMNC' */
#define ConnectionSignature				0x54434E43			/* 'TCNC' */
#define ConsoleCommandSignature			0x4D4F4343			/* 'MOCC' */
#define CompressionSignature			0x53504d43			/* 'SPMC' */
#define DebugCommandSignature			0x53504344			/* 'SPCD' */
#define DebuggerSignature				0x47554244			/* 'GUBD' */
#define DecompressionSignature          0x534d4344			/* 'SMCD' */
#define DiskDeviceSignature				0x444B5344			/* 'DKSD' */
#define DiskDriverSignature				0x4B534444			/* 'KSDD' */
#define DiskLogicalPartitionSignature	0x4C4B5344			/* 'LKSD' */
#define DiskPartitionSignature			0x504B5344			/* 'PKSD' */
#define DiskSystemSignature				0x534B5344			/* 'SKSD' */
#define ECBSignature					0x53424345			/* 'SBCE' */
#define EventSignature					0x544E5645			/* 'TNVE' */
#define ExceptionSignature				0x50435845			/* 'PCXE' */
#define ExpressionParserSignature		0x53504544			/* 'SPED' */
#define FileSystemMonitorSignature		0x534d5346			/* 'SMSF' */
#define GlobalBranchTableSignature		0x47425445			/* 'GBTE' */
#define HistogramSignature				0x54534948			/* 'TSIH' */
#define InterruptSignature				0x50544E49			/* 'PTNI' */
#define InterruptAllocSignature		0x41544E49			/* 'ATNI' */
#define IOConfigHookSignature			0x48434F49			/* 'HCOI' */
#define IOFSCommSignature				0x4D434649			/* 'MCFI' */
#define IORegistrationSignature			0x53524F49			/* 'SROI' */
#define LoadableRouterSignature			0x5452444C			/* 'TRDL' */
#define LoaderExtensionSignature		0x5852444C			/* 'XRDL' */
#define LSLAESEventSignature			0x5345414C			/* 'SEAL' */
#define LSLDefaultStackSignature		0x444C534C			/* 'DLSL' */
#define LSLPreScanStackSignature		0x504C534C			/* 'PLSL' */
#define LSLStackSignature				0x534C534C			/* 'SLSL' */
#define LSLTxPreScanStackSignature		0x544C534C			/* 'TLSL' */
#define MLIDSignature					0x44494C4D			/* 'DILM' */
#define MSLErrorHandlerSignature			0x454C534D			/* 'ELSM' */
#define MSLSignature					0x444C534D			/* 'DLSM' */
#define MSL2TestInterfaceSignature 0x544C534D      /* 'TLSM' */
/* MMApplicationSignature should have been 'PAMM' but was coded wrong, too late now. */
#define MMApplicationSignature			0x50424D4D			/* 'PBMM' */
#define MMCacheSignature				0x41434D4D			/* 'ACMM' */
#define MMEncryptorSignature			0x4E454D4D			/* 'NEMM' */
#define MMNotifySignature				0x4F4E4D4D			/* 'ONMM' */
#define MMIdentifySignature				0x44494D4D			/* 'DIMM' */
#define MMScramblerSignature			0x52534D4D			/* 'RSMM' */
#define NameServiceSignature			0x5245534E			/* 'RESN' */
#define NCPExtensionSignature			0x4550434E			/* 'EPCN' */
#define NCPVerbSignature				0x5650434E			/* 'VPCN' */
#define NetManSignature					0x44494d4e			/* 'DIMN' */
#define NoAThrdCallBackSignature		0x4354414E			/* 'CTAN' */
#define NoSleepWorkCallBackSignature	0x4357534e			/* 'CWSN' */
#define PermMemorySignature				0x544D5250			/* 'TMRP' */
#if (FSEngine)
#define PolicySignature					0x32434C50			/* '2CLP' */
#else	/* (!FSEngine) */
#define PolicySignature					0x31434C50			/* '1CLP' */
#endif	/* (!FSEngine) */
/* #define PolicySignature					0x59434C50			#* 'YCLP' #/ */

#define PollingProcedureSignature		0x52504C50			/* 'RPLP' */
#define ProcessSignature				0x53435250			/* 'SCRP' */
#define ResourceSignatureSignature		0x53525452			/* 'SRTR' */
#define ScreenHandlerSignature			0x444E4148			/* 'DNAH' */
#define ScreenModeSignature				0x004D4353			/* 'xMCS' */
#define ScreenInputSignature			0x494E4353			/* 'INCS' */
#define ScreenSignature					0x4E524353			/* 'NRCS' */
#define SecondProcessorSignature		0x50444E32			/* 'PDN2' */
#define SemaphoreSignature				0x504D4553			/* 'PMES' */
#define SemiPermMemorySignature			0x454D5053			/* 'EMPS' */
#define ServiceFreeSignature			0x53465343			/* 'SFSC' */
#define SetableParameterSignature		0x4D505453			/* 'MPTS' */
#define SocketSignature					0x4B434F53			/* 'KCOS' */
#define StreamSignature					0x4D525453			/* 'MRTS' */
#define SystemCallSignature 			0x43535953			/* 'CSYS' */
#define TaskSignature					0x4B534154			/* 'KSAT' */
#define TimerSignature					0x524D4954			/* 'RMIT' */
#define	TimeSyncAdjustmentSignature		0x4A415354			/* 'JAST' */
#define WorkCallBackSignature			0x50424357			/* 'PBCW' */
#define SendMessageSignature			0x47534d53			/* 'GSMS' */
#define WatchdogNotificationSignature	0x534E4457			/* 'SNDW' */
#define SetParameterCategorySignature	0x53435053			/* 'SCPS' */
#define CommandLineServicesSignature	0x5043574e			/* 'PCWN' */
#define	NetManReplaceSignature 			0x53524d4e			/* 'SRMN' */
#define	NCPDLNSignature					0x534e4c44			/* 'SNLD' */
#define	NCPServiceSignature				0x5350434e			/* 'SPCN' */
#define	RemoteEventSignature			0x5345524e			/* 'SERN' */
#define	ThreadSecuritySignature			0x5353544e			/* 'SSTN' */

#define	NetWareClusterServicesSignature	0x5343574E		/* 'SCWN' ==> NWCS */


#endif	// __RESOURCE_H__

#if		0		// LEAVE THIS ALONE.
ELSE	; LEAVE THIS ALONE.

;****************************************************************************
;**************** BEGIN ASSEMBLY INCLUDE PORTION OF FILE. *******************
;****************************************************************************

ResourceTagStructure STRUC
	RTLink				DD	?
	RTModule			DD	?
	RTSignature			DD	?
	RTResourceCount		DD	?
	RTCustomData		DD	4 DUP (?)
	RTFlags				DD	?
	RTDescriptionString	DB	?
ResourceTagStructure ENDS

;****************************************************************************/
;	This area defines all of the resources signatures for NetWare 386
;	Any additions to this area also need to be made in resources.h
;	Values are stored Low High so they will appear meaningful when
;	view in memory.
;****************************************************************************/

AESProcessSignature					EQU		'PSEA'
;;; AllocCacheAlignSignature			EQU		'CRLA'
AllocHighSignature					EQU		'HRLA'
AllocIOSignature					EQU		'IRLA'
AllocSignature						EQU		'TRLA'
AllocStackSignature					EQU		'SRLA'
AlternateKeyHandlerSignature		EQU		'HBKA'
AuditServiceSignature				EQU		'RESA'
BreakpointSignature					EQU		'TPKB'
BroadcastNotificationSignature		EQU		'SNCB'
CacheBelow16MegMemorySignature		EQU		'61BC'
CacheMovableMemorySignature			EQU		'TMMC'
CacheNonMovableMemorySignature		EQU		'TMNC'
CompressionSignature				EQU     'SPMC'
ConnectionSignature					EQU		'TCNC'
ConsoleCommandSignature				EQU		'MOCC'
DebugCommandSignature				EQU		'SPCD'
DebuggerSignature					EQU		'GUBD'
DecompressionSignature          	EQU     'SMCD'
DiskDeviceSignature					EQU		'DKSD'
DiskDriverSignature					EQU		'KSDD'
DiskLogicalPartitionSignature		EQU		'LKSD'
DiskPartitionSignature				EQU		'PKSD'
DiskSystemSignature					EQU		'SKSD'
ECBSignature						EQU		'SBCE'
EventSignature						EQU		'TNVE'
ExceptionSignature					EQU		'PCXE'
ExpressionParserSignature			EQU		'SPED'
FileSystemMonitorSignature			EQU		'SMSF'
GlobalBranchTableSignature			EQU		'GBTE'
HistogramSignature					EQU		'TSIH'
InterruptSignature					EQU		'PTNI'
InterruptAllocSignature				EQU		'ATNI'
IOConfigHookSignature				EQU		'HCOI'
IOFSCommSignature					EQU		'MCFI'
IORegistrationSignature				EQU		'SROI'
LoadableRouterSignature 			EQU		'TRDL'
LoaderExtensionSignature			EQU		'XRDL'
LSLAESEventSignature				EQU		'SEAL'
LSLDefaultStackSignature			EQU		'DLSL'
LSLPreScanStackSignature			EQU		'PLSL'
LSLStackSignature					EQU		'SLSL'
LSLTxPreScanStackSignature			EQU		'TLSL'
MLIDSignature						EQU		'DILM'
MSLErrorHandlerSignature    		EQU	 	'ELSM'
MSLSignature						EQU		'DLSM'
MSL2TestInterfaceSignature  		EQU 	'TLSM'
;* MMApplicationSignature should have been 'PAMM' but was coded wrong, too late now.
MMApplicationSignature				EQU		'PBMM'
MMCacheSignature					EQU		'ACMM'
MMEncryptorSignature				EQU		'NEMM'
MMNotifySignature					EQU		'ONMM'
MMIdentifySignature					EQU		'DIMM'
MMScramblerSignature				EQU		'RSMM'
NameServiceSignature				EQU		'RESN'
NCPExtensionSignature				EQU		'EPCN'
NCPVerbSignature					EQU		'VPCN'
NetManSignature						EQU		'DIMN'
NoAThrdCallBackSignature			EQU		'CTAN'
NoSleepWorkCallBackSignature		EQU		'CWSN'
PermMemorySignature					EQU		'TMRP'
if (FSEngine)
PolicySignature						EQU		'2CLP'
else	;(NOT FSEngine)
PolicySignature						EQU		'1CLP'
endif	;(NOT FSEngine)
;;;PolicySignature					EQU		'YCLP'

PollingProcedureSignature			EQU		'RPLP'
ProcessSignature					EQU		'SCRP'
ResourceSignatureSignature			EQU		'SRTR'
ScreenHandlerSignature				EQU		'DNAH'
ScreenModeSignature					EQU		'MCS'
ScreenInputSignature				EQU		'INCS'
ScreenSignature						EQU		'NRCS'
SecondProcessorSignature			EQU		'PDN2'
SemaphoreSignature					EQU		'PMES'
SemiPermMemorySignature				EQU		'EMPS'
ServiceFreeSignature				EQU		'SFSC'
SetableParameterSignature			EQU		'MPTS'
SocketSignature						EQU		'KCOS'
StreamSignature						EQU		'MRTS'
TaskSignature						EQU		'KSAT'
TimerSignature						EQU		'RMIT'
TimeSyncAdjustmentSignature			EQU		'JAST'
WorkCallBackSignature				EQU		'PBCW'
SendMessageSignature				EQU		'GSMS'
WatchdogNotificationSignature		EQU		'SNDW'
SetParameterCategorySignature		EQU		'SCPS'
CommandLineServicesSignature		EQU		'PCWN'
NetManReplaceSignature 				EQU		'SRMN'
NCPDLNSignature						EQU		'SNLD'
NCPServiceSignature					EQU		'SPCN'
RemoteEventSignature				EQU		'SERN'
ThreadSecuritySignature				EQU		'SSTN'
NetWareClusterServicesSignature	EQU		'SCWN'

ENDIF		; LEAVE THIS ALONE.
COMMENT();	// LEAVE THIS ALONE.
#endif		// LEAVE THIS ALONE.

/* (	; LEAVE THIS ALONE.
; END ; REMOVE THE FIRST SEMICOLON IF THIS FILE ENDS A COMPILATION UNIT IN A .386 FILE.
;*/		// LEAVE THIS ALONE.
