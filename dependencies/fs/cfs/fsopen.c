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
 | $Workfile:   fsOpen.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Initialise TSA
 +-------------------------------------------------------------------------*/

/* Netware headers */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <mpktypes.h>
#include <mpkapis.h>
#include <nwbitops.h>
#include <nwdos.h>
#include <filHandle.h>
#include "cfsdefines.h"
#include <Nwerrno.h>
#include <cdcommon.h>
#include <nit/nwextatt.h>

#include <lfsproto.h>
#include <fsproto.h>

#include <smsdefns.h>
#include <smstypes.h>

#include <fsinterface.h>
#include "smstserr.h"
#include <tsa_320.mlh>
#include "tsa_defs.h"
#include <tempfile.h>
#include <tsalib.h>
#include <incexc.h>
#include <tsaunicode.h>
#include <smsdebug.h>
#include <customdebug.h>
#include "tsaresources.h"

#undef  FTYPE  
#undef  FNAME   
#undef  FPTR    
#define FTYPE	CFS_READ|FILESYS|CRITICAL|COMPACT
#define FNAME	"FS_CFS_ConvertOpenMode"
#define FPTR	FS_CFS_ConvertOpenMode

CCODE FS_CFS_ConvertOpenMode(FS_FILE_OR_DIR_HANDLE *handle,UINT32 inputMode, UINT32 scanMode, BOOL COWEnabled, UINT32 scanType,
			FS_FILE_OR_DIR_INFO *fileInfo, FS_ACCESS_RIGHTS *outputMode, BOOL *noLock,UINT32 *openMode,BOOL FileIsMigrated)
{
	NetwareInfo *temp=NULL;
	UINT32 dataStreamAttributes;
	int count;


	*outputMode = 0x09;
	*noLock = FALSE;
	
	
	temp=(NetwareInfo *)(fileInfo->information);
	
	*outputMode |=  FS_CFS_READ_ACCESS | FS_CFS_NO_UPDATE_LAST_ACCESSED_ON_CLOSE;

	if ((inputMode & NWSM_OPEN_MODE_MASK) == NWSM_OPEN_READ_DENY_WRITE)
	{
		*outputMode |= FS_CFS_DENY_WRITE_ACCESS; 
		noLock = FALSE;
	}
	else if ((inputMode & NWSM_OPEN_MODE_MASK) == NWSM_USE_LOCK_MODE_IF_DW_FAILS)
	{
		*outputMode |= FS_CFS_DENY_WRITE_ACCESS ;
		*noLock = TRUE;
	}
	else if ((inputMode & NWSM_OPEN_MODE_MASK) == NWSM_NO_LOCK_NO_PROTECTION)		
	{
		*outputMode |= FS_CFS_DENY_WRITE_ACCESS ;
		*noLock = TRUE;
	}

	if(!fileInfo->isDirectory)
	{
		for(count=0;count<handle->streamCount;count++)
		{
			if(count==0)
				dataStreamAttributes=temp->Attributes;
			else
				dataStreamAttributes=0;
			if(dataStreamAttributes & FS_CFS_DATA_STREAM_IS_COMPRESSED)
			{
	
		 		if(!(scanType & NWSM_EXPAND_COMPRESSED_DATA)
							&& !(inputMode & NWSM_EXPAND_COMPRESSED_DATA_SET))
				{
		
					*outputMode |= FS_CFS_ENABLE_IO_ON_COMPRESSED_DATA_BIT;
					handle->handleArray[count].type = DATASET_IS_COMPRESSED;
				}
				else
				{
					*outputMode |= FS_CFS_LEAVE_FILE_COMPRESSED_BIT;
			
				}
			}
		}
	}
	
	*openMode = scanMode;
	
	
	if (inputMode & NWSM_NO_DATA_STREAMS)
		*openMode &= (~FS_DATA_STREAM);
	
	if (inputMode & NWSM_NO_EXTENDED_ATTRIBUTES)
		*openMode &= (~FS_EXTENDED_ATTRIBUTES);
	

	return 0;
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|CFS_READ|MINOR|COMPACT
#define FNAME   "FS_CFS_GetCompressedFileSize"
#define FPTR     FS_CFS_GetCompressedFileSize
CCODE FS_CFS_GetCompressedFileSize(FS_FILE_OR_DIR_HANDLE *handle)
{
	CCODE	ccode;
	UINT32 bytesRead = 0, bytesToRead;
	UINT32 sectorsToRead, sectorsRead, _bytesRead, NoWaitReason;
	
	compressFileHeader_t 	*compFileHeaderPtr=NULL;
	compressFileHeader_t 	compFileHeader={0};
	char					*sectorData=NULL;

	FStart();
	
	compFileHeader.CompressedFileLength=0;
	bytesToRead = sizeof(compressFileHeader_t);
	
	if(handle->handleArray[0].type & DATASET_IS_MIGRATED)
	{
		sectorsToRead = (sizeof(compressFileHeader_t) / 512);
		if(sizeof(compressFileHeader_t) % 512)
			sectorsToRead += 1;				
		sectorData = tsaCalloc(1, sizeof(char) *(sectorsToRead*512));
		if(!sectorData)
		{
			ccode=NWSMTS_OUT_OF_MEMORY;
			goto Return;
		}
		
		ccode = PeekDMFileData(
							handle->cfsStruct.clientConnID,
							(UINT32)handle->cfsStruct.volumeNumber,
							handle->cfsStruct.migrated,
							0,
							0,									// NoWaitFlag, blocking
							0,
							sectorsToRead,
							sectorData,
							&sectorsRead,
							&_bytesRead,
							&NoWaitReason);
		
		if(ccode==0)
		{
			compFileHeaderPtr = (compressFileHeader_t *)sectorData;
			handle->handleArray[0].size=compFileHeaderPtr->CompressedFileLength;
		}
		else
		{
			FLogError("PeekDMFileData", ccode, NULL);
			ccode = NWSMTS_READ_ERROR;
		}
		tsaFree(sectorData);
				
	}
	else
	{
		ccode= ReadFile(
					handle->cfsStruct.clientConnID,
					handle->handleArray[0].handle.cfsHandle, 
					(UINT32) 0,
				 	bytesToRead,
				 	&bytesRead,
					(void *)&compFileHeader
					);
	
	
		if(ccode==0)
		{
			compFileHeaderPtr = &compFileHeader;
			handle->handleArray[0].size=compFileHeaderPtr->CompressedFileLength;
		}
		else
		{
			FLogError("ReadFile", ccode, NULL);
			ccode = NWSMTS_READ_ERROR;
		}
	}
Return:
	FEnd(ccode);		
	return(ccode);
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|OPEN|CRITICAL|VERBOSE
#define FNAME   "FS_CFS_Open"
#define FPTR     FS_CFS_Open
CCODE FS_CFS_Open(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_ACCESS_RIGHTS accessRights, 
			UINT32 openModeFlags, FS_FILE_OR_DIR_INFO *info, BOOL noLock, UINT32 searchAttributes, FS_HL_TABLE ** hlTable)
{
	CCODE  		 	ccode=0;
	UINT32			tempHandle=NULL;
	UINT32			count=0;
	UINT32			pathBase=0;
	UINT32			secStreamSize = 0;
    UINT32			retDirectoryNumber=0;  // ret val not used
	void         	*retDirectoryEntry=NULL;// ret val not used
   	FS_HANDLE		eaHandle;
   	UINT32 			totalSize,totalEAs = 0,currentEAs,totalKeySize;
   	UINT32			maxdatasize=	NWMAX_ENUM_EA_SIZE;
   	UINT16      	nextSequence=0;
   	UINT32			enumBufSize;
   	UINT8       	*enumBuf;
 	UINT8       	*tenumBuf;
 	UINT32 			attributes = 0;
 	char 			*path=NULL;
 	

 	FStart();

    if((fileOrDirHandle == NULL) ||(info == NULL) || !(openModeFlags & FS_PRIMARY_DATA_STREAM))
	{
		ccode = NWSMTS_INVALID_PARAMETER;
		goto Return;
	}
    
	ccode = FS_CFS_GetPathBaseEntry(fileOrDirHandle,  &pathBase, NULL);
	if(ccode)
   	  goto Return;
	
	FTrack(OPEN, DC_COMPACT, "FS_CFS_GetPathBaseEntry\n");

	
	attributes = FS_CFS_GetFileAttributes(info->information);
	
	if(attributes & FS_CFS_REMOTE_DATA_ACCESS)
	{
		fileOrDirHandle->cfsStruct.migrated = pathBase;
		if(!info->isDirectory)
		{
			ccode = FS_CFS_GetStreamSizes(fileOrDirHandle, info); //non-primary size
			if(ccode) 
				goto Return;
			//Mutex name should be changed to dset level instead of stream level
			fileOrDirHandle->migratedFileMutex = tsaKMutexAlloc("TSA_STREAM_ACCESS_LOCK");
			if(!fileOrDirHandle->migratedFileMutex)
			{
				FLogError("tsaKMutexAlloc", 0, 0);
				ccode = NWSMTS_INTERNAL_ERROR;
			}
			if((fileOrDirHandle->handleArray[0].type & DATASET_IS_COMPRESSED))
			{
				fileOrDirHandle->handleArray[0].UnCompressedSize= FS_CFS_GetFileStreamSize(info->information);
				/*size is populated in FS_CFS_GetCompressedFileSize */
			}
			else
			{
				fileOrDirHandle->handleArray[0].size = FS_CFS_GetFileStreamSize(info->information);
				/*UnCompressedSize is not required since the file is not compressed*/
			}
		}
		else
		{
			fileOrDirHandle->streamCount=1;
		}
		//Migration stream handle is not required and Migration EA is not supported
		goto Return;
	}
	
    if((openModeFlags & FS_PRIMARY_DATA_STREAM) && (!info->isDirectory))
    {
		ccode = OpenFile(
					fileOrDirHandle->cfsStruct.clientConnID,// this needs to replaced by clientConnID
					~(fileOrDirHandle->cfsStruct.clientConnID),
					fileOrDirHandle->cfsStruct.volumeNumber,
					pathBase,
					NULL,		   		 //pathstring 
					0,			   		 //pathcount
					fileOrDirHandle->nameSpace,
					searchAttributes,    
					accessRights,  
					(BYTE)count,		
					&tempHandle,
					&retDirectoryNumber,  // ??? this is not used 
					&retDirectoryEntry     // ??? this is not used 
	       		);
		
		if(!ccode)
	    {
	    	fileOrDirHandle->handleArray[0].handle.cfsHandle=tempHandle;
	    	if(fileOrDirHandle->handleArray[0].type & DATASET_IS_COMPRESSED)
	    		fileOrDirHandle->handleArray[0].UnCompressedSize = FS_CFS_GetFileStreamSize(info->information);
	    	else
	    		fileOrDirHandle->handleArray[0].size = FS_CFS_GetFileStreamSize(info->information);
	    	FTrack4(OPEN, DC_COMPACT, "CFS-Primary Stream open\n", fileOrDirHandle->cfsStruct.volumeNumber, pathBase, fileOrDirHandle->nameSpace, accessRights);
		}
	    else
	    {
	    
	    	//FS_CFS_GetFullPathName(fileOrDirHandle, &path);
	    	FLogError("OpenFile", ccode, NULL);
			if(path) {tsaFree(path); path=NULL;}
			
			if(ccode==ERR_LOCK_FAIL || ccode == ERR_ALREADY_IN_USE)
				ccode =NWSMTS_DATA_SET_IN_USE;
			else
				ccode = NWSMTS_OPEN_DATA_STREAM_ERR; 
			
	    	goto Return;
		}
	}

	if ((!fileOrDirHandle->isDirectory) && (openModeFlags & FS_DATA_STREAM))
    {
		if (tempHandle == NULL)
		{
			/**	Opening a data stream or EA requires the handle to the file's primary data stream **/
			ccode = NWSMTS_OPEN_DATA_STREAM_ERR;
			goto Return;	
		}
		
		for(count=1; count<(fileOrDirHandle->streamCount);count++)
		{
			ccode = OpenFile(
					 	 fileOrDirHandle->cfsStruct.clientConnID,// this needs to replaced by clientConnID
						 ~(fileOrDirHandle->cfsStruct.clientConnID),
						 fileOrDirHandle->cfsStruct.volumeNumber,
						 pathBase,
						 NULL,		   		 //pathstring 
						 0,			   		 //pathcount
						 fileOrDirHandle->nameSpace,
						 searchAttributes,    
						 accessRights & ~(FS_CFS_ENABLE_IO_ON_COMPRESSED_DATA_BIT),  
						 (BYTE)count,		
						 &tempHandle,
						 &retDirectoryNumber,  // ??? this is not used 
						 &retDirectoryEntry     // ??? this is not used 
					);
			if(!ccode)
			{
				fileOrDirHandle->handleArray[count].handle.cfsHandle=tempHandle;
				ccode=GetFileSize(fileOrDirHandle->cfsStruct.clientConnID, tempHandle, &secStreamSize);
				fileOrDirHandle->handleArray[count].size=secStreamSize;
				/* CFS does not name secondary streams (VERIFY) */
				fileOrDirHandle->handleArray[count].streamName[0] = '\0';
				FTrack4(OPEN, DC_COMPACT, "CFS -Non-Primary Stream open\n", fileOrDirHandle->cfsStruct.volumeNumber, pathBase, fileOrDirHandle->nameSpace, accessRights);
			}
			else
			{
				//FS_CFS_GetFullPathName(fileOrDirHandle, &path);
		    	FLogError("OpenFile", ccode, NULL);
		    	if(path) {tsaFree(path); path=NULL;}
    			ccode = NWSMTS_OPEN_DATA_STREAM_ERR; 
				goto Return;
			}
		}
    }
	//EA
	enumBuf = (UINT8 *)tsaMalloc(NWDEFAULT_ENUM_EA_SIZE);
	if(!enumBuf)
	{
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}

	if ((!fileOrDirHandle->isDirectory) && (openModeFlags & FS_EXTENDED_ATTRIBUTES))
	{
		if (fileOrDirHandle->eaCount)
		{
			if (tempHandle == NULL)
			{
				/**	Opening a data stream or EA requires the handle to the file's primary data stream **/
				ccode = NWSMTS_OPEN_DATA_STREAM_ERR;
				goto Return;	
			}
				
			ccode=OpenEAHandle(
							  fileOrDirHandle->cfsStruct.clientConnID,
							  ~(fileOrDirHandle->cfsStruct.clientConnID),
					    	  0L, //taken from tsa600
					    	  1, //type flag bit is 1 for EAOpenFileBit
				    		  fileOrDirHandle->handleArray[0].handle.cfsHandle,//this is the open file handle for type flag 1
							  0, //this is ignored if type flag is 1
							  &eaHandle.cfsHandle
						      );
	        if(ccode)
	            FLogError("OpenEAHandle", ccode, NULL);
	        
			if(!ccode)
			{
				//FTrack(CFS_OPEN, DC_COMPACT, "CFS-Extented Attribute Open\n");
				ccode=EnumEA(
							 fileOrDirHandle->cfsStruct.clientConnID,
							 ~(fileOrDirHandle->cfsStruct.clientConnID),
		           			 eaHandle.cfsHandle,
			           		 0x01,
		    	       		 nextSequence,
		        	   		 maxdatasize,
		           			 0,
		           			 NULL,
			           		 enumBuf,
		    	       		 &enumBufSize,
		        	   		 &nextSequence,
		           			 &totalSize,
			           		 &totalEAs,
		    	       		 (WORD *)&currentEAs, 
		        	   		 &totalKeySize,
		           			 maxdatasize
		           			 );

				if(ccode)
				{
					//FS_CFS_GetFullPathName(fileOrDirHandle, &path);
					FLogError("EnumEA", ccode, NULL);
					if(path) {tsaFree(path); path=NULL;}
					ccode = NWSMTS_OPEN_ERROR;
		   	  		goto Return;
				}
				FTrack(OPEN, DC_COMPACT, "CFS-Extented Attribute Enum\n");
			}
			else
			{
				//FS_CFS_GetFullPathName(fileOrDirHandle, &path);
				FLogError("OpenEAHandle", ccode, NULL);
				if(path) {tsaFree(path); path=NULL;}
				ccode = NWSMTS_OPEN_ERROR;
				goto Return;
			}
			if(fileOrDirHandle->eaCount != totalEAs)
			{
				FTrack2(OPEN, DC_COMPACT, "eaCount not euqal to totalEAs %d %d\n", fileOrDirHandle->eaCount, totalEAs);
				ccode = NWSMTS_OPEN_ERROR;
				goto Return;
			}
				
				
			if(totalEAs>0)
			{
				tenumBuf = enumBuf;
				for(count=0; count<(totalEAs);count++)
				{
				fileOrDirHandle->handleArray[count+fileOrDirHandle->streamCount].handle.cfsHandle = eaHandle.cfsHandle;
					strncpy(fileOrDirHandle->handleArray[count + fileOrDirHandle->streamCount].eaName,
							((T_enumerateEAnoKey *)tenumBuf)->keyValue,
							((T_enumerateEAnoKey *)tenumBuf)->keyLength);
					fileOrDirHandle->handleArray[count + fileOrDirHandle->streamCount].eaName[((T_enumerateEAnoKey *)tenumBuf)->keyLength] = '\0';
					fileOrDirHandle->handleArray[count + fileOrDirHandle->streamCount].size= ((T_enumerateEAnoKey *)tenumBuf)->valueLength;
					fileOrDirHandle->handleArray[count + fileOrDirHandle->streamCount].eaAttrFlags= ((T_enumerateEAnoKey *)tenumBuf)->accessFlags;
					if (!((count + 1) == totalEAs))
						tenumBuf = tenumBuf + sizeof(T_enumerateEAnoKey) + ((T_enumerateEAnoKey *)tenumBuf)->keyLength - 1;
				}
				fileOrDirHandle->eaMutex = tsaKMutexAlloc("TSA_EA_ACCESS_LOCK");
				if (fileOrDirHandle->eaMutex == NULL)
				{
					FLogError("tsaKMutexAlloc", ccode, NULL);
					if(path) {tsaFree(path); path=NULL;}
					ccode = NWSMTS_INTERNAL_ERROR;
					goto Return;
				}
			}
		}
	}

	if(enumBuf)
		tsaFree(enumBuf);	
  
Return:
	
   FEnd(ccode);
   return ccode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|CFS_READ
#define FNAME   "FS_CFS_Close"
#define FPTR     FS_CFS_Close

CCODE FS_CFS_Close(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32	closeFlag)
{
	CCODE		ccode=0;
	UINT32		*closeKey=0;
	UINT32		count;
	
	FStart();

	//Data stream's close
	for(count = 0; count < (fileOrDirHandle->streamCount); count++)
	{
		if(fileOrDirHandle->isDirectory)
			continue;
		
		if(closeFlag)
		{
			if((closeFlag & DATASET_CLOSE_ALL_EA_STREAM_HANDLES) && !(closeFlag & DATASET_CLOSE_ALL_NON_PRI_STREAM_HANDLES))
				continue;
			//the only choice is non-primary stream, if not continue			
			if(!(fileOrDirHandle->handleArray[count].type & DATASET_IS_SECONDARY_DATASTREAM)) 
				continue;	
		}
		
		if (fileOrDirHandle->handleArray[count].streamMap)	{
			tsaFree(fileOrDirHandle->handleArray[count].streamMap);
			fileOrDirHandle->handleArray[count].streamMap = NULL;
		}
		if (fileOrDirHandle->handleArray[count].type & DATASET_IS_MIGRATED)
		    continue;

		closeKey=&(fileOrDirHandle->handleArray[count].handle.cfsHandle);
		if(*closeKey)
	   	{
			ccode=CloseFile(fileOrDirHandle->cfsStruct.clientConnID, ~(fileOrDirHandle->cfsStruct.clientConnID), *closeKey);
			if(ccode)
			{
				FLogError("CloseFile", ccode, NULL);
				ccode = NWSMTS_INVALID_DATA_SET_HANDLE;
		   		goto Return;
			}
			else
		   		*closeKey = 0;
		}	
		else
		{
			if(fileOrDirHandle->migrationKey)
			{
				//might be only backed up migration key got restored so no file handle 
				goto Return;
			}
			ccode= NWSMTS_INVALID_DATA_SET_HANDLE;
		   	goto Return;
		}	 	
	}   

	//EA's close. There is only one open handle per EA in legacy, so close just the first one.
	if (fileOrDirHandle->eaCount && (!closeFlag || closeFlag & DATASET_CLOSE_ALL_EA_STREAM_HANDLES))
	{
		if (fileOrDirHandle->eaMutex)
		{
			tsaKMutexFree(fileOrDirHandle->eaMutex);
			fileOrDirHandle->eaMutex = NULL;
		}
		
		closeKey = &(fileOrDirHandle->handleArray[count].handle.cfsHandle);
		if(*closeKey)
		{
			ccode= CloseEAHandle(fileOrDirHandle->cfsStruct.clientConnID, ~(fileOrDirHandle->cfsStruct.clientConnID), *closeKey);
			if(ccode)
			{
				FLogError("CloseEAHandle", ccode, NULL);
				ccode = NWSMTS_INVALID_DATA_SET_HANDLE;
				goto Return;	
			}
			else
				*closeKey = 0;
		}	
		else
		{
			ccode= NWSMTS_INVALID_DATA_SET_HANDLE;
		   	goto Return;
		}
	}

Return:
	FEnd(ccode);
	return ccode;
}

