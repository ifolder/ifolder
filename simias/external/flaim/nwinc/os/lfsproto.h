extern LONG	EAAccessTable[];
extern LONG VolumeFreeBlocks[];
extern BYTE VolumeMountedFlag[];
extern LONG UsedDirectoryEntriesCount[];
extern BYTE *VolumeNameTable[];
extern LONG VolumeFreeSectors[];
extern LONG RestrictionBlocksPerVolume[];
extern LONG MACRootIDs[];
extern LONG VolumeLastModifiedDateAndTime[];
extern LONG *SubdirectoryNumberVectorTable[];
extern LONG *AvailableBlockTable[];
extern LONG *SubdirectoryBitTable[], *TrusteeBitTable[];
extern LONG *FileWithTrusteeBitTable[];
extern LONG *DiskSector0VectorTable[], *DiskSector1VectorTable[];
extern LONG LastAllocatedBlock0[], LastAllocatedBlock1[];
extern LONG *NextBlockListTable[], *AvailBitMapTable[];
extern LONG *FirstBlockVectorTable[];
extern LONG SectorsPerAllocationUnitVector[];
extern LONG *HashSearchListTable[];
extern LONG HashIndexMaskTable[];
extern LONG *EntryLockedBitTable[];
extern LONG NumberOfVolumes;
extern LONG *VolumeStartingSectorVectorTable[];
extern LONG *VolumePartitionIDVectorTable[];
extern LONG *VolumePhysicalDriveOffsetVector[];
extern BYTE VolumeNumberOfDrives[];
extern LONG *VolumeDriveStartVector[];
extern LONG *VolumeDriveAllocStartVector[];
extern BYTE VolumeAllocCurrentDrive[];
extern LONG DirectoryLengthInBlocks[];

void ClearPhantom(		/* moved to vswitch.386 */
		LONG Volume,
		LONG EntryNumber);

LONG EraseFile( 	/* moved to vswitch.386 */
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG MatchBits);

LONG RenameEntry(		/* moved to vswitch.386 */
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG MatchBits,
		BYTE SubdirectoryFlag,
		LONG NewBase,
		BYTE *NewString,
		LONG NewCount,
		LONG CompatabilityFlag,
		BYTE AllowRenamesToMyselfFlag);

LONG OpenFile(			/* moved to vswitch.386 */
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG MatchBits,
		LONG RequestedRights,
		BYTE DataStreamNumber,
		LONG *Handle,
		LONG *DirectoryNumber,
		void **DirectoryEntry);

LONG CreateFile(		/* moved to vswitch.386 */
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG CreatedAttributes,
		LONG FlagBits,
		BYTE DataStreamNumber,
		LONG *Handle,
		LONG *DirectoryNumber,
		void **DirectoryEntry);

LONG CreateAndOpenFile( 	/* moved to vswitch.386 */
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
		LONG *Handle,
		LONG *DirectoryNumber,
		void **DirectoryEntry);

LONG CloseFile(
		LONG station,
		LONG task,
		LONG handle);

LONG CreateDirectory(
		LONG Station,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG DirectoryAccessMask,
		LONG *ReturnedDirectoryNumber,
		void **ReturnedSubDir);

LONG DeleteDirectory(		/* moved to vswitch.386 */
		LONG Station,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace);
LONG MapPathToDirectoryNumber(	/* moved to vswitch.386 */
		LONG Station,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG *DirectoryNumber,
		LONG *FileFlag);

LONG MapPathToDirectoryNumberOrPhantom(
		LONG Station,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG *DirectoryNumber,
		LONG CanCreateFlag);

LONG GetAccessRights(	/* moved to vswitch. 386 */
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		WORD *AccessRights);

LONG MapDirectoryNumberToPath(
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		BYTE *String,
		LONG StringLength,
		LONG *ActualLength);

LONG GetEntryFromPathStringBase(		/* moved to vswitch.386 */
		LONG Station,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG SourceNameSpace,
		LONG DesiredNameSpace,
		struct DirectoryStructure **Dir,
		LONG *DirectoryNumber);

LONG GetOtherNameSpaceEntry(		/* moved to vswitch.386  */
		LONG Volume,
		LONG DirectoryNumber,
		struct DirectoryStructure *Dir,
		LONG SourceNameSpace,
		LONG DesiredNameSpace,
		struct DirectoryStructure **ReturnedDir,
		LONG *ReturnedDirectoryNumber);

