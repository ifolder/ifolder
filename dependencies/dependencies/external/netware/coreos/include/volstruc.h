#ifndef __VOLSTRUC_H__
#define __VOLSTRUC_H__
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
 *  $Workfile:   VOLSTRUC.H  $
 *  $Modtime:   Mar 22 2001 11:31:56  $
 *  $Revision$
 *
 ****************************************************************************/

#define MaximumVolumeMappingTableSize	255

#define NumberOfJumpRoutines	140
#define NumberOfAttrHandles 	32
#define VolumeMappingError		152

typedef struct VolumeStructure
{
	LONG handle;
	LONG volumestatus;
	LONG volumeinstance;
	BYTE volumename[64];
	LONG filesystemid;
	LONG filesystemhandle;
	BYTE *filesystemname;
	LONG (*filesystempollroutine)();
	LONG (*filesystemabortroutine)();

	LONG (*LFS_CreateDirectory)();		/* 1 */
	LONG (*LFS_DeleteDirectory)();
	LONG (*LFS_MapPathToDirectoryNumber)();
	LONG (*LFS_EraseFile)();
	LONG (*LFS_ModifyDirectoryEntry)();
	LONG (*LFS_RenameEntry)();
	LONG (*LFS_GetAccessRights)();
	LONG (*LFS_GetAccessRightsFromIDs)();
	LONG (*LFS_MapDirectoryNumberToPath)();
	LONG (*LFS_GetEntryFromPathStringBa)();  /* 10 */
	LONG (*LFS_GetOtherNameSpaceEntry)();
	LONG (*LFS_DirectorySearch)();
	LONG (*LFS_GetExtendedDirectoryInfo)();
	LONG (*LFS_GetParentDirectoryNumber)();
	LONG (*LFS_AddTrusteeRights)();
	LONG (*LFS_ScanTrusteeRights)();
	LONG (*LFS_DeleteTrusteeRights)();
	LONG (*LFS_PurgeTrustee)();
	LONG (*LFS_FindNextTrusteeReference)();
	LONG (*LFS_ScanUserRestrictionNodes)(); 			/* 20 */
	LONG (*LFS_AddUserRestriction)();
	LONG (*LFS_DeleteUserRestriction)();
	LONG (*LFS_ReturnDirectorySpaceRest)();
	LONG (*LFS_GetActualAvailableDiskSp)();
	LONG (*LFS_CountOwnedFilesAndDirect)();
	LONG (*LFS_ScanDeletedFiles)();
	LONG (*LFS_SalvageDeletedFile)();
	LONG (*LFS_PurgeDeletedFile)();
	LONG (*LFS_OpenFile)();
	LONG (*LFS_CreateFile)();	/* 30 */
	LONG (*LFS_CreateAndOpenFile)();
	LONG (*LFS_MigrateFile)();
	LONG (*LFS_DeMigrateFile)();
	LONG (*LFS_MigratedFileInformation)();
	LONG (*LFS_VolumeMigrationInformation)();
	LONG (*LFS_ReadMigratedFileData)();
	LONG (*LFS_ReadFile)();
	LONG (*LFS_WriteFile)();
	LONG (*LFS_ASyncStartReadFile)();
	LONG (*LFS_ASyncDoReadFile)();	  /* 40 */
	LONG (*LFS_ASyncStartWriteFile)();
	LONG (*LFS_ASyncDoWriteFile)();
	LONG (*LFS_ASyncCheckWriteThrough)();
	LONG (*LFS_NewGetVolumeInfo)();
	LONG (*LFS_MapPathToDirectoryNumberOrPhantom)();
	LONG (*LFS_StationHasAccessRightsGrantedBelow)();
	LONG (*LFS_GetDataStreamLengthsFromPathString)();
	LONG (*LFS_CheckAndGetDirectoryEntry)();
	LONG (*LFS_GetDeletedEntry)();
	LONG (*LFS_GetOriginalNameSpace)(); /* 50 */
	LONG (*LFS_GetActualFileSize)();
	LONG (*LFS_VerifyNameSpaceNumber)();
	LONG (*LFS_VerifyDataStreamNumber)();
	LONG (*LFS_CheckVolumeNumber)();
	LONG (*LFS_GetFileSize)();
	LONG (*LFS_ReadFileNoCheck)();
	LONG (*LFS_SetFileTimeAndDateStamp)();
	LONG (*LFS_GetFileHoles)();
	LONG (*LFS_GetHandleInfoData)();
	LONG (*LFS_CloseFile)();	/* 60 */
	LONG (*LFS_CommitFile)();
	LONG (*LFS_VMGetDirectoryEntry)();
	LONG (*LFS_CreateDataMigratedFileEntry)();
	LONG (*LFS_RenameNameSpaceEntry)();
	LONG (*LFS_CancelFileLockWait)();
	LONG (*LFS_CheckAndSetSingleFileLock)();
	LONG (*LFS_ReleaseSingleFileLock)();
	LONG (*LFS_EnumerateFileLocks)();
	LONG (*LFS_CheckAndSetFileLocks)();
	LONG (*LFS_BackoutFileLocks)(); /* 70 */
	LONG (*LFS_UnenumerateFileLocks)();
	LONG (*LFS_ReleaseFile)();
	LONG (*LFS_CheckAndSetSingleRecordLock)();
	LONG (*LFS_ReleaseSingleRecordLock)();
	LONG (*LFS_EnumerateRecordLocks)();
	LONG (*LFS_CheckAndSetRecordLocks)();
	LONG (*LFS_BackoutRecordLocks)();
	LONG (*LFS_UnenumerateRecordLocks)();
	LONG (*LFS_ReleaseRecordLocks)();
	LONG (*LFS_SetVolumeFlags)(); /* 80 */
	LONG (*LFS_ClearVolumeFlags)();
	LONG (*LFS_GetOriginalInfo)();
	LONG (*LFS_CreateMigratedDir)();
	LONG (*LFS_F3OpenCreate)();
	LONG (*LFS_F3InitFileSearch)();
	LONG (*LFS_F3ContinueFileSearch)();
	LONG (*LFS_F3RenameFile)();
	LONG (*LFS_F3ScanForTrustees)();
	LONG (*LFS_F3ObtainFileInfo)();
	LONG (*LFS_F3ModifyInfo)(); 					/*90*/
	LONG (*LFS_F3EraseFile)();
	LONG (*LFS_INWGetVolumeInformation)();
	LONG (*LFS_F3AddTrustees)();
	LONG (*LFS_F3DeleteTrustees)();
	LONG (*LFS_GetActiveVolumeIOs)();
	LONG (*LFS_F3ScanSalvagedFiles)();
	LONG (*LFS_F3RecoverSalvagedFiles)();
	LONG (*LFS_F3PurgeSalvageableFile)();
	LONG (*LFS_F3GetNSSpecificInfo)();
	LONG (*LFS_F3ModifyNSSpecificInfo)();			/*100*/
	LONG (*LFS_F3SearchSet)();
	LONG (*LFS_F3GetDirBase)();
	LONG (*LFS_F3QueryNameSpaceInfo)();
	LONG (*LFS_F3GetNameSpaceList)();
	LONG (*LFS_F3GetHugeInfo)();
	LONG (*LFS_F3SetHugeInfo)();
	LONG (*LFS_F3GetFullPathString)();
	LONG (*LFS_F3GetEffectiveDirectoryRights)();
	LONG (*LFS_ParseTree)();
	LONG (*LFS_GetReferenceCountFromEntry)();		/* 110 */
	LONG (*LFS_SetOwningNameSpace)();
	LONG (*LFS_GetHandleInfo)();
	LONG (*LFS_GetFileInfoFromHandle)();
	LONG (*LFS_RemoveFile)();
	LONG (*LFS_RemoveFileCompletely)();
	LONG (*LFS_GetNameSpaceInformation)();
	LONG (*LFS_ClearPhantom)();
	LONG (*LFS_GetMaximumUserRestriction)();
	LONG (*LFS_GetCurrentDiskUsedAmount)();
	LONG (*LFS_FlushVolume)();						/* 120 */
	LONG (*LFS_SetCompressedFileSize)();
	LONG (*LFS_OpenEAHandle)(); /* 122 */
	LONG (*LFS_CloseEAHandle)();	/* 123 */
	LONG (*LFS_CleanEAHandle)();	/* 124 */
	LONG (*LFS_CheckAndDeleteEAHandle)();	/* 125 */
	LONG (*LFS_WriteEAData)();	/* 126 */
	LONG (*LFS_ReadEAData)();	/* 127 */
	LONG (*LFS_EnumEA)();	/* 128 */
	LONG (*LFS_AddNameSpace)(); /* 129 */
	LONG (*LFS_DismountVolume)(); /* 130 */
	LONG (*LFS_CheckVolumeForObjectID)(); /* 131 */
	LONG (*LFS_ModifyDOSAttributes)(); /* 132 */
	LONG (*LFS_UpdateDirectory)(); /* 133 */
	LONG (*LFS_GetRawFileSize)(); /* 134 */
	LONG (*LFS_SetVolumeObjectID)(); /* 135 */
	LONG (*LFS_GetVolumeOwnerRestrictionNodes)(); /* 136 */
	LONG (*LFS_F3OpenCreateEx)(); /* 137*/		/*defect 100253657 - RDoxey*/
	LONG reserved[3];
} volumetype;

