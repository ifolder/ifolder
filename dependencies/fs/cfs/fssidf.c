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
 | $Modtime:   03/06/2002 $
 |
 | $Workfile:   SidfPreFormat.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		- TBDTBD
 ****************************************************************************/

/* Includes */
#include <fs_sidf.h>
#include <fsinterface.h>
#include <smstserr.h>
#include <string.h>
#include <smsdebug.h>
#include <tsa_defs.h>

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS | BACKUP
#define FNAME   "FS_CFS_SIDF_ConvertMACInfo"
#define FPTR     FS_CFS_SIDF_ConvertMACInfo
CCODE FS_CFS_SIDF_ConvertMACInfo(void *information, FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, SIDF_MAC_INFO *macInfo)
{
	CCODE		ccode=0;
	UINT32		pathBase=0;
	UINT32		nameSpaceDirBase=0;	
	char		*path=NULL;

	FStart();
	
	ccode = FS_CFS_GetPathBaseEntry(fileOrDirHandle, &pathBase, &nameSpaceDirBase);
	if(ccode)
		goto Return;
	  
	
	ccode = GenNSGetSpecificInfo(
	   						  fileOrDirHandle->cfsStruct.clientConnID,
							  (UINT8)fileOrDirHandle->nameSpace,
							  (UINT8)MACNameSpace,
	   						  (UINT8)fileOrDirHandle->cfsStruct.volumeNumber,
	   						  pathBase, 
	   						  MAC_FIXED_INFO_MASK, 
	   						  (void *)macInfo
	   						  );
	if(ccode)
		{
			//FS_CFS_GetFullPathName(fileOrDirHandle, &path);
			FLogError("GenNSGetSpecificInfo", ccode, NULL);
			if(path) {tsaFree(path); path=NULL;}
			goto Return;
		}
	
	Return:
		
		FEnd(ccode);
		return ccode;	
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS | BACKUP
#define FNAME   "FS_CFS_SIDF_ConvertNFSInfo"
#define FPTR     FS_CFS_SIDF_ConvertNFSInfo
CCODE FS_CFS_SIDF_ConvertNFSInfo(void *information, FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, SIDF_NFS_INFO *nfsInfo)
{
	CCODE		ccode=0;
	UINT32		pathBase=0;
	UINT32		nameSpaceDirBase=0;	
	char		*path=NULL;
	
	FStart();
	
	ccode = FS_CFS_GetPathBaseEntry(fileOrDirHandle, &pathBase, &nameSpaceDirBase);
	if(ccode)
	  goto Return;
	
	ccode = GenNSGetSpecificInfo(
	   						  fileOrDirHandle->cfsStruct.clientConnID,
							  (UINT8)fileOrDirHandle->nameSpace,
							  (UINT8)NFSNameSpace,
	   						  (UINT8)fileOrDirHandle->cfsStruct.volumeNumber,
	   						  pathBase, 
	   						  NFS_FIXED_INFO_MASK, 
	   						  (void *)nfsInfo
	   						  );
	if(ccode)
	{
		//FS_CFS_GetFullPathName(fileOrDirHandle, &path);
		FLogError("GenNSGetSpecificInfo", ccode, NULL);
		if(path) {tsaFree(path); path=NULL;}
		goto Return;
	}
	
    Return:
		
		FEnd(ccode);
		return ccode;
}
