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
 | $Modtime:   20 Jun 2004  $
 |
 | $Workfile:   fsvfsopen.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Specify FS open related functions for VFS.
 +-------------------------------------------------------------------------*/

/* LibC headers */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <osprimitives.h>
#include <errno.h>
#include <unistd.h>
extern int errno;

/* NSS Headers */
#include <zPublics.h>
#include <zParams.h>
#include <zError.h>

#include <smsdefns.h>
#include <smstypes.h>

#include <fsinterface.h>
#include "smstserr.h"
#include <tsa_320.mlh>
#include "tsa_defs.h"
#include <tempfile.h>
#include <tsalib.h>
#include <incexc.h>
#include <tsaunicode.h>
#include <smsdebug.h>
#include <customdebug.h>
#include "tsaresources.h"

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_READ|FILESYS
#define FNAME   "FS_VFS_Open"
#define FPTR 	FS_VFS_Open
CCODE FS_VFS_Open(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_ACCESS_RIGHTS accessRights, 
			UINT32 openModeFlags, FS_FILE_OR_DIR_INFO *info, BOOL noLock, UINT32 searchAttributes, FS_HL_TABLE **hlTable)
{
	int				 status = 0;
	FS_HANDLE		 tmpHandle;
	unsigned char	*path = NULL;
	LONG			 st_mode;
	VARIABLE_DATA	*varData = NULL;
	UINT32 		linkCount = 0;
	STRING ln_name = NULL;
	zInfo_s *fileInfo = NULL;

	FStart();
	
	/**	If either the fileOrDirHandle is NULL, or info is NULL, or if the flags 
		have any unsupported bit set, then there is an error invalid parameter **/
	if((fileOrDirHandle == NULL) ||(info == NULL) || !(openModeFlags & FS_PRIMARY_DATA_STREAM))
	{
		status = FS_INTERNAL_ERROR;
		goto Return;
	}
	
	if(info->isDirectory || info->isVolume)
		accessRights &= ~zRR_DENY_WRITE;

	status = FS_VFS_FixPathToVFS(fileOrDirHandle->parentHandle.vfsHandle.fullPath, fileOrDirHandle->uniPath, &path);
	if (status)
	{
		goto Return;
	}
		
	/* We will open and read only files that are of type regular or hard links 
	 * Just store a dummy handle for the rest. */
	 fileInfo = ((zInfo_s *)(info->information));
	st_mode = fileInfo->unixNS.info.fMode;
	if(st_mode &&  info->fileType && hlTable)
	{
		linkCount = fileInfo->count.hardLink;
		/*get the iNode number, dev_no and filetype*/
		varData = FS_VFS_GetVariableData(info->information);
		if(varData && (status = HL_CheckLink(hlTable, &varData->fileType, varData->dev_no, linkCount, varData->iNodeNumber, path, &ln_name)) == 1)
		{/*if hardlink encode the link name, this will be useful in restore*/
			if(varData->fileType == FS_VFS_HARD_LINK)
			{
				unicode *uniLnName = NULL;
				UINT32 uniPathLength = 0, linkLength;
				linkLength = strlen(ln_name)+1;
				SMS_ByteToUnicode((char*)ln_name, &uniPathLength, &uniLnName, NULL);
				unicpy((unicode *)zINFO_PTR(fileInfo, fileInfo->nextByte), uniLnName);
				zINFO_FILENAMES(fileInfo)[FS_LINK_NAME] =  fileInfo->nextByte;
				tsaFree(uniLnName);
				uniLnName = NULL;
				free(ln_name);
				fileInfo->nextByte += ((uniPathLength+1) * sizeof(unicode));
			}
		}
		if(status && status != 1)/*add the link name if not already stored*/
			status = HL_AddLink(hlTable, &varData->fileType, varData->dev_no, linkCount, varData->iNodeNumber, path);
		
		if(varData)/*update file type as we need it down below*/
			info->fileType = varData->fileType;
	}
	/*read data for hardlinks as well, even though it is duplication - default option*/
	if (st_mode && info->fileType != FS_VFS_REGULAR && info->fileType != FS_VFS_HL_REG && info->fileType != FS_VFS_HARD_LINK)
	{
		fileOrDirHandle->handleArray[0].type = DATASET_IS_NOTREADABLE;
		status = 0;
		goto Return;
	}
	
	/**	If the object to be open is a directory, then data stream check is redundant **/
	if((openModeFlags & FS_PRIMARY_DATA_STREAM) || (info->isDirectory))
	{
		status = open(path, O_RDONLY | O_LARGEFILE);		
		if (status != -1)
		{
			tmpHandle.vfsHandle.fileDescriptor = status;
			status = 0;
		}
		
		if(status == 0)
		{
			fileOrDirHandle->handleArray[0].handle.vfsHandle.fullPath = path;
			path = NULL;
			
			fileOrDirHandle->handleArray[0].handle.vfsHandle.fileDescriptor = tmpHandle.vfsHandle.fileDescriptor;
			fileOrDirHandle->handleArray[0].size = FS_GetFileStreamSize(info->information);
			fileOrDirHandle->streamCount = 1;
		}
		else
		{
		    FLogError("open", errno, path);			
		    if(errno == ETXTBSY)
				status = NWSMTS_DATA_SET_IN_USE;
			else
				status = NWSMTS_OPEN_DATA_STREAM_ERR;
			goto Return;
		}
	}

Return:
	if(status != 0)
	{
		/**	Close all the open handles in case of any error **/
		FS_VFS_Close(fileOrDirHandle, DATASET_CLOSE_ALL_STREAM_HANDLES);
	}
	if (path)
		tsaFree(path);
	FEnd(status);
	return status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_READ|FILESYS
#define FNAME   "FS_VFS_GetInfo"
#define FPTR 	FS_VFS_GetInfo
CCODE FS_VFS_GetInfo(FS_FILE_OR_DIR_HANDLE *handle, UINT32 getInfoMask, UINT32 bufferSize, void *buffer)
{
	CCODE cCode = 0;
	struct stat statBuf;
	unsigned char *nodeName= handle->handleArray[0].handle.vfsHandle.fullPath;
		
	cCode = lstat(nodeName, &statBuf); /* we are doing lstat, so that we don't follow symbolic links*/
	if(cCode)
	{
		FLogError("lstat", errno, nodeName);
		cCode = NWSMTS_INTERNAL_ERROR;
		goto Return;
	}

	FS_VFS_PopulatezInfoFromVFS(buffer, bufferSize, &statBuf, nodeName, getInfoMask, &handle->fileType );
	
Return:
	return cCode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	SMS_INTERNAL
#define FNAME   "FS_VFS_PopulatezInfoFromVFS"
#define FPTR 	FS_VFS_PopulatezInfoFromVFS
void FS_VFS_PopulatezInfoFromVFS(void *infoStruct, int infoSize, struct stat *statBuf, STRING name, UINT32 mask, UINT32* fileType)
{
	zInfoB_s		*fileInfo = NULL;
	UINT32		uniPathSizeinShorts = 0;
	unicode		*uniName = NULL;
	CCODE		cCode = 0;
	unsigned char 	 rootName[] = "ROOT:", *terminalName, *savePtr = NULL;

	fileInfo = infoStruct;
	memset(fileInfo, 0, infoSize);

	/* Set the info version to B always, we will populate the B structure in case of VFS */
	fileInfo->infoVersion = zINFO_VERSION_B;

	/* Set the total bytes in the structure and also point the next free space in the variable data */
	(fileInfo)->totalBytes = (infoSize);
	fileInfo->nextByte = (UINT32)(&(fileInfo->variableData)) - (UINT32)fileInfo;

	/* Set the return info mask */
	fileInfo->retMask = mask;

	/* The following section are not supported by VFS layers, setting the mask to state that the information is not returned. */
	/* EAs: No EAs on Linux. Till at least we start supporting Linux EAs */
	fileInfo->retMask &= ~(zGET_DATA_STREAM_INFO | zGET_EXTENDED_ATTRIBUTE_INFO | zGET_DELETED_INFO |  \
						zGET_MAC_METADATA |zGET_VOLUME_INFO | zGET_VOL_SALVAGE_INFO | zGET_POOL_INFO | \
						zGET_POOL_INFO | zGET_DIR_QUOTA);
	
	/* Filling up std info */
	if (mask & zGET_STD_INFO)
	{
		/* No zID support, ignore zid, dataStreamZid, parentZid
		 * No volume concepts here, ignore volumeID */
		 
		/* TBD: When getting 64 bit file size support this would change */
		if(!S_ISLNK(statBuf->st_mode)) //set only if it is not a link
			fileInfo->std.logicalEOF = statBuf->st_size;
		else
			fileInfo->std.logicalEOF = 0;

		/* Mark this as a regular file, this is true for files/directories */
		fileInfo->std.fileType = zFILE_REGULAR;

		/* Some file attributes can be mapped like follows but are NOT,
		 * READ_ONLY: If the access rights to all/group/owner permits read only then it can be marked READ_ONLY
		 *           But, what is the exact purpose, as this is  asinglt bit of information from three different source,
		 *           that can only be used for restore purposes. As the access rights are backed up, 
		 *           we can ignore such mappings. The same holds for execute/hidden/copy_inhibit etc.. */

		/* Mark the file as a link once link support is added */
		fileInfo->std.fileAttributes = 0;
		if(S_ISDIR(statBuf->st_mode))
			fileInfo->std.fileAttributes |= zFA_SUBDIRECTORY;
		
		/* Set to all enabled if it is used again for modify purposes. */
		fileInfo->std.fileAttributesModMask = 0xFFFFFFFF;
	}

	/* Storage Used */
	if (mask & zGET_STORAGE_USED)
	{
		/* The physical EOF is set to the file size itself rather than the actual 
		 * physical EOF as true sparse recognition is not possible in VFS. */
		if(!S_ISLNK(statBuf->st_mode))
		{
			fileInfo->storageUsed.physicalEOF = statBuf->st_size;
			fileInfo->storageUsed.dataBytes = statBuf->st_size;
		}
		else
		{
			fileInfo->storageUsed.physicalEOF = 0;
			fileInfo->storageUsed.dataBytes = 0;

		}

		/* Strictly the following field is NOT used. Mark it as irrelevant */
		fileInfo->storageUsed.metaDataBytes = 0;
	}

	/* Naming Info */
	if (mask & zGET_PRIMARY_NAMESPACE)
		fileInfo->primaryNameSpaceID = zNSPACE_UNIX;

	if (mask & zGET_NAME)
	{
		fileInfo->nameStart = fileInfo->nextByte;
		if (!strcmp(name, "/"))
		{
			cCode = SMS_ByteToUnicode(rootName, &uniPathSizeinShorts, &uniName, NULL);		
			if(cCode)
			{
				FTrack(BACKUP | RESTORE, DC_MINOR | DC_VERBOSE, "Conversion error\n");
				return;
			}
		}
		else
		{
			terminalName = strrchr(name, '/');
			if (*(terminalName + 1) == '\0')
			{
				savePtr = terminalName;
				*terminalName = '\0';
				terminalName = strrchr(name, '/');
			}
			
			uniPathSizeinShorts = 0;
			/* PTBD - We are doing an extra alloc for the conversion. Got to remove that. */
			if(cCode = SMS_ByteToUnicode(terminalName+1, &uniPathSizeinShorts, &uniName, NULL))
			{
				if (savePtr)
					*savePtr = '/';				
				FTrack(BACKUP | RESTORE, DC_MINOR | DC_VERBOSE, "Conversion error\n");
				return;
			}

			if (savePtr)
				*savePtr = '/';
		}

		unicpy((unicode *)zINFO_NAME(fileInfo), uniName);
		tsaFree(uniName);
		uniName = NULL;
		fileInfo->nextByte += ((uniPathSizeinShorts + 1)*sizeof(unicode));
	}

	if (mask & zGET_ALL_NAMES)
	{
		/* TBD: This should return only the UNIX names for VFS. Currently common code assumes DOS at least.
		 * Fix it once the common code is fixed. */
		cCode = FS_VFS_InfoInitFileName(fileInfo, 6); // 5 maximum name spaces to encode +1 for softlink name
		if (cCode)
		{			
			FTrack(BACKUP | RESTORE, DC_MINOR | DC_VERBOSE, "Error init file names\n");
			return;
		}
		zINFO_FILENAMES(fileInfo)[zNSPACE_DOS] = fileInfo->nameStart;
		zINFO_FILENAMES(fileInfo)[zNSPACE_LONG] = fileInfo->nameStart;
		zINFO_FILENAMES(fileInfo)[zNSPACE_MAC] = fileInfo->nameStart;
		zINFO_FILENAMES(fileInfo)[zNSPACE_UNIX] = fileInfo->nameStart;	
	}

	/* Time info */
	/* TBD: Only one of the two flags below should be requested, this is a common code error, fix it once common code is fixed */
	if (mask & zGET_TIMES_IN_MICROS || zGET_TIMES_IN_SECS)
	{
		/* Creation time is not maintained on VFS so ignoring it 
		 * All times in NSS are also in UTC as the current VFS times, so just store them. */
		fileInfo->time.archived = statBuf->st_mtime;
		fileInfo->time.modified = statBuf->st_mtime;
		fileInfo->time.accessed = statBuf->st_atime;	
		fileInfo->time.metaDataModified = statBuf->st_ctime;	
	}

	/* ID info */
	/* Not overloading the owner ID here for VFS systems. As it needs a GUID representation or the GUID structure,
	 * overload. Instead this can be picked up from the UNIX section. */
		 
	/* The archiver/modifer/meta-modifer attributes are not supported in Linux */

	/* blockSize */
	if (mask & zGET_BLOCK_SIZE)
	{
		fileInfo->blockSize.size = statBuf->st_blksize;
		/* Dummy the size shift, it is not used */
		fileInfo->blockSize.sizeShift = 0;
	}

	/* count */
	if (mask & zGET_COUNTS)
	{
		/* TBD: The open field should contain the number of currently open handles to this file */
		fileInfo->count.open = 0;

		/* TBD: The total number of hardlinks to this file, currently set to 1, revise if working with links */
		fileInfo->count.hardLink = statBuf->st_nlink;
	}

	/* UNIX Name Space Info: This is filled with the stat information that we have */
	if (mask & zGET_UNIX_METADATA)
	{
		UINT32 l_fileType = 0;
		STRING ln_name = NULL;
		UINT32 dev_no = statBuf->st_dev;
		VARIABLE_DATA varData;
		/* File access mode */
		fileInfo->unixNS.info.fMode = statBuf->st_mode;
		fileInfo->unixNS.info.rDev = statBuf->st_rdev;
		/*IGNORED: fileInfo->unixNS.info.myFlags = ; */
		fileInfo->unixNS.info.nfsUID = statBuf->st_uid;
		fileInfo->unixNS.info.nfsGID = statBuf->st_gid;
		fileInfo->unixNS.info.nwUID = statBuf->st_gid;
		fileInfo->unixNS.info.nwGID = statBuf->st_uid;
		/*IGNORED: fileInfo->unixNS.info.nwEveryone = ;*/
		/*IGNORED: fileInfo->unixNS.info.nwUIDRights = ;*/
		/*IGNORED: fileInfo->unixNS.info.nwGIDRights = ;*/
		/*IGNORED: fileInfo->unixNS.info.nwEveryoneRights = ;*/
		/*IGNORED: fileInfo->unixNS.info.acsFlags = ;*/
		
		/* Follow the NSS way of doing this. This indicates that the file need not be set with the default NFS characteristics */
		fileInfo->unixNS.info.firstCreated = 255;
		/*set the file types as we need for internal manipulation*/
		FS_VFS_SetFileTypes(&l_fileType, &fileInfo->unixNS.info, statBuf->st_nlink);
		/*store the soft linked name*/
		if(l_fileType == FS_VFS_SOFT_LINK || l_fileType == FS_VFS_HL_TO_SL)
		{
			STRING_BUFFER *linkName;
			unicode *uniLnName = NULL;
			UINT32 uniPathLength = 0, linkLength;
			linkLength = statBuf->st_size+1;
			linkName = tsaCalloc(1, sizeof(STRING_BUFFER)+linkLength); /*includes null terminator*/
			linkName->size = linkLength;
			name = (char*)FixDirectoryPath(name, 2, NULL);
			
			if(readlink(name, (char*)&linkName->string, linkLength) == -1)
				perror("Error reading link");
			else
			{
				linkName->string[linkLength-1] = 0;
				fileInfo->unixNS.info.variableSize = linkName->size;
				SMS_ByteToUnicode((char*)&linkName->string, &uniPathLength, &uniLnName, NULL);
				unicpy((unicode *)zINFO_PTR(fileInfo, fileInfo->nextByte), uniLnName);
				zINFO_FILENAMES(fileInfo)[FS_LINK_NAME] =  fileInfo->nextByte;
				tsaFree(uniLnName);
				uniLnName = NULL;
				tsaFree(linkName);
				fileInfo->nextByte += ((uniPathLength+1) * sizeof(unicode));
			}
		}
		/*store the iNode, device, type for links*/
		if(l_fileType == FS_VFS_SOFT_LINK || l_fileType == FS_VFS_HARD_LINK)
		{
			varData.iNodeNumber = statBuf->st_ino;
			varData.fileType = l_fileType;
			varData.dev_no = statBuf->st_dev;
			fileInfo->unixNS.offsetToData = fileInfo->nextByte;
			memcpy(zINFO_PTR(fileInfo, fileInfo->nextByte), &varData, sizeof(VARIABLE_DATA));
			fileInfo->nextByte += sizeof(VARIABLE_DATA);
			fileInfo->unixNS.info.variableSize = sizeof(VARIABLE_DATA);
		}
		/*update file type, if requested*/
		if(fileType)
			*fileType = l_fileType;
	}

	return;
}

#define ALIGN(_x_, _p_)			(((_x_) + (_p_) - 1) & ~((_p_) - 1))

STATUS FS_VFS_InfoInitFileName (zInfo_s *info, NINT numNameSpaces)
{
	LONG	*fileNames;
	NINT	next;
	NINT	i;

	if (info->infoVersion > zINFO_VERSION_B)
	{
		goto noSpace;
	}
	next = info->nextByte;
	next = ALIGN(next, sizeof(LONG));
	if (next >= info->totalBytes)
	{
		goto noSpace;
	}
	info->names.fileNameArray = next;
	info->names.numEntries = numNameSpaces;
	fileNames = (LONG *)(((ADDR)info) + next);
	next += numNameSpaces * sizeof(LONG);

	if (next >= info->totalBytes)
	{
		goto noSpace;
	}

	info->nextByte = next;
	for (i = 0; i < numNameSpaces; ++i)
	{
		fileNames[i] = 0;
	}
	return zOK;

noSpace:
	info->names.fileNameArray = 0;
	info->names.numEntries = 0;
	return zERR_BUFFER_TOO_SMALL;
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_READ | FILESYS
#define FNAME   "FS_VFS_Close"
#define FPTR 	FS_VFS_Close
CCODE FS_VFS_Close(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 closeFlag)
{
	STATUS		status = 0; 
	UINT32		count;
	int			closeKey;
	FStart();

	/**	Close all the data Streams handles in case of a file **/
	for(count = 0; count < (fileOrDirHandle->streamCount + fileOrDirHandle->eaCount); count++)
	{
		if(closeFlag)
		{
			if(!(closeFlag & DATASET_CLOSE_ALL_NON_PRI_STREAM_HANDLES) &&
			  !(closeFlag & DATASET_CLOSE_ALL_EA_STREAM_HANDLES))
				continue;

			if(((closeFlag & DATASET_CLOSE_ALL_NON_PRI_STREAM_HANDLES) || (closeFlag & DATASET_CLOSE_ALL_EA_STREAM_HANDLES)) && (
				(fileOrDirHandle->handleArray[count].type != DATASET_IS_SECONDARY_DATASTREAM) && (fileOrDirHandle->handleArray[count].type != DATASET_IS_EXTENDED_ATTRIBUTE))) 
				continue;
		}
		
		if (fileOrDirHandle->handleArray[count].streamMap)	{
			tsaFree(fileOrDirHandle->handleArray[count].streamMap);
			fileOrDirHandle->handleArray[count].streamMap = NULL;
		}
		
		if(closeKey = fileOrDirHandle->handleArray[count].handle.vfsHandle.fileDescriptor)
		{
			status = close(closeKey);
			if (fileOrDirHandle->handleArray[count].handle.vfsHandle.fullPath)
				tsaFree(fileOrDirHandle->handleArray[count].handle.vfsHandle.fullPath);

			if(status)
			{
			    FLogError("close", errno, 0);
				status = NWSMTS_INTERNAL_ERROR;
				goto Return;
			}
			else
			{
				fileOrDirHandle->handleArray[count].handle.vfsHandle.fileDescriptor = 0;
			}
		}
	}

Return:
	FEnd((CCODE)status);
	return (CCODE)status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_READ | FILESYS
#define FNAME   "FS_VFS_GetNameSpaceEntryName"
#define FPTR 	FS_VFS_GetNameSpaceEntryName
CCODE FS_VFS_GetNameSpaceEntryName(UINT32 connID, FS_HANDLE handle, unicode *path, 
			unsigned long  nameSpaceType, unsigned long  nameSpace, unicode **newName, 
			UINT32 taskID, UINT32 *attributes)
{
	/* Pointer table filler. This is not called in any Linux call */
	return 0;
} 

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_READ | FILESYS
#define FNAME   "FS_VFS_ConvertOpenMode"
#define FPTR 	FS_VFS_ConvertOpenMode
CCODE FS_VFS_ConvertOpenMode(FS_FILE_OR_DIR_HANDLE *handle,UINT32 inputMode, UINT32 scanMode, BOOL COWEnabled, UINT32 scanType,
			FS_FILE_OR_DIR_INFO *fileInfo, FS_ACCESS_RIGHTS *outputMode, BOOL *noLock, UINT32 *openMode,BOOL FileIsMigrated)
{
    zInfo_s *temp=NULL;
    UINT32 dataStreamAttributes, count;

	*outputMode = 0;
	temp=(zInfo_s *)(fileInfo->information);
	
	*outputMode |= zRR_SCAN_ACCESS | zRR_READ_ACCESS | zRR_DONT_UPDATE_ACCESS_TIME;

	if ((inputMode & NWSM_OPEN_MODE_MASK) == NWSM_OPEN_READ_DENY_WRITE)
	{
		*outputMode |= zRR_DENY_WRITE;
		noLock = FALSE;
	}
	else if ((inputMode & NWSM_OPEN_MODE_MASK) == NWSM_USE_LOCK_MODE_IF_DW_FAILS)
	{
		*outputMode |= zRR_DENY_WRITE;
		*noLock = TRUE;
	}
	else if ((inputMode & NWSM_OPEN_MODE_MASK) == NWSM_NO_LOCK_NO_PROTECTION)		
	{
		*outputMode |= zRR_DENY_WRITE;
		*noLock = TRUE;
	}

	*openMode = 0;
	*openMode = scanMode;
	
	if (inputMode & NWSM_NO_DATA_STREAMS)
		*openMode &= (~FS_DATA_STREAM);
	
	if (inputMode & NWSM_NO_EXTENDED_ATTRIBUTES)
		*openMode &= (~FS_EXTENDED_ATTRIBUTES);
			
	return 0;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_READ|FILESYS
#define FNAME   "FS_VFS_GetCompressedFileSize"
#define FPTR 	FS_VFS_GetCompressedFileSize
CCODE FS_VFS_GetCompressedFileSize(FS_FILE_OR_DIR_HANDLE *handle)
{
	/* Compressed file support is not present in VFS, if the code comes here its an error.
	 * Function is defined to complete the FS table only. */
	return FS_INTERNAL_ERROR;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	FILESYS|VFS_READ
#define FNAME   "FS_VFS_SetSparseStatus"
#define FPTR 	FS_VFS_SetSparseStatus
CCODE FS_VFS_SetSparseStatus(FS_FILE_OR_DIR_HANDLE *fsHandles)
{
	/* Sparse file support is not available in VFS. Hence do nothing to leave the data set as non sparse */	
	return 0;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	FILESYS|VFS_READ
#define FNAME   "FS_VFS_DeleteFile"
#define FPTR 	FS_VFS_DeleteFile
CCODE FS_VFS_DeleteFile(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 deleteFlags)
{
	STATUS 			 status =0;
	UINT32			 pathLength;
	unsigned char 	*path = NULL;

	status = SMS_UnicodeToByte(fileOrDirHandle->uniPath, &pathLength, &path, NULL);
	if (!status)
	{
		status = remove(path);
		if (status == -1)
		{
			FLogError("remove", errno, NULL);
			status = NWSMTS_INTERNAL_ERROR;
		}

		tsaFree(path);
	}

	return ((CCODE)status);

}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	FILESYS|VFS_READ
#define FNAME   "FS_VFS_DeleteSetPurge"
#define FPTR 	FS_VFS_DeleteSetPurge
CCODE FS_VFS_DeleteSetPurge(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 attributes)

{
	/* We do not have purge capabilities, hence just call the delete */
	return(FS_VFS_DeleteFile(fileOrDirHandle, attributes));
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	FILESYS|VFS_READ
#define FNAME   "FS_VFS_SecStreamTrailerDataRecovery"
#define FPTR 	FS_VFS_SecStreamTrailerDataRecovery
CCODE FS_VFS_SecStreamTrailerDataRecovery(char *dataPtr, QUAD bytesToProcess, QUAD *sidfSize, char *underFlowBuffer, UINT32 *underFlowBufferLen)
{
	/* This function is present only for trailer errors of compressed files. As VFS dosent support compression,
	 * a restore of compressed files shouldent even get here. This is present here for FS pointer table completion */
	 
	return FS_INTERNAL_ERROR;
}

#if 0
CCODE FS_VFS_CheckForLinks()
{
		if(hlTable)
		{
			HL_CheckLink(hlTable, &l_fileType, dev_no, statBuf->st_nlink, statBuf->st_ino, name, &ln_name);

			/* if it is a hardlink, then we need to check for that node in the HLTABLE.
			*   if found we should only note down its link name
			*   else update the HL TABLE with its entry 
			*/
			
			if(l_fileType == FS_VFS_SOFT_LINK || l_fileType == FS_VFS_HL_TO_SL)
			{
				STRING_BUFFER *linkName;
				unicode *uniLnName = NULL;
				UINT32 uniPathLength = 0, linkLength;
				linkLength = statBuf->st_size+1;
				linkName = tsaCalloc(1, sizeof(STRING_BUFFER)+linkLength); /*includes null terminator*/
				linkName->size = linkLength;
				name = (char*)FixDirectoryPath(name, 2, NULL);
				
				if(readlink(name, (char*)&linkName->string, linkLength) == -1)
					perror("Error reading link");
				else
				{
					linkName->string[linkLength-1] = 0;
					fileInfo->unixNS.info.variableSize = linkName->size;
					SMS_ByteToUnicode((char*)&linkName->string, &uniPathLength, &uniLnName, NULL);
					unicpy((unicode *)zINFO_PTR(fileInfo, fileInfo->nextByte), uniLnName);
					zINFO_FILENAMES(fileInfo)[FS_LINK_NAME] =  fileInfo->nextByte;
					tsaFree(uniLnName);
					uniLnName = NULL;
					tsaFree(linkName);
					fileInfo->nextByte += ((uniPathLength+1) * sizeof(unicode));
				}
			}
			else if(l_fileType == FS_VFS_HARD_LINK)
			{
					unicode *uniLnName = NULL;
					UINT32 uniPathLength = 0, linkLength;
					linkLength = strlen(ln_name)+1;
					SMS_ByteToUnicode((char*)ln_name, &uniPathLength, &uniLnName, NULL);
					unicpy((unicode *)zINFO_PTR(fileInfo, fileInfo->nextByte), uniLnName);
					zINFO_FILENAMES(fileInfo)[FS_LINK_NAME] =  fileInfo->nextByte;
					tsaFree(uniLnName);
					uniLnName = NULL;
					free(ln_name);
					fileInfo->nextByte += ((uniPathLength+1) * sizeof(unicode));
			}
			
			if(l_fileType == FS_VFS_HL_REG || l_fileType == FS_VFS_HARD_LINK || l_fileType == FS_VFS_HL_TO_SL)
			{
				varData.iNodeNumber = statBuf->st_ino;
				varData.fileType = l_fileType;
				fileInfo->unixNS.offsetToData = fileInfo->nextByte;
				memcpy(zINFO_PTR(fileInfo, fileInfo->nextByte), &varData, sizeof(VARIABLE_DATA));
				fileInfo->nextByte += sizeof(VARIABLE_DATA);
				fileInfo->unixNS.info.variableSize = sizeof(VARIABLE_DATA);
			}
		}
}
#endif
