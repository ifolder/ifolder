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
 | $Modtime:   27 Mar 2002 17:04:16  $
 |
 | $Workfile:   fsOpen.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Initialise TSA
 +-------------------------------------------------------------------------*/

/* Netware headers */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#ifdef N_PLAT_NLM
#include <mpktypes.h>
#include <mpkapis.h>
#include <nwbitops.h>
#include <nwdos.h>
#include <filHandle.h>
#include "cfsdefines.h"
#include <Nwerrno.h>
#include <cdcommon.h>
#include <nit/nwextatt.h>
#endif

/* NSS Headers */
#include <zPublics.h>
#include <zParams.h>
#include <zError.h>

#ifdef N_PLAT_NLM
/* Legacy headers */
#include <lfsproto.h>
#include <fsproto.h>
#endif

#include <smsdefns.h>
#include <smstypes.h>

#include <fsinterface.h>
#include "smstserr.h"
#include <tsa_320.mlh>
#include "tsa_defs.h"
#include <tempfile.h>
#include <tsalib.h>
#include <incexc.h>
#include <tsaunicode.h>
#include <smsdebug.h>
#include <customdebug.h>
#include "tsaresources.h"

/* Local function prototypes */
void DisplayBitMap(void *bitMap, long size);

#define MAX_FILE_NAME_LENGTH						1024
#ifdef N_PLAT_NLM
extern BOOL NDSBackupServerDataSupported;
#endif
extern UINT32 TSA_TGID;
extern unsigned int tsazInfoVersion;

