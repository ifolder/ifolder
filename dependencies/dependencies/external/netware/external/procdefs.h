#ifndef __PROCDEFS_H__
#define __PROCDEFS_H__


/*****************************************************************************
 *
 *	(C) Copyright 1988 - 1996 Novell, Inc.
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
 *  $Workfile:   procdefs.h  $
 *  $Modtime:   12 Feb 2002 14:58:48  $
 *  $Revision$
 *
 ****************************************************************************/

/*********************************************************************************
 * Program Name:  NetWare 386
 *
 * Filename:	  procdefs.h
 *
 * Date Created:  May 20, 1988
 *
 * Version:		  1.0
 *
 * Programmers:   Howard Davis
 *
 * Files used:
 *
 * Date Modified:
 *
 * Modifications:
 *
 * Comments:	  This file contains function call prototypes for C
 *				  callable functions.
 *
 ****************************************************************************/

/*lint -save -e762 */

#define LONG				unsigned long
#define WORD				unsigned short
#define BYTE				unsigned char
#define NULL				0

#define TRUE				1
#define FALSE				0

#define DIM_0_OR_1			1

#define READONLY			1

#define JUST_EXIT			0
#define RESTART_EXIT		1

#define TypeNormalServer	0
#define TypeIOEngine		1
#define TypeOSEngine		2

/****************************************************************************/
/* Forward declare structures for the Watcom compiler */

struct AddressSpace;
struct AESProcessStructure;
struct AdapterOptionDefinitionStructure;
struct AuditServiceStructure;
struct AuditServiceControlStructure;
struct bitmapstruct;
struct BroadcastMessageStructure;
struct BufferStruct;
struct CacheWriteCallBackStructure;
struct commandParserStructure;
struct commandParserInternalStructure;
struct CurrentRate;
struct DebuggerStructure;
struct DirectoryHandleStructure;
struct DirectoryInfoReturn;
struct DirectoryStructure;
struct diskrequest;
struct DomainStructure;
struct DOSCountryInfoStructure;
struct DriverConfigurationStructure;
struct eventdef;
struct ExternalPublicDefinitionStructure;
struct FATStructure;
struct FileMapStructure;
struct FileListNode;
struct FLockNode;
struct GetStructure;
struct InfoSourceEntry;
struct InternalPublicDefinitionStructure;
struct IOConfigurationStructure;
struct LANConfigurationLimitStructure;
struct LoadDefinitionStructure;
struct LoadFileHeaderStructure;
struct MacFileHandleMapStruct;
struct MacInfoStruct;
struct MacOpenStruct;
struct MacSearchStruct;
struct MacSetStruct;
/*struct mapstruct; directfs */
struct MemoryStructure;
struct MirrorInfoStruct;
struct ModifyStructure;
struct NameServiceStructure;
struct NameServiceControlStructure;
struct NCPDirectoryHandleStructure;
struct NCPExtensionStructure;
struct NetListEntry;
struct ObjectStructure;
struct OldQEntryStruct;
struct ParityInfoStruct;
struct PCBStructure;
struct WorkToDoStructure;	//lint !e763
struct PListNode;
struct PRecNode;
struct ProblemListHeaderStructure;
struct ProcessorStructure;
struct PropertyStructure;
struct RecListNode;
struct ResourceDefinitionStructure;
struct RoutePacketStruct;
struct QEntryStruct;
struct QStruct;
struct RedirectionInfoStruct;
struct ReplyProceduresStructure;
struct RequestPacketStructure;
struct resourcetagdef;
struct ResourceTagStructure;
struct RLockNode;
struct RootNode;
struct rstruct;
struct ScreenCleanUpRequestStructure;
//lint -save -e763
struct ScreenStruct;			
//lint -restore
struct SearchMapStructure;
struct SearchStructure;
struct SemaphoreStructure;
struct SemListNode;
struct ServerPacketStruct;
struct SetableParametersStructure;
struct shortioctl;
struct shortsystem;
struct SpaceStructure;
struct StackFrame;
struct SubdirectoryStructure;
struct SystemInfoStruct;
struct TimerDataStructure;
struct TransListNode;
struct TrusteeListStructure;
struct ValueStructure;
struct volinfodef;
struct VolumeInformationStructure;
struct volumerequestdef;
struct VolumeStruct;
struct VolumeStructure;
struct LockWaitStructure;
struct EnumStructure;
struct qInfoStructure;
struct DriverStatusType;
struct RegisterRouterType;
struct RegisterSAPType;
struct NetEntryType;
struct RouteEntryType;
struct ServerEntryType;
struct ServerSourceEntryType;
struct SAPFilterType;
struct SAPControlAdaptorType;
struct RIPControlAdaptorType;
struct ClientHandleStruct;
struct networkAddressInfo;
struct networkAddressStruct;

/****************************************************************************/

/* Include other files that define structures that are passed */

#include <version.h>
#include <screen.h>
#include <dstruct.h>
#include <resource.h>
#include <timer.h>
#include <replypro.h>
#include <ecb.h>
#include <function.h>
#include <msstruc.h>
#include <synclock.h>
#include <loader.h>


/* Strings Tagged? */
#ifndef MsgTagNLM
#define MsgTagNLM 0
#endif

#ifdef MsgTag
#define MSG(s,id) OSMessageTable[id]
#define InxMSG(s,id) id
#define TxtMSG(s,id) s
#elif MsgTagNLM		
extern BYTE **NLMMessageTable;
#define MSG(s,id) NLMMessageTable[id]
#define InxMSG(s,id) id
#define TxtMSG(s,id) s
#else
#define MSG(s, id) (BYTE *)(s)
#define InxMSG(s,id) id
#define TxtMSG(s,id) s
#endif


/****************************************************************************/
/****************************************************************************/
/****************************************************************************/
/* define the volume types */

#define VINetWare386			0
#define VINetWare286			1
#define VINetWare386V30 	2
#define VINetWare386V31 	3

/* define the StatusFlagBits */

#define VISubAllocEnabledBit			0x1
#define VICompressionEnabledBit 		0x2
#define VIMigrationEnabledBit			0x4
#define VIAuditingEnabledbit			0x08
#define VIReadOnlyEnabledBit			0x10
#define VIImmediatePurgeBit				0x20
#define VI64BitFileOffsetsEnabledBit	0x40
#define VINSSVolumeBit					0x80000000

struct	VolInfoStructure
{
	LONG	VolumeType;
	LONG	StatusFlagBits;
	LONG	SectorSize;
	LONG	SectorsPerCluster;
	LONG	VolumeSizeInClusters;
	LONG	FreedClusters;
	LONG	SubAllocFreeableClusters;
	LONG	FreeableLimboSectors;
	LONG	NonFreeableLimboSectors;
	LONG	NonFreeableAvailableSubAllocSectors;
	LONG	NotUsableSubAllocSectors;
	LONG	SubAllocClusters;
	LONG	DataStreamsCount;
	LONG	LimboDataStreamsCount;
	LONG	OldestDeletedFileAgeInTicks;
	LONG	CompressedDataStreamsCount;
	LONG	CompressedLimboDataStreamsCount;
	LONG	UnCompressableDataStreamsCount;
	LONG	PreCompressedSectors;
	LONG	CompressedSectors;
	LONG	MigratedFiles;
	LONG	MigratedSectors;
	LONG	ClustersUsedByFAT;
	LONG	ClustersUsedByDirectories;
	LONG	ClustersUsedByExtendedDirectories;
	LONG	TotalDirectoryEntries;
	LONG	UnUsedDirectoryEntries;
	LONG	TotalExtendedDirectoryExtants;
	LONG	UnUsedExtendedDirectoryExtants;
	LONG	ExtendedAttributesDefined;
	LONG	ExtendedAttributeExtantsUsed;
	LONG	DirectoryServicesObjectID;
	LONG	VolumeLastModifiedDateAndTime;
};

/****************************************************************************/

struct FATStructure
{
	LONG FATValue;
	LONG FATLink;
};

/****************************************************************************/
/* External variable declarations */

extern BYTE DeveloperFlag;

#if READONLY
extern BYTE VolumeReadOnlyFlag[];
#endif

extern BYTE ForceError; 	/* Debug */
extern LONG RunningProcess;
extern BYTE *FileServerName;

extern BYTE NoMapTable[];

/*- registered devices -*/
extern struct disk *DiskList;

extern BYTE FileServerMajorVersionNumber, FileServerMinorVersionNumber;
extern BYTE FileServerRevisionNumber;
extern BYTE SFTLevel, TTSLevel, VAPVersionNumber;
extern BYTE QueueingVersionNumber, VirtualConsoleVersionNumber;
extern BYTE SecurityRestrictionsLevel;

extern LONG NCPBoundCheckFlag, NCPCompCheckFlag;
extern LONG NCPBoundWarningFlag, NCPCompWarningFlag;
extern LONG NCPBoundCheckFailed, NCPCompCheckFailed;

extern BYTE ActiveFileToFileBits[], StopFileToFileBits[];

extern BYTE CurrentYear, CurrentMonth, CurrentDay;
extern BYTE CurrentHour, CurrentMinute, CurrentSecond, CurrentWeekDay;
extern LONG CurrentTime;
extern LONG TimeInMinutes;
extern BYTE DaysInMonthTable[];
extern LONG DOSDateAndTime;
extern WORD DOSDate;
extern int SecondsRelativeYear2000;

extern BYTE BinderyOpen;

extern BYTE **OSMessages;
extern BYTE *OSMessageTable[];

extern UINT32 SystemVerboseFlag;

/* DOS flavor tables */
extern LONG DOSValidCharBitMap[];
extern LONG DOSValidCharBitMapNoLower[];
extern LONG DOSFirstByteBitMap[];
extern LONG DOSSecondByteBitMap[];
extern BYTE DOSUpperCaseTable[];
extern LONG DOSType;
extern BYTE MACToDOS[];
extern BYTE DOSToMAC[];

/* OS Locale Tables */
extern LONG OSDoubleByteSpace;
extern BYTE OSUpperCaseTable[];
extern BYTE OSSortTable[];
extern BYTE OSLineDrawCharTable[];
extern LONG OSFirstByteBitMap[];
extern LONG OSLanguageID;
extern LONG OSDoubleBytePresentFlag;


/* Resource tags for internal OS use */

extern struct ResourceTagStructure *OSShortTermAllocTag;
extern struct ResourceTagStructure *OSGeneralWorkAllocTag;
extern struct ResourceTagStructure *OSSemaphoreRTag;
extern struct ResourceTagStructure *OSMiscNonMovableCacheRTag;
extern struct ResourceTagStructure *OSAESProcessRTag;
extern struct ResourceTagStructure *OSWorkToDoRTag;
extern struct ResourceTagStructure *OSMiscAllocMemoryRTag;
extern struct ResourceTagStructure *OSStackAllocMemoryRTag;
extern struct ResourceTagStructure *OSTimerResourceTag;
extern struct ResourceTagStructure *OSSetableParameterRTag;
extern struct ResourceTagStructure *OSSPCategoryRTag;
extern struct ResourceTagStructure *OSEventResourceTag;

/****************************************************************************/
/* Routines in the AESPROC module */

void InitializeAESProcess(void);

void NoSleepAESProcess(void);

void ScheduleNoSleepAESProcessEvent(
		struct AESProcessStructure *EventNode);

void CancelNoSleepAESProcessEvent(
		struct AESProcessStructure *EventNode);

void SleepAESProcess(void);

void ScheduleSleepAESProcessEvent(
		struct AESProcessStructure *EventNode);

void CancelSleepAESProcessEvent(
		struct AESProcessStructure *EventNode);

void AESProcessCleanupProcedure(
		struct ResourceTagStructure *RTag);

/* End of routines in the AESPROC module */
/****************************************************************************/
/* Routines in the ALERT module */

LONG	RenameLogFile(
			LONG Volume,
			LONG PathBase,
			BYTE *PathString,
			LONG PathCount,
			LONG *Handle,
			LONG *EntryNumber,
			struct DirectoryStructure **DirectoryEntry,
			LONG *action,
            BYTE *renameLogFileName);


void INWSystemAlert(
		LONG alertIdCode,
		LONG targetStation,
		LONG targetNotificationBits,
		LONG alertSeverity,
		void *alertDataPtr,
		void (*alertDataFree)(
				void *alertDataPtr),
		void *controlString,
		...);

void INWQueueSystemAlert(
		LONG alertIdCode,
		LONG targetStation,
		LONG targetNotificationBits,
		LONG alertSeverity,
		void *alertDataPtr,
		void (*alertDataFree)(
				void *alertDataPtr),
		void *controlString,
		...);

void SystemAlert(
		LONG TargetStation,
		LONG TargetNotificationBits,
		LONG ErrorLocus,
		LONG ErrorClass,
		LONG ErrorCode,
		LONG ErrorSeverity,
		void *controlString,
		...);

LONG QueueSystemAlert(
		LONG TargetStation,
		LONG TargetNotificationBits,
		LONG ErrorLocus,
		LONG ErrorClass,
		LONG ErrorCode,
		LONG ErrorSeverity,
		void *controlString,
		...);

void InitializeSystemAlerts(void);

/* End of routines in the ALERT module */
/****************************************************************************/
/* Routines in the CACHE module */

void InitializeCache(void);

LONG AddVolume(
		LONG volumeNumber);

LONG DeleteVolume(
		LONG volumeNumber);

LONG DiskGet(
		LONG drive,
		LONG sectorNumber,
		BYTE *bufferAddress,
		LONG *releaseReturnHandle);

LONG MDiskGet(
		LONG drive,
		LONG sectorNumber,
		LONG numberOfSectors,
		BYTE **bufferAddress,
		LONG *releaseReturnHandle);

LONG DiskRead(
		LONG drive,
		LONG sectorNumber,
		BYTE *bufferAddress);

LONG DiskGetNoRead(
		LONG drive,
		LONG sectorNumber,
		BYTE **bufferAddress,
		LONG *releaseReturnHandle);

LONG DiskGetNoReadNoWait(
		LONG drive,
		LONG sectorNumber,
		BYTE **bufferAddress,
		LONG *releaseReturnHandle);

LONG DiskGetNoReadNoWaitNoAlloc(
		LONG drive,
		LONG sectorNumber,
		BYTE **bufferAddress,
		LONG *releaseReturnHandle);

void DiskWrite(
		LONG drive,
		LONG sectorNumber,
		LONG numberOfSectors,
		BYTE *bufferAddress,
		LONG fileData);

void DiskRelease(
		LONG releaseReturnHandle);

void DiskUpdate(
		LONG releaseReturnHandle,
		LONG sectorNumber,
		LONG numberOfSectors,
		LONG *fileData);

void CallBackDiskUpdate(
		LONG releaseReturnHandle,
		LONG sectorNumber,
		LONG numberOfSectors,
		LONG *fileData,
		void *CallBackControlNode);

