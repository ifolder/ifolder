/****************************************************************************
 |
 |	(C) Copyright 1995-1999 Novell, Inc.
 |	All Rights Reserved.
 |
 |	This program is an unpublished copyrighted work which is proprietary
 |	to Novell, Inc. and contains confidential information that is not
 |	to be reproduced or disclosed to any other person or entity without
 |	prior written consent from Novell, Inc. in each and every instance.
 |
 |	WARNING:  Unauthorized reproduction of this program as well as
 |	unauthorized preparation of derivative works based upon the
 |	program or distribution of copies by sale, rental, lease or
 |	lending are violations of federal copyright laws and state trade
 |	secret laws, punishable by civil and criminal penalties.
 |
 |***************************************************************************
 |
 |	 Novell Storage Services (NSS) module
 |
 |---------------------------------------------------------------------------
 |
 | $Author$
 | $Modtime:   23 Feb 2001 16:08:32  $
 |
 | $Workfile:   zParams.h  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Define bit masks and other values used in the NSS file system APIs
 +-------------------------------------------------------------------------*/
#ifndef _ZPARAMS_H_
#define _ZPARAMS_H_

#ifndef _ZOMNI_H_
#	include <zOmni.h>
#endif

#ifdef __cplusplus
extern "C" {
#endif

/*=========================================================================
 *=========================================================================
 *	Defines for bit masks and values
 *=========================================================================
 *=========================================================================*/

/*-----------------------------------------------------------------------
 *	Definition of attributes for supportedAttributes and enabledAttributes
 *	fields in the volume and LSS info structures
 *-----------------------------------------------------------------------*/
#define zATTR_SALVAGE					0x00000001
#define zATTR_USER_SPACE_RESTRICTIONS	0x00000002
#define zATTR_READONLY					0x00000004
#define zATTR_COMPRESSION 				0x00000008
#define zATTR_EXTENDED_ATTRIBUTES		0x00000010
#define zATTR_DATA_STREAMS				0x00000020
#define zATTR_DOS_METADATA				0x00000040
#define zATTR_NETWARE_METADATA			0x00000080
#define zATTR_MAC_METADATA				0x00000100
#define zATTR_UNIX_METADATA				0x00000200
#define zATTR_HARD_LINKS				0x00000400
#define zATTR_TRANSACTION				0x00000800
#define zATTR_SPARSE_FILES				0x00001000
#define zATTR_PHYSICAL_EOF				0x00002000
#define zATTR_DIRECT_IO					0x00004000
#define zATTR_PERSISTENT_ATTRIBUTES		0x00008000
#define zATTR_VERIFY					0x00010000
#define zATTR_REBUILD					0x00020000
#define zATTR_COW						0x00040000
#define zATTR_VIRTUAL_FILES				0x00080000
#define zATTR_USER_TRANSACTION			0x00100000	/* indicates the volume 
													 * can be transactioned on 
													 * next activate */
#define zATTR_USER_TRANSACTION_ACTIVE	0x00200000	/* indicates the volume is 
													 * currently being 
													 * transactioned */
#define zATTR_DONT_BACKUP               0x00400000  /* Don't backup this 
													 * volume */
#define zATTR_MFL                       0x00800000  /* Volume maintains MFL 
													 * data structure */
#define zATTR_DIR_QUOTAS				0x01000000
#define zATTR_SHRED_DATA				0x02000000
#define zATTR_SHARED					0x04000000	/* Pool/Volume is part
													 * of a cluster.  I.E.
													 * multiple servers have
													 * access to the pool/vol.
													 */
#define zATTR_HIGH_INTEGRITY			0x08000000
#define zATTR_MIGRATION					0x10000000


/*-----------------------------------------------------------------------
 *	Definition of features(attributes) for supportedFeatures and
 *	enabledFeatures fields in the pool.
 *-----------------------------------------------------------------------*/

#define zPOOL_FEATURE_PERSISTENT_FEATURES		0x00000001uL
			/* zPOOL_FEATURE_PERSISTENT_FEATURES indicates that the pool's
			 * features are stored persistently.
			 */
#define	zPOOL_FEATURE_SHARED_CLUSTER			0x00000002uL
			/* zPOOL_FEATURE_SHARED_CLUSTER indicates that the pool is
			 * part of cluster and can therefore be accessed by more
			 * than one server (although only ONE server at a time).
			 */
#define	zPOOL_FEATURE_READ_ONLY					0x00000004uL
			/* zPOOL_FEATURE_READ_ONLY indicates that the pool is
			 * read only.  This means ABSOLUTELY no writes can be
			 * attempted on the pool.  Cluster software may allow
			 * multiple servers to active the POOL for load balance.
			 */
#define	zPOOL_FEATURE_VERIFY					0x00000008uL
			/* zPOOL_FEATURE_VERIFY indicates that the pool supports
			 * a verify operation.
			 */
#define	zPOOL_FEATURE_REBUILD					0x00000010uL
			/* zPOOL_FEATURE_REBUILD indicates that the pool supports
			 * a rebuild operation.
			 */
#define	zPOOL_FEATURE_MULTIPLE_VOLUMES			0x00000020uL
			/* zPOOL_FEATURE_MULTIPLE_VOLUMES indicates that the pool can
			 * support more than a ONE-TO-ONE relationship between the
			 * POOL and a volume.
			 */
#define	zPOOL_FEATURE_SNAPSHOT					0x00000040uL
			/* zPOOL_FEATURE_SNAPSHOT indicates that the pool is a
			 * snapshot of another pool.  This bit is set by ZLSS
			 * when the nssidk snapshot API is called.
			 */

		/* zPOOL_FEATURE_LIST is just a define of all the LEGAL feature
		 * bits.  Used to indicate locations that must change when a
		 * bit is added and for legal bit verification.
		 */
#define	zPOOL_FEATURE_LIST													\
		  ( zPOOL_FEATURE_PERSISTENT_FEATURES		|						\
			zPOOL_FEATURE_SHARED_CLUSTER			| 						\
			zPOOL_FEATURE_READ_ONLY					|						\
			zPOOL_FEATURE_VERIFY			 		|  						\
			zPOOL_FEATURE_REBUILD			 		|  						\
			zPOOL_FEATURE_MULTIPLE_VOLUMES			|						\
			zPOOL_FEATURE_SNAPSHOT )

/*---------------------------------------------------------------------------
 *	Definitions for authSystemID (authorizaton System ID)
 *---------------------------------------------------------------------------*/
#define zAUTH_SYSTEM_UNDEFINED		0
#define zAUTH_SYSTEM_NSS_DEFAULT	1


/*---------------------------------------------------------------------------
 *	Defintions of connectionID
 *---------------------------------------------------------------------------*/
#define zSYS_CONNECTION				0

#ifdef _NSS_INTERNAL_
	#define INVALID_CONNECTION_ID (-1)
#endif


/*---------------------------------------------------------------------------
 *	Definitions for contextHandleType
 *---------------------------------------------------------------------------*/
#define zCX_CONNECTION_BASED		0
#define zCX_TASK_BASED				1

#ifdef _NSS_INTERNAL_
	#define zUNDEFINED_CXHANDLE		0
	#define zINVALID_CXHANDLE		(-1)

	#define zCX_NONMAPPABLE			2		/* Legacy compatability only */
	#define zCX_ALTERNATEREPLY		0x4000	/* Legacy NCP 87.12 only */
#endif


/*---------------------------------------------------------------------------
 *	Definitions for copyOptions
 *---------------------------------------------------------------------------*/

/* TBD */


/*-------------------------------------------------------------------------
 *	Definition of createFlags
 *-------------------------------------------------------------------------*/
#define zCREATE_OPEN_IF_THERE		0x00000001
#define zCREATE_TRUNCATE_IF_THERE	0x00000002
#define zCREATE_DELETE_IF_THERE		0x00000004

#ifdef _NSS_INTERNAL_
	#define zVALID_CREATE_FLAGS \
			(zCREATE_OPEN_IF_THERE | zCREATE_TRUNCATE_IF_THERE | \
			 zCREATE_DELETE_IF_THERE)
#endif


/*---------------------------------------------------------------------------
 *	Definitions for createParms
 *---------------------------------------------------------------------------*/

/* TBD */


/*---------------------------------------------------------------------------
 *	Definitions of deleteFlags
 *---------------------------------------------------------------------------*/
#define zDELETE_PURGE_IMMEDIATE		0x00000001
#define zDELETE_FORCE_DELETE		0x00000002

/*-------------------------------------------------------------------------
 *	Definition of file attributes
 *-------------------------------------------------------------------------*/
#define zFA_READ_ONLY		 		0x00000001
#define	zFA_HIDDEN 					0x00000002
#define	zFA_SYSTEM 					0x00000004
#define zFA_EXECUTE					0x00000008
#define zFA_SUBDIRECTORY	 		0x00000010
#define	zFA_ARCHIVE					0x00000020
#define	zFA_SHAREABLE		 		0x00000080
#define	zFA_SMODE_BITS		 		0x00000700
#define zFA_NO_SUBALLOC				0x00000800
#define zFA_TRANSACTION				0x00001000
#define zFA_NOT_VIRTUAL_FILE		0x00002000	/* only valid on a volume with the zATTR_VIRTUAL_FILES attribute */
#define zFA_IMMEDIATE_PURGE			0x00010000
#define zFA_RENAME_INHIBIT	 		0x00020000
#define zFA_DELETE_INHIBIT	 		0x00040000
#define zFA_COPY_INHIBIT	 		0x00080000
#define zFA_IS_ADMIN_LINK			0x00100000	/* if set then the file contains persistent admin link info */
#define zFA_IS_LINK					0x00200000
#define zFA_REMOTE_DATA_INHIBIT		0x00800000
#define zFA_COMPRESS_FILE_IMMEDIATELY 0x02000000
#define zFA_DATA_STREAM_IS_COMPRESSED 0x04000000 /* per data stream directory entry */
#define zFA_DO_NOT_COMPRESS_FILE	  0x08000000
#define zFA_CANT_COMPRESS_DATA_STREAM 0x20000000 /* can't save any space by compressiong this data stream */
#define zFA_ATTR_ARCHIVE	 		0x40000000
#define zFA_VOLATILE				0x80000000	/* Data is volatile (no oplocks) */

#ifdef _NSS_INTERNAL_
	#define zFA_VALID_FILE_ATTRIBUTES \
			(zFA_READ_ONLY | zFA_HIDDEN | zFA_SYSTEM | zFA_EXECUTE | \
			 zFA_ARCHIVE | zFA_SHAREABLE | zFA_SMODE_BITS | zFA_NO_SUBALLOC | \
			 zFA_TRANSACTION | zFA_IMMEDIATE_PURGE | zFA_RENAME_INHIBIT | \
			 zFA_DELETE_INHIBIT | zFA_COPY_INHIBIT | zFA_IS_LINK | \
			 zFA_REMOTE_DATA_INHIBIT | zFA_COMPRESS_FILE_IMMEDIATELY | \
			 zFA_DO_NOT_COMPRESS_FILE | zFA_ATTR_ARCHIVE | \
			 zFA_NOT_VIRTUAL_FILE | zFA_VOLATILE)

	#define zFA_VALID_DIRECTORY_ATTRIBUTES	\
			(zFA_VALID_FILE_ATTRIBUTES | zFA_SUBDIRECTORY)

	/* For now, don't allow any data streams to have attributes.  NOTE--
	 * there is code in COMN_Create that assumes zFA_VALID_DATA_STREAM_ATTIBUTES
	 * is always a subset of zFA_VALID_FILE_ATTRIBUTES */
	#define zFA_VALID_DATA_STREAM_ATTRIBUTES (0)
#endif

/*-------------------------------------------------------------------------
 *	Bit definitions for the trustee rights and inherited rights mask
 *-------------------------------------------------------------------------*/

#define	zAUTHORIZE_READ_CONTENTS			0x0001
#define	zAUTHORIZE_WRITE_CONTENTS			0x0002
#define	zAUTHORIZE_CREATE_ENTRY				0x0008
#define	zAUTHORIZE_DELETE_ENTRY				0x0010
#define	zAUTHORIZE_ACCESS_CONTROL			0x0020
#define	zAUTHORIZE_SEE_FILES				0x0040
#define	zAUTHORIZE_MODIFY_METADATA			0x0080
#define zAUTHORIZE_SUPERVISOR				0x0100
#define zAUTHORIZE_SALVAGE					0x0200
#define zAUTHORIZE_SECURE					0x8000 /* This special right is not inherited */

#define zVALID_TRUSTEE_RIGHTS \
			(zAUTHORIZE_READ_CONTENTS | zAUTHORIZE_WRITE_CONTENTS | \
			 zAUTHORIZE_CREATE_ENTRY | zAUTHORIZE_DELETE_ENTRY | \
			 zAUTHORIZE_ACCESS_CONTROL | zAUTHORIZE_SEE_FILES | \
			 zAUTHORIZE_MODIFY_METADATA | zAUTHORIZE_SUPERVISOR)


/*---------------------------------------------------------------------------
 *	Defines for fileHandleID
 *---------------------------------------------------------------------------*/
#define zUNDEFINED_FILEHANDLE	(-1)
#define zINVALID_FILEHANDLE		(0)


/*---------------------------------------------------------------------------
 *	Definition of extentListformat
 *---------------------------------------------------------------------------*/
enum FileMapFormats_t {
	zFILEMAP_ALLOCATION,
	zFILEMAP_LOGICAL,
	zFILEMAP_PHYSICAL
};

/*---------------------------------------------------------------------------
 *	This value is returned as the unit number in a zFILEMAP_PHYSICAL filemap
 *	to represent a sparse portion of the file.
 *---------------------------------------------------------------------------*/
#define zFILEMAP_SPARSEBLOCK	-1

/*---------------------------------------------------------------------------
 *	These structures are returned for file maps
 *---------------------------------------------------------------------------*/
/* For zFILEMAP_ALLOCATION, the following is returned */
typedef struct zAllocationExtentElement_s
{
	QUAD offset;
	QUAD length;
} zAllocationExtentElement_s;

/* For zFILEMAP_LOGICAL, the following is returned */
typedef struct zLogicalExtentElement_s
{
	QUAD blockNumber;
	QUAD numBlocks;
}zLogicalExtentElement_s;

/* For zFILEMAP_PHYSICAL, the following is returned */
typedef struct zPhysicalExtent_s
{
	QUAD	length;			/* Extent length in bytes		*/
	QUAD	logicalOffset;	/* Byte offset in the file		*/
	QUAD	poolOffset;		/* Byte offset in the pool		*/
	struct
	{
		QUAD	offset;		/* Byte offset on the device				*/
		LONG	deviceID;	/* Device ID understood by Media Manager	*/
		LONG	padding;
	} physical;
} zPhysicalExtent_s;


/*---------------------------------------------------------------------------
 *	Definition of fileNameUniquifier
 *
 *	A file may have multiple parents, and multiple names within each parent.
 *	This is an ID which uniquely identifies which parent/name is being 
 *	referred to for a given file.
 *
 *	A well known value of zFNU_FIRST_PARENT will always map to the first
 *	name for the first parent container.  All other parents/names must be
 *	identified by a value retrieved from another API or event.
 *---------------------------------------------------------------------------*/
#define zFNU_UNDEFINED			0
#define zFNU_FIRST_PARENT		zFNU_UNDEFINED

#ifdef _NSS_INTERNAL_
	#define zFNU_INVALID_NAME_UNIQUIFIER	0xFFFF 
#endif

/*
 * File Types -- used to create files, directories and other
 * types of beasts.  It is upto the LSS to select the correct
 * beast type that corresponds to the given file type.
 * ONLY add new types to the bottom of the list before zFILE_MAX.
 */
typedef enum FileType_t {
	zFILE_UNKNOWN,				/* Unknown file type				*/
	zFILE_REGULAR,				/* A file or directory				*/
	zFILE_EXTENDED_ATTRIBUTE,	/* An extended attribute			*/
	zFILE_NAMED_DATA_STREAM,	/* Resource forks for Macintosh		*/
	zFILE_PIPE,					/* Acts like a UNIX named pipe		*/
	zFILE_VOLUME,				/* Logical storage					*/
	zFILE_POOL,					/* Physical storage					*/
	zFILE_MAX					/* End of the range of file types	*/
} FileType_t;

/*-------------------------------------------------------------------------
 *	Definition of Beast types
 *
 * These must be a unique value for every Beast Type.  These values are
 * assigned by Novell.  To request a value send E-Mail to NSS@Novell.Com and
 * ask for a BEAST TYPE for each beast that you are defining.
 *-------------------------------------------------------------------------*/
#define zFTYPE_INVALID				-1

#define zFTYPE_ROOT_BEAST			0
#define zFTYPE_NAMED_DATA_STREAM	1
#define zFTYPE_AUTH_BEAST			2
#define zFTYPE_FILE					3 
#define zFTYPE_ADMIN_VOL_FILE		4
#define zFTYPE_VOLUME				5
#define zFTYPE_ADMIN_VOL			6
#define zFTYPE_NAMESPACE			7
#define	zFTYPE_POOL					8
#define zFTYPE_PIPE					9
#define zFTYPE_BEAST_CLASS			10	/* this must be the last primitive beast*/

#define zFTYPE_EXTENDED_ATTRIBUTE	12

#define zFTYPE_ZAS_ACL_OVERFLOW		18
#define zFTYPE_ZAS_VIS_OVERFLOW		19

#define zFTYPE_AUTH_MODEL			20
#define zFTYPE_ZAS_AUTH_MODEL		21

#define zFTYPE_AUTH_SPACE			30
#define zFTYPE_ZAS_AUTH_SPACE		31

	/* The next 15 IDs are assigned to Novell, Inc.  Contact NSS@Novell.com
	 * They are used in Novell's ZLSS
	 */
#define zFTYPE_ZLSS_ZFSPOOL			40
#define zFTYPE_ZLSS_VOL				41
#define zFTYPE_ZLSS_BTREE			42
#define zFTYPE_ZLSS_VOLUME_DATA		43
#define zFTYPE_ZLSS_BEAST_TREE		44
#define zFTYPE_ZLSS_NAME_TREE		45
#define	zFTYPE_ZLSS_LOG				46
#define zFTYPE_ZLSS_SALVAGE			47
#define	zFTYPE_ZLSS_PURGE_LOG		48
#define zFTYPE_ZLSS_FREE_EXTENT		49
#define zFTYPE_ZLSS_USER_TREE		50
#define zFTYPE_ZLSS_VOLUME_LOCATOR	51
#define	zFTYPE_ZLSS_LOGICAL_POOL	52
#define	zFTYPE_ZLSS_MFL            	53
#define zFTYPE_ZLSS_DIR_TREE		54

	/* The next two IDs are assigned to Novell, Inc.  Contact NSS@Novell.com
	 * They were used in Novell's ???
	 */
#define zFTYPE_CDROM_POOL			70
#define zFTYPE_CDROM_VOLUME			71

	/* The next two IDs are assigned to Novell, Inc.  Contact NSS@Novell.com
	 * They are used in Novell's ISO 9660 CD LSS
	 */
#define zFTYPE_ISO9660_VOLUME 		72		/* DOS LSS Volume ID */
//#define zFTYPE_ISO9660_FILE			73		/* DOS LSS File ID */

	/* The next two IDs are assigned to Novell, Inc.  Contact NSS@Novell.com
	 * They are used in Novell's DOS LSS
	 */
#define zFTYPE_FAT_VOLUME			88		/* DOS LSS Volume ID */

	/* The next ID is assigned to Novell, Inc.  Contact NSS@Novell.com
	 * They are used in Novell's HFS (MAC) CD LSS
	 */
#define zFTYPE_HFSCD_VOLUME			91

	/* The next ID is assigned to Novell, Inc.  Contact NSS@Novell.com
	 * They were used in Novell's UDF (DVD) LSS
	 */
#define zFTYPE_UDFCD_VOLUME			93

	/* The next ID is assigned to Novell, Inc.  Contact NSS@Novell.com
	 * They were used in Novell's 32 Bit DOS LSS
	 */
#define zFTYPE_FAT32_VOLUME			95

	/* The next IDs are assigned to IBM.  Contact NSS@Novell.com
	 * They are used in IBM's NetWare Gateway LSS.
	 */
#define zFTYPE_NWSHRFLD_VOLUME		96
#define zFTYPE_NWSHRFLD_FILE		97


	/* The next ID is assigned to Novell, Inc.  They are used in Novell's
	 * SMS Tape LSS.  Given to Sudhir Subbarao <sksubbarao@novell.com> on
	 * Nov 17, 1998 by Greg Pachner <GPachner@Novell.com>
	 */
#define zFTYPE_SMSTAPE_VOLUME		98

	/* The next ID is assigned to Novell, Inc.  They are used in Novell's
	 * NFS Gateway LSS.  Given to Giridhar V <vgiridhar@novell.com> on
	 * Mar 31, 1999 by Paul Taysom <paul_taysom@Novell.com>
	 */
#define zFTYPE_NFSGATEWAY_VOLUME	99

	/* current maximum supported value*/
#define zFTYPE_MAX				   255		

/*-------------------------------------------------------------------------
 *	Definition of LSS ID Types
 *
 * These must be a unique value for every LSS.  These values are assigned
 * by Novell.  To request a value send E-Mail to NSS@Novell.Com and
 * ask for a LSS ID for each LSS that you are writing.
 *
 *-------------------------------------------------------------------------*/

#define zLSS_ID_INVALID			-1

#define	zLSS_ID_ADMIN			 10	/* Owned by Novell */
#define	zLSS_ID_ZLSS   			 20	/* Owned by Novell */
#define	zLSS_ID_FAT				 30	/* Owned by Novell */
#define	zLSS_ID_ISO9660			 40	/* Owned by Novell */
#define	zLSS_ID_HFSCD			 50	/* Owned by Novell */
#define zLSS_ID_UDFCD			 60	/* Owned by Novell */
#define zLSS_ID_FAT32			 70	/* Owned by Novell */
#define zLSS_ID_SMSTAPE			 90 /* Owned by Novell (SMS Team).
									 * Given out by Greg Pachner on Nov 18,
									 * 1998 to Sudhir Subbarao
									 * <sksubbarao@novell.com>
									 */

#define	zLSS_ID_NWSHRFLD		100 /* Owned by IBM (NWSAA Development Group)
									 * for Netware for SAA.  Given out by
									 * Greg Pachner on May 5, 1998.
									 */

#define zLSS_ID_NFSGATEWAY		110	/* Owned by Novell (NFS Gateway)
									 * Given out by Paul Taysom on Mar 31,
									 * 1999 to Giridhar V <vgiridhar@novell.com>
									 */

/*---------------------------------------------------------------------------
 *	Definition of getNameMask
 *---------------------------------------------------------------------------*/
#define zGFN_INCLUDE_PATH			0x00000001
#define zGFN_INCLUDE_VOLUME			0x00000002

#ifdef _NSS_INTERNAL_
	#define zVALID_GFN_MASK \
			(zGFN_INCLUDE_PATH | zGFN_INCLUDE_VOLUME)
#endif


/*-------------------------------------------------------------------------
 *	Definition of getInfoMask
 *-------------------------------------------------------------------------*/

enum zGetInfoMask_t
{
		/*
		 * Get the standard information about the file.
		 */
	zGET_STD_INFO			= 0x1,
		/*
		 * Get the name of the file object for the name space and hard link
		 * used to locate the file object.
		 */
	zGET_NAME				= 0x2,
		/*
		 * Get the name of the file object for each name space in which a
		 * valid name exists. Also, get the NameSpaceID of the name which
		 * is considered the original name for the file object. For a file
		 * object with multiple hard links, this only gets the names associated
		 * with the link used to find the file object.
		 */
	zGET_ALL_NAMES			= 0x4,
		/*
		 * Get the nameSpaceID of the primary name space for this file
		 * object's name.
		 */
	zGET_PRIMARY_NAMESPACE	= 0x8,
		/*
 		 * Get the various time stamps associated with the file object
		 * in seconds.  Though time is returned in seconds, the underlying
		 * system may only keep a resolution of 2 seconds.
		 */
	zGET_TIMES_IN_SECS		= 0x10,
		/*
		 * Get the various time stamps associated with the file object in
		 * micro-seconds.  Though time is returned in micro-seconds, the
		 * underlying system may only keep a resolution of 1 or 2 seconds.
		 * Note:  Only zGET_TIMES_IN_SECS or zGET_TIMES_IN_MICROS but not
		 * both, may be set.
		 */
	zGET_TIMES_IN_MICROS	= 0x20,
		/*
		 * Get the various NDS object IDs associated with this file object.
		 */
	zGET_IDS				= 0x40,
		/*
		 * Get information for this file object about the physical disk
		 * storage that is used.
		 */
	zGET_STORAGE_USED		= 0x80,
		/*
		 * Get information about the file object's block size.
		 */
	zGET_BLOCK_SIZE			= 0x100,
		/*
		 * Get the file open count and hard link count information
		 * about the file object.
		 */
	zGET_COUNTS				= 0x200,
		/*
		 * Get summary information about extended attributes owned by
		 * this file object.
		 */
	zGET_EXTENDED_ATTRIBUTE_INFO	= 0x400,
		/*
		 * Get summary information about data streams owned by this
		 * file object.
		 */
	zGET_DATA_STREAM_INFO	= 0x800,
		/*
		 * Get deleted file information about the file.  This includes
		 * the time in micro-seconds the file was deleted and the user
		 * ID of the user that deleted the file.
		 */
	zGET_DELETED_INFO		= 0x1000,
		/*
		 * Get the file object's macintosh metadata information.
		 */
	zGET_MAC_METADATA		= 0x2000,
		/*
		 * Get the file object's Unix metadata information.
		 */
	zGET_UNIX_METADATA		= 0x4000,
		/*
		 * If the file object is an extended attribute, this bit returns
		 * the extAttrUserFlags.
		 */
	zGET_EXTATTR_FLAGS		= 0x8000,
		/*
		 * If the file object is a volume, this bit returns the basic
		 * information about the volume.
		 */
	zGET_VOLUME_INFO		= 0x10000,
		/*
		 * If the file object is a volume, this bit returns the salvage
		 * information for the volume.
		 */
	zGET_VOL_SALVAGE_INFO	= 0x20000,
		/*
		 * If the file object is a pool, this bit returns the pool
		 * information for the volume.
		 */
	zGET_POOL_INFO			= 0x40000

#ifdef _NSS_INTERNAL_
	,
	zVALID_GET_INFO_MASK = (zGET_STD_INFO | zGET_NAME | zGET_ALL_NAMES |
			zGET_PRIMARY_NAMESPACE | zGET_TIMES_IN_SECS |
			zGET_TIMES_IN_MICROS | zGET_IDS | zGET_STORAGE_USED |
			zGET_BLOCK_SIZE | zGET_COUNTS | zGET_DATA_STREAM_INFO |
			zGET_DELETED_INFO | zGET_MAC_METADATA | zGET_UNIX_METADATA |
			zGET_VOLUME_INFO | zGET_VOL_SALVAGE_INFO | zGET_POOL_INFO )
#endif
};



/*-------------------------------------------------------------------------
 *	Definition of handlePathType 
 *-------------------------------------------------------------------------*/
#define	zHPT_SIMPLE						1
#define	zHPT_FULL						2
#define	zHPT_CONTEXT					3
#define	zHPT_VOLUME						4
#define	zHPT_VOLUME_ZID					5
#define	zHPT_ADMIN_VOLUME   			6
#define	zHPT_ADMIN_VOLUME_ZID 			7
#define zHPT_FILEHANDLEID				8
#define	zHPT_SEARCH_PATTERN				9
#define zHPT_BEAST						10

#ifdef _NSS_INTERNAL_
	#define zHPT_UNDEFINED				0
	#define	zHPT_VOLUME_ZID_PATH		11
	#define	zHPT_ADMIN_VOLUME_ZID_PATH	12
#endif


/*---------------------------------------------------------------------------
 *	Definition of linkFlags in zLink
 *---------------------------------------------------------------------------*/
#define zLF_HARD_LINK					0x00000001

#ifdef _NSS_INTERNAL_
	#define zVALID_LINK_FLAGS (zLF_HARD_LINK)
#endif
			
/*---------------------------------------------------------------------------
 *	Definition of matchFlags in zMatchAttr_s
 *---------------------------------------------------------------------------*/
#define zMATCH_ALL_DERIVED_TYPES		0x00000001

#define zMATCH_HIDDEN			0x1		/* Match if the hidden bit is set. */
#define zMATCH_NON_HIDDEN		0x2		/* Match if the hidden bit is NOT
										 * set.  If both zMATCH_NON_HIDDEN
										 * and zMATCH_HIDDEN are set, both
										 * hidden and non-hidden files will
										 * match.  If neither is set, nothing
										 * will match.
										 */
#define zMATCH_DIRECTORY		0x4		/* Match if the file object is
										 * a directory.
										 */
#define zMATCH_NON_DIRECTORY	0x8		/* Match if the file object is NOT
										 * a directory.  If both
										 * zMATCH_NON_DIRECTORY and
										 * zMATCH_DIRECTORY are set, both
										 * directories and other files will
										 * match.  If neither is set, nothing
										 * will match.
										 */ 
#define zMATCH_SYSTEM			0x10	/* Match if a system file. */
#define zMATCH_NON_SYSTEM		0x20	/* Match if NOT a system file.
										 * If both zMATCH_NON_SYSTEM and
										 * zMATCH_SYSTEM are set, both
										 * system and non-system file will
										 * match.  If neither is set,
										 * nothing will match.
										 */
#define zMATCH_ALL				(~0)	/* A define combining all of the
										 * above flags that matches all
										 * file objects in the given directory.
										 * NOTE: if the match flag is set
										 * to 0, it is ingnored and has the
										 * same affect as zMATCH_ALL.
										 */

/*-------------------------------------------------------------------------
 *	Definition of modifyInfoMask
 *-------------------------------------------------------------------------*/
#define zMOD_FILE_ATTRIBUTES			0x00000001
#define zMOD_CREATED_TIME				0x00000002
#define zMOD_ARCHIVED_TIME				0x00000004
#define zMOD_MODIFIED_TIME				0x00000008
#define zMOD_ACCESSED_TIME				0x00000010
#define zMOD_METADATA_MODIFIED_TIME		0x00000020
#define zMOD_OWNER_ID					0x00000040
#define zMOD_ARCHIVER_ID				0x00000080
#define zMOD_MODIFIER_ID				0x00000100
#define zMOD_METADATA_MODIFIER_ID		0x00000200
#define zMOD_PRIMARY_NAMESPACE			0x00000400
#define zMOD_DELETED_INFO				0x00000800
#define zMOD_MAC_METADATA				0x00001000
#define zMOD_UNIX_METADATA				0x00002000
#define zMOD_EXTATTR_FLAGS				0x00004000
#define zMOD_VOL_ATTRIBUTES				0x00008000
#define zMOD_VOL_NDS_OBJECT_ID			0x00010000
#define zMOD_VOL_MIN_KEEP_SECONDS		0x00020000
#define zMOD_VOL_MAX_KEEP_SECONDS		0x00040000
#define zMOD_VOL_LOW_WATER_MARK			0x00080000
#define zMOD_VOL_HIGH_WATER_MARK		0x00100000
#define zMOD_POOL_ATTRIBUTES			0x00200000
#define zMOD_POOL_NDS_OBJECT_ID			0x00400000
#define zMOD_VOL_DATA_SHREDDING_COUNT	0x00800000
#define zMOD_VOL_QUOTA					0x01000000

#ifdef _NSS_INTERNAL_
	#define zVALID_MOD_EXTATTR_INFO_MASK \
			(zMOD_EXTATTR_FLAGS)
#endif

#ifdef _NSS_INTERNAL_
	#define zVALID_MOD_VOL_INFO_MASK \
			(zMOD_VOL_ATTRIBUTES | \
			 zMOD_VOL_NDS_OBJECT_ID | \
			 zMOD_VOL_MIN_KEEP_SECONDS | \
			 zMOD_VOL_MAX_KEEP_SECONDS | \
			 zMOD_VOL_LOW_WATER_MARK | \
			 zMOD_VOL_HIGH_WATER_MARK |	\
			 zMOD_VOL_DATA_SHREDDING_COUNT| \
			 zMOD_VOL_QUOTA)
#endif

#ifdef _NSS_INTERNAL_
	#define zVALID_MOD_POOL_INFO_MASK \
			(zMOD_POOL_ATTRIBUTES | \
			 zMOD_POOL_NDS_OBJECT_ID)
#endif

#ifdef _NSS_INTERNAL_
	#define zVALID_MOD_INFO_MASK \
			(zMOD_FILE_ATTRIBUTES | \
			zMOD_CREATED_TIME | zMOD_ARCHIVED_TIME | \
			zMOD_MODIFIED_TIME | zMOD_ACCESSED_TIME | \
			zMOD_METADATA_MODIFIED_TIME | \
			zMOD_OWNER_ID | zMOD_ARCHIVER_ID | \
			zMOD_MODIFIER_ID | zMOD_METADATA_MODIFIER_ID | \
			zMOD_PRIMARY_NAMESPACE | zMOD_DELETED_INFO | \
			zMOD_MAC_METADATA | zMOD_UNIX_METADATA | \
			zVALID_MOD_EXTATTR_INFO_MASK | \
			zVALID_MOD_VOL_INFO_MASK | zVALID_MOD_POOL_INFO_MASK)
#endif


/*---------------------------------------------------------------------------
 *	Definition of nameType.  This is a field that is part of the key
 *	for directories.  It provides for multiple parallel but separate naming
 *	environments in the same directory container.
 *---------------------------------------------------------------------------*/
#define zNTYPE_FILE							0
#define zNTYPE_DATA_STREAM 					1
#define zNTYPE_EXTENDED_ATTRIBUTE			2
#define zNTYPE_DELETED_FILE					3

#ifdef _NSS_INTERNAL_
	#define zNTYPE_MAX_DEFINED					3

	/*#define zNTYPE_DELETED_DATA_STREAM 		4 */
	/*#define zNTYPE_DELETED_EXTENDED_ATTRIBUTE	5 */

	#define zNTYPE_INVALID						(-1)
#endif

/*-------------------------------------------------------------------------
 *	Definitions for nameSpaceID
 *-------------------------------------------------------------------------*/
#define zNSPACE_DOS						0
#define zNSPACE_MAC						1
#define zNSPACE_UNIX					2
/*#define zNSPACE_FTAM					3	not supported*/	
#define zNSPACE_LONG					4
#define zNSPACE_DATA_STREAM				6
#define zNSPACE_EXTENDED_ATTRIBUTE		7

#define zNSPACE_INVALID					(-1)

#define zNSPACE_DOS_MASK				(1<<zNSPACE_DOS)
#define zNSPACE_MAC_MASK				(1<<zNSPACE_MAC)
#define zNSPACE_UNIX_MASK				(1<<zNSPACE_UNIX)
#define zNSPACE_LONG_MASK				(1<<zNSPACE_LONG)
#define zNSPACE_DATA_STREAM_MASK		(1<<zNSPACE_DATA_STREAM)
#define zNSPACE_EXTENDED_ATTRIBUTE_MASK (1<<zNSPACE_EXTENDED_ATTRIBUTE)

#define zNSPACE_ALL_MASK		(0xffffffff)


/*---------------------------------------------------------------------------
 *	Definitions for nameSysID (naming system ID)
 *---------------------------------------------------------------------------*/

/* TBD */


/*---------------------------------------------------------------------------
 *	Definition of special path characters
 *---------------------------------------------------------------------------*/
#ifdef _NSS_INTERNAL_
	/* The zPATH_CHANGE_NAMESPACE character is followed by a WORD nameSpaceID
	 * and a WORD nameType and a word NULL terminator.  To prevent the
	 * nameSpaceID from being a NULL, we or the following bit into the
	 * nameSpaceID. */
	#define zPATH_CHANGE_NSPACE_BIT	0x8000
#endif

/*---------------------------------------------------------------------------
 *	Definition of character formats for paths
 *---------------------------------------------------------------------------*/
#define zPFMT_ASCII					1
#define zPFMT_UNICODE				2
#define zPFMT_UTF8					6

#ifdef _NSS_INTERNAL_
	#define zPFMT_ASCII_LP			3
	#define zPFMT_ASCII_COUNTED		4
	#define zPFMT_ASCII_LEN			5
	#define zPFMT_UNDEFINED			0
#endif

#define zMODE_VOLUME_ID			0x80000000
#define zMODE_UTF8				0x40000000
#define zMODE_DELETED			0x20000000
#define zMODE_LINK				0x10000000


/*-------------------------------------------------------------------------
 *	Definition of renameFlags
 *-------------------------------------------------------------------------*/
#define zRENAME_ALLOW_RENAMES_TO_MYSELF		0x00000001
#define zRENAME_THIS_NAME_SPACE_ONLY		0x00000004

#ifdef _NSS_INTERNAL_
	#define zRENAME_COMPATABILITY			0x00000002
	#define zRENAME_DONT_RENAME_FILES		0x00000008
	#define zRENAME_DONT_RENAME_DIRECTORIES	0x00000010
	#define zRENAME_TARGET_IS_PATTERN 		0x00000020


	#define	zVALID_RENAME_FLAGS \
			(zRENAME_ALLOW_RENAMES_TO_MYSELF | \
			 zRENAME_THIS_NAME_SPACE_ONLY | \
			 zRENAME_COMPATABILITY | \
			 zRENAME_DONT_RENAME_FILES | \
			 zRENAME_DONT_RENAME_DIRECTORIES | \
			 zRENAME_TARGET_IS_PATTERN)
#endif



/*
 *	Definition of requestedRights:
 *
 * zRR_READ_ACCESS					Read access to file object
 *
 * zRR_WRITE_ACCESS					Write access to file object
 *
 * zRR_DENY_READ					Don't let anyone else read this file
 *									If the file is already open for reading,
 *									the open fails.
 *
 * zRR_DENY_WRITE					Don't let anyone else write this file.
 *									If the file is already open for writing,
 *									the open fails.
 *
 * zRR_SCAN_ACCESS					Scan access to file object.  Can use
 *									zWildRead to scan the
 *									files/subdirectories/attributes
 *									of a file or directory.
 *
 * zRR_DELETE_FILE_ON_CLOSE			Delete the file when it is closed.
 *
 * zRR_FLUSH_ON_CLOSE				Flush all the data blocks when the
 *									file is closed.
 *
 * zRR_PURGE_IMMEDIATE_ON_CLOSE		If file is to be deleted on close,
 *									then don't put it in salavage system.
 *
 * zRR_DIO_MODE						Open file in Direct I/O Mode.
 *									Replaces changing to direct I/O mode.
 *									Conflicts with regular open modes of
 *									the file.
 *
 * zRR_TRANSACTION_ACTIVE			Have the file use the default transaction.
 *
 * zRR_CREATE_WITHOUT_READ_ACCESS	Give only write access to the file when
 *									is created.
 *
 * zRR_OPENER_CAN_DELETE_WHILE_OPEN	Only the opener of the file can
 *									delete the file while it is open.
 *
 * zRR_CANT_DELETE_WHILE_OPEN		Don't let anyone delete this file
 *									while the file is open.
 *
 * zRR_DONT_UPDATE_ACCESS_TIME		Don't update the access time. Intended
 *									to let the clients access the icon
 *									information without causing a change
 *									in the files meta-data that would
 *									eventually have to be written.
 */
#define zRR_READ_ACCESS						0x00000001
#define zRR_WRITE_ACCESS					0x00000002
#define zRR_DENY_READ						0x00000004
#define zRR_DENY_WRITE						0x00000008
#define zRR_SCAN_ACCESS						0x00000010
#define zRR_ENABLE_IO_ON_COMPRESSED_DATA	0x00000100
#define zRR_LEAVE_FILE_COMPRESSED	        0x00000200
#define zRR_DELETE_FILE_ON_CLOSE			0x00000400

#define zRR_FLUSH_ON_CLOSE					0x00000800
#define zRR_PURGE_IMMEDIATE_ON_CLOSE		0x00001000
#define zRR_DIO_MODE						0x00002000

#define zRR_ALLOW_SECURE_DIRECTORY_ACCESS	0x00020000

#define zRR_TRANSACTION_ACTIVE				0x00100000

#define zRR_READ_ACCESS_TO_SNAPSHOT			0x04000000
#define zRR_DENY_RW_OPENER_CAN_REOPEN		0x08000000
#define zRR_CREATE_WITHOUT_READ_ACCESS		0x10000000
#define zRR_OPENER_CAN_DELETE_WHILE_OPEN	0x20000000
#define zRR_CANT_DELETE_WHILE_OPEN			0x40000000
#define zRR_DONT_UPDATE_ACCESS_TIME			0x80000000


#ifdef _NSS_INTERNAL_
	#define zVALID_FILE_REQ_RIGHTS 					  \
			(zRR_READ_ACCESS 						| \
			zRR_WRITE_ACCESS 						| \
			zRR_DENY_READ 							| \
			zRR_DENY_WRITE 							| \
			zRR_SCAN_ACCESS 						| \
			zRR_ENABLE_IO_ON_COMPRESSED_DATA 		| \
			zRR_LEAVE_FILE_COMPRESSED 				| \
			zRR_DELETE_FILE_ON_CLOSE 				| \
			zRR_FLUSH_ON_CLOSE 						| \
			zRR_PURGE_IMMEDIATE_ON_CLOSE 			| \
			zRR_DIO_MODE 							| \
			zRR_ALLOW_SECURE_DIRECTORY_ACCESS 		| \
			zRR_READ_ACCESS_TO_SNAPSHOT				| \
			zRR_DENY_RW_OPENER_CAN_REOPEN 			| \
			zRR_CREATE_WITHOUT_READ_ACCESS 			| \
			zRR_OPENER_CAN_DELETE_WHILE_OPEN 		| \
			zRR_CANT_DELETE_WHILE_OPEN 				| \
			zRR_DONT_UPDATE_ACCESS_TIME)
#endif


/*-------------------------------------------------------------------------
 *	Definition of setSizeFlags
 *-------------------------------------------------------------------------*/
#define	zSETSIZE_NON_SPARSE_FILE 0x00000001 /* Alloc blocks to extend the	*
											 * file  	            		*/
#define zSETSIZE_NO_ZERO_FILL	 0x00000002 /* Do not zero fill the newly	*
											 * allocated blocks   			*/
#define zSETSIZE_UNDO_ON_ERR	 0x00000004	/* In non sparse cases truncate	*
											 * back to original eof if an	*
											 * error occurs.				*/
#define zSETSIZE_PHYSICAL_ONLY	 0x00000008 /* Change the physical EOF		*
											 * only, dont change logical	*
											 * EOF. This means non sparse	*
											 *  for the expand case.		*/
#define zSETSIZE_LOGICAL_ONLY	 0x00000010	/* Change only the logical		*
											 * EOF, expand will always be	*
											 * sparse, and trucate won't	*
											 * free physical blocks			*/
#define zSETSIZE_COMPRESSED      0x00000020 /* Set size of compressed stream */

#ifdef _NSS_INTERNAL_
	#define zVALID_SETSIZE_FLAGS \
		(zSETSIZE_NON_SPARSE_FILE | zSETSIZE_PHYSICAL_ONLY | \
	 	 zSETSIZE_UNDO_ON_ERR | zSETSIZE_NO_ZERO_FILL |  \
         zSETSIZE_LOGICAL_ONLY | zSETSIZE_COMPRESSED)
#endif


/*-------------------------------------------------------------------------
 *	Defintions of taskID
 *-------------------------------------------------------------------------*/
#define zNSS_TASK	50		/* We use the NSS task ID */

#ifdef _NSS_INTERNAL_
	#define zNO_TASK	0
#endif

/*-----------------------------------------------------------------------
 *	Definition of volumeState
 *-----------------------------------------------------------------------*/
#define zVOLSTATE_UNKNOWN			0	/* Volume is new and not available 	*/
#define zVOLSTATE_DEACTIVE			2	/* Volume is not currently activated */
#define zVOLSTATE_MAINTENANCE		3	/* Volume is in a maintenance mode */
#define zVOLSTATE_ACTIVE			6	/* Volume is active and available */

/*-----------------------------------------------------------------------
 *	Byte range lock types
 *-----------------------------------------------------------------------*/

enum {
	zLOCK_SHARED    = 0x1,	/* Shared or read lock */
	zLOCK_EXCLUSIVE = 0x3	/* Exclusive or write lock */
};

/*-----------------------------------------------------------------------
 *	Definitions for user space restrictions
 *-----------------------------------------------------------------------*/
#define zUSER_NO_RESTRICTIONS		0x7FFFFFFFFFFFFFFF
#define zDIR_NO_QUOTA				0x7FFFFFFFFFFFFFFF

/*-------------------------------------------------------------------------
 *	Well Known ZIDs on a volume
 *-------------------------------------------------------------------------*/
#define zDEFAULT_ZID_ALLOCATION_RANGE 128	/* how many ZIDS to get when we ask the volume*/

#define zINVALID_ZID			((Zid_t)0)		/* value for an INVALID ZID*/
#define zOPEN_ENTRY_ZID			((Zid_t)-1)		/* zid for open slot in the beast b-tree */
#define zROOTDIR_ZID			((Zid_t)zFIRST_ALLOCATABLE_ZID-1)	/* ID of rootdir*/
#define	zDOT_COOKIE		(zROOTDIR_ZID-1)	/* NFS cookie for "." */
#define zDOTDOT_COOKIE	(zROOTDIR_ZID-2)	/* NFS cookie for ".." */

/* ID of first ZID definable by users, all well known zids will be less than this */
#define zFIRST_ALLOCATABLE_ZID	((Zid_t)zDEFAULT_ZID_ALLOCATION_RANGE)

/*-------------------------------------------------------------------------
 *	Logical Volume no size quota (LV size will mirror pool size)
 *-------------------------------------------------------------------------*/
#define zLV_NO_QUOTA_SIZE	0x7FFFFFFFFFFFF000

/*=========================================================================
 *=========================================================================
 *	"New" Structure definitions
 *=========================================================================
 *=========================================================================*/


/*=========================================================================
 *	Definition of zInfo_s used for returning/modifying information about a
 *	file object.
 *=========================================================================*/
/*-------------------------------------------------------------------------
 *  In order to allow the user to override the default size of the
 *	variableData portion of this structure, a define has been provided
 *	to specify the size of that portion.  This then allows for stack
 *	variables to be created of type zGetInfo, with those stack variables
 *	having the desired size for the variable portion.
 *-------------------------------------------------------------------------*/
#ifndef zGET_INFO_VARIABLE_DATA_SIZE
	#define zGET_INFO_VARIABLE_DATA_SIZE	(zMAX_COMPONENT_NAME*2)
#endif

typedef struct zMacInfo_s
{
	BYTE 	finderInfo[32];
	BYTE 	proDOSInfo[6];
	BYTE	filler[2];
	LONG	dirRightsMask;
} zMacInfo_s;

typedef struct zUnixInfo_s
{
 	LONG	fMode;
 	LONG	rDev;
 	LONG	myFlags;
 	LONG	nfsUID;
 	LONG 	nfsGID;
	LONG	nwUID;
	LONG	nwGID;
	LONG	nwEveryone;
	LONG	nwUIDRights;
	LONG	nwGIDRights;
	LONG	nwEveryoneRights;
 	BYTE	acsFlags;
 	BYTE	firstCreated;
 	SWORD	variableSize;
	/* If variableSize is non-zero, there will be "variableSize" number of
	 * additional bytes of meta data associated with this file. The format
	 * of this variable data is unknown to NSS
	 */
} zUnixInfo_s;

/*---------------------------------------------------------------------------
 *	Definition of typeInfo returned for zFTYPE_VOLUME
 *---------------------------------------------------------------------------*/
typedef struct zVolumeInfo_s
{
	VolumeID_t	volumeID;
   	NDSid_t		ndsObjectID;			/* zMOD_VOL_NDS_OBJECT_ID */
	LONG		volumeState;
	LONG		nameSpaceMask;
	struct {
		QUAD	enabled;		/* zMOD_VOL_FEATURES */
		QUAD	enableModMask;	/* zMOD_VOL_FEATURES */
		QUAD	supported;
	} features;
	QUAD		maximumFileSize;	
	QUAD		totalSpaceQuota; 		/* zMOD_VOL_QUOTA */
	QUAD		numUsedBytes;
	QUAD		numObjects;
	QUAD		numFiles;
	LONG		authModelID;
	LONG		dataShreddingCount;		/* zMOD_VOL_DATA_SHREDDING_COUNT */
	struct {							/* zGET_VOL_SALVAGE_INFO */
		QUAD	purgeableBytes;
		QUAD	nonPurgeableBytes;
		QUAD	numDeletedFiles;
		QUAD	oldestDeletedTime;
		LONG	minKeepSeconds;			/* zMOD_VOL_MIN_KEEP_SECONDS */
		LONG	maxKeepSeconds;			/* zMOD_VOL_MAX_KEEP_SECONDS */
		LONG	lowWaterMark;			/* zMOD_VOL_LOW_WATER_MARK */
		LONG	highWaterMark;			/* zMOD_VOL_HIGH_WATER_MARK */
	} salvage;
    struct {                            /* Compression-related Statistics */
        QUAD    numCompressedFiles;
        QUAD    numCompDelFiles;        /* # compressed deleted files */
        QUAD    numUncompressibleFiles;
        QUAD    numPreCompressedBytes;
        QUAD    numCompressedBytes;
    } comp;
} zVolumeInfo_s;

typedef struct zPoolInfo_s
{
	VolumeID_t	poolID;
	NDSid_t		ndsObjectID;			/* zMOD_POOL_NDS_OBJECT_ID	*/
	LONG		poolState;
	LONG		nameSpaceMask;
	struct {
		QUAD	enabled;				/* zMOD_POOL_FEATURES	*/
		QUAD	enableModMask;			/* zMOD_POOL_FEATURES	*/
		QUAD	supported;
	} features;
	QUAD		totalSpace;
	QUAD		numUsedBytes;
	QUAD		purgeableBytes;
	QUAD		nonPurgeableBytes;
} zPoolInfo_s;

enum
{
	zINFO_VERSION_A = 1
};
	/*
	 * Fields are arranged to make alignment easier (QUAD, LONG, WORD, BYTE)
	 */
typedef struct zInfo_s
{
	LONG	infoVersion;	/* Verion of Info structure			*/
	SLONG	totalBytes;		/* Total bytes in fixed and variable
							 * data sections.  We use both sections
							 * so we can grow the fixed area without
							 * breaking older code.
							 */
	SLONG	nextByte;		/* Next byte to use in variable area */
	LONG	padding;
	QUAD	retMask;		/* Fields set in Info structure		*/
	struct {						/* zGET_STD_INFO			*/
		Zid_t		zid;			/* Zid of the file			*/
		Zid_t		dataStreamZid;	/* Data stream Zid			*/
		Zid_t		parentZid;		/* Parent used to open file	*/
		QUAD		logicalEOF;		/* End of File (see physicalEOF)	*/
		VolumeID_t	volumeID;
		LONG		fileType;
		LONG		fileAttributes;			/* zMOD_FILE_ATTRIBUTES */
		LONG		fileAttributesModMask;	/* zMOD_FILE_ATTRIBUTES */
		LONG		padding;
	} std;

	struct {						/* zGET_STORAGE_USED */
		QUAD	 physicalEOF;	/* The file may have more blocks allocated
								 * than it needs, this marks of the end of
								 * extra data blocks
								 */
    	QUAD	 dataBytes;		/* Storage used by user data	*/
    	QUAD	 metaDataBytes;	/* Storage used by Meta-data	*/
	} storageUsed;

	LONG	primaryNameSpaceID;		/* zGET_PRIMARY_NAMESPACE */
									/* zMOD_PRIMARY_NAMESPACE */
	LONG 	nameStart;				/* zGET_NAME - index into variableData */

	struct {						/* zGET_ALL_NAMES */
		LONG 	numEntries;
		LONG 	fileNameArray;		/* Index of the array of indicies in
									 * the variableData area
									 */
	} names;

	struct {						/* zGET_TIMES */
		QUAD	created;			/* zMOD_CREATED_TIME */
		QUAD	archived;			/* zMOD_ARCHIVED_TIME */
		QUAD	modified;			/* zMOD_MODIFIED_TIME */
		QUAD	accessed;			/* zMOD_ACCESSED_TIME */
		QUAD	metaDataModified;	/* zMOD_METADATA_MODIFIED_TIME */
	} time;

	struct {						/* zGET_IDS */
	 	UserID_t owner;				/* zMOD_OWNER_ID */
    	UserID_t archiver;			/* zMOD_ARCHIVER_ID */
    	UserID_t modifier;			/* zMOD_MODIFIER_ID */
    	UserID_t metaDataModifier;	/* zMOD_METADATA_MODIFIER_ID */
	} id;

	struct {						/* zGET_BLOCK_SIZE */
		LONG	 size;
		LONG	 sizeShift;
	} blockSize;

	struct {						/* zGET_COUNTS */
		LONG	 open;
		LONG	 hardLink;
	} count;

	struct {						/* zGET_DATA_STREAM_INFO */
		LONG	 count;
		LONG	 totalNameSize;
		QUAD	 totalDataSize;
	} dataStream;

	struct {						/* zGET_EXTENDED_ATTRIBUTE_INFO */
		LONG	 count;
		LONG	 totalNameSize;
		QUAD	 totalDataSize;
	} extAttr;

	struct {						/* zGET_DELETED_INFO */
									/* zMOD_DELETED_INFO */
		QUAD	 time;
		UserID_t id;
	} deleted;

	struct {						/* zGET_MAC_METADATA */
		zMacInfo_s info;			/* zMOD_MAC_METADATA */
	} macNS;

	struct {						/* zGET_UNIX_METADATA */
		zUnixInfo_s info;			/* zMOD_UNIX_METADATA */
		LONG		offsetToData;
	} unixNS;

	zVolumeInfo_s	vol;			/* zGET_VOLUME_INFO */

	zPoolInfo_s		pool;			/*	zGET_POOL_METADATA	*/

	LONG	extAttrUserFlags;		/* zGET_EXTATTR_FLAGS */
									/* zMOD_EXTATTR_FLAGS */

	/* The variable portion will contain all variable size optional data */
	BYTE		variableData[zGET_INFO_VARIABLE_DATA_SIZE];

} zInfo_s;

#define zINFO_PTR(_info, _i)	(&(((BYTE *)(_info))[(_i)]))

#define zINFO_NAME(_info)		\
	((unicode_t *)zINFO_PTR((_info), (_info)->nameStart))

#define zINFO_FILENAMES(_info)	\
	((LONG *)&((BYTE *)(_info))[(_info)->names.fileNameArray])

unicode_t *zInfoGetFileName(zInfo_s *info, NINT nameSpace);


/*---------------------------------------------------------------------------
 *	Definition of typeInfo returned for zFTYPE_EXTENDED_ATTRIBUTE
 *---------------------------------------------------------------------------*/
typedef struct ZnewExtAttrInfo_s
{
	NINT		extAttrUserFlags;		/* zGET_EXTATTR_FLAGS */
										/* zMOD_EXTATTR_FLAGS */
} ZnewExtAttrInfo_s;

/*---------------------------------------------------------------------------
 *	Definition of typeInfo returned for zFTYPE_SYM_LINK
 *---------------------------------------------------------------------------*/
#ifndef zGET_SYMLINK_VARIABLE_DATA_SIZE
	#define zGET_SYMLINK_VARIABLE_DATA_SIZE	(zMAX_FULL_NAME*2)
#endif

typedef struct ZnewGetSymLinkInfo_s
{
	LONG	symLinkStart;	/* Start of path name in variableData */
	BYTE	variableData[zGET_SYMLINK_VARIABLE_DATA_SIZE];
} ZnewGetSymLinkInfo_s;


/*=========================================================================
 *=========================================================================
 *	Structure definitions
 *=========================================================================
 *=========================================================================*/

/*=========================================================================
 *	Definition of zGetName_s used for returning name/path  information
 *	about a file object
 *=========================================================================*/
/*-------------------------------------------------------------------------
 *  In order to allow the user to override the default size of the
 *	variableData portion of this structure, a define has been provided
 *	to specify the size of that portion.  This then allows for stack
 *	variables to be created of type zGetName_s, with those stack variables
 *	having the desired size for the variable portion.
 *-------------------------------------------------------------------------*/
#ifndef zGET_NAME_VARIABLE_DATA_SIZE
	#define zGET_NAME_VARIABLE_DATA_SIZE	(zMAX_FULL_NAME*2)
#endif

typedef struct zGetName_s
{
	/* This data is always returned */
	void		*name;

	/* The variable portion will contain all variable size optional data */
	BYTE		variableData[zGET_NAME_VARIABLE_DATA_SIZE];

} zGetName_s;


/*=========================================================================
 *	Definition of zInfo_s used for returning/modifying information about a
 *	file object.
 *=========================================================================*/
/*-------------------------------------------------------------------------
 *  In order to allow the user to override the default size of the
 *	variableData portion of this structure, a define has been provided
 *	to specify the size of that portion.  This then allows for stack
 *	variables to be created of type zGetInfo, with those stack variables
 *	having the desired size for the variable portion.
 *-------------------------------------------------------------------------*/
#ifndef zGET_INFO_VARIABLE_DATA_SIZE
	#define zGET_INFO_VARIABLE_DATA_SIZE	(zMAX_COMPONENT_NAME*2)
#endif




/*===========================================================================*/
/*===========================================================================*/
/* This is Type specific Get Info for Extended Attribute objects */
/*===========================================================================*/
/*===========================================================================*/
/*-------------------------------------------------------------------------
 *	Definition of getTypeInfoMask for zFTYPE_EXTENDED_ATTRIBUTE
 *  (zExtAttrInfo_s)
 *-------------------------------------------------------------------------*/
#define Z_GET_EXTATTR_FLAGS				0x01000000

#ifdef _NSS_INTERNAL_
	#define Z_VALID_GET_EXT_ATTR_INFO_MASK \
			(Z_GET_EXTATTR_FLAGS)
#endif

/*-------------------------------------------------------------------------
 *	Definition of modifyTypeInfoMask for zFTYPE_EXTENDED_ATTRIBUTE
 *	(ModifyExtAttrInfo_s)
 *-------------------------------------------------------------------------*/
#define Z_MOD_EXTATTR_FLAGS				0x01000000

#ifdef _NSS_INTERNAL_
	#define Z_VALID_MOD_EXTATTR_INFO_MASK \
			(Z_MOD_EXTATTR_FLAGS)
#endif

/*---------------------------------------------------------------------------
 *	Definition of typeInfo returned for zFTYPE_EXTENDED_ATTRIBUTE
 *---------------------------------------------------------------------------*/
typedef struct zExtAttrInfo_s
{
	NINT		extAttrUserFlags;		/* zGET_EXTATTR_FLAGS */
										/* zMOD_EXTATTR_FLAGS */
} zExtAttrInfo_s;

/*---------------------------------------------------------------------------
 *	Definition of typeInfo returned for zFTYPE_SYM_LINK
 *---------------------------------------------------------------------------*/
#ifndef zGET_SYMLINK_VARIABLE_DATA_SIZE
	#define zGET_SYMLINK_VARIABLE_DATA_SIZE	(zMAX_FULL_NAME*2)
#endif

typedef struct zGetSymLinkInfo_s
{
	unicode_t	*symLinkPath;
	BYTE	variableData[zGET_SYMLINK_VARIABLE_DATA_SIZE];
} zGetSymLinkInfo_s;


/*=========================================================================
 *	This structure is used to specify match criteria to be applied to a
 *	file when identifing it via the naming information.  If a selected file
 *	does not match this naming criteria, it will be skipped over.
 *=========================================================================*/
typedef struct zMatchAttr_s
{
	NINT	matchFlags;
	NINT	matchFileType;

	NINT 	matchFileAttrSet;
	NINT	matchFileAttrNotSet;

	NINT	matchTypeAttrSet;
	NINT	matchTypeAttrNotSet;
} zMatchAttr_s;



/*---------------------------------------------------------------------------
 *	Buffer definition for the buffer returned by AsyncRead calls
 *---------------------------------------------------------------------------*/
/*** This Buffer matches PubBuffer_s. Any modifications made here
 *** should be reflected in PubBuffer_s in xCache.h 
 ***/
typedef struct zBuffer_s
{
	BYTE *data;
	ADDR reReadHandle;
	QUAD blockNumber;
} zBuffer_s;


/*---------------------------------------------------------------------------
 * Structure to pass user restriction info from the browse routine.
 *---------------------------------------------------------------------------*/
typedef struct zUserRest_s
{
	UserID_t	userID;
	SQUAD		usedAmount;
	SQUAD		restrictionAmount;
} zUserRest_s;


/*---------------------------------------------------------------------------
 * Structure to pass trustee IDs and restriction info for authorization model
 *---------------------------------------------------------------------------*/
typedef struct zTrustee_s
{
	UserID_t	trustee;
	LONG		rights;
	LONG		padding;
} zTrustee_s;

/************************************************************************
 *  States of the virtual file
 ***********************************************************************/
#define FHV_NEW					0x00
#define FHV_ACTUAL_DATA			0x01	/* set if we are working with the real data */
#define FHV_RESULT_DATA			0x02	/* set when data in result buffer is good */
#define FHV_FORCE_RESULT_READ	0x04	/* read from result buffer only */

/*-------------------------------------------------------------------------
 *	Structure maintained in the file handle for a virtual file
 *-------------------------------------------------------------------------*/
#define VIRT_DATASTREAM_NAME_SIZE	64
typedef struct VirtInfo_s
{
	NINT	resultEOF;			/* EOF for the result buffer */
	NINT	virtState;			/* state of the virtual file */
	NINT	offsetAdjustment;	/* amount to adjust the offset */
	NINT	nameSize;			/* Size of the datastream name */
	NINT	resultBufferSize;	/* Size of the result buffer in bytes */
	void	(*closeFunc)(		/* function to be called at close time */
				struct VirtInfo_s *virtInfo);
	NINT	memAllocatedSize;	/* size of memory pointed to by memPtr */
	NINT	memUsedSize;		/* amount of the memory in use */
	void	*memPtr;			/* a pointer to memory supplied by the function */
	BYTE	*resultBuffer;		/* buffer to hold data between operations */
	utf8_t	virtDataStreamName[VIRT_DATASTREAM_NAME_SIZE];
								/* The name of the virtual datastream */	
} VirtInfo_s;

#ifdef __cplusplus
}
#endif

#endif /* _ZPARAMS_H_ */
