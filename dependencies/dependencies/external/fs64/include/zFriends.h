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
 | $Modtime:   09 May 2001 12:36:06  $
 |
 | $Workfile:   zFriends.h  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Function prototypes for NLMs friendly to NSS.
 +-------------------------------------------------------------------------*/
#ifndef _ZFRIENDS_H_
#define _ZFRIENDS_H_

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

STATUS zChangeConnection(
	Key_t	key,
	NINT	connectionID,
	NINT	taskID,
	Key_t	*retKey);

STATUS zOpenByZid(
	Key_t	key,
	NINT	taskID,
	Zid_t	zid,
	NINT	requestedRights,
	Key_t	*retKey);

	/*
	 * The following two routines can be used to convert
	 * to/from traditional 32 bit ids and the NDS GUID for
	 * trustees and creators of files.
	 */
STATUS xIdToGuid(LONG id, NDSid_t *guid);
STATUS xGuidToId(NDSid_t *guid, LONG *idp);

	/*
	 * Convert to/from utc/dos time.
	 */
DOSTime_t xUTC2dosTime(Time_t utc);
Time_t xDOS2utcTime(DOSTime_t dosTime);

STATUS xPoolGUIDToName(
		VolumeID_t  *poolID, 
		unicode_t   *poolName,
		NINT        poolNameSizeInUnicode);

STATUS xPoolNameToGUID(
		unicode_t   *poolName,
		VolumeID_t  *poolID);

STATUS xPoolGetEnabledFeatures(
		VolumeID_t	*poolID, 
		QUAD		*enabledFeatures );

STATUS xVolumeGUIDToName(
		VolumeID_t  *volumeID, 
		unicode_t   *volumeName,
		NINT        volumeNameSizeInUnicode);

STATUS xVolumeNameToGUID(
		unicode_t   *volumeName,
		VolumeID_t  *volumeID);

	/*
	 * The following two routines are for clustering to use when forcing a
	 * volume to upgrade.
	 */
STATUS xForceVolumeUpgrade (
	unicode_t *volName,
	STATUS (*callBack)(NINT processedCount, NINT updatedCount),
	NINT *processedCount,
	NINT *upgradedCount);
BOOL xIsVolumeUpgraded(unicode_t *volName);

	/*
	 * These routines are used to get/give pages of memory to NSS
	 */
void *xRequestPage(void);	/* non-blocking */
void *xRequestContiguousPages(NINT numPages);	/* non-blocking */
void xReturnPage(void *page);	/* blocking - not implemented */
void xReturnContiguousPages(void *pages, NINT numPages);	/* blocking - not implemented */

NINT xAvailableCacheBuffers(void);
NINT xOldestCacheBuffer(void);


#ifdef __cplusplus
}
#endif

#pragma pack(pop)

#endif
