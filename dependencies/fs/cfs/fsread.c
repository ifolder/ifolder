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
 | $Modtime:   27 Mar 2002 17:05:00  $
 |
 | $Workfile:   fsRead.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This file is used to:
 |		Implements FS_Read that reqires an open file, EA or data stream handle, and reads into supplied buffer. 
 | 		Also function FS_GetInheritedRightsMask for reading Inherited Rights Mask.
 +-------------------------------------------------------------------------*/

#include <stdio.h>
#include <stdlib.h>
#include <mpkapis.h>
#include <nwdebug.h>

/* Legacy headers */
#include <fsproto.h>
#include <config.h>
#include <dstruct.h>
#include <migrate.h>

#include <string.h>

#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>


#include <fsinterface.h>
#include <smsdebug.h>

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    BACKUP |FILESYS
#define FNAME   "FS_CFS_Read"
#define FPTR     FS_CFS_Read
CCODE FS_CFS_Read(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_READ_HANDLE *fileReadHandle, UINT32 bytesToRead, void *buffer, UINT32 *bytesRead,UINT32 readIndex)
{
    CCODE 			ccode=0;
    UINT32			accessFlag,totalEalen = 0;
   	UINT32			expectedOffset = 0;
   	UINT32			expectedIndex = 0;
	BOOL			isLocked = 0;
	UINT32 			sectorsToRead = 0;
	UINT32			currentSector = 0;
	UINT32			sectorsRead = 0;
	UINT32			NoWaitReason;
	char			*path=NULL;
	UINT32			_bytesToRead;
	FStart();
	
	if(!fileOrDirHandle || !fileReadHandle || !buffer)
	{
		ccode=NWSMTS_INVALID_PARAMETER;
		goto Return;
	}		

	//Data stream read
	if((fileReadHandle->readFlag & FS_PRIMARY_DATA_STREAM) || (fileReadHandle->readFlag & FS_DATA_STREAM))
   	{
   		if (fileOrDirHandle->handleArray[readIndex].type & DATASET_IS_MIGRATED)
   		{
   			
   			tsaKMutexLock(fileOrDirHandle->migratedFileMutex);
   			sectorsToRead = bytesToRead/512 + (bytesToRead%512 ? 1:0);
   			currentSector  = fileReadHandle->currentOffset/512;
   			ccode = PeekDMFileData(
						fileOrDirHandle->cfsStruct.clientConnID,
						(UINT32)fileOrDirHandle->cfsStruct.volumeNumber,
						fileOrDirHandle->cfsStruct.migrated,
						readIndex,
						0,									
						currentSector,
						sectorsToRead,
						buffer,
						&sectorsRead,
						bytesRead,
						&NoWaitReason);

   			tsaKMutexUnlock(fileOrDirHandle->migratedFileMutex);

   			if(ccode)
   			{
   				//FS_CFS_GetFullPathName(fileOrDirHandle, &path);
				FLogError("PeekDMFileData", ccode, NULL);
				if(path) {tsaFree(path); path=NULL;}
   				ccode = NWSMTS_READ_ERROR;
				goto Return;
   			}
   			else
			{
				fileReadHandle->currentOffset += (QUAD)*bytesRead;
				if((sectorsRead != sectorsToRead) && (fileOrDirHandle->handleArray[readIndex].size != fileReadHandle->currentOffset))
					ccode = NWSMTS_READ_ERROR;
   			}
  		}
   		else
   		{
			//Work around for traditional file system problem
			_bytesToRead = bytesToRead;
			if(fileOrDirHandle->handleArray[readIndex].type == DATASET_IS_COMPRESSED)
			{
				if ((bytesToRead + (UINT32)fileReadHandle->currentOffset) \
					> fileOrDirHandle->handleArray[readIndex].size)
					_bytesToRead = fileOrDirHandle->handleArray[readIndex].size - fileReadHandle->currentOffset;
			}
   			
	   		ccode= ReadFile(
					fileOrDirHandle->cfsStruct.clientConnID,//clientConnID,
					fileReadHandle->handle.cfsHandle,
					(UINT32)fileReadHandle->currentOffset,
					_bytesToRead,
					bytesRead,
					buffer
					);
	   		
	   		if(ccode)
	   		{
	   			//FS_CFS_GetFullPathName(fileOrDirHandle, &path);
	   			FLogError("ReadFile", ccode, NULL);
	   			if(path) {tsaFree(path); path=NULL;}
				ccode = NWSMTS_READ_ERROR;
				goto Return;
	   		}
			else
			{
				fileReadHandle->currentOffset += (QUAD)*bytesRead;
				if((*bytesRead != _bytesToRead) && (fileOrDirHandle->handleArray[readIndex].size != fileReadHandle->currentOffset))
					ccode = NWSMTS_READ_ERROR;
   			}
   		}
	}
	
	if (fileReadHandle->readFlag & FS_EXTENDED_ATTRIBUTES)
	{
	    if (!((fileOrDirHandle->streamCount == readIndex) && (fileReadHandle->currentOffset == 0)))
  		{
  			if (fileReadHandle->currentOffset == 0)
			{
				expectedIndex = readIndex - 1;
				expectedOffset = fileOrDirHandle->handleArray[expectedIndex].size;
			}
			else
			{
				expectedIndex = readIndex;
				expectedOffset = fileReadHandle->currentOffset; 
			}

			while (TRUE)
			{
				tsaKMutexLock(fileOrDirHandle->eaMutex);
				isLocked = 1;

				if ((fileOrDirHandle->lastReadIndex == expectedIndex) && \
						(fileOrDirHandle->lastOffset == expectedOffset))
					break;

				tsaKMutexUnlock(fileOrDirHandle->eaMutex);
				isLocked = 0;
				kYieldThread();
			}
  		}

  		if (fileReadHandle->currentOffset == 0)
  		{
	  		ccode = FS_CFS_ReadEAData(fileOrDirHandle->cfsStruct.clientConnID, ~(fileOrDirHandle->cfsStruct.clientConnID), \
		    		fileReadHandle->handle.cfsHandle, 0, 0xFFFFFFFF, \
		    		strlen(fileOrDirHandle->handleArray[readIndex].eaName), fileOrDirHandle->handleArray[readIndex].eaName, \
				  	buffer, (LONG *)&totalEalen, bytesRead, &accessFlag, bytesToRead);
		}
  		else
  		{
  			
  			ccode = FS_CFS_ReadEAData(fileOrDirHandle->cfsStruct.clientConnID, ~(fileOrDirHandle->cfsStruct.clientConnID), \
		    		fileReadHandle->handle.cfsHandle, (UINT32)fileReadHandle->currentOffset, 0, 0, NULL, \
				  	buffer, (LONG *)&totalEalen, bytesRead, &accessFlag, bytesToRead);
	  	}

		fileOrDirHandle->lastReadIndex = readIndex;
		if (!ccode && !((*bytesRead != bytesToRead) && 
			(fileOrDirHandle->handleArray[readIndex].size != (fileReadHandle->currentOffset + (QUAD)(*bytesRead)))))
			fileOrDirHandle->lastOffset = (UINT32)(fileReadHandle->currentOffset) + *bytesRead;
		/* In case of errors the EA index needs to be moved appropriatly so that any other thread waiting,
		 * on this EA read completion picks up the state and continues. */
		else
		{
			/* As bytesRead is not updated due to errors, update the lastOffset to the size or the next offset */
			if (((UINT32)(fileReadHandle->currentOffset) + bytesToRead) > 
				fileOrDirHandle->handleArray[readIndex].size)
			{
				fileOrDirHandle->lastOffset = fileOrDirHandle->handleArray[readIndex].size;
			}
			else
				fileOrDirHandle->lastOffset = (UINT32)(fileReadHandle->currentOffset) + bytesToRead;
		}

		if (isLocked)
			tsaKMutexUnlock(fileOrDirHandle->eaMutex);

		if(!ccode)
		{
			fileReadHandle->currentOffset += (QUAD)*bytesRead;
			if((*bytesRead != bytesToRead) && (fileOrDirHandle->handleArray[readIndex].size != fileReadHandle->currentOffset))
				ccode = NWSMTS_READ_EA_ERR;
   		}
		else
		{
			FLogError("ReadEAData", ccode, NULL);
			if(path) {tsaFree(path); path=NULL;}
			ccode = NWSMTS_READ_EA_ERR;
		}
  	}
    
Return:
	FEnd(ccode);
	return ccode;
}