LONG CallBackWhenBlockWritten(
		LONG Volume,
		LONG SectorNumber,
		struct CacheWriteCallBackStructure *CallBackNode);

LONG CallBackWhenWritesComplete(
		void (*CallBackProcedure)(),
		void *CallBackParameter,
		LONG DirectoryNumber,
		LONG *CallBackCount,
		LONG Station);

LONG GetSectorFromFileCache(
		LONG Volume,
		LONG Sector,
		void *Buffer);

void FlushCache(
		LONG drive);

void CheckAndFlushCache(
		LONG Volume,
		LONG FirstSector,
		LONG NumberOfSectors);

void CacheUpdateProcess(void);

void CheckForLostWaitNodes(void);

void *AllocMovableCacheMemory(
		LONG size,
		void (*MoveRoutine)(void),
		LONG moveParameter,
		LONG *actualSize,
		struct ResourceTagStructure *RTag);

LONG ExpandMovableCacheMemory(
		void **memoryAddress,
		LONG additionalSize);

void ShrinkMovableCacheMemory(
		void **memoryAddress,
		LONG additionalSize);

void FreeMovableCacheMemory(
		void **memoryAddress);

void StuffValue(void);	/* Must be called from assembly */

LONG ReturnMovableCacheMemorySize(
		void **memoryAddress);

void *AllocNonMovableCacheMemory(
		LONG size,
		LONG *actualSize,
		struct ResourceTagStructure *RTag);	/* CacheNonMovableMemorySignature */

extern void *AllocNonMovableCacheNoSleep(
		LONG nodeTotal, LONG * actualSize,
		struct ResourceTagStructure *CDNonMoveMemoryResourceTag);

void FreeNonMovableCacheMemory(
		void *memoryAddress);

void *AllocBufferBelow16Meg(
		LONG requestedSize,
		LONG *actualSize,
		struct ResourceTagStructure *RTag);

void FreeBufferBelow16Meg(
		void *buffer);

/* End of routines in the CACHE module */
/****************************************************************************/
/* Routines in the CACHEMEM module */

LONG GetSectorsPerCacheBuffer(void);

LONG GetCacheBufferSize(void);

LONG GetCurrReturnableCacheBuffersCount(void);

LONG GetCurrNonMovableCacheBuffersCount(void);

LONG GetNDirtyBlocks(void);

LONG GetOriginalNumberOfCacheBuffers(void);

LONG GetCurrentNumberOfCacheBuffers(void);

LONG GetNumberOfDirectoryCacheBuffers(void);

LONG GetCacheCounters(
		LONG typeOfCacheCounters,
		BYTE *buffer);

void *AllocateMappedPages(
			LONG	NumberOf4KPages,
			LONG	SleepOKFlag,
			LONG	Below16MegFlag,
			struct ResourceTagStructure	*RTag,
			LONG	*SleptFlag);

void DeAllocateMappedPages(void *Memory);

void CGiveMemoryToCache(
		void *Memory,
		LONG MemoryLength,
		LONG ControlFlags);

LONG GetOtherCachePagesCount(void);

/* End of routines in the CACHEMEM module */
/****************************************************************************/
/* Routines in the CACHE2FS module */

void StopWrite(
		LONG stationNumber);

/* End of routines in the CACHE2FS module */
/****************************************************************************/
/* Routines in the CDEBUG module */

extern void DebugCommandParserCleanup(
	struct ResourceTagStructure	*RTag);

extern LONG RegisterDebugCommandParser(
		LONG (*parseRoutine)(
				struct ScreenStruct *debugScreen,
				BYTE *commandLine,
				struct StackFrame *stackFrame),
		struct ResourceTagStructure *resourceTag);

extern LONG SetLoadBreakpoint(
			LONG address,
			struct ProcessorStructure *processor);

extern LONG UnRegisterDebugCommandParser(
		LONG (*parseRoutine)(
				struct ScreenStruct *debugScreen,
				BYTE *commandLine,
				struct StackFrame *stackFrame));

/* End of routines in the CDEBUG module */
/****************************************************************************/
/* Routines in the COMMAND module */
/* temp. hook for pre processor */

void DisplayCurrentTime(
		struct ScreenStruct *screenID,
		BYTE	*messageString);

void ConvertToUpperCase(
		BYTE *string);

void LStrToUpperCase(
		BYTE *string);

void ConvertTokenToUpperCase(
		BYTE *string,
		BYTE *delimeterCharacters,
		int numberOfDelimeters);

LONG StopServer(
		struct ScreenStruct *screenID,
		BYTE forceDown,
		LONG station,
		void (*EarlyReplyProcedure)( /* ReplyKeepNoFragmentsWithStation(station, ccode, task) */
				LONG station,
				LONG completionCode,
				LONG task),
		BYTE *alternateMessage,
		LONG task);

extern LONG InternalStopServer(
		struct ScreenStruct *screenID,
		BYTE forceDown,
		LONG station,
		void (*EarlyReplyProcedure)( /* ReplyKeepNoFragmentsWithStation(station, ccode, task) */
			LONG station,
			LONG completionCode,
			LONG task),
		BYTE *alternateMessage,
		LONG task);

LONG ClearStation(
		struct ScreenStruct *screenID,
		BYTE *commandLine);

LONG SetFileServerTime(
		BYTE *string);

LONG SetTimeFromVector(
		LONG stationNumber,
//;;BEGIN SPD 140859, 152578 - RECALCFX.NLM Change 1 of 9 - TDR 5/23/97
		BYTE *timeVector,//);
		LONG reCalcDST);
//;;END SPD 140859, 152578 - RECALCFX.NLM Change 1 of 9 - TDR 5/23/97

LONG ExecuteCommandFile(
		struct ScreenStruct *screenID,
		BYTE *commandLine);

LONG ExtractTimeFromString(BYTE *commandLine, BYTE *timeVector);
LONG ExtractMonthFromString(BYTE *commandLine, BYTE *timeVector);
LONG ExtractUnsignedByteFromString(BYTE *commandLine, BYTE *value);

void PromptUserForAnyMissingInformation(void);

void PromptUserForFileServerName(void);

LONG SetFileServerName(
		BYTE *newFileServerName, LONG updateFlag);

void PromptUserForIPXInternalNetNumber(void);

LONG SetInternalNetNumber(
		BYTE *commandLine);

extern LONG ProcessSearchPathCommand(
		LONG function,
		struct ScreenStruct *screenID,
		BYTE *commandLine,
		BYTE *upperBuff,
		LONG callerSuppliedToken);

LONG	AddNameSpaceToVolume(
		struct ScreenStruct *screenID,
		BYTE *commandLine,
		LONG pauseFlag,
		LONG SecondsToPause);

/* Defect 265394 - RDoxey*/
//void CheckForTTSVolumeMount(
//		LONG volumeNumber);

LONG ClearSupervisorAccountLockout(void);

LONG DefineAdditionalMemory(
		BYTE *commandLine);

LONG CheckVolumesForDown(
		void (*OutputRoutine)(
				void *controlString,
				...),
		LONG parameter,
      LONG userparameter);

void DownBothFileServerEngines(void);

LONG ProcessSetTimeZone(
		struct ScreenStruct *screenID,
		BYTE *commandLine,
		BYTE *unused,
		LONG token);

/* End of routines in the COMMAND module */
/****************************************************************************/
/* BEGIN routines for cmdproc module */

void ConsoleAudit(BYTE *command, LONG completionCode);

void ChangeLoggedInConsoleUser(LONG userID);

LONG ReadLoggedInConsoleUser();


extern void InitializeCommandLineRTags(void);

extern LONG (*preParseRoutine)(struct ScreenStruct *, BYTE *, BYTE *);

void CommandLineProcess(void);

void ParseCommand(
		BYTE *commandBuffer);

LONG ExecuteCommand(
		LONG commandNumber,
		BYTE *commandLine);

extern int GetConsoleSecuredFlag(void);

extern LONG NWParseCommand(
				struct ScreenStruct *screenID,
				BYTE *cmdBuff);

extern LONG NWParseCommandEx(
					struct ScreenStruct *screenID,
					BYTE *cmdBuff,
					int flags);

LONG RegisterConsoleCommand(
		struct commandParserStructure *newCommandParser);

LONG RegisterAuditedConsoleCommand(LONG position,
		struct commandParserInternalStructure *newCommandParser);

LONG UnRegisterConsoleCommand(
		struct commandParserStructure *commandParserToDelete);

extern LONG RegisterCommand(
		LONG moduleHandle,
		struct ResourceTagStructure *RTag,
		LONG keywordFlags,
		BYTE *ptrKeyword,
		LONG commandHandlerFlags,
		LONG insertionFlags,
		LONG (*commandHandlerRoutine)(
				LONG function,
				struct ScreenStruct *screen,
				BYTE *ptrCommandLine,
				BYTE *ptrUprCaseCommandLine,
				LONG callerSuppliedData),
		LONG callerSuppliedData);

extern LONG DeRegisterCommand(
		LONG moduleHandle,
		struct ResourceTagStructure *RTag,
		LONG keywordFlags,
		BYTE *ptrKeyword);

extern LONG AddAlias(
			LONG function,
			struct ScreenStruct *screen,
			BYTE *cmdLine,
			BYTE *unused,
			LONG token);


/* End of routines in the cmdproc module */
/****************************************************************************/
/* BEGIN routines for compression/decompression */

LONG RegisterCompressionAlgorithm( LONG algorithmStructure ,
	struct ResourceTagStructure *rTag );

LONG GetCurrentCompressionStatus( LONG Volume, LONG compressStatus );

LONG GetCurrentDecompressionStatus( LONG Volume,
	LONG decompressStatus, LONG maxCount, LONG *retCount );

LONG GetCompressionTimeAndCounts( LONG volume,
	LONG *highHighTicks, LONG *lowHighTicks,
	LONG *highBytesInCount, LONG *bytesInCount ,
	LONG *highBytesOutCount, LONG *bytesOutCount );

LONG GetDecompressionTimeAndCounts( LONG volume,
	LONG *highHighTicks, LONG *lowHighTicks,
	LONG *highBytesInCount, LONG *bytesInCount ,
	LONG *highBytesOutCount, LONG *bytesOutCount );

LONG UnRegisterCompressionAlgorithm( LONG algorithmStructure );

LONG RegisterDecompressionAlgorithm( LONG algorithm,
	struct ResourceTagStructure *rTag );

LONG UnRegisterDecompressionAlgorithm ( LONG algorithm );

void CCDDecompressIsDone( LONG outHandle, LONG completionCode, BYTE *dosName );

void CCDUpdateDecompressPosition( LONG writeHandle, LONG fileOffset );

LONG CCDDecompressBuildFile( LONG outHandle, LONG offset, LONG size,
	LONG prevSize, LONG nextSize );

LONG CCDSetFileSize( LONG FileHandle, LONG RealFileSize,LONG InternalFileSize);

LONG CCDReturnWriteCacheBlock( LONG CacheHandle ,	LONG InternalFileHandle,
		LONG NumberOfSectors, 	LONG NoFlushFlag );

LONG CCDReturnReadCacheBlock(	LONG CacheHandle );

LONG CCDGetWriteCacheBlock( LONG FileHandle, LONG CacheBlockNumber,
		LONG NoFlushFlag, BYTE **CacheBlock, LONG *CacheHandle );

LONG CCDGetReadCacheBlock( LONG FileHandle,	LONG CacheBlockNumber,
		LONG *RealBytesRead, BYTE **CacheBlock,	LONG *CacheHandle,
		LONG *HoleFlag );

LONG CCDStartReadAhead( LONG FileHandle,  LONG FileSize,
	LONG ReadCacheHandleReturn, LONG readAheadWindow );

LONG CCDStopReadAhead( LONG CacheHandle );

LONG CCDFreeReadAheadBuffer( LONG readCacheHandle );

LONG CCDGetReadAheadBuffer(	LONG readCache,	BYTE **cachePointer,
	LONG *validBytes, 	LONG *holeFlag );

/*  assembly versions of the above two  */

LONG CDFreeReadAheadBuffer( LONG readCacheHandle );

LONG CDGetReadAheadBuffer(	LONG readCache,	BYTE **cachePointer,
	LONG *validBytes, 	LONG *holeFlag );

/* END routines for compression/decompression */
/****************************************************************************/
/* Routines in the COREDUMP module */

void DoCoreDump(
		struct ScreenStruct *screenID,
		struct StackFrame *stackFrame);

int RegisterOSCoredumpHandler(
		struct LoadDefinitionStructure *moduleHandle,
		int (*coredumpRoutine)(),
		UINT32 *osDebugInfoVersionID,
		int (**getOSDebugInfoRoutinePtr)());

/* End of routines in the COREDUMP module */
/****************************************************************************/
/* Routines in the CPRIMS module */

BYTE ConvertToHexDigit(
		BYTE nibble);

LONG ClaimUnClaimedMemory(
		void *Offset,
		LONG Length,
		LONG Flags);

LONG CheckIfMemoryClaimed(
		void *Offset,
		LONG Length,
		LONG Flags);

/* End of routines in the CPRIMS module */
/****************************************************************************/
/* Routines in the CPROTECT module */

void *CreateBranchEntry(struct InternalPublicDefinitionStructure *symbol
		       ,struct ResourceTagStructure *RTag );
void *CreateBranchEntryByAddress(void *symbolAddress
		       ,struct ResourceTagStructure *RTag );
LONG DeleteBranchEntry(struct InternalPublicDefinitionStructure *symbol );
LONG ModifyGlobalBranchEntry(struct ResourceTagStructure *RTag
			    ,void *symbol_IPDValue
			    ,void *branchRoutine
			    ,void **actualRoutine );
LONG RestoreGlobalBranchEntry(void *symbol_IPDValue );

/* End of routines in the CPROTECT module */
/****************************************************************************/
/* Routines in the DEBUG module */

void EnterDebugger(void);

LONG SignalDebuggerEvent(
		LONG ExceptionNumber,
		LONG ExceptionErrorCode);

void SignalExceptionEvent(
		LONG  ExceptionNumber,
		LONG  ExceptionErrorCode,
		BYTE *ExceptionMessage);

LONG RegisterDebuggerRTag(
		struct DebuggerStructure *alternateDebugger,
		int position);

LONG UnRegisterDebugger(
		struct DebuggerStructure *alternateDebugger);

LONG CSetABreakpoint(
		LONG breakpoint,
		LONG breakAddress,
		BYTE breakType,
		BYTE breakSize);

LONG CSetMPBreakpoint(
		LONG breakpoint,
		LONG breakAddress,
		BYTE breakType,
		BYTE breakSize,
		LONG  engine);

LONG ReserveMPBreakpointRTag(
		struct ResourceTagStructure *resourceTag, LONG engine);

LONG UnReserveMPBreakpoint(
		LONG breakpoint, LONG engine);

