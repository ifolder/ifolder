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
 | $Modtime:   27 Mar 2002 17:05:44  $
 |
 | $Workfile:   fsScan.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |      This module is used to:
 |              Define Scan functions in the file system interface layer.
 +-------------------------------------------------------------------------*/

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

#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>
#include <smsutapi.h>

#ifdef N_PLAT_NLM
#include <lfsproto.h>
#endif
#include <customdebug.h>
#include <stdio.h>
#include <stdlib.h>
#include <zPublics.h>
#include <zParams.h>
#include <zError.h>


/* External Globals */
extern UINT32 totalObjectsScannedByTSA;
extern unsigned int     tsazInfoVersion;

/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_InitFileSystemInterfaceLayer"
#define FPTR     FS_InitFileSystemInterfaceLayer
CCODE FS_InitFileSystemInterfaceLayer(INT32 connectionID, FS_HANDLE *handle, UINT32 *taskID)
{
        STATUS status;
        status = zRootKey(connectionID, &handle->nssHandle);
        if(status)
        {
                FLogError("zRootKey", status, NULL);
        }
                status = zBeginTask(handle->nssHandle, 0, (NINT*)taskID);
                
        return (CCODE)status;
}

/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_DeInitFileSystemInterfaceLayer"
#define FPTR     FS_DeInitFileSystemInterfaceLayer
CCODE FS_DeInitFileSystemInterfaceLayer( FS_HANDLE handle, UINT32 taskID)
{
	STATUS status;
	
	status = zEndTask(handle.nssHandle, taskID);
	status = zClose(handle.nssHandle);

	return status;
}



/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|NSS_SCAN
#define FNAME   "FS_InitScan"
#define FPTR     FS_InitScan
CCODE FS_InitScan(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence)
{
    CCODE status = 0;
    FStart();

    if(!dirEntrySpec || !scanSequence)
    {
        status = FS_PARAMETER_ERROR;
        goto Return;
    }

    /**     Get a handle in dirEntrySpec that can be used to scan the specified directory, if directory is already open, use the handle to initialize a scan  **/
    if(dirEntrySpec->handleArray[0].handle.nssHandle)
    {
        status = zWildRewind(dirEntrySpec->handleArray[0].handle.nssHandle);
        if(status)
        {
            FLogError("zWildRewind", status, NULL);
            goto Return;
        }
    }
    else
    {
        status = zOpen(
                    dirEntrySpec->parentHandle.nssHandle,
                    dirEntrySpec->taskID,                                              /* Binds the returned handle to the same task as the parentHandle */
                    dirEntrySpec->nameSpace,
                    dirEntrySpec->uniPath,
                    zRR_SCAN_ACCESS | zRR_DONT_UPDATE_ACCESS_TIME,
                    &dirEntrySpec->handleArray[0].handle.nssHandle);
        if(status==zERR_LINK_IN_PATH)
        {
            status = zOpen(
                        dirEntrySpec->parentHandle.nssHandle,
                        dirEntrySpec->taskID,                                              /* Binds the returned handle to the same task as the parentHandle */
                        dirEntrySpec->nameSpace|zMODE_LINK,
                        dirEntrySpec->uniPath,
                        zRR_SCAN_ACCESS | zRR_READ_ACCESS | zRR_DONT_UPDATE_ACCESS_TIME | SEARCH_LINK_AWARE|SEARCH_OPERATE_ON_LINK,     /* TBDTBD: For now we only need to scan this directory - might need to change this to read/write later */
                        &dirEntrySpec->handleArray[0].handle.nssHandle);
        }
        if(status)      {
                FLogError("zOpen", status, NULL);
				status = NWSMTS_SCAN_ERROR;
                goto Return;
        }
    }

    /**     Copy the directory handle in the scanSequence parameter so that subsequent callst o FS_ScanNextFileEntry can use this handle **/
    scanSequence->scanHandle.nssHandle = dirEntrySpec->handleArray[0].handle.nssHandle;
    scanSequence->searchPattern = search;
    scanSequence->informationMask = infoMask;

Return:
    FEnd(status);
    return status;
}


/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_SCAN|FILESYS
#define FNAME   "FS_ReInitScan"
#define FPTR     FS_ReInitScan
CCODE FS_ReInitScan(FS_SCAN_SEQUENCE *scanSequence)
{
    STATUS  status = 0;

    if(scanSequence->scanHandle.nssHandle)
		status = zWildRewind(scanSequence->scanHandle.nssHandle);
    else
		status = FS_INTERNAL_ERROR;

    if(status)
    {
        FLogError("zWildRewind", status, NULL);
    }

	return status;
}

/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_SCAN|FILESYS
#define FNAME   "FS_ScanNextFileEntry"
#define FPTR     FS_ScanNextFileEntry

