#define NTCLIENTECO 1
#define DEFECT265394 1

/*  compression data exports */
extern BYTE StopCompressionTable[];
extern LONG CompressMinimumPercentageGain;
extern LONG CompressionLowerLimit[];
extern LONG DeletedFileSemaphoreTable[];

extern BYTE TTSState;

/****************************************************************************/
/* Routines in the DIRC module */

LONG SetVolumeObjectID(
		LONG Volume,
		LONG NDSObjectID);

LONG CopyCacheBuffer(
		LONG Volume,
		LONG Sector,
		void *Buffer);

LONG GetMirroredSectorNumber(
		LONG Volume,
		LONG Sector,
		LONG *NewSector);

void InitializeDirectoryTables(
		LONG Volume,
		LONG UnitSize,
		LONG DirectoryChain,
		LONG *SectorList,
		LONG *LastAllocated);

void ReturnAllDirectoryTables(
		LONG Volume);

LONG ExpandNameSpaceDirectoryTables(
		LONG Volume,
		LONG NewDirectorySize);

BYTE *InitializeVolumeDirectoryStructures(
		struct ScreenStruct *screenID,
		LONG Volume,
		LONG DirectoryChain0,
		LONG DirectoryChain1,
		struct ProblemListHeaderStructure *ProblemHeader);

LONG DOSCheckNameIgnoreLocks(
		LONG Volume,
		LONG Subdirectory,
		BYTE *NameAndLength,
		LONG *entryNumber);

LONG GetDirectoryInformation(
		LONG Volume,
		LONG *TotalDirectoryEntries,
		LONG *AvailableDirectoryEntries);

LONG GetDiskUtilization(
		LONG UserID);

LONG	SetVolumeFlags(
			LONG	Volume,
			BYTE	NewFlagBit);

LONG	ClearVolumeFlags(
			LONG	Volume,
			BYTE	NewFlagBit);

/* End of routines in the DIRC module */
/****************************************************************************/
/* Routines in the DELFILE module */

LONG DeleteAllHardLinkedFilesInADirectory(
		LONG Volume,
		LONG EntryNumber);

LONG DeleteHardLinkedFile (
		LONG Volume,
		LONG DOSEntryNumber,
		struct DirectoryStructure *DOSDir);

LONG DeleteHardLinkedDirectory (
		LONG Volume,
		LONG DOSEntryNumber,
		struct SubdirectoryStructure *DOSDir);

LONG DeleteFileCompletely(
		LONG Volume,
		LONG DirectoryEntryNumber,
		LONG Station,
		struct DirectoryStructure *DOSDir);

LONG DeleteFileToLimbo(
		LONG Volume,
		LONG DirectoryEntryNumber,
		LONG Station,
		struct DirectoryStructure *DOSDir);

void FreeALimboFile(
		LONG Volume,
		BYTE NeedNoSynchronizationFlag,
		BYTE FreeSomethingflag);

LONG SalvageLimboFile(
		LONG Volume,
		LONG SubdirectoryNumber,
		LONG DirectoryEntry,
		struct DirectoryStructure *Dir,
		BYTE *NewNameAndLength,
		LONG NameSpace,
		LONG NewCreatorID);

LONG PurgeLimboFile(
		LONG Volume,
		LONG SubdirectoryNumber,
		LONG DirectoryEntry,
		struct DirectoryStructure *DeletedFile);

LONG ChangeDirectory(
		LONG Volume,
		LONG NewDirectoryEntryNumber,
		LONG FirstDirectoryBlockNumber);

LONG MoveDeletedFiles(
		LONG Volume,
		LONG NewDirectoryEntryNumber,
		LONG FirstDirectoryBlockNumber,
		LONG TransferTrusteeRightsFlag);

/* End of routines in the DELFILE module */
/****************************************************************************/
/* Routines in the DIRCACHE module */

void UnLockPrimaryAndNameSpaceEntry(LONG Volume, LONG DOSEntry, LONG NameSpaceEntry);

LONG LockAndGetDirectoryEntry(LONG Volume, LONG DirectoryEntry, void *EntryPointer);

void MapExternalToInternalDirNumber(
		LONG Volume,
		LONG *ExternalDirectoryNumber);

void MapInternalToExternalDirNumber(
		LONG Volume,
		LONG *InternalDirectoryNumber);

void DeleteDirNumber(
		LONG Volume,
		LONG DirectoryNumber);

void ChangeDirNumber(
		LONG Volume,
		LONG OldDirectoryNumber,
		LONG NewDirectoryNumber);

LONG CaseInsensitiveCompareStrings(
		BYTE *String1,
		BYTE *String2);

BYTE *FindLastComponent(
		BYTE *componentString,
		LONG componentCount);

LONG DOSCheckAndConvertName(
		BYTE *fileNamePointer,
		BYTE *convertedFileNamePointer);

void DOSUnConvertName(
		BYTE *convertedfileNamePointer,
		BYTE *unConvertedFileNamePointer);

// DEFECT 238120 Jim A. Nicolet
/*----------------------------**
** BYTE *VerifyOnlyDOSName(   **
** 		LONG Volume,         **
** 		union DirUnion *Dir, **
** 		LONG EntryNumber);   **
**----------------------------*/
BYTE *VerifyDOSNameOnly(
		LONG Volume,
		union DirUnion *Dir,
		LONG EntryNumber,
		struct ProblemListHeaderStructure *ProblemHeader);

LONG DOSFindDirectoryEntry(
		LONG volumeNumber,
		LONG directoryEntry,
		BYTE *pathAndName,
		LONG componentCount,
		LONG stationNumber,
		struct DirectoryInfoReturn *directoryInfo);

LONG DOSFindDirectoryEntryOrPhantom(
		LONG volumeNumber,
		LONG directoryEntry,
		BYTE *pathAndName,
		LONG componentCount,
		LONG stationNumber,
		struct DirectoryInfoReturn *directoryInfo,
		LONG directoryEntryNameSpace);

LONG GenerateDirectoryHandle(
		LONG volumeNumber,
		struct DirectoryInfoReturn *directoryInfo,
		LONG stationNumber,
		struct DirectoryHandleStructure **directoryHandle);

void ReturnDirectoryHandle(
		LONG station,
		struct DirectoryHandleStructure *directoryHandle );

void ClearStationDirectoryHandles(
		LONG stationNumber);

void ClearVolumeDirectoryHandles(
		LONG volumeNumber);

LONG GetFileAccessRights(
		LONG Volume,
		LONG Station,
		struct DirectoryStructure *Dir,
		LONG DirectoryEntryNumber,
		struct DirectoryHandleStructure *directoryHandle,
		WORD *AccessRights);

LONG GetSubDirAccessRights(
		LONG Volume,
		LONG Station,
		struct SubdirectoryStructure *SubDir,
		LONG DirectoryEntryNumber,
		struct DirectoryHandleStructure *directoryHandle,
		WORD *AccessRights);

void TrusteeChanged(
		LONG volumeNumber,
		LONG trusteeID);

void SubdirectoryAttributesChanged(
		LONG volumeNumber,
		LONG directoryNumber,
		LONG newAttributes);

/* 8-17-94 Jim -- DO NOT EXPORT DOSGetDirectoryHandle */
LONG DOSGetDirectoryHandle(
		LONG volumeNumber,
		LONG directoryNumber,
		LONG stationNumber,
		struct DirectoryHandleStructure **directoryHandle);

/* 8-17-94 Jim -- This is now the exported API instead of
	DOSGetDirectoryHandle
*/
LONG DOSGetInternalDirectoryHandle(
		LONG volumeNumber,
		LONG directoryNumber,
		LONG stationNumber,
		struct DirectoryHandleStructure **directoryHandle);

LONG GetDirectoryEntry(
		LONG volumeNumber,
		LONG directoryEntry,
		void *directoryEntryPointer);

LONG GetDirectoryEntryNoSleep(
		LONG volumeNumber,
		LONG directoryEntry,
		void *directoryEntryPointer);

LONG GetDirectoryEntryAndSleepStatus(
		LONG volumeNumber,
		LONG directoryEntry,
		void *directoryEntryPointer,
		BYTE *SleepStatus);

LONG ImmediateReuseGetDirectoryEntry(
		LONG volumeNumber,
		LONG directoryEntry,
		void *directoryEntryPointer);

LONG MarkDirectoryEntryChanged(
		LONG volumeNumber,
		LONG directoryEntry);

void LockDirectoryEntry(
		LONG volumeNumber,
		LONG directoryEntry);

BYTE CheckLockDirectoryEntry(
		LONG volumeNumber,
		LONG directoryEntry);

void UnLockDirectoryEntry(
		LONG volumeNumber,
		LONG directoryEntry);

LONG CheckDirectoryEntryLock(
		LONG volumeNumber,
		LONG directoryEntry);

void LockDirectoryBlock(
		LONG volumeNumber,
		LONG directoryEntry);

void UnLockDirectoryBlock(
		LONG volumeNumber,
		LONG directoryEntry);

void UnLockDirectoryBlockLockEntry(
		LONG volumeNumber,
		LONG directoryEntry);

LONG AllocateDirectoryEntry(
		LONG volumeNumber,
		LONG firstDirectoryBlock,
		struct DirectoryStructure **directoryEntryPtr,
		LONG *directoryEntry);

