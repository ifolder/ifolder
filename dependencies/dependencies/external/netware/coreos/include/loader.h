#ifndef __LOADER_H__
#define __LOADER_H__
/*****************************************************************************
 *
 *	(C) Copyright 1988-1999 Novell, Inc.
 *	All Rights Reserved.
 *
 *	This program is an unpublished copyrighted work which is proprietary
 *	to Novell, Inc. and contains confidential information that is not
 *	to be reproduced or disclosed to any other person or entity without
 *	prior written consent from Novell, Inc. in each and every instance.
 *
 *	WARNING:  Unauthorized reproduction of this program as well as
 *	unauthorized preparation of derivative works based upon the
 *	program or distribution of copies by sale, rental, lease or
 *	lending are violations of federal copyright laws and state trade
 *	secret laws, punishable by civil and criminal penalties.
 *
 *  $Workfile:   loader.h  $
 *  $Modtime:   21 May 2001 15:05:40  $
 *  $Revision$
 *
 ****************************************************************************/

#include <mpktypes.h>

/*************************** macro definitions *****************************/

#define LD_MODULE_DESCRIPTION_MAX_LENGTH 127
#define LD_MODULE_LONG_NAME_MAX_LENGTH 255

#define	MPKLoaderChanges	TRUE

/* defines for MPKAPIListType field of LoadTimeInfoStruct */
#define MPK_API_LIST_EMPTY							0
#define MPK_API_LIST_IS_FUNNELED_APIS			1
#define MPK_API_LIST_IS_NON_FUNNELED_APIS		2

#define NORMALNLM		0x04
#define	LONGNAMENORMAL	0x05
#define PACKEDNLM		0x84
#define LONGNAMEPACKED	0x85

/* if these #defines are changed, they must be changed in loader.inc as well */
/* changing the values (names are not important) of these #defines may require
   changes in deftoc as well so that the outputs of deftoc match */

// Be careful how you use these "opposite types"
#define XPDFlagData					0x00
#define XPDFlagNoBranch 			0x00
#define XPDFlagNotShared			0x00

#define XPDFlagFunction 			0x01
#define XPDFlagBranch				0x02
#define XPDFlagShared				0x04
#define XPDCNBFunnelStubNeeded	0x08
#define XPDCNBFunnelStubInserted	0x10

/* Maximum length of a logical board name */
#define MaximumLogicalNameLength 17

//
// bagStamp field:
//

#define BAGFILE_STAMP           0x46474142		// 'FGAB'

//
// bagMajorVer and bagMinorVer
//

#define BAGFILE_MAJOR_VERSION   2
#define BAGFILE_MINOR_VERSION   0

//
// bagType field: (Actually the record type).
//

#define BAG_TYPE_SMP					0x00000001
#define	RECORD_TYPE_MPK_NLM_INFO		0x00000002


#define	MPK_NLM_MT_SAFE				0x00000001
#define	MPK_NLM_PREEMPTIBLE			0x00000002
#define	MPK_FUNNELED_API_LIST		0x00000004
#define	MPK_NLM_MT_UNSAFE			0x00000008


//
// stamp field:
//

#define SMP_RPC_STAMP				0x44504d53		// 'SMPD'

//
// version field:
//

#define SMP_RPC_VERSION		1

//
// flags field:
//

#define SMP_FLAG_ALL_APIS			0x00000001

/* Define the public types */
#define PUBLIC_TYPE_INTERNAL		0
#define PUBLIC_TYPE_EXTERNAL		1
#define PUBLIC_TYPE_NEW_EXTERNAL	2
#define PUBLIC_TYPE_OLD_INTERNAL	3
#define PUBLIC_TYPE_OLD_EXTERNAL	4
#define PUBLIC_TYPE_GLOBAL_USER		5
#define PUBLIC_TYPE_GLOBAL_USER_OLD	6

/*	defines for the LoadDefinitonStructure's LDFlags member. */

