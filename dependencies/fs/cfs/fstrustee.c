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
 | $Modtime: $
 |
 | $Workfile: $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This file is used to:
 |		Implement certain functions to obtain/modify trustees associated with a file object.
 +-------------------------------------------------------------------------*/

#include <stdio.h>
#include <stdlib.h>

#include <nwstring.h>
#include <filHandle.h>

#include <zPublics.h>
#include <zParams.h>
#include <zError.h>

#include <smstypes.h>
#include <fsinterface.h>

#include <convids.h>

#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>
#include <smsutapi.h>

#include <tsalib.h>
#include <tsaunicode.h>
#include <smsdebug.h>


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|CFS_SCAN
#define FNAME   "FS_CFS_ScanTrustees"
#define FPTR     FS_CFS_ScanTrustees
CCODE FS_CFS_ScanTrustees(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_FILE_OR_DIR_INFO *info)
{
	CCODE 				ccode = 0;
	UINT32				index = 0;
	UINT32				preIndex=0;
	FS_TRUSTEE_ID		*trusteeListIterator = NULL, *tail=NULL;
	UINT32 				loopCtr=0;	
	UINT32              trusteeID32;
	UINT16				trusteeCount = 0;
	NWHANDLE_PATH     	*pathInfo=NULL;
	NWTRUSTEES 			*trustees=NULL;
	char			 	*path=NULL;
	UINT32				pathLength=0;
	BOOL			isErrorEncountered=FALSE;
   	FStart();
    
	if((info->trusteeList) || (!dirEntrySpec ))
	{
		ccode = NWSMTS_INVALID_PARAMETER;
		goto Return;
	}
	
	pathInfo = (NWHANDLE_PATH *) tsaMalloc(sizeof(NWHANDLE_PATH));
	if(!pathInfo)
	{
	  ccode= NWSMTS_OUT_OF_MEMORY;
	  goto Return;
	}
	
	if(!dirEntrySpec->isDirectory)
	{
		if(dirEntrySpec->cfsStruct.uniFileDirName)
		{
			if (ccode = SMS_UnicodeToByte(dirEntrySpec->cfsStruct.uniFileDirName, &pathLength, &path, NULL)) 
			{
				ccode=NWSMTS_INTERNAL_ERROR;
				goto Return;
			}	
			path[pathLength] =NULL;
		}
		else
		{
	        ccode = NWSMTS_INVALID_PATH;
    	    goto Return;
        }
	}
	else
		path = NULL;
	
	ccode=NewFillHandleStruct((NWHANDLE_PATH *) pathInfo, path, dirEntrySpec->nameSpace, &(dirEntrySpec->cfsStruct));
	if(ccode)
		goto Return;	
	trustees = (NWTRUSTEES  *)tsaMalloc(sizeof(NWTRUSTEES) * 20);
	if(trustees == NULL)
	{
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}

	info->numberOfTrustees=0;//zeroing
	
	while(index !=-1)
	{
		preIndex=index;
		ccode = GenNSScanForTrustees(
 									dirEntrySpec->cfsStruct.clientConnID,
 									pathInfo,
            						dirEntrySpec->cfsStruct.cfsNameSpace,
            					   	NULL,
            					   	index,
			  						&(index), 
				 					&trusteeCount,  
									trustees
									);

		if(ccode)
		{
			if (ccode==0x9c) // This check ir required because this function returns every time this error code when it does not find trustee
				ccode=0;
			else
			{
				FLogError("GenNSScanForTrustees", ccode, path);
				ccode=NWSMTS_SCAN_TRUSTEE_ERR;
			}
			goto Return;
		}
		
		for(loopCtr=0;(loopCtr < trusteeCount);loopCtr++) 
		{
			trusteeID32 = trustees[loopCtr].ObjectID;
			trusteeListIterator = (FS_TRUSTEE_ID *)tsaMalloc(sizeof(FS_TRUSTEE_ID));
			if(trusteeListIterator == NULL)
			{
				ccode = NWSMTS_OUT_OF_MEMORY;
				goto Return;
			}
			trusteeListIterator->id.nid = trusteeID32;
			trusteeListIterator->rights = trustees[loopCtr].TrusteeRights;
			trusteeListIterator->next = NULL;
			if(tail)
				tail->next = trusteeListIterator;
			else
				info->trusteeList = trusteeListIterator;	
			tail = trusteeListIterator;
			info->numberOfTrustees++;	
		}
		
		//additional check to break the while loop and error out if the file sys Api loops itself
		if(index != -1 && (index-preIndex !=20))
		{
			ccode=NWSMTS_SCAN_TRUSTEE_ERR;
			break;
		}
	}
		
Return:
	if(isErrorEncountered==TRUE && !ccode)
		ccode=NWSMTS_SCAN_TRUSTEE_ERR;
	if(path)
		tsaFree(path);
	if(pathInfo)
	  	tsaFree(pathInfo);
 	if(trustees)
 		tsaFree(trustees);
	FEnd(ccode);
	return ccode;
	
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|RESTORE|TRUSTEES
#define FNAME   "FS_CFS_AddTrustee"
#define FPTR     FS_CFS_AddTrustee

CCODE FS_CFS_AddTrustee(FS_FILE_OR_DIR_HANDLE *dirEntrySpec,
                              UINT32                  objectID,
                              UINT16                  trusteeRights)
{
	CCODE 				ccode=0;
 	NWTRUSTEES 			*trustees=NULL;
    UINT16        		searchAttributes=0;
	NWHANDLE_PATH     	*pathInfo=NULL; 
	char			 	*path=NULL;
	UINT32  		 	fileDirLength=0;    
	FStart();
	
	pathInfo = (NWHANDLE_PATH *) tsaMalloc(sizeof(NWHANDLE_PATH));
	if(!pathInfo)
	{
	  ccode= NWSMTS_OUT_OF_MEMORY;
	  goto Return;
	}
	
	if(!dirEntrySpec->isDirectory)
	{
		if(dirEntrySpec->cfsStruct.uniFileDirName)
		{
			if (ccode = SMS_UnicodeToByte(dirEntrySpec->cfsStruct.uniFileDirName, &fileDirLength, &path, NULL)) 
			{
				ccode=NWSMTS_INTERNAL_ERROR;
				goto Return;
			}	
			path[fileDirLength] =NULL;
		}
		else
		{
	        ccode = NWSMTS_INVALID_PATH;
    	    goto Return;
        }
	}
	else
		path = NULL;
	
	
	ccode = NewFillHandleStruct((NWHANDLE_PATH *) pathInfo, path, dirEntrySpec->nameSpace, &(dirEntrySpec->cfsStruct));
	if(ccode)
		goto Return;
	
	trustees = (NWTRUSTEES  *)tsaMalloc(sizeof(NWTRUSTEES) * 20);
	if(trustees == NULL)
	{
		ccode =NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
	
	trustees->ObjectID = NWSwap32(objectID);
	ccode=GenNSAddTrustees(
    			dirEntrySpec->cfsStruct.clientConnID, 
    			pathInfo,
            	(UINT8)dirEntrySpec->cfsStruct.cfsNameSpace, 
            	searchAttributes, 
            	trusteeRights, 
            	0x0001,
				trustees);
	
    if(ccode)
    {
       	FLogError("GenNSAddTrustees", ccode, path);
    	goto Return;
    }

Return:
	if(path)
		tsaFree(path);
	if(pathInfo)
	  	tsaFree(pathInfo);
	if(trustees)
 		tsaFree(trustees);
	
	FEnd(ccode);
	return ccode;
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|RESTORE|TRUSTEES
#define FNAME   "FS_CFS_DeleteTrustees"
#define FPTR     FS_CFS_DeleteTrustees
CCODE FS_CFS_DeleteTrustees(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_FILE_OR_DIR_INFO *info)
{
	CCODE				ccode=0;
	UINT32				fileDirLength=0;
	//UINT32				pathLength=0;
	NWHANDLE_PATH     	*pathInfo=NULL; 
	char			 	*path=NULL;
	NWTRUSTEES 			*trustees=NULL;
	UINT32				trusteeCount=0;
	FS_TRUSTEE_ID		*trusteeParser=NULL;
	UINT32				trustDelCount=0;
	STATUS 				saveError=0;
	FStart();

	if(info)
        trusteeParser = info->trusteeList;
    else
    {
        ccode = NWSMTS_INVALID_PARAMETER;
        FTrack(RESTORE|TRUSTEES, CRITICAL|COMPACT , "info is NULL\n");
        goto Return;
    }

	pathInfo = (NWHANDLE_PATH *) tsaMalloc(sizeof(NWHANDLE_PATH));
	if(!pathInfo)
	{
	  ccode= NWSMTS_OUT_OF_MEMORY;
	  goto Return;
	}
	if(!fileOrDirHandle->isDirectory)
	{
		if(fileOrDirHandle->cfsStruct.uniFileDirName)
		{
			if (ccode = SMS_UnicodeToByte(fileOrDirHandle->cfsStruct.uniFileDirName, &fileDirLength, &path, NULL)) 
			{
				ccode=NWSMTS_INTERNAL_ERROR;
				goto Return;
			}	
			path[fileDirLength] =NULL;
		}
		else
		{
	        ccode = NWSMTS_INVALID_PATH;
    	    goto Return;
        }
	}
	else
		path = NULL;

	ccode=NewFillHandleStruct((NWHANDLE_PATH *) pathInfo, path, fileOrDirHandle->nameSpace, &(fileOrDirHandle->cfsStruct));
	if(ccode)
		goto Return;
	
	//GenNSDeleteTrustees accepts at the max 20 trustees in an array
	trustees = (NWTRUSTEES  *)tsaMalloc(sizeof(NWTRUSTEES) * 20);
	if(!trustees)
	{
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}

	while(trusteeParser)
	{
	 	for(trusteeCount=0; trusteeCount<20; trusteeCount++)
		{
			trustees[trusteeCount].ObjectID = trusteeParser->id.nid;
			if(ccode)
			{
				goto Return;
			}
			trusteeParser = trusteeParser->next;
			if(!trusteeParser)
			  break;	
		}
	 	trustDelCount=trustDelCount+trusteeCount;
		ccode = GenNSDeleteTrustees( 
								  fileOrDirHandle->cfsStruct.clientConnID, 
								  (NWHANDLE_PATH *)pathInfo, 
								  (UINT8)fileOrDirHandle->cfsStruct.cfsNameSpace, 
							 	  trusteeCount+1, 
								  trustees
								  );
		if(ccode) 
		{
			FLogError("GenNSDeleteTrustees", ccode, NULL);
			saveError=ccode;
			ccode=0;
		}

		if(info->numberOfTrustees<trustDelCount) //break if the received trustee list is circular
			break;
	}

Return:
		
	ccode = saveError;
	if(pathInfo)
		tsaFree(pathInfo);
	if(path)
		tsaFree(path);
	if(trustees)
		tsaFree(trustees);
	
	FEnd(ccode);
	return ccode;
}
