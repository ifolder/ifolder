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
 | $Modtime:   20/06/2004 $
 |
 | $Workfile:   fsvfs_sidf.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		- Populate TSAs FS abstraction structures with SIDF data.
 ****************************************************************************/

/* Includes */
#include <fs_sidf.h>
#include <fsinterface.h>
#include <smstserr.h>
#include <string.h>
#include <smsdebug.h>
#include <tsa_defs.h>

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	TSAREAD
#define FNAME   "FS_VFS_SIDF_ConvertMACInfo"
#define FPTR 	FS_VFS_SIDF_ConvertMACInfo
CCODE FS_VFS_SIDF_ConvertMACInfo(void *information, SIDF_MAC_INFO *info)
{
	CCODE 		ccode = 0;
	zInfo_s		fs_info;
	FStart();

	if (!information || !info)
		{ccode = NWSMTS_INTERNAL_ERROR; FTrack(INTERNAL_ERROR,DC_CRITICAL|DC_COMPACT, "Internal Error\n"); }
	else
	{
		fs_info = *(zInfo_s *)information;

		if (fs_info.retMask & zGET_MAC_METADATA)
		{
			memcpy(info->finderInfo, fs_info.macNS.info.finderInfo, 32);
			memcpy(info->proDOSInfo, fs_info.macNS.info.proDOSInfo, 6);
			info->dirRightsMask = fs_info.macNS.info.dirRightsMask;
		}
		else
			{ccode = NWSMTS_INTERNAL_ERROR; FTrack(INTERNAL_ERROR,DC_CRITICAL|DC_COMPACT, "Internal Error\n"); }
	}
	FEnd(ccode);

	return ccode;
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	TSAREAD
#define FNAME   "FS_VFS_SIDF_ConvertNFSInfo"
#define FPTR 	FS_VFS_SIDF_ConvertNFSInfo
CCODE FS_VFS_SIDF_ConvertNFSInfo(void *information, SIDF_NFS_INFO *info)
{
	CCODE 		ccode = 0;
	zInfo_s		fs_info;
	FStart();

	if (!information || !info)
		{ccode = NWSMTS_INTERNAL_ERROR; FTrack(INTERNAL_ERROR,DC_CRITICAL|DC_COMPACT, "Internal Error\n"); }
	else
	{
		fs_info = *(zInfo_s *)information;
		if (fs_info.retMask & zGET_UNIX_METADATA)
		{
			info->fileAccessMode = fs_info.unixNS.info.fMode;
			info->groupOwnerID = fs_info.unixNS.info.nfsGID;
			info->RDevice = fs_info.unixNS.info.rDev;
			info->numberOfLinks = fs_info.count.hardLink;
			info->linkedFlag = 0;
			info->firstCreatedFlag = fs_info.unixNS.info.firstCreated;
			info->acSFlagsBit = fs_info.unixNS.info.acsFlags;
			info->userIDBit = fs_info.unixNS.info.nfsUID;
			info->myFlagsBit = fs_info.unixNS.info.myFlags;
			info->nwEveryone = fs_info.unixNS.info.nwEveryone;
			info->nwUserRights = fs_info.unixNS.info.nwUIDRights;
			info->nwGroupRights = fs_info.unixNS.info.nwGIDRights;
			info->nwEveryoneRights = fs_info.unixNS.info.nwEveryoneRights;
			info->nwOwnerID = fs_info.unixNS.info.nwUID;
			info->nwGroupID	= fs_info.unixNS.info.nwGID;
		}
		else
			{ccode = NWSMTS_INTERNAL_ERROR; FTrack(INTERNAL_ERROR,DC_CRITICAL|DC_COMPACT, "Internal Error\n"); }
	}
	FEnd(ccode);

	return ccode;
}