LONG DirectorySearch(		/* moved to vswitch.386 */
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		LONG StartEntryNumber,
		BYTE *Pattern,
		LONG MatchBits,
		struct DirectoryStructure **DirectoryEntry,
		LONG *ReturnedDirectoryNumber);

LONG GetExtendedDirectoryInfo(	/* moved to vswitch.386 */
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		struct DirectoryStructure *DirectoryEntry,
		struct GetStructure *GetVector,
		LONG GetBits);

LONG CheckAndGetDirectoryEntry(
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		union DirUnion **Dir);


LONG AddTrusteeRights(	/* moved to vswitch.386 */
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		LONG Trustee,
		LONG NewRights);

LONG ScanTrusteeRights( 	/* moved to vswitch.386 */
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		LONG StartingOffset,
		LONG *TrusteeVector,
		WORD *MaskVector,
		LONG VectorSize,
		LONG *ActualVectorSize);

LONG DeleteTrusteeRights(		/* moved to vswitch.386 */
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		LONG DeletedTrustee,
		BYTE CanDeleteOnPurgedFileFlag);

LONG GetAccessRightsFromIDs(
		LONG Volume,
		LONG DirectoryNumber,	/* assumes it is in the DOS name space. */
		LONG UserID,
		LONG GroupID,
		WORD *UserAccessRights,
		WORD *GroupAccessRights);

LONG GetParentDirectoryNumber(
		LONG Volume,
		LONG DirectoryNumber);

LONG PurgeTrustee(
		LONG Volume,
		LONG *TrusteeVector,
		LONG TrusteeCount);


LONG FindNextTrusteeReference(
		LONG Station,
		LONG Volume,
		LONG TrusteeID,
		LONG StartEntryNumber,
		LONG *EntryNumber,
		WORD *TrusteeMask,
		LONG *OwnerEntryNumber);

LONG ScanUserRestrictionNodes(
		LONG Station,
		LONG Volume,
		LONG StartingTrusteeNumber,
		LONG NumberOfTrustees,
		BYTE *AnswerReturnArea,
		LONG *AnswerLength);

LONG AddUserRestriction(
		LONG Station,
		LONG Volume,
		LONG Trustee,
		LONG Value);

LONG DeleteUserRestriction(
		LONG Station,
		LONG Volume,
		LONG Trustee);

LONG GetActualAvailableDiskSpace(
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		long *ReturnedValue);

LONG GetRawFileSize( LONG handle, /* moved to vswitch.386 */
							LONG *fileSize);

LONG GetHandleInfo(
		LONG Station,
		LONG Handle,
		LONG NameSpace,
		LONG *Volume,
		LONG *DirectoryNumber,
		LONG *StreamNumber);

LONG CountOwnedFilesAndDirectories(
		LONG Station,
		LONG Volume,
		LONG ID,
		LONG *FileCount,
		LONG *DirectoryCount);

LONG ScanDeletedFiles(		/* moved to vswitch.386 */
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		LONG StartEntryNumber,
		LONG *ReturnedDirectoryNumber,
		struct DirectoryStructure **EntryPointer);

LONG SalvageDeletedFile(	/* moved to vswitch.386 */
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		LONG ToBeSalvagedDirectoryNumber,
		LONG NameSpace,
		BYTE *NewFileName);

LONG PurgeDeletedFile(	/* moved to vswitch.386 */
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		LONG ToBePurgedDirectoryNumber,
		LONG NameSpace);

LONG GetDeletedEntry(
		LONG Station,
		LONG Volume,
		LONG DOSDirectoryNumber,
		LONG DesiredNameSpace,
		union DirUnion **Dir);

LONG CommitFile(
		LONG Station,
		LONG OpenFileHandle);

LONG GetOriginalNameSpace(
		LONG Volume,
		LONG DirectoryNumber,
		LONG *OriginalNameSpace);

LONG GetOriginalInfo(
		LONG Volume,
		LONG DirectoryNumber,
		LONG *OriginalNameSpace,
		LONG *OriginalEntryNumber,
		void *OriginalFileName);