LONG AllocateDirectoryEntrySkipMac(
		LONG volumeNumber,
		LONG firstDirectoryBlock,
		struct DirectoryStructure **directoryEntryPtr,
		LONG *directoryEntry);

LONG AllocateDeletedDirectoryEntry(
		LONG volumeNumber,
		struct DirectoryStructure **directoryEntryPtr,
		LONG *directoryEntry);

LONG CountHashIndex(
		LONG volumeNumber);

LONG CountHashLinks(
		LONG volumeNumber);

void AddToHash(
		LONG volumeNumber,
		LONG entryNumber);

void AddToHashFromExternal(
		LONG volumeNumber,
		LONG entryNumber,
		struct DirectoryStructure *directoryEntry);

void AddToHashUnlockAndMarkDirectory(
		LONG volumeNumber,
		LONG entryNumber);

void AddToHashAndMarkDirectory(
		LONG volumeNumber,
		LONG entryNumber);

LONG GenerateHashValue(
		LONG volumeNumber,
		LONG subdirectoryNumber,
		BYTE *nameLengthAndString);

void DeleteFromHash(
		LONG volumeNumber,
		LONG entryNumber);

void FreeDirectoryEntry(
		LONG volumeNumber,
		LONG directoryEntry);

void GiveBackAllocatedDirectoryEntry(
		LONG volumeNumber,
		LONG directoryEntryNumber);

void MarkDirectoryChanged(
		LONG volumeNumber,
		LONG subdirectoryNumber);

LONG DOSWildSearchDirectory(
		LONG volumeNumber,
		LONG directoryNumber,
		LONG currentEntryNumber,
		BYTE *patternString,
		LONG stationNumber,
		union DirUnion **entryPointer,
		LONG *entryNumber,
		WORD *accessRights);

LONG DOSWildSearchDirectoryFromInfo(
		LONG volumeNumber,
		struct DirectoryInfoReturn *directoryInfo,
		LONG stationNumber);

LONG DOSWildMatch(
		BYTE *pattern,
		BYTE *name);

LONG DOSGetDirectoryPath(LONG volumeNumber,
		LONG directoryEntryNumber,
		BYTE *string,
		LONG stringLength,
		LONG *returnedStringLength);

LONG DOSGetPath(
		LONG volumeNumber,
		LONG directoryEntry,
		BYTE *string,
		LONG stringLength,
		LONG *returnedStringLength);

LONG AddSubdirectory(
		LONG volumeNumber,
		LONG directoryEntry,
		LONG myParentsFirstDirectoryBlock);

LONG RemoveSubdirectory(
		LONG volumeNumber,
		LONG directoryEntryNumber);

void DeleteDirectoryHandles(
		LONG volumeNumber,
		LONG oldSubdirectoryEntryNumber);

void CalculateDirectoryUsedCount(
		LONG volumeNumber);

void DirectoryCacheUpdateProcess(void);

LONG AllocateDirectoryTables(
		LONG volumeNumber,
		LONG directoryLength,
		LONG volumeAllocationUnit);

void ReturnDirectoryTables(
		LONG volumeNumber);

struct SpaceStructure *AllocateSpaceRestrictionNode(void);

struct TrusteeListStructure *GetTrusteeListNode(
		LONG Volume,
		LONG Trustee);

LONG AddToTrusteeList(
		LONG Volume,
		LONG DirectoryEntryNumber,
		LONG Trustee);

void DeleteFromTrusteeList(
		LONG Volume,
		LONG DirectoryEntryNumber,
		LONG Trustee);

LONG CheckBlockForTrustee(
		LONG Volume,
		LONG BlockNumber,
		LONG Trustee);

LONG CheckTNodeBlockForTrustee(
		LONG Volume,
		LONG BlockNumber,
		LONG Trustee);

LONG MapSubdirectoryToDirectory(
		LONG Volume,
		LONG SubdirectoryEntryNumber);

struct SpaceStructure *MapSubdirectoryToSpaceNode(
		LONG Volume,
		LONG SubdirectoryEntryNumber);

LONG MapSubdirectoryToFirstBlock(
		LONG Volume,
		LONG SubdirectoryEntryNumber);

LONG CheckForAnyRightsForStation(
		LONG Volume,
		LONG FirstDirectoryBlock,
		LONG Station);

LONG GetSectorFromDirectoryCache(
		LONG volume,
		LONG sector,
		void *Buffer);

void FlushDirectoryCache(
		LONG volumeNumber);

/* End of routines in the DIRCACHE module */
/****************************************************************************/
/* Routines in the DIRECTFS module */

LONG SetFileSize(
		LONG station,
		LONG handle,
		LONG filesize,
		LONG truncateflag);

LONG ReturnVolumeBlockInformation(
		LONG volumenumber,
		LONG startingblocknumber,
		LONG numberofblocks,
		BYTE *buffer);

LONG ReturnFileMappingInformation(
		LONG station,
		LONG handle,
		LONG startingblocknumber,
		LONG *numberofentries,
		LONG tablesize,
		struct FileMapStructure *table);

LONG ReturnVolumeMappingInformation(
		LONG volumenumber,
		struct VolumeInformationStructure *volumeInformation);

LONG FreeLimboVolumeSpace(
		LONG volumenumber,
		LONG numberofblocks);

LONG ExpandFileInContiguousBlocks(
		LONG station,
		LONG handle,
		LONG fileblocknumber,
		LONG numberofblocks,
		LONG vblocknumber,
		LONG segnumber);

LONG DirectReadFileNoWait(
		LONG station,
		LONG handle,
		LONG startingsector,
		LONG sectorcount,
		BYTE *buffer,
		void (*callbackroutine)(),
		LONG callbackparameter);

LONG DirectReadFile(
		LONG station,
		LONG handle,
		LONG startingsector,
		LONG sectorcount,
		BYTE *buffer);

LONG DirectWriteFileNoWait(
		LONG station,
		LONG handle,
		LONG startingsector,
		LONG sectorcount,
		BYTE *buffer,
		void (*callbackroutine)(),
		LONG callbackparameter);

LONG DirectWriteFile(
		LONG station,
		LONG handle,
		LONG startingsector,
		LONG sectorcount,
		BYTE *buffer);

LONG AllocateContiguousFileSectors(
		LONG station,
		LONG handle,
		LONG startingsector,
		LONG sectorcount,
		LONG segmentnumber);

LONG DirectReadFileNoWait(
		LONG station,
		LONG handle,
		LONG startingsector,
		LONG sectorcount,
		BYTE *buffer,
		void (*callbackroutine)(),
		LONG callbackparameter);

LONG DirectReadFile(
		LONG station,
		LONG handle,
		LONG startingsector,
		LONG sectorcount,
		BYTE *buffer);

LONG DirectWriteFileNoWait(
		LONG station,
		LONG handle,
		LONG startingsector,
		LONG sectorcount,
		BYTE *buffer,
		void (*callbackroutine)(),
		LONG callbackparameter);

LONG DirectWriteFile(
		LONG station,
		LONG handle,
		LONG startingsector,
		LONG sectorcount,
		BYTE *buffer);

/* End of routines in the DIRECTFS module */
/****************************************************************************/
/* Routines in the DOSNAME module */

LONG DOSCheckNameIgnoreLocks(
		LONG Volume,
		LONG Subdirectory,
		BYTE *NameAndLength,
		LONG *entryNumber);

LONG DOSSetEntryName(
		LONG Volume,
		LONG EntryNumber,
		BYTE *FileName,
		union DirUnion *Dir);

LONG DOSRenameEntryName(
		LONG Volume,
		LONG EntryNumber,
		BYTE *FileName,
		union DirUnion *Dir);

LONG DOSSetDeletedFileEntryName(
		LONG Volume,
		LONG EntryNumber,
		BYTE *FileName,
		union DirUnion *Dir);

void DOSSetRestOfFileEntry(
		union DirUnion *Dir,
		LONG CreatedAttributes,
		LONG UserID,
		LONG CreateDOSDateAndTime,
		LONG CreateInSeconds,
		LONG LastArchivedDOSDateAndTime,
		LONG LastArchivedInSeconds);

void DOSRemoveEntryName(
		LONG Volume,
		LONG EntryNumber,
		union DirUnion *Dir,
		...);

void DOSRemoveOtherInfo(
		LONG Volume,
		LONG EntryNumber,
		union DirUnion *Dir);

void DOSRemoveInfoFromLimboFile(
		LONG Volume,
		LONG EntryNumber,
		union DirUnion *Dir);

void DOSGenerateUniqueName(
		LONG Volume,
		LONG Subdirectory,
		BYTE *SourceName,
		BYTE *UniqueName,
		BYTE UniquenessRequired,
		LONG SourceNameSpace);

LONG DOSMatchAttributes(
		LONG Volume,
		union DirUnion *Dir,
		LONG MatchBits);

LONG DOSGetSpaceUtilized(
		LONG Volume,
		union DirUnion *Dir);

void DOSChangeToPhantomEntry(
		LONG Volume,
		LONG EntryNumber,
		union DirUnion *Dir);

