#ifndef __BITS_H__
#define __BITS_H__
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
 *  $Workfile:   bits.h  $
 *  $Modtime:   Aug 10 2001 09:41:06  $
 *  $Revision$
 *  
 ****************************************************************************/

/* definition of access privledge bits for a given directory area */

#define	ReadExistingFileBit				0x0001
#define	WriteExistingFileBit			0x0002
#define OldOpenExistingFileBit			0x0004
#define	CreateNewEntryBit				0x0008
#define	DeleteExistingEntryBit			0x0010
#define	ChangeAccessControlBit			0x0020
#define	SeeFilesBit						0x0040
#define	ModifyEntryBit					0x0080
#define SupervisorPrivilegesBit			0x0100	/* also defined in Dircache.386 */

/* used by create dir. or create file (both are OS Internal only)  */
#define CreateHardLinkEntryBit			0x10000000
#define HasFileWritePrivilegeBit		0x2000	/* pass into AddFile ONLY */

#define DefinedAccessRightsBits			0x01FB	/* all the bits currently used. */
#define MaximumDirectoryAccessBits		0x01FF	/* all the defined bits for access privileges */
#define	AllValidAccessBits			0x100001FF  /* all the bits that are valid in CreateDirectory */

/* used by create directory only (DesigedAccessRights) Jim A. Nicolet*/
#define	ReqSystemBit				0x00100000
#define	ReqHiddenBit				0x00200000
#define	ReqDeleteInhibitBit			0x00400000
#define	ReqRenameInhibitBit			0x00800000
#define	ReqPurgeImmediateBit		0x01000000
#define	ReqImmediateCompressBit		0x02000000
#define	ReqDontCompressBit			0x04000000

#define	ReqBitsMask1 (ReqSystemBit | ReqHiddenBit | ReqDeleteInhibitBit | ReqRenameInhibitBit | ReqPurgeImmediateBit)
#define ReqBitsMask	(ReqBitsMask1 | ReqImmediateCompressBit | ReqDontCompressBit)

/* Secure Directory Access Mask (_NETWARE) */
#define	SecureDirectoryAccessMask	(ReqSystemBit | ReqHiddenBit | ReqPurgeImmediateBit)

/* definition of file attribute bits */

#define READ_ONLY_BIT					0x00000001
#define	HIDDEN_BIT						0x00000002
#define	SYSTEM_BIT						0x00000004
#define	EXECUTE_BIT						0x00000008
#define	SUBDIRECTORY_BIT				0x00000010
#define	ARCHIVE_BIT						0x00000020
/*		EXECUTE_CONFIRM_BIT				0x00000040 */
#define	SHAREABLE_BIT					0x00000080	/* Valid only on files       */
#define OLD_PRIVATE_BIT					0x00000080	/* Valid only on directories */
/*		LOW_SEARCH_BIT					0x00000100 */
/*		MID_SEARCH_BIT					0x00000200 */
/*		HI_SEARCH_BIT 					0x00000400 */
#define	NO_SUBALLOC_BIT			0x00000800
#define	SMODE_BITS						0x00000700	/* search bits */
#define	TRANSACTION_BIT		  		0x00001000
/*      OLD_INDEXED_BIT					0x00002000 */
#define	READ_AUDIT_BIT					0x00004000
#define	WRITE_AUDIT_BIT		  		0x00008000
#define IMMEDIATE_PURGE_BIT	  		0x00010000
#define RENAME_INHIBIT_BIT				0x00020000
#define DELETE_INHIBIT_BIT				0x00040000
#define COPY_INHIBIT_BIT				0x00080000
#define FILE_AUDITING_BIT				0x00100000	/* system auditing */
/* 	  								0x00200000	#* Currently not in use. #/ */
#define REMOTE_DATA_ACCESS_BIT		0x00400000	/* ie. Data Migration (file only) */
#define REMOTE_DATA_INHIBIT_BIT		0x00800000	/* ie. Data Migration (file only)*/
#define REMOTE_DATA_SAVE_KEY_BIT		0x01000000	/* ie. Data Migration (file only)*/
#define COMPRESS_FILE_IMMEDIATELY_BIT 0x02000000  /* immediately try to compress this file (or all files within this subdirectory) */
#define DATA_STREAM_IS_COMPRESSED_BIT		0x04000000	/* per data stream directory entry */
#define DO_NOT_COMPRESS_FILE_BIT		0x08000000	/* don't compress this file ever (or default files within this subdirectory) */
#define CANT_COMPRESS_DATA_STREAM_BIT 0x20000000 /* can't save any space by compressiong this data stream */
#define ATTR_ARCHIVE_BIT				0x40000000	/* Object Archive Bit  (EAs, OwnerID, Trustees */
#define ZFS_VOLATILE_BIT				0x80000000	// USED BY NSS (Jim A. Nicolet 11-6-2000)


