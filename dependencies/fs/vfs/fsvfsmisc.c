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
 | $Modtime:   20 Jun 2004 10:30:00  $
 |
 | $Workfile:   fsvfsmisc.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Misc. file system functions
 +-------------------------------------------------------------------------*/

/* External headers */
#include <string.h>
#include <stdio.h>
#include <errno.h>
extern int errno;

#include <smstserr.h>
#include <smsutapi.h>

/* NSS headers */
#include <zParams.h>

/* SMS headers */
#include <smstypes.h>
#include <fsinterface.h>
#include <smsdebug.h>

/* external globals */
extern unsigned int	tsazInfoVersion;
extern unsigned int	nssInfoMask;

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	FILESYS|VFS_SCAN
#define FNAME   "FS_VFS_IsCOWEnabled"
#define FPTR 	FS_VFS_IsCOWEnabled
BOOL FS_VFS_IsCOWEnabled(char *volName)
{
	/* COW not supported for VFS */
	return FALSE;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	TSASCAN
#define FNAME   "FS_VFS_CheckCOWOnName"
#define FPTR 	FS_VFS_CheckCOWOnName
BOOL FS_VFS_CheckCOWOnName(unicode *dataSetName)
{
	/* COW not supported for VFS */
	return FALSE;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_SetzInfoVersion"
#define FPTR 	FS_VFS_SetzInfoVersion
void FS_VFS_SetzInfoVersion(char *volName)
{
	/* We will use InfoB structure for info exchange between FS and the TSA */
	tsazInfoVersion = zINFO_VERSION_B;
	nssInfoMask = 0x8FFFF;
	
	return;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	TSAREAD|FILESYS
#define FNAME   "FS_VFS_GetDirSpaceRestriction"
#define FPTR 	FS_VFS_GetDirSpaceRestriction
CCODE FS_VFS_GetDirSpaceRestriction(void *info, FS_FILE_OR_DIR_HANDLE dirHandle, SQUAD *dirSpaceRestriction)
{
	/* No directory space restrictions for VFS */
	return FS_NO_SPACE_RESTRICTION;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_OpenAndRename"
#define FPTR 	FS_VFS_OpenAndRename
CCODE FS_VFS_OpenAndRename(
					FS_FILE_OR_DIR_HANDLE *fsHandle, 
					BOOL isParent, 
					unicode *bytePath, 
					UINT32 nameSpaceType, 
					unicode *oldFullName,
					unicode *newDataSetName)
{
	STATUS			 status;
	unsigned char 	*oldPath = NULL, *terminalNode = NULL, *newPath = NULL;
	UINT32			 pathLength;
	char			*dirOffset;
	char			*tmpEndPtr;
	char		 	 saveChar;
	int			 	 bytesToCopy;

	status = SMS_UnicodeToByte(bytePath, &pathLength, &oldPath, NULL);
	if (!status)
	{
		status = SMS_UnicodeToByte(newDataSetName, &pathLength, &terminalNode, NULL);
		if (!status)
		{
			tmpEndPtr = oldPath + strlen(oldPath) - 1;
			dirOffset = strrchr(oldPath, '/');
			if (dirOffset == NULL || dirOffset == tmpEndPtr)
			{
				if (dirOffset == tmpEndPtr)
				{
					saveChar = *tmpEndPtr;
					*tmpEndPtr = 0;
					dirOffset = strrchr(oldPath, '/');
					*tmpEndPtr = saveChar;
				}
				
				if (dirOffset == NULL)
				{
					dirOffset = strrchr(oldPath, '/');
					if (dirOffset == NULL)
					{
						status = NWSMTS_INTERNAL_ERROR; 
						goto Return;
					}
				}
			}

			/* Allocate and copy the parent, concatenate the new node name */
			bytesToCopy = (unsigned char *)dirOffset - (unsigned char *)oldPath;
			newPath = (unsigned char*)tsaMalloc(bytesToCopy + strlen(terminalNode) + sizeof(char));
			if (newPath == NULL)
			{
				status = FS_NO_MEMORY;
				goto Return;
			}
			memcpy(newPath, oldPath, bytesToCopy + sizeof(char));
			newPath[bytesToCopy + 1] = 0;
			strcat(newPath, terminalNode);
	
			status = rename(oldPath, newPath);
			if (status)
			{
				FLogError("rename", errno, NULL);
				status = NWSMTS_INTERNAL_ERROR;
			}
		}
	}

Return:
	if (oldPath)
		tsaFree(oldPath);

	if (terminalNode)
		tsaFree(terminalNode);

	if (newPath)
		tsaFree(newPath);
	
	return status;
}

#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_CheckBackupBit"
#define FPTR 	FS_VFS_CheckBackupBit
int FS_VFS_CheckBackupBit(char *volName)
{
	/* The do not backup bit for VFS is always FALSE */
	return 0;
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE 	VFS_SCAN|FILESYS
#define FNAME   "FS_VFS_SetFileTypes"
#define FPTR 	FS_VFS_SetFileTypes
void FS_VFS_SetFileTypes(UINT32 * fileType, void * nfsInfo, UINT32 linkCount)
{
	zUnixInfo_s *unixInfo = (zUnixInfo_s*)nfsInfo;
	/* set the file types of device, fifo, s-link,h-link etc*/
	
			if(S_ISCHR(unixInfo->fMode))
				*fileType = FS_VFS_CHARACTER_DEVICE;
			else if(S_ISBLK(unixInfo->fMode))
				*fileType = FS_VFS_BLOCK_DEVICE;
			else if(S_ISFIFO(unixInfo->fMode))
				*fileType = FS_VFS_FIFO;
			else if(S_ISSOCK(unixInfo->fMode))
				*fileType = FS_VFS_SOCKET;
			else if(S_ISLNK(unixInfo->fMode))
				*fileType = FS_VFS_SOFT_LINK;
			else if(S_ISDIR(unixInfo->fMode))
				*fileType = FS_VFS_DIRECTORY;
			else if(!S_ISDIR(unixInfo->fMode) && linkCount > 1)
				*fileType = FS_VFS_HARD_LINK;
			else if(S_ISREG(unixInfo->fMode))
				*fileType = FS_VFS_REGULAR;
			else
				*fileType = FS_VFS_REGULAR; /*assume anyother file type as regular*/

	return;
}

