/****************************************************************************
 |
 |  (C) Copyright 2001 Novell, Inc.
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
 |	 Storage Management Services (SMS)
 |
 |---------------------------------------------------------------------------
 |
 | $Author$
 | $Modtime:   27 Mar 2002 17:03:12  $
 |
 | $Workfile:   fsInterface.h  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Initialise TSA
 +-------------------------------------------------------------------------*/

#ifndef _FILSYSINTERFACE_H_IS_INCLUDED
#define _FILSYSINTERFACE_H_IS_INCLUDED

#include <zParams.h>
#include <zPublics.h>
#include <zError.h>
#include <zFriends.h>

#ifdef N_PLAT_NLM
#include <mpktypes.h>
#include <dstruct.h>
#include <config.h>
#include <migrate.h>
#elif N_PLAT_GNU
#include <sys/stat.h>
#include <lcfsdefines.h>
#endif
#include <linktab.h>

#include <nwsms.h>
#include <smsmac.h>
#include <tsalib.h>
#include <tsaunicode.h>

/* Local defines */
#define	LAST_NAMESPACE		4
#define	FS_VERSION_A		zINFO_VERSION_A
#define FS_LINK_NAME		5
/** FS layer name space definitions **/
#define FS_NAME_SPACE_DOS			zNSPACE_DOS	
#define FS_NAME_SPACE_MAC			zNSPACE_MAC
#define FS_NAME_SPACE_NFS			zNSPACE_UNIX
#define FS_NAME_SPACE_LONG			zNSPACE_LONG
#define FS_NSPACE_DATA_STREAM 			zNSPACE_DATA_STREAM
#define FS_NSPACE_EXTENDED_ATTRIBUTE		zNSPACE_EXTENDED_ATTRIBUTE

/**	FS Layer file attributes definitions **/
#define FS_READ_ONLY						zFA_READ_ONLY
#define FS_HIDDEN							zFA_HIDDEN
#define FS_SYSTEM							zFA_SYSTEM
#define FS_EXECUTE							zFA_EXECUTE
#define FS_SUBDIRECTORY						zFA_SUBDIRECTORY
#define FS_ARCHIVE							zFA_ARCHIVE
#define FS_SHAREABLE						zFA_SHAREABLE
#define FS_SMODE_BITS						zFA_SMODE_BITS
#define FS_SMODE_BIT1						0x00000100
#define FS_SMODE_BIT2						0x00000200
#define FS_SMODE_BIT3						0x00000400
#define FS_REMOTE_DATA_ACCESS          		zFS_REMOTE_DATA_ACCESS
#define FS_SUPPORT_MODULE_ROOT_BIT     		0x01000000
#define FS_NO_SUBALLOC						zFA_NO_SUBALLOC
#define FS_TRANSACTION						zFA_TRANSACTION
#define FS_NOT_VIRTUAL_FILE					zFA_NOT_VIRTUAL_FILE
#define FS_IMMEDIATE_PURGE					zFA_IMMEDIATE_PURGE
#define FS_RENAME_INHIBIT					zFA_RENAME_INHIBIT
#define FS_DELETE_INHIBIT					zFA_DELETE_INHIBIT
#define FS_COPY_INHIBIT						zFA_COPY_INHIBIT
#define FS_IS_ADMIN_LINK					zFA_IS_ADMIN_LINK
#define FS_IS_LINK							zFA_IS_LINK
#define FS_REMOTE_DATA_INHIBIT				zFA_REMOTE_DATA_INHIBIT
#define FS_COMPRESS_FILE_IMMEDIATELY		zFA_COMPRESS_FILE_IMMEDIATELY
#define FS_DATA_STREAM_IS_COMPRESSED		zFA_DATA_STREAM_IS_COMPRESSED
#define FS_DO_NOT_COMPRESS_FILE				zFA_DO_NOT_COMPRESS_FILE
#define FS_CANT_COMPRESS_DATA_STREAM		zFA_CANT_COMPRESS_DATA_STREAM
#define FS_ATTR_ARCHIVE						zFA_ATTR_ARCHIVE
#define FS_VOLATILE							zFA_VOLATILE
#define FS_OBJ_ARCHIVE_BIT					zFA_ATTR_ARCHIVE
#define FS_MAX_FULL_NAME					zMAX_FULL_NAME
#define FS_PURGE                        	zFA_IMMEDIATE_PURGE

/*No equivalent for FS_READ_AUDIT and FS_WRITE_AUDIT in NSS 3.0 as of now,  3June2002. 
  Values will mostly be same even if they are defined in NSS, in future */

#define FS_READ_AUDIT                                   0x00004000
#define FS_WRITE_AUDIT                                  0x00008000

/* Junctions */
#define SEARCH_LINK_AWARE          	 0x20  
#define SEARCH_OPERATE_ON_LINK       0x40

/** FS layer errors **/
#define FS_INTERNAL_ERROR						0xFFFFFFFF	/**	To indicate some internal errors **/
#define FS_PARAMETER_ERROR						0xFFFFFFFE	/**	To indicate invalid parameter error **/
#define FS_INVALID_HANDLE						0xFFFFFFFD
#define FS_NO_SPACE_RESTRICTION					0xFFFFFFFC
#define FS_NO_MEMORY							0xFFFFFFFB

/* FS layer defines */
#define FS_NSS_NO_RESTRICTION					0x7FFFFFFFFFFFFFFF
#define FS_CFS_NO_RESTRICTION					0x7FFFFFFF

/**	FS_OPEN_FLAGS definitions **/
#define FS_PRIMARY_DATA_STREAM					0x01		/**	For now leaving this as 0, see if this is defined by NSS already **/
#define FS_DATA_STREAM							0x02		/**	For now leaving this as 1, see if this is defined by NSS already **/
#define FS_EXTENDED_ATTRIBUTES					0x04
#define FS_MATCH_NON_SYSTEM						0x08
#define FS_MATCH_NON_HIDDEN						0x10

/**	FS_CREATE_MODE flag definitions **/
#define FS_CREATE_OPEN_IF_THERE					zCREATE_OPEN_IF_THERE
#define FS_CREATE_TRUNCATE_IF_THERE				zCREATE_TRUNCATE_IF_THERE
#define FS_CREATE_DELETE_IF_THERE				zCREATE_DELETE_IF_THERE

/**	FS_ACCESS_RIGHTS definitions **/
#define FS_READ_ACCESS							zRR_READ_ACCESS
#define FS_WRITE_ACCESS							zRR_WRITE_ACCESS 
#define FS_SCAN_ACCESS							zRR_SCAN_ACCESS
#define FS_DENY_READ_ACCESS						zRR_DENY_READ
#define FS_DENY_WRITE_ACCESS					zRR_DENY_WRITE
#define FS_DONT_UPDATE_ACCESS_TIME				zRR_DONT_UPDATE_ACCESS_TIME
#define FS_ENABLE_IO_ON_COMPRESSION				zRR_ENABLE_IO_ON_COMPRESSED_DATA_BIT
#define FS_LEAVE_FILE_COMPRESSED				zRR_LEAVE_FILE_COMPRESSED_BIT
#define zRR_ENABLE_IO_ON_COMPRESSED_DATA_BIT			0x00000100
#define zRR_LEAVE_FILE_COMPRESSED_BIT					0x00000200



/**	FS_GET_INFO_MASK definitions **/
#define FS_GET_STD_INFO							zGET_STD_INFO
#define FS_GET_VOLUME_INFO						zGET_VOLUME_INFO

