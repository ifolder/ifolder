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
 | $Modtime:   29 Jun 2004 21:15:00  $
 |
 | $Workfile:   fsvfscreate.c  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Create files and directories
 +-------------------------------------------------------------------------*/

#include <errno.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <tsaresources.h>
#include <tsadset.h>

#include <smsdefns.h>
#include <smstypes.h>
#include <smstserr.h>

#include "fsinterface.h"
#include <smsdebug.h>

/*************************************************************************************************************************************/

/*
 *  1. If the dataset is a volume, the dataset is opened using zopen()
 *  2. If the namespace is MAC or NFS, the volume name has to be truncated
 *     as the volume name in the absolute path is not accepted by these namespaces.
 *  3. If the dataset is already present and is read-only, the dataset has to be opened 
 *     and the read only attributes to be reset.
 *  4. If the dataset is a directory and the path does not exist, create the path nodes
 */


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_VFS_Create"
#define FPTR     FS_VFS_Create
CCODE FS_VFS_Create(FS_FILE_OR_DIR_HANDLE  		*fileOrDirHandle, 
	                FS_CREATE_MODE	createMode, 
	                FS_ACCESS_RIGHTS	accessRights, 
	                FS_ATTRIBUTES		attributes, 
	                UINT32				streamType, 
	                FS_HANDLE			*retHandle, 
	                unicode				*dSetName, 
	                UINT32				rflags, 
	                BOOL				isPathFullyQualified,
	                void* 				buffer,
	                FS_HL_TABLE** hlTable) 
{
	STATUS				status = 0;
	punicode				uniPathName = NULL; 
	unicode				*tmpPathPtr = NULL; 
	unicode				*tmpOriginalPath = NULL; 
	punicode				uniVolName = NULL; 
        unicode					secondSeparator[UNI_SEP_BASE_SIZE]; 
	unicode				*pathCursor = NULL; 
	FS_ATTRIBUTES		tmpAttributes = 0; 
	UINT16				pathNodeCount;
	CCODE				ccode = 0;
	unsigned char			*path = NULL;
	zUnixInfo_s			unixInfo;
        unicode					sep1[UNI_SEP_BASE_SIZE], sep2[UNI_SEP_BASE_SIZE];
        UINT32					nameSpace;

	FStart();
		
        
        if(fileOrDirHandle == NULL)     
        {
		ccode = FS_INTERNAL_ERROR;
		goto Return;
        }
        
        if(attributes & FS_SUBDIRECTORY)
      	{
		if(rflags & DATASET_EXISTS)
			createMode = O_CREAT | O_DIRECTORY;
		else
			createMode = S_IRWXU;
      	}
	else
	{
		if(rflags & DATASET_EXISTS)
			createMode = O_RDWR;
		else
			createMode = O_CREAT |O_TRUNC | O_RDWR;
	}

	if(accessRights & FS_WRITE_ACCESS)
		accessRights = S_IRWXU;
	else
		accessRights = S_IRUSR;

        /* 
         * Convert dSetName from ascii to unicode.
         *
         * Also setting fileOrDirHandle->uniPath. This is required in case of engines which
         * pass terminal node name for a file. Here the absolute path is obtained from the
         * parent handle (the directory in which the file resides).
         */
        if(dSetName) 
        {
			uniPathName = (punicode) tsaMalloc((unilen(dSetName) + 1 ) * sizeof(unicode));
			if (uniPathName == NULL) 
			{
			    ccode = NWSMTS_OUT_OF_MEMORY;
			    goto Return;
			}
			unicpy(uniPathName, dSetName);

			if(tmpOriginalPath)
				dSetName = tmpOriginalPath;
        }

	ccode = FS_VFS_FixPathToVFS(fileOrDirHandle->parentHandle.vfsHandle.fullPath, uniPathName, &path);
	if (ccode)
	{
		goto Return;
	}

	if(buffer)
	{
		if(fileOrDirHandle->isDirectory)
		{
			if(!(rflags & DATASET_EXISTS))
				status = mkdir(path, createMode);
		}
		else
		{
			unixInfo = ((zInfoB_s*)buffer)->unixNS.info;
			
			switch(fileOrDirHandle->fileType)
			{
				case FS_VFS_CHARACTER_DEVICE:
				case FS_VFS_BLOCK_DEVICE:
				case FS_VFS_FIFO:
					/*we need to see if we should unlink and recreate the special files*/
					if(!(rflags & DATASET_EXISTS))
						status = mknod(path, unixInfo.fMode, unixInfo.rDev);
					break;
				case FS_VFS_SOCKET:
					FLogError("Cannot create socket - unsupported file type:", -1, path);
					break;
				case FS_VFS_REGULAR:
				case FS_VFS_HL_REG:
					{
						UINT32 linkCount = ((zInfoB_s*)buffer)->count.hardLink;
						UINT32 dev_no = 0;
						STRING ln_name = NULL;
						BOOL foundInHLTable = FALSE;
						/*lets update the table, so that if links are available and doesn't exists can point to this*/
						foundInHLTable = HL_CheckLink(hlTable, &fileOrDirHandle->fileType, dev_no, linkCount, fileOrDirHandle->handleArray[0].iNodeNumber, path, &ln_name);
						if(foundInHLTable == 0 || foundInHLTable != 1)
						{
							HL_AddLink(hlTable, &fileOrDirHandle->fileType, dev_no, linkCount, fileOrDirHandle->handleArray[0].iNodeNumber, path);
							status = open(path, createMode, accessRights);
						}
						else
						{/*if order of restore is different, then we might end up here*/
							status = link((const char*)ln_name, path);
							if(status == -1)
							{
								if(errno == EXDEV)
									FLogError("link", errno, perror("Error linking"));
								else if(errno == EEXIST) /* recreate the link, as it might be a overwrite operation*/
								{
									status = unlink(path);
									if(status == -1)
										FLogError("unlink", errno, perror("Error unlinking"));
									else
										status = link((const char*)ln_name, path);
									status = 0; /*reset status*/
								}
								else
								{
									/*cannot link looks like something wrong, remove the entry from table
									*/
									FLogError("link", errno, perror("Link creation error"));
									HL_RemoveLink(hlTable, &fileOrDirHandle->fileType, dev_no, fileOrDirHandle->handleArray[0].iNodeNumber);
									status = open(path, createMode, accessRights); /*perform a normal open*/
								}
							}
							else
							{
								fileOrDirHandle->fileType == FS_VFS_HARD_LINK; /*making it so that write is skipped*/
								status = 0;
							}
						}
					}
					break;
				case FS_VFS_SOFT_LINK:
				case FS_VFS_HL_TO_SL:
					{
						UINT32 linkCount = ((zInfoB_s*)buffer)->count.hardLink;
						UINT32 dev_no = 0;
						STRING ln_name = NULL;
						BOOL foundInHLTable = FALSE;
						/*	Check the table for the originator, if found create a link, else report error and remove the entry from the table
							so, we don't point others as well*/
						foundInHLTable = HL_CheckLink(hlTable, &fileOrDirHandle->fileType, dev_no, linkCount, fileOrDirHandle->handleArray[0].iNodeNumber, path, &ln_name);
						if(foundInHLTable == 0 || foundInHLTable != 1)
						{
							/*Update link table, as this might be a potential Hard link*/
							HL_AddLink(hlTable, &fileOrDirHandle->fileType, dev_no, linkCount, fileOrDirHandle->handleArray[0].iNodeNumber, path);
							status = symlink(fileOrDirHandle->handleArray[0].LinkName,path);
							if(status == -1)
							{
								if(errno == ENOENT) /*dangling link*/
									FLogError("symlink", errno, perror("Error linking"));
								else if(errno == EEXIST) /* recreate the link, as it might be a overwrite operation*/
								{
									status = unlink(path);
									if(status == -1)
										FLogError("unlink", errno, perror("Error unlinking"));
									else
										status = symlink(fileOrDirHandle->handleArray[0].LinkName, path);
									status = 0; /*reset status*/
								}
							}
						}
						else
						{
							if(fileOrDirHandle->fileType == FS_VFS_HL_TO_SL)
							{
								status = link((const char*)ln_name, path);
								if(status == -1)
								{
									if(errno == EXDEV)
										FLogError("link", errno, perror("Error linking"));
									else if(errno == EEXIST) /* recreate the link, as it might be a overwrite operation*/
									{
										status = unlink(path);
										if(status == -1)
											FLogError("unlink", errno, perror("Error unlinking"));
										else
											status = link((const char*)ln_name, path);
										status = 0; /*reset status*/
									}
									else
									{
										/*cannot link looks like something wrong, remove the entry from table
										*/
										FLogError("link", errno, perror("Link creation error"));
										HL_RemoveLink(hlTable, &fileOrDirHandle->fileType, dev_no, fileOrDirHandle->handleArray[0].iNodeNumber);
										/*Create a symbolic link atleast to the original*/
										status = symlink(fileOrDirHandle->handleArray[0].LinkName,path);
										if(status == -1)
										{
											if(errno == ENOENT) /*dangling link*/
												FLogError("symlink", errno, perror("Error linking"));
											else if(errno == EEXIST) /* recreate the link, as it might be a overwrite operation*/
											{
												status = unlink(path);
												if(status == -1)
													FLogError("unlink", errno, perror("Error unlinking"));
												else
													status = symlink(fileOrDirHandle->handleArray[0].LinkName, path);
												status = 0; /*reset status*/
											}
										}
									}
								}
								else
									status = 0;
							}
						}
					}
					break;
				case FS_VFS_HARD_LINK:
					{
						BOOL foundInHLTable = FALSE;
						UINT32 linkCount = ((zInfoB_s*)buffer)->count.hardLink;
						UINT32 dev_no = 0; /*let us not worry abt device number, since linking will give an error*/
						STRING ln_name = NULL;
						/*	Check the table for the originator, if found create a link, else report error and remove the entry from the table
							so, we don't point others as well*/
						foundInHLTable = HL_CheckLink(hlTable, &fileOrDirHandle->fileType, dev_no, linkCount, fileOrDirHandle->handleArray[0].iNodeNumber, path, &ln_name);
						if(foundInHLTable == 1)
						{
							status = link((const char*)ln_name, path);
							if(status == -1)
							{
								if(errno == EXDEV)
									FLogError("link", errno, perror("Error linking"));
								else if(errno == EEXIST) /* recreate the link, as it might be a overwrite operation*/
								{
									status = unlink(path);
									if(status == -1)
										FLogError("unlink", errno, perror("Error unlinking"));
									else
										status = link((const char*)ln_name, path);
									status = 0; /*link already exists, lets forget abt it and continue*/
								}
								else
								{
									/*cannot link looks like something wrong, remove the entry from table
									so other don't point again*/
									FLogError("link", errno, perror("Link creation error"));
									HL_RemoveLink(hlTable, &fileOrDirHandle->fileType, dev_no, fileOrDirHandle->handleArray[0].iNodeNumber);
									status = open(path, createMode, accessRights); /*perform a normal open*/
									fileOrDirHandle->fileType = FS_VFS_HL_REG; /*marking it so write is not skipped*/
								}
							}
							else
								status = 0;
						}
						else if(foundInHLTable == 0 || foundInHLTable != 1)
						{
							/*not found in table, looks like some thing is wrong, let us try if we can link this dataset pointed to, 
							during backup is available. We are doing this, incase if the link alone is being tried to restored either in
							the same path or in different path*/
							status = link(fileOrDirHandle->handleArray[0].LinkName, path);
							if(status == -1)
							{/*might be a potential candidate for further links so add this to table*/
								HL_AddLink(hlTable, &fileOrDirHandle->fileType, dev_no, linkCount, fileOrDirHandle->handleArray[0].iNodeNumber, path);
								status = open(path, createMode, accessRights); /*perform a normal open*/
							}
						}
					}
					break;

				default:
					status = open(path, createMode, accessRights); /*assume it as a normal file*/
					
			}
		}
		if(status != -1)
		{
			retHandle->vfsHandle.fileDescriptor = status;
			retHandle->vfsHandle.fullPath  = tsaMalloc(strlen(path)+1);
			strcpy(retHandle->vfsHandle.fullPath, path);
			status = 0;
		}
	}


	/* If the path does not exist, create the path nodes by looping though each node in the path */
	if(status && isPathFullyQualified) 
	{
		nameSpace = MapToAsciiNameSpace(fileOrDirHandle->nameSpace);
                pathCursor = uniPathName;
                /* For restore of single file */
                //if (!fileOrDirHandle->isDirectory) 
                {
                        status = GetUniNameSpaceSeparators(nameSpace, sep1, sep2);
                        if (status)
                                goto Return;
                        GenericCountPositions(fileOrDirHandle->nameSpace, NULL, (void*)sep2, (void*)dSetName, &pathNodeCount);
                        /* Skip root since the path is fully qualified*/
					pathCursor++;
                }
                
                while(pathCursor)
                {
                        pathCursor = unichr(pathCursor, (unicode)sep2[0]);
                        if(pathCursor)
                        {
                                (*pathCursor) = 0;
                        }
			ccode = FS_VFS_FixPathToVFS(fileOrDirHandle->parentHandle.vfsHandle.fullPath, uniPathName, &path);
			if (ccode)
			{
				goto Return;
			}

                        /* For single file restore */
                        --pathNodeCount;
                        if (!fileOrDirHandle->isDirectory && !pathNodeCount)
                        {
                        	status = open(path, createMode, accessRights);
                        }
                        else
                        {
				status = mkdir(path, createMode);
				if(errno == EEXIST) /*part of the directory path exists*/
				{
					errno = 0; /*reset error number*/
					status = 0;
				}
                        }

                
                        if((status == -1) && (pathNodeCount==0)) 
                        {
                            FLogError("open/mkdir", errno, perror("Create Error"));
                                break;
                        }
                        /* For single file restore */
                        if (!fileOrDirHandle->isDirectory && pathNodeCount == 0)  
                                break;  
                
                        if(pathCursor)
                        {
                                (*pathCursor) = sep2[0];
                                pathCursor++;
                        }
                        if(pathCursor && status && status != -1)/*close all handle's except the last one*/
                        {
                                close(status);
                                status = 0;
                        }
                }
		if(status != -1)
		{
			retHandle->vfsHandle.fileDescriptor = status;
			retHandle->vfsHandle.fullPath  = tsaMalloc(strlen(path)+1);
			strcpy(retHandle->vfsHandle.fullPath, path);
		}
        }

	if(status)
        {
			if(fileOrDirHandle->isDirectory) 
			{
		       	 ccode = NWSMTS_CREATE_DIR_ENTRY_ERR; //for directory
			}
			else  
			{
				if(errno == ENXIO || errno == ENODEV)
				{
					FLogError("Error opening special device files.\nDevice doesn't exists...Skipping!!!\n", errno, NULL);
					status = 0;
					ccode = 0;
					retHandle->vfsHandle.fileDescriptor = 0; /*reset fileDescriptor as well, we need to change 0, as it is a valid handle*/
					goto Return;
				}
		            if(errno == EACCES) /* || status == zERR_FILE_READ_LOCKED || status == zERR_CANT_DENY_READ_LOCK || status == zERR_CANT_DENY_WRITE_LOCK || status == zERR_LAST_STATE_UNKNOWN)*/
		                ccode = NWSMTS_DATA_SET_IN_USE;
		            else
		                ccode = NWSMTS_OPEN_ERROR;
			}
		FLogError("open", errno, perror("Create Error"));
        }



Return:
        if(uniVolName)
            tsaFree(uniVolName);

        if(uniPathName) 
            tsaFree(uniPathName);

	if(path)
		tsaFree(path);

		FEnd(ccode);

	return ccode;
}