BYTE *AddNameSpace(
		LONG Volume,
		LONG NameSpace,
		struct ScreenStruct *screenID,
		LONG *NeedToDismountFlag);

LONG GetActualFileSize(
		LONG Volume,
		LONG DirectoryNumber,
		LONG NameSpace,
		LONG DataStreamNumber,
		LONG *FileSize,
		LONG *OpenFlag);


LONG SetCompressedFileSize(	/* moved to vswitch.386 */
		LONG Station,
		LONG Handle,
		LONG NewFileSize,
		LONG *rtnOldFileSize);


LONG RenameNameSpaceEntry(	/* moved to vswitch.386 */
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG MatchBits,
		BYTE *NewString);

LONG	GetReferenceCount(
			LONG Station,
			LONG Handle,
			LONG *ReferenceCount);

LONG	GetReferenceCountFromEntry(	/*move to vswitch.386 */
			LONG Station,
			LONG Volume,
			LONG DirectoryBase,
			LONG NameSpace,
			LONG *ReferenceCount);

LONG	SetOwningNameSpace(
			LONG Station,
			LONG Volume,
			LONG PathBase,
			BYTE *PathString,
			LONG PathCount,
			LONG NameSpace,
			LONG NewOwningNameSpace);

LONG	AddFSMonitorHook(
		struct ResourceTagStructure *RTag,
		LONG	hookNumber,
		void	*fsProcedure);

LONG	RemoveFSMonitorHook(
		struct ResourceTagStructure *RTag,
		LONG	hookNumber,
		void	*fsProcedure);


LONG AddTrustee(
		LONG Station,
		LONG Base,
		BYTE *ModifierString,
		LONG Trustee,
		LONG TrusteeMask,
		BYTE CanAddToFileFlag);

LONG GetActiveVolumeIOs(
		LONG	Volume,
		LONG	*activeIOs,
		LONG	*activeCompressionIOs);

void DismountVolume(
		LONG Volume,
		struct ScreenStruct *screenID); /* Optional: NULL */

LONG FlushVolume(
		LONG volumeNumber);

LONG INWNewGetVolumeInfo(
		LONG	Volume,
		struct	VolInfoStructure	*VolInfo);

LONG ModifyDirectoryEntry(		/* moved to vswitch.386 */
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG MatchBits,
		LONG TargetNameSpace,
		struct ModifyStructure *ModifyVector,
		LONG ModifyBits,
		LONG AllowWildCardsFlag);

LONG ModifyDOSAttributes(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG MatchBits,
		struct ModifyStructure *ModifyVector,
		LONG ModifyBits,
		LONG AllowWildCardsFlag,
		LONG *changecount,
		LONG *entriesFound);

LONG	GetVolumeName(
			LONG Volume,
			LONG NameSpace,
			BYTE *vname);

LONG GetNameSpaceInformation(
		LONG Volume,
		BYTE *Answer,
		LONG *AnswerLength);

LONG VerifyNameSpaceNumber(
		LONG Volume,
		LONG NameSpace);

LONG VerifyDataStreamNumber(
		LONG Volume,
		LONG DataStream);

LONG CheckVolumeNumber(
		LONG Volume,
		LONG NameSpace);

LONG ReturnDataStreamName(
		LONG WhichDataStream,
		BYTE *NameBuff);

LONG ReturnNameSpaceName(
		LONG WhichNameSpace,
		LONG ShortNameFlag,
		BYTE *NameBuff);
LONG VM_Register_Volume(
		LONG *volumemanagerhandle,
		LONG filesystemhandle,
		LONG volumeinstance,
		BYTE *volumename,
		LONG volumestatus,
		LONG filesystemid,
		BYTE *filesystemname,
		LONG (*pollroutine)(
				LONG handle,
				struct volumerequestdef *request),
		LONG (*abortroutine)(
				LONG handle,
				struct volumerequestdef *request),
		LONG (**jumptable)(void));

LONG VM_Unregister_Volume(
		LONG volumemanagerhandle,
		LONG filesystemhandle);

LONG VM_Change_Volume_Status(
		LONG volumemanagerhandle,
		LONG filesystemhandle,
		LONG newinstance,
		BYTE *newname,
		LONG newstatus,
		LONG (**jumptable)(void));

