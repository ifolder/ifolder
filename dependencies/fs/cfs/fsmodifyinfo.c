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
 | Author:    Administrator  
 | Modtime:   27 Mar 2002 17:04:16  
 |
 | Workfile:   fsModifyInfo.c  
 | Revision:   1.0  
 |
 |---------------------------------------------------------------------------
 |      This module is used to:
 |              Initialise TSA
 +-------------------------------------------------------------------------*/


#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>
#include <tsaresources.h>

#include <zPublics.h>
#include <zParams.h>
#include <zError.h>
#include <zOmni.h>

#include "filhandle.h"

#include "fsinterface.h"
#include "cfsdefines.h"
#include "tsadset.h"
#include "smsmac.h"
#include "tsalib.h"
#include "fs_sidf.h"
#include <tsaunicode.h>
#include "customdebug.h"
#include <smsdebug.h> 
#include <restore.h> 

extern unsigned int tsazInfoVersion;


#undef FTYPE
#undef FNAME
#undef FPTR
#define FTYPE RESTORE |FILESYS|BASIC_CHARACTERISTICS|CRITICAL|COMPACT
#define FNAME "FS_SetDirRestrictions"
#define FPTR FS_SetDirRestrictions
CCODE FS_SetDirRestrictions(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, void *buffer) 
{
    CCODE           ccode;
    UINT32          fileDirLength=0;
    QUAD            modifyInfoMask = 0;
    char            *path=NULL;
    void			*savePtr=NULL;
    UINT32			volumeNumber=0;
    void            *info=NULL;
    UINT32          size=0;
    UINT32			nameSpace;
    UINT32		    saveNSpace;
	BOOL saveNSpaceIsSet = FALSE;
	UNICODE_CONTEXT uCtx = {0};

    FStart();

    nameSpace = MapToNSSNameSpace(fileOrDirHandle->nameSpace);
	fileOrDirHandle->cfsStruct.cfsNameSpace = nameSpace;
    
    modifyInfoMask |= NWMODIFY_MAXIMUM_SPACE;

	uCtx.fileSystem = NETWAREPSSFILESYSTEM;
	uCtx.nameSpace = nameSpace;
    ccode = SMS_UnicodeToByte(fileOrDirHandle->uniPath, &fileDirLength, &path, &uCtx);  
    if(ccode)
    {       
        ccode = NWSMTS_INTERNAL_ERROR;
        goto Return;
    }       
    path[fileDirLength] =NULL;
    if(strlen(path)>255)//because legacy having a limitation of 255 char
    {
        ccode=NWSMTS_INVALID_PATH;
        goto Return;
    }

    //save path and namespace
    savePtr = fileOrDirHandle->uniPath;
    fileOrDirHandle->uniPath = (unicode*)path;
    saveNSpace = fileOrDirHandle->nameSpace;
	saveNSpaceIsSet = TRUE;
    fileOrDirHandle->nameSpace=nameSpace;
    
    ccode = getVolumeNumber((void*)path, nameSpace, &volumeNumber);
    if(ccode)
		goto Return;

    fileOrDirHandle->cfsStruct.volumeNumber = volumeNumber;
    
    ccode = FS_CFS_GetDirNumber(fileOrDirHandle);
    if(ccode)
		goto Return;
    
    FS_CFS_AllocateModifyInfo(&info, &size);
    if(!info)
    {
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
    }
    
    ((NWMODIFY_INFO *)info)->MaximumSpace= ((zInfoB_s *)buffer)->dirQuota.quota;//This will work upto 4GB ater that the dat will truncated
    ccode = FS_CFS_ModifyInfo(fileOrDirHandle, modifyInfoMask,  size, info);        
    if(ccode)
    	goto Return;
    
Return:
	
	//restore path and namespace
	if(savePtr)
		fileOrDirHandle->uniPath = savePtr;
	if(saveNSpaceIsSet)
		fileOrDirHandle->nameSpace = saveNSpace;
	
    if(info) 
    	tsaFree(info);
    if(path) 
    	tsaFree(path);
    FEnd(ccode);
    return ccode;
}

#undef FTYPE
#undef FNAME
#undef FPTR
#define FTYPE RESTORE |FILESYS|BASIC_CHARACTERISTICS|CRITICAL|COMPACT
#define FNAME "FS_CFS_ModifyInfo"
#define FPTR FS_CFS_ModifyInfo
CCODE FS_CFS_ModifyInfo(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD modifyInfoMask, UINT32 bufferSize, void *buffer)
{
    CCODE                    ccode=0;
    UINT32                    searchAttributes=0;
    char                      *path=NULL;
    UINT32					  fileDirLength=0;	
    NWHANDLE_PATH           *pathInfo=NULL;
    LONG                      infoMask = modifyInfoMask;
    UINT32					  nameSpace;

    FStart();

	nameSpace = MapToNSSNameSpace(fileOrDirHandle->nameSpace);
    
    if(!fileOrDirHandle || !buffer)
    {
        ccode = NWSMTS_INVALID_PARAMETER;
        goto Return;
    }

    if(!infoMask)
        goto Return;

		
    if(fileOrDirHandle->isDirectory)
        searchAttributes=0x16; //ALL directories
    else
        searchAttributes=0x06; //All files
    
    pathInfo = (NWHANDLE_PATH *) tsaMalloc(sizeof(NWHANDLE_PATH));
    if(!pathInfo)
    {
        ccode = NWSMTS_OUT_OF_MEMORY;
        goto Return;
    }

    if(!fileOrDirHandle->isDirectory)
	{
		if(fileOrDirHandle->cfsStruct.uniFileDirName)
		{
			if (ccode = SMS_UnicodeToByte(fileOrDirHandle->cfsStruct.uniFileDirName, &fileDirLength, &path, NULL)) 
			{
				ccode=NWSMTS_INTERNAL_ERROR;
				goto Return;
			}	
			path[fileDirLength] =NULL;
		}
		else
		{
	        ccode = NWSMTS_INVALID_PATH;
    	    goto Return;
        }
	}
	else
		path = NULL;

    ccode = NewFillHandleStruct((NWHANDLE_PATH *) pathInfo, path, nameSpace, &(fileOrDirHandle->cfsStruct));
    if(ccode)
      goto Return;


    ccode = GenNSModifyInfo(
                           fileOrDirHandle->cfsStruct.clientConnID,
                           ~(fileOrDirHandle->cfsStruct.clientConnID),
                           pathInfo,
                           nameSpace,                      //expected DOSNameSpace, but no matter 
                           searchAttributes,
                           infoMask,
                           (ModifyInfo *)buffer
                          ); 
    if(ccode)
    {	
    	FLogError("GenNSModifyInfo", ccode, path);
        ccode = NWSMTS_SET_FILE_INFO_ERR;
    }
    else
    {
        FTrack(RESTORE, DC_COMPACT, "Restore file meta data modified\n");
    }

Return:
	
	if(path && fileOrDirHandle->cfsStruct.uniFileDirName)
	    tsaFree(path);          
	if(pathInfo)
		tsaFree(pathInfo);              

	FEnd(ccode);
	return ccode;
}