#define LDModuleIsReEntrantBit				0x00000001
#define LDModuleCanBeMultiplyLoadedBit		0x00000002
#define LDSynchronizeStart					0x00000004
#define LDPseudoPreemptionBit				0x00000008
#define LDLoadInKernel						0x00000010
#define LDAutoUnload  						0x00000040
#define LDHiddenModule						0x00000080
#define LDDigitallySignedFile				0x00000100
#define LDLoadProtected						0x00000200
#define LDSharedLibraryModule				0x00000400
#define LDRestartable						0x00000800
#define LDUnsafeToUnloadNow					0x00001000
#define LDModuleIsUniprocessor				0x00002000
#define LDPreemptable						0x00004000
#define LDHasSystemCalls					0x00008000	/* This module has memory protection marshalling code*/
#define LDVirtualMemory						0x00010000
#define LDAllExportsMTSafe					0x00020000

// Available bit positions
#define Available_1							0x00040000
#define Available_2							0x00080000
#define Available_3							0x00100000
#define Available_4							0x00200000
#define Available_5							0x00400000
#define Available_6							0x00800000
/* Clib is using this one. It is just reserved for them.*/
#define LDTSR								0x01000000
#define LDUNICODE_STRINGS				0x02000000

#define LDHotSwapDriver						0x04000000
#define LDStartupDeviceNLMBit				0x08000000
#define LDBoundNLMBit						0x10000000
#define LDDontUnloadBit 					0x20000000
#define LDModuleIsBeingDebugged		 		0x40000000
#define LDMemoryOn4KBoundriesBit			0x80000000

/* bitmask for the flags settable from the load file header */
#define LD_LOAD_FILE_FLAGS \
(LDModuleIsReEntrantBit | LDModuleCanBeMultiplyLoadedBit | LDSynchronizeStart | \
LDPseudoPreemptionBit | LDLoadInKernel | LDAutoUnload | LDHiddenModule | \
LDDigitallySignedFile | LDLoadProtected | LDSharedLibraryModule | LDRestartable | \
LDHotSwapDriver |  LDBoundNLMBit | LDDontUnloadBit | LDModuleIsBeingDebugged | \
LDMemoryOn4KBoundriesBit | LDTSR | LDUNICODE_STRINGS)

// unknown are the following: LDHasSystemCalls, 

// LDMemoryOn4KBoundriesBit - what meaning does this have now?

// It seems like we should do the LDStartupDeviceNLMBit and LDBound cases
// differently

/* LoadModule load options */
#define LO_NORMAL				0x00000000
#define LO_STARTUP				0x00000001
#define LO_PROTECT				0x00000002
#define LO_DEBUG				0x00000004
#define LO_AUTO_LOAD			0x00000008
#define LO_DONT_PROMPT			0x00000010
#define LO_LOAD_LOW				0x00000020
#define LO_RETURN_HANDLE		0x00000040
#define LO_LOAD_SILENT			0x00000080
#define LO_DELAY_RPC_DEF		0x00000100
#define LO_RESTART				0x00000200

#define LO_DONT_DISPLAY_ERROR	0x00002000

#define LO_MEMORY_DEBUG			0x00010000
#define LO_RELAXED_MEMORY_DEBUG 0x00020000

#define	LO_CHECK_STARTUP		0x10000000
#define	LO_SKIP_CFG_INFO		0x20000000
#define	LO_LOAD_BOUND_IN_ONLY	0x40000000
#define	LO_BOUND				0x80000000

/* Loader returned error codes */
#define LOAD_COULD_NOT_FIND_FILE			1
#define LOAD_ERROR_READING_FILE 			2
#define LOAD_NOT_NLM_FILE_FORMAT			3
#define LOAD_WRONG_NLM_FILE_VERSION			4
#define LOAD_REENTRANT_INITIALIZE_FAILURE	5
#define LOAD_CAN_NOT_LOAD_MULTIPLE_COPIES	6
#define LOAD_ALREADY_IN_PROGRESS			7
#define LOAD_NOT_ENOUGH_MEMORY				8
#define LOAD_INITIALIZE_FAILURE 			9
#define LOAD_INCONSISTENT_FILE_FORMAT		10
#define LOAD_CAN_NOT_LOAD_AT_STARTUP		11
#define LOAD_AUTO_LOAD_MODULES_NOT_LOADED	12
#define LOAD_UNRESOLVED_EXTERNAL			13
#define LOAD_PUBLIC_ALREADY_DEFINED			14
#define LOAD_XDC_DATA_ERROR					15
#define LOAD_NOT_KERNEL						16
#define LOAD_NIOS_ONLY_NLM					17
#define LOAD_ADDRESS_SPACE_CREATION			18
#define LOAD_INITIALIZE_FAULT				19

