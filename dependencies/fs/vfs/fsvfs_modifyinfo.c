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
 | Author:    Kalidas Balakrishnan $  
 | Modtime:   29 Jun 2004 21:20:16  $
 |
 | Workfile:   fsvfs_ModifyInfo.c  $
 | Revision:   1.0  
 |
 |---------------------------------------------------------------------------
 |      This module is used to:
 |              Modify File/dir info based on backed up data
 +-------------------------------------------------------------------------*/


#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <utime.h>
#include <errno.h>

#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>
#include <tsadset.h>

#include "fsinterface.h"
#include <smsdebug.h>

extern unsigned int tsazInfoVersion ;
extern int errno;
/******************************************************************************************************************/


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CRITICAL  |  COMPACT  |  FILESYS  |  ( RESTORE | BASIC_CHARACTERISTICS | NFS_CHARCATERISTICS | MAC_CHARACTERICTICS | EXTENDED_ATTRIBUTES )
#define FNAME   "FS_VFS_ModifyInfo"
#define FPTR     FS_VFS_ModifyInfo
CCODE FS_VFS_ModifyInfo(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD modifyInfoMask, UINT32 bufferSize, void *buffer)
{
	STATUS status;
	BOOL isMaxSpace = FALSE;
        
	if(!buffer || bufferSize<=0)
	{
	        status = FS_PARAMETER_ERROR;
	        goto Return;
	}

	/* set the mode for the data set*/
	if(fileOrDirHandle->fileType != FS_VFS_HL_TO_SL && fileOrDirHandle->fileType != FS_VFS_SOFT_LINK && ((zInfo_s *)buffer)->std.fileAttributesModMask != 0xFFFFFFFF)
		status = chmod(fileOrDirHandle->handleArray[0].handle.vfsHandle.fullPath, ((zInfo_s *)buffer)->unixNS.info.fMode);

	if(status)
	{
		FLogError("chmod", errno, perror("ModifyInfo")) ;
	}

	/* set the atime and mtime for the data set*/
	if(fileOrDirHandle->fileType != FS_VFS_HL_TO_SL && fileOrDirHandle->fileType != FS_VFS_SOFT_LINK && !status)
	{
		struct utimbuf tbuf;
		memset(&tbuf, 0, sizeof(struct utimbuf));
		tbuf.actime = ((zInfo_s *)buffer)->time.accessed;
		tbuf.modtime = ((zInfo_s *)buffer)->time.modified;	
		status = utime(fileOrDirHandle->handleArray[0].handle.vfsHandle.fullPath, &tbuf);
	}

	if(status)
	{
		FLogError("utime", errno, perror("ModifyInfo")) ;
	}
	
	/*set the uid and gid*/
	if(!status)
	{
		if(fileOrDirHandle->fileType == FS_VFS_HL_TO_SL || fileOrDirHandle->fileType == FS_VFS_SOFT_LINK)
			status = lchown(fileOrDirHandle->handleArray[0].handle.vfsHandle.fullPath, ((zInfo_s *)buffer)->unixNS.info.nfsUID, ((zInfo_s *)buffer)->unixNS.info.nfsGID);
		else
			status = chown(fileOrDirHandle->handleArray[0].handle.vfsHandle.fullPath, ((zInfo_s *)buffer)->unixNS.info.nfsUID, ((zInfo_s *)buffer)->unixNS.info.nfsGID);
	}
	
	if(status)
	{
		FLogError("chown", errno, perror("ModifyInfo")) ;
	}
       
Return :
        return (CCODE)status;
}


