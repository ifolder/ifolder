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
 | $Modtime:   27 Mar 2002 17:04:16  $
 |
 | $Workfile:   fsCreate.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |      This module is used to:
 |              Initialise TSA
 +-------------------------------------------------------------------------*/

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <filhandle.h>
#include <compath.h>
#include <cfsdefines.h>
#include <fsinterface.h>
#include <incexc.h>
#include <smstserr.h>
#include <tsalib.h>
#include <tsa_defs.h>
#include <smsdebug.h>
#include <restore.h>
#include <tsa_320.mlh>
#include <smsdebug.h>
#include <tsaname.h>
#include <tsaunicode.h>
#include <tsaresources.h>

#include <smsdefns.h>
#include <smstypes.h>

#include <dstruct.h>
#include <Errors.h>
#include <lfsproto.h>
#include <fsproto.h>
#include <linktab.h>

extern unsigned int tsazInfoVersion;  

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|RESTORE 
#define FNAME   "FS_CFS_CreateAndOpen"
#define FPTR     FS_CFS_CreateAndOpen
CCODE FS_CFS_CreateAndOpen(
                                                 FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,
                                                 UINT32                 openMode,                               //Not used
                                                 UINT32                 accessRights,  
                                                 QUAD                   receivedCreateAttributes,//Not used 
                                                 UINT32                 streamType,
                                                 FS_HANDLE              *retHandle,
                                                 void					*dSetName,                              //Not used
                                                 UINT32                 rflags,
                                                 BOOL              isPathFullyQualified,
                                                 void 				*buffer,
                                                 FS_HL_TABLE			**linkTable
                                                 )
{

    CCODE                           ccode=0;
    NWHANDLE_PATH            		*pathInfo=NULL;
    char                             *path=dSetName;
    char                             *relPath=NULL;
    char                             *component=NULL;
    UINT32                           pathLength=0;
    UINT32                           volNum=0;
    UINT32                           pathCount=0;
    UINT32                           pathBase=0;
    BYTE                             *pathString=NULL;
    UINT32                           openCreateFlags=0;
    UINT32                           desirdAccessRights=0;
    UINT32                           createAttributes=0;
    NetwareFileName         		 *fileDirName=NULL;
	BYTE                             openCreateAction=0;
	NetwareInfo             	     *netWareInfo=NULL;
    UINT32                            length=0;
	UINT32                            totalPathCount=0;
    UINT32                            currentPathCount=0;
    FS_HANDLE                        eaHandle;
    char                             sep1[4];
    char                             sep2[4];
    struct DirectoryStructure  		*dStruct=NULL;
    UINT32							dirBase=0;
    UINT32							pathAllocated = FALSE;
    
    FStart();

    pathInfo = (NWHANDLE_PATH *)tsaMalloc(sizeof(NWHANDLE_PATH));
    if(!pathInfo)
    {
		ccode=NWSMTS_OUT_OF_MEMORY;
		goto Return;
    }
    
    component = (char *)tsaMalloc(256);
    if(!component)
    {
		ccode=NWSMTS_OUT_OF_MEMORY;
		goto Return;
    }
    
    netWareInfo = (NetwareInfo *)tsaMalloc(sizeof(NetwareInfo));
    if(!netWareInfo)
    {
		ccode=NWSMTS_OUT_OF_MEMORY;
		goto Return;
    }
    fileDirName = (NetwareFileName *)tsaMalloc(sizeof(NetwareFileName));
    if(!fileDirName)
    {
		ccode=NWSMTS_OUT_OF_MEMORY;
		goto Return;
    }

    if(isPathFullyQualified)
    {
		ccode= getVolumeNumber(path, fileOrDirHandle->nameSpace, &volNum);
		if(ccode)
			goto Return;

		fileOrDirHandle->cfsStruct.volumeNumber=volNum;

		relPath = removeVolName(path, pathLength, fileOrDirHandle->nameSpace);
		if(!relPath)
		{
		    ccode = NWSMTS_INVALID_PATH;
		    goto Return;
		}
    }
    else
    {
        relPath=path;
        volNum=fileOrDirHandle->cfsStruct.volumeNumber;
    }
    if(fileOrDirHandle->isDirectory)
    {
		if(rflags & DATASET_EXISTS)
			goto Return;

		openCreateFlags   = FS_CFS_CREATE_FILE;
		desirdAccessRights = accessRights |0x00001FF|FS_CFS_NO_UPDATE_LAST_ACCESSED_ON_CLOSE;
    }
    else
    {
		if((streamType==1)  &&  !(rflags & DATASET_EXISTS)) 
			openCreateFlags   = FS_CFS_CREATE_FILE | FS_CFS_TRUNCATE_FILE;
        else
			openCreateFlags   = FS_CFS_OPEN_FILE;
    
		desirdAccessRights = accessRights |0x0C|FS_CFS_NO_UPDATE_LAST_ACCESSED_ON_CLOSE;
    }
    if(streamType == DATASET_IS_EXTENDED_ATTRIBUTE)
    goto EACreate;

    //get the path count irrespective of path is fullyQualified or not
    ccode = GetNameSpaceSeparators(fileOrDirHandle->nameSpace, sep1, sep2);
    if(ccode)
		goto Return;
    
    totalPathCount = GetNumberOfNodeNamesInThePath(path, strlen(path), sep1, sep2);
    if(isPathFullyQualified)
		totalPathCount--; //Exclude the volume name 
            
    if(currentPathCount==totalPathCount)
    {
		//Volume cannot be created, graceful exit
		ccode=0; 
		fileOrDirHandle->isDirectory=TRUE;
		goto GraceExit;
    }
    
    while(currentPathCount <totalPathCount)
    {
        if(isPathFullyQualified)
        {
			ccode = getComp(relPath, currentPathCount+1, fileOrDirHandle->nameSpace, component);
			if(ccode)
			goto Return;

			if(!fileOrDirHandle->isDirectory && currentPathCount==totalPathCount-1)
			{
				if(rflags & DATASET_EXISTS) 
					openCreateFlags   = FS_CFS_OPEN_FILE;
				else
					openCreateFlags   = FS_CFS_CREATE_FILE | FS_CFS_TRUNCATE_FILE;

				createAttributes = 0x00; //file               
				desirdAccessRights = accessRights |0x0C|FS_CFS_NO_UPDATE_LAST_ACCESSED_ON_CLOSE;
			}
			else
			{
			  openCreateFlags = FS_CFS_CREATE_FILE;
			  createAttributes = FS_CFS_SUBDIRECTORY; //dir
			  desirdAccessRights = accessRights |0x00001FF|FS_CFS_NO_UPDATE_LAST_ACCESSED_ON_CLOSE;
			}
        }
        else //Break if file
        {
			currentPathCount=totalPathCount;
			createAttributes = 0x00; //normal
			strcpy(component, relPath);
        }
                                
        memset(pathInfo, 0, sizeof(NWHANDLE_PATH));
        
        ccode = _NWStoreAsComponentPath(
                                            (STRING) &(pathInfo->PathComponentCount),
                                            (UINT8)fileOrDirHandle->nameSpace,     
                                            component,                                                      
											FALSE //pathIsFullyQualified
                                            );
        if(ccode)
        {
			ccode = NWSMTS_INVALID_PATH;
            goto Return;
        }
        
		pathString = pathInfo->PathString;//done in _NWStoreAsComponentPath
        pathCount = pathInfo->PathComponentCount;
        
        if(isPathFullyQualified) // && fileOrDirHandle->isDirectory)
			pathBase  = dirBase;//from the previous iteration
        else
			pathBase  = fileOrDirHandle->cfsStruct.directoryNumber;
        
        ccode= F3OpenCreate(
                             fileOrDirHandle->cfsStruct.clientConnID,
                             (~fileOrDirHandle->cfsStruct.clientConnID),
							 volNum,
						     (BYTE)pathCount,
						     pathBase,
						     pathString,
						     fileOrDirHandle->nameSpace,// fileOrDirHandle->nameSpace,
						     streamType-1, //CFS starts 0,1 NSS strts 1,2 so minus one
						     openCreateFlags,
						     0x0,  //searchAttributes
						     createAttributes,
						     desirdAccessRights,
						     (UINT32)NWRETURN_ALL_INFO,     
						     &(retHandle->cfsHandle),//&(fileOrDirHandle->handleArray[0].handle.cfsHandle),
						     &openCreateAction,
						     netWareInfo,
						     (NetwareFileName *) fileDirName,
						     &length
						     );
        if(ccode && ccode != 255) //ERR_FILE_EXISTS
        {
			FLogError("F3OpenCreate", ccode, NULL);
			ccode = NWSMTS_INVALID_PATH;
			goto Return;
        }
        ccode = 0;
        if(currentPathCount !=totalPathCount-1)//to skip the last component
        {
			ccode=GetEntryFromPathStringBase(fileOrDirHandle->cfsStruct.clientConnID, volNum,
			                                  pathBase,
			                                  pathString,
			                                  pathCount,  
			                                  fileOrDirHandle->nameSpace,
			                                  fileOrDirHandle->nameSpace,
			                                  &dStruct,
			                                  &dirBase                                
			                                 );
			if(ccode)
			{
				FLogError("GetEntryFromPathStringBase", ccode, NULL);
				ccode = NWSMTS_INVALID_PATH;
				goto Return;
			}
        }
        currentPathCount++;
    }
    if(ccode)
    {  
		if(fileOrDirHandle->isDirectory)
		{
			ccode = NWSMTS_CREATE_DIR_ENTRY_ERR;
		}
		else
		{
			if(rflags & DATASET_EXISTS)
				ccode = NWSMTS_OPEN_DATA_STREAM_ERR;
			else
				ccode = NWSMTS_CREATE_ERROR;
		}
		goto Return;
    }
    else
    {
        if(streamType != DATASET_IS_EXTENDED_ATTRIBUTE)
          goto Return;
    }
    
EACreate:
    ccode=OpenEAHandle(
						fileOrDirHandle->cfsStruct.clientConnID,
						~(fileOrDirHandle->cfsStruct.clientConnID),
						0L, 
						fileOrDirHandle->isDirectory? 0:1, 
						fileOrDirHandle->isDirectory? fileOrDirHandle->cfsStruct.volumeNumber : fileOrDirHandle->handleArray[0].handle.cfsHandle,
						fileOrDirHandle->isDirectory?  fileOrDirHandle->cfsStruct.nsDirNumbers[0] :0, 
						&eaHandle.cfsHandle
						);
    if(!ccode)
    {
		(*retHandle).cfsHandle = eaHandle.cfsHandle;
    }
    else
    {
		FLogError("OpenEAHandle", ccode, NULL);
		if(path) {tsaFree(path); path=NULL;}
		ccode = NWSMTS_OPEN_ERROR;
    }

Return:
            
    if(!ccode && isPathFullyQualified)
    {
       ccode = FS_CFS_GetDirNumber(fileOrDirHandle);//Dir number is copied into cfsStruct
    }
    
GraceExit:

    if(path && pathAllocated)
      tsaFree(path);
    if(component)
      tsaFree(component);
    if(pathInfo)
      tsaFree(pathInfo);
    if(netWareInfo)
      tsaFree(netWareInfo); 
    if(fileDirName)
      tsaFree(fileDirName); 

    FEnd(ccode);
    return ccode;
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    RESTORE|FILESYS
#define FNAME   "FS_CFS_GetDirNumber"
#define FPTR     FS_CFS_GetDirNumber
CCODE FS_CFS_GetDirNumber( FS_FILE_OR_DIR_HANDLE *fileOrDirHandle) 
{
    CCODE					ccode=0;
    char					*path=NULL;
    NWHANDLE_PATH			*pathInfo=NULL;
    UINT32					volNum=0;
    struct DirectoryStructure  *dStruct=NULL;
    UINT32					dirBase=0;
    UINT8					nameSpace=0;
    RESOURCE_NODE			*resourceNode = NULL;
    BOOL				nameSpaceIsSupported[MAX_NAME_SPACES] = {0};
    
    FStart();

	path = (char *)tsaMalloc(strlen((char*)fileOrDirHandle->uniPath)+1);
	if(!path)
	{
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
   	strcpy(path, (char*)fileOrDirHandle->uniPath);		
   	
    if(! fileOrDirHandle->isDirectory)
		NWSMStripPathChild(fileOrDirHandle->nameSpace, path, NULL, NULL);
          
    ccode = getVolumeNumber(path, fileOrDirHandle->nameSpace, &volNum);
    if(ccode)
    {
        ccode=NWSMTS_INVALID_PATH;
        goto Return;
    }
    
    if (LockResourceList() != 0)
    {
    	return NWSMTS_INTERNAL_ERROR;
    }
    resourceNode = FindResourceByID(volNum);
    for(nameSpace=0; nameSpace<=LAST_NAMESPACE; nameSpace++)
    {
    	nameSpaceIsSupported[nameSpace] = resourceNode->nameSpaceIsSupported[nameSpace];
    }
    UnlockResourceList();
    
    fileOrDirHandle->cfsStruct.volumeNumber = volNum;
    
    pathInfo = (NWHANDLE_PATH *)tsaMalloc(sizeof(NWHANDLE_PATH));
    if(!pathInfo)
    {
        ccode=NWSMTS_OUT_OF_MEMORY;
        goto Return;
    }
    
    ccode = FillHandleStruct((NWHANDLE_PATH *)pathInfo, path, fileOrDirHandle->nameSpace, fileOrDirHandle->cfsStruct.clientConnID);
    if(ccode)
    {
        ccode=NWSMTS_INVALID_PATH;
        goto Return;    
    }
    
    for(nameSpace=0; nameSpace<=LAST_NAMESPACE; nameSpace++)
    {
        if (!nameSpaceIsSupported[nameSpace])
                continue;
                
        if(nameSpace==FTAMNameSpace) 
                continue;
        
        ccode=GetEntryFromPathStringBase(
                                      fileOrDirHandle->cfsStruct.clientConnID,
                                      volNum,
                                      pathInfo->DirectoryBaseOrHandle,
                                      pathInfo->PathString,
                                      pathInfo->PathComponentCount,  
                                      fileOrDirHandle->cfsStruct.cfsNameSpace,
                                      nameSpace,
                                      &dStruct,
                                      &dirBase                                
                                      );
        if(!ccode)
        {
            if(nameSpace==DOSNameSpace)
                  fileOrDirHandle->cfsStruct.nsDirNumbers[0]=dirBase;
            else if(nameSpace==OS2NameSpace)
              fileOrDirHandle->cfsStruct.nsDirNumbers[1]=dirBase;       
            else if(nameSpace==MACNameSpace)
              fileOrDirHandle->cfsStruct.nsDirNumbers[2]=dirBase;       
            else if(nameSpace==NFSNameSpace)
              fileOrDirHandle->cfsStruct.nsDirNumbers[3]=dirBase;       
            
            if(nameSpace==fileOrDirHandle->cfsStruct.cfsNameSpace)
            	fileOrDirHandle->cfsStruct.directoryNumber=dirBase;//for current directory restore of attributes etc
        }
        else
             FLogError("GetEntryFromPathStringBase",ccode, path);
            
    }
Return:
    if(path)
        tsaFree(path);
    if(pathInfo)
        tsaFree(pathInfo);
    
    FEnd(ccode);
    return ccode;
}

#undef FTYPE
#undef FNAME
#undef FPTR
#define FTYPE RESTORE |FILESYS|BASIC_CHARACTERISTICS|CRITICAL|COMPACT
#define FNAME "FS_CFS_SetReadyFileDirForRestore"
#define FPTR FS_CFS_SetReadyFileDirForRestore
CCODE FS_CFS_SetReadyFileDirForRestore(			FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,
												UINT32  isPathFullyQualified, 
												void    *dSetName,
												UINT32  rFlags,
												UINT32  createMode, 
												UINT32  accessRights,
												QUAD    attributes)
{       
    CCODE 		 		ccode = 0;
    void        		*info = NULL;
    UINT32       		infoMask = 0;
    UINT32       		size = 0;
   	NWHANDLE_PATH 		*pathInfo=NULL;
	NetwareInfo			netwareInfo={0};
	char				*name=NULL; 

	FStart();
        
    pathInfo=(NWHANDLE_PATH *)tsaMalloc(sizeof(NWHANDLE_PATH));
	if(!pathInfo)
	{
		ccode= NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
	name = (char *)tsaMalloc(FILE_MAX_LENGTH+1);
	if(!name)
	{
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
	
    if(isPathFullyQualified)
    {
		ccode = FillHandleStruct((NWHANDLE_PATH *) pathInfo, dSetName, fileOrDirHandle->nameSpace, fileOrDirHandle->cfsStruct.clientConnID);
    }
    else
    {
    	pathInfo->DirectoryBaseOrHandle = fileOrDirHandle->cfsStruct.directoryNumber;
    	pathInfo->Volume = fileOrDirHandle->cfsStruct.volumeNumber;
    	pathInfo->HandleFlag = DIRECTORY_BASE_FLAG;
		ccode = _NWStoreAsComponentPath(
                                            (STRING) &(pathInfo->PathComponentCount),
                                            (UINT8)fileOrDirHandle->nameSpace,     
                                            dSetName,                                                      
											FALSE //pathIsFullyQualified
                                            );
	}
	if(ccode)
		goto Return;
			
	ccode = GenNSObtainInfo(
						   fileOrDirHandle->cfsStruct.clientConnID, 
						   pathInfo,
						   (UINT8)fileOrDirHandle->nameSpace,
						   (UINT8)fileOrDirHandle->nameSpace,
						   NWSA_ALL_DIRS,
				           NWRETURN_ATTRIBUTES,
						   &netwareInfo,
						   name
						  );
	if(ccode)
	{
		ccode = NWSMTS_GET_NAME_SPACE_ENTRY_ERR;
		goto Return;
	}
    
    attributes = FS_CFS_GetFileAttributes(&netwareInfo);
    
    if((attributes & FS_CFS_READ_ONLY) ||(attributes & FS_CFS_HIDDEN) || (attributes & FS_CFS_SYSTEM) || (attributes & FS_CFS_RENAME_INHIBIT) || (attributes & FS_CFS_DELETE_INHIBIT))
    {
        attributes &= ~(FS_CFS_READ_ONLY | FS_CFS_HIDDEN | FS_CFS_SYSTEM |FS_CFS_RENAME_INHIBIT |FS_CFS_DELETE_INHIBIT);

        FS_CFS_AllocateModifyInfo(&info, &size);
        ((NWMODIFY_INFO *)info)->FileAttributes=attributes;        
        
        if(isPathFullyQualified)
        {
            ccode = FS_CFS_GetDirNumber(fileOrDirHandle);
            if(ccode)
            {
				goto Return;
            }
        }
        infoMask=FS_CFS_MODIFY_ATTRIBUTES;
        ccode = FS_CFS_ModifyInfo(fileOrDirHandle, infoMask, size, info);
    }
    
Return:
    if(info)
		tsaFree(info);
    if(pathInfo)
    	tsaFree(pathInfo);
    if(name)
    	tsaFree(name);
    
    FEnd(ccode);
    return ccode;           
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_READ|CRITICAL|COMPACT|FILESYS
#define FNAME   "FS_CFS_DeleteFile"
#define FPTR     FS_CFS_DeleteFile

CCODE FS_CFS_DeleteFile(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 deleteFlags)
{
	CCODE 				ccode =0;
	NWHANDLE_PATH     	*pathInfo=NULL; 
	char			 	*path=NULL;
	UINT32  		 	fileDirLength=0;
    
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
			ccode = NWSMTS_INTERNAL_ERROR;
			goto Return;
		}	
		path[fileDirLength] =NULL;
	}
	else
	  path = NULL;
	
	ccode = NewFillHandleStruct(pathInfo, path, fileOrDirHandle->nameSpace, &(fileOrDirHandle->cfsStruct));
	if(ccode)
	  goto Return;
	
	
	ccode=GenNSErase(
					fileOrDirHandle->cfsStruct.clientConnID,
					~(fileOrDirHandle->cfsStruct.clientConnID), 
					pathInfo,
            		fileOrDirHandle->cfsStruct.cfsNameSpace, 
            		deleteFlags 
            		);
	if(ccode)
	{
		FLogError("GenNSErase", ccode, path);
		ccode = NWSMTS_DELETE_ERR;
	}
	
	
Return:
    if(path)
       tsaFree(path);
    if(pathInfo)
    	tsaFree(pathInfo);
	
	FEnd((CCODE)ccode);
	return ((CCODE)ccode);
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    RESTORE 
#define FNAME   "getComponent"
#define FPTR     getComponent
CCODE getComponent(char *component, UINT32 totalPathCount, UINT32 nameSpace)
{
    CCODE           ccode=0;
    UINT32          pathCount;
    char            *position =NULL;

    FStart();
    
    if(!component)
    {
        ccode = NWSMTS_INVALID_PATH;
        goto Return;
    }

    position = component;
            
    if((nameSpace == DOSNameSpace) || (nameSpace == OS2NameSpace) ||(nameSpace == NFSNameSpace))
    {
        for(pathCount=0; pathCount< totalPathCount; pathCount++)
        {
                position = strstr(position, _GetMessage(SLASH));
                position++;
        }
    }
    else if(nameSpace == MACNameSpace)
    {
        for(pathCount=0; pathCount< totalPathCount; pathCount++)
        {
                position = strstr(position, _GetMessage(COLON));
                position++;
        }
    }
    else
    {
        ccode = NWSMTS_INVALID_NAME_SPACE_TYPE;
        goto Return;
    }       

    if(--position)
      *(position) = NULL; //To delimit
    
Return:

    FEnd(ccode);
    return ccode;
}