/* Data Migration Sub Directory Bit Definitions */
#define REMOTE_SUPPORT_MODULE_BOUND_BIT	(REMOTE_DATA_ACCESS_BIT)
#define REMOTE_SUPPORT_MODULE_ROOT_BIT		(REMOTE_DATA_SAVE_KEY_BIT)

/* conflicts with CreateHardLinkEntryBit -- Jim A. Nicolet 4/6/92 */
/*****#define COMPRESS_FILE_IMMEDIATELY_BIT 0x10000000 immediately try to compress this file (or all files within this subdirectory) */

/* The AFP SPG assumes these are the positions for these bits */
#define MAC_DATA_FORK_OPEN				0x0100
#define MAC_RES_FORK_OPEN				0x0200
#define MAC_COPY_INHIBIT_BIT			0x0800
#define MAC_RENAME_INHIBIT_BIT			0x4000
#define MAC_DELETE_INHIBIT_BIT			0x8000

/* Attribute bits that can be set and returned by 286 calls */
#define OLD_MAC_ATTRIBUTE_MASK			0x000017FF
#define OLD_DOS_ATTRIBUTE_MASK			0x000017FF

/* Attribute bits that can be set and returned by 386 calls */
#define VALID_ATTRIBUTE_MASK			0x4ADF1FFF
#define INHERITED_DATA_STREAM_MASK	~DATA_STREAM_IS_COMPRESSED_BIT

/*	define the DFlags bits	*/

#define OldDeletedFileBit				0x01
#define PhantomEntryBit					0x02
#define SubdirectoryEntryBit			0x04
#define ExplicitlyNamedBit				0x08
#define PrimaryNameBit					0x10
#define NewDeletedFileBit				0x20
#define HardLinkedEntryBit				0x40

/* define the MoreFlags */
#define	DeleteFileOnCloseBit			0x00000001
#define	NoUpdateAccessBit				0x00000002
#define EventReportOnCloseBit			0x00000004

/*	define the NameSpace values	*/

#define DOSNameSpace					0
#define MACNameSpace					1

/*	define the data stream values	*/

#define PrimaryDataStream				0
#define MACResourceForkDataStream		1
#define FTAMDataStream					2

/*	note that some of these bits are defined in DIRCACHE.386, dstruct.inc., fileio.386	*/

#define	DELETE_FILE_ON_CREATE_BIT		0x0001
#define	NO_RIGHTS_CHECK_ON_CREATE_BIT	0x0002

/*	bits used with Open File (besides the compatability bits defined in LOCKS.H).	*/

#define READ_ACCESS_BIT					0x00000001
#define WRITE_ACCESS_BIT				0x00000002
#define DENY_READ_BIT					0x00000004
#define DENY_WRITE_BIT					0x00000008
#define COMPATABILITY_MODE_BIT		0x00000010
#define FILE_WRITE_THROUGH_BIT		0x00000040
#define FILE_READ_THROUGH_BIT			0x00000080

