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
 | $Modtime:   22-Jun-2004 		$
 |
 | $Workfile:   fsvfsread.c  		$
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This file is used to:
 |		Implements FS_VFS_Read that reqires an open file and reads into supplied buffer. 
 | 		Also function FS_VFS_GetInheritedRightsMask for reading Inherited Rights Mask which is  a dummy on Linux.
 +-------------------------------------------------------------------------*/

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
extern int errno;

/* SMS headers */
#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>


/* NSS headers */
#include <zPublics.h>
#include <zParams.h>
#include <zError.h>

/* TSA headers */
#include <fsinterface.h>
#include <smsdebug.h>

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	FILESYS | VFS_READ
#define FNAME   "FS_VFS_Read"
#define FPTR 	FS_VFS_Read
CCODE FS_VFS_Read(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,  FS_READ_HANDLE *fileReadHandle, UINT32 bytesToRead, void *buffer, UINT32 *bytesRead, BOOL IsMigratedFile, UINT32 readIndex)
{
	CCODE		status=0;
	UINT32		_bytesToRead=0;
	FStart();

	_bytesToRead = bytesToRead;
	
	/* We must read from fileReadHandle->currentOffset _bytesToRead many bytes. But, read on VFS is not offset based.
	 * Need to try fread to see if it can help with performance */
	*bytesRead = read(fileReadHandle->handle.vfsHandle.fileDescriptor, buffer, _bytesToRead);
	if (*bytesRead == -1)
	{
		status = -1;
		*bytesRead = 0;
	}
	else
		status = 0;

	if(status == 0)
	{
		fileReadHandle->currentOffset += *bytesRead;
	}
	else
	    FLogError("read", errno, NULL);		    

	if(!status)
	{
		if((*bytesRead != _bytesToRead) && (fileOrDirHandle->handleArray[readIndex].size != fileReadHandle->currentOffset))
		{
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
#define FTYPE 	FILESYS | VFS_READ
#define FNAME   "FS_VFS_GetInheritedRightsMask"
#define FPTR 	FS_VFS_GetInheritedRightsMask
CCODE FS_VFS_GetInheritedRightsMask(FS_FILE_OR_DIR_HANDLE *handle, UINT32 *inheritedRightsMask)
{
	/* Function here for FS pointer table completion. */
	return 0;
}