/* public address types */
#define LOCAL_PUBLIC_ADDRESS				0
#define GLOBAL_PUBLIC_ADDRESS				1
#define REAL_PUBLIC_ADDRESS					2
#define ENV_AWARE_ADDRESS					50

/* InterceptPublicAddress errors.*/
#define INVALID_RECORD_TYPE					-1

/* mapped memory types */
/* define the protection areas */
#define MEM_LOCAL_DATA            0x00000001
#define MEM_LOCAL_CODE            0x00000002
#define MEM_USER_CODE             0x00000004
#define MEM_USER_DATA             0x00000008
#define MEM_OS_DATA               0x00000010
#define MEM_OS_CODE               0x00000020
#define MEM_PCB                   0x00000040
#define MEM_SHARED_CODE	          0x00000080
#define MEM_LOGICAL_NEQ_PHYSICAL  0
#define MEM_LOGICAL_EQ_PHYSICAL   MEM_LOCAL_DATA

/* memory map sizes */
#define USER_CODE_AREA_SIZE		(128 * 1024 * 1024)
#define SHARED_CODE_AREA_SIZE		(128 * 1024 * 1024)

/* define the registerable xdc procedure types */
#define XDC_NLM_START		0
#define XDC_NLM_CHECK		1
#define XDC_NLM_EXIT		2
#define XDC_NLM_OTHER		3

/* command-line registry flags */
#define CL_STARTUP	0x00000001	/* STARTUP.NCF */
#define CL_AUTOEXEC	0x00000002	/* AUTOEXEC.NCF */
#define CL_IOSTART	0x00000004	/* IOAUTO.NCF */
#define CL_IOAUTO	0x00000008	/* IOAUTO.NCF */
#define CL_MSSTART	0x00000010	/* MSSTART.NCF */
#define CL_MSAUTO	0x00000020	/* MSAUTO.NCF */

#define PROTECTED_TEXT		  		"PROTECTED "
#define RELOAD_TEXT					"RELOAD"
#define RESTART_TEXT				"RESTART "
#define NO_RESTART_TEXT			 	"NO RESTART"
#define ADDRESS_SPACE_TEXT			"ADDRESS SPACE"
#define DEFAULT_AS_NAME_TEXT		"ADDRESS_SPACE"
#define FORCED_UNLOAD_TEXT			"KILL"


/*** GREEN RIVER CMC LOADER CHANGE ***/
#define	MaximumNumberOfUpdateRoutines		20   /* from ins.h */
#define	NMLongIncrHi						2

#define LT_DOS			1
#define LT_OS2			2
#define LT_MSWIN31	3

/*********************** type and tag declarations *************************/

struct MPKAPIInfoList
{
	struct MPKAPIInfoList *next;
	BYTE matchedToExport;	/* flag to indicate whether this API has been matched to an export */
	BYTE name[1];
};

struct LoadTimeInfoStruct
{
	LONG MPKAPIListType;
	struct MPKAPIInfoList *MPKAPIList;
	LONG MainCodeCNBFunnelCount;
	LONG SharedCodeCNBFunnelCount;
};

//
// The structures and #defines are also used by
// SMPRPC and MPKXDC tools. 
//
// Since both 16-bit as well as 32-bit executables
// of the tools are generated using this header
// files, the fields in the structure
// have explicit field sizes specified in them.
//

