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
#ifdef N_PLAT_NLM
#include <mpkapis.h>
#include <nwdebug.h>

/* Legacy headers */
#include <fsproto.h>
#include <config.h>
#include <dstruct.h>
#include <migrate.h>
#endif
#include <string.h>

#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>


/* NSS headers */
#include <zPublics.h>
#include <zParams.h>
#include <zError.h>


#include <fsinterface.h>
#include <smsdebug.h>

/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_Read"
#define FPTR     FS_Read
CCODE FS_Read(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,  FS_READ_HANDLE *fileReadHandle, UINT32 bytesToRead, void *buffer, UINT32 *bytesRead, BOOL IsMigratedFile, UINT32 readIndex)
{
	CCODE		status=0;
	UINT32		_bytesToRead=0;

	FStart();

	_bytesToRead = bytesToRead;
	
	//Work around for ncopy problem
	if(fileOrDirHandle->handleArray[readIndex].type == DATASET_IS_COMPRESSED)
	{
		if ((bytesToRead + (UINT32)fileReadHandle->currentOffset) \
			> fileOrDirHandle->handleArray[readIndex].size)
			_bytesToRead = fileOrDirHandle->handleArray[readIndex].size - fileReadHandle->currentOffset;
	}

	if (IsMigratedFile)
   	{
   		tsaKMutexLock(fileOrDirHandle->migratedFileMutex);
		status = zRead(
						fileReadHandle->handle.nssHandle,
						zNILXID,
						fileReadHandle->currentOffset,
						_bytesToRead,
						buffer,
						(NINT *)bytesRead
					);

		if(status == 0)
		{
			fileReadHandle->currentOffset += *bytesRead;
		}
		else
            FLogError("zRead", status, NULL);		    
		tsaKMutexUnlock(fileOrDirHandle->migratedFileMutex);
	}
	else 
	{
		status = zRead(
						fileReadHandle->handle.nssHandle,
						zNILXID,
						fileReadHandle->currentOffset,
						_bytesToRead,
						buffer,
						(NINT *)bytesRead
					);
		if(status == 0)
		{
			fileReadHandle->currentOffset += *bytesRead;
		}
		else
            FLogError("zRead", status, NULL);		    
	}

	if(!status)
	{
		if((*bytesRead != _bytesToRead) && (fileOrDirHandle->handleArray[readIndex].size != fileReadHandle->currentOffset))
		{
			if(fileOrDirHandle->handleArray[readIndex].type & DATASET_IS_EXTENDED_ATTRIBUTE)
				status = NWSMTS_READ_EA_ERR;
			else
				status = NWSMTS_READ_ERROR;
		}
	}
	
	FEnd(status);
	return (CCODE)status;
}


/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_GetInheritedRightsMask"
#define FPTR     FS_GetInheritedRightsMask
CCODE FS_GetInheritedRightsMask(FS_FILE_OR_DIR_HANDLE *handle, UINT32 *inheritedRightsMask)
{
	STATUS		status;

	status = zGetInheritedRightsMask(handle->handleArray[0].handle.nssHandle, (LONG *)inheritedRightsMask);
	if (status)
	{
		FLogError("zGetInheritedRightsMask", status, NULL);
	}
	
	return (CCODE)status;
} 
