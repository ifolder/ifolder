#ifndef __NSPACE_H__
#define __NSPACE_H__
/*****************************************************************************
 *
 *	(C) Copyright 1988-1994 Novell, Inc.
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
 *  $Workfile:   nspace.h  $
 *  $Modtime:   02 Aug 2000 13:50:44  $
 *  $Revision$
 *  
 ****************************************************************************/

#include <config.h>

/****************************************************************************/

 
/*	define the routine vectors and defines for the name spaces.	*/


/* 	#define	MaximumNumberOfNameSpaces	10  moved to config.h  JDM */
#define DOSNameSpace			0
#define MACNameSpace			1
#define UNIXNameSpace		2
#define FTAMNameSpace		3
#define OS2NameSpace			4
#define NTNameSpace			5
/* Name Spaces 6 - 9 --> are reserved for Novell use only */

/* Name Space Flags */
#define	UNICODE_SUPPORT	0x00000001

/* Search Attribute to include all files & subdirectories */
#define SEARCH_INCL_ALL		0x8000
/* MatchBits flag to return more information about error */
#define RETURN_EXTENDED_ERROR_CODE 0x80000000

/* RemoveEntryName Event Notification Defines */
#define	RE_rename_or_move_notification		0x4e650000
#define	RE_delete_notification			0x4e650001
#define	RE_open_file_notification		0x4e650002
#define	RE_dirhandle_create_phantom_entry	0x4e650003
#define	RE_rename_entry				0x4e650004
#define	RE_create_directory			0x4e650005
#define	RE_delete_directory			0x4e650006
#define	RE_clear_phantom			0x4e650007
#define	RE_create_and_open_file		 	0x4e650008
#define	RE_delete_hard_link_file	 	0x4e650009
#define	RE_delete_hard_link_directory	 	0x4e65000a
#define	RE_delete_file_completely		0x4e65000b
#define	RE_delete_file_to_limbo			0x4e65000c
#define	RE_salvage_limbo_file			0x4e65000d
#define	RE_change_directory			0x4e65000e

extern BYTE	NameSpaceLoadedVector[MaximumNumberOfNameSpaces];
extern LONG	NumberOfDefinedNameSpaces[MaximumNumberOfVolumes];
extern LONG NumberOfDefinedDataStreams[MaximumNumberOfVolumes];
extern LONG	MapNameSpaceToPosition[MaximumNumberOfVolumes][MaximumNumberOfNameSpaces];
extern LONG	MapPositionToNameSpace[MaximumNumberOfVolumes][MaximumNumberOfNameSpaces];
extern LONG	NameSpaceLoadedOnVolumeMask[MaximumNumberOfVolumes];
extern LONG	FixedFieldsMask[MaximumNumberOfNameSpaces];
extern LONG	VariableFieldsMask[MaximumNumberOfNameSpaces];
extern LONG	HugeFieldsMask[MaximumNumberOfNameSpaces];
extern BYTE	*NSFieldsLengthTable[MaximumNumberOfNameSpaces];
extern BYTE NeedsCreateArchiveNoticeTable[MaximumNumberOfNameSpaces];
extern BYTE	*WildCardPattern[MaximumNumberOfNameSpaces];
extern BYTE	*NameSpaceName[MaximumNumberOfNameSpaces];
extern BYTE	*NameSpaceShortName[MaximumNumberOfNameSpaces];
extern LONG	MapDataStreamToNameSpace[MaximumNumberOfDataStreams];
extern LONG MapNameSpaceToDataStream[MaximumNumberOfNameSpaces];
extern BYTE	*DataStreamName[MaximumNumberOfDataStreams];

extern BYTE	*(*StartVolumeMount[MaximumNumberOfNameSpaces])(); 
extern void	(*AbortVolumeMount[MaximumNumberOfNameSpaces])(); 
extern void	(*EndVolumeMount[MaximumNumberOfNameSpaces])(); 
extern void	(*DoVolumeDismount[MaximumNumberOfNameSpaces])();
extern LONG	(*GrowDirectoryTables[MaximumNumberOfNameSpaces])(); 

// DEFECT 238120 Jim A. Nicolet
//extern BYTE	*(*VerifyOnlyName[MaximumNumberOfNameSpaces])(); 
extern BYTE	*(*VerifyNameOnly[MaximumNumberOfNameSpaces])(); 

extern BYTE	*(*VerifyOtherFields[MaximumNumberOfNameSpaces])(); 
extern void	(*MarkDeletedFile[MaximumNumberOfNameSpaces])(); 
extern LONG	(*VerifyDeletedFile[MaximumNumberOfNameSpaces])(); 
extern void	(*DirectoryEntryUsed[MaximumNumberOfNameSpaces])();
extern LONG	(*ClearExtraNameSpaceEntries[MaximumNumberOfNameSpaces])();

extern LONG	(*GetDirectoryHandle[MaximumNumberOfNameSpaces])(
		LONG volumeNumber,
		LONG directoryNumber,
		LONG stationNumber,
		struct DirectoryHandleStructure **directoryHandle);