//
// The "bag" header.  Identifies a specific bag type.
// v4.11 SMP understands only one bag type and stops
// processing XDC data if any other bag type is
// found.  Some NLMs (especially CLIB's LIB0.NLM)
// use XDC information containing records understood
// only by MPK along with records that are understood
// by both SMP and MPK.  This precludes the
// possibility of using a more efficient scheme.
//

struct bagHeader		/* 32 bytes */
{
	UINT32	bagStamp;				// The bag type.
	BYTE	bagMajorVer;
	BYTE	bagMinorVer;
	BYTE	bagZeros[2];
	UINT32	bagRecCount;
	BYTE	bagZeros2[20];
};

//
// The record header. 
//

struct bagRecHeader		/* 80 bytes */
{
	UINT32	bagType;				// Actually the record type.
	UINT32	bagFlags;				// Does anybody use this field???
	UINT32	bagSize;				// # of bytes of data following
									// this structure (does not
									// include the size of this
									// structure). Unknown record
									// types can be skipped easily.
	BYTE	bagName[16];			/* Length Preceeded, Null Terminated */
	BYTE	bagDescription[32];		/* Length Preceeded, Null Terminated */
	BYTE	bagZeros[20];
};

//
// Bag record header for MPK.
//

typedef	struct	_MPKBagRecordHeader
{
	UINT32	RecordType;					// Same values as that of
										// bagType field of
										// struct bagRecHeader.
	UINT32	RecordFlagsReserved;		// Don't know if it can be used.
										// Leave it alone as a precaution.
	UINT32	RecordSize;					// # of bytes of data following
										// this structure +
										// sizeof (MPKBagRecordHeader) -
										// sizeof (struct bagRecHeader)
	BYTE	RecordName[16];
	BYTE	RecordDescription[32];
	BYTE	RecordReserved[20];

	//
	// All the above fields are the same
	// as in struct bagRecHeader.
	// Following are the new fields. MPK
	// records will not have any smpRpcHeader.
	//

	UINT32	RecordInfoBitMap;
	UINT32	ApiCount;
	UINT32	RecordDataSize;				// # of bytes of data following
										// this structure.
	UINT32	Reserved;
} MPKBagRecordHeader;

//
// Used only when bagType (record type) is BAG_TYPE_SMP
//

struct smpRpcHeader		/* 32 bytes */
{
	UINT32	stamp;
	UINT32	version;
	UINT32	flags;
	UINT32	apiCount;		// # of APIs names that follow.
	UINT32	dataSize;		// Total length of data that
							// follows this structure
							// (If API name list, then
							// total length of all ASCIIZ
							// API names + an additional
							// NULL byte).
	UINT32	zeros[3];
};

struct CNBFunnelingStubStructure
{
	BYTE PushImmediateDWORD;
	LONG AddressOfFunctionBeingStubbed;
	BYTE JumpNearDisplacementRelative;
	LONG JumpDisplacement;
};

/* if this struct is changed, it must be changed in loader.inc as well */
/* changing the fields (names are not important) of this structure may require
   changes in deftoc as well so that the outputs of deftoc match */

struct ExternalPublicDefinitionStructure
{
	/* note: the first 9 fields in this structure should be identical to
	 * those in the InternalPublicDefinitionStructure.
	 */
	struct ExternalPublicDefinitionStructure *EPDLink;
	LONG EPDValue;
	BYTE *EPDName;
	LONG EPDSysCallAddress;
	LONG EPDActualAddress;
	WORD EPDExportDistance;
	BYTE EPDFlag;
	BYTE EPDType;
	struct ExternalPublicDefinitionStructure *EPDHashLink;
	struct LoadDefinitionStructure *EPDLoadRecord;
//	WORD EPDAllowedImporters;
//	WORD EPD_Available_for_Future_Use;	/* unused */

	struct CNBFunnelingStubStructure *EPDCNBFunnelingStubAddress;
	LONG EPDMPKStubSize;// BFR WHAT USE NOW?

	//
	// The # of modules that have imported this
	// symbol.
	//

	UINT	EPDReferenceCount;
};