void DOSSetRestOfSubDirEntry(
		union DirUnion *SubDir,
		LONG DirectoryAccessMask,
		LONG UserID,
		LONG CreateDOSDateAndTime,
		LONG CreateInSeconds,
		LONG LastArchivedDOSDateAndTime,
		LONG LastArchivedInSeconds);

LONG DOSGetName(
		LONG volume,
		union DirUnion *Dir,
		BYTE *Name);

LONG DOSGetNameLen(
		LONG volume,
		union DirUnion *Dir,
		LONG *NameLen);

LONG DOSTransferInfo(
		LONG Volume,
		LONG OldEntryNumber,
		LONG NewEntryNumber,
		union DirUnion *OldDir,
		union DirUnion *NewDir,
		LONG OldUserID,
		LONG UserID);

LONG DOSTransferFileNameAndInfoToLimbo(
		LONG Volume,
		LONG OldEntryNumber,
		LONG NewEntryNumber,
		union DirUnion *OldDir,
		union DirUnion *NewDir,
		LONG OldUserID,
		LONG UserID);

LONG DOSTransferOnlyInfoToLimbo(
		LONG Volume,
		LONG OldEntryNumber,
		LONG NewEntryNumber,
		union DirUnion *OldDir,
		union DirUnion *NewDir,
		LONG OldUserID,
		LONG UserID);

LONG DOSTransferLimboInfoToFile(
		LONG Volume,
		LONG OldEntryNumber,
		LONG NewEntryNumber,
		union DirUnion *OldDir,
		union DirUnion *NewDir,
		LONG OldUserID,
		LONG UserID);

LONG *DOSGetDataStream(
		union DirUnion *Dir,
		LONG *FileSize);

LONG DOSExhumeLimboFile(
		LONG Volume,
		LONG EntryNumber,
		union DirUnion *Dir,
		LONG WhoDidItID,
		LONG *BlockCount);

void DOSChangeStreamsToLimbo(
		LONG Volume,
		LONG EntryNumber,
		union DirUnion *Dir,
		LONG OwnerID);

LONG DOSWildSearchDirectoryEntry(
		LONG volumeNumber,
		LONG directoryEntry,
		LONG StartEntryNumber,
		BYTE *pattern,
		LONG stationNumber,
		struct DirectoryInfoReturn *directoryInfo);

/* End of routines in the DOSNAME module */
/****************************************************************************/
/* Routines in the EASUPP module */

LONG CheckEAHandles(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG DirectoryNumber,
		LONG CloseFlag);

void DeleteEA(
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		LONG ExtendedAttributeNumber);

LONG EAVolumeMount(
		LONG Volume);

void EAVolumeDismount(
		LONG Volume);

void EAEndOfTask(
		LONG Station,
		LONG Task);

void ChangeEADirectoryNumber(
		LONG Volume,
		LONG EANumber,
		LONG OldDirectoryNumber,
		LONG NewDirectoryNumber);

/* End of routines in the EASUPP module */
/****************************************************************************/
/* Routines in the EAVPRIM module */

LONG QueryEADataSize(
		LONG Volume,
		LONG DirectoryNumber,
		LONG EANumber,
		LONG *runningTotal,
		LONG *runningCount,
		LONG *runningKey);

void DismountEAHandlesByVolume(
		LONG Volume);

void DeleteEAHandle(
		LONG Station,
		LONG Task,
		LONG clientID,
		struct ClientHandleStruct *client);

LONG ScanForEAHandle(
		LONG Station,
		LONG Task,
		LONG clientID,
		struct ClientHandleStruct **client);

LONG	CreateEAHandleForClient(
			LONG Station,
			LONG Task,
			LONG Volume,
			struct ClientHandleStruct **newClient);

/* End of routines in the EAVPRIM module */
/****************************************************************************/
/* Routines in the EAPRIM module */

LONG ClaimExtendedAttribute(
		LONG volume,
		LONG MyEntryNumber,
		LONG firstExtant);

void InitializeEASystem(void);

/* End of routines in the EAPRIM module */
/****************************************************************************/
/* Routines in the EXTDIR module */

LONG AllocateExtendedDirectorySpace(
		LONG Volume,
		LONG NumberOfExtants,
		LONG *StartingExtant);

LONG ReadExtendedDirectorySpace(
		LONG Volume,
		LONG StartingExtantNumber,
		LONG NumberOfExtants,
		void *Buffer);

LONG WriteExtendedDirectorySpace(
		LONG Volume,
		LONG StartingExtantNumber,
		LONG NumberOfExtants,
		void *Buffer,
		LONG WriteControlFlags);

LONG CommitExtendedDirectorySpace(
		LONG Volume,
		LONG StartingExtantNumber,
		LONG NumberOfExtants);

LONG WriteEDSFragList(
		LONG Volume,
		LONG WriteControlFlags,
		LONG numberOfFragments,
		LONG frag0StartingExtant,
		...);

/* End of routines in the EXTDIR module */
/****************************************************************************/
/* Routines in the EXTANTS module */

LONG ClaimExtendedDirectorySpace(
		LONG Volume,
		LONG StartingExtantNumber,
		LONG NumberOfExtants);

LONG ReturnExtendedDirectorySpace(
		LONG Volume,
		LONG StartingExtantNumber,
		LONG NumberOfExtants);

/* End of routines in the EXTANTS module */
/****************************************************************************/
/* Routines in the FILEIO module */

LONG AllocateFileHandle(
		LONG Station,
		LONG Task,
		LONG Volume);

LONG ReturnFileHandle(
		LONG filehandle);

void InitializeFileIO(void);

LONG InitializeVolume(
		LONG volumeNumber);

void RemoveVolume(
		LONG volumeNumber);

void FATAddressChangeRoutine(void);

void FATUpdateProcess(void);

void HitFATDirtyBit(
		LONG volumeNumber,
		LONG FATBitNumber);

LONG CheckFAT(
		LONG volumeNumber);

LONG GetSectorFromFAT(
		LONG Volume,
		LONG Sector,
		void *Buffer);

void ChangeDirectoryNumber(
		LONG volumeNumber,
		LONG oldPrimaryDirectoryNumber,
		LONG oldDirectoryNumber,
		LONG newPrimaryDirectoryNumber,
		LONG newDirectoryNumber);

void ChangeOpenFileTrustee(
		LONG volumeNumber,
		LONG directoryNumber,
		LONG newTrustee);

void ChangeHandleTNode(
		LONG handle,
		struct FLockNode *tPtr);

#if ReadAhead
LONG AddFile(
		LONG stationNumber,
		LONG accessBits,
		LONG volumeNumber,
		LONG directoryEntry,
		LONG ownerID,
		LONG fileSize,
		LONG *FirstFAT,
		LONG *handle,
		LONG task,
		struct FLockNode *tPtr,
		BYTE dataStream,
		LONG actualDirectoryEntry,
		BYTE readAheadMode,
		LONG moreAccessBits);
#else
LONG AddFile(
		LONG stationNumber,
		LONG accessBits,
		LONG volumeNumber,
		LONG directoryEntry,
		LONG ownerID,
		LONG fileSize,
		LONG *FirstFAT,
		LONG *handle,
		LONG task,
		struct FLockNode *tPtr,
		BYTE dataStream,
		LONG actualDirectoryEntry);
#endif

void MapDirectoryToHandle(
		LONG stationNumber,
		LONG task,
		LONG volumeNumber,
		LONG directoryEntry,
		LONG *handle0);

LONG MapDOSDirectoryToAnyHandle(
		LONG volume,
		LONG directoryNumber);

void SetHandleTask(
		LONG handle,
		LONG task);

LONG MapDirectoryToAnyHandle(
		LONG stationNumber,
		LONG volumeNumber,
		LONG directoryEntry);

LONG SetFileTimeAndDateStamp(
		LONG stationNumber,
		LONG handle,
		LONG timeStamp);

LONG GetFileSize(
		LONG stationNumber,
		LONG handle,
		LONG *fileSize);

LONG GetFileSizeFromDirEntryNumber(
		LONG volume,
		LONG directoryNumber,
		LONG dataStreamNumber);

LONG CheckIfFileWrittenTo(
		LONG Handle);

LONG GetHandleInfoData(
		LONG Station,
		LONG Handle,
		LONG *Volume,
		LONG *DirectoryNumber,
		LONG *StreamNumber);

LONG UpdateDirectory(
		LONG stationNumber,
		LONG handle);

LONG ForceUpdateDirectoryAndCallBack(
		LONG stationNumber,
		LONG handle,
		void (*CallBackProcedure)(),
		void *CallBackParameter,
		LONG *CallBackCount);

struct FLockNode *GetTreePointer(
		LONG stationNumber,
		LONG handle,
		LONG *task);

LONG GetOpenCount(
		LONG handle);

void SetOpenCountToOne(
		LONG handle);

void DetachFile(
		LONG handle);

void ReAttachFile(
		LONG handle);

void OpenFileSyncCheck(
		LONG stationNumber,
		LONG task);

LONG GetFileHoles(
		LONG stationNumber,
		LONG fileHandle,
		LONG startingOffset,
		LONG numberOfBlocks,
		BYTE *replyBitMap,
		LONG *allocationUnitInBytes);

LONG RemoveFileCompletely(
		LONG stationNumber,
		LONG handle);

LONG RemoveFile(
		LONG stationNumber,
		LONG handle);

