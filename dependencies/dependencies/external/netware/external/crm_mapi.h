/*****************************************************************************
 *
 *	(C) Copyright 1998 Novell, Inc.
 *	All Rights Reserved.
 *
 *	This program is an unpublished copyrighted work which is proprietary
 *	to Novell, Inc. and contains confidential information that is not
 *	to be reproduced or disclosed to any other person or entity without
 *	prior written consent from Novell, Inc. in each and every instance.
 *
 *	WARNING:  Unauthorized reproduction of this program as well as
 *	unauthorized preparation of derivative works based upon the
 *	program or distribution of copies by sale, rental, lease or
 *	lending are violations of federal copyright laws and state trade
 *	secret laws, punishable by civil and criminal penalties.
 *
 *  $Workfile:  crm_mapi.h
 *  $Modtime: $
 *  $Revision$
 *
 ****************************************************************************/

#ifndef _CRM_MAPI_H
#define _CRM_MAPI_H
#include "enable.h"

/*
	Cluster Resource Manager (CRM) Management API (CRM_MAPI)

	This header file defines the public management interface
	to the Cluster Resource Manager. It contains APIs to be used
	be an external Management Agent. The Management Agent must
	provide a callback function when it registers with the
	Cluster Resource Manager. It will be called by the Cluster
	Resource Manager in each of the following scenarios:

	1) A Cluster Resource changed state.
	2) A Cluster Resource requires administrative intervention
	   to confirm either a start, failover or failback.

	The following management functions are provided by the CRM

	CRM_MAPI_STATE			Report cluster resource state
	CRM_MAPI_MIGRATE		Migrate a cluster resource
	CRM_MAPI_ONLINE			Online a cluster resource
	CRM_MAPI_OFFLINE		Offline a cluster resource
	CRM_MAPI_ALERT			Alert notification & response
*/

#include <portable.h>

/*****************************************************************************
	Definitions
******************************************************************************/

#define CRM_MAX_RESNAME_LENGTH	63

/*****************************************************************************
	Datatypes
******************************************************************************/

/* Return codes */
typedef enum _CRM_RETURN {
	CRM_SUCCESS,					/* Normal successful completion */
	CRM_ERROR_INVALID_HANDLE,		/* An invalid handle was supplied */
	CRM_ERROR_INVALID_PARAMETER,	/* An invalid parameter was supplied */
	CRM_FAILURE,
	CRM_BUSY,						/* Retry the MAPI                   */
	CRM_ERROR_TOO_MANY_HANDLES
} CRM_RETURN;

/* Resource states */
typedef enum _CRM_STATE {
	CRM_STATE_OFFLINE		= 1,
	CRM_STATE_COMATOSE		= 2,
	CRM_STATE_LOADING		= 3,
	CRM_STATE_UNLOADING		= 4,
	CRM_STATE_RUNNING		= 5,
	CRM_STATE_ALERT			= 6,
	CRM_STATE_UNASSIGNED	= 7,
	CRM_STATE_QUORUM_WAIT	= 8,
	CRM_STATE_NDS_SYNC		= 9
} CRM_STATE;

/* Resource alerts */
typedef enum _CRM_ALERT {
	CRM_ALERT_ACK		= 0,	/* Acknowledge this alert */
	CRM_ALERT_START		= 1,	/* Start resource? or nak */
	CRM_ALERT_FAILOVER	= 2,	/* Failover resource? or nak */
	CRM_ALERT_FAILBACK	= 3		/* Failback resource? or nak */
} CRM_ALERT;

/* Supported APIs */
typedef enum _CRM_MAPI {
	CRM_MAPI_STATE		= 1,	/* Report cluster resource state */
	CRM_MAPI_MIGRATE	= 2,	/* Migrate a cluster resource */
	CRM_MAPI_ONLINE		= 3,	/* Online a cluster resource */
	CRM_MAPI_OFFLINE	= 4,	/* Offline a cluster resource */
	CRM_MAPI_ALERT		= 5,	/* Alert notification & response */
    CRM_MAPI_CHECK      = 6     /* Check for valid resource name */
} CRM_MAPI;

/* Supported APIs */
typedef enum _CRM_PRI_MAPI {
	CRM_MAPI_GET_PRI_VERSION= 1,/* Get priority list version number */
	CRM_MAPI_SET_PRI_VERSION= 2,/* Set priority list version number */
} CRM_PRI_MAPI;

/* Cluster resource */
typedef struct _CRM_RESOURCE {
	STR			name[CRM_MAX_RESNAME_LENGTH + 1]; /* Resource name */

	UINT32		incarnation;	/* Resource incarnation number */
	CRM_STATE	state;			/* Resource state, as above */
	CRM_ALERT	alert;			/* Resource alert, as above */
	UINT32		location;		/* Node number: current location */

	UINT32		fromTo;			/* Node number: from / to location */
	UINT32		revision;		/* Unique resource revision number */
    UINT32      res_fail_cnt;   /* This counts each time resource has failed  */
	BYTE        dateAndTime[EN_DATE_TIME_LEN+1];/* Resource's up since when */

} CRM_RESOURCE;

#define CRM_UNASSIGNED	(-1)	/* node number: unassigned location */

/* CRM handle */
typedef UINT32	CRM_HANDLE;		/* CRM supplied handle */

/*****************************************************************************
    Prototypes
******************************************************************************/

	CRM_RETURN
CrmRegister(
	UINT32 (*Callback)(CRM_MAPI, CRM_RESOURCE*,  void *cxt), /* IN: callback function */
	CRM_HANDLE		*crmHandle);	/* OUT: CRM supplied handle */

	CRM_RETURN
CrmDeregister(
	CRM_HANDLE		crmHandle);		/* IN: CRM supplied handle */

	CRM_RETURN
CrmResourceApi(
	CRM_HANDLE		crmHandle,		/* IN: CRM supplied handle */
	CRM_MAPI		crmMapi,		/* IN: CRM API function code */
	CRM_RESOURCE	*resource,		/* INOUT: cluster resource */
	void            *cxt);          /* INOUT: Context for the Callback routine*/

	CRM_RETURN
CrmResourcePriorityApi(
	CRM_HANDLE		crmHandle,		/* IN: CRM supplied handle */
	CRM_PRI_MAPI	crmMapi,		/* IN: CRM GET/SET priority list */
	UINT32	 		*version);		/* INOUT: Version of the priority list */

/* Only this API can be used for RING3 app's                                  */
/* The caller must specify the resource name in the resource data structure   */
CrmGetResourceState(
	CRM_RESOURCE	*resource);		/* INOUT: cluster resource */


#endif /* _CRM_MAPI_H */