/* if this struct is changed, it must be changed in loader.inc as well */
/* changing the fields (names are not important) of this structure may require
   changes in deftoc as well so that the outputs of deftoc match */

struct InternalPublicDefinitionStructure
{
	/* note: the first 9 fields in this structure should be identical to
	 * those in the ExternalPublicDefinitionStructure.
	 */
	struct InternalPublicDefinitionStructure *IPDLink;
	void *IPDValue;
	BYTE *IPDName;
	LONG IPDSysCallAddress;
	void *IPDActualAddress;
	WORD IPDExportDistance;
	BYTE IPDFlag;
	BYTE IPDType;
	struct InternalPublicDefinitionStructure *IPDHashLink;
};

struct LoadDefinitionStructure
{
	struct LoadDefinitionStructure *LDLink;
	struct LoadDefinitionStructure *LDKillLink;
	struct LoadDefinitionStructure *LDScanLink;
	struct ResourceTagStructure	*LDResourceList;
	LONG LDIdentificationNumber;
	LONG LDCodeImageOffset;
	LONG LDCodeImageLength;
	LONG LDDataImageOffset;
	LONG LDDataImageLength;
	LONG LDUninitializedDataLength;
	LONG LDCustomDataOffset;
	LONG LDCustomDataSize;
	LONG LDFlags;
	LONG LDType;
	LONG (*LDInitializationProcedure)
	(
		struct LoadDefinitionStructure *LoadRecord,
		struct ScreenStruct *screenID,
		BYTE *CommandLine,
		BYTE *loadDirectoryPath,
		LONG uninitializedDataLength,
		LONG fileHandle,
		LONG (*ReadRoutine)
		(
			LONG fileHandle,
			LONG offset,
			void *buffer,
			LONG numberOfBytes
		),
		LONG customDataOffset,
		LONG customDataSize,
		/*
		** The next two parameters were added in NetWare v4.11.
		** It would be preferred to get this data directly from LoadRecord,
		** They are provided here for any NLMs that still expect these
		** as parameters.
		*/
		LONG messageCount,
		BYTE **messages
	);
	void (*LDExitProcedure)
	(
		void
	);
	LONG (*LDCheckUnloadProcedure)
	(
		struct ScreenStruct *screenID
	);
	struct ExternalPublicDefinitionStructure *LDPublics;

	//
	// Name of the NLM.  This is usually the 8.3 file name, but does not have
	// to look like a file name.
	// This is a length preceeded string.  Is it NULL
	// terminated???
	//

	BYTE LDFileName[36];

	//
	// Description of the NLM.
	// This is a length preceeded string.  Is it NULL
	// terminated????
	//