void IncDebuggerActiveCount(void);

void DecDebuggerActiveCount(void);

LONG GetDebuggerActiveCount(void);

/* End of routines in the DEBUG module */
/****************************************************************************/
/* Routines in the DEBUGDOT module */

void ProcessDebugDotCommand(BYTE *cmd, struct StackFrame *stackFrame );

void DisplayScreensAndAddresses(void);

void DisplayScreen(
		struct ScreenStruct *screenID);

/* End of routines in the DEBUGDOT module */
/****************************************************************************/
/* Routines in the ENABLE module */

LONG CaseInsensitiveCompareStrings(
		BYTE *String1,
		BYTE *String2);

void FormatCurrentDateAndTime(
		BYTE *buffer,
		BYTE flags);			/* Flags defined in Enable.H */

void FormatCurrentDate(
		BYTE *buffer,
		LONG flags);			/* Flags defined in Enable.H */

void FormatCurrentTime(
		BYTE *buffer,
		LONG flags);			/* Flags defined in Enable.H */

void FormatDateAndTime(
		BYTE *buffer,
		LONG year,
		LONG month,
		LONG day,
		LONG hour,
		LONG minute,
		LONG second,
		LONG flags);			/* Flags defined in Enable.H */

void FormatDate(
		BYTE *buffer,
		LONG year,
		LONG month,
		LONG day,
		LONG flags);			/* Flags defined in Enable.H */

void FormatTime(
		BYTE *buffer,
		LONG hour,
		LONG minute,
		LONG second,
		LONG flags);			/* Flags defined in Enable.H */

LONG ReturnOSLanguageID(void);

LONG ReturnOSLanguageName(
		LONG	languageID,
		BYTE	*languageName);

LONG SetOSLanguageID(LONG newLanguageID);

void OSConvertStringToUpperCase(
		BYTE *string,
		LONG length);

LONG OSIsDoubleByteCharacter(
		BYTE c);

LONG GetOSDoubleByteSpace(void);

LONG OSIsDoubleByteUsed(void);

int OSCompareStrings(
		BYTE *string1,
		LONG length1,
		BYTE *string2,
		LONG length2);

BYTE *OSStrRChr(
		BYTE *string,
		LONG searchChar);

BYTE *OSGetChar(
		BYTE *string,
		LONG *ch);

BYTE *OSPutChar(
		BYTE *string,
		LONG ch);

BYTE *OSStrChr(
		BYTE *string,
		LONG searchChar);

LONG OSStrCSpn(
		BYTE *string,
		BYTE *list);

void OSStrNCat(
		BYTE *orig,
		BYTE *addon,
		LONG maxlen);

BYTE *OSStrPBrk(
		BYTE *string,
		BYTE *list);

int AsciiToInt(
		BYTE *data);

int AsciiToIntWithPointer(
		BYTE **data);


void OSGetCountryInfo(
		void *buffer);

LONG OSGetCodePage(void);

void OSGetDBCSVector(
		BYTE *buffer,
		LONG size);

void OSGetCollationTable(
		BYTE *buffer,
		LONG size);

LONG DoLanguageCommand(
		LONG funciton,
		struct ScreenStruct *screenID,
		BYTE *commandLine,
		BYTE *unused,
		LONG token);

LONG IsDoubleByteSpace(
		LONG value);

int AddNewLanguage(
		int show_errors,	/* boolean flag, TRUE means report errors */
		struct ScreenStruct *screenID,	/* screen to use if show_errors is true */
		int validateID, 	/* boolean flag, TRUE means validate ID (false is for predefined ID calls) */
		int addLanguage,	/* boolean flag, TRUE means add language, FALSE means rename existing language */
		LONG newLanguageID,
		BYTE *newLangName);

LONG	ExtractFormattedDate(
		BYTE	**buffer,		/* Input string 						*/
		LONG	*year,
		LONG	*month,
		LONG	*day);

LONG	ExtractFormattedTime(
		BYTE	**buffer,		/* Input string 						*/
		LONG	*hour,
		LONG	*minute,
		LONG	*second);

LONG	ExtractFormattedDateAndTime(
		BYTE	*buffer,		/* Input string 						*/
		LONG	*year,
		LONG	*month,
		LONG	*day,
		LONG	*hour,
		LONG	*minute,
		LONG	*second);

/* End of routines in the ENABLE module */
/****************************************************************************/
/* Routines in the EVENT module */

LONG InitializeEventNotification(void);

LONG ReturnNumberOfRemoteEvents(void);


LONG RegisterForEventNotification(
		struct ResourceTagStructure *resourceTag,
		LONG eventType,
		LONG priority,
		LONG (*warnProcedure)(
				void (*OutputRoutine)(
						void *controlString,
						...),
				LONG parameter,
				LONG userParameter),
		void (*reportProcedure)(
				LONG parameter,
				LONG userParameter),
		LONG userParameter);

LONG UnRegisterEventNotification(
		LONG eventID);

LONG EventCheck(
		LONG type,
		void (*OutputRoutine)(
				void *controlString,
				...),
		LONG parameter);

LONG EventReport(
		LONG type,
		LONG parameter);

LONG	CheckForRegisteredEvent( LONG type);

LONG QueueEventReport(
		LONG type,
		void *parameter,
		LONG pointerFlag,
		LONG dataSize);

LONG QueueEventReportWithPointer(
		void *qevent);

void GenericEventOutput(
		void *controlString,
		...);

LONG CanEventBlock(LONG type);

LONG CanEventGoRemote(LONG type);

LONG AllocateConnectionTasks(
		LONG connectionNumber,
		LONG numberOfTasks,
		LONG *firstTaskNumber,
		struct ResourceTagStructure *resourceTag);

LONG FreeConnectionTasks(
		LONG numberOfTasks,
		LONG firstTaskNumber,
		LONG connectionNumber,
		struct ResourceTagStructure *resourceTag);

LONG	EventTypeSearch(
			LONG *search,
			LONG *rEventTypeID,
			BYTE *rEventName);

LONG MapEventTypeIDToEventName(
		LONG eventTypeID,
		BYTE *eventName);

LONG MapEventNameToEventTypeID(
		BYTE *eventName,
		LONG *rEventID);

LONG	DeRegisterEventType(
			struct ResourceTagStructure *resourceTag,
			BYTE *eventTypeName);

LONG	RegisterEventType(
			struct ResourceTagStructure *resourceTag,
			BYTE *newEventTypeName,
			LONG eventSleepableFlag,
			LONG *rEventType,
			LONG *rEventHeaderSize);

LONG SetEventTypeIDRemoteFlag(
		LONG eventTypeID,
		LONG newFlagValue);

/* End of routines in the EVENT module */
/****************************************************************************/
/* Routines in the HOPTIONS module */

LONG CheckHardwareOptions(
		struct IOConfigurationStructure *IOConfig);

LONG RegisterHardwareOptions(
		struct IOConfigurationStructure *IOConfig,
		struct DriverConfigurationStructure *configuration);

void DeRegisterHardwareOptions(
		struct IOConfigurationStructure *IOConfig);

LONG CheckIOPort(
		LONG IOPort,
		LONG IOLength,
		BYTE ShareFlag);

LONG CheckMemoryDecode(
		LONG MemoryAddress,
		LONG MemoryLength,
		BYTE ShareFlag);

LONG CheckInterrupt(
		LONG InterruptNumber,
		BYTE ShareFlag);

LONG CheckSlot(
		LONG SlotNumber);

LONG CheckDMA(
		LONG DMALevel,
		BYTE ShareFlag);

LONG AddProcedureBeforeDOS(
		void (*BeforeProcedure)(void),
		void (*AfterProcedure)(void));

void DeleteProcedureBeforeDOS(
		void (*BeforeProcedure)(void));

void ExecuteProceduresBeforeDOS(void);

void ExecuteProceduresAfterDOS(void);

LONG RegisterWriteCombinePages(
	ADDR startingPageAddress,
	LONG numberOfPages);

LONG DeRegisterWriteCombinePages(
	ADDR startingPageAddress,
	LONG numberOfPages);

void SetPATSupportAvailableFlag(LONG value);

/* End of routines in the HOPTIONS module */
/****************************************************************************/
/* Routines in the HPFSPRIM module */

LONG HPFSConvertPathString(
		LONG stationNumber,
		BYTE base,
		BYTE *modifierString,
		LONG *volumeNumber,
		LONG *pathBase,
		BYTE *pathString,
		LONG *pathCount);

LONG HPFSConvtPatternAllowAsterisks(
		BYTE *sourcePattern,
		BYTE *convertedPattern,
		LONG *wildCardFlag);

/* End of routines in the HPFSPRIM module */
/****************************************************************************/
/* Routines in the INIT module */

LONG GetServerConfigurationType(void);

void OSMain(void);

void AfterBinderyOpenInitialization(void);

void AfterNetNumberInitialization(void);

void ParseDOSCommandLine(
		BYTE *commandLine);

LONG GetLoaderCommandLine(
		BYTE *commandLine);

void RestartServer(
		BYTE *commandLine);

LONG GetThreadSecurityInterface(struct ResourceTagStructure *RTag, LONG *functionCallback);
LONG ResetThreadSecurityInterface(struct ResourceTagStructure *RTag, LONG *functionCallback);

/* End of routines in the INIT module */
/****************************************************************************/
/* Routines in the IPX module */

LONG CIPXCancelECB(
		void *eventControlBlock);

LONG CIPXCheckForSocket(
		LONG socketNumber);

LONG CIPXCloseSocket(
		LONG socketNumber);

LONG CIPXCountReceiveECBs(
		LONG socketNumber);

void CIPXGetConfiguration(
		int *IPXOpenSocketCount,
		int *IPXSocketTableSize,
		int *IPXMaxSocketTableSize);

LONG CIPXGetECB(
		LONG socketNumber);

void CIPXGetFSAddress(
		BYTE *networkAddress);

void CIPXGetInternetworkAddress(
		BYTE *networkAddress);

LONG CIPXGetLocalTarget(
		BYTE *networkAddress,
		BYTE *immediateAddress,
		LONG *transportTime);

LONG CIPXListen(
		void *eventControlBlock);

LONG CIPXOpenSocketSystem(
		LONG *socketNumber,
		struct ResourceTagStructure *RTag,
		void (*ESR)(
				ECB *systemBuffer));

LONG CIPXOpenSocketESR(
		LONG *socketNumber,
		struct ResourceTagStructure *RTag,
		void (*ESR)(
				ECB *systemBuffer));

LONG CIPXOpenSocketRTag(
		LONG *socketNumber,
		struct ResourceTagStructure *RTag);

LONG CIPXRoutePacketDirectly(
		void *eventControlBlock);

LONG CIPXSendPacket(
		void *eventControlBlock);

LONG CIPXSendPacketSkipChkSum(
		void *eventControlBlock);

void CIPXSetConfiguration(
		int IPXMaxSocketTableSize);

LONG CGetIpxMib(void);

/* End of routine in the IPX module */
/****************************************************************************/
/* Routines in the IPXPROTO module */


LONG GetMaximumPacketSize(
		struct RequestPacketStructure *requestPacket,
		LONG connectionNumber);

void SendPacket(
		LONG LANBoardNumber,
		void *packet,
		BYTE *immediateAddress);

void BroadcastPacket(
		LONG LANBoardNumber,
		void *packet);

void InitializeIPXProtocolStack(
		BYTE *ConfigurationString);

void CIPXRouterReceiveHandler(
		void *ecb);

LONG GetOtherIOInternalNet(void);

LONG GetMSEngineInternalNet(void);

LONG GetServerStateFlags(void);

/* End of routines in the IPXPROTO module */
/****************************************************************************/
/* Routines in the DIAG module */

void InitializeDiag(void);

/* End of routines in the DIAG module */
/****************************************************************************/
/* Routines in the KBBIOS module */

void InitializeKeyboard(void);

void RestoreKeyboard(void);

void NonInterruptHandler(void);

LONG GetKeyboardStatus(void);

LONG RegisterAlternateKeyHandler(
		void (*NewKeyHandler)(
				struct ScreenStruct *screenID,
				LONG scanCode,
				LONG keyStatus,
				LONG keyType,
				LONG keyValue),
				struct ResourceTagStructure *RTag);

void UnRegisterAlternateKeyHandler(void);

void ClearTypeAheadBuffer(void);

/* End of routines in the KBBIOS module */
/****************************************************************************/
/* Routines in the KERNEL module */

LONG CheckInterruptTimeEventLimit(void);

void StartSleepNotAllowed(void);

void EndSleepNotAllowed(void);

LONG CGetThreadHandicapAmount(
		LONG ProcessID);

LONG GetNumberOfServerProcesses(void);

LONG GetRunningProcess(void);

/* End of routines in the KERNEL module */
/****************************************************************************/
/* Routines in the KERNIO module */

LONG CGetMyProcessID(void);

LONG CMakeProcess(
		LONG schedulingPriority,
		void (*codeAddress)(void),
		void *stackTopAddress,
		LONG stackLength,
		BYTE *processName,
		struct ResourceTagStructure *RTag);	/* ProcessSignature */

void CDestroyProcess(
		LONG processID);

void CRescheduleLast(void);

void CRescheduleLastLowPriority(void);

void CRescheduleLastWithDelay(void);

void CYieldIfNeeded(void);
void CYieldWithDelay(void);
void CYieldUntilIdle(void);

void CSleepUntilInterrupt(void);

void CRescheduleFromInterrupt(
		LONG processID);

void CSetThreadHandicapAmount(
		LONG ProcessID,
		LONG HandicapAmount);

LONG CCancelWorkToDo(
		struct	WorkToDoStructure	*TargetNode);

void CScheduleWorkToDo(
		struct WorkToDoStructure *WorkNode);

void CScheduleDelayedWorkToDo(
		struct WorkToDoStructure *WorkNode);

LONG CAllocSemaphore(
		LONG initialSemaphoreValue,
		struct ResourceTagStructure *RTag);

void CDeAllocateSemaphore(
		LONG semaphoreNumber);

void CPSemaphore(
		LONG semaphoreNumber);

void CVSemaphore(
		LONG semaphoreNumber);

void CSemaphoreReleaseAll(
		LONG semaphoreNumber);

long CExamineSemaphore(
		LONG semaphoreNumber);

LONG CFreeUpProcessFromSemaphore(
		LONG processID);

LONG CFreeUpProcessFromSpecificSem(
		LONG semHandle,
		LONG processID);

LONG CSpecifyWorkerThreadStackSize(
		LONG stackSize);

void CScheduleFastWorkToDo(
		struct WorkToDoStructure *WorkNode);

/* End of routines in the KERNIO module */
/****************************************************************************/
/* Routines in the LOADER module */

