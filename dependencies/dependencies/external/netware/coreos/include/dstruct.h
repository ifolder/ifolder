#ifndef __DSTRUCT_H__
#define __DSTRUCT_H__
/*****************************************************************************
 *
 *	(C) Copyright 1988-1995 Novell, Inc.
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
 *  $Workfile:   dstruct.h  $
 *  $Modtime:   17 May 2001 09:36:28  $
 *  $Revision$
 *  
 ****************************************************************************/

#define	NumberOfDirectoryTrustees	4
#define	OldNumberOfDirectoryTrustees	6

#define	NumberOfSubdirectoryTrustees	8
#define	NumberOfTrusteeTrustees		16
#define	NumberOfRestrictionTrustees	14
#define	NumberOfSubAllocFATChains		28


struct DirectoryStructure
{
		int 	DSubdirectory;
		LONG	DFileAttributes;
		BYTE	DUniqueID;
		BYTE	DFlags;
		BYTE	DNameSpace;
		BYTE	DFileNameLength;
		BYTE	DFileName[12];
		LONG	DCreateDateAndTime;
		LONG	DOwnerID;
		LONG	DLastArchivedDateAndTime;
		LONG	DLastArchivedID;
		LONG	DLastUpdatedDateAndTime;
		LONG	DLastUpdatedID;
		LONG	DFileSize;
		LONG	DFirstBlock;
		LONG	DNextTrusteeEntry;
		LONG	DTrustees[NumberOfDirectoryTrustees];
		LONG	DLookUpEntryNumber;
		int		DLastUpdatedInSeconds;
		WORD	DTrusteeMask[NumberOfDirectoryTrustees];
		WORD	DChangeReferenceID;
		WORD	DLastAccessedTime;		/* SPD #200183 Jim A. Nicolet - 6-25-98 */
		WORD	DMaximumAccessMask;
		WORD	DLastAccessedDate;
		LONG	DDeletedFileTime;
		LONG	DDeletedDateAndTime;
		LONG	DDeletedID;
		LONG	DExtendedAttributes;
		LONG	DDeletedBlockSequenceNumber;
		LONG	DPrimaryEntry;
		LONG	DNameList;
};

struct SubdirectoryStructure
{
		int 	SDSubdirectory;
		LONG	SDFileAttributes;
		BYTE	SDUniqueID;
		BYTE	SDFlags;
		BYTE	SDNameSpace;
		BYTE	SDFileNameLength;
		BYTE	SDFileName[12];
		LONG	SDCreateDateAndTime;
		LONG	SDOwnerID;
		LONG	SDLastArchivedDateAndTime;
		LONG	SDLastArchivedID;
		LONG	SDLastModifiedDateAndTime;
		LONG	SDNextTrusteeEntry;
		LONG	SDTrustees[NumberOfSubdirectoryTrustees];
		WORD	SDTrusteeMask[NumberOfSubdirectoryTrustees];	
		int 	SDMaximumSpace;
		WORD	SDMaximumAccessMask;
		WORD	SDSubdirectoryFirstBlockGone;
		LONG	SDMigrationBindID;
		LONG	SDlongunused;
		LONG	SDExtendedAttributes;
		int		SDLastModifiedInSeconds;	/*	formerly BYTE	SDUndefined1[4]; */
		LONG	SDPrimaryEntry;
		LONG	SDNameList;
};

struct RootDirectoryStructure
{
		int 	RDSubdirectory;
		LONG	RDFileAttributes;
		BYTE	RDUniqueID;
		BYTE	RDFlags;
		BYTE	RDNameSpace;
		BYTE	RDNameSpaceCount;
		LONG	RDDirectoryServicesObjectID;	/* formerly at deletedblockseq slot */
		BYTE	RDSupportedNameSpacesNibble[6];
		BYTE	RDDOSType;
		BYTE	RDVolumeFlags;
		LONG	RDCreateDateAndTime;
		LONG	RDOwnerID;
		LONG	RDLastArchivedDateAndTime;
		LONG	RDLastArchivedID;
		LONG	RDLastModifiedDateAndTime;
		LONG	RDNextTrusteeEntry;
		LONG	RDTrustees[NumberOfSubdirectoryTrustees];
		WORD	RDTrusteeMask[NumberOfSubdirectoryTrustees];	
		int 	RDMaximumSpace;
		WORD	RDMaximumAccessMask;
		WORD	RDSubdirectoryFirstBlock;
		LONG	RDExtendedDirectoryChain0;	/* used only by the root. */
		LONG	RDExtendedDirectoryChain1;	/* used only by the root. */
		LONG	RDExtendedAttributes;
		int		RDModifyTimeInSeconds; /* formerly RDDirectoryServicesObjectID; */
		LONG	RDSubAllocationList;
		LONG	RDNameList;
};

