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

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <zPublics.h>
#include <zParams.h>
#include <zError.h>
#ifdef N_PLAT_NLM
#include <Errors.h>
#include <fsproto.h>
#endif

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
#define FNAME   "FS_Write"
#define FPTR     FS_Write
CCODE FS_Write(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD startingOffset, UINT32 bytesToWrite, void *buffer, UINT32 *bytesWritten, UINT32 handleIndex) //08-18-2002
{
	STATUS status;

	if(fileOrDirHandle == NULL || bytesWritten == NULL || buffer == NULL)	{
		status = FS_PARAMETER_ERROR;
		goto Return;
	}

	status = zWrite(
				fileOrDirHandle->handleArray[handleIndex].handle.nssHandle, //08-18-2002
				zNILXID,
				startingOffset,
				bytesToWrite,
				buffer,
				(NINT *)bytesWritten
			);

	if(bytesToWrite != (*bytesWritten))
	{
		FTrack3(RESTORE, DC_CRITICAL|DC_COMPACT, "startingOffset : %lu bytesToWrite : %lu bytesWritten : %lu\n", 
				startingOffset, bytesToWrite, *bytesWritten);
	}

	if(status)
	{
		FLogError("zWrite", status, NULL);
	}
	
Return :
	if((status == zERR_OUT_OF_SPACE) || (status == zERR_NOT_ENOUGH_DIR_SPACE))
		status=NWSMTS_OUT_OF_DISK_SPACE;
	return status;
}