/**	FS_MODIFY_INFO_MASK definitions **/
#define FS_MOD_FILE_ATTRIBUTES					zMOD_FILE_ATTRIBUTES
#define FS_MOD_TIMES							(zMOD_ACCESSED_TIME|zMOD_ARCHIVED_TIME|zMOD_CREATED_TIME|zMOD_MODIFIED_TIME|zMOD_METADATA_MODIFIED_TIME)
#define FS_MOD_ACCESSED_TIME					zMOD_ACCESSED_TIME
#define FS_MOD_CREATED_TIME						zMOD_CREATED_TIME
#define FS_MOD_ARCHIVED_TIME					zMOD_ARCHIVED_TIME
#define FS_MOD_MODIFIED_TIME					zMOD_MODIFIED_TIME
#define FS_MOD_ARCHIVER_ID 						zMOD_ARCHIVER_ID
#define FS_MOD_MODIFIER_ID 						zMOD_MODIFIER_ID
#define FS_MOD_OWNER_ID 						zMOD_OWNER_ID
#define FS_MOD_INHERITED_RIGHTS 				0x1
#define FS_MOD_MAXIMUM_SPACE					zMOD_DIR_QUOTA
#define FS_MOD_MAC_METADATA						zMOD_MAC_METADATA
#define FS_MOD_NFS_METADATA						zMOD_UNIX_METADATA
#define FS_MOD_EA_USERFLAGS						zMOD_EXTATTR_FLAGS 
#define FS_MOD_ALL								0xFFFFFFFF/* TBD This needs to be changed to OR all the bits*/


#define	FS_MAX_NUM_RESTORE_STREAMS				2 	/* Restore allocated only two streams(serial restore all the streams are closed imm. except the primary stream), Backup is made as dynamic that allocation is made for individual files proportional to the number of streams */
#define	FS_MAX_NUM_SCAN_STREAMS					1 	/* Scan only pne stream handle is enough for nss. TFS doesn't require any handle */

/* Additional VFS defines for use */
#define	FS_VFS_USE_HANDLE			0x00000001
#define	FS_VFS_USE_NAME				0x00000002

/* Additional File Types for VFS */
#define FS_VFS_CHARACTER_DEVICE		0x00000001
#define FS_VFS_BLOCK_DEVICE		0x00000002
#define FS_VFS_SOFT_LINK			0x00000004
#define FS_VFS_HARD_LINK			0x00000008
#define FS_VFS_FIFO					0x00000010
#define FS_VFS_SOCKET				0x00000020
#define FS_VFS_REGULAR				0x00000040
#define FS_VFS_DIRECTORY			0x00000080
#define FS_VFS_HL_REG				0x00000100
#define FS_VFS_HL_TO_SL			0x00000200

/* Additional defines for types */
#define DATASET_IS_SPARSE						0x08
#define DATASET_IS_SECONDARY_DATASTREAM			0x02
#define DATASET_IS_EXTENDED_ATTRIBUTE			0x03
#define DATASET_IS_MIGRATED						0x04
#define DATASET_IS_COMPRESSED					0x10
#define DATASET_IS_NOTREADABLE					0x20

#define DATASET_CLOSE_ALL_STREAM_HANDLES				0x0
#define DATASET_CLOSE_ALL_NON_PRI_STREAM_HANDLES		0x1
#define DATASET_CLOSE_ALL_EA_STREAM_HANDLES				0x2


#define DSET_PRIMARY_NAME_SPACE_MASK				0xF
#define DSET_ENGINE_SIDF_NAME_MATCH					0x10

/* Additional define for EA Lock */
#define TSA_EA_ACCESS_LOCK				"TSAFS: Legacy EA mutex"

/***********************************************************************************************************************/

#pragma pack(push,1)
/**	Type definitions for structures used to communicate with the file system layer **/

typedef  void*		FS_SEARCH_TYPE;
typedef	UINT32			FS_ACCESS_RIGHTS;
typedef QUAD			FS_INFORMATION_MASK;
typedef UINT32			FS_OPEN_FLAGS;
typedef UINT32			FS_SCAN_OPTION_TYPE;
typedef UINT32			FS_CREATE_MODE;
typedef QUAD			FS_MODIFY_INFO_MASK;
typedef QUAD			FS_ATTRIBUTES;

#ifdef N_PLAT_GNU

typedef struct _FS_VFS_HANDLE
{
	DIR 			*scanHandle;
	int		 		 fileDescriptor;
	unsigned char	*fullPath;
} FS_VFS_HANDLE;
#endif

typedef union _FS_HANDLE						/**	A union is needed to accomodate 32-bit file handle for legacy file system later. The memory occupied is still 64 bits **/
{
	Key_t	nssHandle;
	UINT32   cfsHandle;
#ifdef N_PLAT_GNU
	FS_VFS_HANDLE	vfsHandle;
#endif
}FS_HANDLE;

typedef struct
{
   UINT32 FileAttributes;
   UINT16 CreationDate;
   UINT16 CreationTime;
   UINT32 CreatorsID;
   UINT16 ModifiedDate;
   UINT16 ModifiedTime;
   UINT32 ModifiersID;
   UINT16 ArchivedDate;
   UINT16 ArchivedTime;
   UINT32 ArchiversID;
   UINT16 LastAccessedDate;
   UINT16 RestrictionGrantMask;
   UINT16 RestrictionRevokeMask;
   UINT32 MaximumSpace;
} NWMODIFY_INFO;

typedef struct _STREAM_HANDLE
{
	FS_HANDLE		handle;
	QUAD			size;
	QUAD			PhysicalSize;
	union{
	unsigned char 	eaName[FS_MAX_FULL_NAME];
	unsigned char 	SSIFileName[FS_MAX_FULL_NAME];
	unsigned char		streamName[FS_MAX_FULL_NAME];
	unsigned char		LinkName[FS_MAX_FULL_NAME];
	};
	LONG			eaAttrFlags;
	QUAD			UnCompressedSize;
	unsigned char	*streamMap;
	QUAD			streamMapSize;
	QUAD			blkSz;
	QUAD			dataSz;
	unsigned int	type;
	long			currentBit;
	QUAD			currentBlockRead;
	char			*ServerSpecificFileName;
	unsigned char	sparseEnd;
	QUAD			endOffset;
	BOOL			isCreated;	/* 08-18-2002 Indicates whether this data stream/ext attr has already been created. TBD */
	UINT32			overFlow; 	//08-18-2002
	UINT32          dataCRC;
	UINT32		iNodeNumber;
	
}STREAM_HANDLE;

typedef struct _FS_TRUSTEE_ID
{
	struct _FS_TRUSTEE_ID	*next;
	union {
		GUID_t					gid;
		UINT32					nid;
	} id;
	UINT32					rights;
	void						*name;
}FS_TRUSTEE_ID;

typedef struct _cfsStruct
{
	UINT32	   migrated;
    UINT32     clientConnID;
	BYTE 	   volumeNumber;		  
	UINT32 	   directoryNumber;		//During backup dos dir base is stored, and in restore the specific name space dir base is stored
    UINT32 	   entryNumber;
    UINT32     searchType;			 //file or dir
    UINT32	   nsDirNumbers[4];		//used only in restore: All name(dos, long, mac, nfs)space dir base is stored here
    UINT32	   cfsNameSpace;	
    char       cfsPattern[257];        //Max+length
	unicode	   *uniFullpath;	 // full path to dir or file, path to init the scan taken from JOB,always DOSnamespace
    unicode     *uniFileDirName;  //file/dir name
}CFS_STRUCT;

typedef struct _FS_SCAN_SEQUENCE
{
	FS_HANDLE				scanHandle;
	FS_SEARCH_TYPE			searchPattern;
	UINT32					matchCriteria;
	FS_INFORMATION_MASK		informationMask;
	CFS_STRUCT				CFSscanStruct;//this is used only for CFS file sys
}FS_SCAN_SEQUENCE;

typedef struct _FS_FILE_OR_DIR_INFO
{
	UINT32				 isDirectory;
	UINT32 				 isVolume;
	UINT32				 FileMatchCriteria;
	UINT32               DirMatchCriteria;
	UINT32				 numberOfTrustees;
	FS_TRUSTEE_ID		*trusteeList;
	UINT32				 sizeOfInformation;
	void				*information;
	unicode				*cfsNameSpaceInfo[LAST_NAMESPACE+1];
	unsigned char		*cfsAsciiNameSpaceInfo[LAST_NAMESPACE+1];
	UINT32 			fileType;
}FS_FILE_OR_DIR_INFO;


