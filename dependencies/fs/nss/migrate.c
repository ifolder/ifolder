/****************************************************************************
 |
 |  (C) Copyright 2002 Novell, Inc.
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
 | $Modtime:   24/02/2002 $
 |
 | $Workfile:   migrate.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		- TBDTBD
 ****************************************************************************/

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <smsdefns.h>
#include <smstypes.h>

#include <fsinterface.h>
#include <incexc.h>
#include <smstserr.h>
#include <tsalib.h>
#include <tsaunicode.h>
#include <smsdebug.h>
#include <smsdebug.h>
#include <migrate.h>
#include <nwlocale.h>
#include <malloc.h>



#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NO_DEBUG_INFO
#define FNAME   "FS_CreateMigratedDirectory"
#define FPTR     FS_CreateMigratedDirectory
CCODE FS_CreateMigratedDirectory(
   FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,
   UINT32			smid,
	UINT32			bindFlag,
	UINT32			*bindKeyNumber)
{
	return NWERR_UNSUPPORTED;
}
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NO_DEBUG_INFO
#define FNAME   "FS_VerifyMigrationKey"
#define FPTR     FS_VerifyMigrationKey
CCODE FS_VerifyMigrationKey(DMKEY *key)
{
	return NWERR_UNSUPPORTED;
}