	BYTE LDName[1 + LD_MODULE_DESCRIPTION_MAX_LENGTH]; /* length preceeded */
	LONG *LDCLIBLoadStructure;
	LONG *LDNLMDebugger;
	LONG LDParentID;
	LONG LDReservedForCLIB;
	void *AllocMemory;
	LONG LDTimeStamp;
	void *LDModuleObjectHandle;	/* If Instrumented BEW 10/16/90 */
	LONG LDMajorVersion;
	LONG LDMinorVersion;
	LONG LDRevision;
	LONG LDYear;
	LONG LDMonth;
	LONG LDDay;
	BYTE *LDCopyright;
	LONG LDSuppressUnloadAllocMsg;
	void *LDLibraryVM;
//	LONG Reserved3;
//	LONG Reserved4[64];
	BYTE LDFullPath[260];           // null terminated string - max size is 256
	LONG Reserved5[12];
	LONG Reserved6;
	struct DomainStructure *LDDomainID;	/* This must be non-zero for the Alloc Hunt code to work right. */
										/* It also points to the domain structure. */
	struct LoadDefinitionStructure *LDEnvLink;
	void *LDAllocPagesListHead;
	struct ExternalPublicDefinitionStructure *LDTempPublicList;
	LONG LDMessageLanguage; 	/* for enabling */
	BYTE **LDMessages;			/* for enabling */
	LONG LDMessageCount;		/* for enabling */
	BYTE *LDHelpFile;			/* for enabling */
	LONG LDMessageBufferSize;	/* for enabling */
	LONG LDHelpBufferSize;		/* for enabling */
	LONG LDSharedCodeOffset;		/* NetWare 4 protection architecture */
	LONG LDSharedCodeLength;		/* NetWare 4 protection architecture */
	LONG LDSharedDataOffset;		/* NetWare 4 protection architecture */
	LONG LDSharedDataLength;		/* NetWare 4 protection architecture */
	LONG (*LDSharedInitProcedure)	/* NetWare 4 protection architecture */
	(
		struct LoadDefinitionStructure *LoadRecord,
		struct ScreenStruct *screenID,
		BYTE *CommandLine,
		BYTE *loadDirectoryPath,
		LONG uninitializedDataLength,
		LONG fileHandle,
		LONG (*ReadRoutine)
		(
			LONG fileHandle,
			LONG offset,
			void *buffer,
			LONG numberOfBytes
		),
		LONG customDataOffset,
		LONG customDataSize,
		/*
		** The next two parameters were added in NetWare v4.11.
		** It would be preferred to get this data directly from LoadRecord,
		** They are provided here for any NLMs that still expect these
		** as parameters.
		*/
		LONG messageCount,
		BYTE **messages
	);
	void (*LDSharedExitProcedure)	/* NetWare 4 protection architecture */
	(
		void
	);
	LONG LDRPCDataTable;
	LONG LDRealRPCDataTable;
	LONG LDRPCDataTableSize;

	//
	// Number of non-server symbols imported by this NLM.
	// (Symbols imported from SERVER.NLM and LOADER.EXE are
	// excluded from this count).
	//

	LONG LDNumberOfReferencedPublics;

	//
	// Pointer to an array each slot of which contains a pointer
	// to the non-server symbol record this module imported.
	// (Note that symbols imported from SERVER.NLM and LOADER.EXE
	// don't show up in this list).
	//

	struct ExternalPublicDefinitionStructure **LDReferencedPublics;

	//
	// Number of symbols exported by this NLM which are
	// referenced by other NLMs which import them.
	//

	LONG LDNumberOfReferencedExports;
	LONG LDNICIObject; /*** GREEN RIVER CMC LOADER CHANGE ***/
	LONG LDAllocPagesListLocked; /* True if LDAllocPagesListHead is locked */
	struct AddressSpace *LDAddressSpace;

	//
	// We will use this field to distinguish between structures
	// allocated by the loader and that allocated by anyone else
	// knowing about the LoadDefinitionStructure format.  Only if
	// this Signature field is valid will be attempt to touch the
	// LDImportedKernelSymbolCount and ppLDImportedKernelSymbols
	// fields.
	//

	UINT	Signature;

	struct CNBFunnelingStubStructure *MainCNBFunnelingStubPtr;
	LONG MainCNBFunnelingStubsLeft;

	LONG LDBuildNumber;	/* NLM Build Number */
	struct LoadDefinitionExtension*LDExtensionData;

	//
	// New fields to track some useful information.
	//

	//
	// # of symbols imported from the "kernel."
	// (i.e. SERVER.NLM and LOADER.EXE).
	//

	UINT										LDImportedKernelSymbolCount;

	//
	// Pointer to an array each slot of which contains a pointer
	// to the list of kernel symbols (i.e. from SERVER.NLM and LOADER.EXE)
	// imported by this module.
	//

	struct InternalPublicDefinitionStructure	**ppLDImportedKernelSymbols;
	struct LoadTimeInfoStruct *loadTimeInfo;
	struct CNBFunnelingStubStructure *SharedCNBFunnelingStubPtr;
	LONG SharedCNBFunnelingStubsLeft;
	UINT8  LDModuleLongName[LD_MODULE_LONG_NAME_MAX_LENGTH]; // Length proceeded only....
};

