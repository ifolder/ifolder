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
 | $Modtime: 27 Aug. 2002 $
 |
 | $Workfile: filHandle.c $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This file is used to:
 |		Implement the FillHandle function used for legacy calls.
 +-------------------------------------------------------------------------*/

#ifndef _FILHANDLE_H_
#define _FILHANDLE_H_

#include <portable.h>

#include <nwsms.h>
#include <fsinterface.h>

/* Required defines */
#define SHORT_DIRECTORY_HANDLE_FLAG   0x00
#define DIRECTORY_BASE_FLAG             0x01
#define NO_HANDLE_FLAG                  0xFF

/* Function prototypes */
CCODE NWFillHandleStruct(NWHANDLE_PATH *pathInfo, char *path, UINT8 nameSpace);
CCODE FillHandleStruct(NWHANDLE_PATH *pathInfo, char *path, UINT8 nameSpace, UINT32 clientConnID);
CCODE NewFillHandleStruct(NWHANDLE_PATH *pathInfo, char *path, UINT8 nameSpace, CFS_STRUCT *cfsStruct);
CCODE getComp(char *component, UINT32 totalPathCount, UINT32 nameSpace, char *strOut);


#endif /* Header latch _FILHANDLE_H_ */
