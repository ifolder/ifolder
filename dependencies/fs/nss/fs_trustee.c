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
 | $Modtime: $
 |
 | $Workfile: $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This file is used to:
 |		Implement certain functions to obtain/modify trustees associated with a file object.
 +-------------------------------------------------------------------------*/

#include <stdio.h>
#include <stdlib.h>
#ifdef N_PLAT_NLM
#include <nwstring.h>
#include <filHandle.h>
#endif

#include <zPublics.h>
#include <zParams.h>
#include <zError.h>

#include <smstypes.h>
#include <fsinterface.h>

#include <convids.h>

#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>
#include <smsutapi.h>

#include <tsalib.h>
#include <tsaunicode.h>
#include <smsdebug.h>

/**	TBDTBD: Use NDS APIs to convert the Trustees from ID to names, see if there are
any new NDS apis that do this conversion taking 64-bit Guids as argument **/


/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|NSS_SCAN
#define FNAME   "FS_ScanTrustees"
#define FPTR     FS_ScanTrustees
CCODE FS_ScanTrustees(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_FILE_OR_DIR_INFO *info)
{
	STATUS				status = 0;
	INT32				index = 0;
	FS_TRUSTEE_ID		*trusteeListIterator = NULL,*tail = NULL;
	GUID_t				trusteeID;
	UINT32				trusteeRights;
	UINT32				trusteeCount = 0;
	FStart();

	if(info->trusteeList != NULL)
	{
		FTrack(TRUSTEES, DC_COMPACT | DC_CRITICAL, "Trustee list is not NULL\n");
		status = FS_INTERNAL_ERROR;
		goto Return;
	}

	while(TRUE)
	{
		status = zGetTrustee(fileOrDirHandle->handleArray[0].handle.nssHandle, index, &trusteeID, (NINT *)&trusteeRights, (NINT *)&index);
		if(status != 0 || index == -1)
		{
			if (status)
			{
				FLogError("zGetTrustee", status, NULL);
			}
			break;
		}
		trusteeCount++;

		trusteeListIterator = (FS_TRUSTEE_ID *)tsaMalloc(sizeof(FS_TRUSTEE_ID));
		if(trusteeListIterator == NULL)
		{
			status = NWSMTS_OUT_OF_MEMORY;
			goto Return;
		}

		trusteeListIterator->id.gid = trusteeID;
		trusteeListIterator->rights = trusteeRights;
		trusteeListIterator->next = NULL;
		if(tail)
			tail->next = trusteeListIterator;
		else
			info->trusteeList = trusteeListIterator;	
		tail = trusteeListIterator;
	}
	info->numberOfTrustees = trusteeCount;

Return:
	FEnd((CCODE)status);   
	return (CCODE)status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|SMS_INTERNAL
#define FNAME   "FS_AddTrustee"
#define FPTR     FS_AddTrustee

CCODE FS_AddTrustee(FS_FILE_OR_DIR_HANDLE *   fileOrDirHandle,
                            UINT32                  objectID,
                            UINT16                  trusteeRights)
{
	STATUS status = 0;
	NINT rights;
	UINT32 objectID_32;
	NDSid_t objectID_64;
	objectID_32 = LongSwap(objectID);

	status = xIdToGuid(objectID_32, &objectID_64);
    if (status)
    {
        FLogError("xIdToGuid", status, NULL);
    }

	if(!status)
	{
		rights = (NINT)trusteeRights;
		status = zAddTrustee(
				fileOrDirHandle->handleArray[0].handle.nssHandle,
    		    zNILXID, &objectID_64, rights);
		if (status)
		{
		    FLogError("zAddTrustee", status, NULL);
		}
	}
	return ((CCODE)status);
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|RESTORE|TRUSTEES
#define FNAME   "FS_DeleteTrustees"
#define FPTR     FS_DeleteTrustees

CCODE FS_DeleteTrustees(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_FILE_OR_DIR_INFO *info)
{
	STATUS status = 0;
    FS_TRUSTEE_ID		*trusteeParser;
	STATUS saveError=0;


	FStart();

    if(info)
        trusteeParser = info->trusteeList ;
    else
    {
        {status = NWSMTS_INTERNAL_ERROR; FTrack(INTERNAL_ERROR,DC_CRITICAL|DC_COMPACT, "Internal Error\n"); }
        FTrack(RESTORE|TRUSTEES, CRITICAL|COMPACT , "info is NULL\n") ;
        goto Return ;
    }

	while(!status && trusteeParser)
	{
        status = zDeleteTrustee(fileOrDirHandle->handleArray[0].handle.nssHandle, zNILXID,
                                    &trusteeParser->id.gid) ;
        if(status)
        {
            	FLogError("zDeleteTrustee", status, 0);
		saveError=status;
		status=0;
        }
        trusteeParser = trusteeParser->next;
	}
	status=saveError;

Return :
	FEnd((CCODE)status);
	return ((CCODE)status);
}