extern volumetype VolumeRecoveryStructure;

extern volumetype **VolumeMappingTable;
extern LONG VolumeMappingTableSize;

extern volumetype **OverflowVolumeMappingTable;
extern LONG OverflowVolumeMappingTableSize;

typedef struct volinfodef
{
	LONG volumestatus;
	LONG volumeinstance;
	BYTE volumename[64];
	LONG filesystemid;
} volinfotype;

typedef struct volumerequestdef
{
	LONG reserved[5];
	LONG volumenumber;
	LONG stamp;
	struct resourcetagdef *resourcetag;

	struct resourcetagdef *resourcetagnext;
	struct resourcetagdef *resourcetaglast;
	LONG callbackparameter;
	void (*callbackroutine)();

	LONG function;
	LONG parameter1;
	LONG parameter2;
	LONG parameter3;
} volumerequesttype;

typedef struct resourcetagdef
{
	LONG reserved[2];
	LONG signature;
	LONG count;
	struct resourcetagdef *resourcetagnext;
	struct resourcetagdef *resourcetaglast;
} resourcetagtype;

typedef struct eventdef
{
	LONG reserved1;
	LONG timeout;
	LONG reserved2;
	void (*routine)();

	struct resourcetagdef *resourcetag;
	LONG reserved3;
	LONG parameter0;
	LONG parameter1;

	LONG parameter2;
	LONG parameter3;
} eventtype;

