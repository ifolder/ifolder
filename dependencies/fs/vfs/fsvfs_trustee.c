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
 | $Modtime: 20/06/2004 $
 |
 | $Workfile: fsvfs_trustee.c$
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This file is used to:
 |		Implement certain functions to obtain/modify trustees associated with a file object.
 +-------------------------------------------------------------------------*/

#include <smstypes.h>
#include <fsinterface.h>
#include <smsdebug.h>

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	FILESYS|VFS_SCAN
#define FNAME   "FS_VFS_ScanTrustees"
#define FPTR 	FS_VFS_ScanTrustees
CCODE FS_VFS_ScanTrustees(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_FILE_OR_DIR_INFO *info)
{
	STATUS				status = 0;
	UINT32				trusteeCount = 0;
	FStart();

	if(info->trusteeList != NULL)
	{
		FTrack(TRUSTEES, DC_COMPACT | DC_CRITICAL, "Trustee list is not NULL\n");
		status = FS_INTERNAL_ERROR;
		goto Return;
	}

	/* No trustees for VFS, set the trustee count to 0 for common code to pick up the same and skip trustees */
	info->numberOfTrustees = trusteeCount;

Return:
	FEnd((CCODE)status);   
	return (CCODE)status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	FILESYS|SMS_INTERNAL
#define FNAME   "FS_VFS_AddTrustee"
#define FPTR 	FS_VFS_AddTrustee
CCODE FS_VFS_AddTrustee(FS_FILE_OR_DIR_HANDLE *   fileOrDirHandle,
                            UINT32                  objectID,
                            UINT16                  trusteeRights)
{
	STATUS status = FS_INTERNAL_ERROR;

	/* No trustee restores for VFS */
	return ((CCODE)status);
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	FILESYS|RESTORE|TRUSTEES
#define FNAME   "FS_VFS_DeleteTrustees"
#define FPTR 	FS_VFS_DeleteTrustees
CCODE FS_VFS_DeleteTrustees(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_FILE_OR_DIR_INFO *info)
{
	STATUS status = FS_INTERNAL_ERROR;

	/* No trustee support for VFS */
	return ((CCODE)status);
}
