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
 | $Modtime:   24/02/2002 $
 |
 | $Workfile:   migrate.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		- TBDTBD
 ****************************************************************************/

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <zPublics.h>
#include <zParams.h>
#include <zError.h>

#include <smsdefns.h>
#include <smstypes.h>

#include <fsinterface.h>
#include <incexc.h>
#include <smstserr.h>
#include <tsalib.h>
#include <tsaunicode.h>
#include <compath.h>
#include <smsdebug.h>
#include "cfsdefines.h"
#include <smsdebug.h>
#include <migrate.h>
#include <nwlocale.h>
#include <malloc.h>

typedef UINT32 (* ImportedSym)();
void *ImportSymbol(int, char *);	/* from CLIB */
extern UINT32	TSANLMHandle;


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NO_DEBUG_INFO
#define FNAME   "FS_CFS_VerifyMigrationKey"
#define FPTR     FS_CFS_VerifyMigrationKey
CCODE FS_CFS_VerifyMigrationKey(DMKEY *key)
{
	CCODE ccode=0;
	static UINT32 (*VerifyKey)() = 0;

	if (!VerifyKey)
	{
		VerifyKey = ImportSymbol(TSANLMHandle, "VerifyKey");
	}

	if (VerifyKey)
	{
		ccode = (*VerifyKey)(key);
	}

	return (ccode);
}




#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_CFS_CreateMigratedFile"
#define FPTR     FS_CFS_CreateMigratedFile
CCODE FS_CFS_CreateMigratedFile(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,
							UINT32				primaryEntryIndex,
							UINT32				vol,
							DMKEY				*dmKey,
						    UINT32				setMask,
						    FS_FILE_OR_DIR_INFO	*scanInfo,
							DMHS				*sizes
							)
{
	CCODE 		ccode = 0;
	static UINT32 (*CreateMigratedFile)() = 0;
    char		*path=NULL;
   	UINT32		uniLength=0;
	char		*tmpStr;
	UINT32		len;
	
	if (!CreateMigratedFile)
	{
		CreateMigratedFile = ImportSymbol(TSANLMHandle, "CreateMigratedFile");
	}
	
	if (CreateMigratedFile)
	{
		if(ccode = SMS_UnicodeToByte(fileOrDirHandle->cfsStruct.uniFileDirName, &uniLength, &path, NULL)) 
		{
			ccode=NWSMTS_INTERNAL_ERROR;
			goto Return;
		}	

		len = strlen(path);
		tmpStr = (char *)tsaCalloc(sizeof(char), len + 2);
		if (!tmpStr)
		{
			ccode = NWSMTS_OUT_OF_MEMORY;
			goto Return;
		}

		if (tmpStr)
		{	
			strcpy(&tmpStr[1], path);
			tmpStr[0] = (char )len;
			ccode = CreateMigratedFile(
				fileOrDirHandle->cfsStruct.clientConnID,
				vol,
				fileOrDirHandle->cfsStruct.cfsNameSpace,
				fileOrDirHandle->cfsStruct.directoryNumber,
				(BYTE *)tmpStr,					/* byte len preceeded string */
				setMask,				/* DOS modify mask */
				0,						/* MAC modify bits */
				(NWMODIFY_INFO*)scanInfo->information,
				(NWMODIFY_INFO*)scanInfo->information,
				dmKey,
				0x10,					/* save key flag */
				0,						/* requested access rights */
				sizes);					/* DMHS struct migrate.h */
			
            if(ccode)
            {
			    FLogError("CreateMigratedFile", ccode, NULL);
				ccode = NWSMTS_CREATE_ERROR;
            }

			tsaFree((void *)tmpStr);
		}
	}

Return:
	if(path)
		tsaFree(path);

	return (ccode);
}



#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_CFS_CreateMigratedDirectory"
#define FPTR     FS_CFS_CreateMigratedDirectory
CCODE FS_CFS_CreateMigratedDirectory(
										   	FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,
										   	UINT32			smid,
											UINT32			bindFlag,
											UINT32			*bindKeyNumber
											)
{
    /*CCODE			ccode = 0;
    char			*path=NULL;
    char			fileName[256];	
    char 			*savePtr=NULL;
    UINT32 			dosDirBase=0;
    UINT32			nameSpaceDirBase=0;
    UINT32			uniLength=0;
    char			pathString[NW_COMPONENT_PATH_LEN+5];
	static UINT32 	(*CreateMigratedDirectory)() = 0;
	UINT32	    	returnedDirectoryNumber=0;
	void			*returnedSubDir=NULL;
	unicode	   		tmpuniFullpath[4096];
	punicode 		uniPathName = NULL; 
	UINT32 			uniPathBufLen; 

	if(ccode = SMS_UnicodeToByte(fileOrDirHandle->cfsStruct.uniFullpath, &uniLength, &path)) 
	{
		ccode=NWSMTS_INTERNAL_ERROR;
		goto Return;
	}	

	savePtr = NWSMStripPathChild(fileOrDirHandle->nameSpace, path, fileName,
								sizeof(fileName));
	if(!savePtr)
	{
		ccode = NWSMTS_INVALID_PATH;
		goto Return;
	}
	else
	{
		*savePtr = fileName[0];	
	}
	
   	if (ccode = SMS_ByteToUnicode(savePtr, &uniPathBufLen, &uniPathName))
   	{
		ccode = NWSMTS_INTERNAL_ERROR;
		goto Return;
	}

	unicpy(tmpuniFullpath, fileOrDirHandle->cfsStruct.uniFullpath); 
   	unicpy(fileOrDirHandle->cfsStruct.uniFullpath,uniPathName);
	ccode =  FS_CFS_GetPathBaseEntry(fileOrDirHandle, &dosDirBase, &nameSpaceDirBase); 
	unicpy( fileOrDirHandle->cfsStruct.uniFullpath,tmpuniFullpath); 
	
	if(!ccode)
	  goto Return;
	
	ccode = _NWStoreAsComponentPath(
									  pathString,
									  (UINT8)fileOrDirHandle->nameSpace,
									  fileName,
									  FALSE);
	if(ccode)
	{
		ccode=NWSMTS_INVALID_PATH;
		goto Return;
	}

	if(!CreateMigratedDirectory)
	{
		CreateMigratedDirectory = ImportSymbol(TSANLMHandle, "CreateMigratedDirectory");
	}

	if(CreateMigratedDirectory)
	{
		ccode = CreateMigratedDirectory(
					fileOrDirHandle->cfsStruct.clientConnID,
					fileOrDirHandle->cfsStruct.volumeNumber,
					dosDirBase, 
					&pathString[1],
					(LONG)pathString[0],
					fileOrDirHandle->nameSpace,
					NWSMS_DEFAULT_INHERITED_RIGHTS,	
					&returnedDirectoryNumber,
					&returnedSubDir,
					bindFlag,
					smid,
					bindKeyNumber);
	}
	else
	{
        FLogError("CreateMigratedDirectory", ccode, NULL);		
        ccode = NWSMTS_CREATE_DIR_ENTRY_ERR;
	}
	
Return:
	if(path)
		tsaFree(path);
	if(uniPathName)
		tsaFree(uniPathName);
		
	return (ccode);*/
	return 0;
}