struct	TrusteeStructure
{
		LONG	TFlag;
		LONG	TAttributes;
		BYTE	TUniqueID;
		BYTE	TTrusteeCount;
		BYTE	TFlags;
		BYTE	TUnUsed[1];
		LONG	TSubdirectory;
		LONG	TNextTrusteeEntry;
		LONG	TFileEntryNumber;	/* file trustee entries only. */
		LONG	TTrustees[NumberOfTrusteeTrustees];
		WORD	TTrusteeMask[NumberOfTrusteeTrustees];
		BYTE	TUndefined[4];
		LONG	TDeletedBlockSequenceNumber;
};

struct	UserRestrictionStructure
{
		LONG	RFlag;
		LONG	RZeros;
		BYTE	RUniqueID;
		BYTE	RTrusteeCount;
		BYTE	RUnUsed[2];
		LONG	RSubdirectory;	/* should always be zero. */
		LONG	RTrustees[NumberOfRestrictionTrustees];
		LONG	RRestriction[NumberOfRestrictionTrustees];
};

struct	SubAllocationStructure
{
		LONG	SAFlag;
		LONG	SAZeros;
		BYTE	SAUniqueID;
		BYTE	SASequenceNumber;
		BYTE	SASkip[2];
		LONG	SASubAllocationList;
		LONG	SAStartingFATChain[NumberOfSubAllocFATChains];
};

struct	MACDirectoryStructure
{
		int 	MACSubdirectory;
		LONG	MACFileAttributes;
		BYTE	MACUniqueID;
		BYTE	MACFlags;
		BYTE	MACMyNameSpace;
		BYTE	MACFileNameLength;
		BYTE	MACFileName[32];
		LONG	MACResourceFork;
		LONG	MACResourceForkSize;
		BYTE	MACFinderInfo[32];
		BYTE	MACProDosInfo[6];
		BYTE	MACDirRightsMask[4]; /* Used to emulate AppleShare. */
		BYTE	MACReserved0[2];
		int		MACCreateTime;
		int		MACBackupTime;
		BYTE	MACReserved1[12];
		LONG	MACDeletedBlockSequenceNumber;
		LONG	MACPrimaryEntry;
		LONG	MACNameList;
};

struct UNIXDirectoryStructure
{
		/*
		 * For any name space these first 7 parameters must be
		 * present and in this order.
		 */
		int	UNIXSubdirectory;
		LONG	UNIXFileAttributes;
		BYTE	UNIXUniqueID;
		BYTE	UNIXFlags;
		BYTE	UNIXMyNameSpace;
		BYTE	UNIXFileNameLength;	/* name len in primary entry */
#define PRINAMELEN 40
		BYTE	UNIXFileName[PRINAMELEN];
		/* name space specific fields follow */
		BYTE	UNIXWholeFileNameLength;  /* whole file name length */
		BYTE	UNIXExtantsUsed;
/* The following IPAX ifdefs make this structure size 128 (used to be 123)
   on RISC platforms. */
#ifndef IAPX386
		BYTE	UNIXLinkedFlag;
		BYTE	UNIXFirstCreated;	/* first creation flag */
#endif /* IAPX386 */
		LONG	UNIXStartExtantNumber;
/* to add a new component into the name space, change RESERVEDLEN definition */
#define MODE2ENDLEN (128 - 12 - PRINAMELEN - 6)
#define MODE2RSVLEN (MODE2ENDLEN - 12)
#define RESERVEDLEN (MODE2RSVLEN - 41)
		LONG	UNIXFMode;		/* unix file mode */
		LONG	UNIXMyFlags;		/* for unix name space use */
#define GETTING_EXT_ENTRY		0x0001
#define	GID_NOT_SET			0x0002
#define USING_NFS_GID			0x0004	/* transition code */
#define USING_NFS_FMODE			0x0008	/* for dir created by sms */
#define READONLY_ENTRY			0x0010	/* CDROM directory entries */
#define UNIX_METADATA_VALID		0x0020	/* defect 205587 - Is UNIX Metadata initialized ? */
#define DEFAULT_PERMISSION_SET	0x0040	/* defect 205587 - Is Default Permission Set */
		LONG	UNIXNFSGroupID;		/* file's gid (nfs gid) */
		LONG	UNIXRDev;		/* UNIX rdev for IFCHR/IFBLK */
		LONG	UNIXNumberOfLinks;	/* number of hard links */
#ifdef IAPX386
		BYTE	UNIXLinkedFlag;
#endif /* IAPX386 */
#define	HARDLINKEND	0x0001		/* terminal hard links */
#define SYMBOLICLINK	0x0002		/* symbolic link */
#define HARDLINK	0x0004		/* non-terminal hard links */
#define	TRANSIT_LINK	0x0008		/* for backup/restore purpose */
#ifdef IAPX386
		BYTE	UNIXFirstCreated;	/* first creation flag */
#endif /* IAPX386 */
		/* NOTE: next 3 directory numbers all contain Unique ID */

/* NOTE:  if LinkNextDirNo == My Entry Number than the file is the master file */
/* NOTE:  LinkPrevDirNo is the start of the list of hard links */