#define ACTIVEVOLUMEREQUEST 		0x52455356
#define ABORTEDVOLUMEREQUEST		0x53564241

/* Novell assigned file system numbers */

#define NETWARE386FILESYSTEM		0x00000001
#define NETWARENFSFILESYSTEM		0x00000002
#define NETWARENSSFILESYSTEM		0x00000003
#define NETWARECDROMFILESYSTEM	0x70000001
#define IBM_SMB_LAN_SERV_FS 	0xf0000001

//////////////////////////////////////////////////////////////////////////////////////
/* volume status bits */
//////////////////////////////////////////////////////////////////////////////////////
#define VOLUME_ONLINE_BIT			0x00000001
#define VOLUME_COMING_ONLINE_BIT	0x00000004
#define VOLUME_MOUNTED_BIT			0x00000002
#define VOLUME_BEING_MOUNTED_BIT	0x00000008

#define VOLUME_MASK1			(VOLUME_ONLINE_BIT | VOLUME_COMING_ONLINE_BIT | VOLUME_MOUNTED_BIT | VOLUME_BEING_MOUNTED_BIT)

//////////////////////////////////////////////////////////////////////////////////////
#define VOLUME_NEEDS_REPAIRING_BIT	0x00000010
#define VOLUME_BEING_REPARED_BIT	0x00000020
#define VOLUME_HOTFIXED_BIT 		0x00000040
#define VOLUME_HOTFIX_OK_BIT		0x00000080

