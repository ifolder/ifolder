/****************************************************************************
 *
 *                      旼컴컴컴컴컴컴컴컴컴컴컴컴커
 *                      � FOR INTERNAL USE ONLY!!! �
 *                      읕컴컴컴컴컴컴컴컴컴컴컴컴켸
 *
 * Program Name:  Storage Management Services (NWSMS Lib)
 *
 * Filename:      CONVIDS.C
 *
 * Date Created:  8 DECEMBER 2000
 *
 * Version:       NetWare 6.0
 *
 * Programmers:   Piyush Kumar Srivastava
 *
 * Files used:    nwsms.h zOmni.h zFriends.h
 *
 * Date Modified: 12/8/00
 *
 * Modifications: For 6-Pack, the time conversion from UTC to DOS and vice-versa
 *				  were needed.
 *
 * Comments:	  This file contains the two ID conversion routines that map 128-bit
 *				  Guid to 32 bit IDs.
 *
 * (C) Unpublished Copyright of Novell, Inc.  All Rights Reserved.
 *
 * No part of this file may be duplicated, revised, translated, localized or
 * modified in any manner or compiled, linked or uploaded or downloaded to or
 * from any computer system without the prior written consent of Novell, Inc.
 ****************************************************************************/

#include <stdio.h>
#include <zFriends.h>
#include <zOmni.h>
#include <convids.h>

#include <smsdebug.h>

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_ConvertGuidToID"
#define FPTR     FS_ConvertGuidToID
STATUS FS_ConvertGuidToID(NDSid_t *guid, LONG *idp)
{
	STATUS status = 0;
	status = xGuidToId(guid, idp);
	if(status)
	{
	    FLogError("xGuidToId", status, NULL);
	}
	return status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_ConvertIDToGuid"
#define FPTR     FS_ConvertIDToGuid
STATUS FS_ConvertIDToGuid(LONG id, NDSid_t *guid)
{
	STATUS status = 0;
	status = xIdToGuid(id, guid);
	if(status)
	{
	    FLogError("xIdToGuid", status, NULL);
	}
	return status;
}

#ifdef N_PLAT_NLM
STATUS CFS_ConvertGuidToID( LONG *guid, LONG id)
{
	
	return 0;
}
#endif


