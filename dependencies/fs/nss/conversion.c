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

#include <zFriends.h>
#ifdef N_PLAT_NLM
#include <portable.h>
#include <smsencp.h>
 #endif

#include <nwsms.h>
    
// Map the TSA defined attributes to NSS defined attributes
void MapModifyFlags(UINT32 modifyFlag, QUAD* retModifyFlag)
{
     if(modifyFlag & NWMODIFY_ATTRIBUTES)
             (*retModifyFlag) |= zMOD_FILE_ATTRIBUTES;
     if(modifyFlag & NWMODIFY_CREATION_TIME)
             (*retModifyFlag) |= zMOD_CREATED_TIME;
     if(modifyFlag & NWMODIFY_CREATOR_ID)
             (*retModifyFlag) |= zMOD_OWNER_ID;
     if(modifyFlag & NWMODIFY_ARCHIVE_TIME)
             (*retModifyFlag) |= zMOD_ARCHIVED_TIME;
     if(modifyFlag & NWMODIFY_ARCHIVER_ID)
             (*retModifyFlag) |= zMOD_ARCHIVER_ID;
     if(modifyFlag & NWMODIFY_MODIFY_TIME)
             (*retModifyFlag) |= zMOD_MODIFIED_TIME;
     if(modifyFlag & NWMODIFY_MODIFIER_ID)
             (*retModifyFlag) |= zMOD_MODIFIER_ID;
     if(modifyFlag & NWMODIFY_ACCESSED_DATE)
             (*retModifyFlag) |= zMOD_ACCESSED_TIME;
}