struct StartInfoRecordStructure
{
	LONG	relocationCount;
	LONG	relocationOffset;
	LONG	codeOffset;
	LONG	dataOffset;
	LONG	startOffset;
	LONG	OSNLMOffset;
	LONG	MSNLMOffset;
	LONG	languageID;
	LONG	messageFileOffset;
	LONG	messageFileLength;
	LONG	tableVersionNumber;
	LONG	DSNLMOffset;
	LONG	loaderImportOffset;
	LONG	loaderImportCount;
	BYTE	LoadFileStatusString[4];
	LONG	reserved[1];
	BYTE	loaderTypeString[1];
};

struct RealModeDataStructure
{
	LONG OriginalCR0;
	LONG XMSEntry;
	LONG XMSHandle;
	LONG XMSMemoryBase;
	LONG XMSMemorySize;
	LONG LoaderStackTop;
	struct StartInfoRecordStructure *StartInfoPointer;
	LONG ParentPSP_Address;
	LONG ChildPSP_Address;
	LONG HardwareLoaderID;
	LONG FeatureFlagsEDX;
	LONG FeatureFlagsECX;
	LONG FeatureFlagsEBX;
	LONG CPUFamily;
	LONG CPUModel;
	LONG CPUStepping;
	LONG AdditionalHighMemory;
	LONG OriginalCR4;
	WORD DOSClientODIShutdownOffset;
	WORD DOSClientODIShutdownSegment;
	WORD DOSClientODIResetOffset;
	WORD DOSClientODIResetSegment;
	WORD DOSClientODIDriversShutdown;
	WORD DOSClientODIDriversReset;
	LONG Reserved;
};

struct LoadEngineNLMStructure
{
	struct LoadEngineNLMStructure *link;
	LONG processID;
	LONG status;	/* 0 - new, 1 - mine, 2 - otherEngine's, 3 - done */
	LONG returnCode;
	BYTE *nameAndCommandLine;
	LONG loadOptions;
	LONG engineNumber;
	LONG function;	/* 0 - load, 1 - is loaded, 2 - unload */
};

typedef LONG (*Netman_f)(LONG *NetManBlock);

typedef struct Netman_t
{
	Netman_f NetmanUpdate[MaximumNumberOfUpdateRoutines];
} Netman_s, *pNetman_s;

struct LoaderSymbols {
	LONG (*SetNICIEntryPoints)();
	LONG (*ClearNICIEntryPoints)();
	BYTE breakOnLoadFlag;
	BYTE *consoleSecured;
	struct ExternalPublicDefinitionStructure *ExternalPublicList;
	struct LoadDefinitionStructure **LoadedList;
	ERROR	(*AcquireLoaderLock)(void);
	ERROR	(*ReleaseLoaderLock)(void);
	struct LoadDefinitionStructure **GhostModuleList;
	struct InternalPublicDefinitionStructure **publicHashTable;
	struct ResourceTagStructure *LoaderAllocRTag;
	struct ResourceTagStructure *LoaderAllocHighRTag;
	LONG *SignalDebuggerOptions;
	LONG (*AddModuleToInternalModuleList)();
	LONG (*RemoveModuleFromInternalModuleList)();
	void (*CryptSymbolStruct)();
	LONG (*GhostCallBack)();
	LONG (*FindPublicRecordStructure)();
	LONG (*FindPublicRecord)();
	LONG (*LoadFile)();
	LONG (*AllocInitialize)();
	LONG (*AllocTerminate)();
	LONG (*UnloadLoadedFile)();
	void (*KillMe)();
	void (*KillLoadFileProcedure)();
	LONG (*LoadAllNeededModules)();
	LONG (*ProcessRelocationRecords)();
	LONG (*ProcessExternalRecords)();
	LONG (*ProcessPublicRecords)();
	LONG (*ProcessDebugRecords)();
	BYTE *(*AllocateMappedMemory)();
	LONG (*ReturnMappedMemory)();
	LONG (*ImportPublicSymbol)();
	LONG (*UnImportPublicSymbol)();
	LONG (*ExportPublicSymbol)();
	LONG (*GetNumberOfReferencedExports)();
	void *(*AllocateEnableMemory)();
	LONG (*FindAlternateMessageFile)();
	LONG (*FindAlternateHelpFile)();
	LONG (*LoadMessageAndHelpFiles)();
	void (*InitializePublicSymbols)();
	void (*SetLoadRecordID)();
	void (*AddToPublicHash)();
	void (*RemoveFromPublicHash)();
	struct LoadDefinitionStructure *(*ValidateModuleHandle)();
	LONG (*SetLoadBreakpoint)();
	struct ProcessorStructure *(*GetProcessorID)();
	void (*ReCalibrateUtilization)(void);
	void (*CRemoveDebuggerSymbol)();
	void (*ReturnAllLeftOverResources)();
	pNetman_s NetMan;
	LONG *NLMsLoaded;
};

