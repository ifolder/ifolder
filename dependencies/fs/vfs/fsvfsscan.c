/****************************************************************************
 |
 |  (C) Copyright 2001 Novell, Inc.
 |      All Rights Reserved.
 |
 |      This program is an unpublished copyrighted work which is proprietary
 |      to Novell, Inc. and contains confidential information that is not
 |      to be reproduced or disclosed to any other person or entity without
 |      prior written consent from Novell, Inc. in each and every instance.
 |
 |      WARNING:  Unauthorized reproduction of this program as well as
 |      unauthorized preparation of derivative works based upon the
 |      program or distribution of copies by sale, rental, lease or
 |      lending are violations of federal copyright laws and state trade
 |      secret laws, punishable by civil and criminal penalties.
 |
 |***************************************************************************
 |
 |       Storage Management Services (SMS)
 |
 |---------------------------------------------------------------------------
 |
 | $Author$
 | $Modtime:   20/06/2004  $
 |
 | $Workfile:   vfsfsScan.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |      This module is used to:
 |              Define Scan functions in the file system interface layer for VFS file systems.
 +-------------------------------------------------------------------------*/

/* LibC includes */
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <errno.h>

extern int errno;

/* SMS NDK includes */
#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>
#include <smsutapi.h>

/* FS layer specification includes */
#include <zPublics.h>
#include <zParams.h>
#include <zError.h>

/* Internal TSAFS includes */
#include <fsinterface.h>
#include <tsajob.h>
#include <tsalib.h>
#include <tsadset.h>
#include <tsaunicode.h>
#include <smsdebug.h>
#include <wildpath.h>
#include <smsdebug.h>
#include <incexc.h> 
#include <tsaresources.h>