void ClearTurboFAT(
		LONG Volume,
		LONG DirectoryEntry);

LONG ReadFile(
		LONG stationNumber,
		LONG handle,
		LONG startingOffset,
		LONG bytesToRead,
		LONG *actualBytesRead,
		void *buffer);

LONG SwitchToDirectFileMode(
		LONG station,
		LONG handle);

LONG TTSReadFileSkipHoles(
		LONG stationNumber,
		LONG handle,
		LONG startingOffset,
		LONG bytesToRead,
		LONG *actualBytesRead,
		void *buffer);

LONG ReadFileSkipHoles(
		LONG stationNumber,
		LONG handle,
		LONG startingOffset,
		LONG bytesToRead,
		LONG *actualBytesRead,
		void *buffer);

LONG WriteFile(
		LONG stationNumber,
		LONG handle,
		LONG startingOffset,
		LONG bytesToWrite,
		void *buffer);

struct FATStructure *GetFATEntry(
		LONG volumeNumber,
		LONG FATEntryNumber);

void MarkFATEntryChanged(
		LONG volumeNumber,
		LONG FATEntryNumber);

void CForceFATUpdate(
		LONG volumeNumber,
		LONG FATEntryNumber);

void ReturnFATChain(
		LONG volumeNumber,
		LONG firstFATEntryNumber,
		BYTE AdjustFreeableLimboCountFlag,
		LONG IsCompressedFlag);

void ReturnNotChargedFATChain(
		LONG volumeNumber,
		LONG firstFATEntryNumber);

LONG ChangeFATChainToLimbo(
		LONG volumeNumber,
		LONG firstFATEntryNumber,
		LONG entryNumber,
		LONG ownerID,
		LONG CompressedFileFlag);

LONG ChangeLimboFATChainToFile(
		LONG volumeNumber,
		LONG firstFATEntryNumber,
		LONG ownerID,
		LONG directoryEntry,
		LONG *BlockCount,
		LONG CompressedFileFlag);

void DOSDeleteLimboFile(
		LONG Volume,
		union DirUnion *Dir,
		BYTE AdjustFreeableLimboBlocksFlag,
		LONG IsCompressedFlag);

void AddBackSubdirectoryRestriction(
		LONG volumeNumber,
		LONG directoryEntry,
		LONG numberOfRestrictionBlocks);

LONG SetUserRestriction(
		LONG volumeNumber,
		LONG ownerID,
		LONG numberOfRestrictionBlocks);

LONG ChargeUser(
		LONG volumeNumber,
		LONG ownerID,
		LONG numberOfRestrictionBlocks);

void DestroyUserDiskRestriction(
		LONG volumeNumber,
		LONG ownerID);

LONG GetCurrentUserRestriction(
		LONG volumeNumber,
		LONG ownerID);

LONG GetCurrentDiskUsedAmount(
		LONG volumeNumber,
		LONG trusteeID);

LONG GetMaximumUserRestriction(
		LONG volumeNumber,
		LONG ownerID);

LONG GetFATChainLength(
		LONG volumeNumber,
		LONG firstFATEntry);

LONG GetVolumeAllocationUnitSize(
		LONG volumeNumber);

void TTSInitializeFileIO(void);

void TTSSetTransaction(
		LONG fileHandle);

LONG TTSGetWriteCount(
		LONG station,
		LONG task);

LONG TTSGetFileHandle(
		LONG station,
		LONG task,
		LONG volume,
		LONG directoryNumber,
		LONG dataStream);

LONG INWTTSEndTransaction(
		LONG station,
		LONG task,
		LONG *referenceNumber);

LONG TTSCheckTransaction(
		LONG referenceNumber);

void TransactionFinishupProcess(void);

LONG GetOpenFileFlags(
		LONG fileHandle,
		LONG *flags);

/* End of routines in the FILEIO module */
/****************************************************************************/
/* Routines in the FILER0 module */

LONG ScanLimboFilesInDirectory(
		LONG volumeNumber,
		LONG directoryNumber,
		LONG firstDirectoryBlock,
		LONG currentDirectoryNumber,
		LONG targetNameSpace,
		struct DirectoryStructure **entryPointer,
		LONG *entryNumber,
		struct DirectoryStructure **DOSDir);

/* End of routines in the FILER0 module */
/****************************************************************************/
/* Routines in the LOCKPRIM module */

LONG CheckAnyUse(
		LONG vol,
		LONG dir);

LONG FileSearch(
		LONG volumeNumber,
		LONG directory,
		struct FLockNode **nodePtr);

struct FLockNode *GetFileTree(
		LONG *index);

void DelFRoot(
		struct FLockNode *tNode,
		struct FLockNode *ReplacementTNode);

LONG TSearch(
		BYTE *string,
		LONG length,
		struct RLockNode **nodePtr);

/* End of routines in the LOCKPRIM module */
/****************************************************************************/
/* Routines in the LOCKS1 module */

void InitLocks(void);

void ClearLnodeCallback(struct FileListNode *lnode);
LONG ClearOpenCallback(LONG station, LONG task, LONG hndl);
LONG LinkRelOpenCallback(LONG pointer);

LONG checkfork(
		struct ConnectionStructure *connection,
		LONG task,
		struct FLockNode *tnode);

void GetPrivs(
		LONG station,
		LONG task,
		LONG vol,
		LONG dir,
		BYTE *privret);

LONG CheckUse(
		LONG station,
		LONG task,
		LONG vol,
		LONG dir,
		LONG cptmode);

LONG CheckUseAndOpLocks(
		LONG station,
		LONG task,
		LONG vol,
		LONG dir,
		LONG cptmode,
		LONG flags,
		void **rtnTnodePtr);

LONG ChangeLockDirectoryNumber(
		LONG vol,
		LONG oldDir,
		LONG newDir);

void RemPrivs(
		LONG priv,
		struct FLockNode *tnode);

void INWLogFile(
		LONG station,
		LONG task,
		LONG vol,
		LONG dir,
		BYTE *filename,
		LONG flags,
		LONG wtime,
		void (*rplyf)());

LONG flocktry(
		struct LockWaitStructure *lockWait,
		LONG task,
		struct FLockNode *tnode,
		LONG flags,
		LONG firsttry,
		LONG wtime,
		void (*rplyf)());

int RegisterFileLockWait(
	LONG		vol,
	LONG		qparms,
	LONG		lockWaitRet,
	LONG		fscookie);

int RegisterFileLockSWait(
	LONG		vol,
	LONG		qparms,
	LONG		lockWaitRet,
	LONG		fscookie);


LONG LockFile(
		LONG station,
		LONG task,
		LONG vol,
		LONG dir,
		BYTE *filename,
		struct FLockNode **tret);

void LFileSet(
		LONG station,
		LONG task,
		LONG wtime,
		void (*rplyf)());

void MultiLFileSet(
		LONG station,
		LONG task,
		LONG wtime,
		void (*rplyf)());

void tryflocks(
		struct LockWaitStructure *lockWait,
		LONG task,
		LONG first,
		LONG wtime,
		LONG flags,
		void (*rplyf)(
				LONG stationNumber,
				LONG completionCode,
				LONG rtask));

LONG RelFileSet(
		LONG station,
		LONG task,
		LONG lastset);

LONG ClearFLocks(
		LONG station,
		LONG task,
		LONG lastset);

LONG INWReleaseFile(
		LONG station,
		LONG task,
		LONG vol,
		LONG dir);

LONG KClose(
		struct ConnectionStructure *connection,
		LONG task,
		struct FileListNode *lnode,
		struct FLockNode *tnode,
		LONG CloseFlag);

LONG CloseLock(
		LONG station,
		LONG Task,
		LONG hndl);

LONG CloseTNode(
		LONG station,
		LONG task,
		struct FLockNode *tnode);

LONG RelTNode(
		LONG station,
		LONG task,
		struct FLockNode *tnode);

LONG INWClearFile(
		LONG station,
		LONG task,
		LONG vol,
		LONG dir);

void ClrFile(
		struct ConnectionStructure *connection,
		LONG task,
		struct FileListNode *lnode,
		struct FLockNode *tnode);

/* NOTE only the low order byte is used for flags parameter of openlock,
	the upper bits used for other things */

LONG PingOpLock( struct FLockNode *tnode, LONG station, LONG task);

LONG OpenLock(
		LONG station,
		LONG task,
		LONG vol,
		LONG dir,
		BYTE *filename,
		BYTE NameSpace,
		LONG flags,
		BYTE Private,
		BYTE trantype,
		BYTE datastreamnumber,
		struct FLockNode **tnoderet);

LONG RegisterOpenCallBack(LONG station,
		LONG task,
		LONG hndl,
		void (*callb)());


LONG IncDupCount(
		LONG station,
		LONG task,
		LONG handle);

LONG FLConflict(
		struct FileListNode *lnode,
		LONG flflag);

LONG LinkHandle(
		LONG station,
		LONG task,
		struct FLockNode *tnode,
		LONG hndl);

void ClearHandle(
		LONG station,
		LONG task,
		LONG vol,
		LONG dir,
		LONG *hret);

void RemHandle(
		struct ConnectionStructure *connection,
		struct FLockNode *tnode,
		struct FileListNode *lnode,
		LONG *hret);

