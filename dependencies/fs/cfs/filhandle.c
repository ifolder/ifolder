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

/* External dependecies */
#include <stdio.h>

#include <smstserr.h>
#include <smsutapi.h>

#include <compath.h>
#include <tsaname.h>
#include <filhandle.h>
#include <restore.h>
#include <fsInterface.h>
#include <tsa_defs.h>
#include <tsa_320.mlh>

#include <cfsdefines.h>
#include <smsdebug.h>
#include <lfsproto.h>

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    SMS_INTERNAL
#define FNAME   "NewFillHandleStruct"
#define FPTR     NewFillHandleStruct
CCODE NewFillHandleStruct(NWHANDLE_PATH *pathInfo, char *path, UINT8 nameSpace, CFS_STRUCT *cfsStruct)
{
	CCODE			ccode=0;
	
	FStart();	
	
	pathInfo->HandleFlag=DIRECTORY_BASE_FLAG;  
	pathInfo->Volume=cfsStruct->volumeNumber;
	pathInfo->DirectoryBaseOrHandle=cfsStruct->directoryNumber;
			
	ccode = _NWStoreAsComponentPath(
										(STRING) &(pathInfo->PathComponentCount),
										(UINT8)nameSpace, 	
										path,							
										FALSE //pathIsFullyQualified
										);
	FEnd(ccode);
	return ccode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    SMS_INTERNAL
#define FNAME   "FillHandleStruct"
#define FPTR     FillHandleStruct
CCODE FillHandleStruct(NWHANDLE_PATH *pathInfo, char *path, UINT8 nameSpace, UINT32 clientConnID)
{
	CCODE					ccode=0;
	BOOL					pathIsFullyQualified=TRUE;
	
	UINT32					volNum=0;
	struct DirectoryStructure	*dStruct=NULL;
	UINT32					dirBase=0;
	NWHANDLE_PATH			*pInfo=NULL;
	char					*relPath=NULL;
	char					*strOut=NULL;
	UINT32					totalPathCount=0;
	UINT32					comp=0;

	UINT32          		att=0;
	char            		*name=NULL;
	NetwareInfo     		*netWareInfo=NULL;

	FStart();

	if(!path || strlen(path)==0)
	{
		ccode=NWSMTS_INVALID_PATH;
		goto Return;
	}

	pInfo = (NWHANDLE_PATH *)tsaMalloc(sizeof(NWHANDLE_PATH));
	if(!pInfo)
	{
		ccode= NWSMTS_OUT_OF_MEMORY;
		goto Return;	
	}
	strOut = (char*)tsaMalloc(sizeof(char)*FILE_MAX_LENGTH + 1);
	if(!strOut)
	{
		ccode= NWSMTS_OUT_OF_MEMORY;
		goto Return;	
	}
	name = (char *) tsaMalloc(FILE_MAX_LENGTH+2);
	netWareInfo = (NetwareInfo *) tsaMalloc(sizeof(NetwareInfo));

	if( !name || !netWareInfo)
	{
		ccode=NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
		
	relPath=removeVolName(path, 0, nameSpace);
	if(!relPath)
	{
		pathInfo->HandleFlag=NO_HANDLE_FLAG;  
		pathInfo->Volume=0;
		pathInfo->DirectoryBaseOrHandle=0;
		pathIsFullyQualified=TRUE;
		ccode = _NWStoreAsComponentPath(
					        (STRING) &(pathInfo->PathComponentCount),
	  				  	    (UINT8)nameSpace, 	
	  				  	    path,							
							pathIsFullyQualified
		           			);
		if(ccode)
			ccode=NWSMTS_INVALID_PATH;
		goto Return;
	}
	else
	{
		if((nameSpace == DOSNameSpace) || (nameSpace == OS2NameSpace) ||(nameSpace == NFSNameSpace))
		{
			totalPathCount = GetNumberOfNodeNamesInThePath(relPath, strlen(relPath), NULL, (char *)_GetMessage(SLASH));
		}
		else if(nameSpace == MACNameSpace)
		{
			totalPathCount = GetNumberOfNodeNamesInThePath(relPath, strlen(relPath), NULL, (char *)_GetMessage(COLON));
		}
	}
	
	ccode= getVolumeNumber(path, nameSpace, &volNum);
	if(ccode) goto Return;

	//totalPathCount has number of components in the given path excluding the volume
	for(comp=1; comp<=totalPathCount; comp++)
	{
		ccode = getComp(relPath, comp, nameSpace, strOut);
		if(ccode)
			goto Return;
		pInfo->DirectoryBaseOrHandle=dirBase;
		if(pInfo->DirectoryBaseOrHandle)
			pathIsFullyQualified=FALSE;

		ccode = _NWStoreAsComponentPath(
				 					      (STRING) &(pInfo->PathComponentCount),
  									  	  (UINT8)nameSpace, 	
  				  		    			  strOut,							
										  pathIsFullyQualified 
       				    				  );	
		if(ccode)
		{
			ccode = NWSMTS_INVALID_PATH;
			goto Return;
		}
		ccode=GetEntryFromPathStringBase(
								  clientConnID,
								  volNum,
								  pInfo->DirectoryBaseOrHandle,
								  pInfo->PathString,
								  pInfo->PathComponentCount,  
								  nameSpace,
								  nameSpace,
								  &dStruct,
								  &dirBase				  
								  );
		if(ccode)
		{
			ccode = NWSMTS_INVALID_PATH;
			goto Return;
		}
		if(comp== totalPathCount)//last comp may be file so checkit out
		{
			pInfo->HandleFlag=DIRECTORY_BASE_FLAG;
			pInfo->Volume = volNum;
			ccode = GenNSObtainInfo(
					               clientConnID, 
					               pInfo,
					               (UINT8)nameSpace,
					               (UINT8)nameSpace,
					               NWSA_ALL_DIRS,
					       		   NWRETURN_ATTRIBUTES,
					               netWareInfo,
					               (BYTE *)name);
			if(ccode) {ccode = NWSMTS_SCAN_ERROR; goto Return;}

            att=FS_CFS_GetFileAttributes(netWareInfo);
			if(!(att & FS_CFS_SUBDIRECTORY)) 
			{
				pathInfo->HandleFlag=DIRECTORY_BASE_FLAG;  
				pathInfo->Volume=volNum;
				pathInfo->DirectoryBaseOrHandle=pInfo->DirectoryBaseOrHandle;
				ccode = _NWStoreAsComponentPath(
						 					      (STRING) &(pathInfo->PathComponentCount),
		  									  	  (UINT8)nameSpace, 	
		  				  		    			  strOut,							
												  pathIsFullyQualified 
		       				    				  );
				if(ccode)
					ccode=NWSMTS_INVALID_PATH;
				goto Return;
			}
		}
				
 	}
		
	pathInfo->HandleFlag=DIRECTORY_BASE_FLAG;  
	pathInfo->Volume=volNum;
	pathInfo->DirectoryBaseOrHandle=dirBase;
	
	ccode = _NWStoreAsComponentPath(
					      			    (STRING) &(pathInfo->PathComponentCount),
  					  	   			    (UINT8)nameSpace, 	
			                 	        NULL,							
									    pathIsFullyQualified
		                           		);
	if(ccode)
		ccode=NWSMTS_INVALID_PATH;
	
Return:
	if(pInfo)
	  tsaFree(pInfo);
	if(strOut)
	  tsaFree(strOut);
	if(name)
	  tsaFree(name);
	if(netWareInfo)
	  tsaFree(netWareInfo);
	
	FEnd(ccode);		
	return ccode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    SMS_INTERNAL
#define FNAME   "getComp"
#define FPTR     getComp
CCODE getComp(char *component, UINT32 totalPathCount, UINT32 nameSpace, char *strOut)
{
	CCODE		ccode=0;
	UINT32		pathCount;
	char		*position =NULL;
	char		*start=NULL;
	char		*end=NULL;

	FStart();
	
	if(!component)
	{
		ccode = NWSMTS_INVALID_PATH;
		goto Return;
	}
	position = component;
	totalPathCount--;
	
	//Start location 
	if((nameSpace == DOSNameSpace) || (nameSpace == OS2NameSpace) ||(nameSpace == NFSNameSpace))
	{
		for(pathCount=0; pathCount< totalPathCount; pathCount++)
		{
			position = strstr(position, (char *) _GetMessage(SLASH));
			if(position)
			position++;
		}
	}
	else if(nameSpace == MACNameSpace)
	{
		for(pathCount=0; pathCount< totalPathCount; pathCount++)
		{
			position = strstr(position, (char *)_GetMessage(COLON));
			if(position)
			position++;
		}
	}
	else
	{
		ccode = NWSMTS_INVALID_NAME_SPACE_TYPE;
		goto Return;
	}	
	start=position;
	//Start location captured
	
	//End Location
	if((nameSpace == DOSNameSpace) || (nameSpace == OS2NameSpace) ||(nameSpace == NFSNameSpace))
	{
		position = strstr(position,(char *) _GetMessage(SLASH));
	}
	else if(nameSpace == MACNameSpace)
	{
		position = strstr(position, (char *)_GetMessage(COLON));
	}
	if(position)
		end=position;
	else
		position=NULL;
	//End location captured
	if(start && end && abs(end-start)<=255)
	{
		memcpy(strOut, start, abs(end-start));
		strOut[abs(end-start)]=NULL;
	}
	else if(start && !end && (strlen(start) <=255))
	{
		pathCount=strlen(start);
		memcpy(strOut, start, pathCount);
		strOut[pathCount]=NULL;
	}
	else
		ccode=NWSMTS_INVALID_PATH;
		
Return:

	FEnd(ccode);
	return ccode;
}



#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    SMS_INTERNAL
#define FNAME   "NWFillHandleStruct"
#define FPTR     NWFillHandleStruct
CCODE NWFillHandleStruct(NWHANDLE_PATH *pathInfo, char *path, UINT8 nameSpace)
{
	
	CCODE			ccode=0;
	UINT32 			pathLength;
	BOOL		pathIsFullyQualified;

	pathLength=strlen(path);
		
	if(pathLength < 300)
	{
		pathInfo->HandleFlag=NO_HANDLE_FLAG;  
		pathInfo->Volume=0;
		pathInfo->DirectoryBaseOrHandle=0;
		pathIsFullyQualified=TRUE;
	}
	else // if(pathLength>=300)
	{
	   ;//TBD
	}
	ccode = _NWStoreAsComponentPath(
				        (STRING) &(pathInfo->PathComponentCount),
  				  	    (UINT8)nameSpace, 	
  				  	    path,							
						pathIsFullyQualified
            			);	
	return ccode;
}
