/****************************************************************************
 *
 *                      旼컴컴컴컴컴컴컴컴컴컴컴컴커
 *                      � FOR INTERNAL USE ONLY!!! �
 *                      읕컴컴컴컴컴컴컴컴컴컴컴컴켸
 *
 * Program Name:  Storage Management Services (NWSMS Lib)
 *
 * Filename:      Setfile.C
 *
 * Date Created:  30 MAY 2002
 *
 * Version:        2.0
 *
 * Programmers:   admin
 *
 * Files used:      nwsms.h, nwcntask.h
 *
 * Date Modified: 
 *
 * Modifications: 
 *
 * Comments:      
 *
 * (C) Unpublished Copyright of Novell, Inc.  All Rights Reserved.
 *
 * No part of this file may be duplicated, revised, translated, localized or
 * modified in any manner or compiled, linked or uploaded or downloaded to or
 * from any computer system without the prior written consent of Novell, Inc.
 ****************************************************************************/


#include <zOmni.h>

#ifdef N_PLAT_NLM
#include <portable.h>
#include <filhandle.h>
#include "cfsdefines.h"
#endif

#include <smstypes.h>
#include <smstserr.h>
#include <tsadset.h>
#include <timeconvc.h>
#include <convids.h>
#include <fsinterface.h>
#include <tsalib.h>
#include <tsaunicode.h>
#include <smsdebug.h>

#ifdef N_PLAT_NLM
void MapModifyFlags(UINT32 modifyFlag, QUAD* retModifyFlag) ; 
#endif

/*******************************************************************************/
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    TSA_LAYER|BACKUP|CRITICAL|COMPACT
#define FNAME   "FS_SetFileEntryInfo"
#define FPTR     FS_SetFileEntryInfo
CCODE FS_SetFileEntryInfo(
    					FS_FILE_OR_DIR_HANDLE    *fileOrDirHandle,
					    UINT32  	      	      setMask,
					    UINT32					  clientObjectID,
					    UINT32 					  fileAttributes,
						UINT32 	              	  archivedDateAndTime)
						
{
	CCODE	retval = 0;
	QUAD	modifyInfoMask = 0;
	zInfo_s  *modifyInfo = NULL;

	FStart();
	
	modifyInfo = (zInfo_s *)tsaMalloc(sizeof(zInfo_s)); 
	if(!modifyInfo)
	{
		retval = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
		
	MapModifyFlags(setMask, &modifyInfoMask);
	
	if (modifyInfoMask & zMOD_FILE_ATTRIBUTES)
	{
	   	modifyInfo->std.fileAttributes 	= fileAttributes; //set/reset the file arhive bit
       	modifyInfo->std.fileAttributesModMask  = -1; //moved from tsa600 code
	}

	if( modifyInfoMask & zMOD_ARCHIVED_TIME )
	{
	    NWConvertDOSToUTCTime(&modifyInfo->time.archived, archivedDateAndTime); //convert and set the datatime, return type is void
	}

	if( modifyInfoMask & zMOD_ARCHIVER_ID)
	{
	    FS_ConvertIDToGuid(clientObjectID, &(modifyInfo->id.archiver));//return type is STATUS
	}

	retval = FS_ModifyInfo(
						fileOrDirHandle,
				  	    modifyInfoMask,
				  	    sizeof(zInfo_s ),
				  	    (void *)modifyInfo);
	if(retval)
	{
		retval = NWSMTS_SET_FILE_INFO_ERR; 
	}
	
Return:

	if(modifyInfo)
		  tsaFree(modifyInfo);	

	FEnd(retval);
	
	return (retval);
	
} 