extern LONG	(*FindDirectoryEntry[MaximumNumberOfNameSpaces])(
		LONG volumeNumber,
		LONG directoryEntry,
		BYTE *pathAndName,
		LONG componentCount,
		LONG stationNumber,
		struct DirectoryInfoReturn *directoryInfo);

extern LONG	(*FindDirectoryEntryOrPhantom[MaximumNumberOfNameSpaces])(
		LONG volumeNumber,
		LONG directoryEntry,
		BYTE *pathAndName,
		LONG componentCount,
		LONG stationNumber,
		struct DirectoryInfoReturn *directoryInfo,
		LONG directoryEntryNameSpace);

extern LONG 	(*WildSearchDirFromInfo[MaximumNumberOfNameSpaces])(
		LONG volumeNumber,
		struct DirectoryInfoReturn *directoryInfo,
		LONG stationNumber);

extern LONG	(*MatchAttributes[MaximumNumberOfNameSpaces])(
		LONG  Volume,
		union DirUnion	*Dir,
		LONG	MatchBits);

extern LONG	(*CheckAndConvertName[MaximumNumberOfNameSpaces])(
		BYTE *Name,
		BYTE *ConvertedName);

extern void	(*UnConvertName[MaximumNumberOfNameSpaces])(
		BYTE *ConvertedName,
		BYTE *UnConvertedName);