#ifdef N_PLAT_GNU
/* For build deps */
typedef struct compressFileHeader_s {

	BYTE	CompressCode;
	BYTE	CompressMajorVersion;
	BYTE	CompressMinorVersion;
	BYTE	Unused;
	LONG	OriginalFileLength;
	LONG	CompressedFileLength;
	LONG	DataTreeNodes;

	LONG	LengthTreeNodes;
	LONG	OffsetTreeNodes;
	LONG	NumberOfHoles;
	LONG	HeaderCheckSum;

} compressFileHeader_t, *compressFileHeader_tp, **compressFileHeader_tpp;
#endif

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_READ|FILESYS
#define FNAME   "FS_Open"
#define FPTR     FS_Open
CCODE FS_Open(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, FS_ACCESS_RIGHTS accessRights, 
			UINT32 openModeFlags, FS_FILE_OR_DIR_INFO *info, BOOL noLock, UINT32 searchAttributes, FS_HL_TABLE ** hlTable)
{
	CCODE			status;
	unicode_t		*unicodeFileName = NULL;
	UINT32			count;
	UINT32			dsCount;
	UINT32			retByteLength;
	FS_HANDLE		tmpHandle;
	FS_HANDLE		eaHandle;
	BOOL			moreAttempts = TRUE;
	UINT32			attributes = 0;
	unsigned char	*tmpNamePtr;
	zInfo_s			*retInfo=NULL;
	UINT32			 infoSize=0;
	FStart();
	
	/**	If either the fileOrDirHandle is NULL, or info is NULL, or if the flags 
		have any unsupported bit set, then there is an error invalid parameter **/
	
	if((fileOrDirHandle == NULL) ||(info == NULL) || !(openModeFlags & FS_PRIMARY_DATA_STREAM))
	{
		status = FS_INTERNAL_ERROR;
		goto Return;
	}

	if(tsazInfoVersion == zINFO_VERSION_A)
		infoSize = sizeof(zInfo_s);
	else
		infoSize = sizeof(zInfoB_s);
	
	retInfo = tsaMalloc(infoSize);
	if(!infoSize)
	{
		status = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}

	attributes = FS_GetFileAttributes(info->information);
	if (attributes & NWFA_REMOTE_DATA_ACCESS)
	{
		if(!info->isDirectory)
		{
			fileOrDirHandle->migratedFileMutex = tsaKMutexAlloc("TSA_STREAM_ACCESS_LOCK");
			if (!fileOrDirHandle->migratedFileMutex)
			{
				status = NWSMTS_INTERNAL_ERROR;
				goto Return;
			}
		}
	}

	if(info->isDirectory || info->isVolume)
		accessRights &= ~zRR_DENY_WRITE;
	
	/**	For primary data stream, we would need the parent handle and the zOpen call will open it as default **/
	/**	If the object to be open is a directory, then data stream check is redundant **/
	if((openModeFlags & FS_PRIMARY_DATA_STREAM) || (info->isDirectory))
	{
		while(moreAttempts)
		{
			status = zOpen(
							fileOrDirHandle->parentHandle.nssHandle,
							fileOrDirHandle->taskID,
							fileOrDirHandle->nameSpace,
							fileOrDirHandle->uniPath,
							accessRights,
							&tmpHandle.nssHandle
						);
			if(status== zERR_LINK_IN_PATH)
			{
				status = zOpen(
							fileOrDirHandle->parentHandle.nssHandle,
							fileOrDirHandle->taskID,
							fileOrDirHandle->nameSpace|zMODE_LINK,
							fileOrDirHandle->uniPath,
							accessRights|SEARCH_LINK_AWARE|SEARCH_OPERATE_ON_LINK,
							&tmpHandle.nssHandle);
			}

			if ((status == zERR_CANT_DENY_WRITE_LOCK) && noLock)
				accessRights &= ~(zRR_DENY_WRITE);
			else
				moreAttempts = FALSE;
		}
		if(status == 0)
		{
			/**	Store a copy in the openHandle array, mostly for consistency sake, the first element is the promary data stream handle **/
			fileOrDirHandle->handleArray[0].handle.nssHandle = tmpHandle.nssHandle;
			status = zGetInfo(
                           fileOrDirHandle->handleArray[0].handle.nssHandle,
                           zGET_DATA_STREAM_INFO,
                           infoSize,
                           tsazInfoVersion,
                           retInfo
                           );
			if(!status)
			{
				if(fileOrDirHandle->handleArray[0].type & DATASET_IS_COMPRESSED)
					fileOrDirHandle->handleArray[0].UnCompressedSize = FS_GetFileStreamSize(retInfo);
				else
					fileOrDirHandle->handleArray[0].size = FS_GetFileStreamSize(retInfo);
			}
			else
			{
				status = NWSMTS_OPEN_DATA_STREAM_ERR;
				goto Return;
			}
		}
		else
		{
		    FLogError("zOpen", status, NULL);			
		    if(status == zERR_FILE_WRITE_LOCKED || status == zERR_FILE_READ_LOCKED || status == zERR_CANT_DENY_READ_LOCK || status == zERR_CANT_DENY_WRITE_LOCK || status == zERR_LAST_STATE_UNKNOWN)
				status = NWSMTS_DATA_SET_IN_USE;
			else
				status = NWSMTS_OPEN_DATA_STREAM_ERR;
			goto Return;
		}
	}

	/**	For all other data streams or extended attributes, we need a NSS handle to that file, using which we search for other data streams and EA, and open if present **/
	if(openModeFlags & (FS_DATA_STREAM | FS_EXTENDED_ATTRIBUTES))
	{
		if (!(tmpHandle.nssHandle != 0))
		{
			/**	Opening a data stream or EA requires the handle to the file's primary data stream in NSS3.0 **/		
			status = NWSMTS_INTERNAL_ERROR;
			goto Return;	
		}

		/**	If data streams was requested for a non-directory, search for all of them and open if found **/
		if ((!fileOrDirHandle->isDirectory) && (openModeFlags & FS_DATA_STREAM))
		{
			/**	The first data stream is already stored as the first element of the array, as it is the primary data stream. So start from the second position in the array **/
			for(count = 1; count < fileOrDirHandle->streamCount; count++)
			{
				memset(retInfo, 0, infoSize);
				status = zWildRead	(
								tmpHandle.nssHandle,
								zPFMT_UNICODE,
								zNTYPE_DATA_STREAM,											/**	Search for data stream type objects **/
								NULL,
								zMATCH_ALL,													/**	Search only for all data streams **/
								zGET_ALL_NAMES|zGET_STORAGE_USED,												/**	Get all names of the data stream **/
								sizeof(zInfo_s),
								zINFO_VERSION_A,
								retInfo
								);
				if(status == 0)
				{
					unicodeFileName = zInfoGetFileName(retInfo, (NINT)zNSPACE_DATA_STREAM);
					status = zOpen(
								tmpHandle.nssHandle,
								fileOrDirHandle->taskID,
								zNSPACE_DATA_STREAM,
								unicodeFileName,
								accessRights & ~(FS_ENABLE_IO_ON_COMPRESSION),
								&fileOrDirHandle->handleArray[count].handle.nssHandle
								);
					if(status)
					{
		                FLogError("zOpen", status, NULL);						
		                if(status == zERR_FILE_WRITE_LOCKED || status == zERR_FILE_READ_LOCKED || status == zERR_CANT_DENY_READ_LOCK || status == zERR_CANT_DENY_WRITE_LOCK || status == zERR_LAST_STATE_UNKNOWN)
							status = NWSMTS_DATA_SET_IN_USE;
						else
							status = NWSMTS_OPEN_DATA_STREAM_ERR;
						goto Return;
					}
					fileOrDirHandle->handleArray[count].size = FS_GetFileStreamSize(retInfo);
					fileOrDirHandle->handleArray[count].PhysicalSize = FS_GetFilePhysicalStreamSize(retInfo);
					/* Store name into handle */
					retByteLength = FS_MAX_FULL_NAME;
					tmpNamePtr = fileOrDirHandle->handleArray[count].streamName;
					status = SMS_UnicodeToByte(unicodeFileName, &retByteLength, &tmpNamePtr, NULL);
					if (status || (retByteLength >= FS_MAX_FULL_NAME))
					{
						fileOrDirHandle->handleArray[count].streamName[0] = '\0';
						status = 0;
					}
				}
				/**	In case zWildRead fails to find this data stream, we have an error as all the data streams returned by zScan should be opened **/
				else
				{
					FLogError("zWildRead",status,NULL);					
					if(status == zERR_FILE_WRITE_LOCKED || status == zERR_FILE_READ_LOCKED || status == zERR_CANT_DENY_READ_LOCK || status == zERR_CANT_DENY_WRITE_LOCK || status == zERR_LAST_STATE_UNKNOWN)
						status = NWSMTS_DATA_SET_IN_USE;
					else
						status = NWSMTS_OPEN_DATA_STREAM_ERR;
					goto Return;
				}
			}
		}


		/**	If Extended Attributes for this file/directory was requested, search for them and open if found **/
		if (openModeFlags & FS_EXTENDED_ATTRIBUTES)
		{
			dsCount = fileOrDirHandle->streamCount;
			/* open a new file handle for EA's as the zWildRead expects a new one */
			if (fileOrDirHandle->eaCount)
			{
				status = zOpen(
							fileOrDirHandle->parentHandle.nssHandle,
							fileOrDirHandle->taskID,
							fileOrDirHandle->nameSpace,
							fileOrDirHandle->uniPath,
							accessRights  & ~(FS_ENABLE_IO_ON_COMPRESSION),
							&eaHandle.nssHandle
							);
				if(status)
				{
					fileOrDirHandle->eaCount = 0;
					FLogError("zOpen",status,NULL);					
					if(status == zERR_FILE_WRITE_LOCKED || status == zERR_FILE_READ_LOCKED || status == zERR_CANT_DENY_READ_LOCK || status == zERR_CANT_DENY_WRITE_LOCK || status == zERR_LAST_STATE_UNKNOWN)
						status = NWSMTS_DATA_SET_IN_USE;
					else
						status = NWSMTS_OPEN_DATA_STREAM_ERR;
					goto Return;
				}
			}
			
			for(count = dsCount; count < (dsCount + fileOrDirHandle->eaCount); count++)
			{
				memset(retInfo, 0, infoSize);
				status = zWildRead	(
								eaHandle.nssHandle,
								zPFMT_UNICODE,
								zNTYPE_EXTENDED_ATTRIBUTE,									/**	Search for extended attributes type objects **/
								NULL,
								zMATCH_ALL,													/**	Search only for all data streams **/
								zGET_ALL_NAMES | zGET_EXTATTR_FLAGS,												/**	Get all names of the data stream **/
								sizeof(zInfo_s),
								zINFO_VERSION_A,
								retInfo
								);
				if(status == 0)
				{
					unicodeFileName = zInfoGetFileName(retInfo, (NINT)zNSPACE_EXTENDED_ATTRIBUTE);
					status = zOpen(
								eaHandle.nssHandle,
								fileOrDirHandle->taskID,
								zNSPACE_EXTENDED_ATTRIBUTE,
								unicodeFileName,
								accessRights & ~(FS_ENABLE_IO_ON_COMPRESSION), 
								&fileOrDirHandle->handleArray[count].handle.nssHandle
								);
					if(status)
					{
						fileOrDirHandle->eaCount = count - dsCount;
						zClose(eaHandle.nssHandle);
						FLogError("zOpen",status,NULL);						
						if(status == zERR_FILE_WRITE_LOCKED || status == zERR_FILE_READ_LOCKED || status == zERR_CANT_DENY_READ_LOCK || status == zERR_CANT_DENY_WRITE_LOCK || status == zERR_LAST_STATE_UNKNOWN)
							status = NWSMTS_DATA_SET_IN_USE;
						else
							status = NWSMTS_OPEN_DATA_STREAM_ERR;
						goto Return;
					}
					memset(fileOrDirHandle->handleArray[count].eaName, 0, sizeof(fileOrDirHandle->handleArray[count].eaName));
					retByteLength = FS_MAX_FULL_NAME;
					tmpNamePtr = fileOrDirHandle->handleArray[count].eaName;
					status = SMS_UnicodeToByte(unicodeFileName, &retByteLength, &tmpNamePtr, NULL);
					if (status || (retByteLength >= FS_MAX_FULL_NAME))
					{
						fileOrDirHandle->handleArray[count].eaName[0] = '\0';
						status = 0;
					}
					fileOrDirHandle->handleArray[count].eaAttrFlags = FS_GetExtendedAttributeUserFlags(retInfo);
					fileOrDirHandle->handleArray[count].size = FS_GetFileStreamSize(retInfo);
				}
				/**	In case zWildRead fails to find this EA, we have an error as all the EAs returned by zScan should be opened **/
				else
				{
					fileOrDirHandle->eaCount = count - dsCount;
					zClose(eaHandle.nssHandle);
					FLogError("zWildRead",status,NULL);
					goto Return;
				}
			}
			if(fileOrDirHandle->eaCount)
			zClose(eaHandle.nssHandle);
		}

	}

Return:
	if(status != 0)
	{
		/**	Close all the open handles in case of any error **/
		FS_Close(fileOrDirHandle, DATASET_CLOSE_ALL_STREAM_HANDLES);
	}
	if(retInfo)
		tsaFree(retInfo);
	
	FEnd(status);
	return status;
}

