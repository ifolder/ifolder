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
 | $Modtime:   29 Jun 2004 21:15:16  $
 |
 | $Workfile:   fsvfswrite.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Write file data to FS
 +-------------------------------------------------------------------------*/

#include <errno.h>

#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>

#include "fsinterface.h"
#include <smsdebug.h>

/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_VFS_Write"
#define FPTR     FS_VFS_Write
CCODE FS_VFS_Write(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD startingOffset, UINT32 bytesToWrite, void *buffer, UINT32 *bytesWritten, UINT32 handleIndex) //08-18-2002
{
	STATUS status = 0;

	if(fileOrDirHandle == NULL || bytesWritten == NULL || buffer == NULL)	{
		status = FS_PARAMETER_ERROR;
		goto Return;
	}

	if(fileOrDirHandle->fileType == FS_VFS_HARD_LINK)
	{
		*bytesWritten = bytesToWrite; /*skip writes if it is a hardlink*/
		goto Return;
	}

	status = lseek(fileOrDirHandle->handleArray[handleIndex].handle.vfsHandle.fileDescriptor, 0, SEEK_SET);

	if(status != -1)
	{
		status = write(
					fileOrDirHandle->handleArray[handleIndex].handle.vfsHandle.fileDescriptor,
					buffer,
					bytesToWrite);

		if(status != -1)
		{
			*bytesWritten = status;
			status = 0;
		}
		else
			status = errno;

		if(bytesToWrite != (*bytesWritten))
		{
			FTrack2(RESTORE, DC_CRITICAL|DC_COMPACT, "bytesToWrite : %lu bytesWritten : %lu\n", 
					bytesToWrite, *bytesWritten);
			status = NWSMTS_INTERNAL_ERROR;
		}
	}

	if(status)
	{
		FLogError("write", errno, NULL);
	}
	
Return :
	return status;
}