/* Traditional EAs need to be read in less than WORD sizes (or 0xFFFF sizes).
 * The following function takes a read call to an EA and splits it into appropriate WORD chunks,
 * also keeping in mind that the offsets for reads are to be at 128 byte boundaries.
 * This way a large read call to an EA for Legacy is broken down into smaller chunks. */
FS_CFS_ReadEAData(		LONG Station, LONG Task, LONG eaHandle, LONG StartPosition, LONG InspectSize,
		LONG KeySize, BYTE *Key, BYTE *OutBuf, LONG *TotalEALen, LONG *CurrentLen, LONG *AccessFlag,
		LONG MaximumDataSize)
{
	WORD		maxLen = 0xFFFF - (0xFFFF % 128);
	WORD		outLen = maxLen;
	LONG		inLen;
	CCODE		cCode = 0;

	/* If the requested size is greater than the maximum read size that can be requested split it */
	if (MaximumDataSize > maxLen)
	{
		/* Do the first read with the max size */
		cCode = ReadEAData(	Station, Task, eaHandle, StartPosition, InspectSize,
					KeySize, Key, OutBuf, TotalEALen, &outLen, AccessFlag,
					maxLen);
		if (cCode)
			goto Return;

		/* Update the state and output buffer positions */
		MaximumDataSize -= outLen;
		StartPosition += outLen;
		OutBuf += outLen;
		*CurrentLen += outLen;

		/* If the EA has lesser than the requested size in it we are done */
		if (outLen < maxLen)
			goto Return;
		
		while(MaximumDataSize)
		{
			inLen = _min(maxLen, MaximumDataSize);

			/* This is the second read at least for this EA, update variables accordingly and request a continuation of read */
			cCode = ReadEAData(	Station, Task, eaHandle, StartPosition, 0,
						0, NULL, OutBuf, TotalEALen, &outLen, AccessFlag,
						inLen);
			if (cCode)
				goto Return;

			/* Update the state and output buffer positions */
			MaximumDataSize -= outLen;
			StartPosition += outLen;
			OutBuf += outLen;
			*CurrentLen += outLen;

			/* If the EA has lesser than the requested size in it we are done */
			if (outLen < inLen)
				goto Return;
		}
	}
	else
	{
		/* Requested size is less than WORD length, call the Legacy API and return */
		return ReadEAData(	Station, Task, eaHandle, StartPosition, InspectSize,
				KeySize, Key, OutBuf, TotalEALen, (WORD *)CurrentLen, AccessFlag,
				MaximumDataSize);
	}

Return:
	return cCode;
}	
