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

#if ! defined(MASV_H)
#define MASV_H

#include <dtypes.h>

#define MASV_NCP_VERB	91

/************************************************************************/
/* Rights (returned	by Test	Access Rights functions						*/
/************************************************************************/
#define	NWMA_NONE	   0
#define	NWMA_R		   1
#define	NWMA_W		   2
#define	NWMA_RW		   (NWMA_R|NWMA_W) 

/***********************************************************************/
/* Graded Authentication Types 							 		              */ 
/***********************************************************************/
#define	NWMA_GA_SI			-1	/* Internal	use	-- Resevered */
#define	NWMA_GA_NONE		0
#define	NWMA_GA_NV4	 		1
#define	NWMA_GA_SSL_CERT	2
#define	NWMA_GA_WEAK		3
#define	NWMA_GA_N3			4	/* N3 Authentication type (Via Bindery calls)*/
#define	NWMA_GA_UNAUTH		5	/* No Credentials Authentication type */
#define	NWMA_GA_V0AUTH		6	/* Version 0 Authentication	Type */

/***********************************************************************/
/* Clearance Request values											   */ 
/***********************************************************************/
#define	NWMA_REQ_UNKNOWN	-1
#define	NWMA_REQ_DEFAULT 	0	/* Highest possible	based on user default*/
#define	NWMA_REQ_REQUIRED	1	/* User	supplied clearance only	if approved*/
#define	NWMA_REQ_HIGHEST	2	/* Highest possible	based on user auth clearances*/

/***********************************************************************/
/* Range/Clearance typedefs											   */ 
/***********************************************************************/

#define	NWMA_RNG_UNKNOWN	-1 /* Used for input to have masv determine */		
#define	NWMA_RNG_SINGLE_LEVEL	0
#define	NWMA_RNG_TRUST_RANGE	1

/***********************************************************************/
/* Human Readable Label	typedefs									   */
/***********************************************************************/

/* Length MAX for Human Readable	Labels */ 
#define	NWMA_MAX_LABELNAME_LEN 	31
#define	NWMA_MAX_LABELNAME_BYTES (NWMA_MAX_LABELNAME_LEN+1) * sizeof(unicode)

/*Label	Types	  */ 
#define	NWMA_LABEL_TYPE_UNKNOWN	-1 /* Init value used for input	*/
#define	NWMA_LABEL_TYPE_IDL		0
#define	NWMA_LABEL_TYPE_V1 	1

typedef struct NWMAHRL
{
    uint32 labelType;
    unicode labelName[NWMA_MAX_LABELNAME_LEN+1];
} NWMAHRL;

/***********************************************************************/
/* HRL Clearance Range typedefs											        */ 
/***********************************************************************/

#define	MA_RNG_UNKNOWN			-1 /* Used for input to have masv determine */		
#define	MA_RNG_SINGLE_LEVEL	0
#define	MA_RNG_TRUST_RANGE	1

typedef	struct NWMARangeHRL
{
    uint32	rangeType;
    NWMAHRL readLabel;
    NWMAHRL writeLabel;
} NWMARangeHRL;

/***********************************************************************/
/* Policy Configuration Structure									           */ 
/***********************************************************************/
#define NWMA_POLICY_VERSION_1 1

typedef struct NWMAPolicyV1
{
    uint8 version;	/* IN/OUT -- NWMA_POLICY_VERSION*/
    uint8 minAPISendFnshAuthVer;
    uint8 minAPIRecvFnshAuthVer;
    uint8 loginDir_OK;
    uint8 setServerAdmin;
	uint32 policyNum;
    NWMARangeHRL	minOperatorRange;
    NWMARangeHRL	defaultAuthRange;
    NWMARangeHRL	defaultRange;
    NWMARangeHRL	unauthenticatedRange;
    NWMAHRL defaultVolumeLabel;
    NWMAHRL loginDirectoryLabel;
    NWMAHRL defaultPartitionLabel;
    NWMAHRL defaultPartitionPublicLabel;
} NWMAPolicyV1;	

/***********************************************************************/
/* MASV NCP Structures									                       */ 
/***********************************************************************/
typedef struct
{
	uint8 version;
	uint8 volumeNameLength;
	uint8 volumeName[1];		// length preceeded
	NWMAHRL volLabel;
}SetVolumeLabelRequest;

typedef struct
{
	uint8 version;
	uint8 activeLabel;
	uint8 volumeNameLength;
	uint8 volumeName[1];		// length preceeded
}GetVolumeLabelRequest;

typedef struct
{
	NWMAHRL volLabel;
}GetVolumeLabelReply;

typedef struct
{
	uint8 version;
	uint32 targetConnID;
}GetConnRangeRequest;

typedef struct
{
	NWMARangeHRL range;
}GetConnRangeReply;

typedef struct
{
	uint8 version;
	NWMAHRL publicLabel;
	NWMAHRL protectedLabel;
	unicode objName[1];
}SetPartLabelRequest;

typedef struct
{
	uint8 version;
	unicode objName[1];
}RemovePartitionPropLabelRequest;

typedef struct
{
	uint8 version;
	unicode objName[1];
}GetObjectLabelRequest;

typedef struct
{
	NWMAHRL publicLabel; 
	NWMAHRL protectedLabel;
}GetObjectLabelReply;

typedef struct
{
	uint8 version;
	uint32 reqType;
	NWMARangeHRL connRange;
	uint32 gaType;
	uint32 credentialSize;
	uint8 credential[1];
	unicode objName[1];
}RequestClearanceRequest;

typedef struct
{
	uint32 flags;
	uint32 credentialSize;
	uint8 credential[1];
}RequestClearanceReply;

typedef struct
{
	uint8 version;
	NWMARangeHRL authRange;
	unicode objName[1];
}SetRangeRequest;

typedef struct
{
	uint8 version;
	unicode objName[1];
}ResetRangeRequest;

typedef struct
{
	uint8 version;
	unicode objName[1];
}GetDefaultRangeRequest;

typedef struct
{
	uint32 labelVersion;
	NWMARangeHRL defaultRange;
}GetDefaultRangeReply;

typedef struct
{
	uint8 version;
	uint32 iterationHandle;
	unicode objName[1];
}ScanAuthRangeRequest;

typedef struct
{
	uint32 labelVersion;
	uint32 iterationHandle;
	NWMARangeHRL authorizationRange;
}ScanAuthRangeReply;

typedef struct
{
	uint8 version;
	NWMAPolicyV1 policy;
}SetPolicyRequest;

typedef struct
{
	uint8 version;
}GetPolicyRequest;

typedef struct
{
	NWMAPolicyV1 policy;
}GetPolicyReply;

#endif
/****************************************************************************/
/****************************************************************************/