LONG VM_Return_Volume_Info(
		LONG volumeid,
		struct volinfodef *volumeinfo);

LONG VM_Get_Volume_ID(
        LONG fileSystemID,
        LONG fileSystemHandle,
        LONG *volumeID);

LONG VM_Volume_Request(
		LONG *requesthandle,
		LONG apprequesthandle,
		LONG volumeid,
		LONG function,
		LONG parameter1,
		LONG parameter2,
		LONG parameter3,
		void (*callbackroutine)(
				struct volumerequestdef *request,
				void *parameter,
				LONG completionCode),
		struct resourcetagdef *resourcetag);

void VM_Volume_Abort_Request(
		struct volumerequestdef *request,
		LONG apprequesthandle);

void VM_Volume_Request_Complete(
		struct volumerequestdef *request,
		LONG completioncode);

LONG	ReturnVolumeMappingTableSize(void);

LONG VM_Find_Next_Volume(
		LONG 	*nextvolumeid,
		LONG	filesystemid);

LONG GetVolumeUsageStatistics(
		LONG Volume,
		BYTE *ReplyArea);

LONG GetDirectoryUsageStatistics(
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		BYTE *ReplyArea);

LONG GetActualVolumeUsageStatistics(
		LONG Volume,
		BYTE *ReplyArea);

LONG GetActualDirectoryUsageStatistics(
		LONG Station,
		LONG Volume,
		LONG DirectoryNumber,
		BYTE *ReplyArea);

LONG GetDirectoryLengthInBlocks(
		LONG volumeNumber);


LONG GetMACRootIDs(
		LONG volumeNumber);

LONG GetRestrictionBlocksPerVolume(
		LONG volumeNumber);

LONG GetUsedDirectoryEntriesCount(
		LONG volumeNumber);

LONG GetVolumeFreeBlocks(
		LONG volumeNumber);

LONG GetVolumeLastModifiedDateAndTime(
		LONG volumeNumber);

LONG GetVolumeMountedFlag(
		LONG volumeNumber);

LONG GetVolumeNameTableEntry(
		LONG volumeNumber,
		BYTE *nameBuffer);

LONG GetNumberOfVolumes(void);

LONG	ParseTree(
			LONG Station,
			LONG Volume,
			LONG DirectoryNumber,
			LONG NameSpace,
			LONG StartEntryNumber,
			LONG Flags,
			LONG TreeInfoMask,
			LONG TreeInfoMask2,
			void *info,
			LONG *infoNextEntryNumber,
			LONG *infoLen,	/* input (max size of info), output (used amount)*/
			LONG *infoItems);


LONG CreateMigratedDirectory(
		LONG Station,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG DirectoryAccessMask,
		LONG *ReturnedDirectoryNumber,
		void **ReturnedSubDir,
		LONG BindFlag,
		LONG SupportModuleID,
		LONG *BindKeyNumber);



LONG GetVolumeOwnerRestrictionNodes(
		LONG Volume,
		LONG bufferSizeInBytes,
		LONG index,
		void *buffer,
		LONG *numberOfNodesReturned,
		LONG *nextIndex);

LONG VMGetDirectoryEntry(
		LONG volumeNumber,
		LONG directoryEntry,
		void *directoryEntryPointer);

#define	RPC_BIT		0x1
#define	FORCE_BIT	0x2

LONG ProcessDismountVolume(
		struct ScreenStruct *screenID,
		BYTE *commandLine,
		LONG ControlBitsFLAG);

LONG ProcessMountVolume(
		struct ScreenStruct *screenID,
		BYTE *commandLine);

#define ASTERISK					'*'
#define AUGMENTED_ASTERISK			('*' | 0x80)
#define QUESTION_MARK				'?'
#define AUGMENTED_QUESTION_MARK		('?' | 0x80)
#define PERIOD						'.'
#define AUGMENTED_PERIOD			('.' | 0x80)
#define BREAK_CHARACTER				0xff

#define UPPERCASE_COMPONENTS		0x1
#define PREPEND_BREAK_ON_WILDCARD	0x2
#define PREPEND_BREAK_ON_BREAK 		0x4

LONG BuildComponentPathString(
		BYTE *modifierString, 
		LONG flags, 
		LONG *volumeNumber, 
		BYTE *pathString, 
		LONG *pathCount);