LONG OtherTasks(
		struct FileListNode *lnode,
		LONG opcheck);

LONG VolClean(
		LONG ldrive,
		LONG doclean);

LONG VolCln(
		LONG ldrive,
		LONG doclean,
		struct FLockNode *tnode);

LONG RemoveNode(
		struct FLockNode *tnode);

void	CancelXFSWait(
	LONG		volumenumber,
	LONG		fscookie);

LONG	XFSFileLock(
	LONG		volumenumber,
	LONG		connection,
	LONG		directorynumber,
	LONG		NameSpace,
	BYTE		*path,
	LONG		waitcookie,
	LONG		task);

void GetXFSLockSemaphore(LONG Volume);

void ReleaseXFSLockSemaphore(LONG Volume);

LONG	ReleaseXFSFileLock(
	LONG		volumenumber,
	LONG		connection,
	LONG		directorynumber,
	LONG		NameSpace,
	BYTE		*path,
	LONG		task);

void	EnumerateFileSet(
	struct	EnumStructure	*firstenumerate,
	LONG		volumenumber,
	LONG		directorynumber,
	LONG		NameSpace,
	BYTE		*path);

LONG	XFSLockFileSet(
	struct	EnumStructure	*firstenumerate,
	LONG		connection,
	LONG		waitcookie,
	LONG		task);

LONG XFSReleaseFileSet(
	struct	EnumStructure	*firstenumerate,
	LONG		station,
	LONG		task);


LONG	XFSSingleRecordLock(
	LONG		volumenumber,
	LONG		handle,
	LONG		offset,
	LONG		end,
	LONG		waitcookie,
	LONG		task);

LONG	XFSReleaseRecordLock(
	LONG		volumenumber,
	LONG		handle,
	LONG		offset,
	LONG		end,
	LONG		task);

void	EnumerateRecordSet(
	struct	EnumStructure	*firstenumerate,
	LONG		volumenumber,
	LONG		handle,
	LONG		offset,
	LONG		end,
	LONG		flag);

LONG	XFSLockRecordSet(
	struct	EnumStructure	*firstenumerate,
	LONG		connection,
	LONG		waitcookie,
	LONG		task);


LONG XFSReleaseRecordSet(
	struct	EnumStructure	*firstenumerate,
	LONG		station,
	LONG		task);

void UnenumerateRecordLocks(
		struct EnumStructure *firstenumerate);

LONG RevokeFileHandleRights(
		LONG Station,
		LONG Task,
		LONG FileHandle,
		LONG QueryFlag,
		LONG removeRights,
		LONG *newRights);

#if NTCLIENTECO
LONG UpdateFileHandleRights(
		LONG Station,
		LONG Task,
		LONG VolumeNumber,
		LONG NameSpace,
		LONG DirectoryNumber,
		LONG OldAccessRights,
		LONG NewAccessRights,
		LONG FileHandle,
		LONG *newRights);
#endif
/* End of routines in the LOCKS1 module */
/****************************************************************************/
/* Routines in the LOCKS2 module */

LONG trans(
		struct ConnectionStructure *connection,
		LONG task);

void BeginTrans(
		LONG station,
		LONG task,
		LONG wtime,
		void (*rplyf)());

LONG EndTrans(
		LONG station,
		LONG task);

void LogRec(
		LONG station,
		LONG task,
		BYTE *string,
		LONG flags,
		LONG wtime,
		void (*rplyf)());

void rlocktry(
		struct LockWaitStructure *lockWait,
		LONG task,
		struct RLockNode *tnode,
		LONG flags,
		LONG firsttry,
		LONG wtime,
		void (*rplyf)(
				LONG stationNumber,
				LONG completionCode,
				LONG rtask));

LONG rlconflict(
		struct RecListNode *lnode);

void LRecSet(
		LONG station,
		LONG task,
		LONG flags,
		LONG wtime,
		void (*rplyf)());

LONG UnlockRec(
		LONG station,
		LONG task,
		BYTE *string);

LONG ReleaseRecord(
		LONG station,
		LONG task,
		BYTE *string);

LONG ClearRLocks(
		LONG station,
		LONG task,
		LONG lastset);

LONG RelRecSet(
		LONG station,
		LONG task,
		LONG lastset);

void rellheldlocks(
		struct ConnectionStructure *connection,
		LONG task);

void AddTaskLLocks(
		struct ConnectionStructure *connection,
		LONG task,
		struct TransListNode *trptr);

void ResetStation(
		LONG station,
		LONG task);

struct FLockNode *ftree(
		LONG vol,
		LONG dir,
		BYTE *filename,
		BYTE NameSpace,
		LONG insflag,
		LONG forkflag);

struct FLockNode *makefnode(
		LONG vol,
		LONG dir,
		BYTE *filename,
		BYTE NameSpace,
		struct FLockNode *prevptr);

struct RLockNode *tree(
		struct RootNode *root,
		BYTE *str,
		LONG len,
		LONG insflag);

void fcondfree(
		struct FLockNode *tnode,
		LONG hndl);

void dftree(
		struct FLockNode *tnode,
		BYTE freeflag);

void dtree(
		struct RootNode *root,
		struct RLockNode *tnode);

LONG queue(
		struct qInfoStructure *qparms);


void dqueue(
		struct LockWaitStructure *lockWait);

void RelQueue(
		struct LockWaitStructure *lockWait);

void TryRelQueue(void);

void RetryXFSLock(
		LONG lockWait);

void frelease(
		struct FLockNode *tnode);

void rrelease(
		struct RLockNode *tnode);

void FreeWaitStation(
		struct LockWaitStructure *lockWait);

struct FileListNode *slist(
		struct ConnectionStructure *connection,
		LONG task,
		struct FLockNode *tnode,
		LONG insflag);

void dlist(
		struct ConnectionStructure *connection,
		struct FileListNode *lnode);

struct RecListNode *srlist(
		struct ConnectionStructure *connection,
		LONG task,
		struct RLockNode *tnode,
		LONG insflag);

void drlist(
		struct ConnectionStructure *connection,
		struct RecListNode *lnode);

LONG SetTransFlags(
		LONG station,
		LONG pthresh,
		LONG lthresh);

void GTransFlags(
		LONG station,
		BYTE *threshrets);

LONG SetTTaskFlags(
		LONG station,
		LONG task,
		LONG pthresh,
		LONG lthresh);

LONG GTTaskFlags(
		LONG station,
		LONG task,
		BYTE *threshrets);

LONG STTaskState(
		LONG station,
		LONG task,
		LONG flags);

void GTTaskState(
		LONG station,
		LONG task,
		BYTE *threshrets);

struct TransListNode *GetTRec(
		struct ConnectionStructure *connection,
		LONG task,
		LONG type,
		LONG lthresh,
		LONG pthresh);

void AddTRLock(
		struct ConnectionStructure *connection,
		struct TransListNode *trptr,
		LONG type);

LONG RelTRLock(
		struct TransListNode *trptr,
		LONG type);

void RelEndTrans(
		struct ConnectionStructure *connection,
		struct TransListNode *trptr);

void FreeTRec(
		struct ConnectionStructure *connection,
		struct TransListNode *trptr);

void StartTTrans(
		struct ConnectionStructure *connection,
		LONG task);

LONG TTrackOn(
		LONG station,
		LONG task);

LONG TTrackOff(
		LONG station,
		LONG task,
		BYTE *retbuf);

void ClearHoldingLocks(
		struct ConnectionStructure *connection,
		LONG task);

LONG TTrackAbort(
		LONG station,
		LONG task);

void AbortTransactions(void);

/* End of routines in the LOCKS2 module */
/****************************************************************************/
/* Routines in the LOCKS3 module */

void InitializeLocksAES(void);

LONG ldelay(
		struct LockWaitStructure *lockWait,
		WORD time);

void cleardelay(
		struct LockWaitStructure *lockWait);

void wakestns(void);

void WakeStation(
		struct LockWaitStructure *lockWait);

void ClearLockWait(
		LONG station);

void ClearTaskWait(
		LONG station,
		LONG task);

void ClearStampWait(
		LONG station,
		LONG task,
		LONG stamp);

LONG otherplock(
		struct FileListNode *lnode,
		struct PListNode *rlnode,
		struct PRecNode *rptr);

LONG overshare(
		struct FileListNode *lnode,
		struct PRecNode *rptr);

LONG LogPRec(
		LONG station,
		LONG hndle,
		LONG start,
		LONG length,
		LONG flags,
		LONG wtime,
		void (*rplyf)(),
		...);

LONG AddWLock(
		LONG station,
		LONG hndl,
		LONG start,
		LONG length);

struct PRecNode *newprec(
		struct FLockNode *tnode,
		struct PRecNode *rptr,
		LONG start,
		LONG end);

LONG prlocktry(
		struct LockWaitStructure *lockWait,
		LONG task,
		struct FLockNode *tnode,
		struct FileListNode *lnode,
		struct PListNode *rlnode,
		LONG flags,
		LONG firsttry,
		LONG wtime,
		LONG ovcheck,
		void (*rplyf)(
				LONG stationNumber,
				LONG completionCode,
				LONG rtask,
				LONG pstmp),
		LONG pstamp);