#undef FTYPE
#undef FNAME
#undef FPTR
#define FTYPE RESTORE |FILESYS|BASIC_CHARACTERISTICS|CRITICAL|COMPACT
#define FNAME "FS_VFS_SetReadyFileDirForRestore"
#define FPTR FS_VFS_SetReadyFileDirForRestore
CCODE FS_VFS_SetReadyFileDirForRestore(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, 
										  UINT32	 isPathFullyQualified, 
										  unicode 	*dSetName,
										  UINT32	 rFlags,
										  UINT32 	 createMode, 
										  FS_ACCESS_RIGHTS accessRights,
										  FS_ATTRIBUTES 	attributes)
{
    CCODE		 ccode = 0;
    zInfoB_s    *objectInfo = NULL;
    UINT32       infoMask = FS_MOD_FILE_ATTRIBUTES;
	UINT32		 objectSize;

        FStart();
	
	/* Get an open handle to the data set after requesting only for read access */
	accessRights = S_IRUSR;
	
	ccode = FS_VFS_Create(fileOrDirHandle,
					createMode,
					accessRights,
					attributes,
					FS_PRIMARY_DATA_STREAM,
					&fileOrDirHandle->handleArray[0].handle,  
					(void *)dSetName, 
					rFlags,
					isPathFullyQualified,
					NULL,
					(FS_HL_TABLE**)NULL);
	if(ccode)
		goto Return;

	fileOrDirHandle->streamCount ++;

	/* Get the current attributes for this data set */
	objectInfo = (zInfoB_s *)tsaMalloc(sizeof(zInfoB_s));
	if(!objectInfo)
	{
		ccode = NWSMTS_OUT_OF_MEMORY;
    	goto CleanupExit;
	}
	objectSize = sizeof(zInfoB_s);
	
	ccode = FS_VFS_GetInfo(fileOrDirHandle, FS_GET_STD_INFO, objectSize, objectInfo);
	if(ccode)
		goto CleanupExit;
		
    attributes = FS_GetFileAttributes(objectInfo);

	/* If the file has any of the attributes that prevents a write access open mode, clear them */
	//PTBD - need to modify the defines based on the VFS info
    if((attributes & FS_READ_ONLY) ||(attributes & FS_HIDDEN) || (attributes & FS_SYSTEM) || (attributes & FS_RENAME_INHIBIT) || (attributes & FS_DELETE_INHIBIT))
    {
		objectInfo->std.fileAttributes = attributes;
        objectInfo->std.fileAttributes &= ~(FS_READ_ONLY|FS_HIDDEN|FS_SYSTEM|FS_RENAME_INHIBIT|FS_DELETE_INHIBIT);
        infoMask = FS_MOD_FILE_ATTRIBUTES;

	/* Need not modify info for sockets and hardlinks, as sockets are not supported and for hardlinks, it would have been already set*/	
	if(fileOrDirHandle->fileType != FS_VFS_SOCKET && fileOrDirHandle->fileType != FS_VFS_HARD_LINK)
	        ccode = FS_VFS_ModifyInfo(fileOrDirHandle,
                         	infoMask,
                         	(tsazInfoVersion == zINFO_VERSION_B) ? sizeof(zInfoB_s) : sizeof(zInfo_s),  
                         	objectInfo);
    }

CleanupExit:
	ccode = FS_VFS_Close(fileOrDirHandle, DATASET_CLOSE_ALL_STREAM_HANDLES);
	fileOrDirHandle->streamCount --;
	
Return:        
    if(objectInfo)
		tsaFree(objectInfo);
    
    FEnd(ccode);
    return ccode;

#ifdef OLD
	/* Get an open handle to the data set after requesting only for read access */
	accessRights &= ~FS_WRITE_ACCESS;


        attributes = FS_GetFileAttributes(fileInformation);
                //currently setting only for read only
        if((attributes & FS_READ_ONLY)) // ||(attributes & FS_HIDDEN) || (attributes & FS_SYSTEM) || (attributes & FS_RENAME_INHIBIT) || (attributes & FS_DELETE_INHIBIT))
        {
                accessRights = ~(S_IRUSR | S_IWUSR);
                //open the file
		ccode = open(fileOrDirHandle->handleArray[0].handle.vfsHandle.fullPath, O_CREAT | O_RDWR, accessRights);
		if(ccode)
		{
			FLogError("open", ccode, NULL);
			ccode=NWSMTS_OUT_OF_MEMORY;
			goto Return;
		}

                objectInfo = (zInfoB_s *) tsaMalloc(sizeof(zInfoB_s));
                if(!objectInfo)
                {
                        ccode=NWSMTS_OUT_OF_MEMORY;
                        goto Return;
                }
				objectInfo->std.fileAttributes = attributes;
                objectInfo->std.fileAttributes &= ~(FS_READ_ONLY); //|FS_HIDDEN|FS_SYSTEM|FS_RENAME_INHIBIT|FS_DELETE_INHIBIT);
                fileOrDirHandle->handleArray[0].handle.vfsHandle.fileDescriptor = ccode;
                infoMask=FS_MOD_FILE_ATTRIBUTES;
                
                ccode = FS_VFS_ModifyInfo(
                                                        fileOrDirHandle,
                                                        infoMask,
                                                        (tsazInfoVersion == zINFO_VERSION_B) ? sizeof(zInfoB_s) : sizeof(zInfo_s),  
                                                        objectInfo
                                                        );
                //FS_VFS_ModifyInfo expected to map the proper sms code
        }
        
Return:
        
        if(tmpKey)
                zClose(tmpKey);
        if(objectInfo)
                tsaFree(objectInfo);
        
        FEnd(ccode);
        return ccode;
#endif		
}

