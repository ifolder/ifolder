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

#include <portable.h>
#include <filhandle.h>
#include "cfsdefines.h"


#include <smstypes.h>
#include <smstserr.h>
#include <tsadset.h>
#include <timeconvc.h>
#include <convids.h>
#include <fsinterface.h>
#include <tsalib.h>
#include <tsaunicode.h>
#include <smsdebug.h>


CCODE NewFillHandleStruct(NWHANDLE_PATH *pathInfo, char *path, UINT8 nameSpace, CFS_STRUCT *cfsStruct);
void MapModifyFlags(UINT32 modifyFlag, QUAD* retModifyFlag) ;
void FS_CFS_MapModifyFlags(UINT32 modifyFlag, QUAD* retModifyFlag);



#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    BACKUP|FILESYS |CRITICAL|COMPACT
#define FNAME   "FS_CFS_SetFileEntryInfo"
#define FPTR     FS_CFS_SetFileEntryInfo
CCODE FS_CFS_SetFileEntryInfo(
    					FS_FILE_OR_DIR_HANDLE    *fileOrDirHandle,
					    UINT32  	      	      setMask,
					    UINT32					  clientObjectID,
					    UINT32 					  fileAttributes,
						UINT32 	              	  archivedDateAndTime)
{
	CCODE				ccode=0;
	UINT32				pathLength=0;
	char				*path=NULL;
	NWMODIFY_INFO		*modifyInfo=NULL;
	UINT32				modifyInfoMask = 0;
    QUAD              	temp;
	NWHANDLE_PATH		*pathInfo=NULL;
	UINT32				searchAttributes=0x06; //All files, there is no point checking this after the scan so hard coded
	FStart();

	if(!fileOrDirHandle) 
	{
		ccode = NWSMTS_INVALID_PARAMETER;
		goto Return;
	}
	
 	pathInfo = (NWHANDLE_PATH *) tsaMalloc(sizeof(NWHANDLE_PATH)); 
 	if(!pathInfo)
 	{
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
 	}	
 	modifyInfo = (NWMODIFY_INFO *) tsaMalloc(sizeof(NWMODIFY_INFO)); 
 	if(!modifyInfo)
 	{
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
 	}
	
	if (!fileOrDirHandle->isDirectory)
	{
		ccode = SMS_UnicodeToByte(fileOrDirHandle->cfsStruct.uniFileDirName, &pathLength, &path, NULL);
		if(ccode)
		{
			ccode=NWSMTS_INTERNAL_ERROR;
			goto Return;
		}	

		path[pathLength] =NULL;
	}

	ccode = NewFillHandleStruct((NWHANDLE_PATH *) pathInfo, path, fileOrDirHandle->nameSpace, &(fileOrDirHandle->cfsStruct));
	if(ccode)
	{
		ccode=NWSMTS_INVALID_PATH; //0xFFDC
		goto Return;
	}
	temp = modifyInfoMask ;
	FS_CFS_MapModifyFlags(setMask, &temp);
	modifyInfoMask = temp ;

	modifyInfo->FileAttributes		= fileAttributes;
	modifyInfo->ArchivedDate		= archivedDateAndTime >> 16;		 
	modifyInfo->ArchivedTime		= (UINT16)archivedDateAndTime;
	modifyInfo->ArchiversID			= clientObjectID;
	
	ccode = GenNSModifyInfo(
						   fileOrDirHandle->cfsStruct.clientConnID,
						   ~(fileOrDirHandle->cfsStruct.clientConnID),
						   pathInfo,
						   fileOrDirHandle->cfsStruct.cfsNameSpace, 			 //expected DOSNameSpace, but no matter 
					   	   searchAttributes,
						   modifyInfoMask,
						   (ModifyInfo *)modifyInfo
						  ); 
	if(ccode)
	{
		FLogError("GenNSModifyInfo", ccode, path);
		ccode = NWSMTS_SET_FILE_INFO_ERR; 
	}

 Return:
   
    if(path)
      tsaFree(path);
    if(modifyInfo)
      tsaFree(modifyInfo);
    if(pathInfo)
      tsaFree(pathInfo);

    FEnd(ccode);
    
	return ccode;
}

void FS_CFS_SetEAUserFlags(NetwareInfo *fileInformation, UINT32 flags)
 {
	 ;
 }	

