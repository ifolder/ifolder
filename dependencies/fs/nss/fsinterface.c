/****************************************************************************
 |
 |  (C) Copyright 2002 Novell, Inc.
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
 | $Modtime:   18Jul2002 $
 |
 | $Workfile:   fsinterface.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		- contains file system layer functions
 ****************************************************************************/

/* Includes */
#include <string.h>
#ifdef N_PLAT_NLM
#include <nwstring.h>
#endif

#include <fs_sidf.h>
#include <smstserr.h>
#include <tsalib.h>
#include <fsinterface.h>
#include <smsdebug.h>


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    SMS_INTERNAL
#define FNAME   "FS_AllocateModifyInfo"
#define FPTR     FS_AllocateModifyInfo
void *FS_AllocateModifyInfo(void  *info, UINT32 *size)
{
         if(*(char **)info = tsaCalloc(1, sizeof(FS_NSS_INFO)))
            *size = sizeof(FS_NSS_INFO) ;
         return *(char **)info ;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NO_DEBUG_INFO
#define FNAME   "FS_ResetModifyInfo"
#define FPTR     FS_ResetModifyInfo

void  FS_ResetModifyInfo(void  *info)
{
         memset(info, '0', sizeof(FS_NSS_INFO)) ;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NO_DEBUG_INFO
#define FNAME   "FS_AllocateMACInfo"
#define FPTR     FS_AllocateMACInfo

void *FS_AllocateMACInfo(FS_FILE_OR_DIR_INFO  *info)
{
    if(  ! info->information && 
        !FS_AllocateModifyInfo(&info->information, &info->sizeOfInformation))
        return 0 ;

    return & ((zInfo_s *)info->information)->macNS.info ;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NO_DEBUG_INFO
#define FNAME   "FS_AllocateNFSInfo"
#define FPTR     FS_AllocateNFSInfo

void *FS_AllocateNFSInfo(FS_FILE_OR_DIR_INFO  *info)
{
    if(  ! info->information && 
        !FS_AllocateModifyInfo(&info->information, &info->sizeOfInformation))
        return 0 ;

    return & ((zInfo_s *)info->information)->unixNS.info ;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    SMS_INTERNAL|FILESYS
#define FNAME   "FS_SetInheritedRights"
#define FPTR     FS_SetInheritedRights

CCODE FS_SetInheritedRights(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, void* info)
{
	STATUS status;
	FS_NSS_INFO *nssInfo = (FS_NSS_INFO *)info ;

	FStart();
	if(!fileOrDirHandle)
	{
		status = FS_PARAMETER_ERROR;
		goto Return;
	}
	status = zSetInheritedRightsMask(
				fileOrDirHandle->handleArray[0].handle.nssHandle,
				zNILXID,
                nssInfo->inheritedRights				
				);
	if(status)
	{
	    FLogError("zSetInheritedRightsMask", status, 0);
	}
Return :
	FEnd((CCODE)status);
	return (CCODE)status;
}
