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
 | $Modtime:   29 Sep 2003 	$
 |
 | $Workfile:   NEBEvent.c 	$
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Register/Unregister and process NEB events.
 +-------------------------------------------------------------------------*/

/* CLIB Dependencies */
#include <string.h>
#include <nwtypes.h>

/* SMS Dependencies */
#include <nebevent.h>
#include <smstypes.h>
#include <tsalib.h>
#include <smdr.h>
#include <tsaresources.h>
#include <tsaunicode.h>
#include <smsutapi.h>
#include <smstserr.h>
#include <tsaMain.h>
#include <smsdebug.h>
#include <customdebug.h>

/* NEB Dependencies */
#include <neb.h>
#include <nebpub.h>

/* NSS dependencies */
#include <zOmni.h>

/* External defines */
typedef struct EventLVolRenameEnter_s
{
	NINT 		enterExitID;
	unicode_t 	*oldName;
	unicode_t 	*newName;
	VolumeID_t	volID;
	VolumeID_t	poolID;
} EventLVolRenameEnter_s;

typedef struct EventLVolRenameExit_s
{
	NINT 		enterExitID;
	STATUS 		enterRetStatus;
	STATUS 		opRetCode;
	unicode_t 	*volName;    /* final name (new or old based on 
							  * if rename was successful) */
	VolumeID_t	volID;
	VolumeID_t	poolID;
} EventLVolRenameExit_s;

/* Globals */
LONG 		TSA_NEBEntryConsumerID = 0;
LONG 		TSA_NEBExitConsumerID = 0;
TSA_NEB_RENAMER_LIST	*RenamerList = NULL;
TSA_MUTEX	RenamerMutex = NULL;

/* External Globals */
extern UINT32				TSANLMHandle;
extern int	TSALoadStatus;
CCODE	RemoveResourceByID(LONG volID);

#undef  FTYPE
#undef  FNAME
#undef  FPTR
#define FTYPE   TSAINIT
#define FNAME   "TSA_NEBEventRegister"
#define FPTR    TSA_NEBEventRegister
CCODE TSA_NEBEventRegister()
{
	struct ConsumerRegistrationInfo consumerRegInfo;
	CCODE	cCode;

	memset(&consumerRegInfo, 0, sizeof(consumerRegInfo));
	consumerRegInfo.CRIVersion = NEB_CONSUMER_VERSION1;
	consumerRegInfo.CRIConsumerName = "TSAFS.VolumeRenameDetector";
	consumerRegInfo.CRIOwnerID = (struct LoadDefinitionStructure *)TSANLMHandle;
	consumerRegInfo.CRIConsumerESR = TSA_NEBVolumeRenameProducerRegistration;
	consumerRegInfo.CRIConsumerCallback = TSA_NEBVolumeRenameEventEntry;
	consumerRegInfo.CRIConsumerType = SYNCHRONOUS_CONSUMER;
	consumerRegInfo.CRIEventName = "NSS.LVolumeRename.Enter";

	cCode = RegisterConsumer(&consumerRegInfo);
	if(cCode)
		FLogError("RegisterConsumer", cCode, NULL);
	TSA_NEBEntryConsumerID = (LONG)consumerRegInfo.CRIConsumerID;

	memset(&consumerRegInfo, 0, sizeof(consumerRegInfo));
	consumerRegInfo.CRIVersion = NEB_CONSUMER_VERSION1;
	consumerRegInfo.CRIConsumerName = "TSAFS.VolumeRenameDetector";
	consumerRegInfo.CRIOwnerID = (struct LoadDefinitionStructure *)TSANLMHandle;
	consumerRegInfo.CRIConsumerESR = TSA_NEBVolumeRenameProducerRegistration;
	consumerRegInfo.CRIConsumerCallback = TSA_NEBVolumeRenameEventExit;
	consumerRegInfo.CRIConsumerType = SYNCHRONOUS_CONSUMER;
	consumerRegInfo.CRIEventName = "NSS.LVolumeRename.Exit";

	cCode = RegisterConsumer(&consumerRegInfo);
	if(cCode)
		FLogError("RegisterConsumer", cCode, NULL);
	TSA_NEBExitConsumerID = (LONG)consumerRegInfo.CRIConsumerID;

	RenamerMutex = tsaKMutexAlloc("TSAFS - Volume renamer mutex");
	if (!RenamerMutex)
		cCode = NWSMTS_INTERNAL_ERROR;

	return cCode;
}

#undef  FTYPE
#undef  FNAME
#undef  FPTR
#define FTYPE   TSAINIT
#define FNAME   "TSA_NEBEventUnRegister"
#define FPTR    TSA_NEBEventUnRegister
void TSA_NEBEventUnRegister()
{
	CCODE	cCode;
	
	cCode = UnRegisterConsumer((void *)TSA_NEBEntryConsumerID, NULL);	
	if(cCode)
		FLogError("UnRegisterConsumer", cCode, NULL);

	cCode = UnRegisterConsumer((void *)TSA_NEBExitConsumerID, NULL);	
	if(cCode)
		FLogError("UnRegisterConsumer", cCode, NULL);

	if (RenamerMutex)
		tsaKMutexFree(RenamerMutex);
	
	return;
}