LONG InterceptPublicAddress(
		STR *name,
		UINT32 type,
		UINT32 moduleHandle,
		UINT32 address,
		UINT32 *CurrentAddress,
		UINT32 *flags);

LONG InterceptAddressAndStub(
		STR *name,
		UINT32 moduleHandle,
		UINT32 address,
		UINT32 stubAddress,
		UINT32 *previousAddress,
		UINT32 *previousStub,
		UINT32 *previousModuleHandle);


LONG FindPublicRecordStructure(
		BYTE *name,
		struct ExternalPublicDefinitionStructure *newPublicRecords,
		LONG *listNumber,	/* 0 - internal, 1 - external, 2 - new external, 3 - old internal */
		LONG *record);

LONG GetLoadFileHeaderFlagsOffset(void);

int SetPMPrivateSymbolHandler(
	int (*PrivateSymbolHandler)(
		struct ScreenStruct *screenID,
		BYTE *dataImage,
		LONG dataImageSize,
		struct LoadDefinitionStructure *loadRecord,
		char *symbolName,
		LONG *symbolValue));

LONG LoadFile(
		struct ScreenStruct *screenID,
		LONG handle,
		LONG (*ReadRoutine)(
				LONG handle,
				LONG offset,
				void *buffer,
				LONG numberOfBytes),
		LONG loadOptions,
		BYTE *loadModuleName,
		BYTE *loadDirectoryPath,
		BYTE *CommandLine);

LONG UnloadLoadedFile(
		struct LoadDefinitionStructure *LoadRecord);

void KillMe(
		LONG moduleHandle);

void RipNLMFromFaultedAddressSpace (
		struct LoadDefinitionStructure *LoadRecord);

LONG LoadAllNeededModules(
		struct ScreenStruct *screenID,
		struct LoadFileHeaderStructure *loadFileHeader,
		LONG handle,
		LONG (*ReadRoutine)(
				LONG handle,
				LONG offset,
				void *buffer,
				LONG numberOfBytes),
		LONG loadOptions,
		BYTE *loadDirectoryPath,
		LONG messageFlag);

BYTE *AllocateMappedMemory(
		LONG imageType,
		LONG size,
		struct DomainStructure	*domainID,
		LONG SleepOKFlag,
		LONG *SleptFlag);

LONG ReturnMappedMemory(
		BYTE *address,
		LONG size,
		struct DomainStructure	*domainID);

LONG ImportPublicSymbol(
		LONG moduleHandle,
		BYTE *symbolName);

LONG UnImportPublicSymbol(
		LONG moduleHandle,
		BYTE *symbolName);

LONG ExportPublicSymbol(
		LONG moduleHandle,
		BYTE *symbolName,
		LONG address);

LONG GetPublicAddress(
		BYTE *name,
		LONG type,
		LONG moduleHandle,
		LONG *address);

UINT32 SetPublicRecordTypeToOld(
		STR *name,
		struct LoadDefinitionStructure *moduleHandle);
		
LONG RegisterDomainDriver(
		void *list,
		struct ResourceTagStructure *RTag);

LONG UnRegisterDomainDriver(
		struct ResourceTagStructure *RTag);

LONG RegisterDomainNLM(LONG rev, void *list,
		struct ResourceTagStructure *RTag);

LONG UnRegisterDomainNLM(void);

BYTE *GetDomainName(struct DomainStructure *domain);

LONG GetDomainRing(struct DomainStructure *domain);

LONG SetFaultSignal(LONG OSsignalMask
		      ,LONG USRsignalMask
		      ,void *stackPtr
		      );
LONG SignalFault(LONG Signal);

LONG FindAlternateMessageFile(
		BYTE *loadDirectoryPath,
		BYTE *loadModuleName,
		struct LoadDefinitionStructure *LoadRecord,
		LONG *count,
		BYTE ***buffer,
		LONG *languageID,
		LONG *size);

LONG ReturnMessageInformation(
		LONG moduleHandle,
		BYTE ***messageTable,
		LONG *messageCount,
		LONG *languageID,
		BYTE **helpFile);

LONG GetNLMVersionInfo(
		LONG moduleHandle,
		LONG *majorVersion,
		LONG *minorVersion,
		LONG *revision,
		LONG *year,
		LONG *month,
		LONG *day,
		BYTE *copyright);

LONG GetNLMNames(
		LONG moduleHandle,
		BYTE *fileName,
		BYTE *moduleDescription);

LONG GetNextLoadedListEntry(
		LONG moduleHandle);

struct LoadDefinitionStructure *ValidateModuleHandle(
		LONG moduleHandle);

ERROR	AddModuleToInternalModuleList(struct LoadDefinitionStructure *);
ERROR	RemoveModuleFromInternalModuleList(struct LoadDefinitionStructure *);

LONG GetNumberOfReferencedExports(
		LONG moduleHandle,
		LONG *count);

LONG RegisterCommandLineInfo(
	LONG moduleHandle,
	BYTE *buffer,
	LONG bufferSize,
	LONG flags,
	LONG customID,
	void *beforeCLHandle,
	void **returnCLHandle,
	void *usingProxie,
	LONG usingProxieSize);
	
void DeRegisterCommandLineInfo(
	void *clHandle);

LONG GetCommandLineInfo(
	void *clHandle,
	BYTE *buffer,
	LONG bufferSize,
	LONG *commandLength,
	LONG *moduleHandle,
	LONG *flags,
	LONG *customID,
	void *loadDefinition,
	LONG loadDefinitionSize);

void *GetNextCommandLineHandle(
	void *lastHandle);

/*** GREEN RIVER NICI LOADER CHANGE ***/
LONG GetNICILoaderSymbols(struct LoaderSymbols *LoaderSymbol);

/*** GREEN RIVER NICI LOADER CHANGE ***/

/* End of routines in the LOADER module */
/****************************************************************************/
/* Routines in the LOADPRIM module */

LONG SwitchStacksAndCall(
		LONG (*ProcedureToCall)(),
		BYTE *Stack,
		LONG StackSize,
		LONG NumberOfParameters,
		...);

LONG SwitchStacksAndCallWithPointer(
		LONG (*ProcedureToCall)(),
		BYTE *Stack,
		LONG StackSize,
		LONG NumberOfParameters,
		void *parameters);

/* End of routines in the LOADPRIM module */
/****************************************************************************/
/* Routines in the LOCALE module */

void GetDOSFirstByteBitMap(
		BYTE *bitmapBuff);

void GetDOSSecondByteBitMap(
		BYTE *bitmapBuff);

LONG GetDOSNameSpaceType(void);

LONG GetFileSystemVersion(void);

void GetDOSUpperCaseTable(
		BYTE *tableBuff);

void GetLongNameUpperCaseTable(
		BYTE *tableBuff);

void GetDOSValidCharBitMap(
		BYTE *bitmapBuff);

void GetOSFirstByteBitMap(
		BYTE *bitmapBuff);

void GetOSUpperCaseTable(
		BYTE *tableBuff);

void GetOSLineDrawCharTable(
		BYTE *tableBuff);

LONG GetDOSOpenFileCount(void);

LONG GetDiskIOsPending(void);

LONG GetDOSDateAndTime(void);

LONG GetTimeInMinutes(void);

LONG GetBellWasRung(void);

LONG SetBellWasRung(
		LONG newBellValue);

LONG GetAllowUnencryptedPasswords(void);

LONG GetLogAllowed(void);

LONG SetLogAllowed(
		LONG newLogAllowedValue);

LONG GetRLocksMax(void);

LONG GetRLocksCount(void);

LONG GetFLocksMax(void);

LONG GetFLocksCount(void);

/* End of routines in the LOCALE module */
/****************************************************************************/
/* Routines in the LSL module */

void InitializeLSL(void);

LONG CLSLAddProtocolID(
		void *ProtocolID,
		void *ProtocolName,
		void *MediaName);

LONG CLSLBindStack(
		LONG ProtocolNumber,
		LONG BoardNumber);

LONG CLSLDeRegisterStack(
		LONG ProtocolNumber);

LONG CLSLGetMLIDControlEntry(
		LONG BoardNumber,
		void (*ControlEntryPoint)(void));

LONG CLSLGetPhysicalNodeAddress(
		LONG BoardNumber,
		void *NodeAddress);

LONG CLSLGetPIDFromStackIDBoard(
		LONG ProtocolNumber,
		LONG BoardNumber,
		void *ProtocolID);

LONG CLSLGetProtocolControlEntry(
		LONG StackID,
		LONG BoardNumber,
		void (*ControlEntryPoint)(void));

LONG CProtocolControl100(
		LONG protocolNumber,
		LONG boardNumber,
		BYTE *displayBuffer);

LONG CLSLGetStackIDFromName(
		BYTE *Name,
		LONG *ProtocolNumber);

LONG CLSLRegisterStackRTag(
		void *ProtocolName,
		void (*ReceiveEntryPoint)(void),
		void (*ControlEntryPoint)(void),
		void *StackID,
		struct ResourceTagStructure *StackRTag, /* Use LSLStackSignature */
		struct ResourceTagStructure *ReceiveRTag); /* use ECBSignature */

LONG CLSLDeRegisterDefaultChain(
		LONG StackID,
		LONG BoardNumber);

LONG CLSLDeRegisterPreScanRxChain(
		LONG StackID,
		LONG BoardNumber);

LONG CLSLDeRegisterPreScanTxChain(
		LONG StackID,
		LONG BoardNumber);

LONG CLSLGetStartChain(
		LONG BoardNumber,
		void *DefaultChainPtr,
		void *ReceivePreScanPtr,
		void *TransmitPreScanPtr);

LONG CLSLRegisterDefaultChain(
		struct ResourceTagStructure *StackRTag, /* Use LSLDefaultStackSignature */
		LONG BoardNumber,
		LONG PositionRequested,
		void *StackID,
		void (*ReceiveEntryPoint)(void),
		void (*ControlEntryPoint)(void),
		struct ResourceTagStructure *ReceiveRTag); /* use ECBSignature */

LONG CLSLRegisterPreScanRxChain(
		struct ResourceTagStructure *StackRTag, /* Use LSLPreScanStackSignature */
		LONG BoardNumber,
		LONG PositionRequested,
		void *StackID,
		void (*ReceiveEntryPoint)(void),
		void (*ControlEntryPoint)(void),
		struct ResourceTagStructure *ReceiveRTag); /* use ECBSignature */

LONG CLSLRegisterPreScanTxChain(
		struct ResourceTagStructure *StackRTag, /* Use LSLTxPreScanStackSignature */
		LONG BoardNumber,
		LONG PositionRequested,
		void *StackID,
		void (*ReceiveEntryPoint)(void),
		void (*ControlEntryPoint)(void),
		struct ResourceTagStructure *ReceiveRTag); /* use ECBSignature */

void CLSLReSubmitDefaultECB(
		LONG StackID,
		void *DefaultECB);

void CLSLReSubmitPreScanRxECB(
		LONG StackID,
		void *SendECB);

void CLSLReSubmitPreScanTxECB(
		LONG StackID,
		void *ReceiveECB);

void CLSLReturnRcvECB(
		void *ReceiveECB);

LONG CLSLGetRcvECBRTag(
		struct ResourceTagStructure *ECBRTag,
		void **ReceiveECB);

LONG CLSLSendPacket(
		void *SendECB);

LONG CLSLUnbindStack(
		LONG StackID,
		LONG BoardNumber);

LONG CLSLModifyStackFilter(
		LONG	StackID,
		LONG	BoardNumber,
		LONG	NewMask,
		LONG	*CurrentMask);

LONG CLSLControlStackFilter(
		LONG	BoardNumber,
		LONG	Function,
		LONG	Mask,
		LONG	Parm0,
		LONG	Parm1);

LONG CLSLGetBoundBoardInfo(
		LONG	BoardNumber,
		LONG	*StackBuffer);

LONG MLIDSendPacket(
		LONG BoardNumber,
		void (*ControlEntryPoint)(void));

LONG LSLGetMaximumPacketSize(void);

LONG GetProtocolNameTableEntry(
		LONG protocolID,
		BYTE *nameBuff);

LONG GetMLIDConfigurationTableEntry(
		LONG volumeNumber,
		BYTE *configBuff);

LONG GetMLIDLoadedHandleTableEntry(
		LONG boardNumber);

LONG GetNumberOfLANs(void);

LONG GetReceiveBuffersCount(void);

LONG LSLSendProtocolInfoToPartner(
		LONG protocolID,
		BYTE *protocolInformation,
		LONG length,
		void (*InformationSentCallBack)(
			LONG reserved,
			BYTE *protocolInformation));

LONG LSLSendProtocolInfoToOtherEng(
		LONG protocolID,
		BYTE *protocolInformation,
		LONG length,
		void (*InformationSentCallBack)(
			BYTE *protocolInformation));

#define ODISTAT unsigned long
#define LSLGetExtendedLinkSupportStats(x,y) CLSL_GetLSLExtendedStatistics(x,y)

ODISTAT CLSL_GetLSLExtendedStatistics(
		void *buffer,
		UINT32 *bufferSize);

ODISTAT CLSL_GetCurrentNumberOfFreeECBs(
		UINT32 freePoolID,
		UINT32 *numFree);

ODISTAT CLSL_ChangeCurrentOwnerOfRcvECB(
		ECB *ecb,
		void *ECBResourceObject);

/* End of routines in the LSL module */
/****************************************************************************/
/* Routines in the ENCP module */

LONG ENCPRequest(
		LONG station,
		LONG task,
		BYTE request,
		BYTE *info,
		BYTE *answer,
		LONG *answerLength,
		LONG DataPacketLength);

/* End of routines in the ENCP module */
/****************************************************************************/
/* Routines in the FNMATCH module */

int fileNameMatch (const char *pzPattern, const char *pzString, int no_leading_period);

/* End of routines in the FNMATCH module */
/****************************************************************************/
/* Routines in MMU module */

LONG GetPageFaultRecoveryAddressPtr(void);

LONG RegisterPage(
		struct DomainStructure *DomainStruct,
		LONG LogicalPageAddress,
		LONG PhysicalPageAddress,
		LONG SupervisorCode,
		LONG WriteEnableCode);

void *UnRegisterPage(
		struct DomainStructure *DomainStruct,
		LONG LogicalPageAddress);

LONG SetLogicalPageProtection(
		struct DomainStructure *DomainStruct,
		LONG LogicalPageAddress,
		LONG SupervisorCode,
		LONG WriteEnableCode);

LONG GetLogicalPageInfo(
		struct DomainStructure *DomainStruct,
		LONG LogicalPageAddress,
		LONG *PhysicalPageAddress,
		LONG *PageControlBits);

LONG ReadPhysicalMemory(
		BYTE *Source,
		BYTE *Dest,
		LONG NumUnits,
		LONG UnitLength);

LONG WritePhysicalMemory(
		BYTE *Source,
		BYTE *Dest,
		LONG NumUnits,
		LONG UnitLength);