CCODE FS_ScanNextFileEntry(FS_SCAN_SEQUENCE *fileScanSequence, FS_FILE_OR_DIR_INFO *fileInfo)
{
    CCODE            status;
    
    /**     Check for any invalid parameters passed in **/
    if(!fileScanSequence || !fileInfo || !fileInfo->information)
    {
        status = FS_PARAMETER_ERROR;
        goto Return;
    }

    status = zWildRead(
                fileScanSequence->scanHandle.nssHandle,
                zPFMT_UNICODE,
                zNTYPE_FILE,                                                                                    /**     Search for file type objects **/
                fileScanSequence->searchPattern,
                zMATCH_NON_DIRECTORY|fileInfo->FileMatchCriteria,                                                               /**     Search only for files - no sub-directories **/
                fileScanSequence->informationMask,
                fileInfo->sizeOfInformation,
                tsazInfoVersion,
                fileInfo->information);
    if (status==zERR_LINK_IN_PATH)
    {
        status = zWildRead(
                    fileScanSequence->scanHandle.nssHandle,
                    zPFMT_UNICODE,
                    zNTYPE_FILE,                                                                                    /**     Search for file type objects **/
                    fileScanSequence->searchPattern,
                    zMATCH_NON_DIRECTORY|fileInfo->FileMatchCriteria|SEARCH_LINK_AWARE|SEARCH_OPERATE_ON_LINK,                                                              /**     Search only for files - no sub-directories **/
                    fileScanSequence->informationMask,
                    fileInfo->sizeOfInformation,
                    tsazInfoVersion,
                    fileInfo->information);
    }

    if(!status) {
        fileInfo->isDirectory = FALSE;
        fileInfo->isVolume = FALSE;
        totalObjectsScannedByTSA++;
    }
    else
    {
        FLogError("zWildRead", status, NULL);       
    }

Return:
    return (CCODE)status;
}


/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_SCAN|FILESYS
#define FNAME   "FS_ScanNextDirectoryEntry"
#define FPTR     FS_ScanNextDirectoryEntry

CCODE FS_ScanNextDirectoryEntry(FS_SCAN_SEQUENCE *dirScanSequence, FS_FILE_OR_DIR_INFO *dirInfo)
{
    CCODE status = 0;
    FStart();
    
    /**     Check for any invalid parameters passed in **/
    if(!dirScanSequence || !dirInfo || !dirInfo->information)
    {
        status = FS_PARAMETER_ERROR;
        goto Return;
    }

    status = zWildRead(
                dirScanSequence->scanHandle.nssHandle,
                zPFMT_UNICODE,
                zNTYPE_FILE,                                                                                    /**     Search for file type objects **/
                dirScanSequence->searchPattern,
                zMATCH_DIRECTORY|dirInfo->DirMatchCriteria,                                                                     /**     Search only for sub-directories **/
                dirScanSequence->informationMask,
                dirInfo->sizeOfInformation,
                tsazInfoVersion,
                dirInfo->information);
    if(!status)     {
        dirInfo->isDirectory = TRUE;    
        dirInfo->isVolume = FALSE;
        totalObjectsScannedByTSA++;
    }
    else
    {
        FLogError("zWildRead", status, 0);
    }


Return:
    FEnd((CCODE)status);
    return (CCODE)status;
}


/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_SCAN|FILESYS
#define FNAME   "FS_EndScan"
#define FPTR     FS_EndScan

