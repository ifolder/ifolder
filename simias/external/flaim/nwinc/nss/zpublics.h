/****************************************************************************
 |
 |	(C) Copyright 1985, 1991, 1993, 1996 Novell, Inc.
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
 |	 NetWare Advance File Services (NSS) module
 |
 |---------------------------------------------------------------------------
 |
 | $Author$
 | $Modtime:   29 Jan 2001 11:39:38  $
 |
 | $Workfile:   zpublics.h  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		NSS	 64 bit (zAPIs) function prototypes.
 +-------------------------------------------------------------------------*/
#ifndef _ZPUBLICS_H_
#define _ZPUBLICS_H_

#ifndef _ZOMNI_H_
#	include <zOmni.h>
#endif

#ifndef _ZPARAMS_H_
#	include <zParams.h>
#endif


#ifdef __cplusplus
extern "C" {
#endif

	/*
	 * zAPIs - The Z NSS File System Interfaces
	 */

STATUS zAbortXaction(
	Key_t	key,
	Xid_t	xid);

STATUS zAddTrustee(
	Key_t	key,
	Xid_t	xid,
	const GUID_t *trustee,
	NINT rights);

STATUS zBeginTask(
	Key_t	key,
	NINT	taskID,
	NINT	*retTaskID);

STATUS zBeginXaction(
	Key_t	key,
	NINT	taskID,
	Xid_t	parentXid,
	Xid_t	*retXid);

STATUS zClose(
	Key_t	key);

STATUS zCommitXaction(
	Key_t	key,
	Xid_t	xid);

STATUS zCreate(
	Key_t		key,
	NINT		taskID,	
	Xid_t		xid,
	NINT		nameSpace,
	const void	*path,
	NINT		fileType,
	QUAD		fileAttributes,
	NINT		createFlags,
	NINT		requestedRights,
	Key_t		*retKey);

STATUS zDelete(
	Key_t		key,
	Xid_t		xid,
	NINT		nameSapce,
	const void	*path,
	NINT		match,
	NINT		deleteFlags);

STATUS zDeleteTrustee(
	Key_t		key,
	Xid_t		xid,
	const GUID_t *trustee);

STATUS zDIORead(
	Key_t	key,
	QUAD	unitOffset,
	NINT	unitsToRead,
	ADDR	callBackContext,
	void	(*dioReadCallBack)(
	ADDR		reserved,
	ADDR		callBackContext,
	NINT		retStatus),
	void	*retBuffer);

STATUS zDIOWrite(
	Key_t		key,
	QUAD		unitOffset,
	NINT		unitsToWrite,
	ADDR		callBackContext,
	void		(*dioWriteCallBack)(
	ADDR			reserved,
	ADDR			callBackContext,
	NINT			retStatus),
	const void	*buffer);

STATUS zEndTask(
	Key_t	key,
	NINT	taskID);

STATUS zFlush(
	Key_t	key);

STATUS zGetFileMap(
	Key_t	key,
	Xid_t	xid,
	QUAD	startingOffset,
	NINT	extentListFormat,
	NINT	bytesForExtents,
	void	*retExtentList,
	QUAD	*retEndingOffset,
	NINT	*retExtentListCount);

STATUS zGetInfo(
	Key_t	key,
	QUAD	getInfoMask,
	NINT	sizeRetGetInfo,
	NINT	infoVersion,
	zInfo_s	*retGetInfo);

STATUS zGetInheritedRightsMask(
	Key_t	key,
	LONG	*retInheritedRightsMask);

STATUS zGetTrustee(
	Key_t	key,
    NINT    startingIndex,
    GUID_t  *retTrustee,
    NINT    *retRights,
    NINT    *retNextIndex);

STATUS zLink(
	Key_t		key,
	Xid_t		xid,
	NINT		srcNameSpace,
	const void	*srcPath,
	NINT		srcMatchAttributes,
	NINT		dstNameSpace,
	const void	*dstPath,
	NINT		renameFlags);

STATUS zLockByteRange(
	Key_t	key,
	Xid_t	xid,	
	NINT	mode,
	QUAD	startingOffset,
	QUAD	length,
	NINT	msecTimeout);

STATUS zModifyInfo(
	Key_t	key,
	Xid_t	xid,
	QUAD	modifyInfoMask,
	NINT	sizeModifyInfo,
	NINT	infoVersion,
	const zInfo_s	*modifyInfo);

STATUS zOpen(
	Key_t		key,
	NINT		taskID,
	NINT		nameSpace,
	const void	*path,
	NINT		requestedRights,
	Key_t		*retKey);

STATUS zRead(
	Key_t	key,
	Xid_t	xid,	
	QUAD	startingOffset,
	NINT	bytesToRead,
	void	*retBuffer,
	NINT	*retBytesRead);

STATUS zRename(
	Key_t		key,
	Xid_t		xid,
	NINT		srcNameSpace,
	const void	*srcPath,
	NINT		srcMatchAttributes,
	NINT		dstNameSpace,
	const void	*dstPath,
	NINT		renameFlags);

STATUS zRootKey(
	NINT	connectionID,
	Key_t	*retRootKey);

STATUS zSetEOF(
	Key_t	key,
	Xid_t	xid,	
	QUAD	startingOffset,
	NINT	flags);

STATUS zSetInheritedRightsMask(
	Key_t	key,
	Xid_t	xid,
	LONG	inheritedRightsMask);

STATUS zUnlockByteRange(
	Key_t	key,
	Xid_t	xid,	
	QUAD	startingOffset,
	QUAD	length);

STATUS zWildRead (
	Key_t		key,
	NINT		characterCode,
	NINT		nameType,
	const void	*pattern,
	NINT		match,
	QUAD		getInfoMask,
	NINT		sizeRetGetInfo,
	NINT		infoVersion,
	zInfo_s		*retGetInfo);

STATUS zWildRewind(
	Key_t	key);

STATUS zWrite(
	Key_t		key,
	Xid_t		xid,	
	QUAD		startingOffset,
	NINT		bytesToWrite,
	const void	*buffer,
	NINT		*retBytesWritten);

/*
 * Helper routines -- these are not true zAPIs but they call zAPIs
 */
BOOL zIsNSSVolume(const utf8_t *path);

STATUS zSetDataSize(
	Xid_t	xid,	
	NINT	connectionID,
	NINT	fileHandleID,
	QUAD	startingOffset,
	NINT	flags);


/*=========================================================================
 *=========================================================================
 *	PSS File System API prototypes
 *=========================================================================
 *=========================================================================*/

STATUS zBrowseUsers(
	NINT connectionID,
	unicode_t *volName,
	NINT numEntriesRequested,
	UserID_t *lastUserReturned,
	zUserRest_s *userEntries,
	NINT *numEntriesReturned);

STATUS zGetNSSVolumeLabel(
	unicode_t 	*volName,
	LONG    	labelSize,
	void    	*label);

STATUS zSetNSSVolumeLabel(
	unicode_t	*volName,
	LONG    	labelSize,
	void    	*label);


#ifdef __cplusplus
}
#endif


#endif /* _ZPUBLICS_H_ */
