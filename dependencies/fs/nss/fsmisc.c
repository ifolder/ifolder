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
 | $Modtime:   03 Jul 2002 10:30:00  $
 |
 | $Workfile:   fsmisc.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Misc. file system functions
 +-------------------------------------------------------------------------*/

/* External headers */
#ifdef N_PLAT_NLM
#include <nwapidef.h>
#endif
#include <string.h>
#include <stdio.h>

#include <smsutapi.h>

/* NSS headers */
#include <zPublics.h>
#include <zParams.h>
#include <zError.h>
#include <zFriends.h>

#ifdef N_PLAT_NLM
/* CFS Headers */
#include <Dstruct.h>
#include <lfsproto.h>
#endif

/* SMS headers */
#include <smsdefns.h>
#include <smstypes.h>
#include <tsaresources.h>
#include <fsinterface.h>
#include <tsalib.h>
#include <tsa_defs.h>
#include <tsaunicode.h>
#include <smstserr.h>
#include <tsa_320.mlh>
#include <smsdebug.h>
/* external globals */
extern unsigned int	tsazInfoVersion;
extern unsigned int	nssInfoMask;

/* External function definitions */
#ifdef N_PLAT_NLM
extern char *NWLstrrchr(const char *string, int find); 
#endif

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS|NSS_SCAN
#define FNAME   "FS_IsCOWEnabled"
#define FPTR     FS_IsCOWEnabled
BOOL FS_IsCOWEnabled(char *volName)
{
	NINT		connID = 0;
	Key_t		rootKey;
	Key_t		volKey;
	STATUS		status = 0;
	zInfo_s		volInfo;
	

	status = zRootKey(connID, &rootKey);
	if (status)
	{
		FLogError("zRootKey", status, NULL);
		return FALSE;
	}

	status = zOpen(rootKey,  zNSS_TASK, zNSPACE_LONG | zMODE_UTF8,\
            volName, zRR_SCAN_ACCESS | zRR_DONT_UPDATE_ACCESS_TIME, &volKey);
	if (status != 0)
	{
		FLogError("zOpen", status, NULL);
		zClose(rootKey);
		return FALSE;
	}
	
	status = zGetInfo(volKey, zGET_VOLUME_INFO, sizeof(volInfo), zINFO_VERSION_A, &volInfo);
	if (status)
	{
		FLogError("zGetInfo", status, NULL);
		zClose(volKey);
		zClose(rootKey);
		return FALSE;
	}

	if (volInfo.vol.features.enabled & zATTR_COW)
	{
		FTrack1(BACKUP, DC_COMPACT | DC_CRITICAL, "Volume : %s : COWEnabled : TRUE\n", volName);
		zClose(volKey);
		zClose(rootKey);
		return TRUE;
	}
	else
	{
		FTrack1(BACKUP, DC_COMPACT | DC_CRITICAL, "Volume : %s : COWEnabled : FALSE\n",  volName);
		zClose(volKey);
		zClose(rootKey);
		return FALSE;
	}
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    TSASCAN
#define FNAME   "FS_CheckCOWOnName"
#define FPTR     FS_CheckCOWOnName
BOOL FS_CheckCOWOnName(unicode *dataSetName)
{
	UINT32			 volSepPos;
	unicode			*volName;
	RESOURCE_NODE	*resNode = NULL;
	unicode			 sep2[UNI_SEP_BASE_SIZE];

	GetUniNameSpaceSeparators(DOSNameSpace, NULL, sep2);
	
	volSepPos = unicspn(dataSetName, sep2);
	volName = (unicode *)tsaMalloc((volSepPos + 2)*sizeof(unicode));
	if (volName == NULL)
	{
		return FALSE;
	}
	
	unincpy(volName, dataSetName, volSepPos + 1);
	volName[volSepPos + 1] = '\0';
	
	LockResourceList();
	resNode = FindResourceByUniName(volName);
	if (resNode == NULL)
	{
		UnlockResourceList();
		tsaFree(volName);
		return FALSE;
	}
	else
	{
		
		if(resNode->COWEnabled)
		{
			UnlockResourceList();
			FTrack1(BACKUP, DC_VERBOSE | DC_MINOR, "Volume : %s : COWEnabled : TRUE\n", volName);
			tsaFree(volName);
			return TRUE;
		}
		else
		{
			UnlockResourceList();
			FTrack1(BACKUP, DC_VERBOSE | DC_MINOR, "Volume : %s : COWEnabled : FALSE\n", volName);
			tsaFree(volName);
			return FALSE;
		}
	}	
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_SCAN|FILESYS
#define FNAME   "FS_SetzInfoVersion"
#define FPTR     FS_SetzInfoVersion
void FS_SetzInfoVersion(char *volName)
{
	NINT		connID = 0;
	Key_t		rootKey;
	Key_t		volKey;
	STATUS		status = 0;
	zInfo_s		volInfo;
	static int	called = 0;

	if (called == 0)
		called = 1;
	else
		return;
	
	status = zRootKey(connID, &rootKey);
	if (status)
	{
		FLogError("zRootKey", status, NULL);
		tsazInfoVersion = zINFO_VERSION_A;
		nssInfoMask = 0xFFFF;
		return;
	}

	status = zOpen(rootKey,  zNSS_TASK, zNSPACE_LONG | zMODE_UTF8,\
            volName, zRR_SCAN_ACCESS | zRR_DONT_UPDATE_ACCESS_TIME, &volKey);
	if (status != 0)
	{
		FLogError("zOpen", status, NULL);
		zClose(rootKey);
		tsazInfoVersion = zINFO_VERSION_A;
		nssInfoMask = 0xFFFF;
		return;
	}
	
	status = zGetInfo(volKey, zGET_VOLUME_INFO, sizeof(volInfo), zINFO_VERSION_B, &volInfo);
	if (status)
	{
		FLogError("zGetInfo", status, NULL);
		tsazInfoVersion = zINFO_VERSION_A;
		nssInfoMask = 0xFFFF;
	}
	else
	{
		FTrack(BACKUP, DC_COMPACT | DC_CRITICAL, "zInfoVersion : zINFO_VERSION_B\n");
		tsazInfoVersion = zINFO_VERSION_B;
		nssInfoMask = 0x8FFFF;
	}
	
	zClose(volKey);
	zClose(rootKey);
	return;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    TSAREAD|FILESYS
#define FNAME   "FS_GetDirSpaceRestriction"
#define FPTR     FS_GetDirSpaceRestriction
CCODE FS_GetDirSpaceRestriction(void *info, FS_FILE_OR_DIR_HANDLE dirHandle, SQUAD *dirSpaceRestriction)
{
#ifdef N_PLAT_NLM
	CCODE			ccode;
	union DirUnion	*dirEntry;
	LONG			dirEntryNum;
	LONG			volNum;
	unicode_t		uniVolName[NW_MAX_VOLUME_NAME_LEN];
	NINT			uniSz = NW_MAX_VOLUME_NAME_LEN;
	char			*volName;
	RESOURCE_NODE	*resNode = NULL;
	UINT32			volLength;
#endif
	FStart();
	
	if(tsazInfoVersion == zINFO_VERSION_B)
	{
		*dirSpaceRestriction = ((zInfoB_s *)info)->dirQuota.quota;			
		
		/*if dir quota is enabled on the volume NSS returns 7FFFFFFFFFFFFFFF and if dir quota is disabled on the volume nss returns FFFFFFFFFFFFFFFF */
		if(*dirSpaceRestriction == FS_NSS_NO_RESTRICTION || *dirSpaceRestriction <0) 
			return FS_NO_SPACE_RESTRICTION;
		
		*dirSpaceRestriction /= (1024 * 4);
	}
#ifdef N_PLAT_NLM	
	else
	{
		/* The ZIDs lower 32 bit is the entry number. (TSA600) */
		dirEntryNum = ((UINT32 *)(&((zInfo_s *)info)->std.zid))[0];

		/* Get the volume ID */
		ccode = xVolumeGUIDToName(&((zInfo_s *)info)->std.volumeID, uniVolName, uniSz);
		if (ccode)
		{
			FLogError("xVolumeGUIDToName", ccode, NULL);
			return FS_INTERNAL_ERROR;
		}

		volLength = unilen(uniVolName) * 6 + 3;
		volName = (char *)tsaCalloc(1, volLength);
		if(!volName)
			return NWSMTS_OUT_OF_MEMORY;
		
		ccode = SMS_UnicodeToByte(uniVolName, &volLength, &volName, NULL);
		if (ccode || (volLength == 0))
		{
			tsaFree(volName);
			return NWSMTS_INTERNAL_ERROR;
		}

		volName[volLength] = *((char *)_GetMessage(COLON));
		volName[volLength + 1] = '\0';
		LockResourceList();
		resNode = FindResourceByName(volName);
		if (resNode == NULL)
		{
			UnlockResourceList();
			tsaFree(volName);
			return FS_INTERNAL_ERROR;
		}
	
		volNum = resNode->resID;
		UnlockResourceList();
		tsaFree(volName);
		
		ccode = CheckAndGetDirectoryEntry(volNum, dirEntryNum, NWNAME_SPACE_PRIMARY, &dirEntry);
		if (!ccode)
		{
			*dirSpaceRestriction = (LONG)dirEntry->DOSSubDir.SDMaximumSpace; //DOSFile returns only 16 bits, while DOSSubDir returns 32 bit information
			if ((LONG)*dirSpaceRestriction == 0)
				return FS_NO_SPACE_RESTRICTION;
		}
		else
		{
			FLogError("CheckAndGetDirectoryEntry", ccode, 0);
			return FS_INTERNAL_ERROR;
		}
	}
#endif
	FEnd(0);
	return 0;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_SCAN|FILESYS
#define FNAME   "FS_OpenAndRename"
#define FPTR     FS_OpenAndRename
CCODE FS_OpenAndRename(
					FS_FILE_OR_DIR_HANDLE *fsHandle, 
					BOOL isParent, 
					unicode *bytePath, 
					UINT32 nameSpaceType, 
					unicode *oldFullName,
					unicode *newDataSetName)
{
	STATUS		 status;
	Key_t		 nssHandle = 0;
	BOOL		 isLink = FALSE;
	unicode		*dirOffset;
	int			 bytesToCopy;
	unicode		*tmpEndPtr;
	unicode 	*dirName = NULL, *childName = NULL;
	unicode		 saveUniChar;
	BOOL		 saved = FALSE;
	unicode		 sep1[UNI_SEP_BASE_SIZE], sep2[UNI_SEP_BASE_SIZE];
	FStart();

	nameSpaceType = MapToNSSNameSpace(nameSpaceType);

	/* Get the unicode name space seperators */
	GetUniNameSpaceSeparators(DOSNameSpace, sep1, sep2);
	
	/* Get the parent name for the data set */
	tmpEndPtr = bytePath + unilen(bytePath) - 1;
	dirOffset = unirchr(bytePath, sep2[0]);
	if (dirOffset == NULL || dirOffset == tmpEndPtr)
	{
		if (dirOffset == tmpEndPtr)
		{
			saveUniChar = *tmpEndPtr;
			*tmpEndPtr = 0;
			dirOffset = unirchr(bytePath, sep2[0]);
			*tmpEndPtr = saveUniChar;
		}
		
		if (dirOffset == NULL)
		{
			dirOffset = unirchr(bytePath, sep1[0]);
			if (dirOffset == NULL)
			{
				status = FS_INTERNAL_ERROR; 
				goto Return;
			}
		}
	}

	/* Allocate and copy the parent */
	bytesToCopy = (unsigned char *)dirOffset - (unsigned char *)bytePath;
	dirName = (unicode *)tsaMalloc(bytesToCopy + 2 * sizeof(unicode));
	if (dirName == NULL)
	{
		status = FS_NO_MEMORY;
		goto Return;
	}
	memcpy(dirName, bytePath, bytesToCopy + sizeof(unicode));
	dirName[bytesToCopy/sizeof(unicode) + 1] = 0;

	/* Open the parent data set */
	status = zOpen(
			fsHandle->parentHandle.nssHandle,
			fsHandle->taskID,
			fsHandle->nameSpace,
			dirName,
			zRR_DONT_UPDATE_ACCESS_TIME,
			&nssHandle);
		
	if(status == zERR_LINK_IN_PATH)
	{
		status = zOpen(
				fsHandle->parentHandle.nssHandle,
				fsHandle->taskID,
				fsHandle->nameSpace | zMODE_LINK,
				dirName,
				zRR_DONT_UPDATE_ACCESS_TIME | SEARCH_LINK_AWARE | SEARCH_OPERATE_ON_LINK,
				&nssHandle);
		isLink = TRUE;
	}
	
	if (status)
	{
		FLogError("zOpen", status, NULL);
		goto Return;
	}

	/* Get the child in the specified name space */
	GetUniNameSpaceSeparators(nameSpaceType, sep1, sep2);
	tmpEndPtr = oldFullName + unilen(oldFullName) - 1;
	childName = unirchr(oldFullName, sep2[0]);
	if (childName == NULL || childName == tmpEndPtr)
	{
		if (childName == tmpEndPtr)
		{
			saveUniChar = *tmpEndPtr;
			*tmpEndPtr = 0;
			saved = TRUE;
			childName = unirchr(oldFullName, sep2[0]);
		}
		
		if (childName == NULL)
		{
			childName = unirchr(oldFullName, sep1[0]);
			if (childName == NULL)
			{
				status = FS_INTERNAL_ERROR; 
				goto Return;
			}
		}
	}
	childName ++;
	
	/* Rename the node ussing the parent handle */
	status = zRename(
			nssHandle,
			zNILXID,
			nameSpaceType| (isLink? zMODE_LINK : 0),
			childName,
			zMATCH_ALL,
			nameSpaceType | (isLink? zMODE_LINK : 0),
			newDataSetName,
			zRENAME_THIS_NAME_SPACE_ONLY | zRENAME_ALLOW_RENAMES_TO_MYSELF);
	
	if(status)
	{
	    FLogError("zRename", status, NULL);
	}
Return:
	if (saved)
		*tmpEndPtr = saveUniChar;
	
	if (dirName)
		tsaFree(dirName);
		
	if (nssHandle)
		zClose(nssHandle);
	
	FEnd(status);
	return status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    NSS_SCAN|FILESYS
#define FNAME   "FS_CheckBackupBit"
#define FPTR     FS_CheckBackupBit
int FS_CheckBackupBit(char *volName)
{
	NINT		connID = 0;
	Key_t		rootKey;
	Key_t		volKey;
	STATUS		status = 0;
	zInfo_s		volInfo;
	int 		retval;
	
	status = zRootKey(connID, &rootKey);
	if (status)
	{
		FLogError("zRootKey", status, NULL);
		return 0;
	}
	
	status = zOpen(rootKey,  zNSS_TASK, zNSPACE_LONG | zMODE_UTF8,\
            volName, zRR_SCAN_ACCESS | zRR_DONT_UPDATE_ACCESS_TIME, &volKey);
	if (status != 0)
	{
		FLogError("zOpen", status, NULL);
		zClose(rootKey);
		return 0;
	}

	status = zGetInfo(volKey, zGET_VOLUME_INFO, sizeof(volInfo), zINFO_VERSION_A, &volInfo);
	if (status)
	{
		FLogError("zGetInfo", status, NULL);
		zClose(volKey);
		zClose(rootKey);
		return 0;
	}

	if (volInfo.vol.features.enabled & zATTR_DONT_BACKUP)
		retval = 1;
	else
		retval = 0;
	
	zClose(volKey);
	zClose(rootKey);
	return retval;
}