CCODE FS_EndScan(FS_SCAN_SEQUENCE *scanSequence)
{
    STATUS  status=0;
    FStart();

    if (scanSequence->scanHandle.nssHandle != 0)    {
		status = zClose(scanSequence->scanHandle.nssHandle);
		scanSequence->scanHandle.nssHandle = 0;
    }
    
    FEnd((CCODE)status);
    return (CCODE)status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_SCAN|FILESYS
#define FNAME   "FS_GetFilePrimaryUNIName"
#define FPTR     FS_GetFilePrimaryUNIName

unicode* FS_GetFilePrimaryUNIName(void *information, UINT32 retNameSpace, unicode *cfsNameSpaceInfo[])
{
	return ((unicode *)zInfoGetFileName(((zInfo_s *)(information)), (NINT)((zInfo_s *)(information))->primaryNameSpaceID));
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_SCAN|FILESYS
#define FNAME   "FS_GetFileNameSpaceUNIName"
#define FPTR     FS_GetFileNameSpaceUNIName

unicode* FS_GetFileNameSpaceUNIName(void *information, UINT32 retNameSpace, unicode *cfsNameSpaceInfo[])
{
	return ((unicode *)zInfoGetFileName(((zInfo_s *)(information)), (NINT)retNameSpace));
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_SCAN|FILESYS
#define FNAME   "FS_RestoreInitScan"
#define FPTR     FS_RestoreInitScan
CCODE FS_RestoreInitScan(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence)
{
	STATUS          	status;
	UINT32          	requestedRights = 0;
	punicode        	uniPathName = NULL;
	punicode        	uniVolName = NULL;
    punicode            tmpPathPtr = NULL;
    Key_t               tmpVolKey = 0;
    Key_t               tmpDirKey = 0;
    UINT32				nameSpace;
    
	FStart();

	nameSpace = MapToNSSNameSpace(dirEntrySpec->nameSpace);

	if(!dirEntrySpec || !scanSequence)
	{
		status = FS_PARAMETER_ERROR;
		goto Return;
	}

	if ((nameSpace == zNSPACE_MAC) || (nameSpace == zNSPACE_UNIX))      
	{
		tmpPathPtr = dirEntrySpec->uniPath;
		
		if(GetPrimaryUniResource(&tmpPathPtr, nameSpace, &uniVolName) == NULL)
		{
			FLogError("NWGetUniVolume", 0, NULL);
			status = FS_INTERNAL_ERROR;
			goto Return;
		}
		
		status = zOpen(dirEntrySpec->parentHandle.nssHandle, dirEntrySpec->taskID, zNSPACE_LONG,
				uniVolName, zRR_SCAN_ACCESS| zRR_DONT_UPDATE_ACCESS_TIME, &tmpVolKey);
		if(status)
		{       
			tmpVolKey = 0;
			goto Return;
		}

		if (tmpPathPtr)
		{
			tmpDirKey = dirEntrySpec->parentHandle.nssHandle;
			dirEntrySpec->parentHandle.nssHandle = tmpVolKey;
			uniPathName = tmpPathPtr;
		}
	}
	else
		uniPathName = dirEntrySpec->uniPath;
                        
	requestedRights = zRR_SCAN_ACCESS|zRR_DONT_UPDATE_ACCESS_TIME;
	status = zOpen(
			dirEntrySpec->parentHandle.nssHandle,
			dirEntrySpec->taskID /*zNSS_TASK*/,
			nameSpace,
			uniPathName,
			requestedRights, 
			&dirEntrySpec->handleArray[0].handle.nssHandle);
        
	if(status==zERR_LINK_IN_PATH)
	{
		status = zOpen(
				dirEntrySpec->parentHandle.nssHandle,
				dirEntrySpec->taskID  /*zNSS_TASK*/,
				nameSpace|zMODE_LINK,
				uniPathName,
				requestedRights|SEARCH_LINK_AWARE|SEARCH_OPERATE_ON_LINK, 
				&dirEntrySpec->handleArray[0].handle.nssHandle);
	}
        
	if( status != zOK )
	{
		FLogError("zOpen", status, NULL);
		goto Return;
	}
        
	/**     Copy the directory handle in the scanSequence parameter so that subsequent callst o FS_ScanNextFileEntry can use this handle **/
	scanSequence->scanHandle.nssHandle = dirEntrySpec->handleArray[0].handle.nssHandle;
	scanSequence->searchPattern = search;
	scanSequence->informationMask = infoMask;

Return:
	if(tmpVolKey)
	{
		zClose(tmpVolKey);
		dirEntrySpec->parentHandle.nssHandle = tmpDirKey;
	}

	if (uniVolName)
		tsaFree(uniVolName);
        
	FEnd((CCODE)status);
	return (CCODE)status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_SCAN
#define FNAME   "FS_SetFileDirSearchAttributes"
#define FPTR     FS_SetFileDirSearchAttributes
void FS_SetFileDirSearchAttributes(JOB *newJob)
{
        newJob->dirInfo.FileMatchCriteria=0;
        newJob->dirInfo.DirMatchCriteria=0;
        
        if(newJob->scanControl->scanType & NWSM_EXCLUDE_HIDDEN_CHILDREN)
          newJob->dirInfo.FileMatchCriteria= zMATCH_NON_HIDDEN;
        if(newJob->scanControl->scanType & NWSM_EXCLUDE_SYSTEM_CHILDREN)
              newJob->dirInfo.FileMatchCriteria=newJob->dirInfo.FileMatchCriteria|zMATCH_NON_SYSTEM;
         
        
        if(newJob->scanControl->scanType & NWSM_EXCLUDE_HIDDEN_PARENTS)
           newJob->dirInfo.DirMatchCriteria= zMATCH_NON_HIDDEN;
        if(newJob->scanControl->scanType & NWSM_EXCLUDE_SYSTEM_PARENTS)
           newJob->dirInfo.DirMatchCriteria=newJob->dirInfo.DirMatchCriteria|zMATCH_NON_SYSTEM; 
}

