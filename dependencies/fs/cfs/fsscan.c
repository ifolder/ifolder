/****************************************************************************
 |
 |  (C) Copyright 2001 Novell, Inc.
 |      All Rights Reserved.
 |
 |      This program is an unpublished copyrighted work which is proprietary
 |      to Novell, Inc. and contains confidential information that is not
 |      to be reproduced or disclosed to any other person or entity without
 |      prior written consent from Novell, Inc. in each and every instance.
 |
 |      WARNING:  Unauthorized reproduction of this program as well as
 |      unauthorized preparation of derivative works based upon the
 |      program or distribution of copies by sale, rental, lease or
 |      lending are violations of federal copyright laws and state trade
 |      secret laws, punishable by civil and criminal penalties.
 |
 |***************************************************************************
 |
 |       Storage Management Services (SMS)
 |
 |---------------------------------------------------------------------------
 |
 | $Author$
 | $Modtime:   27 Mar 2002 17:05:44  $
 |
 | $Workfile:   fsScan.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |      This module is used to:
 |              Define Scan functions in the file system interface layer.
 +-------------------------------------------------------------------------*/

#include <fsinterface.h>
#include <tsajob.h>
#include <tsalib.h>
#include <tsadset.h>
#include <tsaunicode.h>
#include <smsdebug.h>
#include <wildpath.h>
#include <smsdebug.h>
#include <incexc.h> 
#include <tsaresources.h>
#include <filhandle.h>

#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>
#include <smsutapi.h>

#include <stdio.h>
#include <stdlib.h>
#include <lfsproto.h>
#include <customdebug.h>
#include <zPublics.h>
#include <zParams.h>
#include <zError.h>