extern LONG	(*SetEntryName[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG EntryNumber,
		BYTE *FileName,
		union DirUnion	*Dir);

extern LONG	(*SetDeletedFileEntryName[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG EntryNumber,
		BYTE *FileName,
		union DirUnion	*Dir);

extern LONG	(*RenameEntryName[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG EntryNumber,
		BYTE *NewName,
		union DirUnion *Dir);

extern void	(*RemoveOtherInfo[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG EntryNumber,
		union DirUnion *Dir);

extern void	(*RemoveInfoFromLimboFile[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG EntryNumber,
		union DirUnion *Dir);

extern LONG	(*TransferFileNameAndInfoToLimbo[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG OldEntryNumber,
		LONG NewEntryNumber,
		union DirUnion *OldDir,
		union DirUnion *NewDir,
		LONG OldUserID,
		LONG NewUserID);

extern LONG	(*TransferOnlyInfoToLimbo[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG OldEntryNumber,
		LONG NewEntryNumber,
		union DirUnion *OldDir,
		union DirUnion *NewDir,
		LONG OldUserID,
		LONG NewUserID);

extern LONG	(*TransferLimboInfoToFile[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG OldEntryNumber,
		LONG NewEntryNumber,
		union DirUnion *OldDir,
		union DirUnion *NewDir,
		LONG OldUserID,
		LONG NewUserID);

extern void	(*ChangeStreamsToLimbo[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG EntryNumber,
		union DirUnion	*Dir,
		LONG WhoOwnedItID);

extern void	(*DeleteLimboFile[MaximumNumberOfNameSpaces])(
		LONG Volume,
		union DirUnion	*Dir,
		BYTE AdjustFreeableLimboBlocksFlag,
		LONG IsCompressedFlag);

extern LONG	(*ExhumeLimboFile[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG EntryNumber,
		union DirUnion	*Dir,
		LONG WhoDidItID,
		LONG *BlockCount);

extern void	(*SetRestOfFileEntry[MaximumNumberOfNameSpaces])(
		union DirUnion	*Dir,
		LONG		CreatedAttributes,
		LONG		UserID,
		LONG		CreateDOSDateAndTime,
		LONG		CreateSecondsRelative,
		LONG		LastArhivedDOSDateAndTime,
		LONG		LastArchivedSecondsRelative);

extern void	(*GenerateUniqueName[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG Subdirectory,
		BYTE *SourceName,
		BYTE *UniqueName,
		BYTE UniquenessRequiredFlag,
		LONG SourceNameSpace);

/* define the DatesChangedFlags */

#define	CreationDateTimeChangedBit	1
#define	ArchivedDateTimeChangedBit	2

extern void (*CreateArchiveChanged[MaximumNumberOfNameSpaces])(
		union DirUnion	*Dir,
		LONG	DatesChangedFlags,
		LONG	NewCreationTimeInSeconds,
		LONG	NewArchivedTimeInSeconds);

/* ***************************************************************************
NOTE: The first parameter (LONG) after the "union DirUnion *Dir" parameter 
      is the RemoveEntryName Event Notification value (used by NFS to decide
		who called RemoveEntryName).
*************************************************************************** */
extern void	(*RemoveEntryName[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG EntryNumber,
		union DirUnion	*Dir,
		...);

extern LONG	(*ModifyDirectoryFields[MaximumNumberOfNameSpaces])(
		LONG	Volume,
		LONG	EntryNumber,
		union DirUnion	*Dir,
		WORD    Rights,
		struct ModifyStructure		*ModifyVector,
		LONG	ModifyBits,
		BYTE	SubdirectoryFlag,
		BYTE	CreateOnlyFlag);

extern LONG	(*WildReplace[MaximumNumberOfNameSpaces])(
		LONG	Volume,
		BYTE *searchPattern,
		union DirUnion *Dir,
		BYTE *replacePattern,
		BYTE *replacedString);

extern void	(*SetRestOfSubDirEntry[MaximumNumberOfNameSpaces])(
		union DirUnion	*SubDir,
		LONG DirectoryAccessAttributes,
		LONG UserID,
		LONG		CreateDOSDateAndTime,
		LONG		CreateSecondsRelative,
		LONG		LastArhivedDOSDateAndTime,
		LONG		LastArchivedSecondsRelative);

extern LONG	(*GetSpaceUtilized[MaximumNumberOfNameSpaces])(
		LONG volume,
		union DirUnion	*Dir);

extern void	(*ChangeToPhantomEntry[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG EntryNumber,
		union DirUnion	*Dir);

extern LONG	(*GetName[MaximumNumberOfNameSpaces])(
		LONG Volume,
		union DirUnion	*Dir,
		BYTE *returnedName);

extern LONG	(*GetNameLength[MaximumNumberOfNameSpaces])(
		LONG Volume,
		union DirUnion	*Dir,
		LONG *returnedNameLength);

extern LONG	(*GetPath[MaximumNumberOfNameSpaces])(
		LONG Volume, 
		LONG EntryNumber, 
		BYTE *String,
		LONG StringLength,
		LONG *ActualLength);

extern LONG	(*WildSearchDirectory[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG DirectoryNumber, 
		LONG StartEntryNumber,
		BYTE *Pattern,
		LONG Station,
		union DirUnion **DirectoryEntry,
		LONG *ReturnedDirectoryNumber, 
		WORD *AccessRights);

extern LONG	(*WildSearchDirectoryEntry[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG DirectoryNumber, 
		LONG StartEntryNumber,
		BYTE *Pattern,
		LONG Station,
		struct DirectoryInfoReturn *directoryInfo);

extern BYTE	*(*StartNameSpaceAdditionToVolume[MaximumNumberOfNameSpaces])(
		LONG Volume);

extern LONG	(*TransferInfo[MaximumNumberOfNameSpaces])(
		LONG Volume,
		LONG OldEntryNumber,
		LONG NewEntryNumber,
		union DirUnion	*OldDir,
		union DirUnion	*NewDir,
		LONG	OldUserID,
		LONG	UserID);

extern LONG	(*CompareFileNames[MaximumNumberOfNameSpaces])(
		BYTE	*FirstName,
		BYTE	*SecondName);

extern LONG	(*CheckNameIgnoreLocks[MaximumNumberOfNameSpaces])(
		LONG volume,
		LONG subdirectoryEntryNumber,
		BYTE *fileName,
		LONG *entryNumber);

extern LONG	(*GetExtendedInfo[MaximumNumberOfNameSpaces])(
		LONG volume,
		LONG directoryEntryNumber,
		union DirUnion	*dir,
		struct GetStructure	*getVector,
		LONG getBits);

extern LONG	*(*GetDataStream[MaximumNumberOfDataStreams])(
		union DirUnion	*Dir,
		LONG	*FileSize);

extern void	(*ChangeDataStream[MaximumNumberOfDataStreams])(
		union DirUnion	*Dir,
		LONG	FileSize,
		LONG	FirstCluster);

extern LONG	(*GetHugeInfo[MaximumNumberOfNameSpaces])(
		LONG volume,
		LONG DirectoryEntryNumber,
		union DirUnion	*dir,
		LONG HugeMask,
		BYTE *StateInfo,
		BYTE *HugeInfoBuffer,
		LONG *BufferSize,
		BYTE *NextStateInfo);

extern LONG	(*SetLowLevelHugeInfo[MaximumNumberOfNameSpaces])(
		LONG volume,
		LONG DirectoryEntryNumber,
		union DirUnion	*dir,
		LONG HugeMask,
		BYTE *StateInfo,
		BYTE *HugeInfoBuffer,
		LONG BufferSize,
		BYTE *NextStateInfo,
		LONG *BytesConsumed,
		LONG *WorkSpaceSize,
		BYTE *WorkSpace);

extern LONG	(*SetHighLevelHugeInfo[MaximumNumberOfNameSpaces])(
		LONG volume,
		LONG DirectoryEntryNumber,
		LONG HugeMask,
		BYTE *StateInfo,
		BYTE *HugeInfoBuffer,
		LONG BufferSize,
		BYTE *NextStateInfo,
		LONG *BytesConsumed,
		LONG *WorkSpaceSize,
		BYTE *WorkSpace);

extern LONG SetClrNameSpaceName(
		LONG WhichNameSpace,
		BYTE *LongNameBuff,	/* byte len preceded */
		BYTE *ShortNameBuff	/* byte len preceded */);

/****************************************************************************/
/****************************************************************************/

extern	LONG (*UnicodeToAscii[MaximumNumberOfNameSpaces])(
		BYTE *UnicodeName,
		BYTE *AsciiName); 

extern	LONG (*AsciiToUnicode[MaximumNumberOfNameSpaces])(
		BYTE *AsciiName,
		BYTE *UnicodeName);

extern	LONG NameSpaceFlags[MaximumNumberOfNameSpaces];


#endif /* __NSPACE_H__ */