#ifdef N_PLAT_NLM
/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_READ|FILESYS
#define FNAME   "FS_Populate_SSI_Files"
#define FPTR     FS_Populate_SSI_Files
CCODE FS_Populate_SSI_Files(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle,UINT32 filesysType)
{
	UINT8		*VolfileName=NULL;
	UINT8		*fileName;
	CCODE		cCode=0; 
	int			fileIndex;
	int 		i=0;
	Key_t		tHandle = 0, tmpKey = 0;
	FS_HANDLE	legHandle;
	zInfo_s 	retGetInfo = {0};
	zInfo_s		rGetInfo = {0};
	Key_t		fileKey = 0;
	NINT		bytRead;
	UINT32		sz,curOff,curPos,stdSz = 32768;
	NINT		retBytesWritten;
	char		sfile1[100],sfile2[100];
	unsigned char	*buf1 = NULL;
	UINT32		tempHandle=NULL;
	UINT32		retDirectoryNumber=0;  // ret val not used
	void         *retDirectoryEntry=NULL;// ret val not used
	UINT32		priStreamSize = 0;
	UINT32		namespaceDirectoryBase=0;
	UINT32 		dosDirectoryBase=0;
	UINT8		volumeNumber=0;
	char 	*fName = NULL;
	NWHANDLE_PATH     	pathInfo={0};
	
	FStart();

	fileOrDirHandle->streamCount = NUMBER_OF_SERV_SPCFC_FILES;
	fileOrDirHandle->SSIFile1=NULL;
	fileOrDirHandle->SSIFile2=NULL;

	fileOrDirHandle->SSIFile1=(UINT8 *)tsaMalloc(MAX_FILE_NAME_LENGTH);
	if(fileOrDirHandle->SSIFile1==NULL)
	{
		return NWSMTS_OUT_OF_MEMORY;
	}

	fileOrDirHandle->SSIFile2=(UINT8 *)tsaMalloc(MAX_FILE_NAME_LENGTH);
	if(fileOrDirHandle->SSIFile2==NULL)
	{
		return NWSMTS_OUT_OF_MEMORY;
	}
	
	if (!NDSBackupServerDataSupported)
	{
		FTrack(TSAREAD, DC_CRITICAL|DC_VERBOSE, "NDSBackupServerData doesn't exist!\n");
	}

	cCode=InvokeNDSBackupServerData(TRUE,&fName);
    if(cCode)
    {
        FLogError("InvokeNDSBackupServerData", cCode, NULL);
    	goto Return;
    }	

	for (fileIndex = 0; fileIndex < NUMBER_OF_SERV_SPCFC_FILES; fileIndex++)
	{
		switch (fileIndex)
		{
		case SERV_DATA_FILE_INDEX:
			if (fName == NULL)
				fileName = _GetMessage(SERVINFO_PATH_STR);
			else
				fileName=fName;
			fileOrDirHandle->handleArray[i].ServerSpecificFileName = _GetMessage(SERVINFO_FILE_STR);
		break;

		case DS_MISC_FILE_INDEX:
			fileName = _GetMessage(DSMISC_PATH_STR);
			fileOrDirHandle->handleArray[i].ServerSpecificFileName = _GetMessage(DSMISC_FILE_STR);
		break;

		case VOLUME_INFO_FILE_INDEX:
			VolfileName = (UINT8 *)tsaMalloc(256);
			if(VolfileName == NULL)
			{
				cCode = NWSMTS_OUT_OF_MEMORY;
				goto Return;
			}
			
			cCode = BuildVolumeInfoFile(&VolfileName);
	    	if(cCode)
	    	{
	            FLogError("BuildVolumeInfoFile", cCode, NULL);
	    		goto Return;
	    	} 
	    	fileName = VolfileName; 
	    	strcpy(fileOrDirHandle->SSIFile1,VolfileName);
	    	fileOrDirHandle->handleArray[i].ServerSpecificFileName =_GetMessage(VOLUME_INFO_FILE_STRING);
	    break;
		
		case AUTOEXEC_NCF_FILE_INDEX:
			fileName = _GetMessage(AUTOEXEC_NCF_PATH_STRING);
			fileOrDirHandle->handleArray[i].ServerSpecificFileName =_GetMessage(AUTOEXEC_NCF_FILE_STRING);
		break;

		case STARTUP_NCF_FILE_INDEX:
			fileName = _GetMessage(STARTUP_NCF_PATH_STRING);
			strcpy(fileOrDirHandle->SSIFile2, fileName);
	    	cCode = DOSCopy(fileName, _GetMessage(STARTUP_NCF_DOS_PATH));
	    	if(cCode)
	    	{
				strcpy(sfile1,"DOSFAT_C:NWSERVER/STARTUP.NCF");
				strcpy(sfile2,"SYS:SYSTEM/TSA/STARTUP.NCF");
	    		cCode = zOpen(fileOrDirHandle->parentHandle.nssHandle, 
							fileOrDirHandle->taskID, 
							zNSPACE_LONG|zMODE_UTF8, 
							sfile1, 
							zRR_SCAN_ACCESS | zRR_READ_ACCESS | zRR_DONT_UPDATE_ACCESS_TIME, 
							&tHandle);
	    		if(cCode)
	    		{
	    			fileOrDirHandle->streamCount--;
	    			cCode = 0;
	    			goto Return;
	    		}
				
				cCode = zGetInfo(tHandle, zGET_STD_INFO, sizeof(rGetInfo), zINFO_VERSION_A, &rGetInfo);
				if(cCode)
				{
					fileOrDirHandle->streamCount--;
					cCode = 0;
					goto Return;
				}
				sz = rGetInfo.std.logicalEOF;

				buf1 = tsaMalloc(stdSz);
				if (buf1 == NULL)
				{
					cCode=NWSMTS_OUT_OF_MEMORY;
					fileOrDirHandle->streamCount--;
					goto Return;
				}
				
	    		if(filesysType == NETWAREPSSFILESYSTEM)
	    		{
	    			cCode = zCreate(fileOrDirHandle->parentHandle.nssHandle, 
	    					fileOrDirHandle->taskID, 
	    					zNILXID,
	    					zNSPACE_LONG | zMODE_UTF8,
	    					sfile2, 
							zFILE_REGULAR,
							0, 
							zCREATE_OPEN_IF_THERE,
							zRR_READ_ACCESS | zRR_WRITE_ACCESS | zRR_SCAN_ACCESS,
							&fileKey);
	    			if(cCode)
	    			{
	    				fileOrDirHandle->streamCount--;
	    				cCode = 0;
	    				goto Return;
	    			}

					curOff = 0;
					curPos =0;
					while(sz)
					{
						cCode = zRead(tHandle, zNILXID, curOff, stdSz, buf1, &bytRead);
						if (cCode)
						{
							fileOrDirHandle->streamCount--;
							cCode = 0;
							goto Return;
						}
						sz -= bytRead;
						curOff += bytRead;

						cCode = zWrite(fileKey, zNILXID, curPos,bytRead , buf1, &retBytesWritten);
						if(cCode)
						{
							fileOrDirHandle->streamCount--;
							cCode = 0;
							goto Return;
						}
						curPos += retBytesWritten;
					}
	    		}
	    		else
	    		{	
					/*Now we need to call FS_CFS_CreateAndOpen to create and open a file
					in the SYS volume which is a legacy volume now*/
					fileOrDirHandle->cfsStruct.cfsNameSpace = 4;

					if(fileOrDirHandle->uniPath)
					{
						tsaFree(fileOrDirHandle->uniPath);
						fileOrDirHandle->uniPath=NULL;
					}
			    	fileOrDirHandle->uniPath = (punicode) tsaMalloc((strlen(sfile2) + 1 ));
					if(!fileOrDirHandle->uniPath) 
					{
			    	    cCode = NWSMTS_OUT_OF_MEMORY;
			        	goto Return;
					}
					strcpy((char*)fileOrDirHandle->uniPath, sfile2);
					cCode = FS_CFS_CreateAndOpen(fileOrDirHandle, 0, FS_CFS_WRITE_ACCESS, 0, FS_PRIMARY_DATA_STREAM, 
							&legHandle, (void*)sfile2, 0, TRUE, NULL, NULL);
					if(cCode)
					{
						fileOrDirHandle->streamCount--;
						cCode = 0;
						goto Return;
					}
					if(fileOrDirHandle->uniPath)
					{
						tsaFree(fileOrDirHandle->uniPath);
						fileOrDirHandle->uniPath=NULL;
					}					

					curOff = 0;
					curPos =0;
					while(sz)
					{
						cCode = zRead(tHandle, zNILXID, curOff, stdSz, buf1, &bytRead);
						if (cCode)
						{
							fileOrDirHandle->streamCount--;
							cCode = 0;
							goto Return;
						}
						sz -= bytRead;
						curOff += bytRead;

						cCode=WriteFile(fileOrDirHandle->cfsStruct.clientConnID, legHandle.cfsHandle,
								curPos, bytRead, buf1);
						if(cCode)
						{
							fileOrDirHandle->streamCount--;
							cCode = 0;
							goto Return;
					
						}
					}

					cCode=CloseFile(fileOrDirHandle->cfsStruct.clientConnID, ~(fileOrDirHandle->cfsStruct.clientConnID), legHandle.cfsHandle);
					if(cCode)
					{
						fileOrDirHandle->streamCount--;
						cCode = 0;
				   		goto Return;
					}
	    		}
	    	}

	    	fileOrDirHandle->handleArray[i].ServerSpecificFileName =_GetMessage(STARTUP_NCF_FILE_STRING);
		break;
		default:
			cCode = NWSMTS_INTERNAL_ERROR;
			goto Return;
		}
		
		if(filesysType==NETWAREPSSFILESYSTEM)
		{

			cCode = zOpen(fileOrDirHandle->parentHandle.nssHandle,
							fileOrDirHandle->taskID,
							fileOrDirHandle->nameSpace|zMODE_UTF8 ,
							fileName,
							zRR_SCAN_ACCESS | zRR_READ_ACCESS | zRR_DONT_UPDATE_ACCESS_TIME,
							&tmpKey);
			fileOrDirHandle->handleArray[i].handle.nssHandle = tmpKey;
			if(cCode)
			{	
				if (fileIndex == SERV_DATA_FILE_INDEX)
				{
		            FLogError("zOpen", cCode, NULL);
					cCode = NWSMTS_OPEN_ERROR;
					goto Return;
				}
				else
				{
					FTrack1(BACKUP,DC_CRITICAL, "fileIndex = %d\n", fileIndex) ;
					cCode = 0;
					fileOrDirHandle->streamCount--;
					continue;
				}
			}

			cCode = zGetInfo(tmpKey, 
							zGET_STD_INFO, 
							sizeof(retGetInfo), 
							zINFO_VERSION_A, 
							&retGetInfo);
			if(cCode)
			{
				if (fileIndex == SERV_DATA_FILE_INDEX)
				{
					cCode = NWSMTS_OPEN_ERROR;
					goto Return;
				}
				else
				{
					FTrack1(BACKUP,DC_CRITICAL, "fileIndex = %d\n", fileIndex) ;
					cCode = 0;
					fileOrDirHandle->streamCount--;
					continue;
				}
			}
		
			fileOrDirHandle->handleArray[i].size = retGetInfo.std.logicalEOF;
		}
		else
		{
			cCode = FillHandleStruct( (NWHANDLE_PATH *) &pathInfo, fileName, fileOrDirHandle->nameSpace, fileOrDirHandle->cfsStruct.clientConnID);
			if(cCode)
			{
				if (fileIndex == SERV_DATA_FILE_INDEX)
				{
					cCode = NWSMTS_INVALID_PATH;
					goto Return;
				}
				else
				{
					cCode = 0;
					fileOrDirHandle->streamCount--;
					continue;
				}
			}
			cCode = GenNSGetDirBase(
						  fileOrDirHandle->cfsStruct.clientConnID,
						  &(pathInfo),
						  (UINT8)fileOrDirHandle->nameSpace, //DOSNameSpace
						  0/*searchAttributes*/,
						  &namespaceDirectoryBase,
						  &dosDirectoryBase,
						  &volumeNumber, 
						  (UINT8)fileOrDirHandle->nameSpace
					      );
			if(cCode)
			{
				if (fileIndex == SERV_DATA_FILE_INDEX)
				{
					goto Return;
				}
				else
				{
					cCode = 0;
					fileOrDirHandle->streamCount--;
					continue;
				}
			}
			//pathBase=dosDirectoryBase;

			cCode = OpenFile(
					fileOrDirHandle->cfsStruct.clientConnID,// this needs to replaced by clientConnID
					~(fileOrDirHandle->cfsStruct.clientConnID),
					fileOrDirHandle->cfsStruct.volumeNumber,
					dosDirectoryBase,
					NULL,		   		 //pathstring 
					0,			   		 //pathcount
					fileOrDirHandle->nameSpace,
					0/*searchAttributes*/,    
					FS_CFS_READ_ACCESS  /*accessRights*/,  
					0/*(BYTE)count*/,		
					&tempHandle,
					&retDirectoryNumber,  // ??? this is not used 
					&retDirectoryEntry     // ??? this is not used 
	       		);
			if(cCode)
			{
				if (fileIndex == SERV_DATA_FILE_INDEX)
				{
					cCode = NWSMTS_OPEN_ERROR;
					goto Return;
				}
				else
				{
					FTrack1(BACKUP,DC_CRITICAL, "fileIndex = %d\n", fileIndex) ;
					cCode = 0;
					fileOrDirHandle->streamCount--;
					continue;
				}
			}
			fileOrDirHandle->handleArray[i].handle.cfsHandle = tempHandle;

			cCode=GetFileSize(fileOrDirHandle->cfsStruct.clientConnID, tempHandle, &priStreamSize);
			if(cCode)
			{
			    FLogError("GetFileSize", cCode, 0);
				goto Return;
			}
			fileOrDirHandle->handleArray[i].size=priStreamSize;
		}
		i++;
	}

Return:
	if(cCode)
	{
		if(fileOrDirHandle->SSIFile1)
		{
			tsaFree(fileOrDirHandle->SSIFile1);
			fileOrDirHandle->SSIFile1=NULL;
		}
		if(fileOrDirHandle->SSIFile2)
		{
			tsaFree(fileOrDirHandle->SSIFile2);
			fileOrDirHandle->SSIFile2=NULL;
		}
	}
	if (tHandle)
		zClose(tHandle);

	if (fileKey)
		zClose(fileKey);

	if (buf1)
		tsaFree(buf1);
	
	if(VolfileName)
		tsaFree(VolfileName);
	if(fName)
		tsaFree(fName);

	FEnd(cCode);
	return cCode;
}