typedef struct _FS_FILE_OR_DIR_HANDLE
{
	unicode			*uniPath;
	unicode 		*fullPathNameSpaces[LAST_NAMESPACE+1];
	CFS_STRUCT	    cfsStruct;
	
	UINT32			nameSpace;
	UINT32			resPrimNameSpace;
	FS_HANDLE		parentHandle;
	UINT32			taskID;
	UINT32			isDirectory;

	UINT32			fileType;


	/** The remaining fields are filled in by FS_Open call and none of these are needed as input to scan calls **/
	/**	The following 2 fields will not be used for directories **/
	UINT32			streamCount;
	STREAM_HANDLE	*handleArray;
	UINT32			eaCount;
	UINT32			lastReadIndex;
	UINT32			lastOffset;
	TSA_MUTEX		eaMutex	;
	char			*SSIFile1;
	char			*SSIFile2;
	void			*migrationKey;
	UINT32			migrationKeysMatch;
	TSA_MUTEX      migratedFileMutex;	
#ifdef SMSDEBUG 
    void             *dSet ;
#endif
}FS_FILE_OR_DIR_HANDLE;


typedef struct _FS_READ_HANDLE
{
	FS_HANDLE		handle;
	QUAD			currentOffset;
	UINT32 			readFlag;
}FS_READ_HANDLE;

typedef struct _FS_WRITE_HANDLE
{
	FS_HANDLE		handle;
	QUAD			currentOffset;
	UINT32 			writeFlag;
}FS_WRITE_HANDLE;

struct  Quota_{						/* zGET_DIR_QUOTA */
	SQUAD	quota;				/* zMOD_DIR_QUOTA */
	SQUAD	usedAmount;
} ;

typedef struct  
{
    zInfoB_s             info ;

/*    struct Quota_       fillGap ;       //extra structure of zInfoB_s ; 1- byte alignment . So no probs
      UINT64             dirQuota    ;
*/
    UINT32             inheritedRights ;

}FS_NSS_INFO ;

#pragma pack(pop)

/***********************************************************************************************************************/

/**	Prototypes of functions defined in the File System Layer **/

CCODE FS_InitFileSystemInterfaceLayer(INT32 connectionID, FS_HANDLE *handle, UINT32 *taskID);
CCODE FS_DeInitFileSystemInterfaceLayer(FS_HANDLE handle, UINT32 taskID);

CCODE FS_InitScan(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence);
CCODE FS_ReInitScan(FS_SCAN_SEQUENCE *scanSequence);

CCODE FS_RestoreInitScan(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence);


CCODE FS_ScanNextDirectoryEntry(FS_SCAN_SEQUENCE *dirScanSequence, FS_FILE_OR_DIR_INFO *dirInfo);
CCODE FS_ScanNextFileEntry(FS_SCAN_SEQUENCE *fileScanSequence, FS_FILE_OR_DIR_INFO *fileInfo);

CCODE FS_EndScan(FS_SCAN_SEQUENCE *scanSequence);

CCODE FS_Open(FS_FILE_OR_DIR_HANDLE *handle, UINT32 openMode, FS_ACCESS_RIGHTS accessMode, FS_FILE_OR_DIR_INFO *info, BOOL noLock, UINT32 searchAttributes, FS_HL_TABLE **hlTable);
CCODE FS_Create(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_CREATE_MODE createMode, FS_ACCESS_RIGHTS accessRights, FS_ATTRIBUTES attributes, UINT32		streamType, FS_HANDLE *retHandle, unicode *dSetName, UINT32	rflags, BOOL isPathFullyQualified, void* buffer, FS_HL_TABLE **hlTable); 
CCODE FS_Close(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 closeFlag);
CCODE FS_OpenROFileForRestore(Key_t dirKey, UINT32 taskID, UINT32 nameSpace, punicode pathName, UINT16 fileActionFlags,
			UINT32 accessRights, Key_t *retKey); 

CCODE FS_SetCompressedFileSize(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD newSize); 
CCODE FS_SetFileSize(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD newSize, UINT32 handleIndex) ; 
CCODE FS_SetFileSizeForMigratedFile(QUAD handle, QUAD size);

CCODE FS_Read(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,  FS_READ_HANDLE *fileReadHandle, UINT32 bytesToRead, void *buffer, UINT32 *bytesRead, BOOL IsMigratedFile, UINT32 readIndex);
CCODE FS_Write(FS_FILE_OR_DIR_HANDLE *handle, QUAD startingOffset, UINT32 bytesToWrite, void *buffer, UINT32 *bytesWritten, UINT32 handleIndex ); //08-18-2002
CCODE FS_GetInheritedRightsMask(FS_FILE_OR_DIR_HANDLE *handle, UINT32 *inheritedRightsMask);
CCODE FS_ScanTrustees(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_FILE_OR_DIR_INFO *info);

CCODE FS_GetInfo(FS_FILE_OR_DIR_HANDLE *handle, UINT32 getInfoMask, UINT32 bufferSize, void *buffer);
CCODE FS_ModifyInfo(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD modifyInfoMask, UINT32 bufferSize, void *buffer);
CCODE FS_SetDirRestrictions(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, void *buffer);

CCODE FS_AddTrustee(FS_FILE_OR_DIR_HANDLE *   fileOrDirHandle,
                            UINT32                  objectID,
                            UINT16                  trusteeRights);

CCODE FS_DeleteTrustees(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_FILE_OR_DIR_INFO *info);
CCODE FS_SetInheritedRights(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, void* info);
CCODE FS_GetNameSpaceEntryName(UINT32 connID, FS_HANDLE handle, unicode *path, unsigned long  nameSpaceType, unsigned long  nameSpace, unicode **newName, UINT32 taskID, UINT32 *attributes);
CCODE FS_ConvertOpenMode(FS_FILE_OR_DIR_HANDLE *handle,UINT32 inputMode, UINT32 scanMode, BOOL COWStatus, UINT32 scanType, FS_FILE_OR_DIR_INFO *fileInfo, FS_ACCESS_RIGHTS *outputMode, BOOL *noLock, UINT32 *openMode, BOOL FileIsMigrated);
CCODE FS_GetCompressedFileSize(FS_FILE_OR_DIR_HANDLE *handle);
BOOL FS_IsCOWEnabled(char *volName);
BOOL 	FS_CheckCOWOnName(unicode *dataSetName);
void 	FS_SetzInfoVersion(char *volName);
int 	FS_CheckBackupBit(char *volName);
CCODE 	FS_SetSparseStatus(FS_FILE_OR_DIR_HANDLE *fsHandles);
CCODE 	FS_SetFileEntryInfo(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 setMask, UINT32 clientObjectID, UINT32 fileAttributes, UINT32 archivedDateAndTime);
void * 	FS_AllocateModifyInfo(void  *info, UINT32 *size)  ;
void  	FS_ResetModifyInfo(void  *info);
void * 	FS_AllocateMACInfo(FS_FILE_OR_DIR_INFO  *info);
void * 	FS_AllocateNFSInfo( FS_FILE_OR_DIR_INFO*) ;
CCODE 	FS_DeleteFile(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 deleteFlags); 
CCODE 	FS_DeleteSetPurge(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 attributes);
CCODE 	FS_GetDirSpaceRestriction(void *info, FS_FILE_OR_DIR_HANDLE dirHandle, SQUAD *dirSpaceRestriction);
unicode* FS_GetFileNameSpaceUNIName(void *information, UINT32 retNameSpace, unicode *cfsNameSpaceInfo[]);
unicode* FS_GetFilePrimaryUNIName(void *information, UINT32 retNameSpace, unicode *cfsNameSpaceInfo[]);
BOOL FS_FileExcludedByScanControl(NWSM_SCAN_CONTROL	*scanControl, void *Info);
#ifdef N_PLAT_NLM
CCODE 	FS_CreateMigratedDirectory(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,UINT32 smid,
   									UINT32	bindFlag,UINT32	*bindKeyNumber);
CCODE 	FS_VerifyMigrationKey(DMKEY *key);
CCODE 	FS_CreateMigratedFile(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,UINT32	primaryEntryIndex, 	UINT32 vol,	DMKEY *dmKey, 
							UINT32 setMask, FS_FILE_OR_DIR_INFO	*scanInfo, DMHS *sizes);
#endif
CCODE 	FS_OpenAndRename(FS_FILE_OR_DIR_HANDLE *fsHandle, BOOL isParent, 
							unicode *bytePath, UINT32 nameSpaceType, unicode *oldFullName, unicode *newDataSetName);
