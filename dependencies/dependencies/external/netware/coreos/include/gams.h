/*****************************************************************************
 *
 *  (C) Copyright 1997-1998 Novell, Inc.
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

#if ! defined(GAMS_H)
#define GAMS_H

#include <dtypes.h>

#define GAMS_NO_MORE_ITERATIONS -1


/***********************************************************************/
/* Human Readable Label	classes													  */
/***********************************************************************/

#define NWMA_FULL_LABEL_HRL	0x00000000

/***********************************************************************/
/* Human Readable Label	typedefs									   */
/***********************************************************************/

/* Length MAX for Human Readable	Labels */ 
#define	NWMA_MAX_LABELNAME_LEN 	31
#define	NWMA_MAX_LABELNAME_BYTES (NWMA_MAX_LABELNAME_LEN+1) * sizeof(unicode)

/*typedef struct NWMAHRL
{
    uint32 labelType;
    unicode labelName[NWMA_MAX_LABELNAME_LEN+1];
} NWMAHRL;*/

typedef struct NWMACharHRL
{
    uint32 labelType;
    char labelName[NWMA_MAX_LABELNAME_LEN+1];
} NWMACharHRL;

/***********************************************************************/
/* HRL Clearance Range typedefs											        */ 
/***********************************************************************/

/*typedef struct NWMARangeHRL
{
    uint32	rangeType;
    NWMAHRL readLabel;
    NWMAHRL writeLabel;
} NWMARangeHRL;*/

typedef struct NWMACharRangeHRL
{
    uint32	rangeType;
    NWMACharHRL readLabel;
    NWMACharHRL writeLabel;
} NWMACharRangeHRL;

/***********************************************************************/
/* Domain Policy Configuration Structures									     */ 
/***********************************************************************/
#define NWMA_POLICY_VERSION_1 1

typedef struct NWMADomainPolicy
{
	uint32 version;	/* IN/OUT -- NWMA_POLICY_VERSION*/
	uint32 policyNum;
	uint32 minAPISendFnshAuthVer;
	uint32 minAPIRecvFnshAuthVer;
	uint32 loginDir_OK;
	uint32 changableLbls;
	uint32 refreshTime;
	NWMARangeHRL minOperatorRange;
	NWMARangeHRL authRange;
	NWMARangeHRL defaultRange;
	NWMARangeHRL unauthenticatedRange;
	NWMAHRL volumeLabel;
	NWMAHRL loginDirectoryLabel;
	NWMAHRL partitionPublicLabel;
	NWMAHRL partitionProtectedLabel;
} NWMADomainPolicy;

typedef struct NWMACharDomainPolicy
{
	uint32 version;	/* IN/OUT -- NWMA_POLICY_VERSION*/
	uint32 policyNum;
	uint32 minAPISendFnshAuthVer;
	uint32 minAPIRecvFnshAuthVer;
	uint32 loginDir_OK;
	uint32 changableLbls;
	uint32 refreshTime;
	NWMACharRangeHRL minOperatorRange;
	NWMACharRangeHRL authRange;
	NWMACharRangeHRL defaultRange;
	NWMACharRangeHRL unauthenticatedRange;
	NWMACharHRL volumeLabel;
	NWMACharHRL loginDirectoryLabel;
	NWMACharHRL partitionPublicLabel;
	NWMACharHRL partitionProtectedLabel;
} NWMACharDomainPolicy;

#endif
/****************************************************************************/
/****************************************************************************/