void *AllocatePhysicalPages(
		LONG NumberOf4KPages,
		LONG SleepOKFlag,
		LONG *SleptFlag);

void DeAllocatePhysicalPages(
		void *Page,
		LONG NumberOfPages);

LONG CMoveWithPFP(
		BYTE *Source,
		BYTE *Dest,
		LONG NumUnits,
		LONG UnitLength);

LONG RegisterPagesWithNLM(
		BYTE *FirstPageToRegister,
		LONG NumberOfPagesToRegister,
		struct LoadDefinitionStructure *NLMStruct);

void UnRegisterPagesWithNLM(
		BYTE *FirstPageToUnRegister,
		LONG NumberOfPagesToUnRegister,
		struct LoadDefinitionStructure *NLMStruct);

void UnloadNLMAllocPages(
		struct LoadDefinitionStructure *NLMStruct);

struct DomainStructure *FindDomain(
		BYTE *DomainName);

struct DomainStructure *CreateDomain(
		BYTE *DomainName,
		LONG Ring);

void AddNLMToDomain(
		struct LoadDefinitionStructure *NLMPtr,
		struct DomainStructure *DomainPtr);

void DeleteNLMFromDomain(
		struct LoadDefinitionStructure *NLMPtr,
		struct DomainStructure *DomainPtr);

struct LoadDefinitionStructure *GetNextNLMInDomain(
		struct LoadDefinitionStructure *PrevNLMPtr,
		struct DomainStructure *DomainPtr);

LONG DeleteEmptyDomain(
		struct DomainStructure *DomainPtr);

/* End of routines in the MMU module */
/****************************************************************************/
/* Routines in the MMUASM module */

#if !InLineAssemblyEnabled
void FlushTLB(void);
#endif

LONG CMoveWithPFP(
		BYTE *Source,
		BYTE *Destination,
		LONG NumberOfBytes,
		LONG UnitSize);

/*
 * Removed the following functions.
BYTE CReadByteFromDomain(
		BYTE *address,
		struct DomainStructure *Domain);

void CWriteByteToDomain(
		BYTE *address,
		struct DomainStructure *Domain,
		BYTE Data);

LONG CReadLongFromDomain(
		LONG *address,
		struct DomainStructure *Domain);

void CWriteLongToDomain(
		LONG *address,
		struct DomainStructure *Domain,
		LONG Data);
*/

LONG CValidateMappedAddress(
		void *Address,
		void *PageDirectoryPtr,
		LONG Flags);

/* End of routines in the MMUASM module */
/****************************************************************************/
/* Routines in the NAMESERV module */

LONG RegisterNameService(
		struct ResourceTagStructure *resourceTag,
		LONG nameServiceVersion,
		struct NameServiceStructure *NameServiceEntries,
		struct NameServiceControlStructure *NameServiceControlEntries);

LONG DeregisterNameService(
		struct ResourceTagStructure *resourceTag);

void NameServiceCleanupProcedure(
		struct ResourceTagStructure *resourceTag);

LONG EnableNameService(void);

LONG DisableNameService(void);

LONG InitializeObjectIDs(
		LONG serverObjectID,
		LONG supervisorObjectID);

LONG NameServiceNotAvailable(); /* Argument list varies, do not declare */

LONG ScanOldObjects(
		LONG Station,
		LONG task,
		LONG findID,
		LONG findType,
		LONG findInfoBits,
		BYTE *searchPattern,
		LONG *itemCount,
		LONG *nextFindID,
		LONG answerMax,
		BYTE *answer,
		LONG *AnswerLength);

LONG GetDSResetFlag(void);

/* End of routines in the NAMESERV module */
/****************************************************************************/
/* Routines in the NCPEXT module */

LONG ConvertStringToCountPathVolume(
		BYTE *String,
		LONG *Volume,
		BYTE *PathString,
		LONG *PathCount);

/* End of routines in the NCPEXT module */
/****************************************************************************/
/* Routines in the NCPPRIMS module */

LONG ConvertPathString(
		LONG stationNumber,
		BYTE base,
		BYTE *modifierString,
		LONG *volumeNumber,
		LONG *pathBase,
		BYTE *pathString,
		LONG *pathCount);

LONG ConvertPathStringWithDirBase(
		LONG inputVolNumber,
		LONG dirbase,
		BYTE *modifierString,
		LONG *volumeNumber,
		LONG *pathBase,
		BYTE *pathString,
		LONG *pathCount);

/*----------------------------------------------------------**
** struct NCPDirectoryHandleStructure *FindDirectoryHandle( **
** 		LONG stationNumber,                                **
** 		LONG handleNumber);                                **
**----------------------------------------------------------*/

void ConvertPathToNCP(
		BYTE *string,
		LONG stringLength);

struct SearchMapStructure *FindSearchMap(
		LONG stationNumber,
		struct SearchStructure *searchData);

struct SearchMapStructure *FindSearchMapFromParameters(
		LONG stationNumber,
		LONG volumeNumber,
		LONG directoryNumber,
		LONG index);

void EndOfTaskNCPSearchMaps(
		LONG stationNumber,
		LONG task);

void MoveSearchMap(
		LONG Station,
		struct SearchMapStructure *SearchMap);

LONG ConvertBinderyPattern(
		BYTE *sourcePattern,
		BYTE *convertedPattern);

LONG ConvertPatternAllowAsterisks(
		BYTE *sourcePattern,
		BYTE *convertedPattern,
		LONG *wildCardFlag);

void ConvertDirectoryToNCP(
		struct DirectoryStructure *directoryEntry,
//;;BEGIN SPD 138431 - OPNSIZFX.NLM Change 1 of 34 - TDR 5/23/97
//		BYTE *NCPAnswer);
		BYTE *NCPAnswer,
		LONG FileHandle,
		LONG Station);
//;;END SPD 138431 - OPNSIZFX.NLM Change 1 of 34 - TDR 5/23/97

LONG MapVolumeNameToNumber(
		BYTE *volumeNameString,
		LONG *volumeNumber);

LONG CheckForWildCards(
		BYTE *String);

LONG CWildMatch(	/* This can not be used for DOS file names */
		BYTE *pattern,	/* use DOSWildMatch instead */
		BYTE *string);

LONG MapMacIDToNetWare(
		LONG MACID);

LONG MapNetWareIDToMac(
		LONG Volume,
		LONG NetWareID);

LONG VMMapVolumeNameToNumber(
		BYTE *VolumeNameString,
		LONG *Answer);

/* End of routines in the NCPPRIMS module */
/****************************************************************************/
/* Routines in the NLMSUPP module */

LONG FindAndLoadNLM(
		struct ScreenStruct *screenID,
		LONG FLOptions,
		BYTE *nlmName);

/* FLOptions */
#define	FLLoadFromSYSorDOS			0x00000000
#define	FLLoadBoundInCopyOnly		0x00000001
#define	FLLoadFromDOSorList			0x00000002
#define	FLLoadAll					0x00000004
#define	FLLoadFromDOSOnly			0x00000008
#define	FLLoadSilentOff			0x80000000

UINT32 GetBoundInNLMList(
		void *buffer,
		UINT32 maxBufferLength,
		UINT32 *bytesConsumed);

void InitializeSearchPaths(void);

void SwitchSearchPathToSYSVolume(void);

void DisplayCurrentSearchPaths(
		struct ScreenStruct *screenID);

LONG AddSearchPathAtEnd(
		struct ScreenStruct *screenID,
		BYTE *path);

LONG InsertSearchPath(
		struct ScreenStruct *screenID,
		LONG searchPathNumber,
		BYTE *path);

LONG DeleteSearchPath(
		struct ScreenStruct *screenID,
		LONG searchPathNumber);

struct SearchPathInfo
{
	int searchPathNumber;
	int moreFlag;
	int IsDosFlag;
	int searchFlags;
	char path[256];
};


int EnumSearchPathInfo(int searchPathNumber, struct SearchPathInfo *pInfo);
int ReturnSearchPathChangeCounter(int resetFlag);

// The following api's are for the Zen team
int LockSearchPath(int searchPathNumber);
int UnLockSearchPath(int searchPathNumber);
int SetSearchPathInsertNumber(int NewNextSearchPathInsertNumber);
// End of Zen api's

void RemoveDOSSearchPaths(
		struct ScreenStruct *screenID);

LONG ServerOpen(
		BYTE *fileName, /* NOTE: Must have a byte for length at front */
		LONG *handle);

LONG GetSearchPathElement(
		LONG index,				/* which element in search path list to get */
		LONG *isDOSFlag,		/* DOS flag value if element is found */
		BYTE *retPath); 		/* path if element is found */

LONG OpenFileUsingSearchPath(
		BYTE *fileNameAndPath,
		LONG *handle,
		BYTE *isDOSFlag,
		BYTE *openedFilePath,
		BYTE *openedFileName,
		BYTE overrideExistingExtension,
		LONG numberOfExtensions,
		...);

void CloseFileFromSearchPath(
		BYTE isDOSFlag,
		LONG Handle);

void AppendSuffix(
		BYTE *filePath,
		BYTE *suffix,
		LONG replaceFlag);

LONG UnloadModule(
		struct ScreenStruct *screenID,
		BYTE *commandLine);

LONG CompareModuleFileNames(
		BYTE *searchFileName,
		BYTE *fileName);

LONG ListModules(
		LONG function,
		struct ScreenStruct *screenID,
		BYTE *cmdLine,
		BYTE *unused,
		LONG token);

LONG ParseDriverParameters(
		struct IOConfigurationStructure *IOConfig,
		struct DriverConfigurationStructure *configuration,
		struct AdapterOptionDefinitionStructure *adapterOptions,
		struct LANConfigurationLimitStructure *configLimits,
		BYTE *frameTypeDescription[],
		LONG needBitMap,
		BYTE *commandLine,
		struct ScreenStruct *screenID);

LONG DoesHardwareMatch(
		struct IOConfigurationStructure *IOConfig,
		struct DriverConfigurationStructure *configuration,
		LONG checkBits);

LONG GetAllBoundNetWorkInfo(
		LONG nextItem,
		LONG items,
		struct networkAddressInfo *buffer);

LONG GetBoundNetWorkInfo(
		LONG boardNumber,
		LONG protocolNumber,
		struct networkAddressStruct *buffer);

LONG GetHaveBits(
		struct IOConfigurationStructure *IOConfig);

void SetFrameType(
		struct ScreenStruct *screenID,
		BYTE **string,
		BYTE *haveFrameType,
		LONG numberOfSupportedFrameTypes,
		BYTE *frameTypeDescription[],
		struct DriverConfigurationStructure *configuration);

LONG ConvertStringToUnsignedLong(
		BYTE **string,
		LONG *value,
		LONG minValue,
		LONG maxValue,
		LONG base);

LONG ParseNetworkNumber(
		BYTE *commandLine,
		LONG *netNumber,
		struct ScreenStruct *screenID,
		LONG boardNumber);

LONG BindProtocolToDriver(
		struct ScreenStruct *screenID,
		BYTE *commandLine);

LONG UnBindProtocolFromDriver(
		struct ScreenStruct *screenID,
		BYTE *commandLine);

LONG DisplayConfiguration(
		struct ScreenStruct *screenID,
		LONG	*linesUsed);

void DisplayLANBoardConfiguration(
		struct ScreenStruct *screenID,
		LONG boardNumber);

LONG InternalDisplayLANBoardConfiguration(
		struct ScreenStruct *screenID,
		LONG boardNumber,
		LONG *linesUsed);

void DisplayLANConfiguration(
		struct ScreenStruct *screenID,
		struct DriverConfigurationStructure *configInfo);

LONG GetHaveBits(
		struct IOConfigurationStructure *IOConfig);

LONG LoadModule(
		struct ScreenStruct *screenID,
		BYTE *fileName,
		LONG loadOptions);

LONG LoadModuleInSameDomain(
		struct ScreenStruct *screenID,
		BYTE *commandLine,
		LONG loadOptions,
		struct LoadDefinitionStructure *module);

LONG LoadModuleInSpace(
		struct ScreenStruct *screenID,
		BYTE *commandLine,
		LONG loadOptions,
		struct AddressSpace *NLMAddressSpace);

LONG ActivateModule(
		struct ScreenStruct *screenID,
		LONG handle,
		LONG (*ReadRoutine)(
				LONG fileHandle,
				LONG offset,
				void *buffer,
				LONG numberOfBytes),
		LONG loadOptions,
		BYTE *loadModuleName,
		BYTE *loadDirectoryPath,
		BYTE *commandLine,
		struct AddressSpace *NLMAddressSpace);

UINT GetParentAddressSpaceID(void);

UINT DoServerProxyModuleOperations(UINT opFlag, BYTE *moduleName);

UINT LoadModuleAsNewChild(
				STR *commandLine,
				UINT32 loadOptions,
				UINT *AddressSpaceID);

LONG IsNLMLoadedInAddressSpace(BYTE *name, 
			struct AddressSpace	*space);

UINT	StartUpWillExec(void);
UINT	AutoExecWillExec(void);

/* End of routines in the NLMSUPP module */
/****************************************************************************/
/* Routines in the PRELUDE1 module */

void AddAvailableMemory(
		struct ScreenStruct *screenID);

LONG CReadEISAConfig(
		LONG slot,
		LONG function);

LONG INWCReadEISAConfig(
		LONG slot,
		LONG function,
		BYTE *returnBuffer);

LONG ReadEISAConfig(void);

/* End of routines in the PRELUDE1 module */
/****************************************************************************/
/* Routines in the PROTOPRM module */

LONG MapProtocolNameToNumber(
		void *protocolName,
		LONG *protocolNumber);

LONG BindProtocolToBoard(
		LONG protocolNumber,
		LONG boardNumber,
		void *configurationString);

LONG UnBindProtocolFromBoard(
		LONG protocolNumber,
		LONG boardNumber,
		void *configurationString);

/* End of routines in the PROTOPRM module */
/****************************************************************************/
/* Routines in the RESOURCE module */

struct ResourceTagStructure *AllocateResourceTag(
		struct LoadDefinitionStructure *LoadRecord,
		void *ResourceDescriptionString,
		LONG ResourceSignature);

LONG ReturnResourceTag(
		struct ResourceTagStructure *RTag,
		BYTE displayErrorsFlag);

LONG RegisterTrackedResource(
		struct LoadDefinitionStructure *LoadRecord,
		LONG Signature,
		void (*CleanupProcedure)(
				struct ResourceTagStructure *resourceTag,
				LONG RForceFlag),
		void *DescriptionString);

LONG UnRegisterTrackedResource(
		struct LoadDefinitionStructure *LoadRecord,
		LONG Signature);

LONG CheckForDefinedResources(
		struct LoadDefinitionStructure *LoadRecord);

void ReturnAllLeftOverResources(
		struct LoadDefinitionStructure *LoadRecord,
		LONG DisplayWarning);

