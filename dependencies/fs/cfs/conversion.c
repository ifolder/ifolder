/**************************************************************************************
| (C) Copyright 2002 Novell, Inc.
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
|---------------------------------------------------------------------------
|
|	 Storage Management Services (SMS)
|
|---------------------------------------------------------------------------
|
| 	Author		:   Admin
| 	Modtime		:   5 June 2002 
|
| 	Workfile	:   Conversion.c  
| 	Revision	:   1.0  
|
|---------------------------------------------------------------------------
|
|	 This module is used to:
|    Interface between TSA #define's and z #defines's
|
|
****************************************************************************************/

#include <portable.h>
#include <smsencp.h>
#include <nwsms.h>


void FS_CFS_MapModifyFlags(UINT32 modifyFlag, QUAD* retModifyFlag)
{

	 if(modifyFlag & NWMODIFY_ATTRIBUTES)
              			(*retModifyFlag) |= MAttributes;
     if(modifyFlag & NWMODIFY_CREATION_TIME)
              			(*retModifyFlag) |= MCreationTime;
     if(modifyFlag & NWMODIFY_CREATOR_ID)
              			(*retModifyFlag) |= MCreatorID;
     if(modifyFlag & NWMODIFY_ARCHIVE_TIME)
              			(*retModifyFlag) |= MArchiveTime;
     if(modifyFlag & NWMODIFY_ARCHIVER_ID)
              			(*retModifyFlag) |= MArchiveID;
     if(modifyFlag & NWMODIFY_MODIFY_TIME)
              			(*retModifyFlag) |= MModifyTime;
     if(modifyFlag & NWMODIFY_MODIFIER_ID)
              			(*retModifyFlag) |= MModifyID;
     if(modifyFlag & NWMODIFY_ACCESSED_DATE)
              			(*retModifyFlag) |= MLastAccess;
}