struct LoaderExtensionStructure {
	struct LoaderExtensionStructure*LDXNext;
	struct ResourceTagStructure*LDXResourceTag;
	LONG(*LDXHandler)(struct ScreenStruct*screen,
		LONG hFile,
		LONG(*readRoutine)(LONG, LONG, void*, LONG),
		LONG options,
		BYTE*moduleName,
		BYTE*loadPath,
		BYTE*commandLine,
		struct AddressSpace*addressSpace);
		struct KernelUserAddressPair *LDXReadRoutines;
};

struct LoaderSectionInformation {
	void*LDSAddress;
	LONG LDSSize;
	BYTE LDSName[8];
};

struct KernelUserAddressPair {
	void*APKernel;
	void*APUser;
};

/* - each loader extension should start its private data (if
 *   any) with this structure; further fields are up to the
 *   extension
 * - the information is targeted mainly to aid debuggers
 * - LDESignature should uniquely identify the type of extension
 *   in case some other component needs specific image type
 *   information (generally this would be the image's signature)
 * - LDESectionCount and LDESections may be set to zero if they
 *   do not apply and the according (correct) information is
 *   stored in LoadDefinitionStructure
 */
struct LoadDefinitionExtension {
	LONG LDESignature;
	LONG LDESectionCount;
	struct LoaderSectionInformation*LDESections;
};

/*********** remote declarations not found in an included header ***********/

extern struct InternalPublicDefinitionStructure *InternalPublicList;
extern struct InternalPublicDefinitionStructure *InternalOldPublicList;

extern struct ExternalPublicDefinitionStructure *ExternalPublicList;

extern LONG OSDataLinearAddress;
extern LONG OSCodeLinearAddress;

extern WORD screenType;

extern LONG (*NetManUpdate[MaximumNumberOfUpdateRoutines])(LONG *NetManBlock);

extern void CNBFunneler();
extern void UserCNBFunneler();

extern BYTE *GetMappedMemory(LONG	imageType,
			LONG	size,
			LONG	SleepOKFlag,
			LONG	*SleptFlag);

extern LONG ExportUserModeDataSymbol(
			LONG moduleHandle,
			BYTE *symbolName,
			LONG symbolAddress);

extern LONG ExportUserModePublicSymbol(
			LONG moduleHandle,
			BYTE *symbolName,
			LONG syscallStubAddress);

extern LONG ReleaseMappedMemory(BYTE	*address,
			LONG	size);

extern struct RealModeDataStructure *RealModeDataTable;
extern struct RealModeDataStructure RMDTable;

extern struct ResourceTagStructure *LoaderAllocHighRTag;

/************************** function definitions ***************************/

#define FILL_OUT_CNB_FUNNEL_STUB(s, c, t)		\
{												\
s->PushImmediateDWORD =					\
	0x68; 									/* push immediate dword opcode */			\
s->AddressOfFunctionBeingStubbed =	\
	(LONG)(c); 								/* address of function to call */			\
s->JumpNearDisplacementRelative =	\
	0xE9; 									/* jmp near, displ. relative opcode */		\
s->JumpDisplacement =					\
	(LONG)(t) -								/* target address */								\
	((LONG)&(s)->JumpDisplacement) -	/* address that is just after */				\
	sizeof((s)->JumpDisplacement);	/* this instruction */							\
}


#endif /* __LOADER_H__ */