int RegisterRecordLockWait(
	LONG		vol,
	LONG		qparms,
	LONG		lockWaitRet,
	LONG		fscookie);

int RegisterRecordLockSWait(
	LONG		vol,
	LONG		qparms,
	LONG		lockWaitRet,
	LONG		fscookie);


LONG prconflict(
		struct FileListNode *lnode,
		struct PListNode *rlnode);

void LockPRecSet(
		LONG station,
		LONG task,
		LONG flags,
		LONG wtime,
		void (*rplyf)());

void tryprlocks(
		struct LockWaitStructure *lockWait,
		LONG task,
		LONG flags,
		LONG firsttry,
		LONG wtime,
		void (*rplyf)(
				LONG stationNumber,
				LONG completionCode,
				LONG rtask));

LONG ReleasePRec(
		LONG station,
		LONG hndle,
		LONG start,
		LONG length,
		LONG clrflag);

LONG clearprec(
		struct ConnectionStructure *connection,
		struct PListNode *rlnode,
		BYTE clrflag,
		struct FileListNode *lnode,
		struct TransListNode *trptr,
		BYTE	ovcheck,
		BYTE	extflag,
		struct EnumStructure *enums);

void relplnode(
		struct FileListNode *lnode,
		struct PListNode *rlnode,
		LONG relflag);

LONG RelPRecLocks(
		LONG station,
		LONG task,
		LONG lastset,
		LONG clrflag);

void relprlocks(
		struct ConnectionStructure *connection,
		LONG task,
		LONG lastset,
		LONG clrflag,
		LONG	extflag,
		struct EnumStructure *enums);

void relpheldlocks(
		struct ConnectionStructure *connection,
		LONG task,
		struct EnumStructure *enums);

LONG relfilerecs(
		struct ConnectionStructure *connection,
		struct FileListNode *lnode,
		BYTE clrflag,
		BYTE lastset);

void chgprtask(
		struct FileListNode *lnode,
		LONG task);

LONG prqueue(
		struct qInfoStructure *qparms,
		struct PRecNode *rptr);


void dprqueue(
		struct LockWaitStructure *lockWait);

void relprec(
		struct FLockNode *tnode,
		struct PRecNode *rptr);

struct SemListNode *ssem(
		struct ConnectionStructure *connection,
		LONG task,
		struct RLockNode *sem,
		LONG insflag);

LONG SCreateSemaphore(
		LONG station,
		LONG task,
		BYTE *string,
		LONG initial,
		BYTE *repbuf);

LONG SExamineSemaphore(
		LONG station,
		LONG task,
		LONG semval,
		BYTE *retbuf);

void SPSemaphore(
		LONG station,
		LONG task,
		LONG semval,
		LONG wtime,
		void (*rplyf)());

LONG SVSemaphore(
		LONG station,
		LONG task,
		LONG semval);

LONG SCloseSem(
		LONG station,
		LONG task,
		LONG semval);

void SDelSems(
		struct ConnectionStructure *connection,
		LONG task);

/* End of routines in the LOCKS3 module */
/****************************************************************************/
/* Routines in the LOCKSCON module */

LONG VCStnLockStatus(
		LONG station,
		LONG BufferSize,
		BYTE *buffer);

LONG VCStnFiles(
		LONG station,
		LONG buffersize,
		BYTE *buffer,
		LONG next);

LONG VCFileTasks(
		BYTE vol,
		LONG dir,
		BYTE forktype,
		LONG buffersize,
		BYTE *buffer,
		LONG next);

LONG VCStnPhyLocks(
		LONG station,
		BYTE vol,
		LONG dir,
		BYTE forktype,
		LONG buffersize,
		BYTE *buffer,
		LONG next);

LONG VCFilePhyLocks(
		BYTE vol,
		LONG dir,
		BYTE forktype,
		LONG buffersize,
		BYTE *buffer,
		LONG next);

LONG VCStnLRecs(
		LONG station,
		LONG buffersize,
		BYTE *buffer,
		LONG next);

LONG VCLRecTasks(
		BYTE length,
		BYTE *name,
		LONG buffersize,
		BYTE *buffer,
		LONG next);

LONG VCStnSems(
		LONG station,
		LONG buffersize,
		BYTE *buffer,
		LONG next);

LONG VCSemaphoreTasks(
		LONG length,
		BYTE *name,
		LONG buffersize,
		BYTE *buffer,
		LONG next);

/* End of routines in the LOCKSCON module */
/****************************************************************************/
/* Routines in the MIGRATE module */

LONG	StartVolumeSync( LONG Volume);
LONG	EndVolumeSync( LONG Volume);

/* End of routines in the MIGRATE module */
/****************************************************************************/
/* Routines in the MACWARE module */

LONG MacMakeDirectory(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG MacBaseID,
		BYTE *MacName,
		LONG MacNameLength,
		BYTE *FinderInfo,
		BYTE *ProDosInfo,
		LONG *NewDirectoryID);

LONG MacMakeFile(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG MacBaseID,
		BYTE *MacName,
		LONG MacNameLength,
		LONG DeleteExistingFileFlag,
		BYTE *FinderInfo,
		BYTE *ProDosInfo,
		LONG *NewFileID);

LONG MacDelete(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG MacBaseID,
		BYTE *MacName,
		LONG MacNameLength);

LONG MacGetEntryID(
		LONG Station,
		LONG Volume,
		LONG MacBaseID,
		BYTE *MacName,
		LONG MacNameLength,
		LONG *AnswerID);

LONG MacGetEntryInformation(
		LONG Station,
		LONG Volume,
		LONG MacBaseID,
		BYTE *MacName,
		LONG MacNameLength,
		LONG WantedBits,
		struct MacInfoStruct *Answer);

LONG MacMapFileHandleToID(
		LONG Station,
		LONG Handle,
		struct MacFileHandleMapStruct *Ans);

LONG MacRename(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG MacSourceID,
		BYTE *SourceName,
		LONG SourceNameLength,
		LONG MacDestID,
		BYTE *DestName,
		LONG DestNameLength);

LONG MacOpen(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG MacBaseID,
		BYTE *MacName,
		LONG MacNameLength,
		LONG RequestedFork,
		LONG RequestedAccess,
		struct MacOpenStruct *Ans);

LONG MacSetInfo(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG MacBaseID,
		BYTE *MacName,
		LONG MacNameLength,
		LONG WantedBits,
		struct MacSetStruct *SetData);

LONG MacEnumerate(
		LONG Station,
		LONG Volume,
		LONG MacBaseID,
		BYTE *MacName,
		LONG MacNameLength,
		LONG MacSearchID,
		LONG SearchBits,
		LONG WantedBits,
		struct MacSearchStruct *Ans,
		LONG MaxAnswers,
		LONG ShortReplyRequired);

LONG MacMakeDOSHandle(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG MacBaseID,
		BYTE *MacName,
		LONG MacNameLength,
		BYTE *Ans);

LONG DeletedDOSToMacInfo(
		LONG Station,
		LONG Volume,
		LONG DOSBaseID,
		BYTE *Answer);

LONG MacToDOSName(
		LONG Station,
		LONG Volume,
		LONG MacBaseID,
		BYTE *Answer);

LONG MapShortNameToMacID(
		LONG Station,
		LONG Base,
		BYTE *DosName,
		LONG *MacID);

void MacConvertNewTrusteeRightsToOld(
		BYTE *accessRights,
		BYTE setAttributesFlag,
		WORD *attributes,
		LONG station,
		LONG volume,
		LONG directoryNumber,
		LONG nameSpace);

/* End of routines in the MACWARE module */
/****************************************************************************/
/* Routines in the FILESVCS module */

LONG FileServicesNCPRequest(
		LONG station,
		LONG task,
		BYTE request,
		BYTE *info,
		BYTE *answer,
		LONG *answerLength,
		LONG DataPacketLength);

/* End of routines in the FILESVCS module */
/****************************************************************************/
/* Routines in the STREAMIO module */

extern LONG	NDSCreateStreamFile(
					LONG Station,
					LONG Task,
					BYTE *fileName,
					LONG CreateAttributes,
					LONG *fileHandle,
					LONG *DOSDirectoryBase);

extern LONG	NDSOpenStreamFile(
					LONG Station,
					LONG Task,
					BYTE *fileName,
					LONG RequestedRights,
					LONG *fileHandle,
					LONG *DOSDirectoryBase);

extern LONG	NDSDeleteStreamFile(
					LONG Station,
					LONG Task,
					BYTE *fileName,
					LONG *DOSDirectoryBase);

extern LONG InitializeSecureDirectoryNumber(LONG Volume);
extern void ShutdownSecureDirectoryNumber(LONG Volume);

/* End of routines in the STREAMIO module */
/****************************************************************************/
/* Routines in the TTS module */

LONG ProcessDisableTTSCommand(
		struct ScreenStruct *screenID);

void ProcessRemoteDisableTTSCommand(
		LONG stationNumber);

void DisableTTS(
		BYTE ReasonCode);

void ProcessRemoteEnableTTSCommand(
		LONG stationNumber);

LONG EnableTTS(
		struct ScreenStruct *screenID);

LONG INWTTSAbortTransaction(
		LONG Station,
		LONG Task,
		LONG *ReferenceNumber,
		BYTE MessageFlag);