#undef  FTYPE    
#undef  FNAME   
#undef  FPTR    
#define FTYPE    FILESYS
#define FNAME   "FS_VFS_SetFileSize"
#define FPTR     FS_VFS_SetFileSize 
CCODE FS_VFS_SetFileSize(FS_FILE_OR_DIR_HANDLE *fileOrDirHandle, QUAD newSize, UINT32 handleIndex) 
{
        CCODE ccode = 0;

	if(fileOrDirHandle->fileType == FS_VFS_REGULAR || fileOrDirHandle->fileType == FS_VFS_HL_REG)
		ccode = truncate(fileOrDirHandle->handleArray[handleIndex].handle.vfsHandle.fullPath, (UINT32)newSize);
#if 0	
        // 1. De-Allocate the blocks 
        ccode = zSetEOF(
                fileOrDirHandle->handleArray[handleIndex].handle.nssHandle, 
                zNILXID,
                newSize,
                zSETSIZE_PHYSICAL_ONLY);

    if(ccode && ccode !=zERR_PHYSICAL_EOF_NOT_ENABLED)
    {
        char temp[64] ;
        sprintf(temp, "setting %s. size=%-8.8X%-8.8X", "physical", *((UINT32*)&(newSize)+1), *(UINT32*)&(newSize));
        FLogError("zSetEOF", ccode, temp);
    }
        // 2. Set logical file size 
        if (!ccode)
        {
                ccode = zSetEOF(
                        fileOrDirHandle->handleArray[handleIndex].handle.nssHandle, 
                        zNILXID,
                        newSize,
                        zSETSIZE_LOGICAL_ONLY);
        if(ccode)
        {
            char temp[64] ;
            sprintf(temp, "setting %s. size=%-8.8X%-8.8X", "logical", *((UINT32*)&(newSize)+1), *(UINT32*)&(newSize));
            FLogError("zSetEOF", ccode, temp);
        }
        
        }
        //for dosfat_c files    
        if(ccode==zERR_PHYSICAL_EOF_NOT_ENABLED)
        {
                ccode = zSetEOF(
                fileOrDirHandle->handleArray[handleIndex].handle.nssHandle, 
                zNILXID,
                newSize,
                zSETSIZE_NON_SPARSE_FILE);
        }
#endif
	if(ccode == -1) /*need to check for various error values and approp action need to be taken*/
		return errno;
	else
		return 0;
}