CCODE FS_SetPrimaryNameSpace(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 oldNameSpace, UINT32 newNameSpace);
CCODE FS_SetNameSpaceName(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT8 nameSpace, void *newDataSetName);
CCODE FS_WorkAroundGetDirHandle(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, Key_t *parentHandle);//NSS defect work around function
CCODE FS_SetReadyFileDirForRestore(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 isPathFullyQualified, unicode *dSetName, UINT32 rFlags, UINT32 createMode, FS_ACCESS_RIGHTS accessRights, FS_ATTRIBUTES attributes);
void 	MapModifyFlags(UINT32 modifyFlag, QUAD* retModifyFlag) ;

/***********************************************************************************************************************/

/**	Macros for accessing the information present in FS_FILE_INFO structure - TBDTBD: to implement a similar set for legacy file system **/

#define FS_GetFileDiskSpaceAllocated(fileInformation)					(((zInfo_s *)(fileInformation))-storageUsed.physicalEOF)

#define FS_GetFileStreamSize(fileInformation)				(((zInfo_s *)(fileInformation))->std.logicalEOF)
#define FS_GetFilePhysicalStreamSize(fileInformation)				(((zInfo_s *)(fileInformation))->storageUsed.physicalEOF)
#define FS_GetFileNumberOfDataStreams(fileInformation)				(((zInfo_s *)(fileInformation))->dataStream.count)
#define FS_GetFileAttributes(fileInformation)							(((zInfo_s *)(fileInformation))->std.fileAttributes)

#define FS_GetFileUTCCreationTime(fileInformation)					(((zInfo_s *)(fileInformation))->time.created)
#define FS_GetFileUTCArchivedTime(fileInformation)					(((zInfo_s *)(fileInformation))->time.archived)
#define FS_GetFileUTCModifiedTime(fileInformation)					(((zInfo_s *)(fileInformation))->time.modified)
#define FS_GetFileUTCAccessedTime(fileInformation)					(((zInfo_s *)(fileInformation))->time.accessed)

#define Get16Bit_TimeOnlyFromDOSTime(dosTime)						((UINT16) (dosTime & 0x0000FFFF))
#define Get16bit_DateOnlyFromDOSTime(dosTime)						((UINT16) ((dosTime >> 16) & 0x0000FFFF))

#define FS_GetFileVolumeID(fileInformation)							(((zInfo_s *)(fileInformation))->std.volumeID)
#define FS_GetZID(fileInformation)									(((zInfo_s *)(fileInformation))->std.zid)
#define FS_GetFileCreatorID(fileInformation)							(((zInfo_s *)(fileInformation))->id.owner)
#define FS_GetFileArchiverID(fileInformation)							(((zInfo_s *)(fileInformation))->id.archiver)
#define FS_GetFileModifierID(fileInformation)							(((zInfo_s *)(fileInformation))->id.modifier)

#define FS_GetFileNumberOfExtendedAttributes(fileInformation)			(((zInfo_s *)(fileInformation))->extAttr.count)
#define FS_GetFileExtendedAttributesSize(fileInformation)				(((zInfo_s *)(fileInformation))->extAttr.totalDataSize)
#define FS_GetExtendedAttributeUserFlags(fileInformation)				(((zInfo_s *)(fileInformation))->extAttrUserFlags)

//#define FS_GetFilePrimaryUNIName(fileInformation)					(zInfoGetFileName(fileInformation, (NINT)((zInfo_s *)(fileInformation))->primaryNameSpaceID))
//#define FS_GetFileNameSpaceUNIName(fileInformation, nameSpace)		(zInfoGetFileName(fileInformation, (NINT)nameSpace))

/***********************************************************************************************************************/

/**	Macros for accessing the information present in FS_FILE_INFO structure - TBDTBD: to implement a similar set for legacy file system **/
#define FS_SetFileAttributes(fileInformation, attributes)					(dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zInfo_s *)(fileInformation))->std.fileAttributes = attributes):(((NWMODIFY_INFO *)(fileInformation))->FileAttributes=attributes))
//#define FS_NegateFileAttribute(attribute, attributeFlag)         	(attribute &= ~attributeFlag) 

//#define FS_NegateFileAttribute(attribute, attributeFlag)         	(attribute &= ~attributeFlag) 
#define FS_NegateFileAttribute(attribute, nssFlag, cfsFlag)			(dSet->filesysType==NETWAREPSSFILESYSTEM ? (attribute &= ~nssFlag) : (attribute &= ~cfsFlag))
#define FS_SetAttribute(attribute, nssFlag, cfsFlag)				(dSet->filesysType==NETWAREPSSFILESYSTEM ? (attribute |=  nssFlag) : (attribute |= cfsFlag))


#define FS_IS_SetInheritedRightsMask(info)                       		(((FS_NSS_INFO *)info)->inheritedRights)

#define FS_SetFileUTCCreatedTime(fileInformation, utcTime)				(((zInfo_s *)(fileInformation))->time.created = utcTime)
#define FS_SetFileUTCArchivedTime(fileInformation, utcTime)				(((zInfo_s *)(fileInformation))->time.archived = utcTime)
#define FS_SetFileUTCModifiedTime(fileInformation, utcTime)				(((zInfo_s *)(fileInformation))->time.modified = utcTime)
#define FS_SetFileUTCAccessedTime(fileInformation, utcTime)				(((zInfo_s *)(fileInformation))->time.accessed = utcTime)


#define FS_IS_SetFileDOSCreatedTimeMask(mask)                           (mask & FS_MOD_CREATED_TIME)
#define FS_IS_SetFileDOSArchivedTimeMask(mask)                          (mask & FS_MOD_ARCHIVED_TIME)

#define FS_IS_SetFileOwnerIDMask(mask)                                  (mask & FS_MOD_OWNER_ID) 
#define FS_IS_SetFileArchiverIDMask(mask)                                 (mask & FS_MOD_ARCHIVER_ID) 
#define FS_SetFileOwnerIDMask(mask)                 					(mask |= FS_MOD_OWNER_ID) 
#define FS_SetFileArchiverIDMask(mask)           						(mask |= FS_MOD_ARCHIVER_ID) 
#define FS_SetFileDOSCreatedTimeMask(mask)                              (mask |=FS_MOD_CREATED_TIME)
#define FS_SetFileDOSArchivedTimeMask(mask)                             (mask |=FS_MOD_ARCHIVED_TIME)
#define FS_SetEAUserFlags(fileInformation, flags)						(((zInfo_s *)fileInformation)->extAttrUserFlags = flags) //08-18-2002
//#define FS_SET_VERSION(information, newVersion)                         (((zInfo_s *)information)->infoVersion = newVersion);

#define FS_SET_VERSION(information, newVersion)                    (dSet->filesysType==NETWAREPSSFILESYSTEM ?   (((zInfo_s *)information)->infoVersion = newVersion): 0);



#define FS_SetMACFinderInfo(info, data, size)  						(dSet->filesysType==NETWAREPSSFILESYSTEM ? (memmove(((zMacInfo_s*)info)->finderInfo, data, min(sizeof(((zMacInfo_s*)info)->finderInfo), size))) :\
																			memmove(((MAC_NAME_SPACE_INFO *)macNameSpaceInfo)->finderInfo, dataBuffer, min(sizeof(((MAC_NAME_SPACE_INFO *)macNameSpaceInfo)->finderInfo), size)))
#define FS_SetMACproDOSInfo(info, data, size)               		(dSet->filesysType==NETWAREPSSFILESYSTEM ? (memmove(((zMacInfo_s*)info)->proDOSInfo, data, min(sizeof(((zMacInfo_s*)info)->proDOSInfo), size))) :\
																			memmove(((MAC_NAME_SPACE_INFO *)macNameSpaceInfo)->proDOSInfo, dataBuffer, min(sizeof(((MAC_NAME_SPACE_INFO *)macNameSpaceInfo)->proDOSInfo), size)))
#define FS_SetMACdirRightsMask(info, data)                       	(dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zMacInfo_s*)info)->dirRightsMask = *(UINT32 *)dataBuffer) : (((MAC_NAME_SPACE_INFO *)macNameSpaceInfo)->dirRightsMask = *(UINT32 *)dataBuffer))
#define FS_SetModifyFlag_MacMetaData(mask)                     		(dSet->filesysType==NETWAREPSSFILESYSTEM ? (mask |= FS_MOD_MAC_METADATA) : (infoMask =MAC_FIXED_INFO_MASK))