LONG INWTTSBeginTransaction(
		LONG Station,
		LONG Task);

LONG TTSCheck(void);

void InformTTSOnVolumeMount(
		LONG volumeNumber,
		struct ScreenStruct *screenID);

void InformTTSOnVolumeDisMount(
		LONG volumeNumber,
		struct ScreenStruct *screenID);

LONG GetTTSWriteCountFromHandle(LONG handle, LONG *currentTTSWriteCount);
LONG GetTTSWriteCountForTransaction(LONG Station, LONG Task, LONG transactionID, LONG *rtnWriteCount);

/* End of routines in the TTS module */
/****************************************************************************/
/* Routines in the WILDREPL module */

LONG DOSWildReplace(
		LONG Volume,
		BYTE *searchPattern,
		union DirUnion *foundEntry,
		BYTE *replacePattern,
		BYTE *replacedString);

/* End of routines in the WILDREPL module */
/****************************************************************************/
/* Routines in the PTREE module */

LONG CountFolderOffspring(
		LONG Station,
		LONG Volume,
		LONG NameSpace,
		LONG NetwareBaseID);

/* End of routines in the PTREE module */
/****************************************************************************/
/* Routines in the FILER1 module */


BYTE StationHasAccessRightsGrantedBelow(
		LONG station,
		LONG volume,
		LONG directoryNumber,
		LONG nameSpace);

LONG ExhumeSecondaryFiles(
		LONG Volume,
		struct DirectoryStructure *Dir,
		LONG EntryNumber,
		BYTE *NewName,
		LONG WhoDidItID,
		LONG *BlockCount);

LONG KillDirRights(
		LONG killingStationNumber,
		LONG ID);

LONG ReturnDirectorySpaceRestrictions(
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		BYTE *Answer,
		LONG AnswerBufferSize,
		LONG *AnswerSizeReturned);

LONG GetDataStreamLengthsFromPathStringBase(
		LONG Station,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG *DataStreamLengthsVector);

LONG CheckForObjectID(
		LONG *ObjectID,
		BYTE *IsUsed,
		LONG IDCount);

# if NTCLIENTECO
LONG VerifyFileAccessRightsAreAllowable(
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		LONG RequestedRights );
#endif
/* End of routines in the FILER1 module */
/****************************************************************************/
/* Routines in the FILER2 module */
LONG ForceDirectoryUpdate (
		LONG Station,
		LONG OpenFileHandle);

LONG GetHugeInformationFromNameSpace(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		LONG HugeMask,
		BYTE *StateInfo,
		BYTE *buffer,
		LONG *bufferSize,
		BYTE *NextStateInfo);

LONG SetHugeInformationFromNameSpace(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		LONG HugeMask,
		BYTE *StateInfo,
		BYTE *buffer,
		LONG bufferSize,
		BYTE *NextStateInfo,
		LONG *BytesConsumed);

LONG OpenEAHandle(
		LONG Station,
		LONG Task,
		LONG OpenFlags,
		LONG Flags,
		LONG Volume,
		LONG Handle,
		LONG *EAHandle);

LONG CloseEAHandle(
		LONG Station,
		LONG Task,
		LONG eaHandle);

LONG WriteEAData(
		LONG Station,
		LONG Task,
		LONG eaHandle,
		LONG TotalDataLength,
		LONG StartPosition,
		LONG AccessFlag,
		LONG KeySize,
		BYTE *keyBuf,
		LONG DataSize,
		BYTE *inBuf,
		LONG *BytesWritten);

LONG ReadEAData(
		LONG Station,
		LONG Task,
		LONG eaHandle,
		LONG StartPosition,
		LONG InspectSize,
		LONG KeySize,
		BYTE *Key,
		BYTE *OutBuf,
		LONG *TotalEALen,
		WORD *CurrentLen,
		LONG *AccessFlag,
		LONG MaximumDataSize);

LONG EnumEA(
		LONG Station,
		LONG Task,
		LONG eaHandle,
		LONG InfoLevel,
		LONG StartPosition,
		LONG InspectSize,
		LONG KeySize,
		BYTE *Key,
		BYTE *EnumBuf,
		LONG *EnumBufSize,
		WORD *NextPos,
		LONG *TotalDataSizeOfEAs,
		LONG *TotalEAs,
		WORD *CurrentEAsInReply,
		LONG *TotalKeySizeOfEAs,
		LONG MaximumDataSize);

LONG DupEA(
		LONG Station,
		LONG Task,
		LONG srcTypeFlag,
		LONG srcVolume,
		LONG srcHandle,
		LONG dstTypeFlag,
		LONG dstVolume,
		LONG dstHandle,
		LONG *DupCount,
		LONG *DupData,
		LONG *DupKey);

LONG StampEANumber(
		LONG Volume,
		LONG DirectoryNumber,
		LONG EANumber);

LONG ExamDirForEA(
		LONG Volume,
		LONG DirectoryNumber,
		LONG *EANumber);

void NotifyEAError(
		BYTE *fInfo);

/* End of routines in the FILER2 module */
/****************************************************************************/
/* Routines in the FILER3 module */

LONG	GetMountedVolumeList(
			LONG StartingVolumeNumber,
			LONG VolumeRequestFlags,
			LONG NameSpace,
			BYTE *ans,
			LONG length,
			LONG *AnswerLength);

LONG AllocateNCPDirHandle(
		LONG station,
		LONG task,
		LONG Volume,
		LONG NameSpace,
		LONG DirectoryNumber,
		LONG DOSDirectoryNumber,
		LONG AllocateMode,
		LONG *DirectoryHandle);

/* End of routines in the FILER3 module */
/****************************************************************************/
/* Routines in the FSHOOKS module */

/* End of routines in the FSHOOKS module */
/****************************************************************************/
/* Routines in the HANDLE module */

void AlertNCPSearchMapLost(
		LONG Station);

void InitializeNonLoggedInUsers(void);

LONG StartDirectoryHandles(
		struct ConnectionStructure *connection);

void ClearTemporaryDirectoryHandles(
		LONG Station,
		LONG Task);

void ClearDirectoryHandles(
		LONG Station);

void ClearNCPVolumeDirectoryHandles(
		LONG Volume);

LONG CheckNCPDirectoryHandles(
		LONG checkingConnectionNumber,
		LONG Volume,
		LONG DirectoryNumber);

void ClearNCPDirectoryHandles(
		LONG Volume,
		LONG DirectoryNumber);

void ChangeNCPDirectoryHandles(
		LONG Volume,
		LONG OldDirectoryNumber,
		LONG NewDirectoryNumber);

LONG AllocateDirectoryHandle(
		LONG Station,
		LONG Task,
		LONG HandleType,
		LONG Base,
		BYTE *ModifierString,
		BYTE *Answer);

LONG AllocateNCPDirectoryHandle(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG DirectoryNumber,
		BYTE *Answer);

LONG DestroyDirectoryHandle(
		LONG Station,
		LONG Base);

LONG NCPGetAccessPrivileges(
		LONG Station,
		LONG Base,
		BYTE *ModifierString,
		BYTE *Answer);

LONG ModifyMaximumDirectoryAccessRights(
		LONG Station,
		LONG Task,
		LONG Base,
		BYTE *ModifierString,
		LONG RightsGrantMask,
		LONG RightsRevokeMask);

LONG SetDirectoryStatusInformation(
		LONG Station,
		LONG Task,
		LONG Base,
		BYTE *ModifierString,
		LONG CreationDateAndTime,
		LONG OwnerID,
		LONG MaximumAccessRights);

LONG SetFileStatusInformation(
		LONG Station,
		LONG Task,
		LONG Base,
		BYTE *ModifierString,
		BYTE SearchAttributes,
		WORD fileAttributes,
		LONG CreationDate,
		LONG LastAccessedDate,
		LONG LastUpdatedDateAndTime,
		LONG OwnerID);

LONG GetPathString(
		LONG Station,
		LONG Handle,
		BYTE *String);

LONG NCPCreateDirectory(
		LONG Station,
		LONG Base,
		LONG DirectoryAccessMask,
		BYTE *NameString);

LONG NCPDestroyDirectory(
		LONG Station,
		LONG Base,
		BYTE *NameString);

LONG NCPRenameDirectory(
		LONG Station,
		LONG Task,
		LONG Base,
		BYTE *OldNameString,
		BYTE *NewNameString);

LONG StartSearch(
		LONG Station,
		LONG Task,
		LONG Base,
		BYTE *ModifierString,
		struct SearchStructure *SearchData);

LONG GetFileStatusInformation(
		LONG Station,
		LONG Task,
		WORD Index,
		LONG SourceBase,
		LONG SearchAttributes,
		BYTE *ModifierString,
		BYTE *ReplyArea);

LONG GetDirectoryStatusInformation(
		LONG Station,
		LONG Task,
		WORD Index,
		LONG SourceBase,
		BYTE *ModifierString,
		BYTE *ReplyArea);

LONG OldSearchForAFile(
		LONG Station,
		LONG Task,
		LONG SourceBase,
		BYTE *ModifierString,
		LONG SearchAttributes,
		WORD Index,
		BYTE *ReplyArea);

LONG SetPathName(
		LONG Station,
		LONG TargetBase,
		LONG SourceBase,
		BYTE *ModifierString);