struct ResourceTagStructure *CFindResourceTag(
		struct LoadDefinitionStructure *LoadRecord,
		LONG ResourceSignature);

/* End of routines in the RESOURCE module */
/****************************************************************************/
/* Routines in the ROUTER module */

/******* THE FOLLOWING ROUTINES ARE ONLY TO BE USED BY SERVTRAK.C ************/\
extern LONG CheckForNetConflict(
		LONG ConnectedLAN,
		struct RoutePacketStruct *Packet,
		BYTE *PacketSource,
		LONG SquelchLoopback);
extern struct NetListEntry *GetNetEntry(LONG NetID);
extern void PrematureRouterWakeUp(void);
extern void WriteRouterPacket(
		LONG ConnectedLAN,
		struct RoutePacketStruct *Packet,
		BYTE BROADCAST);
/*************************************************************************/\

void DownRouter(void);

void ResetRouter(void);

LONG RIPReset(void);

LONG RIPGetNetEntry(
		LONG  TargetNet,
		struct NetEntryType *NetEntry
		);

LONG RIPGetNetRouters(
		LONG TargetNet,
		LONG NextEntry,
		struct RouteEntryType *RouteEntry,
		LONG *MaxNumberEntries);

LONG RIPGetKnownNetworks(
	WORD	NextEntry,
	struct NetEntryType *NetEntry,
	LONG *MaxNumberEntries);


LONG RIPDisplayTable(
	struct ScreenStruct *screenID);

LONG RIPTrackOn(void);

LONG RIPTrackOff(void);


LONG RIPScanControlAdaptor(LONG NextEntry, LONG AdaptorType, struct RIPControlAdaptorType *ScanBuffer, LONG *MaxNumberEntries);
LONG RIPSetControlAdaptor(struct RIPControlAdaptorType *AdaptorOption);
LONG RIPGetControlAdaptor(struct RIPControlAdaptorType *AdaptorOption);

/* End of routines in the ROUTER module */
/****************************************************************************/
/* Routines in the SCREEN module */

// Screen Activity Error Levels
#define SAEL_OK						0			// Screen Activity 
#define SAEL_INFO						1
#define SAEL_WARNING					2
#define SAEL_ERROR					3

extern void InitializeActivity(BYTE	*text);
extern void TerminateActivity(BYTE *terminationText, UINT	errorLevel);


void InitializeScreenManager(void);

LONG OpenScreen(
		void *screenName,
		struct ResourceTagStructure *resourceTag,
		struct ScreenStruct **newScreenID);

LONG OpenPopUpScreen(
		void *screenName,
		struct ResourceTagStructure *resourceTag,
		struct ScreenStruct **newScreenID);

void CloseScreen(
		struct ScreenStruct *screenID);

void ActivateScreen(
		struct ScreenStruct *screenID);

void ActivatePopUpScreen(
		struct ScreenStruct *screenID);

void EndPopUpScreen(
		struct ScreenStruct *screenID);

LONG CheckIfScreenActive(
		struct ScreenStruct *screenID,
		LONG waitFlag);

void ClearScreen(
		struct ScreenStruct *screenID);

void GetScreenSize(
		WORD *screenHeight,
		WORD *screenWidth);

void PositionInputCursor(
		struct ScreenStruct *screenID,
		WORD row,
		WORD column);

void GetInputCursorPosition(
		struct ScreenStruct *screenID,
		WORD *row,
		WORD *column);

void EnableInputCursor(
		struct ScreenStruct *screenID);

void DisableInputCursor(
		struct ScreenStruct *screenID);

LONG PositionOutputCursor(
		struct ScreenStruct *screenID,
		WORD row,
		WORD column);

void GetOutputCursorPosition(
		struct ScreenStruct *screenID,
		WORD *row,
		WORD *column);

void SetInputToOutputCursorPosition(
		struct ScreenStruct *screenID);

LONG InputFromKeyboard(
		struct ScreenStruct *screenID,
		void *allowedCharacterSet,
		WORD bufferLength,
		WORD editWidth,
		BYTE *buffer,
		LONG linesToProtect,
		BYTE hasDefaultString,
		void *defaultString);

LONG InputFromScreen(
		struct ScreenStruct *screenID,
		void *allowedCharacterSet,
		WORD bufferLength,
		WORD editWidth,
		BYTE *buffer,
		LONG linesToProtect,
		BYTE hasDefaultString,
		void *defaultString,
		void *promptText,
		...);

void GetKey(
		struct ScreenStruct *screenID,
		BYTE *keyType,
		BYTE *keyValue,
		BYTE *keyStatus,
		BYTE *scanCode,
		LONG linesToProtect);

LONG CheckKeyStatus(
		struct ScreenStruct *screenID);

void DebugViewScreens(char *extraInfo);

//
// Start change - SClark 1/30/01, Added support for other polling procedure
//		to be called while keyboard ISR is being polled in the debugger
//		(used by DBNET.NLM).  Found in previouse NW 5.x OS.
//

int AddKeyboardPollingProc(void (*PollingProcedure)(void));

//
// End change - SClark 1/30/01
//

void  RemoveKeyboardPollingProc(void (*PollingProcedure)());

LONG PromptForUnsignedNumber(
		struct ScreenStruct *screenID,
		LONG *result,
		LONG minValue,
		LONG maxValue,
		LONG base,
		LONG linesToProtect,
		BYTE hasDefaultValue,
		LONG defaultValue,
		void *promptText,
		...);

LONG GetUnsignedNumber(
		struct ScreenStruct *screenID,
		LONG *result,
		LONG minValue,
		LONG maxValue,
		LONG base,
		LONG linesToProtect,
		BYTE hasDefaultValue,
		LONG defaultValue);

LONG PromptForUnsignedSixByteNumber(
		struct ScreenStruct *screenID,
		BYTE *result,
		BYTE *minValue,
		BYTE *maxValue,
		LONG linesToProtect,
		BYTE hasDefaultValue,
		BYTE *defaultValue,
		void *promptText,
		...);

LONG GetUnsignedSixByteNumber(
		struct ScreenStruct *screenID,
		BYTE *result,
		BYTE *minValue,
		BYTE *maxValue,
		LONG linesToProtect,
		BYTE hasDefaultValue,
		BYTE *defaultValue);

LONG PromptForYesOrNo(
		struct ScreenStruct *screenID,
		LONG linesToProtect,
		LONG defaultValue,
		void *promptText,
		...);

/*

	Default:
		No    -- 0
		Yes   -- 1
		Skip  -- 2
		All   -- 3

*/
LONG PromptForYesNoAllOrSkip(
		struct ScreenStruct *screenID,
		LONG linesToProtect,
		LONG defaultValue,
		void *promptText,
		...);

void Pause(
		struct ScreenStruct *screenID);

LONG PauseWithEscape(
		struct ScreenStruct *screenID);

LONG IsKeyInSet(
		BYTE keyValue,
		BYTE *allowedCharacterSet);

LONG IsKeyInSetRange(
		BYTE *keyValue,
		BYTE *allowedCharacterSet);

LONG CalcDays(
		BYTE *time);

LONG CalcMinutes(
		BYTE *time);

void CalculateCurrentTime(void);

LONG RegisterScreenInputRoutine(
		struct ScreenStruct *screenID,
		LONG (*routine)(
				LONG key),
		LONG linesToProtect,
		struct ResourceTagStructure *resourceTag);

LONG UnRegisterScreenInputRoutine(
		struct ScreenStruct *screenID);

void ScreenInputCleanUpProcedure(
		struct ResourceTagStructure *RTag);

LONG SuspendProcessWaitingForInput(
		LONG processID,
		struct ScreenStruct *screenID);

LONG SuspendProcessWaitingForScreen(
		LONG processID,
		struct ScreenStruct *screenID);

void ScreenCleanUpProcedure(
		struct ResourceTagStructure *ScreenResourceTag);

void GetCursorStyle(
		struct ScreenStruct *screenID,
		WORD *cursorStyle);

void SetCursorStyle(
		struct ScreenStruct *screenID,
		WORD newcursorStyle);

void ReturnScreenType(
		LONG *type,
		LONG *colorFlag);

BYTE *GetScreenAddress(void);

LONG GetCursorMode(void);

struct ScreenStruct *GetLoggerConsoleScreen(void);

UINT GetLoggerScreenBuffer(BYTE	**bufferStart,
						   UINT	*byteCount);

struct ScreenStruct *GetActiveScreen(void);

struct ScreenStruct *GetSystemConsoleScreen(void);
struct ScreenStruct *GetSystemLoggerScreen(void);

LONG StartScreenUpdateGroup(
		struct ScreenStruct *screenID);

LONG EndScreenUpdateGroup(
		struct ScreenStruct *screenID);

LONG GetActualScreenSize(
		struct ScreenStruct *screenID,
		LONG *screenHeight,
		LONG *screenWidth,
		LONG *screenBufferSize);

LONG SaveFullScreen(
		struct ScreenStruct *screenID,
		BYTE *buffer);

LONG RestoreFullScreen(
		struct ScreenStruct *screenID,
		BYTE *buffer);

LONG SaveScreenArea(
		struct ScreenStruct *screenID,
		LONG line,
		LONG column,
		LONG height,
		LONG width,
		BYTE *buffer);

LONG RestoreScreenArea(
		struct ScreenStruct *screenID,
		LONG line,
		LONG column,
		LONG height,
		LONG width,
		BYTE *buffer);

LONG FillScreenArea(
		struct ScreenStruct *screenID,
		LONG line,
		LONG column,
		LONG height,
		LONG width,
		BYTE character,
		BYTE attribute);

LONG FillScreenAreaAttribute(
		struct ScreenStruct *screenID,
		LONG line,
		LONG column,
		LONG height,
		LONG width,
		BYTE attribute);

LONG ScrollScreenArea(
		struct ScreenStruct *screenID,
		LONG line,
		LONG column,
		LONG height,
		LONG width,
			LONG count,
		BYTE newLineAttribute,
		LONG direction);		/* 0 - down, 1 - up */

LONG DisplayScreenLine(
		struct ScreenStruct *screenID,
		LONG line,
		LONG column,
		LONG length,
		BYTE *textAndAttributes);

LONG DisplayScreenText(
		struct ScreenStruct *screenID,
		LONG line,
		LONG column,
		LONG length,
		BYTE *text);

LONG DisplayScreenTextWithAttribute(
		struct ScreenStruct *screenID,
		LONG line,
		LONG column,
		LONG length,
		BYTE lineAttribute,
		BYTE *text);

LONG ReadScreenCharacter(
		struct ScreenStruct *screenID,
		LONG line,
		LONG column,
		BYTE *character);

LONG WriteScreenCharacter(
		struct ScreenStruct *screenID,
		LONG line,
		LONG column,
		BYTE character);

LONG WriteScreenCharacterAttribute(
		struct ScreenStruct *screenID,
		LONG line,
		LONG column,
		BYTE character,
		BYTE attribute);

LONG GetScreenName(
		struct ScreenStruct *screenID,
		BYTE *nameBuffer);

struct ScreenStruct *GetPreviousScreen(
		struct ScreenStruct *screenID);

struct ScreenStruct *GetNextScreen(
		struct ScreenStruct *screenID);

LONG IncrementScreenActiveCount(
		struct ScreenStruct *screenID);

LONG DecrementScreenActiveCount(
		struct ScreenStruct *screenID);

void ShowTitleBarText(
		void *screenMemoryAddress,
		BYTE *titleBarText,
		LONG textLength);

LONG CheckParameterOrder(
		BYTE **controlString,
		LONG *stack);

LONG UnformattedOutputToScreen(
		struct ScreenStruct *screenID,
		char *controlString);

LONG UnformattedOutputWithAttribute(
		struct ScreenStruct *screenID,
		BYTE attribute,
		char *controlString);

LONG OutputToScreenPointerAttribute(
		struct ScreenStruct *screenID,
		BYTE attribute,
		void *controlString,
		void *arguments);

LONG OutputToScreenWithPointer(
		struct ScreenStruct *screenID,
		void *controlString,
		void *arguments);

LONG OutputToScreenWithAttribute(
		struct ScreenStruct *screenID,
		BYTE attribute,
		void *controlString,
		...);

LONG OutputToScreen(
		struct ScreenStruct *screenID,
		void *controlString,
		...);

LONG UnformattedOutputNoAttribute(
		struct ScreenStruct *screenID,
		char *controlString);

LONG OutputWithPointerNoAttribute(
		struct ScreenStruct *screenID,
		char *controlString,
		void *args);

LONG OutputToScreenNoAttribute(
		struct ScreenStruct *screenID,
		char *controlString,
		...);

void sprintfWithPointer(
		BYTE *destinationString,
		void *controlString,
		void *arguments);

void INWsprintf(
		BYTE *destinationString,
		void *controlString,
		...);

void OutputToStringWithPointer(
		LONG destinationStringLength,
		BYTE *destinationString,
		void *controlString,
		void *arguments);

void OutputToString(
		LONG destinationStringLength,
		BYTE *destinationString,
		void *controlString,
		...);

void OutputToScreenInMargins(
		struct ScreenStruct *screenID,
		BYTE	*string,
		int		 leftMargin,
		int		 rightMargin);

LONG OpenCustomScreen(
		void *screenName,
		struct ResourceTagStructure *resourceTag,
		struct ScreenStruct **newScreenID,
		LONG screenMode);

LONG IsScreenModeSupported(
		LONG screenMode);

LONG RegisterScreenHandler(
		struct ScreenHandlerStructure *screenHandler);

LONG UnRegisterScreenHandler(
		struct ScreenHandlerStructure *screenHandler);

void ReportError(
		struct ScreenStruct *screenID,
		void *formatString,
		...);

LONG RemoveScreenKeyboardOwner(
	struct ScreenStruct *screenID);

LONG RestoreScreenKeyboardOwner(
	struct ScreenStruct *screenID,
	LONG keyboardProcessID);

LONG SetScreenPhysicalAddress(
	LONG physicalAddress,
	BYTE **logicalAddress,
	LONG *oldPhysicalAddress);

LONG ReturnHardwareLoaderID(void);

extern LONG ChangeToAlternateConsoleScreen(void);
extern LONG ChangeToSystemConsoleScreen(void);

/* UNICODE SUPPORT */
typedef BYTE *(*UnicodePrintfHandler_t)(unsigned int unichar,BYTE *retAscii);

extern UnicodePrintfHandler_t RegisterUnicodePrintfHandler(
	UnicodePrintfHandler_t newHandler);

LONG ValidateScreenHandle(
		struct ScreenStruct *screenID);

LONG PauseWithEscapeForAll(struct ScreenStruct *screenID);

/* End of routines in the SCREEN module */
/****************************************************************************/
/* Routines in the SERVER module */

