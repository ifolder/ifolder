/****************************************************************************
 | Copyright (c) 2001 Novell, Inc. All Rights Reserved.
 |
 | This work is subject to U.S. and international copyright laws and
 | treaties. Use and redistribution of this work is subject to the
 | license agreement accompanying the software development kit (SDK)
 | that contains this work.  Pursuant to the SDK license agreement,
 | Novell hereby grants to developer a royalty-free, non-exclusive
 | license to include Novell's sample code in its product. Novell
 | grants developer worldwide distribution rights to market, distribute,
 | or sell Novell's sample code as a component of developer's products.
 | Novell shall have no obligations to developer or developer's customers
 | with respect to this code.
 |
 |***************************************************************************
 |
 |	 NetWare Advance File Services (NSS) module
 |
 |---------------------------------------------------------------------------
 |
 | $Author$
 | $Modtime:   07 Jan 2003 08:02:20  $
 |
 | $Workfile:   zPublics.h  $
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

#pragma pack(push,1)


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

STATUS zEnumerate(
	Key_t		key,
	QUAD		cookie,
	NINT		nameType,
	NINT		match,
	QUAD		getInfoMask,
	NINT		sizeRetGetInfo,
	NINT		infoVersion,
	void		*retGetInfo,
	QUAD		*retCookie);

STATUS zFlush(
	Key_t	key);

STATUS	zGetEffectiveRights(Key_t key, 
	GUID_t objId, 
	NINT *effectiveRights);

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
	void	*retGetInfo);

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
	const void	*modifyInfo);

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
	void		*retGetInfo);

STATUS zWildRewind(
	Key_t	key);

STATUS zWrite(
	Key_t		key,
	Xid_t		xid,	
	QUAD		startingOffset,
	NINT		bytesToWrite,
	const void	*buffer,
	NINT		*retBytesWritten);

STATUS zZIDDelete(
	Key_t		key,
	Xid_t		xid,
	VolumeID_t	*volumeID,
	Zid_t		zid,
	NINT		deleteFlags);

STATUS zZIDOpen(
	Key_t		key,
	NINT		taskID,
	NINT		nameSpace,
	VolumeID_t	*volumeID,
	Zid_t		zid,
	NINT		requestedRights,
	Key_t		*retKey);

STATUS zZIDRename(
	Key_t		key,
	Xid_t		xid,
	VolumeID_t	*srcVolumeID,
	Zid_t		srcZid,
	NINT		dstNameSpace,
	const void	*dstPath,
	NINT		renameFlags);

/*
 * Helper routines -- these are not true zAPIs but they call zAPIs
 */
BOOL zIsNSSVolume(const utf8_t *path);

STATUS zSetDataSize(
	SLONG	unused,	
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

#pragma pack(pop)

#endif /* _ZPUBLICS_H_ */