LONG GetTrusteeList(
		LONG Station,
		LONG Base,
		BYTE *ModifierString,
		LONG TrusteeSetNumber,
		BYTE *ReplyArea);

LONG GetExtendedTrusteeList(
		LONG Station,
		LONG Base,
		BYTE *ModifierString,
		LONG TrusteeSetNumber,
		BYTE *ReplyArea);

LONG GetEffectiveRights(
		LONG Station,
		LONG Base,
		BYTE *ModifierString,
		BYTE *ReplyArea);

LONG GetObjectEffectiveRights(
		LONG connectionNumber,
		LONG objectID,
		LONG Base,
		BYTE *ModifierString,
		BYTE *ReplyArea);

LONG RemoveTrustee(
		LONG Station,
		LONG Base,
		BYTE *ModifierString,
		LONG Trustee,
		BYTE CanRemoveFromFileFlag);

LONG ScanVolumeForTrustee(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG DirLast,
		LONG TrusteeID,
		BYTE *ReplyBuffer);

void ConvertNewTrusteeRightsToOld(
		BYTE *accessRights,
		BYTE macStationFlag,
		LONG station,
		LONG volume,
		LONG directoryNumber,
		LONG nameSpace);

void ConvertOldTrusteeRightsToNew(
		BYTE *accessRights,
		BYTE macStationFlag);

/* End of routines in the HANDLE module */
/****************************************************************************/
/* Routines in the INITVOL module */

void StartVolumeExpansion(void);

void EndVolumeExpansion(void);

LONG ExpandVolumeTables(
		LONG Volume,
		LONG ExtraBlocksBeingAdded,
		LONG **FAT0Vector,
		LONG **FAT1Vector,
		LONG **FAT);

void UnExpandVolumeTables(
		LONG Volume);

void CommitExpandedVolumeTables(
		LONG Volume,
		LONG ExtraBlocksBeingAdded,
		LONG AdditionalFAT0,
		LONG AdditionalFAT1);

void AddToFATFATChain(
		LONG Volume,
		LONG AdditionalFAT0,
		LONG AdditionalFAT1);

void WriteLastFATSector(
		LONG Volume);

void FlushAllVolumes(void);

BYTE *CheckFATEntry(
		LONG Volume,
		LONG Entry,
		struct FATStructure **FAT);

void ReturnDiskBlockNoOneOwns(
		LONG Volume,
		LONG Entry);

BYTE *CheckDataStream(
		LONG	Volume,
		LONG	DirectoryEntryNumber,
		LONG	DOSFileAttributes,
		LONG	MyFileAttributes,
		LONG	DataStreamNumber,
		LONG	*FirstBlock,
		LONG	*FileSize,
		BYTE	*FileNameLength,
		BYTE	*NameSpaceName,
		struct ProblemListHeaderStructure *ProblemListHeader,
		LONG *SpaceUsage);

BYTE *CheckDirectoryEntry(
		LONG Volume,
		LONG DirectoryEntryNumber,
		struct DirectoryStructure *DirectoryEntry,
		struct ProblemListHeaderStructure *ProblemListHeader,
		LONG *SpaceUsage);

void KeepDeletedDataStream(
		LONG Volume,
		LONG DirectoryEntryNumber,
		LONG DataStreamNumber,
		LONG FirstFAT,
		LONG MyFileAttributes,
		LONG FileSize);

LONG CheckDeletedDataStream(
		LONG Volume,
		LONG FirstFAT,
		LONG FileLength,
		LONG DOSFileAttributes,
		LONG MyFileAttributes);

LONG INWGetVolumeInformation(
		LONG Volume,
		LONG *SectorsPerCluster,
		LONG *TotalVolumeClusters,
		LONG *FreedClusters,
		LONG *FreeableLimboClusters,
		LONG *NonFreeableLimboClusters);

/* End of routines in the INITVOL module */
/****************************************************************************/
/* Routines in the MODIFY module */

/* End of routines in the MODIFY module */
/****************************************************************************/
/* Routines in the NSPACE module */

/* End of routines in the NSPACE module */
/****************************************************************************/
/* Routines in the VOLMAN module */


/* End of routines in the VOLMAN module */
/****************************************************************************/
/* Routines in the VOLUME module */

LONG	NewGetVolumeStatistics(
			LONG	Volume,
			struct	RequestPacketStructure	*Request,
			struct	ReplyProceduresStructure	*RP);

/* End of routines in the VOLUME module */
/****************************************************************************/
/* Routines in the VOLTABLE module */
LONG GetFATWriteVectorTable(
		LONG volumeNumber);

LONG GetHashIndexMaskTable(
		LONG volumeNumber);

/* End of routines in the VOLTABLE module */
/****************************************************************************/
/* Routines in the VSWITCH module */

LONG	CreateDataMigratedFileEntry(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG CreatedAttributes,
		LONG RequestedRights,
		LONG FlagBits,
		BYTE DataStreamNumber,
		LONG *DirectoryNumber,
		void	*dmkey,
		void	*streams,
		LONG	SaveKeyFlag);

/* End of routines in the VSWITCH module */

/****************************************************************************/
/* Routines in the VOLMOUNT module */

void GetLastVolumeMountErrorString(LONG length, BYTE *buffer);

LONG GetVolumeClusterUniqueID(
		struct ScreenStruct *screenID,
		BYTE *volumeName,
		LONG *clusterUniqueID);

LONG SetVolumeClusterUniqueID(
		struct ScreenStruct *screenID,
		BYTE *volumeName,
		LONG clusterUniqueID);

void ReturnDriveName(
		LONG driveID,
		BYTE *name);

LONG MountVolume(
		BYTE *volumeName,
		struct ScreenStruct *screenID,	/* Optional: NULL */
		LONG *volumeNumber);

LONG MountAllVolumes(
		BYTE *volumeName, /* Use NULL for the name to mount all volumes */
		struct ScreenStruct *screenID,	/* Optional: NULL */
		BYTE displayErrorIfNoDisksFlag);

void FreeVolumeTablesForDismount(
		LONG Volume);

void DisplayDriveName(
		struct ScreenStruct *screenID,
		LONG driveID);

LONG ExpandVolumeVectors(
		LONG volumeNumber,
		LONG newNumberOfSegments,
		LONG partitionID,
		LONG startingPartitionSector,
		LONG firstBlockInSegment,
		LONG blockShiftFactor);

LONG InternalMountVolume(
		struct ScreenStruct *screenID,
		struct VolumeStruct *currentVolume,
		BYTE sysVolumeFlag,
		BYTE displayWarningIfAlreadyMountedFlag);

LONG CheckVolumeForDismountEventHandler(
		void (*OutputRoutine)(
				void *controlString, ...),
		LONG volumeNumber,
		LONG unused);

#if DEFECT265394
void CheckForTTSVolumeMount(LONG volumeNumber, LONG TTSSetErrorFlag);
#else
void CheckForTTSVolumeMount(LONG volumeNumber);
#endif

/* End of routines in the VOLMOUNT module */

extern void INWLockVolume(LONG Volume);

extern void INWUnLockVolume(LONG Volume);

/****************************************************************************/
/* Routines in the AUDITSRV module */

LONG RegisterAuditService(
		struct ResourceTagStructure *resourceTag,
		LONG auditServiceVersion,
		struct AuditServiceStructure *AuditServiceEntries,
		struct AuditServiceControlStructure *AuditServiceControlEntries);

LONG DeregisterAuditService(
		struct ResourceTagStructure *resourceTag);

/* End of routines in the AUDITSRV modules */
/****************************************************************************/
/* Routines in the AUDIT module */

LONG AuditNCPReq(
		LONG connectionNumber,
		BYTE *info,
		BYTE *answer,
		LONG *answerLength,
		LONG PacketSize);

LONG NWAuditNLMAddRecord(
		LONG volumeNumber,
		LONG NLMRecordTypeID,
		LONG stationNumber,
		LONG statusCode,
		BYTE *data,
		LONG dataSize );

LONG NWAuditRegisterIDName(
			LONG uniqueID,
			LONG networkAddressType,
			BYTE *networkAddress,
			BYTE *name );

LONG NWAuditSetProcessID(
			LONG uniqueID );

LONG OpenVolumeAuditing(
		LONG volumeNumber,
		struct ScreenStruct *screenID);

LONG CloseVolumeAuditing(
		LONG volumeNumber);

LONG GenerateAuditRecord(
		LONG volumeNumber,
		LONG eventTypeID,
		LONG connectionID,		/* ID of event initiator */
		LONG successFailureStatusCode,
		...);

LONG GenerateAuditRecordDelayedWrite(
		LONG volumeNumber,
		LONG eventTypeID,
		LONG connectionID,		/* ID of event initiator */
		LONG successFailureStatusCode,
		...);

BYTE *GetIDName(
		LONG connectionID,
		LONG objectID,
		BYTE *nameBfr,
		int maxLen);

LONG IsEventAudited(
		LONG volumeNumber,
		LONG bitMapBitNumber );

LONG IsUserAudited(
		LONG connectionID,
		LONG userID);

LONG AuditOSReserved( LONG reserved, ...);

LONG AuditOSService( LONG serviceID, ...);

/* End of routines in the AUDIT modules */
