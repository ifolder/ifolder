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

#ifdef N_PLAT_NLM
#include "filhandle.h"
#endif

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

extern unsigned int tsazInfoVersion ;

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CRITICAL  |  COMPACT  |  FILESYS  |  ( RESTORE | BASIC_CHARACTERISTICS | NFS_CHARCATERISTICS | MAC_CHARACTERICTICS | EXTENDED_ATTRIBUTES )
#define FNAME   "FS_ModifyInfo"
#define FPTR     FS_ModifyInfo
CCODE FS_ModifyInfo(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD modifyInfoMask, UINT32 bufferSize, void *buffer)
{
	STATUS 		status;
	NWBOOLEAN   isMaxSpace = FALSE ;

	FStart();
        
    if(!buffer || bufferSize<=0)
	{
	        status = FS_PARAMETER_ERROR;
	        goto Return;
	}

    if(modifyInfoMask & FS_MOD_NFS_METADATA)
    {
        /* There are some incompatibilities between NSS and Legacy with respect to NFS files. These incompatibilities have been
           handled in the SMS code as NFS had already data that was populated with information. The firstcreatedflag indicates
           that nfs information is set for the file/dir and therefore no defaults need to be set. The firstCreatedFlag value 1 for 
           legacy and 255 for NSS. So if we see a 255 we set it to 1 before restroing. For NSS the rdev needs to be populated with
           the symbolic information both in the fMode as well as the rDev as per NFS 3.0 standards. So if we are populating the data
           from legacy then we get the link information from the fMode and populate the rdev. The octal 160000 value is used to mask
           the symbolic link information from the fMode as the fMode holds this information. The RDevice field is not used in legacy.
           This translation is done so that we are able to restore legacy backups to nss3.0.
        */
        
        if(((zInfo_s *)buffer)->unixNS.info.firstCreated==1)
        {
                ((zInfo_s *)buffer)->unixNS.info.firstCreated = 255;
                /*
                  This is done so because the NSS NFS module looks at the rdevice field and this needs to be anded with the fmode bit.
                  This is different from legacy.
            */
                ((zInfo_s *)buffer)->unixNS.info.rDev = (((zInfo_s *)buffer)->unixNS.info.fMode) & 0160000;
        }
        /*
          This check is done to restore 4.x legacy file system data as the first created flag is not 
          used in 4.x consistently
        */
        else if(((zInfo_s *)buffer)->unixNS.info.firstCreated==0 && ((zInfo_s *)buffer)->unixNS.info.fMode)
        {
                ((zInfo_s *)buffer)->unixNS.info.rDev = (((zInfo_s *)buffer)->unixNS.info.fMode) & 0160000;
                ((zInfo_s *)buffer)->unixNS.info.firstCreated = 255;
        }

    }   


    ((zInfo_s *)buffer)->std.fileAttributes &= ~(FS_READ_AUDIT | FS_WRITE_AUDIT) ; //not yet supported in nss - 28Jul02

        if (modifyInfoMask & zMOD_FILE_ATTRIBUTES)
        ((zInfo_s *)buffer)->std.fileAttributesModMask  = -1L ; 


    if(isDebugCharacteristics())
    {
        #ifdef SMSDEBUG
        char temp[32] ;

        NWsprintf(temp, "%s%d%s", 
            "DSet: %-0.", SMS_DBG_MAX_BUF_LENGTH - sizeof("DSet:   /\n"), "s%s\n") ;
        FTrack2(RESTORE, DC_CRITICAL|DC_COMPACT, temp, ((TSA_DATA_SET *)fileOrDirHandle->dSet)->dataSetName, \
                    ((TSA_DATA_SET *)fileOrDirHandle->dSet)->isDirectory ? "/" : "" ); 
        #endif
        
        DebugNSSCharacteristics(buffer);
    }


    if(modifyInfoMask & FS_MOD_MAXIMUM_SPACE)
    {
    	if(fileOrDirHandle->isDirectory)
    	{
	    	if(tsazInfoVersion != zINFO_VERSION_B)
		{
		        modifyInfoMask &= ~FS_MOD_MAXIMUM_SPACE ;
		        isMaxSpace = TRUE ;
		}
		else
		{
			SQUAD dirQuota = ((zInfoB_s *)buffer)->dirQuota.quota;
			dirQuota *= (1024 * 4); //changed from 4k representation to bytes
			((zInfoB_s *)buffer)->dirQuota.quota = dirQuota;
		}
	}
	else
		modifyInfoMask &= ~FS_MOD_MAXIMUM_SPACE ;
    }

    status = zModifyInfo(
                        fileOrDirHandle->handleArray[0].handle.nssHandle,
                        zNILXID,
                        modifyInfoMask,
                        bufferSize,
                        tsazInfoVersion ,
                        (zInfoB_s *)buffer
                        );
    if(status)
        FLogError("zModifyInfo", status, 0) ;
#ifdef N_PLAT_NLM
    if(fileOrDirHandle->isDirectory && isMaxSpace) 
    {
        status = FS_SetDirRestrictions(fileOrDirHandle, buffer);
        /* The return value is currently not handled TBD */
        if(status)
	        status = 0;
    }        
#endif 

Return :
	FEnd((CCODE)status);
    return (CCODE)status;
}

#undef FTYPE
#undef FNAME
#undef FPTR
#define FTYPE RESTORE |FILESYS|BASIC_CHARACTERISTICS|CRITICAL|COMPACT
#define FNAME "FS_SetReadyFileDirForRestore"
#define FPTR FS_SetReadyFileDirForRestore
CCODE FS_SetReadyFileDirForRestore(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, 
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
	accessRights &= ~FS_WRITE_ACCESS;
	
	ccode = FS_Create(fileOrDirHandle,
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
	
	ccode = FS_GetInfo(fileOrDirHandle, FS_GET_STD_INFO, objectSize, objectInfo);
	if(ccode)
		goto CleanupExit;
		
    attributes = FS_GetFileAttributes(objectInfo);

	/* If the file has any of the attributes that prevents a write access open mode, clear them */
    if((attributes & FS_READ_ONLY) ||(attributes & FS_HIDDEN) || (attributes & FS_SYSTEM) || (attributes & FS_RENAME_INHIBIT) || (attributes & FS_DELETE_INHIBIT))
    {
		objectInfo->std.fileAttributes = attributes;
        objectInfo->std.fileAttributes &= ~(FS_READ_ONLY|FS_HIDDEN|FS_SYSTEM|FS_RENAME_INHIBIT|FS_DELETE_INHIBIT);
        infoMask = FS_MOD_FILE_ATTRIBUTES;
        
        ccode = FS_ModifyInfo(fileOrDirHandle,
                         	infoMask,
                         	(tsazInfoVersion == zINFO_VERSION_B) ? sizeof(zInfoB_s) : sizeof(zInfo_s),  
                         	objectInfo);
    }

CleanupExit:
	ccode = FS_Close(fileOrDirHandle, DATASET_CLOSE_ALL_STREAM_HANDLES);
	fileOrDirHandle->streamCount --;
	
Return:        
    if(objectInfo)
		tsaFree(objectInfo);
    
    FEnd(ccode);
    return ccode;
}

