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
 | $Modtime:   27 Mar 2002 17:00:58  $
 |
 | $Workfile:   funpointer.h  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Define function pointer table prototypes
 +-------------------------------------------------------------------------*/

#ifndef _FUNPOINTER_H_
#define _FUNPOINTER_H_

#include <smsdefns.h>
#include <smstypes.h>
#include <fsinterface.h>
#include <tsaunicode.h>
#include <tsa_defs.h>

#ifdef N_PLAT_GNU
#include <osprimitives.h>
#endif
void PopulateAPITable();

#pragma pack(push,1)
typedef struct _FS_API_TABLE
{
	UINT32	filesysType;
	void    (* FS_SetFileDirSearchAttributes) (JOB *newJob);
	CCODE (* FS_InitScan)  (FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence);
	CCODE (* FS_EndScan)  (FS_SCAN_SEQUENCE *scanSequence);
	CCODE (* FS_ReInitScan)(FS_SCAN_SEQUENCE *scanSequence);
	//CCODE (* FS_ScanNextDirectoryEntry)(FS_SCAN_SEQUENCE *dirScanSequence, FS_FILE_OR_DIR_INFO *dirInfo);
	//CCODE (* FS_ScanNextFileEntry)(FS_SCAN_SEQUENCE *fileScanSequence, FS_FILE_OR_DIR_INFO *fileInfo);
	CCODE (* FS_Create)		(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 openMode, UINT32 accessRights, QUAD receivedCreateAttributes,  UINT32 streamType,
						 FS_HANDLE 		*retHandle, unicode *dSetName, UINT32 rflags, BOOL isPathFullyQualified, void* buffer, FS_HL_TABLE **hlTable); 
	CCODE (* FS_Open)		(FS_FILE_OR_DIR_HANDLE *handle, UINT32 openMode, UINT32 accessMode, FS_FILE_OR_DIR_INFO *info, BOOL noLock, UINT32 searchAttributes, FS_HL_TABLE **hlTable);
	CCODE (* FS_SetFileSize) (FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD newSize, UINT32 handleInUse);
	CCODE (* FS_Read)		(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,  FS_READ_HANDLE *fileReadHandle, UINT32 bytesToRead, void *buffer, UINT32 *bytesRead, BOOL IsMigratedFile, UINT32 readIndex);
	CCODE (*FS_Write)		(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD startingOffset, UINT32 bytesToWrite, void *buffer, UINT32 *bytesWritten, UINT32 handleIndex); 
	CCODE (* FS_Close)		(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 closeFlag);
	CCODE (* FS_ModifyInfo) (FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD modifyInfoMask, UINT32 bufferSize, void *buffer);
	CCODE (* FS_SetSparseStatus) (FS_FILE_OR_DIR_HANDLE *fsHandles);
	CCODE (* FS_ConvertOpenMode) (FS_FILE_OR_DIR_HANDLE *handle,UINT32 inputMode, UINT32 scanMode, BOOL COWEnabled, UINT32 scanType, \
			FS_FILE_OR_DIR_INFO *fileInfo, FS_ACCESS_RIGHTS *outputMode, BOOL *noLock, UINT32 *openMode,BOOL FileIsMigrated);
	BOOL (* FS_CheckCOWOnName) (unicode *dataSetName);
	CCODE (* JOB_GetInfoAboutFirstDataSet) (JOB *job, UINT32 *filterState);
	CCODE (* FS_ScanNextFileEntry) (FS_SCAN_SEQUENCE *fileScanSequence, FS_FILE_OR_DIR_INFO *fileInfo);
	CCODE (* FS_ScanNextDirectoryEntry)(FS_SCAN_SEQUENCE *dirScanSequence, FS_FILE_OR_DIR_INFO *dirInfo);
	unicode* (* FS_GetFileNameSpaceUNIName) (void *information, UINT32 retNameSpace, unicode *cfsNameSpaceInfo[]);
	unicode* (* FS_GetFilePrimaryUNIName) (void *information, UINT32 retNameSpace, unicode *cfsNameSpaceInfo[]);
	CCODE	(* FS_SetFileEntryInfo)			(FS_FILE_OR_DIR_HANDLE    *fileOrDirHandle, UINT32 setMask,UINT32 clientObjectID,UINT32 fileAttributes,UINT32 archivedDateAndTime);
	CCODE  (*FS_GetDirSpaceRestriction)			(void *info, FS_FILE_OR_DIR_HANDLE dirHandle, SQUAD *dirSpaceRestriction);
	CCODE (* FS_ScanTrustees)            (FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_FILE_OR_DIR_INFO *info);
	CCODE (* FS_GetNameSpaceEntryName)(UINT32 connID, FS_HANDLE handle, unicode *path, unsigned long  nameSpaceType, unsigned long  nameSpace, unicode **newName, UINT32 taskID, UINT32 *attributes);
	BOOL (* FS_FileExcludedByScanControl) (NWSM_SCAN_CONTROL	*scanControl, void *Info);
	CCODE (* FS_GetCompressedFileSize) (FS_FILE_OR_DIR_HANDLE *handle);
	void* (* FS_AllocateModifyInfo) (void  *info, UINT32 *size);
#ifdef N_PLAT_NLM
	CCODE (* FS_CreateMigratedDirectory) (FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,UINT32 smid, UINT32	bindFlag, UINT32	*bindKeyNumber);
#endif
	CCODE (*FS_SetCompressedFileSize) (FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD newSize);
	CCODE (*FS_AddTrustee)(FS_FILE_OR_DIR_HANDLE *   fileOrDirHandle, UINT32 objectID, UINT16  trusteeRights);
	CCODE (*FS_DeleteTrustees) (FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_FILE_OR_DIR_INFO *info);
	CCODE (*FS_OpenAndRename) (FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, BOOL isParent, unicode *bytePath, UINT32 nameSpaceType, unicode *oldFullName, unicode *newDataSetName);
	CCODE (*FS_SetPrimaryNameSpace) (FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 oldNameSpace, UINT32 newNameSpace);
	CCODE (*FS_SetNameSpaceName) (FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT8 nameSpace, void *newDataSetName);
	CCODE (*FS_GetInfo)(FS_FILE_OR_DIR_HANDLE *handle, UINT32 getInfoMask, UINT32 bufferSize, void *buffer);
	CCODE (*FS_RestoreInitScan)(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence);
	CCODE (*FS_SetReadyFileDirForRestore)(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 isPathFullyQualified, unicode *dSetName, UINT32 rFlags, UINT32 createMode, FS_ACCESS_RIGHTS accessRights, FS_ATTRIBUTES attributes);
	CCODE (*FS_DeleteSetPurge)(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 attributes);
	CCODE (*FS_DeleteFile)(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 deleteFlags);
	void*  (*FS_GenericGetFileNameSpaceName)(void *information, UINT32 retNameSpace, void *cfsNameSpaceInfo[]);
}FS_API_TABLE;

#pragma pack(pop)
#endif /* _FUNPOINTER_H_ */