#define FS_SetNFSAccessMode(nfsNameSpaceInfo, dataBuffer)     		(dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zUnixInfo_s *)nfsNameSpaceInfo)->fMode = *(UINT32 *)dataBuffer ) :(((SIDF_NFS_INFO*) nfsNameSpaceInfo)->fileAccessMode = *(UINT32 *)dataBuffer))
#define FS_SetNFSGroupOwnerId(nfsNameSpaceInfo, dataBuffer)    		(dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zUnixInfo_s *)nfsNameSpaceInfo)->nfsGID = *(UINT32 *)dataBuffer) : (((SIDF_NFS_INFO*)nfsNameSpaceInfo)->groupOwnerID = *(UINT32 *)dataBuffer))
#define FS_SetNFSRDevice(nfsNameSpaceInfo, dataBuffer)           	(dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zUnixInfo_s *)nfsNameSpaceInfo)->rDev = *(UINT32 *)dataBuffer) :(((SIDF_NFS_INFO*)nfsNameSpaceInfo)->RDevice = *(UINT32 *)dataBuffer))
#define FS_SetNFSFirstCreatorFlag(nfsNameSpaceInfo, dataBuffer)     (dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zUnixInfo_s *)nfsNameSpaceInfo)->firstCreated = *dataBuffer) :( ((SIDF_NFS_INFO*) nfsNameSpaceInfo)->firstCreatedFlag = *dataBuffer))
#define FS_SetNFSAcsFlags(nfsNameSpaceInfo, dataBuffer)            (dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zUnixInfo_s *)nfsNameSpaceInfo)->acsFlags = *dataBuffer) : (((SIDF_NFS_INFO*)nfsNameSpaceInfo)->acSFlagsBit = *dataBuffer))
#define FS_SetNFSUserId(nfsNameSpaceInfo, dataBuffer)              (dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zUnixInfo_s *)nfsNameSpaceInfo)->nfsUID = *(UINT32 *)dataBuffer) :(((SIDF_NFS_INFO*) nfsNameSpaceInfo)->userIDBit = *(UINT32 *)dataBuffer))
#define FS_SetNFSMyFlags(nfsNameSpaceInfo, dataBuffer)             (dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zUnixInfo_s *)nfsNameSpaceInfo)->myFlags = *(UINT32 *)dataBuffer) : (((SIDF_NFS_INFO*) nfsNameSpaceInfo)->myFlagsBit = *(UINT32 *)dataBuffer))
#define FS_SetNFSNwEveryOne(nfsNameSpaceInfo, dataBuffer)         (dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zUnixInfo_s *)nfsNameSpaceInfo)->nwEveryone = *(UINT32 *)dataBuffer) :(((SIDF_NFS_INFO*) nfsNameSpaceInfo)->nwEveryone = *(UINT32 *)dataBuffer))
#define FS_SetNFSNwUserRights(nfsNameSpaceInfo, dataBuffer)        (dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zUnixInfo_s *)nfsNameSpaceInfo)->nwUIDRights = *(UINT32 *)dataBuffer) : ( ((SIDF_NFS_INFO*) nfsNameSpaceInfo)->nwUserRights = *(UINT32 *)dataBuffer))
#define FS_SetNFSGroupRights(nfsNameSpaceInfo, dataBuffer)          (dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zUnixInfo_s *)nfsNameSpaceInfo)->nwGIDRights = *(UINT32 *)dataBuffer) :(((SIDF_NFS_INFO*) nfsNameSpaceInfo)->nwGroupRights = *(UINT32 *)dataBuffer))
#define FS_SetNFSEveryoneRights(nfsNameSpaceInfo, dataBuffer)       (dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zUnixInfo_s *)nfsNameSpaceInfo)->nwEveryoneRights = *(UINT32 *)dataBuffer) :(((SIDF_NFS_INFO*) nfsNameSpaceInfo)->nwEveryoneRights = *(UINT32 *)dataBuffer))
#define FS_SetModifyFlag_NfsMetaData(mask)                          (dSet->filesysType==NETWAREPSSFILESYSTEM ? (mask |= FS_MOD_NFS_METADATA) :(infoMask=NFS_FIXED_INFO_MASK))
#define FS_SetNFSGroupId(nfsNameSpaceInfo, id)                      (dSet->filesysType==NETWAREPSSFILESYSTEM ? (((zUnixInfo_s *)nfsNameSpaceInfo)->nwGID = NWSwap32(id)) :(((SIDF_NFS_INFO*) nfsNameSpaceInfo)->nwGroupID = id))
#define FS_SetNFSOwnerId(nfsNameSpaceInfo, id)                      (dSet->filesysType==NETWAREPSSFILESYSTEM ?	(((zUnixInfo_s *)nfsNameSpaceInfo)->nwUID = NWSwap32(id)) :(((SIDF_NFS_INFO*) nfsNameSpaceInfo)->nwOwnerID =id))
#define FS_SetNFSNumberOfLinks(nfsNameSpaceInfo, linkCount)                      (dSet->filesysType==NETWAREPSSFILESYSTEM ?1:(((SIDF_NFS_INFO*) nfsNameSpaceInfo)->numberOfLinks = linkCount))
#define FS_SetVFSNumberOfLinks(fileInformation, dataBuffer)			(dSet->filesysType==NETWAREPSSFILESYSTEM ?(((zInfo_s *)fileInformation)->count.hardLink = *(UINT32 *)dataBuffer) :0)
#define FS_SetFileDOSAccessedTime(fileInformation, dosTime, mask)     (dSet->filesysType==NETWAREPSSFILESYSTEM ? (FS_SetFileUTCAccessedTime(fileInformation, xDOS2utcTime(dosTime)), mask |=FS_MOD_ACCESSED_TIME) :(((NWMODIFY_INFO *)(info))->LastAccessedDate=dosDateTime>>16, mask |=FS_CFS_MODIFY_ACCESSED_DATE))
#define FS_SetFileDOSArchivedTime(fileInformation, dosTime, mask)     (dSet->filesysType==NETWAREPSSFILESYSTEM ? (FS_SetFileUTCArchivedTime(fileInformation, xDOS2utcTime(dosTime)), mask |=FS_MOD_ARCHIVED_TIME) :(((NWMODIFY_INFO *)(info))->ArchivedDate=dosDateTime>>16,((NWMODIFY_INFO *)(info))->ArchivedTime=(UINT16)dosDateTime, mask |=(FS_CFS_MODIFY_ARCHIVE_DATE |FS_CFS_MODIFY_ARCHIVE_TIME)))
#define FS_SetFileDOSCreatedTime(fileInformation, dosTime, mask)      (dSet->filesysType==NETWAREPSSFILESYSTEM ? (FS_SetFileUTCCreatedTime(fileInformation, xDOS2utcTime(dosTime)), mask |=FS_MOD_CREATED_TIME) :(((NWMODIFY_INFO *)(info))->CreationDate=dosDateTime>>16,((NWMODIFY_INFO *)(info))->CreationTime=(UINT16)dosDateTime, mask |=(FS_CFS_MODIFY_CREATION_DATE | FS_CFS_MODIFY_CREATION_TIME)))
#define FS_SetFileDOSModifiedTime(fileInformation, dosTime, mask)      (dSet->filesysType==NETWAREPSSFILESYSTEM ? (FS_SetFileUTCModifiedTime(fileInformation, xDOS2utcTime(dosTime)), mask |=FS_MOD_MODIFIED_TIME) :(((NWMODIFY_INFO *)(info))->ModifiedDate=dosDateTime>>16,((NWMODIFY_INFO *)(info))->ModifiedTime=(UINT16)dosDateTime, mask |=(FS_CFS_MODIFY_MODIFY_DATE |FS_CFS_MODIFY_MODIFY_TIME)))
#define FS_SetFileArchiverID(fileInformation, id, mask)        		   (dSet->filesysType==NETWAREPSSFILESYSTEM ? (xIdToGuid(id, & ((zInfo_s *)fileInformation)->id.archiver), mask |= FS_MOD_ARCHIVER_ID) : (((NWMODIFY_INFO *)(info))->ArchiversID=id, mask |=FS_CFS_MODIFY_ARCHIVER_ID))
#define FS_SetFileModifierID(fileInformation, id, mask)                   (dSet->filesysType==NETWAREPSSFILESYSTEM ?(xIdToGuid(id, & ((zInfo_s *)fileInformation)->id.modifier), mask |= FS_MOD_MODIFIER_ID) :(((NWMODIFY_INFO *)(info))->ModifiersID=id, mask |=FS_CFS_MODIFY_MODIFIER_ID))
#define FS_SetFileOwnerID(fileInformation, id, mask)                     (dSet->filesysType==NETWAREPSSFILESYSTEM ? (xIdToGuid(id, & ((zInfo_s *)fileInformation)->id.owner), mask |= FS_MOD_OWNER_ID) :(((NWMODIFY_INFO*)(info))->CreatorsID=id, mask |= FS_CFS_MODIFY_CREATOR_ID))
#define FS_SetInheritedRightsMask(info,data, size, mask)        	  (dSet->filesysType==NETWAREPSSFILESYSTEM ? (memcpy(&((FS_NSS_INFO *)info)->inheritedRights, data, min(sizeof(((FS_NSS_INFO *)info)->inheritedRights), size )), mask |=FS_MOD_INHERITED_RIGHTS) : \
																				(memcpy(&((NWMODIFY_INFO *)info)->RestrictionGrantMask, data, min(sizeof(((NWMODIFY_INFO *)info)->RestrictionGrantMask), size )),((NWMODIFY_INFO *)info)->RestrictionRevokeMask= 0xFFFF, mask |=FS_CFS_MODIFY_RIGHTS_MASK))