#endif


/*************************************************************************************************************************************/

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_READ|FILESYS
#define FNAME   "FS_Close"
#define FPTR     FS_Close
CCODE FS_Close(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 closeFlag)
{
	STATUS		status = 0; 
	UINT32		count;
	Key_t		*closeKey;
	FStart();

	/**	Close all the data Streams handles in case of a file **/
	for(count = 0; count < (fileOrDirHandle->streamCount + fileOrDirHandle->eaCount); count++)
	{
		if(closeFlag)
		{
			if(!(closeFlag & DATASET_CLOSE_ALL_NON_PRI_STREAM_HANDLES) &&
			  !(closeFlag & DATASET_CLOSE_ALL_EA_STREAM_HANDLES))
				continue;
			/*if( ((closeFlag & DATASET_CLOSE_ALL_NON_PRI_STREAM_HANDLES)
				&& !(fileOrDirHandle->handleArray[count].type == DATASET_IS_SECONDARY_DATASTREAM))
										&&
   			   ((closeFlag & DATASET_CLOSE_ALL_EA_STREAM_HANDLES)
				&& (!(fileOrDirHandle->handleArray[count].type == DATASET_IS_EXTENDED_ATTRIBUTE))))
				continue;	
			*/
			if(((closeFlag & DATASET_CLOSE_ALL_NON_PRI_STREAM_HANDLES) || (closeFlag & DATASET_CLOSE_ALL_EA_STREAM_HANDLES)) && (
				(fileOrDirHandle->handleArray[count].type != DATASET_IS_SECONDARY_DATASTREAM) && (fileOrDirHandle->handleArray[count].type != DATASET_IS_EXTENDED_ATTRIBUTE))) 
				continue;
		}
		
		if (fileOrDirHandle->handleArray[count].streamMap)	{
			tsaFree(fileOrDirHandle->handleArray[count].streamMap);
			fileOrDirHandle->handleArray[count].streamMap = NULL;
		}
		if(closeKey = &(fileOrDirHandle->handleArray[count].handle.nssHandle))
		{
			status = zClose(*closeKey);
			if(status)
			{
			    FLogError("zClose", status, 0);
				goto Return;
			}
			else
			{
				*closeKey = 0;	/**	Ensure that the dataStreamsOpenHandle array contains a 0 after the handle was closed **/
			}
		}
	}

Return:
	FEnd((CCODE)status);
	return (CCODE)status;
}