/*
 *	This gets called when the PRODUCER registers and unregisters.
 *	If PRODUCER is registered already then this gets call immediately
 *  when the consumer registers.
 */
#undef  FTYPE
#undef  FNAME
#undef  FPTR
#define FTYPE   TSAINIT
#define FNAME   "TSA_NEBVolumeRenameProducerRegistration"
#define FPTR    TSA_NEBVolumeRenameProducerRegistration
void TSA_NEBVolumeRenameProducerRegistration(struct EventBlock *evBlk)
{
	return;
}

#undef  FTYPE
#undef  FNAME
#undef  FPTR
#define FTYPE   TSAINIT
#define FNAME   "TSA_NEBVolumeRenameEventEntry"
#define FPTR    TSA_NEBVolumeRenameEventEntry
void TSA_NEBVolumeRenameEventEntry(		struct EventBlock	*evBlk )
{
	TSA_NEB_RENAMER_LIST	*tmpRenamerNode = NULL, *tmpRList = NULL;
	
	while(TSALoadStatus != TSALOADED && TSALoadStatus != TSAUNLOADED)
	{
	   kYieldThread();
	}
	if(TSALoadStatus == TSAUNLOADED)
	     return;

	tmpRenamerNode = tsaMalloc(sizeof(TSA_NEB_RENAMER_LIST));
	if (!tmpRenamerNode)
		return;
		
	tmpRenamerNode->entryExitID = ((EventLVolRenameEnter_s *)evBlk->EBEventData)->enterExitID;
	unicpy(tmpRenamerNode->oldName, ((EventLVolRenameEnter_s *)evBlk->EBEventData)->oldName);
	tmpRenamerNode->next = NULL;

	if (RenamerMutex)
		tsaKMutexLock(RenamerMutex);
	if (RenamerList == NULL)
		RenamerList = tmpRenamerNode;
	else
	{
		for (tmpRList = RenamerList; tmpRList->next != NULL; tmpRList = tmpRList->next);
		tmpRList->next = tmpRenamerNode;
	}
	if (RenamerMutex)
		tsaKMutexUnlock(RenamerMutex);

	return;
}

#undef  FTYPE
#undef  FNAME
#undef  FPTR
#define FTYPE   TSAINIT
#define FNAME   "TSA_NEBVolumeRenameEventExit"
#define FPTR    TSA_NEBVolumeRenameEventExit
void TSA_NEBVolumeRenameEventExit(struct EventBlock	*evBlk)
{
	TSA_NEB_RENAMER_LIST	*prevNode = NULL, *tmpRList = NULL;
	EventLVolRenameExit_s *	 renameEvent = (EventLVolRenameExit_s *)evBlk->EBEventData;
	RESOURCE_NODE			*tmpResNode = NULL;
	unsigned char 			*tmpCharPtr;
	UINT32					 actLen;
	unicode					*tmpUniName = NULL, *tmpNewName = NULL;
	unicode					 sep1[UNI_SEP_BASE_SIZE];
	
	while(TSALoadStatus != TSALOADED && TSALoadStatus != TSAUNLOADED)
	{
	   kYieldThread();
	}
	if(TSALoadStatus == TSAUNLOADED)
	     return;

	tsaKMutexLock(RenamerMutex);
	tmpRList = RenamerList;
	while (tmpRList)
	{
		if (tmpRList->entryExitID == renameEvent->enterExitID)
			break;
		prevNode = tmpRList;
		tmpRList = tmpRList->next;
	}

	if (tmpRList)
	{
		if (prevNode == NULL)
			RenamerList = RenamerList->next;
		else
			prevNode->next = tmpRList->next;
	}
	tsaKMutexUnlock(RenamerMutex);
	
	if (tmpRList != NULL)
	{
		/* The volume names do not have terminating COLON, add these before modifying the resource lists */
		GetUniNameSpaceSeparators(DOSNameSpace, sep1, NULL);
		tmpUniName = (unicode *)tsaMalloc((unilen(tmpRList->oldName) + unilen(sep1) + 1) * sizeof(unicode));
		if (!tmpUniName)
			goto FreeNode;
		unicpy(tmpUniName, tmpRList->oldName);
		unicat(tmpUniName, sep1);

		tmpNewName= (unicode *)tsaMalloc((unilen(renameEvent->volName) + unilen(sep1) + 1) * sizeof(unicode));
		if (!tmpNewName)
			goto FreeNode;
		unicpy(tmpNewName, renameEvent->volName);
		unicat(tmpNewName, sep1);
		
		if (LockResourceList())
			goto FreeNode;
		
		tmpResNode = FindResourceByUniName(tmpUniName);
		if (tmpResNode)
		{
			unicpy(tmpResNode->uniResName, tmpNewName);
			tmpCharPtr = tmpResNode->resName;
			actLen = MAX_RESOURCE_NAME_BYTES;
			if (SMS_UnicodeToByte(tmpResNode->uniResName, &actLen, &tmpCharPtr, NULL))
			{
				UnlockResourceList();
				RemoveResourceByID(tmpResNode->resID);
				goto FreeNode;
			}
		}
		UnlockResourceList();
	}

FreeNode:
	if (tmpRList)
		tsaFree(tmpRList);

	if (tmpNewName)
		tsaFree(tmpNewName);

	if (tmpUniName)
		tsaFree(tmpUniName);
	
	return;
}