#define ENABLE_IO_ON_COMPRESSED_DATA_BIT	0x00000100
#define LEAVE_FILE_COMPRESSED_BIT	0x00000200
#define DELETE_FILE_ON_CLOSE_BIT		0x00000400
/*#define NO_UPDATE_LAST_ACCESSED_ON_CLOSE_BIT		0x00000800*/ /* v5.x definition*/
#define NO_UPDATE_LAST_ACCESSED_ON_CLOSE_BIT		0x80000000	 /* Matches NSS definition in 6 pack*/

#if ReadAhead
#define ALWAYS_READ_AHEAD_BIT			0x00001000	/* these two read ahead bits are mutually exclusive */
#define NEVER_READ_AHEAD_BIT			0x00002000	/* if neither is set then NORMAL read ahead is used */
#endif

#define NO_RIGHTS_CHECK_ON_OPEN_BIT		0x00010000

#define ALLOW_SECURE_DIRECTORY_ACCESS_BIT	0x00020000


#define OPEN_FILE_READ_WRITE_BITS		(READ_ACCESS_BIT | WRITE_ACCESS_BIT | DENY_READ_BIT | DENY_WRITE_BIT | COMPATABILITY_MODE_BIT)
#define OPEN_FILE_EXCLUSIVE_BITS		(READ_ACCESS_BIT | WRITE_ACCESS_BIT | DENY_READ_BIT | DENY_WRITE_BIT)
#define OPEN_FILE_COMPATABILITY_BITS	(READ_ACCESS_BIT | WRITE_ACCESS_BIT | COMPATABILITY_MODE_BIT)
#define OPEN_FILE_PASS_THRU	(FILE_WRITE_THROUGH_BIT | FILE_READ_THROUGH_BIT)
#if ReadAhead
#define OPEN_FILE_READ_AHEAD_BITS		(NEVER_READ_AHEAD_BIT | ALWAYS_READ_AHEAD_BIT)
#endif

/*
#define ALL_RIGHTS_BITS_MASK			
(READ_ACCESS_BIT | WRITE_ACCESS_BIT | DENY_READ_BIT | DENY_WRITE_BIT | COMPATABILITY_MODE_BIT | FILE_WRITE_THROUGH_BIT)
*/
#define ALL_RIGHTS_BITS_MASK (OPEN_FILE_READ_WRITE_BITS | FILE_WRITE_THROUGH_BIT)

/* Volume Segment Table volume flags */

#define SYSTEM_VOLUME		0x01
#define CLUSTERED_VOLUME	0x02
#define TTS_BACKOUT_VOLUME	0x04
#define	READ_ONLY_VOLUME 	0x80

/* define the RDVolumeFlag bits */

#define VOLUME_AUDITING_BIT	  			0x01	/* system auditing */
#define SUB_ALLOCATION_ENABLED_BIT		0x02	/* sub allocation units valid on this volume */
#define FILE_COMPRESSION_ENABLED_BIT	0x04	/* file compression enabled on this volume */
#define DATA_MIGRATION_ENABLED_BIT		0x08	/* data migration is allowed on this volume */
#define NEW_TRUSTEE_COUNT_BIT				0x10	/* 3.2 Volumes have only 4 trustee entries per volume
																instead of 6 */
#define DIR_SVCS_OBJ_UPGRADED_BIT		0x20	/* Modify 3.2 volume DirObjId to
																new position */

#define VOLUME_IMMEDIATE_PURGE_ENABLED_BIT	0x40	/* Volume is marked as immediate purge */

#if ReadAhead
/* define the ReadAheadMode values */
/* note that these values MUST correspond to OPEN_FILE_READ_AHEADBITS >> 12 */
#define RA_NORMAL	0
#define RA_EXEC		1
#define RA_NONE		2
#endif

/****************************************************************************/
/****************************************************************************/


#endif /* __BITS_H__ */