/* External Globals */
extern UINT32 			totalObjectsScannedByTSA;
extern unsigned int     tsazInfoVersion;

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_VFS_InitFileSystemInterfaceLayer"
#define FPTR     FS_VFS_InitFileSystemInterfaceLayer
CCODE FS_VFS_InitFileSystemInterfaceLayer(INT32 connectionID, FS_HANDLE *handle, UINT32 *taskID)
{
	/* This function is a NOP as there is no root key concept in opening/scanning files in VFS */  
    return 0;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_VFS_DeInitFileSystemInterfaceLayer"
#define FPTR     FS_VFS_DeInitFileSystemInterfaceLayer
CCODE FS_VFS_DeInitFileSystemInterfaceLayer( FS_HANDLE handle, UINT32 taskID)
{
	
	/* This function is a NOP as there is no root key concept in opening/scanning files in VFS */  
    return 0;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	FILESYS|VFS_SCAN
#define FNAME   "FS_VFS_InitScan"
#define FPTR	FS_VFS_InitScan
CCODE FS_VFS_InitScan(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence)
{
	CCODE 			 status = 0;
	unsigned char 	*dirPath = NULL;
	int				 pLen;
	FStart();

	/* Inpit parameters check */
	if(!dirEntrySpec || !scanSequence)
	{
		status = FS_PARAMETER_ERROR;
		goto Return;
	}

	/* If we already have a handle then rewind it, happens when a directory is revisited for sub dirs */
	if(dirEntrySpec->handleArray[0].handle.vfsHandle.scanHandle)
	{
	    rewinddir(dirEntrySpec->handleArray[0].handle.vfsHandle.scanHandle);
	}
	else
	{
		/* Create a new DIR handle for scanning the file system */
		status = FS_VFS_FixPathToVFS(dirEntrySpec->parentHandle.vfsHandle.fullPath, dirEntrySpec->uniPath, &dirPath);
		if (status)
			goto Return;
		
		/* open the path for scanning */
		dirEntrySpec->handleArray[0].handle.vfsHandle.scanHandle = opendir(dirPath);
		if (dirEntrySpec->handleArray[0].handle.vfsHandle.scanHandle == NULL)
		{
			FLogError("opendir", errno, dirPath);
			status = NWSMTS_SCAN_ERROR;
			goto Return;
		}

		dirEntrySpec->handleArray[0].handle.vfsHandle.fullPath = dirPath;

		/* Set dirPath to NULL so that it is NOT freed on exit */
		dirPath = NULL;
	}

	/* Copy the directory handle in the scanSequence parameter so that subsequent calls to FS_ScanNextxxx can use this handle */
	scanSequence->scanHandle.vfsHandle.scanHandle = dirEntrySpec->handleArray[0].handle.vfsHandle.scanHandle;
	
	pLen = strlen(dirEntrySpec->handleArray[0].handle.vfsHandle.fullPath) + 1;
	scanSequence->scanHandle.vfsHandle.fullPath = tsaMalloc(pLen);
	if (scanSequence->scanHandle.vfsHandle.fullPath == NULL)
	{
		status = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
	
	memcpy(scanSequence->scanHandle.vfsHandle.fullPath, dirEntrySpec->handleArray[0].handle.vfsHandle.fullPath, pLen);
	scanSequence->searchPattern = search;
	scanSequence->informationMask = infoMask;

Return:
	if (dirPath)
		tsaFree(dirPath);

	FEnd(status);
	return status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE	VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_ReInitScan"
#define FPTR	FS_VFS_ReInitScan
CCODE FS_VFS_ReInitScan(FS_SCAN_SEQUENCE *scanSequence)
{
    STATUS  status = 0;

    if(scanSequence->scanHandle.vfsHandle.scanHandle)
        rewinddir(scanSequence->scanHandle.vfsHandle.scanHandle);
    else
		status = FS_INTERNAL_ERROR;

    if(status)
	{
		FLogError("rewinddir", status, NULL);
	}

	return status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_ScanNextFileEntry"
#define FPTR 	FS_VFS_ScanNextFileEntry
CCODE FS_VFS_ScanNextFileEntry(FS_SCAN_SEQUENCE *fileScanSequence, FS_FILE_OR_DIR_INFO *fileInfo )
{
	struct dirent 	*dirEntry;
	int 			 entryFound = FALSE, parentPathLen;
	UINT32			 searchLength;
	unsigned char	*searchPattern = NULL, *fullPath = NULL;
	struct stat 	 statBuf;
	STATUS           status;
	FStart();

	/**     Check for any invalid parameters passed in **/
	if(!fileScanSequence || !fileInfo || !fileInfo->information)
	{
	    status = FS_PARAMETER_ERROR;
	    goto Return;
	}

	/* The search pattern would be in Unicode convert this to MBCS for use, this is used for single file backups */
	if (fileScanSequence->searchPattern)
	{
		status = SMS_UnicodeToByte(fileScanSequence->searchPattern, &searchLength, &searchPattern, NULL);
		if (status)
		{
			goto Return;
		}
	}

	/* Allocate fullPath to atlease the full path length of the parent path */
	parentPathLen = strlen(fileScanSequence->scanHandle.vfsHandle.fullPath) + 1;
	fullPath = tsaMalloc(parentPathLen);
	if (!fullPath)
	{
		status = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
	
	while(!entryFound)
	{
		/* Read the next entry from the directory */
		if((dirEntry = readdir(fileScanSequence->scanHandle.vfsHandle.scanHandle)) == NULL)
			break;

		/* Exclude the two back pointer nodes */
		if(strcmp(dirEntry->d_name, ".") && strcmp(dirEntry->d_name, ".."))
		{
			/* Reallocate the full path so that it can occupy both parent and child.
			 * Copy the parent and child to get the full path information */
			fullPath = tsaRealloc(fullPath, parentPathLen + strlen(dirEntry->d_name) + strlen("/"));
			if (!fullPath)
			{
				status = NWSMTS_OUT_OF_MEMORY;
				goto Return;
			}
			
			memcpy(fullPath, fileScanSequence->scanHandle.vfsHandle.fullPath, parentPathLen);
			if(fullPath[parentPathLen-2] != '/' )
				strcat(fullPath, "/");
			strcat(fullPath, dirEntry->d_name);

			/* Get the stat buffer for this node to determine, file type or if excluded as FileMatchCriteria */
			status = lstat(fullPath, &statBuf);
			if (status == -1)
			{
				if(errno == EACCES) /*no permissions so skip*/
					continue;
				
				FLogError("lstat", errno, fullPath);
				status = NWSMTS_SCAN_FILE_ENTRY_ERR;
				goto Return;
			}

			/* Check if the current node is a file */
			if (S_ISDIR(statBuf.st_mode))
				continue;

			/* Check if we have read access to the node, no read access, then open/read will fail, hence exclude it */
			/*Dangling links have to be skipped, since access will try to access the data pointed and not the link*/
			if (!S_ISLNK(statBuf.st_mode) && access(fullPath, R_OK))
				continue;
			
			/* Check if the fileMatchCriteria is met. We only check for hidden files, system files are not supported on Linux */
			if (fileInfo->DirMatchCriteria & FS_MATCH_NON_HIDDEN && dirEntry->d_name[0] == '.')
				continue;

			/* Check if the file matches the searchPattern, the pattern is not a wild card pattern */
			if (searchPattern && NWSMMatchName(NFSNameSpace, searchPattern, dirEntry->d_name, FALSE))
				continue;

			/* Found a file proceed with getting other information */
			entryFound = TRUE;
		}
	}

	if(entryFound != FALSE)
	{
		FS_VFS_PopulatezInfoFromVFS(fileInfo->information, fileInfo->sizeOfInformation, &statBuf, fullPath, fileScanSequence->informationMask, &fileInfo->fileType );
	}
	else
		status = NWSMTS_SCAN_FILE_ENTRY_ERR;

	if(!status) {
		fileInfo->isDirectory = FALSE;
		fileInfo->isVolume = FALSE;
		totalObjectsScannedByTSA++;
	}

Return:
	if (searchPattern)
		tsaFree(searchPattern);

	if (fullPath)
		tsaFree(fullPath);
	
	FEnd(status);
	return (CCODE)status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_ScanNextDirectoryEntry"
#define FPTR 	FS_VFS_ScanNextDirectoryEntry
CCODE FS_VFS_ScanNextDirectoryEntry(FS_SCAN_SEQUENCE *dirScanSequence, FS_FILE_OR_DIR_INFO *dirInfo )
{
	struct dirent 	*dirEntry;
	int 			 entryFound = FALSE, parentPathLen;
	UINT32			 searchLength;
	unsigned char	*searchPattern = NULL, *fullPath = NULL;
	struct stat 	 statBuf;
	STATUS			 status = 0;
	FStart();

	/**     Check for any invalid parameters passed in **/
	if(!dirScanSequence || !dirInfo || !dirInfo->information)
	{
		status = FS_PARAMETER_ERROR;
		goto Return;
	}

	/* The search pattern would be in Unicode convert this to MBCS for use, this is used for single file backups */
	if (dirScanSequence->searchPattern)
	{
		status = SMS_UnicodeToByte(dirScanSequence->searchPattern, &searchLength, &searchPattern, NULL);
		if (status)
		{
			goto Return;
		}
	}

	/* Allocate fullPath to atlease the full path length of the parent path */
	parentPathLen = strlen(dirScanSequence->scanHandle.vfsHandle.fullPath) + 1;
	fullPath = tsaMalloc(parentPathLen);
	if (!fullPath)
	{
		status = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
	
	while(!entryFound)
	{
		/* Read the next entry from the directory */
		if((dirEntry = readdir(dirScanSequence->scanHandle.vfsHandle.scanHandle)) == NULL)
			break;

		/* Exclude the two back pointer nodes */
		if(strcmp(dirEntry->d_name, ".") && strcmp(dirEntry->d_name, ".."))
		{
			/* Reallocate the full path so that it can occupy both parent and child.
			 * Copy the parent and child to get the full path information */
			fullPath = tsaRealloc(fullPath, parentPathLen + strlen(dirEntry->d_name) + strlen("/"));
			if (!fullPath)
			{
				status = NWSMTS_OUT_OF_MEMORY;
				goto Return;
			}
			
			memcpy(fullPath, dirScanSequence->scanHandle.vfsHandle.fullPath, parentPathLen);
			if(fullPath[parentPathLen-2] != '/' )
				strcat(fullPath, "/");
			strcat(fullPath, dirEntry->d_name);

			/* Get the stat buffer for this node to determine, file type or if excluded as DirectoryMatchCriteria */
			status = lstat(fullPath, &statBuf);
			if (status == -1)
			{
				if(errno == EACCES) /*no permissions*/
					continue;

				FLogError("lstat", errno, fullPath);
				status = NWSMTS_SCAN_FILE_ENTRY_ERR;
				goto Return;
			}

			/* Check if the current node is a directory */
			if (!S_ISDIR(statBuf.st_mode))
				continue;

			/* Check if we have read access to the node, no read access, then open/read will fail, hence exclude it */
			if (access(fullPath, R_OK))
				continue;

			/* Check if the fileMatchCriteria is met. We only check for hidden files, system files are not supported on Linux */
			if (dirInfo->DirMatchCriteria & FS_MATCH_NON_HIDDEN && dirEntry->d_name[0] == '.')
				continue;

			/* Check if the file matches the searchPattern, the pattern is not a wild card pattern */
			if (searchPattern && NWSMMatchName(NFSNameSpace, searchPattern, dirEntry->d_name, FALSE))
				continue;

			/* Found a dir proceed with getting other information */
			entryFound = TRUE;
		}
	}

	if(entryFound != FALSE)
	{
		FS_VFS_PopulatezInfoFromVFS(dirInfo->information, dirInfo->sizeOfInformation, &statBuf, fullPath, dirScanSequence->informationMask, &dirInfo->fileType );
	}
	else
		status = NWSMTS_SCAN_FILE_ENTRY_ERR;

	if(!status)     {
		dirInfo->isDirectory = TRUE;    
		dirInfo->isVolume = FALSE;
		totalObjectsScannedByTSA++;
	}

Return:
	if (fullPath)
		tsaFree(fullPath);

	FEnd((CCODE)status);
	return (CCODE)status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_EndScan"
#define FPTR 	FS_VFS_EndScan
CCODE FS_VFS_EndScan(FS_SCAN_SEQUENCE *scanSequence)
{
	STATUS  status=0;
	FStart();

	if (scanSequence->scanHandle.vfsHandle.scanHandle != 0)    
	{
		status = closedir(scanSequence->scanHandle.vfsHandle.scanHandle);
		if (status == -1)
			FLogError("closedir", errno, NULL);
		
		scanSequence->scanHandle.vfsHandle.scanHandle = NULL;
		tsaFree(scanSequence->scanHandle.vfsHandle.fullPath);
		scanSequence->scanHandle.vfsHandle.fullPath = NULL;
	}

	FEnd((CCODE)status);
	return (CCODE)status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_GetFilePrimaryUNIName"
#define FPTR     FS_VFS_GetFilePrimaryUNIName
unicode* FS_VFS_GetFilePrimaryUNIName(void *information, UINT32 retNameSpace, unicode *cfsNameSpaceInfo[])
{
	LONG	*fileNames;
	zInfo_s	*info = (zInfo_s *)(information);
	NINT	 nameSpace = info->primaryNameSpaceID;

	if (info->infoVersion > zINFO_VERSION_B)
	{
		return NULL;
	}
	
	if (nameSpace >= info->names.numEntries)
	{
		return NULL;
	}
	
	if (info->names.fileNameArray == 0)
	{
		return NULL;
	}
	
	fileNames = (LONG *)&((BYTE *)info)[info->names.fileNameArray];
	if (fileNames[nameSpace] == 0)
	{
		return NULL;
	}
	
	return (unicode *)&(((BYTE *)info)[fileNames[nameSpace]]);
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_GetFileNameSpaceUNIName"
#define FPTR     FS_VFS_GetFileNameSpaceUNIName
unicode* FS_VFS_GetFileNameSpaceUNIName(void *information, UINT32 retNameSpace, unicode *cfsNameSpaceInfo[])
{
	LONG		*fileNames;
	zInfo_s 	*info = (zInfo_s *)(information);

	if (info->infoVersion > zINFO_VERSION_B)
	{
		return NULL;
	}
	
	if (retNameSpace >= info->names.numEntries)
	{
		return NULL;
	}
	
	if (info->names.fileNameArray == 0)
	{
		return NULL;
	}
	
	fileNames = (LONG *)&((BYTE *)info)[info->names.fileNameArray];
	if (fileNames[retNameSpace] == 0)
	{
		return NULL;
	}
	
	return (unicode *)&(((BYTE *)info)[fileNames[retNameSpace]]);
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_GetVariableData"
#define FPTR     FS_VFS_GetVariableData
void* FS_VFS_GetVariableData(void *information)
{
	VARIABLE_DATA *varData = NULL;
	LONG offSetToData = 0;
	zInfo_s 	*info = (zInfo_s *)(information);

	if (info->infoVersion > zINFO_VERSION_B)
	{
		return NULL;
	}
	
	if (info->unixNS.offsetToData == 0)
	{
		return NULL;
	}
	//the last but one 32 bits of the variable data of UnixNS is iNode number
	offSetToData = info->unixNS.offsetToData;
	 varData = (VARIABLE_DATA*)&((BYTE *)info)[offSetToData];
	
	return varData;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_RestoreInitScan"
#define FPTR     FS_VFS_RestoreInitScan
CCODE FS_VFS_RestoreInitScan(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence)
{
	STATUS          	status = 0;
	UINT32          	requestedRights = 0;
	punicode        	uniPathName = NULL;
	punicode        	uniVolName = NULL;
	punicode            tmpPathPtr = NULL;
	Key_t               tmpVolKey = 0;
	Key_t               tmpDirKey = 0;
	UINT32				nameSpace;
	unsigned char 		*dirPath = NULL;
	FStart();

	if(!dirEntrySpec || !scanSequence)
	{
		status = FS_PARAMETER_ERROR;
		goto Return;
	}

	status = FS_VFS_FixPathToVFS(dirEntrySpec->parentHandle.vfsHandle.fullPath, dirEntrySpec->uniPath, &dirPath);
	if (status)
		goto Return;

	/* open the path for scanning */
	dirEntrySpec->handleArray[0].handle.vfsHandle.scanHandle = opendir(dirPath);
	if (dirEntrySpec->handleArray[0].handle.vfsHandle.scanHandle == NULL)
	{
		FLogError("opendir", errno, NULL);
		status = NWSMTS_SCAN_ERROR;
		goto Return;
	}

	dirEntrySpec->handleArray[0].handle.vfsHandle.fullPath = dirPath;
	dirPath = NULL;
	/**     Copy the directory handle in the scanSequence parameter so that subsequent calls to FS_ScanNextFileEntry can use this handle **/
	scanSequence->scanHandle.vfsHandle.scanHandle = dirEntrySpec->handleArray[0].handle.vfsHandle.scanHandle;
	scanSequence->scanHandle.vfsHandle.fullPath = tsaMalloc(strlen(dirEntrySpec->handleArray[0].handle.vfsHandle.fullPath) + 1);
	strcpy(scanSequence->scanHandle.vfsHandle.fullPath, dirEntrySpec->handleArray[0].handle.vfsHandle.fullPath);

	scanSequence->searchPattern = search;
	scanSequence->informationMask = infoMask;

Return:

	if (uniVolName)
		tsaFree(uniVolName);
	if (dirPath)
		tsaFree(dirPath);
        
	FEnd((CCODE)status);
	return (CCODE)status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_FixPathToVFS"
#define FPTR     FS_VFS_FixPathToVFS
CCODE FS_VFS_FixPathToVFS(unsigned char *parentPath, unicode *uniPath, unsigned char **fixedPath)
{
	CCODE				 cCode = 0;
	UINT32				 pathLength = 0;
	unsigned char		*locPath = NULL, *lastSlash = NULL;
	char				sep2[4]={0};
	BOOL				catSep2=FALSE;

	FStart();
	
	if(uniPath==0)  
	{
		cCode =NWSMTS_INVALID_PATH;
		goto Return;
	}	
	cCode = SMS_UnicodeToByte(uniPath, &pathLength, &locPath, NULL);
	if(cCode)
	{
		FTrack(INTERNAL_ERROR,DC_CRITICAL|DC_COMPACT, "Conversion Error\n");
		goto Return;
	}
	if(parentPath)
	{
		pathLength=strlen(parentPath);
		
		lastSlash = strrchr(parentPath, '/');
		if(lastSlash && *(lastSlash + 1) != '\0')
		{
			catSep2=TRUE;
			pathLength +=1;
		}
	}	
	pathLength += strlen(locPath)+1;

	*fixedPath = tsaMalloc(pathLength);
	if(!*fixedPath)
	{
		cCode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
	if(parentPath)
	{
		strcpy(*fixedPath, parentPath);
		if(catSep2)
			strcat(*fixedPath, "/");		
		strcat(*fixedPath, locPath);
	}
	else
	{
		strcpy(*fixedPath, locPath);
	}
	
Return:
	if (locPath)
		tsaFree(locPath);
	FEnd(cCode);
	return cCode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    VFS_SCAN
#define FNAME   "FS_VFS_SetFileDirSearchAttributes"
#define FPTR     FS_VFS_SetFileDirSearchAttributes
void FS_VFS_SetFileDirSearchAttributes(JOB *newJob)
{
	newJob->dirInfo.FileMatchCriteria=0;
	newJob->dirInfo.DirMatchCriteria=0;

	if(newJob->scanControl->scanType & NWSM_EXCLUDE_HIDDEN_CHILDREN)
		newJob->dirInfo.FileMatchCriteria= FS_MATCH_NON_HIDDEN;
	if(newJob->scanControl->scanType & NWSM_EXCLUDE_SYSTEM_CHILDREN)
		newJob->dirInfo.FileMatchCriteria=newJob->dirInfo.FileMatchCriteria|FS_MATCH_NON_SYSTEM;
	 

	if(newJob->scanControl->scanType & NWSM_EXCLUDE_HIDDEN_PARENTS)
		newJob->dirInfo.DirMatchCriteria= FS_MATCH_NON_HIDDEN;
	if(newJob->scanControl->scanType & NWSM_EXCLUDE_SYSTEM_PARENTS)
		newJob->dirInfo.DirMatchCriteria=newJob->dirInfo.DirMatchCriteria|FS_MATCH_NON_SYSTEM; 

	return;
}
