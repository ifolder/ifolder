#ifndef __FILEIO_H__
#define __FILEIO_H__
/*****************************************************************************
 *
 *	(C) Copyright 1993-1994 Novell, Inc.
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
 *  $Workfile:   FILEIO.H  $
 *  $Modtime:   29 Aug 1998 09:24:58  $
 *  $Revision$
 *
 ****************************************************************************/

#define	NotReadableBit								0x01
#define	NotWriteableBit							0x02
#define	WrittenBit									0x04
#define	DetachedBit									0x08
/* DOCUMENTED WRONG #define	IAmOnTheOpenFileListBit		0x10 */
#define	SwitchingToDirectFileSystemModeBit	0x10
#define	DirectFileSystemModeBit					0x20
#define	FileWriteThroughBit						0x40	;also defined in SERVER.386
/* #define	HasFileWritePrivilegeBit			0x80 **** DEFINED IN BITS.H */

#define	OutOfHandles		129
#define	InvalidFileHandle	136
#define	FileDetached			149
#define	InsufficientMemory	150
#define	NoReadPriviledge		147
#define	IOLockError				162
#define	HardIOError				131
#define	NoWritePriviledge		148
#define	OutOfSpace				1

typedef	struct	FileControlBlockStructuredef
{
/* 00 */	LONG	OpenFileLink;
/* 04 */	LONG	StationLink;
/* 08 */	struct	FileControlBlockStructuredef	*ShareLinkNext;
/* 0C */	struct	FileControlBlockStructuredef	*ShareLinkLast;
/* 10 */	struct	OwnerRestrictionNodeStructure		*OwnerRestrictionNode;
/* 14 */	struct	SpaceRestrictionNodeStructure		*SubdirectoryRestrictionNode;
/* 18 */	LONG	OpenCount;
/* 1C */	LONG	DirectoryEntry;
/* 20 */	LONG	DirectoryNumber;
/* 24 */	LONG	ActualDirectoryEntry;
/* 28 */	LONG	FileSize;
/* 2C */	LONG	FirstCluster;
/* 30 */	LONG	CurrentBlock;
/* 34 */	LONG	CurrentCluster;
/* 38 */	struct	FATStruct	*FATTable;
/* 3C */	LONG	TNodePointer;
/* 40 */	LONG	DateValueStamp;
/* 44 */	void	*TurboFAT;
/* 48 */	LONG	OldFileSize;
/* 4C */	LONG	TransactionPointer;
/* 50 */	LONG	Station;
/* 54 */	LONG	Task;
/* 58 */	BYTE	HandleCount[4];
/* 5C */	BYTE	Flags;
/* 5D */	BYTE	TTSFlags;
/* 5E */	BYTE	ByteToBlockShiftFactor;
/* 5F */	BYTE	BlockToSectorShiftFactor;
/* 60 */	BYTE	SectorToBlockMask;
/* 61 */	BYTE	VolumeNumber;
/* 62 */	BYTE	ExtraFlags;
/* 63 */	BYTE	DataStream;
/* 64 */	LONG	CommitSemaphore;
/* 68 */	LONG	ActualOldLastCluster;
/* 6C */	LONG	FCBInUseCount;
/* 70 */	BYTE	ExtraExtraFlags;
/* 71 */	BYTE	SubAllocFlags;
/* 72 */	BYTE	RemoveSkipUpdateFlags;
/* 73 */	BYTE	DeCompressFlags;
/* 74 */	LONG	VolumeManagerID;
/* 78 */	LONG	SubAllocSemaphore;
/* 7C */	LONG	SAStartingSector[2];
/* 84 */	LONG	SANumberOfSectors[2];
/* 8C */	LONG	SAFATCount;
/* 90 */	LONG	TempCacheListHead;
/* 94 */	LONG	TempCacheListTail;
/* 98 */	struct CompressControlNodeStructure	*CompressControlNode;
/* 9C */	struct	FileControlBlockStructuredef	*DeCompressFCB;
/* A0 */	LONG	DeCompressPosition;
/* A4 */	LONG	DeCompressHandle;
/* A8 */	LONG	RALastReadStartOffset;
/* AC */	LONG	RALastReadEndOffset;
/* B0 */	LONG	RANextReadAheadOffset;
/* B4 */	LONG	RAHalfSize;
/* B8 */	LONG	MoreFlags; /*defined in bits.h */
/* BC */	LONG	StationBackLink;
/* C0 */	SPINLOCK	DFSSpinLock;
/* C4 */	LONG	DFSUseCount;
/* C8 */	LONG	DFSCurrentCluster;
/* CC */	LONG	DFSCurrentBlock;
/* D0 */	LONG	unused;	/* reserved space for future */
/* D4 */	LONG	unused1;
/* D8 */	LONG	unused2;
/* DC */	LONG	unused3;
} FCBType;

/* define the TTSFlags */

#define	TransactionBackoutBit		1
#define	TransactionActiveBit			2

/* define the ExtraFlags */

#define	DiskBlockReturnedBit				0x1
#define	IAmOnTheOpenFileListBit			0x2
#define	FileReadAuditBit					0x4
#define	FileWriteAuditBit					0x8
#define	FileCloseAuditBit					0x10
#define	DontFileWriteSystemAlertBit	0x20
#define	ReadAheadHintBit					0x40
#define	NotifyCompressionOnCloseBit	0x80

/* define the ExtraExtraFlags */
#define	IsWritingCompressedBit		0x1
#define	HasTimeDateBit					0x2
#define	DoingDeCompressionBit		0x4
#define	NoSubAllocBit					0x8
#define	IsATransactionFileBit		0x10
/* #define	HasFileWritePrivilegeBit	0x20 */
#define	TTSReadAuditBit				0x40
#define	TTSWriteAuditBit				0x80

/* define the RemoveSkipUpdateFlags */

#define	RemoveSkipFATChainStuffBit		1
#define	RemoveReturnFATChainBit			2
#define	RemoveSkipDirectoryUpdateBit	4
#define	RemoveSkipDirectoryFATWriteBit	8
#define	RemoveCheckSubAllocBit			0x10
#define	RemoveSkipDiskSpaceTrackingBit	0x20
#define	RemoveSkipArchiveBit	   		0x40
#define	RemoveUpdateCompressSectors		0x80

/* define the DeCompress Flags	*/

#define	DeCompressWantsCompressionBit	1
#define	DeCompressThrowAwayCompressionBit		2
#define	DeCompressHardErrorBit		4
#define	DeCompressHoldOffOrAbortBit	8
#define	DeCompressDeCompressionDoneBit		0x10
#define	DeCompressCanLeaveCompressedBit	0x20
#define	DeCompressWillBeWrittenBit		0x40
#define	DeCompressProcessedWritesDoneBit	0x80

#define	DeCompressHardErrorDueToInsufficientSpaceBit	1	/* used only if HardErrorBit is set */
#define	DeCompressHardErrorDueToSpaceRestrictionCheckBit	2	/* ''' */


extern LONG MapFileHandleToFCB( LONG handle, FCBType **fcb );

#endif /* __FILEIO_H__ */
