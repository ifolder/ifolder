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
 | $Modtime:   03 Jul 2002 10:30:00  $
 |
 | $Workfile:   fsmisc.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Misc. file system functions
 +-------------------------------------------------------------------------*/

#include <cfsdefines.h>
#include <filhandle.h>
#include <compath.h>
#include <tsajob.h>

#include <smsutapi.h>
#include <smsdefns.h>
#include <smstypes.h>
#include <tsaresources.h>
#include <fsinterface.h>
#include <tsalib.h>
#include <tsa_defs.h>
#include <tsaunicode.h>
#include <smstserr.h>
#include <tsa_320.mlh>
#include <smsdebug.h>

#include <string.h>
#include <stdio.h>
#include <errors.h>
#include <nwapidef.h>
#include <Dstruct.h>
#include <lfsproto.h>

extern unsigned int	tsazInfoVersion;
extern unsigned int	nssInfoMask;

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_READ|FILESYS
#define FNAME   "FS_CFS_GetDirSpaceRestriction"
#define FPTR     FS_CFS_GetDirSpaceRestriction
CCODE FS_CFS_GetDirSpaceRestriction(void *info, FS_FILE_OR_DIR_HANDLE dirHandle, SQUAD *dirSpaceRestriction)
{
	union DirUnion 				*dirEntry;
	CCODE						ccode=0;

	FStart();
	
	ccode = CheckAndGetDirectoryEntry(dirHandle.cfsStruct.volumeNumber, dirHandle.cfsStruct.directoryNumber, NWNAME_SPACE_PRIMARY, &dirEntry);
	if (!ccode)
	{
		*dirSpaceRestriction = (LONG)dirEntry->DOSSubDir.SDMaximumSpace; //DOSFile returns only 16 bits, while DOSSubDir returns 32 bit information
		if((*dirSpaceRestriction == FS_CFS_NO_RESTRICTION) || (*dirSpaceRestriction == 0))
		   ccode = FS_NO_SPACE_RESTRICTION;
	}
	else
	{
		FLogError("CheckAndGetDirectoryEntry", ccode, 0);
		return FS_INTERNAL_ERROR;
	}

	FEnd(ccode);
	
	return ccode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    TSASCAN
#define FNAME   "FS_CheckCOWOnName"
#define FPTR     FS_CheckCOWOnName
BOOL FS_CFS_CheckCOWOnName(unicode *dataSetName)
{
	//legacy does not support
	return FALSE;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    TSASCAN|FILESYS
#define FNAME   "FS_CFS_OpenAndRename"
#define FPTR     FS_CFS_OpenAndRename
CCODE FS_CFS_OpenAndRename(FS_FILE_OR_DIR_HANDLE *fsHandle, BOOL isParent, unicode *bytePath, UINT32 nameSpaceType, unicode *oldFullName, unicode *newDataSetName)
{
	CCODE		 ccode=0;
	LONG		 pathBase=0;
	int			 strLen = 0;
	unsigned char *asciiName = NULL;
	UINT32		 nameLen = 0;
	unsigned char *name = NULL;
	FStart();

	nameSpaceType = MapToNSSNameSpace(nameSpaceType);
	
	/* Get the path base for the file system object to rename */
	ccode = FS_CFS_GetPathBaseEntry(fsHandle,  &pathBase, NULL);
	if(ccode)
		goto Return;

	/* Convert the new name to byte for use with the legacy APIs */
	ccode = SMS_UnicodeToByte(newDataSetName, &nameLen, &asciiName, NULL);
	if (ccode)
		goto Return;

	/* Encode length preceded new name */
	strLen = strlen(asciiName);
	name = (unsigned char *)tsaMalloc(strLen + 2);
	if (name == NULL)
	{
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
	name[0] = strLen;
	strcpy((char *)&name[1], asciiName);

	/* modify the requested name space with the new name */
	ccode = GenNSModifySpecificInfo(fsHandle->cfsStruct.clientConnID, ~(fsHandle->cfsStruct.clientConnID),
		NWNAME_SPACE_PRIMARY, (UINT8)nameSpaceType, fsHandle->cfsStruct.volumeNumber, pathBase,
		NWMODIFY_NAME, strLen + 1, (void *)name);
	if(ccode)
		FLogError("GenNSModifySpecificInfo", ccode, NULL);

Return:
	if (name)
		tsaFree(name);

	if (asciiName)
		tsaFree(asciiName);

	FEnd(ccode);
	return ccode;
}
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|CFS_READ|CRITICAL|COMPACT
#define FNAME   "FS_CFS_GetPathBaseEntry"
#define FPTR     FS_CFS_GetPathBaseEntry


CCODE FS_CFS_GetPathBaseEntry(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 *dosDirBase, UINT32 *nameSpaceDirBase)
{
	CCODE				ccode=0;
	UINT32				fileDirLength=0;
	NWHANDLE_PATH     	*pathInfo=NULL;
	char		      	*path=NULL; 
	UINT32				searchAttributes=0;
	UINT32				namespaceDirectoryBase=0;
	UINT32 				dosDirectoryBase=0;
	UINT8				volumeNumber=0;
 
	if(!fileOrDirHandle)
	{
		ccode = FS_PARAMETER_ERROR;
		goto Return;
	}

	pathInfo = (NWHANDLE_PATH *)tsaMalloc(sizeof(NWHANDLE_PATH));
	if(!pathInfo)
	{
	 	ccode = NWSMTS_OUT_OF_MEMORY;
	 	goto Return;
	}
	
    if(!fileOrDirHandle->isDirectory)
	{	
		ccode = SMS_UnicodeToByte(fileOrDirHandle->cfsStruct.uniFileDirName, &fileDirLength, &path, NULL); 
		if(ccode)
		{
			ccode = NWSMTS_INTERNAL_ERROR;
			goto Return;
		}	
		path[fileDirLength] = NULL;
	}
	else
		path = NULL;
	
	
	if(fileOrDirHandle->isDirectory)
	  searchAttributes=0x16;
	else
	  searchAttributes=0x06;
	ccode = NewFillHandleStruct( (NWHANDLE_PATH *) pathInfo, path, fileOrDirHandle->nameSpace, &(fileOrDirHandle->cfsStruct)); 	
	if(ccode)
	{
		ccode=NWSMTS_INVALID_PATH;
		goto Return;
	}
	ccode = GenNSGetDirBase(
						  fileOrDirHandle->cfsStruct.clientConnID,
						  pathInfo,
						  fileOrDirHandle->cfsStruct.cfsNameSpace, //backup always dos, restore spefific to file/dir
						  searchAttributes,
						  &namespaceDirectoryBase,
						  &dosDirectoryBase,
						  &volumeNumber, 
						  (UINT8)fileOrDirHandle->nameSpace
					      );
	if(!ccode)
	{
		if(dosDirBase)
		  *dosDirBase=dosDirectoryBase;
		if(nameSpaceDirBase)
		  *nameSpaceDirBase=namespaceDirectoryBase;
		//Required only for restore
	}
	else
	{
		FLogError("GenNSGetDirBase", ccode, path);
		ccode = NWSMTS_GET_ENTRY_INDEX_ERR;
	}
	
Return:
	if(path)
		tsaFree(path);	
	
	if(pathInfo)
		tsaFree(pathInfo);
	
	return ccode;
}
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN|NFS_CHARCATERISTICS
#define FNAME   "FS_CFS_GeNameSpaceHugeInfo"
#define FPTR     FS_CFS_GetNameSpaceHugeInfo
CCODE FS_CFS_GetNameSpaceHugeInfo(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, char *hugeData, UINT32 *hugeDataSize, UINT32 nameSpace)
{
    CCODE          					ccode=0;
    UINT32                          nsDirBase=0;
    NWHANDLE_PATH                   *pathInfo=NULL;
    UINT32              			fileDirLength=0, i = 0;
    char                   			*path=NULL; 
    struct DirectoryStructure       *dStruct=NULL;
    UINT8               			stateInfo[NWSTATE_INFO_SIZE] = { 0 };
    UINT32       					hugeDataLength = 0, tmpHugeDataSize = 0;
    UINT32           				validState = TRUE;
    /* The hugeData buffer layout
    UINT32 numberOfLinks;
    UINT32 pathlength; - this includes the directory
    STRING pathname;  - length preceeded string
    LONG  UID
    LONG GID
    LONG MODE INDEX
    */
    FStart();
   
    pathInfo = (NWHANDLE_PATH *)tsaMalloc(sizeof(NWHANDLE_PATH));
    if(!pathInfo)
    {
        ccode = NWSMTS_OUT_OF_MEMORY;
        goto Return;
    }
    
    if(!fileOrDirHandle->isDirectory)
    {       
        ccode = SMS_UnicodeToByte(fileOrDirHandle->cfsStruct.uniFileDirName, &fileDirLength, &path, NULL); 
        if(ccode)
        {
			ccode=NWSMTS_INTERNAL_ERROR;
			goto Return;
        }       
        path[fileDirLength] =NULL; //this is a file path 
    }
    else
        path=NULL;

    ccode = NewFillHandleStruct((NWHANDLE_PATH *)pathInfo, path, fileOrDirHandle->cfsStruct.cfsNameSpace, &(fileOrDirHandle->cfsStruct));
    if(ccode)
    {
        ccode=NWSMTS_INVALID_PATH;
        goto Return;    
    }
    
    ccode=GetEntryFromPathStringBase(
                                  fileOrDirHandle->cfsStruct.clientConnID,
                                  fileOrDirHandle->cfsStruct.volumeNumber,
                                  pathInfo->DirectoryBaseOrHandle,
                                  pathInfo->PathString,
                                  pathInfo->PathComponentCount,  
                                  fileOrDirHandle->cfsStruct.cfsNameSpace,
                                  nameSpace,
                                  &dStruct,
                                  &nsDirBase                              
                                  );
    if(ccode)
    {
		FLogError("GetEntryFromPathStringBase", ccode, NULL);
		ccode = NWSMTS_GET_ENTRY_INDEX_ERR;
		goto Return;
    }

    tmpHugeDataSize = *hugeDataSize;

    while(validState && !ccode)
    {
        hugeDataLength = *hugeDataSize;

        if(hugeDataLength == 0)
        {
            ccode = NWSMTS_BUFFER_UNDERFLOW;
            continue;
        }
        ccode = GetHugeInformation(
                                 fileOrDirHandle->cfsStruct.clientConnID,
                                 ~(fileOrDirHandle->cfsStruct.clientConnID),
                                 (UINT8)nameSpace,
                                 fileOrDirHandle->cfsStruct.volumeNumber,
                                 nsDirBase,
                                 0x400,
                                 stateInfo,
                                 hugeData,
                                 &hugeDataLength,       
                                 stateInfo
                                 );
        if(!ccode)
        {
            validState = FALSE;
            if(hugeDataLength == 0)
                    continue;
            /*This is a sanity check so that if GetHugeInformation looped or brought in unexpected results */
            
            *hugeDataSize -= hugeDataLength;
            //check for the buffer overflow
            hugeData += hugeDataLength;
            for(i = 0; i < sizeof(stateInfo); i++) 
            {
                if(stateInfo[i])      
                {
                        validState = TRUE;
                }
            }
                
          }
    }

    if(!ccode)
        *hugeDataSize = tmpHugeDataSize - *hugeDataSize;
    else
    {
        if (ccode == NO_HUGE_DATA)
                ccode = 0;
        *hugeDataSize = 0;
    }

    /*       The hugeData pointer at this point is NOT the same one tht we had received in the function. Take care before using it.
            It currently points to the byte after the last byte.
    */
Return:
    if(path)
	    tsaFree(path);    
    if(pathInfo)
        tsaFree(pathInfo);

    FEnd(ccode);        
    return ccode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN|FILESYS
#define FNAME   "FS_CFS_GetFileNameSpaceName"
#define FPTR     FS_CFS_GetFileNameSpaceName

CCODE FS_CFS_GetFileNameSpaceName(CFS_STRUCT * cfsStruct, char *path, UINT32 inNameSpace, UINT32 outnameSpace, UINT32 searchAttributes, char *retFileDirName)
{
    CCODE                           ccode=0;
    NWHANDLE_PATH           *pathInfo=NULL;
    NetwareInfo                     netwareInfo;
    char                            buf1[257];
    FStart();

    pathInfo=(NWHANDLE_PATH *)tsaMalloc(sizeof(NWHANDLE_PATH));
    if(!pathInfo)
    {
            ccode= NWSMTS_OUT_OF_MEMORY;
            goto Return;
    }               
    
    ccode = NewFillHandleStruct((NWHANDLE_PATH *) pathInfo, path, inNameSpace, cfsStruct);
    if(ccode)
		goto Return;
    ccode = GenNSObtainInfo(
							cfsStruct->clientConnID, 
							pathInfo,
							inNameSpace,
							outnameSpace,
							searchAttributes, 
							NWRETURN_NAME,  
							&netwareInfo,
							(BYTE *)buf1
							);
    if(!ccode)
    {
		memcpy(retFileDirName, &(buf1[1]), buf1[0]);
		retFileDirName[buf1[0]]=NULL;
    }
    else
    {
		FLogError("GenNSObtainInfo", ccode, NULL);
		ccode = NWSMTS_SCAN_NAME_SPACE_ERR;
    }
    
Return:
	if(pathInfo)
		tsaFree(pathInfo);
	FEnd(ccode);
	return ccode;
}
/*************************************************************************************************************************************/
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN
#define FNAME   "FS_CFS_InitFileSystemInterfaceLayer"
#define FPTR     FS_CFS_InitFileSystemInterfaceLayer

CCODE FS_CFS_InitFileSystemInterfaceLayer(INT32 connectionID, FS_HANDLE *handle)
{
        //do nothing in legacy
        return 0;
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN
#define FNAME   "FS_CFS_DeInitFileSystemInterfaceLayer"
#define FPTR     FS_CFS_DeInitFileSystemInterfaceLayer

CCODE FS_CFS_DeInitFileSystemInterfaceLayer( FS_HANDLE handle)
{
        //do nothing in legacy
        return 0;
}




#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN
#define FNAME   "FS_CFS_SetFileDirSearchAttributes"
#define FPTR     FS_CFS_SetFileDirSearchAttributes
void FS_CFS_SetFileDirSearchAttributes(JOB *newJob)
{       
	//default
	newJob->dirInfo.FileMatchCriteria = FS_CFS_SYSTEM | FS_CFS_HIDDEN;
	newJob->dirInfo.DirMatchCriteria = FS_CFS_SUBDIRECTORY | FS_CFS_SYSTEM | FS_CFS_HIDDEN;

	if(newJob->scanControl->scanType & NWSM_EXCLUDE_HIDDEN_CHILDREN)
	          newJob->dirInfo.FileMatchCriteria &= ~FS_CFS_HIDDEN;
	if(newJob->scanControl->scanType & NWSM_EXCLUDE_SYSTEM_CHILDREN)
	   newJob->dirInfo.FileMatchCriteria &= ~FS_CFS_SYSTEM;

	if(newJob->scanControl->scanType & NWSM_EXCLUDE_HIDDEN_PARENTS)
	   newJob->dirInfo.DirMatchCriteria &= ~FS_CFS_HIDDEN;
	if(newJob->scanControl->scanType  & NWSM_EXCLUDE_SYSTEM_PARENTS)
	   newJob->dirInfo.DirMatchCriteria &= ~FS_CFS_SYSTEM;
}
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN|FILESYS
#define FNAME   "FS_CFS_BGetDirBase"
#define FPTR     FS_CFS_BGetDirBase
CCODE FS_CFS_BGetDirBase(CFS_STRUCT *CFSscanStruct , UINT32 *retDirectoryNumber)
{
     CCODE 				ccode=0;
     UINT32				length=0;
     NWHANDLE_PATH 		*pathInfo=NULL; 
     char 				*path=NULL;
     
     FStart();

     pathInfo = (NWHANDLE_PATH *) tsaMalloc(sizeof(NWHANDLE_PATH));
     if(!pathInfo)
     {
        ccode= NWSMTS_OUT_OF_MEMORY;
        goto Return;
     }
     path = (char *) tsaMalloc(255);
     if(!path)
     {
        ccode= NWSMTS_OUT_OF_MEMORY;
        goto Return;
     }
	 length = 255;
     ccode = SMS_UnicodeToByte(CFSscanStruct->uniFileDirName, &length, &path, NULL);
     if(ccode)
     {
		ccode=NWSMTS_INTERNAL_ERROR;
		goto Return;
     }
     path[length]=NULL;
     
     ccode=_NWStoreAsComponentPath(
									(STRING) &(pathInfo->PathComponentCount),
									(UINT8)DOSNameSpace,   
									path,                                                  
									FALSE //pathIsFullyQualified
									);
     if(ccode)
        goto Return;
     
     ccode = MapPathToDirectoryNumber(
									CFSscanStruct->clientConnID,
									CFSscanStruct->volumeNumber,
									CFSscanStruct->directoryNumber,
									pathInfo->PathString,
									pathInfo->PathComponentCount,
									DOSNameSpace, //scan always in dos name space
									retDirectoryNumber,
									&length
									);
     if(ccode)
        FLogError("MapPathToDirectoryNumber", ccode, path);
     
Return:

     if(pathInfo)
        tsaFree(pathInfo);
     if(path)
        tsaFree(path);

     FEnd(ccode);
     return ccode;
}



#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN|FILESYS
#define FNAME   "FS_CFS_GetFileNameSpaceUNIName"
#define FPTR     FS_CFS_GetFileNameSpaceUNIName

unicode* FS_CFS_GetFileNameSpaceUNIName(void *information, UINT32 retNameSpace, unicode *cfsNameSpaceInfo[])
{
        
        return(cfsNameSpaceInfo[retNameSpace]);
}

/*************************************************************************************************************************************/
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN|FILESYS
#define FNAME   "FS_CFS_GenericGetFileNameSpaceName"
#define FPTR     FS_CFS_GenericGetFileNameSpaceName

void* FS_CFS_GenericGetFileNameSpaceName(void *information, UINT32 retNameSpace, void* cfsNameSpaceInfo[])
{
        
        return(cfsNameSpaceInfo[retNameSpace]);
}


/*************************************************************************************************************************************/
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN
#define FNAME   "FS_CFS_GetFilePrimaryUNIName"
#define FPTR     FS_CFS_GetFilePrimaryUNIName

unicode* FS_CFS_GetFilePrimaryUNIName(void *information, UINT32 retNameSpace, unicode *cfsNameSpaceInfo[])
{
        
        return(cfsNameSpaceInfo[retNameSpace]);
}
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|CFS_READ
#define FNAME   "FS_CFS_GetNameSpaceEntryName"
#define FPTR     FS_CFS_GetNameSpaceEntryName

CCODE FS_CFS_GetNameSpaceEntryName(UINT32 clientConnID, FS_HANDLE handle, unicode *path, 
			unsigned long  nameSpaceType, unsigned long  nameSpace, unicode **newName, 
			UINT32 taskID, UINT32 *attributes)
{
	CCODE 				ccode=0;
	NWHANDLE_PATH 		*pathInfo=NULL;
	NetwareInfo			netwareInfo={0};
	int					len=0;
	char				*tmpPath = NULL;
	UINT32				actLen = 0;
	BYTE				*tmpReqPath = NULL;
	FStart();

	pathInfo=(NWHANDLE_PATH *)tsaMalloc(sizeof(NWHANDLE_PATH));
	if(!pathInfo)
	{
		ccode= NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
	
	tmpReqPath = (char *)tsaMalloc(257);
	if (tmpReqPath == NULL)
	{
		ccode= NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
	//TBD if the given path is more than 300 no support
	
	ccode = SMS_UnicodeToByte(path, &actLen, &tmpPath, NULL); 
	if(ccode)
	{
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}	
	tmpPath[actLen] = NULL;
	
	ccode = FillHandleStruct((NWHANDLE_PATH *) pathInfo, tmpPath, nameSpace, clientConnID);
	if(ccode)
	{
		if(ccode != NWSMTS_OUT_OF_MEMORY)
			ccode = NWSMTS_DATA_SET_NOT_FOUND;
		goto Return;
	}
			
	ccode = GenNSObtainInfo(
						   clientConnID, 
						   pathInfo,
						   (UINT8)nameSpace,
						   (UINT8)nameSpaceType,
						   NWSA_ALL_DIRS,
				           NWRETURN_NAME | NWRETURN_ATTRIBUTES,
						   &netwareInfo,
						   tmpReqPath
						  );
	if (!ccode)
    {
    	*attributes = (UINT32)FS_CFS_GetFileAttributes(&netwareInfo);
        len = tmpReqPath[0];
        memmove(tmpReqPath, &tmpReqPath[1], len);
        tmpReqPath[len] = 0;

		ccode = SMS_ByteToUnicode((char *)tmpReqPath, (UINT32 *)&len, newName, NULL);
		if (ccode)
		{
			ccode = NWSMTS_OUT_OF_MEMORY;
			goto Return;
		}
    }
	else
	{
        FLogError("GenNSObtainInfo", ccode, NULL);		
		if(ccode==ERR_NO_FILES_FOUND)
			ccode=NWSMTS_DATA_SET_NOT_FOUND;
	}
	
Return:
	if(pathInfo)
	 	tsaFree(pathInfo);
	
	if (tmpReqPath)
		tsaFree(tmpReqPath);

	if (tmpPath)
		tsaFree(tmpPath);
	
	FEnd(ccode);
	return(ccode);
}
CCODE FS_CFS_GetInfo(FS_FILE_OR_DIR_HANDLE *handle, UINT32 getInfoMask, UINT32 bufferSize, void *buffer)
{
	//not needed for legacy done for function pointer consistency
	return(0);
}
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    OPEN | FILESYS
#define FNAME   "FS_CFS_GetStreamSizes"
#define FPTR     FS_CFS_GetStreamSizes
CCODE FS_CFS_GetStreamSizes(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_FILE_OR_DIR_INFO *info)
{
	CCODE 				ccode=0;
	UINT32				fileDirLength=0;
	NWHANDLE_PATH   	*pathInfo=NULL;
	char		      	*path=NULL; 
	UINT32				count=0;
	UINT8				*fileName=NULL;
	UINT32				retInfo[20];
	FStart();
	
	pathInfo = (NWHANDLE_PATH *)tsaMalloc(sizeof(NWHANDLE_PATH));
	if(!pathInfo)
	{
	 	ccode = NWSMTS_OUT_OF_MEMORY;
	 	goto Return;
	}

	if(!fileOrDirHandle->isDirectory)
	{	
		ccode = SMS_UnicodeToByte(fileOrDirHandle->cfsStruct.uniFileDirName, &fileDirLength, &path, NULL); 
		if(ccode)
		{
			ccode=NWSMTS_INTERNAL_ERROR;
			goto Return;
		}	
		path[fileDirLength] = NULL;
	}
	else
		path = NULL;

	ccode = NewFillHandleStruct( (NWHANDLE_PATH *) pathInfo, path, fileOrDirHandle->nameSpace, &(fileOrDirHandle->cfsStruct)); 	
	if(ccode)
	{
		ccode=NWSMTS_INVALID_PATH;
		goto Return;
	}

	//Get the stream Size
	fileName = (char *)tsaMalloc(256);
	if(!fileName)
	{
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
	//multi change
	//fileOrDirHandle->streamCount = FS_CFS_GetFileNumberOfDataStreams(info->information);
	//fileOrDirHandle->streamCount ++;//for primary
	
	//if(fileOrDirHandle->streamCount > FS_MAX_NO_OF_DATA_STREAMS)
	//  fileOrDirHandle->streamCount = FS_MAX_NO_OF_DATA_STREAMS;
	//end
	  	
	ccode =	GenNSObtainInfo(
							fileOrDirHandle->cfsStruct.clientConnID,
							pathInfo,
							fileOrDirHandle->nameSpace,
							fileOrDirHandle->nameSpace,
							0x06, //All files
							0x80000000|0x00008000,
							(NetwareInfo *)retInfo,
							(BYTE *)fileName
						 );
	if(ccode)
	{
		ccode = NWSMTS_OPEN_DATA_STREAM_ERR;
		goto Return;
	}
	for(count = 1; count < fileOrDirHandle->streamCount; count++)
	{
		fileOrDirHandle->handleArray[count].size = retInfo[count * 2 + 2];
	}


Return:
	if(pathInfo)
		tsaFree(pathInfo);
	if(path)
		tsaFree(path);		
	if(fileName)
		tsaFree(fileName);

	FEnd(ccode);
	return ccode;
}