/* External Globals */
extern UINT32 totalObjectsScannedByTSA;
extern unsigned int     tsazInfoVersion;

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN|FILESYS
#define FNAME   "FS_CFS_InitScan"
#define FPTR     FS_CFS_InitScan
CCODE FS_CFS_InitScan(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence)
{
	CCODE				ccode = 0;
    char                *path=NULL;
    NWHANDLE_PATH     *pathInfo=NULL;
    UINT32                pathLength=0;
    char                *pattern=NULL;
    
    FStart();

    //FS_SEARCH_TYPE search is not used for pattern, instead generated here
    infoMask=0x2FFF;//TSA passess for NSS, so changed for legacy
    
    if(!dirEntrySpec || !scanSequence)
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

    if(ccode = SMS_UnicodeToByte(scanSequence->CFSscanStruct.uniFullpath, &pathLength, &path, NULL)) 
    {
	    ccode=NWSMTS_INTERNAL_ERROR;
	    goto Return;
    }       
    	
	if(search)
	{
	    if(ccode = SMS_UnicodeToByte((unicode *)search, &pathLength, &pattern, NULL)) 
	 	   goto Return;
	}
	       
    ccode = FillHandleStruct( (NWHANDLE_PATH *) pathInfo, path, dirEntrySpec->nameSpace,scanSequence->CFSscanStruct.clientConnID);
    if(ccode)
		goto Return;
        
    ccode = GenNSInitFileSearch(
 			                 scanSequence->CFSscanStruct.clientConnID,
                             pathInfo,      // Is a structure containing path base and path name information about the directory to be searched.
                             dirEntrySpec->nameSpace,
                             &(scanSequence->CFSscanStruct.volumeNumber),             
                             &(scanSequence->CFSscanStruct.directoryNumber),
                             &(scanSequence->CFSscanStruct.entryNumber)
                             );
    if(ccode)
    {
	    //FLogError("GenNSInitFileSearch", ccode, NULL);
	    ccode=NWSMTS_SCAN_ERROR;
	    goto Return;
    }
    else
    {
		scanSequence->informationMask = infoMask; //this is to passed to continueFilesearch
		NWStoreAsWildPath(scanSequence->CFSscanStruct.cfsPattern, dirEntrySpec->nameSpace, search? pattern:"*");
    }
Return:

    if(path)
      tsaFree(path);
    if(pattern)
      tsaFree(pattern);
    if(pathInfo)
      tsaFree(pathInfo);    
    
    FEnd(ccode);
    return ccode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN|FILESYS
#define FNAME   "FS_CFS_RInitScan"
#define FPTR     FS_CFS_RInitScan
CCODE FS_CFS_RInitScan(FS_FILE_OR_DIR_HANDLE *dirEntrySpec, FS_SEARCH_TYPE search, FS_INFORMATION_MASK infoMask, FS_SCAN_SEQUENCE *scanSequence, char *path)
{
	CCODE				ccode = 0;
    NWHANDLE_PATH     *pathInfo=NULL;
    char                *pattern=(char*)search;
    
    FStart();

    //FS_SEARCH_TYPE search is not used for pattern, instead generated here
    infoMask=0x2FFF;//TSA passess for NSS, so changed for legacy
    
    if(!dirEntrySpec || !scanSequence)
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
    ccode = FillHandleStruct( (NWHANDLE_PATH *) pathInfo, path, dirEntrySpec->nameSpace,scanSequence->CFSscanStruct.clientConnID);
    if(ccode)
		goto Return;
        
    ccode = GenNSInitFileSearch(
 			                 scanSequence->CFSscanStruct.clientConnID,
                             pathInfo,      // Is a structure containing path base and path name information about the directory to be searched.
                             dirEntrySpec->nameSpace,
                             &(scanSequence->CFSscanStruct.volumeNumber),             
                             &(scanSequence->CFSscanStruct.directoryNumber),
                             &(scanSequence->CFSscanStruct.entryNumber)
                             );
    if(ccode)
    {
	    //FLogError("GenNSInitFileSearch", ccode, NULL);
	    ccode=NWSMTS_SCAN_ERROR;
	    goto Return;
    }
    else
    {
		scanSequence->informationMask = infoMask; //this is to passed to continueFilesearch
		NWStoreAsWildPath(scanSequence->CFSscanStruct.cfsPattern, dirEntrySpec->nameSpace, search? pattern:"*");
    }
Return:

    if(pathInfo)
      tsaFree(pathInfo);    
    
    FEnd(ccode);
    return ccode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN
#define FNAME   "FS_CFS_ReInitScan"
#define FPTR     FS_CFS_ReInitScan

CCODE FS_CFS_ReInitScan(FS_SCAN_SEQUENCE *scanSequence)
{
        /*This will ensure that the search will begin from the first dir (or) file for the given */
        if(!scanSequence)
        {
                 return FS_PARAMETER_ERROR;
        }
        scanSequence->CFSscanStruct.entryNumber = -1;
        return 0;
}
        

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN|FILESYS
#define FNAME   "FS_CFS_ScanNextFileEntry"
#define FPTR     FS_CFS_ScanNextFileEntry
CCODE FS_CFS_ScanNextFileEntry(FS_SCAN_SEQUENCE *fileScanSequence, FS_FILE_OR_DIR_INFO *fileInfo)
{
    CCODE 			 ccode=0, uniccode = 0;
    UINT32           actLen=0;
    BYTE             fileName[257];
    char 			 buf1[256];
    char 			*path=NULL;
    UINT32           iter;
    UINT32 			 uniPathLen=0;
    unicode         *uniPath=NULL;
    LONG             nsCount;  
    UINT8            nsList[MAX_NAME_SPACES] = {0};
    UINT8            nsSupportedList[MAX_NAME_SPACES] = {0};
    FStart();
    
    if(!fileScanSequence || !fileInfo)
    {
		ccode = NWSMTS_INVALID_PARAMETER;
		goto Return;
    }

    ccode = GenNSContinueFileSearch(
					fileScanSequence->CFSscanStruct.clientConnID,
					DOSNameSpace,                   //(BYTE)fileScanSequence->CFSscanStruct.NameSpace,
                    0x00,                           //unused
                    fileInfo->FileMatchCriteria,                                    //Copied from Tsa600
                    fileScanSequence->informationMask,                      //NWRETURN_ALL_INFO, // (0x2FFF) 
                    fileScanSequence->CFSscanStruct.volumeNumber,
                    fileScanSequence->CFSscanStruct.directoryNumber,
                    fileScanSequence->CFSscanStruct.entryNumber,  //Is the entry number at which the search is to begin.  A value of -1 indicates that the search should begin at the start of the directory.  To continue a previous search, the value returned in NewSEntryNumber by a previous call to this function should be used for this parameter.
                    fileScanSequence->CFSscanStruct.cfsPattern,    //This may include wildcard characters
                    &(fileScanSequence->CFSscanStruct.entryNumber),
                    (NetwareInfo *) fileInfo->information,      //(NetwareInfo *)fileInfo->information 
                    &fileName                       //caller provided buffer provided must be able to handle 256 bytes (1 byte length and 255 max bytes for the name)
                );
    if(!ccode)
    {
        memcpy(buf1, &(fileName[1]),fileName[0]);
		buf1[fileName[0]]='\0';
        fileInfo->isDirectory = FALSE;
        fileInfo->isVolume = FALSE;
        totalObjectsScannedByTSA++;
    }
    else
    {
        ccode = NWSMTS_SCAN_ERROR;
        goto Return;
    }

	if (fileScanSequence->CFSscanStruct.uniFileDirName)
	{
		tsaFree(fileScanSequence->CFSscanStruct.uniFileDirName);
		fileScanSequence->CFSscanStruct.uniFileDirName = NULL;
	}
    if(ccode = SMS_ByteToUnicode(buf1, &actLen, &fileScanSequence->CFSscanStruct.uniFileDirName, NULL))
    {
        ccode = NWSMTS_INTERNAL_ERROR;
        goto Return;
    }
    
    /* name space population */      
    GetVolSupportedNameSpaces(fileScanSequence->CFSscanStruct.volumeNumber, &nsCount, nsSupportedList);
 
    while(nsCount--)
    {
		nsList[nsSupportedList[nsCount]] = 1;
    } 
    
    for(iter=0; iter<=LAST_NAMESPACE; iter++)
    {
		if(iter==FTAMNameSpace || nsList[iter]==FALSE) 
        	continue;
		
        ccode = FS_CFS_GetFileNameSpaceName(
                    &(fileScanSequence->CFSscanStruct),
                    buf1,
                    DOSNameSpace,
                    iter,
                    fileInfo->FileMatchCriteria,
                    fileName);
        if(!ccode)
        {
        	uniPathLen = 0;
			if (fileInfo->cfsNameSpaceInfo[iter])
			{
				tsaFree(fileInfo->cfsNameSpaceInfo[iter]);
				fileInfo->cfsNameSpaceInfo[iter] = NULL;
			}
			ccode = SMS_ByteToUnicode(fileName, &uniPathLen, &fileInfo->cfsNameSpaceInfo[iter], NULL);
			if (ccode)
				goto Return;
        	
			if(fileInfo->cfsAsciiNameSpaceInfo[iter])
			{
				tsaFree(fileInfo->cfsAsciiNameSpaceInfo[iter]);
				fileInfo->cfsAsciiNameSpaceInfo[iter] = NULL;
			}
						
			fileInfo->cfsAsciiNameSpaceInfo[iter] = (char*)tsaMalloc(strlen(fileName)+1);
			if(!fileInfo->cfsAsciiNameSpaceInfo[iter])
			{
				ccode = NWSMTS_OUT_OF_MEMORY;
				goto Return;
			}			
			strcpy(fileInfo->cfsAsciiNameSpaceInfo[iter], fileName);
		}
    }
	
Return:
    if(path)
		tsaFree(path);
	
    if(uniPath)
		tsaFree(uniPath);
	
	if (!ccode && uniccode)
            ccode = uniccode;
     
    FEnd(ccode);
    return  ccode;
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN|FILESYS
#define FNAME   "FS_CFS_ScanNextDirectoryEntry"
#define FPTR     FS_CFS_ScanNextDirectoryEntry

CCODE FS_CFS_ScanNextDirectoryEntry(FS_SCAN_SEQUENCE *dirScanSequence, FS_FILE_OR_DIR_INFO *dirInfo)
{
    CCODE		 ccode=0, uniccode = 0;
	BYTE		 fileName[257];
	char		 buf1[256];
	unicode     *uniPath=NULL;
	UINT32       iter=0;
	UINT32       actLen=0;
	char 		*path=NULL;
	LONG 		 nsCount;
	UINT8		 nsList[MAX_NAME_SPACES] = {0};
	UINT8		 nsSupportedList[MAX_NAME_SPACES] = {0};

    FStart();
        
    if(!dirScanSequence || !dirInfo)
    {
        ccode = NWSMTS_INVALID_PARAMETER;
        goto Return;
    }

    ccode = GenNSContinueFileSearch(
			                    dirScanSequence->CFSscanStruct.clientConnID,
			                    DOSNameSpace,     //(BYTE)fileScanSequence->CFSscanStruct.NameSpace,
			                    0x00,                  //unused
			                    dirInfo->DirMatchCriteria,                     //SEARCH ALL DIR 
			                    dirScanSequence->informationMask,                       //NWRETURN_ALL_INFO (0x2FFF) 
			                    dirScanSequence->CFSscanStruct.volumeNumber,
			                    dirScanSequence->CFSscanStruct.directoryNumber,
			                    dirScanSequence->CFSscanStruct.entryNumber,  //Is the entry number at which the search is to begin.  A value of -1 indicates that the search should begin at the start of the directory.  To continue a previous search, the value returned in NewSEntryNumber by a previous call to this function should be used for this parameter.
			                    dirScanSequence->CFSscanStruct.cfsPattern,     //This may include wildcard characters
			                    &(dirScanSequence->CFSscanStruct.entryNumber),
			                    (NetwareInfo *)dirInfo->information,        //(NetwareInfo *)fileInfo->information 
			                    &fileName          //caller provided buffer provided must be able to handle 256 bytes (1 byte length and 255 max bytes for the name)
			                    );
    if(!ccode)
    {
	    memcpy(buf1, &(fileName[1]),fileName[0]);
		buf1[fileName[0]]='\0';
		fileName[fileName[0]+1]=NULL;
	    dirInfo->isDirectory = TRUE;
	    dirInfo->isVolume = FALSE;
	    totalObjectsScannedByTSA++;
    }
    else
    {
        ccode = NWSMTS_SCAN_ERROR;
        goto Return;
    }

	if (dirScanSequence->CFSscanStruct.uniFileDirName)
	{
		tsaFree(dirScanSequence->CFSscanStruct.uniFileDirName);
		dirScanSequence->CFSscanStruct.uniFileDirName = NULL;
	}
    if(ccode = SMS_ByteToUnicode(buf1, &actLen, &dirScanSequence->CFSscanStruct.uniFileDirName, NULL))
    {
        ccode = NWSMTS_INTERNAL_ERROR;
        goto Return;
    }

    /*name space population */    

    GetVolSupportedNameSpaces(dirScanSequence->CFSscanStruct.volumeNumber, &nsCount, nsSupportedList);

    while(nsCount--)
    {
		nsList[nsSupportedList[nsCount]] = 1;
    }
    
    for(iter=0; iter<=LAST_NAMESPACE; iter++)
    {
        if(iter==FTAMNameSpace || nsList[iter]==FALSE) 
			continue;
		
        ccode = FS_CFS_GetFileNameSpaceName(
					                        &(dirScanSequence->CFSscanStruct),//dirScanSequence->CFSscanStruct.clientConnID,
					                        buf1,//path,
					                        DOSNameSpace,
					                        iter,
					                        dirInfo->DirMatchCriteria,
					                        fileName);
        if(!ccode)
        {
        	actLen = 0;
			if (dirInfo->cfsNameSpaceInfo[iter])
			{
				tsaFree(dirInfo->cfsNameSpaceInfo[iter]);
				dirInfo->cfsNameSpaceInfo[iter] = NULL;
			}
			ccode = SMS_ByteToUnicode(fileName, &actLen, &dirInfo->cfsNameSpaceInfo[iter], NULL);
			if (ccode)
			{
               	goto Return;
			}
        	
			if(dirInfo->cfsAsciiNameSpaceInfo[iter])
			{
				tsaFree(dirInfo->cfsAsciiNameSpaceInfo[iter]);
				dirInfo->cfsAsciiNameSpaceInfo[iter] = NULL;
			}

			dirInfo->cfsAsciiNameSpaceInfo[iter] = (char*)tsaMalloc(strlen(fileName)+1);
			if(!dirInfo->cfsAsciiNameSpaceInfo[iter])
			{
				ccode = NWSMTS_OUT_OF_MEMORY;
				goto Return;
			}
			strcpy(dirInfo->cfsAsciiNameSpaceInfo[iter], fileName);
        }
    }  
Return: 
    if(path)
        tsaFree(path);
    if(uniPath)
        tsaFree(uniPath);
	if (!ccode && uniccode)
		ccode = uniccode;
     
    FEnd(ccode);
    return ccode;
}
/*************************************************************************************************************************************/
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN
#define FNAME   "FS_CFS_EndScan"
#define FPTR     FS_CFS_EndScan

CCODE FS_CFS_EndScan(FS_SCAN_SEQUENCE *scanSequence)
{
        //do nothing in legacy
        return 0;
}
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN|FILESYS
#define FNAME   "FS_CFS_RScanNextDirectoryEntry"
#define FPTR     FS_CFS_RScanNextDirectoryEntry

CCODE FS_CFS_RScanNextDirectoryEntry(FS_SCAN_SEQUENCE *dirScanSequence, FS_FILE_OR_DIR_INFO *dirInfo, UINT32 nameSpace)
{
    CCODE				ccode=0;
    BYTE                fileName[257];
    UINT32				searchAttributes=0x16;

    FStart();

    ccode = GenNSContinueFileSearch(
	                                dirScanSequence->CFSscanStruct.clientConnID,
	                                nameSpace,     //(BYTE)fileScanSequence->CFSscanStruct.NameSpace,
	                                0x00,                  //unused
	                                searchAttributes,                      //SEARCH ALL DIR 
	                                dirScanSequence->informationMask,                       //NWRETURN_ALL_INFO (0x2FFF) 
	                                dirScanSequence->CFSscanStruct.volumeNumber,
	                                dirScanSequence->CFSscanStruct.directoryNumber,
	                                dirScanSequence->CFSscanStruct.entryNumber,  //Is the entry number at which the search is to begin.  A value of -1 indicates that the search should begin at the start of the directory.  To continue a previous search, the value returned in NewSEntryNumber by a previous call to this function should be used for this parameter.
	                                dirScanSequence->CFSscanStruct.cfsPattern,     //This may include wildcard characters
	                                &(dirScanSequence->CFSscanStruct.entryNumber),
	                                (NetwareInfo *)dirInfo->information,        //(NetwareInfo *)fileInfo->information 
	                                &fileName          //caller provided buffer provided must be able to handle 256 bytes (1 byte length and 255 max bytes for the name)
	                                );
    if(ccode)
    {
		FLogError("GenNSContinueFileSearch", ccode, NULL);
		ccode = NWSMTS_SCAN_ERROR;
    }
    
    FEnd(ccode);
        return ccode;
}




#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    CFS_SCAN|FILESYS
#define FNAME   "FS_CFS_RScanNextFileEntry"
#define FPTR     FS_CFS_RScanNextFileEntry
CCODE FS_CFS_RScanNextFileEntry(FS_SCAN_SEQUENCE *fileScanSequence, FS_FILE_OR_DIR_INFO *fileInfo, UINT32 nameSpace)
{
    CCODE				ccode=0;
    BYTE				fileName[257];
    UINT32				searchAttributes=0x06;

    FStart();

    ccode = GenNSContinueFileSearch(
									fileScanSequence->CFSscanStruct.clientConnID,
									nameSpace,              //(BYTE)fileScanSequence->CFSscanStruct.NameSpace,
									0x00,                           //unused
									searchAttributes,                                       //Copied from Tsa600
									fileScanSequence->informationMask,                      //NWRETURN_ALL_INFO, // (0x2FFF) 
									fileScanSequence->CFSscanStruct.volumeNumber,
									fileScanSequence->CFSscanStruct.directoryNumber,
									fileScanSequence->CFSscanStruct.entryNumber,  //Is the entry number at which the search is to begin.  A value of -1 indicates that the search should begin at the start of the directory.  To continue a previous search, the value returned in NewSEntryNumber by a previous call to this function should be used for this parameter.
									fileScanSequence->CFSscanStruct.cfsPattern,    //This may include wildcard characters
									&(fileScanSequence->CFSscanStruct.entryNumber),
									(NetwareInfo *) fileInfo->information,      //(NetwareInfo *)fileInfo->information 
									&fileName                       //caller provided buffer provided must be able to handle 256 bytes (1 byte length and 255 max bytes for the name)
									);
    if(ccode)
    {
    	FLogError("GenNSContinueFileSearch", ccode, NULL);
        ccode = NWSMTS_SCAN_ERROR;
    }
  
   FEnd(ccode);
   return ccode;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    BACKUP|FILESYS
#define FNAME   "FS_CFS_PopulateSingleFile"
#define FPTR     FS_CFS_PopulateSingleFile
CCODE FS_CFS_PopulateSingleFile(unicode *currentDirFullName, FS_FILE_OR_DIR_HANDLE *fileOrDirHandle)
{
    CCODE      			ccode=0;
    char               	*path=NULL;
    UINT32 				pathLength=0;
    BUFFERPTR  			childPtr = NULL;
    BUFFERPTR 			ptr = NULL;
    NWHANDLE_PATH  		*pathInfo=NULL;

    FStart();

    pathInfo = (NWHANDLE_PATH *)tsaMalloc(sizeof(NWHANDLE_PATH));
    if(!pathInfo)
    {
        ccode=NWSMTS_OUT_OF_MEMORY;
        goto Return;
    }
    ccode = SMS_UnicodeToByte(currentDirFullName, &pathLength, &path, NULL);
    if(ccode) goto Return;

    childPtr = NWSMGetPathChild(0, path, &ptr);
    if(fileOrDirHandle->cfsStruct.uniFileDirName)
    {
        tsaFree(fileOrDirHandle->cfsStruct.uniFileDirName);
        fileOrDirHandle->cfsStruct.uniFileDirName=NULL;
    }
    ccode = SMS_ByteToUnicode(childPtr, &pathLength, &fileOrDirHandle->cfsStruct.uniFileDirName, NULL);
    if(ccode) goto Return;

    ptr = NWSMStripPathChild(fileOrDirHandle->cfsStruct.cfsNameSpace, path, NULL, NULL);

    ccode = FillHandleStruct((NWHANDLE_PATH *)pathInfo, path, fileOrDirHandle->cfsStruct.cfsNameSpace, fileOrDirHandle->cfsStruct.clientConnID);
    if(ccode)
    {
        ccode=NWSMTS_INVALID_PATH;
        goto Return;    
    }
    fileOrDirHandle->cfsStruct.directoryNumber = pathInfo->DirectoryBaseOrHandle;

Return:
    if(pathInfo)
        tsaFree(pathInfo);
    if(path)
        tsaFree(path);
    FEnd(ccode);
    return ccode;
}