/*************************************************************************************************************************************/
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_READ|FILESYS
#define FNAME   "FS_GetInfo"
#define FPTR     FS_GetInfo

CCODE FS_GetInfo(FS_FILE_OR_DIR_HANDLE *handle, UINT32 getInfoMask, UINT32 bufferSize, void *buffer)
{
	STATUS status;

	status = zGetInfo(handle->handleArray[0].handle.nssHandle, getInfoMask, bufferSize, tsazInfoVersion, (zInfo_s *)buffer);
	if(status)
	{
	    FLogError("zGetInfo", status, NULL);
	}
	return (CCODE)status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_READ|FILESYS
#define FNAME   "FS_GetNameSpaceEntryName"
#define FPTR     FS_GetNameSpaceEntryName

CCODE FS_GetNameSpaceEntryName(UINT32 connID, FS_HANDLE handle, unicode *path, 
			unsigned long  nameSpaceType, unsigned long  nameSpace, unicode **newName, 
			UINT32 taskID, UINT32 *attributes)
{
    Key_t 		fileKey;
	STATUS 		status = 0;
	UINT32 		requestedRights = 0;
	zInfo_s 	*retGetInfo = NULL;
	int 		retGetInfoSz;
	unicode_t 	*uniName = NULL;  
	punicode 	uniPathName = NULL;
   	punicode 	uniVolName = NULL;
    unicode 	*tmpPathPtr = NULL;
    Key_t 		tmpVolKey = 0;
    Key_t 		tmpDirKey = 0;
    FStart();
    
	if ((nameSpace == zNSPACE_MAC) || (nameSpace == zNSPACE_UNIX))	
	{
		tmpPathPtr = path;
		if(GetPrimaryUniResource(&tmpPathPtr, nameSpace, &uniVolName)==NULL)
		{
			status = FS_INTERNAL_ERROR;
			goto Exit;
		}
		
		if(!tmpPathPtr)
		{
			status = FS_INTERNAL_ERROR;
			goto Exit;
		}

		status = zOpen(
				handle.nssHandle,	
				taskID, 									
				zNSPACE_LONG,						
				uniVolName,							
				zRR_SCAN_ACCESS,	
				&tmpVolKey);
		if(status)
		{	
			tmpVolKey = 0;
			FLogError("zOpen",status,NULL);
			goto Exit;
		}
		
		if(tmpPathPtr)
		{
			tmpDirKey = handle.nssHandle;
			handle.nssHandle = tmpVolKey;
			uniPathName = tmpPathPtr;
		}
	}
	else
		uniPathName = path;
	
	requestedRights = zRR_SCAN_ACCESS;
	status = zOpen(
				handle.nssHandle,
				taskID/*zNSS_TASK*/,
				nameSpace,
				uniPathName,
				requestedRights, 
				&fileKey);
	if(status == zERR_LINK_IN_PATH)
	{
		status = zOpen(
				handle.nssHandle,
				taskID/*zNSS_TASK*/,
				nameSpace|zMODE_LINK,
				uniPathName,
				requestedRights|SEARCH_LINK_AWARE|SEARCH_OPERATE_ON_LINK, 
				&fileKey);
	}
	if( status != zOK )
	{
	    FLogError("zOpen", status, NULL);
		goto Exit;
	}
	retGetInfoSz = sizeof(zInfo_s);
	while (TRUE)
	{
		retGetInfo = (zInfo_s *)tsaMalloc(retGetInfoSz);
		if (retGetInfo == NULL)
		{
			status = NWSMTS_OUT_OF_MEMORY;
			goto Exit1;
		}
		status = zGetInfo(fileKey, zGET_ALL_NAMES | zGET_STD_INFO, retGetInfoSz, zINFO_VERSION_A, retGetInfo);
		if(status == zOK)
		{
			*attributes = retGetInfo->std.fileAttributes;
			uniName = zInfoGetFileName(retGetInfo, nameSpaceType);
			break;
		}
		else if (status == zERR_BUFFER_TOO_SMALL)
		{
			tsaFree(retGetInfo);
			retGetInfo = NULL;
			retGetInfoSz += zMAX_COMPONENT_NAME * 2;
			continue;
		}
		else
		{
			FLogError("zGetInfo",status,NULL);
			goto Exit1;
		}
	}
	
	*newName = (unicode *)tsaMalloc((unilen(uniName) + 1)* sizeof(unicode));
	if (!*newName)
	{
		status = NWSMTS_OUT_OF_MEMORY;
		goto Exit1;
	}
	unicpy(*newName, uniName);
	
Exit1:
	zClose(fileKey);
	if (retGetInfo)
		tsaFree(retGetInfo);
Exit:
	if(tmpVolKey)
	{
		zClose(tmpVolKey);
		handle.nssHandle = tmpDirKey;
	}

	if (uniVolName)
		tsaFree(uniVolName);
	
	if(status)
	{
		if((status==zERR_NAME_NOT_FOUND_IN_DIRECTORY) || (status==zERR_VOLUME_NOT_FOUND))
			status=NWSMTS_DATA_SET_NOT_FOUND;
		else
			status=NWSMTS_INVALID_PATH;
	}
	
	FEnd((CCODE)status);
	return((CCODE)status);
} 

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_READ 
#define FNAME   "FS_ConvertOpenMode"
#define FPTR     FS_ConvertOpenMode
CCODE FS_ConvertOpenMode(FS_FILE_OR_DIR_HANDLE *handle,UINT32 inputMode, UINT32 scanMode, BOOL COWEnabled, UINT32 scanType,
			FS_FILE_OR_DIR_INFO *fileInfo, FS_ACCESS_RIGHTS *outputMode, BOOL *noLock, UINT32 *openMode,BOOL FileIsMigrated)
{
    zInfo_s *temp=NULL;
    UINT32 dataStreamAttributes, count;

	*outputMode = 0;
	temp=(zInfo_s *)(fileInfo->information);
	
	*outputMode |= zRR_SCAN_ACCESS | zRR_READ_ACCESS | zRR_DONT_UPDATE_ACCESS_TIME;

	if ((inputMode & NWSM_OPEN_MODE_MASK) == NWSM_OPEN_READ_DENY_WRITE)
	{
		*outputMode |= zRR_DENY_WRITE;
		noLock = FALSE;
	}
	else if ((inputMode & NWSM_OPEN_MODE_MASK) == NWSM_USE_LOCK_MODE_IF_DW_FAILS)
	{
		*outputMode |= zRR_DENY_WRITE;
		*noLock = TRUE;
	}
	else if ((inputMode & NWSM_OPEN_MODE_MASK) == NWSM_NO_LOCK_NO_PROTECTION)		
	{
		*outputMode |= zRR_DENY_WRITE;
		*noLock = TRUE;
	}
	else if ((inputMode & NWSM_OPEN_MODE_MASK) == NWSM_OPEN_READ_ONLY)		
	{
		if(temp->std.fileAttributes & FS_DATA_STREAM_IS_COMPRESSED)
		{
			if(!(scanType & NWSM_EXPAND_COMPRESSED_DATA)
							&& !(inputMode & NWSM_EXPAND_COMPRESSED_DATA_SET))
				{
					*outputMode |= zRR_DENY_WRITE;
					*noLock = TRUE;
				}
		}	
	}

	if (COWEnabled)
		*outputMode |= zRR_READ_ACCESS_TO_SNAPSHOT;

	if(FileIsMigrated)
		*outputMode &= ~(zRR_DENY_WRITE);

	if(!fileInfo->isDirectory)
	{
		for(count=0;count<handle->streamCount;count++)
		{
			if(count==0)
				dataStreamAttributes=temp->std.fileAttributes;
			else
				dataStreamAttributes=0;
			if(dataStreamAttributes & FS_DATA_STREAM_IS_COMPRESSED)
			{
		 		if(!(scanType & NWSM_EXPAND_COMPRESSED_DATA)
							&& !(inputMode & NWSM_EXPAND_COMPRESSED_DATA_SET))
				{
					*outputMode |= zRR_ENABLE_IO_ON_COMPRESSED_DATA_BIT;
					handle->handleArray[count].type = DATASET_IS_COMPRESSED;
				}
				else
				{
					*outputMode |= zRR_LEAVE_FILE_COMPRESSED_BIT;
			
				}
				if(FileIsMigrated)
				{
					handle->handleArray[count].type &= ~(DATASET_IS_COMPRESSED);
					*outputMode &= ~(zRR_ENABLE_IO_ON_COMPRESSED_DATA_BIT);
					*outputMode |= zRR_LEAVE_FILE_COMPRESSED_BIT;
				}
				/* if the file is migrated from NSS file system and if the file is compressed 
			   remove the DATASET_IS_COMPRESSED bit from handle->handleArray[count].type
			   which will leave file uncompressed.For this we need to pass dataStreamFromNSSIsMigrated
			   bit in this function which will check weather the file is migrated or not.*/
			}
		}
	}

	*openMode = 0;
	*openMode = scanMode;
	
	if (inputMode & NWSM_NO_DATA_STREAMS)
		*openMode &= (~FS_DATA_STREAM);
	
	if (inputMode & NWSM_NO_EXTENDED_ATTRIBUTES)
		*openMode &= (~FS_EXTENDED_ATTRIBUTES);
			
	return 0;
}


/*************************************************************************************************************************************/
#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_READ|FILESYS
#define FNAME   "FS_GetCompressedFileSize"
#define FPTR     FS_GetCompressedFileSize
CCODE FS_GetCompressedFileSize(FS_FILE_OR_DIR_HANDLE *handle)
{
	STATUS		status;
	UINT32 bytesRead = 0, bytesToRead;
	compressFileHeader_t *compFileHeaderPtr, compFileHeader;

	bytesToRead = sizeof(compressFileHeader_t);

	status=zRead(	handle->handleArray[0].handle.nssHandle,
		  			zNILXID,
					0,
					bytesToRead,
					&compFileHeader,
					(NINT *)&bytesRead
				);

	if(status==0)
	{
		compFileHeaderPtr = &compFileHeader;
		handle->handleArray[0].size=compFileHeaderPtr->CompressedFileLength;
	}
	else
	{
	    FLogError("zRead", status, 0);
	}
	    

	return(status);
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|NSS_READ
#define FNAME   "FS_SetSparseStatus"
#define FPTR     FS_SetSparseStatus
CCODE FS_SetSparseStatus(FS_FILE_OR_DIR_HANDLE *fsHandles)
{
	CCODE		ccode = 0;
	zInfo_s		*fileInfo;
	QUAD		streamSz;
	NINT		bytesForExtents;
	void		*retExtentList = NULL;
	QUAD		retEndingOffset, retStartingOffset;
	NINT		retExtentListCount;
	int			i, j, k;
	QUAD		currentStartOffset, currentBlockSize;
	QUAD		blkSz, bytes, tmp;
	int			startBit, blocks, tmpBlocks;
	void		*bitMap = NULL;
	QUAD		totSize;
	BOOL		firstCheck;
	unsigned int  handleType;
	FStart();

	if(fsHandles->isDirectory)
		goto Return;

	fileInfo = (zInfo_s *)tsaMalloc(sizeof(zInfo_s));
	if (fileInfo == NULL)
	{
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}
	bytesForExtents = sizeof(zAllocationExtentElement_s) * 5;
	retExtentList = (void *)tsaMalloc(bytesForExtents);
	if (retExtentList == NULL)
	{
		tsaFree(fileInfo);
		ccode = NWSMTS_OUT_OF_MEMORY;
		goto Return;
	}

	for (j = 0; (j < (fsHandles->streamCount)); j++)
	{
		streamSz = fsHandles->handleArray[j].size;
		if (streamSz == 0)
			continue;
		
		ccode = zGetInfo(fsHandles->handleArray[j].handle.nssHandle, zGET_BLOCK_SIZE, \
						sizeof(zInfo_s), zINFO_VERSION_A, fileInfo);
		if (ccode)
		{
			FLogError("zGetInfo", ccode, NULL);
			break;
		}
		blkSz = fileInfo->blockSize.size;

		blocks = streamSz/blkSz;
		blocks += (streamSz % blkSz)?1:0;

		bytes = blocks/8;
		bytes += (blocks % 8)?1:0;

		/* Allocate a multiple of 4 as the bit API's work on 4 byte boundaries */
		if (bytes % 4)
		{
			tmp = 4 - bytes % 4;
			bytes = bytes + tmp;
		}
		
		FTrack3(TSAREAD, DC_CRITICAL|DC_COMPACT, "File Size : %Ld. Total blocks : %ld, Total bytes : %ld\n ", 
		            streamSz, blocks, bytes);

		bitMap = (void *)tsaMalloc(bytes);
		if (bitMap == NULL)
			break;
		memset(bitMap, 0, bytes);

		retEndingOffset = 1;
		retStartingOffset = 0;
		totSize = 0;
		firstCheck = TRUE;
		handleType = 0;
		
		while (retEndingOffset && streamSz)
		{
			ccode = zGetFileMap(fsHandles->handleArray[j].handle.nssHandle, zNILXID, retStartingOffset, zFILEMAP_ALLOCATION, bytesForExtents,\
						retExtentList, &retEndingOffset, &retExtentListCount);
			if (ccode)
			{
			    FLogError("zGetFileMap", ccode, NULL);
				break;
			}
			if (firstCheck && retExtentListCount < 1)
			{
				firstCheck = FALSE;
				break;
			}
			else if (firstCheck)
			{
				handleType |= DATASET_IS_SPARSE;
				firstCheck = FALSE;
			}
			
			FTrack1(TSAREAD, DC_CRITICAL|DC_COMPACT, "Extent List count : %ld\n", retExtentListCount);
			
			for (i = 0; (i < retExtentListCount); i++)
			{
				currentStartOffset = ((zAllocationExtentElement_s *)retExtentList)[i].offset;
				currentBlockSize = ((zAllocationExtentElement_s *)retExtentList)[i].length;

				tmpBlocks = currentBlockSize/blkSz;
				tmpBlocks += (currentBlockSize % blkSz)?1:0;

				startBit = currentStartOffset/blkSz;
				startBit += (currentStartOffset % blkSz)?1:0;

			    FTrack2(TSAREAD, DC_CRITICAL|DC_COMPACT, "Current start offset : %Lu. block size : %Lu\n", 
			                currentStartOffset, currentBlockSize);

				for (k=0; k<tmpBlocks; k++)
					BitSet(bitMap, k + startBit);
				totSize += currentBlockSize;

			}

			retStartingOffset = retEndingOffset;
		}

		if ((handleType & DATASET_IS_SPARSE) && ccode == 0)
		{
			if (ScanClearedBits(bitMap, 0L, blocks) == -1)
			{
				tsaFree(bitMap);
				continue;
			}
		}
		else
		{
			tsaFree(bitMap);
			continue;
		}
		
		fsHandles->handleArray[j].streamMap = (unsigned char *)bitMap;
		fsHandles->handleArray[j].streamMapSize = bytes;
		fsHandles->handleArray[j].blkSz = blkSz;
		fsHandles->handleArray[j].dataSz = totSize;
		fsHandles->handleArray[j].type = handleType;

		if(isDebugSparse())
            DisplayBitMap(bitMap, bytes);
	}

	tsaFree(retExtentList);
	tsaFree(fileInfo);
	
Return:
	FEnd(ccode);
	return ccode;
}



/*
void DisplayBitMap(void *bitMap, long bytes)
{
	int			i = 0, j, k;
	
	ConsolePrintf("BIT MAP:\n");
	j = 0;
	for (k = 0; ((k < bytes) && ((k + 32) =< bytes)); k+=32)
	{
		j = k / 32;
		ConsolePrintf("%08x %08x %08x %08x ", ((unsigned long *)bitMap)[j * 8], ((unsigned long *)bitMap)[j * 8 + 1], \
				((unsigned long *)bitMap)[j * 8 + 2], ((unsigned long *)bitMap)[j * 8 + 3]);
		ConsolePrintf("%08x %08x %08x %08x\n", ((unsigned long *)bitMap)[j * 8 + 4], ((unsigned long *)bitMap)[j * 8 + 5], \
				((unsigned long *)bitMap)[j * 8 + 6], ((unsigned long *)bitMap)[j * 8 + 7]);
	}
	i = bytes % 32;
	k = 0;
	while (i)
	{
		ConsolePrintf("%02x ", ((unsigned char *)bitMap)[j * 32 + k]);
		i --;
		k ++;
	}
	ConsolePrintf("\n");

	return;
}
*/

void DisplayBitMap(void *bitMap, long size)
{
	int			constLen, lenProcessed=0, len ;
	char        buf[SMS_DBG_MAX_BUF_LENGTH+1] ; 

	FTrack(TSAREAD, DC_CRITICAL|DC_COMPACT, "BIT MAP:\n");
	constLen = (SMS_DBG_MAX_BUF_LENGTH - 2) / OUTPUT_LENGTH ;
	while(lenProcessed < size)
	{
	    len = constLen < (size - lenProcessed) ? constLen : (size - lenProcessed) ;
	    FTrack1(TSAREAD, DC_CRITICAL|DC_COMPACT, "%s\n", SMSPrintHexMem(buf, SMS_DBG_MAX_BUF_LENGTH-1, ((char *)bitMap+lenProcessed), len, FALSE));
	    lenProcessed += len ;
	}
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|NSS_READ
#define FNAME   "FS_DeleteFile"
#define FPTR     FS_DeleteFile

CCODE FS_DeleteFile(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 deleteFlags)
{
	STATUS status =0;
	
	status = zDelete(
		fileOrDirHandle->parentHandle.nssHandle,
		0,
		fileOrDirHandle->nameSpace,
		fileOrDirHandle->uniPath,
		0,
		deleteFlags
		);
        FLogError("zDelete", status, NULL);	
        return ((CCODE)status);

}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|NSS_READ
#define FNAME   "FS_DeleteSetPurge"
#define FPTR     FS_DeleteSetPurge

CCODE FS_DeleteSetPurge(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, UINT32 attributes)

{
	STATUS status =0;
	Key_t tmpHandle = 0;
	zInfo_s modifyInfo = {0};
	FStart();
	
	status = zOpen(
				fileOrDirHandle->parentHandle.nssHandle,
				fileOrDirHandle->taskID,
				fileOrDirHandle->nameSpace,
				fileOrDirHandle->uniPath,
				zRR_SCAN_ACCESS | zRR_READ_ACCESS | zRR_DONT_UPDATE_ACCESS_TIME,
				&tmpHandle
				);
	if (status)
	{
        FLogError("zOpen", status, NULL);		
        goto Return;
	}
	modifyInfo.std.fileAttributes 	= attributes; 
    modifyInfo.std.fileAttributesModMask  = -1; 
    
	status = zModifyInfo(
				tmpHandle,
				zNILXID,
				zMOD_FILE_ATTRIBUTES,
				sizeof(zInfo_s),
				zINFO_VERSION_A,
				&modifyInfo
				);
	if (status)
	{
        FLogError("zOpen", status, NULL);		
	}

	zClose(tmpHandle);
	
	Return:
		FEnd(status);
	return(status);
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    OPEN | FILESYS
#define FNAME   "FS_SecStreamTrailerDataRecovery"
#define FPTR     FS_SecStreamTrailerDataRecovery
CCODE FS_SecStreamTrailerDataRecovery(char *dataPtr, QUAD bytesToProcess, QUAD *sidfSize, char *underFlowBuffer, UINT32 *underFlowBufferLen)
{
	CCODE					ccode =0;
	compressFileHeader_t	compFileHeader;

	FStart();
	
	if(bytesToProcess < sizeof(compressFileHeader_t))
	{
	    if((underFlowBuffer = tsaMalloc(bytesToProcess)) == NULL)   
	    {
            ccode = NWSMTS_OUT_OF_MEMORY;
            goto Return;
	    }
	    memcpy(underFlowBuffer, dataPtr, bytesToProcess); 
	    *underFlowBufferLen = bytesToProcess;
		ccode = NWSMUT_BUFFER_UNDERFLOW;
		goto Return;
	}
	memcpy(&compFileHeader, dataPtr, sizeof(compressFileHeader_t));
	
	//Analyze the stream is compressed or not. Because this code will get executed for files which has uncompressed secondary streams as well.
	if(*sidfSize == compFileHeader.OriginalFileLength)
	{
		if(*sidfSize !=  compFileHeader.CompressedFileLength && *sidfSize > compFileHeader.CompressedFileLength)
			*sidfSize  =  compFileHeader.CompressedFileLength;	
	}
	
Return:
	FEnd(ccode);
	return ccode;
}