int AddPollingProcedureRTag(
		void (*Procedure)(void),
		struct ResourceTagStructure *RTag);

void RemovePollingProcedure(
		void (*Procedure)(void));

void ServerProcessBeingReturned(
		LONG stationNumber);

LONG GetMaximumReplyBufferSize(
		LONG Station);

void *GetReplyBuffer(
		LONG Station,
		LONG BufferLength);

void ReturnReplyBuffer(
		LONG Station,
		void *Buffer);

void ReplyUsingBuffer(
		LONG Station,
		BYTE CompletionCode,
		LONG RealLength);

void ReplyCompletionCodeOnly(
		LONG Station,
		BYTE CompletionCode);

LONG NCPKillStation(
		LONG stationNumber,
		LONG suicideFlag);

void PollingProcessDownServer(void);

void InitializeNetWareCodeProtocol(void);

void PollingProcess(void);

LONG GetLoggedUsersFromNetAddress(
		LONG ConnectionType,
		BYTE *NetAddress,
		LONG Count,
		BYTE **UserNameAndID);

void InitializeSignature(
		LONG *ABCD);

void CMakeSignature(
		BYTE *buffer,
		LONG *InputABCD,
		LONG *OutputABCD);

LONG ReturnServerSigningLevel(void);

LONG RegisterNCPSecurityExtender(
		struct NCPSecurityInputStructure *input,
		struct NCPSecurityOutputStructure *output);

LONG UnRegisterNCPSecurityExtender(void);

/* End of routines in the SERVER module */
/****************************************************************************/
/* Routines in the IPXLAN module */
LONG RegisterRouter(
		struct RegisterRouterType *Request);

LONG DeRegisterRouter(
		struct RegisterRouterType *Request);

LONG IPXChainVectors(struct RegisterRouterType *SetRequest, struct RegisterRouterType **GetChainList);

LONG IPXUnChainVectors(struct RegisterRouterType *SetRequest);
/* End of routines in the IPXLAN module */

/****************************************************************************/
/* Routines in the SERVTRAK module */

/******* THE FOLLOWING ROUTINES ARE ONLY TO BE USED BY ROUTER.C ************/
extern void CInitializeSapBootUp(void);
extern LONG SAPAddFilterService(struct SAPFilterType *SAPFilter);
extern LONG SAPSetControlAdaptorService(struct SAPControlAdaptorType *AdaptorOption);
extern void CInitializeServerTables(void);
extern void ConsumeAdvertisingPacket(
		LONG ConnectedLAN,
		struct ServerPacketStruct *packet,
		BYTE *PacketSource);
void DeleteSapEntries(void);
extern void DiscardRouter(
		LONG ConnectedLAN,
		BYTE *RouterAddress,
		LONG TargetNet);
extern void DownServers(void);
struct SAPControlAdaptorEntryType *SAPGetAdaptorID(LONG Network);
extern void SendServerInformation(
		LONG ConnectedLAN,
		LONG DestinationNet,
		BYTE *DestinationHost,
		WORD DestinationSocket,
		WORD SleepOK,
		LONG SendAll,
		LONG SendCheck,
		WORD NumberOfSapEntries,
		struct ServerPacketStruct *ServLocPacket,
		LONG whoCalledMe);

extern void ShowServerPacket(
		LONG ConnectedLAN,
		struct ServerPacketStruct *packet,
		LONG IN);
extern LONG SAPStartUp(void);
extern LONG SAPShutDown(void);
void SAPSrcDestError(
		LONG ConnectedLAN,
		struct ServerPacketStruct *Packet);
void PeriodicSAPTimer();

extern void	(*AdvertiseSAPHook)(void *, void *, LONG *);
extern void	(*SendServerInformationHook)(
					LONG,
					LONG,
					BYTE *,
					WORD,
					WORD,
					LONG,
					LONG,
					WORD,
					struct ServerPacketStruct *,
					void *,
					void *,
					void *,
					LONG,
					LONG,
					LONG);

extern void	(*SAPServerNameHook)(
					BYTE *,
					LONG *,
					void *,
					void *);

/***************************************************************************/
void MarkServerList(void);

LONG SAPGetServerEntry(
			BYTE *Name,
			WORD Type,
			struct ServerEntryType *ServerEntry);

LONG SAPGetServerSources(
		BYTE *Name,					/* Null Terminated String */
		WORD Type,
		LONG NextEntry,
		struct ServerSourceEntryType *SourceEntry,
		LONG *MaxNumberEntries);


LONG SAPGetKnownServers(
		WORD ServerType,
		WORD NextEntry,
		struct ServerEntryType *ServerEntry,
		LONG *MaxNumberEntries);

LONG SAPRefreshBindery(void);

LONG SAPReset(void);

LONG SAPDisplayTable(
		struct ScreenStruct *screenID,
		BYTE *cmdLine);

LONG SAPTrackOn(void);

LONG SAPTrackOff(void);

LONG SAPAddFilter(struct SAPFilterType *SAPFilter);
LONG SAPDeleteFilter(struct SAPFilterType *SAPFilter);
LONG SAPScanFilters(LONG NextEntry, LONG AdaptorType, LONG Adaptor, struct SAPFilterType *ScanBuffer, LONG *MaxNumberEntries);
LONG SAPScanControlAdaptor(LONG NextEntry, LONG AdaptorType, struct SAPControlAdaptorType *ScanBuffer, LONG *MaxNumberEntries);
LONG SAPSetControlAdaptor(struct SAPControlAdaptorType *AdaptorOption);
LONG SAPGetControlAdaptor(struct SAPControlAdaptorType *AdaptorOption);

/* End of routines in the SERVTRAK module */
/****************************************************************************/
/* Routines in the SETPARMS module */

LONG ProcessUnknownSetParameters(void);

void InitializeSetableParametersManager(void);

LONG ProcessSetCommand(
		struct ScreenStruct *screenID,
		BYTE *upperCaseCommandLine,
		BYTE *commandLine,
		BYTE startUpFlag);

LONG ProcessCSetCommand(
		struct ScreenStruct *screenID,
		BYTE *upperCaseCommandLine,
		BYTE *commandLine,
		BYTE startUpFlag);

LONG RegisterSetableParameter(
		struct SetableParametersStructure *setParameter);

LONG DeRegisterSetableParameter(
		struct SetableParametersStructure *setParameter);

void	ProcessTimeServerType(BYTE *bp);

/* End of routines in the SETPARMS module */
/****************************************************************************/
/* Routines in the CSPACE module */

void *Alloc(
		LONG numberOfBytes,
		struct ResourceTagStructure *RTag);

void *AllocSleepOK(LONG numberOfBytes,
		struct ResourceTagStructure *RTag,
		LONG *SleptFlag);

void AllocGarbageCollect(
		struct LoadDefinitionStructure *LoadRecord);

void Free(
		void *address);

LONG SizeOfAllocBlock(
		void *AllocAddress);

LONG GetCurrentNonCacheMemory(void);

LONG GetCurrentUsedWorkDynamicMemory(void);

LONG GetFailedAllocRequestCount(void);

LONG GetOSShortTermAllocTag(void);

LONG GetTotalWorkDynamicMemory(void);

void CAllocAddMemory(
		void *memory,
		LONG length);

/* End of routines in the CSPACE module */
/****************************************************************************/
/* Routines in the SMPSHIM module */

LONG GetNumberOfRegisteredProcessors(void);

LONG get_cpu_time(void);

/* End of routines in the SMPSHIM module */
/****************************************************************************/
/* Routines in the SPX module */

void SPXInit(void);

LONG CSPXAbortConnection(
		LONG sessionID);

LONG CSPXCancelECB(
		void *eventControlBlock);

LONG CSPXCancelSessionListen(
		void *eventControlBlock);

LONG CSPXCheckInstallation(
		WORD *version,
		LONG *maxConnections);

LONG CSPXEstablishConnection(
		BYTE retryCount,
		BYTE watchDog,
		LONG *sessionID,
		void *eventControlBlock);

LONG CSPXGetConnectionStatus(
		LONG sessionID,
		void *buffer);

LONG CSPXGetConnectionStatus2(
		LONG sessionID,
		void *buffer,
		LONG bufferSize );

void CSPXGetTimersAndCounters(
		int *abortTimeout,
		int *listenTimeout,
		int *verifyTimeout,
		int *retryCount,
		int *configuredSessions,
		int *openSessions);

LONG CSPXListenForConnectedPacket(
		void *eventControlBlock,
		LONG sessionID);

LONG CSPXListenForConnection(
		BYTE retryCount,
		BYTE watchDog,
		LONG *sessionID,
		void *eventControlBlock);

LONG CSPXListenForSequencedPacket(
		void *eventControlBlock);

LONG CSPXSendSequencedPacket(
		LONG sessionID,
		void *eventControlBlock);

LONG CSPXSetTimersAndCounters(
		int abortTimeout,
		int listenTimeout,
		int verifyTimeout,
		int retryCount,
		int configuredSessions);

LONG CSPXTerminateConnection(
		LONG sessionID,
		void *eventControlBlock);

/* End of routines in the SPX module */
/****************************************************************************/
/* Routines in the STATS module */

void ReCalibrateUtilization(void);

LONG GetHistogramPublics(
		LONG (**ActivateHistogram)(
				struct ResourceTagStructure *RTag,
				void **Vector,
				LONG *VectorSize),
		LONG (**DeActivateHistogram)(
				void *Vector,
				struct ResourceTagStructure *RTag),
		LONG (**GetCurrentHistoryVectorPosition)(void),
		void (**GetOverheadValues)(
				LONG *ExtraTaskSwitchOverheadValue,
				LONG *ExtraInterruptOverheadValue,
				LONG *ExtraPreAndPostExceptionOverheadValue,
				LONG *ExtraFSEnginePollerOverheadValue)
);

/* End of routines in the STATS module */
/****************************************************************************/
/* Routines in the SWITCH0 module */

LONG	NCPBoundaryError(
		LONG Station,
		LONG ncpFunction,
		LONG ncpSubFunction);

LONG	NCPComponentError(
		LONG Station,
		LONG ncpFunction,
		LONG ncpSubFunction);


/* End of routines in the SWITCH0 module */
/****************************************************************************/
/* Routines in the SWITCH1 module */

LONG RelOpenCallback(LONG station, LONG task, LONG handle);
int AckOpenCallback(LONG station, LONG task, LONG handle);
int DeclineOpenCallback(LONG station, LONG task, LONG handle);


LONG CopyFileToFile(
		LONG Station,
		LONG SourceHandle,
		LONG TargetHandle,
		LONG SourceOffset,
		LONG TargetOffset,
		LONG BytesToCopy,
		LONG *TotalBytesCopied);

void StopCopyFileToFile(
		LONG Station);

/* End of routines in the SWITCH1 module */
/****************************************************************************/
/* Routines in the TABLES module */

LONG GetCurrentNumberOfTransactions(void);

LONG GetFileServerMajorVersionNumber(void);

LONG GetFileServerMinorVersionNumber(void);

LONG GetFileServerRevisionNumber(void);

LONG GetIOEngineMajorVersionNumber(void);

LONG GetIOEngineMinorVersionNumber(void);

LONG GetIOEngineRevisionNumber(void);

LONG ReturnFileServerName(
		BYTE *nameBuffer);

LONG ReturnIOEngineName(
		BYTE *nameBuffer);

LONG ReturnOtherIOEngineName(
		BYTE *nameBuffer);

LONG ReturnIOInternalNetNumber(void);

LONG ReturnOtherIOInternalNetNumber(void);

LONG GetPeakNumberOfTransactions(void);

LONG GetQueueingVersionNumber(void);

LONG GetSecurityRestrictionsLevel(void);

LONG GetSFTLevel(void);

LONG GetTTSLevel(void);

LONG GetVAPVersionNumber(void);

LONG GetVirtualConsoleVersionNumber(void);

LONG GetStationsInUseCount(void);

LONG GetPeakStationsInUseCount(void);

LONG GetNumberOfPollingLoops(void);

LONG GetMaximumNumberOfPollingLoops(void);

/* End of routines in the SWITCH1 module */
/****************************************************************************/
/* Routines in the TERMIO module */

void PositionRawCursor(
		WORD inputCursorPosition);

void EnableRawCursor(void);

void DisableRawCursor(void);

void RingTheBell(void);

void WaitForKey(
		struct ScreenStruct *screenID);

LONG UngetKey(
		struct ScreenStruct *screenID,
		BYTE keyType,
		BYTE keyValue,
		BYTE keyStatus,
		BYTE scanCode);

/* End of routines in the TERMIO module */
/****************************************************************************/
/* Routines in the TIMER module */

LONG GetHighResolutionTimer(void);

LONG GetSuperHighResolutionTimer(void);

void GetTimeAndDateVector(
		BYTE *TimeDateVector);

void DelayMyself(				/* this is the NEW call. */
		LONG timeInTicks,
		struct ResourceTagStructure *TimerResourceTag);

void StopBell(void);

void SetClock(void);

void GetClock(void);

void CScheduleInterruptTimeCallBack(
		struct TimerDataStructure *TimerNode);

void CCancelInterruptTimeCallBack(
		struct TimerDataStructure *TimerNode);

LONG GetCurrentTime(void);

/* End of routines in the TIMER module */
/****************************************************************************/
/* Routines in the SYMDEB module */

void CryptSymbolStruct(
		void *symbol);

/* End of routines in the SYMDEB module */
/****************************************************************************/
/* Routines in the SynClock module */

LONG OStime(
		LONG *timer);

void OSTZSet(void);

LONG GetCurrentClock(
		clockAndStatus dataPtr);

void GetSyncClockFields(
		LONG bitMap,
		Synchronized_Clock_T *aClock);

void GetTimeZone(
		BYTE *nameBuffer);

void	InitializeSyncClockEventProcess(void);

// DEFECT194885 Jim A. Nicolet 8-24-2000
// void	OSScanForTimeOffset(char **ch, long *tz);
int OSScanForTimeOffset(char **ch, long *tz);

void ProcessSetStartOfDST(void);

void ProcessSetEndOfDST(void);

LONG SetLocalTimeVectorFromUTC(
		LONG UTC,
		BYTE *timeVector);

void	SetLocalTimeFromUTC(void);

void SetSyncClockFields(
		LONG bitMap,
		Synchronized_Clock_T *aClock);

void SetTimeZone(
		BYTE *nameBuffer);

void SetUTCTimeFromLocal(void);

void SyncClockInitialize(void);

void RecalculateDST(void);

/* End of routines in the SynClock module */
/****************************************************************************/
/* Routines in the UPRELUDE module */

LONG GetLoaderSupportedTypes(void);

LONG GetLoaderType(void);

void KillInitProcess(void);

void ExitToDOS(LONG ExitCompletionCode);

LONG CheckAndAddMemory(
		LONG startAddress,
		LONG length);

