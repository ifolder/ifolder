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

#include <zPublics.h>
#include <zParams.h>
#include <zError.h>
#ifdef N_PLAT_NLM
#include <Errors.h>
#include <lfsproto.h>
#include <fsproto.h>
#include <filhandle.h>
#include "cfsdefines.h"
#include <dstruct.h>
#elif N_PLAT_GNU
#include <osprimitives.h>
#endif

#include <smsdefns.h>
#include <smstypes.h>

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

extern unsigned int tsazInfoVersion;  

/*************************************************************************************************************************************/

/*
 *  1. If the dataset is a volume, the dataset is opened using zopen()
 *  2. If the namespace is MAC or NFS, the volume name has to be truncated
 *     as the volume name in the absolute path is not accepted by these namespaces.
 *  3. If the dataset is already present and is read-only, the dataset has to be opened 
 *     and the read only attributes to be reset.
 *  4. If the dataset is a directory and the path does not exist, create the path nodes
 */


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_Create"
#define FPTR     FS_Create

CCODE FS_Create(FS_FILE_OR_DIR_HANDLE  			*fileOrDirHandle, 
	                FS_CREATE_MODE        		createMode, 
	                FS_ACCESS_RIGHTS      		accessRights, 
	                FS_ATTRIBUTES           	attributes, 
	                UINT32 					streamType, 
	                FS_HANDLE				*retHandle, 
	                unicode					*dSetName, 
	                UINT32					rflags, 
	                BOOL				isPathFullyQualified,
	                void*				buffer,
	                FS_HL_TABLE **hlTable) 
{
        STATUS					status;
        punicode				uniPathName = NULL; 
		unicode					*tmpPathPtr = NULL; 
  	  	unicode					*tmpOriginalPath = NULL; 
  		punicode				uniVolName = NULL; 
	    Key_t					tmpVolKey = 0, parentKey;  
    	Key_t					tmpDirKey = 0; 
        unicode					secondSeparator[UNI_SEP_BASE_SIZE]; 
        unicode					*pathCursor = NULL; 
        BOOL					isVolume = FALSE; 
        FS_ATTRIBUTES			tmpAttributes = 0; 
        UINT32					pathNodeCount;
        CCODE					ccode = 0;
        unicode					sep1[UNI_SEP_BASE_SIZE], sep2[UNI_SEP_BASE_SIZE];
        UINT32					nameSpace;

		FStart();
		
		if(streamType != DATASET_IS_EXTENDED_ATTRIBUTE && streamType != DATASET_IS_SECONDARY_DATASTREAM)
			nameSpace = MapToNSSNameSpace(fileOrDirHandle->nameSpace);
		else
			nameSpace = fileOrDirHandle->nameSpace;
        
        if(fileOrDirHandle == NULL)     
        {
            status = FS_INTERNAL_ERROR;
            goto Return;
        }
        
        if(attributes & FS_SUBDIRECTORY)
			createMode = FS_CREATE_OPEN_IF_THERE;
        else
        {
	        if(rflags & DATASET_EXISTS)
				createMode = FS_CREATE_OPEN_IF_THERE;
	        else
				createMode = FS_CREATE_OPEN_IF_THERE|FS_CREATE_TRUNCATE_IF_THERE;
        }

        if(nameSpace == zNSPACE_MAC) 
        {
			GetUniNameSpaceSeparators(MACNameSpace, NULL, secondSeparator);
        }
        else
        {
			GetUniNameSpaceSeparators(DOSNameSpace, NULL, secondSeparator);
        }       

        if(isPathFullyQualified)
        {
			tmpPathPtr = dSetName;
			if(GetPrimaryUniResource(&tmpPathPtr, nameSpace, &uniVolName)==NULL)
			{
		        status = NWERR_FS_INVALID_PATH;
		        goto Return;
			}
			if(!uninicmp(uniVolName, dSetName, MAX_VOL_NAME+1))
		        isVolume = TRUE;
        }

        /* For MAC and NFS the volume name has to be removed from the fully qualified path */
        if(isPathFullyQualified && !isVolume)  
        {
        	if((nameSpace == zNSPACE_MAC) || (nameSpace == zNSPACE_UNIX))
			{
			    if(!tmpPathPtr)
			    {
		            status = NWERR_FS_INVALID_PATH;
		            goto Return;
			    }

			    status = zOpen(
			            fileOrDirHandle->parentHandle.nssHandle,        // Root Key
			            fileOrDirHandle->taskID,                                        // zNSS_TASK
			            zNSPACE_LONG,                                                           // Namespace
			            uniVolName,                                                                     // Name of the object
			            zRR_READ_ACCESS | zRR_SCAN_ACCESS,                      // Requested rights
			            &tmpVolKey                                                                      // Returned key to the object
			            );
			    
			    if(status)
			    {
			        FLogError("zOpen", status, NULL);
			    }
			    
			    if(status)
			    {
		            tmpVolKey = 0;
		            goto Return;
			    }

			    if(tmpPathPtr)
			    {
		            tmpDirKey = fileOrDirHandle->parentHandle.nssHandle;
		            fileOrDirHandle->parentHandle.nssHandle = tmpVolKey;
		            tmpOriginalPath = dSetName;
		            dSetName = tmpPathPtr;
			    }
			}
        }

        /* 
         * Convert dSetName from ascii to unicode.
         *
         * Also setting fileOrDirHandle->uniPath. This is required in case of engines which
         * pass terminal node name for a file. Here the absolute path is obtained from the
         * parent handle (the directory in which the file resides).
         */
        if(dSetName) 
        {
			uniPathName = (punicode) tsaMalloc((unilen(dSetName) + 1 ) * sizeof(unicode));
			if (uniPathName == NULL) 
			{
			    status = NWSMTS_OUT_OF_MEMORY;
			    goto Return;
			}
			unicpy(uniPathName, dSetName);

			if(tmpOriginalPath)
				dSetName = tmpOriginalPath;
        }

        if(nameSpace == FS_NSPACE_DATA_STREAM ||         
            nameSpace == FS_NSPACE_EXTENDED_ATTRIBUTE)
            parentKey = fileOrDirHandle->handleArray[0].handle.nssHandle;
        else
			parentKey = fileOrDirHandle->parentHandle.nssHandle;
        
        if(isVolume)  
        {
                status = zOpen(
                                fileOrDirHandle->parentHandle.nssHandle,        // Root Key
                                fileOrDirHandle->taskID,                                                                                        // zNSS_TASK
                                zNSPACE_LONG,                                                           // Namespace
                                uniPathName,                                                            // Name of the object
                                zRR_READ_ACCESS | zRR_SCAN_ACCESS|zRR_WRITE_ACCESS,     // Requested rights
                                &retHandle->nssHandle                                           // Returned key to the object
                                );
                
                if(status)
	            {
	                FLogError("zOpen", status, NULL);
	            }


                goto Return;
        }
        
        status = zCreate(
                                parentKey, 
                                fileOrDirHandle->taskID,
                                zNILXID,
                                nameSpace,
                                uniPathName,
                                (nameSpace == FS_NSPACE_DATA_STREAM ? zFILE_NAMED_DATA_STREAM: \
                                (nameSpace == FS_NSPACE_EXTENDED_ATTRIBUTE ? zFILE_EXTENDED_ATTRIBUTE: \
                                zFILE_REGULAR)),        /* file type */ /* TBD Need to validate this.*/
                                attributes,                     /* file attributes*/
                                createMode,
                                accessRights, 
                                &retHandle->nssHandle
                        );
        
    if(status)
	{
    	FLogError("zCreate", status, NULL);
	}

        /* If the path does not exist, create the path nodes by looping though each node in the path */
        if(status && isPathFullyQualified) 
        {
                pathCursor = uniPathName;
                /* For restore of single file */
                //if (!fileOrDirHandle->isDirectory) 
                {
                        status = GetUniNameSpaceSeparators(nameSpace, sep1, sep2);
                        if (status)
                                goto Return;
                        pathNodeCount = GetNumberOfNodeNamesInTheUniPath(dSetName, unilen(dSetName), sep1, sep2);
                        /* Excluding the volume name */
                        pathNodeCount = pathNodeCount -1; 
                }
                
                while(pathCursor)
                {
                        pathCursor = unichr(pathCursor, (unicode)secondSeparator[0]);
                        if(pathCursor)
                        {
                                (*pathCursor) = 0;
                        }

                        /* For single file restore */
                        --pathNodeCount;
                        if (!fileOrDirHandle->isDirectory && !pathNodeCount) 
                                tmpAttributes = 0;
                        else
                                tmpAttributes = zFA_SUBDIRECTORY;

                
                        status = zCreate(
                                fileOrDirHandle->parentHandle.nssHandle,        // Root Key
                                fileOrDirHandle->taskID,                                                // zNSS_TASK
                                zNILXID,                                                                        // Transaction ID
                                nameSpace,                                     // Namespace
                                uniPathName,                                                            // Name of directory
                                zFILE_REGULAR,                                                          // File type
                                tmpAttributes,                                                          // File attributes  
                                zCREATE_OPEN_IF_THERE,                                          // Create Mode
                                (zRR_READ_ACCESS|zRR_WRITE_ACCESS|zRR_DONT_UPDATE_ACCESS_TIME), // Requested rights
                                &retHandle->nssHandle                                           // Returned key to the dir
                                );
                        if((status != zOK) && (pathNodeCount==0)) 
                        {
                            FLogError("zCreate", status, NULL);
                                break;
                        }
                        /* For single file restore */
                        if (!fileOrDirHandle->isDirectory && pathNodeCount == 0)  
                                break;  
                
                        if(pathCursor)
                        {
                                (*pathCursor) = secondSeparator[0];
                                pathCursor++;
                        }
                        if(pathCursor && retHandle->nssHandle)//close all handle's except the last one
                        {
                                zClose(retHandle->nssHandle);
                                retHandle->nssHandle=0;
                        }
                }
        }

		if(status)
        {
			if(fileOrDirHandle->isDirectory) 
		        ccode = NWSMTS_CREATE_DIR_ENTRY_ERR; //for directory
			else  
		        ccode = NWSMTS_CREATE_ERROR; //for files
        }

        /* Setting TSA specific error code so that the engine can interpret the error */
        if(status == zERR_OUT_OF_SPACE) 
	        ccode = NWSMTS_OUT_OF_DISK_SPACE;

        /* If the dataset exists and not able to open the file */
        if(status && (rflags & DATASET_EXISTS)) 
        {
            if(status == zERR_FILE_WRITE_LOCKED || status == zERR_FILE_READ_LOCKED || status == zERR_CANT_DENY_READ_LOCK || status == zERR_CANT_DENY_WRITE_LOCK || status == zERR_LAST_STATE_UNKNOWN)
                ccode = NWSMTS_DATA_SET_IN_USE;
            else
                ccode = NWSMTS_OPEN_ERROR;
        }


Return:
        if(tmpVolKey) 
        {
            zClose(tmpVolKey);
            fileOrDirHandle->parentHandle.nssHandle = tmpDirKey;
        }

        if(uniVolName)
            tsaFree(uniVolName);

        if(uniPathName) 
            tsaFree(uniPathName);

		FEnd(ccode);
		
        if(ccode)
            return ccode;
        else
			return status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_SetCompressedFileSize"
#define FPTR     FS_SetCompressedFileSize

CCODE FS_SetCompressedFileSize(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD newSize) 
{
        CCODE ccode = 0;

        ccode = zSetEOF(
                fileOrDirHandle->handleArray[0].handle.nssHandle, 
                zNILXID,
                newSize,
                zSETSIZE_LOGICAL_ONLY);
        
    if(ccode)
    {
        char temp[64] ;
        sprintf(temp, "setting %s. size=%-8.8X%-8.8X", "logical", *((UINT32*)&(newSize)+1), *(UINT32*)&(newSize));
        FLogError("zSetEOF", ccode, temp);
    }

        return ccode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_SetFileSize"
#define FPTR     FS_SetFileSize 

CCODE FS_SetFileSize(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD newSize, UINT32 handleIndex) 
{
        CCODE ccode = 0;
        
        // 1. De-Allocate the blocks 
        ccode = zSetEOF(
                fileOrDirHandle->handleArray[handleIndex].handle.nssHandle, 
                zNILXID,
                newSize,
                zSETSIZE_PHYSICAL_ONLY);

    if(ccode && ccode !=zERR_PHYSICAL_EOF_NOT_ENABLED)
    {
        char temp[64] ;
        sprintf(temp, "setting %s. size=%-8.8X%-8.8X", "physical", *((UINT32*)&(newSize)+1), *(UINT32*)&(newSize));
        FLogError("zSetEOF", ccode, temp);
    }
        // 2. Set logical file size 
        if (!ccode)
        {
                ccode = zSetEOF(
                        fileOrDirHandle->handleArray[handleIndex].handle.nssHandle, 
                        zNILXID,
                        newSize,
                        zSETSIZE_LOGICAL_ONLY);
        if(ccode)
        {
            char temp[64] ;
            sprintf(temp, "setting %s. size=%-8.8X%-8.8X", "logical", *((UINT32*)&(newSize)+1), *(UINT32*)&(newSize));
            FLogError("zSetEOF", ccode, temp);
        }
        
        }
        //for dosfat_c files    
        if(ccode==zERR_PHYSICAL_EOF_NOT_ENABLED)
        {
                ccode = zSetEOF(
                fileOrDirHandle->handleArray[handleIndex].handle.nssHandle, 
                zNILXID,
                newSize,
                zSETSIZE_NON_SPARSE_FILE);
        }

        return ccode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_SetFileSizeForMigratedFile"
#define FPTR     FS_SetFileSizeForMigratedFile 
CCODE FS_SetFileSizeForMigratedFile(QUAD handle, QUAD size)
{
        CCODE ccode = 0;
        ccode = zSetEOF(
                        handle, 
                        0,
                        size,
                        zSETSIZE_LOGICAL_ONLY); 
        return ccode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    RESTORE
#define FNAME   "FS_SetPrimaryNameSpace"
#define FPTR     FS_SetPrimaryNameSpace
CCODE FS_SetPrimaryNameSpace(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 oldNameSpace, UINT32 newNameSpace)
{
        STATUS  status = 0;
        zInfo_s modifyInfo;

        modifyInfo.primaryNameSpaceID = newNameSpace;
                
        status = zModifyInfo(
                fileOrDirHandle->handleArray[0].handle.nssHandle,
                zNILXID,
                zMOD_PRIMARY_NAMESPACE, //primary name space is the old namespace
                sizeof(zInfo_s),
                zINFO_VERSION_A,
                &modifyInfo);

        return status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    RESTORE|FILESYS
#define FNAME   "FS_SetNameSpaceName"
#define FPTR     FS_SetNameSpaceName
CCODE FS_SetNameSpaceName(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT8 nameSpace, void *newDataSetName)
{

	CCODE                   ccode = 0;
	unicode_t*              unicodePathName=NULL;
	zInfo_s                 *objectInfo = NULL;
	Key_t                   dirHandle=0;
	BOOL               dirHandleOpened=FALSE;
	unicode                 *pathCursor = NULL; 
	unicode                 sepColon[UNI_SEP_BASE_SIZE]; 
	UINT32                  size = 0;

	FStart();

	nameSpace	 = MapToNSSNameSpace(nameSpace);

	size = 8192;
	objectInfo = tsaMalloc(size);
	if(!objectInfo)
	{
		ccode=NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}

	// 1. check the the name availability of file/dir to renamed (1 and 2 are NSS work Around for defect 285953)

	ccode = zGetInfo(
					fileOrDirHandle->handleArray[0].handle.nssHandle,
					zGET_ALL_NAMES,
					size,
					zINFO_VERSION_A,
					objectInfo
					);
	if(!ccode)
	{
		unicodePathName = zInfoGetFileName(objectInfo, nameSpace); 
		if(!unicodePathName)
		{
	        FLogError("zInfoGetFileName", ccode, NULL);
	        ccode = NWSMTS_GET_NAME_SPACE_ENTRY_ERR;//No name exits in the file system for this specfic name name so can't rename
	        goto Return;
		}

		if (!(ccode = unicmp((unicode*)newDataSetName, unicodePathName)))
		{
			/* Since the filesystem already has created names in namespaces other than the primary namespace.
			    They are the same as the name that was present during backup (from SIDF stream).  So we do not 
			    need to rename them at all.
			*/
			goto Return;
		}

	}
	else
	{
		FLogError("zGetInfo", ccode, NULL);
		ccode = NWSMTS_GET_NAME_SPACE_ENTRY_ERR;
		goto Return;
	}    

	// 2. Get the handle
	GetUniNameSpaceSeparators(DOSNameSpace, sepColon, NULL);
	pathCursor = unichr(fileOrDirHandle->uniPath, sepColon[0]);
	if(pathCursor)
	{
		//After the NSS fix(defect )remove the file just do a zOpen for the dir and get the handle
		ccode = FS_WorkAroundGetDirHandle(fileOrDirHandle, &dirHandle);//fullyQualified Path
		if(ccode)
			goto Return;
			dirHandleOpened=TRUE;
	}
	else  
		dirHandle=fileOrDirHandle->parentHandle.nssHandle;//use the parent handle

	//After NSS fix(285953)remove 1 and 2 and use the file/dir handle(fileOrDirHandle->handleArray[0].handle.nssHandle) to rename
	// 3. Rename the file/dir
	ccode = zRename(
					dirHandle,
					zNILXID,
					nameSpace, 
					unicodePathName,                                
					zMATCH_ALL,
					nameSpace,
					(unicode*)newDataSetName,
					zRENAME_THIS_NAME_SPACE_ONLY | zRENAME_ALLOW_RENAMES_TO_MYSELF
					);

	if(ccode ==zERR_LINK_IN_PATH)  
	{
		ccode = zRename(
					    dirHandle,
					    zNILXID,
					    nameSpace,
					    unicodePathName,
					    zMATCH_ALL,
					    nameSpace | zMODE_LINK, 
					    (unicode*)newDataSetName,
					    zRENAME_THIS_NAME_SPACE_ONLY | zRENAME_ALLOW_RENAMES_TO_MYSELF
					    );
	}

	if(ccode)
	{       
		if(ccode !=zERR_LINK_IN_PATH && ccode !=zERR_NO_RENAME_PRIVILEGE && ccode !=zERR_RENAME_DIR_INVALID)
		{
		        FLogError("zRename", ccode, NULL);
		        ccode= NWSMTS_WRITE_ERROR;
		        goto Return;
		}
		ccode = 0;
	}  
        
Return:
	if(objectInfo)
		tsaFree(objectInfo);
	if(dirHandle && dirHandleOpened)
		zClose(dirHandle);
	FEnd(ccode);
	return ccode;
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    RESTORE|FILESYS
#define FNAME   "FS_WorkAroundGetDirHandle"
#define FPTR     FS_WorkAroundGetDirHandle
CCODE FS_WorkAroundGetDirHandle(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, Key_t *parentHandle)//NSS defect work around function
{
    CCODE                   ccode=0;
    unicode                 *uniPathtoDir=NULL;
    unicode                 *pathCursor = NULL; 
    unicode                 *uniVolume=NULL;
    Key_t                   volOrRootHandle=0;
    unicode                 dirSep[UNI_SEP_BASE_SIZE], volSep[UNI_SEP_BASE_SIZE]; 
    BOOL               isVolume=FALSE;
    UINT32					nameSpace;

    FStart();

	nameSpace = MapToNSSNameSpace(fileOrDirHandle->nameSpace);
    
    if(!fileOrDirHandle->uniPath)
    {
        ccode = NWSMTS_INVALID_PATH;
        goto Return;
    }
    uniPathtoDir = tsaMalloc((unilen(fileOrDirHandle->uniPath)+1)*sizeof(unicode));
    if(!uniPathtoDir)
    {
        ccode=NWSMTS_OUT_OF_MEMORY;
        goto Return;
    }

    unicpy(uniPathtoDir, fileOrDirHandle->uniPath);
    GetUniNameSpaceSeparators(DOSNameSpace, volSep, dirSep);
    
    if((nameSpace == zNSPACE_MAC) || (nameSpace == zNSPACE_UNIX))
    {
        //take the volume name
        pathCursor = unichr(uniPathtoDir, volSep[0]);
        if(pathCursor)
        {
            pathCursor++; 
            *pathCursor=0;
        }
        else
        {
            ccode = NWSMTS_INVALID_PATH;
            goto Return;
        }
        uniVolume = tsaMalloc((20*sizeof(unicode)));
        if(!uniPathtoDir)
        {
            ccode=NWSMTS_OUT_OF_MEMORY;
            goto Return;
        }
        unicpy(uniVolume, uniPathtoDir);
        //Open the volume
        ccode =zOpen(
                    fileOrDirHandle->parentHandle.nssHandle,
                    fileOrDirHandle->taskID,
                    zNSPACE_LONG,
                    uniVolume,
                    zRR_READ_ACCESS | zRR_SCAN_ACCESS|zRR_DONT_UPDATE_ACCESS_TIME,  
                    &volOrRootHandle
                    );
        if(ccode)
        {
            FLogError("zOpen", ccode, NULL);
            ccode = NWSMTS_OPEN_ERROR;
            goto Return;
        }
        //take the name-excluding volume
        pathCursor = unichr(fileOrDirHandle->uniPath, volSep[0]);
        if(pathCursor) pathCursor++;
        else{ccode = NWSMTS_INVALID_PATH; goto Return;}
        if(*pathCursor==volSep[0])pathCursor++;
        unicpy(uniPathtoDir, pathCursor);
    }

    //strip the child(file) if present in the path
    if(nameSpace == zNSPACE_MAC)
    {
        pathCursor = unirchr(uniPathtoDir, volSep[0]);
        if(pathCursor)
        	*pathCursor=0;//file under directory
        else
        {
            memcpy(parentHandle, &volOrRootHandle, sizeof(SQUAD));
            goto Return;
        }
    }
    else
    {
	    if(nameSpace == zNSPACE_UNIX)
	    {
	        pathCursor = unirchr(uniPathtoDir, dirSep[0]);
	        if(pathCursor)
	            *pathCursor=0;
	        else
	        {
	            memcpy(parentHandle, &volOrRootHandle, sizeof(SQUAD));
	            goto Return;
	        }
	    }
	    else
	    {       
	        pathCursor = unirchr(uniPathtoDir, dirSep[0]);
	        if(pathCursor)
	        {
				*pathCursor=0;//dir need to be opened
	        }
	        else
	        {       
	            pathCursor = unirchr(uniPathtoDir, volSep[0]);  
	            pathCursor++;
	            *pathCursor=0;
	            isVolume=TRUE;//volume need to be opened, file under volume
	        }
	    }
    }
    //Open the dir/volume
    ccode =zOpen(
                volOrRootHandle? volOrRootHandle:fileOrDirHandle->parentHandle.nssHandle,
                fileOrDirHandle->taskID,
                isVolume? zNSPACE_LONG: nameSpace,
                uniPathtoDir,
                zRR_READ_ACCESS | zRR_SCAN_ACCESS|zRR_DONT_UPDATE_ACCESS_TIME,  
                parentHandle
                );
    if(ccode)
	{
        FLogError("zOpen", ccode, NULL);
        ccode = NWSMTS_OPEN_ERROR;
    }
    if(volOrRootHandle)
		zClose(volOrRootHandle);
    
Return:
	
    if(uniVolume)
    	tsaFree(uniVolume);
    if(uniPathtoDir)
    	tsaFree(uniPathtoDir);
    FEnd(ccode);    
    return ccode;
} 

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    RESTORE 
#define FNAME   "removeVolName"
#define FPTR     removeVolName
char * removeVolName(char *path, UINT32 buffsize, UINT32 nameSpace)
{
        char            *position =NULL;

        FStart();
        
        if(!path)
        {
                goto Return;
        }

        if((nameSpace == DOSNameSpace) || (nameSpace == OS2NameSpace) || (nameSpace == NFSNameSpace))
        {
          position = strstr(path, _GetMessage(COLON));
          if(!position)
                goto Return; 
          position++;
                
          //if(*position != 0 && strncmp(position, (char*) _GetMessage(SLASH), strlen((char*) _GetMessage(SLASH)))  
          if(*position != 0 && *position == *((char*) _GetMessage(SLASH)))
                position++;
        }
        else if(nameSpace == MACNameSpace)
        {
          position = strstr(path,_GetMessage(COLONS));
          if(!position)
                goto Return; 
          position ++;
          position ++;
        }
        Return:

          FEnd(0);      
          return position;
}       

