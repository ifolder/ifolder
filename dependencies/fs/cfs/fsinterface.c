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

#include <fs_sidf.h>
#include <smstserr.h>
#include <tsalib.h>
#include <fsinterface.h>
#include <smsdebug.h>

#include <nwstring.h>


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    SMS_INTERNAL
#define FNAME   "FS_CFS_AllocateModifyInfo"
#define FPTR     FS_CFS_AllocateModifyInfo

void * FS_CFS_AllocateModifyInfo(void  *info, UINT32 *size)
{

	
	*(void **)info =(void *) tsaCalloc(1, sizeof(NWMODIFY_INFO));
    if((*(void **)info))
      *size = sizeof(NWMODIFY_INFO);
	return *(void**)info;   
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    SMS_INTERNAL
#define FNAME   "FS_CFS_AllocateMACInfo"
#define FPTR     FS_CFS_AllocateMACInfo
void * FS_CFS_AllocateMACInfo(void  **info, UINT32 *size)
{
	/*
	void *temp ;
	SuppressCompilerWarning(1, info, size) ;

	temp = tsaCalloc(sizeof(MAC_NAME_SPACE_INFO), 1);

	return temp ;
	*/

	*info =(void *)tsaCalloc(1, sizeof(MAC_NAME_SPACE_INFO));
	if(*info)
 	  *size = sizeof(MAC_NAME_SPACE_INFO);
    return *info;   
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    SMS_INTERNAL
#define FNAME   "FS_CFS_AllocateNFSInfo"
#define FPTR     FS_CFS_AllocateNFSInfo
void * FS_CFS_AllocateNFSInfo(void **info, UINT32 *size)
{
 	*info =(void *)tsaCalloc(1, sizeof(SIDF_NFS_INFO));
 	if(*info)
 	  *size = sizeof(SIDF_NFS_INFO);

    return *info;   
    
}