#define FS_SetMaximumSpace(info,data, size, mask)                      (dSet->filesysType==NETWAREPSSFILESYSTEM ? (memcpy(&((FS_NSS_INFO *)info)->info.dirQuota.quota, data, min(sizeof(((FS_NSS_INFO *)info)->info.dirQuota.quota), size )), mask |=FS_MOD_MAXIMUM_SPACE) : \
																				(memcpy(&((NWMODIFY_INFO *)info)->MaximumSpace, data, min(sizeof(((NWMODIFY_INFO *)info)->MaximumSpace), size )), mask |=FS_CFS_MODIFY_MAXIMUM_SPACE))		
#define FS_ReSetMaximumSpace(info, data, mask)                      (dSet->filesysType==NETWAREPSSFILESYSTEM ? (((FS_NSS_INFO *)info)->info.dirQuota.quota=0, mask |=FS_MOD_MAXIMUM_SPACE) : (((NWMODIFY_INFO *)info)->MaximumSpace=0, mask |=FS_CFS_MODIFY_MAXIMUM_SPACE))

#define FS_SetModifyMask_FileAttributes(mask)                           (dSet->filesysType==NETWAREPSSFILESYSTEM ? (mask |=FS_MOD_FILE_ATTRIBUTES) :(mask |=FS_CFS_MODIFY_ATTRIBUTES ))
#define FS_SetFileAttribute(fileInformation, attribute,attri)			(dSet->filesysType==NETWAREPSSFILESYSTEM ?(((zInfo_s *)fileInformation)->std.fileAttributes |= attribute) :(((NWMODIFY_INFO*)fileInformation)->FileAttributes |= attri))
#define FS_ClearFileAttribute(fileInformation, attribute,attri)			(dSet->filesysType==NETWAREPSSFILESYSTEM ?(((zInfo_s *)fileInformation)->std.fileAttributes &= ~attribute) :(((NWMODIFY_INFO*)fileInformation)->FileAttributes &= ~attri))

#define FS_IS_SetFileDOSAccessedTimeMask(mask)                       (dSet->filesysType==NETWAREPSSFILESYSTEM ? (mask & FS_MOD_ACCESSED_TIME)	: (mask &  FS_CFS_MODIFY_ACCESSED_DATE))
#define FS_SetFileDOSAccessedTimeMask(mask)                     		(dSet->filesysType==NETWAREPSSFILESYSTEM ? (mask |=FS_MOD_ACCESSED_TIME) :(mask |= FS_CFS_MODIFY_ACCESSED_DATE))
#define FS_IS_SetFileDOSModifiedTimeMask(mask)                    		(dSet->filesysType==NETWAREPSSFILESYSTEM ? (mask & FS_MOD_MODIFIED_TIME) :(mask & FS_CFS_MODIFY_MODIFY_TIME))
#define FS_SetFileDOSModifiedTimeMask(mask)                         	(dSet->filesysType==NETWAREPSSFILESYSTEM ? (mask |=FS_MOD_MODIFIED_TIME) : (mask |=FS_CFS_MODIFY_MODIFY_TIME))
#define FS_IS_SetFileModifierIDMask(mask)                             	(dSet->filesysType==NETWAREPSSFILESYSTEM ? (mask & FS_MOD_MODIFIER_ID)  : (mask & FS_CFS_MODIFY_MODIFIER_ID   ) )
#define FS_SetFileModifierIDMask(mask)            						(dSet->filesysType==NETWAREPSSFILESYSTEM ? (mask |= FS_MOD_MODIFIER_ID) :(mask |= FS_CFS_MODIFY_MODIFIER_ID ))


#define FS_GetFileTotalDataStreamSize(fileInformation)		(dSet->filesysType==NETWAREPSSFILESYSTEM ?(((zInfo_s *)(fileInformation))->dataStream.totalDataSize):(((NetwareInfo *)(fileInformation))->TotalDataStreamsSize))

#define FS_GetFilePrimaryNameSpace(fileInformation)			(dSet->filesysType==NETWAREPSSFILESYSTEM ?(((zInfo_s *)(fileInformation))->primaryNameSpaceID):(((NetwareInfo *)(fileInformation))->CreatorNameSpaceNumber))

#define FS_GetFileDOSModifiedTime(fileInformation)			(dSet->filesysType==NETWAREPSSFILESYSTEM ?((DOSTime_t)xUTC2dosTime((Time_t)FS_GetFileUTCModifiedTime(fileInformation))) :((((NetwareInfo *)(fileInformation))->ModifiedDate <<16) | (((NetwareInfo *)(fileInformation))->ModifiedTime)))

#define FS_GetFileDOSAccessedTime(fileInformation)			(dSet->filesysType==NETWAREPSSFILESYSTEM ?((DOSTime_t)xUTC2dosTime((Time_t)FS_GetFileUTCAccessedTime(fileInformation))) :(((NetwareInfo *)(fileInformation))->LastAccessedDate <<16))
#define FS_GetFileDOSCreationTime(fileInformation)	 		(dSet->filesysType==NETWAREPSSFILESYSTEM ?((DOSTime_t)xUTC2dosTime((Time_t)FS_GetFileUTCCreationTime(fileInformation))) :((((NetwareInfo *)(fileInformation))->CreationDate <<16) | (((NetwareInfo *)(fileInformation))->CreationTime)))
#define FS_GetFileDOSArchivedTime(fileInformation)	 		(dSet->filesysType==NETWAREPSSFILESYSTEM ?((DOSTime_t)xUTC2dosTime((Time_t)FS_GetFileUTCArchivedTime(fileInformation))) :((((NetwareInfo *)(fileInformation))->ArchivedDate <<16) | (((NetwareInfo *)(fileInformation))->ArchivedTime)))

#define FS_RGetAttributes(fileInformation)					(dSet->filesysType==NETWAREPSSFILESYSTEM ?(((zInfo_s *)(fileInformation))->std.fileAttributes) :(((NWMODIFY_INFO *)(fileInformation))->FileAttributes))
#define FS_BGetAttributes(fileInformation)					(dSet->filesysType==NETWAREPSSFILESYSTEM ?(((zInfo_s *)(fileInformation))->std.fileAttributes) :(((NetwareInfo *)(fileInformation))->Attributes))

#ifdef N_PLAT_NLM
/***********************************************************************************************************************
								LEGACY File System Interface Functions
***********************************************************************************************************************/


CCODE FS_CFS_InitScan(
FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence);
CCODE FS_CFS_ReInitScan(FS_SCAN_SEQUENCE *scanSequence);
CCODE FS_CFS_EndScan(FS_SCAN_SEQUENCE *scanSequence);


