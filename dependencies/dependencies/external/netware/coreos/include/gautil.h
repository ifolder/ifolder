/*****************************************************************************
 *
 *  (C) Copyright 1996-1998 Novell, Inc.
 *  All Rights Reserved.
 *
 *  This program is an unpublished copyrighted work which is proprietary
 *  to Novell, Inc. and contains confidential information that is not
 *  to be reproduced or disclosed to any other person or entity without
 *  prior written consent from Novell, Inc. in each and every instance.
 *
 *  WARNING:  Unauthorized reproduction of this program as well as
 *  unauthorized preparation of derivative works based upon the
 *  program or distribution of copies by sale, rental, lease or
 *  lending are violations of federal copyright laws and state trade
 *  secret laws, punishable by civil and criminal penalties.
 *
 ****************************************************************************/

#ifndef __GAUTIL_H
#define __GAUTIL_H

#include <dtypes.h>
#include <gaapi.h>
#include <gams.h>

#define GAUTIL_VERSION 16
#define GAUTIL_COUNT 18
typedef struct GAMS_GAUtilServiceStructure 
{ 
	uint32 version;		/* Version number    */
	uint32 count;		/* Number of entries */
	/* 1 */
	uint32 (*SetPartitionLabel)(uint32 connID, unicode *tree, unicode *partName, NWMAHRL *pubLClassp, NWMAHRL *protLClassp);
	/* 2 */
	uint32 (*SetPartitionPropLabel)(uint32 connID, unicode *tree, unicode *objName, NWMAHRL *pubLClassp, NWMAHRL *protLClassp);
	/* 3 */
	uint32 (*GetPartitionPropLabel)(uint32 connID, unicode *tree, unicode *objName, NWMAHRL *pubLClassp, NWMAHRL *protLClassp);
	/* 4 */
	uint32 (*RemovePartitionPropLabel)(uint32 connID, unicode *tree, unicode *objName);
	/* 5 */
	uint32 (*GetObjectLabel)(uint32 connID, unicode *tree, unicode *objName, NWMAHRL *pubLClassp, NWMAHRL *protLClassp);
	/* 6 */
	uint32 (*SetVolumeLabel)(uint32 connID, unicode *volName, NWMAHRL *class);
	/* 7 */
	uint32 (*GetVolumeLabel)(uint32 connID, unicode *volName, NWMAHRL *class);
	/* 8 */
	uint32 (*GetConnRange)(uint32 connID, uint32 targetConnectionID, NWMARangeHRL *rangep);
	/* 9 */
	uint32 (*SetAuthRange)(uint32 connID, unicode *tree, unicode *objName, NWMARangeHRL *newRange);
	/*10*/
	uint32 (*SetDefaultRange)(uint32 connID, unicode *tree, unicode *objName, NWMARangeHRL *defaultRange);
	/*11*/
	uint32 (*DeleteAuthRange)(uint32 connID, unicode *tree, unicode *objName, NWMARangeHRL *delRange);
	/*12*/
	uint32 (*ResetAuthRanges)(uint32 connID, unicode *tree, unicode *objName);
	/*13*/
	uint32 (*GetDefaultRange)(uint32 connID, unicode *tree, unicode *objName, uint32 *defFlag, NWMARangeHRL *defRp);
	/*14*/
	uint32 (*ScanAuthRange)(uint32 connID, unicode *tree, unicode *objName, uint32 *defFlag, uint32 *iter,
		                     NWMARangeHRL *defRp);
	/*15*/
	uint32 (*SetDomainPolicy)(uint32 connID, unicode *tree, unicode *masvDN, NWMADomainPolicy *policy);
	/*16*/
	uint32 (*GetDomainPolicy)(uint32 connID, unicode *tree, unicode *masvDN, NWMADomainPolicy *policy);
	/*17*/
	uint32 (*CreateMASVObject)(uint32 connID, unicode *tree, unicode *securityDN, unicode *masvRDN);
	/*18*/
	uint32 (*ScanHRLs)(uint32 *iter, uint32 HRLClass, uint32 maxCount, uint32 *actualCount,
							 NWMAHRL *HRLs);
	/*19*/
	uint32 (*RequestClearance)(uint32 connID, unicode *tree, unicode *objName, uint32 authGrade, uint32 reqType, uint32 requested,
										NWMARangeHRL *clrRequest, uint32 *dataLen, void *data);

} GAMS_GAUtilService; 

extern GAMS_GAUtilService gacfg;

GAMSAPI uint32 GAMS_GAUtilRegisterService(void *key, uint32 version, GAMS_GAUtilService *gams, uint32 *id);

GAMSAPI uint32 GAMS_GAUtilDeregisterService(uint32 id);

#endif
/****************************************************************************/
/****************************************************************************/
