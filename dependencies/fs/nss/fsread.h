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
 | $Author$ Vijai babu M(mvijai@novell.com)
 | $Modtime:     $ 06 Dec 01
 |
 | $Workfile:     $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		implement the OpenDataSetForBackup function
 ****************************************************************************/

#ifndef FSREAD_H_			/* FSRead.h header latch */
#define FSREAD_H_

#include <smsdefns.h>
#include <smstypes.h>

#include "fsinterface.h"



 /***************************************************************************/

CCODE FS_Read(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,  FS_READ_HANDLE *fileReadHandle, UINT32 bytesToRead, void *buffer, UINT32 *bytesRead, BOOL IsMigratedFile, UINT32 readIndex);



CCODE FS_GetInheritedRightsMask(FS_FILE_OR_DIR_HANDLE *handle, UINT32 *inheritedRightsMask);

 /***************************************************************************/
#endif                                    /* FSRead.h  header latch */