CCODE FS_CFS_ScanNextFileEntry(FS_SCAN_SEQUENCE *fileScanSequence, FS_FILE_OR_DIR_INFO *fileInfo);
CCODE FS_CFS_ScanNextDirectoryEntry(FS_SCAN_SEQUENCE *dirScanSequence, FS_FILE_OR_DIR_INFO *dirInfo);

//Restore scan file and directory
CCODE FS_CFS_RScanNextFileEntry(FS_SCAN_SEQUENCE *fileScanSequence, FS_FILE_OR_DIR_INFO *fileInfo, UINT32 nameSpace);
CCODE FS_CFS_RScanNextDirectoryEntry(FS_SCAN_SEQUENCE *dirScanSequence, FS_FILE_OR_DIR_INFO *dirInfo, UINT32 nameSpace);

CCODE FS_CFS_Open(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_ACCESS_RIGHTS accessRights, UINT32 openModeFlags, FS_FILE_OR_DIR_INFO *info, BOOL noLock, UINT32 searchAttributes, FS_HL_TABLE ** hlTable);

CCODE FS_CFS_InitFileSystemInterfaceLayer(INT32 connectionID, FS_HANDLE *handle);
CCODE FS_CFS_DeInitFileSystemInterfaceLayer(FS_HANDLE handle);

CCODE FS_CFS_GetDirSpaceRestriction(void *info, FS_FILE_OR_DIR_HANDLE dirHandle, SQUAD *dirSpaceRestriction);
CCODE FS_CFS_GetPathBaseEntry(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 *dosDirBase, UINT32 *nameSpaceDirBase);
CCODE FS_CFS_Close(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 closeFlag);
CCODE FS_CFS_GetNameSpaceEntryName(UINT32 clientConnID, FS_HANDLE handle, unicode *path, unsigned long  nameSpaceType, unsigned long  nameSpace, unicode **newName, UINT32 taskID, UINT32 *attributes);
unicode* FS_CFS_GetFileNameSpaceUNIName(void *information, UINT32 retNameSpace, unicode *cfsNameSpaceInfo[]);
unicode* FS_CFS_GetFilePrimaryUNIName(void *information, UINT32 retNameSpace, unicode *cfsNameSpaceInfo[]);
CCODE FS_CFS_GetFileNameSpaceName(CFS_STRUCT * cfsStruct, char *path, UINT32 inNameSpace, UINT32 outnameSpace, UINT32 searchAttributes, char *retFileDirName);
CCODE FS_CFS_SetSparseStatus(FS_FILE_OR_DIR_HANDLE *fsHandles);
CCODE FS_CFS_ConvertOpenMode(FS_FILE_OR_DIR_HANDLE *handle,UINT32 inputMode, UINT32 scanMode, BOOL COWEnabled, 
								UINT32 scanType,FS_FILE_OR_DIR_INFO *fileInfo, 
								FS_ACCESS_RIGHTS *outputMode, BOOL *noLock, 
							 UINT32 *openMode,BOOL FileIsMigrated);
CCODE FS_CFS_GetInfo(FS_FILE_OR_DIR_HANDLE *handle, UINT32 getInfoMask, UINT32 bufferSize, void *buffer);
CCODE FS_CFS_SetFileEntryInfo(FS_FILE_OR_DIR_HANDLE  *fileOrDirHandle, UINT32 setMask, UINT32 clientObjectID, UINT32 fileAttributes, UINT32 archivedDateAndTime);
CCODE FS_CFS_ScanTrustees(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_FILE_OR_DIR_INFO *info);
CCODE FS_CFS_AddTrustee(FS_FILE_OR_DIR_HANDLE *   dirEntrySpec,UINT32 objectID,UINT16  trusteeRights);
CCODE FS_CFS_DeleteTrustees(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_FILE_OR_DIR_INFO *info);
void * FS_CFS_AllocateModifyInfo(void  *info, UINT32 *size);
BOOL  FS_CFS_CheckCOWOnName(unicode *dataSetName);
CCODE FS_CFS_CreateAndOpen( FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32			openMode,
						 UINT32			accessRights, QUAD	receivedCreateAttributes,  UINT32 streamType,
						 FS_HANDLE 		*retHandle, unicode *dSetName, UINT32 	rflags,
						 BOOL 		isPathFullyQualified,
						 void* buffer, void** hlTable); 
CCODE FS_CFS_Read(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_READ_HANDLE *fileReadHandle, UINT32 bytesToRead, void *buffer, UINT32 *bytesRead,UINT32 readIndex);
CCODE FS_CFS_Write(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD startingOffset, UINT32 bytesToWrite, void *buffer, UINT32 *bytesWritten, UINT32 handleIndex); 
CCODE FS_CFS_ModifyInfo(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT64 modifyInfoMask, UINT32 bufferSize, void *buffer); 
CCODE FS_CFS_SetPrimaryNameSpace(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 oldNameSpace, UINT32 newNameSpace);

//CCODE FS_CFS_SIDF_ConvertNFSInfo(void *information, FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, SIDF_NFS_INFO *nfsInfo);
//CCODE FS_CFS_SIDF_ConvertMACInfo(void *information, FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, SIDF_MAC_INFO  *macInfo);
CCODE FS_CFS_SetSpecificCharacteristics(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, void  *buffer, UINT32 size, UINT32 nameSpace, UINT32 infoMask);
void FS_CFS_SetMigratedStatus(FS_FILE_OR_DIR_HANDLE *fsHandles, FS_FILE_OR_DIR_INFO scanInfo);

CCODE FS_CFS_SetCompressedFileSize(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD newSize);
CCODE FS_CFS_SetFileSize(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD newSize, UINT32 handleIndex); 

CCODE FS_CFS_GetCompressedFileSize(FS_FILE_OR_DIR_HANDLE *handle);
void FS_CFS_MapOpenCreateFlags(UINT32 openMode, UINT32 *openCreateFlags);
void * FS_CFS_AllocateMACInfo(void  **info, UINT32 *size);
void * FS_CFS_AllocateNFSInfo(void **info, UINT32 *size);
void FS_CFS_SetEAUserFlags(NetwareInfo *fileInformation, UINT32 flags);

BOOL FS_CFS_FileExcludedByScanControl(NWSM_SCAN_CONTROL	*scanControl, void *Info);
CCODE FS_CFS_DeleteFile(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 deleteFlags);
CCODE FS_CFS_CreateMigratedDirectory(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,UINT32	smid,
										   	UINT32 bindFlag, UINT32	*bindKeyNumber
											);
CCODE FS_CFS_GetFullPathName(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, char **path);
CCODE FS_CFS_VerifyMigrationKey(DMKEY *key);
CCODE FS_CFS_CreateMigratedFile(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32	primaryEntryIndex, UINT32		vol,
							DMKEY	*dmKey, UINT32	setMask, FS_FILE_OR_DIR_INFO *scanInfo, DMHS	*sizes);

CCODE FS_CFS_GetDirNumber( FS_FILE_OR_DIR_HANDLE *fileOrDirHandle); 
CCODE FS_CFS_GetPath(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, BOOL isPathFullyQualified, char **path, UINT32 *Length);
CCODE FS_CFS_BGetDirBase(CFS_STRUCT *CFSscanStruct , UINT32 *retDirectoryNumber);
CCODE FS_CFS_GetStreamSizes(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_FILE_OR_DIR_INFO *info);
CCODE FS_CFS_OpenAndRename(FS_FILE_OR_DIR_HANDLE *fsHandle, BOOL isParent, 
							unicode *bytePath, UINT32 nameSpaceType, unicode *oldFullName, unicode *newDataSetName);
CCODE FS_CFS_GetNameSpaceHugeInfo(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, char *hugeData, UINT32 *hugeDataSize, UINT32 nameSpace);

CCODE FS_CFS_SetNameSpaceHugeInfo(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, char *hugeData, UINT32 nameSpace);
CCODE FS_CFS_SetNameSpaceName(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT8 nameSpace, void *newDataSetName);
CCODE FS_CFS_PopulateSingleFile(unicode *currentDirFullName, FS_FILE_OR_DIR_HANDLE *fileOrDirHandle);
CCODE FS_SecStreamTrailerDataRecovery(char *dataPtr, QUAD bytesToProcess, QUAD *sidfSize, char *underFlowBuffer, UINT32 *underFlowBufferLen);
CCODE FS_CFS_RInitScan(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence, char *path);
void FS_CFS_MapModifyFlags(UINT32 modifyFlag, QUAD* retModifyFlag);
void*  FS_CFS_GenericGetFileNameSpaceName(void *information, UINT32 retNameSpace, void *cfsNameSpaceInfo[]);
CCODE FS_CFS_SetReadyFileDirForRestore(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 isPathFullyQualified, void *dSetName, UINT32 rFlags, UINT32 createMode, UINT32 accessRights, QUAD attributes);

