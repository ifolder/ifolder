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
 | $Modtime:   03/05/2002 $
 |
 | $Workfile:   SidfPreFormat.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		- TBDTBD
 ****************************************************************************/

#ifndef _FS_SIDF_H
#define _FS_SIDF_H

/* Dependant includes */
#ifdef N_PLAT_NLM
#include <nwtypes.h>
#endif
#include <smstypes.h>
#include <fsinterface.h>
#include <zPublics.h>

#pragma pack(push,1)

/* structure defines */
typedef struct _SIDF_MAC_INFO
{
	BYTE	finderInfo[32];
	BYTE	proDOSInfo[6];
	LONG	dirRightsMask;
} SIDF_MAC_INFO;


typedef struct _SIDF_NFS_INFO
{
		UINT32						  fileAccessMode;
		UINT32						  groupOwnerID;
		UINT32						  numberOfLinks;
		UINT32						  RDevice;
		UINT8						  linkedFlag;
		UINT8						  firstCreatedFlag;

		UINT32						  userIDBit;
		UINT8						  acSFlagsBit;
		UINT32						  myFlagsBit;

		UINT32						  nwOwnerID;
		UINT32						  nwGroupID;
		UINT32						  nwEveryone;
		UINT32						  nwUserRights;
		UINT32						  nwGroupRights;
		UINT32						  nwEveryoneRights;
}SIDF_NFS_INFO;

#pragma pack(pop)


/* Function prototypes - NSS */
CCODE FS_SIDF_ConvertMACInfo(void *information, SIDF_MAC_INFO *info);
CCODE FS_SIDF_ConvertNFSInfo(void *information, SIDF_NFS_INFO *info);
CCODE FS_CFS_SIDF_ConvertMACInfo(void *information, FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, SIDF_MAC_INFO *macInfo);
CCODE FS_CFS_SIDF_ConvertNFSInfo(void *information, FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, SIDF_NFS_INFO *nfsInfo);


/* Function prototypes - Legacy */ 
#endif /* _FS_SIDF_H */