LONG TotalKnownSystemMemory(void);

LONG GetMemoryNode(
		struct MemoryStructure *memoryNode,
		LONG previousStart,
		LONG typeBits);

LONG GetSystemMemoryMap(
		LONG *ArrayOfOffsetLengths,
		LONG ArraySize);

LONG INWDOSGetFileSize(LONG handle, LONG *size);

void INWDOSDiskReset(
		LONG Drive);

LONG INWDOSOpen(
		BYTE *fileName,
		LONG *handle);

LONG DOSOpenWithAccess(
		BYTE *fileName,
		LONG *handle,
		LONG Access);

LONG INWDOSCreate(
		BYTE *fileName,
		LONG *handle);

LONG INWDOSCreateDirectory(
		BYTE *DirName);

LONG INWDOSRemoveDirectory(
		BYTE *DirName);

LONG DOSCreateWithAttributes(
		BYTE *fileName,
		LONG *handle,
		LONG attributes);

LONG INWDOSClose(
		LONG handle);

void INWDOSShutOffFloppyDrive(void);

LONG INWDOSRead(
		LONG handle,
		LONG offset,
		void *buffer,
		LONG numberOfBytes,
		LONG *bytesRead);

LONG INWDOSWrite(
		LONG handle,
		LONG offset,
		void *buffer,
		LONG numberOfBytes,
		LONG *bytesRead);

LONG INWDOSFindFirstFile(
		BYTE *fileName,
		WORD attributes,
		void *DTA);

LONG INWDOSFindNextFile(
		void *DTA);

LONG INWDOSUnlink(
		BYTE *fileName);

LONG INWDOSRename(
		BYTE *sourceFileName,
		BYTE *targetFileName);

LONG INWDOSLSeek(
		LONG fileHandle,
		LONG offset,
		LONG method,
		LONG *newPosition);

LONG INWDOSGetDefaultDrive(void);
LONG INWDOSSetDefaultDrive(BYTE driverNumber);

LONG INWDOSGetCurrentDirectory(
		LONG driveNumber,
		BYTE *buffer);

LONG INWDOSSetCurrentDirectory(BYTE *buffer);

LONG INWDOSIsFloppy(
		LONG driveNumber,
		LONG *isFloppy);

LONG INWDOSIsNetwork(
		LONG driveNumber,
		LONG *isFloppy);

LONG INWDOSGetDriveType(
		LONG driveNumber,
		LONG *driveType);

LONG INWDOSSetDateAndTime(
		LONG fileHandle,
		LONG date,
		LONG time);

LONG INWDOSGetDateAndTime(
		LONG fileHandle,
		LONG *date,
		LONG *time);

LONG INWDOSChangeFileMode(
		BYTE *name,
		LONG *returnedAttribute,
		LONG function, 					/* 0 - read attrib; 1 - set attrib */
		LONG newAttribute);

LONG BIOSReadDriveParameters(
		LONG *sectors,
		LONG *heads,
		LONG *cylinders,
		LONG *type);

LONG BIOSReadSectors(
		LONG AX,	/* AH = request type (2), AL = number of sectors */
		void *buffer,
		LONG CX,	/* CH = cylinder, CL = sector */
		LONG DX);	/* DH = head, DL = drive (00h-7Fh Floppy, 80h-FFh Fixed Disk */

LONG BIOSWriteSectors(
		LONG AX,	/* AH = request type (3), AL = number of sectors */
		void *buffer,
		LONG CX,	/* CH = cylinder, CL = sector */
		LONG DX);	/* DH = head, DL = drive (00h-7Fh Floppy, 80h-FFh Fixed Disk */

LONG SetExceptionHandler(
		BYTE ExceptionNumber,
		void (*ExceptionHandler)(void),
		struct ResourceTagStructure *RTag,
		BYTE PositionFlag,
		BYTE ShareFlag);

LONG ClearExceptionHandler(
		BYTE ExceptionNumber,
		void (*ExceptionHandler)(void));

void CAdjustRealModeInterruptMask(
		BYTE Level);

void CUnAdjustRealModeInterruptMask(
		BYTE Level);

LONG GetRealModeInterruptMask(
		void);

//lint -save -e18
LONG DoRealModeInterrupt(
		void *InputParameters,
		void *OutputParameters);

LONG DoRealModeInterruptDisabled(
		void *InputParameters,
		void *OutputParameters);

LONG DoRealModeInterrupt32(
		void *InputParameters32,
		void *OutputParameters32);

LONG DoRealModeInterrupt32Disabled(
		void *InputParameters32,
		void *OutputParameters32);

LONG DoRealModeFarCall(
		void *InputParameters32,
		void *OutputParameters32);

LONG DoRealModeFarCallDisabled(
		void *InputParameters32,
		void *OutputParameters32);

//lint -restore

void GetRealModeWorkSpace(
		struct SemaphoreStructure **workSpaceSemaphore,
		LONG *protectedModeAddressOfWorkSpace,
		WORD *realModeSegmentOfWorkSpace,
		WORD *realModeOffsetOfWorkSpace,
		LONG *workSpaceSizeInBytes);

LONG MapAbsoluteAddressToDataOffset(
		LONG AbsoluteAddress);

LONG MapAbsoluteAddressToCodeOffset(
		LONG AbsoluteAddress);

LONG MapDataOffsetToAbsoluteAddress(
		LONG DataOffset);

LONG MapCodeOffsetToAbsoluteAddress(
		LONG CodeOffset);

struct LoadDefinitionStructure *CFindLoadModuleHandle(
		void *CodeAddress);

struct LoadDefinitionStructure *CFindLoadModuleHandleNoAbend(
		void *CodeAddress);

LONG GetNestedInterruptLevel(void);

void IncNestedInterruptLevel(void);

void DecNestedInterruptLevel(void);

LONG IsDOSPresent(void);

LONG RawHookINT7(
		void (*Handler)());

LONG RawUnHookINT7(
		void (*Handler)());

LONG GetServerPhysicalOffset(void);

LONG GetSharedMemoryLinearAddress(
		LONG sharedMemoryPhysicalAddress,
		LONG size);

struct ProcessorStructure *GetProcessorID(void);

/* End of routines in the UPRELUDE module */
/****************************************************************************/
/* Routines in the UTIL module */

/* CMoveFast is optimized to prevent cache polution on the new Katmai and
 * Tanner processors; it uses both a prefetch and a streaming write.
 * It also runs at up to 300% faster depending on cacheing and alignment
 * conditions.
 */
void CMoveFast(
		void *sourceAddress,
		void *destinationAddress,
		LONG numberOfBytes);

void INWFastMove(void);

/* CMoveFastCache is like CMoveFast except that it doesn't use the streaming
 * write.  This places the target data in the processors cache rather that
 * just out in main memory. */
void CMoveFastCache(
		void *sourceAddress,
		void *destinationAddress,
		LONG numberOfBytes);

void INWFastMoveCache(void);

LONG BytesToBlocks(
		BYTE *sixByteBuffer);

LONG GetProcessSwitchCount(void);

LONG GetHardwareBusType(void);

LONG GetUnmaskedHardwareBusType(void);

#ifndef _NO_NETWARE_FLAG_INLINES_
void Enable(void);

void Disable(void);

LONG DisableAndRetFlags(void);

LONG EnableAndRetFlags(void);

void CheckForDisabled(void);

LONG RetFlags(void);

void SetFlags(
		LONG flag);
#endif

void Abend(
		void *abendDescription);

void LocalAbend(
		void *abendDescription);

void CSetB(
		BYTE value,
		void *address,
		LONG numberOfBytes);

void CSetW(
		WORD value,
		void *address,
		LONG numberOfWords);

void CSetD(
		LONG value,
		void *address,
		LONG numberOfDWords);

void CMovB(
		void *sourceAddress,
		void *destinationAddress,
		LONG numberOfBytes);

void CMovW(
		void *sourceAddress,
		void *destinationAddress,
		LONG numberOfWords);

void CMovD(
		void *sourceAddress,
		void *destinationAddress,
		LONG numberOfDWords);

void CMovDBackwards(
		void *sourceAddress,
		void *destinationAddress,
		LONG numberOfDWords);

void CSwapMemD(
		void *address1,
		void *address2,
		LONG numberOfDWords);

LONG CFindB(
		BYTE value,
		void *address,
		LONG numberOfBytes);

LONG CFindD(
		LONG value,
		void *address,
		LONG numberOfDWords);

LONG CFindW(
		WORD value,
		void *address,
		LONG numberOfWords);

LONG CNFindB(
		BYTE value,
		void *address,
		LONG numberOfBytes);

LONG CNFindD(
		LONG value,
		void *address,
		LONG numberOfDWords);

LONG CCmpB(
		void *address1,
		void *address2,
		LONG numberOfBytes);

LONG ICmpB(
		void *address1,
		void *address2,
		LONG numberOfBytes);

LONG CCmpD(
		void *address1,
		void *address2,
		LONG numberOfDWords);

LONG CStrLen(
		void *string);

void CStrCpy(
		void *destinationString,
		void *sourceString);

int CStriCmp(
		void *string1,
		void *string2);

int CStrCmp(
		void *string1,
		void *string2);

int LStrCmp(
		void *string1,
		void *string2);

void LStrCpy(
		void *destinationString,
		void *sourceString);

LONG GetLong(
		void *sourceLong);

WORD NShort(
		void *sourceWord);

LONG NLong(
		void *sourceLong);

LONG InvertLong(
		LONG longValue);

#if !InLineAssemblyEnabled
WORD InvertShort(
		WORD wordValue);

#endif

WORD GetShort(
		void *sourceWord);

void PutShort(
		WORD value,
		void *address);

void PutLong(
		LONG value,
		void *address);

LONG BitTest(
		void *address,
		LONG bitIndex);

void BitClear(
		void *address,
		LONG bitIndex);

void BitSet(
		void *address,
		LONG bitIndex);

LONG BitTestAndSet(
		void *address,
		LONG bitIndex);

LONG BitTestAndClear(
		void *address,
		LONG bitIndex);

LONG ScanBits(
		void *address,
		LONG startingBitIndex,
		LONG countToScan);

LONG ScanClearedBits(
		void *address,
		LONG startingBitIndex,
		LONG countToScan);

void OSWordSwap(
		WORD *wordArray,
		LONG arrayLength);

void OSLongSwap(
		LONG *longArray,
		LONG arrayLength);

void ConvertTicksToSeconds(
		LONG ticks,
		LONG *seconds,
		LONG *tenthsOfSeconds);

void ConvertSecondsToTicks(
		LONG seconds,
		LONG tenthsOfSeconds,
		LONG *ticks);

void NullCheck(
		BYTE *String);

void *GetPCBDirectoryServicesStuff( struct PCBStructure *pcb );

void SetPCBDirectoryServicesStuff( struct PCBStructure *pcb,
				   void *DirectoryServicesStuff);

void *GetPCBStackLimit( struct PCBStructure *pcb );

LONG GetProcessorSpeedRating(void);

LONG GetMaximumNumberOfVolumes(void);

LONG GetMaximumSubdirectoryTreeDepth(void);

LONG GetMaximumNumberOfMLIDBoards(void);

LONG GetMaximumNumberOfNameSpaces(void);

LONG ScanBitBlocks(
		BYTE *bitmap,
		LONG beginningbit,
		LONG endingbit,
		LONG numberofbits);

LONG NWSetProcessData(
					LONG processID,
					LONG offset,
					LONG size,
					void *data);

LONG NWGetProcessData(
					LONG processID,
					LONG offset,
					LONG size,
					void *data);

LONG NWSetCLIBDefinedData(
					LONG processID,
					LONG data1,
					LONG data2,
					LONG data3);

LONG NWSetCLIBLoadStructure(
					LONG moduleHandle,
					LONG CLIBLoadStructure);

void SetLoadRecordFlagBit(
					LONG moduleHandle,
					LONG LDFlags);

void ClearLoadRecordFlagBit(
					LONG moduleHandle,
					LONG LDFlags);

void IncRTagCount(LONG RTag);

void DecRTagCount(LONG RTag);

LONG NWSetScreenStructEntryValue(struct ScreenStruct *screenID,
					LONG offset,
					LONG value,
					LONG	size);

LONG NWIsNLMLoadedProtected(void);


/* End of routines in the UTIL module */
/****************************************************************************/
/* Routines in the VERSION module */

void InitializeVersionStrings(void);

void DisplayProductName(
		struct ScreenStruct *screenID);

void DisplayServerVersion(
		struct ScreenStruct *screenID);

void InternalDisplayServerVersion(
		struct ScreenStruct *screenID,
		BYTE debuggerActiveFlag,
		LONG *linesUsed);

void GetCompanyName(
		BYTE *nameBuffer);

void GetSoftwareRevision(
		BYTE *nameBuffer);

void GetCopyrightNotice(
		BYTE *nameBuffer);

void GetPatentNotice(
		BYTE *nameBuffer);

void GetSoftwareDate(
		BYTE *nameBuffer);

LONG GetBinderyOpen(void);

LONG GetIPXVersion(void);

LONG GetFSEngineInternalNetNumber(void);

LONG GetIPXNetNumberTableEntry(
		LONG boardNumber);

/* End of routines in the VERSION module */
/****************************************************************************/
/*
	Special display routines in the SYSALERT module for Licensing - CDR 10/92
*/
extern void AESLicenseErrorAlert ( void *AESLicenseErrorInfo );

extern void DisplayLicenseInfoAESProcedure (	void *displayInfo );

void InitializeServerSerialNumberCheck(void);

/****************************************************************************/

ERROR	AddModuleToLoadedList(struct LoadDefinitionStructure*);
void		AddNLMToAddressSpace(struct LoadDefinitionStructure*);
void		*AllocInitialize(struct LoadDefinitionStructure*, UINT mode);
void		AllocTerminate(struct LoadDefinitionStructure*);
void		CAddDebuggerSymbol(LONG symbolValue, BYTE *symbolName, LONG moduleHandle);
void		CRemoveDebuggerSymbol(LONG moduleHandle);
void		RemoveNLMFromAddressSpace(struct LoadDefinitionStructure*);
struct LoaderExtensionStructure*RegisterLoaderExtension(struct ResourceTagStructure*,
		LONG(*handler)(struct ScreenStruct*,
			LONG hFile,
			LONG(*readRoutine)(LONG, LONG, void*, LONG),
			LONG options,
			BYTE*moduleName,
			BYTE*loadPath,
			BYTE*commandLine,
			struct AddressSpace*),
		LONG*ldSize,
		LONG*ldOffset,
		struct KernelUserAddressPair *readRoutines);

void		*TranslateLoaderExtensionPointer(void*userAddress);

LONG		UnRegisterLoaderExtension(struct LoaderExtensionStructure*);


/****************************************************************************/
/*lint -restore */

#endif /* __PROCDEFS_H__ */