/***********************************************************************************************************************
								LEGACY File System Convert Macro's
***********************************************************************************************************************/

#define FS_CFS_GetFileAttributes(fileInformation)			(((NetwareInfo *)(fileInformation))->Attributes)
#define FS_CFS_GetFileStreamSize(fileInformation)			(((NetwareInfo *)(fileInformation))->DataStreamSize)
#define FS_CFS_GetFileArchiversID(fileInformation)			(((NetwareInfo *)(fileInformation))->ArchiversID)
#define FS_CFS_GetFileIRFMask(fileInformation)				(((NetwareInfo *)(fileInformation))->MaximumRightsMask)
#define FS_CFS_GetFileCreatorID(fileInformation)			(((NetwareInfo *)(fileInformation))->CreatorsID)
#define FS_CFS_GetFileArchiverID(fileInformation)			(((NetwareInfo *)(fileInformation))->ArchiversID)
#define FS_CFS_GetFileModifierID(fileInformation)			(((NetwareInfo *)(fileInformation))->ModifiersID)
#define FS_CFS_GetFileNumberOfDataStreams(fileInformation)               (((NetwareInfo *)(fileInformation))->NumberOfDataStreams-1)//excluding the primary
#define FS_CFS_GetFileNumberOfExtendedAttributes(fileInformation)         (((NetwareInfo *)(fileInformation))->ExtendedAttributesCount)
#define FS_CFS_SetFileAttributes(fileInformation, attributes)	(((NetwareInfo *)(fileInformation))->Attributes = attributes)
#define FS_CFS_NegateFileAttribute(attribute, attributeFlag)	(attribute &= ~attributeFlag) 

#elif N_PLAT_GNU

CCODE 	FS_VFS_InitScan(FS_FILE_OR_DIR_HANDLE * dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE * scanSequence);	
CCODE 	FS_VFS_EndScan(FS_SCAN_SEQUENCE * scanSequence);
CCODE 	FS_VFS_ReInitScan(FS_SCAN_SEQUENCE * scanSequence);
CCODE 	FS_VFS_Close(FS_FILE_OR_DIR_HANDLE * fileOrDirHandle, UINT32 closeFlag);
CCODE 	FS_VFS_SetSparseStatus(FS_FILE_OR_DIR_HANDLE * fsHandles);
CCODE 	FS_VFS_ConvertOpenMode(FS_FILE_OR_DIR_HANDLE * handle, UINT32 inputMode, UINT32 scanMode, BOOL COWEnabled, UINT32 scanType, FS_FILE_OR_DIR_INFO * fileInfo, FS_ACCESS_RIGHTS * outputMode, BOOL * noLock, UINT32 * openMode, BOOL FileIsMigrated);
CCODE 	FS_VFS_Open(FS_FILE_OR_DIR_HANDLE * fileOrDirHandle, FS_ACCESS_RIGHTS accessRights, UINT32 openModeFlags, FS_FILE_OR_DIR_INFO * info, BOOL noLock, UINT32 searchAttributes, FS_HL_TABLE **hlTable);
BOOL 	FS_VFS_CheckCOWOnName(unicode * dataSetName);
CCODE 	FS_VFS_ScanNextFileEntry(FS_SCAN_SEQUENCE * fileScanSequence, FS_FILE_OR_DIR_INFO * fileInfo );
CCODE 	FS_VFS_ScanNextDirectoryEntry(FS_SCAN_SEQUENCE * dirScanSequence, FS_FILE_OR_DIR_INFO * dirInfo );
unicode *FS_VFS_GetFileNameSpaceUNIName(void * information, UINT32 retNameSpace, unicode * cfsNameSpaceInfo [ ]);
CCODE 	FS_VFS_GetDirSpaceRestriction(void * info, FS_FILE_OR_DIR_HANDLE dirHandle, SQUAD * dirSpaceRestriction);
CCODE 	FS_VFS_ScanTrustees(FS_FILE_OR_DIR_HANDLE * fileOrDirHandle, FS_FILE_OR_DIR_INFO * info);
unicode *FS_VFS_GetFilePrimaryUNIName(void * information, UINT32 retNameSpace, unicode * cfsNameSpaceInfo [ ]);
CCODE 	FS_VFS_GetNameSpaceEntryName(UINT32 connID, FS_HANDLE handle, unicode * path, unsigned long nameSpaceType, unsigned long nameSpace, unicode * * newName, UINT32 taskID, UINT32 * attributes);
CCODE 	FS_VFS_GetCompressedFileSize(FS_FILE_OR_DIR_HANDLE * handle);
CCODE 	FS_VFS_AddTrustee(FS_FILE_OR_DIR_HANDLE * fileOrDirHandle, UINT32 objectID, UINT16 trusteeRights);
CCODE 	FS_VFS_DeleteTrustees(FS_FILE_OR_DIR_HANDLE * fileOrDirHandle, FS_FILE_OR_DIR_INFO * info);
CCODE 	FS_VFS_OpenAndRename(FS_FILE_OR_DIR_HANDLE * fsHandle, BOOL isParent, unicode * bytePath, UINT32 nameSpaceType, unicode * oldFullName, unicode * newDataSetName);
CCODE 	FS_VFS_Read(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,  FS_READ_HANDLE *fileReadHandle, UINT32 bytesToRead, void *buffer, UINT32 *bytesRead, BOOL IsMigratedFile, UINT32 readIndex);
CCODE 	FS_VFS_GetInheritedRightsMask(FS_FILE_OR_DIR_HANDLE *handle, UINT32 *inheritedRightsMask);
CCODE 	FS_VFS_FixPathToVFS(unsigned char *parentPath, unicode *inPath, unsigned char **fixedPath);
CCODE 	FS_VFS_GetInfo(FS_FILE_OR_DIR_HANDLE *handle, UINT32 getInfoMask, UINT32 bufferSize, void *buffer);
void 	FS_VFS_PopulatezInfoFromVFS(void *infoStruct, int infoSize, struct stat *statBuf, STRING name, UINT32 mask, UINT32* fileType);
CCODE 	FS_VFS_RestoreInitScan(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence);
CCODE	FS_VFS_SetReadyFileDirForRestore(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 isPathFullyQualified, unicode *dSetName, UINT32 rFlags, UINT32  createMode, FS_ACCESS_RIGHTS accessRights, FS_ATTRIBUTES 	attributes);
CCODE 	FS_VFS_Write(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD startingOffset, UINT32 bytesToWrite, void *buffer, UINT32 *bytesWritten, UINT32 handleIndex);
CCODE 	FS_VFS_Create(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_CREATE_MODE createMode, FS_ACCESS_RIGHTS accessRights, FS_ATTRIBUTES	attributes, UINT32 streamType, FS_HANDLE *retHandle, unicode *dSetName,UINT32 rflags, BOOL isPathFullyQualified, void* buffer, FS_HL_TABLE **hlTable);
CCODE 	FS_VFS_ModifyInfo(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD modifyInfoMask, UINT32 bufferSize, void *buffer);
CCODE	FS_VFS_SetFileSize(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD newSize, UINT32 handleIndex) ; 
CCODE 	FS_VFS_DeleteSetPurge(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 attributes);
CCODE 	FS_VFS_DeleteFile(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 deleteFlags);
void 	FS_VFS_SetFileTypes(UINT32 *fileType, void * nfsInfo, UINT32 linkCount);
void* 	FS_VFS_GetVariableData(void *information);
#endif /*N_PLAT_NLM */

char * removeVolName(char *path, UINT32 buffsize, UINT32 nameSpace);
CCODE getComponent(char *component, UINT32 totalPathCount,  UINT32 nameSpace);
UINT32 MapToNSSNameSpace(UINT32 nameSpaceType);


#endif