#define VOLUME_MASK2			(VOLUME_NEEDS_REPAIRING_BIT | VOLUME_BEING_REPARED_BIT | VOLUME_HOTFIXED_BIT | VOLUME_HOTFIX_OK_BIT)

//////////////////////////////////////////////////////////////////////////////////////
#define VOLUME_MIRRORED_BIT 		0x00000100
#define VOLUME_MIRRORS_OK_BIT		0x00000200
#define VOLUME_PARITIED_BIT 		0x00000400
#define VOLUME_PARITY_OK_BIT		0x00000800

#define VOLUME_MASK3			(VOLUME_MIRRORED_BIT | VOLUME_MIRRORS_OK_BIT | VOLUME_PARITIED_BIT | VOLUME_PARITY_OK_BIT)


//////////////////////////////////////////////////////////////////////////////////////
#define VOLUME_IS_READONLY_BIT		0x00010000
#define VOLUME_TTS_AVAILABLE_BIT	0x00020000
#define VOLUME_TTS_OK_BIT			0x00040000
//#define VOLUME_IS_REMOTE_BIT		0x00890000

#define VOLUME_MASK4			(VOLUME_IS_READONLY_BIT | VOLUME_TTS_AVAILABLE_BIT | VOLUME_TTS_OK_BIT)


//////////////////////////////////////////////////////////////////////////////////////
#define VOLUME_DATA_MIGRATION_BIT	0x01000000
#define VOLUME_COMPRESSION_BIT		0x02000000
#define VOLUME_SUB_ALLOC_BIT			0x04000000
#define VOLUME_IMMEDIATE_PURGE_BIT	0x08000000

#define VOLUME_MASK5			(VOLUME_DATA_MIGRATION_BIT | VOLUME_COMPRESSION_BIT | VOLUME_SUB_ALLOC_BIT | VOLUME_IMMEDIATE_PURGE_BIT)

//////////////////////////////////////////////////////////////////////////////////////
#define VOLUME_HIDDEN_BIT				0x80000000

#define VOLUME_MASK6			(VOLUME_HIDDEN_BIT)

//////////////////////////////////////////////////////////////////////////////////////
#define VALID_VOLUME_STATUS_BITS_MASK	(VOLUME_MASK1|VOLUME_MASK2|VOLUME_MASK3|VOLUME_MASK4|VOLUME_MASK5|VOLUME_MASK6)

/* Volume function codes */

#define VOLUME_ONLINE		0
#define VOLUME_OFFLINE		1
#define VOLUME_MOUNT		2
#define VOLUME_DISMOUNT 	3
#define VOLUME_DELETE		4
#define VOLUME_RENAME		5
#define VOLUME_REPAIR		6
#define VOLUME_GET_STATS	7


/* volume manager routine error codes */

#define SUCCESSFUL					0x0000
#define MEMORY_ALLOCATION_ERROR 	0x1001
#define TRUNCATION_ERROR			0x1002
#define VOLUME_TABLE_FULL			0x1003
#define INVALID_PARAMETERS			0x1004
#define INVALID_STATE				0x1005
#define UNSUPPORTED_FUNCTION		0x1006

extern BYTE *NW386FileSystemString;

/****************************************************************************/
/****************************************************************************/


#endif /* __VOLSTRUC_H__ */