		LONG	UNIXLinkNextDirNo;	/* directory # of file linked */
		LONG	UNIXLinkEndDirNo;	/* link end's dir no */
		LONG	UNIXLinkPrevDirNo;	/* back ptr to who link to us */
		LONG	UNIXNFSUserID;		/* NFS uid */
		BYTE	UNIXACSFlags;
#define	LASTCHANGED_UNIX	0x0001		/* acs last changed by UNIX */
#define LASTCHANGED_NW		0x0002		/* acs last chagned by DOS */
		WORD	UNIXLastAccessedTime;	/* approx. last accessed time */
		BYTE	UNIXReserved[RESERVEDLEN];
		/*
		 * These next 3 entries must be defined at the same offsets
		 * in the structure that they are in the DOS and MAC name
		 * spaces.
		 */
		LONG	UNIXDeletedBlockSequenceNumber;
		LONG	UNIXPrimaryEntry;	/* corresponding DOS entry # */
		LONG	UNIXNameList;	/* next entry for the same file */
};

struct OS2DirectoryStructure
{
		int	OS2Subdirectory;
		LONG	OS2FileAttributes; /* The DOS dir attributes are used for OS/2 */
		BYTE	OS2UniqueID;
		BYTE	OS2Flags;
		BYTE	OS2MyNameSpace;
		BYTE	OS2FileNameLength;
		BYTE	OS2FileName[80];

		/*	OS/2 v1.2 & 2.0 have a maximum file length of 255 bytes.
			Any file name longer than 80 bytes will be written to the
			extended directory space */
		  
		LONG	OS2ExtendedSpace;
		BYTE	OS2ExtantsUsed;
		BYTE	OS2LengthData;
		BYTE	OS2Reserved[18];

		/* these next 3 entries must be defined at the same offsets in the
			structure as they are in the DOS and MAC name spaces */

		LONG	OS2reserved;	/* same as DOS */
		LONG	OS2PrimaryEntry;
		LONG	OS2NameList; 
};




union DirUnion
{
	struct	DirectoryStructure	DOSFile;
	struct	SubdirectoryStructure	DOSSubDir;
	struct	RootDirectoryStructure	DOSRoot;
	struct	TrusteeStructure	TNode;
	struct	UserRestrictionStructure UserRestriction;
	struct	SubAllocationStructure	SubAllocation;
	struct	MACDirectoryStructure	MACFile;
	struct	UNIXDirectoryStructure	UNIXFile;
	struct	OS2DirectoryStructure	OS2File;
};



#define MAX_SEGMENTS_PER_VOLUME 32

/*	definitions for the extended directory space.	*/
/* Data follows EDUnunsed */
struct ExtendedDirectoryStructure	{
	LONG	EDSignature;
	LONG	EDLength;
	LONG	EDDirectoryNumber;
	BYTE	EDNameSpace;
	BYTE	EDFlags;
	BYTE	EDControlFlags;
	BYTE	EDUnused;
};

struct UNIXExtendedDirectoryStructure	{
	struct ExtendedDirectoryStructure	UEHead;
#define UESignature		UEHead.EDSignature
#define UELength		UEHead.EDLength
#define UEDirectoryNumber	UEHead.EDDirectoryNumber
#define UENameSpace		UEHead.EDNameSpace
#define UEFlags			UEHead.EDFlags
#define UEControlFlags		UEHead.EDControlFlags
#define UEUnused		UEHead.EDUnused
	BYTE					UENameLength;
	BYTE					UEName[1];
};

#define ExtendedDirectorySignature		0x13579ACE
#define OS2ExtDirSignature 			0xABA12190
#define NTExtDirSignature 				0x5346544e
#define UNIXExtDirSignature			0x554E4958	/* "UNIX" */

/* 1357deaf in memory, Data Migration Signature */
#define MigrationSignature		0xafde5713

/* EA Tally Header */
#define	TALLY_ExtendedDirectorySignature	0x2e4e414a
/*  EA */
#define EA_ExtendedDirectorySignature		0x2e6e616a


/*	ControlBeingModifiedBit is set when the complete extant write goes
	over several sectors and is cleared once everything is written.	*/

#define ControlBeingModifiedBit		0x1

/*	These bits are used to control the writing of extants to disk.	*/

#define	FlushWriteCompleteExtantBit		0x1
#define FlushWriteOnlyForPartialExtantBit	0x2

/****************************************************************************/
/****************************************************************************/


#endif /* __DSTRUCT_H__ */
