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
 | $Modtime:   27 Mar 2002 17:04:16  $
 |
 | $Workfile:   fsWrite.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Initialise TSA
 +-------------------------------------------------------------------------*/

#include <compath.h>
#include <filhandle.h>
#include <fs_sidf.h>
#include <fsinterface.h>
#include <smsdebug.h>

#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>

#include <Errors.h>
#include <lfsproto.h>
#include <fsproto.h>
#include <stdlib.h>
#include <string.h>
#include <nwbitops.h>


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN|NFS_CHARCATERISTICS
#define FNAME   "FS_CFS_SetNameSpaceHugeInfo"
#define FPTR     FS_CFS_SetNameSpaceHugeInfo
CCODE FS_CFS_SetNameSpaceHugeInfo(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, char *hugeData,  UINT32 nameSpace)
{
	CCODE                                  ccode=0;
	char                                      *oriHugeData=NULL;
	UINT32                                  i = 0, j=0;
	UINT32                                  numberOfLinks=0;
	UINT8                                   stateInfo[NWSTATE_INFO_SIZE] = { 0 };
	UINT32                                  hugeDataLength = 0;
	UINT32                                  validState = TRUE;

	/* The hugeData buffer layout
	UINT32 numberOfLinks;
	UINT32 pathlength; - this includes the directory
	STRING pathname;  - length preceeded string
	LONG  UID
	LONG GID
	LONG MODE INDEX
	*/

	FStart();

	if(hugeData == NULL)
		goto Return;	

	oriHugeData = hugeData; //preserving the buffer base address

	numberOfLinks = *hugeData;
	hugeData         = hugeData+sizeof(UINT32);

	for(i=0; i<numberOfLinks; i++)
	{
        hugeDataLength = *hugeData;
        hugeData          = hugeData+sizeof(UINT32);    
        ccode = SetHugeInformation(
                                 fileOrDirHandle->cfsStruct.clientConnID,
                                 ~(fileOrDirHandle->cfsStruct.clientConnID),
                                 (UINT8)0x02,//fileOrDirHandle->cfsStruct.cfsNameSpace,
                                 fileOrDirHandle->cfsStruct.volumeNumber,
                                 //nsDirBase,
                                 fileOrDirHandle->cfsStruct.directoryNumber,
                                 0x400,
                                 stateInfo,
                                 hugeDataLength,
                                 hugeData,
                                 stateInfo,
                                 &hugeDataLength//not used       
                                 );
       if(!ccode)
       {
            validState = FALSE;
            if(hugeDataLength == 0)
                continue;
                  
            hugeData = hugeData + hugeDataLength;
            for(j = 0; j < sizeof(stateInfo); j++) 
            {
                if(stateInfo[i])      
                {
                        validState = TRUE;
                }
            }
         }
       else
       {
            FLogError("SetHugeInformation failed", ccode, NULL);
            ccode = NWSMTS_CREATE_ERROR;
            break;
       }
	}

	hugeData = oriHugeData; //ponter preserved

Return:

	FEnd(ccode);        
	return ccode;
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|RESTORE
#define FNAME   "FS_CFS_Write"
#define FPTR     FS_CFS_Write
CCODE FS_CFS_Write(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD startingOffset, UINT32 bytesToWrite, void *buffer, UINT32 *bytesWritten, UINT32 handleIndex)
{
	CCODE 					ccode=0;
	FS_WRITE_HANDLE 		writeHandle={0};
	char					*path=NULL;

	FStart();
	
	if(fileOrDirHandle == NULL || bytesWritten == NULL || buffer == NULL)
	{
		ccode = NWSMTS_INVALID_PARAMETER;
		goto Return;
	}

	//consistancy maintained with backup
	writeHandle.writeFlag	 = fileOrDirHandle->handleArray[handleIndex].type;
	writeHandle.currentOffset    = startingOffset;
	writeHandle.handle.cfsHandle = fileOrDirHandle->handleArray[handleIndex].handle.cfsHandle;
	
	//Data streams
	
	if((writeHandle.writeFlag == FS_PRIMARY_DATA_STREAM) || (writeHandle.writeFlag == FS_DATA_STREAM))
	{

		ccode = WriteFile(
						fileOrDirHandle->cfsStruct.clientConnID,
						writeHandle.handle.cfsHandle,
						(UINT32)startingOffset,
						bytesToWrite,
						buffer
					   );
		if(ccode)
		{
			//FS_CFS_GetFullPathName(fileOrDirHandle, &path);
			FLogError("WriteFile", ccode, NULL);
			if(path) {tsaFree(path); path=NULL;}
			if(ccode==ERR_INSUFFICIENT_SPACE)
				ccode=NWSMTS_OUT_OF_DISK_SPACE;
			else
				ccode = NWSMTS_WRITE_ERROR;
			goto Return;
		}
		else
			*bytesWritten =bytesToWrite;
	}
	//EA
	
	if(writeHandle.writeFlag == DATASET_IS_EXTENDED_ATTRIBUTE)
	{
		ccode=WriteEAData(
						fileOrDirHandle->cfsStruct.clientConnID, 
						~(fileOrDirHandle->cfsStruct.clientConnID), 
			  	  		writeHandle.handle.cfsHandle, 
			  	  		(UINT32)(fileOrDirHandle->handleArray[handleIndex].size), 
			  	  		(UINT32)startingOffset, 
			  	  		fileOrDirHandle->handleArray[handleIndex].eaAttrFlags,
						(startingOffset) ? 0 : strlen(fileOrDirHandle->handleArray[handleIndex].eaName),
						(startingOffset) ? NULL : fileOrDirHandle->handleArray[handleIndex].eaName,
						bytesToWrite, 
						buffer,
			    		bytesWritten
			    		);
		
		if(ccode)
		{
			//FS_CFS_GetFullPathName(fileOrDirHandle, &path);
			FLogError("WriteEAData", ccode, NULL);
			if(path) {tsaFree(path); path=NULL;}
			if(ccode==ERR_INSUFFICIENT_SPACE)
				ccode=NWSMTS_OUT_OF_DISK_SPACE;
			else
				ccode = NWSMTS_WRITE_EA_ERR;
			goto Return;
		}
	}

	if(bytesToWrite != (*bytesWritten))
	FTrack3(RESTORE, DC_CRITICAL|DC_COMPACT, "startingOffset : %lu bytesToWrite : %lu bytesWritten : %lu\n", 
				startingOffset, bytesToWrite, *bytesWritten);
	
Return:
	FEnd(ccode);
	return ccode;
}
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    RESTORE|FILESYS
#define FNAME   "FS_CFS_SetCompressedFileSize"
#define FPTR     FS_CFS_SetCompressedFileSize
CCODE FS_CFS_SetCompressedFileSize(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD newSize) 
{
    CCODE                    ccode = 0;
    UINT32		  			 oldSize;
    char                    *path=NULL;

    FStart();

	ccode = SetCompressedFileSize(fileOrDirHandle->cfsStruct.clientConnID, fileOrDirHandle->handleArray[0].handle.cfsHandle, newSize, &oldSize);
    
    if(ccode)
    {
            //ConsolePrintf("Write error in FS_CFS_SetCompressedFileSize\r\n");
            //FS_CFS_GetFullPathName(fileOrDirHandle, &path);
            FLogError("WriteFile", ccode, NULL);
            if(path) {tsaFree(path); path=NULL;}
			if(ccode==ERR_INSUFFICIENT_SPACE)
				ccode=NWSMTS_OUT_OF_DISK_SPACE;
			else	
				ccode = NWSMTS_WRITE_ERROR;
    }
	FEnd(ccode);
    return ccode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    RESTORE|FILESYS
#define FNAME   "FS_CFS_SetFileSize"
#define FPTR     FS_CFS_SetFileSize 
CCODE FS_CFS_SetFileSize(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD newSize, UINT32 handleIndex) 
{
	CCODE                   ccode = 0;

	FStart();

	ccode = WriteFile(fileOrDirHandle->cfsStruct.clientConnID,  fileOrDirHandle->handleArray[handleIndex].handle.cfsHandle, (UINT32)newSize, 0, "");
	if(ccode)
	    FLogError("WriteFile", ccode, NULL);

	FEnd(ccode);
	return ccode;
}




#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    RESTORE
#define FNAME   "FS_CFS_SetPrimaryNameSpace"
#define FPTR     FS_CFS_SetPrimaryNameSpace
CCODE FS_CFS_SetPrimaryNameSpace(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 oldNameSpace, UINT32 newNameSpace)
{
    CCODE				ccode=0;
    NWHANDLE_PATH       *pathInfo=NULL;
    UINT32				searchAttributes=0;
    UINT32				fileDirLength=0;
    char    			*path=NULL; 
    unicode        		*uniPath=NULL;

    FStart();

    if(fileOrDirHandle->isDirectory)
	    uniPath=NULL;
    else
        uniPath=fileOrDirHandle->cfsStruct.uniFileDirName;

    pathInfo = (NWHANDLE_PATH *)tsaMalloc(sizeof(NWHANDLE_PATH));
    if(!pathInfo)
	{
        ccode = NWSMTS_OUT_OF_MEMORY;
        goto Return;
    }
    
    if(uniPath)
    {       
        ccode = SMS_UnicodeToByte(uniPath, &fileDirLength, &path, NULL); 
        if(ccode)
        {
            {ccode = NWSMTS_INTERNAL_ERROR; FTrack(INTERNAL_ERROR,DC_CRITICAL|DC_COMPACT, "Internal Error\n"); }
            goto Return;
        }       
        path[fileDirLength] = NULL; //this is a file path 
    }
    else
        path = NULL;

    if(fileOrDirHandle->isDirectory)
		searchAttributes=0x16;
    else
		searchAttributes=0x06;
    
    ccode = NewFillHandleStruct((NWHANDLE_PATH *)pathInfo, path, fileOrDirHandle->cfsStruct.cfsNameSpace, &(fileOrDirHandle->cfsStruct));
    if(ccode)
    {
        ccode=NWSMTS_INVALID_PATH;
        goto Return;    
    }
    ccode = SetOwningNameSpace(
                            fileOrDirHandle->cfsStruct.clientConnID,
                            fileOrDirHandle->cfsStruct.volumeNumber,
                            pathInfo->DirectoryBaseOrHandle,
                            pathInfo->PathString,
                            pathInfo->PathComponentCount,  
                            oldNameSpace,
                            newNameSpace
                            );
    if(ccode)
    {
        FLogError("SetOwningNameSpace", ccode, NULL);
        ccode = NWSMTS_SET_FILE_INFO_ERR;
    }
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
#define FTYPE    RESTORE|FILESYS
#define FNAME   "FS_CFS_SetNameSpaceName"
#define FPTR     FS_CFS_SetNameSpaceName
CCODE FS_CFS_SetNameSpaceName(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT8 nameSpace, void *newDataSetName)
{
    CCODE					ccode=0;
    NWHANDLE_PATH			*pathInfo=NULL;
    UINT32					searchAttributes=0;
    UINT32					fileDirLength=0;
    char					*path=NULL; 
    unicode					*uniPath=NULL;
    LONG					pathBase=0;
    struct DirectoryStructure  *dStruct=NULL;
    unsigned char			*component=NULL;

	FStart();

	if(fileOrDirHandle->isDirectory)
		uniPath=NULL;
	else
		uniPath=fileOrDirHandle->cfsStruct.uniFileDirName;

	pathInfo = (NWHANDLE_PATH *)tsaMalloc(sizeof(NWHANDLE_PATH));
	if(!pathInfo)
	{
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
    
	if(uniPath)
	{       
		ccode = SMS_UnicodeToByte(uniPath, &fileDirLength, &path, NULL); 
		if(ccode)
		{
	        {ccode = NWSMTS_INTERNAL_ERROR; FTrack(INTERNAL_ERROR,DC_CRITICAL|DC_COMPACT, "Internal Error\n"); }
	        goto Return;
		}       
		path[fileDirLength] =NULL; //this is a file path 
	}
	else
		path=NULL;

    
	if(fileOrDirHandle->isDirectory)
		searchAttributes=0x16;
	else
		searchAttributes=0x06;
    
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
									fileOrDirHandle->cfsStruct.cfsNameSpace,
									&dStruct,
									&pathBase                               
									);
	if(ccode)
	{
		FLogError("GetEntryFromPathStringBase", ccode, NULL);
		ccode = NWSMTS_GET_ENTRY_INDEX_ERR;
		goto Return;
	}

       
    ccode = _NWStoreAsComponentPath(
										(STRING) &(pathInfo->PathComponentCount),
										(UINT8)fileOrDirHandle->cfsStruct.cfsNameSpace,         
										(char*)newDataSetName,                                                        
										FALSE //pathIsFullyQualified
										);
	if(ccode)
	{
		ccode=NWSMTS_INVALID_PATH;      
		goto Return;
	}
    ccode = GenNSModifySpecificInfo(
								fileOrDirHandle->cfsStruct.clientConnID,
								~(fileOrDirHandle->cfsStruct.clientConnID),
								(UINT8)fileOrDirHandle->cfsStruct.cfsNameSpace,
								(UINT8)nameSpace,
								(UINT8)fileOrDirHandle->cfsStruct.volumeNumber,
								pathBase,
								NWMODIFY_NAME,
								/*strlen(newDataSetName) + 1, */pathInfo->PathString[0]+1,
								(BYTE *)pathInfo->PathString
								);
	if(ccode)
	{
		if(ccode !=ERR_NO_RENAME_PRIVILEGE && ccode != ERR_RENAME_DIR_INVALID)
		{
	        FLogError("GenNSModifySpecificInfo", ccode, NULL);
	        ccode = NWSMTS_WRITE_ERROR;
	        goto Return;
		}
		ccode=0;                        
	}

Return:
	if(component)
		tsaFree(component);
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
#define FTYPE    FILESYS|CFS_READ
#define FNAME   "FS_CFS_SetSparseStatus"
#define FPTR     FS_CFS_SetSparseStatus

CCODE FS_CFS_SetSparseStatus(FS_FILE_OR_DIR_HANDLE *fsHandles)
{
	CCODE 		ccode=0;
	LONG 		startingOffset=0;
	QUAD 		streamSz;
	int			bytes;
	int 		tmp=0;
	LONG 		blkSz;
	QUAD		totSize;
	int 		count, blocks;
	void 		*bitMap=NULL;
	UINT32		dataInLastBlk, avblBlocks, bit;

	FStart();
	
	if(fsHandles->isDirectory)
		goto Return;		
		
	for(count = 0; (count < (fsHandles->streamCount)); count++)
	{
		streamSz = fsHandles->handleArray[count].size;
		if (streamSz == 0)
			continue;
		// get file block size only, pass 0 buffer size 
		
		ccode=GetFileHoles(fsHandles->cfsStruct.clientConnID,fsHandles->handleArray[count].handle.cfsHandle,\
           				   startingOffset, 0, bitMap, &blkSz );
		
		if(ccode)
		{
	        FLogError("GetFileHoles", ccode, 0);
			goto Return;
		}
		blocks = (int)(streamSz/blkSz);
		blocks += (streamSz % blkSz)?1:0;
		
		bytes = blocks/8;
		bytes += (blocks % 8)?1:0;
		
		if (bytes % 4)
		{
			tmp = 4 - bytes % 4;
			bytes = bytes + tmp;
		}
		
		bitMap = (void *)tsaMalloc(bytes);
		if(!bitMap)
		{
			ccode=NWSMTS_OUT_OF_MEMORY;
			goto Return;
		}
		
		memset(bitMap, 0, bytes);
		
		ccode=GetFileHoles(fsHandles->cfsStruct.clientConnID,fsHandles->handleArray[count].handle.cfsHandle,\
           				  startingOffset, blocks, bitMap, &blkSz );
		
		if(ccode)
		{
			tsaFree(bitMap);
			goto Return;
		}
		
		if (ScanClearedBits(bitMap, 0L,blocks) == -1)
    	{
    		tsaFree(bitMap);
    		continue;
    	}
		bit = 0;
		avblBlocks = 0;
    	while ((bit = ScanBits(bitMap, bit, bytes * 8)) != -1)
    	{
			bit ++;
			avblBlocks ++;
				
    	}
    	totSize = avblBlocks * blkSz;
    	if ((dataInLastBlk = (streamSz % blkSz)))
    	{
			if (ScanBits(bitMap, blocks - 1, blocks) != -1)
				totSize -= (blkSz - dataInLastBlk);
    	}
    	
    	fsHandles->handleArray[count].streamMap = (unsigned char *)bitMap;
		fsHandles->handleArray[count].streamMapSize = bytes;
		fsHandles->handleArray[count].blkSz = blkSz;
		fsHandles->handleArray[count].dataSz = totSize;
		fsHandles->handleArray[count].type = DATASET_IS_SPARSE;
		
	}

Return:
	FEnd(ccode);
	return ccode;
}
void FS_CFS_SetMigratedStatus(FS_FILE_OR_DIR_HANDLE *fsHandles, FS_FILE_OR_DIR_INFO scanInfo)
{
	int       j;
	for(j=0;j<fsHandles->streamCount;j++)
		fsHandles->handleArray[j].type = fsHandles->handleArray[j].type | DATASET_IS_MIGRATED;
}


#undef FTYPE
#undef FNAME
#undef FPTR
#define FTYPE RESTORE|FILESYS|MINOR|COMPACT|BASIC_CHARACTERISTICS
#define FNAME "FS_CFS_SetSpecificCharacteristics"
#define FPTR FS_CFS_SetSpecificCharacteristics
CCODE FS_CFS_SetSpecificCharacteristics(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, void  *buffer, UINT32 size, UINT32 nameSpace, UINT32 infoMask)
{
        CCODE           ccode=0;
        UINT32          pathBase=0;
        UINT32          nameSpaceDirBase=0;
        char            *path=NULL;

        FStart();

        if(!fileOrDirHandle || ! buffer)
        {
                ccode=NWSMTS_INVALID_PARAMETER;
                goto Return;
        }
        ccode = FS_CFS_GetPathBaseEntry(fileOrDirHandle, &pathBase, &nameSpaceDirBase);
        if(ccode)
          goto Return;
        
        
        if(nameSpace == NWNAME_SPACE_UNIX)
        {
                //  There are some incompatibilities between NSS and Legacy with respect to NFS files. These incompatibilities have been
                //  handled in the SMS code as NFS had already data that was populated with information. The firstcreatedflag indicates
                //  that nfs information is set for the file/dir and therefore no defaults need to be set. The firstCreatedFlag value 1 for 
                //  legacy and 255 for NSS. So if we see a 255 we set it to 1 before restroing. If the first created flag is set then for
                //  legacy there are bits that indicate which bits should be set so that the default values are not populated. The 0x4 bit
                //  is set so that the GID is not reset. The 0x40 is set so that the accessmode flags and user ID  is not set with default.
                //  This translation is done so that we are able to restore nss2.0 and nss3.0 backups.
                if(((SIDF_NFS_INFO*)buffer)->firstCreatedFlag==255)
                {
                        ((SIDF_NFS_INFO*)buffer)->myFlagsBit    |= 0x0044;
                        ((SIDF_NFS_INFO*)buffer)->firstCreatedFlag = 1;
                }
                // This check is done for 4.x legacy file system to 6pack migration of data. The first created flag is not
                // used in 4x consistently
                else if((((SIDF_NFS_INFO*)buffer)->firstCreatedFlag==0) && ((((SIDF_NFS_INFO*)buffer)->fileAccessMode)))
                {
                        ((SIDF_NFS_INFO*)buffer)->myFlagsBit    |= 0x0044;
                        ((SIDF_NFS_INFO*)buffer)->firstCreatedFlag = 1;
                }
                
        }
        ccode = GenNSModifySpecificInfo(
                                                                 fileOrDirHandle->cfsStruct.clientConnID,
                                                                 ~(fileOrDirHandle->cfsStruct.clientConnID),
                                                                 fileOrDirHandle->nameSpace,
                                                                 nameSpace,
                                                                 fileOrDirHandle->cfsStruct.volumeNumber,
                                                                 nameSpaceDirBase,                               
                                                                 infoMask,
                                                                 size,
                                                                 buffer
                                                                 );
        if(ccode)
        {
                //FS_CFS_GetFullPathName(fileOrDirHandle, &path);
                FLogError("GenNSModifySpecificInfo", ccode, NULL);
                if(path) tsaFree(path);
                ccode = NWSMTS_SET_FILE_INFO_ERR;
        }
Return:

        FEnd(ccode);
        return ccode;
}


